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
using System.Web.UI;
using System.Web;
namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_BentonAR
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();

        public string FTP_BentonAR(string streetNo, string streetName, string direction, string streetType, string unitNumber, string parcelNumber, string ownerName, string searchType, string orderNumber)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string Taxing_Authority = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new PhantomJSDriver();
            //driver = new ChromeDriver();
            string address = "";
            using (driver = new PhantomJSDriver()) //ChromeDriver
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("https://www.arcountydata.com/sponsored.asp");
                    Thread.Sleep(3000);
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    if (direction != "")
                    {
                        address = streetNo + " " + direction + " " + streetName + " " + streetType + " " + unitNumber;
                    }
                    else
                    {
                        address = streetNo + " " + streetName + " " + streetType + " " + unitNumber;
                    }

                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, "", ownerName, address.Trim(), "AR", "Benton");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_BentonAR"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        SelectElement ss = new SelectElement(driver.FindElement(By.XPath("/html/body/div[2]/table/tbody/tr[2]/td/form/div/div/select")));
                        ss.SelectByValue("Benton");
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Select Search Input", driver, "AR", "Benton");
                        driver.FindElement(By.XPath("//*[@id='Assessor']/div/div[2]/a")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.Id("StreetNumber")).SendKeys(streetNo);
                        driver.FindElement(By.Id("StreetName")).SendKeys(streetName);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Parcel Search Input", driver, "AR", "Benton");
                        driver.FindElement(By.Id("Search")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Parcel Search Result", driver, "AR", "Benton");

                        //Multiparcel
                        try
                        {
                            //int Count = 0;
                            string matches = driver.FindElement(By.XPath("/html/body/div[2]/div[2]/div[2]/table/tbody/tr[2]/td[2]")).Text;
                            string matches1 = GlobalClass.After(matches, "to").Trim();
                            IWebElement Multiaddresstable = driver.FindElement(By.XPath("//*[@id='parcel_report']/table/tbody"));
                            IList<IWebElement> multiaddressrow = Multiaddresstable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Multiaddressid;
                            foreach (IWebElement Multiaddress in multiaddressrow)
                            {
                                Multiaddressid = Multiaddress.FindElements(By.TagName("td"));
                                if (multiaddressrow.Count > 4 && Multiaddressid.Count != 0 && Multiaddressid.Count == 10 && !Multiaddress.Text.Contains("Parcel #"))
                                {
                                    string Multiparcelnumber = Multiaddressid[0].Text;
                                    string OWnername = Multiaddressid[1].Text;
                                    string Address1 = Multiaddressid[3].Text;

                                    string multiaddressresult = OWnername + "~" + Address1;
                                    gc.insert_date(orderNumber, Multiparcelnumber, 1625, multiaddressresult, 1, DateTime.Now);
                                    //Count++;
                                }
                            }
                            if (multiaddressrow.Count == 4)
                            {
                                driver.FindElement(By.XPath("//*[@id='parcel_report']/table/tbody/tr[2]/td[1]/a")).Click();
                                Thread.Sleep(2000);
                                gc.CreatePdf_WOP(orderNumber, "Parcel Search Result", driver, "AR", "Benton");
                            }

                            if (multiaddressrow.Count > 4 && Convert.ToInt16(matches1) <= 28)
                            {
                                HttpContext.Current.Session["multiParcel_BentonAR"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Convert.ToInt16(matches1) > 29)
                            {
                                HttpContext.Current.Session["multiParcel_BentonAR_Maximum"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("/html/body/div[2]/div[2]/div/div")).Text;
                            if (nodata.Contains("Nothing matching your search criteria was found"))
                            {
                                HttpContext.Current.Session["Nodata_BentonAR"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {
                        // driver.FindElement(By.XPath("/html/body/div[2]/table/tbody/tr[2]/td/form/div/div/select/option[5]"));
                        SelectElement ss = new SelectElement(driver.FindElement(By.XPath("/html/body/div[2]/table/tbody/tr[2]/td/form/div/div/select")));
                        ss.SelectByValue("Benton");
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Select Search Input", driver, "AR", "Benton");
                        driver.FindElement(By.XPath("//*[@id='Assessor']/div/div[2]/a")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.Id("ParcelNumber")).SendKeys(parcelNumber);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Parcel Search Input", driver, "AR", "Benton");
                        driver.FindElement(By.Id("Search")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Parcel Search Result", driver, "AR", "Benton");
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("/html/body/div[2]/div[2]/div/div")).Text;
                            if (nodata.Contains("Nothing matching your search criteria was found"))
                            {
                                HttpContext.Current.Session["Nodata_BentonAR"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "ownername")
                    {
                        SelectElement ss = new SelectElement(driver.FindElement(By.XPath("/html/body/div[2]/table/tbody/tr[2]/td/form/div/div/select")));
                        ss.SelectByValue("Benton");
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Select OwnerSearch Input", driver, "AR", "Benton");
                        driver.FindElement(By.XPath("//*[@id='Assessor']/div/div[2]/a")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.Id("OwnerName")).SendKeys(ownerName);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Owner Search Input", driver, "AR", "Benton");
                        driver.FindElement(By.Id("Search")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Owner Search Result", driver, "AR", "Benton");
                        //Multiparcel
                        try
                        {
                            //int Count = 0;
                            string matches = driver.FindElement(By.XPath("/html/body/div[2]/div[2]/div[2]/table/tbody/tr[2]/td[2]")).Text;
                            string matches1 = GlobalClass.After(matches, "to").Trim();
                            IWebElement Multiaddresstable = driver.FindElement(By.XPath("//*[@id='parcel_report']/table/tbody"));
                            IList<IWebElement> multiaddressrow = Multiaddresstable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Multiaddressid;
                            foreach (IWebElement Multiaddress in multiaddressrow)
                            {
                                Multiaddressid = Multiaddress.FindElements(By.TagName("td"));
                                if (multiaddressrow.Count > 4 && Multiaddressid.Count != 0 && Multiaddressid.Count == 10 && !Multiaddress.Text.Contains("Parcel #"))
                                {
                                    string Multiparcelnumber = Multiaddressid[0].Text;
                                    string OWnername = Multiaddressid[1].Text;
                                    string Address1 = Multiaddressid[3].Text;

                                    string multiaddressresult = OWnername + "~" + Address1;
                                    gc.insert_date(orderNumber, Multiparcelnumber, 1625, multiaddressresult, 1, DateTime.Now);
                                    //Count++;
                                }
                            }
                            if (multiaddressrow.Count == 4)
                            {
                                driver.FindElement(By.XPath("//*[@id='parcel_report']/table/tbody/tr[2]/td[1]/a")).Click();
                                Thread.Sleep(2000);
                                gc.CreatePdf_WOP(orderNumber, "Parcel Search Result", driver, "AR", "Benton");
                            }

                            if (multiaddressrow.Count > 4 && Convert.ToInt16(matches1) <= 28)
                            {
                                HttpContext.Current.Session["multiParcel_BentonAR"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Convert.ToInt16(matches1) > 29)
                            {
                                HttpContext.Current.Session["multiParcel_BentonAR_Maximum"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("/html/body/div[2]/div[2]/div/div")).Text;
                            if (nodata.Contains("Nothing matching your search criteria was found"))
                            {
                                HttpContext.Current.Session["Nodata_BentonAR"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    //Property Details
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='parcel_report']/table/tbody/tr[2]/td[1]/a")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Parcel Search Result", driver, "AR", "Benton");
                    }
                    catch { }
                    string ParcelID = "", CountyName = "", OwnershipInformation = "", PropertyAddress = "", BillingInformation = "", TotalAcres = "", TimberAcres = "", SecTwpRng = "", Subdivision = "", LegalDescription = "", SchoolDistrict = "", TaxStatus = "", Over65 = "", YearBuilt = "";
                    string title = "", value = "";
                    IWebElement Propinfo1 = driver.FindElement(By.XPath("//*[@id='Basic']/div/div[2]/table/tbody"));
                    IList<IWebElement> TRPropinfo1 = Propinfo1.FindElements(By.TagName("tr"));
                    IList<IWebElement> AherfProp;
                    foreach (IWebElement row in TRPropinfo1)
                    {
                        AherfProp = row.FindElements(By.TagName("td"));

                        if (AherfProp.Count != 0 && AherfProp.Count == 2 && AherfProp[0].Text.Trim() != "" && !row.Text.Contains("Parcel Number:"))
                        {
                            title += AherfProp[0].Text.Replace("\r\n", " ").Replace(":", "").Replace("?", "") + "~";
                            value += AherfProp[1].Text.Replace("\r\n", " ").Replace(":", "").Replace("?", "").Replace("Map This Address", "") + "~";
                        }
                        if (AherfProp.Count != 0 && AherfProp.Count == 2 && AherfProp[0].Text.Trim() != "" && row.Text.Contains("Parcel Number:"))
                        {
                            ParcelID = AherfProp[1].Text;
                        }
                    }
                    try
                    {
                        string current = driver.CurrentWindowHandle;
                        driver.FindElement(By.XPath("/html/body/div[2]/div[3]/div/ul/li[5]/a")).Click();
                        Thread.Sleep(2000);
                        driver.SwitchTo().Window(driver.WindowHandles.Last());
                        driver.SwitchTo().Window(current);
                        YearBuilt = driver.FindElement(By.XPath("//*[@id='Improvements']/div/div[2]/table/tbody/tr[10]/td[2]")).Text.Trim();
                        gc.CreatePdf(orderNumber, ParcelID, "Year Built Result", driver, "AR", "Benton");
                    }
                    catch { }
                    db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + title + "Year Built" + "' where Id = '" + 1457 + "'");
                    gc.insert_date(orderNumber, ParcelID, 1457, value + YearBuilt, 1, DateTime.Now);
                    //Land pdf
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[2]/div[3]/div/ul/li[2]/a")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, ParcelID, "Land Pdf", driver, "AR", "Benton");
                    }
                    catch { }

                    //Sales
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[2]/div[3]/div/ul/li[3]/a")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, ParcelID, "Sales Pdf", driver, "AR", "Benton");
                    }
                    catch { }

                    //Assessment Details                    
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[2]/div[3]/div/ul/li[4]/a")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, ParcelID, "Valuation", driver, "AR", "Benton");
                    }
                    catch { }
                    string title1 = "", value1 = "", value2 = "";
                    IWebElement Propinfo2 = driver.FindElement(By.XPath("//*[@id='Valuation']/div/div[2]/table/tbody"));
                    IList<IWebElement> TRPropinfo2 = Propinfo2.FindElements(By.TagName("tr"));
                    IList<IWebElement> AherfProp1;
                    foreach (IWebElement row1 in TRPropinfo2)
                    {
                        AherfProp1 = row1.FindElements(By.TagName("td"));
                        if (!row1.Text.Contains("Entry Appraised Assessed"))
                        {
                            if (AherfProp1.Count != 0 && AherfProp1.Count == 3 && AherfProp1[0].Text.Trim() != "")
                            {
                                title1 += AherfProp1[0].Text + "~";
                            }
                            if (AherfProp1.Count != 0 && AherfProp1.Count == 2 && AherfProp1[0].Text.Trim() != "")
                            {
                                title1 += AherfProp1[0].Text + "~";
                            }
                            if (AherfProp1.Count != 0 && AherfProp1.Count == 3 && AherfProp1[0].Text.Trim() != "")
                            {
                                value1 += AherfProp1[1].Text + "~";
                                value2 += AherfProp1[2].Text + "~";
                            }
                            if (AherfProp1.Count != 0 && AherfProp1.Count == 2 && AherfProp1[0].Text.Trim() != "")
                            {
                                value2 += AherfProp1[1].Text + "~";
                                //value2 += AherfProp1[2].Text + "~";
                            }

                            value1 = value1.TrimEnd('~');
                            value2 = value2.TrimEnd('~');
                            title1 = title1.TrimEnd('~').Replace(":", " ").Trim();
                            gc.insert_date(orderNumber, ParcelID, 1458, title1.Trim() + "~" + value1.Trim() + "~" + value2.Trim(), 1, DateTime.Now);
                            title1 = ""; value1 = ""; value2 = "";
                        }
                    }
                    //Tax Authority Details
                    driver.Navigate().GoToUrl("https://propertytax.ark.org/benton/index.php");

                    string Taxing_Authority1 = driver.FindElement(By.XPath("//*[@id='app-container']/div[3]/h4")).Text;
                    Taxing_Authority = GlobalClass.After(Taxing_Authority1, "MAIN BRANCH").Trim();

                    //Tax Information Details
                    for (int i = 1; i < 4; i++)
                    {
                        driver.Navigate().GoToUrl("https://www.arcountydata.com/propsearch.asp");
                        Thread.Sleep(2000);
                        IWebElement yearrrs = driver.FindElement(By.XPath("//*[@id='SearchPanel']/div/form/div[1]/div[4]/select"));
                        string stryearsplit = yearrrs.Text.Replace("                    \r\n                    ", "");
                        string[] yearsplit = stryearsplit.Trim().Replace("\n", "").Split('\r');

                        IWebElement newyears = driver.FindElement(By.XPath("//*[@id='SearchPanel']/div/form/div[1]/div[4]/select"));
                        SelectElement newss = new SelectElement(driver.FindElement(By.XPath("//*[@id='SearchPanel']/div/form/div[1]/div[4]/select")));
                        newss.SelectByText(yearsplit[i]);
                        driver.FindElement(By.XPath("//*[@id='SearchPanel']/div/form/div[2]/div/input[1]")).Clear();
                        driver.FindElement(By.XPath("//*[@id='SearchPanel']/div/form/div[2]/div/input[1]")).SendKeys(ParcelID);
                        driver.FindElement(By.Id("searchy")).Click();
                        Thread.Sleep(2000);
                        //gc.CreatePdf(orderNumber, ParcelID, "Tax Search Record1", driver, "AR", "Benton");
                        try
                        {
                            string Nodata = driver.FindElement(By.XPath("/html/body/div[2]/div[3]")).Text;
                            if (Nodata.Contains("Nothing Matching"))
                            {
                                driver.FindElement(By.Id("SearchClose")).Click();
                                Thread.Sleep(2000);
                                newyears = driver.FindElement(By.XPath("//*[@id='SearchPanel']/div/form/div[1]/div[4]/select"));
                                SelectElement newsss = new SelectElement(driver.FindElement(By.XPath("//*[@id='SearchPanel']/div/form/div[1]/div[4]/select")));
                                i++;
                                newsss.SelectByText(yearsplit[i]);
                                driver.FindElement(By.XPath("//*[@id='SearchPanel']/div/form/div[2]/div/input[1]")).Clear();
                                driver.FindElement(By.XPath("//*[@id='SearchPanel']/div/form/div[2]/div/input[1]")).SendKeys(ParcelID);
                                driver.FindElement(By.Id("searchy")).Click();
                                Thread.Sleep(2000);
                            }
                        }
                        catch { }
                        driver.FindElement(By.XPath("/html/body/div[2]/table/tbody/tr[2]/td[1]")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, ParcelID, "Tax Search Record2", driver, "AR", "Benton");
                        //Delinquent Details
                        string delininfo = "";
                        try
                        {
                            string delinquentd = driver.FindElement(By.XPath("/html/body/div[2]/table[1]/tbody/tr[14]/td")).Text.Trim();
                            if (delinquentd.Contains("This property has delinquent taxes."))
                            {
                                delininfo = "For tax amount due, you must call the Collector's Office.";
                                gc.insert_date(orderNumber, ParcelID, 1626, delininfo, 1, DateTime.Now);
                            }
                        }
                        catch { }

                        //Proof Of Payment Download
                        try
                        {
                            string current1 = driver.CurrentWindowHandle;
                            driver.FindElement(By.XPath("/html/body/div[2]/table[2]/tbody/tr[3]/td[9]/a")).Click();
                            Thread.Sleep(2000);

                            driver.SwitchTo().Window(driver.WindowHandles.Last());
                            Thread.Sleep(5000);
                            gc.CreatePdf(orderNumber, ParcelID, "Proof Of Payment1", driver, "AR", "Benton");
                            driver.SwitchTo().Window(current1);
                            driver.FindElement(By.XPath("/html/body/div[2]/table[2]/tbody/tr[4]/td[9]/a")).Click();
                            Thread.Sleep(2000);
                            driver.SwitchTo().Window(driver.WindowHandles.Last());
                            Thread.Sleep(5000);
                            gc.CreatePdf(orderNumber, ParcelID, "Proof Of Payment2", driver, "AR", "Benton");
                            driver.SwitchTo().Window(current1);
                        }
                        catch { }
                        //Current Tax Details Table
                        //string Parcel = "", Taxyearbook = "", OwnershipInformation = "", PropertyAddress = "", BillingInformation = "", TotalAcres = "", TimberAcres = "", SecTwpRng = "", Subdivision = "", LegalDescription = "", SchoolDistrict = "", TaxStatus = "", Over65 = "", YearBuilt = "";
                        string Taxyear1 = driver.FindElement(By.XPath("/html/body/div[2]/table[1]/tbody/tr[3]/td[2]")).Text.Trim();
                        string Taxyear = GlobalClass.Before(Taxyear1, "\r\n").Trim();
                        string Taxtitle = "", Taxvalue = "";
                        IWebElement Taxinfo1 = driver.FindElement(By.XPath("/html/body/div[2]/table[1]/tbody"));
                        IList<IWebElement> TRTaxinfo1 = Taxinfo1.FindElements(By.TagName("tr"));
                        IList<IWebElement> AherfTax;
                        foreach (IWebElement Tax in TRTaxinfo1)
                        {
                            AherfTax = Tax.FindElements(By.TagName("td"));

                            if (AherfTax.Count != 0 && AherfTax.Count == 2 && AherfTax[0].Text.Trim() != "" && !Tax.Text.Contains("View Parcel") && !Tax.Text.Contains("Parcel #:") && !Tax.Text.Contains("Proof Of Payment"))
                            {
                                Taxtitle += AherfTax[0].Text.Replace("\r\n", "").Replace(":", "").Replace("?", "") + "~";
                                Taxvalue += AherfTax[1].Text.Replace("\r\n", " ").Replace(":", "").Replace("?", "") + "~";
                            }
                            if (AherfTax.Count != 0 && AherfTax.Count == 4 && AherfTax[0].Text.Trim() != "" && !Tax.Text.Contains("Parcel #:"))
                            {
                                Taxtitle += AherfTax[0].Text.Replace("\r\n", "").Replace(":", "").Replace("?", "") + "~";
                                Taxvalue += AherfTax[1].Text.Replace("\r\n", "").Replace(":", "").Replace("?", "").Replace("Proof Of Payment", "") + "~";
                            }
                        }
                        Taxtitle = Taxtitle.TrimEnd('~').Replace("/", "");
                        Taxvalue = Taxvalue.TrimEnd('~');
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + Taxtitle.Trim() + "~" + "Tax Authority" + "' where Id = '" + 1460 + "'");
                        gc.insert_date(orderNumber, ParcelID, 1460, Taxvalue.Trim() + "~" + Taxing_Authority, 1, DateTime.Now);
                        Taxtitle = ""; Taxvalue = "";
                        //Tax Information Details Table
                        gc.CreatePdf(orderNumber, ParcelID, "Tax Page" + Taxyear, driver, "AR", "Benton");
                        try
                        {
                            string Taxtype = "", Taxdescription = "", District = "", Exempt = "", Assessedvalue = "", Taxowed = "", Taxpaid = "", Balance = "";
                            IWebElement Bigdata4 = driver.FindElement(By.XPath("/html/body/div[2]/table[3]/tbody"));
                            IList<IWebElement> TRBigdata4 = Bigdata4.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDBigdata4;
                            foreach (IWebElement row4 in TRBigdata4)
                            {
                                TDBigdata4 = row4.FindElements(By.TagName("td"));

                                if (TDBigdata4.Count != 0 && TDBigdata4.Count == 8 && !row4.Text.Contains("Tax Type"))
                                {
                                    Taxtype = TDBigdata4[0].Text;
                                    Taxdescription = TDBigdata4[1].Text;
                                    District = TDBigdata4[2].Text;
                                    Exempt = TDBigdata4[3].Text;
                                    Assessedvalue = TDBigdata4[4].Text;
                                    Taxowed = TDBigdata4[5].Text;
                                    Taxpaid = TDBigdata4[6].Text;
                                    Balance = TDBigdata4[7].Text;
                                    string Taxinfodetails = Taxyear.Trim() + "~" + Taxtype.Trim() + "~" + Taxdescription.Trim() + "~" + District.Trim() + "~" + Exempt.Trim() + "~" + Assessedvalue.Trim() + "~" + Taxowed.Trim() + "~" + Taxpaid.Trim() + "~" + Balance.Trim();
                                    gc.insert_date(orderNumber, ParcelID, 1624, Taxinfodetails, 1, DateTime.Now);
                                }
                                if (TDBigdata4.Count != 0 && TDBigdata4.Count == 4 && !row4.Text.Contains("Tax Type"))
                                {
                                    Taxtype = TDBigdata4[0].Text;
                                    Taxowed = TDBigdata4[1].Text;
                                    Taxpaid = TDBigdata4[2].Text;
                                    Balance = TDBigdata4[3].Text;
                                    string Taxinfodetails = Taxyear.Trim() + "~" + Taxtype.Trim() + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + Taxowed.Trim() + "~" + Taxpaid.Trim() + "~" + Balance.Trim();
                                    gc.insert_date(orderNumber, ParcelID, 1624, Taxinfodetails, 1, DateTime.Now);
                                }
                            }
                        }
                        catch { }
                        //Same Data But Different Xpath
                        try
                        {
                            string Taxtype = "", Taxdescription = "", District = "", Exempt = "", Assessedvalue = "", Taxowed = "", Taxpaid = "", Balance = "";
                            IWebElement Bigdata4 = driver.FindElement(By.XPath("/html/body/div[2]/table[2]/tbody"));
                            IList<IWebElement> TRBigdata4 = Bigdata4.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDBigdata4;
                            foreach (IWebElement row4 in TRBigdata4)
                            {
                                TDBigdata4 = row4.FindElements(By.TagName("td"));

                                if (TDBigdata4.Count != 0 && TDBigdata4.Count == 8 && !row4.Text.Contains("Tax Type"))
                                {
                                    Taxtype = TDBigdata4[0].Text;
                                    Taxdescription = TDBigdata4[1].Text;
                                    District = TDBigdata4[2].Text;
                                    Exempt = TDBigdata4[3].Text;
                                    Assessedvalue = TDBigdata4[4].Text;
                                    Taxowed = TDBigdata4[5].Text;
                                    Taxpaid = TDBigdata4[6].Text;
                                    Balance = TDBigdata4[7].Text;
                                    string Taxinfodetails = Taxyear.Trim() + "~" + Taxtype.Trim() + "~" + Taxdescription.Trim() + "~" + District.Trim() + "~" + Exempt.Trim() + "~" + Assessedvalue.Trim() + "~" + Taxowed.Trim() + "~" + Taxpaid.Trim() + "~" + Balance.Trim();
                                    gc.insert_date(orderNumber, ParcelID, 1624, Taxinfodetails, 1, DateTime.Now);
                                }
                                if (TDBigdata4.Count != 0 && TDBigdata4.Count == 4 && !row4.Text.Contains("Tax Type"))
                                {
                                    Taxtype = TDBigdata4[0].Text;
                                    Taxowed = TDBigdata4[1].Text;
                                    Taxpaid = TDBigdata4[2].Text;
                                    Balance = TDBigdata4[3].Text;
                                    string Taxinfodetails = Taxyear.Trim() + "~" + Taxtype.Trim() + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + Taxowed.Trim() + "~" + Taxpaid.Trim() + "~" + Balance.Trim();
                                    gc.insert_date(orderNumber, ParcelID, 1624, Taxinfodetails, 1, DateTime.Now);
                                }
                            }
                        }
                        catch { }
                    }
                    //Payment Receipts Details Table
                    string Receipt = "", Book = "", TaxYear = "", Receiptdate = "", Cashamt = "", Checkamt = "", Creditamt = "", Total = "";
                    IWebElement Bigdata3 = driver.FindElement(By.XPath("/html/body/div[2]/table[2]/tbody"));
                    IList<IWebElement> TRBigdata3 = Bigdata3.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDBigdata3;
                    foreach (IWebElement row3 in TRBigdata3)
                    {
                        TDBigdata3 = row3.FindElements(By.TagName("td"));

                        if (TDBigdata3.Count != 0 && TDBigdata3.Count == 9 && !row3.Text.Contains("Receipt #"))
                        {
                            Receipt = TDBigdata3[0].Text;
                            Book = TDBigdata3[1].Text;
                            TaxYear = TDBigdata3[2].Text;
                            Receiptdate = TDBigdata3[3].Text;
                            Cashamt = TDBigdata3[4].Text;
                            Checkamt = TDBigdata3[5].Text;
                            Creditamt = TDBigdata3[6].Text;
                            Total = TDBigdata3[7].Text;
                            string Paymentreceiptdetails = Receipt.Trim() + "~" + Book.Trim() + "~" + TaxYear.Trim() + "~" + Receiptdate.Trim() + "~" + Cashamt.Trim() + "~" + Checkamt.Trim() + "~" + Creditamt.Trim() + "~" + Total.Trim();
                            gc.insert_date(orderNumber, ParcelID, 1623, Paymentreceiptdetails, 1, DateTime.Now);
                        }
                    }
                    //All Years Screeshot
                    driver.Navigate().GoToUrl("https://www.arcountydata.com/propsearch.asp");
                    Thread.Sleep(2000);
                    IWebElement newyears1 = driver.FindElement(By.XPath("//*[@id='SearchPanel']/div/form/div[1]/div[4]/select"));
                    SelectElement newss1 = new SelectElement(driver.FindElement(By.XPath("//*[@id='SearchPanel']/div/form/div[1]/div[4]/select")));
                    newss1.SelectByText("All Years");
                    driver.FindElement(By.Id("searchy")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, ParcelID, "All Years", driver, "AR", "Benton");
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "AR", "Benton", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    driver.Quit();
                    gc.mergpdf(orderNumber, "AR", "Benton");
                    return "Data Inserted Successfully";
                }
                catch (Exception ex)
                {
                    driver.Quit();
                    throw ex;
                }
            }
        }
    }
}