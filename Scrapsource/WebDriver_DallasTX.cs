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

namespace ScrapMaricopa
{

    public class WebDriver_DallasTX
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();

        public string FTP_DallasTX(string sno, string sname, string direction, string sttype, string unino, string parcelNumber, string ownername, string searchType, string orderNumber, string directparcel)
        {
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "", AssessTakenTime = "", TaxTakentime = "", CityTaxtakentime = "";
            string straddress = "";
            string multiParcelnumber = "";
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            // driver = new PhantomJSDriver();
            //driver = new ChromeDriver();           
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    if (searchType == "titleflex")
                    {

                        if (direction != "")
                        {
                            straddress = sno + " " + direction + " " + sname + " " + sttype + " " + unino;
                        }
                        else
                        {
                            straddress = sno + " " + sname + " " + sttype + " " + unino;
                        }
                        gc.TitleFlexSearch(orderNumber, "", "", straddress, "TX", "Dallas");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_DallasTX"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    driver.Navigate().GoToUrl("http://www.dallascad.org/SearchOwner.aspx");
                    Thread.Sleep(1000);

                    if (searchType == "address")
                    {

                        IWebElement IAddressSearch1 = driver.FindElement(By.XPath("//*[@id='Form1']/table[2]/tbody/tr[1]/td[2]/p[2]/span/a[3]"));
                        IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                        js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                        Thread.Sleep(3000);

                        driver.FindElement(By.Id("txtAddrNum")).SendKeys(sno);
                        driver.FindElement(By.Id("listStDir")).SendKeys(direction);
                        driver.FindElement(By.Id("txtStName")).SendKeys(sname);
                        gc.CreatePdf_WOP(orderNumber, "Address search Input ", driver, "TX", "Dallas");
                        driver.FindElement(By.Id("cmdSubmit")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Address search Output ", driver, "TX", "Dallas");
                        try
                        {
                            //int Count = 0;   
                            List<string> Multiinfo = new List<string>();
                            IWebElement Multiaddresstable = driver.FindElement(By.Id("SearchResults1_dgResults"));
                            IList<IWebElement> multiaddressrow = Multiaddresstable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Multiaddressid;
                            foreach (IWebElement Multiaddress in multiaddressrow)
                            {
                                Multiaddressid = Multiaddress.FindElements(By.TagName("td"));
                                if (multiaddressrow.Count > 4 && Multiaddressid.Count != 0 && Multiaddressid.Count == 6 && !Multiaddress.Text.Contains("Property Address") && Multiaddress.Text.Contains(straddress))
                                {
                                    string Propertyadd = Multiaddressid[1].Text;
                                    string Cit = Multiaddressid[2].Text;
                                    string OWnername = Multiaddressid[3].Text;
                                    string Totalvl = Multiaddressid[4].Text;
                                    string Type = Multiaddressid[5].Text;
                                    IWebElement value1 = Multiaddressid[1].FindElement(By.Id("Hyperlink1"));
                                    multiParcelnumber = value1.GetAttribute("href");
                                    multiParcelnumber = GlobalClass.After(multiParcelnumber, "http://www.dallascad.org/AcctDetailRes.aspx?ID=");

                                    string multiaddressresult = OWnername.Trim() + "~" + Propertyadd.Trim() + "~" + Cit.Trim() + "~" + Type.Trim() + "~" + Totalvl.Trim();
                                    gc.insert_date(orderNumber, multiParcelnumber, 2155, multiaddressresult, 1, DateTime.Now);
                                    //Count++;
                                }

                            }
                            if (multiaddressrow.Count == 4)
                            {
                                driver.FindElement(By.XPath("//*[@id='Hyperlink1']")).Click();
                                Thread.Sleep(2000);
                            }
                            if (multiaddressrow.Count > 4)
                            {
                                HttpContext.Current.Session["multiparcel_DallasTX"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (multiaddressrow.Count > 35)
                            {
                                HttpContext.Current.Session["multiParcel_DallasTX_Maximum"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("SearchResults1_lblMessage")).Text;
                            if (nodata.Contains("No Records Found."))
                            {
                                HttpContext.Current.Session["Nodata_DallasTX"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {
                        IWebElement IAddressSearch1 = driver.FindElement(By.XPath("//*[@id='Form1']/table[2]/tbody/tr[1]/td[2]/p[2]/span/a[2]"));
                        IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                        js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                        Thread.Sleep(3000);

                        //parcelNumber = parcelNumber.Replace(" ", "").Replace(".", "").Replace("-", "").Trim();
                        driver.FindElement(By.Id("txtAcctNum")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search Input ", driver, "TX", "Dallas");
                        driver.FindElement(By.Id("Button1")).Click();
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search Output ", driver, "TX", "Dallas");

                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("SearchResults1_lblMessage")).Text;
                            if (nodata.Contains("No Records Found."))
                            {
                                HttpContext.Current.Session["Nodata_DallasTX"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("txtOwnerName")).SendKeys(ownername);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Owner search Input ", driver, "TX", "Dallas");

                        IWebElement IAddressSearch1 = driver.FindElement(By.Id("cmdSubmit"));
                        IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                        js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                        Thread.Sleep(3000);

                        gc.CreatePdf_WOP(orderNumber, "Owner search Result ", driver, "TX", "Dallas");
                        ////Multiparcel

                        try
                        {
                            //int Count = 0;   
                            List<string> Multiinfo = new List<string>();
                            IWebElement Multiaddresstable = driver.FindElement(By.Id("SearchResults1_dgResults"));
                            IList<IWebElement> multiaddressrow = Multiaddresstable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Multiaddressid;
                            foreach (IWebElement Multiaddress in multiaddressrow)
                            {
                                Multiaddressid = Multiaddress.FindElements(By.TagName("td"));
                                if (multiaddressrow.Count > 4 && Multiaddressid.Count != 0 && Multiaddressid.Count == 6 && !Multiaddress.Text.Contains("Property Address") && Multiaddress.Text.Contains(straddress))
                                {
                                    string Propertyadd = Multiaddressid[1].Text;
                                    string Cit = Multiaddressid[2].Text;
                                    string OWnername = Multiaddressid[3].Text;
                                    string Totalvl = Multiaddressid[4].Text;
                                    string Type = Multiaddressid[5].Text;
                                    IWebElement value1 = Multiaddressid[1].FindElement(By.Id("Hyperlink1"));
                                    multiParcelnumber = value1.GetAttribute("href");
                                    multiParcelnumber = GlobalClass.After(multiParcelnumber, "http://www.dallascad.org/AcctDetailRes.aspx?ID=");

                                    string multiaddressresult = OWnername.Trim() + "~" + Propertyadd.Trim() + "~" + Cit.Trim() + "~" + Type.Trim() + "~" + Totalvl.Trim();
                                    gc.insert_date(orderNumber, multiParcelnumber, 2155, multiaddressresult, 1, DateTime.Now);
                                    //Count++;
                                }

                            }
                            if (multiaddressrow.Count == 4)
                            {
                                driver.FindElement(By.XPath("//*[@id='Hyperlink1']")).Click();
                                Thread.Sleep(2000);
                            }
                            if (multiaddressrow.Count > 4)
                            {
                                HttpContext.Current.Session["multiparcel_DallasTX"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (multiaddressrow.Count > 35)
                            {
                                HttpContext.Current.Session["multiParcel_DallasTX_Maximum"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("SearchResults1_lblMessage")).Text;
                            if (nodata.Contains("No Records Found."))
                            {
                                HttpContext.Current.Session["Nodata_DallasTX"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    //Property Details
                    try
                    {
                        IWebElement IAddressSearch111 = driver.FindElement(By.XPath("//*[@id='Hyperlink1']"));
                        IJavaScriptExecutor js111 = driver as IJavaScriptExecutor;
                        js111.ExecuteScript("arguments[0].click();", IAddressSearch111);
                        Thread.Sleep(3000);
                    }
                    catch { }

                    string Parcelnumber = "", Ownername = "", Address = "", Addressmail = "", Neighborhood = "", Legaldesc = "", Exemption = "", Yearbuilt = "";
                    ////*[@id="MultiOwner1_dgmultiOwner"]/tbody/tr[2]/td[1]
                    Parcelnumber = driver.FindElement(By.XPath("//*[@id='Form1']/table[2]/tbody/tr[1]/td[2]/p[1]")).Text.Replace("Residential Account #", "").Trim();
                    Ownername = driver.FindElement(By.XPath("//*[@id='MultiOwner1_dgmultiOwner']/tbody/tr[2]/td[1]")).Text;
                    Address = driver.FindElement(By.XPath("//*[@id='PropAddr1_lblPropAddr']")).Text;
                    Neighborhood = driver.FindElement(By.XPath("//*[@id='lblNbhd']")).Text;
                    Legaldesc = driver.FindElement(By.Id("Table8")).Text.Replace("\r\n", "");
                    try
                    {
                        Exemption = driver.FindElement(By.XPath("//*[@id='Exempt1_lblMessage']")).Text.Replace("\r\n", "");
                    }
                    catch { }
                    Yearbuilt = driver.FindElement(By.XPath("//*[@id='table5']/tbody/tr[2]/td[1]")).Text;

                    string Propertydetails = Ownername + "~" + Address + "~" + Neighborhood + "~" + Legaldesc + "~" + Exemption + "~" + Yearbuilt;
                    gc.insert_date(orderNumber, Parcelnumber, 2146, Propertydetails, 1, DateTime.Now);
                    //Assessment Details
                    string Improvements = "", Land = "", Marketvalue = "", Revaluationyear = "", Previousval = "";
                    Improvements = driver.FindElement(By.XPath("//*[@id='ValueSummary1_lblImpVal']")).Text;
                    Land = driver.FindElement(By.XPath("//*[@id='ValueSummary1_pnlValue_lblLandVal']")).Text;
                    Marketvalue = driver.FindElement(By.XPath("//*[@id='ValueSummary1_pnlValue_lblTotalVal']")).Text;
                    try
                    {
                        Revaluationyear = driver.FindElement(By.XPath("//*[@id='tblValueSum']/tbody/tr[3]/td[2]")).Text;
                    }
                    catch { }
                    Previousval = driver.FindElement(By.XPath("//*[@id='tblValueSum']/tbody/tr[4]/td[2]")).Text;

                    string Assessmentdetails = Improvements + "~" + Land + "~" + Marketvalue + "~" + Revaluationyear + "~" + Previousval;
                    gc.insert_date(orderNumber, Parcelnumber, 2147, Assessmentdetails, 1, DateTime.Now);


                    //Tax Information Details
                    string title = "", Citytax = "", City = "", school = "", Countyschool = "", College = "", Hospital = "", SpecialDistrict = "";
                    IWebElement Taxinfo1 = driver.FindElement(By.XPath("//*[@id='TaxEst1_pnlTaxEst']/table/tbody"));
                    IList<IWebElement> TRBillsinfo2 = Taxinfo1.FindElements(By.TagName("tr"));
                    IList<IWebElement> Aherftax;
                    IList<IWebElement> Aherftax1;
                    int i = 0;
                    foreach (IWebElement row in TRBillsinfo2)
                    {
                        Aherftax = row.FindElements(By.TagName("td"));
                        Aherftax1 = row.FindElements(By.TagName("th"));
                        if (Aherftax1.Count != 0 && Aherftax1.Count == 1 && !row.Text.Contains("City"))
                        {
                            title = Aherftax1[0].Text;
                        }
                        if (Aherftax1.Count != 0 && Aherftax1.Count == 2 && !row.Text.Contains("City"))
                        {
                            City = Aherftax1[0].Text;
                            SpecialDistrict = Aherftax1[1].Text;

                            string taxhisdetails = title + "~" + City.Trim() + "~" + school.Trim() + "~" + Countyschool.Trim() + "~" + College.Trim() + "~" + Hospital.Trim() + "~" + SpecialDistrict.Trim();
                            gc.insert_date(orderNumber, Parcelnumber, 2148, taxhisdetails, 1, DateTime.Now);
                        }

                        if (Aherftax.Count != 0 && Aherftax.Count == 6 && !row.Text.Contains("City"))
                        {
                            City = Aherftax[0].Text;
                            school = Aherftax[1].Text;
                            Countyschool = Aherftax[2].Text;
                            College = Aherftax[3].Text;
                            Hospital = Aherftax[4].Text;
                            SpecialDistrict = Aherftax[5].Text;

                            string taxhisdetails = title + "~" + City.Trim() + "~" + school.Trim() + "~" + Countyschool.Trim() + "~" + College.Trim() + "~" + Hospital.Trim() + "~" + SpecialDistrict.Trim();
                            gc.insert_date(orderNumber, Parcelnumber, 2148, taxhisdetails, 1, DateTime.Now);

                        }
                        if (Aherftax.Count != 0 && !row.Text.Contains("City") && (City == "GARLAND" || City == "MESQUITE" || City == "COPPELL"))
                        {

                            Citytax = Aherftax[0].Text;
                        }
                    }
                    //City of Garland Tax Information Details

                    if (Citytax.Contains("GARLAND"))
                    {
                        driver.Navigate().GoToUrl("https://www.texaspayments.com/057120");
                        Thread.Sleep(3000);
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='ddlSearch']/span/span/span[2]/span")).Click();
                            Thread.Sleep(1000);
                        }
                        catch { }
                        IWebElement IAddressSearchCad = driver.FindElement(By.XPath("//*[@id='ddl_accountsearch_listbox']/li[4]"));
                        IJavaScriptExecutor jscad = driver as IJavaScriptExecutor;
                        jscad.ExecuteScript("arguments[0].click();", IAddressSearchCad);
                        Thread.Sleep(2000);
                        driver.FindElement(By.Id("searchValue")).SendKeys(Parcelnumber);
                        driver.FindElement(By.Id("searchBtn")).Click();
                        Thread.Sleep(15000);

                        //Pay Tax Details
                        List<string> billinfo = new List<string>();
                        IWebElement Billsinfo2 = driver.FindElement(By.Id("AccountInfoGrid"));
                        IList<IWebElement> TRBillsinfo22 = Billsinfo2.FindElements(By.TagName("tr"));
                        IList<IWebElement> Aherftax2;
                        // int i = 0;
                        foreach (IWebElement row in TRBillsinfo22)
                        {
                            Aherftax2 = row.FindElements(By.TagName("td"));

                            if (Aherftax2.Count != 0 && Aherftax2.Count == 4 && !row.Text.Contains("Show Detail") /*&& !row.Text.Contains("1st Installment 2nd Installment") && !row.Text.Contains("Bill Type")*/)
                            {
                                string Year = Aherftax2[1].Text;
                                string CurrentLevy1 = Aherftax2[2].Text;
                                string Amountdue = Aherftax2[3].Text;

                                string Paytaxdetails = Year.Trim() + "~" + CurrentLevy1.Trim() + "~" + Amountdue.Trim();
                                gc.insert_date(orderNumber, Parcelnumber, 2149, Paytaxdetails, 1, DateTime.Now);
                            }
                        }
                        //Pay Taxes Information Details
                        string Year1 = "", Taxyear = "", Totalmarketval = "", Homesteadcap = "", Totalappraised = "", Homesteadexemption = "", DisabledPersonOverExemption = "", DisabledVeteransExemption = "", OtherExemptionDeferrals = "", TaxableValue = "", FrozenTaxInformation = "", TaxRate = "", CertifiedLevy = "", CurrentLevy2 = "", TaxesDue = "", PenaltyandInterest = "", AttorneyFees = "", OtherDue = "", AmountDue = "", TotalAmountPaid = "", LastCheckNumber = "", LastPayDate = "";
                        int Cityofgary = 0;
                        driver.FindElement(By.Id("lblShowAllYearDetail")).Click();
                        Thread.Sleep(5000);
                        IWebElement Paytaxinfo = driver.FindElement(By.XPath("//*[@id='AccountBalance']/div[1]/div[5]"));
                        IList<IWebElement> PaytaxinfoRow = Paytaxinfo.FindElements(By.TagName("div"));
                        int rowcount = PaytaxinfoRow.Count;
                        for (int p = 0; p <= rowcount; p++)
                        {
                            if (Cityofgary < 7 && PaytaxinfoRow[p].Text.Contains("CITY OF GARLAND"))
                            {
                                if (p == 0)
                                {
                                    Year1 = PaytaxinfoRow[0].Text.Trim();
                                    Totalmarketval = gc.Between(Year1, "Total Market Value:", "10% Homestead Cap/Ag Deferral:");
                                    Homesteadcap = gc.Between(Year1, "10% Homestead Cap/Ag Deferral:", "Total Appraised Value:");
                                    Totalappraised = gc.Between(Year1, "Total Appraised Value:", "Homestead Exemption:");
                                    Homesteadexemption = gc.Between(Year1, "Homestead Exemption:", "Disabled Person/Over 65 Exemption:");
                                    DisabledPersonOverExemption = gc.Between(Year1, "Disabled Person/Over 65 Exemption:", "Disabled Veterans Exemption:");
                                    DisabledVeteransExemption = gc.Between(Year1, "Disabled Veterans Exemption:", "Other Exemption/Deferrals:");
                                    OtherExemptionDeferrals = gc.Between(Year1, "Other Exemption/Deferrals:", "Taxable Value:");
                                    TaxableValue = gc.Between(Year1, "Taxable Value:", "Frozen Tax Information:");
                                    FrozenTaxInformation = gc.Between(Year1, "Frozen Tax Information:", "Tax Rate:");
                                    TaxRate = gc.Between(Year1, "Tax Rate:", "Certified Levy:");
                                    CertifiedLevy = gc.Between(Year1, "Certified Levy:", "Current Levy:");
                                    CurrentLevy2 = gc.Between(Year1, "Current Levy:", "Taxes Due:");
                                    TaxesDue = gc.Between(Year1, "Taxes Due:", "Penalty and Interest:");
                                    PenaltyandInterest = gc.Between(Year1, "Penalty and Interest:", "Attorney Fees:");
                                    AttorneyFees = gc.Between(Year1, "Attorney Fees:", "Other Due:");
                                    OtherDue = gc.Between(Year1, "Other Due:", "Amount Due:");
                                    AmountDue = gc.Between(Year1, "Amount Due:", "Total Amount Paid:");
                                    TotalAmountPaid = gc.Between(Year1, "Total Amount Paid:", "Last Check Number:");
                                    LastCheckNumber = gc.Between(Year1, "Last Check Number:", "Last Pay Date:");
                                    LastPayDate = GlobalClass.After(Year1, "Last Pay Date:").Replace("\r\n", "").Trim();

                                }
                                if (p == 2)
                                {
                                    Taxyear = PaytaxinfoRow[2].Text.Trim();
                                    string Paytaxinformadetails = Taxyear.Trim() + "~" + Totalmarketval.Trim() + "~" + Homesteadcap.Trim() + "~" + Totalappraised.Trim() + "~" + Homesteadexemption.Trim() + "~" + DisabledPersonOverExemption.Trim() + "~" + DisabledVeteransExemption.Trim() + "~" + OtherExemptionDeferrals.Trim() + "~" + TaxableValue.Trim() + "~" + FrozenTaxInformation.Trim() + "~" + TaxRate.Trim() + "~" + CertifiedLevy.Trim() + "~" + CurrentLevy2.Trim() + "~" + TaxesDue.Trim() + "~" + PenaltyandInterest.Trim() + "~" + AttorneyFees.Trim() + "~" + OtherDue.Trim() + "~" + AmountDue.Trim() + "~" + TotalAmountPaid.Trim() + "~" + LastCheckNumber.Trim() + "~" + LastPayDate.Trim();
                                    gc.insert_date(orderNumber, Parcelnumber, 2150, Paytaxinformadetails, 1, DateTime.Now);
                                    Year1 = ""; Taxyear = ""; Totalmarketval = ""; Homesteadcap = ""; Totalappraised = ""; Homesteadexemption = ""; DisabledPersonOverExemption = ""; DisabledVeteransExemption = ""; OtherExemptionDeferrals = ""; TaxableValue = ""; FrozenTaxInformation = ""; TaxRate = ""; CertifiedLevy = ""; CurrentLevy2 = ""; TaxesDue = ""; PenaltyandInterest = ""; AttorneyFees = ""; OtherDue = ""; AmountDue = ""; TotalAmountPaid = ""; LastCheckNumber = ""; LastPayDate = "";
                                }
                                if (p == 66)
                                {
                                    Year1 = PaytaxinfoRow[66].Text.Trim();
                                    Totalmarketval = gc.Between(Year1, "Total Market Value:", "10% Homestead Cap/Ag Deferral:");
                                    Homesteadcap = gc.Between(Year1, "10% Homestead Cap/Ag Deferral:", "Total Appraised Value:");
                                    Totalappraised = gc.Between(Year1, "Total Appraised Value:", "Homestead Exemption:");
                                    Homesteadexemption = gc.Between(Year1, "Homestead Exemption:", "Disabled Person/Over 65 Exemption:");
                                    DisabledPersonOverExemption = gc.Between(Year1, "Disabled Person/Over 65 Exemption:", "Disabled Veterans Exemption:");
                                    DisabledVeteransExemption = gc.Between(Year1, "Disabled Veterans Exemption:", "Other Exemption/Deferrals:");
                                    OtherExemptionDeferrals = gc.Between(Year1, "Other Exemption/Deferrals:", "Taxable Value:");
                                    TaxableValue = gc.Between(Year1, "Taxable Value:", "Frozen Tax Information:");
                                    FrozenTaxInformation = gc.Between(Year1, "Frozen Tax Information:", "Tax Rate:");
                                    TaxRate = gc.Between(Year1, "Tax Rate:", "Certified Levy:");
                                    CertifiedLevy = gc.Between(Year1, "Certified Levy:", "Current Levy:");
                                    CurrentLevy2 = gc.Between(Year1, "Current Levy:", "Taxes Due:");
                                    TaxesDue = gc.Between(Year1, "Taxes Due:", "Penalty and Interest:");
                                    PenaltyandInterest = gc.Between(Year1, "Penalty and Interest:", "Attorney Fees:");
                                    AttorneyFees = gc.Between(Year1, "Attorney Fees:", "Other Due:");
                                    OtherDue = gc.Between(Year1, "Other Due:", "Amount Due:");
                                    AmountDue = gc.Between(Year1, "Amount Due:", "Total Amount Paid:");
                                    TotalAmountPaid = gc.Between(Year1, "Total Amount Paid:", "Last Check Number:");
                                    LastCheckNumber = gc.Between(Year1, "Last Check Number:", "Last Pay Date:");
                                    LastPayDate = GlobalClass.After(Year1, "Last Pay Date:").Replace("\r\n", "").Trim();
                                }
                                if (p == 68)
                                {

                                    Taxyear = PaytaxinfoRow[68].Text.Trim();
                                    string Paytaxinformadetails = Taxyear.Trim() + "~" + Totalmarketval.Trim() + "~" + Homesteadcap.Trim() + "~" + Totalappraised.Trim() + "~" + Homesteadexemption.Trim() + "~" + DisabledPersonOverExemption.Trim() + "~" + DisabledVeteransExemption.Trim() + "~" + OtherExemptionDeferrals.Trim() + "~" + TaxableValue.Trim() + "~" + FrozenTaxInformation.Trim() + "~" + TaxRate.Trim() + "~" + CertifiedLevy.Trim() + "~" + CurrentLevy2.Trim() + "~" + TaxesDue.Trim() + "~" + PenaltyandInterest.Trim() + "~" + AttorneyFees.Trim() + "~" + OtherDue.Trim() + "~" + AmountDue.Trim() + "~" + TotalAmountPaid.Trim() + "~" + LastCheckNumber.Trim() + "~" + LastPayDate.Trim();
                                    gc.insert_date(orderNumber, Parcelnumber, 2150, Paytaxinformadetails, 1, DateTime.Now);
                                    Year1 = ""; Taxyear = ""; Totalmarketval = ""; Homesteadcap = ""; Totalappraised = ""; Homesteadexemption = ""; DisabledPersonOverExemption = ""; DisabledVeteransExemption = ""; OtherExemptionDeferrals = ""; TaxableValue = ""; FrozenTaxInformation = ""; TaxRate = ""; CertifiedLevy = ""; CurrentLevy2 = ""; TaxesDue = ""; PenaltyandInterest = ""; AttorneyFees = ""; OtherDue = ""; AmountDue = ""; TotalAmountPaid = ""; LastCheckNumber = ""; LastPayDate = "";
                                }

                                if (p == 132)
                                {
                                    Year1 = PaytaxinfoRow[132].Text.Trim();
                                    Totalmarketval = gc.Between(Year1, "Total Market Value:", "10% Homestead Cap/Ag Deferral:");
                                    Homesteadcap = gc.Between(Year1, "10% Homestead Cap/Ag Deferral:", "Total Appraised Value:");
                                    Totalappraised = gc.Between(Year1, "Total Appraised Value:", "Homestead Exemption:");
                                    Homesteadexemption = gc.Between(Year1, "Homestead Exemption:", "Disabled Person/Over 65 Exemption:");
                                    DisabledPersonOverExemption = gc.Between(Year1, "Disabled Person/Over 65 Exemption:", "Disabled Veterans Exemption:");
                                    DisabledVeteransExemption = gc.Between(Year1, "Disabled Veterans Exemption:", "Other Exemption/Deferrals:");
                                    OtherExemptionDeferrals = gc.Between(Year1, "Other Exemption/Deferrals:", "Taxable Value:");
                                    TaxableValue = gc.Between(Year1, "Taxable Value:", "Frozen Tax Information:");
                                    FrozenTaxInformation = gc.Between(Year1, "Frozen Tax Information:", "Tax Rate:");
                                    TaxRate = gc.Between(Year1, "Tax Rate:", "Certified Levy:");
                                    CertifiedLevy = gc.Between(Year1, "Certified Levy:", "Current Levy:");
                                    CurrentLevy2 = gc.Between(Year1, "Current Levy:", "Taxes Due:");
                                    TaxesDue = gc.Between(Year1, "Taxes Due:", "Penalty and Interest:");
                                    PenaltyandInterest = gc.Between(Year1, "Penalty and Interest:", "Attorney Fees:");
                                    AttorneyFees = gc.Between(Year1, "Attorney Fees:", "Other Due:");
                                    OtherDue = gc.Between(Year1, "Other Due:", "Amount Due:");
                                    AmountDue = gc.Between(Year1, "Amount Due:", "Total Amount Paid:");
                                    TotalAmountPaid = gc.Between(Year1, "Total Amount Paid:", "Last Check Number:");
                                    LastCheckNumber = gc.Between(Year1, "Last Check Number:", "Last Pay Date:");
                                    LastPayDate = GlobalClass.After(Year1, "Last Pay Date:").Replace("\r\n", "").Trim();

                                }
                                if (p == 134)
                                {
                                    Taxyear = PaytaxinfoRow[134].Text.Trim();
                                    string Paytaxinformadetails = Taxyear.Trim() + "~" + Totalmarketval.Trim() + "~" + Homesteadcap.Trim() + "~" + Totalappraised.Trim() + "~" + Homesteadexemption.Trim() + "~" + DisabledPersonOverExemption.Trim() + "~" + DisabledVeteransExemption.Trim() + "~" + OtherExemptionDeferrals.Trim() + "~" + TaxableValue.Trim() + "~" + FrozenTaxInformation.Trim() + "~" + TaxRate.Trim() + "~" + CertifiedLevy.Trim() + "~" + CurrentLevy2.Trim() + "~" + TaxesDue.Trim() + "~" + PenaltyandInterest.Trim() + "~" + AttorneyFees.Trim() + "~" + OtherDue.Trim() + "~" + AmountDue.Trim() + "~" + TotalAmountPaid.Trim() + "~" + LastCheckNumber.Trim() + "~" + LastPayDate.Trim();
                                    gc.insert_date(orderNumber, Parcelnumber, 2150, Paytaxinformadetails, 1, DateTime.Now);
                                    Year1 = ""; Taxyear = ""; Totalmarketval = ""; Homesteadcap = ""; Totalappraised = ""; Homesteadexemption = ""; DisabledPersonOverExemption = ""; DisabledVeteransExemption = ""; OtherExemptionDeferrals = ""; TaxableValue = ""; FrozenTaxInformation = ""; TaxRate = ""; CertifiedLevy = ""; CurrentLevy2 = ""; TaxesDue = ""; PenaltyandInterest = ""; AttorneyFees = ""; OtherDue = ""; AmountDue = ""; TotalAmountPaid = ""; LastCheckNumber = ""; LastPayDate = "";
                                }
                                Cityofgary++;
                            }
                        }
                    }



                    if (Citytax.Contains("MESQUITE"))
                    {
                        driver.Navigate().GoToUrl("http://propertytax.cityofmesquite.com/MesquiteTax/");
                        Thread.Sleep(4000);
                        driver.FindElement(By.XPath("//*[@id='search']/table/tbody/tr/td[2]/input")).SendKeys(Parcelnumber);
                        gc.CreatePdf(orderNumber, Parcelnumber, "Tax search", driver, "TX", "Dallas");
                        driver.FindElement(By.Id("submit")).Click();
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, Parcelnumber, "Tax search Result", driver, "TX", "Dallas");
                        driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td/div/table[3]/tbody/tr[2]/td[1]/a/font")).Click();
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Assessment Details", driver, "TX", "Dallas");

                        string Accountno = "", Apd = "", Location = "", Legal = "", Owner = "", Acres = "", YearBuilt = "", Sqfeet = "", Defstart = "";
                        string DefEnd = "", Roll = "", UDI = "", Improvement = "", land = "";

                        Accountno = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[1]/tbody/tr/td[1]/table/tbody/tr[1]/td[2]/font")).Text;
                        Apd = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[1]/tbody/tr/td[1]/table/tbody/tr[2]/td[2]/font")).Text;
                        Location = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[1]/tbody/tr/td[1]/table/tbody/tr[3]/td[2]/font")).Text;
                        Legal = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[1]/tbody/tr/td[1]/table/tbody/tr[4]/td[2]/font")).Text;
                        Owner = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[1]/tbody/tr/td[1]/table/tbody/tr[5]/td[2]")).Text;
                        Acres = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[1]/tbody/tr/td[2]/table/tbody/tr[1]/td[2]/font")).Text;
                        YearBuilt = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td[2]/font")).Text;
                        Sqfeet = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[1]/tbody/tr/td[2]/table/tbody/tr[3]/td[2]/font")).Text;
                        Defstart = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[1]/tbody/tr/td[2]/table/tbody/tr[4]/td[2]/font")).Text;
                        DefEnd = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[1]/tbody/tr/td[2]/table/tbody/tr[5]/td[2]/font")).Text;
                        Roll = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[1]/tbody/tr/td[2]/table/tbody/tr[6]/td[2]/font")).Text;
                        UDI = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[1]/tbody/tr/td[2]/table/tbody/tr[7]/td[2]/font")).Text;
                        Improvement = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[1]/tbody/tr/td[3]/table/tbody/tr[2]/td[2]/font")).Text;
                        land = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[1]/tbody/tr/td[3]/table/tbody/tr[3]/td[2]/font")).Text;
                        //Parcelnumber
                        string propertytaxdetails = Accountno + "~" + Apd + "~" + Location + "~" + Legal + "~" + Owner + "~" + Acres + "~" + YearBuilt + "~" + Sqfeet + "~" + Defstart + "~" + DefEnd + "~" + Roll + "~" + UDI + "~" + Improvement + "~" + land;
                        gc.insert_date(orderNumber, Parcelnumber, 2151, propertytaxdetails, 1, DateTime.Now);

                        // Tax Details

                        IWebElement TaxInfo = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td/div[2]/table/tbody"));
                        IList<IWebElement> TRTaxInfo = TaxInfo.FindElements(By.TagName("tr"));
                        IList<IWebElement> THTaxInfo = TaxInfo.FindElements(By.TagName("th"));
                        IList<IWebElement> TDTaxInfo;
                        foreach (IWebElement row in TRTaxInfo)
                        {
                            TDTaxInfo = row.FindElements(By.TagName("td"));
                            if (TDTaxInfo.Count != 0 && !row.Text.Contains("Penalty"))
                            {
                                string TDTaxInfodetails = TDTaxInfo[0].Text + "~" + TDTaxInfo[1].Text + "~" + TDTaxInfo[2].Text + "~" + TDTaxInfo[3].Text + "~" + TDTaxInfo[4].Text + "~" + TDTaxInfo[5].Text + "~" + TDTaxInfo[6].Text + "~" + TDTaxInfo[7].Text + "~" + TDTaxInfo[8].Text + "~" + TDTaxInfo[9].Text;
                                gc.insert_date(orderNumber, Parcelnumber, 2152, TDTaxInfodetails, 1, DateTime.Now);
                            }
                        }

                    }

                    // Coppell

                    // string Citytax = "Coppell";
                    if (Citytax.Contains("COPPELL"))
                    {
                        driver.Navigate().GoToUrl("http://www.coppelltx.gov/government/departments/tax");
                        Thread.Sleep(4000);
                        driver.FindElement(By.XPath("//*[@id='ctl00_PlaceHolderMain_PageContent__ControlWrapper_RichHtmlField']/p[1]/a[3]/img")).Click();
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, Parcelnumber, "Property tax search", driver, "TX", "Dallas");
                        driver.FindElement(By.XPath("//*[@id='centertwocolumn']/table[3]/tbody/tr[1]/td/input")).Click();
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, Parcelnumber, "Property Tax search result", driver, "TX", "Dallas");
                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/div[1]/a[3]")).Click();
                        Thread.Sleep(4000);
                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/table/tbody/tr/td/center/form/table/tbody/tr[2]/td[2]/h3/input")).SendKeys(Parcelnumber);
                        gc.CreatePdf(orderNumber, Parcelnumber, "Tax Account search", driver, "TX", "Dallas");
                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/table/tbody/tr/td/center/form/table/tbody/tr[3]/td/center/input")).Click();
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, Parcelnumber, "Tax Account search Result", driver, "TX", "Dallas");
                        driver.FindElement(By.XPath("//*[@id='flextable']/tbody/tr/td[1]/a")).Click();
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, Parcelnumber, "Property Tax Details", driver, "TX", "Dallas");

                        string AcNo = "", PropertyAdd = "", ProSiteAdd = "", LegalDesc = "", Currenttaxlevy = "", CurrentAmountDue = "", PrioryearDue = "", TotalAmountDue = "";
                        string Marketval = "", landvalue = "", Improval = "", Cappedval = "", Agrival = "", Exemptions = "";

                        string taxdata = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[2]/tbody/tr/td[1]/h3")).Text.Replace("\r\n", " ");
                        AcNo = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[2]/tbody/tr/td[1]/h3/b[1]")).Text.Replace("Account Number:", "").Trim();
                        PropertyAdd = gc.Between(taxdata, "Address:", "Property Site Address:").Trim();
                        ProSiteAdd = gc.Between(taxdata, "Property Site Address:", "Legal Description:").Trim();
                        LegalDesc = gc.Between(taxdata, "Legal Description:", "Current Tax Levy:").Trim();
                        Currenttaxlevy = gc.Between(taxdata, "Current Tax Levy:", "Current Amount Due:").Trim();
                        CurrentAmountDue = gc.Between(taxdata, "Current Amount Due:", "Prior Year Amount Due:").Trim();
                        PrioryearDue = gc.Between(taxdata, "Prior Year Amount Due:", "Total Amount Due:").Trim();
                        TotalAmountDue = GlobalClass.After(taxdata, "Total Amount Due:").Trim();

                        string taxdata2 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[2]/tbody/tr/td[2]/h3[2]")).Text.Replace("\r\n", " ");
                        Marketval = gc.Between(taxdata2, "Market Value:", "Land Value:").Trim();
                        landvalue = gc.Between(taxdata2, "Land Value:", "Improvement Value:").Trim();
                        Improval = gc.Between(taxdata2, "Improvement Value:", "Capped Value:").Trim();
                        Cappedval = gc.Between(taxdata2, "Capped Value:", "Agricultural Value:").Trim();
                        Agrival = gc.Between(taxdata2, "Agricultural Value:", "Exemptions:").Trim();
                        Exemptions = gc.Between(taxdata2, "Exemptions:", "Current Tax Statement").Trim();

                        string propertytaxdetails = PropertyAdd + "~" + ProSiteAdd + "~" + LegalDesc + "~" + Currenttaxlevy + "~" + CurrentAmountDue + "~" + PrioryearDue + "~" + TotalAmountDue + "~" + Marketval + "~" + landvalue + "~" + Improval + "~" + Cappedval + "~" + Agrival + "~" + Exemptions;
                        gc.insert_date(orderNumber, Parcelnumber, 2156, propertytaxdetails, 1, DateTime.Now);


                        // Taxes Due Details by Year and Jurisdiction

                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[2]/tbody/tr/td[2]/h3[2]/a[3]")).Click();
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, Parcelnumber, "Taxes Due Details", driver, "TX", "Dallas");

                        IWebElement TaxDue = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/center/table/tbody/tr/td/table/tbody"));
                        IList<IWebElement> TRTaxDue = TaxDue.FindElements(By.TagName("tr"));
                        IList<IWebElement> THTaxDue = TaxDue.FindElements(By.TagName("th"));
                        IList<IWebElement> TDTaxDue;
                        foreach (IWebElement row in TRTaxDue)
                        {
                            TDTaxDue = row.FindElements(By.TagName("td"));
                            if (TDTaxDue.Count != 0 && !row.Text.Contains("by end of") && !row.Text.Contains("Base Tax Due") && !row.Text.Contains("No taxes due"))
                            {
                                string TDTaxInfodetails = TDTaxDue[0].Text + "~" + TDTaxDue[1].Text + "~" + TDTaxDue[2].Text + "~" + TDTaxDue[3].Text + "~" + TDTaxDue[4].Text + "~" + TDTaxDue[5].Text + "~" + TDTaxDue[6].Text + "~" + TDTaxDue[7].Text;
                                gc.insert_date(orderNumber, parcelNumber, 2157, TDTaxInfodetails, 1, DateTime.Now);
                            }
                            if (TDTaxDue.Count != 0 && !row.Text.Contains("by end of") && !row.Text.Contains("Base Tax Due") && row.Text.Contains("No taxes due"))
                            {
                                string TDTaxInfodetails = "" + "~" + TDTaxDue[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                gc.insert_date(orderNumber, Parcelnumber, 2157, TDTaxInfodetails, 1, DateTime.Now);
                            }
                        }

                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/h3[2]/a[1]")).Click();
                        Thread.Sleep(4000);

                        // payment Information

                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[2]/tbody/tr/td[2]/h3[2]/a[4]")).Click();
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, Parcelnumber, "Payment Information", driver, "TX", "Dallas");

                        IWebElement payinfo = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/table"));
                        IList<IWebElement> TRpayinfo = payinfo.FindElements(By.TagName("tr"));
                        IList<IWebElement> THpayinfo = payinfo.FindElements(By.TagName("th"));
                        IList<IWebElement> TDpayinfo;
                        foreach (IWebElement row in TRpayinfo)
                        {
                            TDpayinfo = row.FindElements(By.TagName("td"));
                            if (TDpayinfo.Count != 0 && !row.Text.Contains("Receipt Date"))
                            {
                                string payInfodetails = TDpayinfo[0].Text + "~" + TDpayinfo[1].Text + "~" + TDpayinfo[2].Text + "~" + TDpayinfo[3].Text;
                                gc.insert_date(orderNumber, Parcelnumber, 2158, payInfodetails, 1, DateTime.Now);
                            }
                            //if (TDpayinfo.Count != 0 && !row.Text.Contains("Receipt Date"))
                            //{
                            //    string payInfodetails = "" + "~" + TDpayinfo[0].Text + "~" + "" + "~" + "" ;
                            //    gc.insert_date(orderNumber, parcelNumber, 2158, payInfodetails, 1, DateTime.Now);
                            //}
                        }

                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td/h3[2]/a")).Click();
                        Thread.Sleep(4000);

                        // Composite Receipt

                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[2]/tbody/tr/td[2]/h3[2]/a[5]")).Click();
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, Parcelnumber, "Composite Receipt", driver, "TX", "Dallas");
                        var dd = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/form/div[3]/select"));
                        IWebElement SelectOption = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/form/div[3]/select"));
                        IList<IWebElement> Select = SelectOption.FindElements(By.TagName("option"));
                        List<string> option = new List<string>();
                        int count = 0, Check = 0;
                        foreach (IWebElement Op in Select)
                        {

                            if (Select.Count - 1 == Check)
                            {
                                var SelectAddress = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/form/div[3]/select"));
                                var SelectAddressTax = new SelectElement(SelectAddress);
                                SelectAddressTax.SelectByText(Op.Text);
                                Thread.Sleep(4000);
                                driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/form/div[3]/input")).Click();
                                Thread.Sleep(7000);
                                driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/div/h3/a")).Click();
                                Thread.Sleep(4000);
                                string currentwindow = driver.CurrentWindowHandle;
                                driver.SwitchTo().Window(driver.WindowHandles.Last());
                                string url = driver.Url;
                                gc.downloadfile(url, orderNumber, Parcelnumber, "PaymentRecord", "TX", "Dallas");
                                //  gc.CreatePdf(orderNumber, parcelNumber, "Payment Record", driver, "TX", "Dallas");
                                driver.SwitchTo().Window(currentwindow);
                                Thread.Sleep(4000);

                            }
                            Check++;
                        }



                        driver.Navigate().Back();
                        Thread.Sleep(4000);
                        driver.Navigate().Back();
                        Thread.Sleep(4000);

                        // Request an address Correction

                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[2]/tbody/tr/td[2]/h3[2]/a[6]")).Click();
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, Parcelnumber, "Request address correction", driver, "TX", "Dallas");
                        string certifiedAddress = "", AlternateAddress = "";
                        certifiedAddress = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/center/table/tbody/tr[2]/td[1]/h5")).Text.Replace("\r\n", " ");
                        AlternateAddress = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/center/table/tbody/tr[2]/td[2]/h5")).Text.Replace("\r\n", " ");

                        string RACdetails = certifiedAddress + "~" + AlternateAddress;
                        gc.insert_date(orderNumber, Parcelnumber, 2159, RACdetails, 1, DateTime.Now);
                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/h3[4]/a")).Click();
                        Thread.Sleep(4000);

                        // summary tax statement

                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[2]/tbody/tr/td[2]/h3[2]/a[2]")).Click();
                        Thread.Sleep(4000);

                        var chromeOptions = new ChromeOptions();

                        var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];

                        chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                        chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                        chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                        chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                        var driver1 = new ChromeDriver(chromeOptions);
                        driver1.Navigate().GoToUrl(driver.Url);
                        Thread.Sleep(2000);

                        try
                        {
                            driver1.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/h3/a")).Click();
                            Thread.Sleep(4000);
                            driver1.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/div[1]/a[3]")).Click();
                            Thread.Sleep(4000);
                            driver1.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/table/tbody/tr/td/center/form/table/tbody/tr[2]/td[2]/h3/input")).SendKeys(Parcelnumber);
                            driver1.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/table/tbody/tr/td/center/form/table/tbody/tr[3]/td/center/input")).Click();
                            Thread.Sleep(4000);
                            driver1.FindElement(By.XPath("//*[@id='flextable']/tbody/tr/td[1]/a")).Click();
                            Thread.Sleep(4000);
                            driver1.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[2]/tbody/tr/td[2]/h3[2]/a[2]")).Click();
                            Thread.Sleep(4000);
                        }
                        catch { }

                        driver1.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td/div/h3/a")).Click();
                        Thread.Sleep(4000);

                        string fileName = "";
                        fileName = latestfilename();
                        Thread.Sleep(3000);
                        gc.AutoDownloadFile(orderNumber, Parcelnumber, "Dallas", "TX", fileName);
                        Thread.Sleep(4000);
                        string currentwindow2 = driver1.Url;
                        driver1.SwitchTo().Window(driver1.WindowHandles.Last());
                        Thread.Sleep(4000);
                        //string url2 = driver.Url;
                        //gc.downloadfile(url2, orderNumber, parcelNumber, "Tax Statement", "TX", "Dallas");
                        //gc.CreatePdf(orderNumber, parcelNumber, "Tax Statement", driver1, "TX", "Dallas");
                        //driver1.SwitchTo().Window(currentwindow2);
                        driver1.Navigate().Back();
                        Thread.Sleep(4000);

                        // current Tax Statement
                        try
                        {
                            driver1.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/h3/a")).Click();
                            Thread.Sleep(4000);
                            driver1.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/div[1]/a[3]")).Click();
                            Thread.Sleep(4000);
                            driver1.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/table/tbody/tr/td/center/form/table/tbody/tr[2]/td[2]/h3/input")).SendKeys(Parcelnumber);
                            driver1.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/table/tbody/tr/td/center/form/table/tbody/tr[3]/td/center/input")).Click();
                            Thread.Sleep(4000);
                            driver1.FindElement(By.XPath("//*[@id='flextable']/tbody/tr/td[1]/a")).Click();
                            Thread.Sleep(4000);
                            driver1.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table[2]/tbody/tr/td[2]/h3[2]/a[1]")).Click();
                            Thread.Sleep(4000);
                        }
                        catch { }

                        string currentwindow1 = driver1.Url;
                        driver1.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td/div/h3/a")).Click();
                        Thread.Sleep(4000);
                        string fileName1 = "";
                        fileName1 = latestfilename();
                        Thread.Sleep(3000);
                        gc.AutoDownloadFile(orderNumber, Parcelnumber, "Dallas", "TX", fileName1);
                        Thread.Sleep(4000);
                        driver1.Quit();
                        try
                        {
                            driver1.Navigate().Back();
                            Thread.Sleep(4000);
                        }
                        catch { }



                    }





                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "TX", "Dallas", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    driver.Quit();
                    gc.mergpdf(orderNumber, "TX", "Dallas");
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
        public string latestfilename()
        {
            var downloadDirectory1 = ConfigurationManager.AppSettings["AutoPdf"];
            var files = new DirectoryInfo(downloadDirectory1).GetFiles("*.*");
            string latestfile = "";
            DateTime lastupdated = DateTime.MinValue;
            foreach (FileInfo file in files)
            {
                if (file.LastWriteTime > lastupdated)
                {
                    lastupdated = file.LastWriteTime;
                    latestfile = file.Name;
                }

            }
            return latestfile;
        }
    }
}