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
using System.Web.UI;

namespace ScrapMaricopa.Scrapsource
{
    public class Webdriver_WAKitsap
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_WAKitsap(string address, string assessment_id, string parcelNumber, string searchType, string orderNumber, string directParcel, string ownername)
        {
            string ProTaxpayer_Name = "", ProMail_address = "", Parcel_No = "", Accnt_id = "", ProSite_adrre = "", ProStatus = "", ProClass = "", property = "", Zoning = "", Jurisdiction = "", Acres = "", Sec_Twn = "", Tax_description = "", Year_Built = "", yblt = "";
            string Parcelno = "", Acct_Id = "", Site_Address = "", Taxpayer_Name = "", MultiAddress_details = "";
            string Tax_Year = "", Land = "", Bldgs_Etc = "", Market_value = "", Taxable_Value = "", Excemption = "", Tax = "", Taxwithout_Excemption = "", FFFP = "", SSWM = "", Nox_Weed = "", Other = "", Total_Billed = "", TaxHistory_Details = "";
            string Tax_code = "", Tax_status = "", Tax_Value = "", Taxx_Year = "", Assement = "", Voter_Approved = "", Leavy_Title = "", Voted = "", Levy_Rate = "", Tax_Amount = "", Forest_Fire = "", Noxis_weed = "", Storm_Water = "", TaxHistory_Details1 = "", Total_Billed1 = "";
            string Tax_code1 = "", Tax_status1 = "", Tax_Value1 = "", Taxx_Year1 = "", Assement2 = "", Voter_Approved1 = "", Leavy_Title1 = "", Voted1 = "", Levy_Rate1 = "", Tax_Amount1 = "", Forest_Fire1 = "", Noxis_weed1 = "", Storm_Water1 = "", TaxHistory_Details2 = "", Total_Billed2 = "";
            string Fund = "", Taxx = "", TaxOther = "", Tax_Interset = "", Tax_Penalty = "", Tax_Total = "", TaxHistory_Details3 = "", Output = "", Date = "", TReceipt = "", TYear = "", TaxHistory_Details4 = "";
            string Parcel_Loacation = "", Second_Halftion = "", Taxdue_Status = "", Diliquent_TYear = "", Pev_Tax = "", Intest_Penaltity = "", Total_Full = "", Total_Half = "", TAmount_Due = "", TaxDue_Details = "", Second_Halftion1 = "", Taxdue_Status1 = "", Diliquent_TYear1 = "", Pev_Tax1 = "", Intest_Penaltity1 = "", Total_Full1 = "", Total_Half1 = "", TAmount_Due1 = "", TaxDue_Details1 = "";
            string Tax_Authority = "", TaxAu = "", Fax = "", Phone = "", TaxDeliquent_Details = "", Taxes1 = "", Year1 = "", TaxDeliquent_Details1 = "", Total = "", Int_Pen = "", Taxes = "", Year = "", TaxDistributiondeails4 = "", CurrrentTax6 = "", CurrrentTax5 = "", TaxDistributiondeails2 = "", CurrrentTax4 = "", TaxDistributiondeails21 = "", CurrrentTax3 = "", CurrrentTax2 = "", CurrrentTax1 = "";
            string TaxDistributiondeails1 = "", Output4 = "", Output3 = "", Output2 = "", Output1 = "", TaxDistributiondeails = "", TaxYear2 = "", TaxYear1 = "", TaxHistory_Details111 = "", TaxTotal1 = "", Leavy_total1 = "", TotlaBilled1 = "", TaxHistory_Details11 = "", TaxTotal = "", Leavy_total = "", TotlaBilled = "";
            string Details2 = "", Details = "", Details1 = "", Half = "", Parcel_Loacation1 = "";

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

                        gc.TitleFlexSearch(orderNumber, parcelNumber, ownername, "", "WA", "Kitsap");

                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_WAKitsap"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        searchType = "parcel";
                        parcelNumber = GlobalClass.global_parcelNo;
                    }

                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("https://psearch.kitsapgov.com/pdetails/");
                        Thread.Sleep(2000);

                        driver.FindElement(By.XPath("//*[@id='cphFormContent_Search1_rblSearchType_1']")).Click();
                        Thread.Sleep(2000);

