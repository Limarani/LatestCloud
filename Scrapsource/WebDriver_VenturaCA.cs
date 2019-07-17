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
    public class WebDriver_VenturaCA
    {
        IWebDriver driver;
        IWebElement IPaidCheck;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        List<string> strURL = new List<string>();
        public string FTP_VenturaCA(string streetNo, string Direction, string streetName, string streetType, string unitNumber, string parcelNumber, string ownername, string searchType, string orderNumber)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new PhantomJSDriver();
            //driver = new ChromeDriver();
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    if (searchType == "titleflex")
                    {
                        string address = "";
                        if (Direction != "")
                        {
                            address = streetNo + " " + Direction + " " + streetName + " " + streetType + " " + unitNumber;
                        }
                        else
                        {
                            address = streetNo + " " + streetName + " " + streetType + " " + unitNumber;
                        }

                        gc.TitleFlexSearch(orderNumber, parcelNumber, ownername, address.Trim(), "CA", "Ventura");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_VenturaCA"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {

                        driver.Navigate().GoToUrl("http://assessor.countyofventura.org/research/propertyinfo.asp");
                        driver.FindElement(By.Id("label")).SendKeys(streetNo);
                        driver.FindElement(By.Id("label2")).SendKeys(streetName);
                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "CA", "Ventura");
                        driver.FindElement(By.XPath("//*[@id='tcSearch']/table/tbody/tr[8]/td[3]/input[1]")).SendKeys(Keys.Enter);

                        try
                        {
                            IWebElement Imultitable = driver.FindElement(By.XPath("//*[@id='pagebody']/div[2]/div[3]/div/table"));
                            IList<IWebElement> ImultiRow = Imultitable.FindElements(By.TagName("tr"));
                            IList<IWebElement> ImultiTD;
                            foreach (IWebElement multi in ImultiRow)
                            {
                                ImultiTD = multi.FindElements(By.TagName("td"));
                                if (ImultiTD.Count != 0 && !multi.Text.Contains("Parcel Number"))
                                {
                                    IWebElement Iparcel = ImultiTD[0].FindElement(By.TagName("a"));
                                    string strParcel = Iparcel.Text;
                                    string strMultiAddress = ImultiTD[1].Text;

                                    gc.insert_date(orderNumber, strParcel, 1045, strMultiAddress, 1, DateTime.Now);
                                }
                            }
                            if (ImultiRow.Count > 2 && ImultiRow.Count < 26)
                            {
                                HttpContext.Current.Session["multiParcel_Ventura"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (ImultiRow.Count > 26)
                            {
                                HttpContext.Current.Session["multiParcel_Ventura_Maximum"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='pagebody']/div[2]/div[3]/div/table/tbody/tr[1]/td[1]/a")).SendKeys(Keys.Enter);
                        }
                        catch { }
                    }

                    if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("http://assessor.countyofventura.org/research/propertyinfo.asp");
                        driver.FindElement(By.Id("APN")).SendKeys(parcelNumber.Replace("-", ""));
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "CA", "Ventura");
                        driver.FindElement(By.XPath("//*[@id='tcSearch']/table/tbody/tr[8]/td[3]/input[1]")).SendKeys(Keys.Enter);
                    }
                    try
                    {
                        //No Data Found
                        string nodata = driver.FindElement(By.XPath("//*[@id='pagebody']/div[2]/div[3]")).Text;
                        if (nodata.Contains("Property not found"))
                        {
                            HttpContext.Current.Session["Nodata_VenturaCA"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }


                    string strAddress = "", AssessorProeprty = "", previousParcelNumber = "", trackNumber = "", mapNumber = "", acreage = "", yearBuilt = "";
                    int Assessor = 0;
                    //Property Details
                    IWebElement IpropertyID = driver.FindElement(By.XPath("//*[@id='pagebody']/div[2]/div[3]/table[1]/tbody"));
                    IList<IWebElement> IpropertyRow = IpropertyID.FindElements(By.TagName("tr"));
                    IList<IWebElement> IpropertyTD;
                    foreach (IWebElement Property in IpropertyRow)
                    {
                        IpropertyTD = Property.FindElements(By.TagName("td"));
                        if (IpropertyTD.Count != 0)
                        {
                            if (Property.Text.Contains("Assessor Parcel Number"))
                            {
                                parcelNumber = IpropertyTD[1].Text;
                            }
                            if (Property.Text.Contains("Property Address"))
                            {
                                strAddress = GlobalClass.Before(IpropertyTD[1].Text, " (link to Google Maps)");
                            }
                        }
                    }
                    gc.CreatePdf(orderNumber, parcelNumber, "Property Details", driver, "CA", "Ventura");
                    //Property Land Use Details
                    IWebElement IpropertyLand = driver.FindElement(By.XPath("//*[@id='pagebody']/div[2]/div[3]/table[2]/tbody"));
                    IList<IWebElement> IpropertyLandRow = IpropertyLand.FindElements(By.TagName("tr"));
                    IList<IWebElement> IpropertyLandTD;
                    foreach (IWebElement Land in IpropertyLandRow)
                    {
                        IpropertyLandTD = Land.FindElements(By.TagName("td"));
                        if (IpropertyLandTD.Count != 0)
                        {
                            if (Land.Text.Contains("Previous Parcel Number"))
                            {
                                previousParcelNumber = IpropertyLandTD[3].Text;
                            }
                            if (Land.Text.Contains("Tract Number"))
                            {
                                trackNumber = IpropertyLandTD[1].Text;
                            }
                            if (Land.Text.Contains("Map Number"))
                            {
                                mapNumber = IpropertyLandTD[1].Text;
                            }
                            if (Assessor > 0)
                            {
                                AssessorProeprty = IpropertyLandTD[0].Text;
                                Assessor = 0;
                            }
                            if (Land.Text.Contains("Assessor Property Use Description"))
                            {
                                try
                                {
                                    AssessorProeprty = IpropertyLandTD[0].Text;
                                }
                                catch { }
                                Assessor++;
                            }
                        }
                    }

                    //Property Characteristics Details
                    IWebElement IpropertyCh = driver.FindElement(By.XPath("//*[@id='pagebody']/div[2]/div[3]/table[3]/tbody"));
                    IList<IWebElement> IpropertyChRow = IpropertyCh.FindElements(By.TagName("tr"));
                    IList<IWebElement> IpropertyChTD;
                    foreach (IWebElement character in IpropertyChRow)
                    {
                        IpropertyChTD = character.FindElements(By.TagName("td"));
                        if (IpropertyChTD.Count != 0)
                        {
                            if (character.Text.Contains("Acreage"))
                            {
                                acreage = IpropertyChTD[3].Text;
                            }
                            if (character.Text.Contains("Year Built"))
                            {
                                yearBuilt = IpropertyChTD[3].Text;
                            }
                        }
                    }

                    string strPropertyDetails = strAddress + "~" + trackNumber + "~" + mapNumber + "~" + previousParcelNumber + "~" + AssessorProeprty + "~" + acreage + "~" + yearBuilt;
                    gc.insert_date(orderNumber, parcelNumber, 1046, strPropertyDetails, 1, DateTime.Now);

                    //Tax Authority
                    string TaxAuthority = "";
                    try
                    {
                        driver.Navigate().GoToUrl("https://www.ventura.org/ttc/press-room/");
                        string authority = driver.FindElement(By.XPath("//*[@id='page-7756']/div/div[3]/div/div/div/div/p[3]")).Text.Replace("\r\n", " ");
                        string phone = driver.FindElement(By.XPath("//*[@id='page-7756']/div/div[4]/div/div/div/div/table/tbody/tr[3]/td[2]")).Text.Trim();
                        TaxAuthority = authority + " " + phone;
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Authority Details", driver, "CA", "Ventura");
                    }
                    catch { }
                    //Tax Information
                    driver.Navigate().GoToUrl("https://prop-tax.countyofventura.org/");
                    driver.FindElement(By.Id("ctl00_MainContent_txtAPN")).SendKeys(parcelNumber.Replace("-", ""));
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Search Details", driver, "CA", "Ventura");
                    driver.FindElement(By.Id("ctl00_MainContent_btnSubmitSearch")).SendKeys(Keys.Enter);
                    string TaxParcel = "", TaxType = "", TaxPayer = "", TaxLocation = "", TaxAmountDue = "", TaxFirst = "", TaxSecond = "", TaxFPaidDate = "", TaxSPaidDate = "", strDefault = "";
                    TaxParcel = driver.FindElement(By.Id("ctl00_MainContent_lblParcelNo")).Text;
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Search Result Details", driver, "CA", "Ventura");
                    string strCurrent = driver.CurrentWindowHandle;
                    IWebElement ITaxInfo = driver.FindElement(By.Id("ctl00_MainContent_grdvListing"));
                    IList<IWebElement> ITaxRow = ITaxInfo.FindElements(By.TagName("tr"));
                    IList<IWebElement> ITaxTD;
                    IList<IWebElement> ITaxTaxble;
                    foreach (IWebElement Tax in ITaxRow)
                    {
                        ITaxTD = Tax.FindElements(By.TagName("td"));
                        if (ITaxTD.Count != 0)
                        {
                            TaxType = ITaxTD[0].Text.Replace("\r\n", " ");
                            try
                            {
                                TaxFPaidDate = ITaxTD[3].FindElement(By.Id("ctl00_MainContent_grdvListing_ctl03_lbl1stDuePaid")).Text.Replace("\r\n", " ");
                                TaxSPaidDate = ITaxTD[3].FindElement(By.Id("ctl00_MainContent_grdvListing_ctl03_lbl2ndDuePaid")).Text.Replace("\r\n", " ");
                            }
                            catch { }
                            try
                            {
                                TaxFPaidDate = ITaxTD[3].FindElement(By.Id("ctl00_MainContent_grdvListing_ctl02_lbl1st")).Text.Replace("\r\n", " ");
                                TaxSPaidDate = ITaxTD[3].FindElement(By.Id("ctl00_MainContent_grdvListing_ctl02_lbl2nd")).Text.Replace("\r\n", " ");
                            }
                            catch { }
                            try
                            {
                                TaxFPaidDate = ITaxTD[3].FindElement(By.Id("ctl00_MainContent_grdvListing_ctl02_lbl1stDuePaid")).Text.Replace("\r\n", " ");
                                TaxSPaidDate = ITaxTD[3].FindElement(By.Id("ctl00_MainContent_grdvListing_ctl02_lbl2ndDuePaid")).Text.Replace("\r\n", " ");
                            }
                            catch { }
                            try
                            {
                                strDefault = ITaxTD[2].Text;
                                if (strDefault.Contains("For more information"))
                                {
                                    HttpContext.Current.Session["DefaultTax"] = "Default";
                                    gc.insert_date(orderNumber, TaxParcel, 1059, strDefault, 1, DateTime.Now);
                                }
                            }
                            catch { }
                            if (TaxType.Contains("Secured") && !TaxType.Contains("Supplemental"))
                            {

                                try
                                {
                                    TaxFirst = gc.Between(ITaxTD[2].Text, "1st Installment:", "2nd Installment:").Replace("\r\n", " ");
                                    TaxSecond = GlobalClass.After(ITaxTD[2].Text, "2nd Installment:").Replace("\r\n", " ");
                                }
                                catch { }
                                string strTaxLocation = GlobalClass.Before(ITaxTD[1].Text, "\r\nSecured property");
                                string[] secureSplit = strTaxLocation.Split('\r');
                                if (secureSplit.Count() == 2)
                                {
                                    TaxLocation = secureSplit[0].Replace("\n", "");
                                    TaxPayer = secureSplit[1].Replace("\n", "");
                                }
                                ITaxTaxble = ITaxTD[1].FindElements(By.TagName("a"));
                                foreach (IWebElement taxable in ITaxTaxble)
                                {
                                    if (taxable.Text.Contains("Taxable Value Information"))
                                    {
                                        string SecureName = "", SrtTaxAddress = "", strTaxYear = "", strTaxType = "", strTaxRate = "", strParcelNo = "", AssessTitle = "", AssessValue = "";
                                        taxable.Click();
                                        driver.SwitchTo().Window(driver.WindowHandles.Last());
                                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Secured Details", driver, "CA", "Ventura");
                                        IWebElement IName = driver.FindElement(By.XPath("/html/body/table[2]/tbody"));
                                        SecureName = gc.Between(IName.Text, "Name of Owner:", "Property Information");
                                        IWebElement IAddress = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td/table[1]/tbody/tr/td[1]"));
                                        SrtTaxAddress = GlobalClass.After(IAddress.Text, "Property Address:");
                                        IWebElement ISecureTax = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td/table[2]/tbody"));
                                        IList<IWebElement> ISecureTaxRow = ISecureTax.FindElements(By.TagName("tr"));
                                        IList<IWebElement> ISecureTD;
                                        foreach (IWebElement taxyear in ISecureTaxRow)
                                        {
                                            ISecureTD = taxyear.FindElements(By.TagName("td"));
                                            if (ISecureTD.Count != 0)
                                            {
                                                strTaxYear = ISecureTD[0].Text;
                                                strTaxType = ISecureTD[1].Text;
                                                strTaxRate = ISecureTD[2].Text;
                                                strParcelNo = ISecureTD[3].Text;

                                                string SecuredDetails = SecureName + "~" + SrtTaxAddress + "~" + strTaxYear + "~" + strTaxType + "~" + strTaxRate + "~" + "~" + strParcelNo;
                                                gc.insert_date(orderNumber, strParcelNo, 1049, SecuredDetails, 1, DateTime.Now);
                                            }
                                        }
                                        IWebElement ITaxAssess = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/td/table[3]/tbody"));
                                        IList<IWebElement> ITaxAssessRow = ITaxAssess.FindElements(By.TagName("tr"));
                                        IList<IWebElement> ITaxAssessTD;
                                        foreach (IWebElement TaxAssess in ITaxAssessRow)
                                        {
                                            ITaxAssessTD = TaxAssess.FindElements(By.TagName("td"));
                                            if (ITaxAssessTD.Count != 0)
                                            {
                                                AssessTitle += ITaxAssessTD[0].Text.Replace(":", "") + "~";
                                                AssessValue += ITaxAssessTD[1].Text + "~";
                                            }
                                        }
                                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + AssessTitle.Remove(AssessTitle.Length - 1, 1) + "' where Id = '" + 1048 + "'");
                                        gc.insert_date(orderNumber, TaxParcel, 1048, AssessValue.Remove(AssessValue.Length - 1, 1), 1, DateTime.Now);
                                        driver.SwitchTo().Window(strCurrent);
                                    }
                                }
                            }
                            if (TaxType.Contains("Supplemental"))
                            {

                                TaxFirst = gc.Between(ITaxTD[2].Text, "1st Installment:", "2nd Installment:").Replace("\r\n", " ");
                                TaxSecond = GlobalClass.After(ITaxTD[2].Text, "2nd Installment:").Replace("\r\n", " ");
                                string strTaxLocation = GlobalClass.Before(ITaxTD[1].Text, "\r\nOwnership change");
                                string[] secureSplit = strTaxLocation.Split('\r');
                                if (secureSplit.Count() == 2)
                                {
                                    TaxLocation = secureSplit[0].Replace("\n", "");
                                    TaxPayer = secureSplit[1].Replace("\n", "");
                                }
                                ITaxTaxble = ITaxTD[1].FindElements(By.TagName("a"));
                                foreach (IWebElement taxable in ITaxTaxble)
                                {
                                    if (taxable.Text.Contains("Taxable Value Information"))
                                    {
                                        int j = 0;
                                        string[] strYearTitle = new string[] { };
                                        string TaxValue1 = "", TaxValue2 = "", TaxValue3 = "", TaxValue4 = "";
                                        List<string> TaxSupplimentValue = new List<string>();
                                        string SupTaxType = "", SupplimentName = "", SuppTaxAddress = "", supTaxParcel = "", supTaxLine = "", supTaxAccountNo = "", YearTitle = "", YearValue = "", TaxTitle = "";
                                        taxable.Click();
                                        driver.SwitchTo().Window(driver.WindowHandles.Last());
                                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Suppliment Details", driver, "CA", "Ventura");
                                        IWebElement IName = driver.FindElement(By.XPath("/html/body/table[2]/tbody"));
                                        string strSupplimentYear = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[1]/font")).Text.Replace(" Tax Year", "");
                                        SupplimentName = gc.Between(IName.Text, "Name of Owner:", "Property Information");
                                        IWebElement IAddress = driver.FindElement(By.XPath("/html/body/table[3]"));
                                        SuppTaxAddress = gc.Between(IAddress.Text, "Property Address:", "Viewable Tax Years:");
                                        IWebElement ISup = driver.FindElement(By.XPath("/html/body/table[4]/tbody"));
                                        IList<IWebElement> ISupRow = ISup.FindElements(By.TagName("tr"));
                                        IList<IWebElement> ISupTD;
                                        foreach (IWebElement Supp in ISupRow)
                                        {
                                            ISupTD = Supp.FindElements(By.TagName("td"));
                                            if (ISupTD.Count != 0)
                                            {
                                                SupTaxType = ISupTD[0].Text;
                                                supTaxParcel = ISupTD[1].Text;
                                                supTaxLine = ISupTD[2].Text;
                                                supTaxAccountNo = ISupTD[3].Text;

                                                string supplimentDetails = SupplimentName + "~" + SuppTaxAddress + "~" + strSupplimentYear + "~" + SupTaxType + "~" + "~" + supTaxLine + "~" + supTaxAccountNo;
                                                gc.insert_date(orderNumber, supTaxParcel, 1049, supplimentDetails, 1, DateTime.Now);
                                            }
                                        }
                                        IWebElement ISuppYear = driver.FindElement(By.XPath("/html/body/table[6]/tbody"));
                                        IList<IWebElement> ISupYearRow = ISuppYear.FindElements(By.TagName("tr"));
                                        IList<IWebElement> ISupYearTD;
                                        IList<IWebElement> ISupYearTH;
                                        foreach (IWebElement Year in ISupYearRow)
                                        {
                                            ISupYearTD = Year.FindElements(By.TagName("td"));
                                            ISupYearTH = Year.FindElements(By.TagName("th"));
                                            if (ISupYearTH.Count != 0)
                                            {
                                                for (int i = 0; i < ISupYearTH.Count; i++)
                                                {
                                                    YearTitle += ISupYearTH[i].Text + "~";
                                                }
                                                YearTitle = YearTitle.Remove(YearTitle.Length - 1, 1);
                                                strYearTitle = YearTitle.Split('~');
                                            }
                                            if (ISupYearTD.Count != 0)
                                            {
                                                TaxValue1 += ISupYearTD[1].Text + "~";
                                                TaxValue2 += ISupYearTD[2].Text + "~";
                                                TaxValue3 += ISupYearTD[3].Text + "~";
                                                TaxValue4 += ISupYearTD[4].Text + "~";
                                                TaxTitle += ISupYearTD[0].Text + "~";
                                            }
                                        }
                                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + strYearTitle[j] + "~" + TaxTitle.Remove(TaxTitle.Length - 1, 1) + "' where Id = '" + 1061 + "'");
                                        TaxSupplimentValue.Add(TaxValue1.Remove(TaxValue1.Length - 1, 1));
                                        TaxSupplimentValue.Add(TaxValue2.Remove(TaxValue2.Length - 1, 1));
                                        TaxSupplimentValue.Add(TaxValue3.Remove(TaxValue3.Length - 1, 1));
                                        TaxSupplimentValue.Add(TaxValue4.Remove(TaxValue4.Length - 1, 1));
                                        foreach (string strtaxvalue in TaxSupplimentValue)
                                        {
                                            j++;
                                            try
                                            {
                                                gc.insert_date(orderNumber, TaxParcel, 1061, strYearTitle[j] + "~" + strtaxvalue, 1, DateTime.Now);
                                            }
                                            catch { }
                                        }
                                        driver.SwitchTo().Window(strCurrent);
                                    }
                                }
                            }
                            if (TaxType.Contains("Defaulted Taxes"))
                            {
                                ITaxTaxble = ITaxTD[1].FindElements(By.TagName("a"));
                                foreach (IWebElement taxable in ITaxTaxble)
                                {
                                    if (taxable.Text.Contains("Defaulted Tax Bill") && !strDefault.Contains("For more information"))
                                    {
                                        List<string> strDTax = new List<string>();
                                        strDTax.Add("May"); strDTax.Add("June"); strDTax.Add("July");
                                        string strDParcelNo = "", strDStatus = "", strDTaxDue = "", strDTaxType = "", strEligible = "", strRedemTaxType = "", strOriginal = "", strFeeCost = "", strPenalty = "", strTotal = "", strDTaxDate = "", strTTotal = "";
                                        taxable.Click();
                                        //driver.SwitchTo().Window(driver.WindowHandles.Last());
                                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Delinquent Details", driver, "CA", "Ventura");
                                        try
                                        {
                                            IWebElement IEligible = driver.FindElement(By.Id("ctl00_MainContent_lblAuctionDate"));
                                            strEligible = IEligible.Text;
                                        }
                                        catch { }
                                        IWebElement IdefaultTax = driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[3]/div[3]/table/tbody/tr[4]/td/table/tbody/tr/td/table"));
                                        IList<IWebElement> IdefaultTaxRow = IdefaultTax.FindElements(By.TagName("tr"));
                                        IList<IWebElement> IdefaultTaxTD;
                                        foreach (IWebElement defaultTax in IdefaultTaxRow)
                                        {
                                            IdefaultTaxTD = defaultTax.FindElements(By.TagName("td"));
                                            if (IdefaultTaxTD.Count != 0 && !defaultTax.Text.Contains("Status"))
                                            {
                                                strDStatus = IdefaultTaxTD[0].Text;
                                                strDParcelNo = IdefaultTaxTD[1].Text;
                                                strDTaxType = IdefaultTaxTD[3].Text;
                                                strDTaxDue = IdefaultTaxTD[4].Text.Replace("\r\n", "");
                                            }
                                        }
                                        string Default = strDStatus + "~" + strDParcelNo + "~" + strDTaxType + "~" + strDTaxDue + "~" + strEligible;
                                        gc.insert_date(orderNumber, TaxParcel, 1051, Default, 1, DateTime.Now);

                                        try
                                        {
                                            int i = 0;
                                            foreach (string dTax in strDTax)
                                            {
                                                IPaidCheck = driver.FindElement(By.Id("ctl00_MainContent_btnCollapse" + dTax + ""));
                                                if (IPaidCheck.Text == "+")
                                                {
                                                    IPaidCheck.Click();
                                                }
                                                try
                                                {
                                                    strDTaxDate = GlobalClass.After(driver.FindElement(By.Id("ctl00_MainContent_pnl" + dTax + "")).Text, "before ");
                                                }
                                                catch { }
                                                try
                                                {
                                                    IWebElement ITotal = driver.FindElement(By.XPath("//*[@id='ctl00_MainContent_pnlDue']/table[" + (i + 2) + "]"));
                                                    IList<IWebElement> ITotalRow = ITotal.FindElements(By.TagName("tr"));
                                                    IList<IWebElement> ITotalTD;
                                                    foreach (IWebElement total in ITotalRow)
                                                    {
                                                        ITotalTD = total.FindElements(By.TagName("td"));
                                                        if (ITotalTD.Count != 0 && total.Text.Contains("TOTAL:"))
                                                        {
                                                            strTTotal = ITotalTD[0].Text.Replace("TOTAL: ", "").Trim();
                                                        }
                                                    }
                                                }
                                                catch { }
                                                IWebElement IPaid = driver.FindElement(By.Id("ctl00_MainContent_pnlCollapse" + dTax + ""));
                                                IList<IWebElement> IPaidRow = IPaid.FindElements(By.TagName("tr"));
                                                IList<IWebElement> IPaidTD;
                                                foreach (IWebElement paid in IPaidRow)
                                                {
                                                    IPaidTD = paid.FindElements(By.TagName("td"));
                                                    if (IPaidTD.Count != 0 && !paid.Text.Contains("Tax Type") && paid.Text.Trim() != "")
                                                    {
                                                        strRedemTaxType = IPaidTD[0].Text;
                                                        strOriginal = IPaidTD[1].Text;
                                                        strFeeCost = IPaidTD[2].Text;
                                                        strPenalty = IPaidTD[3].Text;
                                                        strTotal = IPaidTD[4].Text;

                                                        string Redemption = strDTaxDate + "~" + strRedemTaxType + "~" + strOriginal + "~" + strFeeCost + "~" + strPenalty + "~" + strTotal;
                                                        gc.insert_date(orderNumber, TaxParcel, 1052, Redemption, 1, DateTime.Now);

                                                    }
                                                }

                                                string RedemptionTotal = "" + "~" + "Total" + "~" + "" + "~" + "" + "~" + "" + "~" + strTTotal;
                                                gc.insert_date(orderNumber, TaxParcel, 1052, RedemptionTotal, 1, DateTime.Now);

                                                i++;
                                                gc.CreatePdf(orderNumber, parcelNumber, "Tax Delinquent Due Details", driver, "CA", "Ventura");
                                            }
                                            for (int j = 0; j < i; j++)
                                            {
                                                driver.Navigate().Back();
                                                Thread.Sleep(2000);
                                            }
                                        }
                                        catch { }
                                    }
                                }
                            }
                            string TaxDetails = TaxType + "~" + TaxLocation + "~" + TaxPayer + "~" + TaxFirst + "~" + TaxSecond + "~" + TaxFPaidDate + "~" + TaxSPaidDate + "~" + TaxAuthority;
                            gc.insert_date(orderNumber, TaxParcel, 1047, TaxDetails, 1, DateTime.Now);
                            TaxType = ""; TaxLocation = ""; TaxPayer = ""; TaxFirst = ""; TaxSecond = ""; TaxFPaidDate = ""; TaxSPaidDate = "";
                        }
                    }
                    try { 
                    //Tax Payment History
                    string strPHYear = "";
                    IWebElement IPayment = driver.FindElement(By.XPath("//*[@id='rightcontentcolumn']/table/tbody/tr[3]/td/table/tbody/tr[2]/td/table"));
                    IList<IWebElement> IPaymentRow = IPayment.FindElements(By.TagName("tr"));
                    IList<IWebElement> IPaymentTD;
                    foreach (IWebElement payment in IPaymentRow)
                    {
                        IPaymentTD = payment.FindElements(By.TagName("td"));
                        if (IPaymentTD.Count != 0)
                        {
                            IWebElement ITaxClick = IPaymentTD[1].FindElement(By.TagName("input"));
                            string strTaxClick = ITaxClick.GetAttribute("src");
                            if (strTaxClick.Contains("images/tph.gif"))
                            {
                                ITaxClick.Click();
                            }
                        }
                    }
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Payment Details", driver, "CA", "Ventura");
                    IWebElement ITaxPayPast = driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[3]/div[3]/table"));
                    IList<IWebElement> ITaxPayPastRow = ITaxPayPast.FindElements(By.TagName("table"));
                    IList<IWebElement> ITaxPayPastTR;
                    IList<IWebElement> ITaxPayTD;
                    foreach (IWebElement Past in ITaxPayPastRow)
                    {
                        ITaxPayPastTR = Past.FindElements(By.TagName("tr"));
                        if (Past.Text != "" && Past.Text.Contains("Date Paid") && !Past.Text.Contains("TOTALS"))
                        {
                            foreach (IWebElement PayPast in ITaxPayPastTR)
                            {
                                ITaxPayTD = PayPast.FindElements(By.TagName("td"));
                                if (ITaxPayTD.Count != 0 && !Past.Text.Contains("Tax Payments") && (PayPast.Text.Contains("Secured") || PayPast.Text.Contains("Supplemental") || PayPast.Text.Contains("Default")))
                                {
                                    string strPayDetails = strPHYear + "~" + ITaxPayTD[0].Text + "~" + ITaxPayTD[1].Text + "~" + ITaxPayTD[2].Text + "~" + ITaxPayTD[3].Text + "~" + ITaxPayTD[4].Text + "~" + ITaxPayTD[5].Text + "~" + ITaxPayTD[6].Text + "~" + ITaxPayTD[7].Text;
                                    gc.insert_date(orderNumber, TaxParcel, 1050, strPayDetails, 1, DateTime.Now);
                                }

                            }
                        }
                        if (Past.Text != "" && !Past.Text.Contains("Date Paid") && Past.Text.Contains("TOTALS") && !Past.Text.Contains("Tax Payments"))
                        {
                            foreach (IWebElement PayPast in ITaxPayPastTR)
                            {
                                ITaxPayTD = PayPast.FindElements(By.TagName("td"));
                                if (ITaxPayTD.Count != 0 && PayPast.Text.Contains("TOTALS") && ITaxPayTD.Count < 5)
                                {
                                    string strPayDetails = strPHYear + "~" + "~" + "~" + "~" + "~" + ITaxPayTD[0].Text + "~" + ITaxPayTD[1].Text + "~" + ITaxPayTD[2].Text + "~" + ITaxPayTD[3].Text;
                                    gc.insert_date(orderNumber, TaxParcel, 1050, strPayDetails, 1, DateTime.Now);
                                }

                            }
                        }
                        if (!Past.Text.Contains("Property Address") && Past.Text.Contains("Tax Payments for Calendar Year"))
                        {
                            strPHYear = ITaxPayPastTR[0].Text.Replace("Tax Payments for Calendar Year ", "");
                        }
                    }
                    }
                    catch { }
                    //Bill
                    int billcount = 0;
                    try
                    {
                        driver.Navigate().Back();
                        Thread.Sleep(3000);
                        IWebElement IBill = driver.FindElement(By.Id("ctl00_MainContent_grdvListing"));
                        IList<IWebElement> IBillRow = IBill.FindElements(By.TagName("a"));
                        foreach (IWebElement bill in IBillRow)
                        {
                            if (bill.Text != "" && bill.Text.Contains("View Your Tax Bill"))
                            {
                                IList<IWebElement> IYear;
                                bill.Click();
                                ////driver.FindElement(By.Id("ctl00_MainContent_btnViewReport")).Click();
                                //IWebElement IYearBill = driver.FindElement(By.Id("ctl00_MainContent_drpDwnTaxYears"));
                                //SelectElement sSelectYear = new SelectElement(IYearBill);
                                //IYear = sSelectYear.Options.ToList();
                                //foreach (IWebElement yearlist in IYear)
                                //{
                                //    if (yearlist.Text.Contains("Supplemental") || yearlist.Text.Contains("Secured"))
                                //    {
                                //        gc.CreatePdf(orderNumber, parcelNumber, "Tax Payment Details" + billcount, driver, "CA", "Ventura");
                                //        billcount++;
                                //    }
                                //}
                                try
                                {
                                    driver.Navigate().GoToUrl(driver.Url);
                                    IWebElement IBillYear = driver.FindElement(By.Id("ctl00_MainContent_drpDwnTaxYears"));
                                    SelectElement selectBill = new SelectElement(IBillYear);
                                    IYear = selectBill.Options;
                                    foreach (IWebElement sBill in IYear)
                                    {
                                        if (sBill.Text.Contains("Secured") || sBill.Text.Contains("Supplemental") || sBill.Text.Contains("Default"))
                                        {
                                            strURL.Add(sBill.Text);
                                        }
                                    }
                                    //selectBill.SelectByIndex(j);
                                    //for (int j = 0; j < 3; j++)
                                    // {
                                    foreach (string YearOption in strURL)
                                    {
                                        IWebElement IBills = driver.FindElement(By.Id("ctl00_MainContent_drpDwnTaxYears"));
                                        SelectElement selectBills = new SelectElement(IBills);
                                        selectBills.SelectByText(YearOption);
                                        Thread.Sleep(3000);
                                        //IWebElement DownloadBill = driver1.FindElement(By.Id("ctl00_MainContent_btnViewReport"));
                                        //DownloadBill.Click();
                                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Payment Details" + YearOption, driver, "CA", "Ventura");
                                        IWebElement IDefault = driver.FindElement(By.Id("ctl00_MainContent_drpDwnTaxYears"));
                                        SelectElement sDefault = new SelectElement(IDefault);
                                        string strdefault = sDefault.SelectedOption.Text;
                                        if (strdefault.Contains("Defaulted Tax") && strDefault.Contains("For more information"))
                                        {
                                            List<string> strDTax = new List<string>();
                                            strDTax.Add("May"); strDTax.Add("June"); strDTax.Add("July");
                                            string strDParcelNo = "", strDStatus = "", strDTaxDue = "", strDTaxType = "", strEligible = "", strRedemTaxType = "", strOriginal = "", strFeeCost = "", strPenalty = "", strTotal = "", strDTaxDate = "", strTTotal = "";
                                            try
                                            {
                                                IWebElement IEligible = driver.FindElement(By.Id("ctl00_MainContent_lblAuctionDate"));
                                                strEligible = IEligible.Text;
                                            }
                                            catch { }
                                            IWebElement IdefaultTax = driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[3]/div[3]/table/tbody/tr[4]/td/table/tbody/tr/td/table"));
                                            IList<IWebElement> IdefaultTaxRow = IdefaultTax.FindElements(By.TagName("tr"));
                                            IList<IWebElement> IdefaultTaxTD;
                                            foreach (IWebElement defaultTax in IdefaultTaxRow)
                                            {
                                                IdefaultTaxTD = defaultTax.FindElements(By.TagName("td"));
                                                if (IdefaultTaxTD.Count != 0 && !defaultTax.Text.Contains("Status"))
                                                {
                                                    strDStatus = IdefaultTaxTD[0].Text;
                                                    strDParcelNo = IdefaultTaxTD[1].Text;
                                                    strDTaxType = IdefaultTaxTD[3].Text;
                                                    strDTaxDue = IdefaultTaxTD[4].Text.Replace("\r\n", "");
                                                }
                                            }
                                            string Default = strDStatus + "~" + strDParcelNo + "~" + strDTaxType + "~" + strDTaxDue + "~" + strEligible;
                                            gc.insert_date(orderNumber, TaxParcel, 1051, Default, 1, DateTime.Now);

                                            try
                                            {
                                                int i = 0;
                                                foreach (string dTax in strDTax)
                                                {
                                                    IPaidCheck = driver.FindElement(By.Id("ctl00_MainContent_btnCollapse" + dTax + ""));
                                                    if (IPaidCheck.Text == "+")
                                                    {
                                                        IPaidCheck.Click();
                                                    }
                                                    try
                                                    {
                                                        strDTaxDate = GlobalClass.After(driver.FindElement(By.Id("ctl00_MainContent_pnl" + dTax + "")).Text, "before ");
                                                    }
                                                    catch { }
                                                    try
                                                    {
                                                        IWebElement ITotal = driver.FindElement(By.XPath("//*[@id='ctl00_MainContent_pnlDue']/table[" + (i + 2) + "]"));
                                                        IList<IWebElement> ITotalRow = ITotal.FindElements(By.TagName("tr"));
                                                        IList<IWebElement> ITotalTD;
                                                        foreach (IWebElement total in ITotalRow)
                                                        {
                                                            ITotalTD = total.FindElements(By.TagName("td"));
                                                            if (ITotalTD.Count != 0 && total.Text.Contains("TOTAL:"))
                                                            {
                                                                strTTotal = ITotalTD[0].Text.Replace("TOTAL: ", "").Trim();
                                                            }
                                                        }
                                                    }
                                                    catch { }
                                                    IWebElement IPaid = driver.FindElement(By.Id("ctl00_MainContent_pnlCollapse" + dTax + ""));
                                                    IList<IWebElement> IPaidRow = IPaid.FindElements(By.TagName("tr"));
                                                    IList<IWebElement> IPaidTD;
                                                    foreach (IWebElement paid in IPaidRow)
                                                    {
                                                        IPaidTD = paid.FindElements(By.TagName("td"));
                                                        if (IPaidTD.Count != 0 && !paid.Text.Contains("Tax Type") && paid.Text.Trim() != "")
                                                        {
                                                            strRedemTaxType = IPaidTD[0].Text;
                                                            strOriginal = IPaidTD[1].Text;
                                                            strFeeCost = IPaidTD[2].Text;
                                                            strPenalty = IPaidTD[3].Text;
                                                            strTotal = IPaidTD[4].Text;

                                                            string Redemption = strDTaxDate + "~" + strRedemTaxType + "~" + strOriginal + "~" + strFeeCost + "~" + strPenalty + "~" + strTotal;
                                                            gc.insert_date(orderNumber, TaxParcel, 1052, Redemption, 1, DateTime.Now);

                                                        }
                                                    }
                                                    string RedemptionTotal = "" + "~" + "Total" + "~" + "" + "~" + "" + "~" + "" + "~" + strTTotal;
                                                    gc.insert_date(orderNumber, TaxParcel, 1052, RedemptionTotal, 1, DateTime.Now);

                                                    i++;
                                                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Delinquent Due Details", driver, "CA", "Ventura");
                                                }
                                            }
                                            catch { }
                                        }
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                    catch { }


                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "CA", "Ventura", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "CA", "Ventura");
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