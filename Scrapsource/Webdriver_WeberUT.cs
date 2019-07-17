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
    public class Webdriver_WeberUT
    {
        IWebDriver driver;
        IWebElement Delinquenttable;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_WeberUT(string address, string parcelNumber, string searchType, string ownername, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string Parcel_number = "", Tax_Authority = "", yearbuilt = "", Nodeliquent = "", Addresshrf = "", TaxesDue = "", lotsize = "", Good_through_date = "";
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            string Inrest = "";
            driverService.HideCommandPromptWindow = true;
            // driver = new ChromeDriver();
            // driver = new PhantomJSDriver();
            if (searchType == "titleflex")
            {
                //string address1 = Streetno + " " + direction + " " + sname + " " + streettype + " " + unitnumber;
                gc.TitleFlexSearch(orderNumber, "", "", address, "UT", "Weber");

                if (HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes")
                {
                    driver.Quit();
                    return "MultiParcel";
                }
                parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                //parcelNumber = HttpContext.Current.Session["titleparcel"].ToString().Replace(".", "").Replace("-", "");
                searchType = "parcel";
                if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                {
                    HttpContext.Current.Session["Zero_Weber"] = "Zero";
                    driver.Quit();
                    return "No Data Found";
                }
            }
            //var option = new ChromeOptions();
            //option.AddArgument("No-Sandbox");
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    driver.Navigate().GoToUrl("http://www.webercountyutah.gov/Treasurer/contact.php");
                    string taxauthority = driver.FindElement(By.XPath("//*[@id='main']/div/div[2]/a[2]")).Text;
                    gc.CreatePdf_WOP(orderNumber, "Taxauthority ", driver, "UT", "Weber");
                    string office = " 801-399-8454";
                    Tax_Authority = taxauthority + " " + office;
                }
                catch { }
                try
                {
                    driver.Navigate().GoToUrl("http://www3.co.weber.ut.us/psearch/index.php");
                    if (searchType == "address")
                    {
                        driver.FindElement(By.Id("address")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "AddressBefore", driver, "UT", "Weber");
                        driver.FindElement(By.XPath("//*[@id='form3']/p/input[3]")).Click();
                        Thread.Sleep(5000);
                        try
                        {
                            string Nodata = driver.FindElement(By.XPath("/html/body/table/tbody/tr[6]/td/table/tbody/tr[2]/td/font")).Text;
                            if (Nodata.Contains("No Parcel Data"))
                            {
                                HttpContext.Current.Session["Zero_Weber"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }

                        }
                        catch { }
                        int Max = 0;
                        gc.CreatePdf_WOP(orderNumber, "Address After", driver, "UT", "Weber");
                        IWebElement Addresstable = driver.FindElement(By.XPath("/html/body/table/tbody/tr[6]/td/table/tbody"));
                        IList<IWebElement> Addresrow = Addresstable.FindElements(By.TagName("tr"));
                        IList<IWebElement> AddressTD;
                        foreach (IWebElement AddressT in Addresrow)
                        {
                            AddressTD = AddressT.FindElements(By.TagName("td"));
                            if (AddressTD.Count > 1 && AddressTD[2].Text.Contains(address.ToUpper()))
                            {
                                IWebElement Parcellink = AddressTD[0].FindElement(By.TagName("a"));
                                Addresshrf = Parcellink.GetAttribute("href");
                                string parcelno = AddressTD[0].Text;
                                string OwnerName = AddressTD[1].Text;
                                string Address = AddressTD[2].Text;
                                string Multiaddress = OwnerName + "~" + Address;
                                gc.insert_date(orderNumber, parcelno, 1852, Multiaddress, 1, DateTime.Now);
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
                            HttpContext.Current.Session["multiParcel_Weber"] = "Yes";
                            driver.Quit();
                            return "MultiParcel";
                        }
                        if (Max > 25)
                        {
                            HttpContext.Current.Session["multiParcel_Weber_Multicount"] = "Maximum";
                            driver.Quit();
                            return "Maximum";
                        }
                        if (Max == 0)
                        {
                            HttpContext.Current.Session["Zero_Weber"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.Id("partial_serial")).SendKeys(parcelNumber.Replace("-", ""));
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "UT", "Weber");
                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[6]/td[1]/table/tbody/tr/td/form/input[2]")).Click();
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search After", driver, "UT", "Weber");
                        IWebElement Parcelele = driver.FindElement(By.XPath("/html/body/table/tbody/tr[6]/td/table/tbody/tr[3]/td[1]")).FindElement(By.TagName("a"));
                        string parcelhref = Parcelele.GetAttribute("href");
                        driver.Navigate().GoToUrl(parcelhref);
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Click", driver, "UT", "Weber");
                    }
                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("owner")).SendKeys(ownername);
                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[6]/td[3]/table/tbody/tr/td/form/center/input[3]")).Click();
                        Thread.Sleep(4000);
                        int Max = 0;
                        gc.CreatePdf_WOP(orderNumber, "Address After", driver, "UT", "Weber");
                        IWebElement Addresstable = driver.FindElement(By.XPath("/html/body/table/tbody/tr[6]/td/table/tbody"));
                        IList<IWebElement> Addresrow = Addresstable.FindElements(By.TagName("tr"));
                        IList<IWebElement> AddressTD;
                        foreach (IWebElement AddressT in Addresrow)
                        {
                            AddressTD = AddressT.FindElements(By.TagName("td"));
                            if (AddressTD.Count > 1 && !AddressT.Text.Contains("Parcel #") && !AddressT.Text.Contains("Search Results"))
                            {
                                IWebElement Parcellink = AddressTD[0].FindElement(By.TagName("a"));
                                Addresshrf = Parcellink.GetAttribute("href");
                                string parcelno = AddressTD[0].Text;
                                string OwnerName = AddressTD[1].Text;
                                string Address = AddressTD[2].Text;
                                string Multiaddress = OwnerName + "~" + Address;
                                gc.insert_date(orderNumber, parcelno, 1852, Multiaddress, 1, DateTime.Now);
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
                            HttpContext.Current.Session["multiParcel_Weber"] = "Yes";
                            driver.Quit();
                            return "MultiParcel";
                        }
                        if (Max > 25)
                        {
                            HttpContext.Current.Session["multiParcel_Weber_Multicount"] = "Maximum";
                            driver.Quit();
                            return "Maximum";
                        }
                        if (Max == 0)
                        {
                            HttpContext.Current.Session["Zero_Weber"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    Parcel_number = driver.FindElement(By.XPath("/html/body/table/tbody/tr[5]/td/table/tbody/tr[3]/td/table/tbody/tr[4]/td[1]")).Text.Replace("Parcel Nbr: ", "").Trim();
                    string market = driver.FindElement(By.XPath("/html/body/table/tbody/tr[5]/td/table/tbody/tr[5]/td/table/tbody/tr/td[2]/table/tbody/tr/td/table/tbody/tr[2]/td[1]")).Text;
                    string tax = driver.FindElement(By.XPath("/html/body/table/tbody/tr[5]/td/table/tbody/tr[5]/td/table/tbody/tr/td[2]/table/tbody/tr/td/table/tbody/tr[2]/td[2]")).Text;
                    string rate = driver.FindElement(By.XPath("/html/body/table/tbody/tr[5]/td/table/tbody/tr[5]/td/table/tbody/tr/td[2]/table/tbody/tr/td/table/tbody/tr[2]/td[4]")).Text;
                    string TaxArea1 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[5]/td/table/tbody/tr[3]/td/table/tbody/tr[4]/td[2]")).Text;
                    string taxarea = GlobalClass.After(TaxArea1, "Tax Area:").Trim();
                    string Netassessment = driver.FindElement(By.XPath("/html/body/table/tbody/tr[5]/td/table/tbody/tr[5]/td/table/tbody/tr/td[2]/table/tbody/tr/td/table/tbody/tr[4]/td[3]")).Text;
                    string totaldirect = driver.FindElement(By.XPath("/html/body/table/tbody/tr[5]/td/table/tbody/tr[5]/td/table/tbody/tr/td[2]/table/tbody/tr/td/table/tbody/tr[5]/td[3]")).Text;
                    string PenaltyCharge = driver.FindElement(By.XPath("/html/body/table/tbody/tr[5]/td/table/tbody/tr[5]/td/table/tbody/tr/td[2]/table/tbody/tr/td/table/tbody/tr[6]/td[3]")).Text;
                    string subtotal = driver.FindElement(By.XPath("/html/body/table/tbody/tr[5]/td/table/tbody/tr[5]/td/table/tbody/tr/td[2]/table/tbody/tr/td/table/tbody/tr[7]/td[3]")).Text;
                    string Totalpayment = driver.FindElement(By.XPath("/html/body/table/tbody/tr[5]/td/table/tbody/tr[5]/td/table/tbody/tr/td[2]/table/tbody/tr/td/table/tbody/tr[9]/td[3]")).Text;
                    string Balance = driver.FindElement(By.XPath("/html/body/table/tbody/tr[5]/td/table/tbody/tr[5]/td/table/tbody/tr/td[2]/table/tbody/tr/td/table/tbody/tr[10]/td[3]")).Text;
                    string type = driver.FindElement(By.XPath("/html/body/table/tbody/tr[5]/td/table/tbody/tr[10]/td")).Text;
                    string Discription = driver.FindElement(By.XPath("/html/body/table/tbody/tr[5]/td/table/tbody/tr[10]/td")).Text;
                    string amount = driver.FindElement(By.XPath("/html/body/table/tbody/tr[5]/td/table/tbody/tr[10]/td")).Text;
                    string Status = driver.FindElement(By.XPath("/html/body/table/tbody/tr[5]/td/table/tbody/tr[10]/td")).Text;
                    string loancompany1 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[5]/td/table/tbody/tr[5]/td/table/tbody/tr/td[1]/p")).Text;
                    string Loancompany = GlobalClass.After(loancompany1, "Loan Company:");
                    gc.CreatePdf(orderNumber, Parcel_number, "Property", driver, "UT", "Weber");
                    driver.FindElement(By.LinkText("Ownership Info")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "owner Name", driver, "UT", "Weber");

                    string owner = driver.FindElement(By.XPath("/html/body/table/tbody/tr[5]/td/table/tbody/tr[6]/td[1]/table/tbody/tr[1]/td[2]")).Text;
                    string proaddress = driver.FindElement(By.XPath("/html/body/table/tbody/tr[5]/td/table/tbody/tr[6]/td[1]/table/tbody/tr[3]/td[2]")).Text;
                    string Mailingaddress = driver.FindElement(By.XPath("/html/body/table/tbody/tr[5]/td/table/tbody/tr[6]/td[1]/table/tbody/tr[5]/td[2]")).Text;
                    string Taxunit = driver.FindElement(By.XPath("/html/body/table/tbody/tr[5]/td/table/tbody/tr[6]/td[1]/table/tbody/tr[7]/td[2]")).Text;
                    string priorParcel = driver.FindElement(By.XPath("/html/body/table/tbody/tr[5]/td/table/tbody/tr[9]/td/table/tbody/tr[7]/td")).Text;
                    string Legal = driver.FindElement(By.XPath("/html/body/table/tbody/tr[5]/td/table/tbody/tr[9]/td/table/tbody/tr[9]/td/table/tbody/tr")).Text;
                    driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/div/table/tbody/tr/td[4]/div/a")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Property Characteristics", driver, "UT", "Weber");
                    string Propertytype = driver.FindElement(By.XPath("/html/body/table/tbody/tr[6]/td/table/tbody/tr[1]/td[2]")).Text;
                    try
                    {
                        yearbuilt = driver.FindElement(By.XPath("/html/body/table/tbody/tr[6]/td/table/tbody/tr[12]/td[2]")).Text;
                    }
                    catch { }
                    try
                    {
                        lotsize = driver.FindElement(By.XPath("/html/body/table/tbody/tr[6]/td/table/tbody/tr[13]/td[2]")).Text;
                    }
                    catch { }
                    string Propertydetail = owner + "~" + proaddress + "~" + Mailingaddress + "~" + Taxunit + "~" + priorParcel + "~" + Legal + "~" + Propertytype + "~" + yearbuilt + "~" + lotsize;
                    gc.insert_date(orderNumber, Parcel_number, 1841, Propertydetail, 1, DateTime.Now);
                    string Assessment = taxarea + "~" + Loancompany + "~" + market + "~" + tax + "~" + rate + "~" + Netassessment + "~" + totaldirect + "~" + PenaltyCharge + "~" + subtotal + "~" + Totalpayment + "~" + Balance + "~" + type + "~" + Discription + "~" + amount + "~" + Status + "~" + Tax_Authority;
                    gc.insert_date(orderNumber, Parcel_number, 1842, Assessment, 1, DateTime.Now);
                    //Tax History
                    driver.FindElement(By.LinkText("Tax History")).Click();
                    Thread.Sleep(2000);
                    int Z = 2;
                    IList<IWebElement> yearlinkcount = driver.FindElement(By.XPath("/html/body/table/tbody/tr[7]/td/table/tbody/tr[2]/td[1]/table/tbody")).FindElements(By.TagName("a"));
                    for (int i = 1; i <= yearlinkcount.Count(); i++)
                    {
                        // /html/body/table/tbody/tr[7]/td/table/tbody/tr[2]/td[1]/table/tbody/tr[4]/td[1]
                        string Taxyear = driver.FindElement(By.XPath("/html/body/table/tbody/tr[7]/td/table/tbody/tr[2]/td[1]/table/tbody/tr[" + Z + "]/td[1]/a")).Text;
                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[7]/td/table/tbody/tr[2]/td[1]/table/tbody/tr[" + Z + "]/td[1]/a")).Click();
                        //string Nameget= Nametax.GetAttribute("name");
                        Thread.Sleep(2000);
                        //Propert Cahrges
                        if (i == 1)
                        {
                            IWebElement Propertytable = driver.FindElement(By.XPath("//*[@id='charge_2019']/table/tbody"));
                            IList<IWebElement> Propertyrow = Propertytable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Propertyid;
                            foreach (IWebElement Property in Propertyrow)
                            {
                                Propertyid = Property.FindElements(By.TagName("td"));
                                if (Propertyid.Count > 1 && Property.Text.Trim() != "")
                                {
                                    string Propertyresult = Taxyear + "~"+Propertyid[1].Text + "~" + Propertyid[2].Text;
                                    gc.insert_date(orderNumber, Parcel_number, 1843, Propertyresult, 1, DateTime.Now);
                                }
                            }
                        }
                        if (i == 2)
                        {
                            IWebElement Propertytable = driver.FindElement(By.XPath("//*[@id='charge_2018']/table/tbody"));
                            IList<IWebElement> Propertyrow = Propertytable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Propertyid;
                            foreach (IWebElement Property in Propertyrow)
                            {
                                Propertyid = Property.FindElements(By.TagName("td"));
                                if (Propertyid.Count > 1 && Property.Text.Trim() != "")
                                {
                                    string Propertyresult = Taxyear + "~" + Propertyid[1].Text + "~" + Propertyid[2].Text;
                                    gc.insert_date(orderNumber, Parcel_number, 1843, Propertyresult, 1, DateTime.Now);
                                }
                            }
                        }
                        Z += 2;
                    }
                    //Property Values
                    driver.FindElement(By.XPath("/html/body/table/tbody/tr[9]/td/table[1]/tbody/tr/td/div/a[2]")).Click();
                    Thread.Sleep(5000);
                    IWebElement Propertyvaluestable = driver.FindElement(By.XPath("/html/body/table/tbody/tr[9]/td/table[2]/tbody"));
                    IList<IWebElement> Propertyvaluesrow = Propertyvaluestable.FindElements(By.TagName("tr"));
                    IList<IWebElement> Propertyvaluesid;
                    foreach (IWebElement Propertyvalues in Propertyvaluesrow)
                    {
                        Propertyvaluesid = Propertyvalues.FindElements(By.TagName("td"));
                        if (Propertyvaluesid.Count == 4 && !Propertyvalues.Text.Contains("Year") && Propertyvaluesid[0].Text.Trim() != "")
                        {
                            string Propertyvaluesresult = Propertyvaluesid[0].Text + "~" + Propertyvaluesid[1].Text + "~" + Propertyvaluesid[2].Text + "~" + Propertyvaluesid[3].Text;
                            gc.insert_date(orderNumber, Parcel_number, 1844, Propertyvaluesresult, 1, DateTime.Now);
                        }
                    }
                    // Taxing Unit Areas
                    driver.FindElement(By.XPath("/html/body/table/tbody/tr[10]/td/table/tbody/tr[1]/td[1]/div/a[1]")).Click();
                    Thread.Sleep(2000);
                    IWebElement TaxingUnittable = driver.FindElement(By.XPath("/html/body/table/tbody/tr[10]/td/table/tbody/tr[2]/td[1]/table/tbody"));
                    IList<IWebElement> TaxingUnitrow = TaxingUnittable.FindElements(By.TagName("tr"));
                    IList<IWebElement> TaxingUnitid;
                    foreach (IWebElement TaxingUnit in TaxingUnitrow)
                    {
                        TaxingUnitid = TaxingUnit.FindElements(By.TagName("td"));
                        if (TaxingUnitid.Count == 4 && !TaxingUnit.Text.Contains("Year") && TaxingUnitid[0].Text.Trim() != "")
                        {
                            string TaxingUnitresult = TaxingUnitid[0].Text + "~" + TaxingUnitid[1].Text + "~" + TaxingUnitid[2].Text;
                            gc.insert_date(orderNumber, Parcel_number, 1845, TaxingUnitresult, 1, DateTime.Now);
                        }
                    }
                    //Payment
                    driver.FindElement(By.XPath("/html/body/table/tbody/tr[10]/td/table/tbody/tr[4]/td[1]/a[1]")).Click();
                    Thread.Sleep(2000);
                    IWebElement Paymenttable = driver.FindElement(By.XPath("/html/body/table/tbody/tr[10]/td/table/tbody/tr[4]/td[1]/table/tbody"));
                    IList<IWebElement> Paymentrow = Paymenttable.FindElements(By.TagName("tr"));
                    IList<IWebElement> Paymentid;
                    foreach (IWebElement Payment in Paymentrow)
                    {
                        Paymentid = Payment.FindElements(By.TagName("td"));
                        if (Paymentid.Count == 3 && !Payment.Text.Contains("Pay Date"))
                        {
                            string Paymentresult = Paymentid[0].Text + "~" + "" + "~" + Paymentid[1].Text + "~" + Paymentid[2].Text;
                            gc.insert_date(orderNumber, Parcel_number, 1846, Paymentresult, 1, DateTime.Now);
                        }
                        if (Paymentid.Count == 4 && !Payment.Text.Contains("Pay Date"))
                        {
                            string Paymentresult = Paymentid[0].Text + "~" + Paymentid[1].Text + "~" + Paymentid[2].Text + "~" + Paymentid[3].Text;
                            gc.insert_date(orderNumber, Parcel_number, 1846, Paymentresult, 1, DateTime.Now);
                        }
                    }
                    gc.CreatePdf(orderNumber, Parcel_number, "Tax History", driver, "UT", "Weber");
                    //Delinquent Tax
                    int D = 0;
                    driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/div/table/tbody/tr/td[5]/div/a")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Delinquent Tax", driver, "UT", "Weber");
                    string currentwindow = driver.CurrentWindowHandle;
                    try
                    {
                        Nodeliquent = driver.FindElement(By.XPath("/html/body/table/tbody/tr[6]/td/center")).Text;
                    }
                    catch { }
                    if (Nodeliquent.Contains("No Record of Past"))
                    {
                        string Delinquentresult = "" + "~" + Nodeliquent + "~" + "";
                        gc.insert_date(orderNumber, Parcel_number, 1847, Delinquentresult, 1, DateTime.Now);
                    }
                    else
                    {
                        try
                        {
                            Inrest = driver.FindElement(By.XPath("/html/body/table/tbody/tr[6]/td/center[1]/table/tbody/tr/td[2]/table/tbody/tr[2]/td/form/center")).Text;
                            IWebElement good_date = driver.FindElement(By.XPath("/html/body/table/tbody/tr[6]/td/center[1]/table/tbody/tr/td[2]/table/tbody/tr[2]/td/form/center/input[1]"));
                            Good_through_date = good_date.GetAttribute("value");
                            if (Good_through_date.Contains("Select A Date"))
                            {
                                Good_through_date = "-";
                            }

                            else
                            {
                                try
                                {
                                    if (Inrest.Contains("Interest Calculation Date"))
                                    {

                                        DateTime G_Date = Convert.ToDateTime(Good_through_date);
                                        string dateChecking = DateTime.Now.ToString("MM") + "/15/" + DateTime.Now.ToString("yyyy");

                                        if (G_Date < Convert.ToDateTime(dateChecking))
                                        {
                                            //end of the month
                                            Good_through_date = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")))).ToString("MM/dd/yyyy");

                                        }
                                        else if (G_Date > Convert.ToDateTime(dateChecking))
                                        {
                                            // nextEndOfMonth 
                                            if ((Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))) < 12))
                                            {
                                                Good_through_date = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM")) + 1), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")) + 1)).ToString("MM/dd/yyyy");
                                                IWebElement Inextmonth = driver.FindElement(By.XPath("//*[@id='ui-datepicker-div']/div/a[2]"));
                                                Inextmonth.Click();
                                            }
                                            else
                                            {
                                                int nxtYr = Convert.ToInt16(DateTime.Now.ToString("yyyy")) + 1;
                                                Good_through_date = new DateTime(nxtYr, 1, DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), 1)).ToString("dd/MM/yyyy");

                                            }
                                        }
                                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[6]/td/center[1]/table/tbody/tr/td[2]/table/tbody/tr[2]/td/form/center/input[1]")).Clear();
                                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[6]/td/center[1]/table/tbody/tr/td[2]/table/tbody/tr[2]/td/form/center/input[1]")).SendKeys("");
                                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[6]/td/center[1]/table/tbody/tr/td[2]/table/tbody/tr[2]/td/form/center/input[1]")).SendKeys(Good_through_date);
                                        IWebElement javaclick = driver.FindElement(By.XPath("/html/body/table/tbody/tr[6]/td/center[1]/table/tbody/tr/td[2]/table/tbody/tr[2]/td/form/center/input[2]"));
                                        IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                                        js1.ExecuteScript("arguments[0].click();", javaclick);
                                        Thread.Sleep(9000);
                                    }
                                }
                                catch { }
                            }
                        }
                        catch { }
                        try
                        {
                            IWebElement Delinquentyeartable = driver.FindElement(By.XPath("/html/body/table/tbody/tr[6]/td"));
                            IList<IWebElement> Delinquentyearrow = Delinquentyeartable.FindElements(By.TagName("table"));
                            IList<IWebElement> Delinquentyearid;
                            foreach (IWebElement Delinquentyear in Delinquentyearrow)
                            {
                                Delinquentyearid = Delinquentyear.FindElements(By.TagName("td"));
                                if (Delinquentyearid.Count == 7 && !Delinquentyear.Text.Contains("Year"))
                                {
                                    string Delinquentyearresult = Delinquentyearid[0].Text + "~" + Delinquentyearid[1].Text + "~" + Delinquentyearid[2].Text + "~" + Delinquentyearid[3].Text + "~" + Delinquentyearid[4].Text + "~" + Delinquentyearid[5].Text + "~" + Delinquentyearid[6].Text;
                                    gc.insert_date(orderNumber, Parcel_number, 1853, Delinquentyearresult, 1, DateTime.Now);
                                }
                            }
                        }
                        catch { }

                        //  /html/body/table/tbody/tr[7]/td/table/tbody
                        try
                        {
                            Delinquenttable = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td[3]/table/tbody"));
                        }
                        catch { }
                        try
                        {
                            Delinquenttable = driver.FindElement(By.XPath("/html/body/table/tbody/tr[7]/td/table/tbody"));
                        }
                        catch { }
                        IList<IWebElement> Delinquentrow = Delinquenttable.FindElements(By.TagName("tr"));
                        IList<IWebElement> Delinquentid;
                        foreach (IWebElement Delinquent in Delinquentrow)
                        {
                            Delinquentid = Delinquent.FindElements(By.TagName("td"));
                            if (Delinquentid.Count == 3 && !Delinquent.Text.Contains("Pay Date"))
                            {
                                string Delinquentresult = Delinquentid[0].Text + "~" + Delinquentid[1].Text + "~" + Delinquentid[2].Text;
                                gc.insert_date(orderNumber, Parcel_number, 1847, Delinquentresult, 1, DateTime.Now);
                            }
                            if (D < 3 && !Delinquent.Text.Contains("Pay Date"))
                            {
                                try
                                {
                                    IWebElement Delequentclick = Delinquentid[0].FindElement(By.TagName("a"));
                                    Delequentclick.Click();
                                    Thread.Sleep(2000);
                                    driver.SwitchTo().Window(driver.WindowHandles.Last());
                                    gc.CreatePdf(orderNumber, Parcel_number, "Delinquent" + D, driver, "UT", "Weber");
                                    driver.SwitchTo().Window(currentwindow);
                                    D++;
                                }
                                catch { }
                            }
                        }

                    }
                    driver.Quit();

                    gc.mergpdf(orderNumber, "UT", "Weber");
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