                        driver.FindElement(By.Id("cphFormContent_Search1_txtSearchText")).SendKeys(address);
                        Thread.Sleep(8000);
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='searchDropDown']/div[3]")).Click();
                            gc.CreatePdf_WOP(orderNumber, "Address search", driver, "WA", "Kitsap");
                            Thread.Sleep(2000);
                        }
                        catch { }

                        try
                        {
                            IWebElement MultiAddressTB = driver.FindElement(By.XPath("//*[@id='dockSearchBoxDynamic']/tbody"));
                            IList<IWebElement> MultiAddressTR = MultiAddressTB.FindElements(By.TagName("div"));
                            IList<IWebElement> MultiAddressTD;
                            gc.CreatePdf_WOP(orderNumber, "Multi Address search", driver, "WA", "Kitsap");

                            int AddressmaxCheck = 0;
                            foreach (IWebElement MultiAddress in MultiAddressTR)
                            {
                                if (AddressmaxCheck <= 25)
                                {
                                    MultiAddressTD = MultiAddress.FindElements(By.TagName("div"));
                                    if (MultiAddressTD.Count != 0 && !MultiAddress.Text.Contains("Details ▾"))
                                    {
                                        Parcelno = MultiAddressTD[0].Text;
                                        Acct_Id = MultiAddressTD[1].Text;
                                        Site_Address = MultiAddressTD[2].Text;
                                        Taxpayer_Name = MultiAddressTD[3].Text;

                                        MultiAddress_details = Acct_Id + "~" + Site_Address + "~" + Taxpayer_Name;
                                        gc.insert_date(orderNumber, Parcelno, 914, MultiAddress_details, 1, DateTime.Now);
                                    }
                                    AddressmaxCheck++;
                                }
                            }
                            if (MultiAddressTR.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_WAKitsap_Multicount"] = "Maximum";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiparcel_WAKitsap"] = "Yes";
                            }
                            driver.Quit();
                            return "MultiParcel";

                        }
                        catch
                        { }
                    }

                    if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("https://psearch.kitsapgov.com/pdetails/");
                        Thread.Sleep(2000);

                        gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearch", driver, "WA", "Kitsap");
                        Thread.Sleep(1000);
                        driver.FindElement(By.Id("cphFormContent_Search1_txtSearchText")).SendKeys(parcelNumber);
                        Thread.Sleep(6000);
                    }

                    if (searchType == "ownername")
                    {
                        driver.Navigate().GoToUrl("https://psearch.kitsapgov.com/pdetails/");
                        Thread.Sleep(2000);

                        driver.FindElement(By.XPath("//*[@id='_rfdSkinnedcphFormContent_Search1_rblSearchType_2']")).Click();
                        Thread.Sleep(2000);

                        driver.FindElement(By.Id("cphFormContent_Search1_txtSearchText")).SendKeys(ownername);
                        Thread.Sleep(6000);

                        driver.FindElement(By.XPath("//*[@id='searchDropDown']/div[3]")).Click();
                        gc.CreatePdf_WOP(orderNumber, "Owner search", driver, "WA", "Kitsap");
                        Thread.Sleep(2000);
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.Id("searchDropDown"));
                        if(INodata.Text.Contains("Records Found: 0"))
                        {
                            HttpContext.Current.Session["Nodata_WAKitsap"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    //Scraped Details
                    try
                    {
                        //Property Details
                        IWebElement PropertyTable = driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_ctl00_dvParcelInfo']/tbody"));
                        IList<IWebElement> PropertyTR = PropertyTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> PropertyTD;
                        int i = 0;
                        foreach (IWebElement Property in PropertyTR)
                        {
                            PropertyTD = Property.FindElements(By.TagName("td"));
                            if (i == 0)
                                ProTaxpayer_Name = PropertyTD[1].Text;
                            if (i == 1)
                                ProMail_address = PropertyTD[1].Text;
                            if (i == 2)
                                Parcel_No = PropertyTD[1].Text;
                            if (i == 3)
                                Accnt_id = PropertyTD[1].Text;
                            if (i == 4)
                                ProSite_adrre = PropertyTD[1].Text;
                            if (i == 5)
                                ProStatus = PropertyTD[1].Text;
                            if (i == 6)
                                ProClass = PropertyTD[1].Text;
                            i++;
                        }

                        try
                        {
                            ByVisibleElement(driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_ctl00_dvParcelInfo']/tbody/tr[4]")));
                            gc.CreatePdf(orderNumber, Parcel_No, "Property", driver, "WA", "Kitsap");
                        }
                        catch
                        { }

                        driver.FindElement(By.XPath("//*[@id='demo']/li[2]/a")).Click();
                        Thread.Sleep(2000);

                        driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_NavigationMenu1_TaxDescription']")).Click();
                        Thread.Sleep(3000);

                        try
                        {
                            gc.CreatePdf(orderNumber, Parcel_No, "Tax Description", driver, "WA", "Kitsap");
                            Tax_description = driver.FindElement(By.Id("cphFormContent_Details1_ctl00_lblTaxDescription")).Text;
                        }
                        catch
                        { }

                        driver.FindElement(By.XPath("//*[@id='demo']/li[2]/a")).Click();
                        Thread.Sleep(3000);

                        driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_NavigationMenu1_LandLocation']")).Click();
                        Thread.Sleep(3000);

                        gc.CreatePdf(orderNumber, Parcel_No, "Property Details2", driver, "WA", "Kitsap");

                        IWebElement PropertyTable1 = driver.FindElement(By.XPath("//*[@id='divPrint']/div[4]/table/tbody"));
                        IList<IWebElement> PropertyTR1 = PropertyTable1.FindElements(By.TagName("tr"));
                        IList<IWebElement> PropertyTD1;
                        int j = 0;
                        foreach (IWebElement Property1 in PropertyTR1)
                        {
                            PropertyTD1 = Property1.FindElements(By.TagName("td"));
                            if (j == 0)
                                Zoning = PropertyTD1[1].Text;
                            if (j == 1)
                                Jurisdiction = PropertyTD1[1].Text;
                            j++;
                        }

                        IWebElement PropertyTable2 = driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_ctl00_dvDetails']/tbody"));
                        IList<IWebElement> PropertyTR2 = PropertyTable2.FindElements(By.TagName("tr"));
                        IList<IWebElement> PropertyTD2;

                        int k = 0;
                        foreach (IWebElement Property2 in PropertyTR2)
                        {
                            PropertyTD2 = Property2.FindElements(By.TagName("td"));
                            if (k == 0)
                                Sec_Twn = PropertyTD2[1].Text;
                            if (k == 1)
                                Acres = PropertyTD2[1].Text;
                            k++;
                        }

                        try
                        {
                            ByVisibleElement(driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_ctl00_dvDetails']/tbody/tr[1]")));
                            gc.CreatePdf(orderNumber, Parcel_No, "Acres", driver, "WA", "Kitsap");
                        }
                        catch
                        { }

                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='demo']/li[2]/a")).Click();
                            Thread.Sleep(3000);

                            driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_NavigationMenu1_BuildingsImprovements']")).Click();
                            Thread.Sleep(3000);
                            gc.CreatePdf(orderNumber, Parcel_No, "Property Details3", driver, "WA", "Kitsap");

                            IWebElement Yeartb = driver.FindElement(By.XPath("//*[@id='divPrint']/div[3]/table/tbody"));
                            IList<IWebElement> YearTR = Yeartb.FindElements(By.TagName("tr"));
                            IList<IWebElement> YearTD;

                            foreach (IWebElement Tax4 in YearTR)
                            {
                                YearTD = Tax4.FindElements(By.TagName("td"));
                                if (YearTD.Count != 0)
                                {
                                    yblt = YearTD[0].Text;
                                    if (yblt.Contains("Year Built"))
                                    {
                                        Year_Built = YearTD[1].Text;
                                    }
                                }
                            }

                        }
                        catch
                        { }

                        property = ProTaxpayer_Name + "~" + ProMail_address + "~" + Accnt_id + "~" + ProSite_adrre + "~" + ProStatus + "~" + ProClass + "~" + Jurisdiction + "~" + Zoning + "~" + Sec_Twn + "~" + Acres + "~" + Year_Built + "~" + Tax_description;
                        gc.insert_date(orderNumber, Parcel_No, 926, property, 1, DateTime.Now);
                    }
                    catch
                    { }

                    try
                    {
                        //Tax, Levies & Assessments Details
                        driver.FindElement(By.XPath("//*[@id='demo']/li[2]/a")).Click();
                        Thread.Sleep(4000);

                        driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_NavigationMenu1_TaxLevyAssessments']")).Click();
                        Thread.Sleep(5000);

                        IWebElement AssemntTable = driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_ctl00_HeaderData']/table/tbody"));
                        IList<IWebElement> AssemntTR = AssemntTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> AssemntTD;
                        int l = 0;
                        foreach (IWebElement Assemnt in AssemntTR)
                        {
                            AssemntTD = Assemnt.FindElements(By.TagName("td"));
                            if (l == 0)
                                Taxx_Year = AssemntTD[1].FindElement(By.TagName("input")).GetAttribute("value").Replace("▼", "").Replace("?", "").Trim();
                            if (l == 1)
                                Tax_code = AssemntTD[1].Text;
                            if (l == 2)
                                Tax_status = AssemntTD[1].Text;
                            if (l == 3)
                                Tax_Value = AssemntTD[1].Text;
                            l++;
                        }

                        Voter_Approved = driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_ctl00_levyAmounts']/div/div")).Text;
                        Voter_Approved = WebDriverTest.After(Voter_Approved, "Voter Approved Property Taxes -- ");
                        gc.CreatePdf(orderNumber, Parcel_No, "Assessment Details1", driver, "WA", "Kitsap");
                        IWebElement AssemntTable2 = driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_ctl00_otherAmounts']/table/tbody"));
                        IList<IWebElement> AssemntTR2 = AssemntTable2.FindElements(By.TagName("tr"));
                        IList<IWebElement> AssemntTD2;
                        int m = 0;

                        foreach (IWebElement Assemnt2 in AssemntTR2)
                        {
                            AssemntTD2 = Assemnt2.FindElements(By.TagName("td"));
                            if (m == 0)
                                Forest_Fire = AssemntTD2[1].Text;
                            if (m == 1)
                                Noxis_weed = AssemntTD2[1].Text;
                            if (m == 2)
                                Storm_Water = AssemntTD2[1].Text;
                            m++;
                        }

                        Total_Billed1 = driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_ctl00_otherAmounts']/table/tfoot/tr/td[2]")).Text;
                        Assement = Taxx_Year + "~" + Tax_code + "~" + Tax_status + "~" + Tax_Value + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + Voter_Approved + "~" + Forest_Fire + "~" + Noxis_weed + "~" + Storm_Water + "~" + Total_Billed1;
                        gc.insert_date(orderNumber, Parcel_No, 931, Assement, 1, DateTime.Now);
                        Taxx_Year = ""; Tax_code = ""; Tax_status = ""; Tax_Value = ""; Voter_Approved = ""; Forest_Fire = ""; Noxis_weed = ""; Storm_Water = ""; Total_Billed1 = "";

                        IWebElement AssemntTB1 = driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_ctl00_levyAmounts']/table/tbody[1]"));
                        IList<IWebElement> AssemntTR1 = AssemntTB1.FindElements(By.TagName("tr"));
                        IList<IWebElement> AssemntTD1;

                        foreach (IWebElement Assemnt1 in AssemntTR1)
                        {
                            AssemntTD1 = Assemnt1.FindElements(By.TagName("td"));
                            if (AssemntTD1.Count != 0 && !Assemnt1.Text.Contains("Levy Title") && !Assemnt1.Text.Contains("TOTAL PROPERTY TAX"))
                            {
                                Leavy_Title = AssemntTD1[0].Text;
                                Voted = AssemntTD1[1].Text;
                                Levy_Rate = AssemntTD1[2].Text;
                                Tax_Amount = AssemntTD1[3].Text;

                                TaxHistory_Details1 = "" + "~" + "" + "~" + "" + "~" + "" + "~" + Leavy_Title + "~" + Voted + "~" + Levy_Rate + "~" + Tax_Amount + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                gc.insert_date(orderNumber, Parcel_No, 931, TaxHistory_Details1, 1, DateTime.Now);
                            }
                        }

                        IWebElement AssemntTB11 = driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_ctl00_levyAmounts']/table/tfoot"));
                        IList<IWebElement> AssemntTR11 = AssemntTB11.FindElements(By.TagName("tr"));
                        IList<IWebElement> AssemntTD11;

                        foreach (IWebElement Assemnt11 in AssemntTR11)
                        {
                            AssemntTD11 = Assemnt11.FindElements(By.TagName("td"));
                            if (AssemntTD11.Count != 0)
                            {
                                TotlaBilled = AssemntTD11[0].Text;
                                Leavy_total = AssemntTD11[2].Text;
                                TaxTotal = AssemntTD11[3].Text;

                                TaxHistory_Details11 = "" + "~" + "" + "~" + "" + "~" + "" + "~" + TotlaBilled + "~" + "" + "~" + Leavy_total + "~" + TaxTotal + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                gc.insert_date(orderNumber, Parcel_No, 931, TaxHistory_Details11, 1, DateTime.Now);
                            }
                        }
                    }
                    catch
                    { }

                    try
                    {
                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_ctl00_levyAmounts']/table/tbody[1]/tr[5]/td[1]")));
                        gc.CreatePdf(orderNumber, Parcel_No, "Leavy", driver, "WA", "Kitsap");
                    }
                    catch
                    { }

                    try
                    {
                        IWebElement AssemntTable3 = driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_ctl00_HeaderData0']/table/tbody"));
                        IList<IWebElement> AssemntTR3 = AssemntTable3.FindElements(By.TagName("tr"));
                        IList<IWebElement> AssemntTD3;

                        int n = 0;
                        foreach (IWebElement Assemnt3 in AssemntTR3)
                        {
                            AssemntTD3 = Assemnt3.FindElements(By.TagName("td"));
                            if (n == 0)
                                Taxx_Year1 = AssemntTD3[1].FindElement(By.TagName("input")).GetAttribute("value").Replace("▼", "").Replace("?", "").Trim(); 
                            if (n == 1)
                                Tax_code1 = AssemntTD3[1].Text;
                            if (n == 2)
                                Tax_status1 = AssemntTD3[1].Text;
                            if (n == 3)
                                Tax_Value1 = AssemntTD3[1].Text;
                            n++;
                        }

                        Voter_Approved1 = driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_ctl00_levyAmounts0']/div/div")).Text;
                        Voter_Approved1 = WebDriverTest.After(Voter_Approved1, "Voter Approved Property Taxes -- ");

                        IWebElement AssemntTable4 = driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_ctl00_otherAmounts0']/table/tbody"));
                        IList<IWebElement> AssemntTR4 = AssemntTable4.FindElements(By.TagName("tr"));
                        IList<IWebElement> AssemntTD4;
                        int o = 0;
                        foreach (IWebElement Assemnt4 in AssemntTR4)
                        {
                            AssemntTD4 = Assemnt4.FindElements(By.TagName("td"));
                            if (o == 0)
                                Forest_Fire1 = AssemntTD4[1].Text;
                            if (o == 1)
                                Noxis_weed1 = AssemntTD4[1].Text;
                            if (o == 2)
                                Storm_Water1 = AssemntTD4[1].Text;
                            o++;
                        }

                        try
                        {
                            ByVisibleElement(driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_ctl00_levyAmounts']/table/tbody[1]/tr[11]/td[1]")));
                            gc.CreatePdf(orderNumber, Parcel_No, "Leavy1", driver, "WA", "Kitsap");
                        }
                        catch
                        { }

                        Total_Billed2 = driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_ctl00_otherAmounts0']/table/tfoot/tr/td[2]")).Text;
                        Assement2 = Taxx_Year1 + "~" + Tax_code1 + "~" + Tax_status1 + "~" + Tax_Value1 + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + Voter_Approved1 + "~" + Forest_Fire1 + "~" + Noxis_weed1 + "~" + Storm_Water1 + "~" + Total_Billed2;
                        gc.insert_date(orderNumber, Parcel_No, 935, Assement2, 1, DateTime.Now);
                        Taxx_Year1 = ""; Tax_code1 = ""; Tax_status1 = ""; Tax_Value1 = ""; Voter_Approved1 = ""; Forest_Fire1 = ""; Noxis_weed1 = ""; Storm_Water1 = ""; Total_Billed2 = "";

                        IWebElement AssemntTB5 = driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_ctl00_levyAmounts0']/table/tbody[1]"));
                        IList<IWebElement> AssemntTR5 = AssemntTB5.FindElements(By.TagName("tr"));
                        IList<IWebElement> AssemntTD5;

                        foreach (IWebElement Assemnt5 in AssemntTR5)
                        {
                            AssemntTD5 = Assemnt5.FindElements(By.TagName("td"));
                            if (AssemntTD5.Count != 0 && !Assemnt5.Text.Contains("Levy Title") && !Assemnt5.Text.Contains("TOTAL PROPERTY TAX"))
                            {
                                Leavy_Title1 = AssemntTD5[0].Text;
                                Voted1 = AssemntTD5[1].Text;
                                Levy_Rate1 = AssemntTD5[2].Text;
                                Tax_Amount1 = AssemntTD5[3].Text;

                                TaxHistory_Details2 = "" + "~" + "" + "~" + "" + "~" + "" + "~" + Leavy_Title1 + "~" + Voted1 + "~" + Levy_Rate1 + "~" + Tax_Amount1 + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                gc.insert_date(orderNumber, Parcel_No, 935, TaxHistory_Details2, 1, DateTime.Now);
                            }
                        }

                        IWebElement AssemntTB111 = driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_ctl00_levyAmounts0']/table/tfoot"));
                        IList<IWebElement> AssemntTR111 = AssemntTB111.FindElements(By.TagName("tr"));
                        IList<IWebElement> AssemntTD111;

                        foreach (IWebElement Assemnt111 in AssemntTR111)
                        {
                            AssemntTD111 = Assemnt111.FindElements(By.TagName("td"));
                            if (AssemntTD111.Count != 0)
                            {
                                TotlaBilled1 = AssemntTD111[0].Text;
                                Leavy_total1 = AssemntTD111[2].Text;
                                TaxTotal1 = AssemntTD111[3].Text;

                                TaxHistory_Details111 = "" + "~" + "" + "~" + "" + "~" + "" + "~" + TotlaBilled1 + "~" + "" + "~" + Leavy_total1 + "~" + TaxTotal1 + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                gc.insert_date(orderNumber, Parcel_No, 935, TaxHistory_Details111, 1, DateTime.Now);
                            }
                        }
                    }
                    catch
                    { }
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");


                    //Values & Tax History Details
                    driver.FindElement(By.XPath("//*[@id='demo']/li[2]/a")).Click();
                    Thread.Sleep(5000);

                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_NavigationMenu1_ValueTaxHistory']")).Click();
                        Thread.Sleep(7000);
                    }
                    catch
                    { }


                    IWebElement TaxHistoryTB = driver.FindElement(By.XPath("//*[@id='divPrint']/div[3]/div/table/tbody"));
                    IList<IWebElement> TaxHistoryTR = TaxHistoryTB.FindElements(By.TagName("tr"));
                    IList<IWebElement> TaxHistoryTD;

                    foreach (IWebElement TaxHistory in TaxHistoryTR)
                    {
                        TaxHistoryTD = TaxHistory.FindElements(By.TagName("td"));
                        if (TaxHistoryTD.Count != 0)
                        {
                            Tax_Year = TaxHistoryTD[0].Text;
                            Land = TaxHistoryTD[1].Text;
                            Bldgs_Etc = TaxHistoryTD[2].Text;
                            Market_value = TaxHistoryTD[3].Text;
                            Taxable_Value = TaxHistoryTD[4].Text;
                            Excemption = TaxHistoryTD[5].Text;
                            Tax = TaxHistoryTD[6].Text;
                            Taxwithout_Excemption = TaxHistoryTD[7].Text;
                            FFFP = TaxHistoryTD[8].Text;
                            SSWM = TaxHistoryTD[9].Text;
                            Nox_Weed = TaxHistoryTD[10].Text;
                            Other = TaxHistoryTD[11].Text;
                            Total_Billed = TaxHistoryTD[12].Text;

                            TaxHistory_Details = Tax_Year + "~" + Land + "~" + Bldgs_Etc + "~" + Market_value + "~" + Taxable_Value + "~" + Excemption + "~" + Tax + "~" + Taxwithout_Excemption + "~" + FFFP + "~" + SSWM + "~" + Nox_Weed + "~" + Other + "~" + Total_Billed;
                            gc.insert_date(orderNumber, Parcel_No, 928, TaxHistory_Details, 1, DateTime.Now);
                        }
                    }

                    try
                    {
                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='divPrint']/div[3]/div/table/tbody/tr[5]/td[7]")));
                        gc.CreatePdf(orderNumber, Parcel_No, "Value Details1", driver, "WA", "Kitsap");
                    }
                    catch
                    { }
                }
                catch
                { }

                try
                {
                    //Tax Payment History Details
                    driver.FindElement(By.XPath("//*[@id='demo']/li[2]/a")).Click();
                    Thread.Sleep(5000);

                    driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_NavigationMenu1_Receipts']")).Click();
                    Thread.Sleep(6000);

                    IWebElement TaxHistoryTB1 = driver.FindElement(By.XPath("//*[@id='divPrint']/div[2]/div/table/tbody"));
                    IList<IWebElement> TaxHistoryTR1 = TaxHistoryTB1.FindElements(By.TagName("tr"));
                    IList<IWebElement> TaxHistoryTD1;

                    foreach (IWebElement TaxHistory1 in TaxHistoryTR1)
                    {
                        TaxHistoryTD1 = TaxHistory1.FindElements(By.TagName("td"));
                        if (TaxHistoryTD1.Count != 0 && !TaxHistory1.Text.Contains("Fund"))
                        {
                            if (TaxHistoryTD1.Count == 1)
                            {
                                Output = TaxHistoryTD1[0].Text;
                                Date = WebDriverTest.Between(Output, "Date: ", " - Receipt: ");
                                TReceipt = WebDriverTest.Between(Output, " - Receipt: ", " - Tax Year: ");
                                TYear = WebDriverTest.After(Output, " - Tax Year: ");

                                TaxHistory_Details4 = TYear + "~" + Date + "~" + TReceipt + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                gc.insert_date(orderNumber, Parcel_No, 936, TaxHistory_Details4, 1, DateTime.Now);
                            }
                            if (TaxHistoryTD1.Count == 6)
                            {
                                Fund = TaxHistoryTD1[0].Text;
                                Taxx = TaxHistoryTD1[1].Text;
                                TaxOther = TaxHistoryTD1[2].Text;
                                Tax_Interset = TaxHistoryTD1[3].Text;
                                Tax_Penalty = TaxHistoryTD1[4].Text;
                                Tax_Total = TaxHistoryTD1[5].Text;

                                TaxHistory_Details3 = "" + "~" + "" + "~" + "" + "~" + Fund + "~" + Taxx + "~" + TaxOther + "~" + Tax_Interset + "~" + Tax_Penalty + "~" + Tax_Total;
                                gc.insert_date(orderNumber, Parcel_No, 936, TaxHistory_Details3, 1, DateTime.Now);
                            }
                        }
                    }

                    try
                    {
                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='divPrint']/div[2]/div/table/tbody/tr[6]")));
                        gc.CreatePdf(orderNumber, Parcel_No, "Tax Payment Details1", driver, "WA", "Kitsap");
                    }
                    catch
                    { }

                    try
                    {
                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='divPrint']/div[2]/div/table/tbody/tr[16]")));
                        gc.CreatePdf(orderNumber, Parcel_No, "Tax Payment Details2", driver, "WA", "Kitsap");
                    }
                    catch
                    { }

                    try
                    {
                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='divPrint']/div[2]/div/table/tbody/tr[26]")));
                        gc.CreatePdf(orderNumber, Parcel_No, "Tax Payment Details3", driver, "WA", "Kitsap");
                    }
                    catch
                    { }

                    try
                    {
                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='divPrint']/div[2]/div/table/tbody/tr[46]")));
                        gc.CreatePdf(orderNumber, Parcel_No, "Tax Payment Details4", driver, "WA", "Kitsap");
                    }
                    catch
                    { }

                    try
                    {
                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='divPrint']/div[2]/div/table/tbody/tr[61]")));
                        gc.CreatePdf(orderNumber, Parcel_No, "Tax Payment Details5", driver, "WA", "Kitsap");
                    }
                    catch
                    { }
                }
                catch
                { }
                string strCurrent = "";
                try
                {
                    strCurrent = driver.CurrentWindowHandle;
                    //Tax Status Details              
                    driver.FindElement(By.XPath("//*[@id='demo']/li[5]/a")).Click();
                    Thread.Sleep(4000);

                    driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_NavigationMenu1_ViewTaxes']")).Click();
                    Thread.Sleep(5000);

                    driver.SwitchTo().Window(driver.WindowHandles.Last());
                    //Tax Distribution Details
                    try
                    {
                        IWebElement TaxDistributionTB1 = driver.FindElement(By.XPath("//*[@id='AutoNumber4']/tbody"));
                        IList<IWebElement> TaxDistributionTR1 = TaxDistributionTB1.FindElements(By.TagName("tr"));
                        IList<IWebElement> TaxDistributionTD1;

                        foreach (IWebElement TaxDistribution in TaxDistributionTR1)
                        {
                            TaxDistributionTD1 = TaxDistribution.FindElements(By.TagName("td"));
                            if (TaxDistributionTD1.Count == 2)
                            {
                                TaxYear1 = TaxDistributionTD1[0].Text;
                                TaxYear2 = TaxDistributionTD1[1].Text;

                                TaxDistributiondeails = "" + "~" + TaxYear1 + "~" + "" + "~" + TaxYear2;
                                gc.insert_date(orderNumber, Parcel_No, 946, TaxDistributiondeails, 1, DateTime.Now);
                            }

                            if (TaxDistributionTD1.Count == 4)
                            {
                                Output1 = TaxDistributionTD1[0].Text;
                                Output2 = TaxDistributionTD1[1].Text;
                                Output3 = TaxDistributionTD1[2].Text;
                                Output4 = TaxDistributionTD1[3].Text;

                                TaxDistributiondeails1 = Output1 + "~" + Output2 + "~" + Output3 + "~" + Output4;
                                gc.insert_date(orderNumber, Parcel_No, 946, TaxDistributiondeails1, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }
                       
                        try
                        {
                            gc.CreatePdf(orderNumber, Parcel_No, "Tax Due Status Details", driver, "WA", "Kitsap");
                        }
                        catch
                        { }
                    try
                    { 
                        IWebElement TaxDistributionTB2 = driver.FindElement(By.XPath("//*[@id='AutoNumber3']/tbody"));
                        IList<IWebElement> TaxDistributionTR2 = TaxDistributionTB2.FindElements(By.TagName("tr"));
                        IList<IWebElement> TaxDistributionTD2;

                        foreach (IWebElement TaxDistribution2 in TaxDistributionTR2)
                        {
                            TaxDistributionTD2 = TaxDistribution2.FindElements(By.TagName("td"));
                            if (TaxDistributionTD2.Count == 3 && TaxDistribution2.Text.Trim() != (""))
                            {
                                CurrrentTax1 = TaxDistributionTD2[0].Text;
                                CurrrentTax2 = TaxDistributionTD2[1].Text;
                                CurrrentTax3 = TaxDistributionTD2[2].Text;

                                TaxDistributiondeails2 = CurrrentTax1 + "~" + CurrrentTax2 + "~" + CurrrentTax3 + "~" + "";
                                gc.insert_date(orderNumber, Parcel_No, 946, TaxDistributiondeails2, 1, DateTime.Now);
                            }
                            if (TaxDistributionTD2.Count == 1 && TaxDistribution2.Text.Trim() != (""))
                            {
                                CurrrentTax4 = TaxDistributionTD2[0].Text;

                                TaxDistributiondeails21 = CurrrentTax4 + "~" + "" + "~" + "" + "~" + "";
                                gc.insert_date(orderNumber, Parcel_No, 946, TaxDistributiondeails21, 1, DateTime.Now);
                            }
                            if (TaxDistributionTD2.Count == 2 && TaxDistribution2.Text.Trim() != (""))
                            {
                                CurrrentTax5 = TaxDistributionTD2[0].Text;
                                CurrrentTax6 = TaxDistributionTD2[1].Text;

                                TaxDistributiondeails4 = CurrrentTax5 + "~" + CurrrentTax6 + "~" + "" + "~" + "";
                                gc.insert_date(orderNumber, Parcel_No, 946, TaxDistributiondeails4, 1, DateTime.Now);
                            }
                        }
                    }
                    catch
                    { }

                    try
                    {
                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='AutoNumber3']/tbody/tr[8]")));
                        gc.CreatePdf(orderNumber, Parcel_No, "View", driver, "WA", "Kitsap");
                    }
                    catch
                    { }

                    try
                    {
                        //Deliquent Details
                        IWebElement TaxDeliquentTB = driver.FindElement(By.XPath("//*[@id='AutoNumber6']/tbody"));
                        IList<IWebElement> TaxDeliquentTR = TaxDeliquentTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> TaxDeliquentTD;

                        foreach (IWebElement TaxDeliquent in TaxDeliquentTR)
                        {
                            TaxDeliquentTD = TaxDeliquent.FindElements(By.TagName("td"));
                            if (TaxDeliquentTD.Count != 0 && !TaxDeliquent.Text.Contains("Delinquent section") && TaxDeliquentTD.Count != 4)
                            {
                                Year = TaxDeliquentTD[1].Text;
                                Taxes = TaxDeliquentTD[2].Text;
                                Int_Pen = TaxDeliquentTD[3].Text;
                                Total = TaxDeliquentTD[4].Text;

                                TaxDeliquent_Details = Year + "~" + Taxes + "~" + Int_Pen + "~" + Total;
                                gc.insert_date(orderNumber, Parcel_No, 946, TaxDeliquent_Details, 1, DateTime.Now);
                            }
                            if (TaxDeliquentTD.Count == 1)
                            {
                                string Year2 = TaxDeliquentTD[0].Text;

                                TaxDeliquent_Details1 = Year2.Replace(":", "") + "~" + "" + "~" + "" + "~" + "";
                                gc.insert_date(orderNumber, Parcel_No, 946, TaxDeliquent_Details1, 1, DateTime.Now);
                            }
                            if (TaxDeliquentTD.Count == 4)
                            {
                                Year1 = TaxDeliquentTD[1].Text;
                                Taxes1 = TaxDeliquentTD[2].Text;

                                TaxDeliquent_Details1 = Year1.Replace(":", "") + "~" + "" + "~" + "" + "~" + Taxes1;
                                gc.insert_date(orderNumber, Parcel_No, 946, TaxDeliquent_Details1, 1, DateTime.Now);
                            }
                        }

                    }
                    catch
                    { }

                    //Tax Due Status Details
                    try
                    {
                        Parcel_Loacation1 = driver.FindElement(By.XPath("//*[@id='AutoNumber7']/tbody/tr[2]/td[5]")).Text;
                        Parcel_Loacation1 = WebDriverTest.After(Parcel_Loacation1, "Parcel Location: ");
                        string Half1 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[3]/td[3]")).Text;

                        Details1 = Parcel_Loacation1 + "~" + Half1 + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                        gc.insert_date(orderNumber, Parcel_No, 937, Details1, 1, DateTime.Now);

                        IWebElement multitableElement11 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[4]/td[4]/table/tbody"));
                        IList<IWebElement> multitableRow11 = multitableElement11.FindElements(By.TagName("tr"));
                        IList<IWebElement> multirowTD11;
                        foreach (IWebElement row1 in multitableRow11)
                        {
                            multirowTD11 = row1.FindElements(By.TagName("td"));
                            if (multirowTD11.Count == 6 && !row1.Text.Contains("Tax") && !row1.Text.Contains("Full Half"))
                            {
                                Details = " " + "~" + " " + "~" + multirowTD11[0].Text.Replace(":", "").Trim() + "~" + multirowTD11[1].Text.Trim() + "~" + multirowTD11[2].Text.Trim() + "~" + multirowTD11[3].Text.Trim() + "~" + multirowTD11[4].Text.Trim() + "~" + multirowTD11[5].Text.Trim();
                                gc.insert_date(orderNumber, Parcel_No, 937, Details, 1, DateTime.Now);

                            }
                            if (multirowTD11.Count == 3)
                            {
                                Details2 = " " + "~" + " " + "~" + multirowTD11[1].Text.Replace(":", "").Trim() + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + multirowTD11[2].Text.Trim();
                                gc.insert_date(orderNumber, Parcel_No, 937, Details2, 1, DateTime.Now);
                            }
                        }
                    }
                    catch
                    { }

                    try
                    {
                        Parcel_Loacation1 = driver.FindElement(By.XPath("//*[@id='AutoNumber7']/tbody/tr[2]/td[5]")).Text;
                        Parcel_Loacation1 = WebDriverTest.After(Parcel_Loacation1, "Parcel Location: ");
                        Half = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[3]/td[3]")).Text;

                        Details1 = Parcel_Loacation1 + "~" + Half + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                        gc.insert_date(orderNumber, Parcel_No, 1043, Details1, 1, DateTime.Now);

                        IWebElement multitableElement1 = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[4]/td[4]/table/tbody"));
                        IList<IWebElement> multitableRow1 = multitableElement1.FindElements(By.TagName("tr"));
                        IList<IWebElement> multirowTD1;
                        foreach (IWebElement row in multitableRow1)
                        {
                            multirowTD1 = row.FindElements(By.TagName("td"));
                            if (multirowTD1.Count == 6 && !row.Text.Contains("Tax") && !row.Text.Contains("Full Half"))
                            {
                                Details = " " + "~" + " " + "~" + multirowTD1[0].Text.Replace(":","").Trim() + "~" + multirowTD1[1].Text.Trim() + "~" + multirowTD1[2].Text.Trim() + "~" + multirowTD1[3].Text.Trim() + "~" + multirowTD1[4].Text.Trim() + "~" + multirowTD1[5].Text.Trim();
                                gc.insert_date(orderNumber, Parcel_No, 1043, Details, 1, DateTime.Now);

                            }
                            if (multirowTD1.Count == 3)
                            {
                                Details2 = " " + "~" + " " + "~" + multirowTD1[1].Text.Replace(":", "").Trim() + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + multirowTD1[2].Text.Trim();
                                gc.insert_date(orderNumber, Parcel_No, 1043, Details2, 1, DateTime.Now);
                            }
                        }
                    }
                    catch
                    { }

                    try
                    {
                        //Tax Authority Details
                        driver.Navigate().GoToUrl("https://www.kitsapgov.com/treas/Pages/Contact.aspx");

                        Phone = driver.FindElement(By.XPath("//*[@id='WebPartWPQ2']/div[1]/table/tbody/tr[1]/td")).Text;
                        Fax = driver.FindElement(By.XPath("//*[@id='WebPartWPQ2']/div[1]/table/tbody/tr[2]/td")).Text;
                        TaxAu = driver.FindElement(By.XPath("//*[@id='WebPartWPQ2']/div[1]/table/tbody/tr[6]/td")).Text;

                        Tax_Authority = TaxAu + " " + "Phone: " + " " + Phone + " " + "Fax: " + " " + TaxAu;
                        gc.CreatePdf(orderNumber, Parcel_No, "Tax Authority Details", driver, "WA", "Kitsap");
                        gc.insert_date(orderNumber, Parcel_No, 949, Tax_Authority, 1, DateTime.Now);

                    }
                    catch
                    { }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "WA", "Kitsap", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "WA", "Kitsap");
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

        public void ByVisibleElement(IWebElement Element)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].scrollIntoView();", Element);
        }
    }
}







