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
    public class WebDriver_FLSarasota
    {

        string Outparcelno = "", outputPath = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_FLSarasota(string address, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            //GlobalClass.global_county = county;
            string outparcelno = "", siteaddr = "", mail_addr = "", owner1 = "", legal_desc = "", year_built = "", tax_authority = "", propuse = "", T_legal_desc, exempt_amount = "", total_Asstax = "", total_milage = "";
            string valued_year = "", tax_year = "", assess_land = "", ass_improve = "", ass_total = "", tax_value = "", exem = "", pathid = "", Total_taxes = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            using (driver = new PhantomJSDriver())
            {

                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "FL", "Sarasota");

                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_FLSarasota"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    driver.Navigate().GoToUrl("http://www.sc-pa.com/propertysearch");

                    if (searchType == "address")
                    {
                        driver.FindElement(By.Id("AddressKeywords")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "Input Passed Address Search", driver, "FL", "Sarasota");
                        driver.FindElement(By.Id("search")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        // gc.CreatePdf_WOP(orderNumber, "Address Search Results", driver, "FL", "Sarasota");
                        try
                        {
                            string multi = driver.FindElement(By.XPath("//*[@id='container']/form/div/span[1]")).Text;
                            if(!multi.Contains("0 matching") && !multi.Contains(""))
                            {
                                multiparcel(orderNumber);
                                return "MulitParcel";
                            }
                        }
                        catch
                        {

                        }
                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.Id("Strap")).SendKeys(parcelNumber);
                        // gc.CreatePdf(orderNumber, parcelNumber, "Assessment Details", driver, "FL", "Sarasota");
                        driver.FindElement(By.Id("search")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                    }
                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("OwnerKeywords")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "Input Passed Ownername search", driver, "FL", "Sarasota");
                        driver.FindElement(By.Id("search")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        // gc.CreatePdf_WOP(orderNumber, "Ownername Search Result", driver, "FL", "Sarasota");
                        try
                        {
                            string multi = driver.FindElement(By.XPath("//*[@id='container']/form/div/span[1]")).Text;
                            if (!multi.Contains("0 matching") && !multi.Contains(""))
                            {
                                multiparcel(orderNumber);
                                return "MulitParcel";
                            }
                        }
                        catch
                        {

                        }
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.XPath("//*[@id='container']/form/div"));
                        if (INodata.Text.Contains("0 matching"))
                        {
                            HttpContext.Current.Session["Nodata_FLSarasota"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    //property details
                    outparcelno = driver.FindElement(By.XPath("//*[@id='targetElemForTooltips']/div[2]/span")).Text;
                    outparcelno = outparcelno.Replace("Property Record Information for ", "");
                    gc.CreatePdf(orderNumber, outparcelno, "Assessment Details", driver, "FL", "Sarasota");
                    string nameaddress = driver.FindElement(By.XPath("//*[@id='container']/ul[1]")).Text;
                    // ownername = Regex.Match(nameaddress, @"^[^0-9]*").Value.Replace("Ownership:", "").Trim().Replace("\r\n",",");
                    string mail_addressname = GlobalClass.Before(nameaddress, "Incorrect mailing address?");
                    mail_addr = mail_addressname.Replace("Ownership:", "").Trim().Replace("\r\n", ",").Trim();
                    //mail_addr = driver.FindElement(By.XPath("//*[@id='container']/ul[1]/li[4]")).Text.Trim();
                    siteaddr = WebDriverTest.After(nameaddress, "Situs Address:").Trim();
                    propuse = driver.FindElement(By.XPath("//*[@id='container']/ul[2]/li[4]")).Text.Trim().Replace("Property Use:", "").Replace("\r\n", "");
                    legal_desc = driver.FindElement(By.XPath("//*[@id='container']/ul[2]/li[10]")).Text.Trim().Replace("Parcel Description:", "").Replace("\r\n", "");
                    try
                    {
                        year_built = driver.FindElement(By.XPath("//*[@id='Buildings']/tbody/tr/td[6]")).Text.Trim();
                    }
                    catch { }
                    string property = mail_addr + "~" + siteaddr + "~" + propuse + "~" + year_built + "~" + legal_desc;
                    gc.insert_date(orderNumber, outparcelno, 249, property, 1, DateTime.Now);
                    //assessment details 

                    IWebElement asstable = driver.FindElement(By.XPath("//*[@id='container']/div[2]/table/tbody"));
                    IList<IWebElement> asstableRow = asstable.FindElements(By.TagName("tr"));
                    int rowcount = asstableRow.Count;
                    IList<IWebElement> asstablerowTD;
                    int w = 0;
                    foreach (IWebElement rowid in asstableRow)
                    {
                        asstablerowTD = rowid.FindElements(By.TagName("td"));
                        if (asstablerowTD.Count != 0 && !rowid.Text.Contains("Year"))
                        {
                            if (w < 3)
                            {
                                string ass = asstablerowTD[0].Text + "~" + asstablerowTD[1].Text + "~" + asstablerowTD[2].Text + "~" + asstablerowTD[3].Text + "~" + asstablerowTD[4].Text + "~" + asstablerowTD[5].Text + "~" + asstablerowTD[6].Text + "~" + asstablerowTD[7].Text + "~" + asstablerowTD[8].Text;
                                gc.insert_date(orderNumber, outparcelno, 250, ass, 1, DateTime.Now);
                            }
                        }
                        w++;
                    }


                    //tax details
                    string exempt = "", milage_code = "", oldacc_no = "", tax_type = "", tax_years = "", date_paid = "", Transaction = "", Receipt = "", Item = "", Amount_Paid = "";
                    driver.Navigate().GoToUrl("http://sarasotataxcollector.governmax.com/collectmax/collect30.asp");
                    IWebElement iframeElement = driver.FindElement(By.XPath("/html/frameset/frame"));
                    driver.SwitchTo().Frame(iframeElement);
                    IWebElement img = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr[1]/td/table[2]/tbody/tr/td[1]/center/a"));
                    img.SendKeys(Keys.Enter);
                    Thread.Sleep(5000);
                    //driver.Navigate().GoToUrl("http://sarasotataxcollector.governmax.com/collectmax/search_collect.asp?wait=done&l_nm=account&form=searchform&formelement=0&sid=B8BBCD1639464B26A8AA14F5E549396C");
                    driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[3]/td/font/a")).SendKeys(Keys.Enter);
                    Thread.Sleep(5000);
                    driver.FindElement(By.Name("account")).SendKeys(outparcelno);
                    gc.CreatePdf(orderNumber, outparcelno, "TaxPage Input Passed", driver, "FL", "Sarasota");
                    driver.FindElement(By.Name("go")).SendKeys(Keys.Enter);
                    Thread.Sleep(10000);
                    gc.CreatePdf(orderNumber, outparcelno, "Tax Deails", driver, "FL", "Sarasota");
                    try
                    {
                        exempt = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[6]/td/table/tbody/tr/td/table/tbody/tr[2]/td[1]/font")).Text.Trim();
                    }
                    catch { }

                    try
                    {

                        IWebElement exemtable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[6]/td/table/tbody/tr/td/table/tbody/tr[2]/td[1]/table/tbody"));
                        IList<IWebElement> exemtableRow = exemtable.FindElements(By.TagName("tr"));
                        int exemtablecount = exemtableRow.Count;
                        IList<IWebElement> exemtablerowTD;
                        int a = 0;
                        foreach (IWebElement rowid in exemtableRow)
                        {
                            exemtablerowTD = rowid.FindElements(By.TagName("td"));
                            if (exemtablerowTD.Count != 0 && !rowid.Text.Contains("Taxing Authority"))
                            {

                                //Authority~Code~Tax Type~Millage~Assessed~Exemption~Taxable~Tax
                                if (a == 0)
                                {
                                    exempt = exemtablerowTD[0].Text;
                                    exempt_amount = exemtablerowTD[1].Text;
                                }
                                else if (a == 1)
                                {
                                    exempt += ",";

                                    exempt += exemtablerowTD[0].Text;
                                    exempt_amount += ",";
                                    exempt_amount += exemtablerowTD[1].Text;
                                }
                                a++;
                            }
                        }
                    }
                    catch { }
                    try
                    {
                        milage_code = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[6]/td/table/tbody/tr/td/table/tbody/tr[2]/td[2]/font")).Text.Trim();
                        oldacc_no = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[3]/td/table/tbody/tr[1]/td[2]/font[6]")).Text.Trim();
                        tax_type = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[2]/td[2]/font/b")).Text.Trim();
                        tax_years = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[2]/td[3]/font/b")).Text.Trim();
                    }
                    catch
                    {

                    }
                    try
                    {

                        IWebElement valoremtable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[8]/td/table[1]/tbody"));
                        IList<IWebElement> valoremtableRow = valoremtable.FindElements(By.TagName("tr"));
                        int valoremtablerowcount = valoremtableRow.Count;
                        IList<IWebElement> valoremtablerowTD;
                        foreach (IWebElement rowid in valoremtableRow)
                        {
                            valoremtablerowTD = rowid.FindElements(By.TagName("td"));
                            if (valoremtablerowTD.Count != 0 && !rowid.Text.Contains("Taxing Authority"))
                            {
                                string valoremtax = "";
                                //Authority~Code~Tax Type~Millage~Assessed~Exemption~Taxable~Tax
                                try
                                {
                                    if (valoremtablerowTD.Count == 6)
                                    {
                                        valoremtax = valoremtablerowTD[0].Text + "~" + "" + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[1].Text + "~" + valoremtablerowTD[2].Text + "~" + valoremtablerowTD[3].Text + "~" + valoremtablerowTD[4].Text + "~" + valoremtablerowTD[5].Text;
                                        gc.insert_date(orderNumber, outparcelno, 251, valoremtax, 1, DateTime.Now);
                                    }
                                    else if (valoremtablerowTD.Count == 2)
                                    {
                                        valoremtax = valoremtablerowTD[0].Text + "~" + "" + "~" + "Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                        gc.insert_date(orderNumber, outparcelno, 251, valoremtax, 1, DateTime.Now);
                                    }

                                }
                                catch { }

                            }
                        }

                        total_milage = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[8]/td/table[2]/tbody/tr/td[2]/font")).Text.Trim();
                        total_Asstax = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[8]/td/table[2]/tbody/tr/td[4]/font")).Text.Trim();
                        string valoremtotal = "Total" + "~" + "" + "~" + "Ad Valorem Taxes" + "~" + total_milage + "~" + " " + "~" + " " + "~" + " " + "~" + total_Asstax;
                        gc.insert_date(orderNumber, outparcelno, 251, valoremtotal, 1, DateTime.Now);

                    }
                    catch { }
                    try
                    {
                        IWebElement nonvaloremtable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[10]/td/table[1]/tbody"));
                        IList<IWebElement> nonvaloremtableRow = nonvaloremtable.FindElements(By.TagName("tr"));
                        int nonvaloremtablerowcount = nonvaloremtableRow.Count;
                        IList<IWebElement> nonvaloremtablerowTD;
                        foreach (IWebElement rowid in nonvaloremtableRow)
                        {
                            nonvaloremtablerowTD = rowid.FindElements(By.TagName("td"));
                            if (nonvaloremtablerowTD.Count != 0 && !rowid.Text.Contains("Levying Authority"))
                            {
                                string nonvaloremtax = "";
                                //Authority~Code~Tax Type~Millage~Assessed~Exemption~Taxable~Tax
                                try
                                {
                                    nonvaloremtax = nonvaloremtablerowTD[1].Text + "~" + nonvaloremtablerowTD[0].Text + "~" + "Non-Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[2].Text;
                                    gc.insert_date(orderNumber, outparcelno, 251, nonvaloremtax, 1, DateTime.Now);

                                }
                                catch { }
                            }
                        }


                        string total_ass = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[10]/td/table[2]/tbody/tr/td[2]/font")).Text.Trim();
                        Total_taxes = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[11]/td/table/tbody/tr/td[4]/font")).Text.Trim();
                        string valoremtotalass = "Total" + "~" + "" + "~" + "Total Assessments" + "~" + "" + "~" + " " + "~" + " " + "~" + " " + "~" + total_ass;
                        gc.insert_date(orderNumber, outparcelno, 251, valoremtotalass, 1, DateTime.Now);
                        string valoremtotaltax = "Total" + "~" + "" + "~" + "Taxes & Assessments" + "~" + "" + "~" + " " + "~" + " " + "~" + " " + "~" + Total_taxes;
                        gc.insert_date(orderNumber, outparcelno, 251, valoremtotaltax, 1, DateTime.Now);
                        AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    }
                    catch
                    {

                    }
                    //taxdue
                    try
                    {
                        IWebElement taxduetable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[4]/tbody/tr/td/table/tbody"));
                        IList<IWebElement> taxduetableRow = taxduetable.FindElements(By.TagName("tr"));
                        int taxduerowcount = taxduetableRow.Count;
                        IList<IWebElement> taxduetablerowTD;
                        foreach (IWebElement rowid in taxduetableRow)
                        {
                            taxduetablerowTD = rowid.FindElements(By.TagName("td"));
                            if (taxduetablerowTD.Count != 0 && !rowid.Text.Contains("If Paid By"))
                            {
                                string curtax = "";
                                //Authority~Code~Tax Type~Millage~Assessed~Exemption~Taxable~Tax
                                try
                                {
                                    curtax = taxduetablerowTD[0].Text + "~" + "Current Tax" + "~" + taxduetablerowTD[1].Text;
                                    gc.insert_date(orderNumber, outparcelno, 252, curtax, 1, DateTime.Now);

                                }
                                catch { }
                            }
                        }
                    }
                    catch { }

                    //curtaxhistory
                    string curtaxhis = "";
                    try
                    {

                        IWebElement taxpaidtable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[5]"));
                        IList<IWebElement> taxpaidtableRow = taxpaidtable.FindElements(By.TagName("tr"));
                        int taxpaidrowcount = taxpaidtableRow.Count;
                        IList<IWebElement> taxduetablerowTD;
                        foreach (IWebElement rowid in taxpaidtableRow)
                        {
                            taxduetablerowTD = rowid.FindElements(By.TagName("td"));
                            if (taxduetablerowTD.Count != 0 && !rowid.Text.Contains("Date Paid"))
                            {

                                try
                                {
                                    //Old Account Number~Exemption Type~Exemption Amount~Millage Code~Millage rate~Tax Year~Tax Amount~Paid Amount~Paid Date~Effective Date~Receipt Number~Taxing Authority
                                    date_paid = taxduetablerowTD[0].Text; Transaction = taxduetablerowTD[1].Text; Receipt = taxduetablerowTD[2].Text;
                                    Item = taxduetablerowTD[3].Text; Amount_Paid = taxduetablerowTD[4].Text;
                                    string currenttax = "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + Item + "~" + "" + "~" + "" + "~" + Amount_Paid + "~" + date_paid + "~" + "" + "~" + Receipt + "~" + "" + "~" + Transaction;
                                    gc.insert_date(orderNumber, outparcelno, 258, currenttax, 1, DateTime.Now);

                                }
                                catch { }
                            }
                        }
                    }
                    catch
                    {
                    }

                    //prior year tax 
                    try
                    {
                        IWebElement priortaxtable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[6]/tbody/tr[2]/td/table[1]/tbody"));
                        IList<IWebElement> priortaxtableRow = priortaxtable.FindElements(By.TagName("tr"));
                        int priorrowcount = priortaxtableRow.Count;
                        IList<IWebElement> priortaxtablerowTD;
                        foreach (IWebElement rowid in priortaxtableRow)
                        {
                            priortaxtablerowTD = rowid.FindElements(By.TagName("td"));
                            if (priortaxtablerowTD.Count != 0 && !rowid.Text.Contains("Year"))
                            {
                                string curtax = "";
                                try
                                {
                                    curtax = priortaxtablerowTD[0].Text + "~" + priortaxtablerowTD[1].Text + "~" + priortaxtablerowTD[2].Text + "~" + priortaxtablerowTD[3].Text + "~" + priortaxtablerowTD[4].Text + "~" + priortaxtablerowTD[5].Text;
                                    gc.insert_date(orderNumber, outparcelno, 253, curtax, 1, DateTime.Now);

                                }
                                catch { }
                            }
                        }
                    }
                    catch { }

                    try
                    {

                        string prior_total = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[6]/tbody/tr[2]/td/table[3]/tbody/tr/td[2]")).Text.Trim();
                        string priortotlinsert = "Prior Years Total " + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + prior_total;
                        gc.insert_date(orderNumber, outparcelno, 253, priortotlinsert, 1, DateTime.Now);


                        IWebElement priortaxdettable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[7]/tbody"));
                        IList<IWebElement> priortaxdettableRow = priortaxdettable.FindElements(By.TagName("tr"));
                        int priortaxdettablecount = priortaxdettableRow.Count;
                        IList<IWebElement> priortaxdettablerowTD;
                        foreach (IWebElement rowid in priortaxdettableRow)
                        {
                            priortaxdettablerowTD = rowid.FindElements(By.TagName("td"));
                            if (priortaxdettablerowTD.Count != 0 && !rowid.Text.Contains("If Paid By"))
                            {
                                string curtax = "";
                                try
                                {

                                    curtax = priortaxdettablerowTD[0].Text + "~" + "Delinquent" + "~" + priortaxdettablerowTD[1].Text;
                                    gc.insert_date(orderNumber, outparcelno, 252, curtax, 1, DateTime.Now);

                                }
                                catch { }
                            }
                        }
                    }
                    catch
                    {

                    }
                    try
                    {
                        //Old Account Number~Exemption Type~Exemption Amount~Millage Code~Millage rate~Tax Year~Tax Amount~Paid Amount~Paid Date~Effective Date~Receipt Number~Taxing Authority
                        try
                        {

                            driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[7]/td/font/a")).SendKeys(Keys.Enter);
                        }
                        catch { }
                        try
                        {
                            driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[6]/td/font/a")).SendKeys(Keys.Enter);
                        }
                        catch { }
                        Thread.Sleep(10000);
                        gc.CreatePdf(orderNumber, outparcelno, "Tax History Deails", driver, "FL", "Sarasota");
                        //html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table[3]

                        IWebElement tablelist = driver.FindElement(By.XPath("html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td"));
                        IList<IWebElement> tablelists = tablelist.FindElements(By.TagName("table"));
                        int tablecount = tablelists.Count;
                        int k = 3;
                        for (k = 3; k < tablecount - 1; k++)
                        {
                            IWebElement taxhistorytable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table[" + k + "]/tbody"));
                            IList<IWebElement> taxhistorytableRow = taxhistorytable.FindElements(By.TagName("tr"));
                            int taxhistorytablecount = taxhistorytableRow.Count;
                            IList<IWebElement> taxhistorytablerowTD;
                            foreach (IWebElement rowid1 in taxhistorytableRow)
                            {
                                taxhistorytablerowTD = rowid1.FindElements(By.TagName("td"));
                                if (taxhistorytablerowTD.Count != 0 && !rowid1.Text.Contains("Payment History") && !rowid1.Text.Contains("Year"))
                                {
                                    try
                                    {
                                        string taxhis = "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + taxhistorytablerowTD[0].Text + "~" + taxhistorytablerowTD[1].Text + "~" + taxhistorytablerowTD[4].Text + "~" + taxhistorytablerowTD[5].Text + "~" + taxhistorytablerowTD[2].Text + "~" + "" + "~" + taxhistorytablerowTD[3].Text + "~" + "" + "~" + "";
                                        gc.insert_date(orderNumber, outparcelno, 258, taxhis, 1, DateTime.Now);
                                    }
                                    catch { }

                                }
                            }
                        }
                    }
                    catch { }

                    try
                    {

                        IWebElement Idownloadurl = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[8]/td/font/a"));
                        string Url = Idownloadurl.GetAttribute("href");
                        driver.Navigate().GoToUrl(Url);
                        Thread.Sleep(10000);
                        gc.CreatePdf(orderNumber, outparcelno, "Tax Bill", driver, "FL", "Sarasota");
                        tax_authority = driver.FindElement(By.XPath("//*[@id='form0']/table[2]/tbody/tr[4]/td/table[2]/tbody/tr/td/font/div")).Text.Trim().Replace("\r\n", "").Replace("Mailing address:", "");
                    }
                    catch
                    {

                    }
                    curtaxhis = oldacc_no + "~" + exempt + "~" + exempt_amount + "~" + milage_code + "~" + total_milage + "~" + tax_years + "~" + "" + "~" + Total_taxes + "~" + Amount_Paid + "~" + date_paid + "~" + "" + "~" + Receipt + "~" + tax_authority;
                    gc.insert_date(orderNumber, outparcelno, 258, curtaxhis, 1, DateTime.Now);

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Quit();
                    gc.mergpdf(orderNumber, "FL", "Sarasota");
                    gc.insert_TakenTime(orderNumber, "FL", "Sarasota", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
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

            string multi = driver.FindElement(By.XPath("//*[@id='container']/form/div/span[1]")).Text;
            string count = multi.Replace(" matching parcels (25 results per page)", "").Trim();
            int multicount = Int32.Parse(count);
            gc.CreatePdf_WOP(ordernumber, "Multiparcel Results", driver, "FL", "Sarasota");
            if (multicount > 1 && multicount <= 10)
            {
                HttpContext.Current.Session["multiparcel_FLSarasota"] = "Yes";
                int k = 3;
                int ccount = multicount + 3;
                for (k = 3; k < ccount; k++)
                {
                    string mparcel = driver.FindElement(By.XPath("//*[@id='container']/form/div/div[" + k + "]/div/div[2]/a/span")).Text.Trim();
                    string maddress = driver.FindElement(By.XPath("//*[@id='container']/form/div/div[" + k + "]/div/div[2]/span/a")).Text.Trim();
                    string fullname = driver.FindElement(By.XPath("//*[@id='container']/form/div/div[" + k + "]/div/div[3]")).Text;
                    string name = gc.Between(fullname, "Ownership", "Most recent transfer").Replace("\r\n", ",");
                    string multiowner = maddress + "~" + name;
                    gc.insert_date(ordernumber, mparcel, 82, multiowner, 1, DateTime.Now);
                }
            }

            else
            {
                HttpContext.Current.Session["multiparcel_FLSarasota"] = "Maximum";
                return "Maximum";
            }
            driver.Quit();
            return "MultiParcel";
        }
    }
}

