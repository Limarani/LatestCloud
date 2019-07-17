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
    public class Webdriver_QueensNY
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();
        string Yearbuild, parcel_number, Addressmax, parcelsplit1, parcelsplit2, DUE, Owner_Name, multiparcel = "";
        int value = 0; string[] splitarray;
        string[] ParcelSplit; IWebElement PropertyValidation;
        public string FTP_QueensNY(string streetno, string direction, string streetname, string streettype, string unitnumber, string ownernm, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new ChromeDriver();
            // driver = new PhantomJSDriver();
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    //  https://a836-pts-access.nyc.gov/care/search/commonsearch.aspx?mode=address
                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, "", ownernm, "", "NY", "Queens");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_QueensNY"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    driver.Navigate().GoToUrl("https://a836-pts-access.nyc.gov/care/search/commonsearch.aspx?mode=address");
                    driver.FindElement(By.Id("btAgree")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf_WOP(orderNumber, "Aggre After", driver, "NY", "Queens");
                    IWebElement PropertyInformation = driver.FindElement(By.Id("Select1"));
                    SelectElement PropertyInformationSelect = new SelectElement(PropertyInformation);
                    PropertyInformationSelect.SelectByIndex(4);
                    Thread.Sleep(2000);
                    gc.CreatePdf_WOP(orderNumber, "County Select", driver, "NY", "Queens");
                    if (searchType == "address")
                    {
                        if (streetname.Any(char.IsDigit))
                        {
                            streetname = Regex.Match(streetname, @"\d+").Value;
                        }
                        string Street = "";
                        if(direction!="")
                        {
                            Street = direction.Trim() + " " + streetname.Trim() + " " + streettype.Trim();
                        }
                        else
                        {
                            Street = streetname.Trim() + " " + streettype.Trim();
                        }
                        driver.FindElement(By.Id("inpNumber")).SendKeys(streetno);
                        driver.FindElement(By.Id("inpStreet")).SendKeys(Street);
                        driver.FindElement(By.Id("inpSuffix2")).SendKeys(unitnumber);
                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "NY", "Queens");
                        driver.FindElement(By.Id("btSearch")).Click();
                        Thread.Sleep(2000);
                        try
                        {
                            int Max = 0;
                            IWebElement Addresstable = driver.FindElement(By.XPath("//*[@id='searchResults']/tbody"));
                            IList<IWebElement> Addresrow = Addresstable.FindElements(By.TagName("tr"));
                            IList<IWebElement> AddressTD;
                            gc.CreatePdf_WOP(orderNumber, "Address After", driver, "NY", "Queens");
                            foreach (IWebElement AddressT in Addresrow)
                            {
                                AddressTD = AddressT.FindElements(By.TagName("td"));
                                if (AddressTD.Count > 1 && !AddressT.Text.Contains("BBL"))
                                {
                                    //IWebElement Parcellink = AddressTD[0].FindElement(By.TagName("a"));
                                    // Addresshrf = Parcellink.GetAttribute("href");
                                    string parcelno = AddressTD[0].Text;
                                    string OwnerName = AddressTD[1].Text;
                                    string Address = AddressTD[2].Text;
                                    string Addressresult = OwnerName + "~" + Address;
                                    Max++;
                                    gc.insert_date(orderNumber, parcelno, 628, Addressresult, 1, DateTime.Now);
                                }
                            }
                            if (Max > 1 && Max < 26)
                            {
                                HttpContext.Current.Session["multiparcel_QueensNY"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Max > 25)
                            {
                                HttpContext.Current.Session["multiParcel_QueensNY_Multicount"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }
                        //try
                        //{
                        //    string Nodata = driver.FindElement(By.XPath("//*[@id='frmMain']/table/tbody/tr/td/div/div/table[2]/tbody/tr/td/table/tbody/tr[3]/td/center/table[1]/tbody/tr[1]/td/div/p")).Text;
                        //    if(Nodata.Contains("Your search did not"))
                        //    {
                        //        HttpContext.Current.Session["Nodata_QueensNY"] = "Zero";
                        //        driver.Quit();
                        //        return "No Data Found";
                        //    }
                        //}
                        //catch { }
                    }
                    if (searchType == "parcel")
                    {
                        IWebElement parcellink = driver.FindElement(By.XPath("//*[@id='secondarytopmenu']/ul/li[2]")).FindElement(By.TagName("a"));
                        string parcelhref = parcellink.GetAttribute("href");
                        driver.Navigate().GoToUrl(parcelhref);
                        Thread.Sleep(2000);
                        IWebElement PropertyInformation1 = driver.FindElement(By.Id("inpParid"));
                        SelectElement PropertyInformationSelect1 = new SelectElement(PropertyInformation1);
                        PropertyInformationSelect1.SelectByIndex(3);
                        // int a= Convert.ToInt32(parcelNumber.Trim().Count());
                        if (Convert.ToInt32(parcelNumber.Trim().Count()) == 10)
                        {
                            driver.FindElement(By.Id("inpTag")).SendKeys(parcelNumber.TrimStart('0').Substring(1, 5));
                            driver.FindElement(By.Id("inpStat")).SendKeys(parcelNumber.TrimStart('0').Substring(6));
                            driver.FindElement(By.Id("btSearch")).Click();
                            Thread.Sleep(2000);
                        }
                        if (parcelNumber.Contains("-"))
                        {
                            splitarray = parcelNumber.Split('-');
                            if (Convert.ToInt16(splitarray.Count()) == 3)
                            {
                                driver.FindElement(By.Id("inpTag")).SendKeys(splitarray[1].TrimStart('0'));
                                driver.FindElement(By.Id("inpStat")).SendKeys(splitarray[2].TrimStart('0'));
                                driver.FindElement(By.Id("btSearch")).Click();
                                Thread.Sleep(2000);
                            }
                            if (Convert.ToInt16(splitarray.Count()) == 2)
                            {
                                driver.FindElement(By.Id("inpTag")).SendKeys(splitarray[0].TrimStart('0'));
                                driver.FindElement(By.Id("inpStat")).SendKeys(splitarray[1].TrimStart('0'));
                                driver.FindElement(By.Id("btSearch")).Click();
                                Thread.Sleep(2000);
                            }
                        }
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.XPath("//*[@id='frmMain']/table/tbody/tr/td/div/div/table[2]"));
                        if (INodata.Text.Contains("search did not find any records"))
                        {
                            HttpContext.Current.Session["Nodata_QueensNY"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    string blocktaken = driver.FindElement(By.XPath("//*[@id='datalet_header_row']/td/table/tbody/tr[2]/td[2]")).Text;
                    string block = gc.Between(blocktaken, "Block:", "Lot:").Trim();
                    string lot = GlobalClass.After(blocktaken, "Lot:").Trim();
                    parcel_number = block + lot;
                    string ownername = driver.FindElement(By.XPath("//*[@id='Property Owner(s)']/tbody/tr[2]/td")).Text;
                    string propertdata = driver.FindElement(By.Id("Property Data")).Text;
                    string Tax_year = gc.Between(propertdata, "Tax Year", "Lot Grouping");
                    string Propertyaddress = gc.Between(propertdata, "Property Address", "Tax Class");
                    string taxclass = gc.Between(propertdata, "Tax Class", "Building Class");
                    string Buildingclass = gc.Between(propertdata, "Building Class", "Condo Development");
                    string propertyresult = ownername + "~" + Tax_year + "~" + Propertyaddress + "~" + taxclass + "~" + Buildingclass;
                    gc.CreatePdf(orderNumber, parcel_number, "Peoperty Detail", driver, "NY", "Queens");
                    gc.insert_date(orderNumber, parcel_number, 1468, propertyresult, 1, DateTime.Now);
                    //Assessment
                    IWebElement accounthistoryy = driver.FindElement(By.XPath("//*[@id='sidemenu']/li[3]")).FindElement(By.TagName("a"));
                    string Accounthref = accounthistoryy.GetAttribute("href");
                    driver.Navigate().GoToUrl(Accounthref);
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, parcel_number, "Account History", driver, "NY", "Queens");
                    try
                    {
                        string Assessment = driver.FindElement(By.XPath("//*[@id='Profile']/tbody")).Text;
                        string BuildingClass= gc.Between(Assessment, "Building Class", "Tax Class");
                        string TaxClass = gc.Between(Assessment, "Tax Class", "Unused SCRIE Credit");
                        string scriecredit = gc.Between(Assessment, "Unused SCRIE Credit", "Unused DRIE Credit");
                        string Direcredit = gc.Between(Assessment, "Unused DRIE Credit", "Refund Amount");
                        string refundamt = gc.Between(Assessment, "Refund Amount", "Overpayment amount");
                        string overpayment = GlobalClass.After(Assessment, "Overpayment amount");
                        string assessmentresult = BuildingClass+"~"+ TaxClass+"~"+scriecredit + "~" + Direcredit + "~" + refundamt + "~" + overpayment;
                        gc.insert_date(orderNumber, parcel_number, 1472, assessmentresult, 1, DateTime.Now);
                    }
                    catch { }
                    //tax History
                    try
                    {
                        IWebElement Accounthistoryhref = driver.FindElement(By.XPath("//*[@id='Account History Details']/tbody/tr[2]/td")).FindElement(By.TagName("a"));
                        string accountyhistoryhref = Accounthistoryhref.GetAttribute("href");
                        driver.Navigate().GoToUrl(accountyhistoryhref);
                        gc.CreatePdf(orderNumber, parcel_number, "Account History1", driver, "NY", "Queens");
                        IWebElement Accounthostorytable = driver.FindElement(By.Id("Account History Details"));
                        IList<IWebElement> Accountrow = Accounthostorytable.FindElements(By.TagName("tr"));
                        IList<IWebElement> accountyhistoryid;
                        foreach (IWebElement accounthistory in Accountrow)
                        {
                            accountyhistoryid = accounthistory.FindElements(By.TagName("td"));
                            if (accountyhistoryid.Count > 1 && !accounthistory.Text.Contains("Year"))
                            {
                                string Accounthistoryresult = accountyhistoryid[0].Text + "~" + accountyhistoryid[1].Text + "~" + accountyhistoryid[2].Text + "~" + accountyhistoryid[3].Text + "~" + accountyhistoryid[4].Text + "~" + accountyhistoryid[5].Text + "~" + accountyhistoryid[6].Text + "~" + accountyhistoryid[7].Text + "~" + accountyhistoryid[8].Text + "~" + accountyhistoryid[9].Text + "~" + accountyhistoryid[10].Text + "~" + accountyhistoryid[11].Text;
                                gc.insert_date(orderNumber, parcel_number, 1471, Accounthistoryresult, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }
                    IWebElement proowner = driver.FindElement(By.XPath("//*[@id='sidemenu']/li[6]")).FindElement(By.TagName("a"));
                    string ownerpro = proowner.GetAttribute("href");
                    driver.Navigate().GoToUrl(ownerpro);
                    gc.CreatePdf(orderNumber, parcel_number, "Ownerpro", driver, "NY", "Queens");
                    try
                    {
                        IWebElement Proownertable = driver.FindElement(By.Id("Exemptions"));
                        IList<IWebElement> Proownerrow = Proownertable.FindElements(By.TagName("tr"));
                        IList<IWebElement> Proownerid;
                        foreach (IWebElement Proowner in Proownerrow)
                        {
                            Proownerid = Proowner.FindElements(By.TagName("td"));
                            if (Proownerid.Count > 1 && !Proowner.Text.Contains("Year"))
                            {
                                string Proownerresult = Proownerid[0].Text + "~" + Proownerid[1].Text + "~" + Proownerid[3].Text + "~" + Proownerid[4].Text;
                                gc.insert_date(orderNumber, parcel_number, 1469, Proownerresult, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }
                    //Property Tax Bills
                    IWebElement propertytaxbill = driver.FindElement(By.XPath("//*[@id='sidemenu']/li[4]")).FindElement(By.TagName("a"));
                    string Propertyhref = propertytaxbill.GetAttribute("href");
                    driver.Navigate().GoToUrl(Propertyhref);
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, parcel_number, "Property Bill", driver, "NY", "Queens");
                    string currentlink = driver.CurrentWindowHandle;
                    int p = 1;
                    IWebElement PropertyBilltable = driver.FindElement(By.Id("Notices of Property Value"));
                    IList<IWebElement> PropertyBillrow = PropertyBilltable.FindElements(By.TagName("tr"));
                    IList<IWebElement> PropertyBillid;
                    foreach (IWebElement PropertyBill in PropertyBillrow)
                    {
                        PropertyBillid = PropertyBill.FindElements(By.TagName("td"));
                        if (PropertyBillid.Count != 0 && PropertyBillid[0].Text.Trim() != "")
                        {
                            if (p < 5)
                            {
                                IWebElement PropertyLink = PropertyBillid[1].FindElement(By.TagName("a"));
                                string Hrefpro = PropertyLink.GetAttribute("href");
                                gc.downloadfile(Hrefpro, orderNumber, parcel_number, "Property Taax Bill" + p, "NY", "Queens");
                                //PropertyLink.Click();
                                //Thread.Sleep(4000);
                                //driver.SwitchTo().Window(driver.WindowHandles.Last());
                                //Thread.Sleep(2000);
                                //gc.CreatePdf(orderNumber, parcel_number, "Property Taax Bill" + p, driver, "NY", "Queens");
                                //driver.Close();
                                //driver.SwitchTo().Window(currentlink);
                                p++;
                            }
                            if (p == 5)
                            {
                                break;
                            }
                        }
                    }
                    // Benefits Business
                    IWebElement Benefits = driver.FindElement(By.XPath("//*[@id='sidemenu']/li[7]")).FindElement(By.TagName("a"));
                    string Beniftshref = Benefits.GetAttribute("href");
                    driver.Navigate().GoToUrl(Beniftshref);
                    Thread.Sleep(1000);
                    gc.CreatePdf(orderNumber, parcel_number, "Benefits", driver, "NY", "Queens");
                    //Benefits Non gov
                    IWebElement Benefitsgov = driver.FindElement(By.XPath("//*[@id='sidemenu']/li[7]")).FindElement(By.TagName("a"));
                    string Beniftshrefgov = Benefitsgov.GetAttribute("href");
                    driver.Navigate().GoToUrl(Beniftshrefgov);
                    Thread.Sleep(1000);
                    gc.CreatePdf(orderNumber, parcel_number, "Benefits Gov", driver, "NY", "Queens");
                    //prior Year
                    IWebElement prioryear = driver.FindElement(By.XPath("//*[@id='sidemenu']/li[10]")).FindElement(By.TagName("a"));
                    string prioryearhref = prioryear.GetAttribute("href");
                    driver.Navigate().GoToUrl(prioryearhref);
                    Thread.Sleep(1000);
                    gc.CreatePdf(orderNumber, parcel_number, "prior year", driver, "NY", "Queens");
                    try
                    {
                        IWebElement prioryeartable = driver.FindElement(By.Id("Historical Market Values and Assessment Rolls"));
                        IList<IWebElement> prioryearrow = prioryeartable.FindElements(By.TagName("tr"));
                        IList<IWebElement> prioryearid;
                        foreach (IWebElement prioryearele in prioryearrow)
                        {
                            prioryearid = prioryearele.FindElements(By.TagName("td"));
                            if (prioryearele.Text.Contains("Final Assessment Roll"))
                            {
                                IWebElement prioryearlink = prioryearid[3].FindElement(By.TagName("a"));
                                string Priorhref = prioryearlink.GetAttribute("href");
                                gc.downloadfile(Priorhref, orderNumber, parcel_number, "Final Assessment Roll", "NY", "Queens");
                                // prioryearlink.Click();
                                // Thread.Sleep(9000);
                                //driver.SwitchTo().Window(driver.WindowHandles.Last());
                               // gc.CreatePdf(orderNumber, parcel_number, "Final Assessment Roll", driver, "NY", "Queens");
                                break;
                            }
                        }
                    }
                    catch { }
                    // currentyear++;
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "NY", "Queens", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();

                    gc.mergpdf(orderNumber, "NY", "Queens");
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
        public void delequenttax(string orderNumber, string parcel_number)
        {
            string strEffectiveDate = "";
            string currDate = DateTime.Now.ToString("MM/dd/yyyy");
            string dateChecking = DateTime.Now.ToString("MM") + "/15/" + DateTime.Now.ToString("yyyy");
            driver.Navigate().GoToUrl("http://nycserv.nyc.gov/NYCServWeb/NYCSERVMain");
            driver.FindElement(By.XPath("/html/body/form[1]/center/table[2]/tbody/tr[1]/td/table/tbody/tr/td[1]/center/table/tbody/tr[3]/td/table/tbody/tr[3]/td[2]/input")).SendKeys(Keys.Enter);
            Thread.Sleep(2000);

            try
            {
                IAlert alert = driver.SwitchTo().Alert();
                alert.Accept();
                Thread.Sleep(1000);
            }
            catch { }
            if (Convert.ToDateTime(currDate) > Convert.ToDateTime(dateChecking))
            {
                string nextEndOfMonth = "";
                if ((Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))) < 12))
                {
                    nextEndOfMonth = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM")) + 1), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")) + 1)).ToString("MM/dd/yyyy");

                }
                else
                {
                    int nxtYr = Convert.ToInt16(DateTime.Now.ToString("yyyy")) + 1;
                    nextEndOfMonth = nextEndOfMonth = new DateTime(nxtYr, 1, DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), 1)).ToString("MM/dd/yyyy");

                }
                strEffectiveDate = nextEndOfMonth;
            }
            else
            {
                string EndOfMonth = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")))).ToString("MM/dd/yyyy");
                strEffectiveDate = EndOfMonth;
            }
            string[] deluquinttacxdate = strEffectiveDate.Split('/');
            string month = deluquinttacxdate[0];
            string date = deluquinttacxdate[1];
            string year = deluquinttacxdate[2];
            IWebElement monthclick = driver.FindElement(By.XPath("/html/body/center/form/table[3]/tbody/tr/td/table/tbody/tr[4]/td[3]/select[1]"));
            SelectElement monthclick1 = new SelectElement(driver.FindElement(By.Name("INTEREST_THROUGH_MONTH")));
            monthclick1.SelectByValue(month);
            driver.FindElement(By.XPath("/html/body/center/form/table[3]/tbody/tr/td/table/tbody/tr[4]/td[3]/select[2]")).SendKeys(date);
            driver.FindElement(By.XPath("/html/body/center/form/table[3]/tbody/tr/td/table/tbody/tr[4]/td[3]/select[3]")).SendKeys(year);
            ParcelSplit = parcel_number.Split('-', '/');
            parcelsplit1 = ParcelSplit[1];
            parcelsplit2 = ParcelSplit[2];
            driver.FindElement(By.XPath("/html/body/center/form/table[1]/tbody/tr/td/table/tbody/tr[6]/td[3]/input")).SendKeys(parcelsplit1);
            driver.FindElement(By.XPath("/html/body/center/form/table[1]/tbody/tr/td/table/tbody/tr[7]/td[3]/input")).SendKeys(parcelsplit2);
            driver.FindElement(By.XPath("/html/body/center/form/table[1]/tbody/tr/td/table/tbody/tr[4]/td[2]/input[4]")).Click();
            try
            {
                IAlert alert = driver.SwitchTo().Alert();
                alert.Accept();
                Thread.Sleep(1000);
            }
            catch { }
            driver.FindElement(By.XPath("/html/body/center/form/table[1]/tbody/tr/td/table/tbody/tr[9]/td[4]/img")).Click();
            Thread.Sleep(2000);
            try
            {
                IAlert alert = driver.SwitchTo().Alert();
                alert.Accept();
                Thread.Sleep(1000);
            }
            catch { }
            gc.CreatePdf(orderNumber, parcel_number.Replace("/", ""), "Deliquent", driver, "NY", "Queens");
        }
    }
}