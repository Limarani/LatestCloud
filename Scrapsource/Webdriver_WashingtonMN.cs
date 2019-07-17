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
    public class Webdriver_WashingtonMN
    {
        string outputPath = "";
        IWebDriver driver;
        IWebElement Parcelweb;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_WashingtonMN(string address, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            List<string> viewbilllist = new List<string>();
            List<string> PdfDownloadlink = new List<string>();
            List<string> Yearlink = new List<string>();
            int a = 0;
            string Parcel_number = "", Tax_Authority = "", Parcelhref = "", Yearbuild = "", Pdf = "", Year1 = "", Year2 = "", Addresshrf = "", Year3 = "";
            // driver = new ChromeDriver();
            // driver = new PhantomJSDriver();
            using (driver = new PhantomJSDriver())
            {
                if (searchType == "titleflex")
                {
                    //string address = streetno + " " + direction + " " + streetname + " " + streettype;
                    gc.TitleFlexSearch(orderNumber, "", ownername, address.Trim(), "MN", "Washington");
                    if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                    {
                        driver.Quit();
                        return "MultiParcel";
                    }
                    else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                    {
                        HttpContext.Current.Session["Nodata_WashingtonMN"] = "Yes";
                        driver.Quit();
                        return "No Data Found";
                    }
                    parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                    searchType = "parcel";
                }
                //Tax Authority
                StartTime = DateTime.Now.ToString("HH:mm:ss");
                try
                {
                    driver.Navigate().GoToUrl("https://mn-washington.manatron.com/ContactUs.aspx");
                    string TaxAuthority1 = driver.FindElement(By.Id("dnn_ctr570_HtmlModule_HtmlModule_lblContent")).Text;
                    Tax_Authority = gc.Between(TaxAuthority1, "Services Division", "Telephone:").Trim();
                    gc.CreatePdf_WOP(orderNumber, "Tax Authority", driver, "MN", "Washington");
                }
                catch { }
                driver.Navigate().GoToUrl("https://mn-washington.manatron.com/");
                try
                {
                    if (searchType == "address")
                    {
                        IWebElement SerachCategory = driver.FindElement(By.Id("selSearchBy"));
                        SelectElement selectElement1 = new SelectElement(SerachCategory);
                        selectElement1.SelectByIndex(1);
                        Thread.Sleep(1000);
                        driver.FindElement(By.Id("fldInput")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "Address Search Input Passed", driver, "MN", "Washington");
                        driver.FindElement(By.Id("btnsearch")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "MN", "Washington");
                        int Max = 0;
                        try
                        {
                            IWebElement addressmulti = driver.FindElement(By.Id("grm-search"));
                            IList<IWebElement> Addressrow = addressmulti.FindElements(By.TagName("tr"));
                            IList<IWebElement> Addressid;
                            foreach (IWebElement Addressmultiple in Addressrow)
                            {
                                if (Addressrow.Count != 0)
                                {
                                    Addressid = Addressmultiple.FindElements(By.TagName("td"));
                                    if (Addressid.Count != 0)
                                    {
                                        IWebElement Adresspin = Addressid[2].FindElement(By.TagName("a"));
                                        Parcelhref = Adresspin.GetAttribute("href");
                                        string Accountnumber = Addressid[2].Text;
                                        string Addressresult = Addressid[1].Text + "~" + Addressid[0].Text;
                                        gc.insert_date(orderNumber, Accountnumber, 1849, Addressresult, 1, DateTime.Now);
                                        Max++;
                                    }
                                }
                            }
                            if (Max == 1)
                            {
                                driver.Navigate().GoToUrl(Parcelhref);
                                Thread.Sleep(2000);
                            }
                            if (Addressrow.Count < 26 && Max > 1)
                            {
                                HttpContext.Current.Session["multiparcel_MNwashin"] = "Yes";
                                gc.CreatePdf_WOP(orderNumber, "MultyAddressSearch", driver, "MN", "Washington");
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Addressrow.Count > 25 && Addressrow.Count != 0)
                            {
                                HttpContext.Current.Session["multiParcel_MNwashin_Multicount"] = "Maximum";
                                gc.CreatePdf_WOP(orderNumber, "MultyAddressSearch", driver, "MN", "Washington");
                                driver.Quit();
                                return "Maximum";
                            }
                            if (Addressrow.Count == 0)
                            {
                                HttpContext.Current.Session["Nodata_WashingtonMN"] = "Yes";
                                gc.CreatePdf_WOP(orderNumber, "MultyAddressSearch", driver, "MN", "Washington");
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.Id("fldInput")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "MN", "Washington");
                        driver.FindElement(By.Id("btnsearch")).Click();
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Results", driver, "MN", "Washington");
                        try
                        {
                            IWebElement Parcellink = driver.FindElement(By.XPath("//*[@id='grm-search']/tbody/tr[2]/td[3]")).FindElement(By.TagName("a"));
                            string searchlink = Parcellink.GetAttribute("href");
                            driver.Navigate().GoToUrl(searchlink);
                            Thread.Sleep(2000);
                        }
                        catch { }
                    }

                    try
                    {
                        //No Data Found
                        string nodata = driver.FindElement(By.Id("lxT910")).Text;
                        if (nodata.Contains("No records found"))
                        {
                            HttpContext.Current.Session["Nodata_WashingtonMN"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    gc.CreatePdf_WOP(orderNumber, "Assessment Details", driver, "MN", "Washington");
                    Parcel_number = driver.FindElement(By.XPath("//*[@id='lxT901']/table/tbody/tr[2]/td[1]")).Text.Trim();
                    string Status = driver.FindElement(By.XPath("//*[@id='lxT901']/table/tbody/tr[2]/td[2]")).Text;
                    string CurrentOwner = driver.FindElement(By.XPath("//*[@id='lxT901']/table/tbody/tr[3]/td/table/tbody/tr[1]/td[1]")).Text.Replace("Current Owner:", "");
                    //CurrentOwner = GlobalClass.After(CurrentOwner, "Current Owner:");
                    string Proaddress = driver.FindElement(By.XPath("//*[@id='lxT901']/table/tbody/tr[3]/td/table/tbody/tr[1]/td[2]")).Text;
                    string PropertyAddress = GlobalClass.After(Proaddress, "Property Address:");
                    string TaxDistrict1 = driver.FindElement(By.XPath("//*[@id='lxT901']/table/tbody/tr[3]/td/table/tbody/tr[2]/td[1]")).Text;
                    string TaxDistrict = GlobalClass.After(TaxDistrict1, "Taxing District");
                    string TaxDescription = driver.FindElement(By.XPath("//*[@id='lxT901']/table/tbody/tr[5]/td")).Text;
                    string Currentwindow = driver.CurrentWindowHandle;
                    driver.FindElement(By.LinkText("GIS MAP AND VALUE INFORMATION")).Click();
                    Thread.Sleep(5000);
                    driver.SwitchTo().Window(driver.WindowHandles.Last());
                    string clas = "", Gistable = "", LegalDescription = "";
                    try
                    {
                        gc.CreatePdf(orderNumber, Parcel_number, "GIS MAP AND VALUE INFORMATION", driver, "MN", "Washington");
                        Gistable = driver.FindElement(By.XPath("//*[@id='pclGeneralInfo']/table/tbody")).Text;
                        clas = gc.Between(Gistable, "Class:", "Legal Description:");
                        LegalDescription = GlobalClass.After(Gistable, "Legal Description:");
                        IWebElement AssessmentTable = driver.FindElement(By.Id("priorYearValues"));
                        IList<IWebElement> Assessmentrow = AssessmentTable.FindElements(By.TagName("div"));
                        IList<IWebElement> AssessmentId;
                        foreach (IWebElement Assessment in Assessmentrow)
                        {
                            AssessmentId = Assessment.FindElements(By.TagName("div"));
                            if (AssessmentId.Count != 0 && !Assessment.Text.Contains("Year") && !Assessment.Text.Contains(" More Years...") && Assessment.Text.Trim() != "")
                            {
                                string Assessmentresult = AssessmentId[0].Text + "~" + AssessmentId[1].Text + "~" + AssessmentId[2].Text + "~" + AssessmentId[3].Text + "~" + AssessmentId[4].Text;
                                gc.insert_date(orderNumber, Parcel_number, 1794, Assessmentresult, 1, DateTime.Now);
                            }
                        }
                        try
                        {
                            Yearbuild = driver.FindElement(By.XPath("//*[@id='res0']/div[3]")).Text;

                        }
                        catch { }
                        gc.CreatePdf(orderNumber, Parcel_number, "Tax Main Page", driver, "MN", "Washington");
                    }
                    catch { }
                    int m = 1;
                    string Propertyresult1 = Status + "~" + CurrentOwner + "~" + PropertyAddress + "~" + TaxDistrict + "~" + TaxDescription + "~" + clas + "~" + LegalDescription + "~" + Yearbuild;
                    gc.insert_date(orderNumber, Parcel_number, 1782, Propertyresult1, 1, DateTime.Now);

                    driver.SwitchTo().Window(Currentwindow);
                    Thread.Sleep(2000);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    //*[@id="lxT906"]/table/tbody
                    IWebElement TaxPdfTable = driver.FindElement(By.XPath("//*[@id='lxT1058']/table/tbody"));
                    IList<IWebElement> TaxPdfrow = TaxPdfTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> TaxpdfId;
                    foreach (IWebElement TaxPdf in TaxPdfrow)
                    {
                        TaxpdfId = TaxPdf.FindElements(By.TagName("td"));
                        if (TaxpdfId.Count != 0 && TaxpdfId.Count == 2 && TaxPdf.Text.Contains(" TAX STATEMENT") || TaxPdf.Text.Contains(" VALUATION NOTICE"))
                        {
                            try
                            {
                                IWebElement Downloadlink = TaxpdfId[1].FindElement(By.TagName("a"));
                                Pdf = Downloadlink.GetAttribute("href");
                                PdfDownloadlink.Add(Pdf);
                            }
                            catch { }
                        }
                    }
                    //IWebElement ValuvationPdf = driver.FindElement(By.XPath("//*[@id='lxT1058']/table/tbody/tr[12]/td[2]")).FindElement(By.TagName("a"));
                    //string Valpsf = ValuvationPdf.GetAttribute("href");
                    //PdfDownloadlink.Add(Valpsf);
                    int I = 0;
                    IWebElement TaxyearTable = driver.FindElement(By.XPath("//*[@id='903']/table/tbody"));
                    IList<IWebElement> taxyearrow = TaxyearTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> taxyearid;
                    foreach (IWebElement Taxyear in taxyearrow)
                    {
                        taxyearid = Taxyear.FindElements(By.TagName("td"));
                        if (taxyearid.Count != 0)
                        {
                            if (I < 3)
                            {
                                IWebElement YearLink = taxyearid[0].FindElement(By.TagName("a"));
                                string Yearhref = YearLink.GetAttribute("href");
                                Yearlink.Add(Yearhref);
                                if (I == 0)
                                {
                                    Year1 = taxyearid[0].Text;
                                }
                                if (I == 1)
                                {
                                    Year2 = taxyearid[0].Text;
                                }
                                if (I == 2)
                                {
                                    Year3 = taxyearid[0].Text;
                                }
                                I++;
                            }
                            string Yearresult = taxyearid[0].Text + "~" + taxyearid[1].Text + "~" + taxyearid[2].Text + "~" + taxyearid[3].Text + "~" + taxyearid[4].Text + "~" + taxyearid[5].Text;
                            gc.insert_date(orderNumber, Parcel_number, 1795, Yearresult, 1, DateTime.Now);
                        }
                    }

                    IWebElement TaxyearTablet = driver.FindElement(By.XPath("//*[@id='903']/table/tfoot"));
                    IList<IWebElement> taxyearrowt = TaxyearTablet.FindElements(By.TagName("tr"));
                    IList<IWebElement> taxyearidt;
                    IList<IWebElement> taxyeartht;
                    foreach (IWebElement Taxyeart in taxyearrowt)
                    {
                        taxyearidt = Taxyeart.FindElements(By.TagName("td"));
                        taxyeartht = Taxyeart.FindElements(By.TagName("th"));
                        if (taxyearidt.Count != 0)
                        {
                            string Yearresult = taxyeartht[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + taxyearidt[0].Text;
                            gc.insert_date(orderNumber, Parcel_number, 1795, Yearresult, 1, DateTime.Now);
                        }
                    }

                    int y = 1;
                    int N = 1;
                    foreach (string Tax in Yearlink)
                    {
                        driver.Navigate().GoToUrl(Tax);
                        Thread.Sleep(3000);
                        string Taxyear = "";
                        gc.CreatePdf(orderNumber, Parcel_number, "Tax Details Year " + y, driver, "MN", "Washington");
                        if (y == 1)
                        {
                            Taxyear = Year1;
                        }
                        if (y == 2)
                        {
                            Taxyear = Year2;
                        }
                        if (y == 3)
                        {
                            Taxyear = Year3;
                        }
                        IWebElement Instalmenttable = driver.FindElement(By.XPath("//*[@id='installments']/tbody"));
                        IList<IWebElement> Instalmentrow = Instalmenttable.FindElements(By.TagName("tr"));
                        IList<IWebElement> InstalmentId;
                        foreach (IWebElement Instalment in Instalmentrow)
                        {
                            InstalmentId = Instalment.FindElements(By.TagName("td"));
                            if (InstalmentId.Count != 0 && !Instalment.Text.Contains("Period") && !Instalment.Text.Contains("Total Due:"))
                            {
                                string Instalmentresult = InstalmentId[0].Text + "~" + InstalmentId[1].Text + "~" + InstalmentId[2].Text + "~" + InstalmentId[3].Text + "~" + InstalmentId[4].Text + "~" + InstalmentId[5].Text + "~" + InstalmentId[6].Text + "~" + InstalmentId[7].Text + "~" + Tax_Authority;
                                gc.insert_date(orderNumber, Parcel_number, 1796, Instalmentresult, 1, DateTime.Now);
                            }
                            if (Instalment.Text.Contains("Total Due:"))
                            {
                                string Instalmentresult = "" + "~" + "" + "~" + InstalmentId[0].Text + "~" + "" + "~" + InstalmentId[1].Text + "~" + InstalmentId[2].Text + "~" + InstalmentId[3].Text + "~" + InstalmentId[4].Text + "~" + Tax_Authority;
                                gc.insert_date(orderNumber, Parcel_number, 1796, Instalmentresult, 1, DateTime.Now);
                            }
                        }
                        if (N == 1)
                        {
                            IWebElement TaxPdfTable1 = driver.FindElement(By.XPath("//*[@id='lxT906']/table/tbody"));
                            IList<IWebElement> TaxPdfrow1 = TaxPdfTable1.FindElements(By.TagName("tr"));
                            IList<IWebElement> TaxpdfId1;
                            foreach (IWebElement TaxPdf1 in TaxPdfrow1)
                            {
                                TaxpdfId1 = TaxPdf1.FindElements(By.TagName("td"));
                                if (TaxpdfId1.Count != 0 && TaxpdfId1.Count == 2 && TaxPdf1.Text.Contains("PROPOSED"))
                                {
                                    try
                                    {
                                        IWebElement Downloadlink = TaxpdfId1[1].FindElement(By.TagName("a"));
                                        Pdf = Downloadlink.GetAttribute("href");
                                        PdfDownloadlink.Add(Pdf);
                                        N++;
                                    }
                                    catch { }
                                }
                            }
                        }
                        IWebElement TaxDetailtable = driver.FindElement(By.XPath("//*[@id='lxT897']/table/tbody"));
                        IList<IWebElement> TaxDetailrow = TaxDetailtable.FindElements(By.TagName("tr"));
                        IList<IWebElement> TaxDetailId;
                        IList<IWebElement> TaxDetailTH;
                        foreach (IWebElement TaxDetail in TaxDetailrow)
                        {
                            TaxDetailId = TaxDetail.FindElements(By.TagName("td"));
                            TaxDetailTH = TaxDetail.FindElements(By.TagName("th"));
                            if (TaxDetailId.Count != 0 && !TaxDetail.Text.Contains("Gross Tax") && TaxDetailId.Count == 4)
                            {
                                string Taxdetailresult = Taxyear + "~" + TaxDetailId[0].Text + "~" + TaxDetailId[1].Text + "~" + TaxDetailId[2].Text + "~" + TaxDetailId[3].Text;
                                gc.insert_date(orderNumber, Parcel_number, 1797, Taxdetailresult, 1, DateTime.Now);
                            }
                            if (TaxDetail.Text.Contains("Total Tax"))
                            {
                                string Taxdetailresult = Taxyear + "~" + TaxDetailTH[0].Text + "~" + TaxDetailId[0].Text + "~" + TaxDetailId[1].Text + "~" + TaxDetailId[2].Text;
                                gc.insert_date(orderNumber, Parcel_number, 1797, Taxdetailresult, 1, DateTime.Now);
                            }
                        }


                        try
                        {
                            IWebElement PaymentHistorytable = driver.FindElement(By.XPath("//*[@id='lxT899']/table/tbody"));
                            IList<IWebElement> PaymentHistoryrow = PaymentHistorytable.FindElements(By.TagName("tr"));
                            IList<IWebElement> PaymentHistoryId;
                            foreach (IWebElement PaymentHistory in PaymentHistoryrow)
                            {
                                PaymentHistoryId = PaymentHistory.FindElements(By.TagName("td"));
                                if (PaymentHistoryId.Count == 5 && !PaymentHistory.Text.Contains("Receipt Number"))
                                {
                                    string PaymentHistoryresult = PaymentHistoryId[0].Text + "~" + PaymentHistoryId[1].Text + "~" + PaymentHistoryId[2].Text + "~" + PaymentHistoryId[3].Text + "~" + PaymentHistoryId[4].Text;
                                    gc.insert_date(orderNumber, Parcel_number, 1798, PaymentHistoryresult, 1, DateTime.Now);
                                }
                                if (PaymentHistoryId.Count == 1)
                                {
                                    string PaymentHistoryresult = "" + "~" + "" + "~" + PaymentHistoryId[0].Text + "~" + "" + "~" + "";
                                    gc.insert_date(orderNumber, Parcel_number, 1798, PaymentHistoryresult, 1, DateTime.Now);
                                }
                            }
                        }
                        catch { }
                        y++;
                    }
                    foreach (string Pdfdownload in PdfDownloadlink)
                    {
                        string fileName = "";
                        var chromeOptions = new ChromeOptions();
                        var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];
                        chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                        chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                        chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                        chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                        Array.ForEach(Directory.GetFiles(@downloadDirectory), File.Delete);
                        var chDriver = new ChromeDriver(chromeOptions);
                        try
                        {
                            chDriver.Navigate().GoToUrl(Pdfdownload);
                            Thread.Sleep(8000);
                            fileName = latestfilename();
                            //gc.AutoDownloadFile(orderNumber, Parcel_number, "Washington", "MN", "TaxBill" + m + ".pdf");
                            gc.AutoDownloadFile(orderNumber, Parcel_number, "Washington", "MN", fileName);
                            chDriver.Quit();
                            // m++;
                        }
                        catch (Exception e)
                        {
                            chDriver.Quit();
                        }
                    }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "MN", "Washington", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    driver.Quit();
                    gc.mergpdf(orderNumber, "MN", "Washington");
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