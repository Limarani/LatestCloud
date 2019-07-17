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
    public class WebDriver_RichlandSC
    {

        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();

        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_RichlandSC(string address, string parcelNumber, string searchType, string ownername, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
             using (driver = new PhantomJSDriver())
            {
               // driver = new ChromeDriver();
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("http://www.richlandonline.com/Online-Services/Property-Value-Tax-Estimate");
                    Thread.Sleep(4000);

                    if (searchType == "titleflex")
                    {
                        string titleaddress = address;
                        gc.TitleFlexSearch(orderNumber, "", ownername, "", "SC", "Richland");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_Richland"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {

                        driver.FindElement(By.Id("txtLocation")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "SC", "Richland");
                        driver.FindElement(By.XPath("//*[@id='cmdSubmit1']")).SendKeys(Keys.Enter);
                        Thread.Sleep(4000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search result", driver, "SC", "Richland");
                        try
                        {
                            int Max = 0;
                            string strowner = "", strAddress = "", strCity = "";
                            string returnStatement = "";
                            IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='DataGrid1']/tbody"));
                            IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> multiTD;
                            foreach (IWebElement multi in multiRow)
                            {
                                multiTD = multi.FindElements(By.TagName("td"));
                                if (multiTD.Count != 0 && multiRow.Count <= 3 && !multi.Text.Contains("Tax Map Number"))
                                {
                                    for (int colCount = 0; colCount < multiRow.Count; colCount++)
                                    {
                                        IList<IWebElement> SingleDataGridTableTd = multiRow[colCount].FindElements(By.TagName("td"));
                                        for (int columnContent = 0; columnContent < SingleDataGridTableTd.Count; columnContent++)
                                        {
                                            if (colCount > 0)
                                            {
                                                if (SingleDataGridTableTd[columnContent].Text.Contains("View"))
                                                {
                                                    ((IJavaScriptExecutor)driver).ExecuteScript("window.confirm = function(msg) { return true; }");
                                                    SingleDataGridTableTd[columnContent].FindElement(By.TagName("a")).Click();
                                                    Thread.Sleep(5000);
                                                    // gc.CreatePdf(orderNumber, parcelNumber, "Property Details", driver, "SC", "Richland");
                                                    Max++;
                                                    returnStatement = "one";
                                                    goto loopend;

                                                }

                                            }
                                        }

                                    }
                                    loopend:;


                                }
                                if (multiTD.Count != 0 && multiRow.Count > 25)
                                {
                                    HttpContext.Current.Session["multiparcel_Richland_Maximum"] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }
                                if (multiTD.Count != 0 && multiRow.Count >= 4 && multiRow.Count <= 25 && !multi.Text.Contains("Tax Map Number") && multi.Text.Trim() != "")
                                {
                                    try
                                    {
                                        strowner = multiTD[4].Text;
                                        strAddress = multiTD[2].Text;
                                        parcelNumber = multiTD[1].Text;

                                        string multidetails = strowner + "~" + strAddress;
                                        gc.insert_date(orderNumber, parcelNumber, 1494, multidetails, 1, DateTime.Now);
                                        Max++;
                                    }
                                    catch { }
                                }
                            }
                            if (Max > 1 && Max < 26)
                            {
                                HttpContext.Current.Session["multiparcel_Richland"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Max == 0)
                            {
                                HttpContext.Current.Session["Nodata_Richland"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }




                    else if (searchType == "parcel")
                    {

                        driver.FindElement(By.Id("TxtProperty")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "parcel search", driver, "SC", "Richland");
                        driver.FindElement(By.Id("cmdSubmit1")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcelNumber, "parcel search Result", driver, "SC", "Richland");
                        try
                        {

                            string returnStatement = "";
                            IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='DataGrid1']/tbody"));
                            IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> multiTD;
                            foreach (IWebElement multi in multiRow)
                            {
                                multiTD = multi.FindElements(By.TagName("td"));
                                if (multiTD.Count != 0 && multiRow.Count <= 3)
                                {
                                    for (int colCount = 0; colCount < multiRow.Count; colCount++)
                                    {
                                        IList<IWebElement> SingleDataGridTableTd = multiRow[colCount].FindElements(By.TagName("td"));
                                        for (int columnContent = 0; columnContent < SingleDataGridTableTd.Count; columnContent++)
                                        {
                                            if (colCount > 0)
                                            {
                                                if (SingleDataGridTableTd[columnContent].Text.Contains("View"))
                                                {
                                                    ((IJavaScriptExecutor)driver).ExecuteScript("window.confirm = function(msg) { return true; }");
                                                    SingleDataGridTableTd[columnContent].FindElement(By.TagName("a")).Click();
                                                    Thread.Sleep(5000);
                                                    // gc.CreatePdf(orderNumber, parcelNumber, "Property Details", driver, "SC", "Richland");
                                                    returnStatement = "one";
                                                    goto loopend;
                                                    break;
                                                }

                                            }
                                        }

                                    }
                                    loopend:;


                                }
                            }
                        }
                        catch { }
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.Id("lblResponse"));
                        if(INodata.Text.Contains("no matches found"))
                        {
                            HttpContext.Current.Session["Nodata_Richland"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    //property details

                    string OwnerName1 = "", OwnerName2 = "", OwnerName = "", Propertylocation = "", legaldesc = "", landtype = "";
                    IWebElement parcelNo = driver.FindElement(By.Id("txtOwnerAcct"));
                    parcelNumber = parcelNo.GetAttribute("value").Trim();
                    string Fname = "";

                    var chromeOptions = new ChromeOptions();
                    var driver1 = new ChromeDriver(chromeOptions);
                    try
                    {

                        driver1.Navigate().GoToUrl("http://www.richlandonline.com/Online-Services/Property-Value-Tax-Estimate");
                        Thread.Sleep(4000);

                        driver1.FindElement(By.Id("TxtProperty")).SendKeys(parcelNumber);

                        driver1.FindElement(By.Id("cmdSubmit1")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);

                        try
                        {

                            string returnStatement = "";
                            IWebElement multiaddress = driver1.FindElement(By.XPath("//*[@id='DataGrid1']/tbody"));
                            IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> multiTD;
                            foreach (IWebElement multi in multiRow)
                            {
                                multiTD = multi.FindElements(By.TagName("td"));
                                if (multiTD.Count != 0 && multiRow.Count <= 3)
                                {
                                    for (int colCount = 0; colCount < multiRow.Count; colCount++)
                                    {
                                        IList<IWebElement> SingleDataGridTableTd = multiRow[colCount].FindElements(By.TagName("td"));
                                        for (int columnContent = 0; columnContent < SingleDataGridTableTd.Count; columnContent++)
                                        {
                                            if (colCount > 0)
                                            {
                                                if (SingleDataGridTableTd[columnContent].Text.Contains("View"))
                                                {
                                                    ((IJavaScriptExecutor)driver1).ExecuteScript("window.confirm = function(msg) { return true; }");
                                                    SingleDataGridTableTd[columnContent].FindElement(By.TagName("a")).Click();
                                                    Thread.Sleep(5000);
                                                    //gc.CreatePdf(orderNumber, parcelNumber, "Property Details", driver, "SC", "Richland");
                                                    returnStatement = "one";
                                                    goto loopend;
                                                    break;
                                                }

                                            }
                                        }

                                    }
                                    loopend:;


                                }
                            }
                        }
                        catch { }


                        gc.CreatePdf(orderNumber, parcelNumber, "Property Details", driver1, "SC", "Richland");

                        IWebElement ProInfo = driver1.FindElement(By.XPath("//*[@id='DataGrid1']/tbody"));
                        IList<IWebElement> TRProInfo = ProInfo.FindElements(By.TagName("tr"));
                        IList<IWebElement> THProInfo = ProInfo.FindElements(By.TagName("th"));
                        IList<IWebElement> TDProInfo;
                        foreach (IWebElement row1 in TRProInfo)
                        {
                            TDProInfo = row1.FindElements(By.TagName("td"));
                            if (!row1.Text.Contains("Current  ") && TDProInfo.Count != 0 && row1.Text.Trim() != "" && TDProInfo.Count >= 3)
                            {


                            }
                        }
                        ByVisibleElement(driver1.FindElement(By.Id("lblLegalInfo")), driver1);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Property Details1", driver1, "SC", "Richland");

                        IWebElement TaxInfo1 = driver1.FindElement(By.XPath("//*[@id='DataGrid3']/tbody"));
                        IList<IWebElement> TRTaxInfo1 = TaxInfo1.FindElements(By.TagName("tr"));
                        IList<IWebElement> THTaxInfo1 = TaxInfo1.FindElements(By.TagName("th"));
                        IList<IWebElement> TDTaxInfo1;
                        foreach (IWebElement row1 in TRTaxInfo1)
                        {
                            TDTaxInfo1 = row1.FindElements(By.TagName("td"));
                            if (!row1.Text.Contains("Structure ") && TDTaxInfo1.Count != 0 && row1.Text.Trim() != "" && TDTaxInfo1.Count >= 3)
                            {


                            }
                        }
                        ByVisibleElement(driver1.FindElement(By.XPath("//*[@id='DataGrid3']/tbody")), driver1);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Property Details2", driver1, "SC", "Richland");
                        driver1.Quit();


                    }
                    catch { }

                    IWebElement StrOwnerName1 = driver.FindElement(By.Id("txtOwner"));
                    OwnerName1 = StrOwnerName1.GetAttribute("value").Trim();
                    IWebElement StrOwnerName2 = driver.FindElement(By.Id("txtAddress1"));
                    OwnerName2 = StrOwnerName2.GetAttribute("value").Trim();
                    OwnerName = OwnerName1 + " " + OwnerName2;
                    IWebElement StrPropertylocation = driver.FindElement(By.Id("txtPropLocation"));
                    Propertylocation = StrPropertylocation.GetAttribute("value");
                    IWebElement Strlegal = driver.FindElement(By.Id("txtLegalDesc1"));
                    legaldesc = Strlegal.GetAttribute("value").Trim();
                    IWebElement Strlandtype = driver.FindElement(By.Id("txtLandTypeDesc"));
                    landtype = Strlandtype.GetAttribute("value").Trim();


                    string propertydetails = OwnerName + "~" + Propertylocation + "~" + legaldesc + "~" + landtype;
                    gc.insert_date(orderNumber, parcelNumber, 1491, propertydetails, 1, DateTime.Now);
                    Thread.Sleep(4000);

                    // Assessment Details
                    string YearofAss = "", Taxdistrict = "", Acreofparcel = "", NonAgrivalue = "", Buildingvalue = "", Taxablvalue = "", SewerConnection = "", WaterConnection = "";
                    string Zoning = "", Legalresidence = "", Agrivalue = "", Improvements = "";
                    IWebElement IYearofAss = driver.FindElement(By.Id("txtAssesYR"));
                    YearofAss = IYearofAss.GetAttribute("value").Trim();
                    IWebElement ITaxdistrict = driver.FindElement(By.Id("txtTaxDistCd"));
                    Taxdistrict = ITaxdistrict.GetAttribute("value").Trim();
                    IWebElement IAcreofparcel = driver.FindElement(By.Id("txtAcreage"));
                    Acreofparcel = IAcreofparcel.GetAttribute("value").Trim();
                    IWebElement INonAgrivalue = driver.FindElement(By.Id("txtLandValue"));
                    NonAgrivalue = INonAgrivalue.GetAttribute("value").Trim();
                    IWebElement IBuildingvalue = driver.FindElement(By.Id("txtBuildValue"));
                    Buildingvalue = IBuildingvalue.GetAttribute("value").Trim();
                    IWebElement ITaxablvalue = driver.FindElement(By.Id("txtMarketValue"));
                    Taxablvalue = ITaxablvalue.GetAttribute("value").Trim();
                    IWebElement IZoning = driver.FindElement(By.Id("txtZoneCode"));
                    Zoning = IZoning.GetAttribute("value").Trim();
                    IWebElement ILegalresidence = driver.FindElement(By.Id("txtPCA"));
                    Legalresidence = ILegalresidence.GetAttribute("value").Trim();

                    IWebElement IsewerConnection = driver.FindElement(By.Id("txtSewerDesc"));
                    SewerConnection = IsewerConnection.GetAttribute("value").Trim();
                    IWebElement IWaterConnection = driver.FindElement(By.Id("txtWaterDesc"));
                    WaterConnection = IWaterConnection.GetAttribute("value").Trim();

                    IWebElement IAgrivalue = driver.FindElement(By.Id("txtAgValue"));
                    Agrivalue = IAgrivalue.GetAttribute("value").Trim();
                    IWebElement IImprovements = driver.FindElement(By.Id("txtImpr"));
                    Improvements = IImprovements.GetAttribute("value").Trim();

                    string Assessmentdetails = YearofAss + "~" + Taxdistrict + "~" + Acreofparcel + "~" + NonAgrivalue + "~" + Buildingvalue + "~" + Taxablvalue + "~" + Zoning + "~" + Legalresidence + "~" + SewerConnection + "~" + WaterConnection + "~" + Agrivalue + "~" + Improvements;
                    gc.insert_date(orderNumber, parcelNumber, 1492, Assessmentdetails, 1, DateTime.Now);
                    Thread.Sleep(5000);

                    // Current Tax Details

                    string Taxyear = "", TaxRelief = "", salestaxcredit = "", TaxAmount = "", Paid = "", Homestead = "", Assessed = "";
                    IWebElement strTaxyear = driver.FindElement(By.Id("txtYear"));
                    Taxyear = strTaxyear.GetAttribute("value").Trim();
                    IWebElement strTaxRelief = driver.FindElement(By.Id("TxtStatecr"));
                    TaxRelief = strTaxRelief.GetAttribute("value").Trim();
                    IWebElement strsalestaxcredit = driver.FindElement(By.Id("txtResidentcr"));
                    salestaxcredit = strsalestaxcredit.GetAttribute("value").Trim();
                    IWebElement strTaxAmount = driver.FindElement(By.Id("txtTbilled"));
                    TaxAmount = strTaxAmount.GetAttribute("value").Trim();
                    IWebElement strPaid = driver.FindElement(By.Id("txtPaidflag"));
                    Paid = strPaid.GetAttribute("value").Trim();
                    IWebElement strHomestead = driver.FindElement(By.Id("txtHSFlag"));
                    Homestead = strHomestead.GetAttribute("value").Trim();
                    IWebElement strAssessed = driver.FindElement(By.Id("txtTotAssd"));
                    Assessed = strAssessed.GetAttribute("value").Trim();

                    //gc.CreatePdf(orderNumber, parcelNumber, "Property Details5", driver, "SC", "Richland");
                    string Taxdetails = Taxyear + "~" + TaxRelief + "~" + salestaxcredit + "~" + TaxAmount + "~" + Paid + "~" + Homestead + "~" + Assessed;
                    gc.insert_date(orderNumber, parcelNumber, 1493, Taxdetails, 1, DateTime.Now);

                    // Taxing Authority

                    driver.Navigate().GoToUrl("http://www.richlandonline.com/Government/Departments/Taxes/Treasurer");
                    Thread.Sleep(5000);
                    string taxAuth = driver.FindElement(By.XPath("//*[@id='dnn_ctr1429_HtmlModule_lblContent']/p[2]")).Text.Replace("Office Hours:", "").Replace("8:30 a.m. - 5:00 p.m.", "").Replace("\r\n", " ");
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Authority", driver, "SC", "Richland");

                    // Tax Detail Table

                    string Tax_Year = "", ParcelNo = "";
                    int Syear = DateTime.Now.Year;
                    int Smonth = DateTime.Now.Month;
                    int iyear = 0;
                    int i = 0;
                    if (Smonth >= 9)
                    {
                        iyear = Syear;
                    }
                    else
                    {
                        iyear = Syear - 1;
                    }
                    for (i = 0; i <= 2; i++)
                    {
                        driver.Navigate().GoToUrl("https://www5.rcgov.us/TreasurerTaxInfo/Main.aspx");
                        Thread.Sleep(5000);

                        try
                        {
                            IWebElement IRealEstateClick = driver.FindElement(By.Id("mnuMain"));
                            IList<IWebElement> IRealEstateRow = IRealEstateClick.FindElements(By.TagName("tr"));
                            IList<IWebElement> IRealEstateTD;
                            foreach (IWebElement realestate in IRealEstateRow)
                            {
                                IRealEstateTD = realestate.FindElements(By.TagName("table"));
                                if (IRealEstateTD.Count != 0 && realestate.Text.Contains(" Real Estate "))
                                {
                                    IWebElement IRealEstate = IRealEstateTD[1].FindElement(By.TagName("a"));
                                    IRealEstate.Click();
                                    Thread.Sleep(2000);
                                    break;
                                }
                            }
                        }
                        catch { }



                        ParcelNo = parcelNumber.Replace("R", "").Replace("-", "").Trim();//*[@id="txtYearRealEstate3"]
                        driver.FindElement(By.Id("txtYearRealEstate3")).SendKeys("onkeypress");
                        driver.FindElement(By.Id("txtYearRealEstate3")).SendKeys(Convert.ToString(iyear));
                        driver.ExecuteJavaScript("document.getElementById('txtTMSRealEstate').setAttribute('value', '" + ParcelNo + "')");
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Search", driver, "SC", "Richland");
                        driver.FindElement(By.Id("btnSubmitRealEstate")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Search Result", driver, "SC", "Richland");
                        try
                        {
                            driver.FindElement(By.LinkText("Select")).SendKeys(Keys.Enter);
                            Thread.Sleep(2000);
                        }
                        catch { }
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Information Details" + iyear, driver, "SC", "Richland");
                        string taxyear = "", taxdata = "", Taxmapno = "", LegalRes = "", PropertyDesc = "", Assessedval = "";
                        string PayAmountBefore1 = "", PayAmountBefore2 = "", AmountDue1 = "", AmountDue2 = "", AmountDue3 = "";
                        string AmountDueDate1 = "", AmountDueDate2 = "", AmountDueDate3 = "";
                        taxyear = driver.FindElement(By.Id("lblTaxInfoRYear")).Text;
                        taxdata = driver.FindElement(By.XPath("//*[@id='formTaxInfo']/table/tbody/tr/td/table[3]/tbody/tr/td/table[2]/tbody")).Text;

                        IWebElement TaxDue = driver.FindElement(By.XPath("//*[@id='formTaxInfo']/table/tbody/tr/td/table[3]/tbody/tr/td/table[2]/tbody"));
                        IList<IWebElement> TRTaxDue = TaxDue.FindElements(By.TagName("tr"));
                        IList<IWebElement> THTaxDue = TaxDue.FindElements(By.TagName("th"));
                        IList<IWebElement> TDTaxDue;
                        foreach (IWebElement row3 in TRTaxDue)
                        {
                            TDTaxDue = row3.FindElements(By.TagName("td"));
                            if (!row3.Text.Contains("Legal Residence") && TDTaxDue.Count != 0 && row3.Text.Trim() != "")
                            {
                                Taxmapno = TDTaxDue[0].Text;
                                LegalRes = TDTaxDue[1].Text;
                                PropertyDesc = TDTaxDue[2].Text;
                                Assessedval = TDTaxDue[3].Text;
                            }
                        }
                        int j = 0;
                        IWebElement Taxvalue = driver.FindElement(By.XPath(" //*[@id='formTaxInfo']/table/tbody/tr/td/table[3]/tbody/tr/td/table[3]/tbody"));
                        IList<IWebElement> TRTaxvalue = Taxvalue.FindElements(By.TagName("tr"));
                        IList<IWebElement> THTaxvalue = Taxvalue.FindElements(By.TagName("th"));
                        IList<IWebElement> TDTaxvalue;
                        foreach (IWebElement row in TRTaxvalue)
                        {
                            TDTaxvalue = row.FindElements(By.TagName("td"));
                            if (!row.Text.Contains("Pay This Amount Before") && TDTaxvalue.Count != 0 && row.Text.Trim() != "" && !row.Text.Contains("Amount Due"))
                            {
                                if (j == 0)
                                {
                                    try
                                    {
                                        PayAmountBefore1 = TDTaxvalue[0].Text;
                                        AmountDueDate1 = TDTaxvalue[1].Text;
                                    }
                                    catch { }
                                    try
                                    {
                                        AmountDueDate2 = TDTaxvalue[2].Text;
                                        AmountDueDate3 = TDTaxvalue[3].Text;
                                        j++;
                                    }
                                    catch { }
                                }
                                if (j >= 1)
                                {
                                    try
                                    {
                                        PayAmountBefore2 = TDTaxvalue[0].Text;
                                        AmountDue1 = TDTaxvalue[1].Text;
                                    }
                                    catch { }
                                    try
                                    {
                                        AmountDue2 = TDTaxvalue[2].Text;
                                        AmountDue3 = TDTaxvalue[3].Text;
                                        j++;
                                    }
                                    catch { }
                                }

                            }
                        }
                        if (AmountDueDate1 != "" && AmountDue1 != "")
                        {
                            try
                            {
                                string TaxInfoDetail1 = taxyear + "~" + Taxmapno + "~" + LegalRes + "~" + PropertyDesc + "~" + Assessedval + "~" + PayAmountBefore1 + "~" + AmountDueDate1 + "~" + AmountDue1;
                                gc.insert_date(orderNumber, parcelNumber, 1499, TaxInfoDetail1, 1, DateTime.Now);

                            }
                            catch { }
                        }
                        if (AmountDueDate2 != "" && AmountDue2 != "")
                        {
                            try
                            {
                                string TaxInfoDetail2 = taxyear + "~" + Taxmapno + "~" + LegalRes + "~" + PropertyDesc + "~" + Assessedval + "~" + PayAmountBefore2 + "~" + AmountDueDate2 + "~" + AmountDue2;
                                gc.insert_date(orderNumber, parcelNumber, 1499, TaxInfoDetail2, 1, DateTime.Now);

                            }
                            catch { }
                        }
                        if (AmountDueDate3 != "" && AmountDue3 != "")
                        {
                            try
                            {
                                string TaxInfoDetail3 = taxyear + "~" + Taxmapno + "~" + LegalRes + "~" + PropertyDesc + "~" + Assessedval + "~" + "" + "~" + AmountDueDate3 + "~" + AmountDue3;
                                gc.insert_date(orderNumber, parcelNumber, 1499, TaxInfoDetail3, 1, DateTime.Now);

                            }
                            catch { }
                        }

                        string taxdata2 = driver.FindElement(By.XPath("//*[@id='formTaxInfo']/table/tbody/tr/td/table[3]/tbody/tr/td/table[4]/tbody")).Text;
                        string Homestead_Exem = "", County_Relief = "", SalesTaxcredit = "", solid_Waste = "", cost = "", AmountDue = "", Duedate = "", Datepaid = "", Amountpaid = "", Tax_Remarks = "";

                        Homestead_Exem = gc.Between(taxdata2, "Homestead Exemption", "County Relief").Trim();
                        County_Relief = gc.Between(taxdata2, "County Relief", "Sales Tax Credit").Trim();
                        SalesTaxcredit = gc.Between(taxdata2, "Sales Tax Credit", "Solid Waste").Trim();
                        solid_Waste = gc.Between(taxdata2, "Solid Waste", "Pen/Cost").Trim();
                        cost = gc.Between(taxdata2, "Pen/Cost", "Amount Due").Trim();
                        AmountDue = gc.Between(taxdata2, "Amount Due", "Due Date").Trim();
                        Duedate = gc.Between(taxdata2, "Due Date", "Date Paid").Trim();
                        Datepaid = gc.Between(taxdata2, "Date Paid", "Amount Paid").Trim();
                        Amountpaid = GlobalClass.After(taxdata2, "Amount Paid").Trim();
                        try
                        {
                            if (Amountpaid.Contains("\r\n"))
                            {
                                Tax_Remarks = GlobalClass.After(Amountpaid, "\r\n").Trim();
                                Amountpaid = GlobalClass.Before(Amountpaid, "\r\n").Trim();
                            }
                        }
                        catch { }

                        try
                        {
                            IWebElement TaxInfo = driver.FindElement(By.XPath("//*[@id='formTaxInfo']/table/tbody/tr/td/table[3]/tbody/tr/td/table[4]/tbody"));
                            IList<IWebElement> TRTaxInfo = TaxInfo.FindElements(By.TagName("tr"));
                            IList<IWebElement> THTaxInfo = TaxInfo.FindElements(By.TagName("th"));
                            IList<IWebElement> TDTaxInfo;
                            foreach (IWebElement row1 in TRTaxInfo)
                            {
                                TDTaxInfo = row1.FindElements(By.TagName("td"));
                                if (!row1.Text.Contains("Notice #") && TDTaxInfo.Count != 0 && row1.Text.Trim() != "" && TDTaxInfo.Count >= 6 && TDTaxInfo.Count >= 9)
                                {

                                    string TaxInformation1 = taxyear + "~" + TDTaxInfo[0].Text + "~" + TDTaxInfo[1].Text + "~" + TDTaxInfo[2].Text + "~" + TDTaxInfo[3].Text + TDTaxInfo[4].Text + "~" + TDTaxInfo[5].Text + "~" + TDTaxInfo[6].Text + TDTaxInfo[7].Text + "~" + Homestead_Exem + "~" + County_Relief + "~" + SalesTaxcredit + "~" + solid_Waste + "~" + cost + "~" + AmountDue + "~" + Duedate + "~" + Datepaid + "~" + Amountpaid + "~" + Tax_Remarks + "~" + taxAuth;
                                    gc.insert_date(orderNumber, parcelNumber, 1500, TaxInformation1, 1, DateTime.Now);
                                }
                                if (!row1.Text.Contains("Notice #") && TDTaxInfo.Count != 0 && row1.Text.Trim() != "" && TDTaxInfo.Count >= 6 && TDTaxInfo.Count != 9)
                                {

                                    string TaxInformation1 = taxyear + "~" + "" + "~" + "" + "~" + TDTaxInfo[1].Text + "~" + TDTaxInfo[2].Text + TDTaxInfo[3].Text + "~" + TDTaxInfo[4].Text + "~" + TDTaxInfo[5].Text + TDTaxInfo[6].Text + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                    gc.insert_date(orderNumber, parcelNumber, 1500, TaxInformation1, 1, DateTime.Now);
                                }
                            }
                        }
                        catch { }
                        // Printable Receipt
                        try
                        {
                            driver.FindElement(By.Id("btnTaxInfoPrintReal")).SendKeys(Keys.Enter);
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Tax Printable Info" + iyear, driver, "SC", "Richland");
                        }
                        catch { }
                        string strtax_Year = "", StrMap = "", StrLoc1 = "", StrLoc2 = "", StrLocation = "", Millage = "", Dist = "", Tot_Asm = "", Where = "", Batch = "", Check = "", Clerk = "";
                        string StrTran = "", str_No = "", Bill = "", CurrentPayment = "", CountyTax = "", CountySalesCr = "", str_Homestead = "", CountyRelief = "";
                        string SolidWaste = "", Pen = "", CityTax = "", CitySalesCr = "", CityPen = "", PaidtoDate = "", DatePaid = "", StrAmountDue = "", Remark = "", Remark1 = "", Remark2 = "";
                        try
                        {

                            strtax_Year = driver.FindElement(By.Id("lblRealEstateTaxYear")).Text;
                            StrMap = driver.FindElement(By.Id("lblRealEstateTMSNo")).Text;
                            StrLoc1 = driver.FindElement(By.Id("lblRealEstateTaxLoc1")).Text;
                            StrLoc2 = driver.FindElement(By.Id("lblRealEstateTaxLoc2")).Text;
                            StrLocation = StrLoc1 + " " + StrLoc2;
                            Millage = driver.FindElement(By.Id("lblRealEstateTaxMillage")).Text;
                            Dist = driver.FindElement(By.Id("lblRealEstateTaxDistNo")).Text;
                            Tot_Asm = driver.FindElement(By.Id("lblRealEstateTaxTotAsm")).Text;
                            Where = driver.FindElement(By.Id("lblRealEstateTaxWhere")).Text;
                            Batch = driver.FindElement(By.Id("lblRealEstateTaxBatch")).Text;
                            Check = driver.FindElement(By.Id("lblRealEstateTaxPaidAmount")).Text;
                            Clerk = driver.FindElement(By.Id("lblRealEstateTaxClerk")).Text;
                            StrTran = driver.FindElement(By.Id("lblRealEstateTaxTranNo")).Text;
                            str_No = driver.FindElement(By.Id("lblRealEstateTaxRefNo")).Text;
                            Bill = driver.FindElement(By.Id("lblRealEstateTaxNoticeNo")).Text;
                            CurrentPayment = driver.FindElement(By.Id("lblRealEstateTaxCurrentPayment")).Text;
                            CountyTax = driver.FindElement(By.Id("lblRealEstateTaxCountyTax")).Text;
                            CountySalesCr = driver.FindElement(By.Id("lblRealEstateTaxCountySalesCr")).Text;
                            str_Homestead = driver.FindElement(By.Id("lblRealEstateTaxHomestead")).Text;
                            CountyRelief = driver.FindElement(By.Id("lblRealEstateTaxCountyRelief")).Text;
                            SolidWaste = driver.FindElement(By.Id("lblRealEstateTaxSolidWaste")).Text;
                            Pen = driver.FindElement(By.Id("lblRealEstateTaxCountyPen")).Text;
                            CityTax = driver.FindElement(By.Id("lblRealEstateTaxCityTax")).Text;
                            CitySalesCr = driver.FindElement(By.Id("lblRealEstateTaxCitySalesCr")).Text;
                            CityPen = driver.FindElement(By.Id("lblRealEstateTaxCityPen")).Text;
                            PaidtoDate = driver.FindElement(By.Id("lblRealEstateTaxPaidToDate")).Text;
                            DatePaid = driver.FindElement(By.Id("lblRealEstateTaxPaidDate")).Text;
                            StrAmountDue = driver.FindElement(By.Id("lblRealEstateTaxBalanceDue")).Text;
                            Remark1 = driver.FindElement(By.Id("lblRealEstateTaxRemarkMessage")).Text;
                            Remark2 = driver.FindElement(By.Id("lblRealEstateTaxPaidMessage")).Text;
                            Remark = Remark1 + " " + Remark2;
                            string TaxInformation = strtax_Year + "~" + StrMap + "~" + StrLocation + "~" + Millage + "~" + Dist + "~" + Tot_Asm + "~" + Where + "~" + Batch + "~" + Check + "~" + Clerk + "~" + StrTran + "~" + str_No + "~" + Bill + "~" + CurrentPayment + "~" + CountyTax + "~" + CountySalesCr + "~" + str_Homestead + "~" + CountyRelief + "~" + SolidWaste + "~" + Pen + "~" + CityTax + "~" + CitySalesCr + "~" + CityPen + "~" + PaidtoDate + "~" + DatePaid + "~" + StrAmountDue + "~" + Remark;
                            gc.insert_date(orderNumber, parcelNumber, 1503, TaxInformation, 1, DateTime.Now);
                        }
                        catch { }
                        iyear--;
                    }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "SC", "Richland", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "SC", "Richland");
                    return "Data Inserted Successfully";
                }

                catch (Exception ex)
                {
                    driver.Quit();
                    throw ex;
                }

            }
        }
        public void ByVisibleElement(IWebElement Element, IWebDriver driver1)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver1;
            js.ExecuteScript("arguments[0].scrollIntoView();", Element);
        }
    }
}