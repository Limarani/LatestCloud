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

namespace ScrapMaricopa
{
    public class WebDriver_SCCharleston
    {

        string parcelno = "";
        string outputPath = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();

        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_SCCharleston(string Address, string account, string parcelNumber, string searchType, string orderno, string ownername, string directParcel)
        {
            GlobalClass.global_orderNo = orderno;
            HttpContext.Current.Session["orderNo"] = orderno;
            GlobalClass.global_parcelNo = parcelNumber;
            string TaxYear = "", BillNumber = "", TaxBillId = "", Installment = "", DueDate = "", TaxAmount = "", Penalty = "", Fee = "", Intrest = "", DueAmount = "";
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                string[] stringSeparators1 = new string[] { "\r\n" };
                List<string> listurl = new List<string>();
                string Date = "";
                Date = DateTime.Now.ToString("M/d/yyyy");
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderno, parcelNumber, "", Address, "SC", "Charleston");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_SCCharleston"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("http://sc-charleston-county.governmax.com/svc/");
                        Thread.Sleep(2000);
                        IWebElement iframeElement = driver.FindElement(By.XPath("/html/frameset/frame"));
                        //now use the switch command
                        driver.SwitchTo().Frame(iframeElement);
                        driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[3]/td/a")).Click();
                        Thread.Sleep(4000);
                        driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[3]/td/a")).Click();
                        Thread.Sleep(4000);
                        driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr/td/table/tbody/tr[2]/td/font/input")).SendKeys(Address.Trim());
                        gc.CreatePdf_WOP(orderno, "Address Search", driver, "SC", "Charleston");

                        driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[3]/td/input")).SendKeys(Keys.Enter);

