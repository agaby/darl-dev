/// <summary>
/// </summary>

﻿using GemBox.Document;
using System.Collections.Generic;
using System.IO;

namespace Datl.Language
{
    public class WordProcessGem : DatlProcess
    {

        protected override string separator { get; set; } = "__";

        public override dynamic CreateDest(dynamic source)
        {
            ComponentInfo.SetLicense("FREE-LIMITED-KEY");
            ComponentInfo.FreeLimitReached += (sender, e) => e.FreeLimitReachedAction = FreeLimitReachedAction.ContinueAsTrial;

            DocumentModel document = DocumentModel.Load(source as Stream, LoadOptions.DocxDefault);
            return document;

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
            var doc = source as DocumentModel;
            var list = new List<dynamic>();
            foreach (var bm in doc.Bookmarks)
            {
                list.Add(bm);
            }
            return list;
        }

        public override void PreProcess(dynamic source, Dictionary<string, string> data)
        {
            var doc = source as DocumentModel;

            var p = new Dictionary<string, object>();

            foreach (var q in data.Keys)
            {
                p.Add(q, data[q]);
            }
            doc.MailMerge.Execute(p, string.Empty);
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
                category = s.Substring(index + separator.Length);
                if (category.Contains(separator)) //category has anti-duplicate extension - remove.
                {
                    var ind = category.IndexOf(separator);
                    category = category.Substring(0, ind);
                }
                return s.Substring(0, index);
            }
            return s;
        }

        public override dynamic PostProcess(dynamic source)
        {
            var doc = source as DocumentModel;
            doc.Bookmarks.Clear();
            MemoryStream ms = new MemoryStream();
            doc.Save(ms, SaveOptions.DocxDefault);
            ms.Position = 0;
            return ms;
        }

        public override void RemoveSection(dynamic output, dynamic startblock, dynamic endblock)
        {
            try
            {
                var bm = endblock as Bookmark;
                if (bm.Start != null)
                    bm.GetContent(true).Delete();
            }
            catch
            {

            }
        }

    }

}
