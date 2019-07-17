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
using System.Web.UI;

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_VALoudounl
    {
        string Property_Details = "", parcelno = "", Name_careOf = "", Primary = "", Primary_Address = "", Map = "", Tax_Map = "", Mailing_Address1 = "", Mailing_Address2 = "", State_Use = "", Mailing_Address = "", StateUse_Class = "", Total_Area = "", Totalland_Area = "", Ele_Dist = "", Election_District = "", Bill_Dist = "", Bill_District = "", Special_Ad = "", Special_Ad_Valorem = "", Division = "", Sub_Division = "", Year_Built = "", Legal1 = "", Legal2 = "", Legal3 = "", Legal_Description = "";
        string Assessment_Details = "", Fair_Marketland = "", FairMarket_Land = "", Fair_marketBuild = "", FiarMarket_Building = "", Bldg = "", Protaded_Bldg = "", Date = "", Effective_Date = "", FarMkt_tl = "", FairMarket_total = "", Land_USe = "", LandUse_Value = "", Ttl_Tax = "", Totltax_Value = "", Deffered = "", Deffered_Value = "", TaxExmpt_Code = "", TaxExempt_Code = "", TaxExmpt_lnd = "", TaxExempt_Land = "", TaxExmpt_bld = "", TaxExmpt_Building = "", TaxExmpt_ttl = "", TaxExmpt_Total = "", Revitalized = "", Revitalized_RealEstate = "", Solar = "", Solar_Exemption = "";
        string TaxPayment_details = "", invoice = "", year = "", Inst = "", Tax_Type = "", Due_Date = "", Date_Paid = "", Status = "", Tax_Amount = "", Penalty = "", Interest = "", Total_due = "";
        string Tax_Details = "", Tax_Ownername = "", Tx_Add = "", Tax_Address = "", Tx_lgl = "", Tax_LegalDescription = "", TX_Accnt = "", Tax_AccntNumber = "", TX_Bill = "", Tax_BillNumber = "", TX_Due = "", TAx_DueDate = "", TX_year = "", Tax_BillYear = "", TX_Inst = "", Tax_Installment = "", TX_Invoice = "", Tax_InvoiceType = "", TX_sta = "", Tax_Status = "", TX_dtpaid = "", Tax_DatePaid = "", TX_Bilamnt = "", Tax_BillAmount = "", TX_pen = "", Tax_Penalty = "", TX_Int = "", Tax_Interest = "", TX_Paid = "", Tax_PaidAmount = "", TX_blnce_Due = "", Tax_BillBalnceDue = "", Tx_bildue = "", Tax_BillsDueasof = "", Tx_bilafter = "", Tax_BillDueAfter = "", Tx_totldue = "", Tax_TotalDueforAllBills = "";
        string Taxing = "", Taxing_Authority = "", Taxauthority_Details = "", Nodata = "";

        IWebElement CurrentTB;
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_VALoudoun(string houseno, string sname, string stype, string unitnumber, string parcelNumber, string searchType, string orderNumber, string ownername, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            // driver = new ChromeDriver();
            //driver = new PhantomJSDriver()
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, parcelNumber, ownername, "", "VA", "Loudoun");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Loudoun_Zero"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("http://reparcelasmt.loudoun.gov/pt/search/commonsearch.aspx?mode=address");
                        Thread.Sleep(2000);

                        driver.FindElement(By.Id("inpNumber")).SendKeys(houseno);
                        driver.FindElement(By.Id("inpStreet")).SendKeys(sname);

                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "VA", "Loudoun");
                        driver.FindElement(By.Id("btSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        try
                        {
                            Nodata = driver.FindElement(By.XPath("//*[@id='frmMain']/table/tbody/tr/td/div/div/table[2]/tbody/tr/td/table/tbody/tr[3]/td/center/table[1]/tbody/tr[1]/td/div/p")).Text;
                            if (Nodata.Contains("Your search did not find any records."))
                            {
                                HttpContext.Current.Session["Loudoun_Zero"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("https://reparcelasmt.loudoun.gov/pt/search/commonsearch.aspx?mode=parid");
                        Thread.Sleep(2000);

                        driver.FindElement(By.Id("inpParid")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearch", driver, "VA", "Loudoun");
                        driver.FindElement(By.Id("btSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        try
                        {
                            Nodata = driver.FindElement(By.XPath("//*[@id='frmMain']/table/tbody/tr/td/div/div/table[2]/tbody/tr/td/table/tbody/tr[3]/td/center/table[1]/tbody/tr[1]/td/div/p")).Text;
                            if (Nodata.Contains("Your search did not find any records."))
                            {
                                HttpContext.Current.Session["Loudoun_Zero"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    //Property Details
                    try
                    {
                        parcelno = driver.FindElement(By.XPath("//*[@id='datalet_header_row']/td/table/tbody/tr[1]/td[1]")).Text;
                        parcelno = WebDriverTest.After(parcelno, ":").Replace("\r\n", " ").Trim();

                        Name_careOf = driver.FindElement(By.XPath("//*[@id='Owner']/tbody/tr[1]/td[2]")).Text;

                        Mailing_Address1 = driver.FindElement(By.XPath("//*[@id='Owner']/tbody/tr[3]/td[2]")).Text;
                        Mailing_Address2 = driver.FindElement(By.XPath("//*[@id='Owner']/tbody/tr[5]/td[2]")).Text;

                        Mailing_Address = Mailing_Address1 + Mailing_Address2;

                        gc.CreatePdf(orderNumber, parcelno, "Property Details", driver, "VA", "Loudoun");

                        IWebElement PropertyTB = driver.FindElement(By.XPath("//*[@id='Parcel']/tbody"));
                        IList<IWebElement> PropertyTR = PropertyTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> PropertyTD;
                        foreach (IWebElement Property in PropertyTR)
                        {
                            PropertyTD = Property.FindElements(By.TagName("td"));
                            if (PropertyTD.Count != 0 || !Property.Text.Contains(" "))
                            {
                                Primary = PropertyTD[0].Text;
                                if (Primary.Contains("Primary Address"))
                                {
                                    Primary_Address = PropertyTD[1].Text;
                                }
                                Map = PropertyTD[0].Text;
                                if (Map.Contains("Tax Map #"))
                                {
                                    Tax_Map = PropertyTD[1].Text;
                                }
                                State_Use = PropertyTD[0].Text;
                                if (State_Use.Contains("State Use Class"))
                                {
                                    StateUse_Class = PropertyTD[1].Text;
                                }
                                Total_Area = PropertyTD[0].Text;
                                if (Total_Area.Contains("Total Land Area (Acreage)"))
                                {
                                    Totalland_Area = PropertyTD[1].Text;
                                }
                                Ele_Dist = PropertyTD[0].Text;
                                if (Ele_Dist.Contains("Election District"))
                                {
                                    Election_District = PropertyTD[1].Text;
                                }
                                Bill_Dist = PropertyTD[0].Text;
                                if (Bill_Dist.Contains("Billing District"))
                                {
                                    Bill_District = PropertyTD[1].Text;
                                }
                                Special_Ad = PropertyTD[0].Text;
                                if (Special_Ad.Contains("Special Ad Valorem Tax District"))
                                {
                                    Special_Ad_Valorem = PropertyTD[1].Text;
                                }
                                Division = PropertyTD[0].Text;
                                if (Division.Contains("Subdivision"))
                                {
                                    Sub_Division = PropertyTD[1].Text;
                                }
                            }
                        }

                        Legal1 = driver.FindElement(By.XPath("//*[@id='Legal Description']/tbody/tr[1]/td[2]")).Text;
                        Legal2 = driver.FindElement(By.XPath("//*[@id='Legal Description']/tbody/tr[2]/td[2]")).Text;
                        Legal3 = driver.FindElement(By.XPath("//*[@id='Legal Description']/tbody/tr[3]/td[2]")).Text;

                        Legal_Description = Legal1 + Legal2 + Legal3;

                        driver.FindElement(By.XPath("//*[@id='sidemenu']/li[6]/a")).Click();
                        Thread.Sleep(2000);

                        IWebElement Yeartable = driver.FindElement(By.XPath("//*[@id='Primary Building']/tbody"));
                        IList<IWebElement> YearTR = Yeartable.FindElements(By.TagName("tr"));
                        IList<IWebElement> YearTD;

                        foreach (IWebElement Year in YearTR)
                        {
                            YearTD = Year.FindElements(By.TagName("td"));
                            if (YearTD.Count != 0 && Year.Text.Contains("Year Built"))
                            {
                                Year_Built = YearTD[1].Text;
                            }
                        }

                        gc.CreatePdf(orderNumber, parcelno, "YearBuilt Details", driver, "VA", "Loudoun");
                        Property_Details = Name_careOf + "~" + Mailing_Address + "~" + Primary_Address + "~" + Tax_Map + "~" + StateUse_Class + "~" + Totalland_Area + "~" + Election_District + "~" + Bill_District + "~" + Special_Ad_Valorem + "~" + Sub_Division + "~" + Legal_Description + "~" + Year_Built;
                        gc.insert_date(orderNumber, parcelno, 1815, Property_Details, 1, DateTime.Now);
                    }

                    catch
                    { }

                    //Assessment Details

                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='sidemenu']/li[2]/a")).Click();
                        Thread.Sleep(2000);

                        IWebElement AssessmentTB = driver.FindElement(By.XPath("//*[@id='2019 Values']/tbody"));
                        IList<IWebElement> AssessmentTR = AssessmentTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> AssessmentTD;
                        foreach (IWebElement Assessment in AssessmentTR)
                        {
                            AssessmentTD = Assessment.FindElements(By.TagName("td"));
                            if (AssessmentTD.Count != 0 || !Assessment.Text.Contains(" "))
                            {
                                Fair_Marketland = AssessmentTD[0].Text;
                                if (Fair_Marketland.Contains("Fair Market Land"))
                                {
                                    FairMarket_Land = AssessmentTD[1].Text;
                                }
                                Fair_marketBuild = AssessmentTD[0].Text;
                                if (Fair_marketBuild.Contains("Fair Market Building"))
                                {
                                    FiarMarket_Building = AssessmentTD[1].Text;
                                }
                                Bldg = AssessmentTD[0].Text;
                                if (Bldg.Contains("Prorated Bldg"))
                                {
                                    Protaded_Bldg = AssessmentTD[1].Text;
                                }
                                Date = AssessmentTD[0].Text;
                                if (Date.Contains("Effective Date"))
                                {
                                    Effective_Date = AssessmentTD[1].Text;
                                }
                                FarMkt_tl = AssessmentTD[0].Text;
                                if (FarMkt_tl.Contains("Fair Market Total"))
                                {
                                    FairMarket_total = AssessmentTD[1].Text;
                                }
                                Land_USe = AssessmentTD[0].Text;
                                if (Land_USe.Contains("Land Use Value"))
                                {
                                    LandUse_Value = AssessmentTD[1].Text;
                                }
                                Ttl_Tax = AssessmentTD[0].Text;
                                if (Ttl_Tax.Contains("Total Taxable Value"))
                                {
                                    Totltax_Value = AssessmentTD[1].Text;
                                }
                                Deffered = AssessmentTD[0].Text;
                                if (Deffered.Contains("*Deferred Land Use Value"))
                                {
                                    Deffered_Value = AssessmentTD[1].Text;
                                }
                                TaxExmpt_Code = AssessmentTD[0].Text;
                                if (TaxExmpt_Code.Contains("Tax Exempt Code"))
                                {
                                    TaxExempt_Code = AssessmentTD[1].Text;
                                }
                                TaxExmpt_lnd = AssessmentTD[0].Text;
                                if (TaxExmpt_lnd.Contains("Tax Exempt Land"))
                                {
                                    TaxExempt_Land = AssessmentTD[1].Text;
                                }
                                TaxExmpt_bld = AssessmentTD[0].Text;
                                if (TaxExmpt_bld.Contains("Tax Exempt Building"))
                                {
                                    TaxExmpt_Building = AssessmentTD[1].Text;
                                }
                                TaxExmpt_ttl = AssessmentTD[0].Text;
                                if (TaxExmpt_ttl.Contains("Tax Exempt Total"))
                                {
                                    TaxExmpt_Total = AssessmentTD[1].Text;
                                }
                                Revitalized = AssessmentTD[0].Text;
                                if (Revitalized.Contains("Revitalized Real Estate"))
                                {
                                    Revitalized_RealEstate = AssessmentTD[1].Text;
                                }
                                Solar = AssessmentTD[0].Text;
                                if (Solar.Contains("Solar Exemption"))
                                {
                                    Solar_Exemption = AssessmentTD[1].Text;
                                }
                            }
                        }

                        gc.CreatePdf(orderNumber, parcelno, "Assessment Details", driver, "VA", "Loudoun");
                        Assessment_Details = FairMarket_Land + "~" + FiarMarket_Building + "~" + Protaded_Bldg + "~" + Effective_Date + "~" + FairMarket_total + "~" + LandUse_Value + "~" + Totltax_Value + "~" + Deffered_Value + "~" + TaxExempt_Code + "~" + TaxExempt_Land + "~" + TaxExmpt_Building + "~" + TaxExmpt_Total + "~" + Revitalized_RealEstate + "~" + Solar_Exemption;
                        gc.insert_date(orderNumber, parcelno, 1816, Assessment_Details, 1, DateTime.Now);
                    }
                    catch
                    { }

                    //Tax Information
                    driver.Navigate().GoToUrl("https://loudounportal.com/taxes/default.aspx");
                    Thread.Sleep(2000);

                    gc.CreatePdf(orderNumber, parcelno, "TaxSearch", driver, "VA", "Loudoun");
                    driver.FindElement(By.XPath("//*[@id='ctl00_cphMainContent_SkeletonCtrl_3_btnAcceptRE']")).Click();
                    Thread.Sleep(2000);

                    driver.FindElement(By.Id("ctl00_cphMainContent_SkeletonCtrl_3_txtSearchParam")).SendKeys(parcelno);
                    gc.CreatePdf(orderNumber, parcelno, "TaxParcelSearch", driver, "VA", "Loudoun");
                    driver.FindElement(By.Id("ctl00_cphMainContent_SkeletonCtrl_3_btnSearch")).SendKeys(Keys.Enter);
                    Thread.Sleep(3000);

                    driver.FindElement(By.XPath("//*[@id='accordion2']/div/div[1]/table/tbody/tr/td[1]/a[2]")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, parcelno, "Tax Payment Details", driver, "VA", "Loudoun");
                    //TaxPayment Details
                    try
                    {
                        IWebElement TaxPaymentTB = driver.FindElement(By.XPath("//*[@id='ctl00_cphMainContent_SkeletonCtrl_3_gvRecords']/tbody"));
                        IList<IWebElement> TaxPaymentTR = TaxPaymentTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> TaxPaymentTD;

                        foreach (IWebElement TaxPayment in TaxPaymentTR)
                        {
                            TaxPaymentTD = TaxPayment.FindElements(By.TagName("td"));
                            if (TaxPaymentTD.Count != 0 && !TaxPayment.Text.Contains("Invoice"))
                            {
                                invoice = TaxPaymentTD[0].Text;
                                year = TaxPaymentTD[1].Text;
                                Inst = TaxPaymentTD[2].Text;
                                Tax_Type = TaxPaymentTD[3].Text;
                                Due_Date = TaxPaymentTD[4].Text;
                                Date_Paid = TaxPaymentTD[5].Text;
                                Status = TaxPaymentTD[6].Text;
                                Tax_Amount = TaxPaymentTD[7].Text;
                                Penalty = TaxPaymentTD[8].Text;
                                Interest = TaxPaymentTD[9].Text;
                                Total_due = TaxPaymentTD[10].Text;

                                TaxPayment_details = invoice + "~" + year + "~" + Inst + "~" + Tax_Type + "~" + Due_Date + "~" + Date_Paid + "~" + Status + "~" + Tax_Amount + "~" + Penalty + "~" + Interest + "~" + Total_due;
                                gc.insert_date(orderNumber, parcelno, 1817, TaxPayment_details, 1, DateTime.Now);
                            }
                        }
                    }
                    catch
                    { }

                    //Tax Info Details

                    IWebElement Receipttable = driver.FindElement(By.XPath("//*[@id='ctl00_cphMainContent_SkeletonCtrl_3_gvRecords']/tbody"));
                    IList<IWebElement> ReceipttableRow = Receipttable.FindElements(By.TagName("tr"));
                    int rowcount = ReceipttableRow.Count;

                    for (int p = 2; p <= rowcount; p++)
                    {
                        if (p < 8)
                        {
                            driver.FindElement(By.Id("ctl00_cphMainContent_SkeletonCtrl_3_gvRecords_ctl0" + p + "_btnSelectRecord")).Click();
                            Thread.Sleep(3000);

                            try
                            {
                                Tax_Ownername = driver.FindElement(By.XPath("/html/body/form/div[4]/div[3]/div[1]/div[5]/div/div[1]/div[1]/div[1]/table/tbody/tr/td")).Text;

                                IWebElement TaxAddresTB = driver.FindElement(By.XPath("//*[@id='ctl00_cphMainContent_SkeletonCtrl_3_sectionRealEstateInfo']/table/tbody"));
                                IList<IWebElement> TaxAddresTR = TaxAddresTB.FindElements(By.TagName("tr"));
                                IList<IWebElement> TaxAddresTD;
                                foreach (IWebElement TaxAddres in TaxAddresTR)
                                {
                                    TaxAddresTD = TaxAddres.FindElements(By.TagName("td"));
                                    if (TaxAddresTD.Count != 0 || !TaxAddres.Text.Contains(" "))
                                    {
                                        Tx_Add = TaxAddresTD[0].Text;
                                        if (Tx_Add.Contains("Address"))
                                        {
                                            Tax_Address = TaxAddresTD[1].Text;
                                        }
                                        Tx_lgl = TaxAddresTD[0].Text;
                                        if (Tx_lgl.Contains("Legal Description"))
                                        {
                                            Tax_LegalDescription = TaxAddresTD[1].Text;
                                        }
                                    }
                                }

                                IWebElement TaxBillTB = driver.FindElement(By.XPath("//*[@id='vOverview']/div[1]/div[2]/table/tbody"));
                                IList<IWebElement> TaxBillTR = TaxBillTB.FindElements(By.TagName("tr"));
                                IList<IWebElement> TaxBillTD;
                                foreach (IWebElement TaxBill in TaxBillTR)
                                {
                                    TaxBillTD = TaxBill.FindElements(By.TagName("td"));
                                    if (TaxBillTD.Count != 0 || !TaxBill.Text.Contains(" "))
                                    {
                                        TX_Accnt = TaxBillTD[0].Text;
                                        if (TX_Accnt.Contains("Account Number"))
                                        {
                                            Tax_AccntNumber = TaxBillTD[1].Text;
                                        }
                                        TX_Bill = TaxBillTD[0].Text;
                                        if (TX_Bill.Contains("Bill Number"))
                                        {
                                            Tax_BillNumber = TaxBillTD[1].Text;
                                        }
                                        TX_Due = TaxBillTD[0].Text;
                                        if (TX_Due.Contains("Due Date"))
                                        {
                                            TAx_DueDate = TaxBillTD[1].Text;
                                        }
                                        TX_year = TaxBillTD[0].Text;
                                        if (TX_year.Contains("Bill Year"))
                                        {
                                            Tax_BillYear = TaxBillTD[1].Text;
                                        }
                                        TX_Inst = TaxBillTD[0].Text;
                                        if (TX_Inst.Contains("Installment"))
                                        {
                                            Tax_Installment = TaxBillTD[1].Text;
                                        }
                                        TX_Invoice = TaxBillTD[0].Text;
                                        if (TX_Invoice.Contains("Invoice Type"))
                                        {
                                            Tax_InvoiceType = TaxBillTD[1].Text;
                                        }
                                    }
                                }

                                IWebElement TaxPaymentTB = driver.FindElement(By.XPath("//*[@id='vOverview']/div[1]/div[3]/table/tbody"));
                                IList<IWebElement> TaxPaymentTR = TaxPaymentTB.FindElements(By.TagName("tr"));
                                IList<IWebElement> TaxPaymentTD;
                                foreach (IWebElement TaxPayment in TaxPaymentTR)
                                {
                                    TaxPaymentTD = TaxPayment.FindElements(By.TagName("td"));
                                    if (TaxPaymentTD.Count != 0 || !TaxPayment.Text.Contains(" "))
                                    {
                                        TX_sta = TaxPaymentTD[0].Text;
                                        if (TX_sta.Contains("Status"))
                                        {
                                            Tax_Status = TaxPaymentTD[1].Text;
                                        }
                                        TX_dtpaid = TaxPaymentTD[0].Text;
                                        if (TX_dtpaid.Contains("Date Paid"))
                                        {
                                            Tax_DatePaid = TaxPaymentTD[1].Text;
                                        }
                                        TX_Bilamnt = TaxPaymentTD[0].Text;
                                        if (TX_Bilamnt.Contains("Bill Amount"))
                                        {
                                            Tax_BillAmount = TaxPaymentTD[1].Text;
                                        }
                                        TX_pen = TaxPaymentTD[0].Text;
                                        if (TX_pen.Contains("Penalty"))
                                        {
                                            Tax_Penalty = TaxPaymentTD[1].Text;
                                        }
                                        TX_Int = TaxPaymentTD[0].Text;
                                        if (TX_Int.Contains("Interest"))
                                        {
                                            Tax_Interest = TaxPaymentTD[1].Text;
                                        }
                                        TX_Paid = TaxPaymentTD[0].Text;
                                        if (TX_Paid.Contains("Paid Amount"))
                                        {
                                            Tax_PaidAmount = TaxPaymentTD[1].Text;
                                        }
                                        TX_blnce_Due = TaxPaymentTD[0].Text;
                                        if (TX_blnce_Due.Contains("Bill Balance Due"))
                                        {
                                            Tax_BillBalnceDue = TaxPaymentTD[1].Text;
                                        }
                                    }
                                }

                                IWebElement TaxAccountTB = driver.FindElement(By.XPath("//*[@id='vOverview']/div[2]/div[2]/table/tbody"));
                                IList<IWebElement> TaxAccountTR = TaxAccountTB.FindElements(By.TagName("tr"));
                                IList<IWebElement> TaxAccountTD;
                                foreach (IWebElement TaxAccount in TaxAccountTR)
                                {
                                    TaxAccountTD = TaxAccount.FindElements(By.TagName("td"));
                                    if (TaxAccountTD.Count != 0 || !TaxAccount.Text.Contains(" "))
                                    {
                                        Tx_bildue = TaxAccountTD[0].Text;
                                        if (Tx_bildue.Contains("Bills Due as of"))
                                        {
                                            Tax_BillsDueasof = TaxAccountTD[1].Text;
                                        }
                                        Tx_bilafter = TaxAccountTD[0].Text;
                                        if (Tx_bilafter.Contains("Bills Due after"))
                                        {
                                            Tax_BillDueAfter = TaxAccountTD[1].Text;
                                        }
                                        Tx_totldue = TaxAccountTD[0].Text;
                                        if (Tx_totldue.Contains("Total Due for All Bills"))
                                        {
                                            Tax_TotalDueforAllBills = TaxAccountTD[1].Text;
                                        }
                                    }
                                }

                                gc.CreatePdf(orderNumber, parcelno, "Tax Details" + Tax_BillYear, driver, "VA", "Loudoun");

                                Tax_Details = Tax_Ownername + "~" + Tax_Address + "~" + Tax_LegalDescription + "~" + Tax_AccntNumber + "~" + Tax_BillNumber + "~" + TAx_DueDate + "~" + Tax_BillYear + "~" + Tax_Installment + "~" + Tax_InvoiceType + "~" + Tax_BillsDueasof + "~" + Tax_BillDueAfter + "~" + Tax_TotalDueforAllBills + "~" + Tax_Status + "~" + Tax_DatePaid + "~" + Tax_BillAmount + "~" + Tax_Penalty + "~" + Tax_Interest + "~" + Tax_PaidAmount + "~" + Tax_BillBalnceDue;
                                gc.insert_date(orderNumber, parcelno, 1818, Tax_Details, 1, DateTime.Now);

                                try
                                {
                                    //Releated Invoices
                                    driver.FindElement(By.XPath("//*[@id='navTabs']/li[2]/a")).Click();
                                    Thread.Sleep(4000);

                                    gc.CreatePdf(orderNumber, parcelno, "Tax Releated Invoices Details" + Tax_BillYear, driver, "VA", "Loudoun");
                                }
                                catch
                                { }

                                try
                                {
                                    //Tax BIll
                                    driver.FindElement(By.XPath("//*[@id='ctl00_cphMainContent_SkeletonCtrl_3_tbreceipt']/a")).Click();
                                    Thread.Sleep(4000);

                                    gc.CreatePdf(orderNumber, parcelno, "Tax Bill Details" + Tax_BillYear, driver, "VA", "Loudoun");
                                }
                                catch
                                { }
                            }
                            catch
                            { }

                            driver.Navigate().Back();
                            Thread.Sleep(3000);

                            driver.FindElement(By.XPath("//*[@id='accordion2']/div/div[1]/table/tbody/tr/td[1]/a[2]")).Click();
                            Thread.Sleep(4000);
                        }
                    }

                    //Tax Authority
                    driver.Navigate().GoToUrl("https://www.loudoun.gov/taxes");
                    Thread.Sleep(2000);

                    try
                    {
                        Taxing = driver.FindElement(By.XPath("//*[@id='divInfoAdv33f025b6-5854-4397-abf2-192acc9238a9']/div[1]/div/div/ol/li/div")).Text;
                        Taxing = WebDriverTest.Between(Taxing, "Loudoun County Government", "Government Center:").Replace("\r\n", " ").Trim();

                        gc.CreatePdf(orderNumber, parcelno, "Tax Authority Details", driver, "VA", "Loudoun");

                        Taxing_Authority = Taxing;

                        Taxauthority_Details = Taxing_Authority;
                        gc.insert_date(orderNumber, parcelno, 1819, Taxauthority_Details, 1, DateTime.Now);
                    }
                    catch
                    { }


                    //City taxes
                    driver.Navigate().GoToUrl("https://leesburg.munisselfservice.com/citizens/RealEstate/Default.aspx?mode=new");
                    Thread.Sleep(2000);

                    driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_ParcelIdSearchFieldLayout_ctl01_ParcelIDTextBox")).SendKeys(parcelno);
                    gc.CreatePdf(orderNumber, parcelno, "CityParcelSearch", driver, "VA", "Loudoun");
                    driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_FormLayoutItem7_ctl01_Button1")).SendKeys(Keys.Enter);
                    Thread.Sleep(3000);

                    List<string> ParcelSearch = new List<string>();

                    try
                    {
                        IWebElement ParcelTB = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_BillsGridView']/tbody"));
                        IList<IWebElement> ParcelTR = ParcelTB.FindElements(By.TagName("tr"));
                        ParcelTR.Reverse();
                        int rows_count = ParcelTR.Count;
                        gc.CreatePdf(orderNumber, parcelno, "Town Parcel", driver, "VA", "Loudoun");
                        for (int row = 0; row <= rows_count; row++)
                        {
                            if (row == rows_count - 1 || row == rows_count - 2 || row == rows_count - 3)
                            {
                                IList<IWebElement> Columns_row = ParcelTR[row].FindElements(By.TagName("td"));

                                int columns_count = Columns_row.Count;

                                for (int column = 0; column < columns_count; column++)
                                {

                                    if (column == columns_count - 2)
                                    {
                                        IWebElement ParcelBill_link = Columns_row[column].FindElement(By.TagName("a"));
                                        string Parcelurl = ParcelBill_link.GetAttribute("id");
                                        ParcelSearch.Add(Parcelurl);
                                    }
                                }
                            }
                        }
                    }
                    catch
                    { }

                    int i = 0;
                    string currentwindow = driver.Url;

                    try
                    {

                        foreach (string Parcelbill in ParcelSearch)
                        {
                            driver.Navigate().GoToUrl(currentwindow);
                            Thread.Sleep(2000);
                            driver.FindElement(By.Id(Parcelbill)).Click();
                            Thread.Sleep(3000);
                            gc.CreatePdf(orderNumber, parcelno, "Town Bill" + i, driver, "VA", "Loudoun");
                            IWebElement test = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_AsOfDateTextBox']"));
                            string Asof = test.GetAttribute("value");
                           
                            string Billyear = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_FiscalYearLabel")).Text;
                            string Billno = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_BillNumberLabel")).Text;
                            string Ownercity = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_OwnerLabel")).Text;
                            IWebElement InstallmentTB = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_BillDetailsUpdatePanel']/table/tbody"));
                            IList<IWebElement> InstallmentTR = InstallmentTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> InstallmentTD;
                            foreach (IWebElement Installment in InstallmentTR)
                            {
                                InstallmentTD = Installment.FindElements(By.TagName("td"));
                                if (InstallmentTD.Count != 0 && !Installment.Text.Contains("TOTAL") && !Installment.Text.Contains("Pay By"))
                                {
                                    string Installmentresult = Asof + "~" + Billyear + "~" + Billno + "~" + Ownercity + "~" + InstallmentTD[0].Text + "~" + InstallmentTD[1].Text + "~" + InstallmentTD[2].Text + "~" + InstallmentTD[3].Text + "~" + InstallmentTD[4].Text + "~" + InstallmentTD[5].Text + "~" + InstallmentTD[6].Text + "~" + "";
                                    gc.insert_date(orderNumber, parcelno, 1822, Installmentresult, 1, DateTime.Now);
                                }
                                if (Installment.Text.Contains("TOTAL"))
                                {
                                    string InstallmentresultTotal = Asof + "~" + Billyear + "~" + Billno + "~" + Ownercity + "~" + InstallmentTD[0].Text + "~" + "" + "~" + InstallmentTD[1].Text + "~" + InstallmentTD[2].Text + "~" + InstallmentTD[3].Text + "~" + InstallmentTD[4].Text + "~" + InstallmentTD[5].Text + "~" + "Leesburg Town Hall  25 West Market Street  Leesburg, Virginia 20176 703-777-2420";
                                    gc.insert_date(orderNumber, parcelno, 1822, InstallmentresultTotal, 1, DateTime.Now);
                                }
                            }

                            //Payemnts&Adjustments
                            string BarCityPayBillYear = "", BarCityPayBillNo = "", Activity = "", Posted = "", PaidBy = "", Amount = "";
                            driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_ViewPaymentsLinkButton")).SendKeys(Keys.Enter);
                            Thread.Sleep(4000);
                            gc.CreatePdf(orderNumber, parcelno, "Tax Bill Details", driver, "VA", "Loudoun");
                            BarCityPayBillYear = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_FiscalYearLabel")).Text;
                            BarCityPayBillNo = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_BillNumberLabel")).Text;
                            try
                            {
                                IWebElement IBarCityPayment = driver.FindElement(By.XPath("//*[@id='molContentMainDiv']/table[2]/tbody"));
                                IList<IWebElement> IBarCityPaymentRow = IBarCityPayment.FindElements(By.TagName("tr"));
                                IList<IWebElement> IBarCityPaymentTD;
                                foreach (IWebElement Payment in IBarCityPaymentRow)
                                {
                                    IBarCityPaymentTD = Payment.FindElements(By.TagName("td"));
                                    if (IBarCityPaymentTD.Count != 0)
                                    {
                                        Activity = IBarCityPaymentTD[0].Text;
                                        Posted = IBarCityPaymentTD[1].Text;
                                        PaidBy = IBarCityPaymentTD[2].Text;
                                        Amount = IBarCityPaymentTD[3].Text;

                                        string PaymentHistoty = BarCityPayBillYear + "~" + BarCityPayBillNo + "~" + Activity + "~" + Posted + "~" + PaidBy + "~" + Amount;
                                        gc.insert_date(orderNumber, parcelno, 1856, PaymentHistoty, 1, DateTime.Now);
                                    }
                                }
                            }
                            catch { }

                            //Charges
                            driver.FindElement(By.XPath("//*[@id='primarynav']/li[6]/ul/li[2]/a")).Click();
                            Thread.Sleep(6000);
                            gc.CreatePdf(orderNumber, parcelno, "Tax Charges" + i, driver, "VA", "Loudoun");
                            IWebElement TaxChargesTB = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_TaxChargesTable"));
                            IList<IWebElement> TaxChargesTR = TaxChargesTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> TaxChargesTD;
                            IList<IWebElement> TaxChargesTH;
                            foreach (IWebElement TaxCharges in TaxChargesTR)
                            {
                                TaxChargesTD = TaxCharges.FindElements(By.TagName("td"));
                                TaxChargesTH = TaxCharges.FindElements(By.TagName("th"));
                                if (TaxChargesTD.Count > 1 && !TaxCharges.Text.Contains("Taxable Value"))
                                {
                                    string TaxChargesresult = Billyear + "~" + TaxChargesTH[0].Text + "~" + TaxChargesTD[0].Text + "~" + TaxChargesTD[1].Text + "~" + TaxChargesTD[2].Text;
                                    gc.insert_date(orderNumber, parcelno, 1824, TaxChargesresult, 1, DateTime.Now);
                                }
                                if (TaxChargesTD.Count == 1)
                                {
                                    string TaxChargesresult = Billyear + "~" + TaxChargesTH[0].Text + "~" + TaxChargesTD[0].Text;
                                    gc.insert_date(orderNumber, parcelno, 1824, TaxChargesresult, 1, DateTime.Now);
                                }
                            }

                            //Propert detail
                            driver.FindElement(By.XPath("//*[@id='primarynav']/li[6]/ul/li[3]/a")).Click();
                            Thread.Sleep(6000);
                            //IWebElement Propertdetaillink = driver.FindElement(By.XPath("//*[@id='primarynav']/li[6]/ul/li[3]")).FindElement(By.TagName("a"));
                            //string Propertyhref = Propertdetaillink.GetAttribute("href");
                            //driver.Navigate().GoToUrl(Propertyhref);
                            //Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, parcelno, "Town Property detail" + i, driver, "VA", "Loudoun");
                            string Alterparcel = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ParcelIdLabel")).Text;
                            string Location = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_LocationLabel")).Text;
                            string LegalDescription = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_LegalDescriptionLabel")).Text;
                            string Ownerasof = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_OwnerOfRecordLabel")).Text;
                            string CustomerID = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_CustomerIdLabel")).Text;
                            string Jurisdiction = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_JurisdictionLabel")).Text;
                            string Acres = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_AcresLabel")).Text;
                            string AssessedValue = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_AssessedValueLabel")).Text;
                            string chargespro = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_TaxLabel")).Text;
                            string propertydetail = Billyear + "~" + Alterparcel + "~" + Location + "~" + LegalDescription + "~" + Ownerasof + "~" + CustomerID + "~" + Jurisdiction + "~" + Acres + "~" + AssessedValue + "~" + chargespro;
                            gc.insert_date(orderNumber, parcelno, 1825, propertydetail, 1, DateTime.Now);
                             
                            //Town Assessment
                            driver.FindElement(By.XPath("//*[@id='primarynav']/li[6]/ul/li[3]/a")).Click();
                            Thread.Sleep(4000);
                            //IWebElement townassess = driver.FindElement(By.XPath("//*[@id='primarynav']/li[6]/ul/li[5]")).FindElement(By.TagName("a"));
                            //string townassesshref = townassess.GetAttribute("href");
                            //driver.Navigate().GoToUrl(townassesshref);
                            //Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, parcelno, "Town Assessment" + i, driver, "VA", "Loudoun");
                            IWebElement TownAssessmentTB = driver.FindElement(By.XPath("//*[@id='molContentMainDiv']/table[2]/tbody"));
                            IList<IWebElement> TownAssessmentTR = TownAssessmentTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> TownAssessmentTD;
                            IList<IWebElement> TownAssessmentTH;
                            foreach (IWebElement TownAssessment in TownAssessmentTR)
                            {
                                TownAssessmentTD = TownAssessment.FindElements(By.TagName("td"));
                                TownAssessmentTH = TownAssessment.FindElements(By.TagName("th"));
                                if (TownAssessmentTD.Count != 0 && !TownAssessment.Text.Contains("Gross Assessment"))
                                {
                                    string TaxChargesresult = Billyear + "~" + TownAssessmentTH[0].Text + "~" + TownAssessmentTD[0].Text;
                                    gc.insert_date(orderNumber, parcelno, 1826, TaxChargesresult, 1, DateTime.Now);
                                }
                            }

                            //Assessment
                            IWebElement AssessmentTB = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_AssessmentsGrid']/tbody"));
                            IList<IWebElement> AssessmentTR = AssessmentTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> AssessmentTD;
                            IList<IWebElement> AssessmentTH;
                            foreach (IWebElement Assessment in AssessmentTR)
                            {
                                AssessmentTD = Assessment.FindElements(By.TagName("td"));
                                AssessmentTH = Assessment.FindElements(By.TagName("th"));
                                if (AssessmentTD.Count > 1 && !Assessment.Text.Contains("Total") && !Assessment.Text.Contains("Description"))
                                {
                                    string TaxAssessmentresult = Billyear + "~" + AssessmentTD[0].Text + "~" + AssessmentTD[1].Text + "~" + AssessmentTD[2].Text + "~" + AssessmentTD[3].Text + "~" + AssessmentTD[4].Text + "~" + AssessmentTD[5].Text;
                                    gc.insert_date(orderNumber, parcelno, 1827, TaxAssessmentresult, 1, DateTime.Now);
                                }
                                if (Assessment.Text.Contains("Total"))
                                {
                                    string TaxAssessmentresultotal = Billyear + "~" + AssessmentTD[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + AssessmentTD[0].Text;
                                    gc.insert_date(orderNumber, parcelno, 1827, TaxAssessmentresultotal, 1, DateTime.Now);
                                }
                            }

                            //Assessment History
                            if (i == 0)
                            {
                                driver.FindElement(By.XPath("//*[@id='submenuselected']")).Click();
                                Thread.Sleep(4000);
                                gc.CreatePdf(orderNumber, parcelno, "Town Assessment History", driver, "VA", "Loudoun");
                                IWebElement AssessmentHistoryTB = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_BillsRepeater_ctl00_BillsGrid']/tbody"));
                                IList<IWebElement> AssessmentHistoryTR = AssessmentHistoryTB.FindElements(By.TagName("tr"));
                                IList<IWebElement> AssessmentHistoryTD;
                                IList<IWebElement> AssessmentHistoryTH;
                                foreach (IWebElement AssessmentHistory in AssessmentHistoryTR)
                                {
                                    AssessmentHistoryTD = AssessmentHistory.FindElements(By.TagName("td"));
                                    AssessmentHistoryTH = AssessmentHistory.FindElements(By.TagName("td"));
                                    if (AssessmentHistoryTD.Count != 0)
                                    {
                                        string TaxAssessmentHistoryresultotal = AssessmentHistoryTD[0].Text + "~" + AssessmentHistoryTD[1].Text + "~" + AssessmentHistoryTD[2].Text + "~" + AssessmentHistoryTD[3].Text + "~" + AssessmentHistoryTD[4].Text;
                                        gc.insert_date(orderNumber, parcelno, 1828, TaxAssessmentHistoryresultotal, 1, DateTime.Now);
                                    }
                                }

                            }
                            i++;
                        }

                    }
                    catch
                    { }
                    driver.Quit();
                    gc.mergpdf(orderNumber, "VA", "Loudoun");
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