//using (driver = new ChromeDriver())
//            {
//                try
//                {
//                    StartTime = DateTime.Now.ToString("HH:mm:ss");

//                    if (searchType == "titleflex")
//                    {

//                        gc.TitleFlexSearch(orderNumber, parcelNumber, ownername, "", "WA", "Kitsap");

//                        if (HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes")
//                        {
//                            return "MultiParcel";
//                        }
//                        searchType = "parcel";
//                        parcelNumber = GlobalClass.global_parcelNo;
//                    }

//                    if (searchType == "address")
//                    {
//                        driver.Navigate().GoToUrl("https://psearch.kitsapgov.com/pdetails/");
//Thread.Sleep(2000);

//                        driver.FindElement(By.XPath("//*[@id='cphFormContent_Search1_rblSearchType_1']")).Click();
//Thread.Sleep(2000);

//                        driver.FindElement(By.Id("cphFormContent_Search1_txtSearchText")).SendKeys(address);
//Thread.Sleep(8000);

//                        driver.FindElement(By.XPath("//*[@id='searchDropDown']/div[3]")).Click();
//gc.CreatePdf_WOP(orderNumber, "Address search", driver, "WA", "Kitsap");
//                        Thread.Sleep(2000);

//                        try
//                        {
//                            IWebElement MultiAddressTB = driver.FindElement(By.XPath("//*[@id='dockSearchBoxDynamic']/tbody"));
//IList<IWebElement> MultiAddressTR = MultiAddressTB.FindElements(By.TagName("tr"));
//IList<IWebElement> MultiAddressTD;
//gc.CreatePdf_WOP(orderNumber, "Multi Address search", driver, "WA", "Kitsap");

