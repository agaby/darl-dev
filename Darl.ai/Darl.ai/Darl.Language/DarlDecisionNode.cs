// ***********************************************************************
// Assembly         : DarlLanguage
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="DarlDecisionNode.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using DarlLanguage.Processing;
using System;
using System.Collections.Generic;

namespace DarlLanguage
{
    /// <summary>
    /// Class DarlDecisionNode.
    /// </summary>
    class DarlDecisionNode
    {
        /// <summary>
        /// The minimum number of leaf nodes to continue tree generation.
        /// </summary>
        private const int dataThreshold = 5;
        /// <summary>
        /// Scaling value to penalize large trees.
        /// </summary>
        private const double leafInfoThreshold = 0.05;
        /// <summary>
        /// The input associated with the current node.
        /// </summary>
        protected InputDefinitionNode input;
        /// <summary>
        /// The single output used in the analysis
        /// </summary>
        protected OutputDefinitionNode output;
        /// <summary>
        /// The list of inputs still unassigned to a DarlDecisionNode
        /// </summary>
        protected List<InputDefinitionNode> inputs;
        /// <summary>
        /// An array of integers containing the indexes of the data records associated with this part of the tree.
        /// </summary>
        protected List<int> indices;
        /// <summary>
        /// Child nodes of this node.
        /// </summary>
        protected List<DarlDecisionNode> subNodes;
        /// <summary>
        /// Holds list of inputs minus that associated with this node to be passed on.
        /// This does not really need to be stored.
        /// </summary>
        protected List<InputDefinitionNode> newInputs;
        /// <summary>
        /// The category or set associated with this node.
        /// </summary>
        protected int index;
        /// <summary>
        /// During merging, a list of the indexes, to sets or categories, that relate to this node.
        /// </summary>
        protected List<int> mergedIndexes;
        /// <summary>
        /// A table of the outputs partitions, so sets or categories, found to be associated with this node
        /// </summary>
        protected Dictionary<object,double> outputPartitions;
        /// <summary>
        /// Shows that this node has been merged with another.
        /// </summary>
        protected bool merged; // m_bMerged
                               /// <summary>
                               /// Rules below this confidence will not be outputted
                               /// </summary>
        internal static double minimumConfidence = 0.0;
        /// <summary>
        /// Rules with support less than this value will not be outputted.
        /// </summary>
        internal static int minimumSupport = 1;


