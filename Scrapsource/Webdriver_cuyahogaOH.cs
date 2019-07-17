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
using System.Linq;

namespace ScrapMaricopa.Scrapsource
{
    public class Webdriver_cuyahogaOH
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        List<string> link = new List<string>();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_cuyahogaOH(string streetno, string direction, string streetname, string streettype, string unitnumber, string ownernm, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            List<string> taxinformation = new List<string>();
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string Parcel_number = "", Tax_Authority = "", address = "", Halfyeardue = "", Propertyresult = "", Valuvationresult = "", Multiaddressadd = "", MailingAddress = "";
            //var driverService = PhantomJSDriverService.CreateDefaultService();
            //driverService.HideCommandPromptWindow = true;
            //driver = new PhantomJSDriver();
            //driver = new ChromeDriver();
            StartTime = DateTime.Now.ToString("HH:mm:ss");
            var option = new ChromeOptions();
            option.AddArgument("No-Sandbox");
            using (driver = new ChromeDriver(option))
            {
                try
                {
                    if (streetname.Any(char.IsDigit))
                    {
                        streetname = Regex.Match(streetname, @"\d+").Value;
                    }
                    if (direction != "")
                    {
                        address = streetno + " " + direction + " " + streetname + " " + streettype;
                    }
                    else
                    {
                        address = streetno + " " + streetname + " " + streettype;
                    }
                    if (searchType == "titleflex")
                    {
                        if (direction != "")
                        {
                            address = streetno + " " + direction + " " + streetname + " " + streettype;
                        }
                        else
                        {
                            address = streetno + " " + streetname + " " + streettype;
                        }
                        //string titleaddress = address;
                        gc.TitleFlexSearch(orderNumber, "", "", address, "OH", "Cuyahoga");

                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Zero_cuyahoga"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    driver.Navigate().GoToUrl("http://myplace.cuyahogacounty.us/");
                    if (searchType == "address")
                    {
                        driver.FindElement(By.Id("Address")).Click();
                        Thread.Sleep(1000);
                        //if (address.Any(char.IsDigit))
                        //{
                        //    address = Regex.Match(address, @"\d+").Value;
                        //}
                        driver.FindElement(By.Id("txtData")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "OH", "Cuyahoga");
                        driver.FindElement(By.Id("btnSearch")).Click();
                        Thread.Sleep(6000);
                        try
                        {
                            IWebElement ul_element = driver.FindElement(By.Id("AddressInfo"));
                            IList<IWebElement> liall = ul_element.FindElements(By.TagName("li"));
                            int u = 0, y = 0, A = 0;
                            foreach (var item in liall)
                            {
                                if (item.Text != "" && !item.Text.Contains("No results found."))
                                {
                                    if (u == 0)
                                    {
                                        A++;
                                        u = 1;
                                    }
                                    Multiaddressadd += item.Text + "~";
                                    y = 0;

                                }
                                else
                                {
                                    if (y == 0 & Multiaddressadd.Trim() != "")
                                    {
                                        u = 0;
                                        y = 1;
                                        string[] split = Multiaddressadd.Split('~');
                                        //string res = "";
                                        string percalno = split[0];
                                        string Ownar = split[1];
                                        string Address = split[2];
                                        gc.insert_date(orderNumber, percalno, 1415, Address + "~" + Ownar, 1, DateTime.Now);
                                        gc.CreatePdf_WOP(orderNumber, "Address Before", driver, "OH", "Cuyahoga");
                                    }
                                    Multiaddressadd = "";
                                }
                                //if (item.Text.Contains("No results found."))
                                //{
                                //    HttpContext.Current.Session["Zero_cuyahoga"] = "Zero";
                                //    driver.Quit();
                                //    return "No Data Found";
                                //}
                            }
                            if (A < 25 && A > 1)
                            {
                                HttpContext.Current.Session["multiParcel_cuyahoga"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (A > 25)
                            {
                                HttpContext.Current.Session["multiParcel_cuyahoga_Multicount"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }
                        try
                        {
                            IWebElement INodata = driver.FindElement(By.Id("AddressInfo"));
                            if(INodata.Text.Contains("No results found"))
                            {
                                HttpContext.Current.Session["Zero_cuyahoga"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("txtData")).SendKeys(ownernm);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "OH", "Cuyahoga");
                        driver.FindElement(By.Id("btnSearch")).Click();
                        Thread.Sleep(2000);
                        try
                        {
                            string zerodata = driver.FindElement(By.XPath("//*[@id='AddressInfo']/li/p")).Text;
                            gc.CreatePdf_WOP(orderNumber, "Address Before", driver, "OH", "Cuyahoga");
                            if (zerodata != "")
                            {
                                HttpContext.Current.Session["Zero_cuyahoga"] = "Zero";
                                driver.Quit();
                                return "";
                            }
                        }
                        catch { }
                        try
                        {
                            IWebElement ul_element = driver.FindElement(By.Id("AddressInfo"));
                            IList<IWebElement> liall = ul_element.FindElements(By.TagName("li"));
                            gc.CreatePdf_WOP(orderNumber, "Address Before", driver, "OH", "Cuyahoga");
                            int u = 0, y = 0, A = 0;
                            foreach (var item in liall)
                            {
                                if (item.Text != "")
                                {
                                    if (u == 0)
                                    {
                                        A++;
                                        u = 1;
                                    }
                                    Multiaddressadd += item.Text + "~";
                                    y = 0;

                                }
                                else
                                {
                                    if (y == 0)
                                    {
                                        u = 0;
                                        y = 1;
                                        string[] split = Multiaddressadd.Split('~');
                                        //string res = "";
                                        string percalno = split[0];
                                        string Ownar = split[1];
                                        string Address = split[2];
                                        gc.insert_date(orderNumber, percalno, 1415, Address + "~" + Ownar, 1, DateTime.Now);
                                    }
                                    Multiaddressadd = "";
                                }
                            }
                            if (A > 26 && A > 1)
                            {
                                HttpContext.Current.Session["multiParcel_cuyahoga"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (A > 25)
                            {
                                HttpContext.Current.Session["multiParcel_cuyahoga_Multicount"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {
                        if (parcelNumber.Trim() != "")
                        {
                            driver.FindElement(By.Id("Parcel")).Click();
                            Thread.Sleep(1000);
                            driver.FindElement(By.Id("txtData")).SendKeys(parcelNumber.Replace("-", ""));
                            gc.CreatePdf_WOP(orderNumber, "Address search", driver, "OH", "Cuyahoga");
                            driver.FindElement(By.Id("btnSearch")).Click();
                            Thread.Sleep(2000);
                        }
                        if (parcelNumber.Trim() == "")
                        {
                            HttpContext.Current.Session["Zero_cuyahoga"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        try
                        {
                            IWebElement INodata = driver.FindElement(By.Id("AddressInfo"));
                            if (INodata.Text.Contains("No results found"))
                            {
                                HttpContext.Current.Session["Zero_cuyahoga"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    Thread.Sleep(6000);
                    driver.FindElement(By.Id("btnPropertyCardInfo")).SendKeys(Keys.Enter);
                    Thread.Sleep(5000);
                    gc.CreatePdf_WOP(orderNumber, "Property click", driver, "OH", "Cuyahoga");
                    string parceldiv = driver.FindElement(By.XPath("//*[@id='dataBody']/div[1]/div[3]")).Text;
                    Parcel_number = GlobalClass.After(parceldiv, "Parcel:");
                    string propertydetailtable = driver.FindElement(By.XPath("//*[@id='dataBody']/table[1]/tbody")).Text;
                    string Ownername = gc.Between(propertydetailtable, "Owner", "Address");
                    string Propertyaddress = gc.Between(propertydetailtable.Replace("\r\n", " "), "Address", "Land Use");
                    string Legal = gc.Between(propertydetailtable, "Legal Description", "Neighborhood");
                    string yearbuilt = driver.FindElement(By.XPath("//*[@id='dataBody']/table[2]/tbody/tr[3]/td[2]")).Text;
                    string Occupancy = driver.FindElement(By.XPath("//*[@id='dataBody']/table[2]/tbody/tr[1]/td[4]")).Text;
                    string Assessyeartable = driver.FindElement(By.XPath("//*[@id='dataBody']/div[4]/div[2]/table/tbody/tr[1]/td[1]")).Text;
                    string AssessmentYear = GlobalClass.Before(Assessyeartable, "Values");
                    string Propertydetail = Ownername + "~" + Propertyaddress + "~" + Legal + "~" + yearbuilt + "~" + Occupancy + "~" + AssessmentYear;
                    //gc.CreatePdf(orderNumber, Parcel_number, "Property Details", driver, "OH", "Cuyahoga");
                    gc.insert_date(orderNumber, Parcel_number, 1398, Propertydetail, 1, DateTime.Now);
                    IWebElement valuvationtable = driver.FindElement(By.XPath("//*[@id='dataBody']/div[4]/div[2]/table/tbody"));
                    IList<IWebElement> valuvationrow = valuvationtable.FindElements(By.TagName("tr"));
                    IList<IWebElement> valuvationid;
                    foreach (IWebElement valuvation in valuvationrow)
                    {
                        valuvationid = valuvation.FindElements(By.TagName("td"));
                        if (valuvationid.Count != 0 & !valuvation.Text.Contains("Taxable") & !valuvation.Text.Contains("Land Use"))
                        {
                            Valuvationresult += valuvationid[4].Text + "~";
                        }
                    }
                    //string Currenturl = driver.Url;
                    //var chromeOptions = new ChromeOptions();
                    //var driver1 = new ChromeDriver(chromeOptions);
                    //try
                    //{
                    //    var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];
                    //    chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                    //    chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                    //    chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                    //    chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                    //    // driver1.Navigate().GoToUrl(Currenturl);
                    //    string fileName = "";
                    //    driver1.Navigate().GoToUrl("http://myplace.cuyahogacounty.us/");
                    //    driver1.FindElement(By.Id("Parcel")).Click();
                    //    Thread.Sleep(1000);
                    //    driver1.FindElement(By.Id("txtData")).SendKeys(Parcel_number.Replace("-", "").Trim());
                    //    driver1.FindElement(By.Id("btnSearch")).Click();
                    //    Thread.Sleep(2000);
                    //    driver1.FindElement(By.Id("btnPropertyCardInfo")).SendKeys(Keys.Enter);
                    //    Thread.Sleep(5000);
                    //    driver1.FindElement(By.XPath("//*[@id='viewPropertyHeader']/div[2]/div/button")).Click();
                    //    Thread.Sleep(3000);
                    //    fileName = Parcel_number.Replace("-", "").Trim() + ".pdf";
                    //    gc.AutoDownloadFile(orderNumber, Parcel_number.Replace("-", ""), "Cuyahoga", "OH", fileName);
                    //    driver1.Quit();
                    //}
                    //catch (Exception ex) { driver1.Quit(); }
                    //Tax Information
                    driver.Navigate().GoToUrl("https://treasurer.cuyahogacounty.us/payments/real_prop/srch_criteria.htm");
                    driver.FindElement(By.Name("parcelNum")).SendKeys(Parcel_number.Trim().Replace("-", ""));
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number.Replace("-", ""), "Tax click", driver, "OH", "Cuyahoga");
                    IWebElement parcelClick = driver.FindElement(By.XPath("//*[@id='byParcelNum']/form/table[2]/tbody/tr[1]/td[2]/a/img"));
                    parcelClick.Click();
                    Thread.Sleep(2000);
                    driver.SwitchTo().Window(driver.WindowHandles.Last());
                    gc.CreatePdf(orderNumber, Parcel_number.Replace("-", ""), "Tax Parcel click window", driver, "OH", "Cuyahoga");
                    IWebElement Atag = driver.FindElement(By.XPath("/html/body/div/table/tbody/tr/td[1]/table/tbody/tr[2]/td/form/table/tbody/tr[2]/td"));
                    IWebElement Href = Atag.FindElement(By.TagName("a"));
                    Href.Click();
                    Thread.Sleep(2000);
                    driver.SwitchTo().Window(driver.WindowHandles.Last());
                    gc.CreatePdf(orderNumber, Parcel_number.Replace("-", ""), "Homestage window Last", driver, "OH", "Cuyahoga");
                    string Homestage = driver.FindElement(By.XPath("//*[@id='PropTax']/table[3]/tbody/tr[2]/td[3]")).Text;
                    string homstageresult = Valuvationresult + Homestage;
                    gc.insert_date(orderNumber, Parcel_number, 1404, homstageresult, 1, DateTime.Now);
                    Tax_Authority = driver.FindElement(By.XPath("//*[@id='PerfBill']/div/strong")).Text;
                    try
                    {

                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='PropTax']/table[5]/tbody/tr[2]/td[2]/table[2]/tbody")));
                        gc.CreatePdf(orderNumber, Parcel_number.Replace("-", ""), "Homestage window Last 2", driver, "OH", "Cuyahoga");
                    }
                    catch { }
                    driver.FindElement(By.LinkText("View full year summary")).Click();
                    Thread.Sleep(2000);
                    //gc.CreatePdf(orderNumber, Parcel_number, "Tax Summar Window Full Year", driver, "OH", "Cuyahoga");
                    int Currentyear = DateTime.Now.Year;
                    int curr = 0;
                    //Tax_Authority = GlobalClass.Before(taxautable, "DISCLAIMER");
                    for (int i = 0; i < 3; i++)
                    {

                        try
                        {
                            IWebElement PropertyInformation = driver.FindElement(By.Id("year"));
                            SelectElement PropertyInformationSelect = new SelectElement(PropertyInformation);
                            PropertyInformationSelect.SelectByValue(Currentyear.ToString());
                            Thread.Sleep(3000);
                        }
                        catch
                        {
                            Currentyear--;
                            curr++;
                        }
                        try
                        {
                            if (curr == 1)
                            {

                                IWebElement PropertyInformation = driver.FindElement(By.Id("year"));
                                SelectElement PropertyInformationSelect = new SelectElement(PropertyInformation);
                                PropertyInformationSelect.SelectByValue(Currentyear.ToString());
                                Thread.Sleep(3000);
                                curr = 0;
                            }
                        }
                        catch { }
                        gc.CreatePdf(orderNumber, Parcel_number.Replace("-", ""), "Tax Summar Window Full Year" + Currentyear, driver, "OH", "Cuyahoga");
                        IWebElement PropertyInformation1 = driver.FindElement(By.Id("year"));
                        SelectElement PropertyInformationSelect1 = new SelectElement(PropertyInformation1);
                        //PropertyInformationSelect1.SelectByValue(Currentyear.ToString());
                        string taxyear = PropertyInformationSelect1.SelectedOption.Text;
                        string taxtable = driver.FindElement(By.XPath("//*[@id='form1']/table[1]/tbody/tr[2]/td/table[1]/tbody")).Text;
                        string taxbill = gc.Between(taxtable, "PRIMARY OWNERS", "SECONDARY OWNERS");
                        Halfyeardue = "";
                        try
                        {
                            string Halfyeardue1 = driver.FindElement(By.XPath("//*[@id='form1']/table[1]/tbody/tr[2]/td/table[2]/tbody/tr[23]/td")).Text;
                            Halfyeardue = GlobalClass.After(Halfyeardue1, "HALF YEAR DUE");
                        }
                        catch { }
                        string Total_Charges = driver.FindElement(By.XPath("//*[@id='form1']/table[1]/tbody/tr[2]/td/table[2]/tbody/tr[21]/td[2]")).Text;
                        string TatalPayment = driver.FindElement(By.XPath("//*[@id='form1']/table[1]/tbody/tr[2]/td/table[2]/tbody/tr[21]/td[3]")).Text;
                        string Full_yearbal = driver.FindElement(By.XPath("//*[@id='form1']/table[1]/tbody/tr[2]/td/table[2]/tbody/tr[21]/td[4]")).Text;
                        string taxinforesult = taxyear + "~" + taxbill + "~" + Halfyeardue + "~" + Total_Charges + "~" + TatalPayment + "~" + Full_yearbal + "~" + Tax_Authority;
                        gc.insert_date(orderNumber, Parcel_number, 1403, taxinforesult, 1, DateTime.Now);
                        //1403
                        //full Year Charge
                        try
                        {
                            string Fullyear = driver.FindElement(By.XPath("//*[@id='form1']/table[1]/tbody/tr[2]/td/table[2]/tbody")).Text;
                            string GrossTax = gc.Between(Fullyear, "GROSS TAX", "FULL RATE");
                            string Less = gc.Between(Fullyear, "LESS 920 RED", "920 RED RATE");
                            string subtotal = gc.Between(Fullyear, "SUB TOTAL", "EFFECTIVE RATE");

                        }
                        catch { }
                        try
                        {

                            ByVisibleElement(driver.FindElement(By.XPath("//*[@id='form1']/table[2]/tbody")));
                            gc.CreatePdf(orderNumber, Parcel_number.Replace("-", ""), "Tax Summar Window Full Year 2" + Currentyear, driver, "OH", "Cuyahoga");
                        }
                        catch { }
                        IWebElement paymenttable = driver.FindElement(By.XPath("//*[@id='form1']/table[2]/tbody"));
                        IList<IWebElement> Paymentrow = paymenttable.FindElements(By.TagName("tr"));
                        IList<IWebElement> Paymentid;
                        foreach (IWebElement Payment in Paymentrow)
                        {
                            Paymentid = Payment.FindElements(By.TagName("td"));
                            if (Paymentid.Count == 5 && !Payment.Text.Contains("TAXSET"))
                            {
                                string PaymentResult = taxyear + "~" + Paymentid[0].Text + "~" + Paymentid[1].Text + "~" + Paymentid[2].Text + "~" + Paymentid[3].Text + "~" + Paymentid[4].Text;
                                gc.insert_date(orderNumber, Parcel_number.Replace("-", ""), 1405, PaymentResult, 1, DateTime.Now);
                            }
                        }
                        try
                        {
                            IWebElement flagestable = driver.FindElement(By.XPath("//*[@id='form1']/table[1]/tbody/tr[2]/td/table[2]/tbody/tr[4]/td[1]/table/tbody"));
                            IList<IWebElement> flagesrow = flagestable.FindElements(By.TagName("tr"));
                            IList<IWebElement> flageid;
                            foreach (IWebElement flag in flagesrow)
                            {
                                flageid = flag.FindElements(By.TagName("td"));
                                if (flageid.Count == 2)
                                {
                                    string Flageresult = taxyear + "~" + flageid[0].Text + "~" + flageid[1].Text;
                                    gc.insert_date(orderNumber, Parcel_number, 1406, Flageresult, 1, DateTime.Now);
                                }
                            }
                        }
                        catch { }
                        try
                        {
                            IWebElement flagestable = driver.FindElement(By.XPath("//*[@id='form1']/table[1]/tbody/tr[2]/td/table[2]/tbody"));
                            IList<IWebElement> flagesrow = flagestable.FindElements(By.TagName("tr"));
                            IList<IWebElement> flageid;
                            foreach (IWebElement flag in flagesrow)
                            {
                                flageid = flag.FindElements(By.TagName("td"));
                                if (flag.Text.Contains("OWN OCCUPANCY CRD") || flag.Text.Contains("HOMESTEAD") || flag.Text.Contains("FORECLOSURE") || flag.Text.Contains("CERT. PEND.") || flag.Text.Contains("CERT. SOLD"))
                                {
                                    string Flageresult = taxyear + "~" + flageid[4].Text + "~" + flageid[5].Text;
                                    gc.insert_date(orderNumber, Parcel_number, 1406, Flageresult, 1, DateTime.Now);
                                }
                                if (flag.Text.Contains("PAYMENT PLAN"))
                                {
                                    string Flageresult = taxyear + "~" + flageid[3].Text + "~" + flageid[4].Text;
                                    gc.insert_date(orderNumber, Parcel_number, 1406, Flageresult, 1, DateTime.Now);
                                    break;
                                }
                            }
                        }
                        catch { }


                        Currentyear--;

                    }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "OH", "Cuyahoga", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "OH", "Cuyahoga");
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
        public void ByVisibleElement(IWebElement Element)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].scrollIntoView();", Element);
        }
    }
}