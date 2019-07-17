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
    public class WebDriver_OHHamilton
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();
        string strparcelNo = "-", strParcelNumber = "-", strName = "-", strAddress = "-", strOwnerName = "-", strProperty = "-", strPropertyAddress = "-", strMailingAddress = "-", strPropertyType = "-", strYearBuild = "-", strLegalDiscription = "-",
               strTaxDistrict = "-", strTax = "-", strTaxYear = "-", strMarketLand = "-", strMarketImprove = "-", strMarketTotal = "-", strAssessLand = "-", strAssessImprove = "-", strAssessTotal = "-", strBOR = "-", strRR = "-", strHS = "-",
               strOOC = "-", strFC = "-", strSA = "-", strCAUV = "-", strTIF = "-", strAdpted = "-", strExcempt = "-", strValueYear = "-", strValueAssDate = "-", strValueLand = "-", strValueImprove = "-", strValueTotal = "-", strValueCAUV = "-",
               strValueReason = "-", strTaxInfoType = "-", strPrriorDelinquent = "-", strAdjDelinquent = "-", strFirstHalf = "-", strAdjFirstHalf = "-", strSecondHalf = "-", strAdjSecondHalf = "-", strOverTitle = "-", strOverValue = "-", strPayDate = "-",
               strPayHalf = "-", strPrior = "-", strPayFirstHalf = "-", strPaySecondHalf = "-", strSurplus = "-", strRateTitle = "-", strRateValue = "-", strcalTitle = "-", strcalValue = "-", strHalfTitle = "-", strHalfValue = "-", SAInfoType="-",
            strSTaxInfoType = "-", strSPrriorDelinquent = "-", strSAdjDelinquent = "-", strSFirstHalf = "-", strSAdjFirstHalf = "-", strSSecondHalf = "-",  strSAdjSecondHalf = "-", SAInfoTyp="-",strPayment="-", strPay="-", strSchool="-";
        string[] addressSplit = new string[] { "\r\n" };
        public string FTP_OHHamilton(string houseno, string sname, string parcelNumber, string ownername, string searchType, string unitno, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {

                try
                {

                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("http://wedge.hcauditor.org/");
                    Thread.Sleep(4000);
                    if (searchType == "titleflex")
                    {
                        string address = houseno + " " + sname + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "OH", "Hamilton");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_OHHamilton"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        driver.FindElement(By.Id("search_radio_address")).Click();
                        driver.FindElement(By.Id("house_number_low")).SendKeys(houseno);
                        driver.FindElement(By.Id("street_name")).SendKeys(sname);
                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "OH", "Hamilton");
                        IWebElement IAddress = driver.FindElement(By.XPath("/html/body/div[1]/div[3]/div[2]/div[3]/form[2]/div[2]/button[1]"));
                        IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                        js.ExecuteScript("arguments[0].click();", IAddress);
                        Thread.Sleep(6000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "OH", "Hamilton");

                        try
                        {
                            //MultiParcel
                            string strMulti = driver.FindElement(By.Id("search-results_info")).Text;
                            string strMultiCount = gc.Between(strMulti, "Showing 1 to ", " of");
                            if (Convert.ToInt32(strMultiCount) >= 25)
                            {
                                HttpContext.Current.Session["multiParcel_OHHamilton_count"] = "Maximum";
                                return "Maximum";
                            }
                            if (!strMulti.Contains("Showing 1 to 1 of 1 entries") && Convert.ToInt32(strMultiCount) <= 10)
                            {
                                IWebElement IMultiElement = driver.FindElement(By.XPath("/html/body/div/div[3]/div/div/div[2]/div[1]/table/tbody"));
                                IList<IWebElement> IMultiRow = driver.FindElements(By.TagName("tr"));
                                IList<IWebElement> IMultiTD;
                                foreach (IWebElement row in IMultiRow)
                                {
                                    IMultiTD = row.FindElements(By.TagName("td"));
                                    if (IMultiTD.Count != 0 && !row.Text.Contains("Parcel Number"))
                                    {
                                        strParcelNumber = IMultiTD[0].Text;
                                        strName = IMultiTD[1].Text;
                                        strAddress = IMultiTD[2].Text;

                                        string strMultiDetails = strName + "~" + strAddress;
                                        gc.insert_date(orderNumber, strParcelNumber, 256, strMultiDetails, 1, DateTime.Now);
                                    }
                                }
                            }
                            driver.Quit();
                            HttpContext.Current.Session["multiparcel_OHHamilton"] = "Yes";
                            return "MultiParcel";
                        }
                        catch { }

                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.Id("search_radio_parcel_id")).Click();
                        driver.FindElement(By.Id("parcel_number")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Result", driver, "OH", "Hamilton");
                        IWebElement IParcel = driver.FindElement(By.XPath("/html/body/div[1]/div[3]/div[2]/div[3]/form[3]/div[2]/button[1]"));
                        IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                        js.ExecuteScript("arguments[0].click();", IParcel);
                        Thread.Sleep(4000);
                    }
                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("search_radio_name")).Click();
                        driver.FindElement(By.Id("owner_name")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "Owner", driver, "OH", "Hamilton");
                        IWebElement IOwner = driver.FindElement(By.XPath("/html/body/div[1]/div[3]/div[2]/div[3]/form[1]/div/div[3]/button[1]"));
                        IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                        js.ExecuteScript("arguments[0].click();", IOwner);
                        Thread.Sleep(6000);
                        gc.CreatePdf_WOP(orderNumber, "Owner Search Result", driver, "OH", "Hamilton");

                        try
                        {
                            //MultiParcel
                            string strMulti = driver.FindElement(By.Id("search-results_info")).Text;
                            string strMultiCount = gc.Between(strMulti, "Showing 1 to ", " of");
                            if (Convert.ToInt32(strMultiCount) >= 25)
                            {
                                HttpContext.Current.Session["multiParcel_OHHamilton_count"] = "Maximum";
                                return "Maximum";
                            }
                            if (!strMulti.Contains("Showing 1 to 1 of 1 entries") && Convert.ToInt32(strMultiCount) <= 25)
                            {
                                IWebElement IMultiElement = driver.FindElement(By.XPath("/html/body/div/div[3]/div/div/div[2]/div[1]/table/tbody"));
                                IList<IWebElement> IMultiRow = driver.FindElements(By.TagName("tr"));
                                IList<IWebElement> IMultiTD;
                                foreach (IWebElement row in IMultiRow)
                                {
                                    IMultiTD = row.FindElements(By.TagName("td"));
                                    if (IMultiTD.Count != 0 && !row.Text.Contains("Parcel Number"))
                                    {
                                        strParcelNumber = IMultiTD[0].Text;
                                        strName = IMultiTD[1].Text;
                                        strAddress = IMultiTD[2].Text;

                                        string strMultiDetails = strName + "~" + strAddress;
                                        gc.insert_date(orderNumber, strParcelNumber, 256, strMultiDetails, 1, DateTime.Now);
                                    }
                                }
                            }
                            driver.Quit();
                            HttpContext.Current.Session["multiparcel_OHHamilton"] = "Yes";
                            return "MultiParcel";
                        }
                        catch { }

                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.Id("search-results"));
                        if(INodata.Text.Contains("No data available in table"))
                        {
                            HttpContext.Current.Session["Nodata_OHHamilton"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    //Assessment Details
                    Thread.Sleep(6000);
                    strParcelNumber = driver.FindElement(By.XPath("/html/body/div/div[3]/div[1]/div[1]")).Text;
                    strparcelNo = GlobalClass.After(strParcelNumber, "Parcel ID\r\n");
                    gc.CreatePdf(orderNumber, strparcelNo, "Tax Result", driver, "OH", "Hamilton");
                    strProperty = driver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/table[1]/tbody/tr[3]/td[1]/div[2]")).Text;
                    string[] addres = strProperty.Split(addressSplit, StringSplitOptions.None);
                    strOwnerName = addres[0];
                    string straddress = driver.FindElement(By.XPath("/html/body/div/div[3]/div[1]/div[2]")).Text;
                    strPropertyAddress = GlobalClass.After(straddress, "Address\r\n");
                    //string strMailing = driver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/table[1]/tbody/tr[3]/td[2]/div[2]")).Text;
                    //string[] strMail = strMailing.Split(addressSplit, StringSplitOptions.None);
                    strMailingAddress = addres[1] + " " + addres[2];
                    strPropertyType = driver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/table[1]/tbody/tr[2]/td[2]/div[2]")).Text;
                    strYearBuild = driver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/div/table[1]/tbody/tr[1]/td[2]")).Text;
                    strLegalDiscription = driver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/table[1]/tbody/tr[5]/td/div[2]")).Text;

                    string strAssessmentDeatils = strOwnerName + "~" + strPropertyAddress + "~" + strMailingAddress + "~" + strPropertyType + "~" + strYearBuild + "~" + strLegalDiscription;
                    gc.insert_date(orderNumber, strparcelNo, 263, strAssessmentDeatils, 1, DateTime.Now);

                    strTaxDistrict = driver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/table[1]/tbody/tr[1]/td[1]/div[2]")).Text;
                    strTax = driver.FindElement(By.XPath("/html/body/div/div[3]/div[1]/div[4]")).Text;
                    strTaxYear = GlobalClass.After(strTax, "Tax Year\r\n");
                    strSchool = driver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/table[1]/tbody/tr[1]/td[1]/div[4]")).Text;

                    strBOR = driver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/div/table[2]/tbody/tr[1]/td[2]")).Text;
                    strRR = driver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/div/table[2]/tbody/tr[2]/td[2]")).Text;
                    strHS = driver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/div/table[2]/tbody/tr[3]/td[2]")).Text;
                    strOOC = driver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/div/table[2]/tbody/tr[4]/td[2]")).Text;
                    strFC = driver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/div/table[2]/tbody/tr[5]/td[2]")).Text;
                    strSA = driver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/div/table[2]/tbody/tr[6]/td[2]")).Text;
                    strCAUV = driver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/div/table[2]/tbody/tr[8]/td[2]")).Text;
                    strTIF = driver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/div/table[2]/tbody/tr[11]/td[2]")).Text;
                    strAdpted = driver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/div/table[2]/tbody/tr[12]/td[2]")).Text;
                    strExcempt = driver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/div/table[2]/tbody/tr[13]/td[2]")).Text;


                    IWebElement IValue = driver.FindElement(By.XPath("/html/body/div/div[4]/div/div[2]/div[1]/a[5]"));
                    string strValue = IValue.GetAttribute("href");
                    driver.Navigate().GoToUrl(strValue);
                    Thread.Sleep(6000);
                    gc.CreatePdf(orderNumber, strparcelNo, "Tax Value Details Result", driver, "OH", "Hamilton");
                    IWebElement IValueTable = driver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/table/tbody"));
                    IList<IWebElement> IvalueRow = IValueTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> IValuetd;
                    foreach (IWebElement values in IvalueRow)
                    {
                        IValuetd = values.FindElements(By.TagName("td"));
                        if (IValuetd.Count != 0 && !values.Text.Contains("Tax Year"))
                        {
                            strValueYear = IValuetd[0].Text;
                            strValueAssDate = IValuetd[1].Text;
                            strValueLand = IValuetd[2].Text;
                            strValueImprove = IValuetd[3].Text;
                            strValueTotal = IValuetd[4].Text;
                            strValueCAUV = IValuetd[5].Text;
                            strValueReason = IValuetd[6].Text;

                            string strValueDetails = strValueYear + "~" + strValueAssDate + "~" + strValueLand + "~" + strValueImprove + "~" + strValueTotal + "~" + strValueCAUV + "~" + strValueReason;
                            gc.insert_date(orderNumber, strparcelNo, 267, strValueDetails, 1, DateTime.Now);
                        }
                    }

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    IWebElement IPayment = driver.FindElement(By.XPath("/html/body/div/div[4]/div/div[2]/div[1]/a[7]"));
                    string strIpayment = IPayment.GetAttribute("href");
                    driver.Navigate().GoToUrl(strIpayment);
                    Thread.Sleep(6000);
                    gc.CreatePdf(orderNumber, strparcelNo, "Tax Payment Details Result", driver, "OH", "Hamilton");
                    IWebElement ITaxInfo = driver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/div[2]/table[2]/tbody"));
                    IList<IWebElement> ItaxRow = ITaxInfo.FindElements(By.TagName("tr"));
                    IList<IWebElement> Itaxtd;
                    foreach (IWebElement info in ItaxRow)
                    {
                        Itaxtd = info.FindElements(By.TagName("td"));
                        if (Itaxtd.Count != 0 && !Itaxtd[1].Text.Contains("Prior\r\nDelinquent"))
                        {
                            strTaxInfoType = Itaxtd[0].Text;
                            strPrriorDelinquent = Itaxtd[1].Text;
                            strAdjDelinquent = Itaxtd[2].Text;
                            strFirstHalf = Itaxtd[3].Text;
                            strAdjFirstHalf = Itaxtd[4].Text;
                            strSecondHalf = Itaxtd[5].Text;
                            strAdjSecondHalf = Itaxtd[6].Text;

                            string strTaxInfoDetails = "Current Year Tax Detail" + "~" + strTaxInfoType + "~" + strPrriorDelinquent + "~" + strAdjDelinquent + "~" + strFirstHalf + "~" + strAdjFirstHalf + "~" + strSecondHalf + "~" + strAdjSecondHalf;
                            gc.insert_date(orderNumber, strparcelNo, 268, strTaxInfoDetails, 1, DateTime.Now);
                        }
                    }

                    //Delinquent Tax Details
                    try
                    {
                        for (int i = 3; i < 6; i++)
                        {
                            IWebElement ITaxSAInfo = driver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/div[2]/table[" + i + "]"));
                            IList<IWebElement> ItaxSARow = ITaxSAInfo.FindElements(By.TagName("tr"));
                            IList<IWebElement> ItaxSAtd;
                            foreach (IWebElement SAinfo in ItaxSARow)
                            {
                                ItaxSAtd = ITaxSAInfo.FindElements(By.TagName("th"));
                                if (SAinfo.Text.Contains("Special Assessment Detail"))
                                {
                                    SAInfoType = ItaxSAtd[0].Text;
                                }
                                ItaxSAtd = SAinfo.FindElements(By.TagName("td"));
                                if (ItaxSAtd.Count != 0 && !ItaxSAtd[1].Text.Contains("Prior\r\nDelinquent") && SAInfoType.Contains("Special Assessment Detail"))
                                {
                                    strTaxInfoType = ItaxSAtd[0].Text;
                                    strPrriorDelinquent = ItaxSAtd[1].Text;
                                    strAdjDelinquent = ItaxSAtd[2].Text;
                                    strFirstHalf = ItaxSAtd[3].Text;
                                    strAdjFirstHalf = ItaxSAtd[4].Text;
                                    strSecondHalf = ItaxSAtd[5].Text;
                                    strAdjSecondHalf = ItaxSAtd[6].Text;

                                    string strTaxInfoDetails = SAInfoType + "~" + strTaxInfoType + "~" + strPrriorDelinquent + "~" + strAdjDelinquent + "~" + strFirstHalf + "~" + strAdjFirstHalf + "~" + strSecondHalf + "~" + strAdjSecondHalf;
                                    gc.insert_date(orderNumber, strparcelNo, 268, strTaxInfoDetails, 1, DateTime.Now);
                                }
                            }
                            SAInfoType = "";
                        }
                    }
                    catch { }

                    try
                    {
                        for (int i = 3; i < 6; i++)
                        {
                            IWebElement ITaxPayment = driver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/div[2]/table[" + i + "]"));
                            IList<IWebElement> IPaymentRow = ITaxPayment.FindElements(By.TagName("tr"));
                            IList<IWebElement> IPaymenttd;
                            foreach (IWebElement pay in IPaymentRow)
                            {
                                IPaymenttd = pay.FindElements(By.TagName("th"));
                                if (pay.Text.Contains("Payment Information"))
                                {
                                    strPay = IPaymenttd[0].Text;
                                }
                                IPaymenttd = pay.FindElements(By.TagName("td"));
                                if (IPaymenttd.Count != 0 && !pay.Text.Contains("Date") && !pay.Text.Contains("Prior\r\nDelinquent") && strPay.Contains("Payment Information"))
                                {
                                    strPayDate = IPaymenttd[0].Text;
                                    strPayHalf = IPaymenttd[1].Text;
                                    strPrior = IPaymenttd[2].Text;
                                    strPayFirstHalf = IPaymenttd[3].Text;
                                    strPaySecondHalf = IPaymenttd[4].Text;
                                    strSurplus = IPaymenttd[5].Text;

                                    string strTaxInfoDetails = strPayDate + "~" + strPayHalf + "~" + strPrior + "~" + strPayFirstHalf + "~" + strPaySecondHalf + "~" + strSurplus;
                                    gc.insert_date(orderNumber, strparcelNo, 271, strTaxInfoDetails, 1, DateTime.Now);
                                }
                            }
                            strPay = "";
                        }
                    }
                    catch { }


                    IWebElement ITaxOver = driver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/div[2]/table[1]/tbody/tr/td[2]/table/tbody"));
                    IList<IWebElement> ITaxOverRow = ITaxOver.FindElements(By.TagName("tr"));
                    IList<IWebElement> ITaxOvertd;
                    foreach (IWebElement over in ITaxOverRow)
                    {
                        ITaxOvertd = over.FindElements(By.TagName("td"));
                        if (ITaxOvertd.Count != 0 && !over.Text.Contains("Prior Delinquent"))
                        {
                            if (ITaxOvertd[0].Text.Contains("\r\nNote: "))
                            {
                                string strOver = ITaxOvertd[0].Text;
                                strOverTitle = GlobalClass.Before(strOver, "\r\nNote: ");
                            }
                            else
                            {
                                strOverTitle = ITaxOvertd[0].Text;
                            }
                            strOverValue = ITaxOvertd[1].Text;

                            string strTaxInfoDetails = strOverTitle + "~" + strOverValue + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-";
                            gc.insert_date(orderNumber, strparcelNo, 268, strTaxInfoDetails, 1, DateTime.Now);
                        }
                    }

                    string strTaxAuthority = driver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/div[2]/table[1]/tbody/tr/td[1]/table[1]/tbody/tr[1]/td[2]")).Text;
                    gc.insert_date(orderNumber, strparcelNo, 268, "Tax Authority" + "~" + strTaxAuthority.Replace("\r\n", " ") + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-", 1, DateTime.Now);

                    IWebElement ITaxDis = driver.FindElement(By.XPath("/html/body/div/div[4]/div/div[2]/div[1]/a[8]"));
                    string strTaxDis = ITaxDis.GetAttribute("href");
                    driver.Navigate().GoToUrl(strTaxDis);
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, strparcelNo, "Tax Distribution Details Result", driver, "OH", "Hamilton");

                    strMarketLand = driver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/div/div[2]/div[1]/table/tbody/tr[1]/td[2]")).Text;
                    strMarketImprove = driver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/div/div[2]/div[1]/table/tbody/tr[2]/td[2]")).Text;
                    strMarketTotal = driver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/div/div[2]/div[1]/table/tbody/tr[3]/td[2]")).Text;
                    strAssessLand = driver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/div/div[2]/div[1]/table/tbody/tr[1]/td[4]")).Text;
                    strAssessImprove = driver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/div/div[2]/div[1]/table/tbody/tr[2]/td[4]")).Text;
                    strAssessTotal = driver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/div/div[2]/div[1]/table/tbody/tr[3]/td[4]")).Text;

                    string strAssessmentDetails = strTaxDistrict + "~" + strSchool + "~" + strTaxYear + "~" + strMarketLand + "~" + strMarketImprove + "~" + strMarketTotal + "~" + strAssessLand + "~" + strAssessImprove + "~" + strAssessTotal + "~" + strBOR + "~" + strRR + "~" + strHS + "~" + strOOC + "~" + strFC + "~" + strSA + "~" + strCAUV + "~" + strTIF + "~" + strAdpted + "~" + strExcempt;
                    gc.insert_date(orderNumber, strparcelNo, 266, strAssessmentDetails, 1, DateTime.Now);

                    IWebElement ITaxRate = driver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/div/div[2]/div[1]/table/tbody"));
                    IList<IWebElement> ITaxRateRow = ITaxRate.FindElements(By.TagName("tr"));
                    IList<IWebElement> ITaxRatetd;
                    foreach (IWebElement rate in ITaxRateRow)
                    {
                        ITaxRatetd = rate.FindElements(By.TagName("td"));
                        if (ITaxRatetd.Count != 0)
                        {
                            try
                            {
                                strRateTitle = ITaxRatetd[4].Text;
                                strRateValue = ITaxRatetd[5].Text;
                            }
                            catch
                            {
                                strRateTitle = ITaxRatetd[1].Text;
                                strRateValue = ITaxRatetd[2].Text;
                            }

                            string strTaxRateDetails = "Tax Rate Information" + "~" + strRateTitle + "~" + strRateValue;
                            gc.insert_date(orderNumber, strparcelNo, 270, strTaxRateDetails, 1, DateTime.Now);
                        }
                    }

                    IWebElement ITaxCal = driver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/div/div[2]/div[3]/table[1]/tbody"));
                    IList<IWebElement> ITaxCalRow = ITaxCal.FindElements(By.TagName("tr"));
                    IList<IWebElement> ITaxCaltd;
                    foreach (IWebElement cal in ITaxCalRow)
                    {
                        ITaxCaltd = cal.FindElements(By.TagName("td"));
                        if (ITaxCaltd.Count != 0)
                        {
                            strcalTitle = ITaxCaltd[0].Text;
                            strcalValue = ITaxCaltd[1].Text;

                            string strTaxCalDetails = "Tax Calculations" + "~" + strcalTitle + "~" + strcalValue;
                            gc.insert_date(orderNumber, strparcelNo, 270, strTaxCalDetails, 1, DateTime.Now);
                        }
                    }

                    IWebElement ITaxHalf = driver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/div/div[2]/div[3]/table[2]/tbody"));
                    IList<IWebElement> ITaxHalfRow = ITaxHalf.FindElements(By.TagName("tr"));
                    IList<IWebElement> ITaxHalftd;
                    foreach (IWebElement Half in ITaxHalfRow)
                    {
                        ITaxHalftd = Half.FindElements(By.TagName("td"));
                        if (ITaxHalftd.Count != 0)
                        {
                            strHalfTitle = ITaxHalftd[0].Text;
                            strHalfValue = ITaxHalftd[1].Text;

                            string strTaxHalfDetails = " Half Year Tax Distributions" + "~" + strHalfTitle + "~" + strHalfValue;
                            gc.insert_date(orderNumber, strparcelNo, 270, strTaxHalfDetails, 1, DateTime.Now);
                        }
                    }

                    try
                    {
                        IWebElement IReport = driver.FindElement(By.XPath("//*[@id='print-links']/a[2]"));
                        string strReport = IReport.GetAttribute("href");
                        driver.Navigate().GoToUrl(strReport);
                        string parent = driver.WindowHandles.Last();
                        driver.SwitchTo().Window(parent);
                        driver.FindElement(By.XPath("//*[@id='content']/form/div/p[13]/input")).SendKeys(Keys.Enter);
                        //string strLink = driver.Url;
                        gc.CreatePdf(orderNumber, strparcelNo, "Property Report", driver, "OH", "Hamilton");
                        //gc.downloadfile(strReport,orderNumber, strparcelNo,"Property Report", "OH", "Hamilton");
                    }
                    catch { }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "OH", "Hamilton", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);


                    driver.Quit();
                    gc.mergpdf(orderNumber, "OH", "Hamilton");
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