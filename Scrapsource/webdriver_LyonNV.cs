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
    public class webdriver_LyonNV
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();
        IWebElement IAmg;
        public string FTP_LyonNV(string Address, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string Parcel_number = "", Tax_Authority = "", Year = "", Assmentresult1 = "", Assmentresult2 = "", Assmentresult3 = "", parcelhref = "", MailingAddress="";
            //request.UseDefaultCredentials = true;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                //RDP Site
                StartTime = DateTime.Now.ToString("HH:mm:ss");
                try
                {
                    driver.Navigate().GoToUrl("http://www.lyon-county.org/16/Clerk-Treasurer");
                    IWebElement Tax_Authority1 = driver.FindElement(By.XPath("//*[@id='cc63f54a4a-d572-4950-b6b6-883bceeb6c73']/div[1]/div/div/div/div/ol/li"));
                    Tax_Authority = gc.Between(Tax_Authority1.Text, "Physical Address", "Fax:");
                    gc.CreatePdf_WOP(orderNumber, "tax Authority", driver, "NV", "Lyon");
                }
                catch { }
                try
                {
                    driver.Navigate().GoToUrl("http://www1.lyon-county.org:403/cgi-bin/asw100");
                    if (searchType == "titleflex")
                    {
                        //string address = houseno + " " + direction + " " + streetname + " " + streettype;
                        gc.TitleFlexSearch(orderNumber, "", "", Address.Trim(), "NV", "Lyon");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_LyonNV"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        driver.FindElement(By.XPath("//*[@id='body']/form[2]/table/tbody/tr/td/table/tbody/tr[7]/td[4]/div/input")).SendKeys(Address);
                        driver.FindElement(By.XPath("//*[@id='body']/form[2]/table/tbody/tr/td/table/tbody/tr[10]/td[5]/div/input")).Click();
                        Thread.Sleep(2000);
                        try
                        {
                            int max = 0;
                            gc.CreatePdf_WOP(orderNumber, "Address", driver, "NV", "Lyon");
                            IWebElement Addressclicktable = driver.FindElement(By.XPath("//*[@id='body']/form[2]/table/tbody/tr[3]/td/table/tbody"));
                            IList<IWebElement> Addressclickrow = Addressclicktable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Addressclickid;
                            foreach (IWebElement Addresemulti in Addressclickrow)
                            {
                                Addressclickid = Addresemulti.FindElements(By.TagName("td"));
                                if (!Addresemulti.Text.Contains("Search Results") && !Addresemulti.Text.Contains("Parcel #") && Addressclickrow.Count < 26 && Addressclickid.Count != 0 && Addresemulti.Text.Trim() != "")
                                {
                                    IWebElement parcelclick = Addressclickid[0].FindElement(By.TagName("a"));
                                    parcelhref = parcelclick.GetAttribute("href");
                                    string multiparcelresult = Addressclickid[1].Text + "~" + Addressclickid[2].Text;
                                    gc.insert_date(orderNumber, Addressclickid[0].Text, 973, multiparcelresult, 1, DateTime.Now);
                                    max++;
                                }
                            }
                            if (max == 1)
                            {
                                driver.Navigate().GoToUrl(parcelhref);
                                Thread.Sleep(2000);
                            }
                            if (max > 1 && max < 26)
                            {
                                HttpContext.Current.Session["multiparcel_Lyon"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (max > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Lyon_Multicount"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }

                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.XPath("//*[@id='asw100parcels']/input[1]")).SendKeys(parcelNumber.Replace("-", ""));
                        driver.FindElement(By.XPath("//*[@id='body']/form[2]/table/tbody/tr/td/table/tbody/tr[10]/td[5]/div/input")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Parcel", driver, "NV", "Lyon");
                        try
                        {
                            IWebElement Addressclicktable = driver.FindElement(By.XPath("//*[@id='body']/form[2]/table/tbody/tr[3]/td/table/tbody"));
                            IList<IWebElement> Addressclickrow = Addressclicktable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Addressclickid;
                            foreach (IWebElement Addresemulti in Addressclickrow)
                            {
                                Addressclickid = Addresemulti.FindElements(By.TagName("td"));
                                if (Addressclickrow.Count == 4 && !Addresemulti.Text.Contains("Search Results") && !Addresemulti.Text.Contains("Parcel #") && Addressclickid.Count != 0)
                                {
                                    IWebElement parcelclick = Addressclickid[0].FindElement(By.TagName("a"));
                                    parcelhref = parcelclick.GetAttribute("href");
                                    driver.Navigate().GoToUrl(parcelhref);
                                    Thread.Sleep(2000);
                                    break;
                                }
                            }
                        }
                        catch { }
                    }
                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.XPath("//*[@id='asw100name']/input")).SendKeys(ownername);
                        driver.FindElement(By.XPath("//*[@id='body']/form[2]/table/tbody/tr/td/table/tbody/tr[10]/td[5]/div/input")).Click();
                        Thread.Sleep(2000);
                        try
                        {
                            int max = 0;
                            gc.CreatePdf_WOP(orderNumber, "Address", driver, "NV", "Lyon");
                            IWebElement Addressclicktable = driver.FindElement(By.XPath("//*[@id='body']/form[2]/table/tbody/tr[3]/td/table/tbody"));
                            IList<IWebElement> Addressclickrow = Addressclicktable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Addressclickid;
                            foreach (IWebElement Addresemulti in Addressclickrow)
                            {
                                Addressclickid = Addresemulti.FindElements(By.TagName("td"));
                                if (!Addresemulti.Text.Contains("Search Results") && !Addresemulti.Text.Contains("Parcel #") && Addressclickrow.Count < 26 && Addressclickid.Count != 0 && Addresemulti.Text.Trim() != "")
                                {
                                    IWebElement parcelclick = Addressclickid[0].FindElement(By.TagName("a"));
                                    parcelhref = parcelclick.GetAttribute("href");
                                    string multiparcelresult = Addressclickid[1].Text + "~" + Addressclickid[2].Text;
                                    gc.insert_date(orderNumber, Addressclickid[0].Text, 973, multiparcelresult, 1, DateTime.Now);
                                    max++;
                                }
                            }
                            if (max == 1)
                            {
                                driver.Navigate().GoToUrl(parcelhref);
                                Thread.Sleep(2000);
                            }
                            if (max > 1 && max < 26)
                            {
                                HttpContext.Current.Session["multiparcel_Lyon"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (max > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Lyon_Multicount"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }

                        }
                        catch { }
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.XPath("//*[@id='body']/form[2]/table/tbody/tr[3]/td/table"));
                        if(INodata.Text.Contains("No results found"))
                        {
                            HttpContext.Current.Session["Nodata_LyonNV"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    //property detail
                    gc.CreatePdf_WOP(orderNumber, "After click", driver, "NV", "Lyon");
                    IWebElement parcelnumweb = driver.FindElement(By.XPath("//*[@id='body']/form[2]/table[2]/tbody/tr[1]/td/div"));
                    Parcel_number = GlobalClass.After(parcelnumweb.Text.Replace("-", ""), "Parcel #").Trim();
                    IWebElement propertydetaillist = driver.FindElement(By.XPath("//*[@id='body']/form[2]/table[2]/tbody/tr[3]/td[1]/table/tbody"));
                    string PropertyLocation = gc.Between(propertydetaillist.Text, "Property Location", "Town");
                    string Town = gc.Between(propertydetaillist.Text, "Town", "District");
                    string District = gc.Between(propertydetaillist.Text, "District", "Subdivision");
                    IWebElement ownershipdetail = driver.FindElement(By.XPath("//*[@id='body']/form[2]/table[2]/tbody/tr[3]/td[2]/table/tbody"));
                    string AssessedOwnerName = gc.Between(ownershipdetail.Text, "Assessed Owner Name", "Mailing Address");
                    if (ownershipdetail.Text.Contains("Add'l Owners"))
                    {
                        MailingAddress = gc.Between(ownershipdetail.Text, "Add'l Owners", "Legal Owner Name");
                    }
                    if (!ownershipdetail.Text.Contains("Add'l Owners"))
                    {
                        MailingAddress = gc.Between(ownershipdetail.Text, "Mailing Address", "Legal Owner Name");
                    }
                    string LegalOwnerName = gc.Between(ownershipdetail.Text, "Legal Owner Name", "Vesting Doc #");
                    try
                    {
                        Year = driver.FindElement(By.XPath("//*[@id='body']/form[2]/table[2]/tbody/tr[4]/td[2]/table/tbody/tr[9]/td[2]/div")).Text;
                    }
                    catch { }
                    string Propertyresult = PropertyLocation + "~" + Town + "~" + District + "~" + AssessedOwnerName + "~" + MailingAddress + "~" + LegalOwnerName + "~" + Year;
                    gc.insert_date(orderNumber, Parcel_number, 930, Propertyresult, 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    string taxassment1 = "", taxassment2 = "", taxassment3 = "";
                    IWebElement assmentdetailtable = driver.FindElement(By.XPath("//*[@id='body']/form[2]/table[2]/tbody/tr[5]/td[1]/table/tbody"));
                    IList<IWebElement> assmentdetailrow = assmentdetailtable.FindElements(By.TagName("tr"));
                    IList<IWebElement> assmentdetailid;
                    foreach (IWebElement assmentdetail in assmentdetailrow)
                    {
                        assmentdetailid = assmentdetail.FindElements(By.TagName("td"));
                        if (assmentdetailid.Count != 0 && !assmentdetail.Text.Contains("Increased") && !assmentdetail.Text.Contains("Assessed Valuation") && assmentdetail.Text.Trim() != "" && !assmentdetail.Text.Contains("Taxable Values"))
                        {
                            Assmentresult1 += assmentdetailid[1].Text + "~";
                            Assmentresult2 += assmentdetailid[2].Text + "~";
                            Assmentresult3 += assmentdetailid[3].Text + "~";

                        }
                        if (assmentdetail.Text.Contains("Increased"))
                        {
                            break;
                        }
                    }
                    IWebElement Taxabledetailtable = driver.FindElement(By.XPath("//*[@id='body']/form[2]/table[2]/tbody/tr[5]/td[2]/table/tbody"));
                    IList<IWebElement> taxdetailrow = Taxabledetailtable.FindElements(By.TagName("tr"));
                    IList<IWebElement> Taxdetailid;
                    foreach (IWebElement taxdetail in taxdetailrow)
                    {
                        Taxdetailid = taxdetail.FindElements(By.TagName("td"));
                        if (Taxdetailid.Count != 0 && !taxdetail.Text.Contains("Increased") && !taxdetail.Text.Contains("Taxable Valuation") && taxdetail.Text.Trim() != "" && !taxdetail.Text.Contains("Taxable Values"))
                        {
                            taxassment1 += Taxdetailid[1].Text + "~";
                            taxassment2 += Taxdetailid[2].Text + "~";
                            taxassment3 += Taxdetailid[3].Text + "~";
                        }
                        if (taxdetail.Text.Contains("Increased"))
                        {
                            break;
                        }
                    }
                    gc.insert_date(orderNumber, Parcel_number, 934, Assmentresult1 + taxassment1.Remove(taxassment1.Length - 1), 1, DateTime.Now);
                    gc.insert_date(orderNumber, Parcel_number, 934, Assmentresult2 + taxassment2.Remove(taxassment2.Length - 1), 1, DateTime.Now);
                    gc.insert_date(orderNumber, Parcel_number, 934, Assmentresult3 + taxassment3.Remove(taxassment3.Length - 1), 1, DateTime.Now);
                    //tax Information
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("http://www1.lyon-county.org:403/cgi-bin/tcw100");
                    driver.FindElement(By.XPath("//*[@id='body']/form[2]/table/tbody/tr/td/table/tbody/tr[3]/td[2]/div/input")).SendKeys(Parcel_number.Trim());
                    gc.CreatePdf(orderNumber, Parcel_number, "Tax Before click", driver, "NV", "Lyon");
                    driver.FindElement(By.XPath("//*[@id='tcw100search']/input")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Tax After click", driver, "NV", "Lyon");
                    IWebElement Taxparcelclicktable = driver.FindElement(By.XPath("//*[@id='body']/form[3]/table/tbody"));
                    IList<IWebElement> Taxparcelclickrow = Taxparcelclicktable.FindElements(By.TagName("tr"));
                    IList<IWebElement> Taxparcelid;
                    foreach (IWebElement Taxparcel in Taxparcelclickrow)
                    {
                        Taxparcelid = Taxparcel.FindElements(By.TagName("td"));
                        if (Taxparcelid.Count != 0 && !Taxparcel.Text.Contains("Search Results") && !Taxparcel.Text.Contains("Search Results") && !Taxparcel.Text.Contains("Parcel #"))
                        {
                            IWebElement taxelement = Taxparcelid[0].FindElement(By.TagName("a"));
                            string taxhfrf = taxelement.GetAttribute("href");
                            driver.Navigate().GoToUrl(taxhfrf);
                            Thread.Sleep(2000);
                            break;
                        }
                    }
                    IWebElement Taxelement = driver.FindElement(By.XPath("//*[@id='body']/table[2]/tbody/tr[2]/td/table[1]/tbody/tr[1]/td[4]"));
                    string[] taxinfoarray = Taxelement.Text.Split('\r', '\n');
                    string Tax_year = taxinfoarray[0];
                    string roll = taxinfoarray[2];
                    string TaxService = taxinfoarray[6];
                    string LandUseCode = taxinfoarray[8];
                    string Taxinformation = Tax_year + "~" + roll + "~" + TaxService + "~" + LandUseCode + "~" + Tax_Authority;
                    gc.insert_date(orderNumber, Parcel_number, 941, Taxinformation, 1, DateTime.Now);
                    gc.CreatePdf(orderNumber, Parcel_number, "Tax Information", driver, "NV", "Lyon");
                    string type = "";
                    IWebElement taxinfotable = driver.FindElement(By.XPath("//*[@id='body']/table[2]/tbody/tr[2]/td/table[2]/tbody"));
                    IList<IWebElement> taxinforow = taxinfotable.FindElements(By.TagName("tr"));
                    IList<IWebElement> taxinfoid;
                    foreach (IWebElement taxinfo in taxinforow)
                    {
                        taxinfoid = taxinfo.FindElements(By.TagName("td"));
                        if (taxinfoid.Count != 0)
                        {
                            if (taxinfo.Text.Trim().Contains("Outstanding Taxes:") || taxinfo.Text.Contains("Current Year"))
                            {
                                type = taxinfoid[0].Text.Trim();
                            }
                            if (type == "Outstanding Taxes:" && !taxinfo.Text.Contains("Prior Year") && taxinfo.Text.Trim() != "" && !taxinfo.Text.Contains("Outstanding Taxes:"))
                            {
                                string Taxuinforesult = taxinfoid[0].Text + "~" + taxinfoid[1].Text + "~" + taxinfoid[2].Text + "~" + taxinfoid[3].Text + "~" + taxinfoid[4].Text + "~" + taxinfoid[5].Text;
                                gc.insert_date(orderNumber, Parcel_number, 945, Taxuinforesult, 1, DateTime.Now);
                            }
                            if (type == "Current Year" && !taxinfo.Text.Contains("Current Year") && taxinfo.Text.Trim() != "")
                            {
                                string taxinforesult = taxinfoid[0].Text + "~" + taxinfoid[1].Text + "~" + taxinfoid[2].Text + "~" + taxinfoid[3].Text + "~" + taxinfoid[4].Text + "~" + taxinfoid[5].Text;
                                gc.insert_date(orderNumber, Parcel_number, 944, taxinforesult, 1, DateTime.Now);

                            }

                        }
                    }
                    //Additional Information 
                    string additional1 = "", additional2 = "", additional3 = "", additional4 = "", additional5 = "";
                    IWebElement addinformationtable = driver.FindElement(By.XPath("//*[@id='body']/table[3]/tbody/tr[2]/td/table/tbody"));
                    IList<IWebElement> addinformationrow = addinformationtable.FindElements(By.TagName("tr"));
                    IList<IWebElement> addinformationid;
                    foreach (IWebElement addinformation in addinformationrow)
                    {
                        addinformationid = addinformation.FindElements(By.TagName("td"));
                        if (addinformationid.Count != 0 && addinformation.Text.Trim() != "")
                        {
                            additional1 += addinformationid[1].Text.Trim() + "~";
                            additional2 += addinformationid[2].Text.Trim() + "~";
                            additional3 += addinformationid[3].Text.Trim() + "~";
                            additional4 += addinformationid[4].Text.Trim() + "~";
                            additional5 += addinformationid[5].Text.Trim() + "~";
                            //gc.insert_date(orderNumber, Parcel_number, 947, addinformationresult, 1, DateTime.Now);
                        }
                    }
                    gc.insert_date(orderNumber, Parcel_number, 947, additional1.Remove(additional1.Length - 1, 1), 1, DateTime.Now);
                    gc.insert_date(orderNumber, Parcel_number, 947, additional2.Remove(additional2.Length - 1, 1), 1, DateTime.Now);
                    gc.insert_date(orderNumber, Parcel_number, 947, additional3.Remove(additional3.Length - 1, 1), 1, DateTime.Now);
                    gc.insert_date(orderNumber, Parcel_number, 947, additional4.Remove(additional4.Length - 1, 1), 1, DateTime.Now);
                    gc.insert_date(orderNumber, Parcel_number, 947, additional5.Remove(additional5.Length - 1, 1), 1, DateTime.Now);
                    //current tax history
                    // int firsttime = 0;
                    for (int i = 1; i <= 3; i++)
                    {
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='body']/table[2]/tbody/tr[2]/td/table[2]/tbody/tr[12]/td[2]/form/input[1]")).Click();
                        }
                        catch { }
                        Thread.Sleep(2000);
                        string allmosttop = driver.FindElement(By.XPath("//*[@id='body']/table[2]/tbody/tr[2]/td/form/table[2]/tbody/tr[1]/td")).Text;
                        //if (allmosttop.Trim() == "Already at bottom of list.")
                        //{

                        //    firsttime++;
                        //}
                        gc.CreatePdf(orderNumber, Parcel_number, "Tax History" + i, driver, "NV", "Lyon");
                        if (allmosttop.Trim() != "Already at top of list.")  //|| firsttime == 1
                        {
                            //gc.CreatePdf(orderNumber, Parcel_number, "Tax History" + i, driver, "NV", "Lyon");
                            IWebElement taxhistorytable = driver.FindElement(By.XPath("//*[@id='body']/table[2]/tbody/tr[2]/td/table[2]/tbody"));
                            IList<IWebElement> taxhistoryrow = taxhistorytable.FindElements(By.TagName("tr"));
                            IList<IWebElement> taxhistoryid;
                            foreach (IWebElement taxhistory in taxhistoryrow)
                            {
                                taxhistoryid = taxhistory.FindElements(By.TagName("td"));
                                if (taxhistoryid.Count != 0 && !taxhistory.Text.Contains("Date") && !taxhistory.Text.Trim().Contains("More..."))
                                {
                                    string taxhistoryresult = taxhistoryid[0].Text.Trim() + "~" + taxhistoryid[1].Text + "~" + taxhistoryid[2].Text + "~" + taxhistoryid[3].Text + "~" + taxhistoryid[4].Text + "~" + taxhistoryid[5].Text;
                                    gc.insert_date(orderNumber, Parcel_number, 968, taxhistoryresult.Replace("\r\n", ""), 1, DateTime.Now);
                                }
                            }
                            driver.FindElement(By.XPath("//*[@id='body']/table[2]/tbody/tr[2]/td/form/table[1]/tbody/tr/td[1]/div/input")).Click();
                            Thread.Sleep(2000);
                        }
                    }
                    //Special Assessment History
                    int p = 0;
                    //int spelcialfirst = 0;
                    for (int j = 1; j <= 3; j++)
                    {

                        try
                        {
                            if (p == 0)
                            {
                                driver.FindElement(By.XPath("//*[@id='body']/table[2]/tbody/tr[2]/td/table[1]/tbody/tr[5]/td[4]/div/input")).Click();
                                Thread.Sleep(2000);
                                p++;
                            }
                        }
                        catch { }

                        IWebElement allmost = driver.FindElement(By.XPath("//*[@id='body']/table[2]/tbody/tr[2]/td/form/table[2]/tbody/tr[1]/td/div"));
                        string topbottamlist = allmost.Text.Trim();
                        //if (topbottamlist == "Already at bottom of list.")
                        //{
                        //    spelcialfirst++;
                        //}
                        gc.CreatePdf(orderNumber, Parcel_number, "Special Assessment History" + j, driver, "NV", "Lyon");
                        if (topbottamlist != "Already at top of list.") //|| spelcialfirst == 1
                        {
                            //gc.CreatePdf(orderNumber, Parcel_number, "Special Assessment History" + j, driver, "NV", "Lyon");
                            IWebElement SpecialAssessmentstable = driver.FindElement(By.XPath("//*[@id='body']/table[2]/tbody/tr[2]/td/table[2]/tbody"));
                            IList<IWebElement> specialassmentrow = SpecialAssessmentstable.FindElements(By.TagName("tr"));
                            IList<IWebElement> specialassmentid;
                            foreach (IWebElement specialassment in specialassmentrow)
                            {
                                specialassmentid = specialassment.FindElements(By.TagName("td"));
                                if (specialassmentid.Count != 0 && !specialassment.Text.Contains("Special Assessment") && !specialassment.Text.Trim().Contains("More..."))
                                {
                                    string splicialassmentresult = specialassmentid[0].Text + "~" + specialassmentid[1].Text + "~" + specialassmentid[2].Text + "~" + specialassmentid[3].Text + "~" + specialassmentid[4].Text + "~" + specialassmentid[5].Text;
                                    gc.insert_date(orderNumber, Parcel_number, 974, splicialassmentresult, 1, DateTime.Now);
                                }
                            }

                            driver.FindElement(By.XPath("//*[@id='body']/table[2]/tbody/tr[2]/td/form/table[1]/tbody/tr/td[1]/div/input")).Click();
                            Thread.Sleep(2000);
                        }
                    }
                    string strAMGParcelId = "", strAMGDistrict = "", strAMGName = "", strAMGStatus = "", strAMGUnbilled = "", strAMGParcelDetails = "", strLegal = "", strOriginalassess = "", strPayOff = "", strAMGAddressDetail = "", strAmgTaxAuthority = "", strType = "", strPrinicipal = "", strInterest = ""
                       , strPenality = "", strOther = "", strTotalDue = "", strBreakParcel = "", strBreakDistrict = "", strBreakdown = "";
                    try
                    {
                        driver.Navigate().GoToUrl("https://amgnv.com/");
                        driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[6]/table/tbody/tr[2]/td[2]/p/table[2]/tbody/tr/td/table/tbody/tr[2]/td[1]/form/center/b/input")).SendKeys(Parcel_number);
                        gc.CreatePdf(orderNumber, Parcel_number, "Special Assessment", driver, "NV", "Lyon");
                        driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[6]/table/tbody/tr[2]/td[2]/p/table[2]/tbody/tr/td/table/tbody/tr[2]/td[1]/form/center/b/font/font/input")).SendKeys(Keys.Enter);
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, Parcel_number, "Special Assessment Result", driver, "NV", "Lyon");
                        IWebElement IAMGParcelSearch = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[6]/table/tbody/tr[2]/td[2]/p/font[1]/table/tbody/tr[2]/td[4]/div/font/a"));
                        string stramgParcel = IAMGParcelSearch.GetAttribute("href");
                        driver.Navigate().GoToUrl(stramgParcel);
                        gc.CreatePdf(orderNumber, Parcel_number, "AMG Result", driver, "NV", "Lyon");
                        IWebElement IParcelTable = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[6]/table/tbody/tr[2]/td[2]/table[3]/tbody"));
                        IList<IWebElement> IParcelRow = IParcelTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> IParcelTD;
                        foreach (IWebElement parcel in IParcelRow)
                        {
                            IParcelTD = parcel.FindElements(By.TagName("td"));
                            if (IParcelTD.Count != 0 && !parcel.Text.Contains("Parcel #") && !parcel.Text.Contains("Amounts updated"))
                            {
                                try
                                {
                                    strAMGParcelId = IParcelTD[0].Text;
                                    strAMGDistrict = IParcelTD[1].Text;
                                    strAMGName = IParcelTD[2].Text;
                                    strAMGStatus = IParcelTD[3].Text;
                                    strAMGUnbilled = IParcelTD[4].Text;
                                }
                                catch { }

                                strAMGParcelDetails = strAMGDistrict + "~" + strAMGName + "~" + strAMGStatus + "~" + strAMGUnbilled;

                            }
                        }

                        IWebElement IAMGAddressTable = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[6]/table/tbody/tr[2]/td[2]/table[4]/tbody"));
                        IList<IWebElement> IAMGAddressRow = IAMGAddressTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> IAMGAddressTd;
                        foreach (IWebElement address in IAMGAddressRow)
                        {
                            IAMGAddressTd = address.FindElements(By.TagName("td"));
                            if (IAMGAddressTd.Count != 0 && !address.Text.Contains("Situs & Legal Description"))
                            {
                                try
                                {
                                    strLegal += IAMGAddressTd[0].Text + " ";
                                    strOriginalassess = IAMGAddressTd[1].Text;
                                    strPayOff = IAMGAddressTd[2].Text;
                                    IAmg = IAMGAddressTd[2].FindElement(By.TagName("a"));
                                }
                                catch { }

                                strAMGAddressDetail = strLegal + "~" + strOriginalassess + "~" + strPayOff;
                            }
                        }
                        if (strAMGParcelDetails.Trim() != "" && strAMGAddressDetail != "")
                        {
                            try
                            {
                                strAmgTaxAuthority = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[6]/table/tbody/tr[2]/td[2]/table[8]/tbody/tr[2]/td[2]/table/tbody/tr/td[2]")).Text;
                            }
                            catch { }

                            gc.insert_date(orderNumber, Parcel_number, 970, strAMGParcelDetails + "~" + strAMGAddressDetail + "~" + strAmgTaxAuthority, 1, DateTime.Now);
                        }
                        IWebElement IAMGDuePayTable = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[6]/table/tbody/tr[2]/td[2]/table[6]/tbody"));
                        IList<IWebElement> IAMGDuePayRow = IAMGDuePayTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> IAMGDuePayTd;
                        foreach (IWebElement Due in IAMGDuePayRow)
                        {
                            IAMGDuePayTd = Due.FindElements(By.TagName("td"));
                            if (IAMGDuePayTd.Count != 0 && !Due.Text.Contains("Principal") && !Due.Text.Contains("Current Due and Payoff Amounts are valid ") && !Due.Text.Contains("* Penalties are added monthly until the Total Due is paid in full.") && !Due.Text.Contains("**Estimated installments") && !Due.Text.Contains("*** Payoff value"))
                            {
                                try
                                {
                                    strType = IAMGDuePayTd[0].Text;
                                    strPrinicipal = IAMGDuePayTd[1].Text;
                                    strInterest = IAMGDuePayTd[2].Text;
                                    strPenality = IAMGDuePayTd[3].Text;
                                    strOther = IAMGDuePayTd[4].Text;
                                    strTotalDue = IAMGDuePayTd[5].Text;
                                }
                                catch { }

                                strAMGAddressDetail = strType + "~" + strPrinicipal + "~" + strInterest + "~" + strPenality + "~" + strOther + "~" + strTotalDue;
                                gc.insert_date(orderNumber, Parcel_number, 971, strAMGAddressDetail, 1, DateTime.Now);
                            }
                        }

                        try
                        {
                            IAmg.SendKeys(Keys.Enter);
                            string strURL = driver.CurrentWindowHandle;
                            string strURLLast = driver.WindowHandles.Last();
                            driver.SwitchTo().Window(strURLLast);
                            gc.CreatePdf(orderNumber, Parcel_number, "BreakDown Result", driver, "NV", "Lyon");
                            strBreakParcel = driver.FindElement(By.XPath("/html/body/center/table[1]/tbody/tr/td/table/tbody/tr[2]/td[1]")).Text;
                            strBreakDistrict = driver.FindElement(By.XPath("/html/body/center/table[1]/tbody/tr/td/table/tbody/tr[2]/td[2]")).Text;
                            IWebElement IBreakDownTable = driver.FindElement(By.XPath("/html/body/center/table[2]/tbody/tr/td/table/tbody"));
                            IList<IWebElement> IBreakDownRow = IBreakDownTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> IBreakDownTD;
                            foreach (IWebElement Break in IBreakDownRow)
                            {
                                IBreakDownTD = Break.FindElements(By.TagName("td"));
                                if (IBreakDownTD.Count != 0 && !Break.Text.Contains("REAL PROPERTY ASSESSED VALUE"))
                                {
                                    try
                                    {
                                        strBreakdown += IBreakDownTD[1].Text + "~";
                                    }
                                    catch { }
                                }
                            }

                            if (strBreakdown.Length != 0)
                            {
                                string strstrBreakdownDetails = strBreakdown.Remove(strBreakdown.Length - 1);
                                gc.insert_date(orderNumber, Parcel_number, 972, strBreakDistrict + "~" + strstrBreakdownDetails, 1, DateTime.Now);
                            }
                        }
                        catch { }
                    }
                    catch { }
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "NV", "Lyon", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    driver.Quit();
                    gc.mergpdf(orderNumber, "NV", "Lyon");
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