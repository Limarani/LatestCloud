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
    public class WebDriver_PimaAZ
    {
        Amrock amck = new Amrock();
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_AZPima(string streetNo, string direction, string streetName, string streetType, string unitNumber, string parcelNumber, string ownerName, string searchType, string orderNumber)
        {
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new PhantomJSDriver();
            //driver = new ChromeDriver();
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("http://www.asr.pima.gov/Home/ParcelSearch");
                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, "", "", streetNo + " " + streetName, "AZ", "Pima");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["AZPima_NoRecord"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (direction != "")
                    {
                        if (direction.ToUpper() == "S")
                        {
                            direction = "3";
                        }
                        if (direction.ToUpper() == "N")
                        {
                            direction = "1";
                        }
                        if (direction.ToUpper() == "W")
                        {
                            direction = "4";
                        }
                        if (direction.ToUpper() == "E")
                        {
                            direction = "2";
                        }
                    }
                    if (searchType == "address")
                    {
                        // 
                        try
                        {
                            driver.FindElement(By.XPath("/html/body/div[2]/div[3]/form/div[1]/div[4]/a")).Click();
                            Thread.Sleep(2000);
                        }
                        catch { }
                        try
                        {
                            //driver.FindElement(By.XPath("//*[@id='searchPills']/ul/li[3]/a")).Click();
                            //Thread.Sleep(2000);
                        }
                        catch { }
                        driver.FindElement(By.LinkText("Address")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.Id("address1")).SendKeys(streetNo);
                        driver.FindElement(By.Id("address2")).SendKeys(streetNo);
                        if (direction != "")
                        {
                            IWebElement PropertyInformation = driver.FindElement(By.Id("selectedDirection"));
                            SelectElement PropertyInformationSelect = new SelectElement(PropertyInformation);
                            PropertyInformationSelect.SelectByIndex(Convert.ToInt16(direction.Trim()));
                        }
                        if (streetType.ToUpper() == "AVE")
                        {
                            streetType = "AV";
                        }
                        if (streetType.ToUpper() == "TRL")
                        {
                            streetType = "TR";
                        }
                        if (streetType.ToUpper() == "TER")
                        {
                            streetType = "TE";
                        }
                        if (streetType.ToUpper() == "WAY")
                        {
                            streetType = "WY";
                        }
                        driver.FindElement(By.Id("strName")).SendKeys(streetName.Trim() + " " + streetType.Trim());
                        IWebElement addressliclick = driver.FindElement(By.XPath("//*[@id='searchPills']/div/div[3]/form/div/div/div[3]"));
                        IList<IWebElement> addressclickli = addressliclick.FindElements(By.TagName("li"));
                        IList<IWebElement> addressclicktd;
                        foreach (IWebElement addressclick in addressclickli)
                        {
                            addressclicktd = addressclick.FindElements(By.TagName("a"));
                            if (addressclicktd.Count != 0)
                            {
                                addressclicktd[0].Click();
                                Thread.Sleep(2000);
                            }
                        }
                        //driver.FindElement(By.Id("strName")).Click();
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "AZ", "Pima");
                        Thread.Sleep(8000);
                        try
                        {
                            IWebElement clickfirst = driver.FindElement(By.XPath("//*[@id='searchPills']/div/div[3]/form/div/div/div[3]/button"));
                            IJavaScriptExecutor js12 = driver as IJavaScriptExecutor;
                            js12.ExecuteScript("arguments[0].click();", clickfirst);
                            //Thread.Sleep(9000);
                            Thread.Sleep(3000);
                        }
                        catch { }
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='searchPills']/div/div[3]/form/div/div/div[3]/button")).SendKeys(Keys.Enter);
                            Thread.Sleep(2000);
                        }
                        catch { }
                        //IWebElement INorecord = driver.FindElement(By.Id("ext-gen9"));
                        //if (INorecord.Text.Contains("no records"))
                        //{
                        //    HttpContext.Current.Session["multiparcel_DesotoMS_NoRecord"] = "Yes";
                        //}
                        //gc.CreatePdf_WOP(orderNumber, "Address search Result", driver, "AZ", "Pima");
                        //try
                        //{
                        //    string multi = GlobalClass.After(driver.FindElement(By.Id("ext-comp-1008")).Text, "of").Trim();
                        //    string strAddress = "", strparcel = "", strOwner = "";
                        //    if (Convert.ToInt32(multi) <= 25)
                        //    {
                        //        IWebElement tbmulti = driver.FindElement(By.Id("ext-gen16"));
                        //        IList<IWebElement> TBmulti = tbmulti.FindElements(By.TagName("table"));
                        //        IList<IWebElement> TRmulti;
                        //        IList<IWebElement> TDmulti;
                        //        foreach (IWebElement row in TBmulti)
                        //        {
                        //            TDmulti = row.FindElements(By.TagName("td"));
                        //            if (TDmulti.Count != 0 && TDmulti[0].Text.Trim() != "")
                        //            {
                        //                strAddress = TDmulti[3].Text;
                        //                strparcel = TDmulti[1].Text;
                        //                strOwner = TDmulti[0].Text;

                        //                string multiDetails = strAddress + "~" + strOwner;
                        //                gc.insert_date(orderNumber, strparcel, 1351, multiDetails, 1, DateTime.Now);
                        //            }
                        //        }
                        //        gc.CreatePdf_WOP(orderNumber, "Multi Address search Result", driver, "AZ", "Pima");
                        //        HttpContext.Current.Session["multiparcel_DesotoMS"] = "Yes";
                        //        driver.Quit();
                        //        gc.mergpdf(orderNumber, "AZ", "Pima");
                        //        return "MultiParcel";
                        //    }

                        //    if (Convert.ToInt32(multi) > 25)
                        //    {
                        //        HttpContext.Current.Session["multiparcel_DesotoMS_Multicount"] = "Maximum";
                        //        driver.Quit();
                        //        return "Maximum";
                        //    }
                        //}
                        //catch { }
                        try
                        {
                            IWebElement INorecord = driver.FindElement(By.XPath("//*[@id='searchPills']/div/div[3]/form/div/div/div[3]/label[2]"));
                            if (INorecord.Text.Contains("No information found"))
                            {
                                HttpContext.Current.Session["AZPima_NoRecord"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }

                    }
                    if (searchType == "parcel")
                    {
                        parcelNumber = parcelNumber.Replace("-", "").Trim();
                        string parcel1 = parcelNumber.Substring(0, 3);
                        string parcel2 = parcelNumber.Substring(3, 2);
                        string parcel3 = parcelNumber.Substring(5, 4);
                        parcelNumber = parcel1 + "-" + parcel2 + "-" + parcel3;
                        driver.FindElement(By.Id("parcelInput")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search", driver, "AZ", "Pima");
                        //  /html/body/div[2]/div[1]/div[1]/div/div[1]/div/div[1]/button
                        driver.FindElement(By.XPath("/html/body/div[2]/div[3]/form/div[1]/div[1]/div/button")).SendKeys(Keys.Enter);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search Result", driver, "AZ", "Pima");
                    }
                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.LinkText("Quick")).SendKeys(Keys.Enter);
                        IWebElement IOwner = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div[1]/div"));
                        IList<IWebElement> IOwnerRow = IOwner.FindElements(By.TagName("label"));
                        foreach (IWebElement owner in IOwnerRow)
                        {
                            if (owner.Text == "Property Owner")
                            {
                                owner.Click();
                                Thread.Sleep(3000);
                                break;
                            }
                        }
                        driver.FindElement(By.Id("taxPayerInput")).SendKeys(ownerName);
                        gc.CreatePdf_WOP(orderNumber, "Owner search", driver, "AZ", "Pima");
                        driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div[1]/div/div[1]/div/div[2]/button")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Owner search Result", driver, "AZ", "Pima");
                        try
                        {
                            IWebElement MultiCount = driver.FindElement(By.XPath("//*[@id='searchResultsModal']/div/div/div[1]"));
                            string strCount = gc.Between(MultiCount.Text, "(", ")").Trim();
                            if (Convert.ToInt32(strCount) <= 25)
                            {
                                IWebElement IMultiOwner = driver.FindElement(By.XPath("//*[@id='searchResultsModal']/div/div/div[2]/div[2]/table/tbody"));
                                IList<IWebElement> IMultiOwnerRow = IMultiOwner.FindElements(By.TagName("tr"));
                                IList<IWebElement> IMultiOwnerTD;
                                foreach (IWebElement multi in IMultiOwnerRow)
                                {
                                    IMultiOwnerTD = multi.FindElements(By.TagName("td"));
                                    if (IMultiOwnerTD.Count != 0)
                                    {
                                        string strParcel = IMultiOwnerTD[0].Text;
                                        string strOwner = IMultiOwnerTD[1].Text;
                                        string strAddress = IMultiOwnerTD[2].Text;

                                        string multiDetails = strAddress + "~" + strOwner;
                                        gc.insert_date(orderNumber, strParcel, 1366, multiDetails, 1, DateTime.Now);
                                    }
                                }
                                gc.CreatePdf_WOP(orderNumber, "Multi Address search Result", driver, "AZ", "Pima");
                                HttpContext.Current.Session["multiparcel_PimaAZ"] = "Yes";
                                driver.Quit();
                                gc.mergpdf(orderNumber, "AZ", "Pima");
                                return "MultiParcel";
                            }
                            if (Convert.ToInt32(strCount) > 25)
                            {
                                HttpContext.Current.Session["multiparcel_PimaAZ_Multicount"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }
                    }

                    try
                    {
                        IWebElement INorecord = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div[1]/div/div[1]/div/div[1]/label[2]"));
                        if (INorecord.Text.Contains("No records found") || INorecord.Text.Contains("No information found"))
                        {
                            HttpContext.Current.Session["AZPima_NoRecord"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    try
                    {
                        IWebElement INoinform = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div[1]/div/div[1]/div/div[2]/label[2]"));
                        if (INoinform.Text.Contains("No records found") || INoinform.Text.Contains("No information found"))
                        {
                            HttpContext.Current.Session["AZPima_NoRecord"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    string TaxYear = "", TaxArea = "", PropertyAddress = "", strOwnerName = "", ProDescription = "", YearBuilt = "";
                    //Property Details
                    Thread.Sleep(5000);
                    IWebElement IParcelNo = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/p"));
                    parcelNumber = gc.Between(IParcelNo.Text, "Parcel Number:", "Expand All +").Trim();
                    gc.CreatePdf(orderNumber, parcelNumber, "Parcel search Result", driver, "AZ", "Pima");
                    if (IParcelNo.Text.Contains("Expand All"))
                    {
                        // /html/body/div[2]/div[1]/p/span[1]
                        driver.FindElement(By.XPath("/html/body/div[2]/div[1]/p/span[1]")).Click();
                        Thread.Sleep(3000);
                    }
                    try
                    {
                        gc.CreatePdf(orderNumber, parcelNumber, "Property Details", driver, "AZ", "Pima");
                    }
                    catch { }
                    TaxYear = DateTime.Now.Year.ToString();
                    var year = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div[1]/div/div[1]/div[1]/div[1]/table/tbody/tr[2]/td[2]/div/select"));
                    var selectElement1 = new SelectElement(year);
                    selectElement1.SelectByText(TaxYear);

                    //IWebElement Iyear = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div[1]/div/div[1]/div[1]/div[1]/table/tbody/tr[2]/td[2]/div/select"));
                    //SelectElement SYear = new SelectElement(Iyear);
                    //TaxYear = SYear.SelectedOption.Text;
                    TaxArea = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div[1]/div/div[1]/div[1]/div[2]/table/tbody/tr[2]/td[2]/button")).Text;
                    PropertyAddress = GlobalClass.After(driver.FindElement(By.Id("PrclAddr")).Text, "Location").Replace("\r\n", "").Trim();
                    string City = "";
                    IWebElement IFullAddress = driver.FindElement(By.XPath("//*[@id='PrclAddr']/table/tbody"));
                    IList<IWebElement> IFullRow = IFullAddress.FindElements(By.TagName("tr"));
                    IList<IWebElement> IFullTD;
                    foreach (IWebElement full in IFullRow)
                    {
                        IFullTD = full.FindElements(By.TagName("td"));
                        if (IFullTD.Count != 0 && full.Text.Trim() != "" && IFullTD.Count == 4)
                        {
                            City = IFullTD[3].Text.Trim();
                        }
                    }
                    IWebElement IOwnerName = driver.FindElement(By.XPath("//*[@id='TaxpyrLegal']/table/tbody"));
                    IList<IWebElement> IOwnerNameRow = IOwnerName.FindElements(By.TagName("tr"));
                    IList<IWebElement> IOwnerNameTD;
                    foreach (IWebElement name in IOwnerNameRow)
                    {
                        IOwnerNameTD = name.FindElements(By.TagName("td"));
                        if (IOwnerNameTD.Count != 0)
                        {
                            strOwnerName = IOwnerNameTD[0].Text.Replace("\r\n", " ");
                            ProDescription = IOwnerNameTD[1].Text.Replace("\r\n", " ");
                        }
                    }
                    //Valuation Details
                    IWebElement IValuation = driver.FindElement(By.XPath("//*[@id='ValuData']/table/tbody"));
                    IList<IWebElement> IValuationRow = IValuation.FindElements(By.TagName("tr"));
                    IList<IWebElement> IValuationTD;
                    foreach (IWebElement valuation in IValuationRow)
                    {
                        IValuationTD = valuation.FindElements(By.TagName("td"));
                        if (IValuationTD.Count != 0)
                        {
                            string ValYear = IValuationTD[0].Text;
                            string ValPropertyClass = IValuationTD[1].Text;
                            string ValAssessRatio = IValuationTD[2].Text;
                            string ValTotalFCV = IValuationTD[5].Text;
                            string ValLimitedValue = IValuationTD[6].Text;
                            string ValLimitedAssess = IValuationTD[7].Text;

                            string valuationDetails = ValYear + "~" + ValPropertyClass + "~" + ValAssessRatio + "~" + ValTotalFCV + "~" + ValLimitedValue + "~" + ValLimitedAssess;
                            gc.insert_date(orderNumber, parcelNumber, 1368, valuationDetails, 1, DateTime.Now);
                        }
                    }
                    //Year Built
                    IWebElement IYearBuilt = driver.FindElement(By.XPath("//*[@id='ResdChr']/table[1]/tbody"));
                    IList<IWebElement> IYearBuiltRow = IYearBuilt.FindElements(By.TagName("tr"));
                    IList<IWebElement> IYearBuiltTD;
                    foreach (IWebElement yearBuilt in IYearBuiltRow)
                    {
                        IYearBuiltTD = yearBuilt.FindElements(By.TagName("td"));
                        if (yearBuilt.Text.Contains("Effective Construction Year:"))
                        {
                            YearBuilt = IYearBuiltTD[1].Text;
                            break;
                        }
                    }

                    string propertyDetails = PropertyAddress + "~" + strOwnerName + "~" + ProDescription + "~" + TaxYear + "~" + TaxArea + "~" + YearBuilt;
                    gc.insert_date(orderNumber, parcelNumber, 1367, propertyDetails, 1, DateTime.Now);

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    //Tax Authority
                    //driver.Navigate().GoToUrl("http://www.to.pima.gov/about/contactus");
                    //IWebElement ITaxAuthor = driver.FindElement(By.XPath("//*[@id='rt-mainbody']/div/div/div/table[1]"));
                    string strTaxAuthor = "";
                    //gc.CreatePdf(orderNumber, parcelNumber, "Tax Authority Details", driver, "AZ", "Pima");

                    //Tax Details
                    int Month = DateTime.Now.Month;
                    int yearcount = DateTime.Now.Year;
                    if (Month < 9)
                    {
                        yearcount--;
                    }
                    for (int i = 0; i <= 2; i++)
                    {
                        try
                        {
                            driver.Navigate().GoToUrl("http://www.to.pima.gov/property-information/property-inquiry");
                            IWebElement ITaxframe = driver.FindElement(By.Id("blockrandom"));
                            driver.SwitchTo().Frame(ITaxframe);
                            driver.FindElement(By.XPath("//*[@id='content']/center/form/table[1]/tbody/tr[1]/td/input")).SendKeys(parcelNumber.Replace("-", "").Trim());
                            IWebElement IYearselect = driver.FindElement(By.XPath("//*[@id='content']/center/form/table[1]/tbody/tr[2]/td/select"));
                            SelectElement selectYear = new SelectElement(IYearselect);
                            selectYear.SelectByText(yearcount.ToString());
                            gc.CreatePdf(orderNumber, parcelNumber, "Tax Search Details" + i, driver, "AZ", "Pima");
                            driver.FindElement(By.XPath("//*[@id='content']/center/form/table[1]/tbody/tr[4]/td/input")).SendKeys(Keys.Enter);
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Tax Search Result Details" + i, driver, "AZ", "Pima");


                            IWebElement ITaxAssess = driver.FindElement(By.XPath("//*[@id='content']/center[1]/table/tbody/tr[1]/td/table"));
                            string StateCode = gc.Between(ITaxAssess.Text, "STATE CODE:", "TAX YEAR:");
                            amck.TaxId = StateCode;
                            string StrTaxYear = gc.Between(ITaxAssess.Text, "TAX YEAR:", "TOTAL TAX:");
                            amck.TaxYear = StrTaxYear;
                            string StrTotalTax = "", basetax = "";
                            StrTotalTax = gc.Between(ITaxAssess.Text, "TOTAL TAX:", "AS OF DATE:");

                            string StrAsOfDate = gc.Between(ITaxAssess.Text, "AS OF DATE:", "TRC NO:");
                            IWebElement ITaxOwner = driver.FindElement(By.XPath("//*[@id='content']/center[1]/table/tbody/tr[2]/td[3]/table"));
                            string StrTaxOwner = gc.Between(ITaxOwner.Text.Replace("\r\n", " "), "TAXPAYER NAME/ADDRESS", "PROPERTY ADDRESS");
                            string StrTaxAddress = gc.Between(ITaxOwner.Text, "PROPERTY ADDRESS", "LEGAL DESCRIPTION");
                            string StrTaxOwnerName = StrTaxOwner;
                            string StrTaxLegal = gc.Between(ITaxOwner.Text, "LEGAL DESCRIPTION", "PAID BY");
                            string StrTaxPaid = gc.Between(ITaxOwner.Text, "PAID BY", "ON BEHALF OF");
                            string StrTaxBehalf = GlobalClass.After(ITaxOwner.Text, "ON BEHALF OF").Replace("\r\n", "");

                            string installTitle = "", firstInstalldel = "", secondInstalldel = "", firstInstallRem = "", secondInstallRem = "", firstInstall = "", secondInstall = "", totalInstall = "", firstInstalltaxpaid = "", secondInstalltaxpaid = "";
                            int installcount = 0;
                            IWebElement ITaxInstall = driver.FindElement(By.XPath("//*[@id='content']/center[1]/table/tbody/tr[2]/td[1]/table[1]"));
                            IList<IWebElement> ITaxInstallRow = ITaxInstall.FindElements(By.TagName("tr"));
                            IList<IWebElement> ITaxInstallTD;
                            IList<IWebElement> ITaxInstallTH;
                            foreach (IWebElement install in ITaxInstallRow)
                            {
                                ITaxInstallTD = install.FindElements(By.TagName("td"));
                                ITaxInstallTH = install.FindElements(By.TagName("th"));
                                if (ITaxInstallTD.Count != 0 && ITaxInstallTH.Count != 0 && !install.Text.Contains("INSTALLMENT") && !install.Text.Contains("CURRENT STATUS AND SUMMARY") && install.Text.Trim() != "")
                                {

                                    if (ITaxInstallTH.Count == 1 && !install.Text.Contains("TOTAL DUE:"))
                                    {
                                        string Currentstatus = yearcount + "~" + ITaxInstallTH[0].Text + "~" + ITaxInstallTD[0].Text + "~" + ITaxInstallTD[1].Text + "~" + ITaxInstallTD[2].Text;
                                        if (ITaxInstallTH[0].Text == "TAX DUE:")
                                        {
                                            firstInstall = ITaxInstallTD[0].Text;
                                            secondInstall = ITaxInstallTD[1].Text;
                                            totalInstall = ITaxInstallTD[2].Text;
                                            if (firstInstall != "")
                                            {
                                                amck.Instamount1 = firstInstall;
                                            }
                                            if (secondInstall != "")
                                            {
                                                amck.Instamount2 = secondInstall;
                                            }
                                        }
                                        if (ITaxInstallTH[0].Text == "TAX PAID:")
                                        {
                                            firstInstalltaxpaid = ITaxInstallTD[0].Text;
                                            secondInstalltaxpaid = ITaxInstallTD[1].Text;

                                            if (firstInstalltaxpaid != "")
                                            {
                                                amck.Instamountpaid1 = firstInstalltaxpaid.Replace("(", "").Replace(")", ""); ;
                                            }
                                            if (secondInstalltaxpaid != "")
                                            {
                                                amck.Instamountpaid2 = secondInstalltaxpaid.Replace("(", "").Replace(")", ""); ;
                                            }
                                        }
                                        if (ITaxInstallTH[0].Text == "REMAINING AMOUNT:")
                                        {
                                            firstInstallRem = ITaxInstallTD[0].Text;
                                            secondInstallRem = ITaxInstallTD[1].Text;

                                            if (firstInstallRem == "$0.00")
                                            {
                                                amck.InstPaidDue1 = "Paid";
                                            }
                                            else
                                            {
                                                amck.InstPaidDue1 = "Due";
                                            }
                                            if (secondInstallRem == "$0.00")
                                            {
                                                amck.InstPaidDue2 = "Paid";
                                            }
                                            else
                                            {
                                                amck.InstPaidDue2 = "Due";
                                            }
                                        }
                                        if (ITaxInstallTH[0].Text == "INTEREST DUE:")
                                        {
                                            firstInstalldel = ITaxInstallTD[0].Text;
                                            secondInstalldel = ITaxInstallTD[1].Text;

                                            if (firstInstalldel != "")
                                            {
                                                amck.IsDelinquent = "Yes";
                                            }
                                            else
                                            {
                                                amck.IsDelinquent = "No";
                                            }
                                            if (secondInstalldel != "")
                                            {
                                                amck.IsDelinquent = "Yes";
                                            }
                                            else
                                            {
                                                amck.IsDelinquent = "No";
                                            }

                                        }
                                        gc.insert_date(orderNumber, parcelNumber, 1370, Currentstatus, 1, DateTime.Now);
                                    }
                                    if (install.Text.Contains("TOTAL DUE:"))
                                    {
                                        string Currentstatus = yearcount + "~" + ITaxInstallTH[0].Text + "~" + " " + "~" + " " + "~" + ITaxInstallTD[0].Text;
                                        gc.insert_date(orderNumber, parcelNumber, 1370, Currentstatus, 1, DateTime.Now);
                                    }
                                }
                            }
                            if (i == 0)
                            {
                                    if(amck.IsDelinquent != "Yes")
                            {
                        gc.InsertAmrockTax(orderNumber, amck.TaxId, amck.Instamount1, amck.Instamount2, amck.Instamount3, amck.Instamount4, amck.Instamountpaid1, amck.Instamountpaid2, amck.Instamountpaid3, amck.Instamountpaid4, amck.InstPaidDue1, amck.InstPaidDue2, amck.instPaidDue3, amck.instPaidDue4, amck.IsDelinquent);
                           }
                            else
                            {
                        gc.InsertAmrockTax(orderNumber, amck.TaxId, null, null, null, null, null, null, null, null, null, null, null, null, amck.IsDelinquent);

                               }
                    }
                            string HistoryYear = "", HistoryAmount = "";
                            IWebElement ITaxHistory = driver.FindElement(By.XPath("//*[@id='content']/center[1]/table/tbody/tr[2]/td[1]/table[2]/tbody"));
                            IList<IWebElement> ITaxHistoryRow = ITaxHistory.FindElements(By.TagName("tr"));
                            IList<IWebElement> ITaxHistoryTD;
                            IList<IWebElement> ITaxHistoryTH;
                            foreach (IWebElement history in ITaxHistoryRow)
                            {
                                ITaxHistoryTD = history.FindElements(By.TagName("td"));
                                ITaxHistoryTH = history.FindElements(By.TagName("th"));
                                if (ITaxHistoryTD.Count != 0 && ITaxHistoryTH.Count != 0)
                                {
                                    HistoryYear = ITaxHistoryTH[0].Text.Replace(":", "");
                                    HistoryAmount = ITaxHistoryTD[0].Text;

                                    string historyDetails = HistoryYear + "~" + HistoryAmount;
                                    gc.insert_date(orderNumber, parcelNumber, 1371, historyDetails, 1, DateTime.Now);
                                }
                            }

       
                          
                                driver.Navigate().GoToUrl("http://www.to.pima.gov/tax-information/tax-statement");
                            IWebElement ITaxStateframe = driver.FindElement(By.Id("blockrandom"));
                            driver.SwitchTo().Frame(ITaxStateframe);
                            driver.FindElement(By.XPath("//*[@id='content']/center/form/table/tbody/tr[1]/td/input")).SendKeys(parcelNumber.Replace("-", "").Trim());
                            IWebElement IstateYear = driver.FindElement(By.XPath("//*[@id='content']/center/form/table/tbody/tr[2]/td/select"));
                            SelectElement stateYear = new SelectElement(IstateYear);
                            stateYear.SelectByText(yearcount.ToString());
                            gc.CreatePdf(orderNumber, parcelNumber, "Tax Statement Search Details" + i, driver, "AZ", "Pima");
                            driver.FindElement(By.XPath("//*[@id='content']/center/form/table/tbody/tr[3]/td/input")).SendKeys(Keys.Enter);
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Tax Statement Result Details" + i, driver, "AZ", "Pima");
                            strTaxAuthor = driver.FindElement(By.XPath("//*[@id='statement']/center[1]/table[5]/tbody/tr/td[3]/table/tbody/tr[2]/td[1]")).Text;
                            string TaxDetails = StateCode + "~" + StrTaxYear + "~" + StrTotalTax + "~" + StrAsOfDate + "~" + StrTaxAddress + "~" + StrTaxOwnerName + "~" + StrTaxLegal + "~" + StrTaxPaid + "~" + StrTaxBehalf + "~" + strTaxAuthor;
                            gc.insert_date(orderNumber, parcelNumber, 1369, TaxDetails, 1, DateTime.Now);
                            string TaxSumAmount = "", strTaxAuthority = "", TaxSumTitle = "";
                            IWebElement ITaxAuthority = driver.FindElement(By.XPath("//*[@id='statement']/center[1]/table[5]/tbody/tr/td[3]/table"));
                            strTaxAuthority = gc.Between(ITaxAuthority.Text.Replace("\r\n", " "), "mail to:", "PLEASE INCLUDE").Trim();
                            IWebElement ITaxesYear = driver.FindElement(By.XPath("//*[@id='statement']/table[3]/tbody/tr/td[2]/table[1]"));
                            string[] TaxesYear = gc.Between(ITaxesYear.Text, "JURISDICTION", "DIFFERENCE").Trim().Split(' ');
                            string firstYear = "", secondYear = "";
                            try
                            {
                                firstYear = TaxesYear[0]; secondYear = TaxesYear[1];
                            }
                            catch { }
                            IWebElement ITaxSummary = driver.FindElement(By.XPath("//*[@id='statement']/table[3]/tbody/tr/td[1]/table[4]"));
                            IList<IWebElement> ITaxSummaryRow = ITaxSummary.FindElements(By.TagName("table"));
                            IList<IWebElement> ITaxSummaryTD;
                            IList<IWebElement> ITaxSummaryTTD;
                            IList<IWebElement> ITaxSummaryTTH;
                            foreach (IWebElement summary in ITaxSummaryRow)
                            {
                                ITaxSummaryTD = summary.FindElements(By.TagName("tr"));
                                if (ITaxSummaryTD.Count != 0)
                                {
                                    foreach (IWebElement Taxsummary in ITaxSummaryTD)
                                    {
                                        ITaxSummaryTTD = Taxsummary.FindElements(By.TagName("td"));
                                        ITaxSummaryTTH = Taxsummary.FindElements(By.TagName("th"));
                                        if (ITaxSummaryTD.Count != 0 && ITaxSummaryTTH.Count != 0 && !Taxsummary.Text.Contains("TOTAL TAX DUE FOR"))
                                        {
                                            TaxSumAmount += ITaxSummaryTTD[0].Text + "~";
                                            TaxSumTitle += ITaxSummaryTTH[1].Text + "~";
                                        }
                                        if (ITaxSummaryTD.Count != 0 && ITaxSummaryTTH.Count != 0 && Taxsummary.Text.Contains("TOTAL TAX DUE FOR"))
                                        {
                                            TaxSumAmount += ITaxSummaryTTD[0].Text + "~";
                                            TaxSumTitle += GlobalClass.Before(ITaxSummaryTTH[0].Text, "FOR") + "~";
                                        }
                                    }
                                }
                            }

                            IWebElement IState = driver.FindElement(By.XPath("//*[@id='statement']/table[2]/tbody/tr/td[1]/table/tbody/tr/td/table"));
                            string strState = GlobalClass.After(IState.Text, "PARCEL").Replace("\r\n", "").Trim();

                            IWebElement ITaxSummry = driver.FindElement(By.XPath("//*[@id='statement']/table[3]/tbody/tr/td[1]/table[3]/tbody/tr/th/table"));
                            string strTaxSummry = GlobalClass.Before(ITaxSummry.Text, " TAX SUMMARY");

                            if (i == 0)
                            {
                                db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + "State Code" + "~" + "Tax Year" + "~" + TaxSumTitle.Remove(TaxSumTitle.Length - 1, 1) + "' where Id = '" + 1373 + "'");
                            }
                            gc.insert_date(orderNumber, parcelNumber, 1373, strState + "~" + strTaxSummry + "~" + TaxSumAmount.Remove(TaxSumAmount.Length - 1, 1), 1, DateTime.Now);

                            IWebElement IJuri = driver.FindElement(By.XPath("//*[@id='statement']/table[3]/tbody/tr/td[2]/table[2]"));
                            IList<IWebElement> IJuriRow = IJuri.FindElements(By.TagName("tr"));
                            IList<IWebElement> IJuriTd;
                            IList<IWebElement> IJuriTH;
                            foreach (IWebElement juri in IJuriRow)
                            {
                                IJuriTd = juri.FindElements(By.TagName("td"));
                                IJuriTH = juri.FindElements(By.TagName("th"));
                                if (IJuriTd.Count != 0 && juri.Text.Trim() != "" && !juri.Text.Trim().Contains("TOTALS"))
                                {
                                    string juriTitle = IJuriTd[0].Text;
                                    string juriFirst = IJuriTd[1].Text;
                                    string juriSecond = IJuriTd[2].Text;

                                    string summaryDetails = juriTitle + "~" + firstYear.Replace("TAXES", "").Trim() + "~" + juriFirst + "~" + secondYear.Replace("TAXES", "").Trim() + "~" + juriSecond;
                                    gc.insert_date(orderNumber, parcelNumber, 1374, summaryDetails, 1, DateTime.Now);
                                }
                                if (IJuriTd.Count != 0 && IJuriTH.Count != 0 && juri.Text.Trim() != "" && juri.Text.Trim().Contains("TOTALS"))
                                {
                                    string juriTitle = IJuriTH[0].Text;
                                    string juriFirst = IJuriTd[0].Text;
                                    string juriSecond = IJuriTd[1].Text;

                                    string summaryDetails = juriTitle + "~" + firstYear.Replace("TAXES", "").Trim() + "~" + juriFirst + "~" + secondYear.Replace("TAXES", "").Trim() + "~" + juriSecond;
                                    gc.insert_date(orderNumber, parcelNumber, 1374, summaryDetails, 1, DateTime.Now);
                                }
                            }

                        }
                        catch { }
                        yearcount--;
                    }

                    if (City.Contains("Tucson"))
                    {
                        try
                        {
                            //City of Tucson 
                            driver.Navigate().GoToUrl("https://www.tucsonaz.gov/dfastwebpublic/default.aspx");
                            Thread.Sleep(2000);
                            IWebElement Iselectparcel = driver.FindElement(By.Id("ctl00_MainContent_propertySearchDropDownList"));
                            SelectElement selectParcel = new SelectElement(Iselectparcel);
                            selectParcel.SelectByText("Property ID");
                            driver.FindElement(By.Id("ctl00_MainContent_propertySearchPhraseTextBox")).SendKeys(parcelNumber);
                            gc.CreatePdf(orderNumber, parcelNumber, "City Tax Search Details", driver, "AZ", "Pima");
                            driver.FindElement(By.Id("ctl00_MainContent_searchButton")).SendKeys(Keys.Enter);
                            gc.CreatePdf(orderNumber, parcelNumber, "City Tax Search Result Details", driver, "AZ", "Pima");
                            driver.FindElement(By.Id("ctl00_MainContent_propertySearchGridView_ctl02_HyperLink1")).SendKeys(Keys.Enter);
                            gc.CreatePdf(orderNumber, parcelNumber, "City Tax Details", driver, "AZ", "Pima");
                            string AccountID = driver.FindElement(By.Id("ctl00_MainContent_accountIdValueLabel")).Text;
                            string PropertyID = driver.FindElement(By.Id("ctl00_MainContent_propertyIdValueLabel")).Text;
                            string AssessmentID = driver.FindElement(By.Id("ctl00_MainContent_assessmentIdValueLabel")).Text;
                            string OwnerID = driver.FindElement(By.Id("ctl00_MainContent_Repeater1_ctl01_ownerValueLabel")).Text;
                            string AddressID = driver.FindElement(By.Id("ctl00_MainContent_Repeater1_ctl01_ownerAddress1ValueLabel")).Text;

                            string cityDetails = AccountID + "~" + AssessmentID + "~" + OwnerID + "~" + AddressID;
                            gc.insert_date(orderNumber, PropertyID, 1375, cityDetails, 1, DateTime.Now);

                            IWebElement ITransactionClick = driver.FindElement(By.Id("menu-bar"));
                            IList<IWebElement> ITransactionClickRow = ITransactionClick.FindElements(By.TagName("li"));
                            IList<IWebElement> ITransactionClickTD;
                            foreach (IWebElement transClick in ITransactionClickRow)
                            {
                                ITransactionClickTD = transClick.FindElements(By.TagName("a"));
                                if (transClick.Text.Contains("Transactions"))
                                {
                                    ITransactionClickTD[0].Click();
                                    break;
                                }
                            }

                            //Transactions
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Tax Transaction Details", driver, "AZ", "Pima");
                            IWebElement ITransaction = driver.FindElement(By.Id("ctl00_MainContent_GridView1"));
                            IList<IWebElement> ITransactionRow = ITransaction.FindElements(By.TagName("tr"));
                            IList<IWebElement> ITransactionTD;
                            foreach (IWebElement trans in ITransactionRow)
                            {
                                ITransactionTD = trans.FindElements(By.TagName("td"));
                                if (ITransactionTD.Count != 0 && !trans.Text.Contains("Effective Date"))
                                {
                                    string EffectiveDate = ITransactionTD[0].Text;
                                    string PostDate = ITransactionTD[1].Text;
                                    string InstallDate = ITransactionTD[2].Text;
                                    string Description = ITransactionTD[3].Text;
                                    string Charges = ITransactionTD[4].Text;
                                    string Credits = ITransactionTD[5].Text;
                                    string Balance = ITransactionTD[6].Text;
                                    string DB = ITransactionTD[7].Text;
                                    string DM = ITransactionTD[8].Text;

                                    string summaryDetails = EffectiveDate + "~" + PostDate + "~" + InstallDate + "~" + Description + "~" + Charges + "~" + Credits + "~" + Balance + "~" + DB + "~" + DM;
                                    gc.insert_date(orderNumber, PropertyID, 1376, summaryDetails, 1, DateTime.Now);
                                }
                            }

                            //Repayment Details
                            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                            IWebElement payment = driver.FindElement(By.LinkText("Repayment Schedule"));
                            Actions action = new Actions(driver);
                            action.MoveToElement(payment).Build().Perform();
                            payment.Click();
                            IWebElement ICurrent = driver.FindElement(By.LinkText("Current"));
                            ICurrent.Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Tax Repayment Schedule Details", driver, "AZ", "Pima");
                            IWebElement IRepaymentCurrent = driver.FindElement(By.Id("ctl00_MainContent_GridView1"));
                            IList<IWebElement> IRepaymentCurrentRow = IRepaymentCurrent.FindElements(By.TagName("tr"));
                            IList<IWebElement> IRepaymentCurrentTD;
                            foreach (IWebElement current in IRepaymentCurrentRow)
                            {
                                IRepaymentCurrentTD = current.FindElements(By.TagName("td"));
                                if (IRepaymentCurrentTD.Count != 0)
                                {
                                    string BillingDate = IRepaymentCurrentTD[0].Text;
                                    string Rate = IRepaymentCurrentTD[1].Text;
                                    string Balance = IRepaymentCurrentTD[2].Text;
                                    string Principal = IRepaymentCurrentTD[3].Text;
                                    string Interest = IRepaymentCurrentTD[4].Text;
                                    string Total = IRepaymentCurrentTD[5].Text;
                                    string Status = IRepaymentCurrentTD[6].Text;

                                    string repaymentDetails = BillingDate + "~" + Rate + "~" + Balance + "~" + Principal + "~" + Interest + "~" + Total + "~" + Status;
                                    gc.insert_date(orderNumber, PropertyID, 1377, repaymentDetails, 1, DateTime.Now);
                                }
                            }
                        }
                        catch { }
                    }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "AZ", "Pima", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    driver.Quit();
                    gc.mergpdf(orderNumber, "AZ", "Pima");
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