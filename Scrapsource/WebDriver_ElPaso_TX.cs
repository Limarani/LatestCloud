using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Drawing.Imaging;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Threading;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Drawing;
using System.Data;
using HtmlAgilityPack;
using iTextSharp.text;
using System.Text.RegularExpressions;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Support.Extensions;
using System.Net;
using System.Collections.ObjectModel;

namespace ScrapMaricopa.Scrapsource
{

    public class WebDriver_ElPaso_TX
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();
        IWebElement Yearclick;
        public string FTP_ElPaso_TX(string streetno, string direction, string streetname, string streettype, string unitnumber, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            //var driverService = PhantomJSDriverService.CreateDefaultService();
            //driverService.HideCommandPromptWindow = true;
            //driver = new ChromeDriver();
            //driver = new PhantomJSDriver();
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string address = "", yearBuilt = "", Parcel_number = "", Yearhref = "";
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("http://www.epcad.org/Search");
                    int currentyear = DateTime.Now.Year;
                    if (searchType == "titleflex")
                    {
                        string straddress = "";
                        if (direction != "")
                        {
                            straddress = streetno + " " + direction + " " + streetname + " " + streettype + " " + unitnumber;
                        }
                        else
                        {
                            straddress = streetno + " " + streetname + " " + streettype + " " + unitnumber;
                        }
                        gc.TitleFlexSearch(orderNumber, parcelNumber, ownername, straddress, "TX", "El Paso");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["ElPaso_Zero"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    IWebElement propertyyeartable = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div/div/div[1]/div/form/div[1]/div/ul"));
                    IList<IWebElement> propertyyearrow = propertyyeartable.FindElements(By.TagName("li"));
                    IList<IWebElement> propertyyearid;
                    foreach (IWebElement propertyyearclick in propertyyearrow)
                    {
                        propertyyearid = propertyyearclick.FindElements(By.TagName("a"));
                        if (propertyyearid.Count != 0)
                        {
                            string yearclick = propertyyearid[0].GetAttribute("href");
                            Yearhref = GlobalClass.After(yearclick, "Year=");
                            if (Yearhref == Convert.ToString(currentyear))
                            {
                                driver.Navigate().GoToUrl(yearclick);
                                break;
                            }

                        }
                    }
                    if (searchType == "address")
                    {
                        if (direction.Trim() != "" && streettype.Trim() != "")
                        {
                            address = streetno + " " + direction.Trim() + " " + streetname.Trim() + " " + streettype.Trim();
                        }
                        if (direction.Trim() != "" && streettype.Trim() == "")
                        {
                            address = streetno + " " + direction.Trim() + " " + streetname.Trim();
                        }
                        if (streettype.Trim() != "" && direction.Trim() == "")
                        {
                            address = streetno + " " + streetname.Trim() + " " + streettype.Trim();
                        }
                        if (streettype.Trim() == "" && direction.Trim() == "")
                        {
                            address = streetno + " " + streetname.Trim();
                        }
                        driver.FindElement(By.Id("Keywords")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "Adderess before search", driver, "TX", "El Paso");
                        driver.FindElement(By.XPath("//*[@id='remote']/span[2]/button")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Adderess After search", driver, "TX", "El Paso");
                        IWebElement pagecountst = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div/div/div[2]/p[1]"));
                        string pagecount = gc.Between(pagecountst.Text, "Found", "results").Trim();
                        if (pagecount == "1")
                        {
                            IWebElement detailclick = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div/div/table/tbody/tr/td/div/div[4]/div[1]/div/a"));
                            string href = detailclick.GetAttribute("href");
                            driver.Navigate().GoToUrl(href);
                        }
                        if (pagecount != "1"&& pagecount != "0")
                        {
                            try
                            {
                                int max = 0;
                                gc.CreatePdf_WOP(orderNumber, "MultyAddressSearch", driver, "TX", "El Paso");
                                //IWebElement Multiparceltable = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div/div/table/tbody"));
                                //IList<IWebElement> Multiparcelrow = Multiparceltable.FindElements(By.TagName("tr"));
                                for (int i = 1; i <= Convert.ToInt16(pagecount); i++)
                                {
                                    string multiparceltd = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div/div/table/tbody/tr[" + i + "]")).Text;
                                    string Type = gc.Between(multiparceltd, "Type", "Property ID:").Trim();
                                    string PropertIdMulti = gc.Between(multiparceltd, "Property ID:", "Geographic ID:").Trim();
                                    string Name = gc.Between(multiparceltd, "Name:", "Address:").Trim();
                                    string AddressMulti = gc.Between(multiparceltd, "Name:", "Appraised Value:").Trim();
                                    string Multiaddress = Type + "~" + Name + "~" + AddressMulti;
                                    gc.insert_date(orderNumber, PropertIdMulti, 1149, Multiaddress, 1, DateTime.Now);
                                    max++;
                                }
                                if (max > 1 && max < 26)
                                {
                                    HttpContext.Current.Session["multiparcel_ElPaso"] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                                if (max < 25)
                                {
                                    HttpContext.Current.Session["multiParcel_ElPaso_Multicount"] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }

                            }
                            catch { } 
                        }
                        if (pagecount == "0")
                        {
                            HttpContext.Current.Session["ElPaso_Zero"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.Id("Keywords")).SendKeys(parcelNumber);
                        gc.CreatePdf_WOP(orderNumber, "Adderess Parcel search", driver, "TX", "El Paso");
                        driver.FindElement(By.XPath("//*[@id='remote']/span[2]/button")).Click();
                        gc.CreatePdf_WOP(orderNumber, "Adderess Parcel search", driver, "TX", "El Paso");
                        Thread.Sleep(2000);
                        IWebElement pagecountst = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div/div/div[2]/p[1]"));
                        string pagecount = gc.Between(pagecountst.Text, "Found", "results").Trim();
                        if (pagecount == "1")
                        {
                            IWebElement detailclick = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div/div/table/tbody/tr/td/div/div[4]/div[1]/div/a"));
                            string href = detailclick.GetAttribute("href");
                            driver.Navigate().GoToUrl(href);
                        }
                    }
                    if (searchType == "unitnumber")
                    {
                        driver.FindElement(By.Id("Keywords")).SendKeys(unitnumber);
                        gc.CreatePdf_WOP(orderNumber, "Adderess Unit search", driver, "TX", "El Paso");
                        driver.FindElement(By.XPath("//*[@id='remote']/span[2]/button")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Adderess Unit search", driver, "TX", "El Paso");
                        IWebElement pagecountst = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div/div/div[2]/p[1]"));
                        string pagecount = gc.Between(pagecountst.Text, "Found", "results").Trim();
                        if (pagecount == "1")
                        {
                            IWebElement detailclick = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div/div/table/tbody/tr/td/div/div[4]/div[1]/div/a"));
                            string href = detailclick.GetAttribute("href");
                            driver.Navigate().GoToUrl(href);
                        }
                    }
                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("Keywords")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "Ownername before click", driver, "TX", "El Paso");
                        driver.FindElement(By.XPath("//*[@id='remote']/span[2]/button")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Ownername After click", driver, "TX", "El Paso");
                        IWebElement pagecountst = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div/div/div[2]/p[1]"));
                        string pagecount = gc.Between(pagecountst.Text, "Found", "results").Trim();
                        if (pagecount == "1")
                        {
                            IWebElement detailclick = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div/div/table/tbody/tr/td/div/div[4]/div[1]/div/a"));
                            string href = detailclick.GetAttribute("href");
                            driver.Navigate().GoToUrl(href);
                        }
                        if (pagecount != "1")
                        {
                            try
                            {
                                int max = 0;
                                for (int i = 1; i <= Convert.ToInt16(pagecount); i++)
                                {
                                    string multiparceltd = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div/div/table/tbody/tr[" + i + "]")).Text;
                                    string Type = gc.Between(multiparceltd, "Type", "Property ID:").Trim();
                                    string PropertIdMulti = gc.Between(multiparceltd, "Property ID:", "Geographic ID:").Trim();
                                    string Name = gc.Between(multiparceltd, "Name:", "Address:").Trim();
                                    string AddressMulti = gc.Between(multiparceltd, "Name:", "Appraised Value:").Trim();
                                    string Multiaddress = Type + "~" + Name + "~" + AddressMulti;
                                    gc.insert_date(orderNumber, PropertIdMulti, 1149, Multiaddress, 1, DateTime.Now);
                                    max++;
                                }
                                if (max > 1 && max < 26)
                                {
                                    HttpContext.Current.Session["multiparcel_ElPaso"] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                                if (max < 25)
                                {
                                    HttpContext.Current.Session["multiParcel_ElPaso_Multicount"] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }

                            }
                            catch { }
                        }
                    }

                    try
                    {
                        IWebElement Inodata = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div/div/div[3]"));
                        if(Inodata.Text.Contains("No more results"))
                        {
                            HttpContext.Current.Session["ElPaso_Zero"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    string propertytable = driver.FindElement(By.XPath("//*[@id='property']/div/div[2]/div")).Text.Replace("\r\n", "");
                    string type = gc.Between(propertytable, "AccountType:", "Property ID:");
                    Parcel_number = gc.Between(propertytable, "Property ID:", "Geographic ID:");
                    string PropertyUseCode = gc.Between(propertytable, "Agent Code:", "Legal Description:");
                    string GeographicID = gc.Between(propertytable, "Geographic ID:", "Agent Code:");
                    string LegalDescription = gc.Between(propertytable, "Legal Description:", "Property Use Code:");
                    string PropertyUse_Description = GlobalClass.After(propertytable, "Property Use Description:");
                    IWebElement Locationrow = driver.FindElement(By.XPath("//*[@id='property']/div/div[3]/div"));
                    string PropertyAddress = gc.Between(Locationrow.Text, "Address:", "Neighborhood:");
                    string MapID = GlobalClass.After(Locationrow.Text, "Map ID:").Trim();
                    IWebElement owenrnametable = driver.FindElement(By.XPath("//*[@id='property']/div/div[4]/div"));
                    string OwnerName = gc.Between(owenrnametable.Text, "Name:", "Mailing Address:").Trim();
                    string MailingAddress = gc.Between(owenrnametable.Text, "Mailing Address:", "Owner ID:");
                    string OwnerID = gc.Between(owenrnametable.Text, "Owner ID:", "Ownership (%):").Trim();
                    string OwnershipPercentage = gc.Between(owenrnametable.Text, "Ownership (%):", "Exemptions");
                    string Exemptions = GlobalClass.After(owenrnametable.Text, "Exemptions").Trim();
                    gc.CreatePdf(orderNumber, Parcel_number, "Property detail", driver, "TX", "El Paso");
                    IWebElement Improvementclicktable = driver.FindElement(By.Id("detail-tabs"));
                    IList<IWebElement> improvementclickrow = Improvementclicktable.FindElements(By.TagName("a"));
                    IList<IWebElement> improvementid;
                    foreach (IWebElement iprovementyear in improvementclickrow)
                    {
                        string strcheck = iprovementyear.GetAttribute("href");
                        if (strcheck.Contains("improvement"))
                        {
                            IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                            js1.ExecuteScript("arguments[0].click();", iprovementyear);
                            break;
                        }
                    }
                    Thread.Sleep(2000);
                    try
                    {
                        gc.CreatePdf(orderNumber, Parcel_number, "Year", driver, "TX", "El Paso");
                        yearBuilt = driver.FindElement(By.XPath("//*[@id='improvement']/div/div[2]/div[2]/div/table/tbody/tr[2]/td[5]")).Text;
                    }
                    catch { }
                    string propertresult = type + "~" + PropertyUseCode + "~" + GeographicID + "~" + LegalDescription + "~" + PropertyUse_Description + "~" + PropertyAddress + "~" + MapID + "~" + OwnerName + "~" + MailingAddress + "~" + OwnerID + "~" + OwnershipPercentage + "~" + Exemptions + "~" + yearBuilt;
                    gc.insert_date(orderNumber, Parcel_number, 1129, propertresult, 1, DateTime.Now);

                    IWebElement Improvementclicktable1 = driver.FindElement(By.Id("detail-tabs"));
                    IList<IWebElement> improvementclickrow1 = Improvementclicktable1.FindElements(By.TagName("a"));
                    IList<IWebElement> improvementid1;
                    foreach (IWebElement iprovementyear1 in improvementclickrow1)
                    {
                        string strcheck1 = iprovementyear1.GetAttribute("href");
                        if (strcheck1.Contains("values"))
                        {
                            //iprovementyear.SendKeys(Keys.Enter);
                            IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                            js1.ExecuteScript("arguments[0].click();", iprovementyear1);
                            break;
                        }

                    }
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "values", driver, "TX", "El Paso");
                    string Novalues = "";
                    try
                    {
                        Novalues = driver.FindElement(By.XPath("//*[@id='values']/div/div[2]/div/div")).Text;
                    }
                    catch { }
                    if (Novalues == "")
                    {
                        string valuestab = driver.FindElement(By.XPath("//*[@id='values']/div/div[2]")).Text.Replace("\r\n", "");
                        string ImprovementHomesiteValue = gc.Between(valuestab, "Improvement Homesite Value:+", "(+) Improvement Non Homesite Value").Trim();
                        string ImprovementNon = gc.Between(valuestab, "(+) Improvement Non Homesite Value:+", "(+) Land Homesite Value");
                        string LandHomesiteValue = gc.Between(valuestab, "(+) Land Homesite Value:+", "(+) Land Non Homesite Value");
                        string LandNonHomesiteValue = gc.Between(valuestab, " Land Non Homesite Value:+", "(+) Agricultural Market");
                        string AgriculturalMarketValuation = gc.Between(valuestab, "Agricultural Market Valuation:+", "(+) Timber Market Valuation:");
                        string TimberMarketValuation = GlobalClass.After(valuestab, "Timber Market Valuation:+").Trim();
                        string Marketvaluetable = driver.FindElement(By.XPath("//*[@id='values']/div/div[3]")).Text.Replace("\r\n", "");
                        string MarketValue = gc.Between(Marketvaluetable, "Market Value:=", "(-) Agricultural Or");
                        string AgriculturalTimberReduction = GlobalClass.After(Marketvaluetable, "Value Reduction:-");
                        string appricealvaluetable = driver.FindElement(By.XPath("//*[@id='values']/div/div[4]")).Text.Replace("\r\n", "");
                        string AppraisedValue = gc.Between(appricealvaluetable, "Appraised Value:=", "(-) HS Cap:");
                        string HSCap = GlobalClass.After(appricealvaluetable, "HS Cap:-");
                        string Assessmenttable = driver.FindElement(By.XPath("//*[@id='values']/div/div[5]")).Text.Replace("\r\n", "");
                        string AssessedValue = GlobalClass.After(Assessmenttable, "(=) Assessed Value:=").Trim();
                        string Assessmentresult = Yearhref + "~" + ImprovementHomesiteValue + "~" + ImprovementNon + "~" + LandHomesiteValue + "~" + LandNonHomesiteValue + "~" + AgriculturalMarketValuation + "~" + TimberMarketValuation + "~" + MarketValue + "~" + AgriculturalTimberReduction + "~" + AppraisedValue + "~" + HSCap + "~" + AssessedValue;
                        gc.insert_date(orderNumber, Parcel_number, 1130, Assessmentresult, 1, DateTime.Now);
                    }
                    //Values
                    if (Novalues.Contains("No values are currently available"))
                    {
                        currentyear--;
                        IWebElement propertyyeartable1 = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div/div[1]/div/ul"));
                        IList<IWebElement> propertyyearrow1 = propertyyeartable1.FindElements(By.TagName("li"));
                        IList<IWebElement> propertyyearid1;
                        foreach (IWebElement propertyyearclick1 in propertyyearrow1)
                        {
                            propertyyearid1 = propertyyearclick1.FindElements(By.TagName("a"));
                            if (propertyyearid1.Count != 0)
                            {
                                string yearclick = propertyyearid1[0].GetAttribute("href");
                                Yearhref = GlobalClass.After(yearclick, "/");
                                if (Yearhref == Convert.ToString(currentyear))
                                {
                                    driver.Navigate().GoToUrl(yearclick);
                                    break;
                                }

                            }
                        }
                        IWebElement Improvementclicktable11 = driver.FindElement(By.Id("detail-tabs"));
                        IList<IWebElement> improvementclickrow11 = Improvementclicktable11.FindElements(By.TagName("a"));
                        IList<IWebElement> improvementid11;
                        foreach (IWebElement iprovementyear11 in improvementclickrow11)
                        {
                            string strcheck1 = iprovementyear11.GetAttribute("href");
                            if (strcheck1.Contains("values"))
                            {
                                //iprovementyear.SendKeys(Keys.Enter);
                                IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                                js1.ExecuteScript("arguments[0].click();", iprovementyear11);
                                break;
                            }

                        }
                        Thread.Sleep(3000);
                        string valuestab = driver.FindElement(By.XPath("//*[@id='values']/div/div[2]")).Text.Replace("\r\n", "");
                        string ImprovementHomesiteValue = gc.Between(valuestab, "Improvement Homesite Value:+", "(+) Improvement Non Homesite Value").Trim();
                        string ImprovementNon = gc.Between(valuestab, "(+) Improvement Non Homesite Value:+", "(+) Land Homesite Value");
                        string LandHomesiteValue = gc.Between(valuestab, "(+) Land Homesite Value:+", "(+) Land Non Homesite Value");
                        string LandNonHomesiteValue = gc.Between(valuestab, " Land Non Homesite Value:+", "(+) Agricultural Market");
                        string AgriculturalMarketValuation = gc.Between(valuestab, "Agricultural Market Valuation:+", "(+) Timber Market Valuation:");
                        string TimberMarketValuation = GlobalClass.After(valuestab, "Timber Market Valuation:+").Trim();
                        string Marketvaluetable = driver.FindElement(By.XPath("//*[@id='values']/div/div[3]")).Text.Replace("\r\n", "");
                        string MarketValue = gc.Between(Marketvaluetable, "Market Value:=", "(-) Agricultural Or");
                        string AgriculturalTimberReduction = GlobalClass.After(Marketvaluetable, "Value Reduction:-");
                        string appricealvaluetable = driver.FindElement(By.XPath("//*[@id='values']/div/div[4]")).Text.Replace("\r\n", "");
                        string AppraisedValue = gc.Between(appricealvaluetable, "Appraised Value:=", "(-) HS Cap:");
                        string HSCap = GlobalClass.After(appricealvaluetable, "HS Cap:-");
                        string Assessmenttable = driver.FindElement(By.XPath("//*[@id='values']/div/div[5]")).Text.Replace("\r\n", "");
                        string AssessedValue = GlobalClass.After(Assessmenttable, "(=) Assessed Value:=").Trim();
                        string Assessmentresult = Yearhref + "~" + ImprovementHomesiteValue + "~" + ImprovementNon + "~" + LandHomesiteValue + "~" + LandNonHomesiteValue + "~" + AgriculturalMarketValuation + "~" + TimberMarketValuation + "~" + MarketValue + "~" + AgriculturalTimberReduction + "~" + AppraisedValue + "~" + HSCap + "~" + AssessedValue;
                        gc.insert_date(orderNumber, Parcel_number, 1130, Assessmentresult, 1, DateTime.Now);
                    }
                    //jurisdiction
                    IWebElement Improvementclicktable2 = driver.FindElement(By.Id("detail-tabs"));
                    IList<IWebElement> improvementclickrow2 = Improvementclicktable2.FindElements(By.TagName("a"));
                    IList<IWebElement> improvementid2;
                    foreach (IWebElement iprovementyear2 in improvementclickrow2)
                    {
                        string strcheck2 = iprovementyear2.GetAttribute("href");
                        if (strcheck2.Contains("tax-jurisdiction"))
                        {
                            //iprovementyear.SendKeys(Keys.Enter);
                            IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                            js1.ExecuteScript("arguments[0].click();", iprovementyear2);
                            break;
                        }

                    }
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, Parcel_number, "tax jurisdiction", driver, "TX", "El Paso");
                    IWebElement Taxingjurisdictiontable = driver.FindElement(By.XPath("//*[@id='tax-jurisdiction']/div/div[2]/div[2]/table/tbody"));
                    IList<IWebElement> Taxingjurisdictionrow = Taxingjurisdictiontable.FindElements(By.TagName("tr"));
                    IList<IWebElement> Taxingid;
                    foreach (IWebElement taxingjuris in Taxingjurisdictionrow)
                    {
                        Taxingid = taxingjuris.FindElements(By.TagName("td"));
                        if (Taxingid.Count != 0 && taxingjuris.Text.Trim() != "")
                        {
                            string Taxingresult = Taxingid[0].Text + "~" + Taxingid[1].Text + "~" + Taxingid[2].Text + "~" + Taxingid[3].Text + "~" + Taxingid[4].Text + "~" + Taxingid[5].Text + "~" + Taxingid[6].Text;
                            gc.insert_date(orderNumber, Parcel_number, 1131, Taxingresult, 1, DateTime.Now);
                        }
                    }
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    //tax Site
                    driver.Navigate().GoToUrl("https://actweb.acttax.com/act_webdev/elpaso/index.jsp");
                    Thread.Sleep(3000);
                    IWebElement PropertyInformation = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/div[2]/table/tbody/tr/td/center/form/table/tbody/tr[3]/td[2]/div[3]/select"));
                    SelectElement PropertyInformationSelect = new SelectElement(driver.FindElement(By.Name("searchby")));
                    PropertyInformationSelect.SelectByValue("4");
                    Thread.Sleep(2000);
                    driver.FindElement(By.Id("criteria")).SendKeys(GeographicID.Trim());
                    gc.CreatePdf(orderNumber, Parcel_number, "tax search", driver, "TX", "El Paso");
                    driver.FindElement(By.Name("submit")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "tax search result", driver, "TX", "El Paso");

                    driver.FindElement(By.XPath("//*[@id='data-block']/table/tbody/tr/td/table/tbody/tr/td[2]/h3/a")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "tax search result details", driver, "TX", "El Paso");
                    //Jurisdiction Information Details Table: 
                    string year = "", account = "", ExemptionsJ = "";


                    driver.FindElement(By.LinkText("Taxes Due Detail by Year and Jurisdiction")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Taxes Due Detail by Year and Jurisdiction", driver, "TX", "El Paso");
                    //Taxes Due Detail by Year Details Table: 
                    //Account No~Appr. Dist. No~Active Lawsuits~Year~Base Tax Due~Penalty, Interest, and ACC* Due(end of July )~Total Due July~Penalty, Interest, and ACC* Due(end of August )~Total Due August~Penalty, Interest, and ACC*Due(end of September)~Total Due September
                    string accountno = "", distno = "", ActiveLawsuits = "";
                    IWebElement multitableElement31 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/center/table/tbody/tr/td/table/tbody"));
                    IList<IWebElement> multitableRow31 = multitableElement31.FindElements(By.TagName("tr"));
                    IList<IWebElement> multirowTD31;
                    foreach (IWebElement row in multitableRow31)
                    {
                        multirowTD31 = row.FindElements(By.TagName("td"));
                        if (multirowTD31.Count == 8 && !row.Text.Contains("Base Tax Due"))
                        {
                            string TaxesDue = multirowTD31[0].Text.Trim() + "~" + multirowTD31[1].Text.Trim() + "~" + multirowTD31[2].Text.Trim() + "~" + multirowTD31[3].Text.Trim() + "~" + multirowTD31[4].Text.Trim() + "~" + multirowTD31[5].Text.Trim() + "~" + multirowTD31[6].Text.Trim() + "~" + multirowTD31[7].Text.Trim();
                            gc.insert_date(orderNumber, Parcel_number, 1134, TaxesDue, 1, DateTime.Now);
                        }
                        if (multirowTD31.Count == 1)
                        {
                            string TaxesDue = multirowTD31[0].Text.Trim() + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                            gc.insert_date(orderNumber, Parcel_number, 1134, TaxesDue, 1, DateTime.Now);

                        }
                        if (multirowTD31.Count == 4)
                        {
                            string TaxesDue = "" + "~" + "" + "~" + multirowTD31[1].Text.Trim() + "~" + "" + "~" + multirowTD31[2].Text.Trim() + "~" + "" + "~" + multirowTD31[3].Text.Trim() + "~" + "";
                            gc.insert_date(orderNumber, Parcel_number, 1134, TaxesDue, 1, DateTime.Now);

                        }
                    }

                    driver.FindElement(By.LinkText("Return to the Previous Page")).Click();
                    Thread.Sleep(2000);
                    //Tax Payment Details Table: 

                    driver.FindElement(By.LinkText("Payment Information & Receipts")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Payment Information", driver, "TX", "El Paso");
                    //Account Number~Paid Date~Amount~Tax Year~Description~Paid By
                    string accountnos = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/center/h3")).Text.Replace("Account No.:", "");
                    try
                    {
                        IWebElement multitableElement32 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/center/table/tbody"));
                        IList<IWebElement> multitableRow32 = multitableElement32.FindElements(By.TagName("tr"));
                        IList<IWebElement> multirowTD32;
                        foreach (IWebElement row in multitableRow32)
                        {
                            multirowTD32 = row.FindElements(By.TagName("td"));
                            if (multirowTD32.Count != 0 && !row.Text.Contains("Receipt Date"))
                            {
                                string TaxesDue = accountnos + "~" + multirowTD32[0].Text.Trim() + "~" + multirowTD32[1].Text.Trim() + "~" + multirowTD32[2].Text.Trim() + "~" + multirowTD32[3].Text.Trim() + "~" + multirowTD32[4].Text.Trim();
                                gc.insert_date(orderNumber, Parcel_number, 1137, TaxesDue, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }
                    driver.FindElement(By.LinkText("Return to the Previous Page")).Click();
                    Thread.Sleep(2000);


                    string jurd = "", pendingpayment = "", accountNo = "", AppraisalDistrict = "", OwnerNametax = "", OwnerAddress = "", PropertyAddresstax = "", legal = "", CurrentTax = "", CurrentAmount = "", PriorYearAmount = "", TotalAmount = "", LastPaymentAmount = "", LastPayer = "", LastPaymentDate = "", ActiveLawsuitsTax = "", GrossValue = "", LandValue = "", ImprovementValue = "", CappedValue = "", AgriculturalValue = "", ExemptionsTax = "";
                    string taxyeartax1 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td/center[1]/table/tbody/tr/td/h5/b")).Text;
                    string taxyear = gc.Between(taxyeartax1, "information for", ". All").Trim();
                    string fullTaxeBill1 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td/center[2]/table/tbody/tr/td[1]")).Text.Replace("\r\n", " ");
                    accountno = gc.Between(fullTaxeBill1, "Account Number:", "Prop. Id.");
                    string market = "";
                    OwnerAddress = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td/center[2]/table/tbody/tr/td[1]/h3[3]")).Text.Trim();
                    string[] Ownersplit = OwnerAddress.Split('\r');
                    OwnerNametax = Ownersplit[1].Replace("\n", "").Trim();
                    PropertyAddress = gc.Between(fullTaxeBill1, "Property Site Address:", " Legal Description:");
                    //legal = gc.Between(fullTaxeBill1, "Legal Description:", " Current Tax Levy:");
                    // CurrentTax = gc.Between(fullTaxeBill1, " Current Tax Levy:", "Current Amount Due:");
                    CurrentAmount = gc.Between(fullTaxeBill1, "Current Amount Due:", "Prior Year Amount Due:");
                    PriorYearAmount = gc.Between(fullTaxeBill1, "Prior Year Amount Due:", "Total Amount Due:");
                    TotalAmount = gc.Between(fullTaxeBill1, "Total Amount Due:", "Last Payment Amount:");
                    LastPaymentAmount = gc.Between(fullTaxeBill1, "Last Payment Amount:", "Last Payer:");
                    LastPayer = gc.Between(fullTaxeBill1, "Last Payer:", "Last Payment Date:");
                    LastPaymentDate = gc.Between(fullTaxeBill1, "Last Payment Date:", "Active Lawsuits:");
                    //  string tax_year = driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr/td/table/tbody/tr[1]/td/table[1]/tbody/tr/td/h5/b")).Text;
                    //  tax_year = gc.Between(tax_year, " tax information for", ". All amounts");
                    ActiveLawsuitsTax = GlobalClass.After(fullTaxeBill1, "Active Lawsuits:");
                    string fullTaxbill2 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td/center[2]/table/tbody/tr/td[2]")).Text.Replace("\r\n", " ");
                    pendingpayment = gc.Between(fullTaxbill2, " PendingCredit / PINless Debit Card or eCheckPayments:", "(Pay single or");
                    market = gc.Between(fullTaxbill2, "Market Value:", "Land Value:");
                    LandValue = gc.Between(fullTaxbill2, "Land Value:", "Improvement Value:");
                    ImprovementValue = gc.Between(fullTaxbill2, "Improvement Value:", "Capped Value:");
                    CappedValue = gc.Between(fullTaxbill2, "Capped Value:", "Agricultural Value:");
                    AgriculturalValue = gc.Between(fullTaxbill2, "Agricultural Value:", "Exemptions:");
                    ExemptionsTax = gc.Between(fullTaxbill2, "Exemptions:", "Exemption and Tax Rate");
                    // string lastCertified = gc.Between(fullTaxbill2, "Last Certified Date: ", "Taxes Due Detail by Year and Jurisdiction ");
                    //Account Number~Owner Information~Property Site Address~Legal Description~Current Tax Levy~Current Amount Due~Prior Year Amount Due~Total Amount Due~Last Payment Amount for Current Year Taxes~Active Lawsuits~market~Land Value~Improvement Value~Capped Value~Agricultural Value~Exemptions~Last Certified Date~Tax Authority
                    string Taxauthority = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[1]/td/h3/font/i/b")).Text;
                    string taxbill = taxyear + "~" + accountno + "~" + OwnerNametax + "~" + PropertyAddress + "~" + TotalAmount + "~" + CurrentAmount + "~" + PriorYearAmount + "~" + TotalAmount + "~" + LastPaymentAmount + "~" + LastPayer + "~" + LastPaymentDate + "~" + ActiveLawsuitsTax + "~" + pendingpayment + "~" + market + "~" + LandValue + "~" + ImprovementValue + "~" + CappedValue + "~" + AgriculturalValue + "~" + ExemptionsTax + "~" + Taxauthority;
                    gc.insert_date(orderNumber, Parcel_number, 1138, taxbill, 1, DateTime.Now);

                    IWebElement Itaxstmt1 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td/center[2]/table/tbody/tr/td[2]/h3[2]/a"));
                    Thread.Sleep(2000);
                    string stmt11 = Itaxstmt1.GetAttribute("href");
                    driver.Navigate().GoToUrl(stmt11);
                    Thread.Sleep(4000);
                    IWebElement Itaxstmt = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[3]/td/div/h3/a"));
                    string stmt1 = Itaxstmt.GetAttribute("href");
                    gc.downloadfile(stmt1, orderNumber, Parcel_number, "Tax statement", "TX", "El Paso");
                    Thread.Sleep(2000);
                    driver.Navigate().Back();
                    Thread.Sleep(2000);
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td/center[2]/table/tbody/tr/td[2]/h3[10]/a[1]")).Click();
                        Thread.Sleep(2000);
                        //Jurisdiction Information for ~Account No~Exemptions~Jurisdictions~Market Value~Exemption Value~Taxable Value~Tax Rate~Levy
                        string accountnosjur = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/center/table/tbody/tr[1]/td/h3[2]")).Text.Replace("Account No.:", "");
                        string jurdinfoYear = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/center/table/tbody/tr[1]/td/h3[1]/b")).Text.Replace("Jurisdiction Information for", "");
                        string exemptions = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/center/table/tbody/tr[1]/td/h3[3]")).Text.Replace("Exemptions:", "");
                        gc.CreatePdf(orderNumber, Parcel_number, "Jurisdiction Information", driver, "TX", "El Paso");
                        IWebElement multitableElement6 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/center/table/tbody/tr[2]/td/table/tbody"));
                        IList<IWebElement> multitableRow6 = multitableElement6.FindElements(By.TagName("tr"));
                        IList<IWebElement> multirowTD6;
                        foreach (IWebElement row in multitableRow6)
                        {
                            multirowTD6 = row.FindElements(By.TagName("td"));
                            if (multirowTD6.Count != 0 && !row.Text.Contains("Jurisdictions"))
                            {
                                string TaxesDue = jurdinfoYear + "~" + accountnosjur + "~" + exemptions + "~" + multirowTD6[0].Text.Trim() + "~" + multirowTD6[1].Text.Trim() + "~" + multirowTD6[2].Text.Trim() + "~" + multirowTD6[3].Text.Trim() + "~" + multirowTD6[4].Text.Trim() + "~" + multirowTD6[5].Text.Trim();
                                gc.insert_date(orderNumber, Parcel_number, 1139, TaxesDue, 1, DateTime.Now);
                            }
                        }
                        driver.FindElement(By.LinkText("Return to the Previous Page")).Click();
                        Thread.Sleep(2000);
                    }
                    catch { }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "TX", "El Paso", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();

                    gc.mergpdf(orderNumber, "TX", "El Paso");
                    return "Data Inserted Successfully";
                }
                catch (Exception ex)
                {
                    driver.Quit();
                    GlobalClass.LogError(ex, orderNumber);
                    throw ex;
                }
            }
        }
    }
}