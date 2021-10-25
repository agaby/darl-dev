using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using System.Xml.Linq;

namespace Datl.Language
{
    public class WordProcessSDK : DatlProcess
    {

        public static XNamespace XMLNS { get { return XNamespace.Get(WORDMLNS); } }

        public static string WORDMLNS = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

        protected override string separator { get; set; } = "__";

        private MemoryStream _workingMemoryStream;

        public override dynamic CreateDest(dynamic source)
        {
            var s = source as UnmanagedMemoryStream;
            _workingMemoryStream = new MemoryStream();
            s.CopyTo(_workingMemoryStream);
            var doc = WordprocessingDocument.Open(_workingMemoryStream, true);
            source.Dispose();
            return doc;
        }

        public override bool IsEndBlock(dynamic block)
        {
            return true;
        }

        public override bool IsReplaceBlock(dynamic block)
        {
            return false; //not needed
        }

        public override bool IsStartBlock(dynamic block)
        {
            return true;
        }

        public override List<dynamic> FindBlocks(dynamic source)
        {
            var doc = source as WordprocessingDocument;
            return doc.MainDocumentPart.Document.Descendants<BookmarkStart>().ToList<dynamic>();  
        }

        public override void PreProcess(dynamic source, Dictionary<string, string> data)
        {
            var doc = source as WordprocessingDocument;
            MailMerge(source, data);
        }

        public override string ExtractVariableName(dynamic block)
        {
            var bl = block as BookmarkStart;
            return bl.Name;
        }

        public override string ExtractVariableName(dynamic block, out string category)
        {
            category = string.Empty;
            string s = ExtractVariableName(block);
            if (s.Contains(separator))
            {
                int index = s.IndexOf(separator);
                category = s.Substring(index + separator.Length);
                return s.Substring(0, index);
            }
            return s;
        }

        public override dynamic PostProcess(dynamic source)
        {
            var doc = source as WordprocessingDocument;
            doc.Close();
            return _workingMemoryStream;
        }


        private static void ReplaceBookmarkParagraphs(MainDocumentPart doc, string bookmark, IEnumerable<OpenXmlElement> paras)
        {
            var start = doc.Document.Descendants<BookmarkStart>().Where(x => x.Name == bookmark).First();
            var end = doc.Document.Descendants<BookmarkEnd>().Where(x => x.Id.Value == start.Id.Value).First();
            OpenXmlElement current = start;
            var done = false;

            while (!done && current != null)
            {
                OpenXmlElement next;
                next = current.NextSibling();

                if (next == null)
                {
                    var parentNext = current.Parent.NextSibling();
                    while (!parentNext.HasChildren)
                    {
                        var toRemove = parentNext;
                        parentNext = parentNext.NextSibling();
                        toRemove.Remove();
                    }
                    next = current.Parent.NextSibling().FirstChild;

                    current.Parent.Remove();
                }

                if (next is BookmarkEnd)
                {
                    BookmarkEnd maybeEnd = (BookmarkEnd)next;
                    if (maybeEnd.Id.Value == start.Id.Value)
                    {
                        done = true;
                    }
                }
                if (current != start)
                {
                    current.Remove();
                }

                current = next;
            }

            foreach (var p in paras)
            {
                end.Parent.InsertBeforeSelf(p);
            }
        }

        private static void MailMerge(WordprocessingDocument wordDocument, Dictionary<string,string> values)
        {
            XElement newBody = XElement.Parse(wordDocument.MainDocumentPart.Document.Body.OuterXml);

            // Get all Mail Merge Fields
            IList<XElement> mailMergeFields =
                (from el in newBody.Descendants()
                 where el.Attribute(XMLNS + "instr") != null
                 select el).ToList();

            // Replace all merge fields with Data
            foreach (XElement field in mailMergeFields)
            {
                string fieldName = field.Attribute(XMLNS + "instr").Value.Replace("\\* MERGEFORMAT", string.Empty).Replace("MERGEFIELD", string.Empty).Trim();
                if (values.ContainsKey(fieldName))
                {
                    XElement newElement = field.Descendants(XMLNS + "r").First();
                    newElement.Descendants(XMLNS + "t").First().Value = values[fieldName];
                    field.ReplaceWith(newElement);
                }
            }
            wordDocument.MainDocumentPart.Document.Body = new Body(newBody.ToString());
            wordDocument.MainDocumentPart.Document.Save();

            // Delete MailMerge Data Source Part
            DocumentSettingsPart settingsPart = wordDocument.MainDocumentPart.GetPartsOfType<DocumentSettingsPart>().First();
            MailMergeRecipientDataPart mmrPart = settingsPart.GetPartsOfType<MailMergeRecipientDataPart>().FirstOrDefault();
            if(mmrPart != null)
                settingsPart.DeletePart(mmrPart);

            // Delete refrence to Mail Merge Data sources
           /* XElement settings = XElement.Parse(settingsPart.RootElement.OuterXml);

            IList<XElement> mailMergeElements =
                (from el in settings.Descendants()
                 where el.Name == (XMLNS + "mailMerge")
                 select el).ToList();

            foreach (XElement field in mailMergeElements)
            {
                field.Remove();
            }

            settingsPart.RootElement.InnerXml = settings.ToString();
            settingsPart.RootElement.Save();*/
        }
    }
}
