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
using System.ComponentModel;
using System.Text;
using HtmlAgilityPack;
using iTextSharp.text;
using System.Text.RegularExpressions;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Support.Extensions;
using System.Net;
using OpenQA.Selenium.Firefox;
using System.Diagnostics;
using OpenQA.Selenium.Remote;
using Org.BouncyCastle.Utilities;

namespace ScrapMaricopa.Scrapsource
{
    public class Webdriver_MercedCA
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();
        IWebElement multiparceladd;
        public string FTP_MercedCA(string Address, string ownernm, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string Parcel_number = "", Tax_Authority = "", YearBuilt = "", Taxresult1 = "", taxresult2 = "", taxresult3 = "", Firsthalf = "", Secondhalf = "", fullhalf = "", prepayresult1 = "", prepayresult2 = "", prepayresult3 = "", paiedamt = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new ChromeDriver();
            // driver = new PhantomJSDriver();
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("https://common1.mptsweb.com/megabytecommonsite/(S(ydnzikonkwvfqbmoevuijk25))/PublicInquiry/Inquiry.aspx?CN=merced&SITE=Public&DEPT=Asr&PG=Search");
                    if (searchType == "titleflex")
                    {
                        //string address = houseno + " " + direction + " " + streetname + " " + streettype;
                        gc.TitleFlexSearch(orderNumber, "", ownernm, "", "CA", "Merced");
                        if (HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes")
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_MercedCA"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString().Replace("-", "");
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        driver.FindElement(By.XPath("/html/body/form/center/table/tbody/tr/td/table/tbody/tr[5]/td[3]/input")).SendKeys(Address);
                        IWebElement ISpan12 = driver.FindElement(By.XPath("/html/body/form/center/table/tbody/tr/td/p/input[1]"));
                        IJavaScriptExecutor js12 = driver as IJavaScriptExecutor;
                        js12.ExecuteScript("arguments[0].click();", ISpan12);
                        Thread.Sleep(4000);
                        try
                        {
                            int max = 0; string multiaddstring = "";
                            gc.CreatePdf_WOP(orderNumber, "MultyAddressSearch", driver, "CA", "Merced");
                            IWebElement Multyaddresstable = driver.FindElement(By.XPath("/html/body/form/center/table/tbody/tr/td/table/tbody"));
                            IList<IWebElement> multiaddressrow = Multyaddresstable.FindElements(By.TagName("tr"));
                            IList<IWebElement> multiaddressid;
                            foreach (IWebElement Multiaddress in multiaddressrow)
                            {
                                multiaddressid = Multiaddress.FindElements(By.TagName("td"));
                                if (multiaddressid.Count != 0 && Multiaddress.Text.Trim() != "" && !Multiaddress.Text.Contains("Fee Parcel"))
                                {
                                    multiparceladd = multiaddressid[0].FindElement(By.TagName("a"));
                                    multiaddstring = multiparceladd.GetAttribute("href");
                                    string multiaddressresult = multiaddressid[1].Text + "~" + multiaddressid[2].Text;
                                    gc.insert_date(orderNumber, multiaddressid[0].Text, 1020, multiaddressresult, 1, DateTime.Now);
                                    max++;
                                }
                            }
                            if (max == 1)
                            {
                                driver.Navigate().GoToUrl(multiaddstring);
                                Thread.Sleep(2000);
                            }
                            if (max > 1 && max < 26)
                            {
                                HttpContext.Current.Session["multiparcel_Merced"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (max > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Merced_Multicount"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }

                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.XPath("/html/body/form/center/table/tbody/tr/td/table/tbody/tr[3]/td[3]/input")).SendKeys(parcelNumber);
                        gc.CreatePdf_WOP(orderNumber, "Parcel search", driver, "CA", "Merced");
                        IWebElement ISpan12 = driver.FindElement(By.XPath("/html/body/form/center/table/tbody/tr/td/p/input[1]"));
                        IJavaScriptExecutor js12 = driver as IJavaScriptExecutor;
                        js12.ExecuteScript("arguments[0].click();", ISpan12);
                        Thread.Sleep(4000);
                        try
                        {
                            IWebElement Multyaddresstable = driver.FindElement(By.XPath("/html/body/form/center/table/tbody/tr/td/table/tbody"));
                            IList<IWebElement> multiaddressrow = Multyaddresstable.FindElements(By.TagName("tr"));
                            IList<IWebElement> multiaddressid;
                            foreach (IWebElement Multiaddress in multiaddressrow)
                            {
                                multiaddressid = Multiaddress.FindElements(By.TagName("td"));
                                if (multiaddressid.Count != 0 && Multiaddress.Text.Trim() != "" && !Multiaddress.Text.Contains("Fee Parcel"))
                                {
                                    multiparceladd = multiaddressid[0].FindElement(By.TagName("a"));
                                    string multiaddstring1 = multiparceladd.GetAttribute("href");
                                    driver.Navigate().GoToUrl(multiaddstring1);
                                    Thread.Sleep(2000);
                                    break;
                                }
                            }
                        }
                        catch { }
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.XPath("/html/body/form/center/table"));
                        if (INodata.Text.Contains("Assessor Inquiry: Please enter search criteria"))
                        {
                            HttpContext.Current.Session["Nodata_MercedCA"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    //property detail
                    IWebElement propertytable = driver.FindElement(By.XPath("/html/body/form/center/table/tbody/tr/td/table/tbody"));
                    Parcel_number = gc.Between(propertytable.Text, "Assessor ID Number", "Tax Rate Area (TRA)").Trim();
                    string TaxRateArea = gc.Between(propertytable.Text, "Tax Rate Area (TRA)", "Last Recording Date").Trim();
                    string Property_Type = gc.Between(propertytable.Text, "Property Type", "Acres").Trim();
                    string Acres = gc.Between(propertytable.Text, "Acres", "Asmt Description").Trim();
                    string Asmt_Description = gc.Between(propertytable.Text, "Asmt Description", "Asmt Status").Trim();
                    string AsmtStatus = gc.Between(propertytable.Text, "Asmt Status", "Roll Values").Trim();
                    string Ownership = gc.Between(propertytable.Text, "Ownership", "Building Description(s)").Trim();
                    try
                    {
                        YearBuilt = gc.Between(propertytable.Text, "Year Built", "Bedrooms");
                    }
                    catch { }
                    string propertyresult = TaxRateArea + "~" + Property_Type + "~" + Acres + "~" + Asmt_Description + "~" + AsmtStatus + "~" + Ownership + "~" + YearBuilt;
                    gc.insert_date(orderNumber, Parcel_number, 1004, propertyresult, 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.CreatePdf(orderNumber, Parcel_number, "Property detail", driver, "CA", "Merced");
                    string lane = gc.Between(propertytable.Text, "Land", "Structure");
                    string Structure = gc.Between(propertytable.Text, "Structure", "Fixtures");
                    string Fixtures = gc.Between(propertytable.Text, "Fixtures", "Growing");
                    string Growing = gc.Between(propertytable.Text, "Growing", "Total Land and Improvements");
                    string TotalLand = gc.Between(propertytable.Text, "Total Land and Improvements", "Manufactured Home");
                    string Manufactured_Home = gc.Between(propertytable.Text, "Manufactured Home", "Personal Property");
                    string PersonalProperty = gc.Between(propertytable.Text, "Personal Property", "Homeowners Exemption");
                    string HomeownersExemption = gc.Between(propertytable.Text, "Homeowners Exemption", "Other Exemption").Trim();
                    string OtherExemption = gc.Between(propertytable.Text, "Other Exemption", "Net Assessment").Trim();
                    string Netassessment = gc.Between(propertytable.Text, "Net Assessment", "Ownership");
                    string propertydetailresult = lane + "~" + Structure + "~" + Fixtures + "~" + Growing + "~" + TotalLand + "~" + Manufactured_Home + "~" + PersonalProperty + "~" + HomeownersExemption + "~" + OtherExemption + "~" + Netassessment;
                    gc.insert_date(orderNumber, Parcel_number, 1005, propertydetailresult, 1, DateTime.Now);
                    //tax Information
                    List<string> hreftax = new List<string>();
                    string addresscs = "", rollcast = "", rollcast1 = "";
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("https://common3.mptsweb.com/mbc/merced/tax/search#");
                    IWebElement Taxauthoritytable = driver.FindElement(By.XPath("//*[@id='footer']/div[1]/div/div[1]/div[2]/div/ul"));
                    Tax_Authority = GlobalClass.After(Taxauthoritytable.Text, "Tax Collector").Trim();
                    driver.FindElement(By.Id("SearchValue")).SendKeys(Parcel_number);
                    gc.CreatePdf(orderNumber, Parcel_number, "Tax Information Parcel", driver, "CA", "Merced");
                    for (int p = 0; p < 2; p++)
                    {
                        if (p == 1)
                        {
                            IWebElement PropertyHistry = driver.FindElement(By.Id("SelTaxYear"));
                            SelectElement PropertySelect = new SelectElement(PropertyHistry);
                            PropertySelect.SelectByIndex(p);
                            Thread.Sleep(1000);
                            gc.CreatePdf(orderNumber, Parcel_number, "Peior", driver, "CA", "Merced");
                        }
                        driver.FindElement(By.Id("SearchSubmit")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, Parcel_number, "Tax Information click" + p, driver, "CA", "Merced");
                        IWebElement taxviewclick = driver.FindElement(By.Id("ResultDiv"));
                        IList<IWebElement> Aherftax = taxviewclick.FindElements(By.TagName("a"));

                        foreach (IWebElement taxviewdetail in Aherftax)
                        {
                            string Taxid = GlobalClass.After(taxviewdetail.GetAttribute("href"), "tax/main/");
                            if (p == 0)
                            {
                                if (taxviewdetail.Text.Contains("View Details"))
                                {
                                    hreftax.Add(taxviewdetail.GetAttribute("href"));

                                }
                            }
                            if (p == 1)
                            {
                                if (taxviewdetail.Text.Contains("View Details") && !hreftax.Contains(Taxid))
                                {
                                    hreftax.Add(taxviewdetail.GetAttribute("href"));

                                }
                            }
                        }
                    }
                    int i = 1; string Balancetax = "";
                    foreach (string firstview in hreftax)
                    {
                        driver.Navigate().GoToUrl(firstview);
                        gc.CreatePdf(orderNumber, Parcel_number, "Tax Information Detail" + i, driver, "CA", "Merced");
                        IWebElement assmenttaxtable = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/ul/li[2]/a"));
                        string assmenttaxhrdf = assmenttaxtable.GetAttribute("href");
                        assmenttaxtable.Click();
                        // driver.Navigate().GoToUrl(assmenttaxhrdf);
                        Thread.Sleep(2000);
                        IWebElement rollcasttable = driver.FindElement(By.XPath("//*[@id='h2tab2']"));
                        rollcast = gc.Between(rollcasttable.Text, "Roll Category", "Doc Num").Trim();
                        addresscs = GlobalClass.After(rollcasttable.Text, "Address");
                        gc.CreatePdf(orderNumber, Parcel_number, "Assment Information Detail" + i, driver, "CA", "Merced");
                        driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/ul/li[1]/a")).Click();
                        Thread.Sleep(2000);
                        IWebElement taxinfotable = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/div/div[1]"));
                        string Assessment = gc.Between(taxinfotable.Text, "ASMT", "PARCEL");
                        string taxyear = gc.Between(taxinfotable.Text, "YEAR", "VIEW TAX BILL").Trim();
                        IList<IWebElement> viwetaxbill = taxinfotable.FindElements(By.TagName("a"));
                        foreach (IWebElement taxyearelement in viwetaxbill)
                        {
                            if (taxyearelement.Text.Contains("VIEW TAX BILL"))
                            {
                                string viewhref = taxyearelement.GetAttribute("href");
                                gc.downloadfile(viewhref, orderNumber, Parcel_number, "ViewTaxBill.pdf" + i, "CA", "Merced");
                                i++;
                            }
                        }
                        IWebElement compain = driver.FindElement(By.XPath("//*[@id='h2tab1']/div[2]"));
                        string compain1 = GlobalClass.Before(compain.Text, "Total Due");
                        string TotalDuecompain = gc.Between(compain.Text, "Total Due", "Total Paid").Trim();
                        string TotalPaidcompain = gc.Between(compain.Text, "Total Paid", "Total Balance").Trim();
                        string TotalBalance = GlobalClass.After(compain.Text, "Total Balance");
                        for (int j = 1; j < 3; j++)
                        {
                            string PaidStatus1 = "", Due_PaidDate = "";
                            IWebElement firstinstalment = driver.FindElement(By.XPath("//*[@id='h2tab1']/div[1]/div[" + j + "]"));
                            string instalfirst = GlobalClass.Before(firstinstalment.Text, "Paid Status");
                            if(instalfirst.Contains("Delinq. Date"))
                            {
                                PaidStatus1 = gc.Between(firstinstalment.Text, "Paid Status", "Delinq. Date").Trim();
                                Due_PaidDate = gc.Between(firstinstalment.Text, "Delinq. Date", "Total Due").Trim();
                            }
                            else
                            {
                                PaidStatus1 = gc.Between(firstinstalment.Text, "Paid Status", "Paid Date").Trim();
                                Due_PaidDate = gc.Between(firstinstalment.Text, "Paid Date", "Total Due").Trim();
                            }
                            string totaldue = gc.Between(firstinstalment.Text, "Total Due", "Total Paid").Trim();
                            string TotalPaid = gc.Between(firstinstalment.Text, "Total Paid", "Balance").Trim();
                            if (firstinstalment.Text.Contains("\r\nADD"))
                            {
                                Balancetax = gc.Between(firstinstalment.Text, "Balance", "\r\nADD").Trim();
                            }
                            if (!firstinstalment.Text.Contains("\r\nADD"))
                            {
                                Balancetax = GlobalClass.After(firstinstalment.Text, "Balance").Trim();
                            }
                            string taxresult = addresscs + "~" + rollcast + "~" + Assessment + "~" + taxyear + "~" + instalfirst + "~" + PaidStatus1 + "~" + Due_PaidDate + "~" + totaldue + "~" + TotalPaid + "~" + Balancetax + "~" + compain1 + "~" + TotalDuecompain + "~" + TotalPaidcompain + "~" + TotalBalance + "~" + Tax_Authority;
                            gc.insert_date(orderNumber, Parcel_number, 1013, taxresult, 1, DateTime.Now);
                        }
                        try
                        {
                            driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/ul/li[3]/a")).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, Parcel_number, "Tax Code Detail" + i, driver, "CA", "Merced");
                            for (int k = 1; k < 11; k++)
                            {
                                try
                                {
                                    IWebElement Taxcodeinfotable = driver.FindElement(By.XPath("//*[@id='h2tab3']/div[" + k + "]/div/div/dl"));
                                    string tax_code = gc.Between(Taxcodeinfotable.Text, "Tax Code", "Description").Trim();
                                    string Description = gc.Between(Taxcodeinfotable.Text, "Description", "Rate").Trim();
                                    string Rate = gc.Between(Taxcodeinfotable.Text, "Rate", "1st").Trim();
                                    string Firstinstalment = gc.Between(Taxcodeinfotable.Text, "1st Installment", "2nd ").Trim();
                                    string secondinstalment = gc.Between(Taxcodeinfotable.Text, "2nd Installment", "Total").Trim();
                                    string Total = gc.Between(Taxcodeinfotable.Text, "Total", "Phone").Trim();
                                    string Phone = GlobalClass.After(Taxcodeinfotable.Text, "Phone").Trim();
                                    string Taxcoderesult = taxyear + "~" + tax_code + "~" + Description + "~" + Rate + "~" + Firstinstalment + "~" + secondinstalment + "~" + Total + "~" + Phone;
                                    gc.insert_date(orderNumber, Parcel_number, 1016, Taxcoderesult, 1, DateTime.Now);
                                }
                                catch { }
                            }
                        }
                        catch { }
                        //Default tax
                        try
                        {
                            driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/ul/li[4]/a")).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, Parcel_number, "Default tax", driver, "CA", "Merced");
                            IWebElement defaulttaxtable = driver.FindElement(By.XPath("//*[@id='h2tab4']/div[2]"));
                            string DefaultNumber = gc.Between(defaulttaxtable.Text, "Default Number", "Pay Plan in Effect");
                            string PayEffect = gc.Between(defaulttaxtable.Text, "in Effect", "Annual Payment");
                            string AnnualPayment = gc.Between(defaulttaxtable.Text, "Annual Payment", "\r\nBalance");
                            string Balance = GlobalClass.After(defaulttaxtable.Text, "Balance\r\n");
                            string defaulttaxresult = DefaultNumber + "~" + PayEffect + "~" + AnnualPayment + "~" + Balance;
                            gc.insert_date(orderNumber, Parcel_number, 1019, defaulttaxresult, 1, DateTime.Now);
                        }
                        catch (Exception ex) { }

                    }
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "CA", "Merced", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    driver.Quit();
                    gc.mergpdf(orderNumber, "CA", "Merced");
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