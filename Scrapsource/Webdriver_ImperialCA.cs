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
    public class WebDriver_ImperialCA
    {

        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();
        public string FTP_ImperialCA(string Address, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string Tax_Authority = "", Year = "", href = "", Addresshrf = "", Propertyresult = "", PaidDate = "", parcelhref = "", MailingAddress = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                // driver = new ChromeDriver();
                List<string> hreftax = new List<string>();
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("http://www.co.imperial.ca.us/Assessor/PublicWebValueNotices.html");

                    if (searchType == "titleflex")
                    {
                        //string address = houseno + " " + direction + " " + streetname + " " + streettype;
                        gc.TitleFlexSearch(orderNumber, "", ownername, "", "CA", "Imperial");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Zero_Imperial"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        IWebElement Multyaddresstable1 = driver.FindElement(By.XPath("//*[@id='ctl00_MasterContentPlaceHolder_ContentiFrame']"));
                        driver.SwitchTo().Frame(Multyaddresstable1);
                        driver.FindElement(By.XPath("/html/body/form/center/table/tbody/tr[5]/td[3]/input")).SendKeys(Address);
                        IWebElement searchclick = driver.FindElement(By.XPath("/html/body/form/center/p/input[1]"));
                        IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                        js1.ExecuteScript("arguments[0].click();", searchclick);
                        Thread.Sleep(2000);
                        try
                        {
                            int Max = 0;
                            gc.CreatePdf_WOP(orderNumber, "SearchAfter", driver, "CA", "Imperial");
                            IWebElement Multiparceladdress = driver.FindElement(By.XPath("/html/body/form/center/table/tbody"));
                            IList<IWebElement> Multiparcelrow = Multiparceladdress.FindElements(By.TagName("tr"));
                            IList<IWebElement> Multiparcelid;
                            foreach (IWebElement multiparcel in Multiparcelrow)
                            {
                                Multiparcelid = multiparcel.FindElements(By.TagName("td"));
                                if (Multiparcelid.Count != 0 && !multiparcel.Text.Contains("Asmt"))
                                {
                                    IWebElement Address1 = Multiparcelid[0].FindElement(By.TagName("a"));
                                    Addresshrf = Address1.GetAttribute("href");
                                    string Assment = Address1.Text;
                                    string Freeparcel = Multiparcelid[1].Text;
                                    string Tra = Multiparcelid[2].Text;
                                    string Multiparcel = Freeparcel + "~" + Tra;
                                    gc.insert_date(orderNumber, Assment, 1275, Multiparcel, 1, DateTime.Now);
                                    Max++;
                                }
                                if (Multiparcelrow.Count == 5 && !multiparcel.Text.Contains("Asmt") && Max < 2)
                                {
                                    driver.Navigate().GoToUrl(Addresshrf);
                                    Thread.Sleep(2000);
                                }
                            }

                            if (Max > 1 && Max < 26)
                            {
                                HttpContext.Current.Session["multiParcel_Imperial"] = "Maximum";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Max > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Imperial_Multicount"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                            if (Max == 0)
                            {
                                HttpContext.Current.Session["Zero_Imperial"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }

                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {
                        IWebElement Multyaddresstable1 = driver.FindElement(By.XPath("//*[@id='ctl00_MasterContentPlaceHolder_ContentiFrame']"));
                        driver.SwitchTo().Frame(Multyaddresstable1);
                        driver.FindElement(By.XPath("/html/body/form/center/table/tbody/tr[4]/td[3]/input")).SendKeys(parcelNumber);
                        IWebElement searchclick = driver.FindElement(By.XPath("/html/body/form/center/p/input[1]"));
                        IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                        js1.ExecuteScript("arguments[0].click();", searchclick);
                        Thread.Sleep(2000);
                        try
                        {
                            driver.FindElement(By.XPath("/html/body/form/center/table/tbody/tr[2]/td[1]/a")).Click();
                        }
                        catch { }
                    }

                    try
                    {
                        //No Data Found
                        string nodata = driver.FindElement(By.XPath("/html/body/form/center/table/tbody")).Text;
                        if (nodata.Contains("Assessor Inquiry: Please enter"))
                        {
                            HttpContext.Current.Session["Zero_Imperial"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    //Property Details
                    string Parcel_number = "", TaxRateArea = "", Propertytype = "", Acres = "", Lotsize = "", AsmtDescription = "", AsmtStatus = "", Land = "", Structure = "", Fixtures = "", Growing = "", TotalLandandImprovements = "", ManufacturedHome = "", PersonalProperty = "", HomeownersExemption = "", OtherExemption = "", NetAssessment = "", NetAssessment1 = "";
                    string Propertydetail = driver.FindElement(By.XPath("/html/body/form/center/table/tbody")).Text;
                    Parcel_number = gc.Between(Propertydetail, "Assessor ID Number", "Tax Rate Area (TRA)");
                    TaxRateArea = gc.Between(Propertydetail, "Tax Rate Area (TRA)", "Last Recording Date");
                    Propertytype = gc.Between(Propertydetail, "Property Type", "Acres");
                    Acres = gc.Between(Propertydetail, "Acres", "Lot Size(SqFt)");
                    Lotsize = gc.Between(Propertydetail, "Lot Size(SqFt)", "Asmt Description");
                    AsmtDescription = gc.Between(Propertydetail, "Asmt Description", "Asmt Status");
                    AsmtStatus = gc.Between(Propertydetail, "Asmt Status", "Roll Values");
                    Land = gc.Between(Propertydetail, "Land", "Structure");
                    Structure = gc.Between(Propertydetail, "Structure", "Fixtures");
                    Fixtures = gc.Between(Propertydetail, "Fixtures", "Growing");
                    Growing = gc.Between(Propertydetail, "Growing", "Total Land and Improvements");
                    TotalLandandImprovements = gc.Between(Propertydetail, "Total Land and Improvements", "Manufactured Home");
                    ManufacturedHome = gc.Between(Propertydetail, "Manufactured Home", "Personal Property");
                    PersonalProperty = gc.Between(Propertydetail, "Personal Property", "Homeowners Exemption");
                    HomeownersExemption = gc.Between(Propertydetail, "Homeowners Exemption", "Other Exemption");
                    OtherExemption = gc.Between(Propertydetail, "Other Exemption", "Net Assessment");
                    NetAssessment1 = GlobalClass.After(Propertydetail, "Net Assessment").Trim();
                    NetAssessment = GlobalClass.Before(NetAssessment1, "Navigation").Trim();
                    gc.CreatePdf(orderNumber, Parcel_number, "Assessment Page", driver, "CA", "Imperial");

                    string Assessesult = Parcel_number.Trim() + "~" + TaxRateArea.Trim() + "~" + Propertytype.Trim() + "~" + Acres.Trim() + "~" + Lotsize.Trim() + "~" + AsmtDescription.Trim() + "~" + AsmtStatus.Trim() + "~" + Land.Trim() + "~" + Structure.Trim() + "~" + Fixtures.Trim() + "~" + Growing.Trim() + "~" + TotalLandandImprovements.Trim() + "~" + ManufacturedHome.Trim() + "~" + PersonalProperty.Trim() + "~" + HomeownersExemption.Trim() + "~" + OtherExemption.Trim() + "~" + NetAssessment.Trim();
                    gc.insert_date(orderNumber, Parcel_number, 1276, Assessesult, 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    //Tax Information Details

                    driver.Navigate().GoToUrl("https://common2.mptsweb.com/mbc/imperial/tax/search");
                    IWebElement Taxauthoritytable = driver.FindElement(By.XPath("//*[@id='footer']/div[1]/div/div[1]/div[2]/div/ul"));
                    Tax_Authority = GlobalClass.After(Taxauthoritytable.Text, "Tax Collector").Trim();

                    driver.FindElement(By.Id("SearchValue")).Clear();
                    IWebElement text = driver.FindElement(By.Id("SearchValue"));
                    IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                    js.ExecuteScript("document.getElementById('SearchValue').value='" + Parcel_number + "'");
                    //driver.FindElement(By.XPath("//*[@id='compwa']/form/div/input[2]")).SendKeys(Keys.Enter);

                    //driver.FindElement(By.Id("SearchValue")).SendKeys(parcelNumber);

                    gc.CreatePdf(orderNumber, Parcel_number, "Tax Information Parcel", driver, "CA", "Imperial");
                    driver.FindElement(By.Id("SearchSubmit")).Click();
                    Thread.Sleep(9000);

                    for (int p = 0; p < 3; p++)
                    {
                        if (p > 0)
                        {
                            IWebElement PropertyHistry = driver.FindElement(By.Id("SelTaxYear"));
                            SelectElement PropertySelect = new SelectElement(PropertyHistry);
                            PropertySelect.SelectByIndex(p);
                            Thread.Sleep(3000);
                            //gc.CreatePdf(orderNumber, Parcel_number, "Peior", driver, "CA", "Imperial");
                        }
                        driver.FindElement(By.Id("SearchSubmit")).Click();
                        Thread.Sleep(9000);
                        gc.CreatePdf(orderNumber, Parcel_number, "Tax Information click" + p, driver, "CA", "Imperial");
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
                            if (p > 0)
                            {
                                if (taxviewdetail.Text.Contains("View Details") && !hreftax.Contains(Taxid))
                                {
                                    hreftax.Add(taxviewdetail.GetAttribute("href"));

                                }
                            }
                        }
                    }
                    int i = 1;
                    int q = 0;
                    foreach (string firstview in hreftax)
                    {
                        driver.Navigate().GoToUrl(firstview);
                        Thread.Sleep(9000);
                        gc.CreatePdf(orderNumber, Parcel_number, "Tax Information Detail" + q, driver, "CA", "Imperial");
                        IWebElement assmenttaxtable = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/ul/li[2]/a"));
                        string assmenttaxhrdf = assmenttaxtable.GetAttribute("href");
                        assmenttaxtable.Click();
                        Thread.Sleep(9000);

                        gc.CreatePdf(orderNumber, Parcel_number, "Assment Information Detail" + q, driver, "CA", "Imperial");

                        IWebElement rollcasttable = driver.FindElement(By.XPath("//*[@id='h2tab2']"));
                        string rollcast = gc.Between(rollcasttable.Text, "Roll Category", "Doc Num").Trim();
                        string addresscs = GlobalClass.After(rollcasttable.Text, "Address");

                        driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/ul/li[1]/a")).Click();
                        //Thread.Sleep(9000);
                        //gc.CreatePdf(orderNumber, Parcel_number, "Tax Info Detail Pdf" + q, driver, "CA", "Imperial");

                        IWebElement taxinfotable = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/div/div[1]"));
                        string Assessment1 = gc.Between(taxinfotable.Text, "ASMT", "PARCEL");
                        string taxyear = gc.Between(taxinfotable.Text, "YEAR", "VIEW TAX BILL").Trim();
                        IList<IWebElement> viwetaxbill = taxinfotable.FindElements(By.TagName("a"));
                        foreach (IWebElement taxyearelement in viwetaxbill)
                        {
                            if (taxyearelement.Text.Contains("VIEW TAX BILL"))
                            {
                                string viewhref = taxyearelement.GetAttribute("href");
                                gc.downloadfile(viewhref, orderNumber, Parcel_number, "ViewTaxBill.pdf" + i, "CA", "Imperial");
                                i++;
                            }
                        }
                        string Balance = "";
                        IWebElement compain = driver.FindElement(By.XPath("//*[@id='h2tab1']/div[2]"));
                        string compain1 = GlobalClass.Before(compain.Text, "Total Due");
                        string TotalDuecompain = gc.Between(compain.Text, "Total Due", "Total Paid").Trim();
                        string TotalPaidcompain = gc.Between(compain.Text, "Total Paid", "Total Balance").Trim();
                        string TotalBalance = GlobalClass.After(compain.Text, "Total Balance");
                        for (int j = 1; j < 3; j++)
                        {
                            IWebElement firstinstalment = driver.FindElement(By.XPath("//*[@id='h2tab1']/div[1]/div[" + j + "]"));
                            string instalfirst = GlobalClass.Before(firstinstalment.Text, "Paid Status");
                            string PaidStatus1 = "", Due_PaidDate = "";
                            if (firstinstalment.Text.Contains("Due/Paid Date"))
                            {
                                PaidStatus1 = gc.Between(firstinstalment.Text, "Paid Status", "Due/Paid Date").Trim();
                                Due_PaidDate = gc.Between(firstinstalment.Text, "Due/Paid Date", "Total Due").Trim();
                            }
                            else
                            {
                                PaidStatus1 = gc.Between(firstinstalment.Text, "Paid Status", "Paid Date").Trim();
                                Due_PaidDate = gc.Between(firstinstalment.Text, "Paid Date", "Total Due").Trim();
                            }
                            string totaldue = gc.Between(firstinstalment.Text, "Total Due", "Total Paid").Trim();
                            string TotalPaid = gc.Between(firstinstalment.Text, "Total Paid", "Balance").Trim();
                            if (firstinstalment.Text.Contains("ADD"))
                            {
                                Balance = gc.Between(firstinstalment.Text, "Balance", "\r\nADD").Trim();
                            }
                            if (!firstinstalment.Text.Contains("ADD"))
                            {
                                Balance = GlobalClass.After(firstinstalment.Text, "Balance").Trim();
                            }
                            string taxresult = addresscs + "~" + rollcast + "~" + Assessment1 + "~" + taxyear + "~" + instalfirst + "~" + PaidStatus1 + "~" + Due_PaidDate + "~" + totaldue + "~" + TotalPaid + "~" + Balance + "~" + compain1 + "~" + TotalDuecompain + "~" + TotalPaidcompain + "~" + TotalBalance + "~" + Tax_Authority;
                            gc.insert_date(orderNumber, Parcel_number, 1277, taxresult, 1, DateTime.Now);
                        }

                        //TaxCode Details
                        try
                        {
                            driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/ul/li[3]/a")).Click();
                            Thread.Sleep(9000);
                            gc.CreatePdf(orderNumber, Parcel_number, "Tax Code Detail" + q, driver, "CA", "Imperial");
                            for (int k = 1; k < 15; k++)
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
                                    gc.insert_date(orderNumber, Parcel_number, 1278, Taxcoderesult, 1, DateTime.Now);
                                }
                                catch { }
                            }
                        }
                        catch { }
                        //Default tax
                        string defaulltyear = "", assessnumber = "";
                        try
                        {
                            IWebElement taxinfotablede = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/div/div[1]"));
                            defaulltyear = gc.Between(taxinfotable.Text, "YEAR", "VIEW TAX BILL");
                            assessnumber = gc.Between(taxinfotable.Text, "ASMT", "PARCEL");

                            driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/ul/li[4]/a")).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, Parcel_number, "Default Tax Detail" + q, driver, "CA", "Imperial");
                            IWebElement defaulttaxtable = driver.FindElement(By.XPath("//*[@id='h2tab4']/div[2]"));
                            string DefaultNumber = gc.Between(defaulttaxtable.Text, "Default Number", "Pay Plan in Effect");
                            string PayEffect = gc.Between(defaulttaxtable.Text, "in Effect", "Annual Payment");
                            string AnnualPayment = gc.Between(defaulttaxtable.Text, "Annual Payment", "\r\nBalance");
                            string Balance1 = GlobalClass.After(defaulttaxtable.Text, "Balance\r\n");
                            string defaulttaxresult = defaulltyear.Trim() + "~" + assessnumber.Trim() + "~" + DefaultNumber + "~" + PayEffect + "~" + AnnualPayment + "~" + Balance1;
                            gc.insert_date(orderNumber, Parcel_number, 1279, defaulttaxresult, 1, DateTime.Now);
                        }
                        catch { }
                        q++;
                    }


                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "CA", "Imperial", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    driver.Quit();
                    gc.mergpdf(orderNumber, "CA", "Imperial");
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
