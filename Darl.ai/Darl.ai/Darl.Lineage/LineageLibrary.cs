using Chronic;
using Darl.Common;
using DarlCommon;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using static SymSpell;

namespace Darl.Lineage
{
    public static class LineageLibrary
    {
        private static readonly string source1 = ".Darl.Lineage.definitions.csv";
        private static readonly string source2 = ".Darl.Lineage.words.csv";
        private static readonly string source3 = ".Darl.Lineage.filteredAssociations.csv";
        private static readonly string source4 = ".Darl.Lineage.frequency_dictionary_en_82_765.txt";

        public static Dictionary<string, LineageRecord> lineages { get; } = new Dictionary<string, LineageRecord>();
        public static Dictionary<string, List<LineageRecord>> words { get; } = new Dictionary<string, List<LineageRecord>>();
        /// <summary>
        /// The maximum phrase length
        /// </summary>
        private static int maxPhraseLength { get; } = 3;

        private static readonly SymSpell symSpell = new SymSpell(82765, 2);

        /// <summary>
        /// Gets the suffixes.
        /// </summary>
        /// <value>The suffixes.</value>
        private static string[] suffixes { get { return new string[] { "s", "ses", "xes", "zes", "ches", "shes", "ies", "es", "es", "ed", "ed", "ing", "ing", "'s", "'", "er", "est", "st" }; } }

        private static string[] keywords { get { return new string[] { "noun", "verb", "adjective", "adverb", "punctuation", "conjunction", "article", "preposition", "postposition", "pronoun", "proper_noun", "auxiliary_verb", "mathsymbol", "negative_auxiliary" }; } }

        private static string[] ValuePlaceholders { get { return new string[] { "value:", "value:number", "value:number,integer", "value:number,float", "value:choice", "value:choice,boolean", "value:time", "value:date", "value:duration", "value:location", "value:currency", "default:", "value:text", "terminus:" }; } }
        /// <summary>
        /// Gets the endings.
        /// </summary>
        /// <value>The endings.</value>
        private static string[] endings { get { return new string[] { "", "s", "x", "z", "ch", "sh", "y", "e", "", "e", "", "e", "", "", "", "", "", "" }; } }

        //parsing numbers in text

        private static readonly Dictionary<string, int> cardinals = new Dictionary<string, int> { {"adjective:12014",0},{"adjective:12016", 1},{"adjective:12017",2},{"adjective:12018",3},
                                                                                        {"adjective:12019", 4},{"adjective:12020",5},{"adjective:12021", 6},{"adjective:12022", 7},
                                                                                        {"adjective:12023", 8},{"adjective:12024", 9},{"adjective:12025",10},{"adjective:12026", 11},
                                                                                        {"adjective:12027", 12},  {"adjective:12028", 13},   {"adjective:12029", 14},{"adjective:12030",15},
                                                                                        {"adjective:12031", 16}, {"adjective:12032", 17}, {"adjective:12033", 18},  {"adjective:12034", 19},    {"adjective:12035", 20},
                                                                                        {"adjective:12036",21},     {"adjective:12037",22},     {"adjective:12038",23},     {"adjective:12039",24},
                                                                                        {"adjective:12040",25},{"adjective:12041",26},{"adjective:12042",27},{"adjective:12043",28},
                                                                                        {"adjective:12044",29},{"adjective:12045", 30}, {"adjective:12046",31}, {"adjective:12047",32},
                                                                                        { "adjective:12048",33}, {"adjective:12049",34},{"adjective:12050",35},{"adjective:12051",36},
                                                                                        {"adjective:12052",37},{"adjective:12053",38},{"adjective:12054",39},{"adjective:12055", 40},
                                                                                        {"adjective:12056",41}, {"adjective:12057",42}, {"adjective:12058",43}, {"adjective:12059",44},
                                                                                        {"adjective:12060",45},{"adjective:12061",46},{"adjective:12062",47},{"adjective:12063",48},
                                                                                        {"adjective:12064",49},{"adjective:12065", 50}, {"adjective:12066",51}, {"adjective:12067",52},
                                                                                        {"adjective:12068",53}, {"adjective:12069",54},{"adjective:12070",55},{"adjective:12071",56},
                                                                                        {"adjective:12072",57},{"adjective:12073",58},{"adjective:12074",59},{"adjective:12075", 60},
                                                                                        {"adjective:12076",61}, {"adjective:12077",62}, {"adjective:12078",63}, {"adjective:12079",64},
                                                                                        {"adjective:12080",65},{"adjective:12081",66},{"adjective:12082",67},{"adjective:12083",68},
                                                                                        {"adjective:12084",69},{"adjective:12085", 70}, {"adjective:12086",71}, {"adjective:12087",72},
                                                                                        {"adjective:12088",73}, {"adjective:12089",74},{"adjective:12090",75},{"adjective:12091",76},
                                                                                        {"adjective:12092",77},{"adjective:12093",78},{"adjective:12094",79},{"adjective:12095", 80},
                                                                                        {"adjective:12096",81}, {"adjective:12097",82}, {"adjective:12098",83}, {"adjective:12099",84},
                                                                                        {"adjective:12100",85},{"adjective:12101",86},{"adjective:12102",87},{"adjective:12103",88},
                                                                                        {"adjective:12104",89},{"adjective:12105", 90}, {"adjective:12106",91}, {"adjective:12107",92},
                                                                                        {"adjective:12108",93}, {"adjective:12109",94},{"adjective:12110",95},{"adjective:12111",96},
                                                                                        {"adjective:12112",97},{"adjective:12113",98},{"adjective:12114",99},
                                                                                        {"adjective:12116", 101},{"adjective:12117",105},{"adjective:12118",110},{"adjective:12119",115},
                                                                                        {"adjective:12120",120},{"adjective:12121", 125}, {"adjective:12122", 130},  {"adjective:12123", 135},
                                                                                        { "adjective:12124", 140},    {"adjective:12125", 145},     {"adjective:12126", 150},      {"adjective:12127", 155},
                                                                                        { "adjective:12128", 160},        {"adjective:12129", 165},{"adjective:12130",170},{"adjective:12131", 175},
                                                                                        {"adjective:12132", 180},  {"adjective:12133", 190},   {"adjective:12134", 200},    {"adjective:12135", 300},
                                                                                        {"adjective:12136", 400},      {"adjective:12137", 500},{"adjective:12139",10000},
                                                                                        {"adjective:12140",100000} };


