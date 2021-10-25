using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using DarlLanguage;
using DarlLanguage.Processing;
using Datl.Language;
using System.Threading.Tasks;
using System.Linq;

namespace Darl_standard_core.test
{
    [TestClass]
    public class WordTest
    {
        [TestMethod]
        [Ignore]
        public void TestWordDocumentParse()
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl_standard_core.test.parkingappealnew.docx");
            var w = new WordProcessGem();
            var results = new Dictionary<string, string>();
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
            stream.Position = 0;
            var p = w.Parse(stream, results) as MemoryStream;
            using (FileStream file = new FileStream("result_res.docx", FileMode.Create, FileAccess.Write))
            {
                p.CopyTo(file);
                p.Close();
            }

            results = new Dictionary<string, string>();            
            results.Add("name", "Alfred George Bloggs");
            results.Add("email", "andy@scientio.com");
            results.Add("ticket_number", "ABC12345");
            results.Add("date_of_ticket", "1/01/2011");
            results.Add("time_of_ticket", "6:55");
            results.Add("reg_number", "RTA 151G");
            results.Add("council", "Milton Keynes");
            results.Add("grounds", "parkingbay");
            results.Add("street_name", "Great Poop St.");
            results.Add("bay_width", "150");
            stream.Position = 0;
            p = w.Parse(stream, results) as MemoryStream;
            using (FileStream file = new FileStream("result_parkingbay.docx", FileMode.Create, FileAccess.Write))
            {
                p.CopyTo(file);
                p.Close();
            }


            results = new Dictionary<string, string>();
            results.Add("name", "Alfred George Bloggs");
            results.Add("email", "andy@scientio.com");
            results.Add("ticket_number", "ABC12345");
            results.Add("date_of_ticket", "1/01/2011");
            results.Add("time_of_ticket", "6:55");
            results.Add("reg_number", "RTA 151G");
            results.Add("council", "Milton Keynes");
            results.Add("grounds", "stolen");
            results.Add("date_of_theft", "06/11/1955");
            results.Add("police_station"," Milton Keynes");
            results.Add("crime_reference", "oldbill1234567");
            results.Add("date_of_report", "05/11/1955");
            stream.Position = 0;
            p = w.Parse(stream, results) as MemoryStream;
            using (FileStream file = new FileStream("result_stolen.docx", FileMode.Create, FileAccess.Write))
            {
                p.CopyTo(file);
                p.Close();
            }


            results = new Dictionary<string, string>();
            results.Add("name", "Alfred George Bloggs");
            results.Add("email", "andy@scientio.com");
            results.Add("ticket_number", "ABC12345");
            results.Add("date_of_ticket", "1/01/2011");
            results.Add("time_of_ticket", "6:55");
            results.Add("reg_number", "RTA 151G");
            results.Add("council", "Milton Keynes");
            results.Add("grounds", "hospital");
            results.Add("hospital_name", "Milton Keynes General");
            results.Add("hospital_address", "27 chafron way");
            results.Add("hospital_postcode", "MK1 1gb");
            results.Add("medical_emergency", "terminal stupidity");
            results.Add("patient", "alfred gonad");
            stream.Position = 0;
            p = w.Parse(stream, results) as MemoryStream;
            using (FileStream file = new FileStream("result_hospital.docx", FileMode.Create, FileAccess.Write))
            {
                p.CopyTo(file);
                p.Close();
            }

            results = new Dictionary<string, string>();
            results.Add("name", "Alfred George Bloggs");
            results.Add("email", "andy@scientio.com");
            results.Add("ticket_number", "ABC12345");
            results.Add("date_of_ticket", "1/01/2011");
            results.Add("time_of_ticket", "6:55");
            results.Add("reg_number", "RTA 151G");
            results.Add("council", "Milton Keynes");
            results.Add("grounds", "ownershipbought");
            results.Add("previous_owner", "Horatio T Poopalot");
            results.Add("previous_address", "27 chafron way");
            results.Add("previous_postcode", "MK1 1gb");
            results.Add("date_of_purchase", "06/01/1967");
            stream.Position = 0;
            p = w.Parse(stream, results) as MemoryStream;
            using (FileStream file = new FileStream("result_ownershipbought.docx", FileMode.Create, FileAccess.Write))
            {
                p.CopyTo(file);
                p.Close();
            }

