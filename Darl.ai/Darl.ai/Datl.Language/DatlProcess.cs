/// <summary>
/// DatlProcess.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System.Collections.Generic;
using System.Linq;

namespace Datl.Language
{
    /// <summary>
    /// Provides a framework for processing datl for a variety of media, such as text, word documents, etc.
    /// <remarks>Additive processes are ones where you take a source document and build a new one by copying over the appropriate parts from the source.
    /// Subtractive processes are where you operate on an existing document, replacing or removing sections.
    /// </remarks>
    /// </summary>
    public class DatlProcess
    {
        protected static string blockRegex = "%%.+?%%";
        protected static char startBlockTag = '{';
        protected static char endBlockTag = '}';
        protected virtual string separator { get; set; } = ".";


        /// <summary>
        /// override to find all the datl control blocks in a document, in document order.
        /// </summary>
        /// <returns>the list</returns>
        public virtual List<dynamic> FindBlocks(dynamic source)
        {
            return null;
        }

        /// <summary>
        /// is this a block representing the start of a conditional section?
        /// </summary>
        /// <param name="block">The block</param>
        /// <returns>True if start block</returns>
        public virtual bool IsStartBlock(dynamic block)
        {
            return false;
        }

        /// <summary>
        /// is this a block representing the end of a conditional section?
        /// </summary>
        /// <param name="block">the block</param>
        /// <returns>True if end block</returns>
        public virtual bool IsEndBlock(dynamic block)
        {
            return false;
        }

        /// <summary>
        /// Does this block represent a simple text replacement?
        /// </summary>
        /// <param name="block">The block</param>
        /// <returns>true if text replacement</returns>
        public virtual bool IsReplaceBlock(dynamic block)
        {
            return false;
        }

        /// <summary>
        /// Perform a text replacement
        /// </summary>
        /// <param name="output">The document or stream</param>
        /// <param name="block">The block</param>
        /// <param name="value">The value to replace</param>
        public virtual void ReplaceBlock(dynamic output, dynamic block, string value)
        {

        }

        /// <summary>
        /// Extract a simple variable name
        /// </summary>
        /// <param name="block">The block</param>
        /// <returns>The name</returns>
        public virtual string ExtractVariableName(dynamic block)
        {
            return string.Empty;
        }

        protected string ExtractVariableName(string s)
        {
            s = s.Remove(0, 2);
            s = s.Remove(s.Count() - 2);
            s = s.Replace(startBlockTag, ' ');
            s = s.Replace(endBlockTag, ' ');
            s = s.Trim();
            return s;
        }

        /// <summary>
        /// Extract a variable name and category
        /// </summary>
        /// <param name="block">The block</param>
        /// <param name="category">The category, or string.Empty if no category</param>
        /// <returns>The name</returns>
        public virtual string ExtractVariableName(dynamic block, out string category)
        {
            category = string.Empty;
            string s = ExtractVariableName(block);
            if (s.Contains(separator))
            {
                var sections = s.Split(separator[0]);
                category = sections[1];
                return sections[0];
            }
            return s;
        }

        /// <summary>
        /// For additive processes override to write a conditional section, subtractive ignore
        /// </summary>
        /// <param name="output"> The stream or document</param>
        /// <param name="startblock">The location of the first block. If null the start of the document</param>
        /// <param name="endblock">The location of the end block. If null the end of the document</param>
        public virtual void WriteSection(dynamic output, dynamic startblock, dynamic endblock)
        {

        }

        /// <summary>
        /// For subtractive processes override to remove a conditional section, additive ignore
        /// </summary>
        /// <param name="output"></param>
        /// <param name="startblock"></param>
        /// <param name="endblock"></param>
        public virtual void RemoveSection(dynamic output, dynamic startblock, dynamic endblock)
        {

        }

        /// <summary>
        /// For additive processes overide this to create the destination document
        /// </summary>
        /// <param name="source">The source document</param>
        /// <returns>the destination document</returns>
        public virtual dynamic CreateDest(dynamic source)
        {
            return source;
        }


        public virtual void ReportError(dynamic block, string error)
        {

        }

        public virtual dynamic PostProcess(dynamic source)
        {
            return source;
        }

        /// <summary>
        /// Process a document, replace values and infer conditional text
        /// </summary>
        /// <param name="source"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public dynamic Parse(dynamic source, Dictionary<string, string> values)
        {
            var dest = CreateDest(source);
            PreProcess(dest, values);
            var stack = new Stack<DocState>();
            var blocks = FindBlocks(dest);
            dynamic startBlock = null;
            foreach (var block in blocks)
            {
                bool active = stack.Count == 0 ? true : stack.Peek().active;
                if (active)
                {
                    WriteSection(dest, startBlock, block); //write all text up to start of block from last block
                }
                string category = string.Empty;
                var variable = ExtractVariableName(block, out category);
                if (IsStartBlock(block))
                {
                    if (string.IsNullOrEmpty(category))
                    {
                        if (!values.ContainsKey(variable) || string.IsNullOrEmpty(values[variable]))
                        {
                            active = false;
                        }
                    }
                    else
                    {
                        if (values.ContainsKey(variable))
                        {
                            if (values[variable] != category)
                            {
                                active = false;
                            }
                        }
                        else
                        {
                            active = false;
                        }
                    }
                    stack.Push(new DocState { name = $"{variable}{separator}{category}", active = active });
                }
                if (IsEndBlock(block))
                {
                    //three scenarios, the variable is on the top of the stack, or the writer has forgotten to close a variable or the variable is misspelled so impossible to take off.
                    //so if the stack includes this variable pop until it is popped, otherwise wait for the next unstacking.
                    if (stack.Peek().name == $"{variable}{separator}{category}")
                    {
                        stack.Pop();
                        if (!active && (stack.Count == 0 ? true : stack.Peek().active))
                            RemoveSection(dest, startBlock, block); //subtractive processes

                    }
                    else
                    {
                        ReportError(block, $"Unbalanced closing tag on {variable}");
                    }

                }
                if (IsReplaceBlock(block))
                {
                    if (values.ContainsKey(variable))
                    {
                        if (active)
                            ReplaceBlock(dest, block, values[variable]);
                    }

                }
                startBlock = block;
            }
            WriteSection(dest, startBlock, null); //write the section from the last block to the end
            if (stack.Count != 0)
            {
                ReportError(startBlock, $"Missing section terminator for {stack.Peek().name} ");
            }
            return PostProcess(dest);
        }

        public virtual void PreProcess(dynamic source, Dictionary<string, string> values)
        {

        }
    }

    public class DocState
    {
        public string name { get; set; }
        public bool active { get; set; }
    }
}
