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
using System.Linq;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_JacksonMO
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();

        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_Jackson(string address, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string TaxingAuthority = "";
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {

                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    // Tax Authority

                    try
                    {
                        driver.Navigate().GoToUrl("https://www.jacksongov.org/");
                        Thread.Sleep(4000);

                        string Taxdata = driver.FindElement(By.XPath("//*[@id='divInfoAdv4a53fb58-d028-47d8-88f0-e3a4adb4ebf3']/div[1]/div/div/ol/li")).Text.Replace("\r\n", " ");

                        TaxingAuthority = Taxdata.Replace("Contact Directory", "").Trim();
                    }
                    catch { }

                    // Property Details  
                    driver.Navigate().GoToUrl("https://ascendweb.jacksongov.org/ascend/(wk2eksmei2b2j1455lgftmfb)/search.aspx");
                    Thread.Sleep(4000);
                    gc.CreatePdf_WOP(orderNumber, "Home page", driver, "MO", "Jackson");

                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, "", ownername, address, "MO", "Jackson");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_JacksonMO"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";

                    }
                    if (searchType == "address")
                    {
                        driver.FindElement(By.Id("mSearchControl_mStreetAddress")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "MO", "Jackson");
                        driver.FindElement(By.Id("mSearchControl_mSubmit")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "MO", "Jackson");
                        try
                        {
                            string strparcel = "";
                            int s = 0;
                            int Max = 0;
                            IWebElement add_search = driver.FindElement(By.XPath("//*[@id='mResultscontrol_mGrid_RealDataGrid']/tbody"));
                            IList<IWebElement> TRadd_search = add_search.FindElements(By.TagName("tr"));
                            IList<IWebElement> THadd_search = add_search.FindElements(By.TagName("th"));
                            IList<IWebElement> TDadd_search;
                            foreach (IWebElement row in TRadd_search)
                            {
                                TDadd_search = row.FindElements(By.TagName("td"));
                                if (TRadd_search.Count >= 2 && TDadd_search.Count != 0 && !row.Text.Contains("Location Address"))
                                {

                                    string parcel_id = TDadd_search[0].Text;
                                    if (parcel_id.Trim().Replace("-", "").Count() == 17 && parcel_id.Trim().Replace("-", "").Count() != 9 && !row.Text.Contains("No Values Found"))
                                    {
                                        string Address = TDadd_search[1].Text;
                                        string ownername1 = TDadd_search[2].Text;
                                        string AddressDetails = ownername1 + "~" + Address;

                                        gc.insert_date(orderNumber, parcel_id, 1420, AddressDetails, 1, DateTime.Now);
                                        s++;
                                        strparcel = parcel_id;
                                        Max++;
                                    }
                                }

                            }
                            if (s == 1)
                            {
                                driver.FindElement(By.LinkText(strparcel)).SendKeys(Keys.Enter);
                                Thread.Sleep(5000);
                                gc.CreatePdf_WOP(orderNumber, "Property Details", driver, "MO", "Jackson");
                            }
                            if (s > 1 && s < 25)
                            {
                                gc.CreatePdf_WOP(orderNumber, "Multi Address search result", driver, "MO", "Jackson");
                                HttpContext.Current.Session["MOJackson"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (s > 25)
                            {
                                HttpContext.Current.Session["multiParcel_MOJackson_Multicount"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                            if (Max == 0)
                            {
                                HttpContext.Current.Session["Nodata_JacksonMO"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }


                        }
                        catch { }



                    }
                    else if (searchType == "ownername")
                    {
                        if (!ownername.Contains('*'))
                        {
                            ownername = ownername + "*";
                        }

                        driver.FindElement(By.Id("mSearchControl_mName")).SendKeys(ownername);
                        gc.CreatePdf(orderNumber, parcelNumber, "ownername search", driver, "MO", "Jackson");
                        driver.FindElement(By.Id("mSearchControl_mSubmit")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcelNumber, "ownername search Result", driver, "MO", "Jackson");
                        try
                        {
                            string strparcel = "";
                            int Max = 0;
                            IWebElement add_search = driver.FindElement(By.XPath("//*[@id='mResultscontrol_mGrid_RealDataGrid']/tbody"));
                            IList<IWebElement> TRadd_search = add_search.FindElements(By.TagName("tr"));
                            IList<IWebElement> THadd_search = add_search.FindElements(By.TagName("th"));
                            IList<IWebElement> TDadd_search;
                            foreach (IWebElement row in TRadd_search)
                            {
                                TDadd_search = row.FindElements(By.TagName("td"));
                                if (TRadd_search.Count > 3 && TDadd_search.Count != 0 && !row.Text.Contains("Location Address") && !row.Text.Contains("No Values Found"))
                                {
                                    string parcel_id = TDadd_search[0].Text;
                                    if (parcel_id.Trim().Replace("-", "").Count() == 17 && parcel_id.Trim().Replace("-", "").Count() != 9 && !row.Text.Contains("No Values Found"))
                                    {

                                        string Address = TDadd_search[2].Text;
                                        string ownername1 = TDadd_search[1].Text;
                                        string AddressDetails = ownername1 + "~" + Address;

                                        gc.insert_date(orderNumber, parcel_id, 1420, AddressDetails, 1, DateTime.Now);
                                        strparcel = parcel_id;
                                        Max++;
                                    }
                                }
                                if (TRadd_search.Count == 3)
                                {
                                    driver.FindElement(By.XPath("//*[@id='mResultscontrol_mGrid_RealDataGrid']/tbody/tr[3]/td[1]/a")).SendKeys(Keys.Enter);
                                    Thread.Sleep(5000);
                                    gc.CreatePdf_WOP(orderNumber, "Property Details", driver, "MO", "Jackson");
                                }
                            }
                            if (TRadd_search.Count < 27 && TRadd_search.Count > 3)
                            {
                                gc.CreatePdf_WOP(orderNumber, "Multi Address search result", driver, "MO", "Jackson");
                                HttpContext.Current.Session["MOJackson"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (TRadd_search.Count >= 27 && TRadd_search.Count > 3)
                            {
                                HttpContext.Current.Session["multiParcel_MOJackson_Multicount"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                            if (Max == 0)
                            {
                                HttpContext.Current.Session["Nodata_JacksonMO"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }

                        }
                        catch { }

                    }

                    else if (searchType == "parcel")
                    {
                        driver.FindElement(By.Id("mSearchControl_mParcelID")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "parcel search", driver, "MO", "Jackson");
                        driver.FindElement(By.Id("mSearchControl_mSubmit")).SendKeys(Keys.Enter);
                        Thread.Sleep(7000);
                        gc.CreatePdf(orderNumber, parcelNumber, "parcel search Result", driver, "MO", "Jackson");
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("mSearchControl_mSearchFieldsValidator")).Text;
                            if (nodata.Contains("does not exist"))
                            {
                                HttpContext.Current.Session["Nodata_JacksonMO"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    try
                    {
                        //No Data Found
                        string nodata = driver.FindElement(By.Id("Table4")).Text;
                        if (nodata.Contains("0 records returned") && nodata.Contains("No Values Found"))
                        {
                            HttpContext.Current.Session["Nodata_JacksonMO"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    string bulkdata = driver.FindElement(By.XPath("//*[@id='mTabGroup_Summary_mGeneralInformation_mGrid_RealDataGrid']/table/tbody")).Text;
                    string Property_Address = "", owner_name = "", PropertyDesc = "", PropertyCategory = "", Status = "", TaxCodeArea = "", PropertyClass = "", TaxPayer = "", MortgageCompany = "";
                    string bulkdata1 = driver.FindElement(By.XPath("//*[@id='ParcelSitusTable']/tbody")).Text;
                    parcelNumber = gc.Between(bulkdata1, "Parcel Number", "Property Address").Trim();
                    Property_Address = GlobalClass.After(bulkdata1, "Property Address").Trim();
                    PropertyDesc = gc.Between(bulkdata, "Property Description", "Property Category").Trim();
                    PropertyCategory = gc.Between(bulkdata, "Property Category", "Status").Trim().Replace(":", "");
                    Status = gc.Between(bulkdata, "Status", "Tax Code Area").Trim().Replace(":", "");
                    TaxCodeArea = GlobalClass.After(bulkdata, "Tax Code Area").Trim().Replace(":", "");
                    try
                    {
                        PropertyClass = driver.FindElement(By.XPath("//*[@id='mTabGroup_Summary_Propertycharacteristics1_mGrid_RealDataGrid']/tbody/tr/td[2]")).Text;
                    }
                    catch { }
                    try
                    {
                        TaxPayer = driver.FindElement(By.XPath("//*[@id='RealDataGrid']/tbody/tr[2]/td[3]")).Text;
                        owner_name = driver.FindElement(By.XPath("//*[@id='RealDataGrid']/tbody/tr[3]/td[3]")).Text;
                        MortgageCompany = driver.FindElement(By.XPath("//*[@id='RealDataGrid']/tbody/tr[4]/td[3]")).Text;
                    }
                    catch { }
                    string propertydetails = Property_Address + "~" + PropertyDesc + "~" + PropertyCategory + "~" + Status + "~" + TaxCodeArea + "~" + PropertyClass + "~" + TaxPayer + "~" + owner_name + "~" + MortgageCompany;
                    gc.insert_date(orderNumber, parcelNumber, 1407, propertydetails, 1, DateTime.Now);





                    // Assessment Details 

                    string taxyear = "", taxyear1 = "", valuetype = "", taxyear2 = "";
                    try
                    {
                        IWebElement Assessmentdetails = driver.FindElement(By.XPath("//*[@id='mTabGroup_Values_mValues_mGrid_RealDataGrid']/tbody"));
                        IList<IWebElement> TRAssessmentdetails = Assessmentdetails.FindElements(By.TagName("tr"));
                        IList<IWebElement> THAssessmentdetails = Assessmentdetails.FindElements(By.TagName("th"));
                        IList<IWebElement> TDAssessmentdetails;
                        foreach (IWebElement row in TRAssessmentdetails)
                        {
                            TDAssessmentdetails = row.FindElements(By.TagName("td"));

                            if (TDAssessmentdetails.Count != 0)
                            {
                                if (!row.Text.Contains("Value Type"))
                                {
                                    valuetype += TDAssessmentdetails[0].Text + "~";
                                }
                                if (!row.Text.Contains("No Values Found"))
                                {
                                    taxyear += TDAssessmentdetails[1].Text.Replace("Tax Year", "").Replace("\r\n", "") + "~";
                                    taxyear1 += TDAssessmentdetails[2].Text.Replace("Tax Year", "").Replace("\r\n", "") + "~";
                                    taxyear2 += TDAssessmentdetails[3].Text.Replace("Tax Year", "").Replace("\r\n", "") + "~";
                                }
                            }
                        }


                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + "Tax Year~" + valuetype.Remove(valuetype.Length - 1, 1) + "' where Id = '" + 1408 + "'");
                        gc.insert_date(orderNumber, parcelNumber, 1408, taxyear.Remove(taxyear.Length - 1, 1), 1, DateTime.Now);
                        gc.insert_date(orderNumber, parcelNumber, 1408, taxyear1.Remove(taxyear1.Length - 1, 1), 1, DateTime.Now);
                        gc.insert_date(orderNumber, parcelNumber, 1408, taxyear2.Remove(taxyear2.Length - 1, 1), 1, DateTime.Now);
                    }
                    catch (Exception ex) { }

                    // Tax Distribution Details



                    string bulkdata2 = driver.FindElement(By.XPath("//*[@id='mTabGroup_Summary_mGeneralInformation_mGrid_RealDataGrid']/table/tbody")).Text;


                    IList<IWebElement> TRTaxInfo1 = driver.FindElements(By.XPath("//*[@id='RealDataGrid']/tbody"));
                    IList<IWebElement> TDTaxInfo1, ValueTaxInfo1;
                    foreach (IWebElement row in TRTaxInfo1)
                    {
                        TDTaxInfo1 = row.FindElements(By.TagName("tr"));
                        if (TRTaxInfo1.Count > 1 && TDTaxInfo1.Count != 0 && row.Text.Contains("District") && row.Text.Trim() != "")
                        {
                            foreach (IWebElement row2 in TDTaxInfo1)
                            {
                                ValueTaxInfo1 = row2.FindElements(By.TagName("td"));

                                if (ValueTaxInfo1.Count > 1 && TDTaxInfo1.Count != 0 && !row2.Text.Contains("District") && row2.Text.Trim() != "" && ValueTaxInfo1.Count() == 2)
                                {

                                    string taxdetails1 = ValueTaxInfo1[0].Text + "~" + ValueTaxInfo1[1].Text;

                                    gc.insert_date(orderNumber, parcelNumber, 1409, taxdetails1, 1, DateTime.Now);

                                }

                            }
                        }
                    }







                    // Tax Payment History Details

                    try
                    {

                        IWebElement Taxpay = driver.FindElement(By.XPath("//*[@id='mTabGroup_Receipts_mReceipts_mGrid_RealDataGrid']/tbody"));
                        IList<IWebElement> TRTaxpay = Taxpay.FindElements(By.TagName("tr"));
                        IList<IWebElement> THTaxpay = Taxpay.FindElements(By.TagName("th"));
                        IList<IWebElement> TDTaxpay;
                        foreach (IWebElement row in TRTaxpay)
                        {
                            TDTaxpay = row.FindElements(By.TagName("td"));

                            if (TDTaxpay.Count != 0 && !row.Text.Contains("Receipt No."))
                            {
                                string taxPaymentdetails = TDTaxpay[1].Text + "~" + TDTaxpay[0].Text + "~" + TDTaxpay[2].Text + "~" + TDTaxpay[3].Text + "~" + TDTaxpay[4].Text + "~" + TDTaxpay[5].Text + "~" + TaxingAuthority;
                                gc.insert_date(orderNumber, parcelNumber, 1410, taxPaymentdetails, 1, DateTime.Now);
                            }
                        }

                    }
                    catch { }

                    // Tax Due Details
                    string strInterest = "";
                    try
                    {
                        IWebElement Taxdue = driver.FindElement(By.XPath("//*[@id='mGrid_RealDataGrid']/tbody"));
                        IList<IWebElement> TRTaxdue = Taxdue.FindElements(By.TagName("tr"));
                        IList<IWebElement> THTaxdue = Taxdue.FindElements(By.TagName("th"));
                        IList<IWebElement> TDTaxdue;
                        foreach (IWebElement row in TRTaxdue)
                        {
                            TDTaxdue = row.FindElements(By.TagName("td"));

                            if (TDTaxdue.Count != 0 && !row.Text.Contains("Cumulative Due"))
                            {
                                string taxPaymentdetails = TDTaxdue[0].Text + "~" + TDTaxdue[1].Text + "~" + TDTaxdue[2].Text + "~" + TDTaxdue[3].Text + "~" + TDTaxdue[4].Text + "~" + TDTaxdue[5].Text + "~" + TDTaxdue[6].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1419, taxPaymentdetails, 1, DateTime.Now);
                            }
                            if (TDTaxdue.Count != 0 && !row.Text.Contains("Cumulative Due") && TDTaxdue[4].Text != "0.00")
                            {
                                strInterest = "Delinquent";
                            }

                        }
                    }
                    catch { }

                    // Delinquent Search

                    string asofdate = "", Good_through_date = "";


                    if (strInterest == "Delinquent")
                    {

                        try
                        {
                            driver.FindElement(By.LinkText("Calculate Future Payoff")).SendKeys(Keys.Enter);
                            Thread.Sleep(4000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Calculate Pay off", driver, "MO", "Jackson");
                        }
                        catch { }



                        try
                        {
                            //Good Through Details

                            IWebElement good_date = driver.FindElement(By.Id("mFuturePayoff_mDate"));
                            Good_through_date = good_date.GetAttribute("value");
                            if (Good_through_date.Contains("Select A Date"))
                            {
                                Good_through_date = "-";
                            }
                            else
                            {

                                DateTime G_Date = Convert.ToDateTime(Good_through_date);
                                string dateChecking = DateTime.Now.ToString("MM") + "/15/" + DateTime.Now.ToString("yyyy");

                                if (G_Date < Convert.ToDateTime(dateChecking))
                                {
                                    //end of the month
                                    Good_through_date = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")))).ToString("MM/dd/yyyy");

                                }
                                else if (G_Date > Convert.ToDateTime(dateChecking))
                                {
                                    // nextEndOfMonth 
                                    if ((Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))) < 12))
                                    {
                                        Good_through_date = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM")) + 1), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")) + 1)).ToString("MM/dd/yyyy");
                                    }
                                    else
                                    {
                                        int nxtYr = Convert.ToInt16(DateTime.Now.ToString("yyyy")) + 1;
                                        Good_through_date = new DateTime(nxtYr, 1, DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), 1)).ToString("MM/dd/yyyy");

                                    }
                                }
                                driver.FindElement(By.Id("mFuturePayoff_mDate")).Clear();
                                Thread.Sleep(1000);
                                try
                                {
                                    driver.FindElement(By.XPath("//*[@id='mFuturePayoff_mDate']")).SendKeys(Good_through_date);
                                    Thread.Sleep(2000);
                                }
                                catch { }





                            }

                        }
                        catch
                        { }
                        try
                        {
                            driver.FindElement(By.Id("mFuturePayoff_mCalculate")).SendKeys(Keys.Enter);
                            Thread.Sleep(7000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Good Through Date", driver, "MO", "Jackson");
                        }
                        catch { }
                        try
                        {
                            IWebElement currenttax = driver.FindElement(By.XPath("//*[@id='mFuturePayoff_mGrid_RealDataGrid']/tbody"));
                            IList<IWebElement> TRcurrenttax = currenttax.FindElements(By.TagName("tr"));
                            IList<IWebElement> THcurrenttax = currenttax.FindElements(By.TagName("th"));
                            IList<IWebElement> TDcurrenttax;
                            foreach (IWebElement row in TRcurrenttax)
                            {
                                TDcurrenttax = row.FindElements(By.TagName("td"));

                                if (TDcurrenttax.Count != 0 && !row.Text.Contains("Principal"))
                                {
                                    string taxPaymentdetails = Good_through_date + "~" + TDcurrenttax[0].Text + "~" + TDcurrenttax[1].Text + "~" + TDcurrenttax[2].Text;
                                    gc.insert_date(orderNumber, parcelNumber, 1483, taxPaymentdetails, 1, DateTime.Now);
                                }
                            }
                        }
                        catch { }
                        try
                        {
                            driver.FindElement(By.LinkText("Property Summary")).SendKeys(Keys.Enter);
                            Thread.Sleep(4000);
                        }
                        catch { }
                    }
                    try
                    {
                        driver.FindElement(By.LinkText("View Detailed Statement")).SendKeys(Keys.Enter);
                        Thread.Sleep(7000);
                        gc.CreatePdf(orderNumber, parcelNumber, "View Detailed Statement", driver, "MO", "Jackson");
                    }
                    catch { }








                    CitytaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "MO", "Jackson", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "MO", "Jackson");
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