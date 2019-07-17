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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ScrapMaricopa.Scrapsource
{

    public class WebDriver_DorchesterSC
    {
        IWebElement PaidCount;
        IWebDriver driver;
        DBconnection db = new DBconnection();
        List<string> strTaxRealestate = new List<string>();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());


        public string FTP_Dorchester(string houseno, string sname, string direction, string type, string unitno, string parcelNumber, string searchType, string orderNumber, string ownername, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            List<string> taxinformation = new List<string>();
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            //IWebElement iframeElement1;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new ChromeDriver())//PhantomJSDriver
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("https://www.dorchestercountysc.gov/government/property-tax-services/assessor/real-estate-mobile-home-search/cama-parcel-lookup");
                    if (searchType == "titleflex")
                    {

                        string straddress = houseno + " " + sname + " " + direction + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, "", "", straddress, "SC", "Dorchester");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["NoRecord_SCDorchester"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    IWebElement iframeElement = driver.FindElement(By.XPath("//*[@id='widget_4_512_1816']/iframe"));

                    driver.SwitchTo().Frame(iframeElement);
                    string Year = DateTime.Now.Year.ToString();
                    int year1 = Convert.ToInt32(Year) - 1;
                    string year2 = year1.ToString();
                    driver.FindElement(By.Name("f_BUILDDATEID")).Clear();
                    driver.FindElement(By.Name("f_BUILDDATEID")).SendKeys(year2);
                    if (searchType == "address")
                    {
                        driver.FindElement(By.Name("f_STREETNO")).SendKeys(houseno);
                        driver.FindElement(By.XPath("//*[@id='contents']/form/table[2]/tbody/tr[7]/td[2]/input")).SendKeys(sname);
                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "SC", "Dorchester");
                        driver.FindElement(By.Id("filter-button")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.XPath("//*[@id='contents']/form/table[2]/tbody/tr[2]/td[2]/input")).SendKeys(parcelNumber);

                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "SC", "Dorchester");
                        driver.FindElement(By.XPath("//*[@id='filter-button']")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                    }
                    if (searchType == "block")
                    {

                        driver.FindElement(By.XPath("//*[@id='contents']/form/table[2]/tbody/tr[3]/td[2]/input")).SendKeys(unitno);
                        gc.CreatePdf_WOP(orderNumber, "account Search", driver, "SC", "Dorchester");

                        driver.FindElement(By.XPath("//*[@id='filter-button']")).SendKeys(Keys.Enter);
                        Thread.Sleep(4000);
                    }
                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.XPath("//*[@id='contents']/form/table[2]/tbody/tr[4]/td[2]/input")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "owner Search", driver, "SC", "Dorchester");

                        driver.FindElement(By.XPath("//*[@id='filter-button']")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                    }
                    //*[@id="contents"]/form

                    try
                    {
                        int trCount = driver.FindElements(By.XPath("//*[@id='contents']/table/tbody/tr")).Count;
                        if (trCount > 2)
                        {
                            IList<IWebElement> tables1 = driver.FindElements(By.XPath("//*[@id='contents']/table"));
                            int count1 = tables1.Count;
                            foreach (IWebElement tab in tables1)
                            {

                                if (tab.Text.Contains("Details"))
                                {
                                    IList<IWebElement> TRmulti5 = tab.FindElements(By.TagName("tr"));
                                    IList<IWebElement> TDmulti5;
                                    int maxCheck = 0;
                                    foreach (IWebElement row in TRmulti5)
                                    {
                                        if (maxCheck <= 25)
                                        {
                                            TDmulti5 = row.FindElements(By.TagName("td"));
                                            if (TDmulti5.Count != 0 && !row.Text.Contains("Details"))
                                            {
                                                string multi1 = TDmulti5[6].Text + " " + TDmulti5[7].Text + " " + TDmulti5[8].Text + "~" + TDmulti5[5].Text;
                                                gc.insert_date(orderNumber, TDmulti5[1].Text, 633, multi1, 1, DateTime.Now);
                                            }
                                            maxCheck++;
                                        }
                                    }
                                    if (TRmulti5.Count > 25)
                                    {
                                        HttpContext.Current.Session["multiParcel_Dorchester_Multicount"] = "Maximum";
                                    }
                                    else
                                    {
                                        HttpContext.Current.Session["multiparcel_Dorchester"] = "Yes";
                                    }
                                }
                            }


                            driver.Quit();
                            return "MultiParcel";
                        }
                        else
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//*[@id='list-table']/tbody/tr[2]/td[1]/span/a")).Click();
                                Thread.Sleep(2000);
                            }
                            catch { }
                        }
                    }
                    catch { }

                    try
                    {
                        IWebElement INorecord = driver.FindElement(By.XPath("//*[@id='list-table']"));
                        IList<IWebElement> Inodatarow = INorecord.FindElements(By.TagName("td"));
                        if (Inodatarow.Count <= 1)
                        {
                            HttpContext.Current.Session["NoRecord_SCDorchester"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }


                    //property details

                    string YearBuilt = "", PropertyType = "", owner_name = "", owneraddress = "", AccountNo = "", TMSNo = "", ParcelAddress = "", TotalLandImprovements = "", TaxDistrict = "", Subdivision = "";
                    string bulkpropertytext = driver.FindElement(By.XPath("//*[@id='page-content']/table[3]/tbody/tr/td/table[1]/tbody")).Text.Replace("\r\n", "").Replace("Owner Information", "");
                    owner_name = gc.Between(bulkpropertytext, "Owner", "Owner Address").Trim();
                    owneraddress = gc.Between(bulkpropertytext, "Owner Address", "Plat Book & Page").Trim();

                    IWebElement tbmulti = driver.FindElement(By.XPath("//*[@id='page-content']/table[2]/tbody"));
                    IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDmulti;
                    foreach (IWebElement row in TRmulti)
                    {

                        TDmulti = row.FindElements(By.TagName("td"));
                        if (TDmulti.Count == 4 && TDmulti[0].Text.Trim() != "" && !row.Text.Contains("Parcel Address"))
                        {
                            AccountNo = TDmulti[0].Text;
                            TMSNo = TDmulti[1].Text;
                            ParcelAddress = TDmulti[2].Text;
                            TotalLandImprovements = TDmulti[3].Text;
                        }

                    }
                    gc.CreatePdf(orderNumber, TMSNo, "property details1", driver, "SC", "Dorchester");
                    string bulkpropertytext1 = driver.FindElement(By.XPath("//*[@id='page-content']/table[3]/tbody/tr/td/table[2]/tbody/tr[2]/td[2]/table/tbody")).Text;

                    TaxDistrict = gc.Between(bulkpropertytext1, "Tax District", "Subdivision").Trim();
                    Subdivision = gc.Between(bulkpropertytext1, "Subdivision", "Section").Trim();

                    //improvements
                    driver.FindElement(By.XPath("//*[@id='detailbutton']")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, TMSNo, "improvements details", driver, "SC", "Dorchester");
                    //*[@id="list-table"]/tbody/tr[2]/td[3]
                    PropertyType = driver.FindElement(By.XPath("//*[@id='list-table']/tbody/tr[2]/td[3]")).Text;
                    YearBuilt = driver.FindElement(By.XPath("//*[@id='list-table']/tbody/tr[2]/td[6]")).Text;

                    string property_details = owner_name + "~" + owneraddress + "~" + AccountNo + "~" + TMSNo + "~" + ParcelAddress + "~" + TotalLandImprovements + "~" + TaxDistrict + "~" + Subdivision + "~" + PropertyType + "~" + YearBuilt;
                    gc.insert_date(orderNumber, TMSNo, 629, property_details, 1, DateTime.Now);
                    //Owner~Owner Address~Account#~TMS#~Parcel Address~Total Land & Improvements~Tax District~Subdivision~Property Type~Year Built

                    driver.FindElement(By.XPath("//*[@id='backbutton']/span")).Click();
                    Thread.Sleep(3000);

                    //lands
                    driver.FindElement(By.XPath("//*[@id='land']/span")).Click();
                    Thread.Sleep(4000);
                    gc.CreatePdf(orderNumber, TMSNo, "land details", driver, "SC", "Dorchester");
                    driver.FindElement(By.XPath(" //*[@id='backbutton']")).Click();
                    Thread.Sleep(3000);
                    //assessment
                    driver.FindElement(By.XPath("//*[@id='assmnt']/span")).Click();
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, TMSNo, "assessment details", driver, "SC", "Dorchester");
                    //TAXYEAR~ABST DESC~ACRES~ACTUAL VALUE~ASSM RATIO~CAP VALUE~TAXABLE VALUE~TAXABLE ASSESSED VALUE
                    IWebElement tbmulti1 = driver.FindElement(By.XPath("//*[@id='list-table']/tbody"));
                    IList<IWebElement> TRmulti1 = tbmulti1.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDmulti1;

                    foreach (IWebElement row in TRmulti1)
                    {

                        TDmulti1 = row.FindElements(By.TagName("td"));
                        if (TDmulti1.Count != 0)
                        {
                            string assessment_details = TDmulti1[0].Text + "~" + TDmulti1[1].Text + "~" + TDmulti1[2].Text + "~" + TDmulti1[3].Text + "~" + TDmulti1[4].Text + "~" + TDmulti1[5].Text + "~" + TDmulti1[6].Text + "~" + TDmulti1[7].Text;
                            gc.insert_date(orderNumber, TMSNo, 630, assessment_details, 1, DateTime.Now);

                        }

                    }
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    //Tax Details
                    driver.Navigate().GoToUrl("https://dorchestercountytaxesonline.com/taxes.html#/WildfireSearch");
                    Thread.Sleep(3000);
                    driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[3]/button[2]")).Click();
                    Thread.Sleep(2000);
                    driver.FindElement(By.XPath("//*[@id='searchBox']")).SendKeys(TMSNo);
                    gc.CreatePdf(orderNumber, TMSNo, "tax details", driver, "SC", "Dorchester");
                    try
                    {
                        //   gc.CreatePdf(orderNumber, parcel_no, "Tax iformation", driver, "SC", "Dorchester");
                        driver.FindElement(By.XPath("//*[@id='searchForm']/div[1]/div/span/button/i")).Click();

                    }
                    catch { }

                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, TMSNo, "taxinfo details", driver, "SC", "Dorchester");
                    //Tax Payment Details Table: 

                    //Owner Name~Year~Notice Number~Type~Paid~Paid Date
                    int i = 1;
                    IWebElement tbmulti11 = null;
                    try
                    {
                        tbmulti11 = driver.FindElement(By.XPath("//*[@id='avalon']/div/div[3]/div[2]/table/tbody"));
                    }
                    catch { }
                    try
                    {
                        tbmulti11 = driver.FindElement(By.XPath("//*[@id='avalon']/div/div[4]/div[2]/table/tbody"));
                    }
                    catch { }

                    IList<IWebElement> TRmulti11 = tbmulti11.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDmulti11;

                    foreach (IWebElement row in TRmulti11)
                    {

                        TDmulti11 = row.FindElements(By.TagName("td"));
                        if (TDmulti11.Count != 0)
                        {
                            if (TDmulti11[3].Text == "Real" && i == 1)
                            {
                                try
                                {
                                    PaidCount = driver.FindElement(By.XPath("//*[@id='avalon']/div/div[3]/div[2]/table/tbody/tr[" + i + "]/td[8]/button"));
                                }
                                catch { }
                                try
                                {
                                    PaidCount = driver.FindElement(By.XPath("//*[@id='avalon']/div/div[4]/div[2]/table/tbody/tr[" + i + "]/td[8]/button"));
                                }
                                catch { }
                                i++;
                            }

                            string tax_payment = TDmulti11[0].Text + "~" + TDmulti11[1].GetAttribute("innerText") + "~" + TDmulti11[2].Text + "~" + TDmulti11[3].GetAttribute("innerText") + "~" + TDmulti11[4].Text + "~" + TDmulti11[5].GetAttribute("innerText");
                            gc.insert_date(orderNumber, TMSNo, 631, tax_payment, 1, DateTime.Now);
                        }

                    }
                    PaidCount.Click();
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, TMSNo, "taxinfo1 details", driver, "SC", "Dorchester");

                    //tax info
                    //Owner &  Address~Map No.~Notice No.~Description~District~Appraised Value~Assessed Value~Status~Payment Date~Amount Paid~Record Type~Tax Year~Receipt~Base Taxes~Penalty~Total Due~Tax Authority
                    string Map = "", Notice = "", Description = "", District = "", AppraisedValue = "", AssessedValue = "", Status = "", PaymentDate = "", AmountPaid = "", RecordType = "", TaxYear = "", Receipt = "", BaseTaxes = "", Penalty = "", TotalDue = "", TaxAuthority = "";
                    string ownertext1 = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[1]/div[1]")).Text.Replace("\r\n", " ").Replace("Owner Information", "");
                    string bulktext1 = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[2]/div[1]")).Text.Replace("\r\n", " ");
                    Map = gc.Between(bulktext1, "Map No.", "Notice No.").Trim();
                    Notice = gc.Between(bulktext1, "Notice No.", "Description").Trim();
                    Description = gc.Between(bulktext1, "Description", "District").Trim();
                    District = gc.Between(bulktext1, "District", "Appraised Value").Trim();
                    AppraisedValue = gc.Between(bulktext1, "Appraised Value", "Assessed Value").Trim();
                    AssessedValue = GlobalClass.After(bulktext1, "Assessed Value").Trim();
                    string bulktext2 = driver.FindElement(By.XPath(" //*[@id='avalon']/div/div/div/div[1]/div[2]/div[2]")).Text.Replace("\r\n", " ");
                    RecordType = gc.Between(bulktext2, "Record Type", "Tax Year").Trim();
                    TaxYear = gc.Between(bulktext2, "Tax Year", "Receipt").Trim();
                    Receipt = GlobalClass.After(bulktext2, "Receipt").Trim();
                    string bulktext3 = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[2]/div[3]")).Text.Replace("\r\n", " ");
                    BaseTaxes = gc.Between(bulktext3, "Base Taxes", "Penalty").Trim();
                    Penalty = gc.Between(bulktext3, "Penalty", "Total Due").Trim();
                    TotalDue = GlobalClass.After(bulktext3, "Total Due").Trim();
                    string bulktext4 = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[1]/div[2]/table/tbody")).Text.Replace("\r\n", " ");
                    Status = gc.Between(bulktext4, "Status", "Payment Date").Trim();
                    PaymentDate = gc.Between(bulktext4, "Payment Date", "Amount Paid").Trim();
                    AmountPaid = GlobalClass.After(bulktext4, "Amount Paid").Trim();
                    TaxAuthority = "County of Dorchester Cindy Chitty, Treasurer P.O. Box 338 St. George, SC 29477";

                    string tax_info = ownertext1 + "~" + Map + "~" + Notice + "~" + Description + "~" + District + "~" + AppraisedValue + "~" + AssessedValue + "~" + Status + "~" + PaymentDate + "~" + AmountPaid + "~" + RecordType + "~" + TaxYear + "~" + Receipt + "~" + BaseTaxes + "~" + Penalty + "~" + TotalDue + "~" + TaxAuthority;
                    gc.insert_date(orderNumber, TMSNo, 632, tax_info, 1, DateTime.Now);

                    driver.FindElement(By.XPath("//*[@id='avalon']/div/div/ul/li[2]/a/div/i")).Click();
                    Thread.Sleep(4000);
                    gc.CreatePdf(orderNumber, TMSNo, "bill1", driver, "SC", "Dorchester");
                    try
                    {
                        driver.FindElement(By.XPath(" //*[@id='avalon']/div/div/ul/li[4]/a/div/i")).Click();
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, TMSNo, "bill2", driver, "SC", "Dorchester");
                    }
                    catch { }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "SC", "Dorchester", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    GlobalClass.titleparcel = "";
                    gc.mergpdf(orderNumber, "SC", "Dorchester");
                    return "Data Inserted Successfully";


                }
                catch (Exception ex)
                {
                    driver.Quit();
                    throw ex;
                }
            }
        }


    }
}