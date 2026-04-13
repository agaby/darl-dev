/// <summary>
/// FormFormatExtensions.cs - Core module for the Darl.dev project.
/// </summary>

﻿using DarlCommon;
using DarlCompiler.Parsing;
using DarlLanguage;
using DarlLanguage.Processing;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.Forms
{
    public static class FormFormatExtensions
    {

        public enum Sets
        {
            [Display(Name = "3")]
            three = 3,
            [Display(Name = "5")]
            five = 5,
            [Display(Name = "7")]
            seven = 7,
            [Display(Name = "9")]
            nine = 9
        }

        public static string CreateTestDataSchema(this FormFormat iOFormat)
        {

            var schema = new JObject(
                    new JProperty("type", "object"),
                    new JProperty("title", "Test"),
                    new JProperty("properties", new JObject(
                    from p in iOFormat.InputFormatList
                    orderby p.Name
                    select new JProperty(p.Name, CreateValue(p)),
                    from q in iOFormat.OutputFormatList
                    orderby q.Name
                    select new JProperty(q.Name, CreateValue(q))
                )));
            return schema.ToString();
        }

        /// <summary>
        /// creates a property tree based on the data type and settings
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private static JObject CreateValue(InputFormat p)
        {
            var list = new List<JProperty>();
            switch (p.InType)
            {
                case InputFormat.InputType.textual:
                    list.Add(new JProperty("type", "string"));
                    if (p.MaxLength > 0)
                    {
                        list.Add(new JProperty("maxLength", p.MaxLength));
                    }
                    if (!string.IsNullOrEmpty(p.Regex))
                    {
                        list.Add(new JProperty("pattern", p.Regex));
                    }
                    break;
                case InputFormat.InputType.numeric:
                    list.Add(new JProperty("type", "number"));
                    list.Add(new JProperty("minimum", p.NumericMin));
                    list.Add(new JProperty("maximum", p.NumericMax));
                    //add min,max
                    break;
                case InputFormat.InputType.temporal:
                    list.Add(new JProperty("type", "string"));
                    break;

                default:
                    list.Add(new JProperty("type", "string"));
                    list.Add(new JProperty("enum", new JArray(p.Categories)));
                    //add categories
                    break;
            }
            return new JObject(list.ToArray());

        }
        /// <summary>
        /// creates a property tree based on the data type and settings
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private static JObject CreateValue(OutputFormat p)
        {
            var list = new List<JProperty>();
            switch (p.OutputType)
            {
                case OutputFormat.OutType.textual:
                    list.Add(new JProperty("type", "string"));
                    break;
                case OutputFormat.OutType.numeric:
                    list.Add(new JProperty("type", "number"));
                    //add min,max
                    break;
                default:
                    list.Add(new JProperty("type", "string"));
                    //add categories
                    break;
            }
            return new JObject(list.ToArray());

        }

        /// <summary>
        /// Align the darl and the IO/texts. Run the tests
        /// </summary>
        /// <param name="pr">The ruleform holding the ruleset and ancillaries</param>
        /// <param name="stores">An optional set of stores used to process dynamic inputs</param>
        /// <returns>A list of test errors for use in the validation process.</returns>
        public static async Task<List<dynamic>> UpdateFromCode(this RuleForm pr, Dictionary<string, ILocalStore>? stores = null)
        {
            ParseTree tree = null;
            var runtime = new DarlRunTime();
            try
            {
                tree = runtime.CreateTree(pr.darl, stores);
                if (tree.HasErrors())
                    return new List<dynamic>();
            }
            catch
            {
                return new List<dynamic>();
            }
            if (pr.format == null)
                pr.format = new FormFormat();
            if (pr.language == null)
                pr.language = new LanguageFormat();

            //Inputs first
            bool inputformatChanges = false;
            bool outputformatChanges = false;
            bool langChanges = false;
            List<InputFormat> newInputs = new List<InputFormat>();
            List<OutputFormat> newOutputs = new List<OutputFormat>();
            List<LanguageText> newLanguage = new List<LanguageText>();
            foreach (var input in tree.GetMapInputs())
            {
                var type = ConvertInputType(tree.GetMapInputType(input.Name));
                //Check to see if this exists in the format stuff
                if (!pr.format.InputFormatList.Any(a => a.Name == input.Name))
                {
                    InputFormat inf = new InputFormat
                    {
                        Name = input.Name,
                        InType = type,
                        Categories = new List<string>()
                    };
                    foreach (var cat in tree.GetMapInputCategories(input.Name))
                        inf.Categories.Add(cat);
                    if (type == InputFormat.InputType.numeric)
                    {
                        var res = tree.GetMapInputRange(input.Name);
                        if (res.values.Count() >= 2)
                        {
                            inf.NumericMin = (double)res.values[0];
                            inf.NumericMax = (double)res.values.Last();
                        }
                    }
                    newInputs.Add(inf);
                    inputformatChanges = true;
                }
                else //already in list
                {
                    InputFormat inf = pr.format.InputFormatList.First(a => a.Name == input.Name);
                    //check type hasn't changed
                    inf.InType = type;
                    //revise categories
                    inf.Categories = new List<string>();
                    foreach (var cat in tree.GetMapInputCategories(input.Name))
                        inf.Categories.Add(cat);
                    newInputs.Add(inf);
                    inputformatChanges = true;
                }
                //now check for Language 
                if (!pr.language.LanguageList.Any(a => a.Name == input.Name))
                {
                    newLanguage.Add(new LanguageText { Name = input.Name, Text = CreateInitialTextFromName(input.Name), VariantList = new List<VariantText>() });
                    langChanges = true;
                }
                else
                {
                    newLanguage.Add(pr.language.LanguageList.First(a => a.Name == input.Name));
                }
                if (type == InputFormat.InputType.categorical)
                {
                    foreach (var cat in tree.GetMapInputCategories(input.Name))
                    {
                        //dynamic categories have both cat and text, separate
                        var text = cat;
                        var category = cat;
                        if (cat.EndsWith("%%"))
                        {
                            var elements = cat.Split('%');
                            category = elements[0];
                            text = elements[2];
                        }
                        string compName = input.Name + "." + category;
                        if (!pr.language.LanguageList.Any(a => a.Name == compName))
                        {
                            newLanguage.Add(new LanguageText { Name = compName, Text = text, VariantList = new List<VariantText>() });
                            langChanges = true;
                        }
                        else
                        {
                            newLanguage.Add(pr.language.LanguageList.First(a => a.Name == compName));
                        }
                    }
                }

            }
            if (inputformatChanges || newInputs.Count != pr.format.InputFormatList.Count)
            {
                pr.format.InputFormatList = newInputs;
            }
            //Now outputs
            foreach (var output in tree.GetMapOutputs())
            {
                var type = ConvertOutputType(tree.GetMapOutputType(output.Name));
                if (!pr.format.OutputFormatList.Any(a => a.Name == output.Name))
                {
                    var outForm = new OutputFormat { Hide = false, Name = output.Name, OutputType = type, displayType = OutputFormat.DisplayType.Text };
                    if (type == OutputFormat.OutType.numeric)
                    {
                        var res = tree.GetMapOutputRange(output.Name);
                        if (res.values.Count() >= 2)
                        {
                            outForm.ScoreBarMinVal = (double)res.values[0];
                            outForm.ScoreBarMaxVal = (double)res.values.Last();
                        }
                    }
                    newOutputs.Add(outForm);
                    outputformatChanges = true;
                }
                else
                {
                    OutputFormat outf = pr.format.OutputFormatList.First(a => a.Name == output.Name);
                    //check type hasn't changed
                    outf.OutputType = type;
                    if (outf.displayType == 0)
                        outf.displayType = OutputFormat.DisplayType.Text;
                    //revise categories
                    newOutputs.Add(outf);
                    outputformatChanges = true;
                }
                //now check for Language 
                if (!pr.language.LanguageList.Any(a => a.Name == output.Name))
                {
                    langChanges = true;
                    newLanguage.Add(new LanguageText { Name = output.Name, Text = CreateInitialTextFromName(output.Name), VariantList = new List<VariantText>() });
                }
                else
                {
                    newLanguage.Add(pr.language.LanguageList.First(a => a.Name == output.Name));
                }
                if (type == OutputFormat.OutType.categorical)
                {
                    foreach (var cat in tree.GetMapOutputCategories(output.Name))
                    {
                        string compName = output.Name + "." + cat;
                        if (!pr.language.LanguageList.Any(a => a.Name == compName))
                        {
                            newLanguage.Add(new LanguageText { Name = compName, Text = cat, VariantList = new List<VariantText>() });
                            langChanges = true;
                        }
                        else
                        {
                            newLanguage.Add(pr.language.LanguageList.First(a => a.Name == compName));
                        }
                    }
                }
            }
            //now fixed names for Format in language
            if (AddFixedNames(pr.language, newLanguage))
                langChanges = true;

            if (inputformatChanges || outputformatChanges || pr.format.DefaultQuestions == 0)
            {
                pr.format.OutputFormatList = newOutputs;
                if (pr.format.DefaultQuestions == 0)
                    pr.format.DefaultQuestions = 1;
            }
            if (langChanges)
            {
                pr.language.LanguageList = newLanguage;
            }
            //pass store names into the ruleform to be matched up in evaluation
            pr.storeNames = tree.GetMapStores().Select(a => a.Name).ToList();
            //now evaluate test data.
            var errors = new List<dynamic>();
            if (!string.IsNullOrEmpty(pr.testData))
            {
                var list = JArray.Parse(pr.testData);
                var inputs = tree.GetMapInputs();
                var outputs = tree.GetMapOutputs();
                foreach (var p in list)//for each pattern
                {
                    var results = new List<DarlResult>();
                    foreach (JProperty q in p) //foreach data item
                    {
                        //check against inputs
                        var i = inputs.Where(a => a.Name == q.Name).FirstOrDefault();
                        if (i != null)
                        {
                            results.Add(new DarlResult(q.Name, q.Value.ToString(), (DarlResult.DataType)Enum.Parse(typeof(DarlResult.DataType), ConvertInputType(tree.GetMapInputType(q.Name)).ToString())));
                        }
                    }
                    var res = await runtime.Evaluate(tree, results);
                    foreach (JProperty q in p) //foreach data item
                    {
                        //check against outputs
                        var o = outputs.Where(a => a.Name == q.Name).FirstOrDefault();
                        if (o != null)
                        {
                            if (res.Any(a => a.name == o.Name))
                            {
                                var resval = res.First(a => a.name == o.Name);
                                if (resval.Value != q.Value) //may be too exact for numeric...
                                {//add to validation error list
                                    var r = resval.Value ?? "empty";
                                    errors.Add(new { property = "required", path = q.Path, message = $"Result is {r}" });
                                }
                            }
                        }

                    }
                }
            }
            return errors;
        }

        /// <summary>
        /// Add fixed names to the list of language texts
        /// </summary>
        /// <param name="language"></param>
        /// <param name="newLanguage"></param>
        /// <returns></returns>
        public static bool AddFixedNames(LanguageFormat language, List<LanguageText> newLanguage)
        {
            bool langChanges = false;
            //now fixed names for Format in language
            var fixedNameRecord = "Format.preamble,Format.questionHeader,Format.resultHeader";
            foreach (var str in fixedNameRecord.Split(new char[] { ',' }))
            {
                if (!language.LanguageList.Any(a => a.Name == str))
                {
                    newLanguage.Add(new LanguageText { Name = str, Text = string.Empty, VariantList = new List<VariantText>() });
                    langChanges = true;
                }
                else
                {
                    newLanguage.Add(language.LanguageList.First(a => a.Name == str));
                }
            }
            return langChanges;
        }

        /// <summary>
        /// Create a new initialised RuleForm
        /// </summary>
        /// <param name="name">The name of the ruleset</param>
        /// <param name="username">The name of the creator</param>
        /// <returns>A new initialised RuleForm</returns>
        public static Task<RuleForm> CreateNewRuleForm(string name, string username)
        {
            var darl = $"ruleset {name.Trim().Replace(' ', '_')}\r\n{{\r\n}}";
            var rf = new RuleForm { darl = darl, name = name, version = "0.0", author = username, price = 0.0, trigger = DefaultTriggerView(), format = new FormFormat(), language = new LanguageFormat() };
            return Task.FromResult<RuleForm>(rf);
        }

        /// <summary>
        /// Create an initialised MLModel
        /// </summary>
        /// <param name="name"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public static Task<MLModel> CreateNewMLModel(string name, string username)
        {
            var darl = $"ruleset {Path.GetFileNameWithoutExtension(name).Trim().Replace(' ', '_')}\r\n{{\r\n}}";
            var mlm = new MLModel { author = username, name = name, darl = darl, percentTest = 0, sets = 3, version = "0.0", destinationRulesetName = $"{Path.GetFileNameWithoutExtension(name)}.rule" };
            return Task.FromResult<MLModel>(mlm);
        }

        /// <summary>
        /// Mine 
        /// </summary>
        /// <param name="rulesource"></param>
        /// <param name="datasource"></param>
        /// <param name="sets"></param>
        /// <param name="percentTrain"></param>
        /// <returns></returns>
        public static async Task<string> MineSupervised(string rulesource, string datasource, int sets = 3, int percentTrain = 100, DarlMineReport? rep = null)
        {
            var runtime = new DarlRunTime();
            return await runtime.MineSupervisedAsync(rulesource, datasource, sets, percentTrain, rep);
        }

        public static async Task<RuleForm> CreateRuleFormFromJsonSchema(string name, string username, string jsonSchemaString)
        {
            var rf = await CreateNewRuleForm(name, username);
            rf.darl = ConvertFromJson(jsonSchemaString);
            await UpdateFromCode(rf);
            return rf;
        }

        /// <summary>
        /// Create a rule set enabling querying and editing a GraphQL schema
        /// </summary>
        /// <param name="name"></param>
        /// <param name="username"></param>
        /// <param name="graphQLSchemaString"></param>
        /// <returns></returns>
        public static Task<RuleForm> CreateRuleFormFromGraphQLSchema(string name, string username, string graphQLSchemaString)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Convert a Json schema into darl
        /// </summary>
        /// <param name="jsonSchemaString"></param>
        /// <returns></returns>
        private static string ConvertFromJson(string jsonSchemaString)
        {
            JSchema schema = JSchema.Parse(jsonSchemaString);
            var sb = new StringBuilder();
            sb.AppendLine(RenderPreamble());
            var names = new HashSet<string>();
            var internals = new HashSet<string>();
            var subnames = new HashSet<string>();
            RenderRuleSet(schema, sb, "Root", names, internals, subnames);


            //add mapinputs, mapoutputs and wires

            sb.AppendLine();
            sb.AppendLine("mapoutput Root_complete;");
            sb.AppendLine();
            foreach (var s in names)
            {
                sb.AppendLine($"mapinput {s.Replace('.', '_')};");
            }
            sb.AppendLine();
            sb.AppendLine("wire Root.complete Root_complete;");
            foreach (var s in names)
            {
                sb.AppendLine($"wire {s.Replace('.', '_')} {s};");
            }
            //add internal wires
            foreach (var s in internals)
            {
                sb.AppendLine(s);
            }

            return sb.ToString();
        }

        /// <summary>
        /// initialize trigger view
        /// </summary>
        /// <returns></returns>
        private static TriggerView DefaultTriggerView()
        {
            return new TriggerView { sendEmailSource = SourceType.fixedvalue, sendEmail = "false", postDataSource = SourceType.fixedvalue, postData = "false", graphqlDataSource = SourceType.fixedvalue, graphqlData = "false" };
        }

        /// <summary>
        /// Converts the type of the input.
        /// </summary>
        /// <param name="ty">The type</param>
        /// <returns>InputFormat.InputType.</returns>
        private static InputFormat.InputType ConvertInputType(string ty)
        {
            if (ty.ToLower().Contains(InputFormat.InputType.categorical.ToString()))
                return InputFormat.InputType.categorical;
            if (ty.ToLower().Contains(InputFormat.InputType.numeric.ToString()))
                return InputFormat.InputType.numeric;
            if (ty.ToLower().Contains(InputFormat.InputType.temporal.ToString()))
                return InputFormat.InputType.temporal;
            return InputFormat.InputType.textual;
        }

        /// <summary>
        /// Converts the type of the output.
        /// </summary>
        /// <param name="ty">The type.</param>
        /// <returns>OutputFormat.OutType.</returns>
        private static OutputFormat.OutType ConvertOutputType(string ty)
        {
            if (ty.ToLower().Contains(OutputFormat.OutType.categorical.ToString()))
                return OutputFormat.OutType.categorical;
            if (ty.ToLower().Contains(OutputFormat.OutType.textual.ToString()))
                return OutputFormat.OutType.textual;
            if (ty.ToLower().Contains(OutputFormat.OutType.temporal.ToString()))
                return OutputFormat.OutType.temporal;
            return OutputFormat.OutType.numeric;
        }


        private static string RenderPreamble()
        {
            return $"/* Created by Darl.JsonSchema {DateTime.Now.ToShortDateString()}. */";
        }

        private static void RenderRuleSet(JSchema schema, StringBuilder sb, string ruleSetName, HashSet<string> names, HashSet<string> internals, HashSet<string> subNames, bool array = false)
        {
            var arrayString = array ? "//multiple" : "";
            var required = new Dictionary<string, string>();
            var subs = new Dictionary<string, SubRuleset>();
            sb.AppendLine();
            sb.AppendLine($"ruleset {ruleSetName} {arrayString}");
            sb.AppendLine($"{{");
            foreach (var propertyName in schema.Properties.Keys)
            {
                var val = schema.Properties[propertyName];
                switch (val.Type)
                {
                    case JSchemaType.String:
                        if (val.Format == "date-time" || val.Format == "date" || val.Format == "time")
                            sb.AppendLine($"\tinput temporal {propertyName};");
                        else
                            sb.AppendLine($"\tinput textual {propertyName};");
                        if (schema.Required.Any(a => a == propertyName))
                            required.Add(propertyName, propertyName);
                        names.Add($"{ruleSetName}.{propertyName}");
                        break;
                    case JSchemaType.Boolean:
                        sb.AppendLine($"\tinput categorical {propertyName} {{true,false}};");
                        if (schema.Required.Any(a => a == propertyName))
                            required.Add(propertyName, propertyName);
                        names.Add($"{ruleSetName}.{propertyName}");
                        break;
                    case JSchemaType.Integer:
                    case JSchemaType.Number:
                        sb.AppendLine($"\tinput numeric {propertyName};");
                        if (schema.Required.Any(a => a == propertyName))
                            required.Add(propertyName, propertyName);
                        names.Add($"{ruleSetName}.{propertyName}");
                        break;
                    case JSchemaType.Array:
                        sb.AppendLine($"\tinput categorical {propertyName}_complete {{true,false}};");
                        if (!subNames.Contains(propertyName))
                        {
                            subs.Add(propertyName, new SubRuleset { array = true, name = propertyName, obj = val.Items[0] });
                            subNames.Add(propertyName);
                        }
                        if (schema.Required.Any(a => a == propertyName))
                            required.Add(propertyName, $"{propertyName}_complete");
                        internals.Add($"wire {propertyName}.complete {ruleSetName}.{propertyName}_complete;");
                        break;
                    case JSchemaType.Object:
                        sb.AppendLine($"\tinput categorical {propertyName}_complete {{true,false}};");
                        if (!subNames.Contains(propertyName))
                        {
                            subs.Add(propertyName, new SubRuleset { array = false, name = propertyName, obj = val });
                            subNames.Add(propertyName);
                        }
                        if (schema.Required.Any(a => a == propertyName))
                            required.Add(propertyName, $"{propertyName}_complete");
                        internals.Add($"wire {propertyName}.complete {ruleSetName}.{propertyName}_complete;");
                        break;
                }
            }
            sb.AppendLine();
            sb.AppendLine($"\toutput categorical complete {{true,false}};");

            var lhs = string.Join(" is present and ", required.Values) + " is present";

            sb.AppendLine($"\tif {lhs} then complete will be true;");
            sb.AppendLine($"}}");

            foreach (string s in subs.Keys)
            {
                var sub = subs[s];
                RenderRuleSet(sub.obj, sb, s, names, internals, subNames, sub.array);
            }


        }

        /// <summary>
        /// Creates initial text for language from the name of IO given
        /// </summary>
        /// <param name="name">The name of the IO</param>
        /// <returns>The suggested text</returns>
        /// <remarks>Considers underscore, hyphen separated and camel casing</remarks>
        private static string CreateInitialTextFromName(string name)
        {
            var res = name.Replace('_', ' ');
            res = res.Replace('-', ' ');
            string text = string.Empty;
            text += char.ToUpper(res[0]);
            for (int n = 1; n < res.Length; n++)
            {
                if (char.IsUpper(res[n]) && char.IsLower(res[n - 1]))
                {
                    text += ' ';
                }
                text += char.ToLower(res[n]);
            }
            return text;
        }

        public class SubRuleset
        {
            public string name { get; set; }

            public bool array { get; set; } = false;

            public JSchema obj { get; set; }
        }

    }


}
