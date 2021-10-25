using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml;
using DarlLanguage.Processing;

namespace Darl.Language
{

    class AssociationRoot
    {
        static double excitationThreshold = 0.5;
        static public double minimumSupport = 0.005;
        static public double minimumConfidence = 0.3;
        /// <summary>
        /// Pre-process the input data to create a sparse array
        /// </summary>
        /// <remarks>In order to avoid re-evaluation and to speed up the processing of the apriori algorithm
        /// Each input value is evaluated and a membership value and index for each is created.
        /// The first pass creates an index linking inputs and sets/categories/vocabularies to a singla master index.
        /// In the next pass each data row is evaluated and a sparse array of excitations created.
        /// These excitations are expressed as <see cref="AssociationDataPoint"/> nodes and are sorted.
        /// This sparse array is passed on to the apriori algorithm as input.
        /// The key linking indexes to inputs and values is stored for use when creating rules</remarks>
        /// <param name="inputList">List of inputs</param>
        /// <param name="inSamplePatterns">integer array of selected patterns</param>
        internal void ProcessData(List<InputDefinitionNode> inputList, List<int> inSamplePatterns)
        {
            int offset;
            sparseArray = new List<List<AssociationDataPoint>>();
            foreach (int index in inSamplePatterns)
            {
                offset = 0;
                List<AssociationDataPoint> excitations = new List<AssociationDataPoint>();
                foreach (InputDefinitionNode input in inputList)
                {
                    int width = input.GetPartitions();
                    if (input.IsNumeric() || input.IsTextual())
                    {
                        //Maximum 9 sets, and excitation may span 2, so not too inefficient.
                        for (int n = 0; n < width; n++)
                        {
                            double excitation = input.CalculateMembership(n, index);
                            if (excitation > excitationThreshold)
                            {
                                excitations.Add(new AssociationDataPoint(excitation, offset + n, input));
                            }
                        }
                    }
                    else if (input.IsCategorical())
                    {
                        //categories can be very large in apriori - do it another way
                        if (input.Value.values[index] is DarlResult)
                        {
                            DarlResult result = (DarlResult)input.Value.values[index];
 /*                           foreach (int itemCat in result.categories.Keys)
                            {
                                excitations.Add(new AssociationDataPoint(1.0, offset + itemCat, input));
                            }*/ //fix this
                        }
                        else if (input.Value.values[index] is int)
                        {
                            excitations.Add(new AssociationDataPoint(1.0, offset + (int)input.Value.values[index], input));
                        }
                    }
                    offset += width;
                }
                excitations.Sort();
                sparseArray.Add(excitations);
            }
            sparseArray.Sort(CompareSparseArrays);
        }
        /// <summary>
        /// This creates the prefix tree from the sparseArray
        /// </summary>
        internal void CreatePrefixTree()
        {
            root = new AssociationNode();
            foreach (List<AssociationDataPoint> points in sparseArray)
            {
                int depth = 0;
                root.AddPoint(points, depth);
            }
            //now prune the trees
            root.Prune(minimumSupport * sparseArray.Count);
            //Now group them by predictee
            root.Group(groups);
        }


        internal string GenerateRules()
        {
            string result = "";
            foreach (int groupIndex in groups.Keys)
            {
                List<AssociationNode> group = groups[groupIndex];
                string outText = GenerateOutputText(group[0].input, groupIndex);
                foreach (AssociationNode node in group)
                {
                    double confidence = node.excitation / node.parent.excitation;
                    if (confidence >= minimumConfidence)
                    {
                        string rule = group.Count == 0 ? @"<rule xmlns='http://www.metarule.com/metarule'><if/><anything/>" : "<rule xmlns='http://www.metarule.com/metarule'><if/>";
                        rule += node.GenerateRule();
                        rule += "<then/>" + outText + "<confidence>" + confidence.ToString("0.000") + "</confidence></rule>";
                        result += rule;
                    }
                }
            }
            return result;
        }


        public static string GenerateInputText(InputDefinitionNode input, int partition)
        {
            string text = "<is><input>" + input.name + "</input>";
            if (input.IsCategorical())
            {
                string catText = "<category>" + input.GetNameFromPartitionIndex(partition) + "</category>";
                text += catText;
            }
            else if (input.IsNumeric())
            {
                string numText = "<set settype='" + input.GetNameFromPartitionIndex(partition) + "'/>";
                text += numText;
            }
            else if (input.IsTextual())
            {
                string inText = "<vocabulary>" + input.GetNameFromPartitionIndex(partition) + "</vocabulary>";
                text += inText;
            }
            text += "</is>";
            return text;
        }

