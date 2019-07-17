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
using System.Collections.ObjectModel;

namespace ScrapMaricopa.Scrapsource
{
    public class Webdriver_SedgwickKS
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();
        string Yearbuild, parcel_number, Addressmax, parcelsplit1, parcelsplit2, DUE, Owner_Name, multiparcel = "";
        int value = 0;
        string[] ParcelSplit;
        IWebElement PropertyValidation;
        public string FTP_Sedgwick_KS(string streetno, string direction, string streetname, string streettype, string unitnumber, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver()) //PhantomJSDriver
            {
                //rdp
                string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
                string address = "", Capitionresult = "", totalresultvalue = "", Parcel_number = "", Tax_Authority = "", Addresshrf = "", Pin = "", yearpro = "", Condition = "";
                try
                {
                    driver.Navigate().GoToUrl("https://www.sedgwickcounty.org/treasurer/");
                    Thread.Sleep(2000);
                    gc.CreatePdf_WOP(orderNumber, "Tax Authority", driver, "KS", "Sedgwick");
                    //*[@id="leftColumn"]/section/address/ul/li
                    string Tax_Authority1 = driver.FindElement(By.XPath("//*[@id='leftColumn']/section/address/ul/li")).Text;
                    string[] Yearhref = Tax_Authority1.Split('\r');
                    Tax_Authority = Yearhref[6].Replace("\n", "") + " " + Yearhref[7].Replace("\n", "") + " " + Yearhref[3].Replace("\n", "") + " " + Yearhref[4].Replace("\n", "");
                }
                catch (Exception ex)
                {
                }
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    //Tax Authority information
                    driver.Navigate().GoToUrl("https://ssc.sedgwickcounty.org/propertytax/");
                    driver.FindElement(By.Id("ctl00_mainContentPlaceHolder_acceptButton")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf_WOP(orderNumber, "Agree Button", driver, "KS", "Sedgwick");
                    if (searchType == "titleflex")
                    {
                        string addressTex = streetno + " " + streettype + " " + streetname + "" + unitnumber;
                        gc.TitleFlexSearch(orderNumber, "", ownername, addressTex, "KS", "Sedgwick");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_SedgwickKS"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        if (direction.Trim() == "")
                        {
                            address = streetno + " " + streetname + " " + streettype.Trim();
                        }
                        if (direction.Trim() != "")
                        {
                            address = streetno + " " + direction + " " + streetname + " " + streettype.Trim();
                        }
                        driver.FindElement(By.Id("ctl00_mainContentPlaceHolder_keywordsTextBox")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "SearchBefore", driver, "KS", "Sedgwick");
                        driver.FindElement(By.Id("ctl00_mainContentPlaceHolder_searchButton")).Click();
                        Thread.Sleep(2000);
                        try
                        {
                            int Max = 0;
                            gc.CreatePdf_WOP(orderNumber, "SearchAfter", driver, "KS", "Sedgwick");
                            IWebElement Multiparceladdress = driver.FindElement(By.XPath("//*[@id='aspnetForm']/table/tbody"));
                            IList<IWebElement> Multiparcelrow = Multiparceladdress.FindElements(By.TagName("tr"));
                            IList<IWebElement> Multiparcelid;
                            foreach (IWebElement multiparcel in Multiparcelrow)
                            {
                                Multiparcelid = multiparcel.FindElements(By.TagName("td"));
                                if (Multiparcelid.Count != 0 && Multiparcelid[2].Text != Pin)
                                {
                                    IWebElement Address1 = Multiparcelid[0].FindElement(By.TagName("a"));
                                    Addresshrf = Address1.GetAttribute("href");
                                    string Addressst = Address1.Text;
                                    string Owner = Multiparcelid[1].Text;
                                    Pin = Multiparcelid[2].Text;
                                    string Multiparcel = Addressst + "~" + Owner;
                                    gc.insert_date(orderNumber, Pin, 1196, Multiparcel, 1, DateTime.Now);
                                    Max++;
                                }
                            }
                            if (Max == 1)
                            {
                                driver.Navigate().GoToUrl(Addresshrf);
                                Thread.Sleep(2000);
                            }
                            if (Max > 1 && Max < 26)
                            {
                                HttpContext.Current.Session["multiParcel_Sedgwick"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Max > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Sedgwick_Multicount"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                            if (Max == 0)
                            {
                                HttpContext.Current.Session["Nodata_SedgwickKS"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }

                        }
                        catch { }                      
                    }
                    if (searchType == "parcel")
                    {
                        if (parcelNumber.Trim() != "")
                        {
                            driver.FindElement(By.Id("ctl00_mainContentPlaceHolder_keywordsTextBox")).SendKeys(parcelNumber);
                            driver.FindElement(By.Id("ctl00_mainContentPlaceHolder_searchButton")).Click();
                            Thread.Sleep(2000);
                            try
                            {
                                driver.FindElement(By.Id("ctl00_mainContentPlaceHolder_resultsRepeater_ctl01_situsAddressHyperLink")).Click();
                                Thread.Sleep(2000);
                            }
                            catch { }
                        }
                        if (parcelNumber.Trim() == "")
                        {
                            HttpContext.Current.Session["Nodata_SedgwickKS"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    if (searchType == "unitnumber")
                    {
                        driver.FindElement(By.Id("ctl00_mainContentPlaceHolder_keywordsTextBox")).SendKeys(unitnumber);
                        driver.FindElement(By.Id("ctl00_mainContentPlaceHolder_searchButton")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.Id("ctl00_mainContentPlaceHolder_resultsRepeater_ctl01_situsAddressHyperLink")).Click();
                        Thread.Sleep(2000);
                    }

                    try
                    {
                        string Nodata = driver.FindElement(By.Id("ctl00_mainContentPlaceHolder_noResultsPanel")).Text;
                        if (Nodata == "No properties were found for the specified search criteria.")
                        {
                            HttpContext.Current.Session["Nodata_SedgwickKS"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    string current = DateTime.Now.Year.ToString();
                    string Propertaddress = driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[4]/div")).Text;
                    // Parcel_number = gc.Between(Propertaddress, "Pin", "\r\n");
                    string Propertyaddress = GlobalClass.After(Propertaddress, "\r\n");
                    string Propertydes = driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[5]/table[1]/tbody")).Text;
                    string Legaldes = gc.Between(Propertydes, "Legal Description", "Owner");
                    string Ownerpro = gc.Between(Propertydes, "Owner", "Mailing Address");
                    string MailingAddress = gc.Between(Propertydes, "Mailing Address", "Geo Code");
                    string GeoCode = gc.Between(Propertydes, "Geo Code", "PIN");
                    Parcel_number = gc.Between(Propertydes, "PIN", "\r\nAIN");
                    string AIN = gc.Between(Propertydes, "AIN", "Tax Unit");
                    string TaxUnit = gc.Between(Propertydes, "Tax Unit", "Land Use");
                    string LandUse = gc.Between(Propertydes, "Land Use", "Market Land Square Feet");
                    string MarketLandSquareFeet = gc.Between(Propertydes, "Market Land Square Feet", "Total Acres").Replace(current, "");
                    string TotalAcres = gc.Between(Propertydes, "Total Acres", "Appraisal").Replace(current, "");
                    string Appraisal = gc.Between(Propertydes, "Appraisal", "Assessment").Replace(current, "");
                    string Assessment = GlobalClass.After(Propertydes, "Assessment").Replace(current, "");
                    try
                    {
                        string residentialelement = driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[5]/table[2]/tbody")).Text;
                        yearpro = gc.Between(residentialelement, "Year Built", "Bedrooms");
                        Condition = gc.Between(residentialelement, "Condition", "More Details");
                    }
                    catch { }
                    string Propertyresult = Propertyaddress + "~" + Legaldes + "~" + Ownerpro + "~" + MailingAddress + "~" + GeoCode + "~" + AIN + "~" + TaxUnit + "~" + LandUse + "~" + MarketLandSquareFeet + "~" + TotalAcres + "~" + Appraisal + "~" + Assessment + "~" + yearpro + "~" + Condition + "~" + Tax_Authority;
                    gc.insert_date(orderNumber, Parcel_number, 1183, Propertyresult, 1, DateTime.Now);
                    gc.CreatePdf(orderNumber, Parcel_number, "PropertyDetail", driver, "KS", "Sedgwick");
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[3]/nav[2]/ul/li[2]/a")).Click();
                    Thread.Sleep(2000);
                    driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[6]/div[1]/div[2]/a")).Click();
                    Thread.Sleep(1000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Appricel value", driver, "KS", "Sedgwick");
                    IWebElement Appricelvalue = driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[6]/div[1]/div[1]/table/tbody"));
                    IList<IWebElement> Appricelvaluerow = Appricelvalue.FindElements(By.TagName("tr"));
                    IList<IWebElement> Appricelvalueid;
                    IList<IWebElement> AppricelValueth;
                    foreach (IWebElement Appriceal in Appricelvaluerow)
                    {
                        Appricelvalueid = Appriceal.FindElements(By.TagName("td"));
                        AppricelValueth = Appriceal.FindElements(By.TagName("th"));
                        if (Appricelvalueid.Count != 0 && AppricelValueth.Count != 0 && !AppricelValueth[0].Text.Contains("Land") && !AppricelValueth[0].Text.Contains("Improvements") && !AppricelValueth[0].Text.Contains("Total"))
                        {
                            string Yearappricial = AppricelValueth[0].Text;
                            string Classappricial = Appricelvalueid[0].Text;
                            string Laneid = Appricelvalueid[2].Text;
                            string Improveid = Appricelvalueid[3].Text;
                            string total1 = Appricelvalueid[4].Text;
                            string total2 = Appricelvalueid[5].Text;
                            string Appricialresult = Yearappricial + "~" + Classappricial + "~" + Laneid + "~" + Improveid + "~" + total1 + "~" + total2;
                            gc.insert_date(orderNumber, Parcel_number, 1185, Appricialresult, 1, DateTime.Now);
                            Appricialresult = "";
                        }

                    }
                    driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[6]/div[2]/div[2]/a")).Click();
                    Thread.Sleep(1000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Assessment value", driver, "KS", "Sedgwick");
                    //*[@id="aspnetForm"]/div[6]/div[2]/div[1]/table/tbody
                    IWebElement Assessmentvalue = driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[6]/div[2]/div[1]/table/tbody"));
                    IList<IWebElement> Assessmentvaluerow = Assessmentvalue.FindElements(By.TagName("tr"));
                    IList<IWebElement> Assessmentvalueid;
                    IList<IWebElement> AssessmentValueth;
                    foreach (IWebElement Assessmentweb in Assessmentvaluerow)
                    {
                        Assessmentvalueid = Assessmentweb.FindElements(By.TagName("td"));
                        AssessmentValueth = Assessmentweb.FindElements(By.TagName("th"));
                        if (Assessmentvalueid.Count != 0 && AssessmentValueth.Count != 0 && !AssessmentValueth[0].Text.Contains("Land") && !AssessmentValueth[0].Text.Contains("Improvements") && !AssessmentValueth[0].Text.Contains("Total"))
                        {
                            string YearAssessment = AssessmentValueth[0].Text;
                            string ClassAssessment = Assessmentvalueid[0].Text;
                            string LaneAssessmentid = Assessmentvalueid[2].Text;
                            string ImproveAssessmentid = Assessmentvalueid[3].Text;
                            string totalAssessment1 = Assessmentvalueid[4].Text;
                            string totalAssessment2 = Assessmentvalueid[5].Text;
                            string Assessmentresult = YearAssessment + "~" + ClassAssessment + "~" + LaneAssessmentid + "~" + ImproveAssessmentid + "~" + totalAssessment1 + "~" + totalAssessment2;
                            gc.insert_date(orderNumber, Parcel_number, 1186, Assessmentresult, 1, DateTime.Now);
                            Assessmentresult = "";
                        }

                    }
                    driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[3]/nav[2]/ul/li[3]/a")).Click();
                    Thread.Sleep(2000);
                    try
                    {
                        gc.CreatePdf(orderNumber, Parcel_number, "special Assessments", driver, "KS", "Sedgwick");
                        for (int i = 1; i < 4; i++)
                        {
                            IWebElement specialAssmenttable = driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[6]/table[" + i + "]"));
                            IList<IWebElement> spcialAssmentrow = specialAssmenttable.FindElements(By.TagName("tr"));
                            IList<IWebElement> spcialAssmentrow1 = specialAssmenttable.FindElements(By.TagName("tbody"));
                            IList<IWebElement> spcialAssmenttdid;
                            IList<IWebElement> SplicialAssmentcapition;
                            IList<IWebElement> splicialAssmentth;
                            //IList<IWebElement> tbodyid;
                            foreach (IWebElement Spcialassment in spcialAssmentrow)
                            {
                                spcialAssmenttdid = Spcialassment.FindElements(By.TagName("td"));
                                splicialAssmentth = Spcialassment.FindElements(By.TagName("th"));
                                SplicialAssmentcapition = specialAssmenttable.FindElements(By.TagName("caption"));
                                try
                                {
                                    if (SplicialAssmentcapition.Count != 0 && splicialAssmentth[0].Text.Trim() != "Totals:")
                                    {
                                        string Capition = SplicialAssmentcapition[0].Text;
                                        Capitionresult = GlobalClass.Before(Capition, "Tax Year Special");

                                    }
                                }
                                catch { }
                                try
                                {
                                    if (splicialAssmentth[0].Text.Trim() == "Totals:" && spcialAssmenttdid.Count != 0)
                                    {
                                        string totalresult = splicialAssmentth[0].Text + "~" + spcialAssmenttdid[0].Text + "~" + spcialAssmenttdid[1].Text + "~" + spcialAssmenttdid[2].Text;
                                        totalresultvalue = Capitionresult + "~" + "~" + "~" + "~" + totalresult;

                                    }
                                }
                                catch { }
                                if (spcialAssmenttdid.Count != 0 && spcialAssmenttdid.Count == 5)
                                {
                                    string tbodyresult = spcialAssmenttdid[0].Text + "~" + spcialAssmenttdid[1].Text + "~" + "~" + "~" + spcialAssmenttdid[2].Text + "~" + spcialAssmenttdid[3].Text + "~" + spcialAssmenttdid[4].Text;
                                    string Tbodyresult = Capitionresult + "~" + tbodyresult;
                                    gc.insert_date(orderNumber, Parcel_number, 1189, Tbodyresult, 1, DateTime.Now);
                                }
                                if (spcialAssmenttdid.Count != 0 && spcialAssmenttdid.Count == 7)
                                {
                                    string tbodyresult = spcialAssmenttdid[0].Text + "~" + spcialAssmenttdid[1].Text + "~" + spcialAssmenttdid[2].Text + "~" + spcialAssmenttdid[3].Text + "~" + spcialAssmenttdid[4].Text + "~" + spcialAssmenttdid[5].Text + "~" + spcialAssmenttdid[6].Text;
                                    string Tbodyresult = Capitionresult + "~" + tbodyresult;
                                    gc.insert_date(orderNumber, Parcel_number, 1189, Tbodyresult, 1, DateTime.Now);
                                }

                            }
                            gc.insert_date(orderNumber, Parcel_number, 1189, totalresultvalue, 1, DateTime.Now);
                        }
                    }
                    catch { }
                    driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[3]/nav[2]/ul/li[4]/a")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Taxbilling", driver, "KS", "Sedgwick");
                    IWebElement taxbillingTable = driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[6]/table[1]/tbody"));
                    IList<IWebElement> taxbillingrow = taxbillingTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> taxbillingid;
                    foreach (IWebElement taxbilling in taxbillingrow)
                    {
                        taxbillingid = taxbilling.FindElements(By.TagName("td"));
                        if (taxbillingid.Count > 3)
                        {
                            string taxbillingresult = taxbillingid[0].Text + "~" + taxbillingid[1].Text + "~" + taxbillingid[2].Text + "~" + taxbillingid[3].Text + "~" + taxbillingid[4].Text + "~" + taxbillingid[5].Text + "~" + taxbillingid[6].Text + "~" + taxbillingid[7].Text + "~" + taxbillingid[8].Text;
                            gc.insert_date(orderNumber, Parcel_number, 1190, taxbillingresult, 1, DateTime.Now);
                        }
                        if (taxbillingid.Count == 3)
                        {
                            string Taxbilling = taxbillingid[2].Text;

                            HttpContext.Current.Session["Taxbilling_Sedgwick"] = Taxbilling;
                        }
                    }

                    IWebElement TaxAuthoritiesTable = driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[6]/table[2]/tbody"));
                    IList<IWebElement> TaxAuthoritiesrow = TaxAuthoritiesTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> TaxAuthoritiesid;
                    foreach (IWebElement TaxAuthorities in TaxAuthoritiesrow)
                    {
                        TaxAuthoritiesid = TaxAuthorities.FindElements(By.TagName("td"));
                        if (TaxAuthoritiesid.Count != 0)
                        {
                            string TaxAuthoritiesresult = TaxAuthoritiesid[0].Text + "~" + TaxAuthoritiesid[1].Text;
                            gc.insert_date(orderNumber, Parcel_number, 1191, TaxAuthoritiesresult, 1, DateTime.Now);
                        }
                    }
                    string totaltax = driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[6]/table[2]/tfoot/tr")).Text;
                    string[] totalsplit = totaltax.Split(':');
                    string TotalResultsplit = totalsplit[0] + "~" + totalsplit[1];
                    gc.insert_date(orderNumber, Parcel_number, 1191, TotalResultsplit, 1, DateTime.Now);
                    driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[3]/nav[2]/ul/li[5]/a")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Transaction History", driver, "KS", "Sedgwick");
                    IWebElement TransactionHistoryTable = driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[6]/table/tbody"));
                    IList<IWebElement> TransactionHistoryrow = TransactionHistoryTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> TransactionHistoryid;
                    foreach (IWebElement TransactionHistory in TransactionHistoryrow)
                    {
                        TransactionHistoryid = TransactionHistory.FindElements(By.TagName("td"));
                        if (TransactionHistoryid.Count != 0)
                        {
                            string TransactionHistoryresult = TransactionHistoryid[0].Text + "~" + TransactionHistoryid[1].Text + "~" + TransactionHistoryid[2].Text + "~" + TransactionHistoryid[3].Text + "~" + TransactionHistoryid[4].Text + "~" + TransactionHistoryid[5].Text + "~" + TransactionHistoryid[6].Text;
                            gc.insert_date(orderNumber, Parcel_number, 1192, TransactionHistoryresult, 1, DateTime.Now);
                        }
                    }
                    string currenturl = driver.CurrentWindowHandle;
                    try
                    {
                        var chromeOptions = new ChromeOptions();
                        var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];
                        chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                        chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                        chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                        chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                        var driver1 = new ChromeDriver(chromeOptions);
                        driver1.Navigate().GoToUrl(driver.Url);
                        driver1.FindElement(By.Id("ctl00_mainContentPlaceHolder_acceptButton")).Click();
                        Thread.Sleep(2000);
                        string fileName = "ValuationNotice_" + Parcel_number.Trim() + ".Pdf";
                        IWebElement Valuationdownload = driver1.FindElement(By.Id("ctl00_mainContentPlaceHolder_propertyHeader_valuationNoticeHyperLink"));
                        string valuation = Valuationdownload.GetAttribute("href");
                        Valuationdownload.Click();
                        Thread.Sleep(4000);
                        // driver1.SwitchTo().Window(driver.WindowHandles.Last());
                        gc.AutoDownloadFile(orderNumber, parcelNumber, "Sedgwick", "KS", fileName);

                        IWebElement Propertydetaildownload = driver1.FindElement(By.Id("ctl00_mainContentPlaceHolder_propertyHeader_prcHyperLink"));
                        string propertyhref = Propertydetaildownload.GetAttribute("href");
                        string Pinhref = GlobalClass.After(propertyhref, "AIN=").Trim();
                        string fileName1 = "PropertyRecord.pdf__" + Pinhref + ".Pdf";
                        Propertydetaildownload.Click();
                        Thread.Sleep(4000);
                        gc.AutoDownloadFile(orderNumber, parcelNumber, "Sedgwick", "KS", fileName1);
                        driver1.Quit();
                    }
                    catch { }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "KS", "Sedgwick", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    gc.mergpdf(orderNumber, "KS", "Sedgwick");
                    driver.Quit();
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