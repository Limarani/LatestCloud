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
    public class WebDriver_FLPolk
    {
        IWebDriver driver;
        IWebElement Ilink;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();
        string strOwnerName = "-", strAddress = "-", strParcelId = "-", strMultiCount = "", strSearchLink = "", strParcelNumber = "-", strOwner = "", strOwner1 = "", strOwner2 = "", strOwner3 = "", strSiteAddress = "-", strSiteAddress1 = "-", strSiteAddress2 = "-", strCity = "-",
               strState = "-", strZip = "-", strNeighBorHood = "-", strSubdivision = "-", strPropertyCode = "-", strAcreage = "-", strTaxing = "-", strCommunityArea = "-", strYear = "-", strYearBuilt = "-", strproperty = "-", strParcelInformation = "-", strTaxingDistrict = "-",
               strLandValue = "-", strBuildingValue = "-", strMiscItemValue = "-", strLClassifiedValue = "-", strMarketValue = "-", strCDPValue = "-", strAgriculture = "-", strAssessedValue = "-", strExemptValue = "-", strTaxableValue = "-", strDD = "-", strFTR = "-", strAV = "-",
               strFAT = "-", strEX = "-", strFTS = "-", strTV = "-", strFT = "-", strLN = "-", strCode = "-", strDesc = "-", strUnits = "-", strRate = "-", strAssessment = "-", strTDesc = "-", strLastYear = "-", strFinal = "-", strParcel = "", strparcelFirst = "", strparcelMiddle = "", strparcelLast = "",
               strFullParcel = "", strTaxType = "-", strAccountNo = "-", strTaxYear = "-", strMailingAddress = "-", strPhysicalAddress = "-", strGeoNo = "-", strExemptAmount = "-", strTaxableAmount = "-", strLegalDiscription = "-", strTaxAuthority = "-", strTaxRate = "-", strTaxAssessedValue = "-",
               strTaxExemptionAmount = "-", strTaxbleValue = "-", strTaxAmount = "-", strTotalMillege = "-", strTotalMillegeAmount = "-", strTotalTax = "-", strTotalTaxAmount = "-", strNonCode = "-", strNonAuthority = "-", strNonAmount = "-", strTotalTaxAssess = "-", strTotalTaxAssessAmount = "-",
               strTotalAss = "-", strTotalAssAmount = "-", strDPaid = "-", strPAmount = "-", strDatePaid = "-", strTransaction = "-", strReciept = "-", strDyear = "-", strDAmount = "-", strDPrior = "-", strTaxPYear = "-", strTaxPayFolio = "-", strTaxPayDatePaid = "-", strTaxPayYear = "-",
               strTaxPayType = "-", strTaxPHYear = "-", strTaxPayReceipt = "-", strTaxPayAmountBilled = "-", strTaxPayAmountPaid = "-", strValuesDistrict="", strValorem="", strTaxNonValorem="", strNonValorem="", strTaxPaidBy="", strTotalTaxAssessDetails="", strParcelsecond="", strParcelthird="", strTaxPayDetails="";

        public string FTP_FLPolk(string address, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())//ChromeDriver();
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, "", "",address, "FL", "Polk");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_FLPolk"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("http://www.polkpa.org/CamaDisplay.aspx?OutputMode=Input&searchType=RealEstate&page=FindByAddress");
                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "FL", "Polk");
                        driver.FindElement(By.Id("address")).SendKeys(address);
                        driver.FindElement(By.XPath("//*[@id='CamaDisplayArea']/div[1]/div[3]/table/tbody/tr/td/input")).SendKeys(Keys.Enter);
                        gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "FL", "Polk");
                        try
                        {
                            IWebElement ImultiCount = driver.FindElement(By.XPath("//*[@id='CamaDisplayArea']/span[1]"));
                            strMultiCount = GlobalClass.Before(ImultiCount.Text, " Matches found");
                            if (Convert.ToInt32(strMultiCount) !=1 && Convert.ToInt32(strMultiCount) >= 1)
                            {
                                if (Convert.ToInt32(strMultiCount) > 25)
                                {
                                    HttpContext.Current.Session["multiparcel_FLPolk_Count"] = "Maximum";
                                    return "Maximum";
                                }

                                IWebElement IMultiTable = driver.FindElement(By.XPath("//*[@id='CamaDisplayArea']/div[3]/table/tbody"));
                                IList<IWebElement> IMultiRow = IMultiTable.FindElements(By.TagName("tr"));
                                IList<IWebElement> IMultiTD;
                                foreach (IWebElement multi in IMultiRow)
                                {
                                    IMultiTD = multi.FindElements(By.TagName("td"));
                                    if (IMultiTD.Count != 0 && IMultiTD.Count > 1 && !multi.Text.Contains("Owner Name") && Convert.ToInt32(strMultiCount) <= 25)
                                    {
                                        HttpContext.Current.Session["multiparcel_FLPolk"] = "Yes";
                                        gc.CreatePdf_WOP(orderNumber, "Multi Parcel Result", driver, "FL", "Polk");

                                        strAddress = IMultiTD[1].Text;
                                        strParcelId = IMultiTD[2].Text;
                                        strOwnerName = IMultiTD[3].Text;

                                        string strMultiDetails = strOwnerName + "~" + strAddress;
                                        gc.insert_date(orderNumber, strParcelId, 426, strMultiDetails, 1, DateTime.Now);
                                    }
                                }

                                driver.Quit();
                                return "MultiParcel";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("http://www.polkpa.org/CamaDisplay.aspx?OutputMode=Input&searchType=RealEstate&page=FindByID");
                        gc.CreatePdf(orderNumber, parcelNumber,"Parcel Search", driver, "FL", "Polk");
                        driver.FindElement(By.Id("parcelID")).SendKeys(parcelNumber);
                        driver.FindElement(By.XPath("//*[@id='CamaDisplayArea']/div[1]/div[3]/table/tbody/tr/td/input")).SendKeys(Keys.Enter);
                        gc.CreatePdf(orderNumber, parcelNumber,"Parcel Search Result", driver, "FL", "Polk");
                    }
                    if (searchType == "ownername")
                    {
                        driver.Navigate().GoToUrl("http://www.polkpa.org/CamaDisplay.aspx?OutputMode=Input&searchType=RealEstate&page=FindByOwnerName");
                        gc.CreatePdf_WOP(orderNumber, "Owner Search", driver, "FL", "Polk");
                        driver.FindElement(By.Id("OwnerName")).SendKeys(ownername);
                        driver.FindElement(By.XPath("//*[@id='CamaDisplayArea']/div[1]/div[3]/table/tbody/tr/td/input")).SendKeys(Keys.Enter);
                        gc.CreatePdf_WOP(orderNumber, "Owner Search Result", driver, "FL", "Polk");

                        try
                        {
                            IWebElement ImultiCount = driver.FindElement(By.XPath("//*[@id='CamaDisplayArea']/span[1]"));
                            strMultiCount = GlobalClass.Before(ImultiCount.Text, " Matches found");
                            if (Convert.ToInt32(strMultiCount) != 1 && Convert.ToInt32(strMultiCount) >= 1)
                            {
                                if (Convert.ToInt32(strMultiCount) > 25)
                                {
                                    HttpContext.Current.Session["multiparcel_FLPolk_Count"] = "Maximum";
                                    return "Maximum";
                                }
                                IWebElement IMultiTable = driver.FindElement(By.XPath("//*[@id='CamaDisplayArea']/div[3]/table/tbody"));
                                IList<IWebElement> IMultiRow = IMultiTable.FindElements(By.TagName("tr"));
                                IList<IWebElement> IMultiTD;
                                foreach (IWebElement multi in IMultiRow)
                                {
                                    IMultiTD = multi.FindElements(By.TagName("td"));
                                    if (IMultiTD.Count != 0 && IMultiTD.Count > 1 && !multi.Text.Contains("Owner Name") && Convert.ToInt32(strMultiCount) <= 25)
                                    {
                                        HttpContext.Current.Session["multiparcel_FLPolk"] = "Yes";
                                        gc.CreatePdf_WOP(orderNumber, "Multi Parcel Result", driver, "FL", "Polk");

                                        strOwnerName = IMultiTD[1].Text;
                                        strParcelId = IMultiTD[2].Text;
                                        strAddress = IMultiTD[3].Text;

                                        string strMultiDetails = strOwnerName + "~" + strAddress;
                                        gc.insert_date(orderNumber, strParcelId, 426, strMultiDetails, 1, DateTime.Now);
                                    }
                                }

                                driver.Quit();
                                return "MultiParcel";
                            }
                        }
                        catch { }

                    }
                    try
                    {
                        IWebElement INodata = driver.FindElement(By.Id("CamaDisplayArea"));
                        if(INodata.Text.Contains("0 Matches found for search results"))
                        {
                            HttpContext.Current.Session["Nodata_FLPolk"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    try
                    {
                        IWebElement ImultiCount = driver.FindElement(By.XPath("//*[@id='CamaDisplayArea']/span[1]"));
                        strMultiCount = GlobalClass.Before(ImultiCount.Text, " Matches found");
                        if (Convert.ToInt32(strMultiCount) == 1 && Convert.ToInt32(strMultiCount) != 0)
                        {
                            IWebElement IMultiTable = driver.FindElement(By.XPath("//*[@id='CamaDisplayArea']/div[3]/table/tbody"));
                            IList<IWebElement> IMultiRow = IMultiTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> IMultiTD;
                            foreach (IWebElement multi in IMultiRow)
                            {
                                IMultiTD = multi.FindElements(By.TagName("td"));
                                if (IMultiTD.Count != 0 && IMultiTD.Count > 1 && !multi.Text.Contains("Owner Name") && Convert.ToInt32(strMultiCount) == 1)
                                {
                                    gc.CreatePdf_WOP(orderNumber, "Search Result", driver, "FL", "Polk");
                                    try
                                    {
                                        Ilink = IMultiTD[2].FindElement(By.TagName("a"));
                                    }
                                    catch { }
                                    try
                                    {
                                        Ilink = IMultiTD[1].FindElement(By.TagName("a"));
                                    }
                                    catch { }
                                    strSearchLink = Ilink.GetAttribute("href");
                                }
                            }
                        }
                    }
                    catch { }

                    driver.Navigate().GoToUrl(strSearchLink);                    
                    string strParcelNo = driver.FindElement(By.XPath("//*[@id='CamaDisplayArea']/table[1]/tbody/tr[1]/td[1]")).Text;
                    strParcelNumber = GlobalClass.After(strParcelNo, "Parcel Details: ");
                    gc.CreatePdf(orderNumber, strParcelNumber, "Property Search Result", driver, "FL", "Polk");
                    strOwner1 = driver.FindElement(By.XPath("//*[@id='CamaDisplayArea']/table[2]/tbody/tr/td[1]/table[1]/tbody/tr[1]/td[1]")).Text.Trim();
                    try
                    {
                        strOwner2 = driver.FindElement(By.XPath("//*[@id='CamaDisplayArea']/table[2]/tbody/tr/td[1]/table[1]/tbody/tr[2]/td[1]")).Text.Trim();
                    }
                    catch { }
                    try
                    {
                        strOwner3 = driver.FindElement(By.XPath("//*[@id='CamaDisplayArea']/table[2]/tbody/tr/td[1]/table[1]/tbody/tr[3]/td[1]")).Text.Trim();
                    }
                    catch { }
                    if (strOwner1 != "")
                    {
                        strOwner = strOwner1 + " " + strOwner2 + " " + strOwner3;
                    }
                    strproperty = driver.FindElement(By.XPath("//*[@id='CamaDisplayArea']/table[2]/tbody/tr/td[1]/table[3]/tbody")).Text.Trim();
                    strSiteAddress1 = gc.Between(strproperty, "Address 1 ", "\r\n");
                    try
                    {
                        strSiteAddress2 = gc.Between(strproperty, "\r\n", "Address 2\r\n");
                        strSiteAddress = strSiteAddress1 + strSiteAddress2;
                    }
                    catch { }
                    strCity = gc.Between(strproperty, "City ", "\r\nState");
                    strState = gc.Between(strproperty, "State", "\r\nZip Code");
                    strZip = GlobalClass.After(strproperty, "\r\nZip Code ").Replace("-","");
                    strParcelInformation = driver.FindElement(By.XPath("//*[@id='CamaDisplayArea']/table[2]/tbody/tr/td[1]/table[4]/tbody")).Text;
                    strNeighBorHood = gc.Between(strParcelInformation, "Neighborhood", "\r\nSubdivision");
                    strSubdivision = gc.Between(strParcelInformation, "\r\nSubdivision\r\n", "\r\nProperty (DOR) Use Code");
                    strPropertyCode = gc.Between(strParcelInformation, "Property (DOR) Use Code", "\r\nAcreage");
                    strAcreage = gc.Between(strParcelInformation, "Acreage", "\r\nTaxing District");
                    strTaxingDistrict = gc.Between(strParcelInformation, "Taxing District", "\r\nCommunity Redevelopment Area");
                    strCommunityArea = GlobalClass.After(strParcelInformation, "Community Redevelopment Area ");
                    try
                    {
                        strYear = driver.FindElement(By.XPath("//*[@id='CamaDisplayArea']/div[6]/table/tbody")).Text;
                        strYearBuilt = gc.Between(strYear, "Actual Year Built: ", "\r\nEffective Year: ");
                    }
                    catch { }
                    string strPropertyDetails = strOwner.Trim() + "~" + strSiteAddress + "~" + strCity + "~" + strState + "~" + strZip + "~" + strNeighBorHood + "~" + strSubdivision + "~" + strPropertyCode + "~" + strAcreage + "~" + strTaxingDistrict + "~" + strCommunityArea + "~" + strYearBuilt;
                    gc.insert_date(orderNumber, strParcelNumber, 428, strPropertyDetails, 1, DateTime.Now);

                    try
                    {
                        IWebElement IValuesTable = driver.FindElement(By.XPath("//*[@id='CamaDisplayArea']/div[12]/table[1]/tbody"));
                        IList<IWebElement> IValuesRow = IValuesTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> IValuesTD;
                        foreach (IWebElement value in IValuesRow)
                        {
                            IValuesTD = value.FindElements(By.TagName("td"));
                            if (IValuesTD.Count != 0 && !value.Text.Contains("Desc Value"))
                            {
                                if (IValuesTD[0].Text == "Land Value")
                                {
                                    strLandValue = IValuesTD[1].Text;
                                }
                                if (IValuesTD[0].Text == "Building Value")
                                {
                                    strBuildingValue = IValuesTD[1].Text;
                                }
                                if (IValuesTD[0].Text == "Misc. Items Value")
                                {
                                    strMiscItemValue = IValuesTD[1].Text;
                                }
                                if (IValuesTD[0].Text == "Land Classified Value")
                                {
                                    strLClassifiedValue = IValuesTD[1].Text;
                                }
                                if (IValuesTD[0].Text == "Just Market Value")
                                {
                                    strMarketValue = IValuesTD[1].Text;
                                }
                                if (IValuesTD[0].Text == "*Cap Differential and Portability")
                                {
                                    strCDPValue = IValuesTD[1].Text;
                                }
                                if (IValuesTD[0].Text == "Agriculture Classification")
                                {
                                    strAgriculture = IValuesTD[1].Text;
                                }
                                if (IValuesTD[0].Text == "Assessed Value")
                                {
                                    strAssessedValue = IValuesTD[1].Text;
                                }
                                if (IValuesTD[0].Text == "Exempt Value (County)")
                                {
                                    strExemptValue = IValuesTD[1].Text;
                                }
                                if (IValuesTD[0].Text == "Taxable Value (County)")
                                {
                                    strTaxableValue = IValuesTD[1].Text;
                                }
                            }
                        }
                    }
                    catch { }

                    string strValueSummary = strLandValue + "~" + strBuildingValue + "~" + strMiscItemValue + "~" + strLClassifiedValue + "~" + strMarketValue + "~" + strCDPValue + "~" + strAgriculture + "~" + strAssessedValue + "~" + strExemptValue + "~" + strTaxableValue;
                    gc.insert_date(orderNumber, strParcelNumber, 429, strValueSummary, 1, DateTime.Now);

                    
                    try
                    {
                        string strValuesD = driver.FindElement(By.XPath("//*[@id='CamaDisplayArea']/div[12]/div[1]")).Text;
                        strValuesDistrict = GlobalClass.Before(strValuesD, "\r\nDistrict Description Final\r\n");
                    }
                    catch { }
                    try
                    {
                        IWebElement IValueDistrict = driver.FindElement(By.XPath("//*[@id='CamaDisplayArea']/div[12]/div[1]/table/tbody"));
                        IList<IWebElement> IValueDistrictRow = IValueDistrict.FindElements(By.TagName("tr"));
                        IList<IWebElement> IValueDistrictTd;
                        foreach (IWebElement district in IValueDistrictRow)
                        {
                            IValueDistrictTd = district.FindElements(By.TagName("td"));
                            if (IValueDistrictTd.Count != 0 && !district.Text.Contains("District Description"))
                            {
                                strDD = IValueDistrictTd[0].Text;
                                strFTR = IValueDistrictTd[1].Text;
                                strAV = IValueDistrictTd[2].Text;
                                strFAT = IValueDistrictTd[3].Text;
                                strEX = IValueDistrictTd[4].Text;
                                strFTS = IValueDistrictTd[5].Text;
                                strTV = IValueDistrictTd[6].Text;
                                strFT = IValueDistrictTd[7].Text;

                                string strValueDistrictDetails = strValuesDistrict + "~" + strDD + "~" + strFTR + "~" + strAV + "~" + strFAT + "~" + strEX + "~" + strFTS + "~" + strTV + "~" + strFT + "~" + "-";
                                gc.insert_date(orderNumber, strParcelNumber, 437, strValueDistrictDetails, 1, DateTime.Now);
                            }

                        }
                    }
                    catch { }

                    try
                    {
                        strNonValorem = driver.FindElement(By.XPath("//*[@id='CamaDisplayArea']/div[12]/div[2]/h3")).Text;
                    }
                    catch { }
                    try
                    {
                        IWebElement INonAdValoremTable = driver.FindElement(By.XPath("//*[@id='CamaDisplayArea']/div[12]/div[2]/table/tbody"));
                        IList<IWebElement> INonAdValoremRow = INonAdValoremTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> INonAdValoremTD;
                        foreach (IWebElement Nvalorem in INonAdValoremRow)
                        {
                            INonAdValoremTD = Nvalorem.FindElements(By.TagName("td"));
                            if (INonAdValoremTD.Count != 0 && (!Nvalorem.Text.Contains("Units") || !Nvalorem.Text.Contains("Rate")) && !Nvalorem.Text.Contains("Total Assessments"))
                            {
                                strLN = INonAdValoremTD[0].Text;
                                strCode = INonAdValoremTD[1].Text;
                                strDesc = INonAdValoremTD[2].Text;
                                strUnits = INonAdValoremTD[3].Text;
                                strRate = INonAdValoremTD[4].Text;
                                strAssessment = INonAdValoremTD[5].Text;

                                string strNonValoremDetails = strNonValorem + "~" + strLN + "~" + strCode + "~" + "-" + "~" + strDesc + "~" + strUnits + "~" + strRate + "~" + strAssessment;
                                gc.insert_date(orderNumber, strParcelNumber, 439, strNonValoremDetails, 1, DateTime.Now);
                            }

                            if (INonAdValoremTD.Count != 0 && Nvalorem.Text.Contains("Total Assessments") && INonAdValoremTD.Count == 2)
                            {
                                strLN = INonAdValoremTD[0].Text;
                                strAssessment = INonAdValoremTD[1].Text;

                                string strNonValoremDetails = strNonValorem + "~" + "-" + "~" + strLN + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + strAssessment;
                                gc.insert_date(orderNumber, strParcelNumber, 439, strNonValoremDetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    try
                    {
                        IWebElement ITaxesTable = driver.FindElement(By.XPath("//*[@id='CamaDisplayArea']/div[12]/table[2]/tbody"));
                        IList<IWebElement> ITaxesRow = ITaxesTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> ITaxesTD;
                        foreach (IWebElement tax in ITaxesRow)
                        {
                            ITaxesTD = tax.FindElements(By.TagName("td"));
                            if (ITaxesTD.Count != 0 && !tax.Text.Contains("Your final tax bill may contain Non-Ad") && !tax.Text.Contains("Desc"))
                            {
                                strTDesc = ITaxesTD[0].Text;
                                strLastYear = ITaxesTD[1].Text;
                                strFinal = ITaxesTD[2].Text;

                                string strTaxesDetails = strTDesc + "~" + strLastYear + "~" + strFinal;
                                gc.insert_date(orderNumber, strParcelNumber, 440, strTaxesDetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    //Tax Details
                    try
                    {
                        driver.Navigate().GoToUrl("http://www.polktaxes.com/generalinfo/officehours_locations.aspx");
                        Thread.Sleep(3000);
                        strTaxAuthority = driver.FindElement(By.XPath("//*[@id='main']/div/div/div/div/div[2]/div/div[6]/div[1]/p")).Text;
                    }
                    catch { }
                    driver.Navigate().GoToUrl("http://fl-polk-taxcollector.governmax.com/collectmax/collect30.asp");
                    Thread.Sleep(3000);
                    IWebElement iframeElement = driver.FindElement(By.XPath("/html/frameset/frame"));
                    driver.SwitchTo().Frame(iframeElement);
                    IWebElement IParcelSerach = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr[1]/td/table[2]/tbody/tr/td[1]/center/a"));
                    IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                    js.ExecuteScript("arguments[0].click();", IParcelSerach);
                    Thread.Sleep(2000);
                    IWebElement IAccountSearch = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[4]/td")).FindElement(By.TagName("a"));
                    string strAccountSearch = IAccountSearch.GetAttribute("href");
                    driver.Navigate().GoToUrl(strAccountSearch);
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, strParcelNumber, "Tax Search", driver, "FL", "Polk");
                    strParcel = strParcelNumber.Replace("-", "");
                    strparcelFirst = strParcel.Substring(4,2);
                    strParcelsecond = strParcel.Substring(2, 2);
                    strParcelthird = strParcel.Substring(0, 2);
                    strparcelMiddle = strParcel.Substring(6, 6);
                    strparcelLast = strParcel.Substring(12, 6);
                    strFullParcel = strparcelFirst + strParcelsecond + strParcelthird + "-" + strparcelMiddle + "-" + strparcelLast;
                    driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[1]/td/table/tbody/tr/td/table/tbody/tr[2]/td/font/input")).SendKeys(strFullParcel);
                    driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[3]/td/input")).SendKeys(Keys.Enter);
                    Thread.Sleep(6000);
                    gc.CreatePdf(orderNumber, strParcelNumber, "Tax Search Result", driver, "FL", "Polk");
                    strAccountNo = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[2]/td[1]")).Text;
                    strTaxType = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[2]/td[2]")).Text;
                    strTaxYear = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[2]/td[3]")).Text;
                    strMailingAddress = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[3]/td/table/tbody/tr[1]/td[1]/font[3]")).Text;
                    strPhysicalAddress = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[3]/td/table/tbody/tr[1]/td[2]/font[3]")).Text;
                    strGeoNo = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[3]/td/table/tbody/tr[1]/td[2]/font[6]")).Text;
                    strExemptAmount = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[5]/td[1]")).Text;
                    strTaxableAmount = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[5]/td[2]")).Text;
                    strLegalDiscription = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[6]/td/table/tbody/tr/td/font[2]")).Text;

                    string strTaxAssessmentDetails = strAccountNo + "~" + strTaxType + "~" + strTaxYear + "~" + strMailingAddress + "~" + strPhysicalAddress + "~" + strGeoNo + "~" + strExemptAmount + "~" + strTaxableAmount + "~" + strLegalDiscription + "~" + strTaxAuthority;
                    gc.insert_date(orderNumber, strAccountNo, 441, strTaxAssessmentDetails, 1, DateTime.Now);


                    try
                    {
                        strValorem = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[7]/td")).Text;
                    }
                    catch { }
                    IWebElement ITaxValoremTable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[8]/td/table[1]/tbody"));
                    IList<IWebElement> ITaxValoremRow = ITaxValoremTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> ITaxValoremTD;
                    foreach(IWebElement valorem in ITaxValoremRow)
                    {
                        ITaxValoremTD = valorem.FindElements(By.TagName("td"));
                        if(ITaxValoremTD.Count !=0 && !valorem.Text.Contains("Taxing Authority"))
                        {
                            try
                            {
                                strTaxAuthority = ITaxValoremTD[0].Text;
                                strTaxRate = ITaxValoremTD[1].Text;
                                strTaxAssessedValue = ITaxValoremTD[2].Text;
                                strTaxExemptionAmount = ITaxValoremTD[3].Text;
                                strTaxbleValue = ITaxValoremTD[4].Text;
                                strTaxAmount = ITaxValoremTD[5].Text;
                            }
                            catch { }

                            string strTaxValoremDetails = strValorem + "~" + strTaxAuthority + "~" + strTaxRate + "~" + strTaxAssessedValue + "~" + "-" + "~" + strTaxExemptionAmount + "~" + "-" + "~" + strTaxbleValue + "~" + "-" + "~" + strTaxAmount;
                            gc.insert_date(orderNumber, strAccountNo, 437, strTaxValoremDetails, 1, DateTime.Now);
                        }
                    }

                    IWebElement ITaxValoremTotalTable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[8]/td/table[2]/tbody"));
                    IList<IWebElement> ITaxValoremTotalRow = ITaxValoremTotalTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> ITaxValoremTotalTD;
                    foreach (IWebElement total in ITaxValoremTotalRow)
                    {
                        ITaxValoremTotalTD = total.FindElements(By.TagName("td"));
                        if (ITaxValoremTotalTD.Count != 0)
                        {
                            strTotalMillege = ITaxValoremTotalTD[0].Text;
                            strTotalMillegeAmount = ITaxValoremTotalTD[1].Text;
                            strTotalTax = ITaxValoremTotalTD[2].Text;
                            strTotalTaxAmount = ITaxValoremTotalTD[3].Text;

                            string strTaxValoremDetails = strValorem + "~" + strTotalMillege + "~" + strTotalMillegeAmount + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~"+ "-" + "~" + strTotalTax + "~" + strTotalTaxAmount;
                            gc.insert_date(orderNumber, strAccountNo, 437, strTaxValoremDetails, 1, DateTime.Now);
                        }
                    }

                    try
                    {
                        strTaxNonValorem = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[9]/td")).Text;
                    }
                    catch { }
                    try
                    {
                        IWebElement ITaxNonValoremTable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[10]/td/table[1]/tbody"));
                        IList<IWebElement> ITaxNonValoremRow = ITaxNonValoremTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> ITaxNonValoremTD;
                        foreach (IWebElement Nvalorem in ITaxNonValoremRow)
                        {
                            ITaxNonValoremTD = Nvalorem.FindElements(By.TagName("td"));
                            if (ITaxNonValoremTD.Count != 0 && !Nvalorem.Text.Contains("Code"))
                            {
                                try
                                {
                                    strNonCode = ITaxNonValoremTD[0].Text;
                                    strNonAuthority = ITaxNonValoremTD[1].Text;
                                    strNonAmount = ITaxNonValoremTD[2].Text;

                                    string strTaxNonValoremDetails = strTaxNonValorem + "~" + "-" + "~" + strNonCode + "~" + strNonAuthority + "~" + "-" + "~" + "-" + "~" + "-" + "~" + strNonAmount;
                                    gc.insert_date(orderNumber, strAccountNo, 439, strTaxNonValoremDetails, 1, DateTime.Now);
                                }
                                catch { }
                            }
                        }
                    }
                    catch { }

                    try
                    {
                        IWebElement ITaxTotalAssessTable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[10]/td/table[2]/tbody"));
                        IList<IWebElement> ITaxTotalAssessRow = ITaxTotalAssessTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> ITaxTotalAssessTD;
                        foreach (IWebElement totalass in ITaxTotalAssessRow)
                        {
                            ITaxTotalAssessTD = totalass.FindElements(By.TagName("td"));
                            if (ITaxTotalAssessTD.Count != 0 && totalass.Text.Contains("Total Assessments "))
                            {
                                strTotalAss = ITaxTotalAssessTD[0].Text;
                                strTotalAssAmount = ITaxTotalAssessTD[1].Text;

                                string strTaxTotalAssess = "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + strTotalAss + "~" + strTotalAssAmount;
                                gc.insert_date(orderNumber, strAccountNo, 439, strTaxTotalAssess, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    try
                    {
                        IWebElement ITotalTaxAssess = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[11]/td/table/tbody"));
                        IList<IWebElement> ITotalTaxAssessRow = ITotalTaxAssess.FindElements(By.TagName("tr"));
                        IList<IWebElement> ITotalTaxAssessTD;
                        foreach (IWebElement Totaltax in ITotalTaxAssessRow)
                        {
                            ITotalTaxAssessTD = Totaltax.FindElements(By.TagName("td"));
                            if (ITotalTaxAssessTD.Count != 0 && Totaltax.Text.Contains("Taxes & Assessments "))
                            {
                                strTotalTaxAssess = ITotalTaxAssessTD[2].Text;
                                strTotalTaxAssessAmount = ITotalTaxAssessTD[3].Text;

                                string strTotalTaxAssessDetails = "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + strTotalTaxAssess + "~" + strTotalTaxAssessAmount;
                                gc.insert_date(orderNumber, strAccountNo, 439, strTotalTaxAssessDetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    try
                    {
                        IWebElement IDTaxTable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[4]/tbody/tr/td/table/tbody"));
                        IList<IWebElement> IDTaxRow = IDTaxTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> IDTaxTD;
                        foreach (IWebElement DTax in IDTaxRow)
                        {
                            IDTaxTD = DTax.FindElements(By.TagName("td"));
                            if (IDTaxTD.Count != 0 && !DTax.Text.Contains("If Paid By"))
                            {
                                strDPaid = IDTaxTD[0].Text;
                                strPAmount = IDTaxTD[1].Text;

                                string strDTaxDetails = strDPaid + "~" + strPAmount + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-";
                                gc.insert_date(orderNumber, strAccountNo, 442, strDTaxDetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    try
                    {
                        IWebElement IDTaxTransactionTable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[5]/tbody"));
                        IList<IWebElement> IDTaxTransactionRow = IDTaxTransactionTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> IDTaxTransactionTD;
                        foreach (IWebElement Transaction in IDTaxTransactionRow)
                        {
                            IDTaxTransactionTD = Transaction.FindElements(By.TagName("td"));
                            if (IDTaxTransactionTD.Count != 0 && !Transaction.Text.Contains("Date Paid"))
                            {
                                strDatePaid = IDTaxTransactionTD[0].Text;
                                strTransaction = IDTaxTransactionTD[1].Text;
                                strReciept = IDTaxTransactionTD[2].Text;
                                strDyear = IDTaxTransactionTD[3].Text;
                                strDAmount = IDTaxTransactionTD[4].Text;

                                string strTotalTaxAssessDetails = "-" + "~" + "-" + "~" + strDatePaid + "~" + strTransaction + "~" + strReciept + "~" + strDyear + "~" + strDAmount + "~" + "-";
                                gc.insert_date(orderNumber, strAccountNo, 442, strTotalTaxAssessDetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    try
                    {
                        IWebElement IDPriorTax = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[6]/tbody/tr[2]/td/table/tbody"));
                        IList<IWebElement> IDPriorTaxRow = IDPriorTax.FindElements(By.TagName("tr"));
                        IList<IWebElement> IDPriorTaxTD;
                        foreach (IWebElement prior in IDPriorTaxRow)
                        {
                            IDPriorTaxTD = prior.FindElements(By.TagName("td"));
                            if (IDPriorTaxTD.Count != 0)
                            {
                                strDPrior = IDPriorTaxTD[0].Text;

                                string strTotalTaxAssessDetails = "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + strDPrior;
                                gc.insert_date(orderNumber, strAccountNo, 442, strTotalTaxAssessDetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    try
                    {
                        IWebElement IPaySearch = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[7]/td")).FindElement(By.TagName("a"));
                        string strPaySearch = IPaySearch.GetAttribute("href");
                        driver.Navigate().GoToUrl(strPaySearch);
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, strParcelNumber, "Tax Payment History Result", driver, "FL", "Polk");
                    }
                    catch { }

                    try
                    {
                        IWebElement ITaxPaymentTable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table[2]/tbody"));
                        IList<IWebElement> ITaxPaymentRow = ITaxPaymentTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> ITaxPaymentTD;
                        foreach (IWebElement Pay in ITaxPaymentRow)
                        {
                            ITaxPaymentTD = Pay.FindElements(By.TagName("td"));
                            if (ITaxPaymentTD.Count != 0 && !Pay.Text.Contains("Account Number "))
                            {
                                strTaxPayType = ITaxPaymentTD[1].Text;
                                strTaxPayYear = ITaxPaymentTD[2].Text;

                                string strTotalTaxAssessD = strTaxPayType + "~" + strTaxPayYear + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-";
                                gc.insert_date(orderNumber, strAccountNo, 443, strTotalTaxAssessD, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    for (int i=3;i<30;i++)
                    {
                        try
                        {
                            IWebElement ITaxPHTable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table[" + i + "]/tbody"));
                            IList <IWebElement> ITaxPHRow = ITaxPHTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> ITaxPHTD;
                            foreach (IWebElement History in ITaxPHRow)
                            {
                                ITaxPHTD = History.FindElements(By.TagName("td"));
                                if (ITaxPHTD.Count != 0 && !History.Text.Contains("Year ") && !History.Text.Contains("Paid By ") && !History.Text.Contains("Payment History"))
                                {
                                        try
                                        {
                                            strTaxPHYear = ITaxPHTD[0].Text;
                                            strTaxPayFolio = ITaxPHTD[1].Text;
                                            strTaxPayDatePaid = ITaxPHTD[2].Text;
                                            strTaxPayReceipt = ITaxPHTD[3].Text;
                                            strTaxPayAmountBilled = ITaxPHTD[4].Text;
                                            strTaxPayAmountPaid = ITaxPHTD[5].Text;

                                            strTaxPayDetails = strTaxPHYear + "~" + strTaxPayFolio + "~" + strTaxPayDatePaid + "~" + strTaxPayReceipt + "~" + strTaxPayAmountBilled + "~" + strTaxPayAmountPaid;
                                    }
                                        catch { }
                                }
                                string[] strTaxPayCount = strTaxPayDetails.Split('~');
                                if (ITaxPHTD.Count != 0 && strTaxPayCount.Length == 6  && (History.Text.Contains("Paid By ") || !History.Text.Trim().Contains("")))
                                {
                                        strTaxPaidBy = ITaxPHTD[1].Text;

                                        if (strTaxPayDetails.Replace("-", "").Replace("~", "") != "")
                                        {
                                            string strTotalTaxAssessDetails = "-" + "~" + "-" + "~" + strTaxPayDetails + "~" + strTaxPaidBy;
                                            gc.insert_date(orderNumber, strAccountNo, 443, strTotalTaxAssessDetails, 1, DateTime.Now);
                                        }
                                }
                            }
                        }
                        catch { }
                    }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "FL", "Polk", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);


                    driver.Quit();
                    gc.mergpdf(orderNumber, "FL", "Polk");
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