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
    public class WebDriver_TXBrazoria
    {
        IWebElement PaidCount;
        IWebDriver driver;
        DBconnection db = new DBconnection();
        List<string> link = new List<string>();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_Brazoria(string houseno, string sname, string stype, string unitno, string direction, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            List<string> taxinformation = new List<string>();
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //new PhantomJSDriver()new ChromeDriver()
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("https://propaccess.trueautomation.com/ClientDB/PropertySearch.aspx?cid=51");
                    //var SelectY = driver.FindElement(By.Id("propertySearchOptions_taxyear"));
                    //var selectElementY = new SelectElement(SelectY);
                    //selectElementY.SelectByIndex(1);
                    if (searchType == "titleflex")
                    {
                        string titleaddress = houseno + " " + sname + " " + stype + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "TX", "Brazoria");
                        if (HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes")
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_TXBrazoria"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    driver.FindElement(By.Id("propertySearchOptions_advanced")).Click();
                    Thread.Sleep(2000);

                    if (searchType == "address")
                    {
                    
                        driver.FindElement(By.Id("propertySearchOptions_streetNumber")).SendKeys(houseno);
                        driver.FindElement(By.Id("propertySearchOptions_streetName")).SendKeys(sname);
                        gc.CreatePdf_WOP(orderNumber, "address search", driver, "TX", "Brazoria");
                        driver.FindElement(By.Id("propertySearchOptions_searchAdv")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "address search result", driver, "TX", "Brazoria");

                    }
                    if (searchType == "parcel")
                    {
                        var Select1 = driver.FindElement(By.Id("propertySearchOptions_searchType"));
                        var selectElement11 = new SelectElement(Select1);
                        selectElement11.SelectByText("Account Number");
                        driver.FindElement(By.Id("propertySearchOptions_propertyid")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search", driver, "TX", "Brazoria");
                        driver.FindElement(By.Id("propertySearchOptions_searchAdv")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search result", driver, "TX", "Brazoria");
                    }

                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("propertySearchOptions_ownerName")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "owner search", driver, "TX", "Brazoria");
                        driver.FindElement(By.Id("propertySearchOptions_searchAdv")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "owner search result", driver, "TX", "Brazoria");
                    }
                    if (searchType == "block")
                    {
                        var Select1 = driver.FindElement(By.Id("propertySearchOptions_searchType"));
                        var selectElement11 = new SelectElement(Select1);
                        selectElement11.SelectByText("Account Number");
                        driver.FindElement(By.Id("propertySearchOptions_geoid")).SendKeys(unitno);
                        gc.CreatePdf_WOP(orderNumber, "Account Number Search ", driver, "TX", "Brazoria");
                        driver.FindElement(By.Id("propertySearchOptions_searchAdv")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Account Number Search  result", driver, "TX", "Brazoria");

                        //       gc.CreatePdf_WOP(orderNumber, "owner search result", driver, "TX", "Williamson");
                    }
                    //Geographic ID~Type~Property Address~Owner Name
                    //*[@id="propertySearchResults_resultsTable"]
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
                                    gc.insert_date(orderNumber, TDmulti5[1].Text, 1070, multi1, 1, DateTime.Now);
                                }
                                maxCheck++;
                            }
                        }
                        if (TRmulti5.Count > 25)
                        {
                            HttpContext.Current.Session["multiParcel_Brazoria_Multicount"] = "Maximum";
                        }
                        else
                        {
                            HttpContext.Current.Session["multiparcel_Brazoria"] = "Yes";
                        }

                        driver.Quit();
                        return "MultiParcel";
                    }
                    else
                    {
                        string type = driver.FindElement(By.XPath("//*[@id='propertySearchResults_resultsTable']/tbody/tr[2]/td[4]")).Text.Replace("\r\n", " ");
                        if (type == "Real")
                        {
                            IWebElement Aherf = driver.FindElement(By.XPath("//*[@id='propertySearchResults_resultsTable']/tbody/tr[2]/td[10]")).FindElement(By.TagName("a"));
                            string Href = Aherf.GetAttribute("href");
                            driver.Navigate().GoToUrl(Href);
                            //Aherf.Click();
                            Thread.Sleep(2000);
                        }
                        else
                        {
                            HttpContext.Current.Session["alert_msg"] = "Yes";

                            driver.Quit();
                            return "MultiParcel";
                        }

                    }

                    //property details
                    string PropertyID = "", GeographicID = "", Name = "", MailingAddress = "", OwnerID = "", LegalDescription = "", Neighborhood = "", Type = "", Address = "", NeighborhoodCD = "", Exemptions = "", MapID = "", YearBuilt = "", Acres = "";
                    List<string> entity = new List<string>();
                    gc.CreatePdf_WOP(orderNumber, "First Page", driver, "TX", "Brazoria");
                    entity.AddRange(new string[] { "M16", "S12", "CCL", "M2", "M9", "M10", "M17", "M18", "M19", "M21", "M22", "M23", "M24", "M25", "M26", "M28", "M29", "M31", "M32", "M34", "M35", "M36", "M39", "M40", "M42", "M55", "M61", "M11", "M100", "(E1)141" });
                    string fulltext = driver.FindElement(By.XPath("//*[@id='propertyDetails']/table/tbody")).Text.Replace("\r\n", " ");
                    PropertyID = gc.Between(fulltext, "Account Property ID:", " Legal Description: ");
                    string property_use_code = "", property_use_dis = "", ownership = "";
                    GeographicID = gc.Between(fulltext, "Geographic ID: ", "Agent Code: ");
                    Name = gc.Between(fulltext, "Owner Name:", "Owner ID:");
                    MailingAddress = gc.Between(fulltext, "Mailing Address:", " % Ownership:");
                    OwnerID = gc.Between(fulltext, "Owner ID:", "Mailing Address:");
                    LegalDescription = gc.Between(fulltext, "Legal Description:", "Geographic ID:");
                    Neighborhood = gc.Between(fulltext, "Neighborhood:", "Map ID:");
                    Type = gc.Between(fulltext, "Type:", "Property Use Code:");
                    Address = gc.Between(fulltext, "Location Address:", "Mapsco:");
                    NeighborhoodCD = gc.Between(fulltext, "Neighborhood CD:", "Owner");
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
                    //Property ID~Geographic ID~Type~Property Use Code~Property Use Description~Legal Description~Property Address~Neighborhood~Neighborhood CD~Owner Name~Mailing Address~Owner ID~% Ownership~Map ID~Exemptions~Year Built~Acres
                    string property_details = PropertyID + "~" + GeographicID + "~" + Type + "~" + property_use_code + "~" + property_use_dis + "~" + LegalDescription + "~" + Address + "~" + Neighborhood + "~" + NeighborhoodCD + "~" + Name + "~" + MailingAddress + "~" + OwnerID + "~" + ownership + "~" + MapID + "~" + Exemptions + "~" + YearBuilt + "~" + Acres;
                    gc.insert_date(orderNumber, PropertyID, 1064, property_details, 1, DateTime.Now);

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
                            gc.insert_date(orderNumber, PropertyID, 1065, ValueDetails, 1, DateTime.Now);

                        }
                    }


                    //Taxing Jurisdiction Details Table:
                    //Owner~% Ownership~Total Value~Entity~Description~Tax Rate~Appraised Value~Taxable Value~Estimated Tax
                    driver.FindElement(By.Id("taxingJurisdiction")).Click();
                    Thread.Sleep(2000);
                    string owner = "", Ownership = "", TotalValue = "";
                    string ValueDetails1 = "", msg = "";
                    string fulltext1 = driver.FindElement(By.XPath("//*[@id='taxingJurisdictionDetails']/table[1]/tbody")).Text.Replace("\r\n", " ");
                    string entityname = "";
                    owner = gc.Between(fulltext1, "Owner:", "% Ownership:");
                    Ownership = gc.Between(fulltext1, "% Ownership:", "Total Value:");
                    TotalValue = GlobalClass.After(fulltext1, "Total Value:");
                    ValueDetails1 = owner + "~" + Ownership + "~" + TotalValue + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                    gc.insert_date(orderNumber, PropertyID, 1066, ValueDetails1, 1, DateTime.Now);
                    List<string> strTaxSearch = new List<string>();
                    string city = "", code = "", jurdsition = "", Authority = "", urlAuth = "";
                    IWebElement multitableElement11 = driver.FindElement(By.XPath("//*[@id='taxingJurisdictionDetails']/table[2]/tbody"));
                    IList<IWebElement> multitableRow11 = multitableElement11.FindElements(By.TagName("tr"));
                    IList<IWebElement> multirowTD11;
                    foreach (IWebElement row in multitableRow11)
                    {
                        multirowTD11 = row.FindElements(By.TagName("td"));
                        if (multirowTD11.Count != 0)
                        {

                            if (multirowTD11[2].Text.Trim() != "0.000000" && multirowTD11[0].Text.Trim() != "" && entity.Any(str => str.Contains(multirowTD11[0].Text)))
                            {
                                strTaxSearch.Add(multirowTD11[0].Text);
                                entityname = multirowTD11[1].Text;
                            }
                            ValueDetails1 = " " + "~" + " " + "~" + " " + "~" + multirowTD11[0].Text.Trim() + "~" + multirowTD11[1].Text.Trim() + "~" + multirowTD11[2].Text.Trim() + "~" + multirowTD11[3].Text.Trim() + "~" + multirowTD11[4].Text.Trim() + "~" + multirowTD11[5].Text.Trim();
                            gc.insert_date(orderNumber, PropertyID, 1066, ValueDetails1, 1, DateTime.Now);
                            //if(multirowTD11[1].Text.Trim().Contains("BRAZORIA COUNTY MUD") && multirowTD11[1].Text.Trim().Contains("0.000000"))
                            //{
                            //    strTaxSearch.Add(multirowTD11[1].Text);
                            //}
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
                            gc.insert_date(orderNumber, PropertyID, 1067, rollDetails, 1, DateTime.Now);
                        }
                    }

                    //deed history

                    driver.FindElement(By.Id("deedHistory")).Click();
                    Thread.Sleep(2000);

                    gc.CreatePdf(orderNumber, PropertyID, "property details", driver, "TX", "Brazoria");
                    string TaxAuthority = "";
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    try
                    {
                        driver.Navigate().GoToUrl("https://actweb.acttax.com/act_webdev/brazoria/index.jsp");
                        Thread.Sleep(3000);

                        driver.FindElement(By.Id("sc5")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.Id("criteria")).SendKeys(PropertyID);
                        gc.CreatePdf(orderNumber, PropertyID, "tax search", driver, "TX", "Brazoria");
                        driver.FindElement(By.Name("submit")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, PropertyID, "tax search result", driver, "TX", "Brazoria");

                        driver.FindElement(By.XPath("//*[@id='printTable']/tbody/tr/td/table/tbody/tr/td[2]/h3/a")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, PropertyID, "tax search result details", driver, "TX", "Brazoria");
                        //Jurisdiction Information Details Table: 
                        string year = "", account = "", ExemptionsJ = "";


                        driver.FindElement(By.LinkText("Taxes Due Detail by Year and Jurisdiction")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, PropertyID, "Taxes Due Detail by Year and Jurisdiction", driver, "TX", "Brazoria");
                        //Taxes Due Detail by Year Details Table: 
                        //Account No~Appr. Dist. No~Active Lawsuits~Year~Base Tax Due~Penalty, Interest, and ACC* Due(end of July )~Total Due July~Penalty, Interest, and ACC* Due(end of August )~Total Due August~Penalty, Interest, and ACC*Due(end of September)~Total Due September
                        string accountno = "", distno = "", ActiveLawsuits = "";

                        //Year~Base Tax Due~Penalty, Interest, and ACC* Due~Total Due~Penalty, Interest, and ACC* Due1~Total Due1~Penalty, Interest, and ACC*Due2~Total Due2

                        IWebElement multitableElement31 = driver.FindElement(By.XPath("//*[@id='pageContent']/center/div/table[1]/tbody/tr/td/center/table/tbody/tr/td/table/tbody"));
                        IList<IWebElement> multitableRow31 = multitableElement31.FindElements(By.TagName("tr"));
                        IList<IWebElement> multirowTD31;
                        foreach (IWebElement row in multitableRow31)
                        {
                            multirowTD31 = row.FindElements(By.TagName("td"));
                            if (multirowTD31.Count == 8 && !row.Text.Contains("Base Tax Due"))
                            {
                                string TaxesDue = multirowTD31[0].Text.Trim() + "~" + multirowTD31[1].Text.Trim() + "~" + multirowTD31[2].Text.Trim() + "~" + multirowTD31[3].Text.Trim() + "~" + multirowTD31[4].Text.Trim() + "~" + multirowTD31[5].Text.Trim() + "~" + multirowTD31[6].Text.Trim() + "~" + multirowTD31[7].Text.Trim();
                                gc.insert_date(orderNumber, PropertyID, 1068, TaxesDue, 1, DateTime.Now);
                            }
                            if (multirowTD31.Count == 1)
                            {
                                string TaxesDue = multirowTD31[0].Text.Trim() + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                gc.insert_date(orderNumber, PropertyID, 1068, TaxesDue, 1, DateTime.Now);

                            }
                            if (multirowTD31.Count == 4)
                            {
                                string TaxesDue = "" + "~" + "" + "~" + multirowTD31[1].Text.Trim() + "~" + "" + "~" + multirowTD31[2].Text.Trim() + "~" + "" + "~" + multirowTD31[3].Text.Trim() + "~" + "";
                                gc.insert_date(orderNumber, PropertyID, 1068, TaxesDue, 1, DateTime.Now);

                            }
                        }

                        driver.FindElement(By.LinkText("Return to the Previous Page")).Click();
                        Thread.Sleep(2000);
                        //Tax Payment Details Table: 

                        driver.FindElement(By.LinkText("Payment Information")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, PropertyID, "Payment Information", driver, "TX", "Brazoria");
                        //Account Number~Paid Date~Amount~Tax Year~Description~Paid By
                        string accountnos = driver.FindElement(By.XPath("//*[@id='pageContent']/center/div/table[1]/tbody/tr/td/h3[3]")).Text.Replace("Account No.:", "");

                        IWebElement multitableElement32 = driver.FindElement(By.XPath("//*[@id='pageContent']/center/div/table[1]/tbody/tr/td/table/tbody"));
                        IList<IWebElement> multitableRow32 = multitableElement32.FindElements(By.TagName("tr"));
                        IList<IWebElement> multirowTD32;
                        foreach (IWebElement row in multitableRow32)
                        {
                            multirowTD32 = row.FindElements(By.TagName("td"));
                            if (multirowTD32.Count != 0 && !row.Text.Contains("Receipt Date"))
                            {
                                string TaxesDue = accountnos + "~" + multirowTD32[0].Text.Trim() + "~" + multirowTD32[1].Text.Trim() + "~" + multirowTD32[2].Text.Trim() + "~" + multirowTD32[3].Text.Trim() + "~" + multirowTD32[4].Text.Trim();
                                gc.insert_date(orderNumber, PropertyID, 1069, TaxesDue, 1, DateTime.Now);
                            }
                        }

                        driver.FindElement(By.LinkText("Return to the Previous Page")).Click();
                        Thread.Sleep(2000);


                        string jurd = "", pendingpayment = "", accountNo = "", AppraisalDistrict = "", OwnerName = "", OwnerAddress = "", PropertyAddress = "", legal = "", CurrentTax = "", CurrentAmount = "", PriorYearAmount = "", TotalAmount = "", LastPaymentAmount = "", LastPayer = "", LastPaymentDate = "", ActiveLawsuitsTax = "", GrossValue = "", LandValue = "", ImprovementValue = "", CappedValue = "", AgriculturalValue = "", ExemptionsTax = "";
                        string fullTaxeBill1 = driver.FindElement(By.XPath("//*[@id='pageContent']/center/div/table[1]/tbody/tr[2]/td/table[2]/tbody")).Text.Replace("\r\n", " ");
                        accountno = gc.Between(fullTaxeBill1, "Account Number:", "Address:");
                        string market = "";
                        OwnerAddress = gc.Between(fullTaxeBill1, "Address:", "Property Site Address:");
                        PropertyAddress = gc.Between(fullTaxeBill1, "Property Site Address:", " Legal Description:");
                        legal = gc.Between(fullTaxeBill1, "Legal Description:", " Current Tax Levy:");
                        CurrentTax = gc.Between(fullTaxeBill1, " Current Tax Levy:", "Current Amount Due:");
                        CurrentAmount = gc.Between(fullTaxeBill1, "Current Amount Due:", "Prior Year Amount Due:");
                        PriorYearAmount = gc.Between(fullTaxeBill1, "Prior Year Amount Due:", "Total Amount Due:");
                        TotalAmount = gc.Between(fullTaxeBill1, "Total Amount Due:", "Last Payment Amount for Current Year Taxes:");
                        LastPaymentAmount = gc.Between(fullTaxeBill1, "Last Payment Amount for Current Year Taxes:", "Active Lawsuits:");
                        //LastPayer = gc.Between(fullTaxeBill1, "Last Payer for Current Year Taxes:", "Last Payment Date for Current Year Taxes:");
                        // LastPaymentDate = gc.Between(fullTaxeBill1, "Last Payment Date for Current Year Taxes:", "Active Lawsuits:");
                        //  string tax_year = driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr/td/table/tbody/tr[1]/td/table[1]/tbody/tr/td/h5/b")).Text;
                        //  tax_year = gc.Between(tax_year, " tax information for", ". All amounts");
                        ActiveLawsuitsTax = gc.Between(fullTaxeBill1, "Active Lawsuits:", "Pending Credit Card");
                        //   pendingpayment = gc.Between(fullTaxeBill1, "Pending Credit Card or E–Check Payments:", "Market Value:");
                        market = gc.Between(fullTaxeBill1, "Market Value:", "Land Value:");
                        LandValue = gc.Between(fullTaxeBill1, "Land Value:", "Improvement Value:");
                        ImprovementValue = gc.Between(fullTaxeBill1, "Improvement Value:", "Capped Value:");
                        CappedValue = gc.Between(fullTaxeBill1, "Capped Value:", "Agricultural Value:");
                        AgriculturalValue = gc.Between(fullTaxeBill1, "Agricultural Value:", "Exemptions:");
                        ExemptionsTax = gc.Between(fullTaxeBill1, "Exemptions:", "Last Certified Date:");
                        string lastCertified = gc.Between(fullTaxeBill1, "Last Certified Date: ", "Taxes Due Detail by Year and Jurisdiction ");
                        try
                        {
                            TaxAuthority = GlobalClass.After(driver.FindElement(By.XPath("//*[@id='pageContent']/center/div/table[3]/tbody/tr/td[1]")).Text, "E-mail: roving@brazoria-county.com").Replace("\r\n", " ");
                        }
                        catch { }
                        //Account Number~Owner Information~Property Site Address~Legal Description~Current Tax Levy~Current Amount Due~Prior Year Amount Due~Total Amount Due~Last Payment Amount for Current Year Taxes~Active Lawsuits~market~Land Value~Improvement Value~Capped Value~Agricultural Value~Exemptions~Last Certified Date~Tax Authority

                        string taxbill = accountno + "~" + OwnerAddress + "~" + PropertyAddress + "~" + legal + "~" + CurrentTax + "~" + CurrentAmount + "~" + PriorYearAmount + "~" + TotalAmount + "~" + LastPaymentAmount + "~" + ActiveLawsuitsTax + "~" + market + "~" + LandValue + "~" + ImprovementValue + "~" + CappedValue + "~" + AgriculturalValue + "~" + ExemptionsTax + "~" + lastCertified + "~" + TaxAuthority;
                        gc.insert_date(orderNumber, PropertyID, 1071, taxbill, 1, DateTime.Now);

                        IWebElement Itaxstmt1 = driver.FindElement(By.XPath("//*[@id='pageContent']/center/div/table[1]/tbody/tr[2]/td/table[2]/tbody/tr/td[2]/h3[11]/a[3]"));
                        Thread.Sleep(2000);
                        string stmt11 = Itaxstmt1.GetAttribute("href");
                        driver.Navigate().GoToUrl(stmt11);
                        Thread.Sleep(4000);
                        IWebElement Itaxstmt = driver.FindElement(By.XPath("//*[@id='pageContent']/center/div/table[1]/tbody/tr[3]/td/div/h3/a"));
                        string stmt1 = Itaxstmt.GetAttribute("href");
                        gc.downloadfile(stmt1, orderNumber, PropertyID, "Tax statement", "TX", "Brazoria");
                        driver.Navigate().Back();
                        Thread.Sleep(5000);

                        driver.FindElement(By.XPath("//*[@id='pageContent']/center/div/table[1]/tbody/tr[2]/td/table[2]/tbody/tr/td[2]/a")).Click();
                        Thread.Sleep(2000);
                        //Jurisdiction Information for ~Account No~Exemptions~Jurisdictions~Market Value~Exemption Value~Taxable Value~Tax Rate~Levy
                        string accountnosjur = driver.FindElement(By.XPath("//*[@id='pageContent']/center/div/table[1]/tbody/tr/td/table/tbody/tr[1]/td/h3[2]")).Text.Replace("Account No.:", "");
                        string jurdinfoYear = driver.FindElement(By.XPath("//*[@id='pageContent']/center/div/table[1]/tbody/tr/td/table/tbody/tr[1]/td/h3[1]/b")).Text.Replace("Jurisdiction Information for", "");
                        string exemptions = driver.FindElement(By.XPath("//*[@id='pageContent']/center/div/table[1]/tbody/tr/td/table/tbody/tr[1]/td/h3[3]")).Text.Replace("Exemptions:", "");

                        IWebElement multitableElement6 = driver.FindElement(By.XPath("//*[@id='pageContent']/center/div/table[1]/tbody/tr/td/table/tbody/tr[2]/td/table/tbody"));
                        IList<IWebElement> multitableRow6 = multitableElement6.FindElements(By.TagName("tr"));
                        IList<IWebElement> multirowTD6;
                        foreach (IWebElement row in multitableRow6)
                        {
                            multirowTD6 = row.FindElements(By.TagName("td"));
                            if (multirowTD6.Count != 0 && !row.Text.Contains("Jurisdictions"))
                            {
                                string TaxesDue = jurdinfoYear + "~" + accountnosjur + "~" + exemptions + "~" + multirowTD6[0].Text.Trim() + "~" + multirowTD6[1].Text.Trim() + "~" + multirowTD6[2].Text.Trim() + "~" + multirowTD6[3].Text.Trim() + "~" + multirowTD6[4].Text.Trim() + "~" + multirowTD6[5].Text.Trim();
                                gc.insert_date(orderNumber, PropertyID, 1072, TaxesDue, 1, DateTime.Now);
                            }
                        }

                        driver.FindElement(By.LinkText("Return to the Previous Page")).Click();
                        Thread.Sleep(2000);

                        string BrazoriaTaxAuthority = "";
                        int Scenario1 = 0, Scenario2 = 0;
                        foreach (string msg1 in strTaxSearch)
                        {
                            if ((msg1 == "M2" || msg1 == "M9" || msg1 == "M10" || msg1 == "M17" || msg1 == "M18" || msg1 == "M19" || msg1 == "M21" || msg1 == "M22" || msg1 == "M23" || msg1 == "M24" || msg1 == "M25" || msg1 == "M26" || msg1 == "M28" || msg1 == "M29" || msg1 == "M31" || msg1 == "M32" || msg1 == "M34" || msg1 == "M35" || msg1 == "M36" || msg1 == "M39" || msg1 == "M40" || msg1 == "M42" || msg1 == "M55" || msg1 == "M61" || msg1 == "M11") && Scenario1 < 1)
                            {
                                try
                                {
                                    driver.Navigate().GoToUrl("http://www.bcmud21.com/tax-matters/");
                                    IWebElement IAddress = driver.FindElement(By.XPath("//*[@id='post-226']/div/div/p[5]"));
                                    BrazoriaTaxAuthority = "Brazoria County Municipal Utility District 21" + " " + gc.Between(IAddress.Text, "Assessments of the Southwest, Inc.", "Website:").Replace("\r\n", "");
                                    gc.CreatePdf(orderNumber, PropertyID, "Tax Authority", driver, "TX", "Brazoria");
                                }
                                catch { }

                                try
                                {
                                    driver.Navigate().GoToUrl("http://aswportal.azurewebsites.net/search/list");
                                    Thread.Sleep(3000);
                                    driver.FindElement(By.Id("tag")).SendKeys(GeographicID.Trim().Replace("-", ""));
                                    driver.FindElement(By.Id("tag")).SendKeys(Keys.Enter);
                                    gc.CreatePdf(orderNumber, PropertyID, "Brazoria Tax Search", driver, "TX", "Brazoria");
                                    IWebElement ITaxClick = driver.FindElement(By.XPath("//*[@id='searchlist']/div/div/div/div/table"));
                                    IList<IWebElement> ITaxRow = ITaxClick.FindElements(By.TagName("tr"));
                                    foreach (IWebElement tax in ITaxRow)
                                    {
                                        if (tax.Text.Contains(GeographicID.Replace("-", "")))
                                        {
                                            string strClick = tax.GetAttribute("id");
                                            if (strClick.Contains("rowid"))
                                            {
                                                IWebElement Iclick = driver.FindElement(By.Id(strClick));
                                                Iclick.Click();
                                                Thread.Sleep(3000);
                                            }
                                        }
                                    }
                                    //driver.FindElement(By.Id("rowid-" + GeographicID.Replace("-", "").Trim() + "")).SendKeys(Keys.Enter);
                                    gc.CreatePdf(orderNumber, PropertyID, "Brazoria Tax Result", driver, "TX", "Brazoria");
                                    string propOwner = "", propID = "", propGEO = "", propSitus = "", propLegal = "", propExemption = "", propValTitle = "", propValValues = "", propPaid = "", propADDN = "", propDue = "", propYear = "", propTaxingEntity = "", propExemptions = "", propTaxable = "", propTaxRate = "", propTaxAmount = "", propTaxDue = "", propDueADDN = "", propTotalDue = "",
                                    propSTYear = "", propSTBill = "", propSTExemptions = "", propSTTaxable = "", propSTTax = "", propSTPaid = "", propSTPI = "", propSTAtty = "", propSTADDN = "", propSTDue = "", propSTDeferred = "", propSTOmitted = "", propSTSuit = "", propSTBalance = "", propRCPT = "", propRCPTDate = "", propRPostDate = "", propRCheck = "", propRPaid1 = "", propRMO = "", propRPaid2 = "", propRCC = "", propRCCType = "", propRPaid3 = "", propROther = "", propRAmountPaid = "", propRCashPaid = "", propRVoid = "", propRCode = "", propRDate = "";
                                    //MUD Property Details
                                    propOwner = driver.FindElement(By.XPath("//*[@id='cardlist']/div/div[1]/div/div[2]/div[1]/div/table/tbody/tr/td")).Text.Replace("\r\n", " ");
                                    IWebElement IPropId = driver.FindElement(By.XPath("//*[@id='cardlist']/div/div[1]/div/div[3]/div[1]/div/table/tbody/tr/td"));
                                    propID = gc.Between(IPropId.Text, "PROP ID:", "GEOID:").Trim();
                                    propGEO = gc.Between(IPropId.Text, "GEOID:", "SITUS:").Trim();
                                    propSitus = GlobalClass.After(IPropId.Text, "SITUS:").Trim();
                                    propLegal = driver.FindElement(By.XPath("//*[@id='cardlist']/div/div[1]/div/div[3]/div[2]/div/table/tbody/tr/td")).Text.Trim();
                                    propExemption = driver.FindElement(By.XPath("//*[@id='cardlist']/div/div[1]/div/div[3]/div[3]/div/table/tbody/tr/td")).Text.Trim();
                                    string PropertyDetails = propGEO + "~" + propOwner + "~" + propSitus + "~" + propLegal + "~" + propExemption + "~" + BrazoriaTaxAuthority.Trim();
                                    gc.insert_date(orderNumber, propID, 1073, PropertyDetails, 1, DateTime.Now);
                                    //MUD Valuation Summary Details
                                    IWebElement IPropValue = driver.FindElement(By.XPath("//*[@id='cardlist']/div/div[1]/div/div[3]/div[4]/div/table/tbody"));
                                    IList<IWebElement> IPropValueRow = IPropValue.FindElements(By.TagName("tr"));
                                    IList<IWebElement> IPropValueTD;
                                    foreach (IWebElement Value in IPropValueRow)
                                    {
                                        IPropValueTD = Value.FindElements(By.TagName("td"));
                                        if (IPropValueTD.Count != 0)
                                        {
                                            propValTitle = IPropValueTD[0].Text.Replace("\r\n", "~").Trim();
                                            propValValues = IPropValueTD[1].Text.Replace("\r\n", "~").Trim();
                                        }
                                    }
                                    db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + propValTitle + "' where Id = '" + 1080 + "'");
                                    gc.insert_date(orderNumber, propID, 1080, propValValues, 1, DateTime.Now);

                                    //MUD Payment Details
                                    IWebElement IPropPayment = driver.FindElement(By.XPath("//*[@id='cardlist']/div/div[1]/div/div[2]/div[3]/div/table/tbody"));
                                    IList<IWebElement> IPropPaymentRow = IPropPayment.FindElements(By.TagName("tr"));
                                    IList<IWebElement> IPropPaymentTD;
                                    foreach (IWebElement Pay in IPropPaymentRow)
                                    {
                                        IPropPaymentTD = Pay.FindElements(By.TagName("td"));
                                        if (IPropPaymentTD.Count != 0)
                                        {
                                            propPaid = IPropPaymentTD[0].Text;
                                            propADDN = IPropPaymentTD[1].Text;
                                            propDue = IPropPaymentTD[2].Text;

                                            string propPaymentDetails = propPaid + "~" + propADDN + "~" + propDue;
                                            gc.insert_date(orderNumber, propID, 1081, propPaymentDetails, 1, DateTime.Now);
                                        }
                                    }

                                    //MUD Tax Due Details
                                    IWebElement IPropDue = driver.FindElement(By.XPath("//*[@id='cardlist']/div/div[1]/div/div[4]/div/div/table/tbody"));
                                    IList<IWebElement> IPropDueRow = IPropDue.FindElements(By.TagName("tr"));
                                    IList<IWebElement> IPropDueTD;
                                    foreach (IWebElement Due in IPropDueRow)
                                    {
                                        IPropDueTD = Due.FindElements(By.TagName("td"));
                                        if (IPropDueTD.Count != 0 && !Due.Text.Contains("============") && IPropDueTD[7].Text != "" && IPropDueTD[6].Text != "")
                                        {
                                            propYear = IPropDueTD[0].Text;
                                            propTaxingEntity = IPropDueTD[1].Text;
                                            propExemptions = IPropDueTD[2].Text;
                                            propTaxable = IPropDueTD[3].Text;
                                            propTaxRate = IPropDueTD[4].Text;
                                            propTaxAmount = IPropDueTD[5].Text;
                                            propTaxDue = IPropDueTD[6].Text;
                                            propDueADDN = IPropDueTD[7].Text;
                                            propTotalDue = IPropDueTD[8].Text;

                                            string propDueDetails = propYear + "~" + propTaxingEntity + "~" + propExemptions + "~" + propTaxable + "~" + propTaxRate + "~" + propTaxAmount + "~" + propTaxDue + "~" + propDueADDN + "~" + propTotalDue;
                                            gc.insert_date(orderNumber, propID, 1082, propDueDetails, 1, DateTime.Now);
                                        }
                                    }

                                    //MUD Tax Statement Details
                                    driver.FindElement(By.LinkText("STATEMENTS")).Click(); //html/body/div[2]/section/div/ul/li[2]/a
                                    gc.CreatePdf(orderNumber, PropertyID, "Brazoria Tax Statement", driver, "TX", "Brazoria");
                                    int j = 0, StateBill = 0;
                                    int stateyear = DateTime.Now.Year;
                                    List<string> srStatement = new List<string>();
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

                                            string propDueDetails = propSTYear + "~" + propSTBill + "~" + propSTExemptions + "~" + propSTTaxable + "~" + propSTTax + "~" + propSTPaid + "~" + propSTBalance + "~" + propSTPI + "~" + propSTAtty + "~" + propSTADDN + "~" + propSTDue + "~" + propSTDeferred + "~" + propSTOmitted + "~" + propSTSuit;
                                            gc.insert_date(orderNumber, propID, 1083, propDueDetails, 1, DateTime.Now);
                                        }
                                        if (IPropStatementTD.Count != 0 && j < IPropStatementRow.Count - 1)
                                        {
                                            string TPI = IPropStatementTD[7].Text;
                                            string TAtty = IPropStatementTD[8].Text;
                                            if (TPI.Contains("0.00") && TAtty.Contains("0.00"))
                                            {
                                                j++;
                                            }
                                        }
                                        try
                                        {
                                            if (IPropStatementTD.Count != 0 && StateBill < 3 && (IPropStatementTD[0].Text.Contains(Convert.ToString(stateyear)) || IPropStatementTD[0].Text.Contains(Convert.ToString(stateyear - 1)) || IPropStatementTD[0].Text.Contains(Convert.ToString(stateyear - 2))))
                                            {
                                                try
                                                {
                                                    string URLLink = IPropStatementTD[0].GetAttribute("onclick");
                                                    string BillLink = "http://aswportal.azurewebsites.net" + gc.Between(URLLink, "window.open('", "', '_blank')");
                                                    srStatement.Add(BillLink);
                                                }
                                                catch { }
                                                StateBill++;
                                            }
                                        }
                                        catch { }
                                    }

                                    if (j == IPropStatementRow.Count)
                                    {
                                        string alert = "Delinquent " + "~" + "Need to Call" + "~" + "For tax amount due, you must call the Collector's Office.";
                                        gc.insert_date(orderNumber, PropertyID, 1101, alert, 1, DateTime.Now);
                                    }
                                    //MUD Tax Reciept Details
                                    driver.FindElement(By.LinkText("RECEIPTS")).Click(); //html/body/div[2]/section/div/ul/li[3]/a
                                    gc.CreatePdf(orderNumber, PropertyID, "Brazoria Tax Reciept", driver, "TX", "Brazoria");
                                    int Bill = 0;
                                    List<string> srReciept = new List<string>();
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

                                            string propDueDetails = propRCPT + "~" + propRCPTDate + "~" + propRPostDate + "~" + propRCheck + "~" + propRPaid1 + "~" + propRMO + "~" + propRPaid2 + "~" + propRCC + "~" + propRCCType + "~" + propRPaid3 + "~" + propROther + "~" + propRAmountPaid + "~" + propRCashPaid + "~" + propRVoid + "~" + propRCode + "~" + propRDate;
                                            gc.insert_date(orderNumber, propID, 1084, propDueDetails, 1, DateTime.Now);
                                        }

                                        if (IPropReceiptTD.Count != 0 && Bill < 3)
                                        {
                                            try
                                            {
                                                string URLLink = IPropReceiptTD[0].GetAttribute("onclick");
                                                string BillLink = "http://aswportal.azurewebsites.net" + gc.Between(URLLink, "window.open('", "', '_blank')");
                                                srReciept.Add(BillLink);
                                            }
                                            catch { }
                                            Bill++;
                                        }
                                    }
                                    int count = 0, stcount = 0;
                                    foreach (string URL in srStatement)
                                    {
                                        driver.Navigate().GoToUrl(URL);
                                        if (stcount == 0)
                                        {
                                            Thread.Sleep(5000);
                                            gc.CreatePdf(orderNumber, PropertyID, "Brazoria Tax Statement Bill" + stcount, driver, "TX", "Brazoria");
                                        }
                                        else
                                        {
                                            Thread.Sleep(3000);
                                            gc.CreatePdf(orderNumber, PropertyID, "Brazoria Tax Statement Bill" + stcount, driver, "TX", "Brazoria");
                                        }
                                        stcount++;
                                    }
                                    foreach (string URL in srReciept)
                                    {
                                        driver.Navigate().GoToUrl(URL);
                                        Thread.Sleep(4000);
                                        gc.CreatePdf(orderNumber, PropertyID, "Brazoria Tax Reciept Bill" + count, driver, "TX", "Brazoria");
                                        count++;
                                    }
                                }
                                catch { }
                                Scenario1++;
                            }


                            if ((msg1 == "M100" || msg1 == "(E1)141") && Scenario2 < 1)
                            {
                                //Utility Tax Service Details
                                string strAccountNo = "", strOwnerName = "", strAddress = "", strServiceAdrress = "", strJuriID = "", strPropertyInfo = "", strTaxAuthority = "", strTaxYear = "", strDelinquentDate = "", strAcerage = "", PayCheck = "", AppraisedTitle = "", AppraisedValue = "", UTax = "", TaxingTitle = "", TaxingValue = "";
                                driver.Navigate().GoToUrl("http://www.utilitytaxservice.com/SrchAcct.aspx");
                                driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWSrchAcctCtl1_AcctNo")).SendKeys(GeographicID.Replace("-", "").Trim());
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
                                db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + "Account Number~Owner Name~Owner Address~Service Address~Tax Year~Property Information~Acreage~Jurisdiction ID~" + AppraisedTitle + "Tax Authority" + "' where Id = '" + 1090 + "'");
                                string UPropertyDetails = strAccountNo + "~" + strOwnerName + "~" + strAddress.Trim() + "~" + strServiceAdrress.Trim() + "~" + strTaxYear + "~" + strPropertyInfo + "~" + strAcerage + "~" + strJuriID + "~" + AppraisedValue + strTaxAuthority;
                                gc.insert_date(orderNumber, strAccountNo, 1090, UPropertyDetails, 1, DateTime.Now);
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
                                        gc.insert_date(orderNumber, strAccountNo, 1093, UTaxLevy, 1, DateTime.Now);
                                    }
                                    if (IUTaxLevyTD.Count != 0 && Levy.Text.Contains("Due for All Years"))
                                    {
                                        string UTaxLevy = strAccountNo + "~" + IUTaxLevyTD[1].Text + "~" + "~" + IUTaxLevyTD[2].Text;
                                        gc.insert_date(orderNumber, strAccountNo, 1093, UTaxLevy, 1, DateTime.Now);
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
                                        gc.insert_date(orderNumber, strAccountNo, 1092, UTaxPost, 1, DateTime.Now);
                                    }
                                }

                                db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + "Account Number~Tax Year~Taxing Unit~Exempt Amount~Taxable Value~Tax Rate~Tax Levy~" + TaxingTitle + "Delinquent Date~Make Checks Payable" + "' where Id = '" + 1091 + "'");
                                string UTaxDetails = strAccountNo + "~" + UTax + "~" + TaxingValue + strDelinquentDate + "~" + PayCheck;
                                gc.insert_date(orderNumber, strAccountNo, 1091, UTaxDetails, 1, DateTime.Now);
                                Scenario2++;
                            }
                            string messages = "";
                            //Alert Message
                            //Jurisdiction Name~Collecting Authority~Message
                            if (msg1 == "M16")
                            {
                                messages = "http://bli-tax.com/records/";
                                string alert = entityname + "~" + "Bob Leared Interest" + "~" + messages;
                                gc.insert_date(orderNumber, PropertyID, 1101, alert, 1, DateTime.Now);
                            }
                            if (msg1 == "CCl")
                            {
                                messages = "Call (979)-265-2541 http://www.ci.clute.tx.us/admin.htm";
                                string alert = entityname + "~" + "Need to Call" + "~" + messages;
                                gc.insert_date(orderNumber, PropertyID, 1101, alert, 1, DateTime.Now);
                            }
                            if (msg1 == "S12")
                            {
                                messages = "Call (281) 482-1198 www.northaustinweb.com/test/FILE304.TXT";
                                string alert = entityname + "~" + "Cathy Bernacki" + "~" + messages;
                                gc.insert_date(orderNumber, PropertyID, 1101, alert, 1, DateTime.Now);
                            }
                        }

                        TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    }
                    catch (Exception ex) { }
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "TX", "Brazoria", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    driver.Quit();
                    GlobalClass.titleparcel = "";
                    gc.mergpdf(orderNumber, "TX", "Brazoria");
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