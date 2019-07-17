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
    public class WebDriver_HenryGA
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;

        public string FTP_HenryGA(string Address, string unitno, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            IJavaScriptExecutor js = driver as IJavaScriptExecutor;
            string strmulti = "", Taxy = "", LegalDescription = "";
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "", accountnumber = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new PhantomJSDriver();
            //driver = new ChromeDriver();
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("http://qpublic7.qpublic.net/ga_henry_search.php");
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
                        gc.TitleFlexSearch(orderNumber, "", "", Address.Trim(), "GA", "Henry");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_HenryGA"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        driver.FindElement(By.Id("ctlBodyPane_ctl02_ctl01_txtAddress")).SendKeys(Address);
                        gc.CreatePdf_WOP(orderNumber, "Address Search Before", driver, "GA", "Henry");
                        driver.FindElement(By.Id("ctlBodyPane_ctl02_ctl01_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search After", driver, "GA", "Henry");
                        try
                        {
                            strmulti = GlobalClass.Before(driver.FindElement(By.Id("ctlBodyPane_ctl00_txtResultCount")).Text.Trim(), "Results");
                            if (Convert.ToInt32(strmulti) > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Henry_Maximum"] = "Maximum";
                                return "Maximum";
                            }
                            IWebElement Imultitable = driver.FindElement(By.Id("ctlBodyPane_ctl00_ctl01_gvwParcelResults"));
                            IList<IWebElement> ImutiRow = Imultitable.FindElements(By.TagName("tr"));
                            IList<IWebElement> ImultiTD;
                            foreach (IWebElement multi in ImutiRow)
                            {
                                ImultiTD = multi.FindElements(By.TagName("td"));
                                if (ImultiTD.Count != 0 && !multi.Text.Contains("Address"))
                                {
                                    string strmultiDetails = ImultiTD[2].Text + "~" + ImultiTD[3].Text;
                                    gc.insert_date(orderNumber, ImultiTD[1].Text, 1647, strmultiDetails, 1, DateTime.Now);
                                }
                            }
                            HttpContext.Current.Session["multiParcel_Henry"] = "Yes";
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
                                HttpContext.Current.Session["Nodata_HenryGA"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_txtParcelID")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Before Search", driver, "GA", "Henry");
                        driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Aftere Pdf", driver, "GA", "Henry");
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("ctlBodyPane_noDataList_pnlNoResults")).Text;
                            if (nodata.Contains("No results match your search criteria."))
                            {
                                HttpContext.Current.Session["Nodata_HenryGA"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    else if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("ctlBodyPane_ctl00_ctl01_txtName")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "OwnerName Search Before", driver, "GA", "Henry");
                        driver.FindElement(By.Id("ctlBodyPane_ctl00_ctl01_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "OwnerName Search After", driver, "GA", "Henry");

                        try
                        {
                            strmulti = GlobalClass.Before(driver.FindElement(By.Id("ctlBodyPane_ctl00_txtResultCount")).Text.Trim(), " Results");
                            if (Convert.ToInt32(strmulti) > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Henry_Maximum"] = "Maximum";
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
                                    gc.insert_date(orderNumber, ImultiTD[1].Text, 1647, strmultiDetails, 1, DateTime.Now);
                                }
                            }
                            HttpContext.Current.Session["multiParcel_Henry"] = "Yes";
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
                                HttpContext.Current.Session["Nodata_HenryGA"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    //Property Details
                    string Owner = "", MailingAddress = "", ParcelNumber = "", Propertytype = "", PropertyAddress = "", ZipCode = "", Class = "", TaxDistrict = "", MillageRate = "", Acres = "", Neighborhood = "", HomesteadExemption = "", LandlotAndDistrictAndSection = "", Subdivision = "", YearBuilt = "";
                    try
                    {
                        string bulktext1 = driver.FindElement(By.Id("ctlBodyPane_ctl02_ctl01_lstOwner")).Text.Trim();
                        string[] Propdata1 = bulktext1.Split('\r');

                        if (Propdata1.Count() == 4)
                        {
                            Owner = Propdata1[0].Replace("\r\n", "").Trim() + "&" + Propdata1[1].Replace("\r\n", "").Trim();
                            MailingAddress = Propdata1[2].Replace("\r\n", "").Trim() + "  " + Propdata1[3].Replace("\r\n", "").Trim();
                        }
                        if (Propdata1.Count() == 3)
                        {
                            Owner = Propdata1[0].Replace("\r\n", "").Trim();
                            MailingAddress = Propdata1[1].Replace("\r\n", "").Trim() + "  " + Propdata1[2].Replace("\r\n", "").Trim();
                        }
                        if (Propdata1.Count() == 2)
                        {
                            MailingAddress = Propdata1[1].Replace("\r\n", "").Trim() + "  " + Propdata1[2].Replace("\r\n", "").Trim();
                        }
                        ParcelNumber = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_mSection']/div/div/div/table/tbody/tr[1]/td[2]")).Text.Trim();
                        //gc.CreatePdf(orderNumber, ParcelNumber, "Property Details and Assessment Details Pdf", driver, "GA", "Henry");
                        PropertyAddress = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_mSection']/div/div/div/table/tbody/tr[2]/td[2]")).Text.Trim();
                        Propertytype = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_mSection']/div/div/div/table/tbody/tr[4]/td[2]")).Text.Trim();
                        try
                        {
                            YearBuilt = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl06_mSection']/div/div/div[1]/table/tbody/tr[3]/td[2]")).Text.Trim();
                        }
                        catch { }

                    }
                    catch { }
                    //Assessment Details                    
                    try
                    {
                        string AssessmentTitle = "", AssessmentValue = "";
                        IWebElement tbcurasses12 = driver.FindElement(By.Id("ctlBodyPane_ctl03_ctl01_grdValuation_grdYearData"));
                        IList<IWebElement> TRcurasses2 = tbcurasses12.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDmulti12;
                        IList<IWebElement> THmulti12;
                        foreach (IWebElement row in TRcurasses2)
                        {
                            TDmulti12 = row.FindElements(By.TagName("td"));
                            THmulti12 = row.FindElements(By.TagName("th"));
                            if (TDmulti12.Count != 0 && !row.Text.Contains("Year"))
                            {
                                AssessmentValue = TDmulti12[0].Text + "~" + TDmulti12[1].Text + "~" + TDmulti12[2].Text + "~" + TDmulti12[3].Text + "~" + TDmulti12[4].Text;
                                gc.insert_date(orderNumber, ParcelNumber, 1642, AssessmentValue, 1, DateTime.Now);
                            }
                            if (THmulti12.Count != 0 && row.Text.Contains("Assessed Year"))
                            {
                                AssessmentTitle = THmulti12[0].Text + "~" + THmulti12[1].Text + "~" + THmulti12[2].Text + "~" + THmulti12[3].Text + "~" + THmulti12[4].Text;

                            }
                        }
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + AssessmentTitle + "' where Id = '" + 1642 + "'");

                    }
                    catch { }
                    //Tax Authority
                    string Taxauthority = "";
                    try
                    {
                        //string Taxauthority1 = "", Taxauthority2 = "", Taxauthority3 = "", Taxauthority4 = "";
                        driver.Navigate().GoToUrl("https://www.henrytc.org/#/");
                        Thread.Sleep(2000);
                        IWebElement taxo = driver.FindElement(By.Id("bottom2"));
                        string[] Taxauthority1 = taxo.Text.Split('\r');
                        gc.CreatePdf(orderNumber, ParcelNumber, "TaxAuthority Pdf", driver, "GA", "Henry");
                        Taxauthority = Taxauthority1[1] + " " + Taxauthority1[2] + " " + Taxauthority1[3] + " " + Taxauthority1[4];

                    }
                    catch { }
                    //Tax information Details
                    string AccountAndRealkey = "";
                    try
                    {
                        driver.Navigate().GoToUrl("https://www.henrytc.org/taxes.html#/WildfireSearch");
                        Thread.Sleep(2000);
                        try
                        {///html/body/div[1]/div/div/div[3]/button[2]
                            driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[3]/button[2]")).SendKeys(Keys.Enter);
                            Thread.Sleep(2000);
                        }
                        catch
                        { }
                        driver.FindElement(By.Id("searchBox")).SendKeys(ParcelNumber);
                        Thread.Sleep(4000);
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='searchForm']/div[1]/div/span/button")).SendKeys(Keys.Enter);
                        }
                        catch { }
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, ParcelNumber, "Payment Details Pdf", driver, "GA", "Henry");

                    }
                    catch { }
                    //Tax Payment Receipt Details Table
                    string Paymentdetails = "", TaxOwnerName = "", TaxYear = "", Bill = "", MapandParcel = "", Taxtype = "", PaidDate = "", Paid = "";

                    try
                    {
                        IWebElement PaymentTB = null;
                        try
                        {
                            PaymentTB = driver.FindElement(By.XPath("//*[@id='avalon']/div/div[3]/div[2]/table"));
                        }
                        catch { }
                        try
                        {
                            if (PaymentTB == null)
                            {
                                PaymentTB = driver.FindElement(By.XPath("//*[@id='avalon']/div/div[4]/div[2]/table"));
                            }
                        }
                        catch { }
                        //IWebElement PaymentTB = driver.FindElement(By.XPath("//*[@id='avalon']/div/div[3]/div[2]/table"));
                        IList<IWebElement> PaymentTR = PaymentTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> PaymentTD;

                        foreach (IWebElement Payment in PaymentTR)
                        {
                            PaymentTD = Payment.FindElements(By.TagName("td"));
                            if (PaymentTD.Count != 0 && !Payment.Text.Contains("Owner Name"))
                            {
                                TaxOwnerName = PaymentTD[0].Text;
                                TaxYear = PaymentTD[1].Text;
                                Bill = PaymentTD[2].Text;
                                MapandParcel = PaymentTD[3].Text;
                                Taxtype = PaymentTD[4].Text;
                                PaidDate = PaymentTD[6].Text;
                                Paid = PaymentTD[5].Text;
                                //Tax OwnerName~TaxYear~Bil Numberl~Property Address~Tax Type~Status~Paid Date
                                Paymentdetails = TaxOwnerName + "~" + TaxYear + "~" + Bill + "~" + MapandParcel + "~" + Taxtype + "~" + Paid + "~" + PaidDate;
                                gc.insert_date(orderNumber, ParcelNumber, 1643, Paymentdetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    //Tax Bill Details
                    string name = "", Taxyear = "", bill_no = "", amount = "", Del_details = "", delinaccount = "", delinparcelid = "";
                    try
                    {
                        //Tax Info Details
                        IWebElement Receipttable = null;
                        try
                        {
                            Receipttable = driver.FindElement(By.XPath("//*[@id='avalon']/div/div[3]/div[2]/table/tbody"));
                        }
                        catch { }
                        try
                        {
                            if (Receipttable == null)
                            {
                                Receipttable = driver.FindElement(By.XPath("//*[@id='avalon']/div/div[4]/div[2]/table/tbody"));
                            }
                        }
                        catch { }
                        //IWebElement Receipttable = driver.FindElement(By.XPath("//*[@id='avalon']/div/div[3]/div[2]/table/tbody"));
                        IList<IWebElement> ReceipttableRow = Receipttable.FindElements(By.TagName("tr"));
                        int rowcount = ReceipttableRow.Count;

                        for (int p = 1; p <= rowcount; p++)
                        {
                            if (p < 4)
                            {
                                ////*[@id="avalon"]/div/div[3]/div[2]/table/tbody/tr[1]/td[9]
                                try
                                {
                                    driver.FindElement(By.XPath("//*[@id='avalon']/div/div[3]/div[2]/table/tbody/tr[" + p + "]/td[9]/button")).Click();
                                }
                                catch { }
                                try
                                {
                                    driver.FindElement(By.XPath("//*[@id='avalon']/div/div[4]/div[2]/table/tbody/tr[" + p + "]/td[9]/button")).Click();
                                }
                                catch { }
                                Thread.Sleep(2000);
                                try
                                {
                                    driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[2]/button[2]")).Click();
                                    Thread.Sleep(2000);
                                }
                                catch
                                { }
                                gc.CreatePdf(orderNumber, ParcelNumber, "Overview & Pay Pdf" + p, driver, "GA", "Henry");

                                Thread.Sleep(6000);
                                //View Delinquent Details
                                try
                                {
                                    driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[1]/div[3]/button[4]")).Click();
                                    IWebElement DeliquentTB = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/table/tbody"));
                                    IList<IWebElement> DeliquentTR = DeliquentTB.FindElements(By.TagName("tr"));
                                    IList<IWebElement> DeliquentTD;

                                    foreach (IWebElement Deliquent in DeliquentTR)
                                    {
                                        DeliquentTD = Deliquent.FindElements(By.TagName("td"));
                                        if (DeliquentTD.Count != 0)
                                        {
                                            name = DeliquentTD[0].Text.Trim();
                                            Taxyear = DeliquentTD[1].Text.Trim();
                                            bill_no = DeliquentTD[2].Text.Trim();
                                            delinaccount = DeliquentTD[3].Text.Trim();
                                            delinparcelid = DeliquentTD[4].Text.Trim();
                                            amount = DeliquentTD[6].Text.Trim();

                                            Del_details = name + "~" + Taxyear + "~" + bill_no + "~" + delinaccount + "~" + delinparcelid + "~" + amount;
                                            gc.CreatePdf(orderNumber, ParcelNumber, "Deliquent Details" + delinaccount, driver, "GA", "Henry");
                                            gc.insert_date(orderNumber, ParcelNumber, 1646, Del_details, 1, DateTime.Now);
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

                                            string Del_details1 = "" + "~" + "" + "~" + "" + "~" + "" + "~" + bill_no1 + "~" + amount1;
                                            gc.insert_date(orderNumber, ParcelNumber, 1646, Del_details1, 1, DateTime.Now);
                                        }
                                    }
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

                                string Taxinfownername = "", Mailingadd = "", AccountNumber = "", RecordType = "", BillNumber = "", BillTaxYear = "", taxown = "";
                                string DueDate = "", PaymentStatus = "", LastPaymentDate = "", TotalAmountPaid = "", TotalDuePayment = "";
                                //Tax information details
                                try
                                {
                                    IWebElement owne = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[1]/div[1]"));
                                    string[] ownesplit = owne.Text.Split('\r');
                                    Taxinfownername = ownesplit[1].Trim();
                                    try
                                    {
                                        Mailingadd = ownesplit[3].Trim() + " " + ownesplit[6].Trim();
                                    }
                                    catch { }
                                    //Bill information
                                    IWebElement TaxTB = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[2]/div[2]/table/tbody"));
                                    IList<IWebElement> TaxTR = TaxTB.FindElements(By.TagName("tr"));
                                    IList<IWebElement> TaxTD;
                                    foreach (IWebElement Tax1 in TaxTR)
                                    {
                                        TaxTD = Tax1.FindElements(By.TagName("td"));
                                        if (TaxTD.Count != 0 && Tax1.Text != "")
                                        {
                                            if (Tax1.Text.Contains("Record Type"))
                                            {
                                                RecordType = TaxTD[1].Text;
                                            }
                                            if (Tax1.Text.Contains("Tax Year"))
                                            {
                                                BillTaxYear = TaxTD[1].Text;
                                            }
                                            if (Tax1.Text.Contains("Bill Number"))
                                            {
                                                BillNumber = TaxTD[1].Text;
                                            }
                                            if (Tax1.Text.Contains("Account Number"))
                                            {
                                                AccountNumber = TaxTD[1].Text;
                                            }
                                            if (Tax1.Text.Contains("Due Date"))
                                            {
                                                DueDate = TaxTD[1].Text;
                                            }
                                        }
                                    }
                                    //Property information
                                    string Propertyid = "", District = "", Propertyadd = "", Assessedvalu = "", Appriasedval = "";
                                    IWebElement TaxTB2 = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[2]/div[1]/table/tbody"));
                                    IList<IWebElement> TaxTR2 = TaxTB2.FindElements(By.TagName("tr"));
                                    IList<IWebElement> TaxTD2;
                                    foreach (IWebElement Tax2 in TaxTR2)
                                    {
                                        TaxTD2 = Tax2.FindElements(By.TagName("td"));
                                        if (TaxTD2.Count != 0 && Tax2.Text != "")
                                        {
                                            if (Tax2.Text.Contains("Parcel Number"))
                                            {
                                                Propertyid = TaxTD2[1].Text;
                                            }
                                            if (Tax2.Text.Contains("District"))
                                            {
                                                District = TaxTD2[1].Text + " ";
                                            }
                                            if (Tax2.Text.Contains("Description"))
                                            {
                                                LegalDescription = TaxTD2[1].Text + " ";
                                            }
                                            if (Tax2.Text.Contains("Property Address"))
                                            {
                                                Propertyadd = TaxTD2[1].Text + " ";
                                            }
                                            if (Tax2.Text.Contains("Assessed Value"))
                                            {
                                                Assessedvalu = TaxTD2[1].Text + " ";
                                            }
                                            if (Tax2.Text.Contains("Appraised Value"))
                                            {
                                                Appriasedval = TaxTD2[1].Text + " ";
                                            }
                                        }
                                    }
                                    //Taxes Information
                                    string BaseTaxes = "", Penalty = "", Interest = "", Otherfees = "", TotaldueTax = "", BackTaxes = "", Yeartotaldue = "";
                                    IWebElement TaxTB3 = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[2]/div[3]/table/tbody"));
                                    IList<IWebElement> TaxTR3 = TaxTB3.FindElements(By.TagName("tr"));
                                    IList<IWebElement> TaxTD3;
                                    foreach (IWebElement Tax3 in TaxTR3)
                                    {
                                        TaxTD3 = Tax3.FindElements(By.TagName("td"));

                                        if (TaxTD3.Count != 0 && Tax3.Text != "")
                                        {
                                            if (Tax3.Text.Contains("Base Taxes"))
                                            {
                                                BaseTaxes = TaxTD3[1].Text;
                                            }
                                            if (Tax3.Text.Contains("Penalty"))
                                            {
                                                Penalty = TaxTD3[1].Text + " ";
                                            }
                                            if (Tax3.Text.Contains("Interest"))
                                            {
                                                Interest = TaxTD3[1].Text;
                                            }
                                            if (Tax3.Text.Contains("Other Fees"))
                                            {
                                                Otherfees = TaxTD3[1].Text;
                                            }
                                            if (Tax3.Text.Contains("Total Due"))
                                            {
                                                try
                                                {
                                                    if (TaxTD3[0].Text.Trim().Count() > 9)
                                                    {
                                                        string Yeartotaldue1 = TaxTD3[1].Text;
                                                        Yeartotaldue = BillTaxYear.Trim() + "   " + Yeartotaldue1.Trim();
                                                    }
                                                }
                                                catch { }

                                            }
                                            if (Tax3.Text.Contains("Back Taxes"))
                                            {
                                                BackTaxes = TaxTD3[1].Text + " ";
                                            }
                                            if (Tax3.Text.Contains("Total Due") && !Tax3.Text.Contains(BillTaxYear))
                                            {
                                                TotaldueTax = TaxTD3[1].Text + " ";
                                            }
                                        }
                                    }
                                    //Payment Information

                                    IWebElement TaxTB4 = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[1]/div[2]/table/tbody"));
                                    IList<IWebElement> TaxTR4 = TaxTB4.FindElements(By.TagName("tr"));
                                    IList<IWebElement> TaxTD4;
                                    foreach (IWebElement Tax4 in TaxTR4)
                                    {
                                        TaxTD4 = Tax4.FindElements(By.TagName("td"));
                                        if (TaxTD4.Count != 0 && Tax4.Text != "")
                                        {

                                            if (Tax4.Text.Contains("Status"))
                                            {
                                                PaymentStatus = TaxTD4[1].Text + " ";
                                            }
                                            if (Tax4.Text.Contains("Last Payment Date"))
                                            {
                                                LastPaymentDate = TaxTD4[1].Text;
                                            }
                                            if (Tax4.Text.Contains("Amount Paid"))
                                            {
                                                TotalAmountPaid = TaxTD4[1].Text + " ";
                                            }
                                        }
                                    }
                                    string Taxinfo_details1 = Taxinfownername + "~" + Propertyadd + "~" + Mailingadd + "~" + District + "~" + Assessedvalu + "~" + Appriasedval + "~" + PaymentStatus + "~" + PaidDate + "~" + TotalAmountPaid + "~" + RecordType + "~" + BillTaxYear + "~" + BillNumber + "~" + AccountNumber + "~" + DueDate + "~" + BaseTaxes + "~" + Penalty + "~" + Interest + "~" + Otherfees + "~" + Yeartotaldue + "~" + BackTaxes + "~" + TotaldueTax + "~" + Taxauthority;
                                    gc.insert_date(orderNumber, Propertyid, 1644, Taxinfo_details1, 1, DateTime.Now);
                                    string Authority = "", Adjusted = "", Assessd = "", Exemptions = "", Taxableval = "", Millage = "", Gross = "", Credit = "", Nettax = "";
                                    //Tax Distribution Details
                                    IWebElement TaxTB5 = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[3]/div/table"));
                                    IList<IWebElement> TaxTR5 = TaxTB5.FindElements(By.TagName("tr"));
                                    IList<IWebElement> TaxTD5;
                                    IList<IWebElement> TaxTH5;
                                    foreach (IWebElement Tax5 in TaxTR5)
                                    {
                                        TaxTD5 = Tax5.FindElements(By.TagName("td"));
                                        TaxTH5 = Tax5.FindElements(By.TagName("th"));
                                        if (TaxTD5.Count != 0 && Tax5.Text != "")
                                        {
                                            Authority = TaxTD5[0].Text.Trim();
                                            Adjusted = TaxTD5[1].Text.Trim();
                                            Assessd = TaxTD5[2].Text.Trim();
                                            Exemptions = TaxTD5[3].Text.Trim();
                                            Taxableval = TaxTD5[4].Text.Trim();
                                            Millage = TaxTD5[5].Text.Trim();
                                            Gross = TaxTD5[6].Text.Trim();
                                            Credit = TaxTD5[7].Text.Trim();
                                            Nettax = TaxTD5[8].Text.Trim();
                                            string Taxinfo_details2 = BillTaxYear + "~" + Authority + "~" + Adjusted + "~" + Assessd + "~" + Exemptions + "~" + Taxableval + "~" + Millage + "~" + Gross + "~" + Credit + "~" + Nettax;
                                            gc.insert_date(orderNumber, Propertyid, 1645, Taxinfo_details2, 1, DateTime.Now);

                                        }
                                        if (TaxTH5.Count != 0 && Tax5.Text != "" && !Tax5.Text.Contains("Entity"))
                                        {
                                            Authority = TaxTH5[0].Text.Trim();
                                            Adjusted = TaxTH5[1].Text.Trim();
                                            Assessd = TaxTH5[2].Text.Trim();
                                            Exemptions = TaxTH5[3].Text.Trim();
                                            Taxableval = TaxTH5[4].Text.Trim();
                                            Millage = TaxTH5[5].Text.Trim();
                                            Gross = TaxTH5[6].Text.Trim();
                                            Credit = TaxTH5[7].Text.Trim();
                                            Nettax = TaxTH5[8].Text.Trim();
                                            string Taxinfo_details3 = BillTaxYear + "~" + Authority + "~" + Adjusted + "~" + Assessd + "~" + Exemptions + "~" + Taxableval + "~" + Millage + "~" + Gross + "~" + Credit + "~" + Nettax;
                                            gc.insert_date(orderNumber, Propertyid, 1645, Taxinfo_details3, 1, DateTime.Now);

                                        }
                                    }

                                }
                                catch { }
                                //Tax Bill
                                driver.FindElement(By.XPath("//*[@id='avalon']/div/div/ul/li[2]/a")).Click();
                                Thread.Sleep(9000);
                                gc.CreatePdf(orderNumber, ParcelNumber, "Tax Bill Details" + BillTaxYear, driver, "GA", "Henry");
                                //View And Print Receipt
                                try
                                {
                                    driver.FindElement(By.XPath("//*[@id='avalon']/div/div/ul/li[3]/a")).Click();
                                    Thread.Sleep(9000);
                                    gc.CreatePdf(orderNumber, ParcelNumber, "View Print Receipt" + BillTaxYear, driver, "GA", "Henry");
                                }
                                catch { }
                                Thread.Sleep(5000);
                                driver.Navigate().Back();
                            }
                        }
                    }
                    catch { }

                    string PropertyDetails = Owner.Trim() + "~" + PropertyAddress.Trim() + "~" + MailingAddress.Trim() + "~" + Propertytype.Trim() + "~" + YearBuilt.Trim() + "~" + LegalDescription.Trim();
                    gc.insert_date(orderNumber, ParcelNumber, 1641, PropertyDetails, 1, DateTime.Now);

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    CitytaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "GA", "Henry", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);


                    driver.Quit();
                    gc.mergpdf(orderNumber, "GA", "Henry");
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