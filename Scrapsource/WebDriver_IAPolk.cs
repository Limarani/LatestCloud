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
    public class WebDriver_IAPolk
    {
        string Outparcelno = "", outputPath = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        private TimeSpan timeOutInSeconds;
        MySqlParameter[] mParam;
        GlobalClass gc = new GlobalClass();
        IWebElement Itax, IAssessTable, IPre, ISACertificate, IMulti, IYearWise;
        string strmulti = "", strDisParcel = "-", strGeoParcel = "-", strMap = "-", strNbhd = "-", strJuri = "-", strSchool = "-", strStreet = "-", strCityZip = "-", strMailingAddress = "-", strLegalDiscription = "-", strAcres = "-", strYearBuild = "-", strYear = "-", strType = "-",
               strClass = "-", strKind = "-", strLand = "-", strBuilding = "-", strTotal = "-", strTaxAuthority = "-", strTaxParcel = "-", strTaxGeo = "-", strTaxClass = "-", strTaxPhysicalAddress = "-", strTaxLegalDis = "-", strTaxLegalParty = "-", strTaxDate = "-",
               strBill = "-", strTaxYear = "-", strSATYear = "-", strTaxBil = "-", strIntallment = "-", strTax = "-", strTaxFee = "-", strTaxInterest = "-", strTaxTotal = "-", strPayInstallment = "-", strPayYear = "-", strPayBill = "-", strPayReceipt = "-", strAmountPaid = "-",
               strDatePaid = "-", strTaxDis = "-", strTaxDisYear = "-", strTaxDisAuth = "-", strTaxDisGross = "-", strTaxDisPercentage = "-", strTaxDisCredit = "-", strTaxDisNet = "-", strSATaxYear = "-", strSATaxBill = "-", strSATaxInstall = "-", strSATaxInterest = "-",
               strSATaxLateInte = "-", strSATaxFee = "-", strSATaxTotal = "-", strSATaxDate = "-", strSATa = "-", strSABond = "-", strBookDate = "-", strCertificate = "-", strAcceptanceDate = "-", strOriginalAmount = "-", strBondInterest = "-", strNoofInstall = "-",
               strFutureBalance = "-", strPHInstallment = "-", strPHYear = "-", strPHBill = "-", strPHReciept = "-", strPHAmount = "-", strPHDatePaid = "-", strSAPDCertificate = "-", strSAPDAssess = "-", strSAPDDes = "-", strSAPDType = "-", strSAPDRun = "-", strSAPDYear = "-",
               strSAPDAmount = "-", strSAPDSatus = "-", strFutureSourceType = "-", strFutureReceiptNo = "-", strFutureAmount = "-", strFutureDatePaid = "-", strTaxSABill = "-", strSAType = "-", strSACertificateNo = "-", strSATaxType = "-", strSADiscrip = "-", strSATax = "-",
               strSAInterest = "-", strSATotal = "-", strSATotalTax = "-", strSATotalInterest = "-", strSATotalTotal = "-", strSALateInterest = "-", strSAFees = "-", strSANetTotal = "-", strSATDTaxDis = "-", strSATDTaxBill = "-", strSATDTaxASS = "-", strSATDCertificate = "-",
               strSATDType = "-", strSATDDis = "-", strSATDTax = "-", strSATDInterest = "-", strSATDTotal = "-", strSATDTotalTax = "-", strSATDTotalInterest = "-", strSATDTotalTotal = "-", strSATDLateInterest = "-", strSATDFees = "-", strSATDNet = "-", strTaxConsolidate = "-",
               strTaxConsoli = "-", strTTotal = "-", strTotalGross = "-", strTotalcredit = "-", strNetTotal = "-", strSATaxTax = "-", strSATaxSAInterest = "-", strSAPHType = "-", strSAPHYear = "-", strURL = "-", strSATaxASS = "-", strSACertificate = "-", strSADType = "-", strSADis = "-",
               strSADTax = "-", strSADInterest = "-", strSADTotal = "-", strAcresTitle = "-", strYearBuildTitle = "-", strSATTaxInstall = "-", strSATTaxTax = "-", strSATTaxSAInterest = "-", strSATTaxLateInte = "-", strSATTaxFee = "-", strSATTaxTotal = "-", strSATTaxDate = "-", strLegalParty = "-", strPDYear = "", strparcel = "";
        string filename, link, type;
        int j = 0, l = 0;


        public string FTP_IAPolk(string houseno, string direction, string sname, string sttype, string parcelNumber, string searchType, string orderNumber, string ownername, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    if (searchType == "titleflex")
                    {
                        string address = houseno + " " + sname;
                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "IA", "Polk");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_IAPlok"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    driver.Navigate().GoToUrl("http://www.assess.co.polk.ia.us/web/inven/query/queryAll.html");
                    Thread.Sleep(3000);
                    if (searchType == "address")
                    {
                        driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/form/table[2]/tbody/tr[1]/td/table/tbody/tr[2]/td[2]/input")).SendKeys(houseno);
                        driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/form/table[2]/tbody/tr[1]/td/table/tbody/tr[2]/td[6]/input")).SendKeys(sname);
                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "IA", "Polk");
                        driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/form/table[1]/tbody/tr[1]/td[4]/input")).SendKeys(Keys.Enter);
                        Thread.Sleep(6000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "IA", "Polk");
                        try
                        {
                            IWebElement ImultiTable = driver.FindElement(By.XPath("/html/body/center/table/tbody/tr[3]/td/form/table/tbody"));
                            IList<IWebElement> ImultiRow = ImultiTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> ImultiTD;
                            foreach (IWebElement row in ImultiRow)
                            {
                                ImultiTD = row.FindElements(By.TagName("td"));
                                if (ImultiTD.Count != 0 && (!row.Text.Contains("District/Parcel") || !row.Text.Contains("Ownership")))
                                {
                                    IWebElement IParcel = ImultiTD[0].FindElement(By.TagName("input"));
                                    parcelNumber = IParcel.GetAttribute("value");
                                    string strmultiDetails = ImultiTD[1].Text + "~" + ImultiTD[2].Text + "~" + ImultiTD[3].Text + "~" + ImultiTD[4].Text;
                                    gc.insert_date(orderNumber, parcelNumber, 191, parcelNumber + "~" + strmultiDetails, 1, DateTime.Now);
                                }
                                if (row.Text.Contains("Records"))
                                {
                                    try
                                    {
                                        strmulti = GlobalClass.Before(row.Text, "Records");
                                    }
                                    catch { }
                                }
                            }
                            if (Convert.ToInt16(strmulti) <= 25)
                            {
                                HttpContext.Current.Session["multiparcel_Polk"] = "Yes";
                                gc.CreatePdf_WOP(orderNumber, "Multi Parcel Search", driver, "IA", "Polk");
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Convert.ToInt16(strmulti) >= 25)
                            {
                                HttpContext.Current.Session["multiparcel_Polk_Maximum"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }
                    }

                    else if (searchType == "parcel")
                    {
                        string strParcelNumber = parcelNumber.Replace("/", "").Replace("-", "").Replace(".", "");
                        string strDistrict = strParcelNumber.Substring(0, 3);
                        driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/form/table[2]/tbody/tr[3]/td/table/tbody/tr[2]/td[2]/input")).SendKeys(strDistrict);
                        string strparcel1 = strParcelNumber.Substring(3, 5);
                        driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/form/table[2]/tbody/tr[3]/td/table/tbody/tr[2]/td[3]/input")).SendKeys(strparcel1);
                        string strparcel2 = strParcelNumber.Substring(8, 3);
                        driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/form/table[2]/tbody/tr[3]/td/table/tbody/tr[2]/td[4]/input")).SendKeys(strparcel2);
                        string strparcel3 = strParcelNumber.Substring(11, 3);
                        driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/form/table[2]/tbody/tr[3]/td/table/tbody/tr[2]/td[5]/input")).SendKeys(strparcel3);
                        gc.CreatePdf(orderNumber, strParcelNumber, "Parcel Search", driver, "IA", "Polk");
                        driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/form/table[1]/tbody/tr[1]/td[4]/input")).SendKeys(Keys.Enter);
                        Thread.Sleep(6000);
                        gc.CreatePdf(orderNumber, strParcelNumber, "Parcel Search Result", driver, "IA", "Polk");
                    }
                    else if (searchType == "ownername")
                    {
                        string[] strowner = ownername.Trim().TrimStart().TrimEnd().Replace(",", "").Split(' ');
                        string strLastName = strowner[0];
                        string strFirstName = strowner[1];
                        driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/form/table[2]/tbody/tr[2]/td/table/tbody/tr[2]/td[2]/input")).SendKeys(strLastName);
                        driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/form/table[2]/tbody/tr[2]/td/table/tbody/tr[2]/td[3]/input")).SendKeys(strFirstName);
                        gc.CreatePdf_WOP(orderNumber, "Owner Search", driver, "IA", "Polk");
                        driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/form/table[1]/tbody/tr[1]/td[4]/input")).SendKeys(Keys.Enter);
                        gc.CreatePdf_WOP(orderNumber, "Owner Search Result", driver, "IA", "Polk");
                        Thread.Sleep(6000);
                        try
                        {
                            IWebElement ImultiTable = driver.FindElement(By.XPath("/html/body/center/table/tbody/tr[3]/td/form/table/tbody"));
                            IList<IWebElement> ImultiRow = ImultiTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> ImultiTD;
                            foreach (IWebElement row in ImultiRow)
                            {
                                ImultiTD = row.FindElements(By.TagName("td"));
                                if (ImultiTD.Count != 0 && (!row.Text.Contains("District/Parcel") || !row.Text.Contains("Ownership")))
                                {
                                    IWebElement IParcel = ImultiTD[0].FindElement(By.TagName("input"));
                                    parcelNumber = IParcel.GetAttribute("value");
                                    string strmultiDetails = ImultiTD[1].Text + "~" + ImultiTD[2].Text + "~" + ImultiTD[3].Text + "~" + ImultiTD[4].Text;
                                    gc.insert_date(orderNumber, parcelNumber, 191, parcelNumber + "~" + strmultiDetails, 1, DateTime.Now);
                                }
                                if (row.Text.Contains("Records"))
                                {
                                    strmulti = GlobalClass.Before(row.Text, "Records");
                                }
                            }
                            if (Convert.ToInt16(strmulti) <= 25)
                            {
                                HttpContext.Current.Session["multiparcel_Polk"] = "Yes";
                                gc.CreatePdf_WOP(orderNumber, "Multi Parcel Search", driver, "IA", "Polk");
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Convert.ToInt16(strmulti) > 25)
                            {
                                HttpContext.Current.Session["multiparcel_Polk_Maximum"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }
                    }

                    try
                    {
                        IWebElement ImultiTable = driver.FindElement(By.XPath("/html/body/center/table/tbody/tr[3]"));
                        if(ImultiTable.Text.Contains("0 Records"))
                        {
                            HttpContext.Current.Session["Nodata_IAPlok"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    //Property and Assessment
                    try
                    {
                        strparcel = driver.FindElement(By.XPath("/html/body/center/table[1]/tbody/tr[2]/td/table/tbody/tr[2]/th[1]/font")).Text;
                        parcelNumber = strparcel.Replace("/", "").Replace("-", "").Replace(".", "");
                        strGeoParcel = driver.FindElement(By.XPath("/html/body/center/table[1]/tbody/tr[2]/td/table/tbody/tr[2]/th[2]/font")).Text;
                        strMap = driver.FindElement(By.XPath("/html/body/center/table[1]/tbody/tr[2]/td/table/tbody/tr[2]/td[1]")).Text.Trim();
                        strNbhd = driver.FindElement(By.XPath("/html/body/center/table[1]/tbody/tr[2]/td/table/tbody/tr[2]/td[2]")).Text.Trim();
                        strJuri = driver.FindElement(By.XPath("/html/body/center/table[1]/tbody/tr[2]/td/table/tbody/tr[2]/th[3]")).Text.Trim();
                        strSchool = driver.FindElement(By.XPath("/html/body/center/table[1]/tbody/tr[2]/td/table/tbody/tr[4]/td[1]")).Text.Trim();
                        strStreet = driver.FindElement(By.XPath("/html/body/center/table[1]/tbody/tr[2]/td/table/tbody/tr[6]/th[1]/font")).Text.Trim();
                        strCityZip = driver.FindElement(By.XPath("/html/body/center/table[1]/tbody/tr[2]/td/table/tbody/tr[6]/th[2]")).Text.Trim();
                        strMailingAddress = driver.FindElement(By.XPath("/html/body/center/table[1]/tbody/tr[4]/td/table/tbody/tr[2]/td")).Text.Trim();
                        strLegalDiscription = driver.FindElement(By.XPath("/html/body/center/table[1]/tbody/tr[5]/td/table/tbody/tr[2]/td")).Text.Trim();

                        try
                        {
                            strAcresTitle = driver.FindElement(By.XPath("/html/body/center/table/tbody/tr[9]/td/table/tbody/tr[3]/td[1]")).Text;
                            if ((strAcresTitle == "ACRES"))
                            {
                                strAcresTitle = "";
                                strAcres = driver.FindElement(By.XPath("/html/body/center/table/tbody/tr[9]/td/table/tbody/tr[3]/td[2]")).Text;
                            }
                        }
                        catch { }
                        try
                        {
                            strAcresTitle = driver.FindElement(By.XPath("/html/body/center/table/tbody/tr[9]/td/table/tbody/tr[3]/td[3]")).Text;
                            if ((strAcresTitle == "ACRES") && (strAcres == "-" || strAcres == ""))
                            {
                                strAcresTitle = "";
                                strAcres = driver.FindElement(By.XPath("/html/body/center/table/tbody/tr[9]/td/table/tbody/tr[3]/td[4]")).Text;
                            }
                        }
                        catch { }
                        try
                        {
                            strAcresTitle = driver.FindElement(By.XPath("/html/body/center/table[1]/tbody/tr[9]/td/table/tbody/tr[3]/td[1]")).Text;
                            if ((strAcresTitle == "ACRES") && (strAcres == "-" || strAcres == ""))
                            {
                                strAcresTitle = "";
                                strAcres = driver.FindElement(By.XPath("/html/body/center/table[1]/tbody/tr[9]/td/table/tbody/tr[3]/td[2]")).Text.Trim();
                            }
                        }
                        catch { }
                        try
                        {
                            strAcresTitle = driver.FindElement(By.XPath("/html/body/center/table[1]/tbody/tr[10]/td/table/tbody/tr[2]/td[3]")).Text;
                            if ((strAcresTitle == "ACRES") && (strAcres == "-" || strAcres == ""))
                            {
                                strAcresTitle = "";
                                strAcres = driver.FindElement(By.XPath("/html/body/center/table[1]/tbody/tr[10]/td/table/tbody/tr[2]/td[4]")).Text;
                            }
                        }
                        catch { }
                        try
                        {
                            strAcresTitle = driver.FindElement(By.XPath("/html/body/center/table[1]/tbody/tr[10]/td/table/tbody/tr[3]/td[1]")).Text;
                            if ((strAcresTitle == "ACRES") && (strAcres == "-" || strAcres == ""))
                            {
                                strAcresTitle = "";
                                strAcres = driver.FindElement(By.XPath("/html/body/center/table[1]/tbody/tr[10]/td/table/tbody/tr[3]/td[2]")).Text;
                            }
                        }
                        catch { }
                        try
                        {
                            strAcresTitle = driver.FindElement(By.XPath("/html/body/center/table[1]/tbody/tr[9]/td/table/tbody/tr[2]/td[3]")).Text;
                            if ((strAcresTitle == "ACRES") && (strAcres == "-" || strAcres == ""))
                            {
                                strAcresTitle = "";
                                strAcres = driver.FindElement(By.XPath("/html/body/center/table[1]/tbody/tr[9]/td/table/tbody/tr[2]/td[4]")).Text;
                            }
                        }
                        catch { }
                        try
                        {
                            strYearBuildTitle = driver.FindElement(By.XPath("/html/body/center/table[1]/tbody/tr[10]/td/table/tbody/tr[3]/td[1]")).Text;
                            if ((strYearBuildTitle == "YEAR BUILT"))
                            {
                                strYearBuildTitle = "";
                                strYearBuild = driver.FindElement(By.XPath("/html/body/center/table[1]/tbody/tr[10]/td/table/tbody/tr[3]/td[2]")).Text;
                            }
                        }
                        catch { }
                        try
                        {
                            strYearBuildTitle = driver.FindElement(By.XPath("/html/body/center/table[1]/tbody/tr[11]/td/table/tbody/tr[3]/td[1]")).Text;
                            if ((strYearBuildTitle == "YEAR BUILT") && (strYearBuild == "-" || strYearBuild == ""))
                            {
                                strYearBuildTitle = "";
                                strYearBuild = driver.FindElement(By.XPath("/html/body/center/table[1]/tbody/tr[11]/td/table/tbody/tr[3]/td[2]")).Text;
                            }
                        }
                        catch { }
                        string strPropertyDetails = strGeoParcel.Trim() + "~" + strMap.Trim() + "~" + strNbhd.Trim() + "~" + strJuri.Trim() + "~" + strSchool.Trim() + "~" + strStreet.Trim().Replace("\r\n", " ") + "~" + strCityZip.Trim() + "~" + strMailingAddress.Trim().Replace("\r\n", " ") + "~" + strLegalDiscription.Trim() + "~" + strAcres.Trim() + "~" + strYearBuild.Trim();
                        gc.insert_date(orderNumber, strparcel, 189, strPropertyDetails, 1, DateTime.Now);
                    }
                    catch { }
                    string currentYear = DateTime.Now.Year.ToString();
                    string stryear = currentYear.Substring(0, 3);
                    string year = currentYear.Substring(0, 2);
                    try
                    {
                        for (int i = 3; i < 6; i++)
                        {
                            IAssessTable = driver.FindElement(By.XPath("/html/body/center/table[3]/tbody/tr[" + i + "]/td/table/tbody"));
                            IList<IWebElement> IAssessRow = IAssessTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> IAssessTD;
                            foreach (IWebElement row in IAssessRow)
                            {
                                IAssessTD = row.FindElements(By.TagName("td"));
                                if (IAssessTD.Count != 0 && (row.Text.Contains(currentYear) || row.Text.Contains(stryear) || row.Text.Contains(year) || row.Text.Contains("Board Action") || row.Text.Contains("Assessment Roll") || row.Text.Contains("Final Value")))
                                {
                                    strYear = IAssessTD[0].Text;
                                    strType = IAssessTD[1].Text;
                                    strClass = IAssessTD[2].Text;
                                    strLand = IAssessTD[4].Text;
                                    strBuilding = IAssessTD[5].Text;
                                    strTotal = IAssessTD[7].Text;

                                    string strAssessDetails = strYear + "~" + strType + "~" + strClass + "~" + strLand + "~" + strBuilding + "~" + strTotal;
                                    gc.insert_date(orderNumber, strparcel, 190, strAssessDetails, 1, DateTime.Now);

                                }
                            }


                            IList<IWebElement> Itaxdownload = IAssessTable.FindElements(By.TagName("a"));
                            IList<IWebElement> Itaxlinkass;
                            List<string> strURL = new List<string>();
                            List<string> strtype = new List<string>();
                            foreach (IWebElement Ilist in Itaxdownload)
                            {
                                link = Ilist.GetAttribute("href");
                                if (Ilist.Text.Contains("Board Action"))
                                {
                                    strURL.Add(link);
                                    strtype.Add("Board Action");
                                }
                                if (Ilist.Text.Contains("Assessment Roll"))
                                {
                                    strURL.Add(link);
                                    strtype.Add("Assessment Roll");
                                }
                                if (Ilist.Text.Contains("Final Value"))
                                {
                                    strURL.Add(link);
                                    strtype.Add("Final Value");
                                }
                            }
                            foreach (string URL in strURL)
                            {
                                type = strtype[j];
                                if (type.Contains("Assessment Roll") || type.Contains("Board Action") || type.Contains("Final Value"))
                                {
                                    filename = GlobalClass.After(URL, "&yr=");
                                    driver.Navigate().GoToUrl(URL);
                                    gc.CreatePdf(orderNumber, parcelNumber, "Assessment" + filename + type, driver, "IA", "Polk");
                                }
                                j++;
                            }
                        }
                    }
                    catch { }

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    try
                    {
                        driver.Navigate().GoToUrl("https://www.polkcountyiowa.gov/treasurer/");
                        Thread.Sleep(3000);
                        string strTaxA = driver.FindElement(By.XPath("//*[@id='contactUs']")).Text;
                        strTaxAuthority = gc.Between(strTaxA, "Contact Us\r\n", "\r\nVehicle: ").Replace("\r\n", " ");
                    }
                    catch { }



                    //Tax Details
                    driver.Navigate().GoToUrl("http://taxsearch.polkcountyiowa.gov/Search");
                    Thread.Sleep(3000);
                    try
                    {
                        IWebElement IselectParcel = driver.FindElement(By.Id("select2-chosen-1"));
                        SelectElement selectParcel = new SelectElement(IselectParcel);
                        selectParcel.SelectByText("Parcel Number");
                    }
                    catch { }
                    string strParcel = parcelNumber.Replace("/", ".").Replace("-", ".");
                    driver.FindElement(By.Id("SearchModel_searchTerm")).SendKeys(strParcel);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Details Search", driver, "IA", "Polk");
                    IWebElement Isearch = driver.FindElement(By.XPath("//*[@id='search-form-post']/fieldset/div/div/div[4]/button"));
                    IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                    js.ExecuteScript("arguments[0].click();", Isearch);
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Details Search Result", driver, "IA", "Polk");
                    IWebElement IParcelTable = driver.FindElement(By.XPath("//*[@id='selectionTable']/tbody/tr/td[3]/a"));
                    js.ExecuteScript("arguments[0].click();", IParcelTable);
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Details Result", driver, "IA", "Polk");
                    ////*[@id="header-row-details"]/div[1]
                    //*[@id="page-wrapper"]/div[6]/div[2]/div[2]
                    //*[@id="page-wrapper"]/div[6]/div[4]/div[2]
                    //*[@id="page-wrapper"]/div[6]/div[6]/div[2]
                    strTaxParcel = driver.FindElement(By.XPath("//*[@id='header-row-details']/div[1]")).Text;
                    strTaxGeo = driver.FindElement(By.XPath("//*[@id='header-row-details']/div[2]")).Text;
                    strTaxClass = driver.FindElement(By.XPath("//*[@id='header-row-details']/div[3]")).Text;
                    try
                    {
                        strTaxPhysicalAddress = driver.FindElement(By.XPath("//*[@id='page-wrapper']/div[5]/div[2]/div[2]")).Text;
                        strTaxLegalDis = driver.FindElement(By.XPath("//*[@id='page-wrapper']/div[5]/div[4]/div[2]")).Text;
                        strTaxLegalParty = driver.FindElement(By.XPath("//*[@id='page-wrapper']/div[5]/div[6]/div[2]")).Text;
                    }
                    catch
                    {
                        strTaxPhysicalAddress = driver.FindElement(By.XPath("//*[@id='page-wrapper']/div[6]/div[2]/div[2]")).Text;
                        strTaxLegalDis = driver.FindElement(By.XPath("//*[@id='page-wrapper']/div[6]/div[4]/div[2]")).Text;
                        strTaxLegalParty = driver.FindElement(By.XPath("//*[@id='page-wrapper']/div[6]/div[6]/div[2]")).Text;

                    }
                    try
                    {
                        for (int i = 7; i < 11; i++)
                        {
                            strLegalParty = " & " + driver.FindElement(By.XPath("//*[@id='page-wrapper']/div[5]/div[" + i + "]/div[2]")).Text;
                        }
                    }
                    catch
                    {



                    }
                    try
                    {
                        for (int i = 7; i < 11; i++)
                        {
                            strLegalParty = " & " + driver.FindElement(By.XPath("//*[@id='page-wrapper']/div[6]/div[" + i + "]/div[2]")).Text;
                        }
                    }
                    catch { }
                    if (strLegalParty.Trim() != "" && strLegalParty.Trim() != "-")
                    {
                        strTaxLegalParty += strLegalParty;
                    }

                    string strParcelInfo = strTaxGeo + "~" + strTaxClass + "~" + strTaxPhysicalAddress + "~" + strTaxLegalDis + "~" + strTaxLegalParty + "~" + strTaxAuthority;
                    gc.insert_date(orderNumber, strTaxParcel, 192, strParcelInfo, 1, DateTime.Now);

                    try
                    {
                        IWebElement IRealTax = driver.FindElement(By.XPath("/html/body/div[1]/nav/div[3]/ul/li[3]/a"));
                        js.ExecuteScript("arguments[0].click();", IRealTax);
                        Thread.Sleep(6000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Real Estate Details", driver, "IA", "Polk");
                        IWebElement IRealTaxYear = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[4]/a"));
                        js.ExecuteScript("arguments[0].click();", IRealTaxYear);
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Current Tax Installment yearWise Details", driver, "IA", "Polk");
                    }
                    catch { }

                    //Current Tax Installment Details(Real Estate)
                    IWebElement IRealTaxInstall = driver.FindElement(By.XPath("/html/body/div[1]/nav/div[3]/ul/li[4]/a"));
                    js.ExecuteScript("arguments[0].click();", IRealTaxInstall);
                    Thread.Sleep(5000);
                    try
                    {
                        gc.CreatePdf(orderNumber, parcelNumber, "Current Tax Installment Details", driver, "IA", "Polk");
                    }
                    catch { }
                    try
                    {
                        strTaxYear = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[4]")).Text;
                    }
                    catch { }
                    try
                    {
                        if (strTaxYear.Trim() == "")
                        {
                            strTaxYear = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[3]")).Text;
                        }
                    }
                    catch { }
                    try
                    {
                        strBill = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[2]")).Text;
                        strTaxBil = GlobalClass.After(strBill, "Bill Number ");
                    }
                    catch { }

                    for (int i = 3; i < 9; i++)
                    {
                        try
                        {
                            strIntallment = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[1]")).Text;
                            strTax = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[3]")).Text;
                            strTaxFee = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[5]")).Text;
                            strTaxInterest = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[7]")).Text;
                            strTaxTotal = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[9]")).Text;
                            try
                            {
                                strTaxDate = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[11]")).Text;
                            }
                            catch { }

                            string strTaxInstallment = "Real Estate Tax Installment" + "~" + strTaxYear + "~" + strTaxBil + "~" + strIntallment + "~" + strTax + "~" + strTaxFee + "~" + "-" + "~" + "-" + "~" + strTaxInterest + "~" + strTaxTotal + "~" + strTaxDate;
                            gc.insert_date(orderNumber, strTaxParcel, 193, strTaxInstallment, 1, DateTime.Now);
                        }
                        catch { }
                    }


                    //PaymentHistory(Real Estate)
                    IWebElement IRealTaxPayment = driver.FindElement(By.XPath("/html/body/div[1]/nav/div[3]/ul/li[5]/a"));
                    js.ExecuteScript("arguments[0].click();", IRealTaxPayment);
                    Thread.Sleep(6000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Payment History Details", driver, "IA", "Polk");

                    try
                    {
                        for (int i = 3; i < 33; i++)
                        {
                            strPayInstallment = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[2]")).Text;
                            strPayYear = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[4]")).Text;
                            strPayBill = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[6]")).Text;
                            strPayReceipt = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[8]")).Text;
                            strAmountPaid = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[10]")).Text;
                            strDatePaid = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[12]")).Text;

                            string strPaidDetails = "Real Estate Payment History" + "~" + strPayInstallment + "~" + strPayYear + "~" + strPayBill + "~" + strPayReceipt + "~" + strAmountPaid + "~" + strDatePaid;
                            gc.insert_date(orderNumber, strTaxParcel, 194, strPaidDetails, 1, DateTime.Now);
                        }
                    }
                    catch { }

                    //Tax Distribution(Real Estate)   
                    IWebElement IRealTaxDis = driver.FindElement(By.XPath("/html/body/div[1]/nav/div[3]/ul/li[6]/a"));
                    js.ExecuteScript("arguments[0].click();", IRealTaxDis);
                    Thread.Sleep(5000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Distribution Details", driver, "IA", "Polk");
                    try
                    {
                        strTaxDisYear = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[3]")).Text;
                    }
                    catch { }
                    try
                    {
                        strTaxDisYear = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[4]")).Text;
                    }
                    catch { }
                    string strTaxDistribution = "Real Estate Tax Distribution" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + strTaxDisYear;
                    gc.insert_date(orderNumber, strTaxParcel, 195, strTaxDistribution, 1, DateTime.Now);
                    try
                    {
                        for (int i = 3; i < 14; i++)
                        {
                            try
                            {
                                strTaxDisAuth = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[2]")).Text;
                                strTaxDisGross = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[4]")).Text;
                                strTaxDisPercentage = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[6]")).Text;
                                strTaxDisCredit = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[8]")).Text;
                                strTaxDisNet = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[10]")).Text;

                                strTaxDistribution = strTaxDisAuth + "~" + strTaxDisGross + "~" + strTaxDisPercentage + "~" + strTaxDisCredit + "~" + strTaxDisNet;
                                gc.insert_date(orderNumber, strTaxParcel, 195, strTaxDistribution, 1, DateTime.Now);
                            }
                            catch { }

                            try
                            {
                                strTaxConsolidate = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[1]")).Text;
                                strTaxConsoli = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[2]")).Text;

                                if (strTaxConsolidate.Contains("Consolidated Tax Levy Non-Agricultural Rate") || strTaxConsolidate.Contains("Consolidated Tax"))
                                {
                                    string strTaxCDistribution = "Real Estate Tax Distribution" + "~" + "-" + "~" + "-" + "~" + strTaxConsolidate + "~" + strTaxConsoli;
                                    gc.insert_date(orderNumber, strTaxParcel, 195, strTaxCDistribution, 1, DateTime.Now);
                                }
                                try
                                {
                                    strTTotal = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[1]")).Text;
                                    strTotalGross = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[3]")).Text;
                                    strTotalcredit = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[6]")).Text;
                                    strNetTotal = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[8]")).Text;

                                    if (strTTotal.Contains("Total") && strTotalGross.Contains("$") && strTotalcredit.Contains("$") && strNetTotal.Contains("$"))
                                    {
                                        string strTTaxDistri = "Real Estate Tax Distribution" + "~" + strTTotal + "~" + strTotalGross + "~" + strTotalcredit + "~" + strNetTotal;
                                        gc.insert_date(orderNumber, strTaxParcel, 195, strTTaxDistri, 1, DateTime.Now);
                                    }
                                }
                                catch { }
                            }
                            catch { }
                        }
                    }
                    catch { }

                    //Special Assessment Tax Dis Year Wise 
                    IWebElement IRealTaxSADisYear = driver.FindElement(By.XPath("/html/body/div[1]/nav/div[3]/ul/li[7]/a"));
                    js.ExecuteScript("arguments[0].click();", IRealTaxSADisYear);
                    Thread.Sleep(5000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Special Assessment Tax Installment Year Wise", driver, "IA", "Polk");
                    try
                    {
                        IWebElement IRealTaxSADisYearwise = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[3]/a"));
                        js.ExecuteScript("arguments[0].click();", IRealTaxSADisYearwise);
                        Thread.Sleep(5000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Special Assessment Tax Installment Year Wise Result", driver, "IA", "Polk");

                    }
                    catch { }
                    try
                    {
                        IWebElement IRealTaxSADisYearwise = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[3]/a"));
                        js.ExecuteScript("arguments[0].click();", IRealTaxSADisYearwise);
                        Thread.Sleep(5000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Special Assessment Tax Installment Year Wise Result", driver, "IA", "Polk");
                    }
                    catch { }


                    //Special Assessment
                    IWebElement IRealTaxSA = driver.FindElement(By.XPath("/html/body/div[1]/nav/div[3]/ul/li[8]/a"));
                    js.ExecuteScript("arguments[0].click();", IRealTaxSA);
                    Thread.Sleep(5000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Special Assessment Tax Installment", driver, "IA", "Polk");
                    try
                    {
                        strSATaxYear = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[3]")).Text;
                    }
                    catch { }
                    try
                    {
                        strSATaxYear = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[4]")).Text;
                    }
                    catch { }
                    try
                    {
                        strSATaxBill = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[2]")).Text;
                        strSATaxBill = GlobalClass.After(strSATaxBill, "Bill Number ");
                    }
                    catch { }
                    try
                    {
                        for (int i = 3; i < 6; i++)
                        {
                            strSATaxInstall = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[2]")).Text;
                            strSATaxTax = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[4]")).Text;
                            strSATaxSAInterest = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[6]")).Text;
                            strSATaxLateInte = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[8]")).Text;
                            strSATaxFee = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[10]")).Text;
                            strSATaxTotal = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[12]")).Text;
                            try
                            {
                                strSATaxDate = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[14]")).Text;
                            }
                            catch { }
                            if (strSATaxInstall != "" && strSATaxTax != "" && strSATaxSAInterest != "" && strSATaxLateInte != "" && strSATaxFee != "" && strSATaxTotal != "")
                            {
                                string strSATaxDistribution = "Special Assessment Tax Installment" + "~" + strSATaxYear + "~" + strSATaxBill + "~" + strSATaxInstall + "~" + strSATaxTax + "~" + strSATaxSAInterest + "~" + strSATaxLateInte + "~" + strSATaxFee + "~" + "-" + "~" + strSATaxTotal + "~" + strSATaxDate;
                                gc.insert_date(orderNumber, strTaxParcel, 193, strSATaxDistribution, 1, DateTime.Now);
                            }
                            try
                            {
                                //Total Due
                                strSATTaxInstall = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[1]")).Text;
                                strSATTaxTax = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[3]")).Text;
                                strSATTaxSAInterest = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[5]")).Text;
                                strSATTaxLateInte = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[7]")).Text;
                                strSATTaxFee = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[9]")).Text;
                                strSATTaxTotal = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[11]")).Text;
                                try
                                {
                                    strSATaxDate = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[13]")).Text;
                                }
                                catch { }
                                if (strSATTaxInstall.Contains("Total Due"))
                                {
                                    string strSATTaxDistribution = "Special Assessment Tax Installment" + "~" + strSATaxYear + "~" + strSATaxBill + "~" + strSATTaxInstall + "~" + strSATTaxTax + "~" + strSATTaxSAInterest + "~" + strSATTaxLateInte + "~" + strSATTaxFee + "~" + "-" + "~" + strSATTaxTotal + "~" + strSATTaxDate;
                                    gc.insert_date(orderNumber, strTaxParcel, 193, strSATTaxDistribution, 1, DateTime.Now);
                                    break;
                                }
                            }
                            catch { }
                        }
                    }
                    catch { }

                    //Special Assessment Tax Payment History 
                    IWebElement IRealTaxSAPH = driver.FindElement(By.XPath("/html/body/div[1]/nav/div[3]/ul/li[9]/a"));
                    js.ExecuteScript("arguments[0].click();", IRealTaxSAPH);
                    Thread.Sleep(5000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Special Assessment Payment History", driver, "IA", "Polk");

                    try
                    {
                        for (int i = 3; i < 10; i++)
                        {
                            strPHInstallment = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[2]")).Text;
                            strPHYear = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[4]")).Text;
                            strPHBill = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[6]")).Text;
                            strPHReciept = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[8]")).Text;
                            strPHAmount = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[10]")).Text;
                            strPHDatePaid = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[12]")).Text;

                            string strPHDetails = "Special Assessment Payment History" + "~" + strPHInstallment + "~" + strPHYear + "~" + strPHBill + "~" + strPHReciept + "~" + strPHAmount + "~" + strPHDatePaid;
                            gc.insert_date(orderNumber, strTaxParcel, 194, strPHDetails, 1, DateTime.Now);
                        }
                    }
                    catch { }

                    //SpecialAssessment Tax Distribution
                    IWebElement IRealTaxSADis = driver.FindElement(By.XPath("/html/body/div[1]/nav/div[3]/ul/li[10]/a"));
                    js.ExecuteScript("arguments[0].click();", IRealTaxSADis);
                    Thread.Sleep(5000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Special Assessment Tax Distribution Details", driver, "IA", "Polk");

                    try
                    {
                        strSATYear = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[3]")).Text;
                    }
                    catch { }
                    try
                    {
                        strSATYear = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[4]")).Text;
                    }
                    catch { }

                    try
                    {
                        strTaxSABill = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[2]")).Text;
                        strTaxSABill = GlobalClass.After(strTaxSABill, "Bill Number :");
                    }
                    catch { }

                    try
                    {
                        //strSATaxASS = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[4]/div[2]")).Text;
                        //strSACertificate = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[4]/div[4]/a")).Text;
                        //strSADType = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[4]/div[6]")).Text;
                        //strSADis = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[4]/div[8]")).Text;
                        //strSADTax = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[4]/div[10]")).Text;
                        //strSADInterest = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[4]/div[12]")).Text;
                        //strSADTotal = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[4]/div[14]")).Text;

                        //string strSADetail = "Special Assessment Tax Distribution"+"~"+strSATYear + "~" + strTaxSABill + "~" + strSATaxASS + "~" + strSACertificate + "~" + strSADType + "~" + strSADis + "~" + strSADTax + "~" + strSADInterest + "~" + strSADTotal;
                        //gc.insert_date(orderNumber, strTaxParcel, 196, strSADetail, 1, DateTime.Now); 

                        //try
                        //{
                        //    if ((strSATaxASS.Trim() != "" || strSATaxASS.Trim() != "-") && (strSADTotal.Trim() != "" || strSADTotal.Trim() != "-"))
                        //    {

                        //        strSATotalTax = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[5]/div[5]")).Text;
                        //        strSATotalInterest = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[5]/div[7]")).Text;
                        //        strSATotalTotal = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[5]/div[9]")).Text;
                        //        string strSATotalDetails = "Special Assessment Tax Distribution" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "Total" + "~" + strSATotalTax + "~" + strSATotalInterest + "~" + strSATotalTotal;
                        //        gc.insert_date(orderNumber, strTaxParcel, 196, strSATotalDetails, 1, DateTime.Now);

                        //        strSALateInterest = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[6]/div[3]")).Text;
                        //        string strSALateTotal = "Special Assessment Tax Distribution" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "LateInterest" + "~" + strSALateInterest;
                        //        gc.insert_date(orderNumber, strTaxParcel, 196, strSALateTotal, 1, DateTime.Now);

                        //        strSAFees = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[7]/div[3]")).Text;
                        //        string strSAFeesTotal = "Special Assessment Tax Distribution" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "Fees" + "~" + strSAFees;
                        //        gc.insert_date(orderNumber, strTaxParcel, 196, strSAFeesTotal, 1, DateTime.Now);

                        //        strSANetTotal = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[8]/div[3]")).Text;
                        //        string strSANetTotalDetails = "Special Assessment Tax Distribution" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "NetTotal" + "~" + strSANetTotal;
                        //        gc.insert_date(orderNumber, strTaxParcel, 196, strSANetTotalDetails, 1, DateTime.Now);
                        //    }
                        //}
                        //catch { }
                        try
                        {
                            for (int k = 4; k < 15; k++)
                            {
                                l = k;
                                strSATDTaxASS = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + k + "]/div[2]")).Text;
                                strSATDCertificate = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + k + "]/div[4]/a")).Text;
                                strSATDType = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + k + "]/div[6]")).Text;
                                strSATDDis = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + k + "]/div[8]")).Text;
                                strSATDTax = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + k + "]/div[10]")).Text;
                                strSATDInterest = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + k + "]/div[12]")).Text;
                                strSATDTotal = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + k + "]/div[14]")).Text;

                                string strSATDTaxDetails = "Special Assessment Tax Distribution" + "~" + strSATYear + "~" + strTaxSABill + "~" + strSATDTaxASS + "~" + strSATDCertificate + "~" + strSATDType + "~" + strSATDDis + "~" + strSATDTax + "~" + strSATDInterest + "~" + strSATDTotal;
                                gc.insert_date(orderNumber, strTaxParcel, 196, strSATDTaxDetails, 1, DateTime.Now);


                            }
                        }
                        catch { }


                        try
                        {
                            for (int i = l; i < l + 10; i++)
                            {
                                strSATDTotalTax = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[5]")).Text;
                                strSATDTotalInterest = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[7]")).Text;
                                strSATDTotalTotal = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + i + "]/div[9]")).Text;

                                string strSATDTaxTotal = "Special Assessment Tax Distribution" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "Total" + "~" + strSATDTotalTax + "~" + strSATDTotalInterest + "~" + strSATDTotalTotal;
                                gc.insert_date(orderNumber, strTaxParcel, 196, strSATDTaxTotal, 1, DateTime.Now);

                                strSATDLateInterest = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + (i + 1) + "]/div[3]")).Text;
                                string strSATDLateTotal = "Special Assessment Tax Distribution" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "Late Interest" + "~" + strSATDLateInterest;
                                gc.insert_date(orderNumber, strTaxParcel, 196, strSATDLateTotal, 1, DateTime.Now);

                                strSATDFees = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + (i + 2) + "]/div[3]")).Text;
                                string strSATDFeeTotal = "Special Assessment Tax Distribution" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "Fee" + "~" + strSATDFees;
                                gc.insert_date(orderNumber, strTaxParcel, 196, strSATDFeeTotal, 1, DateTime.Now);

                                strSATDNet = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + (i + 3) + "]/div[3]")).Text;
                                string strSATDNetTotal = "Special Assessment Tax Distribution" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "Net Total" + "~" + strSATDNet;
                                gc.insert_date(orderNumber, strTaxParcel, 196, strSATDNetTotal, 1, DateTime.Now);

                            }
                        }
                        catch { }
                    }
                    catch { }

                    try
                    {
                        for (int j = 4; j < 10; j++)
                        {
                            ISACertificate = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[" + j + "]/div[4]/a"));
                            js.ExecuteScript("arguments[0].click();", ISACertificate);
                            Thread.Sleep(5000);
                            string strcerticicateno = ISACertificate.Text;
                            gc.CreatePdf(orderNumber, parcelNumber, "Special Assessment Tax Distribution Certificate Details" + strcerticicateno, driver, "IA", "Polk");

                            try
                            {
                                strSATa = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[3]/div[2]")).Text;
                                strSABond = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[3]/div[4]")).Text;
                                strBookDate = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[3]/div[6]")).Text;
                                if (strBookDate.Trim() == "")
                                {
                                    strBookDate = "-";
                                }
                                strCertificate = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[3]/div[8]")).Text;
                                strAcceptanceDate = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[3]/div[10]")).Text;
                                if (strAcceptanceDate.Trim() == "")
                                {
                                    strAcceptanceDate = "-";
                                }
                                strOriginalAmount = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[3]/div[12]")).Text;
                                strBondInterest = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[5]/div[2]")).Text;
                                strNoofInstall = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[5]/div[4]")).Text;
                                strFutureBalance = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[5]/div[6]")).Text;

                                string strSACD = "Special Assesssment Tax Distribution" + "~" + strSATa + "~" + strSABond + "~" + strBookDate + "~" + strCertificate + "~" + strAcceptanceDate + "~" + strOriginalAmount + "~" + strBondInterest + "~" + strNoofInstall + "~" + strFutureBalance;
                                gc.insert_date(orderNumber, strTaxParcel, 205, strSACD, 1, DateTime.Now);
                            }
                            catch { }
                        }
                    }
                    catch { }


                    IWebElement IRealTaxSAPD = driver.FindElement(By.XPath("/html/body/div[1]/nav/div[3]/ul/li[11]/a"));
                    js.ExecuteScript("arguments[0].click();", IRealTaxSAPD);
                    Thread.Sleep(5000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Special Assessment PrelimsDeficiencies", driver, "IA", "Polk");
                    try
                    {
                        strSAPHType = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[1]")).Text;
                        strSAPHYear = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[3]")).Text;
                    }
                    catch { }
                    try
                    {
                        strSAPHYear = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[1]/div[4]")).Text;
                    }
                    catch { }
                    string strSAPHDetails = strSAPHType + "~" + strSAPHYear + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-";
                    gc.insert_date(orderNumber, strTaxParcel, 204, strSAPHDetails, 1, DateTime.Now);
                    try
                    {
                        strSAPDCertificate = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[3]/div[2]")).Text;
                        strSAPDAssess = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[3]/div[4]")).Text;
                        strSAPDDes = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[3]/div[6]")).Text;
                        strSAPDType = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[3]/div[8]")).Text;
                        strSAPDYear = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[5]/div[2]")).Text;
                        strSAPDRun = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[5]/div[4]")).Text;
                        strSAPDAmount = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[5]/div[6]")).Text;
                        strSAPDSatus = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[5]/div[8]")).Text;

                        string strSAPD = strSAPDCertificate + "~" + strSAPDAssess + "~" + strSAPDDes + "~" + strSAPDType + "~" + strSAPDYear + "~" + strSAPDRun + "~" + strSAPDAmount + "~" + strSAPDSatus;
                        gc.insert_date(orderNumber, strTaxParcel, 204, strSAPD, 1, DateTime.Now);


                        try
                        {
                            IYearWise = driver.FindElement(By.XPath("//*[@id='special-assessment-payments']/div[1]/div[2]"));
                            strPDYear = IYearWise.Text;
                        }
                        catch { }
                        try
                        {
                            IPre = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[3]/div[2]/a"));
                            IPre.SendKeys(Keys.Enter);
                            Thread.Sleep(6000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Special Assessment PrelimsDeficiencies All YearWise", driver, "IA", "Polk");
                        }
                        catch { }

                        strSATa = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[3]/div[2]")).Text;
                        strSABond = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[3]/div[4]")).Text;
                        strBookDate = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[3]/div[6]")).Text;
                        if (strBookDate.Trim() == "")
                        {
                            strBookDate = "-";
                        }
                        strCertificate = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[3]/div[8]")).Text;
                        strAcceptanceDate = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[3]/div[10]")).Text;
                        if (strAcceptanceDate.Trim() == "")
                        {
                            strAcceptanceDate = "-";
                        }
                        strOriginalAmount = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[3]/div[12]")).Text;
                        strBondInterest = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[5]/div[2]")).Text;
                        strNoofInstall = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[5]/div[4]")).Text;
                        strFutureBalance = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[5]/div[6]")).Text;

                        string strSACD = "Special Assessment PrelimsDeficiencies" + "~" + strSATa + "~" + strSABond + "~" + strBookDate + "~" + strCertificate + "~" + strAcceptanceDate + "~" + strOriginalAmount + "~" + strBondInterest + "~" + strNoofInstall + "~" + strFutureBalance;
                        gc.insert_date(orderNumber, strTaxParcel, 205, strSACD, 1, DateTime.Now);

                        try
                        {
                            if ((strPDYear.Contains("View All Years") || strPDYear != ""))
                            {
                                IYearWise.SendKeys(Keys.Enter);
                                gc.insert_date(orderNumber, strTaxParcel, 205, strSACD, 1, DateTime.Now);
                            }
                        }
                        catch { }

                    }
                    catch { }

                    IWebElement IRealTaxSAFuture = driver.FindElement(By.XPath("/html/body/div[1]/nav/div[3]/ul/li[12]/a"));
                    js.ExecuteScript("arguments[0].click();", IRealTaxSAFuture);
                    Thread.Sleep(5000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Future Special Assessment History", driver, "IA", "Polk");


                    //Future Payment History
                    IWebElement IRealTaxSAFuturePH = driver.FindElement(By.XPath("/html/body/div[1]/nav/div[3]/ul/li[13]/a"));
                    js.ExecuteScript("arguments[0].click();", IRealTaxSAFuturePH);
                    Thread.Sleep(5000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Future Payment History", driver, "IA", "Polk");

                    try
                    {
                        strFutureSourceType = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[3]/div[2]")).Text;
                        strFutureReceiptNo = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[3]/div[4]")).Text;
                        strFutureAmount = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[3]/div[6]")).Text;
                        strFutureDatePaid = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div[3]/div[8]")).Text;

                        string strFuturePay = "Future Payments History" + "~" + strFutureSourceType + "~" + "-" + "~" + "-" + "~" + strFutureReceiptNo + "~" + strFutureAmount + "~" + strFutureDatePaid;
                        gc.insert_date(orderNumber, strTaxParcel, 194, strFuturePay, 1, DateTime.Now);
                    }
                    catch { }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "IA", "Polk", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);


                    driver.Quit();
                    gc.mergpdf(orderNumber, "IA", "Polk");
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