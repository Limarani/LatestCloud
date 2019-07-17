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
    public class WebDriver_harrison
    {
        string Outparcelno = "", outputPath = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        private TimeSpan timeOutInSeconds;
        MySqlParameter[] mParam;

        public string FTP_Harrison(string houseno, string sname, string sttype, string parcelNumber, string searchType, string orderNumber, string ownername, string directParcel, string account)
        {
            string NAME = "", PROPERTY = "", ADDRESS = "", PARCEL = "";
            string Parcel_No = "-", Owner_Name = "-", Property_Address = "-", Legal_Description = "-", Exempt_Code = "-", Homestead_Code = "-", PPIN = "-", Section = "-", Township = "-", Range = "-";
            string Tax_Year = "-", Assessed_Land_Value = "-", Assessed_Improvement_Value = "-", Total_Value = "-", Assessed = "-";
            List<string> Li = new List<string>();
            IWebElement link;
            string mp = "";
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string addr = sname + " " + sttype;
            string address = houseno + addr;

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    if (HttpContext.Current.Session["MobileTax_harrison"] == null)
                    {
                        if (searchType == "titleflex")
                        {
                            gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "MS", "Harrison");
                            if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                            {
                                driver.Quit();
                                return "MultiParcel";
                            }
                            else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                            {
                                HttpContext.Current.Session["Nodata_HarrisonMS"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                            parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                            searchType = "parcel";
                        }

                        if (searchType == "address")
                        {
                            driver.Navigate().GoToUrl("http://www.deltacomputersystems.com/MS/MS24DELTA/plinkquerym.html");
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[5]/td[2]/form/table/tbody/tr[2]/td[2]/input[1]")).SendKeys(houseno);

                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[5]/td[2]/form/table/tbody/tr[2]/td[2]/input[2]")).SendKeys(addr);
                            gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "MS", "Harrison");
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[5]/td[2]/form/input[1]")).SendKeys(Keys.Enter);
                            gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "MS", "Harrison");
                            Thread.Sleep(3000);

                            IWebElement tb = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody"));
                            IList<IWebElement> TR = tb.FindElements(By.TagName("tr"));
                            string Address = houseno + " " + addr;
                            int a = 1;
                            bool value = false;

                            foreach (IWebElement row1 in TR)

                            {
                                if ((row1.Text.Contains(Address.ToUpper().Trim())) && (!row1.Text.Contains("PROPERTY")))

                                {
                                    mp = "/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody/tr[" + a + "]/td[1]/a";

                                    value = true;
                                    //multiparcel
                                    NAME = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody/tr[" + a + "]/td[3]/font")).Text;
                                    ADDRESS = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody/tr[" + a + "]/td[1]/a/font")).Text;
                                    PARCEL = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody/tr[" + a + "]/td[2]/font")).Text;

                                    Li.Add(NAME);
                                    string multi = NAME + "~" + ADDRESS;
                                    gc.insert_date(orderNumber, PARCEL, 87, multi, 1, DateTime.Now);

                                    //break;
                                }
                                a++;
                            }

                            //Multi parcel
                            if (Li.Count > 1)
                            {
                                HttpContext.Current.Session["multiparcel_harrison"] = "Yes";
                                gc.CreatePdf_WOP(orderNumber, "Address MultiParcel", driver, "MS", "Harrison");
                                driver.Quit();
                                return "MultiParcel";
                            }
                            else if (mp != "")
                            {
                                link = driver.FindElement(By.XPath(mp));
                                link.SendKeys(Keys.Enter);
                            }
                            if (!value)
                            {
                                int b = 1;
                                driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/a[1]")).SendKeys(Keys.Enter);
                                IWebElement tb1 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody"));
                                IList<IWebElement> TR1 = tb1.FindElements(By.TagName("tr"));
                                foreach (IWebElement row2 in TR1)
                                {
                                    if ((row2.Text.Contains(Address.ToUpper().Trim())) && (!row2.Text.Contains("PROPERTY")))

                                    {
                                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody/tr[" + b + "]/td[1]/a")).SendKeys(Keys.Enter);

                                        break;
                                    }
                                    b++;
                                }
                            }
                        }

                        else if (searchType == "parcel")
                        {

                            driver.Navigate().GoToUrl("http://www.deltacomputersystems.com/MS/MS24DELTA/plinkquerym.html");
                            gc.CreatePdf(orderNumber, parcelNumber, "Parcel search", driver, "MS", "Harrison");


                            if (parcelNumber.Contains("-"))
                            {
                                parcelNumber = parcelNumber.Replace("-", "").Trim();

                            }
                            if (parcelNumber.Contains("."))
                            {
                                parcelNumber = parcelNumber.Replace(".", "").Trim();
                            }

                            string first = parcelNumber.Substring(0, 5);
                            string second = parcelNumber.Substring(5, 2);
                            string third = parcelNumber.Substring(7, 3);
                            string fourth = parcelNumber.Substring(10, 3);
                            parcelNumber = first + "-" + second + "-" + third + "." + fourth;
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[5]/td[2]/form/table/tbody/tr[3]/td[2]/input")).SendKeys(parcelNumber);
                            gc.CreatePdf(orderNumber, parcelNumber, "Parcel search result", driver, "MS", "Harrison");
                            Thread.Sleep(3000);
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[5]/td[2]/form/input[1]")).SendKeys(Keys.Enter);
                            Thread.Sleep(3000);
                            IWebElement tb = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody"));
                            IList<IWebElement> TR = tb.FindElements(By.TagName("tr"));

                            int a = 1;

                            foreach (IWebElement row1 in TR)
                            {
                                if ((row1.Text.Contains(parcelNumber.Trim())) && (!row1.Text.Contains("PROPERTY")))
                                {
                                    driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody/tr[" + a + "]/td[1]/a")).SendKeys(Keys.Enter);
                                    break;
                                }
                                a++;

                            }
                        }

                        else if (searchType == "ownername")
                        {
                            HttpContext.Current.Session["multiparcel_harrison"] = "Yes";
                            driver.Navigate().GoToUrl("http://www.deltacomputersystems.com/MS/MS24DELTA/plinkquerym.html");
                            gc.CreatePdf_WOP(orderNumber, "Owner Search", driver, "MS", "Harrison");
                            string[] on = ownername.Split(' ');
                            string first = on[0];
                            string second = on[1];
                            ownername = first + " " + second;

                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[5]/td[2]/form/table/tbody/tr[1]/td[2]/input[4]")).SendKeys(ownername);
                            Thread.Sleep(3000);
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[5]/td[2]/form/input[1]")).SendKeys(Keys.Enter);
                            gc.CreatePdf_WOP(orderNumber, "Owner Search result", driver, "MS", "Harrison");
                            Thread.Sleep(3000);
                            IWebElement tb = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody"));
                            IList<IWebElement> TR = tb.FindElements(By.TagName("tr"));

                            int a = 1;

                            foreach (IWebElement row1 in TR)

                            {
                                if ((row1.Text.Contains(ownername.ToUpper().Trim())) && (!row1.Text.Contains("PROPERTY")))

                                {
                                    NAME = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody/tr[" + a + "]/td[1]/a/font")).Text;
                                    PROPERTY = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody/tr[" + a + "]/td[2]/font")).Text;
                                    ADDRESS = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody/tr[" + a + "]/td[3]/font")).Text;
                                    PARCEL = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody/tr[" + a + "]/td[4]/font")).Text;

                                    ADDRESS = PROPERTY + ADDRESS;
                                    string multi = NAME + "~" + ADDRESS;
                                    gc.insert_date(orderNumber, PARCEL, 87, multi, 1, DateTime.Now);
                                }
                                a++;
                            }

                            driver.Quit();
                            return "MultiParcel";
                        }
                        Thread.Sleep(3000);
                    
                        gc.CreatePdf_WOP(orderNumber, "Property and Tax details result", driver, "MS", "Harrison");
                        //Property Details
                        Parcel_No = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody/tr[2]/td/table/tbody/tr[6]/td[2]/font")).Text.Trim();
                        Owner_Name = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody/tr[2]/td/table/tbody/tr[1]/td[2]/font")).Text.Trim();
                        Property_Address = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody/tr[2]/td/table/tbody/tr[7]/td[2]/font")).Text.Trim();
                        Legal_Description = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody/tr[6]/td/table/tbody/tr[1]/td[4]/font")).Text.Trim();
                        string ab = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody/tr[6]/td/table/tbody/tr[2]/td[4]/font")).Text.Trim();
                        string ac = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody/tr[6]/td/table/tbody/tr[3]/td[4]/font")).Text.Trim();
                        string ad = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody/tr[6]/td/table/tbody/tr[4]/td[4]/font")).Text.Trim();
                        Legal_Description = Legal_Description + ab + ac + ad.Trim();
                        Exempt_Code = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody/tr[6]/td/table/tbody/tr[2]/td[2]/font")).Text.Trim();
                        Homestead_Code = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody/tr[6]/td/table/tbody/tr[3]/td[2]/font")).Text.Trim();
                        PPIN = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody/tr[6]/td/table/tbody/tr[4]/td[2]/font")).Text.Trim();
                        Section = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody/tr[6]/td/table/tbody/tr[5]/td[2]/font")).Text.Trim();
                        Township = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody/tr[6]/td/table/tbody/tr[6]/td[2]/font")).Text.Trim();
                        Range = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody/tr[6]/td/table/tbody/tr[7]/td[2]/font")).Text.Trim();

                        string prop = Owner_Name + "~" + Property_Address + "~" + Legal_Description + "~" + Exempt_Code + "~" + Homestead_Code + "~" + PPIN + "~" + Section + "~" + Township + "~" + Range;
                        gc.insert_date(orderNumber, Parcel_No, 88, prop, 1, DateTime.Now);

                        //Assessment details
                        Tax_Year = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[1]/tbody/tr[3]/td[2]/font[1]/b")).Text;
                        Tax_Year = WebDriverTest.After(Tax_Year, "Year ");
                        Assessed_Land_Value = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody/tr[2]/td/table/tbody/tr[2]/td[4]/font")).Text;
                        Assessed_Improvement_Value = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody/tr[2]/td/table/tbody/tr[3]/td[4]/font")).Text;
                        Total_Value = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody/tr[2]/td/table/tbody/tr[4]/td[4]/font")).Text;
                        Assessed = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody/tr[2]/td/table/tbody/tr[5]/td[4]/font")).Text;

                        string assessment = Tax_Year + "~" + Assessed_Land_Value + "~" + Assessed_Improvement_Value + "~" + Total_Value + "~" + Assessed;
                        gc.insert_date(orderNumber, Parcel_No, 89, assessment, 1, DateTime.Now);
                        AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                        //Tax details

                        string YEAR_2017 = "-", TAX_DUE = "-", PAID = "-", BALANCE = "-", Mail_Payments_To = "-", LAST_PAYMENT_DATE = "-";
                        try
                        {
                            Mail_Payments_To = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody/tr[4]/td/table/tbody/tr[6]/td[4]/font/b/p")).Text;
                        }
                        catch { }
                        try
                        {
                            LAST_PAYMENT_DATE = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody/tr[4]/td/table/tbody/tr[7]/td[2]/font")).Text;
                        }
                        catch { }
                        try
                        {
                            if (Mail_Payments_To.Trim() == "")
                            {
                                Mail_Payments_To = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody/tr[4]/td/table/tbody/tr[7]/td[4]/font/b/p")).Text;
                            }
                        }
                        catch { }
                        try
                        {
                            if (LAST_PAYMENT_DATE.Trim() == "")
                            {
                                LAST_PAYMENT_DATE = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody/tr[4]/td/table/tbody/tr[8]/td[2]/font")).Text;
                            }
                        }
                        catch { }

                        if (Mail_Payments_To.Contains("\r\n"))
                        {
                            Mail_Payments_To = Mail_Payments_To.Replace("\r\n", "");
                        }


                        IWebElement TBTax = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody/tr[4]/td/table/tbody"));
                        IList<IWebElement> TRTax = TBTax.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDTax;
                        //  int count = TRTax.Count - 2;
                        // int i = 1;
                        foreach (IWebElement row1 in TRTax)
                        {
                            if (!row1.Text.Contains("YEAR") && !row1.Text.Contains("Mail") && !row1.Text.Contains("LAST PAYMENT DATE") && !row1.Text.Contains("DELINQUENT PRIOR"))
                            {
                                TDTax = row1.FindElements(By.TagName("td"));
                                YEAR_2017 = TDTax[0].Text;
                                TAX_DUE = TDTax[1].Text;
                                PAID = TDTax[2].Text;
                                BALANCE = TDTax[3].Text;
                                string tax = Owner_Name + "~" + Tax_Year + "~" + YEAR_2017 + "~" + TAX_DUE + "~" + PAID + "~" + BALANCE + "~" + Mail_Payments_To + "~" + LAST_PAYMENT_DATE;
                                gc.insert_date(orderNumber, Parcel_No, 90, tax, 1, DateTime.Now);
                            }
                            // i++;
                        }

                        //tax history details
                        string Year = "-", Owner = "-", Total_Tax = "=", PaidYN = "-", Last_Payment_Date = "-";

                        IWebElement TBTax_History = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody/tr[6]/td/table"));
                        IList<IWebElement> TRTax_History = TBTax_History.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDTax_History;
                        foreach (IWebElement row1 in TRTax_History)
                        {

                            if (row1.Text.Contains("LAST PAYMENT DATE"))
                            {
                                TDTax_History = row1.FindElements(By.TagName("td"));
                                Year = TDTax_History[0].Text;
                                Owner = TDTax_History[1].Text;
                                Total_Tax = TDTax_History[2].Text;
                                PaidYN = TDTax_History[3].Text;
                                Last_Payment_Date = WebDriverTest.After(PaidYN, "DATE ");
                                PaidYN = WebDriverTest.Before(PaidYN, "LAST");
                                if (PaidYN.Contains("\r\n"))
                                {
                                    PaidYN = PaidYN.Replace("\r\n", "").Trim();
                                }

                                string tax_history = Year + "~" + Owner + "~" + Total_Tax + "~" + PaidYN + "~" + Last_Payment_Date;
                                gc.insert_date(orderNumber, Parcel_No, 91, tax_history, 1, DateTime.Now);
                            }
                        }
                    }

                    //Mobile tax
                    string ACCOUNT = "", Mobile_tax_Onwer_NAME = "", MAKE = "", strTaxesFees = "";
                    string Account = "-", Receipt = "-", Due_Date = "-", Landroll_PPIN = "-", Court_Code = "-", Court_Lot = "-", Trailer_Make = "-", Number_of_Stories = "-", Number_of_Owners = "-", Width = "-", Length = "-", Model_Year = "-", Color = "-", MobileTax_Owner_Name = "-", MobileTax_Address = "-";
                    string Millage_Tax_Year = "-", Value_Table_Year = "-", Tax_District1 = "-", Tax_District2 = "-", Tax_District3 = "-", Judicial_District = "-", Full_Value = "-", MobileTax_Total_Value = "-", Prorated_Value = "-", County_Tax_Rate = "-", County_Tax_Due = "-", City_Tax_Rate = "-", City_Tax_Due = "-", School_Tax_Due = "-", School_Tax_Rate = "-", Total_Due = "-", TAXES_PAID = "-", Estimated_Balance_Due = "-";
                    driver.Navigate().GoToUrl("http://www.deltacomputersystems.com/MS/MS24DELTA/mhinquirym2.html");
                    gc.CreatePdf_WOP(orderNumber, "Mobile Tax", driver, "MS", "Harrison");
                    if (Parcel_No == "-")
                    {
                        driver.FindElement(By.XPath("//*[@id='HTMPARCEL']")).SendKeys(parcelNumber);
                    }
                    else
                    {
                        driver.FindElement(By.XPath("//*[@id='HTMPARCEL']")).SendKeys(Parcel_No);
                    }

                    driver.FindElement(By.XPath("/html/body/table/tbody/tr[5]/td[2]/form/div/table/tbody/tr[9]/td[2]/input[1]")).SendKeys(Keys.Enter);
                    gc.CreatePdf_WOP(orderNumber, "Mobile Homes Result", driver, "MS", "Harrison");

                    IWebElement TB_mobiletax = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody"));
                    IList<IWebElement> TRmobiletax = TB_mobiletax.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDmobiletax;
                    if (account == "")
                    {

                        foreach (IWebElement row2 in TRmobiletax)
                        {
                            if (!row2.Text.Contains("ACCOUNT") && row2.Text.Contains(Parcel_No))
                            {
                                TDmobiletax = row2.FindElements(By.TagName("td"));
                                ACCOUNT = TDmobiletax[0].Text;
                                Mobile_tax_Onwer_NAME = TDmobiletax[1].Text;
                                MAKE = TDmobiletax[2].Text;

                                HttpContext.Current.Session["MobileTax_harrison"] = "Yes";
                                string mb = ACCOUNT + "~" + Mobile_tax_Onwer_NAME + "~" + MAKE;
                                gc.insert_date(orderNumber, Parcel_No, 92, mb, 1, DateTime.Now);
                                try
                                {
                                    string deli = TDmobiletax[7].Text;
                                    if (deli.Contains("Delinquent"))
                                    {
                                        HttpContext.Current.Session["deliquent_harrison"] = "Yes";
                                    }
                                }
                                catch { }
                            }

                        }
                    }

                    else
                    {
                        int d = 1;
                        string mobile_Prop = "";
                        string mobile_tax = "";
                        foreach (IWebElement row2 in TRmobiletax)
                        {
                            if (row2.Text.Contains(account))
                            {
                                IWebElement element = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody/tr[" + d + "]/td[1]/a/font"));
                                IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                                js.ExecuteScript("arguments[0].click();", element);
                                gc.CreatePdf_WOP(orderNumber, "Mobile Homes Tax and property Result", driver, "MS", "Harrison");
                                Thread.Sleep(3000);

                                //Mobile tax property details

                                IWebElement TB_mobile = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/table[3]/tbody"));
                                IList<IWebElement> TRmobile = TB_mobile.FindElements(By.TagName("tr"));
                                IList<IWebElement> TDmobile;

                                foreach (IWebElement row3 in TRmobile)
                                {
                                    TDmobile = row3.FindElements(By.TagName("td"));
                                    if (row3.Text.Contains("Account"))
                                    {
                                        Account = TDmobile[2].Text.ToString();
                                        Account = WebDriverTest.Before(Account, " Year");
                                    }
                                    if (row3.Text.Contains("Receipt"))
                                    {
                                        Receipt = TDmobile[2].Text.ToString();
                                    }
                                    if (row3.Text.Contains("Due Date"))
                                    {
                                        Due_Date = TDmobile[2].Text.ToString();
                                    }
                                    if (row3.Text.Contains("Landroll PPIN"))
                                    {
                                        Landroll_PPIN = TDmobile[2].Text.ToString();
                                    }
                                    if (row3.Text.Contains("Court Code"))
                                    {
                                        Court_Code = TDmobile[2].Text.ToString();
                                    }
                                    if (row3.Text.Contains("Court Lot"))
                                    {
                                        Court_Lot = TDmobile[2].Text.ToString();
                                    }
                                    if (row3.Text.Contains("Trailer Make"))
                                    {
                                        Trailer_Make = TDmobile[2].Text.ToString();
                                    }
                                    if (row3.Text.Contains("Number of Stories"))
                                    {
                                        Number_of_Stories = TDmobile[2].Text.ToString();
                                    }
                                    if (row3.Text.Contains("Number of Owners"))
                                    {
                                        Number_of_Owners = TDmobile[2].Text.ToString();
                                    }
                                    if (row3.Text.Contains("Width"))
                                    {
                                        Width = TDmobile[2].Text.ToString();
                                    }
                                    if (row3.Text.Contains("Length"))
                                    {
                                        Length = TDmobile[2].Text.ToString();
                                    }
                                    if (row3.Text.Contains("Model Year"))
                                    {
                                        Model_Year = TDmobile[2].Text.ToString();
                                    }
                                    if (row3.Text.Contains("Color"))
                                    {
                                        Color = TDmobile[2].Text.ToString();
                                    }
                                    if (row3.Text.Contains("Name"))
                                    {
                                        MobileTax_Owner_Name = TDmobile[2].Text.ToString();
                                    }
                                    if (row3.Text.Contains("Address 1"))
                                    {
                                        MobileTax_Address = TDmobile[2].Text.ToString();
                                    }
                                    if (row3.Text.Contains("Zip"))
                                    {
                                        string city1 = TDmobile[2].Text.ToString();
                                        MobileTax_Address = MobileTax_Address + city1;
                                    }


                                    mobile_Prop = Account + "~" + Receipt + "~" + Due_Date + "~" + Landroll_PPIN + "~" + Court_Code + "~" + Court_Lot + "~" + Trailer_Make + "~" + Number_of_Stories + "~" + Number_of_Owners + "~" + Width + "~" + Length + "~" + Model_Year + "~" + Color + "~" + MobileTax_Owner_Name + "~" + MobileTax_Address;

                                    //Mobile Home Current Tax & Fees Details
                                    if (row3.Text.Contains("TAXES & FEES"))
                                    {
                                        strTaxesFees = "TAXES & FEES";
                                    }
                                    if (row3.Text.Contains("Millage"))
                                    {
                                        Millage_Tax_Year = TDmobile[2].Text.ToString();
                                    }
                                    if (row3.Text.Contains("Value Table Year"))
                                    {
                                        Value_Table_Year = TDmobile[2].Text.ToString();
                                    }
                                    if (row3.Text.Contains("Tax District 1"))
                                    {
                                        Tax_District1 = TDmobile[2].Text.ToString();
                                    }
                                    if (row3.Text.Contains("Tax District 2"))
                                    {
                                        Tax_District2 = TDmobile[2].Text.ToString();
                                    }
                                    if (row3.Text.Contains("Tax District 3"))
                                    {
                                        Tax_District3 = TDmobile[2].Text.ToString();
                                    }
                                    if (row3.Text.Contains("Judicial District"))
                                    {
                                        Judicial_District = TDmobile[2].Text.ToString();
                                    }
                                    if (row3.Text.Contains("Full Value"))
                                    {
                                        Full_Value = TDmobile[2].Text.ToString();
                                    }
                                    if (row3.Text.Contains("Total Value"))
                                    {
                                        MobileTax_Total_Value = TDmobile[2].Text.ToString();
                                    }
                                    if (row3.Text.Contains("Prorated Value"))
                                    {
                                        Prorated_Value = TDmobile[2].Text.ToString();
                                    }
                                    if (row3.Text.Contains("County"))
                                    {
                                        County_Tax_Rate = TDmobile[2].Text.ToString();
                                        County_Tax_Due = TDmobile[3].Text.ToString();
                                    }

                                    if (row3.Text.Contains("City") && strTaxesFees == "TAXES & FEES")
                                    {
                                        City_Tax_Rate = TDmobile[2].Text.ToString();
                                        City_Tax_Due = TDmobile[3].Text.ToString();
                                    }

                                    if (row3.Text.Contains("School"))
                                    {
                                        School_Tax_Due = TDmobile[2].Text.ToString();
                                        School_Tax_Rate = TDmobile[3].Text.ToString();
                                    }

                                    if (row3.Text.Contains("Total Due"))
                                    {
                                        Total_Due = TDmobile[3].Text.ToString();
                                    }
                                    if (row3.Text.Contains("Taxes Paid"))
                                    {
                                        TAXES_PAID = TDmobile[3].Text.ToString();
                                    }
                                    if (row3.Text.Contains("Estimated Balance Due"))
                                    {
                                        Estimated_Balance_Due = TDmobile[3].Text.ToString();
                                    }

                                    mobile_tax = Millage_Tax_Year + "~" + Value_Table_Year + "~" + Tax_District1 + "~" + Tax_District2 + "~" + Tax_District3 + "~" + Judicial_District + "~" + Full_Value + "~" + MobileTax_Total_Value + "~" + Prorated_Value + "~" + County_Tax_Rate + "~" + County_Tax_Due + "~" + City_Tax_Rate + "~" + City_Tax_Due + "~" + School_Tax_Due + "~" + School_Tax_Rate + "~" + Total_Due + "~" + TAXES_PAID + "~" + Estimated_Balance_Due;
                                }

                                gc.insert_date(orderNumber, parcelNumber, 94, mobile_Prop, 1, DateTime.Now);
                                gc.insert_date(orderNumber, parcelNumber, 95, mobile_tax, 1, DateTime.Now);
                                break;
                            }
                            d++;
                        }
                    }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "MS", "Harrison", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "MS", "Harrison");
                    return "Data Inserted Successfully";
                }

                catch (NoSuchElementException ex1)
                {
                    driver.Quit();
                    throw ex1;
                }
            }
        }
    }
}