            results = new Dictionary<string, string>();            
            results.Add("name", "Alfred George Bloggs");
            results.Add("email", "andy@scientio.com");
            results.Add("ticket_number", "ABC12345");
            results.Add("date_of_ticket", "1/01/2011");
            results.Add("time_of_ticket", "6:55");
            results.Add("reg_number", "RTA 151G");
            results.Add("council", "Milton Keynes");
            results.Add("grounds", "ownershipsold");
            results.Add("subsequent_owner", "Horatio T Poopalot");
            results.Add("subsequent_address", "27 chafron way");
            results.Add("subsequent_postcode", "MK1 1gb");
            results.Add("date_of_sale", "06/01/1967");
            stream.Position = 0;
            p = w.Parse(stream, results) as MemoryStream;
            using (FileStream file = new FileStream("result_ownershipsold.docx", FileMode.Create, FileAccess.Write))
            {
                p.CopyTo(file);
                p.Close();
            }

            results = new Dictionary<string, string>();            
            results.Add("name", "Alfred George Bloggs");
            results.Add("email", "andy@scientio.com");
            results.Add("ticket_number", "ABC12345");
            results.Add("date_of_ticket", "1/01/2011");
            results.Add("time_of_ticket", "6:55");
            results.Add("reg_number", "RTA 151G");
            results.Add("council", "Milton Keynes");
            results.Add("grounds", "diplomat");
            results.Add("country", "Buggeria");
            results.Add("embassy_address", "27 chafron way");
            results.Add("embassy_postcode", "MK1 1gb");
            results.Add("ticket_type", "congestion");
            stream.Position = 0;
            p = w.Parse(stream, results) as MemoryStream;
            using (FileStream file = new FileStream("result_diplomat.docx", FileMode.Create, FileAccess.Write))
            {
                p.CopyTo(file);
                p.Close();
            }

            results = new Dictionary<string, string>();
            results.Add("name", "Alfred George Bloggs");
            results.Add("email", "andy@scientio.com");
            results.Add("ticket_number", "ABC12345");
            results.Add("date_of_ticket", "1/01/2011");
            results.Add("time_of_ticket", "6:55");
            results.Add("reg_number", "RTA 151G");
            results.Add("council", "Milton Keynes");
            results.Add("grounds", "reg");
            results.Add("incorrect_reg", "CMX 981A");
            results.Add("time_of_arrival", "5:55");
            stream.Position = 0;
            p = w.Parse(stream, results) as MemoryStream;
            using (FileStream file = new FileStream("result_reg.docx", FileMode.Create, FileAccess.Write))
            {
                p.CopyTo(file);
                p.Close();
            }

            results = new Dictionary<string, string>();            
            results.Add("name", "Alfred George Bloggs");
            results.Add("email", "andy@scientio.com");
            results.Add("ticket_number", "ABC12345");
            results.Add("date_of_ticket", "1/01/2011");
            results.Add("time_of_ticket", "6:55");
            results.Add("reg_number", "RTA 151G");
            results.Add("council", "Milton Keynes");
            results.Add("grounds", "missing");
            results.Add("missing_detail", "reason");
            results.Add("summary_of_offence", "Lack of political correctness");
            stream.Position = 0;
            p = w.Parse(stream, results) as MemoryStream;
            using (FileStream file = new FileStream("result_missing.docx", FileMode.Create, FileAccess.Write))
            {
                p.CopyTo(file);
                p.Close();
            }

            results = new Dictionary<string, string>();     
            results.Add("name", "Alfred George Bloggs");
            results.Add("email", "andy@scientio.com");
            results.Add("ticket_number", "ABC12345");
            results.Add("date_of_ticket", "1/01/2011");
            results.Add("time_of_ticket", "6:55");
            results.Add("reg_number", "RTA 151G");
            results.Add("council", "Milton Keynes");
            results.Add("grounds", "incorrect");
            results.Add("incorrect_detail", "reason");
            results.Add("summary_of_offence", "Lack of political correctness");
            results.Add("incorrect_text", "Pay up scum or we'll accuse you of kiddy fiddling");
            results.Add("correct_text", "you unfortunately overstayed");
            stream.Position = 0;
            p = w.Parse(stream, results) as MemoryStream;
            using (FileStream file = new FileStream("result_incorrect.docx", FileMode.Create, FileAccess.Write))
            {
                p.CopyTo(file);
                p.Close();
            }

            results = new Dictionary<string, string>();            
            results.Add("name", "Alfred George Bloggs");
            results.Add("email", "andy@scientio.com");
            results.Add("ticket_number", "ABC12345");
            results.Add("date_of_ticket", "1/01/2011");
            results.Add("time_of_ticket", "6:55");
            results.Add("reg_number", "RTA 151G");
            results.Add("council", "Milton Keynes");
            results.Add("grounds", "hire");
            results.Add("renter_name", "Horatio T Poopalot");
            results.Add("renter_address", "27 chafron way");
            results.Add("renter_postcode", "MK1 1gb");
            results.Add("date_of_hire", "06/01/1967");
            stream.Position = 0;
            p = w.Parse(stream, results) as MemoryStream;
            using (FileStream file = new FileStream("result_hire.docx", FileMode.Create, FileAccess.Write))
            {
                p.CopyTo(file);
                p.Close();
            }

