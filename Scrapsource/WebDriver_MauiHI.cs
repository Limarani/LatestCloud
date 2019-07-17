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
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_MauiHI
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        public string FTP_MauiHI(string houseno, string Direction, string sname, string stype, string account, string parcelNumber, string ownername, string searchType, string orderNumber)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            string outparcelno = "", taxparcel = "", mailingaddress = "", tax_district = "", siteaddr = "", legal_desc = "", year_built = "", propuse = "", par1 = "", par2 = "";
            string Building_Value = "", LandValue = "", JustValue = "", cctaxyear = "", Cap = "", AssessedValue = "", Exemption = "", TaxableValue = "";
            string strBill = "-", strBalance = "-", strBillDate = "-", strBillPaid = "-";
            string taxowner = "", tax_addr = "", accno = "", alterkey = "", millagecode = "", milagerate = "", ctax_year = "", cpaiddate = "", cpaidamount = "", creceipt = "", combinedtaxamount = "", ceffdate = "", grosstax = "", ifpaidby = "", pleasepay = "";
            string taxauth1 = "", taxauth2 = "", taxauth3 = "", taxauth4 = "", TaxYear = ""; ;
            List<string> strissuecertificate = new List<string>();
            List<string> taxhistorylink = new List<string>();
            List<string> downloadlink = new List<string>();
            List<string> taxhistorylinkinst = new List<string>();
            List<string> MailURL = new List<string>();
            string multi = "", TaxAuthority = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                //using (driver = new ChromeDriver())
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    try
                    {
                        driver.Navigate().GoToUrl("https://www.mauicounty.gov/1952/Real-Property-Assessment-Division");
                        Thread.Sleep(1000);
                        TaxAuthority = driver.FindElement(By.XPath("//*[@id='cc25b3e585-80a6-4aee-89a1-3a53a3528100']/div[1]/div/div[1]/div/div/ol/li/div[1]")).Text.Replace("Physical Address", "").Replace("\r\n", " ").Trim();
                    }
                    catch { }

                    driver.Navigate().GoToUrl("http://qpublic9.qpublic.net/hi_maui_search.php");
                    Thread.Sleep(1000);
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='appBody']/div[4]/div/div/div[3]/a[1]")).Click();
                        Thread.Sleep(2000);
                    }
                    catch { }
                    string Parcelno = "", Ownername = "", parcellocation = "";

                    if (searchType == "titleflex")
                    {

                        gc.TitleFlexSearch(orderNumber, "", ownername, "", "HI", "Maui");
                        //gc.TitleFlexSearch(orderNumber, "", "", address.Trim(), "HI", "Maui");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        if ((HttpContext.Current.Session["TitleFlex_Search"] == null))
                        {
                            HttpContext.Current.Session["Zero_Maui"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                        parcelNumber = parcelNumber.Replace("-", "");

                    }
                    //if (searchType == "address")
                    //{


                    //    try
                    //    {
                    //        driver.FindElement(By.LinkText("Search by Location Address")).Click();
                    //        Thread.Sleep(1000);


                    //        driver.FindElement(By.XPath("/html/body/form/table/tbody/tr/td[2]/table/tbody/tr[1]/td[2]/input")).SendKeys(houseno);
                    //        driver.FindElement(By.Id("streetName")).SendKeys(sname);
                    //        // driver.FindElement(By.XPath("/html/body/form/table/tbody/tr/td[2]/table/tbody/tr[3]/td[2]/input")).SendKeys(stype);
                    //        // driver.FindElement(By.XPath("/html/body/form/table/tbody/tr/td[2]/table/tbody/tr[4]/td[2]/input")).SendKeys(account);
                    //        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "HI", "Maui");
                    //        driver.FindElement(By.XPath("/html/body/form/table/tbody/tr/td[2]/table/tbody/tr[6]/td[2]/input")).SendKeys(Keys.Enter);
                    //        Thread.Sleep(1000);
                    //        //IWebElement multirecord = driver.FindElement(By.XPath("//*[@id='mMessage']"));
                    //    }
                    //    catch { }
                    //    gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "HI", "Maui");
                    //    int Max = 0;
                    //    IWebElement multiaddress = driver.FindElement(By.XPath("/html/body/table/tbody"));
                    //    IList<IWebElement> TRmultiaddress = multiaddress.FindElements(By.TagName("tr"));
                    //    IList<IWebElement> THmultiaddress = multiaddress.FindElements(By.TagName("th"));
                    //    IList<IWebElement> TDmultiaddress;

                    //    if (TRmultiaddress.Count > 28)
                    //    {
                    //        HttpContext.Current.Session["multiParcel_Maui_Maximum"] = "Maimum";
                    //        return "Maximum";
                    //        Max++;
                    //    }
                    //    //if (TRmultiaddress.Count == 8)
                    //    //{
                    //    //    IWebElement multiclick = driver.FindElement(By.XPath("/html/body/table/tbody/tr[5]/td[1]/a"));
                    //    //    multiclick.Click();
                    //    //    Max++;
                    //    //}
                    //    if (TRmultiaddress.Count >= 6 && Max != 1)
                    //    {
                    //        foreach (IWebElement row in TRmultiaddress)
                    //        {
                    //            TDmultiaddress = row.FindElements(By.TagName("td"));
                    //            if (!row.Text.Contains("Parcel Number") && !row.Text.Contains("Search") && row.Text.Trim() != "" && TDmultiaddress.Count == 5)
                    //            {
                    //                try
                    //                {
                    //                    Parcelno = TDmultiaddress[0].Text.Trim();
                    //                    Ownername = TDmultiaddress[1].Text.Trim();
                    //                    parcellocation = TDmultiaddress[2].Text.Trim();
                    //                    string Multi = Ownername + "~" + parcellocation;
                    //                    gc.insert_date(orderNumber, Parcelno, 1731, Multi, 1, DateTime.Now);
                    //                    Max++;
                    //                }
                    //                catch { }

                    //            }

                    //        }

                    //    }
                    //    if (Max == 1)
                    //    {
                    //        IWebElement multiclick = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[1]/a"));
                    //        multiclick.Click();
                    //        Thread.Sleep(2000);
                    //    }
                    //    if (Max > 1)
                    //    {
                    //        HttpContext.Current.Session["multiParcel_Maui"] = "Yes";
                    //        driver.Quit();
                    //        return "MultiParcel";
                    //    }
                    //    if (Max == 0)
                    //    {
                    //        HttpContext.Current.Session["Zero_Maui"] = "Zero";
                    //        driver.Quit();
                    //        return "No Data Found";
                    //    }
                    //}

                    //if (searchType == "parcel")
                    //{

                    //    try
                    //    {
                    //        driver.FindElement(By.LinkText("Search by Parcel Number")).SendKeys(Keys.Enter);
                    //        Thread.Sleep(1000);
                    //    }
                    //    catch { }
                    //    string s1 = "", s2 = "", s3 = "", s4 = "", s5 = "", s6 = "";
                    //    if (Convert.ToInt16(parcelNumber.Replace("-", "").Count()) != 12)
                    //    {
                    //        string[] parcelSplit = parcelNumber.Split('-');
                    //        s1 = parcelSplit[0];
                    //        s2 = parcelSplit[1];
                    //        s3 = parcelSplit[2];
                    //        s4 = parcelSplit[3];
                    //        s5 = parcelSplit[4];
                    //        s6 = parcelSplit[5];
                    //        parcelNumber = s2 + s3 + s4 + s5 + s6;
                    //    }
                    //    else
                    //    {
                    //        parcelNumber = parcelNumber.Replace("-", "");
                    //    }
                    //    driver.FindElement(By.XPath("/html/body/form/table/tbody/tr/td/input[2]")).SendKeys(parcelNumber);
                    //    gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "HI", "Maui");
                    //    driver.FindElement(By.XPath("/html/body/form/table/tbody/tr/td/input[5]")).SendKeys(Keys.Enter);
                    //    Thread.Sleep(1000);
                    //    gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Result", driver, "HI", "Maui");


                    //    int Max = 0;
                    //    try
                    //    {
                    //        IWebElement multiaddress = driver.FindElement(By.XPath("/html/body/table/tbody"));
                    //        IList<IWebElement> TRmultiaddress = multiaddress.FindElements(By.TagName("tr"));
                    //        IList<IWebElement> THmultiaddress = multiaddress.FindElements(By.TagName("th"));
                    //        IList<IWebElement> TDmultiaddress;


                    //        if (TRmultiaddress.Count > 28)
                    //        {
                    //            HttpContext.Current.Session["multiParcel_Maui_Maximum"] = "Maimum";
                    //            return "Maximum";
                    //            Max++;
                    //        }
                    //        //if (TRmultiaddress.Count == 8)
                    //        //{
                    //        //    IWebElement multiclick = driver.FindElement(By.XPath("/html/body/table/tbody/tr[5]/td[1]/a"));
                    //        //    multiclick.Click();
                    //        //    Max++;
                    //        //}
                    //        if (TRmultiaddress.Count >= 6 && Max != 1)
                    //        {
                    //            foreach (IWebElement row in TRmultiaddress)
                    //            {
                    //                TDmultiaddress = row.FindElements(By.TagName("td"));
                    //                if (!row.Text.Contains("Parcel Number") && !row.Text.Contains("Search") && row.Text.Trim() != "" && TDmultiaddress.Count == 5)
                    //                {
                    //                    try
                    //                    {
                    //                        Parcelno = TDmultiaddress[0].Text.Trim();
                    //                        Ownername = TDmultiaddress[1].Text.Trim();
                    //                        parcellocation = TDmultiaddress[2].Text.Trim();
                    //                        string Multi = Ownername + "~" + parcellocation;
                    //                        gc.insert_date(orderNumber, Parcelno, 1731, Multi, 1, DateTime.Now);
                    //                        Max++;
                    //                    }
                    //                    catch { }

                    //                }

                    //            }

                    //        }
                    //        if (Max == 1)
                    //        {
                    //            IWebElement multiclick = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[1]/a"));
                    //            multiclick.Click();
                    //            Thread.Sleep(2000);
                    //        }
                    //        if (Max > 1)
                    //        {
                    //            HttpContext.Current.Session["multiParcel_Maui"] = "Yes";
                    //            driver.Quit();
                    //            return "MultiParcel";
                    //        }
                    //        if (Max == 0)
                    //        {
                    //            HttpContext.Current.Session["Zero_Maui"] = "Zero";
                    //            driver.Quit();
                    //            return "No Data Found";
                    //        }
                    //    }
                    //    catch { }
                    //}

                    //if (searchType == "ownername")
                    //{

                    //    try
                    //    {
                    //        driver.FindElement(By.LinkText("Search by Owner Name")).SendKeys(Keys.Enter);
                    //        Thread.Sleep(1000);
                    //    }
                    //    catch { }

                    //    driver.FindElement(By.XPath("/html/body/form/table/tbody/tr/td/input[2]")).SendKeys(ownername);
                    //    gc.CreatePdf_WOP(orderNumber, "Ownername Search", driver, "HI", "Maui");
                    //    driver.FindElement(By.XPath("/html/body/form/table/tbody/tr/td/input[5]")).SendKeys(Keys.Enter);
                    //    Thread.Sleep(1000);
                    //    gc.CreatePdf_WOP(orderNumber, "Ownername Search Result", driver, "HI", "Maui");

                    //    string ParcelNum = "", Owner_Name = "", ParcelLocation = "";
                    //    int Max = 0;
                    //    try
                    //    {
                    //        IWebElement multiaddress = driver.FindElement(By.XPath("/html/body/table/tbody"));
                    //        IList<IWebElement> TRmultiaddress = multiaddress.FindElements(By.TagName("tr"));
                    //        IList<IWebElement> THmultiaddress = multiaddress.FindElements(By.TagName("th"));
                    //        IList<IWebElement> TDmultiaddress;


                    //        if (TRmultiaddress.Count > 28)
                    //        {
                    //            HttpContext.Current.Session["multiParcel_Maui_Maximum"] = "Maimum";
                    //            return "Maximum";
                    //            Max++;
                    //        }

                    //        if (TRmultiaddress.Count >= 6 && Max != 1)
                    //        {
                    //            foreach (IWebElement row in TRmultiaddress)
                    //            {
                    //                TDmultiaddress = row.FindElements(By.TagName("td"));
                    //                if (!row.Text.Contains("Parcel Number") && !row.Text.Contains("Search") && row.Text.Trim() != "" && TDmultiaddress.Count == 5)
                    //                {
                    //                    try
                    //                    {
                    //                        Parcelno = TDmultiaddress[0].Text.Trim();
                    //                        Ownername = TDmultiaddress[1].Text.Trim();
                    //                        parcellocation = TDmultiaddress[2].Text.Trim();
                    //                        string Multi = Ownername + "~" + parcellocation;
                    //                        gc.insert_date(orderNumber, Parcelno, 1731, Multi, 1, DateTime.Now);
                    //                        Max++;
                    //                    }
                    //                    catch { }

                    //                }

                    //            }

                    //        }
                    //        if (Max == 1)
                    //        {
                    //            IWebElement multiclick = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[1]/a"));
                    //            multiclick.Click();
                    //            Thread.Sleep(2000);
                    //        }
                    //        if (Max > 1)
                    //        {
                    //            HttpContext.Current.Session["multiParcel_Maui"] = "Yes";
                    //            driver.Quit();
                    //            return "Multiparcel";
                    //        }
                    //        if (Max == 0)
                    //        {
                    //            HttpContext.Current.Session["Zero_Maui"] = "Zero";
                    //            driver.Quit();
                    //            return "No Record Found";
                    //        }
                    //    }
                    //    catch { }
                    //}

                    //// Property Details

                    //string propertydata = driver.FindElement(By.XPath("/html/body/center[2]/table[2]/tbody")).Text;
                    //string LocationAddress = "", Yearbuilt = "", LandArea = "", MailingAddress = "", Propertyclass = "";
                    //string OwnerName = "", LegalInformation = "", NeighborhoodCode = "", strYearbuilt = "";

                    //parcelNumber = gc.Between(propertydata, "Parcel Number", "Location Address").Trim();
                    //OwnerName = gc.Between(propertydata, "Owner Name", "Today's Date").Replace("Fee Owner", "").Replace("Show All Owners and Addresses", "").Trim();
                    //LocationAddress = gc.Between(propertydata, "Location Address", "Parcel Map").Trim();
                    //MailingAddress = gc.Between(propertydata, "Mailing Address", "Parcel Number").Trim();
                    //LandArea = gc.Between(propertydata, "Land Area", "Legal Information").Trim();
                    //NeighborhoodCode = gc.Between(propertydata, "Neighborhood Code", "Land Area").Trim();
                    //LegalInformation = gc.Between(propertydata, "Legal Information", "Parcel Note").Trim();
                    //try
                    //{
                    //    strYearbuilt = driver.FindElement(By.XPath("/html/body/center[2]/table[6]/tbody")).Text;

                    //}
                    //catch { }
                    //try
                    //{
                    //    if (!strYearbuilt.Contains("No improvement information available for this parcel"))
                    //    {
                    //        IWebElement Iyearbuilt = driver.FindElement(By.XPath("/html/body/center[2]/table[5]/tbody/tr[3]/td[3]"));
                    //        Yearbuilt = Iyearbuilt.Text.Trim();
                    //    }

                    //}
                    //catch { }

                    //string propertydetails = OwnerName + "~" + LocationAddress + "~" + MailingAddress + "~" + LegalInformation + "~" + Yearbuilt;
                    //gc.insert_date(orderNumber, parcelNumber, 1727, propertydetails, 1, DateTime.Now);
                    //gc.CreatePdf(orderNumber, parcelNumber, "Assessment Details", driver, "HI", "Maui");

                    //// Assessment Details
                    //string valuetype = "", Information = "";
                    //try
                    //{
                    //    driver.FindElement(By.LinkText("Show Historical Assessments")).Click();
                    //    Thread.Sleep(4000);
                    //    gc.CreatePdf(orderNumber, parcelNumber, "Assessment History Details", driver, "HI", "Maui");
                    //}
                    //catch { }

                    //try
                    //{
                    //    IWebElement Assessmentdetails = driver.FindElement(By.XPath("/html/body/center[2]/table[3]/tbody"));
                    //    IList<IWebElement> TRAssessmentdetails = Assessmentdetails.FindElements(By.TagName("tr"));
                    //    IList<IWebElement> THAssessmentdetails = Assessmentdetails.FindElements(By.TagName("th"));
                    //    IList<IWebElement> TDAssessmentdetails;
                    //    foreach (IWebElement row in TRAssessmentdetails)
                    //    {
                    //        TDAssessmentdetails = row.FindElements(By.TagName("td"));
                    //        if (TDAssessmentdetails.Count != 0 && !row.Text.Contains("Assessment Information") && row.Text.Trim() != "" && !row.Text.Contains("Year") && TDAssessmentdetails.Count == 9)
                    //        {
                    //            string AssessmentDetail = TDAssessmentdetails[0].Text + "~" + TDAssessmentdetails[1].Text + "~" + TDAssessmentdetails[2].Text + "~" + TDAssessmentdetails[3].Text + "~" + TDAssessmentdetails[4].Text + "~" + TDAssessmentdetails[5].Text + "~" + TDAssessmentdetails[6].Text + "~" + TDAssessmentdetails[7].Text + "~" + TDAssessmentdetails[8].Text;
                    //            gc.insert_date(orderNumber, parcelNumber, 1728, AssessmentDetail, 1, DateTime.Now);

                    //        }
                    //    }

                    //}
                    //catch { }




                    //// Tax History Taxes
                    //int tyear = 0;
                    //try
                    //{
                    //    IWebElement ITaxyear = driver.FindElement(By.XPath("/html/body/center[2]/table[5]/tbody/tr[1]/td/font[1]/a"));
                    //    TaxYear = ITaxyear.Text.Replace("Tax Payments", "").Trim();
                    //    tyear = Convert.ToInt16(TaxYear);
                    //}
                    //catch { }
                    //if (tyear == 0)
                    //{
                    //    try
                    //    {
                    //        TaxYear = driver.FindElement(By.XPath("/html/body/center[2]/table[6]/tbody/tr[1]/td/font[1]/a")).Text;
                    //        TaxYear = TaxYear.Replace("Tax Payments", "").Trim();
                    //        tyear = Convert.ToInt16(TaxYear);
                    //    }
                    //    catch { }
                    //}

                    ////read all tables

                    //IList<IWebElement> tables = driver.FindElements(By.XPath("/html/body/center[2]/table"));
                    //int count = tables.Count;
                    //int j = 0;
                    //foreach (IWebElement tab in tables)
                    //{
                    //    if (tab.Text.Contains("Current Tax Bill Information"))
                    //    {
                    //        IList<IWebElement> currtaxbill = tab.FindElements(By.XPath("tbody/tr"));
                    //        IList<IWebElement> TDCurrentTax;
                    //        foreach (IWebElement row in currtaxbill)
                    //        {
                    //            TDCurrentTax = row.FindElements(By.TagName("td"));
                    //            if (TDCurrentTax.Count != 0 && !row.Text.Contains("Current Tax") && row.Text.Trim() != "" && !row.Text.Contains("Tax Period") && !row.Text.Contains("pay online"))
                    //            {
                    //                if (TDCurrentTax.Count == 10)
                    //                {
                    //                    string CurrentTaxDetail1 = TDCurrentTax[0].Text + "~" + TDCurrentTax[1].Text + "~" + TDCurrentTax[2].Text + "~" + TDCurrentTax[3].Text + "~" + TDCurrentTax[4].Text + "~" + TDCurrentTax[5].Text + "~" + TDCurrentTax[6].Text + "~" + TDCurrentTax[7].Text + "~" + TDCurrentTax[8].Text + "~" + TDCurrentTax[9].Text;
                    //                    gc.insert_date(orderNumber, parcelNumber, 1738, CurrentTaxDetail1, 1, DateTime.Now);
                    //                }

                    //                if (TDCurrentTax.Count == 2)
                    //                {
                    //                    string CurrentTaxDetail2 = "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + TDCurrentTax[0].Text;
                    //                    gc.insert_date(orderNumber, parcelNumber, 1738, CurrentTaxDetail2, 1, DateTime.Now);
                    //                }
                    //            }

                    //        }
                    //    }
                    //    else if (tab.Text.Contains("Historical Tax Information"))
                    //    {
                    //        int u = 0;
                    //        IList<IWebElement> taxhistoryvalue1 = tab.FindElements(By.XPath("tbody/tr"));
                    //        IList<IWebElement> TDTaxHistroy1;
                    //        foreach (IWebElement row in taxhistoryvalue1)
                    //        {
                    //            TDTaxHistroy1 = row.FindElements(By.TagName("td"));
                    //            if (TDTaxHistroy1.Count != 0 && !row.Text.Contains("Historical Tax") && row.Text.Trim() != "" && !row.Text.Contains("Penalty") && !row.Text.Contains("Click") && TDTaxHistroy1.Count == 7)
                    //            {
                    //                if (u < 3)
                    //                {
                    //                    IWebElement Ilink = TDTaxHistroy1[0].FindElement(By.TagName("a"));
                    //                    string URL = Ilink.GetAttribute("href");
                    //                    MailURL.Add(URL);
                    //                    u++;
                    //                }

                    //                string TaxHistoryDetail = TDTaxHistroy1[0].Text + "~" + TDTaxHistroy1[1].Text + "~" + TDTaxHistroy1[2].Text + "~" + TDTaxHistroy1[3].Text + "~" + TDTaxHistroy1[4].Text + "~" + TDTaxHistroy1[5].Text + "~" + TDTaxHistroy1[6].Text;
                    //                gc.insert_date(orderNumber, parcelNumber, 1729, TaxHistoryDetail, 1, DateTime.Now);
                    //            }

                    //        }
                    //    }

                    //    else if (tab.Text.Contains("Sales Information"))
                    //    {
                    //        IList<IWebElement> saleshisvalue = tab.FindElements(By.XPath("tbody/tr"));
                    //        IList<IWebElement> tdsaleshis;
                    //        foreach (IWebElement row in saleshisvalue)
                    //        {
                    //            tdsaleshis = row.FindElements(By.TagName("td"));
                    //            if (tdsaleshis.Count != 0 && !row.Text.Contains("Sale Date") && row.Text.Trim() != "" && !row.Text.Contains("No sales information associated") && tdsaleshis.Count == 9)
                    //            {
                    //                string salesDetail = tdsaleshis[0].Text + "~" + tdsaleshis[1].Text + "~" + tdsaleshis[2].Text + "~" + tdsaleshis[3].Text + "~" + tdsaleshis[4].Text + "~" + tdsaleshis[5].Text + "~" + tdsaleshis[6].Text + "~" + tdsaleshis[7].Text + "~" + tdsaleshis[8].Text;
                    //                gc.insert_date(orderNumber, parcelNumber, 1820, salesDetail, 1, DateTime.Now);
                    //            }

                    //        }
                    //    }
                    //}
                    //int c = 0;
                    //foreach (string suburl in MailURL)
                    //{
                    //    driver.Navigate().GoToUrl(suburl);
                    //    Thread.Sleep(3000);
                    //    IList<IWebElement> ptables = driver.FindElements(By.XPath("/html/body/center[2]/table"));
                    //    int pcount = ptables.Count;

                    //    foreach (IWebElement tabp in ptables)
                    //    {
                    //        if (tabp.Text.Contains("Tax Payment Information "))
                    //        {
                    //            gc.CreatePdf(orderNumber, parcelNumber, "Tax Payment Information" + c, driver, "HI", "Maui");
                    //            IList<IWebElement> taxbill = tabp.FindElements(By.XPath("tbody/tr"));
                    //            IList<IWebElement> TDTax;
                    //            foreach (IWebElement row in taxbill)
                    //            {
                    //                TDTax = row.FindElements(By.TagName("td"));

                    //                if (TDTax.Count != 0 && !row.Text.Contains("Tax Payment") && row.Text.Trim() != "" && !row.Text.Contains("Penalty") && !row.Text.Contains("Totals") && TDTax.Count == 6)
                    //                {
                    //                    string TaxPayHistoryDetail1 = TDTax[0].Text + "~" + TDTax[1].Text + "~" + TDTax[2].Text + "~" + TDTax[3].Text + "~" + TDTax[4].Text + "~" + TDTax[5].Text + "~" + TaxAuthority;
                    //                    gc.insert_date(orderNumber, parcelNumber, 1730, TaxPayHistoryDetail1, 1, DateTime.Now);
                    //                }

                    //            }

                    //        }
                    //    }
                    //    c++;

                    //}

                    //DB Columns
                    //Owner Name~Property Address~Mailing Address~Legal Description~Year Built---- - 1---- 1727
                    //Assessment Year~Property Class~Market Land Value~Agricultural Land Value~Assessed Land Value~Building Value~Total Assessed Value~Total Exemption Value~Total Net Taxable Value-- - 2--1728
                    //Tax Year~Tax Amount~Paid Amount~Penalty~Interest~Other~Due Amount-- 3--1729
                    //Tax Year~Paid Date~Paid Amount~Penalty~Interest~Other~Taxing Authority-- 4--1730
                    //Tax Period~Description~Original Due Date~Taxes Assessment~Tax Credits~Net Tax~Penalty~Interest~Other~Amount Due-- - 5-- - 1738



                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='appBody']/div[5]/div/div/div[3]/a")).Click();
                        Thread.Sleep(2000);
                    }
                    catch { }
                    string address = "";
                    if (Direction != "")
                    {
                        address = houseno + " " + Direction + " " + sname + " " + stype + " " + account;
                    }
                    else
                    {
                        address = houseno + " " + sname + " " + stype + " " + account;
                    }
                    if (searchType == "address")
                    {
                        driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_txtAddress")).SendKeys(address);

                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "HI", "Maui");
                        driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        try
                        {
                            string Owner = "", Property_Address = "", MultiAddress_details = "";
                            IWebElement MultiAddressTB = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_ctl01_gvwParcelResults']/tbody"));
                            IList<IWebElement> MultiAddressTR = MultiAddressTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiAddressTD;
                            gc.CreatePdf_WOP(orderNumber, "Multi Address search", driver, "HI", "Maui");
                            int AddressmaxCheck = 0;

                            foreach (IWebElement MultiAddress in MultiAddressTR)
                            {
                                if (AddressmaxCheck <= 25)
                                {
                                    MultiAddressTD = MultiAddress.FindElements(By.TagName("td"));
                                    if (MultiAddressTD.Count != 0)
                                    {
                                        Parcelno = MultiAddressTD[1].Text;
                                        Owner = MultiAddressTD[3].Text;
                                        Property_Address = MultiAddressTD[4].Text;

                                        MultiAddress_details = Owner + "~" + Property_Address;
                                        gc.insert_date(orderNumber, Parcelno, 1731, MultiAddress_details, 1, DateTime.Now);
                                    }
                                    AddressmaxCheck++;
                                }
                            }
                            if (MultiAddressTR.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Maui_Maximum"] = "Maimum";
                                driver.Quit();
                                return "Maximum";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiParcel_Maui"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }

                        }
                        catch
                        { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("ctlBodyPane_noDataList_pnlNoResults")).Text;
                            if (nodata.Contains("No results match your search criteria."))
                            {
                                HttpContext.Current.Session["Zero_Maui"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.Id("ctlBodyPane_ctl02_ctl01_txtParcelID")).SendKeys(parcelNumber);

                        gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearch", driver, "HI", "Maui");
                        driver.FindElement(By.Id("ctlBodyPane_ctl02_ctl01_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("ctlBodyPane_noDataList_pnlNoResults")).Text;
                            if (nodata.Contains("No results match your search criteria."))
                            {
                                HttpContext.Current.Session["Zero_Maui"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("ctlBodyPane_ctl00_ctl01_txtName")).SendKeys(ownername);

                        gc.CreatePdf_WOP(orderNumber, "Owner search", driver, "HI", "Maui");
                        driver.FindElement(By.Id("ctlBodyPane_ctl00_ctl01_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        try
                        {
                            string Owner = "", Property_Address = "", MultiAddress_details = "";
                            IWebElement MultiAddressTB = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_ctl01_gvwParcelResults']/tbody"));
                            IList<IWebElement> MultiAddressTR = MultiAddressTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiAddressTD;
                            gc.CreatePdf_WOP(orderNumber, "Multi Owner search", driver, "HI", "Maui");
                            int AddressmaxCheck = 0;

                            foreach (IWebElement MultiAddress in MultiAddressTR)
                            {
                                if (AddressmaxCheck <= 25)
                                {
                                    MultiAddressTD = MultiAddress.FindElements(By.TagName("td"));
                                    if (MultiAddressTD.Count != 0)
                                    {
                                        Parcelno = MultiAddressTD[1].Text;
                                        Owner = MultiAddressTD[3].Text;
                                        Property_Address = MultiAddressTD[4].Text;

                                        MultiAddress_details = Owner + "~" + Property_Address;
                                        gc.insert_date(orderNumber, Parcelno, 1731, MultiAddress_details, 1, DateTime.Now);
                                    }
                                    AddressmaxCheck++;
                                }
                            }
                            if (MultiAddressTR.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Maui_Maximum"] = "Maimum";
                                driver.Quit();
                                return "Maximum";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiParcel_Maui"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                        }
                        catch
                        { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("ctlBodyPane_noDataList_pnlNoResults")).Text;
                            if (nodata.Contains("No results match your search criteria."))
                            {
                                HttpContext.Current.Session["Zero_Maui"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    string PropertyAddress = "", NeighborhoodCode = "", LegalInformation = "", LandArea = "", ParcelNote = "", MaillingAddress = "", OwnerName = "", YearBuilt = "";
                    //Property Details
                    IWebElement tbmulti11 = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_mSection']/div/table[1]/tbody"));
                    IList<IWebElement> TRmulti11 = tbmulti11.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDmulti11;
                    foreach (IWebElement row in TRmulti11)
                    {

                        TDmulti11 = row.FindElements(By.TagName("td"));
                        if (TDmulti11.Count != 0)
                        {
                            if (row.Text.Contains("Parcel Number"))
                            {
                                parcelNumber = TDmulti11[1].Text;
                            }
                            if (row.Text.Contains("Location Address"))
                            {
                                try
                                {
                                    PropertyAddress = gc.Between(tbmulti11.Text, "Location Address", "Neighborhood Code");
                                }
                                catch { }
                            }
                            if (row.Text.Contains("Neighborhood Code"))
                            {
                                NeighborhoodCode = TDmulti11[1].Text;
                            }
                            if (row.Text.Contains("Legal Information"))
                            {
                                LegalInformation = TDmulti11[1].Text;
                            }
                            if (row.Text.Contains("Land Area"))
                            {
                                LandArea = TDmulti11[1].Text;
                            }
                            if (row.Text.Contains("Parcel Note"))
                            {
                                ParcelNote = TDmulti11[1].Text;
                            }

                        }
                    }

                    try
                    {
                        MaillingAddress = driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_divMailingAddress")).Text.Replace("Mailing Address\r\n", "").Replace("\r\n", " ").Trim();
                    }
                    catch { }

                    try
                    {
                        string[] owner = driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_lblOtherNames")).Text.Replace("Owner Names\r\n", "").Replace("    ", "").Replace("Fee Owner\r\n", "~").Replace("Fee Owner", "~").Trim().Split('~');
                        if (owner.Count() == 3)
                        {
                            OwnerName = owner[0];
                        }
                        if (owner.Count() == 2)
                        {
                            OwnerName = owner[0];
                        }
                    }
                    catch
                    { }
                    try
                    {
                        IWebElement IyearBuilt = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl08_mSection']/div"));
                        IList<IWebElement> TRIyearBuilt = IyearBuilt.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDIyearBuilt;
                        foreach (IWebElement built in TRIyearBuilt)
                        {

                            TDIyearBuilt = built.FindElements(By.TagName("td"));
                            if (TDIyearBuilt.Count != 0)
                            {
                                if (built.Text.Contains("Year Built") && !built.Text.Contains("Eff Year Built"))
                                {
                                    YearBuilt = TDIyearBuilt[1].Text;
                                    break;
                                }
                            }
                        }
                    }
                    catch
                    { }

                    string PropertyDetails = PropertyAddress + "~" + MaillingAddress + "~" + OwnerName + "~" + NeighborhoodCode + "~" + LegalInformation + "~" + LandArea + "~" + ParcelNote + "~" + YearBuilt + "~" + TaxAuthority;
                    gc.CreatePdf(orderNumber, parcelNumber, "Property Details", driver, "HI", "Maui");
                    gc.insert_date(orderNumber, parcelNumber, 1727, PropertyDetails, 1, DateTime.Now);
                    //Property Address~Mailling Address~Owner Name~Neighborhood Code~Legal Information~Land Area~Parcel Note~Year Built~Tax Authority
                    //Assessment Details
                    try
                    {
                        IWebElement clickfirst = driver.FindElement(By.Id("btndivHistorical"));
                        IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                        js.ExecuteScript("arguments[0].click();", clickfirst);
                        Thread.Sleep(2000);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Property Assessment Details", driver, "HI", "Maui");
                    }
                    catch { }
                    IWebElement AssmTb = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl02_ctl01_gvValuationHistorical']/tbody"));
                    IList<IWebElement> AssmTr = AssmTb.FindElements(By.TagName("tr"));
                    IList<IWebElement> AssmTd;
                    foreach (IWebElement Assm in AssmTr)
                    {
                        AssmTd = Assm.FindElements(By.TagName("td"));
                        if (AssmTd.Count != 0 && Assm.Text.Trim() != "")
                        {
                            string AssessmentDetails = AssmTd[0].Text + "~" + AssmTd[1].Text + "~" + AssmTd[2].Text + "~" + AssmTd[3].Text + "~" + AssmTd[4].Text + "~" + AssmTd[5].Text + "~" + AssmTd[6].Text + "~" + AssmTd[7].Text + "~" + AssmTd[8].Text;
                            gc.insert_date(orderNumber, parcelNumber, 1728, AssessmentDetails, 1, DateTime.Now);
                            //Year~Tax Class~Market Land Value~Agricultural Land Value~Assessed Land Value~Building Value~Total Assessed Value~Total Exemption Value~Total Net Taxable Value 
                        }
                    }

                    //IWebElement ILand = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl04_ctl01_gvwLand']/tbody"));
                    //IList<IWebElement> ILandTr = ILand.FindElements(By.TagName("tr"));
                    //IList<IWebElement> ILandTd;
                    //foreach (IWebElement land in ILandTr)
                    //{
                    //    ILandTd = land.FindElements(By.TagName("td"));
                    //    if (ILandTd.Count != 0)
                    //    {
                    //        string LandDetails = ILandTd[0].Text + "~" + ILandTd[1].Text + "~" + ILandTd[2].Text + "~" + ILandTd[3].Text;
                    //        gc.insert_date(orderNumber, parcelNumber, 1729, LandDetails, 1, DateTime.Now);
                    //        //PropertyClass~Square Footage~Acerage~Agricultural USe Indicator
                    //    }
                    //}

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    IWebElement ISale = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl11_ctl01_gvwSales']/tbody"));
                    IList<IWebElement> ISaleTr = ISale.FindElements(By.TagName("tr"));
                    IList<IWebElement> ISaleTd;
                    foreach (IWebElement sale in ISaleTr)
                    {
                        ISaleTd = sale.FindElements(By.TagName("td"));
                        if (ISaleTd.Count != 0 && sale.Text.Trim() != "")
                        {
                            string SaleDetails = ISaleTd[0].Text + "~" + ISaleTd[1].Text + "~" + ISaleTd[2].Text + "~" + ISaleTd[3].Text + "~" + ISaleTd[4].Text + "~" + ISaleTd[5].Text + "~" + ISaleTd[6].Text + "~" + ISaleTd[7].Text;
                            gc.insert_date(orderNumber, parcelNumber, 1730, SaleDetails, 1, DateTime.Now);
                            //Sale Date~Price~Instrument Number~Instrument Type~Valid Sale or Reason~Document Type~Record Date~Land Court~Land Court Cert
                        }
                    }

                    IWebElement ITaxHistory = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl05_ctl01_gvwHistoricalTax']/tbody"));
                    IList<IWebElement> ITaxHistoryTr = ITaxHistory.FindElements(By.TagName("tr"));
                    IList<IWebElement> ITaxHistoryTd;
                    foreach (IWebElement history in ITaxHistoryTr)
                    {
                        ITaxHistoryTd = history.FindElements(By.TagName("td"));
                        if (ITaxHistoryTd.Count != 0 && history.Text.Trim() != "")
                        {
                            string TaxHistoryDetails = ITaxHistoryTd[0].Text + "~" + ITaxHistoryTd[1].Text + "~" + ITaxHistoryTd[2].Text + "~" + ITaxHistoryTd[3].Text + "~" + ITaxHistoryTd[4].Text + "~" + ITaxHistoryTd[5].Text + "~" + ITaxHistoryTd[6].Text;
                            gc.insert_date(orderNumber, parcelNumber, 1738, TaxHistoryDetails, 1, DateTime.Now);
                            //Year~Tax~Payment and Credits~Penalty~Interest~Other~Amount Due
                        }
                    }


                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Quit();

                    gc.mergpdf(orderNumber, "HI", "Maui");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "HI", "Maui", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
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