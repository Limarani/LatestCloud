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
    public class Webdriver_JeffersonKY
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();
        IWebElement IAmg;
        public string FTP_JeffersonKY(string Address, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string Parcel_number = "", Tax_Authority = "", Year = "", Addresshrf = "", Propertyresult = "", PaidDate = "", parcelhref = "", MailingAddress = "", Yearbuilt = "";
            //request.UseDefaultCredentials = true;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new ChromeDriver();
            //driver = new PhantomJSDriver();
            //RDP Site
            var option = new ChromeOptions();
            option.AddArgument("No-Sandbox");
            using (driver = new ChromeDriver(option))
            {
                try
                {

                    if (searchType == "titleflex")
                    {

                        gc.TitleFlexSearch(orderNumber, "", ownername, Address, "KY", "Jefferson");
                        if (HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes")
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Zero_Jefferson"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    try
                    {
                        StartTime = DateTime.Now.ToString("HH:mm:ss");
                        driver.Navigate().GoToUrl("http://www.jcsoky.org/contacts.htm");
                        string Taxauthority = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/div[1]/table/tbody/tr/td/div/table/tbody/tr/td/div[2]/table/tbody/tr[1]/td[2]/div/table/tbody/tr/td[1]/div[3]/p")).Text.Replace("\r\n", " ");
                        Tax_Authority = gc.Between(Taxauthority, "Mailing Address:", "Click Here for a Map");
                    }
                    catch { }
                    driver.Navigate().GoToUrl("https://jeffersonpva.ky.gov/property-search/");
                    if (searchType == "address")
                    {
                        driver.FindElement(By.Id("psfldAddress")).SendKeys(Address);
                        driver.FindElement(By.XPath("//*[@id='searchFormAddress']/fieldset/p[2]/input")).Click();
                        gc.CreatePdf_WOP(orderNumber, "Searchbefore", driver, "KY", "Jefferson");
                        Thread.Sleep(2000);
                        try
                        {
                            int Max = 0;

                            IWebElement Multiparceladdress = driver.FindElement(By.XPath("//*[@id='content']/table/tbody"));
                            IList<IWebElement> Multiparcelrow = Multiparceladdress.FindElements(By.TagName("tr"));
                            IList<IWebElement> Multiparcelid;
                            gc.CreatePdf_WOP(orderNumber, "Multiparcel", driver, "KY", "Jefferson");
                            foreach (IWebElement multiparcel in Multiparcelrow)
                            {
                                Multiparcelid = multiparcel.FindElements(By.TagName("td"));
                                if (Multiparcelid.Count != 0)
                                {
                                    IWebElement Address1 = Multiparcelid[1].FindElement(By.TagName("a"));
                                    Addresshrf = Address1.GetAttribute("href");
                                    string Addressst = Address1.Text;
                                    string Owner = Multiparcelid[2].Text;
                                    string Pin = Multiparcelid[4].Text;
                                    string Multiparcel = Addressst + "~" + Owner;
                                    gc.insert_date(orderNumber, Pin, 1196, Multiparcel, 1, DateTime.Now);
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
                                HttpContext.Current.Session["multiParcel_Jefferson"] = "Maximum";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Max > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Jefferson_Multicount"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                            if (Max == 0)
                            {
                                HttpContext.Current.Session["Zero_Jefferson"] = "Zero";
                                driver.Quit();
                                return "Zero";
                            }

                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {
                        if (parcelNumber.Trim() != "")
                        {
                            gc.CreatePdf(orderNumber, parcelNumber, "Search Before Parcel", driver, "KY", "Jefferson");
                            IWebElement ParcelclickTble = driver.FindElement(By.Id("propertySearch"));
                            IList<IWebElement> Parcelclickrow = ParcelclickTble.FindElements(By.TagName("li"));
                            IList<IWebElement> Parcelclickid;
                            foreach (IWebElement Parcelclick in Parcelclickrow)
                            {
                                Parcelclickid = Parcelclick.FindElements(By.TagName("a"));
                                if (Parcelclickid.Count != 0 && Parcelclick.Text.Contains("Parcel ID"))
                                {
                                    try
                                    {
                                        Parcelclickid[0].Click();

                                    }
                                    catch { }
                                    try
                                    {
                                        Parcelclickid[0].SendKeys(Keys.Enter);

                                    }
                                    catch { }
                                    try
                                    {
                                        IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                                        js1.ExecuteScript("arguments[0].click();", Parcelclickid);
                                        Thread.Sleep(4000);

                                    }
                                    catch { }
                                    Thread.Sleep(2000);
                                    break;
                                }
                            }
                            gc.CreatePdf(orderNumber, parcelNumber, "SearchAfter Parcel", driver, "KY", "Jefferson");
                            driver.FindElement(By.Id("psfldParcelId")).SendKeys(parcelNumber);
                            driver.FindElement(By.XPath("//*[@id='searchFormParcelId']/fieldset/p[2]/input")).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Searchbefore", driver, "KY", "Jefferson");
                        }
                        if (parcelNumber.Trim() == "")
                        {
                            HttpContext.Current.Session["Zero_Jefferson"] = "Zero";
                            driver.Quit();
                            return "Zero";
                        }
                    }
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    string propertdetail = driver.FindElement(By.XPath("//*[@id='primary']/div/dl")).Text;
                    string OwnerProperty = gc.Between(propertdetail, "Owner", "Parcel ID");
                    Parcel_number = gc.Between(propertdetail, "Parcel ID", "Assessed Value").Trim();
                    string AssessedValue = gc.Between(propertdetail, "Assessed Value", "Acres").Trim();
                    string Acres = gc.Between(propertdetail, "Acres", "Neighborhood").Trim();
                    string Neighborhood = GlobalClass.After(propertdetail, "Neighborhood").Trim();
                    string Propertresult = OwnerProperty + "~" + Neighborhood;
                    string Assmentresult = AssessedValue + "~" + Acres;
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='primary']/div/dl/dd[5]")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, Parcel_number, "PropertyDetail", driver, "KY", "Jefferson");
                    }
                    catch { }
                    gc.CreatePdf(orderNumber, Parcel_number, "PropertyDetail", driver, "KY", "Jefferson");
                    gc.insert_date(orderNumber, Parcel_number, 1203, Propertresult, 1, DateTime.Now);
                    gc.insert_date(orderNumber, Parcel_number, 1204, Assmentresult, 1, DateTime.Now);
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    //driver.Quit();
                    //AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    //string propertdetail = driver.FindElement(By.Id("basic-info")).Text;
                    //MailingAddress = gc.Between(propertdetail, "Mailing Address", "Owner");
                    //string OwnerProperty = gc.Between(propertdetail, "Owner", "Parcel ID");
                    //string Propertyaddress = driver.FindElement(By.XPath("//*[@id='primary']/div/h1")).Text;
                    //Parcel_number = gc.Between(propertdetail, "Parcel ID", "Land Value").Trim();
                    //string Lane = gc.Between(propertdetail, "Land Value", "Improvements Value");
                    //string ImprovementsValue = gc.Between(propertdetail, "Improvements Value", "Assessed Value");
                    //string AssessedValue = gc.Between(propertdetail, "Assessed Value", "Approximate Acreage").Trim();
                    //string Acreage = gc.Between(propertdetail, "Approximate Acreage", "Property Class");
                    //string Propertyclass = gc.Between(propertdetail, "Property Class", "Deed Book/Page");
                    //string DistrictNumber = gc.Between(propertdetail, "District Number", "Old District");
                    //string Olddistrict = gc.Between(propertdetail, "Old District", "Fire District");
                    //string FireDistrict = gc.Between(propertdetail, "Fire District", "School District");
                    //string SchoolDistrict = gc.Between(propertdetail, "School District", "Neighborhood");
                    //string LegalDescription = driver.FindElement(By.XPath("//*[@id='legal-container']/table/tbody/tr/td[2]")).Text;
                    //string Acres = gc.Between(propertdetail, "Acres", "Neighborhood").Trim();
                    //string Neighborhood = gc.Between(propertdetail, "Neighborhood", "Satellite City").Trim();
                    //try
                    //{
                    //    Yearbuilt = driver.FindElement(By.XPath("//*[@id='improvements']/div/div[1]/dl/dd[2]")).Text;
                    //}
                    //catch { }
                    //string Propertresult = Propertyaddress + "~" + MailingAddress+"~"+ OwnerProperty+"~"+ Acreage+"~"+ Propertyclass+"~"+ DistrictNumber+"~"+ Olddistrict+"~"+ FireDistrict+"~"+ SchoolDistrict +"~"+ Neighborhood+"~"+ Yearbuilt+ "~"+ LegalDescription;
                    //string Assmentresult = Lane+"~"+AssessedValue + "~" + ImprovementsValue;
                    //try
                    //{
                    //    driver.FindElement(By.XPath("//*[@id='primary']/div/dl/dd[5]")).Click();
                    //    Thread.Sleep(2000);
                    //    gc.CreatePdf(orderNumber, Parcel_number, "PropertyDetail", driver, "KY", "Jefferson");
                    //}
                    //catch { }
                    //gc.CreatePdf(orderNumber, Parcel_number, "PropertyDetail", driver, "KY", "Jefferson");
                    //gc.insert_date(orderNumber, Parcel_number, 1203, Propertresult, 1, DateTime.Now);
                    //gc.insert_date(orderNumber, Parcel_number, 1204, Assmentresult, 1, DateTime.Now);
                    //TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    //driver.Quit();
                    driver = new PhantomJSDriver();
                    //driver = new ChromeDriver();
                    driver.Navigate().GoToUrl("http://www.jcsoky.org/ptax_search_pid.asp");
                    driver.FindElement(By.Name("WEPROP")).SendKeys(Parcel_number);
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/div/table/tbody/tr/td/div/table/tbody/tr/td/div[3]/table/tbody/tr/td/form/table/tbody/tr/td[3]/input[2]")).Click();
                    }
                    catch { }
                    Thread.Sleep(2000);
                    try
                    {
                        string TaxNodata = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/div/table/tbody/tr/td/div/table/tbody/tr/td/div[3]/table/tbody/tr/td/text()")).Text;
                        if (TaxNodata.Contains("Sorry - your"))
                        {
                            HttpContext.Current.Session["Zero_Jefferson"] = "Zero";
                            driver.Quit();
                            return "Zero";
                        }
                    }
                    catch
                    { }
                    gc.CreatePdf(orderNumber, Parcel_number, "Tax Detail", driver, "KY", "Jefferson");
                    string PropertyAddresstable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/div/table/tbody/tr/td/div/table/tbody/tr/td/div[3]/table/tbody/tr/td/div[2]/table/tbody/tr/td/table[1]/tbody/tr[1]/td[1]/table/tbody")).Text;
                    string PropertyAddress = gc.Between(PropertyAddresstable, "Property Location Address:", "Property's Taxable Assessment:");
                    string Assessed_Value = GlobalClass.After(PropertyAddresstable, "Property's Taxable Assessment:").Replace("\r\n", "");
                    string Propertyidpro = gc.Between(PropertyAddresstable, "Property ID:", "Property Location Address:");
                    string Propertyowner = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/div/table/tbody/tr/td/div/table/tbody/tr/td/div[3]/table/tbody/tr/td/div[2]/table/tbody/tr/td/table[1]/tbody/tr[1]/td[2]/table/tbody")).Text;
                    string OwnerName1 = GlobalClass.After(Propertyowner, "Property Owner:");
                    string[] ownerNamesplit = OwnerName1.Split('\r');
                    string ownerName = ownerNamesplit[1].Replace("\n", "").Trim();
                    string Mailing_Address = ownerNamesplit[2].Replace("\n", "").Trim() + " " + ownerNamesplit[4].Replace("\n", "").Trim();
                    string GrossTax = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/div/table/tbody/tr/td/div/table/tbody/tr/td/div[3]/table/tbody/tr/td/div[2]/table/tbody/tr/td/table[1]/tbody/tr[1]/td[2]/p")).Text;
                    string Grosstaxresult = GlobalClass.After(GrossTax, "Amt:").Trim();
                    string Taxyeartable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/div/table/tbody/tr/td/div/table/tbody/tr/td/div[3]/table/tbody/tr/td/div[2]/table/tbody/tr/td/table[1]/tbody/tr[1]/td[3]/table/tbody")).Text;
                    Year = gc.Between(Taxyeartable, "Tax Year:", "Invoice Number:");
                    string Invoice = gc.Between(Taxyeartable, "Invoice Number:", "Mortgage Company Name:");
                    string Homesteadtable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/div/table/tbody/tr/td/div/table/tbody/tr/td/div[3]/table/tbody/tr/td/div[2]/table/tbody/tr/td/table[2]/tbody")).Text;
                    string HomesteadExemptionAmount = gc.Between(Homesteadtable, "Homestead Exemption Amount:", "Disability Exemption Amount:");
                    string DisabilityExemption = gc.Between(Homesteadtable, "Disability Exemption Amount:", "Payment Periods");
                    string PaidAmount = gc.Between(Homesteadtable, "Amount Paid:", "Paid on:");
                    string Comments = "";
                    if (Homesteadtable.Trim().Contains("(or refunded/modified)"))
                    {
                        PaidDate = gc.Between(Homesteadtable, "Paid on: (or refunded/modified)", "Balance Due:").Trim();
                    }
                    if (!Homesteadtable.Trim().Contains("(or refunded/modified)"))
                    {
                        PaidDate = gc.Between(Homesteadtable, "Paid on:", "Balance Due:").Trim();
                    }
                    string BalanceDue = GlobalClass.After(Homesteadtable, "Balance Due:");
                    IWebElement Homesteadtable1 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/div/table/tbody/tr/td/div/table/tbody/tr/td/div[3]/table/tbody/tr/td/div[2]/table/tbody/tr/td/table[2]/tbody"));
                    IList<IWebElement> Propertyrow = Homesteadtable1.FindElements(By.TagName("tr"));
                    IList<IWebElement> Propertyid;
                    foreach (IWebElement Property in Propertyrow)
                    {
                        Propertyid = Property.FindElements(By.TagName("td"));
                        if (Propertyid.Count == 3 && !Property.Text.Contains("Payment Periods"))
                        {
                            string PropertPeriodsresult = Propertyid[0].Text + "~" + Propertyid[1].Text + "~" + Propertyid[2].Text;
                            gc.insert_date(orderNumber, Parcel_number, 1212, PropertPeriodsresult, 1, DateTime.Now);
                        }
                    }
                    //Delinquent Tax information
                    driver.Navigate().GoToUrl("http://jeffersoncountyclerk.org/deltax/Login.aspx");
                    driver.FindElement(By.Id("Login")).Click();
                    Thread.Sleep(2000);
                    driver.FindElement(By.Id("SearchJefferson")).SendKeys(Propertyidpro);
                    gc.CreatePdf(orderNumber, Parcel_number, "Delinquent Search", driver, "KY", "Jefferson");
                    driver.FindElement(By.Id("btnSearchJefferson")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Delinquent After Search", driver, "KY", "Jefferson");
                    try
                    {
                        string Comments1 = driver.FindElement(By.Id("lblNoRecordsJefferson")).Text;
                        Comments = GlobalClass.Before(Comments1, "Please click Reset");
                    }
                    catch { }
                    string Taxpropertyresult = Propertyidpro + "~" + PropertyAddress + "~" + ownerName + "~" + Mailing_Address + "~" + Year + "~" + Invoice + "~" + Assessed_Value + "~" + Grosstaxresult + "~" + HomesteadExemptionAmount + "~" + DisabilityExemption + "~" + PaidAmount + "~" + PaidDate + "~" + BalanceDue + "~" + Comments + "~" + Tax_Authority;
                    gc.insert_date(orderNumber, Parcel_number, 1211, Taxpropertyresult, 1, DateTime.Now);
                    try
                    {
                        //driver.FindElement(By.Id("txtPageSizeJefferson")).Clear();
                        driver.FindElement(By.Id("txtPageSizeJefferson")).SendKeys("25");
                        Thread.Sleep(1000);
                        driver.FindElement(By.Id("txtPageSizeJefferson")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, Parcel_number, "Delinquent Result", driver, "KY", "Jefferson");
                        IWebElement Delinquenttaxtable = driver.FindElement(By.XPath("//*[@id='BillListJefferson']/tbody"));
                        IList<IWebElement> Delinquentrow = Delinquenttaxtable.FindElements(By.TagName("tr"));
                        IList<IWebElement> Delinquentid;
                        foreach (IWebElement Delinquent in Delinquentrow)
                        {
                            Delinquentid = Delinquent.FindElements(By.TagName("td"));
                            if (Delinquentid.Count != 0 && !Delinquent.Text.Contains("Parcel ID") && Delinquentid.Count != 1)
                            {
                                string Delinquentresult = Delinquentid[1].Text + "~" + Delinquentid[2].Text + "~" + Delinquentid[3].Text + "~" + Delinquentid[4].Text + "~" + Delinquentid[5].Text + "~" + Delinquentid[6].Text + "~" + Delinquentid[7].Text + "~" + Delinquentid[8].Text + "~" + Delinquentid[9].Text + "~" + Delinquentid[10].Text + "~" + Delinquentid[11].Text;
                                gc.insert_date(orderNumber, Parcel_number, 1214, Delinquentresult, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "KY", "Jefferson", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();

                    gc.mergpdf(orderNumber, "KY", "Jefferson");
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