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
using System.Collections.ObjectModel;

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_FLOsceola
    {
        string Outparcelno = "", outputPath = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "", strbillyear = "";
        //txtstreetno.Text.Trim(),txtdirection.Text.Trim(),txtstreetname.Text.Trim(), txtstreettype.Text.Trim(),txtunitnumber.Text.Trim(),txtparcelno.Text.Trim(), txtownername.Text.Trim(), searchType, txtorderno.Text.Trim(), dierctSearch
        public string FTP_FLOsceola(string address, string parcelNumber, string ownername, string searchType, string orderNumber, string directparcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string outparcelno = "", taxparcel = "", mailingaddress = "", tax_district = "", siteaddr = "", legal_desc = "", year_built = "", propuse = "", par1 = "", par2 = "";
            string Building_Value = "", LandValue = "", JustValue = "", cctaxyear = "", Cap = "", AssessedValue = "", Exemption = "", TaxableValue = "";
            string strBill = "-", strBalance = "-", strBillDate = "-", strBillPaid = "-";
            string taxowner = "", tax_addr = "", accno = "", alterkey = "", millagecode = "", milagerate = "", ctax_year = "", cpaiddate = "", cpaidamount = "", creceipt = "", combinedtaxamount = "", ceffdate = "", grosstax = "", ifpaidby = "", pleasepay = "";
            List<string> strissuecertificate = new List<string>();
            List<string> taxhistorylink = new List<string>();
            List<string> downloadlink = new List<string>();
            List<string> taxhistorylinkinst = new List<string>();
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {

                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "FL", "Osceola");

                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_FLOsceola"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("http://ira.property-appraiser.org/PropertySearch/");
                        driver.FindElement(By.Id("txtAddress")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "InputPassed Address Search", driver, "FL", "Osceola");
                        driver.FindElement(By.Id("btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(10000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search Results", driver, "FL", "Osceola");
                        try
                        {
                            string resulttext = driver.FindElement(By.XPath("//*[@id='search-results']/div[1]/ul/li")).Text.Trim().Replace("Total Records: ", "");
                            int resultcount = Int32.Parse(resulttext);

                            if (resultcount > 1)
                            {
                                multiparcel(orderNumber);
                                driver.Quit();
                                HttpContext.Current.Session["multiparcel_FLOsceola"] = "Yes";
                                return "MultiParcel";
                            }

                        }
                        catch { }

                    }
                    if (searchType == "parcel")
                    {
                        if ((HttpContext.Current.Session["titleparcel"] != null && HttpContext.Current.Session["titleparcel"].ToString() != ""))
                        {
                            parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();

                        }
                        driver.Navigate().GoToUrl("http://ira.property-appraiser.org/PropertySearch/");
                        driver.FindElement(By.Id("txtParcel")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "InputPassed Parcel Search", driver, "FL", "Osceola");
                        driver.FindElement(By.Id("btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Results", driver, "FL", "Osceola");
                        try
                        {
                            string resulttext = driver.FindElement(By.XPath("//*[@id='search-results']/div[1]/ul/li")).Text.Trim().Replace("Total Records: ", "");
                            int resultcount = Int32.Parse(resulttext);

                            if (resultcount > 1)
                            {
                                multiparcel(orderNumber);
                                driver.Quit();
                                HttpContext.Current.Session["multiparcel_FLOsceola"] = "Yes";
                                return "MultiParcel";
                            }
                        }
                        catch { }

                    }

                    if (searchType == "ownername")
                    {
                        driver.Navigate().GoToUrl("http://ira.property-appraiser.org/PropertySearch/");
                        driver.FindElement(By.Id("txtOwnerName")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "InputPassed Ownername Search", driver, "FL", "Osceola");
                        driver.FindElement(By.Id("btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "InputPassed Ownername Search", driver, "FL", "Osceola");
                        try
                        {

                            string resulttext = driver.FindElement(By.XPath("//*[@id='search-results']/div[1]/ul/li")).Text.Trim().Replace("Total Records: ", "");
                            int resultcount = Int32.Parse(resulttext);

                            if (resultcount > 1)
                            {
                                multiparcel(orderNumber);
                                driver.Quit();
                                HttpContext.Current.Session["multiparcel_FLOsceola"] = "Yes";
                                return "MultiParcel";
                            }
                        }
                        catch { }
                    }

                    try
                    {
                        IWebElement Inodata = driver.FindElement(By.Id("search-results"));
                        if (Inodata.Text.Contains("search did not return any results"))
                        {
                            HttpContext.Current.Session["Nodata_FLOsceola"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    //*[@id="search-result-table"]/tbody/tr/td[1]
                    driver.FindElement(By.XPath("//*[@id='search-result-table']/tbody/tr/td[1]")).Click();
                    Thread.Sleep(8000);
                    outparcelno = driver.FindElement(By.XPath("//*[@id='parcel-title']")).Text.Trim().Replace("Parcel: ", "");
                    string bulktext = driver.FindElement(By.XPath("//*[@id='owner-information-table']/tbody")).Text.Trim().Replace("\r\n", " ");
                    ownername = gc.Between(bulktext, "Owner Name", "Mailing Address");
                    mailingaddress = gc.Between(bulktext, "Mailing Address", "Physical Address").Trim();
                    siteaddr = gc.Between(bulktext, "Physical Address", "Description").Trim();
                    propuse = gc.Between(bulktext, "Description", "Tax District").Trim();
                    tax_district = GlobalClass.After(bulktext, "Tax District").Trim();
                    try
                    {
                        string bulkyear_built = driver.FindElement(By.XPath("//*[@id='parcel-result']")).Text.Trim();
                        year_built = gc.Between(bulkyear_built, "Year Built", "Bathrooms");
                        if (year_built.Contains("Year Built"))
                        {
                            year_built = GlobalClass.After(year_built, "Year Built").Trim();
                        }
                    }
                    catch
                    {

                    }

                    try
                    {
                        legal_desc = driver.FindElement(By.XPath("//*[@id='parcel-result']")).Text.Trim();
                        legal_desc = GlobalClass.After(legal_desc, "Legal Description");
                    }
                    catch
                    {

                    }


                    //Owner Name~Property Address~Mailing Address~Property Type~Tax District~Year Built~Legal Description
                    string property = ownername + "~" + siteaddr + "~" + mailingaddress + "~" + propuse + "~" + tax_district + "~" + year_built + "~" + legal_desc;
                    gc.insert_date(orderNumber, outparcelno, 505, property, 1, DateTime.Now);

                    //assessment details                
                    IWebElement valuetableElement = driver.FindElement(By.XPath("//*[@id='parcel-result']/div[3]/div/table/tbody"));
                    IList<IWebElement> valuetableRow = valuetableElement.FindElements(By.TagName("tr"));
                    IList<IWebElement> valuerowTD;
                    List<string> Land = new List<string>();
                    List<string> AGBenefit = new List<string>();
                    List<string> ExtraFeatures = new List<string>();
                    List<string> Buildings = new List<string>();
                    List<string> Appraised = new List<string>();
                    List<string> Assessed = new List<string>();
                    List<string> Exemptions = new List<string>();
                    List<string> Taxable = new List<string>();
                    int i = 0;
                    foreach (IWebElement row in valuetableRow)
                    {
                        valuerowTD = row.FindElements(By.TagName("td"));
                        if (valuerowTD.Count != 0)
                        {
                            if (i == 0)
                            {
                                Land.Add(valuerowTD[0].Text);
                                Land.Add(valuerowTD[1].Text);

                            }
                            if (i == 1)
                            {
                                AGBenefit.Add(valuerowTD[0].Text);
                                AGBenefit.Add(valuerowTD[1].Text);
                            }
                            else if (i == 2)
                            {
                                ExtraFeatures.Add(valuerowTD[0].Text);
                                ExtraFeatures.Add(valuerowTD[1].Text);
                            }
                            else if (i == 3)
                            {
                                Buildings.Add(valuerowTD[0].Text);
                                Buildings.Add(valuerowTD[1].Text);
                            }
                            else if (i == 4)
                            {
                                Appraised.Add(valuerowTD[0].Text);
                                Appraised.Add(valuerowTD[1].Text);
                            }
                            else if (i == 5)
                            {
                                Assessed.Add(valuerowTD[0].Text);
                                Assessed.Add(valuerowTD[1].Text);
                            }
                            else if (i == 6)
                            {
                                Exemptions.Add(valuerowTD[0].Text);
                                Exemptions.Add(valuerowTD[1].Text);
                            }

                            else if (i == 7)
                            {
                                Taxable.Add(valuerowTD[0].Text);
                                Taxable.Add(valuerowTD[1].Text);
                            }
                            i++;
                        }
                    }
                    //Year~Land~AG Benefit~Extra Features~Buildings~Appraised Value~Assessed Value~Exemption Value~Taxable Value
                    gc.CreatePdf(orderNumber, outparcelno, "Assessment Details", driver, "FL", "Osceola");
                    string value1 = "Current Values" + "~" + Land[0] + "~" + AGBenefit[0] + "~" + ExtraFeatures[0] + "~" + Buildings[0] + "~" + Appraised[0] + "~" + Assessed[0] + "~" + Exemptions[0] + "~" + Taxable[0];
                    gc.insert_date(orderNumber, outparcelno, 506, value1, 1, DateTime.Now);
                    string value2 = "Certified Values" + "~" + Land[1] + "~" + AGBenefit[1] + "~" + ExtraFeatures[1] + "~" + Buildings[1] + "~" + Appraised[1] + "~" + Assessed[1] + "~" + Exemptions[1] + "~" + Taxable[1];
                    gc.insert_date(orderNumber, outparcelno, 506, value2, 1, DateTime.Now);
                    //Download
                    //*[@id='trim-notice-2012']/a
                    IWebElement trimdownload = driver.FindElement(By.XPath("//*[@id='trim-notice-2012']")).FindElement(By.TagName("a"));
                    string Trimhref = trimdownload.GetAttribute("href");
                    gc.downloadfile(Trimhref, orderNumber, outparcelno, "Assessment.pdf", "FL", "Osceola");
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    //tax details
                    // outparcelno = "3322-505-0106-000/1";
                    driver.Navigate().GoToUrl("https://osceola.county-taxes.com/public");
                    outparcelno = "R" + outparcelno;
                    driver.FindElement(By.Name("search_query")).SendKeys(outparcelno);
                    gc.CreatePdf(orderNumber, outparcelno, "Tax Search", driver, "FL", "Osceola");
                    //*[@id="search-controls"]/div/form[1]/div[1]/div/span/button
                    driver.FindElement(By.XPath("//*[@id='search-controls']/div/form[1]/div[1]/div/span/button")).SendKeys(Keys.Enter);
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, outparcelno, "Tax Search Results", driver, "FL", "Osceola");

                    //full bill history
                    IWebElement ITaxSearch = driver.FindElement(By.LinkText("Full bill history"));
                    string strITaxSearch = ITaxSearch.GetAttribute("href");
                    driver.Navigate().GoToUrl(strITaxSearch);
                    Thread.Sleep(3000);
                    taxparcel = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[1]")).Text;
                    taxparcel = GlobalClass.After(taxparcel, "Real Estate Account #");
                    gc.CreatePdf(orderNumber, outparcelno, "Full Bill History", driver, "FL", "Osceola");
                    IWebElement IBillHistorytable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table"));
                    IList<IWebElement> IBillHistoryRow = IBillHistorytable.FindElements(By.TagName("tr"));
                    IList<IWebElement> IBillHistoryTD;
                    IList<IWebElement> IBillHistoryTH;
                    int i1 = 0; int m = 0; int j = 0; int k = 0;
                    foreach (IWebElement bill in IBillHistoryRow)
                    {
                        string billyear = "", inst = "", paidamount = "", receipt = "", effectivedate = ""; strBalance = ""; strBillDate = "";
                        IBillHistoryTD = bill.FindElements(By.TagName("td"));
                        IBillHistoryTH = bill.FindElements(By.TagName("th"));
                        if (IBillHistoryTD.Count != 0)
                        {
                            try
                            {
                                if (IBillHistoryTD[0].Text.Contains("Redeemed certificate") || IBillHistoryTD[0].Text.Contains("Issued certificate") || IBillHistoryTD[0].Text.Contains("Tax Deed Application") || IBillHistoryTD[0].Text.Contains("Expired certificate"))
                                {
                                    if (IBillHistoryTD.Count == 5)
                                    {
                                        //billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                        billyear = IBillHistoryTD[0].Text;
                                        strBalance = IBillHistoryTD[1].Text.Replace("\r\n", " ");
                                        strBillDate = IBillHistoryTD[2].Text;
                                        paidamount = IBillHistoryTD[3].Text;
                                        string strTaxHistory2 = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                        gc.insert_date(orderNumber, taxparcel, 508, strTaxHistory2, 1, DateTime.Now);
                                    }
                                }

                                if (IBillHistoryTD.Count == 2)
                                {
                                    strBillDate = IBillHistoryTD[0].Text;
                                    paidamount = IBillHistoryTD[1].Text;
                                    if (strBillDate.Contains("Effective"))
                                    {
                                        strBillDate = strBillDate.Replace("Effective ", " ").Trim();
                                        var splitdate = strBillDate.Split(' ');
                                        strBillDate = splitdate[0].Trim();
                                        effectivedate = splitdate[1];
                                    }
                                    if (paidamount.Contains("Paid"))
                                    {
                                        strBillPaid = IBillHistoryTD[1].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                        paidamount = gc.Between(strBillPaid, "Paid ", "Receipt ");
                                        receipt = GlobalClass.After(strBillPaid, "Receipt #");
                                    }
                                    string strTaxHistory2 = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                    gc.insert_date(orderNumber, taxparcel, 508, strTaxHistory2, 1, DateTime.Now);
                                }
                                if (IBillHistoryTD[2].Text.Contains("Paid"))
                                {
                                    inst = "Total"; paidamount = IBillHistoryTD[2].Text.Replace("Paid", ""); strBalance = "";
                                    string strTaxHistory = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                    gc.insert_date(orderNumber, taxparcel, 508, strTaxHistory, 1, DateTime.Now);
                                }

                                if (IBillHistoryTD[0].Text.Contains("Issued certificate"))
                                {
                                    IWebElement ITaxBillCount = IBillHistoryTD[0].FindElement(By.TagName("a"));
                                    string issuelink = ITaxBillCount.GetAttribute("href");
                                    strissuecertificate.Add(issuelink);
                                }


                                if (bill.Text.Contains("Annual Bill") || bill.Text.Contains("Pay this bill") || bill.Text.Contains("Installment Bill"))
                                {
                                    if (m < 12)
                                    {
                                        IWebElement ITaxBillCount = IBillHistoryTD[0].FindElement(By.TagName("a"));
                                        strbillyear = GlobalClass.Before(ITaxBillCount.Text, " Annual Bill");
                                        string taxlink = ITaxBillCount.GetAttribute("href");

                                        if (bill.Text.Contains("Annual Bill"))
                                        {
                                            if (j < 3)
                                            {
                                                //download
                                                try
                                                {
                                                    IWebElement ITaxBill = IBillHistoryTD[3].FindElement(By.TagName("a"));
                                                    string BillTax = ITaxBill.GetAttribute("href");
                                                    downloadlink.Add(BillTax);

                                                    strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();

                                                    BillDownload(orderNumber, parcelNumber, BillTax, taxparcel, strbillyear);
                                                }
                                                catch { }
                                                try
                                                {
                                                    IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                                    string BillTax = ITaxBill.GetAttribute("href");
                                                    downloadlink.Add(BillTax);
                                                    //string dbill = gc.Between(BillTax, "bills/", "/print");
                                                    strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                                    string fname = "Tax Bill" + " " + outparcelno + " " + strBill;
                                                    //gc.downloadfile(BillTax, orderNumber, taxparcel, fname, "FL", "Osceola");

                                                    BillDownload(orderNumber, parcelNumber, BillTax, taxparcel, strbillyear);
                                                }
                                                catch (Exception ex)
                                                {
                                                }
                                                taxhistorylink.Add(taxlink);

                                            }

                                            if (taxhistorylinkinst.Count != 0 && taxhistorylinkinst.Count < 12 && j < 8 && taxhistorylink.Count < 2)
                                            {
                                                //download 
                                                try
                                                {
                                                    IWebElement ITaxBill = IBillHistoryTD[3].FindElement(By.TagName("a"));
                                                    string BillTax = ITaxBill.GetAttribute("href");
                                                    downloadlink.Add(BillTax);

                                                    strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                                    // Osceola-County-Real-Estate-R232529-348000020100-2017-Annual-bill
                                                    //gc.downloadfile(BillTax, orderNumber, taxparcel, "Osceola-County-Real-Estate-" + taxparcel + "-" + strBill + "-bill", "FL", "Osceola");

                                                    BillDownload(orderNumber, parcelNumber, BillTax, taxparcel, strbillyear);

                                                }
                                                catch { }
                                                try
                                                {
                                                    IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                                    string BillTax = ITaxBill.GetAttribute("href");
                                                    downloadlink.Add(BillTax);
                                                    //string dbill = gc.Between(BillTax, "bills/", "/print");
                                                    strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                                    string fname = "Tax Bill" + " " + outparcelno + " " + strBill;
                                                    //gc.downloadfile(BillTax, orderNumber, taxparcel, fname, "FL", "Osceola");

                                                    // View Bill 

                                                    BillDownload(orderNumber, parcelNumber, BillTax, taxparcel, strbillyear);

                                                }
                                                catch (Exception ex)
                                                {
                                                }
                                                taxhistorylink.Add(taxlink);
                                            }
                                            j++;
                                            k++;
                                            strbillyear = "";
                                        }
                                        else if (bill.Text.Contains("Installment Bill"))
                                        {
                                            if (taxhistorylink.Count == 3)
                                            {
                                                //
                                            }
                                            else if (taxhistorylink.Count == 2)
                                            {
                                                if (j < 4)
                                                {
                                                    //download
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[3].FindElement(By.TagName("a"));
                                                        strbillyear = GlobalClass.Before(ITaxBillCount.Text, " Installment Bill");
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                                        downloadlink.Add(BillTax);
                                                        // Osceola-County-Real-Estate-R232529-348000020100-2017-Annual-bill
                                                        // gc.downloadfile(BillTax, orderNumber, taxparcel, "Osceola-County-Real-Estate-" + taxparcel + "-" + strBill + "-bill", "FL", "Osceola");
                                                        // View Bill

                                                        BillDownload(orderNumber, parcelNumber, BillTax, taxparcel, strbillyear);


                                                    }
                                                    catch { }
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                                        strbillyear = GlobalClass.Before(ITaxBillCount.Text, " Installment Bill");
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        downloadlink.Add(BillTax);
                                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                                        // gc.downloadfile(BillTax, orderNumber, taxparcel, "Osceola-County-Real-Estate-" + taxparcel + " - " + strBill + " -bill", "FL", "Osceola");
                                                        // View Bill 

                                                        BillDownload(orderNumber, parcelNumber, BillTax, taxparcel, strbillyear);


                                                    }
                                                    catch
                                                    {
                                                    }


                                                    IWebElement ITaxBillCount1 = IBillHistoryTD[0].FindElement(By.TagName("a"));
                                                    string taxlink1 = ITaxBillCount1.GetAttribute("href");
                                                    taxhistorylinkinst.Add(taxlink1);
                                                    j++;
                                                }
                                            }
                                            else if (taxhistorylink.Count == 1)
                                            {
                                                if (j < 7)
                                                {
                                                    //download
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[3].FindElement(By.TagName("a"));
                                                        strbillyear = GlobalClass.Before(ITaxBillCount.Text, " Installment Bill");
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        downloadlink.Add(BillTax);
                                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                                        // Osceola-County-Real-Estate-R232529-348000020100-2017-Annual-bill
                                                        //gc.downloadfile(BillTax, orderNumber, taxparcel, "Osceola-County-Real-Estate-" + taxparcel + "-" + billyear + "-Annual-bill", "FL", "Osceola");

                                                        BillDownload(orderNumber, parcelNumber, BillTax, taxparcel, strbillyear);


                                                    }
                                                    catch { }
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                                        strbillyear = GlobalClass.Before(ITaxBillCount.Text, " Installment Bill");
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        downloadlink.Add(BillTax);
                                                        //gc.downloadfile(BillTax, orderNumber, taxparcel, "Osceola-County-Real-Estate-" + taxparcel + " - " + billyear + " -Annual-bill", "FL", "Osceola");

                                                        BillDownload(orderNumber, parcelNumber, BillTax, taxparcel, strbillyear);


                                                    }
                                                    catch
                                                    {
                                                    }


                                                    IWebElement ITaxBillCount1 = IBillHistoryTD[0].FindElement(By.TagName("a"));
                                                    string taxlink1 = ITaxBillCount1.GetAttribute("href");
                                                    taxhistorylinkinst.Add(taxlink1);
                                                    j++;
                                                }
                                            }
                                            else if (taxhistorylink.Count == 0)
                                            {
                                                if (j < 12)
                                                {
                                                    //download
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[3].FindElement(By.TagName("a"));
                                                        strbillyear = GlobalClass.Before(ITaxBillCount.Text, " Installment Bill");
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        downloadlink.Add(BillTax);
                                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                                        // Osceola-County-Real-Estate-R232529-348000020100-2017-Annual-bill
                                                        //gc.downloadfile(BillTax, orderNumber, taxparcel, "Osceola-County-Real-Estate-" + taxparcel + "-" + billyear + "-Annual-bill", "FL", "Osceola");
                                                        // View Bill 
                                                        BillDownload(orderNumber, parcelNumber, BillTax, taxparcel, strbillyear);

                                                    }
                                                    catch { }
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                                        strbillyear = GlobalClass.Before(ITaxBillCount.Text, " Installment Bill");
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        downloadlink.Add(BillTax);
                                                        //gc.downloadfile(BillTax, orderNumber, taxparcel, "Osceola-County-Real-Estate-" + taxparcel + " - " + billyear + " -Annual-bill", "FL", "Osceola");
                                                        // View Bill 
                                                        BillDownload(orderNumber, parcelNumber, BillTax, taxparcel, strbillyear);

                                                    }
                                                    catch
                                                    {
                                                    }

                                                    IWebElement ITaxBillCount1 = IBillHistoryTD[0].FindElement(By.TagName("a"));
                                                    string taxlink1 = ITaxBillCount1.GetAttribute("href");
                                                    taxhistorylinkinst.Add(taxlink1);
                                                    j++;
                                                }
                                            }
                                            strbillyear = "";
                                        }
                                        m++;
                                    }
                                    if (bill.Text.Contains("Pay this bill"))
                                    {
                                        //download
                                        try
                                        {
                                            IWebElement ITaxBill = IBillHistoryTD[3].FindElement(By.TagName("a"));
                                            try
                                            {
                                                strbillyear = GlobalClass.Before(ITaxBill.Text, " Annual Bill");
                                            }
                                            catch { }
                                            try
                                            {
                                                strbillyear = GlobalClass.Before(ITaxBill.Text, " Installment Bill");
                                            }
                                            catch { }
                                            string BillTax = ITaxBill.GetAttribute("href");
                                            strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                            downloadlink.Add(BillTax);
                                            // Osceola-County-Real-Estate-R232529-348000020100-2017-Annual-bill
                                            //gc.downloadfile(BillTax, orderNumber, taxparcel, "Osceola-County-Real-Estate-" + taxparcel + "-" + billyear + "-Annual-bill", "FL", "Osceola");
                                            BillDownload(orderNumber, parcelNumber, BillTax, taxparcel, strbillyear);

                                        }
                                        catch { }
                                        try
                                        {
                                            IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                            string BillTax = ITaxBill.GetAttribute("href");
                                            downloadlink.Add(BillTax);
                                            //    gc.downloadfile(BillTax, orderNumber, taxparcel, "Osceola-County-Real-Estate-" + taxparcel + " - " + billyear + " -Annual-bill", "FL", "Osceola");

                                            BillDownload(orderNumber, parcelNumber, BillTax, taxparcel, strbillyear);
                                        }
                                        catch
                                        {
                                        }


                                        IWebElement ITaxBillCount = IBillHistoryTD[0].FindElement(By.TagName("a"));
                                        string taxlink = ITaxBillCount.GetAttribute("href");
                                        taxhistorylink.Add(taxlink);
                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd().Replace("Installment Bill #", "").Replace("Bill", "");
                                        var Splitbill = strBill.Split(' ');
                                        billyear = Splitbill[0]; inst = Splitbill[1];
                                        strBalance = IBillHistoryTD[1].Text.Replace("\r\n", "").TrimStart().TrimEnd();
                                        string strTaxHistory = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                        gc.insert_date(orderNumber, taxparcel, 508, strTaxHistory, 1, DateTime.Now);

                                        strbillyear = "";
                                    }
                                    else
                                    {

                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd().Replace("Installment Bill #", "").Replace("Bill", "");
                                        var Splitbill1 = strBill.Split(' ');
                                        billyear = Splitbill1[0]; inst = Splitbill1[1];
                                        strBalance = IBillHistoryTD[1].Text.Replace("\r\n", "").TrimStart().TrimEnd();
                                        strBillDate = IBillHistoryTD[2].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                        if (strBillDate.Contains("Effective"))
                                        {
                                            strBillDate = strBillDate.Replace("Effective ", "");
                                            var splitdate = strBillDate.Split(' ');
                                            strBillDate = splitdate[0];
                                            effectivedate = splitdate[1];
                                        }
                                        strBillPaid = IBillHistoryTD[3].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                        paidamount = gc.Between(strBillPaid, "Paid ", "Receipt ");
                                        receipt = GlobalClass.After(strBillPaid, "Receipt #");
                                        string strTaxHistory = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                        gc.insert_date(orderNumber, taxparcel, 508, strTaxHistory, 1, DateTime.Now);

                                    }


                                    // m++;
                                    strbillyear = "";
                                }

                                else
                                {
                                    strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd().Replace("Installment Bill #", "").Replace("Bill", "");
                                    var Splitbill = strBill.Split(' ');
                                    billyear = Splitbill[0]; inst = Splitbill[1];
                                    strBalance = IBillHistoryTD[1].Text.Replace("\r\n", "").TrimStart().TrimEnd();
                                    strBillDate = IBillHistoryTD[2].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                    if (strBillDate.Contains("Effective"))
                                    {
                                        strBillDate = strBillDate.Replace("Effective ", "");
                                        var splitdate = strBillDate.Split(' ');
                                        strBillDate = splitdate[0];
                                        effectivedate = splitdate[1];
                                    }
                                    strBillPaid = IBillHistoryTD[3].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                    paidamount = gc.Between(strBillPaid, "Paid ", "Receipt ");
                                    receipt = GlobalClass.After(strBillPaid, "Receipt #");
                                    //Tax Year~Installment~Total Due~Paid Date~Effective Date~Paid Amount~Receipt Number
                                    string strTaxHistory = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                    gc.insert_date(orderNumber, taxparcel, 508, strTaxHistory, 1, DateTime.Now);
                                }

                            }
                            catch
                            {

                            }


                        }
                        if (IBillHistoryTH.Count != 0)
                        {
                            if (IBillHistoryTH[0].Text.Contains("Total Balance"))
                            {
                                inst = "Total Balance"; strBalance = IBillHistoryTH[1].Text.Replace("\r\n", "").TrimStart().TrimEnd();
                                string strTaxHistory = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                gc.insert_date(orderNumber, taxparcel, 508, strTaxHistory, 1, DateTime.Now);
                            }
                        }

                        i++;
                    }

                    int q = 0;
                    foreach (string URL in taxhistorylink)
                    {
                        try
                        {
                            accno = ""; alterkey = ""; taxowner = ""; tax_addr = ""; millagecode = ""; ctax_year = ""; cctaxyear = ""; combinedtaxamount = ""; grosstax = ""; cpaidamount = ""; cpaiddate = ""; ceffdate = ""; creceipt = "";
                            driver.Navigate().GoToUrl(URL);
                            Thread.Sleep(3000);
                            //*[@id="content"]/div[1]/div[8]/div/div[1]                        
                            cctaxyear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[1]/div[2]")).Text.Trim().Replace("Real Estate", "").Replace("Print this bill (PDF)", "").Replace("\r\n", "");
                            gc.CreatePdf(orderNumber, outparcelno, "Tax Details" + cctaxyear, driver, "FL", "Osceola");

                            if (q == 0)
                            {

                                IWebElement valoremtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[2]"));
                                IList<IWebElement> valoremtableRow = valoremtable.FindElements(By.TagName("tr"));
                                int valoremtablerowcount = valoremtableRow.Count;
                                IList<IWebElement> valoremtablerowTD;
                                int d = 0;
                                foreach (IWebElement rowid in valoremtableRow)
                                {
                                    valoremtablerowTD = rowid.FindElements(By.TagName("td"));
                                    if (valoremtablerowTD.Count != 0 && !rowid.Text.Contains("Taxing authority"))
                                    {
                                        string valoremtax = "";
                                        if (d < valoremtablerowcount - 1)
                                        {
                                            if (valoremtablerowTD.Count == 6)
                                            {
                                                //Taxing Authority~Tax Type~Millage~Assessed~Exemption~Taxable~Taxes
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[1].Text + "~" + valoremtablerowTD[2].Text + "~" + valoremtablerowTD[3].Text + "~" + valoremtablerowTD[4].Text + "~" + valoremtablerowTD[5].Text;
                                                gc.insert_date(orderNumber, taxparcel, 509, valoremtax, 1, DateTime.Now);
                                            }

                                            else if (valoremtablerowTD.Count == 2)
                                            {
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                                gc.insert_date(orderNumber, taxparcel, 509, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        else if (d == valoremtablerowcount - 1)
                                        {
                                            milagerate = valoremtablerowTD[0].Text;
                                            valoremtax = "Total" + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + valoremtablerowTD[1].Text;
                                            gc.insert_date(orderNumber, taxparcel, 509, valoremtax, 1, DateTime.Now);
                                        }
                                    }
                                    d++;
                                }

                                //Non Ad-Valorems
                                try
                                {
                                    //*[@id="content"]/div[1]/div[8]/div/table[3]
                                    IWebElement nonvaloremtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[3]"));
                                    IList<IWebElement> nonvaloremtableRow = nonvaloremtable.FindElements(By.TagName("tr"));
                                    int nonvaloremtablerowcount = nonvaloremtableRow.Count;
                                    IList<IWebElement> nonvaloremtablerowTD;
                                    int e = 0;
                                    foreach (IWebElement rowid in nonvaloremtableRow)
                                    {
                                        nonvaloremtablerowTD = rowid.FindElements(By.TagName("td"));
                                        if (nonvaloremtablerowTD.Count != 0 && !rowid.Text.Contains("Levying authority"))
                                        {
                                            string valoremtax = "";
                                            if (e < nonvaloremtablerowcount - 1)
                                            {
                                                //Taxing Authority~Tax Type~Millage~Assessed~Exemption~Taxable~Taxes
                                                valoremtax = nonvaloremtablerowTD[0].Text + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[1].Text + "~" + nonvaloremtablerowTD[2].Text;
                                                gc.insert_date(orderNumber, taxparcel, 509, valoremtax, 1, DateTime.Now);
                                            }
                                            else if (e == nonvaloremtablerowcount - 1)
                                            {
                                                valoremtax = "Total" + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[0].Text;
                                                gc.insert_date(orderNumber, taxparcel, 509, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        e++;
                                    }
                                }
                                catch { }
                            }

                            string currenttaxbulktext = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/p")).Text;
                            combinedtaxamount = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/p")).Text.Trim().Replace("Combined taxes and assessments: ", "");
                            try
                            {
                                string bulkgrosstax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]/tbody/tr/td[1]")).Text.Trim();
                                if (bulkgrosstax.Contains("Gross"))
                                {

                                    grosstax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]/tbody/tr/td[1]")).Text.Trim().Replace("Gross", "");
                                    //if paid by                 
                                    string ifpaidbulk = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]/tbody/tr/td[3]")).Text;
                                    string[] stringSeparators1 = new string[] { "\r\n" };
                                    string[] lines1 = ifpaidbulk.Split(stringSeparators1, StringSplitOptions.None);
                                    ifpaidby = lines1[0]; pleasepay = lines1[1];
                                    string single = cctaxyear + "~" + ifpaidby + "~" + pleasepay;
                                    gc.insert_date(orderNumber, taxparcel, 510, single, 1, DateTime.Now);
                                }

                                else
                                {
                                    string IfPaidBy = "", PlesePay = "", DueDate = "", itaxyear = "";
                                    try
                                    {
                                        IWebElement multitableElement26 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]"));
                                        IList<IWebElement> multitableRow26 = multitableElement26.FindElements(By.TagName("tr"));
                                        IList<IWebElement> multirowTD26;
                                        foreach (IWebElement row in multitableRow26)
                                        {
                                            multirowTD26 = row.FindElements(By.TagName("td"));
                                            int iRowsCount = multirowTD26.Count;
                                            for (int n = 0; n < iRowsCount; n++)
                                            {
                                                if ((!multirowTD26[n].Text.Contains("Face Amt")) && (!multirowTD26[n].Text.Contains("Certificate")))
                                                {
                                                    IfPaidBy = multirowTD26[n].Text.Replace("\r\n", "~");
                                                    var IfpaySplit = IfPaidBy.Split('~');
                                                    IfPaidBy = IfpaySplit[0];
                                                    PlesePay = IfpaySplit[1];
                                                    DueDate = cctaxyear + "~" + IfPaidBy + "~" + PlesePay;
                                                    gc.insert_date(orderNumber, taxparcel, 510, DueDate, 1, DateTime.Now);

                                                }
                                            }
                                        }

                                    }
                                    catch { }
                                }
                            }
                            catch { }
                            try
                            {

                                string bulktext1 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[5]")).Text.Replace("\r\n", ",");
                                taxowner = gc.Between(bulktext1, "Owner,", "Situs address").Replace(",,", "");
                                tax_addr = gc.Between(bulktext1, "Situs address", "Legal description").Trim().Replace(",", "");
                                //*[@id='content']/div[1]/div[7]/div/table[1]/tbody
                                IWebElement currrtaxtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[1]/tbody"));
                                IList<IWebElement> currrtaxtableRow = currrtaxtable.FindElements(By.TagName("tr"));
                                int currrtaxtabletableRowcount = currrtaxtableRow.Count;
                                IList<IWebElement> currrtaxtableRowTD;
                                int e = 0;
                                foreach (IWebElement rowid in currrtaxtableRow)
                                {
                                    currrtaxtableRowTD = rowid.FindElements(By.TagName("td"));
                                    if (currrtaxtableRowTD.Count != 0 && !rowid.Text.Contains("Account number"))
                                    {
                                        accno = currrtaxtableRowTD[0].Text; alterkey = currrtaxtableRowTD[1].Text; millagecode = currrtaxtableRowTD[3].Text;
                                    }
                                }

                            }
                            catch { }
                            try
                            {

                                string paidbulk = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[3]/div[1]/div")).Text.Trim();
                                cpaiddate = gc.Between(paidbulk, "PAID ", " $").Trim();
                                cpaidamount = gc.Between(paidbulk, "$", "Receipt ").Trim();
                                cpaidamount = "$" + cpaidamount;
                                if (cpaidamount.Contains("Effective"))
                                {
                                    ceffdate = GlobalClass.After(cpaidamount, "Effective").Trim();
                                    cpaidamount = GlobalClass.Before(cpaidamount, "Effective").Trim();
                                }
                                creceipt = GlobalClass.After(paidbulk, "Receipt #");
                            }
                            catch { }

                            ////Parcel Details
                            //IWebElement ITaxSearchparcell = driver.FindElement(By.LinkText("Parcel details"));
                            //string strITaxSearchparcell = ITaxSearchparcell.GetAttribute("href");
                            //driver.Navigate().GoToUrl(strITaxSearchparcell);
                            //Thread.Sleep(3000);
                            //gc.CreatePdf(orderNumber, outparcelno, "Parcel Details" + cctaxyear, driver,  "FL", "Osceola");


                            //Account Number~Alternate Key~Owner Name & Mailing Address~Property Address~Millage Code~Millage rate~Tax Year~Tax Amount~Gross Tax~Paid Amount~Paid Date~Effective Date~Receipt Number
                            string curtax = alterkey + "~" + taxowner + "~" + tax_addr + "~" + millagecode + "~" + milagerate + "~" + cctaxyear + "~" + combinedtaxamount + "~" + grosstax + "~" + cpaidamount + "~" + cpaiddate + "~" + ceffdate + "~" + creceipt;
                            gc.insert_date(orderNumber, taxparcel, 511, curtax, 1, DateTime.Now);
                            q++;
                        }

                        catch
                        {

                        }

                        try
                        {
                            accno = ""; alterkey = ""; taxowner = ""; tax_addr = ""; millagecode = ""; ctax_year = ""; cctaxyear = ""; combinedtaxamount = ""; grosstax = ""; cpaidamount = ""; cpaiddate = ""; ceffdate = ""; creceipt = "";
                            driver.Navigate().GoToUrl(URL);
                            Thread.Sleep(3000);

                            cctaxyear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[1]/div[2]")).Text.Trim().Replace("Real Estate", "").Replace("Print this bill (PDF)", "").Replace("\r\n", "");
                            gc.CreatePdf(orderNumber, outparcelno, "Tax Details" + cctaxyear, driver, "FL", "Osceola");

                            if (q == 0)
                            {

                                IWebElement valoremtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[2]"));
                                IList<IWebElement> valoremtableRow = valoremtable.FindElements(By.TagName("tr"));
                                int valoremtablerowcount = valoremtableRow.Count;
                                IList<IWebElement> valoremtablerowTD;
                                int d = 0;
                                foreach (IWebElement rowid in valoremtableRow)
                                {
                                    valoremtablerowTD = rowid.FindElements(By.TagName("td"));
                                    if (valoremtablerowTD.Count != 0 && !rowid.Text.Contains("Taxing authority"))
                                    {
                                        string valoremtax = "";
                                        if (d < valoremtablerowcount - 1)
                                        {
                                            if (valoremtablerowTD.Count == 6)
                                            {
                                                //Taxing Authority~Tax Type~Millage~Assessed~Exemption~Taxable~Taxes
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[1].Text + "~" + valoremtablerowTD[2].Text + "~" + valoremtablerowTD[3].Text + "~" + valoremtablerowTD[4].Text + "~" + valoremtablerowTD[5].Text;
                                                gc.insert_date(orderNumber, taxparcel, 509, valoremtax, 1, DateTime.Now);
                                            }

                                            else if (valoremtablerowTD.Count == 2)
                                            {
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                                gc.insert_date(orderNumber, taxparcel, 509, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        else if (d == valoremtablerowcount - 1)
                                        {
                                            milagerate = valoremtablerowTD[0].Text;
                                            valoremtax = "Total" + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + valoremtablerowTD[1].Text;
                                            gc.insert_date(orderNumber, taxparcel, 509, valoremtax, 1, DateTime.Now);
                                        }
                                    }
                                    d++;
                                }

                                //Non Ad-Valorems
                                try
                                {
                                    //*[@id="content"]/div[1]/div[8]/div/table[3]
                                    IWebElement nonvaloremtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[3]"));
                                    IList<IWebElement> nonvaloremtableRow = nonvaloremtable.FindElements(By.TagName("tr"));
                                    int nonvaloremtablerowcount = nonvaloremtableRow.Count;
                                    IList<IWebElement> nonvaloremtablerowTD;
                                    int e = 0;
                                    foreach (IWebElement rowid in nonvaloremtableRow)
                                    {
                                        nonvaloremtablerowTD = rowid.FindElements(By.TagName("td"));
                                        if (nonvaloremtablerowTD.Count != 0 && !rowid.Text.Contains("Levying authority"))
                                        {
                                            string valoremtax = "";
                                            if (e < nonvaloremtablerowcount - 1)
                                            {
                                                //Taxing Authority~Tax Type~Millage~Assessed~Exemption~Taxable~Taxes
                                                valoremtax = nonvaloremtablerowTD[0].Text + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[1].Text + "~" + nonvaloremtablerowTD[2].Text;
                                                gc.insert_date(orderNumber, taxparcel, 509, valoremtax, 1, DateTime.Now);
                                            }
                                            else if (e == nonvaloremtablerowcount - 1)
                                            {
                                                valoremtax = "Total" + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[0].Text;
                                                gc.insert_date(orderNumber, taxparcel, 509, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        e++;
                                    }
                                }
                                catch { }
                            }

                            string currenttaxbulktext = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/p")).Text;
                            combinedtaxamount = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/p")).Text.Trim().Replace("Combined taxes and assessments: ", "");
                            try
                            {
                                string bulkgrosstax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]/tbody/tr/td[1]")).Text.Trim();
                                if (bulkgrosstax.Contains("Gross"))
                                {

                                    grosstax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]/tbody/tr/td[1]")).Text.Trim().Replace("Gross", "");
                                    //if paid by                 
                                    string ifpaidbulk = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]/tbody/tr/td[3]")).Text;
                                    string[] stringSeparators1 = new string[] { "\r\n" };
                                    string[] lines1 = ifpaidbulk.Split(stringSeparators1, StringSplitOptions.None);
                                    ifpaidby = lines1[0]; pleasepay = lines1[1];
                                    string single = cctaxyear + "~" + ifpaidby + "~" + pleasepay;
                                    gc.insert_date(orderNumber, taxparcel, 510, single, 1, DateTime.Now);
                                }

                                else
                                {
                                    string IfPaidBy = "", PlesePay = "", DueDate = "", itaxyear = "";
                                    try
                                    {
                                        IWebElement multitableElement26 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]"));
                                        IList<IWebElement> multitableRow26 = multitableElement26.FindElements(By.TagName("tr"));
                                        IList<IWebElement> multirowTD26;
                                        foreach (IWebElement row in multitableRow26)
                                        {
                                            multirowTD26 = row.FindElements(By.TagName("td"));
                                            int iRowsCount = multirowTD26.Count;
                                            for (int n = 0; n < iRowsCount; n++)
                                            {
                                                if ((!multirowTD26[n].Text.Contains("Face Amt")) && (!multirowTD26[n].Text.Contains("Certificate")))
                                                {
                                                    IfPaidBy = multirowTD26[n].Text.Replace("\r\n", "~");
                                                    var IfpaySplit = IfPaidBy.Split('~');
                                                    IfPaidBy = IfpaySplit[0];
                                                    PlesePay = IfpaySplit[1];
                                                    DueDate = cctaxyear + "~" + IfPaidBy + "~" + PlesePay;
                                                    gc.insert_date(orderNumber, taxparcel, 510, DueDate, 1, DateTime.Now);

                                                }
                                            }
                                        }

                                    }
                                    catch { }
                                }
                            }
                            catch { }
                            try
                            {

                                string bulktext1 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[5]")).Text.Replace("\r\n", ",");
                                taxowner = gc.Between(bulktext1, "Owner,", "Situs address").Replace(".", "");
                                tax_addr = gc.Between(bulktext1, "Situs address", "Legal description").Trim().Replace(",", "");
                                //*[@id='content']/div[1]/div[7]/div/table[1]/tbody
                                IWebElement currrtaxtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[1]/tbody"));
                                IList<IWebElement> currrtaxtableRow = currrtaxtable.FindElements(By.TagName("tr"));
                                int currrtaxtabletableRowcount = currrtaxtableRow.Count;
                                IList<IWebElement> currrtaxtableRowTD;
                                int e = 0;
                                foreach (IWebElement rowid in currrtaxtableRow)
                                {
                                    currrtaxtableRowTD = rowid.FindElements(By.TagName("td"));
                                    if (currrtaxtableRowTD.Count != 0 && !rowid.Text.Contains("Account number"))
                                    {
                                        accno = currrtaxtableRowTD[0].Text; alterkey = currrtaxtableRowTD[1].Text; millagecode = currrtaxtableRowTD[3].Text;
                                    }
                                }

                            }
                            catch { }
                            try
                            {

                                string paidbulk = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[3]/div[1]/div")).Text.Trim();
                                cpaiddate = gc.Between(paidbulk, "PAID ", " $").Trim();
                                cpaidamount = gc.Between(paidbulk, "$", "Receipt ").Trim();
                                cpaidamount = "$" + cpaidamount;
                                creceipt = GlobalClass.After(paidbulk, "Receipt #");
                            }
                            catch { }

                            ////Parcel Details
                            //IWebElement ITaxSearchparcell = driver.FindElement(By.LinkText("Parcel details"));
                            //string strITaxSearchparcell = ITaxSearchparcell.GetAttribute("href");
                            //driver.Navigate().GoToUrl(strITaxSearchparcell);
                            //Thread.Sleep(3000);
                            //gc.CreatePdf(orderNumber, outparcelno, "Parcel Details" + cctaxyear, driver,  "FL", "Osceola");


                            //Account Number~Alternate Key~Owner Name & Mailing Address~Property Address~Millage Code~Millage rate~Tax Year~Tax Amount~Gross Tax~Paid Amount~Paid Date~Effective Date~Receipt Number
                            string curtax = alterkey + "~" + taxowner + "~" + tax_addr + "~" + millagecode + "~" + milagerate + "~" + cctaxyear + "~" + combinedtaxamount + "~" + grosstax + "~" + cpaidamount + "~" + cpaiddate + "~" + ceffdate + "~" + creceipt;
                            gc.insert_date(orderNumber, taxparcel, 511, curtax, 1, DateTime.Now);
                            q++;
                        }
                        catch
                        { }
                    }


                    q = 0;
                    foreach (string URL in taxhistorylinkinst)
                    {
                        try
                        {
                            accno = ""; alterkey = ""; taxowner = ""; tax_addr = ""; millagecode = ""; ctax_year = ""; cctaxyear = ""; combinedtaxamount = ""; grosstax = ""; cpaidamount = ""; cpaiddate = ""; ceffdate = ""; creceipt = "";
                            driver.Navigate().GoToUrl(URL);
                            Thread.Sleep(3000);
                            //*[@id="content"]/div[1]/div[8]/div/div[1]                        
                            cctaxyear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[1]/div[2]")).Text.Trim().Replace("Real Estate", "").Replace("Print this bill (PDF)", "").Replace("\r\n", "");
                            gc.CreatePdf(orderNumber, outparcelno, "Tax Details" + cctaxyear, driver, "FL", "Osceola");

                            if (q == 0)
                            {

                                IWebElement valoremtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[2]"));
                                IList<IWebElement> valoremtableRow = valoremtable.FindElements(By.TagName("tr"));
                                int valoremtablerowcount = valoremtableRow.Count;
                                IList<IWebElement> valoremtablerowTD;
                                int d = 0;
                                foreach (IWebElement rowid in valoremtableRow)
                                {
                                    valoremtablerowTD = rowid.FindElements(By.TagName("td"));
                                    if (valoremtablerowTD.Count != 0 && !rowid.Text.Contains("Taxing authority"))
                                    {
                                        string valoremtax = "";
                                        if (d < valoremtablerowcount - 1)
                                        {
                                            if (valoremtablerowTD.Count == 6)
                                            {
                                                //Taxing Authority~Tax Type~Millage~Assessed~Exemption~Taxable~Taxes
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[1].Text + "~" + valoremtablerowTD[2].Text + "~" + valoremtablerowTD[3].Text + "~" + valoremtablerowTD[4].Text + "~" + valoremtablerowTD[5].Text;
                                                gc.insert_date(orderNumber, taxparcel, 509, valoremtax, 1, DateTime.Now);
                                            }

                                            else if (valoremtablerowTD.Count == 2)
                                            {
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                                gc.insert_date(orderNumber, taxparcel, 509, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        else if (d == valoremtablerowcount - 1)
                                        {
                                            milagerate = valoremtablerowTD[0].Text;
                                            valoremtax = "Total" + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + valoremtablerowTD[1].Text;
                                            gc.insert_date(orderNumber, taxparcel, 509, valoremtax, 1, DateTime.Now);
                                        }
                                    }
                                    d++;
                                }

                                //Non Ad-Valorems
                                try
                                {
                                    //*[@id="content"]/div[1]/div[8]/div/table[3]
                                    IWebElement nonvaloremtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[3]"));
                                    IList<IWebElement> nonvaloremtableRow = nonvaloremtable.FindElements(By.TagName("tr"));
                                    int nonvaloremtablerowcount = nonvaloremtableRow.Count;
                                    IList<IWebElement> nonvaloremtablerowTD;
                                    int e = 0;
                                    foreach (IWebElement rowid in nonvaloremtableRow)
                                    {
                                        nonvaloremtablerowTD = rowid.FindElements(By.TagName("td"));
                                        if (nonvaloremtablerowTD.Count != 0 && !rowid.Text.Contains("Levying authority"))
                                        {
                                            string valoremtax = "";
                                            if (e < nonvaloremtablerowcount - 1)
                                            {
                                                //Taxing Authority~Tax Type~Millage~Assessed~Exemption~Taxable~Taxes
                                                valoremtax = nonvaloremtablerowTD[0].Text + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[1].Text + "~" + nonvaloremtablerowTD[2].Text;
                                                gc.insert_date(orderNumber, taxparcel, 509, valoremtax, 1, DateTime.Now);
                                            }
                                            else if (e == nonvaloremtablerowcount - 1)
                                            {
                                                valoremtax = "Total" + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[0].Text;
                                                gc.insert_date(orderNumber, taxparcel, 509, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        e++;
                                    }
                                }
                                catch { }
                            }

                            string currenttaxbulktext = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/p")).Text;
                            combinedtaxamount = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/p")).Text.Trim().Replace("Combined taxes and assessments: ", "");
                            try
                            {
                                string bulkgrosstax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]/tbody/tr/td[1]")).Text.Trim();
                                if (bulkgrosstax.Contains("Gross"))
                                {

                                    grosstax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]/tbody/tr/td[1]")).Text.Trim().Replace("Gross", "");
                                    //if paid by                 
                                    string ifpaidbulk = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]/tbody/tr/td[3]")).Text;
                                    string[] stringSeparators1 = new string[] { "\r\n" };
                                    string[] lines1 = ifpaidbulk.Split(stringSeparators1, StringSplitOptions.None);
                                    ifpaidby = lines1[0]; pleasepay = lines1[1];
                                    string single = cctaxyear + "~" + ifpaidby + "~" + pleasepay;
                                    gc.insert_date(orderNumber, taxparcel, 510, single, 1, DateTime.Now);
                                }

                                else
                                {
                                    string IfPaidBy = "", PlesePay = "", DueDate = "", itaxyear = "";
                                    try
                                    {
                                        IWebElement multitableElement26 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]"));
                                        IList<IWebElement> multitableRow26 = multitableElement26.FindElements(By.TagName("tr"));
                                        IList<IWebElement> multirowTD26;
                                        foreach (IWebElement row in multitableRow26)
                                        {
                                            multirowTD26 = row.FindElements(By.TagName("td"));
                                            int iRowsCount = multirowTD26.Count;
                                            for (int n = 0; n < iRowsCount; n++)
                                            {
                                                if ((!multirowTD26[n].Text.Contains("Face Amt")) && (!multirowTD26[n].Text.Contains("Certificate")))
                                                {
                                                    IfPaidBy = multirowTD26[n].Text.Replace("\r\n", "~");
                                                    var IfpaySplit = IfPaidBy.Split('~');
                                                    IfPaidBy = IfpaySplit[0];
                                                    PlesePay = IfpaySplit[1];
                                                    DueDate = cctaxyear + "~" + IfPaidBy + "~" + PlesePay;
                                                    gc.insert_date(orderNumber, taxparcel, 510, DueDate, 1, DateTime.Now);

                                                }
                                            }
                                        }

                                    }
                                    catch { }
                                }
                            }
                            catch { }
                            try
                            {

                                string bulktext1 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[5]")).Text.Replace("\r\n", ",");
                                taxowner = gc.Between(bulktext1, "Owner,", "Situs address").Replace(",,", "");
                                tax_addr = gc.Between(bulktext1, "Situs address", "Legal description").Trim().Replace(",", "");
                                //*[@id='content']/div[1]/div[7]/div/table[1]/tbody
                                IWebElement currrtaxtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[1]/tbody"));
                                IList<IWebElement> currrtaxtableRow = currrtaxtable.FindElements(By.TagName("tr"));
                                int currrtaxtabletableRowcount = currrtaxtableRow.Count;
                                IList<IWebElement> currrtaxtableRowTD;
                                int e = 0;
                                foreach (IWebElement rowid in currrtaxtableRow)
                                {
                                    currrtaxtableRowTD = rowid.FindElements(By.TagName("td"));
                                    if (currrtaxtableRowTD.Count != 0 && !rowid.Text.Contains("Account number"))
                                    {
                                        accno = currrtaxtableRowTD[0].Text; alterkey = currrtaxtableRowTD[1].Text; millagecode = currrtaxtableRowTD[3].Text;
                                    }
                                }

                            }
                            catch { }
                            try
                            {

                                string paidbulk = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[3]/div[1]/div")).Text.Trim();
                                cpaiddate = gc.Between(paidbulk, "PAID ", " $").Trim();
                                cpaidamount = gc.Between(paidbulk, "$", "Receipt ").Trim();
                                cpaidamount = "$" + cpaidamount;
                                if (cpaidamount.Contains("Effective"))
                                {
                                    ceffdate = GlobalClass.After(cpaidamount, "Effective").Trim();
                                    cpaidamount = GlobalClass.Before(cpaidamount, "Effective").Trim();
                                }
                                creceipt = GlobalClass.After(paidbulk, "Receipt #");
                            }
                            catch { }

                            ////Parcel Details
                            //IWebElement ITaxSearchparcell = driver.FindElement(By.LinkText("Parcel details"));
                            //string strITaxSearchparcell = ITaxSearchparcell.GetAttribute("href");
                            //driver.Navigate().GoToUrl(strITaxSearchparcell);
                            //Thread.Sleep(3000);
                            //gc.CreatePdf(orderNumber, outparcelno, "Parcel Details" + cctaxyear, driver,  "FL", "Osceola");


                            //Account Number~Alternate Key~Owner Name & Mailing Address~Property Address~Millage Code~Millage rate~Tax Year~Tax Amount~Gross Tax~Paid Amount~Paid Date~Effective Date~Receipt Number
                            string curtax = alterkey + "~" + taxowner + "~" + tax_addr + "~" + millagecode + "~" + milagerate + "~" + cctaxyear + "~" + combinedtaxamount + "~" + grosstax + "~" + cpaidamount + "~" + cpaiddate + "~" + ceffdate + "~" + creceipt;
                            gc.insert_date(orderNumber, taxparcel, 511, curtax, 1, DateTime.Now);
                            q++;
                        }

                        catch
                        {

                        }

                        try
                        {
                            accno = ""; alterkey = ""; taxowner = ""; tax_addr = ""; millagecode = ""; ctax_year = ""; cctaxyear = ""; combinedtaxamount = ""; grosstax = ""; cpaidamount = ""; cpaiddate = ""; ceffdate = ""; creceipt = "";
                            driver.Navigate().GoToUrl(URL);
                            Thread.Sleep(3000);

                            cctaxyear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[1]/div[2]")).Text.Trim().Replace("Real Estate", "").Replace("Print this bill (PDF)", "").Replace("\r\n", "");
                            gc.CreatePdf(orderNumber, outparcelno, "Tax Details" + cctaxyear, driver, "FL", "Osceola");

                            if (q == 0)
                            {

                                IWebElement valoremtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[2]"));
                                IList<IWebElement> valoremtableRow = valoremtable.FindElements(By.TagName("tr"));
                                int valoremtablerowcount = valoremtableRow.Count;
                                IList<IWebElement> valoremtablerowTD;
                                int d = 0;
                                foreach (IWebElement rowid in valoremtableRow)
                                {
                                    valoremtablerowTD = rowid.FindElements(By.TagName("td"));
                                    if (valoremtablerowTD.Count != 0 && !rowid.Text.Contains("Taxing authority"))
                                    {
                                        string valoremtax = "";
                                        if (d < valoremtablerowcount - 1)
                                        {
                                            if (valoremtablerowTD.Count == 6)
                                            {
                                                //Taxing Authority~Tax Type~Millage~Assessed~Exemption~Taxable~Taxes
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[1].Text + "~" + valoremtablerowTD[2].Text + "~" + valoremtablerowTD[3].Text + "~" + valoremtablerowTD[4].Text + "~" + valoremtablerowTD[5].Text;
                                                gc.insert_date(orderNumber, taxparcel, 509, valoremtax, 1, DateTime.Now);
                                            }

                                            else if (valoremtablerowTD.Count == 2)
                                            {
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                                gc.insert_date(orderNumber, taxparcel, 509, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        else if (d == valoremtablerowcount - 1)
                                        {
                                            milagerate = valoremtablerowTD[0].Text;
                                            valoremtax = "Total" + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + valoremtablerowTD[1].Text;
                                            gc.insert_date(orderNumber, taxparcel, 509, valoremtax, 1, DateTime.Now);
                                        }
                                    }
                                    d++;
                                }

                                //Non Ad-Valorems
                                try
                                {
                                    //*[@id="content"]/div[1]/div[8]/div/table[3]
                                    IWebElement nonvaloremtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[3]"));
                                    IList<IWebElement> nonvaloremtableRow = nonvaloremtable.FindElements(By.TagName("tr"));
                                    int nonvaloremtablerowcount = nonvaloremtableRow.Count;
                                    IList<IWebElement> nonvaloremtablerowTD;
                                    int e = 0;
                                    foreach (IWebElement rowid in nonvaloremtableRow)
                                    {
                                        nonvaloremtablerowTD = rowid.FindElements(By.TagName("td"));
                                        if (nonvaloremtablerowTD.Count != 0 && !rowid.Text.Contains("Levying authority"))
                                        {
                                            string valoremtax = "";
                                            if (e < nonvaloremtablerowcount - 1)
                                            {
                                                //Taxing Authority~Tax Type~Millage~Assessed~Exemption~Taxable~Taxes
                                                valoremtax = nonvaloremtablerowTD[0].Text + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[1].Text + "~" + nonvaloremtablerowTD[2].Text;
                                                gc.insert_date(orderNumber, taxparcel, 509, valoremtax, 1, DateTime.Now);
                                            }
                                            else if (e == nonvaloremtablerowcount - 1)
                                            {
                                                valoremtax = "Total" + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[0].Text;
                                                gc.insert_date(orderNumber, taxparcel, 509, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        e++;
                                    }
                                }
                                catch { }
                            }

                            string currenttaxbulktext = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/p")).Text;
                            combinedtaxamount = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/p")).Text.Trim().Replace("Combined taxes and assessments: ", "");
                            try
                            {
                                string bulkgrosstax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]/tbody/tr/td[1]")).Text.Trim();
                                if (bulkgrosstax.Contains("Gross"))
                                {

                                    grosstax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]/tbody/tr/td[1]")).Text.Trim().Replace("Gross", "");
                                    //if paid by                 
                                    string ifpaidbulk = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]/tbody/tr/td[3]")).Text;
                                    string[] stringSeparators1 = new string[] { "\r\n" };
                                    string[] lines1 = ifpaidbulk.Split(stringSeparators1, StringSplitOptions.None);
                                    ifpaidby = lines1[0]; pleasepay = lines1[1];
                                    string single = cctaxyear + "~" + ifpaidby + "~" + pleasepay;
                                    gc.insert_date(orderNumber, taxparcel, 510, single, 1, DateTime.Now);
                                }

                                else
                                {
                                    string IfPaidBy = "", PlesePay = "", DueDate = "", itaxyear = "";
                                    try
                                    {
                                        IWebElement multitableElement26 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]"));
                                        IList<IWebElement> multitableRow26 = multitableElement26.FindElements(By.TagName("tr"));
                                        IList<IWebElement> multirowTD26;
                                        foreach (IWebElement row in multitableRow26)
                                        {
                                            multirowTD26 = row.FindElements(By.TagName("td"));
                                            int iRowsCount = multirowTD26.Count;
                                            for (int n = 0; n < iRowsCount; n++)
                                            {
                                                if ((!multirowTD26[n].Text.Contains("Face Amt")) && (!multirowTD26[n].Text.Contains("Certificate")))
                                                {
                                                    IfPaidBy = multirowTD26[n].Text.Replace("\r\n", "~");
                                                    var IfpaySplit = IfPaidBy.Split('~');
                                                    IfPaidBy = IfpaySplit[0];
                                                    PlesePay = IfpaySplit[1];
                                                    DueDate = cctaxyear + "~" + IfPaidBy + "~" + PlesePay;
                                                    gc.insert_date(orderNumber, taxparcel, 510, DueDate, 1, DateTime.Now);

                                                }
                                            }
                                        }

                                    }
                                    catch { }
                                }
                            }
                            catch { }
                            try
                            {

                                string bulktext1 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[5]")).Text.Replace("\r\n", ",");
                                taxowner = gc.Between(bulktext1, "Owner,", "Situs address").Replace(".", "");
                                tax_addr = gc.Between(bulktext1, "Situs address", "Legal description").Trim().Replace(",", "");
                                //*[@id='content']/div[1]/div[7]/div/table[1]/tbody
                                IWebElement currrtaxtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[1]/tbody"));
                                IList<IWebElement> currrtaxtableRow = currrtaxtable.FindElements(By.TagName("tr"));
                                int currrtaxtabletableRowcount = currrtaxtableRow.Count;
                                IList<IWebElement> currrtaxtableRowTD;
                                int e = 0;
                                foreach (IWebElement rowid in currrtaxtableRow)
                                {
                                    currrtaxtableRowTD = rowid.FindElements(By.TagName("td"));
                                    if (currrtaxtableRowTD.Count != 0 && !rowid.Text.Contains("Account number"))
                                    {
                                        accno = currrtaxtableRowTD[0].Text; alterkey = currrtaxtableRowTD[1].Text; millagecode = currrtaxtableRowTD[3].Text;
                                    }
                                }

                            }
                            catch { }
                            try
                            {

                                string paidbulk = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[3]/div[1]/div")).Text.Trim();
                                cpaiddate = gc.Between(paidbulk, "PAID ", " $").Trim();
                                cpaidamount = gc.Between(paidbulk, "$", "Receipt ").Trim();
                                cpaidamount = "$" + cpaidamount;
                                creceipt = GlobalClass.After(paidbulk, "Receipt #");
                            }
                            catch { }

                            ////Parcel Details
                            //IWebElement ITaxSearchparcell = driver.FindElement(By.LinkText("Parcel details"));
                            //string strITaxSearchparcell = ITaxSearchparcell.GetAttribute("href");
                            //driver.Navigate().GoToUrl(strITaxSearchparcell);
                            //Thread.Sleep(3000);
                            //gc.CreatePdf(orderNumber, outparcelno, "Parcel Details" + cctaxyear, driver,  "FL", "Osceola");


                            //Account Number~Alternate Key~Owner Name & Mailing Address~Property Address~Millage Code~Millage rate~Tax Year~Tax Amount~Gross Tax~Paid Amount~Paid Date~Effective Date~Receipt Number
                            string curtax = alterkey + "~" + taxowner + "~" + tax_addr + "~" + millagecode + "~" + milagerate + "~" + cctaxyear + "~" + combinedtaxamount + "~" + grosstax + "~" + cpaidamount + "~" + cpaiddate + "~" + ceffdate + "~" + creceipt;
                            gc.insert_date(orderNumber, taxparcel, 511, curtax, 1, DateTime.Now);
                            q++;
                        }
                        catch
                        { }
                    }

                    //issue certificate
                    string adno = "", faceamount = "", issuedate = "", ex_date = "", buyer = "", intrate = "", cer_no = "";
                    foreach (string URL1 in strissuecertificate)
                    {
                        driver.Navigate().GoToUrl(URL1);
                        Thread.Sleep(3000);
                        try
                        {
                            string issueyear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/p")).Text.Trim().Replace("This parcel has an issued certificate for ", "").Replace(".", "");
                            gc.CreatePdf(orderNumber, outparcelno, "Issue Certificate Details" + issueyear, driver, "FL", "Osceola");
                            string issuecertificatebulktext = driver.FindElement(By.XPath("//*[@id='content']/div[1]/dl")).Text.Trim();
                            cer_no = driver.FindElement(By.XPath("//*[@id='certificate']")).Text.Replace("Certificate #", "");
                            adno = gc.Between(issuecertificatebulktext, "Advertised number", "Face amount").Trim();
                            faceamount = gc.Between(issuecertificatebulktext, "Face amount", "Issued date").Trim();
                            issuedate = gc.Between(issuecertificatebulktext, "Issued date", "Expiration date").Trim();
                            ex_date = gc.Between(issuecertificatebulktext, "Expiration date", "Buyer").Trim();
                            buyer = gc.Between(issuecertificatebulktext, "Buyer", "Interest rate").Trim().Replace("\r\n", ",");
                            intrate = GlobalClass.After(issuecertificatebulktext, "Interest rate");
                            //Tax Year~Certificate Number~Advertised Number~Face Amount~Issued Date~Expiration Date~Buyer~Interest Rate
                            string isscer = issueyear + "~" + cer_no + "~" + adno + "~" + faceamount + "~" + issuedate + "~" + ex_date + "~" + buyer + "~" + intrate;
                            gc.insert_date(orderNumber, taxparcel, 512, isscer, 1, DateTime.Now);
                        }
                        catch { }
                    }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Quit();

                    gc.mergpdf(orderNumber, "FL", "Osceola");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "FL", "Osceola", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
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

        public void BillDownload(string orderNumber, string parcelNumber, string BillTax, string taxparcel, string strbillyear)
        {
            string fileName = "";
            var chromeOptions = new ChromeOptions();
            var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];

            chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
            chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
            chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");

            var chDriver = new ChromeDriver(chromeOptions);
            try
            {
                chDriver.Navigate().GoToUrl(BillTax);
                Thread.Sleep(3000);
                try
                {
                    fileName = "Osceola-County-Real-Estate-" + taxparcel + "-" + strbillyear + "-Annual-bill.pdf";
                    gc.AutoDownloadFile(orderNumber, parcelNumber, "FL", "Osceola", fileName);
                }
                catch { }

                try
                {
                    fileName = "Osceola-County-Real-Estate-" + taxparcel + "-" + strbillyear + "-Installment-bill-1.pdf";
                    gc.AutoDownloadFile(orderNumber, parcelNumber, "FL", "Osceola", fileName);
                }
                catch { }
                try
                {
                    fileName = "Osceola-County-Real-Estate-" + taxparcel + "-" + strbillyear + "-Installment-bill-2.pdf";
                    gc.AutoDownloadFile(orderNumber, parcelNumber, "FL", "Osceola", fileName);
                }
                catch { }
                try
                {
                    fileName = "Osceola-County-Real-Estate-" + taxparcel + "-" + strbillyear + "-Installment-bill-3.pdf";
                    gc.AutoDownloadFile(orderNumber, parcelNumber, "FL", "Osceola", fileName);
                }
                catch { }

                try
                {
                    fileName = "Osceola-County-Real-Estate-" + taxparcel + "-" + strbillyear + "-Installment-bill-4.pdf";
                    gc.AutoDownloadFile(orderNumber, parcelNumber, "FL", "Osceola", fileName);
                }
                catch { }

                chDriver.Quit();
            }
            catch (Exception ex)
            {

            }
        }

        public void multiparcel(string ordernumber)
        {

            IWebElement multiaddrtableElement = driver.FindElement(By.XPath("//*[@id='search-result-table']/tbody"));
            IList<IWebElement> multiaddrtableRow = multiaddrtableElement.FindElements(By.TagName("tr"));
            int multiaddrtableRowcount = multiaddrtableRow.Count;
            IList<IWebElement> multiaddrrowTD;
            int o = 0;
            foreach (IWebElement row in multiaddrtableRow)
            {
                if (0 <= 25 && o < multiaddrtableRowcount)
                {
                    multiaddrrowTD = row.FindElements(By.TagName("td"));
                    string multi = multiaddrrowTD[1].Text + "~" + multiaddrrowTD[2].Text;
                    gc.insert_date(ordernumber, multiaddrrowTD[0].Text, 502, multi, 1, DateTime.Now);
                }
                o++;
            }

        }
    }
}



