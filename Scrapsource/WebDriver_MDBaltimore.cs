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
using System.Web.UI;
using System.Text.RegularExpressions;
namespace ScrapMaricopa.Scrapsource
{

    public class WebDriver_MDBaltimore
    {

        IWebDriver driver;
        DBconnection db = new DBconnection();

        GlobalClass gc = new GlobalClass();

        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        private object driverIE;
        private int multicount;
        private string strMultiAddress;
        private string b;

        public string FTP_Baltimore(string houseno, string sname, string stype,string unitno, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {

            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            string Tax_Year = "", Paid_Date = "", Discount = "", Paid_Amount = "", Total_Due = "", b = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("https://sdat.dat.maryland.gov/RealProperty/Pages/default.aspx");
                    Thread.Sleep(3000);
                    var Select = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucSearchType_ddlCounty"));
                    var selectElement1 = new SelectElement(Select);
                    selectElement1.SelectByText("BALTIMORE CITY");
                    if (searchType == "address")
                    {
                        var Select1 = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucSearchType_ddlSearchType"));
                        var selectElement11 = new SelectElement(Select1);
                        selectElement11.SelectByText("STREET ADDRESS");
                        gc.CreatePdf_WOP(orderNumber, "Address", driver, "MD", "Baltimore City");
                        driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_StartNavigationTemplateContainerID_btnContinue")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtStreenNumber")).SendKeys(houseno);
                        driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtStreetName")).SendKeys(sname);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "MD", "Baltimore City");
                        driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_StepNavigationTemplateContainerID_btnStepNextButton")).SendKeys(Keys.Enter);

                        Thread.Sleep(4000);
                        try
                        {
                            if (driver.FindElement(By.XPath(" //*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucSearchResult_gv_SearchResult']/tbody")).Displayed)
                            {

                                gc.CreatePdf_WOP(orderNumber, "MultiParcel search result", driver, "MD", "Baltimore City");
                                IWebElement multitableElement11 = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucSearchResult_gv_SearchResult']/tbody"));
                                IList<IWebElement> multitableRow11 = multitableElement11.FindElements(By.TagName("tr"));
                                IList<IWebElement> multirowTD11;
                                int k = 0;
                                foreach (IWebElement row in multitableRow11)
                                {
                                    if (k <= 25)
                                    {
                                        if (!row.Text.Contains("Name"))
                                        {
                                            multirowTD11 = row.FindElements(By.TagName("td"));
                                            if (multirowTD11.Count != 1 && multirowTD11[0].Text.Trim() != "")
                                            {
                                                string p_no = multirowTD11[1].Text.Trim();
                                                p_no = p_no.Replace(" ", "");
                                                string multi_parcel = multirowTD11[0].Text.Trim() + "~" + multirowTD11[1].Text.Trim() + "~" + multirowTD11[2].Text.Trim() + "~" + multirowTD11[3].Text.Trim() + "~" + multirowTD11[4].Text.Trim() + "~" + multirowTD11[5].Text.Trim();
                                                gc.insert_date(orderNumber, p_no, 210, multi_parcel, 1, DateTime.Now);
                                            }
                                        }
                                        k++;
                                    }
                                }
                                if (multitableRow11.Count > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_Baltimore_Multicount"] = "Maximum";
                                }
                                else
                                {
                                    HttpContext.Current.Session["multiParcel_Baltimore"] = "Yes";
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
                        string titleaddress = houseno + " " + sname + " " + stype + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, "", ownername, titleaddress.Trim(), "MD", "Baltimore City");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_MDBaltimore"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "parcel")
                    {
                        //if ((HttpContext.Current.Session["titleflex_alternateAPN"] == null))
                        //{
                        //    gc.TitleFlexSearch(orderNumber, parcelNumber, "", "", "MD", "Baltimore City");
                        //    string strParcelNumber = HttpContext.Current.Session["titleflex_alternateAPN"].ToString();
                        //    if (strParcelNumber.Trim().Length == 13 || strParcelNumber.Trim().Length == 12 || strParcelNumber.Trim().Length == 9)
                        //    {
                        //        parcelNumber = strParcelNumber.Replace("-", "").Replace(" ", "");
                        //    }
                        //    HttpContext.Current.Session["titleflex_alternateAPN"] = null;
                        //}
                        //if (HttpContext.Current.Session["titleparcel"] != null && HttpContext.Current.Session["titleflex_alternateAPN"] != null)
                        //{
                        //    string strParcelNumber = HttpContext.Current.Session["titleflex_alternateAPN"].ToString();
                        //    if (strParcelNumber.Trim().Length == 13 || strParcelNumber.Trim().Length == 12 || strParcelNumber.Trim().Length == 9)
                        //    {
                        //        parcelNumber = strParcelNumber.Replace("-", "").Replace(" ", "");
                        //    }
                        //}
                        var Select11 = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucSearchType_ddlSearchType"));
                        var selectElement111 = new SelectElement(Select11);
                        selectElement111.SelectByText("PROPERTY ACCOUNT IDENTIFIER");
                        gc.CreatePdf_WOP(orderNumber, "Parcel search", driver, "MD", "Baltimore City");
                        driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_StartNavigationTemplateContainerID_btnContinue")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        if (parcelNumber.Contains("-"))
                        {
                            parcelNumber = parcelNumber.Replace("-", "");
                        }
                        if (parcelNumber.Length == 12)
                        {
                            string ward = parcelNumber.Substring(0, 2);
                            driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtWard")).SendKeys(ward);
                            string section1 = parcelNumber.Substring(2, 2);
                            driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtSection")).SendKeys(section1);
                            string block = parcelNumber.Substring(4, 5);
                            driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtBlock")).SendKeys(block);
                            string lot = parcelNumber.Substring(9, 3);
                            driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtLot")).SendKeys(lot);
                        }
                        else
                        {
                            string ward1 = parcelNumber.Substring(0, 2);
                            driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtWard")).SendKeys(ward1);
                            string section11 = parcelNumber.Substring(2, 2);
                            driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtSection")).SendKeys(section11);

                            string block1 = parcelNumber.Substring(4, 4);
                            driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtBlock")).SendKeys(block1);
                            string lot1 = parcelNumber.Substring(8, 3);
                            driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtLot")).SendKeys(lot1);
                        }
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search result", driver, "MD", "Baltimore City");
                        driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_StepNavigationTemplateContainerID_btnStepNextButton")).SendKeys(Keys.Enter);
                        Thread.Sleep(5000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search result1", driver, "MD", "Baltimore City");
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_lblErr"));
                        if(INodata.Text.Contains("no records that match your criteria"))
                        {
                            HttpContext.Current.Session["Nodata_MDBaltimore"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    //property details
                    gc.CreatePdf(orderNumber, parcelNumber, "Assessment details", driver, "MD", "Baltimore City");
                    string Account_id_number = "", Owner_name = "", Address = "", Legal_Description = "", Year_Built = "", use = "", Principal_Residence = "", Map = "", Sections = "", Land = "", Building = "", Total_Assessed_Value = "";
                    string Grid = "", Parcel = "", Sub_District = "", Subdivision = "", Block = "", Lot = "", Assessment_Year = "", Homestead_Application_Status = "", Homeowners_Tax_Credit_Application_Status = "", Homeowners_Tax_Credit_Application_Date = "";
                    Account_id_number = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblDetailsStreetHeader_0")).Text.Trim();
                    string value = Account_id_number.Replace("Ward", "").Replace("Section", "").Replace("Block", "").Replace("Lot", "");
                    Account_id_number = value.Replace(" ", "").Replace("-", "");


                    Owner_name = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblOwnerName_0")).Text.Trim();
                    Address = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblPremisesAddress_0")).Text.Trim();
                    Address = Address.Replace("\r\n", " ");

                    string Mail_Address = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblMailingAddress_0")).Text.Trim();
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

                    string property_details = Account_id_number + "~" + Owner_name + "~" + Address + "~" + Mail_Address + "~" + Legal_Description + "~" + Year_Built + "~" + use + "~" + Principal_Residence + "~" + Map + "~" + Grid + "~" + Parcel + "~" + Sub_District + "~" + Subdivision + "~" + Sections + "~" + Block + "~" + Lot + "~" + Assessment_Year + "~" + Homestead_Application_Status + "~" + Homeowners_Tax_Credit_Application_Status + "~" + Homeowners_Tax_Credit_Application_Date;
                    gc.insert_date(orderNumber, Account_id_number, 206, property_details, 1, DateTime.Now);

                    //Assessment Details Table:

                    Land = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblBaseLand_0")).Text.Trim();
                    Building = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblBaseImprove_0")).Text.Trim();
                    Total_Assessed_Value = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblBaseTotal_0")).Text.Trim();

                    string Assessment_details = Land + "~" + Building + "~" + Total_Assessed_Value;
                    gc.insert_date(orderNumber, Account_id_number, 207, Assessment_details, 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("http://cityservices.baltimorecity.gov/realproperty/default.aspx");
                    Thread.Sleep(3000);
                    driver.FindElement(By.Id("ctl00_ctl00_rootMasterContent_LocalContentPlaceHolder_txtBlock")).SendKeys(Block);
                    driver.FindElement(By.Id("ctl00_ctl00_rootMasterContent_LocalContentPlaceHolder_txtLot")).SendKeys(Lot);
                    gc.CreatePdf(orderNumber, Account_id_number, "Tax_info", driver, "MD", "Baltimore City");
                    driver.FindElement(By.Id("ctl00_ctl00_rootMasterContent_LocalContentPlaceHolder_btnSearch")).SendKeys(Keys.Enter);
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, Account_id_number, "Tax_info1", driver, "MD", "Baltimore City");
                    driver.FindElement(By.Id("ctl00_ctl00_rootMasterContent_LocalContentPlaceHolder_DataGrid1_ctl02_lnkBtnSelect")).Click();
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, Account_id_number, "Tax_info2", driver, "MD", "Baltimore City");
                    string other_charges = "";
                    Tax_Year = driver.FindElement(By.Id("ctl00_ctl00_rootMasterContent_LocalContentPlaceHolder_LabelEndFY")).Text;
                    string other = "";
                    try
                    {
                        other = driver.FindElement(By.Id("ctl00_ctl00_rootMasterContent_LocalContentPlaceHolder_Other2")).Text;
                    }
                    catch
                    {
                        other = "";
                    }

                    try
                    {
                        if (other != "OTHER CHARGES")
                        {

                            Paid_Date = driver.FindElement(By.Id("ctl00_ctl00_rootMasterContent_LocalContentPlaceHolder_Paid")).Text;
                            Paid_Date = Regex.Replace(Paid_Date, "[A-Za-z ]", "").Trim();
                            string a1 = Paid_Date.Substring(0, 8);
                            Discount = Paid_Date.Substring(8, 4);
                            Paid_Date = a1;
                            Paid_Amount = driver.FindElement(By.Id("ctl00_ctl00_rootMasterContent_LocalContentPlaceHolder_AmtPaid")).Text;
                        }
                        else
                        {
                            Paid_Date = " ";
                            Discount = " ";
                            Paid_Date = driver.FindElement(By.Id("ctl00_ctl00_rootMasterContent_LocalContentPlaceHolder_Other")).Text;
                            Paid_Amount = driver.FindElement(By.Id("ctl00_ctl00_rootMasterContent_LocalContentPlaceHolder_OtherAmt")).Text;
                            Paid_Date = Regex.Replace(Paid_Date, "[A-Za-z ]", "").Trim();
                            string a1 = Paid_Date.Substring(0, 8);
                            Discount = Paid_Date.Substring(8, 4);
                            Paid_Date = a1;
                            other_charges = driver.FindElement(By.Id("ctl00_ctl00_rootMasterContent_LocalContentPlaceHolder_Other2Amt")).Text;


                        }
                    }
                    catch { }

                    Total_Due = driver.FindElement(By.Id("ctl00_ctl00_rootMasterContent_LocalContentPlaceHolder_AmountDue")).Text;
                    if (!Total_Due.Contains("0.00"))
                    {

                        other_charges = driver.FindElement(By.Id("ctl00_ctl00_rootMasterContent_LocalContentPlaceHolder_OtherAmt")).Text;

                    }
                    IWebElement multitableElement1 = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_rootMasterContent_LocalContentPlaceHolder_Table1']/tbody"));
                    IList<IWebElement> multitableRow1 = multitableElement1.FindElements(By.TagName("tr"));
                    IList<IWebElement> multirowTD1;
                    foreach (IWebElement row in multitableRow1)
                    {
                        if (!row.Text.Contains("ASSESSMENT"))
                        {
                            if (!row.Text.Contains("PAID"))
                            {
                                multirowTD1 = row.FindElements(By.TagName("td"));
                                if (multirowTD1.Count != 1)
                                {
                                    if (multirowTD1.Count == 4)
                                    {
                                        string tax_info = Tax_Year + "~" + "" + "~" + multirowTD1[0].Text.Trim() + "~" + multirowTD1[1].Text.Trim() + "~" + multirowTD1[2].Text.Trim() + "~" + multirowTD1[3].Text.Trim() + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-";
                                        gc.insert_date(orderNumber, Account_id_number, 208, tax_info, 1, DateTime.Now);
                                    }
                                }
                            }
                        }

                    }



                    string tax_info1 = Tax_Year + "~" + Paid_Date + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + other_charges + "~" + Discount + "~" + Paid_Amount + "~" + Total_Due;
                    gc.insert_date(orderNumber, Account_id_number, 208, tax_info1, 1, DateTime.Now);

                    string Taxing_authority_address1 = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_rootMasterContent_LocalContentPlaceHolder_tblDetail']/tbody/tr[2]/td[2]")).Text.Trim();
                    Taxing_authority_address1 = Taxing_authority_address1.Replace("\r\n", " ");
                    string Taxing_authority_address = WebDriverTest.Before(Taxing_authority_address1, "TELEPHONE").Trim();

                    string telephone = WebDriverTest.After(Taxing_authority_address1, "BILLING").Trim();
                    telephone = WebDriverTest.Before(telephone, "IVR   REFERENCE").Trim();

                    string tax_info2 = "Taxing Authority  :" + "~" + Taxing_authority_address + "~" + "Phone Number  :" + "~" + telephone + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-";
                    gc.insert_date(orderNumber, Account_id_number, 208, tax_info2, 1, DateTime.Now);

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    //Tax Due / Delinquent Details
                    if (!Total_Due.Contains("0.00"))
                    {
                        string install = "";
                        IWebElement multitableElement11 = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_rootMasterContent_LocalContentPlaceHolder_Table2']/tbody"));
                        IList<IWebElement> multitableRow11 = multitableElement11.FindElements(By.TagName("tr"));
                        IList<IWebElement> multirowTD11;
                        foreach (IWebElement row in multitableRow11)
                        {
                            if (!row.Text.Contains("SEMIANNUAL PAYMENT"))
                            {
                                if (row.Text.Contains("1ST INSTALLMENT"))
                                {
                                    install = "1ST INSTALLMENT";
                                }
                                if (row.Text.Contains("2ND INSTALLMENT"))
                                {
                                    install = "2ND INSTALLMENT";
                                }
                                if (row.Text.Contains("ANNUAL PAYMENT SCHEDULE"))
                                {
                                    install = "ANNUAL PAYMENT SCHEDULE";
                                }
                                if (!row.Text.Contains("IF PAID BY"))
                                {
                                    if (!row.Text.Contains("  "))
                                    {
                                        multirowTD11 = row.FindElements(By.TagName("td"));
                                        if (multirowTD11.Count == 3)
                                        {
                                            string tax_info = "-" + "~" + install + "~" + multirowTD11[0].Text.Trim() + "~" + multirowTD11[1].Text.Trim() + "~" + multirowTD11[2].Text.Trim();
                                            gc.insert_date(orderNumber, Account_id_number, 209, tax_info, 1, DateTime.Now);
                                        }
                                    }
                                }
                            }
                        }
                        string service_fee = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_rootMasterContent_LocalContentPlaceHolder_Table2']/tbody/tr[14]/td[1]")).Text;
                        tax_info1 = service_fee + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                        gc.insert_date(orderNumber, Account_id_number, 209, tax_info1, 1, DateTime.Now);
                        string fee = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_rootMasterContent_LocalContentPlaceHolder_Table2']/tbody/tr[14]/td[2]")).Text;
                        tax_info2 = fee + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                        gc.insert_date(orderNumber, Account_id_number, 209, tax_info2, 1, DateTime.Now);
                    }

                    CitytaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "MD", "Baltimore City", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "MD", "Baltimore City");
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