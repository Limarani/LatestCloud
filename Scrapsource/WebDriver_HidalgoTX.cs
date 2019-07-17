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
    public class WebDriver_HidalgoTX
    {
        IWebElement PaidCount;
        IWebDriver driver;
        DBconnection db = new DBconnection();
        List<string> link = new List<string>();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());


        public string FTP_Hidalgo(string houseno, string sname, string stype, string unitno, string direction, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
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

                    driver.Navigate().GoToUrl("http://propaccess.hidalgoad.org/clientdb/?cid=1");
                    driver.FindElement(By.Id("propertySearchOptions_advanced")).Click();
                    Thread.Sleep(4000);
                    if (searchType == "titleflex")
                    {
                        string titleaddress = houseno + " " + sname + " " + stype + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "TX", "Hidalgo");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_Hidalgo"] = "Yes";
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
                        gc.CreatePdf_WOP(orderNumber, "address search", driver, "TX", "Hidalgo");
                        driver.FindElement(By.Id("propertySearchOptions_searchAdv")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "address search result", driver, "TX", "Hidalgo");
                    }
                    if (searchType == "parcel")
                    {
                        //var Select1 = driver.FindElement(By.Id("propertySearchOptions_searchType"));
                        //var selectElement11 = new SelectElement(Select1);
                        //selectElement11.SelectByText("Account Number");
                        driver.FindElement(By.Id("propertySearchOptions_propertyid")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search", driver, "TX", "Hidalgo");
                        driver.FindElement(By.Id("propertySearchOptions_searchAdv")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search result", driver, "TX", "Hidalgo");
                    }

                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("propertySearchOptions_ownerName")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "owner search", driver, "TX", "Hidalgo");
                        driver.FindElement(By.Id("propertySearchOptions_searchAdv")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "owner search result", driver, "TX", "Hidalgo");
                    }
                    if (searchType == "block")
                    {
                        //var Select1 = driver.FindElement(By.Id("propertySearchOptions_searchType"));
                        //var selectElement11 = new SelectElement(Select1);
                        //selectElement11.SelectByText("Account Number");
                        driver.FindElement(By.Id("propertySearchOptions_geoid")).SendKeys(unitno);
                        gc.CreatePdf_WOP(orderNumber, "Account Number Search ", driver, "TX", "Hidalgo");
                        driver.FindElement(By.Id("propertySearchOptions_searchAdv")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Account Number Search  result", driver, "TX", "Hidalgo");

                        //       gc.CreatePdf_WOP(orderNumber, "owner search result", driver, "TX", "Williamson");
                    }
                    //Geographic ID~Type~Property Address~Owner Name

                    int trCount = driver.FindElements(By.XPath("//*[@id='propertySearchResults_resultsTable']/tbody/tr")).Count;

                    int maxCheck = 0;
                    IWebElement tbmulti = driver.FindElement(By.XPath("//*[@id='propertySearchResults_resultsTable']/tbody"));
                    IList<IWebElement> TRmulti5 = tbmulti.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDmulti5;
                    foreach (IWebElement row in TRmulti5)
                    {
                        if (maxCheck <= 25)
                        {
                            TDmulti5 = row.FindElements(By.TagName("td"));
                            if (TDmulti5.Count == 11 && row.Text.Contains("Real") )
                            {
                                string multi1 = TDmulti5[2].Text + "~" + TDmulti5[3].Text + "~" + TDmulti5[4].Text + "~" + TDmulti5[6].Text;
                                gc.insert_date(orderNumber, TDmulti5[1].Text, 757, multi1, 1, DateTime.Now);

                                maxCheck++;
                            }

                        }
                    }
                    if (maxCheck > 25)
                    {
                        HttpContext.Current.Session["multiParcel_Hidalgo_Multicount"] = "Maximum";
                        driver.Quit();
                        return "MultiParcel";
                    }


                    if (maxCheck == 1)
                    {
                        string type = driver.FindElement(By.XPath("//*[@id='propertySearchResults_resultsTable']/tbody/tr[2]/td[4]")).Text.Replace("\r\n", " ");
                        if (type == "Real")
                        {//*[@id="propertySearchResults_resultsTable"]/tbody/tr[2]/td[10]/a
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
                    else
                    {
                        if (maxCheck != 0)
                        {
                            HttpContext.Current.Session["multiparcel_Hidalgo"] = "Yes";
                            driver.Quit();
                            return "MultiParcel";
                        }
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.Id("propertySearchResults_pageHeading"));
                        if(INodata.Text.Contains("None found"))
                        {
                            HttpContext.Current.Session["Nodata_Hidalgo"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }



                    //property details
                    string PropertyID = "", GeographicID = "", Name = "", MailingAddress = "", OwnerID = "", LegalDescription = "", Neighborhood = "", Type = "", Address = "", NeighborhoodCD = "", Exemptions = "", MapID = "", YearBuilt = "", Acres = "";
                    string ownerdetails = "";
                    string asd = "";
                    //Property ID~Geographic ID~Owner Name~Mailing Address~Owner ID~Type~Legal Description~Address~Neighborhood~Neighborhood CD~Map ID~Exemptions~Year Built~Acres                    
                    IWebElement multitableElement17 = driver.FindElement(By.XPath("//*[@id='propertyDetails']/table/tbody"));
                    IList<IWebElement> multitableRow17 = multitableElement17.FindElements(By.TagName("tr"));
                    IList<IWebElement> multirowTD17;
                    foreach (IWebElement row in multitableRow17)
                    {
                        multirowTD17 = row.FindElements(By.TagName("td"));
                        if (row.Text.Contains("Owner ID:"))
                        {
                            asd = multirowTD17[3].Text.Trim();
                            if (asd.Contains("Property ID"))
                            {
                                asd = GlobalClass.Before(asd, " (Property ID");
                            }
                            ownerdetails = ownerdetails + multirowTD17[1].Text.Trim() + ";" + asd;
                        }
                        if (row.Text.Contains("Mailing Address:"))
                        {
                            ownerdetails = ownerdetails + ";" + multirowTD17[1].Text.Trim().Replace("\r\n", " ") + " & ";
                        }
                    }

                    string fulltext = driver.FindElement(By.XPath("//*[@id='propertyDetails']/table/tbody")).Text.Replace("\r\n", " ");
                    PropertyID = gc.Between(fulltext, "Account Property ID:", " Legal Description: ");
                    //   gc.CreatePdf(orderNumber, parcelNumber, "property details", driver, "TX", "Hidalgo");
                    GeographicID = gc.Between(fulltext, "Geographic ID: ", "Agent Code: ");
                    // Name = gc.Between(fulltext, "Owner Name:", "Owner ID:");
                    //  MailingAddress = gc.Between(fulltext, "Mailing Address:", " % Ownership:");
                    //  OwnerID = gc.Between(fulltext, "Owner ID:", "Mailing Address:");
                    LegalDescription = gc.Between(fulltext, "Legal Description:", "Geographic ID:");
                    Neighborhood = gc.Between(fulltext, "Neighborhood:", "Map ID:");
                    Type = gc.Between(fulltext, "Type:", "Property Use Code:");
                    Address = gc.Between(fulltext, "Location Address:", "Mapsco:");
                    NeighborhoodCD = gc.Between(fulltext, "Neighborhood CD:", "Owner");
                    Exemptions = GlobalClass.After(fulltext, "Exemptions:");
                    MapID = gc.Between(fulltext, "Map ID:", "Neighborhood CD:");
                    //*[@id="improvementBuilding"]
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
                    string property_details = PropertyID + "~" + GeographicID + "~" + ownerdetails + "~" + Type + "~" + LegalDescription + "~" + Address + "~" + Neighborhood + "~" + NeighborhoodCD + "~" + MapID + "~" + Exemptions + "~" + YearBuilt + "~" + Acres;
                    gc.insert_date(orderNumber, PropertyID, 751, property_details, 1, DateTime.Now);

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
                            gc.insert_date(orderNumber, PropertyID, 754, ValueDetails, 1, DateTime.Now);

                        }
                    }
                    //  var list = new List<string> { "CITY OF DONNA", "CITY OF MCALLEN", "DELTA LAKE WTR DIST", "DONNA WATER DIST #1", "DONNA IRRIGATION DIST", "DELTA LAKE IRRIGATION" };
                    // int count = list.Count;
                    //List<string> ListName = new List<string>();
                    //ListName.Add("CITY OF DONNA");

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
                    ValueDetails1 = owner + "~" + Ownership + "~" + TotalValue + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                    gc.insert_date(orderNumber, PropertyID, 753, ValueDetails1, 1, DateTime.Now);
                    string msg1 = "You must call the specific Collector's Office";
                    string city = "";
                    IWebElement multitableElement11 = driver.FindElement(By.XPath("//*[@id='taxingJurisdictionDetails']/table[2]/tbody"));
                    IList<IWebElement> multitableRow11 = multitableElement11.FindElements(By.TagName("tr"));
                    IList<IWebElement> multirowTD11;
                    foreach (IWebElement row in multitableRow11)
                    {
                        //CITY OF ALAMO,CITY OF ALTON,CITY OF DONNA,City of Edcouch,CITY OF EDINBURG,CITY OF EDINBURG,City of Elsa,City of Granjeno,City of Hidalgo,City of La Joya,CITY OF LA VILLA,CITY OF MCALLEN,CITY OF MERCEDES,CITY OF MISSION,City of Palmview,City of Penitas,CITY OF PHARR,City of Progreso,CITY OF SAN JUAN,City of Sullivan ,CITY OF WESLACO
                        multirowTD11 = row.FindElements(By.TagName("td"));
                        if (multirowTD11.Count != 0)
                        {

                            if (row.Text.Contains("DELTA LAKE WTR DIST") || row.Text.Contains("DONNA WATER DIST #1") || row.Text.Contains("CITY OF MCALLEN") || row.Text.Contains("CITY OF ALAMO") || row.Text.Contains("CITY OF ALTON") || row.Text.Contains("City of Edcouch") || row.Text.Contains("CITY OF EDINBURG") || row.Text.Contains("City of Elsa") || row.Text.Contains("City of Granjeno") || row.Text.Contains("City of Hidalgo") || row.Text.Contains("City of La Joya") || row.Text.Contains("CITY OF LA VILLA") || row.Text.Contains("CITY OF MERCEDES") || row.Text.Contains("CITY OF MISSION") || row.Text.Contains("City of Palmview") || row.Text.Contains("City of Penitas") || row.Text.Contains("CITY OF PHARR") || row.Text.Contains("City of Progreso") || row.Text.Contains("CITY OF SAN JUAN") || row.Text.Contains("City of Sullivan") || row.Text.Contains("CITY OF WESLACO"))
                            {
                                HttpContext.Current.Session["tax_alert_msg"] = "Yes";
                                if (multirowTD11[2].Text != "0.000000")
                                {
                                    msg = "success";
                                    city = multirowTD11[1].Text.Trim();
                                    ValueDetails1 = " " + "~" + " " + "~" + " " + "~" + multirowTD11[0].Text.Trim() + "~" + multirowTD11[1].Text.Trim() + "~" + multirowTD11[2].Text.Trim() + "~" + multirowTD11[3].Text.Trim() + "~" + multirowTD11[4].Text.Trim() + "~" + multirowTD11[5].Text.Trim() + "~" + "" + "~" + "" + "~" + "Need to Call Tax Office";
                                    gc.insert_date(orderNumber, PropertyID, 753, ValueDetails1, 1, DateTime.Now);

                                }
                            }

                            else if (multirowTD11[1].Text.Contains("CITY OF DONNA"))
                            {
                                HttpContext.Current.Session["tax_alert_msg"] = "Yes";
                                msg = "success";
                                string donna_taxassessor = "Norma Yanez";
                                string donna_phone = "956-464-3314";
                                //ValueDetails1 = owner + "~" + Ownership + "~" + TotalValue + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + donna_taxassessor + "~" + donna_phone + "~" +msg1;
                                ValueDetails1 = " " + "~" + " " + "~" + " " + "~" + multirowTD11[0].Text.Trim() + "~" + multirowTD11[1].Text.Trim() + "~" + multirowTD11[2].Text.Trim() + "~" + multirowTD11[3].Text.Trim() + "~" + multirowTD11[4].Text.Trim() + "~" + multirowTD11[5].Text.Trim() + "~" + donna_taxassessor + "~" + donna_phone + "~" + msg1;
                                gc.insert_date(orderNumber, PropertyID, 753, ValueDetails1, 1, DateTime.Now);
                            }
                            else if (multirowTD11[1].Text.Contains("DONNA IRRIGATION DIST"))
                            {
                                HttpContext.Current.Session["tax_alert_msg"] = "Yes";
                                msg = "success";
                                string donnaIrrigation_taxassessor = "Nora Zapata";
                                string donnaIrrigation_phone = "956-464-3641";
                                ValueDetails1 = " " + "~" + " " + "~" + " " + "~" + multirowTD11[0].Text.Trim() + "~" + multirowTD11[1].Text.Trim() + "~" + multirowTD11[2].Text.Trim() + "~" + multirowTD11[3].Text.Trim() + "~" + multirowTD11[4].Text.Trim() + "~" + multirowTD11[5].Text.Trim() + "~" + donnaIrrigation_taxassessor + "~" + donnaIrrigation_phone + "~" + msg1;
                                gc.insert_date(orderNumber, PropertyID, 753, ValueDetails1, 1, DateTime.Now);
                            }
                            else if (multirowTD11[1].Text.Contains("DELTA LAKE IRRIGATION"))
                            {
                                HttpContext.Current.Session["tax_alert_msg"] = "Yes";
                                msg = "success";
                                string delta_taxassessor = "Richard Kolterman";
                                string delta_phone = "956-262-2101";
                                ValueDetails1 = " " + "~" + " " + "~" + " " + "~" + multirowTD11[0].Text.Trim() + "~" + multirowTD11[1].Text.Trim() + "~" + multirowTD11[2].Text.Trim() + "~" + multirowTD11[3].Text.Trim() + "~" + multirowTD11[4].Text.Trim() + "~" + multirowTD11[5].Text.Trim() + "~" + delta_taxassessor + "~" + delta_phone + "~" + msg1;
                                gc.insert_date(orderNumber, PropertyID, 753, ValueDetails1, 1, DateTime.Now);
                            }
                            else
                            {
                                ValueDetails1 = " " + "~" + " " + "~" + " " + "~" + multirowTD11[0].Text.Trim() + "~" + multirowTD11[1].Text.Trim() + "~" + multirowTD11[2].Text.Trim() + "~" + multirowTD11[3].Text.Trim() + "~" + multirowTD11[4].Text.Trim() + "~" + multirowTD11[5].Text.Trim() + "~" + "" + "~" + "" + "~" + "";
                                gc.insert_date(orderNumber, PropertyID, 753, ValueDetails1, 1, DateTime.Now);
                            }
                        }
                    }
                    //if(msg== "success")
                    //{                  
                    //    gc.insert_date(orderNumber, PropertyID, 753, ValueDetails1, 1, DateTime.Now);
                    //}
                    //else
                    //{
                    //    gc.insert_date(orderNumber, PropertyID, 753, ValueDetails1, 1, DateTime.Now);
                    //}
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
                            gc.insert_date(orderNumber, PropertyID, 752, rollDetails, 1, DateTime.Now);
                        }
                    }


                    //Tax Due (Payoff Calculation) Details Table:

                    driver.FindElement(By.Id("taxDue")).Click();
                    Thread.Sleep(2000);

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
                    Thread.Sleep(2000);

                    string goodthroughdate = driver.FindElement(By.Id("taxDueDetails_recalculateDate")).Text;
                    string payoff1 = goodthroughdate + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                    gc.insert_date(orderNumber, PropertyID, 762, payoff1, 1, DateTime.Now);

                    //Amount Due if Paid on~Year~Taxing Jurisdiction~Taxable Value~Base Tax~Base Taxes Paid~Base Tax Due~Discount / Penalty & Interest~Attorney Fees~Amount Due
                    IWebElement multitableElement5 = driver.FindElement(By.XPath("//*[@id='taxDueDetails_dataSection']/table/tbody"));
                    IList<IWebElement> multitableRow5 = multitableElement5.FindElements(By.TagName("tr"));
                    IList<IWebElement> multirowTD5;
                    foreach (IWebElement row in multitableRow5)
                    {
                        multirowTD5 = row.FindElements(By.TagName("td"));
                        if (multirowTD5.Count != 0)
                        {
                            string payoff11 = "" + "~" + multirowTD5[0].Text.Trim() + "~" + multirowTD5[1].Text.Trim() + "~" + multirowTD5[2].Text.Trim() + "~" + multirowTD5[3].Text.Trim() + "~" + multirowTD5[4].Text.Trim() + "~" + multirowTD5[5].Text.Trim() + "~" + multirowTD5[6].Text.Trim() + "~" + multirowTD5[7].Text.Trim() + "~" + multirowTD5[8].Text.Trim();
                            gc.insert_date(orderNumber, PropertyID, 762, payoff11, 1, DateTime.Now);
                        }
                    }
                    gc.CreatePdf(orderNumber, PropertyID, "property details", driver, "TX", "Hidalgo");

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    try
                    {
                        driver.Navigate().GoToUrl("https://actweb.acttax.com/act_webdev/hidalgo/index.jsp");
                        Thread.Sleep(3000);


                        driver.FindElement(By.Id("sc5")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.Id("criteria")).SendKeys(PropertyID);
                        gc.CreatePdf(orderNumber, PropertyID, "tax search", driver, "TX", "Hidalgo");
                        driver.FindElement(By.Name("submit")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, PropertyID, "tax search result", driver, "TX", "Hidalgo");
                        driver.FindElement(By.XPath("//*[@id='data-block']/table/tbody/tr/td/table/tbody/tr/td[1]/h3/a")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, PropertyID, "tax search result details", driver, "TX", "Hidalgo");
                        //Jurisdiction Information Details Table: 
                        string year = "", account = "", ExemptionsJ = "";


                        driver.FindElement(By.LinkText("Exemption and Tax Rate Information")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, PropertyID, "Exemption and Tax Rate Information", driver, "TX", "Hidalgo");
                        //Year~Account No~Exemptions~Jurisdictions~Market Value~Exemption Value~Taxable Value~Tax Rate~Levy
                        string fulltextJurisdiction = driver.FindElement(By.XPath("//*[@id='pageContent']/table[2]/tbody/tr[1]/td/table/tbody/tr[1]/td")).Text.Replace("\r\n", " ");
                        year = gc.Between(fulltextJurisdiction, "Jurisdiction Information for", "Account No");

                        account = gc.Between(fulltextJurisdiction, "Account No.:", " Exemptions: ");
                        ExemptionsJ = GlobalClass.After(fulltextJurisdiction, "Exemptions:");
                        string taxmsg = "";
                        string Jurisdiction1 = year + "~" + account + "~" + ExemptionsJ + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                        gc.insert_date(orderNumber, PropertyID, 755, Jurisdiction1, 1, DateTime.Now);
                        IWebElement multitableElement3 = driver.FindElement(By.XPath("//*[@id='pageContent']/table[2]/tbody/tr[1]/td/table/tbody/tr[2]/td/table/tbody"));
                        IList<IWebElement> multitableRow3 = multitableElement3.FindElements(By.TagName("tr"));
                        IList<IWebElement> multirowTD3;
                        foreach (IWebElement row in multitableRow3)
                        {
                            multirowTD3 = row.FindElements(By.TagName("td"));
                            if (multirowTD3.Count == 6 && !row.Text.Contains("Jurisdictions"))
                            {
                                if (multirowTD3[0].Text.Trim() == city)
                                {
                                    taxmsg = "success";
                                }

                                string Jurisdiction = "" + "~" + "" + "~" + "" + "~" + multirowTD3[0].Text.Trim() + "~" + multirowTD3[1].Text.Trim() + "~" + multirowTD3[2].Text.Trim() + "~" + multirowTD3[3].Text.Trim() + "~" + multirowTD3[4].Text.Trim() + "~" + multirowTD3[5].Text.Trim();
                                gc.insert_date(orderNumber, PropertyID, 755, Jurisdiction, 1, DateTime.Now);
                            }
                            if (row.Text.Contains("No tax jurisdictions"))
                            {
                                string Jurisdiction = "" + "~" + "" + "~" + "" + "~" + multirowTD3[0].Text.Trim() + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                gc.insert_date(orderNumber, PropertyID, 755, Jurisdiction, 1, DateTime.Now);
                            }
                        }
                        if (taxmsg == "" && city != "")
                        {
                            HttpContext.Current.Session["tax_alert_msges"] = city;
                            HttpContext.Current.Session["tax_alert_msg1"] = "Yes";
                        }
                        driver.FindElement(By.LinkText("Return to the Previous Page")).Click();
                        Thread.Sleep(2000);


                        driver.FindElement(By.LinkText("Taxes Due Detail by Year and Jurisdiction")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, PropertyID, "Taxes Due Detail by Year and Jurisdiction", driver, "TX", "Hidalgo");
                        //Taxes Due Detail by Year Details Table: 
                        //Account No~Appr. Dist. No~Active Lawsuits~Year~Base Tax Due~Penalty, Interest, and ACC* Due(end of July )~Total Due July~Penalty, Interest, and ACC* Due(end of August )~Total Due August~Penalty, Interest, and ACC*Due(end of September)~Total Due September
                        string accountno = "", distno = "", ActiveLawsuits = "";
                        string fulltextTaxes = driver.FindElement(By.XPath("//*[@id='pageContent']/table[2]/tbody/tr[1]/td/center/table/tbody/tr/td/div")).Text.Replace("\r\n", " ");
                        accountno = gc.Between(fulltextTaxes, "Account No.:", "Appr. Dist. No.: ");
                        distno = gc.Between(fulltextTaxes, "Appr. Dist. No.: ", "Active Lawsuits");
                        ActiveLawsuits = GlobalClass.After(fulltextTaxes, "Active Lawsuits");

                        string TaxesDue1 = accountno + "~" + distno + "~" + ActiveLawsuits + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                        gc.insert_date(orderNumber, PropertyID, 756, TaxesDue1, 1, DateTime.Now);

                        IWebElement multitableElement31 = driver.FindElement(By.XPath("//*[@id='pageContent']/table[2]/tbody/tr[1]/td/center/table/tbody/tr/td/table/tbody"));
                        IList<IWebElement> multitableRow31 = multitableElement31.FindElements(By.TagName("tr"));
                        IList<IWebElement> multirowTD31;
                        foreach (IWebElement row in multitableRow31)
                        {
                            multirowTD31 = row.FindElements(By.TagName("td"));
                            if (multirowTD31.Count == 8 && !row.Text.Contains("Base Tax Due"))
                            {
                                string TaxesDue = "" + "~" + "" + "~" + "" + "~" + multirowTD31[0].Text.Trim() + "~" + multirowTD31[1].Text.Trim() + "~" + multirowTD31[2].Text.Trim() + "~" + multirowTD31[3].Text.Trim() + "~" + multirowTD31[4].Text.Trim() + "~" + multirowTD31[5].Text.Trim() + "~" + multirowTD31[6].Text.Trim() + "~" + multirowTD31[7].Text.Trim();
                                gc.insert_date(orderNumber, PropertyID, 756, TaxesDue, 1, DateTime.Now);
                            }
                            if (multirowTD31.Count == 1)
                            {
                                string TaxesDue = multirowTD31[0].Text.Trim() + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                gc.insert_date(orderNumber, PropertyID, 756, TaxesDue, 1, DateTime.Now);

                            }
                        }

                        driver.FindElement(By.LinkText("Return to the Previous Page")).Click();
                        Thread.Sleep(2000);
                        //Tax Payment Details Table: 

                        driver.FindElement(By.LinkText("Payment Information")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, PropertyID, "Payment Information", driver, "TX", "Hidalgo");
                        //Account No~Receipt Date~Amount~Tax Year
                        string accountnos = driver.FindElement(By.XPath("//*[@id='pageContent']/table[2]/tbody/tr[1]/td/h3[3]")).Text.Replace("Account No.:", "");

                        IWebElement multitableElement32 = driver.FindElement(By.XPath("//*[@id='pageContent']/table[2]/tbody/tr[1]/td/table/tbody"));
                        IList<IWebElement> multitableRow32 = multitableElement32.FindElements(By.TagName("tr"));
                        IList<IWebElement> multirowTD32;
                        foreach (IWebElement row in multitableRow32)
                        {
                            multirowTD32 = row.FindElements(By.TagName("td"));
                            if (multirowTD32.Count != 0 && !row.Text.Contains("Receipt Date"))
                            {
                                string TaxesDue = accountnos + "~" + multirowTD32[0].Text.Trim() + "~" + multirowTD32[1].Text.Trim() + "~" + multirowTD32[2].Text.Trim();
                                gc.insert_date(orderNumber, PropertyID, 758, TaxesDue, 1, DateTime.Now);
                            }
                        }

                        driver.FindElement(By.LinkText("Return to the Previous Page")).Click();
                        Thread.Sleep(2000);

                        //Account Number~Appraisal District Number~Owner Information~Property Site Address~Legal Description~Current Tax Levy~Current Amount Due~Prior Year Amount Due~Total Amount Due~Last Payment Amount for Current Year Taxes~Last Payer for Current Year Taxes~Last Payment Date for Current Year Taxes~Active Lawsuits~Gross Value~Land Value~Improvement Value~Capped Value~Agricultural Value~Exemptions~Tax Authority

                        string accountNo = "", AppraisalDistrict = "", OwnerName = "", OwnerAddress = "", PropertyAddress = "", legal = "", CurrentTax = "", CurrentAmount = "", PriorYearAmount = "", TotalAmount = "", LastPaymentAmount = "", LastPayer = "", LastPaymentDate = "", ActiveLawsuitsTax = "", GrossValue = "", LandValue = "", ImprovementValue = "", CappedValue = "", AgriculturalValue = "", ExemptionsTax = "";
                        string fullTaxeBill1 = driver.FindElement(By.XPath("//*[@id='pageContent']/table[2]/tbody/tr[2]/td/table[2]/tbody/tr/td[1]")).Text.Replace("\r\n", " ");

                        accountno = gc.Between(fullTaxeBill1, "Account Number:", "Appraisal District Number: ");
                        AppraisalDistrict = gc.Between(fullTaxeBill1, "Appraisal District Number: ", "Address:");
                        OwnerName = gc.Between(fullTaxeBill1, "Address:", " Property Site Address:");
                        // OwnerAddress = gc.Between(fullTaxeBill1, "", "");
                        PropertyAddress = gc.Between(fullTaxeBill1, "Property Site Address:", " Legal Description:");
                        legal = gc.Between(fullTaxeBill1, "Legal Description:", " Current Tax Levy:");
                        CurrentTax = gc.Between(fullTaxeBill1, " Current Tax Levy:", "Current Amount Due:");
                        CurrentAmount = gc.Between(fullTaxeBill1, "Current Amount Due:", "Prior Year Amount Due:");
                        PriorYearAmount = gc.Between(fullTaxeBill1, "Prior Year Amount Due:", "Total Amount Due:");
                        TotalAmount = gc.Between(fullTaxeBill1, "Total Amount Due:", "Last Payment Amount for Current Year Taxes:");
                        LastPaymentAmount = gc.Between(fullTaxeBill1, "Last Payment Amount for Current Year Taxes:", "Last Payer for Current Year Taxes:");
                        LastPayer = gc.Between(fullTaxeBill1, "Last Payer for Current Year Taxes:", "Last Payment Date for Current Year Taxes:");
                        LastPaymentDate = gc.Between(fullTaxeBill1, "Last Payment Date for Current Year Taxes:", "Active Lawsuits:");
                        string fullTaxeBill2 = driver.FindElement(By.XPath("//*[@id='pageContent']/table[2]/tbody/tr[2]/td/table[2]/tbody/tr/td[2]")).Text.Replace("\r\n", " ");
                        ActiveLawsuitsTax = GlobalClass.After(fullTaxeBill1, "Active Lawsuits:");
                        GrossValue = gc.Between(fullTaxeBill2, "Gross Value:", "Land Value:");
                        LandValue = gc.Between(fullTaxeBill2, "Land Value:", "Improvement Value:");
                        ImprovementValue = gc.Between(fullTaxeBill2, "Improvement Value:", "Capped Value:");
                        CappedValue = gc.Between(fullTaxeBill2, "Capped Value:", "Agricultural Value:");
                        AgriculturalValue = gc.Between(fullTaxeBill2, "Agricultural Value:", "Exemptions:");
                        ExemptionsTax = gc.Between(fullTaxeBill2, "Exemptions:", "Exemption and Tax Rate Information ");

                        string taxbill = accountno + "~" + AppraisalDistrict + "~" + OwnerName + "~" + PropertyAddress + "~" + legal + "~" + CurrentTax + "~" + CurrentAmount + "~" + PriorYearAmount + "~" + TotalAmount + "~" + LastPaymentAmount + "~" + LastPayer + "~" + LastPaymentDate + "~" + ActiveLawsuitsTax + "~" + GrossValue + "~" + LandValue + "~" + ImprovementValue + "~" + CappedValue + "~" + AgriculturalValue + "~" + ExemptionsTax + "~" + "Administration Bldg 2804 S. Business Hwy 281 Edinburg, TX 78539 Ph: (956) 318-2157";
                        gc.insert_date(orderNumber, PropertyID, 759, taxbill, 1, DateTime.Now);

                        IWebElement Itaxstmt1 = driver.FindElement(By.LinkText("Print a Current Tax Statement"));
                        Thread.Sleep(2000);
                        string stmt11 = Itaxstmt1.GetAttribute("href");
                        driver.Navigate().GoToUrl(stmt11);
                        Thread.Sleep(4000);
                        IWebElement Itaxstmt = driver.FindElement(By.XPath("//*[@id='pageContent']/table[2]/tbody/tr[3]/td/div/h3/a"));
                        string stmt1 = Itaxstmt.GetAttribute("href");
                        gc.downloadfile(stmt1, orderNumber, PropertyID, "Tax statement", "TX", "Hidalgo");
                        driver.Navigate().Back();
                        Thread.Sleep(2000);
                        IWebElement Itaxstmt2 = driver.FindElement(By.XPath("//*[@id='pageContent']/table[2]/tbody/tr[2]/td/table[2]/tbody/tr/td[2]/a[2]"));
                        Thread.Sleep(2000);
                        string stmt2 = Itaxstmt2.GetAttribute("href");
                        driver.Navigate().GoToUrl(stmt2);
                        Thread.Sleep(4000);
                        IWebElement Itaxstmt3 = driver.FindElement(By.XPath("//*[@id='pageContent']/table[2]/tbody/tr[3]/td/div/h3/a"));
                        string stmt3 = Itaxstmt3.GetAttribute("href");
                        gc.downloadfile(stmt3, orderNumber, PropertyID, "Deliquent Tax statement", "TX", "Hidalgo");
                        TaxTime = DateTime.Now.ToString("HH:mm:ss");

                        //city tx

                        if (Address.Contains("MCALLEN"))
                        {
                            driver.Navigate().GoToUrl("http://mytax.eztaxonline.com/mcallen/cart/search/showAccountSearch.do");
                            Thread.Sleep(3000);
                            //driver.FindElement(By.LinkText("Search for Property By ID")).Click();
                            //Thread.Sleep(2000);
                            GeographicID = GeographicID.Replace("-", "");
                            driver.FindElement(By.Id("accountSearchSubView:accountSearchForm:id")).SendKeys(GeographicID);
                            gc.CreatePdf(orderNumber, PropertyID, "city tax", driver, "TX", "Hidalgo");
                            driver.FindElement(By.Id("accountSearchSubView:accountSearchForm:submitButton")).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, PropertyID, "city tax1", driver, "TX", "Hidalgo");
                            driver.FindElement(By.XPath("/html/body/div[3]/form[1]/div/center/table/tbody/tr[3]/td[3]/span/a")).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, PropertyID, "city tax2", driver, "TX", "Hidalgo");
                            driver.FindElement(By.Id("etaxTemplateForm:current1:0:year1")).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, PropertyID, "city tax3", driver, "TX", "Hidalgo");
                            //Account #~Owner Name~Mailing Address~Legal Description~TAX YEAR~Taxing Unit~Appraised Value~Net Taxable Value~Base Due~Penalty / Interest~Attorney Fees / Other Fees~Total~Base Due Total~Penalty / Interest Total~Attorney Fees / Other Fees Total~Grand Total
                            string Account = "", oname = "", Maddress = "", legalCity = "", TaxYear = "", TaxingUnit = "", AppraisedValue = "", NetTaxableValue = "", BaseDue = "", Penalty = "", attorney = "", Total = "", BaseDueTotal = "", PenaltyTotal = "", attorneyTotal = "", grandTotal = "";
                            Account = driver.FindElement(By.Id("etaxTemplateForm:text3")).Text.Trim();
                            oname = driver.FindElement(By.Id("etaxTemplateForm:text4")).Text.Trim();
                            Maddress = driver.FindElement(By.Id("etaxTemplateForm:text5")).Text.Trim();
                            legalCity = driver.FindElement(By.Id("etaxTemplateForm:text6")).Text.Trim();
                            TaxYear = driver.FindElement(By.Id("etaxTemplateForm:text7")).Text.Trim();
                            TaxingUnit = driver.FindElement(By.Id("etaxTemplateForm:entities1:0:description1")).Text.Trim();
                            AppraisedValue = driver.FindElement(By.Id("etaxTemplateForm:entities1:0:appraisedValue1")).Text.Trim();
                            NetTaxableValue = driver.FindElement(By.Id("etaxTemplateForm:entities1:0:netTaxableValue1")).Text.Trim();
                            BaseDue = driver.FindElement(By.Id("etaxTemplateForm:entities1:0:taxesDue1")).Text.Trim();
                            Penalty = driver.FindElement(By.Id("etaxTemplateForm:entities1:0:pniDue1")).Text.Trim();
                            attorney = driver.FindElement(By.Id("etaxTemplateForm:entities1:0:feeDue1")).Text.Trim();
                            Total = driver.FindElement(By.Id("etaxTemplateForm:entities1:0:totalDue1")).Text.Trim();
                            BaseDueTotal = driver.FindElement(By.Id("etaxTemplateForm:entities2:0:taxesDue2")).Text.Trim();
                            PenaltyTotal = driver.FindElement(By.Id("etaxTemplateForm:entities2:0:pniDue2")).Text.Trim();
                            attorneyTotal = driver.FindElement(By.Id("etaxTemplateForm:entities2:0:feeDue2")).Text.Trim();
                            grandTotal = driver.FindElement(By.Id("etaxTemplateForm:entities2:0:totalDue2")).Text.Trim();
                            string cityTax = Account + "~" + oname + "~" + Maddress + "~" + legalCity + "~" + TaxYear + "~" + TaxingUnit + "~" + AppraisedValue + "~" + NetTaxableValue + "~" + BaseDue + "~" + Penalty + "~" + attorney + "~" + Total + "~" + BaseDueTotal + "~" + PenaltyTotal + "~" + attorneyTotal + "~" + grandTotal + "~" + "311 N. 15th Street McAllen, TX 78501 Phone :[956]681-1330";
                            gc.insert_date(orderNumber, PropertyID, 760, cityTax, 1, DateTime.Now);
                            driver.FindElement(By.Id("etaxTemplateForm:text71paymenth")).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, PropertyID, "Payment History City", driver, "TX", "Hidalgo");
                            List<string> strTaxRealestate = new List<string>();
                            //Year~Tax Paid~Penalty/Interest~Fees~Total Paid
                            //Payment History
                            int k = 0;
                            IWebElement multitableElement4 = driver.FindElement(By.XPath("//*[@id='etaxTemplateForm:tableEx1']/tbody"));
                            IList<IWebElement> multitableRow4 = multitableElement4.FindElements(By.TagName("tr"));
                            IList<IWebElement> multirowTD4;
                            foreach (IWebElement row in multitableRow4)
                            {
                                multirowTD4 = row.FindElements(By.TagName("td"));
                                if (multirowTD4.Count != 0)
                                {
                                    string paymentHistory = multirowTD4[0].Text.Trim() + "~" + multirowTD4[1].Text.Trim() + "~" + multirowTD4[2].Text.Trim() + "~" + multirowTD4[3].Text.Trim() + "~" + multirowTD4[4].Text.Trim();
                                    if (k < 3)
                                    {
                                        string yearbill = multirowTD4[0].Text;
                                        IWebElement ITaxBillCount = multirowTD4[0].FindElement(By.TagName("a"));
                                        string strTaxReal = ITaxBillCount.GetAttribute("href");
                                        strTaxRealestate.Add(strTaxReal);
                                        k++;
                                    }
                                    gc.insert_date(orderNumber, PropertyID, 761, paymentHistory, 1, DateTime.Now);
                                }
                            }
                            int m = 1;
                            foreach (string real in strTaxRealestate)
                            {
                                driver.Navigate().GoToUrl(real);

                                Thread.Sleep(4000);
                                gc.CreatePdf(orderNumber, PropertyID, "Payment History" + m, driver, "TX", "Hidalgo");
                                m++;

                            }

                            try
                            {
                                IWebElement Itaxbill = driver.FindElement(By.LinkText("ORIGINAL TAX STATEMENT"));
                                string URL1 = Itaxbill.GetAttribute("href");
                                //gc.downloadfile(URL1, orderNumber, PropertyID, "City TaxBill", "TX", "Hidalgo");
                                var chromeOptions = new ChromeOptions();
                                var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];
                                chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                                chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                                chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                                chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                                var driver1 = new ChromeDriver(chromeOptions);
                                driver1.Navigate().GoToUrl(URL1);
                                try
                                {
                                    gc.AutoDownloadFile(orderNumber, PropertyID, "Hidalgo", "TX", "statements" + ".pdf");
                                }
                                catch
                                {
                                    driver1.Quit();
                                }
                                driver1.Quit();
                            }
                            catch (Exception e)
                            {

                            }
                        }
                    }
                    catch { }
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "TX", "Hidalgo", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    driver.Quit();
                    GlobalClass.titleparcel = "";
                    gc.mergpdf(orderNumber, "TX", "Hidalgo");
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