//                            int AddressmaxCheck = 0;
//                            foreach (IWebElement MultiAddress in MultiAddressTR)
//                            {
//                                if (AddressmaxCheck <= 25)
//                                {
//                                    MultiAddressTD = MultiAddress.FindElements(By.TagName("td"));
//                                    if (MultiAddressTD.Count != 0 && !MultiAddress.Text.Contains("Details ▾"))
//                                    {
//                                        Parcelno = MultiAddressTD[0].Text;
//                                        Acct_Id = MultiAddressTD[1].Text;
//                                        Site_Address = MultiAddressTD[2].Text;
//                                        Taxpayer_Name = MultiAddressTD[3].Text;

//                                        MultiAddress_details = Acct_Id + "~" + Site_Address + "~" + Taxpayer_Name;
//                                        gc.insert_date(orderNumber, Parcelno, 914, MultiAddress_details, 1, DateTime.Now);
//                                    }
//                                    AddressmaxCheck++;
//                                }
//                            }
//                            if (MultiAddressTR.Count > 25)
//                            {
//                                HttpContext.Current.Session["multiParcel_WAKitsap_Multicount"] = "Maximum";
//                            }
//                            else
//                            {
//                                HttpContext.Current.Session["multiparcel_WAKitsap"] = "Yes";
//                            }
//                            driver.Quit();
//                            return "MultiParcel";

