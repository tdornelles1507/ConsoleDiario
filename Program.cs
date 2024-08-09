using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using Rectangle = System.Drawing.Rectangle;
using System;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using static System.Net.WebRequestMethods;

namespace ConsoleDiarioBH
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var userAgents = ListUserAgents();
            ChromeDriverService chromeDriverService = ChromeDriverService.CreateDefaultService();
            ChromeOptions chromeOptions = SetChromeOptions(userAgents);
            ChromeDriver driver = new ChromeDriver(chromeDriverService, chromeOptions, TimeSpan.FromMinutes(20));

            driver.Navigate().GoToUrl("https://dom-web.pbh.gov.br/");
            Thread.Sleep(SetSleepRandom(1500, 2000));
            driver.Manage().Window.Maximize();

            Thread.Sleep(7000);

            HtmlDocument htmlDocument1 = new HtmlDocument();
            htmlDocument1.LoadHtml(driver.PageSource);

            var links = htmlDocument1.DocumentNode.SelectNodes("//a[@class='link']");

            if (links != null)
            {
                List<string> linksSecretarias = new List<string>();
                List<string> linksAnexos = new List<string>();

                foreach (var link in links)
                {
                    // Obtenha o valor do atributo href de cada elemento <a> encontrado
                    string hrefValue = link.GetAttributeValue("href", "");
                    if (hrefValue != "#")
                    {
                        linksSecretarias.Add(hrefValue);
                    }
                }

                foreach (var link in linksSecretarias)
                {
                    driver.Navigate().GoToUrl($"https://dom-web.pbh.gov.br/{link}");
                    Thread.Sleep(SetSleepRandom(1500, 2000));

                    HtmlDocument htmlDocument2 = new HtmlDocument();
                    htmlDocument2.LoadHtml(driver.PageSource);

                    string teste = htmlDocument2.DocumentNode.InnerHtml;

                    var Anexo = $"https://dom-web.pbh.gov.br/{link}";

                    Thread.Sleep(5000);

                    if (teste.Contains("Visualizar Anexos"))
                    {
                        linksAnexos.Add(Anexo);
                    }
                    else
                    {
                        continue;
                    }

                    HtmlDocument htmlDocument3 = new HtmlDocument();
                    htmlDocument3.LoadHtml(driver.PageSource);

                    driver.FindElementByXPath("//*[@id=\"app\"]/div/div/div[2]/div[2]/div[2]/button").Click();

                    Thread.Sleep(SetSleepRandom(1500, 2000));

                    List<string> lxPath = new List<string>();

                    foreach (HtmlNode div in htmlDocument3.DocumentNode.SelectNodes("//a[@class='anexo']"))
                    {
                        if (div.HasClass("anexo"))
                        {
                            lxPath.Add(div.XPath);
                        }
                    }

                    Thread.Sleep(SetSleepRandom(1500, 2000));

                    foreach (var path in lxPath)
                    {
                        driver.FindElementByXPath(path).Click();
                        Thread.Sleep(SetSleepRandom(1500, 2000));
                    }
                }
            }

            else
            {
                Console.WriteLine("Não foram encontrados iframes na página.");
            }

            Console.WriteLine("Downloads finalizados com sucesso");

            Thread.Sleep(SetSleepRandom(5000, 10000));

            driver.Quit();
        }
        private static List<string> ListUserAgents()
        {
            var userAgents = new List<string>();
            var userAgentP = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/141.0.0.0 Safari/537.36";

            userAgents.Add(userAgentP);

            return userAgents;
        }
        static ChromeOptions SetChromeOptions(List<string> userAgents)
        {
            //ChromeOptions chromeOptions = new ChromeOptions();
            //chromeOptions.AddExcludedArgument("enable-automation");
            //chromeOptions.AddUserProfilePreference("useAutomationExtension", false);
            //chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
            //chromeOptions.AddUserProfilePreference("download.default_directory", @"C:\TEMPTOPS");
            //chromeOptions.AddArgument($"--user-agent='{ChangeUserAgents(userAgents)}'");
            //chromeOptions.AcceptInsecureCertificates = true;
            //chromeOptions.AddArgument("ignore-certificate-errors");
            ////chromeOptions.AddArguments("--incognito");
            //chromeOptions.AddArgument("--start-maximize");
            //chromeOptions.AddArgument("--disable-web-security");
            //chromeOptions.AddArgument("--profile-directory=Default");
            //chromeOptions.AddArgument("--whitelisted-ips");
            //chromeOptions.AddArgument("--disable-extensions");
            //chromeOptions.AddArgument("--disable-plugins-discovery");

            ////chromeOptions.AddArgument("--disable-gpu");
            ////chromeOptions.AddArgument("--use-gl=desktop");

            //var proxy = new Proxy();
            //proxy.Kind = ProxyKind.Manual;
            //proxy.IsAutoDetect = false;
            //proxy.HttpProxy = "138.219.71.97";
            ////proxy.SslProxy = "11.456.448.110:8080";
            ////chromeOptions.Proxy = proxy;

            //chromeOptions.PageLoadStrategy = PageLoadStrategy.Normal;

            //return chromeOptions;

            ChromeOptions options = new ChromeOptions();
            options.AddExcludedArgument("enable-automation");
            options.AddUserProfilePreference("useAutomationExtension", false);
            options.AddArgument("--disable-blink-features");
            options.AddArgument("--disable-blink-features=AutomationControlled");
            //options.AddArgument("--proxy-server=54.232.39.128:80");
            options.AddUserProfilePreference("download.default_directory", @"C:\TEMPTOPS");
            options.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
            options.AddUserProfilePreference("profile.default_content_setting_values.automatic_downloads", 1);
            options.AddArgument("--ignore-certificate-errors");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--disable-dev-shm-usage");

            //options.AddArgument($"--user-agent='{ChangeUserAgents(userAgents)}'");

            return options;
        }
        static int SetSleepRandom(int minInt, int maxInt)
        {
            Random random = new Random();
            return random.Next(minInt, maxInt);
        }
        public static IWebElement FindElementById(ChromeDriver driver, string Id)
        {
            return driver.FindElement(By.Id(Id));
        }
        public static ReadOnlyCollection<IWebElement> FindElementsByClassName(ChromeDriver driver, string ClassName)
        {
            return driver.FindElements(By.ClassName(ClassName));
        }
        public static string ExtrairBase64(string htmlContent)
        {
            string base64Pattern = "data:[^;]*;base64,([^\"]+)";
            Regex regex = new Regex(base64Pattern);
            Match match = regex.Match(htmlContent);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return null;
            }
        }
    }
}
