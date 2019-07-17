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
    public class Webdriver_EscambiaFL
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();
        public string FTP_Escambia(string streetno, string direction, string streetname, string streettype, string unitnumber, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string Parcel_number = "", Tax_Authority = "", Year = "", Adderess = "", YearBuilt = "", taxresult3 = "", Firsthalf = "", Secondhalf = "", fullhalf = "", prepayresult1 = "", prepayresult2 = "", prepayresult3 = "", paiedamt = "";
            string outparcelno = "", taxparcel = "", mailingaddress = "", tax_district = "", siteaddr = "", legal_desc = "", year_built = "", propuse = "", par1 = "", par2 = "";
            string Building_Value = "", LandValue = "", JustValue = "", cctaxyear = "", Cap = "", AssessedValue = "", Exemption = "", TaxableValue = "";
            string strBill = "-", strBalance = "-", strBillDate = "-", strBillPaid = "-";
            string taxowner = "", tax_addr = "", accno = "", alterkey = "", millagecode = "", milagerate = "", ctax_year = "", cpaiddate = "", strbillyear = "", cpaidamount = "", creceipt = "", combinedtaxamount = "", ceffdate = "", grosstax = "", ifpaidby = "", pleasepay = "";
            List<string> strissuecertificate = new List<string>();
            List<string> taxhistorylink = new List<string>();
            List<string> downloadlink = new List<string>();
            List<string> taxhistorylinkinst = new List<string>();
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                //rdp site
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("https://escambiataxcollector.com/contact/");
                    Tax_Authority = driver.FindElement(By.XPath("//*[@id='page-261']/div/p[4]")).Text;
                    gc.CreatePdf_WOP(orderNumber, "Tax authority", driver, "FL", "Escambia");
                }
                catch { }
                try
                {
                    driver.Navigate().GoToUrl("http://www.escpa.org/CAMA/Search.aspx");
                    driver.FindElement(By.Id("ctl00_MasterPlaceHolder_rbBegins")).Click();
                    Thread.Sleep(2000);
                    if (searchType == "titleflex")
                    {
                        if (direction != "")
                        {
                            Adderess = streetno.Trim() + " " + direction.Trim() + " " + streetname.Trim() + " " + streettype.Trim();
                        }
                        else
                        {
                            Adderess = streetno.Trim() + " " + streetname.Trim() + " " + streettype.Trim();
                        }
                        gc.TitleFlexSearch(orderNumber, "", "", Adderess, "FL", "Escambia");
                        
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_EscambiaFL"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        driver.FindElement(By.Id("ctl00_MasterPlaceHolder_rbLike")).Click();
                        if (direction != "")
                        {
                            Adderess = streetno.Trim() + " " + direction.Trim() + " " + streetname.Trim() + " " + streettype.Trim();
                        }
                        else
                        {
                            Adderess = streetno.Trim() + " " + streetname.Trim() + " " + streettype.Trim();
                        }
                        driver.FindElement(By.Id("ctl00_MasterPlaceHolder_txtValue")).SendKeys(Adderess);
                        driver.FindElement(By.Id("ctl00_MasterPlaceHolder_btnSubmit")).Click();
                        Thread.Sleep(2000);
                        try
                        {
                            IWebElement pagenumber = driver.FindElement(By.Id("ctl00_MasterPlaceHolder_lblPageInfo"));
                            string pagenumberres = pagenumber.Text;
                            if (pagenumberres.Contains("Page 1 Of 1"))
                            {
                                multiparcel(orderNumber);
                                driver.Quit();
                                HttpContext.Current.Session["multiparcel_Escambia"] = "Yes";
                                return "MultiParcel";
                            }
                        }
                        catch { }

                    }
                    if (searchType == "parcel")
                    {

                        driver.FindElement(By.Id("ctl00_MasterPlaceHolder_txtValue")).SendKeys(parcelNumber);
                        driver.FindElement(By.Id("ctl00_MasterPlaceHolder_rbstrap")).Click();
                        driver.FindElement(By.Id("ctl00_MasterPlaceHolder_btnSubmit")).Click();
                        Thread.Sleep(3000);
                    }
                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("ctl00_MasterPlaceHolder_rbowner")).Click();
                        Thread.Sleep(3000);
                        driver.FindElement(By.Id("ctl00_MasterPlaceHolder_txtValue")).SendKeys(ownername);
                        driver.FindElement(By.Id("ctl00_MasterPlaceHolder_btnSubmit")).Click();
                        Thread.Sleep(3000);
                        try
                        {
                            IWebElement pagenumber = driver.FindElement(By.Id("ctl00_MasterPlaceHolder_lblPageInfo"));
                            string pagenumberres = pagenumber.Text;
                            if (pagenumberres.Contains("Page 1 Of 1"))
                            {
                                multiparcel(orderNumber);
                                driver.Quit();
                                HttpContext.Current.Session["multiparcel_Escambia"] = "Yes";
                                return "MultiParcel";
                            }
                        }
                        catch { }
                    }

                    if (searchType == "unitnumber")
                    {
                        try
                        {
                            driver.FindElement(By.Id("ctl00_MasterPlaceHolder_txtValue")).SendKeys(unitnumber);
                            driver.FindElement(By.Id("ctl00_MasterPlaceHolder_rbacct")).Click();
                            driver.FindElement(By.Id("ctl00_MasterPlaceHolder_btnSubmit")).Click();
                            Thread.Sleep(3000);
                        }
                        catch { }
                    }
                    try
                    {
                        IWebElement Inodata = driver.FindElement(By.Id("ctl00_MasterPlaceHolder_lblErr"));
                        if(Inodata.Text.Contains("No records met search criteria") || Inodata.Text.Contains("Reference number does not match required format"))
                        {
                            HttpContext.Current.Session["Nodata_EscambiaFL"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    //propetry
                    IWebElement propertytable = driver.FindElement(By.XPath("//*[@id='ctl00_MasterPlaceHolder_GenCell']/table/tbody"));
                    Parcel_number = gc.Between(propertytable.Text, "Reference:", "Account:").Trim();
                    string Account = gc.Between(propertytable.Text, "Account:", "Owners:").Trim();
                    string Owner_Name = gc.Between(propertytable.Text, "Owners:", "Mail:").Trim();
                    string MailingAddress = gc.Between(propertytable.Text, "Mail:", "Situs:").Trim();
                    string PropertyAddress = gc.Between(propertytable.Text, "Situs:", "Use Code:").Trim();
                    string UseCode = gc.Between(propertytable.Text, "Use Code:", "Taxing").Trim();
                    string taxingAuthority = gc.Between(propertytable.Text, "Authority:", "Tax Inquiry:");
                    string Exemption1 = driver.FindElement(By.XPath("//*[@id='ctl00_MasterPlaceHolder_ExemCell']/table/tbody/tr[2]/td")).Text;
                    string LegalDescription = driver.FindElement(By.XPath("//*[@id='ctl00_MasterPlaceHolder_LegalCell']/table/tbody/tr[2]/td")).Text;
                    try
                    {
                        IWebElement yearth = driver.FindElement(By.XPath("//*[@id='ctl00_MasterPlaceHolder_tblBldgs']/tbody/tr[3]/td/table/tbody/tr[1]"));
                        YearBuilt = gc.Between(yearth.Text, "Year Built:", "Effective Year:").Trim();
                    }
                    catch { }
                    string propertyresult = Account + "~" + Owner_Name + "~" + MailingAddress + "~" + PropertyAddress + "~" + UseCode + "~" + taxingAuthority + "~" + Exemption1 + "~" + LegalDescription + "~" + YearBuilt;
                    gc.insert_date(orderNumber, Parcel_number, 979, propertyresult, 1, DateTime.Now);
                    gc.CreatePdf(orderNumber, Parcel_number, "Property detail", driver, "FL", "Escambia");
                    IWebElement assessmenttable = driver.FindElement(By.XPath("//*[@id='ctl00_MasterPlaceHolder_AssessCell']/table/tbody"));
                    IList<IWebElement> assessmentrow = assessmenttable.FindElements(By.TagName("tr"));
                    IList<IWebElement> assessmentid;
                    foreach (IWebElement assment in assessmentrow)
                    {
                        assessmentid = assment.FindElements(By.TagName("td"));
                        if (assessmentid.Count == 5 && !assment.Text.Contains("Assessments") && !assment.Text.Contains("Year"))
                        {
                            string assessmentresult = assessmentid[0].Text + "~" + assessmentid[1].Text + "~" + assessmentid[2].Text + "~" + assessmentid[3].Text + "~" + assessmentid[4].Text;
                            gc.insert_date(orderNumber, Parcel_number, 980, assessmentresult, 1, DateTime.Now);
                        }
                    }
                    //tax
                    driver.Navigate().GoToUrl("https://escambia.county-taxes.com/public");
                    driver.FindElement(By.Name("search_query")).SendKeys(Parcel_number);
                    gc.CreatePdf(orderNumber, Parcel_number, "Tax Search parcel", driver, "FL", "Escambia");
                    driver.FindElement(By.XPath("//*[@id='search-controls']/div/form[1]/div[1]/div/span/button")).Click();
                    Thread.Sleep(2000);
                    IWebElement ITaxSearch = driver.FindElement(By.LinkText("Full bill history"));
                    string strITaxSearch = ITaxSearch.GetAttribute("href");
                    driver.Navigate().GoToUrl(strITaxSearch);
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Full Bill History", driver, "FL", "Escambia");
                    IWebElement IBillHistorytable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table"));
                    IList<IWebElement> IBillHistoryRow = IBillHistorytable.FindElements(By.TagName("tr"));
                    IList<IWebElement> IBillHistoryTD;
                    IList<IWebElement> IBillHistoryTH;
                    int i = 0; int m = 0; int j = 0; int k = 0;
                    foreach (IWebElement bill in IBillHistoryRow)
                    {
                        string billyear = "", inst = "", paidamount = "", receipt = "", effectivedate = "";
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
                                        gc.insert_date(orderNumber, Parcel_number, 994, strTaxHistory2, 1, DateTime.Now);
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
                                    gc.insert_date(orderNumber, Parcel_number, 994, strTaxHistory2, 1, DateTime.Now);
                                }
                                if (IBillHistoryTD[2].Text.Contains("Paid"))
                                {
                                    inst = "Total"; paidamount = IBillHistoryTD[2].Text.Replace("Paid", ""); strBalance = "";
                                    string strTaxHistory = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                    gc.insert_date(orderNumber, Parcel_number, 994, strTaxHistory, 1, DateTime.Now);
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
                                                    string dbill = gc.Between(BillTax, "parcels/", "/bills");
                                                    strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();

                                                    BillDownload(orderNumber, Parcel_number, BillTax, dbill, strbillyear);
                                                }
                                                catch { }
                                                try
                                                {
                                                    IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                                    string BillTax = ITaxBill.GetAttribute("href");
                                                    downloadlink.Add(BillTax);
                                                    string dbill = gc.Between(BillTax, "parcels/", "/bills");
                                                    strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                                    string fname = "Tax Bill" + " " + Parcel_number + " " + strBill;
                                                    // gc.downloadfile(BillTax, orderNumber, Parcel_number, fname, "FL", "Escambia");

                                                    BillDownload(orderNumber, Parcel_number, BillTax, dbill, strbillyear);
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

                                                    string dbill = gc.Between(BillTax, "parcels/", "/bills");
                                                    strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();

                                                    BillDownload(orderNumber, Parcel_number, BillTax, dbill, strbillyear);

                                                }
                                                catch { }
                                                try
                                                {
                                                    IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                                    string BillTax = ITaxBill.GetAttribute("href");
                                                    downloadlink.Add(BillTax);
                                                    //string dbill = gc.Between(BillTax, "bills/", "/print");
                                                    string dbill = gc.Between(BillTax, "parcels/", "/bills");
                                                    strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();

                                                    BillDownload(orderNumber, Parcel_number, BillTax, dbill, strbillyear);

                                                }
                                                catch (Exception ex)
                                                {
                                                }
                                                //taxhistorylink.Add(taxlink);
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
                                                        string dbill = gc.Between(BillTax, "parcels/", "/bills");
                                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();

                                                        BillDownload(orderNumber, Parcel_number, BillTax, dbill, strbillyear);


                                                    }
                                                    catch { }
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                                        strbillyear = GlobalClass.Before(ITaxBillCount.Text, " Installment Bill");
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        downloadlink.Add(BillTax);
                                                        string dbill = gc.Between(BillTax, "parcels/", "/bills");
                                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();

                                                        BillDownload(orderNumber, Parcel_number, BillTax, dbill, strbillyear);
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
                                                        string dbill = gc.Between(BillTax, "parcels/", "/bills");
                                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();

                                                        BillDownload(orderNumber, Parcel_number, BillTax, dbill, strbillyear);


                                                    }
                                                    catch { }
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                                        strbillyear = GlobalClass.Before(ITaxBillCount.Text, " Installment Bill");
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        downloadlink.Add(BillTax);
                                                        string dbill = gc.Between(BillTax, "parcels/", "/bills");
                                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();

                                                        BillDownload(orderNumber, Parcel_number, BillTax, dbill, strbillyear);


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
                                                        string dbill = gc.Between(BillTax, "parcels/", "/bills");
                                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();

                                                        BillDownload(orderNumber, Parcel_number, BillTax, dbill, strbillyear);

                                                    }
                                                    catch { }
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                                        strbillyear = GlobalClass.Before(ITaxBillCount.Text, " Installment Bill");
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        downloadlink.Add(BillTax);
                                                        string dbill = gc.Between(BillTax, "parcels/", "/bills");
                                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();

                                                        BillDownload(orderNumber, Parcel_number, BillTax, dbill, strbillyear);

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
                                            string dbill = gc.Between(BillTax, "parcels/", "/bills");
                                            strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();

                                            BillDownload(orderNumber, Parcel_number, BillTax, dbill, strbillyear);

                                        }
                                        catch { }
                                        try
                                        {
                                            IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                            string BillTax = ITaxBill.GetAttribute("href");
                                            downloadlink.Add(BillTax);
                                            string dbill = gc.Between(BillTax, "parcels/", "/bills");
                                            strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();

                                            BillDownload(orderNumber, Parcel_number, BillTax, dbill, strbillyear);
                                        }
                                        catch
                                        {
                                        }


                                        IWebElement ITaxBillCount = IBillHistoryTD[0].FindElement(By.TagName("a"));
                                        string taxlink = ITaxBillCount.GetAttribute("href");
                                        //taxhistorylink.Add(taxlink);
                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd().Replace("Installment Bill #", "").Replace("Bill", "");
                                        var Splitbill = strBill.Split(' ');
                                        billyear = Splitbill[0]; inst = Splitbill[1];
                                        strBalance = IBillHistoryTD[1].Text.Replace("\r\n", "").TrimStart().TrimEnd();
                                        string strTaxHistory = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                        gc.insert_date(orderNumber, Parcel_number, 994, strTaxHistory, 1, DateTime.Now);

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
                                        gc.insert_date(orderNumber, Parcel_number, 994, strTaxHistory, 1, DateTime.Now);

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
                                    gc.insert_date(orderNumber, Parcel_number, 994, strTaxHistory, 1, DateTime.Now);
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
                                string strTaxHistory = billyear + "~" + inst + "~" + strBalance + "~" + "" + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                gc.insert_date(orderNumber, Parcel_number, 994, strTaxHistory, 1, DateTime.Now);
                            }
                        }

                        i++;
                    }

                    int q = 0;
                    foreach (string URL in taxhistorylink)
                    {

                        try
                        {
                            accno = ""; milagerate = ""; alterkey = ""; taxowner = ""; tax_addr = ""; millagecode = ""; ctax_year = ""; cctaxyear = ""; combinedtaxamount = ""; grosstax = ""; cpaidamount = ""; cpaiddate = ""; ceffdate = ""; creceipt = "";
                            driver.Navigate().GoToUrl(URL);
                            Thread.Sleep(3000);
                            //*[@id="content"]/div[1]/div[8]/div/div[1]                        
                            cctaxyear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[1]/div[2]")).Text.Trim().Replace("Real Estate", "").Replace("Print this bill (PDF)", "").Replace("\r\n", "");
                            gc.CreatePdf(orderNumber, Parcel_number, "Tax Details" + cctaxyear, driver, "FL", "Escambia");

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
                                                gc.insert_date(orderNumber, Parcel_number, 995, valoremtax, 1, DateTime.Now);
                                            }

                                            else if (valoremtablerowTD.Count == 2)
                                            {
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                                gc.insert_date(orderNumber, Parcel_number, 995, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        else if (d == valoremtablerowcount - 1)
                                        {
                                            milagerate = valoremtablerowTD[0].Text;
                                            valoremtax = "Total" + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + valoremtablerowTD[1].Text;
                                            gc.insert_date(orderNumber, Parcel_number, 995, valoremtax, 1, DateTime.Now);
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
                                                gc.insert_date(orderNumber, Parcel_number, 995, valoremtax, 1, DateTime.Now);
                                            }
                                            else if (e == nonvaloremtablerowcount - 1)
                                            {
                                                valoremtax = "Total" + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[0].Text;
                                                gc.insert_date(orderNumber, Parcel_number, 995, valoremtax, 1, DateTime.Now);
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
                                    gc.insert_date(orderNumber, Parcel_number, 997, single, 1, DateTime.Now);
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
                                                    gc.insert_date(orderNumber, Parcel_number, 997, DueDate, 1, DateTime.Now);

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
                            IWebElement ITaxSearchparcell = driver.FindElement(By.LinkText("Parcel details"));
                            string strITaxSearchparcell = ITaxSearchparcell.GetAttribute("href");
                            driver.Navigate().GoToUrl(strITaxSearchparcell);
                            Thread.Sleep(3000);
                            gc.CreatePdf(orderNumber, Parcel_number, "Parcel Details" + cctaxyear, driver, "FL", "Escambia");
                            //Account Number~Alternate Key~Owner Name & Mailing Address~Property Address~Millage Code~Millage rate~Tax Year~Tax Amount~Gross Tax~Paid Amount~Paid Date~Effective Date~Receipt Number
                            if (milagerate.Trim() == "")
                            {
                                milagerate = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[3]/div[2]/dl[2]/dd[4]")).Text;
                            }
                            string curtax = alterkey + "~" + taxowner + "~" + tax_addr + "~" + millagecode + "~" + milagerate + "~" + cctaxyear + "~" + combinedtaxamount + "~" + grosstax + "~" + cpaidamount + "~" + cpaiddate + "~" + ceffdate + "~" + creceipt + "~" + Tax_Authority;
                            gc.insert_date(orderNumber, Parcel_number, 998, curtax, 1, DateTime.Now);
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
                            gc.CreatePdf(orderNumber, Parcel_number, "Tax Details" + cctaxyear, driver, "FL", "Escambia");

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
                                                gc.insert_date(orderNumber, Parcel_number, 995, valoremtax, 1, DateTime.Now);
                                            }

                                            else if (valoremtablerowTD.Count == 2)
                                            {
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                                gc.insert_date(orderNumber, Parcel_number, 995, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        else if (d == valoremtablerowcount - 1)
                                        {
                                            milagerate = valoremtablerowTD[0].Text;
                                            valoremtax = "Total" + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + valoremtablerowTD[1].Text;
                                            gc.insert_date(orderNumber, Parcel_number, 995, valoremtax, 1, DateTime.Now);
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
                                                gc.insert_date(orderNumber, Parcel_number, 995, valoremtax, 1, DateTime.Now);
                                            }
                                            else if (e == nonvaloremtablerowcount - 1)
                                            {
                                                valoremtax = "Total" + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[0].Text;
                                                gc.insert_date(orderNumber, Parcel_number, 995, valoremtax, 1, DateTime.Now);
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
                                    gc.insert_date(orderNumber, Parcel_number, 997, single, 1, DateTime.Now);
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
                                                    gc.insert_date(orderNumber, Parcel_number, 997, DueDate, 1, DateTime.Now);

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
                            IWebElement ITaxSearchparcell = driver.FindElement(By.LinkText("Parcel details"));
                            string strITaxSearchparcell = ITaxSearchparcell.GetAttribute("href");
                            driver.Navigate().GoToUrl(strITaxSearchparcell);
                            Thread.Sleep(3000);
                            gc.CreatePdf(orderNumber, Parcel_number, "Parcel Details" + cctaxyear, driver, "FL", "Escambia");


                            //Account Number~Alternate Key~Owner Name & Mailing Address~Property Address~Millage Code~Millage rate~Tax Year~Tax Amount~Gross Tax~Paid Amount~Paid Date~Effective Date~Receipt Number
                            string curtax = alterkey + "~" + taxowner + "~" + tax_addr + "~" + millagecode + "~" + milagerate + "~" + cctaxyear + "~" + combinedtaxamount + "~" + grosstax + "~" + cpaidamount + "~" + cpaiddate + "~" + ceffdate + "~" + creceipt + "~" + Tax_Authority;
                            gc.insert_date(orderNumber, Parcel_number, 998, curtax, 1, DateTime.Now);
                            q++;

                        }
                        catch
                        { }
                    }


                    //q = 0;
                    foreach (string URL in taxhistorylinkinst)
                    {
                        try
                        {
                            accno = ""; alterkey = ""; taxowner = ""; tax_addr = ""; millagecode = ""; ctax_year = ""; cctaxyear = ""; combinedtaxamount = ""; grosstax = ""; cpaidamount = ""; cpaiddate = ""; ceffdate = ""; creceipt = "";
                            driver.Navigate().GoToUrl(URL);
                            Thread.Sleep(3000);
                            //*[@id="content"]/div[1]/div[8]/div/div[1]                        
                            cctaxyear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[1]/div[2]")).Text.Trim().Replace("Real Estate", "").Replace("Print this bill (PDF)", "").Replace("\r\n", "");
                            gc.CreatePdf(orderNumber, Parcel_number, "Tax Details" + cctaxyear, driver, "FL", "Escambia");

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
                                                gc.insert_date(orderNumber, Parcel_number, 995, valoremtax, 1, DateTime.Now);
                                            }

                                            else if (valoremtablerowTD.Count == 2)
                                            {
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                                gc.insert_date(orderNumber, Parcel_number, 995, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        else if (d == valoremtablerowcount - 1)
                                        {
                                            milagerate = valoremtablerowTD[0].Text;
                                            valoremtax = "Total" + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + valoremtablerowTD[1].Text;
                                            gc.insert_date(orderNumber, Parcel_number, 995, valoremtax, 1, DateTime.Now);
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
                                                gc.insert_date(orderNumber, Parcel_number, 995, valoremtax, 1, DateTime.Now);
                                            }
                                            else if (e == nonvaloremtablerowcount - 1)
                                            {
                                                valoremtax = "Total" + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[0].Text;
                                                gc.insert_date(orderNumber, Parcel_number, 995, valoremtax, 1, DateTime.Now);
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
                                    gc.insert_date(orderNumber, Parcel_number, 997, single, 1, DateTime.Now);
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
                                                    gc.insert_date(orderNumber, Parcel_number, 997, DueDate, 1, DateTime.Now);

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

                            //Parcel Details
                            IWebElement ITaxSearchparcell = driver.FindElement(By.LinkText("Parcel details"));
                            string strITaxSearchparcell = ITaxSearchparcell.GetAttribute("href");
                            driver.Navigate().GoToUrl(strITaxSearchparcell);
                            Thread.Sleep(3000);
                            gc.CreatePdf(orderNumber, Parcel_number, "Parcel Details" + cctaxyear, driver, "FL", "Escambia");


                            //Account Number~Alternate Key~Owner Name & Mailing Address~Property Address~Millage Code~Millage rate~Tax Year~Tax Amount~Gross Tax~Paid Amount~Paid Date~Effective Date~Receipt Number
                            string curtax = alterkey + "~" + taxowner + "~" + tax_addr + "~" + millagecode + "~" + milagerate + "~" + cctaxyear + "~" + combinedtaxamount + "~" + grosstax + "~" + cpaidamount + "~" + cpaiddate + "~" + ceffdate + "~" + creceipt + "~" + Tax_Authority;
                            gc.insert_date(orderNumber, Parcel_number, 998, curtax, 1, DateTime.Now);
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
                            gc.CreatePdf(orderNumber, Parcel_number, "Tax Details" + cctaxyear, driver, "FL", "Escambia");

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
                                                gc.insert_date(orderNumber, Parcel_number, 995, valoremtax, 1, DateTime.Now);
                                            }

                                            else if (valoremtablerowTD.Count == 2)
                                            {
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                                gc.insert_date(orderNumber, Parcel_number, 995, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        else if (d == valoremtablerowcount - 1)
                                        {
                                            milagerate = valoremtablerowTD[0].Text;
                                            valoremtax = "Total" + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + valoremtablerowTD[1].Text;
                                            gc.insert_date(orderNumber, Parcel_number, 995, valoremtax, 1, DateTime.Now);
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
                                                gc.insert_date(orderNumber, Parcel_number, 995, valoremtax, 1, DateTime.Now);
                                            }
                                            else if (e == nonvaloremtablerowcount - 1)
                                            {
                                                valoremtax = "Total" + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[0].Text;
                                                gc.insert_date(orderNumber, Parcel_number, 995, valoremtax, 1, DateTime.Now);
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
                                    gc.insert_date(orderNumber, Parcel_number, 997, single, 1, DateTime.Now);
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
                                                    gc.insert_date(orderNumber, Parcel_number, 997, DueDate, 1, DateTime.Now);

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
                            IWebElement ITaxSearchparcell = driver.FindElement(By.LinkText("Parcel details"));
                            string strITaxSearchparcell = ITaxSearchparcell.GetAttribute("href");
                            driver.Navigate().GoToUrl(strITaxSearchparcell);
                            Thread.Sleep(3000);
                            gc.CreatePdf(orderNumber, Parcel_number, "Parcel Details" + cctaxyear, driver, "FL", "Escambia");


                            //Account Number~Alternate Key~Owner Name & Mailing Address~Property Address~Millage Code~Millage rate~Tax Year~Tax Amount~Gross Tax~Paid Amount~Paid Date~Effective Date~Receipt Number
                            string curtax = alterkey + "~" + taxowner + "~" + tax_addr + "~" + millagecode + "~" + milagerate + "~" + cctaxyear + "~" + combinedtaxamount + "~" + grosstax + "~" + cpaidamount + "~" + cpaiddate + "~" + ceffdate + "~" + creceipt + "~" + Tax_Authority;
                            gc.insert_date(orderNumber, Parcel_number, 998, curtax, 1, DateTime.Now);
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
                            gc.CreatePdf(orderNumber, Parcel_number, "Issue Certificate Details" + issueyear, driver, "FL", "Escambia");
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
                            gc.insert_date(orderNumber, Parcel_number, 999, isscer, 1, DateTime.Now);
                        }
                        catch { }
                    }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Quit();

                    gc.mergpdf(orderNumber, "FL", "Escambia");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "FL", "Escambia", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
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
        public void BillDownload(string orderNumber, string Parcel_number, string BillTax, string dbill, string strbillyear)
        {
            string fileName = "";
            var chromeOptions = new ChromeOptions();
            var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];
            chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
            chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
            chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
            chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
            var chDriver = new ChromeDriver(chromeOptions);
            Array.ForEach(Directory.GetFiles(@downloadDirectory), File.Delete); //Delete all the files in auto pdf
            try
            {
                chDriver.Navigate().GoToUrl(BillTax);
                Thread.Sleep(5000);
                try
                {

                    // fileName = "Escambia-County--Real-Estate-" + dbill + "-" + strbillyear + "-Annual-bill.pdf";
                    fileName = latestfilename();
                    Thread.Sleep(3000);
                    gc.AutoDownloadFile(orderNumber, Parcel_number, "Escambia", "FL", fileName);

                }
                catch { }

                try
                {
                    // fileName = "Escambia-County--Real-Estate-" + dbill + "-" + strbillyear + "-Installment-bill-1.pdf";
                    fileName = latestfilename();
                    Thread.Sleep(3000);
                    gc.AutoDownloadFile(orderNumber, Parcel_number, "Escambia", "FL", fileName);

                }
                catch (Exception ex)
                {

                }
                try
                {
                    //fileName = "Escambia-County--Real-Estate-" + dbill + "-" + strbillyear + "-Installment-bill-2.pdf";
                    fileName = latestfilename();
                    Thread.Sleep(3000);
                    gc.AutoDownloadFile(orderNumber, Parcel_number, "Escambia", "FL", fileName);

                }
                catch { }
                try
                {
                    // fileName = "Escambia-County--Real-Estate-" + dbill + "-" + strbillyear + "-Installment-bill-3.pdf";
                    fileName = latestfilename();
                    Thread.Sleep(3000);
                    gc.AutoDownloadFile(orderNumber, Parcel_number, "Escambia", "FL", fileName);

                }
                catch { }

                try
                {
                    //fileName = "Escambia-County--Real-Estate-" + dbill + "-" + strbillyear + "-Installment-bill-4.pdf";
                    fileName = latestfilename();
                    Thread.Sleep(3000);
                    gc.AutoDownloadFile(orderNumber, Parcel_number, "Escambia", "FL", fileName);

                }
                catch { }

                chDriver.Quit();
            }
            catch (Exception ex)
            {
                chDriver.Quit();
            }
        }
        public string latestfilename()
        {
            var downloadDirectory1 = ConfigurationManager.AppSettings["AutoPdf"];
            var files = new DirectoryInfo(downloadDirectory1).GetFiles("*.*");
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
            return latestfile;
        }
        public void multiparcel(string ordernumber)
        {

            IWebElement multiaddrtableElement = driver.FindElement(By.XPath("//*[@id='ctl00_MasterPlaceHolder_grdv']/tbody"));
            IList<IWebElement> multiaddrtableRow = multiaddrtableElement.FindElements(By.TagName("tr"));
            int multiaddrtableRowcount = multiaddrtableRow.Count;
            IList<IWebElement> multiaddrrowTD;
            int o = 0;
            foreach (IWebElement row in multiaddrtableRow)
            {
                if (0 <= 25 && o < multiaddrtableRowcount && !row.Text.Contains("Account"))
                {
                    multiaddrrowTD = row.FindElements(By.TagName("td"));
                    string multi = multiaddrrowTD[4].Text + "~" + multiaddrrowTD[5].Text;
                    gc.insert_date(ordernumber, multiaddrrowTD[3].Text, 1000, multi, 1, DateTime.Now);
                }
                o++;
            }

        }
    }
}