//                        }
//                        catch
//                        { }
//                    }

//                    if (searchType == "parcel")
//                    {
//                        driver.Navigate().GoToUrl("https://psearch.kitsapgov.com/pdetails/");
//Thread.Sleep(2000);

//                        gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearch", driver, "WA", "Kitsap");
//                        Thread.Sleep(1000);
//                        driver.FindElement(By.Id("cphFormContent_Search1_txtSearchText")).SendKeys(parcelNumber);
//Thread.Sleep(6000);
//                    }

//                    if (searchType == "ownername")
//                    {
//                        driver.Navigate().GoToUrl("https://psearch.kitsapgov.com/pdetails/");
//Thread.Sleep(2000);

//                        driver.FindElement(By.XPath("//*[@id='_rfdSkinnedcphFormContent_Search1_rblSearchType_2']")).Click();
//Thread.Sleep(2000);

//                        driver.FindElement(By.Id("cphFormContent_Search1_txtSearchText")).SendKeys(ownername);
//Thread.Sleep(6000);

//                        driver.FindElement(By.XPath("//*[@id='searchDropDown']/div[3]")).Click();
//gc.CreatePdf_WOP(orderNumber, "Owner search", driver, "WA", "Kitsap");
//                        Thread.Sleep(2000);
//                    }

//                    try
//                    {
//                        IWebElement INodata = driver.FindElement(By.Id("searchDropDown"));
//                        if(INodata.Text.Contains("Records Found: 0"))
//                        {

//                        }
//                    }
//                    catch { }

//                    //Scraped Details
//                    try
//                    {
//                        //Property Details
//                        IWebElement PropertyTable = driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_ctl00_dvParcelInfo']/tbody"));
//IList<IWebElement> PropertyTR = PropertyTable.FindElements(By.TagName("tr"));
//IList<IWebElement> PropertyTD;
//int i = 0;
//                        foreach (IWebElement Property in PropertyTR)
//                        {
//                            PropertyTD = Property.FindElements(By.TagName("td"));
//                            if (i == 0)
//                                ProTaxpayer_Name = PropertyTD[1].Text;
//                            if (i == 1)
//                                ProMail_address = PropertyTD[1].Text;
//                            if (i == 2)
//                                Parcel_No = PropertyTD[1].Text;
//                            if (i == 3)
//                                Accnt_id = PropertyTD[1].Text;
//                            if (i == 4)
//                                ProSite_adrre = PropertyTD[1].Text;
//                            if (i == 5)
//                                ProStatus = PropertyTD[1].Text;
//                            if (i == 6)
//                                ProClass = PropertyTD[1].Text;
//                            i++;
//                        }

//                        try
//                        {
//                            ByVisibleElement(driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_ctl00_dvParcelInfo']/tbody/tr[4]")));
//                            gc.CreatePdf(orderNumber, Parcel_No, "Property", driver, "WA", "Kitsap");
//                        }
//                        catch
//                        { }

//                        driver.FindElement(By.XPath("//*[@id='demo']/li[2]/a")).Click();
//Thread.Sleep(2000);

//                        driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_NavigationMenu1_TaxDescription']")).Click();
//Thread.Sleep(3000);

//                        try
//                        {
//                            gc.CreatePdf(orderNumber, Parcel_No, "Tax Description", driver, "WA", "Kitsap");
//                            Tax_description = driver.FindElement(By.XPath("//*[@id='divPrint']/div[2]/div/div")).Text;
//                        }
//                        catch
//                        { }

//                        driver.FindElement(By.XPath("//*[@id='demo']/li[2]/a")).Click();
//Thread.Sleep(3000);

//                        driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_NavigationMenu1_LandLocation']")).Click();
//Thread.Sleep(3000);

//                        gc.CreatePdf(orderNumber, Parcel_No, "Property Details2", driver, "WA", "Kitsap");

//                        IWebElement PropertyTable1 = driver.FindElement(By.XPath("//*[@id='divPrint']/div[4]/table/tbody"));
//IList<IWebElement> PropertyTR1 = PropertyTable1.FindElements(By.TagName("tr"));
//IList<IWebElement> PropertyTD1;
//int j = 0;
//                        foreach (IWebElement Property1 in PropertyTR1)
//                        {
//                            PropertyTD1 = Property1.FindElements(By.TagName("td"));
//                            if (j == 0)
//                                Zoning = PropertyTD1[1].Text;
//                            if (j == 1)
//                                Jurisdiction = PropertyTD1[1].Text;
//                            j++;
//                        }

//                        IWebElement PropertyTable2 = driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_ctl00_dvDetails']/tbody"));
//IList<IWebElement> PropertyTR2 = PropertyTable2.FindElements(By.TagName("tr"));
//IList<IWebElement> PropertyTD2;

//int k = 0;
//                        foreach (IWebElement Property2 in PropertyTR2)
//                        {
//                            PropertyTD2 = Property2.FindElements(By.TagName("td"));
//                            if (k == 0)
//                                Sec_Twn = PropertyTD2[1].Text;
//                            if (k == 1)
//                                Acres = PropertyTD2[1].Text;
//                            k++;
//                        }

//                        try
//                        {
//                            ByVisibleElement(driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_ctl00_dvDetails']/tbody/tr[1]")));
//                            gc.CreatePdf(orderNumber, Parcel_No, "Acres", driver, "WA", "Kitsap");
//                        }
//                        catch
//                        { }

//                        try
//                        {
//                            driver.FindElement(By.XPath("//*[@id='demo']/li[2]/a")).Click();
//Thread.Sleep(3000);

//                            driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_NavigationMenu1_BuildingsImprovements']")).Click();
//Thread.Sleep(3000);
//                            gc.CreatePdf(orderNumber, Parcel_No, "Property Details3", driver, "WA", "Kitsap");

//                            IWebElement Yeartb = driver.FindElement(By.XPath("//*[@id='divPrint']/div[3]/table/tbody"));
//IList<IWebElement> YearTR = Yeartb.FindElements(By.TagName("tr"));
//IList<IWebElement> YearTD;

