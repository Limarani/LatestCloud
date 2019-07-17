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
    public class WebDriver_BeaufortSC
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        public string FTP_BeaufortSC(string sno, string sname, string direction, string sttype, string unitnumber, string parcelNumber, string searchType, string ownername, string orderNumber)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;

            GlobalClass.global_parcelNo = parcelNumber;
            List<string> taxinformation = new List<string>();
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string address = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new PhantomJSDriver();
            //driver = new ChromeDriver();
            StartTime = DateTime.Now.ToString("HH:mm:ss");
            string Taxauthority = "";
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    if (direction != "")
                    {
                        address = sno + " " + direction + " " + sname + " " + sttype + " " + unitnumber;
                    }
                    else
                    {
                        address = sno + " " + sname + " " + sttype + " " + unitnumber;
                    }

                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, "", "", address, "SC", "Beaufort");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_BeaufortSC"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    driver.Navigate().GoToUrl("https://www.beaufortcountytreasurer.com/contact");

                    string Taxauthority1 = driver.FindElement(By.XPath("//*[@id='child']/div/div/div[2]/div")).Text.Trim();
                    Taxauthority = gc.Between(Taxauthority1, "Beaufort Office", "Bluffton Office").Trim();

                    driver.Navigate().GoToUrl("http://sc-beaufort-county.governmax.com/svc/default.asp?");

                    IWebElement Multyaddresstable2 = driver.FindElement(By.XPath("/html/frameset/frame"));
                    driver.SwitchTo().Frame(Multyaddresstable2);
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[4]/td/a")).Click();
                        Thread.Sleep(2000);
                    }
                    catch { }

                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/a")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Parcel search Input ", driver, "SC", "Beaufort");
                        driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[1]/td/table/tbody/tr/td/table/tbody/tr[2]/td/font/input")).SendKeys(parcelNumber);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Parcel search Result ", driver, "SC", "Beaufort");
                        driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[2]/td/input")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Parcel Enter After ", driver, "SC", "Beaufort");
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[1]/tbody/tr/td/table/tbody/tr[2]/td/font/i/span")).Text;
                            if (nodata.Contains("No Records Found"))
                            {
                                HttpContext.Current.Session["Nodata_BeaufortSC"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "account")
                    {
                        driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[4]/td/a")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "AlternateID search Input ", driver, "SC", "Beaufort");
                        driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr/td/table/tbody/tr[2]/td/font/input")).SendKeys(unitnumber);
                        gc.CreatePdf_WOP(orderNumber, "AlternateID search Result ", driver, "SC", "Beaufort");
                        driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[3]/td/input")).SendKeys(Keys.Enter);
                        Thread.Sleep(5000);
                        gc.CreatePdf_WOP(orderNumber, "AlternateID Enter After ", driver, "SC", "Beaufort");
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[1]/tbody/tr/td/table/tbody/tr[2]/td[1]/font/b/a")).Click();
                            Thread.Sleep(5000);
                            gc.CreatePdf_WOP(orderNumber, "AlternateID Enter After All Datas Showed ", driver, "SC", "Beaufort");

                        }
                        catch { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[1]/tbody/tr/td/table/tbody/tr[2]/td/font/i/span")).Text;
                            if (nodata.Contains("No Records Found"))
                            {
                                HttpContext.Current.Session["Nodata_BeaufortSC"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "address")
                    {
                        driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[3]/td/a")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Address search Input ", driver, "SC", "Beaufort");
                        driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr/td/table/tbody/tr[2]/td/font/input")).SendKeys(address);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Address search Result ", driver, "SC", "Beaufort");
                        driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[3]/td/input")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Address Enter After ", driver, "SC", "Beaufort");
                        ////Multiparcel
                        try
                        {
                            IWebElement Multiaddresstable = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[1]/tbody/tr/td/table/tbody"));
                            IList<IWebElement> multiaddressrow = Multiaddresstable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Multiaddressid;
                            foreach (IWebElement Multiaddress in multiaddressrow)
                            {
                                Multiaddressid = Multiaddress.FindElements(By.TagName("td"));
                                if (multiaddressrow.Count > 1 && Multiaddressid.Count != 0 && Multiaddressid.Count == 3 && !Multiaddress.Text.Contains("Parcel ID"))
                                {
                                    string Multiparcelnumber = Multiaddressid[0].Text;
                                    string OWnername = Multiaddressid[1].Text;
                                    string Address1 = Multiaddressid[2].Text;

                                    string multiaddressresult = OWnername + "~" + Address1;
                                    gc.insert_date(orderNumber, Multiparcelnumber, 1763, multiaddressresult, 1, DateTime.Now);
                                    //Count++;
                                }
                            }
                            if (multiaddressrow.Count < 2)
                            {
                                driver.FindElement(By.XPath("//*[@id='ctl00_BodyContentPlaceHolder_PropertySearchUpdatePanel']/div[3]/table/tbody/tr[1]/td[5]/table/tbody/tr/td/table/tbody/tr[1]/th[1]/a")).Click();
                                Thread.Sleep(2000);
                            }

                            if (multiaddressrow.Count > 2)
                            {
                                HttpContext.Current.Session["multiParcel_BeaufortSC"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (multiaddressrow.Count > 26)
                            {
                                HttpContext.Current.Session["multiParcel_BeaufortSC_Maximum"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }
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
                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[7]/td/a")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Owner search Input ", driver, "SC", "Beaufort");
                        driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr/td/table/tbody/tr[2]/td/font/input")).SendKeys(ownername);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Owner search Result ", driver, "SC", "Beaufort");
                        driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[3]/td/input")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Owner Enter After ", driver, "SC", "Beaufort");

                        ////Multiparcel
                        try
                        {
                            IWebElement Multiaddresstable = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[1]/tbody/tr/td/table/tbody"));
                            IList<IWebElement> multiaddressrow = Multiaddresstable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Multiaddressid;
                            foreach (IWebElement Multiaddress in multiaddressrow)
                            {
                                Multiaddressid = Multiaddress.FindElements(By.TagName("td"));
                                if (multiaddressrow.Count > 1 && Multiaddressid.Count != 0 && Multiaddressid.Count == 3 && !Multiaddress.Text.Contains("Parcel ID"))
                                {
                                    string Multiparcelnumber = Multiaddressid[0].Text;
                                    string OWnername = Multiaddressid[2].Text;
                                    string Address1 = Multiaddressid[1].Text;

                                    string multiaddressresult = OWnername + "~" + Address1;
                                    gc.insert_date(orderNumber, Multiparcelnumber, 1763, multiaddressresult, 1, DateTime.Now);
                                    //Count++;
                                }
                            }
                            if (multiaddressrow.Count < 2)
                            {
                                driver.FindElement(By.XPath("//*[@id='ctl00_BodyContentPlaceHolder_PropertySearchUpdatePanel']/div[3]/table/tbody/tr[1]/td[5]/table/tbody/tr/td/table/tbody/tr[1]/th[1]/a")).Click();
                                Thread.Sleep(2000);
                            }

                            if (multiaddressrow.Count > 2)
                            {
                                HttpContext.Current.Session["multiParcel_BeaufortSC"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (multiaddressrow.Count > 26)
                            {
                                HttpContext.Current.Session["multiParcel_BeaufortSC_Maximum"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }
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
                    //Property Details
                    string ParcelID = "", AlternateID = "", OwnerName = "", Propertyaddress = "", Mailingaddress = "", Legaldescription = "", Propertyclass = "", Yearbuilt = "", Acreage = "";

                    IWebElement IProDescription = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[1]/tbody"));
                    IList<IWebElement> IProDescRow = IProDescription.FindElements(By.TagName("tr"));
                    IList<IWebElement> IproDecTD;
                    foreach (IWebElement description in IProDescRow)
                    {
                        IproDecTD = description.FindElements(By.TagName("td"));
                        if (IproDecTD.Count >= 4 && IproDecTD.Count != 0 && !description.Text.Contains("Property ID (PIN)") && !description.Text.Contains("Parcel ID"))
                        {
                            ParcelID = IproDecTD[0].Text;
                            AlternateID = IproDecTD[1].Text;
                            Propertyaddress = IproDecTD[2].Text;
                        }
                    }



                    IWebElement IProDescription1 = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[2]/tbody/tr/td/table[1]/tbody/tr[2]/td[1]/table/tbody"));
                    IList<IWebElement> IProDescRow1 = IProDescription1.FindElements(By.TagName("tr"));
                    IList<IWebElement> IproDecTD1;
                    foreach (IWebElement description1 in IProDescRow1)
                    {
                        IproDecTD1 = description1.FindElements(By.TagName("td"));
                        if (IproDecTD1.Count != 0 && IproDecTD1.Count == 2 && description1.Text.Contains("Owner") && !description1.Text.Contains("Address"))
                        {
                            OwnerName = IproDecTD1[1].Text;
                        }
                        if (IproDecTD1.Count != 0 && IproDecTD1.Count == 2 && description1.Text.Contains("Address"))
                        {
                            Mailingaddress = IproDecTD1[1].Text;
                        }
                    }

                    IWebElement IProDescription2 = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[2]/tbody/tr/td/table[1]/tbody/tr[2]/td[2]/table/tbody"));
                    IList<IWebElement> IProDescRow2 = IProDescription2.FindElements(By.TagName("tr"));
                    IList<IWebElement> IproDecTD2;
                    foreach (IWebElement description2 in IProDescRow2)
                    {
                        IproDecTD2 = description2.FindElements(By.TagName("td"));
                        if (IproDecTD2.Count != 0 && description2.Text.Contains("Property Class Code"))
                        {
                            Propertyclass = IproDecTD2[1].Text;
                        }
                        if (IproDecTD2.Count != 0 && description2.Text.Contains("Acreage"))
                        {
                            Acreage = IproDecTD2[1].Text;
                        }
                    }
                    gc.CreatePdf(orderNumber, ParcelID, "Property Details Pdf", driver, "SC", "Beaufort");
                    Legaldescription = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[2]/tbody/tr/td/table[1]/tbody/tr[3]/td/table/tbody/tr/td[2]/font")).Text;

                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[6]/td/a")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, ParcelID, "Year Built Details Pdf", driver, "SC", "Beaufort");
                    }
                    catch { }
                    try
                    {
                        Yearbuilt = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[2]/tbody/tr/td/table/tbody/tr[3]/td[8]")).Text;
                    }
                    catch { }

                    string Propertydetails = AlternateID + "~" + OwnerName + "~" + Propertyaddress + "~" + Mailingaddress + "~" + Legaldescription + "~" + Propertyclass + "~" + Yearbuilt + "~" + Taxauthority;
                    gc.insert_date(orderNumber, ParcelID, 1756, Propertydetails, 1, DateTime.Now);

                    //Assessment Details
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[3]/td/a")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, ParcelID, "Assessment Details Pdf", driver, "SC", "Beaufort");
                    }
                    catch { }
                    string value1 = "", value2 = "";
                    IWebElement IProAssessment = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[2]/tbody/tr/td/table[5]/tbody"));
                    IList<IWebElement> IProAssessmentRow = IProAssessment.FindElements(By.TagName("tr"));
                    IList<IWebElement> IproAssessmentTD;
                    foreach (IWebElement Assessment in IProAssessmentRow)
                    {
                        IproAssessmentTD = Assessment.FindElements(By.TagName("td"));
                        if (IproAssessmentTD.Count != 0 && IproAssessmentTD.Count == 4)
                        {
                            value1 = IproAssessmentTD[0].Text.Replace("Prior", "").Replace("Current", "").Trim() + "~" + IproAssessmentTD[1].Text.Trim() + "~" + IproAssessmentTD[3].Text.Trim();
                            gc.insert_date(orderNumber, ParcelID, 1758, value1, 1, DateTime.Now);
                        }
                    }
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    //Tax History Table Details
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/a")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, ParcelID, "Overview Details Pdf", driver, "SC", "Beaufort");
                    }
                    catch { }

                    IWebElement IProHistory = driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[2]/td[2]/table/tbody/tr/td/table[2]/tbody/tr/td/table[3]/tbody"));
                    IList<IWebElement> IProHistoryRow = IProHistory.FindElements(By.TagName("tr"));
                    IList<IWebElement> IproHistoryTD;
                    foreach (IWebElement History in IProHistoryRow)
                    {
                        IproHistoryTD = History.FindElements(By.TagName("td"));
                        if (IproHistoryTD.Count != 0 && IproHistoryTD.Count == 6 && !History.Text.Contains("Tax Year"))
                        {
                            string Taxhistorydetails = IproHistoryTD[0].Text.Trim() + "~" + IproHistoryTD[1].Text.Trim() + "~" + IproHistoryTD[2].Text.Trim() + "~" + IproHistoryTD[3].Text.Trim() + "~" + IproHistoryTD[4].Text.Trim() + "~" + IproHistoryTD[5].Text.Trim();
                            gc.insert_date(orderNumber, ParcelID, 1759, Taxhistorydetails, 1, DateTime.Now);
                        }
                    }
                    //Tax Information Details

                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[8]/td/a")).Click();
                        Thread.Sleep(20000);
                        gc.CreatePdf(orderNumber, ParcelID, "Tax Information Details Pdf", driver, "SC", "Beaufort");
                    }
                    catch { }
                    string Parcel = "", Alternateparcel = "", Propertycls = "", Currentowner = "", Ownerofrecord = "", Lender = "", Taxyer = "", Billnum = "", Taxbillid = "", Period = "";
                    string Duedate = "", Taxamt = "", Penaltyfee = "", Interest = "", Totaldue = "", Payinfltitle = "", Payinfll = "", Comments = "", Delinquentcomments = "";

                    Thread.Sleep(2000);
                    IWebElement IProTaxinfo = driver.FindElement(By.XPath("//*[@id='form1']/table/tbody/tr[1]/td/table[2]/tbody"));
                    IList<IWebElement> IProTaxinfoRow = IProTaxinfo.FindElements(By.TagName("tr"));
                    IList<IWebElement> IproTaxinfoTD;
                    foreach (IWebElement Taxinfo in IProTaxinfoRow)
                    {
                        IproTaxinfoTD = Taxinfo.FindElements(By.TagName("td"));
                        if (IproTaxinfoTD.Count != 0 && IproTaxinfoTD.Count == 3 && !Taxinfo.Text.Contains("PIN") && Taxinfo.Text.Trim() !="")
                        {
                            Parcel = IproTaxinfoTD[0].Text.Trim();
                            Alternateparcel = IproTaxinfoTD[1].Text.Trim();
                            Propertycls = IproTaxinfoTD[2].Text.Trim();
                        }
                    }
                    IWebElement IProTaxinfo1 = driver.FindElement(By.XPath("//*[@id='form1']/table/tbody/tr[1]/td/table[3]/tbody/tr/td/table[1]/tbody"));
                    IList<IWebElement> IProTaxinfoRow1 = IProTaxinfo1.FindElements(By.TagName("tr"));
                    IList<IWebElement> IproTaxinfoTD1;
                    foreach (IWebElement Taxinfo1 in IProTaxinfoRow1)
                    {
                        IproTaxinfoTD1 = Taxinfo1.FindElements(By.TagName("td"));
                        if (IproTaxinfoTD1.Count != 0 && IproTaxinfoTD1.Count == 3 && !Taxinfo1.Text.Contains("Current Owner") && Taxinfo1.Text.Trim() !="")
                        {
                            Currentowner = IproTaxinfoTD1[0].Text.Trim();
                            Ownerofrecord = IproTaxinfoTD1[1].Text.Trim();
                            Lender = IproTaxinfoTD1[2].Text.Trim();
                        }
                    }
                    try
                    {
                        Delinquentcomments = driver.FindElement(By.XPath("//*[@id='tabsummary']/table/tbody/tr[2]/td/table/tbody/tr[2]/td/font")).Text;
                        if (Delinquentcomments.Contains("Year"))
                        {
                            Delinquentcomments = "";

                        }
                    }
                    catch { }
                    try
                    {
                        IWebElement IProTaxinfo2 = driver.FindElement(By.XPath("//*[@id='tabsummary']/table/tbody/tr[1]/td/table/tbody"));
                        IList<IWebElement> IProTaxinfoRow2 = IProTaxinfo2.FindElements(By.TagName("tr"));
                        IList<IWebElement> IproTaxinfoTD2;
                        foreach (IWebElement Taxinfo2 in IProTaxinfoRow2)
                        {
                            IproTaxinfoTD2 = Taxinfo2.FindElements(By.TagName("td"));
                            if (IproTaxinfoTD2.Count != 0 && IproTaxinfoTD2.Count == 3 && Taxinfo2.Text.Contains("Tax Year") && Taxinfo2.Text.Trim() !="")
                            {
                                Taxyer = IproTaxinfoTD2[0].Text.Replace("Tax Year:", "").Trim();
                                Billnum = IproTaxinfoTD2[1].Text.Replace("Bill Number:", "").Trim();
                                Taxbillid = IproTaxinfoTD2[2].Text.Replace("TaxBillID:", "").Trim();
                            }
                            if (IproTaxinfoTD2.Count != 0 && IproTaxinfoTD2.Count == 1 && Taxinfo2.Text.Contains("Paid") && Taxinfo2.Text.Trim() != "")
                            {
                                Comments = IproTaxinfoTD2[0].Text.Trim();
                            }
                            if (IproTaxinfoTD2.Count != 0 && IproTaxinfoTD2.Count == 2 && Taxinfo2.Text.Trim() != "")
                            {
                                Payinfltitle = IproTaxinfoTD2[0].Text.Trim();
                                Payinfll = IproTaxinfoTD2[1].Text.Trim();
                            }
                            if (IproTaxinfoTD2.Count != 0 && IproTaxinfoTD2.Count == 6 && !Taxinfo2.Text.Contains("Period") && Taxinfo2.Text.Trim() != "")
                            {
                                Period = IproTaxinfoTD2[0].Text.Trim();
                                Duedate = IproTaxinfoTD2[1].Text.Trim();
                                Taxamt = IproTaxinfoTD2[2].Text.Trim();
                                Penaltyfee = IproTaxinfoTD2[3].Text.Trim();
                                Interest = IproTaxinfoTD2[4].Text.Trim();
                                Totaldue = IproTaxinfoTD2[5].Text.Trim();
                            }
                        }
                        string Taxinfordetails = Alternateparcel.Trim() + "~" + Propertycls.Trim() + "~" + Currentowner.Trim() + "~" + Ownerofrecord.Trim() + "~" + Lender.Trim() + "~" + Taxyer.Trim() + "~" + Billnum + "~" + Taxbillid + "~" + Period + "~" + Duedate + "~" + Taxamt + "~" + Penaltyfee + "~" + Interest + "~" + Totaldue + "~" + Comments + "~" + Delinquentcomments;
                        gc.insert_date(orderNumber, Parcel, 1760, Taxinfordetails, 1, DateTime.Now);
                    }
                    catch { }
                    if (Delinquentcomments != "NO DELINQUENT TAXES")
                    {
                        string Taxinfordetails1 = "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + Payinfltitle + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + Totaldue + "~" + Comments + "~" + Delinquentcomments;
                        gc.insert_date(orderNumber, Parcel, 1760, Taxinfordetails1, 1, DateTime.Now);
                    }
                    //Delinquent Details

                    string Totaldelinquent = "", Totaldelinquenttitle = "";
                    try
                    {
                        IWebElement IProTaxinfo3 = driver.FindElement(By.XPath("//*[@id='tabsummary']/table/tbody/tr[2]/td/table/tbody"));
                        IList<IWebElement> IProTaxinfoRow3 = IProTaxinfo3.FindElements(By.TagName("tr"));
                        IList<IWebElement> IproTaxinfoTD3;
                        foreach (IWebElement Taxinfo3 in IProTaxinfoRow3)
                        {
                            IproTaxinfoTD3 = Taxinfo3.FindElements(By.TagName("td"));
                            if (IproTaxinfoTD3.Count != 0 && IproTaxinfoTD3.Count == 6 && !Taxinfo3.Text.Contains("Year") && Taxinfo3.Text.Trim() != "")
                            {
                                Period = IproTaxinfoTD3[0].Text.Trim();
                                Duedate = IproTaxinfoTD3[1].Text.Trim();
                                Taxamt = IproTaxinfoTD3[2].Text.Trim();
                                Penaltyfee = IproTaxinfoTD3[3].Text.Trim();
                                Interest = IproTaxinfoTD3[4].Text.Trim();
                                Totaldue = IproTaxinfoTD3[5].Text.Trim();
                                string Taxdelinquentdetails1 = Period + "~" + Duedate + "~" + Taxamt + "~" + Penaltyfee + "~" + Interest + "~" + Totaldue;
                                gc.insert_date(orderNumber, Parcel, 1761, Taxdelinquentdetails1, 1, DateTime.Now);
                            }
                            if (IproTaxinfoTD3.Count != 0 && IproTaxinfoTD3.Count == 2 && Taxinfo3.Text.Trim() != "")
                            {
                                Totaldelinquenttitle = IproTaxinfoTD3[0].Text.Trim();
                                Totaldelinquent = IproTaxinfoTD3[1].Text.Trim();
                                string Taxdelinquentdetails2 = "" + "~" + "" + "~" + "" + "~" + "" + "~" + Totaldelinquenttitle + "~" + Totaldelinquent;
                                gc.insert_date(orderNumber, Parcel, 1761, Taxdelinquentdetails2, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }
                    //Tax Distribution Details
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='form1']/table/tbody/tr[3]/td/a[1]")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, ParcelID, "Tax Distribution Details Pdf", driver, "SC", "Beaufort");
                    }
                    catch { }
                    try
                    {
                        string Taxingauthority = "", Grosstax = "", Credits = "", Savings = "", Nettax = "";
                        IWebElement IProDistribution = driver.FindElement(By.XPath("//*[@id='tab_assmt_data_" + Taxbillid + "']/table/tbody"));
                        IList<IWebElement> IProDistributionRow1 = IProDistribution.FindElements(By.TagName("tr"));
                        IList<IWebElement> IproDistributionTD1;
                        foreach (IWebElement Distribution in IProDistributionRow1)
                        {
                            IproDistributionTD1 = Distribution.FindElements(By.TagName("td"));
                            if (IproDistributionTD1.Count != 0 && IproDistributionTD1.Count == 5 && !Distribution.Text.Contains("Authority") && Distribution.Text.Trim() != "")
                            {
                                Taxingauthority = IproDistributionTD1[0].Text.Trim();
                                Grosstax = IproDistributionTD1[1].Text.Trim();
                                Credits = IproDistributionTD1[2].Text.Trim();
                                Savings = IproDistributionTD1[3].Text.Trim();
                                Nettax = IproDistributionTD1[4].Text.Trim();
                                string TaxDistribution = Taxingauthority + "~" + Grosstax + "~" + Credits + "~" + Savings + "~" + Nettax;
                                gc.insert_date(orderNumber, Parcel, 1762, TaxDistribution, 1, DateTime.Now);

                            }
                            if (IproDistributionTD1.Count != 0 && IproDistributionTD1.Count == 1 && !Distribution.Text.Contains("Authority") && !Distribution.Text.Contains("Assessment Information") && Distribution.Text.Trim() != "")
                            {
                                Taxingauthority = IproDistributionTD1[0].Text.Trim();
                                string TaxDistribution = Taxingauthority + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                gc.insert_date(orderNumber, Parcel, 1762, TaxDistribution, 1, DateTime.Now);
                            }
                            if (IproDistributionTD1.Count != 0 && IproDistributionTD1.Count == 2 && Distribution.Text.Contains("Total Net Tax:") && Distribution.Text.Trim() != "")
                            {
                                string Totalnettitle = IproDistributionTD1[0].Text.Trim();
                                string Totaltax = IproDistributionTD1[1].Text.Trim();
                                string TaxDistribution1 = "" + "~" + "" + "~" + "" + "~" + Totalnettitle + "~" + Totaltax;
                                gc.insert_date(orderNumber, Parcel, 1762, TaxDistribution1, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }
                    //Payment History Details
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='form1']/table/tbody/tr[3]/td/a[2]")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, ParcelID, "Tax Payment History Details Pdf", driver, "SC", "Beaufort");
                    }
                    catch { }
                    ////*[@id="tab_pmt_data"]/table/tbody
                    string Lastpaid = "", Amountpaid = "", Receiptnumber = "";
                    IWebElement IProPayment = driver.FindElement(By.XPath("//*[@id='tab_pmt_data']/table/tbody"));
                    IList<IWebElement> IProPaymentRow1 = IProPayment.FindElements(By.TagName("tr"));
                    IList<IWebElement> IproPaymentTD1;
                    foreach (IWebElement Payment in IProPaymentRow1)
                    {
                        IproPaymentTD1 = Payment.FindElements(By.TagName("td"));
                        if (!Payment.Text.Contains("Last Paid") && IproPaymentTD1.Count != 0 && IproPaymentTD1.Count == 3)
                        {
                            Lastpaid = IproPaymentTD1[0].Text.Trim();
                            Amountpaid = IproPaymentTD1[1].Text.Trim();
                            Receiptnumber = IproPaymentTD1[2].Text.Trim();

                            string PaymentHistory = Lastpaid + "~" + Amountpaid + "~" + Receiptnumber;
                            gc.insert_date(orderNumber, Parcel, 1764, PaymentHistory, 1, DateTime.Now);
                        }
                    }
                    //Land 
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[4]/td/a")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, ParcelID, "Land Details Pdf", driver, "SC", "Beaufort");
                    }
                    catch { }
                    //Value History
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='main']/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[8]/td/a")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, ParcelID, "Value History Details Pdf", driver, "SC", "Beaufort");
                    }
                    catch { }

                    //Tax Bill Download
                    driver.Navigate().GoToUrl("https://www.beaufortcountytreasurer.com/tax-bill-lookup-rpp-2018");
                    //driver.FindElement(By.XPath("//*[@id='child']/div/div/div[1]/div[1]/div/a")).Click();
                    //Thread.Sleep(2000);
                    // driver.FindElement(By.XPath("//*[@id='myModal']/div/div/a")).Click();
                    Thread.Sleep(5000);
                    gc.CreatePdf(orderNumber, ParcelID, "Tax Bill Details Pdf", driver, "SC", "Beaufort");
                    List<string> billinfo = new List<string>();
                    IWebElement Billsinfo2 = driver.FindElement(By.XPath("//*[@id='child']/div[1]/div/div/div"));
                    IList<IWebElement> TRBillsinfo2 = Billsinfo2.FindElements(By.TagName("div"));
                    IList<IWebElement> Aherftax;
                    int i = 0;
                    foreach (IWebElement row in TRBillsinfo2)
                    {
                        Aherftax = row.FindElements(By.TagName("a"));

                        if (Aherftax.Count == 3)
                        {
                            string addview = Aherftax[0].GetAttribute("href");
                            string addview1 = Aherftax[1].GetAttribute("href");
                            billinfo.Add(addview);
                            billinfo.Add(addview1);
                        }
                    }
                    foreach (string assessmentclick in billinfo)
                    {
                        int l = 0;
                        string fileName1 = "";
                        //Pdf download
                        try
                        {
                            var chromeOptions = new ChromeOptions();
                            var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];
                            chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                            chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                            chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                            chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                            var chDriver = new ChromeDriver(chromeOptions);
                            Array.ForEach(Directory.GetFiles(@downloadDirectory), File.Delete);
                            chDriver.Navigate().GoToUrl(assessmentclick);
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, Parcel, "Tax Bill1", chDriver, "SC", "Beaufort");
                            chDriver.SwitchTo().Window(chDriver.WindowHandles.Last());
                            Thread.Sleep(5000);
                            IWebElement element1 = chDriver.FindElement(By.XPath("//*[@id='CAT_Custom_1']"));
                            element1.GetAttribute("href");
                            element1.Clear();
                            element1.SendKeys(Parcel);
                            chDriver.FindElement(By.XPath("//*[@id='child']/div[1]/div/div/div/form/table/tbody/tr[2]/td/input")).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, Parcel, "Tax Bill1 Result", chDriver, "SC", "Beaufort");

                            try
                            {
                                IWebElement iclilck = chDriver.FindElement(By.XPath("//*[@id='child']"));
                                IList<IWebElement> rowilsx = iclilck.FindElements(By.TagName("table"));
                                foreach (IWebElement row in rowilsx)
                                {
                                    IList<IWebElement> td;
                                    td = row.FindElements(By.TagName("a"));
                                    if (row.Text.Contains("DOWNLOAD TAX BILL"))
                                    {
                                        td[0].Click();
                                        Thread.Sleep(5000);
                                        gc.CreatePdf(orderNumber, Parcel, "Download bill1", chDriver, "SC", "Beaufort");
                                        l++;
                                    }
                                }
                            }
                            catch { }

                            Thread.Sleep(5000);
                            fileName1 = latestfilename();
                            Thread.Sleep(2000);
                            gc.AutoDownloadFileSpokane(orderNumber, ParcelID, "Beaufort", "SC", fileName1);
                            chDriver.Quit();
                        }
                        catch { }
                    }
                    int bill = 0;
                    foreach (string assessmentclick1 in billinfo)
                    {

                        driver.Navigate().GoToUrl(assessmentclick1);
                        Thread.Sleep(2000);
                        driver.SwitchTo().Window(driver.WindowHandles.Last());
                       // Thread.Sleep(2000);
                        IWebElement element1 = driver.FindElement(By.XPath("//*[@id='CAT_Custom_1']"));
                        element1.GetAttribute("href");
                        element1.Clear();
                        element1.SendKeys(Parcel);
                        driver.FindElement(By.XPath("//*[@id='child']/div[1]/div/div/div/form/table/tbody/tr[2]/td/input")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, Parcel, "Tax Bill Inputshowpage Result1"+bill, driver, "SC", "Beaufort");
                        bill++;
                    }


                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "SC", "Beaufort", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    GlobalClass.titleparcel = "";
                    gc.mergpdf(orderNumber, "SC", "Beaufort");
                    return "Data Inserted Successfully";
                }
                catch (Exception ex)
                {
                    driver.Quit();
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