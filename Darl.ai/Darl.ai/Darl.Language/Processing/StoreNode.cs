using System;
using System.Collections.Generic;
using DarlCompiler.Ast;
using DarlCompiler.Interpreter;
using DarlCompiler.Parsing;
using System.Threading.Tasks;
using System.Linq;

namespace DarlLanguage.Processing
{
    public class StoreNode : IOSequenceDefinitionNode
    {
        public enum StoreType {sink,source}
        /// <summary>
        /// The left side
        /// </summary>
        public DarlIdentifierNode Left { get; private set; }
 
        protected List<DarlNode> arguments = new List<DarlNode>();


        public List<string> address { get; set; } = new List<string>();

        public StoreDefinitionNode storeDefinition { get; set; }

        int writeIndex = 0;

        /// <summary>
        /// Data direction
        /// </summary>
        public StoreType storeType { get; set; } = StoreType.source;

        public override object Value
        {
            get
            {
 /*               if (_value.IsUnknown())
                {
                    if (storeDefinition != null && storeDefinition.storeInterface != null)
                    {
                        try
                        {
                            Action readStore = async () =>
                            {
                                _value = await storeDefinition.storeInterface.ReadAsync(address);
                            };
                            readStore();
                        }
                        catch(Exception ex)
                        {

                        }
                    }
                }*/
                return _value;
            }
            internal set
            {
                DarlResult val = value as DarlResult;
                if (storeDefinition != null && storeDefinition.storeInterface != null)
                {
                    if(string.IsNullOrEmpty(val.name) && address.Count > 0)
                    {
                        val.name = address[0];
                    }
                    try
                    {
                        Action writeStore = () =>
                           {
                               storeDefinition.storeInterface.WriteAsync(address, val).Wait();
                           };
                        writeStore();
                    }
                    catch
                    {

                    }
                }
                _value = val;
            }
        }

        private DarlResult _value = new DarlResult(true,0.0,true); //bug here for non numeric comparisons


        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            Left = (DarlIdentifierNode)AddChild("-", nodes[0]);
            foreach (var node in nodes[1].ChildNodes)
            {
                arguments.Add((DarlNode)AddChild("-", node));
            }
        }
        /// <summary>
        /// return the local name
        /// </summary>
        /// <returns></returns>
        public override string GetName()
        {
            return $"{Left.GetName()}.{string.Join("_",arguments.Select( x => x.GetName()))}"; 
        }

        /// <summary>
        /// return the global name
        /// </summary>
        /// <returns></returns>
        public string GetCompName()
        {
            return $"{Left.ruleset}.{GetName()}";
        }


        protected async override Task<object> DoEvaluate(ScriptThread thread)
        {
            //assume sourcing
            thread.CurrentNode = this;  //standard prologue
            foreach (DarlNode child in arguments)
            {
                if (child != null)
                {
                    DarlResult res1 = (DarlResult)await child.Evaluate(thread);
                    if(res1.dataType == DarlResult.DataType.textual)
                        address.Add(res1.stringConstant);
                    else if(res1.dataType == DarlResult.DataType.categorical)
                        address.Add(res1.Value.ToString());
                }
            }
            //            _accessor = thread.Bind(GetCompName(), BindingRequestFlags.Read);
            //           this.Evaluate = _accessor.GetValueRef; // Optimization - directly set method ref to accessor's method. EvaluateReader;
            //           var result = await this.Evaluate(thread);
            if (storeDefinition != null && storeDefinition.storeInterface != null)
            {
                try
                {

                    _value = await storeDefinition.storeInterface.ReadAsync(address);

                }
                catch
                {

                }
            }
            var result = this.Value;
            thread.CurrentNode = Parent; //standard epilogue
            return result;
        }

        public override void WalkDependencies(List<IntraSetDependency> dependencies, DarlNode currentOutput, ConstantContext context)
        {
            if(context.stores.ContainsKey(Left.name))
            {
                storeDefinition = context.stores[Left.name];
            }
            if(!context.storeInputs.ContainsKey(GetName()))
            {
                context.storeInputs.Add(GetName(), this);
            }
            foreach (var node in arguments)
                node.WalkDependencies(dependencies, currentOutput, context);
            Left.WalkDependencies(dependencies, currentOutput, context);
        }

        public override string midamble
        {
            get
            {
                if (writeIndex > 0)
                {
                    writeIndex++;
                    return ", ";
                }
                else
                {
                    return "";
                }
            }
        }

        public override string postamble
        {
            get
            {
                writeIndex = 0;
                return "] ";
            }
        }

    }
}