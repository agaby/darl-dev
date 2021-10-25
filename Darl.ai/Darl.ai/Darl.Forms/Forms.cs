using Darl.Lineage.Bot;
using DarlCommon;
using DarlLanguage;
using DarlLanguage.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Darl.Forms
{
    public class DarlForms
    {

        DarlRunTime runtime = new DarlRunTime();

        public async Task<QuestionSetProxy> Start(RuleForm form, QuestionCache state)
        {
            state.currentData = DarlVarExtensions.Convert(form.preload);
            return await Evaluate(state, form);
        }

        public async Task<QuestionSetProxy> Next(QuestionSetProxy qsp, RuleForm form, QuestionCache state)
        {
            LoadAnswers(qsp, state, form);
            state.currentIteration++;
            return await Evaluate(state, form);
        }

        public async Task<QuestionSetProxy> Back(RuleForm form, QuestionCache state)
        {
            Unwind(state);
            return await Evaluate(state, form);
        }

        public async Task<QuestionSetProxy> StartRedirect(RuleForm newForm, QuestionCache state, string redirectAddress)
        {
            state.callingRuleSet = state.projectId;
            state.projectId = redirectAddress;
            state.currentData = DarlVarExtensions.Convert(newForm.preload);
            state.currentIteration = 0;
            //if valid, add the return ruleset id to the qstate and start the new form
            return await Evaluate(state, newForm);
        }

        private async Task<QuestionSetProxy> Evaluate(QuestionCache qstate, RuleForm formResources)
        {
            //find the next n questions, where n <= the default question count.
            var tree = runtime.CreateTree(formResources.darl);
            //handle stores if present
            if(qstate.stores != null)
            {
                var storesInDarl = tree.GetMapStores();
                foreach(var s in qstate.stores.Keys)
                {
                    if (storesInDarl.Where(a => a.Name == s).Any())
                        runtime.SetStoreInterface(tree, s, qstate.stores[s]);
                }
                await formResources.UpdateFromCode(qstate.stores);
            }
            var res = await runtime.Evaluate(tree, qstate.currentData);
            var ufsaliences = runtime.CalculateSaliences(qstate.currentData, tree);
            qstate.totalReachableInputs = ufsaliences.Count;
            var qreq = qstate.requestedQuestions == 0 ? formResources.format.DefaultQuestions : qstate.requestedQuestions;
            List<string> unfilled = FindTopN(ufsaliences, qreq);
            //now format those for output
            var inputNames = runtime.GetInputNames(tree);
            QuestionSetProxy q = new QuestionSetProxy { canUnwind = CanUnwind(qstate), language = qstate.languageSelection, ieToken = qstate.SessionKey.ToString(), complete = qstate.totalReachableInputs == 0, percentComplete = inputNames.Count > 0 ? (100 * (inputNames.Count - qstate.totalReachableInputs) / inputNames.Count) : 0, questionsRequested = qreq };
            if (!q.complete) //still collecting data
            {
                //pt.TriggerEventAsync(qstate.tenant, qstate.projectId, null, 1);
                q.questions = new List<QuestionProxy>();
                foreach (string iname in unfilled)
                {
                    var informat = formResources.format.InputFormatList.Where(a => a.Name == iname).FirstOrDefault();
                    if (informat != null)
                    {
                        List<string> cats = new List<string>();
                        if (informat.Categories != null)
                        {
                            foreach (var cat in informat.Categories)
                            {
                                var category = cat;
                                if(cat.Contains("%%"))
                                {
                                    category = cat.Substring(0, cat.IndexOf("%%"));
                                }
                                var txt = TextLookup($"{iname}.{category}", formResources, qstate.languageSelection, true);
                                cats.Add(string.IsNullOrEmpty(txt) ? cat : txt);
                            }
                        }
                        var qtype = (QuestionProxy.QType)Enum.Parse(typeof(QuestionProxy.QType), informat.InType.ToString());
                        if (qtype == QuestionProxy.QType.numeric && informat.ShowSets)
                            qtype = QuestionProxy.QType.categorical;
                        q.questions.Add(new QuestionProxy { maxval = informat.NumericMax, minval = informat.NumericMin, qtype = (int)qtype, increment = informat.Increment, categories = new List<string>(cats), reference = iname, text = TextLookup(iname, formResources, qstate.languageSelection), format = informat.Regex, enforceCrisp = informat.EnforceCrisp });
                    }
                }
                q.responseHeader = TextLookup("Format.resultHeader", formResources, qstate.languageSelection, true);
                q.questionHeader = TextLookup("Format.questionHeader", formResources, qstate.languageSelection, true);
                q.preamble = qstate.currentIteration == 0 ? TextLookup("Format.preamble", formResources, qstate.languageSelection, true) : string.Empty;
                q.values = new Dictionary<string, string>(); //send saliences for debugging purposes
                foreach (var s in ufsaliences.Keys)
                {
                    q.values.Add(s, ufsaliences[s].ToString());
                }
                foreach (var r in res.Where( a => !inputNames.Contains(a.name))) //i.e. outputs only.
                {
                    if (r.IsUnknown())
                    {
                        q.values.Add(r.name, "Unknown");
                    }
                }
            }
            else //reporting results
            {
                //pt.TriggerEventAsync(qstate.tenant, qstate.projectId, null, 1);
                q.responses = new List<ResponseProxy>();
                q.values = new Dictionary<string, string>();
                foreach (var s in res)
                {
                    q.values.Add(s.name, s.ToString(""));
                }

                foreach (var r in res.Where(a => !inputNames.Contains(a.name))) //i.e. outputs only.
                {
                    var outformat = formResources.format.OutputFormatList.FirstOrDefault(a => a.Name == r.name);
                    if (outformat != null && !outformat.Hide)
                    {
                        var op = r;
                        string annotation = TextLookup(r.name, formResources, qstate.languageSelection);
                        ResponseProxy rp;
                        if (op.IsNumeric())
                        {
                            if (op.IsUnknown())
                            {
                                string ukVal = TextLookup(r.name + "_unknown", formResources, qstate.languageSelection);
                                rp = new ResponseProxy { mainText = ukVal, rtype = (int)ResponseProxy.RType.Text, annotation = annotation };

                            }
                            else
                            {
                                switch (outformat.displayType)
                                {
                                    case OutputFormat.DisplayType.ScoreBar:
                                        rp = new ResponseProxy { value = Math.Round((double)op.Value, 1), rtype = (int)ResponseProxy.RType.ScoreBar, color = outformat.ScoreBarColor, lowText = TextLookup(r.name + ".ScoreBarLow", formResources, qstate.languageSelection), highText = TextLookup(r.name + ".ScoreBarHigh", formResources, qstate.languageSelection), annotation = annotation, minVal = outformat.ScoreBarMinVal, maxVal = outformat.ScoreBarMaxVal };
                                        break;

                                    case OutputFormat.DisplayType.Text:
                                        {
                                            string val;
                                            if (!string.IsNullOrEmpty(outformat.ValueFormat))
                                                val = ((double)op.Value).ToString(outformat.ValueFormat);
                                            else
                                                val = op.Value.ToString();
                                            if (outformat.Uncertainty)
                                                rp = new ResponseProxy { mainText = val, rtype = (int)ResponseProxy.RType.Text, annotation = annotation, minVal = (double)op.values.First(), maxVal = (double)op.values.Last() };
                                            else
                                                rp = new ResponseProxy { mainText = val, rtype = (int)ResponseProxy.RType.Text, annotation = annotation };
                                        }
                                        break;
                                    default:
                                        rp = new ResponseProxy();
                                        break;
                                }
                            }
                            q.responses.Add(rp);
                        }
                        else
                        {
                            if (!op.IsUnknown())
                            {
                                if (!outformat.Uncertainty)//just central result
                                {
                                    string mtext = string.Empty;
                                    if (op.dataType == DarlResult.DataType.textual)
                                    {
                                        mtext = op.Value.ToString();
                                    }
                                    else
                                    {
                                        mtext = TextLookup(r.name + "." + op.Value.ToString(), formResources, qstate.languageSelection);
                                    }
                                    switch (outformat.displayType)
                                    {
                                        case OutputFormat.DisplayType.Text:
                                            rp = new ResponseProxy { mainText = mtext, rtype = (int)ResponseProxy.RType.Text, annotation = annotation };
                                            break;
                                        case OutputFormat.DisplayType.Link:
                                            rp = new ResponseProxy { mainText = mtext, rtype = (int)ResponseProxy.RType.Link, annotation = annotation };
                                            break;
                                        case OutputFormat.DisplayType.Redirect:
                                            return new QuestionSetProxy { redirect = mtext };
                                        default:
                                            rp = new ResponseProxy();
                                            break;
                                    }
                                    q.responses.Add(rp);
                                }
                                else // display uncertainty too
                                { //this section needs rewriting
                                    foreach (var c in op.categories.OrderByDescending(a => a.Value))
                                    {
                                        var mtext = TextLookup(r.name + "." + c.Key, formResources, qstate.languageSelection);
                                        switch (outformat.displayType)
                                        {
                                            case OutputFormat.DisplayType.Text:
                                                rp = new ResponseProxy { mainText = mtext, rtype = (int)ResponseProxy.RType.Text, annotation = annotation, format = c.Value.ToString() };
                                                break;
                                            case OutputFormat.DisplayType.Link:
                                                rp = new ResponseProxy { mainText = mtext, rtype = (int)ResponseProxy.RType.Link, annotation = annotation, format = c.Value.ToString() };
                                                break;
                                            case OutputFormat.DisplayType.Redirect:
                                            //start a new evaluation with 
                                            default:
                                                rp = new ResponseProxy();
                                                break;
                                        }
                                        q.responses.Add(rp);
                                    }
                                }
                            }
                        }
                    }
                }
                q.responseHeader = TextLookup("Format.resultHeader", formResources, qstate.languageSelection);
            }

            return q;
        }

        private List<string> FindTopN(Dictionary<string, double> ufsaliences, int p)
        {
            var sortedDict = ufsaliences.OrderByDescending(a => a.Value).ThenBy(a => a.Key).Select(a => a.Key).Take(p); //change 1/11/18
            return new List<string>(sortedDict);
        }

        private string TextLookup(string reference, RuleForm cache, string language, bool noDefault = false)
        {
            foreach (var rec in cache.language.LanguageList)
            {
                if (rec.Name == reference)
                {
                    if (string.IsNullOrEmpty(language) || (language == cache.language.DefaultLanguage) && !string.IsNullOrEmpty(cache.language.DefaultLanguage))
                    {
                        return rec.Text;
                    }
                    if (rec.VariantList != null)
                    {
                        foreach (var variant in rec.VariantList)
                        {
                            if (variant.Language == language)
                                return variant.Text;
                        }
                    }
                }
            }
            return noDefault ? "" : reference.Replace('_', ' ');
        }

        
        private string LookupCategory(string input, string text, RuleForm cache, string language)
        {
            var categories = cache.language.LanguageList.Where(a => a.Name.StartsWith($"{input}.")).ToList();
            foreach (var c in categories)
            {
                if (TextLookup(c.Name, cache, language) == text)
                {
                    return c.Name.Substring(c.Name.IndexOf('.') + 1); //extract the category part of the name.
                }
            }
            foreach (var c in categories) //consider the case where no text was supplied and the cat name was used
            {
                string cat = c.Name.Substring(c.Name.IndexOf('.') + 1);
                if (text == cat || text == cat.Replace('_', ' '))
                    return cat;
            }
            return text; //changed from string.empty 28/10/16
        }

        /// <summary>
        /// Read in answers from an object format
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="cache">The cache.</param>
        private void LoadAnswers(QuestionSetProxy result, QuestionCache cache, RuleForm formCache)
        {
            if (result.questions != null)
            {
                foreach (QuestionProxy answer in result.questions)
                {
                    string reference = answer.reference;
                    string data = string.Empty;
                    DarlResult.DataType dataType = DarlResult.DataType.textual;
                    switch ((QuestionProxy.QType)answer.qtype)
                    {
                        case QuestionProxy.QType.categorical:
                            data = LookupCategory(answer.reference, answer.sResponse, formCache, result.language);
                            dataType = DarlResult.DataType.categorical;
                            break;
                        case QuestionProxy.QType.numeric:
                            dataType = DarlResult.DataType.numeric;
                            try
                            {
                                data = Convert.ToString(answer.dResponse);
                            }
                            catch
                            {
                                data = string.Empty;
                            }
                            break;
                        case QuestionProxy.QType.textual:
                            data = answer.sResponse;
                            break;
                        case QuestionProxy.QType.temporal:
                            data = answer.sResponse;
                            dataType = DarlResult.DataType.temporal;
                            break;
                    }
                    LoadAnswer(data, reference, cache, dataType);
                }
            }
        }

        /// <summary>
        /// Read in answers from a dictionary format
        /// </summary>
        /// <param name="results">dictionary of results</param>
        /// <param name="cache">The cache.</param>
        private void LoadAnswers(Dictionary<string, string> results, QuestionCache cache)
        {
            foreach (string reference in results.Keys)
            {
                string data = results[reference];

                LoadAnswer(data, reference, cache, DarlResult.DataType.textual);
            }
        }

        /// <summary>
        /// Loads an individual answer
        /// </summary>
        /// <param name="data"></param>
        /// <param name="reference"></param>
        /// <param name="cache"></param>
        private void LoadAnswer(string data, string reference, QuestionCache cache, DarlResult.DataType dataType)
        {
            if (!string.IsNullOrEmpty(data))
            {
                if (!cache.currentData.Any( a => a.name == reference))
                {
                    var r = new DarlResult(reference,data, dataType);
                    r.identifier = cache.currentIteration.ToString();
                    cache.currentData.Add( r);
                }
                else
                {
                    var r = cache.currentData.First(a => a.name == reference);
                    r.Value = data;
                    r.identifier = cache.currentIteration.ToString();
                }
            }
        }
        /// <summary>
        /// Removes the most recent data received by the questionnaire, thus unwinding.
        /// </summary>
        private void Unwind(QuestionCache cache)
        {
            int maxIter = -1;
            //find last iteration
            foreach (var resp in cache.currentData)
            {
                int seq = Convert.ToInt32(resp.identifier);
                if (seq > maxIter)
                    maxIter = seq;
            }
            //collect list of data items to remove
            var removeList = new List<DarlResult>();
            foreach (var resp in cache.currentData)
            {
                int seq = Convert.ToInt32(resp.identifier);
                if (seq >= maxIter)
                    removeList.Add(resp);
            }
            //remove them
            foreach (var resp in removeList)
            {
                cache.currentData.Remove(resp);
            }
            cache.currentIteration = Math.Max(0, maxIter - 1);
        }

        /// <summary>
        /// During a questionnaire determines if data exists that can be unwound
        /// </summary>
        /// <returns></returns>
        private bool CanUnwind(QuestionCache cache)
        {
            foreach (var resp in cache.currentData)
            {
                string ident = resp.identifier;
                if (!string.IsNullOrEmpty(ident))
                {
                    int seq = Convert.ToInt32(ident);
                    if (seq > -1)
                        return true;
                }
            }
            return false;
        }
    }
}
