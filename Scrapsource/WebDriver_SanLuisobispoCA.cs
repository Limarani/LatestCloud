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
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_SanLuisobispoCA
    {
        string Outparcelno = "", outputPath = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        string outparcelno = "", outparcelnowoh = "", city = "", siteaddr = "", year_built = "", assyear = "";
        string imp_Value = "", LandValue = "", AssessedValue = "", personal_property = "", fix_value = "", Exemption = "", netTaxableValue = "";
        string par1 = "", par2 = "", par3 = "", byear; string billnumberwoh = "";
        string taxyear = "", taxtype = "", billnumber = "", comment = "", taxsale = "";
        string taxowner = "", tax_addr = "", linkbillno = "", alterkey = "", millagecode = "", milagerate = "", ctax_year = "", cpaiddate = "", cpaidamount = "", creceipt = "", combinedtaxamount = "", ceffdate = "", grosstax = "", ifpaidby = "", pleasepay = "";
        int a = 0;
        int n = 0;
        int p = 0;
        string FirstInstallment, FirstDueDate, FirstTaxesOutDate, FirstPaid, FirstDue, SecondInstallment, SecondDueDate, SecondTaxesOutDate, SecondPaid, SecondDue = "";
        public string FTP_CAobispo(string streetno, string sname, string unitno, string parcelNumber, string ownername, string searchType, string orderNumber, string directparcel)
        {
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "", AssessTakenTime = "", TaxTakentime = "", CityTaxtakentime = "";
            string TotaltakenTime = ""; string tyear = "";
            GlobalClass.sname = "CA";
            GlobalClass.cname = "San Luis Obispo";
            GlobalClass.global_orderNo = orderNumber; HttpContext.Current.Session["orderNo"] = orderNumber; GlobalClass.global_parcelNo = parcelNumber;
            List<string> option = new List<string>(); List<string> MailURL = new List<string>(); List<string> head = new List<string>();
            List<string> duedate = new List<string>(); List<string> delinqafter = new List<string>(); List<string> taxamount = new List<string>();
            List<string> interest = new List<string>(); List<string> penalty = new List<string>(); List<string> cost = new List<string>();
            List<string> fees = new List<string>(); List<string> total = new List<string>(); List<string> amountpaid = new List<string>();
            List<string> datepaid = new List<string>(); List<string> batchnumber = new List<string>(); List<string> paymenttype = new List<string>();
            List<string> balance = new List<string>();
            Placer pltitle = new Placer();
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //ChromeDriver
            //PhantomJSDriver
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    if (searchType == "titleflex")
                    {
                        string address = streetno + " " + sname + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, "", ownername, address, "CA", "San Luis Obispo");
                        if (HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes")
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        searchType = "parcel";
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();

                    }
                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("http://assessor.slocounty.ca.gov/assessor/pisa/Search.aspx");
                        driver.FindElement(By.Id("Main_txtStreetNumber")).SendKeys(streetno);
                        driver.FindElement(By.Id("Main_txtStreetName")).SendKeys(sname);
                        gc.CreatePdf_WOP(orderNumber, "Input Passed Address Search", driver, "CA", "San Luis Obispo");
                        driver.FindElement(By.Id("Main_btnSearchAddress")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search Results", driver, "CA", "San Luis Obispo");
                        try
                        {
                            IWebElement multiaddrtableElement = driver.FindElement(By.XPath("//*[@id='Main_gvSearchResults']/tbody"));
                            IList<IWebElement> multiaddrtableRow = multiaddrtableElement.FindElements(By.TagName("tr"));
                            int multiaddrtableRowcount = multiaddrtableRow.Count;

                            if (multiaddrtableRowcount > 2)
                            {
                                multiparcel(orderNumber);
                                driver.Quit();
                                return "MultiParcel";
                            }
                        }
                        catch { }
                        //No Records
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("Main_lblMessage")).Text;
                            if (nodata.Contains("Sorry, no records matching your search request could be found. Please change your search criteria and try again. (Search tip: Try entering only the name of the street)"))
                            {
                                HttpContext.Current.Session["Nodata_CASanluisobispo"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    if (searchType == "parcel")
                    {
                        if (HttpContext.Current.Session["titleparcel"] != null && HttpContext.Current.Session["titleparcel"].ToString() != "")
                        {
                            parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        }
                        driver.Navigate().GoToUrl("http://assessor.slocounty.ca.gov/assessor/pisa/Search.aspx");
                        parcelNumber = parcelNumber.Replace("-", "").Replace(",", "").Replace(" ", "");
                        par1 = parcelNumber.Substring(0, 3);
                        par2 = parcelNumber.Substring(3, 3);
                        par3 = parcelNumber.Substring(6, 3);
                        driver.FindElement(By.Id("Main_txtAPN1")).SendKeys(par1);
                        driver.FindElement(By.Id("Main_txtAPN2")).SendKeys(par2);
                        driver.FindElement(By.Id("Main_txtAPN3")).SendKeys(par3);
                        gc.CreatePdf(orderNumber, parcelNumber, "Input Passed Parcel Search", driver, "CA", "San Luis Obispo");
                        driver.FindElement(By.Id("Main_btnAPNSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Result", driver, "CA", "San Luis Obispo");
                    }

                    if (searchType == "ownername")
                    {
                        driver.Navigate().GoToUrl("http://assessor.slocounty.ca.gov/assessor/pisa/Search.aspx");
                        driver.FindElement(By.Id("Main_txtOwnerName")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "Input Passed Ownername Search", driver, "CA", "San Luis Obispo");
                        driver.FindElement(By.Id("Main_btnSearchOwner")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Ownername Search Result", driver, "CA", "San Luis Obispo");
                        IWebElement multiaddrtableElement = driver.FindElement(By.XPath("//*[@id='Main_gvSearchResults']/tbody"));
                        IList<IWebElement> multiaddrtableRow = multiaddrtableElement.FindElements(By.TagName("tr"));
                        int multiaddrtableRowcount = multiaddrtableRow.Count;

                        if (multiaddrtableRowcount > 2)
                        {
                            multiparcel(orderNumber);
                            driver.Quit();
                            return "MultiParcel";
                        }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("ctl00_BodyContentPlaceHolder_ErrorLabel")).Text;
                            if (nodata.Contains("Returned 0 records."))
                            {
                                HttpContext.Current.Session["Nodata_LeeFL"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    driver.FindElement(By.XPath("//*[@id='Main_gvSearchResults']/tbody/tr[2]/td[4]/a")).SendKeys(Keys.Enter);
                    Thread.Sleep(3000);

                    //property Details
                    assyear = driver.FindElement(By.XPath("//*[@id='Main_DetailCenter2']/table/tbody/tr[1]/td[1]")).Text.Replace("Assessment Information for the ", "").Replace("Tax Year", "");
                    pltitle.assyear = assyear;
                    outparcelno = driver.FindElement(By.Id("Main_lblAssessmentNumberValue")).Text.Trim();
                    pltitle.TaxIDNumber = outparcelno;
                    outparcelnowoh = outparcelno.Replace("-", "").Replace(",", "").Replace(" ", "");
                    gc.CreatePdf(orderNumber, outparcelnowoh, "Assessment Details", driver, "CA", "San Luis Obispo");
                    //  ownername = driver.FindElement(By.Id("Main_lblOwnerName")).Text.Trim();
                    siteaddr = driver.FindElement(By.Id("Main_lblAddressValue")).Text.Trim();
                    //city = driver.FindElement(By.Id("Main_lblCommunityValue")).Text.Trim();
                    year_built = driver.FindElement(By.Id("Main_lblYearBuiltValue")).Text.Trim();
                    string taxratearea = driver.FindElement(By.Id("Main_lblTaxAreaValue")).Text.Trim();
                    pltitle.TaxIDNumberFurtherDescribed = taxratearea;
                    string property = siteaddr + "~" + year_built;
                    gc.insert_date(orderNumber, outparcelno, 432, property, 1, DateTime.Now);

                    //assessment details                
                    LandValue = driver.FindElement(By.Id("Main_lblLandValue")).Text.Trim();
                    pltitle.Land = LandValue.Replace(",", "").Trim();
                    imp_Value = driver.FindElement(By.Id("Main_lblImprovementsValue")).Text.Trim();
                    pltitle.Improvements = imp_Value.Replace(",", "").Trim();
                    AssessedValue = driver.FindElement(By.Id("Main_lblAssessedValue")).Text.Trim();
                    personal_property = driver.FindElement(By.Id("Main_lblPersonalPropValue")).Text.Trim();
                    fix_value = driver.FindElement(By.Id("Main_lblFixturesValue")).Text.Trim();
                    Exemption = driver.FindElement(By.Id("Main_lblTotalExemptionValue")).Text.Trim();
                    pltitle.ExemptionHomeowners = Exemption.Replace(",", "").Trim();
                    if (Exemption == "")
                    {
                        pltitle.ExemptionHomeowners = null;
                    }
                    netTaxableValue = driver.FindElement(By.Id("Main_lblNetValue")).Text.Trim();
                    //Assessed Year~Land Value~Improvement Value~Total Assessed Value~Personal Property~Fixtures Value~Total Exemption~Net Taxable Value
                    string ass = assyear + "~" + LandValue + "~" + imp_Value + "~" + AssessedValue + "~" + personal_property + "~" + fix_value + "~" + Exemption + "~" + netTaxableValue;
                    gc.insert_date(orderNumber, outparcelno, 433, ass, 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    //Tax details
                    int lat = 0;
                    driver.Navigate().GoToUrl("https://services.slocountytax.org/Entry.aspx");
                    Thread.Sleep(3000);

                    IWebElement SelectOption = driver.FindElement(By.Id("DataDate"));
                    IList<IWebElement> Select = SelectOption.FindElements(By.TagName("option"));
                    int Check = 0;
                    foreach (IWebElement Op in Select)
                    {

                        if (Check <= 3)
                        {
                            option.Add(Op.Text);
                        }
                        Check++;
                    }

                    outparcelnowoh = outparcelno.Replace("-", "").Replace(",", "").Replace(" ", "");
                    par1 = outparcelnowoh.Substring(0, 3);
                    par2 = outparcelnowoh.Substring(3, 3);
                    par3 = outparcelnowoh.Substring(6, 3);
                    driver.FindElement(By.Id("txtAPN1")).SendKeys(par1);
                    driver.FindElement(By.Id("txtAPN2")).SendKeys(par2);
                    driver.FindElement(By.Id("txtAPN3")).SendKeys(par3);
                    driver.FindElement(By.Id("txtAPN4")).SendKeys(Keys.Enter);
                    gc.CreatePdf(orderNumber, outparcelnowoh, "Current Year Tax Search Result", driver, "CA", "San Luis Obispo");
                    Thread.Sleep(3000);
                    try
                    {
                        taxsale = driver.FindElement(By.XPath("//*[@id='POSWarning_tblWarning']/tbody/tr[4]/td/font/b")).Text.Trim();
                        comment = driver.FindElement(By.XPath("//*[@id='POSWarning_tblWarning']/tbody/tr[2]/td/font/b")).Text.Trim();
                        gc.insert_date(orderNumber, outparcelno, 452, comment + "," + taxsale, 1, DateTime.Now);
                        driver.Quit();
                        return "Data Inserted Successfully";
                    }
                    catch { }
                    int k = 0;
                    try
                    {
                        linkbillno = driver.FindElement(By.Id("SecTaxBills_RadListView1_ctrl0_lblBillNum")).Text.Trim();
                        string URL = "https://services.slocountytax.org/Detail.aspx?lblBillnum=" + linkbillno + " &csus=0";
                        MailURL.Add(URL);
                    }
                    catch { }
                    try
                    {
                        linkbillno = driver.FindElement(By.XPath("//*[@id='Form1']/table/tbody/tr[7]/td/table[3]/tbody/tr/td[1]/a")).Text.Trim();
                        string URL = "https://services.slocountytax.org/Detail.aspx?lblBillnum=" + linkbillno + " &csus=0";
                        MailURL.Add(URL);
                    }
                    catch { }
                    try

                    {
                        IWebElement Ilink = driver.FindElement(By.XPath("//*[@id='Form1']/table[3]/tbody/tr/td[1]/a"));
                        string URL = Ilink.GetAttribute("href");
                        if (!URL.Contains("Online-Privacy"))
                        {
                            MailURL.Add(URL);
                        }
                    }
                    catch { }
                    try
                    {
                        IWebElement Ilink = driver.FindElement(By.XPath("//*[@id='Form1']/table[5]/tbody/tr/td[1]/a"));
                        string URL = Ilink.GetAttribute("href");
                        MailURL.Add(URL);
                    }
                    catch { }
                    try
                    {
                        IWebElement Ilink = driver.FindElement(By.XPath("//*[@id='Form1']/table[7]/tbody/tr/td[1]/a"));
                        string URL = Ilink.GetAttribute("href");
                        MailURL.Add(URL);
                    }
                    catch { }

                    int m = 0;
                    foreach (string item in option)
                    {

                        if (m > 0)
                        {
                            driver.Navigate().GoToUrl("https://services.slocountytax.org/Entry.aspx");
                            Thread.Sleep(3000);
                            var Selectyear = driver.FindElement(By.Id("DataDate"));
                            var SelectyearTax = new SelectElement(Selectyear);
                            SelectyearTax.SelectByText(item);
                            byear = item.Substring(5, 2);
                            outparcelnowoh = outparcelno.Replace("-", "").Replace(",", "").Replace(" ", "");
                            par1 = outparcelnowoh.Substring(0, 3);
                            par2 = outparcelnowoh.Substring(3, 3);
                            par3 = outparcelnowoh.Substring(6, 3);
                            driver.FindElement(By.Id("txtAPN1")).SendKeys(par1);
                            driver.FindElement(By.Id("txtAPN2")).SendKeys(par2);
                            driver.FindElement(By.Id("txtAPN3")).SendKeys(par3);
                            driver.FindElement(By.Id("txtAPN4")).SendKeys(Keys.Enter);
                            gc.CreatePdf(orderNumber, outparcelnowoh, "Tax Search Result" + byear, driver, "CA", "San Luis Obispo");
                            Thread.Sleep(3000);

                            try
                            {
                                //MailURL.Clear();
                                linkbillno = driver.FindElement(By.Id("SecTaxBills_RadListView1_ctrl0_lblBillNum")).Text.Trim();
                                string URL = "https://services.slocountytax.org/Detail.aspx?lblBillnum=" + linkbillno + " &csus=0";
                                MailURL.Add(URL);
                            }
                            catch { }
                            try
                            {
                                //MailURL.Clear();
                                linkbillno = driver.FindElement(By.XPath("//*[@id='Form1']/table/tbody/tr[7]/td/table[3]/tbody/tr/td[1]/a")).Text.Trim();
                                string URL = "https://services.slocountytax.org/detailsupp.aspx?lblBillnum="+linkbillno+"&csus=0";
                                MailURL.Add(URL);
                            }
                            catch { }
                            try

                            {
                                IWebElement Ilink = driver.FindElement(By.XPath("//*[@id='Form1']/table[3]/tbody/tr/td[1]/a"));
                                string URL = Ilink.GetAttribute("href");
                                bool has = MailURL.Any(strURL => MailURL.Contains(URL));
                                if (has == false)
                                {
                                    if (!URL.Contains("Online-Privacy"))
                                    {
                                        MailURL.Add(URL);
                                    }
                                }
                                else
                                {
                                    MailURL.Remove(URL);
                                }
                            }
                            catch { }
                            try
                            {
                                IWebElement Ilink = driver.FindElement(By.XPath("//*[@id='Form1']/table[5]/tbody/tr/td[1]/a"));
                                string URL = Ilink.GetAttribute("href");
                                bool has = MailURL.Any(strURL => MailURL.Contains(URL));
                                if (has == false)
                                {
                                    MailURL.Add(URL);
                                }
                                else
                                {
                                    MailURL.Remove(URL);
                                }

                            }
                            catch { }
                            try
                            {
                                IWebElement Ilink = driver.FindElement(By.XPath("//*[@id='Form1']/table[7]/tbody/tr/td[1]/a"));
                                string URL = Ilink.GetAttribute("href");
                                bool has = MailURL.Any(strURL => MailURL.Contains(URL));
                                if (has == false)
                                {
                                    MailURL.Add(URL);
                                }
                                else
                                {
                                    MailURL.Remove(URL);
                                }
                            }
                            catch { }

                        }
                        int z = 0;

                        MailURL = MailURL.Distinct().ToList();
                        foreach (string suburl in MailURL)
                        {
                            driver.Navigate().GoToUrl(suburl);
                            Thread.Sleep(3000);
                            if (z > 0)
                            {
                                //Supplemental Tax Details
                                try
                                {
                                    head.Clear(); duedate.Clear(); delinqafter.Clear(); taxamount.Clear(); interest.Clear(); penalty.Clear(); cost.Clear(); fees.Clear(); total.Clear();
                                    amountpaid.Clear(); datepaid.Clear(); batchnumber.Clear(); paymenttype.Clear(); balance.Clear();
                                    taxtype = driver.FindElement(By.XPath("//*[@id='Form1']/table[3]/tbody/tr[1]/td[1]/b")).Text.Trim().Replace("Detail", "");
                                    taxyear = driver.FindElement(By.XPath("//*[@id='Form1']/table[3]/tbody/tr[1]/td[2]")).Text.Replace("Bill #:", "").Trim();
                                    var vtaxyear1 = taxyear.Split(' ');
                                    taxyear = vtaxyear1[0];
                                    byear = taxyear.Substring(5, 2);
                                    gc.CreatePdf(orderNumber, outparcelnowoh, "Supplemental Tax Details" + byear, driver, "CA", "San Luis Obispo");
                                    billnumber = vtaxyear1[2];
                                    billnumberwoh = billnumber.Replace(",", "").Trim();
                                    taxowner = driver.FindElement(By.XPath("//*[@id='Form1']/table[5]/tbody/tr[1]/td")).Text.Trim();
                                    IWebElement taxtableElement1 = driver.FindElement(By.XPath("//*[@id='Form1']/table[9]"));
                                    IList<IWebElement> taxtableRow1 = taxtableElement1.FindElements(By.TagName("tr"));
                                    IList<IWebElement> taxrowTD1;
                                    int i = 0;
                                    int lat1 = 0;
                                    foreach (IWebElement row in taxtableRow1)
                                    {

                                        taxrowTD1 = row.FindElements(By.TagName("td"));
                                        if (taxrowTD1.Count != 0)
                                        {
                                            if (i == 0)
                                            {
                                                head.Add(taxrowTD1[1].Text.Trim());
                                                head.Add(taxrowTD1[2].Text.Trim());
                                                head.Add(taxrowTD1[3].Text.Trim());
                                            }
                                            if (i == 1)
                                            {
                                                duedate.Add("");
                                                duedate.Add("");
                                                duedate.Add("");

                                                delinqafter.Add(taxrowTD1[1].Text.Trim());
                                                FirstDueDate = duedate[0];
                                                if (FirstDueDate == "")
                                                {
                                                    pltitle.FirstDueDate = null;
                                                }
                                                else
                                                {
                                                    pltitle.FirstDueDate = FirstDueDate;
                                                }

                                                delinqafter.Add(taxrowTD1[2].Text.Trim());
                                                SecondDueDate = duedate[1];
                                                if (SecondDueDate == "")
                                                {
                                                    pltitle.SecondDueDate = null;
                                                }
                                                else
                                                {
                                                    pltitle.SecondDueDate = SecondDueDate;
                                                }
                                                delinqafter.Add(taxrowTD1[3].Text.Trim());

                                            }
                                            else if (i == 2)
                                            {
                                                taxamount.Add(taxrowTD1[1].Text.Trim());
                                                FirstInstallment = taxamount[0];
                                                pltitle.FirstInstallment = FirstInstallment.Replace("$", "").Replace(",", "").Trim();
                                                taxamount.Add(taxrowTD1[2].Text.Trim());
                                                SecondInstallment = taxamount[1];
                                                pltitle.SecondInstallment = SecondInstallment.Replace("$", "").Replace(",", "").Trim();
                                                taxamount.Add(taxrowTD1[3].Text.Trim());
                                            }
                                            else if (i == 3)
                                            {
                                                interest.Add(taxrowTD1[1].Text.Trim());
                                                interest.Add(taxrowTD1[2].Text.Trim());
                                                interest.Add(taxrowTD1[3].Text.Trim());
                                            }
                                            else if (i == 4)
                                            {
                                                penalty.Add(taxrowTD1[1].Text.Trim());
                                                penalty.Add(taxrowTD1[2].Text.Trim());
                                                penalty.Add(taxrowTD1[3].Text.Trim());
                                            }
                                            else if (i == 5)
                                            {
                                                cost.Add(taxrowTD1[1].Text.Trim());
                                                cost.Add(taxrowTD1[2].Text.Trim());
                                                cost.Add(taxrowTD1[3].Text.Trim());
                                            }
                                            else if (i == 6)
                                            {
                                                fees.Add(taxrowTD1[1].Text.Trim());
                                                fees.Add(taxrowTD1[2].Text.Trim());
                                                fees.Add(taxrowTD1[3].Text.Trim());
                                            }

                                            else if (i == 7)
                                            {
                                                total.Add(taxrowTD1[1].Text.Trim());
                                                total.Add(taxrowTD1[2].Text.Trim());
                                                total.Add(taxrowTD1[3].Text.Trim());
                                            }

                                            else if (i == 8)
                                            {
                                                amountpaid.Add(taxrowTD1[1].Text.Trim());
                                                FirstPaid = amountpaid[0];
                                                if (FirstPaid == "$0.00")
                                                {
                                                    pltitle.FirstPaid = "False";
                                                }
                                                else
                                                {
                                                    pltitle.FirstPaid = "True";
                                                }
                                                amountpaid.Add(taxrowTD1[2].Text.Trim());
                                                SecondPaid = amountpaid[1];
                                                if (SecondPaid == "$0.00")
                                                {
                                                    pltitle.SecondPaid = "False";
                                                }
                                                else
                                                {
                                                    pltitle.SecondPaid = "True";
                                                }
                                                amountpaid.Add(taxrowTD1[3].Text.Trim());
                                            }

                                            else if (i == 9)
                                            {
                                                datepaid.Add(taxrowTD1[1].Text.Trim());
                                                FirstTaxesOutDate = datepaid[0];
                                                if (FirstTaxesOutDate == "")
                                                {
                                                    pltitle.FirstTaxesOutDate = null;
                                                }
                                                else
                                                {
                                                    pltitle.FirstTaxesOutDate = FirstTaxesOutDate;
                                                }
                                                datepaid.Add(taxrowTD1[2].Text.Trim());
                                                SecondTaxesOutDate = datepaid[1];
                                                if (SecondTaxesOutDate == "")
                                                {
                                                    pltitle.SecondTaxesOutDate = null;
                                                }
                                                else
                                                {
                                                    pltitle.SecondTaxesOutDate = SecondTaxesOutDate;
                                                }

                                                datepaid.Add(taxrowTD1[3].Text.Trim());
                                            }

                                            else if (i == 10)
                                            {
                                                batchnumber.Add(taxrowTD1[1].Text.Trim());
                                                batchnumber.Add(taxrowTD1[2].Text.Trim());
                                                batchnumber.Add(taxrowTD1[3].Text.Trim());
                                            }
                                            else if (i == 11)
                                            {
                                                paymenttype.Add("");
                                                paymenttype.Add("");
                                                paymenttype.Add("");

                                                balance.Add(taxrowTD1[1].Text.Trim());
                                                FirstDue = balance[0];
                                                if (FirstDue == "$0.00")
                                                {
                                                    pltitle.FirstDue = "False";
                                                }
                                                else
                                                {
                                                    pltitle.FirstDue = "True";
                                                }
                                                balance.Add(taxrowTD1[2].Text.Trim());
                                                SecondDue = balance[1];
                                                if (SecondDue == "$0.00")
                                                {
                                                    pltitle.SecondDue = "False";
                                                }
                                                else
                                                {
                                                    pltitle.SecondDue = "True";
                                                }
                                                balance.Add(taxrowTD1[3].Text.Trim());
                                            }

                                        }
                                        i++;
                                    }

                                    //Tax Year~Tax Type~Bill Number~Owner Name~Installment~Due Date~Delinquent after~Tax Amount~Interest~Penalty~Cost~Fees~Total~Amount Paid~Date Paid~Batch Number~Payment Type~Balance
                                    string value1 = taxyear + "~" + taxtype + "~" + billnumber + "~" + taxowner + "~" + head[0] + "~" + duedate[0] + "~" + delinqafter[0] + "~" + taxamount[0] + "~" + interest[0] + "~" + penalty[0] + "~" + cost[0] + "~" + fees[0] + "~" + total[0] + "~" + amountpaid[0] + "~" + datepaid[0] + "~" + batchnumber[0] + "~" + paymenttype[0] + "~" + balance[0] + "~" + comment;
                                    gc.insert_date(orderNumber, outparcelno, 435, value1, 1, DateTime.Now);
                                    string value2 = taxyear + "~" + taxtype + "~" + billnumber + "~" + taxowner + "~" + head[1] + "~" + duedate[1] + "~" + delinqafter[1] + "~" + taxamount[1] + "~" + interest[1] + "~" + penalty[1] + "~" + cost[1] + "~" + fees[1] + "~" + total[1] + "~" + amountpaid[1] + "~" + datepaid[1] + "~" + batchnumber[1] + "~" + paymenttype[1] + "~" + balance[1] + "~" + comment;
                                    gc.insert_date(orderNumber, outparcelno, 435, value2, 1, DateTime.Now);
                                    string value3 = taxyear + "~" + taxtype + "~" + billnumber + "~" + taxowner + "~" + head[2] + "~" + duedate[2] + "~" + delinqafter[2] + "~" + taxamount[2] + "~" + interest[2] + "~" + penalty[2] + "~" + cost[2] + "~" + fees[2] + "~" + total[2] + "~" + amountpaid[2] + "~" + datepaid[2] + "~" + batchnumber[2] + "~" + paymenttype[2] + "~" + balance[2] + "~" + comment;
                                    gc.insert_date(orderNumber, outparcelno, 435, value3, 1, DateTime.Now);

                                    //Placer Supplemental Tax
                                    //if (p == 0)
                                    //{
                                    if (m == 0)
                                    {
                                        string syear = taxyear.Substring(0, 4);
                                        if (tyear == syear)
                                        {
                                            pltitle.Year = Convert.ToInt32(syear);
                                            gc.InsertSearchTax(orderNumber, pltitle.Land, pltitle.Improvements, pltitle.ExemptionHomeowners, "0", pltitle.FirstInstallment, pltitle.FirstDueDate, pltitle.FirstTaxesOutDate, pltitle.FirstPaid, pltitle.FirstDue, pltitle.SecondInstallment, pltitle.SecondDueDate, pltitle.SecondTaxesOutDate, pltitle.SecondPaid, pltitle.SecondDue, pltitle.assyear, 100, pltitle.Year, "San luis obsipo County Treasurer-Tax Collector", pltitle.TaxIDNumber, "Supplemental", pltitle.TaxIDNumberFurtherDescribed);
                                        }
                                        else if (FirstDue != "$0.00" || SecondDue != "$0.00")
                                        {
                                            pltitle.Year = Convert.ToInt32(syear);
                                            gc.InsertSearchTax(orderNumber, pltitle.Land, pltitle.Improvements, pltitle.ExemptionHomeowners, "0", pltitle.FirstInstallment, pltitle.FirstDueDate, pltitle.FirstTaxesOutDate, pltitle.FirstPaid, pltitle.FirstDue, pltitle.SecondInstallment, pltitle.SecondDueDate, pltitle.SecondTaxesOutDate, pltitle.SecondPaid, pltitle.SecondDue, pltitle.assyear, 100, pltitle.Year, "San luis obsipo County Treasurer-Tax Collector", pltitle.TaxIDNumber, "Supplemental", pltitle.TaxIDNumberFurtherDescribed);
                                        }
                                    }
                                    //}
                                    p++;

                                }
                                catch (Exception ex) { }
                            }
                            else
                            {
                                try
                                {
                                    head.Clear(); duedate.Clear(); delinqafter.Clear(); taxamount.Clear(); interest.Clear(); penalty.Clear(); cost.Clear(); fees.Clear(); total.Clear();
                                    amountpaid.Clear(); datepaid.Clear(); batchnumber.Clear(); paymenttype.Clear(); balance.Clear();
                                    taxtype = driver.FindElement(By.XPath("//*[@id='masterTable']/tbody/tr/td/table[2]/tbody/tr[1]/td[1]/b")).Text.Trim().Replace("Detail", "");
                                    taxyear = driver.FindElement(By.XPath("//*[@id='masterTable']/tbody/tr/td/table[2]/tbody/tr[1]/td[2]")).Text.Replace("Bill #:", "").Trim();
                                    var vtaxyear = taxyear.Split(' ');
                                    taxyear = vtaxyear[0];
                                    byear = taxyear.Substring(5, 2);
                                    gc.CreatePdf(orderNumber, outparcelnowoh, "Secured Tax Details" + byear, driver, "CA", "San Luis Obispo");
                                    billnumber = vtaxyear[3];
                                    billnumberwoh = billnumber.Replace(",", "").Trim();
                                    taxowner = driver.FindElement(By.XPath("//*[@id='masterTable']/tbody/tr/td/table[4]/tbody/tr[1]/td")).Text.Trim();
                                    IWebElement taxtableElement = driver.FindElement(By.XPath("//*[@id='tblInstallmentInfo2']"));
                                    IList<IWebElement> taxtableRow = taxtableElement.FindElements(By.TagName("tr"));
                                    IList<IWebElement> taxrowTD;

                                    int i = 0;
                                    foreach (IWebElement row in taxtableRow)
                                    {
                                        taxrowTD = row.FindElements(By.TagName("td"));
                                        if (taxrowTD.Count != 0)
                                        {
                                            if (i == 0)
                                            {
                                                head.Add(taxrowTD[1].Text.Trim());
                                                head.Add(taxrowTD[2].Text.Trim());
                                                head.Add(taxrowTD[3].Text.Trim());
                                            }
                                            if (i == 1)
                                            {
                                                duedate.Add(taxrowTD[1].Text.Trim());
                                                FirstDueDate = duedate[0];
                                                if (FirstDueDate == "")
                                                {
                                                    pltitle.FirstDueDate = null;
                                                }
                                                else
                                                {
                                                    pltitle.FirstDueDate = FirstDueDate;
                                                }

                                                duedate.Add(taxrowTD[2].Text.Trim());
                                                SecondDueDate = duedate[1];
                                                if (SecondDueDate == "")
                                                {
                                                    pltitle.SecondDueDate = null;
                                                }
                                                else
                                                {
                                                    pltitle.SecondDueDate = SecondDueDate;
                                                }

                                                duedate.Add(taxrowTD[3].Text.Trim());
                                            }
                                            else if (i == 2)
                                            {
                                                delinqafter.Add(taxrowTD[1].Text.Trim());
                                                delinqafter.Add(taxrowTD[2].Text.Trim());
                                                delinqafter.Add(taxrowTD[3].Text.Trim());
                                            }
                                            else if (i == 3)
                                            {
                                                taxamount.Add(taxrowTD[1].Text.Trim());
                                                FirstInstallment = taxamount[0];
                                                pltitle.FirstInstallment = FirstInstallment.Replace("$", "").Replace(",", "").Trim();
                                                taxamount.Add(taxrowTD[2].Text.Trim());
                                                SecondInstallment = taxamount[1];
                                                pltitle.SecondInstallment = SecondInstallment.Replace("$", "").Replace(",", "").Trim();
                                                taxamount.Add(taxrowTD[3].Text.Trim());
                                            }
                                            else if (i == 4)
                                            {
                                                interest.Add(taxrowTD[1].Text.Trim());
                                                interest.Add(taxrowTD[2].Text.Trim());
                                                interest.Add(taxrowTD[3].Text.Trim());
                                            }
                                            else if (i == 5)
                                            {
                                                penalty.Add(taxrowTD[1].Text.Trim());
                                                penalty.Add(taxrowTD[2].Text.Trim());
                                                penalty.Add(taxrowTD[3].Text.Trim());
                                            }
                                            else if (i == 6)
                                            {
                                                cost.Add(taxrowTD[1].Text.Trim());
                                                cost.Add(taxrowTD[2].Text.Trim());
                                                cost.Add(taxrowTD[3].Text.Trim());
                                            }

                                            else if (i == 7)
                                            {
                                                fees.Add(taxrowTD[1].Text.Trim());
                                                fees.Add(taxrowTD[2].Text.Trim());
                                                fees.Add(taxrowTD[3].Text.Trim());
                                            }

                                            else if (i == 8)
                                            {
                                                total.Add(taxrowTD[1].Text.Trim());
                                                total.Add(taxrowTD[2].Text.Trim());
                                                total.Add(taxrowTD[3].Text.Trim());
                                            }

                                            else if (i == 9)
                                            {
                                                amountpaid.Add(taxrowTD[1].Text.Trim());
                                                FirstPaid = amountpaid[0];
                                                if (FirstPaid == "$0.00")
                                                {
                                                    pltitle.FirstPaid = "False";
                                                }
                                                else
                                                {
                                                    pltitle.FirstPaid = "True";
                                                }
                                                amountpaid.Add(taxrowTD[2].Text.Trim());
                                                SecondPaid = amountpaid[1];
                                                if (SecondPaid == "$0.00")
                                                {
                                                    pltitle.SecondPaid = "False";
                                                }
                                                else
                                                {
                                                    pltitle.SecondPaid = "True";
                                                }
                                                amountpaid.Add(taxrowTD[3].Text.Trim());
                                            }

                                            else if (i == 10)
                                            {
                                                datepaid.Add(taxrowTD[1].Text.Trim());
                                                FirstTaxesOutDate = datepaid[0];
                                                if (FirstTaxesOutDate == "")
                                                {
                                                    pltitle.FirstTaxesOutDate = null;
                                                }
                                                else
                                                {
                                                    pltitle.FirstTaxesOutDate = FirstTaxesOutDate;
                                                }
                                                datepaid.Add(taxrowTD[2].Text.Trim());
                                                SecondTaxesOutDate = datepaid[1];
                                                if (SecondTaxesOutDate == "")
                                                {
                                                    pltitle.SecondTaxesOutDate = null;
                                                }
                                                else
                                                {
                                                    pltitle.SecondTaxesOutDate = SecondTaxesOutDate;
                                                }

                                                datepaid.Add(taxrowTD[3].Text.Trim());
                                            }
                                            else if (i == 11)
                                            {
                                                batchnumber.Add(taxrowTD[1].Text.Trim());
                                                batchnumber.Add(taxrowTD[2].Text.Trim());
                                                batchnumber.Add(taxrowTD[3].Text.Trim());
                                            }
                                            else if (i == 12)
                                            {
                                                paymenttype.Add(taxrowTD[1].Text.Trim());
                                                paymenttype.Add(taxrowTD[2].Text.Trim());
                                                paymenttype.Add(taxrowTD[3].Text.Trim());
                                            }
                                            else if (i == 13)
                                            {
                                                balance.Add(taxrowTD[1].Text.Trim());
                                                FirstDue = balance[0];
                                                if (FirstDue == "$0.00")
                                                {
                                                    pltitle.FirstDue = "False";
                                                }
                                                else
                                                {
                                                    pltitle.FirstDue = "True";
                                                }
                                                balance.Add(taxrowTD[2].Text.Trim());
                                                SecondDue = balance[1];
                                                if (SecondDue == "$0.00")
                                                {
                                                    pltitle.SecondDue = "False";
                                                }
                                                else
                                                {
                                                    pltitle.SecondDue = "True";
                                                }
                                                balance.Add(taxrowTD[3].Text.Trim());
                                            }

                                        }
                                        i++;
                                    }
                                    //Tax Year~Tax Type~Bill Number~Owner Name~Installment~Due Date~Delinquent after~Tax Amount~Interest~Penalty~Cost~Fees~Total~Amount Paid~Date Paid~Batch Number~Payment Type~Balance
                                    string value1 = taxyear + "~" + taxtype + "~" + billnumber + "~" + taxowner + "~" + head[0] + "~" + duedate[0] + "~" + delinqafter[0] + "~" + taxamount[0] + "~" + interest[0] + "~" + penalty[0] + "~" + cost[0] + "~" + fees[0] + "~" + total[0] + "~" + amountpaid[0] + "~" + datepaid[0] + "~" + batchnumber[0] + "~" + paymenttype[0] + "~" + balance[0] + "~" + comment;
                                    gc.insert_date(orderNumber, outparcelno, 435, value1, 1, DateTime.Now);
                                    string value2 = taxyear + "~" + taxtype + "~" + billnumber + "~" + taxowner + "~" + head[1] + "~" + duedate[1] + "~" + delinqafter[1] + "~" + taxamount[1] + "~" + interest[1] + "~" + penalty[1] + "~" + cost[1] + "~" + fees[1] + "~" + total[1] + "~" + amountpaid[1] + "~" + datepaid[1] + "~" + batchnumber[1] + "~" + paymenttype[1] + "~" + balance[1] + "~" + comment;
                                    gc.insert_date(orderNumber, outparcelno, 435, value2, 1, DateTime.Now);
                                    string value3 = taxyear + "~" + taxtype + "~" + billnumber + "~" + taxowner + "~" + head[2] + "~" + duedate[2] + "~" + delinqafter[2] + "~" + taxamount[2] + "~" + interest[2] + "~" + penalty[2] + "~" + cost[2] + "~" + fees[2] + "~" + total[2] + "~" + amountpaid[2] + "~" + datepaid[2] + "~" + batchnumber[2] + "~" + paymenttype[2] + "~" + balance[2] + "~" + comment;
                                    gc.insert_date(orderNumber, outparcelno, 435, value3, 1, DateTime.Now);

                                    //Placer County Tax                                  
                                    if (n == 0)
                                    {
                                        tyear = taxyear.Substring(0, 4);
                                        pltitle.Year = Convert.ToInt32(tyear);
                                        //pltitle.TaxIDNumber = billnumberwoh;
                                        gc.InsertSearchTax(orderNumber, pltitle.Land, pltitle.Improvements, pltitle.ExemptionHomeowners, "0", pltitle.FirstInstallment, pltitle.FirstDueDate, pltitle.FirstTaxesOutDate, pltitle.FirstPaid, pltitle.FirstDue, pltitle.SecondInstallment, pltitle.SecondDueDate, pltitle.SecondTaxesOutDate, pltitle.SecondPaid, pltitle.SecondDue, pltitle.assyear, 1, pltitle.Year, "San luis obsipo County Treasurer-Tax Collector", pltitle.TaxIDNumber, "County", pltitle.TaxIDNumberFurtherDescribed);
                                    }
                                    n++;
                                }
                                catch
                                { }
                            }

                            //load chrome driver...
                            //IWebDriver chDriver = new ChromeDriver();
                            var chromeOptions = new ChromeOptions();
                            var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];
                            chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                            chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                            chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                            var chDriver = new ChromeDriver(chromeOptions);

                            //bill download 
                            if (z == 0)
                            {
                                try
                                {
                                    chDriver.Navigate().GoToUrl("https://services.slocountytax.org/ViewPrintSecTaxBill.aspx?billId=" + billnumberwoh + byear);
                                    Thread.Sleep(3000);
                                    gc.CreatePdf(orderNumber, outparcelnowoh, "Secured Tax Bill" + byear, driver, "CA", "San Luis Obispo");
                                    chDriver.FindElement(By.Id("btnSaveToPDF")).SendKeys(Keys.Enter);
                                    Thread.Sleep(3000);
                                    string fileName = "Secured Tax Bill - " + billnumberwoh + byear + ".pdf";
                                    gc.AutoDownloadFile(orderNumber, outparcelnowoh, "San Luis Obispo", "CA", fileName);

                                    string FilePath = gc.filePath(orderNumber, outparcelnowoh) + fileName;
                                    PdfReader reader;
                                    string pdfData;
                                    string pdftext = "";
                                    if (a == 0)
                                    {
                                        try
                                        {
                                            reader = new PdfReader(FilePath);
                                            String textFromPage = PdfTextExtractor.GetTextFromPage(reader, 1);
                                            System.Diagnostics.Debug.WriteLine("" + textFromPage);

                                            pdftext = textFromPage;


                                        }
                                        catch (Exception ex)
                                        { }


                                        string tableassess = gc.Between(pdftext, "LAND", "1 Assessment").Trim();
                                        string land = GlobalClass.Before(tableassess, "IMPROVEMENTS");
                                        string improvement = GlobalClass.After(tableassess, "IMPROVEMENTS");



                                        string tableassess1 = gc.Between(pdftext, "Total Tax Rate", "5 Assessed Owner").Trim();
                                        string[] tableArray = tableassess1.Split(' ');

                                        string assessment = "", billnumber = "", taxrate = "", totaltaxrae = "", legal = "", netassessed = "", year = "";
                                        assessment = tableArray[0];
                                        billnumber = tableArray[1];
                                        taxrate = tableArray[2];
                                        //  pltitle.TaxIDNumberFurtherDescribed = taxrate;
                                        totaltaxrae = tableArray[3];


                                        string tableassess2 = gc.Between(pdftext, "Tax Calculation", "13\nService Agency Contact Rate Amount").Trim();
                                        string[] tableArray1 = tableassess2.Split('\n');
                                        legal = tableArray1[1];
                                        string tableassess3 = gc.Between(pdftext, "9 First Installment Due", "Net Assessed Value").Trim();
                                        string[] tableArray2 = tableassess3.Split('\n');
                                        netassessed = tableArray2[3].Trim().Replace("† ", "");
                                        string tableassess4 = gc.Between(pdftext, "Treasurer-Tax Collecto", "ANNUAL SECURED PROPERTY TAX BILL").Trim();
                                        year = tableassess4.Replace("r\n", "");
                                        try
                                        {
                                            string[] tableArrayyear = year.Split('\n');
                                            year = tableArrayyear[1];
                                        }
                                        catch
                                        {

                                        }
                                        //Tax Rate Area~Total Tax Rate~Legal Description
                                        string prop = taxrate + "~" + totaltaxrae + "~" + legal;
                                        //gc.insert_date(orderNumber, parcelNumber, 432, prop, 1, DateTime.Now);
                                        //Assessed Year~Bill Number~Land~Improvements~Net Assessed Value
                                        ass = year + "~" + billnumber + "~" + land + "~" + improvement + "~" + netassessed;
                                        //  gc.insert_date(orderNumber, parcelNumber, 433, ass, 1, DateTime.Now);
                                        a++;
                                    }
                                }
                                catch { }
                            }
                            else
                            {
                                try
                                {

                                    chDriver.Navigate().GoToUrl("https://services.slocountytax.org/ViewPrintSuppTaxBill.aspx?billId=" + billnumberwoh + byear);
                                    Thread.Sleep(3000);
                                    gc.CreatePdf(orderNumber, outparcelnowoh, "Supplemental Tax Bill" + byear, driver, "CA", "San Luis Obispo");
                                    chDriver.FindElement(By.Id("btnSaveToPDF")).SendKeys(Keys.Enter);
                                    Thread.Sleep(3000);
                                    string fileName = "Supplemental Tax Bill - " + billnumberwoh + byear + ".pdf";
                                    gc.AutoDownloadFile(orderNumber, outparcelnowoh, "San Luis Obispo", "CA", fileName);
                                }
                                catch { }
                            }
                            z++;
                            chDriver.Quit();
                        }
                        m++;
                    }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "CA", "San Luis Obispo", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "CA", "San Luis Obispo");
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

        public void multiparcel(string orderNumber)
        {//*[@id="Main_gvSearchResults"]/tbody
            IWebElement multiaddrtableElement = driver.FindElement(By.XPath("//*[@id='Main_gvSearchResults']/tbody"));
            IList<IWebElement> multiaddrtableRow = multiaddrtableElement.FindElements(By.TagName("tr"));
            int multiaddrtableRowcount = multiaddrtableRow.Count;
            IList<IWebElement> multiaddrrowTD;
            IList<IWebElement> multiaddrrowTH;
            int j = 0;
            HttpContext.Current.Session["multiparcel_CASanluisobispo"] = "Yes";
            foreach (IWebElement row in multiaddrtableRow)
            {
                if (j < multiaddrtableRowcount && j <= 25)
                {
                    gc.CreatePdf_WOP(orderNumber, "Multiparcel Result", driver, "CA", "San Luis Obispo");
                    multiaddrrowTD = row.FindElements(By.TagName("td"));
                    multiaddrrowTH = row.FindElements(By.TagName("th"));
                    if (multiaddrrowTD.Count != 0 && multiaddrrowTD.Count == 5)
                    {
                        string multiowner = multiaddrrowTD[0].Text + "~" + multiaddrrowTD[1].Text + "~" + multiaddrrowTD[2].Text;
                        gc.insert_date(orderNumber, multiaddrrowTD[0].Text.Trim(), 431, multiowner, 1, DateTime.Now);
                    }
                    j++;
                }
            }
            if (multiaddrtableRow.Count > 25)
            {
                HttpContext.Current.Session["multiParcel_CASanluisobispo_Multicount"] = "Maximum";
            }
            else
            {
                HttpContext.Current.Session["multiparcel_CASanluisobispo"] = "Yes";
            }

        }

    }
}