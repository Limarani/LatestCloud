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
using System.Globalization;
namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_DavidsonTN
    {
        string outputPath = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_DavidsonTN(string sno, string sname, string direction, string sttype, string unino, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "", AssessTakenTime = "", TaxTakentime = "", CityTaxtakentime = "";
            string TotaltakenTime = "";
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            GlobalClass.sname = "TN";
            GlobalClass.cname = "Davidson";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            int payment = 0;
            //PhantomJSDriver
            //driver = new ChromeDriver();
            using (driver = new PhantomJSDriver())
            {
                string outparcel = "", location = "", currentowner = "", mailaddr = "", legaldes = "", taxdist = "", assclass = "", legalref = "";
                string propuse = "", zone = "", neighborhood = "", landarea = "", proptype = "", yearbuilt = "";
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    if (searchType == "titleflex")
                    {
                        string straddress = "";
                        if (direction != "")
                        {
                            straddress = sno + " " + direction + " " + sname + " " + sttype + " " + unino;
                        }
                        else
                        {
                            straddress = sno + " " + sname + " " + sttype + " " + unino;
                        }
                        gc.TitleFlexSearch(orderNumber, "", "", straddress, "TN", "Davidson");
                        if (HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes")
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }

                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";

                        if (parcelNumber.Trim() == "")
                        {
                            HttpContext.Current.Session["Nodata_DavidsonTN"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }

                    }
                    driver.Navigate().GoToUrl("http://www.padctn.org/prc/#/search/1");
                    driver.FindElement(By.Id("AgreeDisclaimer")).Click();
                    Thread.Sleep(2000);
                    if (searchType == "parcel")
                    {
                        IList<IWebElement> searchTypes = driver.FindElements(By.XPath("//*[@id='searchType']/option"));
                        foreach (IWebElement option in searchTypes)
                        {
                            if (option.Text.Trim().Contains("Map and Parcel"))
                            {
                                option.Click();
                            }
                        }
                        parcelNumber = parcelNumber.Replace(" ", "").Replace(".", "").Replace("-", "").Trim();
                        driver.FindElement(By.Id("searchTerm")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search input passed", driver, "TN", "Davidson");
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Results", driver, "TN", "Davidson");

                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='searchType']/option")).Text;
                            if (nodata.Contains("0 results"))
                            {
                                HttpContext.Current.Session["Nodata_DavidsonTN"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "ownername")
                    {
                        IList<IWebElement> searchTypes = driver.FindElements(By.XPath("//*[@id='searchType']/option"));
                        foreach (IWebElement option in searchTypes)
                        {
                            if (option.Text.Trim().Contains("Owner"))
                                option.Click();
                        }
                        driver.FindElement(By.Id("searchTerm")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "Input Passed ownername Search", driver, "TN", "Davidson");
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Ownername Search Results", driver, "TN", "Davidson");

                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("//*[@id='mySearchResults']/div/p")).Text;
                            if (nodata.Contains("0 results"))
                            {
                                HttpContext.Current.Session["Nodata_DavidsonTN"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    string check = driver.FindElement(By.Id("searchResultsFragment")).Text.Trim();
                    if (!check.Contains("returned 0 results"))
                    {
                        string resultNo = driver.FindElement(By.XPath("//*[@id='searchResultsFragment']/div[1]/div/ul/li")).Text.Trim();
                        IList<IWebElement> resultList = driver.FindElements(By.XPath("//*[@id='mySearchResults']/div"));
                        int records = resultList.Count;
                        if (records == 2)
                        {
                            driver.FindElement(By.XPath("//*[@id=\"mySearchResults\"]/div[2]/div[2]/div/a")).Click();
                            Thread.Sleep(3000);
                            //property & assessment details
                            string generaltext = driver.FindElement(By.XPath("//*[@id='propertyOverview']")).Text;
                            outparcel = gc.Between(generaltext, "Map & Parcel:", "Location:").Trim();
                            outparcel = outparcel.Replace(" ", "").Replace(".", "").Replace("-", "").Trim();
                            gc.CreatePdf(orderNumber, outparcel, "Property and Assessment Details", driver, "TN", "Davidson");
                            location = gc.Between(generaltext, "Location:", "Current Owner:").Trim();
                            currentowner = gc.Between(generaltext, "Current Owner:", "Click to Enlarge").Trim();
                            mailaddr = gc.Between(generaltext, "Mailing Address:", "Legal Description:").Trim();
                            legaldes = gc.Between(generaltext, "Legal Description:", "Tax District:").Trim();
                            taxdist = gc.Between(generaltext, "Tax District:", "View Tax Record").Trim();
                            assclass = gc.Between(generaltext, "Assessment Classification*:", "Legal Reference:").Trim();
                            legalref = gc.Between(generaltext, "Legal Reference:", "View Deed").Trim();
                            string assyear = "", lastappyear = "", impvalue = "", landvalue = "", totalappvalue = "", assvalue = "";
                            string assappfulltext = driver.FindElement(By.XPath("//*[@id='content']/div/div[4]/div[1]")).Text;
                            assyear = gc.Between(assappfulltext, "Assessment Year:", "Last Reappraisal Year:").Trim();
                            lastappyear = gc.Between(assappfulltext, "Last Reappraisal Year:", "Improvement Value:").Trim();
                            impvalue = gc.Between(assappfulltext, "Improvement Value:", "Land Value:").Trim();
                            landvalue = gc.Between(assappfulltext, "Land Value:", "Total Appraisal Value:").Trim();
                            totalappvalue = gc.Between(assappfulltext, "Total Appraisal Value:", "Assessed Value:").Trim();
                            assvalue = gc.Between(assappfulltext, "Assessed Value:", "Property Use:").Trim();
                            propuse = gc.Between(assappfulltext, "Property Use:", "Zone:").Trim();
                            zone = gc.Between(assappfulltext, "Zone:", "Neighborhood:").Trim();
                            neighborhood = gc.Between(assappfulltext, "Neighborhood:", "Land Area:").Trim();
                            landarea = GlobalClass.After(assappfulltext, "Land Area:").Trim();
                            string yearbuiltfulltext = driver.FindElement(By.XPath("//*[@id='content']/div/div[4]/div[2]/div")).Text;
                            proptype = gc.Between(yearbuiltfulltext, "Property Type:", "Year Built:").Trim();
                            yearbuilt = gc.Between(yearbuiltfulltext, "Year Built:", "Square Footage:").Trim();
                            //Location~Current Owner~Legal Description~Tax District~Assessment Classification~Legal Reference~Property Use~Zone~Neighborhood~Land Area~Property Type~Year Built
                            string propdetails = location + "~" + currentowner + "~" + mailaddr + "~" + legaldes + "~" + taxdist + "~" + assclass + "~" + legalref + "~" + propuse + "~" + zone + "~" + neighborhood + "~" + landarea + "~" + proptype + "~" + yearbuilt;
                            gc.insert_date(orderNumber, outparcel, 870, propdetails, 1, DateTime.Now);
                            //Assessment Year~Last Reappraisal Year~Improvement Value~Land Value~Total Appraisal Value~Assessed Value
                            string assdetails = assyear + "~" + lastappyear + "~" + impvalue + "~" + landvalue + "~" + totalappvalue + "~" + assvalue;
                            gc.insert_date(orderNumber, outparcel, 871, assdetails, 1, DateTime.Now);
                            //historical data
                            driver.FindElement(By.PartialLinkText("Historical")).Click();
                            Thread.Sleep(2000);
                            string addr = GlobalClass.After(driver.FindElement(By.XPath("//*[@id='content']/div/div[2]/div")).Text, "Location Address:");
                            IWebElement taxsumtableElement1 = driver.FindElement(By.XPath("//*[@id='prev_appeals']/table/tbody"));
                            IList<IWebElement> taxsumtableRow1 = taxsumtableElement1.FindElements(By.TagName("tr"));
                            IList<IWebElement> taxsumrowTD1;
                            foreach (IWebElement row1 in taxsumtableRow1)
                            {
                                taxsumrowTD1 = row1.FindElements(By.TagName("td"));
                                if (taxsumrowTD1.Count != 0 && !row1.Text.Contains("Year"))
                                {
                                    //Location Address~Year~Land Use Code~Building~Yard Items~Land Value~Category~Total
                                    string taxsum = addr + "~" + taxsumrowTD1[0].Text.Trim() + "~" + taxsumrowTD1[1].Text.Trim() + "~" + taxsumrowTD1[2].Text.Trim() + "~" + taxsumrowTD1[3].Text.Trim() + "~" + taxsumrowTD1[4].Text.Trim() + "~" + taxsumrowTD1[5].Text.Trim() + "~" + taxsumrowTD1[6].Text.Trim();
                                    gc.insert_date(orderNumber, outparcel, 872, taxsum, 1, DateTime.Now);
                                }
                            }
                            //  Printable Property
                            driver.FindElement(By.PartialLinkText("Printable Property")).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, outparcel, " Printable Property Details", driver, "TN", "Davidson");
                            //Improvement Details
                            driver.FindElement(By.PartialLinkText("Improvement Details")).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, outparcel, "Business Improvement Details", driver, "TN", "Davidson");
                        }
                        //else if (records <= 11)
                        //{
                        //    for (int i = 2; i <= 11; i++)
                        //    {
                        //        string divrowtext = driver.FindElement(By.XPath("//*[@id='mySearchResults']/div[" + i + "]")).Text;
                        //        string subdivtext = driver.FindElement(By.XPath("//*[@id='mySearchResults']/div[" + i + "]/div[2]/div")).Text;
                        //        string[] words = subdivtext.Split(' ');
                        //        string mparcel = GlobalClass.Before(subdivtext, ".00");
                        //        mparcel = mparcel + ".00";
                        //        string owner = gc.Between(subdivtext, ".00", "\r\n").Trim();
                        //        string maddr = GlobalClass.After(subdivtext, owner).Replace("\r\n", ",");
                        //        maddr = maddr.Remove(0, 1);
                        //        string finalfulltext = driver.FindElement(By.XPath("//*[@id='mySearchResults']/div[" + i + "]/div[3]")).Text;
                        //        string totappvalue = gc.Between(finalfulltext, "Total Appraised Value:", "Land Size:").Trim();
                        //        string landsize = gc.Between(finalfulltext, "Land Size:", "Land Use:").Trim();
                        //        string landuse = GlobalClass.After(finalfulltext, "Land Use:").Trim();
                        //        string multi = maddr + "~" + owner + "~" + totappvalue + "~" + landsize + "~" + landuse;
                        //        gc.insert_date(orderNumber, mparcel, 876, multi, 1, DateTime.Now);
                        //    }
                        //    driver.Quit();
                        //    HttpContext.Current.Session["multiParcel_TNDavidson"] = "Yes";
                        //    return "MultiParcel";
                        //}
                    }
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    //Tax Information Details
                    driver.Navigate().GoToUrl("https://nashville-tn.mygovonline.com/mod.php?mod=propertytax&mode=public_lookup&_pb=1");
                    Thread.Sleep(1000);
                    string outparcel1 = "";
                    var Select = driver.FindElement(By.Name("selectMenu"));
                    var selectElement = new SelectElement(Select);
                    selectElement.SelectByIndex(4);
                    outparcel = outparcel.Replace(" ", "").Replace(".", "").Replace("-", "").Trim();
                    int lencount = outparcel.Length;
                    if (lencount == 12)
                    {
                        outparcel1 = outparcel + "CO";
                    }
                    else
                    {
                        outparcel1 = outparcel;
                    }
                    driver.FindElement(By.Id("tax_account_id")).SendKeys(outparcel1);
                    gc.CreatePdf(orderNumber, outparcel, "taxsearch", driver, "TN", "Davidson");
                    driver.FindElement(By.Id("submit_btn")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, outparcel, "taxsearchoutput", driver, "TN", "Davidson");
                    try
                    {
                        driver.FindElement(By.LinkText("View Bill")).Click();
                        Thread.Sleep(3000);
                        //
                    }
                    catch { }
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='row_13']/td[6]/a")).Click();
                        Thread.Sleep(3000);
                        ////*[@id="row_13"]/td[6]/a
                    }
                    catch { }
                    try
                    {

                        IWebElement IAddressSearch1 = driver.FindElement(By.LinkText("View Bill"));
                        IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                        js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                        Thread.Sleep(3000);
                    }
                    catch { }

                    gc.CreatePdf(orderNumber, outparcel, "taxinfo", driver, "TN", "Davidson");

                    //Parcel History Details
                    string Year = "", Account = "", Rcpt = "", Balancedue = "", Confirm = "", parcelid = "", Yearpar = "";

                    List<string> billinfo = new List<string>();
                    IWebElement Billsinfo2 = driver.FindElement(By.Id("data"));
                    IList<IWebElement> TRBillsinfo2 = Billsinfo2.FindElements(By.TagName("tr"));
                    IList<IWebElement> Aherftax;
                    foreach (IWebElement row in TRBillsinfo2)
                    {
                        Aherftax = row.FindElements(By.TagName("td"));

                        if (Aherftax.Count == 6 && !row.Text.Contains("Year"))
                        {
                            Year = Aherftax[1].Text;
                            Account = Aherftax[2].Text;
                            Rcpt = Aherftax[3].Text;
                            Balancedue = Aherftax[4].Text;
                            Confirm = Aherftax[5].Text;
                            //string ParcelHistorydetails = Year.Trim() + "~" + Account.Trim() + "~" + Rcpt.Trim() + "~" + Balancedue.Trim() + "~" + Confirm.Trim();
                            //gc.insert_date(orderNumber, ParcelNumber, 1454, ParcelHistorydetails, 1, DateTime.Now);
                        }
                        try
                        {
                            if (Aherftax.Count == 6 && !row.Text.Contains("Year") && billinfo.Count < 3 && Aherftax[2].Text.Trim() != "")
                            {
                                IWebElement value1 = Aherftax[2].FindElement(By.TagName("a"));
                                string addview = value1.GetAttribute("href");
                                billinfo.Add(addview);
                            }
                        }
                        catch { }
                    }
                    //************************************//
                    //Date~Status~Paid By~Amount
                    // payment history table

                    string yearr1 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/h1")).Text;
                    string yearr = gc.Between(yearr1, "(", ")").Trim();
                    IList<IWebElement> tableList = driver.FindElements(By.TagName("table"));// table list of Payment History Table

                    foreach (IWebElement tab in tableList)
                    {
                        if (tab.Text.Contains("Date") && !tab.Text.Contains("Parcel History"))
                        {
                            IList<IWebElement> ITaxRealRowQ = tab.FindElements(By.TagName("tr"));
                            IList<IWebElement> ITaxRealTdQ;
                            foreach (IWebElement ItaxReal in ITaxRealRowQ)
                            {
                                ITaxRealTdQ = ItaxReal.FindElements(By.TagName("td"));
                                if (payment < 3 && ITaxRealTdQ.Count != 0 && ITaxRealTdQ[0].Text.Trim() != "")
                                {
                                    string paymenthistory = yearr.Trim() + "~" + ITaxRealTdQ[1].Text.Trim() + "~" + ITaxRealTdQ[2].Text.Trim() + "~" + ITaxRealTdQ[3].Text.Trim() + "~" + ITaxRealTdQ[4].Text.Trim();
                                    gc.insert_date(orderNumber, outparcel, 1674, paymenthistory, 1, DateTime.Now);
                                    payment++;
                                }
                            }
                        }
                    }
                    //Bill #~Property~Owner~Control Map~Group~Parcel~P/I~S/I~City Code~Appraisal Year~Land Value~Improvement Value~Personal Property Value~Total Property Value~Appraised Property Value~Taxable Property~Assessed Taxable Value~2017 Tax Rate~2017 Tax Levy~Interest~Existing Payments~State Relief Given~County Relief Given~Balance Due~Balance~Tax Authority 
                    string bill = "", property = "", owner1 = "", controlMap = "", group = "", parcel = "", pi = "", si = "", citycode = "", appraisalyear = "", landvalue1 = "", improvementvalue = "", personalpropertyvalue = "", appraisedpropertyvalue = "", taxableproperty = "", assessedtaxable = "", taxrate = "", taxlevy = "", interest = "", existingpayment = "", statereliefgiven = "", countyreleifgiven = "", balancedue = "", balance = "", taxauthority = "", totalproperty = "";
                    foreach (string assessmentclick in billinfo)
                    {
                        driver.Navigate().GoToUrl(assessmentclick);
                        var assesscolumn = ""; var assessvalue = "";

                        //General Information
                        string parceltax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/h1")).Text.Trim();
                        parcelid = GlobalClass.After(parceltax, "#").Trim();
                        Yearpar = gc.Between(parceltax, "(", ")").Trim();
                        string fulltext = driver.FindElement(By.XPath(" //*[@id='content']/div[1]/table[2]/tbody/tr[2]/td[1]/table/tbody")).Text.Replace("\r\n", " ");
                        bill = gc.Between(fulltext, "Bill #", "Property:");
                        property = gc.Between(fulltext, "Property:", "Owner:");
                        owner1 = gc.Between(fulltext, "Owner:", "Mailing Address:");
                        controlMap = gc.Between(fulltext, "Control Map:", "Group:");
                        group = gc.Between(fulltext, "Group:", "Parcel:");
                        parcel = gc.Between(fulltext, "Parcel:", "P/I:");
                        pi = gc.Between(fulltext, "P/I:", "S/I:");
                        si = gc.Between(fulltext, "S/I:", "City Code:");
                        citycode = GlobalClass.After(fulltext, "City Code:");
                        string taxauthoritys = "P. O. Box 196358 Nashville, TN 37219-6358 Phone 615-862-6330 Fax 615-862-6337";

                        string Generalinfo = Yearpar + "~" + bill + "~" + property + "~" + owner1 + "~" + controlMap + "~" + group + "~" + parcel + "~" + pi + "~" + si + "~" + citycode + "~" + taxauthoritys;
                        gc.insert_date(orderNumber, parcelid, 873, Generalinfo, 1, DateTime.Now);
                        //Appraisal Information
                        string fulltext1 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table[2]/tbody/tr[2]/td[2]/table/tbody")).Text.Replace("\r\n", " ");
                        appraisalyear = gc.Between(fulltext1, "Appraisal Year:", "Land Value:");
                        landvalue1 = gc.Between(fulltext1, "Land Value:", "Improvement Value:");
                        improvementvalue = gc.Between(fulltext1, "Improvement Value:", "Personal Property Value:");
                        personalpropertyvalue = gc.Between(fulltext1, "Personal Property Value:", "Total Property Value:");
                        totalproperty = GlobalClass.After(fulltext1, "Total Property Value:");

                        string Appraisalinfo = Yearpar + "~" + appraisalyear + "~" + landvalue1 + "~" + improvementvalue + "~" + personalpropertyvalue + "~" + totalproperty;
                        gc.insert_date(orderNumber, parcelid, 874, Appraisalinfo, 1, DateTime.Now);
                        //Tax Information
                        string Appraisedpropertyvalue = "", Taxableproperty = "", Assessedtaxableval = "", Taxrate = "", Taxlevy = "", Interest = "", Existingpayments = "", Staterelief = "", Countyrelief = "", Balanceduemarch = "", Balancemonth = "";

                        IWebElement asstableElement = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table[2]/tbody/tr[2]/td[3]/table/tbody"));
                        IList<IWebElement> asstableElementRow = asstableElement.FindElements(By.TagName("tr"));
                        IList<IWebElement> asstableElementRowTD;
                        foreach (IWebElement rowid in asstableElementRow)
                        {
                            asstableElementRowTD = rowid.FindElements(By.TagName("td"));
                            if (asstableElementRowTD.Count != 0 && rowid.Text != "" && asstableElementRowTD.Count == 3 && rowid.Text.Contains("Appraised Property Value:"))
                            {

                                Appraisedpropertyvalue = asstableElementRowTD[1].Text;
                            }
                            if (asstableElementRowTD.Count != 0 && rowid.Text != "" && asstableElementRowTD.Count == 3 && rowid.Text.Contains("Taxable Property:"))
                            {

                                Taxableproperty = asstableElementRowTD[1].Text;
                            }
                            if (asstableElementRowTD.Count != 0 && rowid.Text != "" && asstableElementRowTD.Count == 3 && rowid.Text.Contains("Assessed Taxable Value:"))
                            {

                                Assessedtaxableval = asstableElementRowTD[1].Text;
                            }
                            if (asstableElementRowTD.Count != 0 && rowid.Text != "" && asstableElementRowTD.Count == 3 && rowid.Text.Contains("Tax Rate:"))
                            {

                                Taxrate = asstableElementRowTD[1].Text;
                            }
                            if (asstableElementRowTD.Count != 0 && rowid.Text != "" && asstableElementRowTD.Count == 3 && rowid.Text.Contains("Tax Levy:"))
                            {

                                Taxlevy = asstableElementRowTD[1].Text;
                            }
                            if (asstableElementRowTD.Count != 0 && rowid.Text != "" && asstableElementRowTD.Count == 3 && rowid.Text.Contains("Interest:"))
                            {

                                Interest = asstableElementRowTD[1].Text;
                            }
                            if (asstableElementRowTD.Count != 0 && rowid.Text != "" && asstableElementRowTD.Count == 3 && rowid.Text.Contains("Existing Payments:"))
                            {

                                Existingpayments = asstableElementRowTD[1].Text;
                            }
                            if (asstableElementRowTD.Count != 0 && rowid.Text != "" && asstableElementRowTD.Count == 3 && rowid.Text.Contains("State Relief Given:"))
                            {

                                Staterelief = asstableElementRowTD[1].Text;
                            }
                            if (asstableElementRowTD.Count != 0 && rowid.Text != "" && asstableElementRowTD.Count == 3 && rowid.Text.Contains("County Relief Given:"))
                            {

                                Countyrelief = asstableElementRowTD[1].Text;
                            }
                            if (asstableElementRowTD.Count != 0 && rowid.Text != "" && asstableElementRowTD.Count == 3 && rowid.Text.Contains("Balance Due"))
                            {

                                Balanceduemarch = asstableElementRowTD[1].Text;
                            }
                            if (asstableElementRowTD.Count != 0 && rowid.Text != "" && asstableElementRowTD.Count == 3 && rowid.Text.Contains("Balance if paid next month:"))
                            {

                                Balancemonth = asstableElementRowTD[1].Text;
                            }

                        }
                        string Taxinfo = Yearpar + "~" + Appraisedpropertyvalue + "~" + Taxableproperty + "~" + Assessedtaxableval + "~" + Taxrate + "~" + Taxlevy + "~" + Interest + "~" + Existingpayments + "~" + Staterelief + "~" + Countyrelief + "~" + Balanceduemarch + "~" + Balancemonth;
                        gc.insert_date(orderNumber, outparcel, 875, Taxinfo, 1, DateTime.Now);

                        //parcel History pdf

                        try
                        {
                            IWebElement ITaxBill1 = driver.FindElement(By.LinkText("Bill"));
                            string BillTax1 = ITaxBill1.GetAttribute("href");
                            gc.downloadfile(BillTax1, orderNumber, outparcel, "bill" + Yearpar, "TN", "Davidson");
                            Thread.Sleep(2000);
                        }
                        catch { }
                        try
                        {
                            IWebElement ITaxBill2 = driver.FindElement(By.LinkText("Receipt"));
                            string BillTax2 = ITaxBill2.GetAttribute("href");
                            gc.downloadfile(BillTax2, orderNumber, outparcel, "receipt" + Yearpar, "TN", "Davidson");
                            Thread.Sleep(2000);
                        }
                        catch { }
                    }



                    //**********************************Parcel History Pdf Reader Details******************************//
                    int a = 0;
                    IWebElement ITaxBill = driver.FindElement(By.LinkText("Parcel History"));
                    string BillTax = ITaxBill.GetAttribute("href");
                    gc.downloadfile(BillTax, orderNumber, outparcel, "parcelhistory" + Yearpar, "TN", "Davidson");
                    Thread.Sleep(2000);
                    string fileName = "parcelhistory" + Yearpar + ".pdf";
                    string FilePath = gc.filePath(orderNumber, outparcel) + fileName;
                    PdfReader reader;
                    string pdfData;
                    reader = new PdfReader(FilePath);
                    String textFromPage = PdfTextExtractor.GetTextFromPage(reader, 1);
                    System.Diagnostics.Debug.WriteLine("" + textFromPage);
                    List<string> list = new List<string>();
                    List<string> list1 = new List<string>();
                    List<string> listname = new List<string>();
                    string name1 = "";
                    int listcount1 = 0;
                    string pdftext = textFromPage;
                    string tableassess = gc.Between(pdftext, " Due Method Details Payee", "Page 1 / {nb}").Trim();
                    string[] tableArray = tableassess.Split('\n');
                    int count1 = tableArray.Length;
                    string msg = "";
                    for (int i = 0; i < count1; i++)
                    {
                        string a1 = tableArray[i];
                        Match match = Regex.Match(a1, @"\d{4}\-\d{2}\-\d{2}");
                        string date = match.Value;
                        if (a1.Contains("(AP)"))
                        {
                            a1 = a1.Replace(" (AP)", "");
                            msg = "(AP)";
                        }

                        if (a1.Contains("(CM)"))
                        {
                            a1 = a1.Replace(" (CM)", "");
                            msg = "(CM)";
                        }
                        if (date != "")
                        {
                            string a2 = a1.Replace(" ", "~");
                            string[] rowarrayname = a2.Split('~');
                            listname.AddRange(rowarrayname);
                            name1 = gc.Between(a2, listname[1], date);
                            name1 = name1.Replace("~", " ");
                            a1 = a1.Replace(name1, " name1 ");
                        }
                        else
                        {
                            string dname = "";
                            string a2 = a1.Replace(" ", "~");
                            string[] rowarrayname = a2.Split('~');
                            listname.AddRange(rowarrayname);
                            try
                            {
                                if (!listname[1].Any(Char.IsDigit) && listname.Count != 1)
                                {
                                    for (int b = 1; b < listname.Count(); b++)
                                    {
                                        if (listname[b].Any(Char.IsDigit))
                                        {
                                            break;
                                        }

                                        dname = dname + " " + listname[b];
                                    }
                                    name1 = dname;
                                    try
                                    {
                                        a1 = a1.Replace(name1, " name1");
                                    }
                                    catch { }

                                    listcount1 = list1.Count();
                                }
                                else
                                {
                                    for (int b = 2; b < listname.Count(); b++)
                                    {
                                        if (listname[b].Any(Char.IsDigit))
                                        {
                                            break;
                                        }

                                        dname = dname + " " + listname[b];
                                    }
                                    if (dname != "")
                                    {
                                        name1 = dname;
                                    }
                                    else
                                    {
                                        name1 = name1;
                                    }
                                    try
                                    {
                                        a1 = a1.Replace(name1, " name1 ");
                                    }
                                    catch { }
                                }
                            }
                            catch { }
                        }
                        int tdcount = 0;
                        a1 = Regex.Replace(a1, @"\s+", " ");
                        a1 = a1.Replace(" ", "~");
                        string[] rowarray = a1.Split('~');
                        list.AddRange(rowarray);
                        // list1.Clear();
                        tdcount = rowarray.Length;
                        int listcount = list.Count();

                        if (listcount == 1 || listcount == 8)
                        {
                            list1.AddRange(rowarray);
                            listcount1 = list1.Count();
                        }
                        if ((date == "" && listcount1 >= 9))
                        {
                            list1.Insert(3, "n/a");
                            listcount1 = list1.Count();
                            listcount = 0;
                        }
                        if ((date == "" && listcount >= 9))
                        {
                            list.Insert(3, "n/a");
                            listcount = list.Count();
                        }
                        string payee = "";

                        if (listcount >= 10)
                        {
                            int j = 0;
                            int k = listcount - 9;
                            for (int l = 0; l < k; l++)
                            {
                                int m = 9;

                                payee = payee + list[m + l] + " ";
                                m++;
                            }
                            string newrow = list[j] + msg + "~" + list[j + 1] + "~" + name1 + "~" + list[j + 3] + "~" + list[j + 4] + "~" + list[j + 5] + "~" + list[j + 6] + "~" + list[j + 7] + "~" + list[j + 8] + "~" + payee;
                            gc.insert_date(orderNumber, outparcel, 1675, newrow, 1, DateTime.Now);
                            msg = "";
                            list.Clear();
                            list1.Clear();
                        }
                        listname.Clear();
                        if (listcount1 >= 10)
                        {
                            int p = 0;
                            string newrow = list1[p] + "~" + list1[p + 1] + "~" + owner1 + "~" + list1[p + 3] + "~" + list1[p + 4] + "~" + list1[p + 5] + "~" + list1[p + 6] + "~" + list1[p + 7] + "~" + list1[p + 8] + "~" + list1[p + 9];
                            gc.insert_date(orderNumber, outparcel, 1675, newrow, 1, DateTime.Now);
                            list1.Clear();
                            list.Clear();
                        }
                    }
                    a++;
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    //City Tax Details
                    int check1 = 0;
                    string PaidURL = "", UnpaidURL = "", Parcel1 = "", TaxOwner1 = "", DALAddress1 = "", Type = "", Type1 = "", TaxParcelYear1 = "", ReceiptNumber1 = "", Taxablevalue1 = "", Taxespaid11 = "", Taxespaid = "", PaymentDate1 = "";
                    string Parcel = "", TaxOwner = "", DALAddress = "", Types = "", TaxParcelYear = "", ReceiptNumber = "", Taxablevalue = "", Taxespaid1 = "", PaymentDate = "", TaxDueAmount = "";
                    try
                    {
                        if (citycode.Contains("GOODLETTSVILLE"))
                        {
                            //HttpContext.Current.Session["PauldingGA_City"] = "DALLAS";
                            driver.Navigate().GoToUrl("https://www.municipalonlinepayments.com/goodlettsvilletn/tax/search");
                            Thread.Sleep(5000);
                            IWebElement name = driver.FindElement(By.Id("Name"));
                            IList<IWebElement> inpnam = name.FindElements(By.TagName("input"));
                            foreach (IWebElement row1 in inpnam)
                            {
                                row1.SendKeys(owner1.Replace(",", "").Trim());
                                gc.CreatePdf(orderNumber, outparcel, "With out camma Inputpassing", driver, "TN", "Davidson");
                                break;
                            }
                            driver.FindElement(By.XPath("//*[@id='Name']/form/div[2]/button")).Click(); // click search By button
                            Thread.Sleep(2000);
                            check1++;
                            gc.CreatePdf(orderNumber, outparcel, "City TAX Details Enter Before Pdf" + check1, driver, "TN", "Davidson");

                            IWebElement TaxTB4 = driver.FindElement(By.Id("search_results"));
                            IList<IWebElement> TaxTR4 = TaxTB4.FindElements(By.TagName("li"));
                            IList<IWebElement> TaxTD4;
                            foreach (IWebElement Tax in TaxTR4)
                            {
                                TaxTD4 = Tax.FindElements(By.TagName("a"));
                                if (TaxTD4.Count != 0)
                                {
                                    if (Tax.Text.Contains("Paid"))
                                    {
                                        try
                                        {
                                            PaidURL = TaxTD4[0].GetAttribute("href");
                                            TaxTD4[0].Click();
                                            Thread.Sleep(1000);
                                            gc.CreatePdf(orderNumber, outparcel, "Paid Parcel Pdf", driver, "TN", "Davidson");

                                            //Taxinfo Paid Details
                                            //With Comma

                                            IWebElement Taxpaiddetails = driver.FindElement(By.XPath("//*[@id='paidTable']/tbody"));
                                            IList<IWebElement> TRTaxpaiddetails = Taxpaiddetails.FindElements(By.TagName("tr"));
                                            IList<IWebElement> TDTax;
                                            foreach (IWebElement Taxess in TRTaxpaiddetails)
                                            {
                                                TDTax = Taxess.FindElements(By.TagName("td"));

                                                if (TDTax.Count != 0 && TDTax.Count == 6 && !Taxess.Text.Contains("No paid Parcels found.") && Taxess.Text.Trim() != (""))
                                                {
                                                    string[] splittd = TDTax[0].Text.Split('\r');
                                                    string splitcheck = splittd[2].Replace("Address:", "").Trim();
                                                    if (splitcheck.Contains(property.Trim()))
                                                    {
                                                        if (Taxess.Text.Contains("Real Estate"))
                                                        {
                                                            Parcel1 = gc.Between(TDTax[0].Text, "Parcel Number:", "Owner:").Trim();
                                                            TaxOwner1 = gc.Between(TDTax[0].Text, "Owner:", "Address:").Trim();
                                                            DALAddress1 = gc.Between(TDTax[0].Text, "Address:", "Type:").Trim();
                                                            Type1 = GlobalClass.After(TDTax[0].Text, "\r\nType:").Trim();


                                                            TaxParcelYear1 = TDTax[1].Text.Trim();
                                                            ReceiptNumber1 = TDTax[2].Text.Trim();
                                                            Taxablevalue1 = TDTax[4].Text.Trim();
                                                            Taxespaid11 = TDTax[5].Text.Trim();

                                                            Taxespaid1 = GlobalClass.Before(Taxespaid11, "\r\n").Trim();
                                                            PaymentDate1 = GlobalClass.After(Taxespaid11, "\r\n").Trim();
                                                            string cityTaxauthority = "105 S. Main St. Goodlettsville, TN 37072 615.851.2200";

                                                            string Taxinfo_Dallasdetailspaid = Parcel1.Trim() + "~" + TaxOwner1.Trim() + "~" + DALAddress1 + "~" + Type1 + "~" + TaxParcelYear1 + "~" + ReceiptNumber1 + "~" + Taxablevalue1 + "~" + Taxespaid1 + "~" + PaymentDate1 + "~" + cityTaxauthority;
                                                            gc.insert_date(orderNumber, outparcel, 1676, Taxinfo_Dallasdetailspaid, 1, DateTime.Now);


                                                            //pdf download
                                                            try
                                                            {
                                                                string[] splittd1 = TDTax[2].Text.Split('\r');
                                                                string splitcheck1 = splittd1[1].Trim();
                                                                if (splitcheck1.Contains("View Statement"))
                                                                {
                                                                    IWebElement ITaxBill1 = driver.FindElement(By.LinkText("   View Statement"));
                                                                    string BillTax12 = ITaxBill1.GetAttribute("href");
                                                                    gc.downloadfile(BillTax12, orderNumber, outparcel, "namesearch" + TaxParcelYear1, "TN", "Davidson");
                                                                    Thread.Sleep(2000);
                                                                }
                                                            }
                                                            catch { }
                                                        }
                                                    }
                                                }
                                            }
                                            gc.CreatePdf(orderNumber, outparcel, "City TAX Paid Pdf", driver, "TN", "Davidson");

                                        }
                                        catch
                                        {

                                        }
                                    }
                                    if (Tax.Text.Contains("Unpaid"))
                                    {
                                        try
                                        {
                                            UnpaidURL = TaxTD4[0].GetAttribute("href");
                                            TaxTD4[0].Click();

                                            //Tax info Unpaid Details

                                            IWebElement TaxUnpaiddetails = driver.FindElement(By.XPath("//*[@id='unpaid']/form/table/tbody"));
                                            IList<IWebElement> TRTaxunpaiddetails = TaxUnpaiddetails.FindElements(By.TagName("tr"));
                                            IList<IWebElement> TDTaxunpaid;
                                            foreach (IWebElement Taxesss in TRTaxunpaiddetails)
                                            {
                                                TDTaxunpaid = Taxesss.FindElements(By.TagName("td"));
                                                if (TDTaxunpaid.Count != 0 && TDTaxunpaid.Count == 8 && !Taxesss.Text.Contains("No unpaid Parcels found.") && Taxesss.Text.Trim() != (""))
                                                {
                                                    string[] splittd = TDTaxunpaid[0].Text.Split('\r');
                                                    string splitcheck = splittd[2].Replace("Address:", "").Trim();
                                                    if (splitcheck.Contains(property.Trim()))
                                                    {
                                                        if (Taxesss.Text.Contains("Real Estate"))
                                                        {
                                                            Parcel = gc.Between(TDTaxunpaid[0].Text, "Parcel Number:", "Owner:").Trim();
                                                            TaxOwner = gc.Between(TDTaxunpaid[0].Text, "Owner:", "Address:").Trim();
                                                            DALAddress = gc.Between(TDTaxunpaid[0].Text, "Address:", "Type:").Trim();
                                                            Types = GlobalClass.After(TDTaxunpaid[0].Text, "\r\nType:").Trim();

                                                            TaxParcelYear = TDTaxunpaid[1].Text.Trim();
                                                            ReceiptNumber = TDTaxunpaid[2].Text.Trim();
                                                            Taxablevalue = TDTaxunpaid[4].Text.Trim();
                                                            TaxDueAmount = TDTaxunpaid[6].Text.Trim();
                                                            string cityTaxauthority = "105 S. Main St. Goodlettsville, TN 37072 615.851.2200";
                                                            string Taxinfo_DallasdetailsUnpaid = Parcel.Trim() + "~" + TaxOwner.Trim() + "~" + DALAddress + "~" + Types + "~" + TaxParcelYear + "~" + ReceiptNumber + "~" + Taxablevalue + "~" + TaxDueAmount + "~" + cityTaxauthority;
                                                            gc.insert_date(orderNumber, outparcel, 1677, Taxinfo_DallasdetailsUnpaid, 1, DateTime.Now);
                                                            //pdf download
                                                            try
                                                            {
                                                                string[] splittd1 = TDTaxunpaid[2].Text.Split('\r');
                                                                string splitcheck1 = splittd1[1].Trim();
                                                                if (splitcheck1.Contains("View Statement"))
                                                                {
                                                                    IWebElement ITaxBill1 = driver.FindElement(By.LinkText("   View Statement"));
                                                                    string BillTax12 = ITaxBill1.GetAttribute("href");
                                                                    gc.downloadfile(BillTax12, orderNumber, outparcel, "unpaidnamesearch" + TaxParcelYear1, "TN", "Davidson");
                                                                    Thread.Sleep(2000);
                                                                }
                                                            }
                                                            catch { }
                                                        }
                                                    }
                                                }
                                            }
                                            gc.CreatePdf(orderNumber, outparcel, "City TAX UnPaid Pdf", driver, "TN", "Davidson");
                                        }
                                        catch
                                        {

                                        }
                                    }
                                }
                            }


                            //Without comma

                            driver.Navigate().GoToUrl("https://www.municipalonlinepayments.com/goodlettsvilletn/tax/search");
                            Thread.Sleep(5000);
                            IWebElement inpname = driver.FindElement(By.Id("Name"));
                            //name.SendKeys(ownername);
                            IList<IWebElement> inpnam1 = inpname.FindElements(By.TagName("input"));
                            foreach (IWebElement row1 in inpnam1)
                            {
                                row1.SendKeys(owner1);
                                gc.CreatePdf(orderNumber, outparcel, "Name search", driver, "TN", "Davidson");
                                break;
                            }
                            driver.FindElement(By.XPath("//*[@id='Name']/form/div[2]/button")).Click(); // click search By button
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, outparcel, "City TAX Details Enter Before Pdf", driver, "TN", "Davidson");

                            IWebElement TaxTB5 = driver.FindElement(By.Id("search_results"));
                            IList<IWebElement> TaxTR5 = TaxTB5.FindElements(By.TagName("li"));
                            IList<IWebElement> TaxTD5;
                            foreach (IWebElement Tax1 in TaxTR5)
                            {
                                TaxTD5 = Tax1.FindElements(By.TagName("a"));
                                if (TaxTD5.Count != 0)
                                {
                                    if (Tax1.Text.Contains("Paid"))
                                    {
                                        try
                                        {
                                            PaidURL = TaxTD5[0].GetAttribute("href");
                                            TaxTD5[0].Click();
                                            Thread.Sleep(1000);
                                            gc.CreatePdf(orderNumber, outparcel, "Parcel Paid Found", driver, "TN", "Davidson");
                                            //Taxinfo Paid Details                                            
                                            IWebElement Taxpaiddetails = driver.FindElement(By.XPath("//*[@id='paidTable']/tbody"));
                                            IList<IWebElement> TRTaxpaiddetails = Taxpaiddetails.FindElements(By.TagName("tr"));
                                            IList<IWebElement> TDTax;
                                            foreach (IWebElement Taxess in TRTaxpaiddetails)
                                            {
                                                TDTax = Taxess.FindElements(By.TagName("td"));

                                                if (TDTax.Count != 0 && TDTax.Count == 6 && !Taxess.Text.Contains("No paid Parcels found.") && Taxess.Text.Trim() != (""))
                                                {
                                                    string[] splittd = TDTax[0].Text.Split('\r');
                                                    string splitcheck = splittd[2].Replace("Address:", "").Trim();
                                                    if (splitcheck.Contains(property.Trim()))
                                                    {
                                                        if (Taxess.Text.Contains("Real Estate"))
                                                        {
                                                            Parcel1 = gc.Between(TDTax[0].Text, "Parcel Number:", "Owner:").Trim();
                                                            TaxOwner1 = gc.Between(TDTax[0].Text, "Owner:", "Address:").Trim();
                                                            DALAddress1 = gc.Between(TDTax[0].Text, "Address:", "Type:").Trim();
                                                            Type1 = GlobalClass.After(TDTax[0].Text, "\r\nType:").Trim();


                                                            TaxParcelYear1 = TDTax[1].Text.Trim();
                                                            ReceiptNumber1 = TDTax[2].Text.Trim();
                                                            Taxablevalue1 = TDTax[4].Text.Trim();
                                                            Taxespaid11 = TDTax[5].Text.Trim();

                                                            Taxespaid1 = GlobalClass.Before(Taxespaid11, "\r\n").Trim();
                                                            PaymentDate1 = GlobalClass.After(Taxespaid11, "\r\n").Trim();
                                                            string cityTaxauthority = "105 S. Main St. Goodlettsville, TN 37072 615.851.2200";

                                                            string Taxinfo_Dallasdetailspaid = Parcel1.Trim() + "~" + TaxOwner1.Trim() + "~" + DALAddress1 + "~" + Type1 + "~" + TaxParcelYear1 + "~" + ReceiptNumber1 + "~" + Taxablevalue1 + "~" + Taxespaid1 + "~" + PaymentDate1 + "~" + cityTaxauthority;
                                                            gc.insert_date(orderNumber, outparcel, 1676, Taxinfo_Dallasdetailspaid, 1, DateTime.Now);

                                                            //pdf download
                                                            try
                                                            {
                                                                string[] splittd1 = TDTax[2].Text.Split('\r');
                                                                string splitcheck1 = splittd1[1].Trim();
                                                                if (splitcheck1.Contains("View Statement"))
                                                                {
                                                                    IWebElement ITaxBill1 = driver.FindElement(By.LinkText("   View Statement"));
                                                                    string BillTax12 = ITaxBill1.GetAttribute("href");
                                                                    gc.downloadfile(BillTax12, orderNumber, outparcel, "namesearch" + TaxParcelYear1, "TN", "Davidson");
                                                                    Thread.Sleep(2000);
                                                                }
                                                            }
                                                            catch { }
                                                        }
                                                    }
                                                }
                                            }
                                            gc.CreatePdf(orderNumber, outparcel, "City TAX Paid Pdf", driver, "TN", "Davidson");
                                        }
                                        catch
                                        {

                                        }
                                    }
                                    if (Tax1.Text.Contains("Unpaid"))
                                    {
                                        try
                                        {
                                            UnpaidURL = TaxTD5[0].GetAttribute("href");
                                            TaxTD5[0].Click();

                                            //Tax info Unpaid Details

                                            IWebElement TaxUnpaiddetails = driver.FindElement(By.XPath("//*[@id='unpaid']/form/table/tbody"));
                                            IList<IWebElement> TRTaxunpaiddetails = TaxUnpaiddetails.FindElements(By.TagName("tr"));
                                            IList<IWebElement> TDTaxunpaid;
                                            foreach (IWebElement Taxesss in TRTaxunpaiddetails)
                                            {
                                                TDTaxunpaid = Taxesss.FindElements(By.TagName("td"));
                                                if (TDTaxunpaid.Count != 0 && TDTaxunpaid.Count == 8 && !Taxesss.Text.Contains("No unpaid Parcels found.") && Taxesss.Text.Trim() != (""))
                                                {
                                                    if (Taxesss.Text.Contains("Real Estate"))
                                                    {
                                                        Parcel = gc.Between(TDTaxunpaid[0].Text, "Parcel Number:", "Owner:").Trim();
                                                        TaxOwner = gc.Between(TDTaxunpaid[0].Text, "Owner:", "Address:").Trim();
                                                        DALAddress = gc.Between(TDTaxunpaid[0].Text, "Address:", "Type:").Trim();
                                                        Types = GlobalClass.After(TDTaxunpaid[0].Text, "\r\nType:").Trim();
                                                    }
                                                    TaxParcelYear = TDTaxunpaid[1].Text.Trim();
                                                    ReceiptNumber = TDTaxunpaid[2].Text.Trim();
                                                    Taxablevalue = TDTaxunpaid[4].Text.Trim();
                                                    TaxDueAmount = TDTaxunpaid[6].Text.Trim();
                                                    string cityTaxauthority = "105 S. Main St. Goodlettsville, TN 37072 615.851.2200";
                                                    string Taxinfo_DallasdetailsUnpaid = Parcel.Trim() + "~" + TaxOwner.Trim() + "~" + DALAddress + "~" + Types + "~" + TaxParcelYear + "~" + ReceiptNumber + "~" + Taxablevalue + "~" + TaxDueAmount + "~" + cityTaxauthority;
                                                    gc.insert_date(orderNumber, outparcel, 1677, Taxinfo_DallasdetailsUnpaid, 1, DateTime.Now);
                                                }
                                            }
                                            gc.CreatePdf(orderNumber, outparcel, "City TAX UnPaid Pdf", driver, "TN", "Davidson");
                                        }
                                        catch { }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        driver.Quit();
                        GlobalClass.LogError(ex, orderNumber);
                        throw ex;
                    }
                    CitytaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "TN", "Davidson", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    driver.Quit();
                    gc.mergpdf(orderNumber, "TN", "Davidson");
                    return "Data Inserted Successfully";
                }
                catch (Exception ex)
                {
                    driver.Quit();
                    GlobalClass.LogError(ex, orderNumber);
                    throw ex;
                }
            }
        }
    }
}