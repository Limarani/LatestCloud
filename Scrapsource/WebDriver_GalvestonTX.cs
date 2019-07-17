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
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_GalvestonTX
    {
        IWebElement PaidCount;
        IWebDriver driver;
        DBconnection db = new DBconnection();
        List<string> link = new List<string>();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());


        public string FTP_Galveston(string houseno, string sname, string stype, string unitno, string direction, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            GlobalClass.sname = "TX";
            GlobalClass.cname = "Galveston";
            List<string> taxinformation = new List<string>();
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                // driver = new ChromeDriver();
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("https://propaccess.trueautomation.com/clientdb/?cid=81");
                    var SelectY = driver.FindElement(By.Id("propertySearchOptions_taxyear"));
                    var selectElementY = new SelectElement(SelectY);
                    selectElementY.SelectByIndex(1);
                    driver.FindElement(By.Id("propertySearchOptions_advanced")).Click();
                    Thread.Sleep(4000);

                    if (searchType == "titleflex")
                    {
                        string titleaddress = houseno + " " + sname + " " + stype + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "TX", "Galveston");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_GalvestonTX"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        //  var Select1 = driver.FindElement(By.Id("propertySearchOptions_searchType"));
                        //  var selectElement11 = new SelectElement(Select1);
                        //   selectElement11.SelectByText("Property Address");
                        driver.FindElement(By.Id("propertySearchOptions_streetNumber")).SendKeys(houseno);
                        driver.FindElement(By.Id("propertySearchOptions_streetName")).SendKeys(sname);
                        gc.CreatePdf_WOP(orderNumber, "address search", driver, "TX", "Galveston");
                        driver.FindElement(By.Id("propertySearchOptions_searchAdv")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "address search result", driver, "TX", "Galveston");
                    }
                    if (searchType == "parcel")
                    {
                        //   var Select1 = driver.FindElement(By.Id("propertySearchOptions_searchType"));
                        //   var selectElement11 = new SelectElement(Select1);
                        //   selectElement11.SelectByText("Account Number");
                        driver.FindElement(By.Id("propertySearchOptions_propertyid")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search", driver, "TX", "Galveston");
                        driver.FindElement(By.Id("propertySearchOptions_searchAdv")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search result", driver, "TX", "Galveston");
                    }

                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("propertySearchOptions_ownerName")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "owner search", driver, "TX", "Galveston");
                        driver.FindElement(By.Id("propertySearchOptions_searchAdv")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "owner search result", driver, "TX", "Galveston");
                    }
                    if (searchType == "block")
                    {
                        //var Select1 = driver.FindElement(By.Id("propertySearchOptions_searchType"));
                        //var selectElement11 = new SelectElement(Select1);
                        //selectElement11.SelectByText("Account Number");
                        driver.FindElement(By.Id("propertySearchOptions_geoid")).SendKeys(unitno);
                        gc.CreatePdf_WOP(orderNumber, "Account Number Search ", driver, "TX", "Galveston");
                        driver.FindElement(By.Id("propertySearchOptions_searchAdv")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Account Number Search  result", driver, "TX", "Galveston");
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
                                if (TDmulti5.Count == 10 && TDmulti5[3].Text.Contains("Real"))
                                {
                                    string multi1 = TDmulti5[2].Text + "~" + TDmulti5[3].Text + "~" + TDmulti5[4].Text + "~" + TDmulti5[6].Text;
                                    gc.insert_date(orderNumber, TDmulti5[1].Text, 1099, multi1, 1, DateTime.Now);
                                }
                                maxCheck++;
                            }
                        }
                        if (TRmulti5.Count > 25)
                        {
                            HttpContext.Current.Session["multiParcel_Galveston_Multicount"] = "Maximum";
                        }
                        else
                        {
                            HttpContext.Current.Session["multiParcel_Galveston"] = "Yes";
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
                            HttpContext.Current.Session["Nodata_GalvestonTX"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    //property details
                    List<string> entity = new List<string>();
                    entity.AddRange(new string[] { "S17", "W02", "S16", "D08", "S12", "M08", "M13", "M16", "M20", "M22", "M27", "M30", "M31", "M32", "M33", "M43", "M44", "M45", "M46", "M52", "M54", "M55", "M56", "M57", "M58", "M59", "M66", "P09", "M14", "M15", "P05", "P06", "P07", "P08", "P10", "P12", "S18", "M100", "(E1)141" });
                    string PropertyID = "", GeographicID = "", Name = "", MailingAddress = "", OwnerID = "", LegalDescription = "", Neighborhood = "", Type = "", Address = "", NeighborhoodID = "", Exemptions = "", MapID = "", YearBuilt = "", Acres = "",
                    fulltext = "", property_use_code = "", property_use_dis = "", ownership = "", owner = "", Ownership = "", TotalValue = "", ValueDetails1 = "";
                    fulltext = driver.FindElement(By.XPath("//*[@id='propertyDetails']/table/tbody")).Text.Replace("\r\n", " ");
                    PropertyID = gc.Between(fulltext, "Account Property ID:", " Legal Description: ");
                    GeographicID = gc.Between(fulltext, "Geographic ID: ", "Agent Code: ");
                    Name = gc.Between(fulltext, "Owner Name:", "Owner ID:");
                    MailingAddress = gc.Between(fulltext, "Mailing Address:", " % Ownership:");
                    OwnerID = gc.Between(fulltext, "Owner ID:", "Mailing Address:");
                    LegalDescription = gc.Between(fulltext, "Legal Description:", "Geographic ID:");
                    Neighborhood = gc.Between(fulltext, "Neighborhood:", "Map ID:");
                    Type = gc.Between(fulltext, "Type:", "Property Use Code:");
                    Address = gc.Between(fulltext, "Location Address:", "Mapsco:");
                    NeighborhoodID = gc.Between(fulltext, "Neighborhood CD:", "Owner");
                    Exemptions = GlobalClass.After(fulltext, "Exemptions:");
                    MapID = gc.Between(fulltext, "Map ID:", "Neighborhood CD:");
                    property_use_code = gc.Between(fulltext, "Property Use Code:", "Property Use Description:");
                    property_use_dis = gc.Between(fulltext, "Property Use Description:", "Location Address:");
                    ownership = gc.Between(fulltext, "% Ownership:", "Exemptions:");
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
                    //Property ID~Geographic ID~Type~Property Use Code~Property Use Description~Legal Description~Property Address~Neighborhood~Neighborhood CD~Map ID~Owner Name~Mailing Address~Owner ID~% Ownership~Exemptions~Year Built~Acres
                    string property_details = PropertyID + "~" + GeographicID + "~" + Type + "~" + property_use_code + "~" + property_use_dis + "~" + LegalDescription + "~" + Address + "~" + Neighborhood + "~" + NeighborhoodID + "~" + MapID + "~" + Name + "~" + MailingAddress + "~" + OwnerID + "~" + ownership + "~" + Exemptions + "~" + YearBuilt + "~" + Acres;
                    gc.insert_date(orderNumber, PropertyID, 1075, property_details, 1, DateTime.Now);

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
                            gc.insert_date(orderNumber, PropertyID, 1085, ValueDetails, 1, DateTime.Now);

                        }
                    }
                    //Taxing Jurisdiction Details Table:
                    //Owner~% Ownership~Total Value~Entity~Description~Tax Rate~Appraised Value~Taxable Value~Estimated Tax
                    driver.FindElement(By.Id("taxingJurisdiction")).Click();
                    Thread.Sleep(2000);
                    string fulltext1 = driver.FindElement(By.XPath("//*[@id='taxingJurisdictionDetails']/table[1]/tbody")).Text.Replace("\r\n", " ");

                    owner = gc.Between(fulltext1, "Owner:", "% Ownership:");
                    Ownership = gc.Between(fulltext1, "% Ownership:", "Total Value:");
                    TotalValue = GlobalClass.After(fulltext1, "Total Value:");
                    ValueDetails1 = owner + "~" + Ownership + "~" + TotalValue + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                    gc.insert_date(orderNumber, PropertyID, 1086, ValueDetails1, 1, DateTime.Now);
                    string entityname = "";
                    List<string> strTaxSearch = new List<string>();
                    IWebElement multitableElement11 = driver.FindElement(By.XPath("//*[@id='taxingJurisdictionDetails']/table[2]/tbody"));
                    IList<IWebElement> multitableRow11 = multitableElement11.FindElements(By.TagName("tr"));
                    IList<IWebElement> multirowTD11;
                    foreach (IWebElement row in multitableRow11)
                    {
                        multirowTD11 = row.FindElements(By.TagName("td"));
                        if (multirowTD11.Count != 0)
                        {
                            if ((multirowTD11[2].Text.Trim() != "0.000000" && multirowTD11[2].Text.Trim() != "$0") && multirowTD11[2].Text.Trim() != "N/A" && multirowTD11[0].Text.Trim() != "" && entity.Any(str => str.Contains(multirowTD11[0].Text)))
                            {
                                strTaxSearch.Add(multirowTD11[0].Text);
                                entityname = multirowTD11[1].Text;
                            }
                            if (multirowTD11.Count != 0)
                            {
                                ValueDetails1 = " " + "~" + " " + "~" + " " + "~" + multirowTD11[0].Text.Trim() + "~" + multirowTD11[1].Text.Trim() + "~" + multirowTD11[2].Text.Trim() + "~" + multirowTD11[3].Text.Trim() + "~" + multirowTD11[4].Text.Trim() + "~" + multirowTD11[5].Text.Trim();
                                gc.insert_date(orderNumber, PropertyID, 1086, ValueDetails1, 1, DateTime.Now);

                            }
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
                            gc.insert_date(orderNumber, PropertyID, 1096, rollDetails, 1, DateTime.Now);
                        }
                    }
                    //deed history

                    driver.FindElement(By.Id("deedHistory")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, PropertyID, "property details", driver, "TX", "Galveston");
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    string accountno = "", longaccountno = "", jurd = "", pendingpayment = "", accountNo = "", AppraisalDistrict = "", OwnerName = "", OwnerAddress = "", PropertyAddress = "", legal = "", CurrentTax = "", CurrentAmount = "", PriorYearAmount = "", TotalAmount = "", LastPaymentAmount = "", LastPayer = "", LastPaymentDate = "", ActiveLawsuitsTax = "", GrossValue = "", LandValue = "", ImprovementValue = "", CappedValue = "", AgriculturalValue = "", ExemptionsTax = "",
                           market = "", tax_year = "";
                    try

                    {
                        driver.Navigate().GoToUrl("https://actweb.acttax.com/act_webdev/galveston/index.jsp");
                        Thread.Sleep(3000);
                        driver.FindElement(By.Id("sc4")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.Id("criteria")).SendKeys(PropertyID.Trim());
                        gc.CreatePdf(orderNumber, PropertyID, "tax search", driver, "TX", "Galveston");
                        driver.FindElement(By.Name("submit")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, PropertyID, "tax search result", driver, "TX", "Galveston");

                        driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td/table[4]/tbody/tr/td/table[2]/tbody/tr/td/table/tbody/tr/td[1]/h3/a")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, PropertyID, "tax search result details", driver, "TX", "Galveston");
                        //Jurisdiction Information Details Table: 
                        driver.FindElement(By.LinkText("Taxes Due Detail by Year and Jurisdiction")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, PropertyID, "Taxes Due Detail by Year and Jurisdiction", driver, "TX", "Galveston");
                        //Taxes Due Detail by Year Details Table: 
                        //Account No~Appr. Dist. No~Active Lawsuits~Year~Base Tax Due~Penalty, Interest, and ACC* Due(end of July )~Total Due July~Penalty, Interest, and ACC* Due(end of August )~Total Due August~Penalty, Interest, and ACC*Due(end of September)~Total Due September

                        //Year~Base Tax Due~Penalty, Interest, and ACC* Due~Total Due~Penalty, Interest, and ACC* Due1~Total Due1~Penalty, Interest, and ACC*Due2~Total Due2

                        IWebElement multitableElement31 = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td/table[4]/tbody/tr/td/center/table/tbody/tr/td/table"));
                        IList<IWebElement> multitableRow31 = multitableElement31.FindElements(By.TagName("tr"));
                        IList<IWebElement> multirowTD31;
                        foreach (IWebElement row in multitableRow31)
                        {
                            multirowTD31 = row.FindElements(By.TagName("td"));
                            if (multirowTD31.Count == 8 && !row.Text.Contains("Base Tax Due"))
                            {
                                string TaxesDue = multirowTD31[0].Text.Trim() + "~" + multirowTD31[1].Text.Trim() + "~" + multirowTD31[2].Text.Trim() + "~" + multirowTD31[3].Text.Trim() + "~" + multirowTD31[4].Text.Trim() + "~" + multirowTD31[5].Text.Trim() + "~" + multirowTD31[6].Text.Trim() + "~" + multirowTD31[7].Text.Trim();
                                gc.insert_date(orderNumber, PropertyID, 1097, TaxesDue, 1, DateTime.Now);
                            }
                            if (multirowTD31.Count == 1)
                            {
                                string TaxesDue = multirowTD31[0].Text.Trim() + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                gc.insert_date(orderNumber, PropertyID, 1097, TaxesDue, 1, DateTime.Now);

                            }
                            if (multirowTD31.Count == 4)
                            {
                                string TaxesDue = "" + "~" + "" + "~" + multirowTD31[1].Text.Trim() + "~" + "" + "~" + multirowTD31[2].Text.Trim() + "~" + "" + "~" + multirowTD31[3].Text.Trim() + "~" + "";
                                gc.insert_date(orderNumber, PropertyID, 1097, TaxesDue, 1, DateTime.Now);

                            }
                        }
                        driver.FindElement(By.LinkText("Return to the Previous Page")).Click();
                        Thread.Sleep(2000);
                        //Tax Payment Details Table: 

                        driver.FindElement(By.LinkText("Payment Information & Receipts")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, PropertyID, "Payment Information", driver, "TX", "Galveston");
                        //Account Number~Paid Date~Amount~Tax Year~Description~Paid By
                        string accountnos = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td/table[4]/tbody/tr/td/h3[2]")).Text.Replace("Account No.:", "");

                        IWebElement multitableElement32 = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td/table[4]/tbody/tr/td/table/tbody"));
                        IList<IWebElement> multitableRow32 = multitableElement32.FindElements(By.TagName("tr"));
                        IList<IWebElement> multirowTD32;
                        foreach (IWebElement row in multitableRow32)
                        {
                            multirowTD32 = row.FindElements(By.TagName("td"));
                            if (multirowTD32.Count != 0 && !row.Text.Contains("Receipt Date"))
                            {
                                string TaxesDue = accountnos + "~" + multirowTD32[0].Text.Trim() + "~" + multirowTD32[1].Text.Trim() + "~" + multirowTD32[2].Text.Trim() + "~" + multirowTD32[3].Text.Trim() + "~" + multirowTD32[4].Text.Trim();
                                gc.insert_date(orderNumber, PropertyID, 1098, TaxesDue, 1, DateTime.Now);
                            }
                        }

                        driver.FindElement(By.LinkText("Return to the Previous Page")).Click();
                        Thread.Sleep(2000);

                        string fullTaxeBill1 = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td/table[4]/tbody/tr[1]/td/table[2]/tbody")).Text.Replace("\r\n", " ");
                        accountno = gc.Between(fullTaxeBill1, "Account Number:", " Long Account Number:").Trim();
                        longaccountno = gc.Between(fullTaxeBill1, " Long Account Number:", "Address:").Trim();
                        OwnerAddress = gc.Between(fullTaxeBill1, "Address:", "Property Site Address:");
                        PropertyAddress = gc.Between(fullTaxeBill1, "Property Site Address:", " Legal Description:");
                        legal = gc.Between(fullTaxeBill1, "Legal Description:", " Current Tax Levy:");
                        CurrentTax = gc.Between(fullTaxeBill1, " Current Tax Levy:", "Current Amount Due:");
                        CurrentAmount = gc.Between(fullTaxeBill1, "Current Amount Due:", "Prior Year Amount Due:");
                        PriorYearAmount = gc.Between(fullTaxeBill1, "Prior Year Amount Due:", "Total Amount Due:");
                        TotalAmount = gc.Between(fullTaxeBill1, "Total Amount Due:", "Last Payment Amount for Current Year Taxes:");
                        LastPaymentAmount = gc.Between(fullTaxeBill1, "Last Payment Amount for Current Year Taxes:", "Last Payer for Current Year Taxes:");
                        LastPayer = gc.Between(fullTaxeBill1, "Last Payer for Current Year Taxes:", "Last Payment Date for Current Year Taxes:");
                        LastPaymentDate = gc.Between(fullTaxeBill1, "Last Payment Date for Current Year Taxes:", "Active Lawsuits:");
                        tax_year = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td/table[4]/tbody/tr[1]/td/table[1]/tbody")).Text;
                        tax_year = gc.Between(tax_year, " tax information for", ". All amounts");
                        ActiveLawsuitsTax = gc.Between(fullTaxeBill1, "Active Lawsuits:", "Pending Internet Payments");
                        pendingpayment = gc.Between(fullTaxeBill1, "Pending Internet Payments:", "Market Value:");
                        market = gc.Between(fullTaxeBill1, "Market Value:", "Land Value:");
                        LandValue = gc.Between(fullTaxeBill1, "Land Value:", "Improvement Value:");
                        ImprovementValue = gc.Between(fullTaxeBill1, "Improvement Value:", "Capped Value:");
                        CappedValue = gc.Between(fullTaxeBill1, "Capped Value:", "Agricultural Value:");
                        AgriculturalValue = gc.Between(fullTaxeBill1, "Agricultural Value:", "Exemptions:");
                        ExemptionsTax = gc.Between(fullTaxeBill1, "Exemptions:", "Exemption and Tax Rate Information");
                        try
                        {
                            if (fullTaxeBill1.Contains("Jurisdictions"))
                            {
                                ExemptionsTax = gc.Between(fullTaxeBill1, "Exemptions:", "Jurisdictions:");
                            }

                            string fullTaxeBill11 = driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr/td/table/tbody/tr[1]/td/table[2]/tbody")).Text;

                            jurd = GlobalClass.After(fullTaxeBill11, "Jurisdictions:");
                            jurd = GlobalClass.Before(jurd, "Taxes Due Detail by Year and Jurisdiction");
                            jurd = jurd.Trim();
                            jurd = jurd.Replace("\r\n", " , ");
                            //Tax Year~Account Number~Owner Information~Property Site Address~Legal Description~Current Tax Levy~Current Amount Due~Prior Year Amount Due~Total Amount Due~Last Payment Amount for Current Year Taxes~Last Payer for Current Year Taxes~Last Payment Date for Current Year Taxes~Active Lawsuits~Pending Payment~Land Value~Improvement Value~Capped Value~Agricultural Value~Exemptions~Tax Authority
                        }
                        catch { }
                        string taxbill = tax_year + "~" + accountno + "~" + longaccountno + "~" + OwnerAddress + "~" + PropertyAddress + "~" + legal + "~" + CurrentTax + "~" + CurrentAmount + "~" + PriorYearAmount + "~" + TotalAmount + "~" + LastPaymentAmount + "~" + LastPayer + "~" + LastPaymentDate + "~" + ActiveLawsuitsTax + "~" + pendingpayment + "~" + market + "~" + LandValue + "~" + ImprovementValue + "~" + CappedValue + "~" + AgriculturalValue + "~" + ExemptionsTax + "~" + jurd + "~" + "Postal Address: P O Box 1169 Galveston, TX 77553; Physical Address: 722 Moody Galveston, TX 77553 Telephone: 409-766-2285 Fax: 409-766-2479";
                        gc.insert_date(orderNumber, PropertyID, 1100, taxbill, 1, DateTime.Now);

                        IWebElement Itaxstmt1 = driver.FindElement(By.LinkText("Print a Current Tax Statement"));
                        Thread.Sleep(3000);
                        string stmt11 = Itaxstmt1.GetAttribute("href");
                        IWebElement Itaxdstmt1 = driver.FindElement(By.LinkText("Print a Delinquent Tax Statement"));
                        Thread.Sleep(3000);
                        string dstmt11 = Itaxdstmt1.GetAttribute("href");

                        try
                        {
                            driver.FindElement(By.LinkText("Exemption and Tax Rate Information")).Click();
                            Thread.Sleep(2000);
                            //Jurisdiction Information for ~Account No~Exemptions~Jurisdictions~Market Value~Exemption Value~Taxable Value~Tax Rate~Levy
                            string accountnosjur = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td/table[4]/tbody/tr/td/center/table/tbody/tr[1]/td/h3[2]")).Text.Replace("Account No.:", "");
                            string jurdinfoYear = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td/table[4]/tbody/tr/td/center/table/tbody/tr[1]/td/h3[1]/b")).Text.Replace("Jurisdiction Information for", "");
                            string exemptions = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td/table[4]/tbody/tr/td/center/table/tbody/tr[1]/td/h3[3]")).Text.Replace("Exemptions:", "");
                            gc.CreatePdf(orderNumber, PropertyID, "Jurisdictions Information", driver, "TX", "Galveston");
                            IWebElement multitableElement6 = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td/table[4]/tbody/tr/td/center/table/tbody/tr[2]/td/table/tbody"));
                            IList<IWebElement> multitableRow6 = multitableElement6.FindElements(By.TagName("tr"));
                            IList<IWebElement> multirowTD6;
                            foreach (IWebElement row in multitableRow6)
                            {
                                multirowTD6 = row.FindElements(By.TagName("td"));
                                if (multirowTD6.Count != 0 && !row.Text.Contains("Jurisdictions"))
                                {
                                    string TaxesDue = jurdinfoYear + "~" + accountnosjur + "~" + exemptions + "~" + multirowTD6[0].Text.Trim() + "~" + multirowTD6[1].Text.Trim() + "~" + multirowTD6[2].Text.Trim() + "~" + multirowTD6[3].Text.Trim() + "~" + multirowTD6[4].Text.Trim() + "~" + multirowTD6[5].Text.Trim();
                                    gc.insert_date(orderNumber, PropertyID, 1087, TaxesDue, 1, DateTime.Now);
                                }
                            }
                        }
                        catch { }

                        driver.Navigate().GoToUrl(stmt11);
                        Thread.Sleep(6000);
                        IWebElement Itaxstmt = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td/table[4]/tbody/tr[2]/td/div/h3/a"));
                        string stmt1 = Itaxstmt.GetAttribute("href");
                        gc.downloadfile(stmt1, orderNumber, PropertyID, "Tax statement", "TX", "Galveston");

                        driver.Navigate().GoToUrl(dstmt11);
                        Thread.Sleep(6000);
                        IWebElement dItaxstmt = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td/table[4]/tbody/tr[3]/td/div/h3/a"));
                        string dstmt1 = dItaxstmt.GetAttribute("href");
                        gc.downloadfile(dstmt1, orderNumber, PropertyID, "Tax Delinquent statement", "TX", "Galveston");

                    }
                    catch { }
                    int GalScenario1 = 0, GalScenario2 = 0, GalScenario3 = 0, GalScenario4 = 0, GalScenario5 = 0, GalScenario6 = 0, GalScenario7 = 0, GalScenario8 = 0, GalScenario9 = 0;
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    foreach (string msg1 in strTaxSearch)
                    {
                        //SCENARIO - 1 Clear Creek ISD Tax Office (CLEAR CREEK ISD) and SCENARIO - 5 Santa Fe ISD Tax Office
                        if ((msg1 == "S17" || msg1 == "W02" || msg1 == "S16") && GalScenario1 < 1)
                        {
                            var chromeOptions = new ChromeOptions();
                            var chDriver = new ChromeDriver(chromeOptions);
                            try
                            {
                                string strTaxAuthority = "", strAccount = "", strOwnerName = "", strAddress = "", strPropertyAddress = "", strLegal = "", strCAD = "", strCompany = "", strType = "", strAcres = "", ISDTitle = "", ISDValue = "";
                                if (msg1 == "S17" || msg1 == "W02")
                                {
                                    //Tax Authority
                                    try
                                    {
                                        strTaxAuthority = "4133 Warpath P O Box 370, Santa Fe, Tx 77510, Phone 409-925-3526, Fax 409-925-4002";
                                    }
                                    catch { }
                                }
                                if (msg1 == "S16")
                                {
                                    //Creek Tax Authority
                                    try
                                    {
                                        strTaxAuthority = "2425 East Main Street, League City, Texas 77573, Phone: 281-284-0000, Fax: 281-284-9901";
                                    }
                                    catch { }
                                }
                                if (msg1 == "S17" || msg1 == "W02")
                                {
                                    chDriver.Navigate().GoToUrl("https://texaspayments.com/Home/AccountSearch?officeId=084909");
                                }
                                if (msg1 == "S16")
                                {
                                    //Creek
                                    chDriver.Navigate().GoToUrl("https://texaspayments.com/Home/AccountSearch?officeId=084910");
                                }
                                IWebElement IParcelClick = chDriver.FindElement(By.XPath("//*[@id='ddlSearch']/span"));
                                IParcelClick.Click();
                                IParcelClick.SendKeys("Search by CAD Number");
                                Thread.Sleep(6000);
                                chDriver.FindElement(By.Id("searchValue")).SendKeys(GeographicID.Trim());
                                gc.CreatePdf_Chrome(orderNumber, parcelNumber, "School Parcel Search", chDriver, "TX", "Galveston");
                                IWebElement IClick = chDriver.FindElement(By.Id("searchBtn"));
                                IJavaScriptExecutor js = chDriver as IJavaScriptExecutor;
                                js.ExecuteScript("arguments[0].click();", IClick);
                                Thread.Sleep(5000);
                                try
                                {
                                    driver.FindElement(By.Id("//*[@id='accountInfo']/table/tbody/tr/td[1]/div")).SendKeys(Keys.Enter);
                                }
                                catch { }

                                //Creek Property Details
                                IWebElement Iaccount = chDriver.FindElement(By.XPath("//*[@id='AccountDetail']/div/div[2]/div[1]"));
                                strAccount = gc.Between(Iaccount.Text, "Account Number:", "Owner Name:");
                                strOwnerName = gc.Between(Iaccount.Text, "Owner Name:", "Address:");
                                strAddress = GlobalClass.After(Iaccount.Text, "Address:").Replace("\r\n", "");
                                IWebElement ICAD = chDriver.FindElement(By.XPath("//*[@id='AccountDetail']/div/div[2]/div[2]"));
                                strCAD = gc.Between(ICAD.Text, "CAD Number:", "Mortgage Company:");
                                strCompany = gc.Between(ICAD.Text, "Mortgage Company:", "Property Type:");
                                strType = gc.Between(ICAD.Text, "Property Type:", "Acres:");
                                strAcres = GlobalClass.After(ICAD.Text, "Acres:").Replace("\r\n", "");
                                IWebElement Iproperty = chDriver.FindElement(By.XPath("//*[@id='AccountDetail']/div/div[2]/div[3]"));
                                strPropertyAddress = gc.Between(Iproperty.Text, "Property Address:", "Legal Description:");
                                strLegal = GlobalClass.After(Iproperty.Text, "Legal Description:").Replace("\r\n", " ");
                                gc.CreatePdf_Chrome(orderNumber, parcelNumber, "School Parcel Property", chDriver, "TX", "Galveston");
                                string CreekProperty = strAccount.Replace("\r\n", " ").Trim() + "~" + strOwnerName.Replace("\r\n", " ").Trim() + "~" + strAddress.Replace("\r\n", " ").Trim() + "~" + strCAD.Replace("\r\n", " ").Trim() + "~" + strCompany.Replace("\r\n", " ").Trim() + "~" + strType.Replace("\r\n", " ").Trim() + "~" + strAcres.Replace("\r\n", " ").Trim() + "~" + strPropertyAddress.Replace("\r\n", " ").Trim() + "~" + strLegal.Replace("\r\n", " ").Trim() + "~" + strTaxAuthority.Replace("\r\n", " ").Trim();
                                gc.insert_date(orderNumber, PropertyID, 1102, CreekProperty, 1, DateTime.Now);

                                try
                                {
                                    IJavaScriptExecutor js1 = (IJavaScriptExecutor)chDriver;
                                    IWebElement IElement1 = chDriver.FindElement(By.XPath("//*[@id='AccountDetail']/div/div[1]/div[2]/div/div[6]"));
                                    js1.ExecuteScript("arguments[0].scrollIntoView();", IElement1);
                                    gc.CreatePdf_Chrome(orderNumber, parcelNumber, "School Payment Assessment Property", chDriver, "TX", "Galveston");
                                    IWebElement IElement = chDriver.FindElement(By.XPath("//*[@id='AccountBalance']/div[1]/div[5]"));
                                    js1.ExecuteScript("arguments[0].scrollIntoView();", IElement);
                                    gc.CreatePdf_Chrome(orderNumber, parcelNumber, "School Payment Assessment", chDriver, "TX", "Galveston");
                                }
                                catch { }
                                //Creek Due Details 
                                IWebElement IDueTable = chDriver.FindElement(By.Id("AccountInfoGrid"));
                                IList<IWebElement> IDueRow = IDueTable.FindElements(By.TagName("tr"));
                                IList<IWebElement> IDueTD;
                                foreach (IWebElement Due in IDueRow)
                                {
                                    IDueTD = Due.FindElements(By.TagName("td"));
                                    if (IDueTD.Count != 0)
                                    {
                                        string strDueDetails = IDueTD[1].Text + "~" + IDueTD[2].Text + "~" + IDueTD[3].Text;
                                        gc.insert_date(orderNumber, PropertyID, 1103, strDueDetails, 1, DateTime.Now);
                                    }
                                }

                                //Creek ISD Tax Details
                                IWebElement IISDTable = chDriver.FindElement(By.Id("SubYearlyDetail_2018"));
                                IList<IWebElement> IISDRow = IISDTable.FindElements(By.TagName("div"));
                                foreach (IWebElement ISD in IISDRow)
                                {
                                    if ((ISD.Text.Contains("Total Market Value:") && ISD.Text.Contains("Tax Rate:")) && (ISD.Text.Contains("Certified Levy:") && ISD.Text.Contains("Last Pay Date:")))
                                    {
                                        string[] ISDSplit = ISD.Text.Split('\r');
                                        for (int i = 0; i < ISDSplit.Count(); i++)
                                        {
                                            if (ISDSplit[i].Contains(":") && i < ISDSplit.Count())
                                            {
                                                ISDTitle += ISDSplit[i].Replace("\n", "").Replace(":", "") + "~";
                                                try
                                                {
                                                    if (!ISDSplit[i + 1].Contains(":"))
                                                    {
                                                        ISDValue += ISDSplit[i + 1].Replace("\n", "") + "~";
                                                    }
                                                    if (ISDSplit[i + 1].Contains(":"))
                                                    {
                                                        ISDValue += "~";
                                                        ISDTitle += ISDSplit[i + 1].Replace("\n", "").Replace(":", "") + "~";
                                                    }
                                                }
                                                catch { }
                                            }
                                            i++;
                                        }
                                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + ISDTitle.Remove(ISDTitle.Length - 1, 1) + "' where Id = '" + 1104 + "'");
                                        gc.insert_date(orderNumber, PropertyID, 1104, ISDValue.Remove(ISDValue.Length - 1, 1), 1, DateTime.Now);
                                    }
                                }

                                //Payment History
                                IWebElement IPayment = chDriver.FindElement(By.XPath("//*[@id='AccountDetail']/div/div[1]"));
                                IList<IWebElement> IPaymentRow = IPayment.FindElements(By.TagName("div"));
                                foreach (IWebElement payment in IPaymentRow)
                                {
                                    try
                                    {
                                        string strPayment = payment.GetAttribute("data-id");
                                        if (strPayment.Contains("PrintReceipt"))
                                        {
                                            payment.Click();
                                            Thread.Sleep(2000);
                                            break;
                                        }
                                    }
                                    catch { }
                                }
                                // driver.FindElement(By.LinkText("Payment History &  Print Receipt")).SendKeys(Keys.Enter);
                                string Current = chDriver.CurrentWindowHandle;
                                gc.CreatePdf_Chrome(orderNumber, parcelNumber, "School Payment History", chDriver, "TX", "Galveston");
                                int Billcount = 0;
                                IWebElement IPayTable = chDriver.FindElement(By.Id("PrintReceiptGrid"));
                                IList<IWebElement> IPayRow = IPayTable.FindElements(By.TagName("tr"));
                                IList<IWebElement> IPayTD;
                                foreach (IWebElement Pay in IPayRow)
                                {
                                    IPayTD = Pay.FindElements(By.TagName("td"));
                                    if (IPayTD.Count != 0)
                                    {
                                        string strPayDetails = IPayTD[1].Text + "~" + IPayTD[2].Text + "~" + IPayTD[3].Text + "~" + IPayTD[4].Text + "~" + IPayTD[5].Text + "~" + IPayTD[6].Text + "~" + IPayTD[7].Text + "~" + IPayTD[8].Text + "~" + IPayTD[9].Text;
                                        gc.insert_date(orderNumber, PropertyID, 1105, strPayDetails, 1, DateTime.Now);
                                    }
                                    if (IPayTD.Count != 0 && Billcount < 3)
                                    {
                                        IPayTD[1].Click();
                                        chDriver.SwitchTo().Window(chDriver.WindowHandles.Last());
                                        try
                                        {
                                            gc.downloadfile(chDriver.Url, orderNumber, PropertyID, "Tax Bill" + Billcount, "TX", "Galveston");
                                        }
                                        catch { }
                                        //gc.CreatePdf(orderNumber, parcelNumber, "Bill" + Billcount, chDriver, "TX", "Galveston");
                                        Billcount++;
                                        chDriver.SwitchTo().Window(Current);
                                    }
                                }
                                chDriver.Quit();
                            }
                            catch
                            {
                                chDriver.Quit();
                            }
                            GalScenario1++;
                        }

                        //SCENARIO - 2 Texas City ISD Tax Office
                        if ((msg1 == "S18") && GalScenario2 < 1)
                        {
                            try
                            {
                                driver.Navigate().GoToUrl("https://actweb.acttax.com/act_webdev/texascity/index.jsp");
                                Thread.Sleep(2000);

                                string ISDTaxauthority = "";
                                try
                                {
                                    IWebElement Taxauthorit = driver.FindElement(By.XPath("/html/body/div[2]/table/tbody/tr/td/div/table/tbody/tr[3]/td/font[1]"));
                                    ISDTaxauthority = GlobalClass.Before(Taxauthorit.Text, "You may also e-mail the office.").Trim();

                                }
                                catch { }

                                driver.FindElement(By.Id("sc4")).Click();
                                driver.FindElement(By.Id("criteria")).SendKeys(GeographicID.Replace("-", "").Trim());
                                gc.CreatePdf(orderNumber, parcelNumber, "Enter The Parcel Number Before", driver, "TX", "Galveston");
                                driver.FindElement(By.XPath("/html/body/div[2]/table/tbody/tr/td/div/table/tbody/tr[1]/td/table/tbody/tr/td/table[2]/tbody/tr/td/center/form/table/tbody/tr[5]/td[2]/h3[2]/input")).SendKeys(Keys.Enter);
                                Thread.Sleep(2000);
                                gc.CreatePdf(orderNumber, parcelNumber, "Enter The Parcel Number After", driver, "TX", "Galveston");

                                try
                                {
                                    string parcelclick = driver.FindElement(By.XPath("//*[@id='data-block']/table/tbody/tr/td/table/tbody/tr/td[1]/h3/a")).GetAttribute("href");
                                    driver.Navigate().GoToUrl(parcelclick);
                                    Thread.Sleep(5000);

                                }
                                catch { }
                                gc.CreatePdf(orderNumber, parcelNumber, "Texas City ISD Tax information", driver, "TX", "Galveston");


                                //Tax Bill Details Table:
                                string ISDaccountno = "", OwnerNameAndOwnerAddress = "", ISDPropertyAddress = "", ISDlegal = "", ISDCurrentTax = "", ISDCurrentAmount = "", ISDPriorYearAmount = "", ISDTotalAmount = "", ISDLastPaymentAmount = "", ISDLastPayer = "", ISDLastPaymentDate = "", ISDActiveLawsuitsTax = "", ISDGrossValue = "", ISDLandValue = "", ISDImprovementValue = "", ISDCappedValue = "", ISDAgriculturalValue = "", ISDExemptionsTax = "";
                                string fullTaxeBill1 = driver.FindElement(By.XPath("/html/body/div[2]/table/tbody/tr/td/div/table/tbody/tr[1]/td/table/tbody/tr[2]/td/table[2]/tbody")).Text.Replace("\r\n", " ");

                                ISDaccountno = gc.Between(fullTaxeBill1, "Account Number:", "Address:");
                                OwnerNameAndOwnerAddress = gc.Between(fullTaxeBill1, "Address:", "Property Site Address:");
                                ISDPropertyAddress = gc.Between(fullTaxeBill1, "Property Site Address:", " Legal Description:");
                                ISDlegal = gc.Between(fullTaxeBill1, "Legal Description:", " Current Tax Levy:");
                                ISDCurrentTax = gc.Between(fullTaxeBill1, " Current Tax Levy:", "Current Amount Due:");
                                ISDCurrentAmount = gc.Between(fullTaxeBill1, "Current Amount Due:", "Prior Year Amount Due:");
                                ISDPriorYearAmount = gc.Between(fullTaxeBill1, "Prior Year Amount Due:", "Total Amount Due:");
                                ISDTotalAmount = gc.Between(fullTaxeBill1, "Total Amount Due:", "Last Payment Amount for Current Year Taxes:");
                                ISDLastPaymentAmount = gc.Between(fullTaxeBill1, "Last Payment Amount for Current Year Taxes:", "Last Payer for Current Year Taxes:");
                                ISDLastPayer = gc.Between(fullTaxeBill1, "Last Payer for Current Year Taxes:", "Last Payment Date for Current Year Taxes:");
                                ISDLastPaymentDate = gc.Between(fullTaxeBill1, "Last Payment Date for Current Year Taxes:", "Active Lawsuits:");
                                ISDActiveLawsuitsTax = gc.Between(fullTaxeBill1, "Active Lawsuits:", "Pending Credit Card or E–Check Payments:");

                                ISDGrossValue = gc.Between(fullTaxeBill1, "Gross Value:", "Land Value:");
                                ISDLandValue = gc.Between(fullTaxeBill1, "Land Value:", "Improvement Value:");
                                ISDImprovementValue = gc.Between(fullTaxeBill1, "Improvement Value:", "Capped Value:");
                                ISDCappedValue = gc.Between(fullTaxeBill1, "Capped Value:", "Agricultural Value:");
                                ISDAgriculturalValue = gc.Between(fullTaxeBill1, "Agricultural Value:", "Exemptions:");
                                ISDExemptionsTax = gc.Between(fullTaxeBill1, "Exemptions:", "Exemption and Tax Rate Information");

                                string taxbill = ISDaccountno + "~" + OwnerNameAndOwnerAddress + "~" + ISDPropertyAddress + "~" + ISDlegal + "~" + ISDCurrentTax + "~" + ISDCurrentAmount + "~" + ISDPriorYearAmount + "~" + ISDTotalAmount + "~" + ISDLastPaymentAmount + "~" + ISDLastPayer + "~" + ISDLastPaymentDate + "~" + ISDActiveLawsuitsTax + "~" + ISDGrossValue + "~" + ISDLandValue + "~" + ISDImprovementValue + "~" + ISDCappedValue + "~" + ISDAgriculturalValue + "~" + ISDExemptionsTax + "~" + ISDTaxauthority;
                                gc.insert_date(orderNumber, ISDaccountno, 1106, taxbill, 1, DateTime.Now);

                                //Jurisdiction Information Details Table: 

                                try
                                {
                                    IWebElement Jurisdictionclick = driver.FindElement(By.XPath("/html/body/div[2]/table/tbody/tr/td/div/table/tbody/tr[1]/td/table/tbody/tr[2]/td/table[2]/tbody/tr/td[2]/h3[9]/a[1]"));
                                    IJavaScriptExecutor js11 = driver as IJavaScriptExecutor;
                                    js11.ExecuteScript("arguments[0].click();", Jurisdictionclick);
                                    Thread.Sleep(5000);
                                    gc.CreatePdf(orderNumber, parcelNumber, "Jurisdiction Information Pdf", driver, "TX", "Galveston");

                                }
                                catch { }

                                string accountnosjur = driver.FindElement(By.XPath("/html/body/div[2]/table/tbody/tr/td/div/table/tbody/tr[1]/td/table/tbody/tr/td/table/tbody/tr[1]/td/h3[2]")).Text.Replace("Account No.:", "");
                                string jurdinfoYear = driver.FindElement(By.XPath("/html/body/div[2]/table/tbody/tr/td/div/table/tbody/tr[1]/td/table/tbody/tr/td/table/tbody/tr[1]/td/h3[1]")).Text.Replace("Jurisdiction Information for", "");
                                string exemptions = driver.FindElement(By.XPath("/html/body/div[2]/table/tbody/tr/td/div/table/tbody/tr[1]/td/table/tbody/tr/td/table/tbody/tr[1]/td/h3[3]")).Text.Replace("Exemptions:", "");

                                IWebElement Jurisdictiontable = driver.FindElement(By.XPath("/html/body/div[2]/table/tbody/tr/td/div/table/tbody/tr[1]/td/table/tbody/tr/td/table/tbody/tr[2]"));
                                IList<IWebElement> multitableRow6 = Jurisdictiontable.FindElements(By.TagName("tr"));
                                IList<IWebElement> multirowTD6;
                                foreach (IWebElement row in multitableRow6)
                                {
                                    multirowTD6 = row.FindElements(By.TagName("td"));
                                    if (multirowTD6.Count != 0 && !row.Text.Contains("Jurisdictions"))
                                    {
                                        string Jurisdictiondetailsinsert = jurdinfoYear + "~" + accountnosjur + "~" + exemptions + "~" + multirowTD6[0].Text.Trim() + "~" + multirowTD6[1].Text.Trim() + "~" + multirowTD6[2].Text.Trim() + "~" + multirowTD6[3].Text.Trim() + "~" + multirowTD6[4].Text.Trim() + "~" + multirowTD6[5].Text.Trim();
                                        gc.insert_date(orderNumber, ISDaccountno, 1107, Jurisdictiondetailsinsert, 1, DateTime.Now);
                                    }
                                }


                                driver.FindElement(By.LinkText("Return to the Previous Page")).Click();
                                Thread.Sleep(2000);



                                //Texas City ISD Tax Due Detail by Year Details Table: 

                                try
                                {
                                    IWebElement Taxduedetailbyyear = driver.FindElement(By.XPath("/html/body/div[2]/table/tbody/tr/td/div/table/tbody/tr[1]/td/table/tbody/tr[2]/td/table[2]/tbody/tr/td[2]/h3[9]/a[2]"));
                                    IJavaScriptExecutor js11 = driver as IJavaScriptExecutor;
                                    js11.ExecuteScript("arguments[0].click();", Taxduedetailbyyear);
                                    Thread.Sleep(5000);
                                    gc.CreatePdf(orderNumber, parcelNumber, "Taxes Due Detail by Year and Jurisdiction", driver, "TX", "Galveston");
                                }
                                catch { }

                                string taxdueaccountnosjur = driver.FindElement(By.XPath("/html/body/div[2]/table/tbody/tr/td/div/table/tbody/tr[1]/td/table/tbody/tr/td/center/table/tbody/tr/td/div/h3[2]")).Text.Replace("Account No.:", "");
                                string lawsuits = driver.FindElement(By.XPath("/html/body/div[2]/table/tbody/tr/td/div/table/tbody/tr[1]/td/table/tbody/tr/td/center/table/tbody/tr/td/div/h3[3]")).Text.Replace("Active Lawsuits", "");

                                IWebElement Taxduedetailtable = driver.FindElement(By.XPath("/html/body/div[2]/table/tbody/tr/td/div/table/tbody/tr[1]/td/table/tbody/tr/td/center/table/tbody/tr/td/table/tbody"));
                                IList<IWebElement> multitableRow31 = Taxduedetailtable.FindElements(By.TagName("tr"));
                                IList<IWebElement> multirowTD31;
                                foreach (IWebElement row in multitableRow31)
                                {
                                    multirowTD31 = row.FindElements(By.TagName("td"));
                                    if (multirowTD31.Count == 8 && !row.Text.Contains("Base Tax Due"))
                                    {
                                        string TaxesDuedetailsinsert = multirowTD31[0].Text.Trim() + "~" + multirowTD31[1].Text.Trim() + "~" + multirowTD31[2].Text.Trim() + "~" + multirowTD31[3].Text.Trim() + "~" + multirowTD31[4].Text.Trim() + "~" + multirowTD31[5].Text.Trim() + "~" + multirowTD31[6].Text.Trim() + "~" + multirowTD31[7].Text.Trim();
                                        gc.insert_date(orderNumber, ISDaccountno, 1108, TaxesDuedetailsinsert, 1, DateTime.Now);
                                    }
                                    if (multirowTD31.Count == 1)
                                    {
                                        string TaxesDuedetailsinsert = multirowTD31[0].Text.Trim() + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                        gc.insert_date(orderNumber, ISDaccountno, 1108, TaxesDuedetailsinsert, 1, DateTime.Now);

                                    }
                                    if (multirowTD31.Count == 4)
                                    {
                                        string TaxesDuedetailsinsert = "" + "~" + "" + "~" + multirowTD31[1].Text.Trim() + "~" + "" + "~" + multirowTD31[2].Text.Trim() + "~" + "" + "~" + multirowTD31[3].Text.Trim() + "~" + "";
                                        gc.insert_date(orderNumber, ISDaccountno, 1108, TaxesDuedetailsinsert, 1, DateTime.Now);

                                    }
                                }


                                driver.FindElement(By.LinkText("Return to the Previous Page")).Click();
                                Thread.Sleep(2000);
                                //Tax Payment Details Table: 
                                IWebElement ICurrentTax = driver.FindElement(By.LinkText("Print a Current Tax Statement"));
                                string strCurrent = ICurrentTax.GetAttribute("href");
                                driver.FindElement(By.LinkText("Payment Information")).Click();
                                Thread.Sleep(2000);
                                gc.CreatePdf(orderNumber, parcelNumber, "Payment Information", driver, "TX", "Galveston");
                                string accountnos = driver.FindElement(By.XPath("/html/body/div[2]/table/tbody/tr/td/div/table/tbody/tr[1]/td/table/tbody/tr/td/h3[4]")).Text.Replace("Account No.:", "");
                                IWebElement Paymentdetails = driver.FindElement(By.XPath("/html/body/div[2]/table/tbody/tr/td/div/table/tbody/tr[1]/td/table/tbody/tr/td/table"));
                                IList<IWebElement> multitableRow32 = Paymentdetails.FindElements(By.TagName("tr"));
                                IList<IWebElement> multirowTD32;
                                foreach (IWebElement row in multitableRow32)
                                {
                                    multirowTD32 = row.FindElements(By.TagName("td"));
                                    if (multirowTD32.Count != 0 && !row.Text.Contains("Receipt Date"))
                                    {
                                        string Taxepaymentdetailsinsert = accountnos + "~" + multirowTD32[0].Text.Trim() + "~" + multirowTD32[1].Text.Trim() + "~" + multirowTD32[2].Text.Trim() + "~" + multirowTD32[3].Text.Trim() + "~" + multirowTD32[4].Text.Trim();
                                        gc.insert_date(orderNumber, ISDaccountno, 1109, Taxepaymentdetailsinsert, 1, DateTime.Now);
                                    }
                                }

                                var chromeOptions = new ChromeOptions();
                                var chDriver = new ChromeDriver(chromeOptions);
                                try
                                {
                                    chDriver.Navigate().GoToUrl(strCurrent);
                                    Thread.Sleep(3000);
                                    IWebElement Istatement = chDriver.FindElement(By.XPath("/html/body/div[2]/table/tbody/tr/td/div/table/tbody/tr[1]/td/table/tbody/tr[3]/td/div/h3/a"));
                                    string strstatement = Istatement.GetAttribute("href");
                                    try
                                    {
                                        gc.downloadfile(strstatement, orderNumber, PropertyID, "Current Tax statement1", "TX", "Galveston");
                                    }
                                    catch { }
                                    chDriver.Quit();
                                }
                                catch
                                {
                                    chDriver.Quit();
                                }
                            }
                            catch { }
                            GalScenario2++;
                        }

                        //SCENARIO - 3 Friendswood ISD Tax Office
                        if ((msg1 == "D08" || msg1 == "S12") && GalScenario3 < 1)
                        {
                            try
                            {
                                driver.Navigate().GoToUrl("http://friendswoodtax.azurewebsites.net/search/list");
                                Thread.Sleep(2000);
                                driver.FindElement(By.Id("tag")).SendKeys(GeographicID.Replace("-", "").Trim());
                                gc.CreatePdf(orderNumber, PropertyID, " SD3-Property Search", driver, "TX", "Galveston");
                                driver.FindElement(By.XPath("//*[@id='tag']")).SendKeys(Keys.Enter);
                                Thread.Sleep(2000);
                                gc.CreatePdf(orderNumber, PropertyID, " SD3-Property Search Result", driver, "TX", "Galveston");
                                IWebElement SD_property3 = driver.FindElement(By.XPath("//*[@id='searchlist']/div/div/div/div/table"));
                                IList<IWebElement> TRSD_property3 = SD_property3.FindElements(By.TagName("tr"));
                                IList<IWebElement> THSD_property3 = SD_property3.FindElements(By.TagName("th"));
                                IList<IWebElement> TDSD_property3;
                                foreach (IWebElement row in TRSD_property3)
                                {
                                    TDSD_property3 = row.FindElements(By.TagName("td"));
                                    THSD_property3 = row.FindElements(By.TagName("th"));
                                    if (TDSD_property3.Count != 0 && !row.Text.Contains(""))
                                    {
                                    }
                                }

                                IWebElement ITaxClick = driver.FindElement(By.XPath("//*[@id='searchlist']/div/div/div/div/table"));
                                IList<IWebElement> ITaxRow = ITaxClick.FindElements(By.TagName("tr"));
                                foreach (IWebElement tax in ITaxRow)
                                {
                                    if (tax.Text.Contains(PropertyID.Trim().Replace("-", "")))
                                    {
                                        string strClick = tax.GetAttribute("id");
                                        if (strClick.Contains("rowid"))
                                        {
                                            IWebElement Iclick = driver.FindElement(By.Id(strClick));
                                            Iclick.Click();
                                            Thread.Sleep(2000);
                                        }
                                    }
                                }

                                // Tax Details (Tax Summary)

                                string TaxSummary = driver.FindElement(By.XPath("//*[@id='cardlist']/div/div[1]/div/div[2]/div[1]/div/table")).Text;
                                string PropertyOwnership = "", PropId = "", GeoId = "", Situs = "", LegalDesc = "", TaxAuthority = "";
                                PropertyOwnership = gc.Between(TaxSummary, "PROPERTY OWNERSHIP", "PROP ID:");
                                PropId = gc.Between(TaxSummary, "PROP ID:", "GEOID:").Trim();
                                GeoId = GlobalClass.After(TaxSummary, "GEOID:");
                                string Taxsummary1 = driver.FindElement(By.XPath("//*[@id='cardlist']/div/div[1]/div/div[3]/div[1]/div/table/tbody")).Text;
                                Situs = GlobalClass.After(Taxsummary1, "SITUS:").Trim();
                                string TaxSummary2 = driver.FindElement(By.XPath("//*[@id='cardlist']/div/div[1]/div/div[3]/div[2]/div/table")).Text;
                                LegalDesc = GlobalClass.After(TaxSummary2, "LEGAL DESCRIPTION").Trim();
                                TaxAuthority = "Kimberly Patterson, RTA Tax-Assessor Collector 402 Laurel Friendswood, TX 77546 Phone: (281)482-1198";
                                string TaxDetails = PropertyOwnership + "~" + PropId + "~" + GeoId + "~" + Situs + "~" + LegalDesc + "~" + TaxAuthority;
                                gc.insert_date(orderNumber, PropertyID, 1110, TaxDetails, 1, DateTime.Now);
                                gc.CreatePdf(orderNumber, PropertyID, "SD3-Tax Summary", driver, "TX", "Galveston");

                                // Payment Details Table
                                IWebElement TaxPayment = driver.FindElement(By.XPath("//*[@id='cardlist']/div/div[1]/div/div[2]/div[3]/div/table"));
                                IList<IWebElement> TRTaxPayment = TaxPayment.FindElements(By.TagName("tr"));
                                IList<IWebElement> THTaxPayment = TaxPayment.FindElements(By.TagName("th"));
                                IList<IWebElement> TDTaxPayment;
                                foreach (IWebElement row in TRTaxPayment)
                                {
                                    TDTaxPayment = row.FindElements(By.TagName("td"));
                                    THTaxPayment = row.FindElements(By.TagName("th"));
                                    if (TDTaxPayment.Count != 0 && !row.Text.Contains("AMOUNT DUE"))
                                    {
                                        string TaxPaymentDetails = TDTaxPayment[0].Text + "~" + TDTaxPayment[1].Text + "~" + TDTaxPayment[2].Text;
                                        gc.insert_date(orderNumber, PropertyID, 1112, TaxPaymentDetails, 1, DateTime.Now);
                                    }
                                }

                                // Exemptions
                                string ExemptionDetails1 = "", ExemptionDetails2 = "", TaxValuation1 = "", TaxValuation2 = "";
                                try
                                {
                                    IWebElement TaxExemptions = driver.FindElement(By.XPath("//*[@id='cardlist']/div/div[1]/div/div[3]/div[3]/div/table"));
                                    IList<IWebElement> TRTaxExemptions = TaxExemptions.FindElements(By.TagName("tr"));
                                    IList<IWebElement> THTaxExemptions = TaxExemptions.FindElements(By.TagName("th"));
                                    IList<IWebElement> TDTaxExemptions;
                                    foreach (IWebElement row in TRTaxExemptions)
                                    {
                                        TDTaxExemptions = row.FindElements(By.TagName("td"));
                                        THTaxExemptions = row.FindElements(By.TagName("th"));
                                        if (TDTaxExemptions.Count == 0)
                                        {
                                            //ExemptionDetails1 += THTaxExemptions[0].Text + "~";
                                        }
                                        if (TDTaxExemptions.Count >= 1)
                                        {
                                            if (row.Text.Contains("\r\n"))
                                            {
                                                try
                                                {
                                                    ExemptionDetails1 += TDTaxExemptions[0].Text.Replace("\r\n", "~") + "~";
                                                    ExemptionDetails2 += TDTaxExemptions[1].Text.Replace("\r\n", "~") + "~";
                                                }
                                                catch { }
                                            }
                                            if (!row.Text.Contains("\r\n"))
                                            {
                                                try
                                                {
                                                    ExemptionDetails1 += TDTaxExemptions[0].Text + "~";
                                                    ExemptionDetails2 += TDTaxExemptions[1].Text + "~";
                                                }
                                                catch { }
                                            }
                                            if (ExemptionDetails1 != "" && ExemptionDetails2.Trim() == "")
                                            {
                                                string[] exesplit = ExemptionDetails1.Split('~');
                                                for (int exe = 0; exe < exesplit.Count() - 1; exe++)
                                                {
                                                    ExemptionDetails2 += "~";
                                                }
                                            }
                                        }

                                    }

                                }
                                catch { }
                                // TaxValuations
                                try
                                {
                                    IWebElement TaxValuation = driver.FindElement(By.XPath("//*[@id='cardlist']/div/div[1]/div/div[3]/div[4]/div/table"));
                                    IList<IWebElement> TRTaxValuation = TaxValuation.FindElements(By.TagName("tr"));
                                    IList<IWebElement> THTaxValuation = TaxValuation.FindElements(By.TagName("th"));
                                    IList<IWebElement> TDTaxValuation;
                                    foreach (IWebElement row in TRTaxValuation)
                                    {
                                        TDTaxValuation = row.FindElements(By.TagName("td"));
                                        THTaxValuation = row.FindElements(By.TagName("th"));
                                        if (TDTaxValuation.Count == 0)
                                        {
                                            //TaxValuation1 += THTaxValuation[0].Text + "~";
                                        }
                                        if (TDTaxValuation.Count >= 2)
                                        {
                                            if (row.Text.Contains("\r\n"))
                                            {
                                                try
                                                {
                                                    TaxValuation1 += TDTaxValuation[0].Text.Replace("\r\n", "~") + "~";
                                                    TaxValuation2 += TDTaxValuation[1].Text.Replace("\r\n", "~") + "~";
                                                }
                                                catch { }
                                            }
                                            if (!row.Text.Contains("\r\n"))
                                            {
                                                try
                                                {
                                                    TaxValuation1 += TDTaxValuation[0].Text + "~";
                                                    TaxValuation2 += TDTaxValuation[1].Text + "~";
                                                }
                                                catch { }
                                            }
                                            if (TaxValuation1 != "" && TaxValuation2.Trim() == "")
                                            {
                                                string[] exesplit = TaxValuation1.Split('~');
                                                for (int exe = 0; exe < exesplit.Count() - 1; exe++)
                                                {
                                                    TaxValuation2 += "~";
                                                }
                                            }
                                        }
                                    }
                                    ExemptionDetails1 += TaxValuation1;
                                    ExemptionDetails2 += TaxValuation2;

                                    DBconnection dbconn = new DBconnection();
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + ExemptionDetails1.Remove(ExemptionDetails1.Length - 1, 1) + "' where Id = '" + 1111 + "'");
                                    gc.insert_date(orderNumber, PropertyID, 1111, ExemptionDetails2.Remove(ExemptionDetails2.Length - 1, 1), 1, DateTime.Now);

                                }
                                catch { }



                                // Tax Due Details
                                IWebElement TaxDue = driver.FindElement(By.XPath("//*[@id='cardlist']/div/div[1]/div/div[4]/div/div/table"));
                                IList<IWebElement> TRTaxDue = TaxDue.FindElements(By.TagName("tr"));
                                IList<IWebElement> THTaxDue = TaxDue.FindElements(By.TagName("th"));
                                IList<IWebElement> TDTaxDue;
                                foreach (IWebElement row in TRTaxDue)
                                {
                                    TDTaxDue = row.FindElements(By.TagName("td"));
                                    THTaxDue = row.FindElements(By.TagName("th"));
                                    if (TDTaxDue.Count != 0 && !row.Text.Contains("TAXING ENTITIES") && !row.Text.Contains("\r\n") && !row.Text.Contains("============"))
                                    {
                                        string TaxDueDetails = TDTaxDue[0].Text + "~" + TDTaxDue[1].Text + "~" + TDTaxDue[2].Text + "~" + TDTaxDue[3].Text + "~" + TDTaxDue[4].Text + "~" + TDTaxDue[5].Text + "~" + TDTaxDue[6].Text + "~" + TDTaxDue[7].Text + "~" + TDTaxDue[8].Text;
                                        gc.insert_date(orderNumber, PropertyID, 1113, TaxDueDetails, 1, DateTime.Now);
                                    }
                                    if (TDTaxDue.Count != 0 && !row.Text.Contains("TAXING ENTITIES") && row.Text.Contains("\r\n") && !row.Text.Contains("============"))
                                    {
                                        string[] strTaxYearSplit = TDTaxDue[0].Text.Split('\r');
                                        string[] strTaxEntSplit = TDTaxDue[1].Text.Split('\r');
                                        string[] strTaxExeSplit = TDTaxDue[2].Text.Split('\r');
                                        string[] strTaxableSplit = TDTaxDue[3].Text.Split('\r');
                                        string[] strTaxRateSplit = TDTaxDue[4].Text.Split('\r');
                                        string[] strTaxAmtSplit = TDTaxDue[5].Text.Split('\r');
                                        string[] strTaxDueSplit = TDTaxDue[6].Text.Split('\r');
                                        string[] strTaxAddnSplit = TDTaxDue[7].Text.Split('\r');
                                        string[] strTaxTotalSplit = TDTaxDue[8].Text.Split('\r');
                                        for (int Due = 0; Due < strTaxYearSplit.Count(); Due++)
                                        {
                                            string TaxDueDetails = strTaxYearSplit[Due].Replace("\n", "") + "~" + strTaxEntSplit[Due].Replace("\n", "") + "~" + strTaxExeSplit[Due].Replace("\n", "") + "~" + strTaxableSplit[Due].Replace("\n", "") + "~" + strTaxRateSplit[Due].Replace("\n", "") + "~" + strTaxAmtSplit[Due].Replace("\n", "") + "~" + strTaxDueSplit[Due].Replace("\n", "") + "~" + strTaxAddnSplit[Due].Replace("\n", "") + "~" + strTaxTotalSplit[Due].Replace("\n", "");
                                            gc.insert_date(orderNumber, PropertyID, 1113, TaxDueDetails, 1, DateTime.Now);
                                        }

                                    }
                                }

                                // statements
                                driver.FindElement(By.LinkText("STATEMENTS")).Click();
                                Thread.Sleep(1000);
                                gc.CreatePdf(orderNumber, PropertyID, " SD3-property Statements", driver, "TX", "Galveston");
                                int j = 0;
                                string propSTYear = "", propSTBill = "", propSTExemptions = "", propSTTaxable = "", propSTTax = "", propSTPaid = "", propSTBalance = "", propSTPI = "", propSTAtty = "", propSTADDN = "";
                                string propSTDue = "", propSTDeferred = "", propSTOmitted = "", propSTSuit = "";
                                IWebElement IPropStatement = driver.FindElement(By.XPath("/html/body/div[2]/section/div/form/div/div/div/div/div[1]/div/div/table/tbody"));
                                IList<IWebElement> IPropStatementRow = IPropStatement.FindElements(By.TagName("tr"));
                                IList<IWebElement> IPropStatementTD;
                                foreach (IWebElement Statement in IPropStatementRow)
                                {
                                    IPropStatementTD = Statement.FindElements(By.TagName("td"));
                                    if (IPropStatementTD.Count != 0)
                                    {
                                        propSTYear = IPropStatementTD[0].Text;
                                        propSTBill = IPropStatementTD[1].Text;
                                        propSTExemptions = IPropStatementTD[2].Text;
                                        propSTTaxable = IPropStatementTD[3].Text;
                                        propSTTax = IPropStatementTD[4].Text;
                                        propSTPaid = IPropStatementTD[5].Text;
                                        propSTBalance = IPropStatementTD[6].Text;
                                        propSTPI = IPropStatementTD[7].Text;
                                        propSTAtty = IPropStatementTD[8].Text;
                                        propSTADDN = IPropStatementTD[9].Text;
                                        propSTDue = IPropStatementTD[10].Text;
                                        propSTDeferred = IPropStatementTD[11].Text;
                                        propSTOmitted = IPropStatementTD[12].Text;
                                        propSTSuit = IPropStatementTD[13].Text;

                                        string propDueDetails1 = propSTYear + "~" + propSTBill + "~" + propSTExemptions + "~" + propSTTaxable + "~" + propSTTax + "~" + propSTPaid + "~" + propSTBalance + "~" + propSTPI + "~" + propSTAtty + "~" + propSTADDN + "~" + propSTDue + "~" + propSTDeferred + "~" + propSTOmitted + "~" + propSTSuit;
                                        gc.insert_date(orderNumber, PropertyID, 1115, propDueDetails1, 1, DateTime.Now);
                                    }

                                    if (IPropStatementTD.Count != 0 && j < IPropStatementRow.Count - 1)
                                    {
                                        string TPI = IPropStatementTD[7].Text;
                                        string TAtty = IPropStatementTD[8].Text;
                                        if (!TPI.Contains("0.00") && !TAtty.Contains("0.00"))
                                        {
                                            j++;
                                        }
                                    }
                                }
                                if (j >= 1)
                                {
                                    string alert = "Delinquent " + "~" + "Need to Call" + "~" + "For tax amount due, you must call the Collector's Office.";
                                    gc.insert_date(orderNumber, PropertyID, 1140, alert, 1, DateTime.Now);
                                }

                                // RECEIPTS
                                driver.FindElement(By.LinkText("RECEIPTS")).Click();
                                Thread.Sleep(1000);
                                gc.CreatePdf(orderNumber, PropertyID, "SD3-property Receipts", driver, "TX", "Galveston");
                                string propRCPT = "", propRCPTDate = "", propRPostDate = "", propRCheck = "", propRPaid1 = "", propRMO = "", propRPaid2 = "", propRCC = "", propRCCType = "";
                                string propRPaid3 = "", propROther = "", propRAmountPaid = "", propRCashPaid = "", propRVoid = "", propRCode = "", propRDate = "";
                                IWebElement IPropReceipt = driver.FindElement(By.XPath("/html/body/div[2]/section/div/form/div/div/div/div/div/div/div/table/tbody"));
                                IList<IWebElement> IPropReceiptRow = IPropReceipt.FindElements(By.TagName("tr"));
                                IList<IWebElement> IPropReceiptTD;
                                foreach (IWebElement Reciept in IPropReceiptRow)
                                {
                                    IPropReceiptTD = Reciept.FindElements(By.TagName("td"));
                                    if (IPropReceiptTD.Count != 0)
                                    {
                                        propRCPT = IPropReceiptTD[0].Text;
                                        propRCPTDate = IPropReceiptTD[1].Text;
                                        propRPostDate = IPropReceiptTD[2].Text;
                                        propRCheck = IPropReceiptTD[3].Text;
                                        propRPaid1 = IPropReceiptTD[4].Text;
                                        propRMO = IPropReceiptTD[5].Text;
                                        propRPaid2 = IPropReceiptTD[6].Text;
                                        propRCC = IPropReceiptTD[7].Text;
                                        propRCCType = IPropReceiptTD[8].Text;
                                        propRPaid3 = IPropReceiptTD[9].Text;
                                        propROther = IPropReceiptTD[10].Text;
                                        propRAmountPaid = IPropReceiptTD[11].Text;
                                        propRCashPaid = IPropReceiptTD[12].Text;
                                        propRVoid = IPropReceiptTD[13].Text;
                                        propRCode = IPropReceiptTD[14].Text;
                                        propRDate = IPropReceiptTD[15].Text;

                                        string propDueDetails2 = propRCPT + "~" + propRCPTDate + "~" + propRPostDate + "~" + propRCheck + "~" + propRPaid1 + "~" + propRMO + "~" + propRPaid2 + "~" + propRCC + "~" + propRCCType + "~" + propRPaid3 + "~" + propROther + "~" + propRAmountPaid + "~" + propRCashPaid + "~" + propRVoid + "~" + propRCode + "~" + propRDate;
                                        gc.insert_date(orderNumber, PropertyID, 1116, propDueDetails2, 1, DateTime.Now);
                                    }
                                }

                                var chromeOptions = new ChromeOptions();
                                var chDriver = new ChromeDriver(chromeOptions);
                                try
                                {
                                    chDriver.Navigate().GoToUrl("http://friendswoodtax.azurewebsites.net/search/list");
                                    Thread.Sleep(2000);
                                    chDriver.FindElement(By.Id("tag")).SendKeys(GeographicID.Replace("-", "").Trim());
                                    chDriver.FindElement(By.XPath("//*[@id='tag']")).SendKeys(Keys.Enter);
                                    Thread.Sleep(2000);
                                    IWebElement ITaxClick1 = chDriver.FindElement(By.XPath("//*[@id='searchlist']/div/div/div/div/table"));
                                    IList<IWebElement> ITaxRow1 = ITaxClick1.FindElements(By.TagName("tr"));
                                    foreach (IWebElement tax in ITaxRow1)
                                    {
                                        if (tax.Text.Contains(PropertyID.Trim().Replace("-", "")))
                                        {
                                            string strClick = tax.GetAttribute("id");
                                            if (strClick.Contains("rowid"))
                                            {
                                                IWebElement Iclick = chDriver.FindElement(By.Id(strClick));
                                                Iclick.Click();
                                                Thread.Sleep(2000);
                                            }
                                        }
                                    }
                                    chDriver.FindElement(By.LinkText("STATEMENTS")).Click();
                                    // statement download
                                    try
                                    {
                                        IWebElement ParcelTB = chDriver.FindElement(By.XPath("/html/body/div[2]/section/div/form/div/div/div/div/div[1]/div/div/table/tbody"));
                                        IList<IWebElement> ParcelTR = ParcelTB.FindElements(By.TagName("tr"));
                                        ParcelTR.Reverse();
                                        int rows_count = ParcelTR.Count;
                                        string current = chDriver.CurrentWindowHandle;
                                        string strPrevious = "";
                                        int count = 0, currentyear = DateTime.Now.Year;
                                        for (int row = 0; row < rows_count; row++)
                                        {
                                            if (row == rows_count - 6 || row == rows_count - 5 || row == rows_count - 4 || row == rows_count - 3 || row == rows_count - 2 || row == rows_count - 1)
                                            {
                                                IList<IWebElement> Columns_row = ParcelTR[row].FindElements(By.TagName("td"));
                                                int columns_count = Columns_row.Count;
                                                if (columns_count != 0 && count <= 3 && (Convert.ToInt32(Columns_row[0].Text) == currentyear - 2 || Convert.ToInt32(Columns_row[0].Text) == currentyear - 1 || Convert.ToInt32(Columns_row[0].Text) == currentyear))
                                                {
                                                    string strcurrent = Columns_row[0].Text;
                                                    Columns_row[0].Click();
                                                    chDriver.SwitchTo().Window(chDriver.WindowHandles.Last());
                                                    Thread.Sleep(9000);
                                                    gc.CreatePdf_Chrome(orderNumber, PropertyID, "Statement1" + row, chDriver, "TX", "Galveston");
                                                    if (strcurrent != strPrevious)
                                                    {
                                                        strPrevious = strcurrent;
                                                        count++;
                                                    }
                                                }
                                                chDriver.SwitchTo().Window(current);
                                            }
                                        }
                                    }
                                    catch (Exception ex) { }

                                    // Receipt Download
                                    chDriver.FindElement(By.LinkText("RECEIPTS")).Click();
                                    IWebElement SD_Receipt = chDriver.FindElement(By.XPath("/html/body/div[2]/section/div/form/div/div/div/div/div/div/div/table/tbody"));
                                    IList<IWebElement> TRSD_Receipt = SD_Receipt.FindElements(By.TagName("tr"));
                                    IList<IWebElement> THSD_Receipt = SD_Receipt.FindElements(By.TagName("th"));
                                    IList<IWebElement> TDSD_Receipt;
                                    int i = 0;
                                    string current1 = chDriver.CurrentWindowHandle;
                                    foreach (IWebElement row in TRSD_Receipt)
                                    {
                                        TDSD_Receipt = row.FindElements(By.TagName("td"));
                                        THSD_Receipt = row.FindElements(By.TagName("th"));
                                        if (TDSD_Receipt.Count != 0)
                                        {
                                            if (i < 3)
                                            {
                                                TDSD_Receipt[0].Click();
                                                chDriver.SwitchTo().Window(chDriver.WindowHandles.Last());
                                                Thread.Sleep(5000);
                                                gc.CreatePdf_Chrome(orderNumber, PropertyID, "Receipt" + i, chDriver, "TX", "Galveston");
                                                i++;
                                            }
                                            if (i == 3)
                                            {
                                                break;
                                            }
                                            chDriver.SwitchTo().Window(current1);
                                        }
                                    }
                                    chDriver.Quit();
                                }
                                catch { chDriver.Quit(); }

                            }
                            catch { }
                            GalScenario3++;
                        }
                        //SCENARIO - 4 Galveston CO MUD 14       
                        if ((msg1 == "M14" || msg1 == "M15") && GalScenario4 < 1)
                        {
                            driver.Navigate().GoToUrl("https://www.wheelerassoc.com/search");
                            string CadNo = "";
                            try
                            {
                                driver.FindElement(By.Id("MainContent_AccountTabContainer_TabPanelCAD_CadTextBox")).SendKeys(GeographicID.Replace("-", "").Trim());
                                gc.CreatePdf_WOP(orderNumber, "AddressSearch", driver, "TX", "Galveston");
                                driver.FindElement(By.Id("MainContent_AccountTabContainer_TabPanelCAD_CadButton")).Click();
                                Thread.Sleep(2000);
                                try
                                {
                                    gc.CreatePdf_WOP(orderNumber, "AddressSearch Result", driver, "TX", "Galveston");
                                    IWebElement Mudtable = driver.FindElement(By.XPath("//*[@id='MainContent_AcctListGridView']/tbody"));
                                    IList<IWebElement> Mudrow = Mudtable.FindElements(By.TagName("tr"));
                                    IList<IWebElement> Mudid;
                                    foreach (IWebElement MudidMudid in Mudrow)
                                    {
                                        Mudid = MudidMudid.FindElements(By.TagName("td"));
                                        if (Mudid.Count != 0 && !MudidMudid.Text.Contains("CAD Account #"))
                                        {
                                            IWebElement accountmUd = Mudid[0].FindElement(By.TagName("a"));
                                            string Mudhref = accountmUd.GetAttribute("href");
                                            driver.Navigate().GoToUrl(Mudhref);
                                            Thread.Sleep(4000);
                                            break;
                                        }
                                    }
                                }
                                catch { }
                                for (int i = 0; i < 3; i++)
                                {
                                    //string current = driver.CurrentWindowHandle;
                                    if (i > 0)
                                    {
                                        //driver.SwitchTo().Window(current);
                                        IWebElement PropertyInformation = driver.FindElement(By.Id("MainContent_TaxYearDropDown"));
                                        SelectElement PropertyInformationSelect = new SelectElement(PropertyInformation);
                                        PropertyInformationSelect.SelectByIndex(i);
                                        Thread.Sleep(3000);
                                    }
                                    string Taxrate = "", PaymentsApplied = "", HomesteadExemption = "", lane = "", TaxLevied = "", Improvements = "", TaxableValue = "", Tax_Year_Balance = "";
                                    string Jurisdiction = driver.FindElement(By.Id("MainContent_DistrictNameTxt")).Text;
                                    string Tax_AuthorityMud4 = driver.FindElement(By.XPath("//*[@id='PrintArea']/div[1]")).Text;
                                    string Tax_Authority = GlobalClass.After(Tax_AuthorityMud4, "Jurisdiction").Trim();
                                    string Owner_Name = driver.FindElement(By.Id("MainContent_OwnerNameTxt")).Text;
                                    string OwnerAddress1 = driver.FindElement(By.Id("MainContent_OwnerAdd1Txt")).Text;
                                    string OwnerAddress2 = driver.FindElement(By.Id("MainContent_OwnerAdd2Txt")).Text;
                                    string FullOwnerAddress = OwnerAddress1 + " " + OwnerAddress2;
                                    string InquiryDate = driver.FindElement(By.Id("MainContent_DateTxt")).Text;
                                    string DelinquentDate = driver.FindElement(By.Id("MainContent_DelinquentTxt")).Text;
                                    CadNo = driver.FindElement(By.Id("MainContent_CadNoTxt")).Text;
                                    string TaxYear = driver.FindElement(By.Id("MainContent_TaxYearTxt")).Text;
                                    string JurisdictionCode = driver.FindElement(By.Id("MainContent_JurNoTxt")).Text;
                                    string Acreage = driver.FindElement(By.Id("MainContent_AcreageTxt")).Text;
                                    string strLegalDescription = driver.FindElement(By.Id("MainContent_LegalAddTxt")).Text;
                                    string FullPropertyAddress = driver.FindElement(By.Id("MainContent_SitusAddTxt")).Text;

                                    IWebElement Apprasialvaluetable = driver.FindElement(By.XPath("//*[@id='MainContent_RollGridView']/tbody"));
                                    IList<IWebElement> Appricelvaluerow = Apprasialvaluetable.FindElements(By.TagName("tr"));
                                    IList<IWebElement> Appricelvalueid;
                                    foreach (IWebElement appricelvalue in Appricelvaluerow)
                                    {
                                        Appricelvalueid = appricelvalue.FindElements(By.TagName("td"));
                                        if (appricelvalue.Text.Contains("Land"))
                                        {
                                            lane = Appricelvalueid[1].Text;
                                        }
                                        if (appricelvalue.Text.Contains("Improvements"))
                                        {
                                            Improvements = Appricelvalueid[1].Text;
                                        }
                                        if (appricelvalue.Text.Contains("Homestead Exemption"))
                                        {
                                            HomesteadExemption = Appricelvalueid[1].Text;
                                        }

                                        if (appricelvalue.Text.Contains("Taxable Value"))
                                        {
                                            TaxableValue = Appricelvalueid[1].Text;
                                        }
                                    }
                                    IWebElement taxratetable = driver.FindElement(By.XPath("//*[@id='MainContent_TaxGridView']/tbody"));
                                    IList<IWebElement> Taxratrow = taxratetable.FindElements(By.TagName("tr"));
                                    IList<IWebElement> taxratrowid;
                                    foreach (IWebElement Taxtar in Taxratrow)
                                    {
                                        taxratrowid = Taxtar.FindElements(By.TagName("td"));
                                        if (Taxtar.Text.Contains("Tax Levied"))
                                        {
                                            TaxLevied = taxratrowid[1].Text;
                                        }
                                        if (Taxtar.Text.Contains("Payments Applied To Taxes"))
                                        {
                                            PaymentsApplied = taxratrowid[1].Text;
                                        }
                                        if (Taxtar.Text.Contains("Tax Year"))
                                        {
                                            Tax_Year_Balance = taxratrowid[1].Text;
                                        }

                                    }
                                    Taxrate = gc.Between(taxratetable.Text, "Tax Rate", "Tax Levied").Trim();
                                    string taxmudresult = Jurisdiction + "~" + Owner_Name + "~" + FullOwnerAddress + "~" + InquiryDate + "~" + DelinquentDate + "~" + CadNo + "~" + TaxYear + "~" + JurisdictionCode + "~" + Acreage + "~" + strLegalDescription + "~" + FullPropertyAddress + "~" + lane + "~" + Improvements + "~" + TaxableValue + "~" + Taxrate + "~" + TaxLevied + "~" + PaymentsApplied + "~" + Tax_Year_Balance + "~" + Tax_Authority;
                                    gc.insert_date(orderNumber, CadNo, 1117, taxmudresult, 1, DateTime.Now);
                                    gc.CreatePdf(orderNumber, CadNo, "Property detail MUD4" + TaxYear, driver, "TX", "Galveston");
                                    IWebElement Currenttaxduetable = driver.FindElement(By.XPath("//*[@id='MainContent_DueGridView']/tbody"));
                                    IList<IWebElement> currenttaxduerow = Currenttaxduetable.FindElements(By.TagName("tr"));
                                    IList<IWebElement> Currenttaxdueid;
                                    foreach (IWebElement currenttaxdue in currenttaxduerow)
                                    {
                                        Currenttaxdueid = currenttaxdue.FindElements(By.TagName("td"));
                                        if (Currenttaxdueid.Count != 0 && !currenttaxdue.Text.Contains("Tax Year"))
                                        {
                                            string CurrentresultMud = Currenttaxdueid[0].Text + "~" + Currenttaxdueid[1].Text + "~" + Currenttaxdueid[2].Text;
                                            gc.insert_date(orderNumber, CadNo, 1118, CurrentresultMud, 1, DateTime.Now);
                                        }
                                    }
                                    try
                                    {
                                        IWebElement Taxrecipt = driver.FindElement(By.Id("MainContent_TaxReceiptHyperLink"));
                                        string Taxrecipthref = Taxrecipt.GetAttribute("href");
                                        driver.Navigate().GoToUrl(Taxrecipthref);
                                        Thread.Sleep(3000);
                                        gc.CreatePdf(orderNumber, CadNo, "Tax Recipt" + TaxYear, driver, "TX", "Galveston");
                                        driver.Navigate().Back();
                                        Thread.Sleep(3000);
                                    }
                                    catch { }
                                    //Download
                                    try
                                    {
                                        IWebElement downloadMud = driver.FindElement(By.Id("MainContent_TaxStatementHyperLink"));
                                        string Downloadhref = downloadMud.GetAttribute("href");
                                        string fileName = "Statement.pdf";
                                        var chromeOptions = new ChromeOptions();
                                        var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];
                                        chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                                        chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                                        chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");

                                        var chDriver = new ChromeDriver(chromeOptions);
                                        try
                                        {
                                            chDriver.Navigate().GoToUrl(Downloadhref);
                                            Thread.Sleep(4000);
                                            chDriver.FindElement(By.Id("imagebutton2")).Click();
                                            Thread.Sleep(4000);
                                            gc.AutoDownloadFileSpokane(orderNumber, CadNo, "Galveston", "TX", fileName);
                                        }
                                        catch { }
                                        chDriver.Quit();
                                    }
                                    catch { }
                                }
                            }
                            catch { }
                            GalScenario4++;
                        }
                        //SCENARIO - 7 Assessments of The Southwest Inc (ASW)
                        if ((msg1 == "M08" || msg1 == "M13" || msg1 == "M16" || msg1 == "M20" || msg1 == "M22" || msg1 == "M27" || msg1 == "M30" || msg1 == "M31" || msg1 == "M32" || msg1 == "M33" || msg1 == "M43" || msg1 == "M44" || msg1 == "M45" || msg1 == "M46" || msg1 == "M52" || msg1 == "M54" || msg1 == "M55" || msg1 == "M56" || msg1 == "M57" || msg1 == "M58" || msg1 == "M59" || msg1 == "M66" || msg1 == "P09") && GalScenario7 < 1)
                        {
                            try
                            {
                                driver.Navigate().GoToUrl("http://aswportal.azurewebsites.net/search/list");
                                Thread.Sleep(2000);
                                driver.FindElement(By.Id("tag")).SendKeys(GeographicID.Replace("-", "").Trim());
                                gc.CreatePdf(orderNumber, PropertyID, " SD3-Property Search", driver, "TX", "Galveston");
                                driver.FindElement(By.XPath("//*[@id='tag']")).SendKeys(Keys.Enter);
                                Thread.Sleep(2000);
                                gc.CreatePdf(orderNumber, PropertyID, " SD3-Property Search Result", driver, "TX", "Galveston");
                                IWebElement SD_property3 = driver.FindElement(By.XPath("//*[@id='searchlist']/div/div/div/div/table"));
                                IList<IWebElement> TRSD_property3 = SD_property3.FindElements(By.TagName("tr"));
                                IList<IWebElement> THSD_property3 = SD_property3.FindElements(By.TagName("th"));
                                IList<IWebElement> TDSD_property3;
                                foreach (IWebElement row in TRSD_property3)
                                {
                                    TDSD_property3 = row.FindElements(By.TagName("td"));
                                    THSD_property3 = row.FindElements(By.TagName("th"));
                                    if (TDSD_property3.Count != 0 && !row.Text.Contains(""))
                                    {
                                    }
                                }

                                IWebElement ITaxClick = driver.FindElement(By.XPath("//*[@id='searchlist']/div/div/div/div/table"));
                                IList<IWebElement> ITaxRow = ITaxClick.FindElements(By.TagName("tr"));
                                foreach (IWebElement tax in ITaxRow)
                                {
                                    if (tax.Text.Contains(PropertyID.Trim().Replace("-", "")))
                                    {
                                        string strClick = tax.GetAttribute("id");
                                        if (strClick.Contains("rowid"))
                                        {
                                            IWebElement Iclick = driver.FindElement(By.Id(strClick));
                                            Iclick.Click();
                                            Thread.Sleep(2000);
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (tax.Text.Contains(GeographicID.Trim().Replace("-", "")) && tax.Text.Contains(houseno) && tax.Text.Contains(sname) && !tax.Text.Contains("INACTIVE"))
                                        {
                                            string strClick1 = tax.GetAttribute("id");
                                            if (strClick1.Contains("rowid"))
                                            {
                                                IWebElement Iclick1 = driver.FindElement(By.Id(strClick1));
                                                Iclick1.Click();
                                                Thread.Sleep(2000);
                                                break;
                                            }
                                        }
                                    }
                                }

                                // Tax Details (Tax Summary)

                                string TaxSummary = driver.FindElement(By.XPath("//*[@id='cardlist']/div/div[1]/div/div[2]/div[1]/div/table")).Text;
                                string PropertyOwnership = "", PropId = "", GeoId = "", Situs = "", LegalDesc = "", TaxAuthority = "";
                                PropertyOwnership = GlobalClass.After(TaxSummary, "PROPERTY OWNERSHIP").Trim().Replace("\r\n", "");
                                string TaxSummaryID = driver.FindElement(By.XPath("//*[@id='cardlist']/div/div[1]/div/div[3]/div[1]/div/table/tbody")).Text;
                                PropId = gc.Between(TaxSummaryID, "PROP ID:", "GEOID:").Trim().Replace("\r\n", "");
                                GeoId = gc.Between(TaxSummaryID, "GEOID:", "SITUS:").Trim().Replace("\r\n", "");
                                //string Taxsummary1 = driver.FindElement(By.XPath("//*[@id='cardlist']/div/div[1]/div/div[3]/div[1]/div/table/tbody")).Text;
                                Situs = GlobalClass.After(TaxSummaryID, "SITUS:").Trim().Replace("\r\n", "");
                                string TaxSummary2 = driver.FindElement(By.XPath("//*[@id='cardlist']/div/div[1]/div/div[3]/div[2]/div/table")).Text;
                                LegalDesc = GlobalClass.After(TaxSummary2, "LEGAL DESCRIPTION").Trim().Replace("LEGAL: ", "").Trim().Replace("\r\n", "");
                                TaxAuthority = "Assessments of the Southwest, Inc. #5 Oaktree Friendswood, Texas 77546 Phone: 281-482-0216 Fax: 281-482-5285 Mailing Address P.O. Box 1368 Friendswood, TX 77549";
                                string TaxDetails = PropertyOwnership + "~" + PropId + "~" + GeoId + "~" + Situs + "~" + LegalDesc + "~" + TaxAuthority;
                                gc.insert_date(orderNumber, PropertyID, 1143, TaxDetails, 1, DateTime.Now);
                                gc.CreatePdf(orderNumber, PropertyID, "SD3-Tax Summary", driver, "TX", "Galveston");

                                // Payment Details Table
                                IWebElement TaxPayment = driver.FindElement(By.XPath("//*[@id='cardlist']/div/div[1]/div/div[2]/div[3]/div/table"));
                                IList<IWebElement> TRTaxPayment = TaxPayment.FindElements(By.TagName("tr"));
                                IList<IWebElement> THTaxPayment = TaxPayment.FindElements(By.TagName("th"));
                                IList<IWebElement> TDTaxPayment;
                                foreach (IWebElement row in TRTaxPayment)
                                {
                                    TDTaxPayment = row.FindElements(By.TagName("td"));
                                    THTaxPayment = row.FindElements(By.TagName("th"));
                                    if (TDTaxPayment.Count != 0 && !row.Text.Contains("AMOUNT DUE"))
                                    {
                                        string TaxPaymentDetails = TDTaxPayment[0].Text + "~" + TDTaxPayment[1].Text + "~" + TDTaxPayment[2].Text;
                                        gc.insert_date(orderNumber, PropertyID, 1145, TaxPaymentDetails, 1, DateTime.Now);
                                    }
                                }

                                // Exemptions
                                string ExemptionDetails1 = "", ExemptionDetails2 = "", TaxValuation1 = "", TaxValuation2 = "";
                                try
                                {
                                    IWebElement TaxExemptions = driver.FindElement(By.XPath("//*[@id='cardlist']/div/div[1]/div/div[3]/div[3]/div/table"));
                                    IList<IWebElement> TRTaxExemptions = TaxExemptions.FindElements(By.TagName("tr"));
                                    IList<IWebElement> THTaxExemptions = TaxExemptions.FindElements(By.TagName("th"));
                                    IList<IWebElement> TDTaxExemptions;
                                    foreach (IWebElement row in TRTaxExemptions)
                                    {
                                        TDTaxExemptions = row.FindElements(By.TagName("td"));
                                        THTaxExemptions = row.FindElements(By.TagName("th"));
                                        if (TDTaxExemptions.Count == 0)
                                        {
                                            //ExemptionDetails1 += THTaxExemptions[0].Text + "~";
                                        }
                                        if (TDTaxExemptions.Count >= 1)
                                        {
                                            if (row.Text != "")
                                            {
                                                try
                                                {
                                                    ExemptionDetails1 += "Exemptions" + "~";
                                                    ExemptionDetails2 += TDTaxExemptions[0].Text.Replace("\r\n", "") + "~";
                                                }
                                                catch { }
                                            }
                                            //if (!row.Text.Contains("\r\n"))
                                            //{
                                            //    try
                                            //    {
                                            //        ExemptionDetails1 += TDTaxExemptions[0].Text + "~";
                                            //        ExemptionDetails2 += TDTaxExemptions[1].Text + "~";
                                            //    }
                                            //    catch { }
                                            //}
                                            //if (ExemptionDetails1 != "" && ExemptionDetails2.Trim() == "")
                                            //{
                                            //    string[] exesplit = ExemptionDetails1.Split('~');
                                            //    for (int exe = 0; exe < exesplit.Count() - 1; exe++)
                                            //    {
                                            //        ExemptionDetails2 += "~";
                                            //    }
                                            //}
                                        }
                                    }
                                }
                                catch { }

                                // TaxValuations
                                try
                                {
                                    IWebElement TaxValuation = driver.FindElement(By.XPath("//*[@id='cardlist']/div/div[1]/div/div[3]/div[4]/div/table"));
                                    IList<IWebElement> TRTaxValuation = TaxValuation.FindElements(By.TagName("tr"));
                                    IList<IWebElement> THTaxValuation = TaxValuation.FindElements(By.TagName("th"));
                                    IList<IWebElement> TDTaxValuation;
                                    foreach (IWebElement row in TRTaxValuation)
                                    {
                                        TDTaxValuation = row.FindElements(By.TagName("td"));
                                        THTaxValuation = row.FindElements(By.TagName("th"));
                                        if (TDTaxValuation.Count == 0)
                                        {
                                            //TaxValuation1 += THTaxValuation[0].Text + "~";
                                        }
                                        if (TDTaxValuation.Count >= 2)
                                        {
                                            if (row.Text.Contains("\r\n"))
                                            {
                                                try
                                                {
                                                    TaxValuation1 += TDTaxValuation[0].Text.Replace("\r\n", "~") + "~";
                                                    TaxValuation2 += TDTaxValuation[1].Text.Replace("\r\n", "~") + "~";
                                                }
                                                catch { }
                                            }
                                            if (!row.Text.Contains("\r\n"))
                                            {
                                                try
                                                {
                                                    TaxValuation1 += TDTaxValuation[0].Text + "~";
                                                    TaxValuation2 += TDTaxValuation[1].Text + "~";
                                                }
                                                catch { }
                                            }
                                            if (TaxValuation1 != "" && TaxValuation2.Trim() == "")
                                            {
                                                string[] exesplit = TaxValuation1.Split('~');
                                                for (int exe = 0; exe < exesplit.Count() - 1; exe++)
                                                {
                                                    TaxValuation2 += "~";
                                                }
                                            }
                                        }
                                    }
                                    ExemptionDetails1 += TaxValuation1;
                                    ExemptionDetails2 += TaxValuation2;

                                    DBconnection dbconn = new DBconnection();
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + ExemptionDetails1.Remove(ExemptionDetails1.Length - 1, 1) + "' where Id = '" + 1144 + "'");
                                    gc.insert_date(orderNumber, PropertyID, 1144, ExemptionDetails2.Remove(ExemptionDetails2.Length - 1, 1), 1, DateTime.Now);

                                }
                                catch { }



                                // Tax Due Details
                                IWebElement TaxDue = driver.FindElement(By.XPath("//*[@id='cardlist']/div/div[1]/div/div[4]/div/div/table"));
                                IList<IWebElement> TRTaxDue = TaxDue.FindElements(By.TagName("tr"));
                                IList<IWebElement> THTaxDue = TaxDue.FindElements(By.TagName("th"));
                                IList<IWebElement> TDTaxDue;
                                foreach (IWebElement row in TRTaxDue)
                                {
                                    TDTaxDue = row.FindElements(By.TagName("td"));
                                    THTaxDue = row.FindElements(By.TagName("th"));
                                    if (TDTaxDue.Count != 0 && !row.Text.Contains("TAXING ENTITIES") && !row.Text.Contains("============"))
                                    {
                                        string TaxDueDetails = TDTaxDue[0].Text + "~" + TDTaxDue[1].Text + "~" + TDTaxDue[2].Text + "~" + TDTaxDue[3].Text + "~" + TDTaxDue[4].Text + "~" + TDTaxDue[5].Text + "~" + TDTaxDue[6].Text + "~" + TDTaxDue[7].Text + "~" + TDTaxDue[8].Text;
                                        gc.insert_date(orderNumber, PropertyID, 1146, TaxDueDetails, 1, DateTime.Now);
                                    }
                                }

                                // statements
                                driver.FindElement(By.LinkText("STATEMENTS")).Click();
                                Thread.Sleep(1000);
                                gc.CreatePdf(orderNumber, PropertyID, " SD3-property Statements", driver, "TX", "Galveston");
                                int j = 0;
                                string propSTYear = "", propSTBill = "", propSTExemptions = "", propSTTaxable = "", propSTTax = "", propSTPaid = "", propSTBalance = "", propSTPI = "", propSTAtty = "", propSTADDN = "";
                                string propSTDue = "", propSTDeferred = "", propSTOmitted = "", propSTSuit = "";
                                IWebElement IPropStatement = driver.FindElement(By.XPath("/html/body/div[2]/section/div/form/div/div/div/div/div[1]/div/div/table/tbody"));
                                IList<IWebElement> IPropStatementRow = IPropStatement.FindElements(By.TagName("tr"));
                                IList<IWebElement> IPropStatementTD;
                                foreach (IWebElement Statement in IPropStatementRow)
                                {
                                    IPropStatementTD = Statement.FindElements(By.TagName("td"));
                                    if (IPropStatementTD.Count != 0)
                                    {
                                        propSTYear = IPropStatementTD[0].Text;
                                        propSTBill = IPropStatementTD[1].Text;
                                        propSTExemptions = IPropStatementTD[2].Text;
                                        propSTTaxable = IPropStatementTD[3].Text;
                                        propSTTax = IPropStatementTD[4].Text;
                                        propSTPaid = IPropStatementTD[5].Text;
                                        propSTBalance = IPropStatementTD[6].Text;
                                        propSTPI = IPropStatementTD[7].Text;
                                        propSTAtty = IPropStatementTD[8].Text;
                                        propSTADDN = IPropStatementTD[9].Text;
                                        propSTDue = IPropStatementTD[10].Text;
                                        propSTDeferred = IPropStatementTD[11].Text;
                                        propSTOmitted = IPropStatementTD[12].Text;
                                        propSTSuit = IPropStatementTD[13].Text;

                                        string propDueDetails1 = propSTYear + "~" + propSTBill + "~" + propSTExemptions + "~" + propSTTaxable + "~" + propSTTax + "~" + propSTPaid + "~" + propSTBalance + "~" + propSTPI + "~" + propSTAtty + "~" + propSTADDN + "~" + propSTDue + "~" + propSTDeferred + "~" + propSTOmitted + "~" + propSTSuit;
                                        gc.insert_date(orderNumber, PropertyID, 1147, propDueDetails1, 1, DateTime.Now);
                                    }

                                    if (IPropStatementTD.Count != 0 && j < IPropStatementRow.Count - 1)
                                    {
                                        string TPI = IPropStatementTD[7].Text;
                                        string TAtty = IPropStatementTD[8].Text;
                                        if (!TPI.Contains("0.00") && !TAtty.Contains("0.00"))
                                        {
                                            j++;
                                        }
                                    }
                                }
                                if (j >= 1)
                                {
                                    string alert = "Delinquent " + "~" + "Need to Call" + "~" + "For tax amount due, you must call the Collector's Office.";
                                    gc.insert_date(orderNumber, PropertyID, 1140, alert, 1, DateTime.Now);
                                }

                                //driver.FindElement(By.LinkText("STATEMENTS")).Click();
                                //// statement download
                                //try
                                //{
                                //    IWebElement ParcelTB = driver.FindElement(By.XPath("/html/body/div[2]/section/div/form/div/div/div/div/div[1]/div/div/table/tbody"));
                                //    IList<IWebElement> ParcelTR = ParcelTB.FindElements(By.TagName("tr"));
                                //    ParcelTR.Reverse();
                                //    int rows_count = ParcelTR.Count;
                                //    string current = driver.CurrentWindowHandle;

                                //    for (int row = 0; row < rows_count; row++)
                                //    {


                                //        if (row == rows_count - 3 || row == rows_count - 1 || row == rows_count - 2)
                                //        {
                                //            IList<IWebElement> Columns_row = ParcelTR[row].FindElements(By.TagName("td"));
                                //            int columns_count = Columns_row.Count;
                                //            if (columns_count != 0)
                                //            {
                                //                Columns_row[0].Click();
                                //                driver.SwitchTo().Window(driver.WindowHandles.Last());
                                //                Thread.Sleep(20000);
                                //                gc.CreatePdf(orderNumber, PropertyID, "Statement" + row, driver, "TX", "Galveston");
                                //            }
                                //            driver.SwitchTo().Window(current);
                                //        }
                                //    }
                                //}
                                //catch (Exception ex) { }

                                // RECEIPTS
                                driver.FindElement(By.LinkText("RECEIPTS")).Click();
                                Thread.Sleep(1000);
                                gc.CreatePdf(orderNumber, PropertyID, "SD3-property Receipts", driver, "TX", "Galveston");
                                string propRCPT = "", propRCPTDate = "", propRPostDate = "", propRCheck = "", propRPaid1 = "", propRMO = "", propRPaid2 = "", propRCC = "", propRCCType = "";
                                string propRPaid3 = "", propROther = "", propRAmountPaid = "", propRCashPaid = "", propRVoid = "", propRCode = "", propRDate = "";
                                IWebElement IPropReceipt = driver.FindElement(By.XPath("/html/body/div[2]/section/div/form/div/div/div/div/div/div/div/table/tbody"));
                                IList<IWebElement> IPropReceiptRow = IPropReceipt.FindElements(By.TagName("tr"));
                                IList<IWebElement> IPropReceiptTD;
                                foreach (IWebElement Reciept in IPropReceiptRow)
                                {
                                    IPropReceiptTD = Reciept.FindElements(By.TagName("td"));
                                    if (IPropReceiptTD.Count != 0)
                                    {
                                        propRCPT = IPropReceiptTD[0].Text;
                                        propRCPTDate = IPropReceiptTD[1].Text;
                                        propRPostDate = IPropReceiptTD[2].Text;
                                        propRCheck = IPropReceiptTD[3].Text;
                                        propRPaid1 = IPropReceiptTD[4].Text;
                                        propRMO = IPropReceiptTD[5].Text;
                                        propRPaid2 = IPropReceiptTD[6].Text;
                                        propRCC = IPropReceiptTD[7].Text;
                                        propRCCType = IPropReceiptTD[8].Text;
                                        propRPaid3 = IPropReceiptTD[9].Text;
                                        propROther = IPropReceiptTD[10].Text;
                                        propRAmountPaid = IPropReceiptTD[11].Text;
                                        propRCashPaid = IPropReceiptTD[12].Text;
                                        propRVoid = IPropReceiptTD[13].Text;
                                        propRCode = IPropReceiptTD[14].Text;
                                        propRDate = IPropReceiptTD[15].Text;

                                        string propDueDetails2 = propRCPT + "~" + propRCPTDate + "~" + propRPostDate + "~" + propRCheck + "~" + propRPaid1 + "~" + propRMO + "~" + propRPaid2 + "~" + propRCC + "~" + propRCCType + "~" + propRPaid3 + "~" + propROther + "~" + propRAmountPaid + "~" + propRCashPaid + "~" + propRVoid + "~" + propRCode + "~" + propRDate;
                                        gc.insert_date(orderNumber, PropertyID, 1148, propDueDetails2, 1, DateTime.Now);
                                    }
                                }

                                // Receipt Download
                                //driver.FindElement(By.LinkText("RECEIPTS")).Click();
                                //IWebElement SD_Receipt = driver.FindElement(By.XPath("/html/body/div[2]/section/div/form/div/div/div/div/div/div/div/table/tbody"));
                                //IList<IWebElement> TRSD_Receipt = SD_Receipt.FindElements(By.TagName("tr"));
                                //IList<IWebElement> THSD_Receipt = SD_Receipt.FindElements(By.TagName("th"));
                                //IList<IWebElement> TDSD_Receipt;
                                //int i = 0;
                                //string current1 = driver.CurrentWindowHandle;
                                //foreach (IWebElement row in TRSD_Receipt)
                                //{
                                //    TDSD_Receipt = row.FindElements(By.TagName("td"));
                                //    THSD_Receipt = row.FindElements(By.TagName("th"));
                                //    if (TDSD_Receipt.Count != 0)
                                //    {
                                //        if (i <= 3)
                                //        {
                                //            TDSD_Receipt[0].Click();
                                //            driver.SwitchTo().Window(driver.WindowHandles.Last());
                                //            Thread.Sleep(20000);
                                //            gc.CreatePdf(orderNumber, PropertyID, "Receipt" + i, driver, "TX", "Galveston");
                                //            i++;
                                //        }
                                //        if (i == 3)
                                //        {
                                //            break;
                                //        }
                                //        driver.SwitchTo().Window(current1);
                                //    }
                                //}

                                var chromeOptions = new ChromeOptions();
                                var chDriver = new ChromeDriver(chromeOptions);
                                try
                                {
                                    chDriver.Navigate().GoToUrl("http://aswportal.azurewebsites.net/search/list");
                                    Thread.Sleep(2000);
                                    chDriver.FindElement(By.Id("tag")).SendKeys(GeographicID.Replace("-", "").Trim());
                                    chDriver.FindElement(By.XPath("//*[@id='tag']")).SendKeys(Keys.Enter);
                                    Thread.Sleep(2000);
                                    IWebElement ITaxClick1 = chDriver.FindElement(By.XPath("//*[@id='searchlist']/div/div/div/div/table"));
                                    IList<IWebElement> ITaxRow1 = ITaxClick1.FindElements(By.TagName("tr"));
                                    foreach (IWebElement tax1 in ITaxRow1)
                                    {
                                        if (tax1.Text.Contains(PropertyID.Trim().Replace("-", "")))
                                        {
                                            string strClick = tax1.GetAttribute("id");
                                            if (strClick.Contains("rowid"))
                                            {
                                                IWebElement Iclick = chDriver.FindElement(By.Id(strClick));
                                                Iclick.Click();
                                                Thread.Sleep(2000);
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (tax1.Text.Contains(GeographicID.Trim().Replace("-", "")) && tax1.Text.Contains(houseno) && tax1.Text.Contains(sname) && !tax1.Text.Contains("INACTIVE"))
                                            {
                                                string strClick1 = tax1.GetAttribute("id");
                                                if (strClick1.Contains("rowid"))
                                                {
                                                    IWebElement Iclick1 = chDriver.FindElement(By.Id(strClick1));
                                                    Iclick1.Click();
                                                    Thread.Sleep(2000);
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    chDriver.FindElement(By.LinkText("STATEMENTS")).Click();
                                    // statement download
                                    try
                                    {
                                        IWebElement ParcelTB = chDriver.FindElement(By.XPath("/html/body/div[2]/section/div/form/div/div/div/div/div[1]/div/div/table/tbody"));
                                        IList<IWebElement> ParcelTR = ParcelTB.FindElements(By.TagName("tr"));
                                        ParcelTR.Reverse();
                                        int rows_count = ParcelTR.Count;
                                        string current = chDriver.CurrentWindowHandle;
                                        string strPrevious = "";
                                        int count = 0, currentyear = DateTime.Now.Year;
                                        for (int row = 0; row < rows_count; row++)
                                        {
                                            if (row == rows_count - 6 || row == rows_count - 5 || row == rows_count - 4 || row == rows_count - 3 || row == rows_count - 2 || row == rows_count - 1)
                                            {
                                                IList<IWebElement> Columns_row = ParcelTR[row].FindElements(By.TagName("td"));
                                                int columns_count = Columns_row.Count;
                                                if (columns_count != 0 && count <= 3 && (Convert.ToInt32(Columns_row[0].Text) == currentyear - 2 || Convert.ToInt32(Columns_row[0].Text) == currentyear - 1 || Convert.ToInt32(Columns_row[0].Text) == currentyear))
                                                {
                                                    string strcurrent = Columns_row[0].Text;
                                                    Columns_row[0].Click();
                                                    chDriver.SwitchTo().Window(chDriver.WindowHandles.Last());
                                                    Thread.Sleep(9000);
                                                    gc.CreatePdf_Chrome(orderNumber, PropertyID, "Statement1" + row, chDriver, "TX", "Galveston");
                                                    if (strcurrent != strPrevious)
                                                    {
                                                        strPrevious = strcurrent;
                                                        count++;
                                                    }
                                                }
                                                chDriver.SwitchTo().Window(current);
                                            }
                                        }
                                    }
                                    catch (Exception ex) { }

                                    // Receipt Download
                                    chDriver.FindElement(By.LinkText("RECEIPTS")).Click();
                                    IWebElement SD_Receipt = chDriver.FindElement(By.XPath("/html/body/div[2]/section/div/form/div/div/div/div/div/div/div/table/tbody"));
                                    IList<IWebElement> TRSD_Receipt = SD_Receipt.FindElements(By.TagName("tr"));
                                    IList<IWebElement> THSD_Receipt = SD_Receipt.FindElements(By.TagName("th"));
                                    IList<IWebElement> TDSD_Receipt;
                                    int i = 0;
                                    string current1 = chDriver.CurrentWindowHandle;
                                    foreach (IWebElement row in TRSD_Receipt)
                                    {
                                        TDSD_Receipt = row.FindElements(By.TagName("td"));
                                        THSD_Receipt = row.FindElements(By.TagName("th"));
                                        if (TDSD_Receipt.Count != 0)
                                        {
                                            if (i < 3)
                                            {
                                                TDSD_Receipt[0].Click();
                                                chDriver.SwitchTo().Window(chDriver.WindowHandles.Last());
                                                Thread.Sleep(5000);
                                                gc.CreatePdf_Chrome(orderNumber, PropertyID, "Receipt1" + i, chDriver, "TX", "Galveston");
                                                i++;
                                            }
                                            if (i == 3)
                                            {
                                                break;
                                            }
                                            chDriver.SwitchTo().Window(current1);
                                        }
                                    }
                                    chDriver.Quit();
                                }
                                catch
                                {
                                    chDriver.Quit();
                                }
                            }
                            catch { }
                            GalScenario7++;
                        }

                        //SCENARIO - 8: Public Improvement District (PID)
                        if ((msg1 == "P05" || msg1 == "P06" || msg1 == "P07" || msg1 == "P08" || msg1 == "P10" || msg1 == "P12") && GalScenario8 < 1)
                        {
                            string strstreetNumber = "", strdirection = "", strstreetName = "", streetType = "", unitnumber = "";
                            try
                            {
                                try
                                {
                                    driver.Navigate().GoToUrl("http://www.aswtax.com/look-up/pid");
                                    Thread.Sleep(2000);

                                    AddressParser.AddressParser splitAddr = new AddressParser.AddressParser();
                                    var splitAddrList = splitAddr.ParseAddress(Address);
                                    strstreetNumber = splitAddrList.Number.Trim();
                                    strstreetName = splitAddrList.Street.Trim();
                                    streetType = splitAddrList.Suffix.Trim();
                                    try
                                    {
                                        unitnumber = splitAddrList.SecondaryNumber.Trim();
                                    }
                                    catch { }
                                    try
                                    {
                                        strdirection = splitAddrList.Predirectional.Trim();
                                    }
                                    catch { }
                                    //driver.FindElement(By.XPath("//*[@id='wrap']/div[4]/div[1]/div/div/div/div[1]/form/input[2]")).SendKeys(msg1.Substring(1, 2) + PropertyID.Trim());
                                    //gc.CreatePdf(orderNumber, parcelNumber, "Enter The ParcelNumber Before", driver, "TX", "Galveston");
                                    //driver.FindElement(By.XPath("//*[@id='wrap']/div[4]/div[1]/div/div/div/div[1]/form/input[3]")).SendKeys(Keys.Enter);
                                    //Thread.Sleep(2000);
                                    //gc.CreatePdf(orderNumber, parcelNumber, "Enter The ParcelNumber After", driver, "TX", "Galveston");

                                    driver.FindElement(By.XPath("//*[@id='wrap']/div[4]/div[1]/div/div/div/div[2]/form/input[2]")).SendKeys(strstreetName.Trim() + " " + streetType.Trim());
                                    gc.CreatePdf(orderNumber, parcelNumber, "Enter The Address Before", driver, "TX", "Galveston");
                                    driver.FindElement(By.XPath("//*[@id='wrap']/div[4]/div[1]/div/div/div/div[2]/form/input[3]")).SendKeys(Keys.Enter);
                                    Thread.Sleep(2000);
                                    gc.CreatePdf(orderNumber, parcelNumber, "Enter The Address After", driver, "TX", "Galveston");
                                    IWebElement IAddressSearch = driver.FindElement(By.XPath("//*[@id='wrap']/div[4]/div[1]/div/div/table/tbody"));
                                    IList<IWebElement> IAddressRow = IAddressSearch.FindElements(By.TagName("tr"));
                                    IList<IWebElement> IAddressTD;
                                    foreach (IWebElement search in IAddressRow)
                                    {
                                        IAddressTD = search.FindElements(By.TagName("td"));
                                        if (strdirection != "" && strdirection != null)
                                        {
                                            if (!search.Text.Contains("Address") && search.Text.Contains(strstreetNumber) && search.Text.Contains(strdirection) && search.Text.Contains(strstreetName) && search.Text.Contains(streetType))
                                            {
                                                IWebElement IAddress = IAddressTD[0].FindElement(By.TagName("a"));
                                                if (IAddress.Text != "")
                                                {
                                                    IAddress.Click();
                                                    Thread.Sleep(3000);
                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (!search.Text.Contains("Address") && search.Text.Contains(strstreetNumber) && search.Text.Contains(strstreetName) && search.Text.Contains(streetType))
                                            {
                                                IWebElement IAddress = IAddressTD[0].FindElement(By.TagName("a"));
                                                if (IAddress.Text != "")
                                                {
                                                    IAddress.Click();
                                                    Thread.Sleep(3000);
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                }
                                catch { }
                                //PID Statement Details Table:
                                string tableassess = "";
                                var chromeOptions = new ChromeOptions();
                                var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];
                                chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                                chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                                chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                                chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                                var driver1 = new ChromeDriver(chromeOptions);
                                driver1.Navigate().GoToUrl(driver.Url);
                                try
                                {

                                    string fileName = parcelNumber + "-current-tax-statement.pdf";
                                    IWebElement Receipttable = driver1.FindElement(By.XPath("//*[@id='wrap']/div[4]/div[1]/div/div/div[1]/div[2]/form/input[2]"));
                                    string BillTax2 = Receipttable.GetAttribute("input");
                                    Receipttable.Click();
                                    Thread.Sleep(3000);
                                    gc.AutoDownloadFile(orderNumber, parcelNumber, "Galveston", "TX", fileName);
                                    string FilePath = gc.filePath(orderNumber, parcelNumber) + fileName;
                                    PdfReader reader;
                                    string pdfData;
                                    reader = new PdfReader(FilePath);
                                    String textFromPage = PdfTextExtractor.GetTextFromPage(reader, 1);
                                    System.Diagnostics.Debug.WriteLine("" + textFromPage);
                                    string pdftext = textFromPage;

                                    tableassess = gc.Between(pdftext, "*** OWNER NAME & ADDRESS *** *** ACCOUNT NUMBER *** *** PROPERTY DESCRIPTION ***", "LEAGUE CITY, TX 77573").Trim();
                                    string[] tableArray = tableassess.Split('\n');
                                    string Owner_Name = tableArray[3].Trim();
                                    string OwnerAddress1 = tableArray[6].Trim();
                                    string OwnerAddress2 = tableArray[7].Trim();
                                    string Owner_Address = OwnerAddress1 + OwnerAddress2.Trim();
                                    string AccountNumber = tableArray[0].Trim();

                                    string Dateofnotice1 = tableArray[8].Trim();
                                    string[] splitdateofnotice = Dateofnotice1.Split();
                                    string notice1 = splitdateofnotice[0].Trim() + "   ";
                                    string notice2 = splitdateofnotice[1].Trim();
                                    string Dateofnotice = notice1 + notice2.Trim();

                                    string Acreage1 = splitdateofnotice[3].Trim() + "   ";
                                    string Acreage2 = splitdateofnotice[4].Trim();
                                    string Acreage = Acreage1 + Acreage2.Trim();

                                    string ParcelAddress1 = tableArray[9].Trim();
                                    string ParcelAddress = GlobalClass.After(ParcelAddress1, "PARCEL ADDRESS ->").Trim();

                                    string PropertyDescription1 = tableArray[1].Trim() + "   ";
                                    string PropertyDescription2 = tableArray[2].Trim() + "   ";
                                    string PropertyDescription3 = tableArray[4].Trim();
                                    string PropertyDescription = PropertyDescription1 + PropertyDescription2 + PropertyDescription3.Trim();
                                    string AppraisalAndExemptions = tableArray[11].Trim();
                                    string AnnualAssessment1 = tableArray[18].Trim();
                                    string AnnualAssessment = GlobalClass.After(AnnualAssessment1, "ANNUAL ASSESSMENT").Trim();
                                    string AssessmentAmountDue1 = tableArray[21].Trim();
                                    string AssessmentAmountDue = GlobalClass.After(AssessmentAmountDue1, "ASSESSMENT AMOUNT DUE").Trim();
                                    string Taxauthority1 = tableArray[35].Trim().Replace("LINDAMOOD MARY RENEE", "");
                                    string Taxauthority2 = tableArray[36].Trim();
                                    string Taxauthority3 = tableArray[37].Trim();
                                    string Taxauthority = Taxauthority1 + Taxauthority2 + Taxauthority3;

                                    string Months = tableArray[23].Trim();
                                    string[] splitMonths = Months.Split();
                                    string Months1 = splitMonths[0].Trim();
                                    string Months2 = splitMonths[1].Trim();
                                    string Months3 = splitMonths[2].Trim();

                                    string Month1value = tableArray[25].Trim();

                                    string Month2and3value = tableArray[26].Trim();
                                    string[] splitMonth2and3value = Month2and3value.Split();
                                    string Month2value = splitMonth2and3value[0].Trim();
                                    string Month3value = splitMonth2and3value[1].Trim();

                                    string title = "OWNER NAME~OWNER ADDRESS~ACCOUNT NUMBER~DATE OF NOTICE~PARCEL ADDRESS~PROPERTY DESCRIPTION~ACREAGE~APPRAISAL AND EXEMPTIONS~ANNUAL ASSESSMENT~ASSESSMENT AMOUNT DUE~" + Months1 + "~" + Months2 + "~" + Months3 + "~Tax Authority";
                                    db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + title + "' where Id = '" + 1119 + "'");

                                    string PIDStatementdetails = Owner_Name.Trim() + "~" + Owner_Address.Trim() + "~" + AccountNumber.Trim() + "~" + Dateofnotice.Trim() + "~" + Acreage.Trim() + "~" + ParcelAddress.Trim() + "~" + PropertyDescription + "~" + AppraisalAndExemptions + "~" + AnnualAssessment + "~" + AssessmentAmountDue + "~" + Month1value + "~" + Month2value + "~" + Month3value + "~" + Taxauthority;
                                    gc.insert_date(orderNumber, parcelNumber, 1119, PIDStatementdetails, 1, DateTime.Now);

                                    //Current PID Details Table:
                                    //Please check splitcurrentpid this time only split count 35.So I was used this split count 35.
                                    string CurrentPID1 = tableArray[29].Trim();
                                    string[] splitcurrentpid = CurrentPID1.Split();
                                    string Currentyear = splitcurrentpid[0].Trim();
                                    string CurrentPIDStatementtitle1 = splitcurrentpid[1].Trim() + "   ";
                                    string CurrentPIDStatementtitle2 = splitcurrentpid[2].Trim() + "   ";
                                    string CurrentPIDStatementtitle3 = splitcurrentpid[3].Trim();
                                    string CurrentPIDStatementtitle = CurrentPIDStatementtitle1 + CurrentPIDStatementtitle2 + CurrentPIDStatementtitle3.Trim();

                                    string CurrentPIDStatementvalue1 = splitcurrentpid[32].Trim() + "   ";
                                    string CurrentPIDStatementvalue2 = splitcurrentpid[33].Trim() + "   ";
                                    string CurrentPIDStatementvalue3 = splitcurrentpid[34].Trim() + "   ";
                                    string CurrentPIDStatementvalue4 = splitcurrentpid[35].Trim();
                                    string CurrentPIDStatementvalue = CurrentPIDStatementvalue1 + CurrentPIDStatementvalue2 + CurrentPIDStatementvalue3 + CurrentPIDStatementvalue4.Trim();

                                    string TitleName = tableArray[30].Trim();
                                    string[] splittitlename = TitleName.Split();

                                    string TitleNametitle11 = splittitlename[1].Trim() + "   ";
                                    string TitleNametitle12 = splittitlename[2].Trim();
                                    string TitleNametitle1 = TitleNametitle11 + TitleNametitle12.Trim();

                                    string TitleNametitle21 = splittitlename[5].Trim() + "   ";
                                    string TitleNametitle22 = splittitlename[6].Trim();
                                    string TitleNametitle2 = TitleNametitle21 + TitleNametitle22.Trim();

                                    string TitleNametitle31 = splittitlename[9].Trim() + "   ";
                                    string TitleNametitle32 = splittitlename[10].Trim();
                                    string TitleNametitle3 = TitleNametitle31 + TitleNametitle32.Trim();

                                    string TitleNametitle41 = splittitlename[12].Trim() + "   ";
                                    string TitleNametitle42 = splittitlename[13].Trim();
                                    string TitleNametitle4 = TitleNametitle41 + TitleNametitle42.Trim();

                                    string Values = tableArray[31].Trim();
                                    string[] splittitleValues = Values.Split();
                                    string TitleNameValues1 = splittitleValues[0].Trim() + "   ";
                                    string TitleNameValues2 = splittitleValues[1].Trim() + "   ";
                                    string TitleNameValues31 = splittitleValues[2].Trim() + "  ";
                                    string TitleNameValues32 = splittitleValues[3].Trim() + "  ";
                                    string TitleNameValues33 = splittitleValues[4].Trim() + "  ";
                                    string TitleNameValues3 = TitleNameValues31 + TitleNameValues32 + TitleNameValues33;
                                    string TitleNameValues4 = splittitleValues[5].Trim();

                                    string Amountdue1 = tableArray[32].Trim() + "   ";
                                    string Amountoftrack1 = tableArray[34].Trim();
                                    string Amountoftracktitle = Amountdue1 + Amountoftrack1.Trim();
                                    string Amountoftrackvalue = tableArray[33].Trim();


                                    string strtitle = "Tax Year" + "~" + CurrentPIDStatementtitle + "~" + TitleNametitle1 + "~" + TitleNametitle2 + "~" + TitleNametitle3 + "~" + TitleNametitle4 + "~" + Amountoftracktitle;
                                    db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + strtitle + "' where Id = '" + 1121 + "'");

                                    string CurrentPIDdetails = Currentyear.Trim() + "~" + CurrentPIDStatementvalue.Trim() + "~" + TitleNameValues1.Trim() + "~" + TitleNameValues2.Trim() + "~" + TitleNameValues3.Trim() + "~" + TitleNameValues4.Trim() + "~" + Amountoftrackvalue.Trim();
                                    gc.insert_date(orderNumber, parcelNumber, 1121, CurrentPIDdetails, 1, DateTime.Now);

                                }
                                catch { }

                                //Payment Record Details Table:

                                string tablerecordpayment = "";

                                try
                                {

                                    string fileName1 = parcelNumber + "-current-year-record-payment.pdf";
                                    IWebElement Receipttable1 = driver1.FindElement(By.XPath("//*[@id='wrap']/div[4]/div[1]/div/div/div[1]/div[1]/form/input[2]"));
                                    string BillTax21 = Receipttable1.GetAttribute("input");
                                    Receipttable1.Click();
                                    Thread.Sleep(3000);
                                    gc.AutoDownloadFile(orderNumber, parcelNumber, "Galveston", "TX", fileName1);
                                    string FilePath1 = gc.filePath(orderNumber, parcelNumber) + fileName1;
                                    PdfReader reader1;
                                    string pdfData1;
                                    reader1 = new PdfReader(FilePath1);
                                    String textFromPage1 = PdfTextExtractor.GetTextFromPage(reader1, 1);
                                    System.Diagnostics.Debug.WriteLine("" + textFromPage1);
                                    string pdftext1 = textFromPage1;

                                    tablerecordpayment = GlobalClass.After(pdftext1, "THOMAS W. LEE, R.T.A. TAX ASSESSOR/COLLECTOR").Trim();
                                    string[] tableArray1 = tablerecordpayment.Split('\n');

                                    string PleaseMake = tableArray1[0].Trim();
                                    string PleaseMaketitle1 = GlobalClass.Before(PleaseMake, ":").Trim();
                                    string PleaseMakevalue1 = GlobalClass.After(PleaseMake, ":").Trim();
                                    string Acountnumbertitle = tableArray1[2].Trim();
                                    string Accountnumbertitle1 = gc.Between(Acountnumbertitle, "OWNER NAME AND ADDRESS", "PROPERTY DESCRIPTION").Trim();
                                    string Accountnumbervalue1 = tableArray1[3].Trim();
                                    string Cardnumbertitle1 = tableArray1[4].Trim();
                                    string Cardnumbervalue1 = tableArray1[9].Trim();
                                    string Dateofnoticetitle1 = tableArray1[12].Trim();
                                    string Dateofnoticevalue = tableArray1[13].Trim();
                                    string Dateofnoticevalue1 = GlobalClass.Before(Dateofnoticevalue, "ACREAGE:").Trim();

                                    string Yearbulktdatatitle = tableArray1[14].Trim();
                                    string[] splityearbulkdatatitle = Yearbulktdatatitle.Split();
                                    string yeartitle1 = splityearbulkdatatitle[0].Trim();
                                    string yeartitle2 = splityearbulkdatatitle[1].Trim();
                                    string yeartitle3 = splityearbulkdatatitle[2].Trim();
                                    string yeartitle41 = splityearbulkdatatitle[3].Trim() + "   ";
                                    string yeartitle42 = splityearbulkdatatitle[4].Trim();
                                    string yeartitle4 = yeartitle41 + yeartitle42.Trim();
                                    string yeartitle51 = splityearbulkdatatitle[5].Trim() + "   ";
                                    string yeartitle52 = splityearbulkdatatitle[6].Trim();
                                    string yeartitle5 = yeartitle51 + yeartitle52.Trim();


                                    string Yearbulktdatavalue = tableArray1[15].Trim();
                                    string[] splityearbulkdatavalue = Yearbulktdatavalue.Split();
                                    string yearvalue1 = splityearbulkdatavalue[0].Trim();
                                    string yearvalue2 = splityearbulkdatavalue[1].Trim();
                                    string yearvalue3 = splityearbulkdatavalue[2].Trim();
                                    string yearvalue4 = splityearbulkdatavalue[3].Trim();
                                    string yearvalue5 = splityearbulkdatavalue[4].Trim();

                                    string Totalamountreceived = tableArray1[16].Trim();
                                    string[] splitTotalamountreceived = Totalamountreceived.Split();
                                    string totalamount1 = splitTotalamountreceived[0].Trim() + "   ";
                                    string totalamount2 = splitTotalamountreceived[1].Trim() + "   ";
                                    string totalamount3 = splitTotalamountreceived[2].Trim();
                                    string totalamount = totalamount1 + totalamount2 + totalamount3.Trim();
                                    string totalamountvalue1 = splitTotalamountreceived[3].Trim();

                                    string Paymentdatecheck = tableArray1[17].Trim();
                                    string[] splitPaymentdate = Paymentdatecheck.Split();
                                    string Paymentdate1 = splitPaymentdate[0].Trim() + "   ";
                                    string Paymentdate2 = splitPaymentdate[1].Trim();

                                    string Paymentdatetitle = Paymentdate1 + Paymentdate2.Trim();
                                    string Paymentdatevalue = splitPaymentdate[2].Trim();

                                    string titlepaymentrecord = PleaseMaketitle1.Trim() + "~" + Accountnumbertitle1.Trim() + "~" + Cardnumbertitle1.Trim() + "~" + Dateofnoticetitle1.Trim() + "~" + yeartitle1.Trim() + "~" + yeartitle2.Trim() + "~" + yeartitle3.Trim() + "~" + yeartitle4.Trim() + "~" + yeartitle5.Trim() + "~" + totalamount.Trim() + "~" + Paymentdatetitle;
                                    db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + titlepaymentrecord + "' where Id = '" + 1122 + "'");

                                    string paymentrecorddetails = PleaseMakevalue1.Trim() + "~" + Accountnumbervalue1.Trim() + "~" + Cardnumbervalue1.Trim() + "~" + Dateofnoticevalue1.Trim() + "~" + yearvalue1.Trim() + "~" + yearvalue2.Trim() + "~" + yearvalue3.Trim() + "~" + yearvalue4.Trim() + "~" + yearvalue5.Trim() + "~" + totalamountvalue1.Trim() + "~" + Paymentdatevalue.Trim();
                                    gc.insert_date(orderNumber, parcelNumber, 1122, paymentrecorddetails, 1, DateTime.Now);

                                }
                                catch { }
                            }
                            catch { }
                            GalScenario8++;
                        }
                        //SCENARIO - 9 Utility Tax Service LLC
                        //Utility Tax Service Details
                        if ((msg1 == "M100" || msg1 == "(E1)141") && GalScenario9 < 1)
                        {
                            try
                            {

                                string strAccountNo = "", strOwnerName = "", strAddress = "", strServiceAdrress = "", strJuriID = "", strPropertyInfo = "", strTaxAuthority = "", strTaxYear = "", strDelinquentDate = "", strAcerage = "", PayCheck = "", AppraisedTitle = "", AppraisedValue = "", UTax = "", TaxingTitle = "", TaxingValue = "";
                                driver.Navigate().GoToUrl("http://www.utilitytaxservice.com/SrchAcct.aspx");
                                driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWSrchAcctCtl1_AcctNo")).SendKeys(GeographicID.Replace("-", "").Trim()); //GeographicID.Replace("-", "").Trim()
                                gc.CreatePdf(orderNumber, PropertyID, "Brazoria Utility Tax Search", driver, "TX", "Brazoria");
                                driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWSrchAcctCtl1_SrchButton")).SendKeys(Keys.Enter);
                                gc.CreatePdf(orderNumber, PropertyID, "Brazoria utility Tax Result", driver, "TX", "Brazoria");
                                driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_TSWSrchAcctCtl1_GridView1']/tbody/tr[2]/td[2]/a")).Click();
                                gc.CreatePdf(orderNumber, PropertyID, "Brazoria Tax Search Result", driver, "TX", "Brazoria");
                                strAccountNo = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_FormViewAcctInfo']/tbody/tr[2]/td/b")).Text;
                                strOwnerName = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_FormViewAcctInfo_SortNameLabel")).Text;
                                strAddress = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_FormViewAcctInfo_OwnerAddr2Label")).Text;
                                try
                                {
                                    strAddress = strAddress + " " + driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_FormViewAcctInfo_OwnerAddr3Label")).Text;
                                }
                                catch { }
                                try
                                {
                                    strAddress = strAddress + " " + driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_FormViewAcctInfo_OwnerAddr4Label")).Text;
                                }
                                catch { }
                                strServiceAdrress = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_FormViewAcctInfo_ServiceAddr1Label")).Text;
                                try
                                {
                                    strServiceAdrress = strServiceAdrress + " " + driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_FormViewAcctInfo_ServiceAddr2Label")).Text;
                                }
                                catch { }
                                strJuriID = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_FormViewEntity_EntityIDLabel")).Text;
                                strTaxAuthority = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_FormViewEntity_EntityNameLabel")).Text;
                                try
                                {
                                    strTaxAuthority = strTaxAuthority + " " + driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_FormViewEntity_Addr1Label")).Text;
                                    strTaxAuthority = strTaxAuthority + " " + driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_FormViewEntity_Addr2Label")).Text;
                                    strTaxAuthority = strTaxAuthority + " " + driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_FormViewEntity_Addr3Label")).Text;
                                    strTaxAuthority = strTaxAuthority + " " + driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_FormViewEntity_PhoneNoLabel")).Text;
                                }
                                catch { }
                                IWebElement IBulkData = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_FormViewAcctRoll"));
                                strTaxYear = gc.Between(IBulkData.Text, "Tax Year: ", "Statement Mail");
                                strDelinquentDate = GlobalClass.After(IBulkData.Text, "Delinquent Date:");
                                IWebElement IPropInfo = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_FormViewPropertyInfo"));
                                strPropertyInfo = gc.Between(IPropInfo.Text, "Property Information", "Acreage:");
                                strAcerage = GlobalClass.After(IPropInfo.Text, "Acreage:");
                                PayCheck = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_PayCheckName")).Text;
                                IWebElement IAppraise = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_GridViewValues"));
                                IList<IWebElement> IAppraiseRow = IAppraise.FindElements(By.TagName("tr"));
                                IList<IWebElement> IAppraiseTD;
                                foreach (IWebElement Appraise in IAppraiseRow)
                                {
                                    IAppraiseTD = Appraise.FindElements(By.TagName("td"));
                                    if (IAppraiseTD.Count != 0)
                                    {
                                        AppraisedTitle += IAppraiseTD[0].Text + "~";
                                        AppraisedValue += IAppraiseTD[1].Text + "~";

                                    }
                                }
                                db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + "Account Number~Owner Name~Owner Address~Service Address~Tax Year~Property Information~Acreage~Jurisdiction ID~" + AppraisedTitle + "Tax Authority" + "' where Id = '" + 1150 + "'");
                                string UPropertyDetails = strAccountNo + "~" + strOwnerName + "~" + strAddress.Trim() + "~" + strServiceAdrress.Trim() + "~" + strTaxYear + "~" + strPropertyInfo + "~" + strAcerage + "~" + strJuriID + "~" + AppraisedValue + strTaxAuthority;
                                gc.insert_date(orderNumber, strAccountNo, 1150, UPropertyDetails, 1, DateTime.Now);
                                IWebElement IUTax = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_GridViewTaxes"));
                                IList<IWebElement> IUTaxRow = IUTax.FindElements(By.TagName("tr"));
                                IList<IWebElement> IUTaxTD;
                                foreach (IWebElement Tax in IUTaxRow)
                                {
                                    IUTaxTD = Tax.FindElements(By.TagName("td"));
                                    if (IUTaxTD.Count != 0 && Tax.Text.Trim() != "")
                                    {
                                        UTax = strTaxYear + "~" + IUTaxTD[0].Text + "~" + IUTaxTD[1].Text + "~" + IUTaxTD[2].Text + "~" + IUTaxTD[3].Text + "~" + IUTaxTD[4].Text;
                                    }
                                }

                                IWebElement IUTaxLevy = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_GridViewYears"));
                                IList<IWebElement> IUTaxLevyRow = IUTaxLevy.FindElements(By.TagName("tr"));
                                IList<IWebElement> IUTaxLevyTD;
                                foreach (IWebElement Levy in IUTaxLevyRow)
                                {
                                    IUTaxLevyTD = Levy.FindElements(By.TagName("td"));
                                    if (IUTaxLevyTD.Count != 0 && !Levy.Text.Contains("Due for All Years"))
                                    {
                                        string UTaxLevy = strAccountNo + "~" + IUTaxLevyTD[0].Text + "~" + IUTaxLevyTD[1].Text + "~" + IUTaxLevyTD[2].Text;
                                        gc.insert_date(orderNumber, strAccountNo, 1152, UTaxLevy, 1, DateTime.Now);
                                    }
                                    if (IUTaxLevyTD.Count != 0 && Levy.Text.Contains("Due for All Years"))
                                    {
                                        string UTaxLevy = strAccountNo + "~" + IUTaxLevyTD[1].Text + "~" + "~" + IUTaxLevyTD[2].Text;
                                        gc.insert_date(orderNumber, strAccountNo, 1152, UTaxLevy, 1, DateTime.Now);
                                    }
                                }

                                IWebElement IUTaxPost = driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[3]/table/tbody/tr[3]/td/table[1]/tbody/tr[5]/td[2]/table[1]"));
                                IList<IWebElement> IUTaxPostRow = IUTaxPost.FindElements(By.TagName("tr"));
                                IList<IWebElement> IUTaxPostTD;
                                foreach (IWebElement Post in IUTaxPostRow)
                                {
                                    IUTaxPostTD = Post.FindElements(By.TagName("td"));
                                    if (IUTaxPostTD.Count != 0 && IUTaxPostTD.Count == 2)
                                    {
                                        TaxingTitle += IUTaxPostTD[0].Text + "~";
                                        TaxingValue += IUTaxPostTD[1].Text + "~";
                                    }
                                    if (IUTaxPostTD.Count != 0 && IUTaxPostTD.Count == 3 && !Post.Text.Contains("If Postmarked"))
                                    {
                                        string UTaxPost = strAccountNo + "~" + IUTaxPostTD[0].Text + "~" + IUTaxPostTD[1].Text + "~" + IUTaxPostTD[2].Text;
                                        gc.insert_date(orderNumber, strAccountNo, 1153, UTaxPost, 1, DateTime.Now);
                                    }
                                }

                                db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + "Account Number~Tax Year~Taxing Unit~Exempt Amount~Taxable Value~Tax Rate~Tax Levy~" + TaxingTitle + "Delinquent Date~Make Checks Payable" + "' where Id = '" + 1151 + "'");
                                string UTaxDetails = strAccountNo + "~" + UTax + "~" + TaxingValue + strDelinquentDate + "~" + PayCheck;
                                gc.insert_date(orderNumber, strAccountNo, 1151, UTaxDetails, 1, DateTime.Now);
                            }
                            catch { }
                            GalScenario9++;
                        }
                    }
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "TX", "Galveston", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    driver.Quit();
                    driver.Dispose();
                    gc.mergpdf(orderNumber, "TX", "Galveston");
                    return "Data Inserted Successfully";
                }
                catch (Exception ex)
                {
                    driver.Quit();
                    driver.Dispose();
                    throw ex;
                }
            }
        }

    }
}