        public static string GenerateOutputText(InputDefinitionNode input, int partition)
        {
            string outputText = "<output>" + input.name + "</output><willbe/>";
            if (input.IsCategorical())
            {
                string catText = "<category>" + input.GetNameFromPartitionIndex(partition) + "</category>";
                outputText += catText;
            }
            else if (input.IsNumeric())
            {
                string numText = "<set settype='" + input.GetNameFromPartitionIndex(partition) + "'/>";
                outputText += numText;
            }
            else if (input.IsTextual())
            {
                string outText = "<category>" + input.GetNameFromPartitionIndex(partition) + "</category>";
                string inText = "<vocabulary>" + input.GetNameFromPartitionIndex(partition) + "</vocabulary>";
                outputText += outText;
            }
            return outputText;
        }


        /// <summary>
        /// Create the outputs
        /// </summary>
        /// <remarks>Since this is unsupervised learning, we need to create outputs. 
        /// The algorithm works out what can be predicted, by identifying associations 
        /// with confidence and support greater than the thresholds. This may not include 
        /// all the inputs, or even all the input/set/category combinations.
        /// Outputs are created with the same names as inputs. 
        /// The type is carried over, as is the path, except that textual inputs are 
        /// interpreted as categorical outputs, and the names of the vocabularies are mapped onto categories.  </remarks>
        /// <returns>A string containing the output definitions in DARL</returns>
        internal string GenerateOutputs()
        {
            /*            //this relies on groups clustering outputs - correct
                        string result = "<outputlist xmlns='http://www.metarule.com/metarule'>";
                        string postamble = string.Empty;
                        List<InputDefinitionNode> inputsCovered = new List<InputDefinitionNode>();
                        //The keys in groups relate to the set of all input/partition combinations that occur in rules
                        List<int> indexList = new List<int>(groups.Keys);
                        indexList.Sort();
                        //indexlist now has the indexes in ascending order, so in input major, partition minor order
                        OutputDefinitionNode outBind = null;
                        foreach (int groupIndex in indexList)
                        {
                            InputDefinitionNode input = groups[groupIndex][0].input;
                            if (!inputsCovered.Contains(input)) //new one
                            {
                                outBind = new OutputDefinitionNode();
                                outBind.name = input.name;
                                outBind.path = input.path;
                                outBind.values = input.values;
                                result += postamble; //finish off last output
                                result += "<outputspec><output>" + input.name + "</output>";
                                if (input.HasPath())
                                    result += "<path>" + input.path + "</path>";
                                if (input.IsNumeric())
                                {
                                    outBind.type = IOBinding.IOType.numeric;
                                    result += "<numeric>";
                                    postamble = "</numeric></outputspec>";
                                }
                                else if (input.IsCategorical() || input.IsTextual())
                                {
                                    outBind.type = IOBinding.IOType.categorical;
                                    result += "<categorical>";
                                    postamble = "</categorical></outputspec>";
                                }
                                inputsCovered.Add(input);
                            }
                            int partition = groupIndex;//should be plus input offset.
                            if (input.IsNumeric())
                            {
                                result += "<setdefinition><set settype='" + input.GetNameFromPartitionIndex(partition) + "'/>";
                                DarlResult res = (DarlResult)input.sets[partition];
                                double dLower = (double)res.values[0];
                                result += "<lower>" + XmlConvert.ToString(dLower) + "</lower>";
                                double dUpper = (double)res.values[res.values.Count - 1];
                                if (res.HowFuzzy() == DarlResult.Fuzzyness.triangle)
                                {
                                    double dMiddle = (double)res.values[1];
                                    result += "<middle>" + XmlConvert.ToString(dMiddle) + "</middle>";
                                }
                                if (res.HowFuzzy() == DarlResult.Fuzzyness.trapezoid)
                                {
                                    double dMiddle = (double)res.values[1];
                                    double dUpperMid = (double)res.values[2];
                                    result += "<lowermid>" + XmlConvert.ToString(dMiddle) + "</lowermid>";
                                    result += "<uppermid>" + XmlConvert.ToString(dUpperMid) + "</uppermid>";
                                }
                                result += "<upper>" + XmlConvert.ToString(dUpper) + "</upper></setdefinition>";
                                //add set definition here
                            }
                            else if (input.IsCategorical() || input.IsTextual())
                            {
                                result += "<category>" + input.GetNameFromPartitionIndex(partition) + "</category>";
                                outBind.AddCategory(input.GetNameFromPartitionIndex(partition), outBind.categories.Count);
                            }
                        }
                        result += postamble + "</outputlist>";
                        return result; */
            return string.Empty;
        }


