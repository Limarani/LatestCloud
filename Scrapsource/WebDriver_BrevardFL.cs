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
    public class WebDriver_BrevardFL
    {
        IWebDriver driver;
        IList<IWebElement> taxPaymentdetails, taxPaymentAmountdetails, Itaxtd;
        List<string> strSecured = new List<string>();
        List<string> strCombinedTax = new List<string>();
        List<string> strTaxRealestate = new List<string>();
        List<string> downloadbill = new List<string>();
        List<string> strTaxRealCount = new List<string>();
        List<string> IssuedURL = new List<string>();
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();
        string strMulti = "-", strParcel = "-", strMultiDetails = "-", strParcelNumber = "-", strOwner = "-", strAddress = "-", strProeprtyAddress = "-", strOwnerName = "-",
              strMailingAddress = "-", strLegalDiscription = "-", strYear = "-", strLand = "-", strBuilding = "-", strMarket = "-", strAssessed = "-", strTax = "-",
              strType = "-", strTaxType = "-", strAlternetKey = "-", strEscrowCode = "-", strMillageCode = "-", strTaxAuthority = "-", strMillege = "-", strAssess = "-",
              strExemption = "-", strTaxable = "-", strTaxAmount = "-", strParcelNo = "-", strTaxCom = "-", strLInk = "-", strDate = "-", strAmount = "-", strFace = "-",
              strFBill = "-", strFBalance = "-", strFBillDate = "-", strFBillPaid = "-", strbillTaxAuthority = "-", strbillMillege = "-", strbillAssess = "-",
              strbillExemption = "-", strbillTaxable = "-", strbillTaxAmount = "-", strValAuthority = "-", strValRate = "-", strValAmount = "-", strValoremAuthority = "-",
              strValoremRate = "-", strValoremAmount = "-", strAssessTitle = "-", strCounty = "-", strSchool = "-", strMunicipal = "-", strIndependent = "-", strTaxBill = "",
              strMillageRate = "-", strIssue = "", strIssuePaid = "", strissueTaxType = "", strissueAlternetKey = "", strissueMillageCode = "", strissueMillageRate = "", strissueParcelNo = "-",
              strVTaxType = "-", strNVTaxType = "-", strcombine = "-", strLTaxAuthority = "-", strLMillege = "-", strLAssess = "-", strLExemption = "-", strLTaxable = "-", strLTaxAmount = "-",
               strTaxFeed = "", strBillEff = "", strRate = "", yearbuilt = "", strBillPayReceipt = "", parcelid = "", accno = "", latestfile = "", latestfile1 = "";
        int MultiCount, AnualBillCount, TaxCount = 0, ParcelCount = 0;
        string Parcel_number = "", Tax_Authority = "", Year = "", Adderess = "", YearBuilt = "", taxresult3 = "", Firsthalf = "", Secondhalf = "", fullhalf = "", prepayresult1 = "", prepayresult2 = "", prepayresult3 = "", paiedamt = "";
        string outparcelno = "", taxparcel = "", mailingaddress = "", tax_district = "", siteaddr = "", legal_desc = "", year_built = "", propuse = "", par1 = "", par2 = "";
        string Account = "", LandValue = "", JustValue = "", cctaxyear = "", Cap = "", AssessedValue = "", Exemption = "", TaxableValue = "";
        string strBill = "-", strBalance = "-", strBillDate = "-", strBillPaid = "-";
        string taxowner = "", tax_addr = "", alterkey = "", millagecode = "", milagerate = "", ctax_year = "", cpaiddate = "", strbillyear = "", cpaidamount = "", creceipt = "", combinedtaxamount = "", ceffdate = "", grosstax = "", ifpaidby = "", pleasepay = "";
        public string FTP_FLBrevard(string houseno, string direction, string sname, string sttype, string unitnumber, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            List<string> strissuecertificate = new List<string>();
            List<string> taxhistorylink = new List<string>();
            List<string> downloadlink = new List<string>();
            List<string> taxhistorylinkinst = new List<string>();
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            // driver = new PhantomJSDriver(); //ChromeDriver();
            // driver = new ChromeDriver();
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("https://www.bcpao.us/PropertySearch/#/nav/Search");
                    Thread.Sleep(3000);
                    string address = "";

                    if (houseno != "" && unitnumber != "")
                    {
                        if (direction != "")
                        {
                            address = houseno + " " + direction + " " + sname + " " + sttype + " " + unitnumber;
                        }
                        else
                        {
                            address = houseno + " " + sname + " " + sttype + " " + unitnumber;
                        }
                    }

                    if (direction != "")
                    {
                        address = houseno + " " + direction + " " + sname + " " + sttype;
                    }

                    else
                    {
                        address = houseno + " " + sname + " " + sttype;
                    }

                    //if (searchType == "titleflex")
                    //{
                    //    string address = houseno + " " + strDirection + " " + sname + " " + strStreetType + unitnumber;
                    //    gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "FL", "Brevard");

                    //    if (HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes")
                    //    {
                    //        return "MultiParcel";
                    //    }
                    //    parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                    //    parcelNumber = HttpContext.Current.Session["titleparcel"].ToString().Replace(".", "").Replace("-", "");
                    //    searchType = "parcel";
                    //}

                    if (searchType == "address")
                    {
                        driver.FindElement(By.Id("txtPropertySearch_Address")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "FL", "Brevard");
                        driver.FindElement(By.Id("btnPropertySearch_RealProperty_Go")).Click();
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "FL", "Brevard");

                        try
                        {
                            string nodata = driver.FindElement(By.Id("divNoResultsFound")).Text;
                            if (nodata.Contains("NO RESULTS FOUND"))
                            {
                                HttpContext.Current.Session["Nodata_BrevardFL"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }

                        }
                        catch { }
                        try
                        {
                            strMulti = driver.FindElement(By.Id("Results_Count")).Text;
                            string strMultiCount = gc.Between(strMulti, "of ", " records");
                            if (Convert.ToInt32(strMultiCount) > 25)
                            {
                                HttpContext.Current.Session["multiparcel_FLBrevard_Maximum"] = "Yes";
                                driver.Quit();
                                return "Maximum";
                            }
                            IWebElement IMulti = driver.FindElement(By.XPath("//*[@id='tblSearchResults']/tbody"));
                            IList<IWebElement> IMultirow = IMulti.FindElements(By.XPath("tr"));
                            IList<IWebElement> IMultitd;
                            foreach (IWebElement row in IMultirow)
                            {
                                //GlobalClass.multiPArcel_FLBroward = "Yes";
                                HttpContext.Current.Session["multiPArcel_FLBrevard"] = "Yes";
                                IMultitd = row.FindElements(By.XPath("td"));
                                if (IMultitd.Count != 0 && MultiCount <= 25)
                                {
                                    try
                                    {
                                        MultiCount++;
                                        strParcel = IMultitd[2].Text;
                                        strAddress = IMultitd[3].Text;
                                        strOwner = IMultitd[4].Text;
                                        strMultiDetails = strAddress.Trim() + "~" + strOwner.Trim();
                                        gc.insert_date(orderNumber, strParcel, 1513, strMultiDetails, 1, DateTime.Now);
                                    }
                                    catch { }
                                }
                            }
                            driver.Quit();
                            return "MultiParcel";
                        }
                        catch { }

                    }
                    else if (searchType == "parcel")
                    {
                        driver.FindElement(By.Id("txtPropertySearch_Pid")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber.Replace("-*",""), "Parcel Search", driver, "FL", "Brevard");
                        driver.FindElement(By.Id("btnPropertySearch_RealProperty_Go")).Click();
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcelNumber.Replace("-*", ""), "Parcel Search Result", driver, "FL", "Brevard");
                        try
                        {
                            string nodata = driver.FindElement(By.Id("divNoResultsFound")).Text;
                            if (nodata.Contains("NO RESULTS FOUND"))
                            {
                                HttpContext.Current.Session["Nodata_BrevardFL"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }

                        }
                        catch { }
                    }
                    else if (searchType == "account")
                    {
                        driver.FindElement(By.Id("txtPropertySearch_Account")).SendKeys(unitnumber);
                        gc.CreatePdf(orderNumber, unitnumber, "Account Search", driver, "FL", "Brevard");
                        driver.FindElement(By.Id("btnPropertySearch_RealProperty_Go")).Click();
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, unitnumber, "Account Search Result", driver, "FL", "Brevard");
                        try
                        {
                            string nodata = driver.FindElement(By.Id("divNoResultsFound")).Text;
                            if (nodata.Contains("NO RESULTS FOUND"))
                            {
                                HttpContext.Current.Session["Nodata_BrevardFL"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }

                        }
                        catch { }
                    }
                    else if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("txtPropertySearch_Owner")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "Owner Search", driver, "FL", "Brevard");
                        driver.FindElement(By.Id("btnPropertySearch_RealProperty_Go")).Click();
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Owner Search Result", driver, "FL", "Brevard");
                        try
                        {
                            strMulti = driver.FindElement(By.Id("Results_Count")).Text;
                            string strMultiCount = gc.Between(strMulti, "of ", " records");
                            if (Convert.ToInt32(strMultiCount) > 25)
                            {
                                HttpContext.Current.Session["multiparcel_FLBrevard_Maximum"] = "Yes";
                                driver.Quit();
                                return "Maximum";
                            }
                            IWebElement IMulti = driver.FindElement(By.XPath("//*[@id='tblSearchResults']/tbody"));
                            IList<IWebElement> IMultirow = IMulti.FindElements(By.XPath("tr"));
                            IList<IWebElement> IMultitd;
                            foreach (IWebElement row in IMultirow)
                            {
                                HttpContext.Current.Session["multiPArcel_FLBrevard"] = "Yes";
                                // GlobalClass.multiPArcel_FLBroward = "Yes";
                                IMultitd = row.FindElements(By.XPath("td"));
                                if (IMultitd.Count != 0 && MultiCount <= 25)
                                {
                                    try
                                    {
                                        MultiCount++;
                                        strParcel = IMultitd[2].Text;
                                        strAddress = IMultitd[3].Text;
                                        strOwner = IMultitd[4].Text;
                                        strMultiDetails = strAddress.Trim() + "~" + strOwner.Trim();
                                        gc.insert_date(orderNumber, strParcel, 1513, strMultiDetails, 1, DateTime.Now);
                                    }
                                    catch { }
                                }
                            }
                            driver.Quit();
                            return "MultiParcel";
                        }
                        catch { }

                        try
                        {
                            string nodata = driver.FindElement(By.Id("divNoResultsFound")).Text;
                            if (nodata.Contains("NO RESULTS FOUND"))
                            {
                                HttpContext.Current.Session["Nodata_BrevardFL"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }

                        }
                        catch { }
                    }

                    //Property Details 
                    string owner = "", mailaddr = "", siteaddr = "", taxingdist = "", exemp = "", prop_use = "", subdivname = "", landdesc = "";
                    accno = driver.FindElement(By.Id("divPropertySearch_Details_Account")).Text;
                    accno = GlobalClass.After(accno, "Account:").Trim();//*[@id="cssDetails_Top_Outer"]/div[2]/div
                    string bulkproeprty = driver.FindElement(By.XPath("//*[@id='cssDetails_Top_Outer']/div[2]")).Text;
                    owner = gc.Between(bulkproeprty, "Owners:", "Mail Address:");
                    mailaddr = gc.Between(bulkproeprty, "Mail Address:", "Site Address:");
                    siteaddr = gc.Between(bulkproeprty, "Site Address:", "Parcel ID:");
                    parcelid = gc.Between(bulkproeprty, "Parcel ID:", "Taxing District:");
                    taxingdist = gc.Between(bulkproeprty, "Taxing District:", "Exemptions:");
                    exemp = gc.Between(bulkproeprty, "Exemptions:", "Property Use:");
                    prop_use = gc.Between(bulkproeprty, "Property Use:", "Total Acres:");
                    subdivname = gc.Between(bulkproeprty, "Subdivision Name:", "Land Description:");
                    landdesc = GlobalClass.After(bulkproeprty, "Land Description:");
                    try
                    {
                        string yearbuilttext = driver.FindElement(By.XPath("//*[@id='divBldg_Details']/table/tbody")).Text;
                        yearbuilt = gc.Between(yearbuilttext, "Year Built:", "Story Height:").Trim();
                    }
                    catch { }
                    //string strPropertyDetails = accno + "~" + owner + "~" + mailaddr + "~" + siteaddr + "~" + taxingdist + "~" + exemp + "~" + prop_use + "~" + subdivname + "~" + landdesc + "~" + yearbuilt;
                    //gc.insert_date(orderNumber, parcelid, 1507, strPropertyDetails, 1, DateTime.Now);
                    gc.CreatePdf(orderNumber, accno, "Property Search Result", driver, "FL", "Brevard");

                    //Year~Market Value~Agricultural Land Value~Assessed Value Non School~Assessed Value School~Homestead Exemption~Additional Homestead~Other Exemptions~Taxable Value Non School~Taxable Value School
                    IWebElement valuetableElement = driver.FindElement(By.XPath("//*[@id='divValues_Description']/div/table"));
                    IList<IWebElement> valuetableRow = valuetableElement.FindElements(By.TagName("tr"));
                    IList<IWebElement> valuetableheader = valuetableElement.FindElements(By.TagName("th"));

                    IList<IWebElement> valuerowTD;
                    IList<IWebElement> valuerowTH;
                    List<string> year = new List<string>();
                    List<string> MarketValue = new List<string>();
                    List<string> AgriculturalLandValue = new List<string>();
                    List<string> asvnonschool = new List<string>();
                    List<string> asvschool = new List<string>();
                    List<string> homeexem = new List<string>();
                    List<string> addhome = new List<string>();
                    List<string> otherexem = new List<string>();
                    List<string> taxvaluenonschool = new List<string>();
                    List<string> taxvalueschool = new List<string>();
                    int I = 0;
                    foreach (IWebElement row in valuetableRow)
                    {
                        valuerowTD = row.FindElements(By.TagName("td"));
                        valuerowTH = row.FindElements(By.TagName("th"));

                        if (valuerowTH.Count != 0)
                        {
                            year.Add(valuerowTH[1].Text);
                            year.Add(valuerowTH[2].Text);
                            year.Add(valuerowTH[3].Text);
                        }
                        if (valuerowTD.Count != 0)
                        {
                            if (I == 0)
                            {

                                MarketValue.Add(valuerowTD[1].Text.Trim());
                                MarketValue.Add(valuerowTD[2].Text.Trim());
                                MarketValue.Add(valuerowTD[3].Text.Trim());
                            }
                            else if (I == 1)
                            {
                                AgriculturalLandValue.Add(valuerowTD[1].Text);
                                AgriculturalLandValue.Add(valuerowTD[2].Text);
                                AgriculturalLandValue.Add(valuerowTD[3].Text);
                            }
                            else if (I == 2)
                            {
                                asvnonschool.Add(valuerowTD[1].Text);
                                asvnonschool.Add(valuerowTD[2].Text);
                                asvnonschool.Add(valuerowTD[3].Text);
                            }
                            else if (I == 3)
                            {
                                asvschool.Add(valuerowTD[1].Text);
                                asvschool.Add(valuerowTD[2].Text);
                                asvschool.Add(valuerowTD[3].Text);
                            }
                            else if (I == 4)
                            {
                                homeexem.Add(valuerowTD[1].Text);
                                homeexem.Add(valuerowTD[2].Text);
                                homeexem.Add(valuerowTD[3].Text);
                            }
                            else if (I == 5)
                            {
                                addhome.Add(valuerowTD[1].Text);
                                addhome.Add(valuerowTD[2].Text);
                                addhome.Add(valuerowTD[3].Text);
                            }
                            else if (I == 6)
                            {
                                otherexem.Add(valuerowTD[1].Text);
                                otherexem.Add(valuerowTD[2].Text);
                                otherexem.Add(valuerowTD[3].Text);
                            }
                            else if (I == 7)
                            {
                                taxvaluenonschool.Add(valuerowTD[1].Text);
                                taxvaluenonschool.Add(valuerowTD[2].Text);
                                taxvaluenonschool.Add(valuerowTD[3].Text);
                            }
                            else if (I == 8)
                            {
                                taxvalueschool.Add(valuerowTD[1].Text);
                                taxvalueschool.Add(valuerowTD[2].Text);
                                taxvalueschool.Add(valuerowTD[3].Text);
                            }
                            I++;
                        }

                    }
                    string assessment1 = year[0] + "~" + MarketValue[0] + "~" + AgriculturalLandValue[0] + "~" + asvnonschool[0] + "~" + asvschool[0] + "~" + homeexem[0] + "~" + addhome[0] + "~" + otherexem[0] + "~" + taxvaluenonschool[0] + "~" + taxvalueschool[0];
                    string assessment2 = year[1] + "~" + MarketValue[1] + "~" + AgriculturalLandValue[1] + "~" + asvnonschool[1] + "~" + asvschool[1] + "~" + homeexem[1] + "~" + addhome[1] + "~" + otherexem[1] + "~" + taxvaluenonschool[1] + "~" + taxvalueschool[1];
                    string assessment3 = year[2] + "~" + MarketValue[2] + "~" + AgriculturalLandValue[2] + "~" + asvnonschool[2] + "~" + asvschool[2] + "~" + homeexem[2] + "~" + addhome[2] + "~" + otherexem[2] + "~" + taxvaluenonschool[2] + "~" + taxvalueschool[2];

                    gc.insert_date(orderNumber, parcelid, 1508, assessment1, 1, DateTime.Now);
                    gc.insert_date(orderNumber, parcelid, 1508, assessment2, 1, DateTime.Now);
                    gc.insert_date(orderNumber, parcelid, 1508, assessment3, 1, DateTime.Now);

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    //Tax Authority
                    string Taxauthority = "", Taxauthority1 = "";
                    try
                    {
                        driver.Navigate().GoToUrl("https://www.brevardtaxcollector.com/contact/");
                        Taxauthority = driver.FindElement(By.XPath("//*[@id='cs-content']/div[2]/div/div/div[2]/p[3]")).Text.Trim();
                        gc.CreatePdf(orderNumber, accno, "Tax Authority", driver, "FL", "Brevard");
                    }
                    catch { }
                    string strPropertyDetails = accno + "~" + owner + "~" + mailaddr + "~" + siteaddr + "~" + taxingdist + "~" + exemp + "~" + prop_use + "~" + subdivname + "~" + landdesc + "~" + yearbuilt + "~" + Taxauthority;
                    gc.insert_date(orderNumber, parcelid, 1507, strPropertyDetails, 1, DateTime.Now);

                    //Tax Details 
                    // accno = "2531000";
                    driver.Navigate().GoToUrl("https://brevard.county-taxes.com/public");
                    driver.FindElement(By.Name("search_query")).SendKeys(accno);
                    // gc.CreatePdf(orderNumber, parcelid, "Tax Search", driver, "FL", "Brevard");
                    IWebElement element = driver.FindElement(By.XPath("//*[@id='search-controls']/div/form[1]/div[1]/div/span/button"));
                    IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                    js.ExecuteScript("arguments[0].click();", element);
                    Thread.Sleep(4000);
                    gc.CreatePdf(orderNumber, accno, "Tax Search Result", driver, "FL", "Brevard");
                    IWebElement ITaxSearch = driver.FindElement(By.LinkText("Full bill history"));
                    string strITaxSearch = ITaxSearch.GetAttribute("href");
                    driver.Navigate().GoToUrl(strITaxSearch);
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, accno, "Full Bill History", driver, "FL", "Brevard");
                    IWebElement IBillHistorytable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table"));
                    IList<IWebElement> IBillHistoryRow = IBillHistorytable.FindElements(By.TagName("tr"));
                    IList<IWebElement> IBillHistoryTD;
                    IList<IWebElement> IBillHistoryTH;
                    int i = 0; int m = 0; int j = 0; int k = 0;
                    foreach (IWebElement bill in IBillHistoryRow)
                    {
                        string billyear = "", inst = "", paidamount = "", receipt = "", effectivedate = "";
                        IBillHistoryTD = bill.FindElements(By.TagName("td"));
                        IBillHistoryTH = bill.FindElements(By.TagName("th"));
                        if (IBillHistoryTD.Count != 0)
                        {
                            try
                            {
                                if (IBillHistoryTD[0].Text.Contains("Redeemed certificate") || IBillHistoryTD[0].Text.Contains("Issued certificate") || IBillHistoryTD[0].Text.Contains("Tax Deed Application") || IBillHistoryTD[0].Text.Contains("Expired certificate"))
                                {
                                    if (IBillHistoryTD.Count == 5)
                                    {
                                        //billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                        billyear = IBillHistoryTD[0].Text;
                                        strBalance = IBillHistoryTD[1].Text.Replace("\r\n", " ");
                                        strBillDate = IBillHistoryTD[2].Text;
                                        paidamount = IBillHistoryTD[3].Text;
                                        string strTaxHistory2 = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                        gc.insert_date(orderNumber, accno, 1509, strTaxHistory2, 1, DateTime.Now);
                                    }
                                }

                                if (IBillHistoryTD.Count == 2)
                                {
                                    strBillDate = IBillHistoryTD[0].Text;
                                    paidamount = IBillHistoryTD[1].Text;
                                    if (strBillDate.Contains("Effective"))
                                    {
                                        strBillDate = strBillDate.Replace("Effective ", " ").Trim();
                                        var splitdate = strBillDate.Split(' ');
                                        strBillDate = splitdate[0].Trim();
                                        effectivedate = splitdate[1];
                                    }
                                    if (paidamount.Contains("Paid"))
                                    {
                                        strBillPaid = IBillHistoryTD[1].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                        paidamount = gc.Between(strBillPaid, "Paid ", "Receipt ");
                                        receipt = GlobalClass.After(strBillPaid, "Receipt #");
                                    }
                                    string strTaxHistory2 = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                    gc.insert_date(orderNumber, accno, 1509, strTaxHistory2, 1, DateTime.Now);
                                }
                                if (IBillHistoryTD[2].Text.Contains("Paid"))
                                {
                                    inst = "Total"; paidamount = IBillHistoryTD[2].Text.Replace("Paid", ""); strBalance = "";
                                    string strTaxHistory = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                    gc.insert_date(orderNumber, accno, 1509, strTaxHistory, 1, DateTime.Now);
                                }

                                if (IBillHistoryTD[0].Text.Contains("Issued certificate"))
                                {
                                    IWebElement ITaxBillCount = IBillHistoryTD[0].FindElement(By.TagName("a"));
                                    string issuelink = ITaxBillCount.GetAttribute("href");
                                    strissuecertificate.Add(issuelink);
                                }


                                if (bill.Text.Contains("Annual Bill") || bill.Text.Contains("Pay this bill") || bill.Text.Contains("Installment Bill"))
                                {
                                    if (m < 12)
                                    {
                                        IWebElement ITaxBillCount = IBillHistoryTD[0].FindElement(By.TagName("a"));
                                        strbillyear = GlobalClass.Before(ITaxBillCount.Text, " Annual Bill");
                                        string taxlink = ITaxBillCount.GetAttribute("href");

                                        if (bill.Text.Contains("Annual Bill"))
                                        {
                                            if (j < 3)
                                            {
                                                //download
                                                try
                                                {
                                                    IWebElement ITaxBill = IBillHistoryTD[3].FindElement(By.TagName("a"));
                                                    string BillTax = ITaxBill.GetAttribute("href");
                                                    downloadlink.Add(BillTax);
                                                    string dbill = gc.Between(BillTax, "parcels/", "/bills");
                                                    strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();

                                                    BillDownload(orderNumber, accno, BillTax, dbill, strbillyear);
                                                }
                                                catch { }
                                                try
                                                {
                                                    IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                                    string BillTax = ITaxBill.GetAttribute("href");
                                                    downloadlink.Add(BillTax);
                                                    string dbill = gc.Between(BillTax, "parcels/", "/bills");
                                                    strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                                    string fname = "Tax Bill" + " " + accno + " " + strBill;
                                                    // gc.downloadfile(BillTax, orderNumber, accno, fname, "FL", "Brevard");

                                                    BillDownload(orderNumber, accno, BillTax, dbill, strbillyear);
                                                }
                                                catch (Exception ex)
                                                {
                                                }
                                                taxhistorylink.Add(taxlink);

                                            }

                                            if (taxhistorylinkinst.Count != 0 && taxhistorylinkinst.Count < 12 && j < 8 && taxhistorylink.Count < 2)
                                            {
                                                //download 
                                                try
                                                {
                                                    IWebElement ITaxBill = IBillHistoryTD[3].FindElement(By.TagName("a"));
                                                    string BillTax = ITaxBill.GetAttribute("href");
                                                    downloadlink.Add(BillTax);

                                                    string dbill = gc.Between(BillTax, "parcels/", "/bills");
                                                    strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();

                                                    BillDownload(orderNumber, accno, BillTax, dbill, strbillyear);

                                                }
                                                catch { }
                                                try
                                                {
                                                    IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                                    string BillTax = ITaxBill.GetAttribute("href");
                                                    downloadlink.Add(BillTax);
                                                    //string dbill = gc.Between(BillTax, "bills/", "/print");
                                                    string dbill = gc.Between(BillTax, "parcels/", "/bills");
                                                    strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();

                                                    BillDownload(orderNumber, accno, BillTax, dbill, strbillyear);

                                                }
                                                catch (Exception ex)
                                                {
                                                }
                                                //taxhistorylink.Add(taxlink);
                                            }
                                            j++;
                                            k++;
                                            strbillyear = "";
                                        }
                                        else if (bill.Text.Contains("Installment Bill"))
                                        {
                                            if (taxhistorylink.Count == 3)
                                            {
                                                //
                                            }
                                            else if (taxhistorylink.Count == 2)
                                            {
                                                if (j < 4)
                                                {
                                                    //download
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[3].FindElement(By.TagName("a"));
                                                        strbillyear = GlobalClass.Before(ITaxBillCount.Text, " Installment Bill");
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        string dbill = gc.Between(BillTax, "parcels/", "/bills");
                                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();

                                                        BillDownload(orderNumber, accno, BillTax, dbill, strbillyear);


                                                    }
                                                    catch { }
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                                        strbillyear = GlobalClass.Before(ITaxBillCount.Text, " Installment Bill");
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        downloadlink.Add(BillTax);
                                                        string dbill = gc.Between(BillTax, "parcels/", "/bills");
                                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();

                                                        BillDownload(orderNumber, accno, BillTax, dbill, strbillyear);
                                                    }
                                                    catch
                                                    {
                                                    }
                                                    IWebElement ITaxBillCount1 = IBillHistoryTD[0].FindElement(By.TagName("a"));
                                                    string taxlink1 = ITaxBillCount1.GetAttribute("href");
                                                    taxhistorylinkinst.Add(taxlink1);
                                                    j++;
                                                }
                                            }
                                            else if (taxhistorylink.Count == 1)
                                            {
                                                if (j < 7)
                                                {
                                                    //download
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[3].FindElement(By.TagName("a"));
                                                        strbillyear = GlobalClass.Before(ITaxBillCount.Text, " Installment Bill");
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        downloadlink.Add(BillTax);
                                                        string dbill = gc.Between(BillTax, "parcels/", "/bills");
                                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();

                                                        BillDownload(orderNumber, accno, BillTax, dbill, strbillyear);


                                                    }
                                                    catch { }
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                                        strbillyear = GlobalClass.Before(ITaxBillCount.Text, " Installment Bill");
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        downloadlink.Add(BillTax);
                                                        string dbill = gc.Between(BillTax, "parcels/", "/bills");
                                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();

                                                        BillDownload(orderNumber, accno, BillTax, dbill, strbillyear);


                                                    }
                                                    catch
                                                    {
                                                    }


                                                    IWebElement ITaxBillCount1 = IBillHistoryTD[0].FindElement(By.TagName("a"));
                                                    string taxlink1 = ITaxBillCount1.GetAttribute("href");
                                                    taxhistorylinkinst.Add(taxlink1);
                                                    j++;
                                                }
                                            }
                                            else if (taxhistorylink.Count == 0)
                                            {
                                                if (j < 12)
                                                {
                                                    //download
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[3].FindElement(By.TagName("a"));
                                                        strbillyear = GlobalClass.Before(ITaxBillCount.Text, " Installment Bill");
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        downloadlink.Add(BillTax);
                                                        string dbill = gc.Between(BillTax, "parcels/", "/bills");
                                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();

                                                        BillDownload(orderNumber, accno, BillTax, dbill, strbillyear);

                                                    }
                                                    catch { }
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                                        strbillyear = GlobalClass.Before(ITaxBillCount.Text, " Installment Bill");
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        downloadlink.Add(BillTax);
                                                        string dbill = gc.Between(BillTax, "parcels/", "/bills");
                                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();

                                                        BillDownload(orderNumber, accno, BillTax, dbill, strbillyear);

                                                    }
                                                    catch
                                                    {
                                                    }

                                                    IWebElement ITaxBillCount1 = IBillHistoryTD[0].FindElement(By.TagName("a"));
                                                    string taxlink1 = ITaxBillCount1.GetAttribute("href");
                                                    taxhistorylinkinst.Add(taxlink1);
                                                    j++;
                                                }
                                            }
                                            strbillyear = "";
                                        }
                                        m++;
                                    }
                                    if (bill.Text.Contains("Pay this bill"))
                                    {
                                        //download
                                        try
                                        {
                                            IWebElement ITaxBill = IBillHistoryTD[3].FindElement(By.TagName("a"));
                                            try
                                            {
                                                strbillyear = GlobalClass.Before(ITaxBill.Text, " Annual Bill");
                                            }
                                            catch { }
                                            try
                                            {
                                                strbillyear = GlobalClass.Before(ITaxBill.Text, " Installment Bill");
                                            }
                                            catch { }
                                            string BillTax = ITaxBill.GetAttribute("href");
                                            strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                            downloadlink.Add(BillTax);
                                            string dbill = gc.Between(BillTax, "parcels/", "/bills");
                                            strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();

                                            BillDownload(orderNumber, accno, BillTax, dbill, strbillyear);

                                        }
                                        catch { }
                                        try
                                        {
                                            IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                            string BillTax = ITaxBill.GetAttribute("href");
                                            downloadlink.Add(BillTax);
                                            string dbill = gc.Between(BillTax, "parcels/", "/bills");
                                            strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();

                                            BillDownload(orderNumber, accno, BillTax, dbill, strbillyear);
                                        }
                                        catch
                                        {
                                        }


                                        IWebElement ITaxBillCount = IBillHistoryTD[0].FindElement(By.TagName("a"));
                                        string taxlink = ITaxBillCount.GetAttribute("href");
                                        //taxhistorylink.Add(taxlink);
                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd().Replace("Installment Bill #", "").Replace("Bill", "");
                                        var Splitbill = strBill.Split(' ');
                                        billyear = Splitbill[0]; inst = Splitbill[1];
                                        strBalance = IBillHistoryTD[1].Text.Replace("\r\n", "").TrimStart().TrimEnd();
                                        string strTaxHistory = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                        gc.insert_date(orderNumber, accno, 1509, strTaxHistory, 1, DateTime.Now);

                                        strbillyear = "";
                                    }
                                    else
                                    {

                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd().Replace("Installment Bill #", "").Replace("Bill", "");
                                        var Splitbill1 = strBill.Split(' ');
                                        billyear = Splitbill1[0]; inst = Splitbill1[1];
                                        strBalance = IBillHistoryTD[1].Text.Replace("\r\n", "").TrimStart().TrimEnd();
                                        strBillDate = IBillHistoryTD[2].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                        if (strBillDate.Contains("Effective"))
                                        {
                                            strBillDate = strBillDate.Replace("Effective ", "");
                                            var splitdate = strBillDate.Split(' ');
                                            strBillDate = splitdate[0];
                                            effectivedate = splitdate[1];
                                        }
                                        strBillPaid = IBillHistoryTD[3].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                        paidamount = gc.Between(strBillPaid, "Paid ", "Receipt ");
                                        receipt = GlobalClass.After(strBillPaid, "Receipt #");
                                        string strTaxHistory = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                        gc.insert_date(orderNumber, accno, 1509, strTaxHistory, 1, DateTime.Now);

                                    }


                                    // m++;
                                    strbillyear = "";
                                }

                                else
                                {
                                    strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd().Replace("Installment Bill #", "").Replace("Bill", "");
                                    var Splitbill = strBill.Split(' ');
                                    billyear = Splitbill[0]; inst = Splitbill[1];
                                    strBalance = IBillHistoryTD[1].Text.Replace("\r\n", "").TrimStart().TrimEnd();
                                    strBillDate = IBillHistoryTD[2].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                    if (strBillDate.Contains("Effective"))
                                    {
                                        strBillDate = strBillDate.Replace("Effective ", "");
                                        var splitdate = strBillDate.Split(' ');
                                        strBillDate = splitdate[0];
                                        effectivedate = splitdate[1];
                                    }
                                    strBillPaid = IBillHistoryTD[3].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                    paidamount = gc.Between(strBillPaid, "Paid ", "Receipt ");
                                    receipt = GlobalClass.After(strBillPaid, "Receipt #");
                                    //Tax Year~Installment~Total Due~Paid Date~Effective Date~Paid Amount~Receipt Number
                                    string strTaxHistory = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                    gc.insert_date(orderNumber, accno, 1509, strTaxHistory, 1, DateTime.Now);
                                }

                            }
                            catch
                            {

                            }


                        }
                        if (IBillHistoryTH.Count != 0)
                        {
                            if (IBillHistoryTH[0].Text.Contains("Total Balance"))
                            {
                                inst = "Total Balance"; strBalance = IBillHistoryTH[1].Text.Replace("\r\n", "").TrimStart().TrimEnd();
                                string strTaxHistory = billyear + "~" + inst + "~" + strBalance + "~" + "" + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                gc.insert_date(orderNumber, accno, 1509, strTaxHistory, 1, DateTime.Now);
                            }
                        }

                        i++;
                    }

                    int q = 0;
                    foreach (string URL in taxhistorylink)
                    {

                        try
                        {
                            Account = ""; milagerate = ""; alterkey = ""; taxowner = ""; tax_addr = ""; millagecode = ""; ctax_year = ""; cctaxyear = ""; combinedtaxamount = ""; grosstax = ""; cpaidamount = ""; cpaiddate = ""; ceffdate = ""; creceipt = "";
                            driver.Navigate().GoToUrl(URL);
                            Thread.Sleep(3000);
                            //*[@id="content"]/div[1]/div[8]/div/div[1]                        
                            cctaxyear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[1]/div[2]")).Text.Trim().Replace("Real Estate", "").Replace("Print this bill (PDF)", "").Replace("\r\n", "");
                            gc.CreatePdf(orderNumber, accno, "Tax Details" + cctaxyear, driver, "FL", "Brevard");

                            if (q == 0)
                            {

                                IWebElement valoremtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[2]"));
                                IList<IWebElement> valoremtableRow = valoremtable.FindElements(By.TagName("tr"));
                                int valoremtablerowcount = valoremtableRow.Count;
                                IList<IWebElement> valoremtablerowTD;
                                int d = 0;
                                foreach (IWebElement rowid in valoremtableRow)
                                {
                                    valoremtablerowTD = rowid.FindElements(By.TagName("td"));
                                    if (valoremtablerowTD.Count != 0 && !rowid.Text.Contains("Taxing authority"))
                                    {
                                        string valoremtax = "";
                                        if (d < valoremtablerowcount - 1)
                                        {
                                            if (valoremtablerowTD.Count == 6)
                                            {
                                                //Taxing Authority~Tax Type~Millage~Assessed~Exemption~Taxable~Taxes
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[1].Text + "~" + valoremtablerowTD[2].Text + "~" + valoremtablerowTD[3].Text + "~" + valoremtablerowTD[4].Text + "~" + valoremtablerowTD[5].Text;
                                                gc.insert_date(orderNumber, accno, 1510, valoremtax, 1, DateTime.Now);
                                            }

                                            else if (valoremtablerowTD.Count == 2)
                                            {
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                                gc.insert_date(orderNumber, accno, 1510, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        else if (d == valoremtablerowcount - 1)
                                        {
                                            milagerate = valoremtablerowTD[0].Text;
                                            valoremtax = "Total" + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + valoremtablerowTD[1].Text;
                                            gc.insert_date(orderNumber, accno, 1510, valoremtax, 1, DateTime.Now);
                                        }
                                    }
                                    d++;
                                }

                                //Non Ad-Valorems
                                try
                                {
                                    //*[@id="content"]/div[1]/div[8]/div/table[3]
                                    IWebElement nonvaloremtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[3]"));
                                    IList<IWebElement> nonvaloremtableRow = nonvaloremtable.FindElements(By.TagName("tr"));
                                    int nonvaloremtablerowcount = nonvaloremtableRow.Count;
                                    IList<IWebElement> nonvaloremtablerowTD;
                                    int e = 0;
                                    foreach (IWebElement rowid in nonvaloremtableRow)
                                    {
                                        nonvaloremtablerowTD = rowid.FindElements(By.TagName("td"));
                                        if (nonvaloremtablerowTD.Count != 0 && !rowid.Text.Contains("Levying authority"))
                                        {
                                            string valoremtax = "";
                                            if (e < nonvaloremtablerowcount - 1)
                                            {
                                                //Taxing Authority~Tax Type~Millage~Assessed~Exemption~Taxable~Taxes
                                                valoremtax = nonvaloremtablerowTD[0].Text + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[1].Text + "~" + nonvaloremtablerowTD[2].Text;
                                                gc.insert_date(orderNumber, accno, 1510, valoremtax, 1, DateTime.Now);
                                            }
                                            else if (e == nonvaloremtablerowcount - 1)
                                            {
                                                valoremtax = "Total" + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[0].Text;
                                                gc.insert_date(orderNumber, accno, 1510, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        e++;
                                    }
                                }
                                catch { }
                            }

                            string currenttaxbulktext = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/p")).Text;
                            combinedtaxamount = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/p")).Text.Trim().Replace("Combined taxes and assessments: ", "");
                            try
                            {
                                string bulkgrosstax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]/tbody/tr/td[1]")).Text.Trim();
                                if (bulkgrosstax.Contains("Gross"))
                                {

                                    grosstax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]/tbody/tr/td[1]")).Text.Trim().Replace("Gross", "");
                                    //if paid by                 
                                    string ifpaidbulk = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]/tbody/tr/td[3]")).Text;
                                    string[] stringSeparators1 = new string[] { "\r\n" };
                                    string[] lines1 = ifpaidbulk.Split(stringSeparators1, StringSplitOptions.None);
                                    ifpaidby = lines1[0]; pleasepay = lines1[1];
                                    string single = cctaxyear + "~" + ifpaidby + "~" + pleasepay;
                                    gc.insert_date(orderNumber, accno, 1511, single, 1, DateTime.Now);
                                }

                                else
                                {
                                    string IfPaidBy = "", PlesePay = "", DueDate = "", itaxyear = "";
                                    try
                                    {
                                        IWebElement multitableElement26 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]"));
                                        IList<IWebElement> multitableRow26 = multitableElement26.FindElements(By.TagName("tr"));
                                        IList<IWebElement> multirowTD26;
                                        foreach (IWebElement row in multitableRow26)
                                        {
                                            multirowTD26 = row.FindElements(By.TagName("td"));
                                            int iRowsCount = multirowTD26.Count;
                                            for (int n = 0; n < iRowsCount; n++)
                                            {
                                                if ((!multirowTD26[n].Text.Contains("Face Amt")) && (!multirowTD26[n].Text.Contains("Certificate")))
                                                {
                                                    IfPaidBy = multirowTD26[n].Text.Replace("\r\n", "~");
                                                    var IfpaySplit = IfPaidBy.Split('~');
                                                    IfPaidBy = IfpaySplit[0];
                                                    PlesePay = IfpaySplit[1];
                                                    DueDate = cctaxyear + "~" + IfPaidBy + "~" + PlesePay;
                                                    gc.insert_date(orderNumber, accno, 1511, DueDate, 1, DateTime.Now);

                                                }
                                            }
                                        }

                                    }
                                    catch { }
                                }
                            }
                            catch { }
                            try
                            {

                                string bulktext1 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[5]")).Text.Replace("\r\n", ",");
                                taxowner = gc.Between(bulktext1, "Owner,", "Situs address").Replace(",,", "");
                                tax_addr = gc.Between(bulktext1, "Situs address", "Legal description").Trim().Replace(",", "");
                                //*[@id='content']/div[1]/div[7]/div/table[1]/tbody
                                IWebElement currrtaxtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[1]/tbody"));
                                IList<IWebElement> currrtaxtableRow = currrtaxtable.FindElements(By.TagName("tr"));
                                int currrtaxtabletableRowcount = currrtaxtableRow.Count;
                                IList<IWebElement> currrtaxtableRowTD;
                                int e = 0;
                                foreach (IWebElement rowid in currrtaxtableRow)
                                {
                                    currrtaxtableRowTD = rowid.FindElements(By.TagName("td"));
                                    if (currrtaxtableRowTD.Count != 0 && !rowid.Text.Contains("Account number"))
                                    {
                                        Account = currrtaxtableRowTD[0].Text; alterkey = currrtaxtableRowTD[1].Text; millagecode = currrtaxtableRowTD[3].Text;
                                    }
                                }

                            }
                            catch { }
                            try
                            {

                                string paidbulk = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[3]/div[1]/div")).Text.Trim();
                                cpaiddate = gc.Between(paidbulk, "PAID ", " $").Trim();
                                cpaidamount = gc.Between(paidbulk, "$", "Receipt ").Trim();
                                cpaidamount = "$" + cpaidamount;
                                if (cpaidamount.Contains("Effective"))
                                {
                                    ceffdate = GlobalClass.After(cpaidamount, "Effective").Trim();
                                    cpaidamount = GlobalClass.Before(cpaidamount, "Effective").Trim();
                                }
                                creceipt = GlobalClass.After(paidbulk, "Receipt #");
                            }
                            catch { }

                            ////Parcel Details
                            IWebElement ITaxSearchparcell = driver.FindElement(By.LinkText("Parcel details"));
                            string strITaxSearchparcell = ITaxSearchparcell.GetAttribute("href");
                            driver.Navigate().GoToUrl(strITaxSearchparcell);
                            Thread.Sleep(3000);
                            gc.CreatePdf(orderNumber, accno, "Parcel Details" + cctaxyear, driver, "FL", "Brevard");
                            //Account Number~Alternate Key~Owner Name & Mailing Address~Property Address~Millage Code~Millage rate~Tax Year~Tax Amount~Gross Tax~Paid Amount~Paid Date~Effective Date~Receipt Number
                            if (milagerate.Trim() == "")
                            {
                                milagerate = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[3]/div[2]/dl[2]/dd[4]")).Text;
                            }
                            string curtax = alterkey + "~" + taxowner + "~" + tax_addr + "~" + millagecode + "~" + milagerate + "~" + cctaxyear + "~" + combinedtaxamount + "~" + grosstax + "~" + cpaidamount + "~" + cpaiddate + "~" + ceffdate + "~" + creceipt + "~" + Tax_Authority;
                            gc.insert_date(orderNumber, accno, 1512, curtax, 1, DateTime.Now);
                            q++;

                        }

                        catch
                        {

                        }

                        try
                        {
                            Account = ""; alterkey = ""; taxowner = ""; tax_addr = ""; millagecode = ""; ctax_year = ""; cctaxyear = ""; combinedtaxamount = ""; grosstax = ""; cpaidamount = ""; cpaiddate = ""; ceffdate = ""; creceipt = "";
                            driver.Navigate().GoToUrl(URL);
                            Thread.Sleep(3000);

                            cctaxyear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[1]/div[2]")).Text.Trim().Replace("Real Estate", "").Replace("Print this bill (PDF)", "").Replace("\r\n", "");
                            gc.CreatePdf(orderNumber, accno, "Tax Details" + cctaxyear, driver, "FL", "Brevard");

                            if (q == 0)
                            {

                                IWebElement valoremtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[2]"));
                                IList<IWebElement> valoremtableRow = valoremtable.FindElements(By.TagName("tr"));
                                int valoremtablerowcount = valoremtableRow.Count;
                                IList<IWebElement> valoremtablerowTD;
                                int d = 0;
                                foreach (IWebElement rowid in valoremtableRow)
                                {
                                    valoremtablerowTD = rowid.FindElements(By.TagName("td"));
                                    if (valoremtablerowTD.Count != 0 && !rowid.Text.Contains("Taxing authority"))
                                    {
                                        string valoremtax = "";
                                        if (d < valoremtablerowcount - 1)
                                        {
                                            if (valoremtablerowTD.Count == 6)
                                            {
                                                //Taxing Authority~Tax Type~Millage~Assessed~Exemption~Taxable~Taxes
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[1].Text + "~" + valoremtablerowTD[2].Text + "~" + valoremtablerowTD[3].Text + "~" + valoremtablerowTD[4].Text + "~" + valoremtablerowTD[5].Text;
                                                gc.insert_date(orderNumber, accno, 1510, valoremtax, 1, DateTime.Now);
                                            }

                                            else if (valoremtablerowTD.Count == 2)
                                            {
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                                gc.insert_date(orderNumber, accno, 1510, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        else if (d == valoremtablerowcount - 1)
                                        {
                                            milagerate = valoremtablerowTD[0].Text;
                                            valoremtax = "Total" + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + valoremtablerowTD[2].Text;
                                            gc.insert_date(orderNumber, accno, 1510, valoremtax, 1, DateTime.Now);
                                        }
                                    }
                                    d++;
                                }

                                //Non Ad-Valorems
                                try
                                {
                                    //*[@id="content"]/div[1]/div[8]/div/table[3]
                                    IWebElement nonvaloremtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[3]"));
                                    IList<IWebElement> nonvaloremtableRow = nonvaloremtable.FindElements(By.TagName("tr"));
                                    int nonvaloremtablerowcount = nonvaloremtableRow.Count;
                                    IList<IWebElement> nonvaloremtablerowTD;
                                    int e = 0;
                                    foreach (IWebElement rowid in nonvaloremtableRow)
                                    {
                                        nonvaloremtablerowTD = rowid.FindElements(By.TagName("td"));
                                        if (nonvaloremtablerowTD.Count != 0 && !rowid.Text.Contains("Levying authority"))
                                        {
                                            string valoremtax = "";
                                            if (e < nonvaloremtablerowcount - 1)
                                            {
                                                //Taxing Authority~Tax Type~Millage~Assessed~Exemption~Taxable~Taxes
                                                valoremtax = nonvaloremtablerowTD[0].Text + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[1].Text + "~" + nonvaloremtablerowTD[2].Text;
                                                gc.insert_date(orderNumber, accno, 1510, valoremtax, 1, DateTime.Now);
                                            }
                                            else if (e == nonvaloremtablerowcount - 1)
                                            {
                                                valoremtax = "Total" + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[0].Text;
                                                gc.insert_date(orderNumber, accno, 1510, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        e++;
                                    }
                                }
                                catch { }
                            }

                            string currenttaxbulktext = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/p")).Text;
                            combinedtaxamount = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/p")).Text.Trim().Replace("Combined taxes and assessments: ", "");
                            try
                            {
                                string bulkgrosstax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]/tbody/tr/td[1]")).Text.Trim();
                                if (bulkgrosstax.Contains("Gross"))
                                {

                                    grosstax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]/tbody/tr/td[1]")).Text.Trim().Replace("Gross", "");
                                    //if paid by                 
                                    string ifpaidbulk = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]/tbody/tr/td[3]")).Text;
                                    string[] stringSeparators1 = new string[] { "\r\n" };
                                    string[] lines1 = ifpaidbulk.Split(stringSeparators1, StringSplitOptions.None);
                                    ifpaidby = lines1[0]; pleasepay = lines1[1];
                                    string single = cctaxyear + "~" + ifpaidby + "~" + pleasepay;
                                    gc.insert_date(orderNumber, accno, 1511, single, 1, DateTime.Now);
                                }

                                else
                                {
                                    string IfPaidBy = "", PlesePay = "", DueDate = "", itaxyear = "";
                                    try
                                    {
                                        IWebElement multitableElement26 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]"));
                                        IList<IWebElement> multitableRow26 = multitableElement26.FindElements(By.TagName("tr"));
                                        IList<IWebElement> multirowTD26;
                                        foreach (IWebElement row in multitableRow26)
                                        {
                                            multirowTD26 = row.FindElements(By.TagName("td"));
                                            int iRowsCount = multirowTD26.Count;
                                            for (int n = 0; n < iRowsCount; n++)
                                            {
                                                if ((!multirowTD26[n].Text.Contains("Face Amt")) && (!multirowTD26[n].Text.Contains("Certificate")))
                                                {
                                                    IfPaidBy = multirowTD26[n].Text.Replace("\r\n", "~");
                                                    var IfpaySplit = IfPaidBy.Split('~');
                                                    IfPaidBy = IfpaySplit[0];
                                                    PlesePay = IfpaySplit[1];
                                                    DueDate = cctaxyear + "~" + IfPaidBy + "~" + PlesePay;
                                                    gc.insert_date(orderNumber, accno, 1511, DueDate, 1, DateTime.Now);

                                                }
                                            }
                                        }

                                    }
                                    catch { }
                                }
                            }
                            catch { }
                            try
                            {

                                string bulktext1 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[5]")).Text.Replace("\r\n", ",");
                                taxowner = gc.Between(bulktext1, "Owner,", "Situs address").Replace(".", "");
                                tax_addr = gc.Between(bulktext1, "Situs address", "Legal description").Trim().Replace(",", "");
                                //*[@id='content']/div[1]/div[7]/div/table[1]/tbody
                                IWebElement currrtaxtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[1]/tbody"));
                                IList<IWebElement> currrtaxtableRow = currrtaxtable.FindElements(By.TagName("tr"));
                                int currrtaxtabletableRowcount = currrtaxtableRow.Count;
                                IList<IWebElement> currrtaxtableRowTD;
                                int e = 0;
                                foreach (IWebElement rowid in currrtaxtableRow)
                                {
                                    currrtaxtableRowTD = rowid.FindElements(By.TagName("td"));
                                    if (currrtaxtableRowTD.Count != 0 && !rowid.Text.Contains("Account number"))
                                    {
                                        Account = currrtaxtableRowTD[0].Text; alterkey = currrtaxtableRowTD[1].Text; millagecode = currrtaxtableRowTD[3].Text;
                                    }
                                }

                            }
                            catch { }
                            try
                            {

                                string paidbulk = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[3]/div[1]/div")).Text.Trim();
                                cpaiddate = gc.Between(paidbulk, "PAID ", " $").Trim();
                                cpaidamount = gc.Between(paidbulk, "$", "Receipt ").Trim();
                                cpaidamount = "$" + cpaidamount;
                                creceipt = GlobalClass.After(paidbulk, "Receipt #");
                            }
                            catch { }

                            ////Parcel Details
                            IWebElement ITaxSearchparcell = driver.FindElement(By.LinkText("Parcel details"));
                            string strITaxSearchparcell = ITaxSearchparcell.GetAttribute("href");
                            driver.Navigate().GoToUrl(strITaxSearchparcell);
                            Thread.Sleep(3000);
                            gc.CreatePdf(orderNumber, accno, "Parcel Details" + cctaxyear, driver, "FL", "Brevard");


                            //Account Number~Alternate Key~Owner Name & Mailing Address~Property Address~Millage Code~Millage rate~Tax Year~Tax Amount~Gross Tax~Paid Amount~Paid Date~Effective Date~Receipt Number
                            string curtax = alterkey + "~" + taxowner + "~" + tax_addr + "~" + millagecode + "~" + milagerate + "~" + cctaxyear + "~" + combinedtaxamount + "~" + grosstax + "~" + cpaidamount + "~" + cpaiddate + "~" + ceffdate + "~" + creceipt + "~" + Tax_Authority;
                            gc.insert_date(orderNumber, accno, 1512, curtax, 1, DateTime.Now);
                            q++;

                        }
                        catch
                        { }
                    }


                    //q = 0;
                    foreach (string URL in taxhistorylinkinst)
                    {
                        try
                        {
                            Account = ""; alterkey = ""; taxowner = ""; tax_addr = ""; millagecode = ""; ctax_year = ""; cctaxyear = ""; combinedtaxamount = ""; grosstax = ""; cpaidamount = ""; cpaiddate = ""; ceffdate = ""; creceipt = "";
                            driver.Navigate().GoToUrl(URL);
                            Thread.Sleep(3000);
                            //*[@id="content"]/div[1]/div[8]/div/div[1]                        
                            cctaxyear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[1]/div[2]")).Text.Trim().Replace("Real Estate", "").Replace("Print this bill (PDF)", "").Replace("\r\n", "");
                            gc.CreatePdf(orderNumber, accno, "Tax Details" + cctaxyear, driver, "FL", "Brevard");

                            if (q == 0)
                            {

                                IWebElement valoremtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[2]"));
                                IList<IWebElement> valoremtableRow = valoremtable.FindElements(By.TagName("tr"));
                                int valoremtablerowcount = valoremtableRow.Count;
                                IList<IWebElement> valoremtablerowTD;
                                int d = 0;
                                foreach (IWebElement rowid in valoremtableRow)
                                {
                                    valoremtablerowTD = rowid.FindElements(By.TagName("td"));
                                    if (valoremtablerowTD.Count != 0 && !rowid.Text.Contains("Taxing authority"))
                                    {
                                        string valoremtax = "";
                                        if (d < valoremtablerowcount - 1)
                                        {
                                            if (valoremtablerowTD.Count == 6)
                                            {
                                                //Taxing Authority~Tax Type~Millage~Assessed~Exemption~Taxable~Taxes
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[1].Text + "~" + valoremtablerowTD[2].Text + "~" + valoremtablerowTD[3].Text + "~" + valoremtablerowTD[4].Text + "~" + valoremtablerowTD[5].Text;
                                                gc.insert_date(orderNumber, accno, 1510, valoremtax, 1, DateTime.Now);
                                            }

                                            else if (valoremtablerowTD.Count == 2)
                                            {
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                                gc.insert_date(orderNumber, accno, 1510, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        else if (d == valoremtablerowcount - 1)
                                        {
                                            milagerate = valoremtablerowTD[0].Text;
                                            valoremtax = "Total" + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + valoremtablerowTD[1].Text;
                                            gc.insert_date(orderNumber, accno, 1510, valoremtax, 1, DateTime.Now);
                                        }
                                    }
                                    d++;
                                }

                                //Non Ad-Valorems
                                try
                                {
                                    //*[@id="content"]/div[1]/div[8]/div/table[3]
                                    IWebElement nonvaloremtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[3]"));
                                    IList<IWebElement> nonvaloremtableRow = nonvaloremtable.FindElements(By.TagName("tr"));
                                    int nonvaloremtablerowcount = nonvaloremtableRow.Count;
                                    IList<IWebElement> nonvaloremtablerowTD;
                                    int e = 0;
                                    foreach (IWebElement rowid in nonvaloremtableRow)
                                    {
                                        nonvaloremtablerowTD = rowid.FindElements(By.TagName("td"));
                                        if (nonvaloremtablerowTD.Count != 0 && !rowid.Text.Contains("Levying authority"))
                                        {
                                            string valoremtax = "";
                                            if (e < nonvaloremtablerowcount - 1)
                                            {
                                                //Taxing Authority~Tax Type~Millage~Assessed~Exemption~Taxable~Taxes
                                                valoremtax = nonvaloremtablerowTD[0].Text + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[1].Text + "~" + nonvaloremtablerowTD[2].Text;
                                                gc.insert_date(orderNumber, accno, 1510, valoremtax, 1, DateTime.Now);
                                            }
                                            else if (e == nonvaloremtablerowcount - 1)
                                            {
                                                valoremtax = "Total" + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[0].Text;
                                                gc.insert_date(orderNumber, accno, 1510, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        e++;
                                    }
                                }
                                catch { }
                            }

                            string currenttaxbulktext = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/p")).Text;
                            combinedtaxamount = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/p")).Text.Trim().Replace("Combined taxes and assessments: ", "");
                            try
                            {
                                string bulkgrosstax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]/tbody/tr/td[1]")).Text.Trim();
                                if (bulkgrosstax.Contains("Gross"))
                                {

                                    grosstax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]/tbody/tr/td[1]")).Text.Trim().Replace("Gross", "");
                                    //if paid by                 
                                    string ifpaidbulk = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]/tbody/tr/td[3]")).Text;
                                    string[] stringSeparators1 = new string[] { "\r\n" };
                                    string[] lines1 = ifpaidbulk.Split(stringSeparators1, StringSplitOptions.None);
                                    ifpaidby = lines1[0]; pleasepay = lines1[1];
                                    string single = cctaxyear + "~" + ifpaidby + "~" + pleasepay;
                                    gc.insert_date(orderNumber, accno, 1511, single, 1, DateTime.Now);
                                }

                                else
                                {
                                    string IfPaidBy = "", PlesePay = "", DueDate = "", itaxyear = "";
                                    try
                                    {
                                        IWebElement multitableElement26 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]"));
                                        IList<IWebElement> multitableRow26 = multitableElement26.FindElements(By.TagName("tr"));
                                        IList<IWebElement> multirowTD26;
                                        foreach (IWebElement row in multitableRow26)
                                        {
                                            multirowTD26 = row.FindElements(By.TagName("td"));
                                            int iRowsCount = multirowTD26.Count;
                                            for (int n = 0; n < iRowsCount; n++)
                                            {
                                                if ((!multirowTD26[n].Text.Contains("Face Amt")) && (!multirowTD26[n].Text.Contains("Certificate")))
                                                {
                                                    IfPaidBy = multirowTD26[n].Text.Replace("\r\n", "~");
                                                    var IfpaySplit = IfPaidBy.Split('~');
                                                    IfPaidBy = IfpaySplit[0];
                                                    PlesePay = IfpaySplit[1];
                                                    DueDate = cctaxyear + "~" + IfPaidBy + "~" + PlesePay;
                                                    gc.insert_date(orderNumber, accno, 1511, DueDate, 1, DateTime.Now);

                                                }
                                            }
                                        }

                                    }
                                    catch { }
                                }
                            }
                            catch { }
                            try
                            {

                                string bulktext1 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[5]")).Text.Replace("\r\n", ",");
                                taxowner = gc.Between(bulktext1, "Owner,", "Situs address").Replace(",,", "");
                                tax_addr = gc.Between(bulktext1, "Situs address", "Legal description").Trim().Replace(",", "");
                                //*[@id='content']/div[1]/div[7]/div/table[1]/tbody
                                IWebElement currrtaxtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[1]/tbody"));
                                IList<IWebElement> currrtaxtableRow = currrtaxtable.FindElements(By.TagName("tr"));
                                int currrtaxtabletableRowcount = currrtaxtableRow.Count;
                                IList<IWebElement> currrtaxtableRowTD;
                                int e = 0;
                                foreach (IWebElement rowid in currrtaxtableRow)
                                {
                                    currrtaxtableRowTD = rowid.FindElements(By.TagName("td"));
                                    if (currrtaxtableRowTD.Count != 0 && !rowid.Text.Contains("Account number"))
                                    {
                                        Account = currrtaxtableRowTD[0].Text; alterkey = currrtaxtableRowTD[1].Text; millagecode = currrtaxtableRowTD[3].Text;
                                    }
                                }

                            }
                            catch { }
                            try
                            {

                                string paidbulk = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[3]/div[1]/div")).Text.Trim();
                                cpaiddate = gc.Between(paidbulk, "PAID ", " $").Trim();
                                cpaidamount = gc.Between(paidbulk, "$", "Receipt ").Trim();
                                cpaidamount = "$" + cpaidamount;
                                if (cpaidamount.Contains("Effective"))
                                {
                                    ceffdate = GlobalClass.After(cpaidamount, "Effective").Trim();
                                    cpaidamount = GlobalClass.Before(cpaidamount, "Effective").Trim();
                                }
                                creceipt = GlobalClass.After(paidbulk, "Receipt #");
                            }
                            catch { }

                            //Parcel Details
                            IWebElement ITaxSearchparcell = driver.FindElement(By.LinkText("Parcel details"));
                            string strITaxSearchparcell = ITaxSearchparcell.GetAttribute("href");
                            driver.Navigate().GoToUrl(strITaxSearchparcell);
                            Thread.Sleep(3000);
                            gc.CreatePdf(orderNumber, accno, "Parcel Details" + cctaxyear, driver, "FL", "Brevard");


                            //Account Number~Alternate Key~Owner Name & Mailing Address~Property Address~Millage Code~Millage rate~Tax Year~Tax Amount~Gross Tax~Paid Amount~Paid Date~Effective Date~Receipt Number
                            string curtax = alterkey + "~" + taxowner + "~" + tax_addr + "~" + millagecode + "~" + milagerate + "~" + cctaxyear + "~" + combinedtaxamount + "~" + grosstax + "~" + cpaidamount + "~" + cpaiddate + "~" + ceffdate + "~" + creceipt + "~" + Tax_Authority;
                            gc.insert_date(orderNumber, accno, 1512, curtax, 1, DateTime.Now);
                            q++;
                        }

                        catch
                        {

                        }

                        try
                        {
                            Account = ""; alterkey = ""; taxowner = ""; tax_addr = ""; millagecode = ""; ctax_year = ""; cctaxyear = ""; combinedtaxamount = ""; grosstax = ""; cpaidamount = ""; cpaiddate = ""; ceffdate = ""; creceipt = "";
                            driver.Navigate().GoToUrl(URL);
                            Thread.Sleep(3000);

                            cctaxyear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[1]/div[2]")).Text.Trim().Replace("Real Estate", "").Replace("Print this bill (PDF)", "").Replace("\r\n", "");
                            gc.CreatePdf(orderNumber, accno, "Tax Details" + cctaxyear, driver, "FL", "Brevard");

                            if (q == 0)
                            {

                                IWebElement valoremtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[2]"));
                                IList<IWebElement> valoremtableRow = valoremtable.FindElements(By.TagName("tr"));
                                int valoremtablerowcount = valoremtableRow.Count;
                                IList<IWebElement> valoremtablerowTD;
                                int d = 0;
                                foreach (IWebElement rowid in valoremtableRow)
                                {
                                    valoremtablerowTD = rowid.FindElements(By.TagName("td"));
                                    if (valoremtablerowTD.Count != 0 && !rowid.Text.Contains("Taxing authority"))
                                    {
                                        string valoremtax = "";
                                        if (d < valoremtablerowcount - 1)
                                        {
                                            if (valoremtablerowTD.Count == 6)
                                            {
                                                //Taxing Authority~Tax Type~Millage~Assessed~Exemption~Taxable~Taxes
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[1].Text + "~" + valoremtablerowTD[2].Text + "~" + valoremtablerowTD[3].Text + "~" + valoremtablerowTD[4].Text + "~" + valoremtablerowTD[5].Text;
                                                gc.insert_date(orderNumber, accno, 1510, valoremtax, 1, DateTime.Now);
                                            }

                                            else if (valoremtablerowTD.Count == 2)
                                            {
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                                gc.insert_date(orderNumber, accno, 1510, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        else if (d == valoremtablerowcount - 1)
                                        {
                                            milagerate = valoremtablerowTD[0].Text;
                                            valoremtax = "Total" + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + valoremtablerowTD[1].Text;
                                            gc.insert_date(orderNumber, accno, 1510, valoremtax, 1, DateTime.Now);
                                        }
                                    }
                                    d++;
                                }

                                //Non Ad-Valorems
                                try
                                {
                                    //*[@id="content"]/div[1]/div[8]/div/table[3]
                                    IWebElement nonvaloremtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[3]"));
                                    IList<IWebElement> nonvaloremtableRow = nonvaloremtable.FindElements(By.TagName("tr"));
                                    int nonvaloremtablerowcount = nonvaloremtableRow.Count;
                                    IList<IWebElement> nonvaloremtablerowTD;
                                    int e = 0;
                                    foreach (IWebElement rowid in nonvaloremtableRow)
                                    {
                                        nonvaloremtablerowTD = rowid.FindElements(By.TagName("td"));
                                        if (nonvaloremtablerowTD.Count != 0 && !rowid.Text.Contains("Levying authority"))
                                        {
                                            string valoremtax = "";
                                            if (e < nonvaloremtablerowcount - 1)
                                            {
                                                //Taxing Authority~Tax Type~Millage~Assessed~Exemption~Taxable~Taxes
                                                valoremtax = nonvaloremtablerowTD[0].Text + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[1].Text + "~" + nonvaloremtablerowTD[2].Text;
                                                gc.insert_date(orderNumber, accno, 1510, valoremtax, 1, DateTime.Now);
                                            }
                                            else if (e == nonvaloremtablerowcount - 1)
                                            {
                                                valoremtax = "Total" + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[0].Text;
                                                gc.insert_date(orderNumber, accno, 1510, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        e++;
                                    }
                                }
                                catch { }
                            }

                            string currenttaxbulktext = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/p")).Text;
                            combinedtaxamount = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/p")).Text.Trim().Replace("Combined taxes and assessments: ", "");
                            try
                            {
                                string bulkgrosstax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]/tbody/tr/td[1]")).Text.Trim();
                                if (bulkgrosstax.Contains("Gross"))
                                {

                                    grosstax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]/tbody/tr/td[1]")).Text.Trim().Replace("Gross", "");
                                    //if paid by                 
                                    string ifpaidbulk = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]/tbody/tr/td[3]")).Text;
                                    string[] stringSeparators1 = new string[] { "\r\n" };
                                    string[] lines1 = ifpaidbulk.Split(stringSeparators1, StringSplitOptions.None);
                                    ifpaidby = lines1[0]; pleasepay = lines1[1];
                                    string single = cctaxyear + "~" + ifpaidby + "~" + pleasepay;
                                    gc.insert_date(orderNumber, accno, 1511, single, 1, DateTime.Now);
                                }

                                else
                                {
                                    string IfPaidBy = "", PlesePay = "", DueDate = "", itaxyear = "";
                                    try
                                    {
                                        IWebElement multitableElement26 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]"));
                                        IList<IWebElement> multitableRow26 = multitableElement26.FindElements(By.TagName("tr"));
                                        IList<IWebElement> multirowTD26;
                                        foreach (IWebElement row in multitableRow26)
                                        {
                                            multirowTD26 = row.FindElements(By.TagName("td"));
                                            int iRowsCount = multirowTD26.Count;
                                            for (int n = 0; n < iRowsCount; n++)
                                            {
                                                if ((!multirowTD26[n].Text.Contains("Face Amt")) && (!multirowTD26[n].Text.Contains("Certificate")))
                                                {
                                                    IfPaidBy = multirowTD26[n].Text.Replace("\r\n", "~");
                                                    var IfpaySplit = IfPaidBy.Split('~');
                                                    IfPaidBy = IfpaySplit[0];
                                                    PlesePay = IfpaySplit[1];
                                                    DueDate = cctaxyear + "~" + IfPaidBy + "~" + PlesePay;
                                                    gc.insert_date(orderNumber, accno, 1511, DueDate, 1, DateTime.Now);

                                                }
                                            }
                                        }

                                    }
                                    catch { }
                                }
                            }
                            catch { }
                            try
                            {

                                string bulktext1 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[5]")).Text.Replace("\r\n", ",");
                                taxowner = gc.Between(bulktext1, "Owner,", "Situs address").Replace(".", "");
                                tax_addr = gc.Between(bulktext1, "Situs address", "Legal description").Trim().Replace(",", "");
                                //*[@id='content']/div[1]/div[7]/div/table[1]/tbody
                                IWebElement currrtaxtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[1]/tbody"));
                                IList<IWebElement> currrtaxtableRow = currrtaxtable.FindElements(By.TagName("tr"));
                                int currrtaxtabletableRowcount = currrtaxtableRow.Count;
                                IList<IWebElement> currrtaxtableRowTD;
                                int e = 0;
                                foreach (IWebElement rowid in currrtaxtableRow)
                                {
                                    currrtaxtableRowTD = rowid.FindElements(By.TagName("td"));
                                    if (currrtaxtableRowTD.Count != 0 && !rowid.Text.Contains("Account number"))
                                    {
                                        Account = currrtaxtableRowTD[0].Text; alterkey = currrtaxtableRowTD[1].Text; millagecode = currrtaxtableRowTD[3].Text;
                                    }
                                }

                            }
                            catch { }
                            try
                            {

                                string paidbulk = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[3]/div[1]/div")).Text.Trim();
                                cpaiddate = gc.Between(paidbulk, "PAID ", " $").Trim();
                                cpaidamount = gc.Between(paidbulk, "$", "Receipt ").Trim();
                                cpaidamount = "$" + cpaidamount;
                                creceipt = GlobalClass.After(paidbulk, "Receipt #");
                            }
                            catch { }

                            ////Parcel Details
                            IWebElement ITaxSearchparcell = driver.FindElement(By.LinkText("Parcel details"));
                            string strITaxSearchparcell = ITaxSearchparcell.GetAttribute("href");
                            driver.Navigate().GoToUrl(strITaxSearchparcell);
                            Thread.Sleep(3000);
                            gc.CreatePdf(orderNumber, accno, "Parcel Details" + cctaxyear, driver, "FL", "Brevard");


                            //Account Number~Alternate Key~Owner Name & Mailing Address~Property Address~Millage Code~Millage rate~Tax Year~Tax Amount~Gross Tax~Paid Amount~Paid Date~Effective Date~Receipt Number
                            string curtax = alterkey + "~" + taxowner + "~" + tax_addr + "~" + millagecode + "~" + milagerate + "~" + cctaxyear + "~" + combinedtaxamount + "~" + grosstax + "~" + cpaidamount + "~" + cpaiddate + "~" + ceffdate + "~" + creceipt + "~" + Tax_Authority;
                            gc.insert_date(orderNumber, accno, 1512, curtax, 1, DateTime.Now);
                            q++;
                        }
                        catch
                        { }
                    }

                    //issue certificate
                    string adno = "", faceamount = "", issuedate = "", ex_date = "", buyer = "", intrate = "", cer_no = "";
                    foreach (string URL1 in strissuecertificate)
                    {
                        driver.Navigate().GoToUrl(URL1);
                        Thread.Sleep(3000);
                        try
                        {
                            string issueyear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/p")).Text.Trim().Replace("This parcel has an issued certificate for ", "").Replace(".", "");
                            gc.CreatePdf(orderNumber, accno, "Issue Certificate Details" + issueyear, driver, "FL", "Brevard");
                            string issuecertificatebulktext = "";
                            try
                            {
                                issuecertificatebulktext = driver.FindElement(By.XPath("//*[@id='content']/div[1]/dl")).Text.Trim();
                            }
                            catch { }
                            try
                            {
                                if (!issuecertificatebulktext.Contains("Advertised number") || issuecertificatebulktext == "")
                                {
                                    issuecertificatebulktext = driver.FindElement(By.XPath("//*[@id='content']/div[1]/dl[2]")).Text.Trim();
                                }
                            }
                            catch { }
                            cer_no = driver.FindElement(By.XPath("//*[@id='certificate']")).Text.Replace("Certificate #", "");
                            adno = gc.Between(issuecertificatebulktext, "Advertised number", "Face amount").Trim();
                            faceamount = gc.Between(issuecertificatebulktext, "Face amount", "Issued date").Trim();
                            issuedate = gc.Between(issuecertificatebulktext, "Issued date", "Expiration date").Trim();
                            ex_date = gc.Between(issuecertificatebulktext, "Expiration date", "Buyer").Trim();
                            buyer = gc.Between(issuecertificatebulktext, "Buyer", "Interest rate").Trim().Replace("\r\n", ",");
                            intrate = GlobalClass.After(issuecertificatebulktext, "Interest rate");
                            //Tax Year~Certificate Number~Advertised Number~Face Amount~Issued Date~Expiration Date~Buyer~Interest Rate
                            string isscer = issueyear + "~" + cer_no + "~" + adno + "~" + faceamount + "~" + issuedate + "~" + ex_date + "~" + buyer + "~" + intrate;
                            gc.insert_date(orderNumber, accno, 1514, isscer, 1, DateTime.Now);
                        }
                        catch { }
                    }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "FL", "Brevard", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    driver.Quit();
                    gc.mergpdf(orderNumber, "FL", "Brevard");
                    return "Data Inserted Successfully";
                }
                catch (Exception ex)
                {
                    driver.Quit();
                    throw ex;
                }
            }
        }
        //public void taxHistory(string orderNumber, string parcelid)
        //{

        //    IWebElement IBillHistorytable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table"));
        //    IList<IWebElement> IBillHistoryRow = IBillHistorytable.FindElements(By.TagName("tr"));
        //    IList<IWebElement> IBillHistoryTD;
        //    foreach (IWebElement bill in IBillHistoryRow)
        //    {
        //        IBillHistoryTD = bill.FindElements(By.TagName("td"));
        //        if (IBillHistoryTD.Count != 0 && IBillHistoryTD.Count != 1 && (!bill.Text.Contains("Certificate issued") && !bill.Text.Contains("Advertisement file created") && !(bill.Text.Contains("Certificate redeemed"))))
        //        {
        //            try
        //            {
        //                strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
        //                strBalance = IBillHistoryTD[1].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
        //                strBillDate = IBillHistoryTD[2].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
        //                if (strBillDate.Contains("Effective"))
        //                {
        //                    strBillDate = GlobalClass.Before(IBillHistoryTD[2].Text, "Effective");
        //                    strBillEff = GlobalClass.After(IBillHistoryTD[2].Text, "Effective");
        //                }
        //                //strBillPaid = IBillHistoryTD[3].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
        //                string billpaid = IBillHistoryTD[3].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
        //                if (billpaid.Contains("Paid") || billpaid.Contains("Receipt"))
        //                {
        //                    strBillPaid = gc.Between(billpaid, "Paid ", " Receipt");
        //                    strBillPayReceipt = GlobalClass.After(billpaid, "Receipt");
        //                }

        //            }
        //            catch
        //            {
        //                strBillDate = "";
        //                strBillPaid = "";
        //            }
        //            if (strBillPaid.Contains("Print (PDF)"))
        //            {
        //                strBillPaid = "";
        //            }

        //            if (strBillPaid.Contains("Deed applied"))
        //            {
        //                strBillPayReceipt = strBillPaid;
        //                strBillPaid = "";
        //            }
        //            string strTaxHistory = strBill + "~" + "" + "~" + "" + "~" + strBalance + "~" + strBillDate + "~" + strBillEff + "~" + strBillPaid + "~" + strBillPayReceipt;
        //            gc.insert_date(orderNumber, parcelid, 1514, strTaxHistory, 1, DateTime.Now);
        //        }
        //        if (IBillHistoryTD.Count != 0 && IBillHistoryTD.Count != 1 && ((bill.Text.Contains("Certificate issued") || bill.Text.Contains("Advertisement file created")) || (bill.Text.Contains("Certificate redeemed"))))
        //        {
        //            try
        //            {
        //                strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
        //                if (!strBill.Contains("Redeemed certificate") && !strBill.Contains("Issued certificate") && !strBill.Contains("Certificate issued") && !strBill.Contains("Advertisement file created") && !strBill.Contains("Certificate redeemed"))
        //                {
        //                    strBill = "";
        //                }
        //                string strBillFaceRate = IBillHistoryTD[1].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
        //                if (strBillFaceRate.Contains("Face") || strBillFaceRate.Contains("Rate"))
        //                {
        //                    strFace = gc.Between(IBillHistoryTD[1].Text, "Face ", "\r\nRate");
        //                    strRate = GlobalClass.After(IBillHistoryTD[1].Text, "Rate ");
        //                    strBillDate = IBillHistoryTD[2].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
        //                    if (strBillDate.Contains("Effective"))
        //                    {
        //                        strBillDate = GlobalClass.Before(IBillHistoryTD[2].Text, "Effective");
        //                        strBillEff = GlobalClass.After(IBillHistoryTD[2].Text, "Effective");
        //                    }
        //                    strBillPayReceipt = IBillHistoryTD[3].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
        //                }
        //                if (!strBillFaceRate.Contains("Face") || !strBillFaceRate.Contains("Rate") && IBillHistoryTD.Count == 2)
        //                {
        //                    strBillDate = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
        //                    if (strBillDate.Contains("Effective"))
        //                    {
        //                        strBillDate = GlobalClass.Before(IBillHistoryTD[2].Text, "Effective");
        //                        strBillEff = GlobalClass.After(IBillHistoryTD[2].Text, "Effective");
        //                    }
        //                    strBillPayReceipt = IBillHistoryTD[1].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
        //                }

        //            }
        //            catch
        //            {
        //                strBillDate = "";
        //                strBillPayReceipt = "";
        //            }
        //            if (strBillPayReceipt.Contains("Print (PDF)"))
        //            {
        //                strBillPayReceipt = "";
        //            }
        //            string strTaxHistory = strBill + "~" + strFace + "~" + strRate + "~" + strBalance + "~" + strBillDate + "~" + strBillEff + "~" + strBillPaid + "~" + strBillPayReceipt;
        //            gc.insert_date(orderNumber, parcelid, 1514, strTaxHistory, 1, DateTime.Now);
        //        }
        //        strBill = ""; strFace = ""; strRate = ""; strBalance = ""; strBillDate = ""; strBillEff = ""; strBillPaid = ""; strBillPayReceipt = "";
        //    }

        //    IWebElement IBillHistoryfoottable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table/tfoot"));
        //    IList<IWebElement> IBillHistoryfootRow = IBillHistoryfoottable.FindElements(By.TagName("tr"));
        //    IList<IWebElement> IBillHistoryfootTD;
        //    foreach (IWebElement bill in IBillHistoryRow)
        //    {
        //        IBillHistoryfootTD = bill.FindElements(By.TagName("th"));
        //        if (IBillHistoryfootTD.Count != 0 && bill.Text.Contains("Total"))
        //        {
        //            try
        //            {
        //                strFBill = IBillHistoryfootTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
        //                strFBalance = IBillHistoryfootTD[1].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
        //                strFBillDate = IBillHistoryfootTD[2].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
        //                strFBillPaid = IBillHistoryfootTD[3].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
        //            }
        //            catch
        //            {
        //                strFBillDate = "";
        //                strFBillPaid = "";
        //            }
        //            if (strBillPaid.Contains("Print (PDF)"))
        //            {
        //                strBillPaid = "";
        //            }
        //            string strTaxHistory = strFBill + "~" + "" + "~" + "" + "~" + strFBalance + "~" + strFBillDate + "~" + "" + "~" + strFBillPaid + "~" + "";
        //            gc.insert_date(orderNumber, parcelid, 1514, strTaxHistory, 1, DateTime.Now);
        //        }
        //    }

        //    foreach (string real in strTaxRealestate)
        //    {
        //        try
        //        {
        //            driver.Navigate().GoToUrl(real);
        //            Thread.Sleep(4000);
        //            strPaid = "";
        //            try
        //            {
        //                string strPaidDeatil = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[6]/div/form/button")).Text;
        //                strPaid = GlobalClass.Before(strPaidDeatil, ":");
        //            }
        //            catch { }
        //            try
        //            {
        //                string strPaidDeatil = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[7]/div/div[3]/form/button")).Text;
        //                strPaid = GlobalClass.Before(strPaidDeatil, ":");
        //            }
        //            catch { }
        //            try
        //            {
        //                strType = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[1]/div[2]")).Text;
        //                strTaxType = GlobalClass.Before(strType, "\r\nPrint this bill (PDF)");
        //            }
        //            catch { }

        //            try
        //            {
        //                strIssuePaid = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[4]/div[1]/div[3]/div[1]/form/div")).Text;
        //            }
        //            catch { }
        //            try
        //            {
        //                strIssue = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[4]/div[1]/div[3]/div[1]/form/button")).Text;
        //            }
        //            catch { }
        //            try
        //            {
        //                strEscrowCode = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[1]/tbody/tr/td[3]")).Text;
        //            }
        //            catch { }
        //            try
        //            {
        //                strTaxBill = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[3]/form/div/div/div/span")).Text;
        //            }
        //            catch { }

        //            try
        //            {
        //                strTaxFeed = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[3]/div[1]/div/div")).Text;
        //            }
        //            catch { }
        //            //Latest Bill
        //            if (strPaid.Contains("Pay this bill") || strTaxBill.Contains("PAID") || strTaxFeed.Contains("Tax Deed") || strTaxBill.Contains("Cannot be paid online"))
        //            {
        //                try
        //                {
        //                    //IWebElement ITaxSearch = driver.FindElement(By.LinkText("Latest bill"));
        //                    //string strITaxSearch = ITaxSearch.GetAttribute("href");
        //                    //driver.Navigate().GoToUrl(strITaxSearch);
        //                    gc.CreatePdf(orderNumber, accno, "Tax Detailed Result Latest Bill" + strTaxType, driver, "FL", "Brevard");
        //                    try
        //                    {//*[@id="content"]/div[1]/div[8]/div/div[1]/div[2]/form
        //                        IWebElement Iurl = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[1]/div[2]/form"));
        //                        string strURL = Iurl.GetAttribute("action");
        //                        downloadbill.Add(strURL);
        //                        //  gc.downloadfile(strURL, orderNumber, parcelid, "Paid Billl Reciept" + strTaxType + "", "FL", "Brevard");
        //                    }
        //                    catch (Exception e)
        //                    {
        //                    }
        //                }
        //                catch { }
        //                if (TaxCount < 1)
        //                {
        //                    TaxCount++;
        //                    //Latest Bill Details
        //                    try
        //                    {
        //                        strVTaxType = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[2]/caption")).Text;
        //                    }
        //                    catch { }//*[@id="content"]/div[1]/div[8]/div/table[2]/tbody
        //                    IWebElement ITax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[2]/tbody"));
        //                    IList<IWebElement> ITaxRow = ITax.FindElements(By.TagName("tr"));
        //                    IList<IWebElement> ITaxTD;
        //                    foreach (IWebElement row in ITaxRow)
        //                    {
        //                        ITaxTD = row.FindElements(By.TagName("td"));
        //                        if (ITaxTD.Count != 0 && ITaxTD.Count == 6)
        //                        {
        //                            try
        //                            {
        //                                strLTaxAuthority = ITaxTD[0].Text;
        //                                strLMillege = ITaxTD[1].Text;
        //                                strLAssess = ITaxTD[2].Text;
        //                                strLExemption = ITaxTD[3].Text;
        //                                strLTaxable = ITaxTD[4].Text;
        //                                strLTaxAmount = ITaxTD[5].Text;
        //                            }
        //                            catch
        //                            { }

        //                            string strTaxDetails = strVTaxType + "~" + strTaxType + "~" + strLTaxAuthority + "~" + strLMillege + "~" + strLAssess + "~" + strLExemption + "~" + strLTaxable + "~" + strLTaxAmount;
        //                            gc.insert_date(orderNumber, parcelid, 1510, strTaxDetails, 1, DateTime.Now);

        //                        }
        //                        if (ITaxTD.Count != 0 && ITaxTD.Count < 6)
        //                        {
        //                            try
        //                            {
        //                                strTaxAuthority = ITaxTD[0].Text;
        //                                strMillege = ITaxTD[1].Text;
        //                                strAssess = ITaxTD[2].Text;
        //                                strExemption = ITaxTD[3].Text;
        //                                strTaxable = ITaxTD[4].Text;
        //                                strTaxAmount = ITaxTD[5].Text;
        //                            }
        //                            catch
        //                            { }

        //                            string strTaxDetails = strVTaxType + "~" + strTaxType + "~" + strTaxAuthority + "~" + strMillege + "~" + strAssess + "~" + strExemption + "~" + strTaxable + "~" + strTaxAmount;
        //                            gc.insert_date(orderNumber, parcelid, 1510, strTaxDetails, 1, DateTime.Now);

        //                            strTaxAuthority = ""; strMillege = ""; strAssess = ""; strExemption = ""; strTaxable = ""; strTaxAmount = "";
        //                        }
        //                    }

        //                    IWebElement ITaxBill = driver.FindElement(By.XPath(" //*[@id='content']/div[1]/div[8]/div/table[2]/tfoot"));
        //                    IList<IWebElement> ITaxBillRow = ITaxBill.FindElements(By.TagName("tr"));
        //                    IList<IWebElement> ITaxBillTD;
        //                    foreach (IWebElement bill in ITaxBillRow)
        //                    {
        //                        ITaxBillTD = bill.FindElements(By.TagName("td"));
        //                        if (ITaxBillTD.Count != 0 && bill.Text.Contains("Total"))
        //                        {
        //                            try
        //                            {
        //                                strbillTaxAuthority = ITaxBillTD[0].Text;
        //                                strbillTaxAmount = ITaxBillTD[1].Text;
        //                                strbillAssess = ITaxBillTD[2].Text;
        //                                strbillExemption = ITaxBillTD[3].Text;
        //                                strbillTaxable = ITaxBillTD[4].Text;
        //                                strbillMillege = ITaxBillTD[5].Text;
        //                            }
        //                            catch { }
        //                            string strTaxBillDetails = strVTaxType + "~" + strTaxType + "~" + "Total" + "~" + strbillTaxAuthority + "~" + strbillMillege + "~" + strbillAssess + "~" + strbillExemption + "~" + strbillTaxAmount;
        //                            gc.insert_date(orderNumber, parcelid, 1510, strTaxBillDetails, 1, DateTime.Now);
        //                        }
        //                    }

        //                    //*[@id="content"]/div[1]/div[8]/div/table[3]
        //                    try
        //                    {
        //                        strNVTaxType = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[3]/caption")).Text;
        //                    }
        //                    catch { }
        //                    IWebElement IValoremTable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[3]/tbody"));
        //                    IList<IWebElement> IValoremRow = IValoremTable.FindElements(By.TagName("tr"));
        //                    IList<IWebElement> IValoremTD;

        //                    foreach (IWebElement Valorem in IValoremRow)
        //                    {
        //                        IValoremTD = Valorem.FindElements(By.TagName("td"));
        //                        if (IValoremTD.Count != 0 && Valorem.Text.Contains("$"))
        //                        {
        //                            strValoremAuthority = IValoremTD[0].Text;
        //                            strValoremRate = IValoremTD[1].Text;
        //                            strValoremAmount = IValoremTD[2].Text;

        //                            string strValoremDetails = strNVTaxType + "~" + strValoremAuthority + "~" + strValoremRate + "~" + strValoremAmount;
        //                            gc.insert_date(orderNumber, parcelid, 1515, strValoremDetails, 1, DateTime.Now);
        //                        }
        //                    }

        //                    IWebElement IValTable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[3]/tfoot"));
        //                    IList<IWebElement> IvalRow = IValTable.FindElements(By.TagName("tr"));
        //                    IList<IWebElement> IvalTD;
        //                    foreach (IWebElement val in IvalRow)
        //                    {
        //                        IvalTD = val.FindElements(By.TagName("td"));
        //                        if (IvalTD.Count != 0 && val.Text.Contains("$"))
        //                        {
        //                            try
        //                            {
        //                                strValAuthority = IvalTD[0].Text;
        //                                strValRate = IvalTD[1].Text;
        //                                strValAmount = IvalTD[2].Text;
        //                            }
        //                            catch { }

        //                            string strValDetails = strNVTaxType + "~" + "Total" + "~" + strValRate + "~" + strValAuthority;
        //                            gc.insert_date(orderNumber, parcelid, 1515, strValDetails, 1, DateTime.Now);
        //                        }

        //                        if (IvalTD.Count != 0 && val.Text.Contains("No non-ad valorem assessments."))
        //                        {
        //                            strValoremAuthority = IvalTD[0].Text;

        //                            string strValoremDetails = strNVTaxType + "~" + strValoremAuthority + "~" + "" + "~" + "";
        //                            gc.insert_date(orderNumber, parcelid, 1515, strValoremDetails, 1, DateTime.Now);
        //                        }
        //                    }

        //                    //try
        //                    //{
        //                    //    //strcombine = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/p")).Text;
        //                    //    //strcombine = GlobalClass.After(strcombine, "Combined taxes and assessments: ");
        //                    //    string strTaxDetails = strTaxType + "~" + "" + "~" + "";
        //                    //    gc.insert_date(orderNumber, parcelid, 1511, strTaxDetails, 1, DateTime.Now);
        //                    //}
        //                    //catch { }
        //                    //*[@id="content"]/div[1]/div[8]/div/table[4]/tbody
        //                    IWebElement ITaxCom = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]/tbody"));
        //                    IList<IWebElement> ITaxComRow = ITaxCom.FindElements(By.TagName("tr"));
        //                    IList<IWebElement> ITaxComTD;
        //                    foreach (IWebElement taxrow in ITaxComRow)
        //                    {
        //                        ITaxComTD = taxrow.FindElements(By.TagName("td"));
        //                        if (ITaxComTD.Count != 0 && taxrow.Text.Contains(""))
        //                        {
        //                            for (int j = 0; j < ITaxComTD.Count; j++)
        //                            {
        //                                strTaxCom = ITaxComTD[j].Text;
        //                                string current = DateTime.Now.Year.ToString();
        //                                string strCurrentYear = current.Substring(0, 2);
        //                                if ((!strTaxCom.Contains("Face Amt") && !strTaxCom.Contains("Bid ") && !strTaxCom.Contains("Bidder") && !strTaxCom.Contains("Certificate")) && (strTaxCom.Contains(strCurrentYear) || (strTaxCom.Contains("$"))))
        //                                {
        //                                    strDate = GlobalClass.Before(strTaxCom, "\r\n");
        //                                    strAmount = GlobalClass.After(strTaxCom, "\r\n");

        //                                    string strTaxDetails = strTaxType + "~" + strDate + "~" + strAmount;
        //                                    gc.insert_date(orderNumber, parcelid, 1511, strTaxDetails, 1, DateTime.Now);
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }

        //            string certificate = "", strCertificate = "", stradvertisedNo = "", strFaceAmount = "", strIssuedDate = "", strExpirationDate = "", strBuyer = "", strInterestRate = "", IssuedYear = "", strIssuedYear = "";
        //            strcombine = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/p")).Text;
        //            strcombine = GlobalClass.After(strcombine, "Combined taxes and assessments: ");
        //            try
        //            {
        //                if (strIssue.Contains("Pay this bill:") || strTaxBill.Contains("Cannot be paid online") || strTaxBill.Contains("PAID"))
        //                {
        //                    try
        //                    {
        //                        IWebElement ITaxSearch = driver.FindElement(By.LinkText("Parcel details"));
        //                        string strITaxSearch = ITaxSearch.GetAttribute("href");
        //                        driver.Navigate().GoToUrl(strITaxSearch);

        //                        strissueParcelNo = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[3]/div[2]/dl[2]/dd[1]")).Text;
        //                        strissueAlternetKey = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[3]/div[2]/dl[2]/dd[2]")).Text;
        //                        strissueMillageCode = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[3]/div[2]/dl[2]/dd[3]")).Text;
        //                        strissueMillageRate = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[3]/div[2]/dl[2]/dd[4]")).Text;
        //                        certificate = driver.FindElement(By.XPath("//*[@id='certificate']")).Text;
        //                        strCertificate = GlobalClass.After(certificate, "Certificate ");
        //                        stradvertisedNo = driver.FindElement(By.XPath("//*[@id='content']/div[1]/dl/dd[1]")).Text;
        //                        strFaceAmount = driver.FindElement(By.XPath("//*[@id='content']/div[1]/dl/dd[2]")).Text;
        //                        strIssuedDate = driver.FindElement(By.XPath("//*[@id='content']/div[1]/dl/dd[3]")).Text;
        //                        strExpirationDate = driver.FindElement(By.XPath("//*[@id='content']/div[1]/dl/dd[4]")).Text;
        //                        strBuyer = driver.FindElement(By.XPath("//*[@id='content']/div[1]/dl/dd[5]")).Text;
        //                        strInterestRate = driver.FindElement(By.XPath("//*[@id='content']/div[1]/dl/dd[6]")).Text;
        //                        IssuedYear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/p")).Text;
        //                        strIssuedYear = GlobalClass.After(IssuedYear, "This parcel has an issued certificate for ").Replace(".", "");
        //                        if (IssuedYear.Contains("This parcel has an issued certificate") && !IssuedYear.Contains("This parcel has a redeemed certificate"))
        //                        {
        //                            gc.CreatePdf(orderNumber, accno, "Tax Detailed Result Parcel Details" + strTaxType + "", driver, "FL", "Brevard");

        //                            // Parcel Details 
        //                            string strTaxIssue = strTaxType + "~" + strissueAlternetKey + "~" + strEscrowCode + "~" + strissueMillageCode + "~" + strissueMillageRate + "~" + strcombine + "~" + strCertificate + "~" + stradvertisedNo + "~" + strFaceAmount + "~" + strIssuedDate + "~" + strExpirationDate + "~" + strBuyer + "~" + strIssuedYear;
        //                            gc.insert_date(orderNumber, strissueParcelNo, 1509, strTaxIssue, 1, DateTime.Now);
        //                        }
        //                        if (IssuedYear.Contains("This parcel has a redeemed certificate") && !IssuedYear.Contains("This parcel has an issued certificate"))
        //                        {
        //                            gc.CreatePdf(orderNumber, accno, "Tax Detailed Result Parcel Details" + strTaxType + "", driver, "FL", "Brevard");

        //                            // Parcel Details 
        //                            string strTaxIssue = strTaxType + "~" + strissueAlternetKey + "~" + strEscrowCode + "~" + strissueMillageCode + "~" + strissueMillageRate + "~" + strcombine + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
        //                            gc.insert_date(orderNumber, strissueParcelNo, 1509, strTaxIssue, 1, DateTime.Now);
        //                        }
        //                    }
        //                    catch { }
        //                }
        //                if (certificate == "" && strIssuedYear == "")
        //                {
        //                    gc.CreatePdf(orderNumber, accno, "Tax Detailed Result Parcel Details" + strTaxType + "", driver, "FL", "Brevard");

        //                    string strTaxIssue = strTaxType + "~" + strissueAlternetKey + "~" + strEscrowCode + "~" + strissueMillageCode + "~" + strissueMillageRate + "~" + strcombine + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
        //                    gc.insert_date(orderNumber, strissueParcelNo, 1509, strTaxIssue, 1, DateTime.Now);
        //                }
        //            }
        //            catch { }
        //            try

        //            {
        //                IWebElement IparcelURL = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[3]/div[1]/ul/li[1]/a"));
        //                string parcelhref = IparcelURL.GetAttribute("href");
        //                string strParcelURL = IparcelURL.Text;
        //                driver.Navigate().GoToUrl(parcelhref);
        //                gc.CreatePdf(orderNumber, accno, "Tax Parcel Details" + strTaxType, driver, "FL", "Brevard");
        //            }
        //            catch { }
        //        }
        //        catch { }
        //    }

        public void BillDownload(string orderNumber, string Parcel_number, string BillTax, string dbill, string strbillyear)
        {
            string fileName = "";
            var chromeOptions = new ChromeOptions();
            var downloadDirectory = "F:\\AutoPdf\\";
            chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
            chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
            chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
            chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
            var chDriver = new ChromeDriver(chromeOptions);
            Array.ForEach(Directory.GetFiles(@downloadDirectory), File.Delete); //Delete all the files in auto pdf
            try
            {
                chDriver.Navigate().GoToUrl(BillTax);
                Thread.Sleep(5000);
                try
                {

                    // fileName = "Escambia-County--Real-Estate-" + dbill + "-" + strbillyear + "-Annual-bill.pdf";
                    fileName = latestfilename();
                    Thread.Sleep(3000);
                    gc.AutoDownloadFile(orderNumber, Parcel_number, "Brevard", "FL", fileName);

                }
                catch { }
                try
                {
                    chDriver.Navigate().GoToUrl(BillTax);
                    Thread.Sleep(3000);
                    try
                    {
                        fileName = "Brevard-County-Real-Estate-" + taxparcel + "-" + strbillyear + "-Annual-bill.pdf";
                        gc.AutoDownloadFile(orderNumber, Parcel_number, "FL", "Brevard", fileName);
                    }
                    catch { }

                    try
                    {
                        fileName = "Brevard-County-Real-Estate-" + taxparcel + "-" + strbillyear + "-Installment-bill-1.pdf";
                        gc.AutoDownloadFile(orderNumber, Parcel_number, "FL", "Brevard", fileName);
                    }
                    catch { }
                    try
                    {
                        fileName = "Brevard-County-Real-Estate-" + taxparcel + "-" + strbillyear + "-Installment-bill-2.pdf";
                        gc.AutoDownloadFile(orderNumber, Parcel_number, "FL", "Brevard", fileName);
                    }
                    catch { }
                    try
                    {
                        fileName = "Brevard-County-Real-Estate-" + taxparcel + "-" + strbillyear + "-Installment-bill-3.pdf";
                        gc.AutoDownloadFile(orderNumber, Parcel_number, "FL", "Brevard", fileName);
                    }
                    catch { }

                    try
                    {
                        fileName = "Brevard-County-Real-Estate-" + taxparcel + "-" + strbillyear + "-Installment-bill-4.pdf";
                        gc.AutoDownloadFile(orderNumber, Parcel_number, "FL", "Brevard", fileName);
                    }
                    catch { }
                    
                    chDriver.Quit();
            }
            catch (Exception ex)
            {
                chDriver.Quit();
            }
            }
            catch (Exception ex)
            {
                chDriver.Quit();
            }
            chDriver.Quit();
        }
        public string latestfilename()
        {
            var downloadDirectory1 = "F:\\AutoPdf\\";
            var files = new DirectoryInfo(downloadDirectory1).GetFiles("*.*");
            string latestfile = "";
            DateTime lastupdated = DateTime.MinValue;
            foreach (FileInfo file in files)
            {
                if (file.LastWriteTime > lastupdated)
                {
                    lastupdated = file.LastWriteTime;
                    latestfile = file.Name;
                }

            }
            return latestfile;
        }
    }
}
