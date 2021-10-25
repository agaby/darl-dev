using Darl.Thinkbase.Meta;
using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    public class AttributesNode : LineageMetaNode
    {

        protected override Task<object> DoEvaluate(DarlCompiler.Interpreter.ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            var grammar = thread.Runtime.Language.Grammar as DarlMetaGrammar;
            var datatype = FindMatchingDataType();
            var res = new DarlResult("", datatype);
            foreach (var o in grammar.currentModel.GetConnectedObjects(grammar.currentNode, connLineage, objLineage))
            {
                var att = grammar.currentModel.FindDataAttribute(o.id, attLineage, grammar.state);
                if(att != null)
                    res.FuzzyAggregate(DarlVarExtensions.Convert(att));
            }
            if (!res.values.Any())
                res = new DarlResult(true, 0.0);
            thread.CurrentNode = Parent;
            return Task.FromResult<object>(res);
        }


        /// <summary>
        /// Gets the preamble.
        /// </summary>
        /// <value>
        /// The preamble, used to reconstruct the source code.
        /// </value>
        public override string preamble
        {
            get
            {
                return "attributes( ";
            }
        }

        /// <summary>
        /// Gets the postamble.
        /// </summary>
        /// <value>
        /// The postamble, used to reconstruct the source code.
        /// </value>
        public override string postamble
        {
            get
            {
                return ")";
            }
        }

        //look up the tree to find what datatype conversion is needed
        private DarlResult.DataType FindMatchingDataType()
        {
            switch (Parent)
            {
                case RuleNode r:
                    {
                        var p = r.ChildNodes[1] as DarlMetaIdentifierNode;
                        switch (p.identType)
                        {
                            case DarlMetaIdentifierNode.IdentifierType.categorical_output:
                                return DarlResult.DataType.categorical;
                            case DarlMetaIdentifierNode.IdentifierType.numeric_output:
                                return DarlResult.DataType.numeric;
                            case DarlMetaIdentifierNode.IdentifierType.temporal_output:
                                return DarlResult.DataType.temporal;
                            default:
                                return DarlResult.DataType.textual;
                        }
                    }
                case EqualNode r:
                    return DarlResult.DataType.numeric;
                case LesserEqualNode r:
                    return DarlResult.DataType.numeric;
                case LesserNode r:
                    return DarlResult.DataType.numeric;
                case GreaterNode r:
                    return DarlResult.DataType.numeric;
                case GreaterEqualNode r:
                    return DarlResult.DataType.numeric;
                case NotEqualNode r:
                    return DarlResult.DataType.numeric;
                case BeforeNode t:
                    return DarlResult.DataType.temporal;
                case PrecedesNode t:
                    return DarlResult.DataType.temporal;
                case OverlapsNode t:
                    return DarlResult.DataType.temporal;
                case DuringNode t:
                    return DarlResult.DataType.temporal;
                case StartsNode t:
                    return DarlResult.DataType.temporal;
                case FinishesNode t:
                    return DarlResult.DataType.temporal;
                case AfterNode t:
                    return DarlResult.DataType.temporal;
                case TempEqualNode t:
                    return DarlResult.DataType.temporal;
                case TempNotEqualNode t:
                    return DarlResult.DataType.temporal;
                case IsNode i:
                    return DarlResult.DataType.categorical;
                case MatchNode m:
                    return DarlResult.DataType.textual;
                default:
                    return DarlResult.DataType.numeric;
            }
        }
    }
}
