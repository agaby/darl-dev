using System;
using System.Collections.Generic;
using System.Text;

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

        public bool ContainsAttribute(string completionLineage)
        {
            if (this is GraphObject)
                return (this as GraphObject).ContainsAttribute(completionLineage);
            if(this is KnowledgeRecord)
                return (this as KnowledgeRecord).ContainsAttribute(completionLineage);
            return false;
        }

        public List<GraphConnection> Out(IGraphModel model)
        {
            if (this is GraphObject)
                return (this as GraphObject).Out;
            if (this is KnowledgeRecord)
                return (this as KnowledgeRecord).DeReference(model,null).Item2;
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
    }
}
