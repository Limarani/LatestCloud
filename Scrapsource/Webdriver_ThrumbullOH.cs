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
using System.Collections.ObjectModel;

namespace ScrapMaricopa.Scrapsource
{

    public class Webdriver_ThrumbullOH
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        List<string> link = new List<string>();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_ThrumbullOH(string streetno, string direction, string streettype, string streetname, string unitnumber, string ownernm, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            List<string> taxinformation = new List<string>();
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string Parcel_number = "", Tax_Authority = "", yearbuild = "", apprasiedresult = "", Addresshrf = "", Valuvationresult = "", Multiaddressadd = "", MailingAddress = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;

            // driver = new ChromeDriver()driver = new PhantomJSDriver();
            using (driver = new PhantomJSDriver())
            {
                StartTime = DateTime.Now.ToString("HH:mm:ss");
                try
                {
                    driver.Navigate().GoToUrl("http://property.co.trumbull.oh.us/Search.aspx");
                    if (searchType == "address")
                    {
                        driver.FindElement(By.LinkText("Address")).Click();
                        driver.FindElement(By.Id("ContentPlaceHolder1_Address_tbAddressNumber")).SendKeys(streetno);
                        driver.FindElement(By.Id("ContentPlaceHolder1_Address_tbAddressDirection")).SendKeys(direction);
                        driver.FindElement(By.Id("ContentPlaceHolder1_Address_tbAddressStreet")).SendKeys(streetname);
                        driver.FindElement(By.Id("ContentPlaceHolder1_Address_btnSearchAddress")).Click();
                        Thread.Sleep(2000);
                        try
                        {
                            int Max = 0;
                            IWebElement Multiparceladdress = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_gvSearchResults']/tbody"));
                            IList<IWebElement> Multiparcelrow = Multiparceladdress.FindElements(By.TagName("tr"));
                            IList<IWebElement> Multiparcelid;
                            gc.CreatePdf_WOP(orderNumber, "SearchAfter", driver, "OH", "Trumbull");
                            foreach (IWebElement multiparcel in Multiparcelrow)
                            {
                                Multiparcelid = multiparcel.FindElements(By.TagName("td"));
                                if (Multiparcelid.Count != 0)
                                {
                                    //IWebElement Address1 = Multiparcelid[0].FindElement(By.TagName("a"));
                                    //Addresshrf = Address1.GetAttribute("href");
                                    string Addressst = Multiparcelid[2].Text;
                                    string Owner = Multiparcelid[1].Text;
                                    string Pin = Multiparcelid[0].Text;
                                    string Multiparcel = Addressst + "~" + Owner;
                                    gc.insert_date(orderNumber, Pin, 1434, Multiparcel, 1, DateTime.Now);
                                    Max++;
                                }
                            }
                            //if (Max == 1)
                            //{
                            //    driver.Navigate().GoToUrl(Addresshrf);
                            //    Thread.Sleep(2000);
                            //}
                            if (Max > 1 && Max < 26)
                            {
                                HttpContext.Current.Session["multiParcel_Thumbull"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Max > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Thumbull_Multicount"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                            if (Max == 0)
                            {
                                HttpContext.Current.Session["Zero_Thumbull"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }

                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.LinkText("Parcel")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.Id("ContentPlaceHolder1_Parcel_tbParcelNumber")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Before", driver, "OH", "Trumbull");
                        driver.FindElement(By.Id("ContentPlaceHolder1_Parcel_btnSearchParcel")).Click();
                        Thread.Sleep(2000);
                    }
                    if (searchType == "ownername")
                    {
                        string lastname = "", firstname = "";
                        string[] Ownarsplit = ownernm.Split(' ');
                        try
                        {
                            lastname = Ownarsplit[1];
                        }
                        catch { }
                        try
                        {
                            firstname = Ownarsplit[0];
                        }
                        catch { }
                        driver.FindElement(By.Id("ContentPlaceHolder1_Owner_tbOwnerLastName")).SendKeys(firstname);
                        driver.FindElement(By.Id("ContentPlaceHolder1_Owner_tbOwnerFirstName")).SendKeys(lastname);
                        gc.CreatePdf_WOP(orderNumber, "Search Before", driver, "OH", "Trumbull");
                        driver.FindElement(By.Id("ContentPlaceHolder1_Owner_btnSearchOwner")).Click();
                        Thread.Sleep(2000);
                        try
                        {
                            int Max = 0;

                            IWebElement Multiparceladdress = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_gvSearchResults']/tbody"));
                            IList<IWebElement> Multiparcelrow = Multiparceladdress.FindElements(By.TagName("tr"));
                            IList<IWebElement> Multiparcelid;
                            gc.CreatePdf_WOP(orderNumber, "SearchAfter", driver, "OH", "Trumbull");
                            foreach (IWebElement multiparcel in Multiparcelrow)
                            {
                                Multiparcelid = multiparcel.FindElements(By.TagName("td"));
                                if (Multiparcelid.Count != 0)
                                {
                                    IWebElement Address1 = Multiparcelid[0].FindElement(By.TagName("a"));
                                    Addresshrf = Address1.GetAttribute("href");
                                    string Addressst = Address1.Text;
                                    string Owner = Multiparcelid[1].Text;
                                    string Pin = Multiparcelid[2].Text;
                                    string Multiparcel = Pin + "~" + Owner;
                                    gc.insert_date(orderNumber, Addressst, 1434, Multiparcel, 1, DateTime.Now);
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
                                HttpContext.Current.Session["multiParcel_Thumbull"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Max > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Thumbull_Multicount"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                            if (Max == 0)
                            {
                                HttpContext.Current.Session["Zero_Thumbull"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }

                        }
                        catch { }
                    }
                    try
                    {
                        string Nodatfound = driver.FindElement(By.Id("ContentPlaceHolder1_lblNumberOfResults")).Text;
                        if (Nodatfound.Contains("No results"))
                        {
                            HttpContext.Current.Session["Zero_Thumbull"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    //property detail

                    Parcel_number = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataProfile_ParcelLabel")).Text;
                    string Ownar = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataProfile_OwnerLabel")).Text;
                    string address = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataProfile_AddressLabel")).Text;
                    string mailname = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataMailingAddress_MailingAddressLine1Label")).Text;
                    string mailaddress = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataMailingAddress_MailingAddressLine2Label")).Text;
                    string mailcity = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataMailingAddress_MailingAddressLine3Label")).Text;
                    string mailingaddress = mailname + " " + mailaddress + " " + mailcity;
                    string city = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataGeographic_CityLabel")).Text;
                    string Township = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataGeographic_TownshipLabel")).Text;
                    string school = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataGeographic_SchoolDistrictLabel")).Text;
                    string Taxdistient = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataGeographic_TaxDistrictLabel")).Text;
                    string LegalDescription = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataLegal_LegalDescriptionLabel")).Text;
                    string LegalAcres = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataLegal_TotalAcresLabel")).Text;
                    string LandUse = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataLegal_LandUseDescriptionLabel")).Text;
                    string AnnualTax = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataLegal_AnnualTaxLabel")).Text;
                    string HomesteadReduction = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataLegal_HomesteadReductionLabel")).Text;
                    string Reduction = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataLegal_Reduction25Label")).Text;
                    string Foreclosure = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataLegal_ForeclosedLabel")).Text;
                    string BoardofRevision = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataLegal_BoardOfRevisionLabel")).Text;
                    string NewConstruction = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataLegal_NewConstructionLabel")).Text;
                    string DividedProperty = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataLegal_DividedPropertyLabel")).Text;
                    //string MapNumber = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataLegal_MapNumberLabel")).Text;
                    gc.CreatePdf(orderNumber, Parcel_number, "Base", driver, "OH", "Trumbull");
                    driver.FindElement(By.LinkText("Residential")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Residential", driver, "OH", "Trumbull");
                    try
                    {
                        yearbuild = driver.FindElement(By.Id("ContentPlaceHolder1_Residential_fvDataResidential_YearBuiltLabel")).Text;
                    }
                    catch { }
                    string Propertyresult = Ownar + "~" + address + "~" + mailingaddress + "~" + city + "~" + Township + "~" + school + "~" + Taxdistient + "~" + LegalDescription + "~" + LegalAcres + "~" + LandUse + "~" + AnnualTax + "~" + yearbuild;
                    gc.insert_date(orderNumber, Parcel_number, 1421, Propertyresult, 1, DateTime.Now);
                    string Flagresult = HomesteadReduction + "~" + Reduction + "~" + Foreclosure + "~" + BoardofRevision + "~" + NewConstruction + "~" + DividedProperty;
                    gc.insert_date(orderNumber, Parcel_number, 1575, Flagresult, 1, DateTime.Now);
                    driver.FindElement(By.LinkText("Land")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Lane", driver, "OH", "Trumbull");
                    //assessment
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.FindElement(By.LinkText("Valuation")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Valuation", driver, "OH", "Trumbull");
                    IWebElement valuvationTable = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Valuation_fvDataValuation']/tbody/tr/td/table/tbody"));
                    IList<IWebElement> Valuvationrow = valuvationTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> valuvationid;
                    IList<IWebElement> valuvationTH;
                    foreach (IWebElement valuvation in Valuvationrow)
                    {
                        valuvationid = valuvation.FindElements(By.TagName("td"));
                        valuvationTH = valuvation.FindElements(By.TagName("th"));
                        if (valuvationid.Count != 0)
                        {
                            apprasiedresult = valuvationTH[0].Text + "~" + valuvationid[0].Text + "~" + valuvationid[1].Text;
                            gc.insert_date(orderNumber, Parcel_number, 1423, apprasiedresult, 1, DateTime.Now);
                        }
                    }
                    IWebElement valuvationhistorytable = driver.FindElement(By.Id("ContentPlaceHolder1_Valuation_gvValuationHistory"));
                    IList<IWebElement> valuvationhistortrow = valuvationhistorytable.FindElements(By.TagName("tr"));
                    IList<IWebElement> valuvationhistoryid;
                    foreach (IWebElement valuvation in valuvationhistortrow)
                    {
                        valuvationhistoryid = valuvation.FindElements(By.TagName("td"));
                        if (valuvationhistoryid.Count != 0 & !valuvation.Text.Contains("Date"))
                        {
                            string valuvationhistory = valuvationhistoryid[0].Text + "~" + valuvationhistoryid[1].Text + "~" + valuvationhistoryid[2].Text + "~" + valuvationhistoryid[3].Text + "~" + valuvationhistoryid[4].Text + "~" + valuvationhistoryid[5].Text + "~" + valuvationhistoryid[6].Text + "~" + valuvationhistoryid[7].Text;
                            gc.insert_date(orderNumber, Parcel_number, 1424, valuvationhistory, 1, DateTime.Now);
                        }
                    }
                    driver.FindElement(By.LinkText("Sales")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Sales", driver, "OH", "Trumbull");
                    driver.FindElement(By.LinkText("Tax")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Tax", driver, "OH", "Trumbull");
                    string taxrat = driver.FindElement(By.Id("ContentPlaceHolder1_Tax_fvDataTaxRates_FullRateLabel")).Text;
                    string EFtaxrate = driver.FindElement(By.Id("ContentPlaceHolder1_Tax_fvDataTaxRates_EffectiveRateLabel")).Text;
                    string Escrow = driver.FindElement(By.Id("ContentPlaceHolder1_Tax_fvDataEscrow_EscrowAmountLabel")).Text;
                    string taxinfores = taxrat + "~" + EFtaxrate + "~" + Escrow;
                    gc.insert_date(orderNumber, Parcel_number, 1425, taxinfores, 1, DateTime.Now);

                    IWebElement propertytaxtable = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Tax_fvDataTax']/tbody/tr/td/table/tbody"));
                    IList<IWebElement> propertytaxrow = propertytaxtable.FindElements(By.TagName("tr"));
                    IList<IWebElement> properttaxid;
                    IList<IWebElement> propertytaxth;
                    foreach (IWebElement propertytax in propertytaxrow)
                    {
                        properttaxid = propertytax.FindElements(By.TagName("td"));
                        propertytaxth = propertytax.FindElements(By.TagName("th"));
                        if (properttaxid.Count == 6)
                        {
                            string Propertyresult1 = propertytaxth[0].Text + "~" + properttaxid[0].Text + "~" + properttaxid[1].Text + "~" + properttaxid[2].Text + "~" + properttaxid[3].Text + "~" + properttaxid[4].Text + "~" + properttaxid[5].Text + "~" + " ";
                            gc.insert_date(orderNumber, Parcel_number, 1426, Propertyresult1, 1, DateTime.Now);
                        }
                        if (properttaxid.Count == 3)
                        {
                            string nettaxresult = propertytaxth[0].Text + "~" + properttaxid[0].Text + "~" + " " + "~" + properttaxid[1].Text + "~" + " " + "~" + properttaxid[2].Text + "~" + " " + " ";
                            gc.insert_date(orderNumber, Parcel_number, 1426, nettaxresult, 1, DateTime.Now);
                        }
                        if (properttaxid.Count == 4)
                        {
                            string Netresult = propertytaxth[0].Text + "~" + properttaxid[0].Text + "~" + " " + "~" + properttaxid[1].Text + "~" + " " + "~" + properttaxid[2].Text + "~" + " " + "~" + properttaxid[3].Text;
                            gc.insert_date(orderNumber, Parcel_number, 1426, Netresult, 1, DateTime.Now);
                        }
                    }
                    try
                    {
                        string countvalue = driver.FindElement(By.Id("ContentPlaceHolder1_Tax_fvDataSpecials_lblDataSpecials")).Text.Replace("of", "").Trim();
                        int sp = 0;
                        for (int s = 1; s <= Convert.ToInt16(countvalue); s++)
                        {
                            if (s != 1)
                            {
                                IWebElement PropertyInformation = driver.FindElement(By.Id("ContentPlaceHolder1_Tax_fvDataSpecials_ddlDataSpecials"));
                                SelectElement PropertyInformationSelect = new SelectElement(PropertyInformation);
                                PropertyInformationSelect.SelectByIndex(sp);
                                Thread.Sleep(2000);
                                gc.CreatePdf(orderNumber, Parcel_number, "Tax" + s, driver, "OH", "Trumbull");
                            }
                            string Type = driver.FindElement(By.Id("ContentPlaceHolder1_Tax_fvDataSpecials_Label")).Text;
                            IWebElement Splialassessmentable = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Tax_fvDataSpecials']/tbody/tr/td/table/tbody"));
                            IList<IWebElement> Splialassessmenrow = Splialassessmentable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Splialassessmenid;
                            IList<IWebElement> SplialassessmenTH;
                            foreach (IWebElement Splialassessmen in Splialassessmenrow)
                            {
                                Splialassessmenid = Splialassessmen.FindElements(By.TagName("td"));
                                SplialassessmenTH = Splialassessmen.FindElements(By.TagName("th"));
                                if (Splialassessmenid.Count == 6)
                                {
                                    string taxpaymentresult = SplialassessmenTH[0].Text + "~" + Type + "~" + Splialassessmenid[0].Text + "~" + Splialassessmenid[1].Text + "~" + Splialassessmenid[2].Text + "~" + Splialassessmenid[3].Text + "~" + Splialassessmenid[4].Text + "~" + Splialassessmenid[5].Text;
                                    gc.insert_date(orderNumber, Parcel_number, 1427, taxpaymentresult, 1, DateTime.Now);
                                }
                                if (Splialassessmenid.Count == 3)
                                {
                                    string taxpaymentresult = SplialassessmenTH[0].Text + "~" + Type + "~" + Splialassessmenid[0].Text + "~" + "~" + Splialassessmenid[1].Text + "~" + "~" + Splialassessmenid[2].Text + "~";
                                    gc.insert_date(orderNumber, Parcel_number, 1427, taxpaymentresult, 1, DateTime.Now);
                                }
                            }
                            sp++;
                        }
                    }
                    catch { }
                    IWebElement taxpaymenthistable = driver.FindElement(By.Id("ContentPlaceHolder1_Tax_gvDataPayments"));
                    IList<IWebElement> taxpaymenthistryrow = taxpaymenthistable.FindElements(By.TagName("tr"));
                    IList<IWebElement> taxpaymenthisid;
                    foreach (IWebElement taxpaymenthis in taxpaymenthistryrow)
                    {
                        taxpaymenthisid = taxpaymenthis.FindElements(By.TagName("td"));
                        if (taxpaymenthisid.Count > 1)
                        {
                            string taxpaymentresult = taxpaymenthisid[0].Text + "~" + taxpaymenthisid[1].Text + "~" + taxpaymenthisid[2].Text + "~" + taxpaymenthisid[3].Text + "~" + taxpaymenthisid[4].Text + "~" + taxpaymenthisid[5].Text + "~" + taxpaymenthisid[6].Text;
                            gc.insert_date(orderNumber, Parcel_number, 1428, taxpaymentresult, 1, DateTime.Now);
                        }
                    }
                    driver.FindElement(By.LinkText("Improvements")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Improvements", driver, "OH", "Trumbull");
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "OH", "Trumbull", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    gc.mergpdf(orderNumber, "OH", "Trumbull");
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