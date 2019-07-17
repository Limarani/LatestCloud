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
using System.Web;

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_McLennanTX
    {
        IWebElement PaidCount;
        IWebDriver driver;
        DBconnection db = new DBconnection();
        List<string> link = new List<string>();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());


        public string FTP_McLennan(string houseno, string sname, string stype, string unitno, string direction, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            List<string> taxinformation = new List<string>();
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            { 
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("https://propaccess.trueautomation.com/clientdb/?cid=20");
                    var SelectY = driver.FindElement(By.Id("propertySearchOptions_taxyear"));
                    var selectElementY = new SelectElement(SelectY);
                    selectElementY.SelectByIndex(1);
                    driver.FindElement(By.Id("propertySearchOptions_advanced")).SendKeys(Keys.Enter);
                    Thread.Sleep(2000);
                    if (searchType == "titleflex")
                    {
                        string titleaddress = houseno + " " + sname + " " + stype + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "TX", "McLennan");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_McLennanTX"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        //var Select1 = driver.FindElement(By.Id("propertySearchOptions_searchType"));
                        //var selectElement11 = new SelectElement(Select1);
                        //selectElement11.SelectByText("Property Address");
                        driver.FindElement(By.Id("propertySearchOptions_streetNumber")).SendKeys(houseno);
                        driver.FindElement(By.Id("propertySearchOptions_streetName")).SendKeys(sname);
                        gc.CreatePdf_WOP(orderNumber, "address search", driver, "TX", "McLennan");
                        driver.FindElement(By.Id("propertySearchOptions_searchAdv")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "address search result", driver, "TX", "McLennan");

                    }
                    if (searchType == "parcel")
                    {
                        //var Select1 = driver.FindElement(By.Id("propertySearchOptions_searchType"));
                        //var selectElement11 = new SelectElement(Select1);
                        //selectElement11.SelectByText("Account Number");
                        driver.FindElement(By.Id("propertySearchOptions_propertyid")).SendKeys(parcelNumber.Replace("-", ""));
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search", driver, "TX", "McLennan");
                        driver.FindElement(By.Id("propertySearchOptions_searchAdv")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search result", driver, "TX", "McLennan");

                    }

                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("propertySearchOptions_ownerName")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "owner search", driver, "TX", "McLennan");
                        driver.FindElement(By.Id("propertySearchOptions_searchAdv")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "owner search result", driver, "TX", "McLennan");
                    }
                    if (searchType == "block")
                    {
                        //var Select1 = driver.FindElement(By.Id("propertySearchOptions_searchType"));
                        //var selectElement11 = new SelectElement(Select1);
                        //selectElement11.SelectByText("Account Number");
                        driver.FindElement(By.Id("propertySearchOptions_geoid")).SendKeys(unitno);
                        gc.CreatePdf_WOP(orderNumber, "Account Number Search ", driver, "TX", "McLennan");
                        driver.FindElement(By.Id("propertySearchOptions_searchAdv")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Account Number Search  result", driver, "TX", "McLennan");

                        //       gc.CreatePdf_WOP(orderNumber, "owner search result", driver, "TX", "Williamson");
                    }
                    //Geographic ID~Type~Property Address~Owner Name

                    int trCount = driver.FindElements(By.XPath("//*[@id='propertySearchResults_resultsTable']/tbody/tr")).Count;
                    if (trCount > 3)
                    {
                        int maxCheck = 0;
                        IWebElement tbmulti = driver.FindElement(By.XPath("//*[@id='propertySearchResults_resultsTable']/tbody"));
                        IList<IWebElement> TRmulti5 = tbmulti.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDmulti5;
                        foreach (IWebElement row in TRmulti5)
                        {
                            if (maxCheck <= 25)
                            {
                                TDmulti5 = row.FindElements(By.TagName("td"));
                                if (TDmulti5.Count >= 10 && TDmulti5[3].Text.Contains("Real"))
                                {
                                    string multi1 = TDmulti5[2].Text + "~" + TDmulti5[3].Text + "~" + TDmulti5[4].Text + "~" + TDmulti5[6].Text;
                                    gc.insert_date(orderNumber, TDmulti5[1].Text, 988, multi1, 1, DateTime.Now);
                                }
                                maxCheck++;
                            }
                        }
                        if (TRmulti5.Count > 25)
                        {
                            HttpContext.Current.Session["multiParcel_McLennan_Multicount"] = "Maximum";
                        }
                        else
                        {
                            HttpContext.Current.Session["multiparcel_McLennan"] = "Yes";
                        }

                        driver.Quit();
                        return "MultiParcel";
                    }
                    else
                    {
                        try
                        {
                            string type = driver.FindElement(By.XPath("//*[@id='propertySearchResults_resultsTable']/tbody/tr[2]/td[4]")).Text.Replace("\r\n", " ");
                            if (type == "Real")
                            {
                                driver.FindElement(By.XPath("//*[@id='propertySearchResults_resultsTable']/tbody/tr[2]/td[10]/a")).Click();
                                Thread.Sleep(2000);
                            }
                            else
                            {
                                HttpContext.Current.Session["alert_msg"] = "Yes";

                                driver.Quit();
                                return "MultiParcel";
                            }
                        }
                        catch { }

                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.Id("propertySearchResults_pageHeading"));
                        if (INodata.Text.Contains("None found"))
                        {
                            HttpContext.Current.Session["Nodata_McLennanTX"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    //property details
                    string PropertyID = "", GeographicID = "", Name = "", MailingAddress = "", OwnerID = "", LegalDescription = "", Neighborhood = "", Type = "", Address = "", NeighborhoodCD = "", Exemptions = "", MapID = "", YearBuilt = "", Acres = "";


                    string fulltext = driver.FindElement(By.XPath("//*[@id='propertyDetails']/table/tbody")).Text.Replace("\r\n", " ");
                    PropertyID = gc.Between(fulltext, "Account Property ID:", " Legal Description: ").Trim();
                    string property_use_code = "", property_use_dis = "", ownership = "";
                    GeographicID = gc.Between(fulltext, "Geographic ID: ", "Zoning").Trim();
                    Name = gc.Between(fulltext, "Owner Name:", "Owner ID:").Trim();
                    MailingAddress = gc.Between(fulltext, "Mailing Address:", " % Ownership:").Trim();
                    OwnerID = gc.Between(fulltext, "Owner ID:", "Mailing Address:").Trim();
                    LegalDescription = gc.Between(fulltext, "Legal Description:", "Geographic ID:").Trim();
                    Neighborhood = gc.Between(fulltext, "Neighborhood:", "Map ID:").Trim();
                    Type = gc.Between(fulltext, "Type:", "Agent Code").Trim();
                    Address = gc.Between(fulltext, "Location Address:", "Mapsco:").Trim();
                    NeighborhoodCD = gc.Between(fulltext, "Neighborhood CD:", "Owner").Trim();
                    Exemptions = GlobalClass.After(fulltext, "Exemptions:").Trim();
                    MapID = gc.Between(fulltext, "Map ID:", "Neighborhood CD:").Trim();
                    property_use_code = gc.Between(fulltext, "Property Use Code:", "Property Use Description:").Trim();
                    if (fulltext.Contains("Protest"))
                    {
                        property_use_dis = gc.Between(fulltext, "Property Use Description:", "Protest").Trim();
                    }
                    else
                    {
                        property_use_dis = gc.Between(fulltext, "Property Use Description:", "Location").Trim();
                    }

                    ownership = gc.Between(fulltext, "% Ownership:", "Exemptions:").Trim();
                    driver.FindElement(By.XPath("//*[@id='improvementBuilding']")).Click();
                    Thread.Sleep(2000);
                    try
                    {
                        YearBuilt = driver.FindElement(By.XPath("//*[@id='improvementBuildingDetails']/table[2]/tbody/tr[2]/td[6]")).Text;
                    }
                    catch { }
                    driver.FindElement(By.Id("land")).Click();
                    Thread.Sleep(2000);
                    try
                    {
                        Acres = driver.FindElement(By.XPath("//*[@id='landDetails']/table/tbody/tr[2]/td[4]")).Text;
                    }
                    catch { }
                    //Property ID~Geographic ID~Type~Property Use Code~Property Use Description~Legal Description~Property Address~Neighborhood~Neighborhood CD~Owner Name~Mailing Address~Owner ID~% Ownership~Map ID~Exemptions~Year Built~Acres
                    string property_details = PropertyID + "~" + GeographicID + "~" + Type + "~" + property_use_code + "~" + property_use_dis + "~" + LegalDescription + "~" + Address + "~" + Neighborhood + "~" + NeighborhoodCD + "~" + Name + "~" + MailingAddress + "~" + OwnerID + "~" + ownership + "~" + MapID + "~" + Exemptions + "~" + YearBuilt + "~" + Acres;
                    gc.insert_date(orderNumber, PropertyID, 982, property_details, 1, DateTime.Now);

                    //    Assessment Details Table:

                    //  Values Details Table:
                    //Description~Sign~Value1~Value2
                    driver.FindElement(By.Id("values")).Click();
                    Thread.Sleep(2000);


                    IWebElement multitableElement1 = driver.FindElement(By.XPath("//*[@id='valuesDetails']/table/tbody"));
                    IList<IWebElement> multitableRow1 = multitableElement1.FindElements(By.TagName("tr"));
                    IList<IWebElement> multirowTD1;
                    foreach (IWebElement row in multitableRow1)
                    {
                        multirowTD1 = row.FindElements(By.TagName("td"));
                        if (multirowTD1.Count != 0 && multirowTD1[0].Text != " ")
                        {
                            string ValueDetails = multirowTD1[0].Text.Trim() + "~" + multirowTD1[1].Text.Trim() + "~" + multirowTD1[2].Text.Trim() + "~" + multirowTD1[3].Text.Trim();
                            gc.insert_date(orderNumber, PropertyID, 983, ValueDetails, 1, DateTime.Now);

                        }
                    }


                    //Taxing Jurisdiction Details Table:
                    //Owner~% Ownership~Total Value~Entity~Description~Tax Rate~Appraised Value~Taxable Value~Estimated Tax
                    driver.FindElement(By.Id("taxingJurisdiction")).Click();
                    Thread.Sleep(2000);
                    string owner = "", Ownership = "", TotalValue = "";
                    string ValueDetails1 = "", msg = "";
                    string fulltext1 = driver.FindElement(By.XPath("//*[@id='taxingJurisdictionDetails']/table[1]/tbody")).Text.Replace("\r\n", " ");

                    owner = gc.Between(fulltext1, "Owner:", "% Ownership:").Trim();
                    Ownership = gc.Between(fulltext1, "% Ownership:", "Total Value:").Trim();
                    TotalValue = GlobalClass.After(fulltext1, "Total Value:").Trim();
                    ValueDetails1 = owner + "~" + Ownership + "~" + TotalValue + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                    gc.insert_date(orderNumber, PropertyID, 984, ValueDetails1, 1, DateTime.Now);

                    string city = "", code = "", jurdsition = "", Authority = "", urlAuth = "";
                    IWebElement multitableElement11 = driver.FindElement(By.XPath("//*[@id='taxingJurisdictionDetails']/table[2]/tbody"));
                    IList<IWebElement> multitableRow11 = multitableElement11.FindElements(By.TagName("tr"));
                    IList<IWebElement> multirowTD11;
                    foreach (IWebElement row in multitableRow11)
                    {
                        multirowTD11 = row.FindElements(By.TagName("td"));
                        if (multirowTD11.Count != 0)
                        {

                            if (multirowTD11[0].Text.Trim() == "46" || multirowTD11[0].Text.Trim() == "78")

                            {

                                city = "Bosque County";
                                code = multirowTD11[0].Text;
                                jurdsition = multirowTD11[1].Text;
                                urlAuth = "http://www.bosquecountytaxoffice.com/accountsearch.asp";
                                Authority = code + "~" + jurdsition + "~" + city + "~" + urlAuth;
                                gc.insert_date(orderNumber, PropertyID, 990, Authority, 1, DateTime.Now);

                            }
                            if (multirowTD11[1].Text.Contains("Oglesby") || multirowTD11[1].Text.Contains("Downtown"))
                            {
                                //Code~Jurisdictions~Collecting Authority~Website URL
                                city = "-";
                                code = multirowTD11[0].Text;
                                jurdsition = multirowTD11[1].Text;
                                urlAuth = "Need to call tax office";
                                Authority = code + "~" + jurdsition + "~" + city + "~" + urlAuth;
                                gc.insert_date(orderNumber, PropertyID, 990, Authority, 1, DateTime.Now);
                            }
                            ValueDetails1 = " " + "~" + " " + "~" + " " + "~" + multirowTD11[0].Text.Trim() + "~" + multirowTD11[1].Text.Trim() + "~" + multirowTD11[2].Text.Trim() + "~" + multirowTD11[3].Text.Trim() + "~" + multirowTD11[4].Text.Trim() + "~" + multirowTD11[5].Text.Trim();
                            gc.insert_date(orderNumber, PropertyID, 984, ValueDetails1, 1, DateTime.Now);

                        }
                    }

                    // Roll Value History Details Table:
                    //Year~Improvements~Land Market~Ag Valuation~Appraised~HS Cap~Assessed
                    driver.FindElement(By.Id("rollHistory")).Click();
                    Thread.Sleep(2000);

                    IWebElement multitableElement2 = driver.FindElement(By.XPath("//*[@id='rollHistoryDetails']/table/tbody"));
                    IList<IWebElement> multitableRow2 = multitableElement2.FindElements(By.TagName("tr"));
                    IList<IWebElement> multirowTD2;
                    foreach (IWebElement row in multitableRow2)
                    {
                        multirowTD2 = row.FindElements(By.TagName("td"));
                        if (multirowTD2.Count != 0)
                        {
                            string rollDetails = multirowTD2[0].Text.Trim() + "~" + multirowTD2[1].Text.Trim() + "~" + multirowTD2[2].Text.Trim() + "~" + multirowTD2[3].Text.Trim() + "~" + multirowTD2[4].Text.Trim() + "~" + multirowTD2[5].Text.Trim() + "~" + multirowTD2[6].Text.Trim();
                            gc.insert_date(orderNumber, PropertyID, 985, rollDetails, 1, DateTime.Now);
                        }
                    }
                    //deed history

                    driver.FindElement(By.Id("deedHistory")).Click();
                    Thread.Sleep(2000);


                    gc.CreatePdf(orderNumber, PropertyID, "property details", driver, "TX", "McLennan");

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    try
                    {
                        driver.Navigate().GoToUrl("http://actweb.acttax.com/act_webdev/mclennan/index.jsp");
                        Thread.Sleep(3000);


                        driver.FindElement(By.Id("sc5")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.Id("criteria")).SendKeys(PropertyID);
                        gc.CreatePdf(orderNumber, PropertyID, "tax search", driver, "TX", "McLennan");
                        driver.FindElement(By.Name("submit")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, PropertyID, "tax search result", driver, "TX", "McLennan");

                        driver.FindElement(By.XPath("//*[@id='data-block']/table/tbody/tr/td/table/tbody/tr/td[2]/h3/a")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, PropertyID, "tax search result details", driver, "TX", "McLennan");
                        //Jurisdiction Information Details Table: 
                        string year = "", account = "", ExemptionsJ = "";


                        driver.FindElement(By.LinkText("Taxes Due Detail by Year and Jurisdiction")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, PropertyID, "Taxes Due Detail by Year and Jurisdiction", driver, "TX", "McLennan");
                        //Taxes Due Detail by Year Details Table: 
                        //Account No~Appr. Dist. No~Active Lawsuits~Year~Base Tax Due~Penalty, Interest, and ACC* Due(end of July )~Total Due July~Penalty, Interest, and ACC* Due(end of August )~Total Due August~Penalty, Interest, and ACC*Due(end of September)~Total Due September
                        string accountno = "", distno = "", ActiveLawsuits = "";

                        //Year~Base Tax Due~Penalty, Interest, and ACC* Due~Total Due~Penalty, Interest, and ACC* Due1~Total Due1~Penalty, Interest, and ACC*Due2~Total Due2

                        IWebElement multitableElement31 = driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr[1]/td/table[2]/tbody/tr/td/table/tbody"));
                        IList<IWebElement> multitableRow31 = multitableElement31.FindElements(By.TagName("tr"));
                        IList<IWebElement> multirowTD31;
                        foreach (IWebElement row in multitableRow31)
                        {
                            multirowTD31 = row.FindElements(By.TagName("td"));
                            if (multirowTD31.Count == 8 && !row.Text.Contains("Base Tax Due"))
                            {
                                string TaxesDue = multirowTD31[0].Text.Trim() + "~" + multirowTD31[1].Text.Trim() + "~" + multirowTD31[2].Text.Trim() + "~" + multirowTD31[3].Text.Trim() + "~" + multirowTD31[4].Text.Trim() + "~" + multirowTD31[5].Text.Trim() + "~" + multirowTD31[6].Text.Trim() + "~" + multirowTD31[7].Text.Trim();
                                gc.insert_date(orderNumber, PropertyID, 986, TaxesDue, 1, DateTime.Now);
                            }
                            if (multirowTD31.Count == 1)
                            {
                                string TaxesDue = multirowTD31[0].Text.Trim() + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                gc.insert_date(orderNumber, PropertyID, 986, TaxesDue, 1, DateTime.Now);

                            }
                            if (multirowTD31.Count == 4)
                            {
                                string TaxesDue = "" + "~" + "" + "~" + multirowTD31[1].Text.Trim() + "~" + "" + "~" + multirowTD31[2].Text.Trim() + "~" + "" + "~" + multirowTD31[3].Text.Trim() + "~" + "";
                                gc.insert_date(orderNumber, PropertyID, 986, TaxesDue, 1, DateTime.Now);

                            }
                        }

                        driver.FindElement(By.LinkText("Return to the Previous Page")).Click();
                        Thread.Sleep(2000);
                        //Tax Payment Details Table: 

                        driver.FindElement(By.LinkText("Payment Information")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, PropertyID, "Payment Information", driver, "TX", "McLennan");
                        //Account Number~Paid Date~Amount~Tax Year~Description~Paid By
                        string accountnos = driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr[1]/td/h3[3]")).Text.Replace("Account No.:", "");

                        IWebElement multitableElement32 = driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr[1]/td/table[2]/tbody"));
                        IList<IWebElement> multitableRow32 = multitableElement32.FindElements(By.TagName("tr"));
                        IList<IWebElement> multirowTD32;
                        foreach (IWebElement row in multitableRow32)
                        {
                            multirowTD32 = row.FindElements(By.TagName("td"));
                            if (multirowTD32.Count != 0 && !row.Text.Contains("Receipt Date"))
                            {
                                string TaxesDue = accountnos + "~" + multirowTD32[0].Text.Trim() + "~" + multirowTD32[1].Text.Trim() + "~" + multirowTD32[2].Text.Trim() + "~" + multirowTD32[3].Text.Trim() + "~" + multirowTD32[4].Text.Trim();
                                gc.insert_date(orderNumber, PropertyID, 987, TaxesDue, 1, DateTime.Now);
                            }
                        }

                        driver.FindElement(By.LinkText("Return to the Previous Page")).Click();
                        Thread.Sleep(2000);


                        string jurd = "", pendingpayment = "", accountNo = "", AppraisalDistrict = "", OwnerName = "", OwnerAddress = "", PropertyAddress = "", legal = "", CurrentTax = "", CurrentAmount = "", PriorYearAmount = "", TotalAmount = "", LastPaymentAmount = "", LastPayer = "", LastPaymentDate = "", ActiveLawsuitsTax = "", GrossValue = "", LandValue = "", ImprovementValue = "", CappedValue = "", AgriculturalValue = "", ExemptionsTax = "";
                        string fullTaxeBill1 = driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr/td/table/tbody/tr[1]/td/table[2]/tbody")).Text.Replace("\r\n", " ");
                        accountno = gc.Between(fullTaxeBill1, "Account Number:", "Address:");
                        string market = "";
                        OwnerAddress = gc.Between(fullTaxeBill1, "Address:", "Property Site Address:").Trim();
                        PropertyAddress = gc.Between(fullTaxeBill1, "Property Site Address:", " Legal Description:");
                        legal = gc.Between(fullTaxeBill1, "Legal Description:", " Current Tax Levy:");
                        CurrentTax = gc.Between(fullTaxeBill1, " Current Tax Levy:", "Current Amount Due:");
                        CurrentAmount = gc.Between(fullTaxeBill1, "Current Amount Due:", "Prior Year Amount Due:");
                        PriorYearAmount = gc.Between(fullTaxeBill1, "Prior Year Amount Due:", "Total Amount Due:");
                        TotalAmount = gc.Between(fullTaxeBill1, "Total Amount Due:", "Last Payment Amount for Current Year Taxes:");
                        LastPaymentAmount = gc.Between(fullTaxeBill1, "Last Payment Amount for Current Year Taxes:", "Last Payer for Current Year Taxes:");
                        LastPayer = gc.Between(fullTaxeBill1, "Last Payer for Current Year Taxes:", "Last Payment Date for Current Year Taxes:");
                        LastPaymentDate = gc.Between(fullTaxeBill1, "Last Payment Date for Current Year Taxes:", "Active Lawsuits:");
                        string tax_year = driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr/td/table/tbody/tr[1]/td/table[1]/tbody/tr/td/h5/b")).Text;
                        tax_year = gc.Between(tax_year, " tax information for", ". All amounts");
                        ActiveLawsuitsTax = gc.Between(fullTaxeBill1, "Active Lawsuits:", "Payment Information");
                        pendingpayment = gc.Between(fullTaxeBill1, "Pending Credit Card or E–Check Payments:", "Market Value:");
                        market = gc.Between(fullTaxeBill1, "Market Value:", "Land Value:");
                        LandValue = gc.Between(fullTaxeBill1, "Land Value:", "Improvement Value:");
                        ImprovementValue = gc.Between(fullTaxeBill1, "Improvement Value:", "Capped Value:");
                        CappedValue = gc.Between(fullTaxeBill1, "Capped Value:", "Agricultural Value:");
                        AgriculturalValue = gc.Between(fullTaxeBill1, "Agricultural Value:", "Exemptions:");
                        ExemptionsTax = gc.Between(fullTaxeBill1, "Exemptions:", "Jurisdictions:");
                        string fullTaxeBill11 = driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr/td/table/tbody/tr[1]/td/table[2]/tbody")).Text;

                        jurd = GlobalClass.After(fullTaxeBill11, "Jurisdictions:");
                        jurd = GlobalClass.Before(jurd, "Taxes Due Detail by Year and Jurisdiction");
                        jurd = jurd.Trim();
                        jurd = jurd.Replace("\r\n", " , ");
                        //Tax Year~Account Number~Owner Information~Property Site Address~Legal Description~Current Tax Levy~Current Amount Due~Prior Year Amount Due~Total Amount Due~Last Payment Amount for Current Year Taxes~Last Payer for Current Year Taxes~Last Payment Date for Current Year Taxes~Active Lawsuits~Pending Payment~Land Value~Improvement Value~Capped Value~Agricultural Value~Exemptions~Tax Authority

                        string taxbill = tax_year + "~" + accountno + "~" + OwnerAddress + "~" + PropertyAddress + "~" + legal + "~" + CurrentTax + "~" + CurrentAmount + "~" + PriorYearAmount + "~" + TotalAmount + "~" + LastPaymentAmount + "~" + LastPayer + "~" + LastPaymentDate + "~" + ActiveLawsuitsTax + "~" + pendingpayment + "~" + market + "~" + LandValue + "~" + ImprovementValue + "~" + CappedValue + "~" + AgriculturalValue + "~" + ExemptionsTax + "~" + jurd + "~" + "McLennan County Tax Office P.O.BOX 406, WACO, TX 76703";
                        gc.insert_date(orderNumber, PropertyID, 989, taxbill, 1, DateTime.Now);

                        IWebElement Itaxstmt1 = driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr/td/table/tbody/tr[1]/td/table[2]/tbody/tr/td[2]/h3[3]/a"));
                        Thread.Sleep(2000);
                        string stmt11 = Itaxstmt1.GetAttribute("href");
                        driver.Navigate().GoToUrl(stmt11);
                        Thread.Sleep(4000);
                        IWebElement Itaxstmt = driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr/td/table/tbody/tr[2]/td/div/h3/a"));
                        string stmt1 = Itaxstmt.GetAttribute("href");
                        gc.downloadfile(stmt1, orderNumber, PropertyID, "Tax statement", "TX", "McLennan");
                        driver.Navigate().Back();
                        Thread.Sleep(2000);
                        /*
                         if(city== "Bosque")
                         {
                             driver.Navigate().GoToUrl("http://www.bosquecountytaxoffice.com/accountsearch.asp");
                             Thread.Sleep(3000);
                             driver.FindElement(By.Name("txtSearch")).SendKeys(PropertyID);
                             gc.CreatePdf(orderNumber, PropertyID, "city tax search", driver, "TX", "McLennan");
                             driver.FindElement(By.Name("search")).Click();
                             Thread.Sleep(2000);
                             try
                             {
                                 IWebElement iframe = driver.FindElement(By.XPath("/html/body/div[1]/div[2]/iframe"));
                                 driver.SwitchTo().Frame(iframe);
                                 Thread.Sleep(2000);
                                 driver.FindElement(By.ClassName("rc-button-default goog-inline-block")).Click();

                             }
                             catch  { }
                             Thread.Sleep(2000);
                             gc.CreatePdf(orderNumber, PropertyID, "city tax search result", driver, "TX", "McLennan");

                             driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[2]/table/tbody/tr[2]/td/div/table[3]/tbody/tr[2]/td[1]/a")).Click();
                             Thread.Sleep(2000);
                             driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[2]/tbody/tr/td[1]/a")).Click();
                             Thread.Sleep(2000);


                             //Year~Unit~Levy Amount~Paid Amount~Levy Due~Penalty~Interest~Col Penalty~Total Due~Paid Date
                             IWebElement multitableElement321 = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/div[2]/table/tbody"));
                             IList<IWebElement> multitableRow321 = multitableElement321.FindElements(By.TagName("tr"));
                             IList<IWebElement> multirowTD321;
                             foreach (IWebElement row in multitableRow321)
                             {
                                 multirowTD321 = row.FindElements(By.TagName("td"));
                                 if (multirowTD321.Count != 0 && !row.Text.Contains("Year"))
                                 {
                                     string TaxesDue =  multirowTD321[0].Text.Trim() + "~" + multirowTD321[1].Text.Trim() + "~" + multirowTD321[2].Text.Trim() + "~" + multirowTD321[3].Text.Trim() + "~" + multirowTD321[4].Text.Trim() + "~" + multirowTD321[5].Text.Trim() + "~" + multirowTD321[6].Text.Trim() + "~" + multirowTD321[7].Text.Trim() + "~" + multirowTD321[8].Text.Trim() + "~" + multirowTD321[9].Text.Trim();
                                     gc.insert_date(orderNumber, PropertyID, 990, TaxesDue, 1, DateTime.Now);
                                 }
                             }
                             //      

                             string fullTaxecity = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[1]/tbody/tr/td[1]/table/tbody")).Text.Replace("\r\n", " ");
                             //Account Number~Property ID~Property Address~Owner Name~Agriculture Value~Improvement Value~Non-Home Site Value~Land~Land Ag Land~Exemption Type~Exemption Value~Taxing Authority
                             string accountcity = "", property_id = "", addresscity = "", ownernamecity = "", agriculturalvalue = "", improvementvalue = "", nonHomesite = "", land = "", landAg = "", Exemptiontype = "", ExemptionValue = "", taxcity = "";

                             accountcity = gc.Between(fullTaxecity, "Account:", "APD:");
                             property_id= gc.Between(fullTaxecity, "APD:", "Location:");
                             addresscity = gc.Between(fullTaxecity, "Location:", "Legal:");
                             ownernamecity = GlobalClass .After(fullTaxecity, "Owner:");
                             string fullTaxecity1 = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[1]/tbody/tr/td[3]/table/tbody")).Text;



                             Exemptiontype = GlobalClass.After (fullTaxecity1, "Exemptions");
                             //ExemptionValue = gc.Between(fullTaxecity, "", "");


                             IWebElement multitableElement34 = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[1]/tbody/tr/td[3]/table/tbody"));
                             IList<IWebElement> multitableRow34 = multitableElement34.FindElements(By.TagName("tr"));
                             IList<IWebElement> multirowTD34;
                             foreach (IWebElement row in multitableRow34)
                             {
                                 multirowTD34 = row.FindElements(By.TagName("td"));
                                 if (multirowTD34.Count != 0 && !row.Text.Contains("Year"))
                                 {
                                     if (multirowTD34[0].Text == "Agriculture")
                                         agriculturalvalue = multirowTD34[1].Text;
                                     if (multirowTD34[0].Text == "Improvement")
                                         improvementvalue = multirowTD34[1].Text;
                                     if (multirowTD34[0].Text == "Improvement Non-Home Site")
                                         nonHomesite = multirowTD34[1].Text;
                                     if (multirowTD34[0].Text == "Land")
                                         land = multirowTD34[1].Text;
                                     if (multirowTD34[0].Text == "Land Ag Land")
                                         landAg = multirowTD34[1].Text;

                                 }
                             }

                             string TaxesDuecity = accountcity + "~" + property_id + "~" + addresscity + "~" + ownernamecity + "~" + agriculturalvalue + "~" + improvementvalue + "~" + nonHomesite + "~" + land + "~" + landAg + "~" + Exemptiontype + "~" + ExemptionValue + "~" + "Bosque County Tax Office 102 W.Morgan P.O.Box 346, Meridian, TX 76665 - 0346"; 
                  gc.insert_date(orderNumber, PropertyID, 990, TaxesDuecity, 1, DateTime.Now);


                         }
                         */
                        TaxTime = DateTime.Now.ToString("HH:mm:ss");


                    }
                    catch { }
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "TX", "McLennan", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    driver.Quit();
                    GlobalClass.titleparcel = "";
                    gc.mergpdf(orderNumber, "TX", "McLennan");
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