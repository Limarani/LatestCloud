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
    public class WebDriver_FLPalmBeach
    {
        Amrock amck = new Amrock();
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        string outputpath = "-";
        string Multidata = "-", OwnerName = "-", Location = "-", Municipality = "-";
        string Acres = "-", Year_Built = "-", SubDivision = "-", ParcelNo = "-", Location_Address = "-", Municipaliti = "-", Legal_Description = "-", Property_Details;
        string Appraisals_details1 = "-", Appraisals_details2 = "-", Appraisals_details3 = "-", Appraisals_details4 = "-", Appraisals_details5 = "-", Assessed_details1 = "-", Assessed_details2 = "-", Assessed_details3 = "-", Assessed_details4 = "-", Assessed_details5 = "-", TaxesValue_details1 = "-", TaxesValue_details2 = "-", TaxesValue_details3 = "-", TaxesValue_details4 = "-", TaxesValue_details5 = "-";
        string TaxBill_Details = "-", Real_Property = "-", property_Address = "-", OwnerOf_record = "-", Tax_Authority = "-";
        string TaxpaymentDetails = "-", Bill_Year = "-", Bill_Type = "-", Bill_Number = "-", Gross_Tax = "-", Penalty_Fees = "-", Interest = "-", Discount = "-", Amount_Due = "-";
        string TaxInstallmentDetails = "-", Period = "-", Due_Date = "-", BillNumber = "-", BillYear = "-", Tax = "-", Discount1 = "-", Penality = "-", Interest1 = "-", Total_Due = "-";
        string Payment_BillYear = "-", Payment_BillNumber = "-", Receipt_Number = "-", Amount_Paid = "-", Last_Paid = "-", Paid_By = "-", Payment_Details = "-";
        string Billtaxyear = "", Ad_Valorem = "-", TaxGross_Tax = "-", Credit = "-", Net_Tax = "-", Savings = "-", TaxAssement_Details = "-", Mailing_Address = "", Owner_Name = "", Tax_Type = "", Description = "";
        public string FTP_FLPalmBeach(string address, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            string[] stringSeparators1 = new string[] { "\r\n" };
           // driver = new ChromeDriver();
            using (driver = new PhantomJSDriver())
            {

                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    if (searchType == "titleflex")
                    {
                        // string titleaddress = houseno + " " + housedir + " " + sname + " " + sttype + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, "", "", address, "FL", "Palm Beach");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_FLPalmBeach"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("http://www.pbcgov.com/PAPA/index.htm");
                        Thread.Sleep(4000);

                        IWebElement iframeElement = driver.FindElement(By.Id("master-search"));
                        Thread.Sleep(6000);
                        driver.SwitchTo().Frame(iframeElement);

                        driver.FindElement(By.Id("txtSearch")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "FL", "Palm Beach");

                        driver.FindElement(By.XPath("//*[@id='form2']/div[3]/div[1]/div/button")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        //MultiParcel
                        try
                        {
                            IWebElement MultiTable = driver.FindElement(By.Id("gvSrchResults"));
                            IList<IWebElement> MultiTR = MultiTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiTD;

                            int maxCheck = 0;

                            gc.CreatePdf_WOP(orderNumber, "MultiAddresssearch", driver, "FL", "Palm Beach");
                            foreach (IWebElement Multi in MultiTR)
                            {

                                MultiTD = Multi.FindElements(By.TagName("td"));
                                if (MultiTD.Count != 0 && !Multi.Text.Contains("Owner Name"))
                                {
                                    OwnerName = MultiTD[0].Text;
                                    Location = MultiTD[1].Text;
                                    Municipality = MultiTD[2].Text;
                                    parcelNumber = MultiTD[3].Text;

                                    Multidata = OwnerName + "~" + Location + "~" + Municipality;
                                    gc.insert_date(orderNumber, parcelNumber, 330, Multidata, 1, DateTime.Now);
                                }
                                maxCheck++;
                            }


                            if (maxCheck <= 25)
                            {
                                HttpContext.Current.Session["multiParcel_FLPalmBeach"] = "Yes";
                                GlobalClass.multiParcel_FLPalmBeach = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (MultiTR.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_FLPalmBeach_Multicount"] = "Maximum";
                                GlobalClass.multiParcel_FLPalmBeach_Multicount = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch
                        { }

                    }
                    else if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("http://www.pbcgov.com/PAPA/index.htm");
                        Thread.Sleep(2000);

                        IWebElement iframeElement = driver.FindElement(By.Id("master-search"));
                        Thread.Sleep(6000);
                        driver.SwitchTo().Frame(iframeElement);

                        driver.FindElement(By.Id("txtSearch")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearch", driver, "FL", "Palm Beach");

                        driver.FindElement(By.XPath("//*[@id='form2']/div[3]/div[1]/div/button")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                    }
                    else if (searchType == "ownername")
                    {
                        driver.Navigate().GoToUrl("http://www.pbcgov.com/PAPA/index.htm");
                        Thread.Sleep(2000);

                        IWebElement iframeElement = driver.FindElement(By.Id("master-search"));
                        Thread.Sleep(6000);
                        driver.SwitchTo().Frame(iframeElement);

                        driver.FindElement(By.Id("txtSearch")).SendKeys(ownername);
                        gc.CreatePdf(orderNumber, parcelNumber, "OwnerSearch", driver, "FL", "Palm Beach");

                        driver.FindElement(By.XPath("//*[@id='form2']/div[3]/div[1]/div/button")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        //MultiParcel
                        try
                        {
                            IWebElement MultiTable = driver.FindElement(By.Id("gvSrchResults"));
                            IList<IWebElement> MultiTR = MultiTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiTD;

                            int maxCheck = 0;

                            gc.CreatePdf_WOP(orderNumber, "MultiOwnersearch", driver, "FL", "Palm Beach");
                            foreach (IWebElement Multi in MultiTR)
                            {
                                if (maxCheck <= 25)
                                {
                                    MultiTD = Multi.FindElements(By.TagName("td"));
                                    if (MultiTD.Count != 0 && !Multi.Text.Contains("Owner Name"))
                                    {
                                        OwnerName = MultiTD[0].Text;
                                        Location = MultiTD[1].Text;
                                        Municipaliti = MultiTD[2].Text;
                                        parcelNumber = MultiTD[3].Text;

                                        Multidata = OwnerName + "~" + Location + "~" + Municipaliti;
                                        gc.insert_date(orderNumber, parcelNumber, 330, Multidata, 1, DateTime.Now);
                                    }
                                    maxCheck++;
                                }

                            }
                            HttpContext.Current.Session["multiParcel_FLPalmBeach"] = "Yes";
                            if (MultiTR.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_FLPalmBeach_Multicount"] = "Maximum";
                                return "Maximum";
                            }
                            driver.Quit();
                            return "MultiParcel";
                        }
                        catch
                        { }
                    }
                    
                    try
                    {
                        string Nodata = driver.FindElement(By.Id("MainContent_lblMsg")).Text;
                        if (Nodata == "No Results matched your search criteria. Please modify your search and try again.")
                        {
                            HttpContext.Current.Session["Nodata_FLPalmBeach"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch
                    { }

                    //Scraped Data
                    Location_Address = driver.FindElement(By.XPath("//*[@id='tdDetail']/table/tbody/tr[2]/td[2]")).Text;
                    Municipality = driver.FindElement(By.XPath("//*[@id='tdDetail']/table/tbody/tr[3]/td[2]")).Text;
                    ParcelNo = driver.FindElement(By.XPath("//*[@id='tdDetail']/table/tbody/tr[4]/td[2]")).Text;
                    amck.TaxId = ParcelNo;
                    gc.CreatePdf(orderNumber, ParcelNo, "Property Result", driver, "FL", "Palm Beach");
                    try
                    {
                        SubDivision = driver.FindElement(By.XPath("//*[@id='tdDetail']/table/tbody/tr[5]/td[2]")).Text;
                        Legal_Description = driver.FindElement(By.XPath("//*[@id='tdDetail']/table/tbody/tr[8]/td[2]")).Text;

                        IWebElement Acrestable = driver.FindElement(By.XPath("//*[@id='propertyInformationDiv']/fieldset/table[1]/tbody/tr[2]/td[1]/table[2]/tbody"));
                        IList<IWebElement> AcresTR = Acrestable.FindElements(By.TagName("tr"));
                        IList<IWebElement> AcresTD;

                        foreach (IWebElement Acre in AcresTR)
                        {
                            AcresTD = Acre.FindElements(By.TagName("td"));
                            if (AcresTD.Count != 0 && Acre.Text.Contains("Acres"))
                            {
                                Acres = AcresTD[1].Text;
                            }
                        }

                        IWebElement Yeartable = driver.FindElement(By.XPath("//*[@id='propertyInformationDiv']/fieldset/table[1]/tbody/tr[2]/td[2]/table/tbody"));
                        IList<IWebElement> YearTR = Yeartable.FindElements(By.TagName("tr"));
                        IList<IWebElement> YearTD;

                        foreach (IWebElement Year in YearTR)
                        {
                            YearTD = Year.FindElements(By.TagName("td"));
                            if (YearTD.Count != 0 && Year.Text.Contains("Year Built"))
                            {
                                Year_Built = YearTD[2].Text;
                                
                            }
                        }

                        IWebElement OwnerTable = driver.FindElement(By.XPath("//*[@id='ownerInformationDiv']/fieldset/table/tbody/tr[2]/td[1]/table/tbody"));
                        IList<IWebElement> OwnerTR = OwnerTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> OwnerTD;

                        foreach (IWebElement Owner in OwnerTR)
                        {
                            OwnerTD = Owner.FindElements(By.TagName("td"));
                            if (OwnerTD.Count != 0 && !Owner.Text.Contains("Owner(s)"))
                            {
                                Owner_Name += OwnerTD[0].Text + " ";

                            }
                        }
                        string Mailaddress1 = driver.FindElement(By.Id("MainContent_lblAddrLine3")).Text;
                        string Mailing_Address2 = driver.FindElement(By.XPath("//*[@id='ownerInformationDiv']/fieldset/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td")).Text;
                        Mailing_Address = Mailing_Address2 + " " + Mailaddress1;
                    }
                    catch
                    { }
                    Property_Details = Location_Address + "~" + Municipality + "~" + SubDivision + "~" + Legal_Description + "~" + Acres + "~" + Year_Built + "~" + Owner_Name + "~" + Mailing_Address;
                    gc.insert_date(orderNumber, ParcelNo, 338, Property_Details, 1, DateTime.Now);

                    //Appraisals Details
                    try
                    {
                        IWebElement AppraisalsTable = driver.FindElement(By.XPath("//*[@id='tblApprsal']/tbody"));
                        IList<IWebElement> AppraisalsTR = AppraisalsTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> AppraisalsTD;

                        List<string> Tax_Year = new List<string>();
                        List<string> Improvement_Value = new List<string>();
                        List<string> Land_Value = new List<string>();
                        List<string> TotalMarket_Value = new List<string>();

                        int i = 1;
                        foreach (IWebElement Appraisals in AppraisalsTR)
                        {

                            AppraisalsTD = Appraisals.FindElements(By.TagName("td"));
                            if (!Appraisals.Text.Contains("Show 5 year"))
                            {
                                if (i == 1)
                                {
                                    Tax_Year.Add(AppraisalsTD[1].Text);
                                    Tax_Year.Add(AppraisalsTD[2].Text);
                                    Tax_Year.Add(AppraisalsTD[3].Text);
                                    Tax_Year.Add(AppraisalsTD[4].Text);
                                    Tax_Year.Add(AppraisalsTD[5].Text);
                                }
                                else if (i == 2)
                                {
                                    Improvement_Value.Add(AppraisalsTD[1].Text);
                                    Improvement_Value.Add(AppraisalsTD[2].Text);
                                    Improvement_Value.Add(AppraisalsTD[3].Text);
                                    Improvement_Value.Add(AppraisalsTD[4].Text);
                                    Improvement_Value.Add(AppraisalsTD[5].Text);
                                }
                                else if (i == 3)
                                {
                                    Land_Value.Add(AppraisalsTD[1].Text);
                                    Land_Value.Add(AppraisalsTD[2].Text);
                                    Land_Value.Add(AppraisalsTD[3].Text);
                                    Land_Value.Add(AppraisalsTD[4].Text);
                                    Land_Value.Add(AppraisalsTD[5].Text);
                                }
                                else if (i == 4)
                                {
                                    TotalMarket_Value.Add(AppraisalsTD[1].Text);
                                    TotalMarket_Value.Add(AppraisalsTD[2].Text);
                                    TotalMarket_Value.Add(AppraisalsTD[3].Text);
                                    TotalMarket_Value.Add(AppraisalsTD[4].Text);
                                    TotalMarket_Value.Add(AppraisalsTD[5].Text);
                                }
                                i++;
                            }
                        }
                        Appraisals_details1 = Tax_Year[0] + "~" + Improvement_Value[0] + "~" + Land_Value[0] + "~" + TotalMarket_Value[0];
                        Appraisals_details2 = Tax_Year[1] + "~" + Improvement_Value[1] + "~" + Land_Value[1] + "~" + TotalMarket_Value[1];
                        Appraisals_details3 = Tax_Year[2] + "~" + Improvement_Value[2] + "~" + Land_Value[2] + "~" + TotalMarket_Value[2];
                        Appraisals_details4 = Tax_Year[3] + "~" + Improvement_Value[3] + "~" + Land_Value[3] + "~" + TotalMarket_Value[3];
                        Appraisals_details5 = Tax_Year[4] + "~" + Improvement_Value[4] + "~" + Land_Value[4] + "~" + TotalMarket_Value[4];
                        //gc.CreatePdf(orderNumber, ParcelNo, "Appraisls Details", driver, "FL", "Palm Beach");
                        gc.insert_date(orderNumber, ParcelNo, 342, Appraisals_details1, 1, DateTime.Now);
                        gc.insert_date(orderNumber, ParcelNo, 342, Appraisals_details2, 1, DateTime.Now);
                        gc.insert_date(orderNumber, ParcelNo, 342, Appraisals_details3, 1, DateTime.Now);
                        gc.insert_date(orderNumber, ParcelNo, 342, Appraisals_details4, 1, DateTime.Now);
                        gc.insert_date(orderNumber, ParcelNo, 342, Appraisals_details5, 1, DateTime.Now);
                    }
                    catch
                    { }

                    //Assessed Details
                    try
                    {
                        IWebElement AssessedTable = driver.FindElement(By.XPath("//*[@id='tblAssVal']/tbody"));
                        IList<IWebElement> AssessedTR = AssessedTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> AssessedTD;

                        List<string> Tax_Year = new List<string>();
                        List<string> Assessed_Value = new List<string>();
                        List<string> Exemption_Amount = new List<string>();
                        List<string> Taxable_Value = new List<string>();

                        int j = 1;
                        foreach (IWebElement Assessed in AssessedTR)
                        {

                            AssessedTD = Assessed.FindElements(By.TagName("td"));
                            if (!Assessed.Text.Contains("Show 5 year"))
                            {
                                if (j == 1)
                                {
                                    Tax_Year.Add(AssessedTD[1].Text);
                                    Tax_Year.Add(AssessedTD[2].Text);
                                    Tax_Year.Add(AssessedTD[3].Text);
                                    Tax_Year.Add(AssessedTD[4].Text);
                                    Tax_Year.Add(AssessedTD[5].Text);
                                }
                                else if (j == 2)
                                {
                                    Assessed_Value.Add(AssessedTD[1].Text);
                                    Assessed_Value.Add(AssessedTD[2].Text);
                                    Assessed_Value.Add(AssessedTD[3].Text);
                                    Assessed_Value.Add(AssessedTD[4].Text);
                                    Assessed_Value.Add(AssessedTD[5].Text);
                                }
                                else if (j == 3)
                                {
                                    Exemption_Amount.Add(AssessedTD[1].Text);
                                    Exemption_Amount.Add(AssessedTD[2].Text);
                                    Exemption_Amount.Add(AssessedTD[3].Text);
                                    Exemption_Amount.Add(AssessedTD[4].Text);
                                    Exemption_Amount.Add(AssessedTD[5].Text);
                                }
                                else if (j == 4)
                                {
                                    Taxable_Value.Add(AssessedTD[1].Text);
                                    Taxable_Value.Add(AssessedTD[2].Text);
                                    Taxable_Value.Add(AssessedTD[3].Text);
                                    Taxable_Value.Add(AssessedTD[4].Text);
                                    Taxable_Value.Add(AssessedTD[5].Text);
                                }
                                j++;
                            }
                        }
                        Assessed_details1 = Tax_Year[0] + "~" + Assessed_Value[0] + "~" + Exemption_Amount[0] + "~" + Taxable_Value[0];
                        Assessed_details2 = Tax_Year[1] + "~" + Assessed_Value[1] + "~" + Exemption_Amount[1] + "~" + Taxable_Value[1];
                        Assessed_details3 = Tax_Year[2] + "~" + Assessed_Value[2] + "~" + Exemption_Amount[2] + "~" + Taxable_Value[2];
                        Assessed_details4 = Tax_Year[3] + "~" + Assessed_Value[3] + "~" + Exemption_Amount[3] + "~" + Taxable_Value[3];
                        Assessed_details5 = Tax_Year[4] + "~" + Assessed_Value[4] + "~" + Exemption_Amount[4] + "~" + Taxable_Value[4];
                        //gc.CreatePdf(orderNumber, ParcelNo, "Assessed Details", driver, "FL", "Palm Beach");
                        gc.insert_date(orderNumber, ParcelNo, 348, Assessed_details1, 1, DateTime.Now);
                        gc.insert_date(orderNumber, ParcelNo, 348, Assessed_details2, 1, DateTime.Now);
                        gc.insert_date(orderNumber, ParcelNo, 348, Assessed_details3, 1, DateTime.Now);
                        gc.insert_date(orderNumber, ParcelNo, 348, Assessed_details4, 1, DateTime.Now);
                        gc.insert_date(orderNumber, ParcelNo, 348, Assessed_details5, 1, DateTime.Now);
                    }
                    catch
                    { }
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    //TaxesValue Details
                    try
                    {
                        IWebElement TaxesValueTable = driver.FindElement(By.XPath("//*[@id='tblTaxes']/tbody"));
                        IList<IWebElement> TaxesValueTR = TaxesValueTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> TaxesValueTD;

                        List<string> Tax_Year = new List<string>();
                        List<string> Ad_Valorem = new List<string>();
                        List<string> NonAd_Valorem = new List<string>();
                        List<string> Total_tax = new List<string>();

                        int k = 1;
                        foreach (IWebElement TaxesValue in TaxesValueTR)
                        {

                            TaxesValueTD = TaxesValue.FindElements(By.TagName("td"));
                            if (!TaxesValue.Text.Contains("Show 5 year"))
                            {
                                if (k == 1)
                                {
                                    Tax_Year.Add(TaxesValueTD[1].Text);
                                    Tax_Year.Add(TaxesValueTD[2].Text);
                                    Tax_Year.Add(TaxesValueTD[3].Text);
                                    Tax_Year.Add(TaxesValueTD[4].Text);
                                    Tax_Year.Add(TaxesValueTD[5].Text);
                                }
                                else if (k == 2)
                                {
                                    Ad_Valorem.Add(TaxesValueTD[1].Text);
                                    Ad_Valorem.Add(TaxesValueTD[2].Text);
                                    Ad_Valorem.Add(TaxesValueTD[3].Text);
                                    Ad_Valorem.Add(TaxesValueTD[4].Text);
                                    Ad_Valorem.Add(TaxesValueTD[5].Text);
                                }
                                else if (k == 3)
                                {
                                    NonAd_Valorem.Add(TaxesValueTD[1].Text);
                                    NonAd_Valorem.Add(TaxesValueTD[2].Text);
                                    NonAd_Valorem.Add(TaxesValueTD[3].Text);
                                    NonAd_Valorem.Add(TaxesValueTD[4].Text);
                                    NonAd_Valorem.Add(TaxesValueTD[5].Text);
                                }
                                else if (k == 4)
                                {
                                    Total_tax.Add(TaxesValueTD[1].Text);
                                    Total_tax.Add(TaxesValueTD[2].Text);
                                    Total_tax.Add(TaxesValueTD[3].Text);
                                    Total_tax.Add(TaxesValueTD[4].Text);
                                    Total_tax.Add(TaxesValueTD[5].Text);
                                }
                                k++;
                            }
                        }
                        TaxesValue_details1 = Tax_Year[0] + "~" + Ad_Valorem[0] + "~" + NonAd_Valorem[0] + "~" + Total_tax[0];
                        TaxesValue_details2 = Tax_Year[1] + "~" + Ad_Valorem[1] + "~" + NonAd_Valorem[1] + "~" + Total_tax[1];
                        TaxesValue_details3 = Tax_Year[2] + "~" + Ad_Valorem[2] + "~" + NonAd_Valorem[2] + "~" + Total_tax[2];
                        TaxesValue_details4 = Tax_Year[3] + "~" + Ad_Valorem[3] + "~" + NonAd_Valorem[3] + "~" + Total_tax[3];
                        TaxesValue_details5 = Tax_Year[4] + "~" + Ad_Valorem[4] + "~" + NonAd_Valorem[4] + "~" + Total_tax[4];
                        //gc.CreatePdf(orderNumber, ParcelNo, "TaxValues Details", driver, "FL", "Palm Beach");
                        gc.insert_date(orderNumber, ParcelNo, 349, TaxesValue_details1, 1, DateTime.Now);
                        gc.insert_date(orderNumber, ParcelNo, 349, TaxesValue_details2, 1, DateTime.Now);
                        gc.insert_date(orderNumber, ParcelNo, 349, TaxesValue_details3, 1, DateTime.Now);
                        gc.insert_date(orderNumber, ParcelNo, 349, TaxesValue_details4, 1, DateTime.Now);
                        gc.insert_date(orderNumber, ParcelNo, 349, TaxesValue_details5, 1, DateTime.Now);
                    }
                    catch
                    { }

                    //Tax Information Details
                    driver.Navigate().GoToUrl("https://pbctax.manatron.com/Tabs/PropertyTax.aspx");
                    Thread.Sleep(2000);

                    var SelectParcel = driver.FindElement(By.Id("selSearchBy"));
                    var SelectParcelTax = new SelectElement(SelectParcel);
                    SelectParcelTax.SelectByText("Property Control Number");

                    driver.FindElement(By.Id("fldInput")).SendKeys(ParcelNo);
                    driver.FindElement(By.Id("btnsearch")).SendKeys(Keys.Enter);
                    Thread.Sleep(2000);

                    gc.CreatePdf(orderNumber, ParcelNo, "ParcelTax Details", driver, "FL", "Palm Beach");
                    driver.FindElement(By.XPath("//*[@id='grm-search']/tbody/tr[2]/td[6]/a")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, ParcelNo, "Tax Summary Details", driver, "FL", "Palm Beach");

                    Real_Property = driver.FindElement(By.XPath("//*[@id='lxT506']/table/tbody/tr[2]/td[2]")).Text;
                    property_Address = driver.FindElement(By.XPath("//*[@id='lxT506']/table/tbody/tr[3]/td/table/tbody/tr[1]/td[2]")).Text;
                    property_Address = WebDriverTest.After(property_Address, "Property Address:").Trim();
                    string[] linesName = property_Address.Split(stringSeparators1, StringSplitOptions.None);
                    OwnerOf_record = driver.FindElement(By.XPath("//*[@id='lxT506']/table/tbody/tr[3]/td/table/tbody/tr[2]/td[1]")).Text;
                    OwnerOf_record = WebDriverTest.After(OwnerOf_record, "Owner of Record").Trim();
                    string[] linesName1 = OwnerOf_record.Split(stringSeparators1, StringSplitOptions.None);
                    driver.Navigate().GoToUrl("https://www.pbctax.com/content/help");
                    Thread.Sleep(2000);
                    Tax_Authority = driver.FindElement(By.XPath("//*[@id='content-area']/div/div/div/div[1]/p")).Text.Replace("Mailing Address", "").Trim();
                    gc.CreatePdf(orderNumber, ParcelNo, "Tax Authority", driver, "FL", "Palm Beach");
                    driver.Navigate().Back();
                    Thread.Sleep(2000);
                    TaxBill_Details = Real_Property + "~" + property_Address + "~" + OwnerOf_record + "~" + Tax_Authority;
                    gc.insert_date(orderNumber, ParcelNo, 357, TaxBill_Details, 1, DateTime.Now);

                    //Tax Payment History Details
                    List<string> billinfo = new List<string>();
                    try
                    {
                        IWebElement TaxPaymentTB = driver.FindElement(By.XPath("//*[@id='508']/table/tbody"));
                        IList<IWebElement> TaxPaymentTR = TaxPaymentTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> TaxPaymentTD;
                        int a = 0;
                        //gc.CreatePdf(orderNumber, ParcelNo, "Tax Payment Details", driver, "FL", "Palm Beach");
                        foreach (IWebElement TaxPayment in TaxPaymentTR)
                        {
                            TaxPaymentTD = TaxPayment.FindElements(By.TagName("td"));
                            if (TaxPaymentTD.Count != 0)
                            {
                                if (a < 3)
                                {
                                    if (TaxPaymentTD.Count != 0 && !TaxPayment.Text.Contains("Bill Year"))
                                    {
                                        IWebElement value1 = TaxPaymentTD[0].FindElement(By.TagName("a"));
                                        string addview = value1.GetAttribute("href");
                                        billinfo.Add(addview);
                                    }
                                    a++;
                                }
                                Bill_Year = TaxPaymentTD[0].Text;
                                Bill_Type = TaxPaymentTD[1].Text;
                                Bill_Number = TaxPaymentTD[2].Text;
                                Gross_Tax = TaxPaymentTD[3].Text;
                                Penalty_Fees = TaxPaymentTD[4].Text;
                                Interest = TaxPaymentTD[5].Text;
                                Discount = TaxPaymentTD[6].Text;
                                Amount_Due = TaxPaymentTD[7].Text;
                                TaxpaymentDetails = Bill_Year + "~" + Bill_Type + "~" + Bill_Number + "~" + Gross_Tax + "~" + Penalty_Fees + "~" + Interest + "~" + Discount + "~" + Amount_Due;
                                gc.insert_date(orderNumber, ParcelNo, 359, TaxpaymentDetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch
                    { }
                    //Tax Bill
                    int s = 0;
                    foreach (string taxinfoclick in billinfo)
                    {
                        try
                        {
                            driver.Navigate().GoToUrl(taxinfoclick);
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, ParcelNo, "TaxBill_priorYear" + s, driver, "FL", "Palm Beach");
                            Billtaxyear = driver.FindElement(By.XPath("//*[@id='lxT538']/h1")).Text.Replace("Bill Detail", "").Trim();
                            //Tax And Assement Details//
                            IWebElement TaxAssementTB = driver.FindElement(By.XPath("//*[@id='dnn_ContentPane']/div[4]"));
                            IList<IWebElement> TaxAssementTR = TaxAssementTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> TaxAssementTD;
                            int taxrowcount = TaxAssementTR.Count;
                            int b = 1; /*string pathid = "";*/
                            foreach (IWebElement TaxAssement in TaxAssementTR)
                            {
                                TaxAssementTD = TaxAssement.FindElements(By.TagName("td"));
                                if (TaxAssement.Text.Contains("Ad Valorem"))
                                {

                                    Tax_Type = "Ad Valorem";
                                }
                                if (TaxAssement.Text.Contains("Non Ad Valorem"))
                                {
                                    Tax_Type = "Non Ad Valorem";
                                }

                                if (TaxAssementTD.Count != 0 && !TaxAssement.Text.Contains("Ad Valorem") && TaxAssement.Text != "" && TaxAssementTD.Count != 1 && !TaxAssement.Text.Contains("Non Ad Valorem"))
                                {
                                    if (TaxAssementTD.Count == 4 && !TaxAssement.Text.Contains("Total Tax"))
                                    {
                                        Description = "Sub Total";
                                        TaxGross_Tax = TaxAssementTD[0].Text;
                                        Credit = TaxAssementTD[1].Text;
                                        Net_Tax = TaxAssementTD[2].Text;
                                        Savings = TaxAssementTD[3].Text;
                                        TaxAssement_Details = Billtaxyear + "~" + Tax_Type + "~" + Description + "~" + TaxGross_Tax + "~" + Credit + "~" + Net_Tax + "~" + Savings;
                                        gc.insert_date(orderNumber, ParcelNo, 363, TaxAssement_Details, 1, DateTime.Now);
                                    }
                                    else
                                    {
                                        if (b % 2 == 0 && b != taxrowcount)
                                        {
                                            try
                                            {
                                                //pathid = driver.FindElement(By.XPath("//*[@id='lxT512']/table/tbody/tr["+ b +"]/td[1]")).GetAttribute("tb");
                                                driver.FindElement(By.XPath("//*[@id='lxT512']/table/tbody/tr[" + b + "]/td[1]/a")).SendKeys(Keys.Enter);
                                                Thread.Sleep(2000);
                                                //gc.CreatePdf(orderNumber, ParcelNo, "Ad Valorem and Non Ad Valorem" + b, driver, "FL", "Palm Beach");

                                            }
                                            catch
                                            {

                                            }
                                            Description = TaxAssementTD[0].Text;
                                            TaxGross_Tax = TaxAssementTD[1].Text;
                                            Credit = TaxAssementTD[2].Text;
                                            Net_Tax = TaxAssementTD[3].Text;
                                            Savings = TaxAssementTD[4].Text;
                                            TaxAssement_Details = Billtaxyear + "~" + Tax_Type + "~" + Description + "~" + TaxGross_Tax + "~" + Credit + "~" + Net_Tax + "~" + Savings;
                                            gc.insert_date(orderNumber, ParcelNo, 363, TaxAssement_Details, 1, DateTime.Now);
                                            Description = ""; TaxGross_Tax = ""; Credit = ""; Net_Tax = ""; Savings = "";
                                        }
                                    }
                                    if (TaxAssement.Text.Contains("Total Tax") && !TaxAssement.Text.Contains("Sub Total"))
                                    {
                                        Description = "Total Tax";
                                        TaxGross_Tax = TaxAssementTD[0].Text;
                                        Credit = TaxAssementTD[1].Text;
                                        Net_Tax = TaxAssementTD[2].Text;
                                        if(Tax_Type == "Non Ad Valorem")
                                        {
                                            amck.Instamount1 = Net_Tax;
                                        }
                                        Savings = TaxAssementTD[3].Text;
                                        TaxAssement_Details = Billtaxyear + "~" + Tax_Type + "~" + Description + "~" + TaxGross_Tax + "~" + Credit + "~" + Net_Tax + "~" + Savings;
                                        gc.insert_date(orderNumber, ParcelNo, 363, TaxAssement_Details, 1, DateTime.Now);
                                    }

                                }
                                b++;
                            }
                        }
                        catch { }
                        gc.CreatePdf(orderNumber, ParcelNo, "Ad Valorem and Non Ad Valorem" + Billtaxyear, driver, "FL", "Palm Beach");
                        //Tax Installment Details

                        try
                        {
                            IWebElement TaxInstallmentTB = driver.FindElement(By.XPath("//*[@id='dnn_ContentPane']/div[5]"));
                            IList<IWebElement> TaxInstallmentTR = TaxInstallmentTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> TaxInstallmentTD;

                            foreach (IWebElement TaxInstallment in TaxInstallmentTR)
                            {
                                TaxInstallmentTD = TaxInstallment.FindElements(By.TagName("td"));

                                if (TaxInstallmentTD.Count != 0 && !TaxInstallment.Text.Contains("Period"))
                                {
                                    if (TaxInstallmentTD[0].Text.Trim() == "Total Due:")
                                    {
                                        Period = "";
                                        BillNumber = "";
                                        Due_Date = "";
                                        BillYear = TaxInstallmentTD[0].Text;
                                        Tax = TaxInstallmentTD[1].Text;
                                        Discount1 = TaxInstallmentTD[2].Text;
                                        Penality = TaxInstallmentTD[3].Text;
                                        Interest1 = TaxInstallmentTD[4].Text;
                                        Total_Due = TaxInstallmentTD[5].Text;
                                        if (s == 0)
                                        {
                                            if (TaxInstallmentTR.Count == 3)
                                            {
                                                if (Total_Due == "$0.00")
                                                {
                                                    amck.IsDelinquent = "No";
                                                    amck.InstPaidDue1 = "Paid";
                                                    gc.InsertAmrockTax(orderNumber, amck.TaxId, amck.Instamount1, amck.Instamount2, amck.Instamount3, amck.Instamount4, amck.Instamountpaid1, amck.Instamountpaid2, amck.Instamountpaid3, amck.Instamountpaid4, amck.InstPaidDue1, amck.InstPaidDue2, amck.instPaidDue3, amck.instPaidDue4, amck.IsDelinquent);
                                                }
                                                else if (Total_Due != "$0.00" && Interest1 != "$0.00")
                                                {
                                                    amck.IsDelinquent = "Yes";
                                                    amck.InstPaidDue1 = "Due";
                                                    gc.InsertAmrockTax(orderNumber, amck.TaxId, amck.Instamount1, amck.Instamount2, amck.Instamount3, amck.Instamount4, amck.Instamountpaid1, amck.Instamountpaid2, amck.Instamountpaid3, amck.Instamountpaid4, amck.InstPaidDue1, amck.InstPaidDue2, amck.instPaidDue3, amck.instPaidDue4, amck.IsDelinquent);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Period = TaxInstallmentTD[0].Text;
                                        BillNumber = TaxInstallmentTD[1].Text;
                                        Due_Date = TaxInstallmentTD[2].Text;
                                        BillYear = TaxInstallmentTD[3].Text;
                                        if (BillYear != "")
                                        {
                                            amck.TaxYear = BillYear;
                                        }
                                        Tax = TaxInstallmentTD[4].Text;
                                        Discount1 = TaxInstallmentTD[5].Text;
                                        Penality = TaxInstallmentTD[6].Text;
                                        Interest1 = TaxInstallmentTD[7].Text;
                                        Total_Due = TaxInstallmentTD[8].Text;
                                    }

                                    TaxInstallmentDetails = Period + "~" + BillNumber + "~" + Due_Date + "~" + BillYear + "~" + Tax + "~" + Discount1 + "~" + Penality + "~" + Interest1 + "~" + Total_Due;
                                    gc.insert_date(orderNumber, ParcelNo, 361, TaxInstallmentDetails, 1, DateTime.Now);
                                }
                            }
                        }
                        catch
                        { }
                        //Tax Payment details
                        try
                        {
                            IWebElement PaymentTB = driver.FindElement(By.XPath("//*[@id='dnn_ContentPane']/div[7]"));
                            IList<IWebElement> PaymentTR = PaymentTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> PaymentTD;

                            foreach (IWebElement Payment in PaymentTR)
                            {
                                PaymentTD = Payment.FindElements(By.TagName("td"));

                                if (PaymentTD.Count != 0 && !Payment.Text.Contains("Bill Year"))
                                {
                                    Payment_BillYear = PaymentTD[0].Text;
                                    Payment_BillNumber = PaymentTD[1].Text;
                                    Receipt_Number = PaymentTD[2].Text;
                                    Amount_Paid = PaymentTD[3].Text;
                                    if (Amount_Paid != "")
                                    {
                                        amck.Instamountpaid1 = Amount_Paid;
                                    }
                                    Last_Paid = PaymentTD[4].Text;
                                    Paid_By = PaymentTD[5].Text;

                                    Payment_Details = Payment_BillYear + "~" + Payment_BillNumber + "~" + Receipt_Number + "~" + Amount_Paid + "~" + Last_Paid + "~" + Paid_By;
                                    gc.insert_date(orderNumber, ParcelNo, 362, Payment_Details, 1, DateTime.Now);
                                }
                            }
                        }
                        catch
                        { }
                        //Tax Bill Dowload
                        try
                        {

                            IWebElement taxinfotable = driver.FindElement(By.XPath("//*[@id='lxT538']/p"));
                            IList<IWebElement> viwetaxbill = taxinfotable.FindElements(By.TagName("a"));
                            foreach (IWebElement taxyearelement in viwetaxbill)
                            {
                                if (taxyearelement.Text.Contains("Print Tax Bill"))
                                {
                                    string viewhref = taxyearelement.GetAttribute("href");
                                    gc.downloadfile(viewhref, orderNumber, ParcelNo, "ViewTaxBill.pdf" + s, "FL", "Palm Beach");
                                }
                            }
                            s++;
                        }
                        catch { }
                    }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");

                   
                    gc.insert_TakenTime(orderNumber, "FL", "Palm Beach", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    //megrge pdf files
                    gc.mergpdf(orderNumber, "FL", "Palm Beach");
                    return "Data Inserted Successfully";
                }
                catch (Exception ex)
                {
                    driver.Quit();
                    throw ex;
                }
            }
        }

    }
}