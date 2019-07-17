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
    public class WebDriver_TXEllis
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_TXEllis(string streetno, string direction, string streetname, string city, string streettype, string unitnumber, string ownernm, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            string Parcel_ID = "", Legal_Desp = "", Graphic_Id = "", Type = "", Pro_Addrs = "", Map = "", Nighberhood = "", Owner_Id = "", Name = "", Mailing_Address = "", Excemptions = "", Acres = "", Year_Built = "";
            string Parcel = "", Legal = "", Graphic = "", Tap = "", Adds = "", Maps = "", Nighber = "", Own_Id = "", Nam = "", Mailing_Addrs = "", Exmp = "";
            string ChkMulti = "", Parcelno = "", Multi_Type = "", Owner = "", Property_Address = "", MultiAddress_details = "", Property_Details = "", MultiOwner_details = "";
            string Imp_Home = "", Imp_NonHome = "", Land = "", Non_Land = "", Agricutr = "", Mrkt = "", Ag_Value = "", App_Value = "", Home_Cap = "", Assd_Value = "", Imp_Homesite = "", Imp_NonHomesite = "", Land_Homesite = "", Land_NonHomesite = "", Agri_Markt = "", Mrkt_Value = "", AgUse_Value = "", Appraised_Value = "", Homested_Cap = "", Assessed_Value = "", Assessment_Details = "";
            string Entity = "", Desp = "", Tax_Rate = "", Mrket_Value = "", Taxable_Value = "", Estimated_taxes = "", Freeze_Ceiling = "", Jurisdiction_details = "";
            string tax_rate1 = "", tax_rate2 = "", tax_rate3 = "", Total_TaxRate1 = "", Total_TaxRate2 = "", Total_TaxRate3 = "", Jurisdiction_details1 = "", Year = "", Improv = "", lnd_Mrkt = "", Ag_Valution = "", Appraised = "", HS_Cap = "", Pro_Assed = "", RollValue_details = "";
            string Acct_Number = "", Tax_Owner_Address = "", Pro_SiteAddr = "", Legai_desp = "", CurrentTax_Levy = "", CurrentTax_Due = "", PriorAmount_Due = "", Tlt_Due = "", LastPayment_AmountforCurrentYearTaxes = "", LastPayer_AmountforCurrentYearTaxes = "", LastPayment_DateforCurrentYearTaxes = "", Active_Law = "", Pending_Credit = "", Gross_Value = "", Land_Value = "", Impr_Value = "", Capped_Value = "", Aggricultr_Law = "", Tax_Excemptions = "", TaxBill_Details = "";
            string Tax_JudistionYear = "", Tax_ExmpYear = "", Jurisdictions = "", TaxMKT_Val = "", TaxEXEMPt_Val = "", TaxBle_Val = "", TaxJu_Rate = "", Leavy = "", Tax_Judistion_details = "", Tax_Judistion_details1 = "";
            string Recipt_Date = "", Amount = "", Tax_year = "", Taxing_Desp = "", Payer = "", Tax_Payment_details = "", Mailing_Add = "", Tele1 = "", Tele2 = "", TaxAuthority = "", TaxAuthority_Details = "";
            string Tax_Active_Law = "", TaxesDue_details1 = "", Taxy_Year1 = "", byend_ofSep = "", byend_ofOct = "", byend_ofNov = "", TaxesDue_details2 = "", Taxy_Year = "", BaseTax_Due = "", Penalty_Inter = "", TolTax_Due = "", PenaltyACC_Inter = "", PenaltyAC_Inter = "", TTax_Due = "", TTTax_Due = "", TaxesDue_details = "", urlListTaxBills = "", TaxPrior = "";
            string Grt = "", Lav = "", Imp_vl = "", Cap_vl = "", Agr_vl = "", Tax_vl = "", parcel = "", aaa = "";

            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {

                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    if (searchType == "titleflex")
                    {
                        string address = streetno + " " + streetname + " " + unitnumber;
                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "TX", "Ellis");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_TXEills"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("http://esearch.elliscad.com/");
                        Thread.Sleep(2000);

                        IJavaScriptExecutor js2 = driver as IJavaScriptExecutor;
                        IWebElement ParcelLinkSearch1 = driver.FindElement(By.XPath("//*[@id='home-page-tabs']/li[3]/a"));
                        js2.ExecuteScript("arguments[0].click();", ParcelLinkSearch1);
                        Thread.Sleep(2000);

                        driver.FindElement(By.XPath("//*[@id='StreetNumber']")).SendKeys(streetno);
                        driver.FindElement(By.XPath("//*[@id='StreetName']")).SendKeys(streetname);

                        driver.FindElement(By.XPath("//*[@id='index-search']/div[4]/div/div/button")).SendKeys(Keys.Enter);
                        Thread.Sleep(5000);

                        ChkMulti = driver.FindElement(By.XPath("//*[@id='page-header']")).Text;
                        ChkMulti = WebDriverTest.Before(ChkMulti, " (");

                        if (ChkMulti == "Page 1 of 1 - Total: 1")
                        {
                            aaa = driver.FindElement(By.XPath("//*[@id='results']")).Text;
                            parcel = gc.Between(aaa, "Property ID: ", "\r\nYear").Trim();
                            driver.Navigate().GoToUrl("http://esearch.elliscad.com/Property/View/" + parcel + "");
                            gc.CreatePdf_WOP(orderNumber, "Single Address search", driver, "TX", "Ellis");
                        }
                        else
                        {
                            try
                            {
                                IWebElement MultiAddressTB = driver.FindElement(By.XPath("//*[@id='grid']/div[2]/table/tbody"));
                                IList<IWebElement> MultiAddressTR = MultiAddressTB.FindElements(By.TagName("tr"));
                                IList<IWebElement> MultiAddressTD;
                                gc.CreatePdf_WOP(orderNumber, "Multi Address search", driver, "TX", "Ellis");
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
                                            gc.insert_date(orderNumber, Parcelno, 818, MultiAddress_details, 1, DateTime.Now);
                                        }
                                        AddressmaxCheck++;
                                    }
                                }
                                if (MultiAddressTR.Count > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_TXEllis_Multicount"] = "Maximum";
                                }
                                else
                                {
                                    HttpContext.Current.Session["multiParcel_TXEllis"] = "Yes";
                                }
                                driver.Quit();
                                return "MultiParcel";
                            }
                            catch
                            { }
                        }
                    }

                    if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("http://esearch.elliscad.com/");
                        Thread.Sleep(2000);

                        IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                        IWebElement ParcelLinkSearch = driver.FindElement(By.XPath("//*[@id='home-page-tabs']/li[4]/a"));
                        js1.ExecuteScript("arguments[0].click();", ParcelLinkSearch);
                        Thread.Sleep(2000);

                        driver.FindElement(By.Id("PropertyId")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearch", driver, "TX", "Ellis");
                        driver.FindElement(By.XPath("//*[@id='index-search']/div[4]/div/div/button")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='grid']/div[2]/table/tbody/tr/td[2]")).Click();
                            Thread.Sleep(5000);
                        }
                        catch { }
                    }

                    if (searchType == "ownername")
                    {
                        driver.Navigate().GoToUrl("http://esearch.elliscad.com/");
                        Thread.Sleep(2000);

                        IJavaScriptExecutor js3 = driver as IJavaScriptExecutor;
                        IWebElement ParcelLinkSearch2 = driver.FindElement(By.XPath("//*[@id='home-page-tabs']/li[2]/a"));
                        js3.ExecuteScript("arguments[0].click();", ParcelLinkSearch2);
                        Thread.Sleep(2000);

                        driver.FindElement(By.Id("OwnerName")).SendKeys(ownernm);
                        gc.CreatePdf_WOP(orderNumber, "Owner search", driver, "TX", "Ellis");
                        driver.FindElement(By.XPath("//*[@id='index-search']/div[4]/div/div/button")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        ChkMulti = driver.FindElement(By.XPath("//*[@id='page-header']")).Text;
                        ChkMulti = WebDriverTest.Before(ChkMulti, " (");

                        if (ChkMulti == "Page 1 of 1 - Total: 1")
                        {
                            aaa = driver.FindElement(By.XPath("//*[@id='results']")).Text;
                            parcel = gc.Between(aaa, "Property ID: ", "\r\nYear").Trim();
                            driver.Navigate().GoToUrl("http://esearch.elliscad.com/Property/View/" + parcel + "");
                            gc.CreatePdf_WOP(orderNumber, "Single Owner search", driver, "TX", "Ellis");
                        }
                        else
                        {
                            try
                            {
                                IWebElement MultiAddressTB = driver.FindElement(By.XPath("//*[@id='grid']/div[2]/table/tbody"));
                                IList<IWebElement> MultiAddressTR = MultiAddressTB.FindElements(By.TagName("tr"));
                                IList<IWebElement> MultiAddressTD;
                                gc.CreatePdf_WOP(orderNumber, "Multi Owner search", driver, "TX", "Ellis");
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
                                            gc.insert_date(orderNumber, Parcelno, 818, MultiAddress_details, 1, DateTime.Now);
                                        }
                                        AddressmaxCheck++;
                                    }
                                }
                                if (MultiAddressTR.Count > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_TXEllis_Multicount"] = "Maximum";
                                }
                                else
                                {
                                    HttpContext.Current.Session["multiParcel_TXEllis"] = "Yes";
                                }
                                driver.Quit();
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
                            HttpContext.Current.Session["Nodata_TXEills"] = "Yes";
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
                                    Year_Built = driver.FindElement(By.XPath("//*[@id='detail-page']/div[5]/div[2]/table[1]/tbody/tr[2]/td[5]")).Text;
                                    Acres = driver.FindElement(By.XPath("//*[@id='detail-page']/div[6]/div[2]/table/tbody/tr[2]/td[3]")).Text;
                                }
                                catch
                                { }

                                Property_Details = Legal_Desp + "~" + Graphic_Id + "~" + Type + "~" + Pro_Addrs + "~" + Map + "~" + Nighberhood + "~" + Owner_Id + "~" + Name + "~" + Mailing_Address + "~" + Excemptions + "~" + Acres + "~" + Year_Built;
                                gc.CreatePdf(orderNumber, Parcel_ID, "Property Details", driver, "TX", "Ellis");
                                gc.insert_date(orderNumber, Parcel_ID, 819, Property_Details, 1, DateTime.Now);
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
                                gc.insert_date(orderNumber, Parcel_ID, 820, Assessment_Details, 1, DateTime.Now);
                            }
                        }
                    }
                    catch
                    { }
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    //Jurisdiction Details   
                    try
                    {
                        IList<IWebElement> tables3 = driver.FindElements(By.XPath("//*[@id='detail-page']/div/div[2]/table"));
                        int count3 = tables3.Count;
                        foreach (IWebElement tab3 in tables3)
                        {
                            if (tab3.Text.Contains("Entity"))
                            {

                                IList<IWebElement> JurisdictionTR = tab3.FindElements(By.TagName("tr"));
                                IList<IWebElement> JurisdictionTD;

                                foreach (IWebElement Jurisdiction in JurisdictionTR)
                                {
                                    JurisdictionTD = Jurisdiction.FindElements(By.TagName("td"));
                                    if (JurisdictionTD.Count != 0 && !Jurisdiction.Text.Contains("Entity"))
                                    {
                                        Entity = JurisdictionTD[0].Text;
                                        Desp = JurisdictionTD[1].Text;
                                        Tax_Rate = JurisdictionTD[2].Text;
                                        Mrket_Value = JurisdictionTD[3].Text;
                                        Taxable_Value = JurisdictionTD[4].Text;
                                        Estimated_taxes = JurisdictionTD[5].Text;
                                        Freeze_Ceiling = JurisdictionTD[6].Text;

                                        Jurisdiction_details = Entity + "~" + Desp + "~" + Tax_Rate + "~" + Mrket_Value + "~" + Taxable_Value + "~" + Estimated_taxes + "~" + Freeze_Ceiling;
                                        gc.insert_date(orderNumber, Parcel_ID, 822, Jurisdiction_details, 1, DateTime.Now);
                                    }
                                }

                                try
                                {
                                    tax_rate1 = driver.FindElement(By.XPath("//*[@id='detail-page']/div[4]/div[3]/strong[1]")).Text;
                                    tax_rate2 = driver.FindElement(By.XPath("//*[@id='detail-page']/div[4]/div[3]/strong[2]")).Text;
                                    tax_rate3 = driver.FindElement(By.XPath("//*[@id='detail-page']/div[4]/div[3]/strong[3]")).Text;
                                    Total_TaxRate1 = driver.FindElement(By.XPath("//*[@id='detail-page']/div[4]/div[3]")).Text;
                                    Total_TaxRate1 = WebDriverTest.Between(Total_TaxRate1, "Rate: ", " Estimated Taxes With Exemptions: ");
                                    Total_TaxRate2 = driver.FindElement(By.XPath("//*[@id='detail-page']/div[4]/div[3]")).Text;
                                    Total_TaxRate2 = WebDriverTest.Between(Total_TaxRate2, "With Exemptions: ", " Estimated Taxes");
                                    Total_TaxRate3 = driver.FindElement(By.XPath("//*[@id='detail-page']/div[4]/div[3]")).Text;
                                    Total_TaxRate3 = WebDriverTest.After(Total_TaxRate3, "Without Exemptions: ");

                                    Jurisdiction_details1 = tax_rate1 + "~" + Total_TaxRate1 + "~" + tax_rate2 + "~" + Total_TaxRate2 + "~" + tax_rate3 + "~" + Total_TaxRate3 + "~" + "";
                                    gc.insert_date(orderNumber, Parcel_ID, 822, Jurisdiction_details1, 1, DateTime.Now);
                                }
                                catch
                                { }

                                try
                                {
                                    tax_rate1 = driver.FindElement(By.XPath("//*[@id='detail-page']/div[3]/div[3]/strong[1]")).Text;
                                    tax_rate2 = driver.FindElement(By.XPath("//*[@id='detail-page']/div[3]/div[3]/strong[2]")).Text;
                                    tax_rate3 = driver.FindElement(By.XPath("//*[@id='detail-page']/div[3]/div[3]/strong[3]")).Text;
                                    Total_TaxRate1 = driver.FindElement(By.XPath("//*[@id='detail-page']/div[3]/div[3]")).Text;
                                    Total_TaxRate1 = WebDriverTest.Between(Total_TaxRate1, "Rate: ", " Estimated Taxes With Exemptions: ");
                                    Total_TaxRate2 = driver.FindElement(By.XPath("//*[@id='detail-page']/div[3]/div[3]")).Text;
                                    Total_TaxRate2 = WebDriverTest.Between(Total_TaxRate2, "With Exemptions: ", " Estimated Taxes");
                                    Total_TaxRate3 = driver.FindElement(By.XPath("//*[@id='detail-page']/div[3]/div[3]")).Text;
                                    Total_TaxRate3 = WebDriverTest.After(Total_TaxRate3, "Without Exemptions: ");

                                    Jurisdiction_details1 = tax_rate1 + "~" + Total_TaxRate1 + "~" + tax_rate2 + "~" + Total_TaxRate2 + "~" + tax_rate3 + "~" + Total_TaxRate3 + "~" + "";
                                    gc.insert_date(orderNumber, Parcel_ID, 822, Jurisdiction_details1, 1, DateTime.Now);
                                }
                                catch
                                { }
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
                                        gc.insert_date(orderNumber, Parcel_ID, 824, RollValue_details, 1, DateTime.Now);
                                    }
                                }
                            }
                        }
                    }
                    catch
                    { }

                    //Tax Info
                    driver.Navigate().GoToUrl("http://actweb.acttax.com/act_webdev/ellis/index.jsp");
                    Thread.Sleep(2000);

                    driver.FindElement(By.Id("criteria")).SendKeys(Parcel_ID);
                    Thread.Sleep(2000);

                    driver.FindElement(By.XPath("//*[@id='sc4']")).Click();
                    Thread.Sleep(2000);

                    gc.CreatePdf(orderNumber, Parcel_ID, "Tax Info", driver, "TX", "Ellis");
                    driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr[1]/td/table[2]/tbody/tr/td/center/form/table/tbody/tr[5]/td[2]/h3[2]/input")).SendKeys(Keys.Enter);
                    Thread.Sleep(2000);

                    gc.CreatePdf(orderNumber, Parcel_ID, "Tax Info1", driver, "TX", "Ellis");
                    driver.FindElement(By.XPath("//*[@id='data-block']/table/tbody/tr/td/table/tbody/tr/td[1]/h3/a")).Click();
                    Thread.Sleep(2000);

                    //Tax Bill Details
                    try
                    {
                        Acct_Number = driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr[2]/td/table[2]/tbody/tr/td[1]/h3[1]")).Text;
                        Acct_Number = WebDriverTest.After(Acct_Number, "Account Number:  ");

                        Tax_Owner_Address = driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr[2]/td/table[2]/tbody/tr/td[1]/h3[2]")).Text;
                        Tax_Owner_Address = WebDriverTest.After(Tax_Owner_Address, "Address:");

                        Pro_SiteAddr = driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr[2]/td/table[2]/tbody/tr/td[1]/h3[3]")).Text;
                        Pro_SiteAddr = WebDriverTest.After(Pro_SiteAddr, "Property Site Address:");

                        Legai_desp = driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr[2]/td/table[2]/tbody/tr/td[1]/h3[4]")).Text;
                        Legai_desp = WebDriverTest.After(Legai_desp, "Legal Description:");

                        CurrentTax_Levy = driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr[2]/td/table[2]/tbody/tr/td[1]/h3[5]")).Text;
                        CurrentTax_Levy = WebDriverTest.After(CurrentTax_Levy, "Current Tax Levy:  ");

                        CurrentTax_Due = driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr[2]/td/table[2]/tbody/tr/td[1]/h3[6]")).Text;
                        CurrentTax_Due = WebDriverTest.After(CurrentTax_Due, "Current Amount Due:  ");

                        PriorAmount_Due = driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr[2]/td/table[2]/tbody/tr/td[1]/h3[7]")).Text;
                        PriorAmount_Due = WebDriverTest.After(PriorAmount_Due, "Prior Year Amount Due: ");

                        Tlt_Due = driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr[2]/td/table[2]/tbody/tr/td[1]/h3[8]")).Text;
                        Tlt_Due = WebDriverTest.After(Tlt_Due, "Total Amount Due: ");

                        LastPayment_AmountforCurrentYearTaxes = driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr[2]/td/table[2]/tbody/tr/td[1]/h3[9]")).Text;
                        LastPayment_AmountforCurrentYearTaxes = WebDriverTest.After(LastPayment_AmountforCurrentYearTaxes, "Last Payment Amount for Current Year Taxes: ");

                        LastPayer_AmountforCurrentYearTaxes = driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr[2]/td/table[2]/tbody/tr/td[1]/h3[10]")).Text;
                        LastPayer_AmountforCurrentYearTaxes = WebDriverTest.After(LastPayer_AmountforCurrentYearTaxes, "Last Payer for Current Year Taxes:");

                        LastPayment_DateforCurrentYearTaxes = driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr[2]/td/table[2]/tbody/tr/td[1]/h3[11]")).Text;
                        LastPayment_DateforCurrentYearTaxes = WebDriverTest.After(LastPayment_DateforCurrentYearTaxes, "Last Payment Date for Current Year Taxes: ");

                        Active_Law = driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr[2]/td/table[2]/tbody/tr/td[1]/h3[12]")).Text;
                        Active_Law = WebDriverTest.After(Active_Law, "Active Lawsuits:   ");

                        Pending_Credit = driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr[2]/td/table[2]/tbody/tr/td[2]/h3[1]")).Text;
                        Pending_Credit = WebDriverTest.After(Pending_Credit, "Pending Credit Card or eCheck Payments:");

                        IWebElement GrossTB = driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr[2]/td/table[2]/tbody"));
                        IList<IWebElement> GrossTR = GrossTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> GrossTD;
                        foreach (IWebElement Gross in GrossTR)
                        {
                            GrossTD = Gross.FindElements(By.TagName("td"));
                            if (GrossTD.Count != 0 && GrossTD.Count != 0)
                            {
                                Grt = GrossTD[1].Text;
                                if (Grt.Contains("Gross Value:"))
                                {
                                    Gross_Value = GrossTD[1].Text;
                                    Gross_Value = WebDriverTest.Between(Gross_Value, "Gross Value:  ", "Land Value:  ");
                                }

                                Lav = GrossTD[1].Text;
                                if (Lav.Contains("Land Value:"))
                                {
                                    Land_Value = GrossTD[1].Text;
                                    Land_Value = WebDriverTest.Between(Land_Value, "Land Value:  ", "Improvement Value:   ");
                                }

                                Imp_vl = GrossTD[1].Text;
                                if (Imp_vl.Contains("Improvement Value:"))
                                {
                                    Impr_Value = GrossTD[1].Text;
                                    Impr_Value = WebDriverTest.Between(Impr_Value, "Improvement Value:   ", "Capped Value: ");
                                }

                                Cap_vl = GrossTD[1].Text;
                                if (Cap_vl.Contains("Capped Value:"))
                                {
                                    Capped_Value = GrossTD[1].Text;
                                    Capped_Value = WebDriverTest.Between(Capped_Value, "Capped Value: ", "Agricultural Value:   ");
                                }

                                Agr_vl = GrossTD[1].Text;
                                if (Agr_vl.Contains("Agricultural Value:"))
                                {
                                    Aggricultr_Law = GrossTD[1].Text;
                                    Aggricultr_Law = WebDriverTest.Between(Aggricultr_Law, "Agricultural Value:   ", "Exemptions: ");
                                }

                                Tax_vl = GrossTD[1].Text;
                                if (Tax_vl.Contains("Exemptions:"))
                                {
                                    Tax_Excemptions = GrossTD[1].Text;
                                    Tax_Excemptions = WebDriverTest.Between(Tax_Excemptions, "Exemptions: ", "Exemption and Tax Rate Information");
                                }
                            }
                        }

                        TaxBill_Details = Acct_Number + "~" + Tax_Owner_Address + "~" + Pro_SiteAddr + "~" + Legai_desp + "~" + CurrentTax_Levy + "~" + CurrentTax_Due + "~" + PriorAmount_Due + "~" + Tlt_Due + "~" + LastPayment_AmountforCurrentYearTaxes + "~" + LastPayer_AmountforCurrentYearTaxes + "~" + LastPayment_DateforCurrentYearTaxes + "~" + Active_Law + "~" + Pending_Credit + "~" + Gross_Value + "~" + Land_Value + "~" + Impr_Value + "~" + Capped_Value + "~" + Aggricultr_Law + "~" + Tax_Excemptions;
                        gc.CreatePdf(orderNumber, Parcel_ID, "Tax Bill Details", driver, "TX", "Ellis");
                        gc.insert_date(orderNumber, Parcel_ID, 825, TaxBill_Details, 1, DateTime.Now);
                    }
                    catch
                    { }

                    //Tax Judistion Details
                    try
                    {
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr[2]/td/table[2]/tbody/tr/td[2]/h3[9]/a[1]")).Click();
                            Thread.Sleep(2000);
                        }
                        catch
                        { }

                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr[2]/td/table[2]/tbody/tr/td[2]/h3[8]/a[1]")).Click();
                            Thread.Sleep(2000);
                        }
                        catch
                        { }

                        Tax_JudistionYear = driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr[1]/td/table/tbody/tr[1]/td/h3[1]")).Text;
                        Tax_JudistionYear = WebDriverTest.After(Tax_JudistionYear, "Jurisdiction Information for ");

                        Tax_ExmpYear = driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr[1]/td/table/tbody/tr[1]/td/h3[3]")).Text;
                        Tax_ExmpYear = WebDriverTest.After(Tax_ExmpYear, "Exemptions: ");

                        Tax_Judistion_details1 = Tax_JudistionYear + "~" + Tax_ExmpYear + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                        gc.insert_date(orderNumber, Parcel_ID, 826, Tax_Judistion_details1, 1, DateTime.Now);

                        IWebElement Tax_JudistionTB = driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr[1]/td/table/tbody/tr[2]/td/table/tbody"));
                        IList<IWebElement> Tax_JudistionTR = Tax_JudistionTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> Tax_JudistionTD;

                        foreach (IWebElement Tax_Judistion in Tax_JudistionTR)
                        {
                            Tax_JudistionTD = Tax_Judistion.FindElements(By.TagName("td"));
                            if (Tax_JudistionTD.Count != 0 && !Tax_Judistion.Text.Contains("Jurisdictions"))
                            {
                                Jurisdictions = Tax_JudistionTD[0].Text;
                                TaxMKT_Val = Tax_JudistionTD[1].Text;
                                TaxEXEMPt_Val = Tax_JudistionTD[2].Text;
                                TaxBle_Val = Tax_JudistionTD[3].Text;
                                TaxJu_Rate = Tax_JudistionTD[4].Text;
                                Leavy = Tax_JudistionTD[5].Text;

                                Tax_Judistion_details = "" + "~" + "" + "~" + Jurisdictions + "~" + TaxMKT_Val + "~" + TaxEXEMPt_Val + "~" + TaxBle_Val + "~" + TaxJu_Rate + "~" + Leavy;
                                gc.CreatePdf(orderNumber, Parcel_ID, "Tax Judistion Details", driver, "TX", "Ellis");
                                gc.insert_date(orderNumber, Parcel_ID, 826, Tax_Judistion_details, 1, DateTime.Now);
                            }
                        }
                    }
                    catch
                    { }

                    driver.Navigate().Back();
                    Thread.Sleep(2000);

                    //Taxes Due Details
                    try
                    {
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr[2]/td/table[2]/tbody/tr/td[2]/h3[9]/a[2]")).Click();
                            Thread.Sleep(2000);
                        }
                        catch
                        { }

                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr[2]/td/table[2]/tbody/tr/td[2]/h3[8]/a[2]")).Click();
                            Thread.Sleep(2000);
                        }
                        catch
                        { }

                        Tax_Active_Law = driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr[1]/td/center/table/tbody/tr/td/div/h3[3]")).Text;
                        Tax_Active_Law = WebDriverTest.After(Tax_Active_Law, "Active Lawsuits ");

                        TaxesDue_details1 = Tax_Active_Law + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                        gc.insert_date(orderNumber, Parcel_ID, 830, TaxesDue_details1, 1, DateTime.Now);

                        IWebElement TaxesDueTB2 = driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr[1]/td/center/table/tbody/tr/td/table/tbody"));
                        IList<IWebElement> TaxesDueTR2 = TaxesDueTB2.FindElements(By.TagName("tr"));
                        IList<IWebElement> TaxesDueTD2;

                        foreach (IWebElement TaxesDue2 in TaxesDueTR2)
                        {
                            TaxesDueTD2 = TaxesDue2.FindElements(By.TagName("td"));
                            if (TaxesDueTD2.Count != 0 && TaxesDue2.Text.Contains("by end of "))
                            {
                                Taxy_Year1 = TaxesDueTD2[0].Text;
                                byend_ofSep = TaxesDueTD2[1].Text;
                                byend_ofOct = TaxesDueTD2[2].Text;
                                byend_ofNov = TaxesDueTD2[3].Text;

                                TaxesDue_details2 = Taxy_Year1 + "~" + "" + "~" + "" + "~" + byend_ofSep + "~" + "" + "~" + byend_ofOct + "~" + "" + "~" + byend_ofNov + "~" + "";
                                gc.CreatePdf(orderNumber, Parcel_ID, "Taxes Dues Details", driver, "TX", "Ellis");
                                gc.insert_date(orderNumber, Parcel_ID, 830, TaxesDue_details2, 1, DateTime.Now);
                            }
                        }

                        IWebElement TaxesDueTB = driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr[1]/td/center/table/tbody/tr/td/table/tbody"));
                        IList<IWebElement> TaxesDueTR = TaxesDueTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> TaxesDueTD;

                        foreach (IWebElement TaxesDue in TaxesDueTR)
                        {
                            TaxesDueTD = TaxesDue.FindElements(By.TagName("td"));
                            if (TaxesDueTD.Count != 0 && !TaxesDue.Text.Contains("by end of ") && !TaxesDue.Text.Contains("Year"))
                            {
                                Taxy_Year = TaxesDueTD[0].Text;
                                BaseTax_Due = TaxesDueTD[1].Text;
                                Penalty_Inter = TaxesDueTD[2].Text;
                                TolTax_Due = TaxesDueTD[3].Text;
                                PenaltyACC_Inter = TaxesDueTD[4].Text;
                                TTax_Due = TaxesDueTD[5].Text;
                                PenaltyAC_Inter = TaxesDueTD[6].Text;
                                TTTax_Due = TaxesDueTD[7].Text;

                                TaxesDue_details = "" + "~" + Taxy_Year + "~" + BaseTax_Due + "~" + Penalty_Inter + "~" + TolTax_Due + "~" + PenaltyACC_Inter + "~" + TTax_Due + "~" + PenaltyAC_Inter + "~" + TTTax_Due;
                                gc.CreatePdf(orderNumber, Parcel_ID, "Taxes Due Details", driver, "TX", "Ellis");
                                gc.insert_date(orderNumber, Parcel_ID, 830, TaxesDue_details, 1, DateTime.Now);
                            }
                        }
                    }
                    catch
                    { }

                    driver.Navigate().Back();
                    Thread.Sleep(2000);

                    //Tax Payment Details
                    try
                    {
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr[2]/td/table[2]/tbody/tr/td[2]/h3[8]/a[3]")).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf_Chrome(orderNumber, Parcel_ID, "Taxes Payment Details", driver, "TX", "Ellis");
                        }
                        catch
                        { }

                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr[2]/td/table[2]/tbody/tr/td[2]/h3[9]/a[3]")).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf_Chrome(orderNumber, Parcel_ID, "Taxes Payment Details", driver, "TX", "Ellis");
                        }
                        catch
                        { }

                        IWebElement Tax_PaymentTB = driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr[1]/td/table/tbody"));
                        IList<IWebElement> Tax_PaymentTR = Tax_PaymentTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> Tax_PaymentTD;

                        foreach (IWebElement Tax_Payment in Tax_PaymentTR)
                        {
                            Tax_PaymentTD = Tax_Payment.FindElements(By.TagName("td"));
                            if (Tax_PaymentTD.Count != 0 && !Tax_Payment.Text.Contains("Receipt Date"))
                            {
                                Recipt_Date = Tax_PaymentTD[0].Text;
                                Amount = Tax_PaymentTD[1].Text;
                                Tax_year = Tax_PaymentTD[2].Text;
                                Taxing_Desp = Tax_PaymentTD[3].Text;
                                Payer = Tax_PaymentTD[4].Text;

                                Tax_Payment_details = Recipt_Date + "~" + Amount + "~" + Tax_year + "~" + Taxing_Desp + "~" + Payer;
                                gc.insert_date(orderNumber, Parcel_ID, 827, Tax_Payment_details, 1, DateTime.Now);
                            }
                        }
                    }
                    catch
                    { }

                    driver.Navigate().Back();
                    Thread.Sleep(2000);

                    //Download Tax Bill
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr[2]/td/table[2]/tbody/tr/td[2]/h3[8]/a[4]")).Click();
                        Thread.Sleep(5000);
                    }
                    catch
                    { }
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr[2]/td/table[2]/tbody/tr/td[2]/h3[9]/a[4]")).Click();
                        Thread.Sleep(5000);
                    }
                    catch
                    { }

                    driver.SwitchTo().Window(driver.WindowHandles.Last());
                    Thread.Sleep(2000);

                    try
                    {
                        IWebElement CurrentBill = driver.FindElement(By.XPath("//*[@id='pageContent']/table/tbody/tr[3]/td/div/h3/a"));
                        string Tax = CurrentBill.GetAttribute("href");
                        gc.downloadfileHeader(Tax, orderNumber, Parcel_ID, "Current Tax Bill", "TX", "Ellis", driver);
                    }
                    catch
                    { }

                    //Tax Authority
                    driver.Navigate().GoToUrl("https://www.txdmv.gov/tax-assessor-collectors/county-tax-offices/ellis");
                    Thread.Sleep(2000);

                    try
                    {
                        Mailing_Add = driver.FindElement(By.XPath("//*[@id='k2Container']/div[3]/div[1]/ul/li[4]/span[2]")).Text;
                        Tele1 = driver.FindElement(By.XPath("//*[@id='k2Container']/div[3]/div[1]/ul/li[5]/span[2]")).Text;
                        Tele2 = driver.FindElement(By.XPath("//*[@id='k2Container']/div[3]/div[1]/ul/li[6]/span[2]")).Text;

                        TaxAuthority = Mailing_Add + " " + Tele1 + " " + Tele2;

                        if (PriorAmount_Due == "$")
                        {
                            TaxPrior = "You must call the Tax Collector's Office.";
                        }
                        TaxAuthority_Details = TaxAuthority + "~" + TaxPrior;
                        gc.CreatePdf(orderNumber, Parcel_ID, "Tax Authority Details", driver, "TX", "Ellis");
                        gc.insert_date(orderNumber, Parcel_ID, 838, TaxAuthority_Details, 1, DateTime.Now);
                    }
                    catch
                    { }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "TX", "Ellis", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "TX", "Ellis");
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
    }
}