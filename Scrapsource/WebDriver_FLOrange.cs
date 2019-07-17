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
using OpenQA.Selenium.Firefox;
using System.Diagnostics;
using OpenQA.Selenium.Remote;

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_FLOrange
    {
        string Outparcelno = "", outputPath = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        private TimeSpan timeOutInSeconds;
        MySqlParameter[] mParam;
        GlobalClass gc = new GlobalClass();
        string strMulti = "-", strMultiOwner = "-", strMultiAddress = "-", strMultiParcelNo = "-", strownerName = "-", strstreetAddress = "-", strPropertyUse = "-", strMunicipality = "-", strDescription = "-", strYearBuilt = "-", strParcelNumber = "-", strTaxYear = "-",
               strLand = "-", strBuilding = "-", strFeature = "-", strMarketValue = "-", strAssessedValue = "-", strBTaxYear = "-", strOriginalHome = "-", strOtherExemption = "-", strSOH = "-", strTaxSaving = "-", strAuthority = "-", strAssessDis = "-", strExemption = "-",
               strTaxValue = "-", strMillegeRate = "-", strTaxes = "-", strVAuthority = "-", strAssessDiscrip = "-", strUnits = "-", strRate = "-", strAssessment = "-", strGrossTax = "-", strGrossAmount = "-", strTotalAssess = "-", strTaxableValue = "-", strGross = "-",
               strMillegeCode = "-", strTaxAuthority = "-", strDYear = "-", strOwnerInfo = "-", strAmountDue = "-", strBillReceipt = "-", strPayment = "-", strUnPaidYear = "-", strCurrentPayoff = "-", strPaidBy = "-", strCurrentPay = "-", strPaid = "-", strMakePay = "-",
               strPercentage = "-", strTaxBenefitYear = "-", strTaxBenefitOriginalHome = "-", strTaxBenefitOtherExemption = "-", strTaxBenefitSoH = "-", strTaxBenefitTaxSaving = "-", strAdditional = "-", strPortability = "-", strSProtability = "-", strAGBenefits = "-", strTaxHead = "",
               strTaxBenefitsHead = "", strRealYear = "-", strRealFaceValue = "-", strRealCertificate = "-", strRealStatus = "-", strRealAmountPaid = "-",strUnPaid="", strRealEstate="", strAuthorityAddress="";
        int multicount,billcount;
        IWebElement IAuthority;
        string[] sAdditional,sTaxYear, sLand, sBuilding, sFeature, sPortability, sAGBenefits, sSProtability, sMarketValue, sAssessedValue, sBTaxYear,sOriginalHome,sOtherExemption,sSOH,sTaxSaving;
        List<string> lTaxYear, lLand ,lBuilding,lFeature,lMarketValue,lAssessedValue;
        IList<IWebElement> ITaxRowTR1, ITaxRowTR2, ITaxRowTR3, ITaxRowTR4, ITaxRowTR5, ITaxRowTR6, ITaxRowTR7, ITaxBRowTR1, ITaxBRowTR2, ITaxBRowTR3, ITaxBRowTR4, ITaxBRowTR5, ITaxBRowTR6, ITaxBRowTR7, ITaxBRowTR8;
        public string FTP_FLOrange(string address, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            IJavaScriptExecutor js = driver as IJavaScriptExecutor;
            using (driver = new PhantomJSDriver())
            {
                StartTime = DateTime.Now.ToString("HH:mm:ss");
                try
                {            

                    driver.Navigate().GoToUrl("http://www.ocpafl.org/searches/ParcelSearch.aspx#%23");
                    try
                    {
                        IWebElement Iaccept = driver.FindElement(By.Id("popup_ok"));
                        Iaccept.SendKeys(Keys.Enter);
                        IWebElement Iadd = driver.FindElement(By.Id("ctl00_ctl00_ctl00_ctl00_ContentMain_ctl03_PopupPanel1_CloseButton2"));
                        Iadd.Click();
                    }
                    catch { }
                    Thread.Sleep(6000);


                    if (searchType == "address")
                    {
                        //driver.FindElement(By.Id("ctl00_ctl00_ctl00_ctl00_ContentMain_ContentMain_ContentMain_ContentMain_TabContainer1_Searches_SubTabContainer1_QuickSearches_CompositAddressSearch1_AddressSearchType_Simple")).Click();
                        driver.FindElement(By.Id("ctl00_ctl00_ctl00_ctl00_ContentMain_ContentMain_ContentMain_ContentMain_TabContainer1_Searches_SubTabContainer1_QuickSearches_CompositAddressSearch1_AddressSearch1_ctl00_Address")).Clear();
                        driver.FindElement(By.Id("ctl00_ctl00_ctl00_ctl00_ContentMain_ContentMain_ContentMain_ContentMain_TabContainer1_Searches_SubTabContainer1_QuickSearches_CompositAddressSearch1_AddressSearch1_ctl00_Address")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "FL", "Orange");
                        driver.FindElement(By.Id("ctl00_ctl00_ctl00_ctl00_ContentMain_ContentMain_ContentMain_ContentMain_TabContainer1_Searches_SubTabContainer1_QuickSearches_CompositAddressSearch1_AddressSearch1_ctl00_ActionButton1")).Click();
                        Thread.Sleep(9000);
                        try
                        {
                            IWebElement IMulti = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_ctl00_ctl00_ContentMain_ContentMain_ContentMain_ContentMain_TabContainer1_ResultsTab_Pager1']/table/tbody/tr/td[1]"));
                            strMulti = IMulti.Text;
                            string strMultiCount = gc.Between(IMulti.Text, "(", " total records)");
                            if (Convert.ToInt32(strMultiCount) < 25)
                            {
                                try
                                {
                                    IWebElement ICount = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_ctl00_ctl00_ContentMain_ContentMain_ContentMain_ContentMain_TabContainer1_ResultsTab_Pager1']/table/tbody/tr/td[1]/a"));
                                    ICount.Click();
                                    IWebElement IPageCount = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_ctl00_ctl00_ContentMain_ContentMain_ContentMain_ContentMain_TabContainer1_ResultsTab_Pager1_ctl07']"));
                                    IPageCount.Click();
                                }
                                catch { }
                            }

                            if (Convert.ToInt32(strMultiCount) > 25)
                            {
                                HttpContext.Current.Session["multiparcel_FLOrange_Maximum"] = "Yes";
                                driver.Quit();
                                return "Maximum";
                            }
                            if (!strMulti.Contains("Page 1 of 1(1 total records") && strMulti != "" && strMulti != "-")
                            {
                                gc.CreatePdf_WOP(orderNumber, "Multi Owner Search Result", driver, "FL", "Orange");
                                IWebElement Imultitable = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_ctl00_ctl00_ContentMain_ContentMain_ContentMain_ContentMain_TabContainer1_ResultsTab_ResultsGrid']/tbody"));
                                IList<IWebElement> Irow = Imultitable.FindElements(By.TagName("tr"));
                                IList<IWebElement> Itd;
                                foreach (IWebElement rows in Irow)
                                {
                                    //GlobalClass.multiparcel_FLOrange = "Yes";
                                    HttpContext.Current.Session["multiparcel_FLOrange"] = "Yes";
                                    Itd = rows.FindElements(By.TagName("td"));
                                    if (Itd.Count != 0 && !rows.Text.Contains("Owner(s) Property Address Homestead Parcel ID") && multicount <= 25)
                                    {
                                        strMultiOwner = Itd[0].Text;
                                        strMultiAddress = Itd[1].Text;
                                        strMultiParcelNo = Itd[3].Text;

                                        string strMultiDetails = strMultiOwner + "~" + strMultiAddress;
                                        gc.insert_date(orderNumber, strMultiParcelNo, 340, strMultiDetails, 1, DateTime.Now);
                                    }
                                    multicount++;
                                }

                                driver.Quit();
                                return "MultiParcel";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.Id("ctl00_ctl00_ctl00_ctl00_ContentMain_ContentMain_ContentMain_ContentMain_TabContainer1_Searches_SubTabContainer1_QuickSearches_ParcelIDSearch1_ctl00_FullParcel")).Clear();
                        string strParcel = parcelNumber.Replace("-", "");
                        driver.FindElement(By.Id("ctl00_ctl00_ctl00_ctl00_ContentMain_ContentMain_ContentMain_ContentMain_TabContainer1_Searches_SubTabContainer1_QuickSearches_ParcelIDSearch1_ctl00_FullParcel")).SendKeys(strParcel);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "FL", "Orange");
                        driver.FindElement(By.Id("ctl00_ctl00_ctl00_ctl00_ContentMain_ContentMain_ContentMain_ContentMain_TabContainer1_Searches_SubTabContainer1_QuickSearches_ParcelIDSearch1_ctl00_ActionButton1")).Click();
                        Thread.Sleep(9000);
                    }
                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("ctl00_ctl00_ctl00_ctl00_ContentMain_ContentMain_ContentMain_ContentMain_TabContainer1_Searches_SubTabContainer1_QuickSearches_OwnerNameSearch1_ctl00_OwnerName")).Clear();
                        driver.FindElement(By.Id("ctl00_ctl00_ctl00_ctl00_ContentMain_ContentMain_ContentMain_ContentMain_TabContainer1_Searches_SubTabContainer1_QuickSearches_OwnerNameSearch1_ctl00_OwnerName")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "Owner Search", driver, "FL", "Orange");
                        driver.FindElement(By.Id("ctl00_ctl00_ctl00_ctl00_ContentMain_ContentMain_ContentMain_ContentMain_TabContainer1_Searches_SubTabContainer1_QuickSearches_OwnerNameSearch1_ctl00_ActionButton1")).Click();
                        Thread.Sleep(9000);
                        try
                        {
                            IWebElement IMulti = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_ctl00_ctl00_ContentMain_ContentMain_ContentMain_ContentMain_TabContainer1_ResultsTab_Pager1']/table/tbody/tr/td[1]"));
                            strMulti = IMulti.Text;
                            string strMultiCount = gc.Between(IMulti.Text, "(", " total records)");
                            if (Convert.ToInt32(strMultiCount) < 25)
                            {
                                try
                                {
                                    IWebElement ICount = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_ctl00_ctl00_ContentMain_ContentMain_ContentMain_ContentMain_TabContainer1_ResultsTab_Pager1']/table/tbody/tr/td[1]/a"));
                                    ICount.Click();
                                    IWebElement IPageCount = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_ctl00_ctl00_ContentMain_ContentMain_ContentMain_ContentMain_TabContainer1_ResultsTab_Pager1_ctl07']"));
                                    IPageCount.Click();
                                }
                                catch { }
                            }
                            if (Convert.ToInt32(strMultiCount) > 25)
                            {
                                HttpContext.Current.Session["multiparcel_FLOrange_Maximum"] = "Yes";
                                driver.Quit();
                                return "Maximum";
                            }
                            if (!strMulti.Contains("Page 1 of 1(1 total records") && strMulti != "" && strMulti != "-")
                            {
                                gc.CreatePdf_WOP(orderNumber, "Multi Owner Search Result", driver, "FL", "Orange");
                                IWebElement Imultitable = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_ctl00_ctl00_ContentMain_ContentMain_ContentMain_ContentMain_TabContainer1_ResultsTab_ResultsGrid']/tbody"));
                                IList<IWebElement> Irow = Imultitable.FindElements(By.TagName("tr"));
                                IList<IWebElement> Itd;
                                foreach (IWebElement rows in Irow)
                                {
                                    //GlobalClass.multiparcel_FLOrange = "Yes";
                                    HttpContext.Current.Session["multiparcel_FLOrange"] = "Yes";
                                    Itd = rows.FindElements(By.TagName("td"));
                                    if (Itd.Count != 0 && !rows.Text.Contains("Owner(s) Property Address Homestead Parcel ID") && multicount <= 25)
                                    {
                                        strMultiOwner = Itd[0].Text;
                                        strMultiAddress = Itd[1].Text;
                                        strMultiParcelNo = Itd[3].Text;

                                        string strMultiDetails = strMultiOwner + "~" + strMultiAddress;
                                        gc.insert_date(orderNumber, strMultiParcelNo, 340, strMultiDetails, 1, DateTime.Now);
                                    }
                                    multicount++;
                                }

                                driver.Quit();
                                return "MultiParcel";
                            }
                        }
                        catch { }
                    }
                    try
                    {
                        IWebElement INodata = driver.FindElement(By.Id("ctl00_ctl00_ctl00_ctl00_ContentMain_ContentMain_ContentMain_ContentMain_TabContainer1_ResultsTab_ResultsGrid"));
                        if(INodata.Text.Contains("search did not return any results"))
                        {
                            HttpContext.Current.Session["Nodata_FLOrange"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    //Property Details
                    string strParcelNo = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_ctl00_ctl00_ContentMain_ContentMain_ContentMain_ContentMain_TabContainer1_DetailsTab']/div/table/tbody/tr/td[1]/div[1]/div[1]/span[2]")).Text;
                    strParcelNumber = gc.Between(strParcelNo, "< ", " >");
                    string strowner = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_ctl00_ctl00_ContentMain_ContentMain_ContentMain_ContentMain_TabContainer1_DetailsTab']/div/table/tbody/tr/td[1]/div[1]/div[2]/table/tbody/tr/td[1]/fieldset[1]")).Text.Replace("\r\n", " ");
                    strownerName = GlobalClass.After(strowner, "Name(s) ");
                    string strstreet = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_ctl00_ctl00_ContentMain_ContentMain_ContentMain_ContentMain_TabContainer1_DetailsTab']/div/table/tbody/tr/td[1]/div[1]/div[2]/table/tbody/tr/td[2]/fieldset[1]")).Text;
                    strstreet = GlobalClass.After(strstreet, "Physical Street Address\r\n");
                    string strAddressdt = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_ctl00_ctl00_ContentMain_ContentMain_ContentMain_ContentMain_TabContainer1_DetailsTab']/div/table/tbody/tr/td[1]/div[1]/div[2]/table/tbody/tr/td[2]/fieldset[2]")).Text;
                    strAddressdt = GlobalClass.After(strAddressdt, "Postal City and Zipcode\r\n");
                    strstreetAddress = strstreet + " " + strAddressdt;
                    string strProper = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_ctl00_ctl00_ContentMain_ContentMain_ContentMain_ContentMain_TabContainer1_DetailsTab']/div/table/tbody/tr/td[1]/div[1]/div[2]/table/tbody/tr/td[2]/fieldset[3]")).Text;
                    strPropertyUse = GlobalClass.After(strProper, "Property Use\r\n");
                    string strMun = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_ctl00_ctl00_ContentMain_ContentMain_ContentMain_ContentMain_TabContainer1_DetailsTab']/div/table/tbody/tr/td[1]/div[1]/div[2]/table/tbody/tr/td[2]/fieldset[4]")).Text;
                    strMunicipality = GlobalClass.After(strMun, "Municipality\r\n");

                    IWebElement IProperty = driver.FindElement(By.Id("__tab_ctl00_ctl00_ctl00_ctl00_ContentMain_ContentMain_ContentMain_ContentMain_TabContainer1_DetailsTab_DetailsController1_ctl00_TabContainer1_PropertyFeature"));
                    IProperty.Click();
                    Thread.Sleep(4000);
                    gc.CreatePdf(orderNumber, strParcelNumber, "Property Detail", driver, "FL", "Orange");

                    string strDes = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_ctl00_ctl00_ContentMain_ContentMain_ContentMain_ContentMain_TabContainer1_DetailsTab_DetailsController1_ctl00_TabContainer1_PropertyFeature']/fieldset[2]")).Text;
                    strDescription = GlobalClass.After(strDes, "Property Description\r\n");
                    try { 
                    string strYear = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_ctl00_ctl00_ContentMain_ContentMain_ContentMain_ContentMain_TabContainer1_DetailsTab_DetailsController1_ctl00_TabContainer1_PropertyFeature_PropertyFeatures1_BuildingGrid']/tbody")).Text;
                    strYearBuilt = gc.Between(strYear, "Actual Year Built: ", "\r\nBeds:");
                    }
                    catch { }
                    string strPropertyDetails = strownerName + "~" + strstreetAddress + "~" + strPropertyUse + "~" + strMunicipality + "~" + strDescription + "~" + strYearBuilt;
                    gc.insert_date(orderNumber, strParcelNumber, 350, strPropertyDetails, 1, DateTime.Now);

                    IWebElement Ivalue = driver.FindElement(By.Id("__tab_ctl00_ctl00_ctl00_ctl00_ContentMain_ContentMain_ContentMain_ContentMain_TabContainer1_DetailsTab_DetailsController1_ctl00_TabContainer1_ValueTax"));
                    Ivalue.SendKeys(Keys.Enter);
                    Thread.Sleep(4000);
                    gc.CreatePdf(orderNumber, strParcelNumber, "Values Exemption Taxes", driver, "FL", "Orange");

                    try
                    {
                        IWebElement ITaxValues = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_ctl00_ctl00_ContentMain_ContentMain_ContentMain_ContentMain_TabContainer1_DetailsTab_DetailsController1_ctl00_TabContainer1_ValueTax_ValuesTaxes1_UpdatePanel1']/table[1]/tbody/tr/td[1]/fieldset[1]/table[1]/tbody"));
                        IList<IWebElement> ITaxValuesrow = ITaxValues.FindElements(By.TagName("tr"));
                        IList<IWebElement> ITaxValuestd;
                        IList<IWebElement> ITaxRowTR;
                        IList<IWebElement> ITaxHeading = ITaxValues.FindElements(By.TagName("th"));
                        foreach (IWebElement irow in ITaxHeading)
                        {
                            string strTax = irow.Text + " ";
                            strTaxHead += strTax;
                        }
                        foreach (IWebElement Values in ITaxValuesrow)
                        {
                            ITaxValuestd = Values.FindElements(By.TagName("table"));
                            try
                            {
                                ITaxRowTR1 = ITaxValuestd[0].FindElements(By.TagName("tr"));
                                ITaxRowTR2 = ITaxValuestd[1].FindElements(By.TagName("tr"));
                                ITaxRowTR3 = ITaxValuestd[2].FindElements(By.TagName("tr"));
                                ITaxRowTR4 = ITaxValuestd[3].FindElements(By.TagName("tr"));
                                ITaxRowTR5 = ITaxValuestd[4].FindElements(By.TagName("tr"));
                                ITaxRowTR6 = ITaxValuestd[5].FindElements(By.TagName("tr"));
                                ITaxRowTR7 = ITaxValuestd[6].FindElements(By.TagName("tr"));
                            }
                            catch { }
                            if (ITaxValuestd.Count == 7 && strTaxHead.Contains("Portability"))
                            {
                                for (int i = 0; i <= ITaxValuestd.Count; i++)
                                {
                                    try
                                    {
                                        strTaxYear = ITaxRowTR1[i].Text;
                                        strLand = ITaxRowTR2[i].Text.Replace("+", "").Replace("-", "").Replace("=", "").Trim();
                                        strBuilding = ITaxRowTR3[i].Text.Replace("+", "").Replace("-", "").Replace("=", "").Trim();
                                        strFeature = ITaxRowTR4[i].Text.Replace("+", "").Replace("-", "").Replace("=", "").Trim();
                                        strMarketValue = ITaxRowTR5[i].Text.Replace("+", "").Replace("-", "").Replace("=", "").Trim();
                                        strPortability = ITaxRowTR6[i].Text.Replace("+", "").Replace("-", "").Replace("=", "").Trim();
                                        strAssessedValue = ITaxRowTR7[i].Text.Replace("+", "").Replace("-", "").Replace("=", "").Trim();


                                        string strTaxValueDetails = strTaxYear + "~" + strLand + "~" + strBuilding + "~" + strFeature + "~" + strMarketValue + "~" + strPortability + "~" + strAssessedValue;
                                        gc.insert_date(orderNumber, strParcelNumber, 351, strTaxValueDetails, 1, DateTime.Now);
                                    }
                                    catch { }
                                }
                            }
                            if (ITaxValuestd.Count == 6 && !strTaxHead.Contains("Portability"))
                            {
                                for (int i = 0; i <= ITaxValuestd.Count; i++)
                                {
                                    try
                                    {
                                        strTaxYear = ITaxRowTR1[i].Text;
                                        strLand = ITaxRowTR2[i].Text.Replace("+", "").Replace("-", "").Replace("=", "").Trim();
                                        strBuilding = ITaxRowTR3[i].Text.Replace("+", "").Replace("-", "").Replace("=", "").Trim();
                                        strFeature = ITaxRowTR4[i].Text.Replace("+", "").Replace("-", "").Replace("=", "").Trim();
                                        strMarketValue = ITaxRowTR5[i].Text.Replace("+", "").Replace("-", "").Replace("=", "").Trim();
                                        strAssessedValue = ITaxRowTR6[i].Text.Replace("+", "").Replace("-", "").Replace("=", "").Trim();

                                        string strTaxValueDetails = strTaxYear + "~" + strLand + "~" + strBuilding + "~" + strFeature + "~" + strMarketValue + "~" + "-" + "~" + strAssessedValue;
                                        gc.insert_date(orderNumber, strParcelNumber, 351, strTaxValueDetails, 1, DateTime.Now);
                                    }
                                    catch { }
                                }
                            }

                            if (ITaxValuestd.Count == 5 && !strTaxHead.Contains("Portability"))
                            {
                                for (int i = 0; i <= ITaxValuestd.Count; i++)
                                {
                                    try
                                    {
                                        strTaxYear = ITaxRowTR1[i].Text;
                                        strLand = ITaxRowTR2[i].Text.Replace("+", "").Replace("-", "").Replace("=", "").Trim();
                                        strBuilding = ITaxRowTR3[i].Text.Replace("+", "").Replace("-", "").Replace("=", "").Trim();
                                        strFeature = ITaxRowTR4[i].Text.Replace("+", "").Replace("-", "").Replace("=", "").Trim();
                                        strMarketValue = ITaxRowTR5[i].Text.Replace("+", "").Replace("-", "").Replace("=", "").Trim();
                                        strAssessedValue = ITaxRowTR6[i].Text.Replace("+", "").Replace("-", "").Replace("=", "").Trim();

                                        string strTaxValueDetails = strTaxYear + "~" + strLand + "~" + strBuilding + "~" + strFeature + "~" + strMarketValue + "~" + "-" + "~" + strAssessedValue;
                                        gc.insert_date(orderNumber, strParcelNumber, 351, strTaxValueDetails, 1, DateTime.Now);
                                    }
                                    catch { }
                                }
                            }
                            if (ITaxValuestd.Count == 3 && !strTaxHead.Contains("Portability"))
                            {
                                for (int i = 0; i <= ITaxValuestd.Count; i++)
                                {
                                    try
                                    {
                                        strTaxYear = ITaxRowTR1[i].Text;
                                        strMarketValue = ITaxRowTR2[i].Text.Replace("+", "").Replace("-", "").Replace("=", "").Trim();
                                        strAssessedValue = ITaxRowTR3[i].Text.Replace("+", "").Replace("-", "").Replace("=", "").Trim();

                                        string strTaxValueDetails = strTaxYear + "~" + strLand + "~" + strBuilding + "~" + strFeature + "~" + strMarketValue + "~" + "-" + "~" + strAssessedValue;
                                        gc.insert_date(orderNumber, strParcelNumber, 351, strTaxValueDetails, 1, DateTime.Now);
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                    catch { }
                    try
                    {
                        IWebElement ITaxBenefits = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_ctl00_ctl00_ContentMain_ContentMain_ContentMain_ContentMain_TabContainer1_DetailsTab_DetailsController1_ctl00_TabContainer1_ValueTax_ValuesTaxes1_UpdatePanel1']/table[1]/tbody/tr/td[1]/fieldset[1]/table[2]/tbody"));
                        IList<IWebElement> ITaxBenefitsRow = ITaxBenefits.FindElements(By.TagName("tr"));
                        IList<IWebElement> ITaxBenefitsTD;
                        IList<IWebElement> ITaxBenefitTR;
                        IList<IWebElement> ITaxBenefitsHeading = ITaxBenefits.FindElements(By.TagName("th"));
                        foreach (IWebElement irow in ITaxBenefitsHeading)
                        {
                            string strTaxHead = irow.Text + " ";
                            strTaxBenefitsHead += strTaxHead;
                        }
                        foreach (IWebElement Benefits in ITaxBenefitsRow)
                        {
                            ITaxBenefitsTD = Benefits.FindElements(By.TagName("table"));
                            try
                            {
                                ITaxBRowTR1 = ITaxBenefitsTD[0].FindElements(By.TagName("tr"));
                                ITaxBRowTR2 = ITaxBenefitsTD[1].FindElements(By.TagName("tr"));
                                ITaxBRowTR3 = ITaxBenefitsTD[2].FindElements(By.TagName("tr"));
                                ITaxBRowTR4 = ITaxBenefitsTD[3].FindElements(By.TagName("tr"));
                                ITaxBRowTR5 = ITaxBenefitsTD[4].FindElements(By.TagName("tr"));
                                ITaxBRowTR6 = ITaxBenefitsTD[5].FindElements(By.TagName("tr"));
                                ITaxBRowTR7 = ITaxBenefitsTD[6].FindElements(By.TagName("tr"));
                                ITaxBRowTR8 = ITaxBenefitsTD[7].FindElements(By.TagName("tr"));
                            }
                            catch { }
                            if (ITaxBenefitsTD.Count == 7 && strTaxBenefitsHead.Contains("Portability"))
                            {
                                for (int i = 0; i <= ITaxBenefitsTD.Count; i++)
                                {
                                    try
                                    {
                                        strBTaxYear = ITaxBRowTR1[i].Text;
                                        strOriginalHome = ITaxBRowTR2[i].Text.Replace("+", "").Replace("-", "").Replace("=", "").Trim();
                                        strAdditional = ITaxBRowTR3[i].Text.Replace("+", "").Replace("-", "").Replace("=", "").Trim();
                                        strOtherExemption = ITaxBRowTR4[i].Text.Replace("+", "").Replace("-", "").Replace("=", "").Trim();
                                        strPortability = ITaxBRowTR5[i].Text.Replace("+", "").Replace("-", "").Replace("=", "").Trim();
                                        strSOH = ITaxBRowTR6[i].Text.Replace("+", "").Replace("-", "").Replace("=", "").Trim();
                                        strTaxSaving = ITaxBRowTR7[i].Text.Replace("+", "").Replace("-", "").Replace("=", "").Trim();

                                        string strTaxValueDetails = strBTaxYear + "~" + strOriginalHome + "~" + strAdditional + "~" + strOtherExemption + "~" + strPortability + "~" + strSOH + "~" + "-" + "~" + strTaxSaving;
                                        gc.insert_date(orderNumber, strParcelNumber, 352, strTaxValueDetails, 1, DateTime.Now);
                                    }
                                    catch { }
                                }
                            }
                            if (ITaxBenefitsTD.Count == 6 && !strTaxBenefitsHead.Contains("Portability"))
                            {
                                for (int i = 0; i <= ITaxBenefitsTD.Count; i++)
                                {
                                    try
                                    {
                                        strBTaxYear = ITaxBRowTR1[i].Text;
                                        strOriginalHome = ITaxBRowTR2[i].Text.Replace("+", "").Replace("-", "").Replace("=", "").Trim();
                                        strAdditional = ITaxBRowTR3[i].Text.Replace("+", "").Replace("-", "").Replace("=", "").Trim();
                                        strOtherExemption = ITaxBRowTR4[i].Text.Replace("+", "").Replace("-", "").Replace("=", "").Trim();
                                        strSOH = ITaxBRowTR5[i].Text.Replace("+", "").Replace("-", "").Replace("=", "").Trim();
                                        strTaxSaving = ITaxBRowTR6[i].Text.Replace("+", "").Replace("-", "").Replace("=", "").Trim();

                                        string strTaxValueDetails = strBTaxYear + "~" + strOriginalHome + "~" + strAdditional + "~" + strOtherExemption + "~" + "-" + "~" + strSOH + "~" + "-" + "~" + strTaxSaving;
                                        gc.insert_date(orderNumber, strParcelNumber, 352, strTaxValueDetails, 1, DateTime.Now);
                                    }
                                    catch { }
                                }
                            }
                            if (ITaxBenefitsTD.Count == 5 && !strTaxBenefitsHead.Contains("Portability") && !strTaxBenefitsHead.Contains("Additional Hx"))
                            {
                                for (int i = 0; i <= ITaxBenefitsTD.Count; i++)
                                {
                                    try
                                    {
                                        strBTaxYear = ITaxBRowTR1[i].Text;
                                        strOriginalHome = ITaxBRowTR2[i].Text.Replace("+", "").Replace("-", "").Replace("=", "").Trim();
                                        strOtherExemption = ITaxBRowTR3[i].Text.Replace("+", "").Replace("-", "").Replace("=", "").Trim();
                                        strSOH = ITaxBRowTR4[i].Text.Replace("+", "").Replace("-", "").Replace("=", "").Trim();
                                        strTaxSaving = ITaxBRowTR5[i].Text.Replace("+", "").Replace("-", "").Replace("=", "").Trim();

                                        string strTaxValueDetails = strBTaxYear + "~" + strOriginalHome + "~" + "-" + "~" + strOtherExemption + "~" + "-" + "~" + strSOH + "~" + "-" + "~" + strTaxSaving;
                                        gc.insert_date(orderNumber, strParcelNumber, 352, strTaxValueDetails, 1, DateTime.Now);
                                    }
                                    catch { }
                                }
                            }
                            if (ITaxBenefitsTD.Count == 2 && strTaxBenefitsHead.Contains("Tax Savings"))
                            {
                                for (int i = 0; i <= ITaxBenefitsTD.Count; i++)
                                {
                                    try
                                    {
                                        strBTaxYear = ITaxBRowTR1[i].Text;
                                        strTaxSaving = ITaxBRowTR2[i].Text.Replace("+", "").Replace("-", "").Replace("=", "").Trim();

                                        string strTaxValueDetails = strBTaxYear + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + strTaxSaving;
                                        gc.insert_date(orderNumber, strParcelNumber, 352, strTaxValueDetails, 1, DateTime.Now);
                                    }
                                    catch { }
                                }
                            }
                            if (ITaxBenefitsTD.Count == 3 && (strTaxBenefitsHead.Contains("AG Benefits") || strTaxBenefitsHead.Contains("Ag Benefits") || strTaxBenefitsHead.Contains("AgBenefits") || strTaxBenefitsHead.Contains("AGBenefits")))
                            {
                                for (int i = 0; i <= ITaxBenefitsTD.Count; i++)
                                {
                                    try
                                    {
                                        strBTaxYear = ITaxBRowTR1[i].Text;
                                        strAGBenefits = ITaxBRowTR2[i].Text;
                                        strTaxSaving = ITaxBRowTR3[i].Text.Replace("+", "").Replace("-", "").Replace("=", "").Trim();

                                        string strTaxValueDetails = strBTaxYear + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + strSOH + "~" + strAGBenefits + "~" + strTaxSaving;
                                        gc.insert_date(orderNumber, strParcelNumber, 352, strTaxValueDetails, 1, DateTime.Now);
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                    catch { }

                    try
                    {
                        IWebElement ITaxCertificate = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_ctl00_ctl00_ContentMain_ContentMain_ContentMain_ContentMain_TabContainer1_DetailsTab_DetailsController1_ctl00_TabContainer1_ValueTax_ValuesTaxes1_Grid1']/tbody"));
                        IList<IWebElement> ITaxCertificateRow = ITaxCertificate.FindElements(By.TagName("tr"));
                        IList<IWebElement> ITaxCertificateTD;
                        foreach (IWebElement certificate in ITaxCertificateRow)
                        {
                            ITaxCertificateTD = certificate.FindElements(By.TagName("td"));
                            if (ITaxCertificateTD.Count != 0)
                            {
                                try
                                {
                                    strAuthority = ITaxCertificateTD[0].Text;
                                    strAssessDis = ITaxCertificateTD[1].Text;
                                    strExemption = ITaxCertificateTD[2].Text;
                                    strTaxValue = ITaxCertificateTD[3].Text;
                                    strMillegeRate = ITaxCertificateTD[4].Text;
                                    strTaxes = ITaxCertificateTD[5].Text;
                                    strPercentage = ITaxCertificateTD[6].Text;
                                }
                                catch { }
                                string strTaxCertificateDetails = "Taxing Authority" + "~" + strAuthority + "~" + "-" + "~" + strAssessDis + "~" + strExemption + "~" + strTaxValue + "~" + strMillegeRate + "~" + "-" + "~" + "-" + "~" + strTaxes + "~" + strPercentage;
                                gc.insert_date(orderNumber, strParcelNumber, 353, strTaxCertificateDetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    try
                    {
                        IWebElement ITaxValorem = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_ctl00_ctl00_ContentMain_ContentMain_ContentMain_ContentMain_TabContainer1_DetailsTab_DetailsController1_ctl00_TabContainer1_ValueTax_ValuesTaxes1_NonAdValoremTaxes1_Grid1']/tbody"));
                        IList<IWebElement> ITaxValoremRow = ITaxValorem.FindElements(By.TagName("tr"));
                        IList<IWebElement> ITaxValoremTD;
                        foreach (IWebElement Valorem in ITaxValoremRow)
                        {
                            ITaxValoremTD = Valorem.FindElements(By.TagName("td"));
                            if (ITaxValoremTD.Count != 0 && !Valorem.Text.Contains("There are no Non-Ad Valorem Assessments"))
                            {
                                try
                                {
                                    strVAuthority = ITaxValoremTD[0].Text;
                                    strAssessDiscrip = ITaxValoremTD[1].Text;
                                    strUnits = ITaxValoremTD[2].Text;
                                    strRate = ITaxValoremTD[3].Text;
                                    strAssessment = ITaxValoremTD[4].Text;
                                }
                                catch { }

                                string strTaxValoremDetails = "Levying Authority" + "~" + strVAuthority + "~" + strAssessDiscrip + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + strUnits + "~" + strRate + "~" + strAssessment + "~" + "-";
                                gc.insert_date(orderNumber, strParcelNumber, 353, strTaxValoremDetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    try
                    {
                        strGrossTax = driver.FindElement(By.Id("ctl00_ctl00_ctl00_ctl00_ContentMain_ContentMain_ContentMain_ContentMain_TabContainer1_DetailsTab_DetailsController1_ctl00_TabContainer1_ValueTax_ValuesTaxes1_NAMillageRatesLbl")).Text;
                        strGrossAmount = driver.FindElement(By.Id("ctl00_ctl00_ctl00_ctl00_ContentMain_ContentMain_ContentMain_ContentMain_TabContainer1_DetailsTab_DetailsController1_ctl00_TabContainer1_ValueTax_ValuesTaxes1_NATotalTax")).Text;
                        string strTaxGrossDetails = strGrossTax.Replace(":", "") + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + strGrossAmount + "~" + "-";
                        gc.insert_date(orderNumber, strParcelNumber, 353, strTaxGrossDetails, 1, DateTime.Now);
                    }
                    catch { }
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");


                    //Download
                    try
                    {

                        IWebElement trimdownload = driver.FindElement(By.Id("ctl00_ctl00_ctl00_ctl00_ContentMain_ContentMain_ContentMain_ContentMain_TabContainer1_DetailsTab_DetailsController1_ctl00_EverythingButton6"));
                        string Trimhref = trimdownload.GetAttribute("href");
                        driver.Navigate().GoToUrl(Trimhref);
                        gc.CreatePdf(orderNumber, strParcelNumber, "AssessmentPage", driver, "FL", "Orange");
                        //gc.downloadfile(Trimhref, orderNumber, strParcelNumber, "Assessment", "FL", "Orange");
                    }
                    catch { }
                    try
                    {
                        //Tax Authority Details
                        driver.Navigate().GoToUrl("http://www.octaxcol.com/locations/");
                        try
                        {
                            IWebElement IPopup = driver.FindElement(By.XPath("/html/body/div[2]/div[2]/div[1]/ul/li[6]/a"));
                            IPopup.Click();
                        }
                        catch { }
                        IWebElement Isearch = driver.FindElement(By.XPath("/html/body/div[3]/div[6]/div/div/div/div/div[18]/div[1]/div/div/div[1]/div"));
                        Isearch.Click();
                        gc.CreatePdf(orderNumber, strParcelNumber, "Tax Authority", driver, "FL", "Orange");
                        Thread.Sleep(3000);
                        IAuthority = driver.FindElement(By.XPath("/html/body/div[3]/div[6]/div/div/div/div/div[18]/div[1]/div/div/div[3]/div"));
                        strAuthorityAddress = IAuthority.Text;
                        if (strAuthorityAddress.Trim() == "")
                        {
                            try
                            {
                                IAuthority = driver.FindElement(By.XPath("/html/body/div[3]/div[6]/div/div/div/div/div[18]/div[1]/div/div/div[4]/div"));
                                strAuthorityAddress = IAuthority.Text;
                            }
                            catch { }
                        }
                        strTaxAuthority = gc.Between(strAuthorityAddress, "Address:", "Hours:");
                    }
                    catch (Exception ex) { }
                    //Tax Details //
                    driver.Navigate().GoToUrl("http://pt.octaxcol.com/propertytax/search.aspx");
                    Thread.Sleep(7000);

                    IWebElement TextBox = driver.FindElement(By.Id("mainContent_txtParcel"));
                    String input = strParcelNumber.Replace("-", "");
                    TextBox.GetAttribute("name");
                    IJavaScriptExecutor jst = (IJavaScriptExecutor)driver;
                    jst.ExecuteScript("arguments[1].value = arguments[0]; ", input, TextBox);
                    gc.CreatePdf(orderNumber, strParcelNumber, "Tax Search", driver, "FL", "Orange");

                    
                    IWebElement ItaxSerach = driver.FindElement(By.Id("mainContent_btnParcel"));
                    ItaxSerach.SendKeys(Keys.Enter);
                    Thread.Sleep(7000);
                    gc.CreatePdf(orderNumber, strParcelNumber, "Tax Search Result", driver, "FL", "Orange");
                    strTaxYear = driver.FindElement(By.Id("mainContent_cellTaxYearInfo")).Text;
                    strTaxYear = GlobalClass.After(strTaxYear, "Tax Year: ");
                    strTotalAssess = driver.FindElement(By.Id("mainContent_cellAssessedValue")).Text;
                    strTaxableValue = driver.FindElement(By.Id("mainContent_cellTaxableValue")).Text;
                    strGross = driver.FindElement(By.Id("mainContent_cellGrossTaxAmount")).Text;
                    strMillegeCode = driver.FindElement(By.Id("mainContent_cellMillageCode")).Text;

                    string strTaxAssessDetails = strTaxYear + "~" + strTotalAssess + "~" + strTaxableValue + "~" + strGross + "~" + strMillegeCode + "~" + strTaxAuthority;
                    gc.insert_date(orderNumber, strParcelNumber, 354, strTaxAssessDetails, 1, DateTime.Now);

                    IWebElement IdelinTable = driver.FindElement(By.XPath("//*[@id='mainContent_tableUnpaidTaxes']/tbody"));
                    IList<IWebElement> IdelinRow = IdelinTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> IdelinTD;
                    foreach(IWebElement delin in IdelinRow) 
                    {
                        IdelinTD = delin.FindElements(By.TagName("td"));
                        if(IdelinTD.Count !=0 && !delin.Text.Contains("Year"))
                        {
                            strDYear = IdelinTD[0].Text;
                            strOwnerInfo = IdelinTD[1].Text;
                            strAmountDue = IdelinTD[2].Text;
                            try
                            {
                                IWebElement IbillReceipt = IdelinTD[3].FindElement(By.TagName("a"));
                                strBillReceipt = IbillReceipt.GetAttribute("href");

                            }
                            catch { }
                            try
                            {
                                IWebElement IPayment = IdelinTD[5].FindElement(By.TagName("a"));
                                strPayment = IPayment.GetAttribute("href");
                                if (strPayment != "" && strPayment != "-")
                                {
                                    strPayment = "PayNow";
                                }
                            }
                            catch { }
                            try
                            {
                                if (strPayment == "" || strPayment == "-" && strPayment != "PayNow")
                                {
                                    strPayment = IdelinTD[3].Text;
                                }
                            }
                            catch { }
                            string strDelinTaxDetails = strDYear + "~" + strOwnerInfo + "~" + strAmountDue + "~" + "TaxBill" + "~" + strPayment;
                            gc.insert_date(orderNumber, strParcelNumber, 355, strDelinTaxDetails, 1, DateTime.Now);
                            strPayment = "";
                            try
                            {
                                if (billcount <= 2)
                                {
                                    billcount++;
                                    gc.downloadfile(strBillReceipt, orderNumber, strParcelNumber, strDYear + "Tax Bill ", "FL", "Orange");
                                }
                            }
                            catch { }
                        }
                    }

                    try
                    {
                        strUnPaid = driver.FindElement(By.XPath("//*[@id='mainContent_Table9']/tbody/tr/td/strong")).Text;
                    }
                    catch { }
                    IWebElement IUnpaidTable = driver.FindElement(By.XPath("//*[@id='mainContent_tblUnpaidCerts']/tbody"));
                    IList<IWebElement> IUnpaidRow = IUnpaidTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> IUnpaidTD;
                    foreach (IWebElement unpaid in IUnpaidRow)
                    {
                        IUnpaidTD = unpaid.FindElements(By.TagName("td"));
                        if (IUnpaidTD.Count != 0 && !unpaid.Text.Contains("Year"))
                        {
                            strUnPaidYear = IUnpaidTD[0].Text;
                            strCurrentPayoff = IUnpaidTD[1].Text;
                            strPaidBy = IUnpaidTD[2].Text;
                            strCurrentPay = IUnpaidTD[3].Text;
                            strPaid = IUnpaidTD[4].Text;
                            try
                            {
                                IWebElement IPayment = IUnpaidTD[5].FindElement(By.TagName("a"));
                                strMakePay = IPayment.GetAttribute("href");
                                if (strMakePay != "" && strMakePay != "-")
                                {
                                    strMakePay = "PayNow";
                                }
                            }
                            catch { }
                            try
                            {
                                if (strMakePay == "" || strMakePay == "-" && strMakePay != "PayNow")
                                {
                                    strMakePay = IUnpaidTD[5].Text; ;
                                }
                            }
                            catch { }


                            string strUnpaidTaxDetails = strUnPaid.Replace(":", "") + "~" + strUnPaidYear + "~" + strCurrentPayoff + "~" + strPaidBy + "~" + strCurrentPay + "~" + strPaid + "~" + strMakePay + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-";
                            gc.insert_date(orderNumber, strParcelNumber, 356, strUnpaidTaxDetails, 1, DateTime.Now);
                            strMakePay = "";
                            try
                            {
                                gc.downloadfile(strMakePay, orderNumber, strParcelNumber, strUnPaidYear + " Unpaid Tax Bill ", "FL", "Orange");
                            }
                            catch { }
                        }
                    }

                        try
                        {
                            try
                            {
                                strRealEstate = driver.FindElement(By.XPath("//*[@id='mainContent_tblOtherCertsParent']/tbody/tr/td/strong")).Text;
                            }
                            catch { }
                            IWebElement IRealEstateTable = driver.FindElement(By.XPath("//*[@id='mainContent_tblOtherCerts']/tbody"));
                            IList<IWebElement> IRealEstateRow = IRealEstateTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> IRealEstateTD;
                            foreach (IWebElement Real in IRealEstateRow)
                            {
                                IRealEstateTD = Real.FindElements(By.TagName("td"));
                                if (IRealEstateTD.Count != 0 && !Real.Text.Contains("Year"))
                                {
                                    strRealYear = IRealEstateTD[0].Text;
                                    strRealFaceValue = IRealEstateTD[1].Text;
                                    strRealCertificate = IRealEstateTD[2].Text;
                                    strRealStatus = IRealEstateTD[3].Text;
                                    strRealAmountPaid = IRealEstateTD[4].Text;

                                    string strRealTaxDetails = strRealEstate.Replace(":","") +"~"+strRealYear + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + strRealFaceValue + "~" + strRealCertificate + "~" + strRealStatus + "~" + strRealAmountPaid;
                                    gc.insert_date(orderNumber, strParcelNumber, 356, strRealTaxDetails, 1, DateTime.Now);

                                }
                            }
                        }
                        catch { }


                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "FL", "Orange", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);


                    driver.Quit();
                    gc.mergpdf(orderNumber, "FL", "Orange");
                    return "Data Inserted Successfully";
                }

                catch (NoSuchElementException ex1)
                {
                    driver.Quit();
                    throw ex1;
                }
            }
        }
    }
}