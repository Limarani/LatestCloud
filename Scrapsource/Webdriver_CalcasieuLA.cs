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
    public class Webdriver_CalcasieuLA
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();
        IWebElement DetailS; IWebElement propertytaxtable;
        public string FTP_Calcasieu(string streetno, string direction, string streetname, string city, string streettype, string unitnumber, string ownernm, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            string Parcel_number, TaxAuthority = "", HomesteadDetailsresult = "", TotalAmount = "", Date = "", Detail = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("http://www.calcasieuassessor.org/Search");
                    Thread.Sleep(3000);
                    driver.FindElement(By.XPath("//*[@id='termsOfUse']/div/div/button")).SendKeys(Keys.Enter);
                    Thread.Sleep(2000);

                    if (searchType == "titleflex")
                    {
                        string address = streetno + " " + streetname + " " + streettype + " " + unitnumber;
                        gc.TitleFlexSearch(orderNumber, "", ownernm, address.Trim(), "LA", "Calcasieu");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_CalcasieuLA"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        gc.CreatePdf_WOP_Chrome(orderNumber, "Address Start", driver, "LA", "Calcasieu");
                        driver.FindElement(By.XPath("//*[@id='ng-view']/div/form/input[2]")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.XPath("//*[@id='ng-view']/div/form/div[2]/div/input[1]")).SendKeys(streetno);
                        driver.FindElement(By.XPath("//*[@id='ng-view']/div/form/div[2]/div/input[2]")).SendKeys(streetname);
                        driver.FindElement(By.XPath("//*[@id='ng-view']/div/form/div[5]/button")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        try
                        {
                            IWebElement AddressMulti = driver.FindElement(By.XPath("//*[@id='ng-view']/div/table/tbody"));
                            IList<IWebElement> AddressMultirow = AddressMulti.FindElements(By.TagName("tr"));
                            IList<IWebElement> AddresMultiid;
                            foreach (IWebElement Address in AddressMultirow)
                            {
                                AddresMultiid = Address.FindElements(By.TagName("td"));
                                if (AddresMultiid.Count != 0)
                                {
                                    string parcel = AddresMultiid[0].Text;
                                    Detail = AddresMultiid[1].Text + "~" + AddresMultiid[2].Text;
                                    gc.insert_date(orderNumber, parcel, 706, Detail, 1, DateTime.Now);
                                }
                            }
                            if (AddressMultirow.Count != 0 && AddressMultirow.Count == 1)
                            {
                                driver.FindElement(By.XPath("//*[@id='ng-view']/div/table/tbody/tr/td[4]/a")).Click();
                                Thread.Sleep(2000);
                            }
                            if (AddressMultirow.Count != 0 && AddressMultirow.Count <= 25 && AddressMultirow.Count != 1)
                            {
                                HttpContext.Current.Session["multiParcel_Calcasieu"] = "Yes";
                                gc.CreatePdf_WOP(orderNumber, "Multiple Parcel", driver, "LA", "Calcasieu");
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (AddressMultirow.Count != 0 && AddressMultirow.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Calcasieu_Multicount"] = "Maximum";
                                gc.CreatePdf_WOP(orderNumber, "MultyAddressSearch", driver, "LA", "Calcasieu");

                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.XPath("//*[@id='ng-view']/div/form/input[1]")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Serach", driver, "LA", "Calcasieu");
                        driver.FindElement(By.XPath("//*[@id='ng-view']/div/form/input[6]")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Result", driver, "LA", "Calcasieu");
                        driver.FindElement(By.XPath("//*[@id='ng-view']/div/form/div[5]/button")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        //For Nodata Found
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='ng-view']/div/table/tbody/tr/td[4]/a")).Click();
                            Thread.Sleep(2000);
                        }
                        catch { }
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.XPath("//*[@id='ng-view']/div/div"));
                        if(INodata.Text.Contains("No results found"))
                        {
                            HttpContext.Current.Session["Nodata_CalcasieuLA"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    //propertydetail
                    //driver.FindElement(By.XPath("//*[@id='ng-view']/div/table/tbody/tr/td[4]/a")).Click();
                    //Thread.Sleep(2000);
                    driver.SwitchTo().Window(driver.WindowHandles.Last());
                    IWebElement Propertydetail = driver.FindElement(By.Id("parcelDetails"));
                    Parcel_number = gc.Between(Propertydetail.Text, "Parcel#", "Primary Owner").Trim();
                    gc.CreatePdf(orderNumber, Parcel_number, "Property Search Result", driver, "LA", "Calcasieu");
                    string PrimaryOwner = gc.Between(Propertydetail.Text, "Primary Owner", "Mailing Address").Trim();
                    string MailingAddress = gc.Between(Propertydetail.Text, "Mailing Address", "Ward").Trim();
                    string Ward = gc.Between(Propertydetail.Text, "Ward", "Type").Trim();
                    string Type = gc.Between(Propertydetail.Text, "Type", "Legal").Trim();
                    string Legal = driver.FindElement(By.XPath("//*[@id='parcelDetails']/div[6]/div/span[2]")).Text.Trim();
                    string PropertyAddress = gc.Between(Propertydetail.Text, "Physical Address", "Parcel Items").Trim();
                    string Homestead = driver.FindElement(By.XPath("//*[@id='parcelDetails']/div[9]/table/tbody/tr[1]/td[1]")).Text;
                    string Subdivision = driver.FindElement(By.XPath("//*[@id='parcelDetails']/div[10]/table/tbody/tr/td[1]")).Text;
                    string Propertyresult = PrimaryOwner + "~" + MailingAddress + "~" + Ward + "~" + Type + "~" + Legal + "~" + PropertyAddress + "~" + Homestead + "~" + Subdivision;
                    gc.insert_date(orderNumber, Parcel_number, 688, Propertyresult, 1, DateTime.Now);

                    //Assessment detail
                    IWebElement Assessmentdetailtable = driver.FindElement(By.XPath("//*[@id='parcelDetails']/div[8]/table/tbody"));
                    IList<IWebElement> Assessmentrow = Assessmentdetailtable.FindElements(By.TagName("tr"));
                    IList<IWebElement> Assessmentid;
                    foreach (IWebElement Assessment in Assessmentrow)
                    {
                        Assessmentid = Assessment.FindElements(By.TagName("td"));
                        if (Assessmentid.Count != 0)
                        {
                            string AssessmentValue = Assessmentid[0].Text + "~" + Assessmentid[1].Text + "~" + Assessmentid[3].Text + "~" + Assessmentid[4].Text;
                            gc.insert_date(orderNumber, Parcel_number, 689, AssessmentValue, 1, DateTime.Now);
                        }
                    }

                    IWebElement ParishMillagetable = driver.FindElement(By.XPath("//*[@id='parcelDetails']/div[11]/table/tbody"));
                    IList<IWebElement> ParishMillagerow = ParishMillagetable.FindElements(By.TagName("tr"));
                    IList<IWebElement> Parishmillageid;
                    foreach (IWebElement Parishmaillage in ParishMillagerow)
                    {
                        Parishmillageid = Parishmaillage.FindElements(By.TagName("td"));
                        if (Parishmillageid.Count != 0)
                        {
                            string Parish = Parishmillageid[0].Text + "~" + Parishmillageid[1].Text + "~" + Parishmillageid[2].Text + "~" + Parishmillageid[3].Text;
                            gc.insert_date(orderNumber, Parcel_number, 694, Parish, 1, DateTime.Now);
                        }
                    }
                    try
                    {
                        IWebElement citytaxtable = driver.FindElement(By.XPath("//*[@id='parcelDetails']/div[11]/table[2]/tbody"));
                        IList<IWebElement> citytaxrow = citytaxtable.FindElements(By.TagName("tr"));
                        IList<IWebElement> citytaxid;
                        foreach (IWebElement citytax in citytaxrow)
                        {
                            citytaxid = citytax.FindElements(By.TagName("td"));
                            if (citytaxid.Count != 0)
                            {
                                string citytaxresult = citytaxid[0].Text + "~" + citytaxid[1].Text + "~" + citytaxid[2].Text + "~" + citytaxid[3].Text;
                                gc.insert_date(orderNumber, Parcel_number, 735, citytaxresult, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }
                    string Property_Address = "", tax = "";
                    for (int i = 0; i < 3; i++)
                    {
                        driver.Navigate().GoToUrl("https://www.cpso.com/pay-taxes/");
                        try
                        {
                            TaxAuthority = driver.FindElement(By.Id("cc-matrix-3319575278")).Text;
                            gc.CreatePdf(orderNumber, Parcel_number, "Tax Authority", driver, "LA", "Calcasieu");
                        }
                        catch { }
                        try
                        {
                            driver.Navigate().GoToUrl("http://snstaxpayments.com/calcasieu");
                            driver.FindElement(By.Id("submit")).SendKeys(Keys.Enter);
                            Thread.Sleep(2000);
                            driver.FindElement(By.XPath("/html/body/div[1]/div[3]/form/div/div/div[1]/div[2]/label/input")).Click();
                            // gc.CreatePdf(orderNumber, Parcel_number, "Tax Search", driver, "LA", "Calcasieu");
                            driver.FindElement(By.Id("searchFor1")).SendKeys(Parcel_number);
                            gc.CreatePdf(orderNumber, Parcel_number, "Tax Search Result", driver, "LA", "Calcasieu");
                            //IWebElement Taxyeardropdown = driver.FindElement(By.Id("taxyear"));
                            SelectElement Taxyearselect = new SelectElement(driver.FindElement(By.Name("taxyear")));
                            Taxyearselect.SelectByIndex(i);
                            driver.FindElement(By.Id("searchButton")).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, Parcel_number, "Tax Result" + i, driver, "LA", "Calcasieu");
                            driver.FindElement(By.XPath("//*[@id='results']/table/tbody/tr/td[1]/button")).Click();
                            Thread.Sleep(2000);
                            driver.SwitchTo().Window(driver.WindowHandles.Last());
                            gc.CreatePdf(orderNumber, Parcel_number, "Tax Detailed Result" + i, driver, "LA", "Calcasieu");
                            Thread.Sleep(2000);
                            IWebElement Taxdetailtable = driver.FindElement(By.Id("details"));
                            string TaxNotice = gc.Between(Taxdetailtable.Text, "Tax Notice#", "Tax Year").Trim();
                            string TaxYear = gc.Between(Taxdetailtable.Text, "Tax Year", "Taxpayer").Trim();
                            string Taxpayer = gc.Between(Taxdetailtable.Text, "Taxpayer", "Taxes").Trim();
                            string Taxes = gc.Between(Taxdetailtable.Text, "Taxes", "Interest").Trim();
                            string Interest = gc.Between(Taxdetailtable.Text, "Interest", "Cost").Trim();
                            string Cost = gc.Between(Taxdetailtable.Text, "Cost", "Other").Trim();
                            string Other = gc.Between(Taxdetailtable.Text, "Other", "Paid").Trim();
                            string Paid = gc.Between(Taxdetailtable.Text, "Paid", "Balance").Trim();
                            string Balance = gc.Between(Taxdetailtable.Text, "Balance", "Legal").Trim();
                            string Legals = gc.Between(Taxdetailtable.Text, "Legal", "Parcels").Trim();

                            try
                            {

                                try
                                {
                                    propertytaxtable = driver.FindElement(By.XPath("//*[@id='details']/div[9]/table/tbody"));
                                }
                                catch
                                {
                                }
                                try
                                {
                                    propertytaxtable = driver.FindElement(By.XPath("//*[@id='details']/div[8]/table/tbody"));
                                }
                                catch { }
                                IList<IWebElement> propertytaxrow = propertytaxtable.FindElements(By.TagName("tr"));
                                IList<IWebElement> propertytaxid;
                                foreach (IWebElement property in propertytaxrow)
                                {
                                    propertytaxid = property.FindElements(By.TagName("td"));

                                    if (propertytaxid.Count != 0 && propertytaxid.Count == 3)
                                    {
                                        Property_Address = propertytaxid[1].Text;
                                        tax = propertytaxid[2].Text;
                                    }
                                }
                            }
                            catch { }
                            string taxresult = TaxNotice + "~" + TaxYear + "~" + Taxpayer + "~" + Property_Address + "~" + tax + "~" + Taxes + "~" + Interest + "~" + Cost + "~" + Other + "~" + Paid + "~" + Balance + "~" + Legals + "~" + TaxAuthority;
                            gc.insert_date(orderNumber, Parcel_number, 699, taxresult, 1, DateTime.Now);
                            driver.FindElement(By.XPath("//*[@id='details']/div[10]/h4")).Click();
                            Thread.Sleep(1000);
                            gc.CreatePdf(orderNumber, Parcel_number, "Tax Details Table1" + i, driver, "LA", "Calcasieu");
                            Thread.Sleep(2000);

                            if (HomesteadDetailsresult == "")
                            {
                                IList<IWebElement> tables = driver.FindElements(By.XPath("//*[@id='details']/div/table"));
                                int count = tables.Count;
                                foreach (IWebElement tab in tables)
                                {
                                    if (tab.Text.Contains("Description"))
                                    {

                                        IList<IWebElement> PaymentHistoryDrow = tab.FindElements(By.TagName("tr"));
                                        IList<IWebElement> PaymentHistoryDid;
                                        foreach (IWebElement PaymentHistory in PaymentHistoryDrow)
                                        {
                                            PaymentHistoryDid = PaymentHistory.FindElements(By.TagName("td"));
                                            {
                                                if (PaymentHistoryDid.Count != 0)
                                                {
                                                    if (PaymentHistoryDid.Count > 2)
                                                    {
                                                        Date = PaymentHistoryDid[0].Text;
                                                        string Amount1 = PaymentHistoryDid[2].Text;
                                                        TotalAmount = GlobalClass.Before(Amount1, "Description").Trim();

                                                    }
                                                    if (PaymentHistoryDid.Count == 2)
                                                    {
                                                        string Description = PaymentHistoryDid[0].Text;
                                                        string Amount = PaymentHistoryDid[1].Text;
                                                        string Paymenthistoryresult = TaxYear + "~" + Date + "~" + Description + "~" + Amount + "~" + TotalAmount;
                                                        gc.insert_date(orderNumber, Parcel_number, 702, Paymenthistoryresult, 1, DateTime.Now);
                                                        Date = "";
                                                        TotalAmount = "";
                                                    }

                                                }
                                            }
                                        }
                                    }

                                    if (tab.Text.Contains("Class"))
                                    {

                                        IList<IWebElement> HomesteadDetailsrow = tab.FindElements(By.TagName("tr"));
                                        IList<IWebElement> HomesteadDetailsid;
                                        //IList<IWebElement> HomesteadDetailsth;
                                        foreach (IWebElement HomesteadDetails in HomesteadDetailsrow)
                                        {

                                            //HomesteadDetailsth = HomesteadDetails.FindElements(By.TagName("th"));
                                            HomesteadDetailsid = HomesteadDetails.FindElements(By.TagName("td"));

                                            if (HomesteadDetailsid.Count != 0 && !HomesteadDetails.Text.Contains("Class"))
                                            {
                                                HomesteadDetailsresult = HomesteadDetailsid[0].Text + "~" + HomesteadDetailsid[1].Text + "~" + HomesteadDetailsid[2].Text + "~" + HomesteadDetailsid[3].Text;
                                                gc.insert_date(orderNumber, Parcel_number, 701, HomesteadDetailsresult, 1, DateTime.Now);
                                            }

                                        }
                                    }

                                }

                            }
                        }
                        catch { }
                    }



                    for (int i = 2; i < 5; i++)
                    {
                        try
                        {
                            driver.Navigate().GoToUrl("http://www.latax.state.la.us/Menu_ParishTaxRolls/TaxRolls.aspx");
                            IWebElement StateTaxdropdown = driver.FindElement(By.Id("ctl00_ContentPlaceHolderMain_ddParish"));
                            SelectElement StateTaxselect = new SelectElement(StateTaxdropdown);
                            StateTaxselect.SelectByValue("22019");
                            Thread.Sleep(2000);
                            IWebElement StateYeardropdown = driver.FindElement(By.Id("ctl00_ContentPlaceHolderMain_ddYear"));
                            SelectElement stateyearselect = new SelectElement(StateYeardropdown);
                            stateyearselect.SelectByIndex(i);
                            Thread.Sleep(2000);
                            driver.FindElement(By.Id("ctl00_ContentPlaceHolderMain_rbSearchField_1")).Click();
                            Thread.Sleep(2000);

                            driver.FindElement(By.Id("ctl00_ContentPlaceHolderMain_txtSearch")).SendKeys(Parcel_number);
                            driver.FindElement(By.Id("ctl00_ContentPlaceHolderMain_btnSubmitSearch")).SendKeys(Keys.Enter);
                            gc.CreatePdf(orderNumber, Parcel_number, "State Tax first" + i, driver, "LA", "Calcasieu");
                            driver.FindElement(By.LinkText("Generate Report")).Click();
                            Thread.Sleep(2000);

                            IWebElement Statepdfdownload = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolderMain_lblReport']/a[1]"));
                            string Statepdf = Statepdfdownload.GetAttribute("href");
                            Statepdfdownload.Click();
                            driver.SwitchTo().Window(driver.WindowHandles.Last());
                            Thread.Sleep(3000);
                            //gc.CreatePdf(orderNumber, Parcel_number, "State Tax" + i, driver, "LA", "Calcasieu");
                            gc.downloadfile(Statepdf, orderNumber, Parcel_number, "State Tax" + i, "LA", "Calcasieu");
                        }
                        catch { }
                    }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    //gc.insert_TakenTime(orderNumber, "LA", "Calcasieu", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    gc.mergpdf(orderNumber, "LA", "Calcasieu");

                    driver.Quit();
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