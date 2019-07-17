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
    public class WebDriver_DuvalFL
    {
        string Outparcelno = "", outputPath = "";
        IWebDriver driver;
        IWebElement histaxdettable;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_Duval(string streetno, string streetname, string streettype, string unitnumber, string ownername, string parcelNumber, string searchType, string orderNumber)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            //GlobalClass.global_county = county;
            string outparcelno = "", siteaddr = "", mail_addr = "", owner1 = "", tax_dist = "", legal_desc = "", year_built = "", subdivision = "", propuse = "";
            string valued_year = "", tax_year = "", assess_land = "", ass_improve = "", ass_total = "", tax_value = "", exem = "", pathid = "", Total_taxes = "";
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            int amccount = 0;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {

                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("https://paopropertysearch.coj.net/Basic/Search.aspx");
                    if (searchType == "titleflex")
                    {
                        string titleaddress = streetno + " " + streetname + " " + streettype + " " + unitnumber;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "FL", "Duval");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_DuvalFL"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        driver.FindElement(By.Id("ctl00_cphBody_tbStreetNumber")).SendKeys(streetno);
                        driver.FindElement(By.Id("ctl00_cphBody_tbStreetName")).SendKeys(streetname);
                        driver.FindElement(By.Id("ctl00_cphBody_tbStreetUnit")).SendKeys(unitnumber);
                        gc.CreatePdf_WOP(orderNumber, "Input Passed Address Search", driver, "FL", "Duval");
                        driver.FindElement(By.Id("ctl00_cphBody_bSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search Results", driver, "FL", "Duval");
                        string reccount = driver.FindElement(By.Id("ctl00_cphBody_lblResultsCount")).Text.Trim().Replace(",", "");
                        reccount = reccount.Replace(" properties found", "");
                        int count = Int32.Parse(reccount);
                        if (count > 1)
                        {
                            multiparcel(orderNumber);
                            driver.Quit();
                            return "MultiParcel";
                        }

                        try
                        {
                            string nodata = driver.FindElement(By.XPath("//*[@id='noResults']/h3")).Text;
                            if (nodata.Contains("No Results Found"))
                            {
                                HttpContext.Current.Session["Nodata_DuvalFL"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }

                    }

                    if (searchType == "parcel")
                    {

                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();

                        }
                        if (GlobalClass.titleparcel.Contains(".") || GlobalClass.titleparcel.Contains("-"))
                        {
                            parcelNumber = GlobalClass.titleparcel;
                        }

                        //ctl00_cphBody_tbRE6
                        //ctl00_cphBody_tbRE4
                        var Parsplit = parcelNumber.Split('-');
                        int count = parcelNumber.Replace("-", "").Count();
                        string Pa1 = "";
                        string Pa2 = "";
                        if (count == 10)
                        {
                            Pa1 = Parsplit[0];
                            Pa2 = Parsplit[1];
                            driver.FindElement(By.Id("ctl00_cphBody_tbRE6")).SendKeys(Pa1);
                            driver.FindElement(By.Id("ctl00_cphBody_tbRE4")).SendKeys(Pa2);
                            gc.CreatePdf_WOP(orderNumber, "Input Passed Parcel Search", driver, "FL", "Duval");
                            driver.FindElement(By.Id("ctl00_cphBody_bSearch")).SendKeys(Keys.Enter);
                            Thread.Sleep(3000);
                            gc.CreatePdf_WOP(orderNumber, "Parcel Search Results", driver, "FL", "Duval");
                            string reccount = driver.FindElement(By.Id("ctl00_cphBody_lblResultsCount")).Text.Trim().Replace(",", "");
                            reccount = reccount.Replace(" properties found", "");
                            int count1 = Int32.Parse(reccount);
                            if (count1 > 1)
                            {
                                multiparcel(orderNumber);
                                driver.Quit();
                                return "MultiParcel";
                            }
                            try
                            {
                                string nodata = driver.FindElement(By.XPath("//*[@id='noResults']/h3")).Text;
                                if (nodata.Contains("No Results Found"))
                                {
                                    HttpContext.Current.Session["Nodata_DuvalFL"] = "Zero";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }

                        }
                    }

                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("ctl00_cphBody_tbName")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "Input Passed Ownername Search", driver, "FL", "Duval");
                        driver.FindElement(By.Id("ctl00_cphBody_bSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Ownername Search Results", driver, "FL", "Duval");
                        string reccount = driver.FindElement(By.Id("ctl00_cphBody_lblResultsCount")).Text.Trim().Replace(",", "");
                        reccount = reccount.Replace(" properties found", "");
                        int count = Int32.Parse(reccount);
                        if (count > 1)
                        {
                            multiparcel(orderNumber);
                            driver.Quit();
                            return "MultiParcel";
                        }
                        try
                        {
                            string nodata = driver.FindElement(By.XPath("//*[@id='noResults']/h3")).Text;
                            if (nodata.Contains("No Results Found"))
                            {
                                HttpContext.Current.Session["Nodata_DuvalFL"] = "Zero";
                                driver.Quit();
                                return "No Data Found";

                            }
                        }
                        catch { }

                    }

                    Amrock amc = new Amrock();

                    //property details
                    Thread.Sleep(3000);
                    driver.FindElement(By.XPath("//*[@id='ctl00_cphBody_gridResults']/tbody/tr[2]/td[1]/a")).SendKeys(Keys.Enter);
                    gc.CreatePdf(orderNumber, outparcelno, "Assessment Details", driver, "FL", "Duval");
                    ownername = driver.FindElement(By.Id("ctl00_cphBody_repeaterOwnerInformation_ctl00_lblOwnerName")).Text.Trim();

                    try
                    {
                        ownername += ",";
                        ownername += driver.FindElement(By.Id("ctl00_cphBody_repeaterOwnerInformation_ctl01_lblOwnerName")).Text.Trim();
                    }
                    catch { }
                    siteaddr = driver.FindElement(By.XPath("//*[@id='primaryAddr']/div")).Text.Trim().Replace("\r\n", ",");
                    outparcelno = driver.FindElement(By.Id("ctl00_cphBody_lblRealEstateNumber")).Text.Trim();
                    amc.TaxId = outparcelno;
                    tax_dist = driver.FindElement(By.Id("ctl00_cphBody_lblTaxDistrict")).Text.Trim();
                    propuse = driver.FindElement(By.Id("ctl00_cphBody_lblPropertyUse")).Text.Trim();
                    subdivision = driver.FindElement(By.Id("ctl00_cphBody_lblSubdivision")).Text.Trim();
                    try
                    {
                        //amc Year Built
                        year_built = driver.FindElement(By.Id("ctl00_cphBody_repeaterBuilding_ctl00_lblYearBuilt")).Text.Trim();
                    }
                    catch { }
                    gc.CreatePdf(orderNumber, outparcelno, "Assessment Details", driver, "FL", "Duval");
                    //legaldesc
                    IWebElement legaldes = driver.FindElement(By.Id("ctl00_cphBody_gridLegal"));
                    IList<IWebElement> legaldesRow = legaldes.FindElements(By.TagName("tr"));
                    int rowcount = legaldesRow.Count;
                    IList<IWebElement> legaldesRowTD;
                    int a = 0;
                    foreach (IWebElement rowid in legaldesRow)
                    {
                        legaldesRowTD = rowid.FindElements(By.TagName("td"));
                        if (legaldesRowTD.Count != 0 && !rowid.Text.Contains("LN"))
                        {

                            if (a == 0)
                            {
                                legal_desc = legaldesRowTD[1].Text;

                            }
                            else
                            {
                                legal_desc += ",";
                                legal_desc += legaldesRowTD[1].Text;
                            }

                            a++;
                        }

                    }

                    string property = siteaddr + "~" + ownername + "~" + tax_dist + "~" + propuse + "~" + legal_desc + "~" + subdivision + "~" + year_built;
                    gc.insert_date(orderNumber, outparcelno, 331, property, 1, DateTime.Now);

                    //valuation Information                
                    IWebElement valuetableElement = driver.FindElement(By.XPath("//*[@id='propValue']/table"));
                    IList<IWebElement> valuetableRow = valuetableElement.FindElements(By.TagName("tr"));
                    IList<IWebElement> valuerowTD;
                    IList<IWebElement> valuerowTH;
                    List<string> history = new List<string>();
                    List<string> ValueMethod = new List<string>();
                    List<string> TotalBuildingValue = new List<string>();
                    List<string> ExtraFeatureValue = new List<string>();
                    List<string> LandValue_Market = new List<string>();
                    List<string> LandValue_Agric = new List<string>();
                    List<string> Just_MarketValue = new List<string>();
                    List<string> AssessedValue = new List<string>();
                    List<string> Cap_DiffPortability_Amt = new List<string>();
                    List<string> Exemptions = new List<string>();
                    List<string> TaxableValue = new List<string>();
                    int i = 0;
                    foreach (IWebElement row in valuetableRow)
                    {
                        valuerowTH = row.FindElements(By.TagName("th"));
                        valuerowTD = row.FindElements(By.TagName("td"));
                        if (i == 0)
                        {
                            if (valuerowTH.Count != 0)
                            {
                                history.Add(valuerowTH[1].Text.Trim().Replace("\r\n", " "));
                                history.Add(valuerowTH[2].Text.Trim().Replace("\r\n", " "));
                            }
                        }
                        if (valuerowTD.Count != 0)
                        {
                            if (i == 1)
                            {
                                ValueMethod.Add(valuerowTD[0].Text);
                                ValueMethod.Add(valuerowTD[1].Text);
                            }
                            else if (i == 2)
                            {
                                TotalBuildingValue.Add(valuerowTD[0].Text);
                                TotalBuildingValue.Add(valuerowTD[1].Text);
                            }
                            else if (i == 3)
                            {
                                ExtraFeatureValue.Add(valuerowTD[0].Text);
                                ExtraFeatureValue.Add(valuerowTD[1].Text);
                            }
                            else if (i == 4)
                            {
                                LandValue_Market.Add(valuerowTD[0].Text);
                                LandValue_Market.Add(valuerowTD[1].Text);
                            }
                            else if (i == 5)
                            {
                                LandValue_Agric.Add(valuerowTD[0].Text);
                                LandValue_Agric.Add(valuerowTD[1].Text);
                            }
                            else if (i == 6)
                            {
                                Just_MarketValue.Add(valuerowTD[0].Text);
                                Just_MarketValue.Add(valuerowTD[1].Text);
                            }

                            else if (i == 7)
                            {
                                AssessedValue.Add(valuerowTD[0].Text);
                                //amc.TotalAssessedValue = valuerowTD[0].Text;
                                AssessedValue.Add(valuerowTD[1].Text);
                            }

                            else if (i == 8)
                            {
                                Cap_DiffPortability_Amt.Add(valuerowTD[0].Text.Trim().Replace("\r\n", ","));
                                Cap_DiffPortability_Amt.Add(valuerowTD[1].Text.Trim().Replace("\r\n", ","));
                            }

                            else if (i == 9)
                            {
                                Exemptions.Add(valuerowTD[0].Text);
                                Exemptions.Add(valuerowTD[1].Text);
                            }

                            else if (i == 10)
                            {
                                TaxableValue.Add(valuerowTD[0].Text);
                                TaxableValue.Add(valuerowTD[1].Text);
                            }
                        }
                        i++;
                    }
                    string value1 = history[0] + "~" + ValueMethod[0] + "~" + TotalBuildingValue[0] + "~" + ExtraFeatureValue[0] + "~" + LandValue_Market[0] + "~" + LandValue_Agric[0] + "~" + Just_MarketValue[0] + "~" + AssessedValue[0] + "~" + Cap_DiffPortability_Amt[0] + "~" + Exemptions[0] + "~" + TaxableValue[0];
                    gc.insert_date(orderNumber, outparcelno, 332, value1, 1, DateTime.Now);
                    string value2 = history[1] + "~" + ValueMethod[1] + "~" + TotalBuildingValue[1] + "~" + ExtraFeatureValue[1] + "~" + LandValue_Market[1] + "~" + LandValue_Agric[1] + "~" + Just_MarketValue[1] + "~" + AssessedValue[1] + "~" + Cap_DiffPortability_Amt[1] + "~" + Exemptions[1] + "~" + TaxableValue[1];
                    gc.insert_date(orderNumber, outparcelno, 332, value2, 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    //Tax Details
                    string tax_type = "", tlegal_desc = "", curr_tax_status = "", curr_delinq_status = "", unpaid_tax = "", strTaxYear = "", strTaxFolio = "", strTaxCertificateYear = "", strTaxCertificateNo = "", strTaxCertifcateName = "", strTaxTDANo = "", strTaxTCertificateYear = "", strTaxTCertificateNo = "", strTaxTCertifcateName = "", strTaxTTDANo = "", strTaxAmountDue = "", strTaxTotalDue = "", strUnpaidType = "";
                    driver.Navigate().GoToUrl("http://fl-duval-taxcollector.publicaccessnow.com/PropertyTaxSearch.aspx");
                    driver.FindElement(By.Id("fldSearchFor")).SendKeys(outparcelno);
                    gc.CreatePdf(orderNumber, outparcelno, "Input Passed TaxSearch", driver, "FL", "Duval");
                    driver.FindElement(By.Name("btnSearch")).SendKeys(Keys.Enter);
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, outparcelno, "TaxSearch Result", driver, "FL", "Duval");
                    //*[@id="MVPQuickSearch"]/div[2]/div[1]/div[1]/ul[2]/li[1]/a
                    driver.FindElement(By.XPath("//*[@id='MVPQuickSearch']/div[2]/div[1]/div[1]/ul[2]/li[1]/a")).SendKeys(Keys.Enter);
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, outparcelno, "Tax History Details", driver, "FL", "Duval");
                    //current year tax
                    tax_type = driver.FindElement(By.XPath("//*[@id='lxT449']/table/tbody/tr[2]/td[2]")).Text.Trim();
                    mail_addr = driver.FindElement(By.XPath("//*[@id='lxT449']/table/tbody/tr[3]/td/table/tbody/tr[1]/td[1]")).Text.Trim().Replace("\r\n", ",").Replace("Mailing Address:", "");
                    tlegal_desc = driver.FindElement(By.XPath("//*[@id='lxT449']/table/tbody/tr[5]/td")).Text.Trim();
                    curr_tax_status = driver.FindElement(By.Id("dnn_ctr444_ModuleContent")).Text.Trim();

                    try
                    {
                        IWebElement IdeliquentPay = driver.FindElement(By.Id("dnn_ctr445_ModuleContent"));
                        if (IdeliquentPay.Text.Contains("No delinquent payment due for this account."))
                        {
                            curr_delinq_status = IdeliquentPay.Text.Trim();
                        }
                    }
                    catch { }
                    try
                    {
                        IWebElement IUnpaid = driver.FindElement(By.XPath("//*[@id='lxT531']"));
                        if (IUnpaid.Text.Contains("No Records Found"))
                        {
                            unpaid_tax = IUnpaid.Text.Trim();
                        }
                    }

                    catch { }
                    string currtax = tax_type + "~" + siteaddr + "~" + mail_addr + "~" + tlegal_desc + "~" + curr_tax_status + "~" + curr_delinq_status + "~" + unpaid_tax;
                    gc.insert_date(orderNumber, outparcelno, 334, currtax, 1, DateTime.Now);

                    //bill history               
                    List<string> listurl = new List<string>();
                    IWebElement billhistory = driver.FindElement(By.XPath("//*[@id='443']/table"));
                    IList<IWebElement> billhistoryRow = billhistory.FindElements(By.TagName("tr"));
                    int billrowcount = billhistoryRow.Count;
                    IList<IWebElement> billhistoryRowTD;
                    IList<IWebElement> billhistoryRowTA;
                    int b = 0;
                    foreach (IWebElement rowid in billhistoryRow)
                    {
                        billhistoryRowTD = rowid.FindElements(By.TagName("td"));
                        billhistoryRowTA = rowid.FindElements(By.TagName("a"));
                        if (billhistoryRowTD.Count != 0 && !rowid.Text.Contains("Tax Year"))
                        {
                            if (b <= 3)
                            {
                                try
                                {
                                    listurl.Add(billhistoryRowTA[0].GetAttribute("href"));
                                }
                                catch { }
                            }
                            if (b < billrowcount - 1)
                            {
                                //Tax Year~Date Paid~Folio~Paid By~Ownername~Amount Due~Amount Paid
                                string billhis = billhistoryRowTD[0].Text + "~" + "" + "~" + billhistoryRowTD[1].Text + "~" + "" + "~" + billhistoryRowTD[2].Text + "~" + billhistoryRowTD[3].Text + "~" + "";
                                gc.insert_date(orderNumber, outparcelno, 335, billhis, 1, DateTime.Now);
                            }
                            if (b == billrowcount - 1)
                            {
                                string billhis = "Total" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + billhistoryRowTD[0].Text + "~" + "";
                                gc.insert_date(orderNumber, outparcelno, 335, billhis, 1, DateTime.Now);
                            }
                        }

                        b++;
                    }

                    try
                    {
                        IWebElement IUnpaidtable = driver.FindElement(By.XPath("//*[@id='531']/table[1]/tbody[1]"));
                        IList<IWebElement> IUnpaidRow = IUnpaidtable.FindElements(By.XPath("tr"));
                        IList<IWebElement> IUnpaidTd;
                        foreach (IWebElement unpaid in IUnpaidRow)
                        {
                            IUnpaidTd = unpaid.FindElements(By.XPath("td"));
                            if (IUnpaidTd.Count != 0)
                            {
                                strTaxYear = IUnpaidTd[0].Text;
                                amc.TaxYear = IUnpaidTd[0].Text;
                                strTaxFolio = IUnpaidTd[1].Text;
                                strTaxCertificateYear = IUnpaidTd[2].Text;
                                strTaxCertificateNo = IUnpaidTd[3].Text;
                                strTaxCertifcateName = IUnpaidTd[4].Text;
                                strTaxTDANo = IUnpaidTd[5].Text;

                                string UnpaidDetails = strTaxYear + "~" + strTaxFolio + "~" + strTaxCertificateYear + "~" + strTaxCertificateNo + "~" + strTaxCertifcateName + "~" + strTaxTDANo + "~" + "-";
                                gc.insert_date(orderNumber, outparcelno, 535, UnpaidDetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }


                    foreach (string URL in listurl)
                    {
                        driver.Navigate().GoToUrl(URL);
                        Thread.Sleep(3000);
                        tax_year = driver.FindElement(By.XPath("//*[@id='lxT451']/table/tbody/tr[2]/td[3]")).Text.Trim();
                        gc.CreatePdf(orderNumber, outparcelno, "Tax History Details" + tax_year, driver, "FL", "Duval");
                        try
                        {
                            exem = driver.FindElement(By.XPath("//*[@id='lxT451']/table/tbody/tr[3]/td/table/tbody/tr[2]/td[1]")).Text.Trim().Replace("Exemptions", "").Replace("\r\n", " ");
                        }
                        catch { }

                        //add valorem Tax

                        IWebElement valoremtable = driver.FindElement(By.XPath("//*[@id='lxT500']/table/tbody"));
                        IList<IWebElement> valoremtableRow = valoremtable.FindElements(By.TagName("tr"));
                        int valoremtablerowcount = valoremtableRow.Count;
                        IList<IWebElement> valoremtablerowTD;
                        int d = 0;
                        foreach (IWebElement rowid in valoremtableRow)
                        {
                            valoremtablerowTD = rowid.FindElements(By.TagName("td"));
                            if (valoremtablerowTD.Count != 0)
                            {
                                string valoremtax = "";
                                if (d < valoremtablerowcount - 1)
                                {
                                    //Tax Year~Exemptions~Tax_Type~Taxing Code~Taxing Authority~Assessed Value~Exemption Amount~Taxable Value~Millage Rate~Taxes
                                    valoremtax = tax_year + "~" + exem + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[0].Text + "~" + valoremtablerowTD[1].Text + "~" + valoremtablerowTD[2].Text + "~" + valoremtablerowTD[3].Text + "~" + valoremtablerowTD[4].Text + "~" + valoremtablerowTD[5].Text + "~" + valoremtablerowTD[6].Text;
                                    gc.insert_date(orderNumber, outparcelno, 333, valoremtax, 1, DateTime.Now);
                                }
                                else if (d < valoremtablerowcount)
                                {
                                    valoremtax = tax_year + "~" + exem + "~" + "Ad Valorem Taxes" + "~" + "" + "~" + "Total" + "~" + "" + "~" + "" + "~" + "" + "~" + valoremtablerowTD[3].Text + "~" + valoremtablerowTD[4].Text;
                                    gc.insert_date(orderNumber, outparcelno, 333, valoremtax, 1, DateTime.Now);
                                }
                            }
                            d++;
                        }

                        //Non Ad-Valorems
                        try
                        {
                            IWebElement nonvaloremtable = driver.FindElement(By.XPath("//*[@id='lxT501']/table"));
                            IList<IWebElement> nonvaloremtableRow = nonvaloremtable.FindElements(By.TagName("tr"));
                            int nonvaloremtablerowcount = nonvaloremtableRow.Count;
                            IList<IWebElement> nonvaloremtablerowTD;
                            int e = 0;
                            foreach (IWebElement rowid in nonvaloremtableRow)
                            {
                                nonvaloremtablerowTD = rowid.FindElements(By.TagName("td"));
                                if (nonvaloremtablerowTD.Count != 0)
                                {
                                    string valoremtax = "";
                                    if (e < nonvaloremtablerowcount - 1)
                                    {
                                        //Tax Year~Exemptions~Tax_Type~Taxing Code~Taxing Authority~Assessed Value~Exemption Amount~Taxable Value~Millage Rate~Taxes
                                        valoremtax = tax_year + "~" + exem + "~" + "Non Ad Valorem Taxes" + "~" + nonvaloremtablerowTD[0].Text + "~" + nonvaloremtablerowTD[1].Text + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[2].Text;
                                        gc.insert_date(orderNumber, outparcelno, 333, valoremtax, 1, DateTime.Now);
                                    }
                                    else if (e < nonvaloremtablerowcount)
                                    {
                                        valoremtax = tax_year + "~" + exem + "~" + "Non Ad Valorem Taxes" + "~" + "Total" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[0].Text;
                                        gc.insert_date(orderNumber, outparcelno, 333, valoremtax, 1, DateTime.Now);
                                    }
                                }
                                e++;
                            }
                        }
                        catch { }
                        //If Paid By
                        try
                        {
                            IWebElement taxdettable = driver.FindElement(By.XPath("//*[@id='539']/table/tbody"));
                            IList<IWebElement> taxdettableRow = taxdettable.FindElements(By.TagName("tr"));
                            int taxdettablecount = taxdettableRow.Count;
                            IList<IWebElement> taxdettablerowTD;
                            int f = 0;
                            foreach (IWebElement rowid in taxdettableRow)
                            {
                                taxdettablerowTD = rowid.FindElements(By.TagName("td"));
                                if (taxdettablerowTD.Count != 0)
                                {
                                    string taxdue = "";
                                    if (f < rowcount - 1)
                                    {
                                        //Folio~Taxes~Fees~Interest~Discount~Paid~Due Date~Amount Due
                                        taxdue = tax_year + "~" + "" + "~" + taxdettablerowTD[0].Text + "~" + taxdettablerowTD[1].Text + "~" + taxdettablerowTD[2].Text + "~" + taxdettablerowTD[3].Text + "~" + "" + "~" + taxdettablerowTD[4].Text + "~" + taxdettablerowTD[5].Text;
                                        gc.insert_date(orderNumber, outparcelno, 341, taxdue, 1, DateTime.Now);
                                    }
                                }
                                f++;
                            }

                        }
                        catch { }
                        try
                        {
                            IWebElement priortaxdettable = driver.FindElement(By.XPath("//*[@id='lxT453']/table"));
                            IList<IWebElement> priortaxdettableRow = priortaxdettable.FindElements(By.TagName("tr"));
                            int priortaxdettablecount = priortaxdettableRow.Count;
                            IList<IWebElement> priortaxdettablerowTD;
                            int f = 0;
                            foreach (IWebElement rowid in priortaxdettableRow)
                            {
                                priortaxdettablerowTD = rowid.FindElements(By.TagName("td"));
                                if (priortaxdettablerowTD.Count != 0)
                                {
                                    string taxdue = "";
                                    if (f < rowcount - 1)
                                    {
                                        taxdue = tax_year + "~" + priortaxdettablerowTD[1].Text + "~" + priortaxdettablerowTD[2].Text + "~" + priortaxdettablerowTD[3].Text + "~" + priortaxdettablerowTD[4].Text + "~" + priortaxdettablerowTD[5].Text + "~" + priortaxdettablerowTD[6].Text + "~" + priortaxdettablerowTD[7].Text + "~" + priortaxdettablerowTD[8].Text;
                                        gc.insert_date(orderNumber, outparcelno, 341, taxdue, 1, DateTime.Now);
                                        if (amccount < 1)
                                        {
                                            amc.Instamount1 = priortaxdettablerowTD[2].Text;
                                            amc.Instamountpaid1 = priortaxdettablerowTD[6].Text;
                                            if (!priortaxdettablerowTD[6].Text.Contains("$0.00"))
                                            {
                                                amc.InstPaidDue1 = "Paid";
                                            }
                                            if (priortaxdettablerowTD[3].Text.Contains("$0.00") && priortaxdettablerowTD[4].Text.Contains("$0.00") && priortaxdettablerowTD[6].Text.Contains("$0.00"))
                                            {
                                                amc.InstPaidDue1 = "Due";
                                            }
                                            if((priortaxdettablerowTD[3].Text.Contains("$0.00") || priortaxdettablerowTD[4].Text.Contains("$0.00"))  && !priortaxdettablerowTD[8].Text.Contains("$0.00"))
                                            {
                                                amc.IsDelinquent = "Yes";
                                            }
                                            if((priortaxdettablerowTD[3].Text.Contains("$0.00") && priortaxdettablerowTD[4].Text.Contains("$0.00")) && priortaxdettablerowTD[8].Text.Contains("$0.00"))
                                            {
                                                amc.IsDelinquent = "No";
                                            }
                                            if (amc.IsDelinquent == "Yes")
                                            {
                                                gc.InsertAmrockTax(orderNumber, amc.TaxId, null, null, null, null, null, null, null, null, null, null, null, null, amc.IsDelinquent);
                                            }

                                            if (amc.IsDelinquent == "No")
                                            {
                                                gc.InsertAmrockTax(orderNumber, amc.TaxId, amc.Instamount1, amc.Instamount2, amc.Instamount3, amc.Instamount4, amc.Instamountpaid1, amc.Instamountpaid2, amc.Instamountpaid3, amc.Instamountpaid4, amc.InstPaidDue1, amc.InstPaidDue2, amc.instPaidDue3, amc.instPaidDue4, amc.IsDelinquent);
                                            }
                                            amccount++;
                                        }
                                    }
                                }
                                f++;
                            }
                        }
                        catch
                        {

                        }
                        //payment history
                        try
                        {
                            try
                            {
                                histaxdettable = driver.FindElement(By.XPath("//*[@id='lxT454']/table/tbody"));
                            }

                            catch { }
                            try
                            {
                                if (histaxdettable == null)
                                {
                                    histaxdettable = driver.FindElement(By.XPath("//*[@id='539']/table[2]/tbody"));
                                }
                            }
                            catch { }
                            IList<IWebElement> histaxdettableRow = histaxdettable.FindElements(By.TagName("tr"));
                            int histaxdettablecount = histaxdettableRow.Count;
                            IList<IWebElement> histaxdettablerowTD;
                            int f = 0;
                            foreach (IWebElement rowid in histaxdettableRow)
                            {
                                histaxdettablerowTD = rowid.FindElements(By.TagName("td"));
                                if (histaxdettablerowTD.Count != 0)
                                {
                                    string taxhis = "";
                                    if (f < rowcount - 1)
                                    {
                                        //Tax Year~Date Paid~Folio~Paid By~Ownername~Amount Due~Amount Paid
                                        taxhis = histaxdettablerowTD[1].Text + "~" + histaxdettablerowTD[0].Text + "~" + histaxdettablerowTD[2].Text + "~" + histaxdettablerowTD[3].Text + "~" + "" + "~" + "" + "~" + histaxdettablerowTD[4].Text;
                                        gc.insert_date(orderNumber, outparcelno, 335, taxhis, 1, DateTime.Now);
                                    }
                                }
                                f++;
                            }

                        }
                        catch { }

                        //UnPaid if Avilable
                        try
                        {
                            strUnpaidType = driver.FindElement(By.Id("dnn_ctr504_dnnTITLE_lblTitle")).Text;
                        }
                        catch { }
                        try
                        {
                            IWebElement Iunpaid = driver.FindElement(By.XPath("//*[@id='504']/table/tbody"));
                            IList<IWebElement> IunpaidRow = Iunpaid.FindElements(By.XPath("tr"));
                            IList<IWebElement> IunpaidTd;
                            foreach (IWebElement unpaid in IunpaidRow)
                            {
                                IunpaidTd = unpaid.FindElements(By.XPath("td"));
                                if (IunpaidTd.Count != 0)
                                {
                                    strTaxTCertificateYear = IunpaidTd[0].Text;
                                    strTaxTCertificateNo = IunpaidTd[1].Text;
                                    strTaxTCertifcateName = IunpaidTd[2].Text;
                                    strTaxTTDANo = IunpaidTd[3].Text;
                                    strTaxAmountDue = IunpaidTd[4].Text;

                                    string UnpaidTotalDetails = "-" + "~" + "-" + "~" + strTaxTCertificateYear + "~" + strTaxTCertificateNo + "~" + strTaxTCertifcateName + "~" + strTaxTTDANo + "~" + strTaxAmountDue;
                                    gc.insert_date(orderNumber, outparcelno, 535, UnpaidTotalDetails, 1, DateTime.Now);
                                }
                            }
                        }
                        catch
                        {

                        }
                        try
                        {
                            IWebElement IunTotalpaid = driver.FindElement(By.XPath("//*[@id='504']/table/tfoot"));
                            IList<IWebElement> IunpaidTotalRow = IunTotalpaid.FindElements(By.XPath("tr"));
                            IList<IWebElement> IunpaidTotalTd;
                            foreach (IWebElement unpaid in IunpaidTotalRow)
                            {
                                IunpaidTotalTd = unpaid.FindElements(By.XPath("td"));
                                if (IunpaidTotalTd.Count != 0)
                                {
                                    strTaxTotalDue = IunpaidTotalTd[0].Text;

                                    string UnpaidTotalDetails = "-" + "~" + "Total" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + strTaxTotalDue;
                                    gc.insert_date(orderNumber, outparcelno, 535, UnpaidTotalDetails, 1, DateTime.Now);
                                }
                            }
                        }
                        catch
                        {

                        }

                    }
                  
                    //gc.InsertAmrockTax(orderNumber, amc.TaxId, amc.Instamount1, amc.Instamount2, amc.Instamount3, amc.Instamount4, amc.Instamountpaid1, amc.Instamountpaid2, amc.Instamountpaid3, amc.Instamountpaid4, amc.InstPaidDue1, amc.InstPaidDue2, amc.instPaidDue3, amc.instPaidDue4, amc.IsDelinquent);

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Quit();
                    gc.mergpdf(orderNumber, "FL", "Duval");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "FL", "Duval", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
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
        private string multiparcel(string ordernumber)
        {
            string reccount = driver.FindElement(By.Id("ctl00_cphBody_lblResultsCount")).Text.Trim().Replace(",", "");
            reccount = reccount.Replace(" properties found", "");
            int count = Int32.Parse(reccount);
            if (count > 25)
            {
                HttpContext.Current.Session["multiparcel_duval_count"] = "Maximum";
                return "Maximum";
            }
            if (count > 1)
            {
                HttpContext.Current.Session["multiparcel_duval"] = "Yes";
                //Select address from list....
                IWebElement multiaddrtableElement = driver.FindElement(By.Id("ctl00_cphBody_gridResults"));
                IList<IWebElement> multiaddrtableRow = multiaddrtableElement.FindElements(By.TagName("tr"));
                IList<IWebElement> multiaddrrowTD;
                int j = 0;
                foreach (IWebElement row in multiaddrtableRow)
                {
                    multiaddrrowTD = row.FindElements(By.TagName("td"));
                    if (multiaddrrowTD.Count != 0)
                    {
                        if (j < 25)
                        {
                            try
                            {
                                string multiowner = multiaddrrowTD[1].Text + "~" + multiaddrrowTD[2].Text + "~" + multiaddrrowTD[3].Text + "~" + multiaddrrowTD[4].Text + "~" + multiaddrrowTD[5].Text + "~" + multiaddrrowTD[6].Text + "~" + multiaddrrowTD[7].Text + "~" + multiaddrrowTD[8].Text;
                                gc.insert_date(ordernumber, multiaddrrowTD[0].Text.Trim(), 229, multiowner, 1, DateTime.Now);
                            }
                            catch
                            {
                            }
                            j++;
                        }

                    }
                }

            }

            driver.Quit();
            return "MultiParcel";
        }
    }
}