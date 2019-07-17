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
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_SpokaneWA
    {
        List<string> AddressSearch = new List<string>();
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        List<string> RevaluationNotice = new List<string>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="streetno"></param>
        /// <param name="streetname"></param>
        /// <param name="direction"></param>
        /// <param name="streetype"></param>
        /// <param name="unitno"></param>
        /// <param name="ownername"></param>
        /// <param name="parcelNumber"></param>
        /// <param name="searchType"></param>
        /// <param name="orderNumber"></param>
        /// <param name="directParcel"></param>
        /// <returns></returns>
        public string FTP_Spokane(string streetno, string streetname, string direction, string streetype, string unitno, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            List<string> multiparcel = new List<string>();
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "", TaxAuthority = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                StartTime = DateTime.Now.ToString("HH:mm:ss");
                driver.Navigate().GoToUrl("https://cp.spokanecounty.org/scout/propertyinformation/");

                try
                {
                    if (searchType == "titleflex")
                    {
                        string address = streetno + " " + direction + " " + streetname + " " + streetype + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, parcelNumber, ownername, address, "WA", "Spokane");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_SpokaneWA"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        try
                        {
                            driver.Navigate().GoToUrl("https://cp.spokanecounty.org/scout/propertyinformation/SearchADV.aspx");
                        }
                        catch { }

                        driver.FindElement(By.Id("MainContent_txtFromStreet")).SendKeys(streetno);
                        IWebElement ISelect = driver.FindElement(By.Id("MainContent_ddlStreetDir"));
                        SelectElement sSelect = new SelectElement(ISelect);
                        sSelect.SelectByValue(direction);
                        IWebElement ISelect1 = driver.FindElement(By.Id("MainContent_ddlStreetType"));
                        SelectElement sSelect1 = new SelectElement(ISelect1);
                        sSelect1.SelectByValue(streetype);
                        driver.FindElement(By.Id("MainContent_txtStreetName")).SendKeys(streetname);
                        gc.CreatePdf_WOP(orderNumber, "SearchAddressBefore", driver, "WA", "Spokane");
                        driver.FindElement(By.Id("MainContent_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "SearchAddressAfter", driver, "WA", "Spokane");
                        //Multi Parcel
                        try
                        {
                            IWebElement Multiaddresstable = driver.FindElement(By.XPath("//*[@id='MainContent_GridView1']/tbody"));
                            IList<IWebElement> multiaddressrow = Multiaddresstable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Mutiaddressid;
                            foreach (IWebElement Multiaddress in multiaddressrow)
                            {
                                Mutiaddressid = Multiaddress.FindElements(By.TagName("td"));
                                if (multiaddressrow.Count != 0 && multiaddressrow.Count < 26)
                                {
                                    string Parcelnumber = Mutiaddressid[1].Text;
                                    string multiaddressresult = Mutiaddressid[3].Text + "~" + Mutiaddressid[6].Text;
                                    gc.insert_date(orderNumber, Parcelnumber, 943, multiaddressresult, 1, DateTime.Now);
                                }

                            }
                            if (multiaddressrow.Count > 1 && multiaddressrow.Count < 26)
                            {
                                HttpContext.Current.Session["multiparcel_Spokane"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (multiaddressrow.Count > 1 && multiaddressrow.Count > 25)
                            {
                                HttpContext.Current.Session["multiparcel_Spokane_Maximum"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("MainContent_lblMsg")).Text;
                            if (nodata.Contains("There were no records found."))
                            {
                                HttpContext.Current.Session["Nodata_SpokaneWA"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.Id("txtSearch")).SendKeys(parcelNumber);
                        driver.FindElement(By.Id("MainContent_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "SearchAddressBefore", driver, "WA", "Spokane");
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("MainContent_lblMsg")).Text;
                            if (nodata.Contains("There were no records found.") || nodata.Contains("No record found by that search criteria."))
                            {
                                HttpContext.Current.Session["Nodata_SpokaneWA"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }

                    }
                    //Property Details
                    string PropertyDetails = "", ParcelNumber = "", ParcelNumber1 = "", OwnerName = "", OwnerAddress1 = "", OwnerAddress = "", TaxpayerName = "", TaxPayerAddress = "", ParcelType = "", SiteAddress = "", City = "", Description = "", TaxCodeArea = "", Status = "", ParcelClass = "", NeighborhoodCode = "", NeighborhoodName = "", NeighborhoodDesc = "", YearBuilt = "", Acreage = "";

                    ParcelNumber1 = driver.FindElement(By.Id("lblParcel")).Text.Trim();
                    ParcelNumber = GlobalClass.After(ParcelNumber1, "Parcel Number:").Trim();
                    OwnerName = driver.FindElement(By.Id("MainContent_OwnerName_dlOwner_txtNameLabel_0")).Text.Trim();
                    OwnerAddress1 = driver.FindElement(By.Id("MainContent_OwnerName_dlOwner_addressLabel_0")).Text.Trim();
                    TaxpayerName = driver.FindElement(By.Id("MainContent_Taxpayer_dlTaxpayer_txtNameLabel_0")).Text.Trim();
                    TaxPayerAddress = driver.FindElement(By.Id("MainContent_Taxpayer_dlTaxpayer_addressLabel_0")).Text.Trim();

                    try
                    {
                        IWebElement proinfo2 = driver.FindElement(By.XPath("//*[@id='MainContent_SiteAddress_GridView1']/tbody"));
                        IList<IWebElement> TRproinfo2 = proinfo2.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDproinfo2;
                        foreach (IWebElement row in TRproinfo2)
                        {
                            TDproinfo2 = row.FindElements(By.TagName("td"));
                            if (TDproinfo2.Count != 0 && TDproinfo2.Count == 9)
                            {
                                ParcelType = TDproinfo2[0].Text;
                                SiteAddress = TDproinfo2[1].Text;
                                City = TDproinfo2[2].Text;
                                Description = TDproinfo2[5].Text;
                                TaxCodeArea = TDproinfo2[7].Text;
                                Status = TDproinfo2[8].Text;
                            }
                        }
                    }
                    catch { }
                    try
                    {
                        IWebElement proinfo3 = driver.FindElement(By.XPath("//*[@id='MainContent_Appraisal_GridView3']/tbody"));
                        IList<IWebElement> TRproinfo2 = proinfo3.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDproinfo2;
                        foreach (IWebElement row in TRproinfo2)
                        {
                            TDproinfo2 = row.FindElements(By.TagName("td"));
                            if (TDproinfo2.Count != 0 && TDproinfo2.Count == 7)
                            {
                                ParcelClass = TDproinfo2[0].Text;
                                NeighborhoodCode = TDproinfo2[2].Text;
                                NeighborhoodName = TDproinfo2[3].Text;
                                NeighborhoodDesc = TDproinfo2[4].Text;
                            }
                        }
                    }
                    catch { }
                    try
                    {
                        IWebElement proinfo4 = driver.FindElement(By.XPath("//*[@id='MainContent_Dwelling_GridView6']/tbody"));
                        driver.ExecuteJavaScript("arguments[0].removeAttribute('style')", proinfo4);
                        proinfo4 = driver.FindElement(By.XPath("//*[@id='MainContent_Dwelling_GridView6']/tbody"));
                        IList<IWebElement> TRproinfo2 = proinfo4.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDproinfo2;
                        foreach (IWebElement row in TRproinfo2)
                        {
                            TDproinfo2 = row.FindElements(By.TagName("td"));
                            if (TDproinfo2.Count != 0 && TDproinfo2.Count == 12)
                            {
                                IWebElement proinfo54 = TDproinfo2[1];
                                driver.ExecuteJavaScript("arguments[0].removeAttribute('style')", proinfo54);
                                YearBuilt = TDproinfo2[1].Text;

                            }
                        }

                    }
                    catch { }

                    try
                    {
                        IWebElement proinfo5 = driver.FindElement(By.XPath("//*[@id='MainContent_Land_GridView12']/tbody"));
                        IList<IWebElement> TRproinfo2 = proinfo5.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDproinfo2;
                        foreach (IWebElement row in TRproinfo2)
                        {
                            TDproinfo2 = row.FindElements(By.TagName("td"));
                            if (TDproinfo2.Count != 0 && TDproinfo2.Count == 7)
                            {
                                Acreage = TDproinfo2[2].Text;
                            }
                        }
                    }
                    catch { }
                    //Assessed Value Details
                    string AssessdDetails = "", assesstaxyear = "", Taxable = "", Totalvalue = "", Land = "", DwellingandStructure = "", CurrentuseLand = "", Personalprop = "";
                    try
                    {
                        IWebElement assesinfo1 = driver.FindElement(By.XPath("//*[@id='MainContent_AssessedValue_GridView4']/tbody"));
                        IList<IWebElement> TRassessinfo2 = assesinfo1.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDassessinfo2;
                        foreach (IWebElement row in TRassessinfo2)
                        {
                            TDassessinfo2 = row.FindElements(By.TagName("td"));
                            if (TDassessinfo2.Count != 0 && TDassessinfo2.Count == 7)
                            {
                                assesstaxyear = TDassessinfo2[0].Text;
                                Taxable = TDassessinfo2[1].Text;
                                Totalvalue = TDassessinfo2[2].Text;
                                Land = TDassessinfo2[3].Text;
                                DwellingandStructure = TDassessinfo2[4].Text;
                                CurrentuseLand = TDassessinfo2[5].Text;
                                Personalprop = TDassessinfo2[6].Text;
                            }
                            AssessdDetails = assesstaxyear.Trim() + "~" + Taxable.Trim() + "~" + Totalvalue.Trim() + "~" + Land.Trim() + "~" + DwellingandStructure.Trim() + "~" + CurrentuseLand.Trim() + "~" + Personalprop.Trim();
                            gc.insert_date(orderNumber, ParcelNumber, 929, AssessdDetails, 1, DateTime.Now);
                        }
                        AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    }
                    catch { }
                    //Levy Details
                    string LevyDetails = "", Levyname = "", Levyrate2017 = "", Levyrate2018 = "", Levytype = "", TaxID = "";
                    try
                    {
                        IWebElement levyinfo1 = driver.FindElement(By.XPath("//*[@id='MainContent_Levy_GridView1']/tbody"));
                        IList<IWebElement> TRlevyinfo2 = levyinfo1.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDlevyinfo2;
                        foreach (IWebElement row in TRlevyinfo2)
                        {
                            TDlevyinfo2 = row.FindElements(By.TagName("td"));
                            if (TDlevyinfo2.Count != 0 && TDlevyinfo2.Count == 5)
                            {
                                Levyname = TDlevyinfo2[0].Text;
                                Levyrate2017 = TDlevyinfo2[1].Text;
                                Levyrate2018 = TDlevyinfo2[2].Text;
                                Levytype = TDlevyinfo2[3].Text;
                                TaxID = TDlevyinfo2[4].Text;
                            }

                            LevyDetails = Levyname.Trim() + "~" + Levyrate2017.Trim() + "~" + Levyrate2018.Trim() + "~" + Levytype.Trim() + "~" + TaxID.Trim();
                            gc.insert_date(orderNumber, ParcelNumber, 932, LevyDetails, 1, DateTime.Now);
                        }
                    }
                    catch { }
                    //Tax Information Details
                    //1.Default Tax Delinquent Scenario
                    try
                    {//MainContent_TaxInfo_lblMsg....There are special circumstances regarding this parcel. Please call (509) 477-4713 for tax information.
                        string defaulttax = driver.FindElement(By.Id("MainContent_TaxInfo_lblMsg")).Text.Trim();
                        if (defaulttax.Contains("There are special circumstances regarding this parcel. Please call (509) 477-4713 for tax information."))
                        {
                            string InformationComments = "For prior tax amount due, you must call (509) 477-4713 the Collector's Office.";
                            string alertmessage = InformationComments;
                            gc.insert_date(orderNumber, ParcelNumber, 1187, alertmessage, 1, DateTime.Now);
                        }
                    }
                    catch { }
                    string TaxDueDates = "", TaxinformationDetails = "", Propertytaxyear = "", ChargeType = "", AnnualCharges = "", RemainingChargesOwning = "";
                    try
                    {

                        TaxDueDates = driver.FindElement(By.Id("MainContent_TaxInfo_lblTaxDue")).Text.Trim();
                    }
                    catch { }
                    try
                    {
                        IWebElement taxinfo1 = driver.FindElement(By.XPath("//*[@id='MainContent_TaxInfo_GridView16']/tbody"));
                        IList<IWebElement> TRlevyinfo2 = taxinfo1.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDlevyinfo2;
                        foreach (IWebElement row in TRlevyinfo2)
                        {
                            TDlevyinfo2 = row.FindElements(By.TagName("td"));
                            if (TDlevyinfo2.Count != 0 && TDlevyinfo2.Count == 4)
                            {
                                Propertytaxyear = TDlevyinfo2[0].Text;
                                ChargeType = TDlevyinfo2[1].Text;
                                AnnualCharges = TDlevyinfo2[2].Text;
                                RemainingChargesOwning = TDlevyinfo2[3].Text;
                            }
                            TaxinformationDetails = Propertytaxyear.Trim() + "~" + ChargeType.Trim() + "~" + AnnualCharges.Trim() + "~" + RemainingChargesOwning.Trim();
                            gc.insert_date(orderNumber, ParcelNumber, 933, TaxinformationDetails, 1, DateTime.Now);
                        }
                        if (TaxDueDates != "")
                        {
                            gc.insert_date(orderNumber, ParcelNumber, 933, "" + "~" + TaxDueDates.Trim() + "~" + "" + "~" + "", 1, DateTime.Now);
                        }
                    }
                    catch
                    {


                    }


                    //Tax Payment History Details
                    string TaxpaymentHistoryDetails = "", paymenttaxyear = "", receiptnumber = "", receiptdate = "", receiptamount = "";
                    try
                    {
                        IWebElement taxpayment1 = driver.FindElement(By.XPath("//*[@id='MainContent_TaxInfo_GridView17']/tbody"));
                        IList<IWebElement> TRpayment1info2 = taxpayment1.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDpayment1info2;
                        foreach (IWebElement row in TRpayment1info2)
                        {
                            TDpayment1info2 = row.FindElements(By.TagName("td"));
                            if (TDpayment1info2.Count != 0 && TDpayment1info2.Count == 5)
                            {
                                paymenttaxyear = TDpayment1info2[0].Text;
                                receiptnumber = TDpayment1info2[1].Text;
                                receiptdate = TDpayment1info2[2].Text;
                                receiptamount = TDpayment1info2[3].Text;
                            }
                            TaxpaymentHistoryDetails = paymenttaxyear.Trim() + "~" + receiptnumber.Trim() + "~" + receiptdate.Trim() + "~" + receiptamount.Trim();
                            gc.insert_date(orderNumber, ParcelNumber, 940, TaxpaymentHistoryDetails, 1, DateTime.Now);
                        }
                    }
                    catch
                    {


                    }
                    //Last Three Revaluation Notice details

                    try
                    {
                        string taxurl = driver.FindElement(By.XPath("//*[@id='bs-example-navbar-collapse-1']/ul/li[3]/a")).GetAttribute("href");
                        driver.Navigate().GoToUrl(taxurl);
                        gc.CreatePdf(orderNumber, ParcelNumber, " Last Three Year Revaluation Notice details", driver, "WA", "Spokane");

                    }
                    catch
                    {

                    }

                    string fileName1 = "", fileName2 = "";
                    try
                    {
                        // int pcount = 1,Ocount=3;
                        var chromeOptions = new ChromeOptions();
                        var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];
                        chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                        chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                        chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                        chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                        var driver1 = new ChromeDriver(chromeOptions);
                        driver1.Navigate().GoToUrl(driver.Url);

                        // driver1.Navigate().GoToUrl("https://cp.spokanecounty.org/scout/propertyinformation/");
                        driver1.FindElement(By.Id("txtSearch")).SendKeys(ParcelNumber);
                        driver1.FindElement(By.Id("MainContent_btnSearch")).SendKeys(Keys.Enter);
                        driver1.FindElement(By.LinkText("Notices")).SendKeys(Keys.Enter);
                        string current = driver1.CurrentWindowHandle;

                        //IWebElement Receipttable = driver1.FindElement(By.XPath("//*[@id='MainContent_GridView1']/tbody/tr[1]/td/a"));
                        //string BillTax2 = Receipttable.GetAttribute("href");

                        //Receipttable.Click();




                        fileName1 = "ImageOutput";
                        int pcount = 0, Ocount = 0;
                        IWebElement IParcelAssess = driver1.FindElement(By.Id("MainContent_GridView1"));
                        IList<IWebElement> IParcelAssessList = IParcelAssess.FindElements(By.TagName("a"));
                        foreach (IWebElement parcel in IParcelAssessList)
                        {
                            if (parcel.Text.Contains("Parcel Assessment Notice") && pcount < 3)
                            {
                                parcel.Click();
                                Thread.Sleep(10000);
                                gc.AutoDownloadFileSpokane(orderNumber, ParcelNumber, "Spokane", "WA", fileName1 + ".pdf");
                                driver1.SwitchTo().Window(driver1.WindowHandles.Last());
                                gc.CreatePdf(orderNumber, ParcelNumber, "Parcel Assessment Notice" + pcount, driver1, "WA", "Spokane");
                                pcount++;
                                driver1.SwitchTo().Window(current);
                            }
                        }
                        driver1.SwitchTo().Window(current);
                        //fileName2 = "ImageOutput.pdf";
                        IWebElement IOriginalTax = driver1.FindElement(By.Id("MainContent_GridView2"));
                        IList<IWebElement> IOriginalTaxList = IOriginalTax.FindElements(By.TagName("a"));
                        foreach (IWebElement Original in IOriginalTaxList)
                        {
                            if (Original.Text.Contains("Original Tax Statement") && Ocount < 3)
                            {
                                Original.Click();
                                Thread.Sleep(10000);
                                gc.AutoDownloadFileSpokane(orderNumber, ParcelNumber, "Spokane", "WA", fileName1 + ".pdf");
                                driver1.SwitchTo().Window(driver1.WindowHandles.Last());
                                gc.CreatePdf(orderNumber, ParcelNumber, "Original Tax Statement" + Ocount, driver1, "WA", "Spokane");
                                Ocount++;
                                driver1.SwitchTo().Window(current);
                            }
                        }
                        driver1.Quit();

                    }
                    catch { }

                    //Tax Authority
                    string Taxauthority1 = "", Taxauthority2 = "";
                    try
                    {
                        driver.Navigate().GoToUrl("https://www.spokanecounty.org/Directory.aspx?did=62");
                        gc.CreatePdf(orderNumber, ParcelNumber, "TaxAuthority Pdf", driver, "WA", "Spokane");
                        Taxauthority1 = driver.FindElement(By.XPath("//*[@id='CityDirectoryLeftMargin']/span[1]/p[1]")).Text;
                        Taxauthority2 = driver.FindElement(By.XPath("//*[@id='CityDirectoryLeftMargin']/span[1]/p[3]")).Text;
                        TaxAuthority = Taxauthority1 + " Phone:   " + Taxauthority2;
                    }
                    catch { }

                    if (OwnerName != "" && ParcelNumber != "")
                    {
                        PropertyDetails = OwnerName.Trim() + "~" + OwnerAddress1 + "~" + TaxpayerName.Trim() + "~" + TaxPayerAddress.Trim() + "~" + ParcelType.Trim() + "~" + SiteAddress.Trim() + "~" + City.Trim() + "~" + Description.Trim() + "~" + TaxCodeArea.Trim() + "~" + Status.Trim() + "~" + ParcelClass.Trim() + "~" + NeighborhoodCode.Trim() + "~" + NeighborhoodName.Trim() + "~" + NeighborhoodDesc.Trim() + "~" + YearBuilt.Trim() + "~" + Acreage.Trim() + "~" + TaxAuthority;
                        gc.insert_date(orderNumber, ParcelNumber, 927, PropertyDetails, 1, DateTime.Now);
                    }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Quit();
                    gc.mergpdf(orderNumber, "WA", "Spokane");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "WA", "Spokane", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
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