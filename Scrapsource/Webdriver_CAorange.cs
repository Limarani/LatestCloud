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
using System.ComponentModel;
using System.Text;
using HtmlAgilityPack;
using iTextSharp.text;
using System.Text.RegularExpressions;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Support.Extensions;
using System.Net;
using OpenQA.Selenium.Firefox;
using System.Diagnostics;
using OpenQA.Selenium.Remote;
using Org.BouncyCastle.Utilities;

namespace ScrapMaricopa.Scrapsource
{
    public class Webdriver_CAorange
    {
        string parcelno = "", Land = "", Parcelhref = "", multicount = "", Impro = "", personal = "", Other = "", housown = "", totnet = "", totvalue = "", Taxratarea = "", Homeowner_exepction = "", Taxing_Authority = "", Authority = "", Tax_Year = "", Tax_type = "", Tax_Status = "", Amt_Due = "", Paid_Amt = "", Paid_Date = "", Remark = "", Total_Due_Amount = "", Total_Amount_Paid = "", Pay_review_result = "", Property_Address = "";
        int Mcount = 0, WCount = 0;
        IWebDriver driver;
        IWebElement Parcelclick;
        List<string> strTaxRealestate = new List<string>();
        List<string> strTaxRealestate1 = new List<string>();
        List<string> viewbill = new List<string>();
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();
        public string FTP_Orange_Ca(string Address, string unitnumber, string ownernm, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            int b = 1;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            int i = 0, j = 0, k = 0, a = 1;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            // driver = new ChromeDriver();
            //driver = new PhantomJSDriver();
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("http://tax.ocgov.com/tcweb/search_page.asp");
                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, "", ownernm, Address, "CA", "Orange");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Zero_Orange"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {//*[@id="col2"]/div[2]/table/tbody/tr[2]/td/div/table/tbody/tr[6]/td/table/tbody/tr[10]/td[2]/input
                        driver.FindElement(By.Name("streetname")).SendKeys(Address);
                        gc.CreatePdf_WOP(orderNumber, "AddressSearch", driver, "CA", "Orange");
                        driver.FindElement(By.Name("s_address")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        try
                        {
                            string Nodata = driver.FindElement(By.XPath("//*[@id='col2']/div[2]/table/tbody/tr[2]/td/div/table/tbody/tr[1]/td/strong/font/text()[1]")).Text;
                            if (Nodata.Contains("No address was found"))
                            {
                                HttpContext.Current.Session["Zero_Orange"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                        gc.CreatePdf_WOP(orderNumber, "AddressSearchResult", driver, "CA", "Orange");
                        try
                        {//*[@id="col2"]/div[2]/table/tbody/tr[3]/td/table/tbody/tr[6]/td/table/tbody/tr[3]/td[1]
                            string Pay_review_result = driver.FindElement(By.XPath("//*[@id='col2']/div[2]/table/tbody/tr[3]/td/table/tbody/tr[6]/td/table/tbody/tr[3]/td[1]")).Text;
                            if (Pay_review_result.Contains("Search Results: 1-1 of 1 records"))
                            {
                                driver.FindElement(By.XPath("//*[@id='col2']/div[2]/table/tbody/tr[3]/td/table/tbody/tr[7]/td/table/tbody/tr[2]/td[2]/a")).SendKeys(Keys.Enter);
                                Thread.Sleep(3000);
                            }
                            multicount = gc.Between(Pay_review_result, "of ", " records");
                        }
                        catch { }
                        try
                        {
                            
                            if (Pay_review_result.Trim() != "Search Results: 1-1 of 1 records" && Convert.ToInt32(multicount) <= 20)
                            {
                                IWebElement MProperty_addrs = driver.FindElement(By.XPath("//*[@id='col2']/div[2]/table/tbody/tr[3]/td/table/tbody/tr[7]/td/table/tbody"));
                                IList<IWebElement> Mproperty = MProperty_addrs.FindElements(By.TagName("tr"));
                                IList<IWebElement> Mpropertyid;
                                foreach (IWebElement Mpropertyrow in Mproperty)
                                {
                                    Mpropertyid = Mpropertyrow.FindElements(By.TagName("td"));
                                    if (Mpropertyid.Count != 0 && !Mpropertyrow.Text.Contains("Property or Business Address") && Mpropertyid[0].Text.Contains(Address.ToUpper()))
                                    {
                                        if (Mcount < 20)
                                        {
                                            Address = Mpropertyid[0].Text;
                                            parcelno = Mpropertyid[1].Text;
                                            Parcelclick = Mpropertyid[1].FindElement(By.TagName("a"));
                                            Parcelhref = Parcelclick.GetAttribute("href");
                                            gc.insert_date(orderNumber, parcelno, 553, Address, 1, DateTime.Now);
                                        }
                                        Mcount++;
                                    }

                                }
                            }
                            if (Mcount == 1)
                            {
                                driver.Navigate().GoToUrl(Parcelhref);
                                Thread.Sleep(2000);
                            }
                            if (Mcount > 20 || Convert.ToInt32(multicount) > 20)
                            {
                                HttpContext.Current.Session["multiParcel_Orange_Multicount"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                            if (Mcount > 1 && Mcount < 20)
                            {
                                HttpContext.Current.Session["multiparcel_Orange"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            gc.CreatePdf_WOP(orderNumber, "MultiAddressSearch", driver, "CA", "Orange");

                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {
                        if ((HttpContext.Current.Session["titleparcel"] != null))
                        {
                            parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        }
                        driver.FindElement(By.Name("t_parcel_no")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcelsearch", driver, "CA", "Orange");
                        driver.FindElement(By.Name("s_parcel")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                    }
                    string Parcel = driver.FindElement(By.XPath("//*[@id='col2']/div[2]/table/tbody/tr[3]/td/table/tbody/tr[2]/td/table/tbody/tr[3]/td[1]")).Text;
                    Parcel = GlobalClass.After(Parcel, "Parcel Number :").Trim();
                    gc.CreatePdf(orderNumber, Parcel, "property details", driver, "CA", "Orange");
                    IList<IWebElement> tables = driver.FindElements(By.XPath("//*[@id='col2']/div[2]/table/tbody/tr[3]/td/table/tbody/tr[2]/td/table/tbody/tr"));
                    int count = tables.Count;
                    foreach (IWebElement tab in tables)
                    {
                        if (tab.Text.Contains("Current Year and Unpaid Non-Delinquent Tax Bills"))
                        {
                            IList<IWebElement> ITaxRealRowQ = tab.FindElements(By.TagName("tr"));
                            IList<IWebElement> ITaxRealTdQ;
                            foreach (IWebElement ItaxReal in ITaxRealRowQ)
                            {
                                ITaxRealTdQ = ItaxReal.FindElements(By.TagName("td"));
                                if (!ItaxReal.Text.Contains("Current Year and Unpaid Non-Delinquent Tax Bills") && !ItaxReal.Text.Contains("Parcel Number") && ITaxRealTdQ[0].Text.Trim() != "")
                                {
                                    IWebElement ITaxBillCount = ITaxRealTdQ[0].FindElement(By.TagName("a"));
                                    string strTaxReal = ITaxBillCount.GetAttribute("href");
                                    strTaxRealestate.Add(strTaxReal);
                                }
                            }
                        }
                    }
                    IList<IWebElement> tables1 = driver.FindElements(By.XPath("//*[@id='col2']/div[2]/table/tbody/tr[3]/td/table/tbody/tr[2]/td/table/tbody/tr"));
                    int count1 = tables.Count;
                    foreach (IWebElement tab in tables1)
                    {
                        if (tab.Text.Contains("Previous Year Tax Payment Information"))
                        {
                            IList<IWebElement> ITaxRealRowQ1 = tab.FindElements(By.TagName("tr"));
                            IList<IWebElement> ITaxRealTdQ1;
                            foreach (IWebElement ItaxReal1 in ITaxRealRowQ1)
                            {
                                ITaxRealTdQ1 = ItaxReal1.FindElements(By.TagName("td"));
                                if (ITaxRealTdQ1.Count == 4)
                                {
                                    try
                                    {
                                        IWebElement ITaxBillCount = ITaxRealTdQ1[0].FindElement(By.TagName("a"));
                                        string strTaxReal = ITaxBillCount.GetAttribute("href");
                                        strTaxRealestate.Add(strTaxReal);
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                    IList<IWebElement> tables2 = driver.FindElements(By.XPath("//*[@id='col2']/div[2]/table/tbody/tr[3]/td/table/tbody/tr[2]/td/table/tbody/tr"));
                    int count2 = tables.Count;
                    foreach (IWebElement tab in tables2)
                    {
                        if (tab.Text.Contains("Parcel Number Tax Default No."))
                        {
                            IList<IWebElement> ITaxRealRowQ2 = tab.FindElements(By.TagName("tr"));
                            IList<IWebElement> ITaxRealTdQ2;
                            foreach (IWebElement ItaxReal1 in ITaxRealRowQ2)
                            {
                                ITaxRealTdQ2 = ItaxReal1.FindElements(By.TagName("td"));
                                if (ITaxRealTdQ2.Count == 4 && !ItaxReal1.Text.Contains("Tax Default No") && ITaxRealTdQ2[0].Text.Trim() != "" && ITaxRealTdQ2[1].Text.Trim() != "")
                                {
                                    IWebElement ITaxBillCount = ITaxRealTdQ2[1].FindElement(By.TagName("a"));
                                    string strTaxReal = ITaxBillCount.GetAttribute("href");
                                    strTaxRealestate1.Add(strTaxReal);
                                }
                            }
                        }
                    }
                    //deliquent tax
                    foreach (string real1 in strTaxRealestate1)
                    {
                        driver.Navigate().GoToUrl(real1);
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, Parcel, "Deliquent tax", driver, "CA", "Orange");
                        string tax_no = "", deli_year = "", status = "";
                        tax_no = driver.FindElement(By.XPath("//*[@id='col2']/div[2]/table/tbody/tr[3]/td/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]")).Text;
                        deli_year = driver.FindElement(By.XPath("//*[@id='col2']/div[2]/table/tbody/tr[3]/td/table/tbody/tr[2]/td/table/tbody/tr[3]/td[2] ")).Text;
                        status = driver.FindElement(By.XPath("//*[@id='col2']/div[2]/table/tbody/tr[3]/td/table/tbody/tr[2]/td/table/tbody/tr[4]/td[2] ")).Text;

                        IWebElement multitableElement11 = driver.FindElement(By.XPath("//*[@id='col2']/div[2]/table/tbody/tr[4]/td/form/table/tbody"));
                        IList<IWebElement> multitableRow11 = multitableElement11.FindElements(By.TagName("tr"));
                        IList<IWebElement> multirowTD11;
                        //   Tax Default No.(TDN)~First Year of Delinquency~Tax Status~TDN Parcels~Roll Year~Taxes~Basic Penalties~Cost~Additional Penalties~Total
                        foreach (IWebElement row in multitableRow11)
                        {
                            try
                            {
                                multirowTD11 = row.FindElements(By.TagName("td"));
                                if (multirowTD11[0].Text.Trim() != "" && !row.Text.Contains("TDN Parcels"))
                                {
                                    if (multirowTD11.Count == 7)
                                    {
                                        string tax_deli1 = tax_no + "~" + deli_year + "~" + status + "~" + multirowTD11[0].Text + "~" + multirowTD11[1].Text + "~" + multirowTD11[2].Text + "~" + multirowTD11[3].Text + "~" + multirowTD11[4].Text + "~" + multirowTD11[5].Text + "~" + multirowTD11[6].Text;
                                        gc.insert_date(orderNumber, Parcel, 565, tax_deli1, 1, DateTime.Now);
                                    }
                                    if (multirowTD11.Count == 4)
                                    {
                                        string tax_deli1 = tax_no + "~" + deli_year + "~" + status + "~" + multirowTD11[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + multirowTD11[3].Text;
                                        gc.insert_date(orderNumber, Parcel, 565, tax_deli1, 1, DateTime.Now);
                                    }
                                    if (multirowTD11.Count == 6)
                                    {
                                        string tax_deli1 = tax_no + "~" + deli_year + "~" + status + "~" + multirowTD11[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + multirowTD11[5].Text;
                                        gc.insert_date(orderNumber, Parcel, 565, tax_deli1, 1, DateTime.Now);
                                    }
                                }

                            }
                            catch { }
                        }
                    }
                    string address = "", taxrate = "", rolltype = "", parcelnumber = "", fiscal = "", fiscal_year = "";
                    //property,assessment,taxinfo
                    int c = 1;
                    foreach (string real in strTaxRealestate)
                    {
                        driver.Navigate().GoToUrl(real);
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, Parcel, "taxinfo details" + c, driver, "CA", "Orange");
                        c++;
                        try
                        {
                            try
                            {
                                fiscal = driver.FindElement(By.XPath("//*[@id='col2']/div[2]/table/tbody/tr[1]/td")).Text;
                                if (fiscal == "Property Tax Information")
                                {

                                }
                                else
                                {
                                    fiscal_year = GlobalClass.After(fiscal, "FISCAL YEAR").Trim();
                                }
                            }
                            catch { }
                            try
                            {
                                rolltype = driver.FindElement(By.XPath("//*[@id='col2']/div[2]/table/tbody/tr[3]/td/table/tbody/tr[5]/td[2]")).Text;
                            }
                            catch
                            {
                                rolltype = "";
                            }
                            if (rolltype == "Secured")
                            {
                                if (a == 1)
                                {
                                    try
                                    {
                                        IWebElement multitableElement1 = driver.FindElement(By.XPath("//*[@id='col2']/div[2]/table/tbody/tr[3]/td/table/tbody"));
                                        IList<IWebElement> multitableRow1 = multitableElement1.FindElements(By.TagName("tr"));
                                        IList<IWebElement> multirowTD1;

                                        foreach (IWebElement row in multitableRow1)
                                        {
                                            multirowTD1 = row.FindElements(By.TagName("td"));
                                            if (multirowTD1[0].Text.Trim() != "")
                                            {
                                                if (i == 0)
                                                {
                                                    parcelnumber = multirowTD1[1].Text;
                                                    parcelnumber = GlobalClass.Before(parcelnumber, "View Original Bill").Trim();
                                                }
                                                if (i == 1)
                                                {
                                                    address = multirowTD1[1].Text;
                                                }
                                                if (i == 2)
                                                {
                                                    taxrate = multirowTD1[1].Text;
                                                }
                                                if (i == 3)
                                                {
                                                    rolltype = multirowTD1[1].Text;
                                                }
                                                i++;
                                            }
                                        }
                                    }
                                    catch { }
                                    //property
                                    gc.insert_date(orderNumber, Parcel, 548, address + "~" + taxrate, 1, DateTime.Now);
                                    //Assessment
                                    string Land_Value = "", Mineral_Rights = "", Improvement_Value = "", Personal_Property = "", Others = "", Total_Assessed_Value = "", Homeowner_Exemption = "", Net_Assessed_Value = "";
                                    IWebElement multitableElement = driver.FindElement(By.XPath("//*[@id='col2']/div[2]/table/tbody/tr[7]/td/table/tbody"));
                                    IList<IWebElement> multitableRow = multitableElement.FindElements(By.TagName("tr"));
                                    IList<IWebElement> multirowTD;
                                    foreach (IWebElement row in multitableRow)
                                    {
                                        multirowTD = row.FindElements(By.TagName("td"));
                                        if (multirowTD.Count == 4)
                                        {
                                            if ((multirowTD[2].Text.Trim() != "") && !row.Text.Contains("Assessed Values and Exemptions") && !row.Text.Contains("Description") && multirowTD.Count == 4 && !row.Text.Contains("Total Due and Payable"))
                                            {
                                                if (k == 0)
                                                {
                                                    Land_Value = multirowTD[2].Text;
                                                }
                                                if (k == 1)
                                                {
                                                    Mineral_Rights = multirowTD[2].Text;
                                                }
                                                if (k == 2)
                                                {
                                                    Improvement_Value = multirowTD[2].Text;
                                                }
                                                if (k == 3)
                                                {
                                                    Personal_Property = multirowTD[2].Text;
                                                }
                                                if (k == 4)
                                                {
                                                    Others = multirowTD[2].Text;
                                                }
                                                if (k == 5)
                                                {
                                                    Total_Assessed_Value = multirowTD[2].Text;
                                                }
                                                if (k == 6)
                                                {
                                                    Homeowner_Exemption = multirowTD[2].Text;
                                                }
                                                if (k == 7)
                                                {
                                                    Net_Assessed_Value = multirowTD[2].Text;
                                                }
                                                k++;
                                            }
                                        }
                                    }
                                    string assessment_details = Land_Value + "~" + Mineral_Rights + "~" + Improvement_Value + "~" + Personal_Property + "~" + Others + "~" + Total_Assessed_Value + "~" + Homeowner_Exemption + "~" + Net_Assessed_Value;
                                    gc.insert_date(orderNumber, Parcel, 549, assessment_details, 1, DateTime.Now);
                                }
                                a++;
                            }
                            else
                            {
                                try
                                {
                                    IWebElement multitableElement = driver.FindElement(By.XPath("//*[@id='col2']/div[2]/table/tbody"));
                                    IList<IWebElement> multitableRow = multitableElement.FindElements(By.TagName("tr"));
                                    IList<IWebElement> multirowTD;
                                    j = 0;
                                    foreach (IWebElement row in multitableRow)
                                    {
                                        multirowTD = row.FindElements(By.TagName("td"));
                                        if (multirowTD[0].Text.Trim() != "" && !row.Text.Contains("Current Year and Unpaid Non-Delinquent Tax Bills") && multirowTD.Count == 2)
                                        {
                                            if (j == 0)
                                            {
                                                parcelnumber = multirowTD[1].Text;
                                                parcelnumber = GlobalClass.Before(parcelnumber, "View Bill").Trim();
                                            }
                                            if (j == 1)
                                            {
                                                address = multirowTD[1].Text;
                                            }
                                            if (j == 2)
                                            {
                                                taxrate = multirowTD[1].Text;
                                            }
                                            if (j == 3)
                                            {
                                                rolltype = multirowTD[1].Text;
                                                if (rolltype.Contains("("))
                                                {
                                                    rolltype = GlobalClass.Before(rolltype, "(").Trim();
                                                }
                                            }
                                            j++;
                                        }
                                    }
                                }
                                catch { }
                            }
                            IWebElement tbmulti;
                            //Parcel Number~Tax Year~Tax Type~Installment Type~Due Date~Tax Status~Amount Due~Remarks~Total Due Amount~Total Amt Paid~Paid Date~Paid Amount
                            string InstallmentType = "", Due_Date = "", Tax_Status = "", Amount_Due = "", Remarks = "", Total_Due_Amount = "", Total_Amt_Paid = "", Paid_Amount = "", Paid_Date = "";
                            string taxinfo = "", taxinfo1 = "", taxinfo2 = "";
                            try
                            {
                                tbmulti = driver.FindElement(By.XPath("//*[@id='col2']/div[2]/table/tbody/tr[4]/td/table/tbody"));
                            }
                            catch
                            {
                                tbmulti = driver.FindElement(By.XPath("//*[@id='col2']/div[2]/table/tbody/tr[9]/td/table/tbody"));

                            }
                            IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDmulti;
                            int l = 0;
                            foreach (IWebElement row in TRmulti)
                            {
                                TDmulti = row.FindElements(By.TagName("td"));
                                if (!row.Text.Contains("Installments") && !row.Text.Contains("Total Due and Payable"))
                                {
                                    if (TDmulti.Count == 6)
                                    {
                                        if (l == 0)
                                        {
                                            InstallmentType = TDmulti[1].Text;
                                            Due_Date = TDmulti[2].Text;
                                            Tax_Status = TDmulti[3].Text;
                                            Amount_Due = TDmulti[4].Text;
                                            Remarks = TDmulti[5].Text;
                                            taxinfo = parcelnumber + "~" + fiscal_year + "~" + rolltype + "~" + InstallmentType + "~" + Due_Date + "~" + Tax_Status + "~" + Amount_Due + "~" + Remarks + "~" + "" + "~" + "";
                                        }
                                        if (l == 1)
                                        {
                                            InstallmentType = TDmulti[1].Text;
                                            Due_Date = TDmulti[2].Text;
                                            Tax_Status = TDmulti[3].Text;
                                            Amount_Due = TDmulti[4].Text;
                                            Remarks = TDmulti[5].Text;
                                            taxinfo1 = parcelnumber + "~" + fiscal_year + "~" + rolltype + "~" + InstallmentType + "~" + Due_Date + "~" + Tax_Status + "~" + Amount_Due + "~" + Remarks;
                                        }
                                        l++;
                                    }
                                }
                                if (row.Text.Contains("Total Due and Payable"))
                                {
                                    Total_Due_Amount = TDmulti[3].Text;
                                    taxinfo2 = taxinfo1 + "~" + Total_Due_Amount;
                                }
                            }
                            string taxinfoNew = "";
                            IWebElement tbmulti1;
                            try
                            {
                                tbmulti1 = driver.FindElement(By.XPath("//*[@id='col2']/div[2]/table/tbody/tr[6]/td/table/tbody"));
                            }
                            catch
                            {
                                tbmulti1 = driver.FindElement(By.XPath("//*[@id='col2']/div[2]/table/tbody/tr[12]/td/table/tbody"));
                            }
                            IList<IWebElement> TRmulti1 = tbmulti1.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDmulti1;
                            int m = 0;
                            foreach (IWebElement row in TRmulti1)
                            {
                                TDmulti1 = row.FindElements(By.TagName("td"));
                                if (!row.Text.Contains("Payment Summary") && !row.Text.Contains("Installments"))
                                {
                                    if (TDmulti1.Count == 6)
                                    {
                                        if (m == 0)
                                        {
                                            Paid_Date = TDmulti1[2].Text;
                                            Paid_Amount = TDmulti1[4].Text;
                                            taxinfo = taxinfo + "~" + Paid_Date + "~" + Paid_Amount + "~" + "Orange County Tax Collector, P.O.Box 1438 Santa Ana, CA 92702-1438";
                                            gc.insert_date(orderNumber, Parcel, 556, taxinfo, 1, DateTime.Now);
                                        }//ATTN: Orange County Tax Collector P.O.Box 1438 Santa Ana, CA 92702-1438
                                        if (m == 1)
                                        {
                                            Paid_Date = TDmulti1[2].Text;
                                            Paid_Amount = TDmulti1[4].Text;
                                            taxinfoNew = Paid_Date + "~" + Paid_Amount;
                                        }
                                        m++;
                                    }
                                }
                                if (row.Text.Contains("Total Amt Paid") && TDmulti1.Count == 5)
                                {
                                    string total_amount_due = TDmulti1[3].Text;
                                    taxinfo2 = taxinfo2 + "~" + total_amount_due + "~" + taxinfoNew + "~" + "";
                                    gc.insert_date(orderNumber, Parcel, 556, taxinfo2, 1, DateTime.Now);
                                }
                            }

                        }
                        catch { }
                    }
                    //download bill
                    try
                    {
                        foreach (string real2 in strTaxRealestate)
                        {
                            var chDriver = new ChromeDriver();
                            WebDriverWait waittime = new WebDriverWait(chDriver, TimeSpan.FromSeconds(5));
                            chDriver.Navigate().GoToUrl(real2);
                            Thread.Sleep(4000);
                            try
                            {
                                IWebElement Itaxstmt = chDriver.FindElement(By.LinkText("View Original Bill"));
                                string stmt1 = Itaxstmt.GetAttribute("href");
                                Itaxstmt.Click();
                            }
                            catch
                            {
                                IWebElement Itaxstmt = chDriver.FindElement(By.LinkText("View Bill"));
                                string stmt1 = Itaxstmt.GetAttribute("href");
                                Itaxstmt.Click();
                            }
                            chDriver.SwitchTo().Window(chDriver.WindowHandles.Last());
                            string currentURL = chDriver.Url;
                            gc.downloadfile(currentURL, orderNumber, Parcel, "Billdown" + b, "CA", "Orange");
                            b++;
                            chDriver.Quit();
                        }
                    }
                    catch
                    { }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "CA", "Orange", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    driver.Quit();
                    gc.mergpdf(orderNumber, "CA", "Orange");
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








