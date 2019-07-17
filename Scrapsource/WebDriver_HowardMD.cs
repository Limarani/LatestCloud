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
    public class WebDriver_HowardMD
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();

        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        public string FTP_Howard(string houseno, string sname, string direction, string account, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string Address = houseno + " " + direction + " " + sname;
            string TaxAuth = "";
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
                    try
                    {
                        var chromeOptions = new ChromeOptions();
                        var chDriver = new ChromeDriver(chromeOptions);

                        string myear = DateTime.Now.Year.ToString();
                        chDriver.Navigate().GoToUrl("https://www.howardcountymd.gov/Departments/Finance");
                        Thread.Sleep(4000);
                        TaxAuth = chDriver.FindElement(By.XPath("//*[@id='dnn_ctr3855_HtmlModule_lblContent']/div[1]/div")).Text.Replace("Howard County Department of Finance", "").Replace("\r\n", " ").Trim();
                        gc.CreatePdf_WOP(orderNumber, "Tax Authority", driver, "MD", "Howard");
                        chDriver.Quit();
                    }
                    catch { }

                    if (searchType == "titleflex")
                    {
                        string titleaddress = houseno + " " + sname + " " + direction + " " + directParcel;
                        gc.TitleFlexSearch(orderNumber, parcelNumber, ownername, titleaddress, "MD", "Howard");

                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();

                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_HowardMD"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }

                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("https://sdat.dat.maryland.gov/RealProperty/Pages/default.aspx");
                        Thread.Sleep(2000);


                        var SerachCategory = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucSearchType_ddlCounty']"));
                        var selectElement1 = new SelectElement(SerachCategory);
                        selectElement1.SelectByText("HOWARD COUNTY");

                        var SerachCategoryM = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucSearchType_ddlSearchType']"));
                        var selectElementM1 = new SelectElement(SerachCategoryM);
                        selectElementM1.SelectByText("STREET ADDRESS");
                        Thread.Sleep(3000);

                        driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_StartNavigationTemplateContainerID_btnContinue']")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);

                        driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtStreenNumber']")).SendKeys(houseno);
                        driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtStreetName']")).SendKeys(sname);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "MD", "Howard");
                        driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_StepNavigationTemplateContainerID_btnStepNextButton']")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);

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
                                    HttpContext.Current.Session["MDHoward_Count"] = "Maimum";
                                    driver.Quit();
                                    return "Maximum";
                                }
                            }
                            catch { }
                            //string AlterNateID = "", PropertyAddress = "", LegalDescriptoin = "", YearBuilt = "";
                            foreach (IWebElement row1 in MultiOwnerRow)
                            {
                                MultiOwnerTD = row1.FindElements(By.TagName("td"));
                                if (MultiOwnerTD.Count != 0 && row1.Text.Trim() != "")
                                {
                                    owner = MultiOwnerTD[0].Text;
                                    Accnumber = MultiOwnerTD[1].Text;
                                    PropertyAddress = MultiOwnerTD[2].Text;
                                    string Multi = owner + "~" + PropertyAddress;
                                    gc.insert_date(orderNumber, Accnumber, 1342, Multi, 1, DateTime.Now);
                                }
                            }
                            gc.CreatePdf_WOP(orderNumber, "Multi parcel Address search", driver, "MD", "Howard");
                            HttpContext.Current.Session["MDHoward"] = "Yes";
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
                        selectElement1.SelectByText("HOWARD COUNTY");
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
                            Pa2 = Pa.Substring(3, 6);
                            Sub = Pa.Substring(2, 1);
                        }
                        parcelNumber = parcelNumber.Replace("-", " ");
                        driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtDistrict']")).SendKeys(Pa1);
                        driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtAccountIdentifier']")).SendKeys(Pa2);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "MD", "Howard");
                        driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_StepNavigationTemplateContainerID_btnStepNextButton']")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_lblErr"));
                        if(INodata.Text.Contains("There are no records that match your criteria for county"))
                        {
                            HttpContext.Current.Session["Nodata_HowardMD"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    string LegalDescription = "", Parcel = "", OwnerName1 = "", OwnerName2 = "", Use = "", PrincipalResidence = "", Map = "", Grid = "", SubDistrict = "", Subdivision = "", Section = "", Block = "", Lot = "", AssessmentYear = "", Town = "";
                    string HomesteadApplicationStatus = "", HomeownersTaxCreditApplicationStatus = "", HomeownersTaxCreditApplicationDate = "", District = "";
                    parcelNumber = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblDetailsStreetHeader_0']")).Text.Replace("Folio:", "");
                    District = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblDetailsStreetHeader_0']")).Text.Replace("Folio:", "");
                    parcelNumber = GlobalClass.After(parcelNumber, "Account Number - ");
                    District = gc.Between(District, "District -", "Account Number -");
                    parcelNumber = District + parcelNumber;
                    gc.CreatePdf(orderNumber, parcelNumber, "Assessment information", driver, "MD", "Howard");

                    OwnerName1 = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblOwnerName_0']")).Text;
                    OwnerName2 = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblOwnerName2_0")).Text;
                    ownername = OwnerName1 + " " + OwnerName2;
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
                    gc.insert_date(orderNumber, parcelNumber, 1340, Property, 1, DateTime.Now);
                    string year = "", TotalAssessedValue = "";
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
                                    year = TaxHisTDD[3].Text.Replace("\r\n", " ") + "~" + TaxHisTDD[4].Text.Replace("\r\n", " ");
                                    //gc.insert_date(orderNumber, parcelNumber, 529, year.Replace("\r\n"," "), 1, DateTime.Now);
                                }
                                else if (row1.Text.Contains("Total: ") && !row1.Text.Contains("Phase-in Assessments"))
                                {
                                    TotalAssessedValue = TaxHisTDD[3].Text + "~" + TaxHisTDD[4].Text;
                                    var split = year.Split('~');
                                    var split1 = TotalAssessedValue.Split('~');
                                    gc.insert_date(orderNumber, parcelNumber, 1341, split[0] + "~" + split1[0], 1, DateTime.Now);
                                    gc.insert_date(orderNumber, parcelNumber, 1341, split[1] + "~" + split1[1], 1, DateTime.Now);
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

                                    year = TaxHisTDD[3].Text.Replace("\r\n", " ") + "~" + TaxHisTDD[4].Text.Replace("\r\n", " ");

                                    //gc.insert_date(orderNumber, parcelNumber, 529, year.Replace("\r\n"," "), 1, DateTime.Now);

                                }
                                else if (row1.Text.Contains("Total: ") && !row1.Text.Contains("Phase-in Assessments"))
                                {
                                    TotalAssessedValue = TaxHisTDD[3].Text + "~" + TaxHisTDD[4].Text;

                                    var split = year.Split('~');
                                    var split1 = TotalAssessedValue.Split('~');

                                    gc.insert_date(orderNumber, parcelNumber, 1341, split[0] + "~" + split1[0], 1, DateTime.Now);
                                    gc.insert_date(orderNumber, parcelNumber, 1341, split[1] + "~" + split1[1], 1, DateTime.Now);

                                }
                            }

                        }
                    }

                    catch
                    {

                    }

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    // Tax Information

                    driver.Navigate().GoToUrl("https://howardcountymd.munisselfservice.com/citizens/RealEstate/Default.aspx?mode=new");
                    Thread.Sleep(2000);
                    // parcelNumber = District + parcelNumber;
                    try
                    {
                        IWebElement parcelno = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_ParcelIdSearchFieldLayout_ctl01_ParcelIDTextBox"));
                        //parcelno.SendKeys(District.TrimStart('0') + Subdivision + AccountNo);
                        parcelno.SendKeys(parcelNumber.TrimStart('0').Replace("-", "").Replace(" ", ""));
                    }
                    catch (Exception ex)
                    {

                    }

                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Information Home", driver, "MD", "Howard");

                    IWebElement search = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_FormLayoutItem7_ctl01_Button1"));
                    search.Click();

                    Thread.Sleep(2000);

                    //----------------------------------------------Tax Grid Scraping---------------------------------------------------         



                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Information Search Result", driver, "MD", "Howard");
                    int Iyear = 0; int Cyear = 0;
                    int Syear = DateTime.Now.Year;
                    int Smonth = DateTime.Now.Month;
                    if (Smonth >= 9)
                    {
                        Iyear = Syear;
                        Cyear = Syear;
                    }
                    else
                    {
                        Iyear = Syear - 1;
                        Cyear = Syear - 1;

                    }
                    for (int i = 1; i <= 3; i++)
                    {
                        try
                        {
                            IWebElement resulttable = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_BillsGridView"));
                            IList<IWebElement> rows_table = resulttable.FindElements(By.TagName("tr"));
                            int rows_count = rows_table.Count;
                            for (int row = 0; row < rows_count; row++)
                            {
                                try
                                {
                                    if (row == rows_count - 2)
                                    {

                                        IList<IWebElement> Columns_row = rows_table[row].FindElements(By.TagName("td"));

                                        int columns_count = Columns_row.Count;

                                        for (int column = 0; column < columns_count; column++)
                                        {
                                            if (column == columns_count - 2)
                                            {
                                                if (Columns_row[4].Text.Contains(Convert.ToString(Iyear)))
                                                {
                                                    IWebElement a = Columns_row[column].FindElement(By.TagName("a"));
                                                    a.Click();
                                                }
                                            }

                                        }
                                    }
                                }
                                catch { }
                                try
                                {
                                    if (row == rows_count - 3)
                                    {

                                        IList<IWebElement> Columns_row = rows_table[row].FindElements(By.TagName("td"));

                                        int columns_count = Columns_row.Count;

                                        for (int column = 0; column < columns_count; column++)
                                        {
                                            if (column == columns_count - 2)
                                            {
                                                if (Columns_row[4].Text.Contains(Convert.ToString(Iyear)))
                                                {
                                                    IWebElement a = Columns_row[column].FindElement(By.TagName("a"));
                                                    a.Click();
                                                }
                                            }

                                        }
                                    }
                                }
                                catch { }
                                try
                                {
                                    if (row == rows_count - 1)
                                    {

                                        IList<IWebElement> Columns_row = rows_table[row].FindElements(By.TagName("td"));

                                        int columns_count = Columns_row.Count;

                                        for (int column = 0; column < columns_count; column++)
                                        {
                                            if (column == columns_count - 2)
                                            {
                                                if (Columns_row[4].Text.Contains(Convert.ToString(Iyear)))
                                                {
                                                    IWebElement a = Columns_row[column].FindElement(By.TagName("a"));
                                                    a.Click();
                                                }
                                            }

                                        }
                                    }
                                }
                                catch { }
                                try
                                {
                                    if (row == rows_count - 0)
                                    {

                                        IList<IWebElement> Columns_row = rows_table[row].FindElements(By.TagName("td"));

                                        int columns_count = Columns_row.Count;

                                        for (int column = 0; column < columns_count; column++)
                                        {
                                            if (column == columns_count - 2)
                                            {
                                                if (Columns_row[4].Text.Contains(Convert.ToString(Iyear)))
                                                {
                                                    IWebElement a = Columns_row[column].FindElement(By.TagName("a"));
                                                    a.Click();
                                                }
                                            }

                                        }
                                    }
                                }
                                catch { }
                                try
                                {
                                    if (row == rows_count - 4)
                                    {

                                        IList<IWebElement> Columns_row = rows_table[row].FindElements(By.TagName("td"));

                                        int columns_count = Columns_row.Count;

                                        for (int column = 0; column < columns_count; column++)
                                        {
                                            if (column == columns_count - 2)
                                            {
                                                if (Columns_row[4].Text.Contains(Convert.ToString(Iyear)))
                                                {
                                                    IWebElement a = Columns_row[column].FindElement(By.TagName("a"));
                                                    a.Click();
                                                }
                                            }

                                        }
                                    }
                                }
                                catch { }
                                try
                                {
                                    if (row == rows_count - 5)
                                    {

                                        IList<IWebElement> Columns_row = rows_table[row].FindElements(By.TagName("td"));

                                        int columns_count = Columns_row.Count;

                                        for (int column = 0; column < columns_count; column++)
                                        {
                                            if (column == columns_count - 2)
                                            {
                                                if (Columns_row[4].Text.Contains(Convert.ToString(Iyear)))
                                                {
                                                    IWebElement a = Columns_row[column].FindElement(By.TagName("a"));
                                                    a.Click();
                                                }
                                            }

                                        }
                                    }
                                }
                                catch { }

                            }


                            gc.CreatePdf(orderNumber, parcelNumber, "View Bill" + Iyear, driver, "MD", "Howard");

                            // View payments/ Adjustments
                            string ParcelID = "", OwnerName = "", TaxYear = "", BillNumber = "", Installment = "";
                            string DueDate = "", TaxAmount = "", PaidAmount = "", PaidDate = "", PaidBy = "", PaidAmount2 = "", PaidDate2 = "", PaidBy2 = "";
                            string BalanceDue = "", Interest = "", TotalDue = "", Good_through_date = "";
                            string strInterest = "", Bill_Flag = "";


                            try
                            {
                                driver.FindElement(By.LinkText("View payments/adjustments")).SendKeys(Keys.Enter);
                                Thread.Sleep(2000);
                                gc.CreatePdf(orderNumber, parcelNumber, "View payments or adjustments" + Iyear, driver, "MD", "Howard");
                            }
                            catch { }

                            string TaxBillNO = "", TaxBillyear = "";
                            TaxBillNO = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_BillNumberLabel")).Text;
                            TaxBillyear = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_FiscalYearLabel")).Text;
                            IWebElement Currenttaxdetails = driver.FindElement(By.XPath("//*[@id='innerContent']/table[2]/tbody"));
                            IList<IWebElement> TRCurrenttaxdetails = Currenttaxdetails.FindElements(By.TagName("tr"));
                            IList<IWebElement> THCurrenttaxdetails = Currenttaxdetails.FindElements(By.TagName("th"));
                            IList<IWebElement> TDCurrenttaxdetails;
                            foreach (IWebElement row in TRCurrenttaxdetails)
                            {
                                TDCurrenttaxdetails = row.FindElements(By.TagName("td"));
                                if (TDCurrenttaxdetails.Count != 0 && !row.Text.Contains("Paid By/Reference") && row.Text.Trim() != "")
                                {

                                    if (TDCurrenttaxdetails[0].Text != "No payment activity could be found for this bill.")
                                    {
                                        string taxpayment = TaxBillNO + "~" + TaxBillyear + "~" + TDCurrenttaxdetails[0].Text + "~" + TDCurrenttaxdetails[1].Text + "~" + TDCurrenttaxdetails[2].Text + "~" + TDCurrenttaxdetails[3].Text + "~" + TaxAuth;
                                        gc.insert_date(orderNumber, parcelNumber, 1441, taxpayment, 1, DateTime.Now);
                                    }
                                    else if (TDCurrenttaxdetails[0].Text == "No payment activity could be found for this bill.")
                                    {
                                        string taxpayment = TaxBillNO + "~" + TaxBillyear + "~" + TDCurrenttaxdetails[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + TaxAuth;
                                        gc.insert_date(orderNumber, parcelNumber, 1441, taxpayment, 1, DateTime.Now);
                                    }


                                }

                            }
                            try
                            {
                                driver.FindElement(By.LinkText("Return to view bill")).SendKeys(Keys.Enter);
                                Thread.Sleep(2000);

                            }
                            catch { }
                            try
                            {
                                //Good Through Details

                                strInterest = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_BillDetailsUpdatePanel']/table/tbody/tr[2]/td[6]")).Text;
                                if (strInterest != "$0.00")
                                {
                                    try
                                    {
                                        Bill_Flag = driver.FindElement(By.XPath("//*[@id='innerContent']/p[2]")).Text;
                                    }
                                    catch { }


                                    IWebElement good_date = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_AsOfDateTextBox']"));
                                    Good_through_date = good_date.GetAttribute("value");
                                    if (Good_through_date.Contains("Select A Date"))
                                    {
                                        Good_through_date = "-";
                                    }
                                    else
                                    {
                                        if (Bill_Flag == "Parcel is in tax sale" || Bill_Flag == "** The full Interest, Penalty, or Fee amounts will be included with any payment, regardless of number of installments selected.")
                                        {

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
                                                    Good_through_date = new DateTime(nxtYr, 1, DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), 1)).ToString("dd/MM/yyyy");

                                                }
                                                try
                                                {//*[@id="ui-datepicker-div"]/div/a[2]/span
                                                    IWebElement gclick1 = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_AsOfDateTextBox"));
                                                    gclick1.Click();
                                                    IWebElement Inextmonth = driver.FindElement(By.XPath("//*[@id='ui-datepicker-div']/div/a[2]/span"));
                                                    Inextmonth.Click();
                                                }
                                                catch { }

                                            }
                                            driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_AsOfDateTextBox")).Clear();

                                            string[] daysplit = Good_through_date.Split('/');
                                            try
                                            {
                                                IWebElement gclick = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_AsOfDateTextBox"));
                                                gclick.Click();
                                            }
                                            catch { }


                                            IWebElement Iday = driver.FindElement(By.XPath("//*[@id='ui-datepicker-div']/table/tbody"));
                                            IList<IWebElement> IdayRow = Iday.FindElements(By.TagName("a"));
                                            foreach (IWebElement day in IdayRow)
                                            {
                                                if (day.Text != "" && day.Text == daysplit[1])
                                                {
                                                    day.SendKeys(Keys.Enter);
                                                    Thread.Sleep(2000);
                                                    gc.CreatePdf(orderNumber, parcelNumber, "Good Through Date", driver, "MD", "Howard");

                                                }
                                            }


                                        }
                                    }
                                }
                            }
                            catch
                            { }


                            ParcelID = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_CategoryLabel")).Text;
                            OwnerName = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_OwnerLabel")).Text;
                            TaxYear = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_FiscalYearLabel")).Text;
                            BillNumber = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_BillNumberLabel")).Text;

                            IWebElement Assessmentdetails = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_BillDetailsUpdatePanel']/table/tbody"));
                            IList<IWebElement> TRAssessmentdetails = Assessmentdetails.FindElements(By.TagName("tr"));
                            IList<IWebElement> THAssessmentdetails = Assessmentdetails.FindElements(By.TagName("th"));
                            IList<IWebElement> TDAssessmentdetails;
                            foreach (IWebElement row1 in TRAssessmentdetails)
                            {
                                TDAssessmentdetails = row1.FindElements(By.TagName("td"));
                                if (TDAssessmentdetails.Count != 0 && !row1.Text.Contains("Payments/Credits	") && !row1.Text.Contains("Installment") && row1.Text.Trim() != "" && TDAssessmentdetails.Count == 7)
                                {
                                    string currenttax = BillNumber + "~" + TaxYear + "~" + OwnerName + "~" + ParcelID + "~" + TDAssessmentdetails[0].Text + "~" + TDAssessmentdetails[1].Text + "~" + TDAssessmentdetails[2].Text + "~" + TDAssessmentdetails[3].Text + "~" + TDAssessmentdetails[4].Text + "~" + TDAssessmentdetails[5].Text + "~" + TDAssessmentdetails[6].Text + "~" + Good_through_date;
                                    gc.insert_date(orderNumber, parcelNumber, 1354, currenttax, 1, DateTime.Now);
                                }
                                if (TDAssessmentdetails.Count != 0 && !row1.Text.Contains("Payments/Credits	") && !row1.Text.Contains("Installment") && row1.Text.Trim() != "" && TDAssessmentdetails.Count == 6)
                                {
                                    string currenttax1 = BillNumber + "~" + TaxYear + "~" + OwnerName + "~" + ParcelID + "~" + TDAssessmentdetails[0].Text + "~" + "" + "~" + TDAssessmentdetails[1].Text + "~" + TDAssessmentdetails[2].Text + "~" + TDAssessmentdetails[3].Text + "~" + TDAssessmentdetails[4].Text + "~" + TDAssessmentdetails[5].Text + "~" + Good_through_date;
                                    gc.insert_date(orderNumber, parcelNumber, 1354, currenttax1, 1, DateTime.Now);
                                }
                                if (TDAssessmentdetails.Count != 0 && !row1.Text.Contains("Payments/Credits	") && !row1.Text.Contains("Installment") && row1.Text.Trim() != "" && TDAssessmentdetails.Count == 3)
                                {
                                    string currenttax1 = BillNumber + "~" + TaxYear + "~" + OwnerName + "~" + ParcelID + "~" + TDAssessmentdetails[1].Text + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + TDAssessmentdetails[2].Text + "~" + Good_through_date;
                                    gc.insert_date(orderNumber, parcelNumber, 1354, currenttax1, 1, DateTime.Now);
                                }
                            }




                            // tax charges
                            try
                            {
                                driver.FindElement(By.LinkText("Charges")).SendKeys(Keys.Enter);
                                Thread.Sleep(2000);
                                gc.CreatePdf(orderNumber, parcelNumber, "Tax Charges" + Iyear, driver, "MD", "Howard");
                            }
                            catch { }

                            try
                            {
                                IWebElement Taxcharges = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_TaxChargesTable']/tbody"));
                                IList<IWebElement> TRTaxcharges = Taxcharges.FindElements(By.TagName("tr"));
                                IList<IWebElement> THTaxcharges;
                                IList<IWebElement> TDTaxcharges;
                                foreach (IWebElement row2 in TRTaxcharges)
                                {
                                    TDTaxcharges = row2.FindElements(By.TagName("td"));
                                    THTaxcharges = row2.FindElements(By.TagName("th"));

                                    if (TDTaxcharges.Count != 0 && !row2.Text.Contains("Taxable Value") && !row2.Text.Contains("Tax Rate") && !row2.Text.Contains("Total") && row2.Text.Trim() != "")
                                    {
                                        string Taxchargesdetails = Iyear + "~" + THTaxcharges[0].Text + "~" + TDTaxcharges[0].Text + "~" + TDTaxcharges[1].Text + "~" + TDTaxcharges[2].Text;
                                        gc.insert_date(orderNumber, parcelNumber, 1362, Taxchargesdetails, 1, DateTime.Now);

                                    }
                                    if (TDTaxcharges.Count != 0 && !row2.Text.Contains("Taxable Value") && row2.Text.Contains("Total") && row2.Text.Trim() != "")
                                    {
                                        string Taxchargesdetails1 = Iyear + "~" + THTaxcharges[0].Text + "~" + "" + "~" + "" + "~" + TDTaxcharges[0].Text;
                                        gc.insert_date(orderNumber, parcelNumber, 1362, Taxchargesdetails1, 1, DateTime.Now);

                                    }
                                }
                            }
                            catch (Exception ex) { }

                            // Property Details
                            try
                            {
                                driver.FindElement(By.LinkText("Property Detail")).SendKeys(Keys.Enter);
                                Thread.Sleep(2000);
                                gc.CreatePdf(orderNumber, parcelNumber, "Property Details" + Iyear, driver, "MD", "Howard");
                            }
                            catch { }

                            // Owner Information
                            try
                            {
                                driver.FindElement(By.LinkText("Owner Information")).SendKeys(Keys.Enter);
                                Thread.Sleep(2000);
                                gc.CreatePdf(orderNumber, parcelNumber, "Owner Information" + Iyear, driver, "MD", "Howard");
                            }
                            catch { }


                            // Assessment
                            string stryear = "";
                            try
                            {
                                driver.FindElement(By.LinkText("Assessment")).SendKeys(Keys.Enter);
                                Thread.Sleep(2000);
                                gc.CreatePdf(orderNumber, parcelNumber, "Assessment" + Iyear, driver, "MD", "Howard");
                            }
                            catch { }

                            try
                            {
                                stryear = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_YearLabel")).Text;
                                IWebElement TaxAssess = driver.FindElement(By.XPath("//*[@id='innerContent']/table[2]/tbody"));
                                IList<IWebElement> TRTaxAssess = TaxAssess.FindElements(By.TagName("tr"));
                                IList<IWebElement> THTaxAssess = TaxAssess.FindElements(By.TagName("th"));
                                IList<IWebElement> TDTaxAssess;
                                foreach (IWebElement row3 in TRTaxAssess)
                                {
                                    TDTaxAssess = row3.FindElements(By.TagName("td"));
                                    THTaxAssess = row3.FindElements(By.TagName("th"));
                                    if (TDTaxAssess.Count != 0 && !row3.Text.Contains("Gross Assessment") && row3.Text.Trim() != "")
                                    {
                                        string TaxAssessdetails = stryear + "~" + THTaxAssess[0].Text + "~" + TDTaxAssess[0].Text;
                                        gc.insert_date(orderNumber, parcelNumber, 1476, TaxAssessdetails, 1, DateTime.Now);

                                    }

                                }
                            }
                            catch (Exception ex) { }


                            // Assessment History
                            try
                            {
                                driver.FindElement(By.LinkText("Assessment History")).SendKeys(Keys.Enter);
                                Thread.Sleep(2000);
                                gc.CreatePdf(orderNumber, parcelNumber, "Assessment History" + Iyear, driver, "MD", "Howard");
                            }
                            catch { }

                            // Tax Rates
                            try
                            {
                                driver.FindElement(By.LinkText("Tax Rates")).SendKeys(Keys.Enter);
                                Thread.Sleep(2000);
                                gc.CreatePdf(orderNumber, parcelNumber, "Tax Rates" + Iyear, driver, "MD", "Howard");
                            }
                            catch { }

                            // All Bills

                            try
                            {
                                driver.FindElement(By.LinkText("All Bills")).SendKeys(Keys.Enter);
                                Thread.Sleep(2000);
                                gc.CreatePdf(orderNumber, parcelNumber, "All Bills", driver, "MD", "Howard");
                            }
                            catch { }

                            if (Cyear == Iyear)
                            {
                                try
                                {
                                    IWebElement Allbills = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_BillsRepeater_ctl00_BillsGrid']/tbody"));
                                    IList<IWebElement> TRAllbills = Allbills.FindElements(By.TagName("tr"));
                                    IList<IWebElement> THAllbills = Allbills.FindElements(By.TagName("th"));
                                    IList<IWebElement> TDAllbills;
                                    foreach (IWebElement row3 in TRAllbills)
                                    {
                                        TDAllbills = row3.FindElements(By.TagName("td"));

                                        if (TDAllbills.Count != 0 && !row3.Text.Contains("Owner") && !row3.Text.Contains("Type") && row3.Text.Trim() != "")
                                        {
                                            string Allbillsdetails = TDAllbills[0].Text + "~" + TDAllbills[1].Text + "~" + TDAllbills[2].Text + "~" + TDAllbills[3].Text + "~" + TDAllbills[4].Text;
                                            gc.insert_date(orderNumber, parcelNumber, 1363, Allbillsdetails, 1, DateTime.Now);

                                        }

                                    }
                                }
                                catch (Exception ex) { }
                            }

                            Iyear--;
                            try
                            {
                                driver.FindElement(By.LinkText("Search Results")).SendKeys(Keys.Enter);
                                Thread.Sleep(2000);
                            }
                            catch { }
                        }
                        catch (Exception ex)
                        {

                        }
                    }

                    // View Lien Details


                    string TaxInfo_Details = "", TaxBill_Installment = "", TaxBill_PayBy = "", TaxBill_Amount = "", TaxBill_Credits = "", TaxBill_Balance = "", TaxBill_Interest = "";
                    string TaxBill_Installment1 = "", TaxBill_PayBy1 = "", TaxBill_Amount1 = "", TaxBill_Credits1 = "", TaxBill_Balance1 = "", TaxBill_Interest1 = "", Parcel_Num = "";
                    int currentyear = 0; int currentyear1 = 0; int currentyear2 = 0;
                    List<string> ParcelSearch = new List<string>();
                    List<string> viewlien = new List<string>();
                    int lien = 0;
                    try
                    {
                        if (Smonth >= 9)
                        {
                            currentyear = DateTime.Now.Year + 1;
                            currentyear1 = DateTime.Now.Year;
                            currentyear2 = DateTime.Now.Year - 1;
                        }
                        else
                        {
                            currentyear = DateTime.Now.Year;
                            currentyear1 = DateTime.Now.Year - 1;
                            currentyear2 = DateTime.Now.Year - 2;
                        }

                        IWebElement ParcelTB = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_BillsGridView']/tbody"));
                        IList<IWebElement> ParcelTR = ParcelTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> parcelid;
                        IList<IWebElement> parcelth;
                        if (ParcelSearch.Count() != 3 || ParcelSearch.Count() == 0)
                        {
                            if (ParcelSearch.Count() != 3 & ParcelSearch.Count() != 0)
                            {
                                currentyear--;
                            }
                            foreach (IWebElement IParcel in ParcelTR)
                            {
                                parcelth = IParcel.FindElements(By.TagName("th"));
                                parcelid = IParcel.FindElements(By.TagName("td"));
                                try
                                {
                                    if ((parcelid[4].Text.Contains(Convert.ToString(currentyear)) || parcelid[4].Text.Contains(Convert.ToString(currentyear1)) || parcelid[4].Text.Contains(Convert.ToString(currentyear2))) && parcelid[6].Text.Contains("View Bill") && parcelid.Count != 0)
                                    {

                                        IWebElement ParcelBill_link = parcelid[6].FindElement(By.TagName("a"));
                                        string Parcelurl = ParcelBill_link.GetAttribute("id");
                                        ParcelSearch.Add(Parcelurl);
                                    }
                                    if (parcelid[7].Text.Contains("View Lien") & lien == 0)
                                    {
                                        IWebElement ParcelBill_link1 = parcelid[7].FindElement(By.TagName("a"));
                                        string Parcelurl1 = ParcelBill_link1.GetAttribute("id");
                                        viewlien.Add(Parcelurl1);
                                        lien++;
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                    catch { }
                    string currwindow = driver.Url;

                    if (viewlien.Count != 0)
                    {
                        List<string> Viewbillline = new List<string>();
                        foreach (string Line in viewlien)
                        {
                            driver.Navigate().GoToUrl(currwindow);
                            Thread.Sleep(3000);
                            gc.CreatePdf_Chrome(orderNumber, parcelNumber, "line view detail" + Iyear, driver, "MD", "Howard");
                            driver.FindElement(By.Id(Line)).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, parcelNumber, "line Tax detail" + Iyear, driver, "MD", "Howard");
                            // Parcel_Num = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ParcelIdLabel")).Text.Trim();
                            IWebElement LineFoundTable = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_BillsGrid']/tbody"));
                            IList<IWebElement> Linefoundrow = LineFoundTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> linefoundid;
                            foreach (IWebElement Linefound in Linefoundrow)
                            {
                                linefoundid = Linefound.FindElements(By.TagName("td"));
                                if (linefoundid.Count != 0)
                                {
                                    string Linefoundresult = linefoundid[1].Text + "~" + linefoundid[2].Text + "~" + linefoundid[3].Text + "~" + linefoundid[4].Text;
                                    //  gc.insert_date(orderNumber, parcelNumber, 1475, Linefoundresult, 1, DateTime.Now);
                                }
                                if (Linefound.Text.Contains("View Bill"))
                                {
                                    IWebElement Viewbill = linefoundid[5].FindElement(By.TagName("a"));
                                    string Viewbillid = Viewbill.GetAttribute("id");
                                    Viewbillline.Add(Viewbillid);
                                }
                            }
                        }
                        int Linevi = 0;
                        foreach (string viewlinelink in Viewbillline)
                        {
                            driver.FindElement(By.Id(viewlinelink)).Click();
                            Thread.Sleep(2000);
                            string linebillyear = "", linebill = "", lineownername = "";
                            gc.CreatePdf(orderNumber, parcelNumber, "line Bill detail" + Linevi, driver, "MD", "Howard");
                            linebillyear = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_FiscalYearLabel")).Text;
                            linebill = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_BillNumberLabel")).Text;
                            Parcel_Num = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_CategoryLabel")).Text.Trim();
                            lineownername = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_OwnerLabel")).Text;
                            IWebElement TaxInfoTB = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_BillDetailsUpdatePanel']/table/tbody"));
                            IList<IWebElement> TaxInfoTR = TaxInfoTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> TaxInfoTD;
                            foreach (IWebElement TaxInfo in TaxInfoTR)
                            {
                                TaxInfoTD = TaxInfo.FindElements(By.TagName("td"));
                                if (TaxInfoTD.Count != 0 && TaxInfoTD.Count == 6 && !TaxInfo.Text.Contains("Installment"))
                                {
                                    TaxBill_Installment = TaxInfoTD[0].Text;
                                    TaxBill_PayBy = TaxInfoTD[1].Text;
                                    TaxBill_Amount = TaxInfoTD[2].Text;
                                    TaxBill_Credits = TaxInfoTD[3].Text;
                                    TaxBill_Balance = TaxInfoTD[4].Text;
                                    TaxBill_Interest = TaxInfoTD[5].Text;


                                    TaxInfo_Details = linebill + "~" + linebillyear + "~" + lineownername + "~" + Parcel_Num + "~" + TaxBill_Installment + "~" + TaxBill_PayBy + "~" + TaxBill_Amount + "~" + TaxBill_Credits + "~" + TaxBill_Balance + "~" + TaxBill_Interest;
                                    gc.insert_date(orderNumber, parcelNumber, 1486, TaxInfo_Details, 1, DateTime.Now);
                                }
                                if (TaxInfoTD.Count == 5)
                                {
                                    TaxBill_Installment1 = TaxInfoTD[0].Text;
                                    TaxBill_PayBy1 = "";
                                    TaxBill_Amount1 = TaxInfoTD[1].Text;
                                    TaxBill_Credits1 = TaxInfoTD[2].Text;
                                    TaxBill_Balance1 = TaxInfoTD[3].Text;
                                    TaxBill_Interest1 = TaxInfoTD[4].Text;


                                    TaxInfo_Details = linebill + "~" + linebillyear + "~" + lineownername + "~" + Parcel_Num + "~" + TaxBill_Installment1 + "~" + TaxBill_PayBy1 + "~" + TaxBill_Amount1 + "~" + TaxBill_Credits1 + "~" + TaxBill_Balance1 + "~" + TaxBill_Interest1;
                                    gc.insert_date(orderNumber, parcelNumber, 1486, TaxInfo_Details, 1, DateTime.Now);
                                }
                            }
                            driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_PaymentsAdjustmentsLinkButton")).Click();
                            Thread.Sleep(2000);

                            // Lien Tax Payment Details

                            IWebElement LienTaxPay = driver.FindElement(By.XPath("//*[@id='innerContent']/table[2]/tbody"));
                            IList<IWebElement> TRLienTaxPay = LienTaxPay.FindElements(By.TagName("tr"));
                            IList<IWebElement> THLienTaxPay = LienTaxPay.FindElements(By.TagName("th"));
                            IList<IWebElement> TDLienTaxPay;
                            foreach (IWebElement row3 in TRLienTaxPay)
                            {
                                TDLienTaxPay = row3.FindElements(By.TagName("td"));

                                if (TDLienTaxPay.Count != 0 && !row3.Text.Contains("Activity") && !row3.Text.Contains("Paid By/Reference") && row3.Text.Trim() != "")
                                {
                                    string LienTaxPaydetails = linebill + "~" + linebillyear + "~" + TDLienTaxPay[0].Text + "~" + TDLienTaxPay[1].Text + "~" + TDLienTaxPay[2].Text + "~" + TDLienTaxPay[3].Text;
                                    gc.insert_date(orderNumber, parcelNumber, 1475, LienTaxPaydetails, 1, DateTime.Now);

                                }

                            }


                            gc.CreatePdf(orderNumber, parcelNumber, "line Payment detail" + Linevi, driver, "MD", "Howard");
                            driver.FindElement(By.XPath("//*[@id='innerContent']/ul/li[2]/a")).Click();
                            Thread.Sleep(2000);
                            Linevi++;
                        }

                    }






                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");

                    gc.insert_TakenTime(orderNumber, "MD", "Howard", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);


                    driver.Quit();
                    gc.mergpdf(orderNumber, "MD", "Howard");
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