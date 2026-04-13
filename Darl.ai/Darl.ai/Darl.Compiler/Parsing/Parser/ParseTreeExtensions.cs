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
// <copyright file="ParseTreeExtensions.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.IO;
using System.Xml;

namespace DarlCompiler.Parsing
{
    /// <summary>
    /// Class ParseTreeExtensions.
    /// </summary>
    public static class ParseTreeExtensions
    {

        /// <summary>
        /// To the XML.
        /// </summary>
        /// <param name="parseTree">The parse tree.</param>
        /// <returns>System.String.</returns>
        public static string ToXml(this ParseTree parseTree)
        {
            if (parseTree == null || parseTree.Root == null) return string.Empty;
            var xdoc = ToXmlDocument(parseTree);
            StringWriter sw = new StringWriter();
            XmlTextWriter xw = new XmlTextWriter(sw);
            xw.Formatting = Formatting.Indented;
            xdoc.WriteTo(xw);
            xw.Flush();
            return sw.ToString();
        }

        /// <summary>
        /// To the XML document.
        /// </summary>
        /// <param name="parseTree">The parse tree.</param>
        /// <returns>XmlDocument.</returns>
        public static XmlDocument ToXmlDocument(this ParseTree parseTree)
        {
            var xdoc = new XmlDocument();
            if (parseTree == null || parseTree.Root == null) return xdoc;
            var xTree = xdoc.CreateElement("ParseTree");
            xdoc.AppendChild(xTree);
            var xRoot = parseTree.Root.ToXmlElement(xdoc);
            xTree.AppendChild(xRoot);
            return xdoc;
        }

        /// <summary>
        /// To the XML element.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="ownerDocument">The owner document.</param>
        /// <returns>XmlElement.</returns>
        public static XmlElement ToXmlElement(this ParseTreeNode node, XmlDocument ownerDocument)
        {
            var xElem = ownerDocument.CreateElement("Node");
            xElem.SetAttribute("Term", node.Term.Name);
            var term = node.Term;
            if (term.HasAstConfig() && term.AstConfig.NodeType != null)
                xElem.SetAttribute("AstNodeType", term.AstConfig.NodeType.Name);
            if (node.Token != null)
            {
                xElem.SetAttribute("Terminal", node.Term.GetType().Name);
                //xElem.SetAttribute("Text", node.Token.Text);
                if (node.Token.Value != null)
                    xElem.SetAttribute("Value", node.Token.Value.ToString());
            }
            else
                foreach (var child in node.ChildNodes)
                {
                    var xChild = child.ToXmlElement(ownerDocument);
                    xElem.AppendChild(xChild);
                }
            return xElem;
        }

    }
}