        private static readonly Dictionary<string, int> multipliers = new Dictionary<string, int> { { "adjective:12115", 100 }, { "adjective:12138", 1000 }, { "adjective:12141", 1000000 }, { "adjective:12142", 1000000000 }, { "adjective:12143", 1000000000 } };
        private static readonly List<string> punctuators = new List<string> { "punctuation:0" };
        private static readonly List<string> separators = new List<string> { "conjunction:25" };
        private static readonly List<string> indefiniteArticles = new List<string> { "article:4", "article:5" };

        //parsing time periods
        private static readonly Dictionary<string, TimeSpan> periods = new Dictionary<string, TimeSpan> { { "noun:01,5,15,00", new TimeSpan(1, 0, 0, 0) }, { "noun:01,5,15,04", new TimeSpan(30, 0, 0, 0) }, { "noun:01,5,03,3,045", new TimeSpan(365, 0, 0, 0) }, { "noun:01,5,15,07", new TimeSpan(1, 0, 0) }, { "noun:01,5,15,10", new TimeSpan(0, 1, 0) }, { "noun:01,5,15,12", new TimeSpan(0, 0, 1) }, { "noun:01,5,03,3,039", new TimeSpan(7, 0, 0, 0) } };

        private static readonly string locationRoot = "noun:99,";
        /// <summary>
        /// Constructor
        /// </summary>
        static LineageLibrary()
        {
            var assemblyName = Assembly.GetExecutingAssembly().FullName!.Split(',').First();
            List<string> columns = new List<string>();
            var stream1 = Assembly.GetExecutingAssembly().GetManifestResourceStream(assemblyName + source1);
            if (stream1 == null)
                throw new Exception($"Cannot find {source1} in the manifest of {Assembly.GetExecutingAssembly().FullName}");
            using (var reader = new CsvFileReader(stream1))
            {
                int index = 0;
                while (reader.ReadRow(columns))
                {
                    if (index > 0)
                    {
                        var lrec = new LineageRecord { description = columns[3], typeWord = columns[1], type = LineageType.concept, lineage = RemoveTraillingComma(columns[0]) };
                        lineages.Add(lrec.lineage, lrec);
                    }
                    index++;
                }
            }
            var stream2 = Assembly.GetExecutingAssembly().GetManifestResourceStream(assemblyName + source2);
            if (stream2 == null)
                throw new Exception($"Cannot find {source2} in the manifest of {Assembly.GetExecutingAssembly().FullName}");
            using (var reader = new CsvFileReader(stream2))
            {
                int index = 0;
                while (reader.ReadRow(columns))
                {
                    if (index > 0)
                    {
                        var linList = new List<LineageRecord>();
                        foreach (var s in JsonConvert.DeserializeObject<List<string>>(columns[3])!)
                        {
                            var p = RemoveTraillingComma(s);
                            if (lineages.ContainsKey(p))
                            {
                                linList.Add(lineages[p]);
                            }
                        }
                        words.Add(columns[0], linList);
                    }
                    index++;
                }
            }
            var stream3 = Assembly.GetExecutingAssembly().GetManifestResourceStream(assemblyName + source3);
            if (stream3 == null)
                throw new Exception($"Cannot find {source3} in the manifest of {Assembly.GetExecutingAssembly().FullName}");
            using (var reader = new CsvFileReader(stream3))
            {
                int index = 0;
                while (reader.ReadRow(columns))
                {
                    if (index > 0 && lineages.ContainsKey(columns[0]) && lineages.ContainsKey(columns[1]))
                    {
                        var start = lineages[columns[0]];
                        var end = lineages[columns[1]];
                        double weight = 0;
                        if (double.TryParse(columns[3], out weight))
                        {
                            var la = new LineageAssociation { start = start, end = end, weight = weight };
                            if (start.precedes == null)
                                start.precedes = new List<LineageAssociation>();
                            start.precedes.Add(la);
                            if (end.follows == null)
                                end.follows = new List<LineageAssociation>();
                            end.follows.Add(la);
                        }
                    }
                    index++;
                }
            }
            //now add value LineageRecord
            lineages.Add(ValuePlaceholders[0], new LineageRecord { lineage = ValuePlaceholders[0], description = "a generic value placeholder", type = LineageType.value, typeWord = "value" });
            lineages.Add(ValuePlaceholders[1], new LineageRecord { lineage = ValuePlaceholders[1], description = "a generic numeric value placeholder", type = LineageType.value, typeWord = "numeric value" });
            lineages.Add(ValuePlaceholders[2], new LineageRecord { lineage = ValuePlaceholders[2], description = "an integer value placeholder", type = LineageType.value, typeWord = "integer value" });
            lineages.Add(ValuePlaceholders[3], new LineageRecord { lineage = ValuePlaceholders[3], description = "a floating point value placeholder", type = LineageType.value, typeWord = "float value" });
            lineages.Add(ValuePlaceholders[4], new LineageRecord { lineage = ValuePlaceholders[4], description = "a choice value placeholder", type = LineageType.value, typeWord = "choice value" });
            lineages.Add(ValuePlaceholders[5], new LineageRecord { lineage = ValuePlaceholders[5], description = "a boolean value placeholder", type = LineageType.value, typeWord = "boolean value" });
            lineages.Add(ValuePlaceholders[6], new LineageRecord { lineage = ValuePlaceholders[6], description = "a time value placeholder", type = LineageType.value, typeWord = "time value" });
            lineages.Add(ValuePlaceholders[7], new LineageRecord { lineage = ValuePlaceholders[7], description = "a date value placeholder", type = LineageType.value, typeWord = "date value" });
            lineages.Add(ValuePlaceholders[8], new LineageRecord { lineage = ValuePlaceholders[8], description = "a duration value placeholder", type = LineageType.value, typeWord = "duration value" });
            lineages.Add(ValuePlaceholders[9], new LineageRecord { lineage = ValuePlaceholders[9], description = "a location value placeholder", type = LineageType.value, typeWord = "location value" });
            lineages.Add(ValuePlaceholders[10], new LineageRecord { lineage = ValuePlaceholders[10], description = "a currency value placeholder", type = LineageType.value, typeWord = "currency value" });
            lineages.Add(ValuePlaceholders[11], new LineageRecord { lineage = ValuePlaceholders[11], description = "a default response placeholder", type = LineageType.Default, typeWord = "default response" });
            lineages.Add(ValuePlaceholders[12], new LineageRecord { lineage = ValuePlaceholders[12], description = "a text placeholder", type = LineageType.value, typeWord = "text value" });

            var stream4 = Assembly.GetExecutingAssembly().GetManifestResourceStream(assemblyName + source4);
            if (stream4 == null)
                throw new Exception($"Cannot find {source4} in the manifest of {Assembly.GetExecutingAssembly().FullName}");
            symSpell.LoadDictionary(stream4, 0, 1);
        }

