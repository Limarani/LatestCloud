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
using System.Web;

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_LubbockTX
    {
        IWebElement PaidCount;
        IWebDriver driver;
        DBconnection db = new DBconnection();
        List<string> link = new List<string>();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());


        public string FTP_Lubbock(string houseno, string sname, string stype, string unitno, string direction, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            List<string> taxinformation = new List<string>();
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())//
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("http://lubbockcad.org/");
                    Thread.Sleep(4000);
                    if (searchType == "titleflex")
                    {
                        string taddress = houseno + " " + sname + " " + stype + " " + direction + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, "", "", taddress, "TX", "Lubbock");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_LubbockTX"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        driver.FindElement(By.Id("SearchText")).SendKeys(houseno + " " + direction + " " + sname + " " + stype + " " + unitno);
                        gc.CreatePdf_WOP(orderNumber, "address search", driver, "TX", "Lubbock");
                        driver.FindElement(By.Id("dnn_PropertySearch_SearchButtonDiv")).Click();
                        Thread.Sleep(5000);
                        gc.CreatePdf_WOP(orderNumber, "address search result", driver, "TX", "Lubbock");
                    }
                    if (searchType == "block")
                    {
                        driver.FindElement(By.Id("SearchText")).SendKeys(unitno);
                        gc.CreatePdf(orderNumber, parcelNumber, "block search", driver, "TX", "Lubbock");
                        driver.FindElement(By.Id("dnn_PropertySearch_SearchButtonDiv")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "block search result", driver, "TX", "Lubbock");

                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.Id("SearchText")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search", driver, "TX", "Lubbock");
                        driver.FindElement(By.Id("dnn_PropertySearch_SearchButtonDiv")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search result", driver, "TX", "Lubbock");
                    }

                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("SearchText")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "owner search", driver, "TX", "Lubbock");
                        driver.FindElement(By.Id("dnn_PropertySearch_SearchButtonDiv")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "owner search result", driver, "TX", "Lubbock");
                    }

                    int trCount = driver.FindElements(By.XPath("//*[@id='grid']/div[2]/table/tbody/tr")).Count;
                    if (trCount > 1)
                    {
                        int maxCheck = 0;
                        IWebElement tbmulti = driver.FindElement(By.XPath("//*[@id='grid']/div[2]/table/tbody"));
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
                                    gc.insert_date(orderNumber, TDmulti5[0].Text, 800, multi1, 1, DateTime.Now);
                                }
                                maxCheck++;
                            }
                        }
                        if (TRmulti5.Count > 25)
                        {
                            HttpContext.Current.Session["multiParcel_Lubbock_Multicount"] = "Maximum";
                        }
                        else
                        {
                            HttpContext.Current.Session["multiparcel_Lubbock"] = "Yes";
                        }

                        driver.Quit();
                        return "MultiParcel";
                    }
                    else
                    {
                        driver.FindElement(By.XPath("//*[@id='grid']/div[2]/table/tbody/tr/td[1]")).Click();
                        Thread.Sleep(2000);
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.Id("grid"));
                        if (INodata.Text.Contains("No properties found"))
                        {
                            HttpContext.Current.Session["Nodata_LubbockTX"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    //property details
                    string ParcelID = "", OwnerName = "", PropertyAddress = "", PropertyStatus = "", PropertyType = "", LegalDescription = "", Neighborhood = "", Account = "", MapNumber = "", OwnerID = "", Exemptions = "", PercentOwnership = "", MailingAddress = "";

                    driver.FindElement(By.Id("tdDetailsTab")).Click();
                    Thread.Sleep(2000);

                    //Owner Name~Property Address~Property Status~Property Type~Legal Description~Neighborhood~Account~Map Number~Owner ID~Exemptions~Percent Ownership~Mailing Address

                    ParcelID = driver.FindElement(By.Id("dnn_ctr416_View_tdPropertyID")).Text;
                    gc.CreatePdf(orderNumber, ParcelID, "Property details", driver, "TX", "Lubbock");
                    OwnerName = driver.FindElement(By.Id("dnn_ctr416_View_divOwnersLabel")).Text;
                    PropertyAddress = driver.FindElement(By.Id("dnn_ctr416_View_tdPropertyAddress")).Text;

                    PropertyStatus = driver.FindElement(By.Id("dnn_ctr416_View_tdGIPropertyStatus")).Text;
                    PropertyType = driver.FindElement(By.Id("dnn_ctr416_View_tdGIPropertyType")).Text;
                    LegalDescription = driver.FindElement(By.Id("dnn_ctr416_View_tdGILegalDescription")).Text;
                    Neighborhood = driver.FindElement(By.Id("dnn_ctr416_View_tdGINeighborhood")).Text;
                    Account = driver.FindElement(By.Id("dnn_ctr416_View_tdGIAccount")).Text;
                    MapNumber = driver.FindElement(By.Id("dnn_ctr416_View_tdGIMapNumber")).Text;
                    OwnerID = driver.FindElement(By.Id("dnn_ctr416_View_tdOIPartyQuickRefID")).Text;
                    Exemptions = driver.FindElement(By.Id("dnn_ctr416_View_tdOIExemptions")).Text;
                    PercentOwnership = driver.FindElement(By.Id("dnn_ctr416_View_tdOIPercentOwnership")).Text;
                    MailingAddress = driver.FindElement(By.Id("dnn_ctr416_View_tdOIMailingAddress")).Text;

                    driver.FindElement(By.XPath(" //*[@id='dnn_ctr416_View_divCamaInfo']/ul/li/table/tbody/tr[1]/td[1]/i")).Click();
                    Thread.Sleep(2000);
                    string year_built = "";
                    try
                    {
                        year_built = driver.FindElement(By.XPath("//*[@id='dnn_ctr416_View_divCamaInfo']/ul/li/div/table/tbody/tr[2]/td/table/tbody/tr[1]/td[3]")).Text;
                    }
                    catch { }
                    string property_details = OwnerName + "~" + PropertyAddress + "~" + PropertyStatus + "~" + PropertyType + "~" + LegalDescription + "~" + Neighborhood + "~" + Account + "~" + MapNumber + "~" + OwnerID + "~" + Exemptions + "~" + PercentOwnership + "~" + MailingAddress + "~" + year_built;
                    gc.insert_date(orderNumber, ParcelID, 794, property_details, 1, DateTime.Now);

                    //    Assessment Details Table:
                    //Year~Improvement Homesite Value~Improvement Non-Homesite Value~Total Improvement Market Value~Land Homesite Value~Land Non-Homesite Value~Land Agricultural Market Value~Total Land Market Value~Total Market Value~Agricultural Use~Total Appraised Value~Homestead Cap Loss~Total Assessed Value

                    string Year = "", ImprovementHomesiteValue = "", ImprovementNonHomesite = "", TotalImprovementMarketValue = "", LandHomesiteValue = "", LandNonHomesite = "", LandAgriculturalMarketValue = "", TotalLandMarketValue = "", TotalMarketValue = "", AgriculturalUse = "", TotalAppraisedValue = "", HomesteadCapLoss = "", TotalAssessedValue = "";

                    Year = driver.FindElement(By.Id("dnn_ctr416_View_tdVITitle")).Text;
                    ImprovementHomesiteValue = driver.FindElement(By.Id("dnn_ctr416_View_tdVIImprovementHS")).Text;
                    ImprovementNonHomesite = driver.FindElement(By.Id("dnn_ctr416_View_tdVIImprovementNonHS")).Text;
                    TotalImprovementMarketValue = driver.FindElement(By.Id("dnn_ctr416_View_tdVITotalImprovementMV")).Text;
                    LandHomesiteValue = driver.FindElement(By.Id("dnn_ctr416_View_tdVILandHS")).Text;
                    LandNonHomesite = driver.FindElement(By.Id("dnn_ctr416_View_tdVILandNonHS")).Text;
                    LandAgriculturalMarketValue = driver.FindElement(By.Id("dnn_ctr416_View_tdVILandAgMV")).Text;
                    TotalLandMarketValue = driver.FindElement(By.Id("dnn_ctr416_View_tdVITotalLandMV")).Text;
                    TotalMarketValue = driver.FindElement(By.Id("dnn_ctr416_View_tdVITotalMV")).Text;
                    AgriculturalUse = driver.FindElement(By.Id("dnn_ctr416_View_tdVIAgUse")).Text;
                    TotalAppraisedValue = driver.FindElement(By.Id("dnn_ctr416_View_tdVITotalAppraisedValue")).Text;
                    HomesteadCapLoss = driver.FindElement(By.Id("dnn_ctr416_View_tdVIHomesteadCapLoss")).Text;
                    TotalAssessedValue = driver.FindElement(By.Id("dnn_ctr416_View_tdVITotalAssessedValueRP")).Text;

                    string assessment_details = Year + "~" + ImprovementHomesiteValue + "~" + ImprovementNonHomesite + "~" + TotalImprovementMarketValue + "~" + LandHomesiteValue + "~" + LandNonHomesite + "~" + LandAgriculturalMarketValue + "~" + TotalLandMarketValue + "~" + TotalMarketValue + "~" + AgriculturalUse + "~" + TotalAppraisedValue + "~" + HomesteadCapLoss + "~" + TotalAssessedValue;
                    gc.insert_date(orderNumber, ParcelID, 795, assessment_details, 1, DateTime.Now);


                    //Entity Details Table:
                    //Taxing Entity~Exemptions~Exemptions Amount~Taxable Value~Tax Rate Per 100~Tax Ceiling
                    string EntityDetails = "";
                    IWebElement multitableElement1 = driver.FindElement(By.XPath("//*[@id='dnn_ctr416_View_divEntitiesAndExemptionsData']/table/tbody"));
                    IList<IWebElement> multitableRow1 = multitableElement1.FindElements(By.TagName("tr"));
                    IList<IWebElement> multirowTD1;
                    foreach (IWebElement row in multitableRow1)
                    {
                        if (!row.Text.Contains("TAXING ENTITY"))
                        {
                            multirowTD1 = row.FindElements(By.TagName("td"));
                            if (multirowTD1.Count != 0)
                            {
                                if (multirowTD1[0].Text.Trim().Contains("SAB") || multirowTD1[0].Text.Trim().Contains("SLZ") || multirowTD1[0].Text.Trim().Contains("SSO") || multirowTD1[0].Text.Trim().Contains("CAB"))
                                {
                                    HttpContext.Current.Session["tax_alert_msg"] = "Yes";
                                    HttpContext.Current.Session["tax_alert_msg1"] = multirowTD1[0].Text.Trim();
                                    EntityDetails = multirowTD1[0].Text.Trim() + "~" + multirowTD1[1].Text.Trim() + "~" + multirowTD1[2].Text.Trim() + "~" + multirowTD1[3].Text.Trim() + "~" + multirowTD1[4].Text.Trim() + "~" + multirowTD1[5].Text.Trim() + "~" + multirowTD1[0].Text.Trim() + " taxes are available";
                                    gc.insert_date(orderNumber, ParcelID, 796, EntityDetails, 1, DateTime.Now);
                                }
                                else
                                {
                                    EntityDetails = multirowTD1[0].Text.Trim() + "~" + multirowTD1[1].Text.Trim() + "~" + multirowTD1[2].Text.Trim() + "~" + multirowTD1[3].Text.Trim() + "~" + multirowTD1[4].Text.Trim() + "~" + multirowTD1[5].Text.Trim() + "~" + "";
                                    gc.insert_date(orderNumber, ParcelID, 796, EntityDetails, 1, DateTime.Now);
                                }
                            }

                        }
                    }
                    //Assessment History Table:
                    //Year~Improvement~Land~Market~Ag Market~Ag Loss~Appraised~Hs Cap Loss~Assessed
                    IWebElement multitableElement11 = driver.FindElement(By.XPath("//*[@id='dnn_ctr416_View_tblValueHistoryDataRP']/tbody"));
                    IList<IWebElement> multitableRow11 = multitableElement11.FindElements(By.TagName("tr"));
                    IList<IWebElement> multirowTD11;
                    foreach (IWebElement row in multitableRow11)
                    {
                        if (!row.Text.Contains("YEAR"))
                        {
                            multirowTD11 = row.FindElements(By.TagName("td"));
                            if (multirowTD11.Count != 0)
                            {
                                string AssessmentHistory = multirowTD11[0].Text.Trim() + "~" + multirowTD11[1].Text.Trim() + "~" + multirowTD11[2].Text.Trim() + "~" + multirowTD11[3].Text.Trim() + "~" + multirowTD11[4].Text.Trim() + "~" + multirowTD11[5].Text.Trim() + "~" + multirowTD11[6].Text.Trim() + "~" + multirowTD11[7].Text.Trim() + "~" + multirowTD11[8].Text.Trim();
                                gc.insert_date(orderNumber, ParcelID, 797, AssessmentHistory, 1, DateTime.Now);

                            }

                        }
                    }
                    gc.CreatePdf(orderNumber, ParcelID, "Property details1", driver, "TX", "Lubbock");
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    //Tax Distribution Table:
                    driver.FindElement(By.Id("tdBillsTab")).Click();
                    //Tax Year~Taxing Entity~Levy~P&I~Att. Fee~Credits/Disc.~Total Taxes Due~Paid Date~Paid Amount~Balance
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, ParcelID, "Tax Distribution1", driver, "TX", "Lubbock");
                    string levynew = "";
                    string levy = "", PI = "", Att_fee = "", credits = "", year = "", TaxingEntity = "", TotalTaxes = "", PaidDate = "", Balance = "", PaidAmount = "";
                    //*[@id="dnn_ctr416_View_divBillDetails"]/div[1]/table[1]/tbody/tr/td[3]
                    driver.FindElement(By.XPath("//*[@id='dnn_ctr416_View_divBillDetails']/div[1]/table[1]/tbody/tr/td[3]")).Click();
                    Thread.Sleep(2000);

                    string pi = driver.FindElement(By.XPath(" //*[@id='dnn_ctr416_View_divBillDetails']/div[1]/table[2]/tbody/tr[3]/td[2]/table/tbody/tr[2]/td[2]")).Text;
                    driver.FindElement(By.XPath("//*[@id='dnn_ctr416_View_divBillDetails']/div[1]/table[1]/tbody/tr/td[3]")).Click();
                    Thread.Sleep(2000);
                    if (!pi.Contains("$0.00"))
                    {
                        IWebElement dt = driver.FindElement(By.Id("effectiveDatePicker"));
                        string date = dt.GetAttribute("value");

                        DateTime G_Date = Convert.ToDateTime(date);
                        string dateChecking = DateTime.Now.ToString("MM") + "/15/" + DateTime.Now.ToString("yyyy");

                        if (G_Date < Convert.ToDateTime(dateChecking))
                        {
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
                        driver.FindElement(By.Id("effectiveDatePicker")).SendKeys(Keys.Enter);
                        driver.FindElement(By.Id("tabBills")).Click();
                        Thread.Sleep(2000);
                    }

                    for (int i = 1; i <= 3; i++)
                    {
                        driver.FindElement(By.XPath("//*[@id='dnn_ctr416_View_divBillDetails']/div[" + i + "]/table[1]/tbody/tr/td[3]")).Click();
                        Thread.Sleep(2000);
                        year = driver.FindElement(By.XPath("//*[@id='dnn_ctr416_View_divBillDetails']/div[" + i + "]/table[1]/tbody/tr/td[1]")).Text;

                        IWebElement multitableElement12 = driver.FindElement(By.XPath("//*[@id='dnn_ctr416_View_divBillDetails']/div[" + i + "]/table[2]/tbody"));
                        IList<IWebElement> multitableRow12 = multitableElement12.FindElements(By.TagName("tr"));
                        IList<IWebElement> multirowTD12;
                        foreach (IWebElement row in multitableRow12)
                        {
                            if (!row.Text.Contains("TAXING ENTITY"))
                            {
                                multirowTD12 = row.FindElements(By.TagName("td"));
                                if (multirowTD12.Count == 5 && multirowTD12[0].Text.Trim() != "")
                                {
                                    TaxingEntity = multirowTD12[0].Text.Trim();
                                    TotalTaxes = multirowTD12[1].Text.Trim();
                                    PaidDate = multirowTD12[2].Text.Trim();
                                    PaidAmount = multirowTD12[3].Text.Trim();
                                    Balance = multirowTD12[4].Text.Trim();
                                }
                                if (row.Text.Contains("Levy") && multirowTD12.Count == 13)
                                {
                                    string taxdet = multirowTD12[1].Text.Replace("\r\n", "~");
                                    string[] words = taxdet.Split('~');
                                    levy = words[0].Replace("Levy", "");

                                    PI = words[1].Replace("P&I", "");
                                    if (i == 1)
                                    {
                                        levynew = PI;
                                    }
                                    Att_fee = words[2].Replace("Att. Fee", "");
                                    credits = words[3].Replace("Credits/Disc.", "");
                                    string taxdistri = year + "~" + TaxingEntity + "~" + levy + "~" + PI + "~" + Att_fee + "~" + credits + "~" + TotalTaxes + "~" + PaidDate + "~" + PaidAmount + "~" + Balance;
                                    gc.insert_date(orderNumber, ParcelID, 798, taxdistri, 1, DateTime.Now);
                                }
                                if (row.Text.Contains("TOTALS"))
                                {
                                    string taxdistri1 = year + "~" + TaxingEntity + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + TotalTaxes + "~" + "" + "~" + PaidAmount + "~" + Balance;
                                    gc.insert_date(orderNumber, ParcelID, 798, taxdistri1, 1, DateTime.Now);
                                }
                            }
                        }
                    }

                    gc.CreatePdf(orderNumber, ParcelID, "Tax Distribution", driver, "TX", "Lubbock");
                    //  Tax Information Table:
                    // Good Through Date~Current Amount Due~Past Years Due~Total Due~Taxing Authority
                    string goodthrough = "", CurrentAmountDue = "", PastYearsDue = "", TotalDue = "", TaxingAuthority = "Lubbock Central Appraisal District 2109 Ave Q PO Box 10568, Lubbock, TX 79408-3568 806-762 - 5000 x2";
                    IWebElement dt1 = driver.FindElement(By.Id("effectiveDatePicker"));
                    goodthrough = dt1.GetAttribute("value");
                    //   goodthrough = driver.FindElement(By.ClassName("hasDatepicker")).Text;
                    CurrentAmountDue = driver.FindElement(By.Id("dnn_ctr416_View_tdPMCurrentAmountDue")).Text;
                    PastYearsDue = driver.FindElement(By.Id("dnn_ctr416_View_tdPMPastYearsDue")).Text;
                    TotalDue = driver.FindElement(By.Id("dnn_ctr416_View_tdPMTotalDue")).Text;
                    string taxinfo = goodthrough + "~" + CurrentAmountDue + "~" + PastYearsDue + "~" + TotalDue + "~" + TaxingAuthority;
                    gc.insert_date(orderNumber, ParcelID, 799, taxinfo, 1, DateTime.Now);
                    //Payment History Details Table:
                    driver.FindElement(By.Id("dnn_ctr416_View_divPaymentHistoryExpandCollapse")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, ParcelID, "Payment History", driver, "TX", "Lubbock");
                    //Tax Year~Transaction Date~Effective Date~Payment Amount~Receipt Number

                    int licount = driver.FindElements(By.XPath(" //*[@id='dnn_ctr416_View_divPaymentHistoryInfo']/ul/li")).Count;
                    for (int j = 1; j <= licount; j++)
                    {
                        string year1 = driver.FindElement(By.XPath("//*[@id='dnn_ctr416_View_divPaymentHistoryInfo']/ul/li[" + j + "]/table/tbody/tr/td[2]")).Text;
                        IWebElement multitableElement3 = driver.FindElement(By.XPath("//*[@id='dnn_ctr416_View_divPaymentHistoryInfo']/ul/li[" + j + "]/div/table/tbody"));
                        IList<IWebElement> multitableRow3 = multitableElement3.FindElements(By.TagName("tr"));
                        IList<IWebElement> multirowTD3;
                        foreach (IWebElement row in multitableRow3)
                        {
                            if (!row.Text.Contains("Transaction Date"))
                            {
                                multirowTD3 = row.FindElements(By.TagName("td"));
                                if (multirowTD3.Count == 4)
                                {
                                    string taxHistory = year1 + "~" + multirowTD3[0].Text.Trim() + "~" + multirowTD3[1].Text.Trim() + "~" + multirowTD3[2].Text.Trim() + "~" + multirowTD3[3].Text.Trim().Replace("View", "");
                                    gc.insert_date(orderNumber, ParcelID, 801, taxHistory, 1, DateTime.Now);
                                }
                            }
                        }
                    }

                    try
                    {

                        var chDriver = new ChromeDriver();
                        chDriver.Navigate().GoToUrl(driver.Url);
                        Thread.Sleep(4000);
                        chDriver.FindElement(By.Id("tdBillsTab")).Click();
                        Thread.Sleep(3000);

                        for (int j1 = 1; j1 < 4; j1++)
                        {
                            string year1 = chDriver.FindElement(By.XPath("//*[@id='dnn_ctr416_View_divBillDetails']/div[" + j1 + "]/table[1]/tbody/tr/td[1]")).Text;
                            Thread.Sleep(2000);
                            chDriver.FindElement(By.XPath("//*[@id='btnPrintTaxStatement" + year1 + "']")).SendKeys(Keys.Enter);
                            Thread.Sleep(30000);
                            chDriver.SwitchTo().Window(chDriver.WindowHandles.Last());

                            string currentURL = chDriver.Url;

                            gc.downloadfile(currentURL, orderNumber, ParcelID, "TaxStatement" + j1, "TX", "Lubbock");
                            chDriver.Navigate().Back();
                            Thread.Sleep(3000);
                            chDriver.FindElement(By.Id("tdBillsTab")).Click();
                            Thread.Sleep(4000);
                        }
                        chDriver.Quit();
                    }
                    catch
                    {

                    }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "TX", "Lubbock", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    GlobalClass.titleparcel = "";
                    gc.mergpdf(orderNumber, "TX", "Lubbock");
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