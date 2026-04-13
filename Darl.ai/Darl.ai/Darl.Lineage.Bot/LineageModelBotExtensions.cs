/// </summary>

﻿using Darl.Lineage.Bot.Stores;
using DarlCommon;
using DarlCompiler.Parsing;
using DarlLanguage;
using DarlLanguage.Processing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.Lineage.Bot
{
    public static class LineageModelBotExtensions
    {
        public static string flagLocation { get; set; } = "ConversationData";

        public static string defaultSignum { get; set; } = "default:";

        public static string responseSignum { get; set; } = "response";


        private static readonly DarlRunTime runtime = new DarlRunTime();

        public enum Commands { none, quit, back, help, about, history, debug }

        static readonly Dictionary<string, Commands> commandLookUp = new Dictionary<string, Commands>() { { "verb:060", Commands.quit },
            { "verb:399,0", Commands.quit },
            { "verb:190", Commands.quit },
            { "verb:331,02", Commands.quit },
            { "verb:015,180", Commands.quit },
            { "verb:015,022,29", Commands.quit },
            { "verb:185,1,18", Commands.quit },
            { "verb:004.62", Commands.quit },
            { "verb:023,17,2", Commands.back },
            { "verb:165,0,2,0", Commands.back },
            { "verb:015,018,08,2", Commands.back },
            { "verb:347", Commands.back },
            { "verb:172,072", Commands.back },
            { "verb:397,2", Commands.help },
            { "verb:023,24,02,4,0", Commands.help },
            { "preposition:2", Commands.about },
            { "noun:01,4,05,13,04", Commands.about },
            { "noun:01,2,07,10,13,4", Commands.about },
            { "noun:00,1,01,19,14,1", Commands.about },
            { "noun:01,4,09,01,3,4", Commands.history },
            { "noun:01,1,02,09,0", Commands.history },
            { "verb:015,018,04,1", Commands.debug }
        };

        public static readonly string accessRolesKey = "access_roles";
        /// Respond to a request
        /// </summary>
        /// <param name="model"></param>
        /// <param name="question"></param>
        /// <param name="values"></param>
        /// <param name="stores"></param>
        /// <returns></returns>
        public static async Task<string> Interact(this LineageModel model, string question, List<DarlVar> values, Dictionary<string, ILocalStore>? stores = null)
        {
            var resp = await InteractTest(model, new DarlVar { name = nameof(question), Value = question, dataType = DarlVar.DataType.textual }, values, stores);
            return resp.Count > 0 ? resp[0].response.Value : "";
        }

        /// Respond to a request with test data
        /// </summary>
        /// <param name="model"></param>
        /// <param name="question"></param>
        /// <param name="values"></param>
        /// <param name="stores"></param>
        /// <returns></returns>
        public static async Task<List<InteractTestResponse>> InteractTest(this LineageModel model, DarlVar question, List<DarlVar> values, Dictionary<string, ILocalStore>? stores = null, bool fuzzy = false)
        {
            if (stores == null)
                stores = new Dictionary<string, ILocalStore>();
            if (values == null)
                values = new List<DarlVar>();
            if (model.tree.executionRoot == null)
                model.tree.CreateExecutionTree();
            var call = stores.Values.Where(a => a is CallStore).FirstOrDefault() as CallStore;
            var outList = new List<InteractTestResponse>();
            string source = string.Empty;
            DarlVar response = new DarlVar { dataType = DarlVar.DataType.textual, Value = "No response generated...", unknown = true };
            DarlVar link = new DarlVar { dataType = DarlVar.DataType.link, unknown = true, Value = "" };
            DarlVar callResponse = new DarlVar { dataType = DarlVar.DataType.ruleset, unknown = true, Value = "" };
            DarlVar credentials = new DarlVar { dataType = DarlVar.DataType.credentials, unknown = true, Value = "" };
            List<MatchedElement> matches = null;
            values.Clear();
            LineageMatchNode.comp.lineageMatch = true;
            matches = model.Match(question.Value, values, fuzzy);
            LineageMatchNode.comp.lineageMatch = false;
            while (matches.Count > 0 && (response.unknown || response.weight < 1.0))
            {
                var lastMatch = matches.Last();
                var last = ((MatchedAnnotation)lastMatch).annotation;
                values = lastMatch.values;
                if (stores.ContainsKey("Value"))
                    ((ValuesStore)stores["Value"]).values = values;
                //construct composite rule set
                //and apply it to the Darl runtime
                try
                {
                    if (last.accessRoles != null && last.accessRoles.Any()) //this response is secured
                    {
                        if (stores.ContainsKey(flagLocation))
                        {
                            var rolesRes = await stores[flagLocation].ReadAsync(new List<string> { accessRolesKey });
                            bool needLogin = true;
                            if (!rolesRes.IsUnknown())
                            {
                                var roles = JsonConvert.DeserializeObject<List<string>>(rolesRes.Value.ToString());
                                needLogin = !roles.Intersect(last.accessRoles).Any();
                            }
                            if (needLogin) //ask for log in
                            {
                                credentials.unknown = false;
                                credentials.Value = last.accessRoles[0];
                            }
                        }
                    }
                    source = CreateCompositeRuleSet(model, matches);
                    var tree = runtime.CreateTree(source);
                    foreach (var s in stores.Keys)
                    {
                        try
                        {
                            runtime.SetStoreInterface(tree, s, stores[s]);
                        }
                        catch
                        {

                        }
                    }
                    var res = await runtime.Evaluate(tree, DarlVarExtensions.Convert(values));
                    values = DarlVarExtensions.Convert(res);
                    foreach (var r in res)
                    {
                        if (r.name == nameof(response))
                        {
                            response = DarlVarExtensions.Convert(r);
                            if (matches.Last().path.Contains(defaultSignum))
                            {
                                response.approximate = true; //signals a default:
                            }
                        }
                        if (r.name == nameof(link) && link.unknown)
                        {
                            link = DarlVarExtensions.Convert(r);
                            link.dataType = DarlVar.DataType.link;
                            if (matches.Last().path.Contains(defaultSignum))
                            {
                                link.approximate = true; //signals a default:
                            }
                        }
                        if (r.name.EndsWith("Call.") && callResponse.unknown) //first wins
                        {
                            callResponse = DarlVarExtensions.Convert(r);
                            callResponse.dataType = DarlVar.DataType.ruleset;
                            callResponse.name = "Call";
                            if (matches.Last().path.Contains(defaultSignum))
                            {
                                callResponse.approximate = true; //signals a default:
                            }
                        }

                    }
                }
                catch //probably missing IO or access denied
                {
                    continue;
                }
                response.weight = Math.Min(response.weight, lastMatch.confidence); //pass through the match confidence
                if (response.unknown && !string.IsNullOrEmpty(response.Value))
                    response.unknown = false;
                outList.Add(new InteractTestResponse { darl = source, response = response, matches = matches });
                if (!link.unknown)
                    outList.Add(new InteractTestResponse { darl = source, response = link, matches = matches });
                if (!callResponse.unknown)
                    outList.Add(new InteractTestResponse { darl = source, response = callResponse, matches = matches });
                if (!credentials.unknown)
                    outList.Add(new InteractTestResponse { darl = source, response = credentials, matches = matches });
                matches.RemoveAt(matches.Count - 1); //remove the last match if no valid response
            }
            return outList;
        }


        /// match of single word or known phrase to command
        /// </summary>
        /// <param name="value">string</param>
        public static Commands HandleRuleSetCommands(string value)
        {
            var lineages = LineageLibrary.LookupWord(value.ToLower().Trim());
            if (lineages != null)
            {
                foreach (var s in commandLookUp.Keys)
                {
                    foreach (var l in lineages)
                    {
                        if (l.lineage.StartsWith(s))
                            return commandLookUp[s];
                    }
                }
            }
            return Commands.none;
        }

        private static List<DarlVar> ConvertState(LineageModel model, List<DarlVar>? source = null)
        {
            if (source == null)
                source = new List<DarlVar>();
            //copy model settings, not overwriting existing
            foreach (var s in model.modelSettings.Keys)
            {
                if (!source.Exists(a => a.name == s))
                {
                    source.Add(JsonConvert.DeserializeObject<DarlVar>(model.modelSettings[s]));
                }
            }
            return source;
        }

        private static string CreateCompositeRuleSet(this LineageModel model, List<MatchedElement> matches)
        {
            var sb = new StringBuilder();
            int insertPos = model.ruleSkeleton.IndexOf(LineageModel.insertionPointText);
            sb.Append(model.ruleSkeleton.Substring(0, insertPos));
            var botFormat = JsonConvert.DeserializeObject<BotFormat>(model.form);
            sb.Append(botFormat.ToDarl());
            foreach (var d in ((MatchedAnnotation)matches.Last()).annotation.darl) //changed, used to merge multiple rulesets.
            {
                sb.AppendLine(d);
            }
            sb.Append(model.ruleSkeleton.Substring(insertPos + LineageModel.insertionPointText.Length));
            return sb.ToString();
        }

        /// Compares the framework against all the annotation rules and either reports any missing i/o or deletes any defective rules.
        /// </summary>
        /// <param name="model">The model to work on</param>
        /// <param name="fix">if true deletes the rules</param>
        /// <returns>A list of missing i/o</returns>
        public static List<string> CheckModelAgainstFramework(this LineageModel model, bool fix = false)
        {
            var diffs = new HashSet<string>();
            //get all the darl code in the annotations
            var annotations = new HashSet<LineageAnnotationNode>();
            model.tree.root.CreateExecutionGraph(annotations);
            var runtime = new DarlRunTime();
            foreach (var a in annotations)
            {
                if (a != null)
                {
                    var source = CreateCompositeRuleSet(model, new List<MatchedElement> { new MatchedAnnotation { annotation = a } });
                    var tree = runtime.CreateTreeEdit(source);
                    var goodLines = new List<string>();
                    if (tree.HasErrors())
                    {
                        foreach (string s in a.darl)
                        {
                            foreach (var l in s.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                var subsource = CreateCompositeRuleSet(model, new List<MatchedElement> { new MatchedAnnotation { annotation = new LineageAnnotationNode { darl = new List<string> { l } } } });
                                var subtree = runtime.CreateTreeEdit(subsource);
                                if (!subtree.HasErrors())
                                    goodLines.Add(l);
                                else
                                {
                                    foreach (var err in subtree.ParserMessages)
                                    {
                                        if (err.Message.StartsWith("Use of undeclared I/O:"))
                                            diffs.Add(err.Message);
                                    }
                                }

                            }
                        }
                        if (fix)
                            a.darl = goodLines;
                    }
                }
            }
            return diffs.ToList();
        }

        public static string CreateCodeFromFormat(this LineageModel model)
        {
            var formObj = JsonConvert.DeserializeObject<BotFormat>(model.form);
            //strip out existing io from skeleton;
            var skeleton = model.ruleSkeleton;
            var start = skeleton.IndexOf('{') + 1;
            var end = skeleton.IndexOf(LineageModel.insertionPointText);
            return $"{skeleton.Substring(0, start)}\r\n{formObj.ToDarl()}{skeleton.Substring(end)}";
        }

        /// Reconcile changes in code and produce a canonical result.
        /// </summary>
        /// <param name="currentModel">The model</param>
        /// <param name="code">The possibly updated code</param>
        /// <param name="bf">The updated data</param>
        /// <param name="path">The path to the attribute</param>
        /// <returns>The new DARL code</returns>
        public static string ReconcileCode(this LineageModel currentModel, string code, BotFragment bf, string path)
        {
            LineageMatchNode.comp.multiMatch = true;
            var result = currentModel.tree.FindExecutionTree(path);
            LineageMatchNode.comp.multiMatch = false;

            if (result != null) //if path exists
            {
                if (result.annotation == null) //if no annotation node
                {
                    result.annotation = new LineageAnnotationNode();
                }
                var oldCode = string.Join("\n", result.annotation.darl).Trim();
                if (!string.IsNullOrEmpty(code))
                {
                    if (oldCode != code.Trim())
                        return code; //code exists in the annotation, and it has been updated. return the updated version.
                }
                //not code change - look for response or call changes
                var source = CreateCompositeCode(currentModel, oldCode);
                string newCode = oldCode;
                var runtime = new DarlRunTime();
                try
                {
                    bool treeChanged = false;
                    var tree = runtime.CreateTree(source);
                    if (tree.HasErrors())
                        throw new Exception("errors in code");
                    //handle changes to textual response
                    var res = tree.GetOutputs(responseSignum);
                    if (res.Count > 0) //response is defined - expect this always to be true
                    {//modify existing response if different
                        var texts = tree.GetSingleRuleSetTextualRHS(responseSignum);
                        if (!string.IsNullOrEmpty(bf.Response))
                        {
                            if (texts.Count == 1 && bf.Response != texts[0] || texts.Count > 1)
                            {
                                tree.ChangeSingleRuleSetTextualRHS(responseSignum, new List<string> { bf.Response });
                                treeChanged = true;
                            }
                        }
                        else if (bf.RandomResponses.Count > 0)
                        {
                            if (texts.Count > 0) //existing rule
                            {
                                if (texts.Count != bf.RandomResponses.Count || string.Join(" ", texts) != string.Join(" ", bf.RandomResponses))
                                {
                                    tree.ChangeSingleRuleSetTextualRHS(responseSignum, bf.RandomResponses);
                                    treeChanged = true;
                                }
                            }
                        }
                        else if (texts.Count > 0)
                        {//detect if response needs to be deleted
                            tree.DeleteSingleRulesetRules(responseSignum);
                            treeChanged = true;
                        }
                        if (!treeChanged) //usually no response rules defined 
                        {//create response
                            if (bf.RandomResponses.Count > 0)
                            {
                                newCode = $"{newCode}\n\tif anything then response will be randomtext(\"{string.Join("\",\"", bf.RandomResponses)}\");";
                            }
                            else if (!string.IsNullOrEmpty(bf.Response))
                            {
                                newCode = $"{newCode}\n\tif anything then response will be \"{bf.Response}\";";
                            }
                        }
                    }

                    //handle changes to call...

                    var calls = tree.GetSingleRuleSetTextualRHS("Call.");
                    if (!string.IsNullOrEmpty(bf.CallRuleset))
                    {
                        if (calls.Count == 1)
                        {
                            tree.ChangeSingleRuleSetTextualRHS("Call.", new List<string> { bf.CallRuleset });
                            treeChanged = true;
                        }
                        if (calls.Count == 0 && treeChanged)
                        {//add a call ruleset to modified tree
                            newCode = extractCompositeCode(currentModel, tree, runtime);
                            newCode = $"{newCode}\n\tif anything then Call[\"\"] will be \"{bf.CallRuleset}\";";
                            treeChanged = false;
                        }
                        else
                        {
                            newCode = $"{newCode}\n\tif anything then Call[\"\"] will be \"{bf.CallRuleset}\";";
                        }
                    }
                    if (string.IsNullOrEmpty(bf.CallRuleset) && calls.Count >= 1)
                    {//deleting a call
                        tree.DeleteSingleRulesetRules("Call.");
                        treeChanged = true;
                    }
                    if (treeChanged)
                        return extractCompositeCode(currentModel, tree, runtime);
                    return newCode;
                }
                catch
                {
                }
            }
            return string.Empty; //indicates error
        }


        public static string CreateCompositeCode(this LineageModel currentModel, string code)
        {
            var sb = new StringBuilder();
            int insertPos = currentModel.ruleSkeleton.IndexOf(LineageModel.insertionPointText);
            sb.Append(currentModel.ruleSkeleton.Substring(0, insertPos));
            var botFormat = JsonConvert.DeserializeObject<BotFormat>(currentModel.form);
            sb.Append(botFormat.ToDarl());
            sb.Append(code);
            sb.Append(currentModel.ruleSkeleton.Substring(insertPos + LineageModel.insertionPointText.Length));
            return sb.ToString();
        }

        public static string extractCompositeCode(this LineageModel currentModel, ParseTree tree, DarlRunTime runtime)
        {
            var newCode = tree.ToDarl();
            var skeleton = CreateCompositeCode(currentModel, "");
            var sktree = runtime.CreateTree(skeleton);
            var diff = FindDiff(newCode, sktree.ToDarl());
            return diff;
        }

        /// create a phrase in the model
        /// </summary>
        /// <param name="currentModel">the model</param>
        /// <param name="phrase">the phrase</param>
        public static LineageMatchNode PhraseCreate(this LineageModel currentModel, string phrase)
        {
            //find lowest parent in tree
            var currentPath = phrase;
            var newPath = string.Empty;
            LineageMatchNode.comp.multiMatch = true;
            do
            {
                int lastSeparator = currentPath.LastIndexOf('/');
                if (lastSeparator == -1)
                {
                    newPath = currentPath + newPath;
                    currentPath = string.Empty;
                    break;
                }
                //builds path not currently in tree
                newPath = currentPath.Substring(lastSeparator) + newPath;
                currentPath = currentPath.Substring(0, lastSeparator);
            }
            while (currentModel.tree.Find(currentPath) == null);
            if (newPath.StartsWith("/"))
                newPath = newPath.Substring(1); //remove first slash
            //currentpath contains the parent of the new node(s);
            //newPath may contain multiple sub nodes, so split them
            var subPaths = newPath.Split('/');
            LineageMatchNode lmn = null;
            foreach (var subPath in subPaths)
            {
                lmn = currentModel.tree.Add(currentPath, subPath);
                currentPath = string.IsNullOrEmpty(currentPath) ? subPath : currentPath + "/" + subPath;
            }
            LineageMatchNode.comp.multiMatch = false;
            return lmn;
        }

        /// Delete a phrase
        /// </summary>
        /// <param name="currentModel"></param>
        /// <param name="path"></param>
        public static void PhraseDelete(this LineageModel currentModel, string path)
        {
            LineageMatchNode.comp.multiMatch = true;
            currentModel.tree.Delete(path);
            LineageMatchNode.comp.multiMatch = false;
        }

        /// Separate this phrase from neighbours, giving separate, copied attribute.
        /// </summary>
        /// <param name="currentModel"></param>
        /// <param name="path"></param>
        public static void PhraseSeparate(this LineageModel currentModel, string path)
        {
            LineageMatchNode.comp.multiMatch = true;
            var existing = currentModel.tree.Find(path);
            if (existing == null)
            {
                LineageMatchNode.comp.multiMatch = false;
                return; //existing phrase doesn't exist.
            }
            var seq = currentModel.tree.FindSequence(path);

            //two possible cases - part of multimatch, so must be extracted, or already separate but with
            //shared attribute, so attributes must be separated.
            if (existing.element.type == LineageType.composite && existing.element.lineage.Contains('|')) //multimatch
            {//split last element
                var last = path.Split('/').Last();
                //might itself be multipath
                var lastElements = last.Split('|');
                var oldMulti = existing.element.lineage;
                var existingElements = oldMulti.Split('|');
                var remainingElements = existingElements.Except(lastElements);
                if (remainingElements.Any())//only bother if there is a difference, otherwise sets were the same
                {
                    existing.element.lineage = String.Join("|", remainingElements);
                    //modify parent's children list
                    seq[seq.Count - 2].children.Remove(oldMulti);
                    seq[seq.Count - 2].children.Add(existing.element.lineage, existing);
                    //now we can just create the new element.
                    var res = currentModel.PhraseCreate(path);
                    res.annotation = new LineageAnnotationNode { accessRoles = new List<string>(existing.annotation.accessRoles), darl = new List<string>(existing.annotation.darl), implications = new List<string>(existing.annotation.implications) };
                }
            }
            else
            {//create new copy of existing annotation.
                existing.annotation = new LineageAnnotationNode { accessRoles = new List<string>(existing.annotation.accessRoles), darl = new List<string>(existing.annotation.darl), implications = new List<string>(existing.annotation.implications) };
            }
            LineageMatchNode.comp.multiMatch = false;
        }

        /// Merges a new phrase with an existing phrase, giving the same response
        /// </summary>
        /// <param name="currentModel">The model to work on</param>
        /// <param name="phrase">The phrase to add</param>
        /// <param name="oldPhrase">The existing phrase</param>
        public static void PhraseMerge(this LineageModel currentModel, string phrase, string oldPhrase)
        {
            var existing = currentModel.tree.Find(oldPhrase);
            if (existing == null)
                return; //existing phrase doesn't exist.
            var np = currentModel.PhraseCreate(phrase);
            if (np != null)
                np.annotation = existing.annotation;
        }

        /// Instantiate the stores specified in the model
        /// </summary>
        /// <param name="model">The model</param>
        /// <param name="user">The user</param>
        /// <param name="formInt">interface to online storage</param>
        /// <param name="values">values in the current question</param>
        /// <param name="UserData">Bot specific data</param>
        /// <param name="ConversationData">Bot specific data</param>
        /// <param name="PrivateConversationData">Bot specific data</param>
        /// <returns>The instantiated stores</returns>
        /// <remarks>Needs to be updated for new store types - not good solution.</remarks>
        public static Dictionary<string, ILocalStore> CreateStores(this LineageModel model, string user, IRuleFormInterface formInt, List<DarlVar> values, IBotDataInterface UserData, IBotDataInterface ConversationData, IBotDataInterface PrivateConversationData)
        {
            var dict = new Dictionary<string, ILocalStore>();
            var botFormat = JsonConvert.DeserializeObject<BotFormat>(model.form);
            foreach (var store in botFormat.Stores)
            {
                switch (store.ToLower())
                {
                    case "call":
                        dict.Add("Call", new CallStore(formInt, user));
                        break;
                    case "rest":
                        dict.Add("Rest", new RestStore());
                        break;
                    case "word":
                        dict.Add("Word", new WordStore());
                        break;
                    case "collateral":
                        dict.Add("Collateral", new CollateralStore(formInt, user));
                        break;
                    case "bot":
                        dict.Add("Bot", new SettingsStore(model.modelSettings));
                        break;
                    case "value":
                        dict.Add("Value", new ValuesStore(values));
                        break;
                    case "userdata":
                        dict.Add(nameof(UserData), new BotDataStore(UserData));
                        break;
                    case "privateconversationdata":
                        dict.Add(nameof(PrivateConversationData), new BotDataStore(PrivateConversationData));
                        break;
                    case "conversationdata":
                        dict.Add(nameof(ConversationData), new BotDataStore(ConversationData));
                        break;
                }
            }

            return dict;
        }


        /// copies over any matching data in bot user data to the rule set values
        /// </summary>
        /// <param name="format">The rule set format</param>
        /// <param name="UserData">Bot user data</param>
        /// <param name="values">The value set used by ruleset evaluation</param>
        public static void CopyValues(FormFormat format, IBotDataInterface UserData, List<DarlVar> values)
        {
            //now search through inputs in io format looking for matching data items in the botdata stores
            foreach (var i in format.InputFormatList)
            {
                if (UserData.ContainsKey(i.Name))
                {
                    switch (i.InType)
                    {
                        case InputFormat.InputType.numeric:
                            if (UserData.TryGetValue<double>(i.Name, out double dval))
                            {
                                var existing = values.Where(a => a.name == i.Name).First();
                                if (existing != null)
                                    values.Remove(existing);
                                values.Add(new DarlVar { Value = dval.ToString(), dataType = DarlVar.DataType.numeric, name = i.Name });

                            }
                            break;
                        case InputFormat.InputType.categorical:
                            if (UserData.TryGetValue<string>(i.Name, out string sval))
                            {
                                var existing = values.Where(a => a.name == i.Name).First();
                                if (existing != null)
                                    values.Remove(existing);
                                values.Add(new DarlVar { Value = sval, dataType = DarlVar.DataType.categorical, name = i.Name });
                            }
                            break;
                        case InputFormat.InputType.textual:
                            if (UserData.TryGetValue<string>(i.Name, out string tval))
                            {
                                var existing = values.Where(a => a.name == i.Name).First();
                                if (existing != null)
                                    values.Remove(existing);
                                values.Add(new DarlVar { Value = tval, dataType = DarlVar.DataType.textual, name = i.Name });
                            }
                            break;
                    }
                }
            }
        }

        /// extract part of s1 that is not in s2
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns>the difference</returns>
        private static string FindDiff(string s1, string s2)
        {
            if (s1.RemoveWhiteSpace() == s2.RemoveWhiteSpace())
                return string.Empty;
            var sb = new StringBuilder();
            var s2list = s2.Split('\n');
            var s1list = s1.Split('\n');
            int p = 0;
            for (int n = 0; n < s1list.Length && p < s2list.Length; n++)
            {
                if (s1list[n].RemoveWhiteSpace() == s2list[p].RemoveWhiteSpace())
                {
                    p++;
                }
                else
                {
                    sb.AppendLine(s1list[n]);
                }
            }
            return sb.ToString();
        }

        private static string RemoveWhiteSpace(this string str)
        {
            var sb = new StringBuilder();
            foreach (var c in str)
            {
                if (!Char.IsWhiteSpace(c))
                    sb.Append(c);
            }
            return sb.ToString();
        }

        /// extract any responses and calls from an existing darl fragment
        /// </summary>
        /// <param name="currentModel"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static BotFragment BotFragmentBuilder(this LineageModel currentModel, string code)
        {
            var bf = new BotFragment();
            var source = CreateCompositeCode(currentModel, code);
            var runtime = new DarlRunTime();
            try
            {
                var tree = runtime.CreateTree(source);
                if (tree.HasErrors())
                    throw new Exception("errors in code");
                var res = tree.GetOutputs("response");
                if (res.Count > 0) //response is defined
                {
                    var texts = tree.GetSingleRuleSetTextualRHS("response");
                    if (texts.Count > 1)
                    {
                        bf.RandomResponses = texts;
                    }
                    else if (texts.Count == 1)
                    {
                        bf.Response = texts[0];
                    }
                }
                var calls = tree.GetSingleRuleSetTextualRHS("Call.");
                if (calls.Count == 1)
                {
                    bf.CallRuleset = calls[0];
                }
            }
            catch { }
            return bf;
        }

        /// Find the best match in the existing tree and expand the path to match the text.
        /// </summary>
        /// <param name="text">a text string to match</param>
        /// <returns>A proposed path</returns>
        /// <remarks>To be used when only a default match is available. 
        /// This offers the best path to a new entry making the most use of the existing tree.
        /// If accepted, the new path is used with PhraseCreate. </remarks>
        public static string CreateCandidatePath(string text)
        {
            return "";
        }
    }
}
