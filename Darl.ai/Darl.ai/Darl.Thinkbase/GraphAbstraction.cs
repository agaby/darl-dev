using Darl.Common;
using System;
using System.Collections.Generic;

namespace Darl.Thinkbase
{
    /// <summary>
    /// Top level of graph element hierarchy. Links KnowledgeStates and GraphElements
    /// </summary>
    /// <remarks>Intended to refer only to GraphObject or KnowledgeRecord - No union in C#</remarks>
    public abstract class GraphAbstraction
    {
        KnowledgeRecord? knowledgeRecord { get { return (this is KnowledgeRecord) ? this as KnowledgeRecord : null; } }
        GraphObject? graphObject { get { return (this is GraphObject) ? this as GraphObject : null; } }

        public virtual bool ContainsAttribute(string completionLineage, GraphAttribute.DataType? type = GraphAttribute.DataType.ruleset)
        {
            throw new NotImplementedException();
        }

        public List<GraphConnection> Out(IGraphModel model)
        {
            if (this is GraphObject)
                return (this as GraphObject).Out;
            if (this is KnowledgeRecord)
                return (this as KnowledgeRecord).DeReference(model, null).Item2;
            return new List<GraphConnection>();
        }

        public List<GraphConnection> In(IGraphModel model)
        {
            if (this is GraphObject)
                return (this as GraphObject).In;
            if (this is KnowledgeRecord)
                //bug - this returns both directions
                return (this as KnowledgeRecord).DeReference(model, null).Item2;
            return new List<GraphConnection>();
        }

        public string Id(IGraphModel model)
        {
            if (this is GraphObject)
                return (this as GraphObject).id;
            if (this is KnowledgeRecord)
                //bug - this returns both directions
                return (this as KnowledgeRecord).DeReference(model, null).Item1.id;
            return string.Empty;
        }

        public string Name(IGraphModel model)
        {
            if (this is GraphObject)
                return (this as GraphObject).name;
            if (this is KnowledgeRecord)
                return (this as KnowledgeRecord).DeReference(model, null).Item1.name;
            return string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <param name="model"></param>
        /// <param name="currentTime"></param>
        /// <returns></returns>
        /// <remarks>Currently only works for singletons</remarks>

        public double Coexists(GraphAbstraction other, IGraphModel model, FuzzyTime? currentTime)
        {
            List<DarlTime>? thisExistence = null;
            List<DarlTime>? otherExistence = null;
            if (this is GraphObject)
            {
                thisExistence = (this as GraphObject).existence;
            }
            else if (this is KnowledgeRecord)
            {
                thisExistence = (this as KnowledgeRecord).GetExistence(model);
            }
            if (other is GraphObject)
            {
                otherExistence = (other as GraphObject).existence;
            }
            else if (other is KnowledgeRecord)
            {
                otherExistence = (other as KnowledgeRecord).GetExistence(model);
            }
            if (otherExistence == null && thisExistence == null)
                return 1.0;
            if (thisExistence != null)
            {
                thisExistence = Quadrify(thisExistence);
            }
            if (otherExistence != null)
            {
                otherExistence = Quadrify(otherExistence);
            }
            if (otherExistence != null && currentTime != null) //check existence includes current time
            {
                if (currentTime.darlTimes[0].raw > otherExistence[3].raw || currentTime.darlTimes[0].raw < otherExistence[0].raw)
                    return 0;//not existant at currentTime
            }
            if (thisExistence != null && currentTime != null) //check existence includes current time
            {
                if (currentTime.darlTimes[0].raw > thisExistence[3].raw || currentTime.darlTimes[0].raw < thisExistence[0].raw)
                    return 0;//not existant at currentTime
            }
            if (otherExistence == null || thisExistence == null)
                return 1.0;
            if (currentTime == null)
            {
                if (thisExistence[0].raw > otherExistence[3].raw)
                    return 0.0;
                if (thisExistence[3].raw < otherExistence[0].raw)
                    return 0.0;
            }
            return 1.0;
        }

        private List<DarlTime> Quadrify(List<DarlTime> range)
        {
            var expandedRange = new List<DarlTime>();
            switch (range.Count)
            {
                default:
                    return range;
                case 1:
                    expandedRange.Add(range[0]);
                    expandedRange.Add(range[0]);
                    expandedRange.Add(range[0]);
                    expandedRange.Add(range[0]);
                    return expandedRange;

                case 2:
                    expandedRange.Add(range[0]);
                    expandedRange.Add(range[0]);
                    expandedRange.Add(range[1]);
                    expandedRange.Add(range[1]);
                    return expandedRange;
                case 3:
                    expandedRange.Add(range[0]);
                    expandedRange.Add(range[1]);
                    expandedRange.Add(range[1]);
                    expandedRange.Add(range[2]);
                    return expandedRange;
            }
        }

        public virtual (GraphObject?, List<GraphConnection>) DeReference(IGraphModel model, List<string>? lineages)
        {
            throw new NotImplementedException();
        }

    }
}