        List<List<AssociationDataPoint>> sparseArray;
        AssociationNode root;
        Dictionary<int, List<AssociationNode>> groups = new Dictionary<int, List<AssociationNode>>();
        Dictionary<int, int> partitionReferences = new Dictionary<int, int>();


        private static int CompareSparseArrays(List<AssociationDataPoint> x, List<AssociationDataPoint> y)
        {
            if (x.Count > 0 && y.Count > 0)
            {
                return x[0].column.CompareTo(y[0].column);
            }
            else return 0;
        }


    }

    internal class AssociationDataPoint : IComparable
    {
        internal int column;
        internal double Value;
        internal InputDefinitionNode input;

        internal AssociationDataPoint(double exc, int col, InputDefinitionNode inp)
        {
            column = col;
            Value = exc;
            input = inp;
        }

        #region IComparable Members

        int IComparable.CompareTo(object obj)
        {
            if (!(obj is AssociationDataPoint))
                throw new Exception("Comparing different types");
            return this.column.CompareTo(((AssociationDataPoint)obj).column);
        }

        #endregion
    }



    internal class AssociationNode : IComparable
    {
        internal int columnIndex;
        internal double excitation;
        internal InputDefinitionNode input;
        internal int nodeDepth;
        internal Dictionary<int, AssociationNode> children;
        internal AssociationNode parent;

        internal AssociationNode()
        {
            columnIndex = -1; //signifies root node
            excitation = 0.0;
            nodeDepth = 0;
            parent = null;
            children = new Dictionary<int, AssociationNode>();
        }
        /// <summary>
        /// Recursive procedure to populate 
        /// </summary>
        /// <param name="points"></param>
        /// <param name="depth"></param>
        internal void AddPoint(List<AssociationDataPoint> points, int depth)
        {
            int column = points[depth].column;
            excitation += points[depth].Value;
            input = points[depth].input;
            nodeDepth = depth;
            depth++;
            if (depth == points.Count)
                return;
            if (!children.ContainsKey(column))
            {
                AssociationNode node = new AssociationNode();
                node.columnIndex = column;
                node.parent = this;
                children.Add(column, node);
            }
            children[column].AddPoint(points, depth);
        }

        #region IComparable Members

        int IComparable.CompareTo(object obj)
        {
            if (!(obj is AssociationNode))
                throw new Exception("Comparing different types");
            return ((AssociationNode)obj).excitation.CompareTo(this.excitation);
        }

        #endregion

        internal string GenerateRule()
        {
            string result = "";
            AssociationNode node = this.parent;
            string postamble = "";
            while (node.nodeDepth != 0)
            {
                if (node.nodeDepth > 1)
                {
                    result += "<and>";
                    postamble += "</and>";
                }
                result += AssociationRoot.GenerateInputText(node.input, node.columnIndex);
                node = node.parent;
            }
            result += postamble;
            return result;
        }
        /// <summary>
        /// Delete any parts of the tree with insufficient support or confidence
        /// </summary>
        /// <param name="minimumSupport"></param>
        internal void Prune(double minimumSupport)
        {
            List<AssociationNode> deletionList = new List<AssociationNode>();
            foreach (AssociationNode node in this.children.Values)
            {
                if (node.excitation > minimumSupport)
                    node.Prune(minimumSupport);
                else
                    deletionList.Add(node);
            }
            foreach (AssociationNode node in deletionList)
                children.Remove(node.columnIndex);
        }
        /// <summary>
        /// Collect nodes by predictee
        /// </summary>
        /// <param name="groups"></param>
        internal void Group(Dictionary<int, List<AssociationNode>> groups)
        {
            if (this.nodeDepth > 1)
            {
                if (!groups.ContainsKey(this.columnIndex))
                {
                    List<AssociationNode> list = new List<AssociationNode>();
                    groups.Add(columnIndex, list);
                }
                groups[columnIndex].Add(this);
            }
            foreach (AssociationNode node in this.children.Values)
            {
                node.Group(groups);
            }
        }
    }
}

