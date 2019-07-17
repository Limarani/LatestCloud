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
    public class WebDriver_PauldingGA
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        public string FTP_PauldingGA(string streetNo, string streetName, string direction, string streetType, string accountNo, string parcelNumber, string ownerName, string searchType, string orderNumber)
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
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    //Tax Authority

                    driver.Navigate().GoToUrl("https://qpublic.schneidercorp.com/Application.aspx?App=PauldingCountyGA&Layer=Parcels&PageType=Search");
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
                        gc.TitleFlexSearch(orderNumber, "", "", address.Trim(), "GA", "Paulding");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_PauldingGA"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_txtAddress")).SendKeys(streetNo + " " + streetName);
                        gc.CreatePdf_WOP(orderNumber, "Address Search Before", driver, "GA", "Paulding");
                        driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search After", driver, "GA", "Paulding");
                        try
                        {
                            strmulti = GlobalClass.Before(driver.FindElement(By.Id("ctlBodyPane_ctl00_txtResultCount")).Text.Trim(), " Results");
                            if (Convert.ToInt32(strmulti) > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Paulding_Maximum"] = "Maximum";
                                driver.Quit();
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
                                    string strmultiDetails = ImultiTD[3].Text + "~" + ImultiTD[4].Text;
                                    gc.insert_date(orderNumber, ImultiTD[1].Text, 831, strmultiDetails, 1, DateTime.Now);
                                }
                            }
                            HttpContext.Current.Session["multiParcel_Paulding"] = "Yes";
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
                                HttpContext.Current.Session["Nodata_PauldingGA"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.Id("ctlBodyPane_ctl03_ctl01_txtParcelID")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Before Search", driver, "GA", "Paulding");
                        driver.FindElement(By.Id("ctlBodyPane_ctl03_ctl01_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Aftere Pdf", driver, "GA", "Paulding");
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("ctlBodyPane_noDataList_pnlNoResults")).Text;
                            if (nodata.Contains("No results match your search criteria."))
                            {
                                HttpContext.Current.Session["Nodata_PauldingGA"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    else if (searchType == "account")
                    {
                        accountNo = accountNo.TrimStart('0').TrimEnd('0').Trim();
                        driver.FindElement(By.Id("ctlBodyPane_ctl02_ctl01_txtAlternateIDExactMatch")).SendKeys(accountNo);
                        gc.CreatePdf_WOP(orderNumber, "Account Search Before", driver, "GA", "Paulding");
                        driver.FindElement(By.Id("ctlBodyPane_ctl02_ctl01_btnSearchExactMatch")).SendKeys(Keys.Enter);
                        gc.CreatePdf_WOP(orderNumber, "Account Search After", driver, "GA", "Paulding");
                        try
                        {
                            IWebElement IAddressmatchclick = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_ctl01_gvwParcelResults']/tbody"));
                            IList<IWebElement> TRmulti11 = IAddressmatchclick.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDmulti11;
                            foreach (IWebElement row in TRmulti11)
                            {
                                TDmulti11 = row.FindElements(By.TagName("td"));

                                if (TDmulti11.Count != 0)
                                {
                                    if (TDmulti11[2].Text == accountNo)
                                    {
                                        IWebElement Address = TDmulti11[1].FindElement(By.TagName("a"));
                                        Address.Click();
                                    }
                                }
                            }
                        }
                        catch { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("ctlBodyPane_noDataList_pnlNoResults")).Text;
                            if (nodata.Contains("No results match your search criteria."))
                            {
                                HttpContext.Current.Session["Nodata_PauldingGA"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    else if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("ctlBodyPane_ctl00_ctl01_txtName")).SendKeys(ownerName);
                        gc.CreatePdf_WOP(orderNumber, "OwnerName Search Before", driver, "GA", "Paulding");
                        driver.FindElement(By.Id("ctlBodyPane_ctl00_ctl01_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "OwnerName Search After", driver, "GA", "Paulding");

                        try
                        {
                            strmulti = GlobalClass.Before(driver.FindElement(By.Id("ctlBodyPane_ctl00_txtResultCount")).Text.Trim(), " Results");
                            if (Convert.ToInt32(strmulti) > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Paulding_Maximum"] = "Maximum";
                                driver.Quit();
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
                                    string strmultiDetails = ImultiTD[3].Text + "~" + ImultiTD[4].Text;
                                    gc.insert_date(orderNumber, ImultiTD[1].Text, 831, strmultiDetails, 1, DateTime.Now);
                                }
                            }
                            HttpContext.Current.Session["multiParcel_Paulding"] = "Yes";
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
                                HttpContext.Current.Session["Nodata_PauldingGA"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    //Property Details
                    string Owner = "", OwnerAddress = "", ParcelNumber = "", AccountAndRealkey = "", LocationAddress = "", ZipCode = "", Class = "", TaxDistrict = "", MillageRate = "", Acres = "", Neighborhood = "", HomesteadExemption = "", LandlotAndDistrictAndSection = "", Subdivision = "", YearBuilt = "";
                    string Taxauthority = "";
                    try
                    {
                        Owner = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_mSection']/div")).Text.Trim();
                        //string[] Propdata1 = bulktext1.Split('\r');

                        //if (Propdata1.Count() == 4)
                        //{
                        //    Owner = Propdata1[0].Replace("\r\n", "").Trim() + " " + "&" + " " + Propdata1[1].Replace("\r\n", "").Trim();
                        //    OwnerAddress = Propdata1[2].Replace("\r\n", "").Trim() + " " + Propdata1[3].Replace("\r\n", "").Trim();
                        //}
                        //if (Propdata1.Count() == 3)
                        //{
                        //    Owner = Propdata1[0].Replace("\r\n", "").Trim();
                        //    OwnerAddress = Propdata1[1].Replace("\r\n", "").Trim() + Propdata1[2].Replace("\r\n", "").Trim();
                        //}
                        //if (Propdata1.Count() == 2)
                        //{
                        //    OwnerAddress = Propdata1[1].Replace("\r\n", "").Trim() + Propdata1[2].Replace("\r\n", "").Trim();
                        //}

                        ParcelNumber = driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_lblParcelID")).Text.Trim();
                        gc.CreatePdf(orderNumber, ParcelNumber, "Property Details and Assessment Details Pdf", driver, "GA", "Paulding");
                        AccountAndRealkey = driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_lblRealKey")).Text.Trim();
                        LocationAddress = driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_lblLocationAddress")).Text.Trim();
                        ZipCode = driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_lblZipCode")).Text.Trim();
                        Class = driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_lblClass")).Text.Trim();
                        TaxDistrict = driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_lblTaxDistrict")).Text.Trim();
                        MillageRate = driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_lblMillageRate")).Text.Trim();
                        Acres = driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_lblAcres")).Text.Trim();
                        Neighborhood = driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_lblNeighborhood")).Text.Trim();
                        HomesteadExemption = driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_lblHomesteadExemption")).Text.Trim();
                        LandlotAndDistrictAndSection = driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_lblLandLotDistrict")).Text.Trim();
                        try
                        {
                            Subdivision = driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_lblSubdivision")).Text.Trim();
                        }
                        catch { }
                        try
                        {
                            YearBuilt = driver.FindElement(By.Id("ctlBodyPane_ctl05_ctl01_rptResidential_ctl00_lblYearBuilt")).Text.Trim();
                        }
                        catch { }

                    }
                    catch { }
                    //Assessment Details
                    string AssessmentYear = "";
                    try
                    {

                        AssessmentYear = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl11_ctl01_grdValuation']/thead/tr/th[3]")).Text.Trim();
                    }
                    catch { }
                    try
                    {

                        AssessmentYear = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl11_ctl01_grdValuation']/thead/tr/th")).Text.Trim();
                    }
                    catch { }
                    // gc.CreatePdf(orderNumber, ParcelNumber, "Assessment Details ", driver, "GA", "Paulding");
                    try
                    {
                        string AssessmentTitle = "", AssessmentValue = "";//ctlBodyPane_ctl11_ctl01_grdValuation
                        IWebElement tbcurasses12 = driver.FindElement(By.Id("ctlBodyPane_ctl11_ctl01_grdValuation"));
                        IList<IWebElement> TRcurasses2 = tbcurasses12.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDmulti12;
                        foreach (IWebElement row in TRcurasses2)
                        {
                            TDmulti12 = row.FindElements(By.TagName("td"));
                            if (row.Text.Trim() != "" && TDmulti12.Count != 0 && TDmulti12.Count != 2 && !row.Text.Contains("Year"))
                            {
                                AssessmentTitle += TDmulti12[1].Text + "~";
                                AssessmentValue += TDmulti12[2].Text + "~";
                            }
                        }
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + "Year" + "~" + AssessmentTitle.Remove(AssessmentTitle.Length - 1, 1) + "' where Id = '" + 888 + "'");

                        gc.insert_date(orderNumber, ParcelNumber, 888, AssessmentYear + "~" + AssessmentValue.Remove(AssessmentValue.Length - 1, 1), 1, DateTime.Now);
                    }
                    catch { }

                    try
                    {
                        driver.Navigate().GoToUrl("http://www.paulding.gov/gov/taxcommissioner.asp");
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, ParcelNumber, "TaxAuthority Pdf", driver, "GA", "Paulding");
                        Taxauthority = driver.FindElement(By.XPath("//*[@id='divEditor1ad27fed-3cf3-4768-be2f-72f8da981da9']/div/p[2]")).Text.Trim();

                    }
                    catch { }
                    string PropertyDetails = Owner.Trim() + "~" + AccountAndRealkey.Trim() + "~" + LocationAddress.Trim() + "~" + ZipCode.Trim() + "~" + Class.Trim() + "~" + TaxDistrict.Trim() + "~" + MillageRate.Trim() + "~" + Acres.Trim() + "~" + Neighborhood.Trim() + "~" + HomesteadExemption.Trim() + "~" + LandlotAndDistrictAndSection.Trim() + "~" + Subdivision.Trim() + "~" + YearBuilt.Trim() + "~" + Taxauthority.Trim();
                    gc.insert_date(orderNumber, ParcelNumber, 885, PropertyDetails, 1, DateTime.Now);
                    //Tax information Details
                    try
                    {
                        driver.Navigate().GoToUrl("https://pauldingcountytax.com/taxes.html#/WildfireSearch");
                        Thread.Sleep(2000);
                        try
                        {
                            driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[3]/button[2]")).SendKeys(Keys.Enter);
                            Thread.Sleep(2000);
                        }
                        catch
                        { }
                        if (AccountAndRealkey.Count() == 4)
                        {
                            driver.FindElement(By.Id("searchBox")).SendKeys("R00" + AccountAndRealkey);
                            Thread.Sleep(4000);
                        }
                        if (AccountAndRealkey.Count() == 5)
                        {
                            driver.FindElement(By.Id("searchBox")).SendKeys("R0" + AccountAndRealkey);
                            Thread.Sleep(4000);
                        }
                        driver.FindElement(By.XPath("//*[@id='searchForm']/div[1]/div/span/button")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, ParcelNumber, "Payment Details Pdf", driver, "GA", "Paulding");


                    }
                    catch { }
                    //Tax Payment Receipt Details Table
                    string Paymentdetails = "", TaxOwnerName = "", TaxYear = "", Bill = "", MapandParcel = "", PaidDate = "", Paid = "";

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
                                PaidDate = PaymentTD[4].Text;
                                Paid = PaymentTD[5].Text;

                                Paymentdetails = TaxOwnerName + "~" + TaxYear + "~" + Bill + "~" + MapandParcel + "~" + PaidDate + "~" + Paid;
                                gc.insert_date(orderNumber, ParcelNumber, 892, Paymentdetails, 1, DateTime.Now);
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
                                try
                                {
                                    driver.FindElement(By.XPath("//*[@id='avalon']/div/div[3]/div[2]/table/tbody/tr[" + p + "]/td[8]/button")).Click();
                                }
                                catch { }
                                try
                                {
                                    driver.FindElement(By.XPath("//*[@id='avalon']/div/div[4]/div[2]/table/tbody/tr[" + p + "]/td[8]/button")).Click();
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
                                gc.CreatePdf(orderNumber, ParcelNumber, "Overview & Pay Pdf" + p, driver, "GA", "Paulding");

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
                                            gc.CreatePdf(orderNumber, ParcelNumber, "Deliquent Details" + delinaccount, driver, "GA", "Paulding");
                                            gc.insert_date(orderNumber, ParcelNumber, 895, Del_details, 1, DateTime.Now);
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
                                            gc.insert_date(orderNumber, ParcelNumber, 895, Del_details1, 1, DateTime.Now);
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
                                string Taxinfownername = "", Taxaddress1 = "", Account = "", RecordType = "", BillNumber = "", BillTaxYear = "", taxown = "", addcheck = "";
                                //Tax information details
                                IWebElement ownernamedetails = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[1]/div[1]/table/tbody"));
                                string ownernameaddress = gc.Between(ownernamedetails.Text, "Owner Name", "Account");
                                try
                                {
                                    //Bill information////*[@id="avalon"]/div/div/div/div[1]/div[1]/div[1]/table/tbody
                                    IWebElement TaxTB = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[1]/div[1]/table/tbody"));
                                    IList<IWebElement> TaxTR = TaxTB.FindElements(By.TagName("tr"));
                                    IList<IWebElement> TaxTD;
                                    foreach (IWebElement Tax in TaxTR)
                                    {
                                        TaxTD = Tax.FindElements(By.TagName("td"));
                                        if (TaxTD.Count != 0 && Tax.Text != "")
                                        {

                                            if (Tax.Text.Contains("Account"))
                                            {
                                                Account = TaxTD[1].Text;
                                            }
                                            if (Tax.Text.Contains("Record Type"))
                                            {
                                                RecordType = TaxTD[1].Text;
                                            }
                                            if (Tax.Text.Contains("Bill Number"))
                                            {
                                                BillNumber = TaxTD[1].Text;
                                            }
                                            if (Tax.Text.Contains("Tax Year"))
                                            {
                                                BillTaxYear = TaxTD[1].Text;
                                            }
                                        }
                                    }
                                    //Property information
                                    string Propertyid = "", Description = "";
                                    IWebElement TaxTB2 = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[2]/div[1]/table/tbody"));
                                    IList<IWebElement> TaxTR2 = TaxTB2.FindElements(By.TagName("tr"));
                                    IList<IWebElement> TaxTD2;
                                    foreach (IWebElement Tax in TaxTR2)
                                    {
                                        TaxTD2 = Tax.FindElements(By.TagName("td"));
                                        if (TaxTD2.Count != 0 && Tax.Text != "")
                                        {
                                            if (Tax.Text.Contains("Property ID"))
                                            {
                                                Propertyid = TaxTD2[1].Text;
                                            }
                                            if (Tax.Text.Contains("Description"))
                                            {
                                                Description = TaxTD2[1].Text + " ";
                                            }
                                        }
                                    }
                                    //Tax Information
                                    string Totalorginallevy = "", Fairmarketval = "", Assessedval = "", TotaldueTax = "";
                                    IWebElement TaxTB3 = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[1]/div[2]/table/tbody"));
                                    IList<IWebElement> TaxTR3 = TaxTB3.FindElements(By.TagName("tr"));
                                    IList<IWebElement> TaxTD3;
                                    foreach (IWebElement Tax in TaxTR3)
                                    {
                                        TaxTD3 = Tax.FindElements(By.TagName("td"));
                                        if (TaxTD3.Count != 0 && Tax.Text != "")
                                        {
                                            if (Tax.Text.Contains("Total Original Levy"))
                                            {
                                                Totalorginallevy = TaxTD3[1].Text;
                                            }
                                            if (Tax.Text.Contains("Fair Market Value"))
                                            {
                                                Fairmarketval = TaxTD3[1].Text + " ";
                                            }
                                            if (Tax.Text.Contains("Assessed Value"))
                                            {
                                                Assessedval = TaxTD3[1].Text;
                                            }
                                            if (Tax.Text.Contains("Total Due"))
                                            {
                                                TotaldueTax = TaxTD3[1].Text + " ";
                                            }
                                        }
                                    }
                                    //Payment Information
                                    string DueDate = "", PaymentStatus = "", LastPaymentDate = "", TotalAmountPaid = "", TotalDuePayment = "";
                                    IWebElement TaxTB4 = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[2]/div[2]/table/tbody"));
                                    IList<IWebElement> TaxTR4 = TaxTB4.FindElements(By.TagName("tr"));
                                    IList<IWebElement> TaxTD4;
                                    foreach (IWebElement Tax in TaxTR4)
                                    {
                                        TaxTD4 = Tax.FindElements(By.TagName("td"));
                                        if (TaxTD4.Count != 0 && Tax.Text != "")
                                        {
                                            if (Tax.Text.Contains("Due Date"))
                                            {
                                                DueDate = TaxTD4[1].Text;
                                            }
                                            if (Tax.Text.Contains("Payment Status"))
                                            {
                                                PaymentStatus = TaxTD4[1].Text + " ";
                                            }
                                            if (Tax.Text.Contains("Last Payment Date"))
                                            {
                                                LastPaymentDate = TaxTD4[1].Text;
                                            }
                                            if (Tax.Text.Contains("Total Amount Paid"))
                                            {
                                                TotalAmountPaid = TaxTD4[1].Text + " ";
                                            }
                                            if (Tax.Text.Contains("Total Due"))
                                            {
                                                TotalDuePayment = TaxTD4[1].Text + " ";
                                            }
                                        }
                                    }
                                    string Taxinfo_details1 = ownernameaddress + "~" + Account + "~" + RecordType + "~" + BillNumber + "~" + BillTaxYear + "~" + Propertyid + "~" + Description + "~" + Totalorginallevy + "~" + Fairmarketval + "~" + Assessedval + "~" + TotaldueTax + "~" + DueDate + "~" + PaymentStatus + "~" + LastPaymentDate + "~" + TotalAmountPaid + "~" + TotalDuePayment;
                                    gc.insert_date(orderNumber, ParcelNumber, 897, Taxinfo_details1, 1, DateTime.Now);
                                }
                                catch { }

                                //Tax Bill

                                driver.FindElement(By.XPath("//*[@id='avalon']/div/div/ul/li[2]/a")).Click();
                                Thread.Sleep(9000);
                                gc.CreatePdf(orderNumber, ParcelNumber, "Tax Bill Details" + BillTaxYear, driver, "GA", "Paulding");

                                //View And Print Receipt
                                try
                                {
                                    driver.FindElement(By.XPath("//*[@id='avalon']/div/div/ul/li[3]/a")).Click();
                                    Thread.Sleep(9000);
                                    gc.CreatePdf(orderNumber, ParcelNumber, "View Print Receipt" + BillTaxYear, driver, "GA", "Paulding");
                                }
                                catch { }
                                Thread.Sleep(5000);
                                driver.Navigate().Back();


                            }
                        }
                    }
                    catch { }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    //DALLAS
                    string PaidURL = "", UnpaidURL = "", Parcel1 = "", TaxOwner1 = "", DALAddress1 = "", Type = "", Type1 = "", TaxParcelYear1 = "", ReceiptNumber1 = "", Taxablevalue1 = "", Taxespaid11 = "", Taxespaid = "", PaymentDate1 = "";
                    string Parcel = "", TaxOwner = "", DALAddress = "", Types = "", TaxParcelYear = "", ReceiptNumber = "", Taxablevalue = "", Taxespaid1 = "", PaymentDate = "", TaxDueAmount = "";
                    try
                    {
                        if (TaxDistrict.Contains("DALLAS"))
                        {
                            HttpContext.Current.Session["PauldingGA_City"] = "DALLAS";
                            driver.Navigate().GoToUrl("https://www.municipalonlinepayments.com/dallasga/tax/search");
                            Thread.Sleep(5000);

                            try
                            {
                                IWebElement IAddressSearch1 = driver.FindElement(By.LinkText("Parcel"));
                                IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                                js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                            }
                            catch { }
                            try
                            {
                                IWebElement IAddressSearch1 = driver.FindElement(By.LinkText("Parcel"));
                                IAddressSearch1.Click();
                            }
                            catch { }
                            Thread.Sleep(2000);
                            driver.FindElement(By.Id("ParcelNumber")).SendKeys("0" + AccountAndRealkey);
                            gc.CreatePdf(orderNumber, ParcelNumber, "City TAX Details Enter Before Pdf", driver, "GA", "Paulding");

                            try
                            {
                                IWebElement IAddressSearch2 = driver.FindElement(By.XPath("//*[@id='Parcel']/form/div[2]/button"));
                                IJavaScriptExecutor js2 = driver as IJavaScriptExecutor;
                                js2.ExecuteScript("arguments[0].click();", IAddressSearch2);
                            }
                            catch { }

                            try
                            {
                                IWebElement IAddressSearch2 = driver.FindElement(By.XPath("//*[@id='Parcel']/form/div[2]/button"));
                                IAddressSearch2.Click();
                            }
                            catch { }
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, ParcelNumber, "City TAX Details Enter After Pdf", driver, "GA", "Paulding");


                            IWebElement TaxTB4 = driver.FindElement(By.Id("search_results"));
                            IList<IWebElement> TaxTR4 = TaxTB4.FindElements(By.TagName("li"));
                            IList<IWebElement> TaxTD4;
                            foreach (IWebElement Tax in TaxTR4)
                            {
                                TaxTD4 = Tax.FindElements(By.TagName("a"));
                                if (TaxTD4.Count != 0)
                                {
                                    if (Tax.Text.Contains("Paid"))
                                    {
                                        try
                                        {
                                            PaidURL = TaxTD4[0].GetAttribute("href");
                                            TaxTD4[0].Click();
                                            Thread.Sleep(1000);
                                            //Taxinfo Paid Details

                                            IWebElement Taxpaiddetails = driver.FindElement(By.XPath("//*[@id='paidTable']/tbody"));
                                            IList<IWebElement> TRTaxpaiddetails = Taxpaiddetails.FindElements(By.TagName("tr"));
                                            IList<IWebElement> TDTax;
                                            foreach (IWebElement Taxess in TRTaxpaiddetails)
                                            {
                                                TDTax = Taxess.FindElements(By.TagName("td"));
                                                if (TDTax.Count != 0 && TDTax.Count == 6 && !Taxess.Text.Contains("No paid Parcels found.") && Taxess.Text.Trim() != (""))
                                                {
                                                    if (Taxess.Text.Contains("Parcel Number"))
                                                    {
                                                        Parcel1 = gc.Between(TDTax[0].Text, "Parcel Number:", "Owner:").Trim();
                                                        TaxOwner1 = gc.Between(TDTax[0].Text, "Owner:", "Address:").Trim();
                                                        DALAddress1 = gc.Between(TDTax[0].Text, "Address:", "Type:").Trim();
                                                        Type1 = GlobalClass.After(TDTax[0].Text, "\r\nType:").Trim();
                                                    }

                                                    TaxParcelYear1 = TDTax[1].Text.Trim();
                                                    ReceiptNumber1 = TDTax[2].Text.Trim();
                                                    Taxablevalue1 = TDTax[4].Text.Trim();
                                                    Taxespaid11 = TDTax[5].Text.Trim();

                                                    Taxespaid1 = GlobalClass.Before(Taxespaid11, "\r\n").Trim();
                                                    PaymentDate1 = GlobalClass.After(Taxespaid11, "\r\n").Trim();
                                                    string cityTaxauthority = "Property Tax Office is Located at 200 Main Street Contact Us 770 443-8108";

                                                    string Taxinfo_Dallasdetailspaid = Parcel1.Trim() + "~" + TaxOwner1.Trim() + "~" + DALAddress1 + "~" + Type1 + "~" + TaxParcelYear1 + "~" + ReceiptNumber1 + "~" + Taxablevalue1 + "~" + Taxespaid1 + "~" + PaymentDate1 + "~" + cityTaxauthority;
                                                    gc.insert_date(orderNumber, ParcelNumber, 912, Taxinfo_Dallasdetailspaid, 1, DateTime.Now);

                                                }
                                            }
                                            gc.CreatePdf(orderNumber, ParcelNumber, "City TAX Paid Pdf", driver, "GA", "Paulding");

                                        }
                                        catch
                                        {

                                        }
                                    }
                                    if (Tax.Text.Contains("Unpaid"))
                                    {
                                        try
                                        {
                                            UnpaidURL = TaxTD4[0].GetAttribute("href");
                                            TaxTD4[0].Click();

                                            //Tax info Unpaid Details


                                            IWebElement TaxUnpaiddetails = driver.FindElement(By.XPath("//*[@id='unpaid']/form/table/tbody"));
                                            IList<IWebElement> TRTaxunpaiddetails = TaxUnpaiddetails.FindElements(By.TagName("tr"));
                                            IList<IWebElement> TDTaxunpaid;
                                            foreach (IWebElement Taxesss in TRTaxunpaiddetails)
                                            {
                                                TDTaxunpaid = Taxesss.FindElements(By.TagName("td"));
                                                if (TDTaxunpaid.Count != 0 && TDTaxunpaid.Count == 8 && !Taxesss.Text.Contains("No unpaid Parcels found.") && Taxesss.Text.Trim() != (""))
                                                {
                                                    if (Taxesss.Text.Contains("Parcel Number"))
                                                    {
                                                        Parcel = gc.Between(TDTaxunpaid[0].Text, "Parcel Number:", "Owner:").Trim();
                                                        TaxOwner = gc.Between(TDTaxunpaid[0].Text, "Owner:", "Address:").Trim();
                                                        DALAddress = gc.Between(TDTaxunpaid[0].Text, "Address:", "Type:").Trim();
                                                        Types = GlobalClass.After(TDTaxunpaid[0].Text, "\r\nType:").Trim();
                                                    }
                                                    TaxParcelYear = TDTaxunpaid[1].Text.Trim();
                                                    ReceiptNumber = TDTaxunpaid[2].Text.Trim();
                                                    Taxablevalue = TDTaxunpaid[4].Text.Trim();
                                                    TaxDueAmount = TDTaxunpaid[6].Text.Trim();
                                                    string cityTaxauthority = "Property Tax Office is Located at 200 Main Street Contact Us 770 443-8108";
                                                    string Taxinfo_DallasdetailsUnpaid = Parcel.Trim() + "~" + TaxOwner.Trim() + "~" + DALAddress + "~" + Types + "~" + TaxParcelYear + "~" + ReceiptNumber + "~" + Taxablevalue + "~" + TaxDueAmount + "~" + cityTaxauthority;
                                                    gc.insert_date(orderNumber, ParcelNumber, 913, Taxinfo_DallasdetailsUnpaid, 1, DateTime.Now);

                                                }
                                            }
                                            gc.CreatePdf(orderNumber, ParcelNumber, "City TAX UnPaid Pdf", driver, "GA", "Paulding");
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
                    }
                    catch { }

                    CitytaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "GA", "Paulding", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);


                    driver.Quit();
                    gc.mergpdf(orderNumber, "GA", "Paulding");
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