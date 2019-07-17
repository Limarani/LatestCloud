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
    public class WebDriver_DENewCastle
    {
        string outputPath = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_NCDelaware(string houseno, string sname, string sttype, string parcelNumber, string searchType, string orderNumber, string ownername, string unino)
        {

            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            string strPropertyAddress = "-", strSubdivision = "-", strOwner = "-", strMunicipal = "-", strPropertyClass = "-", stryearBuilt = "-", property_details = "-", strLand = "-", strStructure = "-", strHomesite = "-", strTotal = "-", strCountyTax = "-", strSchoolTax = "-", assement_details = "-";
            string Tax_Year = "-", County_PrincipalDue = "-", County_PenaltyDue = "-", County_AmtPaid = "-", School_PrincipalDue = "-", School_PrincipalPenalty = "-", School_AmtPaid = "-";
            string ChkMulti = "-", Address = "-", City = "-", Currentowner = "-", Data_Paid = "-", Amt_Paid = "-", County_Balncedue = "-", School_Balncedue = "-";
            string County_PrincipleDue = "-", County_PenaltyDetails = "-", County_DatePaid = "-", School_PrincipleDue = "-", School_PenaltyDue = "=", School_DatePaid = "-", School_Balancedue = "-";
            string Taxyear = "-", PrincipalDue = "-", PenaltyDue = "-", DatePaid = "-", AmtPaid = "-", Balncedue = "-", OverPayment = "-";
            string BillYear = "-", TaxableValue = "-", TaxableRate = "-", TaxableAmount = "-", TaxableTotal = "-", Alternate = "-", Location = "-", Owner = "-", Customer = "-", Jurisdiction = "-", Book = "-", Acres = "-", Accssed = "-", Charges = "-";
            string Bill_Year = "-", Land = "-", Building = "-", Bill_Total = "-", Land_Class = "", Land_Description = "-", Land_Area = "-", Land_Deferments = "-", Land_Net_Assements = "-", Building_Class = "-", Building_Description = "-", Building_Area = "-", Building_Deferments = "-", Building_Net_Assements = "-", Building_Total = "-", City_year = "-", Land_Value = "-", Building_Value = "-", Personal_Value = "-", Total_Value = "-", Bill_Type = "", Sewer_Type = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new PhantomJSDriver();
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    if (searchType == "titleflex")
                    {
                        string address = houseno + " " + sname + " " + sttype + " " + unino;
                        gc.TitleFlexSearch(orderNumber, "", ownername, address, "DE", "New Castle");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_DENewCastle"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("http://www3.nccde.org/parcel/search/");
                        driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_ContentPlaceHolder1_ContentPlaceHolder1__TextBoxStreetNumber']")).SendKeys(houseno);
                        driver.FindElement(By.Id("ctl00_ctl00_ContentPlaceHolder1_ContentPlaceHolder1__RadioButtonStreetNameStartsWith")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_ContentPlaceHolder1_ContentPlaceHolder1__TextBoxStreetName']")).SendKeys(sname);

                        //Screenshot
                        gc.CreatePdf_WOP_Chrome(orderNumber, "Property Address", driver, "DE", "New Castle");

                        driver.FindElement(By.Id("ctl00_ctl00_ContentPlaceHolder1_ContentPlaceHolder1__ButtonSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        //MultiParcel
                        ChkMulti = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_ContentPlaceHolder1_ContentPlaceHolder1__UpdatePanelSummary']/div[2]/p")).Text;

                        if (ChkMulti == "There are 1 parcels matching your search criteria.")
                        {
                            driver.FindElement(By.Id("ctl00_ctl00_ContentPlaceHolder1_ContentPlaceHolder1__GridViewResults_ctl02__LinkButtonDetails")).Click();
                            Thread.Sleep(3000);
                            gc.CreatePdf_WOP_Chrome(orderNumber, "Address Details", driver, "DE", "New Castle");
                        }
                        else
                        {
                            try
                            {
                                IWebElement MultiTable = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_ContentPlaceHolder1_ContentPlaceHolder1__GridViewResults']"));
                                gc.CreatePdf_Chrome(orderNumber, parcelNumber, "ParcelSearch", driver, "DE", "New Castle");
                                IList<IWebElement> MultiTR = MultiTable.FindElements(By.TagName("tr"));
                                IList<IWebElement> MultiTD;
                                if (MultiTR.Count <= 25)
                                {
                                    foreach (IWebElement row2 in MultiTR)
                                    {

                                        MultiTD = row2.FindElements(By.TagName("td"));

                                        if (MultiTD.Count != 0)
                                        {
                                            parcelNumber = MultiTD[1].Text;
                                            Address = MultiTD[2].Text;
                                            City = MultiTD[3].Text;
                                            Currentowner = MultiTD[5].Text;
                                            string MultiData = Address + "~" + City + "~" + Currentowner;
                                            gc.insert_date(orderNumber, parcelNumber, 125, MultiData, 1, DateTime.Now);
                                        }
                                    }
                                }
                                if (MultiTR.Count <= 25)
                                {
                                    HttpContext.Current.Session["multiparcel_NCDelaware"] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                                if (MultiTR.Count > 26)

                                {
                                    HttpContext.Current.Session["multiParcel_NCDelaware_count"] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }

                            }
                            catch { }
                        }
                        try
                        {
                            string Nodata = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_ContentPlaceHolder1_ContentPlaceHolder1__UpdatePanelSummary']/div[2]/p")).Text;
                        }
                        catch { }

                    }

                    if (searchType == "parcel")
                    {
                        if (GlobalClass.titleparcel != "")
                        {
                            parcelNumber = GlobalClass.titleparcel;
                        }
                        if (GlobalClass.titleparcel.Contains(".") || GlobalClass.titleparcel.Contains("-"))
                        {
                            parcelNumber = GlobalClass.titleparcel.Replace(".", "").Replace("-", "");
                        }
                        driver.Navigate().GoToUrl("http://www3.nccde.org/parcel/search/");
                        Thread.Sleep(2000);
                        driver.FindElement(By.Id("ctl00_ctl00_ContentPlaceHolder1_ContentPlaceHolder1__TextBoxParcelNumber")).SendKeys(parcelNumber);
                        driver.FindElement(By.Id("ctl00_ctl00_ContentPlaceHolder1_ContentPlaceHolder1__ButtonSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        //Screenshot
                        gc.CreatePdf_Chrome(orderNumber, parcelNumber, "ParcelSearch", driver, "DE", "New Castle");
                        try
                        {
                            driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div[2]/div/div[2]/div[2]/table/tbody/tr[2]/td[1]")).Click();
                            Thread.Sleep(3000);
                        }
                        catch { }
                        //Screenshot                    
                        gc.CreatePdf_Chrome(orderNumber, parcelNumber, "Tax Information", driver, "DE", "New Castle");
                    }

                    try
                    {
                        IWebElement Inodata = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_ContentPlaceHolder1_ContentPlaceHolder1__UpdatePanelSummary']/div[2]/p"));
                        if(Inodata.Text.Contains("There are 0 parcels matching your search criteria"))
                        {
                            HttpContext.Current.Session["Nodata_DENewCastle"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    //Scrapped Data   

                    //parcelNumber = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_ContentPlaceHolder1_ContentPlaceHolder1__Header']")).Text;
                    parcelNumber = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_ContentPlaceHolder1_ContentPlaceHolder1__Header']")).Text;
                    parcelNumber = GlobalClass.After(parcelNumber, "# ");


                    //Property Deatails 
                    strPropertyAddress = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div[2]/div/div/div[2]/table/tbody/tr[1]/td[2]")).Text;
                    if (strPropertyAddress.Contains("\r\n"))
                    {
                        strPropertyAddress = strPropertyAddress.Replace("\r\n", ",");
                    }
                    strSubdivision = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div[2]/div/div/div[2]/table/tbody/tr[2]/td[2]")).Text;
                    strOwner = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div[2]/div/div/div[2]/table/tbody/tr[3]/td[2]")).Text;
                    strMunicipal = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div[2]/div/div/div[2]/table/tbody/tr[5]/td[2]")).Text;
                    strPropertyClass = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div[2]/div/div/div[5]/table/tbody/tr[1]/td[2]")).Text;

                    try
                    {
                        IWebElement Properttable = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div[2]/div/div/div[6]/div[8]/div/table/tbody"));
                        Thread.Sleep(3000);
                        gc.CreatePdf_Chrome(orderNumber, parcelNumber, "Tax Proprty Details", driver, "DE", "New Castle");
                        IList<IWebElement> PropertTR = Properttable.FindElements(By.TagName("tr"));
                        IList<IWebElement> PropertTD;
                        foreach (IWebElement row2 in PropertTR)
                        {
                            PropertTD = row2.FindElements(By.TagName("td"));
                            int CountValue = PropertTD.Count;
                            int i = 0;
                            for (i = 0; i < CountValue; i++)
                            {
                                if (PropertTD[i].Text == "Year Built:")
                                {

                                    stryearBuilt = PropertTD[i + 1].Text;
                                    break;
                                }

                            }
                            if (i == CountValue)
                            {
                                break;
                            }

                        }
                    }
                    catch
                    { }

                    try
                    {
                        stryearBuilt = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_ContentPlaceHolder1_ContentPlaceHolder1__RepeaterResidences_ctl00__LabelResidenceYearBuilt']")).Text;
                    }
                    catch
                    { }

                    property_details = strPropertyAddress + "~" + strSubdivision + "~" + strOwner + "~" + strMunicipal + "~" + strPropertyClass + "~" + stryearBuilt;
                    gc.insert_date(orderNumber, parcelNumber, 126, property_details, 1, DateTime.Now);

                    string Assement = "";
                    //Assessment Details
                    try
                    {
                        IWebElement TBAssement = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div[2]/div/div/div[6]"));
                        IList<IWebElement> TRAssement = TBAssement.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDAssement;
                        foreach (IWebElement row3 in TRAssement)
                        {

                            TDAssement = row3.FindElements(By.TagName("td"));
                            if (TDAssement.Count != 0)
                            {
                                if (TDAssement[0].Text == "Land:")
                                {
                                    Assement = "OK";
                                }
                            }
                            if (TDAssement.Count != 0 && Assement == "OK")
                            {
                                strLand = TDAssement[1].Text + "~" + strLand;
                            }
                            if (TDAssement.Count != 0)
                            {
                                if (TDAssement[0].Text.Trim() == "School Taxable:")
                                {
                                    Assement = "";
                                }
                            }


                        }
                        var AssessSplit = strLand.Split('~');
                        strLand = AssessSplit[5];
                        strStructure = AssessSplit[4];
                        strHomesite = AssessSplit[3];
                        strTotal = AssessSplit[2];
                        strCountyTax = AssessSplit[1];
                        strSchoolTax = AssessSplit[0];

                        assement_details = strLand + "~" + strStructure + "~" + strHomesite + "~" + strTotal + "~ " + strCountyTax + "~ " + strSchoolTax;
                        gc.insert_date(orderNumber, parcelNumber, 127, assement_details, 1, DateTime.Now);
                        gc.CreatePdf_Chrome(orderNumber, parcelNumber, "Assement Details", driver, "DE", "New Castle");
                    }
                    catch
                    { }
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    //Current Tax Details
                    IWebElement TBTax = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_ContentPlaceHolder1_ContentPlaceHolder1__divTaxHistory']/table[1]/tbody"));
                    Thread.Sleep(2000);
                    IList<IWebElement> TRTax = TBTax.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDTax;

                    foreach (IWebElement row2 in TRTax)
                    {
                        if (!row2.Text.Contains("Tax Year"))
                        {

                            TDTax = row2.FindElements(By.TagName("td"));
                            if (TDTax.Count != 0)
                            {
                                Tax_Year = TDTax[0].Text;
                                County_PrincipalDue = TDTax[1].Text;
                                County_PenaltyDue = TDTax[2].Text;
                                County_AmtPaid = TDTax[3].Text;
                                School_PrincipalDue = TDTax[4].Text;
                                School_PrincipalPenalty = TDTax[5].Text;
                                School_AmtPaid = TDTax[6].Text;

                                string tax = strPropertyAddress + "~" + strSubdivision + "~" + strOwner + "~" + Tax_Year + "~ " + County_PrincipalDue + "~ " + County_PenaltyDue + "~ " + County_AmtPaid + "~ " + School_PrincipalDue + "~ " + School_PrincipalPenalty + "~ " + School_AmtPaid;
                                gc.insert_date(orderNumber, parcelNumber, 137, tax, 1, DateTime.Now);
                                gc.CreatePdf_Chrome(orderNumber, parcelNumber, "Tax Assment Details", driver, "DE", "New Castle");

                            }

                        }

                    }

                    //Tax Payment History Details

                    IWebElement TBHistory = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_ContentPlaceHolder1_ContentPlaceHolder1__divTaxHistory']/table[2]/tbody"));
                    IList<IWebElement> TRHistory = TBHistory.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDHistory;
                    foreach (IWebElement rowtax in TRHistory)
                    {
                        if (!rowtax.Text.Contains("Date Paid"))
                        {
                            TDHistory = rowtax.FindElements(By.TagName("td"));
                            if (TDHistory.Count != 0)
                            {
                                Data_Paid = TDHistory[0].Text;
                                Amt_Paid = TDHistory[1].Text;

                                string taxHistory1 = Data_Paid + "~" + Amt_Paid + "~" + County_Balncedue + "~" + School_Balncedue;
                                gc.insert_date(orderNumber, parcelNumber, 138, taxHistory1, 1, DateTime.Now);
                                gc.CreatePdf_Chrome(orderNumber, parcelNumber, "Tax Assment Details", driver, "DE", "New Castle");
                            }
                        }
                    }

                    IWebElement TBCounty = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_ContentPlaceHolder1_ContentPlaceHolder1__divTaxHistory']/table[3]/tbody"));
                    IList<IWebElement> TRCounty = TBCounty.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDCounty;
                    foreach (IWebElement row8 in TRCounty)
                    {

                        TDCounty = row8.FindElements(By.TagName("td"));
                        if (TDCounty.Count != 0)
                        {
                            County_Balncedue = TDCounty[1].Text + "~" + County_Balncedue;
                        }

                    }
                    var CountySplit = County_Balncedue.Split('~');
                    County_Balncedue = CountySplit[1];
                    School_Balncedue = CountySplit[2];

                    string taxHistory = "-" + "~" + "-" + "~" + County_Balncedue + "~" + School_Balncedue;
                    gc.insert_date(orderNumber, parcelNumber, 138, taxHistory, 1, DateTime.Now);
                    gc.CreatePdf_Chrome(orderNumber, parcelNumber, "Tax Assment till Details", driver, "DE", "New Castle");

                    //TaxBill
                    try
                    {
                        driver.FindElement(By.Id("ctl00_ctl00_ContentPlaceHolder1_ContentPlaceHolder1__aTaxBillLink")).Click();
                        Thread.Sleep(2000);

                        List<string> urlListTaxBills = new List<string>();
                        List<string> urlListSewerBills = new List<string>();

                        IWebElement TaxBillsTB = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_ContentPlaceHolder1_ContentPlaceHolder1__DivTaxBills']/div"));
                        IList<IWebElement> TaxBillsTR = TaxBillsTB.FindElements(By.TagName("div"));
                        IList<IWebElement> TaxBillsA;
                        foreach (IWebElement TaxBills in TaxBillsTR)
                        {
                            TaxBillsA = TaxBills.FindElements(By.TagName("a"));
                            if (TaxBillsA.Count != 0)
                            {
                                urlListTaxBills.Add(TaxBillsA[0].GetAttribute("href"));
                            }
                        }
                        try
                        {
                            int i = 0;
                            String Parent_Window1 = driver.CurrentWindowHandle;
                            foreach (string tax in urlListTaxBills)
                            {
                                if (i <= 2)
                                {
                                    i++;
                                    gc.downloadfileHeader(tax, orderNumber, parcelNumber, "Tax Bill" + i, "DE", "New Castle", driver);
                                }
                            }
                            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                            driver.SwitchTo().Window(driver.WindowHandles.Last());
                            Thread.Sleep(4000);

                            driver.SwitchTo().Window(Parent_Window1);
                            Thread.Sleep(2000);
                        }
                        catch
                        { }

                        IWebElement SewerBillsTB = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_ContentPlaceHolder1_ContentPlaceHolder1__DivSewerBills']/div"));
                        IList<IWebElement> SewerBillsTR = SewerBillsTB.FindElements(By.TagName("div"));
                        IList<IWebElement> SewerBillsA;
                        foreach (IWebElement SewerBills in SewerBillsTR)
                        {
                            SewerBillsA = SewerBills.FindElements(By.TagName("a"));
                            if (SewerBillsA.Count != 0)
                            {
                                urlListSewerBills.Add(SewerBillsA[0].GetAttribute("href"));
                            }
                        }
                        try
                        {
                            int k = 0;
                            String Parent_Window2 = driver.CurrentWindowHandle;
                            foreach (string sewer in urlListSewerBills)
                            {
                                if (k <= 2)
                                {
                                    k++;
                                    gc.downloadfileHeader(sewer, orderNumber, parcelNumber, "Sewer Bill" + k, "DE", "New Castle", driver);
                                }
                            }
                            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                            driver.SwitchTo().Window(driver.WindowHandles.Last());
                            Thread.Sleep(4000);
                            driver.SwitchTo().Window(Parent_Window2);
                            Thread.Sleep(2000);
                        }
                        catch
                        { }
                    }
                    catch
                    { }
                    driver.Navigate().Back();
                    Thread.Sleep(2000);

                    //Tax History
                    IWebElement TaxSewer = driver.FindElement(By.Id("ctl00_ctl00_ContentPlaceHolder1_ContentPlaceHolder1__aTaxAndSewerOnly"));
                    IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                    js1.ExecuteScript("arguments[0].click();", TaxSewer);
                    Thread.Sleep(5000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax And Sewer", driver, "DE", "New Castle");
                    IWebElement TBSewer = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_ContentPlaceHolder1_ContentPlaceHolder1__divTaxHistory']/table[1]/tbody"));
                    IList<IWebElement> TRSewer = TBSewer.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDSewer;

                    foreach (IWebElement rowSewer in TRSewer)
                    {
                        if (!rowSewer.Text.Contains("Tax Year"))
                        {
                            TDSewer = rowSewer.FindElements(By.TagName("td"));
                            if (TDSewer.Count != 0)
                            {
                                Tax_Year = TDSewer[0].Text;
                                County_PrincipleDue = TDSewer[1].Text;
                                County_PenaltyDetails = TDSewer[2].Text;
                                County_DatePaid = TDSewer[3].Text;
                                County_AmtPaid = TDSewer[4].Text;
                                School_PrincipleDue = TDSewer[5].Text;
                                School_PenaltyDue = TDSewer[6].Text;
                                School_DatePaid = TDSewer[7].Text;
                                School_AmtPaid = TDSewer[8].Text;

                                string taxSewer = Tax_Year + "~" + County_PrincipleDue + "~" + County_PenaltyDetails + "~" + County_DatePaid + "~ " + County_AmtPaid + "~ " + School_PrincipleDue + "~ " + School_PenaltyDue + "~ " + School_DatePaid + "~ " + School_AmtPaid + "~" + "-" + "~" + "-";
                                gc.insert_date(orderNumber, parcelNumber, 139, taxSewer, 1, DateTime.Now);
                            }
                        }
                    }

                    IWebElement TBSewer1 = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_ContentPlaceHolder1_ContentPlaceHolder1__divTaxHistory']/table[2]/tbody"));
                    IList<IWebElement> TRSewer1 = TBSewer1.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDSewer1;

                    foreach (IWebElement row9 in TRSewer1)
                    {

                        TDSewer1 = row9.FindElements(By.TagName("td"));
                        if (TDSewer1.Count != 0)
                        {
                            County_Balncedue = TDSewer1[1].Text + "~" + County_Balncedue;
                        }

                    }
                    var CountySewer1 = County_Balncedue.Split('~');
                    County_Balncedue = CountySplit[1];
                    School_Balncedue = CountySplit[2];

                    string taxSewer1 = "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~ " + "-" + "~ " + "-" + "~ " + "-" + "~ " + "-" + "~ " + "-" + "~" + County_Balncedue + "~" + School_Balncedue;
                    gc.insert_date(orderNumber, parcelNumber, 139, taxSewer1, 1, DateTime.Now);
                    try
                    {
                        ByVisibleElement(driver.FindElement(By.Id("ctl00_ctl00_ContentPlaceHolder1_ContentPlaceHolder1__pSewerPenalty")));
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Sewer Last Pdf", driver, "DE", "New Castle");
                    }
                    catch { }
                    //Sewer History
                    try
                    {
                        IWebElement TBTax1 = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div[2]/div/div/div[2]/table[1]/tbody"));
                        gc.CreatePdf(orderNumber, parcelNumber, "Sewer", driver, "DE", "New Castle");
                        IList<IWebElement> TRTax1 = TBTax1.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDTax1;

                        foreach (IWebElement rowTax in TRTax1)
                        {
                            if (!rowTax.Text.Contains("Tax Year"))
                            {
                                TDTax1 = rowTax.FindElements(By.TagName("td"));
                                if (TDTax1.Count != 0)
                                {
                                    Taxyear = TDTax1[0].Text;
                                    PrincipalDue = TDTax1[1].Text;
                                    PenaltyDue = TDTax1[2].Text;
                                    DatePaid = TDTax1[3].Text;
                                    AmtPaid = TDTax1[4].Text;

                                    string taxSewer2 = Taxyear + "~" + PrincipalDue + "~" + PenaltyDue + "~" + DatePaid + "~" + AmtPaid + "~ " + "-" + "~ " + "-";
                                    gc.insert_date(orderNumber, parcelNumber, 241, taxSewer2, 1, DateTime.Now);
                                }
                            }
                        }
                    }
                    catch
                    { }

                    try
                    {
                        int i = 0;
                        IWebElement TBSewer2 = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_ContentPlaceHolder1_ContentPlaceHolder1__divSewerHistory']/table[2]/tbody"));
                        IList<IWebElement> TRSewer2 = TBSewer2.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDSewer2;

                        foreach (IWebElement row5 in TRSewer2)
                        {


                            TDSewer2 = row5.FindElements(By.TagName("td"));

                            if (i == 0)
                            {
                                Balncedue = TDSewer2[1].Text;
                                i++;
                            }
                            else if (i == 1)
                            {
                                OverPayment = TDSewer2[1].Text;
                                break;
                            }
                        }
                        string taxSewer3 = "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~ " + "-" + "~ " + Balncedue + "~" + OverPayment;
                        gc.insert_date(orderNumber, parcelNumber, 241, taxSewer3, 1, DateTime.Now);
                    }
                    catch
                    { }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    //City Tax Information
                    try
                    {
                        //City of Wilmington

                        driver.Navigate().GoToUrl("https://citizenselfservice.wilmingtonde.gov/CSS/citizens/RealEstate/Default.aspx?mode=new");
                        driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_ParcelIdSearchFieldLayout_ctl01_ParcelIDTextBox")).SendKeys(parcelNumber);
                        gc.CreatePdf_Chrome(orderNumber, parcelNumber, "CityParcelSearch", driver, "DE", "New Castle");
                        driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_FormLayoutItem7_ctl01_Button1")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);

                        //Tax Bill
                        IWebElement Tax_Bill = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[2]/table/tbody"));
                        IList<IWebElement> rows_table = Tax_Bill.FindElements(By.TagName("tr"));

                        int rows_count = rows_table.Count;
                        for (int row = 0; row < rows_count; row++)
                        {
                            if (row == rows_count - 1)
                            {
                                IList<IWebElement> Columns_row = rows_table[row].FindElements(By.TagName("td"));

                                int columns_count = Columns_row.Count;

                                for (int column = 0; column < columns_count; column++)
                                {
                                    if (column == columns_count - 2)
                                    {
                                        IWebElement a = Columns_row[column].FindElement(By.TagName("a"));
                                        a.Click();

                                    }
                                }
                            }
                        }

                        gc.CreatePdf_Chrome(orderNumber, parcelNumber, "CityViewBill", driver, "DE", "New Castle");

                        string Bill_tax_Year = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[3]/table/tbody/tr/td/table/tbody/tr[2]/td")).Text;
                        string Bill_tax = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[3]/table/tbody/tr/td/table/tbody/tr[3]/td")).Text;
                        string Bill_Owner = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[3]/table/tbody/tr/td/table/tbody/tr[4]/td")).Text;

                        //Payments&adjustements
                        driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_ViewPaymentsLinkButton']")).Click();
                        Thread.Sleep(2000);
                        try
                        {
                            string Activity = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/table[2]/tbody/tr[3]/td[1]")).Text;
                            string Posted = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/table[2]/tbody/tr[3]/td[2]")).Text;
                            string PaidBy = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/table[2]/tbody/tr[3]/td[3]")).Text;
                            string total_Amount = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/table[2]/tbody/tr[3]/td[4]")).Text;
                            driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/ul[2]/li/a")).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf_Chrome(orderNumber, parcelNumber, "ViewBillInfo", driver, "DE", "New Castle");

                            string Bill_Installment = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[3]/table/tbody/tr/td/div/table/tbody/tr[2]/td[1]")).Text;
                            string Bill_Pay_By = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[3]/table/tbody/tr/td/div/table/tbody/tr[2]/td[2]")).Text;
                            string Bill_Amount = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[3]/table/tbody/tr/td/div/table/tbody/tr[2]/td[3]")).Text;
                            string Bill_Payments_Credits = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[3]/table/tbody/tr/td/div/table/tbody/tr[2]/td[4]")).Text;
                            string Bill_Balance = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[3]/table/tbody/tr/td/div/table/tbody/tr[2]/td[5]")).Text;
                            string Bill_Interest = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[3]/table/tbody/tr/td/div/table/tbody/tr[2]/td[6]")).Text;
                            string Bill_Due = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[3]/table/tbody/tr/td/div/table/tbody/tr[2]/td[7]")).Text;

                            string Penalties_Amount = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[3]/table/tbody/tr/td/div/table/tbody/tr[3]/td[2]")).Text;
                            string Penalties_Payments_Credits = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[3]/table/tbody/tr/td/div/table/tbody/tr[3]/td[3]")).Text;
                            string Penalties_Balnce = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[3]/table/tbody/tr/td/div/table/tbody/tr[3]/td[4]")).Text;
                            string Penalties_Interest = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[3]/table/tbody/tr/td/div/table/tbody/tr[3]/td[5]")).Text;
                            string Penalties_Due = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[3]/table/tbody/tr/td/div/table/tbody/tr[3]/td[6]")).Text;

                            string Interest_Amount = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[3]/table/tbody/tr/td/div/table/tbody/tr[4]/td[2]")).Text;
                            string Interest_Payments_Credits = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[3]/table/tbody/tr/td/div/table/tbody/tr[4]/td[3]")).Text;
                            string Interest_Balnce = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[3]/table/tbody/tr/td/div/table/tbody/tr[4]/td[4]")).Text;
                            string Interest_Interest = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[3]/table/tbody/tr/td/div/table/tbody/tr[4]/td[5]")).Text;
                            string Interest_Due = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[3]/table/tbody/tr/td/div/table/tbody/tr[4]/td[6]")).Text;

                            string Total_Amount = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[3]/table/tbody/tr/td/div/table/tbody/tr[5]/td[2]")).Text;
                            string Total_Payments_Credits = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[3]/table/tbody/tr/td/div/table/tbody/tr[5]/td[3]")).Text;
                            string Total_Balnce = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[3]/table/tbody/tr/td/div/table/tbody/tr[5]/td[4]")).Text;
                            string Total_Interest = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[3]/table/tbody/tr/td/div/table/tbody/tr[5]/td[5]")).Text;
                            string Total_Due = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[3]/table/tbody/tr/td/div/table/tbody/tr[5]/td[6]")).Text;

                            string tax_Information = Bill_tax_Year + "~" + Bill_tax + "~" + Bill_Owner + "~" + Bill_Installment + "~" + Bill_Pay_By + "~" + Bill_Amount + "~" + Bill_Payments_Credits + "~" + Bill_Balance + "~" + Bill_Interest + "~" + Bill_Due + "~" + Penalties_Amount + "~" + Penalties_Payments_Credits + "~" + Penalties_Balnce + "~" + Penalties_Interest + "~" + Penalties_Due + "~" + Interest_Amount + "~" + Interest_Payments_Credits + "~" + Interest_Balnce + "~" + Interest_Interest + "~" + Interest_Due + "~" + Total_Amount + "~" + Total_Payments_Credits + "~" + Total_Balnce + "~" + Total_Interest + "~" + Total_Due;
                            gc.insert_date(orderNumber, parcelNumber, 148, tax_Information, 1, DateTime.Now);
                        }
                        catch
                        { }

                        //Charges         
                        driver.FindElement(By.XPath("/html/body/form/div[4]/div[1]/ul/li[7]/ul/li[2]/a")).Click();
                        gc.CreatePdf_Chrome(orderNumber, parcelNumber, "CityCharges", driver, "DE", "New Castle");
                        Thread.Sleep(2000);
                        BillYear = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/table[1]/tbody/tr[3]/td")).Text;
                        TaxableValue = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/table[2]/tbody/tr[2]/td[1]")).Text;
                        TaxableRate = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/table[2]/tbody/tr[2]/td[2]")).Text;
                        TaxableAmount = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/table[2]/tbody/tr[2]/td[3]")).Text;
                        TaxableTotal = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/table[2]/tbody/tr[3]/td")).Text;

                        //City Property Details
                        driver.FindElement(By.XPath("/html/body/form/div[4]/div[1]/ul/li[7]/ul/li[3]/a")).Click();
                        Thread.Sleep(2000);
                        Alternate = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/table/tbody/tr[2]/td")).Text;
                        Location = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/table/tbody/tr[3]/td")).Text;
                        Owner = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/table/tbody/tr[4]/td")).Text;
                        Customer = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/table/tbody/tr[5]/td")).Text;
                        Jurisdiction = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/table/tbody/tr[6]/td")).Text;
                        Book = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/table/tbody/tr[7]/td")).Text;
                        Acres = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/table/tbody/tr[8]/td")).Text;
                        Accssed = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/table/tbody/tr[9]/td")).Text;
                        Charges = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/table/tbody/tr[10]/td")).Text;

                        //Tax Authority
                        driver.FindElement(By.XPath("/html/body/form/div[4]/div[1]/ul/li[7]/ul/li[9]/a")).Click();
                        Thread.Sleep(2000);
                        string taxauthority = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/table/tbody/tr[3]/td")).Text;
                        driver.Navigate().Back();
                        Thread.Sleep(5000);


                        string Bill_Property = Alternate + "~" + Location + "~" + Owner + "~" + Customer + "~ " + Jurisdiction + "~ " + Book + "~ " + Acres + "~ " + Accssed + "~" + Charges + "~" + taxauthority + "~" + BillYear + "~" + TaxableValue + "~" + TaxableRate + "~" + TaxableAmount + "~" + TaxableTotal;
                        gc.insert_date(orderNumber, parcelNumber, 142, Bill_Property, 1, DateTime.Now);

                        //City Assement Details
                        driver.FindElement(By.XPath("/html/body/form/div[4]/div[1]/ul/li[7]/ul/li[5]/a")).Click();
                        Thread.Sleep(2000);
                        Bill_Year = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/table[1]/tbody/tr[3]/td")).Text;
                        Land = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/table[2]/tbody/tr[2]/td")).Text;
                        Building = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/table[2]/tbody/tr[3]/td")).Text;
                        Bill_Total = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/table[2]/tbody/tr[4]/td")).Text;
                        Land_Class = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[2]/table/tbody/tr[2]/td[2]")).Text;
                        Land_Description = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[2]/table/tbody/tr[2]/td[3]")).Text;
                        Land_Area = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[2]/table/tbody/tr[2]/td[4]")).Text;
                        Land_Deferments = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[2]/table/tbody/tr[2]/td[5]")).Text;
                        Land_Net_Assements = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[2]/table/tbody/tr[2]/td[6]")).Text;
                        try
                        {
                            Building_Class = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[2]/table/tbody/tr[3]/td[2]")).Text;
                            Building_Description = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[2]/table/tbody/tr[3]/td[3]")).Text;
                            Building_Area = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[2]/table/tbody/tr[3]/td[4]")).Text;
                            Building_Deferments = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[2]/table/tbody/tr[3]/td[5]")).Text;
                            Building_Net_Assements = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[2]/table/tbody/tr[3]/td[6]")).Text;
                            Building_Total = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[2]/table/tbody/tr[4]/td")).Text;
                        }
                        catch
                        { }

                        string Bill_Assement = Bill_Year + "~" + Land + "~" + Building + "~" + Bill_Total + "~ " + Land_Class + "~ " + Land_Description + "~ " + Land_Area + "~ " + Land_Deferments + "~" + Land_Net_Assements + "~" + Building_Class + "~" + Building_Description + "~" + Building_Area + "~" + Building_Deferments + "~" + Building_Net_Assements + "~" + Building_Total + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~ " + "-";
                        gc.insert_date(orderNumber, parcelNumber, 147, Bill_Assement, 1, DateTime.Now);

                        //Assessment History Details
                        driver.FindElement(By.XPath("/html/body/form/div[4]/div[1]/ul/li[7]/ul/li[6]/a")).Click();
                        Thread.Sleep(2000);

                        IWebElement TBCity = driver.FindElement(By.XPath("/html/body/form/div[4]/div[2]/div/div[2]/table/tbody"));
                        IList<IWebElement> TRCity = TBCity.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDCity;

                        foreach (IWebElement rowCity in TRCity)
                        {
                            if (!rowCity.Text.Contains("Year"))
                            {
                                TDCity = rowCity.FindElements(By.TagName("td"));
                                if (TDCity.Count != 0)
                                {
                                    City_year = TDCity[0].Text;
                                    Land_Value = TDCity[1].Text;
                                    Building_Value = TDCity[2].Text;
                                    Personal_Value = TDCity[3].Text;
                                    Total_Value = TDCity[4].Text;

                                    string Bill_Assement1 = "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~ " + "-" + "~ " + "-" + "~ " + "-" + "~ " + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + City_year + "~" + Land_Value + "~" + Building_Value + "~" + Personal_Value + "~" + Total_Value;
                                    gc.insert_date(orderNumber, parcelNumber, 147, Bill_Assement1, 1, DateTime.Now);
                                }
                            }
                        }
                    }
                    catch
                    { }
                    CitytaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "DE", "New Castle", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    GlobalClass.titleparcel = "";
                    driver.Quit();
                    gc.mergpdf(orderNumber, "DE", "New Castle");
                    return "Data Inserted Successfully";
                }
                catch (Exception ex)
                {
                    driver.Quit();
                    throw ex;
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