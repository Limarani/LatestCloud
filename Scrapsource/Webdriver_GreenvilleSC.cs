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

    public class Webdriver_GreenvilleSC
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();
        IWebElement IAmg;
        public string FTP_GreenvilleSC(string streetno, string direction, string streettype, string streetname, string unitnumber, string ownernm, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            List<string> Taxyearlink = new List<string>();
            string Parcel_number = "", Tax_Authority = "", Year = "", Addresshrf = "", Propertyresult = "", PaidDate = "", parcelhref = "", MailingAddress = "", Yearbuilt = "", commends = "", Address = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new PhantomJSDriver();
            //driver = new ChromeDriver()
            //RDP
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    if (searchType == "titleflex")
                    {
                        if (direction != "")
                        {
                            Address = streetno + "~" + direction + "~" + streetname + "~" + streettype;
                        }
                        else
                        {
                            Address = streetno + "~" + streetname + "~" + streettype;
                        }
                        gc.TitleFlexSearch(orderNumber, "", ownernm, Address, "SC", "Greenville");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null))
                        {
                            return "MultiParcel";
                        }
                        searchType = "parcel";
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString().Replace(".", "");
                    }
                    try
                    {
                        StartTime = DateTime.Now.ToString("HH:mm:ss");
                        driver.Navigate().GoToUrl("https://www.greenvillecounty.org/");
                        //string Taxauthority =
                        gc.CreatePdf_WOP(orderNumber, "Tax Authority", driver, "SC", "Greenville");
                        Tax_Authority = driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[4]/div[3]/div/div[2]/div[2]/ul/li[11]/a")).Text.Replace("\r\n", " ");
                    }
                    catch { }
                    driver.Navigate().GoToUrl("https://www.greenvillecounty.org/appsAS400/RealProperty/");
                    if (searchType == "address")
                    {
                        if (direction != "")
                        {
                            Address = streetno.Trim() + " " + direction.Trim() + " " + streetname.Trim();
                        }
                        else
                        {
                            Address = streetno.Trim() + " " + streetname.Trim();
                        }
                        driver.FindElement(By.Id("ctl00_body_txt_Street")).SendKeys(Address);
                        gc.CreatePdf_WOP(orderNumber, "Searchbefore", driver, "SC", "Greenville");
                        driver.FindElement(By.Id("ctl00_body_btn_Search_ByStreet")).Click();
                        Thread.Sleep(2000);
                        int Max = 0;
                        IWebElement Multiparceladdress = driver.FindElement(By.XPath("//*[@id='tbl_Data']/tbody"));
                        IList<IWebElement> Multiparcelrow = Multiparceladdress.FindElements(By.TagName("tr"));
                        IList<IWebElement> Multiparcelid;
                        gc.CreatePdf_WOP(orderNumber, "Multiparcel", driver, "SC", "Greenville");
                        if (Multiparcelrow.Count < 26)
                        {
                            foreach (IWebElement multiparcel in Multiparcelrow)
                            {
                                Multiparcelid = multiparcel.FindElements(By.TagName("td"));
                                if (Multiparcelid.Count != 0)
                                {
                                    IWebElement Address1 = Multiparcelid[1].FindElement(By.TagName("a"));
                                    Addresshrf = Address1.GetAttribute("href");
                                    string Addressst = Multiparcelid[3].Text;
                                    string Owner1 = Multiparcelid[2].Text;
                                    string Pin = Multiparcelid[1].Text;
                                    string Desc = Multiparcelid[4].Text;
                                    string District1 = Multiparcelid[5].Text;
                                    string Multiparcel = Addressst + "~" + Owner1 + "~" + Desc + "~" + District1;
                                    gc.insert_date(orderNumber, Pin, 2092, Multiparcel, 1, DateTime.Now);
                                    Max++;
                                }
                            }
                            if (Max == 1)
                            {
                                driver.Navigate().GoToUrl(Addresshrf);
                                Thread.Sleep(2000);
                            }
                            if (Max > 1 && Max < 26)
                            {
                                HttpContext.Current.Session["multiParcel_Greenville"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Max == 0)
                            {
                                HttpContext.Current.Session["Zero_Greenville"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        else
                        {
                            HttpContext.Current.Session["multiParcel_Greenville_Multicount"] = "Maximum";
                            driver.Quit();
                            return "Maximum";
                        }

                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.Id("ctl00_body_txt_MapNumber")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber.Trim(), "Parcel Search", driver, "SC", "Greenville");
                        driver.FindElement(By.Id("ctl00_body_btn_Search_ByStreet")).Click();
                        Thread.Sleep(2000);

                    }
                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("ctl00_body_txt_Name")).SendKeys(ownernm);
                        driver.FindElement(By.Id("ctl00_body_btn_Search_ByStreet")).Click();
                        Thread.Sleep(5000);
                        int Max = 0;
                        IWebElement Multiparceladdress = driver.FindElement(By.XPath("//*[@id='tbl_Data']/tbody"));
                        IList<IWebElement> Multiparcelrow = Multiparceladdress.FindElements(By.TagName("tr"));
                        IList<IWebElement> Multiparcelid;
                        gc.CreatePdf_WOP(orderNumber, "Multiparcel", driver, "SC", "Greenville");
                        if (Multiparcelrow.Count < 26)
                        {
                            foreach (IWebElement multiparcel in Multiparcelrow)
                            {
                                Multiparcelid = multiparcel.FindElements(By.TagName("td"));
                                if (Multiparcelid.Count != 0)
                                {
                                    IWebElement Address1 = Multiparcelid[1].FindElement(By.TagName("a"));
                                    Addresshrf = Address1.GetAttribute("href");
                                    string Addressst = Multiparcelid[3].Text;
                                    string Owner1 = Multiparcelid[2].Text;
                                    string Pin = Multiparcelid[1].Text;
                                    string Desc = Multiparcelid[4].Text;
                                    string District1 = Multiparcelid[5].Text;
                                    string Multiparcel = Addressst + "~" + Owner1 + "~" + Desc + "~" + District1;
                                    gc.insert_date(orderNumber, Pin, 2092, Multiparcel, 1, DateTime.Now);
                                    Max++;
                                }
                            }
                            if (Max == 1)
                            {
                                driver.Navigate().GoToUrl(Addresshrf);
                                Thread.Sleep(2000);
                            }
                            if (Max > 1 && Max < 26)
                            {
                                HttpContext.Current.Session["multiParcel_Greenville"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Max == 0)
                            {
                                HttpContext.Current.Session["Zero_Greenville"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        else
                        {
                            HttpContext.Current.Session["multiParcel_Greenville_Multicount"] = "Maximum";
                            driver.Quit();
                            return "Maximum";
                        }

                    }
                    string generaltable = driver.FindElement(By.Id("MyData")).Text;
                    Parcel_number = gc.Between(generaltable, "Map #:", "Tax Year:").Trim();
                    string Taxyear = gc.Between(generaltable, "Tax Year:", "District:");
                    string District = gc.Between(generaltable, "District:", "Owner(s):");
                    string Owner = gc.Between(generaltable, "Owner(s):", "Previous Owner:");
                    string PreviousOwner = gc.Between(generaltable, "Previous Owner:", "Care Of:");
                    string CareOf = gc.Between(generaltable, "Care Of:", "Mailing Address:");
                    MailingAddress = gc.Between(generaltable, "Mailing Address:", "DESCRIPTION");
                    string Acreage = gc.Between(generaltable, "Acreage:", "Description:");
                    string Description = gc.Between(generaltable, "Description:", "Location:");
                    string Location = gc.Between(generaltable, "Location:", "Subdivision:");
                    string Subdivision = gc.Between(generaltable, "Subdivision:", "Deed Book-Page:");
                    string DeedBookPage = gc.Between(generaltable, "Deed Book-Page:", "Deed Date:");
                    string DeedDate = gc.Between(generaltable, "Deed Date:", "Will:");
                    string Will = gc.Between(generaltable, "Will:", "Sale Price:");
                    string SalePrice = gc.Between(generaltable, "Sale Price:", "Plat Book-Page:");
                    string PlatBookPage = gc.Between(generaltable, "Plat Book-Page:", "CLASSIFICATION");
                    string Jurisdiction = gc.Between(generaltable, "Jurisdiction:", "Homestead Code:");
                    string HomesteadCode = gc.Between(generaltable, "Homestead Code:", "Assessment Class:");
                    string AssessmentClass = gc.Between(generaltable, "Assessment Class:", "PROPERTY INFORMATION");
                    string landuse = gc.Between(generaltable, "Land Use:", "VALUE");
                    string FairMarketValue = gc.Between(generaltable, "Fair Market Value:", "Taxable Market Value:");
                    string TaxableMarketValue = gc.Between(generaltable, "Taxable Market Value:", "Total Rollback:");
                    string TotalRollback = gc.Between(generaltable, "Total Rollback:", "Taxes:");
                    string Taxes = GlobalClass.After(generaltable, "Taxes:");
                    gc.CreatePdf(orderNumber, Parcel_number, "Property detail", driver, "SC", "Greenville");
                    string Propertydetail = Taxyear + "~" + District + "~" + Owner + "~" + PreviousOwner + "~" + CareOf + "~" + MailingAddress + "~" + Location + "~" + Subdivision + "~" + Will + "~" + DeedBookPage + "~" + DeedDate + "~" + PlatBookPage;
                    string assessmentdetail = Acreage + "~" + Description + "~" + SalePrice + "~" + Jurisdiction + "~" + HomesteadCode + "~" + AssessmentClass + "~" + landuse + "~" + FairMarketValue + "~" + TaxableMarketValue + "~" + TotalRollback + "~" + Taxes;
                    gc.insert_date(orderNumber, Parcel_number, 2094, Propertydetail, 1, DateTime.Now);
                    gc.insert_date(orderNumber, Parcel_number, 2095, assessmentdetail, 1, DateTime.Now);

                    //Tax Site
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("https://www.greenvillecounty.org/appsas400/votaxqry/");
                    driver.FindElement(By.Id("lnk_RealEstate")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "RealEstate Click", driver, "SC", "Greenville");
                    driver.FindElement(By.Id("ctl00_bodyContent_txt_MapNumber")).SendKeys(Parcel_number.Trim());
                    gc.CreatePdf(orderNumber, Parcel_number, "Tax Click Before", driver, "SC", "Greenville");
                    driver.FindElement(By.Id("ctl00_bodyContent_lnk_Search")).Click();
                    Thread.Sleep(2000);
                    try
                    {
                        string Nodatatax = driver.FindElement(By.Id("ctl00_bodyContent_lbl_Count")).Text;
                        if (Nodatatax.Trim() == "0")
                        {
                            gc.CreatePdf(orderNumber, Parcel_number, "No Data Tax", driver, "SC", "Greenville");
                            HttpContext.Current.Session["Zero_Greenville"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    int I = 0;
                    try
                    {
                        IWebElement TaxDetailTable = driver.FindElement(By.XPath("//*[@id='tbl_Results']/tbody"));
                        IList<IWebElement> TaxDetailrow = TaxDetailTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> TaxDetailid;
                        gc.CreatePdf(orderNumber, Parcel_number, "Tax History", driver, "SC", "Greenville");
                        foreach (IWebElement TaxDetail in TaxDetailrow)
                        {
                            TaxDetailid = TaxDetail.FindElements(By.TagName("td"));
                            if (TaxDetailid.Count != 0)
                            {
                                string Span = TaxDetailid[0].FindElement(By.TagName("span")).Text;
                                if (I == 0)
                                {
                                    IList<IWebElement> Linkhref = TaxDetailid[0].FindElements(By.TagName("a"));
                                    foreach (IWebElement Link in Linkhref)
                                    {
                                        if (Link.Text.Contains("View Tax Notice"))
                                        {
                                            string Href = Link.GetAttribute("href");
                                            Taxyearlink.Add(Href);
                                            I++;
                                        }
                                    }
                                }
                                string Map = TaxDetailid[1].Text;
                                string Permit = TaxDetailid[2].Text;
                                string Exmt = TaxDetailid[3].Text;
                                string Assessment = TaxDetailid[4].Text;
                                string Baseamount = TaxDetailid[5].Text;
                                string Balancedue = TaxDetailid[6].Text;
                                if (Balancedue != "")
                                {
                                    commends = "Taxes are Delinquent";
                                    gc.insert_date(orderNumber, Parcel_number, 2129, commends, 1, DateTime.Now);
                                }
                                string Taxlinkresult = Span + "~" + Map + "~" + Permit + "~" + Exmt + "~" + Assessment + "~" + Baseamount + "~" + Balancedue;
                                gc.insert_date(orderNumber, Parcel_number, 2127, Taxlinkresult, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    string Currentwindow = driver.Url;
                    for (int i = 1; i < 4; i++)
                    {
                        driver.Navigate().GoToUrl(Currentwindow);
                        Thread.Sleep(1000);
                        driver.FindElement(By.XPath(" //*[@id='tbl_Results']/tbody/tr[" + i + "]/td[1]/div[1]/a[1]")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, Parcel_number, "Tax Detail" + i, driver, "SC", "Greenville");
                        string Taxyearproperty = driver.FindElement(By.Id("ctl00_body_rpt_Data_ctl01_lbl_TaxYear")).Text;
                        string taxes = driver.FindElement(By.Id("ctl00_body_rpt_Data_ctl01_lbl_TaxesDue")).Text;
                        string taxresult = Taxyearproperty + "~" + taxes + "~" + Tax_Authority;
                        gc.insert_date(orderNumber, Parcel_number, 2128, taxresult, 1, DateTime.Now);

                    }

                    foreach (string Taxyearlink1 in Taxyearlink)
                    {
                        driver.Navigate().GoToUrl(Taxyearlink1);
                        Thread.Sleep(5000);
                        var chromeOptions = new ChromeOptions();
                        var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];
                        chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                        chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                        chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                        chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                        var driver1 = new ChromeDriver(chromeOptions);
                        try
                        {

                            Array.ForEach(Directory.GetFiles(@downloadDirectory), File.Delete);
                            driver1.Navigate().GoToUrl(Taxyearlink1);
                            Thread.Sleep(3000);
                            string filename = "";

                            filename = latestfilename();
                            gc.AutoDownloadFile(orderNumber, Parcel_number, "Greenville", "SC", filename);
                            driver1.Quit();
                        }
                        catch
                        { }
                        driver1.Quit();
                        //gc.CreatePdf(orderNumber, Parcel_number, "Tax Current Pdf", driver, "SC", "Greenville");
                        //// gc.downloadfile(urlpdf, orderNumber, assessment_id, "Propertypdf", "CT", countynameCT);
                        //gc.downloadfile(driver.Url, orderNumber, Parcel_number, "Current Tax", "SC", "Greenville");
                    }

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "SC", "Greenville", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    driver.Quit();
                    gc.mergpdf(orderNumber, "SC", "Greenville");
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

    }

}