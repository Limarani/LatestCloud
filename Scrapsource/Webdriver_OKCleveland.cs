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
    public class Webdriver_OKCleveland
    {
        string Taxing = "", Entity = "", MillageRate = "", Amount = "";
        string DatePaid = "", Transaction = "", Receipt = "", Item = "", AmountPaid = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();

        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        public string FTP_Cleveland(string houseno, string sname, string direction, string account, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            string Address = houseno + " " + direction + " " + sname;
            if (Address.Trim() != "")
            { searchType = "address"; }
            if (account.Trim()!="")
            { searchType = "block"; }
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                string AlterNateID = "", PropertyAddress = "", owner = "";
                string[] stringSeparators1 = new string[] { "\r\n" };
                List<string> listurl = new List<string>();
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    if (searchType == "titleflex")
                    {
                        string address = houseno + " " + sname;
                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "OK", "Cleveland");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_OKCleveland"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("http://www.clevelandcountyassessor.us/Search.aspx");
                        Thread.Sleep(2000);

                        driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_btnDisclaimerAccept']")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_mnuSearchn3']/table/tbody/tr/td/a")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Address_tbAddressNumber']")).SendKeys(houseno);
                        //driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Address_tbAddressNumber']")).SendKeys(houseno);
                        driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Address_tbAddressStreet']")).SendKeys(sname);
                        // driver.FindElement(By.ClassName("form-control add-suite ng-pristine ng-valid")).SendKeys(account);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "OK", "Cleveland");

                        driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Address_btnSearchAddress']")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Address search Result", driver, "OK", "Cleveland");

                        try
                        {

                            string Muiti = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblNumberOfResults']")).Text;


                            if (Muiti != "1 Results")
                            {

                                IWebElement MultiOwnerTable = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_gvSearchResults']/tbody"));
                                IList<IWebElement> MultiOwnerRow = MultiOwnerTable.FindElements(By.TagName("tr"));
                                IList<IWebElement> MultiOwnerTD;

                                //string AlterNateID = "", PropertyAddress = "", LegalDescriptoin = "", YearBuilt = "";
                                foreach (IWebElement row1 in MultiOwnerRow)
                                {
                                    MultiOwnerTD = row1.FindElements(By.TagName("td"));
                                    if (MultiOwnerTD.Count != 0 && MultiOwnerTD.Count != 2 && MultiOwnerTD.Count != 1)
                                    {
                                        account = MultiOwnerTD[0].Text;
                                        parcelNumber = MultiOwnerTD[1].Text;
                                        ownername = MultiOwnerTD[2].Text;
                                        PropertyAddress = MultiOwnerTD[3].Text;


                                        string Multi = account + "~" + ownername + "~" + PropertyAddress;
                                        gc.insert_date(orderNumber, parcelNumber, 403, Multi, 1, DateTime.Now);

                                    }
                                }

                                HttpContext.Current.Session["multiParcel_Cleveland"] = "Yes";
                                if (MultiOwnerRow.Count > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_Cleveland_Multicount"] = "Maximum";
                                }
                                driver.Quit();
                                return "MultiParcel";

                            }
                        }
                        catch { }
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_gvSearchResults']/tbody/tr[2]/td[1]/a")).Click();
                        }
                        catch { }

                    }

                    else if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("http://www.clevelandcountyassessor.us/Search.aspx");
                        Thread.Sleep(2000);

                        driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_btnDisclaimerAccept']")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_mnuSearchn1']/table/tbody/tr/td/a")).Click();
                        Thread.Sleep(2000);

                        driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Parcel_tbParcelNumber']")).SendKeys(parcelNumber);

                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "OK", "Cleveland");

                        driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Parcel_btnSearchParcel']")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Result", driver, "OK", "Cleveland");

                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_gvSearchResults']/tbody/tr[2]/td[1]/a")).Click();
                        }
                        catch { }
                    }
                    else if (searchType == "block")
                    {
                        driver.Navigate().GoToUrl("http://www.clevelandcountyassessor.us/Search.aspx");
                        Thread.Sleep(2000);
                        driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_btnDisclaimerAccept']")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_mnuSearchn0']/table/tbody/tr/td/a")).Click();
                        Thread.Sleep(2000);

                        driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Account_tbAccount']")).SendKeys(account);

                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Unit search", driver, "OK", "Cleveland");

                        driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Account_btnSearchAccount']")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        //  gc.CreatePdf_WOP(orderNumber, "Address search Result", driver, "FL", "Miami Dade");
                        gc.CreatePdf_WOP(orderNumber, "Unit search Result", driver, "OK", "Cleveland");
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_gvSearchResults']/tbody/tr[2]/td[1]/a")).Click();
                        }
                        catch { }

                    }
                    else if (searchType == "ownername")
                    {
                        driver.Navigate().GoToUrl("http://www.clevelandcountyassessor.us/Search.aspx");
                        Thread.Sleep(2000);
                        driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_btnDisclaimerAccept']")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_mnuSearchn2']/table/tbody/tr/td/a")).Click();
                        Thread.Sleep(2000);
                        var split = ownername.Split(' ');
                        if(split.Count() == 2)
                        {
                            driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Owner_tbOwnerLastName']")).SendKeys(split[0]);
                            //driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Address_tbAddressNumber']")).SendKeys(houseno);
                            driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Owner_tbOwnerFirstName']")).SendKeys(split[1]);
                            // driver.FindElement(By.ClassName("form-control add-suite ng-pristine ng-valid")).SendKeys(account);
                        }
                        if (split.Count() == 1)
                        {
                            driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Owner_tbOwnerLastName']")).SendKeys(ownername);
                            //driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Address_tbAddressNumber']")).SendKeys(houseno);
                            //driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Owner_tbOwnerFirstName']")).SendKeys(split[1]);
                            // driver.FindElement(By.ClassName("form-control add-suite ng-pristine ng-valid")).SendKeys(account);
                        }

                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Owner search", driver, "OK", "Cleveland");

                        driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Owner_btnSearchOwner']")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Owner search Result", driver, "OK", "Cleveland");

                        try
                        {
                            string Muiti = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblNumberOfResults']")).Text;


                            if (Muiti != "1 Results")
                            {

                                IWebElement MultiOwnerTable = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_gvSearchResults']/tbody"));
                                IList<IWebElement> MultiOwnerRow = MultiOwnerTable.FindElements(By.TagName("tr"));
                                IList<IWebElement> MultiOwnerTD;

                                //string AlterNateID = "", PropertyAddress = "", LegalDescriptoin = "", YearBuilt = "";
                                foreach (IWebElement row1 in MultiOwnerRow)
                                {
                                    MultiOwnerTD = row1.FindElements(By.TagName("td"));
                                    if (MultiOwnerTD.Count != 0 && MultiOwnerTD.Count != 2 && MultiOwnerTD.Count != 1)
                                    {
                                        account = MultiOwnerTD[0].Text;
                                        parcelNumber = MultiOwnerTD[1].Text;
                                        ownername = MultiOwnerTD[2].Text;
                                        PropertyAddress = MultiOwnerTD[3].Text;


                                        string Multi = account + "~" + ownername + "~" + PropertyAddress;
                                        gc.insert_date(orderNumber, parcelNumber, 403, Multi, 1, DateTime.Now);

                                    }
                                }

                                HttpContext.Current.Session["multiParcel_Cleveland"] = "Yes";
                                if (MultiOwnerRow.Count > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_Cleveland_Multicount"] = "Maximum";
                                }
                                driver.Quit();
                                return "MultiParcel";
                            }
                        }
                        catch { }
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_gvSearchResults']/tbody/tr[2]/td[1]/a")).Click();
                        }
                        catch { }
                    }

                    Thread.Sleep(2000);

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.Id("ContentPlaceHolder1_lblNumberOfResults"));
                        if(INodata.Text.Contains("No results"))
                        {
                            HttpContext.Current.Session["Nodata_OKCleveland"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    string LegalDescription = "", AccountType = "", Subdivision = "", TaxDistrict = "", LegalAcreage = "", Address1 = "";

                    gc.CreatePdf(orderNumber, parcelNumber, "Property Deatil", driver, "OK", "Cleveland");
                    account = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Base_fvDataProfile_AccountLabel']")).Text;

                    parcelNumber = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Base_fvDataProfile_ParcelLabel']")).Text;
                    Address1 = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Base_fvDataProfile']/tbody/tr/td/table/tbody/tr[5]/td")).Text;
                    LegalDescription = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Base_fvDataLegal_LegalDescriptionLabel']")).Text;
                    AccountType = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Base_fvDataLegal_AccountTypeLabel']")).Text;
                    Subdivision = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Base_fvDataLegal']/tbody/tr/td/table/tbody/tr[3]/td[1]")).Text;
                    TaxDistrict = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Base_fvDataLegal_TaxDistrictLabel']")).Text;
                    LegalAcreage = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Base_fvDataLegal_AcreageLabel']")).Text;
                    ownername = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Base_fvDataProfile_OwnerLabel']")).Text;


                    string Property = account + "~" + Address1 + "~" + LegalDescription + "~" + AccountType + "~" + Subdivision + "~" + TaxDistrict + "~" + LegalAcreage + "~" + ownername;
                    gc.insert_date(orderNumber, parcelNumber, 404, Property, 1, DateTime.Now);

                    driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_mnuDatan1']/table/tbody/tr/td/a")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Assessment Deatil", driver, "OK", "Cleveland");

                    string LandType = "", Acreage = "", Value = "", Description = "", TotalAcres = "", TotalValue = "", MarketValue = "", TaxableValue = "", LandValue = "", GrossAssessedValue = "", Adjustments = "", NetAssessedValue = "", YearBuilt = "";

                    LandType = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Land_gvDataLand']/tbody/tr[2]/td[1]")).Text;
                    Acreage = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Land_gvDataLand']/tbody/tr[2]/td[2]")).Text;
                    Value = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Land_gvDataLand']/tbody/tr[2]/td[3]")).Text;
                    Description = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Land_gvDataLand']/tbody/tr[2]/td[4]")).Text;
                    TotalAcres = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Land_fvDataLandTotals_AcresTotalLabel']")).Text;
                    TotalValue = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Land_fvDataLandTotals_ValueTotalLabel']")).Text;

                    driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_mnuDatan2']/table/tbody/tr/td/a")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Value Info", driver, "OK", "Cleveland");
                    MarketValue = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Valuation_fvDataValuation_MarketValueLabel']")).Text;
                    TaxableValue = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Valuation_fvDataValuation_TaxableValueLabel']")).Text;
                    LandValue = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Valuation_fvDataValuation_LandValueLabel']")).Text;
                    GrossAssessedValue = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Valuation_fvDataValuation_GrossAssessedValueLabel']")).Text;
                    Adjustments = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Valuation_fvDataValuation_AdjustmentsLabel']")).Text;
                    NetAssessedValue = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Valuation_fvDataValuation_NetAssessedValueLabel']")).Text;
                    driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_mnuDatan6']/table/tbody/tr/td/a")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Year Built Info", driver, "OK", "Cleveland");
                    YearBuilt = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Improvements_fvDataImprovements_YearBuiltLabel']")).Text;

                    string Assess = LandType + "~" + Acreage + "~" + Value + "~" + Description + "~" + TotalAcres + "~" + TotalValue + "~" + MarketValue + "~" + TaxableValue + "~" + LandValue + "~" + GrossAssessedValue + "~" + Adjustments + "~" + NetAssessedValue + "~" + YearBuilt;

                    gc.insert_date(orderNumber, parcelNumber, 405, Assess, 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("http://ok-cleveland-treasurer.governmax.com/collectmax/collect30.asp");
                    Thread.Sleep(2000);

                    IWebElement iframeElement1 = driver.FindElement(By.XPath("/html/frameset/frame"));

                    //now use the switch command
                    driver.SwitchTo().Frame(iframeElement1);
                    driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[3]/td/center/a/img")).Click();
                    Thread.Sleep(7000);
                    driver.SwitchTo().DefaultContent();
                    IWebElement iframeElement2 = driver.FindElement(By.XPath("/html/frameset/frame"));

                    //now use the switch command
                    driver.SwitchTo().Frame(iframeElement2);

                    driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[3]/td/font/a")).Click();
                    Thread.Sleep(7000);

                    string AccountPass = account;
                    if (account.Contains("R0") && AccountPass.Count() == 8 && !account.Contains("R00") && !account.Contains("R000"))
                    {
                        AccountPass = AccountPass.Substring(2, 6);
                    }
                    if (account.Contains("R00") && AccountPass.Count() == 8 && !account.Contains("R000"))
                    {
                        AccountPass = AccountPass.Substring(3, 5);
                    }
                    if (account.Contains("R000") && AccountPass.Count() == 8)
                    {
                        AccountPass = AccountPass.Substring(4, 4);
                    }

                    driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[1]/td/table/tbody/tr/td/table/tbody/tr[2]/td/font/input")).SendKeys(AccountPass);
                    driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[4]/td/input")).Click();
                    Thread.Sleep(7000);
                    try
                    {
                        IWebElement Iparcel = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[1]/tbody/tr[2]/td/table/tbody"));
                        IList<IWebElement> IparcelRow = Iparcel.FindElements(By.TagName("tr"));
                        IList<IWebElement> IparcelTd;
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Multi Parcel", driver, "OK", "Cleveland");
                        foreach (IWebElement parcel in IparcelRow)
                        {
                            IparcelTd = parcel.FindElements(By.TagName("td"));
                            if (IparcelTd.Count != 0 && !parcel.Text.Contains(" Search Results ") && parcel.Text.Trim() != "")
                            {
                                try
                                {
                                    string strAccount = IparcelTd[0].Text;
                                    strAccount = GlobalClass.Before(strAccount, " - ");
                                    if (strAccount.Trim() != "" && (strAccount == AccountPass))
                                    {
                                        IWebElement IparcelClick = IparcelTd[0].FindElement(By.TagName("a"));
                                        string strParcelSearch = IparcelClick.GetAttribute("href");
                                        driver.Navigate().GoToUrl(strParcelSearch);
                                        break;
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                    catch { }
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Info", driver, "OK", "Cleveland");
                    string AccountNumber = "", TaxType = "", Taxyear = "", ProperAddress = "", MaillingAddress = "", GEONumber = "", AssessedValue = "", ExemptAmount = "", TaxAuthority = "";

                    try
                    {
                        AccountNumber = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[2]/td[1]/font/b")).Text;
                        TaxType = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[2]/td[2]/font/b")).Text;
                        Taxyear = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[2]/td[3]/font/b")).Text;
                        string strMaillAddress = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[3]/td/table/tbody/tr[1]/td[1]")).Text;
                        MaillingAddress = GlobalClass.After(strMaillAddress, "Mailing Address ");
                        ProperAddress = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[3]/td/table/tbody/tr[1]/td[2]/font[3]")).Text;
                        GEONumber = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[3]/td/table/tbody/tr[1]/td[2]/font[6]")).Text;
                        ExemptAmount = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[5]/td[2]/font/b")).Text;
                        AssessedValue = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[5]/td[1]/font/b")).Text;
                        string Taxable_Value = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[5]/td[3]")).Text;
                        TaxAuthority = "Cleveland County Treasurer Mailing Address: 201 S Jones Ave, Suite 100 Norman, Ok 73069 please call (405) 366-0217 ";


                        string Tax = TaxType + "~" + Taxyear + "~" + ProperAddress + "~" + MaillingAddress + "~" + GEONumber + "~" + ExemptAmount + "~" + AssessedValue + "~" + Taxable_Value + "~" + TaxAuthority;

                        gc.insert_date(orderNumber, AccountNumber, 406, Tax, 1, DateTime.Now);




                        IWebElement TaxDisTB = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[8]/td/table[1]/tbody"));
                        IList<IWebElement> TaxDisTR = TaxDisTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> TaxDisTD;
                        foreach (IWebElement row1 in TaxDisTR)
                        {

                            TaxDisTD = row1.FindElements(By.TagName("td"));
                            if (!row1.Text.Contains("Taxing Entity "))
                            {
                                if (TaxDisTD.Count != 0 && TaxDisTD.Count != 1)
                                {
                                    if (TaxDisTD.Count == 3)
                                    {
                                        Taxing = TaxDisTD[0].Text;
                                        if (Taxing == "")
                                        {
                                            Taxing = "";
                                        }
                                        Entity = TaxDisTD[1].Text;
                                        MillageRate = "";
                                        Amount = "";

                                    }
                                    else if (TaxDisTD.Count == 4)
                                    {
                                        Taxing = TaxDisTD[0].Text;
                                        if (Taxing == "")
                                        {
                                            Taxing = "";
                                        }
                                        Entity = TaxDisTD[1].Text;
                                        MillageRate = TaxDisTD[2].Text;
                                        Amount = TaxDisTD[3].Text;
                                    }


                                    string Adva = Taxing + "~" + Entity + "~" + MillageRate + "~" + Amount;
                                    gc.insert_date(orderNumber, AccountNumber, 407, Adva, 1, DateTime.Now);
                                }
                            }

                        }

                        try
                        {
                            string strMillageRate = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[8]/td/table[2]/tbody/tr/td[2]")).Text;
                            string strAmount = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[8]/td/table[2]/tbody/tr/td[4]")).Text;
                            if (strMillageRate.Trim() != "" || strAmount.Trim().Contains("$"))
                            {
                                string Advalorem = "" + "~" + "Total" + "~" + strMillageRate + "~" + strAmount;
                                gc.insert_date(orderNumber, AccountNumber, 407, Advalorem, 1, DateTime.Now);
                            }
                        }
                        catch { }
                    }
                    catch
                    { }

                    try
                    {
                        IWebElement TaxHisTB = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[4]/tbody"));
                        IList<IWebElement> TaxHisTR = TaxHisTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> TaxHisTD;
                        foreach (IWebElement row1 in TaxHisTR)
                        {

                            TaxHisTD = row1.FindElements(By.TagName("td"));
                            if (!row1.Text.Contains("Transaction "))
                            {
                                if (TaxHisTD.Count != 0)
                                {
                                    DatePaid = TaxHisTD[0].Text;
                                    Transaction = TaxHisTD[1].Text;
                                    Receipt = TaxHisTD[2].Text;
                                    Item = TaxHisTD[3].Text;
                                    AmountPaid = TaxHisTD[4].Text;

                                    string TaxStatus = DatePaid + "~" + Transaction + "~" + Receipt + "~" + Item + "~" + AmountPaid;
                                    gc.insert_date(orderNumber, AccountNumber, 409, TaxStatus, 1, DateTime.Now);
                                }
                            }
                        }
                    }
                    catch { }
                    try
                    {
                        IWebElement TaxHisTBD = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[4]/tbody/tr[2]/td/table[1]/tbody"));
                        IList<IWebElement> TaxHisTRD = TaxHisTBD.FindElements(By.TagName("tr"));
                        IList<IWebElement> TaxHisTDD;
                        string Status = "", Cert = "", CertYr = "", CurrentPriorYearsTotal = "", IfPaidBy = "", Taxes = "", Year1 = "", Folio1 = "";
                        foreach (IWebElement row1 in TaxHisTRD)
                        {
                            TaxHisTDD = row1.FindElements(By.TagName("td"));
                            if (!row1.Text.Contains("Year"))
                            {
                                if (TaxHisTDD.Count != 0 && TaxHisTDD.Count != 2 && TaxHisTDD.Count != 1)
                                {
                                    Year1 = TaxHisTDD[0].Text;
                                    Folio1 = TaxHisTDD[1].Text;
                                    Status = TaxHisTDD[2].Text;
                                    Cert = TaxHisTDD[3].Text;
                                    CertYr = TaxHisTDD[4].Text;
                                    Amount = TaxHisTDD[5].Text;
                                    CurrentPriorYearsTotal = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[4]/tbody/tr[2]/td/table[3]/tbody/tr/td[2]/font")).Text;
                                }
                            }
                        }
                        TaxHisTBD = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[5]/tbody"));
                        TaxHisTRD = TaxHisTBD.FindElements(By.TagName("tr"));
                        IList<IWebElement> TaxHisTDD1;
                        foreach (IWebElement row1 in TaxHisTRD)
                        {
                            TaxHisTDD1 = row1.FindElements(By.TagName("td"));
                            if (!row1.Text.Contains("If Paid By "))
                            {
                                if (TaxHisTDD1.Count != 0)
                                {
                                    IfPaidBy = TaxHisTDD1[0].Text;
                                    Taxes = TaxHisTDD1[1].Text;
                                }
                            }
                        }

                        string Deliq = Year1 + "~" + Folio1 + "~" + Status + "~" + Cert + "~" + CertYr + "~" + Amount + "~" + CurrentPriorYearsTotal + "~" + IfPaidBy + "~" + Taxes;
                        if (IfPaidBy.Trim() != "")
                        {
                            gc.insert_date(orderNumber, AccountNumber, 412, Deliq, 1, DateTime.Now);
                            //TaxType = ""; Taxyear = ""; MaillingAddress = ""; GEONumber = ""; AssessedValue = ""; ExemptAmount = ""; TaxAuthority = "";
                        }
                    }
                    catch { }

                    try { driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[6]/tbody/tr/td/font/a")).Click(); }
                    catch { }
                    try { driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[7]/tbody/tr/td/font/a")).Click(); }
                    catch { }
                    try
                    {
                        IWebElement Ipayment = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[6]/td/font/a"));
                        string strPayment = Ipayment.Text;
                        if (strPayment.Trim().Contains("Payment History"))
                        {
                            Ipayment.Click();
                        }
                    }
                    catch { }
                    try
                    {
                        IWebElement Ipaymenthistory = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[7]/td/font/a"));
                        string strPaymentHistory = Ipaymenthistory.Text;
                        if (strPaymentHistory.Trim().Contains("Payment History"))
                        {
                            Ipaymenthistory.Click();
                        }
                    }
                    catch { }
                    Thread.Sleep(7000);
                    int K = 0;
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Info Detail", driver, "OK", "Cleveland");
                    string TaxPaymentType = "", TaxPaymentYear = "";
                    try
                    {
                        TaxPaymentType = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table[2]/tbody/tr[2]/td[2]/font/b")).Text;
                        TaxPaymentYear = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table[2]/tbody/tr[2]/td[3]/font/b")).Text;
                        //gc.CreatePdf(orderNumber, parcelNumber, "Tax Payment History" + TaxPaymentYear, driver, "OK", "Cleveland");
                        for (int i = 3; i < 25; i++)
                        {
                            try
                            {
                                IWebElement TaxHisTB1 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table[" + i + "]/tbody"));
                                IList<IWebElement> TaxHisTR1 = TaxHisTB1.FindElements(By.TagName("tr"));
                                IList<IWebElement> TaxHisTD1;
                                string Year = "", Folio = "", AmountBilled = "", PaidBy = "";
                                foreach (IWebElement row1 in TaxHisTR1)
                                {
                                    TaxHisTD1 = row1.FindElements(By.TagName("td"));
                                    if (TaxHisTD1.Count != 0 && !row1.Text.Contains("Payment History") && !row1.Text.Contains("Year ") && !row1.Text.Contains("Owner Name ") && !row1.Text.Contains("Paid By "))
                                    {
                                        Year = TaxHisTD1[0].Text;
                                        Folio = TaxHisTD1[1].Text;
                                        DatePaid = TaxHisTD1[2].Text;
                                        Receipt = TaxHisTD1[3].Text;
                                        AmountBilled = TaxHisTD1[4].Text;
                                        AmountPaid = TaxHisTD1[5].Text;
                                    }
                                    else if (TaxHisTD1.Count != 0 && !row1.Text.Contains("Payment History") && row1.Text.Contains("Owner Name ") && !row1.Text.Contains("Year ") && !row1.Text.Contains("Paid By "))
                                    {
                                        ownername = TaxHisTD1[1].Text;
                                    }
                                    else if (TaxHisTD1.Count != 0 && !row1.Text.Contains("Payment History") && row1.Text.Contains("Paid By ") && !row1.Text.Contains("Year ") && !row1.Text.Contains("Owner Name "))
                                    {
                                        PaidBy = TaxHisTD1[1].Text;
                                    }
                                }

                                string PayHistory = TaxType + "~" + Year + "~" + Folio + "~" + DatePaid + "~" + Receipt + "~" + AmountBilled + "~" + AmountPaid + "~" + ownername + "~" + PaidBy;
                                gc.insert_date(orderNumber, AccountNumber, 410, PayHistory, 1, DateTime.Now);
                                Taxyear = ""; MaillingAddress = ""; GEONumber = ""; AssessedValue = ""; ExemptAmount = ""; TaxAuthority = ""; Year = ""; Folio = ""; DatePaid = ""; Receipt = ""; AmountBilled = ""; AmountPaid = ""; ownername = ""; PaidBy = "";

                            }
                            catch { }
                        }
                    }
                    catch { }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "OK", "Cleveland", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "OK", "Cleveland");
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