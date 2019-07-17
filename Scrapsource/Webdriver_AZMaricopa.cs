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
    public class Webdriver_AZMaricopa
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        IWebElement addressclick;
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        string firsthalf = "", secodnhalf = "", Total = "", Muiti = "";
        IWebElement IGoodEvenMonth, IGoodLeap, IGoododdmonth;
        public string FTP_Maricopa(string Address, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            // driver = new PhantomJSDriver();
            //driver = new ChromeDriver();
            string AlterNateID = "", PropertyAddress = "", owner = "";
            string[] stringSeparators1 = new string[] { "\r\n" };
            string TaxAuthority1 = "";
            List<string> listurl = new List<string>();
            TaxAuthority1 = "301 West Jefferson Ste 100 Phoenix, Arizona 85003 (602) 506-8511";
            string newaddr = "";
            if (Address.ToUpper().Contains("UNIT") || Address.ToUpper().Contains("APT"))
            {
              newaddr =  Address.ToUpper().Replace("UNIT ", "#").Replace("APT ", "#");
            }
            else
            {
                newaddr = Address.ToUpper();
            }

            using (driver = new PhantomJSDriver())

            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    
                    if (searchType == "titleflex")
                    {

                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", Address, "AZ", "Maricopa");

                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Zero_Maricopa"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        // driver.Navigate().GoToUrl("https://treasurer.maricopa.gov/Pages/LoadPage?page=Contact");

                        driver.Navigate().GoToUrl("https://mcassessor.maricopa.gov/");
                        Thread.Sleep(2000);

                        driver.FindElement(By.Id("searchBar")).SendKeys(newaddr.ToUpper());

                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "AZ", "Maricopa");                       
                        driver.FindElement(By.XPath("/html/body/div[1]/div[3]/div/form/div/button")).SendKeys(Keys.Enter);

                        //IWebElement TaxSewer = driver.FindElement(By.Id("/html/body/div[1]/div[3]/div/form/div[2]/button"));
                        //IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                        //js.ExecuteScript("arguments[0].click();", TaxSewer);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Address search Result", driver, "AZ", "Maricopa");

                        try
                        {
                            Muiti = driver.FindElement(By.XPath("//*[@id='real-property-results-section']/h3/div[2]/div")).Text;
                            string multiCount = gc.Between(Muiti, "Showing ", " result");
                            if (Convert.ToInt32(multiCount) > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Maricopa_Count"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }
                        try
                        {
                            string Nodata = driver.FindElement(By.XPath("//*[@id='search-results-output']/div")).Text;
                            if (Nodata.Contains("We found 0 results"))
                            {
                                HttpContext.Current.Session["Zero_Maricopa"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                        int A = 0;
                        int M = 1;
                        string path = "";
                        try
                        {
                            if (Muiti.Trim() != "Showing 1 result")
                            {

                                IWebElement MultiOwnerTable = driver.FindElement(By.XPath("//*[@id='rp-table']/tbody"));
                                IList<IWebElement> MultiOwnerRow = MultiOwnerTable.FindElements(By.TagName("tr"));
                                IList<IWebElement> MultiOwnerTD;

                                //string AlterNateID = "", PropertyAddress = "", LegalDescriptoin = "", YearBuilt = "";
                                foreach (IWebElement row1 in MultiOwnerRow)
                                {
                                    MultiOwnerTD = row1.FindElements(By.TagName("td"));
                                    if (MultiOwnerTD.Count != 0 && MultiOwnerTD.Count != 2 && MultiOwnerTD.Count != 1)
                                    {
                                        if (MultiOwnerTD[2].Text.Contains(newaddr.ToUpper()))
                                        {
                                            A++;

                                            parcelNumber = MultiOwnerTD[0].Text;
                                            ownername = MultiOwnerTD[1].Text;
                                            PropertyAddress = MultiOwnerTD[2].Text;
                                            string Multi = ownername + "~" + PropertyAddress;
                                            gc.insert_date(orderNumber, parcelNumber, 438, Multi, 1, DateTime.Now);
                                            path = "//*[@id='rp-table']/tbody/tr[" + M + "]/td[1]/a";
                                        }

                                    }
                                    M++;
                                }
                                if (A == 1)
                                {
                                    driver.FindElement(By.XPath(path)).Click();
                                    Thread.Sleep(2000);
                                }
                                if (A > 1 & A < 26)
                                {
                                    HttpContext.Current.Session["multiParcel_Maricopa"] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                            }

                            else if (Muiti.Trim() == "Showing 1 result")
                            {
                                driver.FindElement(By.XPath("//*[@id='rp-table']/tbody/tr/td[1]/a")).Click();
                                Thread.Sleep(3000);
                            }
                        }
                        catch { }

                        //driver.FindElement(By.XPath("//*[@id='rp-table']/tbody/tr/td[1]/a")).Click();
                    }

                    else if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("https://mcassessor.maricopa.gov/");
                        Thread.Sleep(2000);
                        ////*[@id="searchBar"]
                        driver.FindElement(By.Id("searchBar")).SendKeys(parcelNumber);

                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "AZ", "Maricopa");

                        driver.FindElement(By.XPath("/html/body/div[1]/div[3]/div/form/div/button")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Result", driver, "AZ", "Maricopa");


                    }

                    else if (searchType == "ownername")
                    {
                        driver.Navigate().GoToUrl("https://mcassessor.maricopa.gov/");
                        Thread.Sleep(2000);
                        driver.FindElement(By.Id("searchBar")).SendKeys(ownername);

                        gc.CreatePdf_WOP(orderNumber, "Owner search", driver, "AZ", "Maricopa");

                        driver.FindElement(By.XPath("/html/body/div[1]/div[3]/div/form/div/button")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Owner search Result", driver, "AZ", "Maricopa");

                        try
                        {
                            Muiti = driver.FindElement(By.XPath("//*[@id='real-property-results-section']/h3/div[2]/div")).Text;
                            string multiCount = gc.Between(Muiti, "Showing ", " result");
                            if (Convert.ToInt32(multiCount) > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Maricopa_Count"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }
                        try
                        {
                            if (Muiti.Trim() != "Showing 1 result")
                            {
                                IWebElement MultiOwnerTable = driver.FindElement(By.XPath("//*[@id='rp-table']/tbody"));
                                IList<IWebElement> MultiOwnerRow = MultiOwnerTable.FindElements(By.TagName("tr"));
                                IList<IWebElement> MultiOwnerTD;
                                //string AlterNateID = "", PropertyAddress = "", LegalDescriptoin = "", YearBuilt = "";
                                foreach (IWebElement row1 in MultiOwnerRow)
                                {
                                    MultiOwnerTD = row1.FindElements(By.TagName("td"));
                                    if (MultiOwnerTD.Count != 0 && MultiOwnerTD.Count != 2 && MultiOwnerTD.Count != 1)
                                    {

                                        parcelNumber = MultiOwnerTD[0].Text;
                                        ownername = MultiOwnerTD[1].Text;
                                        PropertyAddress = MultiOwnerTD[2].Text;


                                        string Multi = ownername + "~" + PropertyAddress;
                                        gc.insert_date(orderNumber, parcelNumber, 438, Multi, 1, DateTime.Now);

                                    }
                                }
                                HttpContext.Current.Session["multiParcel_Maricopa"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            else if (Muiti.Trim() == "Showing 1 result")
                            {
                                driver.FindElement(By.XPath("//*[@id='rp-table']/tbody/tr/td[1]/a")).Click();
                                Thread.Sleep(3000);

                            }
                        }
                        catch { }
                    }
                    try
                    {
                        string Nodata = driver.FindElement(By.XPath("//*[@id='search-results-output']/div")).Text;
                        if (Nodata.Contains("We found 0 results"))
                        {
                            HttpContext.Current.Session["Zero_Maricopa"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }


                    string MCR = "", Description = "", HighSchoolDistrict = "", ElementarySchoolDistrict = "", LocalJurisdiction = "", STR = "", Subdivision = "", ConstructionYear = "";

                    parcelNumber = driver.FindElement(By.XPath("//*[@id='header']/div[1]/table/tbody/tr/td[1]/h3")).Text;
                    gc.CreatePdf(orderNumber, parcelNumber, "Propety Deatil", driver, "AZ", "Maricopa");

                    try
                    {
                        try
                        {
                            PropertyAddress = driver.FindElement(By.XPath("//*[@id='property-details']/div/div/table/tbody/tr/td[1]/p/a/strong")).Text;
                            ownername = driver.FindElement(By.XPath("//*[@id='owner-details']/div/div/table/tbody/tr/td[1]/p/a")).Text;
                        }
                        catch { }
                        try
                        {
                            PropertyAddress = driver.FindElement(By.XPath("//*[@id='property-details']/div/div/p/a")).Text.Trim();
                            ownername = driver.FindElement(By.XPath("//*[@id='owner-details']/div/div/p/a")).Text.Trim();
                        }
                        catch
                        {

                        }
                        string tablefulltext = driver.FindElement(By.XPath("//*[@id='property-details']/table/tbody")).Text;
                        MCR = gc.Between(tablefulltext, "MCR # ", "Description:").Trim();
                        Description = gc.Between(tablefulltext, "Description:", "Lat/Long").Trim();
                        HighSchoolDistrict = driver.FindElement(By.XPath("//*[@id='property-details']/table/tbody/tr[7]/td[2]")).Text.Trim();
                        ElementarySchoolDistrict = gc.Between(tablefulltext, "Elementary School District ", "Local Jurisdiction").Trim();
                        LocalJurisdiction = gc.Between(tablefulltext, "Local Jurisdiction", "S/T/R").Trim();
                        STR = gc.Between(tablefulltext, "S/T/R", "Market Area/Neighborhood").Trim();
                        Subdivision = driver.FindElement(By.XPath("//*[@id='property-details']/table/tbody/tr[12]/td[2]")).Text.Trim();
                        ConstructionYear = driver.FindElement(By.XPath("//*[@id='r-c-property-details']/table/tbody/tr[1]/td[2]")).Text.Trim();

                    }
                    catch
                    {

                    }
                    //ownername = driver.FindElement(By.XPath("//*[@id='owner-details']/div/div/table/tbody/tr/td[1]/p/a/strong")).Text;
                    //MCR = driver.FindElement(By.XPath("//*[@id='property-details']/table/tbody/tr[1]/td[2]/a")).Text;
                    //Description = driver.FindElement(By.XPath("//*[@id='property-details']/table/tbody/tr[2]/td[2]")).Text;
                    //HighSchoolDistrict = driver.FindElement(By.XPath("//*[@id='property-details']/table/tbody/tr[7]/td[2]")).Text;
                    //ElementarySchoolDistrict = driver.FindElement(By.XPath("//*[@id='property-details']/table/tbody/tr[8]/td[2]")).Text;
                    //LocalJurisdiction = driver.FindElement(By.XPath("//*[@id='property-details']/table/tbody/tr[9]/td[2]")).Text;
                    //STR = driver.FindElement(By.XPath("//*[@id='property-details']/table/tbody/tr[10]/td[2]")).Text;
                    //Subdivision = driver.FindElement(By.XPath("//*[@id='property-details']/table/tbody/tr[12]/td[2]/a")).Text;
                    //try {
                    //    ConstructionYear = driver.FindElement(By.XPath("//*[@id='r-c-property-details']/table/tbody/tr[1]/td[2]")).Text;
                    //}
                    //catch { }

                    string Property = PropertyAddress + "~" + ownername + "~" + MCR + "~" + Description + "~" + HighSchoolDistrict + "~" + ElementarySchoolDistrict + "~" + LocalJurisdiction + "~" + STR + "~" + Subdivision + "~" + ConstructionYear;
                    gc.insert_date(orderNumber, parcelNumber, 444, Property, 1, DateTime.Now);
                    string TaxYear = "";
                    //, LimitedPropertyValue = "", LegalClass = "", AssessmentRatio = "", AssessedFCV = "", AssessedLPV = "", PropertyUseCode = "", PUDescription = "", TaxAreaCode = "", ValuationSource = "";
                    int m;
                    int iRowsCount = driver.FindElements(By.XPath(" //*[@id='valuation-details']/table/thead/tr/th")).Count;
                    string[] TaxYear1 = new string[iRowsCount];
                    string[] FullCashValue = new string[iRowsCount];
                    string[] LimitedPropertyValue = new string[iRowsCount];
                    string[] LegalClass = new string[iRowsCount];
                    string[] Descrip = new string[iRowsCount];
                    string[] AssessmentRatio = new string[iRowsCount];
                    string[] AssessedFCV = new string[iRowsCount];
                    string[] AssessedLPV = new string[iRowsCount];
                    string[] PropertyUseCode = new string[iRowsCount];

                    string[] PUDescription = new string[iRowsCount];
                    string[] TaxAreaCode = new string[iRowsCount];
                    string[] ValuationSource = new string[iRowsCount];

                    IWebElement tbmulti11 = driver.FindElement(By.XPath("//*[@id='valuation-details']/table/tbody"));
                    IList<IWebElement> TRmulti11 = tbmulti11.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDmulti11;
                    int i = 0, j = 0, k;
                    foreach (IWebElement row in TRmulti11)
                    {

                        TDmulti11 = row.FindElements(By.TagName("td"));
                        if (TDmulti11.Count != 0)
                        {
                            if (j == 0)
                            {
                                for (k = 0; k < iRowsCount; k++)
                                    FullCashValue[k] = TDmulti11[k + 1].Text;
                            }
                            if (j == 1)
                            {
                                for (k = 0; k < iRowsCount; k++)
                                    LimitedPropertyValue[k] = TDmulti11[k + 1].Text;
                            }
                            if (j == 2)
                            {
                                for (k = 0; k < iRowsCount; k++)
                                    LegalClass[k] = TDmulti11[k + 1].Text;
                            }
                            if (j == 3)
                            {
                                for (k = 0; k < iRowsCount; k++)
                                    Descrip[k] = TDmulti11[k + 1].Text;
                            }
                            if (j == 4)
                            {
                                for (k = 0; k < iRowsCount; k++)
                                    AssessmentRatio[k] = TDmulti11[k + 1].Text;
                            }
                            if (j == 5)
                            {
                                for (k = 0; k < iRowsCount; k++)
                                    AssessedFCV[k] = TDmulti11[k + 1].Text;
                            }
                            if (j == 6)
                            {
                                for (k = 0; k < iRowsCount; k++)
                                    AssessedLPV[k] = TDmulti11[k + 1].Text;
                            }
                            if (j == 7)
                            {
                                for (k = 0; k < iRowsCount; k++)
                                    PropertyUseCode[k] = TDmulti11[k + 1].Text;
                            }
                            if (j == 8)
                            {
                                for (k = 0; k < iRowsCount; k++)
                                    PUDescription[k] = TDmulti11[k + 1].Text;
                            }
                            if (j == 9)
                            {
                                for (k = 0; k < iRowsCount; k++)
                                    TaxAreaCode[k] = TDmulti11[k + 1].Text;
                            }
                            if (j == 10)
                            {
                                for (k = 0; k < iRowsCount; k++)
                                    ValuationSource[k] = TDmulti11[k + 1].Text;
                            }

                            j++;
                        }
                    }

                    IWebElement tbmulti111 = driver.FindElement(By.XPath("//*[@id='valuation-details']/table/thead"));
                    IList<IWebElement> TRmulti111 = tbmulti111.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDmulti111;
                    int i1 = 0, j1 = 0, k1;
                    foreach (IWebElement row in TRmulti111)
                    {

                        TDmulti111 = row.FindElements(By.TagName("th"));
                        if (TDmulti111.Count != 0)
                        {
                            if (j1 == 0)
                            {
                                for (k1 = 0; k1 < iRowsCount; k1++)
                                    TaxYear1[k1] = TDmulti111[k1].Text;
                            }
                        }
                    }
                    int l;
                    for (l = 0; l < iRowsCount; l++)
                    {
                        string assessment = TaxYear1[l] + "~" + FullCashValue[l] + "~" + LimitedPropertyValue[l] + "~" + LegalClass[l] + "~" + Descrip[l] + "~" + AssessmentRatio[l] + "~" + AssessedFCV[l] + "~" + AssessedLPV[l] + "~" + PropertyUseCode[l] + "~" + PUDescription[l] + "~" + TaxAreaCode[l] + "~" + ValuationSource[l];
                        gc.insert_date(orderNumber, parcelNumber, 446, assessment, 1, DateTime.Now);

                    }

                    driver.Navigate().GoToUrl("http://treasurer.maricopa.gov/");
                    string TaxType = "";
                    string Pa1 = "", Pa2 = "", Pa3 = "", Pa4 = "";
                    string parcelNumber1 = parcelNumber.Replace("-", "");
                    if (parcelNumber1.Count() == 9)
                    {
                        Pa1 = parcelNumber1.Substring(0, 3);
                        Pa2 = parcelNumber1.Substring(3, 2);
                        Pa3 = parcelNumber1.Substring(5, 3);
                        Pa4 = parcelNumber1.Substring(8, 1);
                    }
                    if (parcelNumber1.Count() == 8)
                    {
                        Pa1 = parcelNumber1.Substring(0, 3);
                        Pa2 = parcelNumber1.Substring(3, 2);
                        Pa3 = parcelNumber1.Substring(5, 3);
                    }
                    driver.FindElement(By.XPath("//*[@id='txtParcelNumBook']")).SendKeys(Pa1);
                    driver.FindElement(By.XPath("//*[@id='txtParcelNumMap']")).SendKeys(Pa2);
                    driver.FindElement(By.XPath("//*[@id='txtParcelNumItem']")).SendKeys(Pa3);
                    driver.FindElement(By.XPath("//*[@id='txtParcelNumSplit']")).SendKeys(Pa4);
                    IWebElement click = driver.FindElement(By.XPath("//*[@id='btnGo']"));
                    IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                    js1.ExecuteScript("arguments[0].click();", click);
                    Thread.Sleep(23000);
                    string strTParcel = driver.FindElement(By.XPath("//*[@id='cphMainContent_cphRightColumn_headerSummary']")).Text;
                    string strTaxParcel = GlobalClass.After(strTParcel, "Tax Summary ");
                    string MaillingName = "", AssessedTax = "", Taxpaid = "", TotalDue = "", Activity = "", Amount = "", ActivityDate = "", PaymentDate = "", Transaction = "";
                    Thread.Sleep(5000);
                    gc.CreatePdf(orderNumber, parcelNumber, " History Tax Deatil", driver, "AZ", "Maricopa");

                    try
                    {
                        gc.CreatePdf(orderNumber, parcelNumber, " Current Tax Deatil", driver, "AZ", "Maricopa");

                        try
                        {
                            MaillingName = driver.FindElement(By.XPath("//*[@id='cphMainContent_cphRightColumn_ParcelNASitusLegal_lblNameAddress']/div")).Text.Replace("\r\n", "").Replace("/", " ");
                            try
                            {
                                AssessedTax = driver.FindElement(By.XPath("//*[@id='cphMainContent_cphRightColumn_taxDue2']/li[2]")).Text;
                                Taxpaid = driver.FindElement(By.XPath("//*[@id='cphMainContent_cphRightColumn_taxDue2']/li[4]")).Text;
                                TotalDue = driver.FindElement(By.XPath("//*[@id='cphMainContent_cphRightColumn_taxDue2']/li[6]")).Text;
                            }
                            catch { }
                            try
                            {
                                if (AssessedTax.Trim() == "" || Taxpaid.Trim() == "" || TotalDue.Trim() == "")
                                {
                                    AssessedTax = driver.FindElement(By.XPath("//*[@id='cphMainContent_cphRightColumn_taxDue1']/tbody/tr/td[2]")).Text;
                                    Taxpaid = driver.FindElement(By.XPath("//*[@id='cphMainContent_cphRightColumn_taxDue1']/tbody/tr/td[4]")).Text;
                                    TotalDue = driver.FindElement(By.XPath("//*[@id='cphMainContent_cphRightColumn_taxDue1']/tbody/tr/td[6]")).Text;
                                }
                            }
                            catch { }
                        }
                        catch { }
                        try
                        {
                            PropertyAddress = driver.FindElement(By.XPath("//*[@id='cphMainContent_cphRightColumn_ParcelNASitusLegal_lblSitusAddress']/div")).Text;
                        }
                        catch { }

                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='siteInnerContentContainer']/div/div[2]/div[2]/div[2]/div[3]/div[1]/div[2]/p/a")).Click();
                            Thread.Sleep(9000);
                            gc.CreatePdf(orderNumber, parcelNumber, " Tax Percentage", driver, "AZ", "Maricopa");
                        }
                        catch { }

                        string TaxDue = MaillingName + "~" + PropertyAddress + "~" + AssessedTax + "~" + Taxpaid + "~" + TotalDue + "~" + TaxAuthority1;

                        gc.insert_date(orderNumber, strTaxParcel, 454, TaxDue, 1, DateTime.Now);


                        string AreaCode = "", ExemptionStatus = "", HalfTax = "", AmountTaxPaid = "", InterestPaid = "", FeesPaid = "", TOTALPAID = "", District = "", Name = "", Percentage = "", SpecialTax = "", SpecialTaxPaid = "";
                        string PrimaryTax = "", SecondaryTax = "", FloodTax = "", SpecialDistrictTax = "";

                        AreaCode = driver.FindElement(By.XPath("//*[@id='cphMainContent_cphRightColumn_lblAreaCode']")).Text;
                        AssessedTax = driver.FindElement(By.XPath("//*[@id='cphMainContent_cphRightColumn_lblAssessedTax']")).Text;
                        ExemptionStatus = driver.FindElement(By.XPath("//*[@id='cphMainContent_cphRightColumn_lblExemptionDescription']")).Text;
                        HalfTax = driver.FindElement(By.XPath("//*[@id='cphMainContent_cphRightColumn_lblHalfTax']")).Text;
                        AmountTaxPaid = driver.FindElement(By.XPath("//*[@id='cphMainContent_cphRightColumn_lblAssessedTaxPaid']")).Text;
                        InterestPaid = driver.FindElement(By.XPath("//*[@id='cphMainContent_cphRightColumn_lblInterestPaid']")).Text;
                        FeesPaid = driver.FindElement(By.XPath("//*[@id='cphMainContent_cphRightColumn_hlTaxYear']")).Text;
                        TOTALPAID = driver.FindElement(By.XPath("//*[@id='cphMainContent_cphRightColumn_lblTotalPaid']")).Text;

                        string TaxDetail = AreaCode + "~" + AssessedTax + "~" + ExemptionStatus + "~" + HalfTax + "~" + AmountTaxPaid + "~" + InterestPaid + "~" + FeesPaid + "~" + TOTALPAID;
                        gc.insert_date(orderNumber, strTaxParcel, 453, TaxDetail, 1, DateTime.Now);

                        //PrimaryTax = driver.FindElement(By.XPath("//*[@id='cphMainContent_cphRightColumn_lblACPrimaryPercent']")).Text;
                        //SecondaryTax = driver.FindElement(By.XPath("//*[@id='cphMainContent_cphRightColumn_lblACSecondaryPercent']")).Text;
                        //FloodTax = driver.FindElement(By.XPath("//*[@id='cphMainContent_cphRightColumn_lblFloodPercent']")).Text;
                        //SpecialDistrictTax = driver.FindElement(By.XPath("//*[@id='cphMainContent_cphRightColumn_lblTotalSpecialDistrictPercent']")).Text;

                        IWebElement MultiCurrentTaxHistoryTB = driver.FindElement(By.XPath("//*[@id='cphMainContent_cphRightColumn_gvSpecialDistricts']/tbody"));
                        IList<IWebElement> MultiCurrentTaxHistoryTR = MultiCurrentTaxHistoryTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> MultiCurrentTaxHistoryTD;

                        foreach (IWebElement row1 in MultiCurrentTaxHistoryTR)
                        {
                            MultiCurrentTaxHistoryTD = row1.FindElements(By.TagName("td"));
                            if (MultiCurrentTaxHistoryTD.Count != 0)
                            {

                                District = MultiCurrentTaxHistoryTD[0].Text;
                                Name = MultiCurrentTaxHistoryTD[1].Text;
                                Percentage = MultiCurrentTaxHistoryTD[2].Text;
                                SpecialTax = MultiCurrentTaxHistoryTD[3].Text;
                                SpecialTaxPaid = MultiCurrentTaxHistoryTD[4].Text;

                                string CurrentTaxDetail = District + "~" + Name + "~" + Percentage + "~" + SpecialTax + "~" + SpecialTaxPaid;
                                gc.insert_date(orderNumber, strTaxParcel, 554, CurrentTaxDetail, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    try
                    {
                        driver.Navigate().GoToUrl("https://treasurer.maricopa.gov/Parcel/Summary.aspx");
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='cphMainContent_cphRightColumn_taxDue1']/tbody/tr/td[6]/a")).Click();
                            Thread.Sleep(3000);
                        }
                        catch { }

                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='cphMainContent_cphRightColumn_taxDue2']/li[6]/a")).Click();
                            Thread.Sleep(3000);
                        }
                        catch { }
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Due", driver, "AZ", "Maricopa");
                        string strDueDate = "";
                        try
                        {
                            IWebElement IGoodDate = driver.FindElement(By.Id("cphMainContent_cphRightColumn_datePicker"));
                            string strGoodDate = IGoodDate.GetAttribute("value");
                            strDueDate = strGoodDate;
                        }
                        catch { }
                        IWebElement ITaxdueTable = driver.FindElement(By.XPath("//*[@id='cphMainContent_cphRightColumn_pnlOpen']/table/tbody"));
                        IList<IWebElement> ITaxDueRow = ITaxdueTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> ITaxTd;
                        foreach (IWebElement due in ITaxDueRow)
                        {
                            ITaxTd = due.FindElements(By.TagName("td"));
                            if (due.Text.Contains("Interest Due:") || due.Text.Contains("Fees Due:"))
                            {
                                if (!due.Text.Contains("$0.00"))
                                {
                                    TaxType = "Delinquent";
                                }
                            }
                            if (ITaxTd.Count != 0)
                            {
                                firsthalf += ITaxTd[1].Text + "~";
                                secodnhalf += ITaxTd[2].Text + "~";
                                Total += ITaxTd[3].Text + "~";
                            }
                        }
                        IWebElement ITaxdueTotalTable = driver.FindElement(By.XPath("//*[@id='cphMainContent_cphRightColumn_pnlOpen']/table/tfoot"));
                        IList<IWebElement> ITaxTotalDueRow = ITaxdueTotalTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> ITaxTotalTd;
                        foreach (IWebElement due in ITaxTotalDueRow)
                        {
                            ITaxTotalTd = due.FindElements(By.TagName("td"));
                            if (ITaxTotalTd.Count != 0 && due.Text.Contains("Total"))
                            {
                                firsthalf += ITaxTotalTd[1].Text + "~";
                                secodnhalf += ITaxTotalTd[2].Text + "~";
                                Total += ITaxTotalTd[3].Text + "~";
                            }
                        }

                        string firstTaxdueDetails = firsthalf.Remove(firsthalf.Length - 1, 1);
                        gc.insert_date(orderNumber, strTaxParcel, 550, strDueDate + "~" + "First Half" + "~" + firstTaxdueDetails, 1, DateTime.Now);
                        string secodTaxdueDetails = secodnhalf.Remove(secodnhalf.Length - 1, 1);
                        gc.insert_date(orderNumber, strTaxParcel, 550, "" + "~" + "Second Half" + "~" + secodTaxdueDetails, 1, DateTime.Now);
                        string TotalTaxdueDetails = Total.Remove(Total.Length - 1, 1);
                        gc.insert_date(orderNumber, strTaxParcel, 550, "" + "~" + "Total" + "~" + TotalTaxdueDetails, 1, DateTime.Now);
                    }
                    catch { }


                    if (TaxType != "" && TaxType == "Delinquent")
                    {
                        try
                        {
                            IWebElement IGoodDate = driver.FindElement(By.Id("cphMainContent_cphRightColumn_datePicker"));
                            string strGoodDate = IGoodDate.GetAttribute("value");
                            string[] strGoodThDate = strGoodDate.Split('/');
                            try
                            {
                                if (Convert.ToInt32(strGoodThDate[2]) <= 15)
                                {
                                    IGoodDate.Click();
                                }
                                if (Convert.ToInt32(strGoodThDate[2]) > 15)
                                {
                                    IGoodDate.Click();
                                    driver.FindElement(By.XPath("//*[@id='ui-datepicker-div']/div/a[2]/span")).Click();
                                }

                                IWebElement Igooddate = driver.FindElement(By.XPath("//*[@id='ui-datepicker-div']/table/tbody"));
                                IList<IWebElement> IgooddateRow = Igooddate.FindElements(By.TagName("tr"));
                                IList<IWebElement> IgoodDateTD;
                                foreach (IWebElement date in IgooddateRow)
                                {
                                    IgoodDateTD = date.FindElements(By.TagName("a"));
                                    if (IgoodDateTD.Count != 0 && (date.Text.Contains("29") || date.Text.Contains("28")))
                                    {
                                        for (int leap = 0; leap < IgoodDateTD.Count; leap++)
                                        {
                                            try
                                            {
                                                if (IgoodDateTD[leap].Text == "28" || IgoodDateTD[leap].Text == "29")
                                                {
                                                    IGoodLeap = IgoodDateTD[leap];
                                                }
                                            }
                                            catch { }
                                        }
                                    }
                                    if (IgoodDateTD.Count != 0 && date.Text.Contains("30"))
                                    {
                                        for (int even = 0; even < IgoodDateTD.Count; even++)
                                        {
                                            try
                                            {
                                                if (IgoodDateTD[even].Text == "30")
                                                {

                                                    IGoodEvenMonth = IgoodDateTD[even];
                                                }
                                            }
                                            catch { }
                                        }
                                    }
                                    if (IgoodDateTD.Count != 0 && date.Text.Contains("31"))
                                    {
                                        for (int odd = 0; odd < IgoodDateTD.Count; odd++)
                                        {
                                            try
                                            {
                                                if (IgoodDateTD[odd].Text == "31")
                                                {

                                                    IGoododdmonth = IgoodDateTD[odd];
                                                }
                                            }
                                            catch { }
                                        }
                                    }
                                }
                                if (IGoodEvenMonth.Text == "" && IGoodLeap.Text != "" && IGoododdmonth.Text == "")
                                {
                                    IGoodLeap.Click();
                                }
                                if (IGoodEvenMonth.Text != "" && IGoodLeap.Text != "" && IGoododdmonth.Text == "")
                                {
                                    IGoodEvenMonth.Click();
                                }
                                if (IGoodEvenMonth.Text != "" && IGoodLeap.Text != "" && IGoododdmonth.Text != "")
                                {
                                    IGoododdmonth.Click();
                                }

                            }
                            catch { }
                            try
                            {
                                string Deliqfirsthalf = "", Deliqsecodnhalf = "", DeliqTotal = "";
                                string strDelinqDate = "";
                                try
                                {
                                    IWebElement IGoodDelinqDate = driver.FindElement(By.Id("cphMainContent_cphRightColumn_datePicker"));
                                    string strGoodDelinqDate = IGoodDelinqDate.GetAttribute("value");
                                    strDelinqDate = strGoodDelinqDate;
                                }
                                catch { }
                                gc.CreatePdf(orderNumber, parcelNumber, "Tax Delinquent", driver, "AZ", "Maricopa");
                                IWebElement ITaxdueDelinTable = driver.FindElement(By.XPath("//*[@id='cphMainContent_cphRightColumn_pnlOpen']/table/tbody"));
                                IList<IWebElement> ITaxDueDelinqRow = ITaxdueDelinTable.FindElements(By.TagName("tr"));
                                IList<IWebElement> ITaxDelinqTd;
                                foreach (IWebElement due in ITaxDueDelinqRow)
                                {
                                    ITaxDelinqTd = due.FindElements(By.TagName("td"));
                                    if (ITaxDelinqTd.Count != 0)
                                    {
                                        Deliqfirsthalf += ITaxDelinqTd[1].Text + "~";
                                        Deliqsecodnhalf += ITaxDelinqTd[2].Text + "~";
                                        DeliqTotal += ITaxDelinqTd[3].Text + "~";
                                    }
                                }
                                IWebElement ITaxDelinTotal = driver.FindElement(By.XPath("//*[@id='cphMainContent_cphRightColumn_pnlOpen']/table/tfoot"));
                                IList<IWebElement> ITaxDelinRow = ITaxDelinTotal.FindElements(By.TagName("tr"));
                                IList<IWebElement> ITaxDelinTd;
                                foreach (IWebElement due in ITaxDelinRow)
                                {
                                    ITaxDelinTd = due.FindElements(By.TagName("td"));
                                    if (ITaxDelinTd.Count != 0 && due.Text.Contains("Total"))
                                    {
                                        Deliqfirsthalf += ITaxDelinTd[1].Text + "~";
                                        Deliqsecodnhalf += ITaxDelinTd[2].Text + "~";
                                        DeliqTotal += ITaxDelinTd[3].Text + "~";
                                    }
                                }

                                string firstDelinqDetails = Deliqfirsthalf.Remove(Deliqfirsthalf.Length - 1, 1);
                                gc.insert_date(orderNumber, strTaxParcel, 550, strDelinqDate + "~" + "First Half" + "~" + firstDelinqDetails, 1, DateTime.Now);
                                string secodDelinqDetails = Deliqsecodnhalf.Remove(Deliqsecodnhalf.Length - 1, 1);
                                gc.insert_date(orderNumber, strTaxParcel, 550, "" + "~" + "Second Half" + "~" + secodDelinqDetails, 1, DateTime.Now);
                                string TotalDelinqDetails = DeliqTotal.Remove(DeliqTotal.Length - 1, 1);
                                gc.insert_date(orderNumber, strTaxParcel, 550, "" + "~" + "Total" + "~" + TotalDelinqDetails, 1, DateTime.Now);
                            }
                            catch { }
                        }
                        catch { }
                    }

                    driver.Navigate().GoToUrl("https://treasurer.maricopa.gov/Parcel/Activities.aspx");
                    gc.CreatePdf(orderNumber, parcelNumber, " Tax Activities Deatil", driver, "AZ", "Maricopa");
                    try
                    {
                        IWebElement MultiAssessTB = driver.FindElement(By.XPath("//*[@id='cphMainContent_cphRightColumn_activityList_gvActs']/tbody"));
                        IList<IWebElement> MultiAssessTR = MultiAssessTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> MultiAssessTD;
                        string AmountDue = "", status = "";
                        if (!MultiAssessTB.Text.Contains("No activity records"))
                        {
                            foreach (IWebElement row1 in MultiAssessTR)
                            {
                                MultiAssessTD = row1.FindElements(By.TagName("td"));
                                if (MultiAssessTD.Count != 0 && row1.Text.Trim() != "")
                                {
                                    TaxYear = MultiAssessTD[0].Text;
                                    Activity = MultiAssessTD[1].Text;
                                    Amount = MultiAssessTD[2].Text;
                                    ActivityDate = MultiAssessTD[3].Text;
                                    PaymentDate = MultiAssessTD[4].Text;
                                    Transaction = MultiAssessTD[5].Text;

                                    string TaxHis = TaxYear + "~" + Activity + "~" + Amount + "~" + ActivityDate + "~" + PaymentDate + "~" + Transaction;
                                    gc.insert_date(orderNumber, strTaxParcel, 455, TaxHis, 1, DateTime.Now);
                                }
                            }
                        }
                    }
                    catch { }
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='linkTaxBill']")).Click();
                        Thread.Sleep(5000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Bill", driver, "AZ", "Maricopa");
                    }
                    catch { }
                    try
                    {

                        string TaxDistrict = "", Rate = "", TaxFYear = "", TaxSYear = "", Change = "", strFyear = "", strSyear = "";
                        driver.Navigate().GoToUrl("https://treasurer.maricopa.gov/Parcel/DetailedTaxStatement.aspx");
                        gc.CreatePdf(orderNumber, parcelNumber, "Detailed Tax Statement", driver, "AZ", "Maricopa");
                        strFyear = driver.FindElement(By.XPath("//*[@id='cphMainContent_cphRightColumn_dtlTaxBill_gvPrimaryDistricts']/thead/tr/th[3]")).Text;
                        strSyear = driver.FindElement(By.XPath("//*[@id='cphMainContent_cphRightColumn_dtlTaxBill_gvPrimaryDistricts']/thead/tr/th[4]")).Text;
                        IWebElement IDetailedTable = driver.FindElement(By.XPath("//*[@id='cphMainContent_cphRightColumn_dtlTaxBill_gvPrimaryDistricts']/tbody"));
                        string tabletext = IDetailedTable.Text;
                        if (!tabletext.Contains("No activity records"))
                        {

                            IList<IWebElement> IDetailedRow = IDetailedTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> IDetailedTD;
                            foreach (IWebElement detail in IDetailedRow)
                            {
                                IDetailedTD = detail.FindElements(By.TagName("td"));
                                if (IDetailedTD.Count != 0 && detail.Text.Contains("State Aid"))
                                {

                                    Rate = IDetailedTD[1].Text;
                                    TaxFYear = IDetailedTD[2].Text;
                                    TaxSYear = IDetailedTD[3].Text;
                                    Change = IDetailedTD[4].Text;

                                    string Detailed = TaxDistrict + "~" + Rate + "~" + TaxFYear + "(" + strFyear + ")" + "~" + TaxSYear + "(" + strSyear + ")" + "~" + Change;
                                    gc.insert_date(orderNumber, strTaxParcel, 555, Detailed, 1, DateTime.Now);
                                }
                            }
                        }
                    }
                    catch { }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");

                    gc.insert_TakenTime(orderNumber, "AZ", "Maricopa", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "AZ", "Maricopa");
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