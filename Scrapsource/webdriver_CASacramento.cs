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
using OpenQA.Selenium.Edge;

namespace ScrapMaricopa.Scrapsource
{
    public class webdriver_CASacramento
    {

        string outputPath = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();

        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_Santamenta(string Address, string account, string parcelNumber, string ownername, string searchType, string orderno, string directParcel)
        {

            GlobalClass.global_orderNo = orderno;
            HttpContext.Current.Session["orderNo"] = orderno;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            //var driverService = PhantomJSDriverService.CreateDefaultService();
            //driverService.HideCommandPromptWindow = true;
            //driver = new ChromeDriver();
            //driver = new PhantomJSDriver();
            //driver.Manage().Window.Size = new Size(1920, 1080);
            var option = new ChromeOptions();
            option.AddArgument("No-Sandbox");
            using (driver = new ChromeDriver(option)) //ChromeDriver
            {
                if (ownername.Trim() != "" || Address.Trim() != "")
                {
                    searchType = "titleflex";

                }
                string[] stringSeparators1 = new string[] { "\r\n" };
                List<string> listurl = new List<string>();
                List<string> Columnurl = new List<string>();
                List<string> Roll = new List<string>();
                string Date = "";
                Date = DateTime.Now.ToString("M/d/yyyy");
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderno, parcelNumber, ownername, Address, "CA", "Sacramento");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_CASacramento"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();

                        searchType = "parcel";
                    }
                    //if (searchType == "address")
                    //{
                    //    driver.Navigate().GoToUrl("http://assessorparcelviewer.saccounty.net/jsviewer/assessor.html?apn={0}");
                    //    Thread.Sleep(2000);
                    //    driver.FindElement(By.XPath("//*[@id='omniInput']")).SendKeys(Address);

                    //    Thread.Sleep(2000);
                    //    //gc.CreatePdf_WOP(orderNumber, "Address search", driver, "FL", "Miami Dade");

                    //    driver.FindElement(By.XPath("//*[@id='goOmniSearch']")).SendKeys(Keys.Enter);
                    //    Thread.Sleep(3000);
                    //   // gc.CreatePdf_WOP(orderNumber, "Address search Result", driver, "FL", "Miami Dade");
                    //    Thread.Sleep(1000);

                    //    try
                    //    {
                    //        string multi = driver.FindElement(By.XPath("//*[@id='SearchDescription']/span")).Text;
                    //        if (multi.Trim()!="")
                    //        {
                    //            IWebElement MuliparcTB = driver.FindElement(By.XPath("//*[@id='datatable']"));
                    //            IList<IWebElement> MuliparcTR = MuliparcTB.FindElements(By.TagName("tr"));


                    //        }
                    //    }
                    //    catch { }


                    //}
                    if (searchType == "parcel")
                    {

                        driver.Navigate().GoToUrl("http://assessorparcelviewer.saccounty.net/jsviewer/assessor.html?apn={0}");
                        Thread.Sleep(5000);
                        try
                        {

                            driver.FindElement(By.XPath("//*[@id='PopupDisclaimerDiv']/div/div/div[3]/button[2]")).Click();
                        }
                        catch { }
                        driver.FindElement(By.XPath("//*[@id='omniInput']")).SendKeys(parcelNumber);


                        gc.CreatePdf(orderno, parcelNumber.Replace("-", ""), "Parcel Search", driver, "CA", "Sacramento");
                        Thread.Sleep(2000);


                        driver.FindElement(By.XPath("//*[@id='goOmniSearch']")).Click();
                        Thread.Sleep(6000);

                        // gc.CreatePdf(orderno, parcelNumber, "Parcel Search Result", driver, "CA", "Sacramento");


                    }


                    //AMROCK Mapping
                    Amrock amc = new Amrock();

