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
    public class WebDriver_JohnsonKS
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        public string FTP_JohnsonKS(string houseno, string sname, string stype, string account, string parcelNumber, string ownername, string searchType, string orderNumber, string direction)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            IJavaScriptExecutor js = driver as IJavaScriptExecutor;
            string strmulti = "";
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new PhantomJSDriver();
            // driver = new ChromeDriver();
            var option = new ChromeOptions();
            option.AddArgument("No-Sandbox");
            using (driver = new ChromeDriver(option))
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("https://land.jocogov.org/default.aspx");
                    Thread.Sleep(8000);
                    IWebElement iframset = driver.FindElement(By.Id("mapiframe"));
                    driver.SwitchTo().Frame(iframset);
                    gc.CreatePdf_WOP(orderNumber, "Iframe Switch", driver, "KS", "Johnson");
                    Thread.Sleep(3000);
                    string addAddress = "";
                    if (direction != "")
                    {
                        addAddress = houseno + " " + direction.ToUpper() + " " + sname.ToUpper() + " " + stype.ToUpper() + " " + account.ToUpper();
                    }
                    else
                    {
                        addAddress = houseno + " " + sname.ToUpper() + " " + stype.ToUpper() + " " + account.ToUpper();
                    }
                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, "", ownername.Replace(",", ""), addAddress.Trim(), "KS", "Johnson");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_JohnsonKS"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        //IWebElement InputAddress = driver.FindElement(By.Id("tbSearchID"));
                        //IList<IWebElement> InputAddressrow = InputAddress.FindElements(By.TagName("input"));
                        //IList<IWebElement> inputaddresstd;
                        //foreach (IWebElement Input in InputAddressrow)
                        //{

                        //    Input.Clear();
                        //}
                        driver.FindElement(By.Id("tbSearchID")).Clear();
                        driver.FindElement(By.Id("tbSearchID")).SendKeys(addAddress.Trim());
                        driver.FindElement(By.Id("btnSimpleSearch")).Click();
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Address click", driver, "KS", "Johnson");
                        try
                        {
                            driver.FindElement(By.Id("btnDisclaimerYes")).Click();
                            Thread.Sleep(8000);
                        }
                        catch { }
                        // gc.CreatePdf_WOP(orderNumber, "Address Alert click", driver, "KS", "Johnson");

                        try
                        {
                            Thread.Sleep(3000);
                            IWebElement IAddress = driver.FindElement(By.XPath("/html/body/ul"));
                            IList<IWebElement> IAddressRow = IAddress.FindElements(By.TagName("li"));
                            IList<IWebElement> IAddressTD;
                            foreach (IWebElement address in IAddressRow)
                            {
                                IAddressTD = address.FindElements(By.TagName("a"));
                                if (address.Text != "" && IAddressRow.Count < 2 && address.Text.Contains(houseno) && address.Text.Contains(sname.ToUpper()) && address.Text.Contains(stype.ToUpper()))
                                {
                                    IAddressTD[0].Click();
                                }
                            }

                        }
                        catch { }
                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "KS", "Johnson");
                        // driver.FindElement(By.Id("btnName")).SendKeys(Keys.Enter);

                        //try
                        //{
                        //    strmulti = driver.FindElement(By.Id("lblRecords")).Text;
                        //    if (Convert.ToInt32(strmulti) > 25)
                        //    {
                        //        HttpContext.Current.Session["multiParcel_Johnson_Maximum"] = "Maximum";
                        //        return "Maximum";
                        //    }
                        //    IWebElement Imultitable = driver.FindElement(By.XPath("//*[@id='grdResults']/tbody"));
                        //    IList<IWebElement> ImutiRow = Imultitable.FindElements(By.TagName("tr"));
                        //    IList<IWebElement> ImultiTD;
                        //    foreach (IWebElement multi in ImutiRow)
                        //    {
                        //        ImultiTD = multi.FindElements(By.TagName("td"));
                        //        if (ImultiTD.Count != 0 && !multi.Text.Contains("Address"))
                        //        {
                        //            string strmultiDetails = ImultiTD[0].Text + "~" + ImultiTD[2].Text + "~" + ImultiTD[3].Text;
                        //            gc.insert_date(orderNumber, ImultiTD[1].Text, 579, strmultiDetails, 1, DateTime.Now);
                        //        }
                        //    }
                        //    HttpContext.Current.Session["multiParcel_Johnson"] = "Yes";
                        //    driver.Quit();
                        //    return "Multiparcel";
                        //}
                        //catch { }
                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.Id("tbSearchID")).Clear();
                        driver.FindElement(By.Id("tbSearchID")).SendKeys(parcelNumber.Replace(".", "").Replace("-", "").Trim());
                        Thread.Sleep(3000);
                        try
                        {
                            IWebElement IParcel = driver.FindElement(By.XPath("/html/body/ul"));
                            IList<IWebElement> IParcelRow = IParcel.FindElements(By.TagName("li"));
                            IList<IWebElement> IParcelTD;
                            foreach (IWebElement parcel in IParcelRow)
                            {
                                IParcelTD = parcel.FindElements(By.TagName("a"));
                                if (parcel.Text != "" && IParcelRow.Count < 2 && parcel.Text.Contains(parcelNumber.Replace(".", "").Replace("-", "").Trim()))
                                {
                                    IParcelTD[0].Click();
                                }
                            }

                        }
                        catch { }
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "KS", "Johnson");
                        driver.FindElement(By.Id("btnName")).SendKeys(Keys.Enter);
                    }
                    else if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("tbSearchID")).Clear();
                        driver.FindElement(By.Id("tbSearchID")).SendKeys(ownername.Trim());
                        Thread.Sleep(3000);
                        try
                        {
                            IWebElement IOwner = driver.FindElement(By.XPath("/html/body/ul"));
                            IList<IWebElement> IOwnerRow = IOwner.FindElements(By.TagName("li"));
                            IList<IWebElement> IOwnerTD;
                            foreach (IWebElement OwnerName in IOwnerRow)
                            {
                                IOwnerTD = OwnerName.FindElements(By.TagName("a"));
                                if (OwnerName.Text != "" && IOwnerRow.Count < 2 && OwnerName.Text.Contains(ownername.ToUpper().Trim()))
                                {
                                    IOwnerTD[0].Click();
                                }
                            }

                        }
                        catch { }
                        gc.CreatePdf(orderNumber, parcelNumber, "Owner Search", driver, "KS", "Johnson");
                        driver.FindElement(By.Id("btnName")).SendKeys(Keys.Enter);
                    }

                    if (searchType == "kup")
                    {
                        driver.FindElement(By.Id("tbSearchID")).Clear();
                        driver.FindElement(By.Id("tbSearchID")).SendKeys(account.Replace(".", "").Replace("-", "").Trim());
                        Thread.Sleep(3000);
                        try
                        {
                            IWebElement IAccount = driver.FindElement(By.XPath("/html/body/ul"));
                            IList<IWebElement> IAccountRow = IAccount.FindElements(By.TagName("li"));
                            IList<IWebElement> IAccountTd;
                            foreach (IWebElement Accno in IAccountRow)
                            {
                                IAccountTd = Accno.FindElements(By.TagName("a"));
                                if (Accno.Text != "" && IAccountRow.Count < 2 && Accno.Text.Contains(account.Replace(".", "").Replace("-", "").Trim()))
                                {
                                    IAccountTd[0].Click();
                                }
                            }

                        }
                        catch { }
                        gc.CreatePdf(orderNumber, parcelNumber, "Account No Search", driver, "KS", "Johnson");
                        driver.FindElement(By.Id("btnName")).SendKeys(Keys.Enter);
                    }
                    string nodata = driver.FindElement(By.Id("spanTaxPropertyID")).Text;
                    if (nodata.Trim() == "")
                    {
                        HttpContext.Current.Session["Nodata_JohnsonKS"] = "Yes";
                        driver.Quit();
                        return "No Data Found";
                    }

                    //Property Details
                    string Address = "", ParcelId = "", KUP = "", QuickRef = "", Description = "", YearBuilt = "";
                    string Splitaddress1 = driver.FindElement(By.Id("spanSitAddline1")).Text;
                    string Splitaddress2 = driver.FindElement(By.Id("spanMailCityLine")).Text;
                    Address = Splitaddress1 + " " + Splitaddress2;
                    ParcelId = driver.FindElement(By.Id("spanTaxPropertyID")).Text.Trim().Replace(" ", "");
                    KUP = driver.FindElement(By.Id("spanKUPN")).Text;
                    QuickRef = driver.FindElement(By.Id("spanQuickRefID")).Text;
                    Description = driver.FindElement(By.Id("spanLegalDesc")).Text;
                    YearBuilt = driver.FindElement(By.Id("spanYearbuilt2")).Text;
                    string Block = driver.FindElement(By.Id("spanGeoBlock")).Text;
                    string Lot = driver.FindElement(By.Id("spanGeoLot")).Text;
                    string blocklot = Block + "/" + Lot;
                    string Subdivision = driver.FindElement(By.Id("spanSbdvName")).Text;
                    gc.CreatePdf(orderNumber, ParcelId, "Property Details", driver, "KS", "Johnson");
                    string Ownerid = driver.FindElement(By.Id("tblOwnerInfo")).Text;
                    string[] ownername11 = Ownerid.Split('\n');
                    string Ownersplit1 = ownername11[0].Replace("Owner 1: ", "");
                    string MailAddress =ownername11[1];
                    string Ownersplit2 = "";
                    try
                    {
                        Ownersplit2 = ownername11[3].Replace("Owner 2:", "");
                    }
                    catch { }
                    string Owner = Ownersplit1 + " " + Ownersplit2;
                    string zoning = driver.FindElement(By.Id("spanZoningDescription")).Text;
                    string propertytype = driver.FindElement(By.Id("spanPrPlTypDsc")).Text;
                    string Taxunit = driver.FindElement(By.Id("spanTaxUnit")).Text;
                    string PropertyDetails = Address + "~" + KUP + "~" + QuickRef + "~" + blocklot + "~" + Subdivision + "~" + MailAddress + "~" + Owner + "~" + zoning + "~" + propertytype + "~" + Taxunit + "~" + Description + "~" + YearBuilt;
                    gc.insert_date(orderNumber, ParcelId, 590, PropertyDetails, 1, DateTime.Now);
                    ByVisibleElement(driver.FindElement(By.XPath("//*[@id='divOwnerInfo']/div/div[1]/table/tbody/tr/td")));
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, ParcelId, "Property Details1", driver, "KS", "Johnson");
                    //Assessment Details
                    string AssesseedDetails = "", strAssessType = "";
                    //  strAssessType = driver.FindElement(By.XPath("//*[@id='Form1']/div/div[8]/div")).Text;
                    ByVisibleElement(driver.FindElement(By.XPath("//*[@id='aprdetailsquarebox']/div[1]/table/tbody/tr/td")));
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, ParcelId, "Appraisal Information", driver, "KS", "Johnson");
                    IWebElement IAssessTable = driver.FindElement(By.XPath("//*[@id='divAPRDetail']/table[1]/tbody"));
                    IList<IWebElement> IAssessRow = IAssessTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> IAssessTD;
                    foreach (IWebElement assess in IAssessRow)
                    {
                        IAssessTD = assess.FindElements(By.TagName("td"));
                        if (IAssessTD.Count != 0)
                        {
                            AssesseedDetails = IAssessTD[0].Text + "~" + IAssessTD[1].Text + "~" + IAssessTD[2].Text;
                            gc.insert_date(orderNumber, ParcelId, 591, AssesseedDetails, 1, DateTime.Now);
                        }
                    }
                    ByVisibleElement(driver.FindElement(By.Id("spanaprcomponents")));
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, ParcelId, "Sub Division", driver, "KS", "Johnson");
                    ByVisibleElement(driver.FindElement(By.XPath("//*[@id='tblInfo']/div[1]/div/div[8]/div[1]/table/tbody")));
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, ParcelId, "utility", driver, "KS", "Johnson");
                    ByVisibleElement(driver.FindElement(By.XPath("//*[@id='tblInfo']/div[1]/div/div[10]/div[1]/table/tbody")));
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, ParcelId, "Census", driver, "KS", "Johnson");
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    //ViwePRC
                    //try
                    //{
                    //    IWebElement IViewPRC = driver.FindElement(By.LinkText("View PRC"));
                    //    string strViewLink = IViewPRC.GetAttribute("href");
                    //    gc.downloadfile(strViewLink, orderNumber, ParcelId, "Bill_PDF", "KS", "Johnson");
                    //}
                    //catch { }

                    //Tax Information
                    driver.Navigate().GoToUrl("https://taxbill.jocogov.org/");

                    string AssessValue = "", PropertyStatus = "", PropertyAddress = "", PropertyType = "", LegalDescription = "", TaxingUnit = "", Neighborhood = "", RETaxID = "", MapNO = "", TaxAuthority = "", strYear = "", TaxYear = "", strCurrentType = "", TaxInstallType = "";
                    driver.FindElement(By.Id("SearchText")).SendKeys(ParcelId.Replace(" ", ""));

                    //IWebElement ITaxBill = driver.FindElement(By.LinkText("Tax Bill"));
                    //string strTaxBill = ITaxBill.GetAttribute("href");
                    //driver.Navigate().GoToUrl(strTaxBill);
                    gc.CreatePdf(orderNumber, ParcelId, "Tax Search Result1", driver, "KS", "Johnson");
                    driver.FindElement(By.Id("dnn_PropertySearch_SearchButtonDiv")).Click();
                    Thread.Sleep(3000);
                    //js.ExecuteScript("arguments[0].click();", IDetails);
                    gc.CreatePdf(orderNumber, ParcelId, "Tax Search Result", driver, "KS", "Johnson");
                    driver.FindElement(By.XPath("//*[@id='grid']/div[2]/table/tbody/tr/td[2]")).Click();
                    Thread.Sleep(2000);
                    //dnn_ctr377_View_divPaymentHistoryInfo
                    //tdDetailsTab
                    IWebElement IDetails = driver.FindElement(By.Id("tdDetailsTab"));
                    IDetails.Click();
                    //  dnn_ctr377_View_tdOITitle
                    strYear = driver.FindElement(By.Id("dnn_ctr377_View_tdOITitle")).Text;
                    ByVisibleElement(driver.FindElement(By.Id("dnn_ctr377_View_tdOITitle")));
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, ParcelId, "Detail", driver, "KS", "Johnson");
                    TaxYear = GlobalClass.Before(strYear, " OWNER INFORMATION");
                    try
                    {
                        Owner = driver.FindElement(By.Id("dnn_ctr377_View_divOwnersLabel")).Text;
                    }
                    catch { }
                    try
                    {
                        if (Owner == "")
                        {
                            IWebElement Iowner = driver.FindElement(By.Id("dnn_ctr377_View_ddOwners"));
                            SelectElement Sowner = new SelectElement(Iowner);
                            Owner = Sowner.SelectedOption.Text;
                        }
                    }
                    catch { }
                    ByVisibleElement(driver.FindElement(By.Id("dnn_ctr377_View_tdOITitle")));
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, ParcelId, "Detail1", driver, "KS", "Johnson");
                    PropertyAddress = driver.FindElement(By.Id("dnn_ctr377_View_tdPropertyAddress")).Text;
                    AssessValue = driver.FindElement(By.Id("dnn_ctr377_View_tdTotalAssessedValue")).Text;
                    PropertyStatus = driver.FindElement(By.Id("dnn_ctr377_View_tdGIPropertyStatus")).Text;
                    PropertyType = driver.FindElement(By.Id("dnn_ctr377_View_tdGIPropertyType")).Text;
                    LegalDescription = driver.FindElement(By.Id("dnn_ctr377_View_tdGILegalDescription")).Text;
                    TaxingUnit = driver.FindElement(By.Id("dnn_ctr377_View_tdTUG")).Text;
                    Neighborhood = driver.FindElement(By.Id("dnn_ctr377_View_tdGINeighborhood")).Text;
                    RETaxID = driver.FindElement(By.Id("dnn_ctr377_View_tdRETaxID")).Text;
                    MapNO = driver.FindElement(By.Id("dnn_ctr377_View_tdGIMapNumber")).Text;

                    // Current Appraised Value
                    //strCurrentType = driver.FindElement(By.Id("dnn_ctr377_View_tdCAVTitle")).Text;
                    //IWebElement ICurrentAppraise = driver.FindElement(By.Id("dnn_ctr377_View_tblCurrentAppraisedValueData"));
                    //IList<IWebElement> ICurrentAppraiseRow = ICurrentAppraise.FindElements(By.TagName("tr"));
                    //IList<IWebElement> ICurrentAppraisedTd;
                    //foreach (IWebElement current in ICurrentAppraiseRow)
                    //{
                    //    ICurrentAppraisedTd = current.FindElements(By.TagName("td"));
                    //    if (ICurrentAppraisedTd.Count != 0)
                    //    {
                    //        try
                    //        {
                    //            string currentAppraised = strCurrentType + "~" + ICurrentAppraisedTd[0].Text + "~" + ICurrentAppraisedTd[1].Text + "~" + ICurrentAppraisedTd[2].Text + "~" + "";
                    //            gc.insert_date(orderNumber, RETaxID, 591, currentAppraised, 1, DateTime.Now);
                    //        }
                    //        catch { }
                    //    }
                    //}
                    //Value History
                    IWebElement IValueHistory = driver.FindElement(By.Id("dnn_ctr377_View_tblValueHistoryDataRP"));
                    IList<IWebElement> IValueHistoryRow = IValueHistory.FindElements(By.TagName("tr"));
                    IList<IWebElement> IValueHistoryTd;
                    foreach (IWebElement value in IValueHistoryRow)
                    {
                        IValueHistoryTd = value.FindElements(By.TagName("td"));
                        if (IValueHistoryTd.Count != 0)
                        {
                            try
                            {
                                string valueHistory = IValueHistoryTd[0].Text + "~" + IValueHistoryTd[1].Text + "~" + IValueHistoryTd[2].Text + "~" + IValueHistoryTd[3].Text + "~" + IValueHistoryTd[4].Text + "~" + IValueHistoryTd[5].Text + "~" + IValueHistoryTd[6].Text;
                                gc.insert_date(orderNumber, RETaxID, 593, valueHistory, 1, DateTime.Now);
                            }
                            catch { }
                        }
                    }
                    //Tax Bills
                    IWebElement IBills = driver.FindElement(By.Id("tdBillsTab"));
                    IBills.Click();
                    //js.ExecuteScript("arguments[0].click();", IBills);
                    //gc.CreatePdf(orderNumber, ParcelId, "Tax Bill Result", driver, "KS", "Johnson");
                    ByVisibleElement(driver.FindElement(By.XPath("//*[@id='dnn_ctr377_View_divBillDetails']/div[1]/table[1]/tbody")));
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, ParcelId, "Bills", driver, "KS", "Johnson");
                    IWebElement ITaxInstallment = driver.FindElement(By.XPath("//*[@id='dnn_ctr377_View_divBillDetails']/div[1]/table[2]"));
                    IList<IWebElement> ITaxInstallmentRow = ITaxInstallment.FindElements(By.TagName("tr"));
                    IList<IWebElement> ITaxInstallmentTd;
                    foreach (IWebElement install in ITaxInstallmentRow)
                    {
                        ITaxInstallmentTd = install.FindElements(By.TagName("td"));
                        if (ITaxInstallmentTd.Count != 0 && !install.Text.Contains("Installment"))
                        {
                            string TaxInstallment = TaxInstallType + "~" + ITaxInstallmentTd[0].Text + "~" + ITaxInstallmentTd[1].Text + "~" + ITaxInstallmentTd[2].Text + "~" + ITaxInstallmentTd[3].Text + "~" + ITaxInstallmentTd[4].Text;
                            gc.insert_date(orderNumber, RETaxID, 594, TaxInstallment, 1, DateTime.Now);
                            TaxInstallType = "";
                        }
                        if (ITaxInstallmentTd.Count != 0 && install.Text.Contains("Installment") && ITaxInstallmentTd.Count < 5)
                        {
                            TaxInstallType = ITaxInstallmentTd[0].Text;
                        }
                    }
                    ByVisibleElement(driver.FindElement(By.XPath("//*[@id='dnn_ctr377_View_divBillDetails']/div[1]/table[2]/tbody/tr[15]")));
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, ParcelId, "Bills1", driver, "KS", "Johnson");
                    //Tax Due
                    string strTaxDueBill = "", strEffectiveDate = "", TaxDue = "", NextTaxDue = "";
                    try
                    {
                        strEffectiveDate = DateTime.Now.Date.ToShortDateString();
                        IWebElement Ibillstatus = driver.FindElement(By.Id("btnPayMyBills"));
                        strTaxDueBill = Ibillstatus.GetAttribute("value");
                    }
                    catch { }
                    ByVisibleElement(driver.FindElement(By.XPath("//*[@id='dnn_ctr377_View_divBillDetails']/div[3]/table[1]/tbody")));
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, ParcelId, "Bills2", driver, "KS", "Johnson");
                    IWebElement ITaxDueTable = driver.FindElement(By.XPath("//*[@id='dnn_ctr377_View_divPaymentModal']/table/tbody/tr[2]/td/table/tbody"));
                    IList<IWebElement> ITaxDueRow = ITaxDueTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> ITaxTD;
                    foreach (IWebElement due in ITaxDueRow)
                    {
                        ITaxTD = due.FindElements(By.TagName("td"));
                        if (ITaxTD.Count != 0 && !due.Text.Contains("No Bills Due") && ITaxTD.Count > 1)
                        {
                            TaxDue += ITaxTD[1].Text + "~";
                        }
                    }
                    gc.insert_date(orderNumber, RETaxID, 595, strEffectiveDate + "~" + TaxDue.Remove(TaxDue.Length - 1, 1), 1, DateTime.Now);

                    if (strTaxDueBill.Trim() == "Pay My Bills")
                    {
                        string currDate = DateTime.Now.ToString("MM/dd/yyyy");
                        string dateChecking = DateTime.Now.ToString("MM") + "/15/" + DateTime.Now.ToString("yyyy");

                        if (Convert.ToDateTime(currDate) > Convert.ToDateTime(dateChecking))
                        {
                            string nextEndOfMonth = "";
                            if ((Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))) < 12))
                            {
                                nextEndOfMonth = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM")) + 1), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")) + 1)).ToString("MM/dd/yyyy");
                                driver.FindElement(By.Id("effectiveDatePicker")).SendKeys(nextEndOfMonth);
                            }
                            else
                            {
                                int nxtYr = Convert.ToInt16(DateTime.Now.ToString("yyyy")) + 1;
                                nextEndOfMonth = nextEndOfMonth = new DateTime(nxtYr, 1, DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), 1)).ToString("MM/dd/yyyy");
                                driver.FindElement(By.Id("effectiveDatePicker")).SendKeys(nextEndOfMonth);
                            }
                            strEffectiveDate = nextEndOfMonth;
                        }
                        else
                        {
                            string EndOfMonth = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")))).ToString("MM/dd/yyyy");
                            strEffectiveDate = EndOfMonth;
                        }
                        driver.FindElement(By.Id("effectiveDatePicker")).Clear();
                        driver.FindElement(By.Id("effectiveDatePicker")).SendKeys(strEffectiveDate);
                        driver.FindElement(By.Id("effectiveDatePicker")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        IWebElement ITaxDueNext = driver.FindElement(By.XPath("//*[@id='dnn_ctr377_View_divPaymentModal']/table/tbody/tr[2]/td/table/tbody"));
                        IList<IWebElement> ITaxDueNextRow = ITaxDueNext.FindElements(By.TagName("tr"));
                        IList<IWebElement> ITaxNextTD;
                        foreach (IWebElement next in ITaxDueNextRow)
                        {
                            ITaxNextTD = next.FindElements(By.TagName("td"));
                            if (ITaxNextTD.Count != 0 && !next.Text.Contains("*Total Due shown may not reflect current amount due."))
                            {
                                try
                                {
                                    NextTaxDue += ITaxNextTD[1].Text + "~";
                                }
                                catch { }
                            }
                        }
                        gc.insert_date(orderNumber, RETaxID, 595, strEffectiveDate + "~" + NextTaxDue.Remove(NextTaxDue.Length - 1, 1), 1, DateTime.Now);
                        gc.CreatePdf(orderNumber, ParcelId, "Tax Due Result" + strEffectiveDate.Replace("/", ""), driver, "KS", "Johnson");
                    }
                    //Payment History 
                    List<string> URL = new List<string>();
                    IWebElement IPaymentClick = driver.FindElement(By.XPath("//*[@id='dnn_ctr377_View_divPaymentHistoryExpandCollapse']/i"));
                    IPaymentClick.Click();
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, ParcelId, "Payment History Reciept", driver, "KS", "Johnson");
                    int Pyear = 0, PaymentBill = 0;
                    IWebElement IlistPaymenttable = driver.FindElement(By.Id("dnn_ctr377_View_divPaymentHistoryInfo"));
                    IList<IWebElement> IlistPayment = driver.FindElements(By.TagName("li"));
                    foreach (IWebElement reciept in IlistPayment)
                    {
                        if (reciept.Text != "" && reciept.Text.Contains("Transaction Date"))
                        {
                            PaymentBill++;
                        }
                    }
                    for (int i = 1; i <= PaymentBill; i++)
                    {
                        //if (Pyear < 3)
                        //{
                        try
                        {
                            string PaymentYear = driver.FindElement(By.XPath("//*[@id='dnn_ctr377_View_divPaymentHistoryInfo']/ul/li[" + i + "]/table/tbody/tr/td[2]")).Text;
                            IWebElement IPaymentHistoryTable = driver.FindElement(By.XPath("//*[@id='dnn_ctr377_View_divPaymentHistoryInfo']/ul/li[" + i + "]/div/table/tbody"));
                            IList<IWebElement> IPaymentHistoryRow = IPaymentHistoryTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> IPaymentHistortTd;
                            foreach (IWebElement Payment in IPaymentHistoryRow)
                            {
                                IPaymentHistortTd = Payment.FindElements(By.TagName("td"));
                                if (IPaymentHistortTd.Count != 0)
                                {
                                    string PaymentHistory = PaymentYear + "~" + IPaymentHistortTd[0].Text + "~" + IPaymentHistortTd[1].Text + "~" + IPaymentHistortTd[2].Text.Trim().Replace("View", "");
                                    gc.insert_date(orderNumber, RETaxID, 596, PaymentHistory, 1, DateTime.Now);
                                }
                            }
                            //  Pyear++;
                        }
                        catch { }
                        //}
                    }

                    //TaxStatement
                    try
                    {
                        IWebElement ITaxStatement = driver.FindElement(By.XPath("//*[@id='dnn_ctr377_View_divBillDetails']/div[1]/table[1]/tbody"));
                        IList<IWebElement> ITaxStatementRow = ITaxStatement.FindElements(By.TagName("tr"));
                        IList<IWebElement> ITaxSyayementTD;
                        foreach (IWebElement statement in ITaxStatementRow)
                        {
                            ITaxSyayementTD = statement.FindElements(By.TagName("td"));
                            if (ITaxSyayementTD.Count != 0)
                            {
                                IWebElement Istatement = ITaxSyayementTD[2].FindElement(By.TagName("input"));
                                string strStatement = Istatement.GetAttribute("value");
                                if (strStatement.Contains("Tax Statement"))
                                {
                                    Istatement.Click();
                                    Thread.Sleep(5000);
                                    gc.downloadfile(driver.Url, orderNumber, ParcelId, " Tax Statement ", "KS", "Johnson");
                                }
                            }
                        }

                    }
                    catch { }


                    //Payment Bill Download Through Chrome
                    string strPaymentYear = "", PreviousYear = "";
                    List<string> StrBill = new List<string>();
                    var chromeOptions = new ChromeOptions();

                    using (var chDriver = new ChromeDriver(chromeOptions))
                    {
                        try
                        {
                            chDriver.Navigate().GoToUrl("https://land.jocogov.org/default.aspx");
                            chDriver.FindElement(By.Id("btnYes")).Click();
                            Thread.Sleep(6000);
                            chDriver.FindElement(By.Id("tbSearchID")).Clear();
                            Thread.Sleep(3000);
                            chDriver.FindElement(By.Id("tbSearchID")).SendKeys(ParcelId.Replace(".", "").Replace("-", "").Trim());
                            Thread.Sleep(6000);
                            try
                            {
                                IWebElement IParcel = chDriver.FindElement(By.XPath("/html/body/ul"));
                                IList<IWebElement> IParcelRow = IParcel.FindElements(By.TagName("li"));
                                IList<IWebElement> IParcelTD;
                                foreach (IWebElement parcel in IParcelRow)
                                {
                                    IParcelTD = parcel.FindElements(By.TagName("a"));
                                    if (parcel.Text != "" && IParcelRow.Count < 2 && parcel.Text.Contains(ParcelId.Replace(".", "").Replace("-", "").Trim()))
                                    {
                                        IParcelTD[0].Click();
                                        Thread.Sleep(3000);
                                    }
                                }

                            }
                            catch { }
                            chDriver.FindElement(By.Id("btnName")).SendKeys(Keys.Enter);
                            Thread.Sleep(3000);
                            IWebElement IBillDownload = chDriver.FindElement(By.Id("navTaxBill"));
                            IBillDownload.Click();
                            chDriver.SwitchTo().Window(chDriver.WindowHandles.Last());
                            IWebElement IBill = chDriver.FindElement(By.Id("tdBillsTab"));
                            IBill.Click();
                            IWebElement IPayClick = chDriver.FindElement(By.XPath("//*[@id='dnn_ctr377_View_divPaymentHistoryExpandCollapse']/i"));
                            IPayClick.Click();
                            int bill = 0;
                            for (int i = 1; i <= PaymentBill; i++)
                            {
                                try
                                {
                                    try
                                    {
                                        IWebElement Ibill = chDriver.FindElement(By.XPath("//*[@id='dnn_ctr377_View_divPaymentHistoryInfo']/ul/li[" + i + "]/div"));
                                        js.ExecuteScript("arguments[0].scrollIntoView();", Ibill);
                                        gc.CreatePdf(orderNumber, ParcelId, "Tax Due Details" + bill, chDriver, "KS", "Johnson");
                                    }
                                    catch { }
                                    if (bill < 3)
                                    {
                                        try
                                        {
                                            strPaymentYear = driver.FindElement(By.XPath("//*[@id='dnn_ctr377_View_divPaymentHistoryInfo']/ul/li[" + i + "]/table/tbody/tr/td[2]")).Text;
                                            if (strPaymentYear != PreviousYear)
                                            {
                                                PreviousYear = strPaymentYear;
                                                bill++;
                                            }
                                        }
                                        catch { }
                                        IWebElement IPayHistory = chDriver.FindElement(By.XPath("//*[@id='dnn_ctr377_View_divPaymentHistoryInfo']/ul/li[" + i + "]/div/table/tbody"));
                                        IList<IWebElement> IPayHistoryRow = IPayHistory.FindElements(By.TagName("tr"));
                                        IList<IWebElement> IPayHistoryTd;
                                        foreach (IWebElement pay in IPayHistoryRow)
                                        {
                                            IPayHistoryTd = pay.FindElements(By.TagName("td"));
                                            if (pay.Text.Contains("View"))
                                            {
                                                IWebElement Iviewlink = IPayHistoryTd[2].FindElement(By.TagName("a"));
                                                string view = gc.Between(Iviewlink.GetAttribute("onclick"), "return OpenReceiptPDF(", ")").Replace("'", "");
                                                string[] taxbillno = view.Split(',');
                                                string BillURL = "https://taxbill.jocogov.org/proxy/APIProxy.ashx?/API/api/v1/documents/pdf/Receipt-" + taxbillno[0].Trim() + "-" + taxbillno[1].Trim() + ".pdf/";

                                                string strCurrent = chDriver.CurrentWindowHandle;
                                                var windowHandles = chDriver.WindowHandles;
                                                chDriver.ExecuteScript(string.Format("window.open('{0}', '_blank');", chDriver.Url));
                                                var newWindowHandles = chDriver.WindowHandles;
                                                var openedWindowHandle = newWindowHandles.Except(windowHandles).Single();
                                                chDriver.SwitchTo().Window(openedWindowHandle);
                                                IWebElement IPayClic = chDriver.FindElement(By.XPath("//*[@id='dnn_ctr377_View_divPaymentHistoryExpandCollapse']/i"));
                                                IPayClic.Click();
                                                IWebElement IPayHis = chDriver.FindElement(By.XPath("//*[@id='dnn_ctr377_View_divPaymentHistoryInfo']/ul/li[" + i + "]/div/table/tbody"));
                                                IList<IWebElement> IPayHisRow = IPayHis.FindElements(By.TagName("tr"));
                                                IList<IWebElement> IPayHisTd;
                                                foreach (IWebElement payH in IPayHisRow)
                                                {
                                                    try
                                                    {
                                                        IPayHisTd = payH.FindElements(By.TagName("td"));
                                                        if (payH.Text.Contains("View"))
                                                        {
                                                            string billno = IPayHisTd[2].Text.Trim().Replace("View", "");
                                                            IWebElement IviewlinkCheck = IPayHisTd[2].FindElement(By.TagName("a"));
                                                            string viewCheck = gc.Between(IviewlinkCheck.GetAttribute("onclick"), "return OpenReceiptPDF(", ")").Replace("'", "");
                                                            string[] taxbillnoCheck = viewCheck.Split(',');
                                                            if (taxbillno[0] == taxbillnoCheck[0] && taxbillno[1] == taxbillnoCheck[1])
                                                            {
                                                                IviewlinkCheck.Click();
                                                                Thread.Sleep(3000);
                                                                try
                                                                {
                                                                    gc.downloadfile(chDriver.Url, orderNumber, ParcelId, taxbillnoCheck[1] + billno + i, "KS", "Johnson");
                                                                }
                                                                catch { }
                                                                chDriver.Close();
                                                                break;
                                                            }
                                                        }
                                                    }
                                                    catch { }
                                                }
                                                chDriver.SwitchTo().Window(strCurrent);
                                            }
                                        }
                                    }
                                }
                                catch
                                {
                                }
                            }
                            chDriver.Quit();
                        }
                        catch (Exception Ex)
                        {

                        }
                    }
                    try
                    {
                        driver.Navigate().GoToUrl("https://taxbill.jocogov.org/");
                        IWebElement ITaxAuthority = driver.FindElement(By.XPath("//*[@id='dnn_ctr443_HtmlModule_lblContent']/table/tbody/tr[4]/td/table/tbody/tr[2]/td[1]/table/tbody"));
                        TaxAuthority = gc.Between(ITaxAuthority.Text, "Johnson County Treasurer", "Fax:");
                        gc.CreatePdf(orderNumber, ParcelId, "Tax Authority", driver, "KS", "Johnson");
                    }
                    catch { }

                    string TaxAssessmentDetails = PropertyAddress + "~" + TaxYear + "~" + AssessValue + "~" + PropertyStatus + "~" + PropertyType + "~" + LegalDescription + "~" + TaxingUnit + "~" + Neighborhood + "~" + RETaxID + "~" + MapNO + "~" + TaxAuthority;
                    gc.insert_date(orderNumber, RETaxID, 592, TaxAssessmentDetails, 1, DateTime.Now);

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "KS", "Johnson", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);


                    driver.Quit();
                    gc.mergpdf(orderNumber, "KS", "Johnson");
                    return "Data Inserted Successfully";
                }
                catch (Exception ex)
                {
                    driver.Quit();
                    throw ex;
                }
            }
        }
        public void ByVisibleElement(IWebElement Element)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].scrollIntoView();", Element);
        }
    }
}