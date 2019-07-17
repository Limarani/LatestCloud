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
    public class WebDriver_FortBendTX
    {
        IWebElement PaidCount;
        IWebDriver driver;
        DBconnection db = new DBconnection();
        List<string> link = new List<string>();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_FortBend(string address, string parcelNumber, string searchType, string orderNumber, string ownername, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            List<string> taxinformation = new List<string>();
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                StartTime = DateTime.Now.ToString("HH:mm:ss");
                try
                {                    
                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, "", "", address, "TX", "Fort Bend");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_FortBend"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    driver.Navigate().GoToUrl("https://www.fbcad.org/");

                    if (searchType == "address")
                    {
                        driver.FindElement(By.Id("SearchText")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "address search", driver, "TX", "Fort Bend");
                        driver.FindElement(By.Id("dnn_PropertySearch_SearchButtonDiv")).Click();
                        Thread.Sleep(5000);
                        gc.CreatePdf_WOP(orderNumber, "address search result", driver, "TX", "Fort Bend");
                    }

                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.Id("SearchText")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search", driver, "TX", "Fort Bend");
                        driver.FindElement(By.Id("dnn_PropertySearch_SearchButtonDiv")).Click();
                        Thread.Sleep(5000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search result", driver, "TX", "Fort Bend");
                    }

                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("SearchText")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "owner search", driver, "TX", "Fort Bend");
                        driver.FindElement(By.Id("dnn_PropertySearch_SearchButtonDiv")).Click();
                        Thread.Sleep(5000);
                        gc.CreatePdf_WOP(orderNumber, "owner search result", driver, "TX", "Fort Bend");
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
                                    gc.insert_date(orderNumber, TDmulti5[0].Text, 1135, multi1, 1, DateTime.Now);
                                }
                                maxCheck++;
                            }
                        }
                        if (TRmulti5.Count > 25)
                        {
                            HttpContext.Current.Session["multiParcel_Williamson_FortBend"] = "Maximum";
                            driver.Quit();
                        }
                        else
                        {
                            HttpContext.Current.Session["multiparcel_FortBend"] = "Yes";
                            driver.Quit();
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
                        string Nodata = driver.FindElement(By.XPath("//*[@id='grid']/div[2]/table/tbody/tr/td")).Text;
                        if (Nodata.Contains("No properties found."))
                        {
                            HttpContext.Current.Session["Nodata_FortBend"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    //property details
                    List<string> entity = new List<string>();
                    entity.AddRange(new string[] { "M84", "M83", "M81", "M289", "M276", "M261", "M24", "W22", "W07", "M94", "M87", "M77", "M75", "M73", "M62", "M264", "M245", "M23", "M197", "M177", "M163", "M98", "M91", "M143", "M141", "M139", "M123", "M121", "M113", "M110", "W05", "SM105", "SM100", "M92", "M260", "M244", "M21", "M189", "M186", "M185", "M167", "M165", "M16", "M02", "M125", "M125", "D01", "M02", "M171", "M174", "M178", "M179", "M183", "M188", "M190", "M191", "M195", "M198", "M201", "M204", "M205", "M206", "M207", "M208", "M21", "M210", "M216", "M221", "M222", "M227", "M235", "M236", "M237", "M238", "M253", "M255", "M260", "M263", "M277", "M278", "M279", "M291", "M30", "M44", "M92", "SM100", "W26", "W28", "W35", "W39", "W41", "W42", "M170", "M175", "M192", "M193", "M136", "M172", "M213", "M228", "M263", "M50", "M52", "M54", "M58", "M96", "W01", "W30", "W32", "M160", "M180", "M181", "M182", "M196", "M199", "M20", "M217", "M22", "M225", "M226", "M290", "M230", "M231", "M232", "M233", "M234", "M246", "M248", "M262", "M266", "M273", "M275", "M29", "M40", "M41", "M42", "M55", "M76", "W06", "W13" });
                    string owneraddress = "", Yearbuilt = "", ParcelID = "", OwnerName = "", PropertyAddress = "", PropertyStatus = "", PropertyType = "", LegalDescription = "", Neighborhood = "", Account = "", MapNumber = "", OwnerID = "", Exemptions = "", PercentOwnership = "", MailingAddress = "";

                    driver.FindElement(By.Id("tabDetails")).Click();
                    Thread.Sleep(2000);
                    ParcelID = driver.FindElement(By.Id("dnn_ctr485_View_tdPropertyID")).Text;
                    gc.CreatePdf(orderNumber, ParcelID, "Property details", driver, "TX", "Fort Bend");
                    OwnerName = driver.FindElement(By.Id("dnn_ctr485_View_tdOwnerName")).Text;
                    PropertyAddress = driver.FindElement(By.Id("dnn_ctr485_View_tdPropertyAddress")).Text;
                    string[] owneradd1 = PropertyAddress.Split(',');
                    owneraddress = owneradd1[0].Trim();

                    // owneraddress = owneraddress2.Trim();
                    PropertyStatus = driver.FindElement(By.Id("dnn_ctr485_View_tdGIPropertyStatus")).Text;
                    PropertyType = driver.FindElement(By.Id("dnn_ctr485_View_tdGIPropertyType")).Text;
                    LegalDescription = driver.FindElement(By.Id("dnn_ctr485_View_tdGILegalDescription")).Text;
                    Neighborhood = driver.FindElement(By.Id("dnn_ctr485_View_tdGINeighborhood")).Text;
                    Account = driver.FindElement(By.Id("dnn_ctr485_View_tdGIAccount")).Text;
                    MapNumber = driver.FindElement(By.Id("dnn_ctr485_View_tdGIMapNumber")).Text;
                    OwnerID = driver.FindElement(By.Id("dnn_ctr485_View_tdOIPartyQuickRefID")).Text;
                    Exemptions = driver.FindElement(By.Id("dnn_ctr485_View_tdOIExemptions")).Text;
                    PercentOwnership = driver.FindElement(By.Id("dnn_ctr485_View_tdOIPercentOwnership")).Text;
                    MailingAddress = driver.FindElement(By.Id("dnn_ctr485_View_tdOIMailingAddress")).Text;
                    try
                    {
                        IWebElement IAddressSearch1 = driver.FindElement(By.XPath("//*[@id='dnn_ctr485_View_divCamaInfo']/ul/li/table/tbody/tr[1]/td[1]"));
                        IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                        js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                        Thread.Sleep(2000);
                        Yearbuilt = driver.FindElement(By.XPath("//*[@id='dnn_ctr485_View_divCamaInfo']/ul/li/div/table/tbody/tr[3]/td/table/tbody/tr[1]/td[3]")).Text.Trim();
                    }
                    catch { }
                    string property_details = OwnerName + "~" + PropertyAddress + "~" + PropertyStatus + "~" + PropertyType + "~" + LegalDescription + "~" + Neighborhood + "~" + Account + "~" + MapNumber + "~" + OwnerID + "~" + Exemptions + "~" + PercentOwnership + "~" + MailingAddress + "~" + Yearbuilt;
                    gc.insert_date(orderNumber, ParcelID, 1132, property_details, 1, DateTime.Now);

                    //    Assessment Details Table:
                    //Year~Improvement Homesite Value~Improvement Non-Homesite Value~Total Improvement Market Value~Land Homesite Value~Land Non-Homesite Value~Land Agricultural Market Value~Total Land Market Value~Total Market Value~Agricultural Use~Total Appraised Value~Homestead Cap Loss~Total Assessed Value

                    string Year1 = "", ImprovementHomesiteValue = "", ImprovementNonHomesite = "", TotalImprovementMarketValue = "", LandHomesiteValue = "", LandNonHomesite = "", LandAgriculturalMarketValue = "", TotalLandMarketValue = "", TotalMarketValue = "", AgriculturalUse = "", TotalAppraisedValue = "", HomesteadCapLoss = "", TotalAssessedValue = "";

                    Year1 = driver.FindElement(By.Id("dnn_ctr485_View_tdVITitle")).Text;
                    string[] splityear = Year1.Split();
                    string Year = splityear[0].Trim();
                    ImprovementHomesiteValue = driver.FindElement(By.Id("dnn_ctr485_View_tdVIImprovementHS")).Text;
                    ImprovementNonHomesite = driver.FindElement(By.Id("dnn_ctr485_View_tdVIImprovementNonHS")).Text;
                    TotalImprovementMarketValue = driver.FindElement(By.Id("dnn_ctr485_View_tdVITotalImprovementMV")).Text;
                    LandHomesiteValue = driver.FindElement(By.Id("dnn_ctr485_View_tdVILandHS")).Text;
                    LandNonHomesite = driver.FindElement(By.Id("dnn_ctr485_View_tdVILandNonHS")).Text;
                    LandAgriculturalMarketValue = driver.FindElement(By.Id("dnn_ctr485_View_tdVILandAgMV")).Text;
                    TotalLandMarketValue = driver.FindElement(By.Id("dnn_ctr485_View_tdVITotalLandMV")).Text;
                    TotalMarketValue = driver.FindElement(By.Id("dnn_ctr485_View_tdVITotalMV")).Text;
                    AgriculturalUse = driver.FindElement(By.Id("dnn_ctr485_View_tdVIAgUse")).Text;
                    TotalAppraisedValue = driver.FindElement(By.Id("dnn_ctr485_View_tdVITotalAppraisedValue")).Text;
                    HomesteadCapLoss = driver.FindElement(By.Id("dnn_ctr485_View_tdVIHomesteadCapLoss")).Text;
                    TotalAssessedValue = driver.FindElement(By.Id("dnn_ctr485_View_tdVITotalAssessedValueRP")).Text;

                    string assessment_details = Year + "~" + ImprovementHomesiteValue + "~" + ImprovementNonHomesite + "~" + TotalImprovementMarketValue + "~" + LandHomesiteValue + "~" + LandNonHomesite + "~" + LandAgriculturalMarketValue + "~" + TotalLandMarketValue + "~" + TotalMarketValue + "~" + AgriculturalUse + "~" + TotalAppraisedValue + "~" + HomesteadCapLoss + "~" + TotalAssessedValue;
                    gc.insert_date(orderNumber, ParcelID, 1133, assessment_details, 1, DateTime.Now);

                    //Entity Details Table:
                    //Taxing Entity~Exemptions~Exemptions Amount~Taxable Value~Tax Rate Per 100~Tax Ceiling
                    string msg1 = "", entityname = "";
                    string a1 = "";
                    List<string> scenario = new List<string>();
                    try
                    {
                        IWebElement multitableElement1 = driver.FindElement(By.XPath("//*[@id='dnn_ctr485_View_divEntitiesAndExemptionsData']/table/tbody"));
                        IList<IWebElement> multitableRow1 = multitableElement1.FindElements(By.TagName("tr"));
                        IList<IWebElement> multirowTD1;
                        foreach (IWebElement row in multitableRow1)
                        {
                            if (!row.Text.Contains("TAXING ENTITY"))
                            {
                                multirowTD1 = row.FindElements(By.TagName("td"));
                                if (multirowTD1.Count != 0)
                                {
                                    string EntityDetails = multirowTD1[0].Text.Trim() + "~" + multirowTD1[1].Text.Trim() + "~" + multirowTD1[2].Text.Trim() + "~" + multirowTD1[3].Text.Trim() + "~" + multirowTD1[4].Text.Trim() + "~" + multirowTD1[5].Text.Trim();
                                    gc.insert_date(orderNumber, ParcelID, 1141, EntityDetails, 1, DateTime.Now);

                                }
                                //M217,M217
                                a1 = multirowTD1[0].Text;
                                a1 = GlobalClass.Before(a1, "-");
                                if ((!row.Text.Contains("TOTALS") && multirowTD1[2].Text.Trim() != "0.000000" || multirowTD1[2].Text.Trim() != "$0") && entity.Any(str => str.Contains(a1)))
                                {
                                    scenario.Add(a1);
                                    // msg1 = a1;
                                    entityname = multirowTD1[1].Text;
                                }
                            }
                        }
                    }
                    catch { }

                    //Assessment History Table:
                    //Year~Improvement~Land~Market~Ag Market~Ag Loss~Appraised~Hs Cap Loss~Assessed
                    try
                    {
                        IWebElement multitableElement11 = driver.FindElement(By.XPath("//*[@id='dnn_ctr485_View_tblValueHistoryDataRP']/tbody"));
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
                                    gc.insert_date(orderNumber, ParcelID, 1142, AssessmentHistory, 1, DateTime.Now);

                                }

                            }
                        }
                    }
                    catch { }

                    //Tax Information Details
                    string Taxauthority1 = "";
                    try
                    {
                        driver.Navigate().GoToUrl("https://actweb.acttax.com/act_webdev/fbc/index.jsp");

                        driver.FindElement(By.Id("sc4")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.Id("criteria")).SendKeys(Account);
                        gc.CreatePdf(orderNumber, ParcelID, "Enter The Parcel Number Before", driver, "TX", "Fort Bend");
                        driver.FindElement(By.Name("submit")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, ParcelID, "Parcel search result", driver, "TX", "Fort Bend");

                        driver.FindElement(By.XPath("//*[@id='data-block']/table/tbody/tr/td/table/tbody/tr/td[2]/h3/a")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, ParcelID, "tax search result details", driver, "TX", "Fort Bend");

                        //Tax Jurisdiction Details
                        driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[3]/td/table/tbody/tr[2]/td[2]/h3[8]/a")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, ParcelID, "Tax Jurisdiction Details", driver, "TX", "Fort Bend");
                        //Jurisdiction Information for ~Account No~Exemptions~Jurisdictions~Market Value~Exemption Value~Taxable Value~Tax Rate~Levy
                        string accountnosjur = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[1]/td/table/tbody/tr[1]/td/h3[2]")).Text.Replace("Account No.:", "");
                        string jurdinfoYear = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[1]/td/table/tbody/tr[1]/td/h3[1]")).Text.Replace("Jurisdiction Information for", "");
                        string exemptions = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[1]/td/table/tbody/tr[1]/td/h3[3]")).Text.Replace("Exemptions:", "");

                        IWebElement multitableElement6 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[1]/td/table/tbody/tr[2]/td/table/tbody"));
                        IList<IWebElement> multitableRow6 = multitableElement6.FindElements(By.TagName("tr"));
                        IList<IWebElement> multirowTD6;
                        foreach (IWebElement row in multitableRow6)
                        {
                            multirowTD6 = row.FindElements(By.TagName("td"));
                            if (multirowTD6.Count != 0 && !row.Text.Contains("Jurisdictions"))
                            {
                                string TaxesDue = jurdinfoYear + "~" + accountnosjur + "~" + exemptions + "~" + multirowTD6[0].Text.Trim() + "~" + multirowTD6[1].Text.Trim() + "~" + multirowTD6[2].Text.Trim() + "~" + multirowTD6[3].Text.Trim() + "~" + multirowTD6[4].Text.Trim() + "~" + multirowTD6[5].Text.Trim();
                                gc.insert_date(orderNumber, ParcelID, 1259, TaxesDue, 1, DateTime.Now);
                            }
                        }

                        driver.FindElement(By.LinkText("Return to the Previous Page")).Click();
                        Thread.Sleep(2000);

                        driver.FindElement(By.LinkText("Taxes Due Detail by Year and Jurisdiction")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, ParcelID, "Taxes Due Detail by Year and Jurisdiction", driver, "TX", "Fort Bend");


                        //Taxes Due Detail by Year Details Table: 
                        string taxdueaccountnosjur = "", lawsuits = "", ActiveLawsuits = "";
                        taxdueaccountnosjur = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[1]/td/center/table/tbody/tr/td/div/h3[2]")).Text.Replace("Account No.:", "");
                        lawsuits = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[1]/td/center/table/tbody/tr/td/div/h3[3]")).Text.Replace("Active Lawsuits", "");
                        //Year~Base Tax Due~Penalty, Interest, and ACC* Due~Total Due~Penalty, Interest, and ACC* Due1~Total Due1~Penalty, Interest, and ACC*Due2~Total Due2

                        IWebElement multitableElement31 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[1]/td/center/table/tbody/tr/td/table/tbody"));
                        IList<IWebElement> multitableRow31 = multitableElement31.FindElements(By.TagName("tr"));
                        IList<IWebElement> multirowTD31;
                        foreach (IWebElement row in multitableRow31)
                        {
                            multirowTD31 = row.FindElements(By.TagName("td"));
                            if (multirowTD31.Count == 8 && !row.Text.Contains("Base Tax Due"))
                            {
                                string TaxesDue = taxdueaccountnosjur + "~" + lawsuits + "~" + multirowTD31[0].Text.Trim() + "~" + multirowTD31[1].Text.Trim() + "~" + multirowTD31[2].Text.Trim() + "~" + multirowTD31[3].Text.Trim() + "~" + multirowTD31[4].Text.Trim() + "~" + multirowTD31[5].Text.Trim() + "~" + multirowTD31[6].Text.Trim() + "~" + multirowTD31[7].Text.Trim();
                                gc.insert_date(orderNumber, ParcelID, 1156, TaxesDue, 1, DateTime.Now);
                            }
                            if (multirowTD31.Count == 1)
                            {
                                string TaxesDue = taxdueaccountnosjur + "~" + lawsuits + "~" + multirowTD31[0].Text.Trim() + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                gc.insert_date(orderNumber, ParcelID, 1156, TaxesDue, 1, DateTime.Now);

                            }
                            if (multirowTD31.Count == 4)
                            {
                                string TaxesDue = taxdueaccountnosjur + "~" + lawsuits + "~" + "" + "~" + "" + "~" + multirowTD31[1].Text.Trim() + "~" + "" + "~" + multirowTD31[2].Text.Trim() + "~" + "" + "~" + multirowTD31[3].Text.Trim() + "~" + "";
                                gc.insert_date(orderNumber, ParcelID, 1156, TaxesDue, 1, DateTime.Now);

                            }
                        }

                        driver.FindElement(By.LinkText("Return to the Previous Page")).Click();
                        Thread.Sleep(2000);


                        try
                        {
                            IWebElement Taxauthorit = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[6]/td/table[1]/tbody/tr/td[1]"));
                            Taxauthority1 = GlobalClass.After(Taxauthorit.Text, "FORT BEND COUNTY TAX OFFICE").Trim();

                        }
                        catch { }

                        //Tax Payment Details Table: 

                        IWebElement clickaddress = driver.FindElement(By.XPath("/html/body/table[2]"));
                        IList<IWebElement> tableread = clickaddress.FindElements(By.TagName("a"));
                        foreach (IWebElement tablerow in tableread)
                        {
                            if (tablerow.Text.Contains("Payment Information"))
                            {
                                tablerow.Click();
                                break;
                            }
                        }
                        Thread.Sleep(2000);

                        gc.CreatePdf(orderNumber, ParcelID, "Payment Information", driver, "TX", "Fort Bend");
                        //Account Number~Paid Date~Amount~Tax Year~Description~Paid By
                        string accountnumber2 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[1]/td/h3[3]")).Text.Replace("Account No.:", "");

                        IWebElement multitableElement32 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[1]/td/table/tbody"));
                        IList<IWebElement> multitableRow32 = multitableElement32.FindElements(By.TagName("tr"));
                        IList<IWebElement> multirowTD32;
                        foreach (IWebElement row in multitableRow32)
                        {
                            multirowTD32 = row.FindElements(By.TagName("td"));
                            if (multirowTD32.Count != 0 && !row.Text.Contains("Receipt Date"))
                            {
                                string TaxesDue = accountnumber2 + "~" + multirowTD32[0].Text.Trim() + "~" + multirowTD32[1].Text.Trim() + "~" + multirowTD32[2].Text.Trim() + "~" + multirowTD32[3].Text.Trim() + "~" + multirowTD32[4].Text.Trim();
                                gc.insert_date(orderNumber, ParcelID, 1157, TaxesDue, 1, DateTime.Now);
                            }
                        }

                    }
                    catch { }
                    try
                    {
                        driver.FindElement(By.LinkText("Return to the Previous Page")).Click();
                        Thread.Sleep(2000);
                    }
                    catch { }
                    ////Tax Bill Details Table:

                    string accountnumber = "", CADReferenceNumber1 = "", CADReferenceNumber = "", OwnerNameAndOwnerAddress = "", TaxPropertyAddress = "", legal = "", Bankruptcy = "", Pendinginterestpayments = "", Totalamountdueforyears = "", Marketvalue = "", CurrentTax = "", CurrentAmount = "", PriorYearAmount = "", TotalAmount = "", LastPaymentAmount = "", LastPayer = "", LastPaymentDate = "", ActiveLawsuitsTax = "", GrossValue = "", LandValue = "", ImprovementValue = "", CappedValue = "", AgriculturalValue = "", ExemptionsTax = "";
                    string fullTaxeBill1 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[3]/td/table")).Text.Replace("\r\n", " ");

                    accountnumber = gc.Between(fullTaxeBill1, "Account Number:", "Address:");
                    OwnerNameAndOwnerAddress = gc.Between(fullTaxeBill1, "Address:", "Property Site Address:");

                    CADReferenceNumber1 = gc.Between(fullTaxeBill1, "Bankruptcy", "Pending Internet Payments:").Trim();
                    string[] CADReferenceNumbersplit = CADReferenceNumber1.Split();
                    CADReferenceNumber = CADReferenceNumbersplit[1];
                    TaxPropertyAddress = gc.Between(fullTaxeBill1, "Property Site Address:", " Legal Description:");
                    legal = gc.Between(fullTaxeBill1, "Legal Description:", "Jurisdictions Collected by the Fort Bend County");
                    ActiveLawsuitsTax = gc.Between(fullTaxeBill1, "Active Lawsuits", "Bankruptcy");
                    Bankruptcy = CADReferenceNumbersplit[0];
                    Pendinginterestpayments = gc.Between(fullTaxeBill1, "Pending Internet Payments:", "Total Amount Due");
                    Totalamountdueforyears = gc.Between(fullTaxeBill1, "Prior Years:", "Market Value:");
                    Marketvalue = gc.Between(fullTaxeBill1, "Market Value:", "Land Value:");

                    LandValue = gc.Between(fullTaxeBill1, "Land Value:", "Improvement Value:");
                    ImprovementValue = gc.Between(fullTaxeBill1, "Improvement Value:", "Capped Value:");
                    CappedValue = gc.Between(fullTaxeBill1, "Capped Value:", "Agricultural Value:");
                    AgriculturalValue = gc.Between(fullTaxeBill1, "Agricultural Value:", "Exemptions");

                    string taxbill = accountnumber.Trim() + "~" + CADReferenceNumber.Trim() + "~" + OwnerNameAndOwnerAddress.Trim() + "~" + TaxPropertyAddress.Trim() + "~" + legal.Trim() + "~" + ActiveLawsuitsTax.Trim() + "~" + Bankruptcy.Trim() + "~" + Pendinginterestpayments.Trim() + "~" + Totalamountdueforyears.Trim() + "~" + Marketvalue.Trim() + "~" + LandValue.Trim() + "~" + ImprovementValue.Trim() + "~" + CappedValue.Trim() + "~" + AgriculturalValue.Trim() + "~" + Taxauthority1.Trim();
                    gc.insert_date(orderNumber, ParcelID, 1158, taxbill, 1, DateTime.Now);

                    //Tax Statement Pdf Download
                    try
                    {
                        IWebElement Itaxstmt1 = driver.FindElement(By.LinkText("Print a Current Tax Statement"));
                        Thread.Sleep(2000);
                        string stmt11 = Itaxstmt1.GetAttribute("href");
                        IWebElement Itaxdstmt1 = driver.FindElement(By.LinkText("Print a Delinquent Tax Statement"));
                        Thread.Sleep(2000);
                        string dstmt11 = Itaxdstmt1.GetAttribute("href");

                        driver.Navigate().GoToUrl(stmt11);
                        Thread.Sleep(4000);
                        IWebElement Itaxstmt = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[3]/td/div/h3/a"));
                        string stmt1 = Itaxstmt.GetAttribute("href");
                        gc.downloadfile(stmt1, orderNumber, ParcelID, "Tax statement", "TX", "Fort Bend");

                        driver.Navigate().GoToUrl(dstmt11);
                        Thread.Sleep(4000);
                        IWebElement dItaxstmt = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[3]/td/div/h3/a"));
                        string dstmt1 = dItaxstmt.GetAttribute("href");
                        gc.downloadfile(dstmt1, orderNumber, ParcelID, "Tax Delinquent statement", "TX", "Fort Bend");

                    }
                    catch { }

                    /////////////////////////////

                    //4) MUD Scenario : MUD Tax Office:
                    //Utility Tax Service Details
                    //"M02", "M171", "M174", "M178", "M179", "M183", "M188", "M190", "M191", "M195", "M198", "M201", "M204", "M205", "M206", "M207", "M208", "M21", "M210", "M216", "M221", "M222", "M227", "M235", "M236", "M237", "M238", "M253", "M255", "M260", "M277", "M278", "M279", "M291", "M30", "M44", "M92", "SM100", "W26","W28","W35","W39","W41","W42"


                    for (int k = 0; k < scenario.Count; k++)
                    {
                        msg1 = scenario[k];
                        if (msg1 == "W05" || msg1 == "SM105" || msg1 == "SM100" || msg1 == "M217" || msg1 == "M92" || msg1 == "M260" || msg1 == "M244" || msg1 == "M21" || msg1 == "M189" || msg1 == "M186" || msg1 == "M185" || msg1 == "M167" || msg1 == "M165" || msg1 == "M16" || msg1 == "M02" || msg1 == "M125" || msg1 == "M171" || msg1 == "M174" || msg1 == "M178" || msg1 == "M179" || msg1 == "M183" || msg1 == "M188" || msg1 == "M190" || msg1 == "M191" || msg1 == "M195" || msg1 == "M198" || msg1 == "M201" || msg1 == "M204" || msg1 == "M205" || msg1 == "M206" || msg1 == "M207" || msg1 == "M208" || msg1 == "M210" || msg1 == "M216" || msg1 == "M221" || msg1 == "M222" || msg1 == "M227" || msg1 == "M235" || msg1 == "M236" || msg1 == "M237" || msg1 == "M238" || msg1 == "M253" || msg1 == "M255" || msg1 == "M260" || msg1 == "M277" || msg1 == "M278" || msg1 == "M279" || msg1 == "M291" || msg1 == "M30" || msg1 == "M44" || msg1 == "M92" || msg1 == "SM100" || msg1 == "W26" || msg1 == "W28" || msg1 == "W35" || msg1 == "W39" || msg1 == "M41" || msg1 == "M42")

                        {
                            try
                            {
                                string strAccountNo = "", strOwnerName = "", strAddress = "", strServiceAdrress = "", strJuriID = "", strPropertyInfo = "", strTaxAuthority = "", strTaxYear = "", strDelinquentDate = "", strAcerage = "", PayCheck = "", AppraisedTitle = "", AppraisedValue = "", UTax = "", TaxingTitle = "", TaxingValue = "";
                                driver.Navigate().GoToUrl("http://www.taxtech.net/SrchAcct.aspx");
                                driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWSrchAcctCtl_AcctNo")).SendKeys(Account.Replace("-", "").Trim()); //GeographicID.Replace("-", "").Trim()
                                gc.CreatePdf(orderNumber, ParcelID, "Fort Bend Utility Tax Search", driver, "TX", "Fort Bend");
                                driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWSrchAcctCtl_SrchButton")).SendKeys(Keys.Enter);
                                gc.CreatePdf(orderNumber, ParcelID, "Fort Bend utility Tax Result", driver, "TX", "Fort Bend");
                                driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_TSWSrchAcctCtl_GridView1']/tbody/tr[2]/td[2]/a")).Click();
                                gc.CreatePdf(orderNumber, ParcelID, "Fort Bend Tax Search Result", driver, "TX", "Fort Bend");
                                strAccountNo = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_FormViewAcctInfo']/tbody/tr[2]/td/b")).Text;
                                strOwnerName = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_FormViewAcctInfo_SortNameLabel")).Text;
                                strAddress = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_FormViewAcctInfo_OwnerAddr2Label")).Text;


                                try
                                {
                                    strAddress = strAddress + " " + driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_FormViewAcctInfo_OwnerAddr3Label")).Text;
                                }
                                catch { }
                                try
                                {
                                    strAddress = strAddress + " " + driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_FormViewAcctInfo_OwnerAddr4Label")).Text;
                                }
                                catch { }
                                strServiceAdrress = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_FormViewAcctInfo_ServiceAddr1Label")).Text;
                                try
                                {
                                    strServiceAdrress = strServiceAdrress + " " + driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_FormViewAcctInfo_ServiceAddr2Label")).Text;
                                }
                                catch { }
                                strJuriID = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_FormViewEntity_EntityIDLabel")).Text;
                                strTaxAuthority = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_FormViewEntity_EntityNameLabel")).Text;

                                try
                                {
                                    strTaxAuthority = strTaxAuthority + " " + driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_FormViewEntity_Addr1Label")).Text;
                                    strTaxAuthority = strTaxAuthority + " " + driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_FormViewEntity_Addr2Label")).Text;
                                    strTaxAuthority = strTaxAuthority + " " + driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_FormViewEntity_Addr3Label")).Text;
                                    strTaxAuthority = strTaxAuthority + " " + driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_FormViewEntity_PhoneNoLabel")).Text;
                                }
                                catch { }

                                IWebElement IBulkData = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_FormViewAcctRoll"));
                                strTaxYear = gc.Between(IBulkData.Text, "Tax Year: ", "Statement Mail");
                                strDelinquentDate = GlobalClass.After(IBulkData.Text, "Delinquent Date:");
                                IWebElement IPropInfo = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_FormViewPropertyInfo"));
                                strPropertyInfo = gc.Between(IPropInfo.Text, "Property Information", "Acreage:");
                                strAcerage = GlobalClass.After(IPropInfo.Text, "Acreage:");
                                PayCheck = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_PayCheckName")).Text;
                                IWebElement IAppraise = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_GridViewValues"));
                                IList<IWebElement> IAppraiseRow = IAppraise.FindElements(By.TagName("tr"));
                                IList<IWebElement> IAppraiseTD;

                                foreach (IWebElement Appraise in IAppraiseRow)
                                {
                                    IAppraiseTD = Appraise.FindElements(By.TagName("td"));
                                    if (IAppraiseTD.Count != 0)
                                    {
                                        AppraisedTitle += IAppraiseTD[0].Text + "~";
                                        AppraisedValue += IAppraiseTD[1].Text + "~";

                                    }
                                }
                                db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + "Account Number~Owner Name~Owner Address~Service Address~Tax Year~Property Information~Acreage~Jurisdiction ID~" + AppraisedTitle + "Tax Authority" + "' where Id = '" + 1164 + "'");
                                string UPropertyDetails = strAccountNo + "~" + strOwnerName + "~" + strAddress.Trim() + "~" + strServiceAdrress.Trim() + "~" + strTaxYear + "~" + strPropertyInfo + "~" + strAcerage + "~" + strJuriID + "~" + AppraisedValue + strTaxAuthority;
                                gc.insert_date(orderNumber, strAccountNo, 1164, UPropertyDetails, 1, DateTime.Now);

                                IWebElement IUTax = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_GridViewTaxes"));
                                IList<IWebElement> IUTaxRow = IUTax.FindElements(By.TagName("tr"));
                                IList<IWebElement> IUTaxTD;
                                foreach (IWebElement Tax in IUTaxRow)
                                {
                                    IUTaxTD = Tax.FindElements(By.TagName("td"));
                                    if (IUTaxTD.Count != 0 && Tax.Text.Trim() != "")
                                    {
                                        UTax = strTaxYear + "~" + IUTaxTD[0].Text + "~" + IUTaxTD[1].Text + "~" + IUTaxTD[2].Text + "~" + IUTaxTD[3].Text + "~" + IUTaxTD[4].Text;
                                    }
                                }

                                IWebElement IUTaxLevy = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_TSWAcctInfoCtl1_GridViewYears"));
                                IList<IWebElement> IUTaxLevyRow = IUTaxLevy.FindElements(By.TagName("tr"));
                                IList<IWebElement> IUTaxLevyTD;
                                foreach (IWebElement Levy in IUTaxLevyRow)
                                {
                                    IUTaxLevyTD = Levy.FindElements(By.TagName("td"));
                                    if (IUTaxLevyTD.Count != 0 && !Levy.Text.Contains("Due for All Years"))
                                    {
                                        string UTaxLevy = strAccountNo + "~" + IUTaxLevyTD[0].Text + "~" + IUTaxLevyTD[1].Text + "~" + IUTaxLevyTD[2].Text;
                                        gc.insert_date(orderNumber, strAccountNo, 1168, UTaxLevy, 1, DateTime.Now);
                                    }
                                    if (IUTaxLevyTD.Count != 0 && Levy.Text.Contains("Due for All Years"))
                                    {
                                        string UTaxLevy = strAccountNo + "~" + IUTaxLevyTD[1].Text + "~" + "~" + IUTaxLevyTD[2].Text;
                                        gc.insert_date(orderNumber, strAccountNo, 1168, UTaxLevy, 1, DateTime.Now);
                                    }
                                }

                                IWebElement IUTaxPost = driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[3]/table/tbody/tr[3]/td/table/tbody/tr/td/table[1]/tbody/tr[5]/td[2]"));
                                IList<IWebElement> IUTaxPostRow = IUTaxPost.FindElements(By.TagName("tr"));
                                IList<IWebElement> IUTaxPostTD;
                                foreach (IWebElement Post in IUTaxPostRow)
                                {
                                    IUTaxPostTD = Post.FindElements(By.TagName("td"));
                                    if (IUTaxPostTD.Count != 0 && IUTaxPostTD.Count == 2)
                                    {
                                        TaxingTitle += IUTaxPostTD[0].Text + "~";
                                        TaxingValue += IUTaxPostTD[1].Text + "~";
                                    }
                                    if (IUTaxPostTD.Count != 0 && IUTaxPostTD.Count == 3 && !Post.Text.Contains("If Postmarked"))
                                    {
                                        string UTaxPost = strAccountNo + "~" + IUTaxPostTD[0].Text + "~" + IUTaxPostTD[1].Text + "~" + IUTaxPostTD[2].Text;
                                        gc.insert_date(orderNumber, strAccountNo, 1169, UTaxPost, 1, DateTime.Now);
                                    }
                                }
                                db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + "Account Number~Tax Year~Taxing Unit~Exempt Amount~Taxable Value~Tax Rate~Tax Levy~" + TaxingTitle + "Delinquent Date~Make Checks Payable" + "' where Id = '" + 1167 + "'");
                                string UTaxDetails = strAccountNo + "~" + UTax + "~" + TaxingValue + strDelinquentDate + "~" + PayCheck;
                                gc.insert_date(orderNumber, strAccountNo, 1167, UTaxDetails, 1, DateTime.Now);
                            }


                            catch { }
                        }

                        //5) MUD Scenario : CINCO MUD 14.

                        if (msg1 == "M98" || msg1 == "M91" || msg1 == "M143" || msg1 == "M141" || msg1 == "M139" || msg1 == "M123" || msg1 == "M121" || msg1 == "M113" || msg1 == "M110" || msg1 == "M108" || msg1 == "M170" || msg1 == "M175" || msg1 == "M192" || msg1 == "M193")
                        {

                            try
                            {

                                driver.Navigate().GoToUrl("https://www.wheelerassoc.com/search");
                                string CadNo = "";
                                try
                                {
                                    driver.FindElement(By.Id("MainContent_AccountTabContainer_TabPanelCAD_CadTextBox")).SendKeys(Account.Replace("-", "").Trim());
                                    gc.CreatePdf_WOP(orderNumber, "AddressSearch", driver, "TX", "Fort Bend");
                                    driver.FindElement(By.Id("MainContent_AccountTabContainer_TabPanelCAD_CadButton")).Click();
                                    Thread.Sleep(2000);
                                    try
                                    {
                                        gc.CreatePdf_WOP(orderNumber, "AddressSearch Result", driver, "TX", "Fort Bend");
                                        IWebElement Mudtable = driver.FindElement(By.XPath("//*[@id='MainContent_AcctListGridView']/tbody"));
                                        IList<IWebElement> Mudrow = Mudtable.FindElements(By.TagName("tr"));
                                        IList<IWebElement> Mudid;
                                        foreach (IWebElement MudidMudid in Mudrow)
                                        {
                                            Mudid = MudidMudid.FindElements(By.TagName("td"));
                                            if (Mudid.Count != 0 && !MudidMudid.Text.Contains("CAD Account #"))
                                            {
                                                IWebElement accountmUd = Mudid[0].FindElement(By.TagName("a"));
                                                string Mudhref = accountmUd.GetAttribute("href");
                                                driver.Navigate().GoToUrl(Mudhref);
                                                Thread.Sleep(2000);
                                                break;
                                            }
                                        }
                                    }
                                    catch { }
                                    for (int i = 0; i < 3; i++)
                                    {
                                        //string current = driver.CurrentWindowHandle;
                                        if (i > 0)
                                        {
                                            //driver.SwitchTo().Window(current);
                                            IWebElement PropertyInformation = driver.FindElement(By.Id("MainContent_TaxYearDropDown"));
                                            SelectElement PropertyInformationSelect = new SelectElement(PropertyInformation);
                                            PropertyInformationSelect.SelectByIndex(i);
                                            Thread.Sleep(3000);
                                        }
                                        string Taxrate = "", PaymentsApplied = "", HomesteadExemption = "", lane = "", TaxLevied = "", Improvements = "", TaxableValue = "", Tax_Year_Balance = "";
                                        string Jurisdiction = driver.FindElement(By.Id("MainContent_DistrictNameTxt")).Text;
                                        string Tax_AuthorityMud4 = driver.FindElement(By.XPath("//*[@id='PrintArea']/div[1]")).Text;
                                        string Tax_Authority = GlobalClass.After(Tax_AuthorityMud4, "Jurisdiction").Trim();
                                        string Owner_Name = driver.FindElement(By.Id("MainContent_OwnerNameTxt")).Text;
                                        string OwnerAddress1 = driver.FindElement(By.Id("MainContent_OwnerAdd1Txt")).Text;
                                        string OwnerAddress2 = driver.FindElement(By.Id("MainContent_OwnerAdd2Txt")).Text;
                                        string FullOwnerAddress = OwnerAddress1 + " " + OwnerAddress2;
                                        string InquiryDate = driver.FindElement(By.Id("MainContent_DateTxt")).Text;
                                        string DelinquentDate = driver.FindElement(By.Id("MainContent_DelinquentTxt")).Text;
                                        CadNo = driver.FindElement(By.Id("MainContent_CadNoTxt")).Text;
                                        string TaxYear = driver.FindElement(By.Id("MainContent_TaxYearTxt")).Text;
                                        string JurisdictionCode = driver.FindElement(By.Id("MainContent_JurNoTxt")).Text;
                                        string Acreage = driver.FindElement(By.Id("MainContent_AcreageTxt")).Text;
                                        string strLegalDescription = driver.FindElement(By.Id("MainContent_LegalAddTxt")).Text;
                                        string FullPropertyAddress = driver.FindElement(By.Id("MainContent_SitusAddTxt")).Text;

                                        IWebElement Apprasialvaluetable = driver.FindElement(By.XPath("//*[@id='MainContent_RollGridView']/tbody"));
                                        IList<IWebElement> Appricelvaluerow = Apprasialvaluetable.FindElements(By.TagName("tr"));
                                        IList<IWebElement> Appricelvalueid;
                                        foreach (IWebElement appricelvalue in Appricelvaluerow)
                                        {
                                            Appricelvalueid = appricelvalue.FindElements(By.TagName("td"));
                                            if (appricelvalue.Text.Contains("Land"))
                                            {
                                                lane = Appricelvalueid[1].Text;
                                            }
                                            if (appricelvalue.Text.Contains("Improvements"))
                                            {
                                                Improvements = Appricelvalueid[1].Text;
                                            }
                                            if (appricelvalue.Text.Contains("Homestead Exemption"))
                                            {
                                                HomesteadExemption = Appricelvalueid[1].Text;
                                            }

                                            if (appricelvalue.Text.Contains("Taxable Value"))
                                            {
                                                TaxableValue = Appricelvalueid[1].Text;
                                            }
                                        }
                                        IWebElement taxratetable = driver.FindElement(By.XPath("//*[@id='MainContent_TaxGridView']/tbody"));
                                        IList<IWebElement> Taxratrow = taxratetable.FindElements(By.TagName("tr"));
                                        IList<IWebElement> taxratrowid;
                                        foreach (IWebElement Taxtar in Taxratrow)
                                        {
                                            taxratrowid = Taxtar.FindElements(By.TagName("td"));
                                            if (Taxtar.Text.Contains("Tax Levied"))
                                            {
                                                TaxLevied = taxratrowid[1].Text;
                                            }
                                            if (Taxtar.Text.Contains("Payments Applied To Taxes"))
                                            {
                                                PaymentsApplied = taxratrowid[1].Text;
                                            }
                                            if (Taxtar.Text.Contains("Tax Year"))
                                            {
                                                Tax_Year_Balance = taxratrowid[1].Text;
                                            }

                                        }
                                        Taxrate = gc.Between(taxratetable.Text, "Tax Rate", "Tax Levied").Trim();
                                        string taxmudresult = Jurisdiction + "~" + Owner_Name + "~" + FullOwnerAddress + "~" + InquiryDate + "~" + DelinquentDate + "~" + CadNo + "~" + TaxYear + "~" + JurisdictionCode + "~" + Acreage + "~" + strLegalDescription + "~" + FullPropertyAddress + "~" + lane + "~" + Improvements + "~" + HomesteadExemption + "~" + TaxableValue + "~" + Taxrate + "~" + TaxLevied + "~" + PaymentsApplied + "~" + Tax_Year_Balance + "~" + Tax_Authority;
                                        gc.insert_date(orderNumber, Account, 1174, taxmudresult, 1, DateTime.Now);
                                        gc.CreatePdf(orderNumber, Account, "Property detail MUD4" + TaxYear, driver, "TX", "Fort Bend");
                                        IWebElement Currenttaxduetable = driver.FindElement(By.XPath("//*[@id='MainContent_DueGridView']/tbody"));
                                        IList<IWebElement> currenttaxduerow = Currenttaxduetable.FindElements(By.TagName("tr"));
                                        IList<IWebElement> Currenttaxdueid;
                                        foreach (IWebElement currenttaxdue in currenttaxduerow)
                                        {
                                            Currenttaxdueid = currenttaxdue.FindElements(By.TagName("td"));
                                            if (Currenttaxdueid.Count != 0 && !currenttaxdue.Text.Contains("Tax Year"))
                                            {
                                                string CurrentresultMud = Currenttaxdueid[0].Text + "~" + Currenttaxdueid[1].Text + "~" + Currenttaxdueid[2].Text;
                                                gc.insert_date(orderNumber, Account, 1175, CurrentresultMud, 1, DateTime.Now);
                                            }
                                        }
                                        try
                                        {
                                            IWebElement Taxrecipt = driver.FindElement(By.Id("MainContent_TaxReceiptHyperLink"));
                                            string Taxrecipthref = Taxrecipt.GetAttribute("href");
                                            driver.Navigate().GoToUrl(Taxrecipthref);
                                            Thread.Sleep(5000);
                                            gc.CreatePdf(orderNumber, Account, "Tax Recipt" + TaxYear, driver, "TX", "Fort Bend");
                                            driver.Navigate().Back();
                                            Thread.Sleep(1000);
                                        }
                                        catch { }
                                        //Download
                                        try
                                        {
                                            IWebElement downloadMud = driver.FindElement(By.Id("MainContent_TaxStatementHyperLink"));
                                            string Downloadhref = downloadMud.GetAttribute("href");
                                            string fileName = "Statement.pdf";
                                            var chromeOptions = new ChromeOptions();
                                            var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];
                                            chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                                            chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                                            chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");

                                            var chDriver = new ChromeDriver(chromeOptions);
                                            try
                                            {
                                                chDriver.Navigate().GoToUrl(Downloadhref);
                                                Thread.Sleep(4000);
                                                chDriver.FindElement(By.Id("imagebutton2")).Click();
                                                Thread.Sleep(9000);
                                                gc.AutoDownloadFileSpokane(orderNumber, Account, "Fort Bend", "TX", fileName);
                                            }
                                            catch { }
                                            chDriver.Quit();
                                        }
                                        catch { }
                                    }
                                }
                                catch { }

                            }
                            catch { }
                        }

                        //6) MUD Scenario : Blueridge West MUD Tax.
                        //"M136","M172","M213","M228","M263","M50","M52","M54","M58","M96","W01","W30","W32"
                        // string owneraddress = "";
                        if (msg1 == "M213" || msg1 == "M24" || msg1 == "W22" || msg1 == "W07" || msg1 == "M94" || msg1 == "M87" || msg1 == "M77" || msg1 == "M75" || msg1 == "M73" || msg1 == "M62" || msg1 == "M264" || msg1 == "M245" || msg1 == "M23" || msg1 == "M197" || msg1 == "M177" || msg1 == "M163" || msg1 == "M136" || msg1 == "M172" || msg1 == "M213" || msg1 == "M228" || msg1 == "M263" || msg1 == "M50" || msg1 == "M52" || msg1 == "M54" || msg1 == "M58" || msg1 == "M96" || msg1 == "W01" || msg1 == "W30" || msg1 == "W32")

                        {
                            //driver = new PhantomJSDriver();
                            List<string> Downloadstring = new List<string>();
                            try
                            {
                                driver.Navigate().GoToUrl("http://bli-tax.com/records/");
                                Account = Account.Substring(0, 16).Trim();
                                string NumberAccount = Account;
                                driver.FindElement(By.Id("cadnumber")).SendKeys(NumberAccount);
                                driver.FindElement(By.XPath("//*[@id='cadno']/p[3]/input")).Click();
                                Thread.Sleep(2000);
                                gc.CreatePdf(orderNumber, ParcelID, "Mud Scenario Details", driver, "TX", "Fort Bend");
                                //IWebElement Multyaddresstable1 = driver.FindElement(By.XPath("/html/body/div[1]/iframe"));
                                //driver.SwitchTo().Frame(Multyaddresstable1);
                                IWebElement Parcelclicktable = driver.FindElement(By.XPath("//*[@id='post-2168']/table/tbody"));
                                IList<IWebElement> Parcelclickrow = Parcelclicktable.FindElements(By.TagName("tr"));
                                IList<IWebElement> parcelclickid;
                                foreach (IWebElement parcelclick in Parcelclickrow)
                                {
                                    parcelclickid = parcelclick.FindElements(By.TagName("td"));
                                    if (parcelclickid.Count != 0 && !parcelclick.Text.Contains("CAD Number"))
                                    {
                                        IWebElement carnumberclcik = parcelclickid[0].FindElement(By.TagName("a"));
                                        string cardnumberhref = carnumberclcik.GetAttribute("href");
                                        driver.Navigate().GoToUrl(cardnumberhref);
                                        Thread.Sleep(2000);
                                        break;
                                    }
                                }
                                //string year = driver.FindElement(By.Id("years")).Text;

                                for (int p = 0; p < 3; p++)
                                {
                                    string appricelresult = "", Exemptionhead = "", QualifiedExemptionsResult = "", Current_AsOf = "";
                                    string appricelresult1 = "", appricelhead = "", ExemptValuesResult = "", ExemptValuesHead = "";

                                    IWebElement mySelectElement = driver.FindElement(By.Id("years"));
                                    SelectElement dropdown = new SelectElement(mySelectElement);
                                    dropdown.SelectByIndex(p);
                                    Thread.Sleep(2000);
                                    IWebElement slelectyear = driver.FindElement(By.Id("years"));
                                    SelectElement dropdown1 = new SelectElement(slelectyear);
                                    string year = dropdown1.SelectedOption.Text;
                                    string propertydetail = driver.FindElement(By.XPath("//*[@id='taxform']/tbody/tr[1]/td/table/tbody")).Text;
                                    string ownernamepro = GlobalClass.Before(propertydetail, "Make Checks Payable To:");
                                    string[] ownerarray = ownernamepro.Split('\r');
                                    string ownername1 = ownerarray[0];
                                    owneraddress = ownerarray[1].Replace("\n", "") + " " + ownerarray[2].Replace("\n", "");
                                    if (propertydetail.Contains("Current As Of"))
                                    {
                                        Current_AsOf = gc.Between(propertydetail, "Current As Of", "Account Number");
                                    }
                                    else
                                    {
                                        Current_AsOf = "";
                                    }
                                    string AccountNumber = gc.Between(propertydetail, "Account Number", "CAD Number");
                                    string CADNumber = GlobalClass.After(propertydetail, "CAD Number").Trim();
                                    string checktable = driver.FindElement(By.XPath("//*[@id='taxform']/tbody/tr[1]/td/table/tbody/tr/td[1]/p[3]")).Text;
                                    string Checkpayble = GlobalClass.After(checktable.Replace("\r\n", ""), "Make Checks Payable To:");
                                    string Legaltable = driver.FindElement(By.XPath("//*[@id='taxform']/tbody/tr[2]/td[1]/table[1]/tbody")).Text;
                                    string legaldescription = GlobalClass.After(Legaltable, "Property Description");

                                    try
                                    {
                                        IWebElement appricelvaluetable = driver.FindElement(By.XPath("//*[@id='taxform']/tbody/tr[2]/td[1]/table[2]/tbody"));
                                        IList<IWebElement> appricelvaluerow = appricelvaluetable.FindElements(By.TagName("tr"));
                                        IList<IWebElement> appricelvalueid;
                                        foreach (IWebElement appricelvalue in appricelvaluerow)
                                        {
                                            appricelvalueid = appricelvalue.FindElements(By.TagName("td"));
                                            if (appricelvalueid.Count != 0 && !appricelvalue.Text.Contains("Appraised Values"))
                                            {
                                                appricelhead += appricelvalueid[0].Text + "~";
                                                appricelresult += appricelvalueid[1].Text + "~";
                                            }
                                        }
                                    }
                                    catch { }
                                    try
                                    {
                                        IWebElement QualifiedExemptionsTable = driver.FindElement(By.XPath("//*[@id='taxform']/tbody/tr[2]/td[1]/table[3]/tbody"));
                                        IList<IWebElement> QualifiedExemptionsrow = QualifiedExemptionsTable.FindElements(By.TagName("tr"));
                                        IList<IWebElement> QualifiedExemptionsid;
                                        foreach (IWebElement QualifiedExemptions in QualifiedExemptionsrow)
                                        {
                                            QualifiedExemptionsid = QualifiedExemptions.FindElements(By.TagName("td"));

                                            if (QualifiedExemptionsid.Count != 0 && !QualifiedExemptions.Text.Contains("Qualified"))
                                            {
                                                QualifiedExemptionsResult = QualifiedExemptionsid[0].Text;
                                            }
                                        }
                                    }
                                    catch { }
                                    try
                                    {
                                        IWebElement ExemptValueTable = driver.FindElement(By.XPath("//*[@id='taxform']/tbody/tr[2]/td[1]/table[4]/tbody"));
                                        IList<IWebElement> ExemptValuesrow = ExemptValueTable.FindElements(By.TagName("tr"));
                                        IList<IWebElement> ExemptValuesid;
                                        foreach (IWebElement ExemptValues in ExemptValuesrow)
                                        {
                                            ExemptValuesid = ExemptValues.FindElements(By.TagName("td"));
                                            if (ExemptValuesid.Count != 0 && ExemptValues.Text.Contains("Tax Rate"))
                                            {
                                                ExemptValuesHead = ExemptValuesid[0].Text + "~" + ExemptValuesid[1].Text + "~" + ExemptValuesid[2].Text + "~" + ExemptValuesid[3].Text;
                                            }
                                            if (ExemptValuesid.Count != 0 && !ExemptValues.Text.Contains("Exempt Value"))
                                            {
                                                ExemptValuesResult = ExemptValuesid[0].Text + "~" + ExemptValuesid[1].Text + "~" + ExemptValuesid[2].Text + "~" + ExemptValuesid[3].Text;
                                            }
                                        }
                                    }
                                    catch { }
                                    try
                                    {
                                        IWebElement Exemptionstable = driver.FindElement(By.XPath("//*[@id='taxform']/tbody/tr[2]/td[1]/table[5]/tbody"));
                                        IList<IWebElement> Exemptionsrow = Exemptionstable.FindElements(By.TagName("tr"));
                                        IList<IWebElement> Exemptionsid;
                                        IList<IWebElement> Exemptionshead;
                                        foreach (IWebElement Exemptions1 in Exemptionsrow)
                                        {
                                            Exemptionsid = Exemptions1.FindElements(By.TagName("td"));
                                            Exemptionshead = Exemptions1.FindElements(By.TagName("th"));
                                            if (Exemptions1.Text.Contains("Homestead"))
                                            {
                                                Exemptionhead = Exemptionshead[0].Text + "~" + Exemptionshead[1].Text + "~" + Exemptionshead[2].Text;
                                            }
                                            if (Exemptionsid.Count != 0 && !Exemptions1.Text.Contains("Homestead"))
                                            {
                                                appricelresult1 = Exemptionsid[0].Text + "~" + Exemptionsid[1].Text + "~" + Exemptionsid[2].Text;
                                            }
                                        }
                                    }
                                    catch { }

                                    if (p == 0)
                                    {
                                        string Propertyfullhead = "Year" + "~" + "Owner Name" + "~" + "Owner Address" + "~" + "Current As Of" + "~" + "Account Number" + "~" + "CAD Number" + "~" + "Property Description" + "~" + appricelhead + Exemptionhead + "~" + "Qualified Exemptions" + "~" + "Exempt Value" + "~" + "Taxable Value" + "~" + "Tax Rate" + "~" + "Taxes" + "~" + "Make Checks Payable To";
                                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + Propertyfullhead + "' where Id = '" + 1165 + "'");
                                    }
                                    string propertyfullresult = year + "~" + ownername1 + "~" + owneraddress + "~" + Current_AsOf + "~" + AccountNumber + "~" + CADNumber + "~" + legaldescription + "~" + appricelresult + appricelresult1 + "~" + QualifiedExemptionsResult + "~" + ExemptValuesResult + "~" + Checkpayble;
                                    gc.insert_date(orderNumber, AccountNumber, 1165, propertyfullresult, 1, DateTime.Now);
                                    gc.CreatePdf(orderNumber, ParcelID, "Mud Scenario Details Table" + p, driver, "TX", "Fort Bend");

                                    //7th Scenario For MUD

                                    //Tax summary
                                    string Taxsummaryresult = "";
                                    IWebElement Taxsummarytable = driver.FindElement(By.XPath("//*[@id='taxform']/tbody/tr[2]/td[2]/table[1]/tbody"));
                                    IList<IWebElement> Taxsummaryrow = Taxsummarytable.FindElements(By.TagName("tr"));
                                    IList<IWebElement> taxsummaryid;
                                    foreach (IWebElement Taxsummary in Taxsummaryrow)
                                    {
                                        taxsummaryid = Taxsummary.FindElements(By.TagName("td"));
                                        if (taxsummaryid.Count != 0 && !Taxsummary.Text.Contains("Tax Summary"))
                                        {

                                            Taxsummaryresult = year + "~" + taxsummaryid[0].Text + "~" + taxsummaryid[1].Text;
                                            gc.insert_date(orderNumber, AccountNumber, 1170, Taxsummaryresult, 1, DateTime.Now);
                                        }

                                    }


                                    Taxsummaryresult = "";
                                    try
                                    {
                                        IWebElement Payingtable = driver.FindElement(By.XPath("//*[@id='taxform']/tbody/tr[2]/td[2]/table[2]/tbody"));
                                        IList<IWebElement> Payingrow = Payingtable.FindElements(By.TagName("tr"));
                                        IList<IWebElement> payingid;
                                        foreach (IWebElement Paying in Payingrow)
                                        {
                                            payingid = Paying.FindElements(By.TagName("td"));
                                            if (payingid.Count != 0)
                                            {
                                                string Payingresult = year + "~" + payingid[0].Text + "~" + payingid[1].Text + "~" + payingid[2].Text + "~" + payingid[3].Text + "~" + payingid[4].Text;
                                                gc.insert_date(orderNumber, AccountNumber, 1171, Payingresult, 1, DateTime.Now);
                                                Payingresult = "";
                                            }
                                        }
                                    }
                                    catch { }
                                    try
                                    {
                                        string Callalert = driver.FindElement(By.XPath("//*[@id='taxform']/tbody/tr[2]/td[2]/p[1]")).Text;
                                        if (Callalert.Contains("account information"))
                                        {
                                            string Alertmessage = "For tax amount due, you must call the MUD tax Collector's Office.";
                                            gc.insert_date(orderNumber, AccountNumber, 1173, Alertmessage, 1, DateTime.Now);
                                            Alertmessage = "";
                                        }

                                    }
                                    catch { }

                                    IWebElement Parceldownload = driver.FindElement(By.XPath("//*[@id='taxform-header']/div[3]/div[2]/a"));
                                    string Parcelhref = Parceldownload.GetAttribute("href");
                                    //Parceldownload.Click();
                                    //Thread.Sleep(2000);
                                    Downloadstring.Add(Parcelhref);

                                    // gc.downloadfile(Parcelhref, orderNumber, ParcelID, "Recept M.U.D 7" + p, "TX", "Fort Bend");
                                }
                                int Re = 0;
                                foreach (string receipt in Downloadstring)
                                {
                                    driver.Navigate().GoToUrl(receipt);
                                    Thread.Sleep(2000);
                                    gc.CreatePdf(orderNumber, ParcelID, "Recept M.U.D 7" + Re, driver, "TX", "Fort Bend");
                                    Re++;
                                }
                            }
                            catch { }
                        }

                        //8) MUD Scenario : Assessments of The Southwest Inc(ASW). 
                        //9) Scenario for Delinquent Taxes (Assessments of The Southwest Inc (ASW))
                        //"M160","M180","M181","M182","M196","M199","M20","M217","M22","M225","M226","M290","M230","M231","M232","M233","M234","M246","M248","M262","M266","M273","M275","M29","M40","M41","M42","M55","M76","W06","W13"

                        if (msg1 == "M194" || msg1 == "M19" || msg1 == "M17" || msg1 == "M120" || msg1 == "M160" || msg1 == "M180" || msg1 == "M181" || msg1 == "M182" || msg1 == "M196" || msg1 == "M199" || msg1 == "M20" || msg1 == "M217" || msg1 == "M22" || msg1 == "M225" || msg1 == "M226" || msg1 == "M290" || msg1 == "M230" || msg1 == "M231" || msg1 == "M206" || msg1 == "M232" || msg1 == "M233" || msg1 == "M234" || msg1 == "M246" || msg1 == "M248" || msg1 == "M261" || msg1 == "M262" || msg1 == "M266" || msg1 == "M273" || msg1 == "M275" || msg1 == "M276" || msg1 == "M289" || msg1 == "M29" || msg1 == "M40" || msg1 == "M41" || msg1 == "M42" || msg1 == "M55" || msg1 == "M76" || msg1 == "M81" || msg1 == "M83" || msg1 == "M84" || msg1 == "W06" || msg1 == "W13")

                        {
                            string TaxAuthority = "";
                            string PropertyID = "", GeographicID = "", exemption = "";
                            //try
                            //{
                            //    driver.Navigate().GoToUrl("http://www.aswtax.com/contact-us");
                            //    string Taxelement = driver.FindElement(By.XPath("//*[@id='wrap']/div[3]/div[1]/div/div/div/div[1]")).Text;
                            //    TaxAuthority = GlobalClass.After(Taxelement, "Assessments of the Southwest, Inc.");
                            ////}
                            //catch { }
                            try
                            {
                                driver.Navigate().GoToUrl("http://aswportal.azurewebsites.net/search/list");
                                Thread.Sleep(2000);
                                driver.FindElement(By.Id("tag")).SendKeys(owneraddress);
                                gc.CreatePdf(orderNumber, ParcelID, " SD3-Property Search", driver, "TX", "Fort Bend");
                                driver.FindElement(By.XPath("//*[@id='tag']")).SendKeys(Keys.Enter);
                                Thread.Sleep(2000);
                                gc.CreatePdf(orderNumber, ParcelID, " SD3-Property Search Result", driver, "TX", "Fort Bend");
                                IWebElement ITaxClick = driver.FindElement(By.XPath("//*[@id='searchlist']/div/div/div/div/table"));
                                IList<IWebElement> ITaxRow = ITaxClick.FindElements(By.TagName("tr"));
                                foreach (IWebElement tax in ITaxRow)
                                {
                                    if (tax.Text.Contains(PropertyID.Trim().Replace("-", "")))
                                    {
                                        string strClick = tax.GetAttribute("id");
                                        if (strClick.Contains("rowid"))
                                        {
                                            IWebElement Iclick = driver.FindElement(By.Id(strClick));
                                            Iclick.Click();
                                            Thread.Sleep(2000);
                                        }
                                    }
                                }

                                // Tax Details (Tax Summary)

                                string TaxSummary = driver.FindElement(By.XPath("//*[@id='cardlist']/div/div[1]/div/div[2]/div[1]/div/table")).Text;
                                string PropertyOwnership = "", PropId = "", GeoId = "", Situs = "", LegalDesc = "", SITUS = "";
                                PropertyOwnership = GlobalClass.After(TaxSummary, "PROPERTY OWNERSHIP");
                                string Peoprtyinfo = driver.FindElement(By.XPath("//*[@id='cardlist']/div/div[1]/div/div[3]/div[1]/div/table/tbody")).Text;
                                PropId = gc.Between(Peoprtyinfo, "PROP ID:", "GEOID:").Trim();
                                GeoId = gc.Between(Peoprtyinfo, "GEOID:", "SITUS:");
                                Situs = GlobalClass.After(Peoprtyinfo, "SITUS:");
                                string TaxSummary2 = driver.FindElement(By.XPath("//*[@id='cardlist']/div/div[1]/div/div[3]/div[2]/div/table/tbody/tr/td")).Text;
                                LegalDesc = GlobalClass.After(TaxSummary2, "LEGAL:").Trim();
                                try
                                {
                                    exemption = driver.FindElement(By.XPath("//*[@id='cardlist']/div/div[1]/div/div[3]/div[3]/div/table/tbody/tr/td")).Text;
                                }
                                catch { }
                                TaxAuthority = "PALMER PLANTATION M.U.D. #1 P.O. BOX 1368 FRIENDSWOOD TX,77549-1368 PHONE:281-482-0216";
                                string TaxDetails = PropertyOwnership + "~" + PropId + "~" + GeoId + "~" + Situs + "~" + LegalDesc + "~" + exemption + "~" + TaxAuthority;
                                gc.insert_date(orderNumber, ParcelID, 1176, TaxDetails, 1, DateTime.Now);
                                gc.CreatePdf(orderNumber, ParcelID, "SD3-Tax Summary", driver, "TX", "Fort Bend");

                                // Payment Details Table
                                IWebElement TaxPayment = driver.FindElement(By.XPath("//*[@id='cardlist']/div/div[1]/div/div[2]/div[3]/div/table/tbody"));
                                IList<IWebElement> TRTaxPayment = TaxPayment.FindElements(By.TagName("tr"));
                                IList<IWebElement> THTaxPayment = TaxPayment.FindElements(By.TagName("th"));
                                IList<IWebElement> TDTaxPayment;
                                foreach (IWebElement row in TRTaxPayment)
                                {
                                    TDTaxPayment = row.FindElements(By.TagName("td"));
                                    //THTaxPayment = row.FindElements(By.TagName("th"));
                                    if (TDTaxPayment.Count != 0 && !row.Text.Contains("AMOUNT DUE"))
                                    {
                                        string TaxPaymentDetails = TDTaxPayment[0].Text + "~" + TDTaxPayment[1].Text + "~" + TDTaxPayment[2].Text;
                                        gc.insert_date(orderNumber, ParcelID, 1177, TaxPaymentDetails, 1, DateTime.Now);
                                    }
                                }

                                // Exemptions
                                string ExemptionDetails1 = "", ExemptionDetails2 = "", TaxValuation1 = "", TaxValuation2 = "";
                                IWebElement TaxDue8 = driver.FindElement(By.XPath("//*[@id='cardlist']/div/div[1]/div/div[4]/div/div/table"));
                                IList<IWebElement> TRTaxDue8 = TaxDue8.FindElements(By.TagName("tr"));
                                IList<IWebElement> THTaxDue8 = TaxDue8.FindElements(By.TagName("th"));
                                IList<IWebElement> TDTaxDue8;
                                IList<IWebElement> Thtaxdue8;
                                foreach (IWebElement row in TRTaxDue8)
                                {
                                    TDTaxDue8 = row.FindElements(By.TagName("td"));
                                    Thtaxdue8 = row.FindElements(By.TagName("th"));
                                    if (TDTaxDue8.Count != 0 && !row.Text.Contains("TAXING ENTITIES") && !row.Text.Contains("\r\n") && !row.Text.Contains("============"))
                                    {
                                        string TaxDueDetails = TDTaxDue8[0].Text + "~" + TDTaxDue8[1].Text + "~" + TDTaxDue8[2].Text + "~" + TDTaxDue8[3].Text + "~" + TDTaxDue8[4].Text + "~" + TDTaxDue8[5].Text + "~" + TDTaxDue8[6].Text + "~" + TDTaxDue8[7].Text + "~" + TDTaxDue8[8].Text;
                                        gc.insert_date(orderNumber, ParcelID, 1178, TaxDueDetails, 1, DateTime.Now);
                                    }
                                    if (TDTaxDue8.Count != 0 && !row.Text.Contains("TAXING ENTITIES") && row.Text.Contains("\r\n") && !row.Text.Contains("============"))
                                    {
                                        string[] strTaxYearSplit = TDTaxDue8[0].Text.Split('\r');
                                        string[] strTaxEntSplit = TDTaxDue8[1].Text.Split('\r');
                                        string[] strTaxExeSplit = TDTaxDue8[2].Text.Split('\r');
                                        string[] strTaxableSplit = TDTaxDue8[3].Text.Split('\r');
                                        string[] strTaxRateSplit = TDTaxDue8[4].Text.Split('\r');
                                        string[] strTaxAmtSplit = TDTaxDue8[5].Text.Split('\r');
                                        string[] strTaxDueSplit = TDTaxDue8[6].Text.Split('\r');
                                        string[] strTaxAddnSplit = TDTaxDue8[7].Text.Split('\r');
                                        string[] strTaxTotalSplit = TDTaxDue8[8].Text.Split('\r');
                                        for (int Due = 0; Due < strTaxYearSplit.Count(); Due++)
                                        {
                                            string TaxDueDetails = strTaxYearSplit[Due].Replace("\n", "") + "~" + strTaxEntSplit[Due].Replace("\n", "") + "~" + strTaxExeSplit[Due].Replace("\n", "") + "~" + strTaxableSplit[Due].Replace("\n", "") + "~" + strTaxRateSplit[Due].Replace("\n", "") + "~" + strTaxAmtSplit[Due].Replace("\n", "") + "~" + strTaxDueSplit[Due].Replace("\n", "") + "~" + strTaxAddnSplit[Due].Replace("\n", "") + "~" + strTaxTotalSplit[Due].Replace("\n", "");
                                            gc.insert_date(orderNumber, ParcelID, 1178, TaxDueDetails, 1, DateTime.Now);
                                        }

                                    }
                                }
                                // TaxValuations
                                try
                                {
                                    IWebElement TaxValuation = driver.FindElement(By.XPath("//*[@id='cardlist']/div/div[1]/div/div[3]/div[4]/div/table"));
                                    IList<IWebElement> TRTaxValuation = TaxValuation.FindElements(By.TagName("tr"));
                                    IList<IWebElement> THTaxValuation = TaxValuation.FindElements(By.TagName("th"));
                                    IList<IWebElement> TDTaxValuation;
                                    foreach (IWebElement row in TRTaxValuation)
                                    {
                                        TDTaxValuation = row.FindElements(By.TagName("td"));
                                        //THTaxValuation = row.FindElements(By.TagName("th"));
                                        if (TDTaxValuation.Count != 0)
                                        {
                                            ExemptionDetails1 = TDTaxValuation[0].Text.Replace("\r\n", "~").Trim();
                                            ExemptionDetails2 = TDTaxValuation[1].Text.Replace("\r\n", "~").Trim();
                                        }
                                    }
                                    ExemptionDetails1 += TaxValuation1;
                                    ExemptionDetails2 += TaxValuation2;
                                    DBconnection dbconn = new DBconnection();
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + ExemptionDetails1 + "' where Id = '" + 1179 + "'");
                                    gc.insert_date(orderNumber, ParcelID, 1179, ExemptionDetails2, 1, DateTime.Now);
                                }
                                catch { }
                                //STATEMENTS
                                driver.FindElement(By.LinkText("STATEMENTS")).Click();
                                Thread.Sleep(2000);
                                gc.CreatePdf(orderNumber, ParcelID, "Statement MUD Scenario 8 & 9 ", driver, "TX", "Fort Bend");
                                // Tax Due Details
                                List<string> ParcelSearch = new List<string>();
                                IWebElement TaxDue = driver.FindElement(By.XPath("/html/body/div[2]/section/div/form/div/div/div/div/div[1]/div/div/table/tbody"));
                                IList<IWebElement> TRTaxDue = TaxDue.FindElements(By.TagName("tr"));
                                // IList<IWebElement> THTaxDue = TaxDue.FindElements(By.TagName("th"));
                                IList<IWebElement> TDTaxDue;
                                foreach (IWebElement row in TRTaxDue)
                                {
                                    TDTaxDue = row.FindElements(By.TagName("td"));
                                    //THTaxDue = row.FindElements(By.TagName("th"));
                                    if (TDTaxDue.Count != 0 && !row.Text.Contains("TAXING ENTITIES"))
                                    {
                                        string TaxDueDetails = TDTaxDue[0].Text + "~" + TDTaxDue[1].Text + "~" + TDTaxDue[2].Text + "~" + TDTaxDue[3].Text + "~" + TDTaxDue[4].Text + "~" + TDTaxDue[5].Text + "~" + TDTaxDue[6].Text + "~" + TDTaxDue[7].Text + "~" + TDTaxDue[8].Text + "~" + TDTaxDue[9].Text + "~" + TDTaxDue[10].Text + "~" + TDTaxDue[11].Text + "~" + TDTaxDue[12].Text + "~" + TDTaxDue[13].Text;
                                        gc.insert_date(orderNumber, ParcelID, 1180, TaxDueDetails, 1, DateTime.Now);
                                    }
                                }

                                // RECEIPTS
                                driver.FindElement(By.LinkText("RECEIPTS")).Click();
                                Thread.Sleep(1000);
                                gc.CreatePdf(orderNumber, ParcelID, "SD3-property Receipts", driver, "TX", "Fort Bend");
                                string propRCPT = "", propRCPTDate = "", propRPostDate = "", propRCheck = "", propRPaid1 = "", propRMO = "", propRPaid2 = "", propRCC = "", propRCCType = "";
                                string propRPaid3 = "", propROther = "", propRAmountPaid = "", propRCashPaid = "", propRVoid = "", propRCode = "", propRDate = "";

                                string current1 = driver.CurrentWindowHandle;
                                IWebElement IPropReceipt = driver.FindElement(By.XPath("/html/body/div[2]/section/div/form/div/div/div/div/div/div/div/table/tbody"));
                                IList<IWebElement> IPropReceiptRow = IPropReceipt.FindElements(By.TagName("tr"));
                                IList<IWebElement> IPropReceiptTD;
                                foreach (IWebElement Reciept in IPropReceiptRow)
                                {
                                    IPropReceiptTD = Reciept.FindElements(By.TagName("td"));
                                    if (IPropReceiptTD.Count != 0)
                                    {
                                        propRCPT = IPropReceiptTD[0].Text;
                                        propRCPTDate = IPropReceiptTD[1].Text;
                                        propRPostDate = IPropReceiptTD[2].Text;
                                        propRCheck = IPropReceiptTD[3].Text;
                                        propRPaid1 = IPropReceiptTD[4].Text;
                                        propRMO = IPropReceiptTD[5].Text;
                                        propRPaid2 = IPropReceiptTD[6].Text;
                                        propRCC = IPropReceiptTD[7].Text;
                                        propRCCType = IPropReceiptTD[8].Text;
                                        propRPaid3 = IPropReceiptTD[9].Text;
                                        propROther = IPropReceiptTD[10].Text;
                                        propRAmountPaid = IPropReceiptTD[11].Text;
                                        propRCashPaid = IPropReceiptTD[12].Text;
                                        propRVoid = IPropReceiptTD[13].Text;
                                        propRCode = IPropReceiptTD[14].Text;
                                        // propRDate = IPropReceiptTD[15].Text;

                                        string propDueDetails2 = propRCPT + "~" + propRCPTDate + "~" + propRPostDate + "~" + propRCheck + "~" + propRPaid1 + "~" + propRMO + "~" + propRPaid2 + "~" + propRCC + "~" + propRCCType + "~" + propRPaid3 + "~" + propROther + "~" + propRAmountPaid + "~" + propRCashPaid + "~" + propRVoid + "~" + propRCode;
                                        gc.insert_date(orderNumber, ParcelID, 1181, propDueDetails2, 1, DateTime.Now);
                                    }
                                }
                                try
                                {
                                    string fileName = "";
                                    var chromeOptions = new ChromeOptions();
                                    var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];
                                    chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                                    chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                                    chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                                    chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                                    var chDriver = new ChromeDriver(chromeOptions);
                                    Array.ForEach(Directory.GetFiles(@downloadDirectory), File.Delete); //Delete all the files in auto pdf
                                                                                                        //R000182641
                                    chDriver.Navigate().GoToUrl("http://aswportal.azurewebsites.net/search/list");
                                    Thread.Sleep(2000);
                                    //*[@id="download"]
                                    chDriver.FindElement(By.Id("tag")).SendKeys(owneraddress);
                                    chDriver.FindElement(By.XPath("//*[@id='tag']")).SendKeys(Keys.Enter);
                                    Thread.Sleep(3000);
                                    //int i = 0;
                                    IWebElement ITaxClick1 = chDriver.FindElement(By.XPath("//*[@id='searchlist']/div/div/div/div/table"));
                                    IList<IWebElement> ITaxRow1 = ITaxClick1.FindElements(By.TagName("tr"));
                                    foreach (IWebElement tax1 in ITaxRow1)
                                    {
                                        if (tax1.Text.Contains(PropertyID.Trim().Replace("-", "")))
                                        {
                                            string strClick = tax1.GetAttribute("id");
                                            if (strClick.Contains("rowid"))
                                            {
                                                IWebElement Iclick1 = chDriver.FindElement(By.Id(strClick));
                                                Iclick1.Click();
                                                Thread.Sleep(2000);
                                            }
                                        }
                                    }
                                    Thread.Sleep(5000);
                                    chDriver.FindElement(By.LinkText("STATEMENTS")).Click();
                                    Thread.Sleep(2000);
                                    string currentch = chDriver.CurrentWindowHandle;
                                    IWebElement ParcelTB = chDriver.FindElement(By.XPath("/html/body/div[2]/section/div/form/div/div/div/div/div[1]/div/div/table/tbody"));
                                    IList<IWebElement> ParcelTR = ParcelTB.FindElements(By.TagName("tr"));
                                    ParcelTR.Reverse();
                                    int rows_count = ParcelTR.Count;

                                    for (int row = 0; row < rows_count; row++)
                                    {
                                        if (row == rows_count - 3 || row == rows_count - 1 || row == rows_count - 2)
                                        {
                                            IList<IWebElement> Columns_row = ParcelTR[row].FindElements(By.TagName("td"));
                                            int columns_count = Columns_row.Count;
                                            if (columns_count != 0)
                                            {
                                                IWebElement ParcelBill_link = Columns_row[1];
                                                ParcelBill_link.Click();
                                                Thread.Sleep(3000);
                                                chDriver.SwitchTo().Window(chDriver.WindowHandles.Last());
                                                IWebElement ParcelTB11 = chDriver.FindElement(By.Id("download"));
                                                ParcelTB11.Click();
                                                Thread.Sleep(5000);
                                                var files = new DirectoryInfo(downloadDirectory).GetFiles("*.*");
                                                string latestfile = "";
                                                DateTime lastupdated = DateTime.MinValue;
                                                foreach (FileInfo file in files)
                                                {
                                                    if (file.LastWriteTime > lastupdated)
                                                    {
                                                        lastupdated = file.LastWriteTime;
                                                        latestfile = file.Name;
                                                    }

                                                }
                                                gc.AutoDownloadFile(orderNumber, ParcelID, "Fort Bend", "TX", latestfile);
                                                chDriver.SwitchTo().Window(currentch);
                                            }
                                        }
                                    }
                                    chDriver.SwitchTo().Window(currentch);
                                    string current = chDriver.CurrentWindowHandle;
                                    chDriver.FindElement(By.LinkText("RECEIPTS")).Click();
                                    Thread.Sleep(1000);
                                    string chcurrent1 = chDriver.CurrentWindowHandle;
                                    int i = 0;
                                    IWebElement IPropReceipt1 = chDriver.FindElement(By.XPath("/html/body/div[2]/section/div/form/div/div/div/div/div/div/div/table/tbody"));
                                    IList<IWebElement> IPropReceiptRow1 = IPropReceipt1.FindElements(By.TagName("tr"));
                                    IList<IWebElement> IPropReceiptTD1;
                                    foreach (IWebElement Reciept1 in IPropReceiptRow1)
                                    {
                                        if (i < 3)
                                        {
                                            IPropReceiptTD1 = Reciept1.FindElements(By.TagName("td"));
                                            if (IPropReceiptTD1.Count != 0)
                                            {

                                                IWebElement linktext = IPropReceiptTD1[0];
                                                linktext.Click();
                                                Thread.Sleep(2000);

                                                chDriver.SwitchTo().Window(chDriver.WindowHandles.Last());
                                                string url = chDriver.Url;
                                                Thread.Sleep(5000);
                                                // gc.downloadfile(url, orderNumber, ParcelID, " tax bill" + i, "TX", "Fort Bend");
                                                gc.CreatePdf(orderNumber, ParcelID, "Tax bil Pdf" + i, chDriver, "TX", "Fort Bend");
                                                chDriver.Close();
                                                chDriver.SwitchTo().Window(chcurrent1);
                                                i++;
                                            }
                                        }
                                    }
                                    chDriver.Quit();
                                }
                                catch { }

                            }
                            catch { }
                        }
                    }
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "TX", "Fort Bend", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    GlobalClass.titleparcel = "";
                    gc.mergpdf(orderNumber, "TX", "Fort Bend");
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
