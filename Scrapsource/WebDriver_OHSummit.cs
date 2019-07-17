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
    public class WebDriver_OHSummit
    {
        string parcelno = "";
        string outputPath = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();

        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());


        public string FTP_Summit(string Address, string account, string parcelNumber, string searchType, string orderNumber, string ownername, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string ParcellNumber = "";
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                string AlterNateID = "", PropertyAddress = "", LegalDescriptoin = "", YearBuilt = "";
                string Route = "", address = "", parcel = "", owner = "";
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    if (searchType == "titleflex")
                    {
                        string straddress = Address + " " + account;
                        gc.TitleFlexSearch(orderNumber, "", "", straddress, "OH", "Summit");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_OHSummit"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("http://fiscaloffice.summitoh.net/index.php/property-tax-search");
                        Thread.Sleep(2000);
                        IWebElement iframeElement = driver.FindElement(By.XPath("//*[@id='blockrandom']"));
                        //now use the switch command
                        driver.SwitchTo().Frame(iframeElement);
                        driver.FindElement(By.XPath("//*[@id='wrapper']/table/tbody/tr[3]/td/table/tbody/tr[4]/td[2]/input")).SendKeys(Address);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "OH", "Summit");

                        driver.FindElement(By.XPath("//*[@id='wrapper']/table/tbody/tr[3]/td/table/tbody/tr[6]/td/input[1]")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Address search Result", driver, "OH", "Summit");
                        Thread.Sleep(1000);
                        int multi = 0;
                        IList<IWebElement> Propertytable = driver.FindElements(By.XPath("/html/body/table[4]/tbody/tr"));
                        int rowcount = Propertytable.Count;
                        for (int i = 2; i <= rowcount; i++)
                        {
                            try
                            {
                                address = driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[" + i + "]/td[4]")).Text;

                                string ChkCondition = address;
                                var ChkSplit = ChkCondition.Split(' ');
                                ChkCondition = ChkSplit[0] + " " + ChkSplit[1];

                                IWebElement pa = driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[" + i + "]/td[2]/input"));
                                parcel = pa.GetAttribute("value");
                                Route = driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[" + i + "]/td[3]")).Text;
                                address = driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[" + i + "]/td[4]")).Text;
                                owner = driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[" + i + "]/td[5]")).Text;

                                string multiparcedata = Route + "~" + address + "~" + owner;
                                gc.insert_date(orderNumber, parcel, 121, multiparcedata, 1, DateTime.Now);

                                if (address.Contains(ChkCondition.ToUpper()))
                                {
                                    multi++;
                                    pa.Click();
                                    break;
                                }

                                //if(multi!=1 && multi > 1)
                                // {
                                //    HttpContext.Current.Session["multiParcel_Summit"] = "Yes";
                                //    driver.Quit();
                                //    return "MultiParcel";
                                //}
                            }
                            catch { }
                        }
                    }

                    else if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("http://fiscaloffice.summitoh.net/index.php/property-tax-search");
                        Thread.Sleep(2000);
                        IWebElement iframeElement = driver.FindElement(By.XPath("//*[@id='blockrandom']"));
                        //now use the switch command
                        driver.SwitchTo().Frame(iframeElement);
                        driver.FindElement(By.XPath("//*[@id='wrapper']/table/tbody/tr[3]/td/table/tbody/tr[2]/td[2]/input")).SendKeys(parcelNumber);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "M_Parcel Number Search", driver, "OH", "Summit");
                        //  gc.CreatePdf_WOP(orderNumber, "Parcel search",driver, "OH", "Summit");
                        driver.FindElement(By.XPath("//*[@id='wrapper']/table/tbody/tr[3]/td/table/tbody/tr[6]/td/input[1]")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "M_Parcel Number Search", driver, "OH", "Summit");
                        try
                        {
                            IWebElement iframeElement3 = driver.FindElement(By.XPath("/html/frameset/frame[2]"));
                            driver.SwitchTo().Frame(iframeElement3);
                            owner = driver.FindElement(By.XPath("/html/body/table/tbody/tr[9]/td[2]/font")).Text;
                            PropertyAddress = driver.FindElement(By.XPath("/html/body/table/tbody/tr[11]/td[2]/font")).Text;
                            parcelNumber = driver.FindElement(By.XPath("/html/body/table/tbody/tr[7]/td[2]/font")).Text;
                            AlterNateID = driver.FindElement(By.XPath("/html/body/table/tbody/tr[8]/td[2]/font")).Text;
                            LegalDescriptoin = driver.FindElement(By.XPath("/html/body/table/tbody/tr[12]/td[2]/font")).Text;
                            YearBuilt = driver.FindElement(By.XPath("/html/body/table/tbody/tr[25]/td[2]/font")).Text;
                            if (YearBuilt.Contains("\r\n"))
                            { YearBuilt = ""; }
                            string PropertyDetail = AlterNateID + "~" + PropertyAddress + "~" + owner + "~" + LegalDescriptoin + "~" + YearBuilt.Replace("\r\n", "");


                            gc.insert_date(orderNumber, parcelNumber, 120, PropertyDetail, 1, DateTime.Now);
                        }
                        catch { }
                    }
                    else if (searchType == "ownername")
                    {
                        driver.Navigate().GoToUrl("http://fiscaloffice.summitoh.net/index.php/property-tax-search");
                        Thread.Sleep(2000);
                        IWebElement iframeElement = driver.FindElement(By.XPath("//*[@id='blockrandom']"));
                        //now use the switch command
                        driver.SwitchTo().Frame(iframeElement);
                        driver.FindElement(By.XPath("//*[@id='wrapper']/table/tbody/tr[3]/td/table/tbody/tr[5]/td[2]/input")).SendKeys(ownername);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "OwnerName search", driver, "OH", "Summit");

                        driver.FindElement(By.XPath("//*[@id='wrapper']/table/tbody/tr[3]/td/table/tbody/tr[6]/td/input[1]")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Ownername search Result", driver, "OH", "Summit");
                        Thread.Sleep(1000);
                        try
                        {

                            IWebElement iframeElement3 = driver.FindElement(By.XPath("/html/frameset/frame[2]"));
                            driver.SwitchTo().Frame(iframeElement3);
                            owner = driver.FindElement(By.XPath("/html/body/table/tbody/tr[9]/td[2]/font")).Text;
                            PropertyAddress = driver.FindElement(By.XPath("/html/body/table/tbody/tr[11]/td[2]/font")).Text;
                            parcelNumber = driver.FindElement(By.XPath("/html/body/table/tbody/tr[7]/td[2]/font")).Text;
                        }
                        catch
                        {


                        }

                        try
                        {

                            IWebElement MultiOwnerTable = driver.FindElement(By.XPath("/html/body/table[4]"));
                            IList<IWebElement> MultiOwnerRow = MultiOwnerTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiOwnerTD;
                            int i = 2;
                            //string AlterNateID = "", PropertyAddress = "", LegalDescriptoin = "", YearBuilt = "";
                            foreach (IWebElement row1 in MultiOwnerRow)
                            {
                                string ChkMulti = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr/th")).Text;
                                ChkMulti = WebDriverTest.After(ChkMulti, "New Search");
                                ChkMulti = ChkMulti.Replace("\r\n", "");


                                if (ChkMulti == "1 Records Found")
                                {
                                    driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[" + i + "]/td[2]/input")).SendKeys(Keys.Enter);
                                    Thread.Sleep(3000);
                                    iframeElement = driver.FindElement(By.XPath("/html/frameset/frame[2]"));

                                    driver.SwitchTo().Frame(iframeElement);

                                    parcelNumber = driver.FindElement(By.XPath("/html/body/table/tbody/tr[7]/td[2]/font")).Text;
                                    AlterNateID = driver.FindElement(By.XPath("/html/body/table/tbody/tr[8]/td[2]/font")).Text;
                                    PropertyAddress = driver.FindElement(By.XPath("/html/body/table/tbody/tr[11]/td[2]/font")).Text;
                                    LegalDescriptoin = driver.FindElement(By.XPath("/html/body/table/tbody/tr[12]/td[2]/font")).Text;
                                    YearBuilt = driver.FindElement(By.XPath("/html/body/table/tbody/tr[25]/td[2]/font")).Text;
                                    owner = driver.FindElement(By.XPath("/html/body/table/tbody/tr[9]/td[2]/font")).Text;
                                    string PropertyDetail = AlterNateID + "~" + PropertyAddress + "~" + owner + "~" + LegalDescriptoin + "~" + YearBuilt;

                                    gc.insert_date(orderNumber, parcelNumber, 120, PropertyDetail, 1, DateTime.Now);

                                    break;
                                }
                                else
                                {
                                    try
                                    {
                                        IWebElement pa = driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[" + i + "]/td[2]/input"));
                                        parcel = pa.GetAttribute("value");
                                        Route = driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[" + i + "]/td[3]")).Text;
                                        address = driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[" + i + "]/td[4]")).Text;
                                        owner = driver.FindElement(By.XPath("/html/body/table[4]/tbody/tr[" + i + "]/td[5]")).Text;

                                        string multiparcedata = Route + "~" + address + "~" + owner;
                                        gc.insert_date(orderNumber, parcel, 121, multiparcedata, 1, DateTime.Now);
                                        i++;

                                    }
                                    catch
                                    {
                                        HttpContext.Current.Session["multiParcel_Summit"] = "Yes";
                                        driver.Quit();
                                        return "MultiParcel";

                                    }



                                }



                            }
                        }
                        catch { }



                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.XPath("/html/body/center/font"));
                        if(INodata.Text.Contains("No records"))
                        {
                            HttpContext.Current.Session["Nodata_OHSummit"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }


                    if (searchType == "address")
                    {
                        IWebElement iframeElement3 = driver.FindElement(By.XPath("/html/frameset/frame[2]"));
                        driver.SwitchTo().Frame(iframeElement3);
                        owner = driver.FindElement(By.XPath("/html/body/table/tbody/tr[9]/td[2]/font")).Text;
                        PropertyAddress = driver.FindElement(By.XPath("/html/body/table/tbody/tr[11]/td[2]/font")).Text;
                        parcelNumber = driver.FindElement(By.XPath("/html/body/table/tbody/tr[7]/td[2]/font")).Text;
                        AlterNateID = driver.FindElement(By.XPath("/html/body/table/tbody/tr[8]/td[2]/font")).Text;
                        LegalDescriptoin = driver.FindElement(By.XPath("/html/body/table/tbody/tr[12]/td[2]/font")).Text;
                        YearBuilt = driver.FindElement(By.XPath("/html/body/table/tbody/tr[25]/td[2]/font")).Text;
                        if (YearBuilt.Contains("\r\n"))
                        { YearBuilt = ""; }
                        string PropertyDetail = AlterNateID + "~" + PropertyAddress + "~" + owner + "~" + LegalDescriptoin + "~" + YearBuilt.Replace("\r\n", "");


                        gc.insert_date(orderNumber, parcelNumber, 120, PropertyDetail, 1, DateTime.Now);

                    }
                    gc.CreatePdf(orderNumber, parcelNumber, "Property Detail", driver, "OH", "Summit");

                    driver.SwitchTo().DefaultContent();
                    IWebElement Iframe = driver.FindElement(By.XPath("/html/frameset/frame[2]"));
                    driver.SwitchTo().Frame(Iframe);
                    string Year = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/font[2]/b")).Text;

                    Year = WebDriverTest.After(Year, "Tax Year ");
                    string Land = "";

                    string Building = "";

                    string Total = "";
                    string AssessedLand = "";
                    string AssessedBuilding = "";
                    string AssessedTotal = "";
                    string Homestead = "";
                    string OwnerOccupancyCredit = "";
                    string Ok = "";
                    int Increment = 0;
                    int IncrementSecond = 0;

                    IWebElement AssessedTable = driver.FindElement(By.XPath("/html/body/table"));
                    IList<IWebElement> AssessedRow = AssessedTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDAss;
                    foreach (IWebElement row1 in AssessedRow)
                    {

                        TDAss = row1.FindElements(By.TagName("td"));
                        if (Ok == "Allowed" && Increment == 0)
                        {

                            Land = TDAss[1].Text.ToString();
                            Building = TDAss[4].Text.ToString();
                            Total = TDAss[7].Text.ToString();
                            Increment++;

                        }
                        else if (Ok == "Allowed" && Increment == 1)
                        {
                            AssessedLand = TDAss[1].Text.ToString();
                            AssessedBuilding = TDAss[4].Text.ToString();
                            AssessedTotal = TDAss[7].Text.ToString();

                            Increment++;
                        }
                        else if (Ok == "Allowed Second" && IncrementSecond == 0 || IncrementSecond == 1)
                        {

                            IncrementSecond++;
                        }
                        else if (Ok == "Allowed Second" && IncrementSecond == 2)
                        {

                            Homestead = TDAss[1].Text.ToString();
                            IncrementSecond++;
                        }
                        else if (Ok == "Allowed Second" && IncrementSecond == 3)
                        {

                            OwnerOccupancyCredit = TDAss[1].Text.ToString();
                            IncrementSecond++;
                        }
                        if (IncrementSecond == 4)
                        {
                            break;
                        }

                        if (row1.Text.Contains("SUMMARY ALL CARDS FOR PARCEL " + parcelNumber.Replace("-", "")))
                        {
                            Ok = "Allowed";


                        }

                        if (row1.Text.Contains(Year + " SUMMARY INFORMATION FOR PARCEL " + parcelNumber.Replace("-", "")))
                        {
                            Ok = "Allowed Second";

                        }



                    }
                    string AssessmentData = Year + "~" + Land + "~" + Building + "~" + Total + "~" + AssessedLand + "~" + AssessedBuilding + "~" + AssessedTotal + "~" + Homestead + "~" + OwnerOccupancyCredit;
                    gc.insert_date(orderNumber, parcelNumber, 124, AssessmentData, 1, DateTime.Now);
                    //driver.FindElement(By.LinkText("Tax Info")).SendKeys(Keys.Enter);

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.SwitchTo().DefaultContent();
                    IWebElement iframeElement1 = driver.FindElement(By.XPath("/html/frameset/frame[1]"));
                    driver.SwitchTo().Frame(iframeElement1);
                    IWebElement ISpan1 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[6]/td/a"));
                    IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                    js1.ExecuteScript("arguments[0].click();", ISpan1);
                    Thread.Sleep(2000);
                    driver.SwitchTo().DefaultContent();
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Information", driver, "OH", "Summit");
                    IWebElement iframeElement2 = driver.FindElement(By.XPath("/html/frameset/frame[2]"));
                    driver.SwitchTo().Frame(iframeElement2);

                    string DeliqTax = "", Adjustment = "", DecemberIntrest = "", AugustInterest = "", TaxTotal = "", RealEstate = "", SpecialAssessment = "", TotalPaid = "", PaidAmount = "", TotalDue = "", M03StreetLightSweeping = "", DeliqTax2nd = "", Adjustment2nd = "", DecemberIntrest2nd = "", AugustInterest2nd = "", TaxTotal2nd = "", RealEstate2nd = "", SpecialAssessment2nd = "", TotalPaid2nd = "", PaidAmount2nd = "", TotalDue2nd = "", M03StreetLightSweeping2nd = "", Adjustment_NonDeliq = "", Adjustment_NonDeliq2nd = "", TotalNondeliq = "", TotalNondeliq2nd, Paymentdate = "", Paymentdate2nd = "";

                    DeliqTax = driver.FindElement(By.XPath("/html/body/table/tbody/tr[34]/td[2]/font")).Text;
                    DeliqTax2nd = "_";
                    Adjustment = driver.FindElement(By.XPath("/html/body/table/tbody/tr[35]/td[2]/font")).Text;
                    Adjustment2nd = "_";
                    DecemberIntrest = driver.FindElement(By.XPath("/html/body/table/tbody/tr[36]/td[2]/font")).Text;
                    DecemberIntrest2nd = "_";
                    AugustInterest = driver.FindElement(By.XPath("/html/body/table/tbody/tr[37]/td[2]/font")).Text;
                    AugustInterest2nd = "_";
                    TaxTotal = driver.FindElement(By.XPath("/html/body/table/tbody/tr[38]/td[2]/font/b")).Text;
                    TaxTotal2nd = "_";
                    RealEstate = driver.FindElement(By.XPath("/html/body/table/tbody/tr[40]/td[2]/font")).Text;
                    RealEstate2nd = driver.FindElement(By.XPath("/html/body/table/tbody/tr[40]/td[3]/font")).Text;
                    SpecialAssessment = driver.FindElement(By.XPath("/html/body/table/tbody/tr[40]/td[2]/font")).Text;
                    SpecialAssessment2nd = driver.FindElement(By.XPath("/html/body/table/tbody/tr[41]/td[3]/font")).Text;
                    Adjustment_NonDeliq = driver.FindElement(By.XPath("/html/body/table/tbody/tr[42]/td[2]/font")).Text;
                    Adjustment_NonDeliq2nd = driver.FindElement(By.XPath("/html/body/table/tbody/tr[42]/td[3]/font")).Text;
                    try
                    {
                        TotalPaid = driver.FindElement(By.XPath("/html/body/table/tbody/tr[49]/td[2]/font")).Text;
                        TotalPaid2nd = driver.FindElement(By.XPath("/html/body/table/tbody/tr[49]/td[3]/font/b")).Text;
                        TotalNondeliq = driver.FindElement(By.XPath("/html/body/table/tbody/tr[43]/td[2]/font/b")).Text;
                        TotalNondeliq2nd = driver.FindElement(By.XPath("/html/body/table/tbody/tr[43]/td[3]/font/b")).Text;

                        Paymentdate = driver.FindElement(By.XPath("/html/body/table/tbody/tr[46]/td[1]/font")).Text + "&" + driver.FindElement(By.XPath("/html/body/table/tbody/tr[47]/td[1]/font")).Text;
                        Paymentdate2nd = "_";
                        PaidAmount = driver.FindElement(By.XPath("/html/body/table/tbody/tr[46]/td[3]/font")).Text + "&" + driver.FindElement(By.XPath("/html/body/table/tbody/tr[47]/td[3]/font")).Text;
                        PaidAmount2nd = "_";
                        M03StreetLightSweeping = driver.FindElement(By.XPath("/html/body/table/tbody/tr[55]/td[5]/font")).Text;
                        M03StreetLightSweeping2nd = driver.FindElement(By.XPath("/html/body/table/tbody/tr[55]/td[6]/font")).Text;


                    }
                    catch
                    {


                    }
                    try
                    {
                        TotalPaid = driver.FindElement(By.XPath("/html/body/table/tbody/tr[48]/td[2]/font")).Text;
                        TotalPaid2nd = driver.FindElement(By.XPath("/html/body/table/tbody/tr[48]/td[3]/font/b")).Text;
                        TotalNondeliq = driver.FindElement(By.XPath("/html/body/table/tbody/tr[43]/td[2]/font/b")).Text;
                        TotalNondeliq2nd = driver.FindElement(By.XPath("/html/body/table/tbody/tr[43]/td[3]/font/b")).Text;

                        Paymentdate = driver.FindElement(By.XPath("/html/body/table/tbody/tr[46]/td[1]/font")).Text;
                        Paymentdate2nd = "_";
                        PaidAmount = driver.FindElement(By.XPath("/html/body/table/tbody/tr[46]/td[3]/font")).Text;
                        PaidAmount2nd = "_";
                        M03StreetLightSweeping = driver.FindElement(By.XPath("/html/body/table/tbody/tr[54]/td[5]/font")).Text;
                        M03StreetLightSweeping2nd = driver.FindElement(By.XPath("/html/body/table/tbody/tr[54]/td[6]/font")).Text;
                    }
                    catch
                    {



                    }
                    try
                    {

                        M03StreetLightSweeping = "";
                        M03StreetLightSweeping2nd = "";

                    }
                    catch
                    { }
                    IWebElement TaxInfotable = driver.FindElement(By.XPath("/html/body/table/tbody"));
                    IList<IWebElement> TaxInfotRow = TaxInfotable.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDTax;

                    foreach (IWebElement row1 in TaxInfotRow)
                    {

                        TDTax = row1.FindElements(By.TagName("td"));
                        if (TDTax[0].Text == "FH/SH AMOUNT DUE:")
                        {

                            TotalDue = TDTax[1].Text;
                            TotalDue2nd = TDTax[2].Text;
                            break;
                        }


                    }



                    address = PropertyAddress;

                    string TaxInformation1 = owner + "~" + address + "~" + Year + "~" + "1st Half" + "~" + DeliqTax + "~" + Adjustment + "~" + DecemberIntrest + "~" + AugustInterest + "~" + TaxTotal + "~" + RealEstate + "~" + SpecialAssessment + "~" + Adjustment_NonDeliq + "~" + TotalNondeliq + "~" + Paymentdate.Replace("\r\n", "") + "~" + PaidAmount.Replace("\r\n", "") + "~" + TotalPaid + "~" + TotalDue + "~" + M03StreetLightSweeping;
                    string TaxInformation2 = owner + "~" + address + "~" + Year + "~" + "2nd Half" + "~" + DeliqTax2nd + "~" + Adjustment2nd + "~" + DecemberIntrest2nd + "~" + AugustInterest2nd + "~" + TaxTotal2nd + "~" + RealEstate2nd + "~" + SpecialAssessment2nd + "~" + Adjustment_NonDeliq2nd + "~" + TotalNondeliq + "~" + Paymentdate2nd.Replace("\r\n", "") + "~" + PaidAmount2nd.Replace("\r\n", "") + "~" + TotalPaid2nd + "~" + TotalDue2nd + "~" + M03StreetLightSweeping2nd;
                    gc.insert_date(orderNumber, parcelNumber, 130, TaxInformation1, 1, DateTime.Now);
                    gc.insert_date(orderNumber, parcelNumber, 130, TaxInformation2, 1, DateTime.Now);



                    var split1 = TaxInformation1.Split('~');
                    var split2 = TaxInformation2.Split('~');
                    string TaxDisDate = "", Settle = "", ProjNumber = "", ActionorCode = "", FisrtHalf = "", Secondhalf = "", Allowed = "";
                    foreach (IWebElement row1 in TaxInfotRow)
                    {
                        int I = 0;
                        TDTax = row1.FindElements(By.TagName("td"));
                        //if (TDTax[0].Text == "2017 TAX BILL DETAILS FOR PARCEL " + parcelNumber.Replace("-", "") || Allowed == "OK")
                        //{
                        //    if (TDTax[0].Text.Contains("TAX BILL DETAILS FOR PARCEL "))

                        //    {

                        //    }
                        //        Allowed = "OK";
                        //    if (TDTax[0].Text.Trim() != "DATE" && TDTax[0].Text != "2017 TAX BILL DETAILS FOR PARCEL " + parcelNumber.Replace("-", ""))
                        //    {
                        if (TDTax[0].Text.Contains("TAX BILL DETAILS FOR PARCEL ") || Allowed == "OK")

                        {
                            Allowed = "OK";
                            if (TDTax[0].Text.Trim() != "DATE" && !row1.Text.Contains("TAX BILL DETAILS FOR PARCEL"))
                            {
                                TaxDisDate = TDTax[0].Text;
                                Settle = TDTax[1].Text;
                                ProjNumber = TDTax[2].Text;
                                ActionorCode = TDTax[3].Text;
                                FisrtHalf = TDTax[4].Text;
                                Secondhalf = TDTax[5].Text;
                                string[] stringSeparators = new string[] { "\r\n" };

                                string[] lines = TaxDisDate.Split(stringSeparators, StringSplitOptions.None);

                                string[] lines2 = Settle.Split(stringSeparators, StringSplitOptions.None);
                                string[] lines3 = ProjNumber.Split(stringSeparators, StringSplitOptions.None);
                                string[] lines4 = ActionorCode.Split(stringSeparators, StringSplitOptions.None);
                                string[] lines5 = FisrtHalf.Split(stringSeparators, StringSplitOptions.None);
                                string[] lines6 = Secondhalf.Split(stringSeparators, StringSplitOptions.None);

                                foreach (var item in lines)
                                {

                                    TaxDisDate = lines[I];
                                    try
                                    { Settle = lines2[I]; }
                                    catch { Settle = ""; }
                                    try
                                    { ProjNumber = lines3[I]; }
                                    catch
                                    { ProjNumber = ""; }

                                    ActionorCode = lines4[I];
                                    FisrtHalf = lines5[I];
                                    Secondhalf = lines6[I];
                                    string TaxDistributiondata = TaxDisDate + "~" + Settle + "~" + ProjNumber + "~" + ActionorCode + "~" + FisrtHalf + "~" + Secondhalf;
                                    gc.insert_date(orderNumber, parcelNumber, 132, TaxDistributiondata, 1, DateTime.Now);
                                    I++;

                                }

                                Allowed = "";




                            }

                        }
                        if (TDTax[0].Text == "DELQ REAL ESTATE & ASSESSMENT TAX:")
                        {

                            break;
                        }



                    }
                    string LUC = "", Class = "", OwnerOccupancycretit = "", FlagHomestead = "", CAUV = "", Forest = "", stub = "", Certyear = "", DeloqContract = "", Bankruptcy = "";

                    LUC = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td[3]/font")).Text;
                    Class = driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[2]/font")).Text;
                    OwnerOccupancycretit = driver.FindElement(By.XPath("/html/body/table/tbody/tr[5]/td[2]/font")).Text;
                    FlagHomestead = driver.FindElement(By.XPath("/html/body/table/tbody/tr[6]/td[2]/font")).Text;
                    CAUV = driver.FindElement(By.XPath("/html/body/table/tbody/tr[7]/td[5]/font")).Text;
                    Forest = driver.FindElement(By.XPath("/html/body/table/tbody/tr[8]/td[5]/font")).Text;
                    stub = driver.FindElement(By.XPath("/html/body/table/tbody/tr[9]/td[5]/font")).Text;
                    Certyear = driver.FindElement(By.XPath("/html/body/table/tbody/tr[10]/td[5]/font")).Text;
                    DeloqContract = driver.FindElement(By.XPath("/html/body/table/tbody/tr[11]/td[5]/font")).Text;
                    Bankruptcy = driver.FindElement(By.XPath("/html/body/table/tbody/tr[12]/td[5]/font")).Text;

                    string FlagDetail = LUC + "~" + Class + "~" + OwnerOccupancycretit + "~" + FlagHomestead + "~" + CAUV + "~" + Forest + "~" + stub + "~" + Certyear + "~" + DeloqContract + "~" + Bankruptcy;
                    gc.insert_date(orderNumber, parcelNumber, 133, FlagDetail, 1, DateTime.Now);
                    driver.SwitchTo().DefaultContent();

                    driver.SwitchTo().Frame(iframeElement1);
                    try
                    {
                        ISpan1 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[15]/td/a"));
                    }
                    catch { }
                    try
                    {
                        if (ISpan1.Text.Trim() == "")
                        {
                            ISpan1 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[16]/td/a"));
                        }
                    }
                    catch { }
                    js1.ExecuteScript("arguments[0].click();", ISpan1);
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax History", driver, "OH", "Summit");
                    string InstallmetTaxhis = "", Taxyear = "", taxAmount = "", Payment = "", TaxDue = "", taxAmount2nd = "", Payment2nd = "", TaxDue2nd = "";
                    driver.SwitchTo().DefaultContent();
                    driver.SwitchTo().Frame(iframeElement2);

                    IWebElement TaxHistoryBody = driver.FindElement(By.XPath("/html/body"));
                    IList<IWebElement> TaxHistoryTable = TaxHistoryBody.FindElements(By.TagName("table"));
                    IList<IWebElement> TDtaxhistory;
                    int j = 3;
                    foreach (IWebElement row1 in TaxHistoryTable)
                    {
                        try
                        {
                            Taxyear = driver.FindElement(By.XPath("/html/body/table[" + j + "]/tbody/tr[2]/td[1]")).Text;
                            taxAmount = driver.FindElement(By.XPath("/html/body/table[" + j + "]/tbody/tr[3]/td[2]")).Text;

                            taxAmount2nd = driver.FindElement(By.XPath("/html/body/table[" + j + "]/tbody/tr[3]/td[3]")).Text;
                            Payment = driver.FindElement(By.XPath("/html/body/table[" + j + "]/tbody/tr[4]/td[2]")).Text;
                            Payment2nd = driver.FindElement(By.XPath("/html/body/table[" + j + "]/tbody/tr[4]/td[3]")).Text;
                            TaxDue = driver.FindElement(By.XPath("/html/body/table[" + j + "]/tbody/tr[5]/td[2]/font")).Text;
                            TaxDue2nd = driver.FindElement(By.XPath("/html/body/table[" + j + "]/tbody/tr[5]/td[3]/font")).Text;

                            string TaxHistoryData1 = "1St Half" + "~" + Taxyear + "~" + taxAmount + "~" + Payment + "~" + TaxDue;
                            string TaxHistoryData2 = "2nd Half" + "~" + Taxyear + "~" + taxAmount2nd + "~" + Payment2nd + "~" + TaxDue2nd;
                            gc.insert_date(orderNumber, parcelNumber, 134, TaxHistoryData1, 1, DateTime.Now);
                            gc.insert_date(orderNumber, parcelNumber, 134, TaxHistoryData2, 1, DateTime.Now);
                            j++;
                        }
                        catch
                        {
                            break;

                        }



                    }
                    string TaxpayPaiddate = "", Taxpaytaxamount = "", Type = "", Bseq = "";
                    driver.SwitchTo().DefaultContent();

                    driver.SwitchTo().Frame(iframeElement1);
                    try
                    {
                        ISpan1 = driver.FindElement(By.LinkText("PAYMENTS"));
                    }
                    catch { }
                    try
                    {
                        if (ISpan1.Text == "" && ISpan1.Text.Contains("Payments"))
                        {
                            ISpan1 = driver.FindElement(By.LinkText("Payments"));
                        }
                    }
                    catch { }
                    //string CHK = driver.FindElement(By.XPath("/html/body/table/tbody/tr[16]/td/a")).Text;
                    //string CHK1 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[17]/td/a")).Text;
                    //try
                    //{
                    //    if (CHK.Contains("Payments"))
                    //    { ISpan1 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[16]/td/a")); }

                    //}
                    //catch { }
                    //try
                    //{

                    //        if (CHK1.Contains("PAYMENTS"))
                    //        {
                    //            ISpan1 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[17]/td/a"));
                    //        }


                    //}
                    //catch { }
                    js1.ExecuteScript("arguments[0].click();", ISpan1);
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Payment Deatil", driver, "OH", "Summit");
                    driver.SwitchTo().DefaultContent();
                    driver.SwitchTo().Frame(iframeElement2);

                    IWebElement TaxPayMenttable = driver.FindElement(By.XPath("/html/body/table[3]/tbody"));
                    IList<IWebElement> TaxpaymentTR = TaxPayMenttable.FindElements(By.TagName("tr"));
                    IList<IWebElement> TaxPaymentTD;

                    foreach (IWebElement row1 in TaxpaymentTR)
                    {
                        TaxPaymentTD = row1.FindElements(By.TagName("td"));
                        if (TaxPaymentTD.Count != 0)
                        {
                            TaxpayPaiddate = TaxPaymentTD[0].Text;
                            Taxpaytaxamount = TaxPaymentTD[1].Text;
                            Type = TaxPaymentTD[2].Text;
                            Bseq = TaxPaymentTD[3].Text;
                            string TaxPayment = TaxpayPaiddate + "~" + Taxpaytaxamount + "~" + Type + "~" + Bseq;

                            gc.insert_date(orderNumber, parcelNumber, 136, TaxPayment, 1, DateTime.Now);

                        }

                    }
                    driver.SwitchTo().DefaultContent();

                    driver.SwitchTo().Frame(iframeElement1);



                    String Parent_Window = driver.CurrentWindowHandle;

                    IWebElement view = driver.FindElement(By.XPath("/html/body/table/tbody/tr[7]/td/a"));



                    string url = view.GetAttribute("href");
                    //string billpdf = outputPath + "Tax_bill.pdf";
                    //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    //WebClient downloadpdf = new WebClient();
                    //downloadpdf.DownloadFile(url, billpdf);
                    gc.downloadfile(url, orderNumber, parcelNumber, "Tax_bill.pdf", "OH", "Summit");


                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");

                    gc.insert_TakenTime(orderNumber, "OH", "Summit", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);


                    driver.Quit();
                    gc.mergpdf(orderNumber, "OH", "Summit");
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