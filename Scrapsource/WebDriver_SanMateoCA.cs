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
    public class WebDriver_SanMateoCA
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        public string FTP_SanMateoCA(string address, string parcelNumber, string ownername, string searchType, string orderNumber)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string Default = "", SupplimentTax = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())//PhantomJSDriver
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, "", ownername.Trim(), address.Trim(), "CA", "San Mateo");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_CASanMateo"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    //if (searchType == "address")
                    //{
                    //    driver.Navigate().GoToUrl("http://www.sanmateocountytaxcollector.org/SMCWPS/pages/secureSearch.jsp");
                    //    Thread.Sleep(2000);
                    //    driver.FindElement(By.LinkText("Address")).Click();
                    //    Thread.Sleep(2000);

                    //    driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form/table/tbody/tr[2]/td[1]/table/tbody/tr[6]/td[2]/h3/input[1]")).SendKeys(streetNo);
                    //    driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form/table/tbody/tr[2]/td[1]/table/tbody/tr[6]/td[2]/h3/input[2]")).SendKeys(streetName);
                    //    IWebElement ISelect = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form/table/tbody/tr[2]/td[1]/table/tbody/tr[7]/td[2]/h3/select"));
                    //    SelectElement sSelect = new SelectElement(ISelect);
                    //    sSelect.SelectByText(city.ToUpper());
                    //    Thread.Sleep(2000);
                    //    gc.CreatePdf_WOP(orderNumber, "Address search", driver, "CA", "San Mateo");

                    //    driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form/table/tbody/tr[2]/td[1]/table/tbody/tr[10]/td/a[1]")).SendKeys(Keys.Enter);
                    //    Thread.Sleep(3000);
                    //    gc.CreatePdf_WOP(orderNumber, "Address search Result", driver, "CA", "San Mateo");
                    //    try
                    //    {
                    //        IWebElement IAddressSecureClick = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form/table/tbody/tr[7]/td/table"));
                    //        IList<IWebElement> IAddressSecureClickRow = IAddressSecureClick.FindElements(By.TagName("tr"));
                    //        IList<IWebElement> IAddressSecureClickTD;
                    //        foreach(IWebElement addressclick in IAddressSecureClickRow)
                    //        {
                    //            IAddressSecureClickTD = addressclick.FindElements(By.TagName("td"));
                    //            if (IAddressSecureClickTD.Count != 0 && !addressclick.Text.Contains("Assessor ID") && IAddressSecureClickRow.Count < 5)
                    //            {
                    //                if (IAddressSecureClick.Text.Contains(streetNo.ToUpper().Trim() + " " + streetName.ToUpper().Trim()))
                    //                {
                    //                    driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form/table/tbody/tr[7]/td/table/tbody/tr[2]/td[1]/a")).SendKeys(Keys.Enter);
                    //                    Thread.Sleep(3000);
                    //                }
                    //            }
                    //        }
                    //    }
                    //    catch { }
                    //    try
                    //    {
                    //        IWebElement IAddressClick = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form/table/tbody/tr[7]/td/table"));
                    //        IList<IWebElement> IAddressClickRow = IAddressClick.FindElements(By.TagName("tr"));
                    //        IList<IWebElement> IAddressClickTD;
                    //        foreach (IWebElement address in IAddressClickRow)
                    //        {
                    //            IAddressClickTD = address.FindElements(By.TagName("td"));
                    //            if (IAddressClickTD.Count != 0 && !address.Text.Contains("Assessor ID") && address.Text.Contains("Tax Defaulted Property") && address.Text.Contains(streetNo.ToUpper().Trim() + " " + streetName.ToUpper().Trim()))
                    //            {
                    //                parcelNumber = IAddressClickTD[0].FindElement(By.TagName("a")).Text;
                    //                Default = "Delinquent";
                    //            }
                    //            if (IAddressClickTD.Count != 0 && !address.Text.Contains("Assessor ID"))
                    //            {
                    //                string strMultiParcelNo = IAddressClickTD[0].Text;
                    //                string strMultiAddress = IAddressClickTD[1].Text;
                    //                string strRollYear = IAddressClickTD[3].Text;
                    //                string strBillType = IAddressClickTD[4].Text;

                    //                string MultiparcelDetails = strMultiAddress + "~" + strRollYear + "~" + strBillType;
                    //                gc.insert_date(orderNumber, strMultiParcelNo, 1879, MultiparcelDetails, 1, DateTime.Now);

                    //                HttpContext.Current.Session["Multiparcel_SanMateoCA"] = "Yes";
                    //                driver.Quit();
                    //                return "MultiParcel";
                    //            }
                    //        }
                    //        gc.CreatePdf_WOP(orderNumber, "Address search Result", driver, "CA", "San Mateo");
                    //    }
                    //    catch { }
                    //}

                    if (searchType == "parcel")
                    {
                        string first = "", middle = "", last = "";
                        driver.Navigate().GoToUrl("http://www.sanmateocountytaxcollector.org/SMCWPS/pages/secureSearch.jsp");
                        //driver.FindElement(By.LinkText("Parcel"));
                        Thread.Sleep(2000);
                        if (parcelNumber.Contains("-"))
                        {
                            parcelNumber = parcelNumber.Replace("-", "");
                        }
                        if (parcelNumber.Count() == 9)
                        {
                            first = parcelNumber.Substring(0, 3);
                            middle = parcelNumber.Substring(3, 3);
                            last = parcelNumber.Substring(6, 3);
                        }
                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form/table/tbody/tr[2]/td[1]/table/tbody/tr[3]/td/h3/input[1]")).SendKeys(first);
                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form/table/tbody/tr[2]/td[1]/table/tbody/tr[3]/td/h3/input[2]")).SendKeys(middle);
                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form/table/tbody/tr[2]/td[1]/table/tbody/tr[3]/td/h3/input[3]")).SendKeys(last);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "CA", "San Mateo");

                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form/table/tbody/tr[2]/td[1]/table/tbody/tr[4]/td/a[1]")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Result", driver, "CA", "San Mateo");
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form/table/tbody/tr[2]"));
                        if (INodata.Text.Contains("No Data Found"))
                        {
                            HttpContext.Current.Session["Nodata_CASanMateo"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    string currentWindow = driver.CurrentWindowHandle;
                    try
                    {
                        IWebElement ISupplimentParcel = driver.FindElement(By.XPath("//*[@id='listTbl']/tbody"));
                        IList<IWebElement> ISupplimentParcelRow = ISupplimentParcel.FindElements(By.TagName("tr"));
                        IList<IWebElement> ISupplimentParcelTD;
                        IList<IWebElement> ISupplimentClick;
                        foreach (IWebElement Suppliment in ISupplimentParcelRow)
                        {
                            ISupplimentParcelTD = Suppliment.FindElements(By.TagName("td"));
                            if (ISupplimentParcelTD.Count != 0 && Suppliment.Text.Contains("Tax Defaulted Property"))
                            {
                                Default = "Delinquent";
                            }
                            if (ISupplimentParcelTD.Count != 0 && Suppliment.Text.Contains("Supplemental Property"))
                            {
                                ISupplimentClick = ISupplimentParcelTD[0].FindElements(By.TagName("a"));
                                foreach (IWebElement click in ISupplimentClick)
                                {
                                    if (click.Text.Trim() == "")
                                    {
                                        click.SendKeys(Keys.Enter);
                                        driver.SwitchTo().Window(driver.WindowHandles.Last());
                                        string TaxType = "", ParcelNo = "", RollYear = "", FiscalYear = "", TaxRate = "", ValueDate = "", OriginalBill = "", OwnerAddress = "", PropertyAddress = "";
                                        string InstallmentType = "", FirstInstallment = "", SecondInstallment = "", InstallmentTotal = "";
                                        string BaseValue = "", Land = "", Improvement = "", PersonalProperty = "", Exemptions = "", NetCash = "", CompositeRate = "", PenaltyRate = "";
                                        string firstdelinquent = "", secondDelinquent = "";
                                        IWebElement ISupplimentTax = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[3]/td/table/tbody/tr/td/form/table[2]/tbody"));
                                        IList<IWebElement> ISupplimentTaxRow = ISupplimentTax.FindElements(By.TagName("tr"));
                                        IList<IWebElement> ISupplimentTaxTD;
                                        foreach (IWebElement SupTax in ISupplimentTaxRow)
                                        {
                                            ISupplimentTaxTD = SupTax.FindElements(By.TagName("td"));
                                            if (ISupplimentTaxTD.Count != 0 && SupTax.Text.Contains("Supplemental Property Tax"))
                                            {
                                                TaxType = ISupplimentTaxTD[0].Text.Trim();
                                            }
                                            if (ISupplimentTaxTD.Count != 0 && !SupTax.Text.Contains("Supplemental Property Tax") && !SupTax.Text.Contains("Parcel") && !SupTax.Text.Contains("PERIOD COVERED") && !SupTax.Text.Contains("Be aware that") && !SupTax.Text.Contains("Base Values") && SupTax.Text.Trim() != "")
                                            {
                                                if (ISupplimentTaxTD.Count == 6)
                                                {
                                                    ParcelNo = ISupplimentTaxTD[0].Text;
                                                    parcelNumber = ISupplimentTaxTD[0].Text;
                                                    RollYear = ISupplimentTaxTD[1].Text;
                                                    FiscalYear = ISupplimentTaxTD[2].Text;
                                                    TaxRate = ISupplimentTaxTD[3].Text;
                                                    ValueDate = ISupplimentTaxTD[4].Text;
                                                    OriginalBill = ISupplimentTaxTD[5].Text;

                                                    gc.CreatePdf(orderNumber, ParcelNo, "Suppliment Tax" + RollYear, driver, "CA", "San Mateo");
                                                }
                                                if (ISupplimentTaxTD.Count == 4 && SupTax.Text.Contains("Owner Address"))
                                                {
                                                    firstdelinquent = ISupplimentTaxTD[2].Text.Trim();
                                                    secondDelinquent = ISupplimentTaxTD[3].Text.Trim();
                                                }
                                                if (ISupplimentTaxTD.Count == 5 && SupTax.Text.Contains("General Tax"))
                                                {
                                                    OwnerAddress += ISupplimentTaxTD[0].Text.Trim();
                                                }
                                                if (ISupplimentTaxTD.Count == 3)
                                                {
                                                    OwnerAddress += ISupplimentTaxTD[0].Text.Trim() + " ";
                                                    firstdelinquent = firstdelinquent + "~" + ISupplimentTaxTD[1].Text;
                                                    secondDelinquent = secondDelinquent + "~" + ISupplimentTaxTD[2].Text;

                                                    gc.insert_date(orderNumber, ParcelNo, 1880, TaxType + "~" + RollYear + "~" + firstdelinquent + "~" + "~", 1, DateTime.Now);
                                                    gc.insert_date(orderNumber, ParcelNo, 1880, TaxType + "~" + RollYear + "~" + secondDelinquent + "~" + "~", 1, DateTime.Now);
                                                }
                                                if (ISupplimentTaxTD.Count == 4 && !SupTax.Text.Contains("Owner Address"))
                                                {
                                                    OwnerAddress += ISupplimentTaxTD[0].Text.Trim() + " ";
                                                    InstallmentType = "General Tax";
                                                    FirstInstallment = ISupplimentTaxTD[1].Text;
                                                    SecondInstallment = ISupplimentTaxTD[2].Text;
                                                    InstallmentTotal = ISupplimentTaxTD[3].Text;

                                                    string GeneralTaxDetails = InstallmentType + "~" + FirstInstallment + "~" + SecondInstallment + "~" + InstallmentTotal;
                                                    gc.insert_date(orderNumber, ParcelNo, 1880, TaxType + "~" + RollYear + "~" + GeneralTaxDetails, 1, DateTime.Now);
                                                }
                                                if (ISupplimentTaxTD.Count == 5 && (SupTax.Text.Contains("Penalty") || SupTax.Text.Contains("Total Amount") || SupTax.Text.Contains("Paid Date")))
                                                {
                                                    if (!ISupplimentTaxTD[0].Text.Contains("Property Location"))
                                                    {
                                                        PropertyAddress += ISupplimentTaxTD[0].Text.Trim() + " ";
                                                    }
                                                    InstallmentType = ISupplimentTaxTD[1].Text;
                                                    FirstInstallment = ISupplimentTaxTD[2].Text;
                                                    SecondInstallment = ISupplimentTaxTD[3].Text;
                                                    InstallmentTotal = ISupplimentTaxTD[4].Text;

                                                    string GeneralTaxDetails = InstallmentType + "~" + FirstInstallment + "~" + SecondInstallment + "~" + InstallmentTotal;
                                                    gc.insert_date(orderNumber, ParcelNo, 1880, TaxType + "~" + RollYear + "~" + GeneralTaxDetails, 1, DateTime.Now);
                                                }
                                                if (ISupplimentTaxTD.Count == 7 && SupTax.Text.Contains("OLD"))
                                                {
                                                    BaseValue = ISupplimentTaxTD[0].Text;
                                                    Land = ISupplimentTaxTD[1].Text;
                                                    Improvement = ISupplimentTaxTD[2].Text;
                                                    PersonalProperty = ISupplimentTaxTD[3].Text;
                                                    Exemptions = ISupplimentTaxTD[4].Text;
                                                    NetCash = ISupplimentTaxTD[5].Text;
                                                    CompositeRate = ISupplimentTaxTD[6].Text;
                                                    PenaltyRate = "";

                                                    string SupplimentDetails = TaxType + "~" + RollYear + "~" + BaseValue + "~" + Land + "~" + Improvement + "~" + PersonalProperty + "~" + Exemptions + "~" + NetCash + "~" + "~" + CompositeRate + "~" + PenaltyRate;
                                                    gc.insert_date(orderNumber, ParcelNo, 1875, SupplimentDetails, 1, DateTime.Now);

                                                }
                                                if (ISupplimentTaxTD.Count == 7 && SupTax.Text.Contains("NEW"))
                                                {
                                                    BaseValue = ISupplimentTaxTD[0].Text;
                                                    Land = ISupplimentTaxTD[1].Text;
                                                    Improvement = ISupplimentTaxTD[2].Text;
                                                    PersonalProperty = ISupplimentTaxTD[3].Text;
                                                    Exemptions = ISupplimentTaxTD[4].Text;
                                                    NetCash = ISupplimentTaxTD[5].Text;
                                                    CompositeRate = "";
                                                    PenaltyRate = "";

                                                    string SupplimentDetails = TaxType + "~" + RollYear + "~" + BaseValue + "~" + Land + "~" + Improvement + "~" + PersonalProperty + "~" + Exemptions + "~" + NetCash + "~" + "~" + CompositeRate + "~" + PenaltyRate;
                                                    gc.insert_date(orderNumber, ParcelNo, 1875, SupplimentDetails, 1, DateTime.Now);

                                                }
                                                if (ISupplimentTaxTD.Count == 7 && SupTax.Text.Contains("SUPPLEMENTAL"))
                                                {
                                                    BaseValue = ISupplimentTaxTD[0].Text;
                                                    Land = ISupplimentTaxTD[1].Text;
                                                    Improvement = ISupplimentTaxTD[2].Text;
                                                    PersonalProperty = ISupplimentTaxTD[3].Text;
                                                    Exemptions = ISupplimentTaxTD[4].Text;
                                                    NetCash = ISupplimentTaxTD[5].Text;
                                                    CompositeRate = "";
                                                    PenaltyRate = ISupplimentTaxTD[6].Text;

                                                    string SupplimentDetails = TaxType + "~" + RollYear + "~" + BaseValue + "~" + Land + "~" + Improvement + "~" + PersonalProperty + "~" + Exemptions + "~" + NetCash + "~" + "~" + CompositeRate + "~" + PenaltyRate;
                                                    gc.insert_date(orderNumber, ParcelNo, 1875, SupplimentDetails, 1, DateTime.Now);

                                                }
                                            }
                                        }

                                        string PropertyDetails = PropertyAddress + "~" + OwnerAddress + "~" + RollYear + "~" + FiscalYear + "~" + TaxRate + "~" + "" + "~" + ValueDate + "~" + OriginalBill;
                                        gc.insert_date(orderNumber, ParcelNo, 1874, PropertyDetails, 1, DateTime.Now);
                                        //db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + InstallmentType.Remove(InstallmentType.Length - 1, 1) + "' where Id = '" + 1880 + "'");
                                        //gc.insert_date(orderNumber, ParcelNo, 1880, TaxType + "~" + RollYear + "~" + FirstInstallment.Remove(FirstInstallment.Length - 1, 1), 1, DateTime.Now);
                                        //gc.insert_date(orderNumber, ParcelNo, 1880, TaxType + "~" + RollYear + "~" + SecondInstallment.Remove(SecondInstallment.Length - 1, 1), 1, DateTime.Now);
                                        //gc.insert_date(orderNumber, ParcelNo, 1880, TaxType + "~" + RollYear + "~" + InstallmentTotal.Remove(InstallmentTotal.Length - 1, 1), 1, DateTime.Now);

                                        driver.SwitchTo().Window(currentWindow);
                                    }
                                }
                            }
                        }
                    }
                    catch { }

                    try
                    {
                        try
                        {
                            IWebElement ISecureParcel = driver.FindElement(By.XPath("//*[@id='listTbl']/tbody"));
                            IList<IWebElement> ISecureParcelRow = ISecureParcel.FindElements(By.TagName("tr"));
                            IList<IWebElement> ISecureParcelTD;
                            foreach (IWebElement Secure in ISecureParcelRow)
                            {
                                ISecureParcelTD = Secure.FindElements(By.TagName("td"));
                                if (ISecureParcelTD.Count != 0 && Secure.Text.Contains("Secured Property"))
                                {
                                    IWebElement IsecureClick = ISecureParcelTD[0].FindElement(By.TagName("a"));
                                    if (IsecureClick.Text != "")
                                    {
                                        IsecureClick.SendKeys(Keys.Enter);
                                        break;
                                    }
                                }
                            }
                            if (ISecureParcel.Text.Contains("Tax Defaulted Property"))
                            {
                                Default = "Delinquent";
                            }
                        }
                        catch { }
                        IWebElement ISelectYear = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[1]/table[1]/tbody/tr[3]/td/table/tbody/tr[1]/td[2]/select"));
                        string strYear = ISelectYear.Text;
                        string strCurrentYear = strYear.Replace("\r\n", "").Replace("  ", " ").Trim();
                        string[] selectYear = strCurrentYear.Split(' ');
                        for (int i = 0; i <= 2; i++)
                        {
                            IWebElement ISecureSelectYear = null;
                            string strTaxType = "", strParcelNo = "", strTaxRate = "", AssessmentYear = "", RollYear = "", OwnerAddress = "", PropertyAddress = "", LegalDescription = "";
                            string strLand = "", strImprovement = "", strExemptions = "", CompositeRate = "", PenaltyRate = "", NetValue = "";
                            string strInstallmentType = "", strFirtsInstallment = "", strSecondInstallment = "", strInstallmentTotal = "";
                            string SpecialCharges = "", PhoneContact = "", Amount = "";
                            string year = selectYear[i];
                            try
                            {
                                try
                                {
                                    ISecureSelectYear = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[1]/table[1]/tbody/tr[3]/td/table/tbody/tr[1]/td[2]/select"));
                                }
                                catch { }
                                try
                                {
                                    if (ISecureSelectYear == null)
                                    {
                                        ISecureSelectYear = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[1]/table[1]/tbody/tr[4]/td/table/tbody/tr[1]/td[2]/select"));
                                    }
                                }
                                catch { }
                                SelectElement sSecureSelectYear = new SelectElement(ISecureSelectYear);
                                if (year != "")
                                {
                                    sSecureSelectYear.SelectByText(year);
                                    Thread.Sleep(3000);
                                }
                                IWebElement IsecureTax = null;
                                try
                                {
                                    IsecureTax = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[1]/table[1]/tbody/tr[3]/td/table/tbody"));

                                }
                                catch { }
                                try
                                {
                                    if (IsecureTax == null)
                                    {
                                        IsecureTax = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[1]/table[1]/tbody/tr[4]/td/table/tbody"));
                                    }
                                }
                                catch { }

                                IList<IWebElement> IsecureTaxRow = IsecureTax.FindElements(By.TagName("tr"));
                                IList<IWebElement> IsecureTaxTd;
                                foreach (IWebElement secureTax in IsecureTaxRow)
                                {
                                    IsecureTaxTd = secureTax.FindElements(By.TagName("td"));
                                    if (IsecureTaxTd.Count != 0 && secureTax.Text.Contains("Secured Property Tax"))
                                    {
                                        strTaxType = IsecureTaxTd[0].Text.Trim();
                                    }
                                    if (IsecureTaxTd.Count != 0 && !secureTax.Text.Contains("Secured Property Tax") && !secureTax.Text.Contains("Parcel") && !secureTax.Text.Contains("Be aware that") && secureTax.Text.Trim() != "")
                                    {
                                        if (IsecureTaxTd.Count == 8 && secureTax.Text.Contains("General Tax"))
                                        {
                                            strParcelNo = IsecureTaxTd[0].Text;
                                            parcelNumber = IsecureTaxTd[0].Text;
                                            strTaxRate = IsecureTaxTd[1].Text;
                                            AssessmentYear = IsecureTaxTd[2].Text;
                                            RollYear = IsecureTaxTd[3].Text;
                                            strInstallmentType = IsecureTaxTd[4].Text;
                                            strFirtsInstallment = IsecureTaxTd[5].Text;
                                            strSecondInstallment = IsecureTaxTd[6].Text;
                                            strInstallmentTotal = IsecureTaxTd[7].Text;
                                            gc.CreatePdf(orderNumber, strParcelNo, "Secure Tax" + i, driver, "CA", "San Mateo");
                                            string GeneralTaxDetails = strInstallmentType + "~" + strFirtsInstallment + "~" + strSecondInstallment + "~" + strInstallmentTotal;
                                            gc.insert_date(orderNumber, strParcelNo, 1880, strTaxType + "~" + RollYear + "~" + GeneralTaxDetails, 1, DateTime.Now);
                                        }
                                        if (IsecureTaxTd.Count == 6 && secureTax.Text.Contains("Total Special Charges"))
                                        {
                                            strInstallmentType = IsecureTaxTd[2].Text;
                                            strFirtsInstallment = IsecureTaxTd[3].Text;
                                            strSecondInstallment = IsecureTaxTd[4].Text;
                                            strInstallmentTotal = IsecureTaxTd[5].Text;

                                            string GeneralTaxDetails = strInstallmentType + "~" + strFirtsInstallment + "~" + strSecondInstallment + "~" + strInstallmentTotal;
                                            gc.insert_date(orderNumber, strParcelNo, 1880, strTaxType + "~" + RollYear + "~" + GeneralTaxDetails, 1, DateTime.Now);
                                        }
                                        if (IsecureTaxTd.Count == 5 && (secureTax.Text.Contains("Total Taxes") || secureTax.Text.Contains("Penalty") || secureTax.Text.Contains("Total Amount") || secureTax.Text.Contains("PAID DATE")))
                                        {
                                            if (!secureTax.Text.Contains("Penalty") || secureTax.Text.Contains("Total Amount"))
                                            {
                                                OwnerAddress += IsecureTaxTd[0].Text.Trim();
                                            }
                                            if (secureTax.Text.Contains("PAID DATE"))
                                            {
                                                PropertyAddress += IsecureTaxTd[0].Text.Trim() + " ";
                                            }
                                            strInstallmentType = IsecureTaxTd[1].Text;
                                            strFirtsInstallment = IsecureTaxTd[2].Text;
                                            strSecondInstallment = IsecureTaxTd[3].Text;
                                            strInstallmentTotal = IsecureTaxTd[4].Text;

                                            string GeneralTaxDetails = strInstallmentType + "~" + strFirtsInstallment + "~" + strSecondInstallment + "~" + strInstallmentTotal;
                                            gc.insert_date(orderNumber, strParcelNo, 1880, strTaxType + "~" + RollYear + "~" + GeneralTaxDetails, 1, DateTime.Now);
                                        }
                                        if (IsecureTaxTd.Count == 5 && secureTax.Text.Contains("Due Date"))
                                        {
                                            strInstallmentType = GlobalClass.Before(IsecureTaxTd[1].Text, "\r\n");
                                            strFirtsInstallment = GlobalClass.Before(IsecureTaxTd[2].Text, "\r\n");
                                            strSecondInstallment = GlobalClass.Before(IsecureTaxTd[3].Text, "\r\n");
                                            strInstallmentTotal = GlobalClass.Before(IsecureTaxTd[4].Text, "\r\n");

                                            string GeneralTaxDetails1 = strInstallmentType + "~" + strFirtsInstallment + "~" + strSecondInstallment + "~" + strInstallmentTotal;
                                            gc.insert_date(orderNumber, strParcelNo, 1880, strTaxType + "~" + RollYear + "~" + GeneralTaxDetails1, 1, DateTime.Now);

                                            strInstallmentType = GlobalClass.After(IsecureTaxTd[1].Text, "\r\n");
                                            strFirtsInstallment = GlobalClass.After(IsecureTaxTd[2].Text, "\r\n");
                                            strSecondInstallment = GlobalClass.After(IsecureTaxTd[3].Text, "\r\n");
                                            strInstallmentTotal = GlobalClass.After(IsecureTaxTd[4].Text, "\r\n");

                                            string GeneralTaxDetails2 = strInstallmentType + "~" + strFirtsInstallment + "~" + strSecondInstallment + "~" + strInstallmentTotal;
                                            gc.insert_date(orderNumber, strParcelNo, 1880, strTaxType + "~" + RollYear + "~" + GeneralTaxDetails2, 1, DateTime.Now);
                                        }
                                        if (IsecureTaxTd.Count == 4 && secureTax.Text.Contains("Detail Special Charges"))
                                        {
                                            PropertyAddress += IsecureTaxTd[0].Text.Trim();
                                        }

                                        if (IsecureTaxTd.Count == 5 && secureTax.Text.Contains("Values"))
                                        {
                                            SpecialCharges = IsecureTaxTd[2].Text;
                                            PhoneContact = IsecureTaxTd[3].Text;
                                            Amount = IsecureTaxTd[4].Text;

                                            string strSpecialCharges = strTaxType + "~" + RollYear + "~" + SpecialCharges + "~" + PhoneContact + "~" + Amount;
                                            gc.insert_date(orderNumber, strParcelNo, 1877, strSpecialCharges, 1, DateTime.Now);
                                        }
                                        if (IsecureTaxTd.Count == 5 && secureTax.Text.Contains("Improvements"))
                                        {
                                            strImprovement = IsecureTaxTd[1].Text;
                                            SpecialCharges = IsecureTaxTd[2].Text;
                                            PhoneContact = IsecureTaxTd[3].Text;
                                            Amount = IsecureTaxTd[4].Text;

                                            string strSpecialCharges = strTaxType + "~" + RollYear + "~" + SpecialCharges + "~" + PhoneContact + "~" + Amount;
                                            gc.insert_date(orderNumber, strParcelNo, 1877, strSpecialCharges, 1, DateTime.Now);
                                        }
                                        if (IsecureTaxTd.Count == 3 && secureTax.Text.Contains("Land"))
                                        {
                                            strLand = IsecureTaxTd[1].Text;
                                        }
                                        if (IsecureTaxTd.Count == 6 && secureTax.Text.Contains("Composite Rate"))
                                        {
                                            strExemptions = IsecureTaxTd[1].Text;
                                            CompositeRate = IsecureTaxTd[3].Text;
                                            PenaltyRate = IsecureTaxTd[5].Text;
                                        }
                                        if (IsecureTaxTd.Count == 3 && secureTax.Text.Contains("Net value"))
                                        {
                                            NetValue = IsecureTaxTd[1].Text;
                                        }
                                        if (IsecureTaxTd.Count == 2 && secureTax.Text.Contains("Legal Description"))
                                        {
                                            LegalDescription = IsecureTaxTd[1].Text;
                                        }
                                    }
                                }

                                string PropertyDetails = PropertyAddress + "~" + OwnerAddress + "~" + RollYear + "~" + AssessmentYear + "~" + strTaxRate + "~" + LegalDescription + "~" + "" + "~" + "";
                                gc.insert_date(orderNumber, strParcelNo, 1874, PropertyDetails, 1, DateTime.Now);
                                //db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + strInstallmentType.Remove(strInstallmentType.Length - 1, 1) + "' where Id = '" + 1880 + "'");
                                //gc.insert_date(orderNumber, strParcelNo, 1880, strTaxType + "~" + RollYear + "~" + strFirtsInstallment.Remove(strFirtsInstallment.Length - 1, 1), 1, DateTime.Now);
                                //gc.insert_date(orderNumber, strParcelNo, 1880, strTaxType + "~" + RollYear + "~" + strSecondInstallment.Remove(strSecondInstallment.Length - 1, 1), 1, DateTime.Now);
                                //gc.insert_date(orderNumber, strParcelNo, 1880, strTaxType + "~" + RollYear + "~" + strInstallmentTotal.Remove(strInstallmentTotal.Length - 1, 1), 1, DateTime.Now);
                                string SupplimentDetails = strTaxType + "~" + RollYear + "~" + "" + "~" + strLand + "~" + strImprovement + "~" + "" + "~" + strExemptions + "~" + "~" + NetValue + "~" + CompositeRate + "~" + PenaltyRate;
                                gc.insert_date(orderNumber, strParcelNo, 1875, SupplimentDetails, 1, DateTime.Now);
                            }
                            catch { }

                            try
                            {
                                driver.FindElement(By.LinkText("General Tax")).Click();
                                gc.CreatePdf(orderNumber, strParcelNo, "Secure General Tax" + i, driver, "CA", "San Mateo");
                                string TaxingAgency = "", TaxRate = "", TaxingAmount = "";
                                IWebElement ITaxing = driver.FindElement(By.Id("taxes"));
                                IList<IWebElement> ITaxingRow = ITaxing.FindElements(By.TagName("tr"));
                                IList<IWebElement> ITaxingTd;
                                foreach (IWebElement taxing in ITaxingRow)
                                {
                                    ITaxingTd = taxing.FindElements(By.TagName("td"));
                                    if (ITaxingTd.Count != 0 && !taxing.Text.Contains("Taxing Agency"))
                                    {
                                        TaxingAgency = ITaxingTd[0].Text;
                                        TaxRate = ITaxingTd[1].Text;
                                        TaxingAmount = ITaxingTd[2].Text;

                                        string TaxingAgencyDetails = strTaxType + "~" + RollYear + "~" + TaxingAgency + "~" + TaxRate + "~" + TaxingAmount;
                                        gc.insert_date(orderNumber, strParcelNo, 1876, TaxingAgencyDetails, 1, DateTime.Now);
                                    }
                                }
                            }
                            catch { }

                            try
                            {
                                driver.FindElement(By.LinkText("More Special Charges")).Click();
                                gc.CreatePdf(orderNumber, strParcelNo, "Secure More Special Charges" + i, driver, "CA", "San Mateo");
                                string SpecialCharge = "", SCPhoneContact = "", SCAmount = "";
                                IWebElement ISpecialCharge = driver.FindElement(By.Id("charges"));
                                IList<IWebElement> ISpecialChargeRow = ISpecialCharge.FindElements(By.TagName("tr"));
                                IList<IWebElement> ISpecialChargeTd;
                                foreach (IWebElement charge in ISpecialChargeRow)
                                {
                                    ISpecialChargeTd = charge.FindElements(By.TagName("td"));
                                    if (ISpecialChargeTd.Count != 0 && !charge.Text.Contains("Special Charge"))
                                    {
                                        SpecialCharge = ISpecialChargeTd[0].Text;
                                        SCPhoneContact = ISpecialChargeTd[1].Text;
                                        SCAmount = ISpecialChargeTd[2].Text;

                                        string SpecialChargeDetails = strTaxType + "~" + RollYear + "~" + SpecialCharge + "~" + SCPhoneContact + "~" + SCAmount;
                                        gc.insert_date(orderNumber, strParcelNo, 1878, SpecialChargeDetails, 1, DateTime.Now);
                                    }
                                }
                                driver.SwitchTo().Window(currentWindow);
                            }
                            catch
                            {
                                driver.SwitchTo().Window(currentWindow);
                            }

                        }
                    }
                    catch { }


                    if (Default == "Delinquent")
                    {
                        try
                        {
                            driver.Navigate().GoToUrl("http://www.sanmateocountytaxcollector.org/SMCWPS/pages/secureSearch.jsp");
                            //driver.FindElement(By.LinkText("Parcel"));
                            Thread.Sleep(2000);
                            string strfirst = "", strmiddle = "", strlast = "";
                            if (parcelNumber.Contains("-"))
                            {
                                parcelNumber = parcelNumber.Replace("-", "");
                            }
                            if (parcelNumber.Count() == 9)
                            {
                                strfirst = parcelNumber.Substring(0, 3);
                                strmiddle = parcelNumber.Substring(3, 3);
                                strlast = parcelNumber.Substring(6, 3);
                            }
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form/table/tbody/tr[2]/td[1]/table/tbody/tr[3]/td/h3/input[1]")).SendKeys(strfirst);
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form/table/tbody/tr[2]/td[1]/table/tbody/tr[3]/td/h3/input[2]")).SendKeys(strmiddle);
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form/table/tbody/tr[2]/td[1]/table/tbody/tr[3]/td/h3/input[3]")).SendKeys(strlast);
                            try
                            {
                                driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form/table/tbody/tr[2]/td[1]/table/tbody/tr[4]/td/a[1]")).SendKeys(Keys.Enter);
                                Thread.Sleep(2000);
                            }
                            catch { }
                            IWebElement IDefaultParcel = driver.FindElement(By.XPath("//*[@id='listTbl']/tbody"));
                            IList<IWebElement> IDefaultParcelRow = IDefaultParcel.FindElements(By.TagName("tr"));
                            IList<IWebElement> IDefaultParcelTD;
                            foreach (IWebElement defaultTax in IDefaultParcelRow)
                            {
                                IDefaultParcelTD = defaultTax.FindElements(By.TagName("td"));
                                if (IDefaultParcelTD.Count != 0 && defaultTax.Text.Contains("Tax Defaulted Property"))
                                {
                                    IWebElement IDefaultClick = IDefaultParcelTD[0].FindElement(By.TagName("a"));
                                    if (IDefaultClick.Text != "")
                                    {
                                        IDefaultClick.SendKeys(Keys.Enter);
                                        driver.SwitchTo().Window(driver.WindowHandles.Last());
                                        string strDefaultTax = "", GoodThrough = "", strDParcel = "", strDRollYear = "", strDTotalTax = "", strDelinqPenalty = "", strDCost = "", strDRedemption = "", strDtotalCharges = "";
                                        string strDLastDate = "", strDPayDue = "", strTotalRedeem = "", strDInterest = "", strDFees = "", strDCredit = "";
                                        IWebElement IDefaultTax = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/table[2]"));
                                        IList<IWebElement> IDefaultTaxRow = IDefaultTax.FindElements(By.TagName("tr"));
                                        IList<IWebElement> IDefaultTaxTD;
                                        foreach (IWebElement taxes in IDefaultTaxRow)
                                        {
                                            IDefaultTaxTD = taxes.FindElements(By.TagName("td"));
                                            if (IDefaultTaxTD.Count != 0 && taxes.Text.Contains("Tax Defaulted") && !taxes.Text.Contains("TOTAL TO REDEEM"))
                                            {
                                                strDefaultTax = IDefaultTaxTD[0].Text;
                                                GoodThrough = GlobalClass.After(IDefaultTaxTD[1].Text, "good through").Trim();
                                            }
                                            if (IDefaultTaxTD.Count != 0 && !taxes.Text.Contains("Tax Defaulted") && !taxes.Text.Contains("Parcel") && !taxes.Text.Contains("Roll Year") && !taxes.Text.Contains("Plans"))
                                            {
                                                if (IDefaultTaxTD.Count == 4 && !taxes.Text.Contains("TOTAL TO REDEEM"))
                                                {
                                                    strDParcel = IDefaultTaxTD[0].Text;
                                                    gc.CreatePdf(orderNumber, strDParcel, "Defaulted Tax", driver, "CA", "San Mateo");
                                                }
                                                if (IDefaultTaxTD.Count == 4 && taxes.Text.Contains("TOTAL TO REDEEM"))
                                                {
                                                    strDLastDate = IDefaultTaxTD[1].Text;
                                                    strTotalRedeem = IDefaultTaxTD[3].Text;
                                                }
                                                if (IDefaultTaxTD.Count == 6)
                                                {
                                                    strDRollYear = IDefaultTaxTD[0].Text;
                                                    strDTotalTax = IDefaultTaxTD[1].Text;
                                                    strDelinqPenalty = IDefaultTaxTD[2].Text;
                                                    strDCost = IDefaultTaxTD[3].Text;
                                                    strDRedemption = IDefaultTaxTD[4].Text;
                                                    strDtotalCharges = IDefaultTaxTD[5].Text;

                                                    string strRollYearDetails = strDefaultTax + "~" + strDRollYear + "~" + strDTotalTax + "~" + strDelinqPenalty + "~" + strDCost + "~" + strDRedemption + "~" + strDtotalCharges + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                                    gc.insert_date(orderNumber, strDParcel, 1964, strRollYearDetails, 1, DateTime.Now);
                                                }
                                                if (IDefaultTaxTD.Count == 3 && taxes.Text.Contains("Interest"))
                                                {
                                                    strDInterest = IDefaultTaxTD[2].Text;
                                                }
                                                if (IDefaultTaxTD.Count == 3 && taxes.Text.Contains("Fees"))
                                                {
                                                    strDFees = IDefaultTaxTD[2].Text;
                                                }
                                                if (IDefaultTaxTD.Count == 3 && taxes.Text.Contains("Credit to Principal"))
                                                {
                                                    strDCredit = IDefaultTaxTD[2].Text;
                                                }
                                            }
                                        }

                                        try
                                        {
                                            strDPayDue = GlobalClass.After(driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[2]")).Text, "Pay Total Due").Trim();
                                        }
                                        catch { }

                                        string strDueDetails = strDefaultTax + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + strDInterest + "~" + strDFees + "~" + strDCredit + "~" + strTotalRedeem + "~" + strDLastDate + "~" + strDPayDue + "~" + GoodThrough;
                                        gc.insert_date(orderNumber, strDParcel, 1964, strDueDetails, 1, DateTime.Now);
                                    }
                                }
                            }
                        }
                        catch { }
                    }

                    //Direct Xpath
                    //try
                    //{
                    //    IWebElement ISupplimentParcel = driver.FindElement(By.XPath("//*[@id='listTbl']/tbody"));
                    //    IList<IWebElement> ISupplimentParcelRow = ISupplimentParcel.FindElements(By.TagName("tr"));
                    //    IList<IWebElement> ISupplimentParcelTD;
                    //    IList<IWebElement> ISupplimentClick;
                    //    foreach (IWebElement Suppliment in ISupplimentParcelRow)
                    //    {
                    //        ISupplimentParcelTD = Suppliment.FindElements(By.TagName("td"));
                    //        if (ISupplimentParcelTD.Count != 0 && Suppliment.Text.Contains("Supplemental Property"))
                    //        {
                    //            ISupplimentClick = ISupplimentParcelTD[0].FindElements(By.TagName("a"));
                    //            foreach (IWebElement click in ISupplimentClick)
                    //            {
                    //                if (click.Text.Trim() == "")
                    //                {
                    //                    click.SendKeys(Keys.Enter);
                    //                    driver.SwitchTo().Window(driver.WindowHandles.Last());
                    //                    string TaxType = "", ParcelNo = "", RollYear = "", FiscalYear = "", TaxRate = "", ValueDate = "", OriginalBill = "", OwnerAddress = "", PropertyAddress = "";
                    //                    string InstallmentType = "", FirstInstallment = "", SecondInstallment = "", InstallmentTotal = "";
                    //                    string BaseValue = "", Land = "", Improvement = "", PersonalProperty = "", Exemptions = "", NetCash = "", CompositeRate = "", PenaltyRate = "";
                    //                    TaxType = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[3]/td/table/tbody/tr/td/form/table[2]/tbody/tr[1]/td[1]")).Text;
                    //                    ParcelNo = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[3]/td/table/tbody/tr/td/form/table[2]/tbody/tr[3]/td[1]")).Text;
                    //                    RollYear = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[3]/td/table/tbody/tr/td/form/table[2]/tbody/tr[3]/td[2]")).Text;
                    //                    FiscalYear = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[3]/td/table/tbody/tr/td/form/table[2]/tbody/tr[3]/td[3]")).Text;
                    //                    TaxRate = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[3]/td/table/tbody/tr/td/form/table[2]/tbody/tr[3]/td[4]")).Text;
                    //                    ValueDate = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[3]/td/table/tbody/tr/td/form/table[2]/tbody/tr[3]/td[5]")).Text;
                    //                    OriginalBill = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[3]/td/table/tbody/tr/td/form/table[2]/tbody/tr[3]/td[6]")).Text;
                    //                    OwnerAddress = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[3]/td/table/tbody/tr/td/form/table[2]/tbody/tr[6]/td[1]")).Text;
                    //                    OwnerAddress += driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[3]/td/table/tbody/tr/td/form/table[2]/tbody/tr[7]/td[1]")).Text;
                    //                    PropertyAddress = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[3]/td/table/tbody/tr/td/form/table[2]/tbody/tr[9]/td[1]")).Text;
                    //                    PropertyAddress += driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[3]/td/table/tbody/tr/td/form/table[2]/tbody/tr[10]/td[1]")).Text;
                    //                    gc.CreatePdf(orderNumber, ParcelNo, "Suppliment Tax" + RollYear, driver, "CA", "San Mateo");
                    //                    string strPropertyDetails = PropertyAddress + "~" + OwnerAddress + "~" + RollYear + "~" + FiscalYear + "~" + TaxRate + "~" + "" + "~" + ValueDate + "~" + OriginalBill;
                    //                    gc.insert_date(orderNumber, ParcelNo, 1874, strPropertyDetails, 1, DateTime.Now);

                    //                    try
                    //                    {
                    //                        InstallmentType = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[3]/td/table/tbody/tr/td/form/table[2]/tbody/tr[4]/td[3]")).Text;
                    //                        FirstInstallment = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[3]/td/table/tbody/tr/td/form/table[2]/tbody/tr[5]/td[2]")).Text;

                    //                        string GeneralTaxDetails = InstallmentType + "~" + FirstInstallment + "~" + "" + "~" + "";
                    //                        gc.insert_date(orderNumber, ParcelNo, 1880, TaxType + "~" + RollYear + "~" + GeneralTaxDetails, 1, DateTime.Now);
                    //                    }
                    //                    catch { }

                    //                    try
                    //                    {
                    //                        InstallmentType = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[3]/td/table/tbody/tr/td/form/table[2]/tbody/tr[4]/td[4]")).Text;
                    //                        FirstInstallment = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[3]/td/table/tbody/tr/td/form/table[2]/tbody/tr[5]/td[3]")).Text;

                    //                        string GeneralTaxDetails = InstallmentType + "~" + FirstInstallment + "~" + "" + "~" + "";
                    //                        gc.insert_date(orderNumber, ParcelNo, 1880, TaxType + "~" + RollYear + "~" + GeneralTaxDetails, 1, DateTime.Now);
                    //                    }
                    //                    catch { }

                    //                    for (int i = 7; i < 11; i++)
                    //                    {
                    //                        if (i == 6)
                    //                        {
                    //                            InstallmentType = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[3]/td/table/tbody/tr/td/form/table[2]/tbody/tr[" + i + "]/td[1]")).Text;
                    //                            FirstInstallment = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[3]/td/table/tbody/tr/td/form/table[2]/tbody/tr[" + i + "]/td[2]")).Text;
                    //                            SecondInstallment = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[3]/td/table/tbody/tr/td/form/table[2]/tbody/tr[" + i + "]/td[3]")).Text;
                    //                            InstallmentTotal = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[3]/td/table/tbody/tr/td/form/table[2]/tbody/tr[" + i + "]/td[4]")).Text;
                    //                        }
                    //                        else
                    //                        {
                    //                            InstallmentType = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[3]/td/table/tbody/tr/td/form/table[2]/tbody/tr[" + i + "]/td[2]")).Text;
                    //                            FirstInstallment = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[3]/td/table/tbody/tr/td/form/table[2]/tbody/tr[" + i + "]/td[3]")).Text;
                    //                            SecondInstallment = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[3]/td/table/tbody/tr/td/form/table[2]/tbody/tr[" + i + "]/td[4]")).Text;
                    //                            InstallmentTotal = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[3]/td/table/tbody/tr/td/form/table[2]/tbody/tr[" + i + "]/td[5]")).Text;
                    //                        }
                    //                        string GeneralTaxDetails = InstallmentType + "~" + FirstInstallment + "~" + SecondInstallment + "~" + InstallmentTotal;
                    //                        gc.insert_date(orderNumber, ParcelNo, 1880, TaxType + "~" + RollYear + "~" + GeneralTaxDetails, 1, DateTime.Now);
                    //                    }

                    //                    for (int k = 12; k < 15; k++)
                    //                    {
                    //                        BaseValue = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[3]/td/table/tbody/tr/td/form/table[2]/tbody/tr[" + k + "]/td[1]")).Text;
                    //                        Land = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[3]/td/table/tbody/tr/td/form/table[2]/tbody/tr[" + k + "]/td[2]")).Text;
                    //                        Improvement = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[3]/td/table/tbody/tr/td/form/table[2]/tbody/tr[" + k + "]/td[3]")).Text;
                    //                        PersonalProperty = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[3]/td/table/tbody/tr/td/form/table[2]/tbody/tr[" + k + "]/td[4]")).Text;
                    //                        Exemptions = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[3]/td/table/tbody/tr/td/form/table[2]/tbody/tr[" + k + "]/td[5]")).Text;
                    //                        NetCash = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[3]/td/table/tbody/tr/td/form/table[2]/tbody/tr[" + k + "]/td[6]")).Text;
                    //                        CompositeRate = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[3]/td/table/tbody/tr/td/form/table[2]/tbody/tr[" + k + "]/td[7]")).Text;
                    //                        PenaltyRate = "";
                    //                        if (k == 12)
                    //                        {
                    //                            CompositeRate = "";
                    //                            PenaltyRate = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr[3]/td/table/tbody/tr/td/form/table[2]/tbody/tr[" + k + "]/td[7]")).Text;
                    //                        }

                    //                        string SupplimentDetails = TaxType + "~" + RollYear + "~" + BaseValue + "~" + Land + "~" + Improvement + "~" + PersonalProperty + "~" + Exemptions + "~" + NetCash + "~" + CompositeRate + "~" + PenaltyRate;
                    //                        gc.insert_date(orderNumber, ParcelNo, 1875, SupplimentDetails, 1, DateTime.Now);
                    //                    }

                    //                    driver.SwitchTo().Window(currentWindow);
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                    //catch { }

                    //try
                    //{
                    //    driver.SwitchTo().Window(currentWindow);
                    //    IWebElement ISecureParcel = driver.FindElement(By.XPath("//*[@id='listTbl']/tbody"));
                    //    IList<IWebElement> ISecureParcelRow = ISecureParcel.FindElements(By.TagName("tr"));
                    //    IList<IWebElement> ISecureParcelTD;
                    //    foreach (IWebElement Secure in ISecureParcelRow)
                    //    {
                    //        ISecureParcelTD = Secure.FindElements(By.TagName("td"));
                    //        if (ISecureParcelTD.Count != 0 && Secure.Text.Contains("Secured Property"))
                    //        {
                    //            IWebElement IsecureClick = ISecureParcelTD[0].FindElement(By.TagName("a"));
                    //            if (IsecureClick.Text != "")
                    //            {
                    //                IsecureClick.SendKeys(Keys.Enter);
                    //                break;
                    //            }
                    //        }
                    //    }

                    //    for (int i = 0; i < 3; i++)
                    //    {
                    //        string strTaxType = "", strParcelNo = "", strTaxRate = "", AssessmentYear = "", RollYear = "", OwnerAddress = "", PropertyAddress = "", LegalDescription = "";
                    //        string strLand = "", strImprovement = "", strExemptions = "", CompositeRate = "", PenaltyRate = "", NetValue = "";
                    //        string strInstallmentType="",strFirtsInstallment = "", strSecondInstallment = "", strInstallmentTotal = "";
                    //        string SpecialCharges = "", PhoneContact = "", Amount = "";
                    //        try
                    //        {
                    //            IWebElement ISelectYear = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[1]/table[1]/tbody/tr[3]/td/table/tbody/tr[1]/td[2]/select"));
                    //            SelectElement sSelectYear = new SelectElement(ISelectYear);
                    //            sSelectYear.SelectByIndex(i);
                    //            strTaxType = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[1]/table[1]/tbody/tr[3]/td/table/tbody/tr[1]/td[1]")).Text;
                    //            strParcelNo = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[1]/table[1]/tbody/tr[3]/td/table/tbody/tr[3]/td[1]")).Text;
                    //            strTaxRate = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[1]/table[1]/tbody/tr[3]/td/table/tbody/tr[3]/td[2]")).Text;
                    //            AssessmentYear = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[1]/table[1]/tbody/tr[3]/td/table/tbody/tr[3]/td[3]")).Text;
                    //            RollYear = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[1]/table[1]/tbody/tr[3]/td/table/tbody/tr[3]/td[4]")).Text;
                    //            OwnerAddress = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[1]/table[1]/tbody/tr[3]/td/table/tbody/tr[6]/td[1]")).Text;
                    //            OwnerAddress += driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[1]/table[1]/tbody/tr[3]/td/table/tbody/tr[7]/td[1]")).Text;
                    //            PropertyAddress = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[1]/table[1]/tbody/tr[3]/td/table/tbody/tr[9]/td[1]")).Text;
                    //            PropertyAddress += driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[1]/table[1]/tbody/tr[3]/td/table/tbody/tr[10]/td[1]")).Text;

                    //            strLand = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[1]/table[1]/tbody/tr[3]/td/table/tbody/tr[13]/td[2]")).Text;
                    //            strImprovement = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[1]/table[1]/tbody/tr[3]/td/table/tbody/tr[12]/td[2]")).Text;
                    //            strExemptions = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[1]/table[1]/tbody/tr[3]/td/table/tbody/tr[14]/td[2]")).Text;
                    //            CompositeRate = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[1]/table[1]/tbody/tr[3]/td/table/tbody/tr[14]/td[4]")).Text;
                    //            PenaltyRate = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[1]/table[1]/tbody/tr[3]/td/table/tbody/tr[14]/td[6]")).Text;
                    //            NetValue = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[1]/table[1]/tbody/tr[3]/td/table/tbody/tr[16]/td[2]")).Text;
                    //            LegalDescription = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[1]/table[1]/tbody/tr[3]/td/table/tbody/tr[17]/td[2]")).Text;

                    //            for (int k = 3; k < 10; k++)
                    //            {
                    //                if (k > 4)
                    //                {
                    //                    strInstallmentType = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[1]/table[1]/tbody/tr[3]/td/table/tbody/tr[" + k + "]/td[2]")).Text;
                    //                    strFirtsInstallment = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[1]/table[1]/tbody/tr[3]/td/table/tbody/tr[" + k + "]/td[3]")).Text;
                    //                    strSecondInstallment = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[1]/table[1]/tbody/tr[3]/td/table/tbody/tr[" + k + "]/td[4]")).Text;
                    //                    strInstallmentTotal = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[1]/table[1]/tbody/tr[3]/td/table/tbody/tr[" + k + "]/td[5]")).Text;

                    //                    string GeneralTaxDetails = strInstallmentType + "~" + strFirtsInstallment + "~" + strSecondInstallment + "~" + strInstallmentTotal;
                    //                    gc.insert_date(orderNumber, strParcelNo, 1880, strTaxType + "~" + RollYear + "~" + GeneralTaxDetails, 1, DateTime.Now);
                    //                }
                    //                if (k == 3)
                    //                {
                    //                    strInstallmentType = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[1]/table[1]/tbody/tr[3]/td/table/tbody/tr[" + k + "]/td[5]")).Text;
                    //                    strFirtsInstallment = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[1]/table[1]/tbody/tr[3]/td/table/tbody/tr[" + k + "]/td[6]")).Text;
                    //                    strSecondInstallment = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[1]/table[1]/tbody/tr[3]/td/table/tbody/tr[" + k + "]/td[7]")).Text;
                    //                    strInstallmentTotal = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[1]/table[1]/tbody/tr[3]/td/table/tbody/tr[" + k + "]/td[8]")).Text;

                    //                    string GeneralTaxDetails = strInstallmentType + "~" + strFirtsInstallment + "~" + strSecondInstallment + "~" + strInstallmentTotal;
                    //                    gc.insert_date(orderNumber, strParcelNo, 1880, strTaxType + "~" + RollYear + "~" + GeneralTaxDetails, 1, DateTime.Now);
                    //                }
                    //                if (k == 4)
                    //                {
                    //                    strInstallmentType = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[1]/table[1]/tbody/tr[3]/td/table/tbody/tr[" + k + "]/td[3]")).Text;
                    //                    strFirtsInstallment = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[1]/table[1]/tbody/tr[3]/td/table/tbody/tr[" + k + "]/td[4]")).Text;
                    //                    strSecondInstallment = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[1]/table[1]/tbody/tr[3]/td/table/tbody/tr[" + k + "]/td[5]")).Text;
                    //                    strInstallmentTotal = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[1]/table[1]/tbody/tr[3]/td/table/tbody/tr[" + k + "]/td[6]")).Text;

                    //                    string GeneralTaxDetails = strInstallmentType + "~" + strFirtsInstallment + "~" + strSecondInstallment + "~" + strInstallmentTotal;
                    //                    gc.insert_date(orderNumber, strParcelNo, 1880, strTaxType + "~" + RollYear + "~" + GeneralTaxDetails, 1, DateTime.Now);
                    //                }
                    //            }
                    //            for (int j = 11; j < 13; j++)
                    //            {
                    //                SpecialCharges = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[1]/table[1]/tbody/tr[3]/td/table/tbody/tr[" + j + "]/td[3]")).Text;
                    //                PhoneContact = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[1]/table[1]/tbody/tr[3]/td/table/tbody/tr[" + j + "]/td[4]")).Text;
                    //                Amount = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td/form[5]/div[1]/table[1]/tbody/tr[3]/td/table/tbody/tr[" + j + "]/td[5]")).Text;

                    //                string SpecialChargesDetails = strTaxType + "~" + RollYear + "~" + SpecialCharges + "~" + PhoneContact + "~" + Amount;
                    //                gc.insert_date(orderNumber, strParcelNo, 1877, SpecialChargesDetails, 1, DateTime.Now);
                    //            }

                    //            string PropertyDetails = PropertyAddress + "~" + OwnerAddress + "~" + RollYear + "~" + AssessmentYear + "~" + strTaxRate + "~" + LegalDescription + "~" + "" + "~" + "";
                    //            gc.insert_date(orderNumber, strParcelNo, 1874, PropertyDetails, 1, DateTime.Now);
                    //            gc.insert_date(orderNumber, strParcelNo, 1880, strTaxType + "~" + RollYear + "~" + strFirtsInstallment.Remove(strFirtsInstallment.Length - 1, 1), 1, DateTime.Now);
                    //            gc.insert_date(orderNumber, strParcelNo, 1880, strTaxType + "~" + RollYear + "~" + strSecondInstallment.Remove(strSecondInstallment.Length - 1, 1), 1, DateTime.Now);
                    //            gc.insert_date(orderNumber, strParcelNo, 1880, strTaxType + "~" + RollYear + "~" + strInstallmentTotal.Remove(strInstallmentTotal.Length - 1, 1), 1, DateTime.Now);
                    //            string SupplimentDetails = strTaxType + "~" + RollYear + "~" + "" + "~" + strLand + "~" + strImprovement + "~" + "" + "~" + strExemptions + "~" + NetValue + "~" + CompositeRate + "~" + PenaltyRate;
                    //            gc.insert_date(orderNumber, strParcelNo, 1875, SupplimentDetails, 1, DateTime.Now);
                    //        }
                    //        catch { }

                    //        try
                    //        {
                    //            driver.FindElement(By.Id("General Tax")).Click();
                    //            gc.CreatePdf(orderNumber, strParcelNo, "Secure General Tax" + i, driver, "CA", "San Mateo");
                    //            string TaxingAgency = "", TaxRate = "", TaxingAmount = "";
                    //            IWebElement ITaxing = driver.FindElement(By.Id("taxes"));
                    //            IList<IWebElement> ITaxingRow = ITaxing.FindElements(By.TagName("tr"));
                    //            IList<IWebElement> ITaxingTd;
                    //            foreach (IWebElement taxing in ITaxingRow)
                    //            {
                    //                ITaxingTd = taxing.FindElements(By.TagName("td"));
                    //                if (ITaxingTd.Count != 0 && !taxing.Text.Contains("Taxing Agency"))
                    //                {
                    //                    TaxingAgency = ITaxingTd[0].Text;
                    //                    TaxRate = ITaxingTd[1].Text;
                    //                    TaxingAmount = ITaxingTd[2].Text;

                    //                    string TaxingAgencyDetails = strTaxType + "~" + RollYear + "~" + TaxingAgency + "~" + TaxRate + "~" + TaxingAmount;
                    //                    gc.insert_date(orderNumber, strParcelNo, 1876, TaxingAgencyDetails, 1, DateTime.Now);
                    //                }
                    //            }
                    //            driver.SwitchTo().Window(currentWindow);
                    //        }
                    //        catch
                    //        {
                    //            driver.SwitchTo().Window(currentWindow);
                    //        }

                    //        try
                    //        {
                    //            driver.FindElement(By.Id("More Special Charges")).Click();
                    //            gc.CreatePdf(orderNumber, strParcelNo, "Secure More Special Charges" + i, driver, "CA", "San Mateo");
                    //            string SpecialCharge = "", SCPhoneContact = "", SCAmount = "";
                    //            IWebElement ISpecialCharge = driver.FindElement(By.Id("charges"));
                    //            IList<IWebElement> ISpecialChargeRow = ISpecialCharge.FindElements(By.TagName("tr"));
                    //            IList<IWebElement> ISpecialChargeTd;
                    //            foreach (IWebElement charge in ISpecialChargeRow)
                    //            {
                    //                ISpecialChargeTd = charge.FindElements(By.TagName("td"));
                    //                if (ISpecialChargeTd.Count != 0 && !charge.Text.Contains("Special Charge"))
                    //                {
                    //                    SpecialCharge = ISpecialChargeTd[0].Text;
                    //                    SCPhoneContact = ISpecialChargeTd[0].Text;
                    //                    SCAmount = ISpecialChargeTd[0].Text;

                    //                    string SpecialChargeDetails = strTaxType + "~" + RollYear + "~" + SpecialCharge + "~" + SCPhoneContact + "~" + SCAmount;
                    //                    gc.insert_date(orderNumber, strParcelNo, 1878, SpecialChargeDetails, 1, DateTime.Now);
                    //                }
                    //            }
                    //            driver.SwitchTo().Window(currentWindow);
                    //        }
                    //        catch
                    //        {
                    //            driver.SwitchTo().Window(currentWindow);
                    //        }

                    //    }
                    //}
                    //catch { }





                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "CA", "San Mateo", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "CA", "San Mateo");
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