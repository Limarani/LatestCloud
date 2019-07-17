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
    public class WebDriver_MontgomeryMD
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();

        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        public string FTP_MontgomeryMD(string houseno, string sname, string Streettype, string direction, string account, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {

                string AlterNateID = "", PropertyAddress = "", LegalDescriptoin = "", YearBuilt = "";
                string Route = "", address = "", parcel = "", owner = "", Accnumber = "";
                string[] stringSeparators1 = new string[] { "\r\n" };
                List<string> listurl = new List<string>();
                List<string> listurl1 = new List<string>();
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    if (searchType == "titleflex")
                    {
                        string titleaddress = "";
                        if (direction != "")
                        {
                            titleaddress = houseno + " " + direction + " " + sname + " " + Streettype + " " + account;

                        }
                        else
                        {
                            titleaddress = houseno + " " + sname + " " + Streettype + " " + account;

                        }
                        gc.TitleFlexSearch(orderNumber, parcelNumber, ownername, titleaddress, "MD", "Montgomery");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_MontgomeryMD"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("https://sdat.dat.maryland.gov/RealProperty/Pages/default.aspx");
                        Thread.Sleep(2000);


                        var SerachCategory = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucSearchType_ddlCounty']"));
                        var selectElement1 = new SelectElement(SerachCategory);
                        selectElement1.SelectByText("MONTGOMERY COUNTY");

                        var SerachCategoryM = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucSearchType_ddlSearchType']"));
                        var selectElementM1 = new SelectElement(SerachCategoryM);
                        selectElementM1.SelectByText("STREET ADDRESS");
                        Thread.Sleep(3000);

                        driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_StartNavigationTemplateContainerID_btnContinue']")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);

                        driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtStreenNumber']")).SendKeys(houseno);
                        driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtStreetName']")).SendKeys(sname);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "MD", "Montgomery");
                        driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_StepNavigationTemplateContainerID_btnStepNextButton']")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Address search Result", driver, "MD", "Montgomery");
                        // driver.FindElement(By.ClassName("form-control add-suite ng-pristine ng-valid")).SendKeys(account);
                        try
                        {
                            IWebElement MultiOwnerTable = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucSearchResult_gv_SearchResult']/tbody"));
                            IList<IWebElement> MultiOwnerRow = MultiOwnerTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiOwnerTD;
                            try
                            {
                                if (MultiOwnerRow.Count > 28)
                                {
                                    HttpContext.Current.Session["MDMontgomery_Count"] = "Maimum";
                                    return "Maximum";
                                }
                            }
                            catch { }

                            foreach (IWebElement row1 in MultiOwnerRow)
                            {
                                MultiOwnerTD = row1.FindElements(By.TagName("td"));
                                if (MultiOwnerTD.Count != 0 && row1.Text.Trim() != "")
                                {
                                    owner = MultiOwnerTD[0].Text;
                                    Accnumber = MultiOwnerTD[1].Text;
                                    PropertyAddress = MultiOwnerTD[2].Text;
                                    string Multi = owner + "~" + PropertyAddress;
                                    gc.insert_date(orderNumber, Accnumber, 1396, Multi, 1, DateTime.Now);
                                }
                            }

                            HttpContext.Current.Session["MDMontgomery"] = "Yes";
                            driver.Quit();
                            return "MultiParcel";
                        }
                        catch { }
                    }

                    else if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("https://sdat.dat.maryland.gov/RealProperty/Pages/default.aspx");
                        Thread.Sleep(2000);
                        var SerachCategory = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucSearchType_ddlCounty']"));
                        var selectElement1 = new SelectElement(SerachCategory);
                        selectElement1.SelectByText("MONTGOMERY COUNTY");
                        var SerachCategoryM = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucSearchType_ddlSearchType']"));
                        var selectElementM1 = new SelectElement(SerachCategoryM);
                        selectElementM1.SelectByText("PROPERTY ACCOUNT IDENTIFIER");
                        Thread.Sleep(3000);
                        driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_StartNavigationTemplateContainerID_btnContinue']")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        string Pa1 = "", Pa2 = "", Sub = "";
                        string Pa = parcelNumber.Replace("-", "");
                        if (Pa.Count() == 8)
                        {

                            Pa1 = Pa.Substring(0, 2);
                            Pa2 = Pa.Substring(2, 6);

                        }
                        else
                        {
                            Pa1 = Pa.Substring(0, 2);
                            Pa2 = Pa.Substring(2, 8);
                            // Sub = Pa.Substring(2, 1);
                        }
                        driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtDistrict']")).SendKeys(Pa1);
                        driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtAccountIdentifier']")).SendKeys(Pa2);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "MD", "Montgomery");
                        driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_StepNavigationTemplateContainerID_btnStepNextButton']")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Result", driver, "MD", "Montgomery");
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_divError"));
                        if (INodata.Text.Contains("There are no records that match your criteria for county"))
                        {
                            HttpContext.Current.Session["Nodata_MontgomeryMD"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    string LegalDescription = "", parcelno = "", Parcel = "", Use = "", PrincipalResidence = "", Map = "", Grid = "", SubDistrict = "", Subdivision = "", Section = "", Block = "", Lot = "", AssessmentYear = "", Town = "";
                    string HomesteadApplicationStatus = "", HomeownersTaxCreditApplicationStatus = "", HomeownersTaxCreditApplicationDate = "", District = "";
                    parcelno = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblDetailsStreetHeader_0']")).Text.Replace("Folio:", "");
                    District = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblDetailsStreetHeader_0']")).Text.Replace("Folio:", "");
                    parcelno = GlobalClass.After(parcelno, "Account Number - ");
                    District = gc.Between(District, "District -", "Account Number -").Trim();
                    parcelNumber = District + "-" + parcelno;
                    //gc.CreatePdf(orderNumber, parcelNumber, "Assessment information", driver, "MD", "Montgomery");

                    ownername = driver.FindElement(By.XPath("//*[@id='detailSearch']/tbody/tr[5]/td[2]")).Text.Replace("\r\n", " ");

                    PropertyAddress = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblPremisesAddress_0']")).Text.Replace("\r\n", " ");
                    LegalDescription = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblLegalDescription_0']")).Text.Replace("\r\n", " ");
                    YearBuilt = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_Label18_0']")).Text;
                    Use = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblUse_0']")).Text;
                    PrincipalResidence = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblPrinResidence_0']")).Text;
                    Map = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_Label5_0']")).Text;
                    Grid = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_Label6_0']")).Text;
                    Parcel = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_Label7_0']")).Text;
                    SubDistrict = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_Label8_0']")).Text;
                    Subdivision = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_Label9_0']")).Text;
                    Section = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_Label10_0']")).Text;
                    Block = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_Label11_0']")).Text;
                    Lot = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_Label12_0']")).Text;
                    AssessmentYear = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_Label13_0']")).Text;
                    Town = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_Label15_0']")).Text;
                    HomesteadApplicationStatus = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblHomeStatus_0']")).Text;
                    HomeownersTaxCreditApplicationStatus = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblHTC_Status_0']")).Text;
                    HomeownersTaxCreditApplicationDate = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblHTC_StatusDate_0']")).Text;
                    //Parcel = driver.FindElement(By.XPath("//*[@id='property_info']/tbody/tr[3]/td/div")).Text;
                    string Property = ownername + "~" + PropertyAddress + "~" + LegalDescription + "~" + YearBuilt + "~" + Use + "~" + PrincipalResidence + "~" + Map + "~" + Grid + "~" + Parcel + "~" + SubDistrict + "~" + Subdivision + "~" + Section + "~" + Block + "~" + Lot + "~" + AssessmentYear + "~" + Town + "~" + HomesteadApplicationStatus + "~" + HomeownersTaxCreditApplicationStatus + "~" + HomeownersTaxCreditApplicationDate;
                    gc.insert_date(orderNumber, parcelNumber, 1378, Property, 1, DateTime.Now);
                    string year = "", TotalAssessedValue = "", Landvalue = "", Improvements = "", Totalvalue = "";
                    try
                    {
                        IWebElement TaxHisTBD = driver.FindElement(By.XPath("//*[@id='detailSearch']/tbody/tr[15]/td/table/tbody"));
                        IList<IWebElement> TaxHisTRD = TaxHisTBD.FindElements(By.TagName("tr"));
                        IList<IWebElement> TaxHisTDD;
                        foreach (IWebElement row1 in TaxHisTRD)
                        {
                            TaxHisTDD = row1.FindElements(By.TagName("td"));
                            if (TaxHisTDD.Count != 0)
                            {
                                if (TaxHisTDD[0].Text.Trim() == "" && !row1.Text.Contains("Phase-in Assessments"))
                                {
                                    year = TaxHisTDD[3].Text.Replace("\r\n", " ");
                                }
                                else if (row1.Text.Contains("Land:") && !row1.Text.Contains("Phase-in Assessments") && !row1.Text.Contains("Preferential Land:"))
                                {
                                    Landvalue = TaxHisTDD[2].Text;

                                }
                                else if (row1.Text.Contains("Improvements") && !row1.Text.Contains("Phase-in Assessments"))
                                {
                                    Improvements = TaxHisTDD[2].Text;

                                }
                                else if (row1.Text.Contains("Total: ") && !row1.Text.Contains("Phase-in Assessments"))
                                {
                                    TotalAssessedValue = TaxHisTDD[2].Text + "~" + TaxHisTDD[3].Text;
                                    var split1 = TotalAssessedValue.Split('~');
                                    Totalvalue = split1[0];
                                    TotalAssessedValue = split1[1];
                                    string Assessmentdetails = year + "~" + Landvalue + "~" + Improvements + "~" + Totalvalue + "~" + TotalAssessedValue;
                                    gc.insert_date(orderNumber, parcelNumber, 1379, Assessmentdetails, 1, DateTime.Now);

                                }
                            }

                        }
                    }

                    catch
                    {

                    }
                    try
                    {
                        IWebElement TaxHisTBD = driver.FindElement(By.XPath("//*[@id='detailSearch']/tbody/tr[14]/td/table/tbody"));
                        IList<IWebElement> TaxHisTRD = TaxHisTBD.FindElements(By.TagName("tr"));
                        IList<IWebElement> TaxHisTDD;


                        foreach (IWebElement row1 in TaxHisTRD)
                        {
                            TaxHisTDD = row1.FindElements(By.TagName("td"));
                            if (TaxHisTDD.Count != 0)
                            {
                                if (TaxHisTDD[0].Text.Trim() == "" && !row1.Text.Contains("Phase-in Assessments"))
                                {
                                    year = TaxHisTDD[3].Text.Replace("\r\n", " ");
                                }
                                else if (row1.Text.Contains("Land:") && !row1.Text.Contains("Phase-in Assessments") && !row1.Text.Contains("Preferential Land:"))
                                {
                                    Landvalue = TaxHisTDD[2].Text;

                                }
                                else if (row1.Text.Contains("Improvements") && !row1.Text.Contains("Phase-in Assessments"))
                                {
                                    Improvements = TaxHisTDD[2].Text;

                                }
                                else if (row1.Text.Contains("Total: ") && !row1.Text.Contains("Phase-in Assessments"))
                                {
                                    TotalAssessedValue = TaxHisTDD[2].Text + "~" + TaxHisTDD[3].Text;
                                    var split1 = TotalAssessedValue.Split('~');
                                    Totalvalue = split1[0];
                                    TotalAssessedValue = split1[1];
                                    string Assessmentdetails = year + "~" + Landvalue + "~" + Improvements + "~" + Totalvalue + "~" + TotalAssessedValue;
                                    gc.insert_date(orderNumber, parcelNumber, 1379, Assessmentdetails, 1, DateTime.Now);

                                }
                            }

                        }
                    }

                    catch
                    {

                    }

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    // Tax Information

                    driver.Navigate().GoToUrl("https://apps.montgomerycountymd.gov/realpropertytax/");
                    Thread.Sleep(4000);
                    driver.FindElement(By.Id("ctl00_MainContent_ParcelCode")).SendKeys(parcelno);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Search", driver, "MD", "Montgomery");
                    driver.FindElement(By.Id("ctl00_MainContent_btnAcc")).Click();
                    Thread.Sleep(3000);
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='acsMainInvite']/div/a[1]")).Click();
                        Thread.Sleep(2000);
                    }
                    catch { }
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax History Details", driver, "MD", "Montgomery");
                    string taxyear = "";
                    taxyear = DateTime.Now.Year.ToString();
                    IWebElement taxdata = driver.FindElement(By.XPath("//*[@id='ctl00_MainContent_grdParcel']/tbody"));
                    IList<IWebElement> TRtaxdata = taxdata.FindElements(By.TagName("tr"));
                    IList<IWebElement> THtaxdata = taxdata.FindElements(By.TagName("th"));
                    IList<IWebElement> TDtaxdata;
                    foreach (IWebElement row in TRtaxdata)
                    {
                        TDtaxdata = row.FindElements(By.TagName("td"));
                        if (!row.Text.Contains("Balance"))
                        {
                            string Taxhistorydetails = TDtaxdata[3].Text + "~" + TDtaxdata[0].Text + "~" + TDtaxdata[1].Text + "~" + TDtaxdata[2].Text + "~" + TDtaxdata[4].Text;
                            gc.insert_date(orderNumber, parcelNumber, 1382, Taxhistorydetails, 1, DateTime.Now);
                        }
                    }
                    string Tax_Year = "";
                    int Syear = DateTime.Now.Year;
                    int Smonth = DateTime.Now.Month;
                    int iyear = 0;
                    if (Smonth >= 9)
                    {
                        iyear = Syear;
                    }
                    else
                    {
                        iyear = Syear - 1;
                    }

                    for (int i = 1; i <= 3; i++)
                    {
                        try
                        {
                            IWebElement taxdata1 = driver.FindElement(By.XPath("//*[@id='ctl00_MainContent_grdParcel']/tbody"));
                            IList<IWebElement> TRtaxdata1 = taxdata1.FindElements(By.TagName("tr"));
                            IList<IWebElement> THtaxdata1 = taxdata1.FindElements(By.TagName("th"));
                            IList<IWebElement> TDtaxdata1;
                            foreach (IWebElement row2 in TRtaxdata1)
                            {
                                TDtaxdata1 = row2.FindElements(By.TagName("td"));
                                if (!row2.Text.Contains("Balance") && TDtaxdata1[0].Text.Trim() == Convert.ToString(iyear))
                                {
                                    IWebElement IClick = TDtaxdata1[2].FindElement(By.TagName("a"));
                                    IClick.Click();
                                    Thread.Sleep(2000);
                                    try
                                    {
                                        driver.FindElement(By.XPath("//*[@id='acsMainInvite']/div/a[1]")).Click();
                                        Thread.Sleep(2000);
                                    }
                                    catch { }

                                    gc.CreatePdf(orderNumber, parcelNumber, "Tax information" + iyear, driver, "MD", "Montgomery");
                                    string Billnumber = "", Ownername = "", Occupancy = "", Billtype = "", StatetaxRate = "", StatetaxCharge = "", CountytaxRate = "", CountytaxCharge = "";
                                    string Solidwastecharge_Tax = "", Solidwastecharge_Charge = "", waterQualprotect = "", Totaltax = "", paidamount = "", Totaldue = "", Duebyamount = "", Duebydate = "";
                                    string countypropertytaxcredit = "", Totalcredits = "", Interest = "";

                                    IWebElement taxbulkdata = driver.FindElement(By.XPath("//*[@id='aspnetForm']/section/div/div[2]/div[2]/div[2]/div[1]/table/tbody"));
                                    Billnumber = driver.FindElement(By.Id("ctl00_MainContent_lblBillNo")).Text;
                                    Ownername = driver.FindElement(By.Id("ctl00_MainContent_lblName")).Text;
                                    Occupancy = driver.FindElement(By.Id("ctl00_MainContent_lblOccupancy")).Text;
                                    Tax_Year = driver.FindElement(By.Id("ctl00_MainContent_LeviYear")).Text;
                                    Billtype = driver.FindElement(By.Id("ctl00_MainContent_lblBillType")).Text.Replace("BILL", "");
                                    StatetaxRate = driver.FindElement(By.Id("ctl00_MainContent_lblTaxRate2")).Text;
                                    StatetaxCharge = driver.FindElement(By.Id("ctl00_MainContent_lblTaxAmount2")).Text;
                                    CountytaxRate = driver.FindElement(By.Id("ctl00_MainContent_lblTaxRate3")).Text;
                                    CountytaxCharge = driver.FindElement(By.Id("ctl00_MainContent_lblTaxAmount3")).Text;
                                    Solidwastecharge_Tax = driver.FindElement(By.Id("ctl00_MainContent_lblTaxRate4")).Text;
                                    Solidwastecharge_Charge = driver.FindElement(By.Id("ctl00_MainContent_lblTaxAmount4")).Text;
                                    waterQualprotect = driver.FindElement(By.Id("ctl00_MainContent_lblTaxAmount5")).Text;
                                    Totaltax = driver.FindElement(By.Id("ctl00_MainContent_lblTaxAmount6")).Text;
                                    paidamount = driver.FindElement(By.Id("ctl00_MainContent_lblprPymtAmt")).Text;
                                    try
                                    {
                                        Interest = driver.FindElement(By.Id("//*[@id='ctl00_MainContent_PlaceHolder']/table/tbody/tr[22]/td[4]")).Text;
                                    }
                                    catch { }

                                    Totaldue = driver.FindElement(By.Id("ctl00_MainContent_lblTotalAmtDue")).Text;
                                    Duebyamount = driver.FindElement(By.Id("ctl00_MainContent_lblIToPay")).Text;
                                    Duebydate = driver.FindElement(By.Id("ctl00_MainContent_lblLastAccptPmtDate")).Text;
                                    try
                                    {
                                        countypropertytaxcredit = driver.FindElement(By.Id("ctl00_MainContent_lblTaxAmount10")).Text;
                                    }
                                    catch { }
                                    try
                                    {
                                        Totalcredits = driver.FindElement(By.Id("ctl00_MainContent_lblTaxAmount10")).Text;
                                    }
                                    catch { }

                                    string Taxdetails = Billnumber + "~" + Ownername + "~" + Occupancy + "~" + Tax_Year + "~" + Billtype + "~" + paidamount + "~" + Interest + "~" + Totaldue + "~" + Duebyamount + "~" + Duebydate;
                                    gc.insert_date(orderNumber, parcelNumber, 1381, Taxdetails, 1, DateTime.Now);


                                    try
                                    {
                                        IWebElement TaxDue = driver.FindElement(By.XPath("//*[@id='ctl00_MainContent_PlaceHolder']/table/tbody"));
                                        IList<IWebElement> TRTaxDue = TaxDue.FindElements(By.TagName("tr"));
                                        IList<IWebElement> THTaxDue = TaxDue.FindElements(By.TagName("th"));
                                        IList<IWebElement> TDTaxDue;
                                        foreach (IWebElement row3 in TRTaxDue)
                                        {
                                            TDTaxDue = row3.FindElements(By.TagName("td"));
                                            if (!row3.Text.Contains("TAX DESCRIPTION") && !row3.Text.Contains("PRIOR PAYMENTS") && !row3.Text.Contains("INTEREST") && !row3.Text.Contains("TOTAL AMOUNT") && !row3.Text.Contains("Amount Due by") && !row3.Text.Contains("CREDIT DESCRIPTION") && TDTaxDue.Count != 0 && row3.Text.Trim() != "")
                                            {
                                                string TaxDueDetails = Tax_Year + "~" + TDTaxDue[0].Text + "~" + TDTaxDue[1].Text + "~" + TDTaxDue[2].Text + "~" + TDTaxDue[3].Text;
                                                gc.insert_date(orderNumber, parcelNumber, 1485, TaxDueDetails, 1, DateTime.Now);
                                            }
                                        }

                                    }
                                    catch { }
                                }



                                // Printbill
                                // string currentwindow = driver.CurrentWindowHandle;

                                //try
                                //{
                                //    IWebElement Iprintbill = driver.FindElement(By.Id("ctl00_billpdf"));
                                //    // string printbill = Iprintbill.GetAttribute("href");
                                //    IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                                //    js.ExecuteScript("arguments[0].click();", Iprintbill);
                                //    Thread.Sleep(10000);
                                //    driver.SwitchTo().Window(driver.WindowHandles.Last());
                                //    Thread.Sleep(2000);
                                //    gc.CreatePdf(orderNumber, parcelNumber, "Print Bill" + iyear, driver, "MD", "Montgomery");
                                //    driver.SwitchTo().Window(currentwindow);
                                //    Thread.Sleep(1000);
                                //}
                                //catch (Exception ex) { }

                                try
                                {
                                    IWebElement strInfo = driver.FindElement(By.XPath("//*[@id='aspnetForm']/section/div/div[2]/div[2]/div[2]/div[1]/table/tbody"));
                                    if (strInfo.Text.Contains("ACCOUNT NUMBER"))
                                    {
                                        driver.Navigate().Back();
                                    }
                                }
                                catch { }

                            }
                        }

                        catch { }
                        iyear--;
                    }
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='acsMainInvite']/div/a[1]")).Click();
                        Thread.Sleep(2000);
                    }
                    catch { }

                    // Download Bill

                    try
                    {

                        var chromeOptions = new ChromeOptions();
                        var chDriver = new ChromeDriver(chromeOptions);

                        string myear = DateTime.Now.Year.ToString();
                        chDriver.Navigate().GoToUrl("https://apps.montgomerycountymd.gov/realpropertytax/");
                        Thread.Sleep(4000);


                        chDriver.FindElement(By.Id("ctl00_MainContent_ParcelCode")).SendKeys(parcelno);
                        chDriver.FindElement(By.Id("ctl00_MainContent_btnAcc")).Click();
                        Thread.Sleep(3000);
                        try
                        {
                            chDriver.FindElement(By.XPath("//*[@id='acsMainInvite']/div/a[1]")).Click();
                            Thread.Sleep(2000);
                        }
                        catch { }
                        try
                        {
                            int StrYear = DateTime.Now.Year;
                            int StrMonth = DateTime.Now.Month;
                            int IYear = 0;
                            if (StrMonth >= 9)
                            {
                                IYear = StrYear;
                            }
                            else
                            {
                                IYear = StrYear - 1;
                            }

                            for (int i = 1; i <= 3; i++)
                            {
                                try
                                {
                                    IWebElement taxdata1 = chDriver.FindElement(By.XPath("//*[@id='ctl00_MainContent_grdParcel']/tbody"));
                                    IList<IWebElement> TRtaxdata1 = taxdata1.FindElements(By.TagName("tr"));
                                    IList<IWebElement> THtaxdata1 = taxdata1.FindElements(By.TagName("th"));
                                    IList<IWebElement> TDtaxdata1;
                                    foreach (IWebElement row2 in TRtaxdata1)
                                    {
                                        TDtaxdata1 = row2.FindElements(By.TagName("td"));
                                        if (!row2.Text.Contains("Balance") && TDtaxdata1[0].Text.Trim() == Convert.ToString(IYear))
                                        {
                                            IWebElement IClick = TDtaxdata1[2].FindElement(By.TagName("a"));
                                            IClick.Click();
                                            Thread.Sleep(2000);
                                            try
                                            {
                                                chDriver.FindElement(By.XPath("//*[@id='acsMainInvite']/div/a[1]")).Click();
                                                Thread.Sleep(2000);
                                            }
                                            catch { }
                                        }
                                    }

                                }
                                catch { }

                                // Printbill
                                string currentwindow = chDriver.CurrentWindowHandle;

                                try
                                {
                                    IWebElement Iprintbill = chDriver.FindElement(By.Id("ctl00_billpdf"));
                                    // string printbill = Iprintbill.GetAttribute("href");
                                    IJavaScriptExecutor js = chDriver as IJavaScriptExecutor;
                                    js.ExecuteScript("arguments[0].click();", Iprintbill);
                                    Thread.Sleep(10000);
                                    chDriver.SwitchTo().Window(chDriver.WindowHandles.Last());
                                    Thread.Sleep(2000);
                                    // gc.CreatePdf(orderNumber, parcelNumber, "Print Bill" + IYear, driver, "MD", "Montgomery");
                                    try
                                    {
                                        gc.downloadfile(chDriver.Url, orderNumber, parcelNumber, "Print Bill" + IYear, "MD", "Montgomery");
                                    }
                                    catch (Exception ex) { chDriver.Quit(); }
                                    chDriver.SwitchTo().Window(currentwindow);
                                    Thread.Sleep(1000);
                                }
                                catch (Exception ex) { chDriver.Quit(); }
                                IYear--;
                                try
                                {
                                    chDriver.Navigate().Back();
                                    Thread.Sleep(2000);
                                }
                                catch { }



                            }

                        }
                        catch { chDriver.Quit(); }
                        chDriver.Quit();
                    }
                    catch { }





                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");

                    gc.insert_TakenTime(orderNumber, "MD", "Montgomery", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);


                    driver.Quit();
                    gc.mergpdf(orderNumber, "MD", "Montgomery");
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
