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
using System.ComponentModel;
using System.Text;
using HtmlAgilityPack;
using iTextSharp.text;
using System.Text.RegularExpressions;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Support.Extensions;
using System.Net;
using OpenQA.Selenium.Firefox;
using System.Diagnostics;
using OpenQA.Selenium.Remote;
using Org.BouncyCastle.Utilities;
namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_FLBroward
    {
        string strMulti = "-", strParcel = "-", strMultiDetails = "-", strParcelNumber = "-", strOwner = "-", strAddress = "-", strProeprtyAddress = "-", strNeighborhood = "", strOwnerName = "-",
               strMailingAddress = "-", strLegalDiscription = "-", strYear = "-", strLand = "-", strBuilding = "-", strMarket = "-", strAssessed = "-", strTax = "-",
               strType = "-", strTaxType = "-", strAlternetKey = "-", strEscrowCode = "-", strMillageCode = "-", strTaxAuthority = "-", strMillege = "-", strAssess = "-",
               strExemption = "-", strTaxable = "-", strTaxAmount = "-", strParcelNo = "-", strTaxCom = "-", strLInk = "-", strDate = "-", strAmount = "-", strFace = "-",
               strBid = "-", strBidder = "-", strCertificate = "-", strTYear = "-", strPaid = "-", strBill = "-", strBalance = "-", strBillDate = "-", strBillPaid = "-",
               strFBill = "-", strFBalance = "-", strFBillDate = "-", strFBillPaid = "-", strbillTaxAuthority = "-", strbillMillege = "-", strbillAssess = "-",
               strbillExemption = "-", strbillTaxable = "-", strbillTaxAmount = "-", strValAuthority = "-", strValRate = "-", strValAmount = "-", strValoremAuthority = "-",
               strValoremRate = "-", strValoremAmount = "-", strAssessTitle = "-", strCounty = "-", strSchool = "-", strMunicipal = "-", strIndependent = "-", strTaxBill = "",
               strMillageRate = "-", strIssue = "", strIssuePaid = "", strissueTaxType = "", strissueAlternetKey = "", strissueMillageCode = "", strissueMillageRate = "", strissueParcelNo = "-",
               strVTaxType = "-", strNVTaxType = "-", strcombine = "-", strLTaxAuthority = "-", strLMillege = "-", strLAssess = "-", strLExemption = "-", strLTaxable = "-", strLTaxAmount = "-",
                strTaxFeed = "", strBillEff = "", strRate = "", strBillPayAmount = "", strBillPayReceipt = "";
        int MultiCount, AnualBillCount, TaxCount = 0, ParcelCount = 0;
        int currentYear = 0;
        IWebDriver driver;
        IList<IWebElement> taxPaymentdetails, taxPaymentAmountdetails, Itaxtd;
        List<string> strSecured = new List<string>();
        List<string> strCombinedTax = new List<string>();
        List<string> strTaxRealestate = new List<string>();
        List<string> strTaxRealCount = new List<string>();
        List<string> IssuedURL = new List<string>();
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();
        Amrock amc = new Amrock();
        public string FTP_FLBroward(string houseno, string sname, string strStreetType, string strDirection, string unitnumber, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            IJavaScriptExecutor js = driver as IJavaScriptExecutor;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())//PhantomJSDriver
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("http://www.bcpa.net/RecMenu.asp");
                    Thread.Sleep(3000);
                    //driver.Navigate().GoToUrl("https://web.bcpa.net/BcpaClient/#/Record-Search");
                    //StartTime = DateTime.Now.ToString("HH:mm:ss");
                    //string address = "";
                    ////if (searchType == "titleflex")
                    ////{
                    ////    address = houseno + " " + strDirection + " " + sname + " " + strStreetType + unitnumber;
                    ////    gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "FL", "Broward");

                    ////    if (HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes")
                    ////    {
                    ////        return "MultiParcel";
                    ////    }
                    ////    parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                    ////    parcelNumber = HttpContext.Current.Session["titleparcel"].ToString().Replace(".", "").Replace("-", "");
                    ////    searchType = "parcel";
                    ////}
                    //var chromeOptions = new ChromeOptions();

                    //var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];

                    //chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                    //chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                    //chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                    //chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                    //var driver1 = new ChromeDriver(chromeOptions);
                    //try
                    //{
                    //    driver1.Navigate().GoToUrl("https://web.bcpa.net/BcpaClient/#/Record-Search");
                    //    if (searchType == "address")
                    //    {
                    //        if (strDirection != "")
                    //        {
                    //            address = houseno + " " + strDirection + " " + sname + " " + strStreetType + unitnumber;
                    //        }
                    //        else
                    //        {
                    //            address = houseno + " " + sname + " " + strStreetType + unitnumber;
                    //        }
                    //        driver1.FindElement(By.Id("txtField")).SendKeys(address.Trim());
                    //        gc.CreatePdf_WOP_Chrome(orderNumber, "Address Search", driver1, "FL", "Broward");
                    //        driver1.FindElement(By.Id("searchButton")).Click();
                    //        gc.CreatePdf_WOP_Chrome(orderNumber, "Address Search Result", driver1, "FL", "Broward");
                    //        try
                    //        {
                    //            strMulti = driver1.FindElement(By.Id("tbl-list-parcelsTotalNumberOfRecordsFound")).Text;
                    //            string strMultiCount = GlobalClass.After(strMulti, "Record Found(s): ").Trim();
                    //            if (Convert.ToInt32(strMultiCount) > 25)
                    //            {
                    //                HttpContext.Current.Session["multiparcel_FLBroward_Maximum"] = "Yes";
                    //                driver1.Quit();
                    //                driver.Quit();
                    //                return "Maximum";
                    //            }
                    //            IWebElement IMulti = driver1.FindElement(By.XPath("//*[@id='tbl-list-parcels']/tbody"));
                    //            IList<IWebElement> IMultirow = IMulti.FindElements(By.XPath("tr"));
                    //            IList<IWebElement> IMultitd;
                    //            foreach (IWebElement row in IMultirow)
                    //            {
                    //                HttpContext.Current.Session["multiPArcel_FLBroward"] = "Yes";
                    //                IMultitd = row.FindElements(By.XPath("td"));
                    //                if (IMultitd.Count != 0 && !row.Text.Contains("Folio Number") && MultiCount <= 25)
                    //                {
                    //                    try
                    //                    {
                    //                        MultiCount++;
                    //                        strParcel = IMultitd[0].Text;
                    //                        strOwner = IMultitd[1].Text;
                    //                        strAddress = IMultitd[2].Text;

                    //                        strMultiDetails = strOwner.Trim() + "~" + strAddress.Trim();
                    //                        gc.insert_date(orderNumber, strParcel, 295, strMultiDetails, 1, DateTime.Now);
                    //                    }
                    //                    catch { }
                    //                }
                    //            }
                    //            driver1.Quit();
                    //            driver.Quit();
                    //            return "MultiParcel";
                    //        }
                    //        catch { }

                    //    }
                    //    else if (searchType == "parcel")
                    //    {
                    //        driver1.FindElement(By.Id("txtField")).SendKeys(parcelNumber);
                    //        gc.CreatePdf_Chrome(orderNumber, parcelNumber, "Parcel Search", driver1, "FL", "Broward");
                    //        driver1.FindElement(By.XPath("//*[@id='searchButton']/span")).Click();
                    //        gc.CreatePdf_Chrome(orderNumber, parcelNumber, "Parcel Search Result", driver1, "FL", "Broward");
                    //    }
                    //    else if (searchType == "ownername")
                    //    {
                    //        driver1.FindElement(By.Id("txtField")).SendKeys(ownername.Trim());
                    //        gc.CreatePdf_WOP_Chrome(orderNumber, "Owner Search", driver1, "FL", "Broward");
                    //        driver1.FindElement(By.Id("searchButton")).Click();
                    //        gc.CreatePdf_WOP_Chrome(orderNumber, "Owner Search Result", driver1, "FL", "Broward");
                    //        try
                    //        {
                    //            strMulti = driver1.FindElement(By.Id("tbl-list-parcelsTotalNumberOfRecordsFound")).Text;
                    //            string strMultiCount = GlobalClass.After(strMulti, "Record Found(s): ").Trim();
                    //            if (Convert.ToInt32(strMultiCount) > 25)
                    //            {
                    //                HttpContext.Current.Session["multiparcel_FLBroward_Maximum"] = "Yes";
                    //                driver1.Quit();
                    //                driver.Quit();
                    //                return "Maximum";
                    //            }
                    //            IWebElement IMulti = driver1.FindElement(By.XPath("//*[@id='tbl-list-parcels']/tbody"));
                    //            IList<IWebElement> IMultirow = IMulti.FindElements(By.XPath("tr"));
                    //            IList<IWebElement> IMultitd;
                    //            foreach (IWebElement row in IMultirow)
                    //            {
                    //                HttpContext.Current.Session["multiPArcel_FLBroward"] = "Yes";
                    //                IMultitd = row.FindElements(By.XPath("td"));
                    //                if (IMultitd.Count != 0 && !row.Text.Contains("Folio Number") && MultiCount <= 25)
                    //                {
                    //                    try
                    //                    {
                    //                        MultiCount++;
                    //                        strParcel = IMultitd[0].Text;
                    //                        strOwner = IMultitd[1].Text;
                    //                        strAddress = IMultitd[2].Text;

                    //                        strMultiDetails = strOwner.Trim() + "~" + strAddress.Trim();
                    //                        gc.insert_date(orderNumber, strParcel, 295, strMultiDetails, 1, DateTime.Now);
                    //                    }
                    //                    catch { }
                    //                }
                    //            }
                    //            driver1.Quit();
                    //            driver.Quit();
                    //            return "MultiParcel";
                    //        }
                    //        catch { }
                    //    }

                    //    try
                    //    {
                    //        IWebElement INodata = driver1.FindElement(By.Id("noRecordFound"));
                    //        if (INodata.Text.Contains("No record found"))
                    //        {
                    //            HttpContext.Current.Session["Nodata_BrowardFL"] = "Yes";
                    //            driver1.Quit();
                    //            driver.Quit();
                    //            return "No Data Found";
                    //        }
                    //    }
                    //    catch { }

                    //    //Property Details 
                    //    strParcelNumber = driver1.FindElement(By.XPath("//*[@id='folioNumberId']/span/a")).Text;
                    //    parcelNumber = strParcelNumber;
                    //    IWebElement IPropertyOwnerAddress = driver1.FindElement(By.XPath("//*[@id='hideRecInfoTab']/div[7]/div[1]/table[1]"));
                    //    IList<IWebElement> IPropertyOwnerAddressRow = IPropertyOwnerAddress.FindElements(By.TagName("tr"));
                    //    IList<IWebElement> IPropertyOwnerAddresstd;
                    //    foreach (IWebElement owner in IPropertyOwnerAddressRow)
                    //    {
                    //        IPropertyOwnerAddresstd = owner.FindElements(By.TagName("td"));
                    //        if (IPropertyOwnerAddresstd.Count != 0 && owner.Text.Contains("Property Owner/s:"))
                    //        {
                    //            strOwnerName = IPropertyOwnerAddresstd[1].Text.Replace("\r\n", " ").Trim();
                    //        }
                    //        if (IPropertyOwnerAddresstd.Count != 0 && owner.Text.Contains("Mailing Address:"))
                    //        {
                    //            strMailingAddress = GlobalClass.Before(IPropertyOwnerAddresstd[1].Text, "click").Replace("\r\n", " ").Trim();
                    //        }
                    //    }
                    //    try
                    //    {
                    //        ByVisibleElement(driver1.FindElement(By.XPath("//*[@id='hideRecInfoTab']/div[5]")), driver1);
                    //        gc.CreatePdf_Chrome(orderNumber, strParcelNumber, "Property Parcel Result", driver1, "FL", "Broward");
                    //    }
                    //    catch { }
                    //    try
                    //    {
                    //        ByVisibleElement(driver1.FindElement(By.XPath("//*[@id='searchURL']")), driver1);
                    //        gc.CreatePdf_Chrome(orderNumber, strParcelNumber, "Property Basic Result", driver1, "FL", "Broward");
                    //    }
                    //    catch { }

                    //    IWebElement IPropertyAddress = driver1.FindElement(By.XPath("//*[@id='hideRecInfoTab']/div[7]/div[1]/table[2]"));
                    //    IList<IWebElement> IPropertyAddressRow = IPropertyAddress.FindElements(By.TagName("tr"));
                    //    IList<IWebElement> IPropertyAddresstd;
                    //    foreach (IWebElement Iaddress in IPropertyAddressRow)
                    //    {
                    //        IPropertyAddresstd = Iaddress.FindElements(By.TagName("td"));
                    //        if (IPropertyAddresstd.Count != 0 && Iaddress.Text.Contains("Physical Address:"))
                    //        {
                    //            strProeprtyAddress = IPropertyAddresstd[1].Text.Replace("\r\n", " ").Trim();
                    //        }
                    //        if (IPropertyAddresstd.Count != 0 && Iaddress.Text.Contains("Neighborhood:"))
                    //        {
                    //            strNeighborhood = IPropertyAddresstd[1].Text.Replace("\r\n", " ").Trim();
                    //        }
                    //    }
                    //    string strPropertyUse = "", strMillageCode = "", strAdjBldg = "", strBdgUnder = "", strEffectiveYear = "", strYearBuilt = "", strUnits = "";
                    //    try
                    //    {
                    //        ByVisibleElement(driver1.FindElement(By.XPath("//*[@id='hideRecInfoTab']/div[7]/div[1]/table[3]")), driver1);
                    //        gc.CreatePdf_Chrome(orderNumber, strParcelNumber, "Property Use Result", driver1, "FL", "Broward");
                    //    }
                    //    catch { }
                    //    IWebElement IPropertyCode = driver1.FindElement(By.XPath("//*[@id='hideRecInfoTab']/div[7]/div[1]/table[3]"));
                    //    IList<IWebElement> IPropertyCodeRow = IPropertyCode.FindElements(By.TagName("tr"));
                    //    IList<IWebElement> IPropertyCodetd;
                    //    foreach (IWebElement code in IPropertyCodeRow)
                    //    {
                    //        IPropertyCodetd = code.FindElements(By.TagName("td"));
                    //        if (IPropertyCodetd.Count != 0 && code.Text.Contains("Property Use:"))
                    //        {
                    //            strPropertyUse = IPropertyCodetd[1].Text.Replace("\r\n", " ").Trim();
                    //        }
                    //        if (IPropertyCodetd.Count != 0 && code.Text.Contains("Millage Code:"))
                    //        {
                    //            strMillageCode = IPropertyCodetd[1].Text.Replace("\r\n", " ").Trim();
                    //        }
                    //        if (IPropertyCodetd.Count != 0 && code.Text.Contains("Adj. Bldg. S.F.:"))
                    //        {
                    //            strAdjBldg = IPropertyCodetd[1].Text.Replace("\r\n", " ").Trim();
                    //        }
                    //        if (IPropertyCodetd.Count != 0 && code.Text.Contains("Bldg Under Air S.F.:"))
                    //        {
                    //            strBdgUnder = IPropertyCodetd[1].Text.Replace("\r\n", " ").Trim();
                    //        }
                    //        if (IPropertyCodetd.Count != 0 && code.Text.Contains("Effective Year:"))
                    //        {
                    //            strEffectiveYear = IPropertyCodetd[1].Text.Replace("\r\n", " ").Trim();
                    //        }
                    //        if (IPropertyCodetd.Count != 0 && code.Text.Contains("Year Built:"))
                    //        {
                    //            strYearBuilt = IPropertyCodetd[1].Text.Replace("\r\n", " ").Trim();
                    //        }
                    //        if (IPropertyCodetd.Count != 0 && code.Text.Contains("Units/Beds/Baths:"))
                    //        {
                    //            strUnits = IPropertyCodetd[1].Text.Replace("\r\n", " ").Trim();
                    //        }
                    //    }

                    //    try
                    //    {
                    //        ByVisibleElement(driver1.FindElement(By.XPath("//*[@id='hideRecInfoTab']/div[10]/table")), driver1);
                    //        gc.CreatePdf_Chrome(orderNumber, strParcelNumber, "Property Legal Result", driver1, "FL", "Broward");
                    //    }
                    //    catch { }
                    //    strLegalDiscription = driver1.FindElement(By.Id("legalDescId")).Text.Replace("\r\n", " ").Trim();

                    //    string strPropertyDetails = strProeprtyAddress.Trim() + "~" + strOwnerName.Trim() + "~" + strMailingAddress + "~" + strLegalDiscription + "~" + strNeighborhood + "~" + strPropertyUse + "~" + strMillageCode + "~" + strAdjBldg + "~" + strBdgUnder + "~" + strEffectiveYear + "~" + strYearBuilt + "~" + strUnits;
                    //    gc.insert_date(orderNumber, strParcelNumber, 293, strPropertyDetails, 1, DateTime.Now);

                    //    try
                    //    {
                    //        ByVisibleElement(driver1.FindElement(By.XPath("//*[@id='RecinfoPropertyAssessmentId']")), driver1);
                    //        gc.CreatePdf_Chrome(orderNumber, strParcelNumber, "Property Assessment Result", driver1, "FL", "Broward");
                    //    }
                    //    catch { }
                    //    //Property Assessment
                    //    string strAssessYear = "", strLand = "", strBuilding = "", strMarkValue = "", strAssessed = "", strAssessTax = "";
                    //    IWebElement IPropertyAssessment = driver1.FindElement(By.XPath("//*[@id='hideRecInfoTab']/div[12]/div/table/tbody"));
                    //    IList<IWebElement> IPropertyAssessmentRow = IPropertyAssessment.FindElements(By.TagName("tr"));
                    //    IList<IWebElement> IPropertyAssessmenttd;
                    //    foreach (IWebElement assessment in IPropertyAssessmentRow)
                    //    {
                    //        IPropertyAssessmenttd = assessment.FindElements(By.TagName("td"));
                    //        if (IPropertyAssessmenttd.Count != 0 && !assessment.Text.Contains("Land"))
                    //        {
                    //            string strPropertyAssessment = IPropertyAssessmenttd[0].Text.Trim() + "~" + IPropertyAssessmenttd[1].Text.Trim() + "~" + IPropertyAssessmenttd[2].Text.Trim() + "~" + IPropertyAssessmenttd[4].Text + "~" + IPropertyAssessmenttd[5].Text.Trim() + "~" + IPropertyAssessmenttd[6].Text.Trim();
                    //            gc.insert_date(orderNumber, strParcelNumber, 294, strPropertyAssessment, 1, DateTime.Now);
                    //        }
                    //    }
                    //    try
                    //    {
                    //        ByVisibleElement(driver1.FindElement(By.XPath("//*[@id='hideRecInfoTab']/div[14]")), driver1);
                    //        gc.CreatePdf_Chrome(orderNumber, strParcelNumber, "Exemptions And Taxing Authority Result", driver1, "FL", "Broward");
                    //    }
                    //    catch { }
                    //    //Exemptions And Taxing Authority Information
                    //    string strTitle = "", strCounty = "", strSchool = "", strMunicipal = "", strIndependent = "";
                    //    IWebElement IExemptions = driver1.FindElement(By.XPath("//*[@id='mytable']/tbody[2]"));
                    //    IList<IWebElement> IExemptionsRow = IExemptions.FindElements(By.TagName("tr"));
                    //    IList<IWebElement> IExemptionstd;
                    //    foreach (IWebElement exemption in IExemptionsRow)
                    //    {
                    //        IExemptionstd = exemption.FindElements(By.TagName("td"));
                    //        if (IExemptionstd.Count != 0 && !exemption.Text.Contains("Land") && exemption.Text.Trim() != "")
                    //        {
                    //            strTitle += IExemptionstd[0].Text.Trim() + "~";
                    //            strCounty += IExemptionstd[1].Text.Trim() + "~";
                    //            strSchool += IExemptionstd[2].Text + "~";
                    //            strMunicipal += IExemptionstd[3].Text.Trim() + "~";
                    //            strIndependent += IExemptionstd[4].Text.Trim() + "~";
                    //        }
                    //    }

                    //    db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + strTitle.Remove(strTitle.Length - 1, 1) + "' where Id = '" + 400 + "'");
                    //    gc.insert_date(orderNumber, strParcelNumber, 400, strCounty.Remove(strCounty.Length - 1, 1), 1, DateTime.Now);
                    //    gc.insert_date(orderNumber, strParcelNumber, 400, strSchool.Remove(strSchool.Length - 1, 1), 1, DateTime.Now);
                    //    gc.insert_date(orderNumber, strParcelNumber, 400, strMunicipal.Remove(strMunicipal.Length - 1, 1), 1, DateTime.Now);
                    //    gc.insert_date(orderNumber, strParcelNumber, 400, strIndependent.Remove(strIndependent.Length - 1, 1), 1, DateTime.Now);

                    //    //Sales History
                    //    IWebElement ISaleHitory = driver1.FindElement(By.XPath("//*[@id='hideRecInfoTab']/div[18]/div/table/tbody"));
                    //    IList <IWebElement> ISaleHitoryRow = ISaleHitory.FindElements(By.TagName("tr"));
                    //    IList<IWebElement> ISaleHitorytd;
                    //    foreach (IWebElement history in ISaleHitoryRow)
                    //    {
                    //        ISaleHitorytd = history.FindElements(By.TagName("td"));
                    //        if (ISaleHitorytd.Count != 0 && !history.Text.Contains("Date"))
                    //        {
                    //            string strSaleHistory = ISaleHitorytd[0].Text.Trim() + "~" + ISaleHitorytd[1].Text.Trim() + "~" + ISaleHitorytd[2].Text.Trim() + "~" + ISaleHitorytd[3].Text + "~" + ISaleHitorytd[4].Text.Trim();
                    //            gc.insert_date(orderNumber, strParcelNumber, 2096, strSaleHistory, 1, DateTime.Now);
                    //        }
                    //    }

                    //    try
                    //    {
                    //        ByVisibleElement(driver1.FindElement(By.XPath("//*[@id='hideRecInfoTab']/div[17]")), driver1);
                    //        gc.CreatePdf_Chrome(orderNumber, strParcelNumber, "Sales History Result", driver1, "FL", "Broward");
                    //    }
                    //    catch { }

                    //    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    //    driver1.Quit();
                    //}
                    //catch
                    //{
                    //    driver1.Quit();
                    //}

                    if (searchType == "address")
                    {
                        if (sname.Any(char.IsDigit))
                        {
                            sname = Regex.Match(sname, @"\d+").Value;
                        }

                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr/td/table/tbody/tr/td[1]/p[4]/a/img")).Click();
                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/form/table/tbody/tr/td[1]/table[1]/tbody/tr[2]/td[2]/input")).SendKeys(houseno);
                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/form/table/tbody/tr/td[1]/table[1]/tbody/tr[2]/td[4]/input")).SendKeys(sname);


                        if (strDirection.Trim() != "")
                        {
                            strDirection = strDirection.ToUpper();
                            IWebElement IDirection = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/form/table/tbody/tr/td[1]/table[1]/tbody/tr[2]/td[3]/select"));
                            SelectElement sDirection = new SelectElement(IDirection);
                            sDirection.SelectByText(strDirection);
                            strDirection = "";
                        }
                        if (strStreetType.Trim() != "")
                        {
                            string street = strStreetType.Substring(0, 1);
                            string strstreet = strStreetType.Substring(1, strStreetType.Length - 1);
                            string strStType = street.ToUpper() + strstreet.ToLower();
                            IWebElement IStreetType = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/form/table/tbody/tr/td[1]/table[1]/tbody/tr[2]/td[5]/select"));
                            try
                            {
                                SelectElement sStreetType = new SelectElement(IStreetType);
                                sStreetType.SelectByText(strStType);
                            }
                            catch { }
                            try
                            {
                                SelectElement sStrType = new SelectElement(IStreetType);
                                sStrType.SelectByValue(strStType.ToUpper());
                            }
                            catch { }
                            strStreetType = "";
                        }
                        if (unitnumber.Trim() != "")
                        {
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/form/table/tbody/tr/td[1]/table[1]/tbody/tr[2]/td[7]/input")).SendKeys(unitnumber);
                        }
                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "FL", "Broward");
                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/form/table/tbody/tr/td[1]/table[2]/tbody/tr/td/a[2]/img")).Click();
                        gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "FL", "Broward");
                        try
                        {
                            strMulti = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr[1]/td/form/div[2]/table[2]/tbody/tr/td[2]")).Text;
                            string strMultiCount = GlobalClass.Before(strMulti, " Records Found");
                            if (Convert.ToInt32(strMultiCount) > 25)
                            {
                                HttpContext.Current.Session["multiparcel_FLBroward_Maximum"] = "Yes";
                                driver.Quit();
                                return "Maximum";
                            }
                            IWebElement IMulti = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr[1]/td/form/div[2]/table[3]/tbody"));
                            IList<IWebElement> IMultirow = IMulti.FindElements(By.XPath("tr"));
                            IList<IWebElement> IMultitd;
                            foreach (IWebElement row in IMultirow)
                            {
                                //GlobalClass.multiPArcel_FLBroward = "Yes";
                                HttpContext.Current.Session["multiPArcel_FLBroward"] = "Yes";
                                IMultitd = row.FindElements(By.XPath("td"));
                                if (IMultitd.Count != 0 && !row.Text.Contains("Folio Number") && MultiCount <= 25)
                                {
                                    try
                                    {
                                        MultiCount++;
                                        strParcel = IMultitd[0].Text;
                                        strOwner = IMultitd[1].Text;
                                        strAddress = IMultitd[2].Text;

                                        strMultiDetails = strOwner.Trim() + "~" + strAddress.Trim();
                                        gc.insert_date(orderNumber, strParcel, 295, strMultiDetails, 1, DateTime.Now);
                                    }
                                    catch { }
                                }
                            }
                            driver.Quit();
                            return "MultiParcel";
                        }
                        catch { }

                    }
                    else if (searchType == "parcel")
                    {
                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr/td/table/tbody/tr/td[1]/p[6]/a/img")).Click();
                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/div/form/table/tbody/tr/td/table/tbody/tr[2]/td[2]/input")).SendKeys(parcelNumber.Replace("-", ""));
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "FL", "Broward");
                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/div/form/table/tbody/tr/td/table/tbody/tr[3]/td/input[2]")).Click();
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Result", driver, "FL", "Broward");
                    }
                    else if (searchType == "ownername")
                    {
                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr/td/table/tbody/tr/td[1]/p[3]/a/img")).Click();
                        driver.FindElement(By.XPath("//*[@id='Text1']")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "Owner Search", driver, "FL", "Broward");
                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/div/form/table/tbody/tr/td/table/tbody/tr[4]/td/a[2]/img")).Click();
                        gc.CreatePdf_WOP(orderNumber, "Owner Search Result", driver, "FL", "Broward");
                        try
                        {
                            strMulti = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr[1]/td/form/div[2]/table[2]/tbody/tr/td[2]/font/b")).Text;
                            string strMultiCount = GlobalClass.Before(strMulti, " Records Found");
                            if (Convert.ToInt32(strMultiCount) > 25)
                            {
                                HttpContext.Current.Session["multiparcel_FLBroward_Maximum"] = "Yes";
                                driver.Quit();
                                return "Maximum";
                            }
                            IWebElement IMulti = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr[1]/td/form/div[2]/table[3]/tbody"));
                            IList<IWebElement> IMultirow = IMulti.FindElements(By.XPath("tr"));
                            IList<IWebElement> IMultitd;
                            foreach (IWebElement row in IMultirow)
                            {
                                HttpContext.Current.Session["multiPArcel_FLBroward"] = "Yes";
                                // GlobalClass.multiPArcel_FLBroward = "Yes";
                                IMultitd = row.FindElements(By.XPath("td"));
                                if (IMultitd.Count != 0 && !row.Text.Contains("Folio Number") && MultiCount <= 25)
                                {
                                    try
                                    {
                                        MultiCount++;
                                        strParcel = IMultitd[0].Text;
                                        strOwner = IMultitd[1].Text;
                                        strAddress = IMultitd[2].Text;

                                        strMultiDetails = strOwner.Trim() + "~" + strAddress.Trim();
                                        gc.insert_date(orderNumber, strParcel, 295, strMultiDetails, 1, DateTime.Now);
                                    }
                                    catch { }
                                }
                            }
                            driver.Quit();
                            return "MultiParcel";
                        }
                        catch { }
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.Id("Table5"));
                        if (INodata.Text.Contains("No records found"))
                        {
                            HttpContext.Current.Session["Nodata_BrowardFL"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    //Property Details 
                    string TotalAasessed = "", YearBuilt = "";
                    strProeprtyAddress = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table/tbody/tr[1]/td[1]/table[1]/tbody/tr/td[1]/table/tbody/tr[1]/td[2]")).Text;
                    strParcelNumber = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table/tbody/tr[1]/td[1]/table[1]/tbody/tr/td[3]/table/tbody/tr[1]/td[2]")).Text;
                    parcelNumber = strParcelNumber;
                    amc.TaxId = parcelNumber;
                    strOwnerName = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table/tbody/tr[1]/td[1]/table[1]/tbody/tr/td[1]/table/tbody/tr[2]/td[2]")).Text;
                    strMailingAddress = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table/tbody/tr[1]/td[1]/table[1]/tbody/tr/td[1]/table/tbody/tr[3]/td[2]")).Text;
                    strLegalDiscription = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table/tbody/tr[1]/td[1]/table[3]/tbody/tr/td[2]")).Text;

                    string strPropertyDetails = strProeprtyAddress.Trim() + "~" + strOwnerName.Trim() + "~" + strMailingAddress + "~" + strLegalDiscription;
                    gc.insert_date(orderNumber, strParcelNumber, 293, strPropertyDetails, 1, DateTime.Now);

                    gc.CreatePdf(orderNumber, strParcelNumber, "Property Search Result", driver, "FL", "Broward");

                    IWebElement IProTable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table/tbody/tr[1]/td[1]/table[5]/tbody"));
                    IList<IWebElement> IProTableRow = IProTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> IProTabletd;
                    foreach (IWebElement ass in IProTableRow)
                    {
                        IProTabletd = ass.FindElements(By.TagName("td"));
                        if (IProTabletd.Count != 0 && !ass.Text.Contains("Property Assessment Values") && !ass.Text.Contains("Year") && (!ass.Text.Contains("Click here to see") && !ass.Text.Contains("Exemptions and Taxable Values as reflected")))
                        {
                            strYear = IProTabletd[0].Text;
                            strLand = IProTabletd[1].Text;
                            strBuilding = IProTabletd[2].Text;
                            strMarket = IProTabletd[3].Text;
                            strAssessed = IProTabletd[4].Text;
                            strTax = IProTabletd[5].Text;

                            string strAssessmentDetails = strYear + "~" + strLand + "~" + strBuilding + "~" + strMarket + "~" + strAssessed + "~" + strTax;
                            gc.insert_date(orderNumber, strParcelNumber, 294, strAssessmentDetails, 1, DateTime.Now);
                        }
                        //AMC Total Assessed Value
                        try
                        {
                            if (IProTabletd.Count != 0 && !ass.Text.Contains("Property Assessment Values") && !ass.Text.Contains("Year") && (!ass.Text.Contains("Click here to see") && !ass.Text.Contains("Exemptions and Taxable Values as reflected")))
                            {
                                string tagname = IProTabletd[0].GetAttribute("bgcolor");
                                if (tagname.Contains("#FFFF00"))
                                {
                                    TotalAasessed = IProTabletd[3].Text;
                                }
                            }
                        }
                        catch { }
                    }

                    IWebElement IAssessTax = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table/tbody/tr[1]/td[1]/table[7]/tbody"));
                    IList<IWebElement> IAssessTaxRow = IAssessTax.FindElements(By.TagName("tr"));
                    IList<IWebElement> IAssessTD;
                    foreach (IWebElement Assess in IAssessTaxRow)
                    {
                        IAssessTD = Assess.FindElements(By.TagName("td"));
                        if (IAssessTD.Count != 0 && !Assess.Text.Contains("Exemptions and Taxable Values by Taxing Authority") && !Assess.Text.Contains("County"))
                        {
                            strAssessTitle = IAssessTD[0].Text;
                            strCounty = IAssessTD[1].Text;
                            strSchool = IAssessTD[2].Text;
                            strMunicipal = IAssessTD[3].Text;
                            strIndependent = IAssessTD[4].Text;


                            string strTaxAssess = strAssessTitle + "~" + strCounty + "~" + strSchool + "~" + strMunicipal + "~" + strIndependent;
                            gc.insert_date(orderNumber, strParcelNumber, 400, strTaxAssess, 1, DateTime.Now);
                        }
                    }

                    //AMC Year Built
                    try
                    {
                        IWebElement IYearBuilt = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table/tbody/tr[1]/td[1]/table[9]/tbody/tr/td[2]/table"));
                        IList<IWebElement> IYearBuiltRow = IYearBuilt.FindElements(By.TagName("tr"));
                        IList<IWebElement> IYearBuiltTD;
                        foreach (IWebElement year in IYearBuiltRow)
                        {
                            IYearBuiltTD = year.FindElements(By.TagName("td"));
                            if (IYearBuiltTD.Count != 0)
                            {
                                YearBuilt = IYearBuiltTD[0].Text.Replace("Eff./Act. Year Built", "").Replace(":", "").Trim();
                            }
                        }
                    }
                    catch { }

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");


                    //Tax Details 
                    driver.Navigate().GoToUrl("https://broward.county-taxes.com/public/search?search_query=" + parcelNumber.Replace(" ", "").Replace("-", "").Trim() + "&category=all");
                    // gc.CreatePdf(orderNumber, strParcelNumber, "Tax Search", driver, "FL", "Broward");
                    Thread.Sleep(6000);
                    try
                    {
                        gc.CreatePdf(orderNumber, strParcelNumber, "Tax Search Result", driver, "FL", "Broward");
                        IWebElement element = driver.FindElement(By.XPath("//*[@id='search-controls']/div/form[1]/div[1]/div/span/button"));
                        js.ExecuteScript("arguments[0].click();", element);
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, strParcelNumber, "Tax Search Result", driver, "FL", "Broward");
                    }
                    catch (Exception e)
                    { }

                    try
                    {
                        IWebElement ITaxSearch = driver.FindElement(By.LinkText("Full bill history"));
                        string strITaxSearch = ITaxSearch.GetAttribute("href");
                        driver.Navigate().GoToUrl(strITaxSearch);
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, strParcelNumber, "Tax Detailed Result", driver, "FL", "Broward");
                    }
                    catch { }

                    try
                    {
                        IWebElement ITaxReal = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table"));
                        IList<IWebElement> ITaxRealRow = ITaxReal.FindElements(By.TagName("tr"));
                        IList<IWebElement> ITaxRealTd;
                        foreach (IWebElement ItaxReal in ITaxRealRow)
                        {
                            ITaxRealTd = ItaxReal.FindElements(By.TagName("td"));
                            if (ITaxRealTd.Count != 0 && ItaxReal.Text != "Bill")
                            {
                                if (ItaxReal.Text.Contains("Annual Bill"))
                                {
                                    AnualBillCount++;
                                }
                                if ((ItaxReal.Text.Contains("Annual Bill") || ItaxReal.Text.Contains("Pay this bill:")) && !ItaxReal.Text.Contains("Redeemed certificate") && AnualBillCount <= 3)
                                {
                                    IWebElement ITaxBillCount = ITaxRealTd[0].FindElement(By.TagName("a"));
                                    string strTaxReal = ITaxBillCount.GetAttribute("href");
                                    strTaxRealestate.Add(strTaxReal);
                                }
                            }
                        }
                        //Tax History Details
                        taxHistory(orderNumber, strParcelNumber);
                    }
                    catch { }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "FL", "Broward", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);


                    driver.Quit();
                    gc.mergpdf(orderNumber, "FL", "Broward");
                    return "Data Inserted Successfully";
                }
                catch (Exception ex)
                {
                    driver.Quit();
                    throw ex;
                }
            }
        }

        public void taxHistory(string orderNumber, string strParcelNumber)
        {

            IWebElement IBillHistorytable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table"));
            IList<IWebElement> IBillHistoryRow = IBillHistorytable.FindElements(By.TagName("tr"));
            IList<IWebElement> IBillHistoryTD;
            foreach (IWebElement bill in IBillHistoryRow)
            {
                IBillHistoryTD = bill.FindElements(By.TagName("td"));
                if (IBillHistoryTD.Count != 0 && bill.Text.Trim() != "" && (!bill.Text.Contains("Certificate issued") && !bill.Text.Contains("Advertisement file created") && !(bill.Text.Contains("Certificate redeemed"))))
                {
                    try
                    {
                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                        strBalance = IBillHistoryTD[1].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                        strBillDate = IBillHistoryTD[2].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                        if (strBillDate.Contains("Effective"))
                        {
                            strBillDate = GlobalClass.Before(IBillHistoryTD[2].Text, "Effective");
                            strBillEff = GlobalClass.After(IBillHistoryTD[2].Text, "Effective");
                        }
                        strBillPaid = IBillHistoryTD[3].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                        if (strBillPaid.Contains("Paid") || strBillPaid.Contains("Receipt"))
                        {
                            strBillPaid = gc.Between(strBillPaid, "Paid ", " Receipt");
                            strBillPayReceipt = GlobalClass.After(strBillPaid, "Receipt ");
                        }
                    }
                    catch
                    {
                        strBillDate = "";
                        strBillPaid = "";
                    }
                    if (strBillPaid.Contains("Print (PDF)"))
                    {
                        strBillPaid = "";
                    }

                    if (strBillPaid.Contains("Deed applied"))
                    {
                        strBillPayReceipt = strBillPaid;
                        strBillPaid = "";
                    }
                    string strTaxHistory = strBill + "~" + "" + "~" + "" + "~" + strBalance + "~" + strBillDate + "~" + strBillEff + "~" + strBillPaid + "~" + strBillPayReceipt;
                    gc.insert_date(orderNumber, strParcelNumber, 300, strTaxHistory, 1, DateTime.Now);
                }
                if (IBillHistoryTD.Count != 0 && ((bill.Text.Contains("Certificate issued") || bill.Text.Contains("Advertisement file created")) || (bill.Text.Contains("Certificate redeemed"))))
                {
                    try
                    {
                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                        if (!strBill.Contains("Redeemed certificate") && !strBill.Contains("Issued certificate") && !strBill.Contains("Certificate issued") && !strBill.Contains("Advertisement file created") && !strBill.Contains("Certificate redeemed"))
                        {
                            strBill = "";
                        }
                        string strBillFaceRate = IBillHistoryTD[1].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                        if (strBillFaceRate.Contains("Face") || strBillFaceRate.Contains("Rate"))
                        {
                            strFace = gc.Between(IBillHistoryTD[1].Text, "Face ", "\r\nRate");
                            strRate = GlobalClass.After(IBillHistoryTD[1].Text, "Rate ");
                            strBillDate = IBillHistoryTD[2].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                            if (strBillDate.Contains("Effective"))
                            {
                                strBillDate = GlobalClass.Before(IBillHistoryTD[2].Text, "Effective");
                                strBillEff = GlobalClass.After(IBillHistoryTD[2].Text, "Effective");
                            }
                            strBillPayReceipt = IBillHistoryTD[3].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                        }
                        if (!strBillFaceRate.Contains("Face") || !strBillFaceRate.Contains("Rate") && IBillHistoryTD.Count == 2)
                        {
                            strBillDate = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                            if (strBillDate.Contains("Effective"))
                            {
                                strBillDate = GlobalClass.Before(IBillHistoryTD[2].Text, "Effective");
                                strBillEff = GlobalClass.After(IBillHistoryTD[2].Text, "Effective");
                            }
                            strBillPayReceipt = IBillHistoryTD[1].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                        }

                    }
                    catch
                    {
                        strBillDate = "";
                        strBillPayReceipt = "";
                    }
                    if (strBillPayReceipt.Contains("Print (PDF)"))
                    {
                        strBillPayReceipt = "";
                    }
                    string strTaxHistory = strBill + "~" + strFace + "~" + strRate + "~" + strBalance + "~" + strBillDate + "~" + strBillEff + "~" + strBillPaid + "~" + strBillPayReceipt;
                    gc.insert_date(orderNumber, strParcelNumber, 300, strTaxHistory, 1, DateTime.Now);
                }
                strBill = ""; strFace = ""; strRate = ""; strBalance = ""; strBillDate = ""; strBillEff = ""; strBillPaid = ""; strBillPayReceipt = "";
            }
            IWebElement IBillHistoryfoottable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table/tfoot"));
            IList<IWebElement> IBillHistoryfootRow = IBillHistoryfoottable.FindElements(By.TagName("tr"));
            IList<IWebElement> IBillHistoryfootTD;
            foreach (IWebElement bill in IBillHistoryRow)
            {
                IBillHistoryfootTD = bill.FindElements(By.TagName("th"));
                if (IBillHistoryfootTD.Count != 0 && bill.Text.Contains("Total"))
                {
                    try
                    {
                        strFBill = IBillHistoryfootTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                        strFBalance = IBillHistoryfootTD[1].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                        strFBillDate = IBillHistoryfootTD[2].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                        strFBillPaid = IBillHistoryfootTD[3].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                    }
                    catch
                    {
                        strFBillDate = "";
                        strFBillPaid = "";
                    }
                    if (strBillPaid.Contains("Print (PDF)"))
                    {
                        strBillPaid = "";
                    }
                    string strTaxHistory = strFBill + "~" + "" + "~" + "" + "~" + strFBalance + "~" + strFBillDate + "~" + "" + "~" + strFBillPaid + "~" + "";
                    gc.insert_date(orderNumber, strParcelNumber, 300, strTaxHistory, 1, DateTime.Now);
                }
            }

            foreach (string real in strTaxRealestate)
            {
                try
                {
                    driver.Navigate().GoToUrl(real);
                    Thread.Sleep(4000);
                    try
                    {
                        string strPaidDeatil = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[3]/form/button")).Text;
                        strPaid = GlobalClass.Before(strPaidDeatil, ":");
                    }
                    catch { }
                    try
                    {
                        //*[@id="content"]/div[1]/div[7]/div/div[1]/div[2]/form/button
                        strType = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[1]/div[2]")).Text;
                        strTaxType = GlobalClass.Before(strType, "\r\nPrint this bill (PDF)");
                        amc.TaxYear = gc.Between(strTaxType, "Real Estate ", " Annual bill");
                    }
                    catch { }

                    try
                    {
                        strIssuePaid = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[3]/div[1]/form/div")).Text;
                    }
                    catch { }
                    try
                    {
                        strIssue = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[3]/div[1]/form/button")).Text;
                    }
                    catch { }
                    try
                    {
                        strEscrowCode = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[1]/tbody/tr/td[3]")).Text;
                    }
                    catch { }
                    try
                    {
                        strTaxBill = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[3]/form/div/div/div")).Text;
                    }
                    catch { }

                    try
                    {
                        strTaxFeed = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[3]/div[1]/div/div")).Text;
                    }
                    catch { }
                    //Latest Bill
                    if (strPaid.Contains("Pay this bill") || strTaxBill.Contains("PAID") || strTaxFeed.Contains("Tax Deed") || strTaxBill.Contains("Cannot be paid online"))
                    {
                        try
                        {
                            //IWebElement ITaxSearch = driver.FindElement(By.LinkText("Latest bill"));
                            //string strITaxSearch = ITaxSearch.GetAttribute("href");
                            //driver.Navigate().GoToUrl(strITaxSearch);
                            gc.CreatePdf(orderNumber, strParcelNumber, "Tax Detailed Result Latest Bill" + strTaxType, driver, "FL", "Broward");
                            try
                            {
                                IWebElement Iurl = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[1]/div[2]/form"));
                                string strURL = Iurl.GetAttribute("action");
                                gc.downloadfile(strURL, orderNumber, strParcelNumber, "Paid Billl Reciept" + strTaxType + "", "FL", "Broward");
                            }
                            catch { }
                        }
                        catch { }
                        if (TaxCount < 1)
                        {
                            TaxCount++;
                            //Latest Bill Details
                            try
                            {
                                strVTaxType = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[2]/caption")).Text;
                            }
                            catch { }
                            //*[@id="content"]/div[1]/div[7]/div/table[2]
                            IWebElement ITax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[2]/tbody"));
                            IList<IWebElement> ITaxRow = ITax.FindElements(By.TagName("tr"));
                            IList<IWebElement> ITaxTD;
                            foreach (IWebElement row in ITaxRow)
                            {
                                ITaxTD = row.FindElements(By.TagName("td"));
                                if (ITaxTD.Count != 0 && ITaxTD.Count == 6)
                                {
                                    try
                                    {
                                        strLTaxAuthority = ITaxTD[0].Text;
                                        strLMillege = ITaxTD[1].Text;
                                        strLAssess = ITaxTD[2].Text;
                                        strLExemption = ITaxTD[3].Text;
                                        strLTaxable = ITaxTD[4].Text;
                                        strLTaxAmount = ITaxTD[5].Text;
                                    }
                                    catch
                                    { }

                                    string strTaxDetails = strVTaxType + "~" + strTaxType + "~" + strLTaxAuthority + "~" + strLMillege + "~" + strLAssess + "~" + strLExemption + "~" + strLTaxable + "~" + strLTaxAmount;
                                    gc.insert_date(orderNumber, strParcelNumber, 297, strTaxDetails, 1, DateTime.Now);

                                }
                                if (ITaxTD.Count != 0 && ITaxTD.Count < 6)
                                {
                                    try
                                    {
                                        strTaxAuthority = ITaxTD[0].Text;
                                        strMillege = ITaxTD[1].Text;
                                        strAssess = ITaxTD[2].Text;
                                        strExemption = ITaxTD[3].Text;
                                        strTaxable = ITaxTD[4].Text;
                                        strTaxAmount = ITaxTD[5].Text;
                                    }
                                    catch
                                    { }

                                    string strTaxDetails = strVTaxType + "~" + strTaxType + "~" + strTaxAuthority + "~" + strMillege + "~" + strAssess + "~" + strExemption + "~" + strTaxable + "~" + strTaxAmount;
                                    gc.insert_date(orderNumber, strParcelNumber, 297, strTaxDetails, 1, DateTime.Now);

                                    strTaxAuthority = ""; strMillege = ""; strAssess = ""; strExemption = ""; strTaxable = ""; strTaxAmount = "";
                                }
                            }

                            IWebElement ITaxBill = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[2]/tfoot"));
                            IList<IWebElement> ITaxBillRow = ITaxBill.FindElements(By.TagName("tr"));
                            IList<IWebElement> ITaxBillTD;
                            foreach (IWebElement bill in ITaxBillRow)
                            {
                                ITaxBillTD = bill.FindElements(By.TagName("td"));
                                if (ITaxBillTD.Count != 0 && bill.Text.Contains("Total"))
                                {
                                    try
                                    {
                                        strbillTaxAuthority = ITaxBillTD[0].Text;
                                        strbillTaxAmount = ITaxBillTD[1].Text;
                                        strbillAssess = ITaxBillTD[2].Text;
                                        strbillExemption = ITaxBillTD[3].Text;
                                        strbillTaxable = ITaxBillTD[4].Text;
                                        strbillMillege = ITaxBillTD[5].Text;
                                    }
                                    catch { }
                                    string strTaxBillDetails = strVTaxType + "~" + strTaxType + "~" + "Total" + "~" + strbillTaxAuthority + "~" + strbillMillege + "~" + strbillAssess + "~" + strbillExemption + "~" + strbillTaxAmount;
                                    gc.insert_date(orderNumber, strParcelNumber, 297, strTaxBillDetails, 1, DateTime.Now);
                                }
                            }


                            try
                            {
                                strNVTaxType = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[3]/caption")).Text;
                            }
                            catch { }
                            IWebElement IValoremTable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[3]/tbody"));
                            IList<IWebElement> IValoremRow = IValoremTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> IValoremTD;

                            foreach (IWebElement Valorem in IValoremRow)
                            {
                                IValoremTD = Valorem.FindElements(By.TagName("td"));
                                if (IValoremTD.Count != 0 && Valorem.Text.Contains("$"))
                                {
                                    strValoremAuthority = IValoremTD[0].Text;
                                    strValoremRate = IValoremTD[1].Text;
                                    strValoremAmount = IValoremTD[2].Text;

                                    string strValoremDetails = strNVTaxType + "~" + strValoremAuthority + "~" + strValoremRate + "~" + strValoremAmount;
                                    gc.insert_date(orderNumber, strParcelNumber, 336, strValoremDetails, 1, DateTime.Now);
                                }
                            }

                            IWebElement IValTable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[3]/tfoot"));
                            IList<IWebElement> IvalRow = IValTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> IvalTD;
                            foreach (IWebElement val in IvalRow)
                            {
                                IvalTD = val.FindElements(By.TagName("td"));
                                if (IvalTD.Count != 0 && val.Text.Contains("$"))
                                {
                                    try
                                    {
                                        strValAuthority = IvalTD[0].Text;
                                        strValRate = IvalTD[1].Text;
                                        strValAmount = IvalTD[2].Text;
                                    }
                                    catch { }

                                    string strValDetails = strNVTaxType + "~" + "Total" + "~" + strValRate + "~" + strValAuthority;
                                    gc.insert_date(orderNumber, strParcelNumber, 336, strValDetails, 1, DateTime.Now);
                                }

                                if (IvalTD.Count != 0 && val.Text.Contains("No non-ad valorem assessments."))
                                {
                                    strValoremAuthority = IvalTD[0].Text;

                                    string strValoremDetails = strNVTaxType + "~" + strValoremAuthority + "~" + "" + "~" + "";
                                    gc.insert_date(orderNumber, strParcelNumber, 336, strValoremDetails, 1, DateTime.Now);
                                }
                            }

                            try
                            {
                                strcombine = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/p")).Text;
                                strcombine = GlobalClass.After(strcombine, "Combined taxes and assessments: ");
                                amc.Instamount1 = strcombine;
                                string strTaxDetails = strTaxType + "~" + strcombine + "~" + "" + "~" + "";
                                gc.insert_date(orderNumber, strParcelNumber, 298, strTaxDetails, 1, DateTime.Now);
                            }
                            catch { }

                            IWebElement ITaxCom = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]/tbody"));
                            IList<IWebElement> ITaxComRow = ITaxCom.FindElements(By.TagName("tr"));
                            IList<IWebElement> ITaxComTD;
                            foreach (IWebElement taxrow in ITaxComRow)
                            {
                                ITaxComTD = taxrow.FindElements(By.TagName("td"));
                                if (ITaxComTD.Count != 0 && taxrow.Text.Contains(""))
                                {
                                    for (int j = 0; j < ITaxComTD.Count; j++)
                                    {
                                        strTaxCom = ITaxComTD[j].Text;
                                        string current = DateTime.Now.Year.ToString();
                                        string strCurrentYear = current.Substring(0, 2);
                                        if ((!strTaxCom.Contains("Face Amt") && !strTaxCom.Contains("Bid ") && !strTaxCom.Contains("Bidder") && !strTaxCom.Contains("Certificate")) && (strTaxCom.Contains(strCurrentYear) || (strTaxCom.Contains("$"))))
                                        {
                                            strDate = GlobalClass.Before(strTaxCom, "\r\n");
                                            strAmount = GlobalClass.After(strTaxCom, "\r\n");
                                            amc.DiscountDate1 = strDate;
                                            string strTaxDetails = strTaxType + "~" + "" + "~" + strDate + "~" + strAmount;
                                            gc.insert_date(orderNumber, strParcelNumber, 298, strTaxDetails, 1, DateTime.Now);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    try
                    {
                        if (currentYear < 1 && strTaxType.Contains("Annual Bill"))
                        {
                            string strstatus = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[6]")).Text;
                            string[] paiddue = strstatus.Split('\r');

                            if ((strstatus.Contains("Paid") || strstatus.Contains("PAID")) && !strstatus.Contains("No taxes due"))
                            {
                                string[] due = paiddue[0].Trim().Replace("\n","").Replace("  ", " ").Split(' ');
                                amc.IsDelinquent = "No";
                                amc.InstPaidDue1 = "Paid";
                                amc.Instamountpaid1 = due[2];
                                gc.InsertAmrockTax(orderNumber, amc.TaxId, amc.Instamount1, amc.Instamount2, amc.Instamount3, amc.Instamount4, amc.Instamountpaid1, amc.Instamountpaid2, amc.Instamountpaid3, amc.Instamountpaid4, amc.InstPaidDue1, amc.InstPaidDue2, amc.instPaidDue3, amc.instPaidDue4, amc.IsDelinquent);
                                currentYear++;
                            }
                            if (!strstatus.Contains("Paid") && !strstatus.Contains("PAID") && strstatus.Contains("due") && !strstatus.Contains("Pay this bill") && !strstatus.Contains("No taxes due"))
                            {
                                string[] due = paiddue[0].Trim().Replace("\n", "").Replace("  ", " ").Split(' ');
                                //amc.IsDelinquent = "No";
                                //amc.InstPaidDue1 = "Paid";
                                double Combine = Convert.ToDouble(strcombine.Replace("$", "").Trim());
                                double amount = Convert.ToDouble(due[2].Replace("$", "").Trim());
                                if (Combine > amount)
                                {
                                    amc.IsDelinquent = "No";
                                    amc.InstPaidDue1 = "Due";
                                    amc.Instamountpaid1 = due[2];
                                }
                                //if (Combine == amount)
                                //{
                                //    amc.IsDelinquent = "No";
                                //    amc.InstPaidDue1 = "Due";
                                //}
                                if (Combine < amount)
                                {
                                    amc.IsDelinquent = "Yes";
                                    amc.InstPaidDue1 = "Due";
                                }
                                currentYear++;
                            }
                            if (strstatus.Contains("Pay this bill") && !strstatus.Contains("No taxes due"))
                            {
                                double Combine = Convert.ToDouble(strcombine.Replace("$", "").Trim());
                                double amount = Convert.ToDouble(strstatus.Replace("Pay this bill:", "").Replace("$", "").Trim());
                                if (Combine > amount)
                                {
                                    amc.IsDelinquent = "No";
                                    amc.InstPaidDue1 = "Due";
                                    amc.Instamountpaid1 = strstatus.Replace("Pay this bill:", "").Trim();
                                }
                                //if (Combine == amount)
                                //{
                                //    amc.IsDelinquent = "No";
                                //    amc.InstPaidDue1 = "Due";
                                //}
                                if (Combine < amount)
                                {
                                    amc.IsDelinquent = "Yes";
                                    amc.InstPaidDue1 = "Due";
                                }
                                
                                currentYear++;
                            }
                            if (amc.IsDelinquent == "Yes")
                            {
                                gc.InsertAmrockTax(orderNumber, amc.TaxId, null, null, null, null, null, null, null, null, null, null, null, null, amc.IsDelinquent);
                            }

                            if (amc.IsDelinquent == "No")
                            {
                                gc.InsertAmrockTax(orderNumber, amc.TaxId, amc.Instamount1, amc.Instamount2, amc.Instamount3, amc.Instamount4, amc.Instamountpaid1, amc.Instamountpaid2, amc.Instamountpaid3, amc.Instamountpaid4, amc.InstPaidDue1, amc.InstPaidDue2, amc.instPaidDue3, amc.instPaidDue4, amc.IsDelinquent);
                            }
                        }
                    }
                    catch { }



                    try
                    {
                        IWebElement IparcelURL = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[3]/div[1]/ul/li[1]/a"));
                        string strParcelURL = IparcelURL.Text;
                        driver.Navigate().GoToUrl(strParcelURL);
                        gc.CreatePdf(orderNumber, strParcelNumber, "Tax Parcel Details" + strTaxType, driver, "FL", "Broward");
                    }
                    catch { }
                    string certificate = "", strCertificate = "", stradvertisedNo = "", strFaceAmount = "", strIssuedDate = "", strExpirationDate = "", strBuyer = "", strInterestRate = "", IssuedYear = "", strIssuedYear = "";
                    try
                    {
                        if (strPaid.Contains("Pay this bill") || strTaxBill.Contains("Cannot be paid online") || strTaxBill.Contains("PAID"))
                        {
                            try
                            {
                                IWebElement ITaxSearch = driver.FindElement(By.LinkText("Parcel details"));
                                string strITaxSearch = ITaxSearch.GetAttribute("href");
                                driver.Navigate().GoToUrl(strITaxSearch);

                                strissueParcelNo = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[3]/div[2]/dl[2]/dd[1]")).Text;
                                strissueAlternetKey = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[3]/div[2]/dl[2]/dd[2]")).Text;
                                strissueMillageCode = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[3]/div[2]/dl[2]/dd[3]")).Text;
                                strissueMillageRate = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[3]/div[2]/dl[2]/dd[4]")).Text;
                                certificate = driver.FindElement(By.XPath("//*[@id='certificate']")).Text;
                                strCertificate = GlobalClass.After(certificate, "Certificate ");
                                stradvertisedNo = driver.FindElement(By.XPath("//*[@id='content']/div[1]/dl/dd[1]")).Text;
                                strFaceAmount = driver.FindElement(By.XPath("//*[@id='content']/div[1]/dl/dd[2]")).Text;
                                strIssuedDate = driver.FindElement(By.XPath("//*[@id='content']/div[1]/dl/dd[3]")).Text;
                                strExpirationDate = driver.FindElement(By.XPath("//*[@id='content']/div[1]/dl/dd[4]")).Text;
                                strBuyer = driver.FindElement(By.XPath("//*[@id='content']/div[1]/dl/dd[5]")).Text;
                                strInterestRate = driver.FindElement(By.XPath("//*[@id='content']/div[1]/dl/dd[6]")).Text;
                                IssuedYear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/p")).Text;
                                strIssuedYear = GlobalClass.After(IssuedYear, "This parcel has an issued certificate for ").Replace(".", "");
                                if (IssuedYear.Contains("This parcel has an issued certificate") && !IssuedYear.Contains("This parcel has a redeemed certificate"))
                                {
                                    gc.CreatePdf(orderNumber, strParcelNumber, "Tax Detailed Result Parcel Details" + strTaxType + "", driver, "FL", "Broward");

                                    // Parcel Details 
                                    string strTaxIssue = strTaxType + "~" + strissueAlternetKey + "~" + strEscrowCode + "~" + strissueMillageCode + "~" + strissueMillageRate + "~" + strCertificate + "~" + stradvertisedNo + "~" + strFaceAmount + "~" + strIssuedDate + "~" + strExpirationDate + "~" + strBuyer + "~" + strIssuedYear;
                                    gc.insert_date(orderNumber, strissueParcelNo, 296, strTaxIssue, 1, DateTime.Now);
                                }
                                if (IssuedYear.Contains("This parcel has a redeemed certificate") && !IssuedYear.Contains("This parcel has an issued certificate"))
                                {
                                    gc.CreatePdf(orderNumber, strParcelNumber, "Tax Detailed Result Parcel Details" + strTaxType + "", driver, "FL", "Broward");

                                    // Parcel Details 
                                    string strTaxIssue = strTaxType + "~" + strissueAlternetKey + "~" + strEscrowCode + "~" + strissueMillageCode + "~" + strissueMillageRate + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                    gc.insert_date(orderNumber, strissueParcelNo, 296, strTaxIssue, 1, DateTime.Now);
                                }
                            }
                            catch { }

                            if (certificate == "" && strIssuedYear == "")
                            {
                                gc.CreatePdf(orderNumber, strParcelNumber, "Tax Detailed Result Parcel Details" + strTaxType + "", driver, "FL", "Broward");

                                string strTaxIssue = strTaxType + "~" + strissueAlternetKey + "~" + strEscrowCode + "~" + strissueMillageCode + "~" + strissueMillageRate + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                gc.insert_date(orderNumber, strissueParcelNo, 296, strTaxIssue, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }
                }
                catch { }

            }
        }
        public void ByVisibleElement(IWebElement Element, IWebDriver driver1)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver1;
            js.ExecuteScript("arguments[0].scrollIntoView();", Element);
        }
    }
}




//StartTime = DateTime.Now.ToString("HH:mm:ss");
//                    driver.Navigate().GoToUrl("http://www.bcpa.net/RecMenu.asp");
//Thread.Sleep(3000);

//                    //if (searchType == "titleflex")
//                    //{
//                    //    string address = houseno + " " + strDirection + " " + sname + " " + strStreetType + unitnumber;
//                    //    gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "FL", "Broward");

//                    //    if (HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes")
//                    //    {
//                    //        return "MultiParcel";
//                    //    }
//                    //    parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
//                    //    parcelNumber = HttpContext.Current.Session["titleparcel"].ToString().Replace(".", "").Replace("-", "");
//                    //    searchType = "parcel";
//                    //}

//                    if (searchType == "address")
//                    {
//                        if (sname.Any(char.IsDigit))
//                        {
//                            sname = Regex.Match(sname, @"\d+").Value;
//                        }

//                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr/td/table/tbody/tr/td[1]/p[4]/a/img")).Click();
//driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/form/table/tbody/tr/td[1]/table[1]/tbody/tr[2]/td[2]/input")).SendKeys(houseno);
//driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/form/table/tbody/tr/td[1]/table[1]/tbody/tr[2]/td[4]/input")).SendKeys(sname);


//                        if (strDirection.Trim() != "")
//                        {
//                            strDirection = strDirection.ToUpper();
//                            IWebElement IDirection = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/form/table/tbody/tr/td[1]/table[1]/tbody/tr[2]/td[3]/select"));
//SelectElement sDirection = new SelectElement(IDirection);
//sDirection.SelectByText(strDirection);
//                            strDirection = "";
//                        }
//                        if (strStreetType.Trim() != "")
//                        {
//                            string street = strStreetType.Substring(0, 1);
//string strstreet = strStreetType.Substring(1, strStreetType.Length - 1);
//string strStType = street.ToUpper() + strstreet.ToLower();
//IWebElement IStreetType = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/form/table/tbody/tr/td[1]/table[1]/tbody/tr[2]/td[5]/select"));
//                            try
//                            {
//                                SelectElement sStreetType = new SelectElement(IStreetType);
//sStreetType.SelectByText(strStType);
//                            }
//                            catch { }
//                            try
//                            {
//                                SelectElement sStrType = new SelectElement(IStreetType);
//sStrType.SelectByValue(strStType.ToUpper());
//                            }
//                            catch { }
//                            strStreetType = "";
//                        }
//                        if (unitnumber.Trim() != "")
//                        {
//                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/form/table/tbody/tr/td[1]/table[1]/tbody/tr[2]/td[7]/input")).SendKeys(unitnumber);
//                        }
//                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "FL", "Broward");
//                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/form/table/tbody/tr/td[1]/table[2]/tbody/tr/td/a[2]/img")).Click();
//gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "FL", "Broward");
//                        try
//                        {
//                            strMulti = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr[1]/td/form/div[2]/table[2]/tbody/tr/td[2]")).Text;
//                            string strMultiCount = GlobalClass.Before(strMulti, " Records Found");
//                            if (Convert.ToInt32(strMultiCount) > 25)
//                            {
//                                HttpContext.Current.Session["multiparcel_FLBroward_Maximum"] = "Yes";
//                                driver.Quit();
//                                return "Maximum";
//                            }
//                            IWebElement IMulti = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr[1]/td/form/div[2]/table[3]/tbody"));
//IList<IWebElement> IMultirow = IMulti.FindElements(By.XPath("tr"));
//IList<IWebElement> IMultitd;
//                            foreach (IWebElement row in IMultirow)
//                            {
//                                //GlobalClass.multiPArcel_FLBroward = "Yes";
//                                HttpContext.Current.Session["multiPArcel_FLBroward"] = "Yes";
//                                IMultitd = row.FindElements(By.XPath("td"));
//                                if (IMultitd.Count != 0 && !row.Text.Contains("Folio Number") && MultiCount <= 25)
//                                {
//                                    try
//                                    {
//                                        MultiCount++;
//                                        strParcel = IMultitd[0].Text;
//                                        strOwner = IMultitd[1].Text;
//                                        strAddress = IMultitd[2].Text;

//                                        strMultiDetails = strOwner.Trim() + "~" + strAddress.Trim();
//                                        gc.insert_date(orderNumber, strParcel, 295, strMultiDetails, 1, DateTime.Now);
//                                    }
//                                    catch { }
//                                }
//                            }
//                            driver.Quit();
//                            return "MultiParcel";
//                        }
//                        catch { }

//                    }
//                    else if (searchType == "parcel")
//                    {
//                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr/td/table/tbody/tr/td[1]/p[6]/a/img")).Click();
//driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/div/form/table/tbody/tr/td/table/tbody/tr[2]/td[2]/input")).SendKeys(parcelNumber.Replace("-", ""));
//                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "FL", "Broward");
//                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/div/form/table/tbody/tr/td/table/tbody/tr[3]/td/input[2]")).Click();
//gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Result", driver, "FL", "Broward");
//                    }
//                    else if (searchType == "ownername")
//                    {
//                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr/td/table/tbody/tr/td[1]/p[3]/a/img")).Click();
//driver.FindElement(By.XPath("//*[@id='Text1']")).SendKeys(ownername);
//gc.CreatePdf_WOP(orderNumber, "Owner Search", driver, "FL", "Broward");
//                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/div/form/table/tbody/tr/td/table/tbody/tr[4]/td/a[2]/img")).Click();
//gc.CreatePdf_WOP(orderNumber, "Owner Search Result", driver, "FL", "Broward");
//                        try
//                        {
//                            strMulti = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr[1]/td/form/div[2]/table[2]/tbody/tr/td[2]/font/b")).Text;
//                            string strMultiCount = GlobalClass.Before(strMulti, " Records Found");
//                            if (Convert.ToInt32(strMultiCount) > 25)
//                            {
//                                HttpContext.Current.Session["multiparcel_FLBroward_Maximum"] = "Yes";
//                                driver.Quit();
//                                return "Maximum";
//                            }
//                            IWebElement IMulti = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr[1]/td/form/div[2]/table[3]/tbody"));
//IList<IWebElement> IMultirow = IMulti.FindElements(By.XPath("tr"));
//IList<IWebElement> IMultitd;
//                            foreach (IWebElement row in IMultirow)
//                            {
//                                HttpContext.Current.Session["multiPArcel_FLBroward"] = "Yes";
//                                // GlobalClass.multiPArcel_FLBroward = "Yes";
//                                IMultitd = row.FindElements(By.XPath("td"));
//                                if (IMultitd.Count != 0 && !row.Text.Contains("Folio Number") && MultiCount <= 25)
//                                {
//                                    try
//                                    {
//                                        MultiCount++;
//                                        strParcel = IMultitd[0].Text;
//                                        strOwner = IMultitd[1].Text;
//                                        strAddress = IMultitd[2].Text;

//                                        strMultiDetails = strOwner.Trim() + "~" + strAddress.Trim();
//                                        gc.insert_date(orderNumber, strParcel, 295, strMultiDetails, 1, DateTime.Now);
//                                    }
//                                    catch { }
//                                }
//                            }
//                            driver.Quit();
//                            return "MultiParcel";
//                        }
//                        catch { }
//                    }

//                    try
//                    {
//                        IWebElement INodata = driver.FindElement(By.Id("Table5"));
//                        if(INodata.Text.Contains("No records found"))
//                        {
//                            HttpContext.Current.Session["Nodata_BrowardFL"] = "Yes";
//                            driver.Quit();
//                            return "No Data Found";
//                        }
//                    }
//                    catch { }

//                    //Property Details 
//                    strProeprtyAddress = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table/tbody/tr[1]/td[1]/table[1]/tbody/tr/td[1]/table/tbody/tr[1]/td[2]")).Text;
//                    strParcelNumber = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table/tbody/tr[1]/td[1]/table[1]/tbody/tr/td[3]/table/tbody/tr[1]/td[2]")).Text;
//                    strOwnerName = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table/tbody/tr[1]/td[1]/table[1]/tbody/tr/td[1]/table/tbody/tr[2]/td[2]")).Text;
//                    strMailingAddress = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table/tbody/tr[1]/td[1]/table[1]/tbody/tr/td[1]/table/tbody/tr[3]/td[2]")).Text;
//                    strLegalDiscription = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table/tbody/tr[1]/td[1]/table[3]/tbody/tr/td[2]")).Text;

//                    string strPropertyDetails = strProeprtyAddress.Trim() + "~" + strOwnerName.Trim() + "~" + strMailingAddress + "~" + strLegalDiscription;
//gc.insert_date(orderNumber, strParcelNumber, 293, strPropertyDetails, 1, DateTime.Now);

//                    gc.CreatePdf(orderNumber, strParcelNumber, "Property Search Result", driver, "FL", "Broward");

//                    IWebElement IProTable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table/tbody/tr[1]/td[1]/table[5]/tbody"));
//IList<IWebElement> IProTableRow = IProTable.FindElements(By.TagName("tr"));
//IList<IWebElement> IProTabletd;
//                    foreach (IWebElement ass in IProTableRow)
//                    {
//                        IProTabletd = ass.FindElements(By.TagName("td"));
//                        if (IProTabletd.Count != 0 && !ass.Text.Contains("Property Assessment Values") && !ass.Text.Contains("Year") && (!ass.Text.Contains("Click here to see") && !ass.Text.Contains("Exemptions and Taxable Values as reflected")))
//                        {
//                            strYear = IProTabletd[0].Text;
//                            strLand = IProTabletd[1].Text;
//                            strBuilding = IProTabletd[2].Text;
//                            strMarket = IProTabletd[3].Text;
//                            strAssessed = IProTabletd[4].Text;
//                            strTax = IProTabletd[5].Text;

//                            string strAssessmentDetails = strYear + "~" + strLand + "~" + strBuilding + "~" + strMarket + "~" + strAssessed + "~" + strTax;
//gc.insert_date(orderNumber, strParcelNumber, 294, strAssessmentDetails, 1, DateTime.Now);
//                        }
//                    }

//                    IWebElement IAssessTax = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table/tbody/tr[1]/td[1]/table[7]/tbody"));
//IList<IWebElement> IAssessTaxRow = IAssessTax.FindElements(By.TagName("tr"));
//IList<IWebElement> IAssessTD;
//                    foreach (IWebElement Assess in IAssessTaxRow)
//                    {
//                        IAssessTD = Assess.FindElements(By.TagName("td"));
//                        if (IAssessTD.Count != 0 && !Assess.Text.Contains("Exemptions and Taxable Values by Taxing Authority") && !Assess.Text.Contains("County"))
//                        {
//                            strAssessTitle = IAssessTD[0].Text;
//                            strCounty = IAssessTD[1].Text;
//                            strSchool = IAssessTD[2].Text;
//                            strMunicipal = IAssessTD[3].Text;
//                            strIndependent = IAssessTD[4].Text;


//                            string strTaxAssess = strAssessTitle + "~" + strCounty + "~" + strSchool + "~" + strMunicipal + "~" + strIndependent;
//gc.insert_date(orderNumber, strParcelNumber, 400, strTaxAssess, 1, DateTime.Now);
//                        }
//                    }
