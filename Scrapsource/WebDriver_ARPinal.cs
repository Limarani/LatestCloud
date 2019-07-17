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
using System.Text;
using OpenQA.Selenium.Firefox;

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_ARPinal
    {

        string outputPath = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();

        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_ARPinal(string houseno, string housedir, string sname, string sttype, string unitno, string parcelNumber, string searchType, string orderNumber, string ownername, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            //IWebElement iframeElement1;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                // driver = new ChromeDriver();
                string Owner_name = "-", parcel_no = "-", property_Address = "-", Legal_Description = "-", UseCode = "-", TaxAreaCode = "-", TaxYear = "-", Mailing_address = "-", prop_use = "-", year_built = "-";
                string FullCashValue = "-", RealPropertyRatio = "-", LimitedValue = "-", AssessedFCV = "-", AssessedLPV = "-";
                string tax_amountIst = "-", tax_amountIInd = "-", tax_amountTotal = "-", interest_feesIst = "-", interest_feesIInd = "-", interest_feesTotal = "-";
                string paid_amountIst = "-", paid_amountIInd = "-", paid_amountTotal = "-", total_dueIst = "-", total_dueIInd = "-", total_duetTotal = "-";
                string status = "-", applied_interest = "-", year = "-", multi = "-", total = "-", amount = "-";
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("http://www.pinalcountyaz.gov/Assessor/Pages/ParcelSearch.aspx");

                    if (searchType == "address")
                    {

                        driver.FindElement(By.Id("ctl00_m_g_e7e24a7d_b298_4c40_8411_c65d5498a997_ctl00_ctl02_txt_Number")).SendKeys(houseno);
                        driver.FindElement(By.Id("ctl00_m_g_e7e24a7d_b298_4c40_8411_c65d5498a997_ctl00_ctl02_ddl_Direction")).SendKeys(housedir);
                        driver.FindElement(By.Id("ctl00_m_g_e7e24a7d_b298_4c40_8411_c65d5498a997_ctl00_ctl02_txt_Name")).SendKeys(sname);
                        driver.FindElement(By.Id("ctl00_m_g_e7e24a7d_b298_4c40_8411_c65d5498a997_ctl00_ctl02_ddl_Suffix")).SendKeys(sttype);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "AZ", "Pinal");
                        driver.FindElement(By.Id("ctl00_m_g_e7e24a7d_b298_4c40_8411_c65d5498a997_ctl00_ctl02_btn_GoPA")).SendKeys(Keys.Enter);
                        Thread.Sleep(4000);

                        gc.CreatePdf_WOP(orderNumber, "Address search result", driver, "AZ", "Pinal");

                        string mul = "";
                        //  string mul = driver.FindElement(By.XPath("//*[@id='ctl00_m_g_e7e24a7d_b298_4c40_8411_c65d5498a997_ctl00_ctl03_lbl_SearchResults']")).Text;
                        //*[@id="tblTitle"]/tbody/tr/td[2]
                        try
                        {
                            mul = driver.FindElement(By.Id("ctl00_m_g_e7e24a7d_b298_4c40_8411_c65d5498a997_ctl00_ctl03_lbl_SearchResults")).Text;

                            mul = WebDriverTest.Before(mul, " Entries");
                            mul = WebDriverTest.After(mul, "(").Trim();
                        }
                        catch { }

                        if ((mul != "1") && (mul != "0") && (mul != ""))
                        {
                            //multi parcel
                            IWebElement tbmulti = driver.FindElement(By.XPath("/html/body/form/placeholder/table/tbody/tr[3]/td/table[2]/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr/td/table/tbody/tr/td/div/div[2]/div[1]/div/div[2]/div/div/div[2]/div/table/tbody"));
                            IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));
                            int maxCheck = 0;
                            IList<IWebElement> TDmulti;
                            foreach (IWebElement row in TRmulti)
                            {
                                if (maxCheck <= 25)
                                {
                                    TDmulti = row.FindElements(By.TagName("td"));
                                    if (TDmulti.Count != 0)
                                    {
                                        string multi1 = TDmulti[0].Text + "~" + TDmulti[1].Text + "~" + TDmulti[2].Text;
                                        gc.insert_date(orderNumber, TDmulti[0].Text, 149, multi1, 1, DateTime.Now);
                                    }
                                    maxCheck++;
                                }
                            }

                            if (TRmulti.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Pinal_Multicount"] = "Maximum";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiparcel_Pinal"] = "Yes";
                            }
                            driver.Quit();
                            return "MultiParcel";
                        }

                        // CreatePdf_WOP(orderNumber, "Multiparcel Address Search");
                    }
                    if (searchType == "titleflex")
                    {
                        string titleaddress = houseno + " " + housedir + " " + sname + " " + sttype + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "AZ", "Pinal");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_AZPinal"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "parcel")
                    {

                        Thread.Sleep(3000);
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();

                        }
                        if (GlobalClass.titleparcel.Contains(".") || GlobalClass.titleparcel.Contains("-"))
                        {
                            parcelNumber = GlobalClass.titleparcel.Replace(".", "").Replace("-", "");
                        }

                        if (parcelNumber.Replace(" ", "").Replace("-", "").Count() == 9)
                        {

                            string strparcelNumber = parcelNumber.Replace(" ", "").Replace("-", "");
                            string strParcelBook = strparcelNumber.Substring(0, 3);
                            string strParcelMap = strparcelNumber.Substring(3, 2);
                            string strParcelNo = strparcelNumber.Substring(5, 4);
                            driver.FindElement(By.Id("ctl00_m_g_e7e24a7d_b298_4c40_8411_c65d5498a997_ctl00_ctl02_txt_Book")).SendKeys(strParcelBook);
                            driver.FindElement(By.Id("ctl00_m_g_e7e24a7d_b298_4c40_8411_c65d5498a997_ctl00_ctl02_txt_Map")).SendKeys(strParcelMap);
                            driver.FindElement(By.Id("ctl00_m_g_e7e24a7d_b298_4c40_8411_c65d5498a997_ctl00_ctl02_txt_Parcel")).SendKeys(strParcelNo);
                        }
                        else if (parcelNumber.Replace(" ", "").Replace("-", "").Count() == 8)
                        {
                            string strparcelNumber = parcelNumber.Replace(" ", "").Replace("-", "");
                            string strParcelBook = strparcelNumber.Substring(0, 3);
                            string strParcelMap = strparcelNumber.Substring(3, 2);
                            string strParcelNo = strparcelNumber.Substring(5, 3);
                            driver.FindElement(By.Id("ctl00_m_g_e7e24a7d_b298_4c40_8411_c65d5498a997_ctl00_ctl02_txt_Book")).SendKeys(strParcelBook);
                            driver.FindElement(By.Id("ctl00_m_g_e7e24a7d_b298_4c40_8411_c65d5498a997_ctl00_ctl02_txt_Map")).SendKeys(strParcelMap);
                            driver.FindElement(By.Id("ctl00_m_g_e7e24a7d_b298_4c40_8411_c65d5498a997_ctl00_ctl02_txt_Parcel")).SendKeys(strParcelNo);
                        }

                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search", driver, "AZ", "Pinal");
                        driver.FindElement(By.Id("ctl00_m_g_e7e24a7d_b298_4c40_8411_c65d5498a997_ctl00_ctl02_btn_GoParcel")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);

                    }
                    if (searchType == "ownername")
                    {

                        Thread.Sleep(3000);
                        driver.FindElement(By.Id("ctl00_m_g_e7e24a7d_b298_4c40_8411_c65d5498a997_ctl00_ctl02_txt_Owner")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "Owner Name search", driver, "AZ", "Pinal");
                        driver.FindElement(By.Id("ctl00_m_g_e7e24a7d_b298_4c40_8411_c65d5498a997_ctl00_ctl02_btn_GoOwner")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Owner Name search result", driver, "AZ", "Pinal");
                        string mul = "";
                        try
                        {
                            mul = driver.FindElement(By.XPath("//*[@id='ctl00_m_g_e7e24a7d_b298_4c40_8411_c65d5498a997_ctl00_ctl03_lbl_SearchResults']")).Text;
                            mul = WebDriverTest.Before(mul, " Entries");
                            mul = WebDriverTest.After(mul, "(").Trim();
                        }
                        catch { }

                        if ((mul != "1") && (mul != "0") && (mul != ""))
                        {
                            //multi parcel
                            IWebElement tbmulti = driver.FindElement(By.XPath("/html/body/form/placeholder/table/tbody/tr[3]/td/table[2]/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr/td/table/tbody/tr/td/div/div[2]/div[1]/div/div[2]/div/div/div[2]"));
                            IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));
                            // int iRowsCount = driver.FindElements(By.XPath("//*[@id='ctl00_m_g_e7e24a7d_b298_4c40_8411_c65d5498a997_ctl00_ctl04_gv_Results']/tbody/tr")).Count;
                            IList<IWebElement> TDmulti;
                            int maxCheck = 0;
                            foreach (IWebElement row in TRmulti)
                            {
                                if (maxCheck <= 25)
                                {
                                    TDmulti = row.FindElements(By.TagName("td"));
                                    if (TDmulti.Count == 4 && TDmulti[0].Text.Trim() != "")
                                    {
                                        string multi1 = TDmulti[0].Text + "~" + TDmulti[1].Text + "~" + TDmulti[2].Text;
                                        gc.insert_date(orderNumber, TDmulti[0].Text, 149, multi1, 1, DateTime.Now);
                                    }
                                    maxCheck++;
                                }
                            }

                            if (TRmulti.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Pinal_Multicount"] = "Maximum";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiparcel_Pinal"] = "Yes";
                            }
                            driver.Quit();
                            return "MultiParcel";
                        }
                        gc.CreatePdf_WOP(orderNumber, "Owner Name search result", driver, "AZ", "Pinal");
                    }

                    try
                    {
                        IWebElement Inodata = driver.FindElement(By.Id("ctl00_m_g_e7e24a7d_b298_4c40_8411_c65d5498a997_ctl00_ctl02_lbl_NoResults"));
                        if(Inodata.Text.Contains("Your search returned 0 results, please try again."))
                        {
                            HttpContext.Current.Session["Nodata_AZPinal"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    //property_details

                    Thread.Sleep(5000);
                    // gc.CreatePdf_WOP(orderNumber, "Parcel search result", driver, "AZ", "Pinal");
                    // Thread.Sleep(3000);
                    parcel_no = driver.FindElement(By.XPath("//*[@id='ctl00_m_g_e7e24a7d_b298_4c40_8411_c65d5498a997_ctl00_ctl06_lbl_ParcelNumber']")).Text.Trim();
                    gc.CreatePdf(orderNumber, parcel_no, "Parcel search result", driver, "AZ", "Pinal");
                    Owner_name = driver.FindElement(By.Id("ctl00_m_g_e7e24a7d_b298_4c40_8411_c65d5498a997_ctl00_ctl06_lbl_Owner1")).Text.Trim();

                    property_Address = driver.FindElement(By.Id("ctl00_m_g_e7e24a7d_b298_4c40_8411_c65d5498a997_ctl00_ctl06_lbl_PropertyAddress")).Text.Trim().Replace("\r\n", "");
                    property_Address = WebDriverTest.Before(property_Address, "Property Address refers to");
                    string address = driver.FindElement(By.Id("ctl00_m_g_e7e24a7d_b298_4c40_8411_c65d5498a997_ctl00_ctl06_lbl_Address")).Text.Trim();
                    string city = driver.FindElement(By.Id("ctl00_m_g_e7e24a7d_b298_4c40_8411_c65d5498a997_ctl00_ctl06_lbl_City")).Text.Trim();
                    string state = driver.FindElement(By.Id("ctl00_m_g_e7e24a7d_b298_4c40_8411_c65d5498a997_ctl00_ctl06_lbl_State")).Text.Trim();
                    string zipcode = driver.FindElement(By.Id("ctl00_m_g_e7e24a7d_b298_4c40_8411_c65d5498a997_ctl00_ctl06_lbl_ZipCode")).Text.Trim();
                    Mailing_address = address + " " + city + " " + state + " " + zipcode.Trim();
                    Legal_Description = driver.FindElement(By.Id("ctl00_m_g_e7e24a7d_b298_4c40_8411_c65d5498a997_ctl00_ctl06_lbl_LegalInformation")).Text.Trim();

                    year_built = driver.FindElement(By.XPath("//*[@id='ctl00_m_g_e7e24a7d_b298_4c40_8411_c65d5498a997_ctl00_acc_Search']/div[4]/div/div[3]/table[4]/tbody/tr/td/table[1]/tbody/tr[2]/td/table/tbody/tr[1]/td[2]/span/span")).Text.Trim();
                    TaxAreaCode = driver.FindElement(By.Id("ctl00_m_g_e7e24a7d_b298_4c40_8411_c65d5498a997_ctl00_ctl06_hyp_TaxAreaCode")).Text.Trim();
                    UseCode = driver.FindElement(By.Id("ctl00_m_g_e7e24a7d_b298_4c40_8411_c65d5498a997_ctl00_ctl06_hyp_UseCode")).Text.Trim();

                    string property_details = Owner_name + "~" + property_Address + "~" + Mailing_address + "~" + Legal_Description + "~" + year_built + "~" + TaxAreaCode + "~" + UseCode;
                    gc.insert_date(orderNumber, parcel_no, 140, property_details, 1, DateTime.Now);


                    //Assessment Details

                    //select tax type from drop down
                    var SerachTax = driver.FindElement(By.Id("ctl00_m_g_e7e24a7d_b298_4c40_8411_c65d5498a997_ctl00_ctl06_ddl_TaxYear"));
                    var selectElement1 = new SelectElement(SerachTax);
                    selectElement1.SelectByIndex(2);




                    try
                    {

                        IAlert alert = driver.SwitchTo().Alert();
                        alert.Accept();
                    }
                    catch
                    {
                    }
                    gc.CreatePdf(orderNumber, parcel_no, "Assessment search result1", driver, "AZ", "Pinal");
                    TaxYear = driver.FindElement(By.XPath("//*[@id='ctl00_m_g_e7e24a7d_b298_4c40_8411_c65d5498a997_ctl00_ctl06_ddl_TaxYear']/option[@selected='selected']")).Text.Trim();
                    FullCashValue = driver.FindElement(By.Id("ctl00_m_g_e7e24a7d_b298_4c40_8411_c65d5498a997_ctl00_ctl06_lbl_FullCashValue")).Text.Trim();
                    LimitedValue = driver.FindElement(By.Id("ctl00_m_g_e7e24a7d_b298_4c40_8411_c65d5498a997_ctl00_ctl06_lbl_LimitedValue")).Text.Trim();
                    RealPropertyRatio = driver.FindElement(By.Id("ctl00_m_g_e7e24a7d_b298_4c40_8411_c65d5498a997_ctl00_ctl06_lbl_RealPropertyRatio")).Text.Trim();
                    AssessedFCV = driver.FindElement(By.Id("ctl00_m_g_e7e24a7d_b298_4c40_8411_c65d5498a997_ctl00_ctl06_lbl_AssessedFCV")).Text.Trim();
                    AssessedLPV = driver.FindElement(By.Id("ctl00_m_g_e7e24a7d_b298_4c40_8411_c65d5498a997_ctl00_ctl06_lbl_AssessLPV")).Text.Trim();

                    string assessment = TaxYear + "~" + FullCashValue + "~" + LimitedValue + "~" + RealPropertyRatio + "~" + AssessedFCV + "~" + AssessedLPV;
                    gc.insert_date(orderNumber, parcel_no, 141, assessment, 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    //Tax Details
                    driver.Navigate().GoToUrl("https://treasurer.pinalcountyaz.gov/ParcelInquiry/");
                    Thread.Sleep(3000);

                    string strparcelNumber1 = parcel_no.Replace(" ", "").Replace("-", "");
                    driver.FindElement(By.XPath("/html/body/div[1]/section/table/tbody/tr/td[2]/div/div[1]/form/div/div/span/span/input")).SendKeys(strparcelNumber1);
                    gc.CreatePdf(orderNumber, parcel_no, "Tax iformation", driver, "AZ", "Pinal");
                    driver.FindElement(By.XPath("/html/body/div[1]/section/table/tbody/tr/td[2]/div/div[1]/form/div/input[1]")).SendKeys(Keys.Enter);
                    Thread.Sleep(3000);
                    //tax history or tax summary
                    gc.CreatePdf(orderNumber, parcel_no, "Tax iformation result", driver, "AZ", "Pinal");
                    List<string> lstrAZ = new List<string>();
                    IWebElement multitableElement = driver.FindElement(By.XPath("/html/body/div[1]/section/table/tbody/tr/td[2]/form/div[1]/table/tbody"));
                    IList<IWebElement> multitableRow = multitableElement.FindElements(By.TagName("tr"));
                    IList<IWebElement> multirowTD;
                    foreach (IWebElement row in multitableRow)
                    {
                        multirowTD = row.FindElements(By.TagName("td"));
                        status = multirowTD[2].Text.Trim();
                        applied_interest = multirowTD[4].Text.Trim();
                        year = multirowTD[1].Text.Trim();
                        multi = multirowTD[1].Text.Trim() + "~" + multirowTD[2].Text.Trim() + "~" + multirowTD[3].Text.Trim() + "~" + multirowTD[4].Text.Trim() + "~" + multirowTD[5].Text.Trim() + "~" + multirowTD[6].Text.Trim();
                        gc.insert_date(orderNumber, multirowTD[0].Text.Trim(), 145, multi, 1, DateTime.Now);
                        total = driver.FindElement(By.XPath("/html/body/div[1]/section/table/tbody/tr/td[2]/form/div[1]/table/tfoot/tr/td[6]")).Text.Trim();
                        amount = driver.FindElement(By.XPath("/html/body/div[1]/section/table/tbody/tr/td[2]/form/div[1]/table/tfoot/tr/td[7]")).Text.Trim();

                        IWebElement dt11 = driver.FindElement(By.XPath("//*[@id='interestDate']"));
                        string interest_date = dt11.GetAttribute("value");
                        string date1 = interest_date.Substring(0, 2);
                        string date2 = interest_date.Substring(3, 2);
                        string date3 = interest_date.Substring(6, 4);

                        if (status.Contains("AZ") || status.Contains("SBPU") || status.Contains("REDC") || status.Contains("PUR"))
                        {



                            if (!applied_interest.Contains("$0.00"))
                            {
                                string strAZURL = "https://treasurer.pinalcountyaz.gov/ParcelInquiry/Parcel/TaxYearDue?parcelNumber=" + parcelNumber + "&taxYear=" + year + "&interestDate=" + date1 + "%2F" + date2 + "%2F" + date3 + "%2000%3A00%3A00";

                                lstrAZ.Add(strAZURL);
                            }
                        }
                    }
                    //Thread.Sleep(4000);
                    foreach (string URL in lstrAZ)
                    {
                        driver.Navigate().GoToUrl(URL);
                        Thread.Sleep(3000);

                        IWebElement dt1 = driver.FindElement(By.XPath("//*[@id='interestDate']"));
                        string interest_date = dt1.GetAttribute("value");

                        string year1 = driver.FindElement(By.XPath("//*[@id='main']/table/tbody/tr/td[2]/form/table/tbody/tr/td[2]/span/span/span[1]")).Text;
                        gc.CreatePdf(orderNumber, parcel_no, "Tax certificate" + year1, driver, "AZ", "Pinal");

                        IWebElement multitableElement2 = driver.FindElement(By.XPath("//*[@id='CertificateGrid']/table/tbody"));

                        IList<IWebElement> multitableRow2 = multitableElement2.FindElements(By.TagName("tr"));
                        IList<IWebElement> multirowTD2;
                        foreach (IWebElement row1 in multitableRow2)
                        {
                            multirowTD2 = row1.FindElements(By.TagName("td"));
                            string tax_certificate = year1 + "~" + multirowTD2[0].Text.Trim() + "~" + interest_date + "~" + multirowTD2[1].Text.Trim() + "~" + multirowTD2[2].Text.Trim() + "~" + multirowTD2[3].Text.Trim();
                            gc.insert_date(orderNumber, strparcelNumber1, 150, tax_certificate, 1, DateTime.Now);

                        }
                        // display text

                        //  string certificate = driver.FindElement(By.XPath("//*[@id='CertificateGrid']/table/tfoot/tr/td[3]/div")).Text.Trim();

                        //  certificate = certificate.Replace("\r\n", "");
                        //  certificate = WebDriverTest.Before(certificate, ":** 2016 Total Due:");
                        //  certificate = WebDriverTest.After(certificate, "* ").Trim();

                        //  string total_due = driver.FindElement(By.XPath("//*[@id='CertificateGrid']/table/tfoot/tr/td[3]/div")).Text.Trim();
                        //  total_due = total_due.Replace("\r\n", "");
                        //  total_due = WebDriverTest.After(total_due, ":** 2016");

                        //values
                        string certificate_due_amount = driver.FindElement(By.XPath("//*[@id='CertificateGrid']/table/tfoot/tr/td[4]/div")).Text.Trim();
                        certificate_due_amount = certificate_due_amount.Replace("\r\n", ",");
                        string s = certificate_due_amount;
                        string[] words = s.Split(',');
                        string first = "", second = "";
                        first = words[0];
                        second = words[1];

                        //  certificate_due_amount = certificate_due_amount.Replace("\r\n", "");
                        string certificate_value = first;
                        string certificate_due_amount_value = second;





                        string certificate_amount = year1 + "~" + "Certificate " + "~" + interest_date + "~" + " " + "~" + " " + "~" + certificate_value;
                        gc.insert_date(orderNumber, strparcelNumber1, 150, certificate_amount, 1, DateTime.Now);

                        string total_amount = year1 + "~" + "Total Due: " + "~" + interest_date + "~" + " " + "~" + " " + "~" + certificate_due_amount_value;
                        gc.insert_date(orderNumber, strparcelNumber1, 150, total_amount, 1, DateTime.Now);


                    }

                    string multi_amount = " " + "~" + " " + "~" + " " + "~" + " " + "~" + total + "~" + amount;
                    gc.insert_date(orderNumber, strparcelNumber1, 145, multi_amount, 1, DateTime.Now);



                    //tax year due
                    driver.FindElement(By.XPath(" /html/body/div[1]/section/table/tbody/tr/td[1]/ul/li[1]/ul/li[3]/a")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, parcel_no, "Tax year due", driver, "AZ", "Pinal");
                    string deli = driver.FindElement(By.XPath("//*[@id='Grid']/table/tbody/tr[2]/td[4]/a")).Text;

                    if (deli == "$0.00")
                    {
                        IWebElement dt = driver.FindElement(By.XPath("//*[@id='interestDate']"));
                        string date = dt.GetAttribute("value");

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
                        driver.FindElement(By.Id("interestDate")).SendKeys(date);

                    }

                    Thread.Sleep(2000);
                    tax_amountIst = driver.FindElement(By.XPath("/html/body/div[1]/section/table/tbody/tr/td[2]/form/div/table/tbody/tr[1]/td[2]")).Text.Trim();
                    tax_amountIInd = driver.FindElement(By.XPath("/html/body/div[1]/section/table/tbody/tr/td[2]/form/div/table/tbody/tr[1]/td[3]")).Text.Trim();
                    tax_amountTotal = driver.FindElement(By.XPath(" /html/body/div[1]/section/table/tbody/tr/td[2]/form/div/table/tbody/tr[1]/td[4]")).Text.Trim();

                    interest_feesIst = driver.FindElement(By.XPath(" /html/body/div[1]/section/table/tbody/tr/td[2]/form/div/table/tbody/tr[2]/td[2]")).Text.Trim();
                    interest_feesIInd = driver.FindElement(By.XPath("/html/body/div[1]/section/table/tbody/tr/td[2]/form/div/table/tbody/tr[2]/td[3]")).Text.Trim();
                    interest_feesTotal = driver.FindElement(By.XPath("/html/body/div[1]/section/table/tbody/tr/td[2]/form/div/table/tbody/tr[2]/td[4]")).Text.Trim();


                    paid_amountIst = driver.FindElement(By.XPath("/html/body/div[1]/section/table/tbody/tr/td[2]/form/div/table/tbody/tr[3]/td[2]")).Text.Trim();
                    paid_amountIInd = driver.FindElement(By.XPath("/html/body/div[1]/section/table/tbody/tr/td[2]/form/div/table/tbody/tr[3]/td[3]")).Text.Trim();
                    paid_amountTotal = driver.FindElement(By.XPath(" /html/body/div[1]/section/table/tbody/tr/td[2]/form/div/table/tbody/tr[3]/td[4]")).Text.Trim();


                    total_dueIst = driver.FindElement(By.XPath(" /html/body/div[1]/section/table/tbody/tr/td[2]/form/div/table/tfoot/tr/td[2]")).Text.Trim();
                    total_dueIInd = driver.FindElement(By.XPath("/html/body/div[1]/section/table/tbody/tr/td[2]/form/div/table/tfoot/tr/td[3]")).Text.Trim();
                    total_duetTotal = driver.FindElement(By.XPath("/html/body/div[1]/section/table/tbody/tr/td[2]/form/div/table/tfoot/tr/td[4]")).Text.Trim();
                    string tax_info = Owner_name + "~" + tax_amountIst + "~" + tax_amountIInd + "~" + tax_amountTotal + "~" + interest_feesIst + "~" + interest_feesIInd + "~" + interest_feesTotal + "~" + paid_amountIst + "~" + paid_amountIInd + "~" + paid_amountTotal + "~" + total_dueIst + "~" + total_dueIInd + "~" + total_duetTotal;

                    gc.insert_date(orderNumber, strparcelNumber1, 144, tax_info, 1, DateTime.Now);



                    // View Bill 
                    var chromeOptions = new ChromeOptions();
                    var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];

                    chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                    chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                    chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");

                    var chDriver = new ChromeDriver(chromeOptions);
                    try
                    {
                        chDriver.Navigate().GoToUrl("https://treasurer.pinalcountyaz.gov/ParcelInquiry/");
                        chDriver.FindElement(By.XPath("/html/body/div[1]/section/table/tbody/tr/td[2]/div/div[1]/form/div/div/span/span/input")).SendKeys(strparcelNumber1);
                        chDriver.FindElement(By.XPath("/html/body/div[1]/section/table/tbody/tr/td[2]/div/div[1]/form/div/input[1]")).SendKeys(Keys.Enter);
                        Thread.Sleep(1000);
                        chDriver.FindElement(By.XPath("//*[@id='PanelBar']/li[1]/ul/li[3]/a")).Click();
                        Thread.Sleep(1000);
                        string parcelURL = chDriver.FindElement(By.XPath("//*[@id='main']/table/tbody/tr/td[2]/form/table/tbody/tr/td[5]/a")).GetAttribute("href");
                        string parNo = "", yr = "", fileName = "";
                        parNo = GlobalClass.After(parcelURL, "parcelNumber");
                        parNo = GlobalClass.Before(parNo, "&").Replace("=", "_");
                        yr = GlobalClass.After(parcelURL, "taxYear").Replace("=", "_");
                        fileName = "TaxBill" + parNo + yr + ".pdf";
                        chDriver.FindElement(By.XPath("//*[@id='main']/table/tbody/tr/td[2]/form/table/tbody/tr/td[5]/a")).Click();
                        Thread.Sleep(3000);
                        gc.AutoDownloadFile(orderNumber, parcelNumber, "Pinal", "AZ", fileName);
                        chDriver.Quit();

                    }
                    catch (Exception ex)
                    {

                    }


                    //  WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                    //    wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("/html/body/div[1]/section/table/tbody/tr/td[2]/form/table/tbody/tr/td[5]/a"))).Click();

                    //string Url = Itaxbill.GetAttribute("href");
                    //string outputPath = ConfigurationManager.AppSettings["screenShotPath-Pinal"];
                    //outputPath = outputPath + orderNumber + "\\TaxBill.pdf";
                    //WebClient downloadTaxBills = new WebClient();
                    //downloadTaxBills.DownloadFile(Url, outputPath);




                    //IWebElement Itaxbill = driver.FindElement(By.XPath("/html/body/div[1]/section/table/tbody/tr/td[2]/form/table/tbody/tr/td[5]/a"));
                    //string URL1 = Itaxbill.GetAttribute("href");
                    //string billpdf = outputPath + "Taxpinal_PayBill.pdf";
                    //WebClient downloadpdf = new WebClient();
                    //downloadpdf.DownloadFile(URL1, billpdf);

                    //IWebElement export = driver.FindElement(By.XPath("//*[@id='main']/table/tbody/tr/td[2]/form/table/tbody/tr/td[5]/a"));
                    //Actions saction = new Actions(driver);
                    //saction.Click(export).Perform();
                    //AutoItX3 autoit = new AutoItX3();
                    //autoit.WinActivate("Opening GPO Plan.xlsx");
                    //Thread.Sleep(3000);
                    //autoit.Send("{ALTDOWN}s{ALTUP}");
                    //autoit.Send("{Enter}");
                    //Thread.Sleep(3000);
                    //autoit.WinActivate("C:\\SeleniumDownloads");
                    //autoit.Send("{Enter}");

                    //Tax Payment History Details Table

                    Thread.Sleep(2000);
                    driver.FindElement(By.XPath("/html/body/div[1]/section/table/tbody/tr/td[1]/ul/li[1]/ul/li[4]/a")).Click();
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, parcel_no, "Tax payment history", driver, "AZ", "Pinal");
                    driver.FindElement(By.XPath("/html/body/div[1]/section/table/tbody/tr/td[2]/form/table/tbody/tr/td[2]/span")).Click();
                    Thread.Sleep(2000);
                    driver.FindElement(By.XPath("//li[contains(text(),'All')]")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, parcel_no, "Tax payment history All", driver, "AZ", "Pinal");
                    IWebElement multitableElement1 = driver.FindElement(By.XPath("/html/body/div[1]/section/table/tbody/tr/td[2]/form/div/table/tbody"));
                    IList<IWebElement> multitableRow1 = multitableElement1.FindElements(By.TagName("tr"));
                    IList<IWebElement> multirowTD1;
                    foreach (IWebElement row in multitableRow1)
                    {
                        multirowTD1 = row.FindElements(By.TagName("td"));

                        string tax_Payment = multirowTD1[2].Text.Trim() + "~" + multirowTD1[3].Text.Trim() + "~" + multirowTD1[4].Text.Trim() + "~" + multirowTD1[5].Text.Trim() + "~" + multirowTD1[6].Text.Trim();
                        gc.insert_date(orderNumber, strparcelNumber1, 146, tax_Payment, 1, DateTime.Now);
                    }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "AZ", "Pinal", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    GlobalClass.titleparcel = "";
                    gc.mergpdf(orderNumber, "AZ", "Pinal");
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