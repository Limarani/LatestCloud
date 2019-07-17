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
using OpenQA.Selenium.Edge;

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_CAEldorado
    {
        string parcelno = "";
        string outputPath = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();

        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_Eldarado(string Address, string account, string parcelNumber, string ownername, string searchType, string orderno, string directParcel)
        {
            GlobalClass.global_orderNo = orderno;
            HttpContext.Current.Session["orderNo"] = orderno;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                int multicount = 0;
                string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
                string multiparcel = "", MapTaxLot = "", SitusAddress = "", LegalDescription = "";
                IWebElement PropertyValidation;
                string[] stringSeparators1 = new string[] { "\r\n" };
                List<string> listurl = new List<string>();

                List<string> Columnurl = new List<string>();
                string Date = "";
                Date = DateTime.Now.ToString("M/d/yyyy");
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    //if (searchType == "titleflex")
                    //{
                    //    gc.TitleFlexSearch(orderno, parcelNumber, ownername, Address, "CA", "El Dorado");

                    //    parcelNumber = GlobalClass.global_parcelNo;
                    //    if (GlobalClass.TitleFlex_Search == "Yes")
                    //    {
                    //        driver.Quit();
                    //        return "MultiParcel";
                    //    }
                    //    searchType = "parcel";
                    //}
                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("https://common3.mptsweb.com/MBC/eldorado/tax/search");
                        IWebElement PropertyInformation = driver.FindElement(By.Id("SearchVal"));
                        SelectElement PropertyInformationSelect = new SelectElement(PropertyInformation);
                        PropertyInformationSelect.SelectByValue("situs");
                        driver.FindElement(By.Id("SearchValue")).SendKeys(Address);
                        driver.FindElement(By.Id("SearchSubmit")).Click();
                        Thread.Sleep(2000);
                        IWebElement Addresstax = driver.FindElement(By.XPath("//*[@id='ResultDiv']/div"));
                        string Firststep = ""; int Max = 0;
                        try
                        {
                            IWebElement PropertyIteamTable = driver.FindElement(By.XPath("//*[@id='ResultDiv']/div/div"));
                            IList<IWebElement> PropertyIeamRow = PropertyIteamTable.FindElements(By.TagName("p"));

                            foreach (IWebElement Property in PropertyIeamRow)
                            {
                                if (PropertyIeamRow.Count != 0 & !Property.Text.Contains("View Details"))
                                {
                                    string Parcelnumber = GlobalClass.After(Property.Text, "Fee Parcel :").Trim();
                                    string[] splitparcel = Parcelnumber.Split('-');
                                    string split3 = splitparcel[2].Substring(1, 2);
                                    parcelNumber = splitparcel[0] + splitparcel[1] + split3;
                                    string Addressta = gc.Between(Property.Text, "Address :", "Year :");
                                    string Yearpar = gc.Between(Property.Text, "Year :", "TRA :");
                                    string roallcast = gc.Between(Property.Text, "Roll Cat. :", "Fee Parcel :");
                                    string Multiresult = "~" + Addressta + "~" + Yearpar + "~" + roallcast;
                                    gc.insert_date(orderno, Parcelnumber, 360, Multiresult, 1, DateTime.Now);
                                    Max++;
                                }
                            }
                            if (Max == 1)
                            {
                                searchType = "parcel";
                            }
                            if (Max > 1 & Max < 26)
                            {
                                HttpContext.Current.Session["multiParcel_CAEldorado"] = "Yes";
                                gc.CreatePdf_WOP(orderno, "MultyAddressSearch", driver, "CA", "El Dorado");
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Max > 25)
                            {
                                HttpContext.Current.Session["multiParcel_CAEldorado_Count"] = "Maximum";
                                gc.CreatePdf_WOP(orderno, "MultyAddressSearch", driver, "CA", "El Dorado");
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }
                        try
                        {
                            IWebElement INodata = driver.FindElement(By.Id("ResultDiv"));
                            if (INodata.Text.Contains("no matching records were found"))
                            {
                                HttpContext.Current.Session["Nodata_CAEldorado"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {

                        driver.Navigate().GoToUrl("http://main.edcgov.us/CGI/WWB012/WWM400/A");

                        Thread.Sleep(3000);
                        var SerachCategory = driver.FindElement(By.XPath("//*[@id='T']"));
                        var selectElement1 = new SelectElement(SerachCategory);
                        selectElement1.SelectByText("Secured Parcel Number (999-999-99)");
                        Thread.Sleep(3000);

                        driver.FindElement(By.XPath("//*[@id='K']")).SendKeys(parcelNumber.Trim());
                        gc.CreatePdf(orderno, parcelNumber, "Account Number Search", driver, "CA", "El Dorado");
                        Thread.Sleep(1000);


                        driver.FindElement(By.XPath("/html/body/form/table/tbody/tr/td/span/input")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderno, parcelNumber, "Parcel Search", driver, "CA", "El Dorado");


                    }
                    else if (searchType == "ownername")
                    {

                        driver.Navigate().GoToUrl("http://main.edcgov.us/CGI/WWB012/WWM400/A");

                        Thread.Sleep(3000);
                        var SerachCategory = driver.FindElement(By.XPath("//*[@id='T']"));
                        var selectElement1 = new SelectElement(SerachCategory);
                        selectElement1.SelectByText("Owner's Name (Last First) or (Company)");
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderno, "ownerNameSearch", driver, "CA", "El Dorado");
                        driver.FindElement(By.XPath("//*[@id='K']")).SendKeys(ownername.Trim());
                        Thread.Sleep(1000);
                        try
                        {
                            IWebElement ImultiCount = driver.FindElement(By.XPath("//*[@id='Q']"));
                            SelectElement selectCount = new SelectElement(ImultiCount);
                            selectCount.SelectByText("50");
                        }
                        catch { }
                        driver.FindElement(By.XPath("/html/body/form/table/tbody/tr/td/span/input")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        try
                        {
                            string strmultiCount = driver.FindElement(By.XPath("/html/body/p[2]")).Text;
                            string strMcount = gc.Between(strmultiCount, "Record Count = ", " of");
                            if (Convert.ToInt32(strMcount) > 25)
                            {
                                HttpContext.Current.Session["multiParcel_CAEldorado_Count"] = "Maximum";
                                return "Maximum";
                            }

                        }
                        catch { }
                        gc.CreatePdf_WOP(orderno, "ownerNameSearchResult", driver, "CA", "El Dorado");
                        IWebElement MuliparcTB = driver.FindElement(By.XPath("//*[@id='idWWM983']/tbody"));
                        IList<IWebElement> MuliparcTR = MuliparcTB.FindElements(By.TagName("tr"));
                        if (MuliparcTR.Count() > 2)
                        {
                            IList<IWebElement> MuliparcTD;

                            foreach (IWebElement row1 in MuliparcTR)
                            {
                                MuliparcTD = row1.FindElements(By.TagName("td"));
                                if (MuliparcTD.Count != 0 && multicount < 25)
                                {
                                    parcelNumber = MuliparcTD[1].Text;
                                    ownername = MuliparcTD[2].Text;

                                    string Multipar = ownername + "~" + "~" + "~";
                                    gc.insert_date(orderno, parcelNumber, 360, Multipar, 1, DateTime.Now);
                                    HttpContext.Current.Session["multiParcel_CAEldorado"] = "Yes";
                                    multicount++;
                                }

                            }
                            driver.Quit();
                            return "MultiParcel";
                        }



                        //gc.CreatePdf(orderno, parcelNumber, "Account Number Search Result", driver, "OR", "Marion");

                    }
                    else if (searchType == "block")
                    {


                        driver.Navigate().GoToUrl("http://main.edcgov.us/CGI/WWB012/WWM400/A");

                        Thread.Sleep(3000);
                        var SerachCategory = driver.FindElement(By.XPath("//*[@id='T']"));
                        var selectElement1 = new SelectElement(SerachCategory);
                        selectElement1.SelectByText("Owner's Name (Last First) or (Company)");
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderno, "Account Number Search", driver, "CA", "El Dorado");
                        driver.FindElement(By.XPath("//*[@id='K']")).SendKeys(ownername.Trim());
                        Thread.Sleep(1000);
                        try
                        {
                            IWebElement ImultiCount = driver.FindElement(By.XPath("//*[@id='Q']"));
                            SelectElement selectCount = new SelectElement(ImultiCount);
                            selectCount.SelectByText("50");
                        }
                        catch { }
                        driver.FindElement(By.XPath("/html/body/form/table/tbody/tr/td/span/input")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        try
                        {
                            string strmultiCount = driver.FindElement(By.XPath("/html/body/p[2]")).Text;
                            string strMcount = gc.Between(strmultiCount, "Record Count = ", " of");
                            if (Convert.ToInt32(strMcount) > 25)
                            {
                                HttpContext.Current.Session["multiParcel_CAEldorado_Count"] = "Maximum";
                                return "Maximum";
                            }

                        }
                        catch { }
                        gc.CreatePdf_WOP(orderno, "Account Number Search Result", driver, "CA", "El Dorado");
                        IWebElement MuliparcTB = driver.FindElement(By.XPath("/[@id='idWWM983']/tbody"));
                        IList<IWebElement> MuliparcTR = MuliparcTB.FindElements(By.TagName("tr"));
                        if (MuliparcTR.Count() > 2)
                        {
                            IList<IWebElement> MuliparcTD;

                            foreach (IWebElement row1 in MuliparcTR)
                            {
                                MuliparcTD = row1.FindElements(By.TagName("td"));
                                if (MuliparcTD.Count != 0 && multicount < 25)
                                {
                                    parcelNumber = MuliparcTD[1].Text;
                                    ownername = MuliparcTD[2].Text;

                                    string Multipar = ownername;
                                    gc.insert_date(orderno, parcelNumber, 360, Multipar, 1, DateTime.Now);

                                    HttpContext.Current.Session["multiParcel_CAEldorado"] = "Yes";
                                    multicount++;
                                }

                            }
                            driver.Quit();
                            return "MultiParcel";
                        }


                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.XPath("/html/body/p[2]"));
                        if(INodata.Text.Contains("No Records Found"))
                        {
                            HttpContext.Current.Session["Nodata_CAEldorado"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    // assessment details
                    string YearBuilt = "", Abstractcode = "", Reference = "", SubdivisionTractNumber = "", SubdivisionTractName = "", TaxRateArea = "", City = "", BulData = "";
                    Reference = driver.FindElement(By.XPath("//*[@id='idWWM983']/tbody/tr[2]/td[3]/span")).Text.Trim();
                    string chkpa1 = "";
                    string chkpa = driver.FindElement(By.XPath("//*[@id='idWWM983']/tbody/tr[2]/td[2]/a")).Text;
                    try
                    {
                        chkpa1 = driver.FindElement(By.XPath("//*[@id='idWWM983']/tbody/tr[3]/td[2]/a")).Text;
                    }
                    catch { }
                    driver.FindElement(By.XPath("//*[@id='idWWM983']/tbody/tr[2]/td[2]/a")).SendKeys(Keys.Enter);
                    Thread.Sleep(1000);
                    gc.CreatePdf(orderno, parcelNumber, "Assessment Details", driver, "CA", "El Dorado");
                    parcelNumber = driver.FindElement(By.XPath("//*[@id='Top']")).Text.Replace("Parcel Number ", "").Trim();
                    IWebElement YearB = driver.FindElement(By.XPath("/html/body/table[8]/tbody"));
                    IList<IWebElement> YearTR = YearB.FindElements(By.TagName("tr"));
                    IList<IWebElement> YearTD;

                    foreach (IWebElement row1 in YearTR)
                    {
                        YearTD = row1.FindElements(By.TagName("td"));
                        if (row1.Text.Contains("Year Built"))
                        {
                            if (YearTD.Count != 0)
                            {
                                YearBuilt = YearTD[1].Text;
                            }
                        }

                    }
                    try
                    {
                        YearB = driver.FindElement(By.XPath("/html/body/table[9]/tbody"));
                        YearTR = YearB.FindElements(By.TagName("tr"));


                        foreach (IWebElement row1 in YearTR)
                        {
                            YearTD = row1.FindElements(By.TagName("td"));
                            if (row1.Text.Contains("Year Built"))
                            {
                                if (YearTD.Count != 0)
                                {
                                    YearBuilt = YearTD[1].Text;
                                }
                            }

                        }
                    }
                    catch { }
                    string OW1 = "", Ow2 = "", Ow3 = "";
                    OW1 = driver.FindElement(By.XPath("/html/body")).Text;

                    ownername = gc.Between(OW1, "Current Property Owners\r\n", "\r\nProperty Description Values Event List Characteristics Background Top of Page").Trim();

                    ownername = ownername.Replace("\r\n", "");




                    BulData = driver.FindElement(By.XPath("//*[@id='PROPDESC']")).Text;

                    Reference = gc.Between(BulData, "Reference:", "For Zoning,").Trim();

                    try { Abstractcode = gc.Between(BulData, "Abstract code:", "Reference:").Trim(); }
                    catch { }
                    if (Abstractcode.Contains("G.I.S. Map"))
                    {
                        Abstractcode = "";
                    }
                    //Reference = WebDriverTest.After(BulData, "Reference:").Trim();
                    //var FRefSplit = Reference.Split('(');
                    //Reference = FRefSplit[0];

                    try
                    {
                        SubdivisionTractNumber = gc.Between(BulData, "Subdivision Tract Number:", "Subdivision Tract Name:").Trim();
                        SubdivisionTractName = gc.Between(BulData, "Subdivision Tract Name: ", "Subdivision map").Trim();
                    }
                    catch { }

                    if (SubdivisionTractName == "")
                    {

                        try
                        {

                            SubdivisionTractName = gc.Between(BulData, "Subdivision Tract Name: ", "Timeshare Interval Id:").Trim();
                        }
                        catch { }

                    }

                    TaxRateArea = WebDriverTest.After(BulData, "Tax Rate Area:").Trim();
                    var Citysplit = TaxRateArea.Split(' ');
                    TaxRateArea = Citysplit[0];
                    City = WebDriverTest.After(BulData, Citysplit[0]).Trim();

                    string ProperTyDetail = ownername + "~" + Abstractcode + "~" + Reference + "~" + SubdivisionTractNumber + "~" + SubdivisionTractName + "~" + TaxRateArea + "~" + City + "~" + YearBuilt;
                    gc.insert_date(orderno, parcelNumber, 260, ProperTyDetail, 1, DateTime.Now);
                    string Column = "";

                    string TaxablePropertyValues20172018 = "", Land = "", LandTotal = "", ImprovementStructures = "", ImprovementTotal = "", TotalRoll = "", NetRoll = "";
                    string LandProp8 = "", ImporevePro8 = "", Exemption = "";
                    TaxablePropertyValues20172018 = driver.FindElement(By.XPath("/html/body/table[4]/caption")).Text.Trim().Replace("\r\n", " ");
                    IWebElement MultiAssessTB = driver.FindElement(By.XPath("/html/body/table[4]/tbody"));
                    IList<IWebElement> MultiAssessTR = MultiAssessTB.FindElements(By.TagName("tr"));
                    IList<IWebElement> MultiAssessTD;

                    foreach (IWebElement row1 in MultiAssessTR)
                    {
                        MultiAssessTD = row1.FindElements(By.TagName("td"));
                        if (MultiAssessTD.Count != 0)
                        {

                            Column = MultiAssessTD[0].Text + "~" + Column;
                            Land = MultiAssessTD[1].Text + "~" + Land;



                        }

                    }
                    Column = Column + "dsds";
                    Land = Land + "dsds";
                    Column = Column.Replace("~dsds", "");
                    Land = Land.Replace("~dsds", "");
                    //if (listurl.Count == 4)
                    //{
                    //    Land = listurl[0];
                    //    LandTotal = listurl[1];
                    //    //   ImprovementStructures = listurl[2];
                    //    //  ImprovementTotal = listurl[3];
                    //    TotalRoll = listurl[2];
                    //    NetRoll = listurl[3];
                    //}
                    //else
                    //{
                    //    Land = listurl[0];
                    //    LandTotal = listurl[1];
                    //    ImprovementStructures = listurl[2];
                    //    ImprovementTotal = listurl[3];
                    //    TotalRoll = listurl[4];
                    //    NetRoll = listurl[5];
                    //}

                    DBconnection dbconn = new DBconnection();



                    dbconn.ExecuteQuery("update  data_field_master set Data_Fields_Text='" + Column + "' where Id = '" + 261 + "'");


                    string Assessment = Land;
                    gc.insert_date(orderno, parcelNumber, 261, Assessment, 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("https://taxcollector.edcgov.us/GetAPN.aspx");
                    Thread.Sleep(2000);
                    driver.FindElement(By.XPath("//*[@id='btnawoc']")).Click();
                    Thread.Sleep(2000);
                    driver.FindElement(By.XPath("//*[@id='form1']/table[2]/tbody/tr[2]/td[1]/a/img")).Click();
                    Thread.Sleep(2000);
                    driver.FindElement(By.XPath("//*[@id='txtAPN']")).SendKeys(parcelNumber);
                    gc.CreatePdf(orderno, parcelNumber, "Input Passed Tax Search", driver, "CA", "El Dorado");
                    Thread.Sleep(1000);



                    //Tax bill download....
                    IWebElement SelectOption = driver.FindElement(By.Id("ddList"));
                    IList<IWebElement> Select = SelectOption.FindElements(By.TagName("option"));
                    List<string> option = new List<string>();
                    int Check = 0;
                    foreach (IWebElement Op in Select)
                    {

                        if (Check <= 2)
                        {
                            option.Add(Op.Text);
                        }
                        Check++;
                    }

                    //load chrome driver...
                    //IWebDriver chDriver = new ChromeDriver();
                    var chromeOptions = new ChromeOptions();
                    var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];

                    chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                    chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                    chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");

                    var chDriver = new ChromeDriver(chromeOptions);
                    try
                    {

                        chDriver.Navigate().GoToUrl("https://taxcollector.edcgov.us/GetAPN.aspx");
                        chDriver.FindElement(By.XPath("//*[@id='btnawoc']")).Click();
                        chDriver.FindElement(By.XPath("//*[@id='form1']/table[2]/tbody/tr[2]/td[1]/a/img")).Click();
                        chDriver.FindElement(By.XPath("//*[@id='txtAPN']")).SendKeys(parcelNumber);

                        foreach (string item in option)
                        {
                            var SelectAddress = chDriver.FindElement(By.Id("ddList"));
                            var SelectAddressTax = new SelectElement(SelectAddress);
                            SelectAddressTax.SelectByText(item);
                            chDriver.FindElement(By.XPath("//*[@id='btnView']")).Click();
                            Thread.Sleep(9000);

                            string yr = GlobalClass.Before(item, "-");
                            string fileName = "EDC_TaxBill_Copy_for_Secured_APN_" + parcelNumber + "_Year_" + yr + ".pdf";
                            gc.AutoDownloadFile(orderno, parcelNumber, "El Dorado", "CA", fileName);
                        }
                        chDriver.Quit();
                    }

                    catch (Exception ex)
                    {
                        chDriver.Quit();
                        GlobalClass.LogError(ex, orderno);
                    }


                    driver.FindElement(By.XPath("//*[@id='btnSecSum']")).SendKeys(Keys.Enter);
                    Thread.Sleep(1000);


                    gc.CreatePdf(orderno, parcelNumber, "Tax History Detail", driver, "CA", "El Dorado");
                    string Description = "", TaxAuthority = "360 Fair Lane, Placerville, CA 95667 (530) 621-5800,Fax: (530) 642-8870", Grand_Total_Secured_Taxes_Due = "", Fisrt_Instalment = "", Due = "", Default_Due = "", Year = "", Second_Instalment = "", Default_Bill = "", Total = "", Total_Default_Due = "";


                    //*[@id="main"]/table[1]/tbody/tr[1]/td/table[4]/tbody

                    string taxsum = "";
                    try
                    {
                        IWebElement tbmulti = driver.FindElement(By.XPath("//*[@id='main']/table[1]/tbody/tr[1]/td/table[4]/tbody"));
                        IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDmulti;
                        foreach (IWebElement row in TRmulti)
                        {
                            if (!row.Text.Contains("Description"))
                            {
                                TDmulti = row.FindElements(By.TagName("td"));
                                if (TDmulti.Count == 8)
                                {
                                    taxsum = TDmulti[0].Text + "~" + TDmulti[1].Text + "~" + TDmulti[2].Text + "~" + TDmulti[3].Text + "~" + TDmulti[4].Text + "~" + TDmulti[5].Text + "~" + TDmulti[6].Text;
                                    gc.insert_date(orderno, parcelNumber, 265, taxsum, 1, DateTime.Now);

                                }
                                if (TDmulti.Count == 4)
                                {
                                    taxsum = "Totals" + "~" + "" + "~" + "" + "~" + TDmulti[1].Text + "~" + TDmulti[2].Text + "~" + "" + "~" + "";
                                    gc.insert_date(orderno, parcelNumber, 265, taxsum, 1, DateTime.Now);

                                }
                                if (TDmulti.Count == 3)
                                {
                                    taxsum = "Grand Total Secured Taxes Due" + "~" + "" + "~" + "" + "~" + TDmulti[1].Text + "~" + "" + "~" + "" + "~" + "";
                                    gc.insert_date(orderno, parcelNumber, 265, taxsum, 1, DateTime.Now);

                                }
                            }
                        }
                    }
                    catch { }

                    //IWebElement CurrentTaxHistoryTB1 = driver.FindElement(By.XPath("//*[@id='main']/table[1]/tbody/tr[1]/td/table[4]/tbody"));
                    //IList<IWebElement> CurrentTaxHistoryTR1 = CurrentTaxHistoryTB1.FindElements(By.TagName("tr"));
                    //IList<IWebElement> CurrentTaxHistoryTD1;
                    //int rowcount = CurrentTaxHistoryTR1.Count;
                    //int count = 0;
                    //foreach (IWebElement row1 in CurrentTaxHistoryTR1)
                    //{

                    //    CurrentTaxHistoryTD1 = row1.FindElements(By.TagName("td"));
                    //    if (CurrentTaxHistoryTD1.Count != 0 && row1.Text != "" && !row1.Text.Contains("Current Owner") && !row1.Text.Contains("Description"))
                    //    {
                    //        if (count < rowcount - 2)
                    //        {
                    //            //Description~Fisrt_Instalment~Second_Instalment~Due~Default_Due~Year~Default_Bill~Total~Total_Default_Due~Grand_Total_Secured_Taxes_Due
                    //            taxsum = CurrentTaxHistoryTD1[0].Text + "~" + CurrentTaxHistoryTD1[1].Text + "~" + CurrentTaxHistoryTD1[2].Text + "~" + CurrentTaxHistoryTD1[3].Text + "~" + CurrentTaxHistoryTD1[4].Text + "~" + CurrentTaxHistoryTD1[5].Text + "~" + CurrentTaxHistoryTD1[6].Text;
                    //            gc.insert_date(orderno, parcelNumber, 265, taxsum, 1, DateTime.Now);
                    //        }
                    //        if (count == rowcount - 2)
                    //        {
                    //            taxsum = "Totals" + "~" + "" + "~" + "" + "~" + CurrentTaxHistoryTD1[1].Text + "~" + CurrentTaxHistoryTD1[2].Text + "~" + "" + "~" + "";
                    //            gc.insert_date(orderno, parcelNumber, 265, taxsum, 1, DateTime.Now);
                    //        }
                    //        if (count == rowcount - 1)
                    //        {
                    //            taxsum = "Grand Total Secured Taxes Due" + "~" + "" + "~" + "" + "~" + CurrentTaxHistoryTD1[1].Text + "~" + "" + "~" + "" + "~" + "";
                    //            gc.insert_date(orderno, parcelNumber, 265, taxsum, 1, DateTime.Now);
                    //        }
                    //    }

                    //    count++;
                    //}


                    try
                    {
                        IWebElement CurrentTaxHistoryTB = driver.FindElement(By.XPath("//*[@id='main']/table[1]/tbody/tr[1]/td/table[4]/tbody"));
                        IList<IWebElement> CurrentTaxHistoryTR = CurrentTaxHistoryTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> CurrentTaxHistoryTD;
                        IList<IWebElement> CurrentTaxHistoryTA;
                        listurl.Clear();
                        foreach (IWebElement row1 in CurrentTaxHistoryTR)
                        {
                            CurrentTaxHistoryTD = row1.FindElements(By.TagName("td"));
                            if (CurrentTaxHistoryTD.Count != 0 && CurrentTaxHistoryTD.Count != 2 && CurrentTaxHistoryTD.Count != 1 && CurrentTaxHistoryTD[0].Text.Trim() != "Description")
                            {
                                foreach (IWebElement row in CurrentTaxHistoryTD)
                                {
                                    CurrentTaxHistoryTA = row.FindElements(By.TagName("a"));
                                    if (CurrentTaxHistoryTA.Count != 0)
                                    {
                                        listurl.Add(CurrentTaxHistoryTA[0].GetAttribute("href"));

                                    }

                                }
                                foreach (string URL in listurl)

                                {
                                    gc.downloadfile(URL, orderno, parcelNumber, "Tax_Bill" + Description, "CA", "El Dorado");

                                }

                            }
                        }
                    }
                    catch { }

                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='MainContent_menus']/table/tbody/tr[2]/td[2]/a/img")).Click();
                        Thread.Sleep(2000);
                    }
                    catch { }
                    try
                    {

                        gc.CreatePdf(orderno, parcelNumber, "Payment History Detail", driver, "CA", "El Dorado");
                        string PaymentHistoryforAPN = "", DatePaid = "", Installment = "", Amount = "";
                        PaymentHistoryforAPN = driver.FindElement(By.XPath("//*[@id='menus']/table/tbody/tr[2]/td[2]/h2[1]")).Text;
                        var Paysplit = PaymentHistoryforAPN.Split(':');
                        PaymentHistoryforAPN = Paysplit[1];
                        IWebElement CurrentPayHistoryTB = driver.FindElement(By.XPath("//*[@id='form1']/table/tbody/tr[3]/td/table/tbody"));
                        IList<IWebElement> CurrentPayHistoryTR = CurrentPayHistoryTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> CurrentPayHistoryTD;

                        foreach (IWebElement row1 in CurrentPayHistoryTR)
                        {
                            CurrentPayHistoryTD = row1.FindElements(By.TagName("td"));
                            if (CurrentPayHistoryTD.Count != 0 && CurrentPayHistoryTD.Count != 1 && CurrentPayHistoryTD[0].Text.Trim() != "Date Paid")
                            {
                                DatePaid = CurrentPayHistoryTD[0].Text;
                                Description = CurrentPayHistoryTD[1].Text;
                                Default_Bill = CurrentPayHistoryTD[2].Text;
                                Installment = CurrentPayHistoryTD[3].Text;
                                Year = CurrentPayHistoryTD[4].Text;
                                Amount = CurrentPayHistoryTD[5].Text;

                                string CurrenttaxHistory = PaymentHistoryforAPN + "~" + DatePaid + "~" + Description + "~" + Default_Bill + "~" + Installment + "~" + Year + "~" + Amount;
                                gc.insert_date(orderno, parcelNumber, 264, CurrenttaxHistory, 1, DateTime.Now);

                            }
                        }
                    }
                    catch { }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");

                    gc.insert_TakenTime(orderno, "CA", "El Dorado", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);


                    driver.Quit();
                    gc.mergpdf(orderno, "CA", "El Dorado");

                    //gc.MMREM_Template(orderno,parcelNumber,"",driver,"CA", "El Dorado","95","");
                    return "Data Inserted Successfully";
                }

                catch (Exception ex)
                {
                    driver.Quit();
                    GlobalClass.LogError(ex, orderno);
                    throw ex;
                }
            }
        }


    }
}