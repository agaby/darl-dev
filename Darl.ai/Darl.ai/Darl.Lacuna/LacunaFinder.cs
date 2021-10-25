using System;
//using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using DarlCompiler.Parsing;
using DarlLanguage;
using DarlLanguage.Processing;

namespace Darl.Lacuna
{
	/// <summary>
	/// Finds gaps and defects in rule sets.
	/// </summary>
	public class LacunaFinder
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public LacunaFinder()
		{
			ruleSetSet = false;
			maxArity = 10;
			nullCategories = false;
			maxGenerations = 50;
			population = 50;
			_mutation = 0.01;
		}
		/// <summary>
		/// Gets or sets the rule set to be analyzed.
		/// </summary>
		public string ruleSet
		{
			set
			{
				_ruleSet = value;
				ruleSetSet = true;
			}
			get
			{
				return _ruleSet;
			}
		}
		private string _ruleSet;
		/// <summary>
		/// If true, GA uses the same random seed each run.
		/// </summary>
		/// <remarks>Ensures repeatability for unit testing</remarks>
		public bool fixedRandomSeed
		{
			get
			{
				return _fixedRandomSeed;
			}
			set
			{
				_fixedRandomSeed = value;
			}
		}
		private bool _fixedRandomSeed = false;

        private List<List<int>> clusters = new List<List<int>>();
		private bool ruleSetSet;
		/// <summary>
		/// Maximum range of arity to be tested.
		/// </summary>
		/// <remarks>Default is 10; arity is >= 0 and unbounded.</remarks>
		public int maxArity;
		/// <summary>
		/// Consider if behavior testing should cover null/unknown categories. 
		/// </summary>
		/// <remarks>Frequently category lists may not cover all the categories that may be experienced in practice.</remarks>
		public bool nullCategories;
		/// <summary>
		/// Determines if rule contention is permissible;
		/// </summary>
		/// <remarks>Enforces strict non-contention. Fuzzy rule sets typically cope with. and benefit from contention, 
		/// but some rule based systems avoid it at all costs.
		/// Clearly some forms of contention don't make sense.</remarks>
		/// <example>if a is true then b will be red
		/// if a is true then b will be green.</example>
		public bool noContention;
		/// <summary>
		/// Population of solutions.
		/// </summary>
		/// <remarks>Default is 50.</remarks>
		public int population;
		/// <summary>
		/// Number of generations to run for.
		/// </summary>
		/// <remarks>Default is 50.</remarks>
		public int maxGenerations;
		/// <summary>
		/// Gets or sets the mutation rate (0,1)
		/// </summary>
		public double mutation
		{
			get
			{
				return _mutation;
			}
			set
			{
				if(mutation > 0.8)
					throw new ArgumentOutOfRangeException("Mutation rate > 0.8!");
				if(mutation < 0.0)
                    throw new ArgumentOutOfRangeException("Mutation rate < 0.0!");
				_mutation = value;
			}
		}
		private double _mutation;

		private List<InputInterface> inputList;
		private DarlRunTime rule = new DarlRunTime();
        private ParseTree tree;
		/// <summary>
		/// Used to hold the current output name.
		/// </summary>
		private string outputName;


