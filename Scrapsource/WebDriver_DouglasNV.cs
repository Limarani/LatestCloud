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
    public class WebDriver_DouglasNV
    {

        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();

        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_DouglasNV(string houseno, string Direction, string sname, string stype, string account, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string lastName = "", firstName = "", Pinnumber = "", PropertyAdd = "", Strownername = "", Pin = "", address = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                //  using (driver = new ChromeDriver())

                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("http://assessor-search.douglasnv.us:1401/cgi-bin/asw100");
                    Thread.Sleep(4000);

                    if (searchType == "titleflex")
                    {
                        if (Direction != "")
                        {
                            address = houseno + " " + Direction + " " + sname + " " + stype + " " + account;
                            address = address.Trim();
                        }
                        if (Direction == "")
                        {
                            address = houseno + " " + sname + " " + stype + " " + account;
                            address = address.Trim();
                        }
                        string titleaddress = address;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "NV", "Douglas");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Zero_DouglasNV"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        if (Direction != "" && stype != "")
                        {
                            address = houseno + " " + Direction + " " + sname + " " + stype + " " + account;
                        }
                        if (Direction != "" && stype == "")
                        {
                            address = houseno + " " + Direction + " " + sname + " " + account;
                        }
                        if (Direction == "" && stype != "")
                        {
                            address = houseno + " " + sname + " " + stype + " " + account;
                        }
                        if (Direction == "" && stype == "")
                        {
                            address = houseno + " " + sname + " " + account;
                        }

                        driver.FindElement(By.XPath("//*[@id='body']/form[2]/table/tbody/tr/td/table/tbody/tr[7]/td[4]/div/input")).SendKeys(address.Trim());
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "NV", "Douglas");
                        driver.FindElement(By.XPath("//*[@id='body']/form[2]/table/tbody/tr/td/table/tbody/tr[10]/td[5]/div/input")).SendKeys(Keys.Enter);
                        Thread.Sleep(4000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search result", driver, "NV", "Douglas");

                        try
                        {
                            int Max = 0;
                            string strowner = "", strAddress = "", strCity = "";
                            string Record = "";


                            IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='body']/form[2]/table/tbody/tr[3]/td/table/tbody"));

                            IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> multiTD;
                            foreach (IWebElement multi in multiRow)
                            {
                                multiTD = multi.FindElements(By.TagName("td"));
                                if (multi.Text.Contains(address.ToUpper()) && multiTD.Count != 0 && multi.Text.Trim() != "" && !multi.Text.Contains("Search Results") && !multi.Text.Contains("Property Location"))
                                {
                                    Strownername = multiTD[1].Text;

                                    parcelNumber = multiTD[0].Text.Trim();
                                    PropertyAdd = multiTD[2].Text + " " + multiTD[3].Text;

                                    string multidetails = Strownername + "~" + PropertyAdd;
                                    gc.insert_date(orderNumber, parcelNumber, 1757, multidetails, 1, DateTime.Now);
                                    Max++;
                                }
                                if (multiTD.Count != 0 && multiRow.Count > 25 && Max >= 25)
                                {
                                    HttpContext.Current.Session["multiparcel_DouglasNV_Maximum"] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }

                            }
                            if (Max > 1 && Max < 25)
                            {
                                HttpContext.Current.Session["multiparcel_DouglasNV"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Max == 1)
                            {
                                IWebElement multiaddress1 = driver.FindElement(By.XPath("//*[@id='body']/form[2]/table/tbody/tr[3]/td/table/tbody"));

                                IList<IWebElement> multiRow1 = multiaddress1.FindElements(By.TagName("tr"));
                                IList<IWebElement> multiTD1;
                                foreach (IWebElement multi in multiRow1)
                                {
                                    multiTD1 = multi.FindElements(By.TagName("td"));
                                    if (multi.Text.Contains(address.ToUpper()) && multiTD1.Count != 0 && multi.Text.Trim() != "" && !multi.Text.Contains("Search Results") && !multi.Text.Contains("Property Location"))
                                    {
                                        Strownername = multiTD1[1].Text;
                                        parcelNumber = multiTD1[0].Text.Trim();
                                        PropertyAdd = multiTD1[2].Text + " " + multiTD1[3].Text;

                                        string nparcel = parcelNumber.Replace("-", "");
                                        string url = "http://assessor-search.douglasnv.us:1401/cgi-bin/asw101?Parcel=" + nparcel + "&aori=a";
                                        driver.Navigate().GoToUrl(url);
                                        Thread.Sleep(4000);
                                        break;
                                    }
                                }
                            }
                            if (Max == 0)
                            {
                                HttpContext.Current.Session["Zero_DouglasNV"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }

                        }
                        catch { }
                    }


                    else if (searchType == "parcel")
                    {
                        parcelNumber = parcelNumber.Replace("-", "");
                        driver.FindElement(By.XPath("//*[@id='asw100parcels']/input[1]")).SendKeys(parcelNumber);
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search", driver, "NV", "Douglas");
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='body']/form[2]/table/tbody/tr/td/table/tbody/tr[10]/td[5]/div/input")).SendKeys(Keys.Enter);
                            Thread.Sleep(3000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Parcel search Result", driver, "NV", "Douglas");
                        }
                        catch { }
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='body']/form[2]/table/tbody/tr[3]/td/table/tbody/tr[3]/td[1]/div/a")).Click();
                            Thread.Sleep(4000);
                        }
                        catch { }
                        try
                        {
                            int Max = 0;
                            string strowner = "", strAddress = "", strCity = "";
                            string Record = "";


                            IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='body']/form[2]/table/tbody/tr[3]/td/table/tbody"));

                            IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> multiTD;
                            foreach (IWebElement multi in multiRow)
                            {
                                multiTD = multi.FindElements(By.TagName("td"));
                                if (multi.Text.Contains(address.ToUpper()) && multiTD.Count != 0 && multi.Text.Trim() != "" && !multi.Text.Contains("Search Results") && !multi.Text.Contains("Property Location") && multi.Text.Contains("No results found"))
                                {
                                    HttpContext.Current.Session["Zero_DouglasNV"] = "Zero";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                        }
                        catch { }
                    }

                    if (searchType == "ownername")
                    {

                        driver.FindElement(By.XPath("//*[@id='asw100name']/input")).SendKeys(ownername);
                        Thread.Sleep(3000);

                        gc.CreatePdf_WOP(orderNumber, "OwnerName search", driver, "NV", "Douglas");
                        driver.FindElement(By.XPath("//*[@id='body']/form[2]/table/tbody/tr/td/table/tbody/tr[10]/td[5]/div/input")).Click();
                        Thread.Sleep(4000);

                        try
                        {
                            int Max = 0;
                            string strowner = "", strAddress = "", strCity = "";
                            string Record = "";


                            IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='body']/form[2]/table/tbody/tr[3]/td/table/tbody"));

                            IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> multiTD;
                            foreach (IWebElement multi in multiRow)
                            {
                                multiTD = multi.FindElements(By.TagName("td"));
                                if (multi.Text.Contains(address.ToUpper()) && multiTD.Count != 0 && multi.Text.Trim() != "" && !multi.Text.Contains("Search Results") && !multi.Text.Contains("Property Location"))
                                {
                                    Strownername = multiTD[1].Text;

                                    parcelNumber = multiTD[0].Text.Trim();
                                    PropertyAdd = multiTD[2].Text + " " + multiTD[3].Text;

                                    string multidetails = Strownername + "~" + PropertyAdd;
                                    gc.insert_date(orderNumber, parcelNumber, 1757, multidetails, 1, DateTime.Now);
                                    Max++;
                                }
                                if (multiTD.Count != 0 && multiRow.Count > 25 && Max >= 25)
                                {
                                    HttpContext.Current.Session["multiparcel_DouglasNV_Maximum"] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }

                            }
                            if (Max > 1 && Max < 25)
                            {
                                HttpContext.Current.Session["multiparcel_DouglasNV"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Max == 1)
                            {
                                IWebElement multiaddress1 = driver.FindElement(By.XPath("//*[@id='body']/form[2]/table/tbody/tr[3]/td/table/tbody"));

                                IList<IWebElement> multiRow1 = multiaddress1.FindElements(By.TagName("tr"));
                                IList<IWebElement> multiTD1;
                                foreach (IWebElement multi in multiRow1)
                                {
                                    multiTD1 = multi.FindElements(By.TagName("td"));
                                    if (multi.Text.Contains(address.ToUpper()) && multiTD1.Count != 0 && multi.Text.Trim() != "" && !multi.Text.Contains("Search Results") && !multi.Text.Contains("Property Location"))
                                    {
                                        Strownername = multiTD1[1].Text;
                                        parcelNumber = multiTD1[0].Text.Trim();
                                        PropertyAdd = multiTD1[2].Text + " " + multiTD1[3].Text;

                                        string nparcel = parcelNumber.Replace("-", "");
                                        string url = "http://assessor-search.douglasnv.us:1401/cgi-bin/asw101?Parcel=" + nparcel + "&aori=a";
                                        driver.Navigate().GoToUrl(url);
                                        Thread.Sleep(4000);
                                        break;
                                    }
                                }
                            }
                            if (Max == 0)
                            {
                                HttpContext.Current.Session["Zero_DouglasNV"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }

                        }
                        catch { }
                    }


                    //property details

                    string MailingAddress = "", Town = "", District = "", AssessedOwnerName = "";
                    string LegalOwnerName = "", PropertyAddress = "", Acres = "", YearBuilt = "";


                    parcelNumber = driver.FindElement(By.XPath("//*[@id='body']/form[2]/table[2]/tbody/tr[1]/td/div")).Text.Replace("Parcel Detail for Parcel #", "").Trim();

                    PropertyAddress = driver.FindElement(By.XPath("//*[@id='body']/form[2]/table[2]/tbody/tr[3]/td[1]/table/tbody/tr[2]/td[2]/div")).Text;
                    Town = driver.FindElement(By.XPath("//*[@id='body']/form[2]/table[2]/tbody/tr[3]/td[1]/table/tbody/tr[3]/td[2]/div")).Text;
                    District = driver.FindElement(By.XPath("//*[@id='body']/form[2]/table[2]/tbody/tr[3]/td[1]/table/tbody/tr[4]/td[2]/div")).Text;

                    AssessedOwnerName = driver.FindElement(By.XPath("//*[@id='body']/form[2]/table[2]/tbody/tr[3]/td[2]/table/tbody/tr[2]/td[2]/div")).Text;
                    LegalOwnerName = driver.FindElement(By.XPath("//*[@id='body']/form[2]/table[2]/tbody/tr[3]/td[2]/table/tbody/tr[4]/td[2]/div")).Text;
                    MailingAddress = driver.FindElement(By.XPath("//*[@id='body']/form[2]/table[2]/tbody/tr[3]/td[2]/table/tbody/tr[3]/td[2]/div")).Text;
                    try
                    {

                        YearBuilt = driver.FindElement(By.XPath("//*[@id='body']/form[2]/table[2]/tbody/tr[4]/td[2]/table/tbody/tr[9]/td[2]/div")).Text;
                    }
                    catch { }
                    //try
                    //{
                    //    if (YearBuilt == "")
                    //    {
                    //        YearBuilt = driver.FindElement(By.XPath("/html/body/table[15]/tbody/tr[2]/td[1]/p")).Text;
                    //    }
                    //}
                    //catch { }
                    string propertydetails = PropertyAddress + "~" + Town + "~" + District + "~" + AssessedOwnerName + "~" + MailingAddress + "~" + LegalOwnerName + "~" + YearBuilt;
                    gc.insert_date(orderNumber, parcelNumber, 1765, propertydetails, 1, DateTime.Now);

                    gc.CreatePdf(orderNumber, parcelNumber, "Property Details", driver, "NV", "Douglas");

                    // Assessment Details - (Assessed Valuation)

                    try
                    {

                        string Assyear = "", Assyear1 = "", Assyear2 = "", Assesstype = "";
                        IWebElement Assessmentdetails = driver.FindElement(By.XPath("//*[@id='body']/form[2]/table[2]/tbody/tr[5]/td[1]/table/tbody"));
                        IList<IWebElement> TRAssessmentdetails = Assessmentdetails.FindElements(By.TagName("tr"));
                        IList<IWebElement> THAssessmentdetails = Assessmentdetails.FindElements(By.TagName("th"));
                        IList<IWebElement> TDAssessmentdetails;
                        foreach (IWebElement row in TRAssessmentdetails)
                        {
                            TDAssessmentdetails = row.FindElements(By.TagName("td"));
                            if (!row.Text.Contains("Assessed Valuation") && row.Text.Trim() != "")
                            {

                                if (TDAssessmentdetails.Count != 0 && !row.Text.Contains("Increased (New) Values"))
                                {
                                    Assesstype += TDAssessmentdetails[0].Text.Replace("Values", "Year") + "~";
                                    Assyear += TDAssessmentdetails[1].Text + "~";
                                    Assyear1 += TDAssessmentdetails[2].Text + "~";
                                    Assyear2 += TDAssessmentdetails[3].Text + "~";
                                }
                                if (TDAssessmentdetails.Count != 0 && row.Text.Contains("Increased (New) Values"))
                                {
                                    break;
                                }
                            }
                        }

                        DBconnection dbconn = new DBconnection();
                        dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + Assesstype.Remove(Assesstype.Length - 1, 1) + "' where Id = '" + 1766 + "'");
                        gc.insert_date(orderNumber, parcelNumber, 1766, Assyear.Remove(Assyear.Length - 1, 1), 1, DateTime.Now);
                        gc.insert_date(orderNumber, parcelNumber, 1766, Assyear1.Remove(Assyear1.Length - 1, 1), 1, DateTime.Now);
                        gc.insert_date(orderNumber, parcelNumber, 1766, Assyear2.Remove(Assyear2.Length - 1, 1), 1, DateTime.Now);
                    }
                    catch { }

                    // Assessment details - (Taxable Valuation)

                    try
                    {

                        string taxyear = "", taxyear1 = "", taxyear2 = "", valuetype = "";
                        IWebElement Assessmentdetails = driver.FindElement(By.XPath("//*[@id='body']/form[2]/table[2]/tbody/tr[5]/td[2]/table/tbody"));
                        IList<IWebElement> TRAssessmentdetails = Assessmentdetails.FindElements(By.TagName("tr"));
                        IList<IWebElement> THAssessmentdetails = Assessmentdetails.FindElements(By.TagName("th"));
                        IList<IWebElement> TDAssessmentdetails;
                        foreach (IWebElement row in TRAssessmentdetails)
                        {
                            TDAssessmentdetails = row.FindElements(By.TagName("td"));
                            if (!row.Text.Contains("Taxable Valuation") && row.Text.Trim() != "")
                            {

                                if (TDAssessmentdetails.Count != 0 && !row.Text.Contains("Increased (New) Values"))
                                {
                                    valuetype += TDAssessmentdetails[0].Text.Replace("Values", "Year") + "~";
                                    taxyear += TDAssessmentdetails[1].Text + "~";
                                    taxyear1 += TDAssessmentdetails[2].Text + "~";
                                    taxyear2 += TDAssessmentdetails[3].Text + "~";
                                }
                                if (TDAssessmentdetails.Count != 0 && row.Text.Contains("Increased (New) Values"))
                                {
                                    break;
                                }
                            }
                        }

                        DBconnection dbconn = new DBconnection();
                        dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + valuetype.Remove(valuetype.Length - 1, 1) + "' where Id = '" + 1767 + "'");
                        gc.insert_date(orderNumber, parcelNumber, 1767, taxyear.Remove(taxyear.Length - 1, 1), 1, DateTime.Now);
                        gc.insert_date(orderNumber, parcelNumber, 1767, taxyear1.Remove(taxyear1.Length - 1, 1), 1, DateTime.Now);
                        gc.insert_date(orderNumber, parcelNumber, 1767, taxyear2.Remove(taxyear2.Length - 1, 1), 1, DateTime.Now);
                    }
                    catch { }


                    // Tax Information Details
                    string taxAuth = "", taxauth1 = "", taxauth2 = "", taxauth3 = "";
                    try
                    {
                        driver.Navigate().GoToUrl("https://cltr.douglasnv.us/");
                        Thread.Sleep(5000);
                        taxauth1 = driver.FindElement(By.XPath("//*[@id='text-16']/div/h4")).Text;
                        taxauth2 = driver.FindElement(By.XPath("//*[@id='text-16']/div/div/p[1]")).Text.Replace("MINDEN:", "").Trim();
                        taxauth3 = driver.FindElement(By.XPath("//*[@id='text-16']/div/div/p[3]")).Text.Replace("Treasurer:", "").Replace("(map)", "").Trim();
                        taxAuth = taxauth1 + " " + taxauth2 + " " + taxauth3;
                        gc.CreatePdf(orderNumber, parcelNumber, "Taxing Authority", driver, "NV", "Douglas");
                    }
                    catch { }

                    driver.Navigate().GoToUrl("https://cltr.douglasnv.us/online-payments/property-taxes/");
                    Thread.Sleep(5000);
                    var Select1 = driver.FindElement(By.XPath("//*[@id='filterBy']"));
                    var selectElement11 = new SelectElement(Select1);
                    selectElement11.SelectByText("Parcel Number");

                    driver.FindElement(By.XPath("//*[@id='searchFor']")).SendKeys(parcelNumber);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Search", driver, "NV", "Douglas");

                    driver.FindElement(By.Id("buttonSearch")).Click();
                    Thread.Sleep(4000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Search Result", driver, "NV", "Douglas");

                    driver.FindElement(By.XPath("//*[@id='content']/div/div[2]/div/table/tbody/tr/td[4]/input")).Click();
                    Thread.Sleep(4000);

                    gc.CreatePdf(orderNumber, parcelNumber, "Current Tax Details", driver, "NV", "Douglas");
                    string strownerName = "", PropAddress = "", Taxyear = "", AccountBalance = "";

                    strownerName = driver.FindElement(By.XPath("//*[@id='content']/div/div[3]/div[2]/strong")).Text;
                    PropAddress = driver.FindElement(By.XPath("//*[@id='content']/div/div[4]/div[2]/strong")).Text;
                    Taxyear = driver.FindElement(By.XPath("//*[@id='content']/div/div[2]/div/h4")).Text.Replace("Tax Summary for", "").Trim();
                    AccountBalance = driver.FindElement(By.XPath("//*[@id='content']/div/div[3]/div[3]/strong")).Text;

                    try
                    {
                        string TaxInfodetails = strownerName + "~" + PropAddress + "~" + Taxyear + "~" + AccountBalance + "~" + taxAuth;
                        gc.insert_date(orderNumber, parcelNumber, 1770, TaxInfodetails, 1, DateTime.Now);

                    }
                    catch { }

                    // Current tax Year Details
                    try
                    {
                        IWebElement CurrentTax = driver.FindElement(By.XPath("//*[@id='content']/div"));
                        IList<IWebElement> TRCurrentTax = CurrentTax.FindElements(By.TagName("div"));
                        IList<IWebElement> THCurrentTax = CurrentTax.FindElements(By.TagName("div"));
                        IList<IWebElement> TDCurrentTax;
                        foreach (IWebElement row in TRCurrentTax)
                        {
                            TDCurrentTax = row.FindElements(By.TagName("div"));
                            if (TDCurrentTax.Count != 0 && !row.Text.Contains("Disposition") && row.Text.Trim() != "" && TDCurrentTax.Count == 6)
                            {
                                string CurrentTaxdetails = TDCurrentTax[0].Text + "~" + TDCurrentTax[1].Text + "~" + TDCurrentTax[2].Text + "~" + TDCurrentTax[3].Text + "~" + TDCurrentTax[4].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1771, CurrentTaxdetails, 1, DateTime.Now);
                            }
                            if (row.Text.Contains("Penalty") && row.Text.Trim() != "" && TDCurrentTax.Count == 3)
                            {
                                string CurrentTaxdetails = "" + "~" + TDCurrentTax[0].Text + "~" + TDCurrentTax[1].Text + "~" + "" + "~" + "";
                                gc.insert_date(orderNumber, parcelNumber, 1771, CurrentTaxdetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    try
                    {
                        driver.FindElement(By.Id("buttonHistory")).Click();
                        Thread.Sleep(6000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Payment History", driver, "NV", "Douglas");
                    }
                    catch { }

                    // Tax Payment History Details
                    try
                    {
                        IWebElement TaxPayment = driver.FindElement(By.XPath("/html/body/div[2]/div/div/div[2]/div/div/table/tbody"));
                        IList<IWebElement> TRTaxPayment = TaxPayment.FindElements(By.TagName("tr"));
                        IList<IWebElement> THTaxPayment = TaxPayment.FindElements(By.TagName("th"));
                        IList<IWebElement> TDTaxPayment;
                        foreach (IWebElement row in TRTaxPayment)
                        {
                            TDTaxPayment = row.FindElements(By.TagName("td"));
                            if (TDTaxPayment.Count != 0 && !row.Text.Contains("PENALTY") && row.Text.Trim() != "")
                            {
                                string TaxPaymentdetails = TDTaxPayment[0].Text + "~" + TDTaxPayment[1].Text + "~" + TDTaxPayment[2].Text + "~" + TDTaxPayment[3].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1772, TaxPaymentdetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }


                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[2]/div/div/div[3]/button")).Click();
                        Thread.Sleep(4000);
                    }
                    catch { }

                    // AMG Details
                    try
                    {
                        driver.Navigate().GoToUrl("https://amgnv.com/parcelsearch_non_pop1.asp");
                        Thread.Sleep(5000);
                        string strparcel = parcelNumber.Replace("-", "");
                        driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[6]/table/tbody/tr[2]/td[2]/p/table[2]/tbody/tr/td/table/tbody/tr[2]/td[1]/form/center/b/input")).SendKeys(strparcel);
                        gc.CreatePdf(orderNumber, parcelNumber, "AMG Details Search", driver, "NV", "Douglas");
                        driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[6]/table/tbody/tr[2]/td[2]/p/table[2]/tbody/tr/td/table/tbody/tr[2]/td[1]/form/center/b/font/font/input")).Click();
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcelNumber, "AMG Details Search Result", driver, "NV", "Douglas");
                    }
                    catch { }
                    string AmgBigdata = "";
                    try
                    {
                        AmgBigdata = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[6]/table/tbody/tr[2]/td[2]/p/table[2]/tbody/tr/td/b/font")).Text;
                    }
                    catch { }
                    if (!AmgBigdata.Contains("No Records were found in the database"))
                    {

                        try
                        {
                            driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[6]/table/tbody/tr[2]/td[2]/p/font[1]/table/tbody/tr[2]/td[4]/div/font/a")).Click();
                            Thread.Sleep(4000);
                            gc.CreatePdf(orderNumber, parcelNumber, "AMG Details Info", driver, "NV", "Douglas");
                        }
                        catch { }

                        string District_AMG_ID = "", OwnerName = "", status = "", Unbilled_Principal = "", ProAdd = "", LegalDesc = "", OriginalAssess = "";
                        string PayOff = "", MailingDate = "", DueDate = "", FinalPayment = "", Checks_payable = "";

                        District_AMG_ID = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[6]/table/tbody/tr[2]/td[2]/table[3]/tbody/tr[3]/td[2]/center/font")).Text;
                        OwnerName = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[6]/table/tbody/tr[2]/td[2]/table[3]/tbody/tr[3]/td[3]/font")).Text;
                        status = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[6]/table/tbody/tr[2]/td[2]/table[3]/tbody/tr[3]/td[4]")).Text;
                        Unbilled_Principal = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[6]/table/tbody/tr[2]/td[2]/table[3]/tbody/tr[3]/td[5]/div/font")).Text;
                        ProAdd = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[6]/table/tbody/tr[2]/td[2]/table[4]/tbody/tr[2]/td[1]/font")).Text;
                        LegalDesc = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[6]/table/tbody/tr[2]/td[2]/table[4]/tbody/tr[3]/td/font")).Text.Trim();
                        OriginalAssess = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[6]/table/tbody/tr[2]/td[2]/table[4]/tbody/tr[2]/td[2]/center/font")).Text;
                        PayOff = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[6]/table/tbody/tr[2]/td[2]/table[4]/tbody/tr[2]/td[3]/center/font/a")).Text;
                        MailingDate = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[6]/table/tbody/tr[2]/td[2]/table[8]/tbody/tr[1]/td[1]/font")).Text.Replace("Mailing Date:", "").Trim();
                        DueDate = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[6]/table/tbody/tr[2]/td[2]/table[8]/tbody/tr[2]/td[1]/font")).Text.Replace("Due Dates:", "").Trim();
                        FinalPayment = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[6]/table/tbody/tr[2]/td[2]/table[8]/tbody/tr[3]/td/font")).Text.Replace(":", "").Trim();

                        Checks_payable = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[6]/table/tbody/tr[2]/td[2]/table[8]/tbody/tr[2]/td[2]/table/tbody/tr/td[2]/font")).Text.Trim();

                        string AMGdetails = District_AMG_ID + "~" + OwnerName + "~" + status + "~" + Unbilled_Principal + "~" + ProAdd + "~" + LegalDesc + "~" + OriginalAssess + "~" + PayOff + "~" + MailingDate + "~" + DueDate + "~" + FinalPayment + "~" + Checks_payable;
                        gc.insert_date(orderNumber, parcelNumber, 1773, AMGdetails, 1, DateTime.Now);

                        // AMG Due Details


                        try
                        {
                            IWebElement AMGDueDetails = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[6]/table/tbody/tr[2]/td[2]/table[6]/tbody"));
                            IList<IWebElement> TRAMGDueDetails = AMGDueDetails.FindElements(By.TagName("tr"));
                            IList<IWebElement> THAMGDueDetails = AMGDueDetails.FindElements(By.TagName("th"));
                            IList<IWebElement> TDAMGDueDetails;
                            foreach (IWebElement row in TRAMGDueDetails)
                            {
                                TDAMGDueDetails = row.FindElements(By.TagName("td"));
                                if (TDAMGDueDetails.Count != 0 && !row.Text.Contains("Total Due") && row.Text.Trim() != "" && TDAMGDueDetails.Count == 6)
                                {
                                    string AMGDuedetails = TDAMGDueDetails[0].Text + "~" + TDAMGDueDetails[1].Text + "~" + TDAMGDueDetails[2].Text + "~" + TDAMGDueDetails[3].Text + "~" + TDAMGDueDetails[4].Text + "~" + TDAMGDueDetails[5].Text;
                                    gc.insert_date(orderNumber, parcelNumber, 1774, AMGDuedetails, 1, DateTime.Now);
                                }
                            }
                        }
                        catch { }

                    }


                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "NV", "Douglas", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "NV", "Douglas");
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
