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
    public class WebDriver_DouglasNE
    {

        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_DouglasNE(string houseno, string Direction, string sname, string stype, string unitNumber, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string address = "", lastName = "", firstName = "", Pinnumber = "", PropertyAdd = "", Strownername = "", Pin = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            //     using (driver = new ChromeDriver())
            {

                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("http://douglascone.wgxtreme.com/?d=1");
                    Thread.Sleep(4000);

                    if (searchType == "titleflex")
                    {
                        if (Direction != "")
                        {
                            address = houseno + " " + Direction + " " + sname + " " + stype + " " + unitNumber;
                            address = address.Trim();
                        }
                        if (Direction == "")
                        {
                            address = houseno + " " + sname + " " + stype + " " + unitNumber;
                            address = address.Trim();
                        }
                        string titleaddress = address;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "NE", "Douglas");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Zero_DouglasNE"] = "Zero";
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
                            address = houseno + " " + Direction + " " + sname + " " + stype + " " + unitNumber;
                        }
                        if (Direction != "" && stype == "")
                        {
                            address = houseno + " " + Direction + " " + sname + " " + unitNumber;
                        }
                        if (Direction == "" && stype != "")
                        {
                            address = houseno + " " + sname + " " + stype + " " + unitNumber;
                        }
                        if (Direction == "" && stype == "")
                        {
                            address = houseno + " " + sname + " " + unitNumber;
                        }
                        driver.FindElement(By.Id("ext-comp-1013")).SendKeys(address.Trim());
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "NE", "Douglas");
                        driver.FindElement(By.XPath("//*[@id='ext-gen311']")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Address search Result", driver, "NE", "Douglas");

                        try
                        {
                            int Max = 0;
                            string strowner = "", strAddress = "", strCity = "";
                            string Record = "";
                            IWebElement multiaddress;

                            gc.CreatePdf(orderNumber, parcelNumber, "Parcel search Result", driver, "NE", "Douglas");

                            multiaddress = driver.FindElement(By.XPath("//*[@id='ext-gen683']"));

                            IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> multiTD;
                            foreach (IWebElement multi in multiRow)
                            {
                                multiTD = multi.FindElements(By.TagName("td"));
                                if (multiTD.Count != 0 && multiRow.Count >= 1 && multiRow.Count <= 25 && multi.Text.Trim() != "")
                                {
                                    Strownername = multiTD[1].Text;

                                    parcelNumber = multiTD[0].Text.Trim();
                                    PropertyAdd = multiTD[2].Text + " " + multiTD[3].Text;

                                    string multidetails = Strownername + "~" + PropertyAdd;
                                    gc.insert_date(orderNumber, parcelNumber, 1715, multidetails, 1, DateTime.Now);
                                    Max++;
                                }
                                if (multiTD.Count != 0 && multiRow.Count > 25)
                                {
                                    HttpContext.Current.Session["multiparcel_DouglasNE_Maximum"] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }

                            }
                            if (Max > 1 && Max < 26)
                            {
                                HttpContext.Current.Session["multiparcel_DouglasNE"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Max == 1)
                            {
                                string nparcel = "R" + parcelNumber;
                                string url = "http://douglascone.wgxtreme.com/java/wgx_douglasne/static/accountinfo.jsp?accountno=" + nparcel;
                                driver.Navigate().GoToUrl(url);
                                Thread.Sleep(4000);
                            }
                            if (Max == 0)
                            {
                                HttpContext.Current.Session["Zero_DouglasNE"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }

                        }
                        catch { }
                    }



                    else if (searchType == "parcel")
                    {

                        driver.FindElement(By.Id("ext-comp-1016")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search", driver, "NE", "Douglas");
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='ext-gen311']")).SendKeys(Keys.Enter);
                            Thread.Sleep(7000);
                        }
                        catch { }
                        try
                        {
                            driver.FindElement(By.LinkText("Submit")).Click();
                            Thread.Sleep(7000);
                        }
                        catch { }
                        try
                        {
                            int Max = 0;
                            string strowner = "", strAddress = "", strCity = "";
                            string Record = "";
                            IWebElement multiaddress;

                            gc.CreatePdf(orderNumber, parcelNumber, "Parcel search Result", driver, "NE", "Douglas");

                            multiaddress = driver.FindElement(By.XPath("//*[@id='ext-gen683']/div/table/tbody"));

                            IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> multiTD;
                            foreach (IWebElement multi in multiRow)
                            {
                                multiTD = multi.FindElements(By.TagName("td"));
                                if (multiTD.Count != 0 && multiRow.Count >= 1 && multiRow.Count <= 25 && multi.Text.Trim() != "")
                                {
                                    Strownername = multiTD[1].Text;

                                    parcelNumber = multiTD[0].Text;
                                    PropertyAdd = multiTD[2].Text + " " + multiTD[3].Text;

                                    string multidetails = Strownername + "~" + PropertyAdd;
                                    gc.insert_date(orderNumber, parcelNumber, 1715, multidetails, 1, DateTime.Now);
                                    Max++;
                                }
                                if (multiTD.Count != 0 && multiRow.Count > 25)
                                {
                                    HttpContext.Current.Session["multiparcel_DouglasNE_Maximum"] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }

                            }
                            if (Max > 1 && Max < 26)
                            {
                                HttpContext.Current.Session["multiparcel_DouglasNE"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Max == 1)
                            {
                                string nparcel = "R" + parcelNumber;
                                string url = "http://douglascone.wgxtreme.com/java/wgx_douglasne/static/accountinfo.jsp?accountno=" + nparcel;
                                driver.Navigate().GoToUrl(url);
                                Thread.Sleep(7000);
                            }
                            if (Max == 0)
                            {
                                HttpContext.Current.Session["Zero_DouglasNE"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                            // }
                        }
                        catch { }
                    }

                    if (searchType == "ownername")
                    {

                        driver.FindElement(By.Id("ext-comp-1015")).SendKeys(ownername);
                        Thread.Sleep(3000);

                        gc.CreatePdf_WOP(orderNumber, "OwnerName search", driver, "NE", "Douglas");
                        driver.FindElement(By.Id("ext-gen311")).Click();
                        Thread.Sleep(4000);

                        try
                        {
                            int Max = 0;
                            string strowner = "", strAddress = "", strCity = "";
                            string Record = "";
                            IWebElement multiaddress;

                            gc.CreatePdf(orderNumber, parcelNumber, "Parcel search Result", driver, "NE", "Douglas");

                            multiaddress = driver.FindElement(By.XPath("//*[@id='ext-gen683']"));
                            //*[@id="ext-gen683"]
                            IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> multiTD;
                            foreach (IWebElement multi in multiRow)
                            {
                                multiTD = multi.FindElements(By.TagName("td"));
                                if (multiTD.Count != 0 && multiRow.Count >= 1 && multiRow.Count <= 25 && multi.Text.Trim() != "")
                                {
                                    Strownername = multiTD[1].Text;

                                    parcelNumber = multiTD[0].Text.Trim();
                                    PropertyAdd = multiTD[2].Text + " " + multiTD[3].Text;

                                    string multidetails = Strownername + "~" + PropertyAdd;
                                    gc.insert_date(orderNumber, parcelNumber, 1715, multidetails, 1, DateTime.Now);
                                    Max++;
                                }
                                if (multiTD.Count != 0 && multiRow.Count > 25)
                                {
                                    HttpContext.Current.Session["multiparcel_DouglasNE_Maximum"] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }

                            }
                            if (Max > 1 && Max < 26)
                            {
                                HttpContext.Current.Session["multiparcel_DouglasNE"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Max == 1)
                            {
                                string nparcel = "R" + parcelNumber;
                                string url = "http://douglascone.wgxtreme.com/java/wgx_douglasne/static/accountinfo.jsp?accountno=" + nparcel;
                                driver.Navigate().GoToUrl(url);
                                Thread.Sleep(4000);
                            }
                            if (Max == 0)
                            {
                                HttpContext.Current.Session["Zero_DouglasNE"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }

                        }
                        catch { }
                    }


                    //property details

                    string MailingAddress = "", KeyNumber = "", AccountType = "", LegalDesc = "";
                    string OwnerName = "", PropertyAddress = "", Acres = "", YearBuilt = "";


                    parcelNumber = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[3]/td")).Text;
                    OwnerName = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td")).Text;
                    // MailingAddress = driver.FindElement(By.XPath("")).Text;
                    KeyNumber = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[1]/td")).Text;
                    AccountType = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[2]/td")).Text;
                    PropertyAddress = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[4]/td")).Text;
                    LegalDesc = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[5]/td")).Text;


                    //read all tables

                    IList<IWebElement> tables = driver.FindElements(By.XPath("/html/body/table"));
                    int count = tables.Count;
                    int j = 0;
                    foreach (IWebElement tab in tables)
                    {
                        if (tab.Text.Contains("Acres"))
                        {
                            IList<IWebElement> currtaxbill = tab.FindElements(By.XPath("tbody/tr"));
                            IList<IWebElement> TDCurrentTax;
                            foreach (IWebElement row in currtaxbill)
                            {
                                TDCurrentTax = row.FindElements(By.TagName("td"));
                                if (TDCurrentTax.Count != 0 && row.Text.Trim() != "" && !row.Text.Contains("Acres"))
                                {
                                    Acres = TDCurrentTax[0].Text;

                                }

                            }
                        }
                        else if (tab.Text.Contains("Year Built"))
                        {

                            IList<IWebElement> currtaxbill = tab.FindElements(By.XPath("tbody/tr"));
                            IList<IWebElement> TDCurrentTax;
                            foreach (IWebElement row in currtaxbill)
                            {
                                TDCurrentTax = row.FindElements(By.TagName("td"));
                                if (TDCurrentTax.Count != 0 && row.Text.Trim() != "" && !row.Text.Contains("Year Built"))
                                {
                                    YearBuilt = TDCurrentTax[0].Text;

                                }

                            }
                        }

                    }

                    string propertydetails = OwnerName + "~" + KeyNumber + "~" + AccountType + "~" + PropertyAddress + "~" + LegalDesc + "~" + Acres + "~" + YearBuilt;
                    gc.insert_date(orderNumber, parcelNumber, 1714, propertydetails, 1, DateTime.Now);

                    gc.CreatePdf(orderNumber, parcelNumber, "Property Details", driver, "NE", "Douglas");

                    // Assessment Details



                    try
                    {
                        IWebElement Assessment = driver.FindElement(By.XPath("/html/body/table[4]/tbody"));
                        IList<IWebElement> TRAssessment = Assessment.FindElements(By.TagName("tr"));
                        IList<IWebElement> THAssessment = Assessment.FindElements(By.TagName("th"));
                        IList<IWebElement> TDAssessment;
                        foreach (IWebElement row in TRAssessment)
                        {
                            TDAssessment = row.FindElements(By.TagName("td"));
                            if (TDAssessment.Count != 0)
                            {
                                string Assessmentdetails = TDAssessment[0].Text + "~" + TDAssessment[1].Text + "~" + TDAssessment[2].Text + "~" + TDAssessment[3].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1716, Assessmentdetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }


                    // Tax Information Details
                    string taxAuth = "", taxauth1 = "", taxauth2 = "";
                    try
                    {
                        driver.Navigate().GoToUrl("https://www.dctreasurer.org/contact-us");
                        taxAuth = driver.FindElement(By.XPath("//*[@id='content-bg']/div/div[1]/div[2]/div[1]/div[3]/p[4]")).Text.Replace("Street Address:", "").Replace("\r\n", " ").Trim();
                    }
                    catch { }
                    driver.Navigate().GoToUrl("https://dotcwsprodweb01.dotcomm.org/TreasTax/");
                    Thread.Sleep(5000);

                    driver.FindElement(By.Id("parcelNum")).SendKeys(parcelNumber);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Search", driver, "NE", "Douglas");
                    driver.FindElement(By.Id("btnSearch2")).Click();
                    Thread.Sleep(4000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Search Result", driver, "NE", "Douglas");

                    string TaxpayerMailAdd = "", TaxpayerMailAdd1 = "", TaxpayerMailAdd2 = "", Tax_keyno = "", strProAdd = "", strLegal = "", Taxyear = "", Taxable_Value = "", TaxLevy = "", TaxAmount = "";

                    TaxpayerMailAdd1 = driver.FindElement(By.XPath("//*[@id='content-bg']/div/div[1]/div[2]/div[1]/table/tbody/tr/td/table[1]/tbody/tr/td[2]/table[1]/tbody/tr/td/span")).Text;
                    TaxpayerMailAdd2 = driver.FindElement(By.XPath("//*[@id='content-bg']/div/div[1]/div[2]/div[1]/table/tbody/tr/td/table[1]/tbody/tr/td[2]/table[3]/tbody/tr/td/span")).Text;
                    TaxpayerMailAdd = TaxpayerMailAdd1 + " " + TaxpayerMailAdd2;
                    Tax_keyno = driver.FindElement(By.XPath("//*[@id='content-bg']/div/div[1]/div[2]/div[1]/table/tbody/tr/td/table[2]/tbody/tr[2]/td[2]/span")).Text;
                    strProAdd = driver.FindElement(By.XPath("//*[@id='content-bg']/div/div[1]/div[2]/div[1]/table/tbody/tr/td/table[2]/tbody/tr[3]/td[2]/span")).Text;
                    strLegal = driver.FindElement(By.XPath("//*[@id='content-bg']/div/div[1]/div[2]/div[1]/table/tbody/tr/td/table[2]/tbody/tr[4]/td[2]/span")).Text;
                    Taxyear = driver.FindElement(By.XPath("//*[@id='content-bg']/div/div[1]/div[2]/div[1]/table/tbody/tr/td/span[4]")).Text;
                    Taxable_Value = driver.FindElement(By.XPath("//*[@id='content-bg']/div/div[1]/div[2]/div[1]/table/tbody/tr/td/table[3]/tbody/tr[1]/td[2]/span")).Text;
                    TaxLevy = driver.FindElement(By.XPath("//*[@id='content-bg']/div/div[1]/div[2]/div[1]/table/tbody/tr/td/table[3]/tbody/tr[2]/td[2]/span")).Text;
                    TaxAmount = driver.FindElement(By.XPath("//*[@id='content-bg']/div/div[1]/div[2]/div[1]/table/tbody/tr/td/table[3]/tbody/tr[3]/td[2]/span")).Text;


                    try
                    {
                        string TaxInfodetails = TaxpayerMailAdd + "~" + Tax_keyno + "~" + strProAdd + "~" + strLegal + "~" + Taxyear + "~" + Taxable_Value + "~" + TaxLevy + "~" + TaxAmount + "~" + taxAuth;
                        gc.insert_date(orderNumber, parcelNumber, 1721, TaxInfodetails, 1, DateTime.Now);

                    }
                    catch { }


                    // Tax Payment History Details
                    try
                    {
                        IWebElement TaxPayment = driver.FindElement(By.XPath("//*[@id='payHist']/table[2]/tbody"));
                        IList<IWebElement> TRTaxPayment = TaxPayment.FindElements(By.TagName("tr"));
                        IList<IWebElement> THTaxPayment = TaxPayment.FindElements(By.TagName("th"));
                        IList<IWebElement> TDTaxPayment;
                        foreach (IWebElement row in TRTaxPayment)
                        {
                            TDTaxPayment = row.FindElements(By.TagName("td"));
                            if (TDTaxPayment.Count != 0 && !row.Text.Contains("Date Posted") && row.Text.Trim() != "")
                            {
                                string TaxPaymentdetails = TDTaxPayment[0].Text + "~" + TDTaxPayment[1].Text + "~" + TDTaxPayment[2].Text + "~" + TDTaxPayment[3].Text + "~" + TDTaxPayment[4].Text + "~" + TDTaxPayment[5].Text + "~" + TDTaxPayment[6].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1722, TaxPaymentdetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    // Tax Payment Worksheet Details


                    try
                    {
                        driver.FindElement(By.LinkText("Make a Payment")).Click();
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Payment Worksheet", driver, "NE", "Douglas");
                    }
                    catch { }



                    try
                    {
                        IWebElement Taxpayworksheet = driver.FindElement(By.XPath("//*[@id='paymentTable']/table[2]/tbody"));
                        IList<IWebElement> TRTaxpayworksheet = Taxpayworksheet.FindElements(By.TagName("tr"));
                        IList<IWebElement> THTaxpayworksheet = Taxpayworksheet.FindElements(By.TagName("th"));
                        IList<IWebElement> TDTaxpayworksheet;
                        foreach (IWebElement row in TRTaxpayworksheet)
                        {
                            TDTaxpayworksheet = row.FindElements(By.TagName("td"));
                            if (TDTaxpayworksheet.Count != 0 && !row.Text.Contains("Advertising") && row.Text.Trim() != "")
                            {
                                string Taxpayworkdetails = TDTaxpayworksheet[0].Text + "~" + TDTaxpayworksheet[1].Text + "~" + TDTaxpayworksheet[2].Text + "~" + TDTaxpayworksheet[3].Text + "~" + TDTaxpayworksheet[4].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1724, Taxpayworkdetails, 1, DateTime.Now);
                            }
                        }


                    }
                    catch { }
                    try
                    {
                        driver.Navigate().Back();
                        Thread.Sleep(3000);
                    }
                    catch { }

                    try
                    {
                        driver.FindElement(By.Id("btnSearch2")).Click();
                        Thread.Sleep(4000);
                    }
                    catch { }

                    try
                    {
                        driver.FindElement(By.LinkText("Levy Info")).Click();
                        Thread.Sleep(4000);
                        // gc.CreatePdf(orderNumber, parcelNumber, "Property Levy Information", driver, "NE", "Douglas");
                    }
                    catch { }
                    try
                    {
                        driver.SwitchTo().Window(driver.WindowHandles.Last());
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Property Levy Information", driver, "NE", "Douglas");
                    }
                    catch { }
                    // Tax Payer Information
                    try
                    {
                        IWebElement TaxLevyInfo = driver.FindElement(By.XPath("//*[@id='levyinfo']/table/tbody/tr/td/table/tbody"));
                        IList<IWebElement> TRTaxLevyInfo = TaxLevyInfo.FindElements(By.TagName("tr"));
                        IList<IWebElement> THTaxLevyInfo = TaxLevyInfo.FindElements(By.TagName("th"));
                        IList<IWebElement> TDTaxLevyInfo;
                        foreach (IWebElement row in TRTaxLevyInfo)
                        {
                            TDTaxLevyInfo = row.FindElements(By.TagName("td"));
                            if (TDTaxLevyInfo.Count != 0 && !row.Text.Contains("Advertising") && row.Text.Trim() != "")
                            {
                                string TaxLevydetails = TDTaxLevyInfo[0].Text + "~" + TDTaxLevyInfo[1].Text + "~" + TDTaxLevyInfo[2].Text + "~" + TDTaxLevyInfo[3].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1723, TaxLevydetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }









                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "NE", "Douglas", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "NE", "Douglas");
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