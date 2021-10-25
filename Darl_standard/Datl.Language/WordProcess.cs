using Spire.Doc;
using Spire.Doc.Documents;
using Spire.Doc.Fields;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Datl.Language
{
    public class WordProcess : DatlProcess
    {

        protected override string separator { get; set; } = "__";
        protected BookmarksNavigator bn;

        public override dynamic CreateDest(dynamic source)
        {
            var doc = new Document();
            doc.LoadFromStream(source, FileFormat.Docx);
            source.Dispose();
            bn = new BookmarksNavigator(doc);
            return doc;
        }

        public override string ExtractVariableName(dynamic block)
        {
            var bl = block as Bookmark;
            return bl.Name;
        }

        public override string ExtractVariableName(dynamic block, out string category)
        {
            category = string.Empty;
            string s = ExtractVariableName(block);
            if (s.Contains(separator))
            {
                int index = s.IndexOf(separator);
                category = s.Substring(index +separator.Length);
                return s.Substring(0,index);
            }
            return s;
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

        public override void RemoveSection(dynamic output, dynamic startblock, dynamic endblock)
        {
            var doc = output as Document;
            Bookmark markStart = endblock as Bookmark;
            try
            {
                bn.MoveToBookmark(markStart.Name);
                bn.DeleteBookmarkContent(true);
            }
            catch
            {

            }
            doc.Bookmarks.Remove(markStart);
        }

        public override void PreProcess(dynamic source, Dictionary<string, string> data)
        {
            var doc = source as Document; //replace all the single variables
            var names = new List<string>();
            var values = new List<string>();
            foreach (var dv in data.Keys)
            {
                names.Add(dv);
                values.Add(data[dv]);
            }
            doc.MailMerge.Execute(names.ToArray(), values.ToArray());
        }

        public override List<dynamic> FindBlocks(dynamic source)
        {
            var doc = source as Document;
            var list = new List<dynamic>();
            foreach (var b in doc.Bookmarks)
                list.Add(b);
            return list;
        }

        public override void ReplaceBlock(dynamic output, dynamic block, string value)
        {
            var doc = output as Document;
            var bl = block as Bookmark;
        }

        public override dynamic PostProcess(dynamic source)
        {
            var doc = source as Document;
            MemoryStream ms = new MemoryStream();
            doc.SaveToStream(ms, FileFormat.Docx);
            ms.Position = 0;
            return ms;
        }

    }
}
