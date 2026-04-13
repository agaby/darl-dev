/// <summary>
/// SmokeTest.cs - Core module for the Darl.dev project.
/// </summary>

using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Threading.Tasks;

namespace Darl.GraphQL.UI.Tests
{
    [TestClass]
    public class SmokeTest
    {
        static OpenQA.Selenium.Chrome.ChromeDriver driver;

        [TestInitialize()]
        public void SmokeTestInitialize()
        {
            driver = new OpenQA.Selenium.Chrome.ChromeDriver
            {
                Url = "https://darlgraphql-stagng.azurewebsites.net/" //"https://darl.dev/"
            };
        }

        [TestCleanup()]
        public void SmokeTestCleanup()
        {
            driver.Dispose();
        }

        /// <summary>
        /// Opens the site, opens a public kg and runs a conversation on it.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task TestPresence()
        {
            Assert.IsTrue(driver.Title == "ThinkBase Portal");
            await Task.Delay(1000);
            var popupButton = driver.FindElement(By.ClassName("messagebox_button_done")); //just one at this stage
            popupButton.Click();
            await Task.Delay(1000);
            var dropdown = driver.FindElement(By.Id("kgdemo-dropdown"));
            var select = new SelectElement(dropdown);
            select.SelectByText("ai_triage.graph");
            await Task.Delay(1000);
            var popups = driver.FindElements(By.ClassName("messagebox_button_done"));
            foreach (var p in popups)
            {
                p.Click();
            }
            var fit = driver.FindElement(By.Id("real-fit"));
            Assert.IsTrue(fit.Displayed);
            driver.FindElement(By.Id("conversation-tab")).Click();
            await Task.Delay(1000);
            var msgInput = driver.FindElement(By.Id("msg_input"));
            msgInput.Clear();
            msgInput.SendKeys("machine learning");
            var msgSend = driver.FindElement(By.Id("msg_send_btn"));
            msgSend.Click();
            await Task.Delay(2000);
            msgInput.Clear();
            msgInput.SendKeys("Yes");
            msgSend.Click();
            await Task.Delay(1000);
            msgInput.Clear();
            msgInput.SendKeys("Yes");
            msgSend.Click();
            await Task.Delay(1000);
            msgInput.Clear();
            msgInput.SendKeys("Yes");
            msgSend.Click();
            await Task.Delay(1000);
            msgInput.Clear();
            msgInput.SendKeys("vectors of numbers");
            msgSend.Click();
            await Task.Delay(1000);
            msgInput.Clear();
            msgInput.SendKeys("input and expected output");
            msgSend.Click();
            await Task.Delay(1000);
            msgInput.Clear();
            msgInput.SendKeys("input and expected output");
            msgSend.Click();
            await Task.Delay(1000);
            msgInput.Clear();
            msgInput.SendKeys("Yes");
            msgSend.Click();
            await Task.Delay(1000);
            driver.FindElement(By.XPath(@"//*[contains(text(),'The problem you have described may be amenable to soft')]"));
        }

        /// <summary>
        /// Log in, access Stripe page, access files, get api key, log out
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task TestAuthorization()
        {

        }

        [TestMethod]
        public async Task TestSubscribe()
        {

        }


    }
}
