using DarlCompiler.Ast;
using DarlCompiler.Interpreter;
using DarlCompiler.Parsing;
using System;
using System.Threading.Tasks;

namespace DarlLanguage.Processing
{
    /// <summary>
    /// Implements a set definition.
    /// </summary>
    public class SetDefinitionNode : DarlNode
    {
        /// <summary>
        /// Gets or sets the set as a <see cref="DarlResult"/>.
        /// </summary>
        /// <value>
        /// The set.
        /// </value>
        public DarlResult Set { get; set; }

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        /// <exception cref="DarlLanguage.Processing.RuleException">Incorrect number of set values</exception>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            string setName = (string)nodes[0].Token.Value;
            var values = nodes[1].ChildNodes;
            switch (values.Count)
            {
                case 0:
                default:
                    throw new RuleException("Incorrect number of set values");
                case 1:
                    Set = new DarlResult(conv(values[0].Token.Value));
                    break;
                case 2:
                    Set = new DarlResult(conv(values[0].Token.Value), conv(values[1].Token.Value));
                    break;
                case 3:
                    Set = new DarlResult(conv(values[0].Token.Value), conv(values[1].Token.Value), conv(values[2].Token.Value));
                    break;
                case 4:
                    Set = new DarlResult(conv(values[0].Token.Value), conv(values[1].Token.Value), conv(values[2].Token.Value), conv(values[2].Token.Value));
                    break;
            }
            Set.identifier = setName;
            context.Values.Add(setName, Set);
        }

        private double conv(object val)
        {
            if (val is string)
            {
                string text = val as string;
                if (text.ToLower() == "-infinity" || text.ToLower() == "-inf")
                    return double.NegativeInfinity;
                if (text.ToLower() == "infinity" || text.ToLower() == "inf")
                    return double.PositiveInfinity;
            }
            return Convert.ToDouble(val);
        }

        /// <summary>
        /// Does the evaluation.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>
        /// The result of the evaluation
        /// </returns>
        protected override Task<object> DoEvaluate(DarlCompiler.Interpreter.ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            thread.Bind(Set.identifier, BindingRequestFlags.Read);
            thread.CurrentNode = Parent; //standard epilogue
            return Task.FromResult<object>(Set);
        }
    }
}
