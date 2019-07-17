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
namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_FLVolusia
    {
        string parcelno = "";
        string outputPath = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();

        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_FLVolusia(string houseno, string sname, string Dir, string account, string parcelNumber, string searchType, string orderno, string ownername, string directParcel)
        {
            GlobalClass.global_orderNo = orderno;
            HttpContext.Current.Session["orderNo"] = orderno;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string outparcelno = "", taxparcel = "", mailingaddress = "", tax_district = "", siteaddr = "", legal_desc = "", year_built = "", propuse = "", par1 = "", par2 = "";
            string Building_Value = "", LandValue = "", JustValue = "", cctaxyear = "", Cap = "", AssessedValue = "", Exemption = "", TaxableValue = "";
            string strBill = "-", strBalance = "-", strBillDate = "-", strBillPaid = "-";
            string taxowner = "", tax_addr = "", accno = "", alterkey = "", millagecode = "", milagerate = "", ctax_year = "", cpaiddate = "", cpaidamount = "", creceipt = "", combinedtaxamount = "", ceffdate = "", grosstax = "", ifpaidby = "", pleasepay = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new ChromeDriver();
            //driver = new PhantomJSDriver();
            //  string[] stringSeparators1 = new string[] { "\r\n" };         
            List<string> listurl = new List<string>();
            List<string> linkurl = new List<string>();
            string Date = "";
            List<string> strissuecertificate = new List<string>();
            List<string> taxhistorylink = new List<string>();
            List<string> downloadlink = new List<string>();
            List<string> taxhistorylinkinst = new List<string>();
            Date = DateTime.Now.ToString("M/d/yyyy");
            string Alternatekey = "", PropertyAddress = "", MailingAddress = "", PropertyType = "", YearBuilt = "", LegalDescription = "", FullParcelID = "", strbillyear = "";
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    if (searchType == "titleflex")
                    {
                        string address = houseno + " " + Dir + " " + sname;
                        gc.TitleFlexSearch(orderno, parcelNumber, "", address, "FL", "Volusia");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_FLVolusia"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("http://vcpa.vcgov.org/searches.html");
                        Thread.Sleep(2000);
                        driver.FindElement(By.XPath("//*[@id='bodysearch']/div[2]/input[2]")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        driver.FindElement(By.XPath("//*[@id='inpNo']")).SendKeys(houseno.Trim());
                        driver.FindElement(By.XPath("//*[@id='inpDir']")).SendKeys(Dir.Trim());
                        driver.FindElement(By.XPath("//*[@id='inpStreet']")).SendKeys(sname.Trim());
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='inpUnit']")).SendKeys(account.Trim());

                        }
                        catch { }
                        Thread.Sleep(1000);
                        gc.CreatePdf_WOP(orderno, "Address Search", driver, "FL", "Volusia");
                        driver.FindElement(By.XPath("//*[@id='btSearch']")).SendKeys(Keys.Enter);
                        Thread.Sleep(1000);
                        gc.CreatePdf_WOP(orderno, "Address Search Result", driver, "FL", "Volusia");
                        try
                        {
                            string Multi = driver.FindElement(By.XPath("//*[@id='frmMain']/table/tbody/tr/td/div/div/table[2]/tbody/tr/td[1]/table/tbody/tr[3]/td/center/table[1]/tbody/tr/td[3]")).Text;

                            try
                            {
                                string MultiCount = gc.Between(Multi, "Displaying 1 - ", " of");
                                if (Convert.ToInt16(MultiCount) >= 15)
                                {
                                    HttpContext.Current.Session["multiParcel_FlVolusia_Maximum"] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }
                            }
                            catch { }
                            IWebElement MultiTb = driver.FindElement(By.XPath("//*[@id='searchResults']/tbody"));

                            IList<IWebElement> MultiTR = MultiTb.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiTD;
                            string City = "";
                            foreach (IWebElement row1 in MultiTR)
                            {

                                MultiTD = row1.FindElements(By.TagName("td"));

                                if (MultiTD.Count != 0 && !row1.Text.Contains("AltKey") && row1.Text.Trim() != "")
                                {
                                    Alternatekey = MultiTD[1].Text;
                                    parcelNumber = MultiTD[2].Text;
                                    ownername = MultiTD[3].Text;
                                    PropertyAddress = MultiTD[4].Text;
                                    City = MultiTD[5].Text;

                                    string Multiparcel = Alternatekey + "~" + ownername + "~" + PropertyAddress + "~" + City;
                                    gc.insert_date(orderno, parcelNumber, 254, Multiparcel, 1, DateTime.Now);
                                    HttpContext.Current.Session["multiParcel_FlVolusia"] = "Yes";

                                }
                            }
                            return "MultiParcel";
                        }
                        catch { }

                    }

                    else if (searchType == "parcel")
                    {

                        driver.Navigate().GoToUrl("http://vcpa.vcgov.org/searches.html");
                        Thread.Sleep(2000);
                        driver.FindElement(By.XPath("//*[@id='bodysearch']/div[2]/input[2]")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        driver.FindElement(By.XPath("//*[@id='inpAltid']")).SendKeys(parcelNumber.Trim());

                        Thread.Sleep(1000);
                        gc.CreatePdf(orderno, parcelNumber, "Parcel Search", driver, "FL", "Volusia");
                        driver.FindElement(By.XPath("//*[@id='btSearch']")).SendKeys(Keys.Enter);
                        Thread.Sleep(1000);
                        gc.CreatePdf(orderno, parcelNumber, "Parcel Search Result", driver, "FL", "Volusia");
                    }
                    else if (searchType == "block")
                    {

                        driver.Navigate().GoToUrl("http://vcpa.vcgov.org/searches.html");
                        Thread.Sleep(2000);
                        driver.FindElement(By.XPath("//*[@id='bodysearch']/div[2]/input[2]")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        driver.FindElement(By.XPath("//*[@id='inpParid']")).SendKeys(account.Trim());

                        Thread.Sleep(1000);
                        gc.CreatePdf(orderno, parcelNumber, "AlterNate Key Search", driver, "FL", "Volusia");
                        driver.FindElement(By.XPath("//*[@id='btSearch']")).SendKeys(Keys.Enter);
                        Thread.Sleep(1000);
                        gc.CreatePdf(orderno, parcelNumber, "AlterNate Key Search Result", driver, "FL", "Volusia");
                    }
                    else if (searchType == "ownername")
                    {

                        driver.Navigate().GoToUrl("http://vcpa.vcgov.org/searches.html");
                        Thread.Sleep(2000);
                        driver.FindElement(By.XPath("//*[@id='bodysearch']/div[2]/input[2]")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        driver.FindElement(By.XPath("//*[@id='inpOwner1']")).SendKeys(ownername.Trim());

                        Thread.Sleep(1000);
                        gc.CreatePdf_WOP(orderno, "Ownername Search", driver, "FL", "Volusia");
                        driver.FindElement(By.XPath("//*[@id='btSearch']")).SendKeys(Keys.Enter);
                        gc.CreatePdf_WOP(orderno, "Ownename Search Result", driver, "FL", "Volusia");
                        try
                        {
                            string Multi = driver.FindElement(By.XPath("//*[@id='frmMain']/table/tbody/tr/td/div/div/table[2]/tbody/tr/td[1]/table/tbody/tr[3]/td/center/table[1]/tbody/tr/td[3]")).Text;
                            string MultiCount = gc.Between(Multi, "Displaying 1 - ", " of");
                            if (Convert.ToInt16(MultiCount) >= 15)
                            {
                                HttpContext.Current.Session["multiParcel_FlVolusia_Maximum"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                            if (Convert.ToInt16(MultiCount) <= 15)
                            {

                                IWebElement MultiTb = driver.FindElement(By.XPath("//*[@id='searchResults']/tbody"));
                                IList<IWebElement> MultiTR = MultiTb.FindElements(By.TagName("tr"));
                                IList<IWebElement> MultiTD;
                                string City = "";
                                foreach (IWebElement row1 in MultiTR)
                                {

                                    MultiTD = row1.FindElements(By.TagName("td"));

                                    if (MultiTD.Count != 0 && MultiTD.Count != 1)
                                    {
                                        if (!MultiTD[1].Text.Contains("AltKey"))

                                        {
                                            Alternatekey = MultiTD[1].Text;
                                            parcelNumber = MultiTD[2].Text;
                                            ownername = MultiTD[3].Text;
                                            PropertyAddress = MultiTD[4].Text;
                                            City = MultiTD[5].Text;

                                            string Multiparcel = Alternatekey + "~" + ownername + "~" + PropertyAddress + "~" + City;
                                            gc.insert_date(orderno, parcelNumber, 254, Multiparcel, 1, DateTime.Now);

                                            HttpContext.Current.Session["multiParcel_FlVolusia"] = "Yes";

                                        }

                                    }
                                }
                                driver.Quit();
                                return "MultiParcel";
                            }

                        }
                        catch { }
                    }

                    try
                    {
                        IWebElement Inodata = driver.FindElement(By.XPath("//*[@id='frmMain']/table/tbody/tr/td/div/div/table[2]/tbody/tr/td/table/tbody/tr[3]/td/center/table[1]"));
                        if(Inodata.Text.Contains("search did not find any records"))
                        {
                            HttpContext.Current.Session["Nodata_FLVolusia"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    string owner2 = "";
                    // string MailingAddress = "", PropertyType = "", YearBuilt = "", LegalDescription = "";
                    parcelNumber = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[1]/table[2]/tbody/tr[1]/td[2]")).Text;
                    FullParcelID = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[4]/table[2]/tbody/tr[3]/td[2]")).Text + "-" + driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[4]/table[2]/tbody/tr[4]/td[2]")).Text;

                    Alternatekey = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[1]/td/table/tbody/tr[1]/td[1]")).Text.Replace("Altkey:", "");
                    ownername = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[2]/table[2]/tbody/tr[1]/td[2]")).Text;
                    try
                    {
                        owner2 = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[2]/table[2]/tbody/tr[2]/td[2]")).Text;
                    }
                    catch { }
                    if (owner2.Trim() != "")
                    {
                        ownername = ownername + "&" + owner2;
                    }
                    PropertyAddress = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[1]/table[2]/tbody/tr[2]/td[2]")).Text;
                    MailingAddress = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[2]/table[2]/tbody/tr[4]/td[2]")).Text + driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[2]/table[2]/tbody/tr[6]/td[2]")).Text; ;
                    PropertyType = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[1]/table[2]/tbody/tr[3]/td[2]")).Text;
                    LegalDescription = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[4]/table[2]/tbody/tr[2]/td[2]")).Text;

                    driver.FindElement(By.XPath("/html/body/div/div[3]/div/nav/div/div/li[3]/a")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderno, parcelNumber, "Assessment Detail", driver, "FL", "Volusia");
                    try
                    {
                        YearBuilt = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[1]/table[2]/tbody/tr[6]/td[2]")).Text.Replace("/ 1977", "");
                    }
                    catch { }
                    string PropertyDetail = FullParcelID + "~" + Alternatekey + "~" + ownername + "~" + PropertyAddress + "~" + MailingAddress + "~" + PropertyType + "~" + YearBuilt + "~" + LegalDescription;
                    gc.insert_date(orderno, parcelNumber, 219, PropertyDetail, 1, DateTime.Now);
                    string AssessedYear = "", ImprValue = "", nonSchoolAssessed = "", NonSchoolAssessed_Exemptions = "", NonSch_taxable = "", HR_Savings = "";
                    driver.FindElement(By.XPath("//*[@id='sidemenu']/li[7]/a")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderno, parcelNumber, "Assessment Detail", driver, "FL", "Volusia");
                    string OK = "";
                    int i = 0;
                    IWebElement AsseTB = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody"));
                    IList<IWebElement> AsseTR = AsseTB.FindElements(By.TagName("tr"));
                    IList<IWebElement> AsseTD;

                    foreach (IWebElement row1 in AsseTR)
                    {

                        AsseTD = row1.FindElements(By.TagName("td"));

                        if (AsseTD.Count != 0 && AsseTD.Count != 1)
                        {

                            if (OK == "OK")
                            {

                                AssessedYear = AsseTD[0].Text;
                                LandValue = AsseTD[1].Text;
                                ImprValue = AsseTD[2].Text;
                                JustValue = AsseTD[3].Text;
                                nonSchoolAssessed = AsseTD[4].Text;
                                //NonSchoolAssessed_Exemptions = AsseTD[5].Text;
                                // NonSch_taxable = AsseTD[6].Text;
                                HR_Savings = AsseTD[5].Text;
                                if (i == 0)
                                {
                                    OK = "";
                                    i++;
                                }

                                string AsseDeatil = AssessedYear + "~" + LandValue + "~" + ImprValue + "~" + JustValue + "~" + nonSchoolAssessed + "~" + HR_Savings;
                                gc.insert_date(orderno, parcelNumber, 232, AsseDeatil, 1, DateTime.Now);
                            }
                            if (AsseTD[0].Text == "Year")
                            {
                                OK = "OK";

                            }

                        }
                    }

                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='sidemenu']/li[3]/a")).SendKeys(Keys.Enter);
                        gc.CreatePdf(orderno, parcelNumber, "Assessment Resedential", driver, "FL", "Volusia");
                    }
                    catch { }

                    //AssessedYear = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[3]/div[2]/dl[2]/dd[1]")).Text;

                    //LandValue = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[3]/div[2]/dl[2]/dd[3]")).Text;
                    //ImprValue = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[3]/div[2]/dl[2]/dd[4]")).Text;
                    //JustValue = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[3]/div[2]/dl[2]/dd[4]")).Text;
                    //nonSchoolAssessed = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[3]/div[2]/dl[2]/dd[4]")).Text;
                    //NonSchoolAssessed_Exemptions = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[3]/div[2]/dl[2]/dd[4]")).Text;
                    //NonSch_taxable = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[3]/div[2]/dl[2]/dd[4]")).Text;
                    //HR_Savings = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[3]/div[2]/dl[2]/dd[4]")).Text;

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("https://volusia.county-taxes.com/public");
                    Thread.Sleep(2000);

                    driver.FindElement(By.Id("search-text form-control")).SendKeys(parcelNumber.Trim());

                    Thread.Sleep(1000);
                    gc.CreatePdf(orderno, parcelNumber, "Tax Search", driver, "FL", "Volusia");
                    driver.FindElement(By.XPath("//*[@id='search-controls']/div/form[1]/div[1]/div/span/button")).SendKeys(Keys.Enter);
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderno, parcelNumber, "Tax Search Result", driver, "FL", "Volusia");


                    ////////////////////////////////////////////


                    //full bill history
                    IWebElement ITaxSearch = driver.FindElement(By.LinkText("Full bill history"));
                    string strITaxSearch = ITaxSearch.GetAttribute("href");
                    driver.Navigate().GoToUrl(strITaxSearch);
                    Thread.Sleep(3000);
                    taxparcel = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[1]")).Text;
                    taxparcel = GlobalClass.After(taxparcel, "Real Estate Account #");
                    gc.CreatePdf(orderno, outparcelno, "Full Bill History", driver, "FL", "Volusia");
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
                                        //  Tax Year~Installment~Total Due~Paid Date~Effective Date~Paid Amount~Receipt Number
                                        //billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                        billyear = IBillHistoryTD[0].Text;
                                        strBalance = IBillHistoryTD[1].Text.Replace("\r\n", " ");
                                        strBillDate = IBillHistoryTD[2].Text;
                                        paidamount = IBillHistoryTD[3].Text;
                                        string strTaxHistory2 = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                        gc.insert_date(orderno, taxparcel, 235, strTaxHistory2, 1, DateTime.Now);
                                    }

                                }
                                if (IBillHistoryTD.Count == 4 && IBillHistoryTD[0].Text.Trim() != "" && !bill.Text.Contains("Pay this bill"))
                                {
                                    //  Tax Year~Installment~Total Due~Paid Date~Effective Date~Paid Amount~Receipt Number
                                    //billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                    strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd().Replace("Installment Bill #", "").Replace("Bill", "");
                                    var Splitbill = strBill.Split(' ');
                                    billyear = Splitbill[0]; inst = Splitbill[1];
                                    strBalance = IBillHistoryTD[1].Text.Replace("\r\n", "").TrimStart().TrimEnd();
                                    // billyear = IBillHistoryTD[0].Text;
                                    // strBalance = IBillHistoryTD[1].Text.Replace("\r\n", " ");
                                    strBillDate = "";
                                    paidamount = "";
                                    string strTaxHistory2 = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                    gc.insert_date(orderno, taxparcel, 235, strTaxHistory2, 1, DateTime.Now);
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
                                    gc.insert_date(orderno, taxparcel, 235, strTaxHistory2, 1, DateTime.Now);
                                }
                                //if (IBillHistoryTD[3].Text.Contains("Paid"))
                                //{
                                //    inst = "Total";
                                //    paidamount = IBillHistoryTD[2].Text.Replace("Paid", "");
                                //    strBalance = "";
                                //    string strTaxHistory = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                //    gc.insert_date(orderno, taxparcel, 235, strTaxHistory, 1, DateTime.Now);
                                //}

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

                                                    BillDownload(orderno, parcelNumber, BillTax, taxparcel, strbillyear);
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
                                                    //gc.downloadfile(BillTax, orderno, taxparcel, fname, "FL", "Osceola");

                                                    BillDownload(orderno, parcelNumber, BillTax, taxparcel, strbillyear);
                                                }
                                                catch (Exception ex)
                                                {
                                                }
                                                if (!bill.Text.Contains("Pay this bill"))
                                                {
                                                    taxhistorylink.Add(taxlink);
                                                }

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
                                                    //gc.downloadfile(BillTax, orderno, taxparcel, "Osceola-County-Real-Estate-" + taxparcel + "-" + strBill + "-bill", "FL", "Osceola");

                                                    BillDownload(orderno, parcelNumber, BillTax, taxparcel, strbillyear);

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
                                                    //gc.downloadfile(BillTax, orderno, taxparcel, fname, "FL", "Osceola");

                                                    // View Bill 

                                                    BillDownload(orderno, parcelNumber, BillTax, taxparcel, strbillyear);

                                                }
                                                catch (Exception ex)
                                                {
                                                }
                                                if (!bill.Text.Contains("Pay this bill"))
                                                {
                                                    taxhistorylink.Add(taxlink);
                                                }
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
                                                        // gc.downloadfile(BillTax, orderno, taxparcel, "Osceola-County-Real-Estate-" + taxparcel + "-" + strBill + "-bill", "FL", "Osceola");
                                                        // View Bill

                                                        BillDownload(orderno, parcelNumber, BillTax, taxparcel, strbillyear);


                                                    }
                                                    catch { }
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                                        strbillyear = GlobalClass.Before(ITaxBillCount.Text, " Installment Bill");
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        downloadlink.Add(BillTax);
                                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                                        // gc.downloadfile(BillTax, orderno, taxparcel, "Osceola-County-Real-Estate-" + taxparcel + " - " + strBill + " -bill", "FL", "Osceola");
                                                        // View Bill 

                                                        BillDownload(orderno, parcelNumber, BillTax, taxparcel, strbillyear);


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
                                                        //gc.downloadfile(BillTax, orderno, taxparcel, "Osceola-County-Real-Estate-" + taxparcel + "-" + billyear + "-Annual-bill", "FL", "Osceola");

                                                        BillDownload(orderno, parcelNumber, BillTax, taxparcel, strbillyear);


                                                    }
                                                    catch { }
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                                        strbillyear = GlobalClass.Before(ITaxBillCount.Text, " Installment Bill");
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        downloadlink.Add(BillTax);
                                                        //gc.downloadfile(BillTax, orderno, taxparcel, "Osceola-County-Real-Estate-" + taxparcel + " - " + billyear + " -Annual-bill", "FL", "Osceola");

                                                        BillDownload(orderno, parcelNumber, BillTax, taxparcel, strbillyear);


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
                                                        //gc.downloadfile(BillTax, orderno, taxparcel, "Osceola-County-Real-Estate-" + taxparcel + "-" + billyear + "-Annual-bill", "FL", "Osceola");
                                                        // View Bill 
                                                        BillDownload(orderno, parcelNumber, BillTax, taxparcel, strbillyear);
                                                        //
                                                    }
                                                    catch { }
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                                        strbillyear = GlobalClass.Before(ITaxBillCount.Text, " Installment Bill");
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        downloadlink.Add(BillTax);
                                                        //gc.downloadfile(BillTax, orderno, taxparcel, "Osceola-County-Real-Estate-" + taxparcel + " - " + billyear + " -Annual-bill", "FL", "Osceola");
                                                        // View Bill 
                                                        BillDownload(orderno, parcelNumber, BillTax, taxparcel, strbillyear);

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
                                                strbillyear = GlobalClass.Before(bill.Text, " Annual Bill");
                                            }
                                            catch { }
                                            try
                                            {
                                                if (strbillyear.Trim() == "")
                                                {
                                                    strbillyear = GlobalClass.Before(bill.Text, " Installment Bill");
                                                }
                                            }
                                            catch { }
                                            string BillTax = ITaxBill.GetAttribute("href");
                                            strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                            downloadlink.Add(BillTax);
                                            // Osceola-County-Real-Estate-R232529-348000020100-2017-Annual-bill
                                            //gc.downloadfile(BillTax, orderno, taxparcel, "Osceola-County-Real-Estate-" + taxparcel + "-" + billyear + "-Annual-bill", "FL", "Osceola");
                                            BillDownload(orderno, parcelNumber, BillTax, taxparcel, strbillyear);

                                        }
                                        catch { }
                                        try
                                        {
                                            IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                            string BillTax = ITaxBill.GetAttribute("href");
                                            downloadlink.Add(BillTax);
                                            //    gc.downloadfile(BillTax, orderno, taxparcel, "Osceola-County-Real-Estate-" + taxparcel + " - " + billyear + " -Annual-bill", "FL", "Osceola");
                                            BillDownload(orderno, parcelNumber, BillTax, taxparcel, strbillyear);
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
                                        gc.insert_date(orderno, taxparcel, 235, strTaxHistory, 1, DateTime.Now);

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
                                        gc.insert_date(orderno, taxparcel, 235, strTaxHistory, 1, DateTime.Now);

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
                                    gc.insert_date(orderno, taxparcel, 235, strTaxHistory, 1, DateTime.Now);
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
                                gc.insert_date(orderno, taxparcel, 235, strTaxHistory, 1, DateTime.Now);
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
                            gc.CreatePdf(orderno, outparcelno, "Tax Details" + cctaxyear, driver, "FL", "Volusia");

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
                                                gc.insert_date(orderno, taxparcel, 236, valoremtax, 1, DateTime.Now);
                                            }

                                            else if (valoremtablerowTD.Count == 2)
                                            {
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                                gc.insert_date(orderno, taxparcel, 236, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        else if (d == valoremtablerowcount - 1)
                                        {
                                            milagerate = valoremtablerowTD[0].Text;
                                            valoremtax = "Total" + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + valoremtablerowTD[1].Text;
                                            gc.insert_date(orderno, taxparcel, 236, valoremtax, 1, DateTime.Now);
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
                                                gc.insert_date(orderno, taxparcel, 236, valoremtax, 1, DateTime.Now);
                                            }
                                            else if (e == nonvaloremtablerowcount - 1)
                                            {
                                                valoremtax = "Total" + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[0].Text;
                                                gc.insert_date(orderno, taxparcel, 236, valoremtax, 1, DateTime.Now);
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
                                    gc.insert_date(orderno, taxparcel, 237, single, 1, DateTime.Now);
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
                                                    gc.insert_date(orderno, taxparcel, 237, DueDate, 1, DateTime.Now);

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
                            //gc.CreatePdf(orderno, outparcelno, "Parcel Details" + cctaxyear, driver,  "FL", "Osceola");


                            //Account Number~Alternate Key~Owner Name & Mailing Address~Property Address~Millage Code~Millage rate~Tax Year~Tax Amount~Gross Tax~Paid Amount~Paid Date~Effective Date~Receipt Number
                            string curtax = alterkey + "~" + taxowner + "~" + tax_addr + "~" + millagecode + "~" + milagerate + "~" + cctaxyear + "~" + combinedtaxamount + "~" + grosstax + "~" + cpaidamount + "~" + cpaiddate + "~" + ceffdate + "~" + creceipt;
                            gc.insert_date(orderno, taxparcel, 238, curtax, 1, DateTime.Now);
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
                            gc.CreatePdf(orderno, outparcelno, "Tax Details" + cctaxyear, driver, "FL", "Volusia");

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
                                                gc.insert_date(orderno, taxparcel, 236, valoremtax, 1, DateTime.Now);
                                            }

                                            else if (valoremtablerowTD.Count == 2)
                                            {
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                                gc.insert_date(orderno, taxparcel, 236, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        else if (d == valoremtablerowcount - 1)
                                        {
                                            milagerate = valoremtablerowTD[0].Text;
                                            valoremtax = "Total" + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + valoremtablerowTD[1].Text;
                                            gc.insert_date(orderno, taxparcel, 236, valoremtax, 1, DateTime.Now);
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
                                                gc.insert_date(orderno, taxparcel, 236, valoremtax, 1, DateTime.Now);
                                            }
                                            else if (e == nonvaloremtablerowcount - 1)
                                            {
                                                valoremtax = "Total" + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[0].Text;
                                                gc.insert_date(orderno, taxparcel, 236, valoremtax, 1, DateTime.Now);
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
                                    gc.insert_date(orderno, taxparcel, 237, single, 1, DateTime.Now);
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
                                                    gc.insert_date(orderno, taxparcel, 237, DueDate, 1, DateTime.Now);

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
                            //gc.CreatePdf(orderno, outparcelno, "Parcel Details" + cctaxyear, driver,  "FL", "Osceola");


                            //Account Number~Alternate Key~Owner Name & Mailing Address~Property Address~Millage Code~Millage rate~Tax Year~Tax Amount~Gross Tax~Paid Amount~Paid Date~Effective Date~Receipt Number
                            string curtax = alterkey + "~" + taxowner + "~" + tax_addr + "~" + millagecode + "~" + milagerate + "~" + cctaxyear + "~" + combinedtaxamount + "~" + grosstax + "~" + cpaidamount + "~" + cpaiddate + "~" + ceffdate + "~" + creceipt;
                            gc.insert_date(orderno, taxparcel, 238, curtax, 1, DateTime.Now);
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
                            gc.CreatePdf(orderno, outparcelno, "Tax Details" + cctaxyear, driver, "FL", "Volusia");

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
                                                gc.insert_date(orderno, taxparcel, 236, valoremtax, 1, DateTime.Now);
                                            }

                                            else if (valoremtablerowTD.Count == 2)
                                            {
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                                gc.insert_date(orderno, taxparcel, 236, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        else if (d == valoremtablerowcount - 1)
                                        {
                                            milagerate = valoremtablerowTD[0].Text;
                                            valoremtax = "Total" + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + valoremtablerowTD[1].Text;
                                            gc.insert_date(orderno, taxparcel, 236, valoremtax, 1, DateTime.Now);
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
                                                gc.insert_date(orderno, taxparcel, 236, valoremtax, 1, DateTime.Now);
                                            }
                                            else if (e == nonvaloremtablerowcount - 1)
                                            {
                                                valoremtax = "Total" + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[0].Text;
                                                gc.insert_date(orderno, taxparcel, 236, valoremtax, 1, DateTime.Now);
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
                                    gc.insert_date(orderno, taxparcel, 237, single, 1, DateTime.Now);
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
                                                    gc.insert_date(orderno, taxparcel, 237, DueDate, 1, DateTime.Now);

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
                            //gc.CreatePdf(orderno, outparcelno, "Parcel Details" + cctaxyear, driver,  "FL", "Osceola");


                            //Account Number~Alternate Key~Owner Name & Mailing Address~Property Address~Millage Code~Millage rate~Tax Year~Tax Amount~Gross Tax~Paid Amount~Paid Date~Effective Date~Receipt Number
                            string curtax = alterkey + "~" + taxowner + "~" + tax_addr + "~" + millagecode + "~" + milagerate + "~" + cctaxyear + "~" + combinedtaxamount + "~" + grosstax + "~" + cpaidamount + "~" + cpaiddate + "~" + ceffdate + "~" + creceipt;
                            gc.insert_date(orderno, taxparcel, 238, curtax, 1, DateTime.Now);
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
                            gc.CreatePdf(orderno, outparcelno, "Tax Details" + cctaxyear, driver, "FL", "Volusia");

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
                                                gc.insert_date(orderno, taxparcel, 236, valoremtax, 1, DateTime.Now);
                                            }

                                            else if (valoremtablerowTD.Count == 2)
                                            {
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                                gc.insert_date(orderno, taxparcel, 236, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        else if (d == valoremtablerowcount - 1)
                                        {
                                            milagerate = valoremtablerowTD[0].Text;
                                            valoremtax = "Total" + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + valoremtablerowTD[1].Text;
                                            gc.insert_date(orderno, taxparcel, 236, valoremtax, 1, DateTime.Now);
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
                                                gc.insert_date(orderno, taxparcel, 236, valoremtax, 1, DateTime.Now);
                                            }
                                            else if (e == nonvaloremtablerowcount - 1)
                                            {
                                                valoremtax = "Total" + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[0].Text;
                                                gc.insert_date(orderno, taxparcel, 236, valoremtax, 1, DateTime.Now);
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
                                    gc.insert_date(orderno, taxparcel, 237, single, 1, DateTime.Now);
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
                                                    gc.insert_date(orderno, taxparcel, 237, DueDate, 1, DateTime.Now);

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
                            //gc.CreatePdf(orderno, outparcelno, "Parcel Details" + cctaxyear, driver,  "FL", "Osceola");


                            //Account Number~Alternate Key~Owner Name & Mailing Address~Property Address~Millage Code~Millage rate~Tax Year~Tax Amount~Gross Tax~Paid Amount~Paid Date~Effective Date~Receipt Number
                            string curtax = alterkey + "~" + taxowner + "~" + tax_addr + "~" + millagecode + "~" + milagerate + "~" + cctaxyear + "~" + combinedtaxamount + "~" + grosstax + "~" + cpaidamount + "~" + cpaiddate + "~" + ceffdate + "~" + creceipt;
                            gc.insert_date(orderno, taxparcel, 238, curtax, 1, DateTime.Now);
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
                            gc.CreatePdf(orderno, outparcelno, "Issue Certificate Details" + issueyear, driver, "FL", "Volusia");
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
                            gc.insert_date(orderno, taxparcel, 243, isscer, 1, DateTime.Now);
                        }
                        catch { }
                    }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Quit();

                    gc.mergpdf(orderno, "FL", "Volusia");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderno, "FL", "Volusia", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    return "Data Inserted Successfully";

                }
                catch (Exception ex)
                {
                    driver.Quit();
                    GlobalClass.LogError(ex, orderno);
                    throw ex;

                }
            }
        }
        public void BillDownload(string orderno, string parcelNumber, string BillTax, string taxparcel, string strbillyear)
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
                    fileName = "County-of-Volusia-Real-Estate-" + taxparcel + "-" + strbillyear + "-Annual-bill.pdf";
                    gc.AutoDownloadFile(orderno, parcelNumber, "FL", "Volusia", fileName);
                }
                catch { }

                try
                {
                    fileName = "County-of-Volusia-Real-Estate-" + taxparcel + "-" + strbillyear + "-Installment-bill-1.pdf";
                    gc.AutoDownloadFile(orderno, parcelNumber, "FL", "Volusia", fileName);
                }
                catch { }
                try
                {
                    fileName = "County-of-Volusia-Real-Estate-" + taxparcel + "-" + strbillyear + "-Installment-bill-2.pdf";
                    gc.AutoDownloadFile(orderno, parcelNumber, "FL", "Volusia", fileName);
                }
                catch { }
                try
                {
                    fileName = "County-of-Volusia-Real-Estate-" + taxparcel + "-" + strbillyear + "-Installment-bill-3.pdf";
                    gc.AutoDownloadFile(orderno, parcelNumber, "FL", "Volusia", fileName);
                }
                catch { }

                try
                {
                    fileName = "County-of-Volusia-Real-Estate-" + taxparcel + "-" + strbillyear + "-Installment-bill-4.pdf";
                    gc.AutoDownloadFile(orderno, parcelNumber, "FL", "Volusia", fileName);
                }
                catch { }

                chDriver.Quit();
            }
            catch (Exception ex)
            {

            }
        }
































        /////////////////////////////////////////////


        /*
    driver.FindElement(By.XPath("//*[@id='results']/div[2]/div/div[4]/div/ul/li[1]/a")).Click();
            Thread.Sleep(2000);
            string AccountNumber = "", MillageCode = "", Millagerate = "", TaxYear = "", TaxAmount = "", PaidAmount = "", PaidDate = "", EffectiveDate = "", ReceiptNumber = "";

            try
            {

                AccountNumber = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[3]/div[2]/dl[2]/dd[1]")).Text;

                MillageCode = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[3]/div[2]/dl[2]/dd[3]")).Text;
                Millagerate = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[3]/div[2]/dl[2]/dd[4]")).Text;

                driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[1]/ul/li[2]/a")).Click();
                Thread.Sleep(2000);
                TaxYear = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[3]/div[2]/ul/li[1]/a")).Text;
                TaxAmount = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/p")).Text.Replace("Combined taxes and assessments:", "");
            }
            catch
            { }
                try
            {
                PaidAmount = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[6]/div/form/div/div/div")).Text;
                var SplitAmount = PaidAmount.Split(' ');
                PaidAmount = SplitAmount[3].Replace("\r\nReceipt", "");
                PaidDate = SplitAmount[1];
                EffectiveDate = "";
                try {
                    ReceiptNumber = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[7]/div/div[6]/div/form/div/div/div/div")).Text.Replace("Receipt", "");
                }
                catch { }
                try
                {
                    ReceiptNumber = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[8]/div/div[6]/div/form/div/div/div/div")).Text.Replace("Receipt", "");
                }
                catch { }
            }
            catch { }

            string TaxDetail = AccountNumber + "~" + Alternatekey + "~" + ownername + "~" + PropertyAddress + "~" + MailingAddress + "~" + MillageCode + "~" + Millagerate + "~" + TaxYear + "~" + TaxAmount + "~" + PaidAmount + "~" + PaidDate + "~" + EffectiveDate + "~" + ReceiptNumber ;
            gc.insert_date(orderno, parcelNumber, 235, TaxDetail, 1, DateTime.Now);

            string Authority = "", TaxType = "",TaxTypeNonAd="", Millage = "", Assessed = "", Exemption = "", Taxable = "", Tax = "";
            int k = 0;

            try
            {
                IWebElement TaxDisTB = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[8]/div/table[2]"));
                IList<IWebElement> TaxDisTR = TaxDisTB.FindElements(By.TagName("tr"));
                IList<IWebElement> TaxDisTD;
                foreach (IWebElement row1 in TaxDisTR)
                {
                    TaxDisTD = row1.FindElements(By.TagName("td"));
                    if (TaxDisTD.Count != 0 && TaxDisTD.Count != 1)
                    {
                        if (TaxDisTD.Count == 2)
                        {
                            Authority = "Total";                                
                            Millage = TaxDisTD[0].Text;
                            Assessed = "";
                            Exemption = "";
                            Taxable = "";
                            Tax = TaxDisTD[1].Text;
                            try
                            {
                                try
                                {
                                    TaxType = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[8]/div/table[2]/caption")).Text;
                                }
                                catch { }
                                if (TaxType.Trim() == "" && TaxType.Trim() == null)
                                {
                                    TaxType = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[2]/caption")).Text;
                                }
                            }
                            catch { }
                        }
                        else
                        {
                            Authority = TaxDisTD[0].Text;
                            Millage = TaxDisTD[1].Text;
                            Assessed = TaxDisTD[2].Text;
                            Exemption = TaxDisTD[3].Text;
                            Taxable = TaxDisTD[4].Text;
                            Tax = TaxDisTD[5].Text;
                            try
                            {
                                try
                                {
                                    TaxType = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[8]/div/table[2]/caption")).Text;
                                }
                                catch { }
                                if (TaxType.Trim() == "" && TaxType.Trim() == null)
                                {
                                    TaxType = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[2]/caption")).Text;
                                }
                            }
                            catch { }
                        }

                        string TaxDisDetail = Authority + "~" + TaxType + "~" + Millage + "~" + Assessed + "~" + Exemption + "~" + Taxable + "~" + Tax + "~" + "" + "~";
                        gc.insert_date(orderno, parcelNumber, 236, TaxDisDetail, 1, DateTime.Now);
                        //Authority~TaxType~Millage~Assessed~Exemption~Taxable~Tax~Rate~Amount
                    }
                }
            }
            catch
            { }

            try
            {
                IWebElement TaxDisTB = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[7]/div/table[2]"));
                IList<IWebElement> TaxDisTR = TaxDisTB.FindElements(By.TagName("tr"));
                IList<IWebElement> TaxDisTD;
                foreach (IWebElement row1 in TaxDisTR)
                {
                    TaxDisTD = row1.FindElements(By.TagName("td"));

                    if (TaxDisTD.Count != 0 && TaxDisTD.Count != 1)
                    {
                        if (TaxDisTD.Count == 2)
                        {
                            Authority = "Total";
                            Millage = TaxDisTD[0].Text;
                            Assessed = "";
                            Exemption = "";
                            Taxable = "";
                            Tax = TaxDisTD[1].Text;
                            try
                            {
                                try
                                {
                                    TaxType = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[8]/div/table[2]/caption")).Text;
                                }
                                catch { }
                                if (TaxType.Trim() == "")
                                {
                                    TaxType = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[2]/caption")).Text;
                                }
                            }
                            catch { }
                        }
                        else
                        {
                            Authority = TaxDisTD[0].Text;
                            Millage = TaxDisTD[1].Text;
                            Assessed = TaxDisTD[2].Text;
                            Exemption = TaxDisTD[3].Text;
                            Taxable = TaxDisTD[4].Text;
                            Tax = TaxDisTD[5].Text;
                            try
                            {
                                try
                                {
                                    TaxType = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[8]/div/table[2]/caption")).Text;
                                }
                                catch { }
                                if (TaxType.Trim() == "")
                                {
                                    TaxType = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[2]/caption")).Text;
                                }
                            }
                            catch { }
                        }

                        string TaxDisDetail = Authority + "~" + TaxType + "~" + Millage + "~" + Assessed + "~" + Exemption + "~" + Taxable + "~" + Tax + "~" + "" + "~";
                        gc.insert_date(orderno, parcelNumber, 236, TaxDisDetail, 1, DateTime.Now);
                        TaxType = "";
                        //Authority~TaxType~Millage~Assessed~Exemption~Taxable~Tax~Rate~Amount
                    }
                }
            }
            catch
            { }

            //Non Ad Valorem
            try
            {
                try
                {
                    TaxTypeNonAd = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[3]/caption")).Text;
                }
                catch { }

                IWebElement INonAdTable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[3]/tbody"));
                IList<IWebElement> INonAdRow = INonAdTable.FindElements(By.TagName("tr"));
                IList<IWebElement> INonAdTd;
                foreach (IWebElement Nonad in INonAdRow)
                {
                    INonAdTd = Nonad.FindElements(By.TagName("td"));
                    if (INonAdTd.Count != 0)
                    {
                        string strAuthority = INonAdTd[0].Text;
                        string strRate = INonAdTd[1].Text;
                        string strAmount = INonAdTd[2].Text;

                        string strNonAd = strAuthority + "~" + TaxTypeNonAd + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + strRate + "~" + strAmount;
                        gc.insert_date(orderno, parcelNumber, 236, strNonAd, 1, DateTime.Now);
                        //Authority~TaxType~Millage~Assessed~Exemption~Taxable~Tax~Rate~Amount
                    }
                }

                IWebElement INonTable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[3]/tfoot"));
                IList<IWebElement> INonRow = INonTable.FindElements(By.TagName("tr"));
                IList<IWebElement> INonTd;
                foreach (IWebElement Non in INonRow)
                {
                    INonTd = Non.FindElements(By.TagName("td"));
                    if (INonTd.Count != 0 || Non.Text.Contains("Total"))
                    {
                        if (Non.Text.Contains("$") && INonTd.Count == 1)
                        {
                            string strAmount = INonTd[0].Text;

                            string strNonAd = "Total" + "~" + TaxTypeNonAd + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + strAmount;
                            gc.insert_date(orderno, parcelNumber, 236, strNonAd, 1, DateTime.Now);
                        }
                        else if (Non.Text.Contains("$") && INonTd.Count == 2)
                        {
                            string strRate = INonTd[0].Text;
                            string strAmount = INonTd[1].Text;

                            string strNonAd = "Total" + "~" + TaxTypeNonAd + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + strRate + "~" + strAmount;
                            gc.insert_date(orderno, parcelNumber, 236, strNonAd, 1, DateTime.Now);
                        }
                        else if (Non.Text.Contains("No non-ad valorem assessments."))
                        {
                            string strNodata = INonTd[0].Text;

                            string strNonAd = strNodata + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                            gc.insert_date(orderno, parcelNumber, 236, strNonAd, 1, DateTime.Now);
                        }
                    }
                }
            }
            catch { }

            string IfPaidBy = "", PlesePay = "";
            try {
                IfPaidBy = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]/tbody/tr/td")).Text.Replace("\r\n", "~");
            }
            catch { }
            try
            {
                PlesePay = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]/tbody/tr/td")).Text;
            }
            catch { }
            try
            {
                IfPaidBy = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]/tbody/tr/td")).Text.Replace("\r\n", "~");
            }
            catch { }
            try
            {
                PlesePay = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]/tbody/tr/td")).Text;
            }
            catch { }
            var IfpaySplit = IfPaidBy.Split('~');
            IfPaidBy = IfpaySplit[0];
            PlesePay = IfpaySplit[1];

            try
            {
                driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[3]/div[1]/ul/li[3]/a")).Click();
                Thread.Sleep(2000);
            }
            catch
            {
                driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[1]/ul/li[3]/a")).Click();
                Thread.Sleep(2000);
            }

            gc.CreatePdf(orderno, parcelNumber, "Full Bill History", driver, "FL", "Volusia");
            IWebElement TaxHisTB = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table"));
            string BillType = "", TotalDue = "",totalgroup="";
            IList<IWebElement> TaxHisTR = TaxHisTB.FindElements(By.TagName("tr"));
            IList<IWebElement> TaxHisTD;
            IList<IWebElement> TaxHisSt;
            IList<IWebElement> TaxHisA;

            foreach (IWebElement row1 in TaxHisTR)
            {

                TaxHisTD = row1.FindElements(By.TagName("td"));
                if (TaxHisTD.Count!=0)
                {
                    //if (TaxHisTD[0].Text.Contains("Redeemed certificate"))
                    //{
                    //    foreach (IWebElement row in TaxHisTD)
                    //    {
                    //        TaxHisA = row.FindElements(By.TagName("a"));
                    //        foreach (IWebElement A in TaxHisA)
                    //        {
                    //            listurl.Add(TaxHisA[0].GetAttribute("href"));
                    //        }
                    //    }
                    //}
                    if (row1.Text.Contains("Issued certificate"))
                    {
                        foreach (IWebElement row in TaxHisTD)
                        {
                            TaxHisA = row.FindElements(By.TagName("a"));
                            foreach (IWebElement A in TaxHisA)
                            {
                                listurl.Add(TaxHisA[0].GetAttribute("href"));
                            }

                        }
                    }
                }

                    if (TaxHisTD.Count != 0 && TaxHisTD.Count != 1 && TaxHisTD.Count != 2 && TaxHisTD.Count != 3)
                    {
                    TaxYear = TaxHisTD[0].Text;
                    try
                    {
                        IWebElement Ilink = TaxHisTD[0].FindElement(By.TagName("a"));
                        string strLink = Ilink.GetAttribute("href");
                        if ((Ilink.Text.Contains("Annual Bill") || Ilink.Text.Contains("Annual bill")) && k < 3)
                        {
                            linkurl.Add(strLink);
                            k++;
                        }
                    }
                    catch { }
                    var TaxYearSplit = TaxYear.Split(' ');
                    TaxYear = TaxYearSplit[0];
                    BillType = TaxYearSplit[1]+TaxYearSplit[2];

                    try
                    {
                        TotalDue = TaxHisTD[1].Text;
                        if (TaxHisTD[2].Text.Contains("Effective"))
                        {
                            var Paiddatesplit = TaxHisTD[2].Text.Split(' ');

                            PaidDate = Paiddatesplit[0].Replace("\r\nEffective", "");
                            EffectiveDate = Paiddatesplit[1];

                        }
                        else
                        {
                            PaidDate = TaxHisTD[2].Text;
                        }
                    }
                    catch { }

                    totalgroup = TaxHisTD[3].Text;
                    var Group = totalgroup.Split(' ');
                    if (totalgroup != "Certificate redeemed" && totalgroup != "Print (PDF)" && totalgroup != "Certificate issued")
                    {
                        PaidAmount = Group[1];
                        ReceiptNumber = Group[3];

                        string TaxHisDetail = TaxYear + "~" + BillType + "~" + TotalDue + "~" + PaidDate + "~" + EffectiveDate + "~" + PaidAmount + "~" + ReceiptNumber;
                        gc.insert_date(orderno, parcelNumber, 237, TaxHisDetail, 1, DateTime.Now);
                    }
                    if(totalgroup == "Certificate redeemed" || totalgroup == "Certificate issued")
                    {
                        string TaxHisDetail = TaxYear + "~" + BillType + "~" + TotalDue + "~" + PaidDate + "~" + EffectiveDate + "~" + PaidAmount + "~" + ReceiptNumber;
                        gc.insert_date(orderno, parcelNumber, 237, TaxHisDetail, 1, DateTime.Now);
                    }

                    if (PaidDate.Contains("Pay this bill:"))
                    {
                        string TaxHisDetail = TaxYear + "~" + BillType + "~" + TotalDue + "~" + PaidDate + "~" + EffectiveDate + "~" + PaidAmount + "~" + ReceiptNumber;
                        gc.insert_date(orderno, parcelNumber, 237, TaxHisDetail, 1, DateTime.Now);
                    }

                    TaxYear = ""; BillType = ""; TotalDue = ""; PaidDate = ""; EffectiveDate = ""; PaidAmount = ""; ReceiptNumber ="";
                }

            }

            string DueDate = IfPaidBy + "~" + PlesePay ;
            gc.insert_date(orderno, parcelNumber, 238, DueDate, 1, DateTime.Now);
            foreach (string ULR in listurl)
            {
                try
                {

                    driver.Navigate().GoToUrl(ULR);
                    Thread.Sleep(3000);

                    string AdvertisedNumber = "", FaceAmount = "", IssuedDate = "", ExpirationDate = "", Buyer = "", InterestRate = "";

                    AdvertisedNumber = driver.FindElement(By.XPath("//*[@id='content']/div[1]/dl/dd[1]")).Text.Replace("Advertised number", "");
                    FaceAmount = driver.FindElement(By.XPath("//*[@id='content']/div[1]/dl/dd[2]")).Text.Replace("Face amount", "");
                    IssuedDate = driver.FindElement(By.XPath("//*[@id='content']/div[1]/dl/dd[3]")).Text.Replace("Issued date", "");
                    string pdfdate = IssuedDate.Replace("/", "");
                    ExpirationDate = driver.FindElement(By.XPath("//*[@id='content']/div[1]/dl/dd[4]")).Text.Replace("Expiration date", "");
                    Buyer = driver.FindElement(By.XPath("//*[@id='content']/div[1]/dl/dd[5]")).Text.Replace("Buyer", "").Replace("\r\n", "");
                    InterestRate = driver.FindElement(By.XPath("//*[@id='content']/div[1]/dl/dd[6]")).Text.Replace("Interest rate", "");

                    gc.CreatePdf(orderno, parcelNumber, "Certificate Issued" + pdfdate, driver, "FL", "Volusia");
                    string TaxSaleDetails = AdvertisedNumber + "~" + FaceAmount + "~" + IssuedDate + "~" + ExpirationDate + "~" + Buyer + "~" + InterestRate;
                    gc.insert_date(orderno, parcelNumber, 243, TaxSaleDetails, 1, DateTime.Now);
                }
                catch { }
            }

            foreach (string strlinkURL in linkurl)
            {
                driver.Navigate().GoToUrl(strlinkURL);
                string strType = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[1]/div[2]")).Text;
                string StrTaxType = gc.Between(strType, "Real Estate ", " Annual Bill");
                if (strType.Contains("Real Estate"))
                {
                    if(strType.Contains("Annual Bill"))
                    {
                        gc.CreatePdf(orderno, parcelNumber, "Tax Latest Bill" + StrTaxType, driver, "FL", "Volusia");
                        IWebElement Ibilldownload = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[1]/div[2]/form"));
                        string strBillDownload = Ibilldownload.GetAttribute("action");
                        gc.downloadfile(strBillDownload, orderno, parcelNumber, "Tax bill" + StrTaxType, "FL", "Volusia");
                        driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[3]/div[1]/ul/li[1]/a")).SendKeys(Keys.Enter);
                        gc.CreatePdf(orderno, parcelNumber, "Tax Parcel Details" + StrTaxType, driver, "FL", "Volusia");
                    }
                }
            }


            TaxTime = DateTime.Now.ToString("HH:mm:ss");

            LastEndTime = DateTime.Now.ToString("HH:mm:ss");

            gc.insert_TakenTime(orderno, "FL", "Volusia", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);


            driver.Quit();
            gc.mergpdf(orderno, "FL", "Volusia");
            //gc.MMREM_Template(orderno, parcelNumber, "", driver, "FL", "Volusia", "80", "4");
            return "Data Inserted Successfully";

        }
        catch (Exception ex1)
        {
            driver.Quit();
            throw ex1;
        }

    }
    */
    }
}