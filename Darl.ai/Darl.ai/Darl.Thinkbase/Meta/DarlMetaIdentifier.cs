using DarlCompiler.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Darl.Thinkbase.Meta
{
    public class DarlMetaIdentifier : IdentifierTerminal
    {

        static HashSet<string> declarationTokens = new HashSet<string> { "numeric", "categorical", "textual", "constant", "string", "sequence", "store", "temporal", "network","duration"};
        static HashSet<string> IOTokens = new HashSet<string>() { "numeric_input", "numeric_output", "categorical_input", "categorical_output", "textual_input", "textual_output", "temporal_output", "temporal_input", "store_io", "network_output" };
        static HashSet<string> ConstantTokens = new HashSet<string> { "numeric_constant", "string_constant", "sequence_constant", "temporal_constant","duration_constant","lineage_constant" };

        public string QualifiedName { get; set; }

        public DarlMetaIdentifier(string name) : base(name)
        {
            QualifiedName = name;
        }

        public override void OnValidateToken(ParsingContext context)
        {
            //add checks for inputs, sets, categories & outputs in the rules that they exist and are legal.
            //Create syntax errors if not.
            base.OnValidateToken(context);
            if (context.PreviousToken == null)
                return;
            try
            {
                //check this is a declaration
                if (declarationTokens.Contains(context.PreviousToken.Text)) // one of the declarations, and not yet referenced
                {
                    string name = context.CurrentToken.Text;
                    context.Values.Add(name, context.CurrentToken.Terminal);
                }
                //look for declarations of categories & sets within input and output declarations
                else if (context.PreviousToken.Text == "{" || (context.PreviousToken.Text == "," && context.OpenBraces.Any() && context.OpenBraces.First().ValueString == "{")) //inside a declaration of sets/categories
                {//the following relies on change to Darl from standard making ParserStack public.
                    for (int n = context.ParserStack.Count - 1; n >= 0; n--)
                    {
                        if (context.ParserStack[n].Token != null && context.ParserStack[n].Token.Terminal != null)
                        {
                            if (context.ParserStack[n].Token.Terminal.Name == "categorical_output" || context.ParserStack[n].Token.Terminal.Name == "categorical_input" || context.ParserStack[n].Token.Terminal.Name == "dynamic_categorical_input")
                            {//store category as ioname.categoryname
                                context.Values.Add(context.ParserStack[n].Token.Text + "." + context.CurrentToken.Text, context.CurrentToken.Terminal);
                                break;
                            }
                            else if (context.ParserStack[n].Token.Terminal.Name == "numeric_output" || context.ParserStack[n].Token.Terminal.Name == "numeric_input")
                            {//store set as ioname.setname
                                context.Values.Add(context.ParserStack[n].Token.Text + "." + context.CurrentToken.Text, context.CurrentToken.Terminal);
                                break;
                            }
                        }

                    }
                }
                else if (context.CurrentTerminals.Count > 1)//identifier usage within a rule or wire definition and there is conflict
                {
                    //first consider wire definition
                    for (int n = context.ParserStack.Count - 1; n >= 0; n--)
                    {
                        if (context.ParserStack[n].Token != null && context.ParserStack[n].Token.Terminal != null)
                        {
                            if (context.ParserStack[n].Token.Terminal.Name == ".")
                            {
                                Terminal termMatch = null;
                                string compName = context.ParserStack[n - 1].Token.Text + "." + context.CurrentToken.Text;
                                foreach (var term in context.CurrentTerminals)
                                {
                                    if (context.Values.ContainsKey(compName)) //generate composite identifier name
                                    {
                                        if (((Terminal)context.Values[compName]).Equals(term))
                                        {
                                            termMatch = term;
                                            break;
                                        }
                                    }
                                }
                                if (termMatch != null)
                                {
                                    context.CurrentTerminals.Clear();
                                    context.CurrentTerminals.Add(termMatch);
                                    context.CurrentToken.SetTerminal(termMatch);
                                    return;
                                }
                            }
                        }
                    }
                    string localIO = string.Empty;
                    //look up the stack to find any local IO
                    for (int n = context.ParserStack.Count - 1; n >= 0; n--)
                    {
                        if (context.ParserStack[n].Token != null && context.ParserStack[n].Token.Terminal != null)
                        {
                            if (IOTokens.Contains(context.ParserStack[n].Token.Terminal.Name))
                            {
                                localIO = context.ParserStack[n].Token.Text;
                                break;
                            }
                        }
                    }
                    //match the name, for inputs, outputs etc, 
                    string globalIOName = context.CurrentToken.Text;
                    if (context.Values.ContainsKey(globalIOName))
                    {
                        if (context.CurrentTerminals.Contains((Terminal)context.Values[globalIOName]))
                        {
                            context.CurrentTerminals.Clear();
                            context.CurrentTerminals.Add((Terminal)context.Values[globalIOName]);
                            context.CurrentToken.SetTerminal((Terminal)context.Values[globalIOName]);
                        }
                        else
                        {
                            context.AddParserError("Wrong IO Type");
                        }

                    }
                    else if (context.Values.ContainsKey(context.CurrentToken.Text))//for 
                    {
                        context.CurrentTerminals.Clear();
                        context.CurrentTerminals.Add((Terminal)context.Values[context.CurrentToken.Text]);
                        context.CurrentToken.SetTerminal((Terminal)context.Values[context.CurrentToken.Text]);
                    }
                    else //or the qualified name for sets and categories
                    {//this is rulsetname.ioname.setname or rulsetname.ioname.catname
                        QualifiedName = localIO + "." + context.CurrentToken.Text;
                        if (context.Values.ContainsKey(QualifiedName))
                        {
                            if (context.CurrentToken.Terminal.Name == ((Terminal)context.Values[QualifiedName]).Name)
                            {//stop at the right one in the terminals list - this will be called for each.
                                context.CurrentTerminals.Clear();
                                context.CurrentTerminals.Add((Terminal)context.Values[QualifiedName]);
                            }
                        }
                        else
                        {
                            string possibilities = string.Empty;
                            bool io = true;
                            foreach (var s in context.CurrentTerminals)
                            {
                                if (EditorInfo.Type == TokenType.Identifier)
                                    possibilities += s.ErrorAlias + " ";
                                else
                                {
                                    io = false;
                                }
                            }
                            if (io)
                            {
                                context.AddParserError("Use of undeclared I/O: {0}, possible types: {1}", context.CurrentToken.Text, possibilities);
                                context.CurrentTerminals.Clear();
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {

                context.AddParserError("General error: {0}", ex.Message);
                context.CurrentTerminals.Clear();
            }
        }

    }
}
