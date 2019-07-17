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
    public class WebDriver_FLHillsborough
    {
        GlobalClass gc = new GlobalClass();
        string strBill = "-", strBalance = "-", strBillDate = "-", strBillPaid = "-", strFBill = "-", strFBalance = "-", strFBillDate = "-", strFBillPaid = "-";
        Amrock amck = new Amrock();
        IList<IWebElement> taxPaymentdetails, taxPaymentAmountdetails, Itaxtd;
        List<string> strSecured = new List<string>();
        List<string> strCombinedTax = new List<string>();
        List<string> strTaxRealestate = new List<string>();
        List<string> strTaxRealCount = new List<string>();
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        private object driverIE;
        private int multicount;
        private string strMultiAddress;
        private string b;
        string mul = "";
        IWebElement Addressclick;
        string TaxYear = "", TaxAmount = "", PaidAmount = "", ReceiptNumber = "", Account_number = "", Millage_Code = "", Millage_rate = "";

        public string FTP_Hillsborough(string houseno, string housetype, string sname, string unitno, string ownername, string parcelNumber, string searchType, string orderNumber, string direction)
        {
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "", AssessTakenTime = "", TaxTakentime = "", CityTaxtakentime = "";
            string TotaltakenTime = "";
            GlobalClass gc = new GlobalClass();
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string owner = "", folio_parcel_no = "-", Legal_desc = "", property_use = "", Tax_district = "", Year_built = "", Neighbourhood = "", subdivision = "", situs_address = "", pin = "";
            int I = 0;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            // driver = new PhantomJSDriver();
            //driver = new ChromeDriver();
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("http://gis.hcpafl.org/PropertySearch/#/nav/Basic%20Search");

                    Thread.Sleep(4000);
                    if (searchType == "titleflex")
                    {
                        string titleaddress = houseno + " " + sname + " " + housetype + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "FL", "Hillsborough");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Zero_Hillsborough"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        searchType = "block";
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        unitno = parcelNumber.Replace(".", "").Replace("-", ""); ;
                    }
                    string address = "";
                    if (searchType == "address")
                    {
                        if (direction != "")
                        {
                            address = houseno + " " + sname + " " + housetype;
                        }
                        else
                        {
                            address = houseno + " " + direction + " " + sname + " " + housetype;
                        }
                        driver.FindElement(By.XPath("//*[@id='basic']/label[3]/input")).SendKeys(address);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "FL", "Hillsborough");
                        driver.FindElement(By.XPath(" //*[@id='basic']/button[1]")).Click();
                        Thread.Sleep(5000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search result", driver, "FL", "Hillsborough");
                        try
                        {
                            string Nodata = driver.FindElement(By.XPath("//*[@id='h-no-results']")).Text;
                            if (Nodata.Contains("No search results"))
                            {
                                HttpContext.Current.Session["Zero_Hillsborough"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }

                        }
                        catch { }
                        Thread.Sleep(3000);
                        string mul = driver.FindElement(By.XPath("//*[@id='results']/div[2]/div[1]/div/div[1]/div")).GetAttribute("innerText");
                        mul = WebDriverTest.After(mul, "Total Records:").Trim();
                        if ((mul != "1") && (mul != "0") && (mul != ""))
                        {   //multi parcel
                            IWebElement tbmulti2 = driver.FindElement(By.XPath("//*[@id='table-basic-results']/tbody"));
                            IList<IWebElement> TRmulti2 = tbmulti2.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDmulti2;
                            int k = 0;
                            foreach (IWebElement row in TRmulti2)
                            {
                                TDmulti2 = row.FindElements(By.TagName("td"));
                                if (k < 25 && TDmulti2[2].Text.Contains(address))
                                {
                                    if (TDmulti2.Count != 0 && TDmulti2[1].Text.Trim() != "")
                                    {
                                        Addressclick = TDmulti2[2];
                                    }
                                    k++;
                                }
                                string multi1 = TDmulti2[1].GetAttribute("innerText") + "~" + TDmulti2[2].GetAttribute("innerText");
                                gc.insert_date(orderNumber, TDmulti2[0].GetAttribute("innerText"), 325, multi1, 1, DateTime.Now);
                            }
                            if (k == 1)
                            {
                                Addressclick.Click();
                                Thread.Sleep(2000);
                            }
                            if (k < 25 && k > 1)
                            {
                                HttpContext.Current.Session["multiParcel_Hillsborough"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (TRmulti2.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Hillsborough_Multicount"] = "Maximum";
                                driver.Quit();
                                return "MultiParcel";
                            }

                        }

                        {
                            driver.FindElement(By.XPath("//*[@id='table-basic-results']/tbody/tr")).Click();
                            Thread.Sleep(3000);
                        }
                    }
                    if (searchType == "parcel")
                    {


                        driver.FindElement(By.XPath("//*[@id='basic']/div/label[2]/input")).Click();
                        driver.FindElement(By.XPath("//*[@id='basic']/label[1]/input[2]")).SendKeys(parcelNumber);
                        // driver.FindElement(By.XPath("//*[@id='basic']/label[1]/input[1]")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "FL", "Hillsborough");
                        driver.FindElement(By.XPath(" //*[@id='basic']/button[1]")).Click();
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search result", driver, "FL", "Hillsborough");
                        string mul = driver.FindElement(By.XPath("//*[@id='results']/div[2]/b[1]")).Text;
                        mul = WebDriverTest.After(mul, "Total Records:").Trim();
                        if ((mul != "1") && (mul != "0") && (mul != ""))
                        {   //multi parcel
                            IWebElement tbmulti2 = driver.FindElement(By.XPath("//*[@id='table-basic-results']/tbody"));
                            IList<IWebElement> TRmulti2 = tbmulti2.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDmulti2;
                            int k = 0;
                            foreach (IWebElement row in TRmulti2)
                            {
                                if (k < 25)
                                {
                                    TDmulti2 = row.FindElements(By.TagName("td"));
                                    if (TDmulti2.Count != 0 && TDmulti2[1].Text.Trim() != "")
                                    {
                                        string multi1 = TDmulti2[1].GetAttribute("innerText") + "~" + TDmulti2[2].GetAttribute("innerText");
                                        gc.insert_date(orderNumber, TDmulti2[0].GetAttribute("innerText"), 325, multi1, 1, DateTime.Now);
                                        //  Owner~address
                                    }
                                    k++;
                                }
                            }
                            HttpContext.Current.Session["multiParcel_Hillsborough"] = "Yes";
                            if (TRmulti2.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Hillsborough_Multicount"] = "Maximum";
                            }
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//*[@id='table-basic-results']/tbody/tr")).Click();
                                Thread.Sleep(3000);
                            }
                            catch { }
                        }
                        try
                        {
                            string Nodata = driver.FindElement(By.XPath("//*[@id='h-no-results']")).Text;
                            if (Nodata.Contains("No search results"))
                            {
                                HttpContext.Current.Session["Zero_Hillsborough"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }

                        }
                        catch { }
                    }
                    if (searchType == "block")
                    {

                        driver.FindElement(By.XPath("//*[@id='basic']/label[1]/input[1]")).SendKeys(unitno);
                        gc.CreatePdf_WOP(orderNumber, "folio Search", driver, "FL", "Hillsborough");
                        driver.FindElement(By.XPath("//*[@id='basic']/button[1]")).Click();
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "folio Search result", driver, "FL", "Hillsborough");

                        string mul = driver.FindElement(By.XPath("//*[@id='results']/div[2]/div[1]/div/div[1]/div")).GetAttribute("innerText");
                        mul = WebDriverTest.After(mul, "Total Records:").Trim();
                        if ((mul != "1") && (mul != "0") && (mul != ""))
                        {   //multi parcel
                            IWebElement tbmulti2 = driver.FindElement(By.XPath("//*[@id='table-basic-results']/tbody"));
                            IList<IWebElement> TRmulti2 = tbmulti2.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDmulti2;
                            int k = 0;
                            foreach (IWebElement row in TRmulti2)
                            {
                                if (k < 25)
                                {
                                    TDmulti2 = row.FindElements(By.TagName("td"));
                                    if (TDmulti2.Count != 0 && TDmulti2[1].Text.Trim() != "")
                                    {
                                        string multi1 = TDmulti2[1].GetAttribute("innerText") + "~" + TDmulti2[2].GetAttribute("innerText");
                                        gc.insert_date(orderNumber, TDmulti2[0].GetAttribute("innerText"), 325, multi1, 1, DateTime.Now);
                                        //  Owner~address
                                    }
                                    k++;
                                }
                            }

                            if (TRmulti2.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Hillsborough_Multicount"] = "Maximum";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiParcel_Hillsborough"] = "Yes";
                            }
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//*[@id='table-basic-results']/tbody/tr")).Click();
                                Thread.Sleep(3000);
                            }
                            catch { }
                        }
                        try
                        {
                            string Nodata = driver.FindElement(By.XPath("//*[@id='h-no-results']")).Text;
                            if (Nodata.Contains("No search results"))
                            {
                                HttpContext.Current.Session["Zero_Hillsborough"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }

                        }
                        catch { }

                    }
                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.XPath("//*[@id='basic']/label[2]/input[1]")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "Owner Search", driver, "FL", "Hillsborough");
                        driver.FindElement(By.XPath("//*[@id='basic']/button[1]")).Click();
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "owner Search result", driver, "FL", "Hillsborough");
                        string mul = driver.FindElement(By.XPath("//*[@id='results']/div[2]/b[1]")).Text;
                        mul = WebDriverTest.After(mul, "Total Records:").Trim();
                        if ((mul != "1") && (mul != "0") && (mul != ""))
                        {   //multi parcel
                            IWebElement tbmulti2 = driver.FindElement(By.XPath("//*[@id='table-basic-results']/tbody"));
                            IList<IWebElement> TRmulti2 = tbmulti2.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDmulti2;
                            int k = 0;
                            foreach (IWebElement row in TRmulti2)
                            {
                                if (k < 25)
                                {
                                    TDmulti2 = row.FindElements(By.TagName("td"));
                                    if (TDmulti2.Count != 0 && TDmulti2[1].Text.Trim() != "")
                                    {
                                        string multi1 = TDmulti2[1].Text + "~" + TDmulti2[2].Text;
                                        gc.insert_date(orderNumber, TDmulti2[0].Text, 325, multi1, 1, DateTime.Now);
                                        //  Owner~address
                                    }
                                    k++;
                                }
                            }
                            HttpContext.Current.Session["multiParcel_Hillsborough"] = "Yes";
                            if (TRmulti2.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Hillsborough_Multicount"] = "Maximum";
                            }
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else
                        {
                            driver.FindElement(By.XPath("//*[@id='table-basic-results']/tbody/tr")).Click();
                            Thread.Sleep(3000);
                        }
                    }

                    //property details
                    try
                    {
                        owner = driver.FindElement(By.XPath("//*[@id='details']/div[3]/div[2]/span[1]")).Text.Trim();
                        owner = owner.Replace("\r\n", " & ");
                        situs_address = driver.FindElement(By.XPath("//*[@id='details']/div[3]/div[2]/span[3]")).Text.Trim();

                    }
                    catch { }
                    try
                    {
                        owner = driver.FindElement(By.XPath("//*[@id='details']/div[3]/div[2]/p[1]")).GetAttribute("innerText").Trim();
                        owner = owner.Replace("\r\n", " & ");
                        situs_address = driver.FindElement(By.XPath("//*[@id='details']/div[3]/div[2]/p[3]")).GetAttribute("innerText").Trim();
                    }
                    catch { }
                    pin = driver.FindElement(By.XPath("//*[@id='details']/div[3]/div[2]/table/tbody/tr[1]/td[2]")).GetAttribute("innerText").Trim();
                    amck.TaxId = pin;
                    folio_parcel_no = driver.FindElement(By.XPath("//*[@id='details']/div[3]/div[2]/table/tbody")).GetAttribute("innerText").Trim();
                    folio_parcel_no = gc.Between(folio_parcel_no, "Folio:", "Prior PIN:").Trim();
                    folio_parcel_no = folio_parcel_no.Replace(" ", "").Replace("-", "");
                    // gc.CreatePdf(orderNumber, folio_parcel_no, "Property Search", driver, "FL", "Hillsborough");

                    string parcelnew = folio_parcel_no.TrimStart('0');
                    Tax_district = driver.FindElement(By.XPath(" //*[@id='details']/div[3]/div[2]/table/tbody/tr[5]/td[2]")).GetAttribute("innerText").Trim();
                    property_use = driver.FindElement(By.XPath(" //*[@id='details']/div[3]/div[2]/table/tbody/tr[6]/td[2]")).GetAttribute("innerText").Trim();
                    Neighbourhood = driver.FindElement(By.XPath("//*[@id='details']/div[3]/div[2]/table/tbody/tr[8]/td[2]")).GetAttribute("innerText").Trim();
                    subdivision = driver.FindElement(By.XPath("//*[@id='details']/div[3]/div[2]/table/tbody/tr[9]/td[2]")).GetAttribute("innerText").Trim();
                    try
                    {
                        Year_built = driver.FindElement(By.XPath(" //*[@id='details']/div[8]/div[4]/table[1]/tbody/tr[2]/td[2]")).GetAttribute("innerText").Trim();
                    }
                    catch
                    { }
                    try
                    {
                        if (Year_built == "")
                        {
                            Year_built = driver.FindElement(By.XPath("//*[@id='details']/div[8]/div[8]/table[1]/tbody/tr[2]/td[2]")).GetAttribute("innerText").Trim();
                        }
                    }

                    catch { }


                    Legal_desc = driver.FindElement(By.XPath("//*[@id='details']/div[12]/div[2]/table/tbody/tr/td[2]")).GetAttribute("innerText").Trim();
                    string property_details = owner + "~" + situs_address + "~" + pin + "~" + Tax_district + "~" + property_use + "~" + Neighbourhood + "~" + subdivision + "~" + Year_built + "~" + Legal_desc;
                    gc.insert_date(orderNumber, folio_parcel_no, 322, property_details, 1, DateTime.Now);
                    //     owner~situs_address~pin~Tax_district~property_use~Neighbourhood~subdivision~Year_built~Legal_desc

                    //Assessment details


                    IWebElement valuetableElement = driver.FindElement(By.XPath("//*[@id='details']/div[5]/div[1]/div[2]/table/tbody"));

                    IList<IWebElement> valuetableRow = valuetableElement.FindElements(By.TagName("tr"));
                    IList<IWebElement> valuerowTD;
                    List<string> County_marketValue = new List<string>();
                    List<string> County_AssessedValue = new List<string>();
                    List<string> County_Exemptions = new List<string>();
                    List<string> County_TaxableValue = new List<string>();

                    List<string> PublicSchools_marketValue = new List<string>();
                    List<string> PublicSchools_AssessedValue = new List<string>();
                    List<string> PublicSchools_Exemptions = new List<string>();
                    List<string> PublicSchools_TaxableValue = new List<string>();

                    List<string> Municipal_marketValue = new List<string>();
                    List<string> Municipal_AssessedValue = new List<string>();
                    List<string> Municipal_Exemptions = new List<string>();
                    List<string> Municipal_TaxableValue = new List<string>();

                    List<string> Other_Districts_marketValue = new List<string>();
                    List<string> Other_Districts_AssessedValue = new List<string>();
                    List<string> Other_Districts_Exemptions = new List<string>();
                    List<string> Other_Districts_TaxableValue = new List<string>();
                    int i = 0;
                    foreach (IWebElement row in valuetableRow)
                    {
                        valuerowTD = row.FindElements(By.TagName("td"));
                        if (valuerowTD.Count != 0)
                        {
                            if (i == 0)
                            {

                                County_marketValue.Add(valuerowTD[1].GetAttribute("innerText"));
                                County_AssessedValue.Add(valuerowTD[2].GetAttribute("innerText"));
                                County_Exemptions.Add(valuerowTD[3].GetAttribute("innerText"));
                                County_TaxableValue.Add(valuerowTD[4].GetAttribute("innerText"));

                            }
                            else if (i == 1)
                            {
                                PublicSchools_marketValue.Add(valuerowTD[1].GetAttribute("innerText"));
                                PublicSchools_AssessedValue.Add(valuerowTD[2].GetAttribute("innerText"));
                                PublicSchools_Exemptions.Add(valuerowTD[3].GetAttribute("innerText"));
                                PublicSchools_TaxableValue.Add(valuerowTD[4].GetAttribute("innerText"));
                            }

                            else if (i == 2)
                            {
                                Municipal_marketValue.Add(valuerowTD[1].GetAttribute("innerText"));
                                Municipal_AssessedValue.Add(valuerowTD[2].GetAttribute("innerText"));
                                Municipal_Exemptions.Add(valuerowTD[3].GetAttribute("innerText"));
                                Municipal_TaxableValue.Add(valuerowTD[4].GetAttribute("innerText"));
                            }
                            else if (i == 3)
                            {
                                Other_Districts_marketValue.Add(valuerowTD[1].GetAttribute("innerText"));
                                Other_Districts_AssessedValue.Add(valuerowTD[2].GetAttribute("innerText"));
                                Other_Districts_Exemptions.Add(valuerowTD[3].GetAttribute("innerText"));
                                Other_Districts_TaxableValue.Add(valuerowTD[4].GetAttribute("innerText"));
                            }


                        }
                        i++;
                    }
                    //  County_marketValue~County_AssessedValue~County_Exemptions~County_TaxableValue~PublicSchools_marketValue~PublicSchools_AssessedValue~PublicSchools_Exemptions~PublicSchools_TaxableValue~Municipal_marketValue~Municipal_AssessedValue~Municipal_Exemptions~Municipal_TaxableValue~Other_Districts_marketValue~Other_Districts_AssessedValue~Other_Districts_Exemptions~Other_Districts_TaxableValue
                    string assessment_details = County_marketValue[0] + "~" + County_AssessedValue[0] + "~" + County_Exemptions[0] + "~" + County_TaxableValue[0] + "~" + PublicSchools_marketValue[0] + "~" + PublicSchools_AssessedValue[0] + "~" + PublicSchools_Exemptions[0] + "~" + PublicSchools_TaxableValue[0] + "~" + Municipal_marketValue[0] + "~" + Municipal_AssessedValue[0] + "~" + Municipal_Exemptions[0] + "~" + Municipal_TaxableValue[0] + "~" + Other_Districts_marketValue[0] + "~" + Other_Districts_AssessedValue[0] + "~" + Other_Districts_Exemptions[0] + "~" + Other_Districts_TaxableValue[0];


                    gc.insert_date(orderNumber, folio_parcel_no, 323, assessment_details, 1, DateTime.Now);

                    gc.CreatePdf(orderNumber, folio_parcel_no, "Assessment details1", driver, "FL", "Hillsborough");


                    ByVisibleElement(driver.FindElement(By.XPath("//*[@id='details']/div[5]/div[1]/div[2]")));
                    gc.CreatePdf(orderNumber, folio_parcel_no, "Assessment details2", driver, "FL", "Hillsborough");
                    try
                    {
                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='details']/div[7]/div[2]/table/thead/tr[1]/th[3]/a")));
                        gc.CreatePdf(orderNumber, folio_parcel_no, "Assessment details3", driver, "FL", "Hillsborough");
                    }
                    catch { }
                    try
                    {
                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='details']/div[8]/div[4]/table[1]/tbody/tr[1]/td[1]/a")));
                        gc.CreatePdf(orderNumber, folio_parcel_no, "Assessment details4", driver, "FL", "Hillsborough");
                    }
                    catch { }
                    try
                    {
                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='details']/div[8]/div[4]/div/table/thead/tr/th[1]/a")));
                        gc.CreatePdf(orderNumber, folio_parcel_no, "Assessment details5", driver, "FL", "Hillsborough");
                    }
                    catch { }

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("https://hillsborough.county-taxes.com/public");
                    Thread.Sleep(5000);
                    driver.FindElement(By.XPath("//*[@id='search-controls']/div/form[1]/div[1]/div/input")).SendKeys(parcelnew);
                    gc.CreatePdf(orderNumber, folio_parcel_no, "Taxinfo", driver, "FL", "Hillsborough");
                    driver.FindElement(By.XPath("//*[@id='search-controls']/div/form[1]/div[1]/div/span/button")).SendKeys(Keys.Enter);
                    Thread.Sleep(3000);
                    try
                    {
                        IWebElement ITaxSearch = driver.FindElement(By.LinkText("Full bill history"));
                        string strITaxSearch = ITaxSearch.GetAttribute("href");
                        driver.Navigate().GoToUrl(strITaxSearch);
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, folio_parcel_no, "Full bill history", driver, "FL", "Hillsborough");
                    }
                    catch { }

                    try
                    {
                        IWebElement ITaxReal = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table"));
                        IList<IWebElement> ITaxRealRow = ITaxReal.FindElements(By.TagName("tr"));
                        IList<IWebElement> ITaxRealTd;
                        int k = 0;
                        foreach (IWebElement ItaxReal in ITaxRealRow)
                        {


                            ITaxRealTd = ItaxReal.FindElements(By.TagName("td"));
                            if ((k <= 2 && ItaxReal.Text.Contains("Annual Bill")) || ItaxReal.Text.Contains("Pay this bill:"))
                            {

                                string yearbill = ITaxRealTd[0].Text;
                                IWebElement ITaxBillCount = ITaxRealTd[0].FindElement(By.TagName("a"));
                                string strTaxReal = ITaxBillCount.GetAttribute("href");
                                strTaxRealestate.Add(strTaxReal);
                                try
                                {
                                    IWebElement ITaxBill = ITaxRealTd[3].FindElement(By.TagName("a"));
                                    string BillTax = ITaxBill.GetAttribute("href");
                                    gc.downloadfile(BillTax, orderNumber, folio_parcel_no, "Taxbill.pdf" + yearbill, "FL", "Hillsborough");
                                }
                                catch
                                {
                                    IWebElement ITaxBill = ITaxRealTd[4].FindElement(By.TagName("a"));
                                    string BillTax = ITaxBill.GetAttribute("href");
                                    gc.downloadfile(BillTax, orderNumber, folio_parcel_no, "Taxbill.pdf" + yearbill, "FL", "Hillsborough");

                                }
                                k++;
                            }

                        }


                        //Tax History Details

                        IWebElement IBillHistorytable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table"));
                        IList<IWebElement> IBillHistoryRow = IBillHistorytable.FindElements(By.TagName("tr"));
                        IList<IWebElement> IBillHistoryTD;
                        foreach (IWebElement bill in IBillHistoryRow)
                        {
                            IBillHistoryTD = bill.FindElements(By.TagName("td"));
                            if (IBillHistoryTD.Count != 0)
                            {
                                try
                                {
                                    strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                    strBalance = IBillHistoryTD[1].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                    strBillDate = IBillHistoryTD[2].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                    strBillPaid = IBillHistoryTD[3].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                }
                                catch
                                {
                                    strBillDate = "";
                                    strBillPaid = "";
                                }
                                if (strBillPaid.Contains("Print (PDF)"))
                                {
                                    strBillPaid = "";
                                }
                                string strTaxHistory = strBill + "~" + strBalance + "~" + strBillDate + "~" + strBillPaid;
                                gc.insert_date(orderNumber, folio_parcel_no, 327, strTaxHistory, 1, DateTime.Now);
                            }
                        }
                        IWebElement IBillHistoryfoottable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table/tfoot"));
                        IList<IWebElement> IBillHistoryfootRow = IBillHistoryfoottable.FindElements(By.TagName("tr"));
                        IList<IWebElement> IBillHistoryfootTD;
                        foreach (IWebElement bill in IBillHistoryRow)
                        {
                            IBillHistoryfootTD = bill.FindElements(By.TagName("th"));
                            if (IBillHistoryfootTD.Count != 0 && bill.Text.Contains("Total"))
                            {
                                try
                                {
                                    strFBill = IBillHistoryfootTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                    strFBalance = IBillHistoryfootTD[1].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                    strFBillDate = IBillHistoryfootTD[2].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                    strFBillPaid = IBillHistoryfootTD[3].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                }
                                catch
                                {
                                    strFBillDate = "";
                                    strFBillPaid = "";
                                }
                                if (strBillPaid.Contains("Print (PDF)"))
                                {
                                    strBillPaid = "";
                                }
                                string strTaxHistory = strFBill + "~" + strFBalance + "~" + strFBillDate + "~" + strFBillPaid;
                                gc.insert_date(orderNumber, folio_parcel_no, 327, strTaxHistory, 1, DateTime.Now);
                            }
                        }

                        { }
                        foreach (string real in strTaxRealestate)
                        {
                            driver.Navigate().GoToUrl(real);
                            Thread.Sleep(4000);
                            string Billtype = "";
                            try
                            {
                                TaxYear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[1]")).Text;
                                TaxYear = WebDriverTest.After(TaxYear, "Real Estate").Trim();
                                string s = TaxYear;
                                string[] words = TaxYear.Split(' ');
                                TaxYear = words[0];
                                Billtype = words[1];
                            }
                            catch
                            {
                                TaxYear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[1]")).Text;
                                TaxYear = WebDriverTest.After(TaxYear, "Real Estate").Trim();
                                string s = TaxYear;
                                string[] words = TaxYear.Split(' ');
                                TaxYear = words[0];
                                Billtype = words[1];
                            }
                            amck.TaxYear = TaxYear;
                            // amck.TaxType = Billtype;
                            gc.CreatePdf(orderNumber, folio_parcel_no, "Tax details" + TaxYear, driver, "FL", "Hillsborough");
                            IWebElement multitableElement3;
                            try
                            {
                                multitableElement3 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[2]/tbody"));
                            }
                            catch
                            {
                                multitableElement3 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[2]/tbody"));
                            }
                            IList<IWebElement> multitableRow3 = multitableElement3.FindElements(By.TagName("tr"));

                            IList<IWebElement> multirowTD3;
                            foreach (IWebElement row in multitableRow3)
                            {
                                multirowTD3 = row.FindElements(By.TagName("td"));
                                if (multirowTD3.Count != 1 && multirowTD3[1].Text.Trim() != "")
                                {
                                    string tax_distri = TaxYear + "~" + multirowTD3[0].Text.Trim() + "~" + "Ad Valorem" + "~" + multirowTD3[1].Text.Trim() + "~" + multirowTD3[2].Text.Trim() + "~" + multirowTD3[3].Text.Trim() + "~" + multirowTD3[4].Text.Trim() + "~" + "" + "~" + multirowTD3[5].Text.Trim();
                                    gc.insert_date(orderNumber, folio_parcel_no, 324, tax_distri, 1, DateTime.Now);
                                }
                            }

                            //total advalorem
                            IWebElement multitableElement31;
                            try
                            {
                                multitableElement31 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[2]/tfoot"));
                            }
                            catch
                            {

                                multitableElement31 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[2]/tfoot"));
                            }
                            IList<IWebElement> multitableRow31 = multitableElement31.FindElements(By.TagName("tr"));

                            IList<IWebElement> multirowTD31;
                            foreach (IWebElement row in multitableRow31)
                            {
                                multirowTD31 = row.FindElements(By.TagName("td"));
                                if (multirowTD31.Count != 1)
                                {
                                    string tax_distri1 = TaxYear + "~" + "Total:" + "~" + "Ad Valorem" + "~" + multirowTD31[0].Text.Trim() + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + multirowTD31[1].Text.Trim();
                                    gc.insert_date(orderNumber, folio_parcel_no, 324, tax_distri1, 1, DateTime.Now);
                                }
                            }
                            //  Non - Ad Valorem                    
                            try
                            {
                                IWebElement multitableElement32;
                                try
                                {
                                    multitableElement32 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[3]/tbody"));
                                }
                                catch
                                {
                                    multitableElement32 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[3]/tbody"));

                                }
                                IList<IWebElement> multitableRow32 = multitableElement32.FindElements(By.TagName("tr"));
                                IList<IWebElement> multirowTD32;
                                foreach (IWebElement row in multitableRow32)
                                {
                                    multirowTD32 = row.FindElements(By.TagName("td"));
                                    if (multirowTD32.Count != 1 && multirowTD32[0].Text.Trim() != "")
                                    {
                                        string tax_distri2 = TaxYear + "~" + multirowTD32[0].Text.Trim() + "~" + "Non-Ad Valorem " + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + multirowTD32[1].Text.Trim() + "~" + multirowTD32[2].Text.Trim();
                                        gc.insert_date(orderNumber, folio_parcel_no, 324, tax_distri2, 1, DateTime.Now);
                                    }
                                }
                                //total non-advalorem

                                IWebElement multitableElement33;
                                try
                                {

                                    multitableElement33 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[3]/tfoot"));
                                }
                                catch
                                {
                                    multitableElement33 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[3]/tfoot"));
                                }
                                IList<IWebElement> multitableRow33 = multitableElement33.FindElements(By.TagName("tr"));
                                IList<IWebElement> multirowTD33;
                                foreach (IWebElement row in multitableRow33)
                                {
                                    multirowTD33 = row.FindElements(By.TagName("td"));
                                    if (multirowTD33.Count != 0)
                                    {
                                        string tax_distri1 = TaxYear + "~" + "Total:" + "~" + "Non-Ad Valorem " + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + multirowTD33[0].Text.Trim();
                                        gc.insert_date(orderNumber, folio_parcel_no, 324, tax_distri1, 1, DateTime.Now);
                                    }
                                }
                            }
                            catch { }
                            //  Taxing_authority~Tax_type~Millage~Assessed~Exemption~Taxable~Rate~Tax
                            //Tax info                     
                            try
                            {
                                
                                TaxAmount = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/p")).Text.Replace("Combined taxes and assessments:", "");
                                PaidAmount = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[6]/div/div[1]/div/div")).Text;
                                string[] status1 = PaidAmount.Split(' ');
                                string Status = status1[0];

                                PaidAmount = PaidAmount.Replace("\r\n", "&");
                                PaidAmount = WebDriverTest.Before(PaidAmount, "&");
                                PaidAmount = WebDriverTest.After(PaidAmount, "$").Trim();
                                if (Status.Trim().ToUpper() == "PAID")
                                {
                                    amck.IsDelinquent = "No";
                                    amck.InstPaidDue1 = Status;
                                }
                                else
                                {
                                    if (Convert.ToDouble(TaxAmount) < Convert.ToDouble(PaidAmount))
                                    {
                                        amck.IsDelinquent = "Yes";
                                        amck.InstPaidDue1 = "Due";
                                    }
                                    if (Convert.ToDouble(TaxAmount) == Convert.ToDouble(PaidAmount))
                                    {
                                        amck.IsDelinquent = "No";
                                        amck.InstPaidDue1 = "Due";
                                    }
                                    if (Convert.ToDouble(TaxAmount) > Convert.ToDouble(PaidAmount))
                                    {
                                        amck.IsDelinquent = "No";
                                        amck.InstPaidDue1 = "Due";
                                    }
                                }
                                
                                amck.Instamount1 = PaidAmount;
                                amck.Instamountpaid1 = TaxAmount;
                                ReceiptNumber = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[6]/div/div[1]/div/div/div")).Text.Replace("Receipt", "");
                                IWebElement tbmulti = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[1]/tbody"));
                                IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));
                                IList<IWebElement> TDmulti;
                                foreach (IWebElement row in TRmulti)
                                {
                                    TDmulti = row.FindElements(By.TagName("td"));
                                    if (TDmulti.Count != 0)
                                    {
                                        Account_number = TDmulti[0].Text;
                                        string Escro_Code = TDmulti[2].Text;
                                        Millage_Code = TDmulti[3].Text;

                                        string tax_info1 = Account_number + "~" + Millage_Code + "~" + Escro_Code + "~" + TaxYear + "~" + TaxAmount + "~" + PaidAmount + "~" + ReceiptNumber + "~" + "Doug Belden, Tax Collector P.O. Box 30012 Tampa, Florida 33630-3012";

                                        gc.insert_date(orderNumber, folio_parcel_no, 326, tax_info1, 1, DateTime.Now);
                                    }
                                }
                            }
                            catch
                            {}
                            try
                            {
                                TaxAmount = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/p")).Text.Replace("Combined taxes and assessments:", "");
                                PaidAmount = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[6]/div/form/button")).Text;
                                //string[] status1 = PaidAmount.Split(' ');
                                //string Status = status1[0];
                                //if (Status.Trim().ToUpper() == "PAID")
                                //{
                                //    amck.IsDelinquent = "No";
                                //}
                                //else
                                //{
                                //    amck.IsDelinquent = "Yes";
                                //}
                                if (PaidAmount.Contains("Pay this bill"))
                                {
                                    PaidAmount = WebDriverTest.After(PaidAmount.Replace("$",""), ":").Trim();
                                    // double Amount1 = Convert.ToInt16(TaxAmount.Replace("$", ""));
                                    string Amount1 = TaxAmount.Replace("$", "");

                                    if (Convert.ToDouble(Amount1.Trim()) > Convert.ToDouble(PaidAmount.Trim()))
                                    {
                                        amck.IsDelinquent = "Yes";
                                        amck.InstPaidDue1 = "Due";
                                    }
                                    if (Convert.ToDouble(Amount1.Trim()) == Convert.ToDouble(PaidAmount.Trim()))
                                    {
                                        amck.IsDelinquent = "No";
                                        amck.InstPaidDue1 = "Due";
                                    }
                                    if (Convert.ToDouble(Amount1.Trim())< Convert.ToDouble(PaidAmount.Trim()))
                                    {
                                        amck.IsDelinquent = "No";
                                        amck.InstPaidDue1 = "Due";
                                    }
                                }
                                else
                                {
                                    PaidAmount = PaidAmount.Replace("\r\n", "&");
                                    var PaidAmount1 = PaidAmount.Split('&');
                                    PaidAmount = PaidAmount1[0];
                                    PaidAmount = WebDriverTest.After(PaidAmount, "$").Trim();
                                    ReceiptNumber = PaidAmount1[1];

                                }
                                IWebElement tbmulti = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[1]/tbody"));
                                IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));

                                IList<IWebElement> TDmulti;
                                foreach (IWebElement row in TRmulti)
                                {
                                    TDmulti = row.FindElements(By.TagName("td"));
                                    if (TDmulti.Count != 0)
                                    {
                                        Account_number = TDmulti[0].Text;
                                        string Escro_Code = TDmulti[2].Text;
                                        Millage_Code = TDmulti[3].Text;
                                        string tax_info1 = Account_number + "~" + Millage_Code + "~" + Escro_Code + "~" + TaxYear + "~" + TaxAmount + "~" + PaidAmount + "~" + ReceiptNumber + "~" + "Doug Belden, Tax Collector P.O. Box 30012 Tampa, Florida 33630-3012";
                                        gc.insert_date(orderNumber, folio_parcel_no, 326, tax_info1, 1, DateTime.Now);
                                    }
                                }
                            }
                            catch { }
                            string IfPaidBy = "", PlesePay = "", DueDate = "", deli = "";
                            try
                            {

                                IWebElement multitableElement26 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]/tbody"));
                                IList<IWebElement> multitableRow26 = multitableElement26.FindElements(By.TagName("tr"));

                                IList<IWebElement> multirowTD26;
                                foreach (IWebElement row in multitableRow26)
                                {

                                    multirowTD26 = row.FindElements(By.TagName("td"));
                                    int iRowsCount = multirowTD26.Count;
                                    for (int n = 0; n < iRowsCount; n++)
                                    {
                                        if ((!multirowTD26[n].Text.Contains("Face Amt")) && (!multirowTD26[n].Text.Contains("Certificate")))
                                        {

                                            IfPaidBy = multirowTD26[n].Text.Replace("\r\n", "~");
                                            var IfpaySplit = IfPaidBy.Split('~');
                                            IfPaidBy = IfpaySplit[0];
                                            PlesePay = IfpaySplit[1];
                                            if (PlesePay == "$0.00")
                                                deli = "paid";
                                            else
                                                deli = "deliquent";
                                            DueDate = TaxYear + "~" + deli + "~" + IfPaidBy + "~" + PlesePay;
                                            gc.insert_date(orderNumber, folio_parcel_no, 328, DueDate, 1, DateTime.Now);

                                        }

                                    }
                                }
                            }
                            //If_paid_by~Please_Pay
                            catch
                            {
                                IWebElement multitableElement26 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]/tbody"));
                                IList<IWebElement> multitableRow26 = multitableElement26.FindElements(By.TagName("tr"));

                                IList<IWebElement> multirowTD26;
                                foreach (IWebElement row in multitableRow26)
                                {
                                    multirowTD26 = row.FindElements(By.TagName("td"));
                                    int iRowsCount = multirowTD26.Count;
                                    for (int n = 0; n < iRowsCount; n++)
                                    {
                                        if ((!multirowTD26[n].Text.Contains("Face Amt")) && (!multirowTD26[n].Text.Contains("Certificate")))
                                        {

                                            IfPaidBy = multirowTD26[n].Text.Replace("\r\n", "~");
                                            var IfpaySplit = IfPaidBy.Split('~');
                                            IfPaidBy = IfpaySplit[0];
                                            PlesePay = IfpaySplit[1];
                                            if (PlesePay == "$0.00")
                                                deli = "paid";
                                            else
                                                deli = "deliquent";
                                            DueDate = TaxYear + "~" + deli + "~" + IfPaidBy + "~" + PlesePay;
                                            gc.insert_date(orderNumber, folio_parcel_no, 328, DueDate, 1, DateTime.Now);

                                        }

                                    }


                                }


                            }

                            //*[@id="content"]/div[1]/div[3]/div[1]/ul/li[1]/a
                            driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[3]/div[1]/ul/li[1]/a")).Click();
                            Thread.Sleep(5000);
                            gc.CreatePdf(orderNumber, folio_parcel_no, "parcel details" + TaxYear, driver, "FL", "Hillsborough");
                            if (I == 0)
                            {
                                if (amck.IsDelinquent!="Yes")
                                { 
                                gc.InsertAmrockTax(orderNumber, amck.TaxId, amck.Instamount1, amck.Instamount2, amck.Instamount3, amck.Instamount4, amck.Instamountpaid1, amck.Instamountpaid2, amck.Instamountpaid3, amck.Instamountpaid4, amck.InstPaidDue1, amck.InstPaidDue2, amck.instPaidDue3, amck.instPaidDue4, amck.IsDelinquent);
                                I++;
                                }
                                else
                                {
                                    gc.InsertAmrockTax(orderNumber, amck.TaxId, null, null, null, null, null, null, null, null, null, null, null, null, amck.IsDelinquent);
                                }
                            }
                        }
                    }
                    catch { }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "FL", "Hillsborough", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "FL", "Hillsborough");
                    return "Data Inserted Successfully";
                }
                catch (Exception ex)
                {
                    driver.Quit();
                    throw ex;
                }
            }
        }
        public Bitmap GetEntereScreenshot()
        {

            Bitmap stitchedImage = null;
            try
            {
                long totalwidth1 = (long)((IJavaScriptExecutor)driver).ExecuteScript("return document.body.offsetWidth");//documentElement.scrollWidth");

                long totalHeight1 = (long)((IJavaScriptExecutor)driver).ExecuteScript("return  document.body.parentNode.scrollHeight");

                int totalWidth = (int)totalwidth1;
                int totalHeight = (int)totalHeight1;

                // Get the Size of the Viewport
                long viewportWidth1 = (long)((IJavaScriptExecutor)driver).ExecuteScript("return document.body.clientWidth");//documentElement.scrollWidth");
                long viewportHeight1 = (long)((IJavaScriptExecutor)driver).ExecuteScript("return window.innerHeight");//documentElement.scrollWidth");

                int viewportWidth = (int)viewportWidth1;
                int viewportHeight = (int)viewportHeight1;


                // Split the Screen in multiple Rectangles
                List<System.Drawing.Rectangle> rectangles = new List<System.Drawing.Rectangle>();
                // Loop until the Total Height is reached
                for (int i = 0; i < totalHeight; i += viewportHeight)
                {
                    int newHeight = viewportHeight;
                    // Fix if the Height of the Element is too big
                    if (i + viewportHeight > totalHeight)
                    {
                        newHeight = totalHeight - i;
                    }
                    // Loop until the Total Width is reached
                    for (int ii = 0; ii < totalWidth; ii += viewportWidth)
                    {
                        int newWidth = viewportWidth;
                        // Fix if the Width of the Element is too big
                        if (ii + viewportWidth > totalWidth)
                        {
                            newWidth = totalWidth - ii;
                        }

                        // Create and add the Rectangle
                        System.Drawing.Rectangle currRect = new System.Drawing.Rectangle(ii, i, newWidth, newHeight);
                        rectangles.Add(currRect);
                    }
                }

                // Build the Image
                stitchedImage = new Bitmap(totalWidth, totalHeight);
                // Get all Screenshots and stitch them together
                System.Drawing.Rectangle previous = System.Drawing.Rectangle.Empty;
                foreach (var rectangle in rectangles)
                {
                    // Calculate the Scrolling (if needed)
                    if (previous != System.Drawing.Rectangle.Empty)
                    {
                        int xDiff = rectangle.Right - previous.Right;
                        int yDiff = rectangle.Bottom - previous.Bottom;
                        // Scroll
                        //selenium.RunScript(String.Format("window.scrollBy({0}, {1})", xDiff, yDiff));
                        ((IJavaScriptExecutor)driver).ExecuteScript(String.Format("window.scrollBy({0}, {1})", xDiff, yDiff));
                        System.Threading.Thread.Sleep(200);
                    }

                    // Take Screenshot
                    var screenshot = ((ITakesScreenshot)driver).GetScreenshot();

                    // Build an Image out of the Screenshot
                    System.Drawing.Image screenshotImage;
                    using (MemoryStream memStream = new MemoryStream(screenshot.AsByteArray))
                    {
                        screenshotImage = System.Drawing.Image.FromStream(memStream);
                    }

                    // Calculate the Source Rectangle
                    System.Drawing.Rectangle sourceRectangle = new System.Drawing.Rectangle(viewportWidth - rectangle.Width, viewportHeight - rectangle.Height, rectangle.Width, rectangle.Height);

                    // Copy the Image
                    using (Graphics g = Graphics.FromImage(stitchedImage))
                    {
                        g.DrawImage(screenshotImage, rectangle, sourceRectangle, GraphicsUnit.Pixel);
                    }

                    // Set the Previous Rectangle
                    previous = rectangle;
                }
            }
            catch (Exception ex)
            {
                // handle
            }
            return stitchedImage;
        }

        public void ByVisibleElement(IWebElement Element)
        {

            //driver = new ChromeDriver();
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

            //Launch the application		
            // driver.Navigate().GoToUrl("http://gis.hcpafl.org/PropertySearch/#/nav/Basic%20Search");

            //Find element by link text and store in variable "Element"        		
            // IWebElement Element = driver.FindElement(By.LinkText("Disclaimer"));

            //This will scroll the page till the element is found		
            js.ExecuteScript("arguments[0].scrollIntoView();", Element);
        }


    }
}

