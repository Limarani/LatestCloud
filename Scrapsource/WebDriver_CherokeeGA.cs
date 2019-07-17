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
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_CherokeeGA
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        private object driverIE;
        private int multicount;
        private string strMultiAddress;
        private string b;
        string mul = "";
        public string FTP_Cherokee(string houseno, string housedir, string sname, string sttype, string unitno, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "", AssessTakenTime = "", TaxTakentime = "", CityTaxtakentime = "";
            string TotaltakenTime = "";
            GlobalClass gc = new GlobalClass();

            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("http://taxassessor.cherokeega.com/taxnet/");
                    Thread.Sleep(4000);
                    if (searchType == "address")
                    {


                        driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearch_usercontrolRealEstateSearch_textboxHouse")).SendKeys(houseno);
                        driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearch_usercontrolRealEstateSearch_dropdownlistDir")).SendKeys(housedir);
                        driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearch_usercontrolRealEstateSearch_textboxStreetName")).SendKeys(sname);
                        driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearch_usercontrolRealEstateSearch_dropdownlistType")).SendKeys(sttype);
                        driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearch_usercontrolRealEstateSearch_textboxUnit")).SendKeys(unitno);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "GA", "Cherokee");
                        driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearch_usercontrolRealEstateSearch_buttonSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(4000);
                        int iRowsCount = driver.FindElements(By.XPath("//*[@id='ctl00_contentplaceholderRealEstateSearchResults_usercontrolRealEstateSearchResult_gridviewSearchResults']/tbody/tr")).Count;
                        int x = 0;
                        if (iRowsCount > 2)
                        {
                            //multi parcel
                            IWebElement tbmulti2 = driver.FindElement(By.XPath(" //*[@id='ctl00_contentplaceholderRealEstateSearchResults_usercontrolRealEstateSearchResult_gridviewSearchResults']/tbody"));
                            IList<IWebElement> TRmulti2 = tbmulti2.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDmulti2;
                            foreach (IWebElement row in TRmulti2)
                            {
                                TDmulti2 = row.FindElements(By.TagName("td"));
                                if (TDmulti2.Count != 0 && x <= 25)
                                {
                                    string multi1 = TDmulti2[2].Text + "~" + TDmulti2[0].Text + "~" + TDmulti2[4].Text;
                                    gc.insert_date(orderNumber, TDmulti2[8].Text, 389, multi1, 1, DateTime.Now);
                                    //  address~Account#~OwnerName
                                }
                                x++;
                            }

                            if (TRmulti2.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Cherokee_Multicount"] = "Maximum";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiParcel_Cherokee"] = "Yes";
                            }
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//*[@id='ctl00_contentplaceholderRealEstateSearchResults_usercontrolRealEstateSearchResult_gridviewSearchResults']/tbody/tr[2]/td[1]/a")).Click();

                                Thread.Sleep(3000);
                            }
                            catch { }

                        }
                    }
                    else if (searchType == "parcel")
                    {
                        string par1 = "", par2 = "", par3 = "", par4 = "";
                        var Parcel_no1 = parcelNumber.Split(' ');

                        string parcel_I = Parcel_no1[0];
                        string parcel_II = Parcel_no1[1];

                        int count = parcel_I.Count();
                        int count1 = parcel_II.Count();

                        if (count == 5)
                        {
                            par1 = parcel_I.Substring(0, 2);
                            par2 = parcel_I.Substring(3, 2);
                            driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearch_usercontrolRealEstateSearch_ctrlParcelNumber_txtDst")).SendKeys(par1);
                            driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearch_usercontrolRealEstateSearch_ctrlParcelNumber_txtMp")).SendKeys(par2);

                        }
                        if (count == 6)
                        {
                            par1 = parcel_I.Substring(0, 2);
                            par2 = parcel_I.Substring(3, 2);
                            par3 = parcel_I.Substring(5, 1);
                            driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearch_usercontrolRealEstateSearch_ctrlParcelNumber_txtDst")).SendKeys(par1);
                            driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearch_usercontrolRealEstateSearch_ctrlParcelNumber_txtMp")).SendKeys(par2);
                            driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearch_usercontrolRealEstateSearch_ctrlParcelNumber_txt1")).SendKeys(par3);

                        }
                        if (count == 7)
                        {
                            par1 = parcel_I.Substring(0, 2);
                            par2 = parcel_I.Substring(3, 2);
                            par3 = parcel_I.Substring(5, 1);
                            par4 = parcel_I.Substring(6, 1);
                            driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearch_usercontrolRealEstateSearch_ctrlParcelNumber_txtDst")).SendKeys(par1);
                            driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearch_usercontrolRealEstateSearch_ctrlParcelNumber_txtMp")).SendKeys(par2);
                            driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearch_usercontrolRealEstateSearch_ctrlParcelNumber_txt1")).SendKeys(par3);
                            driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearch_usercontrolRealEstateSearch_ctrlParcelNumber_txt2")).SendKeys(par4);

                        }
                        if (count1 == 1 || count1 == 2 || count1 == 3)
                        {
                            driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearch_usercontrolRealEstateSearch_ctrlParcelNumber_txtPar")).SendKeys(parcel_II);
                        }
                        if (count1 == 4 || count1 == 5 || count1 == 6)
                        {

                            string parcelno1 = parcel_II.Substring(0, 3);
                            int b = parcelno1.Length;
                            int a = count1 - b;
                            string parcelno2 = parcel_II.Substring(3, a);
                            driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearch_usercontrolRealEstateSearch_ctrlParcelNumber_txtPar")).SendKeys(parcelno1);

                            driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearch_usercontrolRealEstateSearch_ctrlParcelNumber_txtSpl")).SendKeys(parcelno2);
                        }
                        gc.CreatePdf_WOP(orderNumber, "Parcel search", driver, "GA", "Cherokee");

                        driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearch_usercontrolRealEstateSearch_buttonSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(4000);
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='ctl00_contentplaceholderRealEstateSearchResults_usercontrolRealEstateSearchResult_gridviewSearchResults']/tbody/tr[2]/td[1]/a")).Click();
                            Thread.Sleep(3000);
                        }
                        catch { }
                    }
                    if (searchType == "block")
                    {
                        driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearch_usercontrolRealEstateSearch_textboxAccount")).SendKeys(unitno);
                        gc.CreatePdf_WOP(orderNumber, "account search", driver, "GA", "Cherokee");
                        driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearch_usercontrolRealEstateSearch_buttonSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(4000);
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='ctl00_contentplaceholderRealEstateSearchResults_usercontrolRealEstateSearchResult_gridviewSearchResults']/tbody/tr[2]/td[1]/a")).Click();
                            Thread.Sleep(3000);
                        }
                        catch { }
                    }
                    if (searchType == "pin")
                    {
                        driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearch_usercontrolRealEstateSearch_ctrlAlternateIdentifier_txtPIN")).SendKeys(sttype);
                        gc.CreatePdf_WOP(orderNumber, "pin search", driver, "GA", "Cherokee");
                        driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearch_usercontrolRealEstateSearch_buttonSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(4000);
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='ctl00_contentplaceholderRealEstateSearchResults_usercontrolRealEstateSearchResult_gridviewSearchResults']/tbody/tr[2]/td[1]/a")).Click();
                            Thread.Sleep(3000);
                        }
                        catch { }
                    }

                    else if (searchType == "ownername")
                    {
                        string lastname = "", firstname = "";
                        string s = ownername;
                        string[] words = s.Split(' ');
                        if (words.Count() == 1)
                        {
                            lastname = words[0];
                            driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearch_usercontrolRealEstateSearch_textboxOwnerLastName")).SendKeys(lastname);

                        }
                        else
                        {


                            lastname = words[0];
                            firstname = words[1];

                            driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearch_usercontrolRealEstateSearch_textboxOwnerLastName")).SendKeys(lastname);
                            driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearch_usercontrolRealEstateSearch_textboxOwnerFirstName")).SendKeys(firstname);
                        }


                        gc.CreatePdf_WOP(orderNumber, "Owner search", driver, "GA", "Cherokee");
                        driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearch_usercontrolRealEstateSearch_buttonSearch")).SendKeys(Keys.Enter);

                        Thread.Sleep(4000);
                        int iRowsCount = driver.FindElements(By.XPath("//*[@id='ctl00_contentplaceholderRealEstateSearchResults_usercontrolRealEstateSearchResult_gridviewSearchResults']/tbody/tr")).Count;
                        int z = 0;
                        if (iRowsCount > 2)
                        {
                            //multi parcel
                            IWebElement tbmulti2 = driver.FindElement(By.XPath(" //*[@id='ctl00_contentplaceholderRealEstateSearchResults_usercontrolRealEstateSearchResult_gridviewSearchResults']/tbody"));
                            IList<IWebElement> TRmulti2 = tbmulti2.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDmulti2;
                            foreach (IWebElement row in TRmulti2)
                            {
                                TDmulti2 = row.FindElements(By.TagName("td"));
                                if (TDmulti2.Count != 0 && z <= 25)
                                {

                                    string multi1 = TDmulti2[2].Text + "~" + TDmulti2[0].Text + "~" + TDmulti2[4].Text;
                                    gc.insert_date(orderNumber, TDmulti2[8].Text, 389, multi1, 1, DateTime.Now);
                                    //  address~Account#~OwnerName
                                }
                                z++;
                            }

                            if (TRmulti2.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Cherokee_Multicount"] = "Maximum";
                                return "Maximum";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiParcel_Cherokee"] = "Yes";
                            }
                            driver.Quit();
                            return "MultiParcel";

                        }
                        else
                        {
                            try
                            {

                                driver.FindElement(By.XPath("//*[@id='ctl00_contentplaceholderRealEstateSearchResults_usercontrolRealEstateSearchResult_gridviewSearchResults']/tbody/tr[2]/td[1]/a")).Click();
                                Thread.Sleep(3000);
                            }
                            catch { }
                        }
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearchResults_usercontrolRealEstateSearchResult_label1"));
                        if(INodata.Text.Contains("No records found"))
                        {
                            HttpContext.Current.Session["Nodata_CherokeeGA"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    //property details
                    string ParcelID = "", PIN_Number = "", AccountNumber = "", OwnerName = "", Exemptions = "", PropertyAddress = "", MailingAddress = "", PropertyType = "", YearBuilt = "", LegalDescription = "", BuildingValue = "", OutbuildingValue = "", LandValue = "", TotalParcelValue = "", DeferredValue = "", TaxableValue = "";

                    ParcelID = driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearchSummary_usercontrolRealEstateParcelSummaryInfo_LabelParcelNumber")).Text;

                    string parcelId1, ParcelID2;

                    var Parcel_no = ParcelID.Split(' ');

                    parcelId1 = Parcel_no[0];
                    ParcelID2 = Parcel_no[1];
                    gc.CreatePdf(orderNumber, ParcelID, "Assessment details", driver, "GA", "Cherokee");
                    PIN_Number = driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearchSummary_usercontrolRealEstateParcelSummaryInfo_lblAltParcelNumberValue")).Text;

                    AccountNumber = driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearchSummary_usercontrolRealEstateParcelSummaryInfo_labelAccountNumber")).Text;

                    OwnerName = driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearchSummary_usercontrolRealEstateParcelSummaryInfo_labelOwnerName")).Text;

                    Exemptions = driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearchSummary_usercontrolRealEstateParcelSummaryInfo_lblExemptionsValue")).Text;



                    LegalDescription = driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearchSummary_usercontrolRealEstateParcelSummaryInfo_labelLegalDescription")).Text;


                    BuildingValue = driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearchSummary_usercontrolRealEstateParcelSummaryInfo_labelBuilding")).Text;

                    OutbuildingValue = driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearchSummary_usercontrolRealEstateParcelSummaryInfo_labelOutbuilding")).Text;

                    LandValue = driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearchSummary_usercontrolRealEstateParcelSummaryInfo_labelLand")).Text;

                    TotalParcelValue = driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearchSummary_usercontrolRealEstateParcelSummaryInfo_labelParcelValueTotal")).Text;

                    DeferredValue = driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearchSummary_usercontrolRealEstateParcelSummaryInfo_labelDefferedValue")).Text;

                    TaxableValue = driver.FindElement(By.Id("ctl00_contentplaceholderRealEstateSearchSummary_usercontrolRealEstateParcelSummaryInfo_labelTaxableValue")).Text;


                    //building click      
                    try
                    {
                        driver.FindElement(By.Id("__tab_ctl00_contentplaceholderRealEstateWorkplace_tabcontainerWorkSpace_tabpanelBuilding")).Click();
                        Thread.Sleep(6000);
                        gc.CreatePdf(orderNumber, ParcelID, "Building details", driver, "GA", "Cherokee");
                        PropertyAddress = driver.FindElement(By.XPath("//*[@id='ctl00_contentplaceholderRealEstateWorkplace_tabcontainerWorkSpace_tabpanelBuilding_usercontrolRealEstateParcelBuildingData_gridviewParcelBuilding']/tbody/tr[2]/td[9]")).Text.Trim().Replace("\r\n", "");


                        PropertyType = driver.FindElement(By.XPath("//*[@id='ctl00_contentplaceholderRealEstateWorkplace_tabcontainerWorkSpace_tabpanelBuilding_usercontrolRealEstateParcelBuildingData_gridviewParcelBldgUseModel']/tbody/tr[2]/td[1]")).Text;

                        YearBuilt = driver.FindElement(By.XPath("//*[@id='ctl00_contentplaceholderRealEstateWorkplace_tabcontainerWorkSpace_tabpanelBuilding_usercontrolRealEstateParcelBuildingData_gridviewParcelBuilding']/tbody/tr[2]/td[2]")).Text;
                    }
                    catch { }
                    //owner click           

                    driver.FindElement(By.Id("__tab_ctl00_contentplaceholderRealEstateWorkplace_tabcontainerWorkSpace_tabpanelOwners")).Click();
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, ParcelID, "Owner details", driver, "GA", "Cherokee");
                    string Mailing1 = driver.FindElement(By.XPath(" //*[@id='ctl00_contentplaceholderRealEstateWorkplace_tabcontainerWorkSpace_tabpanelOwners']/table/tbody/tr[4]")).Text;
                    string Mailing2 = driver.FindElement(By.XPath(" //*[@id='ctl00_contentplaceholderRealEstateWorkplace_tabcontainerWorkSpace_tabpanelOwners']/table/tbody/tr[5]")).Text;
                    MailingAddress = Mailing1 + " " + Mailing2;
                    //       PIN Number~AccountNumber~Owner Name~Exemptions~Property Address~Mailing Address~Property Type~Year Built~Legal Description

                    //            Building Value~Outbuilding Value~Land Value~Total Parcel Value~Deferred Value~Taxable Value


                    string property_details = PIN_Number + "~" + AccountNumber + "~" + OwnerName + "~" + Exemptions + "~" + PropertyAddress + "~" + MailingAddress + "~" + PropertyType + "~" + YearBuilt + "~" + LegalDescription;
                    gc.insert_date(orderNumber, ParcelID, 381, property_details, 1, DateTime.Now);

                    string assessment_details = BuildingValue + "~" + OutbuildingValue + "~" + LandValue + "~" + TotalParcelValue + "~" + DeferredValue + "~" + TaxableValue;
                    gc.insert_date(orderNumber, ParcelID, 382, assessment_details, 1, DateTime.Now);

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    //Tax details

                    driver.Navigate().GoToUrl("http://legacy.cherokeega.com/apps/TaxBills/taxbillsearch.cfm");
                    Thread.Sleep(4000);
                    gc.CreatePdf(orderNumber, ParcelID, "Tax", driver, "GA", "Cherokee");
                    driver.FindElement(By.Id("map")).Clear();
                    driver.FindElement(By.Id("parcel")).Clear();
                    driver.FindElement(By.Id("map")).SendKeys(parcelId1);
                    driver.FindElement(By.Id("parcel")).SendKeys(ParcelID2);

                    string yearcurrent = driver.FindElement(By.XPath("//*[@id='nested']/div[1]/strong")).Text;

                    var SerachTax = driver.FindElement(By.XPath("//*[@id='mapsearch']/table/tbody/tr[6]/td/select"));
                    var selectElement1 = new SelectElement(SerachTax);
                    selectElement1.SelectByText(yearcurrent);
                    gc.CreatePdf(orderNumber, ParcelID, "Tax", driver, "GA", "Cherokee");
                    driver.FindElement(By.XPath("//*[@id='mapsearch']/table/tbody/tr[7]/td/input")).SendKeys(Keys.Enter);
                    Thread.Sleep(4000);

                    IWebElement multitableElement4 = driver.FindElement(By.XPath("//*[@id='main']/table/tbody"));
                    IList<IWebElement> multitableRow4 = multitableElement4.FindElements(By.TagName("tr"));

                    IList<IWebElement> multirowTD4;
                    int l = 1;
                    foreach (IWebElement row in multitableRow4)
                    {
                        if (row.Text.Contains(ParcelID))
                        {

                            multirowTD4 = row.FindElements(By.TagName("td"));

                            driver.FindElement(By.XPath("//*[@id='main']/table/tbody/tr[" + l + "]/td[1]/strong/span")).Click();
                            break;

                        }
                        l++;

                    }

                    //tax distribution table
                    gc.CreatePdf(orderNumber, ParcelID, "Tax details", driver, "GA", "Cherokee");
                    IWebElement multitableElement2 = driver.FindElement(By.XPath("//*[@id='main']/table[2]/tbody/tr[4]/td/table/tbody"));
                    IList<IWebElement> multitableRow2 = multitableElement2.FindElements(By.TagName("tr"));
                    int rowcount = multitableRow2.Count();
                    IList<IWebElement> multirowTD2;
                    string ok = "";
                    int i = 0;
                    foreach (IWebElement row in multitableRow2)
                    {
                        if (i > 2)
                        {
                            multirowTD2 = row.FindElements(By.TagName("td"));
                            if (multirowTD2.Count == 9)
                            {
                                string tax_distri = multirowTD2[0].Text.Trim() + "~" + multirowTD2[1].Text.Trim() + "~" + multirowTD2[2].Text.Trim() + "~" + multirowTD2[3].Text.Trim() + "~" + multirowTD2[4].Text.Trim() + "~" + multirowTD2[5].Text.Trim() + "~" + multirowTD2[6].Text.Trim() + "~" + multirowTD2[7].Text.Trim() + "~" + multirowTD2[8].Text.Trim();
                                gc.insert_date(orderNumber, ParcelID, 383, tax_distri, 1, DateTime.Now);

                            }
                            if (multirowTD2.Count == 5)
                            {
                                string tax_distri1 = multirowTD2[0].Text.Trim() + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + multirowTD2[1].Text.Trim() + "~" + multirowTD2[2].Text.Trim() + "~" + multirowTD2[3].Text.Trim() + "~" + multirowTD2[4].Text.Trim();
                                gc.insert_date(orderNumber, ParcelID, 383, tax_distri1, 1, DateTime.Now);

                            }
                        }

                        i++;
                    }

                    //Authority~Adjusted FMV~Assessed Value~Exemptions~Taxable Value~Millage Rate~Gross Tax~Credit~Net Tax

                    //taxinfo

                    //Property Address~District~Bill Number~Due Date~Total Due~Good Through Date~Exemptions~Tax Amount~Penalty~Interest~Other Fees~Last Paid Amount~Prior Year Taxes~Total Due~Paid Date~Taxing Authority
                    string Property_Address = "", District = "", BillNumber = "", DueDate = "", TotalDue = "", GoodThroughDate = "", Exemptionss = "", TaxAmount = "", Penalty = "", Interest = "", OtherFees = "", LastPaidAmount = "", PriorYearTaxes = "", TotalDues = "", PaidDate = "", TaxingAuthority = "";

                    string taxpayername = driver.FindElement(By.XPath("//*[@id='main']/table[2]/tbody/tr[3]/td[2]/table/tbody/tr[2]/td[2]")).Text;
                    Property_Address = driver.FindElement(By.XPath("//*[@id='main']/table[2]/tbody/tr[3]/td[2]/table/tbody/tr[5]/td[2]")).Text;

                    District = driver.FindElement(By.XPath(" //*[@id='main']/table[2]/tbody/tr[3]/td[2]/table/tbody/tr[7]/td[2]")).Text;

                    BillNumber = driver.FindElement(By.XPath("//*[@id='main']/table[2]/tbody/tr[1]/td[2]/table[1]/tbody/tr[2]/td[1]")).Text;

                    DueDate = driver.FindElement(By.XPath("//*[@id='main']/table[2]/tbody/tr[1]/td[2]/table[1]/tbody/tr[2]/td[2]")).Text;

                    TotalDue = driver.FindElement(By.XPath("//*[@id='main']/table[2]/tbody/tr[1]/td[2]/table[1]/tbody/tr[2]/td[3]")).Text;

                    GoodThroughDate = driver.FindElement(By.XPath("//*[@id='main']/table[2]/tbody/tr[4]/td/table/tbody/tr[2]/td[8]")).Text;

                    Exemptionss = driver.FindElement(By.XPath(" //*[@id='main']/table[2]/tbody/tr[4]/td/table/tbody/tr[2]/td[9]")).Text;

                    TaxAmount = driver.FindElement(By.XPath("//*[@id='main']/table[2]/tbody/tr[5]/td/table/tbody/tr[1]/td[3]")).Text;

                    Penalty = driver.FindElement(By.XPath("//*[@id='main']/table[2]/tbody/tr[5]/td/table/tbody/tr[3]/td[2]")).Text;

                    Interest = driver.FindElement(By.XPath("  //*[@id='main']/table[2]/tbody/tr[5]/td/table/tbody/tr[4]/td[2]")).Text;

                    OtherFees = driver.FindElement(By.XPath(" //*[@id='main']/table[2]/tbody/tr[5]/td/table/tbody/tr[5]/td[2]")).Text;

                    LastPaidAmount = driver.FindElement(By.XPath(" //*[@id='main']/table[2]/tbody/tr[5]/td/table/tbody/tr[6]/td[2]")).Text;

                    PriorYearTaxes = driver.FindElement(By.XPath("//*[@id='main']/table[2]/tbody/tr[5]/td/table/tbody/tr[7]/td[2]")).Text;

                    TotalDues = driver.FindElement(By.XPath(" //*[@id='main']/table[2]/tbody/tr[5]/td/table/tbody/tr[8]/td[2]")).Text;


                    PaidDate = driver.FindElement(By.XPath(" //*[@id='main']/table[2]/tbody/tr[5]/td/table/tbody/tr[9]/td[2]")).Text;

                    TaxingAuthority = driver.FindElement(By.XPath("//*[@id='main']/table[2]/tbody/tr[1]/td[1]/table/tbody")).Text.Trim().Replace("\r\n", "");
                    TaxingAuthority = gc.Between(TaxingAuthority, "Property Tax Statement", "Make Check or Money Order Payable to:").Trim();
                    string tax_info = taxpayername + "~" + Property_Address + "~" + District + "~" + BillNumber + "~" + DueDate + "~" + TotalDue + "~" + GoodThroughDate + "~" + Exemptionss + "~" + TaxAmount + "~" + Penalty + "~" + Interest + "~" + OtherFees + "~" + LastPaidAmount + "~" + PriorYearTaxes + "~" + TotalDues + "~" + PaidDate + "~" + TaxingAuthority;
                    gc.insert_date(orderNumber, ParcelID, 384, tax_info, 1, DateTime.Now);

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    //City of Woodstock Tax


                    driver.Navigate().GoToUrl("https://woodstock.surecourt.com/epay/epay/search.aspx");
                    Thread.Sleep(4000);
                    string Balance_Due = "", Amount_Pay = "", Convenience_Fee = "", Payment_Amount = "";
                    try
                    {

                        driver.FindElement(By.Id("edtSearchSpec_I")).SendKeys(ParcelID);

                        //Thread.Sleep(4000);
                        IWebElement ISpan = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_ASPxRoundPanel1_btnSearch_I']"));
                        IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                        js.ExecuteScript("arguments[0].click();", ISpan);
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, ParcelID, "woodstock", driver, "GA", "Cherokee");
                        driver.FindElement(By.XPath("//*[@id='grdSearch_DXCBtn0_CD']/span")).Click();
                        Thread.Sleep(7000);
                        gc.CreatePdf(orderNumber, ParcelID, "woodstock details", driver, "GA", "Cherokee");
                        //Tax History Details Table:


                        //     DOE~Tax Year~Bill Number~Due Date~Tax Amount~Balance Due

                        IWebElement multitableElement21 = driver.FindElement(By.XPath("//*[@id='grdBill_DXMainTable']/tbody"));
                        IList<IWebElement> multitableRow21 = multitableElement21.FindElements(By.TagName("tr"));
                        IList<IWebElement> multirowTD21;
                        int j = 0;
                        foreach (IWebElement row in multitableRow21)
                        {
                            if (j >= 7)
                            {
                                multirowTD21 = row.FindElements(By.TagName("td"));
                                if (multirowTD21.Count != 0)
                                {
                                    string tax_history = multirowTD21[0].Text.Trim() + "~" + multirowTD21[1].Text.Trim() + "~" + multirowTD21[2].Text.Trim() + "~" + multirowTD21[3].Text.Trim() + "~" + multirowTD21[4].Text.Trim() + "~" + multirowTD21[5].Text.Trim();
                                    gc.insert_date(orderNumber, ParcelID, 385, tax_history, 1, DateTime.Now);

                                }

                            }
                            j++;
                        }

                        //Tax information Table:
                        // Balance Due~Amount to Pay~Convenience Fee~Payment Amount



                        Balance_Due = driver.FindElement(By.Id("lblBalanceDue")).Text;

                        Amount_Pay = driver.FindElement(By.Id("lblBalanceDue")).Text;

                        Convenience_Fee = driver.FindElement(By.Id("lblConvenienceFee")).Text;

                        Payment_Amount = driver.FindElement(By.Id("lblPaymentAmount")).Text;

                        string tax_info_city = Balance_Due + "~" + Amount_Pay + "~" + Convenience_Fee + "~" + Payment_Amount;
                        gc.insert_date(orderNumber, ParcelID, 386, tax_info_city, 1, DateTime.Now);
                    }
                    catch { }
                    //City of Holly Springs Tax
                    try
                    {
                        driver.Navigate().GoToUrl("https://hollyspringsepay.com/proptax/epay/search.aspx");
                        Thread.Sleep(4000);


                        driver.FindElement(By.Id("edtSearchSpec_I")).SendKeys(ParcelID);


                        IWebElement ISpan1 = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_ASPxRoundPanel1_btnSearch_I']"));
                        IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                        js1.ExecuteScript("arguments[0].click();", ISpan1);
                        Thread.Sleep(6000);
                        gc.CreatePdf(orderNumber, ParcelID, "Holly Springs", driver, "GA", "Cherokee");

                        driver.FindElement(By.XPath("//*[@id='grdSearch_DXCBtn0_CD']/span")).Click();
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, ParcelID, "Holly Springs tax", driver, "GA", "Cherokee");
                        //   Tax History Details Table:


                        //     DOE~Tax Year~Bill Number~Due Date~Tax Amount~Balance Due

                        IWebElement multitableElement22 = driver.FindElement(By.XPath("//*[@id='grdBill_DXMainTable']/tbody"));
                        IList<IWebElement> multitableRow22 = multitableElement22.FindElements(By.TagName("tr"));

                        IList<IWebElement> multirowTD22;
                        int k = 0;
                        foreach (IWebElement row in multitableRow22)
                        {

                            if (k >= 7)
                            {
                                if (!row.Text.Contains("DOE"))
                                {
                                    multirowTD22 = row.FindElements(By.TagName("td"));
                                    if (multirowTD22.Count != 1)
                                    {
                                        string tax_historyC = multirowTD22[0].Text.Trim() + "~" + multirowTD22[1].Text.Trim() + "~" + multirowTD22[2].Text.Trim() + "~" + multirowTD22[3].Text.Trim() + "~" + multirowTD22[4].Text.Trim() + "~" + multirowTD22[5].Text.Trim();
                                        gc.insert_date(orderNumber, ParcelID, 387, tax_historyC, 1, DateTime.Now);

                                    }
                                }
                            }
                            k++;

                        }

                        //Tax information Table:
                        // Balance Due~Amount to Pay~Convenience Fee~Payment Amount
                        Balance_Due = driver.FindElement(By.Id("lblBalanceDue")).Text;

                        Amount_Pay = driver.FindElement(By.Id("lblBalanceDue")).Text;

                        Convenience_Fee = driver.FindElement(By.Id("lblConvenienceFee")).Text;

                        Payment_Amount = driver.FindElement(By.Id("lblPaymentAmount")).Text;

                        string tax_info_city1 = Balance_Due + "~" + Amount_Pay + "~" + Convenience_Fee + "~" + Payment_Amount;
                        gc.insert_date(orderNumber, ParcelID, 388, tax_info_city1, 1, DateTime.Now);
                    }
                    catch { }
                    CitytaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "GA", "Cherokee", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "GA", "Cherokee");
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