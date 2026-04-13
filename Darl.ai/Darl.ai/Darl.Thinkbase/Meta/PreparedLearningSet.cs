/// <summary>
/// </summary>

﻿using DarlCompiler.Parsing;
using System.Collections.Generic;

namespace Darl.Thinkbase.Meta
{
    public class PreparedLearningSet
    {
        public string targetNodeId { get; set; }

        public Dictionary<string, string> inputs { get; set; } = new Dictionary<string, string>();

        public string outputPath { get; set; } = string.Empty;
        public string ruleset { get; set; } = "";

        //check for ruleset with matching name to load.
        public MetaRootNode rroot { get; set; }
        public SourceSpan ruleSetContents { get; set; }


        public int patternCount { get; set; }

        public List<IODefinitionNode> inps { get; set; }

        public OutputDefinitionNode outp { get; set; }

        public List<OutputDefinitionNode> outps { get; set; }

        public List<OutputAsInputDefinitionNode> outAsInps { get; set; }

        public int sets { get; set; }

        public List<int> inSamplePatterns { get; set; } = new List<int>();
        public List<int> outSamplePatterns { get; set; } = new List<int>();
        public Dictionary<string, List<DarlResult>> data { get; set; }

        public List<KnowledgeState> knowledgeStates { get; set; }

        public GraphObject targetNode { get; set; }

        public IGraphModel model { get; set; }

        public int percentTrain { get; set; }
        public string targetLineage { get; internal set; }
        public string valueLineage { get; internal set; }
    }
}
