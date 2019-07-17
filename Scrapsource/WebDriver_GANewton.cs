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
    public class WebDriver_GANewton
    {
        int b = 1;
        string addnew = "";
        string AssessmentYear = "", PreviousValue = "", LandValue = "", ImprovementValue = "", AccessoryValue = "", CurrentValue = "";
        string AssessmentYear1 = "", PreviousValue1 = "", LandValue1 = "", ImprovementValue1 = "", AccessoryValue1 = "", CurrentValue1 = "", comment = "";
        string ParcelID = "", ShortParcelID = "", PropertyAddress = "", OwnerName = "", MailingAddress = "", LegalDescription = "", PropertyClass = "", TaxDistrict = "", MillageRate = "", HomesteadExemption = "", YearBuilt = "", neighbourhood = "";
        int m = 1;
        int i = 0;
        string account = "", owner1 = "", owner2 = "", propadd1 = "", propadd2 = "", property_type = "", bill_no = "", tax_year = "", TaxAmount = "", FairMarketValue = "", AssessedValue = "", UnderAppeal = "", TotalDue = "", DueDate = "", PaymentStatus = "", PaidDate = "", PaidAmount = "", TotalDuepay = "";
        int n = 0, q = 0;
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_GANewton(string address, string assessment_id, string parcelNumber, string searchType, string orderNumber, string directParcel, string ownername)
        {
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "", AssessTakenTime = "", TaxTakentime = "", CityTaxtakentime = "";
            string TotaltakenTime = "";

            IList<IWebElement> TRmulti;
            IWebElement tbmulti;
            IList<IWebElement> TDmulti;
            List<IWebElement> text = new List<IWebElement>();
            List<string> strTaxRealestate1 = new List<string>();

            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new PhantomJSDriver();
            //driver = new ChromeDriver();
            using (driver = new ChromeDriver()) 
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    if (searchType == "titleflex")
                    {

                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "GA", "Newton");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_GANewton"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    driver.Navigate().GoToUrl("https://qpublic.schneidercorp.com/Application.aspx?AppID=794&LayerID=11825&PageTypeID=2&PageID=5724");
                    Thread.Sleep(6000);
                    try
                    {
                        IWebElement ISpan12 = driver.FindElement(By.XPath("//*[@id='appBody']/div[4]/div/div/div[3]/a[1]"));
                        IJavaScriptExecutor js12 = driver as IJavaScriptExecutor;
                        js12.ExecuteScript("arguments[0].click();", ISpan12);
                        Thread.Sleep(3000);
                    }
                    catch { }
                    if (searchType == "address")
                    {

                        driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_txtAddress")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "GA", "Newton");
                        driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl01_ctl01_btnSearch']")).SendKeys(Keys.Enter);

                        Thread.Sleep(3000);
                        try
                        {
                            if (driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_ctl01_gvwParcelResults']/tbody")).Displayed)
                            {
                                gc.CreatePdf_WOP(orderNumber, "Address search result", driver, "GA", "Newton");
                                IWebElement tbmulti5 = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_ctl01_gvwParcelResults']/tbody"));
                                IList<IWebElement> TRmulti5 = tbmulti5.FindElements(By.TagName("tr"));
                                int maxCheck = 0;
                                IList<IWebElement> TDmulti5;
                                foreach (IWebElement row in TRmulti5)
                                {
                                    if (maxCheck <= 25)
                                    {
                                        TDmulti5 = row.FindElements(By.TagName("td"));
                                        if (TDmulti5.Count != 0 && TDmulti5[3].Text.Trim() != "")
                                        {
                                            string multi1 = TDmulti5[3].Text + "~" + TDmulti5[4].Text;
                                            gc.insert_date(orderNumber, TDmulti5[1].Text, 527, multi1, 1, DateTime.Now);
                                            //   Owner~Property Address
                                        }
                                        maxCheck++;
                                    }

                                }

                                if (TRmulti5.Count > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_GANewton_Multicount"] = "Maximum";
                                }
                                else
                                {
                                    HttpContext.Current.Session["multiparcel_GANewton"] = "Yes";
                                }
                                driver.Quit();

                                return "MultiParcel";

                            }
                        }
                        catch { }
                    }
                    else if (searchType == "parcel")
                    {

                        driver.FindElement(By.Id("ctlBodyPane_ctl02_ctl01_txtParcelID")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search ", driver, "GA", "Newton");
                        driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl02_ctl01_btnSearch']")).SendKeys(Keys.Enter);
                        //   gc.CreatePdf_WOP(orderNumber, "Address search result", driver, "CA", "Yolo");
                        Thread.Sleep(6000);

                    }

                    else if (searchType == "ownername")
                    {

                        driver.FindElement(By.Id("ctlBodyPane_ctl00_ctl01_txtName")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "Owner Search", driver, "GA", "Newton");
                        driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_ctl01_btnSearch']")).SendKeys(Keys.Enter);

                        Thread.Sleep(6000);
                        try
                        {
                            if (driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_ctl01_gvwParcelResults']/tbody")).Displayed)
                            {
                                gc.CreatePdf_WOP(orderNumber, "Owner search result", driver, "GA", "Newton");
                                IWebElement tbmulti5 = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_ctl01_gvwParcelResults']/tbody"));
                                IList<IWebElement> TRmulti5 = tbmulti5.FindElements(By.TagName("tr"));
                                int k = 0;
                                IList<IWebElement> TDmulti5;
                                foreach (IWebElement row in TRmulti5)
                                {
                                    if (k < 25)
                                    {
                                        TDmulti5 = row.FindElements(By.TagName("td"));
                                        if (TDmulti5.Count != 0 && TDmulti5[3].Text.Trim() != "")
                                        {
                                            string multi1 = TDmulti5[3].Text + "~" + TDmulti5[4].Text;
                                            gc.insert_date(orderNumber, TDmulti5[1].Text, 527, multi1, 1, DateTime.Now);

                                        }
                                    }
                                    k++;

                                }
                                HttpContext.Current.Session["multiparcel_GANewton"] = "Yes";
                                if (TRmulti5.Count > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_GANewton_Multicount"] = "Maximum";
                                }
                                driver.Quit();

                                return "MultiParcel";

                            }
                        }
                        catch { }
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.Id("ctlBodyPane_noDataList_pnlNoResults"));
                        if(INodata.Text.Contains("No results match your search criteria"))
                        {
                            HttpContext.Current.Session["Nodata_GANewton"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    //property details
                    //Parcel ID~Short Parcel ID~Property Address~Owner Name~Mailing Address~Legal Description~Property Class~Tax District~Millage Rate~Homestead Exemption~Year Built                

                    IWebElement tbmulti11 = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_ctl01_dvNonPrebillMH']/table/tbody"));
                    IList<IWebElement> TRmulti11 = tbmulti11.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDmulti11;
                    foreach (IWebElement row in TRmulti11)
                    {

                        TDmulti11 = row.FindElements(By.TagName("td"));
                        if (TDmulti11.Count != 0)
                        {
                            if (i == 0)
                                ParcelID = TDmulti11[1].Text;
                            if (i == 1)
                                ShortParcelID = TDmulti11[1].Text;
                            if (i == 2)
                                PropertyAddress = TDmulti11[1].Text;
                            if (i == 4)
                                LegalDescription = TDmulti11[1].Text;
                            if (i == 6)
                                PropertyClass = TDmulti11[1].Text;
                            if (i == 9)
                                TaxDistrict = TDmulti11[1].Text;
                            if (i == 10)
                                MillageRate = TDmulti11[1].Text;
                            if (i == 12)
                                neighbourhood = TDmulti11[1].Text;
                            if (i == 13)
                                HomesteadExemption = TDmulti11[1].Text;
                            i++;
                        }
                    }
                    if (neighbourhood.Contains("Multi-Family"))
                    {
                        HttpContext.Current.Session["multiFamily_GANewton"] = "Yes";
                        driver.Quit();

                        gc.mergpdf(orderNumber, "GA", "Newton");
                        return "MultiParcel";
                    }
                    if (TaxDistrict.Contains("OXFORD"))
                    {
                        comment = "Need to check City taxes";
                    }

                    try
                    {
                        OwnerName = driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_lnkOwnerName_lblSearch")).Text.Trim();
                    }
                    catch
                    {
                        OwnerName = driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_lnkOwnerName_lnkSearch")).Text.Trim();
                    }
                    MailingAddress = driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_lblAddress")).Text.Trim() + " " + driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_lblCityStateZip")).Text.Trim();
                    try
                    {
                        YearBuilt = driver.FindElement(By.Id("ctlBodyPane_ctl05_ctl01_rptResidential_ctl00_lblYearBuilt")).Text.Trim();
                    }
                    catch { }
                    string property = ParcelID + "~" + ShortParcelID + "~" + PropertyAddress + "~" + OwnerName + "~" + MailingAddress + "~" + LegalDescription + "~" + PropertyClass + "~" + TaxDistrict + "~" + MillageRate + "~" + HomesteadExemption + "~" + YearBuilt + "~" + comment;
                    gc.insert_date(orderNumber, ParcelID, 522, property, 1, DateTime.Now);

                    gc.CreatePdf(orderNumber, ParcelID, "Property details ", driver, "GA", "Newton");

                    //assessment details
                    //  Assessment Year~Previous Value~Land Value~Improvement Value~Accessory Value~Current Value

                    int j = 0;
                    IWebElement tbmulti1 = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl13_ctl01_grdValuation']"));
                    IList<IWebElement> TRmulti1 = tbmulti1.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDmulti1;
                    IList<IWebElement> THmulti1;
                    foreach (IWebElement row in TRmulti1)
                    {
                        THmulti1 = row.FindElements(By.TagName("th"));
                        TDmulti1 = row.FindElements(By.TagName("td"));
                        if (TDmulti1.Count != 0 || THmulti1.Count != 0)
                        {
                            if (j == 0)
                            {
                                AssessmentYear = THmulti1[0].Text;
                                AssessmentYear1 = THmulti1[1].Text;
                            }
                            if (j == 1)
                            {
                                PreviousValue = TDmulti1[2].Text;
                                PreviousValue1 = TDmulti1[3].Text;
                            }
                            if (j == 2)
                            {
                                LandValue = TDmulti1[2].Text;
                                LandValue1 = TDmulti1[3].Text;
                            }
                            if (j == 3)
                            {
                                ImprovementValue = TDmulti1[2].Text;
                                ImprovementValue1 = TDmulti1[3].Text;
                            }
                            if (j == 4)
                            {
                                AccessoryValue = TDmulti1[2].Text;
                                AccessoryValue1 = TDmulti1[3].Text;
                            }
                            if (j == 5)
                            {
                                CurrentValue = TDmulti1[2].Text;
                                CurrentValue1 = TDmulti1[3].Text;
                            }
                            j++;
                        }
                    }

                    string assessment = AssessmentYear + "~" + PreviousValue + "~" + LandValue + "~" + ImprovementValue + "~" + AccessoryValue + "~" + CurrentValue;
                    gc.insert_date(orderNumber, ParcelID, 523, assessment, 1, DateTime.Now);
                    string assessment1 = AssessmentYear1 + "~" + PreviousValue1 + "~" + LandValue1 + "~" + ImprovementValue1 + "~" + AccessoryValue1 + "~" + CurrentValue1;
                    gc.insert_date(orderNumber, ParcelID, 523, assessment1, 1, DateTime.Now);

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("https://www.newtoncountytax.com/taxes.html#/WildfireSearch");
                    Thread.Sleep(5000);
                    IWebElement ISpan13 = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[3]/button[2]/i"));
                    IJavaScriptExecutor js13 = driver as IJavaScriptExecutor;
                    js13.ExecuteScript("arguments[0].click();", ISpan13);
                    Thread.Sleep(2000);
                    driver.FindElement(By.Id("searchBox")).SendKeys(ParcelID);
                    gc.CreatePdf(orderNumber, ParcelID, "Tax input ", driver, "GA", "Newton");
                    IWebElement ISpan16 = driver.FindElement(By.XPath("//*[@id='searchForm']/div[1]/div/span/button/i"));
                    IJavaScriptExecutor js16 = driver as IJavaScriptExecutor;
                    js16.ExecuteScript("arguments[0].click();", ISpan16);
                    Thread.Sleep(4000);
                    gc.CreatePdf(orderNumber, ParcelID, "tax history details ", driver, "GA", "Newton");
                    //Tax History Details Table
                    //Owner Name~Tax Year~Bill Number~Parcel ID~Paid Date~Status
                    //  List<string> strTax = new List<string>();

                    string[] strTax = new string[8];
                    tbmulti = null;
                    try
                    {
                        tbmulti = driver.FindElement(By.XPath(" //*[@id='avalon']/div/div[3]/div[2]/table/tbody"));
                    }
                    catch { }
                    try
                    {
                        if (tbmulti == null)
                        {
                            tbmulti = driver.FindElement(By.XPath(" //*[@id='avalon']/div/div[4]/div[2]/table/tbody"));

                        }
                    }
                    catch { }


                    TRmulti = tbmulti.FindElements(By.TagName("tr"));

                    foreach (IWebElement row in TRmulti)
                    {

                        TDmulti = row.FindElements(By.TagName("td"));
                        if (TDmulti.Count != 0)
                        {
                            string tax_history = TDmulti[0].Text + "~" + TDmulti[1].Text + "~" + TDmulti[2].Text + "~" + TDmulti[3].Text + "~" + TDmulti[4].Text + "~" + TDmulti[5].Text;
                            gc.insert_date(orderNumber, ParcelID, 524, tax_history, 1, DateTime.Now);

                            if ((TDmulti[5].Text.Trim() == "Paid" && n < 3))
                            {

                                strTax[n] = TDmulti[5].Text;
                                q++;

                            }
                            if (TDmulti[5].Text.Trim() == "Unpaid")
                            {

                                strTax[n] = TDmulti[5].Text;
                                q++;
                            }

                        }
                        n++;
                    }
                    for (int a = q; a > 0; a--)
                    {
                        taxHistory(orderNumber, ParcelID);
                    }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "GA", "Newton", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "GA", "Newton");
                    return "Data Inserted Successfully";
                }

                catch (Exception ex)
                {
                    driver.Quit();
                    throw ex;
                }

            }
        }
        public void taxHistory(string orderNumber, string parcelID)
        {
            IWebElement ITaxClick = null;
            try
            {
                ITaxClick = driver.FindElement(By.XPath("//*[@id='avalon']/div/div[3]/div[2]/table/tbody/tr[" + m + "]/td[8]/button"));
            }
            catch { }
            try
            {
                if(ITaxClick == null)
                {
                    ITaxClick = driver.FindElement(By.XPath("//*[@id='avalon']/div/div[4]/div[2]/table/tbody/tr[" + m + "]/td[8]/button"));
                }
            }
            catch { }
            ITaxClick.Click();
            Thread.Sleep(4000);
            gc.CreatePdf(orderNumber, ParcelID, "tax details ", driver, "GA", "Newton");
            try
            {

                if (b == 1)
                {

                    //Owner Name~Tax Year~Bill Number~Account Number~Parcel ID~Tax Amount
                    IWebElement tbmulti74 = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/table/tbody"));
                    gc.CreatePdf(orderNumber, ParcelID, "tax deliquent details ", driver, "GA", "Newton");
                    IList<IWebElement> TRmulti74 = tbmulti74.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDmulti74;
                    foreach (IWebElement row5 in TRmulti74)
                    {
                        TDmulti74 = row5.FindElements(By.TagName("td"));

                        string tax_deli = TDmulti74[0].Text + "~" + TDmulti74[1].Text + "~" + TDmulti74[2].Text + "~" + TDmulti74[3].Text + "~" + TDmulti74[4].Text + "~" + TDmulti74[6].Text;
                        gc.insert_date(orderNumber, ParcelID, 526, tax_deli, 1, DateTime.Now);

                    }
                    string footer = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/table/tfoot")).Text.Replace("Total:", "");
                    string tax_deli1 = "" + "~" + "Total" + "~" + "" + "~" + "" + "~" + "" + "~" + footer;
                    gc.insert_date(orderNumber, ParcelID, 526, tax_deli1, 1, DateTime.Now);
                }
                b++;

                IWebElement ISpan8 = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[2]/button[2]/i"));
                IJavaScriptExecutor js8 = driver as IJavaScriptExecutor;
                js8.ExecuteScript("arguments[0].click();", ISpan8);
                Thread.Sleep(3000);
            }
            catch { }
            gc.CreatePdf(orderNumber, ParcelID, "tax details " + m, driver, "GA", "Newton");
            int a = 0;

            IWebElement tbmulti7 = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[1]/div[1]/table/tbody"));
            IList<IWebElement> TRmulti7 = tbmulti7.FindElements(By.TagName("tr"));
            IList<IWebElement> TDmulti7;
            foreach (IWebElement row3 in TRmulti7)
            {

                TDmulti7 = row3.FindElements(By.TagName("td"));
                if (TDmulti7.Count != 0)
                {
                    if (a == 0)
                        owner1 = TDmulti7[1].Text;
                    if (a == 1)
                        owner2 = TDmulti7[1].Text;
                    if (a == 2)
                        addnew = TDmulti7[1].Text;
                    if (a == 3)
                        propadd1 = TDmulti7[1].Text;
                    if (a == 5)
                        propadd2 = TDmulti7[1].Text;
                    if (a == 6)
                        account = TDmulti7[1].Text;
                    if (a == 7)
                        property_type = TDmulti7[1].Text;
                    if (a == 8)
                        bill_no = TDmulti7[1].Text;
                    if (a == 9)
                        tax_year = TDmulti7[1].Text;
                    a++;
                }
            }

            int a1 = 0;
            IWebElement tbmulti71 = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[1]/div[2]/table/tbody"));
            IList<IWebElement> TRmulti71 = tbmulti71.FindElements(By.TagName("tr"));
            IList<IWebElement> TDmulti71;
            foreach (IWebElement row2 in TRmulti71)
            {

                TDmulti71 = row2.FindElements(By.TagName("td"));
                if (TDmulti71.Count != 0)
                {
                    if (a1 == 0)
                        TaxAmount = TDmulti71[1].Text;
                    if (a1 == 1)
                        FairMarketValue = TDmulti71[1].Text;
                    if (a1 == 2)
                        AssessedValue = TDmulti71[1].Text;
                    if (a1 == 3)
                        UnderAppeal = TDmulti71[1].Text;
                    if (a1 == 4)
                        TotalDue = TDmulti71[1].Text;

                    a1++;
                }
            }

            int a11 = 0;
            IWebElement tbmulti711 = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[2]/div[2]/table/tbody"));
            IList<IWebElement> TRmulti711 = tbmulti711.FindElements(By.TagName("tr"));
            IList<IWebElement> TDmulti711;

            foreach (IWebElement row1 in TRmulti711)
            {

                TDmulti711 = row1.FindElements(By.TagName("td"));
                if (TDmulti711.Count != 0)
                {
                    if (a11 == 0)
                        DueDate = TDmulti711[1].Text;
                    if (a11 == 1)
                        PaymentStatus = TDmulti711[1].Text;
                    if (a11 == 2)
                        PaidDate = TDmulti711[1].Text;
                    if (a11 == 3)
                        PaidAmount = TDmulti711[1].Text;
                    if (a11 == 4)
                        TotalDuepay = TDmulti711[1].Text;

                    a11++;
                }
            }
            string tax_info = account + "~" + owner1 + owner2 + "~" + addnew + propadd1 + propadd2 + "~" + property_type + "~" + bill_no + "~" + tax_year + "~" + TaxAmount + "~" + FairMarketValue + "~" + AssessedValue + "~" + UnderAppeal + "~" + TotalDue + "~" + DueDate + "~" + PaymentStatus + "~" + PaidDate + "~" + PaidAmount + "~" + TotalDuepay;
            gc.insert_date(orderNumber, ParcelID, 525, tax_info, 1, DateTime.Now);



            IWebElement ISpan136 = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/ul/li[2]/a"));
            IJavaScriptExecutor js136 = driver as IJavaScriptExecutor;
            js136.ExecuteScript("arguments[0].click();", ISpan136);
            Thread.Sleep(4000);
            gc.CreatePdf(orderNumber, ParcelID, "TaxBillScreenshot " + m, driver, "GA", "Newton");
            //download taxbill
            try
            {
                IWebElement Itaxbill = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[2]/div/div[1]/a"));
                string URL1 = Itaxbill.GetAttribute("href");
                gc.downloadfile(URL1, orderNumber, ParcelID, "TaxBill" + m, "GA", "Newton");
            }
            catch { }
            try
            {
                driver.FindElement(By.XPath("//*[@id='avalon']/div/div/ul/li[3]/a")).Click();
                Thread.Sleep(4000);
                gc.CreatePdf(orderNumber, ParcelID, "TaxBillReceiptScreenshot " + m, driver, "GA", "Newton");
            }
            catch { }
            //download taxbill
            try
            {
                IWebElement Itaxbill1 = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[3]/div/div[1]/a"));
                string URL11 = Itaxbill1.GetAttribute("href");
                gc.downloadfile(URL11, orderNumber, ParcelID, "TaxBillReceipt" + m, "GA", "Newton");
            }
            catch { }

            driver.Navigate().Back();
            Thread.Sleep(2000);
            m++;

        }

    }
}
