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

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_CharlotteFL
    {
        GlobalClass gc = new GlobalClass();
        string strBill = "-", strBalance = "-", strBillDate = "-", strBillPaid = "-", strFBill = "-", strFBalance = "-", strFBillDate = "-", strFBillPaid = "-";
        IList<IWebElement> taxPaymentdetails, taxPaymentAmountdetails, Itaxtd;
        List<string> strSecured = new List<string>();
        List<string> strCombinedTax = new List<string>();
        List<string> strTaxRealestate = new List<string>();
        List<string> strTaxRealCount = new List<string>();
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        private object driverIE;
        private int multicount;
        private string strMultiAddress;
        private string b;
        string mul = "";
        string TaxYear = "", TaxAmount = "", PaidAmount = "", ReceiptNumber = "", Account_number = "", Millage_Code = "", Millage_rate = "";

        public string FTP_Charlotte(string houseno, string housetype, string sname, string direction, string unitno, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "", AssessTakenTime = "", TaxTakentime = "", CityTaxtakentime = "";
            string TotaltakenTime = "";
            GlobalClass gc = new GlobalClass();

            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string owner = "", folio_parcel_no = "-", Legal_desc = "", property_use = "", Tax_district = "", Year_built = "", Neighbourhood = "", subdivision = "", situs_address = "", pin = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new PhantomJSDriver();
            //driver = new ChromeDriver();
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("https://www.ccappraiser.com/rp_real_search.asp");

                    Thread.Sleep(4000);
                    if (searchType == "titleflex")
                    {
                        string titleaddress = houseno + " " + sname + " " + housetype + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "FL", "Charlotte");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Charlotte_Zero"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {

                        driver.FindElement(By.Id("PropertyAddressNumber")).SendKeys(houseno);
                        driver.FindElement(By.Id("PropertyAddressStreetName")).SendKeys(sname);
                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "FL", "Charlotte");
                        driver.FindElement(By.Id("Button3")).SendKeys(Keys.Enter);
                        Thread.Sleep(5000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search result", driver, "FL", "Charlotte");

                        int trCount = driver.FindElements(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr")).Count;
                        try
                        {
                            string NOdata = driver.FindElement(By.XPath("/html/body/p[1]")).Text;
                            if(NOdata.Contains("Sorry, your selection"))
                            {
                                HttpContext.Current.Session["Charlotte_Zero"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                        if (trCount > 2)
                        {   //multi parcel
                            IWebElement tbmulti2 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody"));
                            IList<IWebElement> TRmulti2 = tbmulti2.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDmulti2;
                            int k = 0;
                            foreach (IWebElement row in TRmulti2)
                            {
                                if (k < 25)
                                {
                                    TDmulti2 = row.FindElements(By.TagName("td"));
                                    if (TDmulti2.Count != 0 && TDmulti2[1].Text.Trim() != "")
                                    {
                                        string multi1 = TDmulti2[1].Text + "~" + TDmulti2[2].Text;
                                        gc.insert_date(orderNumber, TDmulti2[0].Text, 687, multi1, 1, DateTime.Now);
                                        //  Owner~address
                                    }
                                    k++;
                                }
                            }
                            HttpContext.Current.Session["multiParcel_Charlotte"] = "Yes";
                            if (TRmulti2.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Charlotte_Multicount"] = "Maximum";
                            }
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else
                        {
                            Thread.Sleep(2000);
                            IWebElement Multisingle = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td[1]")).FindElement(By.TagName("a"));
                            string Multiclick = Multisingle.GetAttribute("href");
                            driver.Navigate().GoToUrl(Multiclick);
                            //driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td[1]/a")).Click();
                            Thread.Sleep(3000);
                        }
                    }
                    if (searchType == "parcel")
                    {

                        driver.FindElement(By.Id("ParcelID_number")).SendKeys(parcelNumber);

                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "FL", "Charlotte");
                        driver.FindElement(By.Id("Button3")).SendKeys(Keys.Enter);
                        Thread.Sleep(5000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search result", driver, "FL", "Charlotte");
                        try
                        {
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td[1]/a")).Click();
                            Thread.Sleep(3000);
                        }
                        catch { }
                    }

                    if (searchType == "ownername")
                    {

                        string lastname = "", firstname = "";
                        string s = ownername;
                        string[] words = s.Split(' ');
                        if (words.Count() == 1)
                        {
                            lastname = words[0];
                            driver.FindElement(By.Id("owner")).SendKeys(lastname);

                        }
                        else
                        {
                            lastname = words[0];
                            firstname = words[1];

                            driver.FindElement(By.Id("owner")).SendKeys(lastname + " " + firstname);

                        }

                        gc.CreatePdf_WOP(orderNumber, "Owner Search", driver, "FL", "Charlotte");
                        driver.FindElement(By.Id("Button3")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "owner Search result", driver, "FL", "Charlotte");
                        int trCount = driver.FindElements(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr")).Count;

                        if (trCount > 2)
                        {   //multi parcel
                            IWebElement tbmulti2 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody"));
                            IList<IWebElement> TRmulti2 = tbmulti2.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDmulti2;
                            int k = 0;
                            foreach (IWebElement row in TRmulti2)
                            {
                                if (k < 25)
                                {
                                    TDmulti2 = row.FindElements(By.TagName("td"));
                                    if (TDmulti2.Count != 0 && TDmulti2[1].Text.Trim() != "")
                                    {
                                        string multi1 = TDmulti2[1].Text + "~" + TDmulti2[2].Text;
                                        gc.insert_date(orderNumber, TDmulti2[0].Text, 687, multi1, 1, DateTime.Now);
                                        //  Owner~address
                                    }
                                    k++;
                                }
                            }
                            HttpContext.Current.Session["multiParcel_Charlotte"] = "Yes";
                            if (TRmulti2.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Charlotte_Multicount"] = "Maximum";
                            }
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else
                        {
                            try
                            {
                                Thread.Sleep(2000);
                                // /html/body/table/tbody/tr[2]/td[1]/a
                                driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td[1]/a")).Click();
                                Thread.Sleep(3000);
                            }
                            catch { }
                        }
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.XPath("/html/body/p[1]"));
                        if(INodata.Text.Contains("Sorry, your selection returned"))
                        {
                            HttpContext.Current.Session["Charlotte_Zero"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    //property details


                    //Parcel ID~Old Parcel ID Number~Owner Name~Owner Address~Property Address~Business Name~Year Built~Map Number~Current Use~Future Land Use (Comp.Plan)~Property Zip Code~Section-Township-Range~Taxing District~Market Area / Neighborhood / Subneighborhood~SOH Base Year~Short Legal~Legal Description
                    string parcel_no = "", OldParcelID = "", OwnerAddress = "", PropertyAddress = "", BusinessName = "", YearBuilt = "", MapNumber = "", CurrentUse = "", FutureLandUse = "", PropertyZipCode = "", section = "", TaxingDistrict = "", MarketArea = "", SOHBaseYear = "", legal1 = "", legal2 = "";

                    string bulkpropertytext = "", property_details = "";
                    // driver.FindElement(By.Id("Year1")).Click();
                    //Thread.Sleep(2000);

                    bulkpropertytext = driver.FindElement(By.XPath("/html/body/main/section/div/h1")).Text.Replace("\r\n", "");
                    parcel_no = gc.Between(bulkpropertytext, "Information for", "for the").Trim();
                    gc.CreatePdf(orderNumber, parcel_no, "Property details last", driver, "FL", "Charlotte");
                    string Ownersplit = driver.FindElement(By.XPath("/html/body/main/section/div/div[2]/div/div[1]/div[1]")).Text;
                    string[] Ownerarray = Ownersplit.Split('\r');
                    owner = Ownerarray[0];
                    string MilingAddress = Ownerarray[1] + " " + Ownerarray[2];
                    string propertyaddress1 = driver.FindElement(By.XPath("/html/body/main/section/div/div[2]/div/div[2]/div[1]/div[2]")).Text;
                    string propertyaddress2 = driver.FindElement(By.XPath("/html/body/main/section/div/div[2]/div/div[2]/div[2]/div[2]")).Text;
                    PropertyAddress = propertyaddress1 + " " + propertyaddress2;
                    BusinessName = driver.FindElement(By.XPath("/html/body/main/section/div/div[2]/div/div[2]/div[3]/div[2]")).Text;
                    MapNumber = driver.FindElement(By.XPath("/html/body/main/section/div/div[3]/div/div[1]/div/div[6]/div[2]")).Text;
                    CurrentUse = driver.FindElement(By.XPath("/html/body/main/section/div/div[3]/div/div[1]/div/div[2]/div[2]")).Text;
                   // string SectionTownship = 
                    //PropertyZipCode = gc.Between(bulkpropertytext, "Property Zip Code:", "Business Name:").Trim();
                    section = driver.FindElement(By.XPath("/html/body/main/section/div/div[3]/div/div[1]/div/div[7]/div[2]")).Text;
                    TaxingDistrict = driver.FindElement(By.XPath("/html/body/main/section/div/div[3]/div/div[1]/div/div[1]/div[2]")).Text;
                   // MarketArea = gc.Between(bulkpropertytext, "Market Area/Neighborhood/Subneighborhood:", "Waterfront:").Trim();
                   // SOHBaseYear = GlobalClass.After(bulkpropertytext, "SOH Base Year:").Trim();
                    try
                    {
                        YearBuilt = driver.FindElement(By.XPath("/html/body/main/section/div/div[11]/div/table/tbody/tr[2]/td[5]")).Text;
                    }
                    catch { }

                    try
                    {
                        legal1 = driver.FindElement(By.XPath("/html/body/main/section/div/div[13]/div[2]")).Text;
                        legal2 = GlobalClass.After(legal1, "Long Legal:");
                    }
                    catch
                    {
                    }
                    //Old Parcel ID Number~Owner Name & Address~Property Address~Business Name~Year Built~Map Number~Current Use~Future Land Use (Comp.Plan)~Property Zip Code~Section-Township-Range~Taxing District~Market Area / Neighborhood / Subneighborhood~SOH Base Year~Short Legal~Legal Description

                    property_details = owner + "~" + PropertyAddress + "~" + MilingAddress+"~"+ BusinessName + "~" + YearBuilt + "~" + MapNumber + "~" + CurrentUse + "~" + section + "~" + TaxingDistrict + "~" + legal2;
                    gc.insert_date(orderNumber, parcel_no, 680, property_details, 1, DateTime.Now);


                    //Assessment details

                    //2017 Value Summary Details Table:
                    //2017 Value Summary~Land~Land Improvements~Building~Damage~Total
                    IWebElement tbmulti216 = driver.FindElement(By.XPath("/html/body/main/section/div/div[5]/div/table/tbody"));
                    IList<IWebElement> TRmulti216 = tbmulti216.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDmulti216;

                    foreach (IWebElement row in TRmulti216)
                    {

                        TDmulti216 = row.FindElements(By.TagName("td"));
                        if (TDmulti216.Count == 6 && !row.Text.Contains("Building"))
                        {
                            string multi1 = TDmulti216[0].Text + "~" + TDmulti216[1].Text + "~" + TDmulti216[2].Text + "~" + TDmulti216[3].Text + "~" + TDmulti216[4].Text + "~" + TDmulti216[5].Text;
                            gc.insert_date(orderNumber, parcel_no, 681, multi1, 1, DateTime.Now);

                        }

                    }

                    //2017 Certified Tax Roll Values Details Table:
                    //2017 Certified Tax Roll Values~Non-School~School
                    string current = driver.CurrentWindowHandle;
                    driver.FindElement(By.Id("Year1")).Click();
                    Thread.Sleep(2000);
                    driver.SwitchTo().Window(driver.WindowHandles.Last());
                    IWebElement tbmulti226 = driver.FindElement(By.XPath("/html/body/main/section/div/div[2]/div/table/tbody"));
                    IList<IWebElement> TRmulti226 = tbmulti226.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDmulti226;

                    foreach (IWebElement row in TRmulti226)
                    {

                        TDmulti226 = row.FindElements(By.TagName("td"));
                        if (TDmulti226.Count !=0 && !row.Text.Contains("School"))
                        {
                            string multi1 = TDmulti226[0].Text + "~" + TDmulti226[1].Text + "~" + TDmulti226[2].Text + "~" + TDmulti226[3].Text + "~" + TDmulti226[4].Text;
                            gc.insert_date(orderNumber, parcel_no, 682, multi1, 1, DateTime.Now);

                        }

                    }
                    gc.CreatePdf(orderNumber, parcel_no, "Property details current", driver, "FL", "Charlotte");
                    driver.Close();
                    driver.SwitchTo().Window(current);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("https://charlotte.county-taxes.com/public");
                    Thread.Sleep(5000);
                    IWebElement IParcelClick = driver.FindElement(By.Id("search-text form-control"));
                    IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                    js.ExecuteScript("arguments[0].setAttribute('value', '" + parcel_no + "')", IParcelClick);
                    
                    gc.CreatePdf(orderNumber, parcel_no, "Taxinfo", driver, "FL", "Charlotte");
                    IWebElement IParcelNoClick = driver.FindElement(By.XPath("//*[@id='search-controls']/div/form[1]/div[1]/div/span/button"));
                    js.ExecuteScript("arguments[0].click();", IParcelNoClick);
                    Thread.Sleep(3000);
                    try
                    {
                        IWebElement ITaxSearch = driver.FindElement(By.XPath("//*[@id='results']"));
                        IList<IWebElement> ITaxSearchRow = ITaxSearch.FindElements(By.TagName("li"));
                        //IList<IWebElement> ITaxSearchTD;
                        foreach(IWebElement search in ITaxSearchRow)
                        {
                            IWebElement ITaxSearchTD = search.FindElement(By.TagName("a"));
                            string strTaxDetails = ITaxSearchTD.GetAttribute("innerHTML");
                            if (strTaxDetails.Contains("Full bill history"))
                            {
                                js.ExecuteScript("arguments[0].click();", ITaxSearchTD);
                            }
                        }
                        //IWebElement ITaxSearch = driver.FindElement(By.PartialLinkText("Full bill history"));
                        //js.ExecuteScript("arguments[0].click();", ITaxSearch);
                        //IWebElement ITaxSearch = driver.FindElement(By.LinkText("Full bill history"));
                        //string strITaxSearch = ITaxSearch.GetAttribute("href");
                        //driver.Navigate().GoToUrl(strITaxSearch);
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcel_no, "Full bill history", driver, "FL", "Charlotte");
                    }
                    catch { }

                    try
                    {
                        IWebElement ITaxReal = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table"));
                        IList<IWebElement> ITaxRealRow = ITaxReal.FindElements(By.TagName("tr"));
                        IList<IWebElement> ITaxRealTd;
                        int k = 0;
                        foreach (IWebElement ItaxReal in ITaxRealRow)
                        {
                            ITaxRealTd = ItaxReal.FindElements(By.TagName("td"));
                            if ((k <= 2 && ItaxReal.Text.Contains("Annual Bill")) || ItaxReal.Text.Contains("Pay this bill:"))
                            {
                                string yearbill = ITaxRealTd[0].Text;
                                IWebElement ITaxBillCount = ITaxRealTd[0].FindElement(By.TagName("a"));
                                string strTaxReal = ITaxBillCount.GetAttribute("href");
                                strTaxRealestate.Add(strTaxReal);
                                try
                                {
                                    IWebElement ITaxBill = ITaxRealTd[3].FindElement(By.TagName("a"));
                                    string BillTax = ITaxBill.GetAttribute("href");
                                    gc.downloadfile(BillTax, orderNumber, parcel_no, "Taxbill.pdf" + yearbill, "FL", "Charlotte");
                                }
                                catch
                                {
                                    IWebElement ITaxBill = ITaxRealTd[4].FindElement(By.TagName("a"));
                                    string BillTax = ITaxBill.GetAttribute("href");
                                    gc.downloadfile(BillTax, orderNumber, parcel_no, "Taxbill.pdf" + yearbill, "FL", "Charlotte");
                                }
                                k++;
                            }
                        }


                        //Tax History Details

                        IWebElement IBillHistorytable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table"));
                        IList<IWebElement> IBillHistoryRow = IBillHistorytable.FindElements(By.TagName("tr"));
                        IList<IWebElement> IBillHistoryTD;
                        foreach (IWebElement bill in IBillHistoryRow)
                        {
                            IBillHistoryTD = bill.FindElements(By.TagName("td"));
                            if (IBillHistoryTD.Count != 0)
                            {
                                try
                                {
                                    strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                    strBalance = IBillHistoryTD[1].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                    strBillDate = IBillHistoryTD[2].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                    strBillPaid = IBillHistoryTD[3].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                }
                                catch
                                {
                                    strBillDate = "";
                                    strBillPaid = "";
                                }
                                if (strBillPaid.Contains("Print (PDF)"))
                                {
                                    strBillPaid = "";
                                }
                                string strTaxHistory = strBill + "~" + strBalance + "~" + strBillDate + "~" + strBillPaid;
                                gc.insert_date(orderNumber, parcel_no, 685, strTaxHistory, 1, DateTime.Now);
                            }
                        }
                        IWebElement IBillHistoryfoottable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table/tfoot"));
                        IList<IWebElement> IBillHistoryfootRow = IBillHistoryfoottable.FindElements(By.TagName("tr"));
                        IList<IWebElement> IBillHistoryfootTD;
                        foreach (IWebElement bill in IBillHistoryRow)
                        {
                            IBillHistoryfootTD = bill.FindElements(By.TagName("th"));
                            if (IBillHistoryfootTD.Count != 0 && bill.Text.Contains("Total"))
                            {
                                try
                                {
                                    strFBill = IBillHistoryfootTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                    strFBalance = IBillHistoryfootTD[1].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                    strFBillDate = IBillHistoryfootTD[2].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                    strFBillPaid = IBillHistoryfootTD[3].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                }
                                catch
                                {
                                    strFBillDate = "";
                                    strFBillPaid = "";
                                }
                                if (strBillPaid.Contains("Print (PDF)"))
                                {
                                    strBillPaid = "";
                                }
                                string strTaxHistory = strFBill + "~" + strFBalance + "~" + strFBillDate + "~" + strFBillPaid;
                                gc.insert_date(orderNumber, parcel_no, 685, strTaxHistory, 1, DateTime.Now);
                            }
                        }

                    }
                    catch { }
                    foreach (string real in strTaxRealestate)
                    {
                        driver.Navigate().GoToUrl(real);
                        Thread.Sleep(4000);

                        try
                        {
                            TaxYear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[1]")).Text;
                            TaxYear = WebDriverTest.After(TaxYear, "Real Estate").Trim();
                            string s = TaxYear;
                            string[] words = TaxYear.Split(' ');
                            TaxYear = words[0];
                        }
                        catch
                        {
                            TaxYear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[1]")).Text;
                            TaxYear = WebDriverTest.After(TaxYear, "Real Estate").Trim();
                            string s = TaxYear;
                            string[] words = TaxYear.Split(' ');
                            TaxYear = words[0];
                        }

                        gc.CreatePdf(orderNumber, parcel_no, "Tax details" + TaxYear, driver, "FL", "Charlotte");
                        IWebElement multitableElement3;
                        try
                        {
                            multitableElement3 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[2]/tbody"));
                        }
                        catch
                        {
                            multitableElement3 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[2]/tbody"));
                        }
                        IList<IWebElement> multitableRow3 = multitableElement3.FindElements(By.TagName("tr"));

                        IList<IWebElement> multirowTD3;
                        foreach (IWebElement row in multitableRow3)
                        {
                            multirowTD3 = row.FindElements(By.TagName("td"));
                            if (multirowTD3.Count != 1 && multirowTD3[1].Text.Trim() != "")
                            {
                                string tax_distri = TaxYear + "~" + multirowTD3[0].Text.Trim() + "~" + "Ad Valorem" + "~" + multirowTD3[1].Text.Trim() + "~" + multirowTD3[2].Text.Trim() + "~" + multirowTD3[3].Text.Trim() + "~" + multirowTD3[4].Text.Trim() + "~" + "" + "~" + multirowTD3[5].Text.Trim();
                                gc.insert_date(orderNumber, parcel_no, 683, tax_distri, 1, DateTime.Now);
                            }
                        }

                        //total advalorem
                        IWebElement multitableElement31;
                        try
                        {
                            multitableElement31 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[2]/tfoot"));
                        }
                        catch
                        {

                            multitableElement31 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[2]/tfoot"));
                        }
                        IList<IWebElement> multitableRow31 = multitableElement31.FindElements(By.TagName("tr"));

                        IList<IWebElement> multirowTD31;
                        foreach (IWebElement row in multitableRow31)
                        {
                            multirowTD31 = row.FindElements(By.TagName("td"));
                            if (multirowTD31.Count != 1)
                            {
                                string tax_distri1 = TaxYear + "~" + "Total:" + "~" + "Ad Valorem" + "~" + multirowTD31[0].Text.Trim() + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + multirowTD31[1].Text.Trim();
                                gc.insert_date(orderNumber, parcel_no, 683, tax_distri1, 1, DateTime.Now);
                            }
                        }
                        //  Non - Ad Valorem                    
                        try
                        {
                            IWebElement multitableElement32;
                            try
                            {
                                multitableElement32 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[3]/tbody"));
                            }
                            catch
                            {
                                multitableElement32 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[3]/tbody"));

                            }
                            IList<IWebElement> multitableRow32 = multitableElement32.FindElements(By.TagName("tr"));
                            IList<IWebElement> multirowTD32;
                            foreach (IWebElement row in multitableRow32)
                            {
                                multirowTD32 = row.FindElements(By.TagName("td"));
                                if (multirowTD32.Count != 1 && multirowTD32[0].Text.Trim() != "")
                                {
                                    string tax_distri2 = TaxYear + "~" + multirowTD32[0].Text.Trim() + "~" + "Non-Ad Valorem " + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + multirowTD32[1].Text.Trim() + "~" + multirowTD32[2].Text.Trim();
                                    gc.insert_date(orderNumber, parcel_no, 683, tax_distri2, 1, DateTime.Now);
                                }
                            }
                            //total non-advalorem

                            IWebElement multitableElement33;
                            try
                            {

                                multitableElement33 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[3]/tfoot"));
                            }
                            catch
                            {
                                multitableElement33 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[3]/tfoot"));
                            }
                            IList<IWebElement> multitableRow33 = multitableElement33.FindElements(By.TagName("tr"));
                            IList<IWebElement> multirowTD33;
                            foreach (IWebElement row in multitableRow33)
                            {
                                multirowTD33 = row.FindElements(By.TagName("td"));
                                if (multirowTD33.Count != 0)
                                {
                                    string tax_distri1 = TaxYear + "~" + "Total:" + "~" + "Non-Ad Valorem " + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + multirowTD33[0].Text.Trim();
                                    gc.insert_date(orderNumber, parcel_no, 683, tax_distri1, 1, DateTime.Now);
                                }
                            }
                        }
                        catch { }
                        try
                        {
                            TaxAmount = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/p")).Text.Replace("Combined taxes and assessments:", "");
                        }
                        catch { }
                        string IfPaidBy = "", PlesePay = "", DueDate = "", deli = "";
                        try
                        {

                            IWebElement multitableElement26 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]/tbody"));
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
                                        if (Convert.ToInt16(IfpaySplit.Count()) > 1)
                                        {
                                            IfPaidBy = IfpaySplit[0];
                                            PlesePay = IfpaySplit[1];
                                            if (PlesePay == "$0.00")
                                                deli = "paid";
                                            else
                                                deli = "deliquent";
                                            DueDate = TaxYear + "~" + deli + "~" + TaxAmount + "~" + IfPaidBy + "~" + PlesePay;
                                            gc.insert_date(orderNumber, parcel_no, 686, DueDate, 1, DateTime.Now);
                                        }
                                    }

                                }
                            }
                        }
                        //If_paid_by~Please_Pay
                        catch
                        {
                        }
                        try
                        {
                            IWebElement multitableElement26 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]/tbody"));
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
                                        string[] IfpaySplit = IfPaidBy.Split('~');
                                        if (Convert.ToInt16(IfpaySplit.Count()) > 1)
                                        {
                                            IfPaidBy = IfpaySplit[0];
                                            PlesePay = IfpaySplit[1];
                                            if (PlesePay == "$0.00")
                                                deli = "paid";
                                            else
                                                deli = "deliquent";
                                            DueDate = TaxYear + "~" + deli + "~" + TaxAmount + "~" + IfPaidBy + "~" + PlesePay;
                                            gc.insert_date(orderNumber, parcel_no, 686, DueDate, 1, DateTime.Now);
                                        }
                                    }

                                }


                            }
                        }
                        catch { }
                        //*[@id="content"]/div[1]/div[3]/div[1]/ul/li[1]/a
                        try
                        {
                            //driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[3]/div[1]/ul/li[1]/a")).Click();
                            //Thread.Sleep(5000);
                            IWebElement IBasicDetails = driver.FindElement(By.XPath("//*[@id='content']"));
                            IList<IWebElement> IBasicDetailsRow = IBasicDetails.FindElements(By.TagName("li"));
                            IWebElement IBasicDetailsTD = null;
                            foreach(IWebElement basic in IBasicDetailsRow)
                            {
                                string strClick = basic.GetAttribute("innerHTML");
                                if(strClick.Contains("Parcel details"))
                                {
                                    IBasicDetailsTD = basic.FindElement(By.TagName("img"));
                                    js.ExecuteScript("arguments[0].click();", IBasicDetailsTD);
                                    Thread.Sleep(5000);
                                }
                            }
                        }
                        catch { }
                        gc.CreatePdf(orderNumber, parcel_no, "parcel details" + TaxYear, driver, "FL", "Charlotte");
                        string charitable = "";
                        string ownertax = "", situs = "", alternate_key = "", legal = "", total_aces = "", AssessedValue = "", School_AssessedValue = "", homestead_exemption = "", homestead_school = "", additional_homestead = "", advalorem = "", nonadvalorem = "", total_discount = "", noDiscount = "", total_tax = "", paiddate = "";
                        string taxinfodetails1 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[3]/div[2]")).Text.Replace("\r\n", "");

                        ownertax = gc.Between(taxinfodetails1, "Owner", "Situs").Trim();
                        situs = gc.Between(taxinfodetails1, "Situs", "Account number").Trim();
                        Account_number = gc.Between(taxinfodetails1, "Account number", "Alternate Key").Trim();
                        alternate_key = gc.Between(taxinfodetails1, "Alternate Key", "Millage code").Trim();
                        Millage_Code = gc.Between(taxinfodetails1, "Millage code", "Millage rate").Trim();
                        try
                        {
                            Millage_rate = gc.Between(taxinfodetails1, "Millage rate", "Escrow").Trim();
                        }
                        catch
                        {
                            Millage_rate = gc.Between(taxinfodetails1, "Millage rate", "Assessed value").Trim();
                        }
                        try
                        {
                            AssessedValue = gc.Between(taxinfodetails1, "Assessed value", "School assessed value").Trim();
                        }
                        catch { }
                        try
                        {
                            School_AssessedValue = gc.Between(taxinfodetails1, "School assessed value", "Flags").Trim();
                        }
                        catch
                        {

                        }
                        try
                        {
                            School_AssessedValue = gc.Between(taxinfodetails1, "School assessed value", "Exemptions").Trim();
                        }
                        catch
                        {
                            School_AssessedValue = GlobalClass.After(taxinfodetails1, "School assessed value").Trim();
                        }
                        try
                        {
                            homestead_exemption = gc.Between(taxinfodetails1, "HOMESTEAD EXEMPTION", "HOMESTEAD SCHOOL").Trim();
                            homestead_school = gc.Between(taxinfodetails1, "HOMESTEAD SCHOOL", "ADDITIONAL HOMESTEAD").Trim();
                            additional_homestead = GlobalClass.After(taxinfodetails1, "ADDITIONAL HOMESTEAD").Trim();
                        }
                        catch { }

                        try
                        {
                            charitable = GlobalClass.After(taxinfodetails1, "CHARITABLE").Trim();
                        }
                        catch { }
                        string taxinfodetails2 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[4]")).Text.Replace("\r\n", "");
                        advalorem = gc.Between(taxinfodetails2, "Ad valorem", "Non-ad valorem").Trim();
                        nonadvalorem = gc.Between(taxinfodetails2, "Non-ad valorem", "Total Discountable").Trim();
                        total_discount = gc.Between(taxinfodetails2, "Total Discountable", "No Discount").Trim();
                        noDiscount = gc.Between(taxinfodetails2, "No Discount NAVA", "Total tax").Trim();
                        total_tax = gc.Between(taxinfodetails2, "Total tax", "Legal description").Trim();
                        legal = gc.Between(taxinfodetails2, "Legal description", "Location").Trim();
                        total_aces = GlobalClass.After(taxinfodetails2, "Total acres").Trim();
                        string taxinfodetails3 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[3]/div[1]")).Text;
                        if (taxinfodetails3.Contains("Pay this bill"))
                        {
                            PaidAmount = taxinfodetails3.Replace("Pay this bill:", "&");
                            var PaidAmount1 = PaidAmount.Split('&');
                            PaidAmount = PaidAmount1[1];
                            try
                            {
                                paiddate = gc.Between(PaidAmount, "PAID", "$").Trim();
                            }
                            catch
                            {
                                paiddate = "";
                            }
                            PaidAmount = WebDriverTest.After(PaidAmount, "$").Trim();
                            try
                            {
                                ReceiptNumber = PaidAmount1[2].Replace("Receipt", "");
                            }
                            catch { }
                        }
                        else
                        {
                            PaidAmount = "";
                            ReceiptNumber = "";
                            paiddate = "";
                        }
                        //         Owner Information~Situs Address~Tax Year~Account number~Alternate Key~Millage code~Millage rate~Legal description~Total acres~Assessed value~School assessed value~HOMESTEAD EXEMPTION~HOMESTEAD SCHOOL~ADDITIONAL HOMESTEAD~Ad valorem~Non - ad valorem~Total Discountable~No Discount NAVA~Total tax~PaidDate~Paid Amount~Receipt #~Tax Authority

                        string tax_info1 = ownertax + "~" + situs + "~" + TaxYear + "~" + Account_number + "~" + alternate_key + "~" + Millage_Code + "~" + Millage_rate + "~" + legal + "~" + total_aces + "~" + AssessedValue + "~" + School_AssessedValue + "~" + homestead_exemption + "~" + homestead_school + "~" + additional_homestead + "~" + charitable + "~" + advalorem + "~" + nonadvalorem + "~" + total_discount + "~" + noDiscount + "~" + total_tax + "~" + paiddate + "~" + PaidAmount + "~" + ReceiptNumber + "~" + "Charlotte County Tax Collector 18500 Murdock Circle Port Charlotte, FL 33948 Phone: (941)743-1350 -or- (941)681-3710";
                        gc.insert_date(orderNumber, parcel_no, 684, tax_info1, 1, DateTime.Now);


                    }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "FL", "Charlotte", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "FL", "Charlotte");
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