            results = new Dictionary<string, string>();           
            results.Add("name", "Alfred George Bloggs");
            results.Add("email", "andy@scientio.com");
            results.Add("ticket_number", "ABC12345");
            results.Add("date_of_ticket", "1/01/2011");
            results.Add("time_of_ticket", "6:55");
            results.Add("reg_number", "RTA 151G");
            results.Add("council", "Milton Keynes");
            results.Add("grounds", "sign");
            results.Add("summary_of_offence", "Lack of political correctness");
            results.Add("signage_fault", "contradictory");
            results.Add("sign_1_meaning", "Don't park here oik");
            results.Add("sign_2_meaning", "Please park here, my good sir");
            results.Add("sign_relation", "above");
            results.Add("street_name", "Great Poop St.");
            stream.Position = 0;
            p = w.Parse(stream, results) as MemoryStream;
            using (FileStream file = new FileStream("result_sign_contradictory.docx", FileMode.Create, FileAccess.Write))
            {
                p.CopyTo(file);
                p.Close();
            }

            results = new Dictionary<string, string>();
            
            results.Add("name", "Alfred George Bloggs");
            results.Add("email", "andy@scientio.com");
            results.Add("ticket_number", "ABC12345");
            results.Add("date_of_ticket", "1/01/2011");
            results.Add("time_of_ticket", "6:55");
            results.Add("reg_number", "RTA 151G");
            results.Add("council", "Milton Keynes");
            results.Add("grounds", "sign");
            results.Add("summary_of_offence", "Lack of political correctness");
            results.Add("signage_fault", "absent_unclear");
            results.Add("street_name", "Great Poop St.");
            stream.Position = 0;
            p = w.Parse(stream, results) as MemoryStream;
            using (FileStream file = new FileStream("result_sign_absent.docx", FileMode.Create, FileAccess.Write))
            {
                p.CopyTo(file);
                p.Close();
            }
        }

