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
    public class Webdriver_WoodOH
    {
        IWebDriver driver;
        IWebElement Address1;
        DBconnection db = new DBconnection();
        List<string> link = new List<string>();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_WoodOH(string streetno, string direction, string streettype, string streetname, string unitnumber, string ownernm, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            List<string> taxinformation = new List<string>();
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string Parcel_number = "", Tax_Authority = "", yearbuild = "", apprasiedresult = "", Addresshrf = "", Valuvationresult = "", Multiaddressadd = "", MailingAddress = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;

            //driver = new ChromeDriver();driver = new PhantomJSDriver()
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("http://auditor.co.wood.oh.us/Search.aspx");
                    if (searchType == "address")
                    {
                        driver.FindElement(By.LinkText("Address")).Click();
                        driver.FindElement(By.Id("ContentPlaceHolder1_Address_tbAddressNumber")).SendKeys(streetno);
                        //  driver.FindElement(By.Id("ContentPlaceHolder1_Address_tbAddressDirection")).SendKeys(direction);
                        driver.FindElement(By.Id("ContentPlaceHolder1_Address_tbAddressStreet")).SendKeys(streetname);
                        gc.CreatePdf_WOP(orderNumber, "SearchBefore", driver, "OH", "Wood");
                        driver.FindElement(By.Id("ContentPlaceHolder1_Address_btnSearchAddress")).Click();
                        Thread.Sleep(2000);
                        try
                        {
                            int Max = 0;
                            //*[@id="ContentPlaceHolder1_gvSearchResults"]/tbody
                            IWebElement Multiparceladdress = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_gvSearchResults']/tbody"));
                            IList<IWebElement> Multiparcelrow = Multiparceladdress.FindElements(By.TagName("tr"));
                            IList<IWebElement> Multiparcelid;
                            gc.CreatePdf_WOP(orderNumber, "SearchAfter", driver, "OH", "Wood");
                            foreach (IWebElement multiparcel in Multiparcelrow)
                            {
                                Multiparcelid = multiparcel.FindElements(By.TagName("td"));
                                if (Multiparcelid.Count != 0)
                                {
                                    Address1 = Multiparcelid[0].FindElement(By.TagName("a"));
                                    Addresshrf = Address1.GetAttribute("href");
                                    string Addressst = Address1.Text;
                                    string Owner = Multiparcelid[1].Text;
                                    string Pin = Multiparcelid[2].Text;
                                    string Multiparcel = Pin + "~" + Owner;
                                    gc.insert_date(orderNumber, Addressst, 1490, Multiparcel, 1, DateTime.Now);
                                    Max++;
                                }
                            }
                            if (Max == 1)
                            {
                                Address1.Click();
                                Thread.Sleep(2000);
                            }
                            if (Max > 1 && Max < 26)
                            {
                                HttpContext.Current.Session["multiParcel_WoodOH"] = "Maximum";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Max > 25)
                            {
                                HttpContext.Current.Session["multiParcel_WoodOH_Multicount"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                            if (Max == 0)
                            {
                                HttpContext.Current.Session["Zero_WoodOH"] = "Zero";
                                driver.Quit();
                                return "Zero";
                            }

                        }
                        catch { }
                    }
                    if (searchType == "ownername")
                    {
                        string[] Ownarsplit = ownernm.Split(' ');
                        if(Ownarsplit.Length == 2)
                        {
                            string lastname = Ownarsplit[1];
                            string firstname = Ownarsplit[0];
                            driver.FindElement(By.Id("ContentPlaceHolder1_Owner_tbOwnerLastName")).SendKeys(firstname.Trim());
                            driver.FindElement(By.Id("ContentPlaceHolder1_Owner_tbOwnerFirstName")).SendKeys(lastname.Trim());
                        }
                        else
                        {
                            driver.FindElement(By.Id("ContentPlaceHolder1_Owner_tbOwnerLastName")).SendKeys(ownernm.Trim());
                        }
                        
                        gc.CreatePdf_WOP(orderNumber, "Search Before", driver, "OH", "Wood");
                        driver.FindElement(By.Id("ContentPlaceHolder1_Owner_btnSearchOwner")).Click();
                        Thread.Sleep(2000);
                        try
                        {
                            int Max = 0;

                            IWebElement Multiparceladdress = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_gvSearchResults']/tbody"));
                            IList<IWebElement> Multiparcelrow = Multiparceladdress.FindElements(By.TagName("tr"));
                            IList<IWebElement> Multiparcelid;
                            gc.CreatePdf_WOP(orderNumber, "SearchAfter", driver, "OH", "Wood");
                            foreach (IWebElement multiparcel in Multiparcelrow)
                            {
                                Multiparcelid = multiparcel.FindElements(By.TagName("td"));
                                if (Multiparcelid.Count != 0)
                                {
                                    Address1 = Multiparcelid[0].FindElement(By.TagName("a"));
                                    Addresshrf = Address1.GetAttribute("href");
                                    string Addressst = Address1.Text;
                                    string Owner = Multiparcelid[1].Text;
                                    string Pin = Multiparcelid[2].Text;
                                    string Multiparcel = Pin + "~" + Owner;
                                    gc.insert_date(orderNumber, Addressst, 1490, Multiparcel, 1, DateTime.Now);
                                    Max++;
                                }
                            }
                            if (Max == 1)
                            {
                                Address1.Click();
                                Thread.Sleep(2000);
                            }
                            if (Max > 1 && Max < 26)
                            {
                                HttpContext.Current.Session["multiParcel_WoodOH"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Max > 25)
                            {
                                HttpContext.Current.Session["multiParcel_WoodOH_Multicount"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                            if (Max == 0)
                            {
                                HttpContext.Current.Session["Zero_WoodOH"] = "Zero";
                                driver.Quit();
                                return "Zero";
                            }

                        }
                        catch { }
                        try
                        {
                            string Noresult = driver.FindElement(By.Id("ContentPlaceHolder1_lblNumberOfResults")).Text;
                            if (Noresult.Trim() != "")
                            {
                                HttpContext.Current.Session["Zero_WoodOH"] = "Zero";
                                driver.Quit();
                                return "Zero";
                            }

                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.LinkText("Parcel")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.Id("ContentPlaceHolder1_Parcel_tbParcelNumber")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Before", driver, "OH", "Wood");
                        driver.FindElement(By.Id("ContentPlaceHolder1_Parcel_btnSearchParcel")).Click();
                        Thread.Sleep(2000);
                        try
                        {
                            int Max = 0;

                            IWebElement Multiparceladdress = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_gvSearchResults']/tbody"));
                            IList<IWebElement> Multiparcelrow = Multiparceladdress.FindElements(By.TagName("tr"));
                            IList<IWebElement> Multiparcelid;
                            gc.CreatePdf(orderNumber, parcelNumber, "Parcel After", driver, "OH", "Wood");
                            foreach (IWebElement multiparcel in Multiparcelrow)
                            {
                                Multiparcelid = multiparcel.FindElements(By.TagName("td"));
                                if (Multiparcelid.Count != 0)
                                {
                                    Address1 = Multiparcelid[0].FindElement(By.TagName("a"));
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
                                Address1.Click();
                                Thread.Sleep(2000);
                            }
                            if (Max > 1 && Max < 26)
                            {
                                HttpContext.Current.Session["multiParcel_WoodOH"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Max > 25)
                            {
                                HttpContext.Current.Session["multiParcel_WoodOH_Multicount"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                            if (Max == 0)
                            {
                                HttpContext.Current.Session["Zero_WoodOH"] = "Zero";
                                driver.Quit();
                                return "Zero";
                            }

                        }
                        catch { }
                    }

                    try
                    {
                        //No Data Found
                        string nodata = driver.FindElement(By.Id("ContentPlaceHolder1_lblNumberOfResults")).Text;
                        if (nodata.Contains("No results"))
                        {
                            HttpContext.Current.Session["Zero_WoodOH"] = "Zero";
                            driver.Quit();
                            return "Zero";
                        }
                    }
                    catch { }

                    Parcel_number = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataProfile_ParcelLabel")).Text;
                    string Ownar = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataProfile_OwnerLabel")).Text;
                    string address = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataProfile_AddressLabel")).Text;
                    string mailname = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataMailingAddress_MailingAddressLine1Label")).Text;
                    string mailaddress = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataMailingAddress_MailingAddressLine2Label")).Text;
                    string mailcity = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataMailingAddress_MailingAddressLine3Label")).Text;
                    string mailingaddress = mailname + " " + mailaddress + " " + mailcity;
                    string city = driver.FindElement(By.Id("ContentPlaceHolder1_Base_FormView1_CityLabel")).Text;
                    string Township = driver.FindElement(By.Id("ContentPlaceHolder1_Base_FormView1_TownshipLabel")).Text;
                    string school = driver.FindElement(By.Id("ContentPlaceHolder1_Base_FormView1_SchoolDistrictLabel")).Text;
                    // string Taxdistient = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataGeographic_TaxDistrictLabel")).Text;
                    string LegalDescription = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataLegal_LegalDescriptionLabel")).Text;
                    string LegalAcres = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataLegal_TotalAcresLabel")).Text;
                    string LandUse = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataLegal_LandUseDescriptionLabel")).Text;
                    string AnnualTax = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataLegal_AnnualTaxLabel")).Text;
                    //string MapNumber = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataLegal_MapNumberLabel")).Text;
                    string HomesteadReduction = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataLegal_HomesteadReductionLabel")).Text;
                    string Reduction = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataLegal_Reduction25Label")).Text;
                    string RedictionHd = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Base_fvDataLegal']/tbody/tr/td/table/tbody/tr[2]/th[2]")).Text;
                    string Foreclosure = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataLegal_ForeclosedLabel")).Text;
                    string Board = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataLegal_BoardOfRevisionLabel")).Text;
                    string NewConstruction = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataLegal_NewConstructionLabel")).Text;
                    string DividedProperty = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataLegal_DividedPropertyLabel")).Text;
                    gc.CreatePdf(orderNumber, Parcel_number, "Base", driver, "OH", "Wood");
                    driver.FindElement(By.LinkText("Residential")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Residential", driver, "OH", "Wood");
                    try
                    {
                        yearbuild = driver.FindElement(By.Id("ContentPlaceHolder1_Residential_fvDataResidential_YearBuiltLabel")).Text;
                    }
                    catch { }
                    string Propertyresult = Ownar + "~" + address + "~" + mailingaddress + "~" + city + "~" + Township + "~" + school + "~" + LegalDescription + "~" + LegalAcres + "~" + LandUse + "~" + AnnualTax + "~" + yearbuild;
                    gc.insert_date(orderNumber, Parcel_number, 1435, Propertyresult, 1, DateTime.Now);
                    string flagresult = HomesteadReduction + "~" + Reduction + "~" + Foreclosure + "~" + Board + "~" + NewConstruction + "~" + DividedProperty;
                    gc.insert_date(orderNumber, Parcel_number, 1583, flagresult, 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.FindElement(By.LinkText("Land")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Lane", driver, "OH", "Wood");
                    //assessment
                    driver.FindElement(By.LinkText("Valuation")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Valuation", driver, "OH", "Wood");
                    IWebElement valuvationTable = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Valuation_fvDataValuation']/tbody/tr/td/table/tbody"));
                    IList<IWebElement> Valuvationrow = valuvationTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> valuvationid;
                    IList<IWebElement> valuvationTh;
                    foreach (IWebElement valuvation in Valuvationrow)
                    {
                        valuvationid = valuvation.FindElements(By.TagName("td"));
                        valuvationTh = valuvation.FindElements(By.TagName("th"));
                        if (valuvationid.Count > 1)
                        {
                            apprasiedresult = valuvationTh[0].Text + "~" + valuvationid[0].Text + "~" + valuvationid[1].Text;
                            gc.insert_date(orderNumber, Parcel_number, 1436, apprasiedresult, 1, DateTime.Now);
                        }
                        if (valuvationid.Count == 1)
                        {
                            apprasiedresult = valuvationTh[0].Text + "~" + " " + "~" + valuvationid[0].Text;
                            gc.insert_date(orderNumber, Parcel_number, 1436, apprasiedresult, 1, DateTime.Now);
                        }
                    }
                    //gc.insert_date(orderNumber, Parcel_number, 1436, apprasiedresult.Remove(apprasiedresult.Length - 1), 1, DateTime.Now);
                    //IWebElement valuvationhistorytable = driver.FindElement(By.Id("ContentPlaceHolder1_Valuation_gvValuationHistory"));
                    //IList<IWebElement> valuvationhistortrow = valuvationhistorytable.FindElements(By.TagName("tr"));
                    //IList<IWebElement> valuvationhistoryid;
                    //foreach (IWebElement valuvation in valuvationhistortrow)
                    //{
                    //    valuvationhistoryid = valuvation.FindElements(By.TagName("td"));
                    //    if (valuvationhistoryid.Count != 0 & !valuvation.Text.Contains("Date"))
                    //    {
                    //        string valuvationhistory = valuvationhistoryid[0].Text + "~" + valuvationhistoryid[1].Text + "~" + valuvationhistoryid[2].Text + "~" + valuvationhistoryid[3].Text + "~" + valuvationhistoryid[4].Text + "~" + valuvationhistoryid[5].Text + "~" + valuvationhistoryid[6].Text + "~" + valuvationhistoryid[7].Text;
                    //        gc.insert_date(orderNumber, Parcel_number, 1424, valuvationhistory, 1, DateTime.Now);
                    //    }
                    //}
                    driver.FindElement(By.LinkText("Sales")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Sales", driver, "OH", "Wood");
                    driver.FindElement(By.LinkText("Tax")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Tax", driver, "OH", "Wood");
                    string taxrat = driver.FindElement(By.Id("ContentPlaceHolder1_Tax_fvDataTaxRates_FullRateLabel")).Text;
                    string EFtaxrate = driver.FindElement(By.Id("ContentPlaceHolder1_Tax_fvDataTaxRates_EffectiveRateLabel")).Text;
                    // string Escrow = driver.FindElement(By.Id("ContentPlaceHolder1_Tax_fvDataEscrow_EscrowAmountLabel")).Text;
                    string taxinfores = taxrat + "~" + EFtaxrate;
                    gc.insert_date(orderNumber, Parcel_number, 1437, taxinfores, 1, DateTime.Now);
                    //property detail
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
                            gc.insert_date(orderNumber, Parcel_number, 1438, Propertyresult1, 1, DateTime.Now);
                        }
                        if (properttaxid.Count == 3)
                        {
                            string nettaxresult = propertytaxth[0].Text + "~" + properttaxid[0].Text + "~" + " " + "~" + properttaxid[1].Text + "~" + " " + "~" + properttaxid[2].Text + "~" + " " + "~" + " ";
                            gc.insert_date(orderNumber, Parcel_number, 1438, nettaxresult, 1, DateTime.Now);
                        }
                        if (properttaxid.Count == 4)
                        {
                            string Netresult = propertytaxth[0].Text + "~" + properttaxid[0].Text + "~" + " " + "~" + properttaxid[1].Text + "~" + " " + "~" + properttaxid[2].Text + "~" + " " + "~" + properttaxid[3].Text;
                            gc.insert_date(orderNumber, Parcel_number, 1438, Netresult, 1, DateTime.Now);
                        }
                    }
                    //splialassessment
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
                                Thread.Sleep(1000);
                                gc.CreatePdf(orderNumber, Parcel_number, "Tax" + s, driver, "OH", "Wood");
                            }
                            string spcialtype = driver.FindElement(By.Id("ContentPlaceHolder1_Tax_fvDataSpecials_Label")).Text;
                            IWebElement Splialassessmentable = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Tax_fvDataSpecials']/tbody"));
                            IList<IWebElement> Splialassessmenrow = Splialassessmentable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Splialassessmenid;
                            IList<IWebElement> SplialassessmenTH;
                            foreach (IWebElement Splialassessmen in Splialassessmenrow)
                            {
                                Splialassessmenid = Splialassessmen.FindElements(By.TagName("td"));
                                SplialassessmenTH = Splialassessmen.FindElements(By.TagName("th"));
                                if (Splialassessmenid.Count == 6)
                                {
                                    string taxpaymentresult = spcialtype + "~" + SplialassessmenTH[0].Text + "~" + Splialassessmenid[0].Text + "~" + Splialassessmenid[1].Text + "~" + Splialassessmenid[2].Text + "~" + Splialassessmenid[3].Text + "~" + Splialassessmenid[4].Text + "~" + Splialassessmenid[5].Text;
                                    gc.insert_date(orderNumber, Parcel_number, 1439, taxpaymentresult, 1, DateTime.Now);
                                }
                                if (Splialassessmenid.Count == 3)
                                {
                                    string taxpaymentresult1 = spcialtype + "~" + SplialassessmenTH[0].Text + "~" + Splialassessmenid[0].Text + "~" + "~" + Splialassessmenid[1].Text + "~" + "~" + Splialassessmenid[2].Text + "~";
                                    gc.insert_date(orderNumber, Parcel_number, 1439, taxpaymentresult1, 1, DateTime.Now);
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
                        if (taxpaymenthisid.Count != 0)
                        {
                            string taxpaymentresult = taxpaymenthisid[0].Text + "~" + taxpaymenthisid[1].Text + "~" + taxpaymenthisid[2].Text + "~" + taxpaymenthisid[3].Text + "~" + taxpaymenthisid[4].Text + "~" + taxpaymenthisid[5].Text + "~" + taxpaymenthisid[6].Text;
                            gc.insert_date(orderNumber, Parcel_number, 1576, taxpaymentresult, 1, DateTime.Now);
                        }
                    }
                    driver.FindElement(By.LinkText("Improvements")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Improvements", driver, "OH", "Wood");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "OH", "Wood", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    driver.Quit();
                    gc.mergpdf(orderNumber, "OH", "Wood");
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