        /// <summary>
        /// Main function to generate the decision node tree
        /// </summary>
        /// <param name="currentInput">The input chosen to divide on</param>
        /// <param name="currentOutput">The output to be trained on.</param>
        /// <param name="currentInputs">The list of available inputs not yet used.</param>
        /// <param name="currentIndices">The list of patterns to be trained on</param>
        /// <param name="currentIndex">The ind of this nodes partition</param>
        /// <param name="depth">The depth.</param>
        /// <exception cref="RuleException">Insufficient information in the data set to learn anything.</exception>
        internal void GenerateNodes(InputDefinitionNode currentInput, OutputDefinitionNode currentOutput, List<InputDefinitionNode> currentInputs, List<int> currentIndices, int currentIndex, int depth)
		{
			input = currentInput;
			output = currentOutput;
			inputs = currentInputs;
			indices = currentIndices;
			index = currentIndex;
			depth++;
			int minDataPoints = Math.Max(dataThreshold,currentOutput.learningSource.Count / (currentOutput.categories.Count * 10));
			// if conditions for a leaf node are met return
			double info = output.CalculateInformation(indices);
            if (inputs.Count == 0 || indices.Count < minDataPoints || double.IsNaN(info))
			{
				// its a leaf node
				// choose the output partitions to assign it to.
				outputPartitions = output.ChoosePartitions(indices);
				return;
			}
			// first choose an input to split on. 
			InputDefinitionNode bestInput = null;
			double bestInfoGain = 0.0; 
			foreach(InputDefinitionNode trialInput in inputs)
			{
				double split = 0.0;
				double entropy = trialInput.CalculateInformation(indices,output,ref split);
				double infoGain = Math.Abs(info - entropy)/split;
                if (infoGain > bestInfoGain && !double.IsNaN(entropy))
				{
					bestInput = trialInput;
					bestInfoGain = infoGain;
				}
			}
//			Debug.WriteLine(string.Concat("Best info gain: ",bestInfoGain.ToString(), " Depth: ", depth.ToString()));
			if(bestInput == null || bestInfoGain < (leafInfoThreshold * (double)(depth - 1)))
			{
				if(input == null)
				{
					throw new RuleException("Insufficient information in the data set to learn anything.");
				}
				outputPartitions = output.ChoosePartitions(indices);
				return; //nothing sparkled, leaf node
			}
			// bestInput is it.
			// create a new list of inputs minus that just selected for the new nodes 
			newInputs = new List<InputDefinitionNode>();
			foreach(InputDefinitionNode currInput in inputs)
			{
				if(currInput != bestInput)
					newInputs.Add(currInput);
			}
			// now share out the data values between each class, 
			// generate new nodes and pass the data on to them
			for(int n = 0; n < bestInput.categories.Count; n++)
			{
				DarlDecisionNode node = new DarlDecisionNode();
				subNodes.Add(node);
				List<int> newList = new List<int>();
				foreach(int ind in indices)
				{
					if(bestInput.learningSource[ind] != -1)
					{
	
						if(bestInput.CalculateMembership(ind,n) > 0.5)
							newList.Add(ind);
					}
				}
				node.GenerateNodes(bestInput,output, newInputs, newList, n, depth);
			}
		}
        /// <summary>
        /// Main function to create a metarule version of the decision tree.
        /// </summary>
        /// <param name="start">Reference to start text</param>
        /// <param name="middle">reference to middle text</param>
        /// <param name="level">initially o, incremented as the tree is searched</param>
        /// <returns>System.Int32.</returns>
        internal int GenerateRules(ref string start, string middle, int level)
		{
			int count = 0;
			// generating rules is a question of reading the tree 
			// backwards and can be implemented as a recursive procedure.
			if(level != 0) // !root node
			{
				if(level > 1) // not one of the nodes just below the root, so we need an "and" 
				{
					middle = string.Concat( middle, " and ");
				}
				if(merged)
				{
					string orTerms = " or ";
					WriteTerm(ref orTerms,input,input.categories[index]);
					int nCount = mergedIndexes.Count;
					foreach(int candidate in mergedIndexes)
					{
						if(nCount > 1)
						{
							orTerms = string.Concat(" or ",orTerms);
						}
						WriteTerm(ref orTerms,input,input.categories[candidate]);
						if(nCount > 1)
						{
							orTerms = string.Concat(orTerms," or ");
						}
						nCount--;
					}
					orTerms = string.Concat(orTerms,"");
					middle = string.Concat(middle,orTerms);
				}
				else
					WriteTerm(ref middle,input,input.categories[index]);
				level++;
			}
			if(IsLeaf())
			{ // leaf node, 
				count += CompleteRules(ref start, ref middle);
			}
			foreach(DarlDecisionNode subNode in subNodes)// for each sub tree
			{
				count += subNode.GenerateRules(ref start, middle,level == 0 ? 1 : level);
			}
			return count;
		}
        /// <summary>
        /// Writes out a single "is" term and asociated input and set/category/vocab
        /// </summary>
        /// <param name="middle">Recieves text</param>
        /// <param name="currentInput">Not used</param>
        /// <param name="currentIndex">Partition for this term.</param>
        protected void WriteTerm(ref string middle, IODefinitionNode currentInput, string currentIndex)
		{
			middle = string.Concat(middle, input.name, " is ");
			if(input.iType == InputDefinitionNode.InputTypes.presence_input)
			{
				if(currentIndex[0] == 'a')
					middle = string.Concat(middle, "absent ");
				else
					middle = string.Concat(middle, "present ");
			}
			else if(input.iType == InputDefinitionNode.InputTypes.categorical_input)
			{
				middle += string.Format("\"{0}\" ",currentIndex);
			}
			else
			{
				middle = string.Concat(middle, currentIndex," ");
			}		
		}
        /// <summary>
        /// Consider if this and the node parameter can be merged, if not return false.
        /// </summary>
        /// <param name="node">prospective mergee</param>
        /// <returns>true if a merge has occurred.</returns>
        protected bool Merge(DarlDecisionNode node)
		{
			// Consider if this and pNode can be merged, if not return false.
			// If possible, do it and return true. pNode can then be deleted 
			// and removed from the parent nodelist
			// first test - can only merge two leaf nodes.
			if(!IsLeaf())
			{
				CheckForMerge();
				return false;
			}
			if(!node.IsLeaf())
				return false;
			// we are relying on this and node being contiguous for numeric inputs
			// i.e. if sets very large, large, medium, small and very small are defined
			// if this is large, then pNode must be medium, if this is medium, node must be small etc..
			// Now consider if the merger makes sense in terms of performance.
			// This is done by looking at the outputPartitions Dictionary. 
			// What we are trying to do, is produce less rules with more in them, as a means to increasing 
			// the efficiency of representation and speed of execution.
			// start trivial - merge if the max is the same for both
			double max = 0;
			Object maxCandidate = null;
			foreach(Object candidate in node.outputPartitions.Keys)
			{
				if(node.outputPartitions[candidate] > max)
				{
					maxCandidate = candidate;
					max = node.outputPartitions[candidate];
				}
			}
			max = 0;
			Object maxThis = 0;
			foreach(Object candidate2 in outputPartitions.Keys)
			{
				if(outputPartitions[candidate2] > max)
				{
					maxThis = candidate2;
					max = outputPartitions[candidate2];
				}
			}
			if(maxCandidate != maxThis)
				return false;

			// Finally merge the outputPartitions maps
			foreach(Object candidate3 in node.outputPartitions.Keys)
			{
				if(!outputPartitions.ContainsKey(candidate3))
				{ // not present, add on
					outputPartitions.Add(candidate3,node.outputPartitions[candidate3]);
				}
				else
				{
					double temp = outputPartitions[candidate3];
					outputPartitions[candidate3] = temp + node.outputPartitions[candidate3];
				}
			}	
			// set the merged flag
			merged = true;
			mergedIndexes.Add(node.index);
			if(node.merged)
			{// also need to copy previously merged indexes.
				foreach(int currIndex in node.mergedIndexes)
				{
					mergedIndexes.Add(currIndex);
				}
			}
			return true;
		}
        /// <summary>
        /// Detects if this node is a leaf node
        /// </summary>
        /// <returns>true if a leaf.</returns>
        protected bool IsLeaf()
		{
			return subNodes.Count == 0;
		}
        /// <summary>
        /// Generates the right hand side of a rule.
        /// </summary>
        /// <param name="start">Reference to text receiving definitions</param>
        /// <param name="middle">pregenerated terms.</param>
        /// <returns>System.Int32.</returns>
        protected int CompleteRules(ref string start, ref string middle)
		{
			int count = 0;
			foreach(int partition in outputPartitions.Keys)
			{
				string partitionText = this.output.categories[partition];
				double dRelevance = outputPartitions[partition];
				if (dRelevance >= 1.0 / (double)output.categories.Count && dRelevance >= minimumConfidence && indices.Count >= minimumSupport) //added for assoc rule apps
				{
					start += string.Format("if {0} then ",string.IsNullOrEmpty(middle) ? "anything " : middle);
                    start += string.Format("{0} will be ", output.name);
                    start += this.output.iType == OutputDefinitionNode.OutputTypes.numeric_output ? partitionText : string.Format("\"{0}\"", partitionText);
					double dConf = outputPartitions[partition];
					start += string.Format(" confidence {0};",dConf.ToString());
					int examplecount = indices.Count;
					start += string.Format(" // examples: {0}\n",examplecount.ToString());
					count++;
				}
			}
			return count;
		}
        /// <summary>
        /// constructor.
        /// </summary>
        internal DarlDecisionNode()
		{
			input = null;
			output = null;
			newInputs = null;
			outputPartitions = null;
			merged = false;
            subNodes = new List<DarlDecisionNode>();
			mergedIndexes = new List<int>();
		}
        /// <summary>
        /// Controls the merging process
        /// </summary>
        internal void CheckForMerge()
		{
			// The Merge function checks for leaf nodes that are combineable
			// with OR functions. For a categorical input, any categories may be merged, 
			// since categories are not ordered. Fuzzy sets are ordered, so only adjacent sets are merged.
			DarlDecisionNode firstNode = null;
			List<DarlDecisionNode> copySubNodes = new List<DarlDecisionNode>();
			foreach(DarlDecisionNode subNode in subNodes)// make a copy of the tree
			{
				copySubNodes.Add(subNode);
			}
			foreach(DarlDecisionNode subNode in copySubNodes)// for each sub tree, iterate through copy
			{
				if(firstNode == null)
				{
					subNode.CheckForMerge();
					firstNode = subNode;
				}
				else
				{
					if(subNode.Merge(firstNode))
					{
						subNodes.Remove(firstNode);
						firstNode = subNode;
					}
					else
					{
                        if (subNode.input.iType == InputDefinitionNode.InputTypes.numeric_input || subNode.input.iType == InputDefinitionNode.InputTypes.arity_input)
							firstNode = subNode; // ensures only adjacent are merged for numeric.
					}
				}
			}
		}
    }
}
