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
    public class WebDriver_FLMiamiDade
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        Amrock amc = new Amrock();

        public string FTP_MiamiDade(string houseno, string sname, string direction, string account, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "", strAccountNumber = "";
            string Address = houseno + " " + direction + " " + sname;
            int cYear = 0;
            if (Address.Trim() != "")
            { searchType = "address"; }
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new ChromeDriver();
            // driver = new PhantomJSDriver();
            using (driver = new PhantomJSDriver())
            {
                string AlterNateID = "", PropertyAddress = "", LegalDescriptoin = "", YearBuilt = "";
                string Route = "", address = "", parcel = "", owner = "";
                string[] stringSeparators1 = new string[] { "\r\n" };
                List<string> listurl = new List<string>();
                List<string> listurl1 = new List<string>();
                string TaxType1 = "";
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    if (searchType == "titleflex")
                    {
                        string address1 = houseno.Trim() + " " + sname.Trim() + " " + account.Trim();
                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address1, "FL", "Miami-Dade");

                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_FLMiamidade"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("http://www.miamidade.gov/propertysearch/#/");
                        Thread.Sleep(2000);
                        driver.FindElement(By.XPath("//*[@id='search_box']")).SendKeys(Address);
                        int Y = 0;
                        IWebElement AddreTB = driver.FindElement(By.XPath("//*[@id='address']/div"));
                        IList<IWebElement> AddreTR = AddreTB.FindElements(By.TagName("input"));
                        foreach (IWebElement row1 in AddreTR)
                        {
                            if (Y == 1)
                            {
                                row1.SendKeys(account);
                            }
                            Y++;

                        }
                        // driver.FindElement(By.ClassName("form-control add-suite ng-pristine ng-valid")).SendKeys(account);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "FL", "Miami-Dade");

                        driver.FindElement(By.XPath("//*[@id='search_submit']")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Address search Result", driver, "FL", "Miami-Dade");
                        Thread.Sleep(1000);


                        string Muiti = driver.FindElement(By.XPath("//*[@id='results_list']/div/div[1]/h3")).Text;

                        if ((Muiti != "Exact match not found for search criteria entered.     1 possible match(es) are listed below.") && (Muiti != ""))
                        {

                            IWebElement MultiOwnerTable = driver.FindElement(By.XPath("//*[@id='results_list']"));
                            IList<IWebElement> MultiOwnerRow = MultiOwnerTable.FindElements(By.ClassName("results_record"));
                            IList<IWebElement> MultiOwnerTD;
                            //string AlterNateID = "", PropertyAddress = "", LegalDescriptoin = "", YearBuilt = "";
                            foreach (IWebElement row1 in MultiOwnerRow)
                            {
                                MultiOwnerTD = row1.FindElements(By.TagName("div"));
                                if (MultiOwnerTD.Count != 0)
                                {
                                    parcelNumber = MultiOwnerTD[0].Text.Replace("FOLIO:", "");
                                    owner = MultiOwnerTD[2].Text.Replace("OWNER:", "");
                                    PropertyAddress = MultiOwnerTD[3].Text.Replace("PROP. ADDR:", "");
                                    if (PropertyAddress == "")
                                    {
                                        PropertyAddress = MultiOwnerTD[4].Text.Replace("PROP. ADDR:", "");
                                    }
                                    string Multi = owner + "~" + PropertyAddress;
                                    gc.insert_date(orderNumber, parcelNumber, 358, Multi, 1, DateTime.Now);
                                }
                            }
                            HttpContext.Current.Session["multiParcel_MiamiDade"] = "Yes";
                            driver.Quit();
                            return "MultiParcel";
                        }
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='results_list']/div/div[2]/div[1]/span")).Click();
                        }
                        catch { }

                    }
                    else if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("http://www.miamidade.gov/propertysearch/#/");
                        Thread.Sleep(2000);
                        driver.FindElement(By.XPath("//*[@id='t-folio']")).Click();
                        Thread.Sleep(2000);
                        IWebElement input = driver.FindElement(By.XPath("//*[@id='contentScrollPoint']/div[3]/div[1]/div[2]"));
                        IList<IWebElement> Click = input.FindElements(By.TagName("input"));
                        int inpu = 0;
                        foreach (IWebElement row1 in Click)
                        {
                            if (inpu == 4)
                            {
                                row1.SendKeys(parcelNumber);
                                break;
                            }
                            inpu++;
                        }
                        Thread.Sleep(2000);
                        IWebElement element = driver.FindElement(By.XPath("//*[@id='contentScrollPoint']/div[3]/div[1]/div[2]"));
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "FL", "Miami-Dade");
                        IList<IWebElement> BtnClick = element.FindElements(By.TagName("button"));
                        int btnowner = 0;
                        foreach (IWebElement row1 in BtnClick)
                        {
                            if (btnowner == 3)
                            {
                                row1.Click();
                                break;
                            }
                            btnowner++;
                        }
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Result", driver, "FL", "Miami-Dade");
                    }
                    else if (searchType == "ownername")
                    {
                        driver.Navigate().GoToUrl("http://www.miamidade.gov/propertysearch/#/");
                        Thread.Sleep(2000);
                        driver.FindElement(By.XPath("//*[@id='t-owner']")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.Name("ownerName")).SendKeys(ownername);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Owner search", driver, "FL", "Miami-Dade");
                        IWebElement element = driver.FindElement(By.XPath("//*[@id='contentScrollPoint']/div[3]/div[1]/div[2]"));
                        IList<IWebElement> BtnClick = element.FindElements(By.TagName("button"));
                        int btnowner = 0;
                        foreach (IWebElement row1 in BtnClick)
                        {
                            if (btnowner == 1)
                            {
                                row1.Click();
                                break;
                            }
                            btnowner++;

                        }

                        Thread.Sleep(8000);
                        gc.CreatePdf_WOP(orderNumber, "Owner search Result", driver, "FL", "Miami-Dade");


                        string Muiti = driver.FindElement(By.XPath("//*[@id='results_list']/div/div[1]/h3")).Text;

                        if ((Muiti != "Exact match not found for search criteria entered.     1 possible match(es) are listed below.") && (Muiti != ""))
                        {
                            IWebElement MultiOwnerTable = driver.FindElement(By.XPath("//*[@id='results_list']"));
                            IList<IWebElement> MultiOwnerRow = MultiOwnerTable.FindElements(By.ClassName("results_record"));
                            IList<IWebElement> MultiOwnerTD;
                            foreach (IWebElement row1 in MultiOwnerRow)
                            {
                                MultiOwnerTD = row1.FindElements(By.TagName("div"));
                                if (MultiOwnerTD.Count != 0)
                                {
                                    parcelNumber = MultiOwnerTD[0].Text.Replace("FOLIO: ", "");
                                    parcelNumber = MultiOwnerTD[0].Text.Replace("FOLIO:", "");
                                    parcelNumber = parcelNumber.Replace("(Reference)", "");
                                    owner = MultiOwnerTD[2].Text.Replace("OWNER:", "");
                                    if (MultiOwnerTD[3].Text.Contains("PROP. ADDR:"))
                                    {
                                        PropertyAddress = MultiOwnerTD[3].Text.Replace("PROP. ADDR:", "");
                                    }
                                    else
                                    {
                                        owner = MultiOwnerTD[2].Text.Replace("OWNER:", "") + " " + MultiOwnerTD[3].Text.Replace("OWNER:", "");
                                    }
                                    //PropertyAddress = MultiOwnerTD[4].Text.Replace("PROP. ADDR:", "");
                                    if (PropertyAddress == "")
                                    {
                                        PropertyAddress = MultiOwnerTD[4].Text.Replace("PROP. ADDR:", "");
                                    }
                                    string Multi = owner + "~" + PropertyAddress;
                                    gc.insert_date(orderNumber, parcelNumber, 358, Multi, 1, DateTime.Now);
                                    PropertyAddress = "";
                                }

                            }
                            HttpContext.Current.Session["multiParcel_MiamiDade"] = "Yes";
                            driver.Quit();
                            return "MultiParcel";
                        }
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='results_list']/div/div[2]/div[1]/span")).Click();

                        }
                        catch { }
                    }

                    Thread.Sleep(2000);
                    try
                    {
                        IWebElement INoData = driver.FindElement(By.XPath("//*[@id='error-modal']/div/div"));
                        if (INoData.Text.Contains("Could not find this address") || INoData.Text.Contains("No Data found"))
                        {
                            HttpContext.Current.Session["Nodata_FLMiamidade"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    gc.CreatePdf(orderNumber, parcelNumber, "Assessment information", driver, "FL", "Miami-Dade");
                    string Subdivision = "", PrimaryLandUse = "", yearBuilt;
                    try
                    {
                        PropertyAddress = driver.FindElement(By.XPath("//*[@id='property_info']/tbody/tr[4]/td/div/span/div")).Text.Replace("\r\n", "");
                    }
                    catch { }
                    parcelNumber = driver.FindElement(By.XPath("//*[@id='property_info']/tbody/tr[2]/td")).Text.Replace("Folio:", "").Trim().Replace("(Reference)", "");
                    amc.TaxId = parcelNumber;
                    Subdivision = driver.FindElement(By.XPath("//*[@id='property_info']/tbody/tr[3]/td/div")).Text;
                    ownername = driver.FindElement(By.XPath("//*[@id='property_info']/tbody/tr[5]/td/div")).Text;
                    PrimaryLandUse = driver.FindElement(By.XPath("//*[@id='property_info']/tbody/tr[8]/td/div")).Text;
                    yearBuilt = driver.FindElement(By.XPath("//*[@id='property_info']/tbody/tr[16]/td[2]")).Text;

                    string Property = Subdivision + "~" + PropertyAddress + "~" + PrimaryLandUse + "~" + yearBuilt + "~" + ownername;
                    gc.insert_date(orderNumber, parcelNumber, 337, Property, 1, DateTime.Now);

                    string strAssessYear = "", strLandValue = "", strBuildingValue = "", strExtraFeature = "", strMarketValue = "", strAseesedValue = "";
                    string[] strYear, strLand, strBuilding, strExtra, strMarket, strAseesed;
                    IWebElement Assess = driver.FindElement(By.XPath("//*[@id='contentScrollPoint']/div[4]/div[4]/div[1]/div[1]/div/table/tbody"));
                    IList<IWebElement> AssessRow = Assess.FindElements(By.TagName("tr"));
                    IList<IWebElement> AssessTD;
                    foreach (IWebElement row1 in AssessRow)
                    {
                        AssessTD = row1.FindElements(By.TagName("td"));
                        if (AssessTD.Count != 0 && AssessTD.Count != 1)
                        {
                            if (row1.Text.Contains("Year"))
                            {
                                strAssessYear = AssessTD[1].Text + "~" + AssessTD[2].Text + "~" + AssessTD[3].Text;
                            }
                            if (row1.Text.Contains("Land Value"))
                            {
                                strLandValue = AssessTD[1].Text + "~" + AssessTD[2].Text + "~" + AssessTD[3].Text;
                            }
                            if (row1.Text.Contains("Building Value"))
                            {
                                strBuildingValue = AssessTD[1].Text + "~" + AssessTD[2].Text + "~" + AssessTD[3].Text;
                            }
                            if (row1.Text.Contains("Extra Feature Value"))
                            {
                                strExtraFeature = AssessTD[1].Text + "~" + AssessTD[2].Text + "~" + AssessTD[3].Text;
                            }
                            if (row1.Text.Contains("Market Value"))
                            {
                                strMarketValue = AssessTD[1].Text + "~" + AssessTD[2].Text + "~" + AssessTD[3].Text;
                            }
                            if (row1.Text.Contains("Assessed Value"))
                            {
                                strAseesedValue = AssessTD[1].Text + "~" + AssessTD[2].Text + "~" + AssessTD[3].Text;
                            }
                            //Amc Mapping
                            //if (row1.Text.Contains("Land Value"))
                            //{
                            //    amc.LandValue = AssessTD[2].Text;
                            //}
                            //if (row1.Text.Contains("Building Value"))
                            //{
                            //    amc.BuildingValue = AssessTD[2].Text;
                            //}
                            //if (row1.Text.Contains("Assessed Value"))
                            //{
                            //    amc.Assessed Value = AssessTD[2].Text;
                            //}
                        }
                    }

                    string strTaxableValueType = "", strTaxbleYear = "", strTaxableCountyEx = "", strTaxableCountyTax = "", strTaxableSchoolEx = "", strTaxableSchoolTax = "", strTaxableCityEx = "", strTaxableCityTax = "", strTaxableRegionalEx = "", strTaxableRegionalTax = "";
                    string[] TaxableValueType, strTYear, strTCountyEx, strTSchoolEx, strTCityEx, strTRegionalEx, strTCountyTax, strTSchoolTax, strTCityTax, strTRegionalTax;
                    IWebElement ITaxable = driver.FindElement(By.XPath("//*[@id='contentScrollPoint']/div[4]/div[4]/div[1]/div[2]/div/table/tbody"));
                    IList<IWebElement> ITaxableRow = ITaxable.FindElements(By.TagName("tr"));
                    IList<IWebElement> ITaxableTD;
                    foreach (IWebElement Taxable in ITaxableRow)
                    {
                        ITaxableTD = Taxable.FindElements(By.XPath("td"));
                        if (ITaxableTD.Count != 0)
                        {
                            //string current = DateTime.Now.Year.ToString();
                            //string strCurrentYear = current.Substring(0,2);
                            //try
                            //{
                            //    if (Taxable.Text.Contains(strCurrentYear) && ITaxableTD[0].Text.Trim() == "")
                            //    {
                            //        strTaxbleYear = ITaxableTD[1].Text + "~" + ITaxableTD[2].Text + "~" + ITaxableTD[3].Text;
                            //    }
                            //}
                            //catch { }

                            if (strTaxableValueType.Contains("COUNTY"))
                            {
                                if (Taxable.Text.Contains("Exemption Value"))
                                {
                                    strTaxableCountyEx = ITaxableTD[1].Text + "~" + ITaxableTD[2].Text + "~" + ITaxableTD[3].Text;
                                }
                                if (Taxable.Text.Contains("Taxable Value"))
                                {
                                    strTaxableCountyTax = ITaxableTD[1].Text + "~" + ITaxableTD[2].Text + "~" + ITaxableTD[3].Text;
                                }
                            }

                            if (strTaxableValueType.Contains("SCHOOL BOARD"))
                            {
                                if (Taxable.Text.Contains("Exemption Value"))
                                {
                                    strTaxableSchoolEx = ITaxableTD[1].Text + "~" + ITaxableTD[2].Text + "~" + ITaxableTD[3].Text;
                                }
                                if (Taxable.Text.Contains("Taxable Value"))
                                {
                                    strTaxableSchoolTax = ITaxableTD[1].Text + "~" + ITaxableTD[2].Text + "~" + ITaxableTD[3].Text;
                                }
                            }

                            if (strTaxableValueType.Contains("CITY"))
                            {
                                if (Taxable.Text.Contains("Exemption Value"))
                                {
                                    strTaxableCityEx = ITaxableTD[1].Text + "~" + ITaxableTD[2].Text + "~" + ITaxableTD[3].Text;
                                }
                                if (Taxable.Text.Contains("Taxable Value"))
                                {
                                    strTaxableCityTax = ITaxableTD[1].Text + "~" + ITaxableTD[2].Text + "~" + ITaxableTD[3].Text;
                                }
                            }

                            if (strTaxableValueType.Contains("REGIONAL"))
                            {
                                if (Taxable.Text.Contains("Exemption Value"))
                                {
                                    strTaxableRegionalEx = ITaxableTD[1].Text + "~" + ITaxableTD[2].Text + "~" + ITaxableTD[3].Text;
                                }
                                if (Taxable.Text.Contains("Taxable Value"))
                                {
                                    strTaxableRegionalTax = ITaxableTD[1].Text + "~" + ITaxableTD[2].Text + "~" + ITaxableTD[3].Text;
                                }
                            }

                            try
                            {
                                strTaxableValueType = ITaxableTD[0].Text;
                            }
                            catch { }
                        }
                    }
                    // Year~Land Value~Building Value~Extra Feature Value~Market Value~Assessed Value~County Exemption Value~County TaxableValue~SCHOOL Exemption Value~SCHOOL Taxable Value~City Exemption Value~City Taxable Value~REGIONAL Exemption Value~REGIONAL Taxable Value
                    try
                    {
                        strYear = strAssessYear.Split('~');
                        strLand = strLandValue.Split('~');
                        strBuilding = strBuildingValue.Split('~');
                        strExtra = strExtraFeature.Split('~');
                        strMarket = strMarketValue.Split('~');
                        strAseesed = strAseesedValue.Split('~');
                        strTCountyEx = strTaxableCountyEx.Split('~');
                        strTCountyTax = strTaxableCountyEx.Split('~');
                        strTSchoolEx = strTaxableSchoolEx.Split('~');
                        strTSchoolTax = strTaxableSchoolEx.Split('~');
                        strTCityEx = strTaxableCityEx.Split('~');
                        strTCityTax = strTaxableCityEx.Split('~');
                        strTRegionalEx = strTaxableRegionalEx.Split('~');
                        strTRegionalTax = strTaxableRegionalEx.Split('~');

                        for (int k = 0; k < strYear.Count(); k++)
                        {
                            try
                            {
                                string Assessment = strYear[k] + "~" + strLand[k] + "~" + strBuilding[k] + "~" + strExtra[k] + "~" + strMarket[k] + "~" + strAseesed[k] + "~" + strTCountyEx[k] + "~" + strTCountyTax[k] + "~" + strTSchoolEx[k] + "~" + strTSchoolTax[k] + "~" + strTCityEx[k] + "~" + strTCityTax[k] + "~" + strTRegionalEx[k] + "~" + strTRegionalTax[k];
                                gc.insert_date(orderNumber, parcelNumber, 339, Assessment, 1, DateTime.Now);
                            }
                            catch { }
                        }
                    }
                    catch { }

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("https://miamidade.county-taxes.com/public");
                    Thread.Sleep(2000);
                    driver.FindElement(By.Id("search-text form-control")).SendKeys(parcelNumber.Trim());
                    gc.CreatePdf(orderNumber, parcelNumber.Trim(), "Tax Site Parcel Search", driver, "FL", "Miami-Dade");
                    Thread.Sleep(1000);
                    string AccountNumber = "", MillageCode = "", Millagerate = "", TaxYear = "", TaxAmount = "", PaidAmount = "", PaidDate = "", EffectiveDate = "", ReceiptNumber = "";
                    string Alternatekey = "", MailingAddress = "", PropertyType = "", LegalDescription = "", FullParcelID = "";
                    driver.FindElement(By.XPath("//*[@id='search-controls']/div/form[1]/div[1]/div/span/button")).SendKeys(Keys.Enter);
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, parcelNumber.Trim(), "Tax Site Parcel Search Result", driver, "FL", "Miami-Dade");
                    try { driver.FindElement(By.XPath("//*[@id='results']/div[2]/div/div[4]/div/ul/li[2]/a")).Click(); }
                    catch { }
                    try { driver.FindElement(By.XPath("//*[@id='results']/div[2]/div/div[6]/div/ul/li[2]/a")).Click(); }
                    catch { }

                    List<string> strBillURL = new List<string>();
                    List<string> strIssue = new List<string>();
                    int count = 0, billcount = 0;
                    string currentYear = "", strPreviousYear = "", strBillFace = "", strBillRate = "", strBillType = "", strBillBalance = "", strBillPay = "", strBillPR = "", strBillPaid = "", strBillReciept = "", strBillEff = "";
                    IWebElement IBillHistory = driver.FindElement(By.XPath("//*[@id='content']/div/table"));
                    IList<IWebElement> IBillHistoryRow = IBillHistory.FindElements(By.TagName("tr"));
                    IList<IWebElement> IBillHistoryTD;
                    IList<IWebElement> IBillHistoryTH;
                    foreach (IWebElement bill in IBillHistoryRow)
                    {
                        try
                        {
                            IBillHistoryTD = bill.FindElements(By.TagName("td"));
                            if (IBillHistoryTD.Count != 0)
                            {
                                if (bill.Text.Contains("Pay this bill:"))
                                {
                                    strBillType = IBillHistoryTD[0].Text;
                                    strBillBalance = IBillHistoryTD[1].Text;
                                    strBillPay = IBillHistoryTD[2].Text;
                                    if (strBillPay.Contains("Effective"))
                                    {
                                        strBillPay = GlobalClass.Before(IBillHistoryTD[2].Text, "Effective").Trim();
                                        strBillEff = GlobalClass.After(IBillHistoryTD[2].Text, "Effective").Trim();
                                    }
                                    IWebElement IPay = IBillHistoryTD[0].FindElement(By.TagName("a"));
                                    string strpay = IPay.GetAttribute("href");
                                    strBillURL.Add(strpay);
                                }

                                if (bill.Text.Contains("Installment Bill") && !bill.Text.Contains("Pay this bill:"))
                                {
                                    try
                                    {
                                        strBillType = IBillHistoryTD[0].Text;
                                        strBillBalance = IBillHistoryTD[1].Text;
                                        strBillPay = IBillHistoryTD[2].Text;
                                        if (strBillPay.Contains("Effective"))
                                        {
                                            strBillPay = GlobalClass.Before(IBillHistoryTD[2].Text, "Effective").Trim();
                                            strBillEff = GlobalClass.After(IBillHistoryTD[2].Text, "Effective").Trim();
                                        }
                                        strBillPR = IBillHistoryTD[3].Text;
                                        strBillPaid = gc.Between(strBillPR, "Paid ", " Receipt");
                                        strBillReciept = GlobalClass.After(strBillPR, "Receipt #");
                                        currentYear = GlobalClass.Before(strBillType, " Installment Bill");
                                    }
                                    catch { }
                                    if (billcount < 3)
                                    {
                                        IWebElement IInstall = IBillHistoryTD[0].FindElement(By.TagName("a"));
                                        string strInstall = IInstall.GetAttribute("href");
                                        strBillURL.Add(strInstall);
                                    }
                                }

                                if (bill.Text.Contains("Annual Bill") && !bill.Text.Contains("Pay this bill:"))
                                {
                                    try
                                    {
                                        strBillType = IBillHistoryTD[0].Text;
                                        strBillBalance = IBillHistoryTD[1].Text;
                                        strBillPay = IBillHistoryTD[2].Text;
                                        if (strBillPay.Contains("Effective"))
                                        {
                                            strBillPay = GlobalClass.Before(IBillHistoryTD[2].Text, "Effective").Trim();
                                            strBillEff = GlobalClass.After(IBillHistoryTD[2].Text, "Effective").Trim();
                                        }
                                        strBillPR = IBillHistoryTD[3].Text;
                                        strBillPaid = gc.Between(strBillPR, "Paid ", " Receipt");
                                        strBillReciept = GlobalClass.After(strBillPR, "Receipt #");
                                        currentYear = GlobalClass.Before(strBillType, " Annual Bill");
                                    }
                                    catch { }
                                    if (billcount < 3)
                                    {
                                        IWebElement IAnnual = IBillHistoryTD[0].FindElement(By.TagName("a"));
                                        string strAnnual = IAnnual.GetAttribute("href");
                                        strBillURL.Add(strAnnual);
                                    }
                                }

                                if (bill.Text.Contains("Paid") && !bill.Text.Contains("Installment Bill") && !bill.Text.Contains("Annual Bill"))
                                {
                                    if (IBillHistoryTD[0].Text == "" && IBillHistoryTD[1].Text == "" && IBillHistoryTD[3].Text == "")
                                    {
                                        strBillPay = IBillHistoryTD[2].Text;
                                    }
                                }

                                if (bill.Text.Contains("Certificate redeemed") || bill.Text.Contains("Certificate issued") || bill.Text.Contains("Advertisement file created"))
                                {
                                    strBillType = IBillHistoryTD[0].Text;
                                    if (strBillType.Contains("Issued certificate") || strBillType.Contains("Redeemed certificate"))
                                    {
                                        IWebElement IInstall = IBillHistoryTD[0].FindElement(By.TagName("a"));
                                        string strInstall = IInstall.GetAttribute("href");
                                        strIssue.Add(strInstall);
                                    }
                                    if (!strBillType.Contains("Certificate redeemed") && !strBillType.Contains("Certificate issued") && !strBillType.Contains("Advertisement file created") && !strBillType.Contains("Issued certificate") && !strBillType.Contains("Redeemed certificate"))
                                    {
                                        strBillType = "";
                                    }
                                    try
                                    {
                                        if (IBillHistoryTD[1].Text.Contains("Face") || IBillHistoryTD[1].Text.Contains("Rate"))
                                        {
                                            strBillFace = gc.Between(IBillHistoryTD[1].Text, "Face ", "\r\nRate ");
                                            strBillRate = GlobalClass.After(IBillHistoryTD[1].Text, "Rate ");
                                        }
                                    }
                                    catch { }
                                    try
                                    {
                                        if (!IBillHistoryTD[1].Text.Contains("Face") || !IBillHistoryTD[1].Text.Contains("Rate"))
                                        {
                                            strBillBalance = IBillHistoryTD[1].Text;
                                            strBillPay = IBillHistoryTD[2].Text;
                                        }
                                    }
                                    catch { }
                                }
                                try
                                {
                                    if (currentYear != strPreviousYear)
                                    {
                                        strPreviousYear = currentYear;
                                        count = 0;
                                    }
                                    else
                                    {
                                        if (strPreviousYear == currentYear)
                                        {
                                            count++;
                                        }
                                    }
                                    if (count == 3 && strBillType.Contains("Installment Bill"))
                                    {
                                        billcount++;
                                    }
                                    if (strBillType.Contains("Annual Bill"))
                                    {
                                        billcount++;
                                    }
                                }
                                catch { }

                                string TaxHisDetail = strBillType + "~" + strBillFace + "~" + strBillRate + "~" + strBillBalance + "~" + strBillPay + "~" + strBillEff + "~" + strBillPaid + "~" + strBillReciept;
                                gc.insert_date(orderNumber, parcelNumber, 345, TaxHisDetail, 1, DateTime.Now);
                                strBillType = ""; strBillBalance = ""; strBillPay = ""; strBillPR = ""; strBillPaid = ""; strBillReciept = ""; strBillFace = ""; strBillRate = "";
                            }
                        }
                        catch { }
                    }
                    IWebElement IBill = driver.FindElement(By.XPath("//*[@id='content']/div/table/tfoot"));
                    IList<IWebElement> IBillRow = IBill.FindElements(By.TagName("tr"));
                    foreach (IWebElement totalbill in IBillRow)
                    {
                        IBillHistoryTH = totalbill.FindElements(By.TagName("th"));
                        if (IBillHistoryTH.Count != 0 && totalbill.Text.Contains("$"))
                        {
                            TaxYear = IBillHistoryTH[0].Text;
                            strBillType = IBillHistoryTH[1].Text;
                            strBillPay = IBillHistoryTH[2].Text;
                        }

                        string TaxHisDetail = TaxYear + "~" + "" + "~" + "" + "~" + strBillType + "~" + strBillPay + "~" + "" + "~" + "" + "~" + "";
                        gc.insert_date(orderNumber, parcelNumber, 345, TaxHisDetail, 1, DateTime.Now);
                    }
                    List<string> strURLLink = new List<string>();
                    strURLLink.AddRange(strBillURL);
                    string TaxPreviousYear = "", TaxcurrentYear = "", strParcelURL = "";
                    int Taxcount = 0, Count = 0;
                    foreach (string strURL in strURLLink)
                    {
                        string strTaxAuthority = "", strTaxyear = "", strRate = "", strAmount = "", strTAuthority = "", strOwner = "", strAddress = "", strMillegeCode = "", strMillegeRate = "", strLegadiscription = "",
                        strTaxingAuthority = "", strmillege = "", strAssessed = "", strExemption = "", strTaxable = "", strTax = "", strAdvertisedNumber, strGross = "", strPaidBy = "", strPaidDate = "", strFaceAmount = "",
                        IssuedDate = "", strExpirationDate = "", strBuyer = "", strInterestRate = "", strIssuedYear = "", strCertificateNumber = "", strPayDate = "", strPayAmount = "", strCombine = "", strCombineTax = "", strTaxPayDate = "",
                        strTaxPayAmount = "", payReciept = "", strURLParc = ""; strParcelURL = "";
                        driver.Navigate().GoToUrl(strURL);
                        strURLParc = GlobalClass.After(strURL, "bills/");
                        try
                        {
                            strTaxyear = driver.FindElement(By.XPath("//*[@id='content']/div/div[7]/div/div[1]/div[2]")).Text;
                            TaxYear = GlobalClass.Before(strTaxyear, "\r\n");
                            if (TaxYear.Contains("Annual Bill") && Taxcount < 1)
                            {
                                TaxcurrentYear = gc.Between(TaxYear, "Real Estate ", " Annual Bill");
                                amc.TaxYear = TaxcurrentYear;
                                Taxcount++;
                                if (strURLParc != "")
                                {
                                    strParcelURL = "https://miamidade.county-taxes.com/public/real_estate/parcels/" + parcelNumber.Trim() + "?year=" + TaxcurrentYear;
                                }
                            }
                            if (TaxYear.Contains("Installment Bill") && Taxcount < 6)
                            {
                                TaxcurrentYear = gc.Between(TaxYear, "Real Estate ", " Installment Bill");
                                if (TaxcurrentYear != TaxPreviousYear)
                                {
                                    TaxPreviousYear = TaxcurrentYear;
                                }
                                else
                                {
                                    if (TaxcurrentYear == TaxPreviousYear)
                                    {
                                        Count++;
                                    }
                                    if (Count == 3)
                                    {
                                        Taxcount++;
                                    }
                                }

                                if (strURLParc != "")
                                {
                                    strParcelURL = "https://miamidade.county-taxes.com/public/real_estate/parcels/"  + parcelNumber.Trim() + "?year=" + TaxcurrentYear + "&bill_id=" + strURLParc;
                                }
                            }
                        }
                        catch { }

                        if (Taxcount < 2)
                        {
                            string strAuthority = "";
                            //*[@id="content"]/div/div[8]/div/div[5]/div[1]/dl/dd


                            try
                            {
                                string strOwnerName = driver.FindElement(By.XPath("//*[@id='content']/div/div[7]/div/div[5]/div[1]/dl/dd")).Text;
                                strOwner = GlobalClass.Before(strOwnerName, "\r\n");
                                strAddress = driver.FindElement(By.XPath("//*[@id='content']/div/div[7]/div/div[5]/div[2]/dl/dd[1]")).Text.Replace("\r\n", "");
                                strAccountNumber = driver.FindElement(By.XPath("//*[@id='content']/div/div[7]/div/table[1]/tbody/tr/td[1]/a")).Text;
                                strMillegeCode = driver.FindElement(By.XPath("//*[@id='content']/div/div[7]/div/table[1]/tbody/tr/td[3]")).Text;
                                strLegadiscription = driver.FindElement(By.XPath("//*[@id='content']/div/div[7]/div/div[5]/div[2]/dl/dd[2]")).Text;
                                strTAuthority = driver.FindElement(By.XPath("//*[@id='content']/div/div[7]/div/div[4]/div[1]/dl/dd")).Text;
                                strAuthority = GlobalClass.After(strTAuthority, "Mail payments to:\r\n");
                            }
                            catch
                            {

                            }
                            try
                            {
                                string strOwnerName = driver.FindElement(By.XPath("//*[@id='content']/div/div[8]/div/div[5]/div[1]/dl/dd")).Text;
                                strOwner = GlobalClass.Before(strOwnerName, "\r\n");
                                strAddress = driver.FindElement(By.XPath("//*[@id='content']/div/div[8]/div/div[5]/div[2]/dl/dd[1]")).Text.Replace("\r\n", "");
                                strAccountNumber = driver.FindElement(By.XPath("//*[@id='content']/div/div[8]/div/table[1]/tbody/tr/td[1]/a")).Text;
                                strMillegeCode = driver.FindElement(By.XPath("//*[@id='content']/div/div[8]/div/table[1]/tbody/tr/td[3]")).Text;
                                strLegadiscription = driver.FindElement(By.XPath("//*[@id='content']/div/div[8]/div/div[5]/div[2]/dl/dd[2]")).Text;
                                strTAuthority = driver.FindElement(By.XPath("//*[@id='content']/div/div[8]/div/div[4]/div[1]/dl/dd")).Text;
                                strAuthority = GlobalClass.After(strTAuthority, "Mail payments to:\r\n");
                            }
                            catch { }

                            gc.CreatePdf(orderNumber, strAccountNumber.Trim(), TaxYear + "Latest Bill", driver, "FL", "Miami-Dade");

                            try
                            {

                                IWebElement IAdValorem = driver.FindElement(By.XPath("//*[@id='content']/div/div[8]/div/table[2]"));
                                IList<IWebElement> IAdValoremRow = IAdValorem.FindElements(By.TagName("tr"));
                                IList<IWebElement> IAdValoremTD;
                                foreach (IWebElement Advalorem in IAdValoremRow)
                                {
                                    IAdValoremTD = Advalorem.FindElements(By.TagName("td"));
                                    if (IAdValoremTD.Count != 0)
                                    {
                                        try
                                        {
                                            strTaxingAuthority = IAdValoremTD[0].Text;
                                            strmillege = IAdValoremTD[1].Text;
                                            strAssessed = IAdValoremTD[2].Text;
                                            strExemption = IAdValoremTD[3].Text;
                                            strTaxable = IAdValoremTD[4].Text;
                                            strTax = IAdValoremTD[5].Text;
                                        }
                                        catch { }

                                        if (strTaxingAuthority != "" && !strAssessed.Contains("$") && !strmillege.Contains("$"))
                                        {
                                            string TaxDisDetail = TaxYear + "~" + strTaxingAuthority + "~" + strmillege + "~" + strAssessed + "~" + strExemption + "~" + strTaxable + "~" + strTax + "~" + strRate + "~" + strAmount;
                                            gc.insert_date(orderNumber, strAccountNumber, 344, TaxDisDetail, 1, DateTime.Now);
                                        }
                                        strTaxingAuthority = ""; strmillege = ""; strAssessed = ""; strExemption = ""; strTaxable = ""; strTax = "";
                                    }
                                }
                                IWebElement IAdValoremTotal = driver.FindElement(By.XPath("//*[@id='content']/div/div[8]/div/table[2]/tfoot"));
                                IList<IWebElement> IAdValoremRowTotal = IAdValoremTotal.FindElements(By.TagName("tr"));
                                IList<IWebElement> IAdValoremTHTotal;
                                foreach (IWebElement Total in IAdValoremRowTotal)
                                {
                                    IAdValoremTHTotal = Total.FindElements(By.TagName("td"));
                                    if (IAdValoremTHTotal.Count != 0)
                                    {
                                        try
                                        {
                                            strTaxingAuthority = "Total";
                                            strmillege = IAdValoremTHTotal[0].Text;
                                            strTax = IAdValoremTHTotal[1].Text;
                                        }
                                        catch { }

                                        string TaxDisDetail = TaxYear + "~" + strTaxingAuthority + "~" + strmillege + "~" + "" + "~" + "" + "~" + "" + "~" + strTax + "~" + "" + "~" + "";
                                        gc.insert_date(orderNumber, strAccountNumber, 344, TaxDisDetail, 1, DateTime.Now);
                                    }
                                }
                            }
                            catch
                            { }

                            try
                            {

                                IWebElement IAdValorem = driver.FindElement(By.XPath("//*[@id='content']/div/div[7]/div/table[2]"));
                                IList<IWebElement> IAdValoremRow = IAdValorem.FindElements(By.TagName("tr"));
                                IList<IWebElement> IAdValoremTD;
                                foreach (IWebElement Advalorem in IAdValoremRow)
                                {
                                    IAdValoremTD = Advalorem.FindElements(By.TagName("td"));
                                    if (IAdValoremTD.Count != 0)
                                    {
                                        try
                                        {
                                            strTaxingAuthority = IAdValoremTD[0].Text;
                                            strmillege = IAdValoremTD[1].Text;
                                            strAssessed = IAdValoremTD[2].Text;
                                            strExemption = IAdValoremTD[3].Text;
                                            strTaxable = IAdValoremTD[4].Text;
                                            strTax = IAdValoremTD[5].Text;
                                        }
                                        catch { }

                                        if (strTaxingAuthority != "" && !strAssessed.Contains("$") && !strmillege.Contains("$"))
                                        {
                                            string TaxDisDetail = TaxYear + "~" + strTaxingAuthority + "~" + strmillege + "~" + strAssessed + "~" + strExemption + "~" + strTaxable + "~" + strTax + "~" + strRate + "~" + strAmount;
                                            gc.insert_date(orderNumber, strAccountNumber, 344, TaxDisDetail, 1, DateTime.Now);
                                        }
                                        strTaxingAuthority = ""; strmillege = ""; strAssessed = ""; strExemption = ""; strTaxable = ""; strTax = "";
                                    }
                                }
                                IWebElement IAdValoremTotal = driver.FindElement(By.XPath("//*[@id='content']/div/div[7]/div/table[2]/tfoot"));
                                IList<IWebElement> IAdValoremRowTotal = IAdValoremTotal.FindElements(By.TagName("tr"));
                                IList<IWebElement> IAdValoremTHTotal;
                                foreach (IWebElement Total in IAdValoremRowTotal)
                                {
                                    IAdValoremTHTotal = Total.FindElements(By.TagName("td"));
                                    if (IAdValoremTHTotal.Count != 0)
                                    {
                                        try
                                        {
                                            strTaxingAuthority = "Total";
                                            strmillege = IAdValoremTHTotal[0].Text;
                                            strTax = IAdValoremTHTotal[1].Text;
                                        }
                                        catch { }

                                        string TaxDisDetail = TaxYear + "~" + strTaxingAuthority + "~" + strmillege + "~" + "" + "~" + "" + "~" + "" + "~" + strTax + "~" + "" + "~" + "";
                                        gc.insert_date(orderNumber, strAccountNumber, 344, TaxDisDetail, 1, DateTime.Now);
                                    }
                                }
                            }
                            catch
                            { }

                            try
                            {
                                try
                                {
                                    IWebElement INonAdValorem = driver.FindElement(By.XPath("//*[@id='content']/div/div[8]/div/table[3]"));
                                    IList<IWebElement> INonAdValoremRow = INonAdValorem.FindElements(By.TagName("tr"));
                                    IList<IWebElement> INonAdValoremTD;
                                    foreach (IWebElement NonAdvalorem in INonAdValoremRow)
                                    {
                                        INonAdValoremTD = NonAdvalorem.FindElements(By.TagName("td"));
                                        if (INonAdValoremTD.Count != 0)
                                        {
                                            try
                                            {
                                                strTaxingAuthority = INonAdValoremTD[0].Text;
                                                strRate = INonAdValoremTD[1].Text;
                                                strAmount = INonAdValoremTD[2].Text;
                                            }
                                            catch { }
                                            if (strTaxingAuthority != "" || NonAdvalorem.Text.Contains("No non-ad valorem assessments."))
                                            {
                                                string TaxDisDetail = TaxYear + "~" + strTaxingAuthority + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + strRate + "~" + strAmount;
                                                gc.insert_date(orderNumber, strAccountNumber, 344, TaxDisDetail, 1, DateTime.Now);
                                            }
                                        }
                                    }
                                    strRate = ""; strAmount = "";
                                }
                                catch { }

                                try
                                {
                                    IWebElement INonAdValoremTotal = driver.FindElement(By.XPath("//*[@id='content']/div/div[8]/div/table[3]/tfoot"));
                                    IList<IWebElement> INonAdValoremRowTotal = INonAdValoremTotal.FindElements(By.TagName("tr"));
                                    IList<IWebElement> INonAdValoremTHTotal;
                                    foreach (IWebElement Total in INonAdValoremRowTotal)
                                    {
                                        INonAdValoremTHTotal = Total.FindElements(By.TagName("td"));
                                        if (INonAdValoremTHTotal.Count != 0 && INonAdValoremTHTotal.Count == 1 && !Total.Text.Contains("No non-ad valorem assessments."))
                                        {
                                            try
                                            {
                                                strAmount = INonAdValoremTHTotal[0].Text;

                                                string TaxDisDetail = TaxYear + "~" + "Total" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + strAmount;
                                                gc.insert_date(orderNumber, strAccountNumber, 344, TaxDisDetail, 1, DateTime.Now);
                                            }
                                            catch { }
                                        }
                                        if (INonAdValoremTHTotal.Count != 0 && INonAdValoremTHTotal.Count == 2 && !Total.Text.Contains("No non-ad valorem assessments."))
                                        {
                                            try
                                            {
                                                strRate = INonAdValoremTHTotal[0].Text;
                                                strAmount = INonAdValoremTHTotal[1].Text;
                                            }
                                            catch { }
                                            if ((strRate != "" || strAmount != ""))
                                            {
                                                string TaxDisDetail = TaxYear + "~" + "Total" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + strRate + "~" + strAmount;
                                                gc.insert_date(orderNumber, strAccountNumber, 344, TaxDisDetail, 1, DateTime.Now);
                                            }
                                        }
                                        if (INonAdValoremTHTotal.Count != 0 && INonAdValoremTHTotal.Count == 1 && Total.Text.Contains("No non-ad valorem assessments.") && !strTaxingAuthority.Contains("No non-ad valorem assessments."))
                                        {
                                            strTaxingAuthority = INonAdValoremTHTotal[0].Text;

                                            string TaxDisDetail = TaxYear + "~" + strTaxingAuthority + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                            gc.insert_date(orderNumber, strAccountNumber, 344, TaxDisDetail, 1, DateTime.Now);
                                        }
                                    }
                                }
                                catch { }

                            }
                            catch { }
                            try
                            {
                                IWebElement INonAdValorem = driver.FindElement(By.XPath("//*[@id='content']/div/div[8]/div/table[3]"));
                                IList<IWebElement> INonAdValoremRow = INonAdValorem.FindElements(By.TagName("tr"));
                                IList<IWebElement> INonAdValoremTD;
                                foreach (IWebElement NonAdvalorem in INonAdValoremRow)
                                {
                                    INonAdValoremTD = NonAdvalorem.FindElements(By.TagName("td"));
                                    if (INonAdValoremTD.Count != 0)
                                    {
                                        try
                                        {
                                            strTaxingAuthority = INonAdValoremTD[0].Text;
                                            strRate = INonAdValoremTD[1].Text;
                                            strAmount = INonAdValoremTD[2].Text;
                                        }
                                        catch { }
                                        if (strTaxingAuthority != "" || NonAdvalorem.Text.Contains("No non-ad valorem assessments."))
                                        {
                                            string TaxDisDetail = TaxYear + "~" + strTaxingAuthority + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + strRate + "~" + strAmount;
                                            gc.insert_date(orderNumber, strAccountNumber, 344, TaxDisDetail, 1, DateTime.Now);
                                        }
                                    }
                                }
                                strRate = ""; strAmount = "";
                            }
                            catch { }

                            try
                            {
                                IWebElement INonAdValoremTotal = driver.FindElement(By.XPath("//*[@id='content']/div/div[8]/div/table[3]/tfoot"));
                                IList<IWebElement> INonAdValoremRowTotal = INonAdValoremTotal.FindElements(By.TagName("tr"));
                                IList<IWebElement> INonAdValoremTHTotal;
                                foreach (IWebElement Total in INonAdValoremRowTotal)
                                {
                                    INonAdValoremTHTotal = Total.FindElements(By.TagName("td"));
                                    if (INonAdValoremTHTotal.Count != 0 && INonAdValoremTHTotal.Count == 1 && !Total.Text.Contains("No non-ad valorem assessments."))
                                    {
                                        try
                                        {
                                            strAmount = INonAdValoremTHTotal[0].Text;

                                            string TaxDisDetail = TaxYear + "~" + "Total" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + strAmount;
                                            gc.insert_date(orderNumber, strAccountNumber, 344, TaxDisDetail, 1, DateTime.Now);
                                        }
                                        catch { }
                                    }
                                    if (INonAdValoremTHTotal.Count != 0 && INonAdValoremTHTotal.Count == 2 && !Total.Text.Contains("No non-ad valorem assessments."))
                                    {
                                        try
                                        {
                                            strRate = INonAdValoremTHTotal[0].Text;
                                            strAmount = INonAdValoremTHTotal[1].Text;
                                        }
                                        catch { }
                                        if ((strRate != "" || strAmount != ""))
                                        {
                                            string TaxDisDetail = TaxYear + "~" + "Total" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + strRate + "~" + strAmount;
                                            gc.insert_date(orderNumber, strAccountNumber, 344, TaxDisDetail, 1, DateTime.Now);
                                        }
                                    }
                                    if (INonAdValoremTHTotal.Count != 0 && INonAdValoremTHTotal.Count == 1 && Total.Text.Contains("No non-ad valorem assessments.") && !strTaxingAuthority.Contains("No non-ad valorem assessments."))
                                    {
                                        strTaxingAuthority = INonAdValoremTHTotal[0].Text;

                                        string TaxDisDetail = TaxYear + "~" + strTaxingAuthority + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                        gc.insert_date(orderNumber, strAccountNumber, 344, TaxDisDetail, 1, DateTime.Now);
                                    }
                                }
                            }
                            catch { }

                            try
                            {
                                strCombine = driver.FindElement(By.XPath("//*[@id='content']/div/div[7]/div/p")).Text;
                                strCombineTax = GlobalClass.After(strCombine, "Combined taxes and assessments: ");
                                amc.Instamount1 = strCombineTax;
                                string pDate = driver.FindElement(By.XPath("//*[@id='content']/div/div[7]/div/div[6]/div/div[1]/div/div")).Text;
                                string payDate = gc.Between(pDate, "PAID ", "\r\nReceipt").Trim();
                                string[] pay = payDate.Split(' ');
                                strTaxPayDate = pay[0];
                                strTaxPayAmount = pay[2];
                                amc.Instamountpaid1 = strTaxPayAmount;
                                payReciept = GlobalClass.After(pDate, "Receipt ");

                                string strTaxDetails = TaxYear + "~" + strTaxPayDate + "~" + strTaxPayAmount + "~" + payReciept + "~" + strCombineTax;
                                gc.insert_date(orderNumber, parcelNumber, 346, strTaxDetails, 1, DateTime.Now);
                            }
                            catch { }

                            try
                            {
                                strCombine = driver.FindElement(By.XPath("//*[@id='content']/div/div[8]/div/p")).Text;
                                strCombineTax = GlobalClass.After(strCombine, "Combined taxes and assessments: ");
                                amc.Instamount1 = strCombineTax;
                                string pDate = driver.FindElement(By.XPath("//*[@id='content']/div/div[8]/div/div[6]/div/div[1]/div/div")).Text;
                                string payDate = gc.Between(pDate, "PAID ", "\r\nReceipt").Trim();
                                string[] pay = payDate.Split(' ');
                                strTaxPayDate = pay[0];
                                strTaxPayAmount = pay[2];
                                amc.Instamountpaid1 = strTaxPayAmount;
                                payReciept = GlobalClass.After(pDate, "Receipt ");

                                string strTaxDetails = TaxYear + "~" + strTaxPayDate + "~" + strTaxPayAmount + "~" + payReciept + "~" + strCombineTax;
                                gc.insert_date(orderNumber, parcelNumber, 346, strTaxDetails, 1, DateTime.Now);
                            }
                            catch { }

                            try
                            {
                                IWebElement IPay = driver.FindElement(By.XPath("//*[@id='content']/div/div[8]/div/table[4]/tbody"));
                                IList<IWebElement> IPayRow = IPay.FindElements(By.TagName("tr"));
                                IList<IWebElement> IPayTD;
                                foreach (IWebElement Pay in IPayRow)
                                {
                                    IPayTD = Pay.FindElements(By.TagName("td"));
                                    if (IPayTD.Count != 0)
                                    {
                                        if ((Pay.Text.Contains("If paid by:") && Pay.Text.Contains("Please pay:")) && IPayTD.Count == 1)
                                        {
                                            strPayDate = GlobalClass.Before(IPayTD[0].Text, "\r\n");
                                            strPayAmount = GlobalClass.After(IPayTD[0].Text, "\r\n");
                                            amc.DiscountDate1 = strPayDate;
                                            string TaxSaleDetails = TaxYear + "~" + strTaxPayDate + "~" + strTaxPayAmount + "~" + "" + "~" + "";
                                            gc.insert_date(orderNumber, parcelNumber, 346, TaxSaleDetails, 1, DateTime.Now);
                                        }

                                        if (Pay.Text.Contains("Gross") && Pay.Text.Contains("Certificate") && Pay.Text.Contains("Discount") && (Pay.Text.Contains("If paid by:") && Pay.Text.Contains("Please pay:")) && IPayTD.Count == 3)
                                        {
                                            strPayDate = GlobalClass.Before(IPayTD[2].Text, "\r\n");
                                            strPayAmount = GlobalClass.After(IPayTD[2].Text, "\r\n");

                                            string TaxSaleDetails = TaxYear + "~" + strTaxPayDate + "~" + strTaxPayAmount + "~" + "" + "~" + "";
                                            gc.insert_date(orderNumber, parcelNumber, 346, TaxSaleDetails, 1, DateTime.Now);
                                        }
                                    }
                                }
                            }
                            catch { }

                            try
                            {
                                IWebElement IPay = driver.FindElement(By.XPath("//*[@id='content']/div/div[7]/div/table[4]/tbody"));
                                IList<IWebElement> IPayRow = IPay.FindElements(By.TagName("tr"));
                                IList<IWebElement> IPayTD;
                                foreach (IWebElement Pay in IPayRow)
                                {
                                    IPayTD = Pay.FindElements(By.TagName("td"));
                                    if (IPayTD.Count != 0)
                                    {
                                        if ((Pay.Text.Contains("If paid by:") && Pay.Text.Contains("Please pay:")) && IPayTD.Count == 1)
                                        {
                                            strPayDate = GlobalClass.Before(IPayTD[0].Text, "\r\n");
                                            strPayAmount = GlobalClass.After(IPayTD[0].Text, "\r\n");
                                            amc.DiscountDate1 = strPayDate;
                                            string TaxSaleDetails = TaxYear + "~" + strTaxPayDate + "~" + strTaxPayAmount + "~" + "" + "~" + "";
                                            gc.insert_date(orderNumber, parcelNumber, 346, TaxSaleDetails, 1, DateTime.Now);
                                        }

                                        if (Pay.Text.Contains("Gross") && Pay.Text.Contains("Certificate") && Pay.Text.Contains("Discount") && (Pay.Text.Contains("If paid by:") && Pay.Text.Contains("Please pay:")) && IPayTD.Count == 3)
                                        {
                                            strPayDate = GlobalClass.Before(IPayTD[2].Text, "\r\n");
                                            strPayAmount = GlobalClass.After(IPayTD[2].Text, "\r\n");

                                            string TaxSaleDetails = TaxYear + "~" + strTaxPayDate + "~" + strTaxPayAmount + "~" + "" + "~" + "";
                                            gc.insert_date(orderNumber, parcelNumber, 346, TaxSaleDetails, 1, DateTime.Now);
                                        }
                                    }
                                }
                            }
                            catch { }

                            try
                            {
                                if (cYear < 1 && strTaxyear.Contains("Annual Bill"))
                                {
                                    string strstatus = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[6]")).Text;
                                    string[] paiddue = strstatus.Split('\r');

                                    if ((strstatus.Contains("Paid") || strstatus.Contains("PAID")) && !strstatus.Contains("due") && !strstatus.Contains("No taxes due"))
                                    {
                                        string[] due = paiddue[0].Trim().Replace("\n", "").Replace("  ", " ").Split(' ');
                                        amc.Instamountpaid1 = due[2];
                                        cYear++;
                                    }
                                    if (!strstatus.Contains("Paid") && !strstatus.Contains("PAID") && strstatus.Contains("due") && !strstatus.Contains("Pay this bill") && !strstatus.Contains("No taxes due"))
                                    {
                                        string[] due = paiddue[0].Trim().Replace("\n", "").Replace("  ", " ").Split(' ');
                                        //amc.IsDelinquent = "No";
                                        //amc.InstPaidDue1 = "Paid";
                                        double Combine = Convert.ToDouble(strCombineTax.Replace("$", "").Trim());
                                        double amount = Convert.ToDouble(due[2].Replace("$", "").Trim());
                                        if (Combine > amount)
                                        {
                                            amc.IsDelinquent = "No";
                                            amc.InstPaidDue1 = "Due";
                                            amc.Instamountpaid1 = due[2];

                                        }
                                        //if (Combine == amount)
                                        //{
                                        //    amc.IsDelinquent = "No";
                                        //    amc.InstPaidDue1 = "Due";
                                        //}
                                        if (Combine < amount)
                                        {
                                            amc.IsDelinquent = "Yes";
                                            amc.InstPaidDue1 = "Due";
                                        }
                                        cYear++;
                                    }
                                    if (strstatus.Contains("Pay this bill") && !strstatus.Contains("due") && !strstatus.Contains("No taxes due"))
                                    {
                                        double Combine = Convert.ToDouble(strCombineTax.Replace("$", "").Trim());
                                        double amount = 00.00;
                                        if (strstatus.Contains("\r\n"))
                                        {
                                            string stramount = GlobalClass.Before(strstatus, "\r\n");
                                            amount = Convert.ToDouble(stramount.Replace("Pay this bill:", "").Replace("$", "").Trim());
                                        }
                                        else
                                        {
                                            amount = Convert.ToDouble(strstatus.Replace("Pay this bill:", "").Replace("$", "").Trim());
                                        }
                                        if (Combine > amount)
                                        {
                                            amc.IsDelinquent = "No";
                                            amc.InstPaidDue1 = "Due";

                                            amc.Instamountpaid1 = strstatus.Replace("Pay this bill:", "").Trim();
                                        }
                                        //if (Combine == amount)
                                        //{
                                        //    amc.IsDelinquent = "No";
                                        //    amc.InstPaidDue1 = "Due";
                                        //}
                                        if (Combine < amount)
                                        {
                                            amc.IsDelinquent = "Yes";
                                            amc.InstPaidDue1 = "Due";
                                        }

                                        cYear++;
                                    }

                                    if (amc.IsDelinquent == "Yes" && strTaxyear.Contains("Annual Bill"))
                                    {
                                        gc.InsertAmrockTax(orderNumber, amc.TaxId, null, null, null, null, null, null, null, null, null, null, null, null, amc.IsDelinquent);
                                    }

                                    if (amc.IsDelinquent == "No" && strTaxyear.Contains("Annual Bill"))
                                    {
                                        gc.InsertAmrockTax(orderNumber, amc.TaxId, amc.Instamount1, amc.Instamount2, amc.Instamount3, amc.Instamount4, amc.Instamountpaid1, amc.Instamountpaid2, amc.Instamountpaid3, amc.Instamountpaid4, amc.InstPaidDue1, amc.InstPaidDue2, amc.instPaidDue3, amc.instPaidDue4, amc.IsDelinquent);
                                    }
                                }
                            }
                            catch { }



                            try
                            {
                                strTaxPayAmount = driver.FindElement(By.XPath("//*[@id='content']/div/div[8]/div/div[6]/div/form/button")).Text;

                                if ((!strTaxPayAmount.Contains("Gross")) && (strTaxPayAmount.Contains("Pay this bill: ") || strTaxPayAmount.Contains("$")))
                                {
                                    string strTaxPayDetails = TaxYear + "~" + strTaxPayDate + "~" + strTaxPayAmount + "~" + payReciept + "~" + strCombineTax;
                                    gc.insert_date(orderNumber, parcelNumber, 346, strTaxPayDetails, 1, DateTime.Now);
                                }
                                strCombineTax = "";
                            }
                            catch
                            {

                            }


                            try
                            {
                                strTaxPayAmount = driver.FindElement(By.XPath("//*[@id='content']/div/div[7]/div/div[6]/div/form/button")).Text;

                                if ((!strTaxPayAmount.Contains("Gross")) && (strTaxPayAmount.Contains("Pay this bill: ") || strTaxPayAmount.Contains("$")))
                                {
                                    string strTaxPayDetails = TaxYear + "~" + strTaxPayDate + "~" + strTaxPayAmount + "~" + payReciept + "~" + strCombineTax;
                                    gc.insert_date(orderNumber, parcelNumber, 346, strTaxPayDetails, 1, DateTime.Now);
                                }
                                strCombineTax = "";
                            }
                            catch { }


                            try
                            {
                                IWebElement Ibilldownload = driver.FindElement(By.XPath("//*[@id='content']/div/div[8]/div/div[1]/div[2]/form"));
                                string strURLBill = Ibilldownload.GetAttribute("action");
                                gc.downloadfile(strURLBill, orderNumber, parcelNumber.Trim(), "Current Tax_bill" + TaxYear, "FL", "Miami-Dade");
                            }
                            catch { }

                            try
                            {
                                IWebElement Ibilldownload = driver.FindElement(By.XPath("//*[@id='content']/div/div[7]/div/div[1]/div[2]/form"));
                                string strURLBill = Ibilldownload.GetAttribute("action");
                                gc.downloadfile(strURLBill, orderNumber, parcelNumber.Trim(), "Current Tax_bill" + TaxYear, "FL", "Miami-Dade");
                            }
                            catch { }

                            try
                            {
                                //IWebElement IparcelDetails = driver.FindElement(By.XPath("//*[@id='content']/div/div[1]/ul/li[1]/a"));
                                //string strParcelLink = IparcelDetails.GetAttribute("href");
                                //driver.Navigate().GoToUrl(strParcelLink);
                                //IparcelDetails.Click();
                            }
                            catch { }
                            try
                            {
                                //IWebElement IparcelDetails = driver.FindElement(By.XPath("//*[@id='content']/div/div[3]/div[1]/ul/li[1]/a"));
                                //string strParcelLink = IparcelDetails.GetAttribute("href");
                                driver.Navigate().GoToUrl(strParcelURL);
                                //IparcelDetails.Click();
                            }
                            catch { }


                            try
                            {
                                strMillegeRate = driver.FindElement(By.XPath("//*[@id='content']/div/div[3]/div[2]/dl[2]/dd[3]")).Text;
                            }
                            catch { }

                            try
                            {
                                string TaxDetail = strAuthority + "~" + strLegadiscription + "~" + strAddress + "~" + strMillegeCode + "~" + strMillegeRate + "~" + strOwner;
                                gc.insert_date(orderNumber, strAccountNumber, 343, TaxDetail, 1, DateTime.Now);
                                //TaxingAuthority~Legaldescription~PropertyAddress~AccountNumber~MillageCode~Millagerate~ownername
                                gc.CreatePdf(orderNumber, strAccountNumber.Trim(), TaxYear + "Parcel Details", driver, "FL", "Miami-Dade");
                                strMillegeRate = "";
                            }
                            catch { }
                        }

                        if (Taxcount >= 2)
                        {
                            gc.CreatePdf(orderNumber, strAccountNumber.Trim(), TaxYear + "Latest Bill", driver, "FL", "Miami-Dade");
                            try
                            {
                                driver.Navigate().GoToUrl(strParcelURL);
                                //IWebElement IparcelDetails = driver.FindElement(By.XPath("//*[@id='content']/div/div[1]/ul/li[1]/a"));
                                //string strParcelLink = IparcelDetails.GetAttribute("href");
                                //driver.Navigate().GoToUrl(strParcelLink);
                                //IparcelDetails.Click();
                            }
                            catch { }
                            //try
                            //{
                            //    IWebElement IparcelDetails = driver.FindElement(By.XPath("//*[@id='content']/div/div[3]/div[1]/ul/li[1]/a"));
                            //    string strParcelLink = IparcelDetails.GetAttribute("href");
                            //    driver.Navigate().GoToUrl(strParcelLink);
                            //    IparcelDetails.Click();
                            //}
                            //catch { }
                            gc.CreatePdf(orderNumber, strAccountNumber.Trim(), TaxYear + "Parcel Details", driver, "FL", "Miami-Dade");
                        }
                    }

                    foreach (string strIssueURL in strIssue)
                    {
                        driver.Navigate().GoToUrl(strIssueURL);
                        string strAdvertisedNumber, strGross = "", strPaidBy = "", strPaidDate = "", strFaceAmount = "",
                        IssuedDate = "", strExpirationDate = "", strBuyer = "", strInterestRate = "", strIssuedYear = "", strCertificateNumber = "";
                        try
                        {
                            strCertificateNumber = driver.FindElement(By.XPath("//*[@id='certificate']")).Text;
                            strIssuedYear = driver.FindElement(By.XPath("//*[@id='content']/div/p")).Text;
                            strAdvertisedNumber = driver.FindElement(By.XPath("//*[@id='content']/div/dl/dd[1]")).Text;
                            strFaceAmount = driver.FindElement(By.XPath("//*[@id='content']/div/dl/dd[2]")).Text;
                            strIssuedYear = driver.FindElement(By.XPath("//*[@id='content']/div/dl/dd[3]")).Text;
                            strExpirationDate = driver.FindElement(By.XPath("//*[@id='content']/div/dl/dd[4]")).Text;
                            strBuyer = driver.FindElement(By.XPath("//*[@id='content']/div/dl/dd[5]")).Text;
                            strInterestRate = driver.FindElement(By.XPath("//*[@id='content']/div/dl/dd[6]")).Text;

                            gc.CreatePdf(orderNumber, strAccountNumber, "Certificate redeemed" + IssuedDate.Replace("/", ""), driver, "FL", "Miami-Dade");
                            string TaxCertificateDetails = TaxYear + "~" + strAdvertisedNumber + "~" + strFaceAmount + "~" + strIssuedYear + "~" + strExpirationDate + "~" + strBuyer + "~" + strInterestRate + "~" + strIssuedYear + "~" + strCertificateNumber;
                            gc.insert_date(orderNumber, parcelNumber, 347, TaxCertificateDetails, 1, DateTime.Now);
                            //TaxYear +"~"+ AdvertisedNumber~FaceAmount~IssuedDate~ExpirationDate~Buyer~InterestRate~Tax Year~Certificate Number
                        }
                        catch { }
                    }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");

                    gc.insert_TakenTime(orderNumber, "FL", "Miami-Dade", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "FL", "Miami-Dade");
                    // gc.MMREM_Template(orderNumber, parcelNumber, "", driver, "FL", "Miami-Dade", "105", "4");
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