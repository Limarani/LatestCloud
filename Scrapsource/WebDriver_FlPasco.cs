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
    public class WebDriver_FlPasco
    {
        string parcelno = "";
        string outputPath = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();

        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;

        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_FLPasco(string houseno, string direction, string sname, string sttype, string parcelNumber, string searchType, string orderNumber, string ownername, string directParcel)
        {
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "", AssessTakenTime = "", TaxTakentime = "", CityTaxtakentime = "";
            string TotaltakenTime = "";
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string Address = houseno + " " + sname;

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new ChromeDriver();
            driver = new PhantomJSDriver();
            driver.Manage().Window.Size = new Size(1920, 1080);
            string[] stringSeparators1 = new string[] { "\r\n" };
            List<string> listurl = new List<string>();
            string Date = "";
            Date = DateTime.Now.ToString("M/d/yyyy");
            string Alternatekey = "", PropertyAddress = "", MailingAddress = "", PropertyType = "", YearBuilt = "", LegalDescription = "", FullParcelID = "";
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("http://search.pascopa.com/");
                        Thread.Sleep(2000);

                        driver.FindElement(By.XPath("//*[@id='add']/table/tbody/tr[2]/td[2]/input")).SendKeys(houseno.Trim());
                        // driver.FindElement(By.XPath("//*[@id='inpDir']")).SendKeys(Dir.Trim());
                        driver.FindElement(By.XPath("//*[@id='add']/table/tbody/tr[2]/td[3]/input")).SendKeys(sname.Trim());
                        Thread.Sleep(1000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "FL", "Pasco");
                        driver.FindElement(By.XPath("//*[@id='adds']")).Click();
                        //   string urlSumbmit = "http://search.pascopa.com/default.aspx?pid=add&key=E%5BK&add1=" + houseno + "&add2=" + sname + "&add=Submit";
                        // driver.Navigate().GoToUrl(urlSumbmit);
                        Thread.Sleep(1000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "FL", "Pasco");
                        string MultiChk = "";
                        try
                        {
                            MultiChk = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_PanelResults']/table/tbody/tr[2]/td/table/tbody/tr/td[1]/div")).Text;
                            string MultiCount = gc.Between(MultiChk, "Displaying ", " record");
                            if (Convert.ToInt32(MultiCount) > 25)
                            {
                                HttpContext.Current.Session["multiparcel_Pasco_Count"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }

                        if ((MultiChk != "Displaying 1 record") && (MultiChk != ""))
                        {
                            try
                            {
                                //string Multi = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_PanelResults']/table/tbody/tr[3]/td/table")).Text;

                                IWebElement MultiTb = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_PanelResults']/table/tbody/tr[3]/td/table"));
                                IList<IWebElement> MultiTR = MultiTb.FindElements(By.TagName("tr"));
                                IList<IWebElement> MultiTD;

                                foreach (IWebElement row1 in MultiTR)
                                {

                                    MultiTD = row1.FindElements(By.TagName("td"));

                                    if (MultiTD.Count != 0 && MultiTD[1].Text != "Parcel")
                                    {
                                        IWebElement IparcelNumber = MultiTD[1].FindElement(By.TagName("a"));
                                        parcelNumber = IparcelNumber.Text;
                                        ownername = MultiTD[2].Text;
                                        PropertyAddress = MultiTD[3].Text;

                                        string Multiparcel = ownername + "~" + PropertyAddress;
                                        gc.insert_date(orderNumber, parcelNumber, 274, Multiparcel, 1, DateTime.Now);
                                        HttpContext.Current.Session["multiparcel_Pasco"] = "Yes";

                                    }
                                }
                            }
                            catch { }
                            return "MultiParcel";
                        }
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_PanelResults']/table/tbody/tr[3]/td/table/tbody/tr/td[2]/div/a")).SendKeys(Keys.Enter);
                        }
                        catch { }
                    }

                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, parcelNumber, ownername, Address, "FL", "Pasco");

                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        parcelNumber = parcelNumber.Replace(".", "");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_FLPasco"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        if (HttpContext.Current.Session["titleparcel"] != null)
                        {
                            driver.Navigate().GoToUrl("http://search.pascopa.com/");
                            Thread.Sleep(2000);
                            var Parcelsplit = parcelNumber.Replace(".", "").Split('-');
                            driver.FindElement(By.XPath("//*[@id='Form1']/table/tbody/tr[2]/td[2]/input")).SendKeys(Parcelsplit[2].Trim());

                            driver.FindElement(By.XPath("//*[@id='Form1']/table/tbody/tr[2]/td[3]/input")).SendKeys(Parcelsplit[1].Trim());
                            driver.FindElement(By.XPath("//*[@id='Form1']/table/tbody/tr[2]/td[4]/input")).SendKeys(Parcelsplit[0].Trim());

                            driver.FindElement(By.XPath("//*[@id='Form1']/table/tbody/tr[2]/td[5]/input")).SendKeys(Parcelsplit[3].Trim());
                            driver.FindElement(By.XPath("//*[@id='Form1']/table/tbody/tr[2]/td[6]/input")).SendKeys(Parcelsplit[4].Trim());
                            driver.FindElement(By.XPath("//*[@id='Form1']/table/tbody/tr[2]/td[7]/input")).SendKeys(Parcelsplit[5].Trim());
                            Thread.Sleep(1000);
                            gc.CreatePdf_WOP(orderNumber, "Parcel  Search", driver, "FL", "Pasco");
                            driver.FindElement(By.XPath("//*[@id='Form1']/table/tbody/tr[2]/td[8]/input[1]")).SendKeys(Keys.Enter);
                            Thread.Sleep(1000);
                        }
                    }

                    if (searchType == "parcel")
                    {
                        if (HttpContext.Current.Session["titleparcel"] != null)
                        {
                            driver.Navigate().GoToUrl("http://search.pascopa.com/");
                            Thread.Sleep(2000);
                            var Parcelsplit = parcelNumber.Replace(".", "").Split('-');
                            driver.FindElement(By.XPath("//*[@id='Form1']/table/tbody/tr[2]/td[2]/input")).SendKeys(Parcelsplit[2].Trim());

                            driver.FindElement(By.XPath("//*[@id='Form1']/table/tbody/tr[2]/td[3]/input")).SendKeys(Parcelsplit[1].Trim());
                            driver.FindElement(By.XPath("//*[@id='Form1']/table/tbody/tr[2]/td[4]/input")).SendKeys(Parcelsplit[0].Trim());

                            driver.FindElement(By.XPath("//*[@id='Form1']/table/tbody/tr[2]/td[5]/input")).SendKeys(Parcelsplit[3].Trim());
                            driver.FindElement(By.XPath("//*[@id='Form1']/table/tbody/tr[2]/td[6]/input")).SendKeys(Parcelsplit[4].Trim());
                            driver.FindElement(By.XPath("//*[@id='Form1']/table/tbody/tr[2]/td[7]/input")).SendKeys(Parcelsplit[5].Trim());
                            Thread.Sleep(1000);
                            gc.CreatePdf_WOP(orderNumber, "Parcel  Search", driver, "FL", "Pasco");
                            driver.FindElement(By.XPath("//*[@id='Form1']/table/tbody/tr[2]/td[8]/input[1]")).SendKeys(Keys.Enter);
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            driver.Navigate().GoToUrl("http://search.pascopa.com/");
                            Thread.Sleep(2000);
                            var Parsplit = parcelNumber.Replace(".", "").Split('-');
                            driver.FindElement(By.XPath("//*[@id='Form1']/table/tbody/tr[2]/td[2]/input")).SendKeys(Parsplit[0].Trim());

                            driver.FindElement(By.XPath("//*[@id='Form1']/table/tbody/tr[2]/td[3]/input")).SendKeys(Parsplit[1].Trim());
                            driver.FindElement(By.XPath("//*[@id='Form1']/table/tbody/tr[2]/td[4]/input")).SendKeys(Parsplit[2].Trim());

                            driver.FindElement(By.XPath("//*[@id='Form1']/table/tbody/tr[2]/td[5]/input")).SendKeys(Parsplit[3].Trim());
                            driver.FindElement(By.XPath("//*[@id='Form1']/table/tbody/tr[2]/td[6]/input")).SendKeys(Parsplit[4].Trim());
                            driver.FindElement(By.XPath("//*[@id='Form1']/table/tbody/tr[2]/td[7]/input")).SendKeys(Parsplit[5].Trim());
                            Thread.Sleep(1000);
                            gc.CreatePdf_WOP(orderNumber, "Parcel  Search", driver, "FL", "Pasco");
                            driver.FindElement(By.XPath("//*[@id='Form1']/table/tbody/tr[2]/td[8]/input[1]")).SendKeys(Keys.Enter);
                            Thread.Sleep(1000);
                        }
                        //gc.CreatePdf_WOP(orderNumber, "Parcel  Search Result", driver, "FL", "Pasco");
                    }

                    else if (searchType == "ownername")
                    {

                        driver.Navigate().GoToUrl("http://search.pascopa.com/");
                        Thread.Sleep(2000);

                        driver.FindElement(By.XPath("//*[@id='nam']/table/tbody/tr[2]/td[2]/input")).SendKeys(ownername.Trim());
                        // driver.FindElement(By.XPath("//*[@id='inpDir']")).SendKeys(Dir.Trim());

                        Thread.Sleep(1000);
                        gc.CreatePdf_WOP(orderNumber, "Ownername  Search", driver, "FL", "Pasco");
                        driver.FindElement(By.XPath("//*[@id='nam']/table/tbody/tr[2]/td[3]/input[1]")).SendKeys(Keys.Enter);
                        Thread.Sleep(1000);
                        gc.CreatePdf_WOP(orderNumber, "Ownername  Search Result", driver, "FL", "Pasco");
                        string MultiChk = "";

                        try
                        {
                            MultiChk = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_PanelResults']/table/tbody/tr[2]/td/table/tbody/tr/td[1]/div")).Text;
                            string MultiCount = gc.Between(MultiChk, "Displaying ", " record");
                            if (Convert.ToInt32(MultiCount) > 25)
                            {
                                HttpContext.Current.Session["multiparcel_Pasco_Count"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }

                        if ((MultiChk != "Displaying 1 record") && (MultiChk != ""))
                        {
                            try
                            {
                                //string Multi = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_PanelResults']/table/tbody/tr[3]/td/table")).Text;

                                IWebElement MultiTb = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_PanelResults']/table/tbody/tr[3]/td/table"));
                                IList<IWebElement> MultiTR = MultiTb.FindElements(By.TagName("tr"));
                                IList<IWebElement> MultiTD;
                                string City = "";
                                foreach (IWebElement row1 in MultiTR)
                                {

                                    MultiTD = row1.FindElements(By.TagName("td"));

                                    if (MultiTD.Count != 0 && MultiTD[1].Text != "Parcel")
                                    {
                                        IWebElement IparcelNumber = MultiTD[1].FindElement(By.TagName("a"));
                                        parcelNumber = IparcelNumber.Text;
                                        ownername = MultiTD[2].Text;
                                        PropertyAddress = MultiTD[3].Text;

                                        string Multiparcel = ownername + "~" + PropertyAddress;
                                        gc.insert_date(orderNumber, parcelNumber, 274, Multiparcel, 1, DateTime.Now);
                                        HttpContext.Current.Session["multiparcel_Pasco"] = "Yes";

                                    }
                                }
                            }
                            catch { }

                            return "MultiParcel";

                        }
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_PanelResults']/table/tbody/tr[3]/td/table/tbody/tr/td[2]/div/a")).SendKeys(Keys.Enter);
                        }
                        catch { }
                    }
                    try
                    {
                        IWebElement INodata = driver.FindElement(By.XPath("//*[@id='wrapper']/div"));
                        if (INodata.Text.Contains("NOT FOUND") || INodata.Text.Contains("No Parcels match your query"))
                        {
                            HttpContext.Current.Session["Nodata_FLPasco"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    // string MailingAddress = "", PropertyType = "", YearBuilt = "", LegalDescription = "";
                    Thread.Sleep(2000);
                    try
                    {
                        //gc.CreatePdf_WOP(orderNumber, "Parcel  Search 11", driver, "FL", "Pasco");       
                        parcelNumber = driver.FindElement(By.XPath("//*[@id='wrapper']/div/div[2]/div[1]/div[1]")).Text.Replace("(Card: 1 of 1)", "").Replace("\r\n", "").Replace("Parcel ID", "").Trim();
                        //var ParSplit = parcelNumber.Split(' ');
                        //parcelNumber = ParSplit[0];
                        PropertyType = driver.FindElement(By.XPath("//*[@id='wrapper']/div/div[2]/div[1]/div[2]/div[2]")).Text;
                        MailingAddress = driver.FindElement(By.XPath("//*[@id='parcelLocation']/div[2]")).Text;
                        PropertyAddress = driver.FindElement(By.XPath("//*[@id='parcelLocation']/div[4]")).Text.Replace("\r\n", " ");
                        LegalDescription = driver.FindElement(By.XPath("//*[@id='parcelLocation']/div[6]")).Text;
                        //try
                        //{
                        //    LegalDescription += driver.FindElement(By.XPath("//*[@id='parcelLocation']/div[7]")).Text;
                        //    LegalDescription += driver.FindElement(By.XPath("//*[@id='parcelLocation']/div[8]")).Text;
                        //    LegalDescription += driver.FindElement(By.XPath("//*[@id='parcelLocation']/div[9]")).Text;
                        //    LegalDescription += driver.FindElement(By.XPath("//*[@id='parcelLocation']/div[10]")).Text;
                        //}
                        //catch { }
                        YearBuilt = driver.FindElement(By.XPath("//*[@id='buildingDetailTable']/tbody/tr[1]/td[2]")).Text;
                    }
                    catch { }

                    gc.CreatePdf(orderNumber, parcelNumber, "Assessment Detail", driver, "FL", "Pasco");

                    string[] linesName = MailingAddress.Split(stringSeparators1, StringSplitOptions.None);

                    ownername = linesName[0];
                    MailingAddress = linesName[1] + linesName[2];

                    string PropertyDetail = ownername + "~" + PropertyAddress + "~" + MailingAddress + "~" + PropertyType + "~" + YearBuilt + "~" + LegalDescription;
                    gc.insert_date(orderNumber, parcelNumber, 275, PropertyDetail, 1, DateTime.Now);

                    string AssessedYear = "", AgriculturalLand = "", Taxable = "", LandValue = "", BuildingValue = "", ExtraFeaturesValue = "", JustValue = "", AssessedValue = "", Homestead = "", NonSchoolAdditionalHomestead = "", NonSchoolTaxableValue = "", SchoolDistrictTaxableValue = "";
                    try
                    {
                        AssessedYear = driver.FindElement(By.XPath("//*[@id='wrapper']/div/div[1]/div[2]/a")).Text;
                    }
                    catch { }
                    AgriculturalLand = driver.FindElement(By.XPath("//*[@id='parcelValueTable']/tbody/tr[1]/td[2]")).Text;
                    LandValue = driver.FindElement(By.XPath("//*[@id='parcelValueTable']/tbody/tr[2]/td[2]")).Text;
                    BuildingValue = driver.FindElement(By.XPath("//*[@id='parcelValueTable']/tbody/tr[3]/td[2]")).Text;
                    ExtraFeaturesValue = driver.FindElement(By.XPath("//*[@id='parcelValueTable']/tbody/tr[4]/td[2]")).Text;
                    JustValue = driver.FindElement(By.XPath("//*[@id='parcelValueTable']/tbody/tr[6]/td[2]/span")).Text;
                    AssessedValue = driver.FindElement(By.XPath("//*[@id='parcelValueTable']/tbody/tr[7]/td[2]")).Text;
                    try
                    { Taxable = driver.FindElement(By.XPath("//*[@id='parcelValueTable']/tbody/tr[10]/td[2]/b")).Text; }
                    catch { }
                    try
                    {

                        Homestead = driver.FindElement(By.XPath("//*[@id='parcelValueTable']/tbody/tr[8]/td[2]")).Text;
                        NonSchoolAdditionalHomestead = driver.FindElement(By.XPath("//*[@id='parcelValueTable']/tbody/tr[9]/td[2]")).Text;
                        NonSchoolTaxableValue = driver.FindElement(By.XPath("//*[@id='parcelValueTable']/tbody/tr[11]/td[2]/b")).Text;
                        SchoolDistrictTaxableValue = driver.FindElement(By.XPath("//*[@id='parcelValueTable']/tbody/tr[12]/td[2]/b")).Text;

                    }
                    catch { }


                    string AssessDetail = AssessedYear + "~" + AgriculturalLand + "~" + LandValue + "~" + BuildingValue + "~" + ExtraFeaturesValue + "~" + JustValue + "~" + AssessedValue + "~" + Homestead + "~" + NonSchoolAdditionalHomestead + "~" + NonSchoolTaxableValue + "~" + SchoolDistrictTaxableValue + "~" + Taxable;
                    gc.insert_date(orderNumber, parcelNumber, 276, AssessDetail, 1, DateTime.Now);

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("https://pasco.county-taxes.com/public");
                    Thread.Sleep(2000);
                    driver.FindElement(By.Id("search-text form-control")).SendKeys(parcelNumber.Trim());
                    Thread.Sleep(1000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Search", driver, "FL", "Pasco");
                    driver.FindElement(By.XPath("//*[@id='search-controls']/div/form[1]/div[1]/div/span/button")).SendKeys(Keys.Enter);
                    Thread.Sleep(2000);
                    try { driver.FindElement(By.XPath("//*[@id='results']/div[2]/div/div[4]/div/ul/li[1]/a")).Click(); }
                    catch { }
                    try { driver.FindElement(By.XPath("//*[@id='results']/div[2]/div/div[6]/div/ul/li[1]/a")).Click(); }
                    catch { }
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Search Result", driver, "FL", "Pasco");
                    string Taxparcel = "", AccountNumber = "", MillageCode = "", Millagerate = "", TaxYear = "", TaxAmount = "", PaidAmount = "", PaidDate = "", EffectiveDate = "", ReceiptNumber = "";
                    string billyear = "", inst = "", paidamount = "", receipt = "", effectivedate = "", strBalance = "", strBillDate = "", strBillPaid = "", strbillyear = "", strBill = "";
                    string accno = "", alterkey = "", taxowner = "", tax_addr = "", millagecode = "", ctax_year = "", cctaxyear = "", combinedtaxamount = "", grosstax = "", cpaidamount = "", cpaiddate = "", ceffdate = "", creceipt = "", milagerate = "", ifpaidby = "", pleasepay = "";
                    int i = 0;
                    string[] stringSeparators = new string[] { "\r\n" };
                    List<string> strissuecertificate = new List<string>();
                    List<string> downloadlink = new List<string>();
                    List<string> taxhistorylink = new List<string>();
                    List<string> taxhistorylinkinst = new List<string>();
                    //Annual and Installment Bill 
                    IWebElement ITaxSearch = driver.FindElement(By.LinkText("Full bill history"));
                    string strITaxSearch = ITaxSearch.GetAttribute("href");
                    driver.Navigate().GoToUrl(strITaxSearch);
                    Thread.Sleep(3000);
                    Taxparcel = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[1]")).Text;
                    Taxparcel = GlobalClass.After(Taxparcel, "Real Estate Parcel/Account #");
                    gc.CreatePdf(orderNumber, parcelNumber, "Full Bill History", driver, "FL", "Pasco");
                    IWebElement IBillHistorytable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table"));
                    IList<IWebElement> IBillHistoryRow = IBillHistorytable.FindElements(By.TagName("tr"));
                    IList<IWebElement> IBillHistoryTD;
                    IList<IWebElement> IBillHistoryTH;
                    int i1 = 0; int m = 0; int j = 0; int k = 0;
                    foreach (IWebElement bill in IBillHistoryRow)
                    {
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
                                        gc.insert_date(orderNumber, Taxparcel, 279, strTaxHistory2, 1, DateTime.Now);
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
                                    gc.insert_date(orderNumber, Taxparcel, 279, strTaxHistory2, 1, DateTime.Now);
                                }
                                if (IBillHistoryTD[2].Text.Contains("Paid"))
                                {
                                    inst = "Total"; paidamount = IBillHistoryTD[2].Text.Replace("Paid", ""); strBalance = "";
                                    string strTaxHistory = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                    gc.insert_date(orderNumber, Taxparcel, 279, strTaxHistory, 1, DateTime.Now);
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

                                                    BillDownload(orderNumber, parcelNumber, BillTax, Taxparcel, strbillyear);
                                                }
                                                catch { }
                                                try
                                                {
                                                    IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                                    string BillTax = ITaxBill.GetAttribute("href");
                                                    downloadlink.Add(BillTax);
                                                    //string dbill = gc.Between(BillTax, "bills/", "/print");
                                                    strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                                    string fname = "Tax Bill" + " " + Taxparcel + " " + strBill;
                                                    //gc.downloadfile(BillTax, orderNumber, taxparcel, fname, "FL", "Osceola");

                                                    BillDownload(orderNumber, parcelNumber, BillTax, Taxparcel, strbillyear);
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
                                                    //gc.downloadfile(BillTax, orderNumber, taxparcel, "Osceola-County-Real-Estate-" + taxparcel + "-" + strBill + "-bill", "FL", "Osceola");

                                                    BillDownload(orderNumber, parcelNumber, BillTax, Taxparcel, strbillyear);

                                                }
                                                catch { }
                                                try
                                                {
                                                    IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                                    string BillTax = ITaxBill.GetAttribute("href");
                                                    downloadlink.Add(BillTax);
                                                    //string dbill = gc.Between(BillTax, "bills/", "/print");
                                                    strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                                    string fname = "Tax Bill" + " " + Taxparcel + " " + strBill;
                                                    //gc.downloadfile(BillTax, orderNumber, taxparcel, fname, "FL", "Osceola");

                                                    // View Bill 

                                                    BillDownload(orderNumber, parcelNumber, BillTax, Taxparcel, strbillyear);

                                                }
                                                catch (Exception ex)
                                                {
                                                }
                                                taxhistorylink.Add(taxlink);
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
                                                        // gc.downloadfile(BillTax, orderNumber, taxparcel, "Osceola-County-Real-Estate-" + taxparcel + "-" + strBill + "-bill", "FL", "Osceola");
                                                        // View Bill

                                                        BillDownload(orderNumber, parcelNumber, BillTax, Taxparcel, strbillyear);


                                                    }
                                                    catch { }
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                                        strbillyear = GlobalClass.Before(ITaxBillCount.Text, " Installment Bill");
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        downloadlink.Add(BillTax);
                                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                                        // gc.downloadfile(BillTax, orderNumber, taxparcel, "Osceola-County-Real-Estate-" + taxparcel + " - " + strBill + " -bill", "FL", "Osceola");
                                                        // View Bill 

                                                        BillDownload(orderNumber, parcelNumber, BillTax, Taxparcel, strbillyear);


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
                                                        //gc.downloadfile(BillTax, orderNumber, taxparcel, "Osceola-County-Real-Estate-" + taxparcel + "-" + billyear + "-Annual-bill", "FL", "Osceola");

                                                        BillDownload(orderNumber, parcelNumber, BillTax, Taxparcel, strbillyear);


                                                    }
                                                    catch { }
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                                        strbillyear = GlobalClass.Before(ITaxBillCount.Text, " Installment Bill");
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        downloadlink.Add(BillTax);
                                                        //gc.downloadfile(BillTax, orderNumber, taxparcel, "Osceola-County-Real-Estate-" + taxparcel + " - " + billyear + " -Annual-bill", "FL", "Osceola");

                                                        BillDownload(orderNumber, parcelNumber, BillTax, Taxparcel, strbillyear);


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
                                                        //gc.downloadfile(BillTax, orderNumber, taxparcel, "Osceola-County-Real-Estate-" + taxparcel + "-" + billyear + "-Annual-bill", "FL", "Osceola");
                                                        // View Bill 
                                                        BillDownload(orderNumber, parcelNumber, BillTax, Taxparcel, strbillyear);

                                                    }
                                                    catch { }
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                                        strbillyear = GlobalClass.Before(ITaxBillCount.Text, " Installment Bill");
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        downloadlink.Add(BillTax);
                                                        //gc.downloadfile(BillTax, orderNumber, taxparcel, "Osceola-County-Real-Estate-" + taxparcel + " - " + billyear + " -Annual-bill", "FL", "Osceola");
                                                        // View Bill 
                                                        BillDownload(orderNumber, parcelNumber, BillTax, Taxparcel, strbillyear);

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
                                            //gc.downloadfile(BillTax, orderNumber, taxparcel, "Osceola-County-Real-Estate-" + taxparcel + "-" + billyear + "-Annual-bill", "FL", "Osceola");
                                            BillDownload(orderNumber, parcelNumber, BillTax, Taxparcel, strbillyear);

                                        }
                                        catch { }
                                        try
                                        {
                                            IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                            string BillTax = ITaxBill.GetAttribute("href");
                                            downloadlink.Add(BillTax);
                                            //    gc.downloadfile(BillTax, orderNumber, taxparcel, "Osceola-County-Real-Estate-" + taxparcel + " - " + billyear + " -Annual-bill", "FL", "Osceola");

                                            BillDownload(orderNumber, parcelNumber, BillTax, Taxparcel, strbillyear);
                                        }
                                        catch
                                        {
                                        }


                                        IWebElement ITaxBillCount = IBillHistoryTD[0].FindElement(By.TagName("a"));
                                        string taxlink = ITaxBillCount.GetAttribute("href");
                                        taxhistorylink.Add(taxlink);
                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd().Replace("Installment Bill #", "").Replace("Bill", "");
                                        var Splitbill = strBill.Split(' ');
                                        billyear = Splitbill[0]; inst = Splitbill[1];
                                        strBalance = IBillHistoryTD[1].Text.Replace("\r\n", "").TrimStart().TrimEnd();
                                        string strTaxHistory = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                        gc.insert_date(orderNumber, Taxparcel, 279, strTaxHistory, 1, DateTime.Now);

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
                                        gc.insert_date(orderNumber, Taxparcel, 279, strTaxHistory, 1, DateTime.Now);

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
                                    gc.insert_date(orderNumber, Taxparcel, 279, strTaxHistory, 1, DateTime.Now);
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
                                gc.insert_date(orderNumber, Taxparcel, 279, strTaxHistory, 1, DateTime.Now);
                            }
                        }

                        i++;
                    }

                    int q = 0;
                    foreach (string URL in taxhistorylink)
                    {
                        try
                        {
                            accno = ""; alterkey = ""; taxowner = ""; tax_addr = ""; millagecode = ""; ctax_year = ""; cctaxyear = ""; combinedtaxamount = ""; grosstax = ""; cpaidamount = ""; cpaiddate = ""; ceffdate = ""; creceipt = "";
                            driver.Navigate().GoToUrl(URL);
                            Thread.Sleep(3000);
                            //*[@id="content"]/div[1]/div[8]/div/div[1]                        
                            cctaxyear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[1]/div[2]")).Text.Trim().Replace("Real Estate", "").Replace("Print this bill (PDF)", "").Replace("\r\n", "");
                            gc.CreatePdf(orderNumber, Taxparcel, "Tax Details" + cctaxyear, driver, "FL", "Pasco");

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
                                                gc.insert_date(orderNumber, Taxparcel, 278, valoremtax, 1, DateTime.Now);
                                            }

                                            else if (valoremtablerowTD.Count == 2)
                                            {
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                                gc.insert_date(orderNumber, Taxparcel, 278, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        else if (d == valoremtablerowcount - 1)
                                        {
                                            milagerate = valoremtablerowTD[0].Text;
                                            valoremtax = "Total" + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + valoremtablerowTD[1].Text;
                                            gc.insert_date(orderNumber, Taxparcel, 278, valoremtax, 1, DateTime.Now);
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
                                                valoremtax = nonvaloremtablerowTD[0].Text + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[1].Text + "~" + nonvaloremtablerowTD[2].Text;
                                                gc.insert_date(orderNumber, Taxparcel, 278, valoremtax, 1, DateTime.Now);
                                            }
                                            else if (e == nonvaloremtablerowcount - 1)
                                            {
                                                valoremtax = "Total" + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[0].Text;
                                                gc.insert_date(orderNumber, Taxparcel, 278, valoremtax, 1, DateTime.Now);
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
                                    string[] lines1 = ifpaidbulk.Split(stringSeparators, StringSplitOptions.None);
                                    ifpaidby = lines1[0]; pleasepay = lines1[1];
                                    string single = cctaxyear + "~" + ifpaidby + "~" + pleasepay;
                                    gc.insert_date(orderNumber, Taxparcel, 281, single, 1, DateTime.Now);
                                }

                                else
                                {
                                    string IfPaidBy = "", PlesePay = "", TaxDueDate = "", itaxyear = "";
                                    try
                                    {
                                        IWebElement multitableElement26 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]"));
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
                                                    TaxDueDate = cctaxyear + "~" + IfPaidBy + "~" + PlesePay;
                                                    gc.insert_date(orderNumber, Taxparcel, 281, TaxDueDate, 1, DateTime.Now);

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
                                tax_addr = gc.Between(bulktext1, "Situs address", "Legal description").Trim().Replace(",", "");
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

                            //Account Number~Alternate Key~Owner Name & Mailing Address~Property Address~Millage Code~Millage rate~Tax Year~Tax Amount~Gross Tax~Paid Amount~Paid Date~Effective Date~Receipt Number
                            string curtax = alterkey + "~" + taxowner + "~" + tax_addr + "~" + millagecode + "~" + milagerate + "~" + cctaxyear + "~" + combinedtaxamount + "~" + grosstax + "~" + cpaidamount + "~" + cpaiddate + "~" + ceffdate + "~" + creceipt;
                            gc.insert_date(orderNumber, Taxparcel, 277, curtax, 1, DateTime.Now);
                            q++;
                        }

                        catch
                        {

                        }

                        try
                        {
                            accno = ""; alterkey = ""; taxowner = ""; tax_addr = ""; millagecode = ""; ctax_year = ""; cctaxyear = ""; combinedtaxamount = ""; grosstax = ""; cpaidamount = ""; cpaiddate = ""; ceffdate = ""; creceipt = "";
                            driver.Navigate().GoToUrl(URL);
                            Thread.Sleep(3000);

                            cctaxyear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[1]/div[2]")).Text.Trim().Replace("Real Estate", "").Replace("Print this bill (PDF)", "").Replace("\r\n", "");
                            gc.CreatePdf(orderNumber, Taxparcel, "Tax Details" + cctaxyear, driver, "FL", "Pasco");

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
                                                gc.insert_date(orderNumber, Taxparcel, 278, valoremtax, 1, DateTime.Now);
                                            }

                                            else if (valoremtablerowTD.Count == 2)
                                            {
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                                gc.insert_date(orderNumber, Taxparcel, 278, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        else if (d == valoremtablerowcount - 1)
                                        {
                                            milagerate = valoremtablerowTD[0].Text;
                                            valoremtax = "Total" + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + valoremtablerowTD[1].Text;
                                            gc.insert_date(orderNumber, Taxparcel, 278, valoremtax, 1, DateTime.Now);
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
                                                valoremtax = nonvaloremtablerowTD[0].Text + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[1].Text + "~" + nonvaloremtablerowTD[2].Text;
                                                gc.insert_date(orderNumber, Taxparcel, 278, valoremtax, 1, DateTime.Now);
                                            }
                                            else if (e == nonvaloremtablerowcount - 1)
                                            {
                                                valoremtax = "Total" + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[0].Text;
                                                gc.insert_date(orderNumber, Taxparcel, 278, valoremtax, 1, DateTime.Now);
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
                                    string[] lines1 = ifpaidbulk.Split(stringSeparators, StringSplitOptions.None);
                                    ifpaidby = lines1[0]; pleasepay = lines1[1];
                                    string single = cctaxyear + "~" + ifpaidby + "~" + pleasepay;
                                    gc.insert_date(orderNumber, Taxparcel, 281, single, 1, DateTime.Now);
                                }

                                else
                                {
                                    string IfPaidBy = "", PlesePay = "", TaxDueDate = "", itaxyear = "";
                                    try
                                    {
                                        IWebElement multitableElement26 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]"));
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
                                                    TaxDueDate = cctaxyear + "~" + IfPaidBy + "~" + PlesePay;
                                                    gc.insert_date(orderNumber, Taxparcel, 281, TaxDueDate, 1, DateTime.Now);

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
                                tax_addr = gc.Between(bulktext1, "Situs address", "Legal description").Trim().Replace(",", "");
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

                            //Account Number~Alternate Key~Owner Name & Mailing Address~Property Address~Millage Code~Millage rate~Tax Year~Tax Amount~Gross Tax~Paid Amount~Paid Date~Effective Date~Receipt Number
                            string curtax = alterkey + "~" + taxowner + "~" + tax_addr + "~" + millagecode + "~" + milagerate + "~" + cctaxyear + "~" + combinedtaxamount + "~" + grosstax + "~" + cpaidamount + "~" + cpaiddate + "~" + ceffdate + "~" + creceipt;
                            gc.insert_date(orderNumber, Taxparcel, 277, curtax, 1, DateTime.Now);
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
                            accno = ""; alterkey = ""; taxowner = ""; tax_addr = ""; millagecode = ""; ctax_year = ""; cctaxyear = ""; combinedtaxamount = ""; grosstax = ""; cpaidamount = ""; cpaiddate = ""; ceffdate = ""; creceipt = "";
                            driver.Navigate().GoToUrl(URL);
                            Thread.Sleep(3000);
                            //*[@id="content"]/div[1]/div[8]/div/div[1]                        
                            cctaxyear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[1]/div[2]")).Text.Trim().Replace("Real Estate", "").Replace("Print this bill (PDF)", "").Replace("\r\n", "");
                            gc.CreatePdf(orderNumber, Taxparcel, "Tax Details" + cctaxyear, driver, "FL", "Pasco");

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
                                                gc.insert_date(orderNumber, Taxparcel, 278, valoremtax, 1, DateTime.Now);
                                            }

                                            else if (valoremtablerowTD.Count == 2)
                                            {
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                                gc.insert_date(orderNumber, Taxparcel, 278, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        else if (d == valoremtablerowcount - 1)
                                        {
                                            milagerate = valoremtablerowTD[0].Text;
                                            valoremtax = "Total" + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + valoremtablerowTD[1].Text;
                                            gc.insert_date(orderNumber, Taxparcel, 278, valoremtax, 1, DateTime.Now);
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
                                                valoremtax = nonvaloremtablerowTD[0].Text + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[1].Text + "~" + nonvaloremtablerowTD[2].Text;
                                                gc.insert_date(orderNumber, Taxparcel, 278, valoremtax, 1, DateTime.Now);
                                            }
                                            else if (e == nonvaloremtablerowcount - 1)
                                            {
                                                valoremtax = "Total" + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[0].Text;
                                                gc.insert_date(orderNumber, Taxparcel, 278, valoremtax, 1, DateTime.Now);
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
                                    string[] lines1 = ifpaidbulk.Split(stringSeparators, StringSplitOptions.None);
                                    ifpaidby = lines1[0]; pleasepay = lines1[1];
                                    string single = cctaxyear + "~" + ifpaidby + "~" + pleasepay;
                                    gc.insert_date(orderNumber, Taxparcel, 281, single, 1, DateTime.Now);
                                }

                                else
                                {
                                    string IfPaidBy = "", PlesePay = "", TaxDueDate = "", itaxyear = "";
                                    try
                                    {
                                        IWebElement multitableElement26 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]"));
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
                                                    TaxDueDate = cctaxyear + "~" + IfPaidBy + "~" + PlesePay;
                                                    gc.insert_date(orderNumber, Taxparcel, 281, TaxDueDate, 1, DateTime.Now);

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
                                tax_addr = gc.Between(bulktext1, "Situs address", "Legal description").Trim().Replace(",", "");
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
                            //IWebElement ITaxSearchparcell = driver.FindElement(By.LinkText("Parcel details"));
                            //string strITaxSearchparcell = ITaxSearchparcell.GetAttribute("href");
                            //driver.Navigate().GoToUrl(strITaxSearchparcell);
                            //Thread.Sleep(3000);
                            //gc.CreatePdf(orderNumber, outparcelno, "Parcel Details" + cctaxyear, driver,  "FL", "Osceola");


                            //Account Number~Alternate Key~Owner Name & Mailing Address~Property Address~Millage Code~Millage rate~Tax Year~Tax Amount~Gross Tax~Paid Amount~Paid Date~Effective Date~Receipt Number
                            string curtax = alterkey + "~" + taxowner + "~" + tax_addr + "~" + millagecode + "~" + milagerate + "~" + cctaxyear + "~" + combinedtaxamount + "~" + grosstax + "~" + cpaidamount + "~" + cpaiddate + "~" + ceffdate + "~" + creceipt;
                            gc.insert_date(orderNumber, Taxparcel, 277, curtax, 1, DateTime.Now);
                            q++;
                        }

                        catch
                        {

                        }

                        try
                        {
                            accno = ""; alterkey = ""; taxowner = ""; tax_addr = ""; millagecode = ""; ctax_year = ""; cctaxyear = ""; combinedtaxamount = ""; grosstax = ""; cpaidamount = ""; cpaiddate = ""; ceffdate = ""; creceipt = "";
                            driver.Navigate().GoToUrl(URL);
                            Thread.Sleep(3000);

                            cctaxyear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[1]/div[2]")).Text.Trim().Replace("Real Estate", "").Replace("Print this bill (PDF)", "").Replace("\r\n", "");
                            gc.CreatePdf(orderNumber, Taxparcel, "Tax Details" + cctaxyear, driver, "FL", "Pasco");

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
                                                gc.insert_date(orderNumber, Taxparcel, 278, valoremtax, 1, DateTime.Now);
                                            }

                                            else if (valoremtablerowTD.Count == 2)
                                            {
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                                gc.insert_date(orderNumber, Taxparcel, 278, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        else if (d == valoremtablerowcount - 1)
                                        {
                                            milagerate = valoremtablerowTD[0].Text;
                                            valoremtax = "Total" + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + valoremtablerowTD[1].Text;
                                            gc.insert_date(orderNumber, Taxparcel, 278, valoremtax, 1, DateTime.Now);
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
                                                valoremtax = nonvaloremtablerowTD[0].Text + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[1].Text + "~" + nonvaloremtablerowTD[2].Text;
                                                gc.insert_date(orderNumber, Taxparcel, 278, valoremtax, 1, DateTime.Now);
                                            }
                                            else if (e == nonvaloremtablerowcount - 1)
                                            {
                                                valoremtax = "Total" + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[0].Text;
                                                gc.insert_date(orderNumber, Taxparcel, 278, valoremtax, 1, DateTime.Now);
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
                                    string[] lines1 = ifpaidbulk.Split(stringSeparators, StringSplitOptions.None);
                                    ifpaidby = lines1[0]; pleasepay = lines1[1];
                                    string single = cctaxyear + "~" + ifpaidby + "~" + pleasepay;
                                    gc.insert_date(orderNumber, Taxparcel, 281, single, 1, DateTime.Now);
                                }

                                else
                                {
                                    string IfPaidBy = "", PlesePay = "", TaxDueDate = "", itaxyear = "";
                                    try
                                    {
                                        IWebElement multitableElement26 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]"));
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
                                                    TaxDueDate = cctaxyear + "~" + IfPaidBy + "~" + PlesePay;
                                                    gc.insert_date(orderNumber, Taxparcel, 281, TaxDueDate, 1, DateTime.Now);

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
                                tax_addr = gc.Between(bulktext1, "Situs address", "Legal description").Trim().Replace(",", "");
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
                            //Account Number~Alternate Key~Owner Name & Mailing Address~Property Address~Millage Code~Millage rate~Tax Year~Tax Amount~Gross Tax~Paid Amount~Paid Date~Effective Date~Receipt Number
                            string curtax = alterkey + "~" + taxowner + "~" + tax_addr + "~" + millagecode + "~" + milagerate + "~" + cctaxyear + "~" + combinedtaxamount + "~" + grosstax + "~" + cpaidamount + "~" + cpaiddate + "~" + ceffdate + "~" + creceipt;
                            gc.insert_date(orderNumber, Taxparcel, 277, curtax, 1, DateTime.Now);
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
                            string issueyear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/p")).Text.Trim().Replace("This parcel has an issued certificate for ", "").Replace(".", "");
                            gc.CreatePdf(orderNumber, Taxparcel, "Issue Certificate Details" + issueyear, driver, "FL", "Pasco");
                            string issuecertificatebulktext = driver.FindElement(By.XPath("//*[@id='content']/div[1]/dl")).Text.Trim();
                            cer_no = driver.FindElement(By.XPath("//*[@id='certificate']")).Text.Replace("Certificate #", "");
                            adno = gc.Between(issuecertificatebulktext, "Advertised number", "Face amount").Trim();
                            faceamount = gc.Between(issuecertificatebulktext, "Face amount", "Issued date").Trim();
                            issuedate = gc.Between(issuecertificatebulktext, "Issued date", "Expiration date").Trim();
                            ex_date = gc.Between(issuecertificatebulktext, "Expiration date", "Buyer").Trim();
                            buyer = gc.Between(issuecertificatebulktext, "Buyer", "Interest rate").Trim().Replace("\r\n", ",");
                            intrate = GlobalClass.After(issuecertificatebulktext, "Interest rate");
                            //Tax Year~Certificate Number~Advertised Number~Face Amount~Issued Date~Expiration Date~Buyer~Interest Rate
                            string isscer = issueyear + "~" + cer_no + "~" + adno + "~" + faceamount + "~" + issuedate + "~" + ex_date + "~" + buyer + "~" + intrate;
                            gc.insert_date(orderNumber, Taxparcel, 280, isscer, 1, DateTime.Now);
                        }
                        catch { }
                    }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "FL", "Pasco", StartTime, AssessmentTime, TaxTime, CityTaxtakentime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "FL", "Pasco");
                    // gc.MMREM_Template(orderNumber, parcelNumber, "", driver, "FL", "Pasco", "99", "4");
                    return "Data Inserted Successfully";
                }
                catch (Exception ex1)
                {
                    driver.Quit();
                    throw ex1;
                }
            }
        }
        public void BillDownload(string orderno, string parcelNumber, string BillTax, string taxparcel, string strbillyear)
        {
            string fileName = "";
            var chromeOptions = new ChromeOptions();
            var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];

            chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
            chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
            chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");

            var chDriver = new ChromeDriver(chromeOptions);
            try
            {
                chDriver.Navigate().GoToUrl(BillTax);
                Thread.Sleep(3000);
                try
                {

                    fileName = "Pasco-County-Real-Estate-" + taxparcel + "-" + strbillyear + "-Annual-bill.pdf";
                    gc.AutoDownloadFile(orderno, parcelNumber, "Pasco", "FL", fileName);
                }
                catch { }
                try
                {
                    fileName = "Pasco-County-Real-Estate-" + taxparcel + "-" + strbillyear + "-Installment-bill-1.pdf";
                    gc.AutoDownloadFile(orderno, parcelNumber, "Pasco", "FL", fileName);
                }
                catch { }
                try
                {
                    fileName = "Pasco-County-Real-Estate-" + taxparcel + "-" + strbillyear + "-Installment-bill-2.pdf";
                    gc.AutoDownloadFile(orderno, parcelNumber, "Pasco", "FL", fileName);
                }
                catch { }
                try
                {
                    fileName = "Pasco-County-Real-Estate-" + taxparcel + "-" + strbillyear + "-Installment-bill-3.pdf";
                    gc.AutoDownloadFile(orderno, parcelNumber, "Pasco", "FL", fileName);
                }
                catch { }

                try
                {
                    fileName = "Pasco-County-Real-Estate-" + taxparcel + "-" + strbillyear + "-Installment-bill-4.pdf";
                    gc.AutoDownloadFile(orderno, parcelNumber, "Pasco", "FL", fileName);
                }
                catch { }

                chDriver.Quit();
            }
            catch (Exception ex)
            {

            }
        }
    }
}