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
    public class Webdriver_BellTX
    {

        IWebElement PaidCount;
        IWebDriver driver;
        DBconnection db = new DBconnection();
        List<string> link = new List<string>();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_Bell(string houseno, string sname, string stype, string unitno, string direction, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            List<string> taxinformation = new List<string>();
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new ChromeDriver()
            //driver = new PhantomJSDriver()
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("http://esearch.bellcad.org/");
                    //IJavaScriptExecutor js2 = driver as IJavaScriptExecutor;
                    //IWebElement ParcelLinkSearch1 = driver.FindElement(By.XPath("//*[@id='home-page-tabs']/li[5]"));
                    //js2.ExecuteScript("arguments[0].click();", ParcelLinkSearch1);
                    driver.FindElement(By.XPath("//*[@id='home-page-tabs']/li[5]")).Click();
                    Thread.Sleep(2000);
                    // driver.FindElement(By.Id("propertySearchOptions_advanced")).Click();
                    if (searchType == "titleflex")
                    {
                        string titleaddress = houseno + " " + sname + " " + stype + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "TX", "Bell");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_BellTX"] = "Zero";
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
                        driver.FindElement(By.Id("StreetNumber")).SendKeys(houseno);
                        driver.FindElement(By.Id("StreetName")).SendKeys(sname);
                        gc.CreatePdf_WOP(orderNumber, "address search", driver, "TX", "Bell");
                        driver.FindElement(By.XPath("//*[@id='index-search']/div[4]/div/div/button")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "address search result", driver, "TX", "Bell");
                    }
                    if (searchType == "parcel")
                    {
                        //var Select1 = driver.FindElement(By.Id("propertySearchOptions_searchType"));
                        //var selectElement11 = new SelectElement(Select1);
                        //selectElement11.SelectByText("Account Number");
                        driver.FindElement(By.Id("GeoId")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search", driver, "TX", "Bell");
                        driver.FindElement(By.XPath("//*[@id='index-search']/div[4]/div/div/button")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search result", driver, "TX", "Bell");
                    }

                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("OwnerName")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "owner search", driver, "TX", "Bell");
                        driver.FindElement(By.XPath("//*[@id='index-search']/div[4]/div/div/button")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "owner search result", driver, "TX", "Bell");
                    }
                    if (searchType == "block")
                    {
                        //var Select1 = driver.FindElement(By.Id("propertySearchOptions_searchType"));
                        //var selectElement11 = new SelectElement(Select1);
                        //selectElement11.SelectByText("Account Number");
                        driver.FindElement(By.Id("PropertyId")).SendKeys(unitno);
                        gc.CreatePdf_WOP(orderNumber, "Account Number Search ", driver, "TX", "Bell");
                        driver.FindElement(By.XPath("//*[@id='index-search']/div[4]/div/div/button")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Account Number Search  result", driver, "TX", "Bell");

                        //       gc.CreatePdf_WOP(orderNumber, "owner search result", driver, "TX", "Williamson");
                    }
                    //Geographic ID~Type~Property Address~Owner Name
                    //*[@id="propertySearchResults_resultsTable"]/tbody


                    try
                    {
                        IWebElement INodata = driver.FindElement(By.Id("propertySearchResults_pageHeading"));
                        if (INodata.Text.Contains("None found"))
                        {
                            HttpContext.Current.Session["Nodata_BellTX"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    int trCount = driver.FindElements(By.XPath("//*[@id='grid']/div[2]/table/tbody")).Count;
                    if (trCount > 1)
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
                                    gc.insert_date(orderNumber, TDmulti5[1].Text, 846, multi1, 1, DateTime.Now);
                                }
                                maxCheck++;
                            }
                        }
                        if (TRmulti5.Count > 25)
                        {
                            HttpContext.Current.Session["multiParcel_Bell_Multicount"] = "Maximum";
                        }
                        else
                        {
                            HttpContext.Current.Session["multiparcel_Bell"] = "Yes";
                        }

                        driver.Quit();
                        return "MultiParcel";
                    }
                    else
                    {
                        string type = driver.FindElement(By.XPath("//*[@id='grid']/div[2]/table/tbody/tr/td[6]")).Text.Replace("\r\n", " ");
                        if (type == "Real")
                        {
                            //*[@id="propertySearchResults_resultsTable"]/tbody/tr[2]/td[10]
                            driver.FindElement(By.XPath("//*[@id='grid']/div[2]/table/tbody/tr/td[4]")).Click();
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
                    string ownerdetails = "";
                    string asd = "";
                    //Property ID~Geographic ID~Name~Mailing Address~Owner ID~Mailing Address~Owner ID~Type~Legal Description~Address~Neighborhood~Neighborhood CD~Map ID~Exemptions~Year Built~Acres~Tax Authority                    


                    string fulltext = driver.FindElement(By.XPath("//*[@id='detail-page']/div[3]/div[1]/div/table/tbody")).Text.Replace("\r\n", " ");
                    PropertyID = gc.Between(fulltext, "Property ID:", "Legal Description:");
                    GeographicID = gc.Between(fulltext, "Geographic ID:", "Agent Code:");
                    gc.CreatePdf(orderNumber, GeographicID.Trim(), "property details", driver, "TX", "Bell");
                    LegalDescription = gc.Between(fulltext, "Legal Description:", "Geographic ID:");
                    Name = gc.Between(fulltext, "Name:", "Mailing Address:");
                    MailingAddress = gc.Between(fulltext, "Mailing Address:", "% Ownership:");
                    Exemptions = GlobalClass.After(fulltext, "Exemptions:");
                    OwnerID = gc.Between(fulltext, "Owner ID:", "Name:");

                    Neighborhood = gc.Between(fulltext, "Neighborhood CD:", "Owner");
                    Type = gc.Between(fulltext, "Type:", "Location");
                    Address = gc.Between(fulltext, "Address:", "Map ID:");
                    NeighborhoodCD = gc.Between(fulltext, "Neighborhood CD:", "Owner");

                    MapID = gc.Between(fulltext, "Map ID:", "Neighborhood CD:");
                    //*[@id="improvementBuilding"]
                    //driver.FindElement(By.XPath("//*[@id='improvementBuilding']")).Click();
                    //Thread.Sleep(2000);
                    try
                    {
                        YearBuilt = driver.FindElement(By.XPath("//*[@id='detail-page']/div[5]/div[2]/table[1]/tbody/tr[2]/td[5]")).Text;
                    }
                    catch { }
                    //driver.FindElement(By.Id("land")).Click();
                    //Thread.Sleep(2000);
                    //try
                    //{
                    //    Acres = driver.FindElement(By.XPath("//*[@id='landDetails']/table/tbody/tr[2]/td[4]")).Text;
                    //}
                    //catch { }
                    string property_details = PropertyID + "~" + Name + "~" + MailingAddress + "~" + OwnerID + "~" + Type + "~" + LegalDescription + "~" + Address + "~" + Neighborhood + "~" + NeighborhoodCD + "~" + MapID + "~" + Exemptions + "~" + YearBuilt + "~" + "P.O.Box 390 Belton, TX 76513 Phone Number:  254 - 939 - 5841";
                    gc.insert_date(orderNumber, GeographicID, 841, property_details, 1, DateTime.Now);

                    //    Assessment Details Table:

                    //  Values Details Table:
                    //Description~Sign~Value1~Value2
                    //driver.FindElement(By.Id("values")).Click();
                    //Thread.Sleep(2000);
                    string Propertyhead = "";
                    string Propertyresult = "";
                    //*[@id="detail-page"]/div[3]/div[2]/div[1]/table/tbody
                    IWebElement multitableElement1 = driver.FindElement(By.XPath(" //*[@id='detail-page']/div[3]/div[2]/div[1]/table/tbody"));
                    IList<IWebElement> multitableRow1 = multitableElement1.FindElements(By.TagName("tr"));
                    IList<IWebElement> multirowTD1;
                    IList<IWebElement> multirowTH1;
                    foreach (IWebElement row in multitableRow1)
                    {
                        multirowTH1 = row.FindElements(By.TagName("tH"));
                        multirowTD1 = row.FindElements(By.TagName("td"));
                        if (multirowTD1.Count != 0 && multirowTD1[0].Text != " ")
                        {
                            Propertyhead += multirowTH1[0].Text;
                            Propertyresult += multirowTD1[0].Text;
                            //gc.insert_date(orderNumber, PropertyID, 844, ValueDetails, 1, DateTime.Now);
                        }
                    }
                    db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + Propertyhead + "' where Id = '" + 844 + "'");
                    gc.insert_date(orderNumber, PropertyID, 844, Propertyresult, 1, DateTime.Now);
                    //Taxing Jurisdiction Details Table:
                    //Owner~% Ownership~Total Value~Entity~Description~Tax Rate~Appraised Value~Taxable Value~Estimated Tax
                    //driver.FindElement(By.Id("taxingJurisdiction")).Click();
                    //Thread.Sleep(2000);
                    //string owner = "", Ownership = "", TotalValue = "";
                    string ValueDetails1 = "", msg = "";
                    //string fulltext1 = driver.FindElement(By.XPath("//*[@id='taxingJurisdictionDetails']/table[1]/tbody")).Text.Replace("\r\n", " ");
                    List<string> myList = new List<string>();
                    string Totaltax = driver.FindElement(By.XPath("//*[@id='detail-page']/div[4]/div[3]")).Text;
                    string TotalTaxRate = gc.Between(Totaltax, "Total Tax Rate:", "Estimated Taxes With Exemptions:");
                    string EstimatedTaxes = gc.Between(Totaltax, "With Exemptions:", "Without Exemptions:");
                    string WithoutExemptions = GlobalClass.After(Totaltax, "Without Exemptions:");
                    string totalresult = TotalTaxRate + "~" + EstimatedTaxes + "~" + WithoutExemptions;
                    gc.insert_date(orderNumber, PropertyID, 2080, totalresult, 1, DateTime.Now);

                    IWebElement multitableElement11 = driver.FindElement(By.XPath("//*[@id='detail-page']/div[4]/div[2]/table/tbody"));
                    IList<IWebElement> multitableRow11 = multitableElement11.FindElements(By.TagName("tr"));
                    IList<IWebElement> multirowTD11;
                    foreach (IWebElement row in multitableRow11)
                    {
                        multirowTD11 = row.FindElements(By.TagName("td"));
                        if (multirowTD11.Count != 0)
                        {
                            ValueDetails1 = multirowTD11[0].Text + "~" + multirowTD11[1].Text + "~" + multirowTD11[2].Text + "~" + multirowTD11[3].Text + "~" + multirowTD11[4].Text + "~" + multirowTD11[5].Text + "~" + multirowTD11[6].Text;
                            gc.insert_date(orderNumber, PropertyID, 843, ValueDetails1, 1, DateTime.Now);

                        }
                    }

                    // Property Roll Value History:
                    //Year~Improvements~Land Market~Ag Valuation~Appraised~HS Cap~Assessed
                    //driver.FindElement(By.Id("rollHistory")).Click();
                    //Thread.Sleep(2000);

                    IWebElement multitableElement2 = driver.FindElement(By.XPath("//*[@id='detail-page']/div[7]/div[2]/table/tbody"));
                    IList<IWebElement> multitableRow2 = multitableElement2.FindElements(By.TagName("tr"));
                    IList<IWebElement> multirowTD2;
                    foreach (IWebElement row in multitableRow2)
                    {
                        multirowTD2 = row.FindElements(By.TagName("td"));
                        if (multirowTD2.Count != 0)
                        {
                            string rollDetails = multirowTD2[0].Text.Trim() + "~" + multirowTD2[1].Text.Trim() + "~" + multirowTD2[2].Text.Trim() + "~" + multirowTD2[3].Text.Trim() + "~" + multirowTD2[4].Text.Trim() + "~" + multirowTD2[5].Text.Trim() + "~" + multirowTD2[6].Text.Trim();
                            gc.insert_date(orderNumber, PropertyID, 842, rollDetails, 1, DateTime.Now);
                        }
                    }
                    //Tax Deed
                    IWebElement DeedHistoryTable = driver.FindElement(By.XPath("//*[@id='detail-page']/div[7]/div[2]/table/tbody"));
                    IList<IWebElement> DeedHistoryRow = DeedHistoryTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> DeedHistoryTD;
                    foreach (IWebElement DeedHistory in DeedHistoryRow)
                    {
                        DeedHistoryTD = DeedHistory.FindElements(By.TagName("td"));
                        if (DeedHistoryTD.Count != 0)
                        {
                            string rollDetails = DeedHistoryTD[0].Text.Trim() + "~" + DeedHistoryTD[1].Text.Trim() + "~" + DeedHistoryTD[2].Text.Trim() + "~" + DeedHistoryTD[3].Text.Trim() + "~" + DeedHistoryTD[4].Text.Trim() + "~" + DeedHistoryTD[5].Text.Trim() + "~" + DeedHistoryTD[6].Text.Trim();
                            gc.insert_date(orderNumber, PropertyID, 2081, rollDetails, 1, DateTime.Now);
                        }
                    }
                    //Tax Due (Payoff Calculation) Details Table:


                    //driver.FindElement(By.Id("taxDue")).Click();
                    //Thread.Sleep(2000);
                    string msgdeli = "", taxdue = "";
                    //int count = driver.FindElements(By.XPath("//*[@id='taxDueDetails_dataSection']/table/tbody/tr")).Count();
                    taxdue = driver.FindElement(By.XPath("//*[@id='taxesDue']/table/tbody")).Text;

                    if (taxdue.Contains("Pay"))
                    {
                        // IWebElement dt = driver.FindElement(By.Id("taxesDueDate"));
                        string date = DateTime.Now.ToString();

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
                        driver.FindElement(By.Id("taxesDueDate")).SendKeys(date);
                        driver.FindElement(By.Id("taxesDueDate")).Click();
                        Thread.Sleep(4000);
                    }
                    //string goodthroughdate = driver.FindElement(By.Id("taxDueDetails_recalculateDate")).Text;
                    //string payoff1 = goodthroughdate + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                    //gc.insert_date(orderNumber, PropertyID, 845, payoff1, 1, DateTime.Now);

                    //string yearnow = driver.FindElement(By.XPath("//*[@id='taxDueDetails_dataSection']/table/tbody/tr[2]/td[1]")).Text;
                    ////Amount Due if Paid on~Year~Taxing Jurisdiction~Taxable Value~Base Tax~Base Taxes Paid~Base Tax Due~Discount / Penalty & Interest~Attorney Fees~Amount Due
                    //IWebElement multitableElement5 = driver.FindElement(By.XPath("//*[@id='taxDueDetails_dataSection']/table/tbody"));
                    //IList<IWebElement> multitableRow5 = multitableElement5.FindElements(By.TagName("tr"));
                    //IList<IWebElement> multirowTD5;
                    //foreach (IWebElement row in multitableRow5)
                    //{
                    //    multirowTD5 = row.FindElements(By.TagName("td"));
                    //    if (multirowTD5.Count != 0)
                    //    {
                    //        if (multirowTD5[0].Text == yearnow)
                    //        {
                    //            if (myList.Any(str => str.Contains(multirowTD5[1].Text)))
                    //            {
                    //                myList.RemoveAt(myList.IndexOf(multirowTD5[1].Text));

                    //            }
                    //        }
                    //        int countlist = myList.Count();
                    //        string payoff11 = "" + "~" + multirowTD5[0].Text.Trim() + "~" + multirowTD5[1].Text.Trim() + "~" + multirowTD5[2].Text.Trim() + "~" + multirowTD5[3].Text.Trim() + "~" + multirowTD5[4].Text.Trim() + "~" + multirowTD5[5].Text.Trim() + "~" + multirowTD5[6].Text.Trim() + "~" + multirowTD5[7].Text.Trim() + "~" + multirowTD5[8].Text.Trim();
                    //        gc.insert_date(orderNumber, PropertyID, 845, payoff11, 1, DateTime.Now);
                    //    }
                    //}
                    //string entity = "";
                    //if (myList.Count() > 0)
                    //{
                    //    for (int i = 0; i < myList.Count(); i++)
                    //    {
                    //        if (i == 0)
                    //        {
                    //            entity = myList[i];
                    //        }
                    //        else
                    //        {
                    //            entity = entity + "&" + myList[i];
                    //        }
                    //    }
                    //    string msged = entity + "~" + "Need to call Tax Office";
                    //    gc.insert_date(orderNumber, PropertyID, 893, msged, 1, DateTime.Now);
                    //}
                    //gc.CreatePdf(orderNumber, PropertyID, "property details", driver, "TX", "Bell");
                    // Taxing Jurisdiction
                    IWebElement multitableElement5 = driver.FindElement(By.XPath("//*[@id='taxesDue']/table/tbody"));
                    IList<IWebElement> multitableRow5 = multitableElement5.FindElements(By.TagName("tr"));
                    IList<IWebElement> multirowTD5;
                    IList<IWebElement> multirowTH5;
                    foreach (IWebElement row in multitableRow5)
                    {
                        multirowTD5 = row.FindElements(By.TagName("td"));
                        multirowTH5 = row.FindElements(By.TagName("th"));
                        if (multirowTD5.Count>1 && !row.Text.Contains("Taxing Jurisdiction")&& multirowTH5.Count!=1)
                        {
                            string Taxresult = multirowTD5[0].Text + "~" + multirowTD5[1].Text + "~" + multirowTD5[2].Text + "~" + multirowTD5[3].Text + "~" + multirowTD5[4].Text + "~" + multirowTD5[5].Text + "~" + multirowTD5[6].Text + "~" + multirowTD5[7].Text + "~" + multirowTD5[8].Text;
                            gc.insert_date(orderNumber, PropertyID, 845, Taxresult, 1, DateTime.Now);
                        }
                        if(multirowTD5.Count!=0 && multirowTH5.Count==1)
                        {
                            string Taxresult = multirowTD5[0].Text + "~" + multirowTH5[0].Text + "~" + multirowTD5[1].Text + "~" + multirowTD5[2].Text + "~" + multirowTD5[3].Text + "~" + multirowTD5[4].Text + "~" + multirowTD5[5].Text + "~" + multirowTD5[6].Text + "~" + multirowTD5[7].Text;
                            gc.insert_date(orderNumber, PropertyID, 845, Taxresult, 1, DateTime.Now);
                        }
                    }

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "TX", "Bell", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    driver.Quit();
                    GlobalClass.titleparcel = "";
                    gc.mergpdf(orderNumber, "TX", "Bell");
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