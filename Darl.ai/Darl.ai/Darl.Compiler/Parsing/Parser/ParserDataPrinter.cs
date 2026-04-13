/// <summary>
/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="ParserDataPrinter.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using DarlCompiler.Parsing.Construction;
using System;
using System.Linq;
using System.Text;

namespace DarlCompiler.Parsing
{
    /// <summary>
    /// Class ParserDataPrinter.
    /// </summary>
    public static class ParserDataPrinter
    {

        /// <summary>
        /// Prints the state list.
        /// </summary>
        /// <param name="language">The language.</param>
        /// <returns>System.String.</returns>
        public static string PrintStateList(LanguageData language)
        {
            StringBuilder sb = new StringBuilder();
            foreach (ParserState state in language.ParserData.States)
            {
                sb.Append("State " + state.Name);
                if (state.BuilderData.IsInadequate) sb.Append(" (Inadequate)");
                sb.AppendLine();
                var srConflicts = state.BuilderData.GetShiftReduceConflicts();
                if (srConflicts.Count > 0)
                    sb.AppendLine("  Shift-reduce conflicts on inputs: " + srConflicts.ToString());
                var ssConflicts = state.BuilderData.GetReduceReduceConflicts();
                if (ssConflicts.Count > 0)
                    sb.AppendLine("  Reduce-reduce conflicts on inputs: " + ssConflicts.ToString());
                //LRItems
                if (state.BuilderData.ShiftItems.Count > 0)
                {
                    sb.AppendLine("  Shift items:");
                    foreach (var item in state.BuilderData.ShiftItems)
                        sb.AppendLine("    " + item.ToString());
                }
                if (state.BuilderData.ReduceItems.Count > 0)
                {
                    sb.AppendLine("  Reduce items:");
                    foreach (LRItem item in state.BuilderData.ReduceItems)
                    {
                        var sItem = item.ToString();
                        if (item.Lookaheads.Count > 0)
                            sItem += " [" + item.Lookaheads.ToString() + "]";
                        sb.AppendLine("    " + sItem);
                    }
                }
                sb.Append("  Transitions: ");
                bool atFirst = true;
                foreach (BnfTerm key in state.Actions.Keys)
                {
                    var action = state.Actions[key] as ShiftParserAction;
                    if (action == null)
                        continue;
                    if (!atFirst) sb.Append(", ");
                    atFirst = false;
                    sb.Append(key.ToString());
                    sb.Append("->");
                    sb.Append(action.NewState.Name);
                }
                sb.AppendLine();
                sb.AppendLine();
            }//foreach
            return sb.ToString();
        }

        /// <summary>
        /// Prints the terminals.
        /// </summary>
        /// <param name="language">The language.</param>
        /// <returns>System.String.</returns>
        public static string PrintTerminals(LanguageData language)
        {
            var termList = language.GrammarData.Terminals.ToList();
            termList.Sort((x, y) => string.Compare(x.Name, y.Name));
            var result = string.Join(Environment.NewLine, termList);
            return result;
        }

        /// <summary>
        /// Prints the non terminals.
        /// </summary>
        /// <param name="language">The language.</param>
        /// <returns>System.String.</returns>
        public static string PrintNonTerminals(LanguageData language)
        {
            StringBuilder sb = new StringBuilder();
            var ntList = language.GrammarData.NonTerminals.ToList();
            ntList.Sort((x, y) => string.Compare(x.Name, y.Name));
            foreach (var nt in ntList)
            {
                sb.Append(nt.Name);
                sb.Append(nt.Flags.IsSet(TermFlags.IsNullable) ? "  (Nullable) " : string.Empty);
                sb.AppendLine();
                foreach (Production pr in nt.Productions)
                {
                    sb.Append("   ");
                    sb.AppendLine(pr.ToString());
                }
            }//foreachc nt
            return sb.ToString();
        }

    }
}
