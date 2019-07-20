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
    public class Webdriver_ChesterfieldVA
    {
        string outputPath = "";
        IWebDriver driver;
        IWebElement Parcelweb;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_ChesterfieldVA(string streetno, string direction, string streetname, string city, string streettype, string unitnumber, string ownernm, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            List<string> taxinformation = new List<string>();
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string As_of = "", Total_Due = "", MillLevy = "", Class = "", YearBuilt = "", subdivision = "";
            List<string> pdflink = new List<string>();
            string Parcel_number = "", Tax_Authority = "", type = "", AddressCombain = "", Addresshrf = "", Pin = "", address = "", MailingAddress = "", Constructed = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new PhantomJSDriver();
            //driver = new ChromeDriver()
            using (driver = new PhantomJSDriver())
            {
                if (direction != "")
                {
                    address = streetno.Trim() + " " + direction.Trim() + " " + streetname.Trim() + " " + streettype.Trim();
                }
                else
                {
                    address = streetno.Trim() + " " + streetname.Trim() + " " + streettype.Trim();
                }
                if (searchType == "titleflex")
                {

                    gc.TitleFlexSearch(orderNumber, "", ownernm, address, "VA", "Chesterfield");
                    if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                    {
                        return "MultiParcel";
                    }
                    searchType = "parcel";
                    parcelNumber = HttpContext.Current.Session["titleparcel"].ToString().Replace(".", "");
                }
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("https://www.chesterfield.gov/828/Real-Estate-Assessment-Data#/");
                    if (searchType == "address")
                    {

                        driver.FindElement(By.Id("searchText")).SendKeys(address);
                        Thread.Sleep(4000);
                        ByVisibleElement(driver.FindElement(By.Id("searchText")));
                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "VA", "Chesterfield");
                        driver.FindElement(By.XPath("//*[@id='read-search-toolbar']/div/div/div/button[2]/div/i")).Click();
                        Thread.Sleep(6000);
                        ByVisibleElement(driver.FindElement(By.Id("searchText")));
                        gc.CreatePdf_WOP(orderNumber, "Address After", driver, "VA", "Chesterfield");

                        string Recored1 = driver.FindElement(By.XPath("//*[@id='read-content']/main/div/div/div[2]/div[2]/div/div/div/div[6]")).Text;
                        string Recored = GlobalClass.Before(Recored1, "records");
                        if (Recored.Trim().Contains("1"))
                        {
                            driver.FindElement(By.XPath("//*[@id='read-content']/main/div/div/div[2]/div[2]/div/div/div/div[3]/a/div/div/div[2]")).Click();
                            Thread.Sleep(2000);
                        }
                        if (!Recored.Trim().Contains("1"))
                        {
                            int Max = 0;                                                   //*[@id="read-content"]/main/div/div/div[2]/div[2]/div/div/div
                            IWebElement Multipletable = driver.FindElement(By.XPath("//*[@id='read-content']/main/div/div/div[2]/div[2]/div/div/div"));
                            IList<IWebElement> Multiplerow = Multipletable.FindElements(By.TagName("div"));
                            IList<IWebElement> Multipleid;
                            foreach (IWebElement assessment in Multiplerow)
                            {
                                Multipleid = assessment.FindElements(By.TagName("div"));
                                if (Multipleid.Count == 3 && !assessment.Text.Contains("bedroom(s)") && assessment.Text.Trim() != "" && assessment.Text.Contains("Parcel ID:"))
                                {
                                    //string Adres1 = Multipleid[5].Text;
                                    string addres = Multipleid[0].Text;
                                    string Parcel1 = Multipleid[2].Text;
                                    string Parcel = GlobalClass.After(Parcel1, "Parcel ID:");
                                    string Multipleresult = addres + "~" + Parcel;
                                    gc.insert_date(orderNumber, Parcel, 2154, Multipleresult, 1, DateTime.Now);
                                    Max++;
                                }
                            }

                            if (Max > 1 && Max < 26)
                            {
                                HttpContext.Current.Session["multiparcel_Chesterfield"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Max > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Chesterfield_Multicount"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        // gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "VA", "Chesterfield");
                        try
                        {
                            string Nodata = driver.FindElement(By.XPath("//*[@id='read-content']/main/div/div/div[2]/div[2]/div/div/div/div")).Text;
                            if (Nodata.Contains("No results were found"))
                            {
                                HttpContext.Current.Session["Zero_Chesterfield"] = "Zero";
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.Id("searchText")).SendKeys(parcelNumber);
                        Thread.Sleep(4000);
                        ByVisibleElement(driver.FindElement(By.Id("searchText")));
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "VA", "Chesterfield");
                        driver.FindElement(By.XPath("//*[@id='read-search-toolbar']/div/div/div/button[2]/div/i")).Click();
                        Thread.Sleep(6000);
                        ByVisibleElement(driver.FindElement(By.Id("searchText")));
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search After", driver, "VA", "Chesterfield");
                        driver.FindElement(By.XPath("//*[@id='read-content']/main/div/div/div[2]/div[2]/div/div/div/div[3]/a/div/div/div[2]/div/div[4]/ul[1]")).Click();
                        Thread.Sleep(8000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search click After", driver, "VA", "Chesterfield");
                    }
                    if (searchType == "Block")
                    {
                        driver.FindElement(By.Id("searchText")).SendKeys(unitnumber);
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "VA", "Chesterfield");
                        driver.FindElement(By.XPath("//*[@id='read-search-toolbar']/div/div/div/button[2]/div/i")).Click();
                        Thread.Sleep(6000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search After", driver, "VA", "Chesterfield");
                        driver.FindElement(By.XPath("//*[@id='read-content']/main/div/div/div[2]/div[2]/div/div/div/div[3]/a/div/div/div[2]/div/div[4]/ul[1]")).Click();
                        Thread.Sleep(8000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search click After", driver, "VA", "Chesterfield");
                    }
                    Parcel_number = driver.FindElement(By.Id("parcelNumber")).Text;
                    string RealEstateAccount = driver.FindElement(By.Id("accountNumber")).Text;
                    string PropertyClass = driver.FindElement(By.Id("propClass")).Text;
                    string MagisterialDistrict = driver.FindElement(By.Id("magDistrict")).Text;
                    try
                    {
                        subdivision = driver.FindElement(By.Id("subdivision")).Text;
                    }
                    catch { }

                    string DeededAcreage = driver.FindElement(By.Id("acres")).Text;
                    string ownerName = driver.FindElement(By.Id("ownerName")).Text;
                    string MailingAddress1 = driver.FindElement(By.Id("mailingAddress")).Text;
                    string MailingAddress2 = driver.FindElement(By.Id("mailingAddressLine2")).Text;
                    string MailingAddress3 = MailingAddress1 + " " + MailingAddress2;
                    string OwnershipType = driver.FindElement(By.Id("ownershipType")).Text;
                    string legalDescription = driver.FindElement(By.Id("legalDescription")).Text;
                    gc.CreatePdf(orderNumber, parcelNumber, "Overview", driver, "VA", "Chesterfield");
                    IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                    //*[@id="read-content"]/div[4]/div/div/div[1]/div/div/div
                    IWebElement Assessmentslink = driver.FindElement(By.XPath("//*[@id='read-content']/div[4]/div/div/div[1]/div/div/div"));
                    IList<IWebElement> IChargesRow = Assessmentslink.FindElements(By.TagName("div"));
                    IList<IWebElement> IChargesTD;
                    foreach (IWebElement charge in IChargesRow)
                    {
                        IChargesTD = charge.FindElements(By.TagName("a"));
                        if (IChargesTD.Count != 0)
                        {
                            try
                            {
                                string strcharges = IChargesTD[0].GetAttribute("innerText");
                                if (strcharges.Contains("RESIDENTIAL"))
                                {
                                    IWebElement IChargesSearch = IChargesTD[0];
                                    js.ExecuteScript("arguments[0].click();", IChargesSearch);
                                    Thread.Sleep(3000);
                                    break;
                                }
                            }
                            catch { }
                        }
                    }
                    Thread.Sleep(2000);

                    gc.CreatePdf(orderNumber, parcelNumber, "Residential", driver, "VA", "Chesterfield");
                    try
                    {
                        YearBuilt = driver.FindElement(By.Id("yearBuilt_0")).Text;
                    }
                    catch { }
                    string Propertydetail = RealEstateAccount + "~" + PropertyClass + "~" + MagisterialDistrict + "~" + subdivision + "~" + DeededAcreage + "~" + ownerName + "~" + MailingAddress3 + "~" + OwnershipType + "~" + legalDescription + "~" + YearBuilt;
                    gc.insert_date(orderNumber, Parcel_number, 2140, Propertydetail, 1, DateTime.Now);

                    // Assessmentslink.Click();
                    //Thread.Sleep(2000);
                    IWebElement Assessmentslink1 = driver.FindElement(By.XPath("//*[@id='read-content']/div[4]/div/div/div[1]/div/div/div"));
                    IList<IWebElement> IChargesRow1 = Assessmentslink1.FindElements(By.TagName("div"));
                    IList<IWebElement> IChargesTD1;
                    foreach (IWebElement charge1 in IChargesRow1)
                    {
                        IChargesTD1 = charge1.FindElements(By.TagName("a"));
                        if (IChargesTD1.Count != 0)
                        {
                            try
                            {
                                string strcharges = IChargesTD1[0].GetAttribute("innerText");
                                if (strcharges.Contains("ASSESSMENTS"))
                                {
                                    IWebElement IChargesSearch = IChargesTD1[0];
                                    js.ExecuteScript("arguments[0].click();", IChargesSearch);
                                    Thread.Sleep(3000);
                                    break;
                                }
                            }
                            catch { }
                        }
                    }
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Assessment", driver, "VA", "Chesterfield");
                    IWebElement Assessmenttable = driver.FindElement(By.XPath("//*[@id='tab_Assessments']/div/div[2]/div/div[1]/div/table/tbody"));
                    IList<IWebElement> Assessmentrow = Assessmenttable.FindElements(By.TagName("tr"));
                    IList<IWebElement> assessmentid;
                    foreach (IWebElement assessment in Assessmentrow)
                    {
                        assessmentid = assessment.FindElements(By.TagName("td"));
                        if (assessmentid.Count != 0 && !assessment.Text.Contains("Assessment Year:") && assessment.Text.Trim() != "")
                        {
                            string Assessmentresult = assessmentid[1].Text + "~" + assessmentid[2].Text + "~" + assessmentid[3].Text + "~" + assessmentid[4].Text + "~" + assessmentid[5].Text + "~" + assessmentid[6].Text;
                            gc.insert_date(orderNumber, Parcel_number, 2141, Assessmentresult, 1, DateTime.Now);
                        }
                    }
                    IWebElement Assessmentslink2 = driver.FindElement(By.XPath("//*[@id='read-content']/div[4]/div/div/div[1]/div/div/div"));
                    IList<IWebElement> IChargesRow2 = Assessmentslink2.FindElements(By.TagName("div"));
                    IList<IWebElement> IChargesTD2;
                    foreach (IWebElement charge2 in IChargesRow2)
                    {
                        IChargesTD2 = charge2.FindElements(By.TagName("a"));
                        if (IChargesTD2.Count != 0)
                        {
                            try
                            {
                                string strcharges = IChargesTD2[0].GetAttribute("innerText");
                                if (strcharges.Contains("IMPROVEMENTS"))
                                {
                                    IWebElement IChargesSearch = IChargesTD2[0];
                                    js.ExecuteScript("arguments[0].click();", IChargesSearch);
                                    Thread.Sleep(3000);
                                    break;
                                }
                            }
                            catch { }
                        }
                    }
                    gc.CreatePdf(orderNumber, parcelNumber, "Improvements", driver, "VA", "Chesterfield");
                    IWebElement Improvementtable = driver.FindElement(By.XPath("//*[@id='tab_Improvements']/div/div[2]/div/div[1]/div/table/tbody"));
                    IList<IWebElement> Improvementrow = Improvementtable.FindElements(By.TagName("tr"));
                    IList<IWebElement> Improvementid;
                    foreach (IWebElement Improvement in Improvementrow)
                    {
                        Improvementid = Improvement.FindElements(By.TagName("td"));
                        if (Improvementid.Count > 1)
                        {
                            string Improvementresult = Improvementid[0].Text + "~" + Improvementid[1].Text + "~" + Improvementid[2].Text + "~" + Improvementid[3].Text + "~" + Improvementid[4].Text + "~" + Improvementid[5].Text;
                            gc.insert_date(orderNumber, Parcel_number, 2142, Improvementresult, 1, DateTime.Now);
                        }
                    }

                    IWebElement Assessmentslink3 = driver.FindElement(By.XPath("//*[@id='read-content']/div[4]/div/div/div[1]/div/div/div"));
                    IList<IWebElement> IChargesRow3 = Assessmentslink3.FindElements(By.TagName("div"));
                    IList<IWebElement> IChargesTD3;
                    foreach (IWebElement charge3 in IChargesRow3)
                    {
                        IChargesTD3 = charge3.FindElements(By.TagName("a"));
                        if (IChargesTD3.Count != 0)
                        {
                            try
                            {
                                string strcharges = IChargesTD3[0].GetAttribute("innerText");
                                if (strcharges.Contains("LAND"))
                                {
                                    IWebElement IChargesSearch = IChargesTD3[0];
                                    js.ExecuteScript("arguments[0].click();", IChargesSearch);
                                    Thread.Sleep(3000);
                                    break;
                                }
                            }
                            catch { }
                        }
                    }
                    gc.CreatePdf(orderNumber, parcelNumber, "Land", driver, "VA", "Chesterfield");
                    string Deeded_Acreage = "";
                    IList<IWebElement> Acreslist = driver.FindElements(By.Id("acres"));
                    foreach (IWebElement Acres in Acreslist)
                    {
                        string Deered = Acres.Text;
                        if (Deered != "")
                        {
                            Deeded_Acreage = Deered;
                        }
                    }
                    string floodPlain = driver.FindElement(By.Id("floodPlain")).Text;
                    string Easement = driver.FindElement(By.Id("easement")).Text;
                    string CountyWater = driver.FindElement(By.Id("countyWater")).Text;
                    string CountySewer = driver.FindElement(By.Id("countySewer")).Text;
                    string Well = driver.FindElement(By.Id("well")).Text;
                    string Septic = driver.FindElement(By.Id("septic")).Text;
                    string Gas = driver.FindElement(By.Id("gas")).Text;
                    string electricity = driver.FindElement(By.Id("electricity")).Text;
                    string PavedStreets = driver.FindElement(By.Id("pavedStreets")).Text;
                    string StormDrains = driver.FindElement(By.Id("stormDrains")).Text;
                    string curbing = driver.FindElement(By.Id("curbing")).Text;
                    string Zoning = driver.FindElement(By.XPath("//*[@id='read-zoning']/div[2]/div/ul/li/div")).Text;
                    string LandLinkresult = Deeded_Acreage + "~" + floodPlain + "~" + Easement + "~" + CountyWater + "~" + Well + "~" + Septic + "~" + Gas + "~" + electricity + "~" + PavedStreets + "~" + StormDrains + "~" + curbing + "~" + Zoning;
                    gc.insert_date(orderNumber, Parcel_number, 2143, LandLinkresult, 1, DateTime.Now);
                    IWebElement Assessmentslink4 = driver.FindElement(By.XPath("//*[@id='read-content']/div[4]/div/div/div[1]/div/div/div"));
                    IList<IWebElement> IChargesRow4 = Assessmentslink4.FindElements(By.TagName("div"));
                    IList<IWebElement> IChargesTD4;
                    foreach (IWebElement charge4 in IChargesRow4)
                    {
                        IChargesTD4 = charge4.FindElements(By.TagName("a"));
                        if (IChargesTD4.Count != 0)
                        {
                            try
                            {
                                string strcharges = IChargesTD4[0].GetAttribute("innerText");
                                if (strcharges.Contains("OWNERSHIP"))
                                {
                                    IWebElement IChargesSearch = IChargesTD4[0];
                                    js.ExecuteScript("arguments[0].click();", IChargesSearch);
                                    Thread.Sleep(3000);
                                    break;
                                }
                            }
                            catch { }
                        }
                    }
                    gc.CreatePdf(orderNumber, parcelNumber, "Ownership", driver, "VA", "Chesterfield");
                    IWebElement Ownershiptable = driver.FindElement(By.XPath("//*[@id='tab_Ownership']/div/div[2]/div/div[1]/div/table/tbody"));
                    IList<IWebElement> Ownershiprow = Ownershiptable.FindElements(By.TagName("tr"));
                    IList<IWebElement> Ownershipid;
                    foreach (IWebElement Ownership in Ownershiprow)
                    {
                        Ownershipid = Ownership.FindElements(By.TagName("td"));
                        if (Ownershipid.Count > 1)
                        {
                            string Ownershipresult = Ownershipid[1].Text + "~" + Ownershipid[2].Text + "~" + Ownershipid[3].Text + "~" + Ownershipid[4].Text + "~" + Ownershipid[5].Text + "~" + Ownershipid[6].Text + "~" + Ownershipid[7].Text + "~" + Ownershipid[8].Text + "~" + Ownershipid[9].Text + "~" + Ownershipid[10].Text;
                            gc.insert_date(orderNumber, Parcel_number, 2153, Ownershipresult, 1, DateTime.Now);
                        }
                    }
                    //Tax Site
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    IWebElement Assessmentslink5 = driver.FindElement(By.XPath("//*[@id='read-content']/div[4]/div/div/div[1]/div/div/div"));
                    IList<IWebElement> IChargesRow5 = Assessmentslink5.FindElements(By.TagName("div"));
                    IList<IWebElement> IChargesTD5;
                    foreach (IWebElement charge5 in IChargesRow5)
                    {
                        IChargesTD5 = charge5.FindElements(By.TagName("a"));
                        if (IChargesTD5.Count != 0)
                        {
                            try
                            {
                                string strcharges = IChargesTD5[0].GetAttribute("innerText");
                                if (strcharges.Contains("TAX"))
                                {
                                    IWebElement IChargesSearch = IChargesTD5[0];
                                    js.ExecuteScript("arguments[0].click();", IChargesSearch);
                                    Thread.Sleep(3000);
                                    break;
                                }
                            }
                            catch { }
                        }
                    }
                    gc.CreatePdf(orderNumber, parcelNumber, "TaxHistory", driver, "VA", "Chesterfield");
                    string taxAccount = driver.FindElement(By.Id("taxAccount")).Text;
                    string Currentbalance1 = driver.FindElement(By.XPath("//*[@id='tab_Tax']/div/div[2]/div/ul/li[2]/div/div/div/ul/li/div[1]")).Text;
                    string currentbalncedate = GlobalClass.After(Currentbalance1, "as of");
                    string currentbalanceamt = driver.FindElement(By.XPath("//*[@id='tab_Tax']/div/div[2]/div/ul/li[2]/div/div/div/ul/li/div[2]")).Text;
                    IWebElement TaxAssessmenttable = driver.FindElement(By.XPath("//*[@id='tab_Tax']/div/div[2]/div/ul/li[3]/div[2]/div/div/table/tbody"));
                    IList<IWebElement> TaxAssessmentrow = TaxAssessmenttable.FindElements(By.TagName("tr"));
                    IList<IWebElement> TaxAssessmentid;
                    foreach (IWebElement TaxAssessment in TaxAssessmentrow)
                    {
                        TaxAssessmentid = TaxAssessment.FindElements(By.TagName("td"));
                        if (TaxAssessmentid.Count != 0)
                        {
                            string TaxAssessmentresult = taxAccount + "~" + currentbalncedate + "~" + currentbalanceamt + "~" + TaxAssessmentid[0].Text + "~" + TaxAssessmentid[1].Text + "~" + TaxAssessmentid[2].Text + "~" + TaxAssessmentid[3].Text;
                            gc.insert_date(orderNumber, Parcel_number, 2144, TaxAssessmentresult, 1, DateTime.Now);
                        }
                    }
                    driver.FindElement(By.XPath("//*[@id='tab_Tax']/div/div[2]/div/ul/li[6]/div/div[1]/div[1]")).Click();
                    Thread.Sleep(6000);
                    IList<IWebElement> Licount = driver.FindElement(By.XPath("//*[@id='tab_Tax']/div/div[2]/div/ul/li[6]/div/div[2]/div/ul")).FindElements(By.TagName("li"));
                    try
                    {
                        for (int I = 1; I <= 6; I++)
                        {
                            //*[@id="tab_Tax"]/div/div[2]/div/ul/li[6]/div/div[2]/div/ul/li[1]
                            IWebElement Halfclick = driver.FindElement(By.XPath("//*[@id='tab_Tax']/div/div[2]/div/ul/li[6]/div/div[2]/div/ul/li[" + I + "]"));
                            Halfclick.SendKeys(Keys.Enter);
                            //js.ExecuteScript("arguments[0].click();", Halfclick);
                            Thread.Sleep(4000);
                            string Taxyear1 = driver.FindElement(By.XPath("//*[@id='tab_Tax']/div/div[2]/div/ul/li[6]/div/div[2]/div/ul/li[" + I + "]/div[1]")).Text;
                            string taxyear = GlobalClass.Before(Taxyear1, ":");
                            gc.CreatePdf(orderNumber, parcelNumber, "TaxHistory" + I, driver, "VA", "Chesterfield");
                            IWebElement TaxaccountHistorytable = driver.FindElement(By.XPath("//*[@id='tab_Tax']/div/div[2]/div/ul/li[6]/div/div[2]/div/ul/li[" + I + "]/div[2]/div/div/div/table/tbody"));
                            IList<IWebElement> TaxaccountHistoryrow = TaxaccountHistorytable.FindElements(By.TagName("tr"));
                            IList<IWebElement> TaxaccountHistoryid;
                            foreach (IWebElement TaxaccountHistory in TaxaccountHistoryrow)
                            {
                                TaxaccountHistoryid = TaxaccountHistory.FindElements(By.TagName("td"));
                                if (TaxaccountHistoryid.Count != 0)
                                {
                                    string TaxaccountHistoryresult = taxyear + "~" + TaxaccountHistoryid[0].Text + "~" + TaxaccountHistoryid[1].Text + "~" + TaxaccountHistoryid[2].Text + "~" + TaxaccountHistoryid[3].Text + "~" + TaxaccountHistoryid[4].Text + "~" + TaxaccountHistoryid[5].Text + "~" + TaxaccountHistoryid[6].Text;
                                    gc.insert_date(orderNumber, Parcel_number, 2145, TaxaccountHistoryresult, 1, DateTime.Now);
                                }
                            }
                        }
                    }
                    catch { }

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "VA", "Chesterfield", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    driver.Quit();
                    //HttpContext.Current.Session["titleparcel"] = null;
                    gc.mergpdf(orderNumber, "VA", "Chesterfield");
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
        public void ByVisibleElement(IWebElement Element)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].scrollIntoView();", Element);
        }
    }

}