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
    public class WebDriver_TXGuadalupe
    {
        string ParcelID = "", OwnerName = "", PropertyAddress = "", PropertyStatus = "", PropertyType = "", LegalDescription = "", Neighborhood = "", Account = "", MapNumber = "", OwnerID = "", Exemptions = "", PercentOwnership = "", MailingAddress = "", property_details = "";
        string Year = "", ImprovementHomesiteValue = "", ImprovementNonHomesite = "", TotalImprovementMarketValue = "", LandHomesiteValue = "", LandNonHomesite = "", LandAgriculturalMarketValue = "", TotalLandMarketValue = "", TotalMarketValue = "", AgriculturalUse = "", TotalAppraisedValue = "", HomesteadCapLoss = "", TotalAssessedValue = "", assessment_details = "", EntityDetails = "", AssessmentHistory = "";
        string goodthrough = "", CurrentAmountDue = "", PastYearsDue = "", TotalDue = "", TaxingAuthority = "307 W Court St., Seguin TX 78155 PHONE: 830-379-2315", taxinfo = "";
        string dateChecking = "", date = "", levynew = "", levy = "", PI = "", Att_fee = "", credits = "", year = "", TaxingEntity = "", TotalTaxes = "", PaidDate = "", Balance = "", PaidAmount = "", taxdistri = "", taxdistri1 = "", taxdet = "";
        string taxHistory = "", year1 = "", fileName = "";

        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_TXGuadalupe(string streetno, string direction, string streetname, string city, string streettype, string unitnumber, string ownernm, string parcelNumber, string searchType, string orderNumber, string Account_id)
        {
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
                        string titleaddress = streetno + " " + streetname + " " + unitnumber;
                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", titleaddress, "TX", "Guadalupe");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_TXGuadalupe"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("http://property.co.guadalupe.tx.us/");
                        Thread.Sleep(3000);

                        string Adderss = streetno + " " + streetname;
                        driver.FindElement(By.Id("SearchText")).SendKeys(Adderss);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Address Search", driver, "TX", "Guadalupe");
                        driver.FindElement(By.Id("dnn_PropertySearch_SearchButtonDiv")).Click();
                        Thread.Sleep(2000);
                    }

                    if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("http://property.co.guadalupe.tx.us/");
                        Thread.Sleep(2000);

                        driver.FindElement(By.Id("SearchText")).SendKeys(parcelNumber);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "TX", "Guadalupe");
                        driver.FindElement(By.Id("dnn_PropertySearch_SearchButtonDiv")).Click();
                        Thread.Sleep(2000);
                    }

                    if (searchType == "ownername")
                    {
                        driver.Navigate().GoToUrl("http://property.co.guadalupe.tx.us/");
                        Thread.Sleep(2000);

                        driver.FindElement(By.Id("SearchText")).SendKeys(ownernm);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Owner Search", driver, "TX", "Guadalupe");
                        driver.FindElement(By.Id("dnn_PropertySearch_SearchButtonDiv")).Click();
                        Thread.Sleep(2000);
                    }

                    else if (searchType == "block")
                    {
                        driver.Navigate().GoToUrl("http://property.co.guadalupe.tx.us/");
                        Thread.Sleep(2000);

                        driver.FindElement(By.Id("SearchText")).SendKeys(Account_id);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Account Search", driver, "TX", "Guadalupe");
                        driver.FindElement(By.Id("dnn_PropertySearch_SearchButtonDiv")).Click();
                        Thread.Sleep(2000);
                    }

                    try
                    {
                        int trCount = driver.FindElements(By.XPath("//*[@id='grid']/div[2]/table/tbody/tr")).Count;
                        if (trCount > 1)
                        {
                            int maxCheck = 0;
                            IWebElement tbmulti = driver.FindElement(By.XPath("//*[@id='grid']/div[2]/table/tbody"));
                            gc.CreatePdf_Chrome(orderNumber, parcelNumber, "Multi Address Search", driver, "TX", "Guadalupe");
                            IList<IWebElement> TRmulti5 = tbmulti.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDmulti5;
                            foreach (IWebElement row in TRmulti5)
                            {
                                if (maxCheck <= 25)
                                {
                                    TDmulti5 = row.FindElements(By.TagName("td"));
                                    if (TDmulti5.Count != 0)
                                    {
                                        string multi1 = TDmulti5[1].Text + "~" + TDmulti5[3].Text + "~" + TDmulti5[4].Text;
                                        gc.insert_date(orderNumber, TDmulti5[0].Text, 1025, multi1, 1, DateTime.Now);
                                    }
                                    maxCheck++;
                                }
                            }
                            if (TRmulti5.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_TXGuadalupe_Multicount"] = "Maximum";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiParcel_TXGuadalupe"] = "Yes";
                            }

                            driver.Quit();
                            return "MultiParcel";
                        }
                        else
                        {
                            driver.FindElement(By.XPath("//*[@id='grid']/div[2]/table/tbody/tr/td[1]")).Click();
                            Thread.Sleep(2000);
                        }
                    }
                    catch { }

                    try
                    {
                        //No Data Found
                        string nodata = driver.FindElement(By.XPath("//*[@id='grid']/div[2]/table")).Text;
                        if (nodata.Contains("No properties found"))
                        {
                            HttpContext.Current.Session["Nodata_TXGuadalupe"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    //property details              
                    try
                    {
                        driver.FindElement(By.Id("tabDetails")).Click();
                        Thread.Sleep(2000);

                        ParcelID = driver.FindElement(By.Id("dnn_ctr368_View_tdPropertyID")).Text;
                        gc.CreatePdf(orderNumber, ParcelID, "Property details", driver, "TX", "Guadalupe");
                        OwnerName = driver.FindElement(By.Id("dnn_ctr368_View_tdOwnerName")).Text;
                        PropertyAddress = driver.FindElement(By.Id("dnn_ctr368_View_tdPropertyAddress")).Text;
                        PropertyStatus = driver.FindElement(By.Id("dnn_ctr368_View_tdGIPropertyStatus")).Text;
                        PropertyType = driver.FindElement(By.Id("dnn_ctr368_View_tdGIPropertyType")).Text;
                        LegalDescription = driver.FindElement(By.Id("dnn_ctr368_View_tdGILegalDescription")).Text;
                        Neighborhood = driver.FindElement(By.Id("dnn_ctr368_View_tdGINeighborhood")).Text;
                        Account = driver.FindElement(By.Id("dnn_ctr368_View_tdGIAccount")).Text;
                        MapNumber = driver.FindElement(By.Id("dnn_ctr368_View_tdGIMapNumber")).Text;
                        OwnerID = driver.FindElement(By.Id("dnn_ctr368_View_tdOIPartyQuickRefID")).Text;
                        Exemptions = driver.FindElement(By.Id("dnn_ctr368_View_tdOIExemptions")).Text;
                        PercentOwnership = driver.FindElement(By.Id("dnn_ctr368_View_tdOIPercentOwnership")).Text;
                        MailingAddress = driver.FindElement(By.Id("dnn_ctr368_View_tdOIMailingAddress")).Text;

                        property_details = OwnerName + "~" + PropertyAddress + "~" + PropertyStatus + "~" + PropertyType + "~" + LegalDescription + "~" + Neighborhood + "~" + Account + "~" + MapNumber + "~" + OwnerID + "~" + Exemptions + "~" + PercentOwnership + "~" + MailingAddress;
                        gc.insert_date(orderNumber, ParcelID, 1026, property_details, 1, DateTime.Now);
                    }
                    catch
                    { }

                    //Assessment Details                           
                    try
                    {
                        Year = driver.FindElement(By.Id("dnn_ctr368_View_tdGITitle")).Text;
                        Year = WebDriverTest.Before(Year, " GENERAL INFORMATION");
                        ImprovementHomesiteValue = driver.FindElement(By.Id("dnn_ctr368_View_tdVIImprovementHS")).Text;
                        ImprovementNonHomesite = driver.FindElement(By.Id("dnn_ctr368_View_tdVIImprovementNonHS")).Text;
                        TotalImprovementMarketValue = driver.FindElement(By.Id("dnn_ctr368_View_tdVITotalImprovementMV")).Text;
                        LandHomesiteValue = driver.FindElement(By.Id("dnn_ctr368_View_tdVILandHS")).Text;
                        LandNonHomesite = driver.FindElement(By.Id("dnn_ctr368_View_tdVILandNonHS")).Text;
                        LandAgriculturalMarketValue = driver.FindElement(By.Id("dnn_ctr368_View_tdVILandAgMV")).Text;
                        TotalLandMarketValue = driver.FindElement(By.Id("dnn_ctr368_View_tdVITotalLandMV")).Text;
                        TotalMarketValue = driver.FindElement(By.Id("dnn_ctr368_View_tdVITotalMV")).Text;
                        AgriculturalUse = driver.FindElement(By.Id("dnn_ctr368_View_tdVIAgUse")).Text;
                        TotalAppraisedValue = driver.FindElement(By.Id("dnn_ctr368_View_tdVITotalAppraisedValue")).Text;
                        HomesteadCapLoss = driver.FindElement(By.Id("dnn_ctr368_View_tdVIHomesteadCapLoss")).Text;
                        TotalAssessedValue = driver.FindElement(By.Id("dnn_ctr368_View_tdVITotalAssessedValueRP")).Text;

                        assessment_details = Year + "~" + ImprovementHomesiteValue + "~" + ImprovementNonHomesite + "~" + TotalImprovementMarketValue + "~" + LandHomesiteValue + "~" + LandNonHomesite + "~" + LandAgriculturalMarketValue + "~" + TotalLandMarketValue + "~" + TotalMarketValue + "~" + AgriculturalUse + "~" + TotalAppraisedValue + "~" + HomesteadCapLoss + "~" + TotalAssessedValue;
                        gc.insert_date(orderNumber, ParcelID, 1027, assessment_details, 1, DateTime.Now);
                    }
                    catch
                    { }

                    //Entity Details
                    try
                    {
                        IWebElement EntityTable = driver.FindElement(By.XPath("//*[@id='dnn_ctr368_View_divEntitiesAndExemptionsData']/table/tbody"));
                        IList<IWebElement> EntityTR = EntityTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> EntityTD;
                        foreach (IWebElement Entity in EntityTR)
                        {
                            if (!Entity.Text.Contains("TAXING ENTITY"))
                            {
                                EntityTD = Entity.FindElements(By.TagName("td"));
                                if (EntityTD.Count != 0)
                                {
                                    EntityDetails = EntityTD[0].Text.Trim() + "~" + EntityTD[1].Text.Trim() + "~" + EntityTD[2].Text.Trim() + "~" + EntityTD[3].Text.Trim() + "~" + EntityTD[4].Text.Trim() + "~" + EntityTD[5].Text.Trim();
                                    gc.insert_date(orderNumber, ParcelID, 1028, EntityDetails, 1, DateTime.Now);

                                }

                            }
                        }
                    }
                    catch { }

                    //Assessment History Details
                    try
                    {
                        IWebElement AssementHistoryTable = driver.FindElement(By.XPath("//*[@id='dnn_ctr368_View_tblValueHistoryDataRP']/tbody"));
                        IList<IWebElement> AssementHistoryTR = AssementHistoryTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> AssementHistoryTD;
                        foreach (IWebElement AssementHistory in AssementHistoryTR)
                        {
                            if (!AssementHistory.Text.Contains("YEAR"))
                            {
                                AssementHistoryTD = AssementHistory.FindElements(By.TagName("td"));
                                if (AssementHistoryTD.Count != 0)
                                {
                                    AssessmentHistory = AssementHistoryTD[0].Text.Trim() + "~" + AssementHistoryTD[1].Text.Trim() + "~" + AssementHistoryTD[2].Text.Trim() + "~" + AssementHistoryTD[3].Text.Trim() + "~" + AssementHistoryTD[4].Text.Trim() + "~" + AssementHistoryTD[5].Text.Trim() + "~" + AssementHistoryTD[6].Text.Trim() + "~" + AssementHistoryTD[7].Text.Trim() + "~" + AssementHistoryTD[8].Text.Trim();
                                    gc.insert_date(orderNumber, ParcelID, 1029, AssessmentHistory, 1, DateTime.Now);

                                }

                            }
                        }
                    }
                    catch { }

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    //Tax Distribution Details
                    driver.FindElement(By.Id("tabBills")).Click();
                    Thread.Sleep(2000);
                    try
                    {
                        int Ulcount = driver.FindElements(By.XPath("//*[@id='dnn_ctr368_View_divBillDetails']/div")).Count;

                        for (int i = 1; i <= Ulcount; i++)
                        {
                            driver.FindElement(By.XPath("//*[@id='dnn_ctr368_View_divBillDetails']/div[" + i + "]/table[1]/tbody/tr/td[3]")).Click();
                            Thread.Sleep(2000);
                            year = driver.FindElement(By.XPath("//*[@id='dnn_ctr368_View_divBillDetails']/div[" + i + "]/table[1]/tbody/tr/td[1]")).Text;

                            IWebElement TaxDistributionTable = driver.FindElement(By.XPath("//*[@id='dnn_ctr368_View_divBillDetails']/div[" + i + "]/table[2]/tbody"));
                            IList<IWebElement> TaxDistributionTR = TaxDistributionTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> TaxDistributionTD;
                            foreach (IWebElement TaxDistribution in TaxDistributionTR)
                            {
                                if (!TaxDistribution.Text.Contains("TAXING ENTITY"))
                                {
                                    TaxDistributionTD = TaxDistribution.FindElements(By.TagName("td"));
                                    if (TaxDistributionTD.Count == 5 && TaxDistributionTD[0].Text.Trim() != "")
                                    {
                                        TaxingEntity = TaxDistributionTD[0].Text.Trim();
                                        TotalTaxes = TaxDistributionTD[1].Text.Trim();
                                        PaidDate = TaxDistributionTD[2].Text.Trim();
                                        PaidAmount = TaxDistributionTD[3].Text.Trim();
                                        Balance = TaxDistributionTD[4].Text.Trim();
                                    }
                                    if (TaxDistribution.Text.Contains("Levy") && TaxDistributionTD.Count == 13)
                                    {
                                        taxdet = TaxDistributionTD[1].Text.Replace("\r\n", "~");
                                        string[] words = taxdet.Split('~');
                                        levy = words[0].Replace("Levy", "");

                                        PI = words[1].Replace("P&I", "");
                                        if (i == 1)
                                        {
                                            levynew = PI;
                                        }
                                        Att_fee = words[2].Replace("Att. Fee", "");
                                        credits = words[3].Replace("Credits/Disc.", "");
                                        taxdistri = year + "~" + TaxingEntity + "~" + levy + "~" + PI + "~" + Att_fee + "~" + credits + "~" + TotalTaxes + "~" + PaidDate + "~" + PaidAmount + "~" + Balance;
                                        gc.insert_date(orderNumber, ParcelID, 1030, taxdistri, 1, DateTime.Now);
                                    }
                                    if (TaxDistribution.Text.Contains("TOTALS"))
                                    {
                                        taxdistri1 = year + "~" + TaxingEntity + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + TotalTaxes + "~" + "" + "~" + PaidAmount + "~" + Balance;
                                        gc.insert_date(orderNumber, ParcelID, 1030, taxdistri1, 1, DateTime.Now);
                                    }
                                }
                            }
                        }
                    }
                    catch { }

                    try
                    {
                        if (!levynew.Contains("$0.00"))
                        {
                            IWebElement dt = driver.FindElement(By.Id("effectiveDatePicker"));
                            date = dt.GetAttribute("value");

                            DateTime G_Date = Convert.ToDateTime(date);
                            dateChecking = DateTime.Now.ToString("MM") + "/15/" + DateTime.Now.ToString("yyyy");

                            if (G_Date < Convert.ToDateTime(dateChecking))
                            {
                                //end of the month
                                date = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")))).ToString("MM/dd/yyyy");
                            }
                            else if (G_Date > Convert.ToDateTime(dateChecking))
                            {
                                // nextEndOfMonth 
                                if ((Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))) < 12))
                                {
                                    date = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM")) + 1), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")) + 1)).ToString("MM/dd/yyyy");
                                }
                                else
                                {
                                    int nxtYr = Convert.ToInt16(DateTime.Now.ToString("yyyy")) + 1;
                                    date = new DateTime(nxtYr, 1, DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), 1)).ToString("MM/dd/yyyy");

                                }
                            }
                            Thread.Sleep(2000);
                            dt.Clear();
                            driver.FindElement(By.Id("effectiveDatePicker")).SendKeys(date);
                        }
                        gc.CreatePdf(orderNumber, ParcelID, "Tax Distribution", driver, "TX", "Guadalupe");

                        IWebElement dt1 = driver.FindElement(By.Id("effectiveDatePicker"));
                        goodthrough = dt1.GetAttribute("value");

                        CurrentAmountDue = driver.FindElement(By.Id("dnn_ctr368_View_tdPMCurrentAmountDue")).Text;
                        PastYearsDue = driver.FindElement(By.Id("dnn_ctr368_View_tdPMPastYearsDue")).Text;
                        TotalDue = driver.FindElement(By.Id("dnn_ctr368_View_tdPMTotalDue")).Text;
                        taxinfo = goodthrough + "~" + CurrentAmountDue + "~" + PastYearsDue + "~" + TotalDue + "~" + TaxingAuthority;
                        gc.insert_date(orderNumber, ParcelID, 1031, taxinfo, 1, DateTime.Now);
                    }
                    catch
                    { }

                    //Payment History Details
                    try
                    {
                        driver.FindElement(By.Id("dnn_ctr368_View_divPaymentHistoryExpandCollapse")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, ParcelID, "Payment History", driver, "TX", "Guadalupe");

                        int licount = driver.FindElements(By.XPath(" //*[@id='dnn_ctr368_View_divPaymentHistoryInfo']/ul/li")).Count;
                        for (int j = 1; j <= licount; j++)
                        {
                            year1 = driver.FindElement(By.XPath("//*[@id='dnn_ctr368_View_divPaymentHistoryInfo']/ul/li[" + j + "]/table/tbody/tr/td[2]")).Text;
                            IWebElement PaymentHistoryTable = driver.FindElement(By.XPath("//*[@id='dnn_ctr368_View_divPaymentHistoryInfo']/ul/li[" + j + "]/div/table/tbody"));
                            IList<IWebElement> PaymentHistoryTR = PaymentHistoryTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> PaymentHistoryTD;
                            foreach (IWebElement row in PaymentHistoryTR)
                            {
                                if (!row.Text.Contains("Transaction Date"))
                                {
                                    PaymentHistoryTD = row.FindElements(By.TagName("td"));
                                    if (PaymentHistoryTD.Count == 4)
                                    {
                                        taxHistory = year1 + "~" + PaymentHistoryTD[0].Text.Trim() + "~" + PaymentHistoryTD[1].Text.Trim() + "~" + PaymentHistoryTD[2].Text.Trim() + "~" + PaymentHistoryTD[3].Text.Trim().Replace("View", "");
                                        gc.insert_date(orderNumber, ParcelID, 1032, taxHistory, 1, DateTime.Now);
                                    }
                                }
                            }
                        }
                    }
                    catch
                    { }

                    try
                    {
                        var chromeOptions = new ChromeOptions();
                        var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];
                        chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                        chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                        chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                        chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                        var chDriver = new ChromeDriver(chromeOptions);
                        chDriver.Navigate().GoToUrl(driver.Url);
                        Thread.Sleep(4000);

                        int Billcount = driver.FindElements(By.XPath("//*[@id='dnn_ctr368_View_divBillDetails']/div")).Count;
                        for (int j1 = 1; j1 <= Billcount; j1++)
                        {
                            year1 = chDriver.FindElement(By.XPath("//*[@id='dnn_ctr368_View_divBillDetails']/div[" + j1 + "]/table[1]/tbody/tr/td[1]")).Text;
                            Thread.Sleep(2000);
                            try
                            {
                                IWebElement btnclick = chDriver.FindElement(By.XPath("//*[@id='btnPrintTaxStatement" + year1 + "']"));
                                btnclick.Click();
                                Thread.Sleep(4000);

                                fileName = "download";
                                gc.AutoDownloadFileSpokane(orderNumber, ParcelID, "Guadalupe", "TX", fileName + ".pdf");
                                Thread.Sleep(6000);
                            }
                            catch
                            { }
                        }
                        chDriver.Quit();
                    }
                    catch
                    { }

                    try
                    {
                        IWebElement CurrentBill = driver.FindElement(By.XPath("//*[@id='tdIconLinks']/table/tbody/tr/td[2]/a"));
                        string CurrentTaxBill = CurrentBill.GetAttribute("href");
                        gc.downloadfile(CurrentTaxBill, orderNumber, ParcelID, "Current Tax Bill", "TX", "Guadalupe");
                    }
                    catch
                    { }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "TX", "Guadalupe", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "TX", "Guadalupe");
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