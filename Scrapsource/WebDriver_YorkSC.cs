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
    public class WebDriver_YorkSC
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;

        public string FTP_YorkSC(string streetNo, string streetName, string direction, string streetType, string accountNo, string parcelNumber, string ownerName, string searchType, string orderNumber)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            IJavaScriptExecutor js = driver as IJavaScriptExecutor;
            string strmulti = "", Taxy = "";
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "", accountnumber = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new PhantomJSDriver();
            //driver = new ChromeDriver();
            using (driver = new PhantomJSDriver()) //ChromeDriver
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("https://qpublic.schneidercorp.com/Application.aspx?App=YorkCountySC&Layer=Parcels&PageType=Search");
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='appBody']/div[4]/div/div/div[3]/a[1]")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                    }
                    catch { }
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='appBody']/div[5]/div/div/div[3]/a")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                    }
                    catch { }
                    //Thread.Sleep(3000);
                    if (searchType == "titleflex")
                    {
                        string address = streetNo + " " + direction + " " + streetName + " " + streetType + " " + accountNo;
                        gc.TitleFlexSearch(orderNumber, "", "", address.Trim(), "SC", "York");
                        if (HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].Equals("Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_YorkSC"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_txtAddress")).SendKeys(streetNo + " " + streetName);
                        gc.CreatePdf_WOP(orderNumber, "Address Search Before", driver, "SC", "York");
                        driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search After", driver, "SC", "York");
                        try
                        {
                            strmulti = GlobalClass.Before(driver.FindElement(By.Id("ctlBodyPane_ctl00_txtResultCount")).Text.Trim(), " Results");
                            if (Convert.ToInt32(strmulti) > 25)
                            {
                                HttpContext.Current.Session["multiParcel_YorkSC_Maximum"] = "Maximum";
                                return "Maximum";
                            }
                            IWebElement Imultitable = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_ctl01_gvwParcelResults']/tbody"));
                            IList<IWebElement> ImutiRow = Imultitable.FindElements(By.TagName("tr"));
                            IList<IWebElement> ImultiTD;
                            foreach (IWebElement multi in ImutiRow)
                            {
                                ImultiTD = multi.FindElements(By.TagName("td"));
                                if (ImultiTD.Count != 0 && !multi.Text.Contains("Address"))
                                {
                                    string strmultiDetails = ImultiTD[2].Text + "~" + ImultiTD[3].Text;
                                    gc.insert_date(orderNumber, ImultiTD[1].Text, 1541, strmultiDetails, 1, DateTime.Now);
                                }
                            }
                            HttpContext.Current.Session["multiParcel_YorkSC"] = "Yes";
                            driver.Quit();
                            return "MultiParcel";

                        }
                        catch { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("ctlBodyPane_noDataList_pnlNoResults")).Text;
                            if (nodata.Contains("No results match your search criteria."))
                            {
                                HttpContext.Current.Session["Nodata_YorkSC"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.Id("ctlBodyPane_ctl02_ctl01_txtParcelID")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Before Search", driver, "SC", "York");
                        driver.FindElement(By.Id("ctlBodyPane_ctl02_ctl01_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Aftere Pdf", driver, "SC", "York");
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("ctlBodyPane_noDataList_pnlNoResults")).Text;
                            if (nodata.Contains("No results match your search criteria."))
                            {
                                HttpContext.Current.Session["Nodata_YorkSC"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    else if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("ctlBodyPane_ctl00_ctl01_txtName")).SendKeys(ownerName);
                        gc.CreatePdf_WOP(orderNumber, "OwnerName Search Before", driver, "SC", "York");
                        driver.FindElement(By.Id("ctlBodyPane_ctl00_ctl01_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "OwnerName Search After", driver, "SC", "York");

                        try
                        {
                            strmulti = GlobalClass.Before(driver.FindElement(By.Id("ctlBodyPane_ctl00_txtResultCount")).Text.Trim(), " Results");
                            if (Convert.ToInt32(strmulti) > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Paulding_Maximum"] = "Maximum";
                                return "Maximum";
                            }
                            IWebElement Imultitable = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_ctl01_gvwParcelResults']/tbody"));
                            IList<IWebElement> ImutiRow = Imultitable.FindElements(By.TagName("tr"));
                            IList<IWebElement> ImultiTD;
                            foreach (IWebElement multi in ImutiRow)
                            {
                                ImultiTD = multi.FindElements(By.TagName("td"));
                                if (ImultiTD.Count != 0 && !multi.Text.Contains("Address"))
                                {
                                    string strmultiDetails = ImultiTD[2].Text + "~" + ImultiTD[3].Text;
                                    gc.insert_date(orderNumber, ImultiTD[1].Text, 1541, strmultiDetails, 1, DateTime.Now);
                                }
                            }
                            HttpContext.Current.Session["multiParcel_YorkSC"] = "Yes";
                            driver.Quit();
                            return "MultiParcel";
                        }
                        catch { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("ctlBodyPane_noDataList_pnlNoResults")).Text;
                            if (nodata.Contains("No results match your search criteria."))
                            {
                                HttpContext.Current.Session["Nodata_YorkSC"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    //Property Details

                    string Owner = "", ParcelNumber = "", Propertyaddress = "", Mailingaddress = "", LegalDescription = "", Classcode = "";
                    try
                    {
                        string bulktext1 = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl01_mSection']/div")).Text.Trim();
                        string[] Propdata1 = bulktext1.Split('\r');

                        if (Propdata1.Count() == 4)
                        {
                            Owner = Propdata1[0].Replace("\r\n", "").Trim() + "&" + Propdata1[1].Replace("\r\n", "").Trim();
                            Mailingaddress = Propdata1[2].Replace("\r\n", "").Trim() + Propdata1[3].Replace("\r\n", "").Trim();
                        }
                        if (Propdata1.Count() == 3)
                        {
                            Owner = Propdata1[0].Replace("\r\n", "").Trim();
                            Mailingaddress = Propdata1[1].Replace("\r\n", "").Trim() + Propdata1[2].Replace("\r\n", "").Trim();
                        }
                        if (Propdata1.Count() == 2)
                        {
                            Mailingaddress = Propdata1[1].Replace("\r\n", "").Trim() + Propdata1[2].Replace("\r\n", "").Trim();
                        }
                    }
                    catch { }
                    ParcelNumber = driver.FindElement(By.Id("ctlBodyPane_ctl00_ctl01_lblParcelNumber")).Text.Trim();
                    gc.CreatePdf(orderNumber, ParcelNumber, "Property Details and Assessment Details Pdf", driver, "SC", "York");
                    Propertyaddress = driver.FindElement(By.Id("ctlBodyPane_ctl00_ctl01_lblLocationAddr")).Text.Trim();
                    LegalDescription = driver.FindElement(By.Id("ctlBodyPane_ctl00_ctl01_lblDescription")).Text.Trim();
                    Classcode = driver.FindElement(By.Id("ctlBodyPane_ctl00_ctl01_lblClass")).Text.Trim();
                    string PropertyDetails = Owner.Trim() + "~" + Propertyaddress.Trim() + "~" + Mailingaddress.Trim() + "~" + LegalDescription.Trim() + "~" + Classcode.Trim();
                    gc.insert_date(orderNumber, ParcelNumber, 1539, PropertyDetails, 1, DateTime.Now);
                    //Assessment Details
                    string AssessmentYear = "";
                    try
                    {
                        AssessmentYear = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl11_ctl01_grdValuation']/thead/tr/th[3]")).Text.Trim();
                    }
                    catch { }
                    // gc.CreatePdf(orderNumber, ParcelNumber, "Assessment Details ", driver, "GA", "Paulding");
                    try
                    {
                        string AssessmentTitle = "", AssessmentValue = "";
                        IWebElement tbcurasses12 = driver.FindElement(By.Id("ctlBodyPane_ctl02_mSection"));
                        IList<IWebElement> TRcurasses2 = tbcurasses12.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDmulti12;
                        foreach (IWebElement row in TRcurasses2)
                        {
                            TDmulti12 = row.FindElements(By.TagName("td"));
                            if (TDmulti12.Count != 0 && !row.Text.Contains("Value Information"))
                            {
                                AssessmentTitle += TDmulti12[0].Text + "~";
                                AssessmentValue += TDmulti12[1].Text + "~";
                            }
                        }
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + "Year" + "~" + AssessmentTitle.Remove(AssessmentTitle.Length - 1, 1) + "' where Id = '" + 1540 + "'");
                        gc.insert_date(orderNumber, ParcelNumber, 1540, AssessmentYear + "~" + AssessmentValue.Remove(AssessmentValue.Length - 1, 1), 1, DateTime.Now);
                    }
                    catch { }
                    //Tax Authority
                    string Taxauthority = "", Taxauthority1 = "";
                    try
                    {
                        driver.Navigate().GoToUrl("https://www.yorkcountygov.com/Directory.aspx?did=28");
                        Taxauthority1 = driver.FindElement(By.Id("CityDirectoryLeftMargin")).Text.Trim();
                        Taxauthority = gc.Between(Taxauthority1.Replace("\r\n", " "), "Mailing Address:", "Fax:").Trim();
                        gc.CreatePdf(orderNumber, ParcelNumber, "Tax Authority Details", driver, "SC", "York");
                    }
                    catch { }
                    //Tax Information Details

                    driver.Navigate().GoToUrl("https://onlinetaxes.yorkcountygov.com/taxes#/");
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, ParcelNumber, "Tax main page", driver, "SC", "York");
                    try
                    {//*[@id="cc5f8c90dc-b4cb-431b-90ee-10648f8df655"]/div/div/p[3]/button[1]
                        //*[@id="cc5f8c90dc-b4cb-431b-90ee-10648f8df655"]/div/div/p[3]/button[1]
                        driver.FindElement(By.XPath("//*[@id='cc5f8c90dc-b4cb-431b-90ee-10648f8df655']/div/div/p[3]/button[1]")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, ParcelNumber, "Tax accept click", driver, "SC", "York");
                    }
                    catch
                    { }
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[3]/button[2]")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                    }
                    catch { }
                    ////*[@id="searchBox"]
                    //try { 
                    //driver.Navigate().GoToUrl("https://onlinetaxes.yorkcountygov.com/taxes#/WildfireSearch");
                    //Thread.Sleep(3000);
                    //}catch { }
                    driver.FindElement(By.Id("searchBox")).SendKeys(ParcelNumber);
                    //driver.FindElement(By.XPath("//*[@id='searchBox']")).SendKeys(ParcelNumber);
                    Thread.Sleep(4000);
                    try
                    {
                        //*[@id="searchForm"]/div[1]/div/span/button
                        driver.FindElement(By.XPath("//*[@id='searchForm']/div[1]/div/span/button")).SendKeys(Keys.Enter);

                    }
                    catch { }
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, ParcelNumber, "Payment Details Pdf", driver, "SC", "York");
                    //Tax History Details Table
                    string TaxHistorydetails = "", TaxOwnerName = "", TaxYear = "", Receipt = "", Descripton = "", Type = "", Status = "", PaidDate = "";

                    try
                    {
                        IWebElement PaymentTB = driver.FindElement(By.XPath("//*[@id='cc5f8c90dc-b4cb-431b-90ee-10648f8df655']/div/div/div[4]/div[2]/table")); //Working on div[3]/div[2] Previously
                        IList<IWebElement> PaymentTR = PaymentTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> PaymentTD;

                        foreach (IWebElement Payment in PaymentTR)
                        {
                            PaymentTD = Payment.FindElements(By.TagName("td"));
                            if (PaymentTD.Count != 0 && !Payment.Text.Contains("Owner Name"))
                            {
                                TaxOwnerName = PaymentTD[0].Text;
                                TaxYear = PaymentTD[1].Text;
                                Receipt = PaymentTD[2].Text;
                                Descripton = PaymentTD[3].Text;
                                Type = PaymentTD[4].Text;
                                Status = PaymentTD[5].Text;
                                PaidDate = PaymentTD[6].Text;

                                TaxHistorydetails = TaxOwnerName + "~" + TaxYear + "~" + Receipt + "~" + Descripton + "~" + Type + "~" + Status + "~" + PaidDate + "~" + Taxauthority;
                                gc.insert_date(orderNumber, ParcelNumber, 1547, TaxHistorydetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    //Tax Information Details//
                    //***********************=========***********************//

                    string name = "", Taxyear = "", bill_no = "", amount = "", Del_details = "", delinaccount = "", delinparcelid = "";
                    //int p = 0;
                    for (int i = 1; i < 4; i++)
                    {
                        try
                        {
                            IWebElement Receipttable1 = driver.FindElement(By.XPath("//*[@id='cc5f8c90dc-b4cb-431b-90ee-10648f8df655']/div/div/div[4]/div[2]/table/tbody/tr[" + i + "]/td[9]/button")); //Working on div[3]/div[2] Previously
                            Receipttable1.Click();
                            Thread.Sleep(5000);
                            ////View Delinquent Details                            
                            try
                            {
                                IWebElement DeliquentTB = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/table/tbody"));
                                IList<IWebElement> DeliquentTR = DeliquentTB.FindElements(By.TagName("tr"));
                                IList<IWebElement> DeliquentTD;

                                foreach (IWebElement Deliquent in DeliquentTR)
                                {
                                    DeliquentTD = Deliquent.FindElements(By.TagName("td"));
                                    if (DeliquentTD.Count != 0)
                                    {
                                        name = DeliquentTD[0].Text.Trim();
                                        bill_no = DeliquentTD[1].Text.Trim();
                                        Taxyear = DeliquentTD[2].Text.Trim();
                                        amount = DeliquentTD[6].Text.Trim();

                                        Del_details = name + "~" + Taxyear + "~" + bill_no + "~" + amount;
                                        gc.CreatePdf(orderNumber, ParcelNumber, "Deliquent Details" + i, driver, "SC", "York");
                                        gc.insert_date(orderNumber, ParcelNumber, 1443, Del_details, 1, DateTime.Now);
                                        name = ""; Taxyear = ""; bill_no = ""; amount = ""; delinaccount = ""; delinparcelid = "";
                                    }
                                }
                            }
                            catch
                            { }
                            try
                            {
                                IWebElement DeliquentfootTB = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/table/tfoot"));
                                IList<IWebElement> DeliquentfootTR = DeliquentfootTB.FindElements(By.TagName("tr"));
                                IList<IWebElement> DeliquentfootTD;

                                foreach (IWebElement Deliquentfoot in DeliquentfootTR)
                                {
                                    DeliquentfootTD = Deliquentfoot.FindElements(By.TagName("th"));
                                    if (DeliquentfootTD.Count != 0)
                                    {
                                        string bill_no1 = DeliquentfootTD[0].Text;
                                        string amount1 = DeliquentfootTD[2].Text;

                                        string Del_details1 = "" + "~" + "" + "~" + bill_no1 + "~" + amount1;
                                        gc.insert_date(orderNumber, ParcelNumber, 1443, Del_details1, 1, DateTime.Now);
                                    }
                                }
                                gc.CreatePdf(orderNumber, ParcelNumber, "Delinquent" + i, driver, "SC", "York");
                            }
                            catch
                            { }

                            try
                            {
                                driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[2]/button[2]")).Click();
                                Thread.Sleep(2000);
                            }
                            catch
                            { }
                            string Parcelid = "", Taxinfownername1 = "", PayStatus = "", Paypaiddate = "", Effectivedate = "", Paidamount = "", BillType = "", BillTaxYearinfo = "", Billreceipt = "", Duedate = "", Basetaxamt = "", Penalty = "", Costs = "", Totaldue = "", Credit = "", Fees = "";
                            //Tax Information details1
                            IWebElement TaxTB2 = driver.FindElement(By.XPath("//*[@id='cc5f8c90dc-b4cb-431b-90ee-10648f8df655']/div/div/div/div/div[1]/div[2]"));
                            IList<IWebElement> TaxTR2 = TaxTB2.FindElements(By.TagName("div"));
                            IList<IWebElement> TaxTD2;
                            foreach (IWebElement Tax2 in TaxTR2)
                            {
                                if (Tax2.Text.Contains("Property Information"))
                                {
                                    Parcelid = gc.Between(Tax2.Text, "Parcel Number", "District").Trim();

                                }
                                if (Tax2.Text.Contains("Bill Information"))
                                {
                                    BillType = gc.Between(Tax2.Text, "Record Type", "Tax Year").Trim();
                                    BillTaxYearinfo = gc.Between(Tax2.Text, "Tax Year", "Receipt").Trim();
                                    Billreceipt = gc.Between(Tax2.Text, "Receipt", "Due Date").Trim();
                                    Duedate = GlobalClass.After(Tax2.Text, "Due Date").Trim();
                                }

                            }
                            //Tax information details
                            //Owner information and Payment Information
                            IWebElement TaxTB1 = driver.FindElement(By.XPath("//*[@id='cc5f8c90dc-b4cb-431b-90ee-10648f8df655']/div/div/div/div/div[1]/div[1]"));
                            IList<IWebElement> TaxTR1 = TaxTB1.FindElements(By.TagName("div"));
                            IList<IWebElement> TaxTD;
                            foreach (IWebElement Tax1 in TaxTR1)
                            {
                                if (Tax1.Text.Contains("Owner Information"))
                                {
                                    Taxinfownername1 = GlobalClass.After(Tax1.Text, "Owner Information").Trim();
                                    string[] Taxinfownernamesplit = Taxinfownername1.Split('\r');
                                    TaxOwnerName = Taxinfownernamesplit[0] + " " + Taxinfownernamesplit[2].Trim();
                                }
                                if (Tax1.Text.Contains("Payment Information"))
                                {
                                    PayStatus = gc.Between(Tax1.Text, "Status", "Last Payment Date").Trim();
                                    Paypaiddate = gc.Between(Tax1.Text, "Last Payment Date", "Postmark Date").Trim();
                                    Effectivedate = gc.Between(Tax1.Text, "Postmark Date", "Amount Paid").Trim();
                                    Paidamount = GlobalClass.After(Tax1.Text, "Amount Paid").Trim();
                                }
                                //}
                            }
                            string Taxinformationdetails2 = TaxOwnerName.Trim() + "~" + PayStatus.Trim() + "~" + Paypaiddate.Trim() + "~" + Effectivedate.Trim() + "~" + Paidamount.Trim() + "~" + BillType.Trim() + "~" + BillTaxYearinfo.Trim() + "~" + Billreceipt.Trim() + "~" + Duedate.Trim();
                            gc.insert_date(orderNumber, Parcelid, 1545, Taxinformationdetails2, 1, DateTime.Now);
                            gc.CreatePdf(orderNumber, ParcelNumber, "Payment Details Pdf" + BillTaxYearinfo, driver, "SC", "York");
                            //Property Infor and Bill Infor and Taxes
                            string Basetax = "", Credit1 = "", Fees1 = "", Penalty1 = "", Costs1 = "", Totaldue11 = "";
                            IWebElement TaxTB3 = driver.FindElement(By.XPath("//*[@id='cc5f8c90dc-b4cb-431b-90ee-10648f8df655']/div/div/div/div/div[1]/div[2]/div[3]/table/tbody"));
                            IList<IWebElement> TaxTR3 = TaxTB3.FindElements(By.TagName("tr"));
                            IList<IWebElement> TaxTD3;
                            foreach (IWebElement Tax2 in TaxTR3)
                            {

                                //TaxTR3 = Tax2.FindElements(By.TagName("tr"));
                                TaxTD3 = Tax2.FindElements(By.TagName("td"));
                                if (TaxTD3.Count == 2 && Tax2.Text.Trim() != "" && Tax2.Text.Trim().Contains("Base Taxes"))
                                {
                                    Basetax = TaxTD3[1].Text.Trim();
                                }
                                if (TaxTD3.Count == 2 && Tax2.Text.Trim() != "" && Tax2.Text.Trim().Contains("Credit"))
                                {
                                    Credit1 = TaxTD3[1].Text.Trim();
                                }
                                if (TaxTD3.Count == 2 && Tax2.Text.Trim() != "" && Tax2.Text.Trim().Contains("Fees"))
                                {
                                    Fees1 = TaxTD3[1].Text.Trim();
                                }
                                if (TaxTD3.Count == 2 && Tax2.Text.Trim() != "" && Tax2.Text.Trim().Contains("Penalty"))
                                {
                                    Penalty1 = TaxTD3[1].Text.Trim();
                                }
                                if (TaxTD3.Count == 2 && Tax2.Text.Trim() != "" && Tax2.Text.Trim().Contains("Costs"))
                                {
                                    Costs1 = TaxTD3[1].Text.Trim();

                                }
                                if (TaxTD3.Count == 2 && Tax2.Text.Trim() != "" && Tax2.Text.Trim().Contains("Total Due"))
                                {
                                    Totaldue11 = TaxTD3[1].Text.Trim();
                                }
                            }//Penalty1="", Costs1="",Totaldue11=""
                             //db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + Title.Remove(Value.Length - 1, 1) + "' where Id = '" + 1445 + "'");
                            string Taxinformationdetails = BillTaxYearinfo.Trim() + "~" + Basetax.Trim() + "~" + Credit1.Trim() + "~" + Fees1.Trim() + "~" + Penalty1.Trim() + "~" + Costs1.Trim() + "~" + Totaldue11.Trim();
                            gc.insert_date(orderNumber, ParcelNumber, 1445, Taxinformationdetails, 1, DateTime.Now);
                            //***Due Date Details**
                            string Ifpaidby = "", Penaltydue = "", Amountdue = "";
                            IWebElement DueTB = driver.FindElement(By.XPath("//*[@id='cc5f8c90dc-b4cb-431b-90ee-10648f8df655']/div/div/div/div/div[1]/div[3]/div[2]/table"));
                            IList<IWebElement> DueTR = DueTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> DueTH = DueTB.FindElements(By.TagName("th"));
                            IList<IWebElement> DueTD;

                            foreach (IWebElement Due in DueTR)
                            {
                                try
                                {
                                    DueTD = Due.FindElements(By.TagName("td"));
                                    DueTH = Due.FindElements(By.TagName("th"));

                                    if (DueTH.Count != 0)
                                    {
                                        Penaltydue = DueTH[1].Text;
                                        Amountdue = DueTH[2].Text;
                                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + "Tax Year~If Paid by~" + Penaltydue + "~" + Amountdue + "' where Id = '" + 1546 + "'");
                                    }
                                    if (DueTD.Count != 0)
                                    {
                                        Ifpaidby = DueTD[0].Text;
                                        Penaltydue = DueTD[1].Text;
                                        Amountdue = DueTD[2].Text;
                                        string Duedatedetails = BillTaxYearinfo + "~" + Ifpaidby + "~" + Penaltydue + "~" + Amountdue;
                                        gc.insert_date(orderNumber, Parcelid, 1546, Duedatedetails, 1, DateTime.Now);
                                    }
                                }
                                catch { }
                            }
                            //Tax Bill Download
                            try
                            {
                                driver.FindElement(By.XPath("//*[@id='cc5f8c90dc-b4cb-431b-90ee-10648f8df655']/div/div/div/ul/li[2]/a/div")).Click();
                                Thread.Sleep(5000);
                                gc.CreatePdf(orderNumber, ParcelNumber, "Tax Bill Details" + BillTaxYearinfo, driver, "SC", "York");
                                //Thread.Sleep(5000);
                            }
                            catch { }
                            driver.Navigate().Back();
                        }
                        catch { }
                    }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    CitytaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "SC", "York", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "SC", "York");
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
//View And Print Receipt
//try
//{
//    driver.FindElement(By.XPath("//*[@id='cc5f8c90dc-b4cb-431b-90ee-10648f8df655']/div/div/div/div/div[2]/div/div[1]/a")).Click();
//    Thread.Sleep(9000);
//    gc.CreatePdf(orderNumber, ParcelNumber, "View Print Receipt" + BillTaxYearinfo, driver, "SC", "York");
//}
//catch { }

//Pdf Download Details
//try
//{
//    var chromeOptions = new ChromeOptions();
//    var downloadDirectory = "D:\\AutoPdf\\";
//    chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
//    chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
//    chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
//    chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
//    var driver1 = new ChromeDriver(chromeOptions);
//    driver1.Navigate().GoToUrl(driver.Url);
//    driver.FindElement(By.XPath("//*[@id='cc5f8c90dc-b4cb-431b-90ee-10648f8df655']/div/div/div/ul/li[2]/a/div")).Click();
//    Thread.Sleep(9000);
//    string fileName = "";
//    IWebElement Receipttable = driver.FindElement(By.XPath("//*[@id='cc5f8c90dc-b4cb-431b-90ee-10648f8df655']/div/div/div/div/div[2]/div/div[1]/a"));
//    //IJavaScriptExecutor js2 = driver as IJavaScriptExecutor;
//    //js1.ExecuteScript("arguments[0].click();", Receipttable);
//    //Thread.Sleep(9000);
//    string BillTax2 = Receipttable.GetAttribute("href");
//    Thread.Sleep(9000);
//    fileName = GlobalClass.After(BillTax2, "Reports/").Trim();
//    Thread.Sleep(3000);
//    Receipttable.Click();
//    gc.AutoDownloadFile(orderNumber, parcelNumber, "SC", "York", fileName);
//}
//catch { }
