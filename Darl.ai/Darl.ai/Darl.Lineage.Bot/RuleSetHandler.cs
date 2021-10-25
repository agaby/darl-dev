using Darl.Forms;
using DarlCommon;
using DarlLanguage;
using DarlLanguage.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Darl.Lineage.Bot
{
    [Serializable]
    public class RuleSetHandler : IRuleSetHandler
    {
        public static readonly string questionIdentifier = "__question";

        [NonSerialized]

        private ITrigger trigger;

        [NonSerialized]
        public static DarlRunTime runtime = new DarlRunTime(); //could use parent's

        public List<string> valueSequence { get; set; } = new List<string>();

        public DarlVar pending { get; set; } = null;

        public string user { get; set; } = string.Empty;

        public string modelId { get; set; }

        public RuleForm rf { get; set; }
        public ITrigger Trigger { get => trigger; set => trigger = value; }


        /// <summary>
        /// perform a single pass through the rule set
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public async Task<List<InteractTestResponse>> RuleSetPass(List<DarlVar> values, Dictionary<string, ILocalStore> stores, ServiceConnectivity? service = null)
        {
            if (rf.preload == null)
                rf.preload = new List<DarlVar>();
            string validationResponse;
            if (!Validate(values, out validationResponse)) //out of range value
            {
                return new List<InteractTestResponse> { new InteractTestResponse { darl = "", response = new DarlVar { name = "response", dataType = DarlVar.DataType.textual, Value = validationResponse }, matches = new List<MatchedElement>() } };
            }

            var responses = new List<InteractTestResponse>();
            // add preload if required
            if (rf.preload != null)
            {
                foreach (var p in rf.preload)
                {
                    if (!values.Where(a => a.name == p.name).Any())
                    {
                        values.Add(p);
                    }
                }
            }
            //Convert and add last value
            if (pending != null)
            {
                var question = values.Where(a => a.name == questionIdentifier).FirstOrDefault();
                if (question != null)
                {
                    pending.Value = question.Value;
                    pending.weight = 1.0;
                    pending.unknown = false;
                    values.Remove(question);
                    values.Add(pending);
                }
            }
            var vals = DarlVarExtensions.Convert(values);
            var response = new InteractTestResponse();
            var tree = runtime.CreateTree(rf.darl); //cache?
            runtime.ClearInputs(tree);
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
            //update dynamic inputs
            await rf.UpdateFromCode(stores);
            vals = await runtime.Evaluate(tree, vals);
            var ufsaliences = runtime.CalculateSaliences(vals, tree);
            if (ufsaliences.Any())
            {
                if (!valueSequence.Any()) //first in sequence
                {
                    responses.Add(new InteractTestResponse { response = new DarlVar { name = nameof(response), dataType = DarlVar.DataType.textual, Value = TextLookup("Format.preamble", rf) } });
                }
                var next = (from entry in ufsaliences orderby entry.Value descending select entry.Key).FirstOrDefault(); //find most salient
                valueSequence.Add(next);
                var informat = rf.format.InputFormatList.Where(a => a.Name == next).FirstOrDefault(); //match up with format
                if (informat != null)
                {
                    switch (informat.InType)
                    {
                        case InputFormat.InputType.categorical:
                            {
                                response.response = new DarlVar { dataType = DarlVar.DataType.categorical, weight = 0.0, unknown = true, Value = TextLookup(next, rf), name = next };
                                double n = informat.Categories.Count;
                                response.response.categories = new Dictionary<string, double>();
                                foreach (var cat in informat.Categories)
                                {
                                    var category = cat;
                                    if (cat.Contains("%%"))
                                    {
                                        category = cat.Substring(0, cat.IndexOf("%%"));
                                    }
                                    var txt = TextLookup($"{next}.{category}", rf, "", true);
                                    response.response.categories.Add(string.IsNullOrEmpty(txt) ? cat : txt, n);
                                    n -= 1.0;
                                }
                            }
                            break;
                        case InputFormat.InputType.numeric://values contain, min, max, increment
                            if (informat.ShowSets) //treat a numeric input as categorical using the set names
                            {
                                response.response = new DarlVar { dataType = DarlVar.DataType.categorical, weight = 0.0, unknown = true, Value = TextLookup(next, rf), name = next };
                                double n = informat.Categories.Count;
                                response.response.categories = new Dictionary<string, double>();
                                foreach (var cat in informat.Categories)
                                {
                                    var txt = TextLookup($"{next}.{cat}", rf, "", true);
                                    response.response.categories.Add(string.IsNullOrEmpty(txt) ? cat : txt, n);
                                    n -= 1.0;
                                }
                            }
                            else
                                response.response = new DarlVar { dataType = DarlVar.DataType.numeric, weight = 0.0, unknown = true, Value = TextLookup(next, rf), values = new List<double> { informat.NumericMin, informat.NumericMax, informat.Increment }, name = next };
                            break;
                        default: //textual. If regex exists put as first category, if maxlength put as first value
                            response.response = new DarlVar { dataType = DarlVar.DataType.textual, name = next, weight = 0.0, Value = TextLookup(next, rf), unknown = true, categories = string.IsNullOrEmpty(informat.Regex) ? null : new Dictionary<string, double> { { informat.Regex, 1.0 } }, values = informat.MaxLength > 0 ? new List<double> { informat.MaxLength } : null };
                            break;
                    }
                    pending = response.response;
                }
                responses.Add(response);
            }
            else
            {
                bool returningCall = false;
                //respond - multiple outputs may be present
                if (!String.IsNullOrEmpty(TextLookup("Format.resultHeader", rf)))
                    responses.Add(new InteractTestResponse { response = new DarlVar { name = nameof(response), dataType = DarlVar.DataType.textual, Value = TextLookup("Format.resultHeader", rf) } });
                foreach (var val in vals.Where(a => !runtime.GetInputNames(tree).Contains(a.name))) //i.e. outputs only.
                {
                    var outformat = rf.format.OutputFormatList.FirstOrDefault(a => a.Name == val.name);
                    if (outformat != null && !outformat.Hide)
                    {
                        string annotation = TextLookup(val.name, rf, "");
                        switch (val.dataType)
                        {
                            case DarlResult.DataType.categorical:
                                responses.Add(new InteractTestResponse { response = new DarlVar { name = nameof(response), dataType = DarlVar.DataType.textual, Value = $"{annotation} {TextLookup($"{val.name}.{val.Value.ToString()}", rf, "", true)}" } });
                                break;
                            case DarlResult.DataType.numeric:
                                responses.Add(new InteractTestResponse { response = new DarlVar { name = nameof(response), dataType = DarlVar.DataType.textual, Value = $"{annotation} {((double)val.Value).ToString(outformat.ValueFormat)}" } });
                                break;
                            case DarlResult.DataType.textual:
                                responses.Add(new InteractTestResponse { response = new DarlVar { name = nameof(response), dataType = DarlVar.DataType.textual, Value = $"{annotation} {val.Value.ToString()}" } });
                                break;
                        }
                    }
                    else if (val.name.Contains(".Call") && stores.ContainsKey("Call"))
                    {
                        returningCall = true;
                        responses.Add(new InteractTestResponse { response = new DarlVar { name = nameof(response), dataType = DarlVar.DataType.ruleset, Value = val.Value.ToString() } });
                    }
                }
                //trigger here
                if (Trigger != null)
                    await Trigger.TriggerEvent(DarlVarExtensions.Convert(vals), rf, user, service);
                values.Clear();
                valueSequence.Clear();
                runtime.ClearInputs(tree);
                pending = null;
                //signal complete to calling dialog, etc.
                if (!returningCall)
                    responses.Add(new InteractTestResponse { response = new DarlVar { name = nameof(response), dataType = DarlVar.DataType.complete, Value = string.Empty } });
            }
            return responses;
        }

        private string TextLookup(string reference, RuleForm cache, string language = "", bool noDefault = false)
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

        public void Quit()
        {
            //callStack.Clear();
        }

        public void Back(List<DarlVar> values)
        {
            var last = values.Where(a => a.name == valueSequence.Last()).FirstOrDefault();
            if (last != null)
            {
                values.Remove(last);
                valueSequence.RemoveAt(valueSequence.Count - 1);
                var question = values.Where(a => a.name == questionIdentifier).FirstOrDefault();
                if (question != null)
                {
                    values.Remove(question);
                }
            }
        }

        public bool CanGoBack()
        {
            return valueSequence.Any();
        }

        internal bool Validate(List<DarlVar> values, out string validationResponse)
        {
            validationResponse = "";
            if (pending != null)
            {
                var question = values.Where(a => a.name == questionIdentifier).FirstOrDefault();
                if (question != null)
                {
                    switch (pending.dataType)
                    {
                        case DarlVar.DataType.categorical:
                            string q = question.Value.Trim().ToLower();
                            foreach (var c in pending.categories.Keys)
                            {
                                if (c.ToLower() == q)
                                {
                                    //categories can be transcribed - revert to value used in ruleset.
                                    question.Value = LookupCategory(pending.name, question.Value, rf, "");
                                    return true;
                                }
                            }
                            validationResponse = "You must give one of the choices";
                            values.Remove(question);
                            return false;
                        case DarlVar.DataType.numeric:
                            double dval;
                            if (double.TryParse(question.Value, out dval))
                            {
                                if (dval >= pending.values[0] && dval <= pending.values[1])
                                    return true;
                                validationResponse = $"The value must be between {pending.values[0]} and {pending.values[1]} inclusive";
                                values.Remove(question);
                                return false;

                            }
                            validationResponse = "You must give a number";
                            values.Remove(question);
                            return false;
                        case DarlVar.DataType.textual:
                            if (pending.categories != null) //regex present
                            {
                                var regex = new Regex(pending.categories.Keys.First());
                                if (!regex.Match(question.Value.Trim()).Success)
                                {
                                    //some sources decorate emails etc with html. in this case search through attributes for a valid section
                                    var source = question.Value.Trim();
                                    int index = 0;
                                    while (index < source.Length - 1 && (index = source.IndexOf('"', index)) != -1)//search attributes
                                    {
                                        int endIndex = source.IndexOf('"', index + 1);
                                        if (endIndex != -1)
                                        {
                                            var poss = source.Substring(index + 1, endIndex - (index + 1));
                                            if (regex.Match(poss.Trim()).Success)
                                            {
                                                question.Value = poss.Trim();
                                                return true;
                                            }
                                            index = endIndex + 1;
                                        }
                                    }
                                    index = 0;
                                    while (index < source.Length - 1 && (index = source.IndexOf('>', index)) != -1)//search text elements
                                    {
                                        int endIndex = source.IndexOf('<', index + 1);
                                        if (endIndex != -1)
                                        {
                                            var poss = source.Substring(index + 1, endIndex - (index + 1));
                                            if (regex.Match(poss.Trim()).Success)
                                            {
                                                question.Value = poss.Trim();
                                                return true;
                                            }
                                            index = endIndex + 1;
                                        }
                                    }
                                    validationResponse = "The response is not in the right format";
                                    values.Remove(question);
                                    return false;
                                }

                            }
                            if (pending.values != null && pending.values.Count > 0) //first value is max length expressed as double
                            {
                                if (question.Value.Length > (int)pending.values[0])
                                {
                                    validationResponse = $"Text is longer than {(int)pending.values[0]} characters, the maximum allowed";
                                    values.Remove(question);
                                    return false;
                                }
                            }
                            break;
                    }
                }
            }
            return true;
        }

    }
}
