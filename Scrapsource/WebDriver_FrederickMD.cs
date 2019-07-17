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
    public class WebDriver_FrederickMD
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;

        public string FTP_Frederick(string houseno, string sname, string direction, string account, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string Address = houseno + " " + direction + " " + sname;
            //if (Address.Trim() != "" && !Address.Trim().Contains("UNIT") && !Address.Trim().Contains("APT") && !Address.Trim().Contains("#"))
            //{
            //    searchType = "address";
            //}
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
                        string titleaddress = houseno + " " + direction + " " + sname + " " + account;
                        gc.TitleFlexSearch(orderNumber, "", ownername, titleaddress, "MD", "Frederick");

                       
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_FrederickMD"] = "Yes";
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


                        var SerachCategory = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucSearchType_ddlCounty"));
                        var selectElement1 = new SelectElement(SerachCategory);
                        selectElement1.SelectByText("FREDERICK COUNTY");

                        var SerachCategoryM = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucSearchType_ddlSearchType"));
                        var selectElementM1 = new SelectElement(SerachCategoryM);
                        selectElementM1.SelectByText("STREET ADDRESS");
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "MD", "Frederick");
                        driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_StartNavigationTemplateContainerID_btnContinue")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);

                        driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtStreenNumber")).SendKeys(houseno);
                        driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtStreetName")).SendKeys(sname);
                        driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_StepNavigationTemplateContainerID_btnStepNextButton")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Address search Result", driver, "MD", "Frederick");


                        //Multiparcel

                        try
                        {
                            IWebElement MultiOwnerTable = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucSearchResult_gv_SearchResult']/tbody"));
                            IList<IWebElement> MultiOwnerRow = MultiOwnerTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiOwnerTD;
                            try
                            {
                                if (MultiOwnerRow.Count > 28)
                                {
                                    HttpContext.Current.Session["MDFrederick_Count"] = "Count";
                                    return "Count";
                                }
                            }
                            catch { }
                            //string AlterNateID = "", PropertyAddress = "", LegalDescriptoin = "", YearBuilt = "";
                            foreach (IWebElement row1 in MultiOwnerRow)
                            {
                                MultiOwnerTD = row1.FindElements(By.TagName("td"));
                                if (MultiOwnerTD.Count != 0)
                                {
                                    owner = MultiOwnerTD[0].Text;
                                    Accnumber = MultiOwnerTD[1].Text;
                                    PropertyAddress = MultiOwnerTD[2].Text;
                                    string Multi = owner + "~" + PropertyAddress;
                                    gc.insert_date(orderNumber, Accnumber, 1245, Multi, 1, DateTime.Now);
                                }
                            }

                            HttpContext.Current.Session["FrederickMD"] = "Yes";
                            driver.Quit();
                            return "MultiParcel";
                        }
                        catch { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_lblErr")).Text;
                            if (nodata.Contains("There are no records that match your criteria"))
                            {
                                HttpContext.Current.Session["Nodata_FrederickMD"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    else if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("https://sdat.dat.maryland.gov/RealProperty/Pages/default.aspx");
                        Thread.Sleep(2000);
                        var SerachCategory = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucSearchType_ddlCounty']"));
                        var selectElement1 = new SelectElement(SerachCategory);
                        selectElement1.SelectByText("FREDERICK COUNTY");
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
                        driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtDistrict']")).SendKeys(Pa1);
                        driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtAccountIdentifier']")).SendKeys(Pa2);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "MD", "Frederick");
                        driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_StepNavigationTemplateContainerID_btnStepNextButton']")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_lblErr")).Text;
                            if (nodata.Contains("There are no records that match your criteria for county: FREDERICK COUNTY, account identifier: , , ."))
                            {
                                HttpContext.Current.Session["Nodata_FrederickMD"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    //Property Details Table
                    string AccountNumber = "", PremisesAddress = "", LegalDescription = "", Parcel = "", Use = "", PrincipalResidence = "", Map = "", Grid = "", SubDistrict = "", Subdivision = "", Section = "", Block = "", Lot = "", AssessmentYear = "", Town = "";
                    string HomesteadApplicationStatus = "", HomeownersTaxCreditApplicationStatus = "", HomeownersTaxCreditApplicationDate = "", District = "";
                    parcelNumber = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblDetailsStreetHeader_0")).Text.Replace("Folio:", "");
                    District = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblDetailsStreetHeader_0")).Text.Replace("Folio:", "");
                    parcelNumber = GlobalClass.After(parcelNumber, "Account Number - ");
                    District = gc.Between(District, "District -", "Account Number -");

                    gc.CreatePdf(orderNumber, parcelNumber, "Assessment information", driver, "MD", "Frederick");

                    AccountNumber = District.Trim() + "-" + parcelNumber.Trim();
                    ownername = driver.FindElement(By.XPath("//*[@id='detailSearch']/tbody/tr[5]/td[2]")).Text.Replace("\r\n", "");
                    PropertyAddress = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblMailingAddress_0")).Text;
                    LegalDescription = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblLegalDescription_0")).Text.Replace("\r\n", "  ");
                    YearBuilt = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_Label18_0")).Text;
                    Use = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblUse_0")).Text;
                    PrincipalResidence = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblPrinResidence_0")).Text;
                    PremisesAddress = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblPremisesAddress_0")).Text;
                    Map = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_Label5_0")).Text;
                    Grid = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_Label6_0")).Text;
                    Parcel = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_Label7_0")).Text;
                    SubDistrict = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_Label8_0")).Text;
                    Subdivision = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_Label9_0")).Text;
                    Section = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_Label10_0")).Text;
                    Block = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_Label11_0")).Text;
                    Lot = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_Label12_0")).Text;
                    AssessmentYear = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_Label13_0")).Text;
                    //Town = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_Label15_0']")).Text;
                    HomesteadApplicationStatus = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblHomeStatus_0")).Text;
                    HomeownersTaxCreditApplicationStatus = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblHTC_Status_0")).Text;
                    HomeownersTaxCreditApplicationDate = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblHTC_StatusDate_0")).Text;
                    //Parcel = driver.FindElement(By.XPath("//*[@id='property_info']/tbody/tr[3]/td/div")).Text;
                    string Property = ownername.Trim() + "~" + PropertyAddress.Trim() + "~" + PremisesAddress.Trim() + "~" + LegalDescription.Trim() + "~" + YearBuilt.Trim() + "~" + Use.Trim() + "~" + PrincipalResidence.Trim() + "~" + Map.Trim() + "~" + Grid.Trim() + "~" + Parcel.Trim() + "~" + SubDistrict.Trim() + "~" + Subdivision.Trim() + "~" + Section.Trim() + "~" + Block.Trim() + "~" + Lot.Trim() + "~" + AssessmentYear.Trim() + "~" + HomesteadApplicationStatus.Trim() + "~" + HomeownersTaxCreditApplicationStatus.Trim() + "~" + HomeownersTaxCreditApplicationDate.Trim();
                    gc.insert_date(orderNumber, AccountNumber, 1246, Property, 1, DateTime.Now);

                    //Taxing Authority
                    string TaxingAuthority = "", TaxingAuthority1 = "", TaxingAuthority2 = "", TaxingAuthority3 = "";
                    try
                    {
                        driver.Navigate().GoToUrl("https://frederickcountymd.gov/69/Treasury");
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Authority", driver, "MD", "Frederick");
                        IWebElement taxx = driver.FindElement(By.XPath("//*[@id='divInfoAdv1ee99197-d700-46ae-9ac2-e02a5ab9ce36']/div[1]/div/div/ol/li"));
                        TaxingAuthority1 = gc.Between(taxx.Text, "Treasury Department", "Frederick").Trim();
                        TaxingAuthority2 = gc.Between(taxx.Text, "St.", "Ph:").Trim();
                        TaxingAuthority3 = gc.Between(taxx.Text, "21701", "Fx:").Trim();
                        TaxingAuthority = TaxingAuthority1.Trim() + "  " + TaxingAuthority2.Trim() + "  " + TaxingAuthority3.Trim();
                    }
                    catch { }


                    //Assessment Details
                    string ParcelNumber = "";
                    driver.Navigate().GoToUrl("https://frederickcountymd.munisselfservice.com/citizens/RealEstate/Default.aspx?mode=new");

                    ParcelNumber = AccountNumber.Replace("-", "").Trim();

                    driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_ParcelIdSearchFieldLayout_ctl01_ParcelIDTextBox")).SendKeys(ParcelNumber);
                    driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_FormLayoutItem7_ctl01_Button1")).SendKeys(Keys.Enter);
                    Thread.Sleep(2000);
                    try
                    {
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Information", driver, "MD", "Frederick");
                        ByVisibleElement(driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_BillsGridView_ctl15_ViewBillLinkButton")));
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Infor Half Image1", driver, "MD", "Frederick");

                    }
                    catch { }

                    List<string> ParcelSearch = new List<string>();
                    try
                    {
                        //gc.CreatePdf(orderNumber, parcelNumber, "Tax Information click", driver, "MD", "Frederick");
                        IWebElement ParcelTB = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_BillsGridView"));
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
                                    IWebElement ParcelBill_link = Columns_row[6].FindElement(By.TagName("a"));
                                    string Parcelurl = ParcelBill_link.GetAttribute("id");
                                    ParcelSearch.Add(Parcelurl);
                                }
                            }
                        }
                    }
                    catch { }
                    foreach (string taxlink in ParcelSearch)
                    {

                        try
                        {
                            driver.Navigate().GoToUrl("https://frederickcountymd.munisselfservice.com/citizens/RealEstate/Default.aspx?mode=new");

                            ParcelNumber = AccountNumber.Replace("-", "").Trim();

                            driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_ParcelIdSearchFieldLayout_ctl01_ParcelIDTextBox")).SendKeys(ParcelNumber);
                            driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_FormLayoutItem7_ctl01_Button1")).SendKeys(Keys.Enter);
                            //Thread.Sleep(2000);
                            //gc.CreatePdf(orderNumber, parcelNumber, "Tax Information", driver, "MD", "Frederick");
                            //gc.CreatePdf(orderNumber, parcelNumber, "Tax Information", driver, "MD", "Frederick");
                        }
                        catch { }

                        driver.FindElement(By.Id(taxlink)).Click();
                        Thread.Sleep(9000);

                        string taxbill = driver.CurrentWindowHandle;

                        //Good Through Date
                        IWebElement IasofDateSearch;
                        string asofGood = "", parcelid1 = "", billyearGood = "", billGood = "", ownerGood = "", parcelidGood = "";
                        // Deliquent
                        int q = 0;
                        try
                        {
                            IWebElement Delinquent1 = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_BillDetailsUpdatePanel']/table/tbody/tr[2]/td[6]"));
                            if (Delinquent1.Text != "$0.00")
                            {
                                IasofDateSearch = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_AsOfDateTextBox']"));
                                IasofDateSearch.Clear();
                                string currDate = DateTime.Now.ToString("MM/dd/yyyy");
                                string dateChecking = DateTime.Now.ToString("MM") + "/15/" + DateTime.Now.ToString("yyyy");
                                if (Convert.ToDateTime(currDate) > Convert.ToDateTime(dateChecking))
                                {
                                    string nextEndOfMonth = "";
                                    if ((Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))) < 12))
                                    {
                                        nextEndOfMonth = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM")) + 1), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")) + 1)).ToString("MM/dd/yyyy");
                                    }
                                    else
                                    {
                                        int nxtYr = Convert.ToInt16(DateTime.Now.ToString("yyyy")) + 1;
                                        nextEndOfMonth = nextEndOfMonth = new DateTime(nxtYr, 1, DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), 1)).ToString("MM/dd/yyyy");
                                    }

                                    IasofDateSearch.SendKeys(nextEndOfMonth);
                                    asofGood = nextEndOfMonth;
                                }
                                else
                                {
                                    string EndOfMonth = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")))).ToString("MM/dd/yyyy");
                                    IasofDateSearch.SendKeys(EndOfMonth);
                                    asofGood = EndOfMonth;
                                }

                                driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_AsOfDateTextBox")).Clear();
                                string[] daysplit = asofGood.Split('/');
                                try
                                {
                                    IWebElement gclick = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_AsOfDateTextBox"));
                                    gclick.Click();
                                }
                                catch { }//*[@id="ui-datepicker-div"]/table

                                IWebElement Iday = driver.FindElement(By.XPath("//*[@id='ui-datepicker-div']/table"));
                                IList<IWebElement> IdayRow = Iday.FindElements(By.TagName("a"));
                                foreach (IWebElement day in IdayRow)
                                {
                                    if (day.Text != "" && day.Text == daysplit[1])
                                    {
                                        day.SendKeys(Keys.Enter);
                                        Thread.Sleep(2000);
                                        //gc.CreatePdf(orderNumber, ParcelNumber, "Good Through Date", driver, "MD", "Frederick");

                                    }
                                }

                                gc.CreatePdf(orderNumber, ParcelNumber, "View Bill Yearwise" + q, driver, "MD", "Frederick");
                                string asof = "", billyear = "", bill = "", viewbillowner = "";
                                IWebElement IasofDate = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_AsOfDateTextBox"));
                                asof = IasofDate.GetAttribute("value");

                                //asof = driver.FindElement(By.XPath("//*[@id='BillDetailTable']/tbody/tr[1]/td")).Text.Trim();
                                string bulktxt = driver.FindElement(By.XPath("//*[@id='BillDetailTable']/tbody")).Text;
                                string strbillyear = gc.Between(bulktxt, "Bill Year", "Owner");
                                billyear = GlobalClass.Before(strbillyear, "Bill");
                                bill = GlobalClass.After(strbillyear, "Bill");
                                viewbillowner = gc.Between(bulktxt, "Owner", "Parcel ID");
                                parcelid1 = GlobalClass.After(bulktxt, "Parcel ID");


                                try
                                {
                                    IWebElement Bigdata = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_BillDetailsUpdatePanel"));
                                    IList<IWebElement> TRBigdata = Bigdata.FindElements(By.TagName("tr"));
                                    IList<IWebElement> THBigdata = Bigdata.FindElements(By.TagName("th"));
                                    IList<IWebElement> TDBigdata;
                                    foreach (IWebElement row in TRBigdata)
                                    {
                                        TDBigdata = row.FindElements(By.TagName("td"));
                                        if (TDBigdata.Count == 7 && TDBigdata.Count != 0)
                                        {
                                            string taxdetails1 = asofGood + "~" + asof + "~" + billyear + "~" + bill + "~" + viewbillowner + "~" + parcelid1 + "~" + TDBigdata[0].Text + "~" + TDBigdata[1].Text + "~" + TDBigdata[2].Text + "~" + TDBigdata[3].Text + "~" + TDBigdata[4].Text + "~" + TDBigdata[5].Text + "~" + TDBigdata[6].Text + "~" + TaxingAuthority;

                                            gc.insert_date(orderNumber, parcelid1, 1249, taxdetails1, 1, DateTime.Now);
                                        }
                                        if (TDBigdata.Count == 6 && TDBigdata.Count != 0)
                                        {
                                            string taxdetails1 = asofGood + "~" + asof + "~" + billyear + "~" + bill + "~" + viewbillowner + "~" + parcelid1 + "~" + TDBigdata[0].Text + "~" + "" + "~" + TDBigdata[1].Text + "~" + TDBigdata[2].Text + "~" + TDBigdata[3].Text + "~" + TDBigdata[4].Text + "~" + TDBigdata[5].Text + "~" + TaxingAuthority;

                                            gc.insert_date(orderNumber, parcelid1, 1249, taxdetails1, 1, DateTime.Now);
                                        }

                                    }
                                }
                                catch (Exception ex) { }
                            }
                            q++;
                        }
                        catch { }
                        //Tax Bill Download

                        string bulktxt1 = driver.FindElement(By.XPath("//*[@id='BillDetailTable']/tbody")).Text;
                        string strbillyear1 = gc.Between(bulktxt1, "Bill Year", "Owner");
                        string billyear1 = GlobalClass.Before(strbillyear1, "Bill");

                        try
                        {
                            IWebElement IBillTax = driver.FindElement(By.LinkText("View bill image"));
                            IBillTax.Click();
                            Thread.Sleep(9000);
                            driver.SwitchTo().Window(driver.WindowHandles.Last());
                            Thread.Sleep(9000);
                            gc.CreatePdf(orderNumber, parcelNumber, "TaxBill Download" + billyear1, driver, "MD", "Frederick");
                            driver.Close();
                            driver.SwitchTo().Window(taxbill);

                        }
                        catch { }


                        //Tax Information after View bill details Scrap
                        ////view details click
                        IWebElement Delinquent = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_BillDetailsUpdatePanel']/table/tbody/tr[2]/td[6]"));
                        if (Delinquent.Text == "$0.00")
                        {
                            gc.CreatePdf(orderNumber, ParcelNumber, "View Bill Yearwise" + billyear1, driver, "MD", "Frederick");
                            string asof = "", billyear = "", bill = "", viewbillowner = "";
                            IWebElement IasofDate = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_AsOfDateTextBox"));
                            asof = IasofDate.GetAttribute("value");
                            Thread.Sleep(3000);
                            //asof = driver.FindElement(By.XPath("//*[@id='BillDetailTable']/tbody/tr[1]/td")).Text.Trim();
                            string bulktxt = driver.FindElement(By.XPath("//*[@id='BillDetailTable']/tbody")).Text;
                            string strbillyear = gc.Between(bulktxt, "Bill Year", "Owner");
                            billyear = GlobalClass.Before(strbillyear, "Bill");
                            bill = GlobalClass.After(strbillyear, "Bill");
                            viewbillowner = gc.Between(bulktxt, "Owner", "Parcel ID");
                            parcelid1 = GlobalClass.After(bulktxt, "Parcel ID");


                            try
                            {
                                IWebElement Bigdata = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_BillDetailsUpdatePanel"));
                                IList<IWebElement> TRBigdata = Bigdata.FindElements(By.TagName("tr"));
                                IList<IWebElement> THBigdata = Bigdata.FindElements(By.TagName("th"));
                                IList<IWebElement> TDBigdata;
                                foreach (IWebElement row in TRBigdata)
                                {
                                    TDBigdata = row.FindElements(By.TagName("td"));
                                    if (TDBigdata.Count == 7 && TDBigdata.Count != 0)
                                    {
                                        string taxdetails = "" + "~" + asof + "~" + billyear + "~" + bill + "~" + viewbillowner + "~" + parcelid1 + "~" + TDBigdata[0].Text + "~" + TDBigdata[1].Text + "~" + TDBigdata[2].Text + "~" + TDBigdata[3].Text + "~" + TDBigdata[4].Text + "~" + TDBigdata[5].Text + "~" + TDBigdata[6].Text + "~" + TaxingAuthority;

                                        gc.insert_date(orderNumber, parcelid1, 1249, taxdetails, 1, DateTime.Now);
                                    }
                                    if (TDBigdata.Count == 6 && TDBigdata.Count != 0)
                                    {
                                        string taxdetails = "" + "~" + asof + "~" + billyear + "~" + bill + "~" + viewbillowner + "~" + parcelid1 + "~" + TDBigdata[0].Text + "~" + "" + "~" + TDBigdata[1].Text + "~" + TDBigdata[2].Text + "~" + TDBigdata[3].Text + "~" + TDBigdata[4].Text + "~" + TDBigdata[5].Text + "~" + TaxingAuthority;

                                        gc.insert_date(orderNumber, parcelid1, 1249, taxdetails, 1, DateTime.Now);
                                    }

                                }
                            }
                            catch (Exception ex) { }
                        }

                        //Tax Payment Details
                        string strall_billyear = "", all_billyear = "", all_bill = "", all_billyear1 = "", all_bill1 = "";
                        try
                        {
                            int i = 0;
                            driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_ViewPaymentsLinkButton")).SendKeys(Keys.Enter);
                            Thread.Sleep(5000);
                            string allbills = driver.FindElement(By.XPath("//*[@id='BillActivityTable']/tbody")).Text;

                            strall_billyear = GlobalClass.After(allbills, "Year");
                            all_bill = GlobalClass.After(strall_billyear, "Bill");
                            all_billyear = GlobalClass.Before(strall_billyear, "Bill").Replace("\r\n", "");
                            gc.CreatePdf(orderNumber, parcelNumber, "Tax Payment Bill" + all_billyear, driver, "MD", "Frederick");
                            IWebElement allbill1 = driver.FindElement(By.Id("molContentContainer"));
                            IList<IWebElement> TRallbill1 = allbill1.FindElements(By.TagName("tr"));
                            IList<IWebElement> THallbill1 = allbill1.FindElements(By.TagName("th"));
                            IList<IWebElement> TDallbill1;
                            foreach (IWebElement text in TRallbill1)
                            {
                                TDallbill1 = text.FindElements(By.TagName("td"));
                                THallbill1 = text.FindElements(By.TagName("th"));

                                if (TDallbill1.Count != 0 && TDallbill1.Count == 1 && !text.Text.Contains("Paid By/Reference") && text.Text.Contains("Year"))
                                {
                                    all_billyear1 = TDallbill1[0].Text;

                                    //string Allbilldetails = all_billyear1 + "~" + all_bill + "~" + TDallbill1[0].Text + "~" + TDallbill1[1].Text + "~" + TDallbill1[2].Text + "~" + TDallbill1[3].Text;
                                    //gc.insert_date(orderNumber, ParcelNumber, 1256, Allbilldetails, 1, DateTime.Now);
                                }
                                if (TDallbill1.Count != 0 && TDallbill1.Count == 1 && !text.Text.Contains("Paid By/Reference") && !text.Text.Contains("Year"))
                                {
                                    all_bill1 = TDallbill1[0].Text;

                                    //string Allbilldetails = all_billyear1 + "~" + all_bill + "~" + TDallbill1[0].Text + "~" + TDallbill1[1].Text + "~" + TDallbill1[2].Text + "~" + TDallbill1[3].Text;
                                    //gc.insert_date(orderNumber, ParcelNumber, 1256, Allbilldetails, 1, DateTime.Now);
                                }
                                if (TDallbill1.Count != 0 && TDallbill1.Count == 4 && !text.Text.Contains("Paid By/Reference") && !text.Text.Contains("Bill Year"))
                                {
                                    //  string all_bill1 = TDallbill1[0].Text;

                                    string Allbilldetails = all_billyear1 + "~" + all_bill + "~" + TDallbill1[0].Text + "~" + TDallbill1[1].Text + "~" + TDallbill1[2].Text + "~" + TDallbill1[3].Text;
                                    gc.insert_date(orderNumber, ParcelNumber, 1256, Allbilldetails, 1, DateTime.Now);
                                }

                            }

                            i++;
                            driver.Navigate().Back();
                            //driver.Navigate().Back();
                        }
                        catch { }

                        //Tax Distribution Details (Charges Click)

                        driver.FindElement(By.LinkText("Charges")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Charges" + all_billyear, driver, "MD", "Frederick");

                        string Owner = "", Parcelid = "", ChargesBillyear = "", Tableheading = "", Taxablevalue = "", TaxRate = "", Amount = "";
                        IWebElement Bigdata2 = driver.FindElement(By.Id("molContentContainer"));
                        IList<IWebElement> TRBigdata2 = Bigdata2.FindElements(By.TagName("tr"));
                        IList<IWebElement> THBigdata2 = Bigdata2.FindElements(By.TagName("th"));
                        IList<IWebElement> TDBigdata2;
                        foreach (IWebElement row in TRBigdata2)
                        {
                            TDBigdata2 = row.FindElements(By.TagName("td"));
                            THBigdata2 = row.FindElements(By.TagName("th"));
                            if (TDBigdata2.Count != 0 && TDBigdata2.Count == 1 && !row.Text.Contains("Parcel ID") && !row.Text.Contains("Bill Year") && !row.Text.Contains("Total"))
                            {
                                Owner = TDBigdata2[0].Text.Trim();
                            }
                            if (TDBigdata2.Count != 0 && TDBigdata2.Count == 1 && !row.Text.Contains("Owner") && !row.Text.Contains("Bill Year") && !row.Text.Contains("Total"))
                            {
                                Parcelid = TDBigdata2[0].Text.Trim();
                            }
                            if (TDBigdata2.Count != 0 && TDBigdata2.Count == 1 && !row.Text.Contains("Owner") && !row.Text.Contains("Parcel ID") && !row.Text.Contains("Total"))
                            {
                                ChargesBillyear = TDBigdata2[0].Text.Trim();
                            }
                            if (THBigdata2.Count != 0 && THBigdata2.Count == 1 && !row.Text.Contains("Owner") && !row.Text.Contains("Bill Year") && !row.Text.Contains("Parcel ID"))
                            {
                                Tableheading = THBigdata2[0].Text.Trim();
                            }

                            if (TDBigdata2.Count == 3 && !row.Text.Contains("Owner") && !row.Text.Contains("Bill Year") && !row.Text.Contains("Parcel ID"))
                            {
                                Taxablevalue = TDBigdata2[0].Text;
                                TaxRate = TDBigdata2[1].Text;
                                Amount = TDBigdata2[2].Text;

                                string paymentdetails3 = Owner.Trim() + "~" + Parcelid.Trim() + "~" + ChargesBillyear.Trim() + "~" + Tableheading.Trim() + "~" + Taxablevalue.Trim() + "~" + TaxRate.Trim() + "~" + Amount.Trim();

                                gc.insert_date(orderNumber, ParcelNumber, 1251, paymentdetails3, 1, DateTime.Now);
                            }
                            if (TDBigdata2.Count == 1 && !row.Text.Contains("Owner") && !row.Text.Contains("Bill Year") && !row.Text.Contains("Parcel ID"))
                            {
                                //Taxablevalue = TDBigdata2[0].Text;
                                //TaxRate = TDBigdata2[1].Text;
                                Amount = TDBigdata2[0].Text;

                                string paymentdetails3 = Owner.Trim() + "~" + Parcelid.Trim() + "~" + ChargesBillyear.Trim() + "~" + Tableheading.Trim() + "~" + "" + "~" + "" + "~" + Amount.Trim();

                                gc.insert_date(orderNumber, ParcelNumber, 1251, paymentdetails3, 1, DateTime.Now);
                            }
                        }
                        string ParcelID = "", AlternateParcel = "";
                        //Property Details(Click Property Details)
                        IWebElement IProperty = driver.FindElement(By.XPath("//*[@id='primarynav']/li[3]/ul/li[3]/a"));
                        IProperty.Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Property Details" + all_billyear, driver, "MD", "Frederick");

                        IWebElement Bigdata21 = driver.FindElement(By.Id("ParcelTable"));
                        IList<IWebElement> TRBigdata21 = Bigdata21.FindElements(By.TagName("tr"));
                        IList<IWebElement> THBigdata21 = Bigdata21.FindElements(By.TagName("th"));
                        IList<IWebElement> TDBigdata21;
                        foreach (IWebElement rows1 in TRBigdata21)
                        {
                            TDBigdata21 = rows1.FindElements(By.TagName("td"));
                            THBigdata21 = rows1.FindElements(By.TagName("th"));
                            if (TDBigdata21.Count != 0 && rows1.Text.Contains("Parcel ID") && !rows1.Text.Contains("Alternate"))
                            {
                                ParcelID = TDBigdata21[0].Text;
                                //string AlternateParcel = TDBigdata21[1].Text;
                            }
                            if (TDBigdata21.Count != 0 && rows1.Text.Contains("Alternate Parcel ID"))
                            {
                                //string ParceID = TDBigdata21[0].Text;
                                AlternateParcel = TDBigdata21[0].Text;
                                break;
                            }

                        }

                        //Assessment Details (Click Assessment Details)(Pending)

                        IWebElement IAssessment = driver.FindElement(By.XPath("//*[@id='primarynav']/li[3]/ul/li[5]/a"));
                        IAssessment.Click();
                        Thread.Sleep(9000);
                        //js.ExecuteScript("arguments[0].click();", IAssessment);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Assessment" + all_billyear, driver, "MD", "Frederick");


                        IWebElement bulkavd = driver.FindElement(By.XPath("//*[@id='molContentContainer']/div/table[2]/tbody"));
                        IList<IWebElement> TRbulkavd = bulkavd.FindElements(By.TagName("tr"));
                        IList<IWebElement> THbulkavd = bulkavd.FindElements(By.TagName("th"));
                        IList<IWebElement> TDbulkavd;
                        foreach (IWebElement txt in TRbulkavd)
                        {
                            TDbulkavd = txt.FindElements(By.TagName("td"));
                            THbulkavd = txt.FindElements(By.TagName("th"));
                            if (TDbulkavd.Count != 0 && !txt.Text.Contains("Gross Assessment"))
                            {
                                string Assessvaluedetails = all_billyear.Trim() + "~" + AlternateParcel.Trim() + "~" + THbulkavd[0].Text.Trim() + "~" + TDbulkavd[0].Text.Trim();

                                gc.insert_date(orderNumber, ParcelNumber, 1252, Assessvaluedetails, 1, DateTime.Now);
                            }

                        }
                        //Tax Assessment History Details
                        IWebElement IAssessmenthistory = driver.FindElement(By.XPath("//*[@id='primarynav']/li[3]/ul/li[6]/a"));
                        IAssessmenthistory.Click();
                        Thread.Sleep(5000);
                        //js.ExecuteScript("arguments[0].click();", IAssessment);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Assessment History", driver, "MD", "Frederick");


                        IWebElement bulkahd = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_AssessmentHistoryGrid']/tbody"));
                        IList<IWebElement> TRbulkahd = bulkahd.FindElements(By.TagName("tr"));
                        IList<IWebElement> THbulkahd = bulkahd.FindElements(By.TagName("th"));
                        IList<IWebElement> TDbulkahd;
                        foreach (IWebElement txt in TRbulkahd)
                        {
                            TDbulkahd = txt.FindElements(By.TagName("td"));
                            THbulkahd = txt.FindElements(By.TagName("th"));

                            if (TDbulkahd.Count != 0 && !txt.Text.Contains("Building Value") && !txt.Text.Contains("Found no assessment history information."))
                            {
                                string Assesshistorydetails = TDbulkahd[0].Text + "~" + TDbulkahd[1].Text + "~" + TDbulkahd[2].Text + "~" + TDbulkahd[3].Text + "~" + TDbulkahd[4].Text;

                                gc.insert_date(orderNumber, parcelid1, 1253, Assesshistorydetails, 1, DateTime.Now);
                            }

                        }
                    }
                    try
                    {
                        //Tax History Bill Details

                        try
                        {

                            IWebElement IAddressSearch1 = driver.FindElement(By.LinkText("All Bills"));
                            IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                            js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                            Thread.Sleep(9000);
                        }
                        catch { }
                        try
                        {

                            IWebElement IAddressSearch1 = driver.FindElement(By.XPath("//*[@id='submenuselected']"));
                            IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                            js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                            Thread.Sleep(9000);
                        }
                        catch { }
                        try
                        {

                            IWebElement IAddressSearch1 = driver.FindElement(By.Id("submenuselected"));
                            IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                            js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                            Thread.Sleep(9000);
                        }
                        catch { }
                        try
                        {
                            IWebElement ISpan13 = driver.FindElement(By.Id("submenuselected"));
                            ISpan13.Click();
                            Thread.Sleep(9000);
                        }
                        catch { }
                        try
                        {
                            IWebElement ISpan13 = driver.FindElement(By.XPath("//*[@id='submenuselected']"));
                            ISpan13.Click();
                            Thread.Sleep(9000);
                        }
                        catch { }


                        gc.CreatePdf(orderNumber, parcelNumber, "Tax History Details All Bills1", driver, "MD", "Frederick");

                        try
                        {
                            ByVisibleElement(driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_BillsRepeater_ctl00_BillsGrid_ctl08_ViewBillLinkButton")));
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Utility Bill Details All Bills1", driver, "MD", "Frederick");
                            ByVisibleElement(driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_BillsRepeater_ctl01_BillsGrid']/tbody/tr[11]/td[3]")));
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Utility Bill Details All Bills2", driver, "MD", "Frederick");
                        }
                        catch { }

                        IWebElement Bigdata3 = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_BillsRepeater_ctl00_BillsGrid"));
                        IList<IWebElement> TRBigdata3 = Bigdata3.FindElements(By.TagName("tr"));
                        IList<IWebElement> THBigdata3 = Bigdata3.FindElements(By.TagName("th"));
                        IList<IWebElement> TDBigdata3;
                        foreach (IWebElement row2 in TRBigdata3)
                        {
                            TDBigdata3 = row2.FindElements(By.TagName("td"));
                            THBigdata3 = row2.FindElements(By.TagName("th"));
                            if (TDBigdata3.Count != 0 && TDBigdata3.Count == 6 && row2.Text.Contains("Bill") && !row2.Text.Contains("Total"))
                            {
                                string TaxHistoryBillDetails = TDBigdata3[0].Text + "~" + TDBigdata3[1].Text + "~" + TDBigdata3[2].Text + "~" + TDBigdata3[3].Text + "~" + TDBigdata3[4].Text;

                                gc.insert_date(orderNumber, ParcelNumber, 1254, TaxHistoryBillDetails, 1, DateTime.Now);
                            }

                        }


                        //Utility Bill Details
                        //gc.CreatePdf(orderNumber, ParcelNumber, "Utility Bill Details All Bills", driver, "MD", "Frederick");
                        //try
                        //{
                        //    ByVisibleElement(driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_BillsRepeater_ctl00_BillsGrid_ctl18_ViewBillLinkButton")));
                        //    Thread.Sleep(2000);
                        //    gc.CreatePdf(orderNumber, parcelNumber, "Utility Bill Details All Bills3", driver, "MD", "Frederick");
                        //    ByVisibleElement(driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_BillsRepeater_ctl01_BillsGrid']/tbody/tr[15]/td[5]")));
                        //    Thread.Sleep(2000);
                        //    gc.CreatePdf(orderNumber, parcelNumber, "Utility Bill Details All Bills4", driver, "MD", "Frederick");
                        //}
                        //catch { }
                        IWebElement Bigdata4 = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_BillsRepeater_ctl01_BillsGrid"));
                        IList<IWebElement> TRBigdata4 = Bigdata4.FindElements(By.TagName("tr"));
                        IList<IWebElement> THBigdata4 = Bigdata4.FindElements(By.TagName("th"));
                        IList<IWebElement> TDBigdata4;
                        foreach (IWebElement row1 in TRBigdata4)
                        {
                            TDBigdata4 = row1.FindElements(By.TagName("td"));
                            THBigdata4 = row1.FindElements(By.TagName("th"));
                            if (TDBigdata4.Count != 0 && !row1.Text.Contains("Bill") && !row1.Text.Contains("Total"))
                            {
                                string UtilityBillDetails = TDBigdata4[0].Text + "~" + TDBigdata4[1].Text + "~" + TDBigdata4[2].Text + "~" + TDBigdata4[3].Text + "~" + TDBigdata4[4].Text;

                                gc.insert_date(orderNumber, ParcelNumber, 1255, UtilityBillDetails, 1, DateTime.Now);
                            }

                        }
                    }
                    catch { }


                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");

                    gc.insert_TakenTime(orderNumber, "MD", "Frederick", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);


                    driver.Quit();
                    gc.mergpdf(orderNumber, "MD", "Frederick");
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
        public void ByVisibleElement(IWebElement Element)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].scrollIntoView();", Element);
        }
    }
}


