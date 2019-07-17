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
using System.Linq;

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_ForsythNC
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_Forsyth(string streetno, string direction, string streetname, string streetype, string unitno, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            List<string> multiparcel = new List<string>();
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {

                StartTime = DateTime.Now.ToString("HH:mm:ss");

                driver.Navigate().GoToUrl("http://tellus.co.forsyth.nc.us/lrcpwa/SearchProperty.aspx");
                Thread.Sleep(2000);

                try
                {
                    if (searchType == "titleflex")
                    {
                        string address = streetno + " " + direction + " " + streetname + " " + streetype + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "NC", "Forsyth");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_NCForsyth"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        driver.FindElement(By.XPath("//*[@id='panelSummary']/li[2]/a")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_StreetNumberTextBox")).SendKeys(streetno);
                        driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_StreetNameTextBox")).SendKeys(streetname);
                        gc.CreatePdf_WOP(orderNumber, "SearchAddressBefore", driver, "NC", "Forsyth");
                        driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_AddressButton")).SendKeys(Keys.Enter);
                        gc.CreatePdf_WOP(orderNumber, "SearchAddressAfter", driver, "NC", "Forsyth");
                        Thread.Sleep(2000);

                        string mul = driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[3]/table/tbody/tr[2]/td[2]/div/div/div/div/table/tbody/tr[1]/td[2]/span")).Text;
                        mul = WebDriverTest.Before(mul, "Records").Trim();

                        int i, j = 2;
                        if ((mul != "1") && (mul != "0"))
                        {

                            int iRowsCount = driver.FindElements(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_streetDictionaryResultsGridView']/tbody/tr")).Count;
                            for (i = 2; i <= iRowsCount; i++)
                            {
                                if (j >= 2 && i != 2)
                                {
                                    IWebElement checkbox1 = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_streetDictionaryResultsGridView_ctl0" + j + "_CheckBox1']"));
                                    // checkbox.Clear();
                                    checkbox1.Click();
                                    Thread.Sleep(1000);
                                    j++;
                                }
                                IWebElement checkbox = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_streetDictionaryResultsGridView_ctl0" + i + "_CheckBox1']"));
                                // checkbox.Clear();
                                checkbox.Click();
                                Thread.Sleep(1000);
                                driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_StreetDictionarySearchButton")).SendKeys(Keys.Enter);
                                Thread.Sleep(1000);
                                string mul1 = driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[3]/table/tbody/tr[2]/td[2]/div/div/div/div/table/tbody/tr[1]/td[2]/span")).Text;
                                mul1 = GlobalClass.Before(mul1, " Records Matched Search Criteria").Trim();
                                int count = Convert.ToInt32(mul1);
                                if (count > 0)
                                {   //multi parcel
                                    gc.CreatePdf_WOP(orderNumber, "Multiparcel Result", driver, "NC", "Forsyth");
                                    IWebElement tbmulti2 = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_ParcelStreetsGridView']/tbody"));
                                    IList<IWebElement> TRmulti2 = tbmulti2.FindElements(By.TagName("tr"));
                                    IList<IWebElement> TDmulti2;
                                    int rescount = TRmulti2.Count;

                                    foreach (IWebElement row in TRmulti2)
                                    {
                                        TDmulti2 = row.FindElements(By.TagName("td"));

                                        if (TDmulti2.Count != 0 && TDmulti2[1].Text.Trim() != "" && !row.Text.Contains("Pfx") && TDmulti2.Count == 9)
                                        {
                                            IWebElement multiparcellink = TDmulti2[0].FindElement(By.TagName("a"));
                                            string strmulti = multiparcellink.GetAttribute("href");
                                            multiparcel.Add(strmulti);
                                            string multi1 = TDmulti2[1].Text + " " + TDmulti2[4].Text + "~" + TDmulti2[8].Text;

                                            if (TDmulti2[1].Text == streetno || streetno == "0")
                                            {
                                                gc.insert_date(orderNumber, TDmulti2[0].Text, 616, multi1, 1, DateTime.Now);
                                            }
                                            //  Owner~address
                                        }

                                    }

                                }
                                driver.Navigate().Back();
                                Thread.Sleep(1000);
                            }

                            if (multiparcel.Count > 1)
                            {

                                HttpContext.Current.Session["multiparcel_Forsyth"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            else
                            {
                                foreach (string real in multiparcel)
                                {
                                    driver.Navigate().GoToUrl(real);
                                    Thread.Sleep(4000);
                                }
                            }
                        }

                        else
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_ParcelStreetsGridView']/tbody/tr[2]/td[1]/a")).SendKeys(Keys.Enter);
                                Thread.Sleep(2000);
                            }
                            catch { }
                        }

                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[3]/table/tbody/tr[2]/td[2]/div/div/div/div/table/tbody/tr[1]/td[2]/span")).Text;
                            if (nodata.Contains("0 Records Matched Search Criteria"))
                            {
                                HttpContext.Current.Session["Nodata_NCForsyth"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.XPath("//*[@id='panelSummary']/li[4]/a")).Click();
                        Thread.Sleep(4000);
                        IWebElement text = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_PINNumberTextBox']"));
                        parcelNumber = parcelNumber.Replace("-", "");
                        text.Clear();
                        IWebElement add = driver.FindElement(By.XPath("//*[@id='PIN']/div/div/div/center/table/tbody/tr[2]/td/div/div"));
                        IList<IWebElement> MultiOwnerRow = add.FindElements(By.TagName("input"));
                        foreach (IWebElement row1 in MultiOwnerRow)
                        {
                            row1.SendKeys(parcelNumber);
                        }
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Input Passed", driver, "NC", "Forsyth");
                        // text.SendKeys(parcelNumber);
                        driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_PinButton']")).Click();
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Parcel Search Result", driver, "NC", "Forsyth");
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("ctl00_PageHeader1_ErrorMessageLabel")).Text;
                            if (nodata.Contains("No Records Found"))
                            {
                                HttpContext.Current.Session["Nodata_NCForsyth"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "ownername")
                    {
                        string firstname = "", lastname = "";
                        if (ownername.Contains(' '))
                        {
                            string[] name = ownername.Split(' ');
                            firstname = name[0]; lastname = name[1];
                            if (!ownername.Contains(','))
                            {
                                ownername = firstname + "," + lastname;
                            }
                        }
                        driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_OwnerTextBox']")).SendKeys(ownername);
                        driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_OwnerButton")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        string mul = driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[3]/table/tbody/tr[2]/td[2]/div/div/div/div/table/tbody/tr[1]/td[2]/span")).Text;
                        mul = WebDriverTest.Before(mul, "Records").Trim();

                        if ((mul != "1") && (mul != "0"))
                        {
                            //multi parcel
                            gc.CreatePdf_WOP(orderNumber, "Multiparcel Result", driver, "NC", "Forsyth");
                            IWebElement tbmulti = driver.FindElement(By.XPath(" //*[@id='ctl00_ContentPlaceHolder1_OwnerSearchResultsGridView']/tbody"));
                            IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));
                            int TRmulticount = TRmulti.Count;
                            int maxCheck = 0;
                            IList<IWebElement> TDmulti;
                            foreach (IWebElement row in TRmulti)
                            {
                                if (maxCheck <= 25)
                                {

                                    TDmulti = row.FindElements(By.TagName("td"));
                                    if (TDmulti.Count != 0)
                                    {
                                        if (maxCheck <= 15)
                                        {
                                            string multi1 = TDmulti[3].Text + "~" + TDmulti[2].Text;
                                            gc.insert_date(orderNumber, TDmulti[1].Text, 616, multi1, 1, DateTime.Now);
                                        }
                                    }
                                    maxCheck++;
                                }
                            }

                            if (TRmulti.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Forsyth_Multicount"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiparcel_Forsyth"] = "Yes";
                            }
                            driver.Quit();
                            return "MultiParcel";
                        }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[3]/table/tbody/tr[2]/td[2]/div/div/div/div/table/tbody/tr[1]/td[2]/span")).Text;
                            if (nodata.Contains("0 Records Matched Search Criteria"))
                            {
                                HttpContext.Current.Session["Nodata_NCForsyth"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    //Property Details 

                    string TaxYear = "", REID = "", ParcelID = "", PropertyOwner = "", PropertyAddress = "", LegalDescription = "", MailingAddress = "", OldMapNumber = "", MarketArea = "", Township = "", PlanningJurisdiction = "", City = "", FireDistrict = "", SpecialDistrict = "", PropertyClass = "", YearBuilt = "";
                    //driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_ParcelStreetsGridView']/tbody/tr[2]/td[1]/a")).SendKeys(Keys.Enter);
                    //Thread.Sleep(2000);

                    string propbulktext = driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[3]/table/tbody/tr[2]/td[2]/div[1]/div/div/div[2]/div[1]/div[1]/table/tbody/tr[1]/td/table")).Text;

                    TaxYear = driver.FindElement(By.Id("ctl00_PageHeader1_TaxYear")).Text;
                    REID = driver.FindElement(By.Id("ctl00_PageHeader1_ReidLabelInfo")).Text;
                    ParcelID = driver.FindElement(By.Id("ctl00_PageHeader1_PinLabelInfo")).Text;

                    PropertyOwner = driver.FindElement(By.XPath("//*[@id='ctl00_PageHeader1_DetailsView1']/tbody/tr/td")).Text;
                    PropertyAddress = driver.FindElement(By.Id("ctl00_PageHeader1_LocationAddressLabelInfo")).Text;
                    LegalDescription = driver.FindElement(By.Id("ctl00_PageHeader1_PropertyDescriptionLabelInfo")).Text;
                    MailingAddress = driver.FindElement(By.Id("ctl00_PageHeader1_DetailsView4")).Text.Replace("\r\n", " ").Trim();
                    gc.CreatePdf(orderNumber, ParcelID, "Property  and Assessment Details", driver, "NC", "Forsyth");

                    string bulktext = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_DetailsView5")).Text;
                    OldMapNumber = gc.Between(bulktext, "Old Map# ", "Market Area ").Trim();
                    MarketArea = gc.Between(bulktext, "Market Area ", "Township ").Trim();
                    Township = gc.Between(bulktext, "Township ", "Planning Jurisdiction").Trim();
                    PlanningJurisdiction = gc.Between(bulktext, "Planning Jurisdiction", "City").Trim();
                    City = gc.Between(bulktext, "City", "Fire District").Trim();
                    FireDistrict = gc.Between(bulktext, "Fire District", "Spec District").Trim();
                    SpecialDistrict = gc.Between(bulktext, "Spec District", "Land Class").Trim();
                    PropertyClass = gc.Between(bulktext, "Land Class", "History REID 1").Replace("\r\n", " ").Trim();


                    driver.FindElement(By.Id("ctl00_PageHeader1_BuildingsHyperLink")).SendKeys(Keys.Enter);
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, ParcelID, "Building Details", driver, "NC", "Forsyth");
                    string yearbuilt = "";
                    try
                    {
                        yearbuilt = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_DetailsView4")).Text;
                        YearBuilt = gc.Between(yearbuilt, "Year Built", "Additions").Replace("\r\n", "").Trim();
                    }
                    catch { }
                    string propertydetails = TaxYear + "~" + REID + "~" + PropertyOwner + "~" + PropertyAddress + "~" + LegalDescription + "~" + MailingAddress + "~" + OldMapNumber + "~" + MarketArea + "~" + Township + "~" + PlanningJurisdiction + "~" + City + "~" + FireDistrict + "~" + SpecialDistrict + "~" + PropertyClass + "~" + YearBuilt;
                    gc.insert_date(orderNumber, ParcelID, 603, propertydetails, 1, DateTime.Now);

                    //Assessment Details

                    string currentHandle = driver.CurrentWindowHandle;
                    IWebElement element = driver.FindElement(By.LinkText("Print Property Info"));
                    PopupWindowFinder finder = new PopupWindowFinder(driver);
                    string popupWindowHandle = finder.Click(element);
                    driver.SwitchTo().Window(popupWindowHandle);
                    gc.CreatePdf(orderNumber, ParcelID, "Assessment Details", driver, "NC", "Forsyth");
                    Thread.Sleep(3000);
                    IWebElement asstableElement = driver.FindElement(By.XPath("//*[@id='headerPlaceholder']/div/div[4]/div/table/tbody"));
                    IList<IWebElement> asstableElementRow = asstableElement.FindElements(By.TagName("tr"));
                    IList<IWebElement> asstableElementRowTD;
                    IList<IWebElement> asstableElementRowTH;
                    var assesscolumn = ""; var assessvalue = "";
                    foreach (IWebElement rowid in asstableElementRow)
                    {
                        asstableElementRowTD = rowid.FindElements(By.TagName("td"));
                        asstableElementRowTH = rowid.FindElements(By.TagName("th"));
                        if (asstableElementRowTD.Count != 0 && !rowid.Text.Contains("Property Value") && rowid.Text != "")
                        {
                            if (asstableElementRowTD[0].Text != " ")
                            {
                                assesscolumn += asstableElementRowTH[0].Text + "~";
                                assessvalue += asstableElementRowTD[0].Text + "~";
                            }
                        }
                    }
                    assesscolumn = assesscolumn.TrimEnd('~');
                    assessvalue = assessvalue.TrimEnd('~');

                    DBconnection dbconn = new DBconnection();
                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + assesscolumn + "' where Id = '" + 605 + "'");
                    gc.insert_date(orderNumber, ParcelID, 605, assessvalue, 1, DateTime.Now);



                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    //Tax Information Details
                    driver.Navigate().GoToUrl("http://tellus.co.forsyth.nc.us/PublicWebAccess/BillSearchResults.aspx?");
                    Thread.Sleep(2000);

                    string Par = ".000";
                    parcelNumber = ParcelID + Par;

                    IWebElement ISelect = driver.FindElement(By.Id("lookupCriterion"));
                    SelectElement sSelect = new SelectElement(ISelect);
                    sSelect.SelectByText("Parcel Number");
                    driver.FindElement(By.Id("txtSearchString")).SendKeys(parcelNumber);
                    gc.CreatePdf(orderNumber, ParcelID, "Tax Search  Input Passed", driver, "NC", "Forsyth");
                    driver.FindElement(By.XPath("//*[@id='btnGo']")).SendKeys(Keys.Enter);
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, ParcelID, "Tax Search Result", driver, "NC", "Forsyth");


                    //Tax Histry Details
                    IWebElement tdHistry = driver.FindElement(By.XPath("//*[@id='tblSearchResults']"));
                    IList<IWebElement> TrHistry = tdHistry.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDHistry;
                    IList<IWebElement> TDHistryth;
                    foreach (IWebElement row in TrHistry)
                    {
                        TDHistry = row.FindElements(By.TagName("td"));
                        TDHistryth = row.FindElements(By.TagName("tr"));
                        if (!row.Text.Contains("Bill #"))
                        {
                            if (TDHistry.Count == 7)
                            {
                                string TaxHistryDetails = TDHistry[0].Text + "~" + TDHistry[1].Text + "~" + TDHistry[2].Text + "~" + TDHistry[3].Text + "~" + TDHistry[4].Text + "~" + TDHistry[5].Text + "~" + TDHistry[6].Text;
                                gc.insert_date(orderNumber, ParcelID, 612, TaxHistryDetails, 1, DateTime.Now);
                            }
                            else if (row.Text.Contains("Total"))
                            {
                                string total = GlobalClass.After(row.Text, "Total:");
                                string TaxHistryDetails = "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "Total" + "~" + total;
                                gc.insert_date(orderNumber, ParcelID, 612, TaxHistryDetails, 1, DateTime.Now);
                            }
                        }

                    }

                    List<string> strTaxRealestate = new List<string>();
                    try
                    {
                        IWebElement ITaxinfoDetails = driver.FindElement(By.XPath("//*[@id='G_dgResults']/tbody"));
                        IList<IWebElement> ITaxinfoRealRow = ITaxinfoDetails.FindElements(By.TagName("tr"));
                        IList<IWebElement> ITaxinfoRealTd;
                        foreach (IWebElement ItaxinfoReal in ITaxinfoRealRow)
                        {
                            ITaxinfoRealTd = ItaxinfoReal.FindElements(By.TagName("td"));
                            if (ITaxinfoRealTd.Count != 0 && strTaxRealestate.Count < 3)
                            {
                                IWebElement ITaxBillCount = ITaxinfoRealTd[0].FindElement(By.TagName("a"));
                                string strTaxReal = ITaxBillCount.GetAttribute("href");
                                strTaxRealestate.Add(strTaxReal);

                            }
                        }
                    }
                    catch
                    {

                    }

                    foreach (string real in strTaxRealestate)
                    {
                        string TaxMailingAddress = "", RealValue = "", DeferredValue = "", UseValue = "", PersonalValue = "", ExemptExclusion = "", TotalAssessedValue = "", BillTaxYear = "", PropertyTax = "", BillStatus = "", BillFlag = "", BillNumber = "", OldBillNumber = "", OldAccountNumber = "", DueDate = "", InterestBegins = ""
                            , InterestAmount = "", TotalBilled = "", GoodThroughDate = "", LastPaymentDate = "", CurrentDue = "", TaxAuthority = "", DiscountPeriod = "";

                        driver.Navigate().GoToUrl(real);
                        Thread.Sleep(4000);
                        if (strTaxRealestate.Count != 0 && strTaxRealestate.Count == 3)
                        {

                            try
                            {
                                BillFlag = driver.FindElement(By.Id("lblBillFlag")).Text;
                                if (BillFlag.Contains("DELINQUENT"))
                                {
                                    IWebElement dt = driver.FindElement(By.Id("interestCalDate_input"));
                                    string date = dt.GetAttribute("value");

                                    DateTime G_Date = Convert.ToDateTime(date);
                                    string dateChecking = DateTime.Now.ToString("MM") + "/15/" + DateTime.Now.ToString("yyyy");

                                    if (G_Date < Convert.ToDateTime(dateChecking))
                                    {
                                        //end of the month
                                        date = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")))).ToString("MM/dd/yyyy");

                                    }

                                    else if (G_Date > Convert.ToDateTime(dateChecking))
                                    {
                                        // nextEndOfMonth 
                                        if ((Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))) < 12))
                                        {
                                            date = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM")) + 1), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")) + 1)).ToString("MM/dd/yyyy");
                                        }
                                        else
                                        {
                                            int nxtYr = Convert.ToInt16(DateTime.Now.ToString("yyyy")) + 1;
                                            date = new DateTime(nxtYr, 1, DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), 1)).ToString("MM/dd/yyyy");

                                        }
                                    }

                                    Thread.Sleep(2000);
                                    dt.Clear();
                                    GoodThroughDate = date;
                                    driver.FindElement(By.Id("interestCalDate_input")).SendKeys(date);
                                    driver.FindElement(By.Id("btnRecalInterest")).SendKeys(Keys.Enter);
                                    //*[@id=""]

                                }
                                else
                                {
                                    GoodThroughDate = "";
                                }
                                TaxMailingAddress = driver.FindElement(By.Id("lblMailingAddr")).Text;
                                RealValue = driver.FindElement(By.Id("lblRealOriginal")).Text;
                                DeferredValue = driver.FindElement(By.Id("lblDeferredOriginal")).Text;
                                UseValue = driver.FindElement(By.Id("lblUseOriginal")).Text;
                                PersonalValue = driver.FindElement(By.Id("lblPersonalOriginal")).Text;
                                ExemptExclusion = driver.FindElement(By.Id("lblExemptOriginal")).Text;
                                TotalAssessedValue = driver.FindElement(By.Id("lblTotalValue")).Text;
                                BillTaxYear = driver.FindElement(By.Id("lblBill")).Text;
                                BillTaxYear = BillTaxYear.Substring(11, 4);
                                gc.CreatePdf(orderNumber, ParcelID, "Tax" + BillTaxYear, driver, "NC", "Forsyth");
                                PropertyTax = driver.FindElement(By.Id("lblPropertyType")).Text;
                                BillStatus = driver.FindElement(By.Id("lblBillStatus")).Text;
                                BillNumber = driver.FindElement(By.Id("lblBill")).Text;
                                OldBillNumber = driver.FindElement(By.Id("lblLegacyBillNum")).Text;
                                OldAccountNumber = driver.FindElement(By.Id("lblLegacyAccountNum")).Text;
                                DueDate = driver.FindElement(By.Id("lblDueDate")).Text;
                                InterestBegins = driver.FindElement(By.Id("lblInterest")).Text;
                                InterestAmount = driver.FindElement(By.Id("lblInterestAmt")).Text;
                                TotalBilled = driver.FindElement(By.Id("lblTotalAmountDue")).Text;
                                LastPaymentDate = driver.FindElement(By.Id("lblLastPaymentDate")).Text;
                                CurrentDue = driver.FindElement(By.Id("lblCurrentDue")).Text;

                                string discounttext = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td[2]/table[4]/tbody/tr/td/table")).Text;
                                if (discounttext.Contains("Discount Period"))
                                {
                                    DiscountPeriod = gc.Between(discounttext, "Discount Period:", "Correct if paid by").Replace("\r\n", "").Trim();
                                }
                                //DiscountPeriod= driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td[2]/table[4]/tbody/tr/td/table/tbody/tr[1]/td[2]/font")).Text;
                                TaxAuthority = "Forsyth County Tax Collector P.O.Box 82 Winston - Salem, NC 27102";
                                string TaxInformationdetails = TaxMailingAddress + "~" + RealValue + "~" + DeferredValue + "~" + UseValue + "~" + PersonalValue + "~" + ExemptExclusion + "~" + TotalAssessedValue + "~" + BillTaxYear + "~" + PropertyTax + "~" + BillStatus + "~" + BillFlag + "~" + BillNumber + "~" + OldBillNumber + "~" + OldAccountNumber + "~" + DueDate + "~" + InterestBegins + "~" + InterestAmount + "~" + TotalBilled + "~" + LastPaymentDate + "~" + CurrentDue + "~" + DiscountPeriod + "~" + GoodThroughDate + "~" + TaxAuthority;
                                gc.insert_date(orderNumber, ParcelID, 610, TaxInformationdetails, 1, DateTime.Now);
                            }
                            catch { }
                        }

                        //Tax/Fee Distribution Details
                        IWebElement tbmulti = driver.FindElement(By.XPath("//*[@id='dgShowResultRate']/tbody"));
                        IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));

                        IList<IWebElement> TDmulti;
                        foreach (IWebElement row in TRmulti)
                        {

                            TDmulti = row.FindElements(By.TagName("td"));
                            if (!row.Text.Contains("Rate"))
                            {
                                if (TDmulti.Count == 4)
                                {
                                    string TaxDistribution = BillTaxYear + "~" + TDmulti[0].Text + "~" + TDmulti[1].Text + "~" + TDmulti[2].Text + "~" + TDmulti[3].Text;
                                    gc.insert_date(orderNumber, ParcelID, 611, TaxDistribution, 1, DateTime.Now);
                                }
                            }
                        }
                    }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Quit();
                    gc.mergpdf(orderNumber, "NC", "Forsyth");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "NC", "Forsyth", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
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

        private string multiparcel(string ordernumber)
        {
            try
            {
                List<string> multiparcel = new List<string>();
                string mul = driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[3]/table/tbody/tr[2]/td[2]/div/div/div/div/table/tbody/tr[1]/td[2]/span")).Text;
                mul = GlobalClass.Before(mul, "Records").Trim();
                int count = Convert.ToInt32(mul);
                if (count > 0)
                {   //multi parcel

                    IWebElement tbmulti2 = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_ParcelStreetsGridView']/tbody"));
                    IList<IWebElement> TRmulti2 = tbmulti2.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDmulti2;
                    int rescount = TRmulti2.Count;
                    int k = 0;
                    if (rescount <= 25)
                    {
                        foreach (IWebElement row in TRmulti2)
                        {
                            if (k <= 25)
                            {
                                TDmulti2 = row.FindElements(By.TagName("td"));

                                if (TDmulti2.Count != 0 && TDmulti2[1].Text.Trim() != "" && !row.Text.Contains("Pfx"))
                                {
                                    multiparcel.Add(TDmulti2[0].Text);
                                    string multi1 = TDmulti2[1].Text + " " + TDmulti2[2].Text + "~" + TDmulti2[3].Text;
                                    gc.insert_date(ordernumber, TDmulti2[0].Text, 616, multi1, 1, DateTime.Now);
                                    //  Owner~address
                                }
                                k++;
                            }
                        }

                    }
                    else
                    {
                        HttpContext.Current.Session["multiParcel_Forsyth_Multicount"] = "Maximum";
                        return "Maximum";
                    }

                }

            }
            catch
            { }

            HttpContext.Current.Session["multiParcel_ForsythNC"] = "Yes";
            return "MultiParcel";

        }

    }
}





