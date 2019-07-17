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
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Globalization;

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_HighlandsFL
    {

        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());


        public string FTP_HighlandsFL(string sno, string sname, string direction, string sttype, string unino, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "", AssessTakenTime = "", TaxTakentime = "", CityTaxtakentime = "";

            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            // driver = new PhantomJSDriver();
            //driver = new ChromeDriver();
            //string outparcel = "", location = "", currentowner = "", mailaddr = "", legaldes = "", taxdist = "", assclass = "", legalref = "";
            //string propuse = "", zone = "", neighborhood = "", landarea = "", proptype = "", yearbuilt = "";
            using (driver = new PhantomJSDriver()) //ChromeDriver
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    if (searchType == "titleflex")
                    {
                        string straddress = "";
                        if (direction != "")
                        {
                            straddress = sno + " " + direction + " " + sname + " " + sttype + " " + unino;
                        }
                        else
                        {
                            straddress = sno + " " + sname + " " + sttype + " " + unino;
                        }
                        gc.TitleFlexSearch(orderNumber, parcelNumber, ownername, straddress, "FL", "Highlands");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_FLHighlands"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    driver.Navigate().GoToUrl("https://www.hcpao.org/Search");

                    if (searchType == "address")
                    {
                        driver.FindElement(By.XPath("//*[@id='searchRealEstate']/ul/li[4]/a")).Click();
                        driver.FindElement(By.Id("RealEstateStreetNumber")).SendKeys(sno);
                        driver.FindElement(By.Id("RealEstateStreetName")).SendKeys(sname);
                        driver.FindElement(By.Id("RealEstateStreetPredirectional")).SendKeys(direction);
                        driver.FindElement(By.Id("RealEstateStreetSuffix")).SendKeys(sttype);
                        gc.CreatePdf(orderNumber, parcelNumber, "Address Search input passed", driver, "FL", "Highlands");
                        driver.FindElement(By.XPath("//*[@id='address']/div[5]/div/button")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Address Search Results", driver, "FL", "Highlands");
                        ////Multiparcel
                        try
                        {
                            //int Count = 0;
                            IWebElement Multiaddresstable = driver.FindElement(By.XPath("//*[@id='results']/div[2]/table"));
                            IList<IWebElement> multiaddressrow = Multiaddresstable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Multiaddressid;
                            foreach (IWebElement Multiaddress in multiaddressrow)
                            {
                                Multiaddressid = Multiaddress.FindElements(By.TagName("td"));
                                if (multiaddressrow.Count > 2 && Multiaddressid.Count != 0 && !Multiaddress.Text.Contains("Parcel"))
                                {
                                    string Multiparcelnumber = Multiaddressid[1].Text;
                                    string OWnername = Multiaddressid[2].Text;
                                    string Address1 = Multiaddressid[3].Text;
                                    string City = Multiaddressid[4].Text;
                                    string multiaddressresult = OWnername + "~" + Address1 + "~" + City;
                                    gc.insert_date(orderNumber, Multiparcelnumber, 1465, multiaddressresult, 1, DateTime.Now);
                                    //Count++;
                                }
                            }
                            if (multiaddressrow.Count < 2)
                            {
                                driver.FindElement(By.XPath("//*[@id='searchResults']/tbody/tr[3]/td[2]")).Click();
                                Thread.Sleep(2000);
                            }

                            if (multiaddressrow.Count > 2 && multiaddressrow.Count < 26)
                            {
                                HttpContext.Current.Session["multiParcel_FLHighlands"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (multiaddressrow.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_FLHighlands_Multicount"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                            //if (multiaddressrow.Count == 0)
                            //{
                            //    HttpContext.Current.Session["Zero_FLHighlands"] = "Zero";
                            //    driver.Quit();
                            //    return "Zero";
                            //}

                        }
                        catch { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='results']/div/div[1]/b")).Text;
                            if (nodata.Contains("Returned 0 records."))
                            {
                                HttpContext.Current.Session["Nodata_FLHighlands"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }

                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.XPath("//*[@id='searchRealEstate']/ul/li[2]/a")).Click();
                        Thread.Sleep(3000);
                        parcelNumber = parcelNumber.Replace(" ", "").Replace(".", "").Replace("-", "").Trim();
                        IWebElement element = driver.FindElement(By.Id("RealEstateParcelId"));
                        element.Clear();
                        driver.FindElement(By.Id("RealEstateParcelId")).Click();
                        driver.FindElement(By.Id("RealEstateParcelId")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search input passed", driver, "FL", "Highlands");
                        driver.FindElement(By.XPath("//*[@id='parcel']/div/div/button")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Results", driver, "FL", "Highlands");
                        //*[@id="results"]/div/div[1]/b
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='results']/div/div[1]/b")).Text;
                            if (nodata.Contains("Returned 0 records."))
                            {
                                HttpContext.Current.Session["Nodata_FLHighlands"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.XPath("//*[@id='searchRealEstate']/ul/li[3]/a")).Click();
                        driver.FindElement(By.Id("RealEstateOwnerName")).SendKeys(ownername);
                        gc.CreatePdf(orderNumber, parcelNumber, "Ownername Search Results1", driver, "FL", "Highlands");
                        driver.FindElement(By.XPath("//*[@id='owner']/div/div/button")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Ownername Search Results", driver, "FL", "Highlands");

                        ////Multiparcel
                        try
                        {
                            //int Count = 0;
                            IWebElement Multiaddresstable = driver.FindElement(By.XPath("//*[@id='results']/div[2]/table"));
                            IList<IWebElement> multiaddressrow = Multiaddresstable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Multiaddressid;
                            foreach (IWebElement Multiaddress in multiaddressrow)
                            {
                                Multiaddressid = Multiaddress.FindElements(By.TagName("td"));
                                if (multiaddressrow.Count > 2 && Multiaddressid.Count != 0 && !Multiaddress.Text.Contains("Parcel"))
                                {
                                    string Multiparcelnumber = Multiaddressid[1].Text;
                                    string OWnername = Multiaddressid[2].Text;
                                    string Address1 = Multiaddressid[3].Text;
                                    string City = Multiaddressid[4].Text;
                                    string multiaddressresult = OWnername + "~" + Address1 + "~" + City;
                                    gc.insert_date(orderNumber, Multiparcelnumber, 1465, multiaddressresult, 1, DateTime.Now);
                                    //Count++;
                                }
                            }
                            if (multiaddressrow.Count < 2)
                            {
                                driver.FindElement(By.XPath("//*[@id='searchResults']/tbody/tr[3]/td[2]")).Click();
                                Thread.Sleep(2000);
                            }

                            if (multiaddressrow.Count > 2 && multiaddressrow.Count < 26)
                            {
                                HttpContext.Current.Session["multiParcel_FLHighlands"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (multiaddressrow.Count > 3 && multiaddressrow.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_FLHighlands_Multicount"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                            //if (multiaddressrow.Count == 0)
                            //{
                            //    HttpContext.Current.Session["Zero_FLHighlands"] = "Zero";
                            //    driver.Quit();
                            //    return "Zero";
                            //}

                        }
                        catch { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='results']/div/div[1]/b")).Text;
                            if (nodata.Contains("Returned 0 records."))
                            {
                                HttpContext.Current.Session["Nodata_FLHighlands"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='results']/div[2]/table/tbody/tr/td[2]/a")).Click();
                        Thread.Sleep(2000);
                    }
                    catch { }
                    gc.CreatePdf(orderNumber, parcelNumber, "Property Search Results", driver, "FL", "Highlands");
                    //Property Details
                    string ParcelNumber = "", OwnerName = "", PropertyAddress = "", MailingAddress = "", DorCode = "", Neighborhood = "", LegalDescription = "", Yearbuilt = "";
                    ParcelNumber = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div[1]/h2")).Text.Replace("Parcel", "").Trim();
                    PropertyAddress = driver.FindElement(By.XPath("/html/body/div[2]/div[2]/div[1]/p")).Text.Trim();

                    IWebElement Bigdata1 = driver.FindElement(By.XPath("/html/body/div[2]/div[3]/div[1]"));
                    OwnerName = gc.Between(Bigdata1.Text, "Owners:", "Mailing Address").Trim();
                    MailingAddress = gc.Between(Bigdata1.Text.Replace("\r\n", " "), "Mailing Address", "DOR Code:").Trim();
                    //string[] MailingAddress1split = MailingAddress1.Split(' ');
                    DorCode = gc.Between(Bigdata1.Text, "DOR Code:", "Neighborhood:").Trim();
                    Neighborhood = gc.Between(Bigdata1.Text, "Neighborhood:", "Millage:").Trim();
                    LegalDescription = GlobalClass.After(Bigdata1.Text, "Legal Description").Trim();
                    try
                    {
                        Yearbuilt = driver.FindElement(By.XPath("/html/body/div[2]/div[6]/div/div[1]/table/tbody/tr/td[7]")).Text;
                    }
                    catch { }


                    //Assessment Details
                    string title = "", value = "";

                    IWebElement Bigdata2 = driver.FindElement(By.XPath("/html/body/div[2]/div[4]/div[1]/div/table/tbody"));
                    IList<IWebElement> TRBigdata2 = Bigdata2.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDBigdata2;
                    foreach (IWebElement row1 in TRBigdata2)
                    {
                        TDBigdata2 = row1.FindElements(By.TagName("td"));

                        if (TDBigdata2.Count != 0 && TDBigdata2.Count == 2 && TDBigdata2[0].Text.Trim() != "")
                        {
                            title += TDBigdata2[0].Text + "~";
                            value += TDBigdata2[1].Text + "~";
                        }
                    }
                    IWebElement Bigdata3 = driver.FindElement(By.XPath("/html/body/div[2]/div[4]/div[2]/table/tbody"));
                    IList<IWebElement> TRBigdata3 = Bigdata3.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDBigdata3;
                    foreach (IWebElement row2 in TRBigdata3)
                    {
                        TDBigdata3 = row2.FindElements(By.TagName("td"));

                        if (TDBigdata3.Count != 0 && TDBigdata3.Count == 2 && TDBigdata3[0].Text.Trim() != "")
                        {
                            title += TDBigdata3[0].Text + "~";
                            value += TDBigdata3[1].Text + "~";
                        }
                    }
                    title = title.TrimEnd('~');
                    value = value.TrimEnd('~');
                    db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + title + "' where Id = '" + 1444 + "'");
                    gc.insert_date(orderNumber, ParcelNumber, 1444, value, 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    //Tax Authority
                    string TaxAuthority = "", TaxAuthority1 = "", TaxAuthority2 = "", TaxAuthority3 = "", TaxAuthority4 = "";
                    try
                    {
                        driver.Navigate().GoToUrl("https://www.hctaxcollector.com/make-payments/payments-by-mail/");
                        TaxAuthority1 = driver.FindElement(By.XPath("//*[@id='post-143']/div[2]/p[2]")).Text;
                        string[] Taxauthoritysplit = TaxAuthority1.Split('\r');
                        TaxAuthority2 = Taxauthoritysplit[0].Trim();
                        TaxAuthority3 = Taxauthoritysplit[1].Trim();
                        TaxAuthority4 = Taxauthoritysplit[2].Trim();
                        TaxAuthority = TaxAuthority2 + " " + TaxAuthority3 + " " + TaxAuthority4;
                    }
                    catch { }
                    //Property Details
                    string PropertyDetails1 = OwnerName.Trim() + "~" + PropertyAddress.Trim() + "~" + MailingAddress.Trim() + "~" + DorCode.Trim() + "~" + Neighborhood.Trim() + "~" + LegalDescription.Trim() + "~" + Yearbuilt.Trim() + "~" + TaxAuthority.Trim();
                    gc.insert_date(orderNumber, ParcelNumber, 1442, PropertyDetails1, 1, DateTime.Now);

                    //Tax Information Details
                    driver.Navigate().GoToUrl("https://highlands-fl.mygovonline.com/mod.php?mod=propertytax&mode=public_lookup");
                    Thread.Sleep(1000);

                    string outparcel1 = "";
                    var Select = driver.FindElement(By.Name("selectMenu"));
                    var selectElement = new SelectElement(Select);
                    selectElement.SelectByIndex(4);
                    ParcelNumber = ParcelNumber.Replace(" ", "").Replace(".", "").Replace("-", "").Trim();

                    int lencount = ParcelNumber.Length;
                    if (lencount == 18)
                    {
                        outparcel1 = ParcelNumber.Substring(0, 7) + "-" + ParcelNumber.Substring(7, 11);
                    }
                    driver.FindElement(By.Id("tax_account_id")).SendKeys(outparcel1);
                    gc.CreatePdf(orderNumber, ParcelNumber, "taxsearchBefore", driver, "FL", "Highlands");
                    driver.FindElement(By.Id("submit_btn")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, ParcelNumber, "taxsearchAfter", driver, "FL", "Highlands");
                    driver.FindElement(By.LinkText("View Bill")).Click();
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, ParcelNumber, "taxinfo", driver, "FL", "Highlands");

                    string Bankrupt = "", InformationComments = "";
                    try
                    {
                        Bankrupt = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div")).Text.Trim();

                        if (Bankrupt.Contains("Please contact/visit the Highlands County Tax Collector's office in order to find out the appropriate steps to take in order to settle your property taxes."))
                        {

                            InformationComments = "Please contact/visit the Highlands County Tax Collector's office";
                            string alertmessage = InformationComments;
                            gc.insert_date(orderNumber, ParcelNumber, 1464, alertmessage, 1, DateTime.Now);
                        }

                    }
                    catch { }
                    try
                    {
                        //Parcel History Details
                        string Year = "", Account = "", Rcpt = "", Balancedue = "", Confirm = "";
                        List<string> billinfo = new List<string>();
                        IWebElement Billsinfo2 = driver.FindElement(By.Id("data"));
                        IList<IWebElement> TRBillsinfo2 = Billsinfo2.FindElements(By.TagName("tr"));
                        IList<IWebElement> Aherftax;
                        int i = 0;
                        foreach (IWebElement row in TRBillsinfo2)
                        {
                            Aherftax = row.FindElements(By.TagName("td"));

                            if (Aherftax.Count == 6 && !row.Text.Contains("Year"))
                            {
                                Year = Aherftax[1].Text;
                                Account = Aherftax[2].Text;
                                Rcpt = Aherftax[3].Text;
                                Balancedue = Aherftax[4].Text;
                                Confirm = Aherftax[5].Text;

                                string ParcelHistorydetails = Year.Trim() + "~" + Account.Trim() + "~" + Rcpt.Trim() + "~" + Balancedue.Trim() + "~" + Confirm.Trim();
                                gc.insert_date(orderNumber, ParcelNumber, 1454, ParcelHistorydetails, 1, DateTime.Now);
                            }
                            if (Aherftax.Count == 6 && !row.Text.Contains("Year") && billinfo.Count < 3)
                            {
                                IWebElement value1 = Aherftax[2].FindElement(By.TagName("a"));
                                string addview = value1.GetAttribute("href");
                                billinfo.Add(addview);

                            }
                        }
                        int p = 0;
                        foreach (string assessmentclick in billinfo)
                        {

                            driver.Navigate().GoToUrl(assessmentclick);
                            gc.CreatePdf(orderNumber, ParcelNumber, "Parcel History Yearwis clik" + p, driver, "FL", "Highlands");

                            //General Information
                            string TaxYear = "", TaxYear1 = "", TaxPropertyAddress = "", TaxParcel = "", TaxParcel1 = "", Billnumber = "", Propertytype = "", Owner1 = "", Mailingaddress = "", Situs = "", Legal = "";
                            TaxParcel1 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table[2]/tbody/tr[1]/th/div")).Text;
                            TaxParcel = GlobalClass.After(TaxParcel1, "#").Trim();
                            TaxYear1 = GlobalClass.Before(TaxParcel1, ") #").Trim();
                            TaxYear = GlobalClass.After(TaxYear1, "Property Tax Details for Account (").Trim();

                            IWebElement Bigdata4 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table[2]/tbody/tr[2]/td/table/tbody/tr/td[1]/table/tbody"));
                            IList<IWebElement> TRBigdata4 = Bigdata4.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDBigdata4;
                            foreach (IWebElement row3 in TRBigdata4)
                            {
                                TDBigdata4 = row3.FindElements(By.TagName("td"));

                                if (TDBigdata4.Count != 0 && TDBigdata4.Count == 2 && TDBigdata4[0].Text.Trim() != "" && row3.Text.Contains("BILL NUMBER:"))
                                {
                                    Billnumber = TDBigdata4[1].Text;
                                }
                                if (TDBigdata4.Count != 0 && TDBigdata4.Count == 2 && TDBigdata4[0].Text.Trim() != "" && row3.Text.Contains("PROPERTY TYPE:"))
                                {
                                    Propertytype = TDBigdata4[1].Text;
                                }
                                if (TDBigdata4.Count != 0 && TDBigdata4.Count == 2 && TDBigdata4[0].Text.Trim() != "" && row3.Text.Contains("OWNER:"))
                                {
                                    Owner1 = TDBigdata4[1].Text;
                                }
                                if (TDBigdata4.Count != 0 && TDBigdata4.Count == 2 && TDBigdata4[0].Text.Trim() == "" && !row3.Text.Contains("OWNER:"))
                                {
                                    Mailingaddress = TDBigdata4[1].Text;
                                }
                                if (TDBigdata4.Count != 0 && TDBigdata4.Count == 4 && TDBigdata4[0].Text.Trim() != "" && row3.Text.Contains("SITUS:"))
                                {
                                    Situs = TDBigdata4[1].Text;
                                }
                                if (TDBigdata4.Count != 0 && TDBigdata4.Count == 5 && TDBigdata4[0].Text.Trim() != "" && row3.Text.Contains("LEGAL:"))
                                {
                                    Legal = TDBigdata4[1].Text;
                                }
                            }
                            string TaxPropertyAddress1 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table[2]/tbody/tr[2]/td/table/tbody/tr/td[2]")).Text.Replace("Map not available for this bill", "");
                            TaxPropertyAddress = GlobalClass.After(TaxPropertyAddress1, "PROPERTY ADDRESS:").Trim();
                            string Taxinformations = TaxYear.Trim() + "~" + Billnumber.Trim() + "~" + Owner1.Trim() + "~" + Propertytype.Trim() + "~" + TaxPropertyAddress.Trim() + "~" + Mailingaddress.Trim() + "~" + Situs.Trim() + "~" + Legal.Trim();
                            gc.insert_date(orderNumber, TaxParcel, 1446, Taxinformations, 1, DateTime.Now);

                            //Ad Valorem Taxes Details
                            string AdValoremAuthority = "", AdValoremAssessed = "", AdValoremExempts = "", AdValoremTaxable = "", AdValoremMillage = "", AdValoremTotal = "";
                            IWebElement Bigdata5 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table[2]/tbody/tr[3]/td/fieldset[1]/table/tbody"));
                            IList<IWebElement> TRBigdata5 = Bigdata5.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDBigdata5;
                            foreach (IWebElement row4 in TRBigdata5)
                            {
                                TDBigdata5 = row4.FindElements(By.TagName("td"));

                                if (TDBigdata5.Count != 0 && TDBigdata5.Count == 6 && TDBigdata5[0].Text.Trim() != "" && !row4.Text.Contains("Authority"))
                                {
                                    AdValoremAuthority = TDBigdata5[0].Text;
                                    AdValoremAssessed = TDBigdata5[1].Text;
                                    AdValoremExempts = TDBigdata5[2].Text;
                                    AdValoremTaxable = TDBigdata5[3].Text;
                                    AdValoremMillage = TDBigdata5[4].Text;
                                    AdValoremTotal = TDBigdata5[5].Text;
                                    string AdvaloremTax = TaxYear.Trim() + "~" + AdValoremAuthority.Trim() + "~" + AdValoremAssessed.Trim() + "~" + AdValoremExempts.Trim() + "~" + AdValoremTaxable.Trim() + "~" + AdValoremMillage.Trim() + "~" + AdValoremTotal.Trim();
                                    gc.insert_date(orderNumber, TaxParcel, 1447, AdvaloremTax, 1, DateTime.Now);
                                }
                                if (TDBigdata5.Count != 0 && TDBigdata5.Count == 2 && TDBigdata5[0].Text.Trim() != "" && !row4.Text.Contains("Authority"))
                                {
                                    AdValoremTaxable = TDBigdata5[0].Text;
                                    AdValoremTotal = TDBigdata5[1].Text;
                                    string AdvaloremTax = TaxYear.Trim() + "~" + AdValoremTaxable.Trim() + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + AdValoremTotal.Trim();
                                    gc.insert_date(orderNumber, TaxParcel, 1447, AdvaloremTax, 1, DateTime.Now);
                                }
                            }
                            //Non Ad Valorem
                            string NonAdValoremAuthority = "", NonAdValoremAssessed = "", NonAdValoremExempts = "", NonAdValoremTaxable = "", NonAdValoremMillage = "", NonAdValoremTotal = "";
                            IWebElement Bigdata6 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table[2]/tbody/tr[3]/td/fieldset[2]/table/tbody"));
                            IList<IWebElement> TRBigdata6 = Bigdata6.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDBigdata6;
                            foreach (IWebElement row5 in TRBigdata6)
                            {
                                TDBigdata6 = row5.FindElements(By.TagName("td"));

                                if (TDBigdata6.Count != 0 && TDBigdata6.Count == 2 && TDBigdata6[0].Text.Trim() != "" && !row5.Text.Contains("Authority") && !row5.Text.Contains("Non Ad-Valorem Total: $"))
                                {
                                    NonAdValoremAuthority = TDBigdata6[0].Text;
                                    NonAdValoremTotal = TDBigdata6[1].Text;

                                    string NonAdvaloremTax = TaxYear.Trim() + "~" + NonAdValoremAuthority + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + NonAdValoremTotal.Trim();
                                    gc.insert_date(orderNumber, TaxParcel, 1447, NonAdvaloremTax, 1, DateTime.Now);
                                }
                                if (TDBigdata6.Count != 0 && TDBigdata6.Count == 2 && TDBigdata6[0].Text.Trim() != "" && row5.Text.Contains("Non Ad-Valorem Total: $"))
                                {
                                    NonAdValoremTaxable = TDBigdata6[0].Text;
                                    NonAdValoremTotal = TDBigdata6[1].Text;

                                    string NonAdvaloremTax = TaxYear.Trim() + "~" + NonAdValoremTaxable.Trim() + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + NonAdValoremTotal.Trim();
                                    gc.insert_date(orderNumber, TaxParcel, 1447, NonAdvaloremTax, 1, DateTime.Now);
                                }
                            }
                            //Total Certified Combined
                            string Totalcertified = "", Totalcertifiedvalue = "", Bulkdata = "";
                            Bulkdata = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table[2]/tbody/tr[3]/td/table/tbody/tr/td")).Text;
                            Totalcertified = GlobalClass.Before(Bulkdata, ":").Trim();
                            Totalcertifiedvalue = GlobalClass.After(Bulkdata, ":").Trim();
                            string NonAdvaloremTax1 = TaxYear + "~" + Totalcertified.Trim() + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + Totalcertifiedvalue.Trim();
                            gc.insert_date(orderNumber, TaxParcel, 1447, NonAdvaloremTax1, 1, DateTime.Now);

                            //Payments Details

                            string Payby = "", PleasePay = "";
                            string value1 = "", value2 = "", value3 = "", value4 = "", value5 = "", title1 = "", title2 = "", strval12 = "", strval22 = "", strval13 = "", strval23 = "", strval14 = "", strval24 = "";
                            string faceamt = "", Bid = "", strDate = "", strAmount = "", strval1 = "", strval2 = "", Paymentdetails = "";
                            string Certificate = "", paymenttitle = "", paymentvalue = "", FaceAmount = "", Buyer = "", Issuedyear1 = "", Issuedyear2 = "", Issuedyear3 = "", Issuedyear4 = "", Issuedyear5 = "";
                            IWebElement Bigdata8 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table[2]/tbody/tr[3]/td/fieldset[4]/fieldset/table/tbody"));
                            IList<IWebElement> TRBigdata8 = Bigdata8.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDBigdata8;
                            foreach (IWebElement row7 in TRBigdata8)
                            {
                                TDBigdata8 = row7.FindElements(By.TagName("td"));

                                if (TDBigdata8.Count != 0 && TDBigdata8.Count == 2 && TDBigdata8[1].Text.Trim() != "" && row7.Text.Contains("Pay By"))
                                {
                                    Payby = GlobalClass.Before(TDBigdata8[1].Text, "\r\n");
                                    PleasePay = GlobalClass.After(TDBigdata8[1].Text, "\r\n");
                                    gc.insert_date(orderNumber, ParcelNumber, 1450, TaxYear + "~" + "" + "~" + "" + "~" + "" + "~" + Payby + "~" + PleasePay, 1, DateTime.Now);
                                }
                                //Count Five

                                if (TDBigdata8.Count != 0 && TDBigdata8.Count == 5 && TDBigdata8[1].Text.Trim() != "" && row7.Text.Contains("Pay By") || row7.Text.Contains("Face Amt"))
                                {
                                    if (TDBigdata8.Count != 0 && TDBigdata8.Count == 5 && TDBigdata8[0].Text.Trim() != "" && row7.Text.Contains("Certificate"))
                                    {
                                        Certificate = TDBigdata8[0].Text;
                                    }
                                    if (TDBigdata8.Count != 0 && TDBigdata8.Count == 4 && TDBigdata8[0].Text.Trim() != "" && row7.Text.Contains("Certificate"))
                                    {
                                        Certificate = TDBigdata8[0].Text;
                                    }
                                    try
                                    {
                                        if (row7.Text.Contains("Face Amt"))
                                        {
                                            faceamt = gc.Between(TDBigdata8[1].Text, "Face Amt.", "\r\n");
                                            Bid = GlobalClass.After(TDBigdata8[1].Text, "Bid %");
                                            //gc.insert_date(orderNumber, ParcelNumber, 1450, TaxYear + "~" + Certificate + "~" + faceamt + "~" + Bid + "~" + "" + "~" + "", 1, DateTime.Now);
                                        }
                                    }
                                    catch { }
                                    try
                                    {
                                        if (row7.Text.Contains("Pay By"))
                                        {
                                            strval1 = GlobalClass.Before(TDBigdata8[1].Text, "\r\n");
                                            strval2 = GlobalClass.After(TDBigdata8[1].Text, "\r\n");
                                        }
                                    }
                                    catch { }
                                    strval12 = GlobalClass.Before(TDBigdata8[2].Text, "\r\n").Replace("If Paid By", "");
                                    strval22 = GlobalClass.After(TDBigdata8[2].Text, "\r\n").Replace("Please Pay", "");
                                    strval13 = GlobalClass.Before(TDBigdata8[3].Text, "\r\n");
                                    strval23 = GlobalClass.After(TDBigdata8[3].Text, "\r\n");
                                    try
                                    {
                                        strval14 = GlobalClass.Before(TDBigdata8[4].Text, "\r\n");
                                        strval24 = GlobalClass.After(TDBigdata8[4].Text, "\r\n");
                                    }
                                    catch { }
                                    if (!TDBigdata8[0].Text.Contains("Certificate"))
                                    {
                                        gc.insert_date(orderNumber, ParcelNumber, 1450, TaxYear + "~" + Certificate + "~" + faceamt + "~" + Bid + "~" + strval1 + "~" + strval2, 1, DateTime.Now);
                                    }
                                    if (strval12 != "")
                                    {
                                        gc.insert_date(orderNumber, ParcelNumber, 1450, TaxYear + "~" + Certificate + "~" + faceamt + "~" + Bid + "~" + strval12 + "~" + strval22, 1, DateTime.Now);
                                    }
                                    gc.insert_date(orderNumber, ParcelNumber, 1450, TaxYear + "~" + Certificate + "~" + faceamt + "~" + Bid + "~" + strval13 + "~" + strval23, 1, DateTime.Now);
                                    Certificate = ""; faceamt = ""; Bid = "";
                                    gc.insert_date(orderNumber, ParcelNumber, 1450, TaxYear + "~" + Certificate + "~" + faceamt + "~" + Bid + "~" + strval14 + "~" + strval24, 1, DateTime.Now);
                                    strval1 = ""; strval2 = ""; strval12 = ""; strval13 = ""; strval14 = ""; strval22 = ""; strval23 = ""; strval24 = ""; faceamt = ""; Bid = "";
                                }
                            }
                            //Fees and Interest
                            string Description = "", Total = "";
                            IWebElement Bigdata7 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table[2]/tbody/tr[3]/td/fieldset[3]/table/tbody"));
                            IList<IWebElement> TRBigdata7 = Bigdata7.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDBigdata7;
                            foreach (IWebElement row6 in TRBigdata7)
                            {
                                TDBigdata7 = row6.FindElements(By.TagName("td"));

                                if (TDBigdata7.Count != 0 && TDBigdata7.Count == 2 && TDBigdata7[0].Text.Trim() != "" && !row6.Text.Contains("Description"))
                                {
                                    Description = TDBigdata7[0].Text;
                                    Total = TDBigdata7[1].Text;

                                    string FeesandInterest = TaxYear.Trim() + "~" + Certificate.Trim() + "~" + FaceAmount.Trim() + "~" + Description.Trim() + "~" + Total.Trim();
                                    gc.insert_date(orderNumber, TaxParcel, 1449, FeesandInterest, 1, DateTime.Now);
                                }
                            }
                            //Payment History Details
                            try
                            {
                                string Paymenthistoryyear1 = "", Paymenthistoryyear = "", Date = "", Status = "", PaidBy = "", Amount = "";

                                Paymenthistoryyear1 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table[3]/tbody/tr/td[1]/div[1]")).Text;
                                Paymenthistoryyear = GlobalClass.After(Paymenthistoryyear1, "Payment History -").Replace("Tax Year", "").Trim();
                                IWebElement Bigdata9 = driver.FindElement(By.Id("data0"));
                                IList<IWebElement> TRBigdata9 = Bigdata9.FindElements(By.TagName("tr"));
                                IList<IWebElement> TDBigdata9;
                                foreach (IWebElement row8 in TRBigdata9)
                                {
                                    TDBigdata9 = row8.FindElements(By.TagName("td"));

                                    if (TDBigdata9.Count != 0 && TDBigdata9.Count == 6 && TDBigdata9[0].Text.Trim() != "")
                                    {
                                        //Paymenthistoryyear = TDBigdata9[0].Text;
                                        Date = TDBigdata9[1].Text;
                                        Status = TDBigdata9[2].Text;
                                        PaidBy = TDBigdata9[3].Text;
                                        Amount = TDBigdata9[4].Text;

                                        string PaymentHistorydetails = Paymenthistoryyear.Trim() + "~" + Date.Trim() + "~" + Status.Trim() + "~" + PaidBy.Trim() + "~" + Amount.Trim();
                                        gc.insert_date(orderNumber, TaxParcel, 1451, PaymentHistorydetails, 1, DateTime.Now);
                                    }
                                }
                            }
                            catch { }
                            //Tax Bill Download 
                            try
                            {
                                IWebElement href = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table[2]/tbody/tr[2]/td/table/tbody/tr/td[1]/table/tbody/tr[8]/td/table/tbody/tr/td[3]/a"));
                                string addview = href.GetAttribute("href");
                                gc.downloadfile(addview, orderNumber, parcelNumber, "ViewTaxBill" + p, "FL", "Highlands");
                            }
                            catch { }
                            try
                            {
                                IWebElement href1 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table[2]/tbody/tr[2]/td/table/tbody/tr/td[1]/table/tbody/tr[8]/td/table/tbody/tr/td[3]/a"));
                                string addview1 = href1.GetAttribute("href");
                                gc.downloadfile(addview1, orderNumber, parcelNumber, "Receipt" + p, "FL", "Highlands");
                            }
                            catch { }
                            p++;
                        }
                    }
                    catch { }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "FL", "Highlands", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    driver.Quit();
                    gc.mergpdf(orderNumber, "FL", "Highlands");
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