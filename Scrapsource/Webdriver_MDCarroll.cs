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
    public class Webdriver_MDCarroll
    {

        IWebDriver driver;
        DBconnection db = new DBconnection();

        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        public string FTP_Carroll(string houseno, string sname, string direction, string account, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string Address = houseno + " " + direction + " " + sname;
            if (Address.Trim() != "" && !Address.Trim().Contains("UNIT") && !Address.Trim().Contains("APT") && !Address.Trim().Contains("#"))
            { searchType = "address"; }
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
                        string titleaddress = houseno + " " + sname + " " + direction + " " + directParcel;
                        gc.TitleFlexSearch(orderNumber, parcelNumber, ownername, titleaddress, "MD", "Carroll");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_MDCaroll"] = "Yes";
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
                        selectElement1.SelectByText("CARROLL COUNTY");

                        var SerachCategoryM = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucSearchType_ddlSearchType']"));
                        var selectElementM1 = new SelectElement(SerachCategoryM);
                        selectElementM1.SelectByText("STREET ADDRESS");
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "MD", "Carroll");
                        driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_StartNavigationTemplateContainerID_btnContinue']")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);

                        driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtStreenNumber']")).SendKeys(houseno);
                        driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtStreetName']")).SendKeys(sname);
                        driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_StepNavigationTemplateContainerID_btnStepNextButton']")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Address search Result", driver, "MD", "Carroll");
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
                                    HttpContext.Current.Session["MDCarroll_Count"] = "Maimum";
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
                                    gc.insert_date(orderNumber, Accnumber, 536, Multi, 1, DateTime.Now);
                                }
                            }

                            HttpContext.Current.Session["MDCarroll"] = "Yes";
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
                        selectElement1.SelectByText("CARROLL COUNTY");
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
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "MD", "Carroll");
                        driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_StepNavigationTemplateContainerID_btnStepNextButton']")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_lblErr"));
                        if (INodata.Text.Contains("no records that match your criteria"))
                        {
                            HttpContext.Current.Session["Nodata_MDCaroll"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    string LegalDescription = "", Parcel = "", Use = "", PrincipalResidence = "", Map = "", Grid = "", SubDistrict = "", Subdivision = "", Section = "", Block = "", Lot = "", AssessmentYear = "", Town = "";
                    string HomesteadApplicationStatus = "", HomeownersTaxCreditApplicationStatus = "", HomeownersTaxCreditApplicationDate = "", District = "";
                    parcelNumber = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblDetailsStreetHeader_0']")).Text.Replace("Folio:", "");
                    District = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblDetailsStreetHeader_0']")).Text.Replace("Folio:", "");
                    parcelNumber = GlobalClass.After(parcelNumber, "Account Number - ");
                    District = gc.Between(District, "District -", "Account Number -");

                    gc.CreatePdf(orderNumber, parcelNumber, "Assessment information", driver, "MD", "Carroll");

                    ownername = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblOwnerName_0']")).Text;
                    PropertyAddress = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblPremisesAddress_0']")).Text.Replace("\r\n", "");
                    LegalDescription = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblLegalDescription_0']")).Text.Replace("\r\n", "");
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
                    gc.insert_date(orderNumber, parcelNumber, 528, Property, 1, DateTime.Now);
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
                                    gc.insert_date(orderNumber, parcelNumber, 529, split[0] + "~" + split1[0], 1, DateTime.Now);
                                    gc.insert_date(orderNumber, parcelNumber, 529, split[1] + "~" + split1[1], 1, DateTime.Now);
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

                                    gc.insert_date(orderNumber, parcelNumber, 529, split[0] + "~" + split1[0], 1, DateTime.Now);
                                    gc.insert_date(orderNumber, parcelNumber, 529, split[1] + "~" + split1[1], 1, DateTime.Now);

                                }
                            }

                        }
                    }

                    catch
                    {

                    }

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("http://ccgovernment.carr.org/ccg/BillingInquiry/");
                    Thread.Sleep(2000);


                    var Billtype = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_ddlBillSelect']"));
                    var Bill = new SelectElement(Billtype);
                    Bill.SelectByText("Real Estate Tax");

                    driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_txtDistrict']")).SendKeys(District.Trim());

                    driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_txtAccount']")).SendKeys(parcelNumber);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax  Site", driver, "MD", "Carroll");
                    driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_btnSubmit']")).SendKeys(Keys.Enter);

                    Thread.Sleep(1000);

                    string BillID = "", BillDate = "", Type = "", Status = "";
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax  Info", driver, "MD", "Carroll");

                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_txtDistrict']")).SendKeys(District.Trim());

                        var Sub = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_ddlSubDistrict']"));
                        var Sub1 = new SelectElement(Sub);
                        Sub1.SelectByText("1");

                        driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_txtAccount']")).SendKeys(parcelNumber);

                        driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_btnSubmit']")).SendKeys(Keys.Enter);
                    }
                    catch { }

                    try
                    {
                        IWebElement TaxDisTB = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_pnlResults']/table/tbody"));
                        IList<IWebElement> TaxDisTR = TaxDisTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> TaxDisTD;

                        foreach (IWebElement row1 in TaxDisTR)
                        {

                            TaxDisTD = row1.FindElements(By.TagName("td"));
                            if (TaxDisTD.Count != 0 && TaxDisTD.Count != 1)
                            {

                                BillID = TaxDisTD[0].Text;
                                BillDate = TaxDisTD[1].Text;
                                Type = TaxDisTD[2].Text;
                                Status = TaxDisTD[3].Text;

                                string taxHitory = BillID + "~" + BillDate + "~" + Type + "~" + Status;
                                gc.insert_date(orderNumber, parcelNumber, 530, taxHitory, 1, DateTime.Now);
                            }

                        }
                    }
                    catch
                    {

                    }

                    string AccountNumber = "", PayOffAmount = "", GoodThroughDate = "", PayOffAmount1 = "", GoodThroughDate1 = "", Period = "", CountyAssessment = "", StateAssessment = "", Exemptions = "", GrossAmount = "", fstHalfPaidDate = "", fstHalfPaidAmount = "", sHalfPaidDate = "", sHalfPaidAmount = "", status = "", PrincipalResidenceTax = "", TaxAuthority = "";
                    int F = 0;
                    int G = 0;
                    try
                    {

                        // TaxDisTD = row1.FindElements(By.TagName("a"));
                        IWebElement TaxDisTB = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_pnlResults']/table/tbody"));

                        IList<IWebElement> TaxDisTR = TaxDisTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> TaxDisTd;

                        foreach (IWebElement Row in TaxDisTR)
                        {

                            TaxDisTR = Row.FindElements(By.TagName("a"));
                            TaxDisTd = Row.FindElements(By.TagName("td"));
                            if (TaxDisTd.Count != 0 && TaxDisTd[3].Text.Trim() == "Paid in Full")
                            {

                                if (TaxDisTR.Count != 0)
                                {


                                    string UID = TaxDisTR[0].GetAttribute("id");
                                    listurl.Add(UID);


                                }
                            }
                            else if (TaxDisTd.Count != 0 && TaxDisTd[3].Text.Trim() == "No Payment Activity")
                            {
                                if (TaxDisTR.Count != 0)
                                {


                                    string AID = TaxDisTR[0].GetAttribute("id");
                                    listurl1.Add(AID);


                                }
                            }


                        }





                        foreach (string ID in listurl)
                        {
                            IWebElement ISpan1 = driver.FindElement(By.Id(ID));
                            IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                            js1.ExecuteScript("arguments[0].click();", ISpan1);
                            Thread.Sleep(1000);
                            AccountNumber = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblAcctNo']")).Text;
                            BillID = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblBillIDNo']")).Text;
                            gc.CreatePdf(orderNumber, parcelNumber, "Tax Bill" + BillID, driver, "MD", "Carroll");
                            ownername = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblOwner']")).Text.Replace("\r\n", " ").Trim();
                            LegalDescription = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblPropDesc']")).Text.Replace("\r\n", " ").Trim();
                            Period = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblTaxPeriod']")).Text;
                            CountyAssessment = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblCounty']")).Text;
                            StateAssessment = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblState']")).Text;
                            PrincipalResidenceTax = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblResidence']")).Text;
                            Exemptions = "";
                            try
                            {
                                TaxAuthority = driver.FindElement(By.XPath("//*[@id='indent_sidemenu']/div[5]/div")).Text.Replace("\r\n", " ");

                            }
                            catch { }
                            GrossAmount = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblGrossAmt']")).Text;
                            fstHalfPaidDate = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblFirstPayDate']")).Text;
                            fstHalfPaidAmount = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblFirstPayAmt']")).Text;
                            try
                            {
                                sHalfPaidDate = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblSecondPayDate']")).Text;
                                sHalfPaidAmount = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblSecondPayAmt']")).Text;
                            }
                            catch { }
                            status = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblPayStatus']")).Text;
                            string TaxInfo = BillID + "~" + ownername + "~" + LegalDescription + "~" + Period + "~" + CountyAssessment + "~" + StateAssessment + "~" + PrincipalResidenceTax + "~" + Exemptions + "~" + GrossAmount + "~" + fstHalfPaidDate + "~" + fstHalfPaidAmount + "~" + sHalfPaidDate + "~" + sHalfPaidAmount + "~" + status + "~" + PayOffAmount + "~" + GoodThroughDate + "~" + PayOffAmount1 + "~" + GoodThroughDate1 + "~" + "-";
                            gc.insert_date(orderNumber, AccountNumber, 531, TaxInfo, 1, DateTime.Now);
                            driver.Navigate().Back();
                            BillID = ""; ownername = ""; LegalDescription = ""; Period = ""; CountyAssessment = ""; StateAssessment = ""; Exemptions = ""; GrossAmount = ""; fstHalfPaidDate = ""; fstHalfPaidAmount = ""; sHalfPaidDate = ""; sHalfPaidAmount = ""; status = ""; PayOffAmount = ""; GoodThroughDate = ""; PayOffAmount1 = ""; GoodThroughDate1 = ""; PrincipalResidenceTax = "";
                        }
                        foreach (string ID in listurl1)
                        {
                            IWebElement ISpan1 = driver.FindElement(By.Id(ID));
                            IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                            js1.ExecuteScript("arguments[0].click();", ISpan1);
                            Thread.Sleep(1000);
                            AccountNumber = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblAcctNo']")).Text;
                            BillID = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblBillIDNo']")).Text;
                            gc.CreatePdf(orderNumber, parcelNumber, "Tax Bill" + BillID, driver, "MD", "Carroll");
                            ownername = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblOwner']")).Text.Replace("\r\n", " ").Trim();
                            LegalDescription = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblPropDesc']")).Text.Replace("\r\n", " ").Trim();
                            Period = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblTaxPeriod']")).Text;
                            CountyAssessment = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblCounty']")).Text;
                            StateAssessment = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblState']")).Text;
                            PrincipalResidenceTax = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblResidence']")).Text;
                            Exemptions = "";
                            try
                            {
                                TaxAuthority = driver.FindElement(By.XPath("//*[@id='indent_sidemenu']/div[5]/div")).Text.Replace("\r\n", " ");

                            }
                            catch { }
                            GrossAmount = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblGrossAmt']")).Text;
                            fstHalfPaidDate = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblFirstPayDate']")).Text;
                            fstHalfPaidAmount = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblFirstPayAmt']")).Text;
                            //sHalfPaidDate = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblSecondPayDate']")).Text;
                            //sHalfPaidAmount = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblSecondPayAmt']")).Text;
                            PayOffAmount = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblFullPay1']")).Text;
                            PayOffAmount1 = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblFullPay2']")).Text;
                            GoodThroughDate = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblFullPayCurDate']")).Text;
                            GoodThroughDate1 = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblFullPayNextDate']")).Text;
                            status = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblPayStatus']")).Text;
                            gc.CreatePdf(orderNumber, parcelNumber, "TaxS", driver, "MD", "Carroll");

                            string TaxInfo = BillID + "~" + ownername + "~" + LegalDescription + "~" + Period + "~" + CountyAssessment + "~" + StateAssessment + "~" + PrincipalResidenceTax + "~" + Exemptions + "~" + GrossAmount + "~" + fstHalfPaidDate + "~" + fstHalfPaidAmount + "~" + sHalfPaidDate + "~" + sHalfPaidAmount + "~" + status + "~" + PayOffAmount + "~" + GoodThroughDate + "~" + PayOffAmount1 + "~" + GoodThroughDate1 + "~" + "-";
                            gc.insert_date(orderNumber, AccountNumber, 531, TaxInfo, 1, DateTime.Now);
                            //Payment History for No Payment
                            try
                            {
                                string payment = driver.FindElement(By.Id("ContentPlaceHolder1_lnbPayTable")).Text;
                                string paymentstatus = driver.FindElement(By.Id("ContentPlaceHolder1_lblPayStatus")).Text;
                                if (paymentstatus.Contains("No Payment Activity") || payment.Contains("Click Here for Complete Payment Schedule Information"))
                                {
                                    IWebElement IPaymentHistory = driver.FindElement(By.Id("ContentPlaceHolder1_lnbPayTable"));
                                    IPaymentHistory.Click();
                                    Thread.Sleep(2000);
                                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Payment History " + BillID, driver, "MD", "Carroll");
                                    driver.Navigate().Back();
                                    Thread.Sleep(2000);
                                }
                            }
                            catch { }

                            driver.Navigate().Back();

                            BillID = ""; ownername = ""; LegalDescription = ""; Period = ""; CountyAssessment = ""; StateAssessment = ""; Exemptions = ""; GrossAmount = ""; fstHalfPaidDate = ""; fstHalfPaidAmount = ""; sHalfPaidDate = ""; sHalfPaidAmount = ""; status = ""; PayOffAmount = ""; GoodThroughDate = ""; PayOffAmount1 = ""; GoodThroughDate1 = ""; PrincipalResidenceTax = "";
                        }
                    }
                    catch { }
                    try
                    {
                        string TaxInfo = "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + TaxAuthority;
                        gc.insert_date(orderNumber, AccountNumber, 531, TaxInfo, 1, DateTime.Now);
                    }
                    catch { }
                    //try
                    //{
                    //    F = 0;
                    //    driver.Navigate().Back();
                    //    Thread.Sleep(2000);
                    //    int K = 0;
                    //    TaxDisTB = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_pnlResults']/table/tbody"));
                    //    TaxDisTR = TaxDisTB.FindElements(By.TagName("tr"));


                    //    foreach (IWebElement row1 in TaxDisTR)
                    //    {

                    //        TaxDisTD = row1.FindElements(By.TagName("td"));
                    //        if (TaxDisTD.Count != 0 && TaxDisTD.Count != 1)
                    //        {

                    //            Status = TaxDisTD[3].Text;
                    //            K++;
                    //            if (G!=1)
                    //            {
                    //                if (Status == "No Payment Activity")
                    //                {
                    //                    TaxDisTB = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_pnlResults']/table/tbody"));

                    //                    TaxDisTR = TaxDisTB.FindElements(By.TagName("a"));
                    //                    foreach (IWebElement Row in TaxDisTR)
                    //                    {
                    //                        F++;
                    //                        // TaxDisTD = row1.FindElements(By.TagName("a"));
                    //                        if (F == K)
                    //                        {
                    //                            if (TaxDisTR.Count != 0)
                    //                            {

                    //                                Row.Click();
                    //                                BillID = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblBillIDNo']")).Text;
                    //                                ownername = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblOwner']")).Text;
                    //                                LegalDescription = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblPropDesc']")).Text;
                    //                                Period = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblTaxPeriod']")).Text;
                    //                                CountyAssessment = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblCounty']")).Text;
                    //                                StateAssessment = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblState']")).Text;
                    //                                Exemptions = "";
                    //                                GrossAmount = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblGrossAmt']")).Text;
                    //                                fstHalfPaidDate = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblFirstPayDate']")).Text;
                    //                                fstHalfPaidAmount = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblFirstPayAmt']")).Text;
                    //                                //sHalfPaidDate = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblSecondPayDate']")).Text;
                    //                                //sHalfPaidAmount = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblSecondPayAmt']")).Text;
                    //                                status = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblPayStatus']")).Text;
                    //                                gc.CreatePdf(orderNumber, parcelNumber, "TaxS", driver, "MD", "Carroll");

                    //                                string TaxInfo = BillID + "~" + ownername + "~" + LegalDescription + "~" + Period + "~" + CountyAssessment + "~" + StateAssessment + "~" + Exemptions + "~" + GrossAmount + "~" + fstHalfPaidDate + "~" + fstHalfPaidAmount + "~" + sHalfPaidDate + "~" + sHalfPaidAmount + "~" + Status + "~" + PayOffAmount + "~" + GoodThroughDate + "~" + PayOffAmount1 + "~" + GoodThroughDate1;
                    //                                gc.insert_date(orderNumber, parcelNumber, 531, TaxInfo, 1, DateTime.Now);
                    //                            }


                    //                        }


                    //                    }


                    //                }
                    //                else if (Status != "No Payment Activity")
                    //                {
                    //                    TaxDisTB = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_pnlResults']/table/tbody"));

                    //                    TaxDisTR = TaxDisTB.FindElements(By.TagName("a"));
                    //                    foreach (IWebElement Row in TaxDisTR)
                    //                    {
                    //                        F++;
                    //                        // TaxDisTD = row1.FindElements(By.TagName("a"));
                    //                        if (F == 2)
                    //                        {
                    //                            if (TaxDisTR.Count != 0)
                    //                            {

                    //                                Row.Click();
                    //                                BillID = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblBillIDNo']")).Text;
                    //                                ownername = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblOwner']")).Text;
                    //                                LegalDescription = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblPropDesc']")).Text;
                    //                                Period = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblTaxPeriod']")).Text;
                    //                                CountyAssessment = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblCounty']")).Text;
                    //                                StateAssessment = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblState']")).Text;
                    //                                Exemptions = "";
                    //                                GrossAmount = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblGrossAmt']")).Text;
                    //                                fstHalfPaidDate = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblFirstPayDate']")).Text;
                    //                                fstHalfPaidAmount = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblFirstPayAmt']")).Text;
                    //                                //sHalfPaidDate = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblSecondPayDate']")).Text;
                    //                                //sHalfPaidAmount = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblSecondPayAmt']")).Text;
                    //                                status = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblPayStatus']")).Text;
                    //                                gc.CreatePdf(orderNumber, parcelNumber, "TaxS", driver, "MD", "Carroll");

                    //                                string TaxInfo = BillID + "~" + ownername + "~" + LegalDescription + "~" + Period + "~" + CountyAssessment + "~" + StateAssessment + "~" + Exemptions + "~" + GrossAmount + "~" + fstHalfPaidDate + "~" + fstHalfPaidAmount + "~" + sHalfPaidDate + "~" + sHalfPaidAmount + "~" + Status + "~" + PayOffAmount + "~" + GoodThroughDate + "~" + PayOffAmount1 + "~" + GoodThroughDate1;
                    //                                gc.insert_date(orderNumber, parcelNumber, 531, TaxInfo, 1, DateTime.Now);
                    //                            }


                    //                        }


                    //                    }


                    //                }
                    //            }
                    //            G++;



                    //        }

                    //    }



                    //}
                    //catch { }
                    //try
                    //{
                    //    F = 0;  
                    //    driver.Navigate().Back();
                    //    Thread.Sleep(2000);
                    //    int K = 0;
                    //    TaxDisTB = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_pnlResults']/table/tbody"));
                    //    TaxDisTR = TaxDisTB.FindElements(By.TagName("tr"));


                    //    foreach (IWebElement row1 in TaxDisTR)
                    //    {

                    //        TaxDisTD = row1.FindElements(By.TagName("td"));
                    //        if (TaxDisTD.Count != 0 && TaxDisTD.Count != 1)
                    //        {

                    //            Status = TaxDisTD[3].Text;
                    //            K++;
                    //            if (Status== "No Payment Activity"&& TaxDisTD[2].Text.Contains("Water/Sewer") )
                    //            {
                    //                TaxDisTB = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_pnlResults']/table/tbody"));

                    //                TaxDisTR = TaxDisTB.FindElements(By.TagName("a"));
                    //                foreach (IWebElement Row in TaxDisTR)
                    //                {
                    //                    F++;
                    //                    // TaxDisTD = row1.FindElements(By.TagName("a"));
                    //                    if (F == K)
                    //                    {
                    //                        if (TaxDisTR.Count != 0)
                    //                        {

                    //                            Row.Click();
                    //                            BillID = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblBillIDNo']")).Text;
                    //                            ownername = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblOwner']")).Text;
                    //                            LegalDescription = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblPropDesc']")).Text;
                    //                            Period = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblTaxPeriod']")).Text;
                    //                            CountyAssessment = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblCounty']")).Text;
                    //                            StateAssessment = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblState']")).Text;
                    //                            Exemptions = "";
                    //                            GrossAmount = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblGrossAmt']")).Text;
                    //                            fstHalfPaidDate = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblFirstPayDate']")).Text;
                    //                            fstHalfPaidAmount = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblFirstPayAmt']")).Text;
                    //                            //sHalfPaidDate = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblSecondPayDate']")).Text;
                    //                            //sHalfPaidAmount = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblSecondPayAmt']")).Text;
                    //                            status = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblPayStatus']")).Text;

                    //                            gc.CreatePdf(orderNumber, parcelNumber, "TaxT", driver, "MD", "Carroll");
                    //                            string TaxInfo = BillID + "~" + ownername + "~" + LegalDescription + "~" + Period + "~" + CountyAssessment + "~" + StateAssessment + "~" + Exemptions + "~" + GrossAmount + "~" + fstHalfPaidDate + "~" + fstHalfPaidAmount + "~" + "" + "~" + "" + "~" + Status;
                    //                            gc.insert_date(orderNumber, parcelNumber, 531, TaxInfo, 1, DateTime.Now);
                    //                        }


                    //                    }


                    //                }


                    //            }




                    //        }

                    //    }




                    //}
                    //catch { }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");

                    gc.insert_TakenTime(orderNumber, "MD", "Carroll", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);


                    driver.Quit();
                    gc.mergpdf(orderNumber, "MD", "Carroll");
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