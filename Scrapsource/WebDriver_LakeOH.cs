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
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_LakeOH
    {
        List<string> AddressSearch = new List<string>();
        IWebDriver driver;
        IWebElement Address1;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_Lake(string streetno, string streetname, string direction, string streetype, string unitno, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            List<string> multiparcel = new List<string>();
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string Parcel_number = "", Tax_Authority = "", yearbuilt = "", apprasiedresult = "", Valuvationresult = "", Multiaddressadd = "", MailingAddress = "", Addresshrf1 = "", Address12 = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new PhantomJSDriver();
            //driver = new ChromeDriver();
            using (driver = new PhantomJSDriver())
            {
                StartTime = DateTime.Now.ToString("HH:mm:ss");

                driver.Navigate().GoToUrl("http://lake.iviewauditor.com/Search.aspx");
                Thread.Sleep(2000);
                try
                {
                    if (searchType == "titleflex")
                    {
                        string straddress = streetno + " " + direction + " " + streetname + " " + streetype + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", straddress, "OH", "Lake");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_LakeOH"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        driver.FindElement(By.Id("ContentPlaceHolder1_btnDisclaimerAccept")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.LinkText("Address")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Address search Input ", driver, "OH", "Lake");
                        driver.FindElement(By.Name("ctl00$ContentPlaceHolder1$Address$tbAddressNumber")).SendKeys(streetno);
                        driver.FindElement(By.Id("ContentPlaceHolder1_Address_tbAddressDirection")).SendKeys(direction);
                        driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Address_tbAddressStreet']")).SendKeys(streetname.Trim());
                        gc.CreatePdf_WOP(orderNumber, "Address search Result ", driver, "OH", "Lake");
                        driver.FindElement(By.Id("ContentPlaceHolder1_Address_btnSearchAddress")).Click();
                        Thread.Sleep(5000);
                        try
                        {

                            int Max = 0;
                            //*[@id="ContentPlaceHolder1_gvSearchResults"]/tbody
                            IWebElement Multiparceladdress = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_gvSearchResults']/tbody"));
                            IList<IWebElement> Multiparcelrow = Multiparceladdress.FindElements(By.TagName("tr"));
                            IList<IWebElement> Multiparcelid;
                            gc.CreatePdf_WOP(orderNumber, "SearchAfter", driver, "OH", "Lake");
                            foreach (IWebElement multiparcel1 in Multiparcelrow)
                            {
                                Multiparcelid = multiparcel1.FindElements(By.TagName("td"));
                                if (Multiparcelid.Count != 0)
                                {
                                    Address1 = Multiparcelid[0].FindElement(By.TagName("a"));
                                    string Addresshrf = Address1.GetAttribute("href");
                                    string Addressst = Address1.Text;
                                    string Owner = Multiparcelid[1].Text;
                                    string Pin = Multiparcelid[2].Text;
                                    string Multiparcel = Owner + "~" + Pin;
                                    gc.insert_date(orderNumber, Addressst, 876, Multiparcel, 1, DateTime.Now);
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
                                HttpContext.Current.Session["multiParcel_LakeOH"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Max > 25)
                            {
                                HttpContext.Current.Session["multiParcel_LakeOH_Multicount"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("ContentPlaceHolder1_lblNumberOfResults")).Text;
                            if (nodata.Contains("No results"))
                            {
                                HttpContext.Current.Session["Nodata_LakeOH"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }

                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.Id("ContentPlaceHolder1_btnDisclaimerAccept")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.LinkText("Parcel")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Parcel search Input ", driver, "OH", "Lake");
                        driver.FindElement(By.Id("ContentPlaceHolder1_Parcel_tbParcelNumber")).SendKeys(parcelNumber);
                        gc.CreatePdf_WOP(orderNumber, "Parcel search Result ", driver, "OH", "Lake");
                        driver.FindElement(By.Id("ContentPlaceHolder1_Parcel_btnSearchParcel")).Click();
                        Thread.Sleep(2000);

                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("ContentPlaceHolder1_lblNumberOfResults")).Text;
                            if (nodata.Contains("No results"))
                            {
                                HttpContext.Current.Session["Nodata_LakeOH"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "ownername")
                    {
                        string[] Ownarsplit = ownername.Split(' ');
                        string lastname = Ownarsplit[0];
                        string firstname = Ownarsplit[1];
                        driver.FindElement(By.Id("ContentPlaceHolder1_btnDisclaimerAccept")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.LinkText("Owner")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.Id("ContentPlaceHolder1_Owner_tbOwnerLastName")).SendKeys(lastname.Trim());
                        driver.FindElement(By.Id("ContentPlaceHolder1_Owner_tbOwnerFirstName")).SendKeys(firstname.Trim());
                        gc.CreatePdf_WOP(orderNumber, "Owner search Input ", driver, "OH", "Lake");
                        driver.FindElement(By.Id("ContentPlaceHolder1_Owner_btnSearchOwner")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Owner search Result ", driver, "OH", "Lake");

                        //Multiparcel
                        try
                        {
                            int Max = 0;
                            IWebElement Multiparceladdress = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_gvSearchResults']/tbody"));
                            IList<IWebElement> Multiparcelrow = Multiparceladdress.FindElements(By.TagName("tr"));
                            IList<IWebElement> Multiparcelid;
                            gc.CreatePdf_WOP(orderNumber, "SearchAfter", driver, "OH", "Lake");
                            foreach (IWebElement multiparcel1 in Multiparcelrow)
                            {
                                Multiparcelid = multiparcel1.FindElements(By.TagName("td"));
                                if (Multiparcelid.Count != 0)
                                {
                                    Address1 = Multiparcelid[0].FindElement(By.TagName("a"));
                                    string Addresshrf = Address1.GetAttribute("href");
                                    string Addressst = Address1.Text;
                                    string Owner = Multiparcelid[1].Text;
                                    string Pin = Multiparcelid[2].Text;
                                    string Multiparcel = Owner + "~" + Pin;
                                    gc.insert_date(orderNumber, Addressst, 876, Multiparcel, 1, DateTime.Now);
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
                                HttpContext.Current.Session["multiParcel_LakeOH"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Max > 25)
                            {
                                HttpContext.Current.Session["multiParcel_LakeOH_Multicount"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("ContentPlaceHolder1_lblNumberOfResults")).Text;
                            if (nodata.Contains("No results"))
                            {
                                HttpContext.Current.Session["Nodata_LakeOH"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    //Property Details
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_gvSearchResults']/tbody/tr[2]/td[1]/a")).Click();
                        Thread.Sleep(5000);
                    }
                    catch { }
                    Parcel_number = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataProfile_ParcelLabel")).Text;
                    string Ownar = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataProfile_OwnerLabel")).Text;
                    string address = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataProfile_AddressLabel")).Text;
                    string mailaddress = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataMailingAddress_OwnerAddressLine2Label")).Text;
                    string mailcity = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataMailingAddress_OwnerAddressLine3Label")).Text;
                    string mailingaddress = mailaddress + " " + mailcity;
                    string city = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataGeographic_CityLabel")).Text;
                    string Township = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataGeographic_TownshipLabel")).Text;
                    string school = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataGeographic_SchoolDistrictLabel")).Text;
                    string Legal = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataLegal_LegalDescriptionLine1Label")).Text;
                    string Description = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataLegal_LegalDescriptionLine2Label")).Text;
                    string LegalDescription = Legal + " " + Description;
                    string LegalAcres = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataLegal_RangeLabel")).Text;
                    string LandUse = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataLegal_LandUseCodeLabel")).Text;
                    string HomesteadReduction = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataTaxCredits_HomesteadReductionLabel")).Text;
                    gc.CreatePdf(orderNumber, Parcel_number, "Base", driver, "OH", "Lake");


                    driver.FindElement(By.LinkText("Improvements")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Improvements", driver, "OH", "Lake");
                    try
                    {
                        yearbuilt = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Improvements_gvDataImprovements']/tbody/tr[2]/td[3]")).Text;
                    }
                    catch { }
                    string Propertyresult = Ownar + "~" + address + "~" + mailingaddress + "~" + city + "~" + Township + "~" + school + "~" + LegalDescription + "~" + LegalAcres + "~" + LandUse + "~" + yearbuilt;
                    gc.insert_date(orderNumber, Parcel_number, 1684, Propertyresult, 1, DateTime.Now);
                    //Assessment Details
                    driver.FindElement(By.LinkText("Base")).Click();
                    Thread.Sleep(2000);
                    IWebElement valuvationTable = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Base_fvDataValuation']/tbody/tr/td/table/tbody"));
                    IList<IWebElement> Valuvationrow = valuvationTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> valuvationid;
                    IList<IWebElement> valuvationTh;
                    foreach (IWebElement valuvation in Valuvationrow)
                    {
                        valuvationid = valuvation.FindElements(By.TagName("td"));
                        valuvationTh = valuvation.FindElements(By.TagName("th"));
                        if (valuvationid.Count > 1)
                        {
                            apprasiedresult = valuvationTh[0].Text.Replace(":", "").Trim() + "~" + valuvationid[0].Text + "~" + valuvationid[1].Text;
                            gc.insert_date(orderNumber, Parcel_number, 1685, apprasiedresult, 1, DateTime.Now);
                        }
                        if (valuvationid.Count == 1)
                        {
                            apprasiedresult = valuvationTh[0].Text.Replace(":", "").Trim() + "~" + " " + "~" + valuvationid[0].Text;
                            gc.insert_date(orderNumber, Parcel_number, 1685, apprasiedresult, 1, DateTime.Now);
                        }
                    }
                    //Tax credits
                    string apprasiedresult1 = "";
                    IWebElement valuvationTable1 = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Base_fvDataTaxCredits']/tbody/tr/td/table/tbody"));
                    IList<IWebElement> Valuvationrow1 = valuvationTable1.FindElements(By.TagName("tr"));
                    IList<IWebElement> valuvationid1;
                    IList<IWebElement> valuvationTh1;
                    foreach (IWebElement valuvation1 in Valuvationrow1)
                    {
                        valuvationid1 = valuvation1.FindElements(By.TagName("td"));
                        valuvationTh1 = valuvation1.FindElements(By.TagName("th"));
                        if (valuvationid1.Count == 1)
                        {
                            apprasiedresult1 = valuvationTh1[0].Text.Replace(":", "").Trim() + "~" + " " + "~" + valuvationid1[0].Text;
                            gc.insert_date(orderNumber, Parcel_number, 1685, apprasiedresult1, 1, DateTime.Now);
                        }
                    }

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.FindElement(By.LinkText("Land")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Lane", driver, "OH", "Lake");
                    //Valuation pdf
                    driver.FindElement(By.LinkText("Valuation")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Valuation", driver, "OH", "Lake");

                    //property detail
                    driver.FindElement(By.LinkText("Sales")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Sales", driver, "OH", "Lake");
                    driver.FindElement(By.LinkText("Tax")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Tax", driver, "OH", "Lake");
                    IWebElement propertytaxtable = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Tax_fvDataTax']/tbody/tr/td/table/tbody"));
                    IList<IWebElement> propertytaxrow = propertytaxtable.FindElements(By.TagName("tr"));
                    IList<IWebElement> properttaxid;
                    IList<IWebElement> propertytaxth;
                    foreach (IWebElement propertytax in propertytaxrow)
                    {
                        properttaxid = propertytax.FindElements(By.TagName("td"));
                        propertytaxth = propertytax.FindElements(By.TagName("th"));


                        if (propertytaxth.Count == 2 && !propertytax.Text.Contains("Tax Year 2018 Payable 2019"))
                        {
                            db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + "Property Tax~" + properttaxid[0].Text + "~" + properttaxid[1].Text + "' where Id = '" + 1686 + "'");
                        }
                        if (propertytaxth.Count == 3)
                        {
                            db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + "Property Tax~" + propertytaxth[1].Text + "~" + propertytaxth[2].Text + "' where Id = '" + 1686 + "'");
                        }
                        if (properttaxid.Count == 2)
                        {
                            string Netresult = propertytaxth[0].Text + "~" + properttaxid[0].Text + "~" + properttaxid[1].Text;
                            gc.insert_date(orderNumber, Parcel_number, 1686, Netresult, 1, DateTime.Now);
                        }
                        if (properttaxid.Count == 3)
                        {
                            string Netresult = propertytaxth[0].Text + "~" + properttaxid[0].Text + "~" + properttaxid[1].Text;
                            gc.insert_date(orderNumber, Parcel_number, 1686, Netresult, 1, DateTime.Now);
                        }
                        if (properttaxid.Count == 1)
                        {
                            db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + "Property Tax~" + propertytaxth[0].Text + "' where Id = '" + 1687 + "'");
                            string Netresult1 = propertytaxth[0].Text.Replace(":", "").Trim() + "~" + properttaxid[0].Text.Replace("Pay This Amount", "").Trim();
                            gc.insert_date(orderNumber, Parcel_number, 1687, Netresult1, 1, DateTime.Now);
                        }
                    }
                    //Special Assessment Details
                    //1688
                    try
                    {
                        string countvalue = driver.FindElement(By.Id("ContentPlaceHolder1_Tax_fvDataSpecials_lblDataSpecials")).Text.Replace("of", "").Trim();

                        for (int s = 1; s <= 2; s++)
                        {

                            if (s == 2 & countvalue == "2")
                            {
                                IWebElement PropertyInformation = driver.FindElement(By.Id("ContentPlaceHolder1_Tax_fvDataSpecials_ddlDataSpecials"));
                                SelectElement PropertyInformationSelect = new SelectElement(PropertyInformation);
                                PropertyInformationSelect.SelectByIndex(1);
                                gc.CreatePdf(orderNumber, Parcel_number, "Tax 2", driver, "OH", "Lake");
                            }
                            if (s == 2 & countvalue == "1")
                            {
                                break;
                            }
                            IWebElement Splialassessmentable = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Tax_fvDataSpecials']/tbody"));
                            IList<IWebElement> Splialassessmenrow = Splialassessmentable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Splialassessmenid;
                            foreach (IWebElement Splialassessmen in Splialassessmenrow)
                            {
                                Splialassessmenid = Splialassessmen.FindElements(By.TagName("td"));
                                if (Splialassessmenid.Count == 4)
                                {
                                    string taxpaymentresult = Splialassessmenid[1].Text + "~" + Splialassessmenid[2].Text.Replace("\r", " ") + "~" + Splialassessmenid[3].Text;
                                    gc.insert_date(orderNumber, Parcel_number, 1688, taxpaymentresult, 1, DateTime.Now);
                                }
                            }
                        }
                    }
                    catch { }
                    // Tax Payment History Details
                    IWebElement taxpaymenthistable = driver.FindElement(By.Id("ContentPlaceHolder1_Tax_gvDataPayments"));
                    IList<IWebElement> taxpaymenthistryrow = taxpaymenthistable.FindElements(By.TagName("tr"));
                    IList<IWebElement> taxpaymenthisid;
                    foreach (IWebElement taxpaymenthis in taxpaymenthistryrow)
                    {
                        taxpaymenthisid = taxpaymenthis.FindElements(By.TagName("td"));
                        if (taxpaymenthisid.Count != 0)
                        {
                            string taxpaymentresult = taxpaymenthisid[0].Text + "~" + taxpaymenthisid[1].Text;
                            gc.insert_date(orderNumber, Parcel_number, 1691, taxpaymentresult, 1, DateTime.Now);
                        }
                    }
                    //Tax Bill Download                    
                    driver.FindElement(By.LinkText("Tax")).Click();
                    Thread.Sleep(2000);
                    IWebElement test = driver.FindElement(By.Id("ContentPlaceHolder1_Tax_hlinkTaxBill"));
                    string downurl = test.GetAttribute("href");
                    driver.Navigate().GoToUrl(downurl);
                    Thread.Sleep(15000);
                    gc.downloadfile(downurl, orderNumber, Parcel_number, "ViewTaxBill", "OH", "Lake");
                                      
                    
                    //Improvements
                    driver.FindElement(By.LinkText("Improvements")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Improvements", driver, "OH", "Lake");
                    //Permit
                    driver.FindElement(By.LinkText("Permit")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Permit", driver, "OH", "Lake");
                    //Permit
                    driver.FindElement(By.LinkText("Commercial")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Commercial", driver, "OH", "Lake");
                    //End
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Quit();
                    gc.mergpdf(orderNumber, "OH", "Lake");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "OH", "Lake", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
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
