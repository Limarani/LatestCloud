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
    public class WebDriver_ComalTX
    {
        IWebElement PaidCount;
        IWebDriver driver;
        DBconnection db = new DBconnection();
        List<string> link = new List<string>();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_Comal(string houseno, string sname, string stype, string unitno, string direction, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            string ownership = "", PropertyID = "", GeographicID = "", Name = "", MailingAddress = "", OwnerID = "", LegalDescription = "", Neighborhood = "", Type = "", Address = "", NeighborhoodCD = "", Exemptions = "", MapID = "", YearBuilt = "", Acres = "";

            List<string> myList = new List<string>();
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            List<string> taxinformation = new List<string>();
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            // driver = new PhantomJSDriver();
            // driver = new ChromeDriver();
            using (driver = new PhantomJSDriver()) //PhantomJSDriver
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("https://propaccess.trueautomation.com/clientdb/?cid=56");


                    if (searchType == "titleflex")
                    {
                        string titleaddress = houseno + " " + sname + " " + stype + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "TX", "Comal");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_ComalTX"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {

                        var Select1 = driver.FindElement(By.Id("propertySearchOptions_taxyear"));
                        var selectElement11 = new SelectElement(Select1);
                        selectElement11.SelectByIndex(1);
                        Thread.Sleep(2000);
                        driver.FindElement(By.Id("propertySearchOptions_advanced")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.Id("propertySearchOptions_streetNumber")).SendKeys(houseno);
                        driver.FindElement(By.Id("propertySearchOptions_streetName")).SendKeys(sname);
                        gc.CreatePdf_WOP(orderNumber, "address search", driver, "TX", "Comal");
                        driver.FindElement(By.Id("propertySearchOptions_searchAdv")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "address search result", driver, "TX", "Comal");
                    }
                    if (searchType == "parcel")
                    {
                        var Select1 = driver.FindElement(By.Id("propertySearchOptions_taxyear"));
                        var selectElement11 = new SelectElement(Select1);
                        selectElement11.SelectByIndex(1);
                        Thread.Sleep(2000);
                        driver.FindElement(By.Id("propertySearchOptions_advanced")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.Id("propertySearchOptions_propertyid")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search", driver, "TX", "Comal");
                        driver.FindElement(By.Id("propertySearchOptions_searchAdv")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search result", driver, "TX", "Comal");
                    }

                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("propertySearchOptions_advanced")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.Id("propertySearchOptions_ownerName")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "owner search", driver, "TX", "Comal");
                        driver.FindElement(By.Id("propertySearchOptions_searchAdv")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "owner search result", driver, "TX", "Comal");
                    }
                    if (searchType == "block")
                    {
                        var Select1 = driver.FindElement(By.Id("propertySearchOptions_searchType"));
                        var selectElement11 = new SelectElement(Select1);
                        selectElement11.SelectByText("Account Number");
                        driver.FindElement(By.Id("propertySearchOptions_geoid")).SendKeys(unitno);
                        gc.CreatePdf_WOP(orderNumber, "Account Number Search ", driver, "TX", "Comal");
                        driver.FindElement(By.Id("propertySearchOptions_search")).Click();
                        Thread.Sleep(4000);
                        gc.CreatePdf_WOP(orderNumber, "Account Number Search  result", driver, "TX", "Comal");

                        //       gc.CreatePdf_WOP(orderNumber, "owner search result", driver, "TX", "Williamson");
                    }
                    //Geographic ID~Type~Property Address~Owner Name
                    try
                    {

                        int trCount = driver.FindElements(By.XPath("//*[@id='propertySearchResults_resultsTable']/tbody/tr")).Count;
                        if (trCount > 3)
                        {//*[@id="propertySearchResults_resultsTable"]/tbody
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
                                        gc.insert_date(orderNumber, TDmulti5[1].Text, 866, multi1, 1, DateTime.Now);
                                    }
                                    maxCheck++;
                                }
                            }
                            if (TRmulti5.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Comal_Multicount"] = "Maximum";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiparcel_Comal"] = "Yes";
                            }

                            driver.Quit();
                            return "MultiParcel";
                        }
                        else
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
                    }
                    catch { }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.Id("propertySearchResults_pageHeading"));
                        if(INodata.Text.Contains("None found"))
                        {
                            HttpContext.Current.Session["Nodata_ComalTX"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    //property details
                    //    string ownership = "", PropertyID = "", GeographicID = "", Name = "", MailingAddress = "", OwnerID = "", LegalDescription = "", Neighborhood = "", Type = "", Address = "", NeighborhoodCD = "", Exemptions = "", MapID = "", YearBuilt = "", Acres = "";

                    string fulltext = driver.FindElement(By.XPath("//*[@id='propertyDetails']/table/tbody")).Text.Replace("\r\n", " ");
                    PropertyID = gc.Between(fulltext, "Account Property ID:", " Legal Description: ");
                    if (!fulltext.Contains("Zoning:"))
                    {
                        GeographicID = gc.Between(fulltext, "Geographic ID: ", "Agent Code: ").Trim();
                    }
                    else
                    {
                        GeographicID = gc.Between(fulltext, "Geographic ID: ", "Zoning:").Trim();
                    }
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
                    //Property ID~Geographic ID~Owner Name~Mailing Address~Owner ID~Type~Legal Description~Address~Neighborhood~Neighborhood CD~Map ID~Exemptions~Year Built~Acres                    

                    string property_details = PropertyID + "~" + GeographicID + "~" + Name + "~" + MailingAddress + "~" + OwnerID + "~" + ownership + "~" + Type + "~" + LegalDescription + "~" + Address + "~" + Neighborhood + "~" + NeighborhoodCD + "~" + MapID + "~" + Exemptions + "~" + YearBuilt + "~" + Acres;
                    gc.insert_date(orderNumber, PropertyID, 860, property_details, 1, DateTime.Now);

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
                            gc.insert_date(orderNumber, PropertyID, 861, ValueDetails, 1, DateTime.Now);

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
                    gc.insert_date(orderNumber, PropertyID, 862, ValueDetails1, 1, DateTime.Now);
                    string taxceiling = "";
                    IWebElement multitableElement11 = driver.FindElement(By.XPath("//*[@id='taxingJurisdictionDetails']/table[2]/tbody"));
                    IList<IWebElement> multitableRow11 = multitableElement11.FindElements(By.TagName("tr"));
                    IList<IWebElement> multirowTD11;
                    foreach (IWebElement row in multitableRow11)
                    {
                        multirowTD11 = row.FindElements(By.TagName("td"));
                        if (multirowTD11.Count != 0)
                        {
                            if (multirowTD11[2].Text.Trim() != "0.000000" && multirowTD11[0].Text.Trim() != "")
                            {
                                myList.Add(multirowTD11[1].Text);
                            }
                            string ValueDetails = " " + "~" + " " + "~" + " " + "~" + multirowTD11[0].Text.Trim() + "~" + multirowTD11[1].Text.Trim() + "~" + multirowTD11[2].Text.Trim() + "~" + multirowTD11[3].Text.Trim() + "~" + multirowTD11[4].Text.Trim() + "~" + multirowTD11[5].Text.Trim();
                            gc.insert_date(orderNumber, PropertyID, 862, ValueDetails, 1, DateTime.Now);

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
                            gc.insert_date(orderNumber, PropertyID, 863, rollDetails, 1, DateTime.Now);
                        }
                    }

                    //Tax Due (Payoff Calculation) Details Table:


                    driver.FindElement(By.Id("taxDue")).Click();
                    Thread.Sleep(2000);
                    string msgdeli = "", taxdue = "";

                    int count = driver.FindElements(By.XPath("//*[@id='taxDueDetails_dataSection']/table/tbody/tr")).Count();
                    taxdue = driver.FindElement(By.XPath("//*[@id='taxDueDetails_dataSection']/table/tbody/tr[" + count + "]/td[7]")).Text;

                    if (!taxdue.Contains("$0.00"))
                    {
                        IWebElement dt = driver.FindElement(By.Id("taxDueDetails_recalculateDate"));
                        string date = dt.Text;

                        DateTime G_Date = Convert.ToDateTime(date);
                        string dateChecking = DateTime.Now.ToString("MM") + "/15/" + DateTime.Now.ToString("yyyy");

                        if (G_Date < Convert.ToDateTime(dateChecking))
                        {
                            //end of the month
                            date = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")))).ToString("MM/dd/yyyy");

                        }

                        else if (G_Date > Convert.ToDateTime(dateChecking))
                        {
                            // nextEndOfMonth 
                            if ((Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))) < 12))
                            {
                                date = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM")) + 1), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")) + 1)).ToString("MM/dd/yyyy");
                            }
                            else
                            {
                                int nxtYr = Convert.ToInt16(DateTime.Now.ToString("yyyy")) + 1;
                                date = new DateTime(nxtYr, 1, DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), 1)).ToString("MM/dd/yyyy");

                            }
                        }

                        Thread.Sleep(2000);
                        // dt.Clear();

                        driver.FindElement(By.Id("taxDueDetails_date")).SendKeys(date);

                        driver.FindElement(By.Id("taxDueDetails_recalculate")).Click();
                        Thread.Sleep(4000);
                    }
                    string goodthroughdate = driver.FindElement(By.Id("taxDueDetails_recalculateDate")).Text;
                    string payoff1 = goodthroughdate + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                    gc.insert_date(orderNumber, PropertyID, 864, payoff1, 1, DateTime.Now);
                    string yearnow = driver.FindElement(By.XPath("//*[@id='taxDueDetails_dataSection']/table/tbody/tr[2]/td[1]")).Text;

                    //Amount Due if Paid on~Year~Taxing Jurisdiction~Taxable Value~Base Tax~Base Taxes Paid~Base Tax Due~Discount / Penalty & Interest~Attorney Fees~Amount Due
                    IWebElement multitableElement5 = driver.FindElement(By.XPath("//*[@id='taxDueDetails_dataSection']/table/tbody"));
                    IList<IWebElement> multitableRow5 = multitableElement5.FindElements(By.TagName("tr"));
                    IList<IWebElement> multirowTD5;
                    foreach (IWebElement row in multitableRow5)
                    {
                        multirowTD5 = row.FindElements(By.TagName("td"));
                        if (multirowTD5.Count != 0)
                        {
                            if (multirowTD5[0].Text == yearnow)
                            {
                                if (myList.Any(str => str.Contains(multirowTD5[1].Text)))
                                {
                                    myList.RemoveAt(myList.IndexOf(multirowTD5[1].Text));

                                }
                            }
                            int countlist = myList.Count();
                            string payoff11 = "" + "~" + multirowTD5[0].Text.Trim() + "~" + multirowTD5[1].Text.Trim() + "~" + multirowTD5[2].Text.Trim() + "~" + multirowTD5[3].Text.Trim() + "~" + multirowTD5[4].Text.Trim() + "~" + multirowTD5[5].Text.Trim() + "~" + multirowTD5[6].Text.Trim() + "~" + multirowTD5[7].Text.Trim() + "~" + multirowTD5[8].Text.Trim();
                            gc.insert_date(orderNumber, PropertyID, 864, payoff11, 1, DateTime.Now);
                        }
                    }
                    string entity = "";
                    if (myList.Count() > 0)
                    {
                        for (int i = 0; i < myList.Count(); i++)
                        {
                            if (i == 0)
                            {
                                entity = myList[i];
                            }
                            else
                            {
                                entity = entity + "&" + myList[i];
                            }
                        }
                        string msged = entity + "~" + "Need to call Tax Office";
                        gc.insert_date(orderNumber, PropertyID, 894, msged, 1, DateTime.Now);
                    }

                    gc.CreatePdf(orderNumber, PropertyID, "property details", driver, "TX", "Comal");

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("https://taxes.mycomalcounty.net/taxes.html#/WildfireSearch");
                    Thread.Sleep(3000);

                    driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[3]/button[2]")).Click();
                    Thread.Sleep(2000);
                    driver.FindElement(By.Id("searchBox")).SendKeys(GeographicID);
                    gc.CreatePdf(orderNumber, PropertyID, "tax search", driver, "TX", "Comal");
                    driver.FindElement(By.XPath("//*[@id='searchForm']/div[1]/div/span/button")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, PropertyID, "tax search result1", driver, "TX", "Comal");

                    string mul = "";
                    try {
                    }
                    catch { }
                    //*[@id="avalon"]/div/div[2]/div[1]/div[3]
                    try
                    {
                        mul = driver.FindElement(By.XPath("//*[@id='avalon']/div/div[2]/div[1]/div[3]")).Text;
                    }
                    catch { }

                    try
                    {
                        if(mul.Trim() == "")
                        {
                            mul = driver.FindElement(By.XPath("//*[@id='avalon']/div/div[3]/div[1]/div[3]")).Text;
                        }
                    }
                    catch { }


                    mul = GlobalClass.Before(mul, " record");
                    int trcount = Int32.Parse(mul);
                    IWebElement multitableElement6 = null;
                    try
                    {
                        multitableElement6 = driver.FindElement(By.XPath("//*[@id='avalon']/div/div[3]/div[2]/table/tbody"));
                    }
                    catch { }
                    try
                    {
                        if (multitableElement6 == null)
                        {
                            multitableElement6 = driver.FindElement(By.XPath("//*[@id='avalon']/div/div[4]/div[2]/table/tbody"));
                        }
                    }
                    catch { }
                    IList<IWebElement> multitableRow6 = multitableElement6.FindElements(By.TagName("tr"));
                    IList<IWebElement> multirowTD6;
                    foreach (IWebElement row in multitableRow6)
                    {
                        multirowTD6 = row.FindElements(By.TagName("td"));
                        if (multirowTD6.Count != 0)
                        {
                            string tax = multirowTD6[0].Text.Trim() + "~" + multirowTD6[1].Text.Trim() + "~" + multirowTD6[2].Text.Trim() + "~" + multirowTD6[3].Text.Trim() + "~" + multirowTD6[4].Text.Trim();
                            gc.insert_date(orderNumber, PropertyID, 865, tax, 1, DateTime.Now);
                        }
                    }
                    try
                    {
                        if (trcount > 20)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//*[@id='avalon']/div/div[2]/div[2]/div[2]/ul/li[4]/a")).Click();
                            }
                            catch { }
                            try
                            {
                                driver.FindElement(By.XPath("//*[@id='avalon']/div/div[3]/div[2]/div[2]/ul/li[4]/a")).Click();
                            }
                            catch { }
                           
                            Thread.Sleep(4000);
                            gc.CreatePdf(orderNumber, PropertyID, "tax search result2", driver, "TX", "Comal");
                            IWebElement multitableElement61 = null;
                            try
                            {
                                multitableElement61 = driver.FindElement(By.XPath("//*[@id='avalon']/div/div[3]/div[2]/table/tbody"));
                            }
                            catch { }
                            try
                            {
                                if (multitableElement61 == null)
                                {
                                    multitableElement61 = driver.FindElement(By.XPath("//*[@id='avalon']/div/div[4]/div[2]/table/tbody"));
                                }
                            }
                            catch { }
                            IList<IWebElement> multitableRow61 = multitableElement61.FindElements(By.TagName("tr"));
                            IList<IWebElement> multirowTD61;
                            foreach (IWebElement row in multitableRow61)
                            {
                                multirowTD61 = row.FindElements(By.TagName("td"));
                                if (multirowTD61.Count != 0)
                                {
                                    string tax = multirowTD61[0].Text.Trim() + "~" + multirowTD61[1].Text.Trim() + "~" + multirowTD61[2].Text.Trim() + "~" + multirowTD61[3].Text.Trim() + "~" + multirowTD61[4].Text.Trim();
                                    gc.insert_date(orderNumber, PropertyID, 865, tax, 1, DateTime.Now);
                                }
                            }

                        }
                    }
                    catch { }
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='avalon']/div/div[2]/div[2]/div[2]/ul/li[1]/a")).Click();
                        Thread.Sleep(4000);
                    }
                    catch { }
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='avalon']/div/div[3]/div[2]/div[2]/ul/li[1]/a")).Click();
                        Thread.Sleep(4000);
                    }
                    catch { }
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='avalon']/div/div[3]/div[2]/table/tbody/tr[1]/td[7]/button")).Click();
                        Thread.Sleep(4000);
                    }
                    catch { }
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='avalon']/div/div[4]/div[2]/table/tbody/tr[1]/td[7]/button")).Click();
                        Thread.Sleep(4000);
                    }
                    catch { }
                    gc.CreatePdf(orderNumber, PropertyID, "tax info", driver, "TX", "Comal");
                    //Tax Breakdown Details Table: 
                    //Entity~Description~Assessed~Homestead Exemption~OV65 or DP Exemption~Other Exemption~Freeze Year~Freeze Ceiling~Taxable Value~Rate per $100~Base Tax Due
                    IWebElement multitableElement71 = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[3]/div/table/tbody"));
                    IList<IWebElement> multitableRow71 = multitableElement71.FindElements(By.TagName("tr"));
                    IList<IWebElement> multirowTD71;
                    foreach (IWebElement row in multitableRow71)
                    {
                        multirowTD71 = row.FindElements(By.TagName("td"));
                        if (multirowTD71.Count != 0)
                        {
                            string tax_break = multirowTD71[0].Text.Trim() + "~" + multirowTD71[1].Text.Trim() + "~" + multirowTD71[2].Text.Trim() + "~" + multirowTD71[3].Text.Trim() + "~" + multirowTD71[4].Text.Trim() + "~" + multirowTD71[5].Text.Trim() + "~" + multirowTD71[6].Text.Trim() + "~" + multirowTD71[7].Text.Trim() + "~" + multirowTD71[8].Text.Trim() + "~" + multirowTD71[9].Text.Trim() + "~" + multirowTD71[10].Text.Trim();
                            gc.insert_date(orderNumber, PropertyID, 867, tax_break, 1, DateTime.Now);
                        }
                    }

                    //Property ID~Tax Year~Owner Name~Address~Property Type~Geographic ID~Legal Description~Property Location~Assessed Value~Original Tax Amount~Taxes Paid~Penalty/Interest~Fees~Total Due~Payment Status~Last Payment Date~Net Taxes Paid~Total Due~Tax Authority


                    string id = "", year = "", oname = "", taxaddress = "", propertytype = "", Gid = "", legaldis = "", propertyLocation = "", assessedValue = "", OriginalTaxAmount = "", TaxesPaid = "", PenaltyInterest = "", Fees = "", TotalDue = "", PaymentStatus = "", LastPaymentDate = "", NettTaxesPaid = "", TotalDueTax = "", TaxAuthority = "205 North Seguin Ave. New Braunfels, Texas 78130 (830) 221-1353";
                    //   Bill Information
                    string fullBill = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[1]/div[1]/table/tbody")).Text.Replace("\r\n", " ");
                    id = gc.Between(fullBill, "Property ID", "Tax Year");
                    year = gc.Between(fullBill, "Tax Year", "Owner Name");
                    oname = gc.Between(fullBill, "Owner Name", "Address");
                    taxaddress = gc.Between(fullBill, "Address", "Property Type");
                    propertytype = GlobalClass.After(fullBill, "Property Type");
                    //Tax Information
                    string fullTax = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[1]/div[2]/table/tbody")).Text.Replace("\r\n", " ");
                    OriginalTaxAmount = gc.Between(fullTax, "Original Tax Amount", "Taxes Paid");
                    TaxesPaid = gc.Between(fullTax, "Taxes Paid", "Penalty/Interest");
                    PenaltyInterest = gc.Between(fullTax, "Penalty/Interest", "Fees");
                    Fees = gc.Between(fullTax, "Fees", "Total Due");
                    TotalDue = GlobalClass.After(fullTax, "Total Due");

                    //Property Information
                    string fullTaxProperty = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[2]/div[1]/table/tbody")).Text.Replace("\r\n", " ");
                    Gid = gc.Between(fullTaxProperty, "Geographic ID", "Legal Description");
                    legaldis = gc.Between(fullTaxProperty, "Legal Description", "Property Location");
                    propertyLocation = gc.Between(fullTaxProperty, "Property Location", "Assessed Value");
                    assessedValue = GlobalClass.After(fullTaxProperty, "Assessed Value");
                    //Payment Information
                    try
                    {
                        string fullPayment = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[2]/div[2]/table/tbody")).Text.Replace("\r\n", " ");
                        PaymentStatus = gc.Between(fullPayment, "Payment Status", "Last Payment Date");
                        LastPaymentDate = gc.Between(fullPayment, "Last Payment Date", "Net Taxes Paid");
                        NettTaxesPaid = gc.Between(fullPayment, "Net Taxes Paid", "Total Due");
                        TotalDueTax = GlobalClass.After(fullPayment, "Total Due");
                    }
                    catch
                    {
                        string fullPayment = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[2]/div[2]/table/tbody")).Text.Replace("\r\n", " ");
                        PaymentStatus = gc.Between(fullPayment, "Payment Status", "Net Taxes Paid");
                        NettTaxesPaid = gc.Between(fullPayment, "Net Taxes Paid", "Total Due");
                        TotalDueTax = GlobalClass.After(fullPayment, "Total Due");
                        LastPaymentDate = "";
                    }
                    //Property ID~Tax Year~Owner Name~Address~Property Type~Geographic ID~Legal Description~Property Location~Assessed Value~Original Tax Amount~Taxes Paid~Penalty/Interest~Fees~Total Due~Payment Status~Last Payment Date~Net Taxes Paid~Total Due1~Tax Authority
                    string tax_bill = id + "~" + year + "~" + oname + "~" + taxaddress + "~" + propertytype + "~" + Gid + "~" + legaldis + "~" + propertyLocation + "~" + assessedValue + "~" + OriginalTaxAmount + "~" + TaxesPaid + "~" + PenaltyInterest + "~" + PaymentStatus + "~" + Fees + "~" + TotalDue + "~" + LastPaymentDate + "~" + NettTaxesPaid + "~" + TotalDueTax + "~" + "205 North Seguin Ave. New Braunfels, Texas 78130 (830) 221-1353";
                    gc.insert_date(orderNumber, PropertyID, 868, tax_bill, 1, DateTime.Now);
                    try
                    {
                        // View & Print Bill
                        driver.FindElement(By.XPath("//*[@id='avalon']/div/div/ul/li[2]/a")).Click();
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, PropertyID, "View & Print Bill", driver, "TX", "Comal");
                    }
                    catch
                    { }
                    try
                    {
                        // View & Print Receipt
                        driver.FindElement(By.XPath("//*[@id='avalon']/div/div/ul/li[3]/a")).Click();
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, PropertyID, "View & Print Receipt", driver, "TX", "Comal");
                    }
                    catch { }
                    // View & Print Tax History
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='avalon']/div/div/ul/li[4]/a")).Click();
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, PropertyID, "View & Print Tax History", driver, "TX", "Comal");
                    }
                    catch { }



                    //Taxing Unit~Assessed Value~Cap Value~Exemption Amount~Taxable Value~Tax Rate~Tax Amount~3% Oct~2% Nov~1% Dec 
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "TX", "Comal", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    driver.Quit();
                    GlobalClass.titleparcel = "";
                    gc.mergpdf(orderNumber, "TX", "Comal");
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