                    // assessment details
                    string PropertyAddress = "", City_Zip = "", Jurisdiction = "", CountySupervisorDistrict = "", TaxRateAreaCode = "", ApproxParcelArea = "", Yearbuilt = "";
                    parcelNumber = parcelNumber.Replace("-", "");
                    try
                    {
                        gc.CreatePdf(orderno, parcelNumber, "Property", driver, "CA", "Sacramento");
                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='AssessorRollPortlet']/h3/a")));
                    }
                    catch { }
                    try
                    {
                        gc.CreatePdf(orderno, parcelNumber, "Assessment1", driver, "CA", "Sacramento");
                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='AssessorPortlet']/h3/a")));
                    }
                    catch { }


                    gc.CreatePdf(orderno, parcelNumber, "Assessment2", driver, "CA", "Sacramento");
                    //  gc.CreatePdf(orderno, parcelNumber, "Assessment Details", driver, "CA", "El Dorado");
                    parcelNumber = driver.FindElement(By.XPath("//*[@id='ParcelAPNNoDashes']")).Text;
                    PropertyAddress = driver.FindElement(By.XPath("//*[@id='Address']")).Text;
                    City_Zip = driver.FindElement(By.XPath("//*[@id='PropertyInformationTable']/tbody/tr[3]/td[2]")).Text;
                    Jurisdiction = driver.FindElement(By.XPath("//*[@id='Jurisdiction']")).Text;
                    TaxRateAreaCode = driver.FindElement(By.XPath("//*[@id='TaxRateAreaCodeLink']")).Text;
                    CountySupervisorDistrict = driver.FindElement(By.XPath("//*[@id='SupervisorDistrictLink']")).Text;
                    try
                    {
                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='EffectiveYearBuiltRow']/td[1]")));
                    }
                    catch { }
                    Thread.Sleep(1000);
                    Yearbuilt = driver.FindElement(By.Id("YearBuilt")).Text;

                    gc.CreatePdf(orderno, parcelNumber, "Year built", driver, "CA", "Sacramento");
                    driver.FindElement(By.XPath("//*[@id='AssessorPortlet']/h3")).Click();
                    //gc.CreatePdf(orderno, parcelNumber, "Input Passed Tax Search", driver, "CA", "El Dorado");
                    Thread.Sleep(1000);

                    ApproxParcelArea = driver.FindElement(By.XPath("//*[@id='LotSize']")).Text;

                    string ProperTyDetail = PropertyAddress + "~" + City_Zip + "~" + Jurisdiction + "~" + TaxRateAreaCode + "~" + CountySupervisorDistrict + "~" + ApproxParcelArea + "~" + Yearbuilt;

                    gc.insert_date(orderno, parcelNumber, 368, ProperTyDetail, 1, DateTime.Now);

                    string TaxRollYear = "", LandValue = "", ImprovementValue = "", PersonalPropertyValue = "", Fixtures = "", HomeownerExemption = "", OtherExemption = "", NetAssessedValue = "";

                    //gc.CreatePdf(orderno, parcelNumber, "Assessment", driver, "CA", "Sacramento");


                    //ByVisibleElement(driver.FindElement(By.XPath("//*[@id='AssessorRollPortlet']/h3/a")));

                    //gc.CreatePdf(orderno, parcelNumber, "Assessment1", driver, "CA", "Sacramento");

                    TaxRollYear = driver.FindElement(By.XPath("//*[@id='RollYear']")).Text;
                    LandValue = driver.FindElement(By.XPath("//*[@id='ValueLand']")).Text;
                    ImprovementValue = driver.FindElement(By.XPath("//*[@id='ValueStructure']")).Text;
                    PersonalPropertyValue = driver.FindElement(By.XPath("//*[@id='ValuePersonalProperty']")).Text;
                    Fixtures = driver.FindElement(By.XPath("//*[@id='ValueFixtures']")).Text;
                    HomeownerExemption = driver.FindElement(By.XPath("//*[@id='ValueHomeOwnersException']")).Text;
                    OtherExemption = driver.FindElement(By.XPath("//*[@id='ValueOtherExemption']")).Text;
                    NetAssessedValue = driver.FindElement(By.XPath("//*[@id='ValueTotal']")).Text;


                    string Assessment = TaxRollYear + "~" + LandValue + "~" + ImprovementValue + "~" + PersonalPropertyValue + "~" + Fixtures + "~" + HomeownerExemption + "~" + OtherExemption + "~" + NetAssessedValue;
                    gc.insert_date(orderno, parcelNumber, 369, Assessment, 1, DateTime.Now);

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("https://eproptax.saccounty.net/");
                    Thread.Sleep(5000);
                    try
                    {
                        IWebElement IAddressSearch1 = driver.FindElement(By.XPath("//*[@id='parcelLookup']/div[1]/a"));
                        IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                        js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                        Thread.Sleep(3000);
                       // driver.FindElement(By.XPath("//*[@id='parcelLookup']/div[1]/a")).Click();
                    }
                    catch { }
                    string Pa1 = "", Pa2 = "", Pa3 = "", Pa4 = "";

                    Pa1 = parcelNumber.Substring(0, 3);
                    Pa2 = parcelNumber.Substring(3, 4);
                    Pa3 = parcelNumber.Substring(7, 3);
                    Pa4 = parcelNumber.Substring(10, 4);

                    Thread.Sleep(3000);
                    driver.FindElement(By.XPath("//*[@id='parcel1']")).SendKeys(Pa1);
                    driver.FindElement(By.XPath("//*[@id='parcel2']")).SendKeys(Pa2);
                    driver.FindElement(By.XPath("//*[@id='parcel3']")).SendKeys(Pa3);
                    driver.FindElement(By.XPath("//*[@id='parcel4']")).SendKeys(Pa4);
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderno, parcelNumber, "Tax Search", driver, "CA", "Sacramento");
                    try
                    {
                        IWebElement IAddressSearch1 = driver.FindElement(By.XPath("//*[@id='PopupDisclaimerDiv']/div/div/div[3]/button[2]"));
                        IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                        js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                        Thread.Sleep(2000);
                       // driver.FindElement(By.XPath("//*[@id='PopupDisclaimerDiv']/div/div/div[3]/button[2]")).Click();
                    }
                    catch { }
                    driver.FindElement(By.XPath("//*[@id='btnSubmit']")).Click();
                    //gc.CreatePdf(orderno, parcelNumber, "Input Passed Tax Search", driver, "CA", "El Dorado");
                    Thread.Sleep(3000);
                    int I = 0;
                    gc.CreatePdf(orderno, parcelNumber, "Tax Detail", driver, "CA", "Sacramento");
                    string TaxID = driver.FindElement(By.Id("parcelGlobal")).Text.Replace("Parcel Number ", "").Trim();
                    amc.TaxId = TaxID;
                    string FirstAmount = "", SecondAmount = "", FirstDelinquent = "", SecondDelinquent = "", FirstPenalty = "", SecondPenalty = "", FirstStatus = "", SecondStatus = "";
                    IWebElement CurrentTaxHistoryTB1 = driver.FindElement(By.XPath("//*[@id='billDetailGrid']/table/tbody[1]"));
                    IList<IWebElement> CurrentTaxHistoryTR1 = CurrentTaxHistoryTB1.FindElements(By.TagName("tr"));
                    IList<IWebElement> CurrentTaxHistoryTD1;

                    string BillNumber = "", Billtype = "", DirectLevyPortion = "", OriginalbillAmount = "";
                    try
                    {
                        CurrentTaxHistoryTB1 = driver.FindElement(By.XPath("//*[@id='billDetailGrid']/table/tbody[1]"));
                        CurrentTaxHistoryTR1 = CurrentTaxHistoryTB1.FindElements(By.TagName("tr"));



                        foreach (IWebElement row1 in CurrentTaxHistoryTR1)
                        {

                            CurrentTaxHistoryTD1 = row1.FindElements(By.TagName("td"));
                            if (CurrentTaxHistoryTD1.Count != 0 && CurrentTaxHistoryTD1.Count != 1)
                            {
                                BillNumber = CurrentTaxHistoryTD1[0].Text;
                                Roll.Add(BillNumber);

                                if (row1.Text.Contains("Secured Supplemental") || row1.Text.Contains("Cancelled") || row1.Text.Contains("Secured Additional"))
                                {
                                    amc.IsDelinquent = "Yes";
                                }
                                ////driver.FindElement(By.XPath("//*[@id='navIcons']/li[3]/a")).Click();
                                ////Thread.Sleep(2000);

                            }

                        }
                    }
                    catch { }

                    try
                    {
                        IWebElement IPirorYear = driver.FindElement(By.XPath("//*[@id='parcelMessages ']"));
                        if (IPirorYear.Text.Contains("Prior Years Taxes Are Outstanding"))
                        {
                            amc.IsDelinquent = "Yes";
                        }
                    }
                    catch { }

                    foreach (string Go in Roll)
                    {

                        //driver.Navigate().GoToUrl("https://eproptax.saccounty.net/#BillDetail/" + Go + "");
                        driver.Navigate().GoToUrl("https://eproptax.saccounty.net/#secured/BillDetail/" + Go + "");
                        Thread.Sleep(5000);
                        string SecuredAnnual = "", FirstInstallment = "", SecondInstallment = "", TaxRate = "";
                        CreatePdf(orderno, parcelNumber, "Tax Detail" + I, driver, "CA", "Sacramento");
                        IWebElement CurrentTaxHistoryTB = driver.FindElement(By.XPath("//*[@id='billDetailGrid']/table/tbody"));
                        IList<IWebElement> CurrentTaxHistoryTR = CurrentTaxHistoryTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> CurrentTaxHistoryTD;
                        I++;
                        foreach (IWebElement row in CurrentTaxHistoryTR)
                        {
                            CurrentTaxHistoryTD = row.FindElements(By.TagName("td"));
                            if (CurrentTaxHistoryTD.Count != 0 && row.Text.Trim() != "" && !row.Text.Contains("There is a fee") && CurrentTaxHistoryTD.Count > 2)
                            {
                                TaxRate = driver.FindElement(By.XPath("//*[@id='taxRateGlobal']/a/span")).Text;
                                SecuredAnnual = CurrentTaxHistoryTD[0].Text;
                                FirstInstallment = CurrentTaxHistoryTD[1].Text;
                                SecondInstallment = CurrentTaxHistoryTD[2].Text;

                                string Deliquent = Billtype + "~" + TaxRate + "~" + SecuredAnnual + "~" + FirstInstallment + "~" + SecondInstallment;

                                gc.insert_date(orderno, parcelNumber, 371, Deliquent, 1, DateTime.Now);

                            }
                            try
                            {
                                if (CurrentTaxHistoryTD.Count != 0 && row.Text.Contains("Amount:"))
                                {
                                    amc.Instamount1 = CurrentTaxHistoryTD[1].Text.Trim();
                                    amc.Instamount2 = CurrentTaxHistoryTD[2].Text.Trim();
                                }
                                if (CurrentTaxHistoryTD.Count != 0 && row.Text.Contains("Delinquent Date:"))
                                {
                                    FirstDelinquent = CurrentTaxHistoryTD[1].Text.Trim();
                                    SecondDelinquent = CurrentTaxHistoryTD[2].Text.Trim();
                                }
                                if (CurrentTaxHistoryTD.Count != 0 && row.Text.Contains("Penalty:"))
                                {
                                    FirstPenalty = CurrentTaxHistoryTD[1].Text.Trim();
                                    SecondPenalty = CurrentTaxHistoryTD[2].Text.Trim();
                                }
                                if (CurrentTaxHistoryTD.Count != 0 && row.Text.Contains("Status:"))
                                {
                                    FirstStatus = CurrentTaxHistoryTD[1].Text.Trim();
                                    SecondStatus = CurrentTaxHistoryTD[2].Text.Trim();
                                }
                            }
                            catch { }
                        }

                        driver.Navigate().Back();
                        Thread.Sleep(2000);

                    }
                    //try
                    //{
                    //    CurrentTaxHistoryTB1 = driver.FindElement(By.XPath("//*[@id='billDetailGrid']/table/tbody[1]"));
                    //    CurrentTaxHistoryTR1 = CurrentTaxHistoryTB1.FindElements(By.TagName("tr"));



                    //    foreach (IWebElement row1 in CurrentTaxHistoryTR1)
                    //    {

                    //        CurrentTaxHistoryTD1 = row1.FindElements(By.TagName("td"));
                    //        if (CurrentTaxHistoryTD1.Count != 0)
                    //        {
                    //            BillNumber = CurrentTaxHistoryTD1[0].Text;
                    //            Billtype = CurrentTaxHistoryTD1[1].Text;
                    //            DirectLevyPortion = CurrentTaxHistoryTD1[2].Text;
                    //            OriginalbillAmount = CurrentTaxHistoryTD1[3].Text;
                    //            if (DirectLevyPortion.Trim() != "")
                    //            {
                    //                CurrentTaxHistoryTD1[I].Click();
                    //                Thread.Sleep(2000);



                    //            }
                    //            // CurrentTaxHistoryTR1.Clear();

                    //            driver.FindElement(By.XPath("//*[@id='navIcons']/li[3]/a")).Click();
                    //            Thread.Sleep(2000);

                    //        }

                    //    }
                    //}
                    //catch { I++; }

                    //Amrock Installment
                    if (amc.IsDelinquent != "Yes" || amc.IsDelinquent == "No")
                    {
                        if (FirstPenalty.Contains("$0.00") && FirstPenalty != "")
                        {
                            if (FirstStatus.Contains("Paid"))
                            {
                                amc.InstPaidDue1 = "Paid";
                                amc.IsDelinquent = "No";
                            }
                            if (FirstStatus.Contains("Unpaid"))
                            {
                                amc.InstPaidDue1 = "Due";
                                amc.IsDelinquent = "No";
                            }
                        }
                        else if (!FirstPenalty.Contains("$0.00") && FirstPenalty != "")
                        {
                            if (FirstStatus.Contains("Paid") || FirstStatus.Contains("Unpaid"))
                            {
                                amc.IsDelinquent = "Yes";
                            }
                        }
                        if (SecondPenalty.Contains("$0.00") && SecondPenalty != "")
                        {
                            if (SecondStatus.Contains("Paid"))
                            {
                                amc.InstPaidDue2 = "Paid";
                                amc.IsDelinquent = "No";
                            }
                            if (SecondStatus.Contains("Unpaid"))
                            {
                                amc.InstPaidDue2 = "Due";
                                amc.IsDelinquent = "No";
                            }
                        }
                        else if (!SecondPenalty.Contains("$0.00") && SecondPenalty != "")
                        {
                            if (SecondStatus.Contains("Paid") || SecondStatus.Contains("Unpaid"))
                            {
                                amc.IsDelinquent = "Yes";
                            }
                        }
                    }


                    driver.FindElement(By.XPath("//*[@id='navIcons']/li[3]/a")).Click();
                    Thread.Sleep(2000);
                    try
                    {
                        listurl.Clear();
                        CurrentTaxHistoryTB1 = driver.FindElement(By.XPath("//*[@id='billDetailGrid']/table/tbody[1]"));
                        CurrentTaxHistoryTR1 = CurrentTaxHistoryTB1.FindElements(By.TagName("tr"));



                        foreach (IWebElement row1 in CurrentTaxHistoryTR1)
                        {

                            CurrentTaxHistoryTD1 = row1.FindElements(By.TagName("td"));
                            if (CurrentTaxHistoryTD1.Count != 0 && CurrentTaxHistoryTD1.Count != 1 && row1.Text.Trim() != "" && !row1.Text.Contains("There is a fee"))
                            {
                                listurl.Add(CurrentTaxHistoryTD1[0].Text);

                            }

                        }

                        foreach (string bill in listurl)
                        {
                                                //     https://eproptax.saccounty.net/#secured/DirectLevy/18107260
                            driver.Navigate().GoToUrl("https://eproptax.saccounty.net/#secured/DirectLevy/" + bill + "");
                            Thread.Sleep(6000);
                            CreatePdf(orderno, parcelNumber, "Tax Detail" + bill, driver, "CA", "Sacramento");
                            IWebElement CurrentPayHistoryTB = driver.FindElement(By.XPath("//*[@id='billDetailGrid']/table/tbody"));
                            //*[@id="billDetailGrid"]/table/tbody
                            IList<IWebElement> CurrentPayHistoryTR = CurrentPayHistoryTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> CurrentPayHistoryTD;
                            string DirectLevyNumber = "", LevyName = "", LevyAmount = "", Bond = "";
                            foreach (IWebElement row2 in CurrentPayHistoryTR)
                            {
                                CurrentPayHistoryTD = row2.FindElements(By.TagName("td"));
                                if (CurrentPayHistoryTD.Count != 0 && row2.Text.Trim() != "" && !row2.Text.Contains("There is a fee"))
                                {

                                    // string TaxRate1 = driver.FindElement(By.XPath("//*[@id='taxRateGlobal']/a/span")).Text;
                                    DirectLevyNumber = CurrentPayHistoryTD[0].Text;
                                    Bond = CurrentPayHistoryTD[1].Text;
                                    LevyName = CurrentPayHistoryTD[2].Text;
                                    LevyAmount = CurrentPayHistoryTD[3].Text;


                                    string taxlevy = DirectLevyNumber + "~" + Bond + "~" + LevyName + "~" + LevyAmount;

                                    gc.insert_date(orderno, parcelNumber, 372, taxlevy, 1, DateTime.Now);

                                }
                            }
                        }


                    }
                    catch { }

                    driver.FindElement(By.XPath("//*[@id='navIcons']/li[3]/a")).Click();
                    Thread.Sleep(2000);
                    listurl.Clear();
                    try
                    {
                        gc.CreatePdf(orderno, parcelNumber, "Tax Bill", driver, "CA", "Sacramento");
                        CurrentTaxHistoryTB1 = driver.FindElement(By.XPath("//*[@id='billDetailGrid']/table/tbody[1]"));
                        CurrentTaxHistoryTR1 = CurrentTaxHistoryTB1.FindElements(By.TagName("tr"));

                        foreach (IWebElement row1 in CurrentTaxHistoryTR1)
                        {

                            CurrentTaxHistoryTD1 = row1.FindElements(By.TagName("td"));
                            if (CurrentTaxHistoryTD1.Count != 0 && CurrentTaxHistoryTD1.Count != 1 && row1.Text.Trim() != "" && !row1.Text.Contains("There is a fee"))
                            {
                                BillNumber = CurrentTaxHistoryTD1[0].Text;
                                Billtype = CurrentTaxHistoryTD1[1].Text;
                                DirectLevyPortion = CurrentTaxHistoryTD1[2].Text;
                                OriginalbillAmount = CurrentTaxHistoryTD1[3].Text;
                                string Tax = BillNumber + "~" + Billtype + "~" + DirectLevyPortion + "~" + OriginalbillAmount + "~" + CurrentTaxHistoryTD1[4].Text;
                                gc.insert_date(orderno, parcelNumber, 370, Tax, 1, DateTime.Now);



                            }
                        }
                    }
                    catch { }
                    driver.FindElement(By.XPath("//*[@id='navIcons']/li[4]/a")).Click();
                    Thread.Sleep(5000);

                    gc.CreatePdf(orderno, parcelNumber, "Tax History", driver, "CA", "Sacramento");

                    try
                    {
                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='navIcons']/li[6]/a")));
                        gc.CreatePdf(orderno, parcelNumber, "Tax History1", driver, "CA", "Sacramento");
                    }
                    catch { }

                    try
                    {
                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='applicationHost']/div/div/div/div[2]/h3")));
                        gc.CreatePdf(orderno, parcelNumber, "Tax History2", driver, "CA", "Sacramento");
                    }
                    catch { }
                    string BillYear = "", TaxBillNumber = "", BillType = "", PayByDate = "", Amount = "", PaidDate = "";


                    IWebElement PayHistoryTB = driver.FindElement(By.XPath("//*[@id='billDetailGrid']/table/tbody"));
                    IList<IWebElement> PayHistoryTR = PayHistoryTB.FindElements(By.TagName("tr"));
                    IList<IWebElement> PayHistoryTD;
                    foreach (IWebElement row2 in PayHistoryTR)
                    {
                        PayHistoryTD = row2.FindElements(By.TagName("td"));
                        if (PayHistoryTD.Count != 0 && PayHistoryTD.Count == 6)
                        {
                            BillYear = PayHistoryTD[0].Text;
                            TaxBillNumber = PayHistoryTD[1].Text;
                            BillType = PayHistoryTD[2].Text;
                            PayByDate = PayHistoryTD[3].Text;
                            Amount = PayHistoryTD[4].Text;
                            PaidDate = PayHistoryTD[5].Text;



                            string TaxHistory = BillYear + "~" + TaxBillNumber + "~" + BillType + "~" + PayByDate + "~" + Amount + "~" + PaidDate;

                            gc.insert_date(orderno, parcelNumber, 373, TaxHistory, 1, DateTime.Now);

                        }

                        try
                        {
                            if (amc.IsDelinquent != "Yes" || amc.IsDelinquent == "No")
                            {
                                if (FirstDelinquent == PayHistoryTD[3].Text.Trim())
                                {
                                    amc.TaxYear = PayHistoryTD[0].Text;
                                    amc.Instamountpaid1 = PayHistoryTD[3].Text.Trim();
                                }
                                if (SecondDelinquent == PayHistoryTD[3].Text.Trim())
                                {
                                    amc.TaxYear = PayHistoryTD[0].Text;
                                    amc.Instamountpaid2 = PayHistoryTD[3].Text.Trim();
                                }
                            }
                        }
                        catch { }
                    }

                    if (amc.IsDelinquent == "Yes")
                    {
                        gc.InsertAmrockTax(orderno, amc.TaxId, null, null, null, null, null, null, null, null, null, null, null, null, amc.IsDelinquent);
                    }
                    if(amc.IsDelinquent == "No")
                    {
                        gc.InsertAmrockTax(orderno, amc.TaxId, amc.Instamount1, amc.Instamount2, amc.Instamount3, amc.Instamount4, amc.Instamountpaid1, amc.Instamountpaid2, amc.Instamountpaid3, amc.Instamountpaid4, amc.InstPaidDue1, amc.InstPaidDue2, amc.instPaidDue3, amc.instPaidDue4, amc.IsDelinquent);
                    }

                    // gc.CreatePdf(orderno, parcelNumber, "Tax History Detail", driver, "CA", "El Dorado");

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");

                    gc.insert_TakenTime(orderno, "CA", "Sacramento", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);


                    driver.Quit();
                    gc.mergpdf(orderno, "CA", "Sacramento");
                    //gc.MMREM_Template(orderno, parcelNumber, "", driver, "CA", "Sacramento", "10", "4");
                    return "Data Inserted Successfully";
                }

                catch (Exception ex)
                {
                    driver.Quit();
                    GlobalClass.LogError(ex, orderno);
                    throw ex;
                }
            }
        }

        public void ByVisibleElement(IWebElement Element)
        {


            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

            //Launch the application		
            // driver.Navigate().GoToUrl("http://gis.hcpafl.org/PropertySearch/#/nav/Basic%20Search");

            //Find element by link text and store in variable "Element"        		
            // IWebElement Element = driver.FindElement(By.LinkText("Disclaimer"));

            //This will scroll the page till the element is found		
            js.ExecuteScript("arguments[0].scrollIntoView();", Element);
        }

        public void CreatePdf(string orderno, string parcelno, string pdfName, IWebDriver driver, string sname, string cname)
        {
            GlobalClass Pdf = new GlobalClass();
            string outputPath = Pdf.ReturnPath(sname, cname);
            outputPath = outputPath + orderno + "\\" + parcelno + "\\";
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            string img = outputPath + pdfName + ".png";
            string pdf = outputPath + pdfName + ".pdf";

            //driver.Manage().Window.Maximize();
            driver.TakeScreenshot().SaveAsFile(img, ScreenshotImageFormat.Png);

            WebDriverTest.ConvertImageToPdf(img, pdf);
            if (File.Exists(img))
            {
                File.Delete(img);
            }

        }
    }
}