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
using System.Web;

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_NCGuilford
    {
        string outputPath = "";
        IWebDriver driver;
        IWebElement tbmulti3;
        DBconnection db = new DBconnection();
        List<string> strTaxRealestate = new List<string>();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        List<string> multiparcel = new List<string>();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_NCGuilford(string houseno, string sname, string sttype, string unitno, string parcelNumber, string searchType, string orderNumber, string ownername, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            //IWebElement iframeElement1;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("http://taxcama.guilfordcountync.gov/camapwa/SearchProperty.aspx");

                    if (searchType == "address")
                    {

                        driver.FindElement(By.XPath("//*[@id='panelSummary']/li[2]/a")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_StreetNumberTextBox")).SendKeys(houseno);
                        driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_StreetNameTextBox")).SendKeys(sname.Trim());
                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "NC", "Guilford");
                        driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_AddressButton")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search result", driver, "NC", "Guilford");

                        //try {
                        //    string mul = driver.FindElement(By.Id("ctl00_SearchPageHeader_SearchResultDetailsLabel")).Text;

                        //    mul = WebDriverTest.Before(mul, " Records");


                        //    if ((mul != "1") && (mul != "0"))
                        //    {
                        //        //multi parcel
                        //        try
                        //        {
                        //           tbmulti3 = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder2_ParcelStreetsGridView']/tbody"));

                        //        }
                        //        catch { }
                        //        try
                        //        {
                        //            tbmulti3 = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder2_streetDictionaryResultsGridView']/tbody"));
                        //        }
                        //        catch { }
                        //        IList<IWebElement> TRmulti3 = tbmulti3.FindElements(By.TagName("tr"));
                        //        int maxCheck = 0;
                        //        IList<IWebElement> TDmulti3;
                        //        foreach (IWebElement row in TRmulti3)
                        //        {
                        //            if (maxCheck <= 25)
                        //            {
                        //                TDmulti3 = row.FindElements(By.TagName("td"));
                        //                if (TDmulti3.Count != 0)
                        //                {//Parcel Number~Address~Owner Name
                        //                    string multi1 = TDmulti3[1].Text + " " + TDmulti3[4].Text + " " + TDmulti3[5].Text + "~" + TDmulti3[8].Text;
                        //                    gc.insert_date(orderNumber, TDmulti3[0].Text, 597, multi1, 1, DateTime.Now);
                        //                }
                        //                maxCheck++;
                        //            }
                        //        }

                        //        if (TRmulti3.Count > 25)
                        //        {
                        //            HttpContext.Current.Session["multiParcel_Guilford_Multicount"] = "Maximum";
                        //        }
                        //        else
                        //        {
                        //            HttpContext.Current.Session["multiparcel_Guilford"] = "Yes";
                        //        }
                        //        driver.Quit();
                        //        return "MultiParcel";
                        //    }
                        //    else
                        //    {
                        //        driver.FindElement(By.XPath(" //*[@id='ctl00_ContentPlaceHolder2_ParcelStreetsGridView']/tbody/tr[2]/td[1]/a")).Click();
                        //        Thread.Sleep(2000);
                        //        try
                        //        {
                        //            driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder2_streetDictionaryResultsGridView']/tbody")).Click();
                        //        }
                        //        catch { }
                        //    }
                        //}
                        //catch { }
                        // CreatePdf_WOP(orderNumber, "Multiparcel Address Search");
                        try
                        {
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
                                        gc.CreatePdf_WOP(orderNumber, "Multiparcel Result", driver, "NC", "Guilford");
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

                                                if (TDmulti2[1].Text == houseno || houseno == "0")
                                                {
                                                    gc.insert_date(orderNumber, TDmulti2[0].Text, 597, multi1, 1, DateTime.Now);
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

                                    HttpContext.Current.Session["multiparcel_Guilford"] = "Yes";
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
                                driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_ParcelStreetsGridView']/tbody/tr[2]/td[1]/a")).SendKeys(Keys.Enter);
                                Thread.Sleep(2000);
                            }
                        }
                        catch { }
                    }
                    if (searchType == "titleflex")
                    {
                        string titleaddress = houseno + " " + sname + " " + sttype + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "NC", "Guilford");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_NCGuilford"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "parcel")
                    {

                        Thread.Sleep(3000);
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();

                        }

                        driver.FindElement(By.XPath("//*[@id='panelSummary']/li[3]/a")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_REIDTextBox")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "NC", "Guilford");
                        driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_ReidButton")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                    }
                    if (searchType == "block")
                    {

                        driver.FindElement(By.Id("__tab_ctl00_ContentPlaceHolder1_Tabs_PinTabPanel")).Click();
                        Thread.Sleep(2000);
                        string unitNum = unitno.Replace(" ", "");
                        driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_Tabs_PinTabPanel_PINNumberTextBox")).SendKeys(sttype);
                        gc.CreatePdf_WOP(orderNumber, "block Search", driver, "NC", "Guilford");
                        driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_Tabs_PinTabPanel_PinButton")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                    }
                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_OwnerTextBox")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "Owner Search", driver, "NC", "Guilford");
                        driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_OwnerButton")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        try
                        {
                            string mul = driver.FindElement(By.Id("//*[@id='aspnetForm']/div[3]/table/tbody/tr[2]/td[2]/div/div/div/div/table/tbody/tr[1]/td[2]/span")).Text;

                            mul = WebDriverTest.Before(mul, " Records");


                            if ((mul != "1") && (mul != "0"))
                            {
                                //multi parcel
                                //*[@id="ctl00_ContentPlaceHolder2_OwnerSearchResultsGridView"]/tbody
                                IWebElement tbmulti4 = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder2_OwnerSearchResultsGridView']/tbody"));
                                IList<IWebElement> TRmulti4 = tbmulti4.FindElements(By.TagName("tr"));
                                int maxCheck = 0;
                                IList<IWebElement> TDmulti4;
                                foreach (IWebElement row in TRmulti4)
                                {
                                    if (maxCheck <= 25)
                                    {
                                        TDmulti4 = row.FindElements(By.TagName("td"));
                                        if (TDmulti4.Count != 0)
                                        {
                                            string multi1 = TDmulti4[1].Text + "~" + TDmulti4[2].Text;
                                            gc.insert_date(orderNumber, TDmulti4[0].Text, 597, multi1, 1, DateTime.Now);
                                        }
                                        maxCheck++;
                                    }
                                }

                                if (TRmulti4.Count > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_Guilford_Multicount"] = "Maximum";
                                }
                                else
                                {
                                    HttpContext.Current.Session["multiparcel_Guilford"] = "Yes";
                                }
                                driver.Quit();
                                return "MultiParcel";
                            }
                            else
                            {
                                driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_OwnerSearchResultsGridView']/tbody/tr[2]/td[3]/a")).Click();


                            }
                        }
                        catch { }

                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[3]/table/tbody/tr[2]/td[2]/div/div/div/div/table"));
                        if(INodata.Text.Contains("0 Records Matched Search Criteria"))
                        {
                            HttpContext.Current.Session["Nodata_NCGuilford"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    //property_details

                    //PIN #~Location Address~Property Description~Property Owner~City~Land Class~Acreage~Year Built
                    string parcel_no = "", pin = "", location_address = "", PropertyDescription = "", PropertyOwner = "";
                    string city = "", Land_class = "", Acreage = "", year_built = "";
                    string reid = driver.FindElement(By.Id("ctl00_PageHeader1_ReidLabelInfo")).Text.Trim();
                    parcel_no = driver.FindElement(By.Id("ctl00_PageHeader1_ReidLabelInfo")).Text.Trim();
                    gc.CreatePdf(orderNumber, parcel_no, "property details", driver, "NC", "Guilford");
                    pin = driver.FindElement(By.Id("ctl00_PageHeader1_PinLabelInfo")).Text.Trim();
                    location_address = driver.FindElement(By.Id("ctl00_PageHeader1_LocationAddressLabelInfo")).Text.Trim();
                    PropertyDescription = driver.FindElement(By.Id("ctl00_PageHeader1_PropertyDescriptionLabelInfo")).Text.Trim();
                    PropertyOwner = driver.FindElement(By.Id("ctl00_PageHeader1_DetailsView1")).Text.Trim();
                    string bulkpropertytext = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_DetailsView5']/tbody")).Text;
                    city = gc.Between(bulkpropertytext, "City", "Fire District").Trim();
                    Land_class = gc.Between(bulkpropertytext, "Land Class", "History REID 1").Trim();
                    Acreage = gc.Between(bulkpropertytext, "Acreage", "Permit Date").Trim();

                    driver.FindElement(By.Id("ctl00_PageHeader1_BuildingsHyperLink")).Click();
                    gc.CreatePdf(orderNumber, parcel_no, "Building details", driver, "NC", "Guilford");
                    try
                    {
                        year_built = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_DetailsView4_Label1")).Text.Trim();
                        //year_built = gc.Between(year_built, "", "").Trim();
                    }
                    catch { }

                    string Land_Value = "", Building_Value = "", Outbuilding_Value = "", Appraised_Value = "", OtherExemptions = "", UseValueDeferred = "", Historic_Value_Deferred = "", Total_Deferred_Value = "", Total_Assessed_Value = "";
                    string bulkassessmenttext = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_table8']/tbody")).Text; ;


                    string currentHandle = driver.CurrentWindowHandle;
                    IWebElement element = driver.FindElement(By.LinkText("Print Property Info"));
                    PopupWindowFinder finder = new PopupWindowFinder(driver);
                    string popupWindowHandle = finder.Click(element);
                    driver.SwitchTo().Window(popupWindowHandle);
                    gc.CreatePdf(orderNumber, parcel_no, "Assessment Details", driver, "NC", "Guilford");
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
                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + assesscolumn + "' where Id = '" + 599 + "'");
                    gc.insert_date(orderNumber, parcel_no, 599, assessvalue, 1, DateTime.Now);



                    //     Total Appraised Land Value~Total Appraised Building Value~Total Appraised Outbuilding Value~Total Appraised Value~Other Exemptions~Use Value Deferred~Historic Value Deferred~Total Deferred Value~Total Assessed Value
                    //Land_Value = gc.Between(bulkassessmenttext, "TotalAppraisedLandValue", "TotalAppraisedBuildingValue").Trim();
                    //Building_Value = gc.Between(bulkassessmenttext, "TotalAppraisedBuildingValue", "TotalAppraisedOutbuildingValue").Trim();
                    //Outbuilding_Value = gc.Between(bulkassessmenttext, "TotalAppraisedOutbuildingValue", "TotalAppraisedValue").Trim();
                    //Appraised_Value = gc.Between(bulkassessmenttext, "TotalAppraisedValue", "OtherExemptions").Trim();
                    //OtherExemptions = gc.Between(bulkassessmenttext, "OtherExemptions", "UseValueDeferred").Trim();
                    //UseValueDeferred = gc.Between(bulkassessmenttext, "UseValueDeferred", "HistoricValueDeferred").Trim();
                    //Historic_Value_Deferred = gc.Between(bulkassessmenttext, "HistoricValueDeferred", "TotalDeferredValue").Trim();

                    //Total_Deferred_Value = gc.Between(bulkassessmenttext, "TotalDeferredValue", "TotalAssessedValue").Trim();
                    //Total_Assessed_Value = GlobalClass.After(bulkassessmenttext, "TotalAssessedValue").Trim();



                    string property_details = pin + "~" + location_address + "~" + PropertyDescription + "~" + PropertyOwner + "~" + city + "~" + Land_class + "~" + Acreage + "~" + year_built;
                    gc.insert_date(orderNumber, parcel_no, 598, property_details, 1, DateTime.Now);

                    //string assessment_details = Land_Value + "~" + Building_Value + "~" + Outbuilding_Value + "~" + Appraised_Value + "~" + OtherExemptions + "~" + UseValueDeferred + "~" + Historic_Value_Deferred + "~" + Total_Deferred_Value + "~" + Total_Assessed_Value;
                    //gc.insert_date(orderNumber, parcel_no, 599, assessment_details, 1, DateTime.Now);


                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    //Tax Details
                    driver.Navigate().GoToUrl("http://taxweb.guilfordcountync.gov/publicwebaccess/BillSearchResults.aspx?ParcelNum=" + reid);

                    //IWebElement Itaxstmt = driver.FindElement(By.XPath("//*[@id='ctl00_PageHeader1_TaxBillHyperLink']"));
                    //string stmt1 = Itaxstmt.GetAttribute("href");
                    //driver.Navigate().GoToUrl(stmt1);
                    //Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, parcel_no, "Tax bill table", driver, "NC", "Guilford");

                    //Transaction payment Details Table:
                    //Bill#~Old Bill #~Parcel #~Name~Location~Bill Flags~Current Due~Total:

                    IWebElement tbmulti = driver.FindElement(By.XPath("//*[@id='G_dgResults']/tbody"));
                    IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));

                    IList<IWebElement> TDmulti;
                    IList<IWebElement> THmulti;
                    int b = 0;
                    foreach (IWebElement row in TRmulti)
                    {

                        TDmulti = row.FindElements(By.TagName("td"));
                        THmulti = row.FindElements(By.TagName("th"));
                        if (b < 3 && TDmulti.Count != 0)
                        {
                            IWebElement ITaxBillCount = TDmulti[0].FindElement(By.TagName("a"));
                            string strTaxReal = ITaxBillCount.GetAttribute("href");
                            strTaxRealestate.Add(strTaxReal);
                            b++;
                        }
                        if (TDmulti.Count == 7)
                        {
                            string transpay1 = TDmulti[0].Text + "~" + TDmulti[1].Text + "~" + TDmulti[2].Text + "~" + TDmulti[3].Text + "~" + TDmulti[4].Text + "~" + TDmulti[5].Text + "~" + TDmulti[6].Text;
                            gc.insert_date(orderNumber, parcel_no, 601, transpay1, 1, DateTime.Now);

                        }
                        if (THmulti.Count != 0)
                        {
                            string transhistory11 = "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "Total" + "~" + THmulti[7].Text;
                            gc.insert_date(orderNumber, parcel_no, 601, transhistory11, 1, DateTime.Now);

                        }

                    }
                    int k = 1;
                    foreach (string real in strTaxRealestate)
                    {
                        driver.Navigate().GoToUrl(real);
                        Thread.Sleep(4000);
                        if (k == 1)
                        {
                            //Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, parcel_no, "Tax bill1", driver, "NC", "Guilford");
                            string owner_name = "", Description = "", Location = "", Parcel = "", Lender = "", RealValue = "", DeferredValue = "", UseValue = "", PersonalValue = "", ExemptValue = "", TotalAssessedValue = "";
                            //Owner Name~Description~Location~Lender~Real Value~Deferred Value~Use Value~Personal Value~Exempt & Exclusion Value~Total Assessed Value~Bill Status~Bill Flag~Bill Number~Old Bill Number~Old Account Number~Due Date~Interest Begins~Rate~Tax Districts~Descriptions~Amount~Interest Amount~Total Amount Due~Tax Authority
                            owner_name = driver.FindElement(By.Id("txtName")).Text.Trim();
                            Description = driver.FindElement(By.Id("lblDescr")).Text.Trim();
                            Location = driver.FindElement(By.Id("lblPropAddr")).Text.Trim();
                            Parcel = driver.FindElement(By.Id("lblParcel")).Text.Trim();
                            Lender = driver.FindElement(By.Id("lblLender")).Text.Trim();
                            RealValue = driver.FindElement(By.Id("lblRealOriginal")).Text.Trim();
                            DeferredValue = driver.FindElement(By.Id("lblDeferredOriginal")).Text.Trim();
                            UseValue = driver.FindElement(By.Id("lblUseOriginal")).Text.Trim();
                            PersonalValue = driver.FindElement(By.Id("lblPersonalOriginal")).Text.Trim();
                            ExemptValue = driver.FindElement(By.Id("lblExemptOriginal")).Text.Trim();
                            TotalAssessedValue = driver.FindElement(By.Id("lblTotalValue")).Text.Trim();
                            string BillStatus = "", BillFlag = "", Bill = "", OldBill = "", OldAccountNum = "", DueDate = "", InterestBegins = "", TotalAmountDue = "", InterestAmt = "";
                            BillStatus = driver.FindElement(By.Id("lblBillStatus")).Text.Trim();
                            BillFlag = driver.FindElement(By.Id("lblBillFlag")).Text.Trim();
                            Bill = driver.FindElement(By.Id("lblBill")).Text.Trim();
                            OldBill = driver.FindElement(By.Id("lblLegacyBillNum")).Text.Trim();
                            OldAccountNum = driver.FindElement(By.Id("lblLegacyAccountNum")).Text.Trim();
                            DueDate = driver.FindElement(By.Id("lblDueDate")).Text.Trim();

                            InterestBegins = driver.FindElement(By.Id("lblInterest")).Text.Trim();
                            Thread.Sleep(5000);
                            InterestAmt = driver.FindElement(By.Id("lblInterestAmt")).Text.Trim();
                            TotalAmountDue = driver.FindElement(By.Id("lblTotalAmountDue")).Text.Trim();
                            string TaxBill_details = owner_name + "~" + Description + "~" + Location + "~" + Lender + "~" + RealValue + "~" + DeferredValue + "~" + UseValue + "~" + PersonalValue + "~" + ExemptValue + "~" + TotalAssessedValue + "~" + BillStatus + "~" + BillFlag + "~" + Bill + "~" + OldBill + "~" + OldAccountNum + "~" + DueDate + "~" + InterestBegins + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + InterestAmt + "~" + TotalAmountDue + "~" + " 400 West Market St, Greensboro, North Carolina 27401 Phone: (336) 641 - 3363";
                            gc.insert_date(orderNumber, parcel_no, 600, TaxBill_details, 1, DateTime.Now);


                            IWebElement tbmulti2 = driver.FindElement(By.XPath("//*[@id='dgShowResultRate']/tbody"));
                            IList<IWebElement> TRmulti2 = tbmulti2.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDmulti2;

                            foreach (IWebElement row in TRmulti2)
                            {

                                TDmulti2 = row.FindElements(By.TagName("td"));
                                if (!row.Text.Contains("Rate"))
                                {
                                    if (TDmulti2.Count == 4)
                                    {
                                        string transbill = "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + TDmulti2[0].Text + "~" + TDmulti2[1].Text + "~" + TDmulti2[2].Text + "~" + TDmulti2[3].Text + "" + "~" + "" + "~" + "";
                                        gc.insert_date(orderNumber, parcel_no, 600, transbill, 1, DateTime.Now);
                                    }
                                }
                            }
                            //Transaction History Details Table:
                            //Date~Type~Paid By~Trans #~Amount~Current  Due

                            string CurrentDue = driver.FindElement(By.Id("lblCurrentDue")).Text.Trim();
                            IWebElement tbmulti1 = driver.FindElement(By.XPath("//*[@id='dgShowResultHistory']/tbody"));
                            IList<IWebElement> TRmulti1 = tbmulti1.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDmulti1;

                            foreach (IWebElement row in TRmulti1)
                            {

                                TDmulti1 = row.FindElements(By.TagName("td"));

                                if (TDmulti1.Count == 6)
                                {
                                    string transhistory = TDmulti1[0].Text + "~" + TDmulti1[1].Text + "~" + TDmulti1[2].Text + "~" + TDmulti1[3].Text + "~" + TDmulti1[4].Text + "~" + "";
                                    gc.insert_date(orderNumber, parcel_no, 602, transhistory, 1, DateTime.Now);

                                }

                            }
                            string transhistory1 = "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + CurrentDue;
                            gc.insert_date(orderNumber, parcel_no, 602, transhistory1, 1, DateTime.Now);



                        }
                        if (k == 2)
                        {
                            gc.CreatePdf(orderNumber, parcel_no, "Tax bill2", driver, "NC", "Guilford");
                        }
                        if (k == 3)
                        {
                            gc.CreatePdf(orderNumber, parcel_no, "Tax bill3", driver, "NC", "Guilford");
                        }
                        k++;
                    }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "NC", "Guilford", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    GlobalClass.titleparcel = "";
                    gc.mergpdf(orderNumber, "NC", "Guilford");
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