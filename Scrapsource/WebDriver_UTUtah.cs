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
using OpenQA.Selenium.Firefox;
using System.Diagnostics;
using OpenQA.Selenium.Remote;

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_UTUtah
    {
        string Outparcelno = "", outputPath = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        private TimeSpan timeOutInSeconds;
        MySqlParameter[] mParam;

        public string FTP_Utah(string houseno, string direction, string sname, string sttype, string parcelNumber, string searchType, string orderNumber, string ownername, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            // driver = new PhantomJSDriver();
            //driver = new ChromeDriver();
            IJavaScriptExecutor js = driver as IJavaScriptExecutor;
            string pathurl = "", Owner_Name = "";
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            List<string> taxurllist = new List<string>();
            using (driver = new ChromeDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    if (searchType == "titleflex")
                    {
                        string address = houseno + " " + direction + " " + sname + " " + sttype;
                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "UT", "Utah");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Utah_Zero"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    driver.Navigate().GoToUrl("http://www.utahcountyonline.org/LandRecords/Index.asp");
                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("http://www.utahcountyonline.org/LandRecords/AddressSearchForm.asp");
                        Thread.Sleep(2000);
                        //driver.FindElement(By.XPath("/html/body/div/div[6]/div[2]/table/tbody/tr/td/table/tbody/tr/td[1]/ul[1]/li[2]/a")).SendKeys(Keys.Enter);
                        //gc.CreatePdf_WOP_Chrome(orderNumber, "Address Search",driver, "UT", "Utah");
                        driver.FindElement(By.Id("av_house")).SendKeys(houseno);
                        driver.FindElement(By.Id("av_dir")).SendKeys(direction);
                        driver.FindElement(By.Id("av_street")).SendKeys(sname);
                        driver.FindElement(By.Id("street_type")).SendKeys(sttype);
                        //IWebElement rdio = driver.FindElement(By.XPath("//*[@id='form1']/table/tbody/tr/td/p[2]/label[2]/label/input"));
                        //js.ExecuteScript("arguments[0].click();", rdio);
                        driver.FindElement(By.XPath("//*[@id='form1']/table/tbody/tr/td/p[3]/input")).SendKeys(Keys.Enter);
                        gc.CreatePdf_WOP_Chrome(orderNumber, "Address Search result", driver, "UT", "Utah");
                        //multi parcel
                        IWebElement multitableElement = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table[1]/tbody"));
                        IList<IWebElement> multitableRow = multitableElement.FindElements(By.TagName("tr"));
                        if (multitableRow.Count > 2)
                        {
                            IList<IWebElement> multirowTD;
                            foreach (IWebElement row in multitableRow)
                            {
                                multirowTD = row.FindElements(By.TagName("td"));
                                if (multirowTD.Count != 0 && !row.Text.Contains("Serial Number"))
                                {
                                    string multi = multirowTD[1].Text.Trim() + "~" + "-";
                                    gc.insert_date(orderNumber, multirowTD[0].Text.Trim(), 119, multi, 1, DateTime.Now);
                                }
                            }
                            HttpContext.Current.Session["multiparcel_Utah"] = "Yes";
                            if (multitableRow.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Utah_Multicount"] = "Maximum";
                            }
                            driver.Quit();
                            return "MultiParcel";
                        }
                        if (multitableRow.Count == 1)
                        {
                            HttpContext.Current.Session["Utah_Zero"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        else
                        {
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table[1]/tbody/tr[2]/td[1]/a")).SendKeys(Keys.Enter);
                            IWebElement url = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[2]/td[1]/a"));
                            pathurl = url.GetAttribute("href");
                            Owner_Name = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[2]/td[1]")).Text;
                            driver.Navigate().GoToUrl(pathurl);
                            gc.CreatePdf_WOP_Chrome(orderNumber, "Address Search result page", driver, "UT", "Utah");
                        }



                    }

                    else if (searchType == "parcel")
                    {
                        driver.FindElement(By.XPath("/html/body/div/div[6]/div[2]/table/tbody/tr/td/table/tbody/tr/td[1]/ul[1]/li[3]/a")).SendKeys(Keys.Enter);
                        if (parcelNumber.Contains(":") || parcelNumber.Contains(" "))
                        {
                            parcelNumber = parcelNumber.Replace(":", "").Trim();
                            parcelNumber = parcelNumber.Replace(" ", "").Trim();
                        }
                        gc.CreatePdf_Chrome(orderNumber, parcelNumber, "Parcel Search", driver, "UT", "Utah");
                        driver.FindElement(By.Id("av_serial")).SendKeys(parcelNumber);
                        IWebElement rdio = driver.FindElement(By.XPath("//*[@id='form1']/table/tbody/tr/td/p[2]/label[2]/label/input"));
                        js.ExecuteScript("arguments[0].click();", rdio);
                        driver.FindElement(By.XPath("//*[@id='form1']/table/tbody/tr/td/p[3]/input")).SendKeys(Keys.Enter);
                        gc.CreatePdf_Chrome(orderNumber, parcelNumber, "Parcel Search result", driver, "UT", "Utah");

                        IList<IWebElement> TRserial = driver.FindElements(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr"));
                        int count = TRserial.Count;
                        for (int i = 2; i <= count; i++)
                        {
                            try
                            {
                                string selectedvalue = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[" + i + "]/td[3]/select/option[1]")).Text;
                                selectedvalue = selectedvalue.Replace(":", "");
                                if (selectedvalue == parcelNumber)
                                {
                                    IWebElement propinfo = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[" + i + "]/td[3]/select/option[2]"));
                                    string url = propinfo.GetAttribute("value");
                                    Owner_Name = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[2]/td[1]")).Text;
                                    gc.CreatePdf_Chrome(orderNumber, parcelNumber, "Parcel Search - select parcel number page", driver, "UT", "Utah");
                                    pathurl = "http://www.utahcounty.gov/LandRecords/" + url;
                                    driver.Navigate().GoToUrl(pathurl);
                                    //  break;
                                }
                            }
                            catch { }

                        }

                    }
                    else if (searchType == "ownername")
                    {
                        driver.FindElement(By.XPath("/html/body/div/div[6]/div[2]/table/tbody/tr/td/table/tbody/tr/td[1]/ul[1]/li[1]/a")).SendKeys(Keys.Enter);
                        gc.CreatePdf_WOP_Chrome(orderNumber, "Owner name Search", driver, "UT", "Utah");
                        driver.FindElement(By.Id("av_name")).SendKeys(ownername);
                        IWebElement rdio = driver.FindElement(By.XPath("//*[@id='av_valid_0']"));
                        //js.ExecuteScript("arguments[0].click();", rdio);
                        rdio.Click();
                        driver.FindElement(By.XPath("//*[@id='form1']/div/table/tbody/tr[1]/td/p[3]/input")).SendKeys(Keys.Enter);
                        gc.CreatePdf_WOP_Chrome(orderNumber, "Owner name Search result", driver, "UT", "Utah");

                        try
                        {
                            //multi parcel
                            IWebElement multitableElement = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table[1]/tbody"));
                            IList<IWebElement> multitableRow = multitableElement.FindElements(By.TagName("tr"));
                            IList<IWebElement> multirowTD;
                            foreach (IWebElement row in multitableRow)
                            {
                                multirowTD = row.FindElements(By.TagName("td"));
                                if (multirowTD.Count != 0 && !row.Text.Contains("Owner"))
                                {
                                    string multi = multirowTD[0].Text.Trim() + "~" + multirowTD[4].Text.Trim();
                                    gc.insert_date(orderNumber, multirowTD[1].Text.Trim(), 119, multi, 1, DateTime.Now);
                                }
                            }

                            HttpContext.Current.Session["multiparcel_Utah"] = "Yes";
                            if (multitableRow.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Utah_Multicount"] = "Maximum";
                            }
                            driver.Quit();
                            return "MultiParcel";
                        }
                        catch { }
                        try
                        {
                            IWebElement Nodata = driver.FindElement(By.XPath("/html/body/table"));
                            if (!Nodata.Text.Contains("Property Address") && !Nodata.Text.Contains("Owner Name"))
                            {
                                HttpContext.Current.Session["Utah_Zero"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }


                    Thread.Sleep(3000);
                    //Property details 

                    string Parcel_ID = "-", Property_Address = "-", Mailing_Address = "-", Legal_Description = "-";
                    Parcel_ID = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[1]/td[1]")).Text;
                    Parcel_ID = WebDriverTest.After(Parcel_ID, "Serial Number:");
                    Property_Address = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[3]/td[1]")).Text;
                    Property_Address = WebDriverTest.After(Property_Address, "Property Address:");
                    Mailing_Address = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[4]/td[1]")).Text;
                    Mailing_Address = WebDriverTest.After(Mailing_Address, "Mailing Address:");
                    Legal_Description = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[8]/td[1]")).Text;
                    Legal_Description = WebDriverTest.After(Legal_Description, "Legal Description:");

                    IList<IWebElement> tron = driver.FindElements(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr/td/div/div/div[1]/table[2]/tbody/tr"));
                    for (int trcount = 1; trcount <= tron.Count; trcount++)
                    {
                        IWebElement on = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr/td/div/div/div[1]/table[2]/tbody/tr[" + trcount + "]/td[1]/span"));
                        string greentag = on.GetAttribute("class");

                        if (greentag == "style1yv")
                        {
                            Owner_Name += driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr/td/div/div/div[1]/table[2]/tbody/tr[" + trcount + "]/ td[3]")).Text + "&";

                        }
                    }
                    Owner_Name = Regex.Replace(Owner_Name, "[0-9]{2,}", "").Replace("&", " ").Replace(":", "");
                    if (Parcel_ID.Contains(":"))
                    {
                        Parcel_ID = Parcel_ID.Replace(":", "");
                    }
                    gc.CreatePdf_Chrome(orderNumber, Parcel_ID, "Property Details information", driver, "UT", "Utah");
                    //OwnerName = Owner_Name.Substring(OwnerName.Length - 1,1);
                    string prop = Owner_Name + "~" + Property_Address + "~" + Legal_Description + "~" + Mailing_Address;
                    gc.insert_date(orderNumber, Parcel_ID, 122, prop, 1, DateTime.Now);

                    //assessment details
                    string Tax_Year = "-";
                    Thread.Sleep(4000);
                    driver.FindElement(By.XPath("//*[@id='TabbedPanels1']/ul/li[2]")).SendKeys(Keys.Enter);
                    Thread.Sleep(3000);
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr/td/div/div/div[2]/table/tbody/tr[3]/td[1]/a")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        Tax_Year = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/p[1]")).Text;
                        Tax_Year = WebDriverTest.After(Tax_Year, "Tax Year: ");
                        Tax_Year = WebDriverTest.Before(Tax_Year, "   Tax District:").Trim();
                    }
                    catch { }
                    string RealEstate_Residential = "", RealEstate_Agricultural = "", RealEstate_Commercial = "", RealEstate_Totals = "";
                    string Improvements_Residential = "", Improvements_Agricultural = "", Improvements_Commercial = "", Improvements_Totals = "";
                    string Greenbelt_Residential = "", Greenbelt_RealEstate = "", Greenbelt_HomeSite = "", Greenbelt_Totals = "", Greenbelt_TotalsRealProperty = "", Greenbelt_AttachedPersonalProperty = "", Greenbelt_ToatalValuation = "";
                    string Ok = "";
                    IWebElement tbassess = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table[1]/tbody"));
                    IList<IWebElement> trassess = tbassess.FindElements(By.TagName("tr"));
                    IList<IWebElement> tdassess;
                    foreach (IWebElement row in trassess)
                    {
                        tdassess = row.FindElements(By.TagName("td"));


                        if (Ok == "Real Estate" && tdassess[3].Text.Trim() != "")
                        {
                            RealEstate_Residential = tdassess[1].Text + "~" + tdassess[3].Text + "~" + tdassess[5].Text + "~" + RealEstate_Residential;



                        }
                        if (tdassess.Count != 0 && row.Text.Contains("* * Real Estate"))
                        {
                            Ok = "Real Estate";

                        }
                        if (Ok == "Improvements" && tdassess[3].Text.Trim() != "")
                        {
                            Improvements_Residential = tdassess[1].Text + "~" + tdassess[3].Text + "~" + tdassess[5].Text + "~" + Improvements_Residential;


                            //RealEstate_Residential = new string(RealEstate_Residential.Reverse().ToArray());

                        }
                        if (tdassess.Count != 0 && row.Text.Contains("* * Improvements"))
                        {
                            Ok = "Improvements";

                        }
                        if (Ok == "Greenbelt as of")
                        {
                            Greenbelt_Residential = tdassess[1].Text + "~" + tdassess[3].Text + "~" + tdassess[5].Text + "~" + Greenbelt_Residential;


                            //RealEstate_Residential = new string(RealEstate_Residential.Reverse().ToArray());

                        }
                        if (tdassess.Count != 0 && row.Text.Contains("* * Greenbelt as of "))
                        {
                            Ok = "Greenbelt as of";

                        }

                        //if (tdassess.Count != 0 && !row.Text.Contains("* * Real Estate"))
                        //{
                        //    string assessment = Tax_Year + "~" + tdassess[1].Text.Trim() + "~" + tdassess[3].Text.Trim() + "~" + tdassess[5].Text.Trim(); ;
                        //    gc.insert_date(orderNumber, Parcel_ID, 123, assessment, 1,DateTime.Now);
                        //}
                    }
                    RealEstate_Residential = RealEstate_Residential + "Ert";
                    RealEstate_Residential = RealEstate_Residential.Replace("~Ert", "");



                    Greenbelt_Residential = Greenbelt_Residential + "Ert";
                    Greenbelt_Residential = Greenbelt_Residential.Replace("~Ert", "");



                    Improvements_Residential = Improvements_Residential + "Ert";
                    Improvements_Residential = Improvements_Residential.Replace("~Ert", "");

                    string Overall = RealEstate_Residential + "~" + Improvements_Residential + "~" + Greenbelt_Residential;
                    string[] Real = Overall.Split('~');
                    //for (int i = RealSplit.Count() - 1; i >= 0; i--)
                    //{
                    //    string assessment = RealSplit[i] + "~" + tdassess[1].Text.Trim() + "~" + tdassess[3].Text.Trim() + "~" + tdassess[5].Text.Trim(); ;
                    //    gc.insert_date(orderNumber, Parcel_ID, 123, assessment, 1,DateTime.Now);
                    //}
                    int K = 0;
                    string FirstHalf = "";
                    try
                    {
                        FirstHalf = Real[K + 10] + "~" + Real[K + 11] + "~" + Real[K + 7] + "~" + Real[K + 8] + "~" + Real[K + 4] + "~" + Real[K + 5] + "~" + Real[K + 1] + "~" + Real[K + 2] + "~" + Real[K + 22] + "~" + Real[K + 23] + "~" + Real[K + 19] + "~" + Real[K + 20] + "~" + Real[K + 16] + "~" + Real[K + 17] + "~" + Real[K + 13] + "~" + Real[K + 14] + "~" + Real[K + 40] + "~" + Real[K + 41] + "~" + Real[K + 37] + "~" + Real[K + 38] + "~" + Real[K + 34] + "~" + Real[K + 35] + "~" + Real[K + 31] + "~" + Real[K + 32] + "~" + Real[K + 28] + "~" + Real[K + 29] + "~" + Real[K + 25] + "~" + Real[K + 26];
                    }
                    catch { }
                    gc.insert_date(orderNumber, Parcel_ID, 123, FirstHalf, 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.CreatePdf_Chrome(orderNumber, Parcel_ID, "Assessment Details information", driver, "UT", "Utah");

                    driver.Navigate().GoToUrl(pathurl);
                    Thread.Sleep(3000);

                    //Tax history details

                    driver.FindElement(By.XPath("//*[@id='TabbedPanels1']/ul/li[3]")).SendKeys(Keys.Enter);
                    gc.CreatePdf_Chrome(orderNumber, Parcel_ID, "Tax History Details information", driver, "UT", "Utah");
                    IWebElement tbtaxHist = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr/td/div/div/div[3]/table/tbody"));
                    IList<IWebElement> trtaxhist = tbtaxHist.FindElements(By.TagName("tr"));
                    IList<IWebElement> tdtaxhist;

                    foreach (IWebElement row in trtaxhist)
                    {
                        tdtaxhist = row.FindElements(By.TagName("td"));
                        if (tdtaxhist.Count != 0 && !row.Text.Contains("Year"))
                        {
                            string taxhist = tdtaxhist[0].Text + "~" + tdtaxhist[1].Text + "~" + tdtaxhist[2].Text + "~" + tdtaxhist[3].Text + "~" + tdtaxhist[4].Text + "~" + tdtaxhist[5].Text + "~" + tdtaxhist[6].Text + "~" + tdtaxhist[7].Text + "~" + tdtaxhist[8].Text;
                            gc.insert_date(orderNumber, Parcel_ID, 128, taxhist, 1, DateTime.Now);
                        }
                    }

                    try
                    {
                        IWebElement taxdetailtable = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr/td/div/div/div[3]/table/tbody"));
                        IList<IWebElement> taxdetailtableRow = taxdetailtable.FindElements(By.TagName("tr"));
                        int taxrowcount = taxdetailtableRow.Count;
                        IList<IWebElement> taxdetailrowTD;
                        int c = 0;
                        foreach (IWebElement rowid1 in taxdetailtableRow)
                        {

                            taxdetailrowTD = rowid1.FindElements(By.TagName("td"));
                            if (taxdetailrowTD.Count != 0 && c <= taxrowcount && c > 2)
                            {
                                IWebElement taxhistory = driver.FindElement(By.XPath("//*[@id='TabbedPanels1']/div/div[3]/table/tbody/tr[" + c + "]/td[1]/a"));
                                string taxurl = taxhistory.GetAttribute("href");
                                taxurllist.Add(taxurl);
                                //Thread.Sleep(2000);
                                //string year = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[1]/td/strong[2]")).Text;
                                //gc.CreatePdf_Chrome(orderNumber, Parcel_ID, "TaxDeailInformation" + year, driver);                        
                                //IWebElement URL= driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[17]/td/a"));
                                //string Url = URL.GetAttribute("href");
                                //gc.downloadfile(Url, orderNumber, Parcel_ID, "TaxBill" + year);
                                ////driver.Navigate().GoToUrl("http://www.utahcounty.gov/LandRecords/Property.asp?av_serial="+Parcel_ID);
                                //driver.Navigate().Back();
                            }

                            // Thread.Sleep(3000);
                            c++;
                        }
                    }
                    catch { }
                    int i1 = 0;
                    foreach (string URL in taxurllist)
                    {
                        if (i1 < 3)
                        {
                            driver.Navigate().GoToUrl(URL);
                            Thread.Sleep(4000);
                            string year = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[1]/td/strong[2]")).Text;
                            gc.CreatePdf_Chrome(orderNumber, Parcel_ID, "TaxDeailInformation" + year, driver, "UT", "Utah");
                            IWebElement URL1 = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[17]/td/a"));
                            string Url = URL1.GetAttribute("href");
                            driver.Navigate().GoToUrl(Url);
                            Thread.Sleep(6000);
                            driver.SwitchTo().Window(driver.WindowHandles.Last());
                            Thread.Sleep(2000);
                            gc.CreatePdf_Chrome(orderNumber, Parcel_ID, "Tax Bill" + year, driver, "UT", "Utah");
                            driver.SwitchTo().Window(driver.WindowHandles.First());
                            Thread.Sleep(2000);
                            //gc.downloadfile(Url, orderNumber, Parcel_ID, "TaxBill" + year);
                            i1++;
                        }

                    }


                    driver.Navigate().GoToUrl("http://www.utahcounty.gov/LandRecords/Property.asp?av_serial=" + Parcel_ID);
                    driver.FindElement(By.XPath("//*[@id='TabbedPanels1']/ul/li[3]")).SendKeys(Keys.Enter);
                    //Tax payment details
                    string Taxing_Authority = "", Good_PThrough_Date = "", date = "", deleiquent = "";
                    IWebElement tax_pay;
                    try
                    {
                        tax_pay = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr/td/div/div/div[3]/table/tbody/tr[3]/td[8]/a"));
                        deleiquent = tax_pay.Text;

                        if (deleiquent.Contains("Click for Payoff"))
                        {
                            tax_pay.SendKeys(Keys.Enter);
                            Thread.Sleep(3000);

                            IWebElement td = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr/td"));
                            date = td.Text;
                            date = WebDriverTest.After(date, "Total amount if paid as of").Trim();
                            date = WebDriverTest.Before(date, ".  .  .").Trim();

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
                            string Mon = "";
                            string[] da = date.Split('/');
                            string month = da[0];
                            string Date = da[1];
                            string year = da[2];

                            switch (month)
                            {
                                case "01":
                                    Mon = "January";
                                    break;
                                case "02":
                                    Mon = "February";
                                    break;
                                case "03":
                                    Mon = "March";
                                    break;
                                case "04":
                                    Mon = "April";
                                    break;
                                case "05":
                                    Mon = "May";
                                    break;
                                case "06":
                                    Mon = "June";
                                    break;
                                case "07":
                                    Mon = "July";
                                    break;
                                case "08":
                                    Mon = "August";
                                    break;
                                case "09":
                                    Mon = "September";
                                    break;
                                case "10":
                                    Mon = "October";
                                    break;
                                case "11":
                                    Mon = "November";
                                    break;
                                case "12":
                                    Mon = "December";
                                    break;
                            }
                            driver.FindElement(By.XPath("//*[@id='av_date_btn']")).Click();


                            //select month
                            for (int i = 1; i < 12; i++)
                            {
                                string calmy = driver.FindElement(By.XPath("/html/body/div[2]/table/thead/tr[1]/td[2]")).Text;
                                string[] my = calmy.Split(',');
                                string cal_month = my[0];
                                string cal_year = my[1];

                                if (cal_month.Contains(Mon))
                                {
                                    break;
                                }
                                else
                                {
                                    driver.FindElement(By.XPath("/html/body/div[2]/table/thead/tr[2]/td[4]")).Click();
                                }
                            }

                            //select year
                            for (int j = 1; j <= 5; j++)
                            {
                                string calmy = driver.FindElement(By.XPath("/html/body/div[2]/table/thead/tr[1]/td[2]")).Text;
                                string[] my = calmy.Split(',');
                                string cal_month = my[0];
                                string cal_year = my[1];

                                if (cal_year.Contains(year))
                                {
                                    break;
                                }
                                else
                                {
                                    driver.FindElement(By.XPath("/html/body/div[2]/table/thead/tr[2]/td[5]")).Click();
                                }
                            }

                            //select date
                            IWebElement tbdate = driver.FindElement(By.XPath("/html/body/div[2]/table/tbody"));
                            IList<IWebElement> trdate = tbdate.FindElements(By.TagName("tr"));
                            IList<IWebElement> tddate;
                            foreach (IWebElement row in trdate)
                            {
                                tddate = row.FindElements(By.TagName("td"));
                                for (int a = 1; a <= 5; a++)
                                {
                                    if (row.Text.Contains(Date))
                                    {
                                        for (int b = 1; b <= 8; b++)
                                        {
                                            if (tddate[b].Text.Contains(Date))
                                            {
                                                driver.FindElement(By.XPath("/html/body/div[2]/table/tbody/tr[" + a + "]/td[" + b + "]")).Click();
                                                break;
                                            }
                                        }

                                    }
                                }
                            }

                            driver.FindElement(By.XPath("//*[@id='button']")).SendKeys(Keys.Enter);
                            Thread.Sleep(3000);
                            gc.CreatePdf_Chrome(orderNumber, Parcel_ID, "Deliquent Tax", driver, "UT", "Utah");
                            Taxing_Authority = driver.FindElement(By.XPath("//p[contains(text(),'The Treasurer')]/following-sibling::p")).Text;
                            if (Taxing_Authority.Contains("\r\n"))
                            {
                                Taxing_Authority = Taxing_Authority.Replace("\r\n", " ");
                            }
                            IWebElement deliTB = driver.FindElement(By.XPath("//p[contains(text(),'Serial Number:')]/following-sibling::table/tbody"));
                            IList<IWebElement> trdeli = deliTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> tddeli;
                            foreach (IWebElement row in trdeli)
                            {
                                tddeli = row.FindElements(By.TagName("td"));
                                if (tddeli.Count != 0 && !row.Text.Contains("Year") && !row.Text.Contains("amount if paid as") && !row.Text.Contains("--") && (row.Text != "         "))
                                {
                                    string deliq = tddeli[0].Text + "~" + tddeli[1].Text + "~" + tddeli[2].Text + "~" + tddeli[3].Text + "~" + tddeli[4].Text + "~" + tddeli[5].Text + "~" + date + "~" + Taxing_Authority;
                                    if (deliq.Contains("\r\n"))
                                    {
                                        deliq = deliq.Replace("\r\n", "");
                                    }
                                    gc.insert_date(orderNumber, Parcel_ID, 143, deliq, 1, DateTime.Now);
                                }
                            }
                            TaxTime = DateTime.Now.ToString("HH:mm:ss");
                            driver.Navigate().GoToUrl(pathurl);
                            Thread.Sleep(3000);
                            driver.FindElement(By.XPath("//*[@id='TabbedPanels1']/ul/li[3]")).SendKeys(Keys.Enter);
                            Thread.Sleep(3000);

                            driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr/td/div/div/div[3]/table/tbody/tr[3]/td[6]/div/a")).SendKeys(Keys.Enter);
                            gc.CreatePdf_Chrome(orderNumber, Parcel_ID, "Tax Payment Details information", driver, "UT", "Utah");
                            try
                            {
                                IWebElement tbtax = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody"));
                                IList<IWebElement> trtax = tbtax.FindElements(By.TagName("tr"));
                                IList<IWebElement> tdtax;
                                foreach (IWebElement row in trtax)
                                {
                                    tdtax = row.FindElements(By.TagName("td"));
                                    if (tdtax.Count != 0 && !row.Text.Contains("Tax Year"))
                                    {
                                        string tax = tdtax[0].Text + "~" + tdtax[2].Text + "~" + tdtax[4].Text + "~" + tdtax[6].Text + "~" + tdtax[8].Text + "~" + tdtax[10].Text + "~" + tdtax[12].Text + "~" + tdtax[14].Text + "~" + tdtax[16].Text;
                                        gc.insert_date(orderNumber, Parcel_ID, 129, tax, 1, DateTime.Now);
                                    }
                                }
                            }
                            catch { }
                        }
                    }

                    catch { }


                    if (deleiquent == "")
                    {

                        //
                        driver.FindElement(By.XPath("//*[@id='TabbedPanels1']/div/div[3]/table/tbody/tr[2]/td[1]/a")).SendKeys(Keys.Enter);
                        gc.CreatePdf_Chrome(orderNumber, Parcel_ID, "Tax Payment Details information", driver, "UT", "Utah");

                        try
                        {
                            IWebElement tbtax = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody"));
                            IList<IWebElement> trtax = tbtax.FindElements(By.TagName("tr"));
                            IList<IWebElement> tdtax;
                            foreach (IWebElement row in trtax)
                            {
                                tdtax = row.FindElements(By.TagName("td"));
                                if (tdtax.Count > 1 && tdtax[0].Text.Trim() != "")
                                {
                                    string tax = tdtax[0].Text + "~" + tdtax[1].Text;
                                    gc.insert_date(orderNumber, Parcel_ID, 129, tax, 1, DateTime.Now);
                                }
                            }
                        }
                        catch
                        { }

                    }
                    driver.Navigate().GoToUrl(pathurl);
                    Thread.Sleep(3000);

                    //driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr/td/div/ul/li[3]")).SendKeys(Keys.Enter);
                    //driver.FindElement(By.XPath("//*[@id='TabbedPanels1']/div/div[3]/table/tbody/tr[3]/td[1]/a")).SendKeys(Keys.Enter);
                    //IWebElement URL= driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[17]/td/a"));
                    //string Url = URL.GetAttribute("href");
                    //// URL.SendKeys(Keys.Enter);
                    //// driver.SwitchTo().Window(driver.WindowHandles.Last());
                    // driver.Navigate().GoToUrl(Url);
                    //string billpdf = outputPath + Parcel_ID.Replace(":", "") + "tax_bill.pdf";
                    //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    //WebClient downloadpdf = new WebClient();
                    //downloadpdf.DownloadFile(Url, billpdf);

                    // CreatePdf_WOP(orderNumber, "Tax Bill");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "UT", "Utah", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    driver.Quit();
                    gc.mergpdf(orderNumber, "UT", "Utah");
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