//                            foreach (IWebElement Tax4 in YearTR)
//                            {
//                                YearTD = Tax4.FindElements(By.TagName("td"));
//                                if (YearTD.Count != 0)
//                                {
//                                    yblt = YearTD[0].Text;
//                                    if (yblt.Contains("Year Built"))
//                                    {
//                                        Year_Built = YearTD[1].Text;
//                                    }
//                                }
//                            }

//                        }
//                        catch
//                        { }

//                        property = ProTaxpayer_Name + "~" + ProMail_address + "~" + Accnt_id + "~" + ProSite_adrre + "~" + ProStatus + "~" + ProClass + "~" + Jurisdiction + "~" + Zoning + "~" + Sec_Twn + "~" + Acres + "~" + Year_Built + "~" + Tax_description;
//                        gc.insert_date(orderNumber, Parcel_No, 926, property, 1, DateTime.Now);
//                    }
//                    catch
//                    { }

//                    try
//                    {
//                        //Tax, Levies & Assessments Details
//                        driver.FindElement(By.XPath("//*[@id='demo']/li[2]/a")).Click();
//Thread.Sleep(4000);

//                        driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_NavigationMenu1_TaxLevyAssessments']")).Click();
//Thread.Sleep(5000);

//                        IWebElement AssemntTable = driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_ctl00_HeaderData']/table/tbody"));
//IList<IWebElement> AssemntTR = AssemntTable.FindElements(By.TagName("tr"));
//IList<IWebElement> AssemntTD;

//int l = 0;
//                        foreach (IWebElement Assemnt in AssemntTR)
//                        {
//                            AssemntTD = Assemnt.FindElements(By.TagName("td"));
//                            if (l == 0)
//                                Taxx_Year = AssemntTD[1].Text;
//                            if (l == 1)
//                                Tax_code = AssemntTD[1].Text;
//                            if (l == 2)
//                                Tax_status = AssemntTD[1].Text;
//                            if (l == 3)
//                                Tax_Value = AssemntTD[1].Text;
//                            l++;
//                        }

//                        Voter_Approved = driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_ctl00_levyAmounts']/div/div")).Text;
//                        Voter_Approved = WebDriverTest.After(Voter_Approved, "Voter Approved Property Taxes -- ");
//                        gc.CreatePdf(orderNumber, Parcel_No, "Assessment Details1", driver, "WA", "Kitsap");
//                        IWebElement AssemntTable2 = driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_ctl00_otherAmounts']/table/tbody"));
//IList<IWebElement> AssemntTR2 = AssemntTable2.FindElements(By.TagName("tr"));
//IList<IWebElement> AssemntTD2;
//int m = 0;

//                        foreach (IWebElement Assemnt2 in AssemntTR2)
//                        {
//                            AssemntTD2 = Assemnt2.FindElements(By.TagName("td"));
//                            if (m == 0)
//                                Forest_Fire = AssemntTD2[1].Text;
//                            if (m == 1)
//                                Noxis_weed = AssemntTD2[1].Text;
//                            if (m == 2)
//                                Storm_Water = AssemntTD2[1].Text;
//                            m++;
//                        }

//                        Total_Billed1 = driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_ctl00_otherAmounts']/table/tfoot/tr/td[2]")).Text;
//                        Assement = Taxx_Year + "~" + Tax_code + "~" + Tax_status + "~" + Tax_Value + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + Voter_Approved + "~" + Forest_Fire + "~" + Noxis_weed + "~" + Storm_Water + "~" + Total_Billed1;
//                        gc.insert_date(orderNumber, Parcel_No, 931, Assement, 1, DateTime.Now);
//                        Taxx_Year = ""; Tax_code = ""; Tax_status = ""; Tax_Value = ""; Voter_Approved = ""; Forest_Fire = ""; Noxis_weed = ""; Storm_Water = ""; Total_Billed1 = "";

//                        IWebElement AssemntTB1 = driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_ctl00_levyAmounts']/table/tbody[1]"));
//IList<IWebElement> AssemntTR1 = AssemntTB1.FindElements(By.TagName("tr"));
//IList<IWebElement> AssemntTD1;

//                        foreach (IWebElement Assemnt1 in AssemntTR1)
//                        {
//                            AssemntTD1 = Assemnt1.FindElements(By.TagName("td"));
//                            if (AssemntTD1.Count != 0 && !Assemnt1.Text.Contains("Levy Title") && !Assemnt1.Text.Contains("TOTAL PROPERTY TAX"))
//                            {
//                                Leavy_Title = AssemntTD1[0].Text;
//                                Voted = AssemntTD1[1].Text;
//                                Levy_Rate = AssemntTD1[2].Text;
//                                Tax_Amount = AssemntTD1[3].Text;

//                                TaxHistory_Details1 = "" + "~" + "" + "~" + "" + "~" + "" + "~" + Leavy_Title + "~" + Voted + "~" + Levy_Rate + "~" + Tax_Amount + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
//                                gc.insert_date(orderNumber, Parcel_No, 931, TaxHistory_Details1, 1, DateTime.Now);
//                            }
//                        }

//                        IWebElement AssemntTB11 = driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_ctl00_levyAmounts']/table/tfoot"));
//IList<IWebElement> AssemntTR11 = AssemntTB11.FindElements(By.TagName("tr"));
//IList<IWebElement> AssemntTD11;

//                        foreach (IWebElement Assemnt11 in AssemntTR11)
//                        {
//                            AssemntTD11 = Assemnt11.FindElements(By.TagName("td"));
//                            if (AssemntTD11.Count != 0)
//                            {
//                                TotlaBilled = AssemntTD11[0].Text;
//                                Leavy_total = AssemntTD11[2].Text;
//                                TaxTotal = AssemntTD11[3].Text;

//                                TaxHistory_Details11 = "" + "~" + "" + "~" + "" + "~" + "" + "~" + TotlaBilled + "~" + "" + "~" + Leavy_total + "~" + TaxTotal + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
//                                gc.insert_date(orderNumber, Parcel_No, 931, TaxHistory_Details11, 1, DateTime.Now);
//                            }
//                        }
//                    }
//                    catch
//                    { }

//                    try
//                    {
//                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_ctl00_levyAmounts']/table/tbody[1]/tr[5]/td[1]")));
//                        gc.CreatePdf(orderNumber, Parcel_No, "Leavy", driver, "WA", "Kitsap");
//                    }
//                    catch
//                    { }

//                    try
//                    {
//                        IWebElement AssemntTable3 = driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_ctl00_HeaderData0']/table/tbody"));
//IList<IWebElement> AssemntTR3 = AssemntTable3.FindElements(By.TagName("tr"));
//IList<IWebElement> AssemntTD3;

//int n = 0;
//                        foreach (IWebElement Assemnt3 in AssemntTR3)
//                        {
//                            AssemntTD3 = Assemnt3.FindElements(By.TagName("td"));
//                            if (n == 0)
//                                Taxx_Year1 = AssemntTD3[1].Text;
//                            if (n == 1)
//                                Tax_code1 = AssemntTD3[1].Text;
//                            if (n == 2)
//                                Tax_status1 = AssemntTD3[1].Text;
//                            if (n == 3)
//                                Tax_Value1 = AssemntTD3[1].Text;
//                            n++;
//                        }

//                        Voter_Approved1 = driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_ctl00_levyAmounts0']/div/div")).Text;
//                        Voter_Approved1 = WebDriverTest.After(Voter_Approved1, "Voter Approved Property Taxes -- ");

//                        IWebElement AssemntTable4 = driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_ctl00_otherAmounts0']/table/tbody"));
//IList<IWebElement> AssemntTR4 = AssemntTable4.FindElements(By.TagName("tr"));
//IList<IWebElement> AssemntTD4;
//int o = 0;
//                        foreach (IWebElement Assemnt4 in AssemntTR4)
//                        {
//                            AssemntTD4 = Assemnt4.FindElements(By.TagName("td"));
//                            if (o == 0)
//                                Forest_Fire1 = AssemntTD4[1].Text;
//                            if (o == 1)
//                                Noxis_weed1 = AssemntTD4[1].Text;
//                            if (o == 2)
//                                Storm_Water1 = AssemntTD4[1].Text;
//                            o++;
//                        }

//                        try
//                        {
//                            ByVisibleElement(driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_ctl00_levyAmounts']/table/tbody[1]/tr[11]/td[1]")));
//                            gc.CreatePdf(orderNumber, Parcel_No, "Leavy1", driver, "WA", "Kitsap");
//                        }
//                        catch
//                        { }

//                        Total_Billed2 = driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_ctl00_otherAmounts0']/table/tfoot/tr/td[2]")).Text;
//                        Assement2 = Taxx_Year1 + "~" + Tax_code1 + "~" + Tax_status1 + "~" + Tax_Value1 + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + Voter_Approved1 + "~" + Forest_Fire1 + "~" + Noxis_weed1 + "~" + Storm_Water1 + "~" + Total_Billed2;
//                        gc.insert_date(orderNumber, Parcel_No, 935, Assement2, 1, DateTime.Now);
//                        Taxx_Year1 = ""; Tax_code1 = ""; Tax_status1 = ""; Tax_Value1 = ""; Voter_Approved1 = ""; Forest_Fire1 = ""; Noxis_weed1 = ""; Storm_Water1 = ""; Total_Billed2 = "";

//                        IWebElement AssemntTB5 = driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_ctl00_levyAmounts0']/table/tbody[1]"));
//IList<IWebElement> AssemntTR5 = AssemntTB5.FindElements(By.TagName("tr"));
//IList<IWebElement> AssemntTD5;

//                        foreach (IWebElement Assemnt5 in AssemntTR5)
//                        {
//                            AssemntTD5 = Assemnt5.FindElements(By.TagName("td"));
//                            if (AssemntTD5.Count != 0 && !Assemnt5.Text.Contains("Levy Title") && !Assemnt5.Text.Contains("TOTAL PROPERTY TAX"))
//                            {
//                                Leavy_Title1 = AssemntTD5[0].Text;
//                                Voted1 = AssemntTD5[1].Text;
//                                Levy_Rate1 = AssemntTD5[2].Text;
//                                Tax_Amount1 = AssemntTD5[3].Text;

