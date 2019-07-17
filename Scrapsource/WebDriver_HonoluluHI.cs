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
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_HonoluluHI
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        public string FTP_HonoluluHI(string houseno, string Direction, string sname, string stype, string account, string parcelNumber, string ownername, string searchType, string orderNumber)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            string outparcelno = "", taxparcel = "", mailingaddress = "", tax_district = "", siteaddr = "", legal_desc = "", year_built = "", propuse = "", par1 = "", par2 = "";
            string Building_Value = "", LandValue = "", JustValue = "", cctaxyear = "", Cap = "", AssessedValue = "", Exemption = "", TaxableValue = "";
            string strBill = "-", strBalance = "-", strBillDate = "-", strBillPaid = "-";
            string taxowner = "", tax_addr = "", accno = "", alterkey = "", millagecode = "", milagerate = "", ctax_year = "", cpaiddate = "", cpaidamount = "", creceipt = "", combinedtaxamount = "", ceffdate = "", grosstax = "", ifpaidby = "", pleasepay = "";
            string Address="", taxauth1 = "", taxauth2 = "", taxauth3 = "", taxauth4 = "", TaxYear = ""; 
            List<string> strissuecertificate = new List<string>();
            List<string> taxhistorylink = new List<string>();
            List<string> downloadlink = new List<string>();
            List<string> taxhistorylinkinst = new List<string>();

            string multi = "", TaxAuthority = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                // driver = new ChromeDriver();
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    try
                    {
                        driver.Navigate().GoToUrl("http://www.qpublic.net/hi/honolulu/contact.html");
                        Thread.Sleep(1000);
                        taxauth1 = driver.FindElement(By.XPath("//*[@id='xr_xri']/div[1]/span[14]")).Text;
                        taxauth2 = driver.FindElement(By.XPath("//*[@id='xr_xri']/div[1]/span[15]")).Text;
                        taxauth3 = driver.FindElement(By.XPath("//*[@id='xr_xri']/div[1]/span[16]")).Text;
                        taxauth4 = driver.FindElement(By.XPath("//*[@id='xr_xri']/div[1]/span[17]")).Text;
                        TaxAuthority = taxauth1 + " " + taxauth2 + " " + taxauth3 + " " + taxauth4;
                        gc.CreatePdf_WOP(orderNumber, "Taxing Authority", driver, "HI", "Honolulu");
                    }
                    catch { }

                    driver.Navigate().GoToUrl("http://qpublic9.qpublic.net/hi_honolulu_search.php");
                    Thread.Sleep(1000);

                    string Parcelno = "", Ownername = "", parcellocation = "";
                    if (searchType == "titleflex")
                    {

                        gc.TitleFlexSearch(orderNumber, "", ownername, "", "HI", "Honolulu");
                        //gc.TitleFlexSearch(orderNumber, "", "", address.Trim(), "HI", "Honolulu");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_HonoluluHI"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                        parcelNumber = parcelNumber.Replace("-", "");

                    }
                    if (searchType == "address")
                    {

                        try
                        {
                            driver.FindElement(By.LinkText("Search by Site Address")).Click();
                            Thread.Sleep(1000);

                            if (Direction == "" && account == "")
                            {
                                Address = houseno + " " + sname;
                            }
                            if (Direction == "" && account != "")
                            {
                                Address = houseno + " " + sname + " " + account;
                            }
                            if (Direction != "" && account != "")
                            {
                                Address = houseno + " " + Direction + " " + sname + " " + account;
                            }
                            if (Direction != "" && account == "")
                            {
                                Address = houseno + " " + Direction + " " + sname;
                            }

                            driver.FindElement(By.XPath("/html/body/form/table/tbody/tr/td[2]/table/tbody/tr[1]/td[2]/input")).SendKeys(houseno);
                            driver.FindElement(By.XPath("/html/body/form/table/tbody/tr/td[2]/table/tbody/tr[3]/td[2]/input")).SendKeys(Direction);
                            driver.FindElement(By.Id("streetName")).SendKeys(sname);
                            driver.FindElement(By.XPath("/html/body/form/table/tbody/tr/td[2]/table/tbody/tr[4]/td[2]/input")).SendKeys(account);
                            gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "HI", "Honolulu");
                            driver.FindElement(By.XPath("/html/body/form/table/tbody/tr/td[2]/table/tbody/tr[4]/td[2]/input")).SendKeys(Keys.Enter);
                            Thread.Sleep(1000);
                            //IWebElement multirecord = driver.FindElement(By.XPath("//*[@id='mMessage']"));
                        }
                        catch { }
                        gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "HI", "Honolulu");
                        int Max = 0;
                        IWebElement multiaddress = driver.FindElement(By.XPath("/html/body/table/tbody"));
                        IList<IWebElement> TRmultiaddress = multiaddress.FindElements(By.TagName("tr"));
                        IList<IWebElement> THmultiaddress = multiaddress.FindElements(By.TagName("th"));
                        IList<IWebElement> TDmultiaddress;


                        if (TRmultiaddress.Count > 28)
                        {
                            Max++;
                            HttpContext.Current.Session["multiParcel_Honolulu_Maximum"] = "Maimum";
                            driver.Quit();
                            return "Maximum";                           
                        }
                        if (TRmultiaddress.Count == 7)
                        {
                            IWebElement multiclick = driver.FindElement(By.XPath("/html/body/table/tbody/tr[5]/td[1]/a"));
                            multiclick.Click();
                            Max++;
                        }
                        if (TRmultiaddress.Count > 7)
                        {
                            foreach (IWebElement row in TRmultiaddress)
                            {
                                TDmultiaddress = row.FindElements(By.TagName("td"));
                                if (row.Text.Contains(Address.ToUpper()) && !row.Text.Contains("Parcel Number") && !row.Text.Contains("Search") && row.Text.Trim() != "" && TDmultiaddress.Count == 4)
                                {
                                    try
                                    {
                                        Parcelno = TDmultiaddress[0].Text.Trim();
                                        Ownername = TDmultiaddress[1].Text.Trim();
                                        parcellocation = TDmultiaddress[2].Text.Trim();
                                        string Multi = Ownername + "~" + parcellocation;
                                        gc.insert_date(orderNumber, Parcelno, 1560, Multi, 1, DateTime.Now);
                                        Max++;
                                    }
                                    catch { }

                                }



                            }

                        }
                        if (Max > 1)
                        {
                            HttpContext.Current.Session["multiParcel_Honolulu"] = "Yes";
                            driver.Quit();
                            return "MultiParcel";
                        }
                        //if (Max == 1)
                        //{
                        //    IWebElement multiaddress1 = driver.FindElement(By.XPath("/html/body/table/tbody"));
                        //    IList<IWebElement> TRmultiaddress1 = multiaddress1.FindElements(By.TagName("tr"));
                        //    IList<IWebElement> THmultiaddress1 = multiaddress1.FindElements(By.TagName("th"));
                        //    IList<IWebElement> TDmultiaddress1;
                        //    foreach (IWebElement row in TRmultiaddress1)
                        //    {
                        //        TDmultiaddress1 = row.FindElements(By.TagName("td"));
                        //        if (row.Text.Contains(Address.ToUpper()) && !row.Text.Contains("Parcel Number") && !row.Text.Contains("Search") && row.Text.Trim() != "" && TDmultiaddress1.Count == 4)
                        //        {
                        //            try
                        //            {
                        //                IWebElement IRealEstate = TDmultiaddress1[0].FindElement(By.TagName("a"));
                        //                IRealEstate.Click();
                        //                Thread.Sleep(2000);
                        //                break;
                        //            }
                        //            catch { }

                        //        }

                        //    }
                        //}
                        if (Max == 0)
                        {
                            HttpContext.Current.Session["Nodata_HonoluluHI"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }

                    if (searchType == "parcel")
                    {

                        try
                        {
                            driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td/ul/li[2]/b/a")).SendKeys(Keys.Enter);
                            Thread.Sleep(1000);
                        }
                        catch { }
                        string s1 = "", s2 = "", s3 = "", s4 = "", s5 = "", s6 = "";
                        if (Convert.ToInt16(parcelNumber.Replace("-", "").Count()) != 12)
                        {
                            string[] parcelSplit = parcelNumber.Split('-');
                            s1 = parcelSplit[0];
                            s2 = parcelSplit[1];
                            s3 = parcelSplit[2];
                            s4 = parcelSplit[3];
                            s5 = parcelSplit[4];
                            s6 = parcelSplit[5];
                            parcelNumber = s2 + s3 + s4 + s5 + s6;
                        }
                        else
                        {
                            parcelNumber = parcelNumber.Replace("-", "");
                        }
                        driver.FindElement(By.XPath("/html/body/form/table/tbody/tr/td/input[2]")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "HI", "Honolulu");
                        driver.FindElement(By.XPath("/html/body/form/table/tbody/tr/td/input[5]")).SendKeys(Keys.Enter);
                        Thread.Sleep(1000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Result", driver, "HI", "Honolulu");
                        try
                        {
                            IWebElement Iclick = driver.FindElement(By.XPath("/html/body/table/tbody/tr[5]/td[1]/a"));
                            Iclick.Click();
                            Thread.Sleep(2000);
                        }
                        catch { }

                        int Max = 0;
                        try
                        {
                            IWebElement multiaddress = driver.FindElement(By.XPath("/html/body/table/tbody"));
                            IList<IWebElement> TRmultiaddress = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> THmultiaddress = multiaddress.FindElements(By.TagName("th"));
                            IList<IWebElement> TDmultiaddress;


                            if (TRmultiaddress.Count > 28)
                            {
                                Max++;
                                HttpContext.Current.Session["multiParcel_Honolulu_Maximum"] = "Maimum";
                                driver.Quit();
                                return "Maximum";                                
                            }
                            if (TRmultiaddress.Count == 7)
                            {
                                IWebElement multiclick = driver.FindElement(By.XPath("/html/body/table/tbody/tr[5]/td[1]/a"));
                                multiclick.Click();
                                Max++;
                            }
                            if (TRmultiaddress.Count > 7)
                            {
                                foreach (IWebElement row in TRmultiaddress)
                                {
                                    TDmultiaddress = row.FindElements(By.TagName("td"));
                                    if (!row.Text.Contains("Parcel Number") && !row.Text.Contains("Search") && row.Text.Trim() != "" && TDmultiaddress.Count == 4)
                                    {
                                        try
                                        {
                                            Parcelno = TDmultiaddress[0].Text.Trim();
                                            Ownername = TDmultiaddress[1].Text.Trim();
                                            parcellocation = TDmultiaddress[2].Text.Trim();
                                            string Multi = Ownername + "~" + parcellocation;
                                            gc.insert_date(orderNumber, Parcelno, 1560, Multi, 1, DateTime.Now);
                                            Max++;
                                        }
                                        catch { }

                                    }



                                }
                                HttpContext.Current.Session["multiParcel_Honolulu"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            //if (TRmultiaddress.Count <= 4)
                            //{
                            //    //TDmultiaddress[0].Click();
                            //    driver.FindElement(By.XPath("//*[@id='searchResults']/tbody/tr[3]/td[1]/table/tbody/tr/td[2]/font")).Click();
                            //    Thread.Sleep(1000);
                            //    Max++;
                            //}
                            if (Max == 0)
                            {
                                HttpContext.Current.Session["Nodata_HonoluluHI"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    //if (searchType == "ownername")
                    //{

                    //    try
                    //    {
                    //        driver.FindElement(By.LinkText("Owner Name")).SendKeys(Keys.Enter);
                    //        Thread.Sleep(1000);
                    //    }
                    //    catch { }

                    //    driver.FindElement(By.XPath("/html/body/form/table/tbody/tr/td/input[2]")).SendKeys(ownername);
                    //    gc.CreatePdf_WOP(orderNumber, "Ownername Search", driver, "HI", "Honolulu");
                    //    driver.FindElement(By.XPath("/html/body/form/table/tbody/tr/td/input[5]")).SendKeys(Keys.Enter);
                    //    Thread.Sleep(1000);
                    //    gc.CreatePdf_WOP(orderNumber, "Ownername Search Result", driver, "HI", "Honolulu");

                    //    string ParcelNum = "", Owner_Name = "", ParcelLocation = "";
                    //    int Max = 0;
                    //    IWebElement multiaddress = driver.FindElement(By.XPath("/html/body/table/tbody"));
                    //    IList<IWebElement> TRmultiaddress = multiaddress.FindElements(By.TagName("tr"));
                    //    IList<IWebElement> THmultiaddress = multiaddress.FindElements(By.TagName("th"));
                    //    IList<IWebElement> TDmultiaddress;


                    //    if (TRmultiaddress.Count > 28)
                    //    {
                    //        HttpContext.Current.Session["multiParcel_Honolulu_Maximum"] = "Maimum";
                    //        return "Maximum";
                    //        Max++;
                    //    }
                    //    if (TRmultiaddress.Count == 7)
                    //    {
                    //        IWebElement multiclick = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[1]/a"));
                    //        multiclick.Click();
                    //        Max++;
                    //    }
                    //    if (TRmultiaddress.Count > 7)
                    //    {
                    //        foreach (IWebElement row in TRmultiaddress)
                    //        {
                    //            TDmultiaddress = row.FindElements(By.TagName("td"));
                    //            if (!row.Text.Contains("Parcel Number") && !row.Text.Contains("Search") && row.Text.Trim() != "" && TDmultiaddress.Count >= 7)
                    //            {
                    //                try
                    //                {
                    //                    ParcelNum = TDmultiaddress[0].Text.Trim();
                    //                    Owner_Name = TDmultiaddress[1].Text.Trim();
                    //                    ParcelLocation = TDmultiaddress[2].Text.Trim();
                    //                    string Multi = Owner_Name + "~" + ParcelLocation;
                    //                    gc.insert_date(orderNumber, ParcelNum, 1328, Multi, 1, DateTime.Now);
                    //                    Max++;
                    //                }
                    //                catch { }

                    //            }



                    //        }
                    //        HttpContext.Current.Session["multiParcel_Honolulu"] = "Yes";
                    //        driver.Quit();
                    //        return "MultiParcel";
                    //    }
                    //    //if (TRmultiaddress.Count <= 4)
                    //    //{
                    //    //    //TDmultiaddress[0].Click();
                    //    //    driver.FindElement(By.XPath("//*[@id='searchResults']/tbody/tr[3]/td[1]/table/tbody/tr/td[2]/font")).Click();
                    //    //    Thread.Sleep(1000);
                    //    //    Max++;
                    //    //}
                    //    if (Max == 0)
                    //    {
                    //        HttpContext.Current.Session["Zero_Honolulu"] = "Zero";
                    //        driver.Quit();
                    //        return "Zero";
                    //    }
                    //}

                    // Property Details

                    string propertydata = driver.FindElement(By.XPath("/html/body/center[2]/table[2]/tbody")).Text;
                    string LocationAddress = "", Yearbuilt = "", LandArea_Feet = "", MillageRates = "", Propertyclass = "";
                    string OwnerName = "", LandArea_Acres = "", LegalInformation = "";
                    //IWebElement Iparcelno = driver.FindElement(By.XPath("/html/body/center[2]/table[2]/tbody/tr[3]/td[4]"));
                    // parcelNumber = Iparcelno.Text.Trim();
                    parcelNumber = gc.Between(propertydata, "Parcel Number", "Data current").Trim();
                    OwnerName = gc.Between(propertydata, "Owner Name", "Project Name").Trim();
                    LocationAddress = gc.Between(propertydata, "Location Address", "Plat Map").Trim();
                    Propertyclass = gc.Between(propertydata, "Property Class", "Parcel Map").Trim();
                    LandArea_Feet = gc.Between(propertydata, "Land Area (approximate sq ft)", "Legal Information").Trim();
                    try
                    {
                        LandArea_Acres = GlobalClass.After(propertydata, "Land Area (acres)").Trim();
                    }
                    catch { }
                    try
                    {
                        LandArea_Acres = gc.Between(propertydata, "Land Area (acres)", "Any ownership changes").Trim();
                    }
                    catch { }

                    LegalInformation = gc.Between(propertydata, "Legal Information", "Land Area (acres)").Trim();
                    try
                    {
                        IWebElement Iyearbuilt = driver.FindElement(By.XPath("/html/body/center[2]/table[6]/tbody/tr[3]/td[4]"));
                        Yearbuilt = Iyearbuilt.Text.Trim();
                    }
                    catch { }

                    string propertydetails = OwnerName + "~" + LocationAddress + "~" + Propertyclass + "~" + LandArea_Feet + "~" + LandArea_Acres + "~" + LegalInformation + "~" + Yearbuilt;
                    gc.insert_date(orderNumber, parcelNumber, 1555, propertydetails, 1, DateTime.Now);
                    gc.CreatePdf(orderNumber, parcelNumber, "Assessment Details", driver, "HI", "Honolulu");

                    // Assessment Details
                    string valuetype = "", Information = "";
                    try
                    {
                        driver.FindElement(By.LinkText("Show Historical Assessments")).Click();
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Assessment History Details", driver, "HI", "Honolulu");
                    }
                    catch { }

                    try
                    {
                        IWebElement Assessmentdetails = driver.FindElement(By.XPath("/html/body/center[2]/table[3]/tbody"));
                        IList<IWebElement> TRAssessmentdetails = Assessmentdetails.FindElements(By.TagName("tr"));
                        IList<IWebElement> THAssessmentdetails = Assessmentdetails.FindElements(By.TagName("th"));
                        IList<IWebElement> TDAssessmentdetails;
                        foreach (IWebElement row in TRAssessmentdetails)
                        {
                            TDAssessmentdetails = row.FindElements(By.TagName("td"));
                            if (TDAssessmentdetails.Count != 0 && !row.Text.Contains("Assessment Information") && row.Text.Trim() != "" && !row.Text.Contains("Assessment Year") && TDAssessmentdetails.Count == 12)
                            {
                                string AssessmentDetail = TDAssessmentdetails[0].Text + "~" + TDAssessmentdetails[1].Text + "~" + TDAssessmentdetails[2].Text + "~" + TDAssessmentdetails[3].Text + "~" + TDAssessmentdetails[4].Text + "~" + TDAssessmentdetails[5].Text + "~" + TDAssessmentdetails[6].Text + "~" + TDAssessmentdetails[7].Text + "~" + TDAssessmentdetails[8].Text + "~" + TDAssessmentdetails[9].Text + "~" + TDAssessmentdetails[10].Text + "~" + TDAssessmentdetails[11].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1556, AssessmentDetail, 1, DateTime.Now);

                            }
                        }

                    }
                    catch { }



                    // Current tax details
                    try
                    {
                        IWebElement CurrentTax = driver.FindElement(By.XPath("/html/body/center[2]/table[11]/tbody"));
                        IList<IWebElement> TRCurrentTax = CurrentTax.FindElements(By.TagName("tr"));
                        IList<IWebElement> THCurrentTax = CurrentTax.FindElements(By.TagName("th"));
                        IList<IWebElement> TDCurrentTax;
                        foreach (IWebElement row in TRCurrentTax)
                        {
                            TDCurrentTax = row.FindElements(By.TagName("td"));
                            if (TDCurrentTax.Count != 0 && !row.Text.Contains("Current Tax") && row.Text.Trim() != "" && !row.Text.Contains("Tax Period") && !row.Text.Contains("pay online") && TDCurrentTax.Count == 10)
                            {
                                string CurrentTaxDetail1 = TDCurrentTax[0].Text + "~" + TDCurrentTax[1].Text + "~" + TDCurrentTax[2].Text + "~" + TDCurrentTax[3].Text + "~" + TDCurrentTax[4].Text + "~" + TDCurrentTax[5].Text + "~" + TDCurrentTax[6].Text + "~" + TDCurrentTax[7].Text + "~" + TDCurrentTax[8].Text + "~" + TDCurrentTax[9].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1557, CurrentTaxDetail1, 1, DateTime.Now);

                            }
                            if (TDCurrentTax.Count != 0 && !row.Text.Contains("Current Tax") && row.Text.Trim() != "" && !row.Text.Contains("Tax Period") && !row.Text.Contains("pay online") && TDCurrentTax.Count == 2)
                            {
                                string CurrentTaxDetail2 = "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + TDCurrentTax[1].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1557, CurrentTaxDetail2, 1, DateTime.Now);

                            }
                        }

                    }
                    catch { }
                    try
                    {
                        IWebElement CurrentTax = driver.FindElement(By.XPath("/html/body/center[2]/table[10]/tbody"));
                        IList<IWebElement> TRCurrentTax = CurrentTax.FindElements(By.TagName("tr"));
                        IList<IWebElement> THCurrentTax = CurrentTax.FindElements(By.TagName("th"));
                        IList<IWebElement> TDCurrentTax;
                        foreach (IWebElement row in TRCurrentTax)
                        {
                            TDCurrentTax = row.FindElements(By.TagName("td"));
                            if (TDCurrentTax.Count != 0 && !row.Text.Contains("Current Tax") && row.Text.Trim() != "" && !row.Text.Contains("Tax Period") && !row.Text.Contains("pay online") && TDCurrentTax.Count == 10)
                            {
                                string CurrentTaxDetail1 = TDCurrentTax[0].Text + "~" + TDCurrentTax[1].Text + "~" + TDCurrentTax[2].Text + "~" + TDCurrentTax[3].Text + "~" + TDCurrentTax[4].Text + "~" + TDCurrentTax[5].Text + "~" + TDCurrentTax[6].Text + "~" + TDCurrentTax[7].Text + "~" + TDCurrentTax[8].Text + "~" + TDCurrentTax[9].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1557, CurrentTaxDetail1, 1, DateTime.Now);

                            }
                            if (TDCurrentTax.Count != 0 && !row.Text.Contains("Current Tax") && row.Text.Trim() != "" && !row.Text.Contains("Tax Period") && !row.Text.Contains("pay online") && TDCurrentTax.Count == 2)
                            {
                                string CurrentTaxDetail2 = "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + TDCurrentTax[1].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1557, CurrentTaxDetail2, 1, DateTime.Now);

                            }
                        }

                    }
                    catch { }

                    // Tax History Taxes
                    int tyear = 0;
                    try
                    {
                        IWebElement ITaxyear = driver.FindElement(By.XPath("/html/body/center[2]/a/table[1]/tbody/tr[3]/td[1]/a"));
                        TaxYear = ITaxyear.Text;
                        tyear = Convert.ToInt16(TaxYear);
                    }
                    catch { }
                    //try
                    //{
                    //    driver.FindElement(By.LinkText("Show Historical Taxes")).Click();
                    //    Thread.Sleep(2000);
                    //    gc.CreatePdf(orderNumber, parcelNumber, "Tax Histroy Details", driver, "HI", "Honolulu");
                    //}
                    //catch { }

                    try
                    {
                        IWebElement TaxHistroy1 = driver.FindElement(By.XPath("/html/body/center[2]/a/table[1]/tbody"));
                        IList<IWebElement> TRTaxHistroy1 = TaxHistroy1.FindElements(By.TagName("tr"));
                        IList<IWebElement> THTaxHistroy1 = TaxHistroy1.FindElements(By.TagName("th"));
                        IList<IWebElement> TDTaxHistroy1;
                        foreach (IWebElement row in TRTaxHistroy1)
                        {
                            TDTaxHistroy1 = row.FindElements(By.TagName("td"));
                            if (TDTaxHistroy1.Count != 0 && !row.Text.Contains("Historical Tax") && row.Text.Trim() != "" && !row.Text.Contains("Penalty") && !row.Text.Contains("Click") && TDTaxHistroy1.Count == 7)
                            {
                                string TaxHistoryDetail = TDTaxHistroy1[0].Text + "~" + TDTaxHistroy1[1].Text + "~" + TDTaxHistroy1[2].Text + "~" + TDTaxHistroy1[3].Text + "~" + TDTaxHistroy1[4].Text + "~" + TDTaxHistroy1[5].Text + "~" + TDTaxHistroy1[6].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1582, TaxHistoryDetail, 1, DateTime.Now);
                            }

                        }

                    }
                    catch { }
                    string current = driver.CurrentWindowHandle;

                    for (int i = 0; i < 3; i++)
                    {
                        try
                        {
                            IWebElement TaxHistroy = driver.FindElement(By.XPath("/html/body/center[2]/a/table[1]/tbody"));
                            IList<IWebElement> TRTaxHistroy = TaxHistroy.FindElements(By.TagName("tr"));
                            IList<IWebElement> THTaxHistroy = TaxHistroy.FindElements(By.TagName("th"));
                            IList<IWebElement> TDTaxHistroy;
                            foreach (IWebElement row in TRTaxHistroy)
                            {
                                TDTaxHistroy = row.FindElements(By.TagName("td"));
                                if (TDTaxHistroy.Count != 0 && !row.Text.Contains("Historical Tax") && row.Text.Trim() != "" && !row.Text.Contains("Penalty") && !row.Text.Contains("Click") && TDTaxHistroy[0].Text.Trim() == Convert.ToString(tyear) && TDTaxHistroy.Count == 7)
                                {
                                    IWebElement IRealEstate = TDTaxHistroy[0].FindElement(By.TagName("a"));
                                    IRealEstate.Click();
                                    Thread.Sleep(4000);
                                    break;
                                }

                            }

                        }
                        catch { }



                        try
                        {
                            driver.SwitchTo().Window(driver.WindowHandles.Last());
                            Thread.Sleep(2000);
                        }
                        catch { }


                        try
                        {
                            gc.CreatePdf(orderNumber, parcelNumber, "Tax Info Details" + tyear, driver, "HI", "Honolulu");
                            IWebElement TaxPayHistroy = driver.FindElement(By.XPath("/html/body/center[2]/table[4]/tbody"));
                            IList<IWebElement> TRTaxPayHistroy = TaxPayHistroy.FindElements(By.TagName("tr"));
                            IList<IWebElement> THTaxPayHistroy = TaxPayHistroy.FindElements(By.TagName("th"));
                            IList<IWebElement> TDTaxPayHistroy;
                            foreach (IWebElement row in TRTaxPayHistroy)
                            {
                                TDTaxPayHistroy = row.FindElements(By.TagName("td"));
                                if (TDTaxPayHistroy.Count != 0 && !row.Text.Contains("Tax Payments") && row.Text.Trim() != "" && !row.Text.Contains("Payment Sequence") && !row.Text.Contains("Totals") && TDTaxPayHistroy.Count == 7)
                                {
                                    string TaxPayHistoryDetail1 = TDTaxPayHistroy[0].Text + "~" + TDTaxPayHistroy[1].Text + "~" + TDTaxPayHistroy[2].Text + "~" + TDTaxPayHistroy[3].Text + "~" + TDTaxPayHistroy[4].Text + "~" + TDTaxPayHistroy[5].Text + "~" + TDTaxPayHistroy[6].Text;
                                    gc.insert_date(orderNumber, parcelNumber, 1559, TaxPayHistoryDetail1, 1, DateTime.Now);
                                }
                                if (TDTaxPayHistroy.Count != 0 && !row.Text.Contains("Tax Payments") && row.Text.Trim() != "" && !row.Text.Contains("Payment Sequence") && row.Text.Contains("Totals") && TDTaxPayHistroy.Count == 5)
                                {
                                    string TaxPayHistoryDetail1 = "" + "~" + TDTaxPayHistroy[0].Text + "~" + "" + "~" + TDTaxPayHistroy[1].Text + "~" + TDTaxPayHistroy[2].Text + "~" + TDTaxPayHistroy[3].Text + "~" + TDTaxPayHistroy[4].Text;
                                    gc.insert_date(orderNumber, parcelNumber, 1559, TaxPayHistoryDetail1, 1, DateTime.Now);
                                }
                                if (TDTaxPayHistroy.Count != 0 && row.Text.Trim() != "" && !row.Text.Contains("Payment Sequence") && row.Text.Contains("No Tax Payments") && TDTaxPayHistroy.Count == 1)
                                {
                                    string TaxPayHistoryDetail1 = "" + "~" + TDTaxPayHistroy[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                    gc.insert_date(orderNumber, parcelNumber, 1559, TaxPayHistoryDetail1, 1, DateTime.Now);
                                }

                            }

                        }
                        catch { }



                        //  Tax Information Details
                        try
                        {
                            IWebElement TaxDetails = driver.FindElement(By.XPath("/html/body/center[2]/table[3]/tbody"));
                            IList<IWebElement> TRTaxDetails = TaxDetails.FindElements(By.TagName("tr"));
                            IList<IWebElement> THTaxDetails = TaxDetails.FindElements(By.TagName("th"));
                            IList<IWebElement> TDTaxDetails;
                            foreach (IWebElement row in TRTaxDetails)
                            {
                                TDTaxDetails = row.FindElements(By.TagName("td"));
                                if (TDTaxDetails.Count != 0 && !row.Text.Contains("Tax Details") && row.Text.Trim() != "" && !row.Text.Contains("Tax Period") && !row.Text.Contains("Totals") && TDTaxDetails.Count == 7)
                                {
                                    string TaxInfoDetail1 = TDTaxDetails[0].Text + "~" + TDTaxDetails[1].Text + "~" + TDTaxDetails[2].Text + "~" + TDTaxDetails[3].Text + "~" + TDTaxDetails[4].Text + "~" + TDTaxDetails[5].Text + "~" + TDTaxDetails[6].Text + "~" + TaxAuthority;
                                    gc.insert_date(orderNumber, parcelNumber, 1558, TaxInfoDetail1, 1, DateTime.Now);

                                }
                                if (TDTaxDetails.Count != 0 && !row.Text.Contains("Tax Details") && row.Text.Trim() != "" && !row.Text.Contains("Tax Period") && row.Text.Contains("Totals") && TDTaxDetails.Count == 6)
                                {
                                    string TaxInfoDetail2 = "" + "~" + TDTaxDetails[0].Text + "~" + TDTaxDetails[1].Text + "~" + TDTaxDetails[2].Text + "~" + TDTaxDetails[3].Text + "~" + TDTaxDetails[4].Text + "~" + TDTaxDetails[5].Text + "~" + TaxAuthority;
                                    gc.insert_date(orderNumber, parcelNumber, 1558, TaxInfoDetail2, 1, DateTime.Now);

                                }
                            }


                        }
                        catch { }

                        //  Tax Credit Details

                        try
                        {
                            IWebElement TaxCredit = driver.FindElement(By.XPath("/html/body/center[2]/table[5]/tbody"));
                            IList<IWebElement> TRTaxCredit = TaxCredit.FindElements(By.TagName("tr"));
                            IList<IWebElement> THTaxCredit = TaxCredit.FindElements(By.TagName("th"));
                            IList<IWebElement> TDTaxCredit;
                            foreach (IWebElement row in TRTaxCredit)
                            {
                                TDTaxCredit = row.FindElements(By.TagName("td"));
                                if (TDTaxCredit.Count != 0 && !row.Text.Contains("Tax Credits") && row.Text.Trim() != "" && !row.Text.Contains("Period") && !row.Text.Contains("Total") && TDTaxCredit.Count == 3)
                                {
                                    string TaxCreditDetail1 = TDTaxCredit[0].Text + "~" + TDTaxCredit[1].Text + "~" + TDTaxCredit[2].Text;
                                    gc.insert_date(orderNumber, parcelNumber, 1584, TaxCreditDetail1, 1, DateTime.Now);

                                }
                                if (TDTaxCredit.Count != 0 && !row.Text.Contains("Tax Credits") && row.Text.Trim() != "" && !row.Text.Contains("Period") && row.Text.Contains("Total") && TDTaxCredit.Count == 2)
                                {
                                    string TaxCreditDetail2 = "" + "~" + TDTaxCredit[0].Text + "~" + TDTaxCredit[1].Text;
                                    gc.insert_date(orderNumber, parcelNumber, 1584, TaxCreditDetail2, 1, DateTime.Now);

                                }
                                if (TDTaxCredit.Count != 0 && row.Text.Trim() != "" && !row.Text.Contains("Period") && !row.Text.Contains("Total") && row.Text.Contains("No Tax Credits applicable") && TDTaxCredit.Count == 1)
                                {
                                    string TaxCreditDetail2 = "" + "~" + TDTaxCredit[0].Text + "~" + "";
                                    gc.insert_date(orderNumber, parcelNumber, 1584, TaxCreditDetail2, 1, DateTime.Now);

                                }
                            }


                        }
                        catch { }

                        driver.SwitchTo().Window(current);
                        Thread.Sleep(3000);
                        tyear--;


                    }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Quit();

                    gc.mergpdf(orderNumber, "HI", "Honolulu");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "HI", "Honolulu", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
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