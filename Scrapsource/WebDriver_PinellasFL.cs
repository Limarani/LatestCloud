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
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_PinellasFL
    {
        IWebDriver driver;
        GlobalClass gc = new GlobalClass();
        DBconnection db = new DBconnection();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        private object driverIE;
        private int multicount;
        private string strMultiAddress;
        private string b;
        string mul = "";
        public string FTP_Pinellas(string address, string parcelNumber, string searchType, string ownername, string orderNumber, string directParcel)

        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            string outparcelno = "", taxparcel = "", mailingaddress = "", tax_district = "", siteaddr = "", legal_desc = "", year_built = "", propuse = "", par1 = "", par2 = "";
            string Building_Value = "", LandValue = "", JustValue = "", cctaxyear = "", Cap = "", AssessedValue = "", Exemption = "", TaxableValue = "";
            string strBill = "-", strBalance = "-", strBillDate = "-", strBillPaid = "-";
            string taxowner = "", tax_addr = "", accno = "", alterkey = "", millagecode = "", milagerate = "", ctax_year = "", cpaiddate = "", cpaidamount = "", creceipt = "", combinedtaxamount = "", ceffdate = "", grosstax = "", ifpaidby = "", pleasepay = "";
            string TaxAuthority = "";
            List<string> strissuecertificate = new List<string>();
            List<string> taxhistorylink = new List<string>();
            List<string> downloadlink = new List<string>();
            List<string> taxhistorylinkinst = new List<string>();

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    TaxAuthority = "315 Court Street, 3rd Floor Clearwater, FL 33756";


                    if (searchType == "titleflex")
                    {
                        string titleaddress = address;

                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "FL", "Pinellas");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_PinellasFL"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";

                    }

                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("http://www.pcpao.org/searchbyAddress.php");
                        Thread.Sleep(4000);

                        try
                        {
                            driver.FindElement(By.XPath("/html/body/blockquote/div/p[10]/font/button[1]")).Click();
                            Thread.Sleep(3000);
                        }
                        catch { }
                        //try
                        //{
                        //    IWebElement element = driver.FindElement(By.XPath("/html/body/div[2]/table/tbody[2]/tr[2]/td"));
                        //    Actions builder = new Actions(driver);
                        //    builder.MoveToElement(element).Build().Perform();
                        //    builder.Click();
                        //    try
                        //    {
                        //        IWebElement IRealEstateClick = driver.FindElement(By.Id("dropmenudiv"));
                        //        IList<IWebElement> IRealEstateRow = IRealEstateClick.FindElements(By.TagName("a"));
                        //        IList<IWebElement> IRealEstateTD;
                        //        foreach (IWebElement realestate in IRealEstateRow)
                        //        {
                        //            IRealEstateTD = realestate.FindElements(By.TagName("a"));
                        //            if (realestate.Text.Contains("By Address"))
                        //            {
                        //                IWebElement IRealEstate = driver.FindElement(By.LinkText("By Address"));
                        //                IRealEstate.Click();
                        //                Thread.Sleep(2000);
                        //                break;
                        //            }
                        //        }
                        //    }
                        //    catch { }
                        //    Thread.Sleep(3000);
                        //    driver.SwitchTo().DefaultContent();
                        //    IWebElement IProperty = driver.FindElement(By.XPath("/html/frameset/frame[2]"));
                        //    driver.SwitchTo().Frame(IProperty);
                        //    driver.FindElement(By.XPath("/html/body/blockquote/div/p[10]/font/button[1]")).Click();
                        //    Thread.Sleep(3000);
                        //}
                        //catch { }

                        try
                        {

                            driver.FindElement(By.XPath("//*[@id='Addr2']")).SendKeys(address);


                            gc.CreatePdf_WOP(orderNumber, "Address search", driver, "FL", "Pinellas");
                            driver.FindElement(By.XPath("/html/body/div[2]/table/tbody/tr/td/form/div/p[4]/font/input[1]")).SendKeys(Keys.Enter);
                            Thread.Sleep(4000);
                            gc.CreatePdf_WOP(orderNumber, "Address search result", driver, "FL", "Pinellas");
                        }
                        catch { }

                        try
                        {
                            mul = driver.FindElement(By.XPath("/html/body/form/table[1]/tbody/tr[1]/th")).Text.Trim();
                            mul = WebDriverTest.Between(mul, "through", "of").Trim();
                            if (Convert.ToInt32(mul) > 1)
                            {
                                //multi parcel
                                IWebElement tbmulti = driver.FindElement(By.XPath("//*[@id='ITB']"));
                                IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));

                                IList<IWebElement> TDmulti;
                                foreach (IWebElement row in TRmulti)
                                {
                                    if (!row.Text.Contains("Parcel Info"))
                                    {
                                        TDmulti = row.FindElements(By.TagName("td"));
                                        if (TDmulti.Count != 0)
                                        {
                                            string multi1 = TDmulti[0].Text + "~" + TDmulti[2].Text;
                                            gc.insert_date(orderNumber, TDmulti[1].Text, 1529, multi1, 1, DateTime.Now);
                                        }
                                    }
                                }
                                HttpContext.Current.Session["multiparcel_Pinellas"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            else
                            {
                                driver.FindElement(By.XPath("//*[@id='ITB']/tr[2]/td[2]/a")).Click();
                                Thread.Sleep(4000);

                            }
                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {

                        driver.Navigate().GoToUrl("http://www.pcpao.org/searchbyPARCEL.php");
                        Thread.Sleep(4000);

                        try
                        {
                            driver.FindElement(By.XPath("/html/body/blockquote/div/p[10]/font/button[1]")).Click();
                            Thread.Sleep(3000);
                        }
                        catch { }
                        //try
                        //{
                        //    IWebElement element = driver.FindElement(By.XPath("/html/body/div[2]/table/tbody[2]/tr[2]/td"));
                        //    Actions builder = new Actions(driver);
                        //    builder.MoveToElement(element).Build().Perform();
                        //    builder.Click();
                        //    driver.FindElement(By.LinkText("By Parcel Number")).Click();
                        //    Thread.Sleep(3000);
                        //    driver.SwitchTo().DefaultContent();
                        //    IWebElement IProperty = driver.FindElement(By.XPath("/html/frameset/frame[2]"));
                        //    driver.SwitchTo().Frame(IProperty);
                        //    driver.FindElement(By.XPath("/html/body/blockquote/div/p[10]/font/button[1]")).Click();
                        //    Thread.Sleep(3000);
                        //}
                        //catch { }

                        try
                        {
                            string p1 = "", p2 = "", p3 = "", p4 = "", p5 = "", p6 = "";
                            string[] parcelsplit = parcelNumber.Split('-');
                            p1 = parcelsplit[0].Trim();
                            p2 = parcelsplit[1].Trim();
                            p3 = parcelsplit[2].Trim();
                            p4 = parcelsplit[3].Trim();
                            p5 = parcelsplit[4].Trim();
                            p6 = parcelsplit[5].Trim();

                            driver.FindElement(By.Id("sc")).SendKeys(p1);
                            driver.FindElement(By.Id("tw")).SendKeys(p2);
                            driver.FindElement(By.Id("rg")).SendKeys(p3);
                            driver.FindElement(By.Id("sb")).SendKeys(p4);
                            driver.FindElement(By.Id("bk")).SendKeys(p5);
                            driver.FindElement(By.Id("lot")).SendKeys(p6);

                            gc.CreatePdf(orderNumber, parcelNumber, "Parcel search", driver, "FL", "Pinellas");
                            driver.FindElement(By.Id("submitButtonName")).SendKeys(Keys.Enter);
                            Thread.Sleep(4000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Parcel search result", driver, "FL", "Pinellas");
                        }
                        catch { }


                        try
                        {
                            mul = driver.FindElement(By.XPath("/html/body/form/table[1]/tbody/tr[1]/th")).Text.Trim();
                            mul = WebDriverTest.Between(mul, "through", "of").Trim();
                            if (Convert.ToInt32(mul) > 1)
                            {
                                //multi parcel
                                IWebElement tbmulti = driver.FindElement(By.XPath("//*[@id='ITB']"));
                                IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));

                                IList<IWebElement> TDmulti;
                                foreach (IWebElement row in TRmulti)
                                {
                                    if (!row.Text.Contains("Parcel Info"))
                                    {
                                        TDmulti = row.FindElements(By.TagName("td"));
                                        if (TDmulti.Count != 0)
                                        {
                                            string multi1 = TDmulti[0].Text + "~" + TDmulti[2].Text;
                                            gc.insert_date(orderNumber, TDmulti[1].Text, 1529, multi1, 1, DateTime.Now);
                                        }
                                    }
                                }
                                HttpContext.Current.Session["multiparcel_Pinellas"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            else
                            {
                                driver.FindElement(By.XPath("//*[@id='ITB']/tr[2]/td[1]/a")).Click();
                                Thread.Sleep(4000);

                            }
                        }
                        catch { }
                    }
                    else if (searchType == "ownername")
                    {

                        driver.Navigate().GoToUrl("http://www.pcpao.org/searchbyNAME.php");
                        Thread.Sleep(4000);
                        try
                        {
                            driver.FindElement(By.XPath("/html/body/blockquote/div/p[10]/font/button[1]")).Click();
                            Thread.Sleep(3000);
                        }
                        catch { }
                        //try
                        //{
                        //    IWebElement element = driver.FindElement(By.XPath("/html/body/div[2]/table/tbody[2]/tr[2]/td"));
                        //    Actions builder = new Actions(driver);
                        //    builder.MoveToElement(element).Build().Perform();
                        //    builder.Click();
                        //    driver.FindElement(By.LinkText("By Name")).Click();
                        //    Thread.Sleep(3000);
                        //    driver.SwitchTo().DefaultContent();
                        //    IWebElement IProperty = driver.FindElement(By.XPath("/html/frameset/frame[2]"));
                        //    driver.SwitchTo().Frame(IProperty);
                        //    driver.FindElement(By.XPath("/html/body/blockquote/div/p[10]/font/button[1]")).Click();
                        //    Thread.Sleep(3000);
                        //}
                        //catch { }
                        try
                        {
                            driver.FindElement(By.Id("Text1")).SendKeys(ownername);

                            gc.CreatePdf_WOP(orderNumber, "Owner search", driver, "FL", "Pinellas");
                            driver.FindElement(By.XPath("/html/body/div[2]/table/tbody/tr/td/form/div/p[5]/font/input[1]")).SendKeys(Keys.Enter);
                            Thread.Sleep(4000);
                            gc.CreatePdf_WOP(orderNumber, "Owner search result", driver, "FL", "Pinellas");
                        }
                        catch { }

                        List<string> strParcelmatch = new List<string>();
                        string strparcel = "", strparcelID = "";
                        int y = 0, s = 0;
                        try
                        {
                            mul = driver.FindElement(By.XPath("/html/body/form/table[1]/tbody/tr[1]/th")).Text.Trim();
                            mul = WebDriverTest.Between(mul, "through", "of").Trim();
                            if (Convert.ToInt32(mul) > 1)
                            {
                                //multi parcel
                                IWebElement tbmulti = driver.FindElement(By.XPath("//*[@id='ITB']"));
                                IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));

                                IList<IWebElement> TDmulti;
                                foreach (IWebElement row in TRmulti)
                                {
                                    if (!row.Text.Contains("Parcel Info"))
                                    {

                                        TDmulti = row.FindElements(By.TagName("td"));
                                        if (TDmulti.Count != 0)
                                        {
                                            strparcel = TDmulti[1].Text;
                                            string multi1 = TDmulti[0].Text + "~" + TDmulti[2].Text;
                                            gc.insert_date(orderNumber, TDmulti[1].Text, 1529, multi1, 1, DateTime.Now);
                                            strparcelID = strparcel;
                                            y++;

                                            if (strParcelmatch.Any(str => str.Contains(strparcel)))
                                            {

                                                driver.FindElement(By.XPath("//*[@id='ITB']/tr[2]/td[2]/a")).Click();
                                                Thread.Sleep(4000);
                                                s++;
                                                break;
                                            }
                                            else
                                            {
                                                strParcelmatch.Add(strparcel);
                                            }
                                        }
                                        //if (y >= 2 && row.Text.Contains(strparcel))
                                        //{

                                        //}
                                    }
                                }
                                if (s == 0 && y > 1 && y < 25)
                                {
                                    HttpContext.Current.Session["multiparcel_Pinellas"] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                            }
                            else
                            {
                                driver.FindElement(By.XPath("//*[@id='ITB']/tr[2]/td[2]/a")).Click();
                                Thread.Sleep(4000);

                            }
                        }
                        catch { }
                    }

                    try
                    {
                        IWebElement Inodata = driver.FindElement(By.XPath("/html/body/form/table[1]"));
                        if(Inodata.Text.Contains("search returned no records"))
                        {
                            HttpContext.Current.Session["Nodata_PinellasFL"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    //property details


                    string Parcel_ID = "", Ownership = "", Ownership1 = "", Ownership2 = "", Site_Address = "", Legal_Description = "", TaxDistrict = "", YearBuilt = "", PropertyUse = "";
                    try
                    {
                        IWebElement IAssessment = driver.FindElement(By.XPath("/html/frameset/frame[2]"));
                        driver.SwitchTo().Frame(IAssessment);
                    }
                    catch { }
                    try
                    {
                        Parcel_ID = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[1]/td/table/tbody/tr[1]/th/table/tbody/tr/td/font[1]")).Text.Trim();
                    }
                    catch { }

                    try
                    {
                        Ownership = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[1]/td/table/tbody/tr[3]/td[1]/table/tbody/tr[2]/td[1]")).Text.Trim();
                        //Ownership2 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[1]/td/table/tbody/tr[3]/td[1]/table/tbody/tr[2]/td[1]/text()[2]")).Text.Trim();
                        // Ownership = Ownership1 + Ownership2;
                    }
                    catch { }
                    Site_Address = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[1]/td/table/tbody/tr[3]/td[1]/table/tbody/tr[2]/td[2]")).Text.Trim().Replace("\r\n", " ");
                    Legal_Description = driver.FindElement(By.XPath("//*[@id='legal']")).Text.Trim();
                    TaxDistrict = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[5]/td[1]/table/tbody/tr[2]/td[2]/a")).Text.Trim();
                    PropertyUse = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[1]/td/table/tbody/tr[4]/td/table/tbody/tr/td[1]")).Text.Replace("Property Use:", "").Trim();

                    // Yearbuilt
                    try
                    {

                        IWebElement IActualyearbuilt = driver.FindElement(By.XPath("//*[@id='bb1']/table[1]/tbody/tr[12]/td/b"));
                        YearBuilt = IActualyearbuilt.Text.Trim();
                    }
                    catch { }
                    try
                    {
                        //*[@id="bb1"]/table[1]/tbody/tr[12]/td/b
                        IWebElement IActualyearbuilt = driver.FindElement(By.XPath("//*[@id='bb1']/table[1]/tbody/tr[13]/td/b"));
                        YearBuilt = IActualyearbuilt.Text.Trim();
                    }
                    catch { }
                    string property_details = Ownership + "~" + Site_Address + "~" + Legal_Description + "~" + YearBuilt + "~" + TaxDistrict + "~" + PropertyUse;
                    gc.insert_date(orderNumber, Parcel_ID, 1517, property_details, 1, DateTime.Now);
                    gc.CreatePdf(orderNumber, Parcel_ID, "Property Details", driver, "FL", "Pinellas");

                    // Exemptions
                    IWebElement IExemptions = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td/table/tbody/tr[2]/td[1]/table/tbody"));
                    IList<IWebElement> TRIExemptions = IExemptions.FindElements(By.TagName("tr"));
                    IList<IWebElement> THIExemptions = IExemptions.FindElements(By.TagName("th"));
                    IList<IWebElement> TDIExemptions;
                    foreach (IWebElement row in TRIExemptions)
                    {
                        TDIExemptions = row.FindElements(By.TagName("td"));
                        if (TRIExemptions.Count > 1 && TDIExemptions.Count != 0 && row.Text.Trim() != "" && !row.Text.Contains("Exemption"))
                        {
                            string ExemptionDetails = TDIExemptions[0].Text + "~" + TDIExemptions[1].Text + "~" + TDIExemptions[2].Text;

                            gc.insert_date(orderNumber, Parcel_ID, 1518, ExemptionDetails, 1, DateTime.Now);
                        }
                        if (TRIExemptions.Count > 1 && row.Text.Trim() != "" && row.Text.Contains("Exemption"))
                        {
                            string ExemptionDetails = THIExemptions[0].Text + "~" + THIExemptions[1].Text + "~" + THIExemptions[2].Text;

                            gc.insert_date(orderNumber, Parcel_ID, 1518, ExemptionDetails, 1, DateTime.Now);
                        }
                    }

                    // Property History Details

                    IWebElement prohistory = driver.FindElement(By.XPath("//*[@id='valhist']/table/tbody"));
                    IList<IWebElement> TRprohistory = prohistory.FindElements(By.TagName("tr"));
                    IList<IWebElement> THprohistory = prohistory.FindElements(By.TagName("th"));
                    IList<IWebElement> TDprohistory;
                    foreach (IWebElement row in TRprohistory)
                    {
                        TDprohistory = row.FindElements(By.TagName("td"));
                        if (TRprohistory.Count > 1 && TDprohistory.Count != 0 && row.Text.Trim() != "" && !row.Text.Contains("Homestead Exemption"))
                        {
                            string PropertyHistory = TDprohistory[0].Text + "~" + TDprohistory[1].Text + "~" + TDprohistory[2].Text + "~" + TDprohistory[3].Text + "~" + TDprohistory[4].Text + "~" + TDprohistory[5].Text + "~" + TDprohistory[6].Text;

                            gc.insert_date(orderNumber, Parcel_ID, 1520, PropertyHistory, 1, DateTime.Now);
                        }

                    }
                    // Current Year assessment details

                    IWebElement CurrentAssess = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[3]/td/table/tbody/tr[2]/td/table[2]/tbody"));
                    IList<IWebElement> TRCurrentAssess = CurrentAssess.FindElements(By.TagName("tr"));
                    IList<IWebElement> THCurrentAssess = CurrentAssess.FindElements(By.TagName("th"));
                    IList<IWebElement> TDCurrentAssess;
                    foreach (IWebElement row in TRCurrentAssess)
                    {
                        TDCurrentAssess = row.FindElements(By.TagName("td"));
                        if (TRCurrentAssess.Count > 1 && TDCurrentAssess.Count != 0 && row.Text.Trim() != "" && !row.Text.Contains("School Taxable Value") && !row.Text.Contains("Interim Value"))
                        {
                            string currentAssessDetail = TDCurrentAssess[0].Text + "~" + TDCurrentAssess[2].Text + "~" + TDCurrentAssess[3].Text + "~" + TDCurrentAssess[4].Text + "~" + TDCurrentAssess[5].Text + "~" + TDCurrentAssess[6].Text;

                            gc.insert_date(orderNumber, Parcel_ID, 1522, currentAssessDetail, 1, DateTime.Now);
                        }

                    }


                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    // Tax Details
                    string strbillyear = ""; int i = 0;
                    driver.Navigate().GoToUrl("https://pinellas.county-taxes.com/tcb/app/re/accounts");
                    outparcelno = Parcel_ID;
                    driver.FindElement(By.Name("search_query")).SendKeys(outparcelno);
                    gc.CreatePdf(orderNumber, outparcelno, "Tax Search", driver, "FL", "Pinellas");
                    //*[@id="search-controls"]/div/form[1]/div[1]/div/span/button
                    driver.FindElement(By.XPath("//*[@id='search-controls']/div/form[1]/div[1]/div/span/button")).SendKeys(Keys.Enter);
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, outparcelno, "Tax Search Results", driver, "FL", "Pinellas");

                    //full bill historyFull bill history
                    IWebElement ITaxSearch = driver.FindElement(By.LinkText("Full bill history"));
                    string strITaxSearch = ITaxSearch.GetAttribute("href");
                    driver.Navigate().GoToUrl(strITaxSearch);
                    Thread.Sleep(3000);
                    taxparcel = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[1]")).Text;
                    taxparcel = GlobalClass.After(taxparcel, "Real Estate Account #");
                    gc.CreatePdf(orderNumber, outparcelno, "Full Bill History", driver, "FL", "Pinellas");
                    IWebElement IBillHistorytable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table"));
                    IList<IWebElement> IBillHistoryRow = IBillHistorytable.FindElements(By.TagName("tr"));
                    IList<IWebElement> IBillHistoryTD;
                    IList<IWebElement> IBillHistoryTH;
                    int i1 = 0; int m = 0; int j = 0; int k = 0;
                    foreach (IWebElement bill in IBillHistoryRow)
                    {
                        string billyear = "", inst = "", paidamount = "", receipt = "", effectivedate = ""; strBalance = ""; strBillDate = "";
                        IBillHistoryTD = bill.FindElements(By.TagName("td"));
                        IBillHistoryTH = bill.FindElements(By.TagName("th"));
                        if (IBillHistoryTD.Count != 0)
                        {
                            try
                            {
                                if (IBillHistoryTD[0].Text.Contains("Redeemed certificate") || IBillHistoryTD[0].Text.Contains("Issued certificate") || IBillHistoryTD[0].Text.Contains("Tax Deed Application") || IBillHistoryTD[0].Text.Contains("Expired certificate"))
                                {
                                    if (IBillHistoryTD.Count == 5)
                                    {
                                        //billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                        billyear = IBillHistoryTD[0].Text;
                                        strBalance = IBillHistoryTD[1].Text.Replace("\r\n", " ");
                                        strBillDate = IBillHistoryTD[2].Text;
                                        paidamount = IBillHistoryTD[3].Text;
                                        string strTaxHistory2 = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                        gc.insert_date(orderNumber, Parcel_ID, 1527, strTaxHistory2, 1, DateTime.Now);
                                    }
                                }

                                if (IBillHistoryTD.Count == 2)
                                {
                                    strBillDate = IBillHistoryTD[0].Text;
                                    paidamount = IBillHistoryTD[1].Text;
                                    if (strBillDate.Contains("Effective"))
                                    {
                                        strBillDate = strBillDate.Replace("Effective ", " ").Trim();
                                        var splitdate = strBillDate.Split(' ');
                                        strBillDate = splitdate[0].Trim();
                                        effectivedate = splitdate[1];
                                    }
                                    if (paidamount.Contains("Paid"))
                                    {
                                        strBillPaid = IBillHistoryTD[1].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                        paidamount = gc.Between(strBillPaid, "Paid ", "Receipt ");
                                        receipt = GlobalClass.After(strBillPaid, "Receipt #");
                                    }
                                    string strTaxHistory2 = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                    gc.insert_date(orderNumber, Parcel_ID, 1527, strTaxHistory2, 1, DateTime.Now);
                                }
                                if (IBillHistoryTD[2].Text.Contains("Paid"))
                                {
                                    inst = "Total"; paidamount = IBillHistoryTD[2].Text.Replace("Paid", ""); strBalance = "";
                                    string strTaxHistory = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                    gc.insert_date(orderNumber, Parcel_ID, 1527, strTaxHistory, 1, DateTime.Now);
                                }

                                if (IBillHistoryTD[0].Text.Contains("Issued certificate"))
                                {
                                    IWebElement ITaxBillCount = IBillHistoryTD[0].FindElement(By.TagName("a"));
                                    string issuelink = ITaxBillCount.GetAttribute("href");
                                    strissuecertificate.Add(issuelink);
                                }


                                if (bill.Text.Contains("Annual Bill") || bill.Text.Contains("Pay this bill") || bill.Text.Contains("Installment Bill"))
                                {
                                    if (m < 12)
                                    {
                                        IWebElement ITaxBillCount = IBillHistoryTD[0].FindElement(By.TagName("a"));
                                        strbillyear = GlobalClass.Before(ITaxBillCount.Text, " Annual Bill");
                                        string taxlink = ITaxBillCount.GetAttribute("href");

                                        if (bill.Text.Contains("Annual Bill"))
                                        {
                                            if (j < 3)
                                            {
                                                //download
                                                try
                                                {
                                                    IWebElement ITaxBill = IBillHistoryTD[3].FindElement(By.TagName("a"));
                                                    string BillTax = ITaxBill.GetAttribute("href");
                                                    downloadlink.Add(BillTax);

                                                    strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();

                                                    BillDownload(orderNumber, parcelNumber, BillTax, Parcel_ID, strbillyear);
                                                    // gc.CreatePdf(orderNumber, outparcelno, "BillTax" + j, driver, "FL", "Pinellas");
                                                }
                                                catch { }
                                                try
                                                {
                                                    IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                                    string BillTax = ITaxBill.GetAttribute("href");
                                                    downloadlink.Add(BillTax);
                                                    //string dbill = gc.Between(BillTax, "bills/", "/print");
                                                    strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                                    string fname = "Tax Bill" + " " + outparcelno + " " + strBill;
                                                    //gc.downloadfile(BillTax, orderNumber, Parcel_ID, fname, "FL", "Pinellas");

                                                    BillDownload(orderNumber, parcelNumber, BillTax, Parcel_ID, strbillyear);
                                                }
                                                catch (Exception ex)
                                                {
                                                }
                                                taxhistorylink.Add(taxlink);

                                            }

                                            if (taxhistorylinkinst.Count != 0 && taxhistorylinkinst.Count < 12 && j < 8 && taxhistorylink.Count < 2)
                                            {
                                                //download 
                                                try
                                                {
                                                    IWebElement ITaxBill = IBillHistoryTD[3].FindElement(By.TagName("a"));
                                                    string BillTax = ITaxBill.GetAttribute("href");
                                                    downloadlink.Add(BillTax);

                                                    strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                                    // Osceola-County-Real-Estate-R232529-348000020100-2017-Annual-bill
                                                    //gc.downloadfile(BillTax, orderNumber, Parcel_ID, "Osceola-County-Real-Estate-" + Parcel_ID + "-" + strBill + "-bill", "FL", "Pinellas");

                                                    BillDownload(orderNumber, parcelNumber, BillTax, Parcel_ID, strbillyear);

                                                }
                                                catch { }
                                                try
                                                {
                                                    IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                                    string BillTax = ITaxBill.GetAttribute("href");
                                                    downloadlink.Add(BillTax);
                                                    //string dbill = gc.Between(BillTax, "bills/", "/print");
                                                    strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                                    string fname = "Tax Bill" + " " + outparcelno + " " + strBill;
                                                    //gc.downloadfile(BillTax, orderNumber, Parcel_ID, fname, "FL", "Pinellas");

                                                    // View Bill 

                                                    BillDownload(orderNumber, parcelNumber, BillTax, Parcel_ID, strbillyear);

                                                }
                                                catch (Exception ex)
                                                {
                                                }
                                                // taxhistorylink.Add(taxlink);
                                            }
                                            j++;
                                            k++;
                                            strbillyear = "";
                                        }
                                        else if (bill.Text.Contains("Installment Bill"))
                                        {
                                            if (taxhistorylink.Count == 3)
                                            {
                                                //
                                            }
                                            else if (taxhistorylink.Count == 2)
                                            {
                                                if (j < 4)
                                                {
                                                    //download
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[3].FindElement(By.TagName("a"));
                                                        strbillyear = GlobalClass.Before(ITaxBillCount.Text, " Installment Bill");
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                                        downloadlink.Add(BillTax);
                                                        // Osceola-County-Real-Estate-R232529-348000020100-2017-Annual-bill
                                                        // gc.downloadfile(BillTax, orderNumber, Parcel_ID, "Osceola-County-Real-Estate-" + Parcel_ID + "-" + strBill + "-bill", "FL", "Pinellas");
                                                        // View Bill

                                                        BillDownload(orderNumber, parcelNumber, BillTax, Parcel_ID, strbillyear);


                                                    }
                                                    catch { }
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                                        strbillyear = GlobalClass.Before(ITaxBillCount.Text, " Installment Bill");
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        downloadlink.Add(BillTax);
                                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                                        // gc.downloadfile(BillTax, orderNumber, Parcel_ID, "Osceola-County-Real-Estate-" + Parcel_ID + " - " + strBill + " -bill", "FL", "Pinellas");
                                                        // View Bill 

                                                        BillDownload(orderNumber, parcelNumber, BillTax, Parcel_ID, strbillyear);


                                                    }
                                                    catch
                                                    {
                                                    }


                                                    IWebElement ITaxBillCount1 = IBillHistoryTD[0].FindElement(By.TagName("a"));
                                                    string taxlink1 = ITaxBillCount1.GetAttribute("href");
                                                    taxhistorylinkinst.Add(taxlink1);
                                                    j++;
                                                }
                                            }
                                            else if (taxhistorylink.Count == 1)
                                            {
                                                if (j < 7)
                                                {
                                                    //download
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[3].FindElement(By.TagName("a"));
                                                        strbillyear = GlobalClass.Before(ITaxBillCount.Text, " Installment Bill");
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        downloadlink.Add(BillTax);
                                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                                        // Osceola-County-Real-Estate-R232529-348000020100-2017-Annual-bill
                                                        //gc.downloadfile(BillTax, orderNumber, Parcel_ID, "Osceola-County-Real-Estate-" + Parcel_ID + "-" + billyear + "-Annual-bill", "FL", "Pinellas");

                                                        BillDownload(orderNumber, parcelNumber, BillTax, Parcel_ID, strbillyear);


                                                    }
                                                    catch { }
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                                        strbillyear = GlobalClass.Before(ITaxBillCount.Text, " Installment Bill");
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        downloadlink.Add(BillTax);
                                                        //gc.downloadfile(BillTax, orderNumber, Parcel_ID, "Osceola-County-Real-Estate-" + Parcel_ID + " - " + billyear + " -Annual-bill", "FL", "Pinellas");

                                                        BillDownload(orderNumber, parcelNumber, BillTax, Parcel_ID, strbillyear);


                                                    }
                                                    catch
                                                    {
                                                    }


                                                    IWebElement ITaxBillCount1 = IBillHistoryTD[0].FindElement(By.TagName("a"));
                                                    string taxlink1 = ITaxBillCount1.GetAttribute("href");
                                                    taxhistorylinkinst.Add(taxlink1);
                                                    j++;
                                                }
                                            }
                                            else if (taxhistorylink.Count == 0)
                                            {
                                                if (j < 12)
                                                {
                                                    //download
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[3].FindElement(By.TagName("a"));
                                                        strbillyear = GlobalClass.Before(ITaxBillCount.Text, " Installment Bill");
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        downloadlink.Add(BillTax);
                                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                                        // Osceola-County-Real-Estate-R232529-348000020100-2017-Annual-bill
                                                        //gc.downloadfile(BillTax, orderNumber, Parcel_ID, "Osceola-County-Real-Estate-" + Parcel_ID + "-" + billyear + "-Annual-bill", "FL", "Pinellas");
                                                        // View Bill 
                                                        BillDownload(orderNumber, parcelNumber, BillTax, Parcel_ID, strbillyear);

                                                    }
                                                    catch { }
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                                        strbillyear = GlobalClass.Before(ITaxBillCount.Text, " Installment Bill");
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        downloadlink.Add(BillTax);
                                                        //gc.downloadfile(BillTax, orderNumber, Parcel_ID, "Osceola-County-Real-Estate-" + Parcel_ID + " - " + billyear + " -Annual-bill", "FL", "Pinellas");
                                                        // View Bill 
                                                        BillDownload(orderNumber, parcelNumber, BillTax, Parcel_ID, strbillyear);

                                                    }
                                                    catch
                                                    {
                                                    }

                                                    IWebElement ITaxBillCount1 = IBillHistoryTD[0].FindElement(By.TagName("a"));
                                                    string taxlink1 = ITaxBillCount1.GetAttribute("href");
                                                    taxhistorylinkinst.Add(taxlink1);
                                                    j++;
                                                }
                                            }
                                            strbillyear = "";
                                        }
                                        m++;
                                    }
                                    if (bill.Text.Contains("Pay this bill"))
                                    {
                                        //download
                                        try
                                        {
                                            IWebElement ITaxBill = IBillHistoryTD[3].FindElement(By.TagName("a"));
                                            try
                                            {
                                                strbillyear = GlobalClass.Before(ITaxBill.Text, " Annual Bill");
                                            }
                                            catch { }
                                            try
                                            {
                                                strbillyear = GlobalClass.Before(ITaxBill.Text, " Installment Bill");
                                            }
                                            catch { }
                                            string BillTax = ITaxBill.GetAttribute("href");
                                            strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                            downloadlink.Add(BillTax);
                                            // Osceola-County-Real-Estate-R232529-348000020100-2017-Annual-bill
                                            //gc.downloadfile(BillTax, orderNumber, Parcel_ID, "Osceola-County-Real-Estate-" + Parcel_ID + "-" + billyear + "-Annual-bill", "FL", "Pinellas");
                                            BillDownload(orderNumber, parcelNumber, BillTax, Parcel_ID, strbillyear);

                                        }
                                        catch { }
                                        try
                                        {
                                            IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                            string BillTax = ITaxBill.GetAttribute("href");
                                            downloadlink.Add(BillTax);
                                            //    gc.downloadfile(BillTax, orderNumber, Parcel_ID, "Osceola-County-Real-Estate-" + Parcel_ID + " - " + billyear + " -Annual-bill", "FL", "Pinellas");

                                            BillDownload(orderNumber, parcelNumber, BillTax, Parcel_ID, strbillyear);
                                        }
                                        catch
                                        {
                                        }


                                        IWebElement ITaxBillCount = IBillHistoryTD[0].FindElement(By.TagName("a"));
                                        string taxlink = ITaxBillCount.GetAttribute("href");
                                        // taxhistorylink.Add(taxlink);
                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd().Replace("Installment Bill #", "").Replace("Bill", "");
                                        var Splitbill = strBill.Split(' ');
                                        billyear = Splitbill[0]; inst = Splitbill[1];
                                        strBalance = IBillHistoryTD[1].Text.Replace("\r\n", "").TrimStart().TrimEnd();
                                        string strTaxHistory = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                        gc.insert_date(orderNumber, Parcel_ID, 1527, strTaxHistory, 1, DateTime.Now);

                                        strbillyear = "";
                                    }
                                    else
                                    {

                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd().Replace("Installment Bill #", "").Replace("Bill", "");
                                        var Splitbill1 = strBill.Split(' ');
                                        billyear = Splitbill1[0]; inst = Splitbill1[1];
                                        strBalance = IBillHistoryTD[1].Text.Replace("\r\n", "").TrimStart().TrimEnd();
                                        strBillDate = IBillHistoryTD[2].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                        if (strBillDate.Contains("Effective"))
                                        {
                                            strBillDate = strBillDate.Replace("Effective ", "");
                                            var splitdate = strBillDate.Split(' ');
                                            strBillDate = splitdate[0];
                                            effectivedate = splitdate[1];
                                        }
                                        strBillPaid = IBillHistoryTD[3].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                        paidamount = gc.Between(strBillPaid, "Paid ", "Receipt ");
                                        receipt = GlobalClass.After(strBillPaid, "Receipt #");
                                        string strTaxHistory = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                        gc.insert_date(orderNumber, Parcel_ID, 1527, strTaxHistory, 1, DateTime.Now);

                                    }


                                    // m++;
                                    strbillyear = "";
                                }

                                else
                                {
                                    strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd().Replace("Installment Bill #", "").Replace("Bill", "");
                                    var Splitbill = strBill.Split(' ');
                                    billyear = Splitbill[0]; inst = Splitbill[1];
                                    strBalance = IBillHistoryTD[1].Text.Replace("\r\n", "").TrimStart().TrimEnd();
                                    strBillDate = IBillHistoryTD[2].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                    if (strBillDate.Contains("Effective"))
                                    {
                                        strBillDate = strBillDate.Replace("Effective ", "");
                                        var splitdate = strBillDate.Split(' ');
                                        strBillDate = splitdate[0];
                                        effectivedate = splitdate[1];
                                    }
                                    strBillPaid = IBillHistoryTD[3].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                    paidamount = gc.Between(strBillPaid, "Paid ", "Receipt ");
                                    receipt = GlobalClass.After(strBillPaid, "Receipt #");
                                    //Tax Year~Installment~Total Due~Paid Date~Effective Date~Paid Amount~Receipt Number
                                    string strTaxHistory = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                    gc.insert_date(orderNumber, Parcel_ID, 1527, strTaxHistory, 1, DateTime.Now);
                                }

                            }
                            catch
                            {

                            }


                        }
                        if (IBillHistoryTH.Count != 0)
                        {
                            if (IBillHistoryTH[0].Text.Contains("Total Balance"))
                            {
                                inst = "Total Balance"; strBalance = IBillHistoryTH[1].Text.Replace("\r\n", "").TrimStart().TrimEnd();
                                string strTaxHistory = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                gc.insert_date(orderNumber, Parcel_ID, 1527, strTaxHistory, 1, DateTime.Now);
                            }
                        }

                        i++;
                    }

                    int q = 0; string TaxYear = "";
                    foreach (string URL in taxhistorylink)
                    {
                        try
                        {
                            accno = ""; alterkey = ""; taxowner = ""; tax_addr = ""; millagecode = ""; milagerate = ""; ctax_year = ""; cctaxyear = ""; combinedtaxamount = ""; grosstax = ""; cpaidamount = ""; cpaiddate = ""; ceffdate = ""; creceipt = "";
                            driver.Navigate().GoToUrl(URL);
                            Thread.Sleep(3000);
                            //*[@id="content"]/div[1]/div[8]/div/div[1]                        
                            cctaxyear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[1]/div[2]")).Text.Trim().Replace("Real Estate", "").Replace("Print this bill (PDF)", "").Replace("\r\n", "");
                            gc.CreatePdf(orderNumber, outparcelno, "Tax Details" + cctaxyear, driver, "FL", "Pinellas");
                            try
                            {
                                IWebElement ITaxYear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[3]/div[2]/ul/li[1]/a"));
                                TaxYear = ITaxYear.Text;
                            }
                            catch { }
                            if (q == 0)
                            {

                                IWebElement valoremtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[2]"));
                                IList<IWebElement> valoremtableRow = valoremtable.FindElements(By.TagName("tr"));
                                int valoremtablerowcount = valoremtableRow.Count;
                                IList<IWebElement> valoremtablerowTD;
                                int d = 0;
                                foreach (IWebElement rowid in valoremtableRow)
                                {
                                    valoremtablerowTD = rowid.FindElements(By.TagName("td"));
                                    if (valoremtablerowTD.Count != 0 && !rowid.Text.Contains("Taxing authority"))
                                    {
                                        string valoremtax = "";
                                        if (d < valoremtablerowcount - 1)
                                        {
                                            if (valoremtablerowTD.Count == 6)
                                            {
                                                //Taxing Authority~Tax Type~Millage~Assessed~Exemption~Taxable~Taxes
                                                valoremtax = TaxYear + "~" + valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[1].Text + "~" + valoremtablerowTD[2].Text + "~" + valoremtablerowTD[3].Text + "~" + valoremtablerowTD[4].Text + "~" + valoremtablerowTD[5].Text;
                                                gc.insert_date(orderNumber, Parcel_ID, 1528, valoremtax, 1, DateTime.Now);
                                            }

                                            else if (valoremtablerowTD.Count == 2)
                                            {
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                                gc.insert_date(orderNumber, Parcel_ID, 1528, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        else if (d == valoremtablerowcount - 1)
                                        {
                                            milagerate = valoremtablerowTD[0].Text;
                                            valoremtax = TaxYear + "~" + "Total" + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + valoremtablerowTD[1].Text;
                                            gc.insert_date(orderNumber, Parcel_ID, 1528, valoremtax, 1, DateTime.Now);
                                        }
                                    }
                                    d++;
                                }

                                //Non Ad-Valorems
                                try
                                {
                                    //*[@id="content"]/div[1]/div[8]/div/table[3]
                                    IWebElement nonvaloremtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[3]"));
                                    IList<IWebElement> nonvaloremtableRow = nonvaloremtable.FindElements(By.TagName("tr"));
                                    int nonvaloremtablerowcount = nonvaloremtableRow.Count;
                                    IList<IWebElement> nonvaloremtablerowTD;
                                    int e = 0;
                                    foreach (IWebElement rowid in nonvaloremtableRow)
                                    {
                                        nonvaloremtablerowTD = rowid.FindElements(By.TagName("td"));
                                        if (nonvaloremtablerowTD.Count != 0 && !rowid.Text.Contains("Levying authority"))
                                        {
                                            string valoremtax = "";
                                            if (e < nonvaloremtablerowcount - 1)
                                            {
                                                //Taxing Authority~Tax Type~Millage~Assessed~Exemption~Taxable~Taxes
                                                valoremtax = TaxYear + "~" + nonvaloremtablerowTD[0].Text + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[1].Text + "~" + nonvaloremtablerowTD[2].Text;
                                                gc.insert_date(orderNumber, Parcel_ID, 1528, valoremtax, 1, DateTime.Now);
                                            }
                                            else if (e == nonvaloremtablerowcount - 1)
                                            {
                                                valoremtax = TaxYear + "~" + "Total" + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[0].Text;
                                                gc.insert_date(orderNumber, Parcel_ID, 1528, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        e++;
                                    }
                                }
                                catch { }
                            }

                            string currenttaxbulktext = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/p")).Text;
                            combinedtaxamount = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/p")).Text.Trim().Replace("Combined taxes and assessments: ", "");
                            try
                            {
                                string bulkgrosstax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]/tbody/tr/td[1]")).Text.Trim();
                                if (bulkgrosstax.Contains("Gross"))
                                {

                                    grosstax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]/tbody/tr/td[1]")).Text.Trim().Replace("Gross", "");
                                    //if paid by                 
                                    string ifpaidbulk = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]/tbody/tr/td[3]")).Text;
                                    string[] stringSeparators1 = new string[] { "\r\n" };
                                    string[] lines1 = ifpaidbulk.Split(stringSeparators1, StringSplitOptions.None);
                                    ifpaidby = lines1[0]; pleasepay = lines1[1];
                                    string single = cctaxyear + "~" + ifpaidby + "~" + pleasepay + "~" + TaxAuthority;
                                    gc.insert_date(orderNumber, Parcel_ID, 1532, single, 1, DateTime.Now);
                                }

                                else
                                {
                                    string IfPaidBy = "", PlesePay = "", DueDate = "", itaxyear = "";
                                    try
                                    {
                                        IWebElement multitableElement26 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]"));
                                        IList<IWebElement> multitableRow26 = multitableElement26.FindElements(By.TagName("tr"));
                                        IList<IWebElement> multirowTD26;
                                        foreach (IWebElement row in multitableRow26)
                                        {
                                            multirowTD26 = row.FindElements(By.TagName("td"));
                                            int iRowsCount1 = multirowTD26.Count;
                                            for (int n = 0; n < iRowsCount1; n++)
                                            {
                                                if ((!multirowTD26[n].Text.Contains("Face Amt")) && (!multirowTD26[n].Text.Contains("Certificate")))
                                                {
                                                    IfPaidBy = multirowTD26[n].Text.Replace("\r\n", "~");
                                                    var IfpaySplit = IfPaidBy.Split('~');
                                                    IfPaidBy = IfpaySplit[0];
                                                    PlesePay = IfpaySplit[1];
                                                    DueDate = cctaxyear + "~" + IfPaidBy + "~" + PlesePay + "~" + TaxAuthority;
                                                    gc.insert_date(orderNumber, Parcel_ID, 1532, DueDate, 1, DateTime.Now);

                                                }
                                            }
                                        }

                                    }
                                    catch { }
                                }
                            }
                            catch { }
                            try
                            {

                                string bulktext1 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[5]")).Text.Replace("\r\n", ",");
                                taxowner = gc.Between(bulktext1, "Owner,", "Situs address").Replace(",,", "");
                                tax_addr = gc.Between(bulktext1, "Situs address", "Legal description").Trim().Replace(",", " ");
                                //*[@id='content']/div[1]/div[7]/div/table[1]/tbody
                                IWebElement currrtaxtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[1]/tbody"));
                                IList<IWebElement> currrtaxtableRow = currrtaxtable.FindElements(By.TagName("tr"));
                                int currrtaxtabletableRowcount = currrtaxtableRow.Count;
                                IList<IWebElement> currrtaxtableRowTD;
                                int e = 0;
                                foreach (IWebElement rowid in currrtaxtableRow)
                                {
                                    currrtaxtableRowTD = rowid.FindElements(By.TagName("td"));
                                    if (currrtaxtableRowTD.Count != 0 && !rowid.Text.Contains("Account number"))
                                    {
                                        accno = currrtaxtableRowTD[0].Text; alterkey = currrtaxtableRowTD[1].Text; millagecode = currrtaxtableRowTD[3].Text;
                                    }
                                }

                            }
                            catch { }
                            try
                            {

                                string paidbulk = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[3]/div[1]/div")).Text.Trim();
                                cpaiddate = gc.Between(paidbulk, "PAID ", " $").Trim();
                                cpaidamount = gc.Between(paidbulk, "$", "Receipt ").Trim();
                                cpaidamount = "$" + cpaidamount;
                                if (cpaidamount.Contains("Effective"))
                                {
                                    ceffdate = GlobalClass.After(cpaidamount, "Effective").Trim();
                                    cpaidamount = GlobalClass.Before(cpaidamount, "Effective").Trim();
                                }
                                creceipt = GlobalClass.After(paidbulk, "Receipt #");
                            }
                            catch { }

                            ////Parcel Details
                            IWebElement ITaxSearchparcell = driver.FindElement(By.LinkText("Parcel details"));
                            string strITaxSearchparcell = ITaxSearchparcell.GetAttribute("href");
                            driver.Navigate().GoToUrl(strITaxSearchparcell);
                            Thread.Sleep(3000);
                            gc.CreatePdf(orderNumber, outparcelno, "Parcel Details" + cctaxyear, driver, "FL", "Pinellas");
                            if (milagerate.Trim() == "")
                            {
                                milagerate = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[3]/div[2]/dl[2]/dd[4]")).Text;
                            }

                            //Account Number~Alternate Key~Owner Name & Mailing Address~Property Address~Millage Code~Millage rate~Tax Year~Tax Amount~Gross Tax~Paid Amount~Paid Date~Effective Date~Receipt Number
                            string curtax = alterkey + "~" + taxowner + "~" + tax_addr + "~" + millagecode + "~" + milagerate + "~" + cctaxyear + "~" + combinedtaxamount + "~" + grosstax + "~" + cpaidamount + "~" + cpaiddate + "~" + ceffdate + "~" + creceipt;
                            gc.insert_date(orderNumber, Parcel_ID, 1530, curtax, 1, DateTime.Now);
                            q++;
                        }

                        catch
                        {

                        }

                        try
                        {
                            accno = ""; alterkey = ""; taxowner = ""; tax_addr = ""; millagecode = ""; milagerate = ""; ctax_year = ""; cctaxyear = ""; combinedtaxamount = ""; grosstax = ""; cpaidamount = ""; cpaiddate = ""; ceffdate = ""; creceipt = "";
                            driver.Navigate().GoToUrl(URL);
                            Thread.Sleep(3000);
                            try
                            {
                                IWebElement ITaxYear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[3]/div[2]/ul/li[1]/a"));
                                TaxYear = ITaxYear.Text;
                            }
                            catch { }
                            cctaxyear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[1]/div[2]")).Text.Trim().Replace("Real Estate", "").Replace("Print this bill (PDF)", "").Replace("\r\n", "");
                            gc.CreatePdf(orderNumber, outparcelno, "Tax Details" + cctaxyear, driver, "FL", "Pinellas");

                            if (q == 0)
                            {

                                IWebElement valoremtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[2]"));
                                IList<IWebElement> valoremtableRow = valoremtable.FindElements(By.TagName("tr"));
                                int valoremtablerowcount = valoremtableRow.Count;
                                IList<IWebElement> valoremtablerowTD;
                                int d = 0;
                                foreach (IWebElement rowid in valoremtableRow)
                                {
                                    valoremtablerowTD = rowid.FindElements(By.TagName("td"));
                                    if (valoremtablerowTD.Count != 0 && !rowid.Text.Contains("Taxing authority"))
                                    {
                                        string valoremtax = "";
                                        if (d < valoremtablerowcount - 1)
                                        {
                                            if (valoremtablerowTD.Count == 6)
                                            {
                                                //Taxing Authority~Tax Type~Millage~Assessed~Exemption~Taxable~Taxes
                                                valoremtax = TaxYear + "~" + valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[1].Text + "~" + valoremtablerowTD[2].Text + "~" + valoremtablerowTD[3].Text + "~" + valoremtablerowTD[4].Text + "~" + valoremtablerowTD[5].Text;
                                                gc.insert_date(orderNumber, Parcel_ID, 1528, valoremtax, 1, DateTime.Now);
                                            }

                                            else if (valoremtablerowTD.Count == 2)
                                            {
                                                valoremtax = TaxYear + "~" + valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                                gc.insert_date(orderNumber, Parcel_ID, 1528, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        else if (d == valoremtablerowcount - 1)
                                        {
                                            milagerate = valoremtablerowTD[0].Text;
                                            valoremtax = TaxYear + "~" + "Total" + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + valoremtablerowTD[1].Text;
                                            gc.insert_date(orderNumber, Parcel_ID, 1528, valoremtax, 1, DateTime.Now);
                                        }
                                    }
                                    d++;
                                }

                                //Non Ad-Valorems
                                try
                                {
                                    //*[@id="content"]/div[1]/div[8]/div/table[3]
                                    IWebElement nonvaloremtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[3]"));
                                    IList<IWebElement> nonvaloremtableRow = nonvaloremtable.FindElements(By.TagName("tr"));
                                    int nonvaloremtablerowcount = nonvaloremtableRow.Count;
                                    IList<IWebElement> nonvaloremtablerowTD;
                                    int e = 0;
                                    foreach (IWebElement rowid in nonvaloremtableRow)
                                    {
                                        nonvaloremtablerowTD = rowid.FindElements(By.TagName("td"));
                                        if (nonvaloremtablerowTD.Count != 0 && !rowid.Text.Contains("Levying authority"))
                                        {
                                            string valoremtax = "";
                                            if (e < nonvaloremtablerowcount - 1)
                                            {
                                                //Taxing Authority~Tax Type~Millage~Assessed~Exemption~Taxable~Taxes
                                                valoremtax = TaxYear + "~" + nonvaloremtablerowTD[0].Text + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[1].Text + "~" + nonvaloremtablerowTD[2].Text;
                                                gc.insert_date(orderNumber, Parcel_ID, 1528, valoremtax, 1, DateTime.Now);
                                            }
                                            else if (e == nonvaloremtablerowcount - 1)
                                            {
                                                valoremtax = TaxYear + "~" + "Total" + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[0].Text;
                                                gc.insert_date(orderNumber, Parcel_ID, 1528, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        e++;
                                    }
                                }
                                catch { }
                            }

                            string currenttaxbulktext = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/p")).Text;
                            combinedtaxamount = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/p")).Text.Trim().Replace("Combined taxes and assessments: ", "");
                            try
                            {
                                string bulkgrosstax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]/tbody/tr/td[1]")).Text.Trim();
                                if (bulkgrosstax.Contains("Gross"))
                                {

                                    grosstax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]/tbody/tr/td[1]")).Text.Trim().Replace("Gross", "");
                                    //if paid by                 
                                    string ifpaidbulk = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]/tbody/tr/td[3]")).Text;
                                    string[] stringSeparators1 = new string[] { "\r\n" };
                                    string[] lines1 = ifpaidbulk.Split(stringSeparators1, StringSplitOptions.None);
                                    ifpaidby = lines1[0]; pleasepay = lines1[1];
                                    string single = cctaxyear + "~" + ifpaidby + "~" + pleasepay + "~" + TaxAuthority;
                                    gc.insert_date(orderNumber, Parcel_ID, 1532, single, 1, DateTime.Now);
                                }

                                else
                                {
                                    string IfPaidBy = "", PlesePay = "", DueDate = "", itaxyear = "";
                                    try
                                    {
                                        IWebElement multitableElement26 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]"));
                                        IList<IWebElement> multitableRow26 = multitableElement26.FindElements(By.TagName("tr"));
                                        IList<IWebElement> multirowTD26;
                                        foreach (IWebElement row in multitableRow26)
                                        {
                                            multirowTD26 = row.FindElements(By.TagName("td"));
                                            int iRowsCount2 = multirowTD26.Count;
                                            for (int n = 0; n < iRowsCount2; n++)
                                            {
                                                if ((!multirowTD26[n].Text.Contains("Face Amt")) && (!multirowTD26[n].Text.Contains("Certificate")))
                                                {
                                                    IfPaidBy = multirowTD26[n].Text.Replace("\r\n", "~");
                                                    var IfpaySplit = IfPaidBy.Split('~');
                                                    IfPaidBy = IfpaySplit[0];
                                                    PlesePay = IfpaySplit[1];
                                                    DueDate = cctaxyear + "~" + IfPaidBy + "~" + PlesePay + "~" + TaxAuthority;
                                                    gc.insert_date(orderNumber, Parcel_ID, 1532, DueDate, 1, DateTime.Now);

                                                }
                                            }
                                        }

                                    }
                                    catch { }
                                }
                            }
                            catch { }
                            try
                            {

                                string bulktext1 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[5]")).Text.Replace("\r\n", ",");
                                taxowner = gc.Between(bulktext1, "Owner,", "Situs address").Replace(".", "");
                                tax_addr = gc.Between(bulktext1, "Situs address", "Legal description").Trim().Replace(",", " ");
                                //*[@id='content']/div[1]/div[7]/div/table[1]/tbody
                                IWebElement currrtaxtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[1]/tbody"));
                                IList<IWebElement> currrtaxtableRow = currrtaxtable.FindElements(By.TagName("tr"));
                                int currrtaxtabletableRowcount = currrtaxtableRow.Count;
                                IList<IWebElement> currrtaxtableRowTD;
                                int e = 0;
                                foreach (IWebElement rowid in currrtaxtableRow)
                                {
                                    currrtaxtableRowTD = rowid.FindElements(By.TagName("td"));
                                    if (currrtaxtableRowTD.Count != 0 && !rowid.Text.Contains("Account number"))
                                    {
                                        accno = currrtaxtableRowTD[0].Text; alterkey = currrtaxtableRowTD[1].Text; millagecode = currrtaxtableRowTD[3].Text;
                                    }
                                }

                            }
                            catch { }
                            try
                            {

                                string paidbulk = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[3]/div[1]/div")).Text.Trim();
                                cpaiddate = gc.Between(paidbulk, "PAID ", " $").Trim();
                                cpaidamount = gc.Between(paidbulk, "$", "Receipt ").Trim();
                                cpaidamount = "$" + cpaidamount;
                                creceipt = GlobalClass.After(paidbulk, "Receipt #");
                            }
                            catch { }

                            ////Parcel Details
                            IWebElement ITaxSearchparcell = driver.FindElement(By.LinkText("Parcel details"));
                            string strITaxSearchparcell = ITaxSearchparcell.GetAttribute("href");
                            driver.Navigate().GoToUrl(strITaxSearchparcell);
                            Thread.Sleep(3000);
                            gc.CreatePdf(orderNumber, outparcelno, "Parcel Details" + cctaxyear, driver, "FL", "Pinellas");
                            if (milagerate.Trim() == "")
                            {
                                milagerate = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[3]/div[2]/dl[2]/dd[4]")).Text;
                            }

                            //Account Number~Alternate Key~Owner Name & Mailing Address~Property Address~Millage Code~Millage rate~Tax Year~Tax Amount~Gross Tax~Paid Amount~Paid Date~Effective Date~Receipt Number
                            string curtax = alterkey + "~" + taxowner + "~" + tax_addr + "~" + millagecode + "~" + milagerate + "~" + cctaxyear + "~" + combinedtaxamount + "~" + grosstax + "~" + cpaidamount + "~" + cpaiddate + "~" + ceffdate + "~" + creceipt;
                            gc.insert_date(orderNumber, Parcel_ID, 1530, curtax, 1, DateTime.Now);
                            q++;
                        }
                        catch
                        { }
                    }


                    q = 0;
                    foreach (string URL in taxhistorylinkinst)
                    {
                        try
                        {
                            accno = ""; alterkey = ""; taxowner = ""; tax_addr = ""; millagecode = ""; milagerate = ""; ctax_year = ""; cctaxyear = ""; combinedtaxamount = ""; grosstax = ""; cpaidamount = ""; cpaiddate = ""; ceffdate = ""; creceipt = "";
                            driver.Navigate().GoToUrl(URL);
                            Thread.Sleep(3000);
                            //*[@id="content"]/div[1]/div[8]/div/div[1]                        
                            cctaxyear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[1]/div[2]")).Text.Trim().Replace("Real Estate", "").Replace("Print this bill (PDF)", "").Replace("\r\n", "");
                            gc.CreatePdf(orderNumber, outparcelno, "Tax Details" + cctaxyear, driver, "FL", "Pinellas");

                            if (q == 0)
                            {

                                IWebElement valoremtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[2]"));
                                IList<IWebElement> valoremtableRow = valoremtable.FindElements(By.TagName("tr"));
                                int valoremtablerowcount = valoremtableRow.Count;
                                IList<IWebElement> valoremtablerowTD;
                                int d = 0;
                                foreach (IWebElement rowid in valoremtableRow)
                                {
                                    valoremtablerowTD = rowid.FindElements(By.TagName("td"));
                                    if (valoremtablerowTD.Count != 0 && !rowid.Text.Contains("Taxing authority"))
                                    {
                                        string valoremtax = "";
                                        if (d < valoremtablerowcount - 1)
                                        {
                                            if (valoremtablerowTD.Count == 6)
                                            {
                                                //Taxing Authority~Tax Type~Millage~Assessed~Exemption~Taxable~Taxes
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[1].Text + "~" + valoremtablerowTD[2].Text + "~" + valoremtablerowTD[3].Text + "~" + valoremtablerowTD[4].Text + "~" + valoremtablerowTD[5].Text;
                                                gc.insert_date(orderNumber, Parcel_ID, 1528, valoremtax, 1, DateTime.Now);
                                            }

                                            else if (valoremtablerowTD.Count == 2)
                                            {
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                                gc.insert_date(orderNumber, Parcel_ID, 1528, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        else if (d == valoremtablerowcount - 1)
                                        {
                                            milagerate = valoremtablerowTD[0].Text;
                                            valoremtax = "Total" + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + valoremtablerowTD[1].Text;
                                            gc.insert_date(orderNumber, Parcel_ID, 1528, valoremtax, 1, DateTime.Now);
                                        }
                                    }
                                    d++;
                                }

                                //Non Ad-Valorems
                                try
                                {
                                    //*[@id="content"]/div[1]/div[8]/div/table[3]
                                    IWebElement nonvaloremtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[3]"));
                                    IList<IWebElement> nonvaloremtableRow = nonvaloremtable.FindElements(By.TagName("tr"));
                                    int nonvaloremtablerowcount = nonvaloremtableRow.Count;
                                    IList<IWebElement> nonvaloremtablerowTD;
                                    int e = 0;
                                    foreach (IWebElement rowid in nonvaloremtableRow)
                                    {
                                        nonvaloremtablerowTD = rowid.FindElements(By.TagName("td"));
                                        if (nonvaloremtablerowTD.Count != 0 && !rowid.Text.Contains("Levying authority"))
                                        {
                                            string valoremtax = "";
                                            if (e < nonvaloremtablerowcount - 1)
                                            {
                                                //Taxing Authority~Tax Type~Millage~Assessed~Exemption~Taxable~Taxes
                                                valoremtax = TaxYear + "~" + nonvaloremtablerowTD[0].Text + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[1].Text + "~" + nonvaloremtablerowTD[2].Text;
                                                gc.insert_date(orderNumber, Parcel_ID, 1528, valoremtax, 1, DateTime.Now);
                                            }
                                            else if (e == nonvaloremtablerowcount - 1)
                                            {
                                                valoremtax = TaxYear + "~" + "Total" + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[0].Text;
                                                gc.insert_date(orderNumber, Parcel_ID, 1528, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        e++;
                                    }
                                }
                                catch { }
                            }

                            string currenttaxbulktext = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/p")).Text;
                            combinedtaxamount = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/p")).Text.Trim().Replace("Combined taxes and assessments: ", "");
                            try
                            {
                                string bulkgrosstax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]/tbody/tr/td[1]")).Text.Trim();
                                if (bulkgrosstax.Contains("Gross"))
                                {

                                    grosstax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]/tbody/tr/td[1]")).Text.Trim().Replace("Gross", "");
                                    //if paid by                 
                                    string ifpaidbulk = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]/tbody/tr/td[3]")).Text;
                                    string[] stringSeparators1 = new string[] { "\r\n" };
                                    string[] lines1 = ifpaidbulk.Split(stringSeparators1, StringSplitOptions.None);
                                    ifpaidby = lines1[0]; pleasepay = lines1[1];
                                    string single = cctaxyear + "~" + ifpaidby + "~" + pleasepay + "~" + TaxAuthority;
                                    gc.insert_date(orderNumber, Parcel_ID, 1532, single, 1, DateTime.Now);
                                }

                                else
                                {
                                    string IfPaidBy = "", PlesePay = "", DueDate = "", itaxyear = "";
                                    try
                                    {
                                        IWebElement multitableElement26 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]"));
                                        IList<IWebElement> multitableRow26 = multitableElement26.FindElements(By.TagName("tr"));
                                        IList<IWebElement> multirowTD26;
                                        foreach (IWebElement row in multitableRow26)
                                        {
                                            multirowTD26 = row.FindElements(By.TagName("td"));
                                            int iRowsCount3 = multirowTD26.Count;
                                            for (int n = 0; n < iRowsCount3; n++)
                                            {
                                                if ((!multirowTD26[n].Text.Contains("Face Amt")) && (!multirowTD26[n].Text.Contains("Certificate")))
                                                {
                                                    IfPaidBy = multirowTD26[n].Text.Replace("\r\n", "~");
                                                    var IfpaySplit = IfPaidBy.Split('~');
                                                    IfPaidBy = IfpaySplit[0];
                                                    PlesePay = IfpaySplit[1];
                                                    DueDate = cctaxyear + "~" + IfPaidBy + "~" + PlesePay + "~" + TaxAuthority;
                                                    gc.insert_date(orderNumber, Parcel_ID, 1532, DueDate, 1, DateTime.Now);

                                                }
                                            }
                                        }

                                    }
                                    catch { }
                                }
                            }
                            catch { }
                            try
                            {

                                string bulktext1 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[5]")).Text.Replace("\r\n", ",");
                                taxowner = gc.Between(bulktext1, "Owner,", "Situs address").Replace(",,", "");
                                tax_addr = gc.Between(bulktext1, "Situs address", "Legal description").Trim().Replace(",", " ");
                                //*[@id='content']/div[1]/div[7]/div/table[1]/tbody
                                IWebElement currrtaxtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[1]/tbody"));
                                IList<IWebElement> currrtaxtableRow = currrtaxtable.FindElements(By.TagName("tr"));
                                int currrtaxtabletableRowcount = currrtaxtableRow.Count;
                                IList<IWebElement> currrtaxtableRowTD;
                                int e = 0;
                                foreach (IWebElement rowid in currrtaxtableRow)
                                {
                                    currrtaxtableRowTD = rowid.FindElements(By.TagName("td"));
                                    if (currrtaxtableRowTD.Count != 0 && !rowid.Text.Contains("Account number"))
                                    {
                                        accno = currrtaxtableRowTD[0].Text; alterkey = currrtaxtableRowTD[1].Text; millagecode = currrtaxtableRowTD[3].Text;
                                    }
                                }

                            }
                            catch { }
                            try
                            {

                                string paidbulk = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[3]/div[1]/div")).Text.Trim();
                                cpaiddate = gc.Between(paidbulk, "PAID ", " $").Trim();
                                cpaidamount = gc.Between(paidbulk, "$", "Receipt ").Trim();
                                cpaidamount = "$" + cpaidamount;
                                if (cpaidamount.Contains("Effective"))
                                {
                                    ceffdate = GlobalClass.After(cpaidamount, "Effective").Trim();
                                    cpaidamount = GlobalClass.Before(cpaidamount, "Effective").Trim();
                                }
                                creceipt = GlobalClass.After(paidbulk, "Receipt #");
                            }
                            catch { }

                            ////Parcel Details
                            IWebElement ITaxSearchparcell = driver.FindElement(By.LinkText("Parcel details"));
                            string strITaxSearchparcell = ITaxSearchparcell.GetAttribute("href");
                            driver.Navigate().GoToUrl(strITaxSearchparcell);
                            Thread.Sleep(3000);
                            gc.CreatePdf(orderNumber, outparcelno, "Parcel Details" + cctaxyear, driver, "FL", "Pinellas");
                            if (milagerate.Trim() == "")
                            {
                                milagerate = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[3]/div[2]/dl[2]/dd[4]")).Text;
                            }

                            //Account Number~Alternate Key~Owner Name & Mailing Address~Property Address~Millage Code~Millage rate~Tax Year~Tax Amount~Gross Tax~Paid Amount~Paid Date~Effective Date~Receipt Number
                            string curtax = alterkey + "~" + taxowner + "~" + tax_addr + "~" + millagecode + "~" + milagerate + "~" + cctaxyear + "~" + combinedtaxamount + "~" + grosstax + "~" + cpaidamount + "~" + cpaiddate + "~" + ceffdate + "~" + creceipt;
                            gc.insert_date(orderNumber, Parcel_ID, 1530, curtax, 1, DateTime.Now);
                            q++;
                        }

                        catch
                        {

                        }

                        try
                        {
                            accno = ""; alterkey = ""; taxowner = ""; tax_addr = ""; millagecode = ""; milagerate = ""; ctax_year = ""; cctaxyear = ""; combinedtaxamount = ""; grosstax = ""; cpaidamount = ""; cpaiddate = ""; ceffdate = ""; creceipt = "";
                            driver.Navigate().GoToUrl(URL);
                            Thread.Sleep(3000);

                            cctaxyear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[1]/div[2]")).Text.Trim().Replace("Real Estate", "").Replace("Print this bill (PDF)", "").Replace("\r\n", "");
                            gc.CreatePdf(orderNumber, outparcelno, "Tax Details" + cctaxyear, driver, "FL", "Pinellas");

                            if (q == 0)
                            {

                                IWebElement valoremtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[2]"));
                                IList<IWebElement> valoremtableRow = valoremtable.FindElements(By.TagName("tr"));
                                int valoremtablerowcount = valoremtableRow.Count;
                                IList<IWebElement> valoremtablerowTD;
                                int d = 0;
                                foreach (IWebElement rowid in valoremtableRow)
                                {
                                    valoremtablerowTD = rowid.FindElements(By.TagName("td"));
                                    if (valoremtablerowTD.Count != 0 && !rowid.Text.Contains("Taxing authority"))
                                    {
                                        string valoremtax = "";
                                        if (d < valoremtablerowcount - 1)
                                        {
                                            if (valoremtablerowTD.Count == 6)
                                            {
                                                //Taxing Authority~Tax Type~Millage~Assessed~Exemption~Taxable~Taxes
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[1].Text + "~" + valoremtablerowTD[2].Text + "~" + valoremtablerowTD[3].Text + "~" + valoremtablerowTD[4].Text + "~" + valoremtablerowTD[5].Text;
                                                gc.insert_date(orderNumber, Parcel_ID, 1528, valoremtax, 1, DateTime.Now);
                                            }

                                            else if (valoremtablerowTD.Count == 2)
                                            {
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                                gc.insert_date(orderNumber, Parcel_ID, 1528, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        else if (d == valoremtablerowcount - 1)
                                        {
                                            milagerate = valoremtablerowTD[0].Text;
                                            valoremtax = "Total" + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + valoremtablerowTD[1].Text;
                                            gc.insert_date(orderNumber, Parcel_ID, 1528, valoremtax, 1, DateTime.Now);
                                        }
                                    }
                                    d++;
                                }

                                //Non Ad-Valorems
                                try
                                {
                                    //*[@id="content"]/div[1]/div[8]/div/table[3]
                                    IWebElement nonvaloremtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[3]"));
                                    IList<IWebElement> nonvaloremtableRow = nonvaloremtable.FindElements(By.TagName("tr"));
                                    int nonvaloremtablerowcount = nonvaloremtableRow.Count;
                                    IList<IWebElement> nonvaloremtablerowTD;
                                    int e = 0;
                                    foreach (IWebElement rowid in nonvaloremtableRow)
                                    {
                                        nonvaloremtablerowTD = rowid.FindElements(By.TagName("td"));
                                        if (nonvaloremtablerowTD.Count != 0 && !rowid.Text.Contains("Levying authority"))
                                        {
                                            string valoremtax = "";
                                            if (e < nonvaloremtablerowcount - 1)
                                            {
                                                //Taxing Authority~Tax Type~Millage~Assessed~Exemption~Taxable~Taxes
                                                valoremtax = TaxYear + "~" + nonvaloremtablerowTD[0].Text + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[1].Text + "~" + nonvaloremtablerowTD[2].Text;
                                                gc.insert_date(orderNumber, Parcel_ID, 1528, valoremtax, 1, DateTime.Now);
                                            }
                                            else if (e == nonvaloremtablerowcount - 1)
                                            {
                                                valoremtax = TaxYear + "~" + "Total" + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[0].Text;
                                                gc.insert_date(orderNumber, Parcel_ID, 1528, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        e++;
                                    }
                                }
                                catch { }
                            }

                            string currenttaxbulktext = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/p")).Text;
                            combinedtaxamount = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/p")).Text.Trim().Replace("Combined taxes and assessments: ", "");
                            try
                            {
                                string bulkgrosstax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]/tbody/tr/td[1]")).Text.Trim();
                                if (bulkgrosstax.Contains("Gross"))
                                {

                                    grosstax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]/tbody/tr/td[1]")).Text.Trim().Replace("Gross", "");
                                    //if paid by                 
                                    string ifpaidbulk = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]/tbody/tr/td[3]")).Text;
                                    string[] stringSeparators1 = new string[] { "\r\n" };
                                    string[] lines1 = ifpaidbulk.Split(stringSeparators1, StringSplitOptions.None);
                                    ifpaidby = lines1[0]; pleasepay = lines1[1];
                                    string single = cctaxyear + "~" + ifpaidby + "~" + pleasepay + "~" + TaxAuthority;
                                    gc.insert_date(orderNumber, Parcel_ID, 1532, single, 1, DateTime.Now);
                                }

                                else
                                {
                                    string IfPaidBy = "", PlesePay = "", DueDate = "", itaxyear = "";
                                    try
                                    {
                                        IWebElement multitableElement26 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]"));
                                        IList<IWebElement> multitableRow26 = multitableElement26.FindElements(By.TagName("tr"));
                                        IList<IWebElement> multirowTD26;
                                        foreach (IWebElement row in multitableRow26)
                                        {
                                            multirowTD26 = row.FindElements(By.TagName("td"));
                                            int iRowsCount4 = multirowTD26.Count;
                                            for (int n = 0; n < iRowsCount4; n++)
                                            {
                                                if ((!multirowTD26[n].Text.Contains("Face Amt")) && (!multirowTD26[n].Text.Contains("Certificate")))
                                                {
                                                    IfPaidBy = multirowTD26[n].Text.Replace("\r\n", "~");
                                                    var IfpaySplit = IfPaidBy.Split('~');
                                                    IfPaidBy = IfpaySplit[0];
                                                    PlesePay = IfpaySplit[1];
                                                    DueDate = cctaxyear + "~" + IfPaidBy + "~" + PlesePay + "~" + TaxAuthority;
                                                    gc.insert_date(orderNumber, Parcel_ID, 1532, DueDate, 1, DateTime.Now);

                                                }
                                            }
                                        }

                                    }
                                    catch { }
                                }
                            }
                            catch { }
                            try
                            {

                                string bulktext1 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[5]")).Text.Replace("\r\n", ",");
                                taxowner = gc.Between(bulktext1, "Owner,", "Situs address").Replace(".", "");
                                tax_addr = gc.Between(bulktext1, "Situs address", "Legal description").Trim().Replace(",", " ");
                                //*[@id='content']/div[1]/div[7]/div/table[1]/tbody
                                IWebElement currrtaxtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[1]/tbody"));
                                IList<IWebElement> currrtaxtableRow = currrtaxtable.FindElements(By.TagName("tr"));
                                int currrtaxtabletableRowcount = currrtaxtableRow.Count;
                                IList<IWebElement> currrtaxtableRowTD;
                                int e = 0;
                                foreach (IWebElement rowid in currrtaxtableRow)
                                {
                                    currrtaxtableRowTD = rowid.FindElements(By.TagName("td"));
                                    if (currrtaxtableRowTD.Count != 0 && !rowid.Text.Contains("Account number"))
                                    {
                                        accno = currrtaxtableRowTD[0].Text; alterkey = currrtaxtableRowTD[1].Text; millagecode = currrtaxtableRowTD[3].Text;
                                    }
                                }

                            }
                            catch { }
                            try
                            {

                                string paidbulk = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[3]/div[1]/div")).Text.Trim();
                                cpaiddate = gc.Between(paidbulk, "PAID ", " $").Trim();
                                cpaidamount = gc.Between(paidbulk, "$", "Receipt ").Trim();
                                cpaidamount = "$" + cpaidamount;
                                creceipt = GlobalClass.After(paidbulk, "Receipt #");
                            }
                            catch { }

                            ////Parcel Details
                            IWebElement ITaxSearchparcell = driver.FindElement(By.LinkText("Parcel details"));
                            string strITaxSearchparcell = ITaxSearchparcell.GetAttribute("href");
                            driver.Navigate().GoToUrl(strITaxSearchparcell);
                            Thread.Sleep(3000);
                            gc.CreatePdf(orderNumber, outparcelno, "Parcel Details" + cctaxyear, driver, "FL", "Pinellas");
                            if (milagerate.Trim() == "")
                            {
                                milagerate = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[3]/div[2]/dl[2]/dd[4]")).Text;
                            }

                            //Account Number~Alternate Key~Owner Name & Mailing Address~Property Address~Millage Code~Millage rate~Tax Year~Tax Amount~Gross Tax~Paid Amount~Paid Date~Effective Date~Receipt Number
                            string curtax = alterkey + "~" + taxowner + "~" + tax_addr + "~" + millagecode + "~" + milagerate + "~" + cctaxyear + "~" + combinedtaxamount + "~" + grosstax + "~" + cpaidamount + "~" + cpaiddate + "~" + ceffdate + "~" + creceipt;
                            gc.insert_date(orderNumber, Parcel_ID, 1530, curtax, 1, DateTime.Now);
                            q++;
                        }
                        catch
                        { }
                    }

                    //issue certificate
                    string adno = "", faceamount = "", issuedate = "", ex_date = "", buyer = "", intrate = "", cer_no = "";
                    foreach (string URL1 in strissuecertificate)
                    {
                        driver.Navigate().GoToUrl(URL1);
                        Thread.Sleep(3000);
                        try
                        {
                            string issuecertificatebulktext = "";
                            string issueyear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/p")).Text.Trim().Replace("This parcel has an issued certificate for ", "").Replace(".", "");
                            gc.CreatePdf(orderNumber, outparcelno, "Issue Certificate Details" + issueyear, driver, "FL", "Pinellas");
                            try
                            {
                                issuecertificatebulktext = driver.FindElement(By.XPath("//*[@id='content']/div[1]/dl")).Text;
                            }
                            catch { }
                            try
                            {
                                issuecertificatebulktext = driver.FindElement(By.XPath("//*[@id='content']/div[1]/dl[2]")).Text;
                            }
                            catch { }

                            cer_no = driver.FindElement(By.XPath("//*[@id='certificate']")).Text.Replace("Certificate #", "");
                            adno = gc.Between(issuecertificatebulktext, "Advertised number", "Face amount").Trim();
                            faceamount = gc.Between(issuecertificatebulktext, "Face amount", "Issued date").Trim();
                            issuedate = gc.Between(issuecertificatebulktext, "Issued date", "Expiration date").Trim();
                            ex_date = gc.Between(issuecertificatebulktext, "Expiration date", "Buyer").Trim();
                            // buyer = gc.Between(issuecertificatebulktext, "Buyer", "Interest rate").Replace("\r\n"," ");
                            buyer = driver.FindElement(By.XPath("//*[@id='content']/div[1]/dl/dd[5]")).Text.Replace("\r\n", " ");
                            intrate = GlobalClass.After(issuecertificatebulktext, "Interest rate");
                            //Tax Year~Certificate Number~Advertised Number~Face Amount~Issued Date~Expiration Date~Buyer~Interest Rate
                            string isscer = issueyear + "~" + cer_no + "~" + adno + "~" + faceamount + "~" + issuedate + "~" + ex_date + "~" + buyer + "~" + intrate;
                            gc.insert_date(orderNumber, Parcel_ID, 1531, isscer, 1, DateTime.Now);
                        }
                        catch { }

                    }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "FL", "Pinellas", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "FL", "Pinellas");
                    return "Data Inserted Successfully";
                }
                catch (Exception ex)
                {
                    driver.Quit();
                    throw ex;
                }
            }
        }
        public void BillDownload(string orderNumber, string Parcel_number, string BillTax, string dbill, string strbillyear)
        {
            string fileName = "";
            var chromeOptions = new ChromeOptions();
            var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];
            chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
            chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
            chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
            chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
            var chDriver = new ChromeDriver(chromeOptions);
            try
            {
                chDriver.Navigate().GoToUrl(BillTax);
                Thread.Sleep(5000);
                try
                {

                    // fileName = "Escambia-County--Real-Estate-" + dbill + "-" + strbillyear + "-Annual-bill.pdf";
                    fileName = latestfilename();
                    gc.AutoDownloadFile(orderNumber, Parcel_number, "Pinellas", "FL", fileName);
                    Thread.Sleep(1000);
                }
                catch { }

                try
                {
                    // fileName = "Escambia-County--Real-Estate-" + dbill + "-" + strbillyear + "-Installment-bill-1.pdf";
                    fileName = latestfilename();
                    gc.AutoDownloadFile(orderNumber, Parcel_number, "Pinellas", "FL", fileName);
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {

                }
                try
                {
                    //fileName = "Escambia-County--Real-Estate-" + dbill + "-" + strbillyear + "-Installment-bill-2.pdf";
                    fileName = latestfilename();
                    gc.AutoDownloadFile(orderNumber, Parcel_number, "Pinellas", "FL", fileName);
                    Thread.Sleep(1000);
                }
                catch { }
                try
                {
                    // fileName = "Escambia-County--Real-Estate-" + dbill + "-" + strbillyear + "-Installment-bill-3.pdf";
                    fileName = latestfilename();
                    gc.AutoDownloadFile(orderNumber, Parcel_number, "Pinellas", "FL", fileName);
                    Thread.Sleep(1000);
                }
                catch { }

                try
                {
                    //fileName = "Escambia-County--Real-Estate-" + dbill + "-" + strbillyear + "-Installment-bill-4.pdf";
                    fileName = latestfilename();
                    gc.AutoDownloadFile(orderNumber, Parcel_number, "Pinellas", "FL", fileName);
                    Thread.Sleep(1000);
                }
                catch { }

                chDriver.Quit();
            }
            catch (Exception ex)
            {
                chDriver.Quit();
            }
        }
        public string latestfilename()
        {
            var downloadDirectory1 = ConfigurationManager.AppSettings["AutoPdf"];
            var files = new DirectoryInfo(downloadDirectory1).GetFiles("*.*");
            string latestfile = "";
            DateTime lastupdated = DateTime.MinValue;
            foreach (FileInfo file in files)
            {
                if (file.LastWriteTime > lastupdated)
                {
                    lastupdated = file.LastWriteTime;
                    latestfile = file.Name;
                }

            }
            return latestfile;
        }
        public void multiparcel(string ordernumber)
        {

            IWebElement multiaddrtableElement = driver.FindElement(By.XPath("//*[@id='ctl00_MasterPlaceHolder_grdv']/tbody"));
            IList<IWebElement> multiaddrtableRow = multiaddrtableElement.FindElements(By.TagName("tr"));
            int multiaddrtableRowcount = multiaddrtableRow.Count;
            IList<IWebElement> multiaddrrowTD;
            int o = 0;
            foreach (IWebElement row in multiaddrtableRow)
            {
                if (0 <= 25 && o < multiaddrtableRowcount && !row.Text.Contains("Account"))
                {
                    multiaddrrowTD = row.FindElements(By.TagName("td"));
                    string multi = multiaddrrowTD[4].Text + "~" + multiaddrrowTD[5].Text;
                    gc.insert_date(ordernumber, multiaddrrowTD[3].Text, 1000, multi, 1, DateTime.Now);
                }
                o++;
            }

        }
    }
}