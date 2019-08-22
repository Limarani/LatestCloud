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
    public class Webdriver_WakeNC
    {
        Amrock amck = new Amrock();
        int i = 0;
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();
        string Yearbuild, parcel_number, Addressmax, Multihref, parcelsplit2, DUE, propertyaddress = "", legalDescription = "", Taxing_Authority = "";
        int value = 0;
        string[] ParcelSplit; IWebElement MultiParcel, Accountlink;
        int cuttentyearr = DateTime.Now.Year;
        List<string> AccountAdd = new List<string>();
        public string FTP_WakeNC(string streetno, string direction, string streetname, string streettype, string unitnumber, string ownernm, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            // driver = new ChromeDriver();
            //driver = new PhantomJSDriver();
            //RDP
            //Phantom site
            var option = new ChromeOptions();
            option.AddArgument("No-Sandbox");
            using (driver = new ChromeDriver(option))
            {
                string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
                try
                {
                    string address = "";
                    if (searchType == "titleflex")
                    {
                        if (direction != "")
                        {
                            address = streetno + " " + direction + " " + streetname + " " + unitnumber;
                        }
                        else
                        {
                            address = streetno + " " + streetname + " " + unitnumber;
                        }
                        gc.TitleFlexSearch(orderNumber, "", "", address, "NC", "Wake");
                        if (HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes")
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                        if (parcelNumber.Trim() == "")
                        {
                            HttpContext.Current.Session["Zero_WakeNC"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    driver.Navigate().GoToUrl("http://services.wakegov.com/realestate/search.asp?");
                    if (searchType == "address")
                    {
                        driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[2]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[2]/td[1]/div/form/input")).SendKeys(streetno);
                        driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[2]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]/div/input")).SendKeys(direction.Trim() + " " + streetname.Trim());
                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "NC", "Wake");
                        driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[2]/td[1]/table/tbody/tr[3]/td/div/input")).Click();
                        Thread.Sleep(2000);
                        int Max = 0;
                        try
                        {

                            string Pageno = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr[5]/td[2]/table/tbody/tr/td[2]/center/font")).Text.Trim();
                            string countnumber = gc.Between(Pageno, "Page", "of").Trim();
                            gc.CreatePdf_WOP(orderNumber, "Address After", driver, "NC", "Wake");
                            //try
                            //{
                            //    IWebElement Adressclick = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table/tbody/tr[2]/td[2]/b/a"));
                            //    Adressclick.Click();
                            //    Thread.Sleep(2000);
                            //    Max++;
                            //}
                            //catch { }
                            if (countnumber.Trim() == "0")
                            {
                                HttpContext.Current.Session["Zero_WakeNC"] = "Zero";
                                gc.CreatePdf_WOP(orderNumber, "ZeroSearch", driver, "NC", "Wake");
                                driver.Quit();
                                return "No Data Found";
                            }
                            int pfx = 0;
                            IWebElement MultiAddressTable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table/tbody"));
                            IList<IWebElement> MultiAddressrow = MultiAddressTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiAddressid;
                            foreach (IWebElement Multiaddress in MultiAddressrow)
                            {
                                MultiAddressid = Multiaddress.FindElements(By.TagName("td"));
                                if (Multiaddress.Text.Contains("Sfx") && pfx == 0)
                                {
                                    break;
                                }
                                if (MultiAddressid.Count > 1 & !Multiaddress.Text.Contains("Line"))
                                {
                                    MultiParcel = MultiAddressid[1].FindElement(By.TagName("a"));
                                    Multihref = MultiParcel.GetAttribute("href");
                                    string parcelMulti = MultiAddressid[1].Text;
                                    string Streetno = MultiAddressid[2].Text;
                                    string streetna = MultiAddressid[5].Text;
                                    string OwnerMulti = MultiAddressid[9].Text;
                                    string Owenerresult = Streetno + " " + streetna + "~" + OwnerMulti;
                                    gc.insert_date(orderNumber, parcelMulti, 1554, Owenerresult, 1, DateTime.Now);
                                    Max++;
                                }
                                pfx++;
                            }
                            if (Max == 1)
                            {
                                IWebElement Adressclick = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table/tbody/tr[2]/td[2]/b/a"));
                                Adressclick.Click();
                                Thread.Sleep(2000);
                            }
                            if (Max > 1 && Max < 26)
                            {
                                HttpContext.Current.Session["multiparcel_WakeNC"] = "Yes";
                                gc.CreatePdf_WOP(orderNumber, "MultyAddressSearch", driver, "NC", "Wake");
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Max > 25)
                            {
                                HttpContext.Current.Session["multiParcel_WakeNC_Multicount"] = "Maximum";
                                gc.CreatePdf_WOP(orderNumber, "MultyAddressSearch", driver, "NC", "Wake");
                                driver.Quit();
                                return "Maximum";
                            }

                        }
                        catch { }
                        if (Max == 0)
                        {
                            SecondMultiparcel(orderNumber, streetno, direction, streetname, streettype);
                            try
                            {
                                if (HttpContext.Current.Session["multiparcel_WakeNC"].ToString() == "Yes")
                                {

                                    return "MultiParcel";
                                }
                                else
                                {
                                    HttpContext.Current.Session["Single_WakeNC"] = "";
                                }
                                
                            }
                            catch { }
                            try
                            {
                                if(HttpContext.Current.Session["Zero_WakeNC"].ToString()== "Zero")
                                {
                                    return "No Data Found";
                                }
                            }
                            catch { }
                        }

                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[1]/td[2]/table/tbody/tr[3]/td/div/form/input")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "NC", "Wake");
                        driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[1]/td[2]/table/tbody/tr[4]/td/div/input")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel After", driver, "NC", "Wake");
                        try
                        {
                            IWebElement INOdata = driver.FindElement(By.XPath("/html/body/div[1]"));
                            if(INOdata.Text.Contains("No matching records were found"))
                            {
                                HttpContext.Current.Session["Zero_WakeNC"] = "Zero";
                                gc.CreatePdf_WOP(orderNumber, "ZeroSearch", driver, "NC", "Wake");
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "ownername")
                    {
                        string[] Ownersplit = ownernm.Split(',');
                        int Countnumber = Ownersplit.Length;
                        if (Countnumber == 3)
                        {
                            driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[2]/td[3]/div/input")).SendKeys(Ownersplit[2]);
                            driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[2]/td[4]/div/input")).SendKeys(Ownersplit[1]);
                            driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[2]/td[6]/div/input")).SendKeys(Ownersplit[0]);
                            Thread.Sleep(2000);
                        }
                        if (Countnumber == 2)
                        {
                            driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[2]/td[3]/div/input")).SendKeys(Ownersplit[1].Trim());
                            driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]/div/input")).SendKeys(Ownersplit[0].Trim());
                            Thread.Sleep(2000);
                        }
                        driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[1]/td[1]/table/tbody/tr[3]/td/div/input")).Click();
                        Thread.Sleep(2000);
                        int Max = 0;
                        try
                        {

                            string Pageno = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr[5]/td[2]/table/tbody/tr/td[2]/center/font")).Text.Trim();
                            string countnumber = gc.Between(Pageno, "Page", "of").Trim();
                            //if (countnumber == "1")
                            //{
                            //    IWebElement Adressclick = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table/tbody/tr[2]/td[5]/b/a"));
                            //    Adressclick.Click();
                            //    Thread.Sleep(2000);
                            //    Max++;
                            //}

                            IWebElement MultiAddressTable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table/tbody"));
                            IList<IWebElement> MultiAddressrow = MultiAddressTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiAddressid;
                            foreach (IWebElement Multiaddress in MultiAddressrow)
                            {
                                MultiAddressid = Multiaddress.FindElements(By.TagName("td"));
                                if (MultiAddressid.Count > 1 & !Multiaddress.Text.Contains("Line"))
                                {
                                    // MultiParcel = MultiAddressid[1].FindElement(By.TagName("a"));
                                    // Multihref = MultiParcel.GetAttribute("href");
                                    string parcelMulti = MultiAddressid[2].Text;
                                    string Streetno = MultiAddressid[5].Text;
                                    //string streetna = MultiAddressid[5].Text;
                                    string OwnerMulti = MultiAddressid[4].Text;
                                    string Owenerresult = Streetno + "~" + OwnerMulti;
                                    gc.insert_date(orderNumber, parcelMulti, 1554, Owenerresult, 1, DateTime.Now);
                                    Max++;
                                }
                            }
                            if (Max > 1 && Max < 26)
                            {
                                HttpContext.Current.Session["multiparcel_WakeNC"] = "Yes";
                                gc.CreatePdf_WOP(orderNumber, "MultyAddressSearch", driver, "NC", "Wake");
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Max > 25)
                            {
                                HttpContext.Current.Session["multiParcel_WakeNC_Multicount"] = "Maximum";
                                gc.CreatePdf_WOP(orderNumber, "MultyAddressSearch", driver, "NC", "Wake");
                                driver.Quit();
                                return "Maximum";
                            }

                        }
                        catch { }
                        if (Max == 0)
                        {
                            driver.Navigate().GoToUrl("http://services.wakegov.com/realestate/search.asp?");
                            driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[2]/td[6]/div/input")).SendKeys(ownernm);
                            driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[1]/td[1]/table/tbody/tr[3]/td/div/input")).Click();
                            Thread.Sleep(2000);
                            int Z = 0;
                            string Pageno = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr[5]/td[2]/table/tbody/tr/td[2]/center/font")).Text.Trim();
                            string countnumber = gc.Between(Pageno, "Page", "of").Trim();
                            if (countnumber == "0")
                            {
                                HttpContext.Current.Session["Zero_WakeNC"] = "Zero";
                                gc.CreatePdf_WOP(orderNumber, "ZeroSearch", driver, "NC", "Wake");
                                driver.Quit();
                                return "No Data Found";
                            }
                            if (countnumber == "1")
                            {
                                IWebElement Adressclick = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table/tbody/tr[2]/td[5]/b/a"));
                                Adressclick.Click();
                                Thread.Sleep(2000);
                                Z++;
                            }
                            if (countnumber != "1")
                            {
                                IWebElement MultiAddressTable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table/tbody"));
                                IList<IWebElement> MultiAddressrow = MultiAddressTable.FindElements(By.TagName("tr"));
                                IList<IWebElement> MultiAddressid;
                                foreach (IWebElement Multiaddress in MultiAddressrow)
                                {
                                    MultiAddressid = Multiaddress.FindElements(By.TagName("td"));
                                    if (MultiAddressid.Count > 1 & !Multiaddress.Text.Contains("Line"))
                                    {
                                        // MultiParcel = MultiAddressid[1].FindElement(By.TagName("a"));
                                        // Multihref = MultiParcel.GetAttribute("href");
                                        string parcelMulti = MultiAddressid[1].Text;
                                        string Streetno = MultiAddressid[3].Text;
                                        //string streetna = MultiAddressid[5].Text;
                                        string OwnerMulti = MultiAddressid[2].Text;
                                        string Owenerresult = Streetno + "~" + OwnerMulti;
                                        gc.insert_date(orderNumber, parcelMulti, 1554, Owenerresult, 1, DateTime.Now);
                                        Z++;
                                    }
                                }

                                if (Z > 1 && Z < 26)
                                {
                                    driver.Navigate().GoToUrl(Multihref);
                                    Thread.Sleep(2000);
                                }
                            }
                        }
                    }
                    string PropertydetailTable = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr[3]/td[1]/table/tbody")).Text;
                    parcel_number = gc.Between(PropertydetailTable, "Real Estate ID ", "PIN #").Trim();
                    string Pin = gc.Between(PropertydetailTable, "PIN #", "Location Address").Trim();
                    propertyaddress = gc.Between(PropertydetailTable, "Location Address", "Property Description").Trim();
                    if (propertyaddress.Trim() == "")
                    {
                        string Proaddress = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td[3]/table/tbody")).Text;
                        propertyaddress = GlobalClass.After(Proaddress, "Property Location Address");
                        legalDescription = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr[3]/td[1]/table/tbody/tr[4]/td[2]")).Text;
                    }
                    else
                    {
                        legalDescription = GlobalClass.After(PropertydetailTable, "Property Description").Trim();
                    }
                    string Ownername = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td[1]/table/tbody/tr[2]/td")).Text.Trim();
                    string MailingTable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td[2]/table/tbody")).Text;
                    string MailingAddress = GlobalClass.After(MailingTable, "Owner's Mailing Address").Trim();
                    gc.CreatePdf(orderNumber, parcel_number.Replace("/", ""), "Assessment", driver, "NC", "Wake");
                    IWebElement Assessmenttable = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[3]/table/tbody"));
                    IList<IWebElement> Assessmentrow = Assessmenttable.FindElements(By.TagName("tr"));
                    IList<IWebElement> assessmentid;
                    foreach (IWebElement Assessment in Assessmentrow)
                    {
                        assessmentid = Assessment.FindElements(By.TagName("td"));
                        if (assessmentid.Count > 1 && !Assessment.Text.Contains("Assessed Value") && assessmentid[0].Text.Trim() != "")
                        {
                            string Assessmentresult = assessmentid[0].Text + "~" + assessmentid[1].Text;
                            gc.insert_date(orderNumber, parcel_number, 1535, Assessmentresult, 1, DateTime.Now);
                        }
                    }
                    driver.FindElement(By.LinkText("Buildings")).Click();
                    Thread.Sleep(2000);
                    try
                    {
                        Yearbuild = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[2]/table/tbody/tr[1]/td[1]/table/tbody/tr[1]/td[2]")).Text;
                    }
                    catch { }
                    gc.CreatePdf(orderNumber, parcel_number.Replace("/", ""), "Year Build", driver, "NC", "Wake");
                    string buildingtype = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[1]/table/tbody/tr[1]/td[2]")).Text;
                    string Propertyresult = Pin + "~" + Ownername + "~" + propertyaddress + "~" + MailingAddress + "~" + legalDescription + "~" + buildingtype + "~" + Yearbuild;
                    gc.insert_date(orderNumber, parcel_number, 1534, Propertyresult, 1, DateTime.Now);


                    // gc.insert_date(orderNumber, parcel_number, 1535, Assessmentresult, 1, DateTime.Now);
                    //Tax Information
                    try
                    {
                        driver.Navigate().GoToUrl("https://services.wakegov.com/TaxPortal/Contact");
                        string Taxauthority1 = driver.FindElement(By.XPath("/html/body/div[4]/div")).Text;
                        Taxing_Authority = gc.Between(Taxauthority1, "Physical Address", "Mailing Address");
                    }
                    catch { }
                    driver.Navigate().GoToUrl("https://services.wakegov.com/ptax/main/billing/");
                    IWebElement searchfor = driver.FindElement(By.Id("ddlYears"));
                    SelectElement Searchforvalue = new SelectElement(driver.FindElement(By.Name("ddlYears")));
                    Searchforvalue.SelectByValue("10");
                    Thread.Sleep(1000);
                    gc.CreatePdf(orderNumber, parcel_number.Replace("/", ""), "Tax Site Year", driver, "NC", "Wake");
                    IWebElement ParcelTaxLink = driver.FindElement(By.Id("ddlSearchBy"));
                    SelectElement ParcelTaxvalue = new SelectElement(ParcelTaxLink);
                    ParcelTaxvalue.SelectByIndex(2);
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, parcel_number.Replace("/", ""), "Tax search type", driver, "NC", "Wake");
                    driver.FindElement(By.Id("txtAccount")).SendKeys(parcel_number.Trim());
                    gc.CreatePdf(orderNumber, parcel_number.Replace("/", ""), "Tax Search Before", driver, "NC", "Wake");
                    driver.FindElement(By.Id("Search")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, parcel_number.Replace("/", ""), "Tax Search After", driver, "NC", "Wake");
                    //*[@id="testdefault"]/table/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[1]/tbody/tr[2]/td[4]
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='testdefault']/table/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[1]/tbody/tr[2]/td[4]/a")).Click();
                        Thread.Sleep(2000);
                    }
                    catch { }
                    gc.CreatePdf(orderNumber, parcel_number.Replace("/", ""), "No paging", driver, "NC", "Wake");
                    string accountnumber = "";

                    int month = DateTime.Now.Month;
                    if (month < 6)
                    {
                        cuttentyearr--;
                    }
                    try
                    {
                        IWebElement Parcelclicktax = driver.FindElement(By.XPath("//*[@id='testdefault']/table/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[2]/tbody/tr/td/table/tbody"));
                        IList<IWebElement> parcelclickrow = Parcelclicktax.FindElements(By.TagName("tr"));
                        IList<IWebElement> Pareclclicktd;
                        foreach (IWebElement Percelclick in parcelclickrow)
                        {
                            Pareclclicktd = Percelclick.FindElements(By.TagName("td"));
                            if (Pareclclicktd.Count >19)
                            {
                                accountnumber = Pareclclicktd[7].Text;
                                string[] accountarray = accountnumber.Split('-');
                                //string yeartax = accountarray[1];
                                if (accountarray[0].Substring(3) == parcel_number & accountarray[1] == Convert.ToString(cuttentyearr))
                                {
                                    IWebElement parceltaxclick = Pareclclicktd[8];
                                    ByVisibleElement(parceltaxclick);
                                    gc.CreatePdf(orderNumber, parcel_number.Replace("/", ""), "Tax Deatil Before click" + cuttentyearr, driver, "NC", "Wake");
                                    parceltaxclick.Click();
                                    Thread.Sleep(3000);
                                    gc.CreatePdf(orderNumber, parcel_number.Replace("/", ""), "Tax Deatil" + cuttentyearr, driver, "NC", "Wake");
                                    Taxdetail(orderNumber, parcel_number, cuttentyearr);
                                    gc.CreatePdf(orderNumber, parcel_number.Replace("/", ""), "Rerutn Main Page" + cuttentyearr, driver, "NC", "Wake");
                                    cuttentyearr--;
                                }
                            }
                        }
                    }
                    catch { }
                    try
                    {
                        IWebElement Parcelclicktax = driver.FindElement(By.XPath("//*[@id='testdefault']/table/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[2]/tbody/tr/td/table/tbody"));
                        IList<IWebElement> parcelclickrow = Parcelclicktax.FindElements(By.TagName("tr"));
                        IList<IWebElement> Pareclclicktd;
                        foreach (IWebElement Percelclick in parcelclickrow)
                        {
                            Pareclclicktd = Percelclick.FindElements(By.TagName("td"));
                            if (Pareclclicktd.Count > 19)
                            {
                                accountnumber = Pareclclicktd[7].Text;
                                string[] accountarray = accountnumber.Split('-');
                                //string yeartax = accountarray[1];
                                if (accountarray[0].Substring(3) == parcel_number & accountarray[1] == Convert.ToString(cuttentyearr))
                                {
                                    IWebElement parceltaxclick = Pareclclicktd[8];
                                    ByVisibleElement(parceltaxclick);
                                    gc.CreatePdf(orderNumber, parcel_number.Replace("/", ""), "Tax Deatil Before click" + cuttentyearr, driver, "NC", "Wake");
                                    //IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                                    //js.ExecuteScript("arguments[0].click();", parceltaxclick);
                                    parceltaxclick.Click();
                                    Thread.Sleep(2000);
                                    Taxdetail(orderNumber, parcel_number, cuttentyearr);
                                    gc.CreatePdf(orderNumber, parcel_number.Replace("/", ""), "Tax Deatil" + cuttentyearr, driver, "NC", "Wake");
                                    cuttentyearr--;
                                }
                            }
                        }
                    }
                    catch { }
                    try
                    {
                        IWebElement Parcelclicktax = driver.FindElement(By.XPath("//*[@id='testdefault']/table/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[2]/tbody/tr/td/table/tbody"));
                        IList<IWebElement> parcelclickrow = Parcelclicktax.FindElements(By.TagName("tr"));
                        IList<IWebElement> Pareclclicktd;
                        foreach (IWebElement Percelclick in parcelclickrow)
                        {
                            Pareclclicktd = Percelclick.FindElements(By.TagName("td"));
                            if (Pareclclicktd.Count > 19)
                            {
                                accountnumber = Pareclclicktd[7].Text;
                                string[] accountarray = accountnumber.Split('-');
                                //string yeartax = accountarray[1];
                                if (accountarray[0].Substring(3) == parcel_number & accountarray[1] == Convert.ToString(cuttentyearr))
                                {
                                    IWebElement parceltaxclick = Pareclclicktd[8];
                                    parceltaxclick.Click();
                                    Thread.Sleep(2000);
                                    Taxdetail(orderNumber, parcel_number, cuttentyearr);
                                    gc.CreatePdf(orderNumber, parcel_number.Replace("/", ""), "Tax Deatil" + cuttentyearr, driver, "NC", "Wake");
                                    cuttentyearr--;
                                }
                            }
                        }
                    }
                    catch { }
                    driver.Quit();
                    gc.mergpdf(orderNumber, "NC", "Wake");
                    return "Data Inserted Successfully";

                }
                catch (Exception ex)
                {
                    driver.Quit();
                    throw ex;
                }
            }
        }
        public string SecondMultiparcel(string orderNumber, string Number, string direction, string Stname, string Sttype)
        {
            IWebElement SecondmultiparcelTable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td[1]/table/tbody"));
            IList<IWebElement> SecondMultiparcelrow = SecondmultiparcelTable.FindElements(By.TagName("tr"));
            IList<IWebElement> SecondMultiTD;
            foreach (IWebElement SecondMulti in SecondMultiparcelrow)
            {
                SecondMultiTD = SecondMulti.FindElements(By.TagName("td"));

                if (!SecondMulti.Text.Contains("Street Name"))
                {
                    IWebElement Checkbok = SecondMultiTD[0];
                    Checkbok.Click();
                    break;
                }
            }
            try
            {
                driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[2]/div/input")).Click();
                Thread.Sleep(2000);
            }
            catch { }
            try
            {
                string Nodata = driver.FindElement(By.XPath("/html/body/div[1]")).Text;
                if (Nodata.Contains("No Matching"))
                {
                    HttpContext.Current.Session["Zero_WakeNC"] = "Zero";
                    gc.CreatePdf_WOP(orderNumber, "ZeroSearch", driver, "NC", "Wake");
                    driver.Quit();
                    return "No Data Found";
                }
            }
            catch { }
            int Max = 0;
            string parcelMulti = "";
            IWebElement MultiAddressTable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table/tbody"));
            IList<IWebElement> MultiAddressrow = MultiAddressTable.FindElements(By.TagName("tr"));
            IList<IWebElement> MultiAddressid;
            foreach (IWebElement Multiaddress in MultiAddressrow)
            {
                MultiAddressid = Multiaddress.FindElements(By.TagName("td"));
                if (MultiAddressid.Count > 1 && !Multiaddress.Text.Contains("Line") && MultiAddressid[2].Text.Contains(Number) && MultiAddressid[5].Text.Contains(Stname.ToUpper()) && MultiAddressid[6].Text.Contains(Sttype) && MultiAddressid[1].Text != parcelMulti)
                {
                    MultiParcel = MultiAddressid[1].FindElement(By.TagName("a"));
                    Multihref = MultiParcel.GetAttribute("href");
                    parcelMulti = MultiAddressid[1].Text;
                    string Streetno = MultiAddressid[2].Text;
                    string StreetMisc = MultiAddressid[3].Text;
                    string streetna = MultiAddressid[5].Text;
                    string streetTY = MultiAddressid[6].Text;
                    string OwnerMulti = MultiAddressid[9].Text;
                    string Owenerresult = Streetno + " " + StreetMisc + " " + streetna + " " + streetTY + "~" + OwnerMulti;
                    gc.insert_date(orderNumber, parcelMulti, 1554, Owenerresult, 1, DateTime.Now);
                    Max++;
                }
            }
            if (Max == 1)
            {
                driver.Navigate().GoToUrl(Multihref);
                Thread.Sleep(2000);
                HttpContext.Current.Session["Single_WakeNC"] = "Yes";
            }
            if (Max > 1 && Max < 26)
            {
                HttpContext.Current.Session["multiparcel_WakeNC"] = "Yes";
                gc.CreatePdf_WOP(orderNumber, "MultyAddressSearch", driver, "NC", "Wake");
                driver.Quit();
                return "MultiParcel";
            }
            if (Max > 25)
            {
                HttpContext.Current.Session["multiParcel_WakeNC_Multicount"] = "Maximum";
                gc.CreatePdf_WOP(orderNumber, "MultyAddressSearch", driver, "NC", "Wake");
                driver.Quit();
                return "Maximum";
            }
            return "";
        }
        public void Taxdetail(string orderNumber, string parcel_number, int cuttentyearr)
        {
            string Accuntno = driver.FindElement(By.XPath("//*[@id='Table23']/tbody/tr/td[2]")).Text;
            string[] taxyearsplit = Accuntno.Split('-');
            string taxyear = taxyearsplit[1].Trim();
            amck.TaxYear = taxyear;
            string ownernametax = driver.FindElement(By.XPath("//*[@id='Table22']/tbody")).Text;
            string Billdate = driver.FindElement(By.XPath("//*[@id='Table24']/tbody/tr[1]/td[2]")).Text;
            string duedate = driver.FindElement(By.XPath("//*[@id='Table24']/tbody/tr[2]/td[2]")).Text;
            string intrest = driver.FindElement(By.XPath("//*[@id='Table24']/tbody/tr[3]/td[2]")).Text;
            string status1 = driver.FindElement(By.XPath("//*[@id='Table25']/tbody/tr[1]")).Text;
            string status = GlobalClass.After(status1, "Acct Status:");
            string Reid = driver.FindElement(By.XPath("//*[@id='Table1']/tbody/tr[4]/td[1]/table/tbody/tr/td[2]")).Text;
            amck.TaxId = Reid;
            string Locationtax = driver.FindElement(By.XPath("//*[@id='Table1']/tbody/tr[3]/td[1]/table/tbody/tr[2]/td[2]")).Text;
            string Municipality = driver.FindElement(By.XPath("//*[@id='Table1']/tbody/tr[3]/td[2]/table/tbody/tr[1]/td[2]")).Text;
            string FireDistrict = driver.FindElement(By.XPath("//*[@id='Table1']/tbody/tr[3]/td[2]/table/tbody/tr[2]/td[2]")).Text;
            string SpecialDist = driver.FindElement(By.XPath("//*[@id='Table1']/tbody/tr[3]/td[2]/table/tbody/tr[3]/td[2]")).Text;
            string Pdate = driver.FindElement(By.XPath("//*[@id='Table36']/tbody")).Text;
            string paiddate = GlobalClass.After(Pdate, "Paid in Full");
            string tax_inforesult = Accuntno + "~" + ownernametax + "~" + Billdate + "~" + duedate + "~" + intrest + "~" + status + "~" + Reid + "~" + Locationtax + "~" + Taxing_Authority + "~" + paiddate + "~" + FireDistrict + "~" + SpecialDist;
            gc.insert_date(orderNumber, parcel_number, 1550, tax_inforesult, 1, DateTime.Now);
            gc.CreatePdf(orderNumber, parcel_number.Replace("/", ""), "Tax Property" + cuttentyearr, driver, "NC", "Wake");
            IWebElement PaymentAllocationTable = driver.FindElement(By.XPath("//*[@id='Table35']/tbody"));
            IList<IWebElement> PaymentAllocationrow = PaymentAllocationTable.FindElements(By.TagName("tr"));
            IList<IWebElement> paymentAllocationid;
            foreach (IWebElement paymentallocation in PaymentAllocationrow)
            {
                paymentAllocationid = paymentallocation.FindElements(By.TagName("td"));
                if (paymentAllocationid.Count != 0)
                {
                    string paymentResult = taxyear + "~" + paymentAllocationid[0].Text + "~" + paymentAllocationid[1].Text;
                    gc.insert_date(orderNumber, parcel_number, 1551, paymentResult, 1, DateTime.Now);
                    if(paymentAllocationid[0].Text.Trim().Contains("Total Paid"))
                    {
                        amck.Instamountpaid1 = paymentAllocationid[1].Text;
                    }
                }
            }

            IWebElement PaymentAllocationTable1 = driver.FindElement(By.XPath("//*[@id='Table38']/tbody"));
            IList<IWebElement> PaymentAllocationrow1 = PaymentAllocationTable1.FindElements(By.TagName("tr"));
            IList<IWebElement> paymentAllocationid1;
            foreach (IWebElement paymentallocation1 in PaymentAllocationrow1)
            {
                paymentAllocationid1 = paymentallocation1.FindElements(By.TagName("td"));
                if (paymentAllocationid1.Count != 0)
                {
                    string paymentResult1 = taxyear + "~" + paymentAllocationid1[0].Text + "~" + paymentAllocationid1[1].Text;
                    gc.insert_date(orderNumber, parcel_number, 1551, paymentResult1, 1, DateTime.Now);
                    if (paymentAllocationid1[0].Text.Trim().Contains("Interest Due"))
                    {
                        string Inrestamt = paymentAllocationid1[1].Text;
                        if(Inrestamt.Trim()=="0.00")
                        {
                            amck.IsDelinquent = "NO";
                        }
                        else
                        {
                            amck.IsDelinquent = "Yes";
                        }
                    }
                    if(paymentAllocationid1[0].Text.Trim().Contains("Total Due"))
                    {
                        string Dueamt= paymentAllocationid1[1].Text;
                        if(Dueamt.Trim()=="0.00")
                        {
                            amck.InstPaidDue1 = "Paid";
                        }
                        else
                        {
                            amck.InstPaidDue1 = "Due";
                        }
                    }
                }
            }

            IWebElement Taxunitstable = driver.FindElement(By.XPath("//*[@id='Table34']/tbody"));
            IList<IWebElement> TaxunitsRow = Taxunitstable.FindElements(By.TagName("tr"));
            IList<IWebElement> TaxunitsTD;
            foreach (IWebElement Taxunits in TaxunitsRow)
            {
                TaxunitsTD = Taxunits.FindElements(By.TagName("td"));
                if (TaxunitsTD.Count == 4)
                {
                    string TaxunitsResult = taxyear + "~" + TaxunitsTD[0].Text + "~" + TaxunitsTD[1].Text + "~" + TaxunitsTD[2].Text + "~" + TaxunitsTD[3].Text;
                    gc.insert_date(orderNumber, parcel_number, 1552, TaxunitsResult, 1, DateTime.Now);
                }
                if (TaxunitsTD.Count == 3)
                {
                    string TaxunitsResult = taxyear + "~" + TaxunitsTD[0].Text + "~" + " " + "~" + TaxunitsTD[1].Text + "~" + TaxunitsTD[2].Text;
                    gc.insert_date(orderNumber, parcel_number, 1552, TaxunitsResult, 1, DateTime.Now);
                    if(TaxunitsTD[0].Text.Trim().Contains("Total Billed"))
                    {
                        amck.Instamount1 = TaxunitsTD[1].Text;
                    }
                }
            }
            if (i == 0)
            {
                gc.InsertAmrockTax(orderNumber, amck.TaxId, amck.Instamount1, amck.Instamount2, amck.Instamount3, amck.Instamount4, amck.Instamountpaid1, amck.Instamountpaid2, amck.Instamountpaid3, amck.Instamountpaid4, amck.InstPaidDue1, amck.InstPaidDue2, amck.instPaidDue3, amck.instPaidDue4, amck.IsDelinquent);
                i++;
            }
            ByVisibleElement(driver.FindElement(By.XPath("//*[@id='testdefault']/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[1]/td")));
            gc.CreatePdf(orderNumber, parcel_number.Replace("/", ""), "Tax Property Last" + cuttentyearr, driver, "NC", "Wake");
            IWebElement Returnclick = driver.FindElement(By.XPath("//*[@id='testdefault']/table/tbody/tr[1]/td/table/tbody/tr[2]/td/table/tbody/tr/td[2]/a[3]"));
            Returnclick.Click();
            Thread.Sleep(2000);
            try
            {
                IWebElement Nopaing = driver.FindElement(By.XPath("//*[@id='testdefault']/table/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[1]/tbody/tr[2]/td[4]/a"));
                Nopaing.Click();
                Thread.Sleep(2000);
            }
            catch { }
        }
        public void ByVisibleElement(IWebElement Element)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].scrollIntoView();", Element);
        }
    }
}