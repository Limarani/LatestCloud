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
namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_CODouglas
    {

        IWebDriver driver;


        DBconnection db = new DBconnection();

        GlobalClass gc = new GlobalClass();

        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        private object driverIE;
        private int multicount;
        private string strMultiAddress;
        private string b;

        public string FTP_Douglas(string houseno, string sname, string ownername, string assessment_id, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass gc = new GlobalClass();
            string Parcel_ID = "", Account_Number = "", Owner_Name = "", Property_Address = "", Mailing_Address = "", Property_Type = "", Year_Built = "", Legal_Description = "";
            string IstInstallment = "", IIndInstallment = "";
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string Account = "", Owner = "", Address = "";

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string address = houseno + " " + sname;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new PhantomJSDriver();
            var option = new ChromeOptions();
            option.AddArgument("No-Sandbox");
            using (driver = new ChromeDriver(option))
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    if (searchType == "titleflex")
                    {

                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "CO", "Douglas");

                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Douglas_Zero"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    driver.Navigate().GoToUrl("https://www.douglas.co.us/assessor/#/");
                    Thread.Sleep(4000);
                    if (searchType == "address")
                    {
                        //*[@id="#SearchBar"]/div[1]/div[1]/button

                        IWebElement ISpan112 = driver.FindElement(By.XPath("//*[@id='#SearchBar']/div[1]/div[1]/button"));
                        IJavaScriptExecutor js112 = driver as IJavaScriptExecutor;
                        js112.ExecuteScript("arguments[0].click();", ISpan112);
                        //*[@id="#SearchBar"]/div[1]/div[1]/ul/li[2]/a
                        IWebElement ISpan12 = driver.FindElement(By.XPath("//*[@id='#SearchBar']/div[1]/div[1]/ul/li[2]/a"));
                        IJavaScriptExecutor js12 = driver as IJavaScriptExecutor;
                        js12.ExecuteScript("arguments[0].click();", ISpan12);

                        //driver.FindElement(By.XPath("//*[@id='#SearchBar']/div[1]/div[1]/button")).SendKeys(Keys.Enter);
                        //Thread.Sleep(3000);
                        //driver.FindElement(By.XPath("//*[@id='#SearchBar']/div[1]/div[1]/ul/li[2]")).Click();
                        //Thread.Sleep(1000);

                        driver.FindElement(By.XPath("//*[@id='SearchBar']/input")).Clear();
                        driver.FindElement(By.XPath("//*[@id='SearchBar']/input")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "CO", "Douglas");

                        IWebElement ISpan13 = driver.FindElement(By.XPath("//*[@id='#SearchBar']/div[1]/div[3]/button[2]"));
                        IJavaScriptExecutor js13 = driver as IJavaScriptExecutor;
                        js13.ExecuteScript("arguments[0].click();", ISpan13);
                        Thread.Sleep(10000);
                        //driver.FindElement(By.XPath("//*[@id='#SearchBar']/div[1]/div[3]/button[2]")).Click();
                        //    Thread.Sleep(8000);
                        try
                        {
                            string Nodata1 = driver.FindElement(By.XPath("//*[@id='assessor-search']/div/wp/app-home/div[1]/div")).Text;
                            if (Nodata1.Contains("There were no results"))
                            {
                                HttpContext.Current.Session["Douglas_Zero"] = "Zero";
                                return "No Data Found";
                            }
                        }
                        catch { }
                        gc.CreatePdf_WOP(orderNumber, "Address search result1", driver, "CO", "Douglas");

                        try
                        {

                            string multi = driver.FindElement(By.XPath("//*[@id='assessor-search']/div/wp/home/div[2]/div[3]")).Text;
                            string multiCount = gc.Between(multi, "\r\n", " results");


                            if (multiCount != "1")
                            {
                                //multi parcel                     
                                IWebElement tbmulti2 = driver.FindElement(By.XPath("//*[@id='assessor-search']/div/wp/home/div[1]/div[1]"));
                                IList<IWebElement> TRmulti2 = tbmulti2.FindElements(By.TagName("a"));
                                int maxCheck = 0;
                                IList<IWebElement> TDmulti2;
                                string[] parcel = new string[3]; int p = 0;
                                foreach (IWebElement row in TRmulti2)
                                {
                                    if (maxCheck <= 25)
                                    {
                                        if (row.Text.Contains(address.ToUpper()))
                                        {
                                            TDmulti2 = row.FindElements(By.TagName("div"));
                                            if (TDmulti2.Count != 0)
                                            {
                                                if (p == 0)
                                                {
                                                    parcel[0] = TDmulti2[0].Text;

                                                }
                                                if (p == 1)
                                                {
                                                    parcel[1] = TDmulti2[0].Text;

                                                }
                                                if (p == 2)
                                                {
                                                    parcel[2] = TDmulti2[0].Text;

                                                }
                                                p++;
                                                string multi1 = TDmulti2[1].Text + "~" + TDmulti2[2].Text;
                                                gc.insert_date(orderNumber, TDmulti2[0].Text, 283, multi1, 1, DateTime.Now);
                                                maxCheck++;
                                            }
                                        }

                                    }
                                }
                                if (maxCheck == 0)
                                {
                                    if (Convert.ToInt32(multiCount) > 25)
                                    {
                                        HttpContext.Current.Session["multiParcel_Douglas_Count"] = "Maximum";
                                        return "Maximum";
                                    }
                                }


                                if (maxCheck == 1)
                                {
                                    IWebElement element5 = driver.FindElement(By.XPath("//*[@id='assessor-search']/div/wp/home/div[1]/div[1]/a[1]"));
                                    IJavaScriptExecutor js5 = driver as IJavaScriptExecutor;
                                    js5.ExecuteScript("arguments[0].click();", element5);
                                    Thread.Sleep(3000);
                                }

                                try
                                {
                                    driver.FindElement(By.XPath("//*[@id='assessor-search']/div/wp/home/div[2]/div[1]/button")).Click();
                                    driver.FindElement(By.XPath("//*[@id='assessor-search']/div/wp/home/div[2]/div[1]/ul/li[3]/a")).Click();
                                }
                                catch { }
                            }
                        }
                        catch { }

                        try
                        {
                            if (driver.FindElement(By.XPath("//*[@id='assessor-search']/div/wp/home/div[2]/div[3]")).Displayed)
                            {
                                for (int i = 1; i <= 25; i++)
                                {
                                    try
                                    {
                                        Account = driver.FindElement(By.XPath("//*[@id='assessor-search']/div/wp/home/div[1]/div[1]/a[" + i + "]/div[1]")).Text;
                                        Owner = driver.FindElement(By.XPath("//*[@id='assessor-search']/div/wp/home/div[1]/div[1]/a[" + i + "]/div[2]")).Text;
                                        Address = driver.FindElement(By.XPath("//*[@id='assessor-search']/div/wp/home/div[1]/div[1]/a[" + i + "]/div[3]")).Text;
                                        string multi_parcel = Owner + "~" + Address;
                                        gc.insert_date(orderNumber, Account, 231, multi_parcel, 1, DateTime.Now);
                                    }
                                    catch { }
                                }
                                HttpContext.Current.Session["multiParcel_Douglas"] = "Yes";

                                driver.Quit();
                                //gc.mergpdf(orderNumber, "CO", "Douglas");
                                return "MultiParcel";

                            }
                        }
                        catch { }


                    }
                    else if (searchType == "parcel")
                    {
                        driver.FindElement(By.XPath("//*[@id='#SearchBar']/div[1]/div[1]/button")).Click();
                        Thread.Sleep(3000);
                        driver.FindElement(By.XPath("//*[@id='#SearchBar']/div[1]/div[1]/ul/li[5]")).Click();
                        Thread.Sleep(1000);

                        if (parcelNumber.Contains("-"))
                        {
                            parcelNumber = parcelNumber.Replace("-", "");
                        }
                        driver.FindElement(By.XPath("//*[@id='SearchBar']/input")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search", driver, "CO", "Douglas");
                        driver.FindElement(By.XPath("//*[@id='#SearchBar']/div[1]/div[3]/button[2]")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search result", driver, "CO", "Douglas");
                    }
                    else if (searchType == "block")
                    {
                        driver.FindElement(By.XPath("//*[@id='#SearchBar']/div[1]/div[1]/button")).Click();
                        Thread.Sleep(3000);
                        driver.FindElement(By.XPath("//*[@id='#SearchBar']/div[1]/div[1]/ul/li[4]")).Click();
                        Thread.Sleep(1000);
                        driver.FindElement(By.XPath("//*[@id='SearchBar']/input")).SendKeys(assessment_id);
                        gc.CreatePdf_WOP(orderNumber, "Account search", driver, "CO", "Douglas");
                        driver.FindElement(By.XPath("//*[@id='#SearchBar']/div[1]/div[3]/button[2]")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Account search result", driver, "CO", "Douglas");
                    }
                    else if (searchType == "ownername")
                    {
                        driver.FindElement(By.XPath("//*[@id='#SearchBar']/div[1]/div[1]/button")).Click();
                        Thread.Sleep(3000);
                        driver.FindElement(By.XPath("//*[@id='#SearchBar']/div[1]/div[1]/ul/li[3]")).Click();
                        Thread.Sleep(1000);
                        driver.FindElement(By.XPath("//*[@id='SearchBar']/input")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "Owner search", driver, "CO", "Douglas");
                        driver.FindElement(By.XPath("//*[@id='#SearchBar']/div[1]/div[3]/button[2]")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Owner search result", driver, "CO", "Douglas");

                        try
                        {

                            string multi = driver.FindElement(By.XPath("//*[@id='assessor-search']/div/wp/home/div[2]/div[3]")).Text;
                            string multiCount = gc.Between(multi, "\r\n", " results");
                            if (Convert.ToInt32(multiCount) > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Douglas_Count"] = "Maximum";
                                return "Maximum";
                            }


                            try
                            {
                                driver.FindElement(By.XPath("//*[@id='assessor-search']/div/wp/home/div[2]/div[1]/button")).Click();
                                driver.FindElement(By.XPath("//*[@id='assessor-search']/div/wp/home/div[2]/div[1]/ul/li[3]/a")).Click();
                            }
                            catch { }
                        }
                        catch { }


                        try
                        {
                            if (driver.FindElement(By.XPath("//*[@id='assessor-search']/div/wp/home/div[2]/div[3]")).Displayed)
                            {
                                for (int i = 1; i <= 25; i++)
                                {
                                    try
                                    {
                                        Account = driver.FindElement(By.XPath("//*[@id='assessor-search']/div/wp/home/div[1]/div[1]/a[" + i + "]/div[1]")).Text;
                                        Owner = driver.FindElement(By.XPath("//*[@id='assessor-search']/div/wp/home/div[1]/div[1]/a[" + i + "]/div[2]")).Text;
                                        Address = driver.FindElement(By.XPath("//*[@id='assessor-search']/div/wp/home/div[1]/div[1]/a[" + i + "]/div[3]")).Text;
                                        string multi_parcel = Owner + "~" + Address;
                                        gc.insert_date(orderNumber, Account, 231, multi_parcel, 1, DateTime.Now);
                                    }
                                    catch { }
                                }
                                HttpContext.Current.Session["multiParcel_Douglas"] = "Yes";

                                driver.Quit();
                                //gc.mergpdf(orderNumber, "CO", "Douglas");
                                return "MultiParcel";

                            }
                        }
                        catch { }
                    }

                    //property details
                    Owner_Name = driver.FindElement(By.XPath("/html/body/app-root/div/div[3]/app-details/div[2]/div[1]/div[1]/overview/div[1]/div/div[1]/div/div[2]")).Text;

                    Parcel_ID = driver.FindElement(By.XPath("//*[@id='StickyInfo']/span[2]")).Text;
                    Parcel_ID = WebDriverTest.After(Parcel_ID, "Parcel #:").Trim();
                    //  Parcel_ID = Parcel_ID.Replace("-", "");
                    Account_Number = driver.FindElement(By.XPath("//*[@id='StickyInfo']/span[1]")).Text;
                    Account_Number = WebDriverTest.After(Account_Number, "Account #:").Trim();
                    //Owner_Name = driver.FindElement(By.XPath("/html/body/app/div/div[3]/parceldetails/div[2]/div[1]/div[1]/overview/div[1]/div/div[1]/div/div[2]")).Text;
                    ///html/body/app/div/div[3]/parceldetails/div[2]/div[1]/div[1]/overview/div[1]/div/div[1]/h3 
                    Property_Address = driver.FindElement(By.XPath("/html/body/app-root/div/div[3]/app-details/div[2]/div[1]/div[1]/overview/div[1]/div/div[1]/div/div[3]/span")).Text;
                    Property_Address = Property_Address.Replace("\r\n", " ");
                    try
                    {
                        Mailing_Address = driver.FindElement(By.XPath("/html/body/app/div/div[3]/parceldetails/div[2]/div[1]/div[1]/overview/div[1]/div/div[1]/div/div[3]/span")).Text;
                        Mailing_Address = Mailing_Address.Replace("\r\n", " ");
                    }
                    catch
                    { }
                    IWebElement ISpan1 = driver.FindElement(By.XPath("/html/body/app-root/div/div[3]/app-details/div[2]/div[3]/div[1]/span[1]"));
                    IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                    js1.ExecuteScript("arguments[0].click();", ISpan1);

                    ByVisibleElement(driver.FindElement(By.XPath("//*[@id='SummaryAccountInfo']/span")));

                    gc.CreatePdf(orderNumber, Parcel_ID, "assessment1", driver, "CO", "Douglas");

                    ByVisibleElement(driver.FindElement(By.XPath("//*[@id='ValuationInfo']/span")));

                    gc.CreatePdf(orderNumber, Parcel_ID, "Expand All", driver, "CO", "Douglas");

                    ByVisibleElement(driver.FindElement(By.XPath("//*[@id='SalesAndTrans']/span")));

                    gc.CreatePdf(orderNumber, Parcel_ID, "assement2", driver, "CO", "Douglas");
                    Property_Type = driver.FindElement(By.XPath("//*[@id='SummaryAccountInfo']/div/account-summary/div[1]/div[3]/div[2]")).Text;
                    try
                    {
                        Year_Built = driver.FindElement(By.XPath("/html/body/app/div/div[3]/parceldetails/div[2]/div[3]/div[2]/div[4]/div/building-details/div/div/div[2]/div[2]/div[1]/div[1]/div/div[2]/div[2]")).Text;
                    }
                    catch { }
                    Legal_Description = driver.FindElement(By.XPath("//*[@id='SummaryAccountInfo']/div/account-summary/div[2]/div[7]")).Text;
                    string property_details = Account_Number + "~" + Owner_Name + "~" + Property_Address + "~" + Mailing_Address + "~" + Property_Type + "~" + Year_Built + "~" + Legal_Description;
                    gc.insert_date(orderNumber, Parcel_ID, 223, property_details, 1, DateTime.Now);

                    //Assessment details
                    string class_code = "", description = "", actual_value = "", assessed_value;
                    string year = "", Tax_rate = "", EstTax_amount;
                    int iRowsCount = driver.FindElements(By.XPath("//*[@id='ValuationInfo']/div/valuation/div[2]/table/tbody")).Count;
                    for (int i = 2; i <= iRowsCount; i++)
                    {
                        //*[@id="ValuationInfo"]/div/valuation/div[2]/table/tbody[2]/tr[1]/td[1]
                        //*[@id="ValuationInfo"]/div/valuation/div[2]/table/tbody[3]/tr[1]/td[1]
                        IWebElement ISpan113 = driver.FindElement(By.XPath("//*[@id='ValuationInfo']/div/valuation/div[2]/table/tbody[" + i + "]/tr[1]/td[1]"));
                        IJavaScriptExecutor js113 = driver as IJavaScriptExecutor;
                        js113.ExecuteScript("arguments[0].click();", ISpan113);
                        Thread.Sleep(1000);
                        year = driver.FindElement(By.XPath("//*[@id='ValuationInfo']/div/valuation/div[2]/table/tbody[" + i + "]/tr[1]/td[1] ")).Text;
                        string actual_value1 = driver.FindElement(By.XPath("//*[@id='ValuationInfo']/div/valuation/div[2]/table/tbody[" + i + "]/tr[1]/td[2] ")).Text;
                        string assessed_value1 = driver.FindElement(By.XPath("//*[@id='ValuationInfo']/div/valuation/div[2]/table/tbody[" + i + "]/tr[1]/td[3] ")).Text;
                        Tax_rate = driver.FindElement(By.XPath("//*[@id='ValuationInfo']/div/valuation/div[2]/table/tbody[" + i + "]/tr[1]/td[4] ")).Text;
                        EstTax_amount = driver.FindElement(By.XPath("//*[@id='ValuationInfo']/div/valuation/div[2]/table/tbody[" + i + "]/tr[1]/td[5] ")).Text;
                        if (i == 2)
                        {

                            ByVisibleElement(driver.FindElement(By.XPath("//*[@id='ValuationInfo']/div/valuation/div[2]/table/tbody[" + i + "]/tr[1]/td[1] ")));
                            gc.CreatePdf(orderNumber, Parcel_ID, "current year", driver, "CO", "Douglas");
                        }
                        if (i == 3)
                        {

                            ByVisibleElement(driver.FindElement(By.XPath("//*[@id='ValuationInfo']/div/valuation/div[2]/table/tbody[" + i + "]/tr[1]/td[1] ")));
                            gc.CreatePdf(orderNumber, Parcel_ID, "prior year", driver, "CO", "Douglas");
                        }
                        if (i == 4)
                        {

                            ByVisibleElement(driver.FindElement(By.XPath("//*[@id='ValuationInfo']/div/valuation/div[2]/table/tbody[" + i + "]/tr[1]/td[1] ")));
                            gc.CreatePdf(orderNumber, Parcel_ID, "2 year before", driver, "CO", "Douglas");
                        }
                        for (int j = 1; j <= 2; j++)
                        {
                            for (int k = 2; k <= 4; k++)
                            {
                                try
                                {
                                    class_code = driver.FindElement(By.XPath("//*[@id='ValuationInfo']/div/valuation/div[2]/table/tbody[" + i + "]/tr[3]/td/div/div[" + j + "]/div[" + k + "]/div[1]")).Text;
                                    description = driver.FindElement(By.XPath("//*[@id='ValuationInfo']/div/valuation/div[2]/table/tbody[" + i + "]/tr[3]/td/div/div[" + j + "]/div[" + k + "]/div[2]")).Text;
                                    actual_value = driver.FindElement(By.XPath("//*[@id='ValuationInfo']/div/valuation/div[2]/table/tbody[" + i + "]/tr[3]/td/div/div[" + j + "]/div[" + k + "]/div[3]")).Text;
                                    assessed_value = driver.FindElement(By.XPath("//*[@id='ValuationInfo']/div/valuation/div[2]/table/tbody[" + i + "]/tr[3]/td/div/div[" + j + "]/div[" + k + "]/div[4]")).Text;
                                    //     Year~Class_code~Description~actual_value~assessed_value~Tax_rate~EstTax_amount
                                    string assessment_details = year + "~" + class_code + "~" + description + "~" + actual_value + "~" + assessed_value + "~" + "-" + "~" + "-";
                                    gc.insert_date(orderNumber, Parcel_ID, 224, assessment_details, 1, DateTime.Now);
                                }
                                catch { }

                            }
                        }
                        string assessment_details1 = year + "~" + "-" + "~" + "Grand Total :" + "~" + actual_value1 + "~" + assessed_value1 + "~" + Tax_rate + "~" + EstTax_amount;
                        gc.insert_date(orderNumber, Parcel_ID, 224, assessment_details1, 1, DateTime.Now);
                    }
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    //Tax details
                    driver.Navigate().GoToUrl("http://apps.douglas.co.us/treasurer/web/login.jsp");
                    Thread.Sleep(4000);

                    //Tax information
                    driver.FindElement(By.XPath("//*[@id='middle_left']/form/input[1]")).Click();
                    Thread.Sleep(3000);
                    driver.FindElement(By.Id("TaxAccountID")).SendKeys(Account_Number);
                    gc.CreatePdf(orderNumber, Parcel_ID, "Tax search", driver, "CO", "Douglas");
                    driver.FindElement(By.XPath("//*[@id='middle']/form/table[3]/tbody/tr/td[1]/input")).SendKeys(Keys.Enter);
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, Parcel_ID, "Tax search result", driver, "CO", "Douglas");
                    driver.FindElement(By.XPath("//*[@id='searchResultsTable']/tbody/tr/td[1]/strong/a")).Click();
                    Thread.Sleep(3000);


                    string date = "";
                    string fulltext = driver.FindElement(By.XPath("//*[@id='totals']/table/tbody")).Text.Trim().Replace("\r\n", "");
                    if (!fulltext.Contains("Interest Due"))
                    {

                        IWebElement Idate1 = driver.FindElement(By.Id("paymentDate"));
                        date = Idate1.GetAttribute("value");


                    }
                    else
                    {

                        IWebElement dt = driver.FindElement(By.XPath("//*[@id='paymentDate']"));
                        date = dt.GetAttribute("value");
                        DateTime G_Date = Convert.ToDateTime(date);
                        string dateChecking = DateTime.Now.ToString("MM") + "/15/" + DateTime.Now.ToString("yyyy");
                        if (G_Date < Convert.ToDateTime(dateChecking))
                        {
                            //end of the month
                            date = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")))).ToString("MM/dd/yyyy");

                        }

                        else if (G_Date > Convert.ToDateTime(dateChecking))
                        {
                            // nextEndOfMonth 
                            if ((Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))) < 12))
                            {
                                date = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM")) + 1), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")) + 1)).ToString("MM/dd/yyyy");
                            }
                            else
                            {
                                int nxtYr = Convert.ToInt16(DateTime.Now.ToString("yyyy")) + 1;
                                date = new DateTime(nxtYr, 1, DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), 1)).ToString("MM/dd/yyyy");
                            }
                        }

                        Thread.Sleep(2000);
                        dt.Clear();



                        driver.FindElement(By.Id("paymentDate")).SendKeys(date);
                        Thread.Sleep(3000);

                    }

                    driver.FindElement(By.Id("paymentTypeFirst")).Click();
                    Thread.Sleep(4000);

                    ByVisibleElement(driver.FindElement(By.XPath("//*[@id='taxAccountSummary']/table")));
                    gc.CreatePdf(orderNumber, Parcel_ID, "Tax info result first", driver, "CO", "Douglas");
                    IstInstallment = driver.FindElement(By.XPath("//*[@id='inquiryForm']/table/tbody/tr[2]/td[2]/label[1]")).Text;
                    IWebElement multitableElement4 = driver.FindElement(By.XPath("//*[@id='totals']/table/tbody"));
                    IList<IWebElement> multitableRow4 = multitableElement4.FindElements(By.TagName("tr"));
                    IList<IWebElement> multirowTD4;
                    foreach (IWebElement row in multitableRow4)
                    {
                        multirowTD4 = row.FindElements(By.TagName("td"));
                        string tax_infodeli = Account_Number + "~" + IstInstallment + "~" + multirowTD4[0].Text.Trim() + "~" + multirowTD4[1].Text.Trim();
                        gc.insert_date(orderNumber, Parcel_ID, 225, tax_infodeli, 1, DateTime.Now);

                    }

                    try
                    {
                        driver.FindElement(By.Id("paymentTypeSecond")).Click();
                        Thread.Sleep(3000);
                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='taxAccountSummary']/table")));
                        gc.CreatePdf(orderNumber, Parcel_ID, "Tax info result second", driver, "CO", "Douglas");

                    }
                    catch
                    {
                        driver.FindElement(By.Id("paymentTypeFull")).Click();
                        Thread.Sleep(3000);
                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='taxAccountSummary']/table")));
                        gc.CreatePdf(orderNumber, Parcel_ID, "Tax info result full", driver, "CO", "Douglas");
                    }
                    IIndInstallment = driver.FindElement(By.XPath("//*[@id='inquiryForm']/table/tbody/tr[2]/td[2]/label[2]")).Text;
                    IWebElement multitableElement3 = driver.FindElement(By.XPath("//*[@id='totals']/table/tbody"));
                    IList<IWebElement> multitableRow3 = multitableElement3.FindElements(By.TagName("tr"));
                    IList<IWebElement> multirowTD3;
                    foreach (IWebElement row in multitableRow3)
                    {
                        multirowTD3 = row.FindElements(By.TagName("td"));

                        string tax_infodeli1 = Account_Number + "~" + IIndInstallment + "~" + multirowTD3[0].Text.Trim() + "~" + multirowTD3[1].Text.Trim();
                        gc.insert_date(orderNumber, Parcel_ID, 225, tax_infodeli1, 1, DateTime.Now);
                    }
                    string tax_infodeli2 = Account_Number + "~" + "Good Through Date :" + "~" + date + "~" + "-";
                    gc.insert_date(orderNumber, Parcel_ID, 225, tax_infodeli2, 1, DateTime.Now);

                    //Tax Distribution Table:

                    driver.FindElement(By.XPath("//*[@id='accountLinks']/a[2]")).Click();
                    Thread.Sleep(4000);

                    ByVisibleElement(driver.FindElement(By.XPath(" //*[@id='middle']/table[1]/tbody/tr/td[1]")));
                    gc.CreatePdf(orderNumber, Parcel_ID, "Account value", driver, "CO", "Douglas");
                    IWebElement multitableElement1 = driver.FindElement(By.XPath("//*[@id='middle']/table[3]/tbody"));
                    IList<IWebElement> multitableRow1 = multitableElement1.FindElements(By.TagName("tr"));
                    IList<IWebElement> multirowTD1;
                    foreach (IWebElement row in multitableRow1)
                    {
                        if (!row.Text.Contains("Authority"))
                        {
                            multirowTD1 = row.FindElements(By.TagName("td"));
                            if (multirowTD1.Count == 4)
                            {
                                string tax_distri = multirowTD1[0].Text.Trim() + "~" + multirowTD1[1].Text.Trim() + "~" + multirowTD1[2].Text.Trim() + "~" + multirowTD1[3].Text.Trim();
                                gc.insert_date(orderNumber, Parcel_ID, 226, tax_distri, 1, DateTime.Now);
                            }
                        }
                    }

                    //Due Date Details Table:


                    driver.FindElement(By.XPath("//*[@id='accountLinks']/a[3]")).Click();
                    Thread.Sleep(4000);
                    gc.CreatePdf(orderNumber, Parcel_ID, "Transaction detail1", driver, "CO", "Douglas");
                    try
                    {
                        ByVisibleElement(driver.FindElement(By.XPath(" //*[@id='middle']/table[2]/tbody/tr[11]")));
                    }
                    catch { }
                    gc.CreatePdf(orderNumber, Parcel_ID, "Transaction detail", driver, "CO", "Douglas");
                    IWebElement multitableElement = driver.FindElement(By.XPath("//*[@id='middle']/table[1]/tbody"));
                    IList<IWebElement> multitableRow = multitableElement.FindElements(By.TagName("tr"));
                    IList<IWebElement> multirowTD;
                    foreach (IWebElement row in multitableRow)
                    {
                        multirowTD = row.FindElements(By.TagName("td"));

                        string tax_duedate = multirowTD[0].Text.Trim() + "~" + multirowTD[1].Text.Trim() + "~" + multirowTD[2].Text.Trim() + "~" + multirowTD[3].Text.Trim() + "~" + multirowTD[4].Text.Trim() + "~" + multirowTD[5].Text.Trim() + "~" + multirowTD[6].Text.Trim() + "~" + multirowTD[7].Text.Trim();
                        gc.insert_date(orderNumber, Parcel_ID, 227, tax_duedate, 1, DateTime.Now);
                    }

                    //Tax History Details Table
                    IWebElement multitableElement2 = driver.FindElement(By.XPath("//*[@id='middle']/table[2]/tbody"));
                    IList<IWebElement> multitableRow2 = multitableElement2.FindElements(By.TagName("tr"));
                    IList<IWebElement> multirowTD2;
                    foreach (IWebElement row in multitableRow2)
                    {
                        multirowTD2 = row.FindElements(By.TagName("td"));
                        string tax_duedate = multirowTD2[0].Text.Trim() + "~" + multirowTD2[1].Text.Trim() + "~" + multirowTD2[2].Text.Trim() + "~" + multirowTD2[3].Text.Trim() + "~" + multirowTD2[4].Text.Trim();
                        gc.insert_date(orderNumber, Parcel_ID, 228, tax_duedate, 1, DateTime.Now);

                    }
                    //*[@id="plugin"]


                    //webtax
                    //   *[@id = "left"] / em / div[1] / a[1]
                    // IWebElement ISpan = driver.FindElement(By.LinkText("Web Tax Notice"));
                    //IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                    //js.ExecuteScript("arguments[0].click();", ISpan);
                    ////download taxbill


                    //try
                    //{
                    //    IWebElement ISpan11 = driver.FindElement(By.LinkText("Web Tax Notice"));
                    //    IJavaScriptExecutor js11 = driver as IJavaScriptExecutor;
                    //    js11.ExecuteScript("arguments[0].click();", ISpan11);

                    //    gc.CreatePdf(orderNumber, Parcel_ID, "Tax BillScreen", driver, "CO", "Douglas");

                    //    ByVisibleElement(driver.FindElement(By.XPath("//*[@id='plugin']")));
                    //    gc.CreatePdf(orderNumber, Parcel_ID, "Tax BillScreen1", driver, "CO", "Douglas");
                    //}
                    //catch { }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "CO", "Douglas", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "CO", "Douglas");
                    return "Data Inserted Successfully";
                }
                catch (Exception ex)
                {
                    driver.Quit();
                    throw ex;
                }
        }

        public void ByVisibleElement(IWebElement Element)
        {


            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

            //Launch the application		
            // driver.Navigate().GoToUrl("http://gis.hcpafl.org/PropertySearch/#/nav/Basic%20Search");

            //Find element by link text and store in variable "Element"        		
            // IWebElement Element = driver.FindElement(By.LinkText("Disclaimer"));

            //This will scroll the page till the element is found		
            js.ExecuteScript("arguments[0].scrollIntoView();", Element);
        }

    }
}

