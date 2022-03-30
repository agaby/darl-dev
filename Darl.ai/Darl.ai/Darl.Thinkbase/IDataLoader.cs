using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.Thinkbase
{
    public  interface IDataLoader
    {
        List<KnowledgeState> LoadCsvData(string userId, string name, IGraphModel model, string data, string patternPath, List<DataMap> dataMaps);

        List<KnowledgeState> LoadXMLData(string userId, string name, IGraphModel model, string data, string patternPath, List<DataMap> dataMaps);

        List<KnowledgeState> LoadJsonData(string userId, string name, IGraphModel model, string data, string patternPath, List<DataMap> dataMaps);
    }
}
