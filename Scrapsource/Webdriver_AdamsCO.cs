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
    public class Webdriver_AdamsCO
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string AdamsCO(string Streetno, string sname, string direction, string streettype, string unitnumber, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            List<string> taxinformation = new List<string>();
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string As_of = "", Total_Due = "", MillLevy = "", Class = "", Built = "";
            string Parcel_number = "", Tax_Authority = "", yearbuild = "", AddressCombain = "", Addresshrf = "", TaxesDue = "", Multiaddressadd = "", MailingAddress = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new PhantomJSDriver();
            //driver = new ChromeDriver()
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    if (searchType == "titleflex")
                    {
                        string Address = "";
                        if (direction != "")
                        {
                            Address = Streetno.Trim() + " " + direction.ToUpper().Trim() + " " + sname.ToUpper().Trim() + " " + streettype.ToUpper().Trim();
                        }
                        if (Streetno != "")
                        {
                            Address = Streetno + " " + sname + " " + streettype + " " + unitnumber;
                        }
                        gc.TitleFlexSearch(orderNumber, "", ownername, Address, "CO", "Adams");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Adams_Zero"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        searchType = "parcel";
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString().Replace(".", "");
                    }
                    driver.Navigate().GoToUrl("http://gisapp.adcogov.org/quicksearch/");
                    if (searchType == "address")
                    {
                        driver.FindElement(By.Id("ctl00_ContentPlaceHolder_SearchOptions_2")).Click();
                        Thread.Sleep(1000);
                        driver.FindElement(By.Id("ctl00_ContentPlaceHolder_SearchInput")).SendKeys(Streetno);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "CO", "Adams");
                        driver.FindElement(By.Id("ctl00_ContentPlaceHolder_SearchSubmitLink")).Click();
                        Thread.Sleep(2000);
                        if (direction.Trim() != "")
                        {
                            AddressCombain = Streetno + " " + direction + " " + sname + " " + streettype;
                        }
                        else
                        {
                            AddressCombain = Streetno + " " + sname + " " + streettype.Trim();
                        }
                        int Max = 0;
                        IWebElement Addresstable = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder_QuickSearchResultsDisplay']/tbody"));
                        IList<IWebElement> Addresrow = Addresstable.FindElements(By.TagName("tr"));
                        IList<IWebElement> AddressTD;
                        gc.CreatePdf_WOP(orderNumber, "Address After", driver, "CO", "Adams");
                        foreach (IWebElement AddressT in Addresrow)
                        {
                            AddressTD = AddressT.FindElements(By.TagName("td"));
                            if (AddressTD.Count != 0 && AddressTD[2].Text.Contains(AddressCombain.ToUpper()))
                            {
                                IWebElement Parcellink = AddressTD[0].FindElement(By.TagName("a"));
                                Addresshrf = Parcellink.GetAttribute("href");
                                string parcelno = AddressTD[0].Text;
                                string OwnerName = AddressTD[1].Text;
                                string Address = AddressTD[2].Text;
                                string Multiresult = OwnerName + "~" + Address;
                                gc.insert_date(orderNumber, parcelno, 1851, Multiresult, 1, DateTime.Now);
                                Max++;
                            }
                        }
                        if (Max == 1)
                        {
                            driver.Navigate().GoToUrl(Addresshrf);
                            Thread.Sleep(2000);
                        }
                        if (Max > 1 && Max < 26)
                        {
                            HttpContext.Current.Session["multiParcel_Adams"] = "Yes";
                            driver.Quit();
                            return "MultiParcel";
                        }
                        if (Max > 25)
                        {
                            HttpContext.Current.Session["multiParcel_Adams_Multicount"] = "Maximum";
                            driver.Quit();
                            return "Maximum";
                        }
                        if (Max == 0)
                        {
                            HttpContext.Current.Session["Adams_Zero"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    if (searchType == "parcel")
                    {
                        string Parcel = parcelNumber.Substring(0, 1);
                        if (Convert.ToInt16(Parcel) == 0)
                        {
                            driver.FindElement(By.Id("ctl00_ContentPlaceHolder_SearchInput")).SendKeys(parcelNumber.Replace("-", "").Trim());
                        }
                        if (Convert.ToInt16(Parcel) != 0)
                        {
                            driver.FindElement(By.Id("ctl00_ContentPlaceHolder_SearchInput")).SendKeys("0" + parcelNumber.Replace("-", "").Trim());
                        }
                        driver.FindElement(By.Id("ctl00_ContentPlaceHolder_SearchSubmitLink")).Click();
                        Thread.Sleep(2000);
                        try
                        {
                            string Nodata = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder_QuickSearchResultsDisplay']/tbody/tr/td")).Text;
                            if (Nodata.Contains("Sorry, there are no data"))
                            {
                                HttpContext.Current.Session["Adams_Zero"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                        IWebElement ParcelLink = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder_QuickSearchResultsDisplay']/tbody/tr[2]/td[1]/a"));
                        string Parcelhref = ParcelLink.GetAttribute("href");
                        driver.Navigate().GoToUrl(Parcelhref);
                        Thread.Sleep(2000);
                    }
                    if (searchType == "Account")
                    {
                        driver.FindElement(By.Id("ctl00_ContentPlaceHolder_SearchOptions_1")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.Id("ctl00_ContentPlaceHolder_SearchInput")).SendKeys(unitnumber);
                        gc.CreatePdf_WOP(orderNumber, "Account search", driver, "CO", "Adams");
                        driver.FindElement(By.Id("ctl00_ContentPlaceHolder_SearchSubmitLink")).Click();
                        Thread.Sleep(20000);
                        gc.CreatePdf_WOP(orderNumber, "Account Click", driver, "CO", "Adams");
                        IWebElement Accountlink = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder_QuickSearchResultsDisplay']/tbody/tr[2]/td[1]")).FindElement(By.TagName("a"));
                        string Accounthref = Accountlink.GetAttribute("href");
                        driver.Navigate().GoToUrl(Accounthref);
                    }
                    //*[@id="propertyReport"]/span[3]/span[1]/div/span[2]
                    Parcel_number = driver.FindElement(By.XPath("//*[@id='propertyReport']/span[3]/span[1]/div/span[2]")).Text;
                    string owner = driver.FindElement(By.Id("ownerNameLabel")).Text;
                    string AddressPro = driver.FindElement(By.Id("propertyContentCell")).Text;
                    IWebElement Expandall = driver.FindElement(By.XPath("//*[@id='propertyReport']/span[2]/span[1]/a"));
                    Expandall.Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Expand All", driver, "CO", "Adams");
                    string Account = driver.FindElement(By.XPath("//*[@id='propertyReport']/span[6]/span[3]/div/table/tbody/tr[2]/td[1]/span")).Text;
                    string Taxdistrist = driver.FindElement(By.XPath("//*[@id='propertyReport']/span[6]/span[3]/div/table/tbody/tr[2]/td[3]/a")).Text;
                    string Legal = driver.FindElement(By.XPath(" //*[@id='propertyReport']/span[5]/span[3]/div/span")).Text;
                    string Subdevision = driver.FindElement(By.XPath("//*[@id='propertyReport']/span[5]/span[6]/div/span")).Text;
                    string MillLevyAss = driver.FindElement(By.XPath("//*[@id='propertyReport']/span[6]/span[3]/div/table/tbody/tr[2]/td[4]/span")).Text;
                    string Permit = driver.FindElement(By.XPath("//*[@id='propertyReport']/span[8]/span[3]/div")).Text;
                    Thread.Sleep(2000);
                    try
                    {
                        yearbuild = driver.FindElement(By.XPath("//*[@id='propertyReport']/span[14]/span[2]/div/table/tbody/tr[2]/td[2]/span")).Text;
                    }
                    catch { }
                    string Buildas = driver.FindElement(By.XPath("//*[@id='propertyReport']/span[14]/span[2]/div/table/tbody/tr[1]/td[2]/span")).Text;
                    string Buildingtype = driver.FindElement(By.XPath("//*[@id='propertyReport']/span[14]/span[2]/div/table/tbody/tr[3]/td[2]/span")).Text;
                    string Propertydetailresult = owner + "~" + AddressPro + "~" + Account + "~" + Taxdistrist + "~" + Legal + "~" + Subdevision + "~" + MillLevyAss + "~" + Buildas + "~" + Buildingtype + "~" + yearbuild + "~" + Permit;
                    gc.insert_date(orderNumber, Parcel_number, 1613, Propertydetailresult, 1, DateTime.Now);
                    IWebElement landvaluetable = driver.FindElement(By.XPath("//*[@id='propertyReport']/span[12]/span[3]/div/table/tbody"));
                    IList<IWebElement> landvaluerow = landvaluetable.FindElements(By.TagName("tr"));
                    IList<IWebElement> landvalueid;
                    foreach (IWebElement landvalue in landvaluerow)
                    {
                        landvalueid = landvalue.FindElements(By.TagName("td"));
                        if (!landvalue.Text.Contains("Land Subtotal") && !landvalue.Text.Contains("Account Number") && landvalueid[0].Text.Trim() != "")
                        {
                            string LandResult1 = landvalueid[0].Text + "~" + landvalueid[1].Text + "~" + landvalueid[2].Text + "~" + landvalueid[3].Text + "~" + landvalueid[4].Text + "~" + landvalueid[5].Text + "~" + landvalueid[6].Text + "~" + landvalueid[7].Text + "~" + landvalueid[8].Text;
                            gc.insert_date(orderNumber, Parcel_number, 1628, LandResult1, 1, DateTime.Now);
                        }
                        if (landvalue.Text.Contains("Land Subtotal"))
                        {
                            string Landresult = landvalueid[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + landvalueid[7].Text + "~" + landvalueid[8].Text;
                            gc.insert_date(orderNumber, Parcel_number, 1628, Landresult, 1, DateTime.Now);
                        }
                    }
                    IWebElement Improvementtable = driver.FindElement(By.XPath("//*[@id='propertyReport']/span[12]/span[5]/div/table/tbody"));
                    IList<IWebElement> Improvementrow = Improvementtable.FindElements(By.TagName("tr"));
                    IList<IWebElement> Improvementid;
                    foreach (IWebElement Improvement in Improvementrow)
                    {
                        Improvementid = Improvement.FindElements(By.TagName("td"));
                        if (Improvement.Text.Contains("Improvements Subtotal:"))
                        {
                            string impromentresult = Improvementid[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + Improvementid[1].Text + "~" + Improvementid[2].Text; ;
                            gc.insert_date(orderNumber, Parcel_number, 1628, impromentresult, 1, DateTime.Now);
                        }
                    }
                    IWebElement TotalPropertytable = driver.FindElement(By.XPath("//*[@id='propertyReport']/span[12]/span[7]/table/tbody"));
                    IList<IWebElement> TotalPropertyrow = TotalPropertytable.FindElements(By.TagName("tr"));
                    IList<IWebElement> TotalPropertytid;
                    foreach (IWebElement TotalProperty in TotalPropertyrow)
                    {
                        TotalPropertytid = TotalProperty.FindElements(By.TagName("td"));
                        if (TotalPropertytid.Count != 0)
                        {
                            string TotalPropertyresult = TotalPropertytid[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + TotalPropertytid[1].Text + "~" + TotalPropertytid[2].Text;
                            gc.insert_date(orderNumber, Parcel_number, 1628, TotalPropertyresult, 1, DateTime.Now);
                        }
                    }
                    //Tax_Authority
                    try
                    {
                        driver.Navigate().GoToUrl("http://www.adcogov.org/faqs/where-do-i-make-payment-0");
                        string Taxauthority = driver.FindElement(By.XPath("//*[@id='node-8873']/div[1]/div/div/p")).Text;
                        Tax_Authority = gc.Between(Taxauthority, "to the", ". To");
                    }
                    catch { }
                    //Tax Site
                    driver.Navigate().GoToUrl("https://www.adcotax.com/treasurer/web/login.jsp");
                    driver.FindElement(By.XPath("//*[@id='middle_left']/font/form/input[1]")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Tax Site Enter", driver, "CO", "Adams");
                    driver.FindElement(By.Id("TaxAParcelID")).SendKeys(Parcel_number);
                    gc.CreatePdf(orderNumber, Parcel_number, "Tax Site Parcel", driver, "CO", "Adams");
                    driver.FindElement(By.XPath("//*[@id='middle']/font/form/table[3]/tbody/tr/td[1]/input")).Click();
                    Thread.Sleep(2000);
                    driver.FindElement(By.XPath("//*[@id='searchResultsTable']/tbody/tr/td[1]/strong/a")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Tax Site After", driver, "CO", "Adams");
                    //Summary
                    string check = "", AreaID = "", Title1 = "", ValueAmount = "", paymenttype = "", ValueTitle = "", Title2 = "", Taxes = "", TActual = "", TAssessed = "";
                    try
                    {
                        //IWebElement currenttaxtable = driver.FindElement(By.LinkText("Account Summary"));
                        IWebElement IValue = driver.FindElement(By.XPath("//*[@id='taxAccountValueSummary']/div/table/tbody"));
                        IList<IWebElement> IValueRow = IValue.FindElements(By.TagName("tr"));
                        IList<IWebElement> IValueTD;
                        foreach (IWebElement value in IValueRow)
                        {
                            IValueTD = value.FindElements(By.TagName("td"));
                            if (IValueTD.Count != 0 && value.Text != "")
                            {
                                if (IValueTD[1].Text.Trim() == "" && IValueTD[0].Text.Trim() != "" && IValueTD[2].Text.Trim() != "" && !IValueTD[0].Text.Contains("Area Id"))
                                {
                                    if (check != "" && check == "Area ID")
                                    {
                                        check = "";
                                        ValueAmount += IValueTD[0].Text + "~" + IValueTD[2].Text + "~";
                                    }
                                    else
                                    {
                                        ValueTitle += IValueTD[0].Text + "~";
                                        ValueAmount += IValueTD[2].Text + "~";
                                    }
                                }
                                if (IValueTD[1].Text.Trim() == "" && IValueTD[0].Text.Trim() != "" && IValueTD[2].Text.Trim() != "" && IValueTD[0].Text.Contains("Area Id"))
                                {
                                    ValueTitle += IValueTD[0].Text + "~" + IValueTD[2].Text + "~";
                                    check = "Area ID";
                                }
                                if (IValueTD[0].Text.Trim() == "" && IValueTD[1].Text.Trim() != "" && IValueTD[2].Text.Trim() != "")
                                {
                                    Title1 = IValueTD[1].Text;
                                    Title2 = IValueTD[2].Text;
                                }
                                if (IValueTD[0].Text.Trim() != "" && IValueTD[1].Text.Trim() != "" && IValueTD[2].Text.Trim() != "")
                                {
                                    ValueTitle += IValueTD[0].Text + "(" + Title1 + ")" + "~" + IValueTD[0].Text + "(" + Title2 + ")" + "~";
                                    ValueAmount += IValueTD[1].Text + "~" + IValueTD[2].Text + "~";
                                }
                            }
                        }

                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + ValueTitle.Remove(ValueTitle.Length - 1, 1) + "' where Id = '" + 1634 + "'");
                        gc.insert_date(orderNumber, Parcel_number, 1634, ValueAmount.Remove(ValueAmount.Length - 1, 1), 1, DateTime.Now);
                    }
                    catch
                    { }
                    int z = 0;
                    //*[@id="totals"]/table/tbody
                    //*[@id="totals"]/table/tbody
                    try
                    {

                        for (int i = 1; i < 3; i++)

                        {
                            IWebElement Inquirytable = driver.FindElement(By.Id("totals"));
                            if (Inquirytable.Text.Contains("Misc Due") || Inquirytable.Text.Contains("Interest Due"))
                            {
                                break;
                            }
                            IWebElement As_off = driver.FindElement(By.Id("paymentDate"));
                            As_of = As_off.GetAttribute("value");
                            if (i == 1)
                            {
                                paymenttype = driver.FindElement(By.XPath("//*[@id='inquiryForm']/table/tbody/tr[2]/td[2]/label[1]")).Text;
                                driver.FindElement(By.Id("paymentTypeFirst")).Click();
                                Thread.Sleep(3000);
                                gc.CreatePdf(orderNumber, Parcel_number, "first Due", driver, "CO", "Adams");
                            }
                            else
                            {
                                paymenttype = driver.FindElement(By.XPath("//*[@id='inquiryForm']/table/tbody/tr[2]/td[2]/label[2]")).Text;
                                //if (paymenttype == "First")
                                //{
                                //    driver.FindElement(By.Id("paymentTypeFirst")).Click();
                                //    Thread.Sleep(1000);
                                //    gc.CreatePdf(orderNumber, Parcel_number, "first", driver, "CO", "Adams");
                                //}
                                if (paymenttype == "Second")
                                {
                                    driver.FindElement(By.Id("paymentTypeSecond")).Click();
                                    Thread.Sleep(3000);
                                    gc.CreatePdf(orderNumber, Parcel_number, "second Due", driver, "CO", "Adams");
                                }
                            }

                            Total_Due = GlobalClass.After(Inquirytable.Text, "Total Due").Trim();
                            if (Inquirytable.Text.Contains("Taxes Due"))
                            {
                                TaxesDue = gc.Between(Inquirytable.Text, "Taxes Due", "Total Due").Trim();
                            }
                            string cuttenttaxresult1 = As_of + "~" + paymenttype + "~" + TaxesDue + "~" + Total_Due;
                            gc.insert_date(orderNumber, Parcel_number, 1635, cuttenttaxresult1, 1, DateTime.Now);
                            z++;
                        }
                    }
                    catch { }
                    if (z > 1)
                    {
                        for (int i = 1; i < 3; i++)
                        {
                            string strEffectiveDate = "";
                            string currDate = DateTime.Now.ToString("MM/dd/yyyy");
                            string dateChecking = DateTime.Now.ToString("MM") + "/15/" + DateTime.Now.ToString("yyyy");
                            if (i == 1)
                            {
                                if (Convert.ToDateTime(currDate) > Convert.ToDateTime(dateChecking))
                                {
                                    string nextEndOfMonth = "";
                                    if ((Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))) < 12))
                                    {
                                        nextEndOfMonth = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM")) + 1), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")) + 1)).ToString("MM/dd/yyyy");

                                    }
                                    else
                                    {
                                        int nxtYr = Convert.ToInt16(DateTime.Now.ToString("yyyy")) + 1;
                                        nextEndOfMonth = nextEndOfMonth = new DateTime(nxtYr, 1, DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), 1)).ToString("MM/dd/yyyy");

                                    }
                                    strEffectiveDate = nextEndOfMonth;
                                }
                                else
                                {
                                    string EndOfMonth = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")))).ToString("MM/dd/yyyy");
                                    strEffectiveDate = EndOfMonth;
                                }
                            }
                            paymenttype = driver.FindElement(By.XPath("//*[@id='inquiryForm']/table/tbody/tr[2]/td[2]/label[1]")).Text;
                            if (i == 1)
                            {
                                if (paymenttype == "First")
                                {
                                    driver.FindElement(By.Id("paymentDate")).Clear();
                                    driver.FindElement(By.Id("paymentDate")).SendKeys(strEffectiveDate);
                                    Thread.Sleep(1000);
                                    driver.FindElement(By.Id("paymentTypeFirst")).Click();
                                    Thread.Sleep(1000);
                                    gc.CreatePdf(orderNumber, Parcel_number, "first", driver, "CO", "Adams");
                                }
                            }
                            else
                            {
                                paymenttype = driver.FindElement(By.XPath("//*[@id='inquiryForm']/table/tbody/tr[2]/td[2]/label[2]")).Text;
                                if (paymenttype == "Full")
                                {
                                    driver.FindElement(By.Id("paymentTypeFull")).Click();
                                    Thread.Sleep(2000);
                                    gc.CreatePdf(orderNumber, Parcel_number, "Full", driver, "CO", "Adams");
                                }
                                if (paymenttype == "Second")
                                {
                                    driver.FindElement(By.Id("paymentTypeSecond")).Click();
                                    Thread.Sleep(2000);
                                    gc.CreatePdf(orderNumber, Parcel_number, "Second", driver, "CO", "Adams");
                                }
                            }
                            IWebElement As_off = driver.FindElement(By.Id("paymentDate"));
                            As_of = As_off.GetAttribute("value");
                            string strTaxesDue = "", strInterestDue = "", strPenalty = "", strTotal = "";
                            IWebElement Totaltable = driver.FindElement(By.Id("totals"));
                            IList<IWebElement> Totalrow = Totaltable.FindElements(By.TagName("tr"));
                            IList<IWebElement> totalth;
                            foreach (IWebElement Total in Totalrow)
                            {
                                totalth = Total.FindElements(By.TagName("td"));
                                if (totalth.Count != 0 && Total.Text.Contains("Taxes Due"))
                                {
                                    strTaxesDue = totalth[1].Text;
                                }
                                if (totalth.Count != 0 && Total.Text.Contains("Penalty"))
                                {
                                    strPenalty = totalth[1].Text;
                                }
                                if (totalth.Count != 0 && Total.Text.Contains("Interest Due"))
                                {
                                    strInterestDue = totalth[1].Text;
                                }
                                if (totalth.Count != 0 && Total.Text.Contains("Total Due"))
                                {
                                    strTotal = totalth[1].Text;
                                }
                            }
                            db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + "Payment Type~Taxes Due~Interest Due~Total Due~As of" + "' where Id = '" + 1640 + "'");
                            string Totalresult = paymenttype + "~" + strTaxesDue + "~" + strInterestDue + "~" + strTotal + "~" + As_of;
                            gc.insert_date(orderNumber, Parcel_number, 1640, Totalresult, 1, DateTime.Now);
                        }
                    }

                    driver.FindElement(By.LinkText("Transaction Detail")).Click();
                    Thread.Sleep(2000);
                    IWebElement Transationtable = driver.FindElement(By.XPath("//*[@id='middle']/table[2]/tbody"));
                    IList<IWebElement> Transationrow = Transationtable.FindElements(By.TagName("tr"));
                    IList<IWebElement> Transationid;
                    foreach (IWebElement Transation in Transationrow)
                    {
                        Transationid = Transation.FindElements(By.TagName("td"));
                        if (Transationid.Count != 0 && !Transation.Text.Contains("Tax Year"))
                        {
                            string impromentresult = Transationid[0].Text + "~" + Transationid[1].Text + "~" + Transationid[2].Text + "~" + Transationid[3].Text + "~" + Transationid[4].Text;
                            gc.insert_date(orderNumber, Parcel_number, 1636, impromentresult, 1, DateTime.Now);
                        }
                    }
                    gc.CreatePdf(orderNumber, Parcel_number, "Transaction Detail", driver, "CO", "Adams");
                    IWebElement summaryTransationtable = driver.FindElement(By.XPath("//*[@id='middle']/table[1]/tbody"));
                    IList<IWebElement> summaryTransationrow = summaryTransationtable.FindElements(By.TagName("tr"));
                    IList<IWebElement> summaryTransationid;
                    foreach (IWebElement summaryTransation in summaryTransationrow)
                    {
                        summaryTransationid = summaryTransation.FindElements(By.TagName("td"));
                        if (summaryTransationid.Count != 0 & !summaryTransation.Text.Contains("Tax Year"))
                        {
                            string impromentsummaryTransationresult = summaryTransationid[0].Text + "~" + summaryTransationid[1].Text + "~" + summaryTransationid[2].Text + "~" + summaryTransationid[3].Text + "~" + summaryTransationid[4].Text + "~" + summaryTransationid[5].Text + "~" + summaryTransationid[6].Text + "~" + summaryTransationid[7].Text + "~" + Tax_Authority;
                            gc.insert_date(orderNumber, Parcel_number, 1637, impromentsummaryTransationresult, 1, DateTime.Now);
                        }
                    }
                    driver.FindElement(By.LinkText("Account Value")).Click();
                    Thread.Sleep(2000);
                    IWebElement Authoritytable = driver.FindElement(By.XPath("//*[@id='middle']/table[3]/tbody"));
                    IList<IWebElement> Authorityrow = Authoritytable.FindElements(By.TagName("tr"));
                    IList<IWebElement> Authorityid;
                    foreach (IWebElement Authority in Authorityrow)
                    {
                        Authorityid = Authority.FindElements(By.TagName("td"));
                        if (Authorityid.Count == 4 & !Authority.Text.Contains("Authority"))
                        {
                            string Authorityresult = Authorityid[0].Text + "~" + Authorityid[1].Text + "~" + Authorityid[2].Text + "~" + Authorityid[3].Text;
                            gc.insert_date(orderNumber, Parcel_number, 1638, Authorityresult, 1, DateTime.Now);
                        }
                    }
                    gc.CreatePdf(orderNumber, Parcel_number, "Account Value", driver, "CO", "Adams");
                    string geturl = driver.Url;
                    //Adams County Property
                    try
                    {
                        IWebElement IPropertyTax = driver.FindElement(By.LinkText("Adams County Property Tax Notice"));
                        string strPropertyTax = IPropertyTax.GetAttribute("href");
                        IWebElement IReedemedTax = driver.FindElement(By.LinkText("Redemption Certificate"));
                        string strReedemedTax = IReedemedTax.GetAttribute("href");
                        IWebElement IAccountBalance = driver.FindElement(By.LinkText("Account Balance"));
                        string strAccountBalance = IAccountBalance.GetAttribute("href");
                        IWebElement IStatementTaxDue = driver.FindElement(By.LinkText("Statement Of Taxes Due"));
                        string strStatementTaxDue = IStatementTaxDue.GetAttribute("href");
                        IWebElement ISummaryTax = driver.FindElement(By.LinkText("Summary of Taxes Due"));
                        string strSummaryTax = ISummaryTax.GetAttribute("href");

                        driver.Navigate().GoToUrl(strPropertyTax);
                        Thread.Sleep(6000);
                        try
                        {
                            IWebElement IPropertyTaxClick = driver.FindElement(By.LinkText("Adams County Property Tax Notice"));
                            IPropertyTaxClick.Click();
                            Thread.Sleep(4000);
                            gc.CreatePdf(orderNumber, Parcel_number, "Account Adams County Property Tax", driver, "CO", "Adams");
                        }
                        catch { }
                        driver.Navigate().GoToUrl(strReedemedTax);
                        Thread.Sleep(6000);
                        try
                        {
                            IWebElement IReedemedTaxClick = driver.FindElement(By.LinkText("Redemption Certificate"));
                            IReedemedTaxClick.Click();
                            Thread.Sleep(4000);
                            gc.CreatePdf(orderNumber, Parcel_number, "Account Redemption Certificate", driver, "CO", "Adams");
                        }
                        catch { }
                        driver.Navigate().GoToUrl(strAccountBalance);
                        Thread.Sleep(6000);
                        try
                        {
                            IWebElement IAccountTaxClick = driver.FindElement(By.LinkText("Account Balance"));
                            IAccountTaxClick.Click();
                            Thread.Sleep(4000);
                            gc.CreatePdf(orderNumber, Parcel_number, "Account Account Balance", driver, "CO", "Adams");
                        }
                        catch { }
                        driver.Navigate().GoToUrl(strStatementTaxDue);
                        Thread.Sleep(6000);
                        try
                        {
                            IWebElement IStatementTaxClick = driver.FindElement(By.LinkText("Statement Of Taxes Due"));
                            IStatementTaxClick.Click();
                            Thread.Sleep(4000);
                            gc.CreatePdf(orderNumber, Parcel_number, "Account Statement Of Taxes Due", driver, "CO", "Adams");
                        }
                        catch { }
                        driver.Navigate().GoToUrl(strSummaryTax);
                        Thread.Sleep(6000);
                        try
                        {
                            IWebElement ISummaryTaxClick = driver.FindElement(By.LinkText("Summary of Taxes Due"));
                            ISummaryTaxClick.Click();
                            Thread.Sleep(4000);
                            gc.CreatePdf(orderNumber, Parcel_number, "Account Summary of Taxes Due", driver, "CO", "Adams");
                        }
                        catch { }


                        //driver.FindElement(By.LinkText("Adams County Property Tax Notice")).Click();
                        //Thread.Sleep(9000);
                        ////*[@id="myReports"]/form/table[1]/tbody
                        //gc.CreatePdf(orderNumber, Parcel_number, "County Property Tax Notice", driver, "CO", "Adams");
                        ////driver.FindElement(By.XPath("//*[@id='myReports']/form/table[1]/tbody/tr[2]/td[2]/a"))
                        //driver.FindElement(By.LinkText("Adams County Property Tax Notice")).Click();
                        //Thread.Sleep(5000);
                        //gc.CreatePdf(orderNumber, Parcel_number, "Adams County Property", driver, "CO", "Adams");
                        //// string Adamsproperty = Countyadmslink.GetAttribute("href");
                        //driver.Navigate().GoToUrl(geturl);
                        ////Redemption Certificate
                        //driver.FindElement(By.LinkText("Redemption Certificate")).Click();
                        //Thread.Sleep(4000);
                        //IWebElement RedemWeb = driver.FindElement(By.LinkText("Redemption Certificate"));
                        //string Hrefred = RedemWeb.GetAttribute("href");
                        //gc.downloadfile(Hrefred, orderNumber, Parcel_number, "ViewTaxBill", "SC", "Lexington");
                        //Thread.Sleep(4000);
                        //gc.CreatePdf(orderNumber, Parcel_number, "Redemption County Property", driver, "CO", "Adams");
                        //driver.Navigate().GoToUrl(geturl);
                        //driver.FindElement(By.LinkText("Account Balance")).Click();
                        //Thread.Sleep(4000);
                        //driver.FindElement(By.LinkText("Account Balance")).Click();
                        //Thread.Sleep(4000);
                        //gc.CreatePdf(orderNumber, Parcel_number, "Account Balance", driver, "CO", "Adams");
                    }
                    catch { }
                    driver.Quit();
                    gc.mergpdf(orderNumber, "CO", "Adams");
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
