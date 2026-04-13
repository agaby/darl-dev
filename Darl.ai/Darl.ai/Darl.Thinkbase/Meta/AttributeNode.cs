/// <summary>
/// AttributeNode.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    public class AttributeNode : UnaryDarlMetaNode
    {
        protected override async Task<object> DoEvaluate(DarlCompiler.Interpreter.ScriptThread thread)
        {
            Prologue(thread);
            DarlResult res1 = (DarlResult)await Argument.Evaluate(thread);
            var grammar = thread.Runtime.Language.Grammar as DarlMetaGrammar;
            if (grammar!.currentNode == null || grammar.currentNode.properties == null)
            {
                var res = new DarlResult(0, true);
                Epilogue(thread, res);
                return res;
            }
            var att = grammar.currentModel.FindDataAttribute(grammar!.currentNode.id, res1.Value.ToString(), grammar.state);
            if (att == null)
            {
                var res = new DarlResult(0, true);
                Epilogue(thread, res);
                return res;
            }
            var datatype = FindMatchingDataType();
            res1 = new DarlResult(datatype, res1.GetWeight());
            switch (datatype)
            {
                case DarlResult.DataType.numeric:
                    if (double.TryParse(att.Value, out double res))
                    {
                        var weight = res1.GetWeight();
                        res1 = new DarlResult(res);
                        res1.SetWeight(weight);
                    }
                    else
                    {
                        res1 = new DarlResult(0, true);
                    }
                    break;
                case DarlResult.DataType.textual:
                    {
                        res1.Value = att;
                        res1.stringConstant = att.Value;
                    }
                    break;
                case DarlResult.DataType.categorical:
                    {
                        res1.categories = new Dictionary<string, double> { { att.Value, res1.GetWeight() } };
                        res1.Value = att;
                    }
                    break;
                case DarlResult.DataType.temporal:
                    if (DateTime.TryParse(att.Value, out DateTime time))
                    {
                        res1.Value = time;
                    }
                    else
                    {
                        res1 = new DarlResult(0, true);
                    }
                    break;
            }
            Epilogue(thread, res1);
            return res1;
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
                return "attribute( ";
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