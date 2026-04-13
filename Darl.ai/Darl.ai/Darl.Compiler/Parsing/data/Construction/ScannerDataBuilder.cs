/// <summary>
/// ScannerDataBuilder.cs - Core module for the Darl.dev project.
/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="ScannerDataBuilder.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using Darl.ai;
using System.Collections.Generic;

namespace DarlCompiler.Parsing.Construction
{
    /// <summary>
    /// Class ScannerDataBuilder.
    /// </summary>
    internal class ScannerDataBuilder
    {
        /// <summary>
        /// The _language
        /// </summary>
        readonly LanguageData _language;

        /// <summary>
        /// The _grammar
        /// </summary>
        readonly Grammar _grammar;

        /// <summary>
        /// The _grammar data
        /// </summary>
        readonly GrammarData _grammarData;
        /// <summary>
        /// The _data
        /// </summary>
        ScannerData _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScannerDataBuilder"/> class.
        /// </summary>
        /// <param name="language">The language.</param>
        internal ScannerDataBuilder(LanguageData language)
        {
            _language = language;
            _grammar = _language.Grammar;
            _grammarData = language.GrammarData;
        }

        /// <summary>
        /// Builds this instance.
        /// </summary>
        internal void Build()
        {
            _data = _language.ScannerData;
            InitMultilineTerminalsList();
            ProcessNonGrammarTerminals();
            BuildTerminalsLookupTable();
        }

        /// <summary>
        /// Initializes the multiline terminals list.
        /// </summary>
        private void InitMultilineTerminalsList()
        {
            foreach (var terminal in _grammarData.Terminals)
            {
                if (terminal.Flags.IsSet(TermFlags.IsNonScanner)) continue;
                if (terminal.Flags.IsSet(TermFlags.IsMultiline))
                {
                    _data.MultilineTerminals.Add(terminal);
                    terminal.MultilineIndex = (byte)(_data.MultilineTerminals.Count);
                }
            }
        }

        /// <summary>
        /// Processes the non grammar terminals.
        /// </summary>
        private void ProcessNonGrammarTerminals()
        {
            foreach (var term in _grammar.NonGrammarTerminals)
            {
                var firsts = term.GetFirsts();
                if (firsts == null || firsts.Count == 0)
                {
                    _language.Errors.Add(GrammarErrorLevel.Error, null, Resources.ErrTerminalHasEmptyPrefix, term.Name);
                    continue;
                }
                AddTerminalToLookup(_data.NonGrammarTerminalsLookup, term, firsts);
            }

            //sort each list
            foreach (var list in _data.NonGrammarTerminalsLookup.Values)
            {
                if (list.Count > 1)
                    list.Sort(Terminal.ByPriorityReverse);
            }
        }

        /// <summary>
        /// Builds the terminals lookup table.
        /// </summary>
        private void BuildTerminalsLookupTable()
        {
            foreach (Terminal term in _grammarData.Terminals)
            {
                //Non-grammar terminals are scanned in a separate step, before regular terminals; so we don't include them here
                if (term.Flags.IsSet(TermFlags.IsNonScanner | TermFlags.IsNonGrammar)) continue;
                var firsts = term.GetFirsts();
                if (firsts == null || firsts.Count == 0)
                {
                    _grammarData.NoPrefixTerminals.Add(term);
                    continue; //foreach term
                }
                AddTerminalToLookup(_data.TerminalsLookup, term, firsts);
            }

            if (_grammarData.NoPrefixTerminals.Count > 0)
            {
                //copy them to Scanner data
                _data.NoPrefixTerminals.AddRange(_grammarData.NoPrefixTerminals);
                // Sort in reverse priority order
                _data.NoPrefixTerminals.Sort(Terminal.ByPriorityReverse);
                //Now add Fallback terminals to every list, then sort lists by reverse priority
                // so that terminal with higher priority comes first in the list
                foreach (TerminalList list in _data.TerminalsLookup.Values)
                    foreach (var ft in _data.NoPrefixTerminals)
                        if (!list.Contains(ft))
                            list.Add(ft);
            }

            //Finally sort every list in terminals lookup table
            foreach (TerminalList list in _data.TerminalsLookup.Values)
                if (list.Count > 1)
                    list.Sort(Terminal.ByPriorityReverse);

        }

        /// <summary>
        /// Adds the terminal to lookup.
        /// </summary>
        /// <param name="_lookup">The _lookup.</param>
        /// <param name="term">The term.</param>
        /// <param name="firsts">The firsts.</param>
        private void AddTerminalToLookup(TerminalLookupTable _lookup, Terminal term, IList<string> firsts)
        {
            foreach (string prefix in firsts)
            {
                if (string.IsNullOrEmpty(prefix))
                {
                    _language.Errors.Add(GrammarErrorLevel.Error, null, Resources.ErrTerminalHasEmptyPrefix, term.Name);
                    continue;
                }
                //Calculate hash key for the prefix
                char firstChar = prefix[0];
                if (_grammar.CaseSensitive)
                    AddTerminalToLookupByFirstChar(_lookup, term, firstChar);
                else
                {
                    AddTerminalToLookupByFirstChar(_lookup, term, char.ToLower(firstChar));
                    AddTerminalToLookupByFirstChar(_lookup, term, char.ToUpper(firstChar));
                }
            }
        }

        /// <summary>
        /// Adds the terminal to lookup by first character.
        /// </summary>
        /// <param name="_lookup">The _lookup.</param>
        /// <param name="term">The term.</param>
        /// <param name="firstChar">The first character.</param>
        private void AddTerminalToLookupByFirstChar(TerminalLookupTable _lookup, Terminal term, char firstChar)
        {
            TerminalList currentList;
            if (!_lookup.TryGetValue(firstChar, out currentList))
            {
                //if list does not exist yet, create it
                currentList = new TerminalList();
                _lookup[firstChar] = currentList;
            }
            //add terminal to the list
            if (!currentList.Contains(term))
                currentList.Add(term);

        }

    }

}
