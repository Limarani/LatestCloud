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

    public class WebDriver_LeeFL
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();

        public string FTP_LeeFL(string Address, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            List<string> strissuecertificate = new List<string>();
            List<string> taxhistorylink = new List<string>();
            List<string> downloadlink = new List<string>();
            List<string> taxhistorylinkinst = new List<string>();
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new ChromeDriver();
            //driver = new PhantomJSDriver();
            using (driver = new PhantomJSDriver()) //ChromeDriver
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("http://www.leepa.org/Search/PropertySearch.aspx");

                    if (searchType == "titleflex")
                    {

                        gc.TitleFlexSearch(orderNumber, "", "", Address, "FL", "Lee");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_LeeFL"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString().Replace("-", "");
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        driver.FindElement(By.Id("ctl00_BodyContentPlaceHolder_AddressTextBox")).SendKeys(Address);
                        gc.CreatePdf_WOP(orderNumber, "Address search Input ", driver, "FL", "Lee");
                        driver.FindElement(By.Id("ctl00_BodyContentPlaceHolder_SubmitPropertySearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Address search Result ", driver, "FL", "Lee");
                        ////Multiparcel
                        try
                        {
                            //int Count = 0;
                            string matches = driver.FindElement(By.Id("ctl00_BodyContentPlaceHolder_SearchResultsRepeater_ctl00_infoHeaderDetail")).Text;
                            string matches1 = gc.Between(matches, "found", "matches").Trim();
                            IWebElement Multiaddresstable = driver.FindElement(By.XPath("//*[@id='ctl00_BodyContentPlaceHolder_PropertySearchUpdatePanel']/div[3]/table/tbody"));
                            IList<IWebElement> multiaddressrow = Multiaddresstable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Multiaddressid;
                            foreach (IWebElement Multiaddress in multiaddressrow)
                            {
                                Multiaddressid = Multiaddress.FindElements(By.TagName("td"));
                                if (multiaddressrow.Count > 8 && Multiaddressid.Count != 0 && Multiaddressid.Count == 8 && !Multiaddress.Text.Contains("STRAP  / Folio ID "))
                                {
                                    string Multiparcelnumber = Multiaddressid[1].Text;
                                    string OWnername = Multiaddressid[2].Text;
                                    string Address1 = Multiaddressid[3].Text;

                                    string multiaddressresult = OWnername + "~" + Address1;
                                    gc.insert_date(orderNumber, Multiparcelnumber, 1525, multiaddressresult, 1, DateTime.Now);
                                    //Count++;
                                }
                            }
                            if (multiaddressrow.Count == 8)
                            {
                                driver.FindElement(By.XPath("//*[@id='ctl00_BodyContentPlaceHolder_PropertySearchUpdatePanel']/div[3]/table/tbody/tr[1]/td[5]/table/tbody/tr/td/table/tbody/tr[1]/th[1]/a")).Click();
                                Thread.Sleep(2000);
                            }

                            if (multiaddressrow.Count > 8 && Convert.ToInt16(matches1) <= 10)
                            {
                                HttpContext.Current.Session["multiparcel_LeeFL"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Convert.ToInt16(matches1) > 10)
                            {
                                HttpContext.Current.Session["multiParcel_LeeFL_Maximum"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("ctl00_BodyContentPlaceHolder_ErrorLabel")).Text;
                            if (nodata.Contains("Returned 0 records.") || nodata.Contains("No matches found"))
                            {
                                HttpContext.Current.Session["Nodata_LeeFL"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.Id("ctl00_BodyContentPlaceHolder_STRAPTextBox")).SendKeys(parcelNumber);
                        gc.CreatePdf_WOP(orderNumber, "Parcel search", driver, "FL", "Lee");
                        driver.FindElement(By.Id("ctl00_BodyContentPlaceHolder_SubmitPropertySearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(5000);
                        gc.CreatePdf_WOP(orderNumber, "Parcel search Result", driver, "FL", "Lee");
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("ctl00_BodyContentPlaceHolder_ErrorLabel")).Text;
                            if (nodata.Contains("Returned 0 records.") || nodata.Contains("No matches found"))
                            {
                                HttpContext.Current.Session["Nodata_LeeFL"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("ctl00_BodyContentPlaceHolder_OwnerNameTextBox")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "Ownername search Input ", driver, "FL", "Lee");
                        driver.FindElement(By.Id("ctl00_BodyContentPlaceHolder_SubmitPropertySearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(5000);
                        gc.CreatePdf_WOP(orderNumber, "Ownername search Result ", driver, "FL", "Lee");

                        ////Multiparcel
                        try
                        {
                            //int Count = 0;
                            string matches = driver.FindElement(By.Id("ctl00_BodyContentPlaceHolder_SearchResultsRepeater_ctl00_infoHeaderDetail")).Text;
                            string matches1 = gc.Between(matches, "found", "matches").Trim();
                            IWebElement Multiaddresstable = driver.FindElement(By.XPath("//*[@id='ctl00_BodyContentPlaceHolder_PropertySearchUpdatePanel']/div[3]/table/tbody"));
                            IList<IWebElement> multiaddressrow = Multiaddresstable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Multiaddressid;
                            foreach (IWebElement Multiaddress in multiaddressrow)
                            {
                                Multiaddressid = Multiaddress.FindElements(By.TagName("td"));
                                if (multiaddressrow.Count > 8 && Multiaddressid.Count != 0 && Multiaddressid.Count == 8 && !Multiaddress.Text.Contains("STRAP  / Folio ID "))
                                {
                                    string Multiparcelnumber = Multiaddressid[1].Text;
                                    string OWnername = Multiaddressid[2].Text;
                                    string Address1 = Multiaddressid[3].Text;

                                    string multiaddressresult = OWnername + "~" + Address1;
                                    gc.insert_date(orderNumber, Multiparcelnumber, 1525, multiaddressresult, 1, DateTime.Now);
                                    //Count++;
                                }
                            }
                            if (multiaddressrow.Count == 8)
                            {
                                driver.FindElement(By.XPath("//*[@id='ctl00_BodyContentPlaceHolder_PropertySearchUpdatePanel']/div[3]/table/tbody/tr[1]/td[5]/table/tbody/tr/td/table/tbody/tr[1]/th[1]/a")).Click();
                                Thread.Sleep(2000);
                            }

                            if (multiaddressrow.Count > 8 && Convert.ToInt16(matches1) <= 10)
                            {
                                HttpContext.Current.Session["multiparcel_LeeFL"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Convert.ToInt16(matches1) > 10)
                            {
                                HttpContext.Current.Session["multiParcel_LeeFL_Maximum"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("ctl00_BodyContentPlaceHolder_ErrorLabel")).Text;
                            if (nodata.Contains("Returned 0 records."))
                            {
                                HttpContext.Current.Session["Nodata_LeeFL"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    //Property Details
                    string ParcelID = "", FolioNo = "", OwnerName = "", PropertyAddress = "", MailingAdd1 = "", MailingAdd2 = "", MailingAdd3 = "", MailingAdd4 = "", MailingAdd = "", Legaldes = "", Yearbuilt = "", Usecode = "", Usecodedescri = "";
                    //ParcelID = driver.FindElement(By.XPath("//*[@id='ctl00_BodyContentPlaceHolder_PropertySearchUpdatePanel']/div[3]/table/tbody/tr[1]/td[2]")).Text;
                    //  FolioNo= driver.FindElement(By.XPath("//*[@id='ctl00_BodyContentPlaceHolder_PropertySearchUpdatePanel']/div[3]/table/tbody/tr[2]/td[1]")).Text;

                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='ctl00_BodyContentPlaceHolder_PropertySearchUpdatePanel']/div[3]/table/tbody/tr[1]/td[5]/table/tbody/tr/td/table/tbody/tr[1]/th[1]/a")).Click();
                        Thread.Sleep(9000);
                        driver.SwitchTo().Window(driver.WindowHandles.Last());
                    }
                    catch { }
                    try
                    {
                        IWebElement Receipttable = driver.FindElement(By.XPath("//*[@id='ctl00_BodyContentPlaceHolder_PropertySearchUpdatePanel']/div[3]/table/tbody/tr[1]/td[5]/table/tbody/tr/td/table/tbody/tr[1]/th[1]/a"));
                        IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                        js1.ExecuteScript("arguments[0].click();", Receipttable);
                        Thread.Sleep(9000);
                        driver.SwitchTo().Window(driver.WindowHandles.Last());
                    }
                    catch { }
                    try
                    {
                        IWebElement Receipttable = driver.FindElement(By.LinkText("Parcel Details"));
                        IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                        js1.ExecuteScript("arguments[0].click();", Receipttable);
                        Thread.Sleep(9000);
                        driver.SwitchTo().Window(driver.WindowHandles.Last());
                    }
                    catch { }
                    Thread.Sleep(2000);
                    //gc.CreatePdf(orderNumber, parcelNumber, "Parcel search Input", driver, "FL", "Lee");
                    //driver.SwitchTo().Window(driver.WindowHandles.Last());
                    //Thread.Sleep(5000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Parcel search Result", driver, "FL", "Lee");


                    IWebElement ParcelID1 = driver.FindElement(By.XPath("//*[@id='content']/div/div[1]/div[2]"));
                    ParcelID = gc.Between(ParcelID1.Text, "STRAP:", "Folio ID:").Trim();
                    FolioNo = GlobalClass.After(ParcelID1.Text, "Folio ID:").Trim();
                    PropertyAddress = driver.FindElement(By.XPath("//*[@id='divDisplayParcelOwner']/div[2]/div[2]")).Text.Trim();
                    IWebElement OwnerName1 = driver.FindElement(By.XPath("//*[@id='divDisplayParcelOwner']/div[1]/div/div[2]/div"));
                    //string[] OwnerName1split = OwnerName1.Text.Split('\r');
                    //OwnerName = OwnerName1split[0].Replace("\n", "").Trim();
                    //try
                    //{
                    //    MailingAdd1 = OwnerName1split[1].Replace("\n", "").Trim();
                    //    MailingAdd2 = OwnerName1split[2].Replace("\n", "").Trim();
                    //    MailingAdd3 = OwnerName1split[3].Replace("\n", "").Trim();
                    //    MailingAdd4 = OwnerName1split[4].Replace("\n", "").Trim();

                    //}
                    //catch { }
                    //MailingAdd = MailingAdd1.Trim() + " " + MailingAdd2.Trim() + " " + MailingAdd3.Trim() + " " + MailingAdd4.Trim();
                    ownername = OwnerName1.Text.Replace("\r\n", " ");
                    Legaldes = driver.FindElement(By.XPath("//*[@id='divDisplayParcelOwner']/div[3]/div[2]")).Text.Trim();
                    //Yearbuilt = driver.FindElement(By.XPath("//*[@id='divDisplayParcelAttributes']/div[2]/table[2]/tbody/tr[6]/td")).Text.Trim();
                    driver.FindElement(By.Id("AppraisalHyperLink2")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Appraisal Details", driver, "FL", "Lee");
                    try
                    {
                        Usecode = driver.FindElement(By.XPath("//*[@id='AppraisalDetails']/div[2]/table[1]/tbody/tr[3]/td[1]")).Text.Trim();
                        Usecodedescri = driver.FindElement(By.XPath("//*[@id='AppraisalDetails']/div[2]/table[1]/tbody/tr[3]/td[2]")).Text.Trim();
                    }
                    catch { }
                    try
                    {
                        Yearbuilt = driver.FindElement(By.XPath("//*[@id='divDisplayParcelAttributes']/div[2]/table[2]/tbody/tr[6]/td")).Text.Trim();
                    }
                    catch { }
                    try
                    {
                        Yearbuilt = driver.FindElement(By.XPath("//*[@id='AppraisalDetails']/div[3]/table[2]/tbody/tr[3]/td[11]")).Text.Trim();
                    }
                    catch { }
                    //Assessment Details
                    string Assessedyear = "", Just = "", Assessed = "", PortabilityApp = "", Capassessed = "", Taxable = "", Capdifferen = "", Homestead = "", Additional = "";

                    driver.FindElement(By.Id("LastRollHyperLink2")).Click();
                    Thread.Sleep(2000);
                    IWebElement Assessedyear1 = driver.FindElement(By.Id("LastRollHyperLink2"));
                    Assessedyear = gc.Between(Assessedyear1.Text, "Values (", "Tax Roll)").Trim();

                    IWebElement Bigdata3 = driver.FindElement(By.XPath("//*[@id='taxRollTable']/tbody/tr[2]/td[1]/table/tbody"));
                    IList<IWebElement> TRBigdata3 = Bigdata3.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDBigdata3;
                    foreach (IWebElement row3 in TRBigdata3)
                    {
                        TDBigdata3 = row3.FindElements(By.TagName("td"));

                        if (TDBigdata3.Count != 0 && TDBigdata3.Count == 1 && row3.Text.Contains("Just"))
                        {
                            Just = TDBigdata3[0].Text;
                        }
                        if (TDBigdata3.Count != 0 && TDBigdata3.Count == 1 && row3.Text.Contains("Assessed") && !row3.Text.Contains("Cap"))
                        {
                            Assessed = TDBigdata3[0].Text;
                        }
                        if (TDBigdata3.Count != 0 && TDBigdata3.Count == 1 && row3.Text.Contains("Portability Applied"))
                        {
                            PortabilityApp = TDBigdata3[0].Text;
                        }
                        if (TDBigdata3.Count != 0 && TDBigdata3.Count == 1 && row3.Text.Contains("Cap") && !row3.Text.Contains("Difference"))
                        {
                            Capassessed = TDBigdata3[0].Text;
                        }
                        if (TDBigdata3.Count != 0 && TDBigdata3.Count == 1 && row3.Text.Contains("Taxable"))
                        {
                            Taxable = TDBigdata3[0].Text;
                        }
                        if (TDBigdata3.Count != 0 && TDBigdata3.Count == 1 && row3.Text.Contains("Cap Difference"))
                        {
                            Capdifferen = TDBigdata3[0].Text;
                        }
                    }
                    string AssessmentDetails = Assessedyear.Trim() + "~" + Just.Trim() + "~" + Assessed.Trim() + "~" + PortabilityApp.Trim() + "~" + Capassessed.Trim() + "~" + Taxable.Trim() + "~" + Capdifferen.Trim();
                    gc.insert_date(orderNumber, ParcelID, 1516, AssessmentDetails, 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    //Overall Extended Click Screenshots
                    //Exemption click
                    try
                    {
                        driver.FindElement(By.Id("ExemptionsHyperLink2")).Click();
                        Thread.Sleep(2000);
                    }
                    catch { }
                    //Taxing Authorities
                    try
                    {
                        driver.FindElement(By.Id("TaxAuthorityHyperLink2")).Click();
                        Thread.Sleep(2000);
                    }
                    catch { }
                    //Sales /Transactions
                    try
                    {
                        driver.FindElement(By.Id("SalesHyperLink2")).Click();
                        Thread.Sleep(2000);
                    }
                    catch { }
                    //Building/Construction Permit Data
                    try
                    {
                        driver.FindElement(By.Id("PermitHyperLink2")).Click();
                        Thread.Sleep(2000);
                    }
                    catch { }
                    //Parcel Numbering History
                    try
                    {
                        driver.FindElement(By.Id("NumberingHyperLink2")).Click();
                        Thread.Sleep(2000);
                    }
                    catch { }
                    //Location Information
                    try
                    {
                        driver.FindElement(By.Id("LocationHyperLink2")).Click();
                        Thread.Sleep(2000);
                    }
                    catch { }
                    //Solid Waste
                    try
                    {
                        driver.FindElement(By.Id("GarbageHyperLink2")).Click();
                        Thread.Sleep(2000);
                    }
                    catch { }
                    //Flood and Storm Information
                    try
                    {
                        driver.FindElement(By.Id("ElevationHyperLink2")).Click();
                        Thread.Sleep(2000);
                    }
                    catch { }
                    //Appraisal Details (Current Working Values)
                    try
                    {
                        driver.FindElement(By.Id("AppraisalHyperLinkCurrent2")).Click();
                        Thread.Sleep(2000);
                    }
                    catch { }
                    gc.CreatePdf(orderNumber, parcelNumber, "Extended Click", driver, "FL", "Lee");
                    //Tax Authority Details
                    string TaxAuthority = "", TaxAuthority1 = "", TaxAuthority2 = "", TaxAuthority3 = "", TaxAuthority4 = "";
                    try
                    {
                        driver.Navigate().GoToUrl("https://www.leetc.com/contact-us");
                        TaxAuthority = driver.FindElement(By.XPath("//*[@id='form1']/div[4]/div/div/div[2]/div[1]/div[2]/p[4]")).Text;
                    }
                    catch { }
                    //Property Details Insert
                    string PropertyDetails = FolioNo.Trim() + "~" + PropertyAddress.Trim() + "~" + ownername + "~" + Legaldes.Trim() + "~" + Yearbuilt.Trim() + "~" + Usecode.Trim() + "~" + Usecodedescri.Trim() + "~" + TaxAuthority.Trim();
                    gc.insert_date(orderNumber, ParcelID, 1506, PropertyDetails, 1, DateTime.Now);
                    //Tax Information Details
                    driver.Navigate().GoToUrl("https://www.leetc.com/ncp/search_criteria.asp?searchtype=RP");

                    string Parcelnum = "", Propertyadd = "", Owner = "", TaxYear = "", Status = "", Totaldue1 = "", Totaldue = "", Asofgoodamt = "", Paidamt = "", Paiddate = "", Effdate = "", Certinum = "", Goodthroughdate = "";

                    driver.FindElement(By.Id("account_query1")).SendKeys(ParcelID.Replace(".", "").Replace("-", "").Trim());
                    SelectElement ss = new SelectElement(driver.FindElement(By.Id("account_queryAddl")));
                    ss.SelectByText("All");
                    driver.FindElement(By.Id("searchsubmit")).SendKeys(Keys.Enter);
                    Thread.Sleep(9000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax History pdf", driver, "FL", "Lee");
                    //Payment History Details
                    string SSBillType1 = "", SSStatus1 = "", SSDuedate1 = "", Owneradd = "", SSDue1 = "";
                    string Syear = DateTime.Now.Year.ToString();
                    int iyear = Int32.Parse(Syear);
                    List<string> billinfo = new List<string>();
                    IWebElement Billsinfo2 = driver.FindElement(By.Id("resultstable"));
                    IList<IWebElement> TRBillsinfo2 = Billsinfo2.FindElements(By.TagName("tr"));
                    IList<IWebElement> Aherftax;

                    foreach (IWebElement row in TRBillsinfo2)
                    {
                        Aherftax = row.FindElements(By.TagName("td"));

                        if (Aherftax.Count != 0 && Aherftax.Count == 5 && !row.Text.Contains("Account"))
                        {
                            SSBillType1 = Aherftax[0].Text;
                            SSStatus1 = Aherftax[1].Text;
                            SSDuedate1 = Aherftax[2].Text;
                            string[] SSDuedate1split = SSDuedate1.Split('\r');
                            Owneradd = SSDuedate1split[0].Trim();
                            SSDue1 = Aherftax[3].Text;

                            string Paymentdetails = SSStatus1.Trim() + "~" + Owneradd.Trim() + "~" + SSDue1.Trim();
                            gc.insert_date(orderNumber, ParcelID, 1602, Paymentdetails, 1, DateTime.Now);
                        }
                        if (billinfo.Count < 3 && Aherftax.Count != 0 && Aherftax[0].Text.Trim() != "" && !row.Text.Contains("Account"))
                        {
                            if (iyear == Convert.ToInt32(SSStatus1))
                            {
                                IWebElement value1 = Aherftax[0].FindElement(By.TagName("a"));
                                string addview = value1.GetAttribute("href");
                                billinfo.Add(addview);
                            }
                            else if ((Convert.ToInt32(iyear) - 1) == Convert.ToInt32(SSStatus1))
                            {
                                IWebElement value1 = Aherftax[0].FindElement(By.TagName("a"));
                                string addview = value1.GetAttribute("href");
                                billinfo.Add(addview);
                            }

                            else if ((Convert.ToInt32(iyear) - 2) == Convert.ToInt32(SSStatus1))
                            {
                                IWebElement value1 = Aherftax[0].FindElement(By.TagName("a"));
                                string addview = value1.GetAttribute("href");
                                billinfo.Add(addview);

                            }
                            else if ((Convert.ToInt32(iyear) - 3) == Convert.ToInt32(SSStatus1))
                            {
                                IWebElement value1 = Aherftax[0].FindElement(By.TagName("a"));
                                string addview = value1.GetAttribute("href");
                                billinfo.Add(addview);
                            }
                        }
                    }
                    int i = 0;
                    foreach (string assessmentclick in billinfo)
                    {

                        driver.Navigate().GoToUrl(assessmentclick);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Page" + i, driver, "FL", "Lee");
                        Parcelnum = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div/div/div/table/tbody/tr[3]/td[1]/span/a")).Text.Trim();
                        Propertyadd = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div/div/div/table/tbody/tr[9]/td[1]")).Text.Trim();
                        Owner = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div/div/div/table/tbody/tr[7]/td/span")).Text.Trim();
                        TaxYear = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div/div/div/table/tbody/tr[3]/td[2]")).Text.Trim();
                        Status = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div/div/div/table/tbody/tr[3]/td[3]/span/a")).Text.Trim();
                        try
                        {
                            Totaldue1 = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div/div/div/table/tbody/tr[12]/td[2]")).Text.Trim();
                        }
                        catch { }
                        try
                        {
                            if (!Totaldue1.Contains("Status"))
                            {
                                Totaldue = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div/div/div/table/tbody/tr[12]/td[2]")).Text.Trim();
                            }
                        }
                        catch { }
                        string Taxinformationdetails = Propertyadd.Trim() + "~" + Owner.Trim() + "~" + TaxYear.Trim() + "~" + Status.Trim() + "~" + Totaldue.Trim();
                        gc.insert_date(orderNumber, Parcelnum, 1519, Taxinformationdetails, 1, DateTime.Now);
                        //Tax Distribution Details
                        //Ad Valorem Details
                        string Taxingauthority = "", Millrate = "", Assess = "", Exempt = "", Taxable1 = "", Amount = "", Taxtype = "";

                        driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div/div/div/span/ul/li[2]/a")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Ad valorem" + i, driver, "FL", "Lee");
                        Taxtype = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div/div/div/div[2]/table[2]/tbody/tr/td/span/strong")).Text.Replace("Taxes", "").Trim();
                        IWebElement Bigdata1 = driver.FindElement(By.Id("advalorem"));
                        IList<IWebElement> TRBigdata1 = Bigdata1.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDBigdata1;
                        foreach (IWebElement row1 in TRBigdata1)
                        {
                            TDBigdata1 = row1.FindElements(By.TagName("td"));

                            if (TDBigdata1.Count != 0 && TDBigdata1.Count == 6 && !row1.Text.Contains("Taxing Authority"))
                            {
                                Taxingauthority = TDBigdata1[0].Text;
                                Millrate = TDBigdata1[1].Text;
                                Assess = TDBigdata1[2].Text;
                                Exempt = TDBigdata1[3].Text;
                                Taxable1 = TDBigdata1[4].Text;
                                Amount = TDBigdata1[5].Text;

                                string Advaloremdetails = TaxYear.Trim() + "~" + Taxtype.Trim() + "~" + Taxingauthority.Trim() + "~" + Millrate.Trim() + "~" + Assess.Trim() + "~" + Exempt.Trim() + "~" + Taxable1.Trim() + "~" + Amount.Trim();
                                gc.insert_date(orderNumber, Parcelnum, 1521, Advaloremdetails, 1, DateTime.Now);
                            }
                        }
                        //Non Ad Valorem Details
                        string NonadTaxingauthority = "", Rate = "", Basis = "", NonadAmount = "", NonadTaxtype = "";

                        try
                        {       // driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div/div/div/span/ul/li[2]/a")).Click();
                            NonadTaxtype = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div/div/div/div[2]/table[5]/tbody/tr/td/span/strong")).Text.Replace("Assessments", "").Trim();
                            IWebElement Bigdata2 = driver.FindElement(By.Id("nonadvalorem"));
                            IList<IWebElement> TRBigdata2 = Bigdata2.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDBigdata2;
                            foreach (IWebElement row2 in TRBigdata2)
                            {
                                TDBigdata2 = row2.FindElements(By.TagName("td"));

                                if (TDBigdata2.Count != 0 && TDBigdata2.Count == 4 && !row2.Text.Contains("Taxing Authority"))
                                {
                                    NonadTaxingauthority = TDBigdata2[0].Text;
                                    Rate = TDBigdata2[1].Text;
                                    Basis = TDBigdata2[2].Text;
                                    NonadAmount = TDBigdata2[3].Text;

                                    string NonAdvaloremdetails = TaxYear.Trim() + "~" + NonadTaxtype.Trim() + "~" + NonadTaxingauthority.Trim() + "~" + Rate.Trim() + "~" + Basis.Trim() + "~" + NonadAmount.Trim();
                                    gc.insert_date(orderNumber, Parcelnum, 1523, NonAdvaloremdetails, 1, DateTime.Now);
                                }
                            }
                        }
                        catch
                        { }
                        //Due Date Details
                        try
                        {
                            IWebElement valuetableElement = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div/div/div/div[2]/table[8]/tbody/tr/td/table/tbody"));
                            IList<IWebElement> valuetableRow = valuetableElement.FindElements(By.TagName("tr"));
                            IList<IWebElement> valuerowTD;
                            List<string> Ifpaidby = new List<string>();
                            List<string> Pleasepay = new List<string>();
                            int s = 0;
                            foreach (IWebElement row2 in valuetableRow)
                            {
                                valuerowTD = row2.FindElements(By.TagName("td"));
                                if (valuerowTD.Count != 0 && !row2.Text.Contains("Amount Due If Paid In") && valuerowTD.Count == 5)
                                {
                                    if (s == 0)
                                    {
                                        Ifpaidby.Add(valuerowTD[0].Text.Trim());
                                        Ifpaidby.Add(valuerowTD[1].Text.Trim());
                                        Ifpaidby.Add(valuerowTD[2].Text.Trim());
                                        Ifpaidby.Add(valuerowTD[3].Text.Trim());
                                        Ifpaidby.Add(valuerowTD[4].Text.Trim());
                                    }
                                    else if (s == 1)
                                    {
                                        Pleasepay.Add(valuerowTD[0].Text);
                                        Pleasepay.Add(valuerowTD[1].Text);
                                        Pleasepay.Add(valuerowTD[2].Text);
                                        Pleasepay.Add(valuerowTD[3].Text);
                                        Pleasepay.Add(valuerowTD[4].Text);
                                    }
                                    s++;
                                }
                            }
                            string Amountdue1 = Ifpaidby[0] + "~" + Pleasepay[0];
                            string Amountdue2 = Ifpaidby[1] + "~" + Pleasepay[1];
                            string Amountdue3 = Ifpaidby[2] + "~" + Pleasepay[2];
                            string Amountdue4 = Ifpaidby[3] + "~" + Pleasepay[3];
                            string Amountdue5 = Ifpaidby[4] + "~" + Pleasepay[4];
                            gc.insert_date(orderNumber, Parcelnum, 1524, TaxYear + "~" + Amountdue1, 1, DateTime.Now);
                            gc.insert_date(orderNumber, Parcelnum, 1524, TaxYear + "~" + Amountdue2, 1, DateTime.Now);
                            gc.insert_date(orderNumber, Parcelnum, 1524, TaxYear + "~" + Amountdue3, 1, DateTime.Now);
                            gc.insert_date(orderNumber, Parcelnum, 1524, TaxYear + "~" + Amountdue4, 1, DateTime.Now);
                            gc.insert_date(orderNumber, Parcelnum, 1524, TaxYear + "~" + Amountdue5, 1, DateTime.Now);
                        }
                        catch { }
                        //Tax History Details
                        try
                        {
                            string Taxyearhis = "", Taxablevalue = "", Exemptions = "", Totaltax = "";
                            driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div/div/div/span/ul/li[5]/a")).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, Parcelnum, "Tax History" + i, driver, "FL", "Lee");
                            IWebElement Bigdata4 = driver.FindElement(By.Id("history"));
                            IList<IWebElement> TRBigdata4 = Bigdata4.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDBigdata4;
                            foreach (IWebElement row4 in TRBigdata4)
                            {
                                TDBigdata4 = row4.FindElements(By.TagName("td"));

                                if ( TDBigdata4.Count != 0 && TDBigdata4.Count == 5 && !row4.Text.Contains("Tax Year"))
                                {
                                    Taxyearhis = TDBigdata4[0].Text;
                                    Taxablevalue = TDBigdata4[2].Text;
                                    Exemptions = TDBigdata4[3].Text;
                                    Totaltax = TDBigdata4[4].Text;

                                    string Taxhistorydetails = Taxyearhis.Trim() + "~" + Taxablevalue.Trim() + "~" + Exemptions.Trim() + "~" + Totaltax.Trim();
                                    gc.insert_date(orderNumber, Parcelnum, 1526, Taxhistorydetails, 1, DateTime.Now);
                                }
                            }
                        }
                        catch { }
                        //Delinquent Taxes
                        try
                        {

                            driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div/div/div/span/ul/li[4]/a")).Click();
                            string Certificate = "", year = "", Status1 = "", Balance1 = "", Balance2 = "", title = "";
                            gc.CreatePdf(orderNumber, Parcelnum, "View Bill Yearwise" + i, driver, "FL", "Lee");
                            IWebElement Bigdata5 = driver.FindElement(By.Id("stacktable"));
                            IList<IWebElement> TRBigdata5 = Bigdata5.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDBigdata5;
                            foreach (IWebElement row5 in TRBigdata5)
                            {
                                TDBigdata5 = row5.FindElements(By.TagName("td"));
                                if (TDBigdata5.Count != 0 && TDBigdata5.Count == 6 && row5.Text.Contains("Certificate"))
                                {
                                    title = TDBigdata5[0].Text + "~" + TDBigdata5[1].Text + "~" + TDBigdata5[3].Text + "~" + TDBigdata5[4].Text + "~" + TDBigdata5[5].Text;
                                    db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + title + "' where Id = '" + 1533 + "'");
                                }
                                if (TDBigdata5.Count != 0 && TDBigdata5.Count == 6 && !row5.Text.Contains("Certificate"))
                                {
                                    Certificate = TDBigdata5[0].Text;
                                    year = TDBigdata5[1].Text;
                                    Status1 = TDBigdata5[3].Text;
                                    Balance1 = TDBigdata5[4].Text;
                                    Balance2 = TDBigdata5[5].Text;

                                    string UTaxDetails = Certificate.Trim() + "~" + year.Trim() + "~" + Status1.Trim() + "~" + Balance1.Trim() + "~" + Balance2.Trim();
                                    gc.insert_date(orderNumber, Parcelnum, 1533, UTaxDetails, 1, DateTime.Now);
                                }
                            }
                        }
                        catch { }
                        //Payments Details
                        try
                        {
                            driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div/div/div/span/ul/li[3]/a")).Click();
                            Thread.Sleep(2000);
                            string title1 = "", Paymenttaxyear = "", PaymentAccount = "", Processed = "", AsPaid = "", AmountPaid = "";
                            gc.CreatePdf(orderNumber, Parcelnum, "Payment Details" + i, driver, "FL", "Lee");
                            IWebElement Bigdata6 = driver.FindElement(By.Id("stacktable"));
                            IList<IWebElement> TRBigdata6 = Bigdata6.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDBigdata6;
                            foreach (IWebElement row6 in TRBigdata6)
                            {
                                TDBigdata6 = row6.FindElements(By.TagName("td"));
                                if (TDBigdata6.Count != 0 && TDBigdata6.Count == 5 && row6.Text.Contains("Tax Year"))
                                {
                                    title1 = TDBigdata6[0].Text.Trim() + "~" + TDBigdata6[2].Text.Trim() + "~" + TDBigdata6[3].Text.Trim() + "~" + TDBigdata6[4].Text.Trim();
                                    db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + title1 + "' where Id = '" + 1601 + "'");
                                }
                                if (TDBigdata6.Count != 0 && TDBigdata6.Count == 5 && !row6.Text.Contains("Tax Year"))
                                {
                                    Paymenttaxyear = TDBigdata6[0].Text;
                                    Processed = TDBigdata6[2].Text;
                                    AsPaid = TDBigdata6[3].Text;
                                    AmountPaid = TDBigdata6[4].Text;
                                    string UTaxDetails = Paymenttaxyear + "~" + Processed + "~" + AsPaid + "~" + AmountPaid;
                                    gc.insert_date(orderNumber, Parcelnum, 1601, UTaxDetails, 1, DateTime.Now);
                                }

                            }
                        }
                        catch { }

                        i++;
                    }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Quit();
                    gc.mergpdf(orderNumber, "FL", "Lee");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "FL", "Lee", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
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