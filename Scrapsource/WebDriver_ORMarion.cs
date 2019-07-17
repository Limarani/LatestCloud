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
    public class WebDriver_ORMarion
    {
        string parcelno = "";
        string outputPath = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();
        string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_Marion(string houseno, string sname, string sttype, string account, string parcelNumber, string searchType, string orderNumber, string ownername, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;

            GlobalClass.global_parcelNo = parcelNumber;
            string ParcellNumber = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                string Address = houseno + " " + sname;
                string AccountNo = "", OwnerName = "", MapTaxLot = "", SitusAddress = "", LegalDescription = "";
                string[] stringSeparators1 = new string[] { "\r\n" };
                List<string> listurl = new List<string>();
                string Date = "";
                Date = DateTime.Now.ToString("M/d/yyyy");
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", Address, "OR", "Marion");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_ORMarion"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("http://mcasr.co.marion.or.us/");
                        Thread.Sleep(2000);
                        driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Accept']")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        var SerachCategory = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_ddlstSearchCategory']"));
                        var selectElement1 = new SelectElement(SerachCategory);
                        selectElement1.SelectByText("Situs Address");
                        Thread.Sleep(3000);
                        driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_txtbSearchCriteria']")).SendKeys(Address);
                        Thread.Sleep(1000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "OR", "Marion");
                        driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Search']")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "OR", "Marion");

                        string Multi = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblSearchMessage']")).Text;
                        try
                        {
                            string MultiCount = GlobalClass.Before(Multi, " property found.");
                            if ((Convert.ToInt32(MultiCount)) > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Marion_Count"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }
                        if (Multi == "1 property found.")
                        {


                        }
                        else if (Multi == "0 properties found.\r\nPlease modify your search criteria.")
                        {
                            HttpContext.Current.Session["Nodata_ORMarion"] = "Yes";
                            driver.Quit();
                            return "No Record Found";
                        }
                        else
                        {
                            string Sc = "";
                            string Sc1 = "";
                            int I = 0;
                            IList<IWebElement> URL;

                            IWebElement MultiAddressTB = driver.FindElement(By.XPath("//*[@id='form1']/div[4]"));
                            IList<IWebElement> MultiAddressTR = MultiAddressTB.FindElements(By.ClassName("row"));
                            IList<IWebElement> MultiAddressTD;

                            foreach (IWebElement row1 in MultiAddressTR)
                            {
                                MultiAddressTD = row1.FindElements(By.TagName("div"));
                                URL = row1.FindElements(By.TagName("a"));
                                if (URL.Count != 0 && MultiAddressTD.Count != 0 && MultiAddressTD.Count != 1 && MultiAddressTD[0].Text.Trim() != "Account No." && !MultiAddressTD[0].Text.Trim().Contains("Find Property on Map") && !MultiAddressTD[0].Text.Trim().Contains("Select a Category") && MultiAddressTD[0].Text.Trim() != "" || MultiAddressTD.Count == 9)
                                {

                                    string Link = URL[0].GetAttribute("href");
                                    listurl.Add(Link);
                                    AccountNo = MultiAddressTD[1].Text;
                                    OwnerName = MultiAddressTD[3].Text;
                                    string[] linesName = OwnerName.Split(stringSeparators1, StringSplitOptions.None);
                                    try { OwnerName = linesName[0] + linesName[1] + linesName[2]; }
                                    catch { }
                                    try { OwnerName = linesName[0] + linesName[1] + linesName[2] + linesName[3]; }
                                    catch { }

                                    parcelNumber = MultiAddressTD[5].Text;
                                    SitusAddress = MultiAddressTD[7].Text;
                                    string[] linesTax = SitusAddress.Split(stringSeparators1, StringSplitOptions.None);
                                    SitusAddress = linesTax[0] + linesTax[1];
                                    LegalDescription = MultiAddressTD[8].Text;
                                    string MutiParcel = parcelNumber + "~" + OwnerName + "~" + SitusAddress + "~" + LegalDescription;
                                    gc.insert_date(orderNumber, AccountNo, 154, MutiParcel, 1, DateTime.Now);

                                    if (I == 0)
                                    {
                                        Sc = OwnerName;
                                        I++;

                                    }
                                    else if (I == 1)
                                    {
                                        Sc1 = OwnerName;
                                        I++;

                                    }
                                    if (Sc == Sc1)
                                    {

                                        break;
                                    }

                                    HttpContext.Current.Session["multiParcel_Marion"] = "Yes";


                                }

                            }
                            driver.Quit();
                            return "MultiParcel";



                        }

                    }

                    else if (searchType == "parcel")
                    {

                        driver.Navigate().GoToUrl("http://mcasr.co.marion.or.us/");
                        Thread.Sleep(2000);
                        driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Accept']")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        var SerachCategory = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_ddlstSearchCategory']"));
                        var selectElement1 = new SelectElement(SerachCategory);
                        selectElement1.SelectByText("Map Tax Lot");
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Number Search", driver, "OR", "Marion");
                        driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_txtbSearchCriteria']")).SendKeys(parcelNumber.Replace(" ", ""));
                        Thread.Sleep(1000);
                        driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Search']")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Number Search Result", driver, "OR", "Marion");
                        string Multi = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblSearchMessage']")).Text;

                        try
                        {
                            string MultiCount = GlobalClass.Before(Multi, " property found.");
                            if ((Convert.ToInt32(MultiCount)) > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Marion_Count"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }


                        if (Multi == "1 property found." || Multi == "0 properties found.\r\nPlease modify your search criteria.")
                        {
                            HttpContext.Current.Session["Nodata_ORMarion"] = "Yes";
                            driver.Quit();
                            return "No Record Found";
                        }
                        else
                        {
                            IList<IWebElement> URL;

                            IWebElement MultiAddressTB = driver.FindElement(By.XPath("//*[@id='form1']/div[4]"));
                            IList<IWebElement> MultiAddressTR = MultiAddressTB.FindElements(By.ClassName("row"));
                            IList<IWebElement> MultiAddressTD;

                            foreach (IWebElement row1 in MultiAddressTR)
                            {
                                MultiAddressTD = row1.FindElements(By.TagName("div"));
                                URL = row1.FindElements(By.TagName("a"));
                                if (URL.Count != 0 && MultiAddressTD.Count != 0 && MultiAddressTD.Count != 1 && MultiAddressTD[0].Text.Trim() != "Account No." && !MultiAddressTD[0].Text.Trim().Contains("Find Property on Map") && !MultiAddressTD[0].Text.Trim().Contains("Select a Category") && MultiAddressTD[0].Text.Trim() != "" || MultiAddressTD.Count == 9)
                                {

                                    string Link = URL[0].GetAttribute("href");
                                    listurl.Add(Link);
                                    AccountNo = MultiAddressTD[1].Text;
                                    OwnerName = MultiAddressTD[3].Text;
                                    string[] linesName = OwnerName.Split(stringSeparators1, StringSplitOptions.None);
                                    try { OwnerName = linesName[0] + linesName[1] + linesName[2]; }
                                    catch { }
                                    try { OwnerName = linesName[0] + linesName[1] + linesName[2] + linesName[3]; }
                                    catch { }

                                    parcelNumber = MultiAddressTD[5].Text;
                                    SitusAddress = MultiAddressTD[7].Text;
                                    string[] linesTax = SitusAddress.Split(stringSeparators1, StringSplitOptions.None);
                                    SitusAddress = linesTax[0] + linesTax[1];
                                    LegalDescription = MultiAddressTD[8].Text;
                                    string MutiParcel = parcelNumber + "~" + OwnerName + "~" + SitusAddress + "~" + LegalDescription;
                                    gc.insert_date(orderNumber, AccountNo, 154, MutiParcel, 1, DateTime.Now);


                                }

                            }
                            HttpContext.Current.Session["multiParcel_Marion"] = "Yes";
                            driver.Quit();
                            return "MultiParcel";
                        }



                    }
                    else if (searchType == "block")
                    {


                        driver.Navigate().GoToUrl("http://mcasr.co.marion.or.us/");
                        Thread.Sleep(2000);
                        driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Accept']")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        var SerachCategory = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_ddlstSearchCategory']"));
                        var selectElement1 = new SelectElement(SerachCategory);
                        selectElement1.SelectByText("Account Number");
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Account Number Search", driver, "OR", "Marion");
                        driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_txtbSearchCriteria']")).SendKeys(account);
                        Thread.Sleep(1000);
                        driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Search']")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Account Number Search Result", driver, "OR", "Marion");

                    }
                    IList<IWebElement> Href;

                    IWebElement MultiHrefTB = driver.FindElement(By.XPath("//*[@id='form1']/div[4]"));
                    //*[@id="form1"]/div[4]
                    //*[@id="form1"]/div[4]
                    //*[@id="form1"]/div[4]/div[4]
                    IList<IWebElement> MultiHrefTR = MultiHrefTB.FindElements(By.ClassName("row"));
                    IList<IWebElement> MultiHrefTD;

                    foreach (IWebElement row1 in MultiHrefTR)
                    {
                        MultiHrefTD = row1.FindElements(By.TagName("div"));
                        Href = row1.FindElements(By.TagName("a"));
                        if (Href.Count != 0 && MultiHrefTD.Count != 0 && MultiHrefTD.Count != 1 && MultiHrefTD[0].Text.Trim() != "Account No." && !MultiHrefTD[0].Text.Trim().Contains("Find Property on Map") && !MultiHrefTD[0].Text.Trim().Contains("Select a Category") && MultiHrefTD[0].Text.Trim() != "" || MultiHrefTD.Count == 9)
                        {

                            string Link = Href[0].GetAttribute("href");
                            driver.Navigate().GoToUrl(Link);
                            break;
                        }
                    }

                    string Subdivision = "", YearBuilt = "", LegalAcreage = "", PropertyCode = "", PropertyClass = "", LevyCodeArea = "", Zoning = "", ManufacturedHomeID = "";
                    Thread.Sleep(1000);
                    parcelNumber = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblMapTaxLot']")).Text;
                    AccountNo = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblPropertyID']")).Text;
                    SitusAddress = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblSitusAddress']")).Text;
                    string[] linesSitusAddress = SitusAddress.Split(stringSeparators1, StringSplitOptions.None);
                    SitusAddress = linesSitusAddress[0] + linesSitusAddress[1];
                    ownername = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblOwner']")).Text;
                    try
                    {
                        ManufacturedHomeID = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblManufacturedHomeID']")).Text;
                    }
                    catch { }

                    string[] linesIwnerName = ownername.Split(stringSeparators1, StringSplitOptions.None);

                    try { ownername = linesIwnerName[0] + linesIwnerName[1] + linesIwnerName[2]; }
                    catch { }
                    try { ownername = linesIwnerName[0] + linesIwnerName[1] + linesIwnerName[2] + linesIwnerName[3]; }
                    catch { }
                    LegalDescription = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblLegalDescription']")).Text;
                    Subdivision = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblSubdivision']")).Text;
                    IWebElement ISpan1 = driver.FindElement(By.Id("divPropertyDetails"));
                    IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                    js1.ExecuteScript("arguments[0].click();", ISpan1);
                    Thread.Sleep(3000);
                    try { LegalAcreage = driver.FindElement(By.XPath("//*[@id='divPropertyDetailsBody']/div[1]/div[1]/div[1]/div[2]")).Text; }
                    catch { }
                    try { LegalAcreage = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblLegalAcreage']")).Text; }
                    catch { }
                    try
                    {
                        parcelNumber = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblMapTaxLot']")).Text;
                        PropertyCode = driver.FindElement(By.XPath("//*[@id='divPropertyDetailsBody']/div[1]/div[2]/div[1]/div[2]")).Text;
                        PropertyClass = driver.FindElement(By.XPath("//*[@id='divPropertyDetailsBody']/div[1]/div[2]/div[2]/div[2]")).Text;
                        LevyCodeArea = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblLevyCodeArea']")).Text;
                    }
                    catch { }

                    try
                    {
                        YearBuilt = driver.FindElement(By.XPath("//*[@id='divPropertyDetailsBody']/div[7]/div/table/tbody/tr[2]/td[6]")).Text;
                        Zoning = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblZoning']")).Text;
                    }
                    catch { }


                    string ProperTyDetail = AccountNo + "~" + ownername + "~" + SitusAddress + "~" + LegalDescription + "~" + Subdivision + "~" + LegalAcreage + "~" + PropertyCode + "~" + PropertyClass + "~" + LevyCodeArea + "~" + Zoning + "~" + ManufacturedHomeID + "~" + YearBuilt;
                    gc.insert_date(orderNumber, parcelNumber, 156, ProperTyDetail, 1, DateTime.Now);



                    string RMVLandMarket = "", RMVLandSpecAssess = "", RMVStructures = "", RMVTotal = "", SAV = "", ExceptionRMV = "", ExemptionRMV = "", M5Taxable = "", MAV = "", MSAV = "", AV = "", ExemptionDescription = "";
                    IWebElement Value = driver.FindElement(By.Id("divValueInformation"));

                    js1.ExecuteScript("arguments[0].click();", Value);
                    RMVLandMarket = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblRMVLandMarket']")).Text;
                    RMVLandSpecAssess = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblRMVLandSpecAssess']")).Text;
                    RMVStructures = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblRMVStructures']")).Text;
                    RMVTotal = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblRMVTotal']")).Text;
                    SAV = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblSAV']")).Text;
                    ExceptionRMV = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblExceptionRMV']")).Text;
                    ExemptionRMV = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblExemptionRMV']")).Text;
                    M5Taxable = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblM5Taxable']")).Text;
                    MAV = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblMAV']")).Text;
                    MSAV = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblMSAV']")).Text;
                    AV = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblAV']")).Text;
                    ExemptionDescription = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblExemptiondescription']")).Text;


                    string ValueInformation = RMVLandMarket + "~" + RMVLandSpecAssess + "~" + RMVStructures + "~" + RMVTotal + "~" + SAV + "~" + ExceptionRMV + "~" + ExemptionRMV + "~" + M5Taxable + "~" + MAV + "~" + MSAV + "~" + AV + "~" + ExemptionDescription;
                    gc.insert_date(orderNumber, parcelNumber, 158, ValueInformation, 1, DateTime.Now);
                    IWebElement Assess = driver.FindElement(By.Id("divAssessmentHistory"));
                    js1.ExecuteScript("arguments[0].click();", Assess);
                    Thread.Sleep(1000);
                    int K = 0;
                    string Year = "", ImprovementsRMV = "", LandRMV = "", SpecialMktUse = "", Exemptions = "", TaxableAssessedValue = "";
                    IWebElement MultiAssessTB = driver.FindElement(By.XPath("//*[@id='divAssessmentHistoryBody']/div/div/table/tbody"));
                    IList<IWebElement> MultiAssessTR = MultiAssessTB.FindElements(By.TagName("tr"));
                    IList<IWebElement> MultiAssessTD;

                    foreach (IWebElement row1 in MultiAssessTR)
                    {
                        MultiAssessTD = row1.FindElements(By.TagName("td"));
                        if (MultiAssessTD.Count != 0)
                        {
                            Year = MultiAssessTD[0].Text;
                            ImprovementsRMV = MultiAssessTD[1].Text;
                            LandRMV = MultiAssessTD[2].Text;
                            SpecialMktUse = MultiAssessTD[3].Text;
                            Exemptions = MultiAssessTD[4].Text;
                            TaxableAssessedValue = MultiAssessTD[5].Text;

                            string AssessmentDetail = Year + "~" + ImprovementsRMV + "~" + LandRMV + "~" + SpecialMktUse + "~" + Exemptions + "~" + TaxableAssessedValue;
                            gc.insert_date(orderNumber, parcelNumber, 159, AssessmentDetail, 1, DateTime.Now);
                            K++;
                            if (K == 2)
                            { break; }
                        }
                    }

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    string TransactionID = "_ ", TaxPaid = " _", Discount = " _", Interest = " _", AmountPaid = " _", DatePaid = " _", Norecord = "";
                    string TaxPayMentHistory = "";
                    IWebElement TaxHistory = driver.FindElement(By.Id("divTaxPaymentHistory"));
                    js1.ExecuteScript("arguments[0].click();", TaxHistory);
                    try
                    {
                        string NoResult = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_NoTaxHistoryDetails']")).Text;
                        TaxPayMentHistory = NoResult + "~" + TransactionID + "~" + TaxPaid + "~" + Discount + "~" + Interest + "~" + AmountPaid + "~" + DatePaid;
                        gc.insert_date(orderNumber, parcelNumber, 160, TaxPayMentHistory, 1, DateTime.Now);
                        Norecord = "Yes";
                    }
                    catch { }
                    if (Norecord == "")
                    {

                        try
                        {
                            IWebElement MultiTaxHistoryTB = driver.FindElement(By.XPath("//*[@id='divTaxPaymentHistoryBody']/div/table/tbody"));
                            IList<IWebElement> MultiTaxHistoryTR = MultiTaxHistoryTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiTaxHistoryTD;

                            foreach (IWebElement row1 in MultiTaxHistoryTR)
                            {
                                MultiTaxHistoryTD = row1.FindElements(By.TagName("td"));
                                if (MultiTaxHistoryTD.Count != 0)
                                {
                                    Year = MultiTaxHistoryTD[0].Text;
                                    TransactionID = MultiTaxHistoryTD[1].Text;
                                    TaxPaid = MultiTaxHistoryTD[2].Text;
                                    Discount = MultiTaxHistoryTD[3].Text;
                                    Interest = MultiTaxHistoryTD[4].Text;
                                    AmountPaid = MultiTaxHistoryTD[5].Text;
                                    DatePaid = MultiTaxHistoryTD[6].Text;

                                    TaxPayMentHistory = Year + "~" + TransactionID + "~" + TaxPaid + "~" + Discount + "~" + Interest + "~" + AmountPaid + "~" + DatePaid;
                                    gc.insert_date(orderNumber, parcelNumber, 160, TaxPayMentHistory, 1, DateTime.Now);

                                }
                            }
                        }
                        catch { }

                    }
                    IWebElement CurrentTax = driver.FindElement(By.Id("divTaxes"));
                    js1.ExecuteScript("arguments[0].click();", CurrentTax);
                    try
                    {
                        gc.CreatePdf(orderNumber, parcelNumber, "Property Tax", driver, "OR", "Marion");
                        driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblTaxStatement']/a")).SendKeys(Keys.Enter);
                    }
                    catch
                    {

                    }
                    string TaxesLevied = "", TaxRate = "", CurrentTaxPayoffAmount = "", TotalLevied = "", AdValorem = "", SpecialAssessments = "", Principal = "", InterestDue = "", TotalOwed = "", TaxAuthority = "555 Court St NE Ste. 2242 Salem, OR 97301 Phone: (503) 588-5215";
                    string TaxPayCalculatedFor = "";
                    string TaxPay = "";

                    IWebElement MultiCurrentTaxHistoryTB = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_TaxLevyPanel']/div[2]/table/tbody"));
                    IList<IWebElement> MultiCurrentTaxHistoryTR = MultiCurrentTaxHistoryTB.FindElements(By.TagName("tr"));
                    IList<IWebElement> MultiCurrentTaxHistoryTD;

                    foreach (IWebElement row1 in MultiCurrentTaxHistoryTR)
                    {
                        MultiCurrentTaxHistoryTD = row1.FindElements(By.TagName("td"));
                        if (MultiCurrentTaxHistoryTD.Count != 0)
                        {
                            TaxesLevied = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblTaxesLevied']")).Text;
                            TaxRate = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblTaxRate']")).Text;
                            CurrentTaxPayoffAmount = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblTaxPayoffAmount']")).Text;


                            Year = MultiCurrentTaxHistoryTD[0].Text;
                            TotalLevied = MultiCurrentTaxHistoryTD[1].Text;
                            AdValorem = MultiCurrentTaxHistoryTD[2].Text;
                            SpecialAssessments = MultiCurrentTaxHistoryTD[3].Text;
                            Principal = MultiCurrentTaxHistoryTD[4].Text;
                            InterestDue = MultiCurrentTaxHistoryTD[5].Text;
                            DatePaid = MultiCurrentTaxHistoryTD[6].Text;
                            TotalOwed = MultiCurrentTaxHistoryTD[7].Text;



                            break;
                        }
                    }


                    if (DatePaid == "Unpaid")
                    {
                        string nextEndOfMonth = "";
                        string gtd = "";
                        string currDate = DateTime.Now.ToString("MM/dd/yyyy");
                        string dateChecking = DateTime.Now.ToString("MM") + "/15/" + DateTime.Now.ToString("yyyy");
                        if (Convert.ToDateTime(currDate) > Convert.ToDateTime(dateChecking))
                        {
                            if ((Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))) < 12))
                            {
                                gtd = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")) + 1)).ToString("MM/dd/yyyy");
                                driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_TaxLevyDate']")).Clear();
                                driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_TaxLevyDate']")).SendKeys(gtd);
                                driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_TaxLevyDateSubmit']")).SendKeys(Keys.Enter);

                            }
                            else
                            {
                                int nxtYr = Convert.ToInt16(DateTime.Now.ToString("yyyy")) + 1;
                                gtd = new DateTime(nxtYr, 1, DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), 1)).ToString("MM/dd/yyyy");
                                driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_TaxLevyDate']")).Clear();
                                driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_TaxLevyDate']")).SendKeys(gtd);
                                driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_TaxLevyDateSubmit']")).SendKeys(Keys.Enter);

                            }






                        }

                        else
                        {

                            if ((Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))) < 12))
                            {
                                gtd = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")))).ToString("MM/dd/yyyy");
                                driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_TaxLevyDate']")).Clear();
                                driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_TaxLevyDate']")).SendKeys(gtd);
                                driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_TaxLevyDateSubmit']")).SendKeys(Keys.Enter);

                            }
                            else
                            {
                                int nxtYr = Convert.ToInt16(DateTime.Now.ToString("yyyy")) + 1;
                                gtd = new DateTime(nxtYr, 1, DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), 1)).ToString("MM/dd/yyyy");
                                driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_TaxLevyDate']")).Clear();
                                driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_TaxLevyDate']")).SendKeys(gtd);
                                driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_TaxLevyDateSubmit']")).SendKeys(Keys.Enter);

                            }
                        }
                        Thread.Sleep(2000);

                        TaxPayCalculatedFor = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_CalculatedTotalText']")).Text.Replace("Tax payoff calculated for", "");
                        TaxPay = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_CalculatedTaxTotal']")).Text;
                        TaxPayCalculatedFor = TaxPayCalculatedFor.Replace(":", "");

                    }
                    string Currenttax = TaxesLevied + "~" + TaxRate + "~" + CurrentTaxPayoffAmount + "~" + TotalLevied + "~" + AdValorem + "~" + SpecialAssessments.Replace("\r\n", " ") + "~" + Principal + "~" + InterestDue + "~" + DatePaid + "~" + TotalOwed + "~" + TaxAuthority + "~" + TaxPayCalculatedFor + "~" + TaxPay;
                    //gc.insert_date(orderNumber, parcelNumber, 162, Currenttax, 1, DateTime.Now);
                    MultiCurrentTaxHistoryTB = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_TaxLevyPanel']/div[2]/table/tbody"));
                    MultiCurrentTaxHistoryTR = MultiCurrentTaxHistoryTB.FindElements(By.TagName("tr"));
                    TaxPayCalculatedFor = "";
                    TaxPay = "";
                    int J = 0;

                    foreach (IWebElement row1 in MultiCurrentTaxHistoryTR)
                    {
                        MultiCurrentTaxHistoryTD = row1.FindElements(By.TagName("td"));
                        if (MultiCurrentTaxHistoryTD.Count != 0)
                        {
                            TaxesLevied = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblTaxesLevied']")).Text;
                            TaxRate = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblTaxRate']")).Text;
                            CurrentTaxPayoffAmount = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblTaxPayoffAmount']")).Text;


                            Year = MultiCurrentTaxHistoryTD[0].Text;
                            TotalLevied = MultiCurrentTaxHistoryTD[1].Text;
                            AdValorem = MultiCurrentTaxHistoryTD[2].Text;
                            SpecialAssessments = MultiCurrentTaxHistoryTD[3].Text;
                            Principal = MultiCurrentTaxHistoryTD[4].Text;
                            InterestDue = MultiCurrentTaxHistoryTD[5].Text;
                            DatePaid = MultiCurrentTaxHistoryTD[6].Text;
                            TotalOwed = MultiCurrentTaxHistoryTD[7].Text;

                            if (J == 0)
                            {
                                Currenttax = Year + "~" + TaxesLevied + "~" + TaxRate + "~" + CurrentTaxPayoffAmount + "~" + TotalLevied + "~" + AdValorem + "~" + SpecialAssessments.Replace("\r\n", " ") + "~" + Principal + "~" + InterestDue + "~" + DatePaid + "~" + TotalOwed + "~" + TaxAuthority + "~" + TaxPayCalculatedFor + "~" + TaxPay;
                            }
                            else
                            {
                                Currenttax = Year + "~" + "" + "~" + "" + "~" + "" + "~" + TotalLevied + "~" + AdValorem + "~" + SpecialAssessments.Replace("\r\n", " ") + "~" + Principal + "~" + InterestDue + "~" + DatePaid + "~" + TotalOwed + "~" + TaxAuthority + "~" + TaxPayCalculatedFor + "~" + TaxPay;
                            }


                            gc.insert_date(orderNumber, parcelNumber, 162, Currenttax, 1, DateTime.Now);
                            J++;

                        }
                    }

                    try
                    {


                        string TaxStetement = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblTaxStatement']/a")).GetAttribute("href");
                        gc.downloadfile(TaxStetement, orderNumber, parcelNumber, "TaxStateMent", "OR", "Marion");


                    }
                    catch { }
                    try
                    {
                        IWebElement Owner = driver.FindElement(By.Id("divOwnerHistory"));
                        js1.ExecuteScript("arguments[0].click();", Owner);
                        Thread.Sleep(1000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Owner Info", driver, "OR", "Marion");
                    }
                    catch { }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");

                    gc.insert_TakenTime(orderNumber, "OR", "Marion", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "OR", "Marion");
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