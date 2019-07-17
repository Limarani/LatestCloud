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
    public class WebDriver_CAFresno
    {
        string Outparcelno = "", outputPath = "", strTaxPay = "", strpayment = "", strTaxPayAmount = "", strPayAmount = "", strTaxInstallF = "", strTaxInstallS = "", strparcelNumber = "";
        string[] strTaxPayment, strTaxPAmount;
        string strTaxYear = "-", strLand = "-", strImprove = "-", strMobile = "-", strPersonalProperty = "-", strExemption = "-", strNetTabeleValue = "-", strTaxArea = "-", strPestControl = "-", strAssessed = "-", strLocation = "-", strSecuredParcel = "",
            strTaxInstall1 = "-", strTaxInstalldetails1 = "-", strTaxInstall2 = "-", strTaxInstalldetails2 = "-", strsecured = "-", strSuppTaxYear = "-", strLandValue = "-", strImprovements = "-", strFixtures = "-", strMobileHome = "-", strPersonalProeprty = "-",
            strSuppExemption = "-", strSuppTotal = "-", strSuppNet = "-", strDTax = "-", strDParcelNumber = "-", strDDefault = "-", strDLocation = "-", strHomeLess = "-", strLessExemption = "-", strNet = "-", strTaxes = "-", strTaxeAmount = "-", strDTaxInform = "-", strDTaxS = "-", strDTaxI = "-", strDTaxSum = "-", strTaxsuppAmount = "-", strSuppParcelNumber = "-";
        string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
        IWebDriver driver;
        IList<IWebElement> taxPaymentdetails, taxPaymentAmountdetails, Itaxtd;
        List<string> strSecured = new List<string>();
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();

        public string FTP_CAFresno(string houseno, string sname, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
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
                        string address = houseno + " " + sname;
                        gc.TitleFlexSearch(orderNumber, parcelNumber, ownername, address, "CA", "Fresno");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else
                        {                            
                            if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                            {
                                HttpContext.Current.Session["Nodata_CAFresno"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                            //string strTitleAssess = HttpContext.Current.Session["TitleFlexAssess"].ToString();
                            parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                            //gc.insert_date(orderNumber, parcelNumber, 311, strTitleAssess, 1, DateTime.Now);
                            searchType = "parcel";
                            AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                        }
                    }
                    if (searchType == "parcel")
                    {
                        ParcelSearch(orderNumber, parcelNumber);
                        IWebElement ITaxTable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/form/table/tbody/tr[6]/td/table/tbody"));
                        IList<IWebElement> Itaxrow = ITaxTable.FindElements(By.TagName("input"));
                        foreach (IWebElement row in Itaxrow)
                        {
                            Itaxtd = row.FindElements(By.TagName("input"));
                            string strValue = row.GetAttribute("value");
                            if (strValue == "View Tax Info" || strValue == "View Payment Info")
                            {
                                string strSearch = row.GetAttribute("onclick");
                                if (strSearch != "" && strSearch.Contains("SecuredDetails") || strSearch.Contains("SupplementalDetails") || strSearch.Contains("SecuredDelinquentOptions"))
                                {
                                    if (strSearch.Contains("SecuredDetails"))
                                    {
                                        string strSplitUrl = WebDriverTest.Between(strSearch, "window.location='", "'");
                                        string strUrl = "https://www2.co.fresno.ca.us/0410/fresnottcpaymentapplication/" + strSplitUrl + "";
                                        strSecured.Add(strUrl);
                                    }
                                    if (strSearch.Contains("SupplementalDetails"))
                                    {
                                        string strSplitUrl = WebDriverTest.Between(strSearch, "window.location='", "'");
                                        string strUrl = "https://www2.co.fresno.ca.us/0410/fresnottcpaymentapplication/" + strSplitUrl + "";
                                        strSecured.Add(strUrl);
                                    }
                                    if (strSearch.Contains("SecuredDelinquentOptions"))
                                    {
                                        string strSplitUrl = WebDriverTest.Between(strSearch, "window.location='", "'");
                                        string strUrl = "https://www2.co.fresno.ca.us/0410/fresnottcpaymentapplication/" + strSplitUrl + "";
                                        strSecured.Add(strUrl);
                                    }
                                }
                            }
                        }
                        foreach (string URL in strSecured)
                        {
                            driver.Navigate().GoToUrl(URL);
                            if (URL.Contains("SecuredDetails"))
                            {
                                Thread.Sleep(3000);
                                TaxDetais(orderNumber, strparcelNumber, "Secured Property Tax");
                            }
                            if (URL.Contains("SupplementalDetails"))
                            {
                                Thread.Sleep(3000);
                                TaxDetais(orderNumber, strparcelNumber, "Supplemental Property Tax(s)");
                            }
                            if (URL.Contains("SecuredDelinquentOptions"))
                            {
                                Thread.Sleep(3000);
                                TaxDetais(orderNumber, strparcelNumber, "Secured Delinquent Property Tax(s)");
                            }
                        }
                    }
                    HttpContext.Current.Session["titleparcel"] = null;
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "CA", "Fresno", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);


                    driver.Quit();
                    gc.mergpdf(orderNumber, "CA", "Fresno");
                    return "Data Inserted Successfully";
                }
                catch (Exception ex)
                {
                    driver.Quit();
                    throw ex;
                }
            }
        }

        public void ParcelSearch(string orderNumber, string parcelNumber)
        {
            driver.Navigate().GoToUrl("https://www2.co.fresno.ca.us/0410/fresnottcpaymentapplication/Search.aspx");
            driver.FindElement(By.Id("StartSearch")).SendKeys(Keys.Enter);
            if (parcelNumber.Contains(" "))
            {
                strparcelNumber = parcelNumber.Replace(" ", "");
            }
            if (parcelNumber.Contains("-"))
            {
                strparcelNumber = parcelNumber.Replace("-", "");
            }

            string strParcelStart = strparcelNumber.Substring(0, 3);
            string strParcelMiddle = strparcelNumber.Substring(3, 3);
            string strParcelEnd = strparcelNumber.Substring(6, 2);
            driver.FindElement(By.Id("parcelnumber1")).SendKeys(strParcelStart);
            driver.FindElement(By.Id("parcelnumber2")).SendKeys(strParcelMiddle);
            driver.FindElement(By.Id("parcelnumber3")).SendKeys(strParcelEnd);
            gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearch", driver, "CA", "Fresno");
            driver.FindElement(By.Id("Submit")).SendKeys(Keys.Enter);
            try
            {
                IWebElement Iresult = driver.FindElement(By.XPath("//*[@id='ParcelLocked']"));
                HttpContext.Current.Session["result_CAfreshno"] = Iresult.Text; 
                return;
            }
            catch { }
            try
            {
                IWebElement Iresult = driver.FindElement(By.XPath("//*[@id='NoRows']"));
                HttpContext.Current.Session["result_CAfreshno"] = Iresult.Text;
                return;
            }
            catch { }
            gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearchResult", driver, "CA", "Fresno");
            Thread.Sleep(3000);
        }

        public void TaxDetais(string orderNumber, string parcelNumber, string strsecured)
        {
                if (strsecured == "Secured Property Tax")
                {
                    gc.CreatePdf(orderNumber, parcelNumber, "Secured Property TaxDetails", driver, "CA", "Fresno");
                    strSecuredParcel = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/form/table/tbody/tr/td/table/tbody/tr[4]/td/table/tbody/tr[3]/td/table/tbody/tr[1]/td/table/tbody/tr/td[3]/table/tbody/tr[2]/td")).Text;
                    strSecuredParcel = strSecuredParcel.Remove(strSecuredParcel.Length - 1);
                    string Year = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/form/table/tbody/tr/td/table/tbody/tr[4]/td/table/tbody/tr[3]/td/table/tbody/tr[1]/td/table/tbody/tr/td[2]")).Text;
                    string strTSplitYear = GlobalClass.After(Year, "FRESNO COUNTY SECURED PROPERTY TAX DETAILS\r\n");
                    strTaxYear = strTSplitYear.Substring(0, strTSplitYear.IndexOf("\r\nJULY"));
                    strLand = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/form/table/tbody/tr/td/table/tbody/tr[4]/td/table/tbody/tr[3]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td[1]")).Text;
                    strLand = strLand.Remove(strLand.Length - 1);
                    strImprove = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/form/table/tbody/tr/td/table/tbody/tr[4]/td/table/tbody/tr[3]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td[2]")).Text;
                    strImprove = strImprove.Remove(strImprove.Length - 1);
                    strMobile = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/form/table/tbody/tr/td/table/tbody/tr[4]/td/table/tbody/tr[3]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td[3]")).Text;
                    strMobile = strMobile.Remove(strMobile.Length - 1);
                    strPersonalProperty = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/form/table/tbody/tr/td/table/tbody/tr[4]/td/table/tbody/tr[3]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td[4]")).Text;
                    strPersonalProperty = strPersonalProperty.Remove(strPersonalProperty.Length - 1);
                    strExemption = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/form/table/tbody/tr/td/table/tbody/tr[4]/td/table/tbody/tr[3]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td[5]")).Text;
                    strExemption = strExemption.Remove(strExemption.Length - 1);
                    strNetTabeleValue = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/form/table/tbody/tr/td/table/tbody/tr[4]/td/table/tbody/tr[3]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td[6]")).Text;
                    strNetTabeleValue = strNetTabeleValue.Remove(strNetTabeleValue.Length - 1);
                    strTaxArea = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/form/table/tbody/tr/td/table/tbody/tr[4]/td/table/tbody/tr[3]/td/table/tbody/tr[5]/td/table/tbody/tr/td[2]")).Text;
                    strTaxArea = strTaxArea.Remove(strTaxArea.Length - 1);
                    strPestControl = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/form/table/tbody/tr/td/table/tbody/tr[4]/td/table/tbody/tr[3]/td/table/tbody/tr[5]/td/table/tbody/tr/td[4]")).Text;
                    strPestControl = strPestControl.Remove(strPestControl.Length - 1);
                    strAssessed = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/form/table/tbody/tr/td/table/tbody/tr[4]/td/table/tbody/tr[3]/td/table/tbody/tr[7]/td/table/tbody/tr/td[1]/table/tbody/tr[2]/td")).Text.Replace(".", "").Trim().TrimEnd();
                    strAssessed = strAssessed.Remove(strAssessed.Length - 1);
                    strLocation = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/form/table/tbody/tr/td/table/tbody/tr[4]/td/table/tbody/tr[3]/td/table/tbody/tr[7]/td/table/tbody/tr/td[2]/table/tbody/tr[2]/td")).Text;
                    strLocation = strLocation.Remove(strLocation.Length - 1);
                    strFixtures = "-";

                    string strTaxDetails = strsecured.Trim() + "~" + strTaxYear.Trim() + "~" + strLand.Trim() + "~" + strImprove.Trim() + "~" + strMobile.Trim() + "~" + strPersonalProperty.Trim() + "~" + strFixtures.Trim().Replace("\r\n", "") + "~" + strExemption.Trim().Replace("\r\n", "") + "~" + strNetTabeleValue.Trim().Replace("\r\n", "") + "~" + strTaxArea.Trim().Replace("\r\n", "") + "~" + strPestControl.Trim() + "~" + strAssessed.Trim() + "~" + strLocation.Trim() + "~" + "-" + "~" + "-" + "~" + "-";
                    gc.insert_date(orderNumber, strSecuredParcel, 103, strTaxDetails, 1, DateTime.Now);

                    IWebElement ItaxInstallTable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/form/table/tbody/tr/td/table/tbody/tr[4]/td/table/tbody/tr[3]/td/table/tbody/tr[13]/td/table/tbody"));
                    IList<IWebElement> IInstalltaxrow = ItaxInstallTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> IInstalltaxtd;
                    foreach (IWebElement Irows in IInstalltaxrow)
                    {
                        IInstalltaxtd = Irows.FindElements(By.TagName("td"));
                        if (IInstalltaxtd.Count != 0 && !Irows.Text.Contains("Parcel Number"))
                        {
                            try
                            {
                                if (IInstalltaxtd[0].Text.Contains("Installment"))
                                {
                                    strTaxInstall1 = IInstalltaxtd[0].Text.Replace("\r\n", "").Replace(".", "").Trim();
                                    strTaxInstall2 = IInstalltaxtd[2].Text.Replace("\r\n", "").Replace(".", "").Trim();
                                }
                                if (!IInstalltaxtd[1].Text.Contains(". ."))
                                {
                                    strTaxInstalldetails1 = "~" + IInstalltaxtd[1].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                    strTaxInstallF += strTaxInstalldetails1.Remove(strTaxInstalldetails1.Length - 1, 1);
                                    strTaxInstalldetails2 = "~" + IInstalltaxtd[3].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                    strTaxInstallS += strTaxInstalldetails2.Remove(strTaxInstalldetails2.Length - 1, 1);
                                }
                            }
                            catch { }
                        }
                        if (IInstalltaxtd[0].Text.Contains("Total Amount Due"))
                        {
                            string strTaxInstallFirst = strsecured.Trim() + "~" + strTaxInstall1.Trim() + strTaxInstallF.Trim();
                            gc.insert_date(orderNumber, strSecuredParcel, 105, strTaxInstallFirst, 1, DateTime.Now);
                            string strTaxInstallSecond = strsecured.Trim() + "~" + strTaxInstall2.Trim() + strTaxInstallS.Trim();
                            gc.insert_date(orderNumber, strSecuredParcel, 105, strTaxInstallSecond, 1, DateTime.Now);
                        }
                    }


                    IWebElement ItaxPaymentTable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/form/table/tbody/tr/td/table/tbody/tr[4]/td/table/tbody/tr[3]/td/table/tbody/tr[11]/td/table/tbody"));
                    IList<IWebElement> IPaymenttaxrow = ItaxPaymentTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> IPaymenttaxtd;
                    foreach (IWebElement Irows in IPaymenttaxrow)
                    {
                        IPaymenttaxtd = Irows.FindElements(By.TagName("td"));
                        if ((IPaymenttaxtd.Count != 0) && (IPaymenttaxtd[0].Text != "TAX PAYMENT IS DISTRIBUTED AS BELOW.") && (IPaymenttaxtd[0].Text != "TAXING AGENCIES / VOTER APPROVED BONDS / SPECIAL ASSESSMENTS."))
                        {
                            strTaxPay = IPaymenttaxtd[0].Text;
                            try
                            {
                                strPayAmount = IPaymenttaxtd[1].Text;
                                strPayAmount = strPayAmount.Remove(strPayAmount.Length - 1, 1);
                                if ((strPayAmount != "") && (!strPayAmount.Contains("\r\n")) && (strPayAmount.Contains("$")))
                                {
                                    strTaxes = strTaxPay.Replace("\r", "");
                                    strTaxeAmount = strPayAmount;
                                    string strTotalTax = strsecured + "~" + strTaxes + "~" + strTaxeAmount;
                                    gc.insert_date(orderNumber, strSecuredParcel, 104, strTotalTax, 1, DateTime.Now);
                                }
                                strTaxPayAmount = IPaymenttaxtd[3].Text;
                            }
                            catch { }
                        }
                        strTaxPayment = strTaxPay.Split('\n').ToArray();
                        strTaxPAmount = strTaxPayAmount.Split('\n').ToArray();
                        if ((IPaymenttaxtd[0].Text == "TAX PAYMENT IS DISTRIBUTED AS BELOW."))
                        {
                            string strTTaxYear = "TaxYear" + "~" + strTaxYear + "~" + "-";
                            gc.insert_date(orderNumber, strSecuredParcel, 104, strTTaxYear, 1, DateTime.Now);
                            string strTaxAuth = "TaxAuthority" + "~" + "Tax Collector P.O.Box 1192 Fresno, California 93715" + "~" + "-";
                            gc.insert_date(orderNumber, strSecuredParcel, 104, strTaxAuth, 1, DateTime.Now);
                        }
                        for (int i = 0; i < strTaxPayment.Length - 1; i++)
                        {
                            if ((IPaymenttaxtd[0].Text != "TAX PAYMENT IS DISTRIBUTED AS BELOW.") && (IPaymenttaxtd[0].Text != "TAXING AGENCIES / VOTER APPROVED BONDS / SPECIAL ASSESSMENTS."))
                            {
                                strTaxes = strTaxPayment[i].Replace("\r", "");
                                strTaxeAmount = strTaxPAmount[i].Replace("\r", "").Replace(" ", "");
                                string strTotalTax = strsecured + "~" + strTaxes + "~" + strTaxeAmount;
                                gc.insert_date(orderNumber, strSecuredParcel, 104, strTotalTax, 1, DateTime.Now);
                            }
                        }
                    }
                }
                else if (strsecured == "Supplemental Property Tax(s)")
                {
                    string strSuppLandValue = "-", strsuppImprovements = "", strsuppMobileHome = "", strsuppFixtures = "", strsuppHomeLess = "", strsuppLessExemption = "", strsuppNet = "", strsuppTaxArea = "", strsuppPestControl = "", strsuppAssessed = "", strsuppLocation = "", strTaxsuppInstalldetails1 = "",
                          strTaxsuppInstalldetails2 = "", strTaxsuppIns1 = "", strTaxsuppIns2 = "", strTaxsuppInstallF = "", strTaxsuppInstallS = "", strsuppTaxPay = "", strsuppPayAmount = "", strsuppTaxeAmount = "", strsuppTaxes = "", strsuppTaxPayAmount = "";
                    string[] strsuppTaxPayment, strsuppTaxPAmount;


                    strSuppParcelNumber = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/form/table/tbody/tr/td/table/tbody/tr[4]/td/table/tbody/tr[3]/td/table/tbody/tr[1]/td/table/tbody/tr/td[3]/table/tbody/tr[2]/td")).Text;
                    strSuppParcelNumber = strSuppParcelNumber.Remove(strSuppParcelNumber.Length - 1);
                    gc.CreatePdf(orderNumber, parcelNumber, "Supplemental Property TaxDetails" + strSuppParcelNumber + "", driver, "CA", "Fresno");
                    //strSuppTaxYear = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/form/table/tbody/tr/td/table/tbody/tr[4]/td/table/tbody/tr[3]/td/table/tbody/tr[1]/td/table/tbody/tr/td[3]/table/tbody/tr[2]/td")).Text;
                    strSuppLandValue = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/form/table/tbody/tr/td/table/tbody/tr[4]/td/table/tbody/tr[3]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td[2]")).Text;
                    strSuppLandValue = strSuppLandValue.Remove(strSuppLandValue.Length - 1);
                    strsuppImprovements = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/form/table/tbody/tr/td/table/tbody/tr[4]/td/table/tbody/tr[3]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td[3]")).Text;
                    strsuppImprovements = strsuppImprovements.Remove(strsuppImprovements.Length - 1);
                    strsuppMobileHome = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/form/table/tbody/tr/td/table/tbody/tr[4]/td/table/tbody/tr[3]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td[5]")).Text;
                    strsuppMobileHome = strsuppMobileHome.Remove(strsuppMobileHome.Length - 1);
                    strsuppFixtures = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/form/table/tbody/tr/td/table/tbody/tr[4]/td/table/tbody/tr[3]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td[4]")).Text;
                    strsuppFixtures = strsuppFixtures.Remove(strsuppFixtures.Length - 1);
                    strSuppTotal = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/form/table/tbody/tr/td/table/tbody/tr[4]/td/table/tbody/tr[3]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td[6]")).Text;
                    strSuppTotal = strSuppTotal.Remove(strSuppTotal.Length - 1);
                    strsuppHomeLess = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/form/table/tbody/tr/td/table/tbody/tr[4]/td/table/tbody/tr[3]/td/table/tbody/tr[3]/td/table/tbody/tr[6]/td[2]")).Text;
                    strsuppHomeLess = strsuppHomeLess.Remove(strsuppHomeLess.Length - 1);
                    strsuppLessExemption = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/form/table/tbody/tr/td/table/tbody/tr[4]/td/table/tbody/tr[3]/td/table/tbody/tr[3]/td/table/tbody/tr[7]/td[2]")).Text;
                    strsuppLessExemption = strsuppLessExemption.Remove(strsuppLessExemption.Length - 1);
                    strsuppNet = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/form/table/tbody/tr/td/table/tbody/tr[4]/td/table/tbody/tr[3]/td/table/tbody/tr[3]/td/table/tbody/tr[8]/td[2]")).Text;
                    strsuppNet = strsuppNet.Remove(strsuppNet.Length - 1);
                    strsuppTaxArea = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/form/table/tbody/tr/td/table/tbody/tr[4]/td/table/tbody/tr[3]/td/table/tbody/tr[5]/td/table/tbody/tr/td[2]")).Text;
                    strsuppTaxArea = strsuppTaxArea.Remove(strsuppTaxArea.Length - 1);
                    strsuppPestControl = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/form/table/tbody/tr/td/table/tbody/tr[4]/td/table/tbody/tr[3]/td/table/tbody/tr[5]/td/table/tbody/tr/td[4]")).Text;
                    strsuppPestControl = strsuppPestControl.Remove(strsuppPestControl.Length - 1);
                    strsuppAssessed = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/form/table/tbody/tr/td/table/tbody/tr[4]/td/table/tbody/tr[3]/td/table/tbody/tr[7]/td/table/tbody/tr/td[1]/table/tbody/tr[2]/td")).Text.Replace(" ", "").Replace("\r\n", "");
                    strsuppAssessed = strsuppAssessed.Remove(strsuppAssessed.Length - 1);
                    strsuppLocation = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/form/table/tbody/tr/td/table/tbody/tr[4]/td/table/tbody/tr[3]/td/table/tbody/tr[7]/td/table/tbody/tr/td[2]/table/tbody/tr[4]/td")).Text;
                    strsuppLocation = strsuppLocation.Remove(strsuppLocation.Length - 1);

                    string strTaxSupply = strsecured.Trim().TrimStart().TrimEnd() + "~" + strTaxYear.Trim().TrimStart().TrimEnd() + "~" + strSuppLandValue.Trim().TrimStart().TrimEnd() + "~" + strsuppImprovements.Trim().TrimStart().TrimEnd() + "~" + strsuppMobileHome.Trim().Replace(" ", "").Replace("\r\n", "") + "~" + "-" + "~" + strsuppFixtures.Trim().Replace(" ", "").Replace("\r\n", "") + "~" + "-" + "~" + strSuppTotal.Trim().Replace(" ", "").Replace("\r\n", "") + "~" + strsuppTaxArea.Trim().Replace(" ", "").Replace("\r\n", "") + "~" + strsuppPestControl.Trim().Replace(" ", "").Replace("\r\n", "") + "~" + strsuppAssessed.Trim() + "~" + strsuppLocation.Trim() + "~" + strsuppHomeLess + "~" + strsuppLessExemption + "~" + strsuppNet;
                    gc.insert_date(orderNumber, strSuppParcelNumber, 103, strTaxSupply, 1, DateTime.Now);
                    //string strHome = "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "Less HomeOwners Exemption" + "~" + strsuppHomeLess;
                    //gc.insert_date(orderNumber, strSuppParcelNumber, 103, strHome, 1, DateTime.Now);
                    //string strLExemption = "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "Less Others Exemption" + "~" + strsuppLessExemption;
                    //gc.insert_date(orderNumber, strSuppParcelNumber, 103, strLExemption, 1, DateTime.Now);
                    //string strNTax = "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "Net Tax Value" + "~" + strsuppNet;
                    //gc.insert_date(orderNumber, strSuppParcelNumber, 103, strNTax, 1, DateTime.Now);

                    IWebElement ItaxsuppInstallTable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/form/table/tbody/tr/td/table/tbody/tr[4]/td/table/tbody/tr[3]/td/table/tbody/tr[13]/td/table/tbody"));
                    IList<IWebElement> IInstalltaxsupprow = ItaxsuppInstallTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> IInstalltaxsupptd;
                    foreach (IWebElement Irows in IInstalltaxsupprow)
                    {
                        IInstalltaxsupptd = Irows.FindElements(By.TagName("td"));
                        if (IInstalltaxsupptd.Count != 0 && !Irows.Text.Contains("Parcel Number"))
                        {
                            try
                            {
                                if (IInstalltaxsupptd[0].Text.Contains("Installment"))
                                {
                                    strTaxsuppIns1 = IInstalltaxsupptd[0].Text.Replace("\r\n", "").Replace(".", "").Trim();
                                    try
                                    {
                                        strTaxsuppIns2 = IInstalltaxsupptd[1].Text.Replace("\r\n", "").Replace(".", "").Trim();
                                    }
                                    catch { }
                                    try
                                    {
                                        strTaxsuppIns2 = IInstalltaxsupptd[2].Text.Replace("\r\n", "").Replace(".", "").Trim();
                                    }
                                    catch { }
                                }
                                if (!IInstalltaxsupptd[1].Text.Contains(". .") && !IInstalltaxsupptd[0].Text.Contains("Installment"))
                                {
                                    strTaxsuppInstalldetails1 = "~" + IInstalltaxsupptd[1].Text.Replace("\r\n", " ").TrimEnd().TrimStart();
                                    strTaxsuppInstallF += strTaxsuppInstalldetails1.Remove(strTaxsuppInstalldetails1.Length - 1, 1);
                                    strTaxsuppInstalldetails2 = "~" + IInstalltaxsupptd[3].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                    strTaxsuppInstallS += strTaxsuppInstalldetails2.Remove(strTaxsuppInstalldetails2.Length - 1, 1);
                                }
                            }
                            catch { }
                        }
                        if (IInstalltaxsupptd[0].Text.Contains("Total Amount Due"))
                        {
                            string strTaxsuppInstallFirst = strsecured.Trim() + "~" + strTaxsuppIns1.Trim() + strTaxsuppInstallF.Trim();
                            gc.insert_date(orderNumber, strSuppParcelNumber, 105, strTaxsuppInstallFirst, 1, DateTime.Now);
                            string strTaxsuppInstallSecond = strsecured.Trim() + "~" + strTaxsuppIns2.Trim() + strTaxsuppInstallS.Trim();
                            gc.insert_date(orderNumber, strSuppParcelNumber, 105, strTaxsuppInstallSecond, 1, DateTime.Now);
                        }
                    }

                    IWebElement ItaxsuppPaymentTable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/form/table/tbody/tr/td/table/tbody/tr[4]/td/table/tbody/tr[3]/td/table/tbody/tr[11]/td/table/tbody"));
                    IList<IWebElement> IPaymenttaxsupprow = ItaxsuppPaymentTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> IPaymenttaxsupptd;
                    foreach (IWebElement Irows in IPaymenttaxsupprow)
                    {
                        IPaymenttaxsupptd = Irows.FindElements(By.TagName("td"));
                        if ((IPaymenttaxsupptd.Count != 0) && (IPaymenttaxsupptd[0].Text != "TAX PAYMENT IS DISTRIBUTED AS BELOW.") && (IPaymenttaxsupptd[0].Text != "TAXING AGENCIES / VOTER APPROVED BONDS / SPECIAL ASSESSMENTS."))
                        {
                            strsuppTaxPay = IPaymenttaxsupptd[0].Text;
                            try
                            {
                                strsuppPayAmount = IPaymenttaxsupptd[1].Text;
                                strsuppPayAmount = strsuppPayAmount.Remove(strsuppPayAmount.Length - 1, 1);
                                if ((strsuppPayAmount != "") && (!strPayAmount.Contains("\r\n")) && (strsuppPayAmount.Contains("$")))
                                {
                                    strsuppTaxes = strsuppTaxPay.Replace("\r", "");
                                    strsuppTaxeAmount = strsuppPayAmount;
                                    string strsuppTotalTax = strsecured + "~" + strsuppTaxes + "~" + strsuppTaxeAmount;
                                    gc.insert_date(orderNumber, strSuppParcelNumber, 104, strsuppTotalTax, 1, DateTime.Now);
                                }
                                strsuppTaxPayAmount = IPaymenttaxsupptd[3].Text;
                            }
                            catch { }
                        }
                        strsuppTaxPayment = strsuppTaxPay.Split('\n').ToArray();
                        strsuppTaxPAmount = strsuppTaxPayAmount.Split('\n').ToArray();
                        if ((IPaymenttaxsupptd[0].Text == "TAX PAYMENT IS DISTRIBUTED AS BELOW."))
                        {
                            string strTTaxYear = "TaxYear" + "~" + strTaxYear + "~" + "-";
                            gc.insert_date(orderNumber, strSuppParcelNumber, 104, strTTaxYear, 1, DateTime.Now);
                            string strTaxAuth = "TaxAuthority" + "~" + "Tax Collector P.O.Box 1192 Fresno, California 93715" + "~" + "-";
                            gc.insert_date(orderNumber, strSuppParcelNumber, 104, strTaxAuth, 1, DateTime.Now);
                        }
                        for (int i = 0; i < strsuppTaxPayment.Length - 1; i++)
                        {
                            if ((IPaymenttaxsupptd[0].Text != "TAX PAYMENT IS DISTRIBUTED AS BELOW.") && (IPaymenttaxsupptd[0].Text != "TAXING AGENCIES / VOTER APPROVED BONDS / SPECIAL ASSESSMENTS."))
                            {
                                strsuppTaxes = strsuppTaxPayment[i].Replace("\r", "");
                                strsuppTaxeAmount = strsuppTaxPAmount[i].Replace("\r", "").Replace(" ", "");
                                string strsuppTotalTax = strsecured + "~" + strsuppTaxes + "~" + strsuppTaxeAmount;
                                gc.insert_date(orderNumber, strSuppParcelNumber, 104, strsuppTotalTax, 1, DateTime.Now);
                            }
                        }
                    }
                }
                else if (strsecured == "Secured Delinquent Property Tax(s)")
                {
                    try
                    {
                        gc.CreatePdf(orderNumber, parcelNumber, "Secured Delinquent Property Tax(s) Details", driver, "CA", "Fresno");
                        strDParcelNumber = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/table/tbody/tr/td[1]/table/tbody/tr[2]/td[1]")).Text;
                        //strDParcelNumber = strDParcelNumber.Remove(strDParcelNumber.Length - 1);
                        strDDefault = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/table/tbody/tr/td[1]/table/tbody/tr[2]/td[2]")).Text;
                        //strDDefault = strDDefault.Remove(strDDefault.Length - 1);
                        strDLocation = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/table/tbody/tr/td[1]/table/tbody/tr[11]/td")).Text;
                        //strDLocation = strDLocation.Remove(strDLocation.Length - 1);
                        driver.FindElement(By.XPath("//*[@id='UnsecuredDetails']")).SendKeys(Keys.Enter);
                        //Tax Remeded
                        IWebElement ITaxDTable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/form/table/tbody/tr[3]/td/table/tbody/tr[2]/td/table/tbody/tr/td[3]/table/tbody"));
                        IList<IWebElement> ITaxDRow = ITaxDTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> ITaxDtd;
                        foreach (IWebElement Irow in ITaxDRow)
                        {
                            ITaxDtd = Irow.FindElements(By.TagName("td"));
                            if (ITaxDtd.Count != 0)
                            {
                                try
                                {
                                    strDTax = ITaxDtd[0].Text + "~" + ITaxDtd[1].Text;
                                    gc.insert_date(orderNumber, strDParcelNumber, 107, strDTax.Trim().Remove(strDTax.Length - 1), 1, DateTime.Now);
                                }
                                catch { }
                            }
                        }

                        //Tax Summary
                        IWebElement ITaxDeliTable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/form/table/tbody/tr[3]/td/table/tbody/tr[3]/td/table/tbody"));
                        IList<IWebElement> ITaxDeliRow = ITaxDeliTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> ITaxDeliutd;
                        foreach (IWebElement Irow in ITaxDeliRow)
                        {
                            ITaxDeliutd = Irow.FindElements(By.TagName("td"));
                            if (!Irow.Text.Contains("TAX SUMMARY") && !Irow.Text.Contains("ROLL YEAR"))
                            {
                                if (ITaxDeliutd.Count != 0 || ITaxDeliutd[0].Text.Contains("TOTAL"))
                                {
                                    for (int i = 0; i < ITaxDeliutd.Count; i++)
                                    {
                                        strDTaxI += ITaxDeliutd[i].Text + "~";
                                    }
                                    if (ITaxDeliutd[0].Text.Contains("TOTAL"))
                                    {
                                        strDTaxInform = "-" + "~" + "-" + "~" + strDTaxI.Remove(strDTaxI.Length - 1, 1);
                                        gc.insert_date(orderNumber, strDParcelNumber, 108, strDTaxInform.Trim(), 1, DateTime.Now);
                                    }
                                    else
                                    {
                                        strDTaxInform = strDTaxI.Remove(strDTaxI.Length - 1, 1);
                                        gc.insert_date(orderNumber, strDParcelNumber, 108, strDTaxInform.Trim(), 1, DateTime.Now);
                                    }
                                    strDTaxI = "";
                                }
                            }
                        }
                    }
                    catch { }
                }
        }
    }
}