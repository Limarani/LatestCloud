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
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace ScrapMaricopa.Scrapsource
{
    public class Webdriver_BoulderCO
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();
        public string FTP_BoulderCO(string streetno, string direction, string streetname, string city, string streettype, string unitnumber, string ownernm, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string Parcel_number = "", Tax_Authority = "", As_of = "", Total_Due = "", MillLevy = "", Class = "", Built = "";
            //request.UseDefaultCredentials = true;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //  driver = new PhantomJSDriver()
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    try
                    {
                        StartTime = DateTime.Now.ToString("HH:mm:ss");
                        driver.Navigate().GoToUrl("https://www.bouldercounty.org/departments/assessor/");
                        //driver.SwitchTo().Window(driver.WindowHandles.Last());
                        IWebElement Taxauthority1 = driver.FindElement(By.XPath("//*[@id='ctl00_PlaceHolderMain_ContactBottom__ControlWrapper_RichHtmlField']"));
                        Tax_Authority = GlobalClass.After(Taxauthority1.Text, "Mailing Address");
                        // driver.Close();
                        Thread.Sleep(2000);
                    }
                    catch { }
                    driver.Navigate().GoToUrl("http://maps.boco.solutions/propertysearch/");
                    Thread.Sleep(2000);
                    // IWebElement switchwebsite = driver.FindElement(By.XPath("//*[@id='platsDiv']/div[4]/div[8]/iframe"));
                    //driver.SwitchTo().Frame(switchwebsite);
                    if (searchType == "titleflex")
                    {
                        string address = streetno + " " + direction + " " + streetname + " " + streettype;
                        gc.TitleFlexSearch(orderNumber, "", ownernm, address.Trim(), "CO", "Boulder");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_BoulderCO"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        string address = "";
                        if (direction != "")
                        {
                            address = streetno + " " + direction + " " + streetname + " " + streettype.Trim();
                        }
                        else
                        {
                            address = streetno + " " + streetname + " " + streettype.Trim();
                        }
                        driver.FindElement(By.Id("searchField")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "Address Start", driver, "CO", "Boulder");
                        Thread.Sleep(4000);
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='choices']/div/b/span")).Click();
                            Thread.Sleep(7000);
                        }
                        catch { }
                        try
                        {
                            driver.FindElement(By.XPath("/html/body/div[5]/div[1]/div/a")).Click();
                        }
                        catch { }
                        try
                        {
                            //int StateYeardropdown = driver.FindElement(By.Id("choices")).FindElements(By.ClassName("ng-scope")).Count;
                            IWebElement addressmulti = driver.FindElement(By.XPath("//*[@id='search-grid']/div[2]/table/tbody"));
                            IList<IWebElement> Addressrow = addressmulti.FindElements(By.TagName("tr"));
                            IList<IWebElement> Addressid;
                            foreach (IWebElement Addressmultiple in Addressrow)
                            {
                                if (Addressrow.Count != 0)
                                {
                                    Addressid = Addressmultiple.FindElements(By.TagName("td"));
                                    if (Addressid.Count != 0)
                                    {
                                        string Accountnumber = Addressid[2].Text;
                                        string Addressresult = Addressid[3].Text + "~" + Addressid[4].Text;
                                        gc.insert_date(orderNumber, Accountnumber, 835, Addressresult, 1, DateTime.Now);
                                    }
                                }
                            }
                            if (Addressrow.Count < 26 && Addressrow.Count != 0)
                            {
                                HttpContext.Current.Session["multiparcel_Boulder"] = "Yes";
                                gc.CreatePdf_WOP(orderNumber, "MultyAddressSearch", driver, "CO", "Boulder");
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Addressrow.Count > 25 && Addressrow.Count != 0)
                            {
                                HttpContext.Current.Session["multiParcel_Boulder_Multicount"] = "Maximum";
                                gc.CreatePdf_WOP(orderNumber, "MultyAddressSearch", driver, "CO", "Boulder");
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }

                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.Id("searchField")).SendKeys(parcelNumber);
                        Thread.Sleep(8000);
                        gc.CreatePdf_WOP(orderNumber, "parcel", driver, "CO", "Boulder");
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='choices']/div/b/span")).Click();
                            Thread.Sleep(7000);
                        }
                        catch { }
                        try
                        {
                            driver.FindElement(By.XPath("/html/body/div[5]/div[1]/div/a")).Click();
                        }
                        catch { }
                        try
                        {
                            driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div/a/span")).Click();
                        }
                        catch { }
                    }
                    if (searchType == "unitnumber")
                    {
                        driver.FindElement(By.Id("searchField")).SendKeys(unitnumber);
                        Thread.Sleep(9000);
                        gc.CreatePdf_WOP(orderNumber, "parcel", driver, "CO", "Boulder");
                        driver.FindElement(By.Id("searchField")).SendKeys(Keys.Space);
                        //driver.FindElement(By.XPath("//*[@id='choices']/div/b/span")).Click();
                        Thread.Sleep(7000);
                        try
                        {
                            driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div")).Click();
                        }
                        catch { }
                    }

                    try
                    {
                        IWebElement Inodata = driver.FindElement(By.Id("autocomplete"));
                        if(Inodata.Text.Contains("Invalid entry"))
                        {
                            HttpContext.Current.Session["Nodata_BoulderCO"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    driver.FindElement(By.XPath("//*[@id='propertyInfo']/span")).Click();
                    Thread.Sleep(7000);
                    gc.CreatePdf_WOP(orderNumber, "Property Search", driver, "CO", "Boulder");

                    IWebElement propertytable = driver.FindElement(By.XPath("//*[@id='propertyInfo']/div/property/div[1]/div[3]"));
                    string PropertyAddress = gc.Between(propertytable.Text, "Property Address:", "Location:").Trim();
                    string City = gc.Between(propertytable.Text, "City:", "Zip:").Trim();
                    string Owner = gc.Between(propertytable.Text, "Owner:", "Mailing Address:").Trim();
                    Parcel_number = gc.Between(propertytable.Text, "Parcel Number:", "Property Address:").Trim();
                    string AccountNumber = gc.Between(propertytable.Text, "Account Number:", "Owner:").Trim();
                    string MailingAddress = gc.Between(propertytable.Text, "Mailing Address:", "City:").Trim();
                    string Zip = gc.Between(propertytable.Text, "Zip:", "Sec-Town-Range:").Trim();
                    string SecTown_Range = gc.Between(propertytable.Text, "Sec-Town-Range:", "Subdivision:").Trim();
                    string Subdivision = gc.Between(propertytable.Text, "Subdivision:", "Market Area:").Trim();
                    string Jurisdiction = gc.Between(propertytable.Text, "Jurisdiction:", "Legal Description:").Trim();
                    string LegalDescription = gc.Between(propertytable.Text, "Legal Description:", "Est. Parcel Area:").Trim();
                    string Acres = GlobalClass.After(propertytable.Text, "Acres:").Trim();
                    driver.FindElement(By.XPath("//*[@id='dataDisplay']/ul/li[2]/span")).Click();
                    Thread.Sleep(3000);
                    gc.CreatePdf_WOP(orderNumber, "Property Search2", driver, "CO", "Boulder");
                    IWebElement Propertytable2 = driver.FindElement(By.Id("assessmentDiv"));
                    string TaxArea = gc.Between(Propertytable2.Text, "Tax Area:", "No. of Improvements:");
                    string SiteAddress = gc.Between(Propertytable2.Text, "Site Address:", "Neighborhood:").Trim();
                    string Neighborhood = gc.Between(Propertytable2.Text, "Neighborhood:", "Total Account Value").Trim();
                    MillLevy = gc.Between(Propertytable2.Text, "MillLevy:", "See breakdown").Trim();
                    try
                    {
                        Class = gc.Between(Propertytable2.Text, "Class:", "Built:").Trim();
                    }
                    catch { }
                    try
                    {
                        Built = gc.Between(Propertytable2.Text, "Built:", "Design:").Trim();
                    }
                    catch { }


                    string Propertyresult = PropertyAddress + "~" + City + "~" + Owner + "~" + AccountNumber + "~" + MailingAddress + "~" + Zip + "~" + SecTown_Range + "~" + Subdivision + "~" + Jurisdiction + "~" + LegalDescription + "~" + Acres + "~" + TaxArea + "~" + SiteAddress + "~" + Neighborhood + "~" + MillLevy + "~" + Class + "~" + Built;
                    gc.insert_date(orderNumber, Parcel_number, 775, Propertyresult, 1, DateTime.Now);
                    gc.CreatePdf(orderNumber, Parcel_number, "Property Search Result", driver, "CO", "Boulder");
                    //assessment
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    IWebElement Assessmentable = driver.FindElement(By.XPath("//*[@id='assessmentDiv']/table[1]/tbody"));
                    IList<IWebElement> Assessmentrow = Assessmentable.FindElements(By.TagName("tr"));
                    IList<IWebElement> Assessmentid;
                    foreach (IWebElement Assessment in Assessmentrow)
                    {
                        Assessmentid = Assessment.FindElements(By.TagName("td"));
                        if (Assessmentid.Count != 0 && !Assessment.Text.Contains("MillLevy:"))
                        {
                            string Assessmentresult = Assessmentid[0].Text + "~" + Assessmentid[1].Text + "~" + Assessmentid[2].Text;
                            gc.insert_date(orderNumber, Parcel_number, 776, Assessmentresult, 1, DateTime.Now);
                        }
                    }
                    string current1 = driver.CurrentWindowHandle;
                    driver.FindElement(By.XPath("//*[@id='dataDisplay']/div[1]/button")).Click();
                    Thread.Sleep(2000);
                    driver.SwitchTo().Window(driver.WindowHandles.Last());
                    gc.CreatePdf(orderNumber, Parcel_number, "Property giued", driver, "CO", "Boulder");
                    driver.Close();
                    driver.SwitchTo().Window(current1);
                    string check = "", AreaID = "", Title1 = "", ValueAmount = "", paymenttype = "", ValueTitle = "", Title2 = "", Taxes = "", TActual = "", TAssessed = "";
                    try
                    {
                        driver.FindElement(By.LinkText("Property Taxes")).Click();
                        Thread.Sleep(7000);
                        driver.SwitchTo().Window(driver.WindowHandles.Last());
                        gc.CreatePdf(orderNumber, Parcel_number, "Tax Click before", driver, "CO", "Boulder");
                        driver.FindElement(By.XPath("//*[@id='middle_left']/form/input[1]")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, Parcel_number, "Tax Click After", driver, "CO", "Boulder");
                    }
                    catch
                    { }
                    //try
                    //{
                    //    driver.Navigate().GoToUrl("https://treasurer.bouldercounty.org/treasurer/treasurerweb/account.jsp?account=" + Parcel_number + "");
                    //}
                    //catch { }


                    try
                    {
                        IWebElement currenttaxtable = driver.FindElement(By.Id("taxAccountSummary"));

                        IWebElement IValue = driver.FindElement(By.XPath("//*[@id='taxAccountValueSummary']/div/table/tbody"));
                        IList<IWebElement> IValueRow = IValue.FindElements(By.TagName("tr"));
                        IList<IWebElement> IValueTD;
                        foreach (IWebElement value in IValueRow)
                        {
                            IValueTD = value.FindElements(By.TagName("td"));
                            if (IValueTD.Count != 0 && value.Text != "")
                            {
                                if (IValueTD[1].Text.Trim() == "" && IValueTD[0].Text.Trim() != "" && IValueTD[2].Text.Trim() != "" && !IValueTD[0].Text.Contains("Area Id"))
                                {
                                    if (check != "" && check == "Area ID")
                                    {
                                        check = "";
                                        ValueAmount += IValueTD[0].Text + "~" + IValueTD[2].Text + "~";
                                    }
                                    else
                                    {
                                        ValueTitle += IValueTD[0].Text + "~";
                                        ValueAmount += IValueTD[2].Text + "~";
                                    }
                                }
                                if (IValueTD[1].Text.Trim() == "" && IValueTD[0].Text.Trim() != "" && IValueTD[2].Text.Trim() != "" && IValueTD[0].Text.Contains("Area Id"))
                                {
                                    ValueTitle += IValueTD[0].Text + "~" + IValueTD[2].Text + "~";
                                    check = "Area ID";
                                }
                                if (IValueTD[0].Text.Trim() == "" && IValueTD[1].Text.Trim() != "" && IValueTD[2].Text.Trim() != "")
                                {
                                    Title1 = IValueTD[1].Text;
                                    Title2 = IValueTD[2].Text;
                                }
                                if (IValueTD[0].Text.Trim() != "" && IValueTD[1].Text.Trim() != "" && IValueTD[2].Text.Trim() != "")
                                {
                                    ValueTitle += IValueTD[0].Text + "(" + Title1 + ")" + "~" + IValueTD[0].Text + "(" + Title2 + ")" + "~";
                                    ValueAmount += IValueTD[1].Text + "~" + IValueTD[2].Text + "~";
                                }
                            }
                        }

                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + ValueTitle.Remove(ValueTitle.Length - 1, 1) + "' where Id = '" + 778 + "'");
                        gc.insert_date(orderNumber, Parcel_number, 778, ValueAmount.Remove(ValueAmount.Length - 1, 1), 1, DateTime.Now);
                    }
                    catch
                    { }

                    try
                    {
                        for (int i = 1; i < 3; i++)

                        {
                            IWebElement Inquirytable = driver.FindElement(By.Id("totals"));
                            IWebElement As_off = driver.FindElement(By.Id("paymentDate"));
                            As_of = As_off.GetAttribute("value");
                            if (i == 1)
                            {
                                paymenttype = driver.FindElement(By.XPath("//*[@id='inquiryForm']/table/tbody/tr[2]/td[2]/label[1]")).Text;
                                driver.FindElement(By.Id("paymentTypeFirst")).Click();
                            }
                            else
                            {
                                paymenttype = driver.FindElement(By.XPath("//*[@id='inquiryForm']/table/tbody/tr[2]/td[2]/label[2]")).Text;
                                if (paymenttype == "Full")
                                {
                                    driver.FindElement(By.Id("paymentTypeFull")).Click();
                                }
                                if (paymenttype == "Second")
                                {
                                    driver.FindElement(By.Id("paymentTypeSecond")).Click();
                                }
                            }
                            Total_Due = GlobalClass.After(Inquirytable.Text, "Total Due").Trim();
                            string cuttenttaxresult1 = As_of + "~" + paymenttype + "~" + Total_Due + "~" + Tax_Authority;
                            gc.insert_date(orderNumber, Parcel_number, 782, cuttenttaxresult1, 1, DateTime.Now);
                        }
                    }
                    catch { }
                    try
                    {
                        driver.FindElement(By.LinkText("Mill Levy Breakdown")).Click();
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, Parcel_number, "Tax Billed", driver, "CO", "Boulder");
                        string TaxBillingrate = driver.FindElement(By.XPath("//*[@id='middle']/h2")).Text;
                        TaxTime = DateTime.Now.ToString("HH:mm:ss");
                        IWebElement Taxaccounttable = driver.FindElement(By.XPath("//*[@id='middle']/table[3]/tbody"));
                        IList<IWebElement> Taxaccountrow = Taxaccounttable.FindElements(By.TagName("tr"));
                        IList<IWebElement> Taxaccountid;
                        foreach (IWebElement Taxaccount in Taxaccountrow)
                        {
                            Taxaccountid = Taxaccount.FindElements(By.TagName("td"));
                            if (Taxaccountid.Count != 0 && !Taxaccount.Text.Contains("* Credit Levy"))
                            {
                                string Taxaccountresult = Taxaccountid[0].Text + "~" + Taxaccountid[1].Text + "~" + Taxaccountid[2].Text + "~" + Taxaccountid[3].Text + "~" + TaxBillingrate;
                                gc.insert_date(orderNumber, Parcel_number, 783, Taxaccountresult, 1, DateTime.Now);
                            }

                        }
                        IWebElement propertycodetable = driver.FindElement(By.XPath("//*[@id='middle']/table[4]/tbody"));
                        IList<IWebElement> propertycoderow = propertycodetable.FindElements(By.TagName("tr"));
                        IList<IWebElement> propertycodeid;
                        foreach (IWebElement propertycode in propertycoderow)
                        {
                            propertycodeid = propertycode.FindElements(By.TagName("td"));
                            if (propertycodeid.Count != 0 && !propertycode.Text.Contains("Property Code"))
                            {
                                string propertycoderesult = propertycodeid[0].Text + "~" + propertycodeid[1].Text + "~" + propertycodeid[2].Text + "~" + propertycodeid[3].Text;
                                gc.insert_date(orderNumber, Parcel_number, 857, propertycoderesult, 1, DateTime.Now);
                            }

                        }
                    }
                    catch
                    { }
                    driver.FindElement(By.LinkText("Transaction Detail")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Tax Account Summary", driver, "CO", "Boulder");
                    IWebElement TaxSummaryTable = driver.FindElement(By.XPath("//*[@id='middle']/table[1]/tbody"));
                    IList<IWebElement> Taxsummaryrow = TaxSummaryTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> TaxSummaryid;
                    foreach (IWebElement Taxsummary in Taxsummaryrow)
                    {
                        TaxSummaryid = Taxsummary.FindElements(By.TagName("td"));
                        if (TaxSummaryid.Count != 0)
                        {
                            string TaxSummarydetail = TaxSummaryid[0].Text + "~" + TaxSummaryid[1].Text + "~" + TaxSummaryid[2].Text + "~" + TaxSummaryid[3].Text + "~" + TaxSummaryid[4].Text + "~" + TaxSummaryid[5].Text + "~" + TaxSummaryid[6].Text + "~" + TaxSummaryid[7].Text;
                            gc.insert_date(orderNumber, Parcel_number, 786, TaxSummarydetail, 1, DateTime.Now);
                        }
                    }
                    IWebElement TransactionDetailstable = driver.FindElement(By.XPath("//*[@id='middle']/table[2]/tbody"));
                    IList<IWebElement> Transactiondetailrow = TransactionDetailstable.FindElements(By.TagName("tr"));
                    IList<IWebElement> Transactiondetailid;
                    foreach (IWebElement Transaction in Transactiondetailrow)
                    {
                        Transactiondetailid = Transaction.FindElements(By.TagName("td"));
                        if (Transactiondetailid.Count != 1)
                        {
                            string Transactionresult = Transactiondetailid[0].Text + "~" + Transactiondetailid[1].Text + "~" + Transactiondetailid[2].Text + "~" + Transactiondetailid[3].Text + "~" + Transactiondetailid[4].Text;
                            gc.insert_date(orderNumber, Parcel_number, 787, Transactionresult, 1, DateTime.Now);
                        }
                    }

                    try
                    {

                        //Account Balance pdf
                        string geturl = driver.Url;
                        try
                        {
                            var chromeOptions = new ChromeOptions();
                            var chDriver = new ChromeDriver(chromeOptions);
                            IJavaScriptExecutor js = (IJavaScriptExecutor)chDriver;
                            chDriver.Navigate().GoToUrl(geturl);
                            chDriver.FindElement(By.XPath("//*[@id='middle_left']/form/input[1]")).Click();
                            chDriver.FindElement(By.LinkText("Account Balance")).Click();
                            Thread.Sleep(2000);
                            chDriver.FindElement(By.LinkText("Account Balance")).Click();
                            Thread.Sleep(8000);
                            gc.CreatePdf(orderNumber, Parcel_number, "Account Balance", chDriver, "CO", "Boulder");

                            //Statement Of Taxes Due
                            chDriver.Navigate().GoToUrl(geturl);
                            Thread.Sleep(2000);
                            chDriver.FindElement(By.LinkText("Statement Of Taxes Due")).Click();
                            Thread.Sleep(2000);
                            chDriver.FindElement(By.LinkText("Statement of Taxes Due")).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, Parcel_number, "Statement of Taxes Due", chDriver, "CO", "Boulder");
                            //Summary of Taxes Due
                            chDriver.Navigate().GoToUrl(geturl);
                            Thread.Sleep(2000);
                            chDriver.FindElement(By.LinkText("Summary of Taxes Due")).Click();
                            Thread.Sleep(2000);
                            chDriver.FindElement(By.LinkText("Summary of Taxes Due")).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, Parcel_number, "Summary of Taxes Due", chDriver, "CO", "Boulder");
                            chDriver.Quit();
                        }
                        catch
                        {

                        }
                    }
                    catch
                    {

                    }
                    string tableassess = "";
                    try
                    {
                        var chromeOptions = new ChromeOptions();
                        var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];
                        chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                        chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                        chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                        chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                        var driver1 = new ChromeDriver(chromeOptions);
                        driver1.Navigate().GoToUrl(driver.Url);
                        string fileName = "";
                        driver1.FindElement(By.XPath("//*[@id='middle_left']/form/input[1]")).Click();
                        for (int j = 1; j < 4; j++)
                        {
                            IWebElement Receipttable = driver1.FindElement(By.XPath("//*[@id='receiptHistory']/a[" + j + "]"));
                            string BillTax2 = Receipttable.GetAttribute("href");
                            fileName = gc.Between(BillTax2, "taxreceipt/", "?id=").Replace("-", "_");
                            Receipttable.Click();
                            Thread.Sleep(3000);
                            gc.AutoDownloadFile(orderNumber, parcelNumber, "Boulder", "CO", fileName);
                            string FilePath = gc.filePath(orderNumber, parcelNumber) + fileName;
                            PdfReader reader;
                            string pdfData;
                            reader = new PdfReader(FilePath);
                            String textFromPage = PdfTextExtractor.GetTextFromPage(reader, 1);
                            System.Diagnostics.Debug.WriteLine("" + textFromPage);
                            string pdftext = textFromPage;
                            try
                            {
                                tableassess = gc.Between(pdftext, "Receipt Number", "Situs Address").Trim();
                                string[] tableArray = tableassess.Split('\n');
                                for (int i = 0; i < 1; i++)
                                {
                                    int count1 = tableArray.Length;
                                    string a1 = tableArray[i].Replace(" ", "~");
                                    string[] rowarray = a1.Split('~');
                                    int tdcount = rowarray.Length;
                                    if (tdcount < 7)
                                    {
                                        string datepdf = rowarray[0] + "~" + rowarray[2] + "" + rowarray[3] + " " + rowarray[4] + "~" + " " + "~" + rowarray[5];
                                        gc.insert_date(orderNumber, Parcel_number, 889, datepdf, 1, DateTime.Now);
                                    }
                                    if (tdcount > 6)
                                    {
                                        string datepdf = rowarray[0] + "~" + rowarray[2] + "" + rowarray[3] + " " + rowarray[4] + "~" + rowarray[5] + " " + rowarray[6] + " " + rowarray[7] + "~" + rowarray[8];
                                        gc.insert_date(orderNumber, Parcel_number, 889, datepdf, 1, DateTime.Now);
                                    }

                                }
                            }
                            catch { }
                            try
                            {
                                tableassess = gc.Between(pdftext, "Account:", "TransNo:").Trim();
                                string[] tableArray = tableassess.Split('\n');
                                for (int i = 0; i < 2; i++)
                                {
                                    int count1 = tableArray.Length;
                                    string a1 = tableArray[i].Replace(" ", "~");
                                    string[] rowarray = a1.Split('~');
                                    int tdcount = rowarray.Length;
                                    if (tdcount < 7)
                                    {
                                        string datepdf = rowarray[0] + "~" + rowarray[2] + "" + rowarray[3] + " " + rowarray[4] + "~" + " " + "~" + rowarray[5];
                                        gc.insert_date(orderNumber, Parcel_number, 889, datepdf, 1, DateTime.Now);
                                    }
                                    if (tdcount > 6)
                                    {
                                        string datepdf = rowarray[0] + "~" + rowarray[2] + "" + rowarray[3] + " " + rowarray[4] + "~" + rowarray[5] + " " + rowarray[6] + " " + rowarray[7] + "~" + rowarray[8];
                                        gc.insert_date(orderNumber, Parcel_number, 889, datepdf, 1, DateTime.Now);
                                    }

                                }
                            }
                            catch { }

                        }

                        driver1.Quit();
                    }
                    catch (Exception ex) { driver.Quit(); }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "CO", "Boulder", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    driver.Quit();

                    gc.mergpdf(orderNumber, "CO", "Boulder");
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