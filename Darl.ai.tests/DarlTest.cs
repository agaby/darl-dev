using Darl.Lacuna;
using DarlCommon;
using DarlCompiler.Parsing;
using DarlLanguage;
using DarlLanguage.Processing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Darl_standard_core.test
{
    [TestClass]
    public class MRuleTest
    {
        [TestMethod, TestCategory("Darl functionality")]
        public void TestMRuleParsing1()
        {

            LanguageData language = new LanguageData(new DarlGrammar());
            Parser parser = new Parser(language);
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.FirstProg.darl"));
            ParseTree parseTree = parser.Parse(reader.ReadToEnd(), null);
            ParseTreeNode root = parseTree.Root;
            Assert.IsNotNull(root);
        }

        [TestMethod, TestCategory("Darl functionality")]
        public void TestMRuleParsing2()
        {

            LanguageData language = new LanguageData(new DarlGrammar());
            Parser parser = new Parser(language);
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.SecondRule.darl"));
            ParseTree parseTree = parser.Parse(reader.ReadToEnd(), null);
            ParseTreeNode root = parseTree.Root;
            Assert.IsNotNull(root);
        }

        [TestMethod, TestCategory("Darl Map functionality")]
        public void TestMRuleParsingMap()
        {

            LanguageData language = new LanguageData(new DarlGrammar());
            Parser parser = new Parser(language);
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.MapTest.darl"));
            ParseTree parseTree = parser.Parse(reader.ReadToEnd());
            ParseTreeNode root = parseTree.Root;
            Assert.IsNotNull(root);
        }

        [TestMethod, TestCategory("Darl functionality")]
        public async Task TestMRuleEvaluation()
        {
            DarlGrammar grammar = new DarlGrammar();
            LanguageData language = new LanguageData(grammar);
            Parser parser = new Parser(language);
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.FirstProg.darl"));
            ParseTree parseTree = parser.Parse(reader.ReadToEnd());
            await grammar.RunSample(new RunSampleArgs(language, "", parseTree));
        }

        [TestMethod, TestCategory("Darl functionality")]
        [ExpectedException(typeof(RuleException))]
        public async Task TestMRuleEvaluationLoop()
        {
            DarlGrammar grammar = new DarlGrammar();
            LanguageData language = new LanguageData(grammar);
            Parser parser = new Parser(language);
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.LoopProg.darl"));
            ParseTree parseTree = parser.Parse(reader.ReadToEnd());
            await grammar.RunSample(new RunSampleArgs(language, "", parseTree));
        }

        [TestMethod, TestCategory("Darl functionality")]
        public async Task TestMRuleEvaluationSequence()
        {
            DarlGrammar grammar = new DarlGrammar();
            LanguageData language = new LanguageData(grammar);
            Parser parser = new Parser(language);
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.SequenceProg.darl"));
            ParseTree parseTree = parser.Parse(reader.ReadToEnd());
            await grammar.RunSample(new RunSampleArgs(language, "", parseTree));
        }

        [TestMethod, TestCategory("Darl functionality")]
        public async Task TestMRuleRunTime()
        {
            DarlRunTime runtime = new DarlRunTime();
            var results = new List<DarlResult>();
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.SequenceProg.darl"));
            await runtime.Evaluate(results, reader.ReadToEnd(), "SequenceProg");
            Assert.AreEqual(6, results.Count);
        }

        [TestMethod, TestCategory("Darl functionality")]
        public async Task TestMRuleRunTimeUKTax()
        {
            DarlRunTime runtime = new DarlRunTime();
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.UKTax.darl"));
            string source = reader.ReadToEnd();
            var results = new List<DarlResult>
            {
                new DarlResult("UKTax.AGE_YEARS", 56),
                new DarlResult("UKTax.EARNED_INCOME", 35000),
                new DarlResult("UKTax.DIVIDEND_INCOME", 0),
                new DarlResult("UKTax.BLIND", "False"),
                new DarlResult("UKTax.MARRIED", "False")
            };
            await runtime.Evaluate(results, source, "UKTax");
            Assert.AreEqual(3288.96, (double)results.First(async => async.name == "UKTax.NI").values[0], 0.01);
            Assert.AreEqual(8105, (double)results.First(async => async.name == "UKTax.TOTAL_ALLOWANCES").values[0], 0.01);
            Assert.AreEqual(5379.00, (double)results.First(async => async.name == "UKTax.EARNED_TAX").values[0], 0.01);
            Assert.AreEqual(5379.00, (double)results.First(async => async.name == "UKTax.TOTAL_TAX").values[0], 0.01);
            Assert.AreEqual(0, (double)results.First(async => async.name == "UKTax.DIVIDEND_TAX").values[0], 0.01);
            results.Clear();
            results.Add(new DarlResult("UKTax.AGE_YEARS", 56));
            results.Add(new DarlResult("UKTax.EARNED_INCOME", 55000));
            results.Add(new DarlResult("UKTax.DIVIDEND_INCOME", 0));
            results.Add(new DarlResult("UKTax.BLIND", "False"));
            results.Add(new DarlResult("UKTax.MARRIED", "False"));
            await runtime.Evaluate(results, source, "UKTax");
            Assert.AreEqual(4437.36, (double)results.First(async => async.name == "UKTax.NI").values[0], 1.0);
            Assert.AreEqual(8105, (double)results.First(async => async.name == "UKTax.TOTAL_ALLOWANCES").values[0], 1.0);
            Assert.AreEqual(11884.0, (double)results.First(async => async.name == "UKTax.EARNED_TAX").values[0], 1.0);
            Assert.AreEqual(11884.0, (double)results.First(async => async.name == "UKTax.TOTAL_TAX").values[0], 1.0);
            Assert.AreEqual(0, (double)results.First(async => async.name == "UKTax.DIVIDEND_TAX").values[0], 1.0);
            results.Clear();
            results.Add(new DarlResult("UKTax.AGE_YEARS", 56));
            results.Add(new DarlResult("UKTax.EARNED_INCOME", 105000));
            results.Add(new DarlResult("UKTax.DIVIDEND_INCOME", 0));
            results.Add(new DarlResult("UKTax.BLIND", "False"));
            results.Add(new DarlResult("UKTax.MARRIED", "False"));
            await runtime.Evaluate(results, source, "UKTax");
            Assert.AreEqual(5437.36, (double)results.First(async => async.name == "UKTax.NI").values[0], 1.0);
            Assert.AreEqual(5605, (double)results.First(async => async.name == "UKTax.TOTAL_ALLOWANCES").values[0], 1.0);
            Assert.AreEqual(32884.0, (double)results.First(async => async.name == "UKTax.EARNED_TAX").values[0], 1.0);
            Assert.AreEqual(32884.0, (double)results.First(async => async.name == "UKTax.TOTAL_TAX").values[0], 1.0);
            Assert.AreEqual(0, (double)results.First(async => async.name == "UKTax.DIVIDEND_TAX").values[0], 1.0);
            results.Clear();
            results.Add(new DarlResult("UKTax.AGE_YEARS", 56));
            results.Add(new DarlResult("UKTax.EARNED_INCOME", 160000));
            results.Add(new DarlResult("UKTax.DIVIDEND_INCOME", 0));
            results.Add(new DarlResult("UKTax.BLIND", "False"));
            results.Add(new DarlResult("UKTax.MARRIED", "False"));
            await runtime.Evaluate(results, source, "UKTax");
            Assert.AreEqual(6537.36, (double)results.First(async => async.name == "UKTax.NI").values[0], 1.0);
            Assert.AreEqual(0, (double)results.First(async => async.name == "UKTax.TOTAL_ALLOWANCES").values[0], 1.0);
            Assert.AreEqual(58126.0, (double)results.First(async => async.name == "UKTax.EARNED_TAX").values[0], 1.0);
            Assert.AreEqual(58126.0, (double)results.First(async => async.name == "UKTax.TOTAL_TAX").values[0], 1.0);
            Assert.AreEqual(0, (double)results.First(async => async.name == "UKTax.DIVIDEND_TAX").values[0], 1.0);
            results.Clear();
            results.Add(new DarlResult("UKTax.AGE_YEARS", 56));
            results.Add(new DarlResult("UKTax.EARNED_INCOME", 35000));
            results.Add(new DarlResult("UKTax.DIVIDEND_INCOME", 0));
            results.Add(new DarlResult("UKTax.BLIND", "True"));
            results.Add(new DarlResult("UKTax.MARRIED", "False"));
            await runtime.Evaluate(results, source, "UKTax");
            Assert.AreEqual(3288.96, (double)results.First(async => async.name == "UKTax.NI").values[0], 0.01);
            Assert.AreEqual(10205, (double)results.First(async => async.name == "UKTax.TOTAL_ALLOWANCES").values[0], 0.01);
            Assert.AreEqual(4959.00, (double)results.First(async => async.name == "UKTax.EARNED_TAX").values[0], 0.01);
            Assert.AreEqual(4959.00, (double)results.First(async => async.name == "UKTax.TOTAL_TAX").values[0], 0.01);
            Assert.AreEqual(0, (double)results.First(async => async.name == "UKTax.DIVIDEND_TAX").values[0], 0.01);
            results.Clear();
            results.Add(new DarlResult("UKTax.AGE_YEARS", 56));
            results.Add(new DarlResult("UKTax.EARNED_INCOME", 35000));
            results.Add(new DarlResult("UKTax.DIVIDEND_INCOME", 20000));
            results.Add(new DarlResult("UKTax.BLIND", "False"));
            results.Add(new DarlResult("UKTax.MARRIED", "False"));
            await runtime.Evaluate(results, source, "UKTax");
            Assert.AreEqual(3288.96, (double)results.First(async => async.name == "UKTax.NI").values[0], 1.0);
            Assert.AreEqual(8105, (double)results.First(async => async.name == "UKTax.TOTAL_ALLOWANCES").values[0], 1.0);
            Assert.AreEqual(5379.00, (double)results.First(async => async.name == "UKTax.EARNED_TAX").values[0], 1.0);
            Assert.AreEqual(3317.00, (double)results.First(async => async.name == "UKTax.DIVIDEND_TAX").values[0], 1.0);
            Assert.AreEqual(8696.00, (double)results.First(async => async.name == "UKTax.TOTAL_TAX").values[0], 1.0);
            results.Clear();
            results.Add(new DarlResult("UKTax.AGE_YEARS", 56));
            results.Add(new DarlResult("UKTax.EARNED_INCOME", 0));
            results.Add(new DarlResult("UKTax.DIVIDEND_INCOME", 0));
            results.Add(new DarlResult("UKTax.BLIND", "False"));
            results.Add(new DarlResult("UKTax.MARRIED", "False"));
            await runtime.Evaluate(results, source, "UKTax");
            Assert.AreEqual(0, (double)results.First(async => async.name == "UKTax.NI").values[0], 1.0);
            Assert.AreEqual(8105, (double)results.First(async => async.name == "UKTax.TOTAL_ALLOWANCES").values[0], 1.0);
            Assert.AreEqual(0, (double)results.First(async => async.name == "UKTax.EARNED_TAX").values[0], 1.0);
            Assert.AreEqual(0, (double)results.First(async => async.name == "UKTax.DIVIDEND_TAX").values[0], 1.0);
            Assert.AreEqual(0, (double)results.First(async => async.name == "UKTax.TOTAL_TAX").values[0], 1.0);
            Assert.AreEqual(0, (double)results.First(async => async.name == "UKTax.TAX_TAKE_PERCENT").values[0], 1.0);
        }

        [TestMethod, TestCategory("Darl functionality")]
        public async Task TestMRuleRunTimeNumericOut()
        {
            DarlRunTime runtime = new DarlRunTime();
            var results = new List<DarlResult>();
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.NumericOut.darl"));
            await runtime.Evaluate(results, reader.ReadToEnd(), "NumericOut");
            Assert.AreEqual(2, results.Count);
        }

        [TestMethod, TestCategory("Darl functionality")]
        public async Task TestMRuleOpOrder()
        {
            DarlRunTime runtime = new DarlRunTime();
            var results = new List<DarlResult>
            {
                new DarlResult("oporder.EARNED_INCOME", 2),
                new DarlResult("oporder.DIVIDEND_INCOME", 1)
            };
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.oporder.darl"));
            await runtime.Evaluate(results, reader.ReadToEnd(), "oporder");
            Assert.AreEqual(4, results.Count);
            Assert.AreEqual(116, (double)results.First(async => async.name == "oporder.TOTAL_ALLOWANCES").values[0], 1.0);
        }


        [TestMethod, TestCategory("Darl functionality")]
        public void TestToDarl()
        {
            LanguageData language = new LanguageData(new DarlGrammar());
            Parser parser = new Parser(language);
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.FirstProg.darl"));
            ParseTree parseTree = parser.Parse(reader.ReadToEnd());
            string newDarl = parseTree.ToDarl();
        }


        [TestMethod, TestCategory("Darl Errors")]
        public void TestSimpleError()
        {
            DarlGrammar grammar = new DarlGrammar();
            LanguageData language = new LanguageData(grammar);
            Parser parser = new Parser(language);
            string source = "ruleset fred { input numeric fred {{small, 1, 2, 3},{medium, 2, 3, 4},{large, 3, 4, 5}};\npoo\nif anything then fred will be small;}";
            ParseTree parseTree = parser.Parse(source);
            Assert.IsTrue(parseTree.HasErrors());
            Assert.AreEqual(4, parseTree.ParserMessages.Count);
            Assert.AreEqual(1, parseTree.ParserMessages[1].Location.Line);
            Assert.AreEqual(0, parseTree.ParserMessages[1].Location.Column);
            Assert.AreEqual("Syntax error, expected: if, input, output, constant, string, sequence, store, duration, otherwise, }", parseTree.ParserMessages[1].Message);
            source = "ruleset fred {input numeric fred {{small, 1, 2, 3},{medium, 2, 3, 4},{large, 3, 4, 5}};\nif anything then fred wont be small; }";
            parseTree = parser.Parse(source);
            Assert.IsTrue(parseTree.HasErrors());
            Assert.AreEqual(9, parseTree.ParserMessages.Count);
            Assert.AreEqual(1, parseTree.ParserMessages[5].Location.Line);
            Assert.AreEqual(22, parseTree.ParserMessages[5].Location.Column);
            //            Assert.AreEqual("Syntax error, expected: will", parseTree.ParserMessages[5].Message);
            Assert.AreEqual(1, parseTree.ParserMessages[0].Location.Line);
            Assert.AreEqual(17, parseTree.ParserMessages[0].Location.Column);
            Assert.AreEqual("Wrong IO Type", parseTree.ParserMessages[0].Message);
        }


        [TestMethod, TestCategory("Darl Errors")]
        public void TestUndefinedOutputError()
        {
            DarlGrammar grammar = new DarlGrammar();
            LanguageData language = new LanguageData(grammar);
            Parser parser = new Parser(language);
            string source = "input numeric fred {{small, 1, 2, 3},{medium, 2, 3, 4},{large, 3, 4, 5}};\nif anything then bill will be small";
            ParseTree parseTree = parser.Parse(source);
            Assert.IsTrue(parseTree.HasErrors());
        }

        [TestMethod, TestCategory("Darl Errors")]
        public void TestUndefinedSetError()
        {
            DarlGrammar grammar = new DarlGrammar();
            LanguageData language = new LanguageData(grammar);
            Parser parser = new Parser(language);
            string source = "input numeric fred {{small, 1, 2, 3},{medium, 2, 3, 4},{large, 3, 4, 5}};\nif anything then fred will be huge";
            ParseTree parseTree = parser.Parse(source);
            Assert.IsTrue(parseTree.HasErrors());
        }

        [TestMethod, TestCategory("Darl Supervised learning")]
        public void TestIris()
        {
            DarlRunTime runtime = new DarlRunTime();
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.IrisShell.darl"));
            var trainSourceReader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.iris_data.xml"));
            DarlMineReport rep = new DarlMineReport();
            string data = trainSourceReader.ReadToEnd();
            string code = reader.ReadToEnd();
            var newSource = runtime.MineSupervised(code, data, 3, 100, rep);
            Assert.IsTrue(rep.trainPerformance > 90.0);
            Assert.AreEqual(100, rep.trainPercent);
            Assert.AreEqual(0.0, rep.testPerformance);
            newSource = runtime.MineSupervised(code, data, 5, 100, rep);
            Assert.IsTrue(rep.trainPerformance > 90.0);
            Assert.AreEqual(100, rep.trainPercent);
            Assert.AreEqual(0.0, rep.testPerformance);
            newSource = runtime.MineSupervised(code, data, 7, 100, rep);
            Assert.IsTrue(rep.trainPerformance > 90.0);
            Assert.AreEqual(100, rep.trainPercent);
            Assert.AreEqual(0.0, rep.testPerformance);
            newSource = runtime.MineSupervised(code, data, 9, 100, rep);
            Assert.IsTrue(rep.trainPerformance > 90.0);
            Assert.AreEqual(100, rep.trainPercent);
            Assert.AreEqual(0.0, rep.testPerformance);            //accuracy 93.45% - should be 97.368
            Assert.IsTrue(newSource.Contains("-Infinity")); //test that machine learning doesn't use the infinity symbol, because can't be represented in ascii.
        }

        [TestMethod, TestCategory("Darl Supervised learning")]
        public async Task TestIrisAsync()
        {
            DarlRunTime runtime = new DarlRunTime();
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.IrisShell.darl"));
            var trainSourceReader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.iris_data.xml"));
            DarlMineReport rep = new DarlMineReport();
            string data = trainSourceReader.ReadToEnd();
            string code = reader.ReadToEnd();
            var newSource = await runtime.MineSupervisedAsync(code, data, 3, 100, rep);
            Assert.IsTrue(rep.trainPerformance > 90.0);
            Assert.AreEqual(100, rep.trainPercent);
            Assert.AreEqual(0.0, rep.testPerformance);
            newSource = await runtime.MineSupervisedAsync(code, data, 5, 100, rep);
            Assert.IsTrue(rep.trainPerformance > 90.0);
            Assert.AreEqual(100, rep.trainPercent);
            Assert.AreEqual(0.0, rep.testPerformance);
            newSource = await runtime.MineSupervisedAsync(code, data, 7, 100, rep);
            Assert.IsTrue(rep.trainPerformance > 90.0);
            Assert.AreEqual(100, rep.trainPercent);
            Assert.AreEqual(0.0, rep.testPerformance);
            newSource = await runtime.MineSupervisedAsync(code, data, 9, 100, rep);
            Assert.IsTrue(rep.trainPerformance > 90.0);
            Assert.AreEqual(100, rep.trainPercent);
            Assert.AreEqual(0.0, rep.testPerformance);            //accuracy 93.45% - should be 97.368
        }

        [TestMethod, TestCategory("Darl Lacuna")]
        //[Ignore]
        public async Task TestLacuna()
        {
            DarlRunTime runtime = new DarlRunTime();
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.IrisShell.darl"));
            var trainSourceReader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.iris_data.xml"));
            DarlMineReport rep = new DarlMineReport();
            string data = trainSourceReader.ReadToEnd();
            string code = reader.ReadToEnd();
            var newSource = await runtime.MineSupervisedAsync(code, data, 3, 100, rep);
            var lf = new LacunaFinder();
            var report = await lf.Find(newSource);

        }


        [TestMethod, TestCategory("Darl Supervised learning")]
        public void TestCleveHeart()
        {
            DarlRunTime runtime = new DarlRunTime();
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.cleve_heart.darl"));
            var trainSourceReader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.cleve_heart.xml"));
            DarlMineReport rep = new DarlMineReport();
            var newSource = runtime.MineSupervised(reader.ReadToEnd(), trainSourceReader.ReadToEnd(), 3, 100, rep);
            Assert.IsTrue(rep.trainPerformance > 68.0);
            Assert.AreEqual(100, rep.trainPercent);
            Assert.AreEqual(0.0, rep.testPerformance);
        }

        [TestMethod, TestCategory("Darl Supervised learning")]
        public void TestSine()
        {
            DarlRunTime runtime = new DarlRunTime();
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.sine.darl"));
            string code = reader.ReadToEnd();
            var trainSourceReader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.sine.xml"));
            string source = trainSourceReader.ReadToEnd();
            DarlMineReport rep = new DarlMineReport();
            var newSource = runtime.MineSupervised(code, source, 9, 100, rep);
            Assert.IsTrue(rep.trainPerformance < 0.05);
            Assert.AreEqual(100, rep.trainPercent);
            Assert.AreEqual(0.0, rep.testPerformance);
            runtime.MineSupervised(code, source, 7, 100, rep);
            Assert.IsTrue(rep.trainPerformance < 0.07);
            Assert.AreEqual(100, rep.trainPercent);
            Assert.AreEqual(0.0, rep.testPerformance);
            runtime.MineSupervised(code, source, 5, 100, rep);
            Assert.IsTrue(rep.trainPerformance < 0.11);
            Assert.AreEqual(100, rep.trainPercent);
            Assert.AreEqual(0.0, rep.testPerformance);
            runtime.MineSupervised(code, source, 3, 100, rep);
            Assert.IsTrue(rep.trainPerformance < 0.15);
            Assert.AreEqual(100, rep.trainPercent);
            Assert.AreEqual(0.0, rep.testPerformance);
        }

        [TestMethod, TestCategory("Darl map inference")]
        public async Task TestMultipleRuleMap()
        {
            DarlRunTime runtime = new DarlRunTime();
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.MultipleRuleSet.darl"));
            var results = new List<DarlResult>
            {
                new DarlResult("age", 56),
                new DarlResult("earned_income", 35000),
                new DarlResult("dividend_income", 35000),
                new DarlResult("blind", "False"),
                new DarlResult("married", "False")
            };
            await runtime.Evaluate(results, reader.ReadToEnd());
            Assert.AreEqual(5379.0, results.First(async => async.name == "earned_tax").Value);
        }

        [TestMethod, TestCategory("Darl Supervised learning")]
        public void TestFindSetBoundaries()
        {
            DarlRunTime runtime = new DarlRunTime();
            var input = new InputDefinitionNode();
            var values = new List<double>();
            double val = 0.0;
            for (int n = 0; n < 100; n++)
            {
                values.Add(val);
                val += 1.0;
            }
            runtime.FindSetBoundaries(3, input, values);
            Assert.AreEqual(3, input.sets.Count);
            Assert.AreEqual("small", input.categories[0]);
            Assert.AreEqual(double.NegativeInfinity, (double)input.sets["small"].values[0]);
            Assert.AreEqual(0, (double)input.sets["small"].values[1]);
            Assert.AreEqual(50, (double)input.sets["small"].values[2]);
            Assert.AreEqual(0, (double)input.sets["medium"].values[0]);
            Assert.AreEqual(50, (double)input.sets["medium"].values[1]);
            Assert.AreEqual(99, (double)input.sets["medium"].values[2]);
            Assert.AreEqual(50, (double)input.sets["large"].values[0]);
            Assert.AreEqual(99, (double)input.sets["large"].values[1]);
            Assert.AreEqual(double.PositiveInfinity, (double)input.sets["large"].values[2]);
            input = new InputDefinitionNode();
            runtime.FindSetBoundaries(5, input, values);
            Assert.AreEqual(5, input.sets.Count);
            Assert.AreEqual("very_small", input.categories[0]);
            Assert.AreEqual(double.NegativeInfinity, (double)input.sets["very_small"].values[0]);
            Assert.AreEqual(0, (double)input.sets["very_small"].values[1]);
            Assert.AreEqual(25, (double)input.sets["very_small"].values[2]);
            Assert.AreEqual(0, (double)input.sets["small"].values[0]);
            Assert.AreEqual(25, (double)input.sets["small"].values[1]);
            Assert.AreEqual(50, (double)input.sets["small"].values[2]);
            Assert.AreEqual(25, (double)input.sets["medium"].values[0]);
            Assert.AreEqual(50, (double)input.sets["medium"].values[1]);
            Assert.AreEqual(75, (double)input.sets["medium"].values[2]);
            Assert.AreEqual(50, (double)input.sets["large"].values[0]);
            Assert.AreEqual(75, (double)input.sets["large"].values[1]);
            Assert.AreEqual(99, (double)input.sets["large"].values[2]);
            Assert.AreEqual(75, (double)input.sets["very_large"].values[0]);
            Assert.AreEqual(99, (double)input.sets["very_large"].values[1]);
            Assert.AreEqual(double.PositiveInfinity, (double)input.sets["very_large"].values[2]);
            input = new InputDefinitionNode();
            runtime.FindSetBoundaries(7, input, values);
            Assert.AreEqual(7, input.sets.Count);
            Assert.AreEqual("very_small", input.categories[0]);
            Assert.AreEqual(double.NegativeInfinity, (double)input.sets["very_small"].values[0]);
            Assert.AreEqual(0, (double)input.sets["very_small"].values[1]);
            Assert.AreEqual(16, (double)input.sets["very_small"].values[2]);
            Assert.AreEqual(0, (double)input.sets["small"].values[0]);
            Assert.AreEqual(16, (double)input.sets["small"].values[1]);
            Assert.AreEqual(33, (double)input.sets["small"].values[2]);
            Assert.AreEqual(16, (double)input.sets["quite_small"].values[0]);
            Assert.AreEqual(33, (double)input.sets["quite_small"].values[1]);
            Assert.AreEqual(50, (double)input.sets["quite_small"].values[2]);
            Assert.AreEqual(33, (double)input.sets["medium"].values[0]);
            Assert.AreEqual(50, (double)input.sets["medium"].values[1]);
            Assert.AreEqual(66, (double)input.sets["medium"].values[2]);
            Assert.AreEqual(50, (double)input.sets["quite_large"].values[0]);
            Assert.AreEqual(66, (double)input.sets["quite_large"].values[1]);
            Assert.AreEqual(83, (double)input.sets["quite_large"].values[2]);
            Assert.AreEqual(66, (double)input.sets["large"].values[0]);
            Assert.AreEqual(83, (double)input.sets["large"].values[1]);
            Assert.AreEqual(99, (double)input.sets["large"].values[2]);
            Assert.AreEqual(83, (double)input.sets["very_large"].values[0]);
            Assert.AreEqual(99, (double)input.sets["very_large"].values[1]);
            Assert.AreEqual(double.PositiveInfinity, (double)input.sets["very_large"].values[2]);
            input = new InputDefinitionNode();
            runtime.FindSetBoundaries(9, input, values);
            Assert.AreEqual(9, input.sets.Count);
            Assert.AreEqual("extremely_small", input.categories[0]);
            Assert.AreEqual(double.NegativeInfinity, (double)input.sets["extremely_small"].values[0]);
            Assert.AreEqual(0, (double)input.sets["extremely_small"].values[1]);
            Assert.AreEqual(12, (double)input.sets["extremely_small"].values[2]);
            Assert.AreEqual(0, (double)input.sets["very_small"].values[0]);
            Assert.AreEqual(12, (double)input.sets["very_small"].values[1]);
            Assert.AreEqual(25, (double)input.sets["very_small"].values[2]);
            Assert.AreEqual(12, (double)input.sets["small"].values[0]);
            Assert.AreEqual(25, (double)input.sets["small"].values[1]);
            Assert.AreEqual(37, (double)input.sets["small"].values[2]);
            Assert.AreEqual(25, (double)input.sets["quite_small"].values[0]);
            Assert.AreEqual(37, (double)input.sets["quite_small"].values[1]);
            Assert.AreEqual(50, (double)input.sets["quite_small"].values[2]);
            Assert.AreEqual(37, (double)input.sets["medium"].values[0]);
            Assert.AreEqual(50, (double)input.sets["medium"].values[1]);
            Assert.AreEqual(62, (double)input.sets["medium"].values[2]);
            Assert.AreEqual(50, (double)input.sets["quite_large"].values[0]);
            Assert.AreEqual(62, (double)input.sets["quite_large"].values[1]);
            Assert.AreEqual(75, (double)input.sets["quite_large"].values[2]);
            Assert.AreEqual(62, (double)input.sets["large"].values[0]);
            Assert.AreEqual(75, (double)input.sets["large"].values[1]);
            Assert.AreEqual(87, (double)input.sets["large"].values[2]);
            Assert.AreEqual(75, (double)input.sets["very_large"].values[0]);
            Assert.AreEqual(87, (double)input.sets["very_large"].values[1]);
            Assert.AreEqual(99, (double)input.sets["very_large"].values[2]);
            Assert.AreEqual(87, (double)input.sets["extremely_large"].values[0]);
            Assert.AreEqual(99, (double)input.sets["extremely_large"].values[1]);
            Assert.AreEqual(double.PositiveInfinity, (double)input.sets["extremely_large"].values[2]);
        }

        [TestMethod, TestCategory("Darl Supervised learning")]
        [ExpectedException(typeof(RuleException))]
        public void TestFindSetBoundariesException()
        {
            DarlRunTime runtime = new DarlRunTime();
            var input = new InputDefinitionNode();
            var values = new List<double>();
            double val = 0.0;
            for (int n = 0; n < 100; n++)
            {
                values.Add(val);
                val += 1.0;
            }
            runtime.FindSetBoundaries(6, input, values); //illegal set choice
        }

        [TestMethod, TestCategory("Darl Supervised learning")]
        public void TestCalculateSetMembership()
        {
            DarlRunTime runtime = new DarlRunTime();
            var input = new InputDefinitionNode();
            var values = new List<double>();
            double val = 0.0;
            for (int n = 0; n < 100; n++)
            {
                values.Add(val);
                val += 1.0;
            }
            runtime.FindSetBoundaries(3, input, values);
            double tolerance = 0.002;
            for (int n = 0; n < 100; n++)
            {
                var setVal = input.CalculateSetMembership(input.learningSource[n], 0);
                Assert.IsTrue(setVal >= 0.0 && setVal <= 1.0);
                if (n >= 50)
                    Assert.AreEqual(0.0, setVal);
                DarlResult res = new DarlResult(values[n], values[n]);
                Assert.AreEqual(setVal, (double)input.sets[input.categories[0]].Equal(res).values[0], tolerance);
            }
            for (int n = 0; n < 100; n++)
            {
                var setVal = input.CalculateSetMembership(input.learningSource[n], 1);
                Assert.IsTrue(setVal >= 0.0 && setVal <= 1.0);
                if (n == 50)
                    Assert.AreEqual(1.0, setVal);
                DarlResult res = new DarlResult(values[n], values[n]);
                Assert.AreEqual(setVal, (double)input.sets[input.categories[1]].Equal(res).values[0], tolerance);
            }
            for (int n = 0; n < 100; n++)
            {
                var setVal = input.CalculateSetMembership(input.learningSource[n], 2);
                Assert.IsTrue(setVal >= 0.0 && setVal <= 1.0);
                if (n < 50)
                    Assert.AreEqual(0.0, setVal);
                DarlResult res = new DarlResult(values[n], values[n]);
                Assert.AreEqual(setVal, (double)input.sets[input.categories[2]].Equal(res).values[0], tolerance);
            }
            input = new InputDefinitionNode();
            runtime.FindSetBoundaries(5, input, values);

            for (int n = 0; n < 100; n++)
            {
                var setVal = input.CalculateSetMembership(input.learningSource[n], 0);
                Assert.IsTrue(setVal >= 0.0 && setVal <= 1.0);
                if (n >= 25)
                    Assert.AreEqual(0.0, setVal);
                DarlResult res = new DarlResult(values[n], values[n]);
                Assert.AreEqual(setVal, (double)input.sets[input.categories[0]].Equal(res).values[0], tolerance);
            }
            for (int n = 0; n < 100; n++)
            {
                var setVal = input.CalculateSetMembership(input.learningSource[n], 1);
                Assert.IsTrue(setVal >= 0.0 && setVal <= 1.0);
                if (n >= 50)
                    Assert.AreEqual(0.0, setVal);
                DarlResult res = new DarlResult(values[n], values[n]);
                Assert.AreEqual(setVal, (double)input.sets[input.categories[1]].Equal(res).values[0], tolerance);
            }
            for (int n = 0; n < 100; n++)
            {
                var setVal = input.CalculateSetMembership(input.learningSource[n], 2);
                Assert.IsTrue(setVal >= 0.0 && setVal <= 1.0);
                if (n < 25)
                    Assert.AreEqual(0.0, setVal);
                if (n == 50)
                    Assert.AreEqual(1.0, setVal);
                if (n > 75)
                    Assert.AreEqual(0.0, setVal);
                DarlResult res = new DarlResult(values[n], values[n]);
                Assert.AreEqual(setVal, (double)input.sets[input.categories[2]].Equal(res).values[0], tolerance);
            }
            for (int n = 0; n < 100; n++)
            {
                var setVal = input.CalculateSetMembership(input.learningSource[n], 3);
                Assert.IsTrue(setVal >= 0.0 && setVal <= 1.0);
                if (n < 50)
                    Assert.AreEqual(0.0, setVal);
                DarlResult res = new DarlResult(values[n], values[n]);
                Assert.AreEqual(setVal, (double)input.sets[input.categories[3]].Equal(res).values[0], tolerance);
            }
            for (int n = 0; n < 100; n++)
            {
                var setVal = input.CalculateSetMembership(input.learningSource[n], 4);
                Assert.IsTrue(setVal >= 0.0 && setVal <= 1.0);
                if (n < 75)
                    Assert.AreEqual(0.0, setVal);
                DarlResult res = new DarlResult(values[n], values[n]);
                Assert.AreEqual(setVal, (double)input.sets[input.categories[4]].Equal(res).values[0], tolerance);
            }
            input = new InputDefinitionNode();
            runtime.FindSetBoundaries(7, input, values);

            for (int n = 0; n < 100; n++)
            {
                var setVal = input.CalculateSetMembership(input.learningSource[n], 0);
                Assert.IsTrue(setVal >= 0.0 && setVal <= 1.0);
                if (n > 16)
                    Assert.AreEqual(0.0, setVal);
                DarlResult res = new DarlResult(values[n], values[n]);
                Assert.AreEqual(setVal, (double)input.sets[input.categories[0]].Equal(res).values[0], tolerance);
            }
            for (int n = 0; n < 100; n++)
            {
                var setVal = input.CalculateSetMembership(input.learningSource[n], 1);
                Assert.IsTrue(setVal >= 0.0 && setVal <= 1.0);
                if (n > 33)
                    Assert.AreEqual(0.0, setVal);
                DarlResult res = new DarlResult(values[n], values[n]);
                Assert.AreEqual(setVal, (double)input.sets[input.categories[1]].Equal(res).values[0], tolerance);
            }
            for (int n = 0; n < 100; n++)
            {
                var setVal = input.CalculateSetMembership(input.learningSource[n], 2);
                Assert.IsTrue(setVal >= 0.0 && setVal <= 1.0);
                if (n < 16)
                    Assert.AreEqual(0.0, setVal);
                if (n > 50)
                    Assert.AreEqual(0.0, setVal);
                DarlResult res = new DarlResult(values[n], values[n]);
                Assert.AreEqual(setVal, (double)input.sets[input.categories[2]].Equal(res).values[0], tolerance);
            }
            for (int n = 0; n < 100; n++)
            {
                var setVal = input.CalculateSetMembership(input.learningSource[n], 3);
                Assert.IsTrue(setVal >= 0.0 && setVal <= 1.0);
                if (n == 50)
                    Assert.AreEqual(1.0, setVal);
                if (n < 33)
                    Assert.AreEqual(0.0, setVal);
                if (n > 66)
                    Assert.AreEqual(0.0, setVal);
                DarlResult res = new DarlResult(values[n], values[n]);
                Assert.AreEqual(setVal, (double)input.sets[input.categories[3]].Equal(res).values[0], tolerance);
            }
            for (int n = 0; n < 100; n++)
            {
                var setVal = input.CalculateSetMembership(input.learningSource[n], 4);
                Assert.IsTrue(setVal >= 0.0 && setVal <= 1.0);
                if (n < 50)
                    Assert.AreEqual(0.0, setVal);
                if (n > 83)
                    Assert.AreEqual(0.0, setVal);
                DarlResult res = new DarlResult(values[n], values[n]);
                Assert.AreEqual(setVal, (double)input.sets[input.categories[4]].Equal(res).values[0], tolerance);
            }
            for (int n = 0; n < 100; n++)
            {
                var setVal = input.CalculateSetMembership(input.learningSource[n], 5);
                Assert.IsTrue(setVal >= 0.0 && setVal <= 1.0);
                if (n < 66)
                    Assert.AreEqual(0.0, setVal);
                DarlResult res = new DarlResult(values[n], values[n]);
                Assert.AreEqual(setVal, (double)input.sets[input.categories[5]].Equal(res).values[0], tolerance);
            }
            for (int n = 0; n < 100; n++)
            {
                var setVal = input.CalculateSetMembership(input.learningSource[n], 6);
                Assert.IsTrue(setVal >= 0.0 && setVal <= 1.0);
                if (n < 83)
                    Assert.AreEqual(0.0, setVal);
                DarlResult res = new DarlResult(values[n], values[n]);
                Assert.AreEqual(setVal, (double)input.sets[input.categories[6]].Equal(res).values[0], tolerance);
            }
            //9 sets
            input = new InputDefinitionNode();
            runtime.FindSetBoundaries(9, input, values);

            for (int n = 0; n < 100; n++)
            {
                var setVal = input.CalculateSetMembership(input.learningSource[n], 0);
                Assert.IsTrue(setVal >= 0.0 && setVal <= 1.0);
                if (n > 12)
                    Assert.AreEqual(0.0, setVal);
                DarlResult res = new DarlResult(values[n], values[n]);
                Assert.AreEqual(setVal, (double)input.sets[input.categories[0]].Equal(res).values[0], tolerance);
            }
            for (int n = 0; n < 100; n++)
            {
                var setVal = input.CalculateSetMembership(input.learningSource[n], 1);
                Assert.IsTrue(setVal >= 0.0 && setVal <= 1.0);
                if (n > 25)
                    Assert.AreEqual(0.0, setVal);
                DarlResult res = new DarlResult(values[n], values[n]);
                Assert.AreEqual(setVal, (double)input.sets[input.categories[1]].Equal(res).values[0], tolerance);
            }
            for (int n = 0; n < 100; n++)
            {
                var setVal = input.CalculateSetMembership(input.learningSource[n], 2);
                Assert.IsTrue(setVal >= 0.0 && setVal <= 1.0);
                if (n < 12)
                    Assert.AreEqual(0.0, setVal);
                if (n > 37)
                    Assert.AreEqual(0.0, setVal);
                DarlResult res = new DarlResult(values[n], values[n]);
                Assert.AreEqual(setVal, (double)input.sets[input.categories[2]].Equal(res).values[0], tolerance);
            }
            for (int n = 0; n < 100; n++)
            {
                var setVal = input.CalculateSetMembership(input.learningSource[n], 3);
                Assert.IsTrue(setVal >= 0.0 && setVal <= 1.0);
                if (n < 25)
                    Assert.AreEqual(0.0, setVal);
                if (n > 50)
                    Assert.AreEqual(0.0, setVal);
                DarlResult res = new DarlResult(values[n], values[n]);
                Assert.AreEqual(setVal, (double)input.sets[input.categories[3]].Equal(res).values[0], tolerance);
            }
            for (int n = 0; n < 100; n++)
            {
                var setVal = input.CalculateSetMembership(input.learningSource[n], 4);
                Assert.IsTrue(setVal >= 0.0 && setVal <= 1.0);
                if (n == 50)
                    Assert.AreEqual(1.0, setVal);
                if (n < 37)
                    Assert.AreEqual(0.0, setVal);
                if (n > 62)
                    Assert.AreEqual(0.0, setVal);
                DarlResult res = new DarlResult(values[n], values[n]);
                Assert.AreEqual(setVal, (double)input.sets[input.categories[4]].Equal(res).values[0], tolerance);
            }
            for (int n = 0; n < 100; n++)
            {
                var setVal = input.CalculateSetMembership(input.learningSource[n], 5);
                Assert.IsTrue(setVal >= 0.0 && setVal <= 1.0);
                if (n < 50)
                    Assert.AreEqual(0.0, setVal);
                if (n > 75)
                    Assert.AreEqual(0.0, setVal);
                DarlResult res = new DarlResult(values[n], values[n]);
                Assert.AreEqual(setVal, (double)input.sets[input.categories[5]].Equal(res).values[0], tolerance);
            }
            for (int n = 0; n < 100; n++)
            {
                var setVal = input.CalculateSetMembership(input.learningSource[n], 6);
                Assert.IsTrue(setVal >= 0.0 && setVal <= 1.0);
                if (n < 62)
                    Assert.AreEqual(0.0, setVal);
                if (n > 87)
                    Assert.AreEqual(0.0, setVal);
                DarlResult res = new DarlResult(values[n], values[n]);
                Assert.AreEqual(setVal, (double)input.sets[input.categories[6]].Equal(res).values[0], tolerance);
            }
            for (int n = 0; n < 100; n++)
            {
                var setVal = input.CalculateSetMembership(input.learningSource[n], 7);
                Assert.IsTrue(setVal >= 0.0 && setVal <= 1.0);
                if (n < 75)
                    Assert.AreEqual(0.0, setVal);
                DarlResult res = new DarlResult(values[n], values[n]);
                Assert.AreEqual(setVal, (double)input.sets[input.categories[7]].Equal(res).values[0], tolerance);
            }
            for (int n = 0; n < 100; n++)
            {
                var setVal = input.CalculateSetMembership(input.learningSource[n], 8);
                Assert.IsTrue(setVal >= 0.0 && setVal <= 1.0);
                if (n < 87)
                    Assert.AreEqual(0.0, setVal);
                DarlResult res = new DarlResult(values[n], values[n]);
                Assert.AreEqual(setVal, (double)input.sets[input.categories[8]].Equal(res).values[0], tolerance);
            }
        }
        [TestMethod, TestCategory("Darl Questionnaire evaluation")]
        public void TestCalculateSaliency()
        {
            DarlRunTime runtime = new DarlRunTime();
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.MultipleRuleSet.darl"));
            string source = reader.ReadToEnd();
            var tree = runtime.CreateTree(source);
            var results = new List<DarlResult>();
            Dictionary<string, double> sals = runtime.CalculateSaliences(results, tree);
            Assert.AreEqual(37.5, sals["earned_income"]);
            Assert.AreEqual(30, sals["dividend_income"]);
            Assert.AreEqual(18.75, sals["age"]);
            Assert.AreEqual(3.75, sals["married"]);
            Assert.AreEqual(15, sals["blind"]);
            //now test that removed values don't occur in the list.
            sals = runtime.CalculateSaliences(results, tree);
            Assert.AreEqual(5, sals.Count);
            results.Add(new DarlResult("age", 56));
            results.Add(new DarlResult("earned_income", 35000));
            results.Add(new DarlResult("dividend_income", 35000));
            results.Add(new DarlResult("blind", "False"));
            results.Add(new DarlResult("married", "False"));
            var fullset = runtime.Evaluate(tree, results);
            sals = runtime.CalculateSaliences(results, tree);
            Assert.AreEqual(0, sals.Count);
            //now look at a rule set where saliences must be calculated for each rule.
            reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.SiteNavigation.darl"));
            source = reader.ReadToEnd();
            tree = runtime.CreateTree(source);
            results = new List<DarlResult>();
            sals = runtime.CalculateSaliences(results, tree);
            Assert.AreEqual(10, sals.Count);
            results.Add(new DarlResult("generalType", "academic"));
            fullset = runtime.Evaluate(tree, results);
            sals = runtime.CalculateSaliences(results, tree);
            Assert.AreEqual(6, sals.Count);
            results.Add(new DarlResult("academicType", "collaboration"));
            fullset = runtime.Evaluate(tree, results);
            sals = runtime.CalculateSaliences(results, tree);
            Assert.AreEqual(5, sals.Count);
            results.Add(new DarlResult("urgency", "immediate"));
            fullset = runtime.Evaluate(tree, results);
            sals = runtime.CalculateSaliences(results, tree);
            Assert.AreEqual(4, sals.Count);
            runtime.ClearInputs(tree);
            results = new List<DarlResult>();
            sals = runtime.CalculateSaliences(results, tree);
            Assert.AreEqual(10, sals.Count);
            results.Add(new DarlResult("generalType", "technology"));
            fullset = runtime.Evaluate(tree, results);
            sals = runtime.CalculateSaliences(results, tree);
            Assert.AreEqual(1, sals.Count);
            results.Add(new DarlResult("technologyType", "fuzzy_logic"));
            fullset = runtime.Evaluate(tree, results);
            sals = runtime.CalculateSaliences(results, tree);
            Assert.AreEqual(0, sals.Count);
        }

        [TestMethod]
        public async Task TestParkrSalience()
        {
            DarlRunTime runtime = new DarlRunTime();
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.ParkrBuildr.darl"));
            string source = reader.ReadToEnd();
            var tree = runtime.CreateTree(source);
            var results = new List<DarlResult>();
            var fullset = await runtime.Evaluate(tree, results);
            var sals = runtime.CalculateSaliences(results, tree);
        }

        [TestMethod, TestCategory("Darl functionality")]
        public async Task TestAutoWiring()
        {
            DarlRunTime runtime = new DarlRunTime();
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.SiteNavigationSimple.darl"));
            var source = reader.ReadToEnd();
            var tree = runtime.CreateTree(source);
            var results = new List<DarlResult>();
            var sals = runtime.CalculateSaliences(results, tree);
            Assert.AreEqual(10, sals.Count);
            results.Add(new DarlResult("generalType", "academic"));
            var fullset = await runtime.Evaluate(tree, results);
            sals = runtime.CalculateSaliences(results, tree);
            Assert.AreEqual(6, sals.Count);
            results.Add(new DarlResult("academicType", "collaboration"));
            fullset = await runtime.Evaluate(tree, results);
            sals = runtime.CalculateSaliences(results, tree);
            Assert.AreEqual(5, sals.Count);
            results.Add(new DarlResult("urgency", "immediate"));
            fullset = await runtime.Evaluate(tree, results);
            sals = runtime.CalculateSaliences(results, tree);
            Assert.AreEqual(4, sals.Count);
            runtime.ClearInputs(tree);
            results = new List<DarlResult>();
            sals = runtime.CalculateSaliences(results, tree);
            Assert.AreEqual(10, sals.Count);
            results.Add(new DarlResult("generalType", "technology"));
            fullset = await runtime.Evaluate(tree, results);
            sals = runtime.CalculateSaliences(results, tree);
            Assert.AreEqual(1, sals.Count);
            results.Add(new DarlResult("technologyType", "fuzzy_logic"));
            fullset = await runtime.Evaluate(tree, results);
            sals = runtime.CalculateSaliences(results, tree);
            Assert.AreEqual(0, sals.Count);
        }

        [TestMethod, TestCategory("Darl functionality")]
        public async Task TestAbsentPresent()
        {
            DarlRunTime runtime = new DarlRunTime();
            var results = new List<DarlResult>();
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.AbsentPresent.darl"));
            string ruleset = reader.ReadToEnd();
            var res1 = await runtime.Evaluate(results, ruleset, "AbsentPresent");
            Assert.AreEqual("true", res1.First(a => a.name == "AbsentPresent.bill").Value);
            results.Add(new DarlResult("AbsentPresent.c", 5));
            await runtime.Evaluate(results, ruleset, "AbsentPresent");
            Assert.AreEqual("false", results.First(a => a.name == "AbsentPresent.bill").Value);
        }

        [TestMethod, TestCategory("Darl functionality")]
        public async Task TestNumberLiteralInNumericCalc()
        {
            DarlRunTime runtime = new DarlRunTime();
            var results = new List<DarlResult>();
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.NumberLiteral.darl"));
            string ruleset = reader.ReadToEnd();
            results.Add(new DarlResult("NumberLiteral.c", 8));
            await runtime.Evaluate(results, ruleset, "NumberLiteral");
            Assert.AreEqual("true", results.First(a => a.name == "NumberLiteral.bill").Value);
            results.Remove(results.First(a => a.name == "NumberLiteral.c"));
            results.Add(new DarlResult("NumberLiteral.c", 4));
            await runtime.Evaluate(results, ruleset, "NumberLiteral");
            Assert.AreEqual("false", results.First(a => a.name == "NumberLiteral.bill").Value);
        }

        [TestMethod, TestCategory("DarlPolicy functionality")]
        public void TestSequenceParsing()
        {
            LanguageData language = new LanguageData(new DarlGrammar());
            Parser parser = new Parser(language);
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.SequenceParse.darl"));
            ParseTree parseTree = parser.Parse(reader.ReadToEnd());
            ParseTreeNode root = parseTree.Root;
            Assert.IsNotNull(root);
        }

        [TestMethod, TestCategory("DarlPolicy functionality")]
        public async Task TestSequenceEvaluation()
        {
            DarlRunTime runtime = new DarlRunTime();
            var results = new List<DarlResult>();
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.SequenceParse.darl"));
            await runtime.Evaluate(results, reader.ReadToEnd());
        }

        [TestMethod, TestCategory("DarlPolicy functionality")]
        public async Task TestTextParsing()
        {
            DarlRunTime runtime = new DarlRunTime();
            var results = new List<DarlResult>
            {
                new DarlResult("arthur", "poop", DarlResult.DataType.textual)
            };
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.texttest.darl"));
            await runtime.Evaluate(results, reader.ReadToEnd());
        }

        [TestMethod, TestCategory("Darl text functionality")]
        public async Task TestTextOutputParsing()
        {
            LanguageData language = new LanguageData(new DarlGrammar());
            Parser parser = new Parser(language);
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.textoutputtest.darl"));
            var source = reader.ReadToEnd();
            var doc = @"This is some text with %% a %% with %% bill %% and %% c %%";

            DarlRunTime runtime = new DarlRunTime();
            var results = new List<DarlResult>
            {
                new DarlResult("a", 1.0),
                new DarlResult("c", 3.0),
                new DarlResult("bill", "poo"),
                new DarlResult("d", doc, DarlResult.DataType.textual)
            };
            await runtime.Evaluate(results, source);
            Assert.AreEqual(@"This is some text with 1 with poo and 3", results.First(a => a.name == "f").stringConstant);
            Assert.AreEqual(doc, results.First(a => a.name == "q").stringConstant);
            Assert.AreEqual(@"This is some text with 1 with poo and 3", results.First(a => a.name == "f").stringConstant);
            Assert.AreEqual("this is a text string", results.First(a => a.name == "r").stringConstant);

        }

        [TestMethod, TestCategory("DarlRuntime functionality")]
        public void TestGetInputOutputNames()
        {
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.textoutputtest.darl"));
            var source = reader.ReadToEnd();
            DarlRunTime runtime = new DarlRunTime();
            var tree = runtime.CreateTree(source);
            var ins = runtime.GetInputNames(tree);
            var outs = runtime.GetOutputNames(tree);
            Assert.AreEqual(4, ins.Count);
            Assert.AreEqual(4, outs.Count);
            Assert.AreEqual("d", ins[0]);
            Assert.AreEqual("a", ins[1]);
            Assert.AreEqual("c", ins[2]);
            Assert.AreEqual("bill", ins[3]);
            Assert.AreEqual("f", outs[0]);
            Assert.AreEqual("q", outs[1]);
            Assert.AreEqual("r", outs[2]);
            Assert.AreEqual("p", outs[3]);
        }

        [TestMethod, TestCategory("Darl text functionality")]
        public async Task TestDocumentHandling()
        {
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.ParkingAppealUK.darl"));
            var source = reader.ReadToEnd();
            var docsource = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.DocumentTestTemplate.txt"));
            var doc = docsource.ReadToEnd();
            //grounds : res
            var results = new List<DarlResult>
            {
                new DarlResult("docText", doc, DarlResult.DataType.textual),
                new DarlResult("name", "Alfred George Bloggs", DarlResult.DataType.textual),
                new DarlResult("email", "andy@scientio.com", DarlResult.DataType.textual),
                new DarlResult("ticket_number", "ABC12345", DarlResult.DataType.textual),
                new DarlResult("date_of_ticket", "1/01/2011", DarlResult.DataType.textual),
                new DarlResult("time_of_ticket", "6:55", DarlResult.DataType.textual),
                new DarlResult("reg_number", "RTA 151G", DarlResult.DataType.textual),
                new DarlResult("council", "Milton Keynes", DarlResult.DataType.textual),
                new DarlResult("grounds", "res"),
                new DarlResult("permit_number", "567890", DarlResult.DataType.textual),
                new DarlResult("time_of_arrival", "5:55", DarlResult.DataType.textual),
                new DarlResult("incorrect_date", "06/11/1955", DarlResult.DataType.textual),
                new DarlResult("correct_date", "06/11/1966", DarlResult.DataType.textual)
            };
            DarlRunTime runtime = new DarlRunTime();
            await runtime.Evaluate(results, source);
            var expected = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.UKParking_res.txt")).ReadToEnd();
            Assert.AreEqual(expected, results.First(a => a.name == "doc").stringConstant);
            results = new List<DarlResult>
            {
                new DarlResult("docText", doc, DarlResult.DataType.textual),
                new DarlResult("name", "Alfred George Bloggs", DarlResult.DataType.textual),
                new DarlResult("email", "andy@scientio.com", DarlResult.DataType.textual),
                new DarlResult("ticket_number", "ABC12345", DarlResult.DataType.textual),
                new DarlResult("date_of_ticket", "1/01/2011", DarlResult.DataType.textual),
                new DarlResult("time_of_ticket", "6:55", DarlResult.DataType.textual),
                new DarlResult("reg_number", "RTA 151G", DarlResult.DataType.textual),
                new DarlResult("council", "Milton Keynes", DarlResult.DataType.textual),
                new DarlResult("grounds", "parkingbay"),
                new DarlResult("street_name", "Great Poop St.", DarlResult.DataType.textual),
                new DarlResult("bay_width", 150)
            };
            await runtime.Evaluate(results, source);
            expected = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.UKParking_parkingbay.txt")).ReadToEnd();
            Assert.AreEqual(expected, results.First(a => a.name == "doc").stringConstant);
            results = new List<DarlResult>
            {
                new DarlResult("docText", doc, DarlResult.DataType.textual),
                new DarlResult("name", "Alfred George Bloggs", DarlResult.DataType.textual),
                new DarlResult("email", "andy@scientio.com", DarlResult.DataType.textual),
                new DarlResult("ticket_number", "ABC12345", DarlResult.DataType.textual),
                new DarlResult("date_of_ticket", "1/01/2011", DarlResult.DataType.textual),
                new DarlResult("time_of_ticket", "6:55", DarlResult.DataType.textual),
                new DarlResult("reg_number", "RTA 151G", DarlResult.DataType.textual),
                new DarlResult("council", "Milton Keynes", DarlResult.DataType.textual),
                new DarlResult("grounds", "stolen"),
                new DarlResult("date_of_theft", "06/11/1955", DarlResult.DataType.textual),
                new DarlResult("police_station", "Milton Keynes", DarlResult.DataType.textual),
                new DarlResult("crime_reference", "oldbill1234567", DarlResult.DataType.textual),
                new DarlResult("date_of_report", "05/11/1955", DarlResult.DataType.textual)
            };
            await runtime.Evaluate(results, source);
            expected = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.UKParking_stolen.txt")).ReadToEnd();
            Assert.AreEqual(expected, results.First(a => a.name == "doc").stringConstant);
            results = new List<DarlResult>
            {
                new DarlResult("docText", doc, DarlResult.DataType.textual),
                new DarlResult("name", "Alfred George Bloggs", DarlResult.DataType.textual),
                new DarlResult("email", "andy@scientio.com", DarlResult.DataType.textual),
                new DarlResult("ticket_number", "ABC12345", DarlResult.DataType.textual),
                new DarlResult("date_of_ticket", "1/01/2011", DarlResult.DataType.textual),
                new DarlResult("time_of_ticket", "6:55", DarlResult.DataType.textual),
                new DarlResult("reg_number", "RTA 151G", DarlResult.DataType.textual),
                new DarlResult("council", "Milton Keynes", DarlResult.DataType.textual),
                new DarlResult("grounds", "hospital"),
                new DarlResult("hospital_name", "Milton Keynes General", DarlResult.DataType.textual),
                new DarlResult("hospital_address", "27 chafron way", DarlResult.DataType.textual),
                new DarlResult("hospital_postcode", "MK1 1gb", DarlResult.DataType.textual),
                new DarlResult("medical_emergency", "terminal stupidity", DarlResult.DataType.textual),
                new DarlResult("patient", "alfred gonad", DarlResult.DataType.textual)
            };
            await runtime.Evaluate(results, source);
            expected = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.UKParking_hospital.txt")).ReadToEnd();
            Assert.AreEqual(expected, results.First(a => a.name == "doc").stringConstant);
            results = new List<DarlResult>
            {
                new DarlResult("docText", doc, DarlResult.DataType.textual),
                new DarlResult("name", "Alfred George Bloggs", DarlResult.DataType.textual),
                new DarlResult("email", "andy@scientio.com", DarlResult.DataType.textual),
                new DarlResult("ticket_number", "ABC12345", DarlResult.DataType.textual),
                new DarlResult("date_of_ticket", "1/01/2011", DarlResult.DataType.textual),
                new DarlResult("time_of_ticket", "6:55", DarlResult.DataType.textual),
                new DarlResult("reg_number", "RTA 151G", DarlResult.DataType.textual),
                new DarlResult("council", "Milton Keynes", DarlResult.DataType.textual),
                new DarlResult("grounds", "ownershipbought"),
                new DarlResult("previous_owner", "Horatio T Poopalot", DarlResult.DataType.textual),
                new DarlResult("previous_address", "27 chafron way", DarlResult.DataType.textual),
                new DarlResult("previous_postcode", "MK1 1gb", DarlResult.DataType.textual),
                new DarlResult("date_of_purchase", "06/01/1967", DarlResult.DataType.textual)
            };
            await runtime.Evaluate(results, source);
            expected = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.UKParking_ownerbought.txt")).ReadToEnd();
            Assert.AreEqual(expected, results.First(a => a.name == "doc").stringConstant);
            results = new List<DarlResult>
            {
                new DarlResult("docText", doc, DarlResult.DataType.textual),
                new DarlResult("name", "Alfred George Bloggs", DarlResult.DataType.textual),
                new DarlResult("email", "andy@scientio.com", DarlResult.DataType.textual),
                new DarlResult("ticket_number", "ABC12345", DarlResult.DataType.textual),
                new DarlResult("date_of_ticket", "1/01/2011", DarlResult.DataType.textual),
                new DarlResult("time_of_ticket", "6:55", DarlResult.DataType.textual),
                new DarlResult("reg_number", "RTA 151G", DarlResult.DataType.textual),
                new DarlResult("council", "Milton Keynes", DarlResult.DataType.textual),
                new DarlResult("grounds", "ownershipsold"),
                new DarlResult("subsequent_owner", "Horatio T Poopalot", DarlResult.DataType.textual),
                new DarlResult("subsequent_address", "27 chafron way", DarlResult.DataType.textual),
                new DarlResult("subsequent_postcode", "MK1 1gb", DarlResult.DataType.textual),
                new DarlResult("date_of_sale", "06/01/1967", DarlResult.DataType.textual)
            };
            await runtime.Evaluate(results, source);
            expected = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.UKParking_ownersold.txt")).ReadToEnd();
            Assert.AreEqual(expected, results.First(a => a.name == "doc").stringConstant);
            results = new List<DarlResult>
            {
                new DarlResult("docText", doc, DarlResult.DataType.textual),
                new DarlResult("name", "Alfred George Bloggs", DarlResult.DataType.textual),
                new DarlResult("email", "andy@scientio.com", DarlResult.DataType.textual),
                new DarlResult("ticket_number", "ABC12345", DarlResult.DataType.textual),
                new DarlResult("date_of_ticket", "1/01/2011", DarlResult.DataType.textual),
                new DarlResult("time_of_ticket", "6:55", DarlResult.DataType.textual),
                new DarlResult("reg_number", "RTA 151G", DarlResult.DataType.textual),
                new DarlResult("council", "Milton Keynes", DarlResult.DataType.textual),
                new DarlResult("grounds", "diplomat"),
                new DarlResult("country", "Buggeria", DarlResult.DataType.textual),
                new DarlResult("embassy_address", "27 chafron way", DarlResult.DataType.textual),
                new DarlResult("embassy_postcode", "MK1 1gb", DarlResult.DataType.textual),
                new DarlResult("ticket_type", "congestion")
            };
            await runtime.Evaluate(results, source);
            expected = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.UKParking_diplomat.txt")).ReadToEnd();
            Assert.AreEqual(expected, results.First(a => a.name == "doc").stringConstant);
            results = new List<DarlResult>
            {
                new DarlResult("docText", doc, DarlResult.DataType.textual),
                new DarlResult("name", "Alfred George Bloggs", DarlResult.DataType.textual),
                new DarlResult("email", "andy@scientio.com", DarlResult.DataType.textual),
                new DarlResult("ticket_number", "ABC12345", DarlResult.DataType.textual),
                new DarlResult("date_of_ticket", "1/01/2011", DarlResult.DataType.textual),
                new DarlResult("time_of_ticket", "6:55", DarlResult.DataType.textual),
                new DarlResult("reg_number", "RTA 151G", DarlResult.DataType.textual),
                new DarlResult("council", "Milton Keynes", DarlResult.DataType.textual),
                new DarlResult("grounds", "reg"),
                new DarlResult("incorrect_reg", "CMX 981A", DarlResult.DataType.textual),
                new DarlResult("time_of_arrival", "5:55", DarlResult.DataType.textual)
            };
            await runtime.Evaluate(results, source);
            expected = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.UKParking_reg.txt")).ReadToEnd();
            Assert.AreEqual(expected, results.First(a => a.name == "doc").stringConstant);
            results = new List<DarlResult>
            {
                new DarlResult("docText", doc, DarlResult.DataType.textual),
                new DarlResult("name", "Alfred George Bloggs", DarlResult.DataType.textual),
                new DarlResult("email", "andy@scientio.com", DarlResult.DataType.textual),
                new DarlResult("ticket_number", "ABC12345", DarlResult.DataType.textual),
                new DarlResult("date_of_ticket", "1/01/2011", DarlResult.DataType.textual),
                new DarlResult("time_of_ticket", "6:55", DarlResult.DataType.textual),
                new DarlResult("reg_number", "RTA 151G", DarlResult.DataType.textual),
                new DarlResult("council", "Milton Keynes", DarlResult.DataType.textual),
                new DarlResult("grounds", "missing"),
                new DarlResult("missing_detail", "reason"),
                new DarlResult("summary_of_offence", "Lack of political correctness", DarlResult.DataType.textual)
            };
            await runtime.Evaluate(results, source);
            expected = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.UKParking_missing.txt")).ReadToEnd();
            Assert.AreEqual(expected, results.First(a => a.name == "doc").stringConstant);
            results = new List<DarlResult>
            {
                new DarlResult("docText", doc, DarlResult.DataType.textual),
                new DarlResult("name", "Alfred George Bloggs", DarlResult.DataType.textual),
                new DarlResult("email", "andy@scientio.com", DarlResult.DataType.textual),
                new DarlResult("ticket_number", "ABC12345", DarlResult.DataType.textual),
                new DarlResult("date_of_ticket", "1/01/2011", DarlResult.DataType.textual),
                new DarlResult("time_of_ticket", "6:55", DarlResult.DataType.textual),
                new DarlResult("reg_number", "RTA 151G", DarlResult.DataType.textual),
                new DarlResult("council", "Milton Keynes", DarlResult.DataType.textual),
                new DarlResult("grounds", "incorrect"),
                new DarlResult("incorrect_detail", "reason"),
                new DarlResult("summary_of_offence", "Lack of political correctness", DarlResult.DataType.textual),
                new DarlResult("incorrect_text", "Pay up scum or we'll accuse you of kiddy fiddling", DarlResult.DataType.textual),
                new DarlResult("correct_text", "you unfortunately overstayed", DarlResult.DataType.textual)
            };
            await runtime.Evaluate(results, source);
            expected = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.UKParking_incorrect.txt")).ReadToEnd();
            Assert.AreEqual(expected, results.First(a => a.name == "doc").stringConstant);
            results = new List<DarlResult>
            {
                new DarlResult("docText", doc, DarlResult.DataType.textual),
                new DarlResult("name", "Alfred George Bloggs", DarlResult.DataType.textual),
                new DarlResult("email", "andy@scientio.com", DarlResult.DataType.textual),
                new DarlResult("ticket_number", "ABC12345", DarlResult.DataType.textual),
                new DarlResult("date_of_ticket", "1/01/2011", DarlResult.DataType.textual),
                new DarlResult("time_of_ticket", "6:55", DarlResult.DataType.textual),
                new DarlResult("reg_number", "RTA 151G", DarlResult.DataType.textual),
                new DarlResult("council", "Milton Keynes", DarlResult.DataType.textual),
                new DarlResult("grounds", "hire"),
                new DarlResult("renter_name", "Horatio T Poopalot", DarlResult.DataType.textual),
                new DarlResult("renter_address", "27 chafron way", DarlResult.DataType.textual),
                new DarlResult("renter_postcode", "MK1 1gb", DarlResult.DataType.textual),
                new DarlResult("date_of_hire", "06/01/1967", DarlResult.DataType.textual)
            };
            await runtime.Evaluate(results, source);
            expected = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.UKParking_hire.txt")).ReadToEnd();
            Assert.AreEqual(expected, results.First(a => a.name == "doc").stringConstant);
            results = new List<DarlResult>
            {
                new DarlResult("docText", doc, DarlResult.DataType.textual),
                new DarlResult("name", "Alfred George Bloggs", DarlResult.DataType.textual),
                new DarlResult("email", "andy@scientio.com", DarlResult.DataType.textual),
                new DarlResult("ticket_number", "ABC12345", DarlResult.DataType.textual),
                new DarlResult("date_of_ticket", "1/01/2011", DarlResult.DataType.textual),
                new DarlResult("time_of_ticket", "6:55", DarlResult.DataType.textual),
                new DarlResult("reg_number", "RTA 151G", DarlResult.DataType.textual),
                new DarlResult("council", "Milton Keynes", DarlResult.DataType.textual),
                new DarlResult("grounds", "sign"),
                new DarlResult("summary_of_offence", "Lack of political correctness", DarlResult.DataType.textual),
                new DarlResult("signage_fault", "contradictory"),
                new DarlResult("sign_1_meaning", "Don't park here oik", DarlResult.DataType.textual),
                new DarlResult("sign_2_meaning", "Please park here, my good sir", DarlResult.DataType.textual),
                new DarlResult("sign_relation", "above"),
                new DarlResult("street_name", "Great Poop St.", DarlResult.DataType.textual)
            };
            await runtime.Evaluate(results, source);
            expected = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.UKParking_sign_incorrect.txt")).ReadToEnd();
            Assert.AreEqual(expected, results.First(a => a.name == "doc").stringConstant);
            results = new List<DarlResult>
            {
                new DarlResult("docText", doc, DarlResult.DataType.textual),
                new DarlResult("name", "Alfred George Bloggs", DarlResult.DataType.textual),
                new DarlResult("email", "andy@scientio.com", DarlResult.DataType.textual),
                new DarlResult("ticket_number", "ABC12345", DarlResult.DataType.textual),
                new DarlResult("date_of_ticket", "1/01/2011", DarlResult.DataType.textual),
                new DarlResult("time_of_ticket", "6:55", DarlResult.DataType.textual),
                new DarlResult("reg_number", "RTA 151G", DarlResult.DataType.textual),
                new DarlResult("council", "Milton Keynes", DarlResult.DataType.textual),
                new DarlResult("grounds", "sign"),
                new DarlResult("summary_of_offence", "Lack of political correctness", DarlResult.DataType.textual),
                new DarlResult("signage_fault", "absent_unclear"),
                new DarlResult("street_name", "Great Poop St.", DarlResult.DataType.textual)
            };
            await runtime.Evaluate(results, source);
            expected = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.UKParking_sign_absent.txt")).ReadToEnd();
            Assert.AreEqual(expected, results.First(a => a.name == "doc").stringConstant);
            var dvresults = JsonConvert.SerializeObject(Convert(results));
            Assert.AreEqual(55489, dvresults.Length);

        }

        [TestMethod, TestCategory("Darl text functionality")]
        public void TestDocumentHandling2()
        {
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.Cross_border.darl"));
            var source = reader.ReadToEnd();
            var docsource = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.Cross_border.txt"));
            var doc = docsource.ReadToEnd();
            //grounds : res
            var results = new List<DarlResult>
            {
                new DarlResult("doc", doc, DarlResult.DataType.textual),
                new DarlResult("no_doc", "The transfer is not permitted because the conditions have not been met.", DarlResult.DataType.textual),
                new DarlResult("source_country", "United Kingdom"),
                new DarlResult("dest_country", "United Kingdom"),
                new DarlResult("data_type", "normal"),
                new DarlResult("business_purpose", "AML")
            };
            DarlRunTime runtime = new DarlRunTime();
            var res = runtime.Evaluate(results, source);
        }

        [TestMethod, TestCategory("Darl text functionality")]
        public void TestDocumentHandling3()
        {
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.Cross_border2.darl"));
            var source = reader.ReadToEnd();
            var docsource = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.Cross_border.txt"));
            var doc = docsource.ReadToEnd();
            //grounds : res
            var results = new List<DarlResult>
            {
                new DarlResult("doc", doc, DarlResult.DataType.textual),
                new DarlResult("no_doc", "The transfer is not permitted because the conditions have not been met.", DarlResult.DataType.textual),
                new DarlResult("source_country", "United Kingdom"),
                new DarlResult("dest_country", "United Kingdom"),
                new DarlResult("data_type", "sensitive"),
                new DarlResult("business_purpose", "AML")
            };
            DarlRunTime runtime = new DarlRunTime();
            var tree = runtime.CreateTree(source);
            var res = runtime.Evaluate(tree, results);
            var s = runtime.CalculateSaliences(results, tree);
            foreach (var sal in s.Keys)
            {
                Trace.WriteLine($"input {sal} value {s[sal]}");
            }
        }

        [TestMethod, TestCategory("Darl text functionality")]
        public void TestDocumentHandling4()
        {
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.Cross_border2.darl"));
            var source = reader.ReadToEnd();
            var docsource = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.cross_border2.txt"));
            var doc = docsource.ReadToEnd();
            //grounds : res
            var results = new List<DarlResult>
            {
                new DarlResult("doc", doc, DarlResult.DataType.textual),
                new DarlResult("no_doc", "The transfer is not permitted because the conditions have not been met.", DarlResult.DataType.textual),
                new DarlResult("source_country", "United Kingdom"),
                new DarlResult("dest_country", "United Kingdom"),
                new DarlResult("data_type", "normal"),
                new DarlResult("business_purpose", "AML")
            };
            DarlRunTime runtime = new DarlRunTime();
            var tree = runtime.CreateTree(source);
            var res = runtime.Evaluate(tree, results);
        }

        [TestMethod, TestCategory("Darl text functionality")]
        public void TestDocumentHandling5()
        {
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.Cross_border3.darl"));
            var source = reader.ReadToEnd();
            var docsource = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.cross_border2.txt"));
            var doc = docsource.ReadToEnd();
            //grounds : res
            var results = new List<DarlResult>
            {
                new DarlResult("doc", doc, DarlResult.DataType.textual),
                new DarlResult("no_doc", "The transfer is not permitted because the conditions have not been met.", DarlResult.DataType.textual),
                new DarlResult("source_country", "United Kingdom"),
                new DarlResult("dest_country", "United Kingdom"),
                new DarlResult("data_type", "personal"),
                new DarlResult("business_purpose", "AML"),
                new DarlResult("personal_condition1", "yes"),
                new DarlResult("personal_commentary1", "Coz I say so.", DarlResult.DataType.textual),
                new DarlResult("personal_condition2", "no")
            };
            DarlRunTime runtime = new DarlRunTime();
            var tree = runtime.CreateTree(source);
            var res = runtime.Evaluate(tree, results);
        }

        [TestMethod, TestCategory("Darl text functionality")]
        public void TestDocumentHandling6() //in this case check for no exception if document list contains duplicates
        {
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.Cross_border4.darl"));
            var source = reader.ReadToEnd();
            var docsource = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.cross_border2.txt"));
            var doc = docsource.ReadToEnd();
            //grounds : res
            var results = new List<DarlResult>
            {
                new DarlResult("doc", doc, DarlResult.DataType.textual),
                new DarlResult("no_doc", "The transfer is not permitted because the conditions have not been met.", DarlResult.DataType.textual),
                new DarlResult("source_country", "United Kingdom"),
                new DarlResult("dest_country", "United Kingdom"),
                new DarlResult("data_type", "personal"),
                new DarlResult("business_purpose", "AML"),
                new DarlResult("personal_condition1", "yes"),
                new DarlResult("personal_commentary1", "Coz I say so.", DarlResult.DataType.textual),
                new DarlResult("personal_condition2", "no")
            };
            DarlRunTime runtime = new DarlRunTime();
            var tree = runtime.CreateTree(source);
            var res = runtime.Evaluate(tree, results);
        }

        [TestMethod, TestCategory("Darl text functionality")]
        public async Task TestBlueWaveSaliency()
        {
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.BluewavePricing.darl"));
            var source = reader.ReadToEnd();
            var results = new List<DarlResult>
            {
                new DarlResult("bizmodel", "_product")
            };
            var runtime = new DarlRunTime();
            var tree = runtime.CreateTree(source);
            results = await runtime.Evaluate(tree, results);
            var ufsaliences = runtime.CalculateSaliences(results, tree);

        }

        [TestMethod, TestCategory("Darl text functionality")]
        public async Task TestEUClaim()
        {
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.EUClaim.darl"));
            var source = reader.ReadToEnd();

            var docsource = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.EUClaimText.txt"));
            var doc = docsource.ReadToEnd();


            List<DarlResult> results = new List<DarlResult>
            {
                new DarlResult("endsInEU", "both"),
                new DarlResult("docText", doc, DarlResult.DataType.textual),
                new DarlResult("cannotClaimText", "Unfortunately you cannot claim.", DarlResult.DataType.textual),
                new DarlResult("withinSixYear", "true"),
                new DarlResult("delayInHours", 5),
                new DarlResult("reasonForDelay", "all_others"),
                new DarlResult("flightLength", "1000"),
                new DarlResult("bookingRef", "45678"),
                new DarlResult("dateOfFlight", "06/11/13"),
                new DarlResult("timeOfFlight", "7.55"),
                new DarlResult("fullName", "Andy Edmonds"),
                new DarlResult("flightNumber", "FU1234")
            };


            var runtime = new DarlRunTime();
            var tree = runtime.CreateTree(source);
            results = await runtime.Evaluate(tree, results);
            var ufsaliences = runtime.CalculateSaliences(results, tree);
            Assert.AreEqual(0, ufsaliences.Count);
            var dvresults = JsonConvert.SerializeObject(Convert(results));
        }

        [TestMethod, TestCategory("Darl otherwise functionality")]
        public async Task TestMilitary_service()
        {
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.military_service.darl"));
            var source = reader.ReadToEnd();
            List<DarlResult> results = new List<DarlResult>
            {
                new DarlResult("comply_text", "comply", DarlResult.DataType.textual),
                new DarlResult("non_comply_text", "Non comply", DarlResult.DataType.textual),
                new DarlResult("military", "true"),
            };

            var runtime = new DarlRunTime();
            var tree = runtime.CreateTree(source);
            results = await runtime.Evaluate(tree, results);
            Assert.AreEqual(14, results.Count);
            results.Add(new DarlResult("honorable", "true"));
            results.Add(new DarlResult("name", "bum"));
            results.Add(new DarlResult("email", "bum"));
            results = await runtime.Evaluate(tree, results);
            Assert.AreEqual("comply", results[19].Value);
        }


        [TestMethod]
        public async Task TestRandomText()
        {
            var source = "ruleset rand { output textual fred; if anything then fred will be randomtext(\"bum\",\"poop\",\"smell\",\"fart\");  }";
            var runtime = new DarlRunTime();
            var results = new List<DarlResult>();
            await runtime.Evaluate(results, source);
            var r = results.First(a => a.name == "fred").Value;
            Assert.IsTrue((string)r == "poop" || (string)r == "bum" || (string)r == "smell" || (string)r == "fart");
        }

        [TestMethod]
        public async Task TestTextOutputStringLiteral()
        {
            var source = "ruleset rand { output textual fred; if anything then fred will be \"poop\";  }";
            var runtime = new DarlRunTime();
            var results = new List<DarlResult>();
            await runtime.Evaluate(results, source);
            Assert.IsTrue((string)results.First(a => a.name == "fred").Value == "poop");
        }

        [TestMethod]
        public async Task TestBrackets()
        {
            var source = "ruleset rand { input categorical a {true,false};\ninput categorical b {true,false};\ninput categorical c {true,false};\noutput textual fred; if a is true or (b is true and c is false) then fred will be \"poop\";  }";
            var runtime = new DarlRunTime();
            var results = new List<DarlResult>
            {
                new DarlResult("a", "false"),
                new DarlResult("b", "true"),
                new DarlResult("c", "true")
            };
            await runtime.Evaluate(results, source);
            Assert.IsTrue(results.First(a => a.name == "fred").IsUnknown());
        }


        [TestMethod]
        public async Task TestDefault()
        {
            var source = "ruleset simple_questionnaire\n{\ninput categorical a { true,false };\ninput categorical b { true, false };\noutput categorical c { true, false};\nif a is true and b is true then c will be true;\notherwise if a is present and b is present then c will be false;\n}\n";
            var runtime = new DarlRunTime();
            var tree = runtime.CreateTree(source);
            Assert.IsFalse(tree.HasErrors());
            List<DarlResult> results = new List<DarlResult>
            {
                new DarlResult("a", "true"),
                new DarlResult("b", "true")
            };
            await runtime.Evaluate(tree, results);
            Assert.IsTrue((string)results.First(a => a.name == "c").Value == "true");
            results = new List<DarlResult>
            {
                new DarlResult("a", "true"),
                new DarlResult("b", "false")
            };
            await runtime.Evaluate(tree, results);
            Assert.IsTrue((string)results.First(a => a.name == "c").Value == "false");
            //            runtime = new DarlRunTime();
            //            tree = runtime.CreateTree(source);
            results = new List<DarlResult>
            {
                new DarlResult("a", "true")
            };
            await runtime.Evaluate(tree, results);
            Assert.IsTrue(results.First(a => a.name == "c").IsUnknown());
        }

        [TestMethod]
        public async Task TestTextualAggregation()
        {
            var source = "ruleset aggtest\n{\noutput textual a;\nif anything then a will be \"fred1\";\nif anything then a will be \"fred2\";\n}";
            var runtime = new DarlRunTime();
            var tree = runtime.CreateTree(source);
            Assert.IsFalse(tree.HasErrors());
            var results = new List<DarlResult>();
            await runtime.Evaluate(tree, results);
            Assert.AreEqual("fred1\nfred2", results.First(a => a.name == "a").Value);
            source = "ruleset aggtest\n{\noutput textual a;\nif anything then a will be \"fred1 \";\nif anything then a will be \"fred2\";\n}";
            runtime = new DarlRunTime();
            tree = runtime.CreateTree(source);
            Assert.IsFalse(tree.HasErrors());
            results = new List<DarlResult>();
            await runtime.Evaluate(tree, results);
            Assert.AreEqual("fred1 fred2", results.First(a => a.name == "a").Value);
        }

        [TestMethod, TestCategory("Dabl functionality")]
        public async Task TestStoreParsing()
        {
            var source = "ruleset storetest\n{\nstore a;\nif anything then a[\"result\"] will be \"fred1\";\n}";
            var runtime = new DarlRunTime();
            var tree = runtime.CreateTree(source);
            Assert.IsFalse(tree.HasErrors());
            var results = new List<DarlResult>();
            await runtime.Evaluate(tree, results);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("fred1", results.First(a => a.name == "storetest.a.result").Value);
        }


        [TestMethod, TestCategory("Dabl functionality")]
        public async Task TestFurtherStoreParsing()
        {
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.StoreTest.darl"));
            var source = reader.ReadToEnd();
            var runtime = new DarlRunTime();
            var tree = runtime.CreateTree(source);
            Assert.IsFalse(tree.HasErrors());
            var results = new List<DarlResult>();
            await runtime.Evaluate(tree, results);
            Assert.AreEqual(3, results.Count);
            var m_a = new StoreTest();
            m_a.results.Add(new DarlResult("fred", "this is string a1", DarlResult.DataType.textual));
            m_a.results.Add(new DarlResult("bill", "this is string a1", DarlResult.DataType.textual));
            m_a.results.Add(new DarlResult("john", 2, DarlResult.DataType.numeric));
            m_a.results.Add(new DarlResult("james", 24, DarlResult.DataType.numeric));
            var m_b = new StoreTest();
            results.Add(new DarlResult("c", 5));
            runtime.SetStoreInterface(tree, "a", m_a);
            runtime.SetStoreInterface(tree, "b", m_b);
            await runtime.Evaluate(tree, results);
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(3.0, m_a.results.First(a => a.name == "poop").Value);
            Assert.AreEqual(27.0, m_b.results.First(a => a.name == "age").Value);
            Assert.AreEqual("arbuthnot", m_b.results.First(a => a.name == "name").Value);
        }

        /// <summary>
        /// if a rule set contains only a store, no salience calcs are performed.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task TestStoreSalience()
        {
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.bot_choice.darl"));
            var source = reader.ReadToEnd();
            var runtime = new DarlRunTime();
            var tree = runtime.CreateTree(source);
            Assert.IsFalse(tree.HasErrors());
            var ufsaliences = runtime.CalculateSaliences(new List<DarlResult>(), tree);
            Assert.AreEqual(1, ufsaliences.Count);
        }

        [TestMethod]
        public async Task TestStoreDependency()
        {
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.storedependency.darl"));
            var source = reader.ReadToEnd();
            var runtime = new DarlRunTime();
            var tree = runtime.CreateTree(source);
            Assert.IsFalse(tree.HasErrors());
            var results = new List<DarlResult>();
            var store = new StoreTest();
            store.results.Add(new DarlResult("fred", "bill", DarlResult.DataType.textual));
            runtime.SetStoreInterface(tree, "word", store);
            await runtime.Evaluate(tree, results);
            Assert.AreEqual("bill", results.First(a => a.name == "response").stringConstant);

        }

        [TestMethod]
        public async Task TestDocumentParsing()
        {
            var source = "ruleset fred\n{\ninput textual poo;\ninput textual whiff;\noutput textual fart;\nif anything then fart will be document(\"%% poo %% ish %% whiff %%\",{ poo,whiff});\n}";
            var runtime = new DarlRunTime();
            var tree = runtime.CreateTree(source);

        }

        [TestMethod]
        public async Task TestMultiple()
        {
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.data_collection.darl"));
            var source = reader.ReadToEnd();
            var runtime = new DarlRunTime();
            var tree = runtime.CreateTree(source);
            var saliences = runtime.CalculateSaliences(new List<DarlResult>(), tree);

        }

        [TestMethod]
        public void InsertCommentTest()
        {
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.ruleseteditor.darl"));
            var source = reader.ReadToEnd();
            var runtime = new DarlRunTime();
            var tree = runtime.CreateTree(source);
        }

        [TestMethod]
        public async Task AlphMindTest()
        {
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.Alphamind.darl"));
            var source = reader.ReadToEnd();
            var runtime = new DarlRunTime();
            var tree = runtime.CreateTree(source);
            var inputs = new List<DarlResult>();
            inputs.Add(new DarlResult("rows", 0.0));
            var res = await runtime.Evaluate(tree, inputs);
        }

        [TestMethod]
        public async Task DynamicCategoricalInputTest()
        {
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.dynamiccategoricalinput.darl"));
            var source = reader.ReadToEnd();
            var runtime = new DarlRunTime();
            var tree = runtime.CreateTree(source);
        }

        [TestMethod]
        public async Task CopyDynamicToStore()
        {
            var source = "ruleset grateful_dead_artists\n{\n store UserData;\n store graph;\n input dynamic categorical artist graph[\"categories\", \"\"];\n if artist is present then UserData[\"artist\"] will be artist;\n}";
            var runtime = new DarlRunTime();
            var tree = runtime.CreateTree(source);
        }

        [TestMethod]
        public async Task StoreInStoreInDef()
        {
            var source = "ruleset grateful_dead_artists\n{\n store UserData;\n store graph;\n input dynamic categorical song graph[\"categories\", UserData[\"artist\"], \"noun:00,2,00,015,01\", \"name\"];\n if song is present then UserData[\"song\"] will be song;} ";
            var runtime = new DarlRunTime();
            var tree = runtime.CreateTree(source);
        }

        [TestMethod]
        public async Task DynCatInStoreInDef()
        {
            var source = "ruleset grateful_dead_artists\n{\n store UserData;\n store Graph;\n store Call;\n output textual response;\n output numeric performances;\n output textual songType;\n output textual songName;\n input dynamic categorical song Graph[\"categories\", UserData[\"artist\"], \"noun:01,4,14,1,10,33\", \"name\"];\n input textual docText;\n if song is present then UserData[\"song\"] will be song;\n if song is present then performances will be Graph[\"attribute\", song, \"noun:01,5,04,3,07\"];\n if song is present then songType will be Graph[\"attribute\", song, \"noun:01,0,0,15,07,02,02,0,01\"];\n  if song is present then songName will be Graph[\"attribute\", song, \"name\"];\n if song is present then response will be document(\"You selected song '%% song %%' of type %% songType %%, performed %% performances %% times.\",\n { performances,songName,songType});\n }\n";
            var runtime = new DarlRunTime();
            var tree = runtime.CreateTree(source);
        }

        public static List<DarlVar> Convert(List<DarlResult> values)
        {
            var res = new List<DarlVar>();
            if (values != null)
            {
                foreach (var r in values)
                {
                    List<double> vals = new List<double>();
                    if (r.values != null && r.dataType == DarlResult.DataType.numeric)
                    {
                        foreach (var v in r.values)
                            vals.Add((double)v);
                    }
                    res.Add(new DarlVar { name = r.name, approximate = r.approximate, categories = r.categories, dataType = (DarlVar.DataType)Enum.Parse(typeof(DarlVar.DataType), r.dataType.ToString()), sequence = r.sequence, unknown = r.IsUnknown(), Value = r.Value == null ? "" : r.Value.ToString(), values = vals, weight = r.GetWeight() });
                }
            }
            return res;
        }


    }

    public class StoreTest : ILocalStore
    {
        public List<DarlResult> results = new List<DarlResult>();

        public StoreTest()
        {
        }

        public async Task<DarlResult> ReadAsync(List<string> address)
        {
            var res = results.FirstOrDefault(a => a.name == address[0]);
            if (res.Exists())
            {
                return res;
            }
            return new DarlResult(address[0], 0.0, true);
        }

        public async Task WriteAsync(List<string> address, DarlResult value)
        {
            results.RemoveAll(a => a.name == address[0]);
            results.Add(value);
        }
    }
}
