using DarlCompiler.Ast;
using DarlCompiler.Parsing;

namespace DarlLanguage.Processing
{
    /// <summary>
    /// Implements a wire definition
    /// </summary>
    public class WireDefinitionNode : DarlNode
    {
        /// <summary>
        /// The possible types of wires
        /// </summary>
        public enum WireType {
            /// <summary>
            /// The wirein
            /// </summary>
            wirein,
            /// <summary>
            /// The wireout
            /// </summary>
            wireout,
            /// <summary>
            /// The wirethrough
            /// </summary>
            wirethrough,
            /// <summary>
            /// The wireinternal
            /// </summary>
            wireinternal,
            /// <summary>
            /// Bidirectional wire for a store
            /// </summary>
            wirestore
        }

        /// <summary>
        /// Gets the sourcename.
        /// </summary>
        /// <value>
        /// The sourcename.
        /// </value>
        public string sourcename { get;  set; }

        /// <summary>
        /// Gets the destname.
        /// </summary>
        /// <value>
        /// The destname.
        /// </value>
        public string destname { get;  set; }

        /// <summary>
        /// Gets the source ruleset.
        /// </summary>
        /// <value>
        /// The source ruleset.
        /// </value>
        public string sourceRuleset { get;  set; }

        /// <summary>
        /// Gets the dest ruleset.
        /// </summary>
        /// <value>
        /// The dest ruleset.
        /// </value>
        public string destRuleset { get;  set; }

        /// <summary>
        /// Get the composite source
        /// </summary>
        public string compSource
        {
            get
            {
                if (string.IsNullOrEmpty(sourceRuleset))
                    return sourcename;
                else
                    return sourceRuleset + "." + sourcename;
            }
        }

        int writeIndex = 0;

        /// <summary>
        /// Get the composite destination
        /// </summary>
        public string compDest
        {
            get
            {
                if (string.IsNullOrEmpty(destRuleset))
                    return destname;
                else
                    return destRuleset + "." + destname;
            }
        }

        /// <summary>
        /// Gets the wiretype.
        /// </summary>
        /// <value>
        /// The wiretype.
        /// </value>
        public WireType wiretype { get;  set; }

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            foreach (var child in nodes)
            {
                var childAst = AddChild("-", child);
                switch (childAst.AsString)
                {
                    case "wiretemporalin":
                    case "wirenumericin":
                    case "wirecategoricin":
                    case "wiretextualin":
                        sourcename = ((DarlIdentifierNode)childAst.ChildNodes[0]).name;
                        sourceRuleset = string.Empty;
                        destRuleset = ((DarlIdentifierNode)childAst.ChildNodes[1].ChildNodes[0]).name;
                        destname = ((DarlIdentifierNode)childAst.ChildNodes[1].ChildNodes[1]).name;
                        wiretype = WireType.wirein;
                        break;
                    case "wiretemporalout":
                    case "wirenumericout":
                    case "wirecategoricout":
                    case "wiretextualout":
                        sourcename = ((DarlIdentifierNode)childAst.ChildNodes[0].ChildNodes[1]).name;
                        sourceRuleset = ((DarlIdentifierNode)childAst.ChildNodes[0].ChildNodes[0]).name;
                        destRuleset = string.Empty;
                        destname = ((DarlIdentifierNode)childAst.ChildNodes[1]).name;
                        wiretype = WireType.wireout;
                        break;
                    case "wiretemporalinternal":
                    case "wirenumericinternal":
                    case "wirecategoricinternal":
                    case "wiretextualinternal":
                        sourcename = ((DarlIdentifierNode)childAst.ChildNodes[0].ChildNodes[1]).name;
                        sourceRuleset = ((DarlIdentifierNode)childAst.ChildNodes[0].ChildNodes[0]).name;
                        destRuleset = ((DarlIdentifierNode)childAst.ChildNodes[1].ChildNodes[0]).name;
                        destname = ((DarlIdentifierNode)childAst.ChildNodes[1].ChildNodes[1]).name;
                        wiretype = WireType.wireinternal;
                        break;
                    case "wireinout":
                        sourcename = ((DarlIdentifierNode)childAst.ChildNodes[0]).name;
                        sourceRuleset = string.Empty;
                        destRuleset = string.Empty;
                        destname = ((DarlIdentifierNode)childAst.ChildNodes[1]).name;
                        wiretype = WireType.wirethrough;
                        break;
                }
            }
        }

        /// <summary>
        /// Walks the saliences.
        /// </summary>
        /// <param name="saliency">The incoming saliency.</param>
        /// <param name="root">The map root.</param>
        /// <param name="currentRuleSet">The current rule set.</param>
        /// <param name="currentOutput">The current output.</param>
        public override void WalkSaliences(double saliency, MapRootNode root, string currentRuleSet, string currentOutput)
        {
            root.NavigateSource(sourcename, sourceRuleset).WalkSaliences(saliency, root, sourceRuleset, sourcename);
        }

        public override string preamble
        {
            get
            {
                return "wire ";
            }
        }

        public override string midamble
        {
            get
            {
                switch(writeIndex)
                {
                    case 0:
                        writeIndex++;
                        return string.IsNullOrEmpty(sourceRuleset) ? "" : $"{sourceRuleset}.";
                    case 1:
                        writeIndex++;
                        return string.IsNullOrEmpty(destRuleset) ? "" : $"{destRuleset}.";
                    default:
                        return "";
                }
            }
        }

        public override string postamble
        {
            get
            {
                writeIndex = 0;
                return ";";
            }
        }
    }
}