//                                TaxHistory_Details2 = "" + "~" + "" + "~" + "" + "~" + "" + "~" + Leavy_Title1 + "~" + Voted1 + "~" + Levy_Rate1 + "~" + Tax_Amount1 + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
//                                gc.insert_date(orderNumber, Parcel_No, 935, TaxHistory_Details2, 1, DateTime.Now);
//                            }
//                        }

//                        IWebElement AssemntTB111 = driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_ctl00_levyAmounts0']/table/tfoot"));
//IList<IWebElement> AssemntTR111 = AssemntTB111.FindElements(By.TagName("tr"));
//IList<IWebElement> AssemntTD111;

//                        foreach (IWebElement Assemnt111 in AssemntTR111)
//                        {
//                            AssemntTD111 = Assemnt111.FindElements(By.TagName("td"));
//                            if (AssemntTD111.Count != 0)
//                            {
//                                TotlaBilled1 = AssemntTD111[0].Text;
//                                Leavy_total1 = AssemntTD111[2].Text;
//                                TaxTotal1 = AssemntTD111[3].Text;

//                                TaxHistory_Details111 = "" + "~" + "" + "~" + "" + "~" + "" + "~" + TotlaBilled1 + "~" + "" + "~" + Leavy_total1 + "~" + TaxTotal1 + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
//                                gc.insert_date(orderNumber, Parcel_No, 935, TaxHistory_Details111, 1, DateTime.Now);
//                            }
//                        }
//                    }
//                    catch
//                    { }
//                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");


//                    //Values & Tax History Details
//                    driver.FindElement(By.XPath("//*[@id='demo']/li[2]/a")).Click();
//Thread.Sleep(5000);

//                    try
//                    {
//                        driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_NavigationMenu1_ValueTaxHistory']")).Click();
//Thread.Sleep(7000);
//                    }
//                    catch
//                    { }


//                    IWebElement TaxHistoryTB = driver.FindElement(By.XPath("//*[@id='divPrint']/div[3]/div/table/tbody"));
//IList<IWebElement> TaxHistoryTR = TaxHistoryTB.FindElements(By.TagName("tr"));
//IList<IWebElement> TaxHistoryTD;

//                    foreach (IWebElement TaxHistory in TaxHistoryTR)
//                    {
//                        TaxHistoryTD = TaxHistory.FindElements(By.TagName("td"));
//                        if (TaxHistoryTD.Count != 0)
//                        {
//                            Tax_Year = TaxHistoryTD[0].Text;
//                            Land = TaxHistoryTD[1].Text;
//                            Bldgs_Etc = TaxHistoryTD[2].Text;
//                            Market_value = TaxHistoryTD[3].Text;
//                            Taxable_Value = TaxHistoryTD[4].Text;
//                            Excemption = TaxHistoryTD[5].Text;
//                            Tax = TaxHistoryTD[6].Text;
//                            Taxwithout_Excemption = TaxHistoryTD[7].Text;
//                            FFFP = TaxHistoryTD[8].Text;
//                            SSWM = TaxHistoryTD[9].Text;
//                            Nox_Weed = TaxHistoryTD[10].Text;
//                            Other = TaxHistoryTD[11].Text;
//                            Total_Billed = TaxHistoryTD[12].Text;

//                            TaxHistory_Details = Tax_Year + "~" + Land + "~" + Bldgs_Etc + "~" + Market_value + "~" + Taxable_Value + "~" + Excemption + "~" + Tax + "~" + Taxwithout_Excemption + "~" + FFFP + "~" + SSWM + "~" + Nox_Weed + "~" + Other + "~" + Total_Billed;
//                            gc.insert_date(orderNumber, Parcel_No, 928, TaxHistory_Details, 1, DateTime.Now);
//                        }
//                    }

//                    try
//                    {
//                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='divPrint']/div[3]/div/table/tbody/tr[5]/td[7]")));
//                        gc.CreatePdf(orderNumber, Parcel_No, "Value Details1", driver, "WA", "Kitsap");
//                    }
//                    catch
//                    { }
//                }
//                catch
//                { }

//                try
//                {
//                    //Tax Payment History Details
//                    driver.FindElement(By.XPath("//*[@id='demo']/li[2]/a")).Click();
//Thread.Sleep(5000);

//                    driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_NavigationMenu1_Receipts']")).Click();
//Thread.Sleep(6000);

//                    IWebElement TaxHistoryTB1 = driver.FindElement(By.XPath("//*[@id='divPrint']/div[2]/div/table/tbody"));
//IList<IWebElement> TaxHistoryTR1 = TaxHistoryTB1.FindElements(By.TagName("tr"));
//IList<IWebElement> TaxHistoryTD1;

//                    foreach (IWebElement TaxHistory1 in TaxHistoryTR1)
//                    {
//                        TaxHistoryTD1 = TaxHistory1.FindElements(By.TagName("td"));
//                        if (TaxHistoryTD1.Count != 0 && !TaxHistory1.Text.Contains("Fund"))
//                        {
//                            if (TaxHistoryTD1.Count == 1)
//                            {
//                                Output = TaxHistoryTD1[0].Text;
//                                Date = WebDriverTest.Between(Output, "Date: ", " - Receipt: ");
//                                TReceipt = WebDriverTest.Between(Output, " - Receipt: ", " - Tax Year: ");
//                                TYear = WebDriverTest.After(Output, " - Tax Year: ");

//                                TaxHistory_Details4 = TYear + "~" + Date + "~" + TReceipt + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
//                                gc.insert_date(orderNumber, Parcel_No, 936, TaxHistory_Details4, 1, DateTime.Now);
//                            }
//                            if (TaxHistoryTD1.Count == 6)
//                            {
//                                Fund = TaxHistoryTD1[0].Text;
//                                Taxx = TaxHistoryTD1[1].Text;
//                                TaxOther = TaxHistoryTD1[2].Text;
//                                Tax_Interset = TaxHistoryTD1[3].Text;
//                                Tax_Penalty = TaxHistoryTD1[4].Text;
//                                Tax_Total = TaxHistoryTD1[5].Text;

//                                TaxHistory_Details3 = "" + "~" + "" + "~" + "" + "~" + Fund + "~" + Taxx + "~" + TaxOther + "~" + Tax_Interset + "~" + Tax_Penalty + "~" + Tax_Total;
//                                gc.insert_date(orderNumber, Parcel_No, 936, TaxHistory_Details3, 1, DateTime.Now);
//                            }
//                        }
//                    }

//                    try
//                    {
//                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='divPrint']/div[2]/div/table/tbody/tr[6]")));
//                        gc.CreatePdf(orderNumber, Parcel_No, "Tax Payment Details1", driver, "WA", "Kitsap");
//                    }
//                    catch
//                    { }

//                    try
//                    {
//                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='divPrint']/div[2]/div/table/tbody/tr[16]")));
//                        gc.CreatePdf(orderNumber, Parcel_No, "Tax Payment Details2", driver, "WA", "Kitsap");
//                    }
//                    catch
//                    { }

//                    try
//                    {
//                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='divPrint']/div[2]/div/table/tbody/tr[26]")));
//                        gc.CreatePdf(orderNumber, Parcel_No, "Tax Payment Details3", driver, "WA", "Kitsap");
//                    }
//                    catch
//                    { }

//                    try
//                    {
//                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='divPrint']/div[2]/div/table/tbody/tr[46]")));
//                        gc.CreatePdf(orderNumber, Parcel_No, "Tax Payment Details4", driver, "WA", "Kitsap");
//                    }
//                    catch
//                    { }

//                    try
//                    {
//                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='divPrint']/div[2]/div/table/tbody/tr[61]")));
//                        gc.CreatePdf(orderNumber, Parcel_No, "Tax Payment Details5", driver, "WA", "Kitsap");
//                    }
//                    catch
//                    { }
//                }
//                catch
//                { }

//                try
//                {
//                    //Tax Status Details              
//                    driver.FindElement(By.XPath("//*[@id='demo']/li[5]/a")).Click();
//Thread.Sleep(4000);

//                    driver.FindElement(By.XPath("//*[@id='cphFormContent_Details1_NavigationMenu1_ViewTaxes']")).Click();
//Thread.Sleep(5000);


//                    //Tax Distribution Details
//                    try
//                    {
//                        IWebElement TaxDistributionTB1 = driver.FindElement(By.XPath("//*[@id='AutoNumber4']/tbody"));
//IList<IWebElement> TaxDistributionTR1 = TaxDistributionTB1.FindElements(By.TagName("tr"));
//IList<IWebElement> TaxDistributionTD1;

//                        foreach (IWebElement TaxDistribution in TaxDistributionTR1)
//                        {
//                            TaxDistributionTD1 = TaxDistribution.FindElements(By.TagName("td"));
//                            if (TaxDistributionTD1.Count == 2)
//                            {
//                                TaxYear1 = TaxDistributionTD1[0].Text;
//                                TaxYear2 = TaxDistributionTD1[1].Text;

//                                TaxDistributiondeails = "" + "~" + TaxYear1 + "~" + "" + "~" + TaxYear2;
//                                gc.insert_date(orderNumber, Parcel_No, 946, TaxDistributiondeails, 1, DateTime.Now);
//                            }

//                            if (TaxDistributionTD1.Count == 4)
//                            {
//                                Output1 = TaxDistributionTD1[0].Text;
//                                Output2 = TaxDistributionTD1[1].Text;
//                                Output3 = TaxDistributionTD1[2].Text;
//                                Output4 = TaxDistributionTD1[3].Text;

//                                TaxDistributiondeails1 = Output1 + "~" + Output2 + "~" + Output3 + "~" + Output4;
//                                gc.insert_date(orderNumber, Parcel_No, 946, TaxDistributiondeails1, 1, DateTime.Now);
//                            }
//                        }

