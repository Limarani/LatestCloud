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
using System.Text;
using OpenQA.Selenium.Firefox;

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_GAClayton
    {
        string outputPath = ""; int searchcount1; int searchcount;
        IWebDriver driver;
        DBconnection db = new DBconnection();

        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_GAClayton(string houseno, string sname, string unitno, string parcelNumber, string searchType, string orderNumber, string ownername, string directParcel)
        {
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "", AssessTakenTime = "", TaxTakentime = "", CityTaxtakentime = "";
            string TotaltakenTime = "";
            string OwnerName = "", JointOwnerName = "", PropertyAddress = "", MailingAddress = "", Municipality = "", PropertyUse = "", YearBuilt = "", LegalDescription = "", parcel_id = "";
            List<string> strTaxRealestate = new List<string>();
            List<string> strTaxRealestate1 = new List<string>();
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            //IWebElement iframeElement1;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    if (searchType == "titleflex")
                    {
                        string address = houseno + " " + sname + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "GA", "Clayton");

                        if (HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes")
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_GAClayton"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    driver.Navigate().GoToUrl("https://www.claytoncountyga.gov/government/tax-assessor/property-search-information/real-property-records-search");
                    Thread.Sleep(3000);
                    if (searchType == "address")
                    {
                        IWebElement frame12 = driver.FindElement(By.XPath("//*[@id='Clayton County']"));
                        driver.SwitchTo().Frame(frame12);
                        driver.FindElement(By.Name("StreetName")).SendKeys(sname);
                        driver.FindElement(By.Id("qLocn")).SendKeys(houseno);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "GA", "Clayton");
                        driver.FindElement(By.Id("btnSrchAddress")).SendKeys(Keys.Enter);
                        Thread.Sleep(4000);
                        gc.CreatePdf_WOP(orderNumber, "Address search result", driver, "GA", "Clayton");
                        afterclick(houseno, sname);
                        if (searchcount1 == 0)
                        {
                            driver.FindElement(By.LinkText("NEXT")).Click();
                            Thread.Sleep(4000);
                            afterclick(houseno, sname);
                            if (searchcount1 == 0)
                            {
                                driver.FindElement(By.LinkText("NEXT")).Click();
                                Thread.Sleep(4000);
                                afterclick(houseno, sname);
                            }
                        }

                    }
                    if (searchType == "parcel")
                    {
                        HttpContext.Current.Session["parcel_GAClayton"] = "Yes";
                        driver.Quit();
                        return "No parcel Search";
                    }
                    if (searchType == "ownername")
                    {
                        HttpContext.Current.Session["owner_GAClayton"] = "Yes";
                        driver.Quit();
                        return "No ownername Search";
                    }
                    //property_details

                    string location = "", district = "", county = "", totalparcel = "", landovr = "", improvementovr = "", value1 = "", value2 = "", comments = "";
                    parcel_id = driver.FindElement(By.XPath("//*[@id='content']/table/tbody/tr/td/table[1]/tbody/tr[6]/td[2]")).Text.Trim().Replace("PARCEL ID . . ", "").Replace("-", "");

                    gc.CreatePdf(orderNumber, parcel_id, "property details", driver, "GA", "Clayton");

                    location = driver.FindElement(By.XPath("//*[@id='content']/table/tbody/tr/td/table[1]/tbody/tr[7]/td[2]")).Text.Trim().Replace("LOCATION . . ", "");
                    district = driver.FindElement(By.XPath("//*[@id='content']/table/tbody/tr/td/table[2]/tbody/tr[1]/td[4]")).Text.Trim().Replace("DISTRICT", "");
                    county = driver.FindElement(By.XPath("//*[@id='content']/table/tbody/tr/td/table[2]/tbody/tr[1]/td[5]")).Text.Trim();
                    if (county.Contains("COUNTY"))
                    {
                        county = county.Replace("COUNTY - ", "");
                    }
                    if (county.Contains("CONELY") || county.Contains("FOREST PARK") || county.Contains("JONESBORO") || county.Contains("LOVEJOY") || county.Contains("MORROW") || county.Contains("REX") || county.Contains("RIVERDALE"))
                    {
                        comments = "Please call to specific city tax collector";
                    }
                    else
                    {
                        comments = "Non City Tax";
                    }
                    string owner1 = driver.FindElement(By.XPath("//*[@id='content']/table/tbody/tr/td/table[1]/tbody/tr[6]/td[1]")).Text.Trim();
                    string owner2 = driver.FindElement(By.XPath("//*[@id='content']/table/tbody/tr/td/table[1]/tbody/tr[7]/td[1]")).Text.Trim();
                    string owner3 = driver.FindElement(By.XPath("//*[@id='content']/table/tbody/tr/td/table[1]/tbody/tr[8]/td[1]")).Text.Trim();
                    OwnerName = owner1 + " " + owner2 + " " + owner3;
                    string Legal1 = driver.FindElement(By.XPath("//*[@id='content']/table/tbody/tr/td/table[2]/tbody/tr[1]/td[3]")).Text.Trim();
                    string Legal2 = driver.FindElement(By.XPath("//*[@id='content']/table/tbody/tr/td/table[2]/tbody/tr[2]/td[3]")).Text.Trim();
                    string Legal3 = driver.FindElement(By.XPath("//*[@id='content']/table/tbody/tr/td/table[2]/tbody/tr[3]/td[3]")).Text.Trim();
                    string Legal4 = driver.FindElement(By.XPath("//*[@id='content']/table/tbody/tr/td/table[2]/tbody/tr[3]/td[5]")).Text.Trim();
                    LegalDescription = Legal1 + " " + Legal2 + " " + Legal3 + " " + Legal4;
                    //        Location~Legal Description~District~County~Owner Name~Total Parcel Values~Comments

                    //    assessment details


                    try
                    {
                        //int c = 0;//*[@id="content"]/table/tbody/tr/td/table[9]/tbody
                        for (int c = 1; c < 10; c++)
                        {
                            try
                            {
                                IWebElement tables1 = driver.FindElement(By.XPath("//*[@id='content']/table/tbody/tr/td/table[" + c + "]/tbody"));
                                IList<IWebElement> ITaxRealRowQ1 = tables1.FindElements(By.TagName("tr"));
                                IList<IWebElement> ITaxRealTdQ1;
                                if (tables1.Text.Contains("TOTAL PARCEL VALUES"))
                                {
                                    foreach (IWebElement ItaxReal1 in ITaxRealRowQ1)
                                    {
                                        ITaxRealTdQ1 = ItaxReal1.FindElements(By.TagName("td"));
                                        if (ITaxRealTdQ1.Count == 7 && !ItaxReal1.Text.Contains("TOTAL PARCEL VALUES"))
                                        {
                                            try
                                            {
                                                totalparcel = ITaxRealTdQ1[0].Text;
                                                landovr = ITaxRealTdQ1[1].Text;
                                                improvementovr = ITaxRealTdQ1[3].Text;
                                                value1 = ITaxRealTdQ1[5].Text;
                                                value2 = ITaxRealTdQ1[6].Text;
                                            }
                                            catch { }
                                        }
                                    }
                                }

                            }
                            catch { }
                        }

                    }
                    catch (Exception e)
                    {

                    }


                    string property_details = location + "~" + LegalDescription + "~" + district + "~" + county + "~" + OwnerName + "~" + totalparcel + "~" + comments;
                    gc.insert_date(orderNumber, parcel_id, 490, property_details, 1, DateTime.Now);
                    string assessment_details = landovr + "~" + improvementovr + "~" + value1 + "~" + value2;
                    gc.insert_date(orderNumber, parcel_id, 491, assessment_details, 1, DateTime.Now);
                    //Land / OVR~Improvements / OVR~Current Year Value~Prior Year Value

                    IWebElement element = driver.FindElement(By.XPath("//*[@id='content']/table/tbody/tr/td/table[1]/tbody/tr[2]/td/a[3]"));
                    IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                    js.ExecuteScript("arguments[0].click();", element);
                    Thread.Sleep(6000);
                    gc.CreatePdf(orderNumber, parcel_id, "sales data", driver, "GA", "Clayton");

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("http://weba.co.clayton.ga.us/tcmsvr/htdocs/indextcm.shtml");

                    Thread.Sleep(4000);
                    driver.FindElement(By.Name("StreetName")).SendKeys(sname);
                    driver.FindElement(By.Id("qLocn")).SendKeys(houseno);
                    gc.CreatePdf(orderNumber, parcel_id, "tax input", driver, "GA", "Clayton");
                    driver.FindElement(By.Id("btnSrchAddress")).SendKeys(Keys.Enter);
                    Thread.Sleep(4000);
                    gc.CreatePdf(orderNumber, parcel_id, "tax info", driver, "GA", "Clayton");
                    aftertaxclick(houseno, sname);
                    if (searchcount == 0)
                    {
                        driver.FindElement(By.LinkText("NEXT")).Click();
                        Thread.Sleep(4000);
                        aftertaxclick(houseno, sname);
                        if (searchcount == 0)
                        {
                            driver.FindElement(By.LinkText("NEXT")).Click();
                            Thread.Sleep(4000);
                            aftertaxclick(houseno, sname);
                        }
                    }


                    //   Tax Payment Details Table: 
                    gc.CreatePdf(orderNumber, parcel_id, "Tax Payment Details", driver, "GA", "Clayton");
                    List<string> data = new List<string>();
                    IWebElement tbmulti = driver.FindElement(By.ClassName("apps10"));
                    IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDmulti;
                    int j = 0;
                    foreach (IWebElement row in TRmulti)
                    {
                        if (!row.Text.Contains("TAX YEAR"))
                        {
                            TDmulti = row.FindElements(By.TagName("td"));
                            if (TDmulti.Count == 5 && TDmulti[0].Text.Trim() != "")
                            {
                                j++;
                                string year = TDmulti[0].Text;
                                IWebElement ITaxBillCount = TDmulti[0].FindElement(By.TagName("a"));
                                string strTaxReal = ITaxBillCount.GetAttribute("href");
                                strTaxRealestate1.Add(strTaxReal);
                                //Tax Year~Bill No~Property Owner~Due Date~Date Paid
                                string tax_payment = TDmulti[0].Text + "~" + TDmulti[1].Text + "~" + TDmulti[2].Text + "~" + TDmulti[3].Text + "~" + TDmulti[4].Text;

                                gc.insert_date(orderNumber, parcel_id, 492, tax_payment, 1, DateTime.Now);
                            }
                        }
                    }
                    //      
                    int k = 0;
                    foreach (string real in strTaxRealestate1)
                    {
                        k++;
                        if (k == j)
                        {
                            //current year data
                            string property_location = "", Tax_year = "", billno = "", Date_paid = "", Tax_District = "", Due_date = "", FairMarketValue = "", AssessedValue = "", Exemptions = "", TaxAuthority = "";
                            driver.Navigate().GoToUrl(real);
                            Thread.Sleep(4000);
                            gc.CreatePdf(orderNumber, parcel_id, "Tax Bill Details", driver, "GA", "Clayton");
                            //Tax Bill Details Table:

                            property_location = driver.FindElement(By.XPath(" //*[@id='content']/table/tbody/tr[3]/td/span/em/strong")).Text.Trim();
                            Tax_year = driver.FindElement(By.XPath("//*[@id='content']/table/tbody/tr[5]/td[1]/p[2]")).Text.Trim();
                            billno = driver.FindElement(By.XPath("//*[@id='content']/table/tbody/tr[5]/td[2]/p[2]")).Text.Trim();
                            Date_paid = driver.FindElement(By.XPath(" //*[@id='content']/table/tbody/tr[5]/td[4]/p[2]")).Text.Trim();
                            Tax_District = driver.FindElement(By.XPath(" //*[@id='content']/table/tbody/tr[5]/td[5]/p[2]")).Text.Trim();
                            Due_date = driver.FindElement(By.XPath("//*[@id='content']/table/tbody/tr[5]/td[6]/p[2]")).Text.Trim();
                            FairMarketValue = driver.FindElement(By.XPath("//*[@id='content']/table/tbody/tr[8]/td[1]")).Text.Trim();
                            FairMarketValue = WebDriverTest.After(FairMarketValue, "FAIR MARKET VALUE ").Trim();
                            AssessedValue = driver.FindElement(By.XPath(" //*[@id='content']/table/tbody/tr[8]/td[2]")).Text.Trim();
                            AssessedValue = WebDriverTest.After(AssessedValue, "ASSESSED VALUE").Trim();
                            Exemptions = driver.FindElement(By.XPath(" //*[@id='content']/table/tbody/tr[10]/td[1]")).Text.Trim();
                            Exemptions = WebDriverTest.After(Exemptions, "EXEMPTIONS").Trim();
                            TaxAuthority = "Clayton County Administration Annex 3, 2nd Floor 121 South McDonough St. Jonesboro, GA 30236 Tax: (770) 477-3311";

                            //Property located at~Tax Year~Bill No~Date Paid~Tax District~Due date~Fair Market Value~Assessed Value~Exemptions~Tax Authority
                            string tax_bill = property_location + "~" + Tax_year + "~" + billno + "~" + Date_paid + "~" + Tax_District + "~" + Due_date + "~" + FairMarketValue + "~" + AssessedValue + "~" + Exemptions + "~" + TaxAuthority;
                            gc.insert_date(orderNumber, parcel_id, 493, tax_bill, 1, DateTime.Now);
                            //Current Tax Statement Details Table: 
                            //Tax Statement~Millage Rate~Tax
                            string balance = "";
                            balance = driver.FindElement(By.XPath("//*[@id='content']/table/tbody/tr[41]")).Text;

                            if (!balance.Contains("Delinquent Penalities"))
                            {
                                balance = WebDriverTest.After(balance, "Total balance due:").Trim();
                            }
                            else
                            {
                                string date = DateTime.Now.ToString("MM/dd/yyyy");
                                driver.FindElement(By.Id("date2")).SendKeys(date);
                                driver.FindElement(By.XPath("//*[@id='content']/table/tbody/tr[43]/td[2]/form/input[2]")).SendKeys(Keys.Enter);
                                Thread.Sleep(3000);
                                gc.CreatePdf(orderNumber, parcel_id, "Pay off Details", driver, "GA", "Clayton");
                                string countytax = "", interest = "", deliqpenality = "", fifa = "", amsfee = "", legalfee = "", totaldue = "", payoffdate = "";
                                payoffdate = driver.FindElement(By.XPath("//*[@id='content']/table[2]/tbody/tr[6]/td[3]")).Text;
                                IWebElement tbmulti12 = driver.FindElement(By.XPath("//*[@id='content']/table[3]/tbody/tr/td[1]/table/tbody"));
                                IList<IWebElement> TRmulti12 = tbmulti12.FindElements(By.TagName("tr"));
                                IList<IWebElement> TDmulti12;
                                int m = 0;
                                foreach (IWebElement row in TRmulti12)
                                {
                                    TDmulti12 = row.FindElements(By.TagName("td"));
                                    if (TDmulti12.Count == 2 && TDmulti12[0].Text.Trim() != "")
                                    {
                                        if (m == 1)
                                            countytax = TDmulti12[1].Text;
                                        if (m == 2)
                                            interest = TDmulti12[1].Text;
                                        if (m == 3)
                                            deliqpenality = TDmulti12[1].Text;
                                        if (m == 4)
                                            fifa = TDmulti12[1].Text;
                                        if (m == 5)
                                            amsfee = TDmulti12[1].Text;
                                        if (m == 6)
                                            legalfee = TDmulti12[1].Text;
                                        if (m == 7)
                                            totaldue = TDmulti12[1].Text;

                                        m++;
                                    }
                                }
                                //PayOff Date~Due Date~Tax District~County Tax~Interest~Deliquent Penalty~FIFA~AMS Fee~Legal Fee~Total Due
                                string tax_deli = payoffdate + "~" + countytax + "~" + interest + "~" + deliqpenality + "~" + fifa + "~" + amsfee + "~" + legalfee + "~" + totaldue;
                                gc.insert_date(orderNumber, parcel_id, 494, tax_deli, 1, DateTime.Now);
                                driver.Navigate().Back();
                                Thread.Sleep(4000);
                            }
                            IWebElement tbmulti1 = driver.FindElement(By.ClassName("apps10"));
                            IList<IWebElement> TRmulti1 = tbmulti1.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDmulti1;
                            foreach (IWebElement row in TRmulti1)
                            {
                                TDmulti1 = row.FindElements(By.TagName("td"));
                                if (!row.Text.Contains("Millage Rate") && !row.Text.Contains("FAIR MARKET VALUE"))
                                {
                                    if (TDmulti1.Count == 4 && TDmulti1[0].Text.Trim() != "")
                                    {
                                        string tax_info11 = TDmulti1[0].Text + "~" + TDmulti1[1].Text + "~" + TDmulti1[2].Text;
                                        gc.insert_date(orderNumber, parcel_id, 495, tax_info11, 1, DateTime.Now);
                                    }
                                }
                            }
                            string tax_info1 = "Total Due" + "~" + "" + "~" + balance;
                            gc.insert_date(orderNumber, parcel_id, 495, tax_info1, 1, DateTime.Now);
                        }
                        if (k == (j - 1))
                        {
                            driver.Navigate().GoToUrl(real);
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, parcel_id, "tax info year", driver, "GA", "Clayton");
                            driver.Navigate().Back();
                            Thread.Sleep(2000);
                        }
                        if (k == (j - 2))
                        {
                            driver.Navigate().GoToUrl(real);
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, parcel_id, "tax info year1", driver, "GA", "Clayton");
                            driver.Navigate().Back();
                            Thread.Sleep(2000);
                        }

                    }

                    if (county == "RIVERDALE")
                    {
                        driver.Navigate().GoToUrl("https://wipp.edmundsassoc.com/Wipp/?wippid=RDGA");
                        Thread.Sleep(4000);
                        string address = houseno + " " + sname;
                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/div/table/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr[2]/td[5]/input")).SendKeys(address);
                        gc.CreatePdf(orderNumber, parcel_id, "city search", driver, "GA", "Clayton");
                        //  gc.CreatePdf_WOP(orderNumber, "city Address search", driver, "GA", "Clayton");
                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/div/table/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr[2]/td[6]/button")).SendKeys(Keys.Enter);
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, parcel_id, "search result", driver, "GA", "Clayton");
                        IWebElement searchtableElement1 = driver.FindElement(By.XPath("/html/body/div[2]/div/table/tbody/tr[2]/td[2]/div/table/tbody/tr[1]/td/table/tbody"));
                        IList<IWebElement> searchtableRow1 = searchtableElement1.FindElements(By.TagName("tr"));
                        IList<IWebElement> searchrowTD1;
                        // List<string> searchlist1 = new List<string>();
                        // int i1 = 1, p = 0;
                        // string[] parcel = new string[3];
                        foreach (IWebElement row in searchtableRow1)
                        {
                            searchrowTD1 = row.FindElements(By.TagName("td"));
                            if (searchrowTD1.Count != 0)
                            {
                                if (!row.Text.Contains("Property Location"))
                                {

                                    if (row.Text.Contains(address.ToUpper()))
                                    {
                                        IWebElement city = searchrowTD1[0].FindElement(By.TagName("input"));
                                        city.Click();
                                        break;
                                    }
                                }
                            }
                        }
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, parcel_id, "tax info city", driver, "GA", "Clayton");
                        //Block/Lot/Qual: 1315.1C D 018. Tax Account Id: 2301 Property Location: 6765 POWERS ST Zoning Code: Owner Name/Address: MARTIN HEIDEMARLE Land Value: 4,400 284 HIGHAM HILL RD Improvement Value: 13,715 E175RG Exempt Value: 0 LONDON, . . Total Assessed Value: 18,115 Deductions: None
                        //Block/Lot/Qual~Property Location~Owner Name~Owner Address~Tax Account Id~Zoning Code~Land Value~Improvement Value~Exempt Value~Total Assessed Value~Deductions~City Tax Authority
                        string fulltext = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/table/tbody/tr/td/table/tbody")).Text.Replace("\r\n", " ");

                        string block = "", PropertyLocation = "", OwnerNameCity = "", OwnerAddress1 = "", OwnerAddress2 = "", OwnerAddress3 = "", TaxAccountId = "", ZoningCode = "", LandValue = "", ImprovementValue = "", ExemptValue = "", TotalAssessedValue = "", Deductions = "", taxaddress = "";

                        block = gc.Between(fulltext, "Block/Lot/Qual:", "Tax Account Id:");
                        PropertyLocation = gc.Between(fulltext, "Property Location:", "Zoning Code:");
                        OwnerNameCity = gc.Between(fulltext, "Owner Name/Address:", "Land Value:");
                        OwnerAddress1 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/table/tbody/tr/td/table/tbody/tr[4]/td[2]")).Text;
                        OwnerAddress2 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/table/tbody/tr/td/table/tbody/tr[5]/td[2]")).Text;
                        OwnerAddress3 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/table/tbody/tr/td/table/tbody/tr[6]/td[2]")).Text;
                        TaxAccountId = gc.Between(fulltext, "Tax Account Id:", "Property Location:");
                        ZoningCode = gc.Between(fulltext, "Zoning Code:", "Owner Name/Address:");
                        LandValue = gc.Between(fulltext, "Land Value:", "Improvement Value:").Split(' ')[1];
                        ImprovementValue = gc.Between(fulltext, "Improvement Value:", "Exempt Value:").Split(' ')[1];
                        ExemptValue = gc.Between(fulltext, "Exempt Value:", "Total Assessed Value").Split(' ')[1];
                        TotalAssessedValue = gc.Between(fulltext, "Total Assessed Value:", "Deductions:");
                        Deductions = GlobalClass.After(fulltext, "Deductions:");
                        OwnerAddress1 = OwnerAddress1 + " " + OwnerAddress2 + " " + OwnerAddress3;
                        string tax_infoCity = block + "~" + PropertyLocation + "~" + OwnerNameCity + "~" + OwnerAddress1 + "~" + TaxAccountId + "~" + ZoningCode + "~" + LandValue + "~" + ImprovementValue + "~" + ExemptValue + "~" + TotalAssessedValue + "~" + Deductions + "~" + "City of Riverdale 6690 Church Street Riverdale, GA 30274 Telephone 770 - 909 - 5501";
                        gc.insert_date(orderNumber, parcel_id, 496, tax_infoCity, 1, DateTime.Now);
                        //     
                        //Year~Due Date~Type~Billed~Balance~Interest~Total Due~Status    
                        string msg = "";
                        string lastpayment = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div/table/tbody/tr[3]/td/table/tbody")).Text.Replace("\r\n", " ");
                        lastpayment = GlobalClass.After(lastpayment, "Last Payment:");
                        IWebElement tbmulti1 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/div/div/table/tbody/tr[2]/td/table/tbody"));
                        IList<IWebElement> TRmulti1 = tbmulti1.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDmulti1;
                        foreach (IWebElement row in TRmulti1)
                        {
                            TDmulti1 = row.FindElements(By.TagName("td"));
                            if (!row.Text.Contains("Year"))
                            {
                                if (TDmulti1.Count != 0 && TDmulti1[0].Text.Trim() != "")
                                {
                                    string tax_city1 = TDmulti1[0].Text + "~" + TDmulti1[1].Text + "~" + TDmulti1[2].Text + "~" + TDmulti1[3].Text + "~" + TDmulti1[5].Text + "~" + TDmulti1[6].Text + "~" + TDmulti1[7].Text + "~" + TDmulti1[8].Text;
                                    gc.insert_date(orderNumber, parcel_id, 834, tax_city1, 1, DateTime.Now);
                                    if (!TDmulti1[6].Text.Contains("0.00"))
                                    {
                                        msg = "For tax amount due, you must call the Collector's Office";

                                    }
                                }
                            }
                        }
                        string tax_city = "Last Payment" + "~" + lastpayment + "~" + msg + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                        gc.insert_date(orderNumber, parcel_id, 834, tax_city, 1, DateTime.Now);

                    }


                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "GA", "Clayton", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    GlobalClass.titleparcel = "";
                    gc.mergpdf(orderNumber, "GA", "Clayton");
                    return "Data Inserted Successfully";
                }
                catch (Exception ex)
                {
                    driver.Quit();
                    throw ex;
                }
            }
        }

        private void afterclick(string houseno, string sname)
        {

            IWebElement searchtableElement1 = driver.FindElement(By.XPath("//*[@id='content']/table/tbody"));
            IList<IWebElement> searchtableRow1 = searchtableElement1.FindElements(By.TagName("tr"));
            IList<IWebElement> searchrowTD1;
            List<string> searchlist1 = new List<string>();
            List<string> url1 = new List<string>();
            int i1 = 1, p = 0, a = 5; int n = 0;
            string[] parcel = new string[3];
            string element = "";
            foreach (IWebElement row in searchtableRow1)
            {
                searchrowTD1 = row.FindElements(By.TagName("td"));
                if (searchrowTD1.Count == 4)
                {
                    if (!row.Text.Contains("ADDRESS"))
                    {
                        string add = houseno + " " + sname;
                        if (row.Text.Contains(add.ToUpper()))
                        {
                            element = "//*[@id='content']/table/tbody/tr[" + a + "]/td/table/tbody/tr/td[1]/div/a";
                            //url1.Add(element);
                            if (p == 0)
                            {
                                parcel[0] = searchrowTD1[0].Text;

                            }
                            if (p == 1)
                            {
                                parcel[1] = searchrowTD1[0].Text;

                            }
                            if (p == 2)
                            {
                                parcel[2] = searchrowTD1[0].Text;

                            }

                            searchlist1.Add(searchrowTD1[0].Text);
                            p++;
                        }
                        a++;
                    }
                }
                i1++;
            }
            searchcount1 = searchlist1.Count;

            // n = a + 4;
            if (searchcount1 == 1)
            {
                IWebElement element1 = driver.FindElement(By.XPath(element));
                IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                js1.ExecuteScript("arguments[0].click();", element1);
                Thread.Sleep(3000);
            }
            else
            {
                if (searchcount1 == 2)
                {
                    if (parcel[0] == parcel[1])
                    {
                        IWebElement element5 = driver.FindElement(By.XPath(element));
                        IJavaScriptExecutor js5 = driver as IJavaScriptExecutor;
                        js5.ExecuteScript("arguments[0].click();", element5);
                        Thread.Sleep(3000);
                    }
                }
                if (searchcount1 == 3)
                {
                    if ((parcel[0] == parcel[1]) && (parcel[1] == parcel[2]))
                    {
                        IWebElement element4 = driver.FindElement(By.XPath(element));
                        IJavaScriptExecutor js4 = driver as IJavaScriptExecutor;
                        js4.ExecuteScript("arguments[0].click();", element4);
                        Thread.Sleep(3000);
                    }
                }
                // HttpContext.Current.Session["multiparcel_GAClayton"] = "Yes";
                // driver.Quit();
                // return "MultiParcel";
            }
        }
        private void aftertaxclick(string houseno, string sname)
        {
            IWebElement searchtableElement = driver.FindElement(By.XPath("//*[@id='content']/table[2]/tbody"));
            IList<IWebElement> searchtableRow = searchtableElement.FindElements(By.TagName("tr"));
            IList<IWebElement> searchrowTD;
            List<string> searchlist = new List<string>();
            List<string> url = new List<string>();
            int i = 1; int a = 5;
            int q = 0; string element = "";
            string[] parceltax = new string[5];
            foreach (IWebElement row in searchtableRow)
            {
                searchrowTD = row.FindElements(By.TagName("td"));
                if (searchrowTD.Count == 4)
                {
                    if (!row.Text.Contains("ADDRESS"))
                    {
                        string add = houseno + "   " + sname;
                        if (row.Text.Contains(add.ToUpper()))
                        {//*[@id="content"]/table[2]/tbody/tr[6]/td/table/tbody/tr/td[1]/a
                            element = "//*[@id='content']/table[2]/tbody/tr[" + a + "]/td/table/tbody/tr/td[1]/a";
                            if (q == 0)
                            {
                                parceltax[0] = searchrowTD[0].Text;

                            }
                            if (q == 1)
                            {
                                parceltax[1] = searchrowTD[0].Text;

                            }
                            if (q == 2)
                            {
                                parceltax[2] = searchrowTD[0].Text;

                            }
                            if (q == 3)
                            {
                                parceltax[3] = searchrowTD[0].Text;

                            }
                            if (q == 4)
                            {
                                parceltax[4] = searchrowTD[0].Text;

                            }
                            searchlist.Add(searchrowTD[0].Text);
                            q++;
                        }

                        a++;
                    }
                }
                i++;
            }
            searchcount = searchlist.Count;

            if (searchcount == 1)
            {
                IWebElement element2 = driver.FindElement(By.XPath(element));
                IJavaScriptExecutor js2 = driver as IJavaScriptExecutor;
                js2.ExecuteScript("arguments[0].click();", element2);
                Thread.Sleep(3000);
            }
            else
            {
                if (searchcount == 2)
                {
                    if (parceltax[0] == parceltax[1])
                    {
                        IWebElement element6 = driver.FindElement(By.XPath(element));
                        IJavaScriptExecutor js6 = driver as IJavaScriptExecutor;
                        js6.ExecuteScript("arguments[0].click();", element6);
                        Thread.Sleep(3000);

                    }
                }
                if (searchcount == 3)
                {
                    if ((parceltax[0] == parceltax[1]) && (parceltax[1] == parceltax[2]))
                    {
                        IWebElement element3 = driver.FindElement(By.XPath(element));
                        IJavaScriptExecutor js3 = driver as IJavaScriptExecutor;
                        js3.ExecuteScript("arguments[0].click();", element3);
                        Thread.Sleep(3000);
                    }
                }

                if (searchcount == 4)
                {
                    if ((parceltax[0] == parceltax[1]) && (parceltax[1] == parceltax[2]) && (parceltax[2] == parceltax[3]))
                    {
                        IWebElement element3 = driver.FindElement(By.XPath(element));
                        IJavaScriptExecutor js3 = driver as IJavaScriptExecutor;
                        js3.ExecuteScript("arguments[0].click();", element3);
                        Thread.Sleep(3000);
                    }
                }
                if (searchcount == 5)
                {
                    if ((parceltax[0] == parceltax[1]) && (parceltax[1] == parceltax[2]) && (parceltax[2] == parceltax[3]) && (parceltax[3] == parceltax[4]))
                    {
                        IWebElement element3 = driver.FindElement(By.XPath(element));
                        IJavaScriptExecutor js3 = driver as IJavaScriptExecutor;
                        js3.ExecuteScript("arguments[0].click();", element3);
                        Thread.Sleep(3000);
                    }
                }

            }
        }

    }
}