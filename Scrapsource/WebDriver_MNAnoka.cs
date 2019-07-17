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
    public class WebDriver_MNAnoka
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_Anoka(string address, string ownername, string assessment_id, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass gc = new GlobalClass();
            string mul;
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new PhantomJSDriver();
            //driver = new ChromeDriver();
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("https://prtinfo.co.anoka.mn.us/(kxx2oxebgw01e1450i1kvc3m)/search.aspx");
                    Thread.Sleep(4000);
                    if (searchType == "address")
                    {
                        driver.FindElement(By.Id("mSearchControl_mStreetAddress")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "MN", "Anoka");
                        driver.FindElement(By.Id("mSearchControl_mSubmit")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        //   gc.CreatePdf_WOP(orderNumber, "Address search result", driver, "MN", "Anoka");
                        mul = driver.FindElement(By.Id("mResultscontrol_mMessage")).Text.Trim();
                        mul = WebDriverTest.Before(mul, " records");
                        if (mul != "1")
                        {
                            //multi parcel                     
                            IWebElement tbmulti2 = driver.FindElement(By.XPath("//*[@id='mResultscontrol_mResultGrid_RealDataGrid']/tbody"));
                            IList<IWebElement> TRmulti2 = tbmulti2.FindElements(By.TagName("tr"));
                            int maxCheck = 0;
                            IList<IWebElement> TDmulti2;
                            string[] parcel = new string[3]; int p = 0;
                            foreach (IWebElement row in TRmulti2)
                            {
                                if (maxCheck <= 25)
                                {
                                    if (!row.Text.Contains("Parcel Number"))
                                    {
                                        TDmulti2 = row.FindElements(By.TagName("td"));
                                        if (TDmulti2.Count != 0 && TDmulti2.Count > 1)
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
                                        }
                                    }
                                    maxCheck++;
                                }
                            }


                            if (TRmulti2.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Anoka_Multicount"] = "Maximum";
                                driver.Quit();
                                gc.mergpdf(orderNumber, "MN", "Anoka");
                                return "Maximum";
                            }
                            else
                            {

                                if (maxCheck == 3)
                                {
                                    if (parcel[0] == parcel[1])
                                    {
                                        IWebElement element5 = driver.FindElement(By.XPath("//*[@id='mResultscontrol_mResultGrid_RealDataGrid']/tbody/tr[2]/td[1]/a"));
                                        IJavaScriptExecutor js5 = driver as IJavaScriptExecutor;
                                        js5.ExecuteScript("arguments[0].click();", element5);
                                        Thread.Sleep(3000);
                                    }
                                }
                                else if (maxCheck == 4)
                                {
                                    if ((parcel[0] == parcel[1]) && (parcel[1] == parcel[2]))
                                    {
                                        IWebElement element5 = driver.FindElement(By.XPath("//*[@id='mResultscontrol_mResultGrid_RealDataGrid']/tbody/tr[2]/td[1]/a"));
                                        IJavaScriptExecutor js5 = driver as IJavaScriptExecutor;
                                        js5.ExecuteScript("arguments[0].click();", element5);
                                        Thread.Sleep(3000);
                                    }
                                }

                                else if (maxCheck > 2)
                                {
                                    HttpContext.Current.Session["multiparcel_Anoka"] = "Yes";
                                    driver.Quit();
                                    gc.mergpdf(orderNumber, "MN", "Anoka");
                                    return "MultiParcel";
                                }
                            }


                        }
                        else
                        {
                            driver.FindElement(By.XPath("//*[@id='mResultscontrol_mResultGrid_RealDataGrid']/tbody/tr[2]/td[1]/a")).Click();
                            Thread.Sleep(3000);
                            //Thread.Sleep(3000);
                        }
                    }
                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, "", ownername, address, "MN", "Anoka");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_MNAnoka"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "parcel")
                    {
                        //if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        //{
                        //    parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();

                        //}
                        driver.FindElement(By.Id("mSearchControl_mParcelID")).SendKeys(parcelNumber);


                        if (parcelNumber.Contains("-"))
                        {
                            parcelNumber = parcelNumber.Replace("-", "");
                        }
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search", driver, "MN", "Anoka");
                        driver.FindElement(By.Id("mSearchControl_mSubmit")).SendKeys(Keys.Enter);
                        Thread.Sleep(1000);
                        Thread.Sleep(3000);
                        // gc.CreatePdf(orderNumber, parcelNumber, "Parcel search result", driver, "MN", "Anoka");
                        try
                        {
                            mul = driver.FindElement(By.Id("mResultscontrol_mMessage")).Text.Trim();
                            mul = WebDriverTest.Before(mul, " records");
                            if (mul != "1")
                            {
                                IWebElement tbmulti2 = driver.FindElement(By.XPath("//*[@id='mResultscontrol_mResultGrid_RealDataGrid']/tbody"));
                                IList<IWebElement> TRmulti2 = tbmulti2.FindElements(By.TagName("tr"));
                                IList<IWebElement> TDmulti2;
                                int maxCheck = 0;
                                foreach (IWebElement row in TRmulti2)
                                {
                                    if (maxCheck <= 25)
                                    {
                                        if (!row.Text.Contains("Parcel Number"))
                                        {
                                            TDmulti2 = row.FindElements(By.TagName("td"));
                                            if (TDmulti2.Count != 0 && (TDmulti2.Count > 1))
                                            {
                                                string multi1 = TDmulti2[1].Text + "~" + TDmulti2[2].Text;
                                                gc.insert_date(orderNumber, TDmulti2[0].Text, 283, multi1, 1, DateTime.Now);
                                            }
                                        }
                                        maxCheck++;
                                    }
                                }
                                if (TRmulti2.Count > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_Anoka_Multicount"] = "Maximum";
                                    driver.Quit();
                                    gc.mergpdf(orderNumber, "MN", "Anoka");
                                    return "Maximum";
                                }
                                else if ((TRmulti2.Count > 2) && (TRmulti2.Count <= 25))
                                {
                                    HttpContext.Current.Session["multiparcel_Anoka"] = "Yes";
                                    driver.Quit();
                                    gc.mergpdf(orderNumber, "MN", "Anoka");
                                    return "MultiParcel";
                                }

                            }
                        }
                        catch { }
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.Id("Table1"));
                        if(INodata.Text.Contains("0 records returned") && INodata.Text.Contains("No Records Found"))
                        {
                            HttpContext.Current.Session["Nodata_MNAnoka"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        IWebElement Ino = driver.FindElement(By.Id("mSearchControl_mSearchFieldsValidator"));
                        if (INodata.Text.Contains("does not exist"))
                        {
                            HttpContext.Current.Session["Nodata_MNAnoka"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    //property details
                    string Property_ID = "", Situs_Address = "", Property_Description = "", Status = "", Abstract_Torrens = "", Owner = "", Lot_Size = "", Year_Built = "", City_Name = "", School_District_Number_Name = "", Property_classification_2018 = "", Property_classification_2017;
                    Property_ID = driver.FindElement(By.XPath("//*[@id='mGeneralInformation_mGrid_RealDataGrid']/tbody/tr[1]/td[2]/a")).Text.Trim();
                    if (Property_ID.Contains("-"))
                    {
                        Property_ID = Property_ID.Replace("-", "");
                    }
                    gc.CreatePdf(orderNumber, Property_ID, "information", driver, "MN", "Anoka");
                    Situs_Address = driver.FindElement(By.XPath(" //*[@id='mGeneralInformation_mGrid_RealDataGrid']/tbody/tr[2]/td[2] ")).Text.Trim();
                    Property_Description = driver.FindElement(By.XPath(" //*[@id='mGeneralInformation_mGrid_RealDataGrid']/tbody/tr[3]/td[2] ")).Text.Trim();
                    Status = driver.FindElement(By.XPath("//*[@id='mGeneralInformation_mGrid_RealDataGrid']/tbody/tr[8]/td[2] ")).Text.Trim();
                    Abstract_Torrens = driver.FindElement(By.XPath(" //*[@id='mGeneralInformation_mGrid_RealDataGrid']/tbody/tr[9]/td[2] ")).Text.Trim();
                    string owner1 = driver.FindElement(By.XPath("//*[@id='mParties_mGrid_RealDataGrid']/tbody/tr[2]/td[2] ")).Text.Trim();
                    string owner2 = "";
                    try
                    {
                        owner2 = driver.FindElement(By.XPath("//*[@id='mParties_mGrid_RealDataGrid']/tbody/tr[3]/td[2] ")).Text.Trim();
                    }
                    catch { }
                    if (owner2 == "")
                    {
                        Owner = owner1;
                    }
                    else
                    {
                        Owner = owner1 + " & " + owner2;
                    }
                    try
                    {
                        Lot_Size = driver.FindElement(By.XPath("//*[@id='mPropertyCharacteristics_mPropertyCharacteristics_RealDataGrid']/tbody/tr[1]/td[2] ")).Text.Trim();
                        Year_Built = driver.FindElement(By.XPath("//*[@id='mPropertyCharacteristics_mPropertyCharacteristics_RealDataGrid']/tbody/tr[2]/td[2] ")).Text.Trim();
                    }
                    catch
                    {

                    }
                    try
                    {
                        Lot_Size = "";
                        Year_Built = driver.FindElement(By.XPath("//*[@id='mPropertyCharacteristics_mPropertyCharacteristics_RealDataGrid']/tbody/tr[1]/td[2] ")).Text.Trim();
                    }
                    catch { }
                    City_Name = driver.FindElement(By.XPath(" //*[@id='mTaxDistrictInformation_mGrid_RealDataGrid']/tbody/tr[2]/td[2] ")).Text.Trim();
                    School_District_Number_Name = driver.FindElement(By.XPath("//*[@id='mTaxDistrictInformation_mGrid_RealDataGrid']/tbody/tr[1]/td[2] ")).Text.Trim();
                    // Property_classification_2018 = driver.FindElement(By.XPath("//*[@id='mPropertyClassification_mGrid_RealDataGrid']/tbody/tr[2]/td[2] ")).Text.Trim();
                    Property_classification_2017 = "";
                    string property_details = Situs_Address + "~" + Property_Description + "~" + Status + "~" + Abstract_Torrens + "~" + Owner + "~" + Lot_Size + "~" + Year_Built + "~" + City_Name + "~" + School_District_Number_Name + "~" + Property_classification_2018 + "~" + "";
                    gc.insert_date(orderNumber, Property_ID, 284, property_details, 1, DateTime.Now);

                    //assessment details
                    IWebElement Assessmenttable = driver.FindElement(By.XPath("//*[@id='mValues_mGrid_RealDataGrid']/tbody"));
                    IList<IWebElement> Assessmentrow = Assessmenttable.FindElements(By.TagName("tr"));
                    IList<IWebElement> AssessmentTD;
                    foreach (IWebElement Assessment in Assessmentrow)
                    {
                        AssessmentTD = Assessment.FindElements(By.TagName("td"));
                        if (!Assessment.Text.Contains("Tax Year"))
                        {
                            string Assessmentresult = AssessmentTD[0].Text + "~" + AssessmentTD[1].Text + "~" + AssessmentTD[2].Text;
                            gc.insert_date(orderNumber, Property_ID, 285, Assessmentresult, 1, DateTime.Now);
                        }
                    }



                    //IWebElement valuetableElement = driver.FindElement(By.XPath("//*[@id='mValues_mGrid_RealDataGrid']/tbody"));
                    //IList<IWebElement> valuetableRow = valuetableElement.FindElements(By.TagName("tr"));
                    //IList<IWebElement> valuerowTD;
                    //string year = "", Est_Market_Land = "";
                    //foreach (IWebElement row in valuetableRow)
                    //{
                    //    valuerowTD = row.FindElements(By.TagName("td"));
                    //    if (valuerowTD.Count != 0 && valuerowTD[0].Text.Trim() != "Tax Year")
                    //    {
                    //        year = valuerowTD[0].Text + "~" + year;
                    //        Est_Market_Land = valuerowTD[2].Text + "~" + Est_Market_Land;
                    //    }
                    //}
                    //year = year + "fe";
                    //string currentyear = "", prioryear = "";
                    //year = year.Replace("~fe", "");
                    //Est_Market_Land = Est_Market_Land + "fe";
                    //Est_Market_Land = Est_Market_Land.Replace("~fe", "");
                    //string[] Split1 = year.Split('~');
                    //string[] Split2 = Est_Market_Land.Split('~');
                    //for (int K = Split1.Length - 1; K >= 0; K--)
                    //{
                    //    string assessment2 = Split1[K] + "~" + Split2[K] + "~" + Split2[K - 1] + "~" + Split2[K - 2] + "~" + Split2[K - 3] + "~" + Split2[K - 4];
                    //    gc.insert_date(orderNumber, Property_ID, 285, assessment2, 1, DateTime.Now);
                    //    currentyear = Split1[K];
                    //    break;
                    //}
                    //for (int K = Split1.Length - 1; K >= 0; K--)
                    //{
                    //    if (Split1[K] != currentyear)
                    //    {
                    //        prioryear = Split1[K];
                    //        string assessment2 = Split1[K] + "~" + "-" + "~" + "-" + "~" + Split2[K] + "~" + Split2[K - 1] + "~" + Split2[K - 2];
                    //        gc.insert_date(orderNumber, Property_ID, 285, assessment2, 1, DateTime.Now);
                    //        break;
                    //    }
                    //}
                    //for (int K = Split1.Length - 1; K >= 0; K--)
                    //{
                    //    if (Split1[K] != prioryear && Split1[K] != currentyear)
                    //    {
                    //        string assessment2 = Split1[K] + "~" + "-" + "~" + "-" + "~" + Split2[K] + "~" + Split2[K - 1] + "~" + Split2[K - 2];
                    //        gc.insert_date(orderNumber, Property_ID, 285, assessment2, 1, DateTime.Now);
                    //        break;
                    //    }
                    //}
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    //tax details
                    try
                    {
                        IWebElement tbmulti1 = driver.FindElement(By.XPath("//*[@id='mPropertyClassification_mGrid_RealDataGrid']/tbody"));
                        IList<IWebElement> TRmulti1 = tbmulti1.FindElements(By.TagName("tr"));

                        IList<IWebElement> TDmulti1;
                        foreach (IWebElement row in TRmulti1)
                        {
                            if (!row.Text.Contains("Tax Year"))
                            {
                                TDmulti1 = row.FindElements(By.TagName("td"));
                                if (TDmulti1.Count != 0)
                                {
                                    string multi1 = TDmulti1[0].Text + "~" + TDmulti1[1].Text + "~" + TDmulti1[2].Text + "~" + "Anoka County, Minnesota 2100 3rd Ave.| Anoka, MN  55303 Ph: 763-324-4000(switchboard)";
                                    gc.insert_date(orderNumber, Property_ID, 286, multi1, 1, DateTime.Now);
                                }
                            }
                        }
                    }
                    catch { }
                    //tax payment  
                    try
                    {
                        IWebElement tbmulti4 = driver.FindElement(By.XPath("//*[@id='mPaymentHistoryAnoka_mGrid_RealDataGrid']/tbody"));
                        IList<IWebElement> TRmulti4 = tbmulti4.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDmulti4;
                        foreach (IWebElement row in TRmulti4)
                        {
                            if (!row.Text.Contains("Date Paid"))
                            {
                                TDmulti4 = row.FindElements(By.TagName("td"));
                                if (TDmulti4.Count != 0)
                                {
                                    string multi1 = TDmulti4[0].Text + "~" + TDmulti4[1].Text + "~" + TDmulti4[2].Text + "~" + TDmulti4[3].Text + "~" + TDmulti4[4].Text;
                                    gc.insert_date(orderNumber, Property_ID, 287, multi1, 1, DateTime.Now);
                                }
                            }
                        }
                    }
                    catch { }
                    //  Tax Installments Payable Details Table:
                    try
                    {
                        string deliquent_amount = "";
                        driver.FindElement(By.XPath("//*[@id='mTaxChargesBalancePaymentInjected_mIncludeInstallmentsCheckbox']")).Click();
                        Thread.Sleep(3000);
                        IWebElement tbmulti5 = driver.FindElement(By.XPath("//*[@id='mGridInstallments_RealDataGrid']/tbody"));
                        IList<IWebElement> TRmulti5 = tbmulti5.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDmulti5;
                        foreach (IWebElement row in TRmulti5)
                        {
                            if (!row.Text.Contains("Tax Year"))
                            {
                                TDmulti5 = row.FindElements(By.TagName("td"));
                                if (TDmulti5.Count != 0)
                                {
                                    string multi13 = TDmulti5[0].Text + "~" + TDmulti5[1].Text + "~" + TDmulti5[2].Text + "~" + TDmulti5[3].Text + "~" + TDmulti5[4].Text + "~" + TDmulti5[5].Text + "~" + TDmulti5[6].Text + "~" + TDmulti5[6].Text + "~" + "-";
                                    gc.insert_date(orderNumber, Property_ID, 288, multi13, 1, DateTime.Now);
                                }
                            }
                        }
                        try
                        {
                            string date = "";
                            int counttd;
                            driver.FindElement(By.XPath("//*[@id='mTaxChargesBalancePaymentInjected_mIncludeDelinquentTaxYearsCheckbox']")).Click();
                            deliquent_amount = driver.FindElement(By.XPath("//*[@id='mTaxChargesBalancePaymentInjected_mDelinquentAmount']")).Text.Trim();
                            IWebElement tbmulti6 = driver.FindElement(By.XPath(" //*[@id='mGridDelinquent_RealDataGrid']/tbody"));
                            IList<IWebElement> TRmulti6 = tbmulti6.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDmulti6;
                            foreach (IWebElement row in TRmulti6)
                            {
                                if (!row.Text.Contains("Tax Year"))
                                {
                                    TDmulti6 = row.FindElements(By.TagName("td"));
                                    counttd = TDmulti6.Count;
                                    if (TDmulti6.Count == 1)
                                    {
                                        string multi1 = "No records" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "$0.00";
                                        gc.insert_date(orderNumber, Property_ID, 288, multi1, 1, DateTime.Now);
                                    }
                                    else
                                    {
                                        string multi1 = TDmulti6[0].Text + "~" + TDmulti6[1].Text + "~" + TDmulti6[2].Text + "~" + TDmulti6[3].Text + "~" + TDmulti6[4].Text + "~" + TDmulti6[5].Text + "~" + TDmulti6[6].Text + "~" + "-" + "~" + TDmulti6[6].Text;
                                        gc.insert_date(orderNumber, Property_ID, 288, multi1, 1, DateTime.Now);
                                        driver.FindElement(By.XPath("//*[@id='mTaxChargesBalancePaymentInjected_mFuturePayoff']")).Click();
                                        Thread.Sleep(4000);

                                        IWebElement dt = driver.FindElement(By.XPath("//*[@id='mFuturePayoff_mDate']"));
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
                                        driver.FindElement(By.Id("mFuturePayoff_mDate")).SendKeys(date);

                                    }
                                    driver.FindElement(By.XPath("//*[@id='mFuturePayoff_mCalculate']")).Click();
                                    Thread.Sleep(7000);
                                    gc.CreatePdf(orderNumber, Property_ID, "Futurepayoff", driver, "MN", "Anoka");
                                    IWebElement tbmulti51 = driver.FindElement(By.XPath("//*[@id='mFuturePayoff_mGrid_RealDataGrid']/tbody"));
                                    IList<IWebElement> TRmulti51 = tbmulti51.FindElements(By.TagName("tr"));
                                    IList<IWebElement> TDmulti51;
                                    foreach (IWebElement row1 in TRmulti51)
                                    {
                                        if (!row1.Text.Contains("Principal"))
                                        {
                                            TDmulti51 = row1.FindElements(By.TagName("td"));
                                            if (TDmulti51.Count != 0)
                                            {
                                                string fututepay = date + "~" + TDmulti51[0].Text + "~" + TDmulti51[1].Text + "~" + TDmulti51[2].Text;
                                                gc.insert_date(orderNumber, Property_ID, 289, fututepay, 1, DateTime.Now);
                                                //AsOfDate~Principal~Interest_Penalties_Costs~InstallmentTotal
                                            }
                                        }
                                    }
                                }

                            }
                        }
                        catch
                        {
                        }
                    }
                    catch { }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "MN", "Anoka", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "MN", "Anoka");
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