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
    public class WebDriver_CAMonterey
    {
        string Outparcelno = "", outputPath = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        private TimeSpan timeOutInSeconds;
        MySqlParameter[] mParam;
        GlobalClass gc = new GlobalClass();
        string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
        string strParcelNumber="",strRoll = "-", strRollCS = "-", strRollS = "-", strRollSS = "-", strhref = "-", strAssessment = "-", strTaxYear = "-", strFPaidStatus = "-", strFDuePaidDate = "-", strFDue = "-",
            strFPaid = "-", strFBalance = "-", strSPaidStatus = "-", strSDuePaidDate = "-", strSDue = "-", strSPaid = "-", strSBalance = "-", strTotalDue = "-", strTotalPaid = "-",
            strTotalBalance = "-", strCategory = "-", strTaxCode = "-", strDis = "-", strRate = "-", strFInstall = "-", strSInstall = "-", strTotal = "-", strPhone = "-", strDAssessNo = "-",
            strDYear = "-", strDDefault = "-", strDPayPlan = "-", strDAnnual = "-", strDBalance = "-", strDParcelNo = "-", stresult = "", strRollTitle = "", strAcknow = "", strCurr = "";
        List<string> strlURL = new List<string>();
        List<string> strTax = new List<string>();
        List<string> strTaxPrior = new List<string>();
        List<string> strTaxHistory = new List<string>();
        List<string> strTaxDetails = new List<string>();
        IList<IWebElement> strYear, strType;
        int priorcount, historycount;
        IWebElement Iresult;
        public string FTP_CAMonterey(string address, string parcelNumber, string searchType, string orderNumber, string ownername, string directParcel, string state, string county)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;            
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {

                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    if (searchType == "titleflex")
                    {
                        if (parcelNumber.Length == 9)
                        {
                            strParcelNumber = parcelNumber.Replace("-", "");
                            strParcelNumber += "000";
                            if (parcelNumber.Contains("-"))
                            {
                                strParcelNumber += "000";
                                gc.TitleFlexSearch(orderNumber, strParcelNumber, ownername, address, state, county);
                            }
                        }
                        else
                        {
                            gc.TitleFlexSearch(orderNumber, parcelNumber, ownername, address, state, county);
                        }
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {

                            HttpContext.Current.Session["Nodata_CAMonterey"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        else
                        {
                            //string strTitleAssess = HttpContext.Current.Session["TitleFlexAssess"].ToString();
                            parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                            //gc.insert_date(orderNumber, parcelNumber, 312, strTitleAssess, 1, DateTime.Now);
                            searchType = "parcel";

                            AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                        }
                    }
                    if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("https://common3.mptsweb.com/mbc/monterey/tax/search");
                        IWebElement Iyear = driver.FindElement(By.Id("SelTaxYear"));
                        SelectElement syear = new SelectElement(Iyear);
                        strYear = syear.Options;
                        IWebElement IType = driver.FindElement(By.Id("SearchVal"));
                        SelectElement stype = new SelectElement(IType);
                        strType = stype.Options;
                        driver.FindElement(By.Id("SearchValue")).SendKeys(parcelNumber);

                        foreach (IWebElement IRollyear in strYear)
                        {
                            syear.SelectByText(IRollyear.Text);
                            stype.SelectByText("FEE PARCEL");
                            // gc.CreatePdf(orderNumber, parcelNumber, IRollyear.Text + "Parcel Search", driver, "CA", "Monterey");
                            driver.FindElement(By.Id("SearchSubmit")).SendKeys(Keys.Enter);
                            Thread.Sleep(3000);
                            try
                            {
                                IWebElement Inodata = driver.FindElement(By.Id("ResultDiv"));
                                if(Inodata.Text.Contains("no matching records were found"))
                                {
                                    HttpContext.Current.Session["Nodata_CAMonterey"] = "Zero";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }
                            gc.CreatePdf(orderNumber, parcelNumber, IRollyear.Text + "Parcel Search Result", driver, "CA", "Monterey");

                            try
                            {
                                Iresult = driver.FindElement(By.XPath("//*[@id='ResultDiv']/div"));
                                stresult = Iresult.Text;
                                if (stresult.Contains("Sorry, no matching records were found. Please review your search criteria and try again.") && stresult.Trim() != "")
                                {
                                    stype.SelectByText("ASSESSMENT");
                                    stresult = "";
                                    //gc.CreatePdf(orderNumber, parcelNumber, IRollyear.Text + "Parcel Search", driver, "CA", "Monterey");
                                    driver.FindElement(By.Id("SearchSubmit")).SendKeys(Keys.Enter);
                                    Thread.Sleep(3000);
                                    gc.CreatePdf(orderNumber, parcelNumber, IRollyear.Text + "Parcel Search Result", driver, "CA", "Monterey");
                                }

                            }
                            catch { }
                            try
                            {
                                if (IRollyear.Text != "")
                                {

                                    for (int i = 1; i < 20; i++)
                                    {

                                        historycount = 0; priorcount = 0;
                                        string strTaxType = stype.SelectedOption.Text;
                                        strRollTitle = driver.FindElement(By.XPath("/html/body/div[2]/section[2]/div/div/div/div/div[" + i + "]/div/div/h4")).Text;
                                        strRollCS = driver.FindElement(By.XPath("/html/body/div[2]/section[2]/div/div/div/div/div[" + i + "]/div/div/p")).Text;
                                        if (IRollyear.Text.Contains("HISTORICAL"))
                                        {
                                            try
                                            {
                                                foreach (string strlink in strTax)
                                                {
                                                    if ((strRollTitle + strRollCS) != strlink)
                                                    {
                                                        historycount++;
                                                    }
                                                    if ((historycount == strTax.Count) && (strRollCS.Contains("Roll Cat. : CS") || strRollCS.Contains("Roll Cat. : SS") || strRollCS.Contains("Roll Cat. : DU")))
                                                    {
                                                        IWebElement ISsearch = driver.FindElement(By.XPath("/html/body/div[2]/section[2]/div/div/div/div/div[ " + i + "]/div/div/div/a"));
                                                        strhref = ISsearch.GetAttribute("href");
                                                        strlURL.Add(strhref);
                                                    }
                                                }
                                            }
                                            catch { }
                                        }
                                        if (IRollyear.Text.Contains("PRIOR"))
                                        {
                                            try
                                            {
                                                foreach (string strlink in strTax)
                                                {
                                                    //strTax.Add(strRollTitle + strRollCS);
                                                    if ((strRollTitle + strRollCS) != strlink)
                                                    {
                                                        priorcount++;
                                                    }
                                                    if ((priorcount == strTax.Count) && (strRollCS.Contains("Roll Cat. : CS") || strRollCS.Contains("Roll Cat. : SS") || strRollCS.Contains("Roll Cat. : DU")))
                                                    {
                                                        IWebElement ISsearch = driver.FindElement(By.XPath("/html/body/div[2]/section[2]/div/div/div/div/div[ " + i + "]/div/div/div/a"));
                                                        strhref = ISsearch.GetAttribute("href");
                                                        strlURL.Add(strhref);
                                                    }

                                                }
                                            }
                                            catch { }
                                        }
                                        string currentYear = DateTime.Now.Year.ToString();
                                        string year = currentYear.Substring(0, 3);
                                        if (IRollyear.Text.Contains(year) && !IRollyear.Text.Contains("HISTORICAL") && !IRollyear.Text.Contains("PRIOR"))
                                        {
                                            strTax.Add(strRollTitle + strRollCS);
                                            if (strRollCS.Contains("Roll Cat. : CS") || strRollCS.Contains("Roll Cat. : SS") || strRollCS.Contains("Roll Cat. : DU"))
                                            {
                                                IWebElement ISsearch = driver.FindElement(By.XPath("/html/body/div[2]/section[2]/div/div/div/div/div[ " + i + "]/div/div/div/a"));
                                                strhref = ISsearch.GetAttribute("href");
                                                strlURL.Add(strhref);
                                            }
                                        }
                                    }

                                }
                            }
                            catch (Exception ex) { }
                        }


                        foreach (string strURL in strlURL)
                        {
                            driver.Navigate().GoToUrl(strURL);
                            Thread.Sleep(3000);
                            for (int i = 5; i < 7; i++)
                            {
                                try
                                {
                                    strAssessment = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[" + i + "]/div/div[1]/div[1]/div[2]")).Text;
                                    strTaxYear = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[" + i + "]/div/div[1]/div[3]/div[2]")).Text;
                                    strFPaidStatus = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[" + i + "]/div/div[2]/div[1]/div[1]/dl/dd[1]")).Text;
                                    strFDuePaidDate = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[" + i + "]/div/div[2]/div[1]/div[1]/dl/dd[2]")).Text;
                                    strFDue = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[" + i + "]/div/div[2]/div[1]/div[1]/dl/dd[3]")).Text;
                                    strFPaid = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[" + i + "]/div/div[2]/div[1]/div[1]/dl/dd[4]")).Text;
                                    strFBalance = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[" + i + "]/div/div[2]/div[1]/div[1]/dl/dd[5]")).Text;

                                    gc.CreatePdf(orderNumber, parcelNumber, strAssessment + strTaxYear + strFDuePaidDate.Replace("/", "-") + "Tax Search Result", driver, "CA", "Monterey");

                                    strSPaidStatus = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[" + i + "]/div/div[2]/div[1]/div[2]/dl/dd[1]")).Text;
                                    strSDuePaidDate = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[" + i + "]/div/div[2]/div[1]/div[2]/dl/dd[2]")).Text;
                                    strSDue = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[" + i + "]/div/div[2]/div[1]/div[2]/dl/dd[3]")).Text;
                                    strSPaid = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[" + i + "]/div/div[2]/div[1]/div[2]/dl/dd[4]")).Text;
                                    strSBalance = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[" + i + "]/div/div[2]/div[1]/div[2]/dl/dd[5]")).Text;

                                    strTotalDue = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[" + i + "]/div/div[2]/div[2]/dl/dd[1]")).Text;
                                    strTotalPaid = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[" + i + "]/div/div[2]/div[2]/dl/dd[2]")).Text;
                                    strTotalBalance = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[" + i + "]/div/div[2]/div[2]/dl/dd[3]")).Text;


                                    //gc.CreatePdf(orderNumber, parcelNumber, "Tax Search Result", driver, "CA", "Monterey");
                                    driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[" + i + "]/ul/li[2]/a")).SendKeys(Keys.Enter);
                                    Thread.Sleep(2000);
                                    strCategory = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[" + i + "]/div/div[3]/dl/dd[4]")).Text;
                                    gc.CreatePdf(orderNumber, parcelNumber, strTaxYear + strCategory + strFDuePaidDate.Replace("/", "-") + "Tax Assessment Search Result", driver, "CA", "Monterey");

                                    string strFirstInstall = strAssessment + "~" + strTaxYear + "~" + strCategory + "~" + "1st Installment" + "~" + strFPaidStatus + "~" + strFDuePaidDate + "~" + strFDue + "~" + strFPaid + "~" + strFBalance;
                                    gc.insert_date(orderNumber, parcelNumber, 233, strFirstInstall, 1, DateTime.Now);

                                    string strSecondInstall = strAssessment + "~" + strTaxYear + "~" + strCategory + "~" + "2nd Installment" + "~" + strSPaidStatus + "~" + strSDuePaidDate + "~" + strSDue + "~" + strSPaid + "~" + strSBalance;
                                    gc.insert_date(orderNumber, parcelNumber, 233, strSecondInstall, 1, DateTime.Now);

                                    string strTotalInstall = strAssessment + "~" + strTaxYear + "~" + strCategory + "~" + "1st & 2nd Installment" + "~" + "-" + "~" + "-" + "~" + strTotalDue + "~" + strTotalPaid + "~" + strTotalBalance;
                                    gc.insert_date(orderNumber, parcelNumber, 233, strTotalInstall, 1, DateTime.Now);

                                    driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[" + i + "]/ul/li[3]/a")).SendKeys(Keys.Enter);
                                    Thread.Sleep(2000);
                                    gc.CreatePdf(orderNumber, strAssessment, strTaxYear + strCategory + strFDuePaidDate.Replace("/", "-") + "Tax Information Result", driver, "CA", "Monterey");
                                    try
                                    {
                                        for (int j = 1; j < 20; j++)
                                        {
                                            strTaxCode = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[" + i + "]/div/div[4]/div[" + j + "]/div/div/dl/dd[1]")).Text;
                                            strDis = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[" + i + "]/div/div[4]/div[" + j + "]/div/div/dl/dd[2]")).Text;
                                            strRate = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[" + i + "]/div/div[4]/div[" + j + "]/div/div/dl/dd[3]")).Text;
                                            strFInstall = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[" + i + "]/div/div[4]/div[" + j + "]/div/div/dl/dd[4]")).Text;
                                            strSInstall = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[" + i + "]/div/div[4]/div[" + j + "]/div/div/dl/dd[5]")).Text;
                                            strTotal = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[" + i + "]/div/div[4]/div[" + j + "]/div/div/dl/dd[6]")).Text;
                                            strPhone = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[" + i + "]/div/div[4]/div[" + j + "]/div/div/dl/dd[7]")).Text;

                                            string strTaxCodeInfo = strAssessment + "~" + strTaxCode + "~" + strDis + "~" + strRate + "~" + strFInstall + "~" + strSInstall + "~" + strTotal + "~" + strPhone;
                                            gc.insert_date(orderNumber, parcelNumber, 234, strTaxCodeInfo, 1, DateTime.Now);
                                        }
                                    }
                                    catch { }

                                    try
                                    {
                                        IWebElement url = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[" + i + "]/div/div[1]/div[4]/div/a"));
                                        string URL = url.GetAttribute("href");
                                        gc.downloadfile(URL, orderNumber, parcelNumber, strAssessment + strTaxYear + strCategory + strFDuePaidDate.Replace("/", "-") + "ViewTax_Bill.pdf", "CA", "Monterey");

                                    }
                                    catch { }

                                    try
                                    {
                                        driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[" + i + "]/ul/li[4]/a")).SendKeys(Keys.Enter);
                                        Thread.Sleep(2000);

                                        strDAssessNo = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[" + i + "]/div/div[1]/div[1]/div[2]")).Text;
                                        strDParcelNo = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[" + i + "]/div/div[1]/div[2]/div[2]")).Text;
                                        strDYear = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[" + i + "]/div/div[1]/div[3]/div[2]")).Text;
                                        strDDefault = driver.FindElement(By.XPath("//*[@id='h2tab4']/div[2]/div[1]/div[2]")).Text;
                                        strDPayPlan = driver.FindElement(By.XPath("//*[@id='h2tab4']/div[2]/div[2]/div[2]")).Text;
                                        strDAnnual = driver.FindElement(By.XPath("//*[@id='h2tab4']/div[2]/div[3]/div[2]")).Text;
                                        strDBalance = driver.FindElement(By.XPath("//*[@id='h2tab4']/div[2]/div[4]/div[2]")).Text;
                                        gc.CreatePdf(orderNumber, parcelNumber, strDYear + "Delinquent Search Result", driver, "CA", "Monterey");

                                        string strDTaxInfo = strDAssessNo + "~" + strDYear + "~" + strDDefault + "~" + strDPayPlan + "~" + strDAnnual + "~" + strDBalance;
                                        gc.insert_date(orderNumber, strDParcelNo, 255, strDTaxInfo, 1, DateTime.Now);

                                        IWebElement IDurl = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[" + i + "]/div/div[1]/div[4]/div/a"));
                                        string DelinquentURL = IDurl.GetAttribute("href");
                                        gc.downloadfile(DelinquentURL, orderNumber, parcelNumber, strDAssessNo + strDYear + "ViewTax_Bill.pdf", "CA", "Monterey");
                                    }
                                    catch { }
                                }
                                catch { }
                            }
                        }

                    }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "CA", "Monterey", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);


                    driver.Quit();
                    gc.mergpdf(orderNumber, "CA", "Monterey");
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