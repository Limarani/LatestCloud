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

namespace ScrapMaricopa.Scrapsource
{
    public class Webdriver_FLCollier
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();

        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;

        public string FTP_Collier(string Address, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            string strBill = "-", strBalance = "-", strBillDate = "-", strBillPaid = "-", strFBill = "-", strFBalance = "-", strFBillDate = "-", strFBillPaid = "-";
            string TaxYear = "", TaxAmount = "", PaidAmount = "", ReceiptNumber = "", Account_number = "", Millage_Code = "", Millage_rate = "";

            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "", AssessTakenTime = "", TaxTakentime = "", CityTaxtakentime = "";
            string TotaltakenTime = "";
            List<string> strTaxRealestate = new List<string>();
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                string AlterNateID = "", PropertyAddress = "", owner = "";
                string[] stringSeparators1 = new string[] { "\r\n" };

                List<string> listurl = new List<string>();


                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    //if (searchType == "titleflex")
                    //{
                    //    gc.TitleFlexSearch(orderNumber, parcelNumber, ownername, Address, "IN", "Hamilton");
                    //    if (GlobalClass.TitleFlex_Search == "Yes")
                    //    {
                    //        return "MultiParcel";
                    //    }
                    //    else
                    //    {
                    //        string strTitleAssess = GlobalClass.TitleFlexAssess;
                    //        parcelNumber = GlobalClass.titleparcel;

                    //        searchType = "parcel";
                    //    }
                    //}

                    if (searchType == "titleflex")
                    {

                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", Address, "FL", "Collier");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_FLCollier"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {

                        driver.Navigate().GoToUrl("http://www.collierappraiser.com/");
                        Thread.Sleep(2000);


                        IWebElement iframeElement = driver.FindElement(By.XPath("/html/frameset/frame"));
                        //now use the switch command
                        driver.SwitchTo().Frame(iframeElement);

                        IWebElement iframeElement1 = driver.FindElement(By.XPath("/html/frameset/frame[2]"));
                        //now use the switch command
                        driver.SwitchTo().Frame(iframeElement1);

                        driver.FindElement(By.XPath("//*[@id='AutoNumber3']/tbody/tr/td[3]/table/tbody/tr[4]/td/a")).Click();
                        Thread.Sleep(10000);
                        driver.SwitchTo().DefaultContent();
                        Thread.Sleep(2000);

                        IWebElement iframeElement2 = driver.FindElement(By.XPath("//*[@id='rbottom']"));
                        //now use the switch command
                        driver.SwitchTo().Frame(iframeElement2);
                        Thread.Sleep(2000);
                        //now use the switch command

                        driver.FindElement(By.XPath("//*[@id='a_searchlink']")).Click();
                        Thread.Sleep(5000);
                        driver.FindElement(By.XPath("//*[@id='ui-id-16']")).Click();
                        Thread.Sleep(5000);
                        driver.FindElement(By.XPath("//*[@id='FullAddress']")).SendKeys(Address);
                        Thread.Sleep(1000);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "FL", "Collier");
                        driver.FindElement(By.XPath("//*[@id='SearchFullAddress']")).Click();
                        Thread.Sleep(6000);
                        gc.CreatePdf_WOP(orderNumber, "Address search Result", driver, "FL", "Collier");


                        try
                        {
                            IWebElement MultiOwnerTable = driver.FindElement(By.Id("flex1"));
                            IList<IWebElement> MultiOwnerRow = MultiOwnerTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiOwnerTD;
                            //string AlterNateID = "", PropertyAddress = "", LegalDescriptoin = "", YearBuilt = "";
                            foreach (IWebElement row1 in MultiOwnerRow)
                            {
                                MultiOwnerTD = row1.FindElements(By.TagName("td"));
                                if (MultiOwnerTD.Count != 0 && MultiOwnerTD.Count != 2 && MultiOwnerTD.Count != 1 && !row1.Text.Contains("Parcel No.") && MultiOwnerTD[1].Text.Trim() != "")
                                {
                                    parcelNumber = MultiOwnerTD[1].Text;
                                    ownername = MultiOwnerTD[3].Text;
                                    PropertyAddress = MultiOwnerTD[5].Text;
                                    string Multi = PropertyAddress + "~" + ownername;
                                    gc.insert_date(orderNumber, parcelNumber, 488, Multi, 1, DateTime.Now);
                                }
                            }
                            //GlobalClass.multiParcel_MiamiDade = "Yes";
                            HttpContext.Current.Session["multiParcel_Collier"] = "Yes";
                            driver.Quit();
                            return "MultiParcel";
                        }
                        catch { }
                    }
                    if (searchType == "ownername")
                    {
                        driver.Navigate().GoToUrl("http://www.collierappraiser.com/");
                        Thread.Sleep(2000);
                        IWebElement iframeElement = driver.FindElement(By.XPath("/html/frameset/frame"));
                        //now use the switch command
                        driver.SwitchTo().Frame(iframeElement);

                        IWebElement iframeElement1 = driver.FindElement(By.XPath("/html/frameset/frame[2]"));
                        //now use the switch command
                        driver.SwitchTo().Frame(iframeElement1);

                        driver.FindElement(By.XPath("//*[@id='AutoNumber3']/tbody/tr/td[3]/table/tbody/tr[4]/td/a")).Click();
                        Thread.Sleep(10000);
                        driver.SwitchTo().DefaultContent();
                        Thread.Sleep(2000);

                        IWebElement iframeElement2 = driver.FindElement(By.XPath("//*[@id='rbottom']"));
                        //now use the switch command
                        driver.SwitchTo().Frame(iframeElement2);
                        Thread.Sleep(2000);
                        //now use the switch command

                        driver.FindElement(By.XPath("//*[@id='a_searchlink']")).Click();
                        Thread.Sleep(6000);
                        driver.FindElement(By.XPath("//*[@id='ui-id-15']")).Click();
                        Thread.Sleep(6000);
                        driver.FindElement(By.XPath("//*[@id='Name1']")).SendKeys(ownername);
                        Thread.Sleep(1000);
                        gc.CreatePdf_WOP(orderNumber, "Owner search", driver, "FL", "Collier");
                        driver.FindElement(By.XPath("//*[@id='SearchName1']")).Click();
                        Thread.Sleep(6000);

                        gc.CreatePdf_WOP(orderNumber, "Owner search Result", driver, "FL", "Collier");
                        try
                        {

                            IWebElement MultiOwnerTable = driver.FindElement(By.Id("flex1"));
                            IList<IWebElement> MultiOwnerRow = MultiOwnerTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiOwnerTD;

                            //string AlterNateID = "", PropertyAddress = "", LegalDescriptoin = "", YearBuilt = "";
                            foreach (IWebElement row1 in MultiOwnerRow)
                            {
                                MultiOwnerTD = row1.FindElements(By.TagName("td"));
                                if (MultiOwnerTD.Count != 0 && MultiOwnerTD.Count != 2 && MultiOwnerTD.Count != 1 && !row1.Text.Contains("Parcel No.") && MultiOwnerTD[1].Text.Trim() != "")
                                {

                                    parcelNumber = MultiOwnerTD[1].Text;

                                    ownername = MultiOwnerTD[3].Text;
                                    PropertyAddress = MultiOwnerTD[5].Text;


                                    string Multi = PropertyAddress + "~" + ownername;
                                    gc.insert_date(orderNumber, parcelNumber, 488, Multi, 1, DateTime.Now);

                                }
                            }

                            //GlobalClass.multiParcel_MiamiDade = "Yes";
                            HttpContext.Current.Session["multiParcel_Collier"] = "Yes";
                            driver.Quit();
                            return "MultiParcel";


                        }
                        catch { }

                    }

                    if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("http://www.collierappraiser.com/");
                        Thread.Sleep(2000);


                        IWebElement iframeElement = driver.FindElement(By.XPath("/html/frameset/frame"));
                        //now use the switch command
                        driver.SwitchTo().Frame(iframeElement);

                        IWebElement iframeElement1 = driver.FindElement(By.XPath("/html/frameset/frame[2]"));
                        //now use the switch command
                        driver.SwitchTo().Frame(iframeElement1);

                        driver.FindElement(By.XPath("//*[@id='AutoNumber3']/tbody/tr/td[3]/table/tbody/tr[4]/td/a")).Click();
                        Thread.Sleep(10000);
                        driver.SwitchTo().DefaultContent();
                        Thread.Sleep(2000);

                        IWebElement iframeElement2 = driver.FindElement(By.XPath("//*[@id='rbottom']"));
                        //now use the switch command
                        driver.SwitchTo().Frame(iframeElement2);
                        Thread.Sleep(2000);
                        //now use the switch command

                        driver.FindElement(By.XPath("//*[@id='a_searchlink']")).Click();
                        Thread.Sleep(6000);
                        driver.FindElement(By.XPath("//*[@id='ui-id-17']")).Click();
                        Thread.Sleep(6000);
                        driver.FindElement(By.XPath("//*[@id='ParcelID']")).SendKeys(parcelNumber);
                        Thread.Sleep(1000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "FL", "Collier");
                        driver.FindElement(By.XPath("//*[@id='SearchParcelID']")).Click();
                        Thread.Sleep(6000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Result", driver, "FL", "Collier");

                    }
                    try
                    {
                        IAlert alert = driver.SwitchTo().Alert();
                        if(alert.Text.Contains("No Parcels Found"))
                        {
                            HttpContext.Current.Session["Nodata_FLCollier"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    string SiteAdr = "", MapNo = "", Strapno = "", Section = "", Township = "", Range = "", Acres = "", MillageArea = "", Condo = "", UseCode = "", School = "", Other = "", Total = "", YearBuilt = "";

                    gc.CreatePdf(orderNumber, parcelNumber, "Property Deatil", driver, "FL", "Collier");
                    parcelNumber = driver.FindElement(By.XPath("//*[@id='ParcelID']")).Text;
                    ownername = driver.FindElement(By.XPath("//*[@id='Name1']")).Text;
                    try
                    {
                        ownername = driver.FindElement(By.XPath("//*[@id='Name1']")).Text + " " + driver.FindElement(By.XPath("//*[@id='Name2']")).Text;
                    }
                    catch { }
                    try
                    {
                        SiteAdr = driver.FindElement(By.XPath("//*[@id='FullAddress']")).Text + " " + driver.FindElement(By.XPath("//*[@id='City']")).Text + " " + driver.FindElement(By.XPath("//*[@id='State']")).Text;
                    }
                    catch { }
                    MapNo = driver.FindElement(By.XPath("//*[@id='MapNumber']")).Text;
                    Strapno = driver.FindElement(By.XPath("//*[@id='StrapNumber']")).Text;
                    Section = driver.FindElement(By.XPath("//*[@id='Section']")).Text;
                    Township = driver.FindElement(By.XPath("//*[@id='Township']")).Text;
                    Range = driver.FindElement(By.XPath("//*[@id='Range']")).Text;
                    Acres = driver.FindElement(By.XPath("//*[@id='TotalAcres']")).Text;
                    MillageArea = driver.FindElement(By.XPath("//*[@id='MillageArea']")).Text;
                    Condo = driver.FindElement(By.XPath("//*[@id='scDescription']")).Text;
                    UseCode = driver.FindElement(By.XPath("//*[@id='ucDescription']")).Text;
                    School = driver.FindElement(By.XPath("//*[@id='School']")).Text;
                    Other = driver.FindElement(By.XPath("//*[@id='Other']")).Text;
                    Total = driver.FindElement(By.XPath("//*[@id='Total']")).Text;

                    driver.FindElement(By.XPath("//*[@id='ui-id-2']")).Click();
                    Thread.Sleep(4000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Property Summary", driver, "FL", "Collier");
                    try
                    {
                        YearBuilt = driver.FindElement(By.XPath("//*[@id='trBuilding1']/td[2]")).Text;
                    }
                    catch { }
                    string property = ownername + "~" + SiteAdr + "~" + MapNo + "~" + Strapno + "~" + Section + "~" + Township + "~" + Range + "~" + Acres + "~" + MillageArea + "~" + Condo + "~" + UseCode + "~" + School + "~" + Other + "~" + Total + "~" + YearBuilt;

                    gc.insert_date(orderNumber, parcelNumber, 469, property, 1, DateTime.Now);
                    driver.FindElement(By.XPath("//*[@id='ui-id-1']")).Click();
                    Thread.Sleep(4000);

                    string LandValue = "", ImprovedValue = "", Marketvalue = "", SaveOurHome = "", AssessedValue = "", Homestead = "", SchoolTaxableValue = "", AdditionalHomestead = "", TaxableValue = "";
                    IWebElement TaxHisTBD = driver.FindElement(By.XPath("//*[@id='PropSum']/table[6]/tbody/tr/td[3]/table/tbody"));
                    IList<IWebElement> TaxHisTRD = TaxHisTBD.FindElements(By.TagName("tr"));
                    IList<IWebElement> TaxHisTDD;

                    foreach (IWebElement row1 in TaxHisTRD)
                    {
                        TaxHisTDD = row1.FindElements(By.TagName("td"));
                        if (!row1.Text.Contains("Certified Tax Roll") && !row1.Text.Contains("(Subject") && row1.Text.Trim() != "" && TaxHisTDD.Count != 1)
                        {
                            LandValue = TaxHisTDD[0].Text + "~" + LandValue;
                            ImprovedValue = TaxHisTDD[1].Text + "~" + ImprovedValue;
                        }
                    }

                    LandValue = LandValue + "lk";
                    ImprovedValue = ImprovedValue + "lk";

                    LandValue = LandValue.Replace("~lk", "");
                    ImprovedValue = ImprovedValue.Replace("~lk", "");
                    DBconnection dbconn = new DBconnection();
                    dbconn.ExecuteQuery("update  data_field_master set Data_Fields_Text='" + LandValue + "' where Id = '" + 470 + "'");
                    gc.insert_date(orderNumber, parcelNumber, 470, ImprovedValue, 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("https://collier.county-taxes.com/public");
                    Thread.Sleep(2000);

                    driver.FindElement(By.XPath("//*[@id='search-controls']/div/form[1]/div[1]/div/input")).SendKeys(parcelNumber);
                    gc.CreatePdf(orderNumber, parcelNumber, "Taxinfo", driver, "FL", "Collier");
                    driver.FindElement(By.XPath("//*[@id='search-controls']/div/form[1]/div[1]/div/span/button")).SendKeys(Keys.Enter);
                    Thread.Sleep(3000);
                    try
                    {
                        IWebElement ITaxSearch = driver.FindElement(By.LinkText("Full bill history"));
                        string strITaxSearch = ITaxSearch.GetAttribute("href");
                        driver.Navigate().GoToUrl(strITaxSearch);
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Full bill history", driver, "FL", "Collier");
                    }
                    catch { }

                    try
                    {
                        IWebElement ITaxReal = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table"));
                        IList<IWebElement> ITaxRealRow = ITaxReal.FindElements(By.TagName("tr"));
                        IList<IWebElement> ITaxRealTd;
                        int k = 0;
                        foreach (IWebElement ItaxReal in ITaxRealRow)
                        {
                            ITaxRealTd = ItaxReal.FindElements(By.TagName("td"));
                            if ((k <= 2 && ItaxReal.Text.Contains("Annual Bill")) || ItaxReal.Text.Contains("Pay this bill:"))
                            {

                                string yearbill = ITaxRealTd[0].Text;
                                IWebElement ITaxBillCount = ITaxRealTd[0].FindElement(By.TagName("a"));
                                string strTaxReal = ITaxBillCount.GetAttribute("href");
                                strTaxRealestate.Add(strTaxReal);
                                try
                                {
                                    IWebElement ITaxBill = ITaxRealTd[3].FindElement(By.TagName("a"));
                                    string BillTax = ITaxBill.GetAttribute("href");
                                    gc.downloadfile(BillTax, orderNumber, parcelNumber, "Taxbill.pdf" + yearbill, "FL", "Collier");
                                }
                                catch
                                {
                                    IWebElement ITaxBill = ITaxRealTd[4].FindElement(By.TagName("a"));
                                    string BillTax = ITaxBill.GetAttribute("href");
                                    gc.downloadfile(BillTax, orderNumber, parcelNumber, "Taxbill.pdf" + yearbill, "FL", "Collier");

                                }
                                k++;
                            }

                        }

                        //Tax History Details

                        IWebElement IBillHistorytable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table"));
                        IList<IWebElement> IBillHistoryRow = IBillHistorytable.FindElements(By.TagName("tr"));
                        IList<IWebElement> IBillHistoryTD;
                        foreach (IWebElement bill in IBillHistoryRow)
                        {
                            IBillHistoryTD = bill.FindElements(By.TagName("td"));
                            if (IBillHistoryTD.Count != 0)
                            {
                                try
                                {
                                    strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                    strBalance = IBillHistoryTD[1].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                    strBillDate = IBillHistoryTD[2].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                    strBillPaid = IBillHistoryTD[3].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                }
                                catch
                                {
                                    strBillDate = "";
                                    strBillPaid = "";
                                }
                                if (strBillPaid.Contains("Print (PDF)"))
                                {
                                    strBillPaid = "";
                                }
                                string strTaxHistory = strBill + "~" + strBalance + "~" + strBillDate + "~" + strBillPaid;
                                gc.insert_date(orderNumber, parcelNumber, 471, strTaxHistory, 1, DateTime.Now);
                            }
                        }

                        IWebElement IBillHistoryfoottable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table/tfoot"));
                        IList<IWebElement> IBillHistoryfootRow = IBillHistoryfoottable.FindElements(By.TagName("tr"));
                        IList<IWebElement> IBillHistoryfootTD;
                        foreach (IWebElement bill in IBillHistoryRow)
                        {
                            IBillHistoryfootTD = bill.FindElements(By.TagName("th"));
                            if (IBillHistoryfootTD.Count != 0 && bill.Text.Contains("Total"))
                            {
                                try
                                {
                                    strFBill = IBillHistoryfootTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                    strFBalance = IBillHistoryfootTD[1].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                    strFBillDate = IBillHistoryfootTD[2].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                    strFBillPaid = IBillHistoryfootTD[3].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                }
                                catch
                                {
                                    strFBillDate = "";
                                    strFBillPaid = "";
                                }
                                if (strBillPaid.Contains("Print (PDF)"))
                                {
                                    strBillPaid = "";
                                }
                                string strTaxHistory = strFBill + "~" + strFBalance + "~" + strFBillDate + "~" + strFBillPaid;
                                gc.insert_date(orderNumber, parcelNumber, 471, strTaxHistory, 1, DateTime.Now);
                            }
                        }
                        foreach (string real in strTaxRealestate)
                        {
                            driver.Navigate().GoToUrl(real);
                            Thread.Sleep(4000);

                            try
                            {
                                TaxYear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[1]")).Text;
                                TaxYear = WebDriverTest.After(TaxYear, "Real Estate").Trim();
                                string s = TaxYear;
                                string[] words = TaxYear.Split(' ');
                                TaxYear = words[0];
                            }
                            catch
                            {
                                TaxYear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[1]")).Text;
                                TaxYear = WebDriverTest.After(TaxYear, "Real Estate").Trim();
                                string s = TaxYear;
                                string[] words = TaxYear.Split(' ');
                                TaxYear = words[0];
                            }

                            gc.CreatePdf(orderNumber, parcelNumber, "Tax details" + TaxYear, driver, "FL", "Collier");
                            IWebElement multitableElement3;
                            try
                            {
                                multitableElement3 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[2]/tbody"));
                            }
                            catch
                            {
                                multitableElement3 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[2]/tbody"));
                            }
                            IList<IWebElement> multitableRow3 = multitableElement3.FindElements(By.TagName("tr"));

                            IList<IWebElement> multirowTD3;
                            foreach (IWebElement row in multitableRow3)
                            {
                                multirowTD3 = row.FindElements(By.TagName("td"));
                                if (multirowTD3.Count != 1 && multirowTD3[1].Text.Trim() != "")
                                {
                                    string tax_distri = TaxYear + "~" + multirowTD3[0].Text.Trim() + "~" + "Ad Valorem" + "~" + multirowTD3[1].Text.Trim() + "~" + multirowTD3[2].Text.Trim() + "~" + multirowTD3[3].Text.Trim() + "~" + multirowTD3[4].Text.Trim() + "~" + "" + "~" + multirowTD3[5].Text.Trim();
                                    gc.insert_date(orderNumber, parcelNumber, 472, tax_distri, 1, DateTime.Now);
                                }
                            }

                            //total advalorem
                            IWebElement multitableElement31;
                            try
                            {
                                multitableElement31 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[2]/tfoot"));
                            }
                            catch
                            {

                                multitableElement31 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[2]/tfoot"));
                            }
                            IList<IWebElement> multitableRow31 = multitableElement31.FindElements(By.TagName("tr"));

                            IList<IWebElement> multirowTD31;
                            foreach (IWebElement row in multitableRow31)
                            {
                                multirowTD31 = row.FindElements(By.TagName("td"));
                                if (multirowTD31.Count != 1)
                                {
                                    string tax_distri1 = TaxYear + "~" + "Total:" + "~" + "Ad Valorem" + "~" + multirowTD31[0].Text.Trim() + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + multirowTD31[1].Text.Trim();
                                    gc.insert_date(orderNumber, parcelNumber, 472, tax_distri1, 1, DateTime.Now);
                                }
                            }
                            //  Non - Ad Valorem                    
                            try
                            {
                                IWebElement multitableElement32;
                                try
                                {
                                    multitableElement32 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[3]/tbody"));
                                }
                                catch
                                {
                                    multitableElement32 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[3]/tbody"));

                                }
                                IList<IWebElement> multitableRow32 = multitableElement32.FindElements(By.TagName("tr"));
                                IList<IWebElement> multirowTD32;
                                foreach (IWebElement row in multitableRow32)
                                {
                                    multirowTD32 = row.FindElements(By.TagName("td"));
                                    if (multirowTD32.Count != 1 && multirowTD32[0].Text.Trim() != "")
                                    {
                                        string tax_distri2 = TaxYear + "~" + multirowTD32[0].Text.Trim() + "~" + "Non-Ad Valorem " + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + multirowTD32[1].Text.Trim() + "~" + multirowTD32[2].Text.Trim();
                                        gc.insert_date(orderNumber, parcelNumber, 472, tax_distri2, 1, DateTime.Now);
                                    }
                                }
                                //total non-advalorem

                                IWebElement multitableElement33;
                                try
                                {

                                    multitableElement33 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[3]/tfoot"));
                                }
                                catch
                                {
                                    multitableElement33 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[3]/tfoot"));
                                }
                                IList<IWebElement> multitableRow33 = multitableElement33.FindElements(By.TagName("tr"));
                                IList<IWebElement> multirowTD33;
                                foreach (IWebElement row in multitableRow33)
                                {
                                    multirowTD33 = row.FindElements(By.TagName("td"));
                                    if (multirowTD33.Count != 0)
                                    {
                                        string tax_distri1 = TaxYear + "~" + "Total:" + "~" + "Non-Ad Valorem " + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + multirowTD33[0].Text.Trim();
                                        gc.insert_date(orderNumber, parcelNumber, 472, tax_distri1, 1, DateTime.Now);
                                    }
                                }
                            }
                            catch { }
                            //  Taxing_authority~Tax_type~Millage~Assessed~Exemption~Taxable~Rate~Tax
                            //Tax info                     
                            try
                            {
                                TaxAmount = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/p")).Text.Replace("Combined taxes and assessments:", "");
                                PaidAmount = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[6]/div/div[1]/div/div")).Text;
                                PaidAmount = PaidAmount.Replace("\r\n", "&");
                                PaidAmount = WebDriverTest.Before(PaidAmount, "&");
                                PaidAmount = WebDriverTest.After(PaidAmount, "$").Trim();

                                ReceiptNumber = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[6]/div/div[1]/div/div/div")).Text.Replace("Receipt", "");
                                IWebElement tbmulti = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[1]/tbody"));
                                IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));
                                IList<IWebElement> TDmulti;
                                foreach (IWebElement row in TRmulti)
                                {
                                    TDmulti = row.FindElements(By.TagName("td"));
                                    if (TDmulti.Count != 0)
                                    {
                                        Account_number = TDmulti[0].Text;
                                        string Escro_Code = TDmulti[2].Text;
                                        Millage_Code = TDmulti[3].Text;

                                        string tax_info1 = Account_number + "~" + Millage_Code + "~" + Escro_Code + "~" + TaxYear + "~" + TaxAmount + "~" + PaidAmount + "~" + ReceiptNumber + "~" + "Doug Belden, Tax Collector P.O. Box 30012 Tampa, Florida 33630-3012";

                                        gc.insert_date(orderNumber, parcelNumber, 473, tax_info1, 1, DateTime.Now);
                                    }
                                }
                            }
                            catch
                            {


                                TaxAmount = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/p")).Text.Replace("Combined taxes and assessments:", "");

                                PaidAmount = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[3]")).Text;
                                if (PaidAmount.Contains("Pay this bill"))
                                {
                                    PaidAmount = "";
                                    ReceiptNumber = "";

                                }
                                else
                                {
                                    PaidAmount = PaidAmount.Replace("\r\n", "&");
                                    var PaidAmount1 = PaidAmount.Split('&');
                                    PaidAmount = PaidAmount1[0];
                                    PaidAmount = WebDriverTest.After(PaidAmount, "$").Trim();
                                    ReceiptNumber = PaidAmount1[1];

                                }

                                IWebElement tbmulti = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[1]/tbody"));
                                IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));

                                IList<IWebElement> TDmulti;
                                foreach (IWebElement row in TRmulti)
                                {
                                    TDmulti = row.FindElements(By.TagName("td"));
                                    if (TDmulti.Count != 0)
                                    {
                                        Account_number = TDmulti[0].Text;
                                        string Escro_Code = TDmulti[2].Text;
                                        Millage_Code = TDmulti[3].Text;
                                        string tax_info1 = Account_number + "~" + Millage_Code + "~" + Escro_Code + "~" + TaxYear + "~" + TaxAmount + "~" + PaidAmount + "~" + ReceiptNumber + "~" + "Doug Belden, Tax Collector P.O. Box 30012 Tampa, Florida 33630-3012";
                                        gc.insert_date(orderNumber, parcelNumber, 473, tax_info1, 1, DateTime.Now);
                                    }
                                }
                            }
                            string IfPaidBy = "", PlesePay = "", DueDate = "", deli = "";
                            try
                            {

                                IWebElement multitableElement26 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]/tbody"));
                                IList<IWebElement> multitableRow26 = multitableElement26.FindElements(By.TagName("tr"));

                                IList<IWebElement> multirowTD26;
                                foreach (IWebElement row in multitableRow26)
                                {

                                    multirowTD26 = row.FindElements(By.TagName("td"));
                                    int iRowsCount = multirowTD26.Count;
                                    for (int n = 0; n < iRowsCount; n++)
                                    {
                                        if ((!multirowTD26[n].Text.Contains("Face Amt")) && (!multirowTD26[n].Text.Contains("Certificate")))
                                        {

                                            IfPaidBy = multirowTD26[n].Text.Replace("\r\n", "~");
                                            var IfpaySplit = IfPaidBy.Split('~');
                                            IfPaidBy = IfpaySplit[0];
                                            PlesePay = IfpaySplit[1];
                                            if (PlesePay == "$0.00")
                                                deli = "paid";
                                            else
                                                deli = "deliquent";
                                            DueDate = TaxYear + "~" + deli + "~" + IfPaidBy + "~" + PlesePay;
                                            gc.insert_date(orderNumber, parcelNumber, 474, DueDate, 1, DateTime.Now);

                                        }

                                    }
                                }
                            }
                            //If_paid_by~Please_Pay
                            catch
                            {
                                IWebElement multitableElement26 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]/tbody"));
                                IList<IWebElement> multitableRow26 = multitableElement26.FindElements(By.TagName("tr"));

                                IList<IWebElement> multirowTD26;
                                foreach (IWebElement row in multitableRow26)
                                {
                                    multirowTD26 = row.FindElements(By.TagName("td"));
                                    int iRowsCount = multirowTD26.Count;
                                    for (int n = 0; n < iRowsCount; n++)
                                    {
                                        if ((!multirowTD26[n].Text.Contains("Face Amt")) && (!multirowTD26[n].Text.Contains("Certificate")))
                                        {

                                            IfPaidBy = multirowTD26[n].Text.Replace("\r\n", "~");
                                            var IfpaySplit = IfPaidBy.Split('~');
                                            IfPaidBy = IfpaySplit[0];
                                            PlesePay = IfpaySplit[1];
                                            if (PlesePay == "$0.00")
                                                deli = "paid";
                                            else
                                                deli = "deliquent";
                                            DueDate = TaxYear + "~" + deli + "~" + IfPaidBy + "~" + PlesePay;
                                            gc.insert_date(orderNumber, parcelNumber, 474, DueDate, 1, DateTime.Now);
                                        }
                                    }
                                }
                            }

                            //*[@id="content"]/div[1]/div[3]/div[1]/ul/li[1]/a
                            driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[3]/div[1]/ul/li[1]/a")).Click();
                            Thread.Sleep(5000);
                            gc.CreatePdf(orderNumber, parcelNumber, "parcel details" + TaxYear, driver, "FL", "Collier");
                        }
                    }
                    catch { }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "FL", "Collier", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "FL", "Collier");
                    //gc.MMREM_Template(orderNumber, parcelNumber, "", driver, "FL", "Collier", "138", "4");
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
