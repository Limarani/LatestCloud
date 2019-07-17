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
    public class WebDriver_YellowstoneMT
    {
        List<string> AddressSearch = new List<string>();
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_Yellowstone(string streetno, string streetname, string direction, string streetype, string unitno, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            List<string> multiparcel = new List<string>();
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                //driver = new ChromeDriver();
                string Parcel_Number = "", YearBuilt = "", Taxauthority = "";
                List<string> strUrl = new List<string>();
                StartTime = DateTime.Now.ToString("HH:mm:ss");

                //Tax Authority Details
                try
                {
                    driver.Navigate().GoToUrl("http://www.co.yellowstone.mt.gov/departments/list.asp");
                    IWebElement Taxauthorit = driver.FindElement(By.XPath("/html/body/div/div[7]/div[2]/font/div/table[26]/tbody"));
                    string Taxauthority1 = gc.Between(Taxauthorit.Text, "Sherry Long", "1st floor").Trim();
                    string Taxauthority2 = gc.Between(Taxauthorit.Text, "Rm 108", "slong").Trim();
                    Taxauthority = Taxauthority1 + Taxauthority2;
                }
                catch { }
                driver.Navigate().GoToUrl("http://www.co.yellowstone.mt.gov/gis/index.asp");
                Thread.Sleep(3000);
                try
                {
                    if (searchType == "titleflex")
                    {
                        string address = streetno + " " + direction + " " + streetname + " " + streetype + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "MT", "Yellowstone");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_YellowstoneMT"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        driver.FindElement(By.XPath("/html/body/div/div[7]/div[2]/table[3]/tbody/tr/td/div[3]/table[1]/tbody/tr[9]/td[2]/input")).SendKeys(streetno);
                        driver.FindElement(By.XPath("/html/body/div/div[7]/div[2]/table[3]/tbody/tr/td/div[3]/table[1]/tbody/tr[10]/td[2]/font/input[1]")).SendKeys(direction);
                        driver.FindElement(By.XPath("/html/body/div/div[7]/div[2]/table[3]/tbody/tr/td/div[3]/table[1]/tbody/tr[10]/td[2]/font/input[2]")).SendKeys(streetname);
                        driver.FindElement(By.XPath("/html/body/div/div[7]/div[2]/table[3]/tbody/tr/td/div[3]/table[1]/tbody/tr[10]/td[2]/font/input[3]")).SendKeys(streetype);
                        gc.CreatePdf(orderNumber, parcelNumber, "Address Search Pdf Before", driver, "MT", "Yellowstone");
                        driver.FindElement(By.XPath("/html/body/div/div[7]/div[2]/table[3]/tbody/tr/td/div[3]/table[1]/tbody/tr[20]/td/input[1]")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Address Search Pdf After", driver, "MT", "Yellowstone");
                        try
                        {

                            int Mcount = 0;
                            IWebElement single = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table[3]/tbody/tr[1]/td"));
                            if (single.Text.Contains("Page 1 of 1 from 1 total records"))
                            {
                                IWebElement IAddressSearch11 = driver.FindElement(By.LinkText("Full Orion Detail"));
                                IJavaScriptExecutor js11 = driver as IJavaScriptExecutor;
                                js11.ExecuteScript("arguments[0].click();", IAddressSearch11);
                                Thread.Sleep(5000);
                            }
                            if (!single.Text.Contains("Page 1 of 1 from 1 total records"))
                            {
                                IWebElement Multiaddresstable = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table[3]/tbody"));
                                IList<IWebElement> multiaddressrow = Multiaddresstable.FindElements(By.TagName("tr"));
                                IList<IWebElement> Multiaddressid;

                                foreach (IWebElement Multiaddress in multiaddressrow)
                                {
                                    Multiaddressid = Multiaddress.FindElements(By.TagName("td"));

                                    if (Multiaddress.Text.Contains("Full Orion Detail"))
                                    {
                                        IList<IWebElement> IMulti = Multiaddressid[0].FindElements(By.TagName("a"));
                                        foreach (IWebElement multi in IMulti)
                                        {
                                            if (multi.Text.Contains("Full Orion Detail"))
                                            {
                                                string strMulti = multi.GetAttribute("href");
                                                strUrl.Add(strMulti);
                                            }
                                        }
                                    }
                                }
                                string Primaryownermulti = "", PropertyAddressmulti = "";
                                foreach (string strLink in strUrl)
                                {
                                    driver.Navigate().GoToUrl(strLink);
                                    IWebElement Propertydetails11 = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table[3]/tbody/tr/td/table[2]/tbody"));
                                    Primaryownermulti = gc.Between(Propertydetails11.Text, "Primary Owner:", "Tax ID:").Trim();
                                    PropertyAddressmulti = gc.Between(Propertydetails11.Text, "Property Address:", "Legal Description:").Trim();
                                    string multiaddressresult1 = Primaryownermulti + "~" + PropertyAddressmulti;
                                    gc.insert_date(orderNumber, parcelNumber, 996, multiaddressresult1, 1, DateTime.Now);

                                }

                                if (multiaddressrow.Count > 1 && multiaddressrow.Count < 20 && strUrl.Count < 20)
                                {
                                    HttpContext.Current.Session["multiParcel_Yellowstone"] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                                if (multiaddressrow.Count > 1 && multiaddressrow.Count > 20 || strUrl.Count > 20)
                                {
                                    HttpContext.Current.Session["multiparcel_Yellowstone_Maximum"] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }
                            }
                        }
                        catch
                        {
                        }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("/html/body/div/div[7]/div[1]")).Text;
                            if (nodata.Contains("No Records Found"))
                            {
                                HttpContext.Current.Session["Nodata_YellowstoneMT"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.XPath("/html/body/div/div[7]/div[2]/table[3]/tbody/tr/td/div[3]/table[1]/tbody/tr[7]/td[2]/input")).SendKeys(ownername);
                        gc.CreatePdf(orderNumber, parcelNumber, "Ownername Search Pdf Before", driver, "MT", "Yellowstone");
                        driver.FindElement(By.XPath("/html/body/div/div[7]/div[2]/table[3]/tbody/tr/td/div[3]/table[1]/tbody/tr[20]/td/input[1]")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Ownername Search Pdf After", driver, "MT", "Yellowstone");

                        List<string> strUrlowner = new List<string>();
                        try
                        {
                            IWebElement Multiaddresstable = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table[3]/tbody"));
                            IList<IWebElement> multiaddressrow = Multiaddresstable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Multiaddressid;

                            foreach (IWebElement Multiaddress in multiaddressrow)
                            {
                                Multiaddressid = Multiaddress.FindElements(By.TagName("td"));

                                if (Multiaddress.Text.Contains("Full Orion Detail"))
                                {
                                    IList<IWebElement> IMulti = Multiaddressid[0].FindElements(By.TagName("a"));
                                    foreach (IWebElement multi in IMulti)
                                    {
                                        if (multi.Text.Contains("Full Orion Detail"))
                                        {
                                            string strMulti = multi.GetAttribute("href");
                                            strUrlowner.Add(strMulti);
                                        }
                                    }
                                }
                            }
                            string Primaryownermulti = "", PropertyAddressmulti = "";
                            foreach (string strLink in strUrlowner)
                            {
                                driver.Navigate().GoToUrl(strLink);
                                IWebElement Propertydetails11 = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table[3]/tbody/tr/td/table[2]/tbody"));
                                parcelNumber = gc.Between(Propertydetails11.Text, "Tax ID:", "Geo Code:").Trim();
                                Primaryownermulti = gc.Between(Propertydetails11.Text, "Primary Owner:", "Tax ID:").Trim();
                                PropertyAddressmulti = gc.Between(Propertydetails11.Text, "Property Address:", "Legal Description:").Trim();
                                string multiaddressresult1 = Primaryownermulti + "~" + PropertyAddressmulti;
                                gc.insert_date(orderNumber, parcelNumber, 996, multiaddressresult1, 1, DateTime.Now);

                            }
                            if (multiaddressrow.Count > 1 && multiaddressrow.Count < 20 && strUrlowner.Count < 20)
                            {
                                HttpContext.Current.Session["multiParcel_Yellowstone"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (multiaddressrow.Count > 1 && multiaddressrow.Count > 20 || strUrlowner.Count > 20)
                            {
                                HttpContext.Current.Session["multiparcel_Yellowstone_Maximum"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }

                        catch { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("/html/body/div/div[7]/div[1]")).Text;
                            if (nodata.Contains("No Records Found"))
                            {
                                HttpContext.Current.Session["Nodata_YellowstoneMT"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.XPath("/html/body/div/div[7]/div[2]/table[3]/tbody/tr/td/div[3]/table[1]/tbody/tr[5]/td[2]/input")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Pdf Before", driver, "MT", "Yellowstone");
                        driver.FindElement(By.XPath("/html/body/div/div[7]/div[2]/table[3]/tbody/tr/td/div[3]/table[1]/tbody/tr[20]/td/input[1]")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Ownername Search Pdf After", driver, "MT", "Yellowstone");
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("/html/body/div/div[7]/div[1]")).Text;
                            if (nodata.Contains("No Records Found"))
                            {
                                HttpContext.Current.Session["Nodata_YellowstoneMT"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    //Property Details
                    //Full Orion Details
                    try
                    {

                        IWebElement IAddressSearch1 = driver.FindElement(By.LinkText("Full Orion Detail"));
                        IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                        js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                        Thread.Sleep(2000);
                    }
                    catch { }
                    gc.CreatePdf(orderNumber, parcelNumber, "Full Orion Details Click", driver, "MT", "Yellowstone");

                    IWebElement Propertydetails1 = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table[3]/tbody/tr/td/table[2]/tbody"));
                    string Primaryowner = gc.Between(Propertydetails1.Text, "Primary Owner:", "Tax ID:").Trim();
                    Parcel_Number = gc.Between(Propertydetails1.Text, "Tax ID:", "Geo Code:").Trim();
                    string GeoCode = gc.Between(Propertydetails1.Text, "Geo Code:", "Property Address:").Trim();
                    string PropertyAddress = gc.Between(Propertydetails1.Text, "Property Address:", "Legal Description:").Trim();
                    string LegaDescription = gc.Between(Propertydetails1.Text, "Legal Description:", "Property Type :").Trim();
                    string PropertyType = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table[3]/tbody/tr/td/table[2]/tbody/tr[8]/td[2]/font")).Text.Trim();

                    IWebElement Propertydetails2 = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table[3]/tbody/tr/td/table[4]/tbody"));
                    string Neighborhoodcode1 = GlobalClass.Before(Propertydetails2.Text, "Location:").Trim();
                    string Neighborhoodcode2 = gc.Between(Propertydetails2.Text, "Neighborhood Code:", "Fronting").Trim();
                    string Neighborhoodcode = Neighborhoodcode1 + Neighborhoodcode2;
                    string Location = gc.Between(Propertydetails2.Text, "Location:", "Parking type:").Trim();

                    string Type = "";
                    try
                    {
                        IWebElement Propertydetails3 = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table[3]/tbody/tr/td/table[6]/tbody"));
                        Type = gc.Between(Propertydetails3.Text, "Type:", "Index").Trim();
                        YearBuilt = gc.Between(Propertydetails3.Text, "Year Built:", "ECF").Trim();
                    }
                    catch { }
                    string propertyDetails = Primaryowner + "~" + GeoCode + "~" + PropertyAddress + "~" + LegaDescription + "~" + PropertyType + "~" + Neighborhoodcode + "~" + Location + "~" + Type + "~" + YearBuilt;
                    gc.insert_date(orderNumber, Parcel_Number, 951, propertyDetails, 1, DateTime.Now);

                    //Tax information
                    string TaxID = "", primaryowner = "", MailingAddress = "", Property_Address = "", Subdivision = "", Township = "", Full_Legal = "", Geo_Code = "";
                    driver.FindElement(By.LinkText("Property Tax Detail")).Click();
                    Thread.Sleep(5000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Property Tax Details Pdf", driver, "MT", "Yellowstone");
                    ///html/body/table/tbody/tr/td/table[2]/tbody/tr[2]/td/table/tbody
                    IList<IWebElement> tableList0 = driver.FindElements(By.TagName("table"));

                    foreach (IWebElement row12 in tableList0)
                    {
                        if (row12.Text.Contains("Disclaimer: Not all fields are currently "))
                        {
                            //TaxID = gc.Between(row12.Text, "Tax ID:", "Primary Party").Trim();
                            primaryowner = gc.Between(row12.Text, "Primary Owner Name:", "Ownership History").Trim();
                            MailingAddress = gc.Between(row12.Text, "Mailing Address:", "Property Address:").Replace(primaryowner, "").Trim();
                            Property_Address = gc.Between(row12.Text, "Property Address:", "Township:").Trim();
                            Township = gc.Between(row12.Text, "Township:", "Subdivision:").Trim();
                            Subdivision = gc.Between(row12.Text, "Subdivision:", "Full Legal:").Trim();
                            Full_Legal = gc.Between(row12.Text, "Full Legal:", "GeoCode:").Trim();
                            Geo_Code = gc.Between(row12.Text, "GeoCode:", "Show on Map").Trim();
                            break;
                        }
                    }

                    string Taxinformationdetail = primaryowner.Trim() + "~" + MailingAddress.Trim() + "~" + Property_Address.Trim() + "~" + Township.Trim() + "~" + Subdivision.Trim() + "~" + Full_Legal.Trim() + "~" + Geo_Code.Trim() + "~" + Taxauthority.Trim();
                    gc.insert_date(orderNumber, Parcel_Number, 950, Taxinformationdetail, 1, DateTime.Now);



                    int tcount = 0;
                    string Leve_district = "", AssessedLandValue = "", Assessedbuilding = "", totalassessed = "", Assessedyear = "", ResidentialCityorTownLots = "", ImprovementsonResidentialCityorTownLots = "", ImprovementsonCommercialTractLand = "", Totalasessvalue = "";
                    IList<IWebElement> tableList = driver.FindElements(By.TagName("table"));// table list of Payment History Table

                    foreach (IWebElement tab in tableList)
                    {
                        if (tcount == 5)
                        {

                            if (tab.Text.Contains("Property Assessment Information"))
                            {

                                IList<IWebElement> Taxassementrow = tab.FindElements(By.TagName("tr"));
                                IList<IWebElement> taxassmentid;

                                foreach (IWebElement taxassemnt in Taxassementrow)
                                {
                                    taxassmentid = taxassemnt.FindElements(By.TagName("td"));

                                    if (taxassemnt.Text.Contains("Levy District:"))
                                    {
                                        Leve_district = taxassmentid[1].Text;
                                    }
                                    if (taxassemnt.Text.Contains("Assessed Land Value = $") && taxassmentid.Count != 2)
                                    {
                                        AssessedLandValue = taxassmentid[2].Text;
                                        Assessedbuilding = taxassmentid[4].Text;
                                        totalassessed = taxassmentid[6].Text;
                                    }
                                    if (taxassemnt.Text.Contains("Assessed Value Detail Tax Year:") && taxassmentid.Count == 10 && taxassemnt.Text.Contains("3501 - Improvements on Residential City or Town Lots = $"))
                                    {
                                        Assessedyear = taxassmentid[1].Text;
                                        ResidentialCityorTownLots = taxassmentid[5].Text;
                                        ImprovementsonResidentialCityorTownLots = taxassmentid[7].Text;
                                        Totalasessvalue = taxassmentid[9].Text;
                                    }
                                    if (taxassmentid.Count == 8 && taxassemnt.Text.Contains("3307 - Improvements on Commercial Tract Land = $"))
                                    {
                                        Assessedyear = taxassmentid[1].Text;
                                        ImprovementsonCommercialTractLand = taxassmentid[5].Text;
                                        Totalasessvalue = taxassmentid[7].Text;
                                    }
                                    if (taxassemnt.Text.Contains("Assessed Value Detail Tax Year:") && taxassemnt.Text.Contains("3507 - Improvements on Commercial City or Town Lots = $"))
                                    {
                                        Assessedyear = taxassmentid[1].Text;
                                        ResidentialCityorTownLots = taxassmentid[5].Text;
                                        ImprovementsonResidentialCityorTownLots = taxassmentid[7].Text;
                                        Totalasessvalue = taxassmentid[9].Text;
                                    }

                                }
                                string Propertyassessment = Leve_district.Trim() + "~" + AssessedLandValue.Trim() + "~" + Assessedbuilding.Trim() + "~" + totalassessed.Trim() + "~" + Assessedyear.Trim() + "~" + ResidentialCityorTownLots.Trim() + "~" + ImprovementsonResidentialCityorTownLots + "~" + ImprovementsonCommercialTractLand + "~" + Totalasessvalue.Trim();
                                gc.insert_date(orderNumber, Parcel_Number, 953, Propertyassessment, 1, DateTime.Now);

                            }
                        }
                        tcount++;
                    }



                    //Tax Payment Details Table
                    string TaxPaymentdetails = "", Taxpaymentyear = "", Firsthalf = "", Statusone = "", Secondhalf = "", Statustwo = "", Total = "";

                    try
                    {
                        int pcount = 0;
                        IList<IWebElement> tableList1 = driver.FindElements(By.TagName("table"));// table list of Payment History Table

                        foreach (IWebElement tab1 in tableList1)
                        {
                            if (pcount == 8)
                            {

                                if (tab1.Text.Contains("Year"))
                                {
                                    IList<IWebElement> PaymentTR = tab1.FindElements(By.TagName("tr"));
                                    IList<IWebElement> PaymentTD;

                                    foreach (IWebElement PaymentTax in PaymentTR)
                                    {
                                        PaymentTD = PaymentTax.FindElements(By.TagName("td"));
                                        if (PaymentTD.Count != 0 && !PaymentTax.Text.Contains("Year"))
                                        {
                                            Taxpaymentyear = PaymentTD[0].Text;
                                            Firsthalf = PaymentTD[1].Text;
                                            Statusone = PaymentTD[2].Text;
                                            Secondhalf = PaymentTD[3].Text;
                                            Statustwo = PaymentTD[4].Text;
                                            Total = PaymentTD[5].Text;

                                            TaxPaymentdetails = Taxpaymentyear + "~" + Firsthalf + "~" + Statusone + "~" + Secondhalf + "~" + Statustwo + "~" + Total;
                                            gc.insert_date(orderNumber, Parcel_Number, 966, TaxPaymentdetails, 1, DateTime.Now);
                                        }
                                    }

                                }

                            }

                            pcount++;
                        }
                    }
                    catch { }

                    //Property Tax Details Table
                    string scrshot = "";
                    string current = driver.CurrentWindowHandle;
                    int year = 0, p = 0, currentyeartable = DateTime.Now.Year;

                    try
                    {

                        int scount = 0;
                        IList<IWebElement> tableList2 = driver.FindElements(By.TagName("table"));// table list of Payment History Table

                        foreach (IWebElement tab2 in tableList2)
                        {
                            if (scount == 8)
                            {

                                if (tab2.Text.Contains("Year"))
                                {
                                    IList<IWebElement> Currentrow = tab2.FindElements(By.TagName("tr"));
                                    IList<IWebElement> currentid;
                                    int YearCount = Currentrow.Count - 1;
                                    foreach (IWebElement currenttax in Currentrow)
                                    {
                                        currentid = currenttax.FindElements(By.TagName("td"));
                                        if (currentid.Count != 0)
                                        {
                                            IWebElement Aherfyear = currentid[0];
                                            if (year == YearCount - 1 || year == YearCount - 2)
                                            {
                                                try
                                                {

                                                    Aherfyear.FindElement(By.TagName("a")).Click();
                                                    Thread.Sleep(4000);
                                                    driver.SwitchTo().Window(driver.WindowHandles.Last());
                                                    gc.CreatePdf(orderNumber, Parcel_Number, " Last Property Tax Details  pdf" + year, driver, "MT", "Yellowstone");
                                                    driver.SwitchTo().Window(current);
                                                }
                                                catch { }
                                            }

                                            if (year == YearCount)
                                            {
                                                try
                                                {
                                                    Aherfyear.FindElement(By.TagName("a")).Click();
                                                    Thread.Sleep(4000);
                                                    driver.SwitchTo().Window(driver.WindowHandles.Last());
                                                    gc.CreatePdf(orderNumber, Parcel_Number, " Last Property Tax Details  pdf", driver, "MT", "Yellowstone");
                                                }
                                                catch { }
                                            }
                                        }
                                        year++;
                                    }
                                }

                            }
                            scount++;
                        }

                        driver.SwitchTo().Window(driver.WindowHandles.Last());
                        IWebElement yearswitchiweb = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table[2]/tbody"));
                        string yearswitch = GlobalClass.After(yearswitchiweb.Text, "Tax Year:").Trim();

                        IWebElement taxyeartable = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table[3]/tbody"));
                        IList<IWebElement> taxyearrow = taxyeartable.FindElements(By.TagName("tr"));
                        IList<IWebElement> taxyearid;
                        foreach (IWebElement taxyear in taxyearrow)
                        {
                            taxyearid = taxyear.FindElements(By.TagName("td"));
                            if (taxyearid.Count > 5 && !taxyear.Text.Contains("Code") && !taxyear.Text.Contains("Date Paid") && !taxyear.Text.Contains("Totals"))
                            {
                                string taxyearresult = yearswitch + "~" + taxyearid[0].Text.Trim() + "~" + taxyearid[1].Text.Trim() + "~" + taxyearid[2].Text.Trim() + "~" + taxyearid[3].Text.Trim() + "~" + taxyearid[4].Text.Trim() + "~" + taxyearid[5].Text.Trim() + "~" + taxyearid[6].Text.Trim();
                                gc.insert_date(orderNumber, Parcel_Number, 956, taxyearresult, 1, DateTime.Now);
                            }
                            if (taxyear.Text.Contains("Totals"))
                            {
                                string taxyearresult = yearswitch + "~" + "" + "~" + taxyearid[1].Text.Trim() + "~" + taxyearid[2].Text.Trim() + "~" + "" + "~" + taxyearid[4].Text.Trim() + "~" + "" + "~" + taxyearid[6].Text.Trim();
                                gc.insert_date(orderNumber, Parcel_Number, 956, taxyearresult, 1, DateTime.Now);
                            }
                            if (taxyear.Text.Contains("Date Paid") && !taxyear.Text.Contains("District"))
                            {
                                string taxyearresult = yearswitch + "~" + "" + "~" + taxyearid[1].Text.Trim() + "~" + "" + "~" + taxyearid[2].Text.Trim() + "~" + "" + "~" + taxyearid[3].Text.Trim() + "~" + "";
                                gc.insert_date(orderNumber, Parcel_Number, 956, taxyearresult, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }
                    driver.SwitchTo().Window(current);

                    //Delinquent Tax Information Details
                    string delinquenttax = "";
                    string InformationComments = "";
                    try
                    {
                        try
                        {
                            delinquenttax = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table[3]/tbody/tr[14]/td/center/a[2]")).Text.Trim();
                        }
                        catch { }
                        try
                        {
                            delinquenttax = driver.FindElement(By.XPath("//html/body/table/tbody/tr/td/table[3]/tbody/tr[14]/td/center/a[3]/font")).Text.Trim();
                        }
                        catch { }
                        try
                        {
                            delinquenttax = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table[3]/tbody/tr[15]/td/center/a[2]")).Text.Trim();
                        }
                        catch { }

                        if (delinquenttax.Contains("Current records indicate delinquent tax status click here for more information."))
                        {

                            InformationComments = "Contact the Yellowstone County Treasurer's Office, (406) 256-2802, for complete payoff information.";
                            string alertmessage = InformationComments;
                            gc.insert_date(orderNumber, Parcel_Number, 1014, alertmessage, 1, DateTime.Now);
                        }
                    }
                    catch { }

                    string taxurl = "";
                    try
                    {

                        try
                        {
                            taxurl = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table[3]/tbody/tr[14]/td/center/a[2]")).GetAttribute("href");
                            driver.Navigate().GoToUrl(taxurl);
                            Thread.Sleep(5000);
                            gc.CreatePdf(orderNumber, Parcel_Number, "Delinquent Details Click", driver, "MT", "Yellowstone");
                        }
                        catch { }
                        try
                        {
                            taxurl = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table[3]/tbody/tr[15]/td/center/a[2]")).GetAttribute("href");
                            driver.Navigate().GoToUrl(taxurl);
                            Thread.Sleep(5000);
                            gc.CreatePdf(orderNumber, Parcel_Number, "Delinquent Details Click", driver, "MT", "Yellowstone");
                        }
                        catch { };


                        string bulkdata1 = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table[2]/tbody/tr[3]/td")).Text.Trim();
                        string TaxID1 = gc.Between(bulkdata1, "Tax ID:", "\r\n").Trim();
                        string Title = GlobalClass.After(bulkdata1, "\r\n").Trim();
                        string DelinquentPaymentdetails = "";

                        IWebElement delinquenttabledata = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table[3]/tbody"));
                        IList<IWebElement> delinquenttabledataTR = delinquenttabledata.FindElements(By.TagName("tr"));
                        IList<IWebElement> delinquenttabledataTD;

                        foreach (IWebElement PaymentTax in delinquenttabledataTR)
                        {
                            delinquenttabledataTD = PaymentTax.FindElements(By.TagName("td"));
                            if (delinquenttabledataTD.Count != 0 && !PaymentTax.Text.Contains("Year"))
                            {
                                string delinquentyear = delinquenttabledataTD[0].Text;
                                string delinqhalf = delinquenttabledataTD[1].Text;
                                string delinqamount = delinquenttabledataTD[2].Text;
                                string delinqpenalt = delinquenttabledataTD[3].Text;
                                string delinqinterest = delinquenttabledataTD[4].Text;
                                string delinqTotal = delinquenttabledataTD[5].Text;

                                DelinquentPaymentdetails = TaxID1 + "~" + Title + "~" + delinquentyear + "~" + delinqhalf + "~" + delinqamount + "~" + delinqpenalt + "~" + delinqinterest + "~" + delinqTotal;
                                gc.insert_date(orderNumber, Parcel_Number, 992, DelinquentPaymentdetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }


                    string date = "", GoodThroughDate = "";
                    try
                    {
                        IWebElement dt = driver.FindElement(By.XPath("/html/body/center/form/input[1]"));
                        date = dt.GetAttribute("value");

                        DateTime G_Date = Convert.ToDateTime(date);
                        string dateChecking = DateTime.Now.ToString("MM") + "/15/" + DateTime.Now.ToString("yyyy");

                        if (G_Date < Convert.ToDateTime(dateChecking))
                        {
                            //end of the month
                            date = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")))).ToString("MM/dd/yyyy");

                        }
                        else if (G_Date > Convert.ToDateTime(dateChecking))
                        {
                            // nextEndOfMonth 
                            if ((Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))) < 12))
                            {
                                date = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM")) + 1), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")) + 1)).ToString("MM/dd/yyyy");
                            }
                            else
                            {
                                int nxtYr = Convert.ToInt16(DateTime.Now.ToString("yyyy")) + 1;
                                date = new DateTime(nxtYr, 1, DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), 1)).ToString("MM/dd/yyyy");

                            }
                        }
                        Thread.Sleep(2000);
                        dt.Clear();
                        GoodThroughDate = date;
                        IWebElement dateto = driver.FindElement(By.XPath("/html/body/center/form/input[1]"));
                        dateto.Clear();
                        driver.FindElement(By.XPath("/html/body/center/form/input[1]")).SendKeys(GoodThroughDate);
                        driver.FindElement(By.XPath("/html/body/center/form/input[2]")).Click();
                        Thread.Sleep(2000);


                        try
                        {
                            string bulkdata1 = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table[2]/tbody/tr[3]/td")).Text.Trim();
                            string TaxID2 = gc.Between(bulkdata1, "Tax ID:", "\r\n").Trim();
                            string Title = GlobalClass.After(bulkdata1, "\r\n").Trim();
                            string DelinquentPaymentdetails = "";

                            IWebElement delinquenttabledata = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table[3]/tbody"));
                            IList<IWebElement> delinquenttabledataTR = delinquenttabledata.FindElements(By.TagName("tr"));
                            IList<IWebElement> delinquenttabledataTD;

                            foreach (IWebElement PaymentTax in delinquenttabledataTR)
                            {
                                delinquenttabledataTD = PaymentTax.FindElements(By.TagName("td"));
                                if (delinquenttabledataTD.Count != 0 && !PaymentTax.Text.Contains("Year"))
                                {
                                    string delinquentyear = delinquenttabledataTD[0].Text;
                                    string delinqhalf = delinquenttabledataTD[1].Text;
                                    string delinqamount = delinquenttabledataTD[2].Text;
                                    string delinqpenalt = delinquenttabledataTD[3].Text;
                                    string delinqinterest = delinquenttabledataTD[4].Text;
                                    string delinqTotal = delinquenttabledataTD[5].Text;

                                    DelinquentPaymentdetails = TaxID2 + "~" + Title + "~" + delinquentyear + "~" + delinqhalf + "~" + delinqamount + "~" + delinqpenalt + "~" + delinqinterest + "~" + delinqTotal;
                                    gc.insert_date(orderNumber, Parcel_Number, 992, DelinquentPaymentdetails, 1, DateTime.Now);
                                }
                            }
                        }
                        catch { }

                    }
                    catch { }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Quit();
                    gc.mergpdf(orderNumber, "MT", "Yellowstone");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "MT", "Yellowstone", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
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