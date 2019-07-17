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
using System.Text;
using OpenQA.Selenium.Firefox;

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_HarfordMD
    {

        string outputPath = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();

        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_MDHarford(string houseno, string sname, string unitno, string parcelNumber, string searchType, string orderNumber, string ownername, string directParcel)
        {
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "", AssessTakenTime = "", TaxTakentime = "", CityTaxtakentime = "";
            string TotaltakenTime = "";
            string total_due = "", future_due = "";
            string balancedue_prior = "";
            string Account_Number1 = "", Balance = "", Bill_Date = "", Penalty_Date = "", TurnOffDate = "", PresentReadingDate = "", PreviousReadingDate = "", LastPaymentDate = "", LastPaymentAmount = "", current_due = "";
            string Account_id_number = "", Owner_name = "", Address = "", Legal_Description = "", Year_Built = "", use = "", Principal_Residence = "", Map = "", Sections = "", Land = "", Building = "", Total_Assessed_Value = "";
            string Grid = "", Parcel = "", Sub_District = "", Subdivision = "", Block = "", Lot = "", Assessment_Year = "", Homestead_Application_Status = "", Homeowners_Tax_Credit_Application_Status = "", Homeowners_Tax_Credit_Application_Date = "";

            string Tax_Amount = "", Discount_Amount = "", Interest = "", Total_Paid = "", Total_Due = "", Paid_Date = "", Paid_Amount = "";
            string Tax_Year = "", Homeowner_Occupied = "", Account_Number = "";
            string Account_id_number_city = "";
            List<string> strTaxRealestate = new List<string>();
            List<string> strTaxRealestate1 = new List<string>();
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            //IWebElement iframeElement1;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("https://sdat.dat.maryland.gov/RealProperty/Pages/default.aspx");
                    Thread.Sleep(7000);
                    var Select = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucSearchType_ddlCounty"));
                    var selectElement1 = new SelectElement(Select);
                    selectElement1.SelectByText("HARFORD COUNTY");
                    if (searchType == "address")
                    {
                        var Select1 = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucSearchType_ddlSearchType"));
                        var selectElement11 = new SelectElement(Select1);
                        selectElement11.SelectByText("STREET ADDRESS");
                        gc.CreatePdf_WOP(orderNumber, "Address", driver, "MD", "Harford");

                        driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_StartNavigationTemplateContainerID_btnContinue")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtStreenNumber")).SendKeys(houseno);
                        driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtStreetName")).SendKeys(sname);
                        gc.CreatePdf_WOP(orderNumber, "Address1", driver, "MD", "Harford");
                        driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_StepNavigationTemplateContainerID_btnStepNextButton")).SendKeys(Keys.Enter);

                        Thread.Sleep(7000);
                        try
                        {
                            if (driver.FindElement(By.XPath(" //*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucSearchResult_gv_SearchResult']/tbody")).Displayed)
                            {

                                gc.CreatePdf_WOP(orderNumber, "Address2", driver, "MD", "Harford");
                                IWebElement multitableElement11 = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucSearchResult_gv_SearchResult']/tbody"));
                                IList<IWebElement> multitableRow11 = multitableElement11.FindElements(By.TagName("tr"));
                                IList<IWebElement> multirowTD11;
                                int maxCheck = 0;
                                foreach (IWebElement row in multitableRow11)
                                {
                                    if (maxCheck <= 25)
                                    {
                                        if (!row.Text.Contains("Name"))
                                        {
                                            multirowTD11 = row.FindElements(By.TagName("td"));
                                            if (multirowTD11.Count != 1 && multirowTD11[0].Text.Trim() != "")
                                            {
                                                string p_no = multirowTD11[1].Text.Trim();
                                                p_no = p_no.Replace(" ", "");
                                                string multi_parcel = multirowTD11[0].Text.Trim() + "~" + multirowTD11[1].Text.Trim() + "~" + multirowTD11[2].Text.Trim() + "~" + multirowTD11[3].Text.Trim() + "~" + multirowTD11[4].Text.Trim() + "~" + multirowTD11[5].Text.Trim();
                                                gc.insert_date(orderNumber, p_no, 422, multi_parcel, 1, DateTime.Now);
                                                //             Name~Account~Street~Own_Occ~Map~Parcel
                                            }
                                        }
                                        maxCheck++;
                                    }
                                }

                                if (multitableRow11.Count > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_MDHarford_Multicount"] = "Maximum";
                                }
                                else
                                {
                                    HttpContext.Current.Session["multiparcel_MDHarford"] = "Yes";
                                }
                                driver.Quit();
                                // gc.mergpdf(orderNumber);
                                return "MultiParcel";
                            }
                        }
                        catch { }
                    }

                    if (searchType == "titleflex")
                    {
                        string titleaddress = houseno + " " + sname + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, "", ownername, titleaddress, "MD", "Harford");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_HarfordMD"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        searchType = "parcel";
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();

                    }


                    if (searchType == "parcel")
                    {

                        var Select11 = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucSearchType_ddlSearchType"));
                        var selectElement111 = new SelectElement(Select11);
                        selectElement111.SelectByText("PROPERTY ACCOUNT IDENTIFIER");
                        gc.CreatePdf_WOP(orderNumber, "Parcel", driver, "MD", "Harford");
                        driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_StartNavigationTemplateContainerID_btnContinue")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        if (parcelNumber.Contains("-"))
                        {
                            parcelNumber = parcelNumber.Replace("-", "");
                        }

                        string district = parcelNumber.Substring(0, 2);
                        driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtDistrict")).SendKeys(district);
                        string accountId = parcelNumber.Substring(2, 6);
                        driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtAccountIdentifier")).SendKeys(accountId);


                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search ", driver, "MD", "Harford");
                        driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_StepNavigationTemplateContainerID_btnStepNextButton")).SendKeys(Keys.Enter);
                        Thread.Sleep(5000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Assessment search ", driver, "MD", "Harford");
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_lblErr"));
                        if(INodata.Text.Contains("There are no records that match your criteria for county"))
                        {
                            HttpContext.Current.Session["Nodata_HarfordMD"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    //property_details

                    // string  Account_id_number_city = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblDetailsStreetHeader_0")).Text.Trim();
                    Thread.Sleep(5000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Assessment search ", driver, "MD", "Harford");
                    Account_id_number = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblDetailsStreetHeader_0")).Text.Trim();
                    string value = Account_id_number.Replace("District", "").Replace("Account Number", "");
                    Account_id_number = value.Replace(" ", "").Replace("-", "");

                    Owner_name = driver.FindElement(By.XPath("//*[@id='detailSearch']/tbody/tr[5]/td[2]")).Text.Trim();
                    Address = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblPremisesAddress_0")).Text.Trim();
                    Address = Address.Replace("\r\n", " ");
                    string Mail_Address = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblMailingAddress_0")).Text.Trim();
                    Mail_Address = Mail_Address.Replace("\r\n", " ");
                    Legal_Description = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblLegalDescription_0")).Text.Trim();
                    Legal_Description = Legal_Description.Replace("\r\n", " ");
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

                    string property_details = Account_id_number + "~" + Owner_name + "~" + Address + "~" + Mail_Address + "~" + Legal_Description + "~" + Year_Built + "~" + use + "~" + Principal_Residence + "~" + Map + "~" + Grid + "~" + Parcel + "~" + Sub_District + "~" + Subdivision + "~" + Sections + "~" + Block + "~" + Lot + "~" + Assessment_Year + "~" + Homestead_Application_Status + "~" + Homeowners_Tax_Credit_Application_Status + "~" + Homeowners_Tax_Credit_Application_Date;
                    gc.insert_date(orderNumber, Account_id_number, 413, property_details, 1, DateTime.Now);
                    //         Account id number~Owner name~Address~Mail Address~Legal Description~Year Built~Use~Principal Residence~Map~Grid~Parcel~Sub District~Sub division~Sections~Block~Lot~Assessment Year~Homestead Application Status~Homeowners Tax Credit Application Status~Homeowners Tax Credit Application Date
                    //Assessment Details Table:

                    string date1 = "", date2 = "", Total1 = "", Total2 = "";
                    date1 = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblPhaseDate_0")).Text.Trim().Replace("As of", "").Replace("\r\n", "");
                    date2 = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblAssesDate_0")).Text.Trim().Replace("As of", "").Replace("\r\n", "");
                    Total1 = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblPhaseInTotal_0")).Text.Trim();
                    Total2 = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblAssesTotal_0")).Text.Trim();
                    string Assessment_details1 = date1 + "~" + Total1;
                    gc.insert_date(orderNumber, Account_id_number, 414, Assessment_details1, 1, DateTime.Now);
                    string Assessment_details2 = date2 + "~" + Total2;
                    gc.insert_date(orderNumber, Account_id_number, 414, Assessment_details2, 1, DateTime.Now);
                    //   Year~Total Assessed Value
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("http://hcgweb01.harfordcountymd.gov/BillPay/SearchPropertyBill.aspx");
                    Thread.Sleep(5000);

                    driver.FindElement(By.Id("searchProperty")).SendKeys(Account_id_number);
                    gc.CreatePdf(orderNumber, Account_id_number, "Tax_info", driver, "MD", "Harford");
                    driver.FindElement(By.Id("searchButton")).SendKeys(Keys.Enter);
                    Thread.Sleep(4000);
                    gc.CreatePdf(orderNumber, Account_id_number, "Tax_info details", driver, "MD", "Harford");
                    IWebElement tbmulti = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_grdTaxCurr_DXMainTable']/tbody"));
                    IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDmulti;
                    foreach (IWebElement row in TRmulti)
                    {

                        if (!row.Text.Contains("Account No."))
                        {
                            TDmulti = row.FindElements(By.TagName("td"));
                            if (TDmulti.Count == 7)
                            {
                                string yearcurrent = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_grdTaxCurr_DXTitle']/tbody/tr/td")).Text.Trim().Replace("PROPERTY BILLS", "");
                                string Tax_history1 = yearcurrent + "~" + TDmulti[0].Text + "~" + TDmulti[1].Text + "~" + TDmulti[2].Text;
                                gc.insert_date(orderNumber, Account_id_number, 415, Tax_history1, 1, DateTime.Now);
                            }
                        }
                    }

                    string priorcurrent = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_grdTaxesPrev_DXTitle']/tbody/tr/td")).Text.Trim().Replace("PROPERTY BILLS", "");

                    IWebElement tbmulti1 = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_grdTaxesPrev_DXMainTable']/tbody"));
                    IList<IWebElement> TRmulti1 = tbmulti1.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDmulti1;
                    foreach (IWebElement row in TRmulti1)
                    {
                        if (!row.Text.Contains("Account No."))
                        {
                            TDmulti1 = row.FindElements(By.TagName("td"));
                            if (TDmulti1.Count == 7)
                            {
                                string Tax_history2 = priorcurrent + "~" + TDmulti1[0].Text + "~" + TDmulti1[1].Text + "~" + TDmulti1[2].Text;
                                gc.insert_date(orderNumber, Account_id_number, 415, Tax_history2, 1, DateTime.Now);
                            }
                        }
                    }

                    //    Year~Account Number~Description~Balance Due

                    //  2018 PROPERTY BILLS
                    IWebElement ITaxReal = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_grdTaxCurr_DXMainTable']/tbody"));
                    IList<IWebElement> ITaxRealRow = ITaxReal.FindElements(By.TagName("tr"));
                    IList<IWebElement> ITaxRealTd;

                    foreach (IWebElement ItaxReal in ITaxRealRow)
                    {


                        ITaxRealTd = ItaxReal.FindElements(By.TagName("td"));
                        if (ItaxReal.Text.Contains("View Details"))
                        {

                            //    string ac = ITaxRealTd[0].Text;
                            IWebElement ITaxBillCount = ITaxRealTd[3].FindElement(By.TagName("a"));
                            string strTaxReal = ITaxBillCount.GetAttribute("id");
                            strTaxRealestate.Add(strTaxReal);
                        }
                    }

                    //      2017 PROPERTY BILLS

                    IWebElement ITaxReal1 = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_grdTaxesPrev_DXMainTable']/tbody"));
                    IList<IWebElement> ITaxRealRow1 = ITaxReal1.FindElements(By.TagName("tr"));
                    IList<IWebElement> ITaxRealTd1;

                    foreach (IWebElement ItaxReal1 in ITaxRealRow1)
                    {
                        ITaxRealTd1 = ItaxReal1.FindElements(By.TagName("td"));
                        if (ItaxReal1.Text.Contains("View Details"))
                        {
                            balancedue_prior = ITaxRealTd1[2].Text;
                            IWebElement ITaxBillCount = ITaxRealTd1[3].FindElement(By.TagName("a"));
                            string strTaxReal = ITaxBillCount.GetAttribute("id");
                            strTaxRealestate1.Add(strTaxReal);
                        }
                    }

                    string Assessed_Value = driver.FindElement(By.Id("ContentPlaceHolder1_lblCurrentAssessmentText")).Text.Trim();

                    foreach (string real in strTaxRealestate)
                    {
                        IWebElement element1 = driver.FindElement(By.Id(real));
                        element1.Click();

                        Thread.Sleep(4000);
                        try
                        {
                            string a1 = driver.FindElement(By.XPath("//*[@id='screendump']/fieldset/div[1]/div/div[2]")).Text.Trim();

                            try
                            {
                                gc.CreatePdf(orderNumber, Account_id_number, "Real Estate", driver, "MD", "Harford");
                                Tax_Year = driver.FindElement(By.Id("ContentPlaceHolder1_lblBillingPeriodText")).Text.Trim();
                                Tax_Year = WebDriverTest.Before(Tax_Year, "(");
                                Homeowner_Occupied = driver.FindElement(By.Id("ContentPlaceHolder1_lblHomeownerOccupiedText")).Text.Trim();

                                Account_Number = driver.FindElement(By.Id("ContentPlaceHolder1_lblAccountNumberText")).Text.Trim();

                            }
                            catch { }
                            //Taxinformation table
                            //  Account Number~Tax Year~Homeowner Occupied~Assessed Value~Tax Amount~Discount Amount~Interest~Total Paid~Current Due~Total Due~Future Due~Paid Date~Paid Amount

                            //Real Estate Tax Information

                            int i1 = 0;
                            string current_dueName = "", total_dueName = "";
                            IWebElement tbmulti2;

                            try
                            {
                                tbmulti2 = driver.FindElement(By.XPath("//*[@id = 'ctl00_ContentPlaceHolder1_grdAnnual_DXMainTable'] / tbody"));
                            }
                            catch
                            {
                                tbmulti2 = driver.FindElement(By.XPath("//*[@id = 'ctl00_ContentPlaceHolder1_grdTaxBill_DXMainTable'] / tbody"));
                            }
                            IList<IWebElement> TRmulti2 = tbmulti2.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDmulti2;
                            foreach (IWebElement row in TRmulti2)
                            {

                                TDmulti2 = row.FindElements(By.TagName("td"));
                                if (TDmulti2.Count != 0)
                                {
                                    if (i1 == 0)
                                        Tax_Amount = TDmulti2[1].Text;
                                    if (i1 == 1)
                                        Discount_Amount = TDmulti2[1].Text;
                                    if (i1 == 2)
                                        Interest = TDmulti2[1].Text;
                                    if (i1 == 3)
                                        Total_Paid = TDmulti2[1].Text;
                                    if (i1 == 4)
                                    {
                                        current_dueName = TDmulti2[0].Text;
                                        if (current_dueName.Contains("Current Discount"))
                                        {
                                            current_due = TDmulti2[1].Text;
                                        }
                                        else if (current_dueName.Contains("TOTAL DUE"))
                                        {
                                            total_due = TDmulti2[1].Text;
                                        }
                                    }
                                    if (i1 == 5)
                                        total_due = TDmulti2[1].Text;
                                    if (i1 == 6)
                                        future_due = TDmulti2[1].Text;


                                    i1++;
                                }


                            }



                            try
                            {
                                Paid_Date = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_grdPayment_DXDataRow0']/td[1]")).Text.Trim();
                                Paid_Amount = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_grdPayment_DXDataRow0']/td[2]")).Text.Trim();
                            }
                            catch
                            {
                                Paid_Date = "";
                                Paid_Amount = "";

                            }
                            //   
                            string installment = "";
                            //Account Number~Tax Year~Homeowner Occupied~Assessed Value~Tax Amount~Discount Amount~Interest~Total Paid~Current Due~Total Due~Future Due~Paid Date~Paid Amount
                            try
                            {
                                installment = driver.FindElement(By.XPath(" //*[@id='ContentPlaceHolder1_pnlMultiTab']/ul/li[2]/a")).Text.Trim();
                            }
                            catch
                            {
                                installment = " ";
                            }
                            //               Account Number~Tax Year~Homeowner Occupied~Assessed Value~Installment~Annual Tax Amount~1st Installment Tax Amount~2nd Installment Tax Amount~Discount Amount~Interest~Total Paid~Current Due~Total Due~Future Due~Full Year Balance~Paid Date~Paid Amount
                            string tax_info1 = Account_Number + "~" + Tax_Year + "~" + Homeowner_Occupied + "~" + Assessed_Value + "~" + installment + "~" + Tax_Amount + "~" + " " + "~" + " " + "~" + Discount_Amount + "~" + Interest + "~" + Total_Paid + "~" + current_due + "~" + total_due + "~" + future_due + "~" + " " + "~" + Paid_Date + "~" + Paid_Amount;
                            gc.insert_date(orderNumber, Account_id_number, 416, tax_info1, 1, DateTime.Now);
                            //    Tax Distribution Details Table
                            //    State Tax~County Tax~Highway Tax~TOTAL TAX~County Homestead Credit~State Homestead Credit~County Homeowners Credit~State Homeowners Credit~Enterprise Zone Credit~Agricultural Credit~Other Credits~TOTAL CREDITS~TOTAL TAX BILL
                            string StateTax = "", CountyTax = "", HighwayTax = "", TOTALTAX = "", CountyHomesteadCredit = "", StateHomesteadCredit = "", CountyHomeownersCredit = "", StateHomeownersCredit = "", EnterpriseZoneCredit = "", AgriculturalCredit = "", OtherCredits = "", TOTALCREDITS = "", TOTALTAXBILL = "";

                            string a = driver.FindElement(By.XPath("//*[@id='screendump']/fieldset/div[1]/div/div[2]/h3")).Text.Trim();
                            int i = 0;
                            IWebElement tbmulti11 = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_grdTaxes_DXMainTable']/tbody"));
                            IList<IWebElement> TRmulti11 = tbmulti11.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDmulti11;
                            foreach (IWebElement row in TRmulti11)
                            {

                                TDmulti11 = row.FindElements(By.TagName("td"));
                                if (TDmulti11.Count != 0)
                                {
                                    if (i == 0)
                                        StateTax = TDmulti11[1].Text;
                                    if (i == 1)
                                        CountyTax = TDmulti11[1].Text;
                                    if (i == 2)
                                        HighwayTax = TDmulti11[1].Text;
                                    if (i == 3)
                                        TOTALTAX = TDmulti11[1].Text;
                                    if (i == 4)
                                        CountyHomesteadCredit = TDmulti11[1].Text;
                                    if (i == 5)
                                        StateHomesteadCredit = TDmulti11[1].Text;
                                    if (i == 6)
                                        CountyHomeownersCredit = TDmulti11[1].Text;
                                    if (i == 7)
                                        StateHomeownersCredit = TDmulti11[1].Text;
                                    if (i == 8)
                                        EnterpriseZoneCredit = TDmulti11[1].Text;
                                    if (i == 9)
                                        AgriculturalCredit = TDmulti11[1].Text;
                                    if (i == 10)
                                        OtherCredits = TDmulti11[1].Text;
                                    if (i == 11)
                                        TOTALCREDITS = TDmulti11[1].Text;
                                    if (i == 12)
                                        TOTALTAXBILL = TDmulti11[1].Text;

                                    i++;
                                }


                            }
                            string Tax_distri = StateTax + "~" + CountyTax + "~" + HighwayTax + "~" + TOTALTAX + "~" + CountyHomesteadCredit + "~" + StateHomesteadCredit + "~" + CountyHomeownersCredit + "~" + StateHomeownersCredit + "~" + EnterpriseZoneCredit + "~" + AgriculturalCredit + "~" + OtherCredits + "~" + TOTALCREDITS + "~" + TOTALTAXBILL;
                            gc.insert_date(orderNumber, Account_id_number, 417, Tax_distri, 1, DateTime.Now);


                            string installment1 = "";
                            try
                            {
                                driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_pnlMultiTab']/ul/li[1]/a")).Click();
                                Thread.Sleep(2000);
                                gc.CreatePdf(orderNumber, Account_id_number, "Real EstateSemi", driver, "MD", "Harford");
                                installment1 = driver.FindElement(By.XPath(" //*[@id='ContentPlaceHolder1_pnlMultiTab']/ul/li[1]/a")).Text.Trim();
                            }
                            catch
                            {
                                installment1 = "";
                            }

                            int i11 = 0;
                            string firstInstall = "", secondInstal = "", full_year_balance = "";
                            try
                            {
                                IWebElement tbmulti21 = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_grdSemiTaxBill_DXMainTable']/tbody"));
                                IList<IWebElement> TRmulti21 = tbmulti21.FindElements(By.TagName("tr"));
                                IList<IWebElement> TDmulti21;
                                foreach (IWebElement row in TRmulti21)
                                {

                                    TDmulti21 = row.FindElements(By.TagName("td"));
                                    if (TDmulti21.Count != 0)
                                    {
                                        if (i11 == 0)
                                            firstInstall = TDmulti21[1].Text;
                                        if (i11 == 1)
                                            secondInstal = TDmulti21[1].Text;
                                        if (i11 == 2)
                                            Discount_Amount = TDmulti21[1].Text;
                                        if (i11 == 3)
                                            Interest = TDmulti21[1].Text;
                                        if (i11 == 4)
                                            Total_Paid = TDmulti21[1].Text;
                                        if (i11 == 5)
                                            current_due = TDmulti21[1].Text;
                                        if (i11 == 6)
                                            total_due = TDmulti21[1].Text;
                                        if (i11 == 7)
                                            future_due = TDmulti21[1].Text;
                                        if (i11 == 8)
                                            full_year_balance = TDmulti21[1].Text;

                                        i11++;
                                    }


                                }
                                try
                                {
                                    Paid_Date = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_grdPayment_DXDataRow0']/td[1]")).Text.Trim();
                                    Paid_Amount = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_grdPayment_DXDataRow0']/td[2]")).Text.Trim();
                                }
                                catch
                                {
                                    Paid_Date = "";
                                    Paid_Amount = "";

                                }

                                string tax_info = "" + "~" + "" + "~" + "" + "~" + "" + "~" + installment1 + "~" + "" + "~" + firstInstall + "~" + secondInstal + "~" + Discount_Amount + "~" + Interest + "~" + Total_Paid + "~" + current_due + "~" + total_due + "~" + future_due + "~" + full_year_balance + "~" + Paid_Date + "~" + Paid_Amount;
                                gc.insert_date(orderNumber, Account_id_number, 416, tax_info, 1, DateTime.Now);

                            }
                            catch { }








                        }
                        catch { }







                        try
                        {
                            //   USER BENEFIT ASSESSMENT and SCOTS FANCY ROAD IMPROVEMENT


                            //Year~Description~Account Number~Tax Amount~Discount~Interest~Total Payment~Paid Date~Paid Amount

                            //Special Assessments Table:
                            string a2 = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lgdProp']")).Text.Trim();

                            if (a2.Contains("USER BENEFIT"))
                            {
                                gc.CreatePdf(orderNumber, Account_id_number, "User benefit", driver, "MD", "Harford");

                            }

                            if (a2.Contains("SCOTS FANCY"))
                            {
                                gc.CreatePdf(orderNumber, Account_id_number, "SCOTS FANCY", driver, "MD", "Harford");

                            }

                            string description = driver.FindElement(By.XPath(" //*[@id='ContentPlaceHolder1_lgdProp']")).Text.Trim();
                            description = WebDriverTest.Before(description, " - ENDS:");
                            string account_noSpecial = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblAccountNumberText']")).Text.Trim();

                            string total_dueSpe = "", future_dueSpe = "", Total_PaidSpec = "", interestspec = "";
                            int i2 = 0;

                            try
                            {
                                IWebElement tbmulti22 = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_grdTaxBill_DXMainTable']/tbody"));
                                IList<IWebElement> TRmulti22 = tbmulti22.FindElements(By.TagName("tr"));
                                IList<IWebElement> TDmulti22;
                                foreach (IWebElement row in TRmulti22)
                                {//*[@id="ctl00_ContentPlaceHolder1_grdTaxBill_DXDataRow2"]/td[3]
                                 //*[@id="ctl00_ContentPlaceHolder1_grdTaxBill_DXDataRow2"]/td[2]
                                 //*[@id="ctl00_ContentPlaceHolder1_grdTaxBill_DXDataRow0"]/td[3]

                                    TDmulti22 = row.FindElements(By.TagName("td"));
                                    if (TDmulti22.Count != 0)
                                    {
                                        if (i2 == 0)
                                            Tax_Amount = TDmulti22[2].Text;
                                        if (i2 == 1)
                                            Discount_Amount = TDmulti22[2].Text;
                                        if (i2 == 2)
                                            interestspec = TDmulti22[4].Text;
                                        if (i2 == 5)
                                            Total_PaidSpec = TDmulti22[2].Text;
                                        if (i2 == 6)
                                            total_dueSpe = TDmulti22[2].Text;
                                        if (i2 == 7)
                                            future_dueSpe = TDmulti22[2].Text;

                                        i2++;
                                    }


                                }
                                try
                                {
                                    Paid_Date = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_grdPayment_DXDataRow0']/td[1]")).Text.Trim();
                                    Paid_Amount = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_grdPayment_DXDataRow0']/td[3]")).Text.Trim();

                                }
                                catch
                                {
                                    Paid_Date = "";
                                    Paid_Amount = "";
                                }
                                string special_assessmet = Tax_Year + "~" + description + "~" + account_noSpecial + "~" + Tax_Amount + "~" + Discount_Amount + "~" + interestspec + "~" + Total_PaidSpec + "~" + total_dueSpe + "~" + future_dueSpe + "~" + Paid_Date + "~" + Paid_Amount;
                                gc.insert_date(orderNumber, Account_id_number, 418, special_assessmet, 1, DateTime.Now);


                            }
                            catch { }

                        }
                        catch { }

                        try
                        {
                            //*[@id="screendump"]/div/fieldset/div[1]/div/div[2]/h3
                            //   Utility Payment History Table:
                            string a3 = driver.FindElement(By.XPath(" //*[@id='screendump']/div/fieldset/div[1]/div/div[2]/h3")).Text.Trim();
                            gc.CreatePdf(orderNumber, Account_id_number, "WATER SEWER USAGE", driver, "MD", "Harford");
                            IWebElement tbmulti11 = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_grdTaxBill_DXMainTable']/tbody"));
                            IList<IWebElement> TRmulti11 = tbmulti11.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDmulti11;
                            int i = 0;
                            foreach (IWebElement row in TRmulti11)
                            {

                                TDmulti11 = row.FindElements(By.TagName("td"));
                                if (TDmulti11.Count != 0)
                                {
                                    if (i == 0)
                                        Account_Number1 = TDmulti11[1].Text;
                                    if (i == 3)
                                        Balance = TDmulti11[1].Text;
                                    if (i == 4)
                                        Bill_Date = TDmulti11[1].Text;

                                    if (i == 5)
                                        Penalty_Date = TDmulti11[1].Text;
                                    if (i == 6)
                                        TurnOffDate = TDmulti11[1].Text;

                                    if (i == 7)
                                        PresentReadingDate = TDmulti11[1].Text;
                                    if (i == 8)
                                        PreviousReadingDate = TDmulti11[1].Text;
                                    if (i == 9)
                                        LastPaymentDate = TDmulti11[1].Text;
                                    if (i == 10)
                                        LastPaymentAmount = TDmulti11[1].Text;

                                    i++;
                                }

                            }
                            string Tax_utility = Account_Number1 + "~" + Balance + "~" + Bill_Date + "~" + Penalty_Date + "~" + TurnOffDate + "~" + PresentReadingDate + "~" + PreviousReadingDate + "~" + LastPaymentDate + "~" + LastPaymentAmount;
                            gc.insert_date(orderNumber, Account_id_number, 419, Tax_utility, 1, DateTime.Now);
                            //Account Number~Balance~Bill Date~Penalty Date~Turn Off Date~Present Reading Date~Previous Reading Date~Last Payment Date~Last Payment Amount

                        }
                        catch { }
                        driver.Navigate().Back();
                    }
                    /////prior year
                    foreach (string real in strTaxRealestate1)
                    {

                        IWebElement element2 = driver.FindElement(By.Id(real));
                        element2.Click();
                        Thread.Sleep(4000);
                        string a4 = "";
                        try
                        {//*[@id="screendump"]/fieldset/div[1]/div/div[2]/h3
                            a4 = driver.FindElement(By.XPath("//*[@id='screendump']/fieldset/div[1]/div/div[2]/h3")).Text.Trim();
                            //     Real Estate
                        }
                        catch { }
                        try
                        {

                            a4 = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lgdProp']")).Text.Trim();

                        }
                        catch { }
                        if (a4.Contains("Real Estate"))
                        {
                            gc.CreatePdf(orderNumber, Account_id_number, "Real Estate Prior", driver, "MD", "Harford");
                        }
                        if (a4.Contains("USER BENEFIT"))
                        {
                            gc.CreatePdf(orderNumber, Account_id_number, "User benefit prior", driver, "MD", "Harford");
                        }
                        if (a4.Contains("SCOTS FANCY"))
                        {
                            gc.CreatePdf(orderNumber, Account_id_number, "SCOTS FANCY prior", driver, "MD", "Harford");
                        }


                        //gc.CreatePdf(orderNumber, Account_id_number, "Parcel search result", driver, "MD", "Harford");
                        string prioryear = "";
                        string current_Interest = "", Goodthroughdate1 = "", DueAmount1 = "", Goodthroughdate2 = "", DueAmount2 = "";

                        prioryear = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblBillingPeriodText']")).Text.Trim();
                        prioryear = WebDriverTest.Before(prioryear, " (");
                        int i = 0;
                        if (!balancedue_prior.Contains("$0.00"))
                        {
                            IWebElement tbmulti11 = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_grdTaxBill_DXMainTable']/tbody"));
                            IList<IWebElement> TRmulti11 = tbmulti11.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDmulti11;
                            foreach (IWebElement row in TRmulti11)
                            {

                                TDmulti11 = row.FindElements(By.TagName("td"));
                                if (TDmulti11.Count != 0)
                                {
                                    if (i == 0)
                                        Tax_Amount = TDmulti11[1].Text;
                                    if (i == 1)
                                        Discount_Amount = TDmulti11[1].Text;
                                    if (i == 2)
                                        Interest = TDmulti11[1].Text;
                                    if (i == 3)
                                        Total_Paid = TDmulti11[1].Text;
                                    if (i == 4)
                                        current_Interest = TDmulti11[1].Text;
                                    if (i == 5)
                                    {
                                        Goodthroughdate1 = TDmulti11[0].Text;
                                        Goodthroughdate1 = WebDriverTest.After(Goodthroughdate1, "as of").Trim();
                                        DueAmount1 = TDmulti11[1].Text;
                                    }

                                    if (i == 6)
                                    {
                                        Goodthroughdate2 = TDmulti11[0].Text;
                                        Goodthroughdate2 = WebDriverTest.After(Goodthroughdate2, "Future Due Date (").Trim();
                                        Goodthroughdate2 = WebDriverTest.Before(Goodthroughdate2, "):").Trim();

                                        DueAmount2 = TDmulti11[1].Text;

                                    }

                                    i++;
                                }
                                //   Year~Tax Amount~Discount~Interest~Total Paid~Current Interest~Good through date1~Due Amount1~Good through date2~Due Amount2
                            }

                            string Tax_deli = prioryear + "~" + Tax_Amount + "~" + Discount_Amount + "~" + Interest + "~" + Total_Paid + "~" + current_Interest + "~" + Goodthroughdate1 + "~" + DueAmount1 + "~" + Goodthroughdate2 + "~" + DueAmount2;
                            gc.insert_date(orderNumber, Account_id_number, 420, Tax_deli, 1, DateTime.Now);

                        }
                        driver.Navigate().Back();
                        Thread.Sleep(2000);
                    }
                    string citytax = "";
                    try
                    {
                        citytax = driver.FindElement(By.Id("ContentPlaceHolder1_Label_TownInformation")).Text.Trim();

                    }
                    catch
                    {

                        citytax = "";
                    }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    if (citytax.Contains("City of Havre de Grace"))
                    {
                        try
                        {
                            driver.Navigate().GoToUrl("https://wipp.edmundsassoc.com/Wipp/?wippid=HAVR&modFilter=TU");
                            Thread.Sleep(4000);
                            //driver.set_page_load_timeout(30);
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/div/table[1]/tbody/tr/td/div/a")).Click();

                            string id1 = Account_id_number.Substring(0, 2);
                            string id11 = Account_id_number.Substring(2, 6);
                            Account_id_number_city = id1 + "-" + id11;

                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/div/table[1]/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr[1]/td[2]/input")).SendKeys(Account_id_number_city);
                            gc.CreatePdf(orderNumber, Account_id_number, "Taxinfo city", driver, "MD", "Harford");
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/div/table[1]/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr[1]/td[3]/button")).SendKeys(Keys.Enter);
                            gc.CreatePdf(orderNumber, Account_id_number, "Taxinfo city1", driver, "MD", "Harford");
                            string a = driver.FindElement(By.XPath(" /html/body/div[2]/div/table/tbody/tr[2]/td[2]/div/table/tbody/tr[1]/td/table/tbody/tr[2]/td[1]")).Text;
                            //          
                            string strTaxReal = "";
                            IWebElement ITaxReal11 = driver.FindElement(By.XPath("/html/body/div[2]/div/table/tbody/tr[2]/td[2]/div/table/tbody/tr[1]/td/table/tbody"));
                            IList<IWebElement> ITaxRealRow11 = ITaxReal11.FindElements(By.TagName("tr"));
                            IList<IWebElement> ITaxRealTd11;

                            foreach (IWebElement ItaxReal11 in ITaxRealRow11)
                            {

                                //  strTaxRealestate.Clear();
                                ITaxRealTd11 = ItaxReal11.FindElements(By.TagName("td"));
                                if (ItaxReal11.Text.Contains(Account_id_number_city))
                                {

                                    //  string yearbill = ITaxRealTd[0].Text;
                                    IWebElement ITaxBillCount = ITaxRealTd11[0].FindElement(By.TagName("input"));
                                    strTaxReal = ITaxBillCount.GetAttribute("id");
                                    strTaxRealestate.Add(strTaxReal);

                                }
                            }
                            IWebElement element1 = driver.FindElement(By.Id(strTaxReal));
                            element1.Click();
                            Thread.Sleep(4000);

                            gc.CreatePdf(orderNumber, Account_id_number, "Taxinfo city2", driver, "MD", "Harford");



                            //Account Number~Owner Name~Property Address~Land Value~Improvement Value~Exempt Value~Total Assessed Value~Deductions~Last Paid Date

                            string AccountNumber = "", OwnerName = "", PropertyAddress = "", LandValue = "", ImprovementValue = "", ExemptValue = "", TotalAssessedValue = "", Deductions = "", LastPaidDate = "";

                            int i = 0;
                            IWebElement tbmulti11 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/table/tbody/tr/td/table/tbody"));
                            IList<IWebElement> TRmulti11 = tbmulti11.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDmulti11;
                            foreach (IWebElement row in TRmulti11)
                            {

                                TDmulti11 = row.FindElements(By.TagName("td"));
                                if (TDmulti11.Count == 4)
                                {
                                    if (i == 0)
                                        AccountNumber = TDmulti11[3].Text;

                                    if (i == 1)
                                        PropertyAddress = TDmulti11[1].Text;
                                    if (i == 2)
                                    {
                                        OwnerName = TDmulti11[1].Text;
                                        LandValue = TDmulti11[3].Text;
                                    }
                                    if (i == 3)
                                        ImprovementValue = TDmulti11[3].Text;
                                    if (i == 4)
                                        ExemptValue = TDmulti11[3].Text;
                                    if (i == 5)
                                        TotalAssessedValue = TDmulti11[3].Text;
                                    if (i == 6)
                                        Deductions = TDmulti11[3].Text;
                                    i++;
                                }
                            }
                            LastPaidDate = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div/table/tbody/tr[3]/td/table/tbody/tr/td/div")).Text.Trim();
                            LastPaidDate = WebDriverTest.After(LastPaidDate, "Last Payment:").Trim();
                            string Tax_infoCity = AccountNumber + "~" + OwnerName + "~" + PropertyAddress + "~" + LandValue + "~" + ImprovementValue + "~" + ExemptValue + "~" + TotalAssessedValue + "~" + Deductions + "~" + LastPaidDate;
                            gc.insert_date(orderNumber, Account_id_number, 421, Tax_infoCity, 1, DateTime.Now);


                            IWebElement multitableElement1 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div/table/tbody/tr[2]/td/table/tbody"));
                            IList<IWebElement> multitableRow1 = multitableElement1.FindElements(By.TagName("tr"));
                            IList<IWebElement> multirowTD1;
                            foreach (IWebElement row in multitableRow1)
                            {
                                if (!row.Text.Contains("Year") && !row.Text.Contains("Last Payment:"))
                                {

                                    multirowTD1 = row.FindElements(By.TagName("td"));

                                    if (multirowTD1.Count != 0)
                                    {
                                        string tax_HistoryCity = multirowTD1[0].Text.Trim() + "~" + multirowTD1[1].Text.Trim() + "~" + multirowTD1[2].Text.Trim() + "~" + multirowTD1[3].Text.Trim() + "~" + multirowTD1[5].Text.Trim() + "~" + multirowTD1[6].Text.Trim() + "~" + multirowTD1[7].Text.Trim() + "~" + multirowTD1[8].Text.Trim();
                                        gc.insert_date(orderNumber, Account_id_number, 423, tax_HistoryCity, 1, DateTime.Now);
                                    }
                                }
                                //Year~Due Date~Type~Tax Amount~Balance~Interest~Total Due~Status

                            }
                        }
                        catch { }
                    }
                    if (citytax.Contains("City of Aberdeen"))
                    {
                        try
                        {
                            driver.Navigate().GoToUrl("http://propertytax.aberdeenmd.gov:8000/cgi-bin/WebProperty.exe");
                            Thread.Sleep(5000);
                            //*[@id="header"]/form/div[1]/div/input
                            driver.FindElement(By.XPath("//*[@id='header']/form/div[1]/div/input")).SendKeys(Account_id_number);
                            gc.CreatePdf(orderNumber, Account_id_number, "Aberdeen city", driver, "MD", "Harford");
                            //*[@id="header"]/form/div[4]/button
                            driver.FindElement(By.XPath("//*[@id='header']/form/div[4]/button")).SendKeys(Keys.Enter);
                            Thread.Sleep(4000);
                            gc.CreatePdf(orderNumber, Account_id_number, "Aberdeen city1", driver, "MD", "Harford");
                            //*[@id="PropertyInfo"]/table/tbody/tr[1]/td
                            string city_parcel = driver.FindElement(By.XPath("//*[@id='PropertyInfo']/table/tbody/tr[1]/td")).Text.Trim();
                            city_parcel = WebDriverTest.After(city_parcel, "Parcel Number:").Trim();

                            string totaldue_city = driver.FindElement(By.XPath("//*[@id='PropertyInfo']/table/tbody/tr[6]/td")).Text.Trim();
                            totaldue_city = WebDriverTest.After(totaldue_city, "Total Amount Due:").Trim();


                            string tax_infoCity1 = city_parcel + "~" + Owner_name + "~" + Address + "~" + totaldue_city;
                            gc.insert_date(orderNumber, Account_id_number, 424, tax_infoCity1, 1, DateTime.Now);

                            //Parcel#~Owner_name~Address~totaldue_city
                            //*[@id="propertyinfo"]/div[2]/button[1]
                            // view charge history
                            driver.FindElement(By.XPath("//*[@id='propertyinfo']/div[2]/button[1]")).Click();
                            Thread.Sleep(4000);
                            gc.CreatePdf(orderNumber, Account_id_number, "view charge history", driver, "MD", "Harford");
                            IWebElement multitableElement1 = driver.FindElement(By.XPath("//*[@id='header']/form/table/tbody"));
                            IList<IWebElement> multitableRow1 = multitableElement1.FindElements(By.TagName("tr"));
                            IList<IWebElement> multirowTD1;
                            foreach (IWebElement row in multitableRow1)
                            {
                                if (!row.Text.Contains("Tax Year"))
                                {
                                    //*[@id="header"]/form/table
                                    multirowTD1 = row.FindElements(By.TagName("td"));

                                    if (multirowTD1.Count != 0)
                                    {
                                        string tax_HistoryCity1 = multirowTD1[0].Text.Trim() + "~" + multirowTD1[1].Text.Trim() + "~" + multirowTD1[2].Text.Trim() + "~" + multirowTD1[3].Text.Trim() + "~" + multirowTD1[4].Text.Trim() + "~" + multirowTD1[5].Text.Trim() + "~" + multirowTD1[6].Text.Trim() + "~" + multirowTD1[7].Text.Trim() + "~" + multirowTD1[8].Text.Trim() + "~" + multirowTD1[9].Text.Trim();
                                        gc.insert_date(orderNumber, Account_id_number, 425, tax_HistoryCity1, 1, DateTime.Now);
                                    }
                                }
                                //Tax Year~Due Date~Description~Tax Amount~Penalty~Interest~Gross Tax~Paid Amount~Check Number~Paid Date

                            }
                            //   goback

                            driver.Navigate().Back();
                            Thread.Sleep(2000);


                            //Current Unpaid Charges
                            try
                            {
                                driver.FindElement(By.XPath("//*[@id='propertyinfo']/div[2]/button[2]")).Click();
                                Thread.Sleep(2000);
                                gc.CreatePdf(orderNumber, Account_id_number, "Current Unpaid Charges", driver, "MD", "Harford");
                                IWebElement multitableElement13 = driver.FindElement(By.XPath("//*[@id='header']/form/table/tbody"));
                                IList<IWebElement> multitableRow13 = multitableElement13.FindElements(By.TagName("tr"));
                                IList<IWebElement> multirowTD13;
                                foreach (IWebElement row in multitableRow13)
                                {
                                    if (!row.Text.Contains("Fiscal Year"))
                                    {
                                        multirowTD13 = row.FindElements(By.TagName("td"));

                                        if (multirowTD13.Count != 0)
                                        {
                                            string tax_HistoryCity11 = multirowTD13[0].Text.Trim() + "~" + multirowTD13[1].Text.Trim() + "~" + multirowTD13[2].Text.Trim() + "~" + multirowTD13[3].Text.Trim() + "~" + multirowTD13[4].Text.Trim() + "~" + multirowTD13[5].Text.Trim() + "~" + multirowTD13[6].Text.Trim();
                                            gc.insert_date(orderNumber, Account_id_number, 551, tax_HistoryCity11, 1, DateTime.Now);
                                        }
                                    }
                                    //Year~In Effect~Description~Normal~Penalty~Interest~Gross

                                }


                            }
                            catch
                            { }
                            //   goback
                            driver.Navigate().Back();
                            Thread.Sleep(2000);

                            //view assessment history 
                            driver.FindElement(By.XPath("//*[@id='propertyinfo']/div[2]/button[3]")).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, Account_id_number, "view assessment history ", driver, "MD", "Harford");

                            //   goback
                            driver.Navigate().Back();

                            Thread.Sleep(2000);

                            //  view bills 
                            driver.FindElement(By.XPath("//*[@id='propertyinfo']/div[2]/button[5]")).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, Account_id_number, "view bills dis ", driver, "MD", "Harford");

                            //driver.FindElement(By.XPath("//*[@id='header']/form/table/tbody/tr[2]/td[2]/button")).Click();
                            //Thread.Sleep(2000);
                            //driver.SwitchTo().Window(driver.WindowHandles[1]);
                            //Thread.Sleep(2000);
                            //gc.CreatePdf(orderNumber, Account_id_number, "view bills ", driver, "MD", "Harford");
                            //   goback
                            // driver.Navigate().Back();

                            String Parent_Window1 = driver.CurrentWindowHandle;

                            driver.FindElement(By.XPath("//*[@id='header']/form/table/tbody/tr[2]/td[2]/button")).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, Account_id_number, "view bills ", driver, "MD", "Harford");
                            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                            driver.SwitchTo().Window(driver.WindowHandles.Last());
                            Thread.Sleep(4000);

                            driver.SwitchTo().Window(Parent_Window1);
                            Thread.Sleep(2000);




                        }
                        catch { }
                    }
                    CitytaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "MD", "Harford", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    // driver.Quit();
                    GlobalClass.titleparcel = "";
                    gc.mergpdf(orderNumber, "MD", "Harford");
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