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
using System.Web.UI;

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_TXTravis
    {
        string fulltext = "", PropertyID = "", GeographicID = "", Name = "", MailingAddress = "", OwnerID = "", LegalDescription = "", Neighborhood = "", Type = "", ProAddress = "", NeighborhoodCD = "", Exemptions = "", MapID = "", YearBuilt = "", Acres = "", property_details = "";
        string Address = "", ChkMulti = "", Parcelno = "", Multi_Type = "", Property_Address = "", Owner = "", MultiAddress_details = "", ValueDetails = "", rollDetails = "";
        string owner = "", Ownership = "", TotalValue = "", ValueDetails1 = "", fulltext1 = "", Receipt = "", Receipt_Taxyear = "", Receipt_PaymentDate = "", Receipt_PaymentAmount = "", TaxPayment_Details = "";
        string Tax_Unit = "", Assed_Value = "", NetTaxb_Val = "", Basedue_Val = "", Penalty_Val = "", Attrney_Val = "", Total_Val = "", TaxUnit_Details = "", Tax_Unit1 = "", Assed_Value1 = "", NetTaxb_Val1 = "", Basedue_Val1 = "", Penalty_Val1 = "", Attrney_Val1 = "", Total_Val1 = "", TaxUnit_Details1 = "";
        string Account = "", Tax_Owner = "", Tax_Mailing = "", Tax_LegalDesp = "", Tax_Year = "", Base_Due = "", Penalty_Interst = "", OtherFee = "", Tax_Total = "", MainNumber = "", Authotity = "", Tax_Authority = "";
        string ReceiptInfoTaburl = "", OriginalTax = "", PriceTax = "", CurrentTax = "", Deliquent_Comments = "";
        string Deliquent_Year = "", Deliquent_Base = "", Deliquent_Intrest = "", Deliquent_Fees = "", Deliquent_Total = "", Deliquent_Details = "", Deliquent_Year1 = "", Deliquent_Base1 = "", Deliquent_Intrest1 = "", Deliquent_Fees1 = "", Deliquent_Total1 = "", Deliquent_Details1 = "", Taxyear_detail = "", TaxYear_Details = "";

        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_TXTravis(string streetno, string direction, string streetname, string city, string streettype, string unitnumber, string ownernm, string parcelNumber, string searchType, string orderNumber, string Account_id)
        {
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

                    if (searchType == "titleflex")
                    {
                        string titleaddress = streetno + " " + streetname + " " + unitnumber;
                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", titleaddress, "TX", "Travis");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_TXTravis"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("http://propaccess.traviscad.org/clientdb/?cid=1");
                        Thread.Sleep(2000);
                        //var Select1 = driver.FindElement(By.Id("propertySearchOptions_taxyear"));
                        //var selectElement11 = new SelectElement(Select1);
                        //selectElement11.SelectByIndex(1);
                        //Thread.Sleep(2000);
                        driver.FindElement(By.Id("propertySearchOptions_advanced")).Click();
                        Thread.Sleep(2000);

                        //Address = streetno + " " + streetname;
                        driver.FindElement(By.Id("propertySearchOptions_streetNumber")).SendKeys(streetno);
                        driver.FindElement(By.Id("propertySearchOptions_streetName")).SendKeys(streetname);
                        //driver.FindElement(By.Id("propertySearchOptions_searchText")).SendKeys(Address);
                        gc.CreatePdf_WOP(orderNumber, "address search", driver, "TX", "Travis");
                        driver.FindElement(By.Id("propertySearchOptions_searchAdv")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        try
                        {
                            ChkMulti = driver.FindElement(By.XPath("//*[@id='pageTitle']/h2")).Text;
                            ChkMulti = WebDriverTest.Between(ChkMulti, "Property Search Results > ", " for");

                            if (ChkMulti == "1 - 1 of 1")
                            {
                                driver.FindElement(By.XPath("//*[@id='propertySearchResults_resultsTable']/tbody/tr[2]/td[10]/a")).Click();
                                Thread.Sleep(2000);
                                gc.CreatePdf_WOP(orderNumber, "Single Address search", driver, "TX", "Travis");
                            }
                            else
                            {
                                try
                                {
                                    IWebElement MultiAddressTB = driver.FindElement(By.XPath("//*[@id='propertySearchResults_resultsTable']/tbody"));
                                    IList<IWebElement> MultiAddressTR = MultiAddressTB.FindElements(By.TagName("tr"));
                                    IList<IWebElement> MultiAddressTD;
                                    gc.CreatePdf_WOP(orderNumber, "Multi Address search", driver, "TX", "Travis");
                                    int AddressmaxCheck = 0;
                                    foreach (IWebElement MultiAddress in MultiAddressTR)
                                    {
                                        if (AddressmaxCheck <= 25 && MultiAddressTR.Count > 2)
                                        {
                                            MultiAddressTD = MultiAddress.FindElements(By.TagName("td"));
                                            if (MultiAddressTD.Count != 0 && !MultiAddress.Text.Contains("Mobile Home") && !MultiAddress.Text.Contains("     ") && !MultiAddress.Text.Contains("Page:   1"))
                                            {
                                                Parcelno = MultiAddressTD[1].Text;
                                                Multi_Type = MultiAddressTD[3].Text;
                                                Property_Address = MultiAddressTD[4].Text;
                                                Owner = MultiAddressTD[6].Text;

                                                MultiAddress_details = Multi_Type + "~" + Property_Address + "~" + Owner;
                                                gc.insert_date(orderNumber, Parcelno, 1058, MultiAddress_details, 1, DateTime.Now);
                                            }
                                            AddressmaxCheck++;
                                        }
                                    }
                                    if (MultiAddressTR.Count <= 2)
                                    {
                                        HttpContext.Current.Session["Nodata_TXTravis"] = "Yes";
                                        driver.Quit();
                                        return "No Data Found";
                                    }
                                    if (MultiAddressTR.Count > 25)
                                    {
                                        HttpContext.Current.Session["multiParcel_TXTravis_Multicount"] = "Maximum";
                                    }
                                    else
                                    {
                                        HttpContext.Current.Session["multiParcel_TXTravis"] = "Yes";
                                    }
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                                catch
                                { }
                            }
                        }
                        catch { }

                    }

                    if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("http://propaccess.traviscad.org/clientdb/?cid=1");
                        Thread.Sleep(2000);
                        //driver.FindElement(By.Id("propertySearchOptions_advanced")).Click();
                        //Thread.Sleep(2000);
                        driver.FindElement(By.Id("propertySearchOptions_searchText")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search", driver, "TX", "Travis");

                        driver.FindElement(By.Id("propertySearchOptions_search")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='propertySearchResults_resultsTable']/tbody/tr[2]/td[10]/a")).Click();
                            Thread.Sleep(2000);
                        }
                        catch{ }
                    }

                    if (searchType == "ownername")
                    {
                        driver.Navigate().GoToUrl("http://propaccess.traviscad.org/clientdb/?cid=1");
                        Thread.Sleep(2000);
                        driver.FindElement(By.Id("propertySearchOptions_advanced")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.Id("propertySearchOptions_ownerName")).SendKeys(ownernm);
                        gc.CreatePdf_WOP(orderNumber, "owner search", driver, "TX", "Travis");

                        driver.FindElement(By.Id("propertySearchOptions_searchAdv")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        try
                        {
                            ChkMulti = driver.FindElement(By.XPath("//*[@id='pageTitle']/h2")).Text;
                            ChkMulti = WebDriverTest.Between(ChkMulti, "Property Search Results > ", " for");

                            if (ChkMulti == "1 - 1 of 1")
                            {
                                driver.FindElement(By.XPath("//*[@id='propertySearchResults_resultsTable']/tbody/tr[2]/td[10]/a")).Click();
                                Thread.Sleep(2000);
                                gc.CreatePdf_WOP(orderNumber, "Single Owner search", driver, "TX", "Travis");
                            }
                            else
                            {
                                try
                                {
                                    IWebElement MultiAddressTB = driver.FindElement(By.XPath("//*[@id='propertySearchResults_resultsTable']/tbody"));
                                    IList<IWebElement> MultiAddressTR = MultiAddressTB.FindElements(By.TagName("tr"));
                                    IList<IWebElement> MultiAddressTD;
                                    gc.CreatePdf_WOP(orderNumber, "Multi Owner search", driver, "TX", "Travis");
                                    int AddressmaxCheck = 0;
                                    foreach (IWebElement MultiAddress in MultiAddressTR)
                                    {
                                        if (AddressmaxCheck <= 25)
                                        {
                                            MultiAddressTD = MultiAddress.FindElements(By.TagName("td"));
                                            if (MultiAddressTD.Count != 0 && !MultiAddress.Text.Contains("Mobile Home") && !MultiAddress.Text.Contains("     ") && !MultiAddress.Text.Contains("Page:   1"))
                                            {
                                                Parcelno = MultiAddressTD[1].Text;
                                                Multi_Type = MultiAddressTD[3].Text;
                                                Property_Address = MultiAddressTD[4].Text;
                                                Owner = MultiAddressTD[6].Text;

                                                MultiAddress_details = Multi_Type + "~" + Property_Address + "~" + Owner;
                                                gc.insert_date(orderNumber, Parcelno, 1058, MultiAddress_details, 1, DateTime.Now);
                                            }
                                            AddressmaxCheck++;
                                        }
                                    }
                                    if (MultiAddressTR.Count > 25)
                                    {
                                        HttpContext.Current.Session["multiParcel_TXTravis_Multicount"] = "Maximum";
                                    }
                                    else
                                    {
                                        HttpContext.Current.Session["multiParcel_TXTravis"] = "Yes";
                                    }
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                                catch
                                { }
                            }
                        }
                        catch { }
                    }

                    else if (searchType == "block")
                    {
                        driver.Navigate().GoToUrl("http://propaccess.traviscad.org/clientdb/?cid=1");
                        Thread.Sleep(2000);

                        driver.FindElement(By.Id("propertySearchOptions_searchText")).SendKeys(Account_id);
                        gc.CreatePdf_WOP(orderNumber, "Account Number Search ", driver, "TX", "Travis");

                        driver.FindElement(By.Id("propertySearchOptions_search")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        try
                        {
                                                          
                            driver.FindElement(By.XPath("//*[@id='propertySearchResults_resultsTable']/tbody/tr[2]/td[10]/a")).Click();
                            Thread.Sleep(2000);
                        }
                        catch { }
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.Id("propertySearchResults_pageHeading"));
                        if (INodata.Text.Contains("None found"))
                        {
                            HttpContext.Current.Session["Nodata_TXTravis"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    //property details              
                    try
                    {
                        fulltext = driver.FindElement(By.XPath("//*[@id='propertyDetails']/table/tbody")).Text.Replace("\r\n", " ");
                        PropertyID = gc.Between(fulltext, "Account Property ID: ", " Legal Description: ");
                        GeographicID = gc.Between(fulltext, "Geographic ID: ", " Zoning: ");
                        Type = gc.Between(fulltext, "Type: ", " Agent Code: ");
                        LegalDescription = gc.Between(fulltext, " Legal Description: ", " Geographic ID: ");
                        ProAddress = gc.Between(fulltext, "Location Address: ", " Mapsco: ");
                        try
                        {
                            Neighborhood = gc.Between(fulltext, "Neighborhood: ", " Map ID: ");
                        }
                        catch
                        { }
                        NeighborhoodCD = gc.Between(fulltext, " Neighborhood CD: ", " Owner Name: ");
                        MapID = gc.Between(fulltext, "Map ID: ", " Neighborhood CD: ");
                        Name = gc.Between(fulltext, "Owner Name: ", " Owner ID: ");
                        MailingAddress = gc.Between(fulltext, "Mailing Address: ", " % Ownership:");
                        OwnerID = gc.Between(fulltext, "Owner ID: ", " Mailing Address: ");
                        Exemptions = GlobalClass.After(fulltext, "Exemptions: ");
                        gc.CreatePdf(orderNumber, PropertyID, "Property Info1", driver, "TX", "Travis");
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='improvementBuilding']")).Click();
                            Thread.Sleep(2000);
                            YearBuilt = driver.FindElement(By.XPath("//*[@id='improvementBuildingDetails']/table[2]/tbody/tr[2]/td[6]")).Text;
                            gc.CreatePdf(orderNumber, PropertyID, "Property Info2", driver, "TX", "Travis");
                        }
                        catch { }

                        try
                        {
                            driver.FindElement(By.Id("land")).Click();
                            Thread.Sleep(2000);
                            Acres = driver.FindElement(By.XPath("//*[@id='landDetails']/table/tbody/tr[2]/td[4]")).Text;
                            gc.CreatePdf(orderNumber, PropertyID, "Property Info3", driver, "TX", "Travis");
                        }
                        catch { }

                        property_details = GeographicID + "~" + Type + "~" + LegalDescription + "~" + ProAddress + "~" + Neighborhood + "~" + NeighborhoodCD + "~" + MapID + "~" + Name + "~" + MailingAddress + "~" + OwnerID + "~" + Exemptions + "~" + YearBuilt + "~" + Acres;
                        gc.insert_date(orderNumber, PropertyID, 1074, property_details, 1, DateTime.Now);
                    }
                    catch
                    { }

                    //Assessment Details
                    try
                    {
                        driver.FindElement(By.Id("values")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, PropertyID, "Value Info", driver, "TX", "Travis");
                        IWebElement AssessmentTable = driver.FindElement(By.XPath("//*[@id='valuesDetails']/table/tbody"));
                        IList<IWebElement> AssessmentTR = AssessmentTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> AssessmentTD;
                        foreach (IWebElement Assessment in AssessmentTR)
                        {
                            AssessmentTD = Assessment.FindElements(By.TagName("td"));
                            if (AssessmentTD.Count != 0 && AssessmentTD[0].Text != " ")
                            {
                                ValueDetails = AssessmentTD[0].Text.Trim() + "~" + AssessmentTD[1].Text.Trim() + "~" + AssessmentTD[2].Text.Trim() + "~" + AssessmentTD[3].Text.Trim();
                                gc.insert_date(orderNumber, PropertyID, 1076, ValueDetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch
                    { }

                    //Taxing Jurisdiction Details
                    try
                    {
                        driver.FindElement(By.Id("taxingJurisdiction")).Click();
                        Thread.Sleep(2000);

                        fulltext1 = driver.FindElement(By.XPath("//*[@id='taxingJurisdictionDetails']/table[1]/tbody")).Text.Replace("\r\n", " ");
                        owner = gc.Between(fulltext1, "Owner:", "% Ownership:");
                        Ownership = gc.Between(fulltext1, "% Ownership:", "Total Value:");
                        TotalValue = GlobalClass.After(fulltext1, "Total Value:");
                        ValueDetails1 = owner + "~" + Ownership + "~" + TotalValue + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                        gc.insert_date(orderNumber, PropertyID, 1078, ValueDetails1, 1, DateTime.Now);
                        gc.CreatePdf(orderNumber, PropertyID, "Jurisdiction Info", driver, "TX", "Travis");
                        IWebElement JurisdictionTable = driver.FindElement(By.XPath("//*[@id='taxingJurisdictionDetails']/table[2]/tbody"));
                        IList<IWebElement> JurisdictionTR = JurisdictionTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> JurisdictionTD;
                        foreach (IWebElement row in JurisdictionTR)
                        {
                            JurisdictionTD = row.FindElements(By.TagName("td"));
                            if (JurisdictionTD.Count != 0)
                            {
                                ValueDetails1 = " " + "~" + " " + "~" + " " + "~" + JurisdictionTD[0].Text.Trim() + "~" + JurisdictionTD[1].Text.Trim() + "~" + JurisdictionTD[2].Text.Trim() + "~" + JurisdictionTD[3].Text.Trim() + "~" + JurisdictionTD[4].Text.Trim() + "~" + JurisdictionTD[5].Text.Trim();
                                gc.insert_date(orderNumber, PropertyID, 1078, ValueDetails1, 1, DateTime.Now);
                            }
                        }
                    }
                    catch
                    { }

                    //Roll Value History Details
                    try
                    {
                        driver.FindElement(By.Id("rollHistory")).Click();
                        Thread.Sleep(2000);
                        IWebElement RollTable = driver.FindElement(By.XPath("//*[@id='rollHistoryDetails']/table/tbody"));
                        IList<IWebElement> RollTR = RollTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> RollTD;

                        foreach (IWebElement Roll in RollTR)
                        {
                            RollTD = Roll.FindElements(By.TagName("td"));
                            if (RollTD.Count != 0)
                            {
                                rollDetails = RollTD[0].Text.Trim() + "~" + RollTD[1].Text.Trim() + "~" + RollTD[2].Text.Trim() + "~" + RollTD[3].Text.Trim() + "~" + RollTD[4].Text.Trim() + "~" + RollTD[5].Text.Trim() + "~" + RollTD[6].Text.Trim();
                                gc.insert_date(orderNumber, PropertyID, 1077, rollDetails, 1, DateTime.Now);
                                gc.CreatePdf(orderNumber, PropertyID, "Roll Info", driver, "TX", "Travis");
                            }
                        }

                    }
                    catch
                    { }
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("https://travis.go2gov.net/cart/search/quickSearch.do");
                    Thread.Sleep(2000);

                    driver.FindElement(By.XPath("//*[@id='quickSearchForm']/input[6]")).SendKeys(GeographicID);
                    gc.CreatePdf(orderNumber, GeographicID, "Tax Search Info", driver, "TX", "Travis");
                    driver.FindElement(By.XPath("//*[@id='quickSearchForm']/button")).SendKeys(Keys.Enter);
                    Thread.Sleep(2000);

                    driver.FindElement(By.XPath("/html/body/div[3]/form[1]/div/table/tbody/tr/td/center/table/tbody/tr[3]/td[3]/span/a")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, GeographicID, "Tax Info", driver, "TX", "Travis");
                    try
                    {
                        //Tax Bill Details
                        Account = driver.FindElement(By.XPath("//*[@id='accountSummarySubView:accountSummaryForm']/table/tbody/tr[1]/td/table/tbody/tr[2]/td/table/tbody/tr/td[1]")).Text;
                        Tax_Owner = driver.FindElement(By.XPath("//*[@id='accountSummarySubView:accountSummaryForm']/table/tbody/tr[1]/td/table/tbody/tr[2]/td/table/tbody/tr/td[2]")).Text;
                        Tax_Mailing = driver.FindElement(By.XPath("//*[@id='accountSummarySubView:accountSummaryForm']/table/tbody/tr[1]/td/table/tbody/tr[2]/td/table/tbody/tr/td[3]")).Text;
                        Tax_LegalDesp = driver.FindElement(By.XPath("//*[@id='accountSummarySubView:accountSummaryForm']/table/tbody/tr[1]/td/table/tbody/tr[2]/td/table/tbody/tr/td[4]")).Text;

                        try
                        {
                            Tax_Year = driver.FindElement(By.XPath("//*[@id='accountSummarySubView:accountSummaryForm']/table/tbody/tr[4]/td/table/tbody/tr[2]/td/table/tbody/tr/td[1]")).Text;
                            Base_Due = driver.FindElement(By.XPath("//*[@id='accountSummarySubView:accountSummaryForm']/table/tbody/tr[4]/td/table/tbody/tr[2]/td/table/tbody/tr/td[2]")).Text;
                            Penalty_Interst = driver.FindElement(By.XPath("//*[@id='accountSummarySubView:accountSummaryForm']/table/tbody/tr[4]/td/table/tbody/tr[2]/td/table/tbody/tr/td[3]")).Text;
                            OtherFee = driver.FindElement(By.XPath("//*[@id='accountSummarySubView:accountSummaryForm']/table/tbody/tr[4]/td/table/tbody/tr[2]/td/table/tbody/tr/td[4]")).Text;
                            Tax_Total = driver.FindElement(By.XPath("//*[@id='accountSummarySubView:accountSummaryForm']/table/tbody/tr[4]/td/table/tbody/tr[2]/td/table/tbody/tr/td[5]")).Text;
                        }
                        catch
                        { }

                        try
                        {
                            string Previous_Year = driver.FindElement(By.XPath("//*[@id='accountSummarySubView:accountSummaryForm']/table/tbody/tr[3]/td/table/thead/tr/th")).Text;
                            Previous_Year = WebDriverTest.After(Previous_Year, "Previous Tax Year ");

                            if (Previous_Year == "Taxes Due")
                            {
                                try
                                {
                                    Tax_Year = driver.FindElement(By.XPath("//*[@id='accountSummarySubView:accountSummaryForm']/table/tbody/tr[5]/td/table/tbody/tr[2]/td/table/tbody/tr/td[1]")).Text;
                                    Base_Due = driver.FindElement(By.XPath("//*[@id='accountSummarySubView:accountSummaryForm']/table/tbody/tr[5]/td/table/tbody/tr[2]/td/table/tbody/tr/td[2]")).Text;
                                    Penalty_Interst = driver.FindElement(By.XPath("//*[@id='accountSummarySubView:accountSummaryForm']/table/tbody/tr[5]/td/table/tbody/tr[2]/td/table/tbody/tr/td[3]")).Text;
                                    OtherFee = driver.FindElement(By.XPath("//*[@id='accountSummarySubView:accountSummaryForm']/table/tbody/tr[5]/td/table/tbody/tr[2]/td/table/tbody/tr/td[4]")).Text;
                                    Tax_Total = driver.FindElement(By.XPath("//*[@id='accountSummarySubView:accountSummaryForm']/table/tbody/tr[5]/td/table/tbody/tr[2]/td/table/tbody/tr/td[5]")).Text;
                                }
                                catch
                                { }

                                //Deliquent Details
                                driver.FindElement(By.XPath("//*[@id='accountSummarySubView:accountSummaryForm:_idJsp89']")).Click();
                                Thread.Sleep(2000);

                                IWebElement DeliquentTB = driver.FindElement(By.XPath("//*[@id='accountYearDetailSubView:accountYearDetailForm:_idJsp81']/tbody"));
                                IList<IWebElement> DeliquentTR = DeliquentTB.FindElements(By.TagName("tr"));
                                IList<IWebElement> DeliquentTD;
                                gc.CreatePdf(orderNumber, GeographicID, "Deliquent Info", driver, "TX", "Travis");
                                foreach (IWebElement Deliquent in DeliquentTR)
                                {
                                    DeliquentTD = Deliquent.FindElements(By.TagName("td"));
                                    if (DeliquentTD.Count != 0)
                                    {
                                        Deliquent_Year = DeliquentTD[0].Text;
                                        Deliquent_Base = DeliquentTD[1].Text;
                                        Deliquent_Intrest = DeliquentTD[2].Text;
                                        Deliquent_Fees = DeliquentTD[3].Text;
                                        Deliquent_Total = DeliquentTD[4].Text;

                                        Deliquent_Details = Deliquent_Year + "~" + Deliquent_Base + "~" + Deliquent_Intrest + "~" + Deliquent_Fees + "~" + Deliquent_Total;
                                        gc.insert_date(orderNumber, GeographicID, 1095, Deliquent_Details, 1, DateTime.Now);
                                    }
                                }

                                IWebElement DeliquentTB1 = driver.FindElement(By.XPath("//*[@id='accountYearDetailSubView:accountYearDetailForm:_idJsp81']/tfoot/tr/td/table/tbody/tr[2]/td/table/tbody"));
                                IList<IWebElement> DeliquentTR1 = DeliquentTB1.FindElements(By.TagName("tr"));
                                IList<IWebElement> DeliquentTD1;

                                foreach (IWebElement Deliquent1 in DeliquentTR1)
                                {
                                    DeliquentTD1 = Deliquent1.FindElements(By.TagName("td"));
                                    if (DeliquentTD1.Count != 0)
                                    {
                                        Deliquent_Year1 = DeliquentTD1[0].Text;
                                        Deliquent_Base1 = DeliquentTD1[1].Text;
                                        Deliquent_Intrest1 = DeliquentTD1[2].Text;
                                        Deliquent_Fees1 = DeliquentTD1[3].Text;
                                        Deliquent_Total1 = DeliquentTD1[4].Text;

                                        Deliquent_Details1 = Deliquent_Year1 + "~" + Deliquent_Base1 + "~" + Deliquent_Intrest1 + "~" + Deliquent_Fees1 + "~" + Deliquent_Total1;
                                        gc.insert_date(orderNumber, GeographicID, 1095, Deliquent_Details1, 1, DateTime.Now);
                                    }
                                }
                                driver.Navigate().Back();
                                Thread.Sleep(2000);
                            }

                            Deliquent_Comments = "For prior year tax amount due, you must call the Collector's Office.";
                        }
                        catch
                        { }
                    }
                    catch
                    { }

                    //Taxing Unit Details
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='accountSummarySubView:accountSummaryForm:_idJsp73']")).Click();
                        Thread.Sleep(2000);

                        try
                        {
                            Taxyear_detail = driver.FindElement(By.XPath("//*[@id='accountYearDetailSubView:accountYearDetailForm']/table/tbody/tr[3]/td")).Text;
                            TaxYear_Details = Taxyear_detail + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                            gc.insert_date(orderNumber, GeographicID, 1089, TaxYear_Details, 1, DateTime.Now);
                        }
                        catch
                        { }
                        IWebElement TaxUnitTB = driver.FindElement(By.XPath("//*[@id='accountYearDetailSubView:accountYearDetailForm:_idJsp83']/tbody"));
                        IList<IWebElement> TaxUnitTR = TaxUnitTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> TaxUnitTD;
                        gc.CreatePdf(orderNumber, GeographicID, "Tax Unit Info", driver, "TX", "Travis");
                        foreach (IWebElement TaxUnit in TaxUnitTR)
                        {
                            TaxUnitTD = TaxUnit.FindElements(By.TagName("td"));
                            if (TaxUnitTD.Count != 0)
                            {
                                Tax_Unit = TaxUnitTD[0].Text;
                                Assed_Value = TaxUnitTD[1].Text;
                                NetTaxb_Val = TaxUnitTD[2].Text;
                                Basedue_Val = TaxUnitTD[3].Text;
                                Penalty_Val = TaxUnitTD[4].Text;
                                Attrney_Val = TaxUnitTD[5].Text;
                                Total_Val = TaxUnitTD[6].Text;

                                TaxUnit_Details = "" + "~" + Tax_Unit + "~" + Assed_Value + "~" + NetTaxb_Val + "~" + Basedue_Val + "~" + Penalty_Val + "~" + Attrney_Val + "~" + Total_Val;
                                gc.insert_date(orderNumber, GeographicID, 1089, TaxUnit_Details, 1, DateTime.Now);
                            }
                        }

                        IWebElement TaxUnitTB1 = driver.FindElement(By.XPath("//*[@id='accountYearDetailSubView:accountYearDetailForm:_idJsp83']/tfoot/tr/td/table/tbody/tr[2]/td/table/tbody"));
                        IList<IWebElement> TaxUnitTR1 = TaxUnitTB1.FindElements(By.TagName("tr"));
                        IList<IWebElement> TaxUnitTD1;

                        foreach (IWebElement TaxUnit1 in TaxUnitTR1)
                        {
                            TaxUnitTD1 = TaxUnit1.FindElements(By.TagName("td"));
                            if (TaxUnitTD1.Count != 0)
                            {
                                Tax_Unit1 = TaxUnitTD1[0].Text;
                                Assed_Value1 = TaxUnitTD1[1].Text;
                                NetTaxb_Val1 = TaxUnitTD1[2].Text;
                                Basedue_Val1 = TaxUnitTD1[3].Text;
                                Penalty_Val1 = TaxUnitTD1[4].Text;
                                Attrney_Val1 = TaxUnitTD1[5].Text;
                                Total_Val1 = TaxUnitTD1[6].Text;

                                TaxUnit_Details1 = "" + "~" + Tax_Unit1 + "~" + Assed_Value1 + "~" + NetTaxb_Val1 + "~" + Basedue_Val1 + "~" + Penalty_Val1 + "~" + Attrney_Val1 + "~" + Total_Val1;
                                gc.insert_date(orderNumber, GeographicID, 1089, TaxUnit_Details1, 1, DateTime.Now);
                            }
                        }
                    }
                    catch
                    { }

                    //Tax Payment Details
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='quickSearchSubView:quickSearchForm:_idJsp38']")).Click();
                        Thread.Sleep(2000);

                        IWebElement TaxPaymentTB = driver.FindElement(By.XPath("//*[@id='legalPaymentHistorySubView:legalPaymentHistoryForm:data']/tbody"));
                        IList<IWebElement> TaxPaymentTR = TaxPaymentTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> TaxPaymentTD;
                        gc.CreatePdf(orderNumber, GeographicID, "Tax Payment Info", driver, "TX", "Travis");
                        foreach (IWebElement TaxPayment in TaxPaymentTR)
                        {
                            TaxPaymentTD = TaxPayment.FindElements(By.TagName("td"));
                            if (TaxPaymentTD.Count != 0)
                            {
                                Receipt = TaxPaymentTD[0].Text;
                                Receipt_Taxyear = TaxPaymentTD[1].Text;
                                Receipt_PaymentDate = TaxPaymentTD[2].Text;
                                Receipt_PaymentAmount = TaxPaymentTD[3].Text;

                                TaxPayment_Details = Receipt + "~" + Receipt_Taxyear + "~" + Receipt_PaymentDate + "~" + Receipt_PaymentAmount;
                                gc.insert_date(orderNumber, GeographicID, 1094, TaxPayment_Details, 1, DateTime.Now);
                            }
                        }
                    }
                    catch
                    { }

                    List<string> ReceiptInfoTab = new List<string>();
                    try
                    {
                        var chromeOptions = new ChromeOptions();
                        var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];
                        chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                        chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                        chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                        chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                        var driver1 = new ChromeDriver(chromeOptions);
                        driver1.Navigate().GoToUrl(driver.Url);

                        for (int j = 0; j < 3; j++)
                        {
                            IWebElement Receipttable = driver1.FindElement(By.XPath("//*[@id='legalPaymentHistorySubView:legalPaymentHistoryForm:data:" + j + ":_idJsp87']"));
                            string BillTax2 = Receipttable.GetAttribute("href");
                            Receipttable.Click();
                            Thread.Sleep(3000);
                            var files = new DirectoryInfo(downloadDirectory).GetFiles("*.*");
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
                            gc.AutoDownloadFile(orderNumber, parcelNumber, "Travis", "TX", latestfile);
                        }

                        //Bill Download
                        try
                        {
                            IWebElement OriginalTaxBill = driver1.FindElement(By.Id("quickSearchSubView:quickSearchForm:_idJsp41"));
                            OriginalTax = OriginalTaxBill.GetAttribute("href");
                            string Original_Tax = gc.Between(OriginalTax, ".net/", "?").Trim();
                            OriginalTaxBill.Click();
                            Thread.Sleep(3000);
                            gc.AutoDownloadFileSpokane(orderNumber, parcelNumber, "Travis", "TX", Original_Tax + ".pdf");
                        }
                        catch
                        { }

                        try
                        {
                            IWebElement PriceTaxBill = driver1.FindElement(By.Id("quickSearchSubView:quickSearchForm:_idJsp47"));
                            PriceTax = PriceTaxBill.GetAttribute("href");
                            string priceTax_Bill = gc.Between(PriceTax, ".net/", "?").Trim();
                            PriceTaxBill.Click();
                            Thread.Sleep(3000);
                            gc.AutoDownloadFileSpokane(orderNumber, parcelNumber, "Travis", "TX", priceTax_Bill + ".pdf");
                        }
                        catch
                        { }

                        try
                        {
                            IWebElement CurrentTaxBill = driver1.FindElement(By.Id("quickSearchSubView:quickSearchForm:_idJsp50"));
                            CurrentTax = CurrentTaxBill.GetAttribute("href");
                            string Current_TaxBill = gc.Between(CurrentTax, ".net/", "?").Trim();
                            CurrentTaxBill.Click();
                            Thread.Sleep(3000);
                            gc.AutoDownloadFileSpokane(orderNumber, parcelNumber, "Travis", "TX", Current_TaxBill + ".pdf");
                        }
                        catch
                        { }

                        driver1.Quit();
                    }
                    catch
                    { }

                    try
                    {
                        driver.Navigate().GoToUrl("https://tax-office.traviscountytx.gov/contact");
                        Thread.Sleep(2000);

                        MainNumber = driver.FindElement(By.XPath("//*[@id='tab1']/div/div/p[1]")).Text;
                        MainNumber = WebDriverTest.Before(MainNumber, " Monday - Friday");

                        driver.FindElement(By.XPath("//*[@id='container']/div/div[2]/div/ul/li[2]/a")).Click();
                        Thread.Sleep(1000);
                        gc.CreatePdf(orderNumber, GeographicID, "Tax Authority", driver, "TX", "Travis");
                        Authotity = driver.FindElement(By.XPath("//*[@id='tab2']/div/div/p[3]")).Text;
                        Tax_Authority = Authotity + " " + MainNumber;
                    }
                    catch
                    { }

                    ValueDetails1 = Tax_Year + "~" + Account + "~" + Tax_Owner + "~" + Tax_Mailing + "~" + Tax_LegalDesp + "~" + Base_Due + "~" + Penalty_Interst + "~" + OtherFee + "~" + Tax_Total + "~" + Tax_Authority + "~" + Deliquent_Comments;
                    gc.insert_date(orderNumber, GeographicID, 1079, ValueDetails1, 1, DateTime.Now);
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "TX", "Travis", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "TX", "Travis");
                    return "Data Inserted Successfully";
                }
                catch (Exception ex)
                {
                    driver.Quit();
                    GlobalClass.LogError(ex, orderNumber);
                    throw;
                }
            }
        }
    }
}
