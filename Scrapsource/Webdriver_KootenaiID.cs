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
    public class Webdriver_KootenaiID
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();
        string Parcel_number, Ain, PropertyAddress, heading = "", Assisementresult1 = "", Assisementresult2 = "", taxauthorityResult, YearBuilt;
        public string FTP_KootenaiID(string streetno, string direction, string streetname, string city, string streettype, string unitnumber, string ownernm, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string address = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new ChromeDriver();
            //driver = new PhantomJSDriver();
            string Propertyresult1;
            using (driver = new PhantomJSDriver())
            { 
                try
                {
                    if (searchType == "titleflex")
                    {
                        address = streetno + " " + direction + " " + streetname;
                        gc.TitleFlexSearch(orderNumber, "", "", address.Trim(), "ID", "Kootenai");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_Kootenai"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    driver.Navigate().GoToUrl("http://id-kootenai-assessor.governmax.com/propertymax/agency/id-kootenai-assessor/ID-Kootenai_Homepage2017.asp?sid=D6D1FB69FDAD40A7A8E723B0B36DB4D7");
                    IWebElement Firstclick = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table[1]/tbody/tr[1]/td[2]/table/tbody"));
                    IWebElement Firstaherf = Firstclick.FindElement(By.TagName("a"));
                    string First1 = Firstclick.GetAttribute("href");
                    Firstaherf.Click();

                    if (searchType == "address")
                    {
                       if(direction=="")
                        {
                            address = streetno.Trim() + " " + streetname.Trim() + " " + streettype.Trim();
                        }
                        else
                        {
                            address = streetno.Trim() + " " + direction.Trim() + " " + streetname.Trim() + " "+ streettype.Trim();
                        }
                        driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[1]/td/table/tbody/tr/td/table/tbody/tr[2]/td/font/input")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "AddressSearch", driver, "ID", "Kootenai");
                        Thread.Sleep(3000);
                        driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[3]/td/input")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "AddressSearch After", driver, "ID", "Kootenai");
                        try
                        {
                            string Multiplowner = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[1]/td[2]/table/tbody/tr/td/table/tbody/tr/td[2]/table/tbody")).Text.Replace("\r\n", "");
                            Thread.Sleep(2000);
                    
                                IWebElement Multipleownertable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[1]/tbody/tr/td/table/tbody"));
                                IList<IWebElement> Multiplerow = Multipleownertable.FindElements(By.TagName("tr"));
                                IList<IWebElement> Multipleid;
                                if (Multiplerow.Count < 27)
                                {
                                    foreach (IWebElement Multipleownername in Multiplerow)
                                    {

                                        Multipleid = Multipleownername.FindElements(By.TagName("td"));
                                        if (Multipleid.Count != 0 && !Multipleownername.Text.Contains("Parcel ID"))
                                        {

                                            string Parcel = Multipleid[0].Text;
                                            string detail = Multipleid[1].Text + "~" + Multipleid[2].Text;
                                            gc.insert_date(orderNumber, Parcel, 750, detail, 1, DateTime.Now);
                                        }
                                    }
                                    gc.CreatePdf_WOP(orderNumber, "Multiple Parcel", driver, "ID", "Kootenai");
                                    HttpContext.Current.Session["multiParcel_Kootenai"] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                                if (Multiplerow.Count > 27)
                                {
                                    HttpContext.Current.Session["multiParcel_Kootenai_Multicount"] = "Maximum";
                                    gc.CreatePdf_WOP(orderNumber, "MultyAddressSearch", driver, "ID", "Kootenai");

                                    driver.Quit();
                                    return "Maximum";
                                }
                        }
                        catch { }
                        try
                        {
                            string Nodata = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[1]/tbody/tr/td/table/tbody/tr[2]/td")).Text;
                            if (Nodata.Contains("No Records Found"))
                            {
                                gc.CreatePdf_WOP(orderNumber, "NO Record", driver, "ID", "Kootenai");
                                HttpContext.Current.Session["Nodata_Kootenai"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {
                        if (parcelNumber != "")
                        {
                            driver.FindElement(By.LinkText("   Parcel Number")).SendKeys(Keys.Enter);
                            Thread.Sleep(2000);
                            driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[1]/td/table/tbody/tr/td/table/tbody/tr[2]/td/font/input")).SendKeys(parcelNumber);
                            gc.CreatePdf(orderNumber, parcelNumber, "Property Search Result", driver, "ID", "Kootenai");
                            driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[2]/td/input")).SendKeys(Keys.Enter);
                            Thread.Sleep(2000);
                        }
                        if (parcelNumber == "")
                        {
                            try
                            {
                                string Nodata = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[1]/tbody/tr/td/table/tbody/tr[2]/td")).Text;
                                if (Nodata.Contains("No Records Found"))
                                {
                                    HttpContext.Current.Session["Nodata_Kootenai"] = "Yes";
                                    driver.Quit();
                                    return "No Records Found";
                                }
                            }
                            catch { }
                        }
                    }
                    if (searchType == "unitnumber")
                    {
                        driver.FindElement(By.LinkText("   AIN")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr/td/table/tbody/tr[2]/td/font/input")).SendKeys(unitnumber);
                        gc.CreatePdf_WOP(orderNumber, "unitnumber", driver, "ID", "Kootenai");
                        driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[3]/td/input")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                    }
                    if (searchType == "owner")
                    {
                        driver.FindElement(By.LinkText("   Owner")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr/td/table/tbody/tr[2]/td/font/input")).SendKeys(ownernm);
                        driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[3]/td/input")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        try
                        {
                            string Multiplowner = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[1]/td[2]/table/tbody/tr/td/table/tbody/tr/td[2]/table/tbody")).Text.Replace("\r\n", "");
                            // string Multicoutpage=
                            Thread.Sleep(2000);
                            if (Multiplowner.Contains("page1 of 1"))

                            {
                                IWebElement Multipleownertable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[1]/tbody/tr/td/table/tbody"));
                                IList<IWebElement> Multiplerow = Multipleownertable.FindElements(By.TagName("tr"));
                                IList<IWebElement> Multipleid;
                                if (Multiplerow.Count < 27)
                                {
                                    foreach (IWebElement Multipleownername in Multiplerow)
                                    {

                                        Multipleid = Multipleownername.FindElements(By.TagName("td"));
                                        if (Multipleid.Count != 0 && !Multipleownername.Text.Contains("Parcel ID"))
                                        {

                                            string Parcel = Multipleid[0].Text;
                                            string detail = Multipleid[1].Text + "~" + Multipleid[2].Text;
                                            gc.insert_date(orderNumber, Parcel, 750, detail, 1, DateTime.Now);
                                        }
                                    }
                                    gc.CreatePdf_WOP(orderNumber, "Multiple Parcel", driver, "ID", "Kootenai");
                                    HttpContext.Current.Session["multiParcel_Kootenai"] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                                if (Multiplerow.Count > 27)
                                {
                                    HttpContext.Current.Session["multiParcel_Kootenai_Multicount"] = "Maximum";
                                    gc.CreatePdf_WOP(orderNumber, "MultyAddressSearch", driver, "ID", "Kootenai");

                                    driver.Quit();
                                    return "Maximum";
                                }
                            }
                            else
                            {
                                HttpContext.Current.Session["multiParcel_Kootenai_Multicount"] = "Maximum";
                                gc.CreatePdf_WOP(orderNumber, "MultyAddressSearch", driver, "ID", "Kootenai");
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }
                        try
                        {
                            string Nodata = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[1]/tbody/tr/td/table/tbody/tr[2]/td")).Text;
                            if (Nodata.Contains("No Records Found"))
                            {
                                HttpContext.Current.Session["Nodata_Kootenai"] = "Yes";
                                driver.Quit();
                                return "No Records Found";
                            }
                        }
                        catch { }
                    }

                    IWebElement Propertytable1 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[1]/tbody"));
                    IList<IWebElement> Propertytable1row = Propertytable1.FindElements(By.TagName("tr"));
                    IList<IWebElement> Propertytableid;
                    foreach (IWebElement Property1 in Propertytable1row)
                    {
                        Propertytableid = Property1.FindElements(By.TagName("td"));
                        if (Propertytableid.Count != 0 && !Property1.Text.Contains("Parcel Number "))
                        {
                            Parcel_number = Propertytableid[0].Text;
                            Ain = Propertytableid[1].Text;
                            PropertyAddress = Propertytableid[2].Text;

                            break;
                        }
                    }


                    //Property
                    IWebElement Propertytable2 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[2]/tbody"));
                    string OwnerName = gc.Between(Propertytable2.Text, "Owner Name ", "Owner Address ").Trim();
                    string MailingAddress = gc.Between(Propertytable2.Text, "Owner Address ", "Transfer Date ");
                    string TaxAuthorityGroup = gc.Between(Propertytable2.Text, "Tax Authority Group ", "Situs Address ").Trim();
                    string Acreage = gc.Between(Propertytable2.Text, "Acreage ", "Current Legal Desc. ").Trim();
                    string LegalDescription = gc.Between(Propertytable2.Text, "Current Legal Desc. ", "Parcel Type ").Trim();
                    string PropertyClass = gc.Between(Propertytable2.Text, "Property Class Code ", "Neighborhood Code ").Trim();
                    string Neighborhood = gc.Between(Propertytable2.Text, "Neighborhood Code ", "Assessment Information ").Trim();
                    driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[6]/td/font/a")).SendKeys(Keys.Enter);
                    Thread.Sleep(2000);
                    try
                    {
                        YearBuilt = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[2]/tbody/tr/td/table/tbody/tr[3]/td[8]/font")).Text;
                    }
                    catch { }
                    Propertyresult1 = Ain + "~" + PropertyAddress + "~" + OwnerName + "~" + MailingAddress + "~" + TaxAuthorityGroup + "~" + Acreage + "~" + LegalDescription + "~" + PropertyClass + "~" + Neighborhood + "~" + YearBuilt;
                    gc.insert_date(orderNumber, Parcel_number, 720, Propertyresult1, 1, DateTime.Now);
                    gc.CreatePdf(orderNumber, Parcel_number, "Property Search Result", driver, "ID", "Kootenai");
                    driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[3]/td/font/a")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Assessment", driver, "ID", "Kootenai");
                    try
                    {
                        IWebElement Assisementdetail = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[2]/tbody/tr/td/table[5]/tbody"));
                        IList<IWebElement> Assisementdetailrow = Assisementdetail.FindElements(By.TagName("tr"));
                        IList<IWebElement> Assisementdetailid;
                        foreach (IWebElement Assisement in Assisementdetailrow)
                        {
                            Assisementdetailid = Assisement.FindElements(By.TagName("td"));
                            if (Assisementdetailid.Count != 0 && !Assisement.Text.Contains("Assessment Information ") && Assisement.Text.Trim() != "")
                            {
                                if (!heading.Contains(Assisementdetailid[2].Text.Trim()))
                                {
                                    if (!Assisement.Text.Contains("Current Year "))
                                    {
                                        heading += Assisementdetailid[2].Text.Trim() + "~";
                                    }
                                    Assisementresult1 += Assisementdetailid[3].Text.Trim() + "~";
                                    Assisementresult2 += Assisementdetailid[5].Text.Trim() + "~";
                                }
                            }
                        }
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + "Year" + "~" + heading.Remove(heading.Length - 1, 1) + "' where Id = '" + 730 + "'");
                        gc.insert_date(orderNumber, Parcel_number, 730, Assisementresult1.Remove(Assisementresult1.Length - 1), 1, DateTime.Now);
                        gc.insert_date(orderNumber, Parcel_number, 730, Assisementresult2.Remove(Assisementresult2.Length - 1), 1, DateTime.Now);
                    }
                    catch { }
                    //Assessnment History
                    driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[7]/td/font/a")).SendKeys(Keys.Enter);
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Assessment History", driver, "ID", "Kootenai");

                    IWebElement AssessmentHistrytable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[2]/tbody/tr/td/table[2]/tbody"));
                    IList<IWebElement> Assessmenthistryrow = AssessmentHistrytable.FindElements(By.TagName("tr"));
                    IList<IWebElement> Assessmenthistryid;
                    foreach (IWebElement Assmenthistory in Assessmenthistryrow)
                    {
                        Assessmenthistryid = Assmenthistory.FindElements(By.TagName("td"));
                        if (Assessmenthistryid.Count != 0 && !Assmenthistory.Text.Contains("Assessment Date"))
                        {
                            string Assessmenthistoryresult = Assessmenthistryid[0].Text + "~" + Assessmenthistryid[1].Text + "~" + Assessmenthistryid[2].Text + "~" + Assessmenthistryid[3].Text + "~" + Assessmenthistryid[4].Text + "~" + Assessmenthistryid[5].Text;
                            gc.insert_date(orderNumber, Parcel_number, 737, Assessmenthistoryresult, 1, DateTime.Now);
                        }
                    }

                    //tax information

                    try
                    {
                        //Tax Authority
                        driver.Navigate().GoToUrl("http://id-kootenai-treasurer.governmax.com/collectmax/agency/id-kootenai-treasurer/homepage2017.asp?sid=49B81F6AF42C4778B7357D97F7DE9C95");
                        driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr[1]/td/p[2]/a")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        driver.FindElement(By.LinkText("   Contact Us")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, Parcel_number, "Tax author", driver, "ID", "Kootenai");
                        IWebElement TaxAuthoritytable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[2]/tbody"));
                        IList<IWebElement> Taxauthorityrow = TaxAuthoritytable.FindElements(By.TagName("tr"));
                        IList<IWebElement> Taxauthorityid;
                        foreach (IWebElement taxauthority in Taxauthorityrow)
                        {
                            Taxauthorityid = taxauthority.FindElements(By.TagName("td"));
                            if (Taxauthorityid.Count != 0 && taxauthority.Text.Contains("Kootenai"))
                            {
                                taxauthorityResult = Taxauthorityid[0].Text.Trim().Replace("\r,\n", " ");
                                break;
                            }

                        }
                        //end
                    }
                    catch { }
                    driver.Navigate().GoToUrl("http://id-kootenai-treasurer.governmax.com/collectmax/agency/id-kootenai-treasurer/homepage2017.asp?sid=49B81F6AF42C4778B7357D97F7DE9C95");
                    gc.CreatePdf(orderNumber, Parcel_number, "Current TAx1", driver, "ID", "Kootenai");
                    driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr[1]/td/p[2]/a")).Click();
                    Thread.Sleep(4000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Current TAx2", driver, "ID", "Kootenai");
                    driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[1]/td/table/tbody/tr/td/table/tbody/tr[2]/td/font/input")).SendKeys(Parcel_number);
                    gc.CreatePdf(orderNumber, Parcel_number, "Current TAx3", driver, "ID", "Kootenai");
                    driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[3]/td/input")).SendKeys(Keys.Enter);
                    Thread.Sleep(4000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Current TAx4", driver, "ID", "Kootenai");
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[1]/tbody/tr[2]/td/table/tbody/tr[2]/td/font/b/a")).Click();
                        Thread.Sleep(7000);
                    }
                    catch { }
                    string TaxRoll = driver.FindElement(By.XPath("//*[@id='form1']/table/tbody/tr[1]/td/table[2]/tbody/tr[2]/td[3]/font")).Text;
                    gc.insert_date(orderNumber, Parcel_number, 741, TaxRoll + "~" + taxauthorityResult, 1, DateTime.Now);
                    //Current Tax table
                    gc.CreatePdf(orderNumber, Parcel_number, "Current TAx", driver, "ID", "Kootenai");
                    IWebElement currenttaxtable = driver.FindElement(By.XPath("//*[@id='tabsummary']/table/tbody/tr[1]/td/table/tbody"));
                    string TaxYear = gc.Between(currenttaxtable.Text, "TaxYear: ", "Bill Number").Trim();
                    string BillNumber = gc.Between(currenttaxtable.Text, "Bill Number:", "Tax Bill ID:").Trim();
                    string TaxBillID = gc.Between(currenttaxtable.Text, "Tax Bill ID:", "Installment ").Trim();
                    IList<IWebElement> currenttaxrow = currenttaxtable.FindElements(By.TagName("tr"));
                    IList<IWebElement> currenttaxid;
                    foreach (IWebElement currenttax in currenttaxrow)
                    {
                        currenttaxid = currenttax.FindElements(By.TagName("td"));
                        if (currenttax.Text.Contains("Prior Year Taxes Due "))
                        {
                            break;
                        }
                        if (currenttaxid.Count > 2 && !currenttax.Text.Contains("TaxYear") && !currenttax.Text.Contains("Period"))
                        {
                            string currentaxresult1 = TaxYear + "~" + BillNumber + "~" + TaxBillID + "~" + currenttaxid[0].Text + "~" + currenttaxid[1].Text + "~" + currenttaxid[2].Text + "~" + currenttaxid[3].Text + "~" + currenttaxid[4].Text + "~" + currenttaxid[5].Text;
                            gc.insert_date(orderNumber, Parcel_number, 743, currentaxresult1, 1, DateTime.Now);
                        }
                        if (currenttaxid.Count == 1 && !currenttax.Text.Contains("Installment"))
                        {
                            string currenttaxresult2 = TaxYear + "~" + BillNumber + "~" + TaxBillID + "~" + "~" + "~" + currenttaxid[0].Text + "~" + "~";
                            gc.insert_date(orderNumber, Parcel_number, 743, currenttaxresult2, 1, DateTime.Now);
                        }
                        if (currenttaxid.Count == 2 && !currenttax.Text.Contains("Installment"))
                        {
                            string currenttaxresult2 = TaxYear + "~" + BillNumber + "~" + TaxBillID + "~" + currenttaxid[0].Text + "~" + "~" + "~" + "~" + "~" + currenttaxid[1].Text;
                            gc.insert_date(orderNumber, Parcel_number, 743, currenttaxresult2, 1, DateTime.Now);
                        }
                    }
                    //delinquenttax
                    IWebElement delinquenttaxtable = driver.FindElement(By.XPath("//*[@id='tabsummary']/table/tbody/tr[2]/td/table/tbody"));
                    IList<IWebElement> deliquenttaxrow = delinquenttaxtable.FindElements(By.TagName("tr"));
                    IList<IWebElement> deliquenttaxid;
                    foreach (IWebElement deliquent in deliquenttaxrow)
                    {
                        deliquenttaxid = deliquent.FindElements(By.TagName("td"));
                        if (deliquenttaxid.Count > 2 && !deliquent.Text.Contains("Prior Year Taxes Due ") && !deliquent.Text.Contains("Year"))
                        {
                            string deliquentresult = deliquenttaxid[0].Text + "~" + deliquenttaxid[1].Text + "~" + deliquenttaxid[2].Text + "~" + deliquenttaxid[3].Text + "~" + deliquenttaxid[4].Text + "~" + deliquenttaxid[5].Text;
                            gc.insert_date(orderNumber, Parcel_number, 744, deliquentresult, 1, DateTime.Now);
                        }
                        if (deliquenttaxid.Count == 1 && !deliquent.Text.Contains("Prior Year Taxes Due") && !deliquent.Text.Contains("Delinquent Years"))
                        {
                            string deliquentresult1 = "~" + "~" + "~" + deliquenttaxid[0].Text + "~";
                            gc.insert_date(orderNumber, Parcel_number, 744, deliquentresult1, 1, DateTime.Now);
                        }
                        if (deliquenttaxid.Count == 2 && !deliquent.Text.Contains("Prior Year Taxes Due"))
                        {
                            string deliquentresult1 = deliquenttaxid[0].Text + "~" + "~" + "~" + "~" + "~" + deliquenttaxid[1].Text;
                            gc.insert_date(orderNumber, Parcel_number, 744, deliquentresult1, 1, DateTime.Now);
                        }
                    }
                    //Assessment tax
                    driver.FindElement(By.LinkText("View Tax Assessment Values")).SendKeys(Keys.Enter);
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Assessment Values", driver, "ID", "Kootenai");
                    try
                    {
                        IWebElement Assessmenttableinfo = driver.FindElement(By.XPath("//*[@id='tab_assmt_data_" + TaxBillID + "']/table"));
                        IList<IWebElement> Assessmentrow = Assessmenttableinfo.FindElements(By.TagName("tr"));
                        IList<IWebElement> Assessmentid;
                        foreach (IWebElement Assessmentinfo in Assessmentrow)
                        {
                            Assessmentid = Assessmentinfo.FindElements(By.TagName("td"));
                            {
                                if (Assessmentid.Count > 2 && !Assessmentinfo.Text.Contains("Authority"))
                                {
                                    string Assessmentinforesult = Assessmentid[0].Text + "~" + Assessmentid[1].Text + "~" + Assessmentid[2].Text + "~" + Assessmentid[3].Text + "~" + Assessmentid[4].Text + "~" + Assessmentid[5].Text + "~" + Assessmentid[6].Text + "~" + Assessmentid[7].Text;
                                    gc.insert_date(orderNumber, Parcel_number, 746, Assessmentinforesult, 1, DateTime.Now);
                                }
                                if (Assessmentid.Count == 1 && !Assessmentinfo.Text.Contains("Assessment Information"))
                                {
                                    string Assessmentinforesult1 = Assessmentid[0].Text;
                                    gc.insert_date(orderNumber, Parcel_number, 746, Assessmentinforesult1, 1, DateTime.Now);
                                }
                                if (Assessmentid.Count == 2)
                                {
                                    string Assessmentinforesult = Assessmentid[0].Text + "~" + "~" + "~" + "~" + "~" + "~" + "~" + Assessmentid[1].Text;
                                    gc.insert_date(orderNumber, Parcel_number, 746, Assessmentinforesult, 1, DateTime.Now);
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                    string paymenthistoryresult = "";
                    //Paymenthistorytax
                    driver.FindElement(By.LinkText("View Payment History")).SendKeys(Keys.Enter);
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Payment Hiistory", driver, "ID", "Kootenai");
                    IWebElement Paymenthistorytable = driver.FindElement(By.XPath("//*[@id='tab_pmt_data']/table/tbody"));
                    IList<IWebElement> Paymenthistoryrow = Paymenthistorytable.FindElements(By.TagName("tr"));
                    IList<IWebElement> paymenthistoryid;
                    foreach (IWebElement Paymenthistory in Paymenthistoryrow)
                    {
                        paymenthistoryid = Paymenthistory.FindElements(By.TagName("td"));
                        if (paymenthistoryid.Count > 2 && !Paymenthistory.Text.Contains("Payment Information") && !Paymenthistory.Text.Contains("Last Paid"))
                        {
                            paymenthistoryresult = paymenthistoryid[0].Text + "~" + paymenthistoryid[1].Text + "~" + paymenthistoryid[2].Text + "~" + paymenthistoryid[3].Text;

                        }
                        if (paymenthistoryid.Count == 2)
                        {
                            paymenthistoryresult += "~" + paymenthistoryid[1].Text;
                            gc.insert_date(orderNumber, Parcel_number, 747, paymenthistoryresult, 1, DateTime.Now);
                        }
                    }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.mergpdf(orderNumber, "ID", "Kootenai");
                    driver.Quit();

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