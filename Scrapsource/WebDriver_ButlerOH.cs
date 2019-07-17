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
    public class WebDriver_ButlerOH
    {

        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();

        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_ButlerOH(string houseno, string Direction, string sname, string stype, string unitNumber, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string address = "", lastName = "", firstName = "", Pinnumber = "", PropertyAdd = "", Strownername = "", Address = "";
            List<string> MailURL = new List<string>();
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
                   using (driver = new PhantomJSDriver())
            //     using (driver = new ChromeDriver())
            {

                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    string taxAuth = "", taxauth1 = "", taxauth2 = "", taxauth3 = "", taxauth4 = "";
                    try
                    {
                        driver.Navigate().GoToUrl("http://www.butlercountytreasurer.org/topic/index.php?topicid=143&structureid=1");
                        taxauth1 = driver.FindElement(By.XPath("//*[@id='descContain']/p[1]")).Text.Replace("Address:", "").Trim();
                        taxauth2 = driver.FindElement(By.XPath("//*[@id='descContain']/p[2]")).Text.Trim();
                        taxauth3 = driver.FindElement(By.XPath("//*[@id='descContain']/p[3]")).Text.Trim();
                        taxauth4 = driver.FindElement(By.XPath("//*[@id='descContain']/p[4]")).Text.Trim();
                        taxAuth = taxauth1 + " " + taxauth2 + " " + taxauth3 + " " + taxauth4;

                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Authority", driver, "OH", "Butler");
                    }
                    catch { }

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
                        gc.TitleFlexSearch(orderNumber, "", ownername, "", "OH", "Butler");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Zero_ButlerOH"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("http://propertysearch.butlercountyohio.org/PT/forms/htmlframe.aspx?mode=content/home.htm");
                        Thread.Sleep(4000);

                        driver.FindElement(By.XPath("//*[@id='content']/div/table/tbody/tr/td/font[2]/font[2]/font/font/a/span")).Click();
                        Thread.Sleep(2000);

                        Address = houseno + " " + sname;
                        try
                        {
                            driver.FindElement(By.Id("inpNumber")).SendKeys(houseno);
                            driver.FindElement(By.Id("inpStreet")).SendKeys(sname);
                            gc.CreatePdf_WOP(orderNumber, "Address search", driver, "OH", "Butler");
                        }
                        catch { }

                        driver.FindElement(By.Id("btSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Address search Result", driver, "OH", "Butler");

                        try
                        {
                            int Max = 0;
                            string Roll = "", TransferDate = "", Price = "";
                            string Record = "";
                            IWebElement multiaddress;

                            multiaddress = driver.FindElement(By.Id("searchResults"));

                            // gc.CreatePdf(orderNumber, parcelNumber, "Multi Address search Result", driver, "OH", "Butler");
                            IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> multiTD;
                            foreach (IWebElement multi in multiRow)
                            {
                                multiTD = multi.FindElements(By.TagName("td"));
                                if (multi.Text.Contains(Address.ToUpper()) && multiTD.Count != 0 && multiRow.Count >= 1 && multiRow.Count <= 25 && multi.Text.Trim() != "" && !multi.Text.Contains("Address"))
                                {
                                    Strownername = multiTD[1].Text;

                                    parcelNumber = multiTD[0].Text;
                                    PropertyAdd = multiTD[2].Text;
                                    Roll = multiTD[3].Text;
                                    TransferDate = multiTD[4].Text;
                                    Price = multiTD[5].Text;
                                    string multidetails = Strownername + "~" + PropertyAdd + "~" + Roll + "~" + TransferDate + "~" + Price;
                                    gc.insert_date(orderNumber, parcelNumber, 1829, multidetails, 1, DateTime.Now);
                                    Max++;
                                }
                                if (multiTD.Count != 0 && multiRow.Count > 25)
                                {
                                    HttpContext.Current.Session["multiparcel_ButlerOH_Maximum"] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }

                            }
                            if (Max > 1 && Max < 26)
                            {
                                HttpContext.Current.Session["multiparcel_ButlerOH"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }

                            if (Max == 0)
                            {
                                HttpContext.Current.Session["Zero_ButlerOH"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                            // }
                        }
                        catch { }
                    }



                    if (searchType == "ownername")
                    {

                        driver.Navigate().GoToUrl("http://propertysearch.butlercountyohio.org/PT/search/commonsearch.aspx?mode=owner");
                        Thread.Sleep(4000);

                        driver.FindElement(By.Id("inpOwner")).SendKeys(ownername);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "OwnerName search", driver, "OH", "Butler");
                        try
                        {
                            driver.FindElement(By.Id("btSearch")).SendKeys(Keys.Enter);
                            Thread.Sleep(3000);
                            gc.CreatePdf(orderNumber, parcelNumber, "OwnerName search Result", driver, "OH", "Butler");
                        }
                        catch { }

                        try
                        {
                            int Max = 0;
                            string Roll = "", TransferDate = "", Price = "";
                            string Record = "";
                            IWebElement multiaddress;

                            multiaddress = driver.FindElement(By.Id("searchResults"));
                            // gc.CreatePdf(orderNumber, parcelNumber, "OwnerName Multi search Result", driver, "OH", "Butler");
                            IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> multiTD;
                            foreach (IWebElement multi in multiRow)
                            {
                                multiTD = multi.FindElements(By.TagName("td"));
                                if (multiTD.Count != 0 && multiRow.Count >= 1 && multiRow.Count <= 25 && multi.Text.Trim() != "" && !multi.Text.Contains("Address"))
                                {
                                    Strownername = multiTD[1].Text;

                                    parcelNumber = multiTD[0].Text;
                                    PropertyAdd = multiTD[2].Text;
                                    Roll = multiTD[3].Text;
                                    TransferDate = multiTD[4].Text;
                                    Price = multiTD[5].Text;
                                    string multidetails = Strownername + "~" + PropertyAdd + "~" + Roll + "~" + TransferDate + "~" + Price;
                                    gc.insert_date(orderNumber, parcelNumber, 1829, multidetails, 1, DateTime.Now);
                                    Max++;
                                }
                                if (multiTD.Count != 0 && multiRow.Count > 25)
                                {
                                    HttpContext.Current.Session["multiparcel_ButlerOH_Maximum"] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }

                            }
                            if (Max > 1 && Max < 26)
                            {
                                HttpContext.Current.Session["multiparcel_ButlerOH"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }

                            if (Max == 0)
                            {
                                HttpContext.Current.Session["Zero_ButlerOH"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                            // }
                        }
                        catch { }
                    }

                    else if (searchType == "parcel")
                    {

                        driver.Navigate().GoToUrl("http://propertysearch.butlercountyohio.org/PT/search/commonsearch.aspx?mode=parid");
                        Thread.Sleep(4000);

                        driver.FindElement(By.Id("inpParid")).SendKeys(parcelNumber);
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search", driver, "OH", "Butler");
                        try
                        {
                            driver.FindElement(By.Id("btSearch")).SendKeys(Keys.Enter);
                            Thread.Sleep(3000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Parcel search Result", driver, "OH", "Butler");
                        }
                        catch { }

                        try
                        {
                            int Max = 0;
                            string Roll = "", TransferDate = "", Price = "";
                            string Record = "";
                            IWebElement multiaddress;

                            multiaddress = driver.FindElement(By.Id("searchResults"));

                            IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> multiTD;
                            foreach (IWebElement multi in multiRow)
                            {
                                multiTD = multi.FindElements(By.TagName("td"));
                                if (multiTD.Count != 0 && multiRow.Count >= 1 && (multiTD[0].Text.Trim() == parcelNumber) && multiRow.Count <= 25 && multi.Text.Trim() != "" && !multi.Text.Contains("Address"))
                                {
                                    IWebElement IRealEstate = multiTD[0].FindElement(By.TagName("a"));
                                    IRealEstate.Click();
                                    Thread.Sleep(2000);
                                    Max++;
                                    break;
                                }
                                if (multiTD.Count != 0 && multiRow.Count >= 1 && multiRow.Count <= 25 && multi.Text.Trim() != "" && !multi.Text.Contains("Address"))
                                {
                                    Strownername = multiTD[1].Text;

                                    parcelNumber = multiTD[0].Text;
                                    PropertyAdd = multiTD[2].Text;
                                    Roll = multiTD[3].Text;
                                    TransferDate = multiTD[4].Text;
                                    Price = multiTD[5].Text;
                                    string multidetails = Strownername + "~" + PropertyAdd + "~" + Roll + "~" + TransferDate + "~" + Price;
                                    gc.insert_date(orderNumber, parcelNumber, 1829, multidetails, 1, DateTime.Now);
                                    Max++;
                                }
                                if (multiTD.Count != 0 && multiRow.Count > 25)
                                {
                                    HttpContext.Current.Session["multiparcel_ButlerOH_Maximum"] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }

                            }
                            if (Max > 1 && Max < 26)
                            {
                                HttpContext.Current.Session["multiparcel_ButlerOH"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }

                            if (Max == 0)
                            {
                                HttpContext.Current.Session["Zero_ButlerOH"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                            // }
                        }
                        catch { }
                    }

                    //property details

                    string PropertyAddress = "", OwnerName = "", MailingAddress = "", Class = "", LandUseCode = "", Neighborhood = "", TotalAcres = "", TaxingDistrict = "";
                    string DistrictName = "", GrossTaxRate = "", EffectiveTaxRate = "", NonBusinessCredit = "", OwnerOccupiedCredit = "", YearBuilt = "", LegalDesc = "";

                    string bulkdata = driver.FindElement(By.XPath("//*[@id='Parcel']/tbody")).Text;
                    parcelNumber = gc.Between(bulkdata, "Parcel Id", "Address").Replace(":", "").Replace("-", "").Trim();
                    string bulkdata2 = driver.FindElement(By.XPath("//*[@id='datalet_div_8']")).Text;
                    string bulkdata3 = driver.FindElement(By.XPath("//*[@id='datalet_div_9']")).Text;

                    PropertyAddress = gc.Between(bulkdata, "Address", "Class").Replace(":", "").Trim();
                    OwnerName = gc.Between(bulkdata2, "Owner 1", "Legal 1").Replace("Owner 2", "").Trim();
                    MailingAddress = GlobalClass.After(bulkdata3, "Address 1").Replace("Address 2", "").Replace("Address 3", "").Replace("\r\n", "").Trim();
                    Class = gc.Between(bulkdata, "Class", "Land Use Code").Replace(":", "").Trim();
                    LandUseCode = gc.Between(bulkdata, "Land Use Code", "Neighborhood").Replace(":", "").Trim();
                    Neighborhood = gc.Between(bulkdata, "Neighborhood", "Total Acres").Replace(":", "").Trim();
                    TotalAcres = gc.Between(bulkdata, "Total Acres", "Taxing District").Replace(":", "").Trim();
                    TaxingDistrict = gc.Between(bulkdata, "Taxing District", "District Name").Replace(":", "").Trim();
                    DistrictName = gc.Between(bulkdata, "District Name", "Gross Tax Rate").Replace(":", "").Trim();
                    GrossTaxRate = gc.Between(bulkdata, "Gross Tax Rate", "Effective Tax Rate").Replace(":", "").Trim();
                    EffectiveTaxRate = gc.Between(bulkdata, "Effective Tax Rate", "Non Business Credit").Replace(":", "").Trim();
                    NonBusinessCredit = gc.Between(bulkdata, "Non Business Credit", "Owner Occupied Credit").Replace(":", "").Trim();
                    OwnerOccupiedCredit = GlobalClass.After(bulkdata, "Owner Occupied Credit").Replace(":", "").Trim();
                    try
                    {
                        LegalDesc = gc.Between(bulkdata2, "Legal 1", "Future  ").Replace("Legal 2", "").Replace("Legal 3", "").Trim();
                    }
                    catch { }
                    try
                    {

                        YearBuilt = driver.FindElement(By.XPath("//*[@id='Dwelling']/tbody/tr[6]/td[2]")).Text;
                    }
                    catch { }

                    string propertydetails = PropertyAddress + "~" + OwnerName + "~" + MailingAddress + "~" + Class + "~" + LandUseCode + "~" + Neighborhood + "~" + TotalAcres + "~" + TaxingDistrict + "~" + DistrictName + "~" + GrossTaxRate + "~" + EffectiveTaxRate + "~" + NonBusinessCredit + "~" + OwnerOccupiedCredit + "~" + YearBuilt + "~" + LegalDesc;
                    gc.insert_date(orderNumber, parcelNumber, 1830, propertydetails, 1, DateTime.Now);

                    //gc.CreatePdf(orderNumber, parcelNumber, "Property Details", driver, "OH", "Butler");

                    // Assessment Details

                    string Land = "", Building = "", TotalValue = "", CAUV = "", AssessedTaxYear = "", Land2 = "", Building2 = "", AssesedTotal = "";

                    string Assessdata = driver.FindElement(By.XPath("//*[@id='Current Value']/tbody")).Text;

                    Land = gc.Between(Assessdata, "Land (100%)", "Building (100%)").Trim();
                    Building = gc.Between(Assessdata, "Building (100%)", "Total Value (100%)").Trim();
                    TotalValue = gc.Between(Assessdata, "Total Value (100%)", "CAUV").Trim();
                    CAUV = gc.Between(Assessdata, "CAUV", "Assessed Tax Year").Trim();
                    AssessedTaxYear = gc.Between(Assessdata, "Assessed Tax Year", "Land (35%)").Trim();
                    Land2 = gc.Between(Assessdata, "Land (35%)", "Building (35%)").Trim();
                    Building2 = gc.Between(Assessdata, "Building (35%)", "Assessed Total (35%)").Trim();
                    AssesedTotal = GlobalClass.After(Assessdata, "Assessed Total (35%)").Trim();

                    string Assessmentdetails = Land + "~" + Building + "~" + TotalValue + "~" + CAUV + "~" + AssessedTaxYear + "~" + Land2 + "~" + Building2 + "~" + AssesedTotal;
                    gc.insert_date(orderNumber, parcelNumber, 1831, Assessmentdetails, 1, DateTime.Now);

                    // Excemption Details

                    string excempdata = driver.FindElement(By.Id("datalet_div_5")).Text;
                    string HomesteadExemption = "", OwnerOccupied = "", VeteranExemption = "";

                    HomesteadExemption = gc.Between(excempdata, "Homestead Exemption", "Owner Occupied Credit").Trim();
                    OwnerOccupied = gc.Between(excempdata, "Owner Occupied Credit", "100% Disabled Veteran Exemption").Trim();
                    VeteranExemption = GlobalClass.After(excempdata, "100% Disabled Veteran Exemption").Trim();

                    string Excemptiondetails = HomesteadExemption + "~" + OwnerOccupied + "~" + VeteranExemption;
                    gc.insert_date(orderNumber, parcelNumber, 1832, Excemptiondetails, 1, DateTime.Now);

                    // Tax Information Details

                    try
                    {
                        IWebElement TaxInformation = driver.FindElement(By.Id("datalet_div_7"));
                        IList<IWebElement> TRTaxInformation = TaxInformation.FindElements(By.TagName("tr"));
                        IList<IWebElement> THTaxInformation = TaxInformation.FindElements(By.TagName("th"));
                        IList<IWebElement> TDTaxInformation;
                        foreach (IWebElement row in TRTaxInformation)
                        {
                            TDTaxInformation = row.FindElements(By.TagName("td"));
                            if (TDTaxInformation.Count != 0 && !row.Text.Contains("Current Year") && !row.Text.Contains("Prior Year") && row.Text.Trim() != "")
                            {
                                string TaxBodydetails = TDTaxInformation[0].Text + "~" + TDTaxInformation[1].Text + "~" + TDTaxInformation[2].Text + "~" + TDTaxInformation[3].Text + "~" + TDTaxInformation[4].Text + "~" + taxAuth;
                                gc.insert_date(orderNumber, parcelNumber, 1833, TaxBodydetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='sidemenu']/li[10]/a")).Click();
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Distribution Details", driver, "OH", "Butler");
                    }
                    catch { }

                    // Current Year Charges Table

                    //try
                    //{
                    //    IWebElement Currentcharges = driver.FindElement(By.Id("datalet_div_7"));
                    //    IList<IWebElement> TRCurrentcharges = Currentcharges.FindElements(By.TagName("tr"));
                    //    IList<IWebElement> THCurrentcharges = Currentcharges.FindElements(By.TagName("th"));
                    //    IList<IWebElement> TDCurrentcharges;
                    //    foreach (IWebElement row in TRCurrentcharges)
                    //    {
                    //        TDCurrentcharges = row.FindElements(By.TagName("td"));
                    //        if (TDCurrentcharges.Count != 0 && !row.Text.Contains("Current Year") && !row.Text.Contains("Prior Year") && row.Text.Trim() != "")
                    //        {
                    //            string TaxBodydetails = TDCurrentcharges[0].Text + "~" + TDCurrentcharges[1].Text + "~" + TDCurrentcharges[2].Text + "~" + TDCurrentcharges[3].Text;
                    //            gc.insert_date(orderNumber, parcelNumber, 1834, TaxBodydetails, 1, DateTime.Now);
                    //        }
                    //    }
                    //}
                    //catch { }

                    // Distribution Details Table
                    string strdistribution = "";
                    try
                    {
                        strdistribution = driver.FindElement(By.XPath("//*[@id='datalet_div_2']/table[1]/tbody/tr/td")).Text;
                        IWebElement TaxDistribution = driver.FindElement(By.Id("Tax Distribution"));
                        IList<IWebElement> TRTaxDistribution = TaxDistribution.FindElements(By.TagName("tr"));
                        IList<IWebElement> THTaxDistribution = TaxDistribution.FindElements(By.TagName("th"));
                        IList<IWebElement> TDTaxDistribution;
                        foreach (IWebElement row in TRTaxDistribution)
                        {
                            TDTaxDistribution = row.FindElements(By.TagName("td"));
                            if (TDTaxDistribution.Count != 0 && !row.Text.Contains("TAXING AUTHORITY") && row.Text.Trim() != "")
                            {
                                string TaxDistributiondetails = strdistribution + "~" + TDTaxDistribution[0].Text + "~" + TDTaxDistribution[1].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1835, TaxDistributiondetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    // Distribution Details Table 2
                    string strdistribution2 = "";
                    try
                    {
                        strdistribution2 = driver.FindElement(By.XPath("//*[@id='datalet_div_3']/table[1]/tbody/tr/td")).Text;
                        IWebElement TaxDistribution2 = driver.FindElement(By.Id("Tax Distribution - County Portion Only"));
                        IList<IWebElement> TRTaxDistribution2 = TaxDistribution2.FindElements(By.TagName("tr"));
                        IList<IWebElement> THTaxDistribution2 = TaxDistribution2.FindElements(By.TagName("th"));
                        IList<IWebElement> TDTaxDistribution2;
                        foreach (IWebElement row in TRTaxDistribution2)
                        {
                            TDTaxDistribution2 = row.FindElements(By.TagName("td"));
                            if (TDTaxDistribution2.Count != 0 && !row.Text.Contains("TAXING AUTHORITY") && row.Text.Trim() != "")
                            {
                                string TaxBodydetails = strdistribution2 + "~" + TDTaxDistribution2[0].Text + "~" + TDTaxDistribution2[1].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1835, TaxBodydetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    // Tax Histroy Details 1
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='sidemenu']/li[11]/a")).Click();
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax History Details1", driver, "OH", "Butler");
                    }
                    catch { }


                    try
                    {

                        IWebElement TaxHistroy1 = driver.FindElement(By.XPath("//*[@id='datalet_div_0']/table[2]/tbody"));
                        IList<IWebElement> TRTaxHistroy1 = TaxHistroy1.FindElements(By.TagName("tr"));
                        IList<IWebElement> THTaxHistroy1 = TaxHistroy1.FindElements(By.TagName("th"));
                        IList<IWebElement> TDTaxHistroy1;
                        foreach (IWebElement row in TRTaxHistroy1)
                        {
                            TDTaxHistroy1 = row.FindElements(By.TagName("td"));
                            if (TDTaxHistroy1.Count != 0 && !row.Text.Contains("Prior Year") && row.Text.Trim() != "")
                            {
                                string TaxHistorydetails = TDTaxHistroy1[0].Text + "~" + TDTaxHistroy1[1].Text + "~" + TDTaxHistroy1[2].Text + "~" + TDTaxHistroy1[3].Text + "~" + TDTaxHistroy1[4].Text + "~" + TDTaxHistroy1[5].Text + "~" + TDTaxHistroy1[6].Text + "~" + TDTaxHistroy1[7].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1836, TaxHistorydetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    // Tax Histroy Details 2
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='sidemenu']/li[12]/a")).Click();
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax History Details2", driver, "OH", "Butler");
                    }
                    catch { }


                    try
                    {

                        IWebElement TaxHistroy1 = driver.FindElement(By.XPath("//*[@id='datalet_div_0']/table[2]/tbody"));
                        IList<IWebElement> TRTaxHistroy1 = TaxHistroy1.FindElements(By.TagName("tr"));
                        IList<IWebElement> THTaxHistroy1 = TaxHistroy1.FindElements(By.TagName("th"));
                        IList<IWebElement> TDTaxHistroy1;
                        foreach (IWebElement row in TRTaxHistroy1)
                        {
                            TDTaxHistroy1 = row.FindElements(By.TagName("td"));
                            if (TDTaxHistroy1.Count != 0 && !row.Text.Contains("Prior Year") && row.Text.Trim() != "")
                            {
                                string TaxHistorydetails = TDTaxHistroy1[0].Text + "~" + TDTaxHistroy1[1].Text + "~" + TDTaxHistroy1[2].Text + "~" + TDTaxHistroy1[3].Text + "~" + TDTaxHistroy1[4].Text + "~" + TDTaxHistroy1[5].Text + "~" + TDTaxHistroy1[6].Text + "~" + TDTaxHistroy1[7].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1836, TaxHistorydetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    // Tax Histroy Details 3

                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='sidemenu']/li[13]/a")).Click();
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax History Details3", driver, "OH", "Butler");
                    }
                    catch { }


                    try
                    {

                        IWebElement TaxHistroy1 = driver.FindElement(By.XPath("//*[@id='datalet_div_0']/table[2]/tbody"));
                        IList<IWebElement> TRTaxHistroy1 = TaxHistroy1.FindElements(By.TagName("tr"));
                        IList<IWebElement> THTaxHistroy1 = TaxHistroy1.FindElements(By.TagName("th"));
                        IList<IWebElement> TDTaxHistroy1;
                        foreach (IWebElement row in TRTaxHistroy1)
                        {
                            TDTaxHistroy1 = row.FindElements(By.TagName("td"));
                            if (TDTaxHistroy1.Count != 0 && !row.Text.Contains("Prior Year") && row.Text.Trim() != "")
                            {
                                string TaxHistorydetails = TDTaxHistroy1[0].Text + "~" + TDTaxHistroy1[1].Text + "~" + TDTaxHistroy1[2].Text + "~" + TDTaxHistroy1[3].Text + "~" + TDTaxHistroy1[4].Text + "~" + TDTaxHistroy1[5].Text + "~" + TDTaxHistroy1[6].Text + "~" + TDTaxHistroy1[7].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1836, TaxHistorydetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }


                    // special Assessment Details

                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='sidemenu']/li[28]/a")).Click();
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Special Assessment Details", driver, "OH", "Butler");
                    }
                    catch { }


                    try
                    {

                        IWebElement SplAssessment = driver.FindElement(By.Id("datalet_div_0"));
                        IList<IWebElement> TRSplAssessment = SplAssessment.FindElements(By.TagName("tr"));
                        IList<IWebElement> THSplAssessment = SplAssessment.FindElements(By.TagName("th"));
                        IList<IWebElement> TDSplAssessment;
                        foreach (IWebElement row in TRSplAssessment)
                        {
                            TDSplAssessment = row.FindElements(By.TagName("td"));
                            if (TDSplAssessment.Count != 0 && !row.Text.Contains("Special Assessments") && !row.Text.Contains("Project Number") && row.Text.Trim() != "")
                            {
                                string SplAssessmentdetails = TDSplAssessment[0].Text + "~" + TDSplAssessment[1].Text + "~" + TDSplAssessment[2].Text + "~" + TDSplAssessment[3].Text + "~" + TDSplAssessment[4].Text + "~" + TDSplAssessment[5].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1837, SplAssessmentdetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    // Tax Bill


                    //var chromeOptions = new ChromeOptions();

                    //var downloadDirectory = "F:\\AutoPdf\\";

                    //chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                    //chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                    //chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                    //chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                    //var driver1 = new ChromeDriver(chromeOptions);
                    driver.Navigate().GoToUrl("https://www.butlercountytreasurer.org/egov/apps/bill/pay.egov?view=search&itemid=143");
                    string fileName = "TaxBill.pdf";
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='prop']")).Click();
                        Thread.Sleep(3000);
                    }
                    catch { }
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='clientSearchForm']/div[2]/label[1]/input")).Click();
                        Thread.Sleep(3000);
                    }
                    catch { }
                    driver.FindElement(By.Id("ebillSearch_accountNumFld")).SendKeys(parcelNumber);
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Bill Search", driver, "OH", "Butler");
                    driver.FindElement(By.XPath("//*[@id='eGov_buttons']/input")).Click();
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Bill Search View", driver, "OH", "Butler");


                    IWebElement assessme = driver.FindElement(By.XPath("//*[@id='eGovManager']/table[2]/tbody/tr/td[1]/a"));
                    string download = assessme.GetAttribute("href");
                    driver.Navigate().GoToUrl(download);
                    Thread.Sleep(4000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Bill", driver, "OH", "Butler");
                    //try
                    //{
                    //    gc.downloadfile(download, orderNumber, parcelNumber, "TaxBill Download", "OH", "Butler");
                    //}
                    //catch { driver.Quit(); }

                    //try
                    //{
                    //    gc.AutoDownloadFile(orderNumber, parcelNumber, "Butler", "OH", fileName);
                    //}
                    //catch (Exception ex) { }






                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "OH", "Butler", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "OH", "Butler");
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