        [TestMethod]
        [Ignore]
        public async Task TestMSWithWord()
        {
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl_standard_core.test.modernslavery.darl"));
            var source = reader.ReadToEnd();
            var wordsource = Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl_standard_core.test.modernslaverytest.docx");
            var results = new List<DarlResult>();
            results.Add(new DarlResult("mspolicyexists", "yes"));
            results.Add(new DarlResult("mspolicysteps", "yes"));
            results.Add(new DarlResult("mssupchainexists", "yes"));
            results.Add(new DarlResult("mssupchainsteps", "yes"));
            results.Add(new DarlResult("email", "andy@scientio.com", DarlResult.DataType.textual));
            results.Add(new DarlResult("part_of_group", "yes"));
            results.Add(new DarlResult("financial_year_end", "06/11/2016", DarlResult.DataType.textual));
            results.Add(new DarlResult("signer", "Arbuthnot Peregrine Poops", DarlResult.DataType.textual));
            results.Add(new DarlResult("signer_role", "Director", DarlResult.DataType.textual));
            results.Add(new DarlResult("organization_name", "Eastern European Sex Slaves PLC", DarlResult.DataType.textual));
            results.Add(new DarlResult("low_risk", "yes"));
            DarlRunTime runtime = new DarlRunTime();
            await runtime.Evaluate(results, source);
            Assert.AreEqual("intro_section, C", results.First(a => a.name == "intro_section").ToString());
            var w = new WordProcessGem();
            var res = new Dictionary<string, string>();
            foreach(var k in results)
            {
                res.Add(k.name, k.ToString());
            }
            wordsource.Position = 0;
            var p = w.Parse(wordsource, res) as MemoryStream;
            using (FileStream file = new FileStream("modernslavery_lowrisk.docx", FileMode.Create, FileAccess.Write))
            {
                p.CopyTo(file);
                p.Close();
            }
            results = new List<DarlResult>();
            results.Add(new DarlResult("mspolicyexists", "no"));
            results.Add(new DarlResult("mspolicysteps", "no"));
            results.Add(new DarlResult("mssupchainexists", "no"));
            results.Add(new DarlResult("mssupchainsteps", "no"));
            results.Add(new DarlResult("part_of_group", "no"));
            results.Add(new DarlResult("email", "andy@scientio.com", DarlResult.DataType.textual));
            results.Add(new DarlResult("financial_year_end", "06/11/2016", DarlResult.DataType.textual));
            results.Add(new DarlResult("signer","Arbuthnot Peregrine Poops", DarlResult.DataType.textual));
            results.Add(new DarlResult("signer_role", "Director", DarlResult.DataType.textual));
            results.Add(new DarlResult("organization_name", "Eastern European Sex Slaves PLC", DarlResult.DataType.textual));
            results.Add(new DarlResult("low_risk","no"));
            await runtime.Evaluate(results, source);
            Assert.AreEqual("intro_section, D", results.First(a => a.name == "intro_section").ToString());
            res = new Dictionary<string, string>();
            foreach (var k in results)
            {
                res.Add(k.name, k.ToString());
            }
            wordsource.Position = 0;
            p = w.Parse(wordsource, res) as MemoryStream;
            using (FileStream file = new FileStream("modernslavery_nopolicies.docx", FileMode.Create, FileAccess.Write))
            {
                p.CopyTo(file);
                p.Close();
            }
            results = new List<DarlResult>();
            results.Add(new DarlResult("mspolicyexists", "yes"));
            results.Add(new DarlResult("mspolicysteps", "no"));
            results.Add(new DarlResult("mssupchainexists", "yes"));
            results.Add(new DarlResult("mssupchainsteps", "no"));
            results.Add(new DarlResult("part_of_group", "no"));
            results.Add(new DarlResult("email", "andy@scientio.com", DarlResult.DataType.textual));
            results.Add(new DarlResult("financial_year_end", "06/11/2016", DarlResult.DataType.textual));
            results.Add(new DarlResult("signer","Arbuthnot Peregrine Poops", DarlResult.DataType.textual));
            results.Add(new DarlResult("signer_role", "Director", DarlResult.DataType.textual));
            results.Add(new DarlResult("organization_name", "Eastern European Sex Slaves PLC", DarlResult.DataType.textual));
            results.Add(new DarlResult("low_risk","no"));
            await runtime.Evaluate(results, source);
            Assert.AreEqual("intro_section, B2", results.First(a => a.name == "intro_section").ToString());
            res = new Dictionary<string, string>();
            foreach (var k in results)
            {
                res.Add(k.name, k.ToString());
            }
            wordsource.Position = 0;
            p = w.Parse(wordsource, res) as MemoryStream;
            using (FileStream file = new FileStream("modernslavery_policy_andsup_nosteps.docx", FileMode.Create, FileAccess.Write))
            {
                p.CopyTo(file);
                p.Close();
            }
            results = new List<DarlResult>();
            results.Add(new DarlResult("mspolicyexists", "yes"));
            results.Add(new DarlResult("mspolicysteps", "yes"));
            results.Add(new DarlResult("mssupchainexists", "yes"));
            results.Add(new DarlResult("mssupchainsteps", "yes"));
            results.Add(new DarlResult("part_of_group", "no"));
            results.Add(new DarlResult("email", "andy@scientio.com", DarlResult.DataType.textual));
            results.Add(new DarlResult("financial_year_end", "06/11/2016", DarlResult.DataType.textual));
            results.Add(new DarlResult("signer","Arbuthnot Peregrine Poops", DarlResult.DataType.textual));
            results.Add(new DarlResult("signer_role", "Director", DarlResult.DataType.textual));
            results.Add(new DarlResult("organization_name", "Eastern European Sex Slaves PLC", DarlResult.DataType.textual));
            results.Add(new DarlResult("low_risk","no"));
            results.Add(new DarlResult("policystartknown", "no"));
            await runtime.Evaluate(results, source);
            Assert.AreEqual("intro_section, E", results.First(a => a.name == "intro_section").ToString());
            res = new Dictionary<string, string>();
            foreach (var k in results)
            {
                res.Add(k.name, k.ToString());
            }
            wordsource.Position = 0;
            p = w.Parse(wordsource, res) as MemoryStream;
            using (FileStream file = new FileStream("modernslavery_policy_andsup_steps.docx", FileMode.Create, FileAccess.Write))
            {
                p.CopyTo(file);
                p.Close();
            }
        }

        [TestMethod]
        [Ignore]
        public async Task TestAITriage()
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl_standard_core.test.ai_triage.docx");
            var w = new WordProcessGem();
            var results = new Dictionary<string, string>();
            results.Add("name", "Alfred George Bloggs");
            results.Add("type", "analytic");
            var p = w.Parse(stream, results) as MemoryStream;
            using (FileStream file = new FileStream("ai_triage_analytic.docx", FileMode.Create, FileAccess.Write))
            {
                p.CopyTo(file);
                p.Close();
            }
            stream.Position = 0;
            results = new Dictionary<string, string>();
            results.Add("name", "Alfred George Bloggs");
            results.Add("type", "critic");
            p = w.Parse(stream, results) as MemoryStream;
            using (FileStream file = new FileStream("ai_triage_critic.docx", FileMode.Create, FileAccess.Write))
            {
                p.CopyTo(file);
                p.Close();
            }

        }
    }
}