		/// <summary>
		/// Starts the process of finding lacunae.
		/// Returns immediately
		/// </summary>
		/// <param name="ruleSet">The rule set to find lacunae in.</param>
		public async Task<LacunaReport> Find(string ruleSet)
		{
			this._ruleSet = ruleSet;
			this.ruleSetSet = true;
			return await Find();	
		}
		/// <summary>
		/// Starts the process of finding lacunae.
		/// </summary>
		/// <remarks>Uses a preloaded rule set.</remarks>
		public async Task<LacunaReport> Find()
		{
			LacunaReport report = new LacunaReport();
			if(!ruleSetSet)
				throw new ArgumentNullException("LacunaFinder: Ruleset not set.");
			clusters.Clear();
			inputList = new List<InputInterface>();
            var runtime = new DarlRunTime();
            tree = runtime.CreateTree(_ruleSet);
			//open the rule set

            foreach(var inp in tree.GetMapInputs())
            {
                var name = inp.Name;
                InputInterface inputInf = new InputInterface(tree.GetMapInputType(name))
                {
                    name = name
                };
                inputList.Add(inputInf);
                switch(inputInf.type)
                {
                    case InputInterface.DataType.numeric:
                        var range = tree.GetMapPracticalInputRange(name);
                        inputInf.lowerBound = (double)range.values.First();
                        inputInf.upperBound = (double)range.values.Last();
                        break;
                    case InputInterface.DataType.categorical:
                        foreach(var c in tree.GetMapInputCategories(name))
                        {
                            inputInf.AddCategory(c);
                        }
                        if (nullCategories)
                        {
                            inputInf.AddCategory("nullcategory");
                        }
                        break;
                }
            }
            var outputs = tree.GetMapOutputs();
            report.outputs = new OutputReport[outputs.Count];
            report.date = DateTime.Now;
            int outputIndex = 0;
            foreach (var output in outputs)
            {
                //Search independently for each output
                outputName = output.Name;
                DifferentialEvolution diffEv = new DifferentialEvolution();
                if (_fixedRandomSeed)
                    DifferentialEvolution.random = new Random(123456);
                diffEv.D = inputList.Count;
                diffEv.NP = 400;
                diffEv.CR = 0.95;
                diffEv.K = 0.85;
                diffEv.F = 0.85;
                diffEv.Gmax = 100;
                diffEv.sigma = 2.0;
                diffEv.hi = new double[inputList.Count];
                diffEv.lo = new double[inputList.Count];
                int ind = 0;
                foreach (InputInterface inp in inputList)
                {
                    diffEv.lo[ind] = inp.lowerBound;
                    diffEv.hi[ind] = inp.upperBound;
                    ind++;
                }
                //pass the evaluation function
                double[,] results = await diffEv.Evolve(this);
                report.outputs[outputIndex] = new OutputReport
                {
                    name = outputName,
                    lacunae = await FindClusters(results, diffEv.rawcost, diffEv.tree)
                };
                outputIndex++;
            }
			return report;
		}
		/// <summary>
		/// Evaluate the rule set on the test values 
		/// </summary>
		/// <param name="values">values created by Differential Evolution</param>
		/// <returns>The confidence value associated with those values.</returns>
		public async Task<double> Evaluate(double[] values)
		{
			int index = 0;
            var dict = new List<DarlResult>();
			foreach(InputInterface inp in inputList)
			{
                dict.Add( new DarlResult(inp.name, inp.Convert(values[index]),(DarlResult.DataType)Enum.Parse(typeof(DarlResult.DataType),inp.type.ToString())));
				index++;
			}
			var res =  await rule.Evaluate(tree, dict);
            return res.First(a => a.name == outputName).GetWeight();
        }