        private static string RemoveTraillingComma(string source)
        {
            if (source.EndsWith(","))
                return source.Substring(0, source.Count() - 1);
            return source;
        }

        /// <summary>
        /// Matches a single word against a list of lineages
        /// </summary>
        /// <param name="text">The word text</param>
        /// <param name="lineageArray">the list of lineages</param>
        /// <param name="conceptFrequency">optional lineage frequency capture</param>
        /// <returns></returns>
        public static bool Match(string text, List<string> lineageArray, Dictionary<string, int>? conceptFrequency = null)
        {
            var word = text.Trim().ToLower();
            var index = 0;
            var concepts = WordRecognizer(new List<string> { word }, ref index, true);
            if (conceptFrequency != null && concepts != null)
            {
                foreach (var c in concepts)
                {
                    if (conceptFrequency.ContainsKey(c.lineage))
                        conceptFrequency[c.lineage]++;
                    else
                        conceptFrequency.Add(c.lineage, 1);
                }
            }
            foreach (var lin in lineageArray)
            {
                if (concepts != null)
                {
                    foreach (var child in concepts)
                    {
                        if (child.lineage.StartsWith(lin))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Match a string against a lineage sequence
        /// </summary>
        /// <param name="text">The string to match</param>
        /// <param name="lineageArraySequence">the lineage array sequence</param>
        /// <param name="values">A repository for any values found</param>
        /// <returns></returns>
        public static bool Match(string text, List<List<string>> lineageArraySequence, out List<DarlVar> values)
        {
            Trace.WriteLine($"New Match, text: {text}");
            values = new List<DarlVar>();
            var wordList = SimpleTokenizer(text);
            int index = 0;
            var concepts = WordRecognizer(wordList, ref index);
            if (RecursiveMatch(wordList, index, lineageArraySequence, 0, values, concepts))
                return true;
            return false;
        }

        private static bool RecursiveMatch(List<string> wordList, int wordIndex, List<List<string>> lineageArraySequence, int sequenceIndex, List<DarlVar> values, List<LineageRecord>? currentConcepts)
        {
            LineageRecord? matchedConcept = null;
            if (sequenceIndex >= lineageArraySequence.Count)
            {
                //disambiguate here on the way back up the tree
                Trace.WriteLine($"Match succeeded");
                return true;
            }
            if (wordIndex > wordList.Count || currentConcepts == null) //was > 28/10/17
            {
                Trace.WriteLine($"Match failed");
                return false;
            }
            foreach (var lin in lineageArraySequence[sequenceIndex])
            {
                var val = lineages[lin];
                if (val.type == LineageType.value)
                {
                    wordIndex--;
                    var res = HandleValues(val.lineage, wordList, ref wordIndex, currentConcepts);
                    wordIndex++;
                    if (!res.unknown)
                    {
                        values.Add(res);
                        var concepts = WordRecognizer(wordList, ref wordIndex, true);//was false 28/10/17
                        return RecursiveMatch(wordList, wordIndex, lineageArraySequence, sequenceIndex + 1, values, concepts);
                    }

                }
                else
                {
                    foreach (var child in currentConcepts)
                    {
                        if (child.lineage.StartsWith(lin))
                        {
                            Trace.WriteLine($"Match with parent concept {lin}");
                            matchedConcept = lineages[lin];
                            var concepts = WordRecognizer(wordList, ref wordIndex, true); //was false 28/10/17
                            return RecursiveMatch(wordList, wordIndex, lineageArraySequence, sequenceIndex + 1, values, concepts);
                        }
                    }
                }
            }
            var cs = WordRecognizer(wordList, ref wordIndex, true);//was false 28/10/17
            return RecursiveMatch(wordList, wordIndex, lineageArraySequence, sequenceIndex, values, cs);
        }


        public static DarlVar HandleValues(string lineage, List<string> wordList, ref int wordIndex, List<LineageRecord> currentConcepts, string name = "", int tokens = 1)
        {
            Chronic.Parser.IsDebugMode = true;
            var parser = new Chronic.Parser();
            switch (lineage)
            {

                case "value:text":
                case "value:":
                    {
                        try
                        {
                            var res = new DarlVar() { unknown = false, Value = string.Join(" ", wordList.GetRange(wordIndex, tokens)), dataType = DarlVar.DataType.textual, name = name };
                            return res;
                        }
                        catch
                        {
                            return new DarlVar();
                        }
                    }
                case "value:number":
                case "value:number,integer":
                case "value:number,float":
                    {
                        double v;
                        if (double.TryParse(wordList[wordIndex], out v))//was wordindex - 1
                        {
                            Trace.WriteLine($"Match with Numeric Value as number {v.ToString()}");
                            return new DarlVar { dataType = DarlVar.DataType.numeric, Value = v.ToString(), unknown = false, weight = 1.0, values = new List<double> { v }, name = name };
                        }
                        else
                        {
                            var res = ConvertTextNumbers(wordList, ref wordIndex, currentConcepts);
                            if (!res.unknown)
                            {
                                Trace.WriteLine($"Match with Numeric Value as text {res.Value}");
                                return res;
                            }
                        }
                    }
                    break;
                case "value:choice":
                    break;
                case "value:choice,boolean":
                    break;
                case "value:time":
                case "value:date":
                    {
                        // chronic parser returns non-null only when the complete phrase can be converted without extraneous words.
                        Span? res = null;
                        int endIndex = wordIndex;
                        for (int n = wordIndex; n < wordList.Count; n++) //iterate through longer and longer phrases until parser returns null and select the longest.//was wordindex - 1
                        {
                            var p = parser.Parse(ConcatenateRest(wordList, wordIndex, n));//was wordindex - 1
                            if (p != null)
                            {
                                res = p;
                            }
                            if (res != null && p == null)
                            {
                                endIndex = n;
                                break;
                            }
                        }
                        if (res != null)
                        {
                            Trace.WriteLine($"Match with time value {res.Start.ToString()}");
                            return new DarlVar { dataType = DarlVar.DataType.date, times = new List<DarlTime> { new DarlTime(res.Start ?? DateTime.MinValue), new DarlTime(res.End ?? DateTime.MaxValue) }, Value = res.Start.ToString()!, unknown = false, weight = 1.0, name = name };
                        }
                    }
                    break;
                case "value:duration":
                    {
                        var res = ConvertDuration(wordList, ref wordIndex, currentConcepts);
                        if (!res.unknown)
                        {
                            Trace.WriteLine($"Match with duration {res.Value}");
                            return res;
                        }
                    }
                    break;
                case "value:location":
                    bool glueWord = false;
                    foreach (var c in currentConcepts)
                    {
                        if (!c.lineage.StartsWith("noun") && !c.lineage.StartsWith("adverb") && !c.lineage.StartsWith("adjective") && !c.lineage.StartsWith("verb"))
                        {
                            glueWord = true;
                            break;
                        }
                    }
                    if (!glueWord)
                    {
                        foreach (var c in currentConcepts)
                        {
                            if (c.lineage.StartsWith(locationRoot))
                            {
                                var text = c.typeWord;
                                Trace.WriteLine($"Match with location value {text}");
                                return new DarlVar { dataType = DarlVar.DataType.location, Value = text, unknown = false, weight = 1.0, name = name };
                            }
                        }
                    }
                    break;
            }
            return new DarlVar() { unknown = true };
        }

        private static DarlVar ConvertDuration(List<string> wordList, ref int wordIndex, List<LineageRecord>? currentConcepts)
        {
            //format is <integer> <period> [and <integer> <period>]
            // or a(n) <period>  i.e a day, a month...
            var res = new DarlVar { unknown = true, dataType = DarlVar.DataType.numeric, Value = "", name = "number in text", values = new List<double> { 0.0 }, weight = 0.0 };
            bool complete = false;
            TimeSpan sum = TimeSpan.Zero;
            TimeSpan subsum = TimeSpan.Zero;
            double multiplier = 0.0;
            var concepts = currentConcepts;
            bool numberFound = false;
            bool periodFound = false;
            while (!complete)
            {
                bool found = false; //indicates a valid token of either a number, a period or a connective has been found
                bool stepNeeded = true;
                if (!numberFound)
                {
                    double v;
                    if (double.TryParse(wordList[wordIndex - 1], out v)) //digits
                    {
                        Trace.WriteLine($"Found natural number {v.ToString()} in duration.");
                        multiplier = v;
                        numberFound = true;
                        found = true;
                    }
                    else if (concepts != null)
                    {
                        foreach (var c in concepts) // check it's not an indefinite article, i.e. a week.
                        {
                            if (indefiniteArticles.Contains(c.lineage))
                            {
                                Trace.WriteLine($"Found indefinite article in  in duration.");
                                multiplier = 1.0;
                                numberFound = true;
                                found = true;
                                break;
                            }
                        }
                        if (!found) //might be a separator i.e. five days {and} three hours
                        {
                            foreach (var c in concepts)
                            {
                                if (separators.Contains(c.lineage))
                                {
                                    Trace.WriteLine($"Found separator {c.typeWord}  in duration.");
                                    found = true;
                                    break;
                                }
                                else if (punctuators.Contains(c.lineage))
                                {
                                    Trace.WriteLine($"Found punctuator {c.typeWord}  in duration.");
                                    found = true;
                                    break;
                                }

                            }
                        }

                        if (!found) //then look for text in words
                        {
                            //stepNeeded = false;
                            var m = ConvertTextNumbers(wordList, ref wordIndex, concepts); //this moves the pointers and concepts on to the next word. no need to do that at the end of the loop.
                            if (!m.unknown)
                            {
                                Trace.WriteLine($"Found number in text {m.values[0].ToString()}  in duration.");
                                multiplier = m.values[0];
                                numberFound = true;
                                found = true;
                            }
                        }

                    }

                }
                else if (concepts != null)//process a period
                {
                    foreach (var c in concepts)
                    {
                        if (periods.ContainsKey(c.lineage))
                        {
                            if (subsum != TimeSpan.Zero)
                                sum += subsum;
                            Trace.WriteLine($"Found time period {c.typeWord}  in duration.");
                            subsum = new TimeSpan(periods[c.lineage].Ticks * (long)multiplier);
                            periodFound = true;
                            numberFound = false;
                            found = true;
                            break;
                        }

                    }
                }
                if (!found && concepts != null) //might be a separator i.e. five days {and} three hours
                {
                    foreach (var c in concepts)
                    {
                        if (separators.Contains(c.lineage))
                        {
                            Trace.WriteLine($"Found separator {c.typeWord}  in duration.");
                            found = true;
                            break;
                        }
                    }
                }
                if (!found || wordIndex >= wordList.Count)
                {
                    if (periodFound)
                    {
                        sum += subsum;
                        Trace.WriteLine($"period complete in duration. Period is {sum.ToString()}");
                        complete = true; //have left the period
                        res.times = new List<DarlTime>();
                        res.times.Add(new DarlTime(DateTime.MinValue));
                        res.times.Add(new DarlTime(DateTime.MinValue + sum));
                        res.Value = sum.ToString();
                        res.unknown = false;
                        res.weight = 1.0;
                    }
                    else
                    {
                        complete = true; //nothing found
                    }
                }
                else if (stepNeeded)
                    concepts = WordRecognizer(wordList, ref wordIndex, true);
            }
            return res;
        }

        private static string ConcatenateRest(List<string> wordList, int wordIndex, int maxIndex)
        {
            var sb = new StringBuilder();
            for (int n = wordIndex; n < Math.Min(wordList.Count, maxIndex + 1); n++)
            {
                sb.Append(wordList[n] + " ");
            }
            return sb.ToString().Trim();
        }

        enum TokenizerState { scanning, token, reported_speech };

        /// <summary>
        /// Simple tokenizer.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>List&lt;System.String&gt;.</returns>
        /// <remarks>Modify to emit reported speech as a single token.</remarks>
        public static List<string> SimpleTokenizer(string text)
        {
            List<string> tokens = new List<string>();
            TokenizerState inToken = TokenizerState.scanning;
            int tokenStart = 0;
            for (int n = 0; n < text.Length; n++)
            {
                char c = text[n];

                if (Char.IsLetterOrDigit(c) || Char.IsSymbol(c) || (c == '\'' && (inToken == TokenizerState.token || inToken == TokenizerState.reported_speech)) || (c == '/' && (inToken == TokenizerState.token || inToken == TokenizerState.reported_speech)))
                {

                    if (inToken == TokenizerState.scanning)
                    {
                        tokenStart = n;
                        inToken = TokenizerState.token;
                    }
                }
                else if (char.IsWhiteSpace(c) || c == '-')
                {
                    if (inToken == TokenizerState.token)
                    {
                        tokens.Add(text.Substring(tokenStart, n - tokenStart).ToLower());
                    }
                    if (inToken == TokenizerState.token)
                        inToken = TokenizerState.scanning;
                }
                else if (char.IsPunctuation(c))
                {
                    if (inToken == TokenizerState.token) //punctuation immediately following text, emit both
                    {
                        if (!((c == ',' || c == '.') && char.IsDigit(text[n - 1]) && char.IsDigit(text[Math.Min(n + 1, text.Count() - 1)]))) //detect commas and stops within numbers and ignore
                        {
                            tokens.Add(text.Substring(tokenStart, n - tokenStart).ToLower());
                            tokenStart = n;
                        }
                    }
                    else if (inToken == TokenizerState.reported_speech)
                    {
                        if (c == '"') // terminate reported speech
                        {
                            tokens.Add(text.Substring(tokenStart + 1, n - (tokenStart + 1)));//reported speech not converted to lower case.
                            inToken = TokenizerState.scanning;
                        }
                    }
                    else
                    {
                        if (c == '"') // terminate reported speech
                        {
                            inToken = TokenizerState.reported_speech;
                            tokenStart = n;
                        }
                        else
                        {
                            tokens.Add(text.Substring(n, 1));
                            inToken = TokenizerState.scanning;
                        }
                    }
                }
            }
            if (inToken == TokenizerState.token)//left over text without terminating punctuation
            {
                tokens.Add(text.Substring(tokenStart, text.Length - tokenStart).ToLower());
            }
            else if (inToken == TokenizerState.reported_speech)
            {
                tokens.Add(text.Substring(tokenStart, text.Length - tokenStart)); //reported speech not converted to lower case.
            }
            return tokens;
        }

        /// <summary>
        /// Recognizes and stems up to 3 word phrases, returning the raw lineages.
        /// </summary>
        /// <param name="wordList"></param>
        /// <param name="wordIndex"></param>
        /// <returns></returns>
        public static List<LineageRecord>? WordRecognizer(List<string> wordList, ref int wordIndex, bool single = false)
        {
            if (wordIndex >= wordList.Count)
                return null;
            HashSet<LineageRecord>? list = null;
            var t = wordList[wordIndex];
            if (t.Length == 1) //handle punctuation
            {
                var w = LookupWord(t);
                if (w != null)
                {
                    wordIndex++;
                    return w;
                }
            }
            bool found = false;
            for (int n = single ? 1 : maxPhraseLength; n > 0; n--)//note that this finds exact matches, but not matches where suffix changes are needed. also the source does not contain regular past participles. These are always wrongly matched as adjectives
            {
                int increment = 0;
                if (wordIndex + n <= wordList.Count) //this length of phrase is possible
                {
                    var sb = new StringBuilder();
                    for (int p = 0; p < n; p++)
                    {
                        sb.Append(wordList[wordIndex + p] + " ");
                    }
                    var token = sb.ToString().Trim();
                    var w = LookupWord(token);
                    if (w != null)
                    {
                        found = true;
                        //                        Trace.WriteLine($"Found word/phrase: {token}, length: {n} initial wordIndex: {wordIndex}, next wordIndex: {wordIndex + n}");
                        increment = n;
                        list = new HashSet<LineageRecord>();
                        foreach (var s in w)
                            list.Add(s);
                    }
                    if (n == 1) //consider matches minus suffixes even though a match may have been found. 
                    {
                        if (suffixes != null)//check for suffixes
                        {
                            for (int q = 0; q < suffixes.GetLength(0); q++)
                            {
                                if (token.EndsWith(suffixes[q]) && suffixes[q].Length < token.Length)
                                {
                                    // replace suffix with ending
                                    string temptoken = token.Substring(0, token.Length - suffixes[q].Length);
                                    temptoken = string.Concat(temptoken, endings[q]);
                                    var tt = LookupWord(temptoken);
                                    if (tt != null)
                                    {
                                        int offset = 1;
                                        //if this is the lemma, perhaps there's a two word match?
                                        if (wordIndex + 1 < wordList.Count)
                                        {
                                            var tt2 = LookupWord(temptoken + " " + wordList[wordIndex + 1]);
                                            if (tt2 != null)
                                            {
                                                offset++;
                                                tt = tt2;
                                                temptoken += " " + wordList[wordIndex + 1];
                                            }
                                        }
                                        if (list == null)
                                        {
                                            list = new HashSet<LineageRecord>();
                                        }
                                        foreach (var s in tt)
                                            list.Add(s);
                                        //                                        Trace.WriteLine($"Found word/phrase by stemming: {temptoken}, length: {n} initial wordIndex: {wordIndex}, next wordIndex: {wordIndex + offset}");
                                        increment = offset;
                                        found = true;
                                    }
                                }
                            }
                        }
                    }
                }
                if (found)
                {
                    wordIndex += increment;
                    break;
                }
            }
            if (!found)//not found any of the matches
            {
                //                Trace.WriteLine($"Not found word/phrase: {t}, length: {1} initial wordIndex: {wordIndex}, next wordIndex: {wordIndex + 1}");
                list = new HashSet<LineageRecord> { lineages["proper_noun:18"] };
                wordIndex++;
            }

            return list!.ToList();
        }

        public static List<LineageRecord>? LookupWord(string word)
        {
            if (!words.ContainsKey(word))
                return null;
            return words[word];
        }

        private static DarlVar ConvertTextNumbers(List<string> wordList, ref int wordIndex, List<LineageRecord>? currentConcepts)
        {
            var res = new DarlVar { unknown = true, dataType = DarlVar.DataType.numeric, Value = "", name = "number in text", values = new List<double> { 0.0 }, weight = 0.0 };
            bool complete = false;
            int sum = 0;
            int subsum = 0;
            var concepts = currentConcepts;
            List<LineageRecord>? oldConcepts = null;
            int oldWordIndex = 0; ;
            while (!complete)
            {

                bool found = false;
                foreach (var c in concepts ?? new List<LineageRecord>())
                {
                    if (cardinals.ContainsKey(c.lineage))
                    {
                        subsum += cardinals[c.lineage];
                        found = true;
                        break;
                    }
                    else if (multipliers.ContainsKey(c.lineage))
                    {
                        subsum *= multipliers[c.lineage];
                        found = true;
                        break;
                    }
                    else if (separators.Contains(c.lineage))
                    {
                        found = true;
                        break;
                    }
                    else if (punctuators.Contains(c.lineage))
                    {
                        sum += subsum;
                        subsum = 0;
                        found = true;
                        break;
                    }


                }
                if (!found || wordIndex >= wordList.Count)
                {
                    if (subsum != 0.0)
                    {
                        sum += subsum;
                        complete = true; //have left the number
                        res.values[0] = sum;
                        res.Value = sum.ToString();
                        res.unknown = false;
                        res.weight = 1.0;
                        if (!found)
                        {
                            currentConcepts = oldConcepts;
                            wordIndex = oldWordIndex;
                        }
                    }
                    else
                    {
                        complete = true; //have not found a number
                    }
                }
                else
                {
                    oldConcepts = currentConcepts;
                    oldWordIndex = wordIndex;
                    concepts = WordRecognizer(wordList, ref wordIndex, true);
                    currentConcepts = concepts;
                }

            }
            return res;
        }

        /// <summary>
        /// This is based on the levenshtein distance measure. The distance is turned into a possibility value
        /// via 1-d/l where d is distance and l is the max length of the two strings
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        /// <returns>A result scaled between 0 and 1 where 1 is identical</returns>
        public static double Similarity(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int p = Math.Max(n, m);
            int[,] d = new int[n + 1, m + 1];

            if (n == 0 || m == 0)
            {
                return 0.0;
            }

            for (int i = 0; i <= n; i++)
                d[i, 0] = i;
            for (int j = 0; j <= m; j++)
                d[0, j] = j;

            for (int j = 1; j <= m; j++)
                for (int i = 1; i <= n; i++)
                    if (s[i - 1] == t[j - 1])
                        d[i, j] = d[i - 1, j - 1];  //no operation
                    else
                        d[i, j] = Math.Min(Math.Min(
                            d[i - 1, j] + 1,    //a deletion
                            d[i, j - 1] + 1),   //an insertion
                            d[i - 1, j - 1] + 1 //a substitution
                            );
            return 1 - d[n, m] / (double)p;
        }

        public static List<SuggestItem> SimilarWordSuggestions(string word, int maxDistance)
        {
            return symSpell.Lookup(word, Verbosity.Closest, maxDistance);
        }

        /// <summary>
        /// Check that a string holding a lineage is properly formed
        /// </summary>
        /// <param name="lineage"></param>
        /// <returns>true id properly formed</returns>
        public static bool CheckLineage(string lineage)
        {
            var lin = lineage.Trim();
            var lineages = lin.Split('+');
            return lineages.Length == 1 ? CheckLineageInner(lineages[0]) : CheckLineageInner(lineages[0]) && CheckLineageInner(lineages[1]);
        }

        private static bool CheckLineageInner(string lin)
        {
            //check for whitespace
            if (lin.IndexOfAny(new char[] { ' ', '\t', '\n' }) != -1)
                return false;
            //check for value
            if (ValuePlaceholders.Contains(lin))
                return true;
            //now split for type
            var sections = lin.Split(':');
            if (sections.Length < 2)
                return false;
            if (!keywords.Contains(sections[0]))
                return false;
            var indexes = sections[1].Split(',');
            if (indexes.Length == 0)
                return false;
            foreach (var i in indexes)
            {
                if (!int.TryParse(i, out int ii))
                    return false;
            }
            return true;
        }

        public static (bool, string) CheckLineageWithTypeWord(string lineage)
        {
            var lin = lineage.Trim();
            if (lineages.ContainsKey(lineage))
            {
                return (true, lineages[lineage].typeWord);
            }
            return (CheckLineage(lineage), "");
        }
    }
}
