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
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace ScrapMaricopa
{
    public class WebDriver_TNShelby
    {

        string outparcelno = "";
        string outputPath = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        string PropertyAddress = "", LocationId = "", AccountId = "", OldAccountnumber = "", Year = "", BillNumber = "", Activity = "", Posted = "", Reference = "", BAmount = "", AssessmentTax = "", TaxDue = "", Interest = "", TotalFees = "", TotalDue = "", TotalTaxes = "", PendingPaymentAmount = "", TaxingAuthority = "", yearbuilt = "", ownername = "";
        string Assessment = "", CurrenTaxYear = "", TaxBillNumber = "", CurrentBalance = "", Type = "", PayBy = "", Payments = "", MillageRate = "", TaxAssessed = "", Balance = "", Installment = "", Penalty = "", Intrest = "", OtherCharges = "", Due = "", Acreage = "", PropertyUse = "", date = "", Status = "", PaidBy = "", Amount = "", year = "", Billedtaxes = "", GCurrenTaxYear = "", CurrentTaxesDue = "", Collections = "", GTotalDue = "", CityTaxAuthority = "", TaxAuthority = "", BTaxYear = "";
        string Link1 = "", Pa1 = "", Pa2 = "", Pa3 = "", Pa4 = "", Pa5 = "", Pa6 = "";
        string strMuliCount = "", CGTaxAuthority = "", AssessYear = "", AssessTaxYear = "", TaxingYear = "", TaxYear = "", TransactionDate = "", TransactionType = "", TransactionAmount = "", STaxingAuthority = "", TransactionNumber = "";
        string Notice_Taxingauthority = "", Notice_NoticeType = "", Notice_Classification = "", Notice_Location1 = "", Notice_Location2 = "", Notice_Location = "", Notice_Subdivision = "", Notice_TaxYear = "", Notice_AssedssedValue = "", Notice_TaxRate = "", Notice_Town = "", Notice_BaseTax = "", Notice_Interest = "", Notice_Court = "", Notice_Atty = "", Notice_Misc = "", Notice_BalnceDue = "", Notice_TotalAmountDue = "", Noticetax = "";
        string TaxParcelID = "";
        List<string> strTaxURL = new List<string>();
        List<string> AssessTaxURL = new List<string>();
        public string FTP_TNShelby(string houseno, string sname, string sttype, string account, string parcelNumber, string searchType, string orderno, string ownername, string directParcel)
        {
            GlobalClass.global_orderNo = orderno;
            HttpContext.Current.Session["orderNo"] = orderno;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new ChromeDriver())//PhantomJSDriver
            {
                string[] stringSeparators1 = new string[] { "\r\n" };
                List<string> listurl = new List<string>();
                string Date = "";
                Date = DateTime.Now.ToString("M/d/yyyy");
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    if (searchType == "titleflex")
                    {
                        string address = houseno + " " + sname;
                        gc.TitleFlexSearch(orderno, parcelNumber, "", address, "TN", "Shelby");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["ShelBy_Zero"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("http://www.assessor.shelby.tn.us/content.aspx");
                        driver.FindElement(By.Id("ctl02_txtStreetNumber")).SendKeys(houseno);
                        driver.FindElement(By.Id("ctl02_txtStreetName")).SendKeys(sname);
                        gc.CreatePdf_WOP(orderno, "Address Search", driver, "TN", "Shelby");
                        driver.FindElement(By.Id("ctl02_btnStreetSearch")).SendKeys(Keys.Enter);

                        try
                        {
                            string strMulti = driver.FindElement(By.XPath("//*[@id='content_text']/table[2]/tbody")).Text;
                            strMuliCount = gc.Between(strMulti, "of  ", " Total Records").Trim();
                            if (Convert.ToInt32(strMuliCount) > 27)
                            {
                                HttpContext.Current.Session["TN_Shelby_Count"] = "Maximum";
                                return "Maximum";
                            }

                            if (Convert.ToInt32(strMuliCount) > 1 && Convert.ToInt32(strMuliCount) <= 27)
                            {
                                try
                                {
                                    IWebElement Imulticount = driver.FindElement(By.Id("PageNavigator1_ddlPageSize"));
                                    SelectElement selectMulticount = new SelectElement(Imulticount);
                                    selectMulticount.SelectByText("50");
                                }
                                catch { }
                                gc.CreatePdf_WOP(orderno, "Multi Address Search", driver, "TN", "Shelby");
                                IWebElement Imulti = driver.FindElement(By.XPath("//*[@id='dgList']/tbody"));
                                IList<IWebElement> ImultiRow = Imulti.FindElements(By.TagName("tr"));
                                IList<IWebElement> ImultiTD;
                                foreach (IWebElement multi in ImultiRow)
                                {
                                    ImultiTD = multi.FindElements(By.TagName("td"));
                                    if (ImultiTD.Count != 0 && !multi.Text.Contains("Parcel ID"))
                                    {
                                        gc.insert_date(orderno, ImultiTD[0].Text, 167, ImultiTD[1].Text + "~" + ImultiTD[2].Text, 1, DateTime.Now);
                                    }
                                }

                                HttpContext.Current.Session["TN_Shelby"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                        }
                        catch { }

                        try
                        {
                            string Nodata = driver.FindElement(By.XPath("//*[@id='content_text']/div/div/p[1]")).Text;
                            if (Nodata.Contains("Your search did not yield any results."))
                            {
                                HttpContext.Current.Session["ShelBy_Zero"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }

                    }

                    else if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("http://www.assessor.shelby.tn.us/content.aspx");
                        driver.FindElement(By.Id("ctl02_txtParcelID")).SendKeys(parcelNumber);
                        gc.CreatePdf_WOP(orderno, "Parcel Search", driver, "TN", "Shelby");
                        driver.FindElement(By.Id("ctl02_btnParcelSearch")).SendKeys(Keys.Enter);

                        try
                        {
                            string Nodata = driver.FindElement(By.XPath("//*[@id='content_text']/div/div/p[1]")).Text;
                            if (Nodata.Contains("Your search did not yield any results."))
                            {
                                HttpContext.Current.Session["ShelBy_Zero"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }

                    }

                    else if (searchType == "Business")
                    {
                        driver.Navigate().GoToUrl("http://www.assessor.shelby.tn.us/content.aspx");
                        driver.FindElement(By.Id("ctl02_txtBusiness")).SendKeys(account);
                        gc.CreatePdf_WOP(orderno, "Business Search", driver, "TN", "Shelby");
                        driver.FindElement(By.Id("ctl02_btnBusinessSearch")).SendKeys(Keys.Enter);

                        try
                        {
                            string strMulti = driver.FindElement(By.XPath("//*[@id='content_text']/table[2]/tbody")).Text;
                            strMuliCount = gc.Between(strMulti, "of  ", " Total Records").Trim();
                            if (Convert.ToInt32(strMuliCount) > 27)
                            {
                                HttpContext.Current.Session["TN_Shelby_Count"] = "Maximum";
                                return "Maximum";
                            }

                            if (Convert.ToInt32(strMuliCount) > 1 && Convert.ToInt32(strMuliCount) <= 27)
                            {
                                try
                                {
                                    IWebElement Imulticount = driver.FindElement(By.Id("PageNavigator1_ddlPageSize"));
                                    SelectElement selectMulticount = new SelectElement(Imulticount);
                                    selectMulticount.SelectByText("50");
                                }
                                catch { }
                                gc.CreatePdf_WOP(orderno, "Multi Business Search", driver, "TN", "Shelby");
                                IWebElement Imulti = driver.FindElement(By.XPath("//*[@id='dgList']/tbody"));
                                IList<IWebElement> ImultiRow = Imulti.FindElements(By.TagName("tr"));
                                IList<IWebElement> ImultiTD;
                                foreach (IWebElement multi in ImultiRow)
                                {
                                    ImultiTD = multi.FindElements(By.TagName("td"));
                                    if (ImultiTD.Count != 0 && !multi.Text.Contains("Parcel ID"))
                                    {
                                        gc.insert_date(orderno, ImultiTD[0].Text, 167, ImultiTD[1].Text + "~" + ImultiTD[2].Text, 1, DateTime.Now);
                                    }
                                }

                                HttpContext.Current.Session["TN_Shelby"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                        }
                        catch { }

                        try
                        {
                            string Nodata = driver.FindElement(By.XPath("//*[@id='content_text']/div/div/p[1]")).Text;
                            if (Nodata.Contains("Your search did not yield any results."))
                            {
                                HttpContext.Current.Session["ShelBy_Zero"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }

                    }

                    else if (searchType == "ownername")
                    {
                        driver.Navigate().GoToUrl("http://www.assessor.shelby.tn.us/content.aspx");
                        string[] owner = ownername.Trim().Split(' ');
                        driver.FindElement(By.Id("ctl02_txtFirstName")).SendKeys(owner[0]);
                        try
                        {
                            driver.FindElement(By.Id("ctl02_txtLastName")).SendKeys(owner[1]);
                        }
                        catch { }
                        gc.CreatePdf_WOP(orderno, "OwnerName Search", driver, "TN", "Shelby");
                        driver.FindElement(By.Id("ctl02_btnNameSearch")).SendKeys(Keys.Enter);

                        try
                        {
                            string strMulti = driver.FindElement(By.XPath("//*[@id='content_text']/table[2]/tbody")).Text;
                            strMuliCount = gc.Between(strMulti, "of  ", " Total Records").Trim();
                            if (Convert.ToInt32(strMuliCount) > 27)
                            {
                                HttpContext.Current.Session["TN_Shelby_Count"] = "Maximum";
                                return "Maximum";
                            }

                            if (Convert.ToInt32(strMuliCount) > 1 && Convert.ToInt32(strMuliCount) <= 27)
                            {
                                try
                                {
                                    IWebElement Imulticount = driver.FindElement(By.Id("PageNavigator1_ddlPageSize"));
                                    SelectElement selectMulticount = new SelectElement(Imulticount);
                                    selectMulticount.SelectByText("50");
                                }
                                catch { }
                                gc.CreatePdf_WOP(orderno, "Multi Owner Search", driver, "TN", "Shelby");
                                IWebElement Imulti = driver.FindElement(By.XPath("//*[@id='dgList']/tbody"));
                                IList<IWebElement> ImultiRow = Imulti.FindElements(By.TagName("tr"));
                                IList<IWebElement> ImultiTD;
                                foreach (IWebElement multi in ImultiRow)
                                {
                                    ImultiTD = multi.FindElements(By.TagName("td"));
                                    if (ImultiTD.Count != 0 && !multi.Text.Contains("Parcel ID"))
                                    {
                                        gc.insert_date(orderno, ImultiTD[0].Text, 167, ImultiTD[1].Text + "~" + ImultiTD[2].Text, 1, DateTime.Now);
                                    }
                                }

                                HttpContext.Current.Session["TN_Shelby"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                        }
                        catch { }

                        try
                        {
                            string Nodata = driver.FindElement(By.XPath("//*[@id='content_text']/div/div/p[1]")).Text;
                            if (Nodata.Contains("Your search did not yield any results."))
                            {
                                HttpContext.Current.Session["ShelBy_Zero"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }

                    }

                    IWebElement IPropertySearch = driver.FindElement(By.XPath("//*[@id='dgList']/tbody"));
                    IList<IWebElement> IPropertySearchRow = IPropertySearch.FindElements(By.TagName("tr"));
                    IList<IWebElement> IpropertySearchTD;
                    foreach (IWebElement search in IPropertySearchRow)
                    {
                        IpropertySearchTD = search.FindElements(By.TagName("td"));
                        if (IpropertySearchTD.Count != 0 && !search.Text.Contains("Parcel ID"))
                        {
                            IWebElement IProperty = IpropertySearchTD[3].FindElement(By.TagName("a"));
                            if (IProperty.Text != "" && IProperty.Text.Contains("View") && IPropertySearchRow.Count == 2)
                            {
                                string strProeprtyClick = IProperty.GetAttribute("href");
                                driver.Navigate().GoToUrl(strProeprtyClick);
                                break;
                            }
                        }
                    }

                    string parcelNumber2 = parcelNumber.Replace(" ", "");
                    gc.CreatePdf(orderno, parcelNumber, "Property  Result", driver, "TN", "Shelby");
                    string PropertyAddress = "", yearbuilt = "", MunicipalJurisdiction = "", NeighborhoodNumber = "", Acres = "", LotDimensions = "", SubdivisionName = "", SubdivisionLotNumber = "";
                    MunicipalJurisdiction = driver.FindElement(By.XPath("//*[@id='spnMunicipality']")).Text;
                    parcelNumber = driver.FindElement(By.XPath("//*[@id='spnParcelID']")).Text;
                    PropertyAddress = driver.FindElement(By.XPath("//*[@id='spnAddress']")).Text;
                    ownername = driver.FindElement(By.XPath("//*[@id='spnOwnerName']")).Text;
                    string CHKMunicipalJurisdiction = MunicipalJurisdiction.Substring(0, 1);
                    NeighborhoodNumber = driver.FindElement(By.XPath("//*[@id='spnNeighborhood']")).Text;
                    Acres = driver.FindElement(By.XPath("//*[@id='spnLandAcres']")).Text;
                    LotDimensions = driver.FindElement(By.XPath("//*[@id='spnLandDimensions']")).Text;
                    SubdivisionName = driver.FindElement(By.XPath("//*[@id='spnSubdivisionName']")).Text;
                    SubdivisionLotNumber = driver.FindElement(By.XPath("//*[@id='spnSubdivisionLot']")).Text;
                    try
                    {
                        yearbuilt = driver.FindElement(By.XPath("//*[@id='spnCYearBuilt']")).Text;
                    }
                    catch { }
                    try
                    {
                        yearbuilt = driver.FindElement(By.Id("spnYearBuilt")).Text;
                    }
                    catch { }              //PropertyAddress~MunicipalJurisdiction~NeighborhoodNumber~Acres~LotDimensions~SubdivisionName~SubdivisionLotNumber~ownername~Yearbuilt
                    string PropertyDeatil = PropertyAddress + "~" + MunicipalJurisdiction + "~" + NeighborhoodNumber + "~" + Acres + "~" + LotDimensions.Replace("~", "") + "~" + SubdivisionName + "~" + SubdivisionLotNumber + "~" + ownername + "~" + yearbuilt;
                    gc.insert_date(orderno, parcelNumber, 168, PropertyDeatil, 1, DateTime.Now);

                    string Class = "", LandAppraisal = "", BuildingAppraisal = "", TotalAppraisal = "", TotalAssessment = "", GreenbeltLandAppraisal = "", HomesiteLandAppraisal = "", HomesiteBuildingAppraisal = "", GreenbeltAppraisal = "", GreenbeltAssessment = "";
                    Class = driver.FindElement(By.XPath("//*[@id='spnPropertyClass']")).Text;
                    LandAppraisal = driver.FindElement(By.XPath("//*[@id='spnLandAppraisal']")).Text;
                    BuildingAppraisal = driver.FindElement(By.XPath("//*[@id='spnBuildingAppraisal']")).Text;
                    TotalAppraisal = driver.FindElement(By.XPath("//*[@id='spnTotalAppraisal']")).Text;
                    TotalAssessment = driver.FindElement(By.XPath("//*[@id='spnTotalAssessment']")).Text;
                    GreenbeltLandAppraisal = driver.FindElement(By.XPath("//*[@id='spnGreenbeltLand']")).Text;
                    HomesiteLandAppraisal = driver.FindElement(By.XPath("//*[@id='spnHomesiteLand']")).Text;
                    HomesiteBuildingAppraisal = driver.FindElement(By.XPath("//*[@id='spnHomesiteBuilding']")).Text;
                    GreenbeltAppraisal = driver.FindElement(By.XPath("//*[@id='spnGreenbeltAppraisal']")).Text;
                    GreenbeltAssessment = driver.FindElement(By.XPath("//*[@id='spnGreenBeltAssessment']")).Text;
                    string Assessmentq = Class + "~" + LandAppraisal + "~" + BuildingAppraisal + "~" + TotalAppraisal + "~" + TotalAssessment + "~" + GreenbeltLandAppraisal + "~" + HomesiteLandAppraisal + "~" + HomesiteBuildingAppraisal + "~" + GreenbeltAppraisal + "~" + GreenbeltAssessment;
                    gc.insert_date(orderno, parcelNumber, 170, Assessmentq, 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    Amrock amc = new Amrock();
                    driver.Navigate().GoToUrl("http://www.shelbycountytrustee.com/");
                    Thread.Sleep(8000);
                    IWebElement ISpan1 = driver.FindElement(By.XPath("//*[@id='graphicLinkWidget21661b59-8f7c-4b5e-93bf-47291cb48c37']/div[2]/div/div[2]/div/div/div/a"));
                    ISpan1.Click();
                    // js.ExecuteScript("arguments[0].click();", ISpan1);
                    Thread.Sleep(10000);
                    try
                    {
                        string strTaxAuthority = driver.FindElement(By.Id("divInfoAdv4e088475-a702-426a-bd26-90d935c3d48d")).Text;
                        TaxAuthority = GlobalClass.Before(strTaxAuthority, "\r\nSign up for E-Newsletter").Replace("\r\n", " ");
                    }
                    catch { }
                    //IWebElement iframeElement = driver.FindElement(By.Id("divInfoAdv4e088475-a702-426a-bd26-90d935c3d48d"));
                    //now use the switch command
                    driver.Navigate().GoToUrl("https://apps.shelbycountytrustee.com/TaxQuery/TaxQry.aspx?flag=0");
                    driver.FindElement(By.Id("rdbSearch_2")).Click();
                    //try
                    //{
                    //    TaxAuthority = driver.FindElement(By.XPath("//*[@id='quickLinks214']/div[2]/div/div[1]/table/tbody/tr[2]/td/span")).Text;
                    //}
                    //catch { }
                    //driver.FindElement(By.XPath("//*[@id='rdbSearch_2']")).Click();
                    Thread.Sleep(10000);

                    var Parsplit = parcelNumber.Split(' ');
                    int count = parcelNumber.Replace(" ", "").Count();

                    if (count == 10)
                    {
                        Pa1 = Parsplit[0] + "00";
                        Pa2 = "0" + Parsplit[1] + "0";
                    }
                    if (count == 11)
                    {
                        Pa1 = Parsplit[0] + "00";
                        Pa2 = Parsplit[1] + "0";
                    }
                    else if (count == 12)
                    {
                        Pa1 = Parsplit[0] + "0";
                        Pa2 = Parsplit[1] + "0";
                    }
                    else if (count == 14)
                    {
                        Pa1 = Parsplit[0];
                        Pa2 = Parsplit[1];
                    }
                    else if (count == 13)
                    {
                        Pa1 = Parsplit[0] + "0";
                        Pa2 = Parsplit[1];
                    }

                    Pa3 = "";
                    Pa4 = "";
                    Pa5 = "";
                    Pa6 = Pa1 + Pa2;


                    Pa1 = Pa6.Substring(0, 3);
                    Pa2 = Pa6.Replace(" ", "").Substring(3, 4);
                    Pa3 = Pa6.Substring(7, 1);
                    Pa4 = Pa6.Substring(8, 5);
                    Pa5 = Pa6.Substring(13, 1);
                    driver.FindElement(By.XPath("//*[@id='Ward']")).SendKeys(Pa1.Trim());
                    driver.FindElement(By.XPath("//*[@id='Block']")).SendKeys(Pa2.Trim());
                    driver.FindElement(By.XPath("//*[@id='SubNumber']")).SendKeys(Pa3.Trim());
                    driver.FindElement(By.XPath("//*[@id='Parcel']")).SendKeys(Pa4.Trim());
                    driver.FindElement(By.XPath("//*[@id='Tag']")).SendKeys(Pa5.Trim());
                    Thread.Sleep(1000);
                    driver.FindElement(By.XPath("//*[@id='Search']")).SendKeys(Keys.Enter);
                    Thread.Sleep(4000);
                    gc.CreatePdf(orderno, parcelNumber, "Property Location Search Result", driver, "TN", "Shelby");
                    //driver.SwitchTo().DefaultContent();
                    //IWebElement iframeElement1 = driver.FindElement(By.XPath("//*[@id='Section1']/iframe"));
                    //now use the switch command
                    //driver.SwitchTo().Frame(iframeElement1);
                    //Thread.Sleep(1000);
                    try
                    {
                        tax_details(orderno, parcelNumber);
                        TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    }
                    catch
                    {
                        driver.Navigate().GoToUrl("http://www.shelbycountytrustee.com/");
                        Thread.Sleep(3000);
                        IWebElement ISpan2 = driver.FindElement(By.XPath("//*[@id='graphicLinkWidget21661b59-8f7c-4b5e-93bf-47291cb48c37']/div[2]/div/div[2]/div/div/div/a"));
                        IJavaScriptExecutor js2 = driver as IJavaScriptExecutor;
                        js2.ExecuteScript("arguments[0].click();", ISpan2);
                        Thread.Sleep(10000);

                        IWebElement iframeElementtest = driver.FindElement(By.XPath("//*[@id='Section1']/iframe"));
                        //now use the switch command
                        driver.SwitchTo().Frame(iframeElementtest);

                        driver.FindElement(By.XPath("//*[@id='rdbSearch_2']")).Click();
                        Thread.Sleep(10000);

                        var Parsplit1 = parcelNumber.Split(' ');
                        count = parcelNumber.Replace(" ", "").Count();

                        if (count == 10)
                        {
                            Pa1 = Parsplit[0] + "00";
                            Pa2 = "0" + Parsplit[1] + "0";
                        }
                        if (count == 11)
                        {
                            Pa1 = Parsplit[0] + "00";
                            Pa2 = Parsplit[1] + "0";
                        }
                        else if (count == 12)
                        {
                            Pa1 = Parsplit[0] + "0";
                            Pa2 = "0" + Parsplit[1];
                        }
                        else if (count == 14)
                        {
                            Pa1 = Parsplit[0];
                            Pa2 = Parsplit[1];
                        }
                        else if (count == 13)
                        {
                            Pa1 = Parsplit[0] + "0";
                            Pa2 = Parsplit[1];
                        }

                        Pa3 = "";
                        Pa4 = "";
                        Pa5 = "";
                        Pa6 = Pa1 + Pa2;

                        Pa1 = Pa6.Substring(0, 3);
                        Pa2 = Pa6.Replace(" ", "").Substring(3, 4);
                        Pa3 = Pa6.Substring(7, 1);
                        Pa4 = Pa6.Substring(8, 5);
                        Pa5 = Pa6.Substring(13, 1);
                        driver.FindElement(By.XPath("//*[@id='Ward']")).SendKeys(Pa1.Trim());
                        driver.FindElement(By.XPath("//*[@id='Block']")).SendKeys(Pa2.Trim());
                        driver.FindElement(By.XPath("//*[@id='SubNumber']")).SendKeys(Pa3.Trim());
                        driver.FindElement(By.XPath("//*[@id='Parcel']")).SendKeys(Pa4.Trim());
                        driver.FindElement(By.XPath("//*[@id='Tag']")).SendKeys(Pa5.Trim());
                        Thread.Sleep(1000);
                        driver.FindElement(By.XPath("//*[@id='Search']")).SendKeys(Keys.Enter);
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderno, parcelNumber, "Property Location Search Result", driver, "TN", "Shelby");
                        driver.SwitchTo().DefaultContent();
                        IWebElement iframeElementtest1 = driver.FindElement(By.XPath("//*[@id='Section1']/iframe"));
                        //now use the switch command
                        driver.SwitchTo().Frame(iframeElementtest1);
                        Thread.Sleep(1000);
                        try
                        {
                            tax_details(orderno, parcelNumber);
                        }
                        catch { }
                        TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    }

                    if (TaxParcelID != "")
                    {
                        amc.TaxId = TaxParcelID;
                    }

                    //City of Memphis
                    if (CHKMunicipalJurisdiction == "M")
                   {
                        string MemParcelId = "", MemOwnerName = "", MemPAddress = "", MemBalance = "", MemRecieptDate = "", MemParcelNo = "", MemTaxOwner = "", MemTaxAddress = "";
                        try
                        {
                            driver.Navigate().GoToUrl("https://epayments.memphistn.gov/property/");
                            try
                            {
                                driver.FindElement(By.XPath("//*[@id='ctl00_MainBodyPlaceHolder_radParcelNo']")).Click();
                                Thread.Sleep(6000);
                                driver.FindElement(By.XPath("//*[@id='ctl00_MainBodyPlaceHolder_txtParcelNo']")).SendKeys(parcelNumber);
                                Thread.Sleep(1000);
                                gc.CreatePdf(orderno, parcelNumber, "M_Parcel Number Search", driver, "TN", "Shelby");
                                driver.FindElement(By.XPath("//*[@id='ctl00_MainBodyPlaceHolder_btnSearch']")).SendKeys(Keys.Enter);
                            }
                            catch { }
                            try
                            {
                                driver.FindElement(By.XPath("//*[@id='MainBodyPlaceHolder_radParcelNo']")).Click();
                                Thread.Sleep(6000);
                                driver.FindElement(By.XPath("//*[@id='MainBodyPlaceHolder_txtParcelNo']")).SendKeys(parcelNumber);
                                Thread.Sleep(1000);
                                gc.CreatePdf(orderno, parcelNumber, "M_Parcel Number Search", driver, "TN", "Shelby");
                                driver.FindElement(By.XPath("//*[@id='MainBodyPlaceHolder_btnSearch']")).SendKeys(Keys.Enter);

                            }
                            catch { }
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderno, parcelNumber, "M_Parcel Number Search Result", driver, "TN", "Shelby");
                            amc.TaxType = "Memphis";

                            string MemCurrentUrl = driver.CurrentWindowHandle;
                            string PreviousYear = "";
                            int memCount = 0;
                            try
                            {
                                MemParcelId = driver.FindElement(By.Id("ctl00_MainBodyPlaceHolder_lblParcelNo")).Text;
                                MemOwnerName = driver.FindElement(By.Id("ctl00_MainBodyPlaceHolder_lblOwnerName")).Text;
                                MemPAddress = driver.FindElement(By.Id("ctl00_MainBodyPlaceHolder_lblOwnerAddress")).Text;
                                MemBalance = driver.FindElement(By.Id("ctl00_MainBodyPlaceHolder_lblCurrBalance")).Text;
                            }
                            catch { }
                            try
                            {
                                MemParcelId = driver.FindElement(By.Id("MainBodyPlaceHolder_lblParcelNo")).Text;
                                MemOwnerName = driver.FindElement(By.Id("MainBodyPlaceHolder_lblOwnerName")).Text;
                                MemPAddress = driver.FindElement(By.Id("MainBodyPlaceHolder_lblOwnerAddress")).Text;
                                MemBalance = driver.FindElement(By.Id("MainBodyPlaceHolder_lblCurrBalance")).Text;
                            }
                            catch { }
                            IWebElement IMCityAssess = null;
                            try
                            {
                                IMCityAssess = driver.FindElement(By.XPath("//*[@id='ctl00_MainBodyPlaceHolder_gridDetail']/tbody"));
                            }
                            catch { }
                            try
                            {
                                if (IMCityAssess == null)
                                {
                                    IMCityAssess = driver.FindElement(By.XPath("//*[@id='MainBodyPlaceHolder_gridDetail']/tbody"));
                                }
                            }
                            catch { }
                            IList<IWebElement> IMCityAssessRow = IMCityAssess.FindElements(By.TagName("tr"));
                            IList<IWebElement> IMCityAssessTD;
                            int memphiscount = 0;
                            foreach (IWebElement MCity in IMCityAssessRow)
                            {
                                IMCityAssessTD = MCity.FindElements(By.TagName("td"));
                                if (IMCityAssessTD.Count != 0)
                                {
                                    string MemTaxYear = IMCityAssessTD[0].Text;
                                    string MemTaxType = IMCityAssessTD[1].Text;
                                    string MemTaxAssessment = IMCityAssessTD[2].Text;
                                    string MemTaxMillege = IMCityAssessTD[3].Text;
                                    string MemTaxBillNo = IMCityAssessTD[4].Text;
                                    string MemTaxAssessed = IMCityAssessTD[5].Text;
                                    string MemInterest = IMCityAssessTD[6].Text;
                                    string MemOtherCharges = IMCityAssessTD[7].Text;
                                    string MemTotalDue = IMCityAssessTD[8].Text;
                                    //string CityTax =AccountId + "~" + LocationId + "~" + OldAccountnumber + "~" + ownername + "~" + PropertyAddress + "~" + TaxBillNumber + "~" + Assessment +"~"+ Year + "~" + Type + "~" + PayBy + "~" + Payments + "~" + MillageRate + "~" + TaxAssessed + "~" + Balance + "~" + Installment + "~" + Penalty + "~" + Intrest + "~" + OtherCharges + "~" + TotalDue + "~" + Due + "~" + Acreage + "~" + PropertyUse + "~" + "" + "~" + Status + "~" + PaidBy + "~" + Amount + "~" + year + "~" + Billedtaxes + "~" + CurrenTaxYear + "~" + CurrentTaxesDue + "~" + Collections + "~" + GTotalDue + "~" + CityTaxAuthority + "~" + TaxAuthority;
                                    string MemTaxAssDetails = "" + "~" + "" + "~" + "" + "~" + MemOwnerName + "~" + MemPAddress + "~" + MemTaxBillNo + "~" + MemTaxAssessment + "~" + MemTaxYear + "~" + MemTaxType + "~" + "" + "~" + "" + "~" + MemTaxMillege + "~" + MemTaxAssessed + "~" + MemBalance + "~" + "" + "~" + "" + "~" + MemInterest + "~" + MemOtherCharges + "~" + MemTotalDue + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "Payments to: 125 N. Main St. Room 375 Memphis, TN 38103. (901) 636-7200 or 1-866-340-7379." + "~" + TaxAuthority;
                                    gc.insert_date(orderno, MemParcelId, 176, MemTaxAssDetails, 1, DateTime.Now);

                                    if (memphiscount == 0 && IMCityAssessTD.Count != 0)
                                    {
                                        amc.TaxYear = IMCityAssessTD[0].Text;
                                        amc.Instamount1 = IMCityAssessTD[3].Text;
                                        amc.Instamountpaid1 = IMCityAssessTD[7].Text;
                                        if (!IMCityAssessTD[7].Text.Contains("$0.00") && IMCityAssessTD[7].Text != "")
                                        {
                                            amc.InstPaidDue1 = "Due";
                                        }
                                        if (IMCityAssessTD[7].Text.Contains("$0.00") && IMCityAssessTD[7].Text != "")
                                        {
                                            amc.InstPaidDue1 = "Paid";
                                        }
                                        memphiscount++;
                                    }
                                }

                                if (IMCityAssessTD.Count != 0 && memCount < 3)
                                {
                                    try
                                    {
                                        string MemProTaxYear = "", MemProTaxType = "", MemProTaxBillNo = "", MemProTaxAssess = "", MemProInterest = "", MemProOtherCharges = "", MemProPaymentDate = "", MemProAmount = "", MemProBalanceDue = "", MemProTotalOutStanding = "";
                                        IWebElement ICityTaxyear = IMCityAssessTD[0].FindElement(By.TagName("a"));
                                        ICityTaxyear.SendKeys(Keys.Enter);
                                        driver.SwitchTo().Window(driver.WindowHandles.Last());
                                        gc.CreatePdf(orderno, parcelNumber, "M_ParcelNumber Due Details", driver, "TN", "Shelby");
                                        MemParcelNo = driver.FindElement(By.Id("lblParcelNo")).Text;
                                        MemTaxOwner = driver.FindElement(By.Id("lblOwnerName")).Text;
                                        MemTaxAddress = driver.FindElement(By.Id("lblOwnerAddress")).Text;
                                        MemProBalanceDue = GlobalClass.After(driver.FindElement(By.Id("lblBalanceDueFor")).Text, ": ");
                                        MemProTotalOutStanding = GlobalClass.After(driver.FindElement(By.Id("lblTotalBalance")).Text, ": ");
                                        IWebElement IMCityTax = driver.FindElement(By.XPath("//*[@id='gridDetail']/tbody"));
                                        IList<IWebElement> IMCityTaxRow = IMCityTax.FindElements(By.TagName("tr"));
                                        IList<IWebElement> IMCityTaxTD;
                                        foreach (IWebElement citytax in IMCityTaxRow)
                                        {
                                            IMCityTaxTD = citytax.FindElements(By.TagName("td"));
                                            if (IMCityTaxTD.Count != 0)
                                            {
                                                MemProTaxYear = IMCityTaxTD[0].Text;
                                                MemProTaxType = IMCityTaxTD[1].Text;
                                                MemProTaxBillNo = IMCityTaxTD[2].Text;
                                                MemProTaxAssess = IMCityTaxTD[3].Text;
                                                MemProInterest = IMCityTaxTD[4].Text;
                                                MemProOtherCharges = IMCityTaxTD[5].Text;
                                                MemProPaymentDate = IMCityTaxTD[6].Text;
                                                MemProAmount = IMCityTaxTD[7].Text;
                                                //  string Payment = ownername + "~" + PropertyAddress + "~" + CurrentBalance + "~" + BTaxYear + "~" + Type + "~" + TaxBillNumber + "~" + TaxAssessed + "~" + Penalty + "~" + OtherCharges + "~" + PaymentDate + "~" + MAmount + "~" + BalanceDue + "~" + TotalOutstandingBalance;
                                                string MemTaxDetails = MemTaxOwner + "~" + MemTaxAddress + "~" + MemProTaxYear + "~" + MemProTaxType + "~" + MemProTaxBillNo + "~" + MemProTaxAssess + "~" + MemProInterest + "~" + MemProOtherCharges + "~" + MemProPaymentDate + "~" + MemProAmount + "~" + MemProBalanceDue + "~" + MemProTotalOutStanding;
                                                gc.insert_date(orderno, MemParcelNo, 178, MemTaxDetails, 1, DateTime.Now);
                                                gc.CreatePdf(orderno, parcelNumber, "Memphis City Tax Year Details" + MemProTaxBillNo + MemProTaxYear, driver, "TN", "Shelby");
                                            }
                                            if (memphiscount == 0 && IMCityTaxTD.Count != 0)
                                            {
                                                amc.Remainingbalance1 = IMCityTaxTD[7].Text;
                                                memphiscount++;
                                            }
                                        }
                                        if (MemProTaxYear != PreviousYear)
                                        {
                                            PreviousYear = MemProTaxYear;
                                            memCount++;
                                        }
                                    }
                                    catch { }
                                    driver.SwitchTo().Window(MemCurrentUrl);
                                }
                            }
                        }
                        catch { }
                        CitytaxTime = DateTime.Now.ToString("HH:mm:ss");
                    }

                    //City of Bartlett
                    else if (CHKMunicipalJurisdiction == "B")
                    {
                        string strCompareYear = "", strcurrentYear = "";
                        int currentYear, yearcount = 0;
                        strcurrentYear = DateTime.Now.Year.ToString();
                        currentYear = Convert.ToInt32(strcurrentYear);
                        for (int j = 0; j < 4; j++)
                        {
                            try
                            {
                                driver.Navigate().GoToUrl("https://cityofbartlett.munisselfservice.com/citizens/RealEstate/Default.aspx?mode=new");
                                Thread.Sleep(3000);
                                amc.TaxType = "Bartlett";
                                driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_ParcelIdSearchFieldLayout_ctl01_ParcelIDTextBox']")).SendKeys(parcelNumber.Replace(" ", ""));
                                Thread.Sleep(1000);
                                gc.CreatePdf(orderno, parcelNumber, "Barlet ParcelMuber Search", driver, "TN", "Shelby");
                                driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_Control_FormLayoutItem7_ctl01_Button1']")).SendKeys(Keys.Enter);
                                Thread.Sleep(2000);
                                gc.CreatePdf(orderno, parcelNumber, "Barlet ParcelNumber Search Result", driver, "TN", "Shelby");
                                IWebElement IBarTaxYear = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_BillsGridView']/tbody"));
                                IList<IWebElement> IBarTaxRow = IBarTaxYear.FindElements(By.TagName("tr"));
                                IList<IWebElement> IBarTaxTD;
                                foreach (IWebElement BarCity in IBarTaxRow)
                                {
                                    IBarTaxTD = BarCity.FindElements(By.TagName("td"));
                                    if (IBarTaxTD.Count != 0 && BarCity.Text.Contains(strcurrentYear) && yearcount < 3)
                                    {
                                        IWebElement ICityYear = BarCity.FindElement(By.TagName("a"));
                                        if (ICityYear.Text != "" && ICityYear.Text.Contains("View Bill"))
                                        {
                                            string strYear = ICityYear.GetAttribute("id");
                                            IWebElement IBarCityClick = driver.FindElement(By.Id(strYear));
                                            IBarCityClick.Click();
                                            Thread.Sleep(3000);
                                            if (yearcount < 1)
                                            {
                                                try
                                                {
                                                    driver.FindElement(By.LinkText("Charges")).SendKeys(Keys.Enter);
                                                    Thread.Sleep(2000);
                                                    gc.CreatePdf(orderno, parcelNumber, "Barlet Charges Details Result", driver, "TN", "Shelby");
                                                    driver.FindElement(By.LinkText("Property Detail")).SendKeys(Keys.Enter);
                                                    Thread.Sleep(2000);
                                                    gc.CreatePdf(orderno, parcelNumber, "Barlet Property Details Result", driver, "TN", "Shelby");
                                                    driver.FindElement(By.LinkText("Owner Information")).SendKeys(Keys.Enter);
                                                    Thread.Sleep(2000);
                                                    gc.CreatePdf(orderno, parcelNumber, "Barlet Owner Information Result", driver, "TN", "Shelby");
                                                    driver.FindElement(By.LinkText("Assessment")).SendKeys(Keys.Enter);
                                                    Thread.Sleep(2000);
                                                    gc.CreatePdf(orderno, parcelNumber, "Barlet Assessment Details Result", driver, "TN", "Shelby");
                                                    driver.FindElement(By.LinkText("Assessment History")).SendKeys(Keys.Enter);
                                                    Thread.Sleep(2000);
                                                    gc.CreatePdf(orderno, parcelNumber, "Barlet Assessment History Details Result", driver, "TN", "Shelby");
                                                    driver.FindElement(By.LinkText("Tax Rates")).SendKeys(Keys.Enter);
                                                    Thread.Sleep(2000);
                                                    gc.CreatePdf(orderno, parcelNumber, "Barlet Tax Rates Details Result", driver, "TN", "Shelby");
                                                }
                                                catch { }
                                            }
                                            driver.FindElement(By.LinkText("View Bill")).SendKeys(Keys.Enter);
                                            Thread.Sleep(2000);
                                            gc.CreatePdf(orderno, parcelNumber, "Barlett ParcelNumber Tax Year Details" + DateTime.Now.ToString().Replace("-", "").Replace(" ", "").Replace("/", "").Replace(":", "").Trim(), driver, "TN", "Shelby");
                                            string CityBarTaxYear = "", CityBarBillYear = "", CityBarBillNo = "", CityBarOwner = "", CityBarParcelID = "", CityBarInstall = "", CityBarPay = "", CityBarAmount = "", CityBarPayment = "", CityBarBalance = "", CityBarInterest = "", CityBarDue = "";
                                            IWebElement ICBarTaxYear = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_AsOfDateTextBox"));
                                            CityBarTaxYear = ICBarTaxYear.GetAttribute("value");
                                            CityBarBillYear = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_FiscalYearLabel")).Text;
                                            amc.TaxYear = CityBarBillYear;
                                            if (strcurrentYear != strCompareYear)
                                            {
                                                strCompareYear = strcurrentYear;
                                                yearcount++;
                                            }
                                            CityBarBillNo = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_BillNumberLabel")).Text;
                                            CityBarOwner = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_OwnerLabel")).Text;
                                            CityBarParcelID = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_CategoryLabel")).Text;
                                            amc.TaxId = CityBarParcelID;
                                            IWebElement IBarCityTax = driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_BillDetailsUpdatePanel']/table/tbody"));
                                            IList<IWebElement> IBarCityTaxRow = IBarCityTax.FindElements(By.TagName("tr"));
                                            IList<IWebElement> IBarCityTaxTD;
                                            foreach (IWebElement view in IBarCityTaxRow)
                                            {
                                                IBarCityTaxTD = view.FindElements(By.TagName("td"));
                                                if (IBarCityTaxTD.Count != 0 && !view.Text.Contains("TOTAL"))
                                                {
                                                    CityBarInstall = IBarCityTaxTD[0].Text;
                                                    CityBarPay = IBarCityTaxTD[1].Text;
                                                    CityBarAmount = IBarCityTaxTD[2].Text;
                                                    CityBarPayment = IBarCityTaxTD[3].Text;
                                                    CityBarBalance = IBarCityTaxTD[4].Text;
                                                    CityBarInterest = IBarCityTaxTD[5].Text;
                                                    CityBarDue = IBarCityTaxTD[6].Text;

                                                    // AccountId + "~" + LocationId + "~" + OldAccountnumber + "~" + ownername + "~" + PropertyAddress + "~" + BillNumber + "~" + Assessment + "~" + year + "~" + "" + "~" + PayBy + "~" + Payments + "~" + MillageRate + "~" + TaxAssessed + "~" + Balance + "~" + Installment + "~" + Penalty + "~" + Intrest + "~" + OtherCharges + "~" + "" + "~" + Due + "~" + Acreage + "~" + PropertyUse + "~" + "" + "~" + Status + "~" + PaidBy + "~" + Amount + "~" + year + "~" + Billedtaxes + "~" + GCurrenTaxYear + "~" + CurrentTaxesDue + "~" + Collections + "~" + GTotalDue + "~" + CityTaxAuthority + "~" + TaxAuthority;
                                                    string BarCityTaxBillDetails = "" + "~" + "" + "~" + "" + "~" + CityBarOwner + "~" + "" + "~" + CityBarBillNo + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + CityBarPayment + "~" + "" + "~" + "" + "~" + CityBarBalance + "~" + CityBarInstall + "~" + "" + "~" + CityBarInterest + "~" + "" + "~" + "" + "~" + CityBarDue + "~" + "" + "~" + "" + "~" + "" + "~" + CityBarTaxYear + "~" + CityBarPay + "~" + CityBarAmount + "~" + CityBarBillYear + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "City of Bartlett Tax Dept at (901) 385-6428" + "~" + TaxAuthority;
                                                    gc.insert_date(orderno, parcelNumber, 176, BarCityTaxBillDetails, 1, DateTime.Now);
                                                }
                                                if (IBarCityTaxTD.Count != 0 && view.Text.Contains("TOTAL"))
                                                {
                                                    CityBarInstall = IBarCityTaxTD[0].Text;
                                                    CityBarAmount = IBarCityTaxTD[1].Text;
                                                    CityBarPayment = IBarCityTaxTD[2].Text;
                                                    CityBarBalance = IBarCityTaxTD[3].Text;
                                                    CityBarInterest = IBarCityTaxTD[4].Text;
                                                    CityBarDue = IBarCityTaxTD[5].Text;

                                                    // AccountId + "~" + LocationId + "~" + OldAccountnumber + "~" + ownername + "~" + PropertyAddress + "~" + BillNumber + "~" + Assessment + "~" + year + "~" + "" + "~" + PayBy + "~" + Payments + "~" + MillageRate + "~" + TaxAssessed + "~" + Balance + "~" + Installment + "~" + Penalty + "~" + Intrest + "~" + OtherCharges + "~" + "" + "~" + Due + "~" + Acreage + "~" + PropertyUse + "~" + "" + "~" + Status + "~" + PaidBy + "~" + Amount + "~" + year + "~" + Billedtaxes + "~" + GCurrenTaxYear + "~" + CurrentTaxesDue + "~" + Collections + "~" + GTotalDue + "~" + CityTaxAuthority + "~" + TaxAuthority;
                                                    string BarCityTaxBillDetails = "" + "~" + "" + "~" + "" + "~" + CityBarOwner + "~" + "" + "~" + CityBarBillNo + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + CityBarPayment + "~" + "" + "~" + "" + "~" + CityBarBalance + "~" + CityBarInstall + "~" + "" + "~" + CityBarInterest + "~" + "" + "~" + "" + "~" + CityBarDue + "~" + "" + "~" + "" + "~" + "" + "~" + CityBarTaxYear + "~" + CityBarPay + "~" + CityBarAmount + "~" + CityBarBillYear + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "City of Bartlett Tax Dept at (901) 385-6428" + "~" + TaxAuthority;
                                                    gc.insert_date(orderno, parcelNumber, 176, BarCityTaxBillDetails, 1, DateTime.Now);
                                                }
                                                if (IBarCityTaxTD.Count != 0 && !view.Text.Contains("TOTAL"))
                                                {
                                                    try
                                                    {
                                                        if (IBarCityTaxTD[0].Text.Contains("1"))
                                                        {
                                                            amc.Instamount1 = IBarCityTaxTD[2].Text;
                                                            amc.Instamountpaid1 = IBarCityTaxTD[3].Text;
                                                            amc.Remainingbalance1 = IBarCityTaxTD[6].Text;
                                                            if (!IBarCityTaxTD[6].Text.Contains("$0.00") && IBarCityTaxTD[6].Text != "")
                                                            {
                                                                amc.InstPaidDue1 = "Due";
                                                            }
                                                            if (IBarCityTaxTD[6].Text.Contains("$0.00") && IBarCityTaxTD[6].Text != "")
                                                            {
                                                                amc.InstPaidDue1 = "Paid";
                                                            }
                                                        }
                                                    }
                                                    catch { }
                                                }
                                            }

                                            string BarCityPayBillYear = "", BarCityPayBillNo = "", Activity = "", Posted = "", PaidBy = "", Amount = "";
                                            driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_ViewBill1_ViewPaymentsLinkButton")).SendKeys(Keys.Enter);
                                            Thread.Sleep(13000);
                                            gc.CreatePdf(orderno, parcelNumber, "Barlett_ParcelNumber Tax Bill Details" + DateTime.Now.ToString().Replace("-", "").Replace(" ", "").Replace("/", "").Replace(":", "").Trim(), driver, "TN", "Shelby");
                                            BarCityPayBillYear = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_FiscalYearLabel")).Text;
                                            BarCityPayBillNo = driver.FindElement(By.Id("ctl00_ctl00_PrimaryPlaceHolder_ContentPlaceHolderMain_BillNumberLabel")).Text;
                                            try
                                            {
                                                IWebElement IBarCityPayment = driver.FindElement(By.XPath("//*[@id='molContentMainDiv']/table[2]/tbody"));
                                                IList<IWebElement> IBarCityPaymentRow = IBarCityPayment.FindElements(By.TagName("tr"));
                                                IList<IWebElement> IBarCityPaymentTD;
                                                foreach (IWebElement Payment in IBarCityPaymentRow)
                                                {
                                                    IBarCityPaymentTD = Payment.FindElements(By.TagName("td"));
                                                    if (IBarCityPaymentTD.Count != 0)
                                                    {
                                                        Activity = IBarCityPaymentTD[0].Text;
                                                        Posted = IBarCityPaymentTD[1].Text;
                                                        PaidBy = IBarCityPaymentTD[2].Text;
                                                        Amount = IBarCityPaymentTD[3].Text;

                                                        string PaymentHistoty = BarCityPayBillYear + "~" + BarCityPayBillNo + "~" + Activity + "~" + Posted + "~" + PaidBy + "~" + Amount;
                                                        gc.insert_date(orderno, parcelNumber, 183, PaymentHistoty, 1, DateTime.Now);
                                                    }
                                                }
                                            }
                                            catch { }
                                        }
                                    }
                                }
                            }
                            catch { }
                            currentYear--;
                            strcurrentYear = Convert.ToString(currentYear);
                        }
                        CitytaxTime = DateTime.Now.ToString("HH:mm:ss");
                    }

                    //Town of Collierville
                    else if (CHKMunicipalJurisdiction == "C")
                    {
                        string CollierVilleAurhority = "", CollierYear = "", PaymentDate = "", PaymentStatus = "", PaymentPaid = "", PaymentAmount = "", PaymentTotalDate = "", PaymentTotalStatus = "", PaymentTotalPaid = "", TotalAmount = "", CollierBill = "",
                               CollierProperty = "", CollierOwner = "", CollierMailing = "", CollierControl = "", CollierGroup = "", CollierParcel = "", CollierPI = "", CollierSI = "", CollierCityCode = "",
                               CollierAppraisalYear = "", CollierLand = "", CollierImprovement = "", CollierPersonalProperty = "", CollierTotalProperty = "", CollierAppraisedProperty = "", CollierTaxable = "",
                               CollierAssessedProperty = "", CollierTaxRate = "", CollierTaxLevy = "", CollierInterest = "", CollierExisting = "", CollierState = "", CollierBalanceDue = "";
                        try
                        {
                            string parcelNumber1 = parcelNumber.Replace(" ", "");
                            driver.Navigate().GoToUrl("https://collierville-tn.mygovonline.com/mod.php?mod=propertytax&mode=public_lookup");
                            Thread.Sleep(2000);
                            amc.TaxType = "Collierville";
                            var SerachCategory = driver.FindElement(By.XPath("//*[@id='propertyTax']/table/tbody/tr[1]/td[2]/select"));
                            var selectElement1 = new SelectElement(SerachCategory);
                            selectElement1.SelectByText("Property Address");
                            Thread.Sleep(3000);
                            driver.FindElement(By.XPath("//*[@id='property_address']")).SendKeys(PropertyAddress);
                            Thread.Sleep(1000);
                            gc.CreatePdf(orderno, parcelNumber, "C_Address Search", driver, "TN", "Shelby");
                            driver.FindElement(By.XPath("//*[@id='submit_btn']")).SendKeys(Keys.Enter);
                            Thread.Sleep(1000);
                            gc.CreatePdf(orderno, parcelNumber, "C_Address Search Result", driver, "TN", "Shelby");
                            CollierVilleAurhority = "Town of Collierville 500 Poplar View Parkway Collierville, TN 38017";
                            IWebElement ISearch = driver.FindElement(By.XPath("//*[@id='data']/tbody"));
                            IList<IWebElement> ISearchRow = ISearch.FindElements(By.TagName("tr"));
                            IList<IWebElement> ISearchTD;
                            foreach (IWebElement search in ISearchRow)
                            {
                                ISearchTD = search.FindElements(By.TagName("td"));
                                if (ISearchTD.Count != 0 && ISearchTD.Count == 7 && search.Text.Contains("View Bill"))
                                {
                                    IWebElement ISearchClick = ISearchTD[6].FindElement(By.TagName("a"));
                                    ISearchClick.Click();
                                    Thread.Sleep(3000);
                                    break;
                                }
                            }
                            gc.CreatePdf(orderno, parcelNumber, "C_Address Result", driver, "TN", "Shelby");
                            try
                            {
                                string currentYear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table[2]/tbody/tr[1]/th/div")).Text;
                                amc.TaxYear = gc.Between(currentYear, "Bill (", ")").Trim();
                            }
                            catch { }
                            IWebElement IGeneral = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table[2]/tbody/tr[3]/td[1]/table/tbody"));
                            IList<IWebElement> IGeneralRow = IGeneral.FindElements(By.TagName("tr"));
                            IList<IWebElement> IGeneralTD;
                            foreach (IWebElement general in IGeneralRow)
                            {
                                IGeneralTD = general.FindElements(By.TagName("td"));
                                if (IGeneralTD.Count != 0)
                                {
                                    if (general.Text.Contains("Bill #"))
                                    {
                                        CollierBill = IGeneralTD[1].Text;
                                    }
                                    if (general.Text.Contains("Property:"))
                                    {
                                        CollierProperty = IGeneralTD[1].Text;
                                    }
                                    if (general.Text.Contains("Owner:"))
                                    {
                                        CollierOwner = IGeneralTD[1].Text;
                                    }
                                    if (general.Text.Contains("Mailing Address:"))
                                    {
                                        CollierMailing = IGeneralTD[1].Text;
                                    }
                                    if (general.Text.Contains("Control Map:"))
                                    {
                                        CollierControl = IGeneralTD[1].Text;
                                    }
                                    if (general.Text.Contains("Group:"))
                                    {
                                        CollierGroup = IGeneralTD[1].Text;
                                    }
                                    if (general.Text.Contains("Parcel:"))
                                    {
                                        CollierParcel = IGeneralTD[1].Text;
                                    }
                                    if (general.Text.Contains("P/I:"))
                                    {
                                        CollierPI = IGeneralTD[1].Text;
                                    }
                                    if (general.Text.Contains("S/I:"))
                                    {
                                        CollierSI = IGeneralTD[1].Text;
                                    }
                                    if (general.Text.Contains("City Code:"))
                                    {
                                        CollierCityCode = IGeneralTD[1].Text;
                                    }
                                }
                            }

                            IWebElement IApraisal = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table[2]/tbody/tr[3]/td[2]/table/tbody"));
                            IList<IWebElement> IApraisalRow = IApraisal.FindElements(By.TagName("tr"));
                            IList<IWebElement> IApraisalTD;
                            foreach (IWebElement Apraisal in IApraisalRow)
                            {
                                IApraisalTD = Apraisal.FindElements(By.TagName("td"));
                                if (IApraisalTD.Count != 0)
                                {
                                    if (Apraisal.Text.Contains("Appraisal Year:"))
                                    {
                                        CollierAppraisalYear = IApraisalTD[1].Text;
                                    }
                                    if (Apraisal.Text.Contains("Land Value:"))
                                    {
                                        CollierLand = IApraisalTD[1].Text;
                                    }
                                    if (Apraisal.Text.Contains("Improvement Value:"))
                                    {
                                        CollierImprovement = IApraisalTD[1].Text;
                                    }
                                    if (Apraisal.Text.Contains("Personal Property Value:"))
                                    {
                                        CollierPersonalProperty = IApraisalTD[1].Text;
                                    }
                                    if (Apraisal.Text.Contains("Total Property Value:"))
                                    {
                                        CollierTotalProperty = IApraisalTD[1].Text;
                                    }
                                }
                            }

                            IWebElement ITaxInfo = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table[2]/tbody/tr[3]/td[3]/table/tbody"));
                            IList<IWebElement> ITaxInfoRow = ITaxInfo.FindElements(By.TagName("tr"));
                            IList<IWebElement> ITaxInfoTD;
                            foreach (IWebElement TaxInfo in ITaxInfoRow)
                            {
                                ITaxInfoTD = TaxInfo.FindElements(By.TagName("td"));
                                if (ITaxInfoTD.Count != 0)
                                {
                                    if (TaxInfo.Text.Contains("Appraised Property Value:"))
                                    {
                                        CollierAppraisedProperty = ITaxInfoTD[1].Text;
                                    }
                                    if (TaxInfo.Text.Contains("Taxable Property:"))
                                    {
                                        CollierTaxable = ITaxInfoTD[1].Text;
                                    }
                                    if (TaxInfo.Text.Contains("Assessed Taxable Value:"))
                                    {
                                        CollierAssessedProperty = ITaxInfoTD[1].Text;
                                    }
                                    if (TaxInfo.Text.Contains("Tax Rate:"))
                                    {
                                        CollierTaxRate = ITaxInfoTD[1].Text;
                                        amc.Instamount1 = ITaxInfoTD[1].Text;
                                    }
                                    if (TaxInfo.Text.Contains("Tax Levy:"))
                                    {
                                        CollierTaxLevy = ITaxInfoTD[1].Text;
                                    }
                                    if (TaxInfo.Text.Contains("Interest:"))
                                    {
                                        CollierInterest = ITaxInfoTD[1].Text;
                                    }
                                    if (TaxInfo.Text.Contains("Existing Payments:"))
                                    {
                                        CollierExisting = ITaxInfoTD[1].Text;
                                        amc.Instamountpaid1 = ITaxInfoTD[1].Text;
                                    }
                                    if (TaxInfo.Text.Contains("State Relief"))
                                    {
                                        CollierState = ITaxInfoTD[1].Text;
                                    }
                                    if (TaxInfo.Text.Contains("Balance Due"))
                                    {
                                        CollierBalanceDue = ITaxInfoTD[1].Text;
                                        amc.Remainingbalance1 = ITaxInfoTD[1].Text;
                                        if (!CollierBalanceDue.Contains("$0.00") && CollierBalanceDue != "")
                                        {
                                            amc.InstPaidDue1 = "Due";
                                        }
                                        if (CollierBalanceDue.Contains("$0.00") && CollierBalanceDue != "")
                                        {
                                            amc.InstPaidDue1 = "Paid";
                                        }
                                    }
                                }
                            }

                            string strColliergeneral = CollierBill + "~" + CollierControl + "~" + CollierGroup + "~" + CollierParcel + "~" + CollierPI + "~" + CollierSI + "~" + CollierCityCode + "~" + CollierAppraisalYear + "~" + CollierLand + "~" + CollierImprovement + "~" + CollierPersonalProperty + "~" + CollierTotalProperty + "~" + CollierAppraisedProperty + "~" + CollierTaxable + "~" + CollierAssessedProperty + "~" + CollierTaxRate + "~" + CollierTaxLevy + "~" + CollierInterest + "~" + CollierExisting + "~" + CollierState + "~" + CollierBalanceDue;
                            gc.insert_date(orderno, parcelNumber, 532, strColliergeneral, 1, DateTime.Now);
                            try
                            {
                                CollierYear = gc.Between(driver.FindElement(By.XPath("//*[@id='content']/div[1]/h1")).Text, "(", ")");
                            }
                            catch { }
                            try
                            {
                                IWebElement IPaymentHistory = driver.FindElement(By.XPath("//*[@id='data0']/tbody"));
                                IList<IWebElement> IPaymentHistoryRow = IPaymentHistory.FindElements(By.TagName("tr"));
                                IList<IWebElement> IPaymentHistoryTD;
                                foreach (IWebElement PayHistory in IPaymentHistoryRow)
                                {
                                    IPaymentHistoryTD = PayHistory.FindElements(By.TagName("td"));
                                    if (IPaymentHistoryTD.Count != 0)
                                    {
                                        PaymentDate = IPaymentHistoryTD[1].Text;
                                        PaymentStatus = IPaymentHistoryTD[2].Text;
                                        PaymentPaid = IPaymentHistoryTD[3].Text;
                                        PaymentAmount = IPaymentHistoryTD[4].Text;
                                        try
                                        {
                                            IWebElement IPaymentHistoryBill = IPaymentHistoryTD[5].FindElement(By.TagName("a"));
                                            string strPaymentURL = IPaymentHistoryBill.GetAttribute("href");
                                            gc.downloadfile(strPaymentURL, orderno, parcelNumber, "Payment History Bill", "TN", "Shelby");
                                        }
                                        catch { }
                                    }
                                }
                            }
                            catch { }

                            string strCollierPaymentDeatils = "" + "~" + "" + "~" + "" + "~" + CollierOwner + "~" + CollierProperty + "~" + CollierBill + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + PaymentDate + "~" + PaymentStatus + "~" + PaymentPaid + "~" + PaymentAmount + "~" + CollierYear + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + CollierVilleAurhority + "~" + TaxAuthority;
                            gc.insert_date(orderno, parcelNumber, 176, strCollierPaymentDeatils, 1, DateTime.Now);

                            try
                            {
                                IWebElement IParcelHsitoryPdf = driver.FindElement(By.LinkText("Parcel History"));
                                string strParcelHsitoryPdf = IParcelHsitoryPdf.GetAttribute("href");
                                gc.downloadfile(strParcelHsitoryPdf, orderno, parcelNumber, "Payment_History", "TN", "Shelby");

                                string FilePath = gc.filePath(orderno, parcelNumber) + "Payment_History.pdf";
                                PdfReader reader;
                                reader = new PdfReader(FilePath);
                                String textFromPage = PdfTextExtractor.GetTextFromPage(reader, 1);
                                System.Diagnostics.Debug.WriteLine("" + textFromPage);

                                string pdftext = textFromPage;

                                string tableassess = gc.Between(pdftext, "Payee", "Page 1 / {nb}").Trim();
                                string[] tableArray = tableassess.Split('\n');

                                List<string> rowarray1 = new List<string>();
                                int i = 0, j = 0;
                                int count1 = tableArray.Length;

                                for (i = 0; i < count1; i++)
                                {
                                    string a1 = tableArray[i].Replace(" ", "~");
                                    string[] rowarray = a1.Split('~');
                                    rowarray1.AddRange(rowarray);

                                    if (rowarray1.Count == 17)
                                    {
                                        string newrow = rowarray1[j] + "~" + rowarray1[j + 1] + " " + rowarray1[j + 2] + "~" + rowarray1[j + 3] + " " + rowarray1[j + 4] + " " + rowarray1[j + 5] + " " + rowarray1[j + 6] + " " + rowarray1[j + 7] + " " + rowarray1[j + 8] + "~" + rowarray1[j + 9] + "~" + rowarray1[j + 10] + "~" + rowarray1[j + 11] + "~" + rowarray1[j + 12] + "~" + rowarray1[j + 13] + "~" + rowarray1[j + 14] + "~" + rowarray1[j + 15] + " " + rowarray1[j + 16];
                                        gc.insert_date(orderno, parcelNumber, 1855, newrow, 1, DateTime.Now);
                                    }
                                    if (rowarray1.Count == 15)
                                    {
                                        string newrow1 = rowarray1[j] + "~" + rowarray1[j + 1] + " " + rowarray1[j + 2] + "~" + rowarray1[j + 3] + " " + rowarray1[j + 4] + " " + rowarray1[j + 5] + "~" + rowarray1[j + 6] + "~" + rowarray1[j + 7] + "~" + rowarray1[j + 8] + "~" + rowarray1[j + 9] + "~" + rowarray1[j + 10] + "~" + rowarray1[j + 11] + "~" + rowarray1[j + 12] + " " + rowarray1[j + 13] + " " + rowarray1[j + 14];
                                        gc.insert_date(orderno, parcelNumber, 1855, newrow1, 1, DateTime.Now);
                                    }
                                    if (rowarray1.Count == 14)
                                    {
                                        string newrow2 = rowarray1[j] + "~" + rowarray1[j + 1] + " " + rowarray1[j + 2] + "~" + rowarray1[j + 3] + " " + rowarray1[j + 4] + " " + rowarray1[j + 5] + "~" + rowarray1[j + 6] + "~" + rowarray1[j + 7] + "~" + rowarray1[j + 8] + "~" + rowarray1[j + 9] + "~" + rowarray1[j + 10] + "~" + rowarray1[j + 11] + "~" + rowarray1[j + 12] + " " + rowarray1[j + 13];
                                        gc.insert_date(orderno, parcelNumber, 1855, newrow2, 1, DateTime.Now);
                                    }
                                    if (rowarray1.Count == 1)
                                    {
                                        string newrow3 = rowarray1[j] + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                        gc.insert_date(orderno, parcelNumber, 1855, newrow3, 1, DateTime.Now);
                                    }
                                    if (rowarray1.Count == 13)
                                    {
                                        string newrow4 = "" + "~" + rowarray1[j] + " " + rowarray1[j + 1] + "~" + rowarray1[j + 2] + " " + rowarray1[j + 3] + " " + rowarray1[j + 4] + "~" + rowarray1[j + 5] + "~" + rowarray1[j + 6] + "~" + rowarray1[j + 7] + "~" + rowarray1[j + 8] + "~" + rowarray1[j + 9] + "~" + rowarray1[j + 10] + "~" + rowarray1[j + 11] + " " + rowarray1[j + 12];
                                        gc.insert_date(orderno, parcelNumber, 1855, newrow4, 1, DateTime.Now);
                                    }
                                    if (rowarray1.Count == 11)
                                    {
                                        string newrow5 = "" + "~" + rowarray1[j] + " " + rowarray1[j + 1] + "~" + rowarray1[j + 2] + " " + rowarray1[j + 3] + " " + rowarray1[j + 4] + "~" + "" + "~" + rowarray1[j + 5] + "~" + rowarray1[j + 6] + "~" + rowarray1[j + 7] + "~" + rowarray1[j + 8] + "~" + rowarray1[j + 9] + "~" + rowarray1[j + 10];
                                        gc.insert_date(orderno, parcelNumber, 1855, newrow5, 1, DateTime.Now);
                                    }
                                    rowarray1.Clear();
                                }
                            }
                            catch { }
                            try
                            {
                                IWebElement IParcelRecieptPdf = driver.FindElement(By.LinkText("Receipt"));
                                string strParcelRecieptPdf = IParcelRecieptPdf.GetAttribute("href");
                                gc.downloadfile(strParcelRecieptPdf, orderno, parcelNumber, "Payment Reciept", "TN", "Shelby");
                            }
                            catch { }
                            try
                            {
                                IWebElement IParcelBillPdf = driver.FindElement(By.LinkText("Bill"));
                                string strParcelBillPdf = IParcelBillPdf.GetAttribute("href");
                                gc.downloadfile(strParcelBillPdf, orderno, parcelNumber, "Payment Bill", "TN", "Shelby");
                            }
                            catch { }

                        }
                        catch { }
                        CitytaxTime = DateTime.Now.ToString("HH:mm:ss");
                    }

                    //City of Germantown
                    else if (CHKMunicipalJurisdiction == "G")
                    {
                        string GURL = "";
                        List<string> GermanTaxYear = new List<string>();
                        try
                        {
                            driver.Navigate().GoToUrl("https://www.germantown-tn.gov/live/online-services-and-payments");
                            Thread.Sleep(2000);
                            amc.TaxType = "Germantown";
                            string ClickG = driver.FindElement(By.XPath("//*[@id='widget_241_94_236']/p[5]/a")).GetAttribute("href");
                            driver.Navigate().GoToUrl(ClickG);
                            Thread.Sleep(1000);
                            driver.FindElement(By.XPath("//*[@id='menuWrapper']/a[3]")).Click();
                            var SerachCategory = driver.FindElement(By.XPath("//*[@id='searchMethod']"));
                            var selectElement1 = new SelectElement(SerachCategory);
                            selectElement1.SelectByText("Parcel");
                            Thread.Sleep(3000);
                            string PO = parcelNumber.Replace(" ", "");
                            string Par1 = PO.Substring(0, 3);
                            string Par2 = PO.Substring(3, 3);
                            string Par3 = PO.Substring(6, 1);
                            string Par4 = PO.Substring(7, 5);
                            driver.FindElement(By.XPath("//*[@id='parcel.parcelNumber1']")).SendKeys(Par1.Trim());
                            driver.FindElement(By.XPath("//*[@id='parcel.parcelNumber2']")).SendKeys(Par2.Trim());
                            driver.FindElement(By.XPath("//*[@id='parcel.parcelNumber3']")).SendKeys(Par3.Trim());
                            driver.FindElement(By.XPath("//*[@id='parcel.parcelNumber4']")).SendKeys(Par4.Trim());
                            Thread.Sleep(1000);
                            gc.CreatePdf(orderno, parcelNumber, "G_Parcel Search", driver, "TN", "Shelby");
                            driver.FindElement(By.XPath("//*[@id='parcelSbmtBtn']")).SendKeys(Keys.Enter);
                            Thread.Sleep(1000);
                            gc.CreatePdf(orderno, parcelNumber, "G_Parcel Search Result", driver, "TN", "Shelby");

                            IWebElement ParcelResutTB = driver.FindElement(By.XPath("//*[@id='DataTables_Table_0']/tbody"));
                            IList<IWebElement> ParcelResutTR = ParcelResutTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> ParcelResutTD;
                            IList<IWebElement> ParcelResutTA;
                            foreach (IWebElement row2 in ParcelResutTR)
                            {
                                ParcelResutTD = row2.FindElements(By.TagName("td"));
                                foreach (IWebElement row7 in ParcelResutTD)
                                {
                                    ParcelResutTA = row7.FindElements(By.TagName("a"));
                                    if (ParcelResutTA.Count != 0)
                                    {
                                        if (ParcelResutTD.Count != 0)
                                        {
                                            string Pachk = ParcelResutTD[0].Text.Replace(" ", "").Replace("-", "");
                                            Pachk = Pachk.Replace("-", "");
                                            if (Pachk.Contains(parcelNumber.Replace(" ", "")))
                                            {
                                                GURL = ParcelResutTA[0].GetAttribute("href");
                                            }
                                        }
                                    }
                                }
                            }

                            driver.Navigate().GoToUrl(GURL);
                            gc.CreatePdf(orderno, parcelNumber, "Account Inquiry", driver, "TN", "Shelby");
                            Thread.Sleep(3000);
                            CGTaxAuthority = driver.FindElement(By.Id("footer")).Text;
                            string GermanYear = "", GerManAccountID = "", GermanOldAccountnumber = "", GerManOwnername = "", GerManLocationID = "", GerManAddress = "", GerManParcelID = "", GerManAcreage = "", GerManPropertyAddress = "", GermanBilled = "", GermanCurrentDue = "",
                                  GermanInterest = "", GermanTotalDue = "", AmountBilled = "", AmountPaid = "", AmountUnPaid = "", AmountDue = "", GermanDate = "", GermanPeriod = "", GermanType = "", GermanAmount = "";

                            string strBasic = driver.FindElement(By.XPath("//*[@id='contentPanel']/div/div[1]")).Text;
                            GermanYear = GlobalClass.After(driver.FindElement(By.XPath("//*[@id='contentPanel']/div/div[3]/h3")).Text, "Tax Information for ").Trim();
                            amc.TaxYear = GermanYear;
                            GerManAccountID = gc.Between(strBasic, "Account ID:", "Owner Name:");
                            GerManOwnername = gc.Between(strBasic, "Owner Name:", "Location ID:");
                            GerManLocationID = gc.Between(strBasic, "Location ID:", "Address:");
                            try
                            {
                                GermanOldAccountnumber = driver.FindElement(By.XPath("//*[@id='contentPanel']/div/div[1]/div[3]/div[1]/p")).Text;
                            }
                            catch { }
                            try
                            {
                                GerManAddress = gc.Between(strBasic, "Address:", "Parcel ID:");
                            }
                            catch { }
                            try
                            {
                                if (GermanOldAccountnumber == "")
                                {
                                    GermanOldAccountnumber = gc.Between(strBasic, "Address:", "Old Account");
                                }
                            }
                            catch { }
                            GerManParcelID = GlobalClass.After(strBasic, "Parcel ID:").Replace("\r\n", "");
                            try
                            {
                                string strGeneral = driver.FindElement(By.XPath("//*[@id='contentPanel']/div/div[2]")).Text;
                                GerManAcreage = gc.Between(strGeneral, "Acreage:", "Property Use:");
                                GerManPropertyAddress = gc.Between(strGeneral, "Property Use:", "Township:");
                            }
                            catch { }
                            try
                            {
                                if (GerManAcreage == "")
                                {
                                    GerManAcreage = driver.FindElement(By.XPath("//*[@id='contentPanel']/div/div[2]/div[1]/div/p")).Text;
                                }
                            }
                            catch { }

                            string strTaxInfo = driver.FindElement(By.XPath("//*[@id='contentPanel']/div/div[3]")).Text;
                            GermanBilled = gc.Between(strTaxInfo, "Billed taxes:", "Current Taxes Due:");
                            amc.Instamount1 = GermanBilled;
                            GermanCurrentDue = gc.Between(strTaxInfo, "Current Taxes Due:", "Interest");
                            GermanInterest = gc.Between(strTaxInfo, "Collections:", "Total Due:");
                            GermanTotalDue = GlobalClass.After(strTaxInfo, "Total Due:");
                            amc.Remainingbalance1 = GermanTotalDue;
                            if (!GermanTotalDue.Contains("$0.00") && GermanTotalDue != "")
                            {
                                amc.InstPaidDue1 = "Due";
                            }
                            if (GermanTotalDue.Contains("$0.00") && GermanTotalDue != "")
                            {
                                amc.InstPaidDue1 = "Paid";
                            }
                            IWebElement IPayAmount = driver.FindElement(By.Id("partialPayAmount"));
                            string payamount = IPayAmount.GetAttribute("value");

                            string GermanAssessDetails = GerManAccountID + "~" + GerManLocationID + "~" + GermanOldAccountnumber + "~" + GerManOwnername + "~" + GerManAddress + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + GerManAcreage + "~" + GerManPropertyAddress + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + GermanBilled + "~" + GermanYear + "~" + GermanCurrentDue + "~" + GermanInterest + "~" + GermanTotalDue + "~" + CGTaxAuthority + "~" + TaxAuthority;
                            gc.insert_date(orderno, parcelNumber, 176, GermanAssessDetails, 1, DateTime.Now);

                            string Year = "";
                            int CurrentYear = DateTime.Now.Year;
                            for (int i = 0; i < 4; i++)
                            {
                                if (GermanTaxYear.Count < 3)
                                {
                                    try
                                    {
                                        IWebElement IHistory = driver.FindElement(By.Id("menuWrapper"));
                                        IList<IWebElement> IHistoryRow = IHistory.FindElements(By.TagName("a"));
                                        foreach (IWebElement history in IHistoryRow)
                                        {
                                            if (history.Text.Contains("History"))
                                            {
                                                history.Click();
                                                Thread.Sleep(6000);
                                                break;
                                            }
                                        }
                                        //driver.FindElement(By.LinkText("History"));
                                        //Thread.Sleep(9000);
                                        SerachCategory = driver.FindElement(By.XPath("//*[@id='TaxYear']"));
                                        selectElement1 = new SelectElement(SerachCategory);
                                        selectElement1.SelectByText(Convert.ToString(CurrentYear));
                                        Thread.Sleep(9000);
                                        //Year = selectElement1.SelectedOption.Text;
                                        gc.CreatePdf(orderno, parcelNumber, "Valuation History" + CurrentYear, driver, "TN", "Shelby");
                                        string strTaxYear = driver.FindElement(By.XPath("//*[@id='frmHistoryForm']/div[2]")).Text;
                                        AmountBilled = gc.Between(strTaxYear, "Total Amount Billed:", "Total Amount Paid:");
                                        AmountPaid = gc.Between(strTaxYear, "Total Amount Paid:", "Total Amount Unapplied:");
                                        amc.Instamountpaid1 = AmountPaid;
                                        AmountUnPaid = gc.Between(strTaxYear, "Total Amount Unapplied:", "Total Amount Due:");
                                        AmountDue = GlobalClass.After(strTaxYear, "Total Amount Due:");
                                        IWebElement ITaxYearTable = driver.FindElement(By.XPath("//*[@id='DataTables_Table_0']/tbody"));
                                        IList<IWebElement> ITaxYearRow = ITaxYearTable.FindElements(By.TagName("tr"));
                                        IList<IWebElement> ITaxYearTD;
                                        foreach (IWebElement ITax in ITaxYearRow)
                                        {
                                            ITaxYearTD = ITax.FindElements(By.TagName("td"));
                                            if (ITaxYearTD.Count != 0)
                                            {
                                                GermanDate = ITaxYearTD[0].Text;
                                                GermanPeriod = ITaxYearTD[1].Text;
                                                GermanType = ITaxYearTD[2].Text;
                                                GermanAmount = ITaxYearTD[3].Text;

                                                string Tax = GerManAccountID + "~" + GerManLocationID + "~" + GermanOldAccountnumber + "~" + GerManOwnername + "~" + GerManAddress + "~" + CurrentYear + "~" + AmountBilled + "~" + AmountPaid + "~" + AmountUnPaid + "~" + AmountDue.Replace("\r\n", "") + "~" + GermanDate + "~" + GermanPeriod + "~" + GermanType + "~" + GermanAmount;
                                                gc.insert_date(orderno, GerManParcelID, 187, Tax, 1, DateTime.Now);
                                            }
                                        }
                                    }
                                    catch { }
                                    CurrentYear--;
                                }
                            }
                        }
                        catch { }
                        CitytaxTime = DateTime.Now.ToString("HH:mm:ss");
                    }

                    gc.InsertAmrockTax(orderno, amc.TaxId, amc.Instamount1, amc.Instamount2, amc.Instamount3, amc.Instamount4, amc.Instamountpaid1, amc.Instamountpaid2, amc.Instamountpaid3, amc.Instamountpaid4, amc.InstPaidDue1, amc.InstPaidDue2, amc.instPaidDue3, amc.instPaidDue4, amc.IsDelinquent);

                    HttpContext.Current.Session["CHKMunicipalJurisdiction"] = CHKMunicipalJurisdiction;
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderno, "TN", "Shelby", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderno, "TN", "Shelby");
                    return "Data Inserted Successfully";
                }
                catch (NoSuchElementException ex1)
                {
                    driver.Quit();
                    throw ex1;
                }

            }
        }

        public void tax_details(string orderno, string parcelNumber)
        {
            string Tax_OwnerName = "", Tax_ProAdddress = "";
            try
            {
                Tax_OwnerName = driver.FindElement(By.XPath("//*[@id='ownerFormView']/tbody/tr/td/table/tbody/tr[1]/td[2]")).Text;
                Tax_ProAdddress = driver.FindElement(By.XPath("//*[@id='ownerFormView']/tbody/tr/td/table/tbody/tr[2]/td[2]")).Text;
                TaxParcelID = driver.FindElement(By.XPath("//*[@id='ownerFormView']/tbody/tr/td/table/tbody/tr[4]/td[2]")).Text;

                string Currenttax1 = Tax_OwnerName + "~" + Tax_ProAdddress + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                gc.insert_date(orderno, parcelNumber, 174, Currenttax1, 1, DateTime.Now);
            }
            catch
            { }

            try
            {
                IWebElement MultiTaxTB = driver.FindElement(By.XPath("//*[@id='PanelMain']/table[3]/tbody/tr[3]/td/table/tbody"));
                IList<IWebElement> MultiTaxTR = MultiTaxTB.FindElements(By.TagName("tr"));
                IList<IWebElement> MultiTaxTD;
                int taxyearcount = 0;
                foreach (IWebElement row1 in MultiTaxTR)
                {
                    MultiTaxTD = row1.FindElements(By.TagName("td"));
                    if (MultiTaxTD.Count != 0)
                    {
                        AssessYear = MultiTaxTD[0].Text;
                        try
                        {
                            if (AssessYear.Trim() != "" && taxyearcount < 3)
                            {
                                IWebElement IYear = MultiTaxTD[0].FindElement(By.TagName("a"));
                                string strYear = IYear.GetAttribute("href");
                                AssessTaxURL.Add(strYear);
                            }
                            if (AssessYear != TaxingYear)
                            {
                                TaxingYear = AssessYear;
                                taxyearcount++;
                            }
                        }
                        catch { }
                        TaxingAuthority = MultiTaxTD[1].Text;
                        AssessmentTax = MultiTaxTD[2].Text;
                        TaxDue = MultiTaxTD[3].Text;
                        Interest = MultiTaxTD[4].Text;
                        TotalFees = MultiTaxTD[5].Text;
                        TotalDue = MultiTaxTD[6].Text;

                        string Currenttax = "" + "~" + "" + "~" + AssessYear + "~" + TaxingAuthority + "~" + AssessmentTax + "~" + TaxDue + "~" + Interest + "~" + TotalFees + "~" + TotalDue + "~" + "";
                        gc.insert_date(orderno, parcelNumber, 174, Currenttax, 1, DateTime.Now);
                        AssessmentTax = ""; TaxingAuthority = "";
                    }
                }
            }
            catch { }
            try
            {
                IWebElement MultiTaxTB1 = driver.FindElement(By.XPath("//*[@id='PanelMain']/table[3]/tbody/tr[4]/td/table/tbody"));
                IList<IWebElement> MultiTaxTR1 = MultiTaxTB1.FindElements(By.TagName("tr"));
                IList<IWebElement> MultiTaxTD1;
                int taxyearcount = 0;
                foreach (IWebElement row1 in MultiTaxTR1)
                {
                    MultiTaxTD1 = row1.FindElements(By.TagName("td"));
                    if (MultiTaxTD1.Count != 0)
                    {
                        AssessTaxYear = MultiTaxTD1[0].Text;
                        TaxDue = MultiTaxTD1[3].Text;
                        Interest = MultiTaxTD1[4].Text;
                        TotalFees = MultiTaxTD1[5].Text;
                        TotalDue = MultiTaxTD1[6].Text;


                        string Currenttax = "" + "~" + "" + "~" + AssessTaxYear + "~" + "" + "~" + "" + "~" + TaxDue + "~" + Interest + "~" + TotalFees + "~" + TotalDue + "~" + TaxAuthority;
                        gc.insert_date(orderno, parcelNumber, 174, Currenttax, 1, DateTime.Now);
                    }
                }
            }
            catch { }

            try
            {
                //Property Tax Notice Details
                string Parent_Window1 = driver.CurrentWindowHandle;
                driver.FindElement(By.XPath("//*[@id='HyperLinkPrinter']")).Click();
                Thread.Sleep(7000);

                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                driver.SwitchTo().Window(driver.WindowHandles.Last());
                Thread.Sleep(4000);

                IWebElement iframeElement = driver.FindElement(By.Id("bobjid_1556792652556_iframe"));
                driver.SwitchTo().Frame(iframeElement);
                Thread.Sleep(2000);

                Notice_Taxingauthority = driver.FindElement(By.XPath("//*[@id='TaxingAuthority1']")).Text;
                Notice_NoticeType = driver.FindElement(By.XPath("//*[@id='NoticeType1']/table/tbody/tr/td/table/tbody/tr/td")).Text;
                Notice_Classification = driver.FindElement(By.XPath("//*[@id='Classification1']/table/tbody/tr/td/table/tbody/tr/td")).Text;
                Notice_Location1 = driver.FindElement(By.XPath("//*[@id='LocationStreet1']/table/tbody/tr/td/table/tbody/tr/td")).Text;
                Notice_Location2 = driver.FindElement(By.XPath("//*[@id='LocationCityStateZip1']/table/tbody/tr/td/table/tbody/tr/td")).Text;
                Notice_Location = Notice_Location1 + Notice_Location2;
                Notice_Subdivision = driver.FindElement(By.XPath("//*[@id='Subdivision1']/table/tbody/tr/td/table/tbody/tr/td")).Text;

                try
                {
                    Notice_TaxYear = driver.FindElement(By.XPath("//*[@id='TaxYear1']/table/tbody/tr/td/table/tbody/tr/td")).Text;
                    Notice_AssedssedValue = driver.FindElement(By.XPath("//*[@id='AssessedValue1']/table/tbody/tr/td/table/tbody/tr/td")).Text;
                    Notice_TaxRate = driver.FindElement(By.XPath("//*[@id='TaxRate1']/table/tbody/tr/td/table/tbody/tr/td")).Text;
                    Notice_Town = driver.FindElement(By.XPath("//*[@id='TownCode2']/table/tbody/tr/td/table/tbody/tr/td")).Text;
                    Notice_BaseTax = driver.FindElement(By.XPath("//*[@id='Taxes1']/table/tbody/tr/td/table/tbody/tr/td")).Text;
                    Notice_Interest = driver.FindElement(By.XPath("//*[@id='Interest1']/table/tbody/tr/td/table/tbody/tr/td")).Text;
                    Notice_Court = driver.FindElement(By.XPath("//*[@id='CourtFees1']/table/tbody/tr/td/table/tbody/tr/td")).Text;
                    Notice_Atty = driver.FindElement(By.XPath("//*[@id='AttyFees1']/table/tbody/tr/td/table/tbody/tr/td")).Text;
                    Notice_Misc = driver.FindElement(By.XPath("//*[@id='MiscFees1']/table/tbody/tr/td/table/tbody/tr/td/span")).Text;
                    Notice_BalnceDue = driver.FindElement(By.XPath("//*[@id='BalanceDue1']/table/tbody/tr/td/table/tbody/tr/td")).Text;
                }
                catch
                { }
                Notice_TotalAmountDue = driver.FindElement(By.XPath("//*[@id='BalanceDue1']/table/tbody/tr/td/table/tbody/tr/td")).Text;

                gc.CreatePdf(orderno, parcelNumber, "Property Tax Notice", driver, "TN", "Shelby");
                Noticetax = Notice_Taxingauthority + "~" + Notice_NoticeType + "~" + Notice_Classification + "~" + Notice_Location + "~" + Notice_Subdivision + "~" + Notice_TaxYear + "~" + Notice_AssedssedValue + "~" + Notice_TaxRate + "~" + Notice_Town + "~" + Notice_BaseTax + "~" + Notice_Interest + "~" + Notice_Court + "~" + Notice_Atty + "~" + Notice_Misc + "~" + Notice_BalnceDue + "~" + Notice_TotalAmountDue;
                gc.insert_date(orderno, parcelNumber, 1854, Noticetax, 1, DateTime.Now);

                driver.SwitchTo().Window(Parent_Window1);
                Thread.Sleep(2000);
            }
            catch { }

            Link1 = "https://apps.shelbycountytrustee.com/TaxQuery/PrintNotice.aspx?ParcelNo=" + Pa6.Trim().Replace(" ", "") + "";
            driver.Navigate().GoToUrl(Link1);
            //downloadpdf.DownloadFile(url, billpdf);
            Thread.Sleep(2000);
            gc.CreatePdf(orderno, parcelNumber, "Property Tax Notice", driver, "TN", "Shelby");
            try
            {
                foreach (string LinkTran in AssessTaxURL)
                {
                    try
                    {
                        driver.Navigate().GoToUrl(LinkTran);
                        Thread.Sleep(2000);
                        string strTransReciept = "", PropertyAddress = "";

                        PropertyAddress = driver.FindElement(By.Id("LabelPropLoc")).Text;
                        TaxYear = driver.FindElement(By.XPath("//*[@id='LabelTaxYear']")).Text;
                        STaxingAuthority = driver.FindElement(By.Id("LabelTown")).Text;

                        IWebElement TransTB = driver.FindElement(By.XPath("//*[@id='PanelMain']/table[2]/tbody"));
                        IList<IWebElement> TransTR = TransTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> TransTD;

                        foreach (IWebElement row1 in TransTR)
                        {
                            TransTD = row1.FindElements(By.TagName("td"));
                            if (TransTD.Count != 0)
                            {
                                IWebElement IReceipt = TransTD[0].FindElement(By.TagName("a"));
                                strTransReciept = IReceipt.GetAttribute("href");
                                TransactionNumber = TransTD[1].Text;
                                TransactionDate = TransTD[2].Text;
                                TransactionType = TransTD[3].Text;
                                TransactionAmount = TransTD[4].Text;
                                string Trnsaction = PropertyAddress + "~" + TaxYear + "~" + TransactionNumber + "~" + TransactionDate + "~" + TransactionType + "~" + TransactionAmount + "~" + STaxingAuthority;
                                gc.insert_date(orderno, parcelNumber, 175, Trnsaction, 1, DateTime.Now);
                                STaxingAuthority = "";
                                gc.CreatePdf(orderno, parcelNumber, "Tax Notice" + TransactionNumber, driver, "TN", "Shelby");

                            }
                        }

                        driver.Navigate().GoToUrl(strTransReciept);
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderno, parcelNumber, "Tax Receipt information" + TaxYear, driver, "TN", "Shelby");

                        try
                        {
                            var chromeOptions = new ChromeOptions();
                            var driver1 = new ChromeDriver(chromeOptions);
                            var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];

                            chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                            chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                            chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                            chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);

                            driver1.Navigate().GoToUrl(LinkTran);
                            Thread.Sleep(1000);
                            string fileName = "Receipt.pdf";
                            IWebElement IReceipt = driver1.FindElement(By.XPath("//*[@id='PanelMain']/table[2]/tbody"));
                            IList<IWebElement> IReceiptRow = IReceipt.FindElements(By.TagName("tr"));
                            IList<IWebElement> IReceiptTD;

                            foreach (IWebElement down in IReceiptRow)
                            {
                                IReceiptTD = down.FindElements(By.TagName("td"));
                                if (IReceiptTD.Count != 0)
                                {
                                    IWebElement IReceiptUrl = IReceiptTD[0].FindElement(By.TagName("a"));
                                    IReceiptUrl.Click();
                                    Thread.Sleep(3000);
                                    driver1.SwitchTo().Window(driver1.WindowHandles.Last());
                                }
                            }
                            gc.downloadfile(driver1.Url, orderno, parcelNumber, "Receipt", "TN", "Shelby");
                            Thread.Sleep(3000);
                            try
                            {
                                gc.AutoDownloadFile(orderno, parcelNumber, "Shelby", "TN", fileName);
                            }
                            catch { }
                            driver1.Quit();
                        }
                        catch { }
                    }
                    catch { }
                }
            }
            catch { }
        }
    }
}