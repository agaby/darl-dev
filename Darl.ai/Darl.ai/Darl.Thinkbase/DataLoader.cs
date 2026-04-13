/// <summary>
/// DataLoader.cs - Core module for the Darl.dev project.
/// </summary>

﻿using CsvHelper;
using Darl.Thinkbase.Meta;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Darl.Thinkbase
{
    public class DataLoader : IDataLoader
    {
        private IMetaStructureHandler _metaHandler;

        public DataLoader(IMetaStructureHandler metaHandler)
        {
            _metaHandler = metaHandler;
        }

        public List<KnowledgeState> LoadCsvData(string userId, string name, IGraphModel model, string data, string patternPath, List<DataMap> dataMaps)
        {
            var kstates = new List<KnowledgeState>();
            FixDataMaps(dataMaps, model);
            try
            {
                using (var reader = new StringReader(data))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        var ks = new KnowledgeState { userId = userId, knowledgeGraphName = name, subjectId = Guid.NewGuid().ToString(), created = DateTime.UtcNow };
                        foreach (var d in dataMaps)
                        {
                            var att = model.vertices.ContainsKey(d.objId) ? model.vertices[d.objId].GetAttribute(d.attLineage) : null;
                            if (att == null)
                                throw new MetaRuleException($"ObjectId {d.objId} or attribute lineage {d.attLineage} don't point to an existing attribute.");
                            var value = csv.GetField(d.relPath);
                            if (value != null)
                            {
                                var newAtt = new GraphAttribute(att);
                                newAtt.value = value;
                                ks.AddAttribute(d.objId, newAtt);
                            }
                        }
                        kstates.Add(ks);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new MetaRuleException($"Cannot parse CSV.", ex);
            }
            return kstates;
        }
        public List<KnowledgeState> LoadXMLData(string userId, string name, IGraphModel model, string data, string patternPath, List<DataMap> dataMaps)
        {
            var kstates = new List<KnowledgeState>();
            //fix any string references to externalIds or typewords
            try
            {
                XDocument doc = XDocument.Parse(data);
                FixDataMaps(dataMaps, model);
                var xPatterns = doc.XPathSelectElements(patternPath);
                foreach (var xPattern in xPatterns)
                {
                    var ks = new KnowledgeState { userId = userId, knowledgeGraphName = name, subjectId = Guid.NewGuid().ToString(), created = DateTime.UtcNow };
                    foreach (var d in dataMaps)
                    {
                        var att = model.vertices.ContainsKey(d.objId) ? model.vertices[d.objId].GetAttribute(d.attLineage) : null;
                        if (att == null)
                            throw new MetaRuleException($"ObjectId {d.objId} or attribute lineage {d.attLineage} don't point to an existing attribute.");
                        var value = xPattern.XPathSelectElement(d.relPath);
                        if (value != null)
                        {
                            var newAtt = new GraphAttribute(att);
                            newAtt.value = value.Value;
                            ks.AddAttribute(d.objId, newAtt);
                        }
                    }
                    kstates.Add(ks);
                }
            }
            catch (Exception ex)
            {
                throw new MetaRuleException($"Cannot parse XML.", ex);
            }
            return kstates;
        }

        public List<KnowledgeState> LoadJsonData(string userId, string name, IGraphModel model, string data, string patternPath, List<DataMap> dataMaps)
        {
            var kstates = new List<KnowledgeState>();
            FixDataMaps(dataMaps, model);
            try
            {


                JObject doc = JObject.Parse(data);
                var jPatterns = doc.SelectTokens(patternPath);
                foreach (var jPattern in jPatterns)
                {
                    var ks = new KnowledgeState { userId = userId, knowledgeGraphName = name, subjectId = Guid.NewGuid().ToString(), created = DateTime.UtcNow };
                    foreach (var d in dataMaps)
                    {
                        var att = model.vertices.ContainsKey(d.objId) ? model.vertices[d.objId].GetAttribute(d.attLineage) : null;
                        if (att == null)
                            throw new MetaRuleException($"ObjectId {d.objId} or attribute lineage {d.attLineage} don't point to an existing attribute.");
                        var value = jPattern.SelectToken(d.relPath);
                        if (value != null)
                        {
                            var newAtt = new GraphAttribute(att);
                            newAtt.value = value.ToString();
                            ks.AddAttribute(d.objId, newAtt);
                        }
                    }
                    kstates.Add(ks);
                }
            }
            catch (Exception ex)
            {
                throw new MetaRuleException($"Cannot parse Json.", ex);
            }
            return kstates;
        }


        private void FixDataMaps(List<DataMap> dataMaps, IGraphModel model)
        {
            foreach (var k in dataMaps)
            {
                if (!model.vertices.ContainsKey(k.objId))
                {
                    var alt = model.vertices.Values.Where(a => a.externalId == k.objId).FirstOrDefault();
                    if (alt != null)
                    {
                        k.objId = alt.id ?? string.Empty;
                    }
                    else
                    {
                        throw new MetaRuleException($"ObjectId {k.objId} not found in the model.");
                    }
                }
                var att = model.vertices[k.objId].GetAttribute(k.attLineage);
                if (att == null) //check if it's a typeword
                {
                    if (_metaHandler.CommonLineages.TryGetValue(k.attLineage, out var lineage))
                    {
                        k.attLineage = lineage;
                    }
                    else
                    {
                        throw new MetaRuleException($"Attribute lineage {k.attLineage} not found in the model under object {k.objId}.");
                    }
                }
            }

        }

    }
}
