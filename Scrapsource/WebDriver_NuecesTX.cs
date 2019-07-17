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
    public class WebDriver_NuecesTX
    {
        IWebElement PaidCount;
        IWebDriver driver;
        DBconnection db = new DBconnection();
        List<string> link = new List<string>();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());


        public string FTP_Nueces(string houseno, string sname, string stype, string unitno, string direction, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            GlobalClass.sname = "TX";
            GlobalClass.cname = "Nueces";
            List<string> taxinformation = new List<string>();
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("https://propaccess.trueautomation.com/clientdb/?cid=75");
                    var SelectY = driver.FindElement(By.Id("propertySearchOptions_taxyear"));
                    var selectElementY = new SelectElement(SelectY);
                    selectElementY.SelectByIndex(2);

                    if (searchType == "titleflex")
                    {
                        string titleaddress = houseno + " " + sname + " " + stype + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "TX", "Nueces");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_Nueces"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    //if (searchType == "address")
                    //{

                    //    var Select1 = driver.FindElement(By.Id("propertySearchOptions_searchType"));
                    //    var selectElement11 = new SelectElement(Select1);
                    //    selectElement11.SelectByText("Property Address");
                    //    driver.FindElement(By.Id("propertySearchOptions_streetNumber")).SendKeys(houseno);
                    //    driver.FindElement(By.Id("propertySearchOptions_streetName")).SendKeys(sname);
                    //    gc.CreatePdf_WOP(orderNumber, "address search", driver, "TX", "Nueces");
                    //    driver.FindElement(By.Id("propertySearchOptions_search")).Click();
                    //    Thread.Sleep(2000);
                    //    gc.CreatePdf_WOP(orderNumber, "address search result", driver, "TX", "Nueces");
                    //}
                    //if (searchType == "parcel")
                    //{
                    //    var Select1 = driver.FindElement(By.Id("propertySearchOptions_searchType"));
                    //    var selectElement11 = new SelectElement(Select1);
                    //    selectElement11.SelectByText("Account Number");
                    //    driver.FindElement(By.Id("propertySearchOptions_propertyid")).SendKeys(parcelNumber);
                    //    gc.CreatePdf(orderNumber, parcelNumber, "Parcel search", driver, "TX", "Nueces");
                    //    driver.FindElement(By.Id("propertySearchOptions_search")).Click();
                    //    Thread.Sleep(2000);
                    //    gc.CreatePdf(orderNumber, parcelNumber, "Parcel search result", driver, "TX", "Nueces");
                    //}

                    //if (searchType == "ownername")
                    //{
                    //    driver.FindElement(By.Id("propertySearchOptions_ownerName")).SendKeys(ownername);
                    //    gc.CreatePdf_WOP(orderNumber, "owner search", driver, "TX", "Nueces");
                    //    driver.FindElement(By.Id("propertySearchOptions_search")).Click();
                    //    Thread.Sleep(2000);
                    //    gc.CreatePdf_WOP(orderNumber, "owner search result", driver, "TX", "Nueces");
                    //}
                    //if (searchType == "block")
                    //{
                    //    var Select1 = driver.FindElement(By.Id("propertySearchOptions_searchType"));
                    //    var selectElement11 = new SelectElement(Select1);
                    //    selectElement11.SelectByText("Account Number");
                    //    driver.FindElement(By.Id("propertySearchOptions_geoid")).SendKeys(unitno);
                    //    gc.CreatePdf_WOP(orderNumber, "Account Number Search ", driver, "TX", "Nueces");
                    //    driver.FindElement(By.Id("propertySearchOptions_search")).Click();
                    //    Thread.Sleep(4000);
                    //    gc.CreatePdf_WOP(orderNumber, "Account Number Search  result", driver, "TX", "Nueces");

                    //    //       gc.CreatePdf_WOP(orderNumber, "owner search result", driver, "TX", "Williamson");
                    //}
                    ////Geographic ID~Type~Property Address~Owner Name

                    //int trCount = driver.FindElements(By.XPath("//*[@id='propertySearchResults_resultsTable']/tbody/tr")).Count;
                    //if (trCount > 3)
                    //{
                    //    int maxCheck = 0;
                    //    IWebElement tbmulti = driver.FindElement(By.XPath("//*[@id='propertySearchResults_resultsTable']/tbody"));
                    //    IList<IWebElement> TRmulti5 = tbmulti.FindElements(By.TagName("tr"));
                    //    IList<IWebElement> TDmulti5;
                    //    foreach (IWebElement row in TRmulti5)
                    //    {
                    //        if (maxCheck <= 25)
                    //        {
                    //            TDmulti5 = row.FindElements(By.TagName("td"));
                    //            if (TDmulti5.Count == 10)
                    //            {
                    //                string multi1 = TDmulti5[2].Text + "~" + TDmulti5[3].Text + "~" + TDmulti5[4].Text + "~" + TDmulti5[6].Text;
                    //                gc.insert_date(orderNumber, TDmulti5[1].Text, 813, multi1, 1, DateTime.Now);
                    //            }
                    //            maxCheck++;
                    //        }
                    //    }
                    //    if (TRmulti5.Count > 25)
                    //    {
                    //        HttpContext.Current.Session["multiParcel_Nueces_Multicount"] = "Maximum";
                    //    }
                    //    else
                    //    {
                    //        HttpContext.Current.Session["multiparcel_Nueces"] = "Yes";
                    //    }

                    //    driver.Quit();
                    //    return "MultiParcel";
                    //}
                    //else
                    //{

                    //    driver.FindElement(By.XPath("//*[@id='propertySearchResults_resultsTable']/tbody/tr[2]/td[9]/a")).Click();
                    //    Thread.Sleep(2000);


                    //}

                    //try
                    //{
                    //    IWebElement INodata = driver.FindElement(By.Id("propertySearchResults_pageHeading"));
                    //    if(INodata.Text.Contains("None found"))
                    //    {
                    //        HttpContext.Current.Session["Nodata_Nueces"] = "Yes";
                    //        driver.Quit();
                    //        return "No Data Found";
                    //    }
                    //}
                    //catch { }

                    try
                    {
                        driver.FindElement(By.Id("propertySearchOptions_advanced")).SendKeys(Keys.Enter);
                    }
                    catch { }
                    if (searchType == "titleflex")
                    {
                        string titleaddress = houseno + " " + sname + " " + stype + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "TX", "Nueces");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_Nueces"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        driver.FindElement(By.Id("propertySearchOptions_streetNumber")).SendKeys(houseno);
                        driver.FindElement(By.Id("propertySearchOptions_streetName")).SendKeys(sname);
                        gc.CreatePdf_WOP(orderNumber, "address search", driver, "TX", "Nueces");
                        try
                        {
                            driver.FindElement(By.Id("propertySearchOptions_search")).Click();
                        }
                        catch { }
                        try
                        {
                            driver.FindElement(By.Id("propertySearchOptions_searchAdv")).Click();
                        }
                        catch { }
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "address search result", driver, "TX", "Nueces");
                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.Id("propertySearchOptions_propertyid")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search", driver, "TX", "Nueces");
                        try
                        {
                            driver.FindElement(By.Id("propertySearchOptions_search")).Click();
                        }
                        catch { }
                        try
                        {
                            driver.FindElement(By.Id("propertySearchOptions_searchAdv")).Click();
                        }
                        catch { }
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search result", driver, "TX", "Nueces");
                    }

                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("propertySearchOptions_ownerName")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "owner search", driver, "TX", "Nueces");
                        try
                        {
                            driver.FindElement(By.Id("propertySearchOptions_search")).Click();
                        }
                        catch { }
                        try
                        {
                            driver.FindElement(By.Id("propertySearchOptions_searchAdv")).Click();
                        }
                        catch { }
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "owner search result", driver, "TX", "Nueces");
                    }
                    if (searchType == "block")
                    {
                        driver.FindElement(By.Id("propertySearchOptions_geoid")).SendKeys(unitno);
                        gc.CreatePdf_WOP(orderNumber, "Account Number Search ", driver, "TX", "Nueces");
                        try
                        {
                            driver.FindElement(By.Id("propertySearchOptions_search")).Click();
                        }
                        catch { }
                        try
                        {
                            driver.FindElement(By.Id("propertySearchOptions_searchAdv")).Click();
                        }
                        catch { }
                        Thread.Sleep(4000);
                        gc.CreatePdf_WOP(orderNumber, "Account Number Search  result", driver, "TX", "Nueces");

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
                                if (TDmulti5.Count == 10)
                                {
                                    string multi1 = TDmulti5[2].Text + "~" + TDmulti5[3].Text + "~" + TDmulti5[4].Text + "~" + TDmulti5[6].Text;
                                    gc.insert_date(orderNumber, TDmulti5[1].Text, 813, multi1, 1, DateTime.Now);
                                }
                                maxCheck++;
                            }
                        }
                        if (TRmulti5.Count > 25)
                        {
                            HttpContext.Current.Session["multiParcel_Nueces_Multicount"] = "Maximum";
                        }
                        else
                        {
                            HttpContext.Current.Session["multiparcel_Nueces"] = "Yes";
                        }

                        driver.Quit();
                        return "MultiParcel";
                    }
                    else
                    {
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='propertySearchResults_resultsTable']/tbody/tr[2]/td[9]/a")).Click();
                        }
                        catch { }

                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='propertySearchResults_resultsTable']/tbody/tr[2]/td[10]/a")).Click();
                        }
                        catch { }
                        Thread.Sleep(2000);

                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.Id("propertySearchResults_pageHeading"));
                        if (INodata.Text.Contains("None found"))
                        {
                            HttpContext.Current.Session["Nodata_Nueces"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    //property details
                    string ownership = "", PropertyID = "", GeographicID = "", Name = "", MailingAddress = "", OwnerID = "", LegalDescription = "", Neighborhood = "", Type = "", Address = "", NeighborhoodCD = "", Exemptions = "", MapID = "", YearBuilt = "", Acres = "";

                    //Property ID~Geographic ID~Owner Name~Mailing Address~Owner ID~Type~Legal Description~Address~Neighborhood~Neighborhood CD~Map ID~Exemptions~Year Built~Acres                    
                    string fulltext = driver.FindElement(By.XPath("//*[@id='propertyDetails']/table/tbody")).Text.Replace("\r\n", " ");
                    PropertyID = gc.Between(fulltext, "Account Property ID:", " Legal Description: ");

                    GeographicID = gc.Between(fulltext, "Geographic ID: ", "Agent Code: ");
                    Name = gc.Between(fulltext, "Owner Name:", "Owner ID:");
                    MailingAddress = gc.Between(fulltext, "Mailing Address:", " % Ownership:");
                    OwnerID = gc.Between(fulltext, "Owner ID:", "Mailing Address:");
                    LegalDescription = gc.Between(fulltext, "Legal Description:", "Geographic ID:");
                    Neighborhood = gc.Between(fulltext, "Neighborhood:", "Map ID:");
                    Type = gc.Between(fulltext, "Type:", "Property Use Code:");
                    Address = gc.Between(fulltext, "Location Address:", "Mapsco:");
                    NeighborhoodCD = gc.Between(fulltext, "Neighborhood CD:", "Owner Name:");
                    Exemptions = GlobalClass.After(fulltext, "Exemptions:");
                    MapID = gc.Between(fulltext, "Map ID:", "Neighborhood CD:");
                    ownership = gc.Between(fulltext, "% Ownership:", " Exemptions:");
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
                    catch
                    { }
                    string property_details = PropertyID + "~" + GeographicID + "~" + Name + "~" + MailingAddress + "~" + OwnerID + "~" + ownership + "~" + Type + "~" + LegalDescription + "~" + Address + "~" + Neighborhood + "~" + NeighborhoodCD + "~" + MapID + "~" + Exemptions + "~" + YearBuilt + "~" + Acres;
                    gc.insert_date(orderNumber, PropertyID, 806, property_details, 1, DateTime.Now);

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
                            gc.insert_date(orderNumber, PropertyID, 809, ValueDetails, 1, DateTime.Now);

                        }
                    }

                    //Taxing Jurisdiction Details Table:
                    //Owner~% Ownership~Total Value~Entity~Description~Tax Rate~Appraised Value~Taxable Value~Estimated Tax
                    driver.FindElement(By.Id("taxingJurisdiction")).Click();
                    Thread.Sleep(2000);
                    string owner = "", Ownership = "", TotalValue = "";
                    string ValueDetails1 = "", msg = "";
                    string fulltext1 = driver.FindElement(By.XPath("//*[@id='taxingJurisdictionDetails']/table[1]/tbody")).Text.Replace("\r\n", " ");

                    owner = gc.Between(fulltext1, "Owner:", "% Ownership:");
                    Ownership = gc.Between(fulltext1, "% Ownership:", "Total Value:");
                    TotalValue = GlobalClass.After(fulltext1, "Total Value:");
                    ValueDetails1 = owner + "~" + Ownership + "~" + TotalValue + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                    gc.insert_date(orderNumber, PropertyID, 808, ValueDetails1, 1, DateTime.Now);
                    string taxceiling = "";
                    IWebElement multitableElement11 = driver.FindElement(By.XPath("//*[@id='taxingJurisdictionDetails']/table[2]/tbody"));
                    IList<IWebElement> multitableRow11 = multitableElement11.FindElements(By.TagName("tr"));
                    IList<IWebElement> multirowTD11;
                    foreach (IWebElement row in multitableRow11)
                    {
                        //CITY OF ALAMO,CITY OF ALTON,CITY OF DONNA,City of Edcouch,CITY OF EDINBURG,CITY OF EDINBURG,City of Elsa,City of Granjeno,City of Nueces,City of La Joya,CITY OF LA VILLA,CITY OF MCALLEN,CITY OF MERCEDES,CITY OF MISSION,City of Palmview,City of Penitas,CITY OF PHARR,City of Progreso,CITY OF SAN JUAN,City of Sullivan ,CITY OF WESLACO
                        multirowTD11 = row.FindElements(By.TagName("td"));
                        if (multirowTD11.Count != 0)
                        {
                            //if (!multirowTD11[7].Text.Trim().Contains(""))
                            //{
                            //    taxceiling = multirowTD11[7].Text.Trim();
                            //}
                            string ValueDetails = " " + "~" + " " + "~" + " " + "~" + multirowTD11[0].Text.Trim() + "~" + multirowTD11[1].Text.Trim() + "~" + multirowTD11[2].Text.Trim() + "~" + multirowTD11[3].Text.Trim() + "~" + multirowTD11[4].Text.Trim() + "~" + multirowTD11[5].Text.Trim() + "~" + multirowTD11[7].Text.Trim();
                            gc.insert_date(orderNumber, PropertyID, 808, ValueDetails, 1, DateTime.Now);
                            //taxceiling = "";
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
                            gc.insert_date(orderNumber, PropertyID, 807, rollDetails, 1, DateTime.Now);
                        }
                    }

                    gc.CreatePdf(orderNumber, PropertyID, "property details", driver, "TX", "Nueces");

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("https://actweb.acttax.com/act_webdev/nueces/index.jsp");
                    Thread.Sleep(3000);
                    driver.FindElement(By.Id("sc5")).Click();
                    Thread.Sleep(2000);
                    driver.FindElement(By.Id("criteria")).SendKeys(PropertyID);
                    gc.CreatePdf(orderNumber, PropertyID, "tax search", driver, "TX", "Nueces");
                    driver.FindElement(By.Name("submit")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, PropertyID, "tax search result", driver, "TX", "Nueces");

                    driver.FindElement(By.XPath("//*[@id='data-block']/table/tbody/tr/td/table/tbody/tr/td[2]/h3/a")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, PropertyID, "tax search result details", driver, "TX", "Nueces");
                    //Jurisdiction Information Details Table: 
                    string year = "", account = "", ExemptionsJ = "";

                    driver.FindElement(By.LinkText("Taxes Due Detail by Year and Jurisdiction")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, PropertyID, "Taxes Due Detail by Year and Jurisdiction", driver, "TX", "Nueces");
                    //Taxes Due Detail by Year Details Table: 
                    //Account No~Appr. Dist. No~Active Lawsuits~Year~Base Tax Due~Penalty, Interest, and ACC* Due~Total Due~Penalty, Interest, and ACC* Due1~Total Due1~Penalty, Interest, and ACC*Due2~Total Due2
                    string accountno = "", distno = "", ActiveLawsuits = "";


                    IWebElement multitableElement31 = driver.FindElement(By.XPath("/html/body/div[3]/center/table/tbody/tr/td/table/tbody"));
                    IList<IWebElement> multitableRow31 = multitableElement31.FindElements(By.TagName("tr"));
                    IList<IWebElement> multirowTD31;
                    foreach (IWebElement row in multitableRow31)
                    {
                        multirowTD31 = row.FindElements(By.TagName("td"));
                        if (multirowTD31.Count == 8 && !row.Text.Contains("Base Tax Due"))
                        {
                            string TaxesDue = multirowTD31[0].Text.Trim() + "~" + multirowTD31[1].Text.Trim() + "~" + multirowTD31[2].Text.Trim() + "~" + multirowTD31[3].Text.Trim() + "~" + multirowTD31[4].Text.Trim() + "~" + multirowTD31[5].Text.Trim() + "~" + multirowTD31[6].Text.Trim() + "~" + multirowTD31[7].Text.Trim();
                            gc.insert_date(orderNumber, PropertyID, 810, TaxesDue, 1, DateTime.Now);
                        }
                        if (multirowTD31.Count == 1)
                        {
                            string TaxesDue = multirowTD31[0].Text.Trim() + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                            gc.insert_date(orderNumber, PropertyID, 810, TaxesDue, 1, DateTime.Now);

                        }
                        if (multirowTD31.Count == 4)
                        {
                            string TaxesDue = "" + "~" + "" + "~" + multirowTD31[1].Text.Trim() + "~" + "" + "~" + multirowTD31[2].Text.Trim() + "~" + "" + "~" + multirowTD31[3].Text.Trim() + "~" + "";
                            gc.insert_date(orderNumber, PropertyID, 810, TaxesDue, 1, DateTime.Now);

                        }
                    }

                    driver.FindElement(By.LinkText("Return to the Previous Page")).Click();
                    Thread.Sleep(2000);
                    //Tax Payment Details Table: 

                    //Thread.Sleep(2000);

                    //Account Number~Appraisal District Number~Owner Information~Property Site Address~Legal Description~Current Tax Levy~Current Amount Due~Prior Year Amount Due~Total Amount Due~Last Payment Amount for Current Year Taxes~Last Payer for Current Year Taxes~Last Payment Date for Current Year Taxes~Active Lawsuits~Gross Value~Land Value~Improvement Value~Capped Value~Agricultural Value~Exemptions~Tax Authority

                    string year1 = driver.FindElement(By.XPath("/html/body/div[3]/table[1]/tbody/tr/td/h5")).Text;
                    string yearnew = gc.Between(year1, "to tax information for", ". All amounts due include penalty");
                    string marketvalue = "", accountNo = "", AppraisalDistrict = "", OwnerName = "", OwnerAddress = "", PropertyAddress = "", legal = "", CurrentTax = "", CurrentAmount = "", PriorYearAmount = "", TotalAmount = "", LastPaymentAmount = "", LastPayer = "", LastPaymentDate = "", ActiveLawsuitsTax = "", GrossValue = "", LandValue = "", ImprovementValue = "", CappedValue = "", AgriculturalValue = "", ExemptionsTax = "";
                    string fullTaxeBill1 = driver.FindElement(By.XPath("/html/body/div[3]/table[2]/tbody/tr/td[1]")).Text.Replace("\r\n", " ");
                    accountno = gc.Between(fullTaxeBill1, "Account Number:", "Address: ");
                    OwnerName = gc.Between(fullTaxeBill1, "Address:", " Property Site Address:");
                    PropertyAddress = gc.Between(fullTaxeBill1, "Property Site Address:", " Legal Description:");
                    legal = gc.Between(fullTaxeBill1, "Legal Description:", " Current Tax Levy:");
                    CurrentTax = gc.Between(fullTaxeBill1, " Current Tax Levy:", "Current Amount Due:");
                    CurrentAmount = gc.Between(fullTaxeBill1, "Current Amount Due:", "Prior Year Amount Due:");
                    PriorYearAmount = gc.Between(fullTaxeBill1, "Prior Year Amount Due:", "Total Amount Due:");
                    TotalAmount = gc.Between(fullTaxeBill1, "Total Amount Due:", "Last Payment Amount for Current Year Taxes:");
                    LastPaymentAmount = gc.Between(fullTaxeBill1, "Last Payment Amount for Current Year Taxes:", "Last Payer for Current Year Taxes:");
                    LastPayer = gc.Between(fullTaxeBill1, "Last Payer for Current Year Taxes:", "Last Payment Date for Current Year Taxes:");
                    LastPaymentDate = gc.Between(fullTaxeBill1, "Last Payment Date for Current Year Taxes:", "Active Lawsuits:");
                    string fullTaxeBill2 = driver.FindElement(By.XPath("/html/body/div[3]/table[2]/tbody/tr/td[2]")).Text.Replace("\r\n", " ");
                    ActiveLawsuitsTax = GlobalClass.After(fullTaxeBill1, "Active Lawsuits:");
                    GrossValue = gc.Between(fullTaxeBill2, "Pending Credit Card or E–Check Payments:", "Market Value:");
                    LandValue = gc.Between(fullTaxeBill2, "Land Value:", "Improvement Value:");
                    ImprovementValue = gc.Between(fullTaxeBill2, "Improvement Value:", "Capped Value:");
                    CappedValue = gc.Between(fullTaxeBill2, "Capped Value:", "Agricultural Value:");
                    AgriculturalValue = gc.Between(fullTaxeBill2, "Agricultural Value:", "Exemptions:");
                    ExemptionsTax = gc.Between(fullTaxeBill2, "Exemptions:", "Taxes Due Detail by Year and Jurisdiction");
                    marketvalue = gc.Between(fullTaxeBill2, "Market Value:", "Land Value:");
                    string taxbill = yearnew + "~" + accountno + "~" + OwnerName + "~" + PropertyAddress + "~" + legal + "~" + CurrentTax + "~" + CurrentAmount + "~" + PriorYearAmount + "~" + TotalAmount + "~" + LastPaymentAmount + "~" + LastPayer + "~" + LastPaymentDate + "~" + ActiveLawsuitsTax + "~" + GrossValue + "~" + marketvalue + "~" + LandValue + "~" + ImprovementValue + "~" + CappedValue + "~" + AgriculturalValue + "~" + ExemptionsTax + "~" + "P.O. Box 2810, Corpus Christi, Texas 78403-2810";
                    gc.insert_date(orderNumber, PropertyID, 811, taxbill, 1, DateTime.Now);
                    try
                    {
                        IWebElement Itaxstmt1 = driver.FindElement(By.LinkText("Print a Current Tax Statement"));
                        Thread.Sleep(2000);
                        string stmt11 = Itaxstmt1.GetAttribute("href");
                        driver.Navigate().GoToUrl(stmt11);
                        Thread.Sleep(15000);
                        IWebElement Itaxstmt = driver.FindElement(By.XPath("/html/body/div[3]/div/h3/a"));
                        string stmt1 = Itaxstmt.GetAttribute("href");
                        gc.downloadfile(stmt1, orderNumber, PropertyID, "Tax statement", "TX", "Nueces");
                        driver.Navigate().Back();
                        Thread.Sleep(2000);

                        string FilePath = gc.filePath(orderNumber, PropertyID) + "Tax statement.pdf";
                        PdfReader reader;
                        string pdfData;
                        reader = new PdfReader(FilePath);
                        String textFromPage = PdfTextExtractor.GetTextFromPage(reader, 1);
                        System.Diagnostics.Debug.WriteLine("" + textFromPage);
                        string newrow = "";
                        string PaymentsReceived = "";
                        string pdftext = textFromPage;
                        string tableassess = "";
                        string basetax = "";
                        try
                        {
                            tableassess = gc.Between(pdftext, "Unit Assessed Value Value Amount Value Rate Tax", "Prior Payments Received").Trim();
                        }
                        catch
                        {
                            tableassess = gc.Between(pdftext, "Unit Assessed Value Value Amount Value Rate Tax", "Base Tax").Trim();

                        }
                        try
                        {
                            PaymentsReceived = gc.Between(pdftext, "Prior Payments Received", "Base Tax").Trim();
                            if (PaymentsReceived.Contains("Unit Assessed Value Value Amount Value Rate Tax"))
                            {
                                PaymentsReceived = "";
                            }
                        }
                        catch
                        {
                            PaymentsReceived = "";
                        }
                        try
                        {
                            basetax = gc.Between(pdftext, "Base Tax", "Exemptions:").Trim().Replace("\n", "").Replace(" ", "~");
                        }
                        catch
                        {
                            basetax = gc.Between(pdftext, "Base Tax", "School Information:").Trim().Replace("\n", "").Replace(" ", "~");
                        }
                        string[] tableArray = tableassess.Split('\n');
                        string name = "";
                        int count1 = tableArray.Length;
                        for (int i = 0; i < count1; i++)
                        {

                            string a1 = tableArray[i].Replace(" ", "~");
                            string[] rowarray = a1.Split('~');
                            int tdcount = rowarray.Length;
                            if (tdcount == 10)
                            {
                                int k = 0;
                                newrow = yearnew + "~" + name + "~" + rowarray[k + 1] + "~" + rowarray[k + 2] + "~" + rowarray[k + 3] + "~" + rowarray[k + 4] + "~" + rowarray[k + 5] + "~" + rowarray[k + 6] + "~" + rowarray[k + 7] + "~" + rowarray[k + 8] + "~" + rowarray[k + 9];
                                gc.insert_date(orderNumber, PropertyID, 812, newrow, 1, DateTime.Now);
                            }
                            else
                            {
                                name = tableArray[i];
                            }
                        }
                        if (PaymentsReceived != "")
                        {
                            newrow = "" + "~" + "Prior Payments Received" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + PaymentsReceived + "~" + "" + "~" + "" + "~" + "";
                            gc.insert_date(orderNumber, PropertyID, 812, newrow, 1, DateTime.Now);
                        }
                        newrow = "" + "~" + "Base Tax" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + basetax;
                        gc.insert_date(orderNumber, PropertyID, 812, newrow, 1, DateTime.Now);
                    }
                    catch { }
                    //Taxing Unit~Assessed Value~Cap Value~Exemption Amount~Taxable Value~Tax Rate~Tax Amount~3% Oct~2% Nov~1% Dec 
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "TX", "Nueces", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    driver.Quit();
                    GlobalClass.titleparcel = "";
                    gc.mergpdf(orderNumber, "TX", "Nueces");
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