                        Thread.Sleep(12000);
                        gc.CreatePdf_WOP(orderno, "Address Search Result", driver, "SC", "Charleston");
                        try
                        {
                            IWebElement MultiTB = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[1]/tbody/tr/td/table/tbody"));
                            IList<IWebElement> MultiTR = MultiTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiTD;

                            foreach (IWebElement row1 in MultiTR)
                            {

                                MultiTD = row1.FindElements(By.TagName("td"));
                                if (MultiTD.Count != 0 && MultiTD.Count != 1 && !MultiTD[0].Text.Contains("Parcel ID"))
                                {
                                    parcelNumber = MultiTD[0].Text;
                                    Address = MultiTD[1].Text;
                                    ownername = MultiTD[2].Text;
                                    string MutiDetail = Address + "~" + ownername;
                                    gc.insert_date(orderno, parcelNumber, 212, MutiDetail, 1, DateTime.Now);
                                }
                            }
                            if (MultiTR.Count > 3)
                            {
                                HttpContext.Current.Session["multiParcel_SCCharleston"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                        }
                        catch { }



                    }

                    else if (searchType == "parcel")
                    {

                        driver.Navigate().GoToUrl("http://sc-charleston-county.governmax.com/svc/");

                        Thread.Sleep(2000);
                        IWebElement iframeElement = driver.FindElement(By.XPath("/html/frameset/frame"));
                        //now use the switch command
                        driver.SwitchTo().Frame(iframeElement);
                        //driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table[1]/tbody/tr/td[2]/table/tbody/tr/td/font/a")).Click();
                        driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[3]/td/a")).Click();

                        Thread.Sleep(4000);
                        driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/a")).Click();
                        Thread.Sleep(8000);
                        driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[1]/td/table/tbody/tr/td/table/tbody/tr[2]/td/font/input")).SendKeys(parcelNumber.Trim());
                        gc.CreatePdf(orderno, parcelNumber, "Parcel Name Search", driver, "SC", "Charleston");

                        driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[2]/td/input")).SendKeys(Keys.Enter);
                        Thread.Sleep(12000);
                        gc.CreatePdf(orderno, parcelNumber, "Parcel Name Search Result", driver, "SC", "Charleston");
                    }

                    else if (searchType == "ownername")
                    {
                        driver.Navigate().GoToUrl("http://sc-charleston-county.governmax.com/svc/");



                        Thread.Sleep(2000);
                        IWebElement iframeElement = driver.FindElement(By.XPath("/html/frameset/frame"));
                        //now use the switch command
                        driver.SwitchTo().Frame(iframeElement);
                        //driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table[1]/tbody/tr/td[2]/table/tbody/tr/td/font/a")).Click();
                        driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[3]/td/a")).Click();
                        Thread.Sleep(4000);

                        driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr/td/table/tbody/tr[2]/td/font/input")).SendKeys(ownername.Trim());
                        gc.CreatePdf_WOP(orderno, "Owner name Search", driver, "SC", "Charleston");
                        driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[3]/td/input")).SendKeys(Keys.Enter);
                        Thread.Sleep(15000);
                        gc.CreatePdf_WOP(orderno, "Owner Name Search Result", driver, "SC", "Charleston");
                        try
                        {
                            IWebElement MultiTB = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[1]/tbody/tr/td/table/tbody"));
                            IList<IWebElement> MultiTR = MultiTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiTD;

                            foreach (IWebElement row1 in MultiTR)
                            {

                                MultiTD = row1.FindElements(By.TagName("td"));
                                if (MultiTD.Count != 0 && MultiTD.Count != 1 && MultiTD[0].Text.Trim() != "Parcel ID")
                                {
                                    parcelNumber = MultiTD[0].Text;
                                    Address = MultiTD[1].Text;
                                    ownername = MultiTD[2].Text;
                                    string MutiDetail = Address + "~" + ownername;
                                    gc.insert_date(orderno, parcelNumber, 212, MutiDetail, 1, DateTime.Now);
                                }
                            }
                            if (MultiTR.Count > 3)
                            {
                                HttpContext.Current.Session["multiParcel_SCCharleston"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }

                        }
                        catch { }

                    }
                    driver.SwitchTo().DefaultContent();
                    IWebElement iframeElement1 = driver.FindElement(By.XPath("/html/frameset/frame"));
                    //now use the switch command
                    driver.SwitchTo().Frame(iframeElement1);
                    string AlternateParcel = "", PropertyAddress = "", MailingAddress = "", PropertyType = "", YearBuilt = "", LegalDescription = "";

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[1]/tbody/tr/td/table"));
                        if(INodata.Text.Contains("No Records Found"))
                        {
                            HttpContext.Current.Session["Nodata_SCCharleston"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }


                    ownername = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table[1]/tbody/tr/td/table[2]/tbody/tr/td/table[1]/tbody/tr[2]/td[1]/table/tbody/tr[1]/td[2]/font")).Text;
                    parcelNumber = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table[1]/tbody/tr/td/table[1]/tbody/tr[2]/td[1]/font/span")).Text;
                    AlternateParcel = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table[1]/tbody/tr/td/table[1]/tbody/tr[2]/td[2]/font/span")).Text;
                    PropertyAddress = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table[1]/tbody/tr/td/table[1]/tbody/tr[2]/td[3]/font/span")).Text;
                    MailingAddress = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table[1]/tbody/tr/td/table[2]/tbody/tr/td/table[1]/tbody/tr[2]/td[1]/table/tbody/tr[2]/td[2]/font")).Text;
                    gc.CreatePdf(orderno, parcelNumber, "Property Detail", driver, "SC", "Charleston");
                    LegalDescription = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table[1]/tbody/tr/td/table[2]/tbody/tr/td/table[1]/tbody/tr[3]/td/table/tbody/tr/td[2]/font")).Text;
                    PropertyType = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table[1]/tbody/tr/td/table[2]/tbody/tr/td/table[1]/tbody/tr[2]/td[2]/table/tbody/tr[1]/td[2]/font")).Text;
                    driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[5]/td/a")).Click();
                    Thread.Sleep(5000);
                    gc.CreatePdf(orderno, parcelNumber, "ImproveMent", driver, "SC", "Charleston");
                    try { YearBuilt = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[2]/tbody/tr/td/table/tbody/tr[3]/td[8]/font")).Text; }
                    catch { }


                    string PropertyDeatil = AlternateParcel + "~" + ownername + "~" + PropertyAddress + "~" + MailingAddress + "~" + PropertyType + "~" + LegalDescription + "~" + YearBuilt;
                    gc.insert_date(orderno, parcelNumber, 213, PropertyDeatil, 1, DateTime.Now);
                    driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[3]/td/a")).Click();
                    Thread.Sleep(10000);

                    string AssesYear = "", AppraisedLandValue = "", AppraisedBuildingValue = "", TotalAppraisedValue = "", CappedAppraisedValue = "", ExemptionAmount = "", TaxableValue = "", AssessmentRatio = "", AssessedValue = "";
                    gc.CreatePdf(orderno, parcelNumber, "Assessment Detail", driver, "SC", "Charleston");
                    AssesYear = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[2]/tbody/tr/td/table[5]/tbody/tr[2]/td[4]/font")).Text;
                    AppraisedLandValue = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[2]/tbody/tr/td/table[5]/tbody/tr[3]/td[4]/font")).Text;
                    AppraisedBuildingValue = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[2]/tbody/tr/td/table[5]/tbody/tr[4]/td[4]/font")).Text;
                    TotalAppraisedValue = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[2]/tbody/tr/td/table[5]/tbody/tr[5]/td[4]/font")).Text;
                    CappedAppraisedValue = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[2]/tbody/tr/td/table[5]/tbody/tr[6]/td[4]/font")).Text;
                    ExemptionAmount = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[2]/tbody/tr/td/table[5]/tbody/tr[7]/td[4]/font")).Text;
                    TaxableValue = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[2]/tbody/tr/td/table[5]/tbody/tr[8]/td[4]/font")).Text;
                    AssessmentRatio = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[2]/tbody/tr/td/table[5]/tbody/tr[9]/td[4]/font")).Text;
                    AssessedValue = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[2]/tbody/tr/td/table[5]/tbody/tr[10]/td[4]/font")).Text;

                    string AsseDeatil = AssesYear + "~" + AppraisedLandValue + "~" + AppraisedBuildingValue + "~" + TotalAppraisedValue + "~" + CappedAppraisedValue + "~" + ExemptionAmount + "~" + TaxableValue + "~" + AssessmentRatio + "~" + AssessedValue;
                    gc.insert_date(orderno, parcelNumber, 214, AsseDeatil, 1, DateTime.Now);

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[7]/td/a")).Click();
                    Thread.Sleep(10000);
                    try
                    {
                        string Deliq = driver.FindElement(By.XPath("//*[@id='tabsummary']/table/tbody/tr[2]/td/table/tbody/tr[2]/td/font")).Text.Replace("\r\n", "");


                        gc.CreatePdf(orderno, parcelNumber, "Tax Detail", driver, "SC", "Charleston");
                        TaxYear = driver.FindElement(By.XPath("//*[@id='tabsummary']/table/tbody/tr[1]/td/table/tbody/tr[1]/td[1]/font/b")).Text;
                        BillNumber = driver.FindElement(By.XPath("//*[@id='tabsummary']/table/tbody/tr[1]/td/table/tbody/tr[1]/td[2]/font/b")).Text;
                        TaxBillId = driver.FindElement(By.XPath("//*[@id='tabsummary']/table/tbody/tr[1]/td/table/tbody/tr[1]/td[3]/font/b")).Text;

                        TaxYear = TaxYear.Replace("Tax Year:", "");
                        BillNumber = BillNumber.Replace("Bill Number:", "");
                        TaxBillId = TaxBillId.Replace("TaxBillID:", "");
                        //Installment = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[2]/tbody/tr/td/table[5]/tbody/tr[5]/td[2]/font")).Text;
                        //DueDate = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[2]/tbody/tr/td/table[5]/tbody/tr[6]/td[2]/font")).Text;
                        //TaxAmount = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[2]/tbody/tr/td/table[5]/tbody/tr[7]/td[2]/font")).Text;
                        //Penalty = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[2]/tbody/tr/td/table[5]/tbody/tr[8]/td[2]/font")).Text;
                        //Fee = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[2]/tbody/tr/td/table[5]/tbody/tr[9]/td[2]/font")).Text;
                        //Intrest = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[2]/tbody/tr/td/table[5]/tbody/tr[10]/td[2]/font")).Text;
                        //DueAmount = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[2]/tbody/tr/td/table[5]/tbody/tr[10]/td[2]/font")).Text;
                        string TaxDeatil = TaxYear + "~" + BillNumber + "~" + TaxBillId + "~" + Installment + "~" + DueDate + "~" + TaxAmount + "~" + Penalty + "~" + Fee + "~" + Intrest + "~" + DueAmount;
                        if (Deliq.Trim() != "Year" && Penalty.Trim() != "Penalty")
                        {
                            gc.insert_date(orderno, parcelNumber, 215, TaxDeatil, 1, DateTime.Now);
                        }


                    }
                    catch
                    {

                    }

                    try
                    {
                        string Deliq = driver.FindElement(By.XPath("//*[@id='tabsummary']/table/tbody/tr[2]/td/table/tbody/tr[2]/td/font/span")).Text.Replace("\r\n", "");


                        gc.CreatePdf(orderno, parcelNumber, "Tax Detail", driver, "SC", "Charleston");
                        TaxYear = driver.FindElement(By.XPath("//*[@id='tabsummary']/table/tbody/tr[2]/td/table/tbody/tr[1]/td[1]/font/b")).Text;
                        BillNumber = driver.FindElement(By.XPath("//*[@id='tabsummary']/table/tbody/tr[2]/td/table/tbody/tr[1]/td[2]/font/b")).Text;
                        TaxBillId = driver.FindElement(By.XPath("//*[@id='tabsummary']/table/tbody/tr[2]/td/table/tbody/tr[1]/td[3]/font/b")).Text;

                        TaxYear = TaxYear.Replace("Tax Year:", "");
                        BillNumber = BillNumber.Replace("Bill Number:", "");
                        TaxBillId = TaxBillId.Replace("TaxBillID:", "");
                        //Installment = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[2]/tbody/tr/td/table[5]/tbody/tr[5]/td[2]/font")).Text;
                        //DueDate = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[2]/tbody/tr/td/table[5]/tbody/tr[6]/td[2]/font")).Text;
                        //TaxAmount = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[2]/tbody/tr/td/table[5]/tbody/tr[7]/td[2]/font")).Text;
                        //Penalty = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[2]/tbody/tr/td/table[5]/tbody/tr[8]/td[2]/font")).Text;
                        //Fee = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[2]/tbody/tr/td/table[5]/tbody/tr[9]/td[2]/font")).Text;
                        //Intrest = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[2]/tbody/tr/td/table[5]/tbody/tr[10]/td[2]/font")).Text;
                        //DueAmount = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[2]/tbody/tr/td/table[5]/tbody/tr[10]/td[2]/font")).Text;
                        string TaxDeatil = TaxYear + "~" + BillNumber + "~" + TaxBillId + "~" + Installment + "~" + DueDate + "~" + TaxAmount + "~" + Penalty + "~" + Fee + "~" + Intrest + "~" + DueAmount;
                        if (Deliq.Trim() != "Year" && Penalty.Trim() != "Penalty")
                        {
                            gc.insert_date(orderno, parcelNumber, 215, TaxDeatil, 1, DateTime.Now);
                        }


                    }
                    catch
                    {

                    }
                    try
                    {

                        TaxYear = driver.FindElement(By.XPath("//*[@id='tabsummary']/table/tbody/tr[1]/td/table/tbody/tr[1]/td[1]/font/b")).Text;
                        BillNumber = driver.FindElement(By.XPath("//*[@id='tabsummary']/table/tbody/tr[1]/td/table/tbody/tr[1]/td[2]/font/b")).Text;
                        TaxBillId = driver.FindElement(By.XPath("//*[@id='tabsummary']/table/tbody/tr[1]/td/table/tbody/tr[1]/td[3]/font/b")).Text;
                        // string Deliq = driver.FindElement(By.XPath("//*[@id='tabsummary']/table/tbody/tr[2]/td/table/tbody/tr[2]/td/font")).Text.Replace("\r\n", "");
                        TaxYear = TaxYear.Replace("Tax Year:", "");
                        BillNumber = BillNumber.Replace("Bill Number:", "");
                        TaxBillId = TaxBillId.Replace("TaxBillID:", "");


                        IWebElement InstallmentTB = driver.FindElement(By.XPath("//*[@id='tabsummary']/table/tbody/tr[1]/td/table/tbody"));
                        IList<IWebElement> InstallmentTR = InstallmentTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> InstallmentTD;

                        foreach (IWebElement row1 in InstallmentTR)
                        {

                            InstallmentTD = row1.FindElements(By.TagName("td"));
                            if (InstallmentTD.Count != 0 && InstallmentTD.Count != 1 && InstallmentTD.Count != 3 && InstallmentTD[0].Text.Trim() != "Period")
                            {

                                if (InstallmentTD[0].Text.Trim() == "Pay In Full:")
                                {
                                    Installment = InstallmentTD[0].Text;
                                    DueDate = "";
                                    TaxAmount = "";
                                    Penalty = "";
                                    Fee = "";
                                    Intrest = "";
                                    DueAmount = InstallmentTD[1].Text;
                                    string TaxDeatil = TaxYear + "~" + BillNumber + "~" + TaxBillId + "~" + Installment + "~" + DueDate + "~" + TaxAmount + "~" + Penalty + "~" + Fee + "~" + Intrest + "~" + DueAmount;
                                    gc.insert_date(orderno, parcelNumber, 215, TaxDeatil, 1, DateTime.Now);
                                }
                                else
                                {
                                    Installment = InstallmentTD[0].Text;
                                    DueDate = InstallmentTD[1].Text;
                                    TaxAmount = InstallmentTD[2].Text;
                                    Penalty = InstallmentTD[3].Text;
                                    Fee = InstallmentTD[4].Text;
                                    Intrest = InstallmentTD[5].Text;
                                    DueAmount = InstallmentTD[6].Text;
                                    string TaxDeatil = TaxYear + "~" + BillNumber + "~" + TaxBillId + "~" + Installment + "~" + DueDate + "~" + TaxAmount + "~" + Penalty + "~" + Fee + "~" + Intrest + "~" + DueAmount;
                                    if (Penalty.Trim() != "Penalty")
                                    {
                                        gc.insert_date(orderno, parcelNumber, 215, TaxDeatil, 1, DateTime.Now);

                                    }
                                }


                            }
                        }
                        gc.CreatePdf(orderno, parcelNumber, "Tax Detail", driver, "SC", "Charleston");

                        InstallmentTB = driver.FindElement(By.XPath("//*[@id='tabsummary']/table/tbody/tr[2]/td/table/tbody"));
                        InstallmentTR = InstallmentTB.FindElements(By.TagName("tr"));


                        foreach (IWebElement row in InstallmentTR)
                        {

                            InstallmentTD = row.FindElements(By.TagName("td"));
                            if (InstallmentTD.Count != 0 && InstallmentTD.Count != 1 && InstallmentTD.Count != 3 && InstallmentTD[0].Text.Trim() != "Year")
                            {

                                if (InstallmentTD[0].Text.Trim() == "Total Delinquent:")
                                {
                                    TaxYear = InstallmentTD[0].Text;
                                    BillNumber = "";
                                    Installment = "";
                                    DueDate = "";
                                    TaxAmount = "";
                                    Penalty = "";
                                    Fee = "";
                                    Intrest = "";
                                    DueAmount = InstallmentTD[1].Text;
                                    string TaxDeatil = TaxYear + "~" + BillNumber + "~" + TaxBillId + "~" + Installment + "~" + DueDate + "~" + TaxAmount + "~" + Penalty + "~" + Fee + "~" + Intrest + "~" + DueAmount;
                                    if (Penalty.Trim() != "Penalty")
                                    {
                                        gc.insert_date(orderno, parcelNumber, 215, TaxDeatil, 1, DateTime.Now);
                                    }
                                }
                                else
                                {
                                    TaxYear = InstallmentTD[0].Text;
                                    BillNumber = InstallmentTD[1].Text;
                                    Installment = "";
                                    DueDate = "";
                                    TaxAmount = InstallmentTD[2].Text;
                                    Penalty = InstallmentTD[3].Text;
                                    Fee = InstallmentTD[4].Text;
                                    Intrest = InstallmentTD[5].Text;
                                    DueAmount = InstallmentTD[6].Text;

                                    string TaxDeatil = TaxYear.Replace("Tax Year:", "") + "~" + BillNumber.Replace("Bill Number:", "") + "~" + TaxBillId.Replace("TaxBillID:", "") + "~" + Installment + "~" + DueDate + "~" + TaxAmount + "~" + Penalty + "~" + Fee + "~" + Intrest + "~" + DueAmount;
                                    if (Penalty.Trim() != "Penalty")
                                    {
                                        gc.insert_date(orderno, parcelNumber, 215, TaxDeatil, 1, DateTime.Now);
                                    }

                                }
                            }
                        }




                    }
                    catch
                    {

                    }
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='form1']/table/tbody/tr[3]/td/a[1]")).Click();
                        //*[@id="main"]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr[2]/td/a[1]
                        Thread.Sleep(2000);

                        string Authority = "", Gross = "", Credits = "", Saving = "", Nettax = "";


                        string xpath = "//*[@id='tab_assmt_data_" + TaxBillId + "']/table/tbody";
                        xpath = xpath.Replace(" ", "");
                        IWebElement TaxDistributionTB = driver.FindElement(By.XPath(xpath));
                        IList<IWebElement> TaxDistributionTR = TaxDistributionTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> TaxDistributionTD;
                        gc.CreatePdf(orderno, parcelNumber, "Tax Assessment  Detail", driver, "SC", "Charleston");
                        foreach (IWebElement row1 in TaxDistributionTR)
                        {

                            TaxDistributionTD = row1.FindElements(By.TagName("td"));
                            if (TaxDistributionTD.Count != 0 && TaxDistributionTD.Count != 1 && TaxDistributionTD[0].Text != "Authority")
                            {
                                if (TaxDistributionTD[0].Text.Trim() == "Total Net Tax:")
                                {
                                    Authority = TaxDistributionTD[0].Text;
                                    Gross = "";
                                    Credits = "";
                                    Saving = "";
                                    Nettax = TaxDistributionTD[1].Text;
                                    string TaxPayMentHistory = Authority + "~" + Gross + "~" + Credits + "~" + Saving + "~" + Nettax;
                                    if (Authority.Trim() != "Authority")
                                    {
                                        gc.insert_date(orderno, parcelNumber, 216, TaxPayMentHistory, 1, DateTime.Now);
                                    }

                                }
                                else

                                {
                                    Authority = TaxDistributionTD[0].Text;
                                    Gross = TaxDistributionTD[1].Text;
                                    Credits = TaxDistributionTD[2].Text;
                                    Saving = TaxDistributionTD[3].Text;
                                    Nettax = TaxDistributionTD[4].Text;
                                    string TaxPayMentHistory = Authority + "~" + Gross + "~" + Credits + "~" + Saving + "~" + Nettax;
                                    if (Authority.Trim() != "Authority")
                                    {
                                        gc.insert_date(orderno, parcelNumber, 216, TaxPayMentHistory, 1, DateTime.Now);
                                    }
                                }

                            }

                        }

                    }
                    catch { }

                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='form1']/table/tbody/tr[3]/td/a[2]")).Click();
                        Thread.Sleep(2000);
                        string PaidDate = "", PaidAmount = "", ReceiptNumber = "";
                        IWebElement TaxPaymentTB = driver.FindElement(By.XPath("//*[@id='tab_pmt_data']/table/tbody"));
                        IList<IWebElement> TaxPaymentTR = TaxPaymentTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> TaxPaymentTD;
                        gc.CreatePdf(orderno, parcelNumber, "PayMent History Detail", driver, "SC", "Charleston");
                        foreach (IWebElement row1 in TaxPaymentTR)
                        {

                            TaxPaymentTD = row1.FindElements(By.TagName("td"));
                            if (TaxPaymentTD.Count != 0 && TaxPaymentTD.Count != 1)
                            {
                                PaidDate = TaxPaymentTD[0].Text;
                                PaidAmount = TaxPaymentTD[1].Text;
                                ReceiptNumber = TaxPaymentTD[2].Text;

                                if (PaidDate.Trim() != "Last Paid")
                                {
                                    string TaxPayMentHistory1 = PaidDate + "~" + PaidAmount + "~" + ReceiptNumber;
                                    gc.insert_date(orderno, parcelNumber, 217, TaxPayMentHistory1, 1, DateTime.Now);
                                }

                            }
                            if (TaxPaymentTD.Count == 1)
                            {

                                if (TaxPaymentTD[0].Text.Trim() == "No records found.")
                                {
                                    PaidDate = TaxPaymentTD[0].Text;
                                    PaidAmount = "";
                                    ReceiptNumber = "";
                                    string TaxPayMentHistory1 = PaidDate + "~" + PaidAmount + "~" + ReceiptNumber;
                                    gc.insert_date(orderno, parcelNumber, 217, TaxPayMentHistory1, 1, DateTime.Now);
                                }
                            }
                        }
                    }
                    catch { }
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");

                    gc.insert_TakenTime(orderno, "SC", "Charleston", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);


                    driver.Quit();
                    gc.mergpdf(orderno, "SC", "Charleston");
                    return "Data Inserted Successfully";


                }
                catch (Exception ex1)
                {

                    driver.Quit();
                    throw ex1;
                }
            }

        }

    }
}