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
    public class Webdriver_CharlesMD
    {

        IWebDriver driver;
        DBconnection db = new DBconnection();

        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;                                
        public string FTP_Charles(string houseno, string direction, string sname,string sType , string unitNumber, string parcelNumber, string ownername, string searchType, string orderNumber)
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
                string Route = "", address = "", parcel = "", owner = "", Accnumber = "", PropertyNumber = "", TaxAuthority = "";
                string[] stringSeparators1 = new string[] { "\r\n" };
                List<string> listurl = new List<string>();
                List<string> listurl1 = new List<string>();
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("https://www.charlescountymd.gov/fas/treasury/treasury-your-taxes");
                    string taxauth = driver.FindElement(By.XPath("//*[@id='mini-panel-footer_contact_info']/div[1]/div/div/div/div/div/div/div/div/p")).Text;
                    TaxAuthority = gc.Between(taxauth, "GOVERNMENT", "Mobile Site");
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Authority", driver, "MD", "Charles");

                    if (searchType == "titleflex")
                    {
                        string titleaddress = "";
                        if (direction != "")
                        {
                            titleaddress = houseno + " " + direction + " " + sname + " " + sType + " " + unitNumber;
                        }
                        if (direction == "")
                        {
                            titleaddress = houseno + " " + sname + " " + sType + " " + unitNumber;
                        }
                        gc.TitleFlexSearch(orderNumber, "", ownername, titleaddress, "MD", "Charles");

                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_CharlesMD"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("https://sdat.dat.maryland.gov/RealProperty/Pages/default.aspx");
                        Thread.Sleep(1000);


                        var SerachCategory = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucSearchType_ddlCounty']"));
                        var selectElement1 = new SelectElement(SerachCategory);
                        selectElement1.SelectByText("CHARLES COUNTY");

                        var SerachCategoryM = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucSearchType_ddlSearchType']"));
                        var selectElementM1 = new SelectElement(SerachCategoryM);
                        selectElementM1.SelectByText("STREET ADDRESS");
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "MD", "Charles");
                        driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_StartNavigationTemplateContainerID_btnContinue']")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);

                        driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtStreenNumber']")).SendKeys(houseno);
                        driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtStreetName']")).SendKeys(sname);
                        gc.CreatePdf_WOP(orderNumber, "Address search Input", driver, "MD", "Charles");
                        driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_StepNavigationTemplateContainerID_btnStepNextButton']")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        //gc.CreatePdf_WOP(orderNumber, "Address search Result", driver, "MD", "Charles");

                        try
                        {
                            IWebElement MultiOwnerTable = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucSearchResult_gv_SearchResult']/tbody"));
                            IList<IWebElement> MultiOwnerRow = MultiOwnerTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiOwnerTD;
                            try
                            {
                                if (MultiOwnerRow.Count > 28)
                                {
                                    HttpContext.Current.Session["MDCharles_Count"] = "Maimum";
                                    return "Maximum";
                                }
                            }
                            catch { }

                            foreach (IWebElement row1 in MultiOwnerRow)
                            {
                                MultiOwnerTD = row1.FindElements(By.TagName("td"));
                                if ((MultiOwnerTD.Count != 0) && row1.Text.Trim() != "")
                                {
                                    owner = MultiOwnerTD[0].Text;
                                    Accnumber = MultiOwnerTD[1].Text;
                                    PropertyAddress = MultiOwnerTD[2].Text;
                                    string Multi = owner + "~" + PropertyAddress;
                                    gc.insert_date(orderNumber, Accnumber, 965, Multi, 1, DateTime.Now);
                                }
                            }

                            HttpContext.Current.Session["MDCharles"] = "Yes";
                            driver.Quit();
                            return "MultiParcel";
                        }
                        catch { }
                    }

                    else if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("https://sdat.dat.maryland.gov/RealProperty/Pages/default.aspx");
                        Thread.Sleep(1000);
                        var SerachCategory = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucSearchType_ddlCounty']"));
                        var selectElement1 = new SelectElement(SerachCategory);
                        selectElement1.SelectByText("CHARLES COUNTY");
                        var SerachCategoryM = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucSearchType_ddlSearchType']"));
                        var selectElementM1 = new SelectElement(SerachCategoryM);
                        selectElementM1.SelectByText("PROPERTY ACCOUNT IDENTIFIER");
                        Thread.Sleep(3000);
                        // gc.CreatePdf_WOP(orderNumber, "Parcel search", driver, "MD", "Charles");
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
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "MD", "Charles");
                        driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_StepNavigationTemplateContainerID_btnStepNextButton']")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_lblErr"));
                        if(INodata.Text.Contains("There are no records that match your criteria for county"))
                        {
                            HttpContext.Current.Session["Nodata_CharlesMD"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    string LegalDescription = "", MailingAddress = "", PremisesAddress = "", Account_Identifier = "", Parcel = "", Use = "", PrincipalResidence = "", Map = "", Grid = "", SubDistrict = "", Subdivision = "", Section = "", Block = "", Lot = "", AssessmentYear = "", Town = "";
                    string District = "", PrimaryStructureBuilt = "", PropertyLandArea = "", Parcelno = "";
                    parcelNumber = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblDetailsStreetHeader_0']")).Text.Replace("Folio:", "");
                    Account_Identifier = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblDetailsStreetHeader_0']")).Text.Replace("Folio:", "");
                    District = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblDetailsStreetHeader_0']")).Text.Replace("Folio:", "");
                    parcelNumber = GlobalClass.After(parcelNumber, "Account Number - ");
                    District = gc.Between(District, "District -", "Account Number -");
                    Parcelno = District + parcelNumber;
                    parcelNumber = Parcelno.Replace(" ", "");
                    Parcelno = Parcelno.Replace(" ", "");
                    gc.CreatePdf(orderNumber, parcelNumber, "Assessment information", driver, "MD", "Charles");

                    ownername = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblOwnerName_0']")).Text;
                    MailingAddress = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblPremisesAddress_0']")).Text.Replace("\r\n", "");
                    LegalDescription = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblLegalDescription_0']")).Text.Replace("\r\n", "");
                    YearBuilt = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_Label18_0']")).Text;
                    Use = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblUse_0']")).Text;
                    PrincipalResidence = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblPrinResidence_0']")).Text;
                    PremisesAddress = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblPremisesAddress_0']")).Text;
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
                    PrimaryStructureBuilt = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_Label18_0']")).Text;
                    PropertyLandArea = driver.FindElement(By.XPath("//*[@id='MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_Label20_0']")).Text;
                    string Property = Account_Identifier + "~" + ownername + "~" + MailingAddress + "~" + Use + "~" + PrincipalResidence + "~" + PremisesAddress + "~" + LegalDescription + "~" + Map + "~" + Grid + "~" + Parcel + "~" + SubDistrict + "~" + Subdivision + "~" + Section + "~" + Block + "~" + Lot + "~" + AssessmentYear + "~" + PrimaryStructureBuilt + "~" + PropertyLandArea;
                    gc.insert_date(orderNumber, parcelNumber, 957, Property, 1, DateTime.Now);


                    //Assessment Details Table:

                    string date1 = "", date2 = "", Total1 = "", Total2 = "";

                    date1 = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblPhaseDate_0")).Text.Trim().Replace("As of", "").Replace("\r\n", "");
                    date2 = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblAssesDate_0")).Text.Trim().Replace("As of", "").Replace("\r\n", "");
                    Total1 = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblPhaseInTotal_0")).Text.Trim();
                    Total2 = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblAssesTotal_0")).Text.Trim();
                    string Assessment_details1 = date1 + "~" + Total1;
                    gc.insert_date(orderNumber, parcelNumber, 959, Assessment_details1, 1, DateTime.Now);
                    string Assessment_details2 = date2 + "~" + Total2;
                    gc.insert_date(orderNumber, parcelNumber, 959, Assessment_details2, 1, DateTime.Now);

                    //   Year~Total Assessed Value
                    //string year = "", TotalAssessedValue = "";
                    //try
                    //{
                    //    IWebElement TaxHisTBD = driver.FindElement(By.XPath("//*[@id='detailSearch']/tbody/tr[15]/td/table/tbody"));
                    //    IList<IWebElement> TaxHisTRD = TaxHisTBD.FindElements(By.TagName("tr"));
                    //    IList<IWebElement> TaxHisTDD;
                    //    foreach (IWebElement row1 in TaxHisTRD)
                    //    {
                    //        TaxHisTDD = row1.FindElements(By.TagName("td"));
                    //        if (TaxHisTDD.Count != 0)
                    //        {
                    //            if (TaxHisTDD[0].Text.Trim() == "" && !row1.Text.Contains("Phase-in Assessments"))
                    //            {
                    //                string Assessmentdetials = "" + "~" + "" + "~" + TaxHisTDD[2].Text.Replace("\r\n", " ") + "~" + TaxHisTDD[3].Text.Replace("\r\n", " ") + "~" + TaxHisTDD[4].Text.Replace("\r\n", " ");
                    //                gc.insert_date(orderNumber, parcelNumber, 959, Assessmentdetials, 1, DateTime.Now);
                    //            }
                    //            else if (row1.Text.Contains("Land") && !row1.Text.Contains("Preferential") || row1.Text.Contains("Improvements") && !row1.Text.Contains("Phase-in Assessments"))

                    //            {
                    //                string Assessmentdetials2 = TaxHisTDD[0].Text + "~" + TaxHisTDD[1].Text + "~" + TaxHisTDD[2].Text + "~" + "" + "~" + "";

                    //                gc.insert_date(orderNumber, parcelNumber, 959, Assessmentdetials2, 1, DateTime.Now);

                    //            }

                    //            else if (row1.Text.Contains("Total:") && !row1.Text.Contains("Phase-in Assessments") && !row1.Text.Contains("Preferential"))

                    //            {
                    //                string Assessmentdetials3 = TaxHisTDD[0].Text + "~" + TaxHisTDD[1].Text + "~" + TaxHisTDD[2].Text + "~" + TaxHisTDD[3].Text + "~" + TaxHisTDD[4].Text;

                    //                gc.insert_date(orderNumber, parcelNumber, 959, Assessmentdetials3, 1, DateTime.Now);

                    //            }
                    //            else if (row1.Text.Contains("Preferential Land") && !row1.Text.Contains("Phase-in Assessments"))

                    //            {
                    //                string Assessmentdetials4 = TaxHisTDD[0].Text + "~" + TaxHisTDD[1].Text + "~" + "" + "~" + "" + "~" + TaxHisTDD[4].Text;

                    //                gc.insert_date(orderNumber, parcelNumber, 959, Assessmentdetials4, 1, DateTime.Now);

                    //            }
                    //        }

                    //    }
                    //}

                    //catch
                    //{

                    //}

                    // Homestead Details Table
                    try
                    {
                        string HSApplicationStatus = "", TaxCreditAppStatus = "", HSApplicationStatus_Value = "", TaxCreditAppStatus_Value = "", HSDate = "", HSDatedata = "";
                        IWebElement HSAppStatus = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_HyperLink17_0"));
                        HSApplicationStatus = HSAppStatus.Text;
                        IWebElement HSAppStatus_val = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblHomeStatus_0"));
                        HSApplicationStatus_Value = HSAppStatus_val.Text;
                        IWebElement TaxAppStatus = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_HyperLink37_0"));
                        TaxCreditAppStatus = TaxAppStatus.Text;
                        IWebElement TaxAppStatus_val = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblHTC_Status_0"));
                        TaxCreditAppStatus_Value = TaxAppStatus_val.Text;
                        IWebElement Date = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_HyperLink40_0"));
                        HSDate = Date.Text;
                        IWebElement Datedata = driver.FindElement(By.Id("MainContent_MainContent_cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lblHTC_StatusDate_0"));
                        HSDatedata = Datedata.Text;
                        string HomesteadDetails1 = HSApplicationStatus + "~" + HSApplicationStatus_Value;
                        string HomesteadDetails2 = TaxCreditAppStatus + "~" + TaxCreditAppStatus_Value;
                        string HomesteadDetails3 = HSDate + "~" + HSDatedata;
                        gc.insert_date(orderNumber, parcelNumber, 960, HomesteadDetails1, 1, DateTime.Now);
                        gc.insert_date(orderNumber, parcelNumber, 960, HomesteadDetails2, 1, DateTime.Now);
                        gc.insert_date(orderNumber, parcelNumber, 960, HomesteadDetails3, 1, DateTime.Now);
                    }
                    catch
                    {

                    }


                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    //Legal   
                    string Ownername = "", Propertynumber = "", Mailingaddress = "", Premiseaddress = "", TotalDue = "", LegalDesc = "";
                    driver.Navigate().GoToUrl("http://www.charlescounty.org/treas/taxes/acctinquiry/legal.jsp?propertyNumber=" + parcelNumber + "");
                    Thread.Sleep(2000);
                    LegalDesc = driver.FindElement(By.XPath("/html/body/table/tbody")).Text.Replace("\r\n", "");
                    gc.CreatePdf(orderNumber, parcelNumber, "Legal Description", driver, "MD", "Charles");

                    // Current Tax Info
                    driver.Navigate().GoToUrl("http://www.charlescounty.org/treas/taxes/acctinquiry/selection.jsp");
                    Thread.Sleep(4000);

                    List<string> TaxYearDetails = new List<string>();
                    driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/form/table[1]/tbody/tr[4]/td[2]/input")).SendKeys(Parcelno);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Info Input", driver, "MD", "Charles");
                    driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/form/table[2]/tbody/tr[10]/td/div/strong/input")).SendKeys(Keys.Enter);
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Info Result", driver, "MD", "Charles");
                    string bulkdata = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table[1]/tbody")).Text;
                    try
                    {
                        Ownername = gc.Between(bulkdata, "Owner Name", "Property Number").Replace(":", "");
                        Propertynumber = gc.Between(bulkdata, "Property Number", "Mailing Address").Replace(":", "");
                        Mailingaddress = gc.Between(bulkdata, "Mailing Address", "Premise Address").Replace(":", "");
                        Premiseaddress = gc.Between(bulkdata, "Premise Address", "Total Due").Replace(":", "");
                        TotalDue = GlobalClass.After(bulkdata, "Total Due").Replace(":", "").Replace("\r\n", "");

                        string CurrentTaxDetails = Ownername + "~" + Propertynumber + "~" + Mailingaddress + "~" + Premiseaddress + "~" + TotalDue + "~" + LegalDesc + "~" + TaxAuthority;
                        gc.insert_date(orderNumber, parcelNumber, 961, CurrentTaxDetails, 1, DateTime.Now);
                    }
                    catch { }
                    IWebElement Deliquenttax = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table/tbody"));
                    string Deliquent = Deliquenttax.Text;
                    if (Deliquent.Contains("For complete information on account"))
                    {
                        HttpContext.Current.Session["CharlesMD_Delinquent"] = "Yes";
                    }

                    //Tax Status
                    try
                    {
                        IWebElement taxstatus = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table[2]/tbody"));
                        IList<IWebElement> TRtaxstatus = taxstatus.FindElements(By.TagName("tr"));
                        IList<IWebElement> THtaxstatus = taxstatus.FindElements(By.TagName("th"));
                        IList<IWebElement> TDtaxstatus;
                        foreach (IWebElement text in TRtaxstatus)
                        {
                            TDtaxstatus = text.FindElements(By.TagName("td"));
                            THtaxstatus = text.FindElements(By.TagName("th"));

                            if (TDtaxstatus.Count != 0 && !text.Text.Contains("Amount Due"))
                            {
                                string TaxStatus = TDtaxstatus[0].Text + "~" + TDtaxstatus[1].Text + "~" + TDtaxstatus[2].Text + "~" + TDtaxstatus[3].Text + "~" + TDtaxstatus[4].Text + "~" + TDtaxstatus[5].Text + "~" + TDtaxstatus[6].Text + "~" + TDtaxstatus[7].Text;
                                gc.insert_date(orderNumber, parcelNumber, 962, TaxStatus, 1, DateTime.Now);
                            }
                            //if (TDtaxstatus.Count != 0 && TaxYearDetails.Count < 4 && !text.Text.Contains("Amount Due"))
                            //{
                            //    TaxYearDetails.Add(TDtaxstatus[0].Text.Trim() + "~" + TDtaxstatus[1].Text.Trim() + "~" + TDtaxstatus[2].Text.Trim());
                            //}
                        }
                    }
                    catch { }
                    // Dequent Tax
                    if (LegalDesc.Contains("1 ACANNAPOLIS"))
                    {

                        try
                        {
                            IWebElement taxstatus = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table[2]/tbody"));
                            IList<IWebElement> TRtaxstatus = taxstatus.FindElements(By.TagName("tr"));
                            IList<IWebElement> THtaxstatus = taxstatus.FindElements(By.TagName("th"));
                            IList<IWebElement> TDtaxstatus;
                            foreach (IWebElement text in TRtaxstatus)
                            {
                                TDtaxstatus = text.FindElements(By.TagName("td"));
                                THtaxstatus = text.FindElements(By.TagName("th"));
                                if (TDtaxstatus.Count != 0 && !text.Text.Contains("Amount Due") && TDtaxstatus[6].Text == "0.00 ")
                                {
                                    TaxYearDetails.Add(TDtaxstatus[0].Text.Trim() + "~" + TDtaxstatus[1].Text.Trim() + "~" + TDtaxstatus[2].Text.Trim());
                                }
                            }
                        }
                        catch { }


                    }
                    else
                    {
                        // Tax Status 
                        try
                        {
                            IWebElement taxstatus = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table[2]/tbody"));
                            IList<IWebElement> TRtaxstatus = taxstatus.FindElements(By.TagName("tr"));
                            IList<IWebElement> THtaxstatus = taxstatus.FindElements(By.TagName("th"));
                            IList<IWebElement> TDtaxstatus;
                            foreach (IWebElement text in TRtaxstatus)
                            {
                                TDtaxstatus = text.FindElements(By.TagName("td"));
                                THtaxstatus = text.FindElements(By.TagName("th"));
                                if (TDtaxstatus.Count != 0 && TaxYearDetails.Count < 3 && !text.Text.Contains("Amount Due"))
                                {
                                    TaxYearDetails.Add(TDtaxstatus[0].Text.Trim() + "~" + TDtaxstatus[1].Text.Trim() + "~" + TDtaxstatus[2].Text.Trim());
                                }
                            }
                        }
                        catch { }
                    }

                    // Payment History
                    try
                    {
                        if (!Deliquent.Contains("For complete information on account"))
                        {
                            driver.Navigate().GoToUrl("http://www.charlescounty.org/treas/taxes/acctinquiry/payments.jsp?txtPropertyNumber=" + parcelNumber.Trim() + "");
                            Thread.Sleep(2000);
                            //driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/div[2]/a[1]")).SendKeys(Keys.Enter);
                            gc.CreatePdf(orderNumber, parcelNumber, "Payment Histroy", driver, "MD", "Charles");
                            IWebElement payhistory = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table/tbody/tr/td/table[2]/tbody"));
                            IList<IWebElement> TRpayhistory = payhistory.FindElements(By.TagName("tr"));
                            IList<IWebElement> THpayhistory = payhistory.FindElements(By.TagName("th"));
                            IList<IWebElement> TDpayhistory;
                            foreach (IWebElement row2 in TRpayhistory)
                            {
                                TDpayhistory = row2.FindElements(By.TagName("td"));
                                THpayhistory = row2.FindElements(By.TagName("th"));

                                if (TDpayhistory.Count != 0 && !row2.Text.Contains("Adjustment"))
                                {
                                    string paymenthistory = TDpayhistory[0].Text + "~" + TDpayhistory[1].Text + "~" + TDpayhistory[2].Text;
                                    gc.insert_date(orderNumber, parcelNumber, 969, paymenthistory, 1, DateTime.Now);
                                }
                            }
                        }
                    }
                    catch { }



                    // Delinquent Tax Bill Details & Tax Year

                    int yearcount = 0;

                    foreach (string strTax in TaxYearDetails)
                    {
                        string taxinstallbill1 = "", taxinstallbill2 = "";
                        string[] taxsplit = strTax.Split('~');



                        try
                        {
                            driver.Navigate().GoToUrl("http://www.charlescounty.org/treas/taxes/acctinquiry/entityV2.jsp?txtPropertyNumber=" + parcelNumber.Trim() + "&txtTaxYear=" + taxsplit[0] + "&txtTaxPeriod=" + taxsplit[1] + "&txtTaxType=" + taxsplit[2] + "");
                            gc.CreatePdf(orderNumber, parcelNumber, "Tax Year Histroy" + taxsplit[0], driver, "MD", "Charles");
                            IWebElement Taxhistory = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table/tbody/tr[1]/td/table[2]/tbody"));
                            IList<IWebElement> TRTaxhistory = Taxhistory.FindElements(By.TagName("tr"));
                            IList<IWebElement> THTaxhistory = Taxhistory.FindElements(By.TagName("th"));
                            IList<IWebElement> TDTaxhistory;
                            foreach (IWebElement row2 in TRTaxhistory)
                            {
                                TDTaxhistory = row2.FindElements(By.TagName("td"));
                                THTaxhistory = row2.FindElements(By.TagName("th"));

                                if (TDTaxhistory.Count != 0 && !row2.Text.Contains("Entity"))
                                {
                                    string paymenthistory = taxsplit[0] + "~" + taxsplit[2] + "~" + TDTaxhistory[0].Text + "~" + TDTaxhistory[1].Text + "~" + TDTaxhistory[2].Text + "~" + TDTaxhistory[3].Text + "~" + TDTaxhistory[4].Text + "~" + TDTaxhistory[5].Text + "~" + TDTaxhistory[6].Text + "~" + "~";
                                    gc.insert_date(orderNumber, parcelNumber, 976, paymenthistory, 1, DateTime.Now);
                                }
                            }

                            IWebElement ITaxbill = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table/tbody/tr[2]/td/table/tbody"));
                            IList<IWebElement> ITRTaxbill = ITaxbill.FindElements(By.TagName("tr"));
                            IList<IWebElement> ITDTaxbill;
                            foreach (IWebElement row3 in ITRTaxbill)
                            {
                                ITDTaxbill = row3.FindElements(By.TagName("td"));
                                if (ITDTaxbill.Count != 0 && !row3.Text.Contains("Entity") && row3.Text.Contains("Installment #1 Amount Billed:"))
                                {
                                    taxinstallbill1 = ITDTaxbill[1].Text;
                                }
                                if (ITDTaxbill.Count != 0 && !row3.Text.Contains("Entity") && row3.Text.Contains("Installment #2 Amount Billed:"))
                                {
                                    taxinstallbill2 = ITDTaxbill[1].Text;
                                    gc.insert_date(orderNumber, parcelNumber, 976, taxsplit[0] + "~" + "~" + "~" + "~" + "~" + "~" + "~" + "~" + "~" + taxinstallbill1 + "~" + taxinstallbill2, 1, DateTime.Now);
                                }
                            }
                        }
                        catch { }
                    }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");

                    gc.insert_TakenTime(orderNumber, "MD", "Charles", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);


                    driver.Quit();
                    gc.mergpdf(orderNumber, "MD", "Charles");
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