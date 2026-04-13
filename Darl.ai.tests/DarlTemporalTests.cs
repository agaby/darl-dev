/// </summary>

﻿using DarlLanguage;
using DarlLanguage.Processing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Darl_standard_core.test
{
    [TestClass]
    public class DarlTemporalTests
    {
        [TestMethod]
        public async Task TestTemporalParsing()
        {
            var runtime = new DarlRunTime();
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.temporal.darl"));
            string source = reader.ReadToEnd();
            var tree = runtime.CreateTree(source);
            Assert.IsFalse(tree.HasErrors());
            //test trial expired
            var results = new List<DarlResult>
            {
                new DarlResult("current_date",new DateTime(2018, 6, 1), DarlResult.DataType.temporal) ,
                new DarlResult("trial_start",new DateTime(2018, 5, 1), DarlResult.DataType.temporal),
                new DarlResult("last_payment_date", new DateTime(2018, 4, 1), DarlResult.DataType.temporal),
                new DarlResult("usage_count", 900),
                new DarlResult("account_type", "personal") ,
                new DarlResult("account_state", "trial") ,
                new DarlResult("last_account_paid", "true") ,
                new DarlResult("last_billing_date", 0, true) ,
                new DarlResult("invoice_template", "", DarlResult.DataType.textual) ,
                new DarlResult("delinquent_template", "", DarlResult.DataType.textual) ,
                new DarlResult("suspended_template", "", DarlResult.DataType.textual) ,
                new DarlResult("trial_expired_template", "", DarlResult.DataType.textual) ,
                new DarlResult("name", "", DarlResult.DataType.textual) ,
                new DarlResult( "company", "", DarlResult.DataType.textual) ,
                new DarlResult("email_to", "", DarlResult.DataType.textual) ,
                new DarlResult("email_from", "", DarlResult.DataType.textual),
                new DarlResult("signed_up", "false")
            };
            var res = await runtime.Evaluate(tree, results);
            Assert.AreEqual("trial_expired", res.First(a => a.name == "new_account_state").Value);
            Assert.AreEqual("true", res.First(a => a.name == "trial_expired_flag").Value);
            Assert.AreEqual("true", res.First(a => a.name == "create_email").Value);
            Assert.AreEqual("false", res.First(a => a.name == "delinquent_flag").Value);
            Assert.AreEqual("false", res.First(a => a.name == "suspended_flag").Value);
            Assert.AreEqual("false", res.First(a => a.name == "invoice_flag").Value);
            Assert.IsTrue(res.First(a => a.name == "total_charge").IsUnknown());
            Assert.IsTrue(res.First(a => a.name == "usage_charge").IsUnknown());
            Assert.IsTrue(res.First(a => a.name == "license_charge").IsUnknown());
            //test still in trial
            results = new List<DarlResult>
            {
                new DarlResult("current_date", new DateTime(2018, 6, 1), DarlResult.DataType.temporal),
                new DarlResult("trial_start", new DateTime(2018, 5, 24), DarlResult.DataType.temporal),
                new DarlResult("last_payment_date", new DateTime(2018, 4, 1), DarlResult.DataType.temporal),
                new DarlResult("usage_count", 900),
                new DarlResult("account_type", "personal"),
                new DarlResult("account_state", "trial"),
                new DarlResult("last_account_paid", "true"),
                new DarlResult("last_billing_date", 0, true),
                new DarlResult("invoice_template", "", DarlResult.DataType.textual),
                new DarlResult("delinquent_template", "", DarlResult.DataType.textual),
                new DarlResult("suspended_template", "", DarlResult.DataType.textual),
                new DarlResult("trial_expired_template", "", DarlResult.DataType.textual),
                new DarlResult("name", "", DarlResult.DataType.textual),
                new DarlResult("company", "", DarlResult.DataType.textual),
                new DarlResult("email_to", "", DarlResult.DataType.textual),
                new DarlResult("email_from", "", DarlResult.DataType.textual),
                new DarlResult("signed_up", "false")
            };
            res = await runtime.Evaluate(tree, results);
            Assert.AreEqual("trial", res.First(a => a.name == "new_account_state").Value);
            Assert.AreEqual("false", res.First(a => a.name == "trial_expired_flag").Value);
            Assert.AreEqual("false", res.First(a => a.name == "create_email").Value);
            Assert.AreEqual("false", res.First(a => a.name == "delinquent_flag").Value);
            Assert.AreEqual("false", res.First(a => a.name == "suspended_flag").Value);
            Assert.AreEqual("false", res.First(a => a.name == "invoice_flag").Value);
            Assert.IsTrue(res.First(a => a.name == "total_charge").IsUnknown());
            Assert.IsTrue(res.First(a => a.name == "usage_charge").IsUnknown());
            Assert.IsTrue(res.First(a => a.name == "license_charge").IsUnknown());
            //test paying but well overdue
            results = new List<DarlResult>
            {
                new DarlResult("current_date", new DateTime(2018, 6, 1), DarlResult.DataType.temporal),
                new DarlResult("trial_start", new DateTime(2018, 5, 24), DarlResult.DataType.temporal),
                new DarlResult("last_payment_date", new DateTime(2018, 2, 1), DarlResult.DataType.temporal),
                new DarlResult("usage_count", 900),
                new DarlResult("account_type", "personal"),
                new DarlResult("account_state", "paying"),
                new DarlResult("last_account_paid", "false"),
                new DarlResult("last_billing_date", new DateTime(2018, 3, 1), DarlResult.DataType.temporal),
                new DarlResult("invoice_template", "", DarlResult.DataType.textual),
                new DarlResult("delinquent_template", "", DarlResult.DataType.textual),
                new DarlResult("suspended_template", "", DarlResult.DataType.textual),
                new DarlResult("trial_expired_template", "", DarlResult.DataType.textual),
                new DarlResult("name", "", DarlResult.DataType.textual),
                new DarlResult("company", "", DarlResult.DataType.textual),
                new DarlResult("email_to", "", DarlResult.DataType.textual),
                new DarlResult("email_from", "", DarlResult.DataType.textual),
                new DarlResult("signed_up", "false")
            };
            res = await runtime.Evaluate(tree, results);
            Assert.AreEqual("suspended", res.First(a => a.name == "new_account_state").Value);
            Assert.AreEqual("false", res.First(a => a.name == "trial_expired_flag").Value);
            Assert.AreEqual("true", res.First(a => a.name == "create_email").Value);
            Assert.AreEqual("false", res.First(a => a.name == "delinquent_flag").Value);
            Assert.AreEqual("true", res.First(a => a.name == "suspended_flag").Value);
            Assert.AreEqual("false", res.First(a => a.name == "invoice_flag").Value);
            Assert.IsTrue(res.First(a => a.name == "total_charge").IsUnknown());
            Assert.IsTrue(res.First(a => a.name == "usage_charge").IsUnknown());
            Assert.IsTrue(res.First(a => a.name == "license_charge").IsUnknown());
            //test paying but just overdue
            results = new List<DarlResult>
            {
                new DarlResult("current_date", new DateTime(2018, 6, 1), DarlResult.DataType.temporal),
                new DarlResult("trial_start", new DateTime(2018, 5, 24), DarlResult.DataType.temporal),
                new DarlResult("last_payment_date", new DateTime(2018, 2, 1), DarlResult.DataType.temporal),
                new DarlResult("usage_count", 900),
                new DarlResult("account_type", "personal"),
                new DarlResult("account_state", "paying"),
                new DarlResult("last_account_paid", "false"),
                new DarlResult("last_billing_date", new DateTime(2018, 5, 22), DarlResult.DataType.temporal),
                new DarlResult("invoice_template", "", DarlResult.DataType.textual),
                new DarlResult("delinquent_template", "", DarlResult.DataType.textual),
                new DarlResult("suspended_template", "", DarlResult.DataType.textual),
                new DarlResult("trial_expired_template", "", DarlResult.DataType.textual),
                new DarlResult("name", "", DarlResult.DataType.textual),
                new DarlResult("company", "", DarlResult.DataType.textual),
                new DarlResult("email_to", "", DarlResult.DataType.textual),
                new DarlResult("email_from", "", DarlResult.DataType.textual),
                new DarlResult("signed_up", "false")
            };
            res = await runtime.Evaluate(tree, results);
            Assert.AreEqual("delinquent", res.First(a => a.name == "new_account_state").Value);
            Assert.AreEqual("false", res.First(a => a.name == "trial_expired_flag").Value);
            Assert.AreEqual("true", res.First(a => a.name == "create_email").Value);
            Assert.AreEqual("true", res.First(a => a.name == "delinquent_flag").Value);
            Assert.AreEqual("false", res.First(a => a.name == "suspended_flag").Value);
            Assert.AreEqual("false", res.First(a => a.name == "invoice_flag").Value);
            Assert.IsTrue(res.First(a => a.name == "total_charge").IsUnknown());
            Assert.IsTrue(res.First(a => a.name == "usage_charge").IsUnknown());
            Assert.IsTrue(res.First(a => a.name == "license_charge").IsUnknown());
            //test paying no problems
            results = new List<DarlResult>
            {
                new DarlResult("current_date", new DateTime(2018, 6, 1), DarlResult.DataType.temporal),
                new DarlResult("trial_start", new DateTime(2018, 5, 24), DarlResult.DataType.temporal),
                new DarlResult("last_payment_date", new DateTime(2018, 5, 1), DarlResult.DataType.temporal),
                new DarlResult("usage_count", 900),
                new DarlResult("account_type", "personal"),
                new DarlResult("account_state", "paying"),
                new DarlResult("last_account_paid", "true"),
                new DarlResult("last_billing_date", new DateTime(2018, 5, 22), DarlResult.DataType.temporal),
                new DarlResult("invoice_template", "", DarlResult.DataType.textual),
                new DarlResult("delinquent_template", "", DarlResult.DataType.textual),
                new DarlResult("suspended_template", "", DarlResult.DataType.textual),
                new DarlResult("trial_expired_template", "", DarlResult.DataType.textual),
                new DarlResult("name", "", DarlResult.DataType.textual),
                new DarlResult("company", "", DarlResult.DataType.textual),
                new DarlResult("email_to", "", DarlResult.DataType.textual),
                new DarlResult("email_from", "", DarlResult.DataType.textual),
                new DarlResult("signed_up", "false")
            };
            res = await runtime.Evaluate(tree, results);
            Assert.AreEqual("paying", res.First(a => a.name == "new_account_state").Value);
            Assert.AreEqual("false", res.First(a => a.name == "trial_expired_flag").Value);
            Assert.AreEqual("false", res.First(a => a.name == "create_email").Value);
            Assert.AreEqual("false", res.First(a => a.name == "delinquent_flag").Value);
            Assert.AreEqual("false", res.First(a => a.name == "suspended_flag").Value);
            Assert.AreEqual("false", res.First(a => a.name == "invoice_flag").Value);
            Assert.IsTrue(res.First(a => a.name == "total_charge").IsUnknown());
            Assert.IsTrue(res.First(a => a.name == "usage_charge").IsUnknown());
            Assert.IsTrue(res.First(a => a.name == "license_charge").IsUnknown());
        }


        [TestMethod]
        public void TestIntersection()
        {
            var res1 = new DarlResult(5, 10, 20, 25);
            var res2 = new DarlResult(3, 15, 15, 27);
            var res = DarlResult.Intersection(res1, res2);
            Assert.AreEqual(0.2857, (double)res.values[0], 0.001);
            res1 = new DarlResult(5, 10, 20, 25);
            res2 = new DarlResult(7, 7, 7, 7);
            res = DarlResult.Intersection(res1, res2);
            Assert.AreEqual(0.4, (double)res.values[0], 0.001);
            res1 = new DarlResult(5, 10, 20, 25);
            res2 = new DarlResult(23, 23, 23, 23);
            res = DarlResult.Intersection(res1, res2);
            Assert.AreEqual(0.4, (double)res.values[0], 0.001);
            res1 = new DarlResult(5, 10, 20, 25);
            res2 = new DarlResult(11, 11, 23, 23);
            res = DarlResult.Intersection(res1, res2);
            Assert.AreEqual(0.4, (double)res.values[0], 0.001);
        }

        [TestMethod]
        public void TestDuring()
        {
            var res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            var res1 = new DarlResult("", new List<double> { 3, 15, 15, 27 }, DarlResult.DataType.temporal);
            var res = DarlResult.During(res1, res2);
            Assert.AreEqual(0.2857, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 7, 7, 7, 7 }, DarlResult.DataType.temporal);
            res = DarlResult.During(res1, res2);
            Assert.AreEqual(0.4, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 23, 23, 23, 23 }, DarlResult.DataType.temporal);
            res = DarlResult.During(res1, res2);
            Assert.AreEqual(0.4, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 11, 11, 23, 23 }, DarlResult.DataType.temporal);
            res = DarlResult.During(res1, res2);
            Assert.AreEqual(0.4, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 4, 4, 4, 4 }, DarlResult.DataType.temporal);
            res = DarlResult.During(res1, res2);
            Assert.AreEqual(0, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 26, 26, 26, 26 }, DarlResult.DataType.temporal);
            res = DarlResult.During(res1, res2);
            Assert.AreEqual(0, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 18, 18, 18, 18 }, DarlResult.DataType.temporal);
            res = DarlResult.During(res1, res2);
            Assert.AreEqual(1, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 12, 12, 18, 18 }, DarlResult.DataType.temporal);
            res = DarlResult.During(res1, res2);
            Assert.AreEqual(1, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 12, 16, 16, 18 }, DarlResult.DataType.temporal);
            res = DarlResult.During(res1, res2);
            Assert.AreEqual(1, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 12, 16, 20, 25 }, DarlResult.DataType.temporal);
            res = DarlResult.During(res1, res2);
            Assert.AreEqual(1, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res = DarlResult.During(res1, res2);
            Assert.AreEqual(0, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 5, 10, 16, 18 }, DarlResult.DataType.temporal);
            res = DarlResult.During(res1, res2);
            Assert.AreEqual(1, (double)res.values[0], 0.001);
        }

        [TestMethod]
        public void TestBefore()
        {
            var res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            var res1 = new DarlResult("", new List<double> { 3, 15, 15, 27 }, DarlResult.DataType.temporal);
            var res = DarlResult.Before(res1, res2);
            Assert.AreEqual(0, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 7, 7, 7, 7 }, DarlResult.DataType.temporal);
            res = DarlResult.Before(res1, res2);
            Assert.AreEqual(0.6, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 23, 23, 23, 23 }, DarlResult.DataType.temporal);
            res = DarlResult.Before(res1, res2);
            Assert.AreEqual(0, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 11, 11, 23, 23 }, DarlResult.DataType.temporal);
            res = DarlResult.Before(res1, res2);
            Assert.AreEqual(0, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 4, 4, 4, 4 }, DarlResult.DataType.temporal);
            res = DarlResult.Before(res1, res2);
            Assert.AreEqual(1, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 26, 26, 26, 26 }, DarlResult.DataType.temporal);
            res = DarlResult.Before(res1, res2);
            Assert.AreEqual(0, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 18, 18, 18, 18 }, DarlResult.DataType.temporal);
            res = DarlResult.Before(res1, res2);
            Assert.AreEqual(0, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 12, 12, 18, 18 }, DarlResult.DataType.temporal);
            res = DarlResult.Before(res1, res2);
            Assert.AreEqual(0, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 12, 16, 16, 18 }, DarlResult.DataType.temporal);
            res = DarlResult.Before(res1, res2);
            Assert.AreEqual(0, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 12, 16, 20, 25 }, DarlResult.DataType.temporal);
            res = DarlResult.Before(res1, res2);
            Assert.AreEqual(0, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res = DarlResult.Before(res1, res2);
            Assert.AreEqual(0, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 5, 5, 5, 5 }, DarlResult.DataType.temporal);
            res = DarlResult.Before(res1, res2);
            Assert.AreEqual(1, (double)res.values[0], 0.001);

        }

        [TestMethod]
        public void TestAfter()
        {
            var res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            var res1 = new DarlResult("", new List<double> { 3, 15, 15, 27 }, DarlResult.DataType.temporal);
            var res = DarlResult.After(res1, res2);
            Assert.AreEqual(0, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 7, 7, 7, 7 }, DarlResult.DataType.temporal);
            res = DarlResult.After(res1, res2);
            Assert.AreEqual(0, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 23, 23, 23, 23 }, DarlResult.DataType.temporal);
            res = DarlResult.After(res1, res2);
            Assert.AreEqual(0.6, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 11, 11, 23, 23 }, DarlResult.DataType.temporal);
            res = DarlResult.After(res1, res2);
            Assert.AreEqual(0, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 4, 4, 4, 4 }, DarlResult.DataType.temporal);
            res = DarlResult.After(res1, res2);
            Assert.AreEqual(0, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 26, 26, 26, 26 }, DarlResult.DataType.temporal);
            res = DarlResult.After(res1, res2);
            Assert.AreEqual(1, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 18, 18, 18, 18 }, DarlResult.DataType.temporal);
            res = DarlResult.After(res1, res2);
            Assert.AreEqual(0, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 12, 12, 18, 18 }, DarlResult.DataType.temporal);
            res = DarlResult.After(res1, res2);
            Assert.AreEqual(0, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 12, 16, 16, 18 }, DarlResult.DataType.temporal);
            res = DarlResult.After(res1, res2);
            Assert.AreEqual(0, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 12, 16, 20, 25 }, DarlResult.DataType.temporal);
            res = DarlResult.After(res1, res2);
            Assert.AreEqual(0, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res = DarlResult.After(res1, res2);
            Assert.AreEqual(0, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 25, 25, 25, 25 }, DarlResult.DataType.temporal);
            res = DarlResult.After(res1, res2);
            Assert.AreEqual(1, (double)res.values[0], 0.001);
        }

        [TestMethod]
        public void TestOverlapping()
        {
            var res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            var res1 = new DarlResult("", new List<double> { 3, 15, 15, 27 }, DarlResult.DataType.temporal);
            var res = DarlResult.Overlapping(res1, res2);
            Assert.AreEqual(0.7142, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 7, 7, 7, 7 }, DarlResult.DataType.temporal);
            res = DarlResult.Overlapping(res1, res2);
            Assert.AreEqual(0, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 23, 23, 23, 23 }, DarlResult.DataType.temporal);
            res = DarlResult.Overlapping(res1, res2);
            Assert.AreEqual(0, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 11, 11, 23, 23 }, DarlResult.DataType.temporal);
            res = DarlResult.Overlapping(res1, res2);
            Assert.AreEqual(0.6, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 4, 4, 4, 4 }, DarlResult.DataType.temporal);
            res = DarlResult.Overlapping(res1, res2);
            Assert.AreEqual(0, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 26, 26, 26, 26 }, DarlResult.DataType.temporal);
            res = DarlResult.Overlapping(res1, res2);
            Assert.AreEqual(0, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 18, 18, 18, 18 }, DarlResult.DataType.temporal);
            res = DarlResult.Overlapping(res1, res2);
            Assert.AreEqual(0, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 12, 12, 18, 18 }, DarlResult.DataType.temporal);
            res = DarlResult.Overlapping(res1, res2);
            Assert.AreEqual(0, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 12, 16, 16, 18 }, DarlResult.DataType.temporal);
            res = DarlResult.Overlapping(res1, res2);
            Assert.AreEqual(0, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 12, 16, 20, 25 }, DarlResult.DataType.temporal);
            res = DarlResult.Overlapping(res1, res2);
            Assert.AreEqual(0, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res = DarlResult.Overlapping(res1, res2);
            Assert.AreEqual(0, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 25, 25, 25, 25 }, DarlResult.DataType.temporal);
            res = DarlResult.Overlapping(res1, res2);
            Assert.AreEqual(0, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 0, 2, 12, 17 }, DarlResult.DataType.temporal);
            res = DarlResult.Overlapping(res1, res2);
            Assert.AreEqual(1, (double)res.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 15, 18, 30, 35 }, DarlResult.DataType.temporal);
            res = DarlResult.Overlapping(res1, res2);
            Assert.AreEqual(1, (double)res.values[0], 0.001);
        }

        [TestMethod]
        public void TestOrthogonality()
        {
            var res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            var res1 = new DarlResult("", new List<double> { 3, 15, 15, 27 }, DarlResult.DataType.temporal);
            var reso = DarlResult.Overlapping(res1, res2);
            var resd = DarlResult.During(res1, res2);
            var resa = DarlResult.After(res1, res2);
            var resb = DarlResult.Before(res1, res2);
            var rese = DarlResult.TempEqual(res1, res2);
            Assert.AreEqual(1, (double)reso.values[0] + (double)resd.values[0] + (double)resa.values[0] + (double)resb.values[0] + (double)rese.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 7, 7, 7, 7 }, DarlResult.DataType.temporal);
            reso = DarlResult.Overlapping(res1, res2);
            resd = DarlResult.During(res1, res2);
            resa = DarlResult.After(res1, res2);
            resb = DarlResult.Before(res1, res2);
            rese = DarlResult.TempEqual(res1, res2);
            Assert.AreEqual(1, (double)reso.values[0] + (double)resd.values[0] + (double)resa.values[0] + (double)resb.values[0] + (double)rese.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 23, 23, 23, 23 }, DarlResult.DataType.temporal);
            reso = DarlResult.Overlapping(res1, res2);
            resd = DarlResult.During(res1, res2);
            resa = DarlResult.After(res1, res2);
            resb = DarlResult.Before(res1, res2);
            rese = DarlResult.TempEqual(res1, res2);
            Assert.AreEqual(1, (double)reso.values[0] + (double)resd.values[0] + (double)resa.values[0] + (double)resb.values[0] + (double)rese.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 11, 11, 23, 23 }, DarlResult.DataType.temporal);
            reso = DarlResult.Overlapping(res1, res2);
            resd = DarlResult.During(res1, res2);
            resa = DarlResult.After(res1, res2);
            resb = DarlResult.Before(res1, res2);
            rese = DarlResult.TempEqual(res1, res2);
            Assert.AreEqual(1, (double)reso.values[0] + (double)resd.values[0] + (double)resa.values[0] + (double)resb.values[0] + (double)rese.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 4, 4, 4, 4 }, DarlResult.DataType.temporal);
            reso = DarlResult.Overlapping(res1, res2);
            resd = DarlResult.During(res1, res2);
            resa = DarlResult.After(res1, res2);
            resb = DarlResult.Before(res1, res2);
            rese = DarlResult.TempEqual(res1, res2);
            Assert.AreEqual(1, (double)reso.values[0] + (double)resd.values[0] + (double)resa.values[0] + (double)resb.values[0] + (double)rese.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 26, 26, 26, 26 }, DarlResult.DataType.temporal);
            reso = DarlResult.Overlapping(res1, res2);
            resd = DarlResult.During(res1, res2);
            resa = DarlResult.After(res1, res2);
            resb = DarlResult.Before(res1, res2);
            rese = DarlResult.TempEqual(res1, res2);
            Assert.AreEqual(1, (double)reso.values[0] + (double)resd.values[0] + (double)resa.values[0] + (double)resb.values[0] + (double)rese.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 18, 18, 18, 18 }, DarlResult.DataType.temporal);
            reso = DarlResult.Overlapping(res1, res2);
            resd = DarlResult.During(res1, res2);
            resa = DarlResult.After(res1, res2);
            resb = DarlResult.Before(res1, res2);
            rese = DarlResult.TempEqual(res1, res2);
            Assert.AreEqual(1, (double)reso.values[0] + (double)resd.values[0] + (double)resa.values[0] + (double)resb.values[0] + (double)rese.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 12, 12, 18, 18 }, DarlResult.DataType.temporal);
            reso = DarlResult.Overlapping(res1, res2);
            resd = DarlResult.During(res1, res2);
            resa = DarlResult.After(res1, res2);
            resb = DarlResult.Before(res1, res2);
            rese = DarlResult.TempEqual(res1, res2);
            Assert.AreEqual(1, (double)reso.values[0] + (double)resd.values[0] + (double)resa.values[0] + (double)resb.values[0] + (double)rese.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 12, 16, 16, 18 }, DarlResult.DataType.temporal);
            reso = DarlResult.Overlapping(res1, res2);
            resd = DarlResult.During(res1, res2);
            resa = DarlResult.After(res1, res2);
            resb = DarlResult.Before(res1, res2);
            rese = DarlResult.TempEqual(res1, res2);
            Assert.AreEqual(1, (double)reso.values[0] + (double)resd.values[0] + (double)resa.values[0] + (double)resb.values[0] + (double)rese.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 12, 16, 20, 25 }, DarlResult.DataType.temporal);
            reso = DarlResult.Overlapping(res1, res2);
            resd = DarlResult.During(res1, res2);
            resa = DarlResult.After(res1, res2);
            resb = DarlResult.Before(res1, res2);
            rese = DarlResult.TempEqual(res1, res2);
            Assert.AreEqual(1, (double)reso.values[0] + (double)resd.values[0] + (double)resa.values[0] + (double)resb.values[0] + (double)rese.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            reso = DarlResult.Overlapping(res1, res2);
            resd = DarlResult.During(res1, res2);
            resa = DarlResult.After(res1, res2);
            resb = DarlResult.Before(res1, res2);
            rese = DarlResult.TempEqual(res1, res2);
            Assert.AreEqual(1, (double)reso.values[0] + (double)resd.values[0] + (double)resa.values[0] + (double)resb.values[0] + (double)rese.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 25, 25, 25, 25 }, DarlResult.DataType.temporal);
            reso = DarlResult.Overlapping(res1, res2);
            resd = DarlResult.During(res1, res2);
            resa = DarlResult.After(res1, res2);
            resb = DarlResult.Before(res1, res2);
            rese = DarlResult.TempEqual(res1, res2);
            Assert.AreEqual(1, (double)reso.values[0] + (double)resd.values[0] + (double)resa.values[0] + (double)resb.values[0] + (double)rese.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 0, 2, 12, 17 }, DarlResult.DataType.temporal);
            reso = DarlResult.Overlapping(res1, res2);
            resd = DarlResult.During(res1, res2);
            resa = DarlResult.After(res1, res2);
            resb = DarlResult.Before(res1, res2);
            rese = DarlResult.TempEqual(res1, res2);
            Assert.AreEqual(1, (double)reso.values[0] + (double)resd.values[0] + (double)resa.values[0] + (double)resb.values[0] + (double)rese.values[0], 0.001);
            res2 = new DarlResult("", new List<double> { 5, 10, 20, 25 }, DarlResult.DataType.temporal);
            res1 = new DarlResult("", new List<double> { 15, 18, 30, 35 }, DarlResult.DataType.temporal);
            reso = DarlResult.Overlapping(res1, res2);
            resd = DarlResult.During(res1, res2);
            resa = DarlResult.After(res1, res2);
            resb = DarlResult.Before(res1, res2);
            rese = DarlResult.TempEqual(res1, res2);
            Assert.AreEqual(1, (double)reso.values[0] + (double)resd.values[0] + (double)resa.values[0] + (double)resb.values[0] + (double)rese.values[0], 0.001);
        }

        [TestMethod]
        public async Task TestNow()
        {
            var runtime = new DarlRunTime();
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.EUClaimTemporal.darl"));
            string source = reader.ReadToEnd();
            var tree = runtime.CreateTree(source);
            Assert.IsFalse(tree.HasErrors());
            //test trial expired
            var results = new List<DarlResult>
            {
                new DarlResult("endsInEU", "both") ,
                new DarlResult("delayInHours",5),
                new DarlResult("reasonForDelay", "all_others"),
                new DarlResult("flightLength", 5000),
                new DarlResult("flightNumber", "lkjlkajslkjl", DarlResult.DataType.textual) ,
                new DarlResult("bookingRef", "uyiyiy", DarlResult.DataType.textual) ,
                new DarlResult("dateOfFlight",  (DateTime.Now - TimeSpan.FromDays(1825)), DarlResult.DataType.temporal), //five years ago
                new DarlResult("timeOfFlight", "08:00", DarlResult.DataType.textual) ,
                new DarlResult("fullName", "andy edmonds", DarlResult.DataType.textual) ,
                new DarlResult("docText", "", DarlResult.DataType.textual) ,
                new DarlResult("cannotClaimText", "", DarlResult.DataType.textual)
            };
            var res = await runtime.Evaluate(tree, results);
            Assert.AreEqual("true", res[32].Value);
            var dof = (DateTime.Now - TimeSpan.FromDays(1810)).ToString("yyyy-MM-dd");
            results = new List<DarlResult>
            {
                new DarlResult("endsInEU", "both") ,
                new DarlResult("delayInHours",5),
                new DarlResult("reasonForDelay", "all_others"),
                new DarlResult("flightLength", 5000),
                new DarlResult("flightNumber", "lkjlkajslkjl", DarlResult.DataType.textual) ,
                new DarlResult("bookingRef", "uyiyiy", DarlResult.DataType.textual) ,
                new DarlResult("dateOfFlight",  dof, DarlResult.DataType.temporal) ,
                new DarlResult("timeOfFlight", "08:00", DarlResult.DataType.textual) ,
                new DarlResult("fullName", "andy edmonds", DarlResult.DataType.textual) ,
                new DarlResult("docText", "", DarlResult.DataType.textual) ,
                new DarlResult("cannotClaimText", "", DarlResult.DataType.textual)
            };
            res = await runtime.Evaluate(tree, results);
            Assert.AreEqual("true", res[32].Value);

            results = new List<DarlResult>
            {
                new DarlResult("endsInEU", "both") ,
                new DarlResult("delayInHours",5),
                new DarlResult("reasonForDelay", "all_others"),
                new DarlResult("flightLength", 5000),
                new DarlResult("flightNumber", "lkjlkajslkjl", DarlResult.DataType.textual) ,
                new DarlResult("bookingRef", "uyiyiy", DarlResult.DataType.textual) ,
                new DarlResult("dateOfFlight",  new DateTime(2010, 6, 1), DarlResult.DataType.temporal) ,
                new DarlResult("timeOfFlight", "08:00", DarlResult.DataType.textual) ,
                new DarlResult("fullName", "andy edmonds", DarlResult.DataType.textual) ,
                new DarlResult("docText", "", DarlResult.DataType.textual) ,
                new DarlResult("cannotClaimText", "", DarlResult.DataType.textual)
            };
            res = await runtime.Evaluate(tree, results);
            Assert.AreEqual("false", res[32].Value);
            results = new List<DarlResult>
            {
                new DarlResult("endsInEU", "both") ,
                new DarlResult("delayInHours",5),
                new DarlResult("reasonForDelay", "all_others"),
                new DarlResult("flightLength", 5000),
                new DarlResult("flightNumber", "lkjlkajslkjl", DarlResult.DataType.textual) ,
                new DarlResult("bookingRef", "uyiyiy", DarlResult.DataType.textual) ,
                new DarlResult("dateOfFlight",  "2010-01-01", DarlResult.DataType.temporal) ,
                new DarlResult("timeOfFlight", "08:00", DarlResult.DataType.textual) ,
                new DarlResult("fullName", "andy edmonds", DarlResult.DataType.textual) ,
                new DarlResult("docText", "", DarlResult.DataType.textual) ,
                new DarlResult("cannotClaimText", "", DarlResult.DataType.textual)
            };
            res = await runtime.Evaluate(tree, results);
            Assert.AreEqual("false", res[32].Value);
        }

    }
}
