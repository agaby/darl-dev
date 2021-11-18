using DarlCompiler.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    public interface IDarlMetaRunTime
    {
        bool licensed { get; }

        SalienceRecord CalculateKGSaliences(List<SalienceRecord> saliences, KnowledgeState ks, ParseTree tree);
        Dictionary<string, double> CalculateSaliences(List<DarlResult> currentState, ParseTree tree);
        ParseTree CreateTree(string source, GraphObject node, IGraphModel model);
        ParseTree CreateTreeEdit(string source);
        Task Evaluate(ParseTree parseTree, List<DarlResult> inputs, KnowledgeState ks);
        List<GraphObject> ExploreGraph(ParseTree tree);
        void SetLicense(string license);
        string TermToDarl(DarlMetaNode node);
        string ToDarl(ParseTree parseTree);
    }
}
