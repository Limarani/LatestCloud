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
using OpenQA.Selenium.Firefox;
using System.Diagnostics;
using OpenQA.Selenium.Remote;


namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_CAKern
    {
        //415 Reynosa Ave Bakersfield, CA 93307
        string Outparcelno = "", outputPath = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_CAKern(string streetNo, string direction, string streetName, string streetType, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string address = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    //Title Flex

                    if (searchType == "titleflex")
                    {
                        string Address = streetNo + " " + streetName;
                        gc.TitleFlexSearch(orderNumber, "", ownername, Address, "CA", "Kern");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_CAKern"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "parcel")
                    {

                        //tax details
                        // string outputparcelno = "528-153-15-00-6";
                        //string parcelNumber = "463-520-22-00-0";
                        //string parcelNumber = parcelNumber;
                        TaxSearch(orderNumber, parcelNumber);
                        try
                        {
                            IWebElement Inodata = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_lblNoResults"));
                            if (Inodata.Text.Contains("No matches found"))
                            {
                                HttpContext.Current.Session["Nodata_CAKern"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "CurrentYear Tax_Aummary", driver, "CA", "Kern");
                        taxdetails(orderNumber, parcelNumber);
                        TaxSearch(orderNumber, parcelNumber);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "PreviousYear Tax_Aummary", driver, "CA", "Kern");
                        driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_btnPreviousYear")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        taxdetails_previousyear(orderNumber, parcelNumber);
                        GlobalClass.titleparcel = "";
                    }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "CA", "Kern", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "CA", "Kern");
                    return "Data Inserted Successfully";
                }
                catch (Exception ex)
                {
                    driver.Quit();
                    throw ex;
                }
            }
        }
        public void taxdetails(string orderno, string parcelno)
        {
            //*[@id="Table5"]
            IWebElement taxbilltable = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_tblBills']"));
            IList<IWebElement> taxbilltableRow = taxbilltable.FindElements(By.TagName("tr"));
            int rowcount = taxbilltableRow.Count;
            IList<IWebElement> taxbillrowTD;
            List<string> listurl = new List<string>();
            foreach (IWebElement rownew in taxbilltableRow)
            {
                taxbillrowTD = rownew.FindElements(By.TagName("a"));
                if (taxbillrowTD.Count != 0 && !rownew.Text.Contains("Installment"))
                {
                    string url = taxbillrowTD[0].GetAttribute("href");
                    listurl.Add(url);
                }
            }
            int a = 0;
            foreach (string URL in listurl)
            {
                driver.Navigate().GoToUrl(URL);
                Thread.Sleep(2000);
                string bill_number = "", fiscal_year = "", tax_rate_area1 = "", total_amount_due = "", bill_type = "", prop_address1 = "";
                string inst = "", delinq_date = "", amount = "", statuss = "", paid_amount = "", paid_date = "", penalty = "", penalty1 = "";
                string inst1 = "", delinq_date1 = "", amount1 = "", status1 = "", paid_amount1 = "", paid_date1 = "", comment = "";
                try
                {
                    bill_number = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_lblBillNumber")).Text.Trim();
                    fiscal_year = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_lblTaxYear")).Text.Trim();
                    fiscal_year = WebDriverTest.After(fiscal_year, "For the fiscal year").Trim();
                    tax_rate_area1 = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_lblTaxRateArea")).Text.Trim();
                    total_amount_due = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_lblTotalAmountDue")).Text.Trim();
                    bill_type = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_lblBillType")).Text.Trim();
                    prop_address1 = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_lblAddress")).Text.Trim();
                    if (prop_address1.Contains("\r\n"))
                    {
                        prop_address1 = prop_address1.Replace("\r\n", ",");
                    }                 
                    string land = "", imp = "", minerals = "", perprop = "", otherimp = "", exem = "", netassvalue = "";
                    if (a==0)
                    {

                         land = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_lblLand")).Text.Trim();
                         imp = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_lblImprovements")).Text.Trim();
                        minerals = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_lblMinerals")).Text.Trim();
                        perprop = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_lblPersonalProperty")).Text.Trim();
                        otherimp = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_lblOtherImprovements")).Text.Trim();
                        exem = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_lblExemptions")).Text.Trim();
                        netassvalue = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_lblNetAssessedValue")).Text.Trim();
                        string ass = fiscal_year + "~" +  land + "~" + imp + "~" + minerals + "~" + perprop + "~" + otherimp + "~" + exem + "~" + netassvalue;
                        gc.insert_date(orderno, parcelno, 77, ass, 1, DateTime.Now);
                        string prop = prop_address1 + "~" + tax_rate_area1;
                        gc.insert_date(orderno, parcelno, 75, prop, 1, DateTime.Now);
                    }
                    gc.CreatePdf(orderno, parcelno, "TaxSummary_" + fiscal_year + "_" + bill_number, driver, "CA", "Kern");
                    IWebElement insttable = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_tblInstallments']"));
                    IList<IWebElement> insttableRow = insttable.FindElements(By.TagName("tr"));
                    int instrowcount = insttableRow.Count;
                    IList<IWebElement> instrowTD;
                    int i = 1;
                    foreach (IWebElement rownew1 in insttableRow)
                    {
                        instrowTD = rownew1.FindElements(By.TagName("td"));
                        if (instrowTD.Count != 0)
                        {
                            try
                            {
                                if (instrowTD[1].Text.Contains("Bill was cancelled"))
                                {
                                    comment = instrowTD[1].Text.Trim();
                                    if (comment.Contains("\r\n"))
                                    {
                                        comment = comment.Replace("\r\n", "");
                                    }
                                }
                                if (instrowTD[1].Text.Contains("Bill has been redeemed"))
                                {
                                    comment = instrowTD[1].Text.Trim();
                                    if (comment.Contains("\r\n"))
                                    {
                                        comment = comment.Replace("\r\n", "");
                                    }
                                }
                                if (instrowTD[1].Text.Contains("Please note"))
                                {

                                }
                                //This property has had multiple owners during the year
                                if (instrowTD[1].Text.Contains("This property has had multiple owners during the year"))
                                {
                                    comment = instrowTD[1].Text.Trim();
                                    if (comment.Contains("\r\n"))
                                    {
                                        comment = comment.Replace("\r\n", "");
                                    }
                                }
                                if (instrowTD[1].Text.Contains("First"))
                                {
                                    string bulk_status = instrowTD[4].Text.Trim();
                                    statuss = WebDriverTest.Before(bulk_status, "$").Trim();
                                    if (statuss.Contains("Paid"))
                                    {
                                        inst = instrowTD[1].Text.Trim(); delinq_date = instrowTD[2].Text.Trim(); amount = instrowTD[3].Text.Trim();
                                        paid_amount = gc.Between(bulk_status, "Paid", "on").Trim();
                                        paid_date = WebDriverTest.After(bulk_status, "on").Trim();
                                    }
                                    inst = instrowTD[1].Text.Trim();
                                    delinq_date = instrowTD[2].Text.Trim();
                                    amount = instrowTD[3].Text.Trim();
                                }

                                if (instrowTD[1].Text.Contains("Second"))
                                {
                                    string bulk_status1 = instrowTD[4].Text.Trim();
                                    status1 = WebDriverTest.Before(bulk_status1, "$").Trim();
                                    if (status1.Contains("Paid"))
                                    {
                                        inst1 = instrowTD[1].Text.Trim(); delinq_date1 = instrowTD[2].Text.Trim(); amount1 = instrowTD[3].Text.Trim();
                                        paid_amount1 = gc.Between(bulk_status1, "Paid", "on").Trim();
                                        paid_date1 = WebDriverTest.After(bulk_status1, "on").Trim();
                                    }
                                    inst1 = instrowTD[1].Text.Trim(); delinq_date1 = instrowTD[2].Text.Trim(); amount1 = instrowTD[3].Text.Trim();
                                }
                                if (instrowTD[1].Text.Contains("Second") && instrowTD[4].Text.Contains("Unpaid"))
                                {
                                    inst1 = instrowTD[1].Text.Trim();
                                    delinq_date1 = instrowTD[2].Text.Trim();
                                    amount1 = instrowTD[3].Text.Trim();
                                    string bulk_stat = instrowTD[4].Text.Trim();
                                    status1 = WebDriverTest.Before(bulk_stat, "$").Trim();
                                }
                                if (instrowTD[0].Text.Contains("Penalty if Delinquent"))
                                {
                                    delinq_date1 = delinq_date1 + "(Penalty if Delinquent)";
                                    penalty1 = instrowTD[1].Text.Trim();
                                }
                                if (instrowTD[1].Text.Contains("Includes Late Penalty"))
                                {
                                    delinq_date1 = delinq_date1 + "(Includes Late Penalty)";
                                    penalty1 = instrowTD[1].Text.Trim();
                                }
                                try
                                {
                                    if (instrowTD[2].Text.Contains("Single"))
                                    {
                                        inst = instrowTD[1].Text.Trim();
                                        delinq_date = instrowTD[2].Text.Trim();
                                        amount = instrowTD[3].Text.Trim();
                                    }
                                }
                                catch { }
                                i++;
                            }
                            catch { }
                        }
                    }
                
                //Property Address~TRA~Bill Number~Tax Year~total_amount_due~Bill Type~Installment1~Due Dat1e~Amount Due1~Penalty1~Paid Amount1~Paid Date1~Total Due1~Installment2~Due Dat2e~Amount Due2~Penalty2~Paid Amount2~Paid Date2~Total Due2~Comments
                string inst_details = prop_address1 + "~" + tax_rate_area1 + "~" + bill_number + "~" + fiscal_year + "~" + total_amount_due + "~" + bill_type + "~" + inst + "~" + delinq_date + "~" + amount + "~" + penalty + "~" + statuss + "~" + paid_amount + "~" + paid_date + "~" + inst1 + "~" + delinq_date1 + "~" + amount1 + "~" + penalty1 + "~" + status1 + "~" + paid_amount1 + "~" + paid_date1 + "~" + comment;
                gc.insert_date(orderno, parcelno, 79, inst_details, 1, DateTime.Now);
                     
                }

                catch { } //bill details
                a++;
                driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_btnDetails")).SendKeys(Keys.Enter);
                Thread.Sleep(2000);
                gc.CreatePdf(orderno, parcelno, "TaxBreakdowns_" + fiscal_year + "_" + bill_number, driver, "CA", "Kern");
                IWebElement breaktable = driver.FindElement(By.XPath("//*[@id='Table5']/tbody/tr[2]/td/table"));
                IList<IWebElement> breaktableRow = breaktable.FindElements(By.TagName("tr"));
                int breakrowcount = breaktableRow.Count;
                IList<IWebElement> breakrowTD;
                int j = 0;
                try
                {
                    foreach (IWebElement rowneww in breaktableRow)
                    {
                        breakrowTD = rowneww.FindElements(By.TagName("td"));
                        if (breakrowTD.Count != 0 && !rowneww.Text.Contains("Taxing Agency"))
                        {
                            //Taxing Agency~Telephone~Rate~Amount
                            if (j < breakrowcount - 3)
                            {
                                string breakdet = breakrowTD[0].Text.Trim() + "~" + breakrowTD[1].Text.Trim() + "~" + breakrowTD[2].Text.Trim() + "~" + breakrowTD[3].Text.Trim();
                                gc.insert_date(orderno, parcelno, 80, breakdet, 1, DateTime.Now);
                            }
                            else if (j == breakrowcount - 3)
                            {
                                string breakdet = breakrowTD[0].Text.Trim() + "~" + "-" + "~" + breakrowTD[1].Text.Trim() + "~" + "-";
                                gc.insert_date(orderno, parcelno, 80, breakdet, 1, DateTime.Now);
                            }
                            else if (j == breakrowcount - 2)
                            {
                                string breakdet = breakrowTD[0].Text.Trim() + "~" + "-" + "~" + "-" + "~" + breakrowTD[1].Text.Trim();
                                gc.insert_date(orderno, parcelno, 80, breakdet, 1, DateTime.Now);
                            }
                            else if (j == breakrowcount - 1)
                            {
                                string breakdet = breakrowTD[0].Text.Trim() + "~" + "-" + "~" + "-" + "~" + breakrowTD[1].Text.Trim();
                                gc.insert_date(orderno, parcelno, 80, breakdet, 1, DateTime.Now);
                            }
                        }
                        j++;
                    }
                }
                catch (Exception ex)
                {

                }

            }
        }
        public void taxdetails_previousyear(string orderno, string parcelno)
        {
            //*[@id="Table5"]
            IWebElement taxbilltable = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_tblBills']"));
            IList<IWebElement> taxbilltableRow = taxbilltable.FindElements(By.TagName("tr"));
            int rowcount = taxbilltableRow.Count;
            IList<IWebElement> taxbillrowTD;
            List<string> listurl = new List<string>();
            foreach (IWebElement rownew in taxbilltableRow)
            {
                taxbillrowTD = rownew.FindElements(By.TagName("a"));
                if (taxbillrowTD.Count != 0 && !rownew.Text.Contains("Installment"))
                {
                    string url = taxbillrowTD[0].GetAttribute("href");
                    if (!url.Contains("pdf"))
                    {
                        listurl.Add(url);
                    }

                }
            }

            foreach (string URL in listurl)
            {
                driver.Navigate().GoToUrl(URL);
                Thread.Sleep(2000);
                string bill_number = "", fiscal_year = "", tax_rate_area1 = "", total_amount_due = "", bill_type = "", prop_address1 = "";
                string inst = "", delinq_date = "", amount = "", statuss = "", paid_amount = "", paid_date = "", penalty1 = "", penalty = "";
                string inst1 = "", delinq_date1 = "", amount1 = "", status1 = "", paid_amount1 = "", paid_date1 = "", comment = "";
                try
                {
                    bill_number = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_lblBillNumber")).Text.Trim();
                    fiscal_year = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_lblTaxYear")).Text.Trim();
                    fiscal_year = WebDriverTest.After(fiscal_year, "For the fiscal year").Trim();
                    tax_rate_area1 = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_lblTaxRateArea")).Text.Trim();
                    total_amount_due = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_lblTotalAmountDue")).Text.Trim();
                    bill_type = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_lblBillType")).Text.Trim();
                    prop_address1 = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_lblAddress")).Text.Trim();
                    if (prop_address1.Contains("\r\n"))
                    {
                        prop_address1 = prop_address1.Replace("\r\n", ",");
                    }

                    gc.CreatePdf(orderno, parcelno, "TaxSummary_" + fiscal_year + "_" + bill_number, driver, "CA", "Kern");
                    IWebElement insttable = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_tblInstallments']"));
                    IList<IWebElement> insttableRow = insttable.FindElements(By.TagName("tr"));
                    int instrowcount = insttableRow.Count;
                    IList<IWebElement> instrowTD;
                    int i = 1;
                    foreach (IWebElement rownew1 in insttableRow)
                    {
                        instrowTD = rownew1.FindElements(By.TagName("td"));
                        if (instrowTD.Count != 0)
                        {
                            try
                            {
                                if (instrowTD[0].Text.Contains("Please note"))
                                {

                                }
                                if (instrowTD[1].Text.Contains("Bill was cancelled"))
                                {
                                    comment = instrowTD[1].Text.Trim();
                                    if (comment.Contains("\r\n"))
                                    {
                                        comment = comment.Replace("\r\n", "");
                                    }
                                }
                                if (instrowTD[1].Text.Contains("Bill has been redeemed"))
                                {
                                    comment = instrowTD[1].Text.Trim();
                                    if (comment.Contains("\r\n"))
                                    {
                                        comment = comment.Replace("\r\n", "");
                                    }
                                }
                                if (instrowTD[1].Text.Contains("This property has had multiple owners during the year"))
                                {
                                    comment = instrowTD[1].Text.Trim();
                                    if (comment.Contains("\r\n"))
                                    {
                                        comment = comment.Replace("\r\n", "");
                                    }
                                }
                                if (instrowTD[1].Text.Contains("First"))
                                {
                                    string bulk_status = instrowTD[4].Text.Trim();
                                    statuss = WebDriverTest.Before(bulk_status, "$").Trim();
                                    if (statuss.Contains("Paid"))
                                    {
                                        inst = instrowTD[1].Text.Trim(); delinq_date = instrowTD[2].Text.Trim(); amount = instrowTD[3].Text.Trim();
                                        paid_amount = gc.Between(bulk_status, "Paid", "on").Trim();
                                        paid_date = WebDriverTest.After(bulk_status, "on").Trim();
                                    }
                                    inst = instrowTD[1].Text.Trim();
                                    delinq_date = instrowTD[2].Text.Trim();
                                    amount = instrowTD[3].Text.Trim();
                                }

                                if (instrowTD[1].Text.Contains("Second"))
                                {
                                    string bulk_status1 = instrowTD[4].Text.Trim();
                                    status1 = WebDriverTest.Before(bulk_status1, "$").Trim();
                                    if (status1.Contains("Paid"))
                                    {
                                        inst1 = instrowTD[1].Text.Trim(); delinq_date1 = instrowTD[2].Text.Trim(); amount1 = instrowTD[3].Text.Trim();
                                        paid_amount1 = gc.Between(bulk_status1, "Paid", "on").Trim();
                                        paid_date1 = WebDriverTest.After(bulk_status1, "on").Trim();
                                    }
                                    inst1 = instrowTD[1].Text.Trim(); delinq_date1 = instrowTD[2].Text.Trim(); amount1 = instrowTD[3].Text.Trim();
                                }
                                if (instrowTD[1].Text.Contains("Second") && instrowTD[4].Text.Contains("Unpaid"))
                                {
                                    inst1 = instrowTD[1].Text.Trim();
                                    delinq_date1 = instrowTD[2].Text.Trim();
                                    amount1 = instrowTD[3].Text.Trim();
                                    string bulk_stat = instrowTD[4].Text.Trim();
                                    status1 = WebDriverTest.Before(bulk_stat, "$").Trim();
                                }
                                if (instrowTD[0].Text.Contains("Penalty if Delinquent"))
                                {
                                    delinq_date1 = delinq_date1 + "(Penalty if Delinquent)";
                                    penalty1 = instrowTD[1].Text.Trim();
                                }
                                if (instrowTD[1].Text.Contains("Includes Late Penalty"))
                                {
                                    delinq_date1 = delinq_date1 + "(Includes Late Penalty)";
                                    penalty1 = instrowTD[1].Text.Trim();
                                }
                                try
                                {
                                    if (instrowTD[2].Text.Contains("Single"))
                                    {
                                        inst = instrowTD[1].Text.Trim();
                                        delinq_date = instrowTD[2].Text.Trim();
                                        amount = instrowTD[3].Text.Trim();
                                    }
                                }
                                catch { }
                                i++;
                            }
                            catch { }
                        }
                    }
                //Property Address~TRA~Bill Number~Tax Year~total_amount_due~Bill Type~Installment1~Due Dat1e~Amount Due1~Penalty1~Paid Amount1~Paid Date1~Total Due1~Installment2~Due Dat2e~Amount Due2~Penalty2~Paid Amount2~Paid Date2~Total Due2~Comments
                string inst_details = prop_address1 + "~" + tax_rate_area1 + "~" + bill_number + "~" + fiscal_year + "~" + total_amount_due + "~" + bill_type + "~" + inst + "~" + delinq_date + "~" + amount + "~" + penalty + "~" + statuss + "~" + paid_amount + "~" + paid_date + "~" + inst1 + "~" + delinq_date1 + "~" + amount1 + "~" + penalty1 + "~" + status1 + "~" + paid_amount1 + "~" + paid_date1 + "~" + comment;
                gc.insert_date(orderno, parcelno, 79, inst_details, 1, DateTime.Now);

            }
                catch { }
        }
        }
        public void TaxSearch(string orderNumber, string parcelNumber)
        {
            try
            {
                driver.Navigate().GoToUrl("http://www.kcttc.co.kern.ca.us/payment/mainsearch.aspx");
                driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_txtSearchbyNumber")).SendKeys(parcelNumber);
                gc.CreatePdf(orderNumber, parcelNumber, "Tax Address Search", driver, "CA", "Kern");
                driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_btnSearch']")).SendKeys(Keys.Enter);
                gc.CreatePdf(orderNumber, parcelNumber, "Tax Address Search Result", driver, "CA", "Kern");
                IWebElement Isearch = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder1_dlSearchResults']/tbody/tr/td/table/tbody"));
                IList<IWebElement> IsearchRow = Isearch.FindElements(By.TagName("tr"));
                IList<IWebElement> IsearchTD;
                foreach (IWebElement search in IsearchRow)
                {
                    IsearchTD = search.FindElements(By.TagName("td"));
                    if (IsearchTD.Count != 0 && search.Text.Contains(parcelNumber) && !search.Text.Contains("Property Address"))
                    {
                        IWebElement ITaxSearch = IsearchTD[0].FindElement(By.TagName("a"));
                        ITaxSearch.Click();
                        break;
                    }
                }
            }
            catch(Exception ex)
            {

            }
        }
    }
}