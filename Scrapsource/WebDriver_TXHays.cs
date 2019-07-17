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
using System.Web.UI;
using System.Web;

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_TXHays
    {
        string parcel = "", aaa = "", Parcelno = "", Multi_Type = "", Owner = "", Property_Address = "", MultiAddress_details = "";
        string Parcel_ID = "", Legal_Desp = "", Graphic_Id = "", Type = "", Pro_Addrs = "", Map = "", Nighberhood = "", Owner_Id = "", Name = "", Mailing_Address = "", Excemptions = "", Acres = "", Year_Built = "";
        string Parcel = "", Legal = "", Graphic = "", Tap = "", Adds = "", Maps = "", Nighber = "", Own_Id = "", Nam = "", Mailing_Addrs = "", Exmp = "", Property_Details = "";
        string Imp_Home = "", Imp_NonHome = "", Land = "", Non_Land = "", Agricutr = "", Mrkt = "", Ag_Value = "", App_Value = "", Home_Cap = "", Assd_Value = "", Imp_Homesite = "", Imp_NonHomesite = "", Land_Homesite = "", Land_NonHomesite = "", Agri_Markt = "", Mrkt_Value = "", AgUse_Value = "", Appraised_Value = "", Homested_Cap = "", Assessed_Value = "", Assessment_Details = "";
        string Entity = "", Desp = "", Mrket_Value = "", Jurisdiction_details = "";
        string Owners = "", year = "", Quiref_Id = "", Situs_Address = "", Status = "", Payment_details = "", Year = "", Improv = "", lnd_Mrkt = "", Ag_Valution = "", Appraised = "", HS_Cap = "", Pro_Assed = "", RollValue_details = "", Txble_Value = "";
        string Jurisdiction = "", T_Year = "", Base_Tax = "", tax_Penalty = "", Tax_Inst = "", attroney_Fee = "", Payments = "", Balnce = "", Breakdown_details = "";
        string Tax_Deatils = "", Tax_OwnerAddress = "", Taxy_Parcel = "", taxy_QuickID = "", Taxy_LglDes = "", Tax_Owner = "", Taxy_Year = "", taxy_sta = "", Taxy_Base = "", Tl_Taxy = "", TaxyTl_Due = "", Lega = "", Quick = "", Tx_Parcel = "", Own_Addr = "", Due = "", Tl_Pay = "", Ba_Tx = "", Sta = "", Tx_year = "", Own_Nm = "", TaxSale_Comments = "";
        string Taxing = "", Phone = "", Fax = "", Taxing_Authority = "", Taxauthority_Details = "";

        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_TXHays(string streetno, string direction, string streetname, string city, string streettype, string unitnumber, string ownernm, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new PhantomJSDriver();
            //driver = new ChromeDriver();
            using (driver = new PhantomJSDriver()) //ChromeDriver
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    if (searchType == "titleflex")
                    {
                        string titleaddress = streetno + " " + streetname + " " + streettype + " " + unitnumber;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "TX", "Hays");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_TXHays"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("http://esearch.hayscad.com/");
                        Thread.Sleep(2000);

                        IJavaScriptExecutor js2 = driver as IJavaScriptExecutor;
                        IWebElement ParcelLinkSearch1 = driver.FindElement(By.XPath("//*[@id='home-page-tabs']/li[3]/a"));
                        js2.ExecuteScript("arguments[0].click();", ParcelLinkSearch1);
                        Thread.Sleep(2000);

                        driver.FindElement(By.XPath("//*[@id='StreetNumber']")).SendKeys(streetno);
                        driver.FindElement(By.XPath("//*[@id='StreetName']")).SendKeys(streetname);

                        driver.FindElement(By.XPath("//*[@id='index-search']/div[4]/div/div/button")).SendKeys(Keys.Enter);
                        Thread.Sleep(5000);

                        aaa = driver.FindElement(By.XPath("//*[@id='page-header']")).Text;
                        parcel = gc.Between(aaa, "Page 1 of 1 - ", " (").Trim();

                        if (parcel == "Total: 1")
                        {
                            gc.CreatePdf_WOP(orderNumber, "Owner", driver, "TX", "Hays");
                            var chDriver = new ChromeDriver();
                            chDriver.Navigate().GoToUrl(driver.Url);
                            Thread.Sleep(4000);

                            IWebElement Multiaddresstable = chDriver.FindElement(By.XPath("//*[@id='grid']/div[2]/table/tbody"));
                            IList<IWebElement> multiaddressrow = Multiaddresstable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Multiaddressid;
                            foreach (IWebElement Multiaddress in multiaddressrow)
                            {
                                Multiaddressid = Multiaddress.FindElements(By.TagName("td"));
                                if (Multiaddressid.Count != 0)
                                {
                                    parcel = Multiaddressid[1].Text;
                                }
                            }
                            gc.CreatePdf_WOP(orderNumber, "Owner Address search1", driver, "TX", "Hays");
                            //ByVisibleElement(chDriver.FindElement(By.Id("DownloadResults")));
                            gc.CreatePdf_WOP(orderNumber, "Owner Address search2", driver, "TX", "Hays");
                            driver.Navigate().GoToUrl("http://esearch.hayscad.com/Property/View/" + parcel + "");
                            Thread.Sleep(2000);
                            gc.CreatePdf_WOP(orderNumber, "Owner Address search1", driver, "TX", "Hays");
                            chDriver.Quit();
                        }

                        else
                        {
                            try
                            {
                                var chDriver = new ChromeDriver();
                                chDriver.Navigate().GoToUrl(driver.Url);
                                Thread.Sleep(4000);

                                IWebElement MultiAddressTB = chDriver.FindElement(By.XPath("//*[@id='grid']/div[2]/table/tbody"));
                                IList<IWebElement> MultiAddressTR = MultiAddressTB.FindElements(By.TagName("tr"));
                                IList<IWebElement> MultiAddressTD;
                                gc.CreatePdf_WOP(orderNumber, "Multi Address search", driver, "TX", "Hays");
                                int AddressmaxCheck = 0;
                                foreach (IWebElement MultiAddress in MultiAddressTR)
                                {
                                    if (AddressmaxCheck <= 25)
                                    {
                                        MultiAddressTD = MultiAddress.FindElements(By.TagName("td"));
                                        if (MultiAddressTD.Count != 0 && !MultiAddress.Text.Contains("Mobile Home"))
                                        {
                                            Parcelno = MultiAddressTD[1].Text;
                                            Multi_Type = MultiAddressTD[5].Text;
                                            Owner = MultiAddressTD[6].Text;
                                            Property_Address = MultiAddressTD[7].Text;

                                            MultiAddress_details = Multi_Type + "~" + Owner + "~" + Property_Address;
                                            gc.insert_date(orderNumber, Parcelno, 1002, MultiAddress_details, 1, DateTime.Now);
                                        }
                                        AddressmaxCheck++;
                                    }
                                }
                                if (MultiAddressTR.Count > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_TXHays_Multicount"] = "Maximum";
                                }
                                else
                                {
                                    HttpContext.Current.Session["multiParcel_TXHays"] = "Yes";
                                }
                                chDriver.Quit();
                                return "MultiParcel";
                            }
                            catch
                            { }
                        }
                    }

                    if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("http://esearch.hayscad.com/");
                        Thread.Sleep(2000);

                        IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                        IWebElement ParcelLinkSearch = driver.FindElement(By.XPath("//*[@id='home-page-tabs']/li[4]/a"));
                        js1.ExecuteScript("arguments[0].click();", ParcelLinkSearch);
                        Thread.Sleep(2000);

                        driver.FindElement(By.Id("PropertyId")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearch", driver, "TX", "Hays");
                        driver.FindElement(By.XPath("//*[@id='index-search']/div[4]/div/div/button")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        driver.FindElement(By.XPath("//*[@id='grid']/div[2]/table/tbody/tr/td[2]")).Click();
                        Thread.Sleep(5000);
                        gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearch1", driver, "TX", "Hays");
                    }

                    if (searchType == "ownername")
                    {
                        driver.Navigate().GoToUrl("http://esearch.hayscad.com/");
                        Thread.Sleep(2000);

                        IJavaScriptExecutor js3 = driver as IJavaScriptExecutor;
                        IWebElement ParcelLinkSearch2 = driver.FindElement(By.XPath("//*[@id='home-page-tabs']/li[2]/a"));
                        js3.ExecuteScript("arguments[0].click();", ParcelLinkSearch2);
                        Thread.Sleep(2000);

                        driver.FindElement(By.Id("OwnerName")).SendKeys(ownernm);
                        gc.CreatePdf_WOP(orderNumber, "Owner search", driver, "TX", "Hays");
                        driver.FindElement(By.XPath("//*[@id='index-search']/div[4]/div/div/button")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        aaa = driver.FindElement(By.XPath("//*[@id='page-header']")).Text;
                        parcel = gc.Between(aaa, "Page 1 of 1 - ", " (").Trim();

                        if (parcel == "Total: 1")
                        {
                            gc.CreatePdf_WOP(orderNumber, "Owner", driver, "TX", "Hays");
                            var chDriver = new ChromeDriver();
                            chDriver.Navigate().GoToUrl(driver.Url);
                            Thread.Sleep(4000);

                            IWebElement Multiaddresstable = chDriver.FindElement(By.XPath("//*[@id='grid']/div[2]/table/tbody"));
                            IList<IWebElement> multiaddressrow = Multiaddresstable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Multiaddressid;
                            foreach (IWebElement Multiaddress in multiaddressrow)
                            {
                                Multiaddressid = Multiaddress.FindElements(By.TagName("td"));
                                if (Multiaddressid.Count != 0)
                                {
                                    parcel = Multiaddressid[1].Text;
                                }
                            }
                            gc.CreatePdf_WOP(orderNumber, "Owner search1", driver, "TX", "Hays");
                            //ByVisibleElement(chDriver.FindElement(By.Id("DownloadResults")));
                            gc.CreatePdf_WOP(orderNumber, "Owner Search2", driver, "TX", "Hays");
                            driver.Navigate().GoToUrl("http://esearch.hayscad.com/Property/View/" + parcel + "");
                            Thread.Sleep(2000);
                            gc.CreatePdf_WOP(orderNumber, "Owner search3", driver, "TX", "Hays");
                            chDriver.Quit();
                        }

                        else
                        {
                            try
                            {
                                var chDriver = new ChromeDriver();
                                chDriver.Navigate().GoToUrl(driver.Url);
                                Thread.Sleep(4000);

                                IWebElement MultiAddressTB = chDriver.FindElement(By.XPath("//*[@id='grid']/div[2]/table/tbody"));
                                IList<IWebElement> MultiAddressTR = MultiAddressTB.FindElements(By.TagName("tr"));
                                IList<IWebElement> MultiAddressTD;
                                gc.CreatePdf_WOP(orderNumber, "Multi Address search", driver, "TX", "Hays");
                                int AddressmaxCheck = 0;
                                foreach (IWebElement MultiAddress in MultiAddressTR)
                                {
                                    if (AddressmaxCheck <= 25)
                                    {
                                        MultiAddressTD = MultiAddress.FindElements(By.TagName("td"));
                                        if (MultiAddressTD.Count != 0 && !MultiAddress.Text.Contains("Mobile Home"))
                                        {
                                            Parcelno = MultiAddressTD[1].Text;
                                            Multi_Type = MultiAddressTD[5].Text;
                                            Owner = MultiAddressTD[6].Text;
                                            Property_Address = MultiAddressTD[7].Text;

                                            MultiAddress_details = Multi_Type + "~" + Owner + "~" + Property_Address;
                                            gc.insert_date(orderNumber, Parcelno, 1002, MultiAddress_details, 1, DateTime.Now);
                                        }
                                        AddressmaxCheck++;
                                    }
                                }
                                if (MultiAddressTR.Count > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_TXHays_Multicount"] = "Maximum";
                                }
                                else
                                {
                                    HttpContext.Current.Session["multiParcel_TXHays"] = "Yes";
                                }
                                chDriver.Quit();
                                return "MultiParcel";
                            }
                            catch
                            { }
                        }
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.Id("results-page"));
                        if (INodata.Text.Contains("Page 1 of 0 - Total: 0"))
                        {
                            HttpContext.Current.Session["Nodata_TXHays"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    //Property Details
                    try
                    {
                        IList<IWebElement> tables1 = driver.FindElements(By.XPath("//*[@id='detail-page']/div/div[1]/div/table"));
                        int count1 = tables1.Count;
                        foreach (IWebElement tab1 in tables1)
                        {
                            if (tab1.Text.Contains("Account"))
                            {
                                IList<IWebElement> ProTR = tab1.FindElements(By.TagName("tr"));
                                IList<IWebElement> ProTD;
                                IList<IWebElement> ProTH;
                                foreach (IWebElement Pro in ProTR)
                                {
                                    ProTD = Pro.FindElements(By.TagName("td"));
                                    ProTH = Pro.FindElements(By.TagName("th"));
                                    if (!Pro.Text.Contains("Account") && !Pro.Text.Contains("Location") && !Pro.Text.Contains("% Ownership:") && !Pro.Text.Contains("Agent Code:"))
                                    {
                                        Parcel = ProTH[0].Text;
                                        if (Parcel.Contains("Property ID:"))
                                        {
                                            Parcel_ID = ProTD[0].Text;
                                        }
                                        Legal = ProTH[0].Text;
                                        if (Legal.Contains("Legal Description:"))
                                        {
                                            Legal_Desp = ProTD[0].Text;
                                        }
                                        Graphic = ProTH[0].Text;
                                        if (Graphic.Contains("Geographic ID:"))
                                        {
                                            Graphic_Id = ProTD[0].Text;
                                        }
                                        Tap = ProTH[0].Text;
                                        if (Tap.Contains("Type:"))
                                        {
                                            Type = ProTD[0].Text;
                                        }
                                        Adds = ProTH[0].Text;
                                        if (Adds.Contains("Address:"))
                                        {
                                            Pro_Addrs = ProTD[0].Text;
                                        }
                                        Maps = ProTH[0].Text;
                                        if (Maps.Contains("Map ID:"))
                                        {
                                            Map = ProTD[0].Text;
                                        }
                                        Nighber = ProTH[0].Text;
                                        if (Nighber.Contains("Neighborhood CD:"))
                                        {
                                            Nighberhood = ProTD[0].Text;
                                        }
                                        Own_Id = ProTH[0].Text;
                                        if (Own_Id.Contains("Owner ID:"))
                                        {
                                            Owner_Id = ProTD[0].Text;
                                        }
                                        Nam = ProTH[0].Text;
                                        if (Nam.Contains("Name:"))
                                        {
                                            Name = ProTD[0].Text;
                                        }
                                        Mailing_Addrs = ProTH[0].Text;
                                        if (Mailing_Addrs.Contains("Mailing Address:"))
                                        {
                                            Mailing_Address = ProTD[0].Text;
                                        }
                                        Exmp = ProTH[0].Text;
                                        if (Exmp.Contains("Exemptions:"))
                                        {
                                            Excemptions = ProTD[0].Text;
                                        }
                                    }
                                }

                                try
                                {
                                    Year_Built = driver.FindElement(By.XPath("//*[@id='detail-page']/div[5]/div[2]/table[1]/tbody/tr[2]/td[4]")).Text;
                                    Acres = driver.FindElement(By.XPath("//*[@id='detail-page']/div[6]/div[2]/table/tbody/tr[2]/td[3]")).Text;
                                }
                                catch
                                { }

                                Property_Details = Legal_Desp + "~" + Graphic_Id + "~" + Type + "~" + Pro_Addrs + "~" + Map + "~" + Nighberhood + "~" + Owner_Id + "~" + Name + "~" + Mailing_Address + "~" + Excemptions + "~" + Acres + "~" + Year_Built;
                                gc.CreatePdf(orderNumber, Parcel_ID, "Property Details", driver, "TX", "Hays");
                                gc.insert_date(orderNumber, Parcel_ID, 1003, Property_Details, 1, DateTime.Now);
                            }

                        }
                    }
                    catch
                    { }

                    //Values Details   
                    try
                    {
                        IList<IWebElement> tables2 = driver.FindElements(By.XPath("//*[@id='detail-page']/div/div[2]/div/table"));
                        int count2 = tables2.Count;
                        foreach (IWebElement tab2 in tables2)
                        {
                            if (tab2.Text.Contains("Improvement Homesite Value:"))
                            {
                                IList<IWebElement> AssemntTR = tab2.FindElements(By.TagName("tr"));
                                IList<IWebElement> AssemntTD;
                                IList<IWebElement> AssemntTH;
                                foreach (IWebElement Assemnt in AssemntTR)
                                {
                                    AssemntTD = Assemnt.FindElements(By.TagName("td"));
                                    AssemntTH = Assemnt.FindElements(By.TagName("th"));

                                    if (Assemnt.Text != " ")
                                    {
                                        Imp_Home = AssemntTH[0].Text;
                                        if (Imp_Home.Contains("Improvement Homesite Value:"))
                                        {
                                            Imp_Homesite = AssemntTD[0].Text;
                                        }

                                        Imp_NonHome = AssemntTH[0].Text;
                                        if (Imp_NonHome.Contains("Improvement Non-Homesite Value:"))
                                        {
                                            Imp_NonHomesite = AssemntTD[0].Text;
                                        }

                                        Land = AssemntTH[0].Text;
                                        if (Land.Contains("Land Homesite Value:"))
                                        {
                                            Land_Homesite = AssemntTD[0].Text;
                                        }

                                        Non_Land = AssemntTH[0].Text;
                                        if (Non_Land.Contains("Land Non-Homesite Value:"))
                                        {
                                            Land_NonHomesite = AssemntTD[0].Text;
                                        }

                                        Agricutr = AssemntTH[0].Text;
                                        if (Agricutr.Contains("Agricultural Market Valuation:"))
                                        {
                                            Agri_Markt = AssemntTD[0].Text;
                                        }

                                        Mrkt = AssemntTH[0].Text;
                                        if (Mrkt.Contains("Market Value:"))
                                        {
                                            Mrkt_Value = AssemntTD[0].Text;
                                        }

                                        Ag_Value = AssemntTH[0].Text;
                                        if (Ag_Value.Contains("Ag Use Value:"))
                                        {
                                            AgUse_Value = AssemntTD[0].Text;
                                        }

                                        App_Value = AssemntTH[0].Text;
                                        if (App_Value.Contains("Appraised Value:"))
                                        {
                                            Appraised_Value = AssemntTD[0].Text;
                                        }

                                        Home_Cap = AssemntTH[0].Text;
                                        if (Home_Cap.Contains("Homestead Cap Loss:"))
                                        {
                                            Homested_Cap = AssemntTD[0].Text;
                                        }

                                        Assd_Value = AssemntTH[0].Text;
                                        if (Assd_Value.Contains("Assessed Value:"))
                                        {
                                            Assessed_Value = AssemntTD[0].Text;
                                        }
                                    }
                                }
                                Assessment_Details = Imp_Homesite + "~" + Imp_NonHomesite + "~" + Land_Homesite + "~" + Land_NonHomesite + "~" + Agri_Markt + "~" + Mrkt_Value + "~" + AgUse_Value + "~" + Appraised_Value + "~" + Homested_Cap + "~" + Assessed_Value;
                                gc.insert_date(orderNumber, Parcel_ID, 1006, Assessment_Details, 1, DateTime.Now);
                            }
                        }
                    }
                    catch
                    { }
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    //Jurisdiction Details 

                    try
                    {
                        IWebElement JurisdictionTB = driver.FindElement(By.XPath("//*[@id='detail-page']/div[4]/div[2]/table/tbody"));
                        IList<IWebElement> JurisdictionTR = JurisdictionTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> JurisdictionTD;

                        foreach (IWebElement Jurisdiction in JurisdictionTR)
                        {
                            JurisdictionTD = Jurisdiction.FindElements(By.TagName("td"));
                            if (JurisdictionTD.Count != 0 && !Jurisdiction.Text.Contains("Entity"))
                            {
                                Entity = JurisdictionTD[0].Text;
                                Desp = JurisdictionTD[1].Text;
                                Mrket_Value = JurisdictionTD[2].Text;
                                Txble_Value = JurisdictionTD[3].Text;

                                Jurisdiction_details = Entity + "~" + Desp + "~" + Mrket_Value + "~" + Txble_Value;
                                gc.insert_date(orderNumber, Parcel_ID, 1007, Jurisdiction_details, 1, DateTime.Now);
                            }
                        }
                    }
                    catch
                    { }

                    //RollValue Details                                       
                    try
                    {
                        IList<IWebElement> tables4 = driver.FindElements(By.XPath("//*[@id='detail-page']/div/div[2]/table"));
                        int count4 = tables4.Count;
                        foreach (IWebElement tab4 in tables4)
                        {
                            if (tab4.Text.Contains("HS Cap Loss"))
                            {
                                IList<IWebElement> RollValueTR = tab4.FindElements(By.TagName("tr"));
                                IList<IWebElement> RollValueTD;

                                foreach (IWebElement RollValue in RollValueTR)
                                {
                                    RollValueTD = RollValue.FindElements(By.TagName("td"));
                                    if (RollValueTD.Count != 0 && !RollValue.Text.Contains("Year"))
                                    {
                                        Year = RollValueTD[0].Text;
                                        Improv = RollValueTD[1].Text;
                                        lnd_Mrkt = RollValueTD[2].Text;
                                        Ag_Valution = RollValueTD[3].Text;
                                        Appraised = RollValueTD[4].Text;
                                        HS_Cap = RollValueTD[5].Text;
                                        Pro_Assed = RollValueTD[6].Text;

                                        RollValue_details = Year + "~" + Improv + "~" + lnd_Mrkt + "~" + Ag_Valution + "~" + Appraised + "~" + HS_Cap + "~" + Pro_Assed;
                                        gc.insert_date(orderNumber, Parcel_ID, 1008, RollValue_details, 1, DateTime.Now);
                                    }
                                }
                            }
                        }
                    }
                    catch
                    { }

                    //Tax Details
                    driver.Navigate().GoToUrl("https://hayscountytax.com/taxes.html#/WildfireSearch");
                    Thread.Sleep(2000);

                    try
                    {
                        driver.FindElement(By.Id("btnAccept")).Click();
                        Thread.Sleep(2000);
                    }
                    catch
                    { }

                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[3]/button[1]")).Click();
                        Thread.Sleep(2000);
                    }
                    catch
                    { }

                    driver.FindElement(By.Id("searchBox")).SendKeys(Parcel_ID);
                    Thread.Sleep(5000);

                    //TaxPayment Details
                    try
                    {
                        IWebElement TaxPaymentTB = null;
                        try
                        {
                            TaxPaymentTB = driver.FindElement(By.XPath("//*[@id='avalon']/div/div[3]/div[2]/table/tbody"));
                        }
                        catch { }
                        try
                        {
                            if (TaxPaymentTB == null)
                            {
                                TaxPaymentTB = driver.FindElement(By.XPath("//*[@id='avalon']/div/div[4]/div[2]/table/tbody"));
                            }
                        }
                        catch { }
                        IList<IWebElement> TaxPaymentTR = TaxPaymentTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> TaxPaymentTD;

                        foreach (IWebElement TaxPayment in TaxPaymentTR)
                        {
                            TaxPaymentTD = TaxPayment.FindElements(By.TagName("td"));
                            if (TaxPaymentTD.Count != 0)
                            {
                                Owners = TaxPaymentTD[0].Text;
                                year = TaxPaymentTD[1].Text;
                                Quiref_Id = TaxPaymentTD[2].Text;
                                Situs_Address = TaxPaymentTD[3].Text;
                                Status = TaxPaymentTD[4].Text;

                                Payment_details = Owners + "~" + year + "~" + Quiref_Id + "~" + Situs_Address + "~" + Status;
                                gc.CreatePdf(orderNumber, Parcel_ID, "Tax Payment Details", driver, "TX", "Hays");
                                gc.insert_date(orderNumber, Parcel_ID, 1009, Payment_details, 1, DateTime.Now);
                            }
                        }
                    }
                    catch
                    { }

                    //Tax Info Details
                    int p1 = 0, p2 = 0, p3 = 0;
                    List<string> strTaxRealestate = new List<string>();
                    List<IWebElement> strTaxRealestate1 = new List<IWebElement>();
                    IWebElement ITaxReal1 = null;
                    try
                    {
                        ITaxReal1 = driver.FindElement(By.XPath("//*[@id='avalon']/div/div[3]/div[2]/table/tbody"));
                    }
                    catch { }
                    try
                    {
                        if (ITaxReal1 == null)
                        {
                            ITaxReal1 = driver.FindElement(By.XPath("//*[@id='avalon']/div/div[4]/div[2]/table/tbody"));
                        }
                    }
                    catch { }
                    IList<IWebElement> ITaxRealRow1 = ITaxReal1.FindElements(By.TagName("tr"));
                    IList<IWebElement> ITaxRealTd1;

                    foreach (IWebElement row in ITaxRealRow1)
                    {
                        ITaxRealTd1 = row.FindElements(By.TagName("td"));
                        if (row.Text.Contains("Unpaid"))
                        {
                            p1++;
                            p2++;
                        }
                        if (row.Text.Contains("Paid") && p2 < 3)
                        {
                            p3++;
                            p2++;
                        }

                    }
                    int p4 = p1 + p3;

                    for (int p5 = 1; p5 <= p4; p5++)
                    {
                        try
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//*[@id='avalon']/div/div[3]/div[2]/table/tbody/tr[" + p5 + "]/td[7]/button")).Click();
                            }
                            catch { }
                            try
                            {
                                driver.FindElement(By.XPath("//*[@id='avalon']/div/div[4]/div[2]/table/tbody/tr[" + p5 + "]/td[7]/button")).Click();
                            }
                            catch { }
                            Thread.Sleep(6000);

                            IWebElement TaxTB = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[1]/div[1]/table/tbody"));
                            IList<IWebElement> TaxTR = TaxTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> TaxTD;
                            foreach (IWebElement Tax in TaxTR)
                            {

                                TaxTD = Tax.FindElements(By.TagName("td"));
                                if (TaxTD.Count != 0)
                                {
                                    Own_Nm = TaxTD[0].Text;
                                    if (Own_Nm.Contains("Owner Name"))
                                    {
                                        Tax_Owner = TaxTD[1].Text;
                                    }
                                    Tx_year = TaxTD[0].Text;
                                    if (Tx_year.Contains("Tax Year"))
                                    {
                                        Taxy_Year = TaxTD[1].Text;
                                    }
                                    Sta = TaxTD[0].Text;
                                    if (Sta.Contains("Status"))
                                    {
                                        taxy_sta = TaxTD[1].Text;
                                    }
                                    Ba_Tx = TaxTD[0].Text;
                                    if (Ba_Tx.Contains("Base Tax"))
                                    {
                                        Taxy_Base = TaxTD[1].Text;
                                    }
                                    Tl_Pay = TaxTD[0].Text;
                                    if (Tl_Pay.Contains("Total Payments"))
                                    {
                                        Tl_Taxy = TaxTD[1].Text;
                                    }
                                    Due = TaxTD[0].Text;
                                    if (Due.Contains("Total Due"))
                                    {
                                        TaxyTl_Due = TaxTD[1].Text;
                                    }
                                }
                            }
                            gc.CreatePdf(orderNumber, Parcel_ID, "Tax Overview Details" + Taxy_Year, driver, "TX", "Hays");

                            IWebElement TaxTB1 = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[1]/div[2]/table/tbody"));
                            IList<IWebElement> TaxTR1 = TaxTB1.FindElements(By.TagName("tr"));
                            IList<IWebElement> TaxTD1;
                            foreach (IWebElement Tax1 in TaxTR1)
                            {

                                TaxTD1 = Tax1.FindElements(By.TagName("td"));
                                if (TaxTD1.Count != 0)
                                {
                                    Own_Addr = TaxTD1[0].Text;
                                    if (Own_Addr.Contains("Owner Address"))
                                    {
                                        Tax_OwnerAddress = TaxTD1[1].Text;
                                    }
                                    Tx_Parcel = TaxTD1[0].Text;
                                    if (Tx_Parcel.Contains("Parcel ID"))
                                    {
                                        Taxy_Parcel = TaxTD1[1].Text;
                                    }
                                    Quick = TaxTD1[0].Text;
                                    if (Quick.Contains("Quick Reference ID"))
                                    {
                                        taxy_QuickID = TaxTD1[1].Text;
                                    }
                                    Lega = TaxTD1[0].Text;
                                    if (Lega.Contains("Legal Description"))
                                    {
                                        Taxy_LglDes = TaxTD1[1].Text;
                                    }
                                }
                            }

                            if (TaxyTl_Due == "Please call tax office for total due")
                            {
                                TaxSale_Comments = "You must call the Collector's Office.";
                            }
                            Tax_Deatils = Taxy_Year + "~" + Tax_OwnerAddress + "~" + Taxy_Parcel + "~" + taxy_QuickID + "~" + Taxy_LglDes + "~" + Tax_Owner + "~" + taxy_sta + "~" + Taxy_Base + "~" + Tl_Taxy + "~" + TaxyTl_Due + "~" + TaxSale_Comments;
                            gc.insert_date(orderNumber, Parcel_ID, 1011, Tax_Deatils, 1, DateTime.Now);

                            //Tax Breakdown
                            try
                            {
                                IWebElement BreakdownTB = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[2]/div/table/tbody"));
                                IList<IWebElement> BreakdownTR = BreakdownTB.FindElements(By.TagName("tr"));
                                IList<IWebElement> BreakdownTD;

                                foreach (IWebElement Breakdown in BreakdownTR)
                                {
                                    BreakdownTD = Breakdown.FindElements(By.TagName("td"));
                                    if (BreakdownTD.Count != 0)
                                    {
                                        Jurisdiction = BreakdownTD[0].Text;
                                        T_Year = BreakdownTD[1].Text;
                                        Base_Tax = BreakdownTD[2].Text;
                                        tax_Penalty = BreakdownTD[3].Text;
                                        Tax_Inst = BreakdownTD[4].Text;
                                        attroney_Fee = BreakdownTD[5].Text;
                                        Payments = BreakdownTD[6].Text;
                                        Balnce = BreakdownTD[7].Text;

                                        Breakdown_details = Jurisdiction + "~" + T_Year + "~" + Base_Tax + "~" + tax_Penalty + "~" + Tax_Inst + "~" + attroney_Fee + "~" + Payments + "~" + Balnce;
                                        gc.insert_date(orderNumber, Parcel_ID, 1010, Breakdown_details, 1, DateTime.Now);
                                    }
                                }
                            }
                            catch
                            { }

                            //Tax Bill
                            try
                            {
                                driver.FindElement(By.XPath("//*[@id='avalon']/div/div/ul/li[2]/a")).Click();
                                Thread.Sleep(5000);

                                gc.CreatePdf(orderNumber, Parcel_ID, "Tax Bill Details" + Taxy_Year, driver, "TX", "Hays");
                            }
                            catch
                            { }
                            try
                            {
                                driver.FindElement(By.XPath("//*[@id='avalon']/div/div/ul/li[3]/a")).Click();
                                Thread.Sleep(4000);

                                gc.CreatePdf(orderNumber, Parcel_ID, "Tax Receipt Details" + Taxy_Year, driver, "TX", "Hays");
                            }
                            catch
                            { }
                            try
                            {
                                driver.FindElement(By.XPath("//*[@id='avalon']/div/div/ul/li[4]/a")).Click();
                                Thread.Sleep(4000);

                                gc.CreatePdf(orderNumber, Parcel_ID, "Tax Summary Details" + Taxy_Year, driver, "TX", "Hays");
                            }
                            catch
                            { }

                            driver.Navigate().Back();
                            Thread.Sleep(3000);
                        }
                        catch
                        { }
                    }

                    //Tax Authority
                    driver.Navigate().GoToUrl("https://hayscountytax.com/#/contact");
                    Thread.Sleep(2000);
                    try
                    {
                        Taxing = "712 S Stagecoach Trail San Marcos, Texas 78666";
                        Phone = "Phone: (512) 393-5545";
                        Fax = "Fax: (512) 393-5547";

                        gc.CreatePdf(orderNumber, Parcel_ID, "Tax Authority Details", driver, "TX", "Hays");
                        Taxing_Authority = Taxing + " " + Phone + " " + Fax;

                        Taxauthority_Details = Taxing_Authority;
                        gc.insert_date(orderNumber, Parcel_ID, 1012, Taxauthority_Details, 1, DateTime.Now);
                    }
                    catch
                    { }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "TX", "Hays", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "TX", "Hays");
                    return "Data Inserted Successfully";
                }
                catch (Exception ex)
                {
                    driver.Quit();
                    GlobalClass.LogError(ex, orderNumber);
                    throw;
                }
            }
        }
        public void TaxDetails(string orderNumber, string parcelNumber)
        {

        }
        public void ByVisibleElement(IWebElement Element)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].scrollIntoView();", Element);
        }
    }
}