/// <summary>
/// </summary>

﻿using DarlLanguage;
using DarlLanguage.Processing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Darl_standard_core.test
{
    [TestClass]
    public class DarlParseTreeExtensionsTest
    {
        [TestMethod]
        public void TestParseTreeExtensionsIO()
        {
            DarlRunTime runtime = new DarlRunTime();
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.MultipleRuleSet.darl"));
            string source = reader.ReadToEnd();
            var results = new Dictionary<string, DarlResult>();
            var tree = runtime.CreateTree(source);
            Assert.AreEqual(5, tree.GetMapInputs().Count);
            Assert.AreEqual(5, tree.GetMapInputs().Count);
            Assert.AreEqual(1, tree.GetMapOutputs().Count);
            Assert.AreEqual("numeric_output", tree.GetMapOutputType("earned_tax"));
            Assert.AreEqual("numeric_input", tree.GetMapInputType("age"));
            Assert.AreEqual("categorical_input", tree.GetMapInputType("married"));
            var res = tree.GetMapInputRange("age");
            Assert.AreEqual(0, (double)res.values[0]);
            Assert.AreEqual(100, (double)res.values[1]);
            Assert.AreEqual(2, tree.GetMapInputCategories("married").Count);
            reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.textoutputtest.darl"));
            source = reader.ReadToEnd();
            tree = runtime.CreateTree(source);
            Assert.AreEqual("textual_output", tree.GetMapOutputType("r"));
        }


        [TestMethod]
        public void TestToHtml()
        {
            DarlRunTime runtime = new DarlRunTime();
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.MultipleRuleSet.darl"));
            string source = reader.ReadToEnd();
            var results = new Dictionary<string, DarlResult>();
            var tree = runtime.CreateTreeEdit(source);
            var html = tree.ToHtml();

        }

        [TestMethod]
        public void RHSTests()
        {
            DarlRunTime runtime = new DarlRunTime();
            var tree = runtime.CreateTree("ruleset rhs { output textual response; if anything then response will be \"hello poopy\"; }");
            var res = tree.GetSingleRuleSetTextualRHS("response");
            Assert.AreEqual(1, res.Count);
            Assert.AreEqual("hello poopy", res[0]);
            tree = runtime.CreateTree("ruleset rhs { store Call; if anything then Call[\"\"] will be \"hello poopy\"; }");
            res = tree.GetSingleRuleSetTextualRHS("Call.");
            Assert.AreEqual(1, res.Count);
            Assert.AreEqual("hello poopy", res[0]);
            tree = runtime.CreateTree("ruleset rhs { store Call; if anything then Call[\"\"] will be randomtext(\"hello poopy\",\"hello farty\"); }");
            res = tree.GetSingleRuleSetTextualRHS("Call.");
            Assert.AreEqual(2, res.Count);
            Assert.AreEqual("hello poopy", res[0]);
            Assert.AreEqual("hello farty", res[1]);
        }

        [TestMethod]
        public void TestToDarl()
        {
            DarlRunTime runtime = new DarlRunTime();
            var tree = runtime.CreateTree("ruleset rhs { output textual response; if anything then response will be \"hello poopy\"; }");
            var source = tree.ToDarl();
            tree = runtime.CreateTree(source);
            Assert.IsFalse(tree.HasErrors());
            tree = runtime.CreateTree("ruleset rhs { store Call; if anything then Call[\"\"] will be randomtext(\"hello poopy\",\"hello farty\"); }");
            source = tree.ToDarl();
            tree = runtime.CreateTree(source);
            Assert.IsFalse(tree.HasErrors());
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.textoutputtest.darl"));
            var code = reader.ReadToEnd();
            tree = runtime.CreateTree(code);
            source = tree.ToDarl();
            tree = runtime.CreateTree(source);
            Assert.IsFalse(tree.HasErrors());
            reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.MultipleRuleSet.darl"));
            code = reader.ReadToEnd();
            tree = runtime.CreateTree(code);
            source = tree.ToDarl();
            tree = runtime.CreateTree(source);
            Assert.IsFalse(tree.HasErrors());
            reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.StoreTest.darl"));
            code = reader.ReadToEnd();
            tree = runtime.CreateTree(code);
            source = tree.ToDarl();
            tree = runtime.CreateTree(source);
            Assert.IsFalse(tree.HasErrors());
            reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.Cross_border.darl"));
            code = reader.ReadToEnd();
            tree = runtime.CreateTree(code);
            source = tree.ToDarl();
            tree = runtime.CreateTree(source);
            Assert.IsFalse(tree.HasErrors());
            reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.SequenceProg.darl"));
            code = reader.ReadToEnd();
            tree = runtime.CreateTree(code);
            source = tree.ToDarl();
            tree = runtime.CreateTree(source);
            Assert.IsFalse(tree.HasErrors());
            reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.Personality.darl"));
            code = reader.ReadToEnd();
            tree = runtime.CreateTree(code);
            source = tree.ToDarl();
            tree = runtime.CreateTree(source);
            Assert.IsFalse(tree.HasErrors());
            reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.logistic.darl"));
            code = reader.ReadToEnd();
            tree = runtime.CreateTree(code);
            source = tree.ToDarl();
            tree = runtime.CreateTree(source);
            Assert.IsFalse(tree.HasErrors());

        }

    }

}
