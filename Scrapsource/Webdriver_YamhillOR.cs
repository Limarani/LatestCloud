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
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace ScrapMaricopa.Scrapsource
{
    public class Webdriver_YamhillOR
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        public string FTP_YamhillOR(string houseno, string Direction, string sname, string stype, string account, string parcelNumber, string ownername, string searchType, string orderNumber)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string multi = "", TaxAuthority = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            //driverService.HideCommandPromptWindow = true;
            using (driver = new ChromeDriver())
            {
                //driver = new ChromeDriver();
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    try
                    {

                        driver.Navigate().GoToUrl("https://www.co.yamhill.or.us/content/assessor-department");
                        Thread.Sleep(1000);
                        string taxauthdata = driver.FindElement(By.XPath("//*[@id='block-block-23']/div[2]/p[1]")).Text.Replace("\r\n", " ");
                        TaxAuthority = gc.Between(taxauthdata, "Staff Listing", "TTY 800 735-2900").Replace(":", ": ");
                        gc.CreatePdf_WOP(orderNumber, "Tax Authority", driver, "OR", "Yamhill");
                    }
                    catch { }
                    driver.Navigate().GoToUrl("https://ascendweb.co.yamhill.or.us/AcsendWeb/(S(ynxt0rmtheuyaephvjnr0xup))/default.aspx");
                    Thread.Sleep(1000);
                    if (searchType == "titleflex")
                    {
                        string address = "";
                        if (Direction != "")
                        {
                            address = houseno + " " + Direction + " " + sname + " " + stype + " " + account;
                        }
                        if (Direction == "")
                        {
                            address = houseno + " " + sname + " " + stype + " " + account;
                        }

                        gc.TitleFlexSearch(orderNumber, "", "", address.Trim(), "OR", "Yamhill");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["nodata_Yamhill"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "account";
                        account = parcelNumber.Replace("-", "");

                    }
                    if (searchType == "address")
                    {
                        string PropertyAdd = "", AccoutNumber = "";
                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "OR", "Yamhill");
                        if (Direction == "")
                        {
                            driver.FindElement(By.Id("mStreetAddress")).SendKeys(houseno + "%" + sname);
                        }
                        if (Direction != "")
                        {
                            driver.FindElement(By.Id("mStreetAddress")).SendKeys(houseno + "%" + Direction + "%" + sname);
                        }
                        gc.CreatePdf_WOP(orderNumber, "Address Search Input", driver, "OR", "Yamhill");
                        driver.FindElement(By.Id("mSubmit")).SendKeys(Keys.Enter);
                        Thread.Sleep(1000);
                        //IWebElement multirecord = driver.FindElement(By.XPath("//*[@id='mMessage']"));

                        gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "OR", "Yamhill");
                        try
                        {
                            string nodata = driver.FindElement(By.XPath("//*[@id='mMessage']")).Text;
                            if (nodata.Contains("0 records"))
                            {
                                HttpContext.Current.Session["nodata_Yamhill"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }

                        IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='mGrid']/tbody"));
                        IList<IWebElement> TRmultiaddress = multiaddress.FindElements(By.TagName("tr"));
                        IList<IWebElement> THmultiaddress = multiaddress.FindElements(By.TagName("th"));
                        IList<IWebElement> TDmultiaddress;

                        if (TRmultiaddress.Count <= 2)
                        {
                            IWebElement parceldata = driver.FindElement(By.XPath("//*[@id='mGrid']/tbody/tr[2]/td[1]/a"));
                            parceldata.Click();
                            Thread.Sleep(1000);
                        }
                        if (TRmultiaddress.Count > 28)
                        {
                            HttpContext.Current.Session["multiParcel_Yamhill_Maximum"] = "Maimum";
                            driver.Quit();
                            return "Maximum";
                        }
                        if (TRmultiaddress.Count > 2)
                        {
                            foreach (IWebElement row in TRmultiaddress)
                            {

                                TDmultiaddress = row.FindElements(By.TagName("td"));
                                if (!row.Text.Contains("Parcel Number"))
                                {
                                    AccoutNumber = TDmultiaddress[0].Text;
                                    PropertyAdd = TDmultiaddress[1].Text;

                                    string Multi = PropertyAdd;
                                    gc.insert_date(orderNumber, AccoutNumber, 1041, Multi, 1, DateTime.Now);
                                }



                            }
                            HttpContext.Current.Session["multiParcel_Yamhill"] = "Yes";
                            driver.Quit();
                            return "MultiParcel";
                        }

                    }

                    if (searchType == "parcel")
                    {
                        gc.CreatePdf_WOP(orderNumber, "Parcel Search", driver, "OR", "Yamhill");
                        driver.FindElement(By.Id("mAlternateParcelID")).SendKeys(parcelNumber);
                        gc.CreatePdf_WOP(orderNumber, "Parcel Search Input", driver, "OR", "Yamhill");
                        driver.FindElement(By.Id("mSubmit")).SendKeys(Keys.Enter);
                        Thread.Sleep(1000);

                    }

                    if (searchType == "account")
                    {
                        gc.CreatePdf_WOP(orderNumber, "Account Search", driver, "OR", "Yamhill");
                        driver.FindElement(By.Id("mParcelID2")).SendKeys(account);
                        gc.CreatePdf_WOP(orderNumber, "Account Search Input", driver, "OR", "Yamhill");
                        driver.FindElement(By.Id("mSubmit")).SendKeys(Keys.Enter);
                        Thread.Sleep(1000);

                    }

                    try
                    {
                        string nodata = driver.FindElement(By.XPath("//*[@id='mSearchFieldsValidator']")).Text;
                        if (nodata.Contains("does not exist"))
                        {
                            HttpContext.Current.Session["nodata_Yamhill"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    // Property Details

                    // gc.CreatePdf_WOP(orderNumber, "Property Details", driver, "OR", "Yamhill");
                    string propertydata = driver.FindElement(By.XPath("//*[@id='ParcelSitusTable']/tbody")).Text;
                    string AcNo = "", PropertyAddress = "";
                    AcNo = gc.Between(propertydata, "Account Number", "Property Address").Trim();
                    PropertyAddress = GlobalClass.After(propertydata, "Property Address");

                    string Bulkdata = driver.FindElement(By.XPath("//*[@id='mGeneralInformation']/tbody")).Text;
                    string Alternateproperty = "", PropertyDesc = "", Propertycategory = "", TaxCodeArea = "", toatal_rate = "", accountacres = "";
                    Alternateproperty = gc.Between(Bulkdata, "Alternate Property", "Property Description").Replace(" # ", "");
                    PropertyDesc = gc.Between(Bulkdata, "Property Description", "Property Category");
                    Propertycategory = gc.Between(Bulkdata, "Property Category", "Status");
                    TaxCodeArea = gc.Between(Bulkdata, "Tax Code Area", "Remarks").Trim();
                    parcelNumber = Alternateproperty;

                    // Total rate
                    IWebElement totalrate = driver.FindElement(By.XPath("//*[@id='mTaxRate']/tbody/tr[2]/td[2]"));
                    toatal_rate = totalrate.Text;
                    string Bulkinfo = driver.FindElement(By.XPath("//*[@id='mPropertyCharacteristics']/tbody")).Text;
                    accountacres = gc.Between(Bulkinfo, "Account Acres", "Change Property Ratio");

                    // Year Built
                    string Yearbuilt = "", Yearbuiltvalue = "";
                    IWebElement yearbuilt = driver.FindElement(By.XPath("//*[@id='mPropertyDetails']/tbody"));
                    IList<IWebElement> TRyearbuilt = yearbuilt.FindElements(By.TagName("tr"));
                    IList<IWebElement> THyearbuilt = yearbuilt.FindElements(By.TagName("th"));
                    IList<IWebElement> TDyearbuilt;
                    foreach (IWebElement row in TRyearbuilt)
                    {
                        TDyearbuilt = row.FindElements(By.TagName("td"));
                        if (!row.Text.Contains("Year Built"))
                        {
                            Yearbuilt = TDyearbuilt[2].Text;
                        }
                    }

                    string propertydetails = AcNo + "~" + PropertyAddress + "~" + Alternateproperty + "~" + PropertyDesc + "~" + Propertycategory + "~" + Yearbuilt + "~" + TaxCodeArea + "~" + toatal_rate + "~" + accountacres + "~" + TaxAuthority;
                    gc.insert_date(orderNumber, parcelNumber, 1017, propertydetails, 1, DateTime.Now);
                    gc.CreatePdf(orderNumber, parcelNumber, "Property Account Summary", driver, "OR", "Yamhill");
                    // Assessment Details
                    string exemption = "";
                    IWebElement Exemption = driver.FindElement(By.XPath("//*[@id='mActiveExemptions']/tbody"));
                    exemption = Exemption.Text;

                    string taxyear = "", taxyear1 = "", valuetype = "";
                    IWebElement Assessmentdetails = driver.FindElement(By.XPath("//*[@id='mPropertyValues']/tbody"));
                    IList<IWebElement> TRAssessmentdetails = Assessmentdetails.FindElements(By.TagName("tr"));
                    IList<IWebElement> THAssessmentdetails = Assessmentdetails.FindElements(By.TagName("th"));
                    IList<IWebElement> TDAssessmentdetails;
                    foreach (IWebElement row in TRAssessmentdetails)
                    {
                        TDAssessmentdetails = row.FindElements(By.TagName("td"));
                        if (THAssessmentdetails.Count != 0 && row.Text.Contains("Value Type"))
                        {
                            valuetype += THAssessmentdetails[0].Text + "~";
                            taxyear += THAssessmentdetails[1].Text.Replace("Tax Year", "").Replace("\r\n", "") + "~";
                            taxyear1 += THAssessmentdetails[2].Text.Replace("Tax Year", "").Replace("\r\n", "") + "~";
                        }
                        if (TDAssessmentdetails.Count != 0 && !row.Text.Contains("Value Type"))
                        {
                            valuetype += TDAssessmentdetails[0].Text + "~";
                            taxyear += TDAssessmentdetails[1].Text + "~";
                            taxyear1 += TDAssessmentdetails[2].Text + "~";
                        }
                    }
                    valuetype += "Exemptions";
                    taxyear += exemption;
                    taxyear1 += exemption;
                    DBconnection dbconn = new DBconnection();
                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + valuetype + "' where Id = '" + 1018 + "'");
                    gc.insert_date(orderNumber, parcelNumber, 1018, taxyear, 1, DateTime.Now);
                    gc.insert_date(orderNumber, parcelNumber, 1018, taxyear1, 1, DateTime.Now);


                    // tax Information details
                    try
                    {
                        IWebElement taxdetail = driver.FindElement(By.XPath("//*[@id='mTaxChargesBalancePayment']/tbody"));
                        IList<IWebElement> TRtaxdetail = taxdetail.FindElements(By.TagName("tr"));
                        IList<IWebElement> THtaxdetail = taxdetail.FindElements(By.TagName("th"));
                        IList<IWebElement> TDtaxdetail;
                        foreach (IWebElement row in TRtaxdetail)
                        {
                            TDtaxdetail = row.FindElements(By.TagName("td"));

                            if (TDtaxdetail.Count != 0 && !row.Text.Contains("Installment"))
                            {

                                string taxdetails = AcNo + "~" + PropertyAddress + "~" + Alternateproperty + "~" + TDtaxdetail[0].Text + "~" + TDtaxdetail[1].Text + "~" + TDtaxdetail[2].Text + "~" + TDtaxdetail[3].Text + "~" + TDtaxdetail[4].Text + "~" + TDtaxdetail[5].Text + "~" + TDtaxdetail[6].Text + "~" + TDtaxdetail[7].Text + "~" + TDtaxdetail[8].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1037, taxdetails, 1, DateTime.Now);

                            }
                        }

                    }
                    catch { }

                    // Payment History Table

                    IWebElement payhistory = driver.FindElement(By.XPath("//*[@id='mReceipts']/tbody"));
                    IList<IWebElement> TRpayhistory = payhistory.FindElements(By.TagName("tr"));
                    IList<IWebElement> THpayhistory = payhistory.FindElements(By.TagName("th"));
                    IList<IWebElement> TDpayhistory;
                    foreach (IWebElement row in TRpayhistory)
                    {
                        TDpayhistory = row.FindElements(By.TagName("td"));

                        if (TDpayhistory.Count != 0 && !row.Text.Contains("Receipt No"))
                        {
                            string payhistorydetails = TDpayhistory[0].Text + "~" + TDpayhistory[1].Text + "~" + TDpayhistory[2].Text + "~" + TDpayhistory[3].Text + "~" + TDpayhistory[4].Text + "~" + TDpayhistory[5].Text;
                            gc.insert_date(orderNumber, parcelNumber, 1038, payhistorydetails, 1, DateTime.Now);
                        }
                    }



                    // tax History details
                    try
                    {
                        int tax_year = DateTime.Now.Year;
                        for (int i = 0; i < 3; i++)
                        {
                            try
                            {
                                driver.FindElement(By.LinkText("Property Summary")).SendKeys(Keys.Enter);
                                Thread.Sleep(4000);
                            }
                            catch { }

                            driver.FindElement(By.Id("mDifferentYear")).Clear();
                            driver.FindElement(By.Id("mDifferentYear")).SendKeys(Convert.ToString(tax_year));
                            IWebElement taxhistorydetails = driver.FindElement(By.LinkText("Installments Payable/Paid for Tax Year(Enter 4-digit Year, then Click-Here):"));
                            IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                            js1.ExecuteScript("arguments[0].click();", taxhistorydetails);
                            Thread.Sleep(5000);

                            IWebElement taxhistory = driver.FindElement(By.XPath("//*[@id='mGrid']/tbody"));
                            IList<IWebElement> TRtaxhistory = taxhistory.FindElements(By.TagName("tr"));
                            IList<IWebElement> THtaxhistory = taxhistory.FindElements(By.TagName("th"));
                            IList<IWebElement> TDtaxhistory;
                            foreach (IWebElement row in TRtaxhistory)
                            {
                                TDtaxhistory = row.FindElements(By.TagName("td"));

                                if (TDtaxhistory.Count != 0 && !row.Text.Contains("TCA/District") && !row.Text.Contains("TOTAL Due"))
                                {
                                    string TaxHistoryDetails1 = TDtaxhistory[0].Text + "~" + TDtaxhistory[1].Text + "~" + TDtaxhistory[2].Text + "~" + TDtaxhistory[3].Text + "~" + TDtaxhistory[4].Text + "~" + TDtaxhistory[5].Text + "~" + TDtaxhistory[6].Text;
                                    gc.insert_date(orderNumber, parcelNumber, 1040, TaxHistoryDetails1, 1, DateTime.Now);
                                }
                                if (TDtaxhistory.Count != 0 && !row.Text.Contains("TCA/District") && row.Text.Contains("TOTAL Due"))

                                {
                                    string TaxHistoryDetails2 = "" + "~" + TDtaxhistory[0].Text + "~" + TDtaxhistory[2].Text + "~" + TDtaxhistory[3].Text + "~" + TDtaxhistory[4].Text + "~" + TDtaxhistory[5].Text + "~" + TDtaxhistory[6].Text;
                                    gc.insert_date(orderNumber, parcelNumber, 1040, TaxHistoryDetails2, 1, DateTime.Now);
                                    gc.CreatePdf(orderNumber, parcelNumber, "Tax History Details" + tax_year, driver, "OR", "Yamhill");
                                }
                            }
                            tax_year--;

                        }

                    }
                    catch { }

                    // Due Details Table
                    try
                    {
                        try
                        {
                            driver.FindElement(By.LinkText("Property Summary")).SendKeys(Keys.Enter);
                            Thread.Sleep(4000);
                        }
                        catch { }
                        string delinquenttax = "";
                        IWebElement taxdetail1 = driver.FindElement(By.XPath("//*[@id='mTaxChargesBalancePayment']/tbody"));
                        IList<IWebElement> TRtaxdetail1 = taxdetail1.FindElements(By.TagName("tr"));
                        IList<IWebElement> THtaxdetail1 = taxdetail1.FindElements(By.TagName("th"));
                        IList<IWebElement> TDtaxdetail1;
                        foreach (IWebElement row1 in TRtaxdetail1)
                        {
                            TDtaxdetail1 = row1.FindElements(By.TagName("td"));
                            try
                            {
                                delinquenttax = TDtaxdetail1[4].Text;
                            }
                            catch { }
                            if (TDtaxdetail1.Count != 0 && !row1.Text.Contains("Installment") && (delinquenttax == "$0.00"))
                            {

                                driver.Navigate().GoToUrl("https://ascendweb.co.yamhill.or.us/AcsendWeb/(S(arnzehuuw5x1dfkk3jafaqef))/statement.aspx");
                                Thread.Sleep(5000);
                                gc.CreatePdf(orderNumber, parcelNumber, "Due Details", driver, "OR", "Yamhill");
                                IWebElement duedetails = driver.FindElement(By.XPath("//*[@id='mGrid']/tbody"));
                                IList<IWebElement> TRduedetails = duedetails.FindElements(By.TagName("tr"));
                                IList<IWebElement> THduedetails = duedetails.FindElements(By.TagName("th"));
                                IList<IWebElement> TDduedetails;
                                foreach (IWebElement row in TRduedetails)
                                {
                                    TDduedetails = row.FindElements(By.TagName("td"));

                                    if (TDduedetails.Count != 0 && !row.Text.Contains("TCA/District") && !row.Text.Contains("TOTAL Due"))
                                    {
                                        string Duedetails1 = TDduedetails[0].Text + "~" + TDduedetails[1].Text + "~" + TDduedetails[2].Text + "~" + TDduedetails[3].Text + "~" + TDduedetails[4].Text + "~" + TDduedetails[5].Text + "~" + TDduedetails[6].Text;
                                        gc.insert_date(orderNumber, parcelNumber, 1039, Duedetails1, 1, DateTime.Now);
                                    }
                                    if (TDduedetails.Count != 0 && !row.Text.Contains("TCA/District") && row.Text.Contains("TOTAL Due"))

                                    {
                                        string Duedetails2 = "" + "~" + TDduedetails[0].Text + "~" + TDduedetails[2].Text + "~" + TDduedetails[3].Text + "~" + TDduedetails[4].Text + "~" + TDduedetails[5].Text + "~" + TDduedetails[6].Text;
                                        gc.insert_date(orderNumber, parcelNumber, 1039, Duedetails2, 1, DateTime.Now);
                                    }
                                }
                                break;
                            }
                            else if (TDtaxdetail1.Count != 0 && !row1.Text.Contains("Installment") && (delinquenttax != "$0.00"))
                            {
                                // Delinquent Tax
                                driver.Navigate().GoToUrl("https://ascendweb.co.yamhill.or.us/AcsendWeb/(S(arnzehuuw5x1dfkk3jafaqef))/statement.aspx");
                                Thread.Sleep(4000);
                                driver.FindElement(By.Id("mGoBack")).SendKeys(Keys.Enter);
                                Thread.Sleep(4000);
                                IWebElement IasofDateSearch = driver.FindElement(By.Id("mDate"));
                                IasofDateSearch.Clear();
                                string currDate = DateTime.Now.ToString("MM/dd/yyyy");
                                string dateChecking = DateTime.Now.ToString("MM") + "/15/" + DateTime.Now.ToString("yyyy");
                                if (Convert.ToDateTime(currDate) > Convert.ToDateTime(dateChecking))
                                {
                                    string nextEndOfMonth = "";
                                    if ((Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))) < 12))
                                    {
                                        nextEndOfMonth = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM")) + 1), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")) + 1)).ToString("MM/dd/yyyy");
                                        IasofDateSearch.SendKeys(nextEndOfMonth);

                                    }
                                    else
                                    {
                                        int nxtYr = Convert.ToInt16(DateTime.Now.ToString("yyyy")) + 1;
                                        nextEndOfMonth = nextEndOfMonth = new DateTime(nxtYr, 1, DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), 1)).ToString("MM/dd/yyyy");
                                        IasofDateSearch.SendKeys(nextEndOfMonth);
                                    }

                                }
                                else
                                {
                                    string EndOfMonth = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")))).ToString("MM/dd/yyyy");
                                    IasofDateSearch.SendKeys(EndOfMonth);
                                }
                                driver.FindElement(By.XPath("//*[@id='mCalculate']")).SendKeys(Keys.Enter);
                                Thread.Sleep(4000);
                                gc.CreatePdf(orderNumber, parcelNumber, "Due Details", driver, "OR", "Yamhill");
                                IWebElement duedetails = driver.FindElement(By.XPath("//*[@id='mGrid']/tbody"));
                                IList<IWebElement> TRduedetails = duedetails.FindElements(By.TagName("tr"));
                                IList<IWebElement> THduedetails = duedetails.FindElements(By.TagName("th"));
                                IList<IWebElement> TDduedetails;
                                foreach (IWebElement row in TRduedetails)
                                {
                                    TDduedetails = row.FindElements(By.TagName("td"));

                                    if (TDduedetails.Count != 0 && !row.Text.Contains("TCA/District") && !row.Text.Contains("TOTAL Due"))
                                    {
                                        string Duedetails1 = TDduedetails[0].Text + "~" + TDduedetails[1].Text + "~" + TDduedetails[2].Text + "~" + TDduedetails[3].Text + "~" + TDduedetails[4].Text + "~" + TDduedetails[5].Text + "~" + TDduedetails[6].Text;
                                        gc.insert_date(orderNumber, parcelNumber, 1039, Duedetails1, 1, DateTime.Now);
                                    }
                                    if (TDduedetails.Count != 0 && !row.Text.Contains("TCA/District") && row.Text.Contains("TOTAL Due"))

                                    {
                                        string Duedetails2 = "" + "~" + TDduedetails[0].Text + "~" + TDduedetails[2].Text + "~" + TDduedetails[3].Text + "~" + TDduedetails[4].Text + "~" + TDduedetails[5].Text + "~" + TDduedetails[6].Text;
                                        gc.insert_date(orderNumber, parcelNumber, 1039, Duedetails2, 1, DateTime.Now);

                                    }
                                }
                                break;
                            }

                        }
                    }
                    catch { }







                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "OR", "Yamhill", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "OR", "Yamhill");
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