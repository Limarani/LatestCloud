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
    public class WebDriver_ILWill
    {
        string outputPath = "", Outparcel = "", InserParcel = "";
        string Parcel_A = "", Parcel_B = "", Parcel_C = "", Parcel_D = "", Parcel_E = "", Parcel_F = "";
        string Address = "", City = "", Zip = "", Class = "", Multidata = "";
        string Year_Built = "", Residential = "", Legal_Description = "", Property_Details;
        string Land = "", Form_Land = "", Building = "", Form_Building = "", Total = "", Form_Total = "", Assement_Details = "";
        string TaxInfo_Details = "", FirstYear = "", SecondYear = "", TaxYear = "", TaxPreviousYear = "", OwnerName = "", Mailing_Address1 = "", Mailing_Address2 = "", Mailing_Address3 = "", Mailing_Address = "", Assessed_Value = "", Exemptions = "", Tax_Code = "", Tax_Rate = "", Taxing_Authority = "";
        string TaxDisTitle = "", Installment = "", Tax_Amount = "", Interest = "", Paid_Amount = "", Paid_Date = "", Balance_Due = "", Current_Year = "", Current_Yeartotal = "";
        string Authority = "", Rate_2016 = "", Amount_2016 = "", Rate_2017 = "", Amount_2017 = "", Tax_Distribution = "", Assessment_Year = "";
        string PTaxInfo_Details = "", PFirstYear = "", PSecondYear = "", PTaxYear = "", PTaxPreviousYear = "", POwnerName = "", PMailing_Address1 = "", PMailing_Address2 = "", PMailing_Address3 = "", PMailing_Address = "", PAssessed_Value = "", PExemptions = "", PTax_Code = "", PTax_Rate = "";
        string PTaxDisTitle = "", PInstallment = "", PTax_Amount = "", PInterest = "", PPaid_Amount = "", PPaid_Date = "", PBalance_Due = "", PCurrent_Year = "", PCurrent_Yeartotal = "";
        string PAuthority = "", PRate_2016 = "", PAmount_2016 = "", PRate_2017 = "", PAmount_2017 = "", PTax_Distribution = "", PAssessment_Year = "";
        string PreviousYearURL = "", CurrentYearURL = "";
        List<string> TAXURL = new List<string>();

        IWebElement CurrentTB;
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_ILWill(string houseno, string sname, string stype, string unitnumber, string parcelNumber, string searchType, string orderNumber, string ownername, string directParcel)
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
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    if (searchType == "titleflex")
                    {
                        if(directParcel=="")
                        { 
                        Address = houseno + " " + sname + " " + stype + " " + unitnumber;
                        }
                        else
                        {
                            Address = houseno + " " + directParcel+" "+ sname + " " + stype + " " + unitnumber;
                        }
                        gc.TitleFlexSearch(orderNumber, "", ownername, Address, "IL", "Will");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_ILWill"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("http://www.willcountysoa.com/search.aspx");
                        Thread.Sleep(2000);

                        driver.FindElement(By.Id("ctl00_BC_btnContinue")).Click();
                        Thread.Sleep(2000);

                        driver.FindElement(By.Id("ctl00_BC_hlAddress")).Click();
                        Thread.Sleep(2000);

                        driver.FindElement(By.Id("ctl00_BC_txStreetFrom")).SendKeys(houseno);
                        driver.FindElement(By.Id("ctl00_BC_txStreetName")).SendKeys(sname);

                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "IL", "Will");
                        driver.FindElement(By.Id("ctl00_BC_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        //MultiParcel
                        try
                        {
                            IWebElement MultiTable = driver.FindElement(By.XPath("//*[@id='ctl00_BC_gvParcels']/tbody"));
                            IList<IWebElement> MultiTR = MultiTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiTD;

                            int maxCheck = 0;
                            gc.CreatePdf_WOP(orderNumber, "MultiAddresssearch", driver, "IL", "Will");
                            foreach (IWebElement Multi in MultiTR)
                            {
                                if (maxCheck <= 25)
                                {
                                    MultiTD = Multi.FindElements(By.TagName("td"));
                                    if (MultiTD.Count > 2 && !Multi.Text.Contains("Displaying") && !Multi.Text.Contains("Pin"))
                                    {
                                        parcelNumber = MultiTD[0].Text;
                                        Address = MultiTD[1].Text;
                                        City = MultiTD[2].Text;
                                        Zip = MultiTD[3].Text;
                                        Class = MultiTD[4].Text;
                                        Multidata = Address + "~" + City + "~" + Zip + "~" + Class;
                                        gc.insert_date(orderNumber, parcelNumber, 394, Multidata, 1, DateTime.Now);
                                        maxCheck++;
                                    }
                                   
                                }

                            }
                            
                            if (MultiTR.Count > 30)
                            {
                                HttpContext.Current.Session["multiparcel_ILWill_count"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                                //return GlobalClass.multiparcel_ILWill = "Yes";
                            }
                            if (MultiTR.Count > 5 && MultiTR.Count < 30)
                            {
                                HttpContext.Current.Session["multiparcel_ILWill"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                        }
                        catch
                        { }
                    }

                    if (searchType == "parcel")
                    {
                        if ((HttpContext.Current.Session["titleparcel"] != null && HttpContext.Current.Session["titleparcel"].ToString() == "Yes"))
                        {
                            parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();

                        }
                        driver.Navigate().GoToUrl("http://www.willcountysoa.com/search.aspx");
                        Thread.Sleep(2000);

                        driver.FindElement(By.Id("ctl00_BC_btnContinue")).Click();
                        Thread.Sleep(2000);

                        driver.FindElement(By.Id("ctl00_BC_hlPIN")).Click();
                        Thread.Sleep(2000);

                        if (parcelNumber.Contains("-"))
                        {
                            parcelNumber = parcelNumber.Replace("-", "");
                        }

                        Parcel_A = parcelNumber.Substring(0, 2);
                        Parcel_B = parcelNumber.Substring(2, 2);
                        Parcel_C = parcelNumber.Substring(4, 2);
                        Parcel_D = parcelNumber.Substring(6, 3);
                        Parcel_E = parcelNumber.Substring(9, 3);
                        Parcel_F = parcelNumber.Substring(12, 4);

                        driver.FindElement(By.Id("ctl00_BC_txP1")).SendKeys(Parcel_A);
                        driver.FindElement(By.Id("ctl00_BC_txP2")).SendKeys(Parcel_B);
                        driver.FindElement(By.Id("ctl00_BC_txP3")).SendKeys(Parcel_C);
                        driver.FindElement(By.Id("ctl00_BC_txP4")).SendKeys(Parcel_D);
                        driver.FindElement(By.Id("ctl00_BC_txP5")).SendKeys(Parcel_E);
                        driver.FindElement(By.Id("ctl00_BC_txP6")).SendKeys(Parcel_F);

                        gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearch", driver, "IL", "Will");
                        driver.FindElement(By.Id("ctl00_BC_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.Id("ctl00_BC_gvParcels"));
                        if(INodata.Text.Contains("There are no records that match your criteria"))
                        {
                            HttpContext.Current.Session["Nodata_ILWill"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    //Property Details
                    InserParcel = driver.FindElement(By.XPath("/html/body/form/div[3]/table/tbody/tr/td[2]/table/tbody/tr[3]/td/table/tbody/tr/td[2]/table/tbody/tr[3]/td[1]/div[1]/div/table[2]/tbody/tr/td[1]/table/tbody/tr[1]/td")).Text.Replace("PIN #:", "");
                    if (InserParcel.Contains("-"))
                    {
                        Outparcel = InserParcel.Replace("-", "").Trim();
                    }
                    // Outparcel = WebDriverTest.After(Outparcel, "PIN #:").Trim();
                    Residential = driver.FindElement(By.XPath("/html/body/form/div[3]/table/tbody/tr/td[2]/table/tbody/tr[3]/td/table/tbody/tr/td[2]/table/tbody/tr[3]/td[1]/div[1]/div/table[2]/tbody/tr/td[1]/table/tbody/tr[2]/td")).Text;

                    try
                    {
                        IWebElement Yeartable = driver.FindElement(By.XPath("/html/body/form/div[3]/table/tbody/tr/td[2]/table/tbody/tr[3]/td/table/tbody/tr/td[2]/table/tbody/tr[3]/td[1]/div[1]/div/table[6]/tbody"));
                        IList<IWebElement> YearTR = Yeartable.FindElements(By.TagName("tr"));
                        IList<IWebElement> YearTD;

                        foreach (IWebElement Year in YearTR)
                        {
                            YearTD = Year.FindElements(By.TagName("td"));
                            if (YearTD.Count != 0 && Year.Text.Contains("Year Built:"))
                            {
                                Year_Built = YearTD[1].Text;
                            }
                        }

                        IWebElement LegalTB = driver.FindElement(By.XPath("/html/body/form/div[3]/table/tbody/tr/td[2]/table/tbody/tr[3]/td/table/tbody/tr/td[2]/table/tbody/tr[3]/td[1]/div[1]/div/table[9]/tbody"));
                        IList<IWebElement> LegalTR = LegalTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> LegalTD;

                        foreach (IWebElement Legal in LegalTR)
                        {
                            LegalTD = Legal.FindElements(By.TagName("td"));
                            if (LegalTD.Count != 0 && !Legal.Text.Contains("") || Legal.Text.Contains("IN"))
                            {
                                Legal_Description = LegalTD[0].Text;
                            }
                        }
                    }
                    catch
                    { }

                    gc.CreatePdf(orderNumber, Outparcel, "Proprty Details", driver, "IL", "Will");
                    Property_Details = Residential + "~" + Year_Built + "~" + Legal_Description;
                    gc.insert_date(orderNumber, InserParcel, 395, Property_Details, 1, DateTime.Now);

                    //Assement Details
                    try
                    {
                        Assessment_Year = driver.FindElement(By.XPath("//*[@id='Table2']/tbody/tr/td[1]/table/tbody/tr[10]/td")).Text;
                        Assessment_Year = WebDriverTest.After(Assessment_Year, "(").Replace(")", "").Trim();
                        Land = driver.FindElement(By.XPath("/html/body/form/div[3]/table/tbody/tr/td[2]/table/tbody/tr[3]/td/table/tbody/tr/td[2]/table/tbody/tr[3]/td[1]/div[1]/div/table[4]/tbody/tr[1]/td[2]")).Text;
                        Form_Land = driver.FindElement(By.XPath("/html/body/form/div[3]/table/tbody/tr/td[2]/table/tbody/tr[3]/td/table/tbody/tr/td[2]/table/tbody/tr[3]/td[1]/div[1]/div/table[4]/tbody/tr[1]/td[5]")).Text;
                        Building = driver.FindElement(By.XPath("/html/body/form/div[3]/table/tbody/tr/td[2]/table/tbody/tr[3]/td/table/tbody/tr/td[2]/table/tbody/tr[3]/td[1]/div[1]/div/table[4]/tbody/tr[2]/td[2]")).Text;
                        Form_Building = driver.FindElement(By.XPath("/html/body/form/div[3]/table/tbody/tr/td[2]/table/tbody/tr[3]/td/table/tbody/tr/td[2]/table/tbody/tr[3]/td[1]/div[1]/div/table[4]/tbody/tr[2]/td[5]")).Text;
                        Total = driver.FindElement(By.XPath("/html/body/form/div[3]/table/tbody/tr/td[2]/table/tbody/tr[3]/td/table/tbody/tr/td[2]/table/tbody/tr[3]/td[1]/div[1]/div/table[4]/tbody/tr[3]/td[2]")).Text;
                        Form_Total = driver.FindElement(By.XPath("/html/body/form/div[3]/table/tbody/tr/td[2]/table/tbody/tr[3]/td/table/tbody/tr/td[2]/table/tbody/tr[3]/td[1]/div[1]/div/table[4]/tbody/tr[3]/td[5]")).Text;

                        Assement_Details = Assessment_Year + "~" + Land + "~" + Form_Land + "~" + Building + "~" + Form_Building + "~" + Total + "~" + Form_Total;
                        gc.insert_date(orderNumber, InserParcel, 396, Assement_Details, 1, DateTime.Now);
                    }
                    catch
                    { }
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    //Taxing Authority
                    try
                    {
                        driver.Navigate().GoToUrl("http://www.willcountysoa.com/contact.aspx");
                        Thread.Sleep(2000);

                        gc.CreatePdf(orderNumber, InserParcel, "Authority Details" + TaxYear, driver, "IL", "Will");
                        Taxing_Authority = driver.FindElement(By.XPath("/html/body/form/div[3]/table/tbody/tr/td[2]/table/tbody/tr[3]/td/table/tbody/tr/td[2]/table/tbody/tr[3]/td[1]/table/tbody/tr[5]/td[2]")).Text.Replace("\r\n", "");
                    }
                    catch { }

                    //Tax Information
                    driver.Navigate().GoToUrl("http://willtax.willcountydata.com/ccalm07.asp");
                    Thread.Sleep(2000);
                    //*[@id="hero"]/div[1]/div/div[1]/div/p/a[1]
                    //IWebElement ITaxPreviousYear = driver.FindElement(By.XPath("//*[@id='hero']/div[1]/div/div[1]/div/p"));
                    //IList<IWebElement> ITaxPreviousYearRow = ITaxPreviousYear.FindElements(By.TagName("a"));
                    //foreach (IWebElement PreviousYear in ITaxPreviousYearRow)
                    //{
                    //    if (PreviousYear.Text != "" && PreviousYear.Text.Contains("VIEW") && !PreviousYear.Text.Contains("PAY") && PreviousYear.Text.Contains("TAXES"))
                    //    {
                    //        PreviousYearURL = PreviousYear.GetAttribute("href");
                    //        TAXURL.Add(PreviousYearURL);
                    //    }
                    //}

                    //try
                    //{
                    //    IWebElement ITaxYear = driver.FindElement(By.XPath("//*[@id='hero']/div[1]/div/div[1]/div/p"));
                    //    IList<IWebElement> ITaxYearRow = ITaxYear.FindElements(By.TagName("a"));
                    //    foreach (IWebElement Year in ITaxYearRow)
                    //    {
                    //        if (Year.Text != "" && Year.Text.Contains("VIEW") && Year.Text.Contains("PAY") && Year.Text.Contains("TAXES"))
                    //        {
                    //            Year.Click();
                    //            IWebElement IYearSearch = driver.FindElement(By.LinkText("YES"));
                    //            if (IYearSearch.Text !="" && IYearSearch.Text.Contains("YES"))
                    //            {
                    //                CurrentYearURL = IYearSearch.GetAttribute("href");
                    //            }
                    //        }
                    //    }
                    //}
                    //catch { }
                    int i = 0;


                    driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/table/tbody/tr/td/form/p[1]/input")).SendKeys(Outparcel);
                    driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/table/tbody/tr/td/form/p[2]/input[1]")).SendKeys(Keys.Enter);
                    Thread.Sleep(2000);
                    PTaxYear = GlobalClass.Before(driver.FindElement(By.XPath("/html/body/div/center/table/tbody/tr/td/center[1]/table/tbody/tr[2]/td")).Text, " Levy Real Estate");
                    gc.CreatePdf(orderNumber, InserParcel, "Tax Search" + PTaxYear, driver, "IL", "Will");
                    POwnerName = driver.FindElement(By.XPath("/html/body/div/center/table/tbody/tr/td/table/tbody/tr[2]/td[2]")).Text;
                    PMailing_Address1 = driver.FindElement(By.XPath("/html/body/div/center/table/tbody/tr/td/table/tbody/tr[3]/td[2]")).Text;
                    PMailing_Address2 = driver.FindElement(By.XPath("/html/body/div/center/table/tbody/tr/td/table/tbody/tr[4]/td[2]")).Text;
                    PMailing_Address3 = driver.FindElement(By.XPath("/html/body/div/center/table/tbody/tr/td/table/tbody/tr[5]/td[2]")).Text;

                    PMailing_Address = PMailing_Address1 + " " + PMailing_Address2 + " " + PMailing_Address3;

                    PAssessed_Value = driver.FindElement(By.XPath("/html/body/div/center/table/tbody/tr/td/table/tbody/tr[7]/td[3]")).Text;
                    PExemptions = driver.FindElement(By.XPath("/html/body/div/center/table/tbody/tr/td/table/tbody/tr[7]/td[4]")).Text;
                    PTax_Code = driver.FindElement(By.XPath("/html/body/div/center/table/tbody/tr/td/table/tbody/tr[9]/td[4]")).Text;
                    PTax_Rate = driver.FindElement(By.XPath("/html/body/div/center/table/tbody/tr/td/table/tbody/tr[9]/td[5]")).Text;
                    gc.CreatePdf(orderNumber, InserParcel, "Tax Details" + PTaxYear, driver, "IL", "Will");


                    //Current TaxYear
                    try
                    {
                        IWebElement CurrentTB = driver.FindElement(By.XPath("/html/body/div/center/table/tbody/tr/td/div[1]/p/table/tbody"));
                        IList<IWebElement> CurrentTR = CurrentTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> CurrentTD;

                        foreach (IWebElement Current in CurrentTR)
                        {
                            CurrentTD = Current.FindElements(By.TagName("td"));
                            if (CurrentTD.Count != 0 && !Current.Text.Contains("Installment") && !Current.Text.Contains("Total Base Tax"))
                            {
                                PInstallment = CurrentTD[0].Text.Replace("----", " ").Trim();
                                PTax_Amount = CurrentTD[1].Text.Replace("\r\n", "----").Trim();
                                PInterest = CurrentTD[2].Text.Replace("\r\n", "----").Trim();
                                PPaid_Amount = CurrentTD[3].Text.Replace("\r\n", "----").Trim();
                                PPaid_Date = CurrentTD[4].Text.Replace("\r\n", " ").Trim();
                                PBalance_Due = CurrentTD[5].Text.Replace("\r\n", "----").Trim();

                                PCurrent_Year = PTaxYear + "~" + PInstallment + "~" + PTax_Amount + "~" + PInterest + "~" + PPaid_Amount + "~" + PPaid_Date + "~" + PBalance_Due;
                                gc.insert_date(orderNumber, InserParcel, 402, PCurrent_Year, 1, DateTime.Now);
                            }
                            else if (Current.Text.Contains("Total Base Tax"))
                            {
                                PInstallment = CurrentTD[0].Text.Replace("\r\n", " ").Trim();
                                PTax_Amount = CurrentTD[1].Text.Replace("\r\n", "----").Trim();

                                PCurrent_Yeartotal = PTaxYear + "~" + PInstallment + "~" + PTax_Amount + "~" + " " + "~" + " " + "~" + " " + "~" + " ";
                                gc.insert_date(orderNumber, InserParcel, 402, PCurrent_Yeartotal, 1, DateTime.Now);
                            }
                        }
                    }
                    catch
                    { }

                    try
                    {
                        IWebElement CurrentTB = driver.FindElement(By.XPath("/html/body/div/center/table/tbody/tr/td/div[1]/p[2]/table/tbody"));
                        IList<IWebElement> CurrentTR = CurrentTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> CurrentTD;

                        foreach (IWebElement Current in CurrentTR)
                        {
                            CurrentTD = Current.FindElements(By.TagName("td"));
                            if (CurrentTD.Count != 0 && !Current.Text.Contains("Installment") && !Current.Text.Contains("Total Base Tax"))
                            {
                                PInstallment = CurrentTD[0].Text.Replace("----", " ").Trim();
                                PTax_Amount = CurrentTD[1].Text.Replace("\r\n", "----").Trim();
                                PInterest = CurrentTD[2].Text.Replace("\r\n", "----").Trim();
                                PPaid_Amount = CurrentTD[3].Text.Replace("\r\n", "----").Trim();
                                PPaid_Date = CurrentTD[4].Text.Replace("\r\n", " ").Trim();
                                PBalance_Due = CurrentTD[5].Text.Replace("\r\n", "----").Trim();

                                PCurrent_Year = PTaxYear + "~" + PInstallment + "~" + PTax_Amount + "~" + PInterest + "~" + PPaid_Amount + "~" + PPaid_Date + "~" + PBalance_Due;
                                gc.insert_date(orderNumber, InserParcel, 402, PCurrent_Year, 1, DateTime.Now);
                            }
                            else if (Current.Text.Contains("Total Base Tax"))
                            {
                                PInstallment = CurrentTD[0].Text.Replace("\r\n", " ").Trim();
                                PTax_Amount = CurrentTD[1].Text.Replace("\r\n", "----").Trim();

                                PCurrent_Yeartotal = PTaxYear + "~" + PInstallment + "~" + PTax_Amount + "~" + " " + "~" + " " + "~" + " " + "~" + " ";
                                gc.insert_date(orderNumber, InserParcel, 402, PCurrent_Yeartotal, 1, DateTime.Now);
                            }
                        }
                    }
                    catch
                    { }

                    //Tax Inqiury
                    try
                    {
                        String Parent_Window = driver.CurrentWindowHandle;
                        IWebElement Tax_Enquiry = driver.FindElement(By.LinkText("Five Year Tax Inquiry"));
                        Tax_Enquiry.SendKeys(Keys.Enter);
                        Thread.Sleep(4000);
                        driver.SwitchTo().Window(driver.WindowHandles.Last());
                        gc.CreatePdf(orderNumber, InserParcel, "Tax Inquiry Details" + PTaxYear, driver, "IL", "Will");
                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        driver.SwitchTo().Window(Parent_Window);
                        //driver.Navigate().Back();
                    }
                    catch
                    { }

                    if (i == 0)
                    {
                        //Tax Distribution
                        try
                        {
                            String Parent_Window5 = driver.CurrentWindowHandle;
                            IWebElement Distribution = driver.FindElement(By.LinkText("Tax Detail Inquiry"));
                            Distribution.SendKeys(Keys.Enter);
                            Thread.Sleep(2000);
                            driver.SwitchTo().Window(driver.WindowHandles.Last());
                            gc.CreatePdf(orderNumber, InserParcel, "Tax Distribution" + PTaxYear, driver, "IL", "Will");

                            IWebElement HeadingDistributionTB = driver.FindElement(By.XPath("/html/body/div/center/table/tbody/tr/td/p[5]/table/tbody/tr/td/table/tbody"));
                            IList<IWebElement> HeadingDistributionTR = HeadingDistributionTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> HeadingDistributionTH;
                            foreach (IWebElement Heading in HeadingDistributionTR)
                            {
                                HeadingDistributionTH = Heading.FindElements(By.TagName("th"));
                                if (HeadingDistributionTH.Count != 0 && !Heading.Text.Contains("Taxing Body") && Heading.Text.Trim() != "")
                                {
                                    PFirstYear = HeadingDistributionTH[0].Text;
                                    PSecondYear = HeadingDistributionTH[1].Text;
                                }
                                if (HeadingDistributionTH.Count != 0 && Heading.Text.Contains("Taxing Body") && Heading.Text.Trim() != "")
                                {
                                    PTaxDisTitle = HeadingDistributionTH[0].Text + "~" + "TaxYear" + "~" + HeadingDistributionTH[1].Text + " " + PFirstYear + "~" + HeadingDistributionTH[2].Text + " " + PFirstYear + "~" + HeadingDistributionTH[3].Text + " " + PSecondYear + "~" + HeadingDistributionTH[4].Text + " " + PSecondYear;
                                }
                            }
                            db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + PTaxDisTitle + "' where Id = '" + 408 + "'");

                            IWebElement DistributionTB = driver.FindElement(By.XPath("/html/body/div/center/table/tbody/tr/td/p[5]/table/tbody/tr/td/table/tbody"));
                            IList<IWebElement> DistributionTR = DistributionTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> DistributionTD;
                            foreach (IWebElement Distributions in DistributionTR)
                            {
                                DistributionTD = Distributions.FindElements(By.TagName("td"));
                                if (DistributionTD.Count != 0 && !Distributions.Text.Contains("Taxing Body") && !Distributions.Text.Contains(PFirstYear) && Distributions.Text.Trim() != "")
                                {
                                    PAuthority = DistributionTD[0].Text.Replace("\r\n", " ").Trim();
                                    PRate_2016 = DistributionTD[1].Text.Replace("\r\n", " ").Trim();
                                    PAmount_2016 = DistributionTD[2].Text.Replace("\r\n", " ").Trim();
                                    PRate_2017 = DistributionTD[3].Text.Replace("\r\n", " ").Trim();
                                    PAmount_2017 = DistributionTD[4].Text.Replace("\r\n", " ").Trim();

                                    PTax_Distribution = PAuthority + "~" + PTaxYear + "~" + PRate_2016 + "~" + PAmount_2016 + "~" + PRate_2017 + "~" + PAmount_2017;
                                    gc.insert_date(orderNumber, InserParcel, 408, PTax_Distribution, 1, DateTime.Now);
                                }
                            }
                            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                            driver.SwitchTo().Window(Parent_Window5);
                        }
                        catch
                        { }
                    }

                    PTaxInfo_Details = PTaxYear + "~" + POwnerName + "~" + PMailing_Address + "~" + PAssessed_Value + "~" + PExemptions + "~" + PTax_Code + "~" + PTax_Rate + "~" + Taxing_Authority;
                    gc.insert_date(orderNumber, InserParcel, 399, PTaxInfo_Details, 1, DateTime.Now);
                    i++;


                    //if (CurrentYearURL != "")
                    //{
                    //    try
                    //    {
                    //        try
                    //        {
                    //            driver.Navigate().GoToUrl(CurrentYearURL);
                    //            Thread.Sleep(2000);

                    //            driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/table/tbody/tr/td/form/p[1]/input")).SendKeys(Outparcel);
                    //            driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/table/tbody/tr/td/form/p[2]/input[1]")).SendKeys(Keys.Enter);
                    //            Thread.Sleep(2000);
                    //            TaxYear = GlobalClass.Before(driver.FindElement(By.XPath("/html/body/div/center/table/tbody/tr/td/center[1]/table/tbody/tr[2]/td")).Text, " Levy Real Estate");
                    //            gc.CreatePdf(orderNumber, InserParcel, "Tax Search" + TaxYear, driver, "IL", "Will");
                    //            OwnerName = driver.FindElement(By.XPath("/html/body/div/center/table/tbody/tr/td/table/tbody/tr[2]/td[2]")).Text;
                    //            Mailing_Address1 = driver.FindElement(By.XPath("/html/body/div/center/table/tbody/tr/td/table/tbody/tr[3]/td[2]")).Text;
                    //            Mailing_Address2 = driver.FindElement(By.XPath("/html/body/div/center/table/tbody/tr/td/table/tbody/tr[4]/td[2]")).Text;
                    //            Mailing_Address3 = driver.FindElement(By.XPath("/html/body/div/center/table/tbody/tr/td/table/tbody/tr[5]/td[2]")).Text;

                    //            Mailing_Address = Mailing_Address1 + " " + Mailing_Address2 + " " + Mailing_Address3;

                    //            Assessed_Value = driver.FindElement(By.XPath("/html/body/div/center/table/tbody/tr/td/table/tbody/tr[7]/td[3]")).Text;
                    //            Exemptions = driver.FindElement(By.XPath("/html/body/div/center/table/tbody/tr/td/table/tbody/tr[7]/td[4]")).Text;
                    //            Tax_Code = driver.FindElement(By.XPath("/html/body/div/center/table/tbody/tr/td/table/tbody/tr[9]/td[4]")).Text;
                    //            Tax_Rate = driver.FindElement(By.XPath("/html/body/div/center/table/tbody/tr/td/table/tbody/tr[9]/td[5]")).Text;
                    //            gc.CreatePdf(orderNumber, InserParcel, "Tax Details" + TaxYear, driver, "IL", "Will");
                    //        }
                    //        catch
                    //        { }

                    //        //Current TaxYear
                    //        try
                    //        {
                    //            try
                    //            {
                    //                CurrentTB = driver.FindElement(By.XPath("/html/body/div/center/table/tbody/tr/td/div[1]/p/table/tbody"));
                    //            }
                    //            catch { }
                    //            try
                    //            {
                    //                if (CurrentTB.Text != "")
                    //                {
                    //                    CurrentTB = driver.FindElement(By.XPath("/html/body/div/center/table/tbody/tr/td/div[1]/p[2]/table/tbody"));
                    //                }
                    //            }
                    //            catch { }
                    //            IList<IWebElement> CurrentTR = CurrentTB.FindElements(By.TagName("tr"));
                    //            IList<IWebElement> CurrentTD;

                    //            foreach (IWebElement Current in CurrentTR)
                    //            {
                    //                CurrentTD = Current.FindElements(By.TagName("td"));
                    //                if (CurrentTD.Count != 0 && !Current.Text.Contains("Installment") && !Current.Text.Contains("Total Base Tax"))
                    //                {
                    //                    Installment = CurrentTD[0].Text.Replace("----", " ").Trim();
                    //                    Tax_Amount = CurrentTD[1].Text.Replace("\r\n", "----").Trim();
                    //                    Interest = CurrentTD[2].Text.Replace("\r\n", "----").Trim();
                    //                    Paid_Amount = CurrentTD[3].Text.Replace("\r\n", "----").Trim();
                    //                    Paid_Date = CurrentTD[4].Text.Replace("\r\n", " ").Trim();
                    //                    Balance_Due = CurrentTD[5].Text.Replace("\r\n", "----").Trim();

                    //                    Current_Year = TaxYear + "~" + Installment + "~" + Tax_Amount + "~" + Interest + "~" + Paid_Amount + "~" + Paid_Date + "~" + Balance_Due;
                    //                    gc.insert_date(orderNumber, InserParcel, 402, Current_Year, 1, DateTime.Now);
                    //                }
                    //                else if (Current.Text.Contains("Total Base Tax"))
                    //                {
                    //                    Installment = CurrentTD[0].Text.Replace("\r\n", " ").Trim();
                    //                    Tax_Amount = CurrentTD[1].Text.Replace("\r\n", "----").Trim();

                    //                    Current_Yeartotal = TaxYear + "~" + Installment + "~" + Tax_Amount + "~" + " " + "~" + " " + "~" + " " + "~" + " ";
                    //                    gc.insert_date(orderNumber, InserParcel, 402, Current_Yeartotal, 1, DateTime.Now);
                    //                }
                    //            }
                    //        }
                    //        catch
                    //        { }

                    //        //Tax Inqiury
                    //        try
                    //        {
                    //            String Parent_Window = driver.CurrentWindowHandle;
                    //            IWebElement Tax_Enquiry = driver.FindElement(By.LinkText("Five Year Tax Inquiry"));
                    //            string strTax_Enquiry = Tax_Enquiry.GetAttribute("href");
                    //            Tax_Enquiry.SendKeys(Keys.Control + "t");
                    //            Thread.Sleep(4000);
                    //            gc.CreatePdf(orderNumber, InserParcel, "Tax Inquiry Details" + TaxYear, driver, "IL", "Will");
                    //            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    //            driver.SwitchTo().Window(Parent_Window);
                    //        }
                    //        catch
                    //        { }

                    //        //Tax Distribution
                    //        try
                    //        {
                    //            String Parent_Window5 = driver.CurrentWindowHandle;
                    //            IWebElement Distribution = driver.FindElement(By.LinkText("Tax Detail Inquiry"));
                    //            Thread.Sleep(2000);
                    //            Distribution.SendKeys(Keys.Control + "t");

                    //            driver.SwitchTo().Window(driver.WindowHandles.Last());
                    //            gc.CreatePdf(orderNumber, InserParcel, "Tax Distribution" + TaxYear, driver, "IL", "Will");

                    //            IWebElement HeadingDistributionTB = driver.FindElement(By.XPath("/html/body/div/center/table/tbody/tr/td/p[5]/table/tbody/tr/td/table/tbody"));
                    //            IList<IWebElement> HeadingDistributionTR = HeadingDistributionTB.FindElements(By.TagName("tr"));
                    //            IList<IWebElement> HeadingDistributionTH;
                    //            foreach (IWebElement Heading in HeadingDistributionTR)
                    //            {
                    //                HeadingDistributionTH = Heading.FindElements(By.TagName("th"));
                    //                if (HeadingDistributionTH.Count != 0 && !Heading.Text.Contains("Taxing Body") && Heading.Text.Trim() != "")
                    //                {
                    //                    FirstYear = HeadingDistributionTH[0].Text;
                    //                    SecondYear = HeadingDistributionTH[1].Text;
                    //                }
                    //                if (HeadingDistributionTH.Count != 0 && Heading.Text.Contains("Taxing Body") && Heading.Text.Trim() != "")
                    //                {
                    //                    TaxDisTitle = HeadingDistributionTH[0].Text + "~" + "TaxYear" + "~" + HeadingDistributionTH[1].Text + " " + FirstYear + "~" + HeadingDistributionTH[2].Text + " " + FirstYear + "~" + HeadingDistributionTH[3].Text + " " + SecondYear + "~" + HeadingDistributionTH[4].Text + " " + SecondYear;
                    //                }
                    //            }
                    //            db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + TaxDisTitle + "' where Id = '" + 408 + "'");

                    //            IWebElement DistributionTB = driver.FindElement(By.XPath("/html/body/div/center/table/tbody/tr/td/p[5]/table/tbody/tr/td/table/tbody"));
                    //            IList<IWebElement> DistributionTR = DistributionTB.FindElements(By.TagName("tr"));
                    //            IList<IWebElement> DistributionTD;
                    //            foreach (IWebElement Distributions in DistributionTR)
                    //            {
                    //                DistributionTD = Distributions.FindElements(By.TagName("td"));
                    //                if (DistributionTD.Count != 0 && !Distributions.Text.Contains("Taxing Body") && !Distributions.Text.Contains(FirstYear) && Distributions.Text.Trim() != "")
                    //                {
                    //                    Authority = DistributionTD[0].Text.Replace("\r\n", " ").Trim();
                    //                    Rate_2016 = DistributionTD[1].Text.Replace("\r\n", " ").Trim();
                    //                    Amount_2016 = DistributionTD[2].Text.Replace("\r\n", " ").Trim();
                    //                    Rate_2017 = DistributionTD[3].Text.Replace("\r\n", " ").Trim();
                    //                    Amount_2017 = DistributionTD[4].Text.Replace("\r\n", " ").Trim();

                    //                    Tax_Distribution = Authority + "~" + TaxYear + "~" + Rate_2016 + "~" + Amount_2016 + "~" + Rate_2017 + "~" + Amount_2017;
                    //                    gc.insert_date(orderNumber, InserParcel, 408, Tax_Distribution, 1, DateTime.Now);
                    //                }
                    //            }
                    //            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    //            driver.SwitchTo().Window(Parent_Window5);
                    //        }
                    //        catch
                    //        { }
                    //        try
                    //        {
                    //            driver.Navigate().GoToUrl("http://www.willcountysoa.com/contact.aspx");
                    //            Thread.Sleep(2000);

                    //            gc.CreatePdf(orderNumber, InserParcel, "Authority Details" + TaxYear, driver, "IL", "Will");
                    //            Taxing_Authority = driver.FindElement(By.XPath("/html/body/form/div[3]/table/tbody/tr/td[2]/table/tbody/tr[3]/td/table/tbody/tr/td[2]/table/tbody/tr[3]/td[1]/table/tbody/tr[5]/td[2]")).Text.Replace("\r\n", "");
                    //        }
                    //        catch { }

                    //        TaxInfo_Details = TaxYear + "~" + OwnerName + "~" + Mailing_Address + "~" + Assessed_Value + "~" + Exemptions + "~" + Tax_Code + "~" + Tax_Rate + "~" + Taxing_Authority;
                    //        gc.insert_date(orderNumber, InserParcel, 399, TaxInfo_Details, 1, DateTime.Now);
                    //    }
                    //    catch { }
                    //}


                    //Tax Redemption
                    try
                    {
                        driver.Navigate().GoToUrl("http://willtax.willcountydata.com/ccwtx20.asp");
                        Thread.Sleep(2000);

                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/form/p[1]/input")).SendKeys(Outparcel);
                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/form/p[2]/input[1]")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, InserParcel, "Tax Redemption" + TaxYear, driver, "IL", "Will");

                        List<string> Bill = new List<string>();
                        IWebElement MultiOwnerTable = driver.FindElement(By.XPath("/html/body/div/center/table/tbody/tr/td/p[5]/table/tbody"));
                        IList<IWebElement> MultiOwnerRowurl = MultiOwnerTable.FindElements(By.TagName("td"));
                        IList<IWebElement> Multibill;
                        foreach (IWebElement row1 in MultiOwnerRowurl)
                        {
                            Multibill = row1.FindElements(By.TagName("a"));
                            if (Multibill.Count != 0 && row1.Text != "")
                            {
                                try
                                {
                                    string MultiOwnerTD = Multibill[0].GetAttribute("href");
                                    Bill.Add(MultiOwnerTD);
                                }
                                catch
                                { }
                            }
                        }
                        try
                        {
                            foreach (string url in Bill)
                            {
                                driver.Navigate().GoToUrl(url);
                                string Tayear = driver.FindElement(By.XPath("/html/body/div/center/table/tbody/tr/td/table[1]/tbody/tr[2]/td[3]")).Text;
                                gc.CreatePdf(orderNumber, InserParcel, "Tax Redemption" + Tayear, driver, "IL", "Will");
                            }
                        }
                        catch { }
                    }
                    catch { }


                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "IL", "Will", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "IL", "Will");
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