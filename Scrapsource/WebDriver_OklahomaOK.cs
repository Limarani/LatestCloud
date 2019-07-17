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
    public class WebDriver_OklahomaOK
    {

        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();

        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_OklahomaOK(string address, string parcelNumber, string searchType, string ownername, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                //   driver = new ChromeDriver();

                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("https://ariisp1.oklahomacounty.org/AssessorWP5/DefaultSearch.asp");
                    Thread.Sleep(4000);

                    if (searchType == "titleflex")
                    {
                        string titleaddress = address;
                        gc.TitleFlexSearch(orderNumber, "", address, "", "OK", "Oklahoma");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_Oklahoma"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {

                        driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[3]/td[2]/form/table/tbody/tr/td/input[1]")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "OK", "Oklahoma");
                        driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[3]/td[2]/form/table/tbody/tr/td/input[2]")).SendKeys(Keys.Enter);
                        Thread.Sleep(4000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search result", driver, "OK", "Oklahoma");
                        try
                        {
                            int Max = 0;
                            string strowner = "", strAddress = "", strCity = "";
                            string returnStatement = "";
                            IWebElement multiaddress = driver.FindElement(By.XPath("/html/body/table[4]/tbody"));
                            IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> multiTD;
                            foreach (IWebElement multi in multiRow)
                            {
                                multiTD = multi.FindElements(By.TagName("td"));
                                if (multiTD.Count != 0 && multiRow.Count <= 3 && !multi.Text.Contains("Tax Map Number") && !multi.Text.Contains("No physical address records returned."))
                                {
                                    IWebElement IParcelClick = multiTD[0].FindElement(By.TagName("a"));
                                    IParcelClick.Click();
                                    Thread.Sleep(3000);
                                    Max++;

                                }
                                if (multiTD.Count != 0 && multiRow.Count <= 3 && !multi.Text.Contains("Tax Map Number") && multi.Text.Contains("No physical address records returned."))
                                {
                                    HttpContext.Current.Session["Nodata_Oklahoma"] = "Yes";
                                    driver.Quit();
                                    return "No Data Found";

                                }
                                if (multiTD.Count != 0 && multiRow.Count > 25)
                                {
                                    HttpContext.Current.Session["multiparcel_Oklahoma_Maximum"] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }
                                if (multiTD.Count != 0 && multiRow.Count >= 4 && multiRow.Count <= 25 && !multi.Text.Contains("Tax Map Number") && multi.Text.Trim() != "")
                                {
                                    try
                                    {
                                        strowner = multiTD[1].Text;
                                        strAddress = multiTD[2].Text + " " + multiTD[3].Text;
                                        parcelNumber = multiTD[0].Text.Trim();

                                        string multidetails = strowner + "~" + strAddress;
                                        gc.insert_date(orderNumber, parcelNumber, 1590, multidetails, 1, DateTime.Now);
                                        Max++;
                                    }
                                    catch { }
                                    //try
                                    //{
                                    //    strowner = multiTD[2].Text.Trim();
                                    //    strAddress = multiTD[3].Text + ", " + multiTD[4].Text;
                                    //    parcelNumber = multiTD[0].Text.Trim();

                                    //    string multidetails = strowner + "~" + strAddress;
                                    //    gc.insert_date(orderNumber, parcelNumber, 1590, multidetails, 1, DateTime.Now);
                                    //    Max++;
                                    //}
                                    //catch { }
                                }
                            }
                            if (Max > 1 && Max < 26)
                            {
                                HttpContext.Current.Session["multiparcel_Oklahoma"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Max == 0)
                            {
                                HttpContext.Current.Session["Nodata_Oklahoma"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }


                    else if (searchType == "ownername")
                    {

                        driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[2]/td[2]/form/table/tbody/tr/td/input[1]")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "OwnerName search", driver, "OK", "Oklahoma");
                        driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[2]/td[2]/form/table/tbody/tr/td/input[2]")).SendKeys(Keys.Enter);
                        Thread.Sleep(4000);
                        gc.CreatePdf_WOP(orderNumber, "OwnerName Search result", driver, "OK", "Oklahoma");
                        try
                        {
                            int Max = 0;
                            string strowner = "", strAddress = "", strCity = "";
                            string returnStatement = "";
                            IWebElement multiaddress = driver.FindElement(By.XPath("/html/body/table[4]/tbody"));
                            IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> multiTD;
                            foreach (IWebElement multi in multiRow)
                            {
                                multiTD = multi.FindElements(By.TagName("td"));
                                if (multiTD.Count != 0 && multiRow.Count <= 3 && !multi.Text.Contains("Tax Map Number"))
                                {
                                    IWebElement IParcelClick = multiTD[0].FindElement(By.TagName("a"));
                                    IParcelClick.Click();
                                    Thread.Sleep(3000);
                                    Max++;

                                }
                                if (multiTD.Count != 0 && multiRow.Count > 25)
                                {
                                    HttpContext.Current.Session["multiparcel_Oklahoma_Maximum"] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }
                                if (multiTD.Count != 0 && multiRow.Count >= 4 && multiRow.Count <= 25 && !multi.Text.Contains("Tax Map Number") && multi.Text.Trim() != "")
                                {
                                    try
                                    {
                                        strowner = multiTD[2].Text.Trim();
                                        strAddress = multiTD[3].Text + ", " + multiTD[4].Text;
                                        parcelNumber = multiTD[0].Text.Trim();

                                        string multidetails = strowner + "~" + strAddress;
                                        gc.insert_date(orderNumber, parcelNumber, 1590, multidetails, 1, DateTime.Now);
                                        Max++;
                                    }
                                    catch { }
                                }
                            }
                            if (Max > 1 && Max < 26)
                            {
                                HttpContext.Current.Session["multiparcel_Oklahoma"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Max == 0)
                            {
                                HttpContext.Current.Session["Nodata_Oklahoma"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    else if (searchType == "parcel")
                    {
                        if (parcelNumber.Count() == 10)
                        {
                            driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[4]/td[2]/form/table/tbody/tr/td/input[1]")).Clear();
                        }
                        else
                        {
                            driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[4]/td[2]/form/table/tbody/tr/td/input[1]")).Clear();
                            parcelNumber = "R" + parcelNumber;
                        }
                        driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[4]/td[2]/form/table/tbody/tr/td/input[1]")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "parcel search", driver, "OK", "Oklahoma");
                        driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[4]/td[2]/form/table/tbody/tr/td/input[2]")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcelNumber, "parcel search Result", driver, "OK", "Oklahoma");
                        try
                        {

                            string returnStatement = "";
                            IWebElement multiaddress = driver.FindElement(By.XPath("/html/body/table[4]/tbody"));
                            IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> multiTD;
                            foreach (IWebElement multi in multiRow)
                            {
                                multiTD = multi.FindElements(By.TagName("td"));
                                if (multiTD.Count != 0 && multiRow.Count <= 3)
                                {
                                    IWebElement IParcelClick = multiTD[0].FindElement(By.TagName("a"));
                                    IParcelClick.Click();
                                    Thread.Sleep(3000);

                                }
                            }
                        }
                        catch { }

                        try
                        {
                            IWebElement INodata = driver.FindElement(By.XPath("/html/body/table[4]/tbody"));
                            if(INodata.Text.Contains("No records returned"))
                            {
                                HttpContext.Current.Session["Nodata_Oklahoma"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    //property details

                    gc.CreatePdf(orderNumber, parcelNumber, "Property Details1", driver, "OK", "Oklahoma");
                    string OwnerName1 = "", OwnerName2 = "", OwnerName = "", Propertylocation = "", legaldesc = "", landtype = "";
                    string Propertylocation1 = "", Propertylocation2 = "", Propertylocation3 = "", MailingAddress = "";
                    string PropertyAddress = "", ProAdd1 = "", ProAdd2 = "", ProAdd3 = "", YearBuilt = "";

                    IWebElement parcelNo = driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[1]/td[1]/font/font"));
                    parcelNumber = parcelNo.Text.Trim();
                    IWebElement StrOwnerName1 = driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[3]/td[2]/font"));
                    OwnerName1 = StrOwnerName1.Text.Trim();
                    IWebElement StrOwnerName2 = driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[4]/td[2]/font"));
                    OwnerName2 = StrOwnerName2.Text.Trim();
                    OwnerName = OwnerName1 + " & " + OwnerName2;
                    IWebElement StrPropertylocation1 = driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[5]/td[2]/font"));
                    Propertylocation1 = StrPropertylocation1.Text.Trim();
                    IWebElement StrPropertylocation2 = driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[6]/td[2]"));
                    Propertylocation2 = StrPropertylocation2.Text.Trim();
                    IWebElement StrPropertylocation3 = driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[7]/td[2]/font"));
                    Propertylocation3 = StrPropertylocation3.Text.Trim();
                    MailingAddress = Propertylocation1 + " " + Propertylocation2 + " " + Propertylocation3;

                    IWebElement IproAdd1 = driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[1]/td[5]/p/font"));
                    ProAdd1 = IproAdd1.Text.Trim();
                    IWebElement IproAdd2 = driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[2]/td[4]/p/font"));
                    ProAdd2 = IproAdd2.Text.Trim();

                    PropertyAddress = ProAdd1 + " " + ProAdd2;
                    try
                    {
                        IWebElement Strlegal = driver.FindElement(By.XPath("/html/body/table[5]/tbody/tr/td/font"));
                        legaldesc = Strlegal.Text.Replace("Full Legal Description:", "").Trim();
                    }
                    catch { }

                    IWebElement Strlandtype = driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[1]/td[2]/font/font"));
                    landtype = Strlandtype.Text.Trim();

                    YearBuilt = driver.FindElement(By.XPath("/html/body/table[13]/tbody/tr/td[5]/p/font")).Text;

                    string propertydetails = landtype + "~" + OwnerName + "~" + PropertyAddress + "~" + MailingAddress + "~" + legaldesc + "~" + YearBuilt;
                    gc.insert_date(orderNumber, parcelNumber, 1585, propertydetails, 1, DateTime.Now);


                    // Assessment Details
                    try
                    {
                        IWebElement Assessment = driver.FindElement(By.XPath("/html/body/table[7]"));
                        IList<IWebElement> TRAssessment = Assessment.FindElements(By.TagName("tr"));
                        IList<IWebElement> THAssessment = Assessment.FindElements(By.TagName("th"));
                        IList<IWebElement> TDAssessment;
                        foreach (IWebElement row1 in TRAssessment)
                        {
                            TDAssessment = row1.FindElements(By.TagName("td"));
                            if (!row1.Text.Contains("Market Value") && !row1.Text.Contains("Valuation History") && TDAssessment.Count != 0 && row1.Text.Trim() != "" && TDAssessment.Count >= 9)
                            {

                                string AssessDetails = TDAssessment[0].Text + "~" + TDAssessment[1].Text + "~" + TDAssessment[2].Text + "~" + TDAssessment[3].Text + "~" + TDAssessment[4].Text + "~" + TDAssessment[5].Text + "~" + TDAssessment[6].Text + "~" + TDAssessment[7].Text + "~" + TDAssessment[8].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1586, AssessDetails, 1, DateTime.Now);
                            }

                        }
                    }
                    catch { }
                    // Current Tax Details

                    string taxAuth = "";
                    string Tax_Year = "", ParcelNo = "";
                    int Syear = DateTime.Now.Year;
                    int Smonth = DateTime.Now.Month;
                    int iyear = 0;
                    int i = 0, j = 0, k = 0;
                    if (Smonth >= 9)
                    {
                        iyear = Syear;
                    }
                    else
                    {
                        iyear = Syear - 1;
                    }
                    for (i = 0; i <= 2; i++)
                    {
                        driver.Navigate().GoToUrl("https://docs.oklahomacounty.org/treasurer/PublicAccess.asp");
                        Thread.Sleep(5000);

                        driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[1]/td[1]/form/p/input[1]")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Search", driver, "OK", "Oklahoma");
                        driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[1]/td[1]/form/p/input[2]")).Click();
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Search Result", driver, "OK", "Oklahoma");

                        if (j == 0)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/div[1]/center/table/tbody/tr[2]/td/a")).Click();
                                Thread.Sleep(2000);
                            }
                            catch { }

                            // Taxing Authority


                            try
                            {
                                string taxAuth1 = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/div/center/table/tbody/tr[2]/td/table/tbody/tr[1]/td/b")).Text.Replace("\r\n", " ").Trim();
                                string taxAuth2 = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/div/center/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]")).Text.Replace("\r\n", " ").Trim();
                                taxAuth = taxAuth1 + " " + taxAuth2;
                                gc.CreatePdf(orderNumber, parcelNumber, "Tax Authority", driver, "OK", "Oklahoma");
                                j++;
                            }
                            catch { }
                            try
                            {
                                driver.Navigate().Back();
                                Thread.Sleep(2000);
                            }
                            catch { }
                        }
                        if (k == 0)
                        {
                            IWebElement Taxvalue = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/div[2]/center/table"));
                            IList<IWebElement> TRTaxvalue = Taxvalue.FindElements(By.TagName("tr"));
                            IList<IWebElement> THTaxvalue = Taxvalue.FindElements(By.TagName("th"));
                            IList<IWebElement> TDTaxvalue;
                            foreach (IWebElement row in TRTaxvalue)
                            {
                                TDTaxvalue = row.FindElements(By.TagName("td"));
                                if (!row.Text.Contains("CLICK TO PAY") && TDTaxvalue.Count != 0 && row.Text.Trim() != "")
                                {
                                    string taxhistory = TDTaxvalue[0].Text + "~" + TDTaxvalue[1].Text + "~" + TDTaxvalue[2].Text + "~" + TDTaxvalue[3].Text;
                                    gc.insert_date(orderNumber, parcelNumber, 1588, taxhistory, 1, DateTime.Now);


                                }

                            }
                            k++;
                        }


                        IWebElement TaxSearch = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/div[2]/center/table"));
                        IList<IWebElement> TRTaxSearch = TaxSearch.FindElements(By.TagName("tr"));
                        IList<IWebElement> THTaxSearch = TaxSearch.FindElements(By.TagName("th"));
                        IList<IWebElement> TDTaxSearch;
                        foreach (IWebElement row in TRTaxSearch)
                        {
                            TDTaxSearch = row.FindElements(By.TagName("td"));
                            if (!row.Text.Contains("CLICK TO PAY") && TDTaxSearch.Count != 0 && row.Text.Trim() != "" && TDTaxSearch[1].Text.Trim() == Convert.ToString(iyear))
                            {
                                IWebElement ITaxSearch = TDTaxSearch[1].FindElement(By.TagName("a"));
                                ITaxSearch.Click();
                                Thread.Sleep(2000);
                                gc.CreatePdf(orderNumber, parcelNumber, "Tax Information" + iyear, driver, "OK", "Oklahoma");
                                break;
                            }
                        }

                        string owner_Address = "", TaxYear = "", Taxtype = "";
                        try
                        {
                            IWebElement strOwnername = driver.FindElement(By.XPath("//*[@id='TABLE1']/tbody/tr[1]/td[1]"));
                            owner_Address = strOwnername.Text.Replace("\r\n", " ").Trim();
                            IWebElement ITaxYear = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/div/center/table/tbody/tr/td/div[1]/table/tbody/tr[2]/td[2]/b[2]"));
                            TaxYear = ITaxYear.Text;
                            IWebElement ITaxtype = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/div/center/table/tbody/tr/td/div[1]/table/tbody/tr[2]/td[3]/b[2]"));
                            Taxtype = ITaxtype.Text;
                        }
                        catch { }

                        string AssessedValue = "", ExemptAmount = "", NetValue = "", Rate = "", TaxAmount = "", NetPayments = "", TaxBalance = "", Cost = "";
                        string Interest = "", TotalAmountDue = "", PaidDate = "", SaleInformation = "";
                        try
                        {
                            string bulkdata = driver.FindElement(By.XPath("//*[@id='TABLE1']/tbody/tr[1]/td[2]/table/tbody")).Text;
                            AssessedValue = gc.Between(bulkdata, "Assessed Value", "Exempt Amount").Replace(":", "").Trim();
                            ExemptAmount = gc.Between(bulkdata, "Exempt Amount", "Net Value").Replace(":", "").Trim();
                            NetValue = gc.Between(bulkdata, "Net Value", "Rate").Replace(":", "").Trim();
                            Rate = gc.Between(bulkdata, "Rate/$1000:", "TAX AMOUNT").Replace(":", "").Trim();
                            TaxAmount = gc.Between(bulkdata, "TAX AMOUNT", "Net Payments").Replace(":", "").Trim();
                            NetPayments = gc.Between(bulkdata, "Net Payments", "Tax Balance").Replace(":", "").Trim();
                            TaxBalance = gc.Between(bulkdata, "Tax Balance", "Costs").Replace(":", "").Trim();
                            Cost = gc.Between(bulkdata, "Costs", "Interest").Replace(":", "").Trim();
                            Interest = gc.Between(bulkdata, "Interest", "TOTAL AMOUNT DUE IF PAID IMMEDIATELY").Replace(":", "").Trim();
                            TotalAmountDue = GlobalClass.After(bulkdata, "TOTAL AMOUNT DUE IF PAID IMMEDIATELY").Replace(":", "").Trim();
                        }
                        catch { }
                        try
                        {
                            SaleInformation = driver.FindElement(By.XPath("//*[@id='TABLE1']/tbody/tr[3]/td[2]")).Text.Replace("Sales/Endorsement Information", " ").Trim();
                        }
                        catch { }
                        try
                        {
                            if (TaxYear != "")
                            {
                                string Taxdetails = owner_Address + "~" + TaxYear + "~" + Taxtype + "~" + AssessedValue + "~" + ExemptAmount + "~" + NetValue + "~" + Rate + "~" + TaxAmount + "~" + NetPayments + "~" + TaxBalance + "~" + Cost + "~" + Interest + "~" + TotalAmountDue + "~" + SaleInformation + "~" + taxAuth;
                                gc.insert_date(orderNumber, parcelNumber, 1589, Taxdetails, 1, DateTime.Now);
                            }
                        }
                        catch { }

                        // Tax Payment Details
                        string ReceiptNo1 = "", PaidDate1 = "", PaidAmount1 = "";
                        string ReceiptNo2 = "", PaidDate2 = "", PaidAmount2 = "";
                        try
                        {
                            PaidDate = driver.FindElement(By.XPath("//*[@id='TABLE1']/tbody/tr[2]/td")).Text.Replace("Payments", "").Replace("\r\n", " ").Trim();
                            string[] split = PaidDate.Split(' ');
                            try
                            {
                                ReceiptNo1 = split[1].Replace("Payments", "");
                                PaidDate1 = split[2].Replace("Payments", "");
                                PaidAmount1 = split[3].Replace("Payments", "");
                            }
                            catch { }
                            try
                            {
                                ReceiptNo2 = split[5].Replace("Payments", "");
                                PaidDate2 = split[6].Replace("Payments", "");
                                PaidAmount2 = split[7].Replace("Payments", "");
                            }
                            catch { }
                            string TaxPaydetails1 = TaxYear + "~" + ReceiptNo1 + "~" + PaidDate1 + "~" + PaidAmount1;
                            gc.insert_date(orderNumber, parcelNumber, 1603, TaxPaydetails1, 1, DateTime.Now);

                            if (ReceiptNo2 != "")
                            {
                                string TaxPaydetails2 = TaxYear + "~" + ReceiptNo2 + "~" + PaidDate2 + "~" + PaidAmount2;
                                gc.insert_date(orderNumber, parcelNumber, 1603, TaxPaydetails2, 1, DateTime.Now);
                            }

                        }
                        catch { }

                        iyear--;

                    }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "OK", "Oklahoma", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "OK", "Oklahoma");
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