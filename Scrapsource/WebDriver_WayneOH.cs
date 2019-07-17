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

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_WayneOH
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();

        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_Wayne(string houseno, string sname, string streettype, string housedir, string unitno, string parcelNumber, string searchType, string ownername, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                //  driver = new ChromeDriver();

                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("http://www.waynecountyauditor.org/Search.aspx");
                    Thread.Sleep(4000);
                    if (searchType == "titleflex")
                    {
                        string titleaddress = houseno + " " + sname + " " + housedir + " " + unitno;

                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "OH", "Wayne");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_WayneOH"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        try
                        {
                            driver.FindElement(By.Id("ContentPlaceHolder1_btnDisclaimerAccept")).Click();
                            Thread.Sleep(3000);
                        }
                        catch { }
                        try
                        {
                            driver.FindElement(By.LinkText("Address")).Click();
                            Thread.Sleep(1000);
                        }
                        catch { }
                        driver.FindElement(By.Id("ContentPlaceHolder1_Address_tbAddressNumber")).SendKeys(houseno);
                        driver.FindElement(By.Id("ContentPlaceHolder1_Address_tbAddressDirection")).SendKeys(housedir);
                        driver.FindElement(By.Id("ContentPlaceHolder1_Address_tbAddressStreet")).SendKeys(sname);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "OH", "Wayne");
                        driver.FindElement(By.Id("ContentPlaceHolder1_Address_btnSearchAddress")).SendKeys(Keys.Enter);
                        Thread.Sleep(4000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search result", driver, "OH", "Wayne");
                        try
                        {
                            int Max = 0;
                            string strowner = "", strAddress = "", strCity = "";
                            IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_gvSearchResults']/tbody"));
                            IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> multiTD;
                            foreach (IWebElement multi in multiRow)
                            {
                                multiTD = multi.FindElements(By.TagName("td"));
                                if (multiTD.Count != 0 && multiRow.Count < 3)
                                {
                                    IWebElement IsearchClick = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_gvSearchResults']/tbody/tr[2]/td[1]/a"));
                                    IsearchClick.Click();
                                    Thread.Sleep(2000);
                                    Max++;
                                    break;

                                }
                                if (multiTD.Count != 0 && multiRow.Count > 25)
                                {
                                    HttpContext.Current.Session["multiparcel_WayneOH_Maximum"] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }
                                if (multiTD.Count != 0 && multiRow.Count > 2 && multiRow.Count <= 25)
                                {
                                    strowner = multiTD[1].Text;
                                    strAddress = multiTD[2].Text;

                                    string multidetails = strowner + "~" + strAddress;
                                    gc.insert_date(orderNumber, multiTD[0].Text, 1431, multidetails, 1, DateTime.Now);
                                    Max++;
                                }
                            }
                            if (Max > 1 && Max < 26)
                            {
                                HttpContext.Current.Session["multiparcel_WayneOH"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Max == 0)
                            {
                                HttpContext.Current.Session["Nodata_WayneOH"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    else if (searchType == "ownername")
                    {
                        try
                        {
                            driver.FindElement(By.Id("ContentPlaceHolder1_btnDisclaimerAccept")).Click();
                            Thread.Sleep(1000);
                        }
                        catch { }
                        try
                        {
                            driver.FindElement(By.LinkText("Owner")).Click();
                            Thread.Sleep(1000);
                        }
                        catch { }
                        string Lastname = "", Firstname = "";
                        try
                        {
                            var ownersplit = ownername.Trim().Split(' ');
                            if(ownersplit.Length == 2)
                            {
                                Lastname = ownersplit[0];
                                Firstname = ownersplit[1];
                                driver.FindElement(By.Id("ContentPlaceHolder1_Owner_tbOwnerLastName")).SendKeys(Lastname);
                                driver.FindElement(By.Id("ContentPlaceHolder1_Owner_tbOwnerFirstName")).SendKeys(Firstname);
                            }
                            else
                            {
                                driver.FindElement(By.Id("ContentPlaceHolder1_Owner_tbOwnerLastName")).SendKeys(ownername.Trim());
                            }
                            ownername = Lastname + " " + Firstname;
                        }
                        catch { }

                        gc.CreatePdf_WOP(orderNumber, "OwnerName Search", driver, "OH", "Wayne");
                        driver.FindElement(By.Id("ContentPlaceHolder1_Owner_btnSearchOwner")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "OwnerName Search Results", driver, "OH", "Wayne");
                        try
                        {
                            int Max = 0;
                            string strowner = "", strAddress = "", strCity = "";
                            IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_gvSearchResults']/tbody"));
                            IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> multiTD;
                            foreach (IWebElement multi in multiRow)
                            {
                                multiTD = multi.FindElements(By.TagName("td"));
                                if (multiTD.Count != 0 && multiRow.Count < 3)
                                {
                                    IWebElement IsearchClick = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_gvSearchResults']/tbody/tr[2]/td[1]/a"));
                                    IsearchClick.Click();
                                    Thread.Sleep(2000);
                                    Max++;
                                    break;
                                }
                                if (multiTD.Count != 0 && multiRow.Count > 25)
                                {
                                    HttpContext.Current.Session["multiparcel_WayneOH_Maximum"] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }
                                if (multiTD.Count != 0 && multiRow.Count > 2 && multiRow.Count <= 25)
                                {
                                    strowner = multiTD[1].Text;
                                    strAddress = multiTD[2].Text;

                                    string multidetails = strowner + "~" + strAddress;
                                    gc.insert_date(orderNumber, multiTD[0].Text, 1431, multidetails, 1, DateTime.Now);
                                    Max++;
                                }
                            }
                            if (Max > 1 && Max < 26)
                            {
                                HttpContext.Current.Session["multiparcel_WayneOH"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Max == 0)
                            {
                                HttpContext.Current.Session["Nodata_WayneOH"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }


                    else if (searchType == "parcel")
                    {
                        try
                        {
                            driver.FindElement(By.Id("ContentPlaceHolder1_btnDisclaimerAccept")).Click();
                            Thread.Sleep(1000);
                        }
                        catch { }
                        try
                        {
                            driver.FindElement(By.LinkText("Parcel")).Click();
                            Thread.Sleep(1000);
                        }
                        catch { }
                        driver.FindElement(By.Id("ContentPlaceHolder1_Parcel_tbParcelNumber")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "parcel search", driver, "OH", "Wayne");
                        driver.FindElement(By.Id("ContentPlaceHolder1_Parcel_btnSearchParcel")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcelNumber, "parcel search Result", driver, "OH", "Wayne");
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_gvSearchResults']/tbody/tr[2]/td[1]/a")).SendKeys(Keys.Enter);
                            Thread.Sleep(3000);
                        }
                        catch { }
                    }

                    try
                    {
                        //No Data Found
                        string nodata = driver.FindElement(By.Id("ContentPlaceHolder1_lblNumberOfResults")).Text;
                        if (nodata.Contains("No results"))
                        {
                            HttpContext.Current.Session["Nodata_WayneOH"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    //property details

                    string owner_name = "", PropertyAddress = "", MailingAddress = "", City = "", Township = "", SchoolDistrict = "", LegalAcres = "";
                    string LegalDesc = "", Landuse = "", HomesteadReduction = "", YearBuilt = "";
                    string bulkdata = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Base_fvDataProfile']/tbody/tr/td/table/tbody")).Text;

                    parcelNumber = gc.Between(bulkdata, "Parcel:", "Owner").Trim();
                    owner_name = gc.Between(bulkdata, "Owner:", "Address:").Trim();
                    PropertyAddress = GlobalClass.After(bulkdata, "Address:").Trim();
                    gc.CreatePdf(orderNumber, parcelNumber, "Property Details", driver, "OH", "Wayne");
                    string bulkdata1 = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Base_fvDataMailingAddress']/tbody/tr/td/table/tbody")).Text;
                    MailingAddress = GlobalClass.After(bulkdata1, "Address:").Replace("City State Zip:", "").Replace("\r\n", "").Trim();

                    string bulkdata2 = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Base_fvDataGeographic']/tbody/tr/td/table/tbody")).Text;

                    City = gc.Between(bulkdata2, "City:", "Township:").Trim();
                    Township = gc.Between(bulkdata2, "Township:", "School District:").Trim();
                    SchoolDistrict = GlobalClass.After(bulkdata2, "School District:").Trim();

                    string bulkdata3 = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Base_fvDataLegal']/tbody/tr/td/table/tbody")).Text;

                    LegalAcres = gc.Between(bulkdata3, "Legal Acres:", "Homestead Reduction").Trim();
                    LegalDesc = gc.Between(bulkdata3, "Legal Description:", "2.5% Reduction").Trim();
                    Landuse = gc.Between(bulkdata3, "Land Use:", "Foreclosure:").Trim();
                    HomesteadReduction = gc.Between(bulkdata3, "Homestead Reduction:", "Legal Description:").Trim();
                    try
                    {
                        driver.FindElement(By.LinkText("Improvements")).Click();
                        Thread.Sleep(1000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Improvements", driver, "OH", "Wayne");

                    }
                    catch { }

                    try
                    {
                        driver.FindElement(By.LinkText("Residential")).Click();
                        Thread.Sleep(1000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Residential", driver, "OH", "Wayne");
                        YearBuilt = driver.FindElement(By.Id("ContentPlaceHolder1_Residential_fvDataResidential_YearBuiltLabel")).Text;
                    }
                    catch { }
                    string propertydetails = owner_name + "~" + PropertyAddress + "~" + MailingAddress + "~" + City + "~" + Township + "~" + SchoolDistrict + "~" + LegalAcres + "~" + LegalDesc + "~" + Landuse + "~" + HomesteadReduction + "~" + YearBuilt;
                    gc.insert_date(orderNumber, parcelNumber, 1430, propertydetails, 1, DateTime.Now);

                    try
                    {
                        driver.FindElement(By.LinkText("Land")).Click();
                        Thread.Sleep(1000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Land", driver, "OH", "Wayne");
                    }
                    catch { }
                    try
                    {
                        driver.FindElement(By.LinkText("Sales")).Click();
                        Thread.Sleep(1000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Sales", driver, "OH", "Wayne");
                    }
                    catch { }

                    // Assessment Details
                    try
                    {
                        driver.FindElement(By.LinkText("Valuation")).Click();
                        Thread.Sleep(1000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Valuation", driver, "OH", "Wayne");
                    }
                    catch { }

                    //string AppraisedLandValue = "", AssessedLandValue = "", AppraisedBuildingValue = "", AssessedBuildingValue = "";
                    //string TotalAppraisedValue = "", TotalAssessedValue = "", CAUV_Value;

                    //AppraisedLandValue = driver.FindElement(By.Id("ContentPlaceHolder1_Valuation_fvDataValuation_AppraisedLandValueLabel")).Text;
                    //AssessedLandValue = driver.FindElement(By.Id("ContentPlaceHolder1_Valuation_fvDataValuation_AssessedLandValueLabel")).Text;
                    //AppraisedBuildingValue = driver.FindElement(By.Id("ContentPlaceHolder1_Valuation_fvDataValuation_AppraisedImprovementsValueLabel")).Text;
                    //AssessedBuildingValue = driver.FindElement(By.Id("ContentPlaceHolder1_Valuation_fvDataValuation_AssessedImprovementsValueLabel")).Text;
                    //TotalAppraisedValue = driver.FindElement(By.Id("ContentPlaceHolder1_Valuation_fvDataValuation_TotalAppraisedValueLabel")).Text;
                    //TotalAssessedValue = driver.FindElement(By.Id("ContentPlaceHolder1_Valuation_fvDataValuation_TotalAssessedValueLabel")).Text;
                    //CAUV_Value = driver.FindElement(By.Id("ContentPlaceHolder1_Valuation_fvDataValuation_AppraisedCAUVValueLabel")).Text;

                    //string Assessmentdetails = AppraisedLandValue + "~" + AppraisedBuildingValue + "~" + AssessedLandValue + "~" + AssessedBuildingValue + "~" + TotalAppraisedValue + "~" + TotalAssessedValue + "~" + CAUV_Value;
                    //gc.insert_date(orderNumber, parcelNumber, 1432, Assessmentdetails, 1, DateTime.Now);

                    try
                    {
                        IWebElement Assessment = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Valuation_fvDataValuation']/tbody/tr/td/table/tbody"));
                        IList<IWebElement> TRAssessment = Assessment.FindElements(By.TagName("tr"));
                        IList<IWebElement> THAssessment;
                        IList<IWebElement> TDAssessment;
                        foreach (IWebElement row in TRAssessment)
                        {
                            TDAssessment = row.FindElements(By.TagName("td"));
                            THAssessment = row.FindElements(By.TagName("th"));
                            if (TRAssessment.Count > 3 && TDAssessment.Count != 0 && !row.Text.Contains("Appraised"))
                            {
                                string Assessmentdetails = THAssessment[0].Text + "~" + TDAssessment[0].Text + "~" + TDAssessment[1].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1432, Assessmentdetails, 1, DateTime.Now);

                            }
                        }
                    }
                    catch { }
                    try
                    {
                        IWebElement AssessHistory = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Valuation_gvValuationHistory']/tbody"));
                        IList<IWebElement> TRAssessHistory = AssessHistory.FindElements(By.TagName("tr"));
                        IList<IWebElement> THAssessHistory;
                        IList<IWebElement> TDAssessHistory;
                        foreach (IWebElement row in TRAssessHistory)
                        {
                            TDAssessHistory = row.FindElements(By.TagName("td"));
                            // THAssessHistory = row.FindElements(By.TagName("th"));
                            if (TRAssessHistory.Count > 3 && TDAssessHistory.Count != 0 && !row.Text.Contains("Appraised"))
                            {
                                string AssessHistorydetails = TDAssessHistory[0].Text + "~" + TDAssessHistory[1].Text + "~" + TDAssessHistory[2].Text + "~" + TDAssessHistory[3].Text + "~" + TDAssessHistory[4].Text + "~" + TDAssessHistory[5].Text + "~" + TDAssessHistory[6].Text + "~" + TDAssessHistory[7].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1504, AssessHistorydetails, 1, DateTime.Now);

                            }
                        }
                    }
                    catch { }
                    try
                    {
                        driver.FindElement(By.LinkText("Commercial")).Click();
                        Thread.Sleep(1000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Commercial", driver, "OH", "Wayne");
                    }
                    catch { }


                    try
                    {
                        driver.FindElement(By.LinkText("Tax")).Click();
                        Thread.Sleep(1000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax", driver, "OH", "Wayne");
                    }
                    catch { }

                    // Tax RAte Details Table
                    string fulltaxrate = "", Effectaxrate = "", TaxYear = "", Totalescrowpayments = "";
                    fulltaxrate = driver.FindElement(By.Id("ContentPlaceHolder1_Tax_fvDataTaxRates_FullRateLabel")).Text;
                    Effectaxrate = driver.FindElement(By.Id("ContentPlaceHolder1_Tax_fvDataTaxRates_EffectiveRateLabel")).Text;
                    TaxYear = driver.FindElement(By.Id("ContentPlaceHolder1_Tax_fvDataTax_lblTaxYear")).Text.Replace("Tax Year", "").Trim();
                    Totalescrowpayments = driver.FindElement(By.Id("ContentPlaceHolder1_Tax_fvDataEscrow_EscrowAmountLabel")).Text;

                    string TaxRatedetails = fulltaxrate + "~" + Effectaxrate + "~" + Totalescrowpayments;
                    gc.insert_date(orderNumber, parcelNumber, 1553, TaxRatedetails, 1, DateTime.Now);

                    // Tax Information Table

                    IWebElement Taxinfo = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Tax_fvDataTax']/tbody/tr/td/table/tbody"));
                    IList<IWebElement> TRTaxinfo = Taxinfo.FindElements(By.TagName("tr"));
                    IList<IWebElement> THTaxinfo;
                    IList<IWebElement> TDTaxinfo;
                    foreach (IWebElement row in TRTaxinfo)
                    {
                        THTaxinfo = row.FindElements(By.TagName("th"));
                        TDTaxinfo = row.FindElements(By.TagName("td"));
                        if (TDTaxinfo.Count != 0 && !row.Text.Contains("First Half") && !row.Text.Contains("Tax:") && !row.Text.Contains("Net") && row.Text.Trim() != "")
                        {
                            string Taxpaymentdetails = TaxYear + "~" + THTaxinfo[0].Text + "~" + TDTaxinfo[0].Text + "~" + TDTaxinfo[1].Text + "~" + TDTaxinfo[2].Text + "~" + TDTaxinfo[3].Text + "~" + TDTaxinfo[4].Text + "~" + TDTaxinfo[5].Text + "~" + "" + "~" + "";
                            gc.insert_date(orderNumber, parcelNumber, 1440, Taxpaymentdetails, 1, DateTime.Now);

                        }
                        if (TDTaxinfo.Count != 0 && !row.Text.Contains("First Half") && row.Text.Contains("Net") && row.Text.Trim() != "")
                        {
                            if (!row.Text.Contains("Net Owed:") && !row.Text.Contains("Net Paid:") && !row.Text.Contains("Net Due:") && row.Text.Contains("Net Tax:"))
                            {
                                string Taxpaymentdetails = TaxYear + "~" + THTaxinfo[0].Text + "~" + TDTaxinfo[0].Text + "~" + "" + "~" + TDTaxinfo[1].Text + "~" + "" + "~" + TDTaxinfo[2].Text + "~" + "" + "~" + "" + "~" + "";
                                gc.insert_date(orderNumber, parcelNumber, 1440, Taxpaymentdetails, 1, DateTime.Now);

                            }
                            if (row.Text.Contains("Owed") && !row.Text.Contains("Net Paid:") && !row.Text.Contains("Net Due:") && !row.Text.Contains("Net Tax:"))
                            {
                                string Taxpaymentdetails = TaxYear + "~" + THTaxinfo[0].Text + "~" + TDTaxinfo[0].Text + "~" + "" + "~" + TDTaxinfo[1].Text + "~" + "" + "~" + TDTaxinfo[2].Text + "~" + "" + "~" + "" + "~" + TDTaxinfo[3].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1440, Taxpaymentdetails, 1, DateTime.Now);

                            }
                            if (!row.Text.Contains("Owed") && row.Text.Contains("Net Paid:") && !row.Text.Contains("Net Due:") && !row.Text.Contains("Net Tax:"))
                            {
                                string Taxpaymentdetails = TaxYear + "~" + THTaxinfo[0].Text + "~" + TDTaxinfo[0].Text + "~" + "" + "~" + TDTaxinfo[1].Text + "~" + "" + "~" + TDTaxinfo[2].Text + "~" + "" + "~" + TDTaxinfo[3].Text + "~" + TDTaxinfo[4].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1440, Taxpaymentdetails, 1, DateTime.Now);

                            }
                            if (!row.Text.Contains("Owed") && !row.Text.Contains("Net Paid:") && row.Text.Contains("Net Due:") && !row.Text.Contains("Net Tax:"))
                            {
                                string Taxpaymentdetails = TaxYear + "~" + THTaxinfo[0].Text + "~" + TDTaxinfo[0].Text + "~" + "" + "~" + TDTaxinfo[1].Text + "~" + "" + "~" + TDTaxinfo[2].Text + "~" + "" + "~" + "" + "~" + TDTaxinfo[3].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1440, Taxpaymentdetails, 1, DateTime.Now);

                            }
                        }
                    }

                    // Tax Payment History Table

                    IWebElement TaxPayment = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Tax_gvDataPayments']/tbody"));
                    IList<IWebElement> TRTaxPayment = TaxPayment.FindElements(By.TagName("tr"));
                    IList<IWebElement> THadd_search = TaxPayment.FindElements(By.TagName("th"));
                    IList<IWebElement> TDTaxPayment;
                    foreach (IWebElement row in TRTaxPayment)
                    {
                        TDTaxPayment = row.FindElements(By.TagName("td"));
                        if (TRTaxPayment.Count > 3 && TDTaxPayment.Count != 0 && !row.Text.Contains("Location Address"))
                        {
                            string Taxpaymentdetails = TDTaxPayment[0].Text + "~" + TDTaxPayment[1].Text + "~" + TDTaxPayment[2].Text + "~" + TDTaxPayment[3].Text + "~" + TDTaxPayment[4].Text + "~" + TDTaxPayment[5].Text + "~" + TDTaxPayment[6].Text;
                            gc.insert_date(orderNumber, parcelNumber, 1433, Taxpaymentdetails, 1, DateTime.Now);

                        }
                    }

                    // Special Assessment Details
                    int s = 1;
                    string splcount = "";
                    string splAssesstype = "";
                    try
                    {
                        IWebElement spltype = driver.FindElement(By.Id("ContentPlaceHolder1_Tax_fvDataSpecials_Label"));
                        splAssesstype = spltype.Text;
                    }
                    catch { }

                    try
                    {
                        splcount = driver.FindElement(By.Id("ContentPlaceHolder1_Tax_fvDataSpecials_lblDataSpecials")).Text.Replace("of", "").Trim();

                    }
                    catch { }
                    try
                    {
                        for (int i = 1; i <= Convert.ToInt16(splcount); i++)
                        {
                            if (i > 1)
                            {
                                IWebElement dropdirection = driver.FindElement(By.Id("ContentPlaceHolder1_Tax_fvDataSpecials_ddlDataSpecials"));
                                SelectElement PropertyInformationSelect = new SelectElement(dropdirection);
                                PropertyInformationSelect.SelectByText(Convert.ToString(i));
                                Thread.Sleep(4000);
                                gc.CreatePdf(orderNumber, parcelNumber, "Special Assessment Details" + i, driver, "OH", "Wayne");

                            }
                            IWebElement Splassess = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Tax_fvDataSpecials']/tbody/tr/td/table/tbody"));
                            IList<IWebElement> TRSplassess = Splassess.FindElements(By.TagName("tr"));
                            IList<IWebElement> THSplassess;
                            IList<IWebElement> TDSplassess;
                            foreach (IWebElement row in TRSplassess)
                            {
                                THSplassess = row.FindElements(By.TagName("th"));
                                TDSplassess = row.FindElements(By.TagName("td"));
                                if (TDSplassess.Count != 0 && !row.Text.Contains("First Half") && !row.Text.Contains("Tax:") && !row.Text.Contains("Net") && row.Text.Trim() != "")
                                {
                                    string Splassessdetails = splAssesstype + "~" + THSplassess[0].Text + "~" + TDSplassess[0].Text + "~" + TDSplassess[1].Text + "~" + TDSplassess[2].Text + "~" + TDSplassess[3].Text + "~" + TDSplassess[4].Text + "~" + TDSplassess[5].Text;
                                    gc.insert_date(orderNumber, parcelNumber, 1448, Splassessdetails, 1, DateTime.Now);

                                }
                                if (TDSplassess.Count != 0 && !row.Text.Contains("First Half") && row.Text.Contains("Net") && row.Text.Trim() != "")
                                {

                                    string Splassessdetails = splAssesstype + "~" + THSplassess[0].Text + "~" + TDSplassess[0].Text + "~" + "" + "~" + TDSplassess[1].Text + "~" + "" + "~" + TDSplassess[2].Text + "~" + "";
                                    gc.insert_date(orderNumber, parcelNumber, 1448, Splassessdetails, 1, DateTime.Now);


                                }
                            }

                        }
                    }
                    catch { }

                    // Tax Bill

                    try
                    {

                        IWebElement Iclick = driver.FindElement(By.LinkText("Click here to view the tax bill for this parcel."));
                        gc.downloadfile(Iclick.GetAttribute("href"), orderNumber, parcelNumber, "Print Bill", "OH", "Wayne");
                    }
                    catch { }









                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "OH", "Wayne", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "OH", "Wayne");
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