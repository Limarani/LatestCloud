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
using System.Collections.ObjectModel;

namespace ScrapMaricopa.Scrapsource
{
    public class Webdriver_LAEastBatonRouge
    {
        string outputPath = "", outparcelno = "";
        string chkMulti = "", Physicaladdrerss = "", Multidata = "", Ownername = "";
        string OwnerName = "-", Mailingaddress = "-", Propertyaddress = "-", Ward = "-", Property_Type = "-", Legal = "-", Subdivision = "-", Lot = "-", property_details = "";
        string TaxDistributionDetails = "-", Millage = "-", Mills = "-", Tax = "-", Homestead_Tax = "-";
        string Assement_details = "-", Property_class = "-", AssedValues = "-", Units = "-", Homestead = "-";
        string Date = "-", Description = "-", Amount = "-", TaxHistory = "-";
        string Taxes = "-", Interest = "-", Cost = "-", Other = "-", Paid = "-", Balance = "-", Notice = "-", Taxyear = "-", TaxPayer = "-", TaxInformation = "-";
        string Deliquent_Interest = "", Deliquent_Balance = "", DeliquentTaxInformation = "-", strlast = "", Legal_Description = "", strTax = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_LAEastBatonRouge(string houseno, string sname, string sttype, string parcelNumber, string searchType, string orderNumber, string ownername, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {

                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    if (searchType == "titleflex")
                    {
                        string address = houseno + " " + sname + " " + sttype;
                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "LA", "East Baton Rouge");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["EastbatonLA_NoRecord"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }


                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("http://www.ebrpa.org/PageDisplay.asp?p1=1503");
                        Thread.Sleep(2000);

                        IWebElement iframeElement = driver.FindElement(By.XPath("/html/body/center/div[2]/table/tbody/tr/td[2]/table/tbody/tr/td/table/tbody/tr[2]/td/font/div[1]/iframe"));
                        Thread.Sleep(2000);
                        driver.SwitchTo().Frame(iframeElement);

                        driver.FindElement(By.XPath("/html/body/div[2]/div/div[3]/div/div/form/input[3]")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.XPath("/html/body/div[2]/div/div[3]/div/div/form/div[2]/div/input[1]")).SendKeys(houseno);
                        driver.FindElement(By.XPath("/html/body/div[2]/div/div[3]/div/div/form/div[2]/div/input[2]")).SendKeys(sname);

                        driver.FindElement(By.XPath("/html/body/div[2]/div/div[3]/div/div/form/div[5]/button")).Click();
                        Thread.Sleep(2000);
                        //Screen-Shot
                        gc.CreatePdf_WOP(orderNumber, "AddressSearch", driver, "LA", "East Baton Rouge");


                        //MultiParcel
                        IWebElement MultiParcelTable = driver.FindElement(By.XPath("/html/body/div[2]/div/div[3]/div/div/table/tbody"));
                        IList<IWebElement> MultiParcelTR = MultiParcelTable.FindElements(By.TagName("tr"));

                        if (MultiParcelTR.Count == 1)
                        {
                            NavigateUrl(driver);
                        }
                        else
                        {
                            try
                            {
                                string no = driver.FindElement(By.XPath("//*[@id='ng-view']/div/div")).Text;
                                if (no.Contains("No results found"))
                                {
                                    HttpContext.Current.Session["EastbatonLA_NoRecord"] = "Yes";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch
                            {

                            }

                            IList<IWebElement> MultiParcelTD;
                            foreach (IWebElement multi in MultiParcelTR)
                            {
                                MultiParcelTD = multi.FindElements(By.TagName("td"));
                                if (MultiParcelTD.Count != 0)
                                {
                                    parcelNumber = MultiParcelTD[0].Text;
                                    Ownername = MultiParcelTD[1].Text;
                                    Physicaladdrerss = MultiParcelTD[2].Text;
                                    Multidata = Ownername + "~" + Physicaladdrerss;
                                    gc.insert_date(orderNumber, parcelNumber, 177, Multidata, 1, DateTime.Now);
                                }
                                HttpContext.Current.Session["multiParcel_LAEastBatonRouge"] = "Yes";
                            }

                            if (MultiParcelTR.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_LAEastBatonRouge_Multicount"] = "Maximum";
                            }
                            driver.Quit();
                            return "MultiParcel";
                        }
                    }

                    else if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("http://www.ebrpa.org/PageDisplay.asp?p1=1503");
                        Thread.Sleep(2000);

                        IWebElement iframeElement = driver.FindElement(By.XPath("/html/body/center/div[2]/table/tbody/tr/td[2]/table/tbody/tr/td/table/tbody/tr[2]/td/font/div[1]/iframe"));
                        driver.SwitchTo().Frame(iframeElement);

                        driver.FindElement(By.XPath("/html/body/div[2]/div/div[3]/div/div/form/input[1]")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.XPath("/html/body/div[2]/div/div[3]/div/div/form/input[7]")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearch", driver, "LA", "East Baton Rouge");
                        driver.FindElement(By.XPath("/html/body/div[2]/div/div[3]/div/div/form/div[5]/button")).Click();
                        Thread.Sleep(2000);

                        driver.FindElement(By.XPath("/html/body/div[2]/div/div[3]/div/div/table/tbody/tr/td[4]/a")).Click();
                        Thread.Sleep(3000);
                        NavigateUrl(driver);
                    }

                    else if (searchType == "ownername")
                    {
                        driver.Navigate().GoToUrl("http://www.ebrpa.org/PageDisplay.asp?p1=1503");
                        Thread.Sleep(2000);

                        IWebElement iframeElement = driver.FindElement(By.XPath("/html/body/center/div[2]/table/tbody/tr/td[2]/table/tbody/tr/td/table/tbody/tr[2]/td/font/div[1]/iframe"));
                        driver.SwitchTo().Frame(iframeElement);

                        driver.FindElement(By.XPath("/html/body/div[2]/div/div[3]/div/div/form/input[2]")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.XPath("/html/body/div[2]/div/div[3]/div/div/form/input[7]")).SendKeys(ownername);
                        gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearch", driver, "LA", "East Baton Rouge");
                        driver.FindElement(By.XPath("/html/body/div[2]/div/div[3]/div/div/form/div[5]/button")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, outparcelno, "Property_Search", driver, "LA", "East Baton Rouge");
                        //MultiParcel
                        IWebElement MultiParcelTable = driver.FindElement(By.XPath("/html/body/div[2]/div/div[3]/div/div/table/tbody"));
                        IList<IWebElement> MultiParcelTR = MultiParcelTable.FindElements(By.TagName("tr"));

                        if (MultiParcelTR.Count == 1)
                        {
                            NavigateUrl(driver);
                        }
                        else
                        {
                            IList<IWebElement> MultiParcelTD;
                            foreach (IWebElement multi in MultiParcelTR)
                            {
                                MultiParcelTD = multi.FindElements(By.TagName("td"));
                                if (MultiParcelTD.Count != 0)
                                {
                                    parcelNumber = MultiParcelTD[0].Text;
                                    Ownername = MultiParcelTD[1].Text;
                                    Physicaladdrerss = MultiParcelTD[2].Text;
                                    Multidata = Ownername + "~" + Physicaladdrerss;
                                    gc.insert_date(orderNumber, parcelNumber, 177, Multidata, 1, DateTime.Now);
                                }
                            }

                            HttpContext.Current.Session["multiParcel_LAEastBatonRouge"] = "Yes";
                            if (MultiParcelTR.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_LAEastBatonRouge_Multicount"] = "Maximum";
                            }
                            driver.Quit();
                            return "MultiParcel";
                        }

                    }

                    //Scrapped Data 

                    //Property Deatails              
                    outparcelno = driver.FindElement(By.XPath("/html/body/div[3]/div/div[1]/span[2]")).Text;
                    OwnerName = driver.FindElement(By.XPath("/html/body/div[3]/div/div[2]/div/span[2]")).Text;
                    if (OwnerName.Contains("\r\n"))
                    {
                        OwnerName = OwnerName.Replace("\r\n", ",");
                    }
                    Mailingaddress = driver.FindElement(By.XPath("/html/body/div[3]/div/div[3]/div/span")).Text;
                    if (Mailingaddress.Contains("\r\n"))
                    {
                        Mailingaddress = Mailingaddress.Replace("\r\n", ",");
                    }
                    Property_Type = driver.FindElement(By.XPath("/html/body/div[3]/div/div[5]/div/span")).Text;
                    Propertyaddress = driver.FindElement(By.XPath("/html/body/div[3]/div/div[7]/div/span[2]")).Text;
                    if (Propertyaddress.Contains("\r\n"))
                    {
                        Propertyaddress = Propertyaddress.Replace("\r\n", ",");
                    }
                    gc.CreatePdf(orderNumber, outparcelno, "Assement", driver, "LA", "East Baton Rouge");

                    //Assessment Details
                    string year = driver.FindElement(By.XPath("/html/body/div[2]/div/span[1]")).Text;
                    IWebElement TBAssess = driver.FindElement(By.XPath("/html/body/div[3]/div/div[8]/table/tbody"));
                    IList<IWebElement> TRAssess = TBAssess.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDAssess;
                    foreach (IWebElement assess in TRAssess)
                    {
                        TDAssess = assess.FindElements(By.TagName("td"));
                        if (TDAssess.Count != 0)
                        {
                            Property_class = TDAssess[0].Text;
                            AssedValues = TDAssess[1].Text;
                            Units = TDAssess[2].Text;
                            Homestead = TDAssess[3].Text;
                            Assement_details = year + "~" + Property_class + "~" + AssedValues + "~" + Units + "~" + Homestead;
                            gc.insert_date(orderNumber, outparcelno, 198, Assement_details, 1, DateTime.Now);
                        }
                    }
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    //TaxDistribution Details
                    IWebElement TBTax = driver.FindElement(By.XPath("/html/body/div[3]/div/div[11]/table/tbody"));
                    IList<IWebElement> TRTax = TBTax.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDTax;
                    foreach (IWebElement tax in TRTax)
                    {
                        TDTax = tax.FindElements(By.TagName("td"));
                        if (TDTax.Count != 0)
                        {
                            Millage = TDTax[0].Text;
                            Mills = TDTax[1].Text;
                            Tax = TDTax[2].Text;
                            Homestead_Tax = TDTax[3].Text;
                            TaxDistributionDetails = Millage + "~" + Mills + "~" + Tax + "~" + Homestead_Tax;
                            gc.insert_date(orderNumber, outparcelno, 201, TaxDistributionDetails, 1, DateTime.Now);
                        }
                    }
                    //TaxInformation Details
                    driver.Navigate().GoToUrl("http://snstaxpayments.com/ebr");
                    Thread.Sleep(2000);

                    driver.FindElement(By.Id("submit")).SendKeys(Keys.Enter);
                    Thread.Sleep(2000);

                    driver.FindElement(By.XPath("/html/body/div[1]/div[3]/form/div/div/div[1]/div[2]/label/input")).Click();
                    Thread.Sleep(2000);
                    driver.FindElement(By.Id("searchFor1")).SendKeys(outparcelno);
                    gc.CreatePdf(orderNumber, outparcelno, "Tax", driver, "LA", "East Baton Rouge");
                    driver.FindElement(By.Id("searchButton")).SendKeys(Keys.Enter);
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, outparcelno, "View", driver, "LA", "East Baton Rouge");

                    IWebElement SelectOption = driver.FindElement(By.Id("taxyear"));
                    IList<IWebElement> Select = SelectOption.FindElements(By.TagName("option"));
                    List<string> option = new List<string>();
                    int Check = 0;
                    foreach (IWebElement Op in Select)
                    {

                        if (Check <= 2)
                        {
                            option.Add(Op.Text);
                            Check++;
                        }

                    }
                    foreach (string item in option)
                    {
                        var SelectAddress = driver.FindElement(By.Id("taxyear"));
                        var SelectAddressTax = new SelectElement(SelectAddress);
                        SelectAddressTax.SelectByText(item);
                        Thread.Sleep(4000);
                        driver.FindElement(By.Id("searchButton")).SendKeys(Keys.Enter);
                        Thread.Sleep(4000);

                        try
                        {
                            driver.FindElement(By.XPath("/html/body/div[1]/div[3]/div[3]/table/tbody/tr/td[1]/button")).Click();
                            Thread.Sleep(7000);
                        }
                        catch
                        { }

                        gc.CreatePdf(orderNumber, outparcelno, "Popup 2015", driver, "LA", "East Baton Rouge");

                        //Open Popup
                        try
                        {
                            Notice = driver.FindElement(By.XPath("/html/body/div[5]/div[2]/div/div/div[3]/div[1]")).Text;
                            Notice = WebDriverTest.After(Notice, "Tax Notice#");

                            Taxyear = driver.FindElement(By.XPath("/html/body/div[5]/div[2]/div/div/div[3]/div[2]")).Text;
                            Taxyear = WebDriverTest.After(Taxyear, "Tax Year");

                            TaxPayer = driver.FindElement(By.XPath("/html/body/div[5]/div[2]/div/div/div[4]")).Text;
                            TaxPayer = WebDriverTest.Between(TaxPayer, "Taxpayer", "**** ").Replace("\r\n", " ").Trim();

                            IWebElement TBOpen = driver.FindElement(By.XPath("/html/body/div[5]/div[2]/div/div/div[5]"));
                            IList<IWebElement> DivMaster = TBOpen.FindElements(By.TagName("div"));
                            foreach (IWebElement div in DivMaster)
                            {
                                Taxes = DivMaster[0].Text;
                                Interest = DivMaster[1].Text;
                                Cost = DivMaster[2].Text;
                                Other = DivMaster[3].Text;
                                Paid = DivMaster[4].Text;
                                Balance = DivMaster[5].Text;
                            }
                            if (Interest.Replace("Interest\r\n", "") != "0.00" && Balance.Replace("Balance\r\n", "") != "0.00")
                            {
                                Deliquent_Interest = Interest;
                                Deliquent_Balance = Balance;
                                Interest = "";
                                Balance = "";
                                DeliquentTaxInformation = Notice + "~" + Taxyear + "~" + TaxPayer + "~" + Taxes.Replace("Taxes\r\n", "") + "~" + Interest.Replace("Interest\r\n", "") + "~" + Cost.Replace("Cost\r\n", "") + "~" + Other.Replace("Other\r\n", "") + "~" + Paid.Replace("Paid\r\n", "") + "~" + Balance.Replace("Balance\r\n", "") + "~" + Deliquent_Interest.Replace("Interest\r\n", "") + "~" + Deliquent_Balance.Replace("Balance\r\n", "");
                                gc.insert_date(orderNumber, outparcelno, 203, DeliquentTaxInformation, 1, DateTime.Now);
                            }
                            else
                            {
                                string TaxInformation = Notice + "~" + Taxyear + "~" + TaxPayer + "~" + Taxes.Replace("Taxes\r\n", "") + "~" + Interest.Replace("Interest\r\n", "") + "~" + Cost.Replace("Cost\r\n", "") + "~" + Other.Replace("Other\r\n", "") + "~" + Paid.Replace("Paid\r\n", "") + "~" + Balance.Replace("Balance\r\n", "") + "~" + Deliquent_Interest.Replace("Interest\r\n", "") + "~" + Deliquent_Balance.Replace("Balance\r\n", "");
                                gc.insert_date(orderNumber, outparcelno, 203, TaxInformation, 1, DateTime.Now);
                            }

                            Legal_Description = driver.FindElement(By.XPath("//*[@id='details']/div[7]")).Text.Replace("Legal", "");

                            //Tax History
                            IWebElement TBHistory = driver.FindElement(By.XPath("/html/body/div[5]/div[2]/div/div/div[9]/table/tbody"));
                            IList<IWebElement> TRHistory = TBHistory.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDHistory;
                            foreach (IWebElement History in TRHistory)
                            {
                                TDHistory = History.FindElements(By.TagName("td"));
                                if (TDHistory.Count != 0)
                                {
                                    Date = TDHistory[0].Text;
                                    Description = TDHistory[1].Text;
                                    Amount = TDHistory[2].Text;
                                    TaxHistory = Date + "~" + Description + "~" + Amount;
                                    gc.insert_date(orderNumber, outparcelno, 202, TaxHistory, 1, DateTime.Now);
                                }
                            }

                            IWebElement ITax = driver.FindElement(By.XPath("//*[@id='details']/div[2]/a"));
                            strTax = ITax.GetAttribute("href");
                            Thread.Sleep(5000);
                            driver.Navigate().GoToUrl(strTax);
                            Actions action = new Actions(driver);
                            action.SendKeys(Keys.Escape).Build().Perform();
                            gc.CreatePdf(orderNumber, outparcelno, "Bill 2015", driver, "LA", "East Baton Rouge");
                            driver.Navigate().Back();
                            Thread.Sleep(2000);


                        }
                        catch
                        { }
                    }
                    try
                    {
                        for (int k = 1; k < 4; k++)
                        {
                            if (k == 1)
                            {
                                try
                                {
                                    var SelectAddress2017 = driver.FindElement(By.Id("taxyear"));
                                    var SelectAddressTax2017 = new SelectElement(SelectAddress2017);
                                    SelectAddressTax2017.SelectByIndex(0);
                                    Thread.Sleep(4000);
                                    driver.FindElement(By.Id("searchButton")).SendKeys(Keys.Enter);
                                    Thread.Sleep(4000);
                                    gc.CreatePdf(orderNumber, outparcelno, "View1", driver, "LA", "East Baton Rouge");
                                    driver.FindElement(By.XPath("/html/body/div[1]/div[3]/div[3]/table/tbody/tr/td[1]/button")).Click();
                                    Thread.Sleep(5000);

                                    IWebElement ITax = driver.FindElement(By.XPath("//*[@id='details']/div[2]/a"));
                                    strTax = ITax.GetAttribute("href");
                                    Thread.Sleep(5000);
                                    driver.Navigate().GoToUrl(strTax);
                                    Actions action = new Actions(driver);
                                    action.SendKeys(Keys.Escape).Build().Perform();
                                    gc.CreatePdf(orderNumber, outparcelno, "Bill 2017", driver, "LA", "East Baton Rouge");
                                    driver.Navigate().Back();
                                    Thread.Sleep(2000);
                                }
                                catch
                                { }
                            }
                            else if (k == 2)
                            {
                                try
                                {
                                    var SelectAddress2016 = driver.FindElement(By.Id("taxyear"));
                                    var SelectAddressTax2016 = new SelectElement(SelectAddress2016);
                                    SelectAddressTax2016.SelectByIndex(1);
                                    Thread.Sleep(4000);
                                    driver.FindElement(By.Id("searchButton")).SendKeys(Keys.Enter);
                                    Thread.Sleep(4000);
                                    gc.CreatePdf(orderNumber, outparcelno, "View2", driver, "LA", "East Baton Rouge");
                                    driver.FindElement(By.XPath("/html/body/div[1]/div[3]/div[3]/table/tbody/tr/td[1]/button")).Click();
                                    Thread.Sleep(5000);

                                    IWebElement ITax = driver.FindElement(By.XPath("//*[@id='details']/div[2]/a"));
                                    strTax = ITax.GetAttribute("href");
                                    Thread.Sleep(5000);
                                    driver.Navigate().GoToUrl(strTax);
                                    Actions action = new Actions(driver);
                                    action.SendKeys(Keys.Escape).Build().Perform();
                                    gc.CreatePdf(orderNumber, outparcelno, "Bill 2016", driver, "LA", "East Baton Rouge");
                                    driver.Close();
                                }
                                catch
                                { }
                            }
                        }
                    }
                    catch
                    { }

                    property_details = OwnerName + "~" + Propertyaddress + "~" + Mailingaddress + "~" + Property_Type + "~" + Legal_Description;
                    gc.insert_date(orderNumber, outparcelno, 197, property_details, 1, DateTime.Now);

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "LA", "East Baton Rouge", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    //megrge pdf files
                    gc.mergpdf(orderNumber, "LA", "East Baton Rouge");
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
        public void NavigateUrl(IWebDriver driver)
        {
            //Navigate URl
            IWebElement MultiParcelTable1 = driver.FindElement(By.XPath("/html/body/div[2]/div/div[3]/div/div/table"));
            IList<IWebElement> MultiParcelTR1 = MultiParcelTable1.FindElements(By.TagName("a"));

            List<string> urlList = new List<string>();
            foreach (IWebElement url in MultiParcelTR1)
            {
                string strUrl = url.GetAttribute("href");
                urlList.Add(strUrl);
            }

            foreach (string assURL in urlList)
            {
                driver.Navigate().GoToUrl(assURL);
                Thread.Sleep(3000);
            }
        }

    }
}