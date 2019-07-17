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
using System.ComponentModel;
using System.Text;
using HtmlAgilityPack;
using iTextSharp.text;
using System.Text.RegularExpressions;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Support.Extensions;
using System.Net;
using OpenQA.Selenium.Firefox;
using System.Diagnostics;
using OpenQA.Selenium.Remote;
using Org.BouncyCastle.Utilities;

namespace ScrapMaricopa.Scrapsource
{
    public class Webdriver_LexingtonSC
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();
        IWebElement Address1;
        public string FTP_LexingtonSC(string address, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            List<string> PdfDownloadlink = new List<string>();
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string Parcel_number = "", Tax_Authority = "", TaxSale = "", Delinquent_Tax = "", Propertyresult = "", LastYear = "", LegalDescription = "", Addresshrf = "", duedata = "";
            // driver = new ChromeDriver();
            // driver = new PhantomJSDriver();
            using (driver = new PhantomJSDriver())//PhantomJSDriver
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    if (searchType == "titleflex")
                    {
                        string titleaddress = address;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "SC", "Lexington");

                        if (HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes")
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                        if (parcelNumber.Trim() == "")
                        {
                            HttpContext.Current.Session["Nodata_LexingtonSC"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    driver.Navigate().GoToUrl("http://www.lex-co.com/PCSearch/TaxInfoPropertySearch.asp");
                    if (searchType == "address")
                    {
                        driver.FindElement(By.XPath("//*[@id='parcelINQ']/table/tbody/tr[4]/td[2]/input")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "SearchBefore", driver, "SC", "Lexington");
                        driver.FindElement(By.XPath("//*[@id='parcelINQ']/table/tbody/tr[11]/td/input")).Click();
                        Thread.Sleep(2000);
                        int Max = 0;
                        try
                        {
                            string Pin = "";
                            IWebElement Multiparceladdress = driver.FindElement(By.XPath("//*[@id='divcenter']/table"));
                            IList<IWebElement> Multiparcelrow = Multiparceladdress.FindElements(By.TagName("tr"));
                            IList<IWebElement> Multiparcelid;
                            gc.CreatePdf_WOP(orderNumber, "SearchAfter", driver, "SC", "Lexington");
                            string currentyear = DateTime.Now.Year.ToString();
                            foreach (IWebElement multiparcel in Multiparcelrow)
                            {
                                Multiparcelid = multiparcel.FindElements(By.TagName("td"));
                                if (Multiparcelid.Count != 0 && currentyear.Trim() == Multiparcelid[0].Text.Trim())
                                {
                                    Address1 = Multiparcelid[1].FindElement(By.TagName("a"));
                                    Addresshrf = Address1.GetAttribute("href");
                                    // string Stnumber = Multiparcelid[1].Text;
                                    string Owner = Multiparcelid[2].Text;
                                    string street = Multiparcelid[3].Text;
                                    //string Unit = Multiparcelid[4].Text;
                                    string Addressst = Owner + "~" + street;
                                    // string Owner = Multiparcelid[1].Text;
                                    Pin = Address1.Text;
                                    // string Multiparcel = Addressst;
                                    gc.insert_date(orderNumber, Pin, 1706, Addressst, 1, DateTime.Now);
                                    Max++;
                                }
                                if (Multiparcelid.Count != 0 && Multiparcelid[0].Text.Trim() != "")
                                {
                                    LastYear = Multiparcelid[0].Text;
                                }
                            }
                            Multiparcelrow.Reverse();
                            int rows_count1 = Multiparcelrow.Count;

                            if (Addresshrf == "")
                            {
                                for (int row = 0; row < rows_count1; row++)
                                {
                                    IList<IWebElement> Columns_row = Multiparcelrow[row].FindElements(By.TagName("td"));
                                    int columns_count = Columns_row.Count;
                                    if (columns_count != 0 && Columns_row[0].Text.Trim() == LastYear)
                                    {
                                        Address1 = Columns_row[1].FindElement(By.TagName("a"));
                                        Addresshrf = Address1.GetAttribute("href");
                                        Max++;
                                        break;
                                    }
                                }
                            }
                            if (Max == 1)
                            {
                                driver.Navigate().GoToUrl(Addresshrf);
                                Thread.Sleep(2000);
                            }
                            if (Max > 1 && Max < 26)
                            {
                                HttpContext.Current.Session["multiParcel_Lexington"] = "Maximum";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Max > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Lexington_Multicount"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                            if (Max == 0)
                            {
                                HttpContext.Current.Session["Nodata_LexingtonSC"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.XPath("//*[@id='parcelINQ']/table/tbody/tr[2]/td[2]/input")).SendKeys(parcelNumber.Replace("-", ""));
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "SC", "Lexington");
                        driver.FindElement(By.XPath("//*[@id='parcelINQ']/table/tbody/tr[11]/td/input")).Click();
                        Thread.Sleep(2000);
                        int Max = 0;
                        try
                        {
                            IWebElement Multiparceladdress = driver.FindElement(By.XPath("//*[@id='divcenter']/table"));
                            IList<IWebElement> Multiparcelrow = Multiparceladdress.FindElements(By.TagName("tr"));
                            IList<IWebElement> Multiparcelid;
                            gc.CreatePdf_WOP(orderNumber, "SearchAfter", driver, "SC", "Lexington");
                            string currentyear = DateTime.Now.Year.ToString();
                            foreach (IWebElement multiparcel in Multiparcelrow)
                            {
                                Multiparcelid = multiparcel.FindElements(By.TagName("td"));
                                if (Multiparcelid.Count != 0 && currentyear.Trim() == Multiparcelid[0].Text.Trim())
                                {
                                    Address1 = Multiparcelid[1].FindElement(By.TagName("a"));
                                    Addresshrf = Address1.GetAttribute("href");
                                    driver.Navigate().GoToUrl(Addresshrf);
                                    break;
                                }
                            }
                        }
                        catch { }
                    }
                    //if (searchType == "ownername")
                    //{
                    //    driver.FindElement(By.XPath("//*[@id='parcelINQ']/table/tbody/tr[3]/td[2]/input")).SendKeys(ownername);
                    //    driver.FindElement(By.XPath("//*[@id='parcelINQ']/table/tbody/tr[11]/td/input")).Click();
                    //    int Max = 0;
                    //    try
                    //    {
                    //        IWebElement Multiparceladdress = driver.FindElement(By.XPath("//*[@id='divcenter']/table"));
                    //        IList<IWebElement> Multiparcelrow = Multiparceladdress.FindElements(By.TagName("tr"));
                    //        IList<IWebElement> Multiparcelid;
                    //        gc.CreatePdf_WOP(orderNumber, "SearchAfter", driver, "SC", "Lexington");
                    //        string currentyear = DateTime.Now.Year.ToString();
                    //        foreach (IWebElement multiparcel in Multiparcelrow)
                    //        {
                    //            Multiparcelid = multiparcel.FindElements(By.TagName("td"));
                    //            if (Multiparcelid.Count != 0)
                    //            {
                    //                Address1 = Multiparcelid[1].FindElement(By.TagName("a"));
                    //                Addresshrf = Address1.GetAttribute("href");
                    //                // string Stnumber = Multiparcelid[1].Text;
                    //                string Owner = Multiparcelid[2].Text;
                    //                string street = Multiparcelid[3].Text;
                    //                //string Unit = Multiparcelid[4].Text;
                    //                string Addressst = Owner + "~" + street;
                    //                // string Owner = Multiparcelid[1].Text;
                    //                string Pin = Address1.Text;
                    //                // string Multiparcel = Addressst;
                    //                gc.insert_date(orderNumber, Pin, 1706, Addressst, 1, DateTime.Now);
                    //                Max++;
                    //            }
                    //        }
                    //        if (Max == 1)
                    //        {
                    //            driver.Navigate().GoToUrl(Addresshrf);
                    //            Thread.Sleep(2000);
                    //        }
                    //        if (Max > 1 && Max < 26)
                    //        {
                    //            HttpContext.Current.Session["multiParcel_Lexington"] = "Maximum";
                    //            driver.Quit();
                    //            return "MultiParcel";
                    //        }
                    //        if (Max > 25)
                    //        {
                    //            HttpContext.Current.Session["multiParcel_Lexington_Multicount"] = "Maximum";
                    //            driver.Quit();
                    //            return "Maximum";
                    //        }
                    //        if (Max == 0)
                    //        {
                    //            HttpContext.Current.Session["Nodata_LexingtonSC"] = "Yes";
                    //            driver.Quit();
                    //            return "No Data Found";
                    //        }
                    //    }
                    //    catch { }
                    //}

                    //try
                    //{
                    //    IWebElement INodata = driver.FindElement(By.Id("divcenter"));
                    //    if(INodata.Text.Contains("search produced no results"))
                    //    {
                    //        HttpContext.Current.Session["Nodata_LexingtonSC"] = "Yes";
                    //        driver.Quit();
                    //        return "No Data Found";
                    //    } 
                    //}
                    //catch { }

                    string Propertytable = driver.FindElement(By.XPath("//*[@id='divcenter']/table[1]/tbody/tr/td/table/tbody")).Text;
                    Parcel_number = gc.Between(Propertytable, "TMS#:", "TAX YEAR:");
                    string owner = gc.Between(Propertytable, "OWNER:", "ADDRESS:");
                    string MailingAddress = gc.Between(Propertytable, "ADDRESS:", "PROPERTY ADDRESS:");
                    string propertyaddress = gc.Between(Propertytable, "PROPERTY ADDRESS:", "LEGAL DESCRIPTION:");
                    if (Propertytable.Contains("DEED BOOK & PAGE:"))
                    {
                        LegalDescription = gc.Between(Propertytable, "LEGAL DESCRIPTION:", "DEED BOOK & PAGE:");
                    }
                    if (Propertytable.Contains("PLAT:"))
                    {
                        LegalDescription = gc.Between(Propertytable, "LEGAL DESCRIPTION:", "PLAT:");
                    }
                    string LandUse = gc.Between(Propertytable, "LAND USE:", "TAX DISTRICT:");
                    string Yearbuilt = driver.FindElement(By.XPath("//*[@id='divcenter']/table[2]/tbody/tr/td[2]/table/tbody/tr[4]/td[2]")).Text;
                    string AssessedYear = gc.Between(Propertytable, "TAX YEAR:", "OWNER:");
                    Propertyresult = owner + "~" + propertyaddress + "~" + MailingAddress + "~" + Yearbuilt + "~" + LegalDescription + "~" + LandUse + "~" + AssessedYear;
                    gc.insert_date(orderNumber, Parcel_number, 1707, Propertyresult, 1, DateTime.Now);
                    gc.CreatePdf(orderNumber, parcelNumber, "Assessment", driver, "SC", "Lexington");
                    IWebElement AssessmentInfoTable = driver.FindElement(By.XPath("//*[@id='divcenter']/table[2]/tbody/tr/td[1]/table/tbody"));
                    IList<IWebElement> AssessmentInforow = AssessmentInfoTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> AssessmentInfoid;
                    foreach (IWebElement AssessmentInfo in AssessmentInforow)
                    {
                        AssessmentInfoid = AssessmentInfo.FindElements(By.TagName("td"));
                        if (AssessmentInfoid.Count != 0 & !AssessmentInfo.Text.Contains("ASSESSMENT INFORMATION"))
                        {
                            string AssessmentInfoResult = AssessmentInfoid[0].Text + "~" + AssessmentInfoid[1].Text;
                            gc.insert_date(orderNumber, Parcel_number, 1720, AssessmentInfoResult, 1, DateTime.Now);
                        }
                    }
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    //Tax
                    driver.Navigate().GoToUrl("http://www.lex-co.com/PCSearch/TaxInfoPropertySearch.asp");
                    driver.FindElement(By.XPath("//*[@id='taxINQ']/table/tbody/tr/td[1]/table/tbody/tr[2]/td[2]/input")).SendKeys(Parcel_number.Replace("-", "").Trim());
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax search", driver, "SC", "Lexington");
                    driver.FindElement(By.XPath("//*[@id='taxINQ']/table/tbody/tr/td[1]/table/tbody/tr[6]/td/input")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax search Result", driver, "SC", "Lexington");
                    try
                    {
                        string Nodata = driver.FindElement(By.XPath("//*[@id='page']/div[2]/p[2]/font")).Text;
                        if (Nodata.Contains("Your search produced"))
                        {
                            HttpContext.Current.Session["Nodata_LexingtonSC"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    IWebElement TaxSearchTable = driver.FindElement(By.XPath("//*[@id='page']/div[2]/table"));
                    IList<IWebElement> TaxSearchrow = TaxSearchTable.FindElements(By.TagName("tr"));
                    //TaxSearchrow.Reverse();
                    IList<IWebElement> TaxSearchid;
                    //int rows_count = TaxSearchrow.Count - 1;
                    foreach (IWebElement TaxSearch in TaxSearchrow)
                    {
                        TaxSearchid = TaxSearch.FindElements(By.TagName("td"));

                        if (TaxSearchid.Count != 0 && TaxSearchid[0].Text.Trim() != "")
                        {
                            string taxSearchResult = TaxSearchid[0].Text + "~" + TaxSearchid[1].Text + "~" + TaxSearchid[2].Text + "~" + TaxSearchid[3].Text + "~" + TaxSearchid[4].Text + "~" + TaxSearchid[5].Text + "~" + TaxSearchid[6].Text;
                            gc.insert_date(orderNumber, Parcel_number, 1717, taxSearchResult, 1, DateTime.Now);
                        }
                        if (TaxSearchid.Count != 0 && TaxSearchid[4].Text.Contains("TAX"))
                        {
                            TaxSale = "Please contact county for tax information" + "~" + "";
                            gc.insert_date(orderNumber, Parcel_number, 1726, TaxSale, 1, DateTime.Now);
                        }
                    }

                    List<string> ParcelSearch = new List<string>();
                    IWebElement ParcelTB = driver.FindElement(By.XPath("//*[@id='page']/div[2]/table"));
                    IList<IWebElement> ParcelTR = ParcelTB.FindElements(By.TagName("tr"));
                    ParcelTR.Reverse();
                    int rows_count = ParcelTR.Count - 1;

                    for (int row = 0; row < rows_count; row++)
                    {
                        if (row == rows_count - 3 || row == rows_count - 1 || row == rows_count - 5)
                        {
                            IList<IWebElement> Columns_row = ParcelTR[row].FindElements(By.TagName("td"));
                            int columns_count = Columns_row.Count;
                            if (columns_count != 0)
                            {
                                IWebElement ParcelBill_link = Columns_row[0].FindElement(By.TagName("a"));
                                string Parcelurl = ParcelBill_link.GetAttribute("href");
                                ParcelSearch.Add(Parcelurl);
                            }
                        }
                    }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    foreach (string taxlink in ParcelSearch)
                    {
                        driver.Navigate().GoToUrl(taxlink);
                        Thread.Sleep(2000);
                        string addres1 = driver.FindElement(By.XPath("//*[@id='divcenter']/table[3]/tbody/tr[3]")).Text.Trim();
                        string address2 = driver.FindElement(By.XPath("//*[@id='divcenter']/table[3]/tbody/tr[4]")).Text;
                        string address3 = addres1 + address2;
                        string Billtable = driver.FindElement(By.XPath("//*[@id='divcenter']/table[2]/tbody")).Text;
                        string Bill = gc.Between(Billtable, "Bill#", "TxYr");
                        string TaxYear = gc.Between(Billtable, "TxYr", "Rev#");
                        string Owner = driver.FindElement(By.XPath("//*[@id='divcenter']/table[3]/tbody/tr[2]")).Text;
                        string TaxFeestable = driver.FindElement(By.XPath("//*[@id='divcenter']/table[4]/tbody")).Text;
                        string Taxamount = gc.Between(TaxFeestable, "TAXES", "COSTS");
                        string credit = gc.Between(TaxFeestable, "CREDIT/RELIEF", "FEES");
                        string HomesteadExemption = gc.Between(TaxFeestable, "H/S EMPT", "TOTAL");
                        string Penalty = gc.Between(TaxFeestable, "PENALTY", "PAID");
                        string Interest = gc.Between(TaxFeestable, "INTERNET/INTEREST", "REFUND");
                        string Costs = gc.Between(TaxFeestable, "COSTS", "ASMT DEDCT");
                        string Fees = gc.Between(TaxFeestable, "FEES", "ADJ RATIO");
                        string Total = gc.Between(TaxFeestable, "TOTAL", "# OF MONTHS");
                        string Paid = gc.Between(TaxFeestable, "PAID", "Adj Value");
                        string Refund = GlobalClass.After(TaxFeestable, "REFUND");
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax search Result" + TaxYear, driver, "SC", "Lexington");
                        //property type
                        string HomesteadPercentage = "";
                        IWebElement IHome = null;
                        try
                        {
                            IHome = driver.FindElement(By.XPath("//*[@id='divcenter']/center[2]/table/tbody"));
                        }
                        catch { }
                        try
                        {
                            if (IHome == null)
                            {
                                IHome = driver.FindElement(By.XPath("//*[@id='divcenter']/center[2]/table/tbody"));
                            }
                        }
                        catch { }
                        IList<IWebElement> IHomerow = IHome.FindElements(By.TagName("tr"));
                        IList<IWebElement> IHomeid;
                        foreach (IWebElement home in IHomerow)
                        {
                            IHomeid = home.FindElements(By.TagName("td"));
                            if (IHomeid.Count != 0 && home.Text.Contains("H/S%")                                                                                                                        )
                            {
                                for(int i=0;i< IHomeid.Count;i++)
                                {
                                    string check = IHomeid[i].Text;
                                    try
                                    {
                                        if (check == "H/S%")
                                        {
                                            i++;
                                            HomesteadPercentage = IHomeid[i].Text;
                                            break;
                                        }
                                    }
                                    catch { }
                                }
                            }
                        }
                        string Duedate1 = driver.FindElement(By.XPath("//*[@id='divcenter']/center[1]/b")).Text;
                        if (Duedate1.Contains("PENALTY DATE"))
                        {
                            duedata = gc.Between(Duedate1, "DUE BY", "PENALTY DATE");
                        }
                        else
                        {
                            duedata = GlobalClass.After(Duedate1, "DUE BY");
                        }
                        string TaxResult = address3 + "~" + Owner + "~" + Bill + "~" + TaxYear + "~" + Taxamount + "~" + duedata + "~" + credit + "~" + HomesteadExemption + "~" + Penalty + "~" + Interest + "~" + Costs + "~" + Fees + "~" + Total + "~" + Paid + "~" + Refund + "~" + HomesteadPercentage;
                        gc.insert_date(orderNumber, Parcel_number, 1725, TaxResult, 1, DateTime.Now);
                        if (!Penalty.Contains("0.00"))
                        {
                            Delinquent_Tax = "" + "~" + "Please contact county for tax information";
                            gc.insert_date(orderNumber, Parcel_number, 1726, TaxSale, 1, DateTime.Now);
                        }
                        Thread.Sleep(2000);
                        IWebElement LinkDownload = driver.FindElement(By.LinkText("View Current Tax Bill"));
                        // gc.downloadfile(Linkhref, orderNumber, Parcel_number.Replace("-", ""), "ViewTaxBill" + TaxYear.Trim(), "SC", "Lexington");
                        // gc.CreatePdf(orderNumber, parcelNumber, "ViewTaxBill" + TaxYear, driver, "SC", "Lexington");
                        //gc.downloadfile(Linkhref, orderNumber, Parcel_number.Replace("-",""), "ViewTaxBill"+ TaxYear.Trim(), "SC", "Lexington");
                        string Linkhref = LinkDownload.GetAttribute("href");
                        PdfDownloadlink.Add(Linkhref);
                        //driver.Navigate().GoToUrl(Linkhref);
                        //Thread.Sleep(8000);
                        //gc.CreatePdf(orderNumber, parcelNumber, "ViewTaxBill1" + TaxYear, driver, "SC", "Lexington");
                    }
                    foreach (string Pdfdownload in PdfDownloadlink)
                    {
                        string fileName = "";
                        var chromeOptions = new ChromeOptions();
                        var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];
                        chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                        chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                        chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                        chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                        Array.ForEach(Directory.GetFiles(@downloadDirectory), File.Delete);
                        var chDriver = new ChromeDriver(chromeOptions);
                        try
                        {
                            chDriver.Navigate().GoToUrl(Pdfdownload);
                            Thread.Sleep(2000);
                            fileName = latestfilename();
                            //gc.AutoDownloadFile(orderNumber, Parcel_number, "Washington", "MN", "TaxBill" + m + ".pdf");
                            gc.AutoDownloadFile(orderNumber, Parcel_number, "Lexington", "SC", fileName);
                            chDriver.Quit();
                            // m++;
                        }
                        catch (Exception e)
                        {
                            chDriver.Quit();
                        }
                    }

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "SC", "Lexington", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    driver.Quit();
                    gc.mergpdf(orderNumber, "SC", "Lexington");
                    return "Data Inserted Successfully";
                }
                catch (Exception ex)
                {
                    driver.Quit();
                    throw ex;
                }
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
    }
}