		private async Task<SingleLacuna []> FindClusters(double[,] results, double[] costs, KDTree tree)
		{
			double[] point = new double[inputList.Count];
			for(int example = 0; example < results.GetLength(0); example++)
			{
				if(costs[example] >= 0.5)//ignore minima above 0.5 confidence
					continue;
				for(int n = 0; n < inputList.Count; n++)
					point[n] = results[example,n];
				bool found = false;
				foreach(List<int> cluster in clusters)
				{
					found = true;
					foreach(int candidate in cluster)
					{
						double [] refpoint = new double[inputList.Count];
						for(int p = 0; p < inputList.Count; p++)
							refpoint[p] = results[candidate,p];

						double cost = await EvaluateMidPoint(point,refpoint);
						if(cost > Math.Max(costs[example],costs[candidate]))
						{
							found = false;
							break;
						}
					}
					if(found)
					{
						cluster.Add(example);
						break;
					}
				}
				if(!found)
				{
                    List<int> cluster = new List<int>
                    {
                        example
                    };
                    clusters.Add(cluster);
				}				
			}
			//now determine the bounds of the clusters
			SingleLacuna[] result = new SingleLacuna[clusters.Count];
			int clusterCount = 0;
			foreach(List<int> cluster in clusters)
			{
				double minCost = double.MaxValue;
				double[] maxbound = new double[inputList.Count];
				for(int n = 0; n < inputList.Count; n++)
					maxbound[n] = double.MinValue;
				double[] minbound = new double[inputList.Count];
				for(int n = 0; n < inputList.Count; n++)
					minbound[n] = double.MaxValue;
				foreach(int p in cluster)
				{
					for(int n = 0; n < inputList.Count; n++)
					{
						maxbound[n] = Math.Max(maxbound[n],results[p,n]);
						minbound[n] = Math.Min(minbound[n],results[p,n]);
						minCost = Math.Min(minCost,costs[p]);
					}
				}
                result[clusterCount] = new SingleLacuna
                {
                    minimumConfidence = minCost,
                    examples = cluster.Count
                };
                //Check each input to see that the bounds are less than the range of the variable
                //if not, we can ignore that input
                List<LacunaInput> filteredInputs = new List<LacunaInput>();
				for(int i = 0; i < inputList.Count; i++)
				{
					double sensitivity = 0.02;//how close the bound must be, as a ratio, to be considered identical.
					InputInterface.DataType inputType = ((InputInterface)inputList[i]).type;
					if(inputType == InputInterface.DataType.categorical || inputType == InputInterface.DataType.presence)
					{
						sensitivity = 1.0/(double)((InputInterface)inputList[i]).categoryCount;
					}
					double interval = ((InputInterface)inputList[i]).upperBound - ((InputInterface)inputList[i]).lowerBound;
					double lowerGap = minbound[i] - ((InputInterface)inputList[i]).lowerBound;
					double upperGap = ((InputInterface)inputList[i]).upperBound - maxbound[i];
					if(upperGap/interval > sensitivity || lowerGap / interval > sensitivity)
					{
						LacunaInput lac = new LacunaInput();
						if(inputType == InputInterface.DataType.numeric || inputType == InputInterface.DataType.arity)
						{
							lac.maxValue = maxbound[i].ToString();
							lac.minValue = minbound[i].ToString();
						}
						else if(inputType == InputInterface.DataType.categorical || inputType == InputInterface.DataType.presence)
						{
							string minCat = ((InputInterface)inputList[i]).ConvertCategory(minbound[i]);
							string maxCat = ((InputInterface)inputList[i]).ConvertCategory(maxbound[i]);
							if(minCat != maxCat)
								throw new Exception("Cluster straddles two categories!");
							lac.category = minCat;
						}
						lac.name = ((InputInterface)inputList[i]).name;
						lac.inputType = Enum.GetName(typeof(InputInterface.DataType),((InputInterface)inputList[i]).type);
						filteredInputs.Add(lac);
					}
				}
				result[clusterCount].inputs = new LacunaInput[filteredInputs.Count];
				for(int i = 0; i < filteredInputs.Count; i++)
				{
					result[clusterCount].inputs[i] = (LacunaInput)filteredInputs[i];
				}
				clusterCount++;
			}
			return result;
		}
		/// <summary>
		/// Evaluate a point midway between these two.
		/// </summary>
		/// <param name="first">set of input values for first point</param>
		/// <param name="second">set of input values for second point</param>
		/// <returns></returns>
		private async Task<double> EvaluateMidPoint(double[] first, double[] second)
		{
			double[] midpoint = new double[first.Length];
			for(int n = 0; n < first.Length; n++)
			{
				midpoint[n] = (first[n] + second[n])/2.0;
			}
			return await Evaluate(midpoint);
		}

	}
}
