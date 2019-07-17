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
    public class WebDriver_WASnohomish
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        int searchcount;
        public string FTP_WASnohomish(string streetNo, string direction, string streetName, string streetType, string unitNumber, string parcelNumber, string ownerName, string searchType, string orderNumber)
        {
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = ""; string address = "";
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            if (direction != "")
            {
                address = streetNo + " " + direction + " " + streetName + " " + streetType + " " + unitNumber;
            }
            else
            {
                address = streetNo + " " + streetName + " " + streetType + " " + unitNumber;
            }
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;

            // driver = new PhantomJSDriver();
            // driver = new ChromeDriver();
            using (driver = new PhantomJSDriver()) //ChromeDriver
            {

                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");


                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, "", ownerName, address.Trim(), "WA", "Snohomish");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_SnohomishWA"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    string Taxauthority = "", Taxauthority1 = "";
                    try
                    {
                        driver.Navigate().GoToUrl("https://snohomishcountywa.gov/1939/Contact-the-Treasurer");
                        Taxauthority1 = driver.FindElement(By.XPath("//*[@id='divEditora2843f18-3f97-4b0b-82fa-db3573a4564a']/div/p[3]")).Text.Trim();
                        Taxauthority = gc.Between(Taxauthority1, "Mailing Address:", "Location:").Trim();
                    }
                    catch { }
                    //*[@id="divInfoAdvde1c891c-1514-4aa8-a077-aa893157d9c0"]/div[1]/div/div/ol/li/iframe

                    driver.Navigate().GoToUrl("https://snohomishcountywa.gov/5167/Assessor");
                    IWebElement Frame = driver.FindElement(By.XPath("//*[@id='divInfoAdvbb390278-c95a-444b-b449-e5114383cd71']/div[1]/div/div/ol/li/iframe"));
                    driver.SwitchTo().Frame(Frame);
                    if (searchType == "address")
                    {
                        driver.FindElement(By.Id("strhousenum")).SendKeys(streetNo);
                        driver.FindElement(By.Id("strstrtname")).SendKeys(streetName.ToUpper());
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "WA", "Snohomish");
                        driver.FindElement(By.XPath("//*[@id='srchbar']/button")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        driver.SwitchTo().Window(driver.WindowHandles.Last());

                        try
                        {

                            string multi = GlobalClass.Before(driver.FindElement(By.Id("mMessage")).Text, " records returned from your search input.").Trim();
                            string strAddress = "", strparcel = "", strOwner = "";
                            List<string> searchlist = new List<string>();
                            IWebElement tbmulti = driver.FindElement(By.Id("mGrid"));
                            IList<IWebElement> TBmulti = tbmulti.FindElements(By.TagName("tr"));
                            IList<IWebElement> TRmulti;
                            IList<IWebElement> TDmulti;

                            foreach (IWebElement row in TBmulti)
                            {
                                TDmulti = row.FindElements(By.TagName("td"));

                                if (Convert.ToInt16(multi) > 1 && TDmulti.Count != 0 && TDmulti[0].Text.Trim() != "")
                                {
                                    if (TDmulti[0].Text.Trim() != "" & row.Text.Trim().Contains(address.ToUpper().Trim()))
                                    {
                                        strOwner = TDmulti[1].Text; strAddress = TDmulti[2].Text; strparcel = TDmulti[0].Text;
                                        searchlist.Add(TDmulti[0].Text);
                                        searchcount = searchlist.Count;
                                        //string multiDetails = strOwner + "~" + strAddress;
                                        //gc.insert_date(orderNumber, strparcel, 1390, multiDetails, 1, DateTime.Now);
                                    }

                                }
                            }
                            if (searchcount == 1)
                            {
                                IWebElement element2 = driver.FindElement(By.XPath("//*[@id='mGrid']/tbody/tr[2]/td[1]/a"));
                                IJavaScriptExecutor js2 = driver as IJavaScriptExecutor;
                                js2.ExecuteScript("arguments[0].click();", element2);
                                Thread.Sleep(3000);
                            }

                            else
                            {
                                if (searchcount > 1 && searchcount < 25)
                                {
                                    multiparcel(orderNumber, address);
                                    gc.CreatePdf_WOP(orderNumber, "Multi Address search Result", driver, "WA", "Snohomish");
                                    HttpContext.Current.Session["multiparcel_SnohomishWA"] = "Yes";
                                    driver.Quit();
                                    gc.mergpdf(orderNumber, "WA", "Snohomish");
                                    return "MultiParcel";
                                }
                                if (searchcount > 25)
                                {
                                    HttpContext.Current.Session["multiparcel_SnohomishWA_Multicount"] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }
                            }
                        }
                        catch { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("mMessage")).Text;
                            if (nodata.Contains("0 records returned"))
                            {
                                HttpContext.Current.Session["Nodata_SnohomishWA"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.Id("strparcel")).SendKeys(parcelNumber.Replace("-", "").Trim());
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search Before", driver, "WA", "Snohomish");
                        driver.FindElement(By.XPath("//*[@id='srchbar']/button")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search After", driver, "WA", "Snohomish");
                        driver.SwitchTo().Window(driver.WindowHandles.Last());
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("mAccountSearchLabel")).Text;
                            if (nodata.Contains("does not exist"))
                            {
                                HttpContext.Current.Session["Nodata_SnohomishWA"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    //Property Details
                    driver.SwitchTo().DefaultContent();
                    Thread.Sleep(3000);
                    string propertyAddress = "", propertyDecription = "", propertyTaxCode = "", propertyUseCode = "", yearBuilt = "", strownerName = "", TaxableValue = "", ExemptionAmount = "", AssessedValue = "", TaxYear1 = "", TaxYear2 = "", TaxYear3 = "", ActiveExemption = "", MarketTotal = "", MarketLand = "", MarketImprovement = "", PersonalProperty = "";
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='mGrid']/tbody/tr[2]/td[1]/a")).Click();
                        Thread.Sleep(2000);
                    }
                    catch { }
                    parcelNumber = driver.FindElement(By.Id("mParcelNumber")).Text;
                    gc.CreatePdf(orderNumber, parcelNumber, "search Result", driver, "WA", "Snohomish");
                    propertyAddress = driver.FindElement(By.Id("mSitusAddress")).Text;
                    IWebElement IProDescription = driver.FindElement(By.Id("mGeneralInformation"));
                    IList<IWebElement> IProDescRow = IProDescription.FindElements(By.TagName("tr"));
                    IList<IWebElement> IproDecTD;
                    foreach (IWebElement description in IProDescRow)
                    {
                        IproDecTD = description.FindElements(By.TagName("td"));
                        if (IproDecTD.Count != 0 && description.Text.Contains("Property Description"))
                        {
                            propertyDecription = IproDecTD[1].Text;
                        }
                        if (IproDecTD.Count != 0 && description.Text.Contains("Tax Code Area"))
                        {
                            propertyTaxCode = IproDecTD[1].Text;
                        }
                    }
                    IWebElement IProUseCode = driver.FindElement(By.Id("mPropertyCharacteristics"));
                    IList<IWebElement> IProUseRow = IProUseCode.FindElements(By.TagName("tr"));
                    IList<IWebElement> IproUseTD;
                    foreach (IWebElement useCode in IProUseRow)
                    {
                        IproDecTD = useCode.FindElements(By.TagName("td"));
                        if (IproDecTD.Count != 0 && useCode.Text.Contains("Use Code"))
                        {
                            propertyUseCode = IproDecTD[1].Text;
                        }
                    }
                    try
                    {
                        IWebElement IYearBuilt = driver.FindElement(By.Id("mRealPropertyStructures"));
                        IList<IWebElement> IYearBulitRow = IYearBuilt.FindElements(By.TagName("tr"));
                        IList<IWebElement> IYearBuiltTD;
                        foreach (IWebElement year in IYearBulitRow)
                        {
                            IYearBuiltTD = year.FindElements(By.TagName("td"));
                            if (IYearBuiltTD.Count != 0)
                            {
                                yearBuilt = IYearBuiltTD[2].Text;
                            }
                        }
                    }
                    catch { }

                    IWebElement IOwnerName = driver.FindElement(By.Id("mParties"));
                    IList<IWebElement> IOwnerNameRow = IOwnerName.FindElements(By.TagName("tr"));
                    IList<IWebElement> IOwnerNameTD;
                    foreach (IWebElement owner in IOwnerNameRow)
                    {
                        IOwnerNameTD = owner.FindElements(By.TagName("td"));
                        if (IOwnerNameTD.Count != 0 && !owner.Text.Contains("Role") && owner.Text.Contains("Owner"))
                        {
                            strownerName = IOwnerNameTD[2].Text;
                        }
                    }
                    string PropertyDetails = propertyAddress + "~" + strownerName + "~" + propertyDecription + "~" + propertyTaxCode + "~" + propertyUseCode + "~" + yearBuilt + "~" + Taxauthority;
                    gc.insert_date(orderNumber, parcelNumber, 1391, PropertyDetails, 1, DateTime.Now);

                    IWebElement IValues = driver.FindElement(By.Id("mPropertyValues"));
                    IList<IWebElement> IValuesRow = IValues.FindElements(By.TagName("tr"));
                    IList<IWebElement> IValuesTD;
                    IList<IWebElement> IValuesTH;
                    foreach (IWebElement value in IValuesRow)
                    {
                        IValuesTD = value.FindElements(By.TagName("td"));
                        IValuesTH = value.FindElements(By.TagName("th"));
                        if (IValuesTH.Count != 0 && value.Text.Contains("Tax Year"))
                        {
                            TaxYear1 = IValuesTH[1].Text.Replace("Tax Year", "").Replace("\r\n", "").Trim() + "~";
                            TaxYear2 = IValuesTH[2].Text.Replace("Tax Year", "").Replace("\r\n", "").Trim() + "~";
                            TaxYear3 = IValuesTH[3].Text.Replace("Tax Year", "").Replace("\r\n", "").Trim() + "~";
                        }
                        if (IValuesTD.Count != 0 && value.Text.Contains("Taxable Value Regular"))
                        {
                            TaxYear1 += IValuesTD[1].Text + "~";
                            TaxYear2 += IValuesTD[2].Text + "~";
                            TaxYear3 += IValuesTD[3].Text + "~";
                        }
                        if (IValuesTD.Count != 0 && value.Text.Contains("Exemption Amount Regular"))
                        {
                            TaxYear1 += IValuesTD[1].Text + "~";
                            TaxYear2 += IValuesTD[2].Text + "~";
                            TaxYear3 += IValuesTD[3].Text + "~";
                        }
                        if (IValuesTD.Count != 0 && value.Text.Contains("Market Total"))
                        {
                            TaxYear1 += IValuesTD[1].Text + "~";
                            TaxYear2 += IValuesTD[2].Text + "~";
                            TaxYear3 += IValuesTD[3].Text + "~";
                        }
                        if (IValuesTD.Count != 0 && value.Text.Contains("Assessed Value"))
                        {
                            TaxYear1 += IValuesTD[1].Text + "~";
                            TaxYear2 += IValuesTD[2].Text + "~";
                            TaxYear3 += IValuesTD[3].Text + "~";
                        }
                        if (IValuesTD.Count != 0 && value.Text.Contains("Market Land"))
                        {
                            TaxYear1 += IValuesTD[1].Text + "~";
                            TaxYear2 += IValuesTD[2].Text + "~";
                            TaxYear3 += IValuesTD[3].Text + "~";
                        }
                        if (IValuesTD.Count != 0 && value.Text.Contains("Market Improvement"))
                        {
                            TaxYear1 += IValuesTD[1].Text + "~";
                            TaxYear2 += IValuesTD[2].Text + "~";
                            TaxYear3 += IValuesTD[3].Text + "~";
                        }
                        if (IValuesTD.Count != 0 && value.Text.Contains("Personal Property"))
                        {
                            TaxYear1 += IValuesTD[1].Text + "~";
                            TaxYear2 += IValuesTD[2].Text + "~";
                            TaxYear3 += IValuesTD[3].Text + "~";
                        }
                    }

                    IWebElement IExempet = driver.FindElement(By.Id("mActiveExemptions"));
                    IList<IWebElement> IExempetRow = IExempet.FindElements(By.TagName("tr"));
                    IList<IWebElement> IExempetTD;
                    foreach (IWebElement value in IExempetRow)
                    {
                        IExempetTD = value.FindElements(By.TagName("td"));
                        if (IExempetTD.Count != 0)
                        {
                            ActiveExemption = IExempetTD[0].Text;
                        }
                    }
                    //string assessDetails = TaxYear + "~" + TaxableValue + "~" + ExemptionAmount + "~" + MarketTotal + "~" + AssessedValue + "~" + MarketLand + "~" + MarketImprovement + "~" + PersonalProperty + "~" + ActiveExemption;
                    //gc.insert_date(orderNumber, parcelNumber, 1392, assessDetails, 1, DateTime.Now);
                    gc.insert_date(orderNumber, parcelNumber, 1392, TaxYear1 + ActiveExemption, 1, DateTime.Now);
                    gc.insert_date(orderNumber, parcelNumber, 1392, TaxYear2, 1, DateTime.Now);
                    gc.insert_date(orderNumber, parcelNumber, 1392, TaxYear3, 1, DateTime.Now);

                    //driver.Navigate().GoToUrl("https://www.snoco.org/proptax/(S(nlptehf3sx30hqqqizw4t1g1))/default.aspx");
                    //driver.FindElement(By.Id("mParcelID")).SendKeys(parcelNumber.Replace("-", "").Trim());
                    //gc.CreatePdf(orderNumber, parcelNumber, "Tax search", driver, "WA", "Snohomish");
                    //driver.FindElement(By.Id("mSubmit")).SendKeys(Keys.Enter);
                    //gc.CreatePdf(orderNumber, parcelNumber, "Tax search Result", driver, "WA", "Snohomish");
                    string TaxAuthority = "";
                    try
                    {
                        IWebElement ITaxAuthority = driver.FindElement(By.Id("mPaymentMessage"));
                        TaxAuthority = GlobalClass.After(ITaxAuthority.Text, "Send to ").Trim();
                    }
                    catch { }

                    IWebElement IDistribution = driver.FindElement(By.Id("mCurrentTaxesDistribution"));
                    IList<IWebElement> IDistributionRow = IDistribution.FindElements(By.TagName("tr"));
                    IList<IWebElement> IDistributionTD;
                    foreach (IWebElement distribution in IDistributionRow)
                    {
                        IDistributionTD = distribution.FindElements(By.TagName("td"));
                        if (IDistributionTD.Count != 0)
                        {
                            string DistributionDetails = IDistributionTD[0].Text + "~" + IDistributionTD[1].Text + "~" + IDistributionTD[2].Text + "~" + IDistributionTD[3].Text + "~" + IDistributionTD[4].Text;
                            gc.insert_date(orderNumber, parcelNumber, 1394, DistributionDetails, 1, DateTime.Now);
                        }
                    }
                    IWebElement IReceipt = driver.FindElement(By.Id("mReceipts"));
                    IList<IWebElement> IReceiptRow = IReceipt.FindElements(By.TagName("tr"));
                    IList<IWebElement> IReceiptTD;
                    foreach (IWebElement receipt in IReceiptRow)
                    {
                        IReceiptTD = receipt.FindElements(By.TagName("td"));
                        if (IReceiptTD.Count != 0)
                        {
                            string ReceiptDetails = IReceiptTD[0].Text + "~" + IReceiptTD[1].Text + "~" + IReceiptTD[2].Text + "~" + IReceiptTD[3].Text;
                            gc.insert_date(orderNumber, parcelNumber, 1393, ReceiptDetails, 1, DateTime.Now);
                        }
                    }
                    ////Installments and Charges Details
                    try
                    {
                        IWebElement IInstallment = driver.FindElement(By.Id("mTaxChargesBalancePayment"));
                        IList<IWebElement> IInstallmentRow = IInstallment.FindElements(By.TagName("tr"));
                        IList<IWebElement> IInstallmentTD;
                        foreach (IWebElement install in IInstallmentRow)
                        {
                            IInstallmentTD = install.FindElements(By.TagName("td"));
                            if (IInstallmentTD.Count != 0)
                            {
                                string InstallmentDetails = IInstallmentTD[0].Text + "~" + IInstallmentTD[1].Text + "~" + IInstallmentTD[2].Text + "~" + IInstallmentTD[3].Text + "~" + IInstallmentTD[4].Text + "~" + IInstallmentTD[5].Text + "~" + IInstallmentTD[6].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1592, InstallmentDetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }
                    //Levy Rate History Table
                    IWebElement ILevyrate = driver.FindElement(By.Id("LevyRateHistoryPanel"));
                    IList<IWebElement> ILevyrateRow = ILevyrate.FindElements(By.TagName("tr"));
                    IList<IWebElement> ILevyrateTD;
                    foreach (IWebElement Levyrate in ILevyrateRow)
                    {
                        ILevyrateTD = Levyrate.FindElements(By.TagName("td"));
                        if (ILevyrateTD.Count != 0 && ILevyrateTD.Count == 2 && !Levyrate.Text.Contains("Levy Rate History") && !Levyrate.Text.Contains("Tax Year Total Levy Rate"))
                        {
                            string LevyrateDetails = ILevyrateTD[0].Text + "~" + ILevyrateTD[1].Text;
                            gc.insert_date(orderNumber, parcelNumber, 1591, LevyrateDetails, 1, DateTime.Now);

                        }
                    }
                    //Good Through Details
                    try
                    {
                        List<string> billinfo = new List<string>();
                        string Bill_Flag = driver.FindElement(By.XPath("//*[@id='mTaxChargesBalancePayment']/tbody/tr[2]/td[5]")).Text;
                        string Good_through_date = "";
                        if (Bill_Flag != "$0.00")
                        {

                            IWebElement href = driver.FindElement(By.Id("mFuturePayoff"));
                            string addview = href.GetAttribute("href");
                            driver.Navigate().GoToUrl(addview);
                            gc.CreatePdf(orderNumber, parcelNumber, "Calculate Future Payoff", driver, "WA", "Snohomish");

                            IWebElement good_date = driver.FindElement(By.Id("mDate"));
                            Good_through_date = good_date.GetAttribute("value");

                            DateTime G_Date = Convert.ToDateTime(Good_through_date);
                            string dateChecking = DateTime.Now.ToString("MM") + "/15/" + DateTime.Now.ToString("yyyy");

                            if (G_Date < Convert.ToDateTime(dateChecking))
                            {
                                //end of the month
                                Good_through_date = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")))).ToString("MM/dd/yyyy");

                            }

                            else if (G_Date > Convert.ToDateTime(dateChecking))
                            {
                                // nextEndOfMonth 
                                if ((Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))) < 12))
                                {
                                    Good_through_date = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM")) + 1), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")) + 1)).ToString("MM/dd/yyyy");
                                }
                                else
                                {
                                    int nxtYr = Convert.ToInt16(DateTime.Now.ToString("yyyy")) + 1;
                                    Good_through_date = new DateTime(nxtYr, 1, DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), 1)).ToString("MM/dd/yyyy");

                                }
                            }
                            driver.FindElement(By.Id("mDate")).Clear();
                            driver.FindElement(By.XPath("//*[@id='mDate']")).SendKeys(Good_through_date);
                            driver.FindElement(By.Id("mCalculate")).SendKeys(Keys.Enter);
                            gc.CreatePdf(orderNumber, parcelNumber, "Calculate Future Payoff1", driver, "WA", "Snohomish");

                            string asofdate = "", Principal = "", Interestandpenalties = "", Totaldue = "";
                            asofdate = driver.FindElement(By.Id("mDisplayDate")).Text;

                            IWebElement TaxInfoTB = driver.FindElement(By.Id("mGrid"));
                            IList<IWebElement> TaxInfoTR = TaxInfoTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> TaxInfoTD;

                            foreach (IWebElement TaxInfo in TaxInfoTR)
                            {
                                TaxInfoTD = TaxInfo.FindElements(By.TagName("td"));
                                if (TaxInfoTD.Count != 0 && TaxInfoTD.Count == 3 && !TaxInfo.Text.Contains("Principal"))
                                {
                                    Principal = TaxInfoTD[0].Text;
                                    Interestandpenalties = TaxInfoTD[1].Text;
                                    Totaldue = TaxInfoTD[2].Text;


                                    string TaxdelinqInfo_Details = asofdate.Trim() + "~" + Principal.Trim() + "~" + Interestandpenalties.Trim() + "~" + Totaldue.Trim();
                                    gc.insert_date(orderNumber, parcelNumber, 1484, TaxdelinqInfo_Details, 1, DateTime.Now);
                                }

                            }
                        }
                    }
                    catch { }
                    try
                    {
                        IWebElement Backtoproperty = driver.FindElement(By.XPath("//*[@id='mMainHeader_SiteMapPath1']/span[5]/a"));
                        Backtoproperty.Click();
                        Thread.Sleep(2000);
                    }
                    catch { }
                    try
                    {
                        IWebElement viewdetailsclick = driver.FindElement(By.Id("mDetailedStatement"));
                        viewdetailsclick.Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "View Details", driver, "WA", "Snohomish");
                    }
                    catch { }
                    try
                    {
                        IWebElement Backtoproperty = driver.FindElement(By.XPath("//*[@id='mPageHeader_SiteMapPath1']/span[5]/a"));
                        Backtoproperty.Click();
                        Thread.Sleep(2000);
                        //driver.Navigate().Back();
                    }
                    catch { }
                    //try
                    //{
                    //    IWebElement IUIDChrges = driver.FindElement(By.Id("mULID"));
                    //    IUIDChrges.Click();
                    //    gc.CreatePdf(orderNumber, parcelNumber, "Tax Charges", driver, "WA", "Snohomish");
                    //    TaxAuthority = GlobalClass.After(IUIDChrges.Text, "Send to ").Trim();
                    //    IWebElement ICharges = driver.FindElement(By.Id("mTaxChargesBalancePayment"));
                    //    IList<IWebElement> IChargesRow = ICharges.FindElements(By.TagName("tr"));
                    //    IList<IWebElement> IChargesTD;
                    //    foreach (IWebElement charges in IChargesRow)
                    //    {
                    //        IChargesTD = charges.FindElements(By.TagName("td"));
                    //        if (IChargesTD.Count != 0)
                    //        {
                    //            string ChargesDetails = IChargesTD[0].Text + "~" + IChargesTD[1].Text + "~" + IChargesTD[2].Text + "~" + IChargesTD[3].Text + "~" + IChargesTD[4].Text + "~" + IChargesTD[5].Text + "~" + IChargesTD[6].Text + "~" + TaxAuthority;
                    //            gc.insert_date(orderNumber, parcelNumber, 1395, ChargesDetails, 1, DateTime.Now);
                    //        }
                    //    }
                    //}
                    //catch { }

                    //Tax Balance Details
                    try
                    {
                        driver.Navigate().GoToUrl("https://www.paydici.com/snohomish-county-wa/search/new");
                        Thread.Sleep(2000);
                        driver.FindElement(By.Id("q")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Balace Result1", driver, "WA", "Snohomish");
                        driver.FindElement(By.XPath("//*[@id='main']/div[3]/div/div[5]/div/form/input[2]")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Balace Result2", driver, "WA", "Snohomish");

                        try
                        {
                            IWebElement IAddressSearch1 = driver.FindElement(By.Id("bill_group_" + parcelNumber));
                            IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                            js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                            Thread.Sleep(9000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Tax Balace Result3", driver, "WA", "Snohomish");
                        }
                        catch { }
                        string Taxyear = "", Amountdue = "", Total = "";
                        IWebElement taxbal = driver.FindElement(By.XPath("//*[@id='main']/div[3]/div/div[2]/div[2]/div[1]/div/table/tbody"));
                        IList<IWebElement> taxbalRow = taxbal.FindElements(By.TagName("li"));
                        IList<IWebElement> taxbalTDstrong;
                        IList<IWebElement> taxbalTDspan;
                        foreach (IWebElement taxbal1 in taxbalRow)
                        {
                            taxbalTDstrong = taxbal1.FindElements(By.TagName("strong"));
                            if (taxbalTDstrong.Count != 0 && taxbal1.Text.Contains("Property Tax"))
                            {
                                Taxyear = taxbalTDstrong[0].Text.Replace("Property Tax", "").Trim();
                            }

                            taxbalTDspan = taxbal1.FindElements(By.TagName("span"));
                            if (taxbalTDspan.Count != 0 && taxbal1.Text.Contains("Minimum Amount Due"))
                            {

                                Amountdue = taxbalTDspan[0].Text.Replace("Minimum Amount Due", "").Trim().Replace("(", "").Replace(")", "");
                                Total = taxbalTDspan[1].Text.Replace("Total (", "").Trim().Replace(")", "").Trim();

                                string Taxbalacedetails = Taxyear + "~" + Amountdue + "~" + Total;
                                gc.insert_date(orderNumber, parcelNumber, 1395, Taxbalacedetails, 1, DateTime.Now);
                                Taxyear = ""; Amountdue = ""; Total = "";
                            }
                        }
                    }
                    catch { }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "WA", "Snohomish", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    driver.Quit();
                    gc.mergpdf(orderNumber, "WA", "Snohomish");
                    return "Data Inserted Successfully";
                }

                catch (Exception ex)
                {
                    driver.Quit();
                    throw ex;
                }
            }

        }

        private void multiparcel(string orderNumber, string address)
        {
            string multi = GlobalClass.Before(driver.FindElement(By.Id("mMessage")).Text, " records returned from your search input.").Trim();
            string strAddress = "", strparcel = "", strOwner = "";
            List<string> searchlist = new List<string>();
            IWebElement tbmulti = driver.FindElement(By.Id("mGrid"));
            IList<IWebElement> TBmulti = tbmulti.FindElements(By.TagName("tr"));
            IList<IWebElement> TRmulti;
            IList<IWebElement> TDmulti;

            foreach (IWebElement row in TBmulti)
            {
                TDmulti = row.FindElements(By.TagName("td"));

                if (Convert.ToInt16(multi) > 1 && TDmulti.Count != 0 && TDmulti[0].Text.Trim() != "")
                {
                    if (TDmulti[0].Text.Trim() != "" & row.Text.Trim().Contains(address.ToUpper().Trim()))
                    {
                        strOwner = TDmulti[1].Text; strAddress = TDmulti[2].Text; strparcel = TDmulti[0].Text;
                        searchlist.Add(TDmulti[0].Text);
                        searchcount = searchlist.Count;
                        string multiDetails = strOwner + "~" + strAddress;
                        gc.insert_date(orderNumber, strparcel, 1390, multiDetails, 1, DateTime.Now);
                    }

                }
            }
        }
    }
}