/// <summary>
/// IDataLoader.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System.Collections.Generic;

namespace Darl.Thinkbase
{
    public interface IDataLoader
    {
        List<KnowledgeState> LoadCsvData(string userId, string name, IGraphModel model, string data, string patternPath, List<DataMap> dataMaps);

        List<KnowledgeState> LoadXMLData(string userId, string name, IGraphModel model, string data, string patternPath, List<DataMap> dataMaps);

        List<KnowledgeState> LoadJsonData(string userId, string name, IGraphModel model, string data, string patternPath, List<DataMap> dataMaps);
    }
}