//                        try
//                        {
//                            gc.CreatePdf(orderNumber, Parcel_No, "Tax Due Status Details", driver, "WA", "Kitsap");
//                        }
//                        catch
//                        { }

//                        IWebElement TaxDistributionTB2 = driver.FindElement(By.XPath("//*[@id='AutoNumber3']/tbody"));
//IList<IWebElement> TaxDistributionTR2 = TaxDistributionTB2.FindElements(By.TagName("tr"));
//IList<IWebElement> TaxDistributionTD2;

//                        foreach (IWebElement TaxDistribution2 in TaxDistributionTR2)
//                        {
//                            TaxDistributionTD2 = TaxDistribution2.FindElements(By.TagName("td"));
//                            if (TaxDistributionTD2.Count == 3 && TaxDistribution2.Text.Trim() != (""))
//                            {
//                                CurrrentTax1 = TaxDistributionTD2[0].Text;
//                                CurrrentTax2 = TaxDistributionTD2[1].Text;
//                                CurrrentTax3 = TaxDistributionTD2[2].Text;

//                                TaxDistributiondeails2 = CurrrentTax1 + "~" + CurrrentTax2 + "~" + CurrrentTax3 + "~" + "";
//                                gc.insert_date(orderNumber, Parcel_No, 946, TaxDistributiondeails2, 1, DateTime.Now);
//                            }
//                            if (TaxDistributionTD2.Count == 1 && TaxDistribution2.Text.Trim() != (""))
//                            {
//                                CurrrentTax4 = TaxDistributionTD2[0].Text;

//                                TaxDistributiondeails21 = CurrrentTax4 + "~" + "" + "~" + "" + "~" + "";
//                                gc.insert_date(orderNumber, Parcel_No, 946, TaxDistributiondeails21, 1, DateTime.Now);
//                            }
//                            if (TaxDistributionTD2.Count == 2 && TaxDistribution2.Text.Trim() != (""))
//                            {
//                                CurrrentTax5 = TaxDistributionTD2[0].Text;
//                                CurrrentTax6 = TaxDistributionTD2[1].Text;

//                                TaxDistributiondeails4 = CurrrentTax5 + "~" + CurrrentTax6 + "~" + "" + "~" + "";
//                                gc.insert_date(orderNumber, Parcel_No, 946, TaxDistributiondeails4, 1, DateTime.Now);
//                            }
//                        }
//                    }
//                    catch
//                    { }

//                    try
//                    {
//                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='AutoNumber3']/tbody/tr[8]")));
//                        gc.CreatePdf(orderNumber, Parcel_No, "View", driver, "WA", "Kitsap");
//                    }
//                    catch
//                    { }

//                    try
//                    {
//                        //Deliquent Details
//                        IWebElement TaxDeliquentTB = driver.FindElement(By.XPath("//*[@id='AutoNumber6']/tbody"));
//IList<IWebElement> TaxDeliquentTR = TaxDeliquentTB.FindElements(By.TagName("tr"));
//IList<IWebElement> TaxDeliquentTD;

//                        foreach (IWebElement TaxDeliquent in TaxDeliquentTR)
//                        {
//                            TaxDeliquentTD = TaxDeliquent.FindElements(By.TagName("td"));
//                            if (TaxDeliquentTD.Count != 0 && !TaxDeliquent.Text.Contains("Delinquent section") && TaxDeliquentTD.Count != 4)
//                            {
//                                Year = TaxDeliquentTD[1].Text;
//                                Taxes = TaxDeliquentTD[2].Text;
//                                Int_Pen = TaxDeliquentTD[3].Text;
//                                Total = TaxDeliquentTD[4].Text;

//                                TaxDeliquent_Details = Year + "~" + Taxes + "~" + Int_Pen + "~" + Total;
//                                gc.insert_date(orderNumber, Parcel_No, 946, TaxDeliquent_Details, 1, DateTime.Now);
//                            }
//                            if (TaxDeliquentTD.Count == 1)
//                            {
//                                string Year2 = TaxDeliquentTD[0].Text;

//TaxDeliquent_Details1 = Year2 + "~" + "" + "~" + "" + "~" + "";
//                                gc.insert_date(orderNumber, Parcel_No, 946, TaxDeliquent_Details1, 1, DateTime.Now);
//                            }
//                            if (TaxDeliquentTD.Count == 4)
//                            {
//                                Year1 = TaxDeliquentTD[1].Text;
//                                Taxes1 = TaxDeliquentTD[2].Text;

//                                TaxDeliquent_Details1 = Year1 + "~" + "" + "~" + "" + "~" + Taxes1;
//                                gc.insert_date(orderNumber, Parcel_No, 946, TaxDeliquent_Details1, 1, DateTime.Now);
//                            }
//                        }

//                    }
//                    catch
//                    { }

//                    //Tax Due Status Details
//                    try
//                    {
//                        Parcel_Loacation1 = driver.FindElement(By.XPath("//*[@id='AutoNumber7']/tbody/tr[2]/td[5]")).Text;
//                        Parcel_Loacation1 = WebDriverTest.After(Parcel_Loacation1, "Parcel Location: ");
//                        string Half1 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[3]/td[3]")).Text;

//Details1 = Parcel_Loacation1 + "~" + Half1 + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
//                        gc.insert_date(orderNumber, Parcel_No, 937, Details1, 1, DateTime.Now);

//                        IWebElement multitableElement11 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[4]/td[4]/table/tbody"));
//IList<IWebElement> multitableRow11 = multitableElement11.FindElements(By.TagName("tr"));
//IList<IWebElement> multirowTD11;
//                        foreach (IWebElement row1 in multitableRow11)
//                        {
//                            multirowTD11 = row1.FindElements(By.TagName("td"));
//                            if (multirowTD11.Count == 6 && !row1.Text.Contains("Tax") && !row1.Text.Contains("Full Half"))
//                            {
//                                Details = " " + "~" + " " + "~" + multirowTD11[0].Text.Trim() + "~" + multirowTD11[1].Text.Trim() + "~" + multirowTD11[2].Text.Trim() + "~" + multirowTD11[3].Text.Trim() + "~" + multirowTD11[4].Text.Trim() + "~" + multirowTD11[5].Text.Trim();
//                                gc.insert_date(orderNumber, Parcel_No, 937, Details, 1, DateTime.Now);

//                            }
//                            if (multirowTD11.Count == 3)
//                            {
//                                Details2 = " " + "~" + " " + "~" + multirowTD11[1].Text.Trim() + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + multirowTD11[2].Text.Trim();
//                                gc.insert_date(orderNumber, Parcel_No, 937, Details2, 1, DateTime.Now);
//                            }
//                        }
//                    }
//                    catch
//                    { }

//                    try
//                    {
//                        Parcel_Loacation1 = driver.FindElement(By.XPath("//*[@id='AutoNumber7']/tbody/tr[2]/td[5]")).Text;
//                        Parcel_Loacation1 = WebDriverTest.After(Parcel_Loacation1, "Parcel Location: ");
//                        Half = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[3]/td[3]")).Text;

//                        Details1 = Parcel_Loacation1 + "~" + Half + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
//                        gc.insert_date(orderNumber, Parcel_No, 1043, Details1, 1, DateTime.Now);

//                        IWebElement multitableElement1 = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[4]/td[4]/table/tbody"));
//IList<IWebElement> multitableRow1 = multitableElement1.FindElements(By.TagName("tr"));
//IList<IWebElement> multirowTD1;
//                        foreach (IWebElement row in multitableRow1)
//                        {
//                            multirowTD1 = row.FindElements(By.TagName("td"));
//                            if (multirowTD1.Count == 6 && !row.Text.Contains("Tax") && !row.Text.Contains("Full Half"))
//                            {
//                                Details = " " + "~" + " " + "~" + multirowTD1[0].Text.Trim() + "~" + multirowTD1[1].Text.Trim() + "~" + multirowTD1[2].Text.Trim() + "~" + multirowTD1[3].Text.Trim() + "~" + multirowTD1[4].Text.Trim() + "~" + multirowTD1[5].Text.Trim();
//                                gc.insert_date(orderNumber, Parcel_No, 1043, Details, 1, DateTime.Now);

//                            }
//                            if (multirowTD1.Count == 3)
//                            {
//                                Details2 = " " + "~" + " " + "~" + multirowTD1[1].Text.Trim() + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + multirowTD1[2].Text.Trim();
//                                gc.insert_date(orderNumber, Parcel_No, 1043, Details2, 1, DateTime.Now);
//                            }
//                        }
//                    }
//                    catch
//                    { }

//                    try
//                    {
//                        //Tax Authority Details
//                        driver.Navigate().GoToUrl("https://www.kitsapgov.com/treas/Pages/Contact.aspx");

//Phone = driver.FindElement(By.XPath("//*[@id='WebPartWPQ2']/div[1]/table/tbody/tr[1]/td")).Text;
//                        Fax = driver.FindElement(By.XPath("//*[@id='WebPartWPQ2']/div[1]/table/tbody/tr[2]/td")).Text;
//                        TaxAu = driver.FindElement(By.XPath("//*[@id='WebPartWPQ2']/div[1]/table/tbody/tr[6]/td")).Text;

//                        Tax_Authority = TaxAu + " " + "Phone: " + " " + Phone + " " + "Fax: " + " " + TaxAu;
//                        gc.CreatePdf(orderNumber, Parcel_No, "Tax Authority Details", driver, "WA", "Kitsap");
//                        gc.insert_date(orderNumber, Parcel_No, 949, Tax_Authority, 1, DateTime.Now);

//                    }
//                    catch
//                    { }
//                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

//                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
//                    gc.insert_TakenTime(orderNumber, "WA", "Kitsap", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

//                    driver.Quit();
//                    gc.mergpdf(orderNumber, "WA", "Kitsap");
//                    return "Data Inserted Successfully";
//                }
//                catch (Exception ex)
//                {
//                    driver.Quit();
//                    GlobalClass.LogError(ex, orderNumber);
//                    throw;
//                }
//            }