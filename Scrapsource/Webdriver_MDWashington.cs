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
    public class Webdriver_MDWashington
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_MDWashignton(string houseno, string sname, string direction, string account, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            string Parcel_ID = "", owner = "", Accnumber = "", PropertyAddress = "", Multi = "", Pa1 = "", Pa2 = "", Sub = "", Pa = "", Land = "", Building = "", Total_Assessed_Value = "", Assessment_details = "";
            string Parcel = "", Map = "", Grid = "", Subdivision = "", Block = "", Lot = "", value = "", Mail_Address = "", property_details = "", tax_Parcelid = "", TaxBill_Installment = "", TaxBill_PayBy = "", TaxBill_Amount = "", TaxBill_Credits = "", TaxBill_Balance = "", TaxBill_Interest = "", TaxBill_Due = "-", TaxInfo_Details = "-";
            string Account_id_number = "", Owner_name = "", Address = "", Legal_Description = "", Year_Built = "", use = "", Principal_Residence = "", Sections = "";
            string Sub_District = "", Assessment_Year = "", Homestead_Application_Status = "", Homeowners_Tax_Credit_Application_Status = "", Homeowners_Tax_Credit_Application_Date = "";
            string Bill = "", tax_Type = "", Year = "", Tax_Owner = "", Tax_paid = "", TaxHistory_Info = "", Bill_tax_Year = "", Bill_tax = "", Bill_Owner = "", TaxPay_Activity = "", TaxPay_Posted = "", TaxPay_Reference = "", TaxPay_Amount = "", TaxPay_Details = "";
            string TaxBill_Installment1 = "", TaxBill_PayBy1 = "", TaxBill_Amount1 = "", TaxBill_Credits1 = "", TaxBill_Balance1 = "", TaxBill_Interest1 = "", TaxBill_Due1 = "", TaxInfo_Details1 = "", TaxInfo_Details2 = "";
            string TaxCharge_Entity = "", TaxCharge_taxable = "", TaxCharge_taxRate = "", TaxCharge_Amount = "", TaxCharge_Details = "", TaxCharge_Entity1 = "", TaxCharge_taxable1 = "", TaxCharge_taxRate1 = "", TaxCharge_Amount1 = "", TaxCharge_Details1 = "", TaxCharge_Entity2 = "", TaxCharge_Amount2 = "", TaxCharge_Details2 = "", Good_through_date = "";

            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                //driver = new ChromeDriver();
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    if (searchType == "titleflex")
                    {
                        string titleaddress = houseno + " " + sname + " " + direction + " " + directParcel;
                        gc.TitleFlexSearch(orderNumber, parcelNumber, ownername, titleaddress.Trim(), "MD", "Washington");

                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_MDWashington"] = "Yes";
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
                        selectElement1.SelectByText("WASHINGTON COUNTY");
                        var SerachCategoryM = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucSearchType_ddlSearchType']"));
                        var selectElementM1 = new SelectElement(SerachCategoryM);
                        selectElementM1.SelectByText("STREET ADDRESS");
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "MD", "Washington");
                        driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_StartNavigationTemplateContainerID_btnContinue']")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);

                        driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtStreenNumber']")).SendKeys(houseno);
                        driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtStreetName']")).SendKeys(sname);
                        driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_StepNavigationTemplateContainerID_btnStepNextButton']")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Address search Result", driver, "MD", "Washington");

                        try
                        {
                            IWebElement MultiAddressTable = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucSearchResult_gv_SearchResult']/tbody"));
                            IList<IWebElement> MultiAddressRow = MultiAddressTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiAddressTD;

                            int maxCheck = 0;
                            gc.CreatePdf_WOP(orderNumber, "MultiAddress", driver, "MD", "Washington");

                            foreach (IWebElement row1 in MultiAddressRow)
                            {
                                if (maxCheck <= 25)
                                {
                                    MultiAddressTD = row1.FindElements(By.TagName("td"));
                                    if (MultiAddressTD.Count != 0)
                                    {
                                        owner = MultiAddressTD[0].Text;
                                        Accnumber = MultiAddressTD[1].Text;
                                        PropertyAddress = MultiAddressTD[2].Text;
                                        Multi = owner + "~" + PropertyAddress;
                                        gc.insert_date(orderNumber, Accnumber, 1343, Multi, 1, DateTime.Now);
                                    }
                                }
                            }
                            HttpContext.Current.Session["MDWashignton"] = "Yes";

                            if (MultiAddressRow.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_MDWashignton_Maximum"] = "Maximum";
                            }
                            driver.Quit();
                            return "MultiParcel";
                        }
                        catch { }
                    }

                    if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("https://sdat.dat.maryland.gov/RealProperty/Pages/default.aspx");
                        Thread.Sleep(2000);
                        var SerachCategory = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucSearchType_ddlCounty']"));
                        var selectElement1 = new SelectElement(SerachCategory);
                        selectElement1.SelectByText("WASHINGTON COUNTY");
                        var SerachCategoryM = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucSearchType_ddlSearchType']"));
                        var selectElementM1 = new SelectElement(SerachCategoryM);
                        selectElementM1.SelectByText("PROPERTY ACCOUNT IDENTIFIER");
                        Thread.Sleep(3000);
                        driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_StartNavigationTemplateContainerID_btnContinue']")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);

                        Pa = parcelNumber.Replace("-", "");
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
                        driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtDistrict']")).SendKeys(Pa1);
                        driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtAccountIdentifier']")).SendKeys(Pa2);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "MD", "Washington");
                        driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_StepNavigationTemplateContainerID_btnStepNextButton']")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_lblErr"));
                        if (INodata.Text.Contains("no records that match your criteria"))
                        {
                            HttpContext.Current.Session["Nodata_MDWashington"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    //property details
                    Account_id_number = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblDetailsStreetHeader_0']")).Text.Replace("Folio:", "");
                    Account_id_number = GlobalClass.After(Account_id_number, "Account Number").Replace("-", "").Trim();
                    value = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblDetailsStreetHeader_0']")).Text.Replace("Folio:", "");
                    value = gc.Between(value, "District -", "Account Number -").Replace(" ", "");

                    Parcel_ID = value + Account_id_number;
                    gc.CreatePdf(orderNumber, Parcel_ID, "Property and Assessment details", driver, "MD", "Washington");

                    string Owner_name1 = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblOwnerName_0")).Text.Trim();
                    string Ownarname = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblOwnerName2_0")).Text.Trim();
                    Owner_name = Owner_name1 + " " + Ownarname;
                    Address = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblPremisesAddress_0")).Text.Trim();
                    Address = Address.Replace("\r\n", " ");

                    Mail_Address = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblMailingAddress_0")).Text.Trim();
                    Mail_Address = Mail_Address.Replace("\r\n", " ");

                    Legal_Description = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblLegalDescription_0")).Text.Trim();
                    Year_Built = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_Label18_0")).Text.Trim();
                    use = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblUse_0")).Text.Trim();
                    Principal_Residence = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblPrinResidence_0")).Text.Trim();
                    Map = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_Label5_0")).Text.Trim();
                    Grid = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_Label6_0")).Text.Trim();
                    Parcel = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_Label7_0")).Text.Trim();
                    Sub_District = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_Label8_0")).Text.Trim();
                    Subdivision = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_Label9_0")).Text.Trim();
                    Sections = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_Label10_0")).Text.Trim();
                    Block = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_Label11_0")).Text.Trim();
                    Lot = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_Label12_0")).Text.Trim();
                    Assessment_Year = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_Label13_0")).Text.Trim();
                    Homestead_Application_Status = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblHomeStatus_0")).Text.Trim();
                    Homeowners_Tax_Credit_Application_Status = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblHTC_Status_0")).Text.Trim();
                    Homeowners_Tax_Credit_Application_Date = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblHTC_StatusDate_0")).Text.Trim();
                    string Town = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_Label15_0")).Text;
                    property_details = Owner_name + "~" + Address + "~" + Mail_Address + "~" + Legal_Description + "~" + Town + "~" + Year_Built + "~" + use + "~" + Principal_Residence + "~" + Map + "~" + Grid + "~" + Parcel + "~" + Sub_District + "~" + Subdivision + "~" + Sections + "~" + Block + "~" + Lot + "~" + Assessment_Year + "~" + Homestead_Application_Status + "~" + Homeowners_Tax_Credit_Application_Status + "~" + Homeowners_Tax_Credit_Application_Date;
                    gc.insert_date(orderNumber, Parcel_ID, 1344, property_details, 1, DateTime.Now);

                    //Assessment Details Table:

                    Land = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblBaseLand_0")).Text.Trim();
                    Building = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblBaseImprove_0")).Text.Trim();
                    Total_Assessed_Value = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblBaseTotal_0")).Text.Trim();

                    Assessment_details = Land + "~" + Building + "~" + Total_Assessed_Value;
                    gc.insert_date(orderNumber, Parcel_ID, 1345, Assessment_details, 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    //Tax Information

                    driver.Navigate().GoToUrl("https://payments.washco-md.net/MSS/citizens/RealEstate/Default.aspx?mode=new");
                    Thread.Sleep(3000);

                    tax_Parcelid = Parcel_ID.Replace("-", "");
                    driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_ParcelIdSearchFieldLayout_ctl01_ParcelIDTextBox")).SendKeys(tax_Parcelid);
                    gc.CreatePdf(orderNumber, tax_Parcelid, "Tax_info", driver, "MD", "Washington");
                    driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_FormLayoutItem7_ctl01_Button1")).SendKeys(Keys.Enter);
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, tax_Parcelid, "Tax view Bill", driver, "MD", "Washington");

                    try
                    {

                    }
                    catch
                    { }
                    List<string> ParcelSearch = new List<string>();
                    List<string> viewlien = new List<string>();
                    try
                    {
                        int viewline = 0;
                        int currentyear = DateTime.Now.Year; int currentyear1 = DateTime.Now.Year - 1; int currentyear2 = DateTime.Now.Year - 2;
                        int month = DateTime.Now.Month;
                        if (month > 9)
                        {
                            currentyear++;

                        }
                        IWebElement ParcelTB = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_BillsGridView']/tbody"));
                        IList<IWebElement> ParcelTR = ParcelTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> parcelid;
                        IList<IWebElement> parcelth;

                        foreach (IWebElement parcel in ParcelTR)
                        {
                            parcelth = parcel.FindElements(By.TagName("th"));
                            parcelid = parcel.FindElements(By.TagName("td"));
                            try
                            {
                                if ((parcelid[4].Text.Contains(Convert.ToString(currentyear)) || parcelid[4].Text.Contains(Convert.ToString(currentyear1)) || parcelid[4].Text.Contains(Convert.ToString(currentyear2))) && parcelid[6].Text.Contains("View Bill") && parcelid.Count != 0)
                                {
                                    IWebElement ParcelBill_link = parcelid[6].FindElement(By.TagName("a"));
                                    string Parcelurl = ParcelBill_link.GetAttribute("id");
                                    ParcelSearch.Add(Parcelurl);
                                }
                                if (parcelid[7].Text.Contains("View Lien") && viewline == 0)

                                {
                                    IWebElement ParcelBill_link = parcelid[7].FindElement(By.TagName("a"));
                                    string Parcelurl = ParcelBill_link.GetAttribute("id");
                                    viewlien.Add(Parcelurl);
                                    viewline++;
                                }
                            }
                            catch { }
                        }
                    }
                    catch { }
                    string currwindow = driver.Url;
                    int i = 0;
                    foreach (string Taxclick in ParcelSearch)
                    {
                        driver.Navigate().GoToUrl(currwindow);
                        Thread.Sleep(3000);
                        driver.FindElement(By.Id(Taxclick)).Click();
                        Thread.Sleep(2000);
                        try
                        {
                            //Good Through Details
                            string Bill_Flag = "";
                            try
                            {
                                Bill_Flag = driver.FindElement(By.XPath("//*[@id='innerContent']/p[2]")).Text;
                            }
                            catch { }
                            try
                            {
                                IWebElement Intresttable = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_BillDetailsUpdatePanel']/table/tbody"));
                                IList<IWebElement> Intrestrow1 = Intresttable.FindElements(By.TagName("tr"));
                                IList<IWebElement> IntrestTD;

                                foreach (IWebElement Inrest in Intrestrow1)
                                {
                                    IntrestTD = Inrest.FindElements(By.TagName("td"));
                                    if (Inrest.Text.Contains("TOTAL"))
                                    {
                                        Bill_Flag = IntrestTD[3].Text.Trim();
                                    }
                                }
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
                                if (Bill_Flag == "Parcel is in tax sale" || Bill_Flag.Trim() != "$0.00")
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
                                            IWebElement Inextmonth = driver.FindElement(By.XPath("//*[@id='ui-datepicker-div']/div/a[2]"));
                                            Inextmonth.Click();
                                        }
                                        else
                                        {
                                            int nxtYr = Convert.ToInt16(DateTime.Now.ToString("yyyy")) + 1;
                                            Good_through_date = new DateTime(nxtYr, 1, DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), 1)).ToString("dd/MM/yyyy");

                                        }
                                    }
                                    driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_AsOfDateTextBox")).Clear();
                                    string[] daysplit = Good_through_date.Split('/');
                                    Thread.Sleep(5000);
                                    //*[@id="ui-datepicker-div"]/div/a[2]
                                    //driver.FindElement(By.XPath("//*[@id='ui-datepicker-div']/div/a[2]")).Click();
                                    IWebElement Iday = driver.FindElement(By.XPath("//*[@id='ui-datepicker-div']/table/tbody"));
                                    IList<IWebElement> IdayRow = Iday.FindElements(By.TagName("a"));
                                    foreach (IWebElement day in IdayRow)
                                    {
                                        if (day.Text != "" && day.Text == daysplit[1])
                                        {
                                            day.SendKeys(Keys.Enter);
                                        }
                                    }
                                    // driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_AsOfDateTextBox")).Click();
                                    Thread.Sleep(5000);
                                }
                            }
                        }
                        catch
                        { }
                        gc.CreatePdf_Chrome(orderNumber, tax_Parcelid, "ViewBill" + i, driver, "MD", "Washington");
                        Bill_tax_Year = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_BillYearRow']/td")).Text;
                        Bill_tax = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_BillNumberRow']/td")).Text;
                        Bill_Owner = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_OwnerRow']/td")).Text;
                        IWebElement TaxInfoTB = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_BillDetailsUpdatePanel']/table/tbody"));
                        IList<IWebElement> TaxInfoTR = TaxInfoTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> TaxInfoTD;
                        foreach (IWebElement TaxInfo in TaxInfoTR)
                        {
                            TaxInfoTD = TaxInfo.FindElements(By.TagName("td"));
                            if (TaxInfoTD.Count != 0 && TaxInfoTD.Count == 7 && !TaxInfo.Text.Contains("Installment"))
                            {
                                TaxBill_Installment = TaxInfoTD[0].Text;
                                TaxBill_PayBy = TaxInfoTD[1].Text;
                                TaxBill_Amount = TaxInfoTD[2].Text;
                                TaxBill_Credits = TaxInfoTD[3].Text;
                                TaxBill_Balance = TaxInfoTD[4].Text;
                                TaxBill_Interest = TaxInfoTD[5].Text;
                                TaxBill_Due = TaxInfoTD[6].Text;

                                TaxInfo_Details = Bill_tax_Year + "~" + Bill_tax + "~" + Bill_Owner + "~" + TaxBill_Installment + "~" + TaxBill_PayBy + "~" + TaxBill_Amount + "~" + TaxBill_Credits + "~" + TaxBill_Balance + "~" + TaxBill_Interest + "~" + TaxBill_Due + "~" + Good_through_date;
                                gc.insert_date(orderNumber, tax_Parcelid, 1347, TaxInfo_Details, 1, DateTime.Now);
                            }
                            if (TaxInfoTD.Count == 6)
                            {
                                TaxBill_Installment1 = TaxInfoTD[0].Text;
                                TaxBill_PayBy1 = "";
                                TaxBill_Amount1 = TaxInfoTD[1].Text;
                                TaxBill_Credits1 = TaxInfoTD[2].Text;
                                TaxBill_Balance1 = TaxInfoTD[3].Text;
                                TaxBill_Interest1 = TaxInfoTD[4].Text;
                                TaxBill_Due1 = TaxInfoTD[5].Text;
                                TaxInfo_Details = Bill_tax_Year + "~" + Bill_tax + "~" + Bill_Owner + "~" + TaxBill_Installment1 + "~" + TaxBill_PayBy1 + "~" + TaxBill_Amount1 + "~" + TaxBill_Credits1 + "~" + TaxBill_Balance1 + "~" + TaxBill_Interest1 + "~" + TaxBill_Due1 + "~" + Good_through_date;
                                gc.insert_date(orderNumber, tax_Parcelid, 1347, TaxInfo_Details, 1, DateTime.Now);
                            }
                        }

                        //Tax Pay/Adjust 
                        try
                        {
                            driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_ViewPaymentsLinkButton")).Click();
                            Thread.Sleep(2000);

                            gc.CreatePdf_Chrome(orderNumber, tax_Parcelid, "Tax PayAdjust" + i, driver, "MD", "Washington");

                            IWebElement TaxPayTB = driver.FindElement(By.XPath("//*[@id='innerContent']/table[2]/tbody"));
                            IList<IWebElement> TaxPayTR = TaxPayTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> TaxPayTD;

                            foreach (IWebElement TaxPay in TaxPayTR)
                            {
                                TaxPayTD = TaxPay.FindElements(By.TagName("td"));
                                if (TaxPayTD.Count != 0 && !TaxPay.Text.Contains("Activity"))
                                {
                                    TaxPay_Activity = TaxPayTD[0].Text;
                                    TaxPay_Posted = TaxPayTD[1].Text;
                                    TaxPay_Reference = TaxPayTD[2].Text;
                                    TaxPay_Amount = TaxPayTD[3].Text;

                                    TaxPay_Details = Bill_tax_Year + "~" + TaxPay_Activity + "~" + TaxPay_Posted + "~" + TaxPay_Reference + "~" + TaxPay_Amount;
                                    gc.insert_date(orderNumber, tax_Parcelid, 1348, TaxPay_Details, 1, DateTime.Now);
                                }
                            }
                        }
                        catch
                        { }
                        //Tax Charges
                        driver.FindElement(By.LinkText("Charges")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_Chrome(orderNumber, tax_Parcelid, "Tax Charge" + i, driver, "MD", "Washington");
                        IWebElement TaxChargeTB = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_TaxChargesTable']"));
                        IList<IWebElement> TaxChargeTR = TaxChargeTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> TaxChargeTD;
                        IList<IWebElement> TaxChargeTH;

                        foreach (IWebElement TaxCharge in TaxChargeTR)
                        {
                            TaxChargeTD = TaxCharge.FindElements(By.TagName("td"));
                            TaxChargeTH = TaxCharge.FindElements(By.TagName("th"));
                            if (TaxChargeTD.Count != 0 && TaxChargeTD.Count == 3 && !TaxCharge.Text.Contains("Taxable Value") && TaxChargeTH.Count == 1)
                            {
                                TaxCharge_Entity = TaxChargeTH[0].Text;
                                TaxCharge_taxable = TaxChargeTD[0].Text;
                                TaxCharge_taxRate = TaxChargeTD[1].Text;
                                TaxCharge_Amount = TaxChargeTD[2].Text;

                                TaxCharge_Details = Bill_tax_Year + "~" + TaxCharge_Entity + "~" + TaxCharge_taxable + "~" + TaxCharge_taxRate + "~" + TaxCharge_Amount;
                                gc.insert_date(orderNumber, tax_Parcelid, 1349, TaxCharge_Details, 1, DateTime.Now);
                            }
                            if (TaxChargeTD.Count == 1 && TaxChargeTH.Count == 1)
                            {
                                TaxCharge_Entity1 = TaxChargeTH[0].Text;
                                TaxCharge_taxable1 = "";
                                TaxCharge_taxRate1 = "";
                                TaxCharge_Amount1 = TaxChargeTD[0].Text;

                                TaxCharge_Details1 = Bill_tax_Year + "~" + TaxCharge_Entity1 + "~" + "" + "~" + "" + "~" + TaxCharge_Amount1;
                                gc.insert_date(orderNumber, tax_Parcelid, 1349, TaxCharge_Details1, 1, DateTime.Now);
                            }
                        }

                        TaxCharge_Entity2 = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_TotalTaxTable']/tbody/tr/th")).Text;
                        TaxCharge_Amount2 = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_TotalTaxTable']/tbody/tr/td")).Text;

                        TaxCharge_Details2 = Bill_tax_Year + "~" + TaxCharge_Entity2 + "~" + "" + "~" + "" + "~" + TaxCharge_Amount2;
                        gc.insert_date(orderNumber, tax_Parcelid, 1349, TaxCharge_Details2, 1, DateTime.Now);
                        //property Detail  
                        driver.FindElement(By.LinkText("Property Detail")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_Chrome(orderNumber, tax_Parcelid, "property Detail" + i, driver, "MD", "Washington");
                        //Owner Information
                        driver.FindElement(By.LinkText("Owner Information")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_Chrome(orderNumber, tax_Parcelid, "Owner Information" + i, driver, "MD", "Washington");
                        //assessment
                        driver.FindElement(By.LinkText("Assessment")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_Chrome(orderNumber, tax_Parcelid, "Assessment" + i, driver, "MD", "Washington");
                        IWebElement assessmenttable = driver.FindElement(By.XPath("//*[@id='innerContent']/table[2]/tbody"));
                        IList<IWebElement> assessmentrow = assessmenttable.FindElements(By.TagName("tr"));
                        IList<IWebElement> assessmentidtr;
                        IList<IWebElement> assessmentidth;
                        foreach (IWebElement assessment in assessmentrow)
                        {
                            assessmentidtr = assessment.FindElements(By.TagName("td"));
                            assessmentidth = assessment.FindElements(By.TagName("th"));
                            if (assessmentidtr.Count != 0)
                            {
                                string assessmentresult = Bill_tax_Year + "~" + assessmentidth[0].Text + "~" + assessmentidtr[0].Text;
                                gc.insert_date(orderNumber, tax_Parcelid, 1474, assessmentresult, 1, DateTime.Now);
                            }
                        }
                        //Assessment History
                        driver.FindElement(By.LinkText("Assessment History")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_Chrome(orderNumber, tax_Parcelid, "Assessment History" + i, driver, "MD", "Washington");
                        //Tax rate
                        driver.FindElement(By.LinkText("Tax Rates")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_Chrome(orderNumber, tax_Parcelid, "Tax Rates" + i, driver, "MD", "Washington");
                        //All bills
                        try
                        {
                            driver.FindElement(By.LinkText("All Bills")).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf_Chrome(orderNumber, tax_Parcelid, "Tax Uitility Bill" + i, driver, "MD", "Washington");
                            if (i == 0)
                            {
                                IWebElement TaxHistoryTable = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_BillsRepeater_ctl00_BillsGrid']/tbody"));
                                IList<IWebElement> TaxHistoryTR = TaxHistoryTable.FindElements(By.TagName("tr"));
                                IList<IWebElement> TaxHistoryTD;
                                foreach (IWebElement TaxHistory in TaxHistoryTR)
                                {
                                    TaxHistoryTD = TaxHistory.FindElements(By.TagName("td"));
                                    if (!TaxHistory.Text.Contains("Year"))
                                    {
                                        Bill = TaxHistoryTD[0].Text;
                                        tax_Type = TaxHistoryTD[1].Text;
                                        Year = TaxHistoryTD[2].Text;
                                        Tax_Owner = TaxHistoryTD[3].Text;
                                        Tax_paid = TaxHistoryTD[4].Text;
                                        TaxHistory_Info = Bill + "~" + tax_Type + "~" + Year + "~" + Tax_Owner + "~" + Tax_paid;
                                        gc.insert_date(orderNumber, tax_Parcelid, 1346, TaxHistory_Info, 1, DateTime.Now);
                                    }

                                }
                            }
                        }
                        catch
                        { }
                        i++;
                    }
                    if (viewlien.Count != 0)
                    {
                        List<string> Viewbillline = new List<string>();
                        foreach (string Line in viewlien)
                        {
                            driver.Navigate().GoToUrl(currwindow);
                            Thread.Sleep(3000);
                            driver.FindElement(By.Id(Line)).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf_Chrome(orderNumber, tax_Parcelid, "line view detail", driver, "MD", "Washington");
                            IWebElement LineFoundTable = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_BillsGrid']/tbody"));
                            IList<IWebElement> Linefoundrow = LineFoundTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> linefoundid;
                            foreach (IWebElement Linefound in Linefoundrow)
                            {
                                linefoundid = Linefound.FindElements(By.TagName("td"));
                                //if (linefoundid.Count != 0)
                                //{
                                //    string Linefoundresult = linefoundid[1].Text + "~" + linefoundid[2].Text + "~" + linefoundid[3].Text + "~" + linefoundid[4].Text;
                                //    gc.insert_date(orderNumber, tax_Parcelid, 1456, Linefoundresult, 1, DateTime.Now);
                                //}
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
                            gc.CreatePdf_Chrome(orderNumber, tax_Parcelid, "line Bill detail" + Linevi, driver, "MD", "Washington");
                            linebillyear = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_FiscalYearLabel")).Text;
                            linebill = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_BillNumberLabel")).Text;
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


                                    TaxInfo_Details = linebillyear + "~" + linebill + "~" + lineownername + "~" + TaxBill_Installment + "~" + TaxBill_PayBy + "~" + TaxBill_Amount + "~" + TaxBill_Credits + "~" + TaxBill_Balance + "~" + TaxBill_Interest;
                                    gc.insert_date(orderNumber, tax_Parcelid, 1455, TaxInfo_Details, 1, DateTime.Now);
                                }
                                if (TaxInfoTD.Count == 5)
                                {
                                    TaxBill_Installment1 = TaxInfoTD[0].Text;
                                    TaxBill_PayBy1 = "";
                                    TaxBill_Amount1 = TaxInfoTD[1].Text;
                                    TaxBill_Credits1 = TaxInfoTD[2].Text;
                                    TaxBill_Balance1 = TaxInfoTD[3].Text;
                                    TaxBill_Interest1 = TaxInfoTD[4].Text;
                                    TaxInfo_Details = linebillyear + "~" + linebill + "~" + lineownername + "~" + TaxBill_Installment1 + "~" + TaxBill_PayBy1 + "~" + TaxBill_Amount1 + "~" + TaxBill_Credits1 + "~" + TaxBill_Balance1 + "~" + TaxBill_Interest1;
                                    gc.insert_date(orderNumber, tax_Parcelid, 1455, TaxInfo_Details, 1, DateTime.Now);
                                }
                            }
                            driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_PaymentsAdjustmentsLinkButton")).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf_Chrome(orderNumber, tax_Parcelid, "line Payment detail" + Linevi, driver, "MD", "Washington");
                            IWebElement assessmenttable1 = driver.FindElement(By.XPath("//*[@id='innerContent']/table[2]/tbody"));
                            IList<IWebElement> assessmentrow1 = assessmenttable1.FindElements(By.TagName("tr"));
                            IList<IWebElement> assessmentidtr1;
                            IList<IWebElement> assessmentidth;
                            foreach (IWebElement assessment1 in assessmentrow1)
                            {
                                assessmentidtr1 = assessment1.FindElements(By.TagName("td"));
                                assessmentidth = assessment1.FindElements(By.TagName("th"));
                                if (assessmentidtr1.Count != 0)
                                {
                                    string assessmentresult1 = linebillyear + "~" + assessmentidtr1[0].Text + "~" + assessmentidtr1[1].Text + "~" + assessmentidtr1[2].Text + "~" + assessmentidtr1[3].Text;
                                    gc.insert_date(orderNumber, tax_Parcelid, 1477, assessmentresult1, 1, DateTime.Now);
                                }
                            }
                            driver.FindElement(By.XPath("//*[@id='innerContent']/ul/li[2]/a")).Click();
                            Thread.Sleep(2000);
                            Linevi++;
                        }

                    }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "MD", "Washington", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "MD", "Washington");
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