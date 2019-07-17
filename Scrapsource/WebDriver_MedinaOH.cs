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
    public class WebDriver_MedinaOH
    {

        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();

        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_Medina(string address, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            GlobalClass.sname = "OH";
            GlobalClass.cname = "Medina";
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                //  driver = new ChromeDriver();

                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("http://www.medinacountyauditor.org/property-search.htm");
                    Thread.Sleep(4000);
                    if (searchType == "titleflex")
                    {
                        string titleaddress = address;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "OH", "Medina");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_MedinaOH"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";

                    }

                    if (searchType == "address")
                    {
                        driver.FindElement(By.Id("address")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "OH", "Medina");

                        driver.FindElement(By.XPath("//*[@id='propsearch']/p/input[1]")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search result", driver, "OH", "Medina");
                        try
                        {
                            string strowner = "", strAddress = "", strCity = "";
                            IWebElement multiaddress = driver.FindElement(By.XPath("/html/body/div[2]/div/div/div[2]/table/tbody"));
                            IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> multiTD;
                            foreach (IWebElement multi in multiRow)
                            {
                                multiTD = multi.FindElements(By.TagName("td"));
                                if (multiTD.Count != 0 && multiRow.Count < 3)
                                {
                                    IWebElement IsearchClick = multiTD[3].FindElement(By.TagName("a"));
                                    string searchclick = IsearchClick.GetAttribute("href");
                                    driver.Navigate().GoToUrl(searchclick);
                                    break;
                                }
                                if (multiTD.Count != 0 && multiRow.Count > 25)
                                {
                                    HttpContext.Current.Session["multiparcel_MedinaOH_Maximum"] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }
                                if (multiTD.Count != 0 && multiRow.Count > 2 && multiRow.Count <= 25)
                                {
                                    strowner = multiTD[0].Text;
                                    strAddress = multiTD[1].Text;
                                    strCity = multiTD[2].Text;

                                    string multidetails = strowner + "~" + strAddress + "~" + strCity;
                                    gc.insert_date(orderNumber, multiTD[3].Text, 637, multidetails, 1, DateTime.Now);
                                }
                            }
                            if (multiRow.Count > 2 && multiRow.Count <= 25)
                            {
                                HttpContext.Current.Session["multiparcel_MedinaOH"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (multiRow.Count == 1)
                            {
                                HttpContext.Current.Session["Nodata_MedinaOH"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    else if (searchType == "parcel")
                    {
                        driver.FindElement(By.XPath("//*[@id='parcel']")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "parcel search", driver, "OH", "Medina");
                        driver.FindElement(By.XPath("//*[@id='propsearch']/p/input[1]")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcelNumber, "parcel search Result", driver, "OH", "Medina");
                        try
                        {
                            driver.FindElement(By.XPath("/html/body/div[2]/div/div/div[2]/table/tbody/tr[2]/td[4]/a")).SendKeys(Keys.Enter);
                            Thread.Sleep(3000);
                        }
                        catch { }
                        try
                        {
                            IWebElement multiaddress = driver.FindElement(By.XPath("/html/body/div[2]/div/div/div[2]/table/tbody"));
                            IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                            if (multiRow.Count == 1)
                            {
                                HttpContext.Current.Session["Nodata_MedinaOH"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    else if (searchType == "ownername")
                    {
                        //*[@id="owner"]
                        driver.FindElement(By.XPath("//*[@id='owner']")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "OwnerName Search", driver, "OH", "Medina");
                        driver.FindElement(By.XPath("//*[@id='propsearch']/p/input[1]")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "OwnerName Search Results", driver, "OH", "Medina");
                        try
                        {
                            string strowner = "", strAddress = "", strCity = "";
                            IWebElement multiaddress = driver.FindElement(By.XPath("/html/body/div[2]/div/div/div[2]/table/tbody"));
                            IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> multiTD;
                            foreach (IWebElement multi in multiRow)
                            {
                                multiTD = multi.FindElements(By.TagName("td"));
                                if (multiTD.Count != 0 && multiRow.Count < 2)
                                {
                                    IWebElement IsearchClick = multiTD[3].FindElement(By.TagName("a"));
                                    string searchclick = IsearchClick.GetAttribute("href");
                                    driver.Navigate().GoToUrl(searchclick);
                                    break;
                                }
                                if (multiTD.Count != 0 && multiRow.Count > 25)
                                {
                                    HttpContext.Current.Session["multiparcel_MedinaOH_Maximum"] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }
                                if (multiTD.Count != 0 && multiRow.Count > 2 && multiRow.Count <= 25)
                                {
                                    strowner = multiTD[0].Text;
                                    strAddress = multiTD[1].Text;
                                    strCity = multiTD[2].Text;

                                    string multidetails = strowner + "~" + strAddress + "~" + strCity;
                                    gc.insert_date(orderNumber, multiTD[3].Text, 637, multidetails, 1, DateTime.Now);
                                }
                            }
                            if (multiRow.Count > 2 && multiRow.Count <= 25)
                            {
                                HttpContext.Current.Session["multiparcel_MedinaOH"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (multiRow.Count == 1)
                            {
                                HttpContext.Current.Session["Nodata_MedinaOH"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    //property details

                    string parcel_no = "", owner_name = "", Address = "", strMultiAddress = "", property_class = "", average = "", Legal_Description = "", yearbuilt = "";
                    //   Parcel Number 033 - 12C - 17 - 041Owner Name MILLER GARY L & DEBRA LAddress 7015 FAIRHAVEN OVALCity, State, Zip Code MEDINA, OH 44256Property Class 510     Property Class CodesAcreage 2.030000Legal Descrption LOT 11 WH CREEKSIDE CHASE SUB 2.0258A
                    string bulkpropertytext = driver.FindElement(By.XPath("/html/body/div[2]/div/div/div[2]/table/tbody")).Text.Trim().Replace("\r\n", "");

                    parcel_no = gc.Between(bulkpropertytext, "Parcel Number", "Owner Name").Trim();
                    gc.CreatePdf(orderNumber, parcel_no, "property details search", driver, "OH", "Medina");
                    owner_name = gc.Between(bulkpropertytext, "Owner Name", "Address").Trim();
                    Address = GlobalClass.After(bulkpropertytext, "Address").Trim();
                    strMultiAddress = gc.Between(bulkpropertytext, "Address", "Property Class").Trim();
                    property_class = gc.Between(bulkpropertytext, "Property Class", "Property Class Codes").Trim();
                    try
                    {
                        average = gc.Between(bulkpropertytext, "Acreage", "Legal Description").Trim();
                    }
                    catch { }
                    try
                    {
                        Legal_Description = GlobalClass.After(bulkpropertytext, "Legal Description").Trim();
                    }
                    catch { }
                    try
                    {

                        string bulkdwellingtext = driver.FindElement(By.XPath("/html/body/div[4]/div/div/div[2]/table/tbody")).Text.Trim();
                        yearbuilt = gc.Between(bulkdwellingtext, "Year Built", "Story Height");
                    }
                    catch
                    {

                    }

                    string propertydetails = owner_name + "~" + strMultiAddress + "~" + property_class + "~" + average + "~" + Legal_Description + "~" + yearbuilt;
                    gc.insert_date(orderNumber, parcel_no, 618, propertydetails, 1, DateTime.Now);

                    //assessment details
                    string land_value = "", cauv_value = "", building_value = "", total_value = "";
                    //Land Value 98,220CAUV Value 0Building Value 186,280Total Value 284,500
                    string bulkaccesstext = driver.FindElement(By.XPath("/html/body/div[3]/div[1]/div/div[2]/table/tbody")).Text.Trim().Replace("\r\n", "");
                    land_value = gc.Between(bulkaccesstext, "Land Value", "CAUV Value").Trim();
                    cauv_value = gc.Between(bulkaccesstext, "CAUV Value", "Building Value").Trim();
                    building_value = gc.Between(bulkaccesstext, "Building Value", "Total Value").Trim();
                    total_value = GlobalClass.After(bulkaccesstext, "Total Value").Trim();

                    //assessment Tax information
                    string tland_value = "", tcauv_value = "", tbuilding_value = "", ttotal_value = "";
                    string bulktaxabletext = driver.FindElement(By.XPath("/html/body/div[3]/div[2]/div/div[2]/table/tbody")).Text.Trim().Replace("\r\n", "");
                    tland_value = gc.Between(bulktaxabletext, "Taxable Land Value", "Taxable CAUV Value").Trim();
                    tcauv_value = gc.Between(bulktaxabletext, "Taxable CAUV Value", "Taxable Building Value").Trim();
                    tbuilding_value = gc.Between(bulktaxabletext, "Taxable Building Value", "Taxable Total Value").Trim();
                    ttotal_value = GlobalClass.After(bulktaxabletext, "Taxable Total Value").Trim();
                    string assessmentdetails = land_value + "~" + cauv_value + "~" + building_value + "~" + total_value + "~" + tland_value + "~" + tcauv_value + "~" + tbuilding_value + "~" + ttotal_value;
                    gc.insert_date(orderNumber, parcel_no, 619, assessmentdetails, 1, DateTime.Now);

                    //tax bill

                    driver.FindElement(By.LinkText("Tax Bill")).SendKeys(Keys.Enter);
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, parcel_no, "Tax Bill search", driver, "OH", "Medina");
                    string gross_tax1 = "", gross_tax2 = "", reduction_1 = "", reduction_2 = "", sub_total1 = "", sub_total2 = "", nonbusiness_reduction1 = "", nonbusiness_reduction2 = "";
                    string owneroccupied_reduction1 = "", owneroccupied_reduction2 = "", homestead_reduction1 = "", homestead_reduction2 = "", current_tax1 = "", current_tax2 = "", special_assessment1 = "", special_assessment2 = "", total_due1 = "", total_due2 = "", total_paid1 = "", total_paid2 = "", grand_totaldue2 = "";
                    string Sp_A_Penalty1 = "", Sp_A_Penalty2 = "", Sp_A_Penalty3 = "", Sp_A_Penalty4 = "", penalty1 = "", penalty2 = "", special_assessment3 = "", special_assessment4 = "";
                    int i1 = 1;
                    IWebElement tbmulti2;

                    tbmulti2 = driver.FindElement(By.XPath("/html/body/div[2]/div/div/div[2]/table[2]/tbody"));
                    IList<IWebElement> TRmulti2 = tbmulti2.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDmulti2;
                    foreach (IWebElement row in TRmulti2)
                    {
                        TDmulti2 = row.FindElements(By.TagName("td"));
                        if (!row.Text.Contains("Delinquent") && TDmulti2[1].Text.Trim() != "")
                        {

                            if (TDmulti2.Count == 2 && TDmulti2[1].Text.Trim() != "")
                            {

                                if (i1 == 1)
                                {
                                    gross_tax1 = TDmulti2[0].Text;
                                    gross_tax2 = TDmulti2[1].Text;

                                }
                                if (i1 == 2)
                                {
                                    reduction_1 = TDmulti2[0].Text;
                                    reduction_2 = TDmulti2[1].Text;
                                }
                                if (i1 == 3)
                                {
                                    sub_total1 = TDmulti2[0].Text;
                                    sub_total2 = TDmulti2[1].Text;
                                }
                                if (i1 == 4)
                                {
                                    nonbusiness_reduction1 = TDmulti2[0].Text;
                                    nonbusiness_reduction2 = TDmulti2[1].Text;
                                }
                                if (i1 == 5)
                                {
                                    owneroccupied_reduction1 = TDmulti2[0].Text;
                                    owneroccupied_reduction2 = TDmulti2[1].Text;
                                }
                                if (i1 == 6)
                                {
                                    homestead_reduction1 = TDmulti2[0].Text;
                                    homestead_reduction2 = TDmulti2[1].Text;
                                }
                                if (i1 == 7)
                                {
                                    current_tax1 = TDmulti2[0].Text;
                                    current_tax2 = TDmulti2[1].Text;

                                }
                                if ((i1 == 8) && !row.Text.Contains("Penalty"))
                                {
                                    special_assessment1 = TDmulti2[0].Text;
                                    special_assessment2 = TDmulti2[1].Text;

                                }
                                if ((i1 == 9) && !row.Text.Contains("Special Assessment"))
                                {
                                    total_due1 = TDmulti2[0].Text;
                                    total_due2 = TDmulti2[1].Text;
                                }
                                if ((i1 == 10) && !row.Text.Contains("Sp. A. Penalty"))
                                {
                                    total_paid1 = TDmulti2[0].Text;
                                    total_paid2 = TDmulti2[1].Text;
                                }
                                if ((i1 == 11) && !row.Text.Contains("Special Assessment"))
                                {
                                    // grand_totaldue1 = TDmulti2[0].Text;
                                    grand_totaldue2 = TDmulti2[1].Text;
                                }
                                try
                                {
                                    if ((i1 == 8) && row.Text.Contains("Penalty"))
                                    {
                                        penalty1 = TDmulti2[0].Text;
                                        penalty2 = TDmulti2[1].Text;

                                    }
                                    if ((i1 == 9) && row.Text.Contains("Special Assessment"))
                                    {
                                        special_assessment1 = TDmulti2[0].Text;
                                        special_assessment2 = TDmulti2[1].Text;
                                    }
                                    if ((i1 == 10) && row.Text.Contains("Sp. A. Penalty"))
                                    {
                                        Sp_A_Penalty1 = TDmulti2[0].Text;
                                        Sp_A_Penalty2 = TDmulti2[1].Text;
                                    }
                                    if ((i1 == 11) && !row.Text.Contains("Special Assessment"))
                                    {
                                        special_assessment3 = TDmulti2[0].Text;
                                        special_assessment4 = TDmulti2[1].Text;
                                    }
                                    if ((i1 == 12) && row.Text.Contains("Sp. A. Penalty"))
                                    {
                                        Sp_A_Penalty3 = TDmulti2[0].Text;
                                        Sp_A_Penalty4 = TDmulti2[1].Text;
                                    }
                                    if ((i1 == 13) && row.Text.Contains("Total Due"))
                                    {
                                        total_due1 = TDmulti2[0].Text;
                                        total_due2 = TDmulti2[1].Text;
                                    }
                                    if ((i1 == 14) && row.Text.Contains("Total Paid"))
                                    {
                                        total_paid1 = TDmulti2[0].Text;
                                        total_paid2 = TDmulti2[1].Text;
                                    }
                                    if ((i1 == 15) && row.Text.Contains("Grand Total Due"))
                                    {
                                        grand_totaldue2 = TDmulti2[1].Text;
                                    }
                                }
                                catch { }

                            }
                            i1++;
                        }

                    }
                    string taxbill1 = "FirstHalf" + "~" + gross_tax1 + "~" + reduction_1 + "~" + sub_total1 + "~" + nonbusiness_reduction1 + "~" + owneroccupied_reduction1 + "~" + homestead_reduction1 + "~" + current_tax1 + "~" + special_assessment1 + "~" + total_due1 + "~" + total_paid1;
                    gc.insert_date(orderNumber, parcel_no, 620, taxbill1, 1, DateTime.Now);
                    string taxbill2 = "SecondHalf" + "~" + gross_tax2 + "~" + reduction_2 + "~" + sub_total2 + "~" + nonbusiness_reduction2 + "~" + owneroccupied_reduction2 + "~" + homestead_reduction2 + "~" + current_tax2 + "~" + special_assessment2 + "~" + total_due2 + "~" + total_paid2 + "~" + grand_totaldue2;
                    gc.insert_date(orderNumber, parcel_no, 620, taxbill2, 1, DateTime.Now);

                    //Tax Distribution Details Table
                    driver.FindElement(By.XPath("/html/body/div[2]/div/div/div[2]/p[1]/a[5]")).SendKeys(Keys.Enter);
                    Thread.Sleep(3000);
                    try
                    {

                        IWebElement taxdb1 = driver.FindElement(By.XPath("/html/body/div[2]/div/div/div[2]/center"));
                        string taxdb = "";
                        taxdb = taxdb1.Text;
                        if (taxdb.Contains("No transfers found for parcel"))
                        {
                            driver.FindElement(By.XPath("/html/body/div[2]/div/div/div[2]/p[1]/a[5]")).SendKeys(Keys.Enter);
                            Thread.Sleep(2000);
                        }
                        else
                        {

                        }
                    }
                    catch { }
                    gc.CreatePdf(orderNumber, parcel_no, "Tax Distribution Table search", driver, "OH", "Medina");
                    IWebElement tbmulti = driver.FindElement(By.XPath("/html/body/div[2]/div/div/div[2]/table/tbody"));
                    IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));

                    IList<IWebElement> TDmulti;
                    foreach (IWebElement row in TRmulti)
                    {

                        TDmulti = row.FindElements(By.TagName("td"));
                        if (TDmulti.Count == 5)
                        {
                            string multi1 = TDmulti[0].Text + "~" + TDmulti[1].Text + "~" + TDmulti[2].Text + "~" + TDmulti[3].Text + "~" + TDmulti[4].Text;
                            gc.insert_date(orderNumber, parcel_no, 622, multi1, 1, DateTime.Now);
                        }
                        if (TDmulti.Count == 1 && row.Text.Contains("Total"))
                        {
                            string multi1 = "" + "~" + "" + "~" + "Total" + "~" + "" + "~" + TDmulti[0].Text;
                            gc.insert_date(orderNumber, parcel_no, 622, multi1, 1, DateTime.Now);
                        }

                    }

                    //special Assessment Details

                    driver.FindElement(By.XPath("/html/body/div[2]/div/div/div[2]/p[1]/a[6]")).SendKeys(Keys.Enter);
                    Thread.Sleep(1000);
                    try
                    {

                        IWebElement taxdb1 = driver.FindElement(By.XPath("/html/body/div[2]/div/div/div[2]/center"));
                        string taxdb = "";
                        taxdb = taxdb1.Text;
                        if (taxdb.Contains("No transfers found for parcel"))
                        {
                            driver.FindElement(By.XPath("/html/body/div[2]/div/div/div[2]/p[1]/a[6]")).SendKeys(Keys.Enter);
                            Thread.Sleep(2000);
                        }
                        else
                        {

                        }
                    }
                    catch { }
                    gc.CreatePdf(orderNumber, parcel_no, "Special Assessment Details search", driver, "OH", "Medina");
                    int i2 = 0;
                    string first_half = "", second_half = "", charge_no1 = "", charge_no2 = "", charge_amount1 = "", charge_amount2 = "", penalty_1 = "", penalty_2 = "", penalty_amount1 = "", penalty_amount2 = "", paid_1 = "", paid_amount1 = "", paid_2 = "", paid_amount2 = "", half_due1 = "", half_due2 = "", halfdue_amount1 = "", halfdue_amount2 = "";
                    try
                    {
                        IWebElement tbmulti3 = driver.FindElement(By.XPath("/html/body/div[2]/div/div/div[2]/table[2]/tbody"));
                        IList<IWebElement> TRmulti3 = tbmulti3.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDmulti3;
                        IList<IWebElement> THmulti3;
                        foreach (IWebElement row in TRmulti3)
                        {
                            TDmulti3 = row.FindElements(By.TagName("td"));
                            THmulti3 = row.FindElements(By.TagName("th"));
                            if (THmulti3.Count != 0)
                            {
                                if (i2 == 0)
                                {
                                    first_half = THmulti3[0].Text;

                                    second_half = THmulti3[1].Text;

                                }
                            }
                            if (TDmulti3.Count != 0)
                            {

                                if (i2 == 1)
                                {
                                    charge_no1 = TDmulti3[0].Text.Replace("\r\n", " ").Replace("Charge", "");
                                    charge_amount1 = TDmulti3[1].Text;
                                    charge_no2 = TDmulti3[2].Text.Replace("\r\n", " ").Replace("Charge", "");
                                    charge_amount2 = TDmulti3[3].Text;

                                }
                                if (i2 == 2)
                                {
                                    penalty_1 = TDmulti3[0].Text.Replace("\r\n", " ").Replace("Penalty", "");
                                    penalty_amount1 = TDmulti3[1].Text;
                                    penalty_2 = TDmulti3[2].Text.Replace("\r\n", " ").Replace("Penalty", "");
                                    penalty_amount2 = TDmulti3[3].Text;

                                }
                                if (i2 == 3)
                                {
                                    paid_1 = TDmulti3[0].Text.Replace("\r\n", " ").Replace("Paid", "");
                                    paid_amount1 = TDmulti3[1].Text;
                                    paid_2 = TDmulti3[2].Text.Replace("\r\n", " ").Replace("Paid", "");
                                    paid_amount2 = TDmulti3[3].Text;
                                }
                                if (i2 == 4)
                                {
                                    half_due1 = TDmulti3[0].Text.Replace("\r\n", " ").Replace("Due", "");
                                    halfdue_amount1 = TDmulti3[1].Text;
                                    half_due2 = TDmulti3[2].Text.Replace("\r\n", " ").Replace("Due", "");
                                    halfdue_amount2 = TDmulti3[3].Text;
                                }
                                i2++;
                            }

                        }
                        string firsthalf_splassessment = first_half + "~" + charge_no1 + "~" + charge_amount1 + "~" + penalty_1 + "~" + penalty_amount1 + "~" + paid_1 + "~" + paid_amount1 + "~" + half_due1 + "~" + halfdue_amount1;
                        gc.insert_date(orderNumber, parcel_no, 623, firsthalf_splassessment, 1, DateTime.Now);
                        string secondhalf_splassessment = second_half + "~" + charge_no2 + "~" + charge_amount2 + "~" + penalty_2 + "~" + penalty_amount2 + "~" + paid_2 + "~" + paid_amount2 + "~" + half_due2 + "~" + halfdue_amount2;
                        gc.insert_date(orderNumber, parcel_no, 623, secondhalf_splassessment, 1, DateTime.Now);
                    }
                    catch { }
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    // Tax information

                    driver.Navigate().GoToUrl("https://www.medinacountytax.com/taxes.html#/WildfireSearch");
                    Thread.Sleep(4000);

                    gc.CreatePdf(orderNumber, parcel_no, "Tax Information search", driver, "OH", "Medina");
                    //try
                    //{
                    //    driver.FindElement(By.XPath("//*[@id='vertNav2']/a[1]")).Click();
                    //    Thread.Sleep(5000);
                    //}
                    //catch { }
                    //try
                    //{
                    //    driver.FindElement(By.XPath("//*[@id='avalon']/div/p[3]/button[1]/span")).Click();
                    //    Thread.Sleep(5000);
                    //}
                    //catch { }
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[3]/button[2]")).SendKeys(Keys.Enter);
                        Thread.Sleep(4000);
                    }
                    catch { }
                    gc.CreatePdf(orderNumber, parcel_no, "Tax Information Result", driver, "OH", "Medina");
                    driver.FindElement(By.XPath("//*[@id='searchBox']")).SendKeys(parcel_no);
                    Thread.Sleep(5000);
                    gc.CreatePdf(orderNumber, parcel_no, "Tax Information Result view", driver, "OH", "Medina");
                    IWebElement ISpan12 = null;
                    try
                    {
                        ISpan12 = driver.FindElement(By.XPath("//*[@id='avalon']/div/div[3]/div[2]/table/tbody/tr/td[6]/button"));
                    }
                    catch { }
                    try
                    {
                        ISpan12 = driver.FindElement(By.XPath("//*[@id='avalon']/div/div[4]/div[2]/table/tbody/tr/td[6]/button"));
                    }
                    catch { }
                    IJavaScriptExecutor js12 = driver as IJavaScriptExecutor;
                    js12.ExecuteScript("arguments[0].click();", ISpan12);
                    Thread.Sleep(3000);

                    // Tax payer Information 
                    string taxpayer_information = "";
                    string bulktaxtext4 = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[2]/div[2]")).Text.Trim().Replace("\r\n", "");

                    taxpayer_information = GlobalClass.After(bulktaxtext4, "Taxpayer Information").Trim();

                    string property_type = "", loacation_name = "", tax_district = "", school_district = "", land_use = "";
                    string bulktaxtext = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[3]/div[1]/table[1]/tbody")).Text.Trim().Replace("\r\n", "");
                    property_type = gc.Between(bulktaxtext, "Property Type", "Location").Trim();
                    loacation_name = gc.Between(bulktaxtext, "Location", "Tax District").Trim();
                    tax_district = gc.Between(bulktaxtext, "Tax District", "School District").Trim();
                    school_district = gc.Between(bulktaxtext, "School District", "Land Use").Trim();
                    land_use = gc.Between(bulktaxtext, "Land Use", "Acres").Trim();

                    // assessment information
                    string annual_tax = "", homestead_exemption = "", owneroccupied_reduction3 = "", escrow_1 = "";
                    string bulktaxtext2 = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[4]/div[1]/table/tbody")).Text.Trim().Replace("\r\n", "");
                    annual_tax = gc.Between(bulktaxtext2, "Annual Real Estate Tax", "Homestead Exemption").Trim();
                    homestead_exemption = gc.Between(bulktaxtext2, "Homestead Exemption", "Owner Occupied Reduction").Trim();
                    owneroccupied_reduction3 = gc.Between(bulktaxtext2, "Owner Occupied Reduction", "Escrow").Trim();
                    escrow_1 = gc.Between(bulktaxtext2, "Escrow", "Lender ID").Trim();

                    string taxing_authority = "Medina County Treasurer's Office Medina County Administration Building 144 North Broadway Medina OH 44256Phone 330 - 725 - 9748";

                    // Tax information
                    gc.CreatePdf(orderNumber, parcel_no, "Tax Information1", driver, "OH", "Medina");
                    int i3 = 0, j = 1; ;
                    string first_hf = "", fhtax_amount = "", fhpaid_amount = "", fhbalance_amount = "", due_date = "", second_hf = "", sectax_amount = "", secpaid_amount = "", due_date2 = "", secbalance_amount = "", afsec_half = "", afsectax_amount = "", afsecpaid_amount = "", totaldue_amount = "", DelinquentAmount = "", DelinquentPaid = "";
                    IWebElement tbmulti1 = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[3]/div[2]/table/tbody"));
                    IList<IWebElement> TRmulti1 = tbmulti1.FindElements(By.TagName("tr"));

                    IList<IWebElement> TDmulti1;
                    foreach (IWebElement row in TRmulti1)
                    {

                        TDmulti1 = row.FindElements(By.TagName("td"));
                        if (TDmulti1.Count == 2)
                        {
                            totaldue_amount = TDmulti1[1].Text;
                        }
                        if (TDmulti1.Count == 3 && TDmulti1[1].Text.Trim() != "" && !row.Text.Contains("Delinquent"))
                        {

                            if (i3 == 0)
                            {
                                first_hf = TDmulti1[0].Text;
                                fhtax_amount = TDmulti1[2].Text;
                            }


                            if (i3 == 1)
                            {
                                fhpaid_amount = TDmulti1[2].Text;
                            }
                            if (i3 == 2)
                            {
                                due_date = TDmulti1[0].Text.Replace("\r\n", " ");

                                fhbalance_amount = TDmulti1[2].Text;
                            }

                            if (i3 == 3)
                            {
                                second_hf = TDmulti1[0].Text;

                                sectax_amount = TDmulti1[2].Text;

                            }
                            if (i3 == 4)
                            {
                                secpaid_amount = TDmulti1[2].Text;
                            }
                            if (i3 == 5)
                            {
                                due_date2 = TDmulti1[0].Text.Replace("\r\n", " ");

                                secbalance_amount = TDmulti1[2].Text;

                            }
                            if (i3 == 6)
                            {
                                afsec_half = TDmulti1[0].Text;
                                afsectax_amount = TDmulti1[2].Text;
                            }
                            if (i3 == 7)
                            {
                                afsecpaid_amount = TDmulti1[2].Text;
                            }
                            i3++;

                        }
                        if (TDmulti1.Count == 3 && TDmulti1[1].Text.Trim() != "" && row.Text.Contains("Delinquent"))
                        {
                            if (j == 1)
                            {
                                DelinquentAmount = TDmulti1[2].Text;

                            }
                            if (j == 2)
                            {
                                DelinquentPaid = TDmulti1[2].Text;
                            }
                            j++;

                        }
                    }
                    string taxinformation = taxpayer_information + "~" + property_type + "~" + loacation_name + "~" + tax_district + "~" + school_district + "~" + land_use + "~" + annual_tax + "~" + homestead_exemption + "~" + owneroccupied_reduction3 + "~" + escrow_1 + "~" + DelinquentAmount + "~" + DelinquentPaid + "~" + first_hf + "~" + due_date + "~" + fhtax_amount + "~" + fhpaid_amount + "~" + fhbalance_amount + "~" + " " + "~" + taxing_authority;
                    gc.insert_date(orderNumber, parcel_no, 624, taxinformation, 1, DateTime.Now);

                    string taxinformation1 = "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + second_hf + "~" + due_date2 + "~" + sectax_amount + "~" + secpaid_amount + "~" + secbalance_amount + "~" + " " + "~" + "";
                    gc.insert_date(orderNumber, parcel_no, 624, taxinformation1, 1, DateTime.Now);

                    string taxinformation2 = "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + afsec_half + "~" + "" + "~" + "" + "~" + afsecpaid_amount + "~" + afsectax_amount + "~" + totaldue_amount + "~" + "";
                    gc.insert_date(orderNumber, parcel_no, 624, taxinformation2, 1, DateTime.Now);


                    //   Tax History Details Table:
                    try
                    {
                        IWebElement ITaxHistory = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/ul/li[5]/a"));
                        IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                        js.ExecuteScript("arguments[0].click();", ITaxHistory);
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, parcel_no, "Tax History Details search", driver, "OH", "Medina");
                        IWebElement tbmulti4 = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[5]/div/div/table/tbody"));
                        IList<IWebElement> TRmulti4 = tbmulti4.FindElements(By.TagName("tr"));

                        IList<IWebElement> TDmulti4;
                        foreach (IWebElement row in TRmulti4)
                        {

                            TDmulti4 = row.FindElements(By.TagName("td"));
                            if (TDmulti4.Count == 7)
                            {
                                string multi4 = TDmulti4[0].Text + "~" + TDmulti4[1].Text + "~" + TDmulti4[2].Text + "~" + TDmulti4[3].Text + "~" + TDmulti4[4].Text + "~" + TDmulti4[5].Text + "~" + TDmulti4[6].Text;
                                gc.insert_date(orderNumber, parcel_no, 627, multi4, 1, DateTime.Now);
                            }

                        }
                    }
                    catch { }
                    // view & Print Bill
                    try
                    {
                        IWebElement IViewPrint = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/ul/li[3]/a"));
                        IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                        js.ExecuteScript("arguments[0].click();", IViewPrint);
                        Thread.Sleep(6000);
                        gc.CreatePdf(orderNumber, parcel_no, "View and Print Bill search", driver, "OH", "Medina");
                    }
                    catch { }
                    //download taxbill
                    
                    try
                    {
                        IWebElement Itaxbill = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[4]/div/div[1]/a"));
                        string URL1 = Itaxbill.GetAttribute("href");

                        string fileName = "";
                        var chromeOptions = new ChromeOptions();
                        var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];

                        chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                        chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                        chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                        chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                        var chDriver = new ChromeDriver(chromeOptions);
                        try
                        {
                            chDriver.Navigate().GoToUrl(URL1);
                            Thread.Sleep(3000);
                            try
                            {
                                fileName = "Bill.pdf";
                                gc.AutoDownloadFileSpokane(orderNumber, parcel_no, "Medina", "OH", fileName);

                            }
                            catch { }
                            chDriver.Quit();
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    catch { }

                    // view and print receipt
                    try
                    {
                        IWebElement IPrintReciept = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/ul/li[4]/a"));
                        IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                        js.ExecuteScript("arguments[0].click();", IPrintReciept);
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcel_no, "Print Receipt", driver, "OH", "Medina");
                    }
                    catch { }
                    // download receipt

                    

                    try
                    {
                        IWebElement Ireceipt = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[4]/div/div[1]/a"));
                        string URL2 = Ireceipt.GetAttribute("href");

                        string fileName = "";
                        var chromeOptions = new ChromeOptions();
                        var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];

                        chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                        chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                        chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                        chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                        var chDriver = new ChromeDriver(chromeOptions);
                        try
                        {
                            chDriver.Navigate().GoToUrl(URL2);
                            Thread.Sleep(3000);
                            try
                            {
                                fileName = "Bill.pdf";
                                gc.AutoDownloadFileSpokane(orderNumber, parcel_no,"Medina", "OH", fileName);
                            }
                            catch { }
                            chDriver.Quit();
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    catch { }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "OH", "Medina", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "OH", "Medina");
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