using Datl.Language;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Darl_standard_core.test
{
    [TestClass]
    public class TextTest
    {
        [TestMethod]
        public void TestTextDocumentParse()
        {
            var docsource = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.EUClaimText.txt"));
            var doc = docsource.ReadToEnd();


            var results = new Dictionary<string, string>();
            results.Add("endsInEU", "both");
            results.Add("withinSixYear", "true");
            results.Add("delayInHours", "3");
            results.Add("reasonForDelay", "all_others");
            results.Add("flightLength", "1000");
            results.Add("bookingRef", "45678");
            results.Add("dateOfFlight", "06/11/13");
            results.Add("timeOfFlight", "7.55");
            results.Add("fullName", "Andy Edmonds");
            results.Add("flightNumber", "FU1234");
            results.Add("claim", "EU_GT1500");
            var t = new TextProcess();
            var s = t.Parse(doc, results) as string;
            Assert.IsTrue(s.Contains("400"));

            docsource = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.DocumentTestTemplate.txt"));
            doc = docsource.ReadToEnd();

            results = new Dictionary<string, string>();
            results.Add("name", "Alfred George Bloggs");
            results.Add("email", "andy@scientio.com");
            results.Add("ticket_number", "ABC12345");
            results.Add("date_of_ticket", "1/01/2011");
            results.Add("time_of_ticket", "6:55");
            results.Add("reg_number", "RTA 151G");
            results.Add("council", "Milton Keynes");
            results.Add("grounds", "res");
            results.Add("permit_number", "567890");
            results.Add("time_of_arrival", "5:55");
            results.Add("incorrect_date", "06/11/1955");
            results.Add("correct_date", "06/11/1966");
            s = t.Parse(doc, results) as string;
            Assert.IsTrue(s.Contains("06/11/1955"));


            docsource = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.Cross_border.txt"));
            doc = docsource.ReadToEnd();
            results = new Dictionary<string, string>();
            results.Add("source_country", "United Kingdom");
            results.Add("dest_country", "United Kingdom");
            results.Add("data_type", "normal");
            results.Add("business_purpose", "AML");
            s = t.Parse(doc, results) as string;
            Assert.IsTrue(s.Contains("approved"));
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void TestTextDocumentErrors1()
        {
            var docsource = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.unbalancedclose.txt"));
            var doc = docsource.ReadToEnd();
            var results = new Dictionary<string, string>();
            results.Add("var1", "true");
            results.Add("var2", "true");
            var t = new TextProcess();
            var s = t.Parse(doc, results) as string;
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void TestTextDocumentErrors2()
        {
            var docsource = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.unbalancedopen.txt"));
            var doc = docsource.ReadToEnd();
            var results = new Dictionary<string, string>();
            results.Add("var1", "true");
            results.Add("var2", "true");
            var t = new TextProcess();
            var s = t.Parse(doc, results) as string;
        }

        [TestMethod]
        public void TestTextDocumentVariables()
        {
            var docsource = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.Variables.txt"));
            var doc = docsource.ReadToEnd();
            var results = new Dictionary<string, string>();
            results.Add("var1", "true");
            results.Add("var2", "true");
            var t = new TextProcess();
            var s = t.Parse(doc, results) as string;
            Assert.AreEqual("true stuff true  conditional stuff ", s);
        }

        [TestMethod]
        public void TestTextDocumentNoVariables()
        {
            var docsource = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.NoVar.txt"));
            var doc = docsource.ReadToEnd();
            var results = new Dictionary<string, string>();
            results.Add("var1", "true");
            results.Add("var2", "true");
            var t = new TextProcess();
            var s = t.Parse(doc, results) as string;
            Assert.AreEqual(doc, s);
        }
    }
}
