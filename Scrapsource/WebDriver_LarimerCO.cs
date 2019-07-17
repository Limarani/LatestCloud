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

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_LarimerCO
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        public string FTP_LarimerCO(string houseno, string sname, string stype, string account, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string straccount = "", strOwner = "", strOccuPancy = "", strAddress = "", multicount = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new PhantomJSDriver();
            //driver = new ChromeDriver();
            var option = new ChromeOptions();
            option.AddArgument("No-Sandbox");
            using (driver = new ChromeDriver(option))
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    //old
                    //http://www.co.larimer.co.us/assessor/query/search.cfm
                    driver.Navigate().GoToUrl("https://www.larimer.org/assessor/search#/property/");

                    if (searchType == "titleflex")
                    {
                        string address = houseno + " " + sname + " " + stype + " " + account;
                        gc.TitleFlexSearch(orderNumber, "", ownername, address, "CO", "Larimer");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_LarimerCO"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        driver.FindElement(By.Id("fromAddrNum")).SendKeys(houseno);
                        driver.FindElement(By.Id("address")).SendKeys(sname);
                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "CO", "Larimer");
                        driver.FindElement(By.XPath("//*[@id='searchForm']/div/div[2]/div/button")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        try
                        {
                            multicount = driver.FindElement(By.XPath("//*[@id='resultsTable']/thead/tr[1]/td/table-header-sm/div[2]/div[1]/span[1]")).Text.Trim();
                            if (Convert.ToInt32(multicount) > 1 && Convert.ToInt32(multicount) != 0)
                            {
                                if (Convert.ToInt32(multicount) > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_LarimerCO_Maximum"] = "Maximum";
                                    driver.Quit();
                                }

                                try
                                {
                                    IWebElement IMultiChoose = driver.FindElement(By.Id("items2"));
                                    SelectElement Smultiselect = new SelectElement(IMultiChoose);
                                    Smultiselect.SelectByText("41");
                                }
                                catch { }

                                IWebElement IMultiAddress = driver.FindElement(By.XPath("//*[@id='resultsTable']/tbody"));
                                IList<IWebElement> IMultiRow = IMultiAddress.FindElements(By.TagName("tr"));
                                IList<IWebElement> IMultiTd;
                                foreach (IWebElement multi in IMultiRow)
                                {
                                    IMultiTd = multi.FindElements(By.TagName("td"));
                                    if (IMultiTd.Count != 0)
                                    {
                                        straccount = IMultiTd[0].Text;
                                        strOwner = IMultiTd[2].Text;
                                        strOccuPancy = IMultiTd[3].Text;
                                        strAddress = IMultiTd[4].Text;

                                        string multiDetails = straccount + "~" + strOwner + "~" + strOccuPancy + "~" + strAddress;
                                        gc.insert_date(orderNumber, IMultiTd[1].Text, 634, multiDetails, 1, DateTime.Now);
                                    }
                                }
                                gc.CreatePdf_WOP(orderNumber, "Multi Address Search", driver, "CO", "Larimer");
                                HttpContext.Current.Session["multiParcel_LarimerCO"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.Id("parcelno")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "CO", "Larimer");
                        driver.FindElement(By.XPath("//*[@id='searchForm']/div/div[2]/div/button")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        try
                        {
                            multicount = driver.FindElement(By.XPath("//*[@id='resultsTable']/thead/tr[1]/td/table-header-sm/div[2]/div[1]/span[1]")).Text.Trim();
                            if (Convert.ToInt32(multicount) > 1 && Convert.ToInt32(multicount) != 0)
                            {
                                driver.FindElement(By.XPath("//*[@id='resultsTable']/tbody/tr[1]/td[2]")).Click();
                                Thread.Sleep(2000);
                            }
                        }
                        catch { }
                    }
                    if (searchType == "account")
                    {
                        if (account.Contains("R"))
                        {
                            account = account.Replace("R", "");
                        }
                        driver.FindElement(By.Id("scheduleno")).SendKeys(account);
                        gc.CreatePdf_WOP(orderNumber, "Account Search", driver, "CO", "Larimer");
                        driver.FindElement(By.XPath("//*[@id='searchForm']/div/div[2]/div/button")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        try
                        {
                            multicount = driver.FindElement(By.XPath("//*[@id='resultsTable']/thead/tr[1]/td/table-header-sm/div[2]/div[1]/span[1]")).Text.Trim();
                            if (Convert.ToInt32(multicount) > 1 && Convert.ToInt32(multicount) != 0)
                            {
                                driver.FindElement(By.XPath("//*[@id='resultsTable']/tbody/tr[1]/td[2]")).Click();
                                Thread.Sleep(2000);
                            }
                        }
                        catch { }
                    }
                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("name")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "OwnerName Search", driver, "CO", "Larimer");
                        driver.FindElement(By.XPath("//*[@id='searchForm']/div/div[2]/div/button")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        try
                        {
                            multicount = driver.FindElement(By.XPath("//*[@id='resultsTable']/thead/tr[1]/td/table-header-sm/div[2]/div[1]/span[1]")).Text.Trim();
                            if (Convert.ToInt32(multicount) > 1 && Convert.ToInt32(multicount) != 0)
                            {
                                if (Convert.ToInt32(multicount) > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_LarimerCO_Maximum"] = "Maximum";
                                    driver.Quit();
                                }

                                try
                                {
                                    IWebElement IMultiChoose = driver.FindElement(By.Id("items2"));
                                    SelectElement Smultiselect = new SelectElement(IMultiChoose);
                                    Smultiselect.SelectByText("41");
                                }
                                catch { }

                                IWebElement IMultiAddress = driver.FindElement(By.XPath("//*[@id='resultsTable']/tbody"));
                                IList<IWebElement> IMultiRow = IMultiAddress.FindElements(By.TagName("tr"));
                                IList<IWebElement> IMultiTd;
                                foreach (IWebElement multi in IMultiRow)
                                {
                                    IMultiTd = multi.FindElements(By.TagName("td"));
                                    if (IMultiTd.Count != 0)
                                    {
                                        straccount = IMultiTd[0].Text;
                                        strOwner = IMultiTd[2].Text;
                                        strOccuPancy = IMultiTd[3].Text;
                                        strAddress = IMultiTd[4].Text;

                                        string multiDetails = straccount + "~" + strOwner + "~" + strOccuPancy + "~" + strAddress;
                                        gc.insert_date(orderNumber, IMultiTd[1].Text, 634, multiDetails, 1, DateTime.Now);
                                    }
                                }

                                gc.CreatePdf_WOP(orderNumber, "Multi Address Search", driver, "CO", "Larimer");
                                HttpContext.Current.Session["multiParcel_LarimerCO"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                        }
                        catch { }
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.XPath("//*[@id='main']/div[3]/div/div[2]/div/div/div/div/div[2]/table-results/div[2]"));
                        if(INodata.Text.Contains("search returned no results"))
                        {
                            HttpContext.Current.Session["Nodata_LarimerCO"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    //Property Details                               
                    driver.SwitchTo().Window(driver.WindowHandles.Last());
                    Thread.Sleep(2000);
                    gc.CreatePdf_WOP(orderNumber, "Switch window", driver, "CO", "Larimer");
                    ByVisibleElement(driver.FindElement(By.XPath("//*[@id='p-info']/div[2]/div/div[1]")));
                    gc.CreatePdf_WOP(orderNumber, "Switch window", driver, "CO", "Larimer");
                    driver.FindElement(By.XPath("//*[@id='detailModal']/div/div/div[1]/button[2]")).Click();
                    driver.SwitchTo().Window(driver.WindowHandles.Last());
                    Thread.Sleep(2000);
                    string strgeneralInfo = "", straddress = "", ScheduleNumber = "", strLegal = "", LegalDiscription = "", PropertyTaxYear = "", PropertyAddress = "", strownerName = "", MailingAddress = "", OwnerName = "", YearBuilt = "", PropertyType = "", Occupancy = "";
                    strgeneralInfo = driver.FindElement(By.XPath("//*[@id='p-info']/div[1]/div[1]/div[1]/div[1]")).Text;
                    parcelNumber = gc.Between(strgeneralInfo, "Parcel Number: ", "Schedule Number:");
                    gc.CreatePdf(orderNumber, parcelNumber, "Property Search Result", driver, "CO", "Larimer");
                    ScheduleNumber = gc.Between(strgeneralInfo, "Schedule Number: ", "Tax District:");
                    PropertyTaxYear = gc.Between(strgeneralInfo, "Property Tax Year: ", "Current Mill Levy:");
                    strLegal = driver.FindElement(By.XPath("//*[@id='p-info']/div[1]/div[1]/div[2]")).Text;
                    LegalDiscription = GlobalClass.After(strLegal, "Legal Description: ");
                    straddress = driver.FindElement(By.XPath("//*[@id='p-info']/div[1]/div[1]/div[1]/div[2]")).Text;
                    PropertyAddress = gc.Between(straddress, "Property Address:", "Owner Name & Address:");
                    strownerName = GlobalClass.After(straddress, "Owner Name & Address:");
                    MailingAddress = GlobalClass.After(strownerName, "\r\n\r\n");
                    OwnerName = GlobalClass.Before(strownerName, "\r\n\r\n");

                    string LActualValue = "", LAssessedValue = "", IActualValue = "", IAssessedValue = "", TActualValue = "", TAssessedValue = "";
                    IWebElement IAssessmentTable = driver.FindElement(By.XPath("//*[@id='p-info']/div[3]/table/tbody"));
                    IList<IWebElement> IAssessmentRow = IAssessmentTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> IAssessmentTD;
                    foreach (IWebElement Assessment in IAssessmentRow)
                    {
                        IAssessmentTD = Assessment.FindElements(By.TagName("td"));
                        if (IAssessmentTD.Count != 0 && !Assessment.Text.Contains("Totals:") && Assessment.Text.Contains("Land"))
                        {
                            LActualValue = IAssessmentTD[3].Text;
                            LAssessedValue = IAssessmentTD[4].Text;
                        }
                        if (IAssessmentTD.Count != 0 && !Assessment.Text.Contains("Totals:") && (Assessment.Text.Contains("Improvement") || Assessment.Text.Contains("Building")))
                        {
                            IActualValue = IAssessmentTD[3].Text;
                            IAssessedValue = IAssessmentTD[4].Text;
                        }
                        if (IAssessmentTD.Count != 0 && Assessment.Text.Contains("Totals:"))
                        {
                            TActualValue = IAssessmentTD[1].Text;
                            TAssessedValue = IAssessmentTD[2].Text;
                        }
                    }
                    string AssessmentDetails = PropertyTaxYear + "~" + LActualValue + "~" + IActualValue + "~" + TActualValue + "~" + LAssessedValue + "~" + IAssessedValue + "~" + TAssessedValue;
                    gc.insert_date(orderNumber, parcelNumber, 640, AssessmentDetails, 1, DateTime.Now);

                    driver.FindElement(By.LinkText("Building Info")).Click();
                    gc.CreatePdf(orderNumber, parcelNumber, "Year Build", driver, "CO", "Larimer");
                    try
                    {
                        IWebElement Iyear = driver.FindElement(By.XPath("//*[@id='imp']/tbody"));
                        IList<IWebElement> IyearRow = Iyear.FindElements(By.TagName("tr"));
                        IList<IWebElement> IyearTD;
                        foreach (IWebElement year in IyearRow)
                        {
                            IyearTD = year.FindElements(By.TagName("td"));
                            if (IyearTD.Count != 0 && year.Text.Contains("Year Built from:"))
                            {
                                YearBuilt = IyearTD[1].Text;
                            }
                            if (IyearTD.Count != 0 && year.Text.Contains("Property Type:"))
                            {
                                PropertyType = IyearTD[1].Text;
                            }
                            if (IyearTD.Count != 0 && year.Text.Contains("Occupancy:"))
                            {
                                Occupancy = IyearTD[1].Text;
                            }

                        }
                    }
                    catch { }
                    ByVisibleElement(driver.FindElement(By.XPath("//*[@id='main']/div[3]/div/div[2]/div/div/div/div/div[4]/div[2]/div/div[3]/div[2]/table")));
                    gc.CreatePdf(orderNumber, parcelNumber, "Building Info", driver, "CO", "Larimer");
                    string PropertyDetails = ScheduleNumber + "~" + OwnerName + "~" + PropertyAddress + "~" + MailingAddress + "~" + LegalDiscription + "~" + YearBuilt + "~" + PropertyType + "~" + Occupancy;
                    gc.insert_date(orderNumber, parcelNumber, 639, PropertyDetails, 1, DateTime.Now);
                    driver.SwitchTo().DefaultContent();
                    string TaxParcelNo = "", TaxYear = "", TaxPeriod = "", TaxDue = "", TaxAmount = "", TaxAuthority = "", TaxAuthOffice = "", TaxAuthAddress = "";
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    try
                    {
                        var chromeOptions = new ChromeOptions();
                        var chDriver = new ChromeDriver(chromeOptions);
                        IJavaScriptExecutor js = (IJavaScriptExecutor)chDriver;
                        try
                        {
                            chDriver.Navigate().GoToUrl("https://www.larimer.org/treasurer/search");
                            chDriver.FindElement(By.Id("parcelno")).SendKeys(parcelNumber);
                            chDriver.FindElement(By.XPath("//*[@id='searchForm']/div[2]/div[2]/input")).SendKeys(Keys.Enter);
                            Thread.Sleep(3000);
                            chDriver.FindElement(By.XPath("//*[@id='resultsTable']/tbody/tr")).Click();
                            Thread.Sleep(5000);
                            try
                            {
                                IWebElement IPrperty = chDriver.FindElement(By.XPath("//*[@id='detailModal']/div/div/div[2]/div/div/h2"));
                                js.ExecuteScript("arguments[0].scrollIntoView();", IPrperty);
                                gc.CreatePdf_Chrome(orderNumber, parcelNumber, "Tax Property", chDriver, "CO", "Larimer");
                                IWebElement IPayment = chDriver.FindElement(By.XPath("//*[@id='detailModal']/div/div/div[2]/div/div/div[2]/div[2]"));
                                js.ExecuteScript("arguments[0].scrollIntoView();", IPayment);
                                gc.CreatePdf_Chrome(orderNumber, parcelNumber, "Tax Payment", chDriver, "CO", "Larimer");
                                IWebElement IStPayment = chDriver.FindElement(By.XPath("//*[@id='detailModal']/div/div/div[2]/div/div/div[5]/div[1]"));
                                js.ExecuteScript("arguments[0].scrollIntoView();", IStPayment);
                                gc.CreatePdf_Chrome(orderNumber, parcelNumber, "Tax StPayment", chDriver, "CO", "Larimer");
                                IWebElement ILevy = chDriver.FindElement(By.XPath("//*[@id='detailModal']/div/div/div[2]/div/div/div[5]/div[2]"));
                                js.ExecuteScript("arguments[0].scrollIntoView();", ILevy);
                                gc.CreatePdf_Chrome(orderNumber, parcelNumber, "Tax Levy", chDriver, "CO", "Larimer");
                            }
                            catch { }
                            chDriver.Quit();
                        }

                        catch (Exception ex)
                        {
                            try
                            {
                                chDriver.Navigate().GoToUrl("https://www.larimer.org/treasurer/search");
                                chDriver.FindElement(By.Id("parcelno")).SendKeys(parcelNumber);

                                int Year = DateTime.Now.Year;
                                string currentyear = "";
                                for (int i = 1; i < 3; i++)

                                    currentyear = Convert.ToString(Year - 2);
                                IWebElement ITaxYear = driver.FindElement(By.XPath("//*[@id='main']/div[3]/div/div[2]/div/div/div/div/div/div[2]/div[2]/form/select"));
                                SelectElement STaxYear = new SelectElement(ITaxYear);
                                STaxYear.SelectByText(currentyear);
                                TaxYear = STaxYear.SelectedOption.Text;
                                Thread.Sleep(2000);

                            }
                            catch { }
                        }

                        chDriver.Quit();

                    }
                    catch
                    {

                    }



                    driver.Navigate().GoToUrl("https://www.larimer.org/treasurer/search");
                    driver.FindElement(By.Id("parcelno")).SendKeys(parcelNumber);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Search", driver, "CO", "Larimer");
                    driver.FindElement(By.XPath("//*[@id='searchForm']/div[2]/div[2]/input")).SendKeys(Keys.Enter);
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Search Result", driver, "CO", "Larimer");
                    //*[@id="resultsTable"]/tbody/tr/td[2]
                    //*[@id="resultsTable"]/tbody/tr
                    try
                    {
                        driver.FindElement(By.XPath(" //*[@id='resultsTable']/tbody/tr/td[2]")).Click();
                        Thread.Sleep(2000);
                    }
                    catch { }
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='resultsTable']/tbody/tr")).Click();
                        Thread.Sleep(2000);
                    }
                    catch { }
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Result", driver, "CO", "Larimer");
                    driver.SwitchTo().Window(driver.WindowHandles.Last());
                    Thread.Sleep(2000);
                    //*[@id="detailModal"]/div/div/div[1]/button[2]/i
                    driver.FindElement(By.XPath(" //*[@id='detailModal']/div/div/div[1]/button[2]/i")).Click();
                    Thread.Sleep(3000);
                    driver.SwitchTo().Window(driver.WindowHandles.Last());
                    string parcelsearch = driver.FindElement(By.XPath("//*[@id='main']/div[3]/div/div[2]/div/div/div/div/div/div[3]/div[1]/div[2]/p[3]/span[2]")).Text;
                    TaxParcelNo = GlobalClass.After(parcelsearch, "Parcel Number: ");
                    try
                    {
                        int Year = DateTime.Now.Year;
                        string currentyear = "";
                        for (int i = 0; i < 2; i++)
                        {
                            if (TaxYear == "")
                            {
                                try
                                {
                                    currentyear = Convert.ToString(Year);
                                    //*[@id="main"]/div[3]/div/div[2]/div/div/div/div/div/div[3]/div[1]/div[2]/form/select
                                    IWebElement ITaxYear = driver.FindElement(By.XPath("//*[@id='main']/div[3]/div/div[2]/div/div/div/div/div/div[3]/div[1]/div[2]/form/select"));
                                    SelectElement STaxYear = new SelectElement(ITaxYear);
                                    STaxYear.SelectByText(currentyear);
                                    TaxYear = STaxYear.SelectedOption.Text;
                                    Thread.Sleep(2000);
                                }
                                catch { }
                            }
                            Year--;
                        }
                    }
                    catch { }

                    //gc.CreatePdf(orderNumber, parcelNumber, "Tax Assessment Result Details", driver, "CO", "Larimer");
                    //Tax Authority
                    TaxAuthOffice = driver.FindElement(By.XPath("//*[@id='main']/div[3]/div/div[2]/div/div/div/div/div/div[3]/div[5]/div[2]/div/div[2]/div[1]/strong")).Text;
                    TaxAuthAddress = GlobalClass.After(driver.FindElement(By.XPath("//*[@id='main']/div[3]/div/div[2]/div/div/div/div/div/div[3]/div[5]/div[2]/div/div[2]/div[2]")).Text, "Mailing address:");
                    TaxAuthority = TaxAuthOffice + " " + TaxAuthAddress.Replace("\r\n", " ").Trim();
                    string TaxAuth = TaxYear + "~" + "" + "~" + "" + "~" + "" + "~" + TaxAuthority;
                    gc.insert_date(orderNumber, TaxParcelNo, 641, TaxAuth, 1, DateTime.Now);

                    //Tax General Information
                    IWebElement ITaxInfo = driver.FindElement(By.XPath("//*[@id='main']/div[3]/div/div[2]/div/div/div/div/div/div[3]/div[2]/div[2]/table/tbody"));
                    IList<IWebElement> ITaxInfoRow = ITaxInfo.FindElements(By.TagName("tr"));
                    IList<IWebElement> ITaxInfoTD;
                    foreach (IWebElement Info in ITaxInfoRow)
                    {
                        ITaxInfoTD = Info.FindElements(By.TagName("td"));
                        if (ITaxInfoTD.Count != 0 && ITaxInfoTD.Count == 3 && !Info.Text.Contains("Payment Received Date") && !Info.Text.Contains("Calculate"))
                        {
                            TaxPeriod = ITaxInfoTD[0].Text;
                            TaxDue = ITaxInfoTD[1].Text;
                            TaxAmount = ITaxInfoTD[2].Text;

                            string TaxInformation = "~" + TaxPeriod + "~" + TaxDue + "~" + TaxAmount + "~";
                            gc.insert_date(orderNumber, TaxParcelNo, 641, TaxInformation, 1, DateTime.Now);
                        }
                        if (ITaxInfoTD.Count != 0 && ITaxInfoTD.Count == 2 && !Info.Text.Contains("Calculate") && Info.Text.Contains("Payment Received Date") && ITaxInfoTD.Count < 3 && !Info.Text.Contains("Period"))
                        {
                            TaxPeriod = ITaxInfoTD[0].Text;
                            TaxDue = ITaxInfoTD[1].Text;
                            TaxAmount = "";

                            string TaxInformation = "~" + TaxPeriod + "~" + TaxDue + "~" + TaxAmount + "~";
                            gc.insert_date(orderNumber, TaxParcelNo, 641, TaxInformation, 1, DateTime.Now);
                        }
                    }

                    //Tax Jurisdictions
                    string TaxLevy = "", TaxLevyAuthority = "", TaxLevyAmount = "";
                    IWebElement ITaxLevy = driver.FindElement(By.XPath("//*[@id='main']/div[3]/div/div[2]/div/div/div/div/div/div[3]/div[5]/div[1]/table[2]/tbody"));
                    IList<IWebElement> ITaxLevyRow = ITaxLevy.FindElements(By.TagName("tr"));
                    IList<IWebElement> ITaxLevyTD;
                    foreach (IWebElement Levy in ITaxLevyRow)
                    {
                        ITaxLevyTD = Levy.FindElements(By.TagName("td"));
                        if (ITaxLevyTD.Count != 0)
                        {
                            TaxLevy = ITaxLevyTD[0].Text;
                            TaxLevyAuthority = ITaxLevyTD[1].Text;
                            TaxLevyAmount = ITaxLevyTD[2].Text;

                            string TaxLevyJuris = TaxLevy + "~" + TaxLevyAuthority + "~" + TaxLevyAmount;
                            gc.insert_date(orderNumber, TaxParcelNo, 642, TaxLevyJuris, 1, DateTime.Now);
                        }
                    }

                    //Tax statement
                    try
                    {
                        IWebElement ITaxStatement = driver.FindElement(By.LinkText("Tax Statement"));
                        string strTaxSattement = ITaxStatement.GetAttribute("href");
                        gc.downloadfile(strTaxSattement, orderNumber, parcelNumber, "Tax Statement", "CO", "Larimer");
                    }
                    catch { }

                    //Tax History
                    driver.FindElement(By.XPath("//*[@id='main']/div[3]/div/div[2]/div/div/div/div/div/div[3]/div[1]/div[2]/p[4]/a[1]")).Click();
                    Thread.Sleep(3000);
                    driver.SwitchTo().Window(driver.WindowHandles.Last());
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax History Details", driver, "CO", "Larimer");
                    //Tax Status
                    string TaxStatusHead = "", TaxStatusAmount = "";
                    //*[@id="p-tax"]/div[3]/table[1]/tbody
                    IWebElement ITaxStatus = driver.FindElement(By.XPath("//*[@id='p-tax']/div[3]/table[1]/tbody"));
                    IList<IWebElement> ITaxStatusRow = ITaxStatus.FindElements(By.TagName("tr"));
                    IList<IWebElement> ITaxStatusTD;
                    foreach (IWebElement Status in ITaxStatusRow)
                    {
                        ITaxStatusTD = Status.FindElements(By.TagName("td"));
                        if (ITaxStatusTD.Count != 0)
                        {
                            TaxStatusHead += ITaxStatusTD[0].Text.Replace("'", "").Replace("(", "").Replace(")", "") + "~";
                            TaxStatusAmount += ITaxStatusTD[1].Text + "~";
                        }
                    }
                    TaxStatusHead = TaxStatusHead.Remove(TaxStatusHead.Length - 1, 1);
                    db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + TaxStatusHead + "' where Id = '" + 643 + "'");
                    TaxStatusAmount = TaxStatusAmount.Remove(TaxStatusAmount.Length - 1, 1);
                    gc.insert_date(orderNumber, TaxParcelNo, 643, TaxStatusAmount, 1, DateTime.Now);

                    //Tax History
                    string TaxHistoryYear = "", TaxLiability = "", PropertyBalance = "", OwnerTaxLiability = "", StateTaxLiabilty = "", TotalActualValue = "", TotalAssessedValue = "";
                    IWebElement ITaxHistory = driver.FindElement(By.XPath("//*[@id='p-tax']/div[2]/table/tbody"));
                    IList<IWebElement> ITaxHistoryRow = ITaxHistory.FindElements(By.TagName("tr"));
                    IList<IWebElement> ITaxHistoryTD;
                    foreach (IWebElement History in ITaxHistoryRow)
                    {
                        ITaxHistoryTD = History.FindElements(By.TagName("td"));
                        if (ITaxHistoryTD.Count != 0)
                        {
                            TaxHistoryYear = ITaxHistoryTD[0].Text;
                            TaxLiability = ITaxHistoryTD[1].Text;
                            PropertyBalance = ITaxHistoryTD[2].Text;
                            OwnerTaxLiability = ITaxHistoryTD[3].Text;
                            StateTaxLiabilty = ITaxHistoryTD[4].Text;
                            TotalActualValue = ITaxHistoryTD[5].Text;
                            TotalAssessedValue = ITaxHistoryTD[6].Text;

                            string TaxHistoryDetails = TaxHistoryYear + "~" + TaxLiability + "~" + PropertyBalance + "~" + OwnerTaxLiability + "~" + StateTaxLiabilty + "~" + TotalActualValue + "~" + TotalAssessedValue;
                            gc.insert_date(orderNumber, TaxParcelNo, 644, TaxHistoryDetails, 1, DateTime.Now);
                        }
                    }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "CO", "Larimer", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "CO", "Larimer");
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