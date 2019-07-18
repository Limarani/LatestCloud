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
    public class WebDriver_NMBernalillio
    {
        IWebDriver driver;
        IWebElement LinkSearch;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        int Address;
        string srtAddress = "-", outputPath = "";
        string strClassname = "-", strTaxdistrcit = "-", strTaxyear = "-", strOwnername = "-", strLocationaddrss = "-", strCity = "-",
       strProprtydescription = "-", strRealproperty = "-", strValues = "-", strTablevalues = "-", strException = "-", strNettaxablevalue = "-", strYearBuilt = "-", strLotSize = "-", strLandUseCode = "-", strStyle = "-",
       strFullLandValue = "-", strFullImpValue = "-", strTotalValue = "-", strTaxable = "-", strVetaranan = "-", strOther = "-";
        string Owner1 = "-", Location_Address = "-", LegalDescription = "-", taxYear = "-", netTaxable = "-", tax = "-", Interest = "-", Penalty = "-", Fees = "-", Paid = "-", AmountDue = "-",
               FirstHalf = "-", FirstHalftax = "-", FirstHalfInterest = "-", FirstHalfPenalty = "-",
               FirstHalfFees = "-", FirstHalfPaid = "-", FirstHalfAmountDue = "-", SecondHalf = "-", SecondHalftax = "-", SecondHalfInterest = "-", SecondHalfPenalty = "-",
               SecondHalfFees = "-", SecondHalfPaid = "-", SecondHalfAmountDue = "-", TotalHalf = "-", TotalHalftax = "-", TotalHalfInterest = "-", TotalHalfPenalty = "-",
               TotalHalfFees = "-", TotalHalfPaid = "-", TotalHalfAmountDue = "-", Tax_Authority = "-";
        string strRowData = "", strTaxRate = "-", strNetTaxable = "-", strAmountDue = "-", Agencies = "-", TaxRate = "-", NetTaxableValue = "-", strTotalRate = "-";
        string outparcelno = "";
        string OtherTaxDueYear = "_", OtherTaxDueTax = "-", OtherTaxDueInterest = "-", OtherTaxDuePanalty = "-", OtherTaxDueFees = "-", OtherTaxDueAmountDue = "-";
        public string FTP_NMBernalillo(string houseno, string sname, string sttype, string parcelNumber, string searchType, string orderNumber, string ownername, string dierctSearch)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new ChromeDriver();
            //driver = new PhantomJSDriver();
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    if (searchType == "titleflex")
                    {
                        string address = houseno + " " + sname;
                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "NM", "Bernalillo");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_NMBernalillio"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        string result = "";
                        // http://assessor.bernco.gov/public.access/Search/Disclaimer.aspx?FromUrl=../search/commonsearch.aspx?mode=realprop
                        driver.Navigate().GoToUrl("http://assessor.bernco.gov/public.access/Search/Disclaimer.aspx?FromUrl=../search/commonsearch.aspx?mode=realprop");
                        try
                        {
                            driver.FindElement(By.Id("btAgree")).Click();
                            Thread.Sleep(2000);
                        }
                        catch
                        { }
                        try
                        {
                            driver.FindElement(By.Id("btAgree")).SendKeys(Keys.Enter);
                        }
                        catch { }
                        driver.FindElement(By.Id("inpNo")).SendKeys(houseno);
                        driver.FindElement(By.Id("inpStreet")).SendKeys(sname);
                        gc.CreatePdf_WOP(orderNumber, "InputPassed AddressSearch", driver, "NM", "Bernalillo");
                        driver.FindElement(By.Id("inpStreet")).SendKeys(Keys.Enter);
                        Thread.Sleep(6000);
                        try
                        {
                            result = driver.FindElement(By.XPath("//*[@id='ml']")).Text;
                        }

                        catch
                        {

                        }

                        try
                        {
                            srtAddress = driver.FindElement(By.XPath("//*[@id='frmMain']/table/tbody/tr/td/div/div/table[2]/tbody/tr/td/table/tbody/tr[3]/td/center/table[1]/tbody/tr/td[3]")).Text;
                            Address = Convert.ToInt32(WebDriverTest.Between(srtAddress, "Displaying 1 - ", " of"));


                            if (Address > 15)
                            {
                                HttpContext.Current.Session["multiParcel_NMBernalillo_count"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                            if (srtAddress != "" && Address <= 15 && !result.Contains("Return to Search Results"))
                            {
                                IWebElement IAddressTable = driver.FindElement(By.XPath("//*[@id='searchResults']/tbody"));
                                IList<IWebElement> IAddressrow = IAddressTable.FindElements(By.TagName("tr"));
                                IList<IWebElement> IAddressTD;
                                foreach (IWebElement Row in IAddressrow)
                                {
                                    IAddressTD = Row.FindElements(By.TagName("td"));
                                    HttpContext.Current.Session["multiparcel_NMBernalillo"] = "Yes";
                                    if (IAddressTD.Count != 0 && !Row.Text.Contains("Parcel ID") && Row.Text != "")
                                    {
                                        string ParcelID = IAddressTD[0].Text.Replace("...", "").Trim();
                                        string Owner = IAddressTD[1].Text;
                                        string Addres = IAddressTD[2].Text;
                                        string Property = Owner + "~" + Addres;
                                        gc.insert_date(orderNumber, ParcelID, 98, Property, 1, DateTime.Now);
                                        gc.CreatePdf_WOP(orderNumber, "Mulitparcelgrid", driver, "NM", "Bernalillo");
                                    }
                                }
                                driver.Quit();
                                return "MultiParcel";
                            }
                            else
                            {

                            }
                        }
                        catch { }
                    }                    

                    if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("http://assessor.bernco.gov/public.access/search/commonsearch.aspx?mode=parid");
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='topmenu']/ul/li[1]/a")).Click();
                            Thread.Sleep(2000);
                        }
                        catch
                        { }
                        driver.FindElement(By.XPath("//*[@id='btAgree']")).SendKeys(Keys.Enter);
                        driver.FindElement(By.Id("inpSuf")).SendKeys(parcelNumber.Replace("...","").Trim());
                        gc.CreatePdf(orderNumber, parcelNumber, "InputPassed ParcelSearch", driver, "NM", "Bernalillo");
                        driver.FindElement(By.XPath("//*[@id='btSearch']")).SendKeys(Keys.Enter);
                        Thread.Sleep(6000);
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.XPath("//*[@id='frmMain']/table/tbody/tr/td/div/div/table[2]"));
                        if(INodata.Text.Contains("search did not find any records"))
                        {
                            HttpContext.Current.Session["Nodata_NMBernalillio"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    //Property Details  //*[@id="searchResults"]/tbody/tr[3]
                    //driver.FindElement(By.XPath("")).SendKeys(Keys.Enter);
                    try
                    {
                        IWebElement IAddressSearch1 = driver.FindElement(By.XPath("//*[@id='searchResults']/tbody/tr[3]"));
                        IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                        js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                        Thread.Sleep(3000);
                    }
                    catch { }
                    //Screenshot
                    gc.CreatePdf_WOP(orderNumber, "PropertyDetails", driver, "NM", "Bernalillo");
                    //Profile
                    outparcelno = driver.FindElement(By.XPath("//*[@id='datalet_header_row']/td/table/tbody/tr[1]/td[1]")).Text;
                    outparcelno = WebDriverTest.After(outparcelno, ": ");
                    strClassname = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[1]/table[2]/tbody/tr[1]/td[2]")).Text;
                    strTaxdistrcit = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[1]/table[2]/tbody/tr[2]/td[2]")).Text;
                    strTaxyear = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[2]/table[2]/tbody/tr[1]/td[2]")).Text;
                    strOwnername = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[2]/table[2]/tbody/tr[2]/td[2]")).Text;
                    strLocationaddrss = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[4]/table[2]/tbody/tr[1]/td[2]")).Text;
                    strCity = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[4]/table[2]/tbody/tr[2]/td[2]")).Text;
                    strProprtydescription = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[4]/table[2]/tbody/tr[5]/td[2]")).Text;
                    strRealproperty = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[6]/table[2]/tbody/tr[1]/td[2]")).Text;
                    strYearBuilt = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[6]/table[2]/tbody/tr[2]/td[2]")).Text;
                    strLotSize = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[6]/table[2]/tbody/tr[3]/td[2]")).Text;
                    strLandUseCode = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[6]/table[2]/tbody/tr[4]/td[2]")).Text;
                    strStyle = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[6]/table[2]/tbody/tr[5]/td[2]")).Text;

                    string property_details = strClassname + "~" + strTaxdistrcit + "~" + strTaxyear + "~" + strOwnername + "~" + strLocationaddrss + "~" + strCity + "~" + strProprtydescription + "~" + strRealproperty + "~" + strYearBuilt + "~" + strLotSize + "~" + strLandUseCode + "~" + strStyle;
                    gc.insert_date(orderNumber, outparcelno, 100, property_details, 1, DateTime.Now);

                    //Assement Details
                    //driver.FindElement(By.XPath("//*[@id='sidemenu']/li[2]/a")).SendKeys(Keys.Enter);
                    //Screenshot
                    IWebElement IAddressSearch2 = driver.FindElement(By.LinkText("Values"));
                    IJavaScriptExecutor js2 = driver as IJavaScriptExecutor;
                    js2.ExecuteScript("arguments[0].click();", IAddressSearch2);
                    Thread.Sleep(3000);

                    gc.CreatePdf(orderNumber, outparcelno, "Assessment Details", driver, "NM", "Bernalillo");
                    //Values
                    strValues = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[1]/table[2]/tbody/tr[1]/td[2]")).Text;
                    strFullLandValue = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[1]/table[2]/tbody/tr[2]/td[2]")).Text;
                    strTablevalues = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[1]/table[2]/tbody/tr[3]/td[2]")).Text;
                    strFullImpValue = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[1]/table[2]/tbody/tr[4]/td[2]")).Text;
                    strTotalValue = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[1]/table[2]/tbody/tr[5]/td[2]")).Text;
                    strTaxable = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[1]/table[2]/tbody/tr[7]/td[2]")).Text;
                    strException = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[2]/table[2]/tbody/tr[1]/td[2]")).Text;
                    strVetaranan = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[2]/table[2]/tbody/tr[2]/td[2]")).Text;
                    strOther = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[2]/table[2]/tbody/tr[3]/td[2]")).Text;
                    strNettaxablevalue = driver.FindElement(By.XPath("/html/body/div/div[3]/section/div/form/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/div[3]/table[2]/tbody/tr[1]/td[2]")).Text;

                    string assement_details = strValues + "~" + strFullLandValue + "~" + strTablevalues + "~" + strFullImpValue + "~" + strTotalValue + "~" + strTaxable + "~" + strException + "~" + strVetaranan + "~" + strOther + "~" + strNettaxablevalue;
                    gc.insert_date(orderNumber, outparcelno, 101, assement_details, 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    //Tax Status

                    driver.Navigate().GoToUrl("https://www.bernco.gov/treasurer/property-tax-search.aspx");
                    Thread.Sleep(5000);
                    //Screenshot
                    gc.CreatePdf(orderNumber, outparcelno, "Address Search Result", driver, "NM", "Bernalillo");

                    //Tax Authority
                    Tax_Authority = "One Civic Plaza NW Albuquerque, NM 87102";

                    //IWebElement Multyaddresstable1 = driver.FindElement(By.Id("iFrameResizer1"));
                    //driver.SwitchTo().Frame(Multyaddresstable1);
                    //Thread.Sleep(7000);
                    //try { 
                    //IWebElement LinkSearch = driver.FindElement(By.Id("ctl03_TemplateBody_ctl00_PageLayout_ctl00_Placeholder3_ctl00_pageContent_ctl00_PlaceHolder2_ctl00_searchByParcel"));
                    //IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                    //js.ExecuteScript("arguments[0].click();", LinkSearch);
                    //Thread.Sleep(7000);
                    //gc.CreatePdf(orderNumber, outparcelno, "Parcel Search Result1", driver, "NM", "Bernalillo");
                    //}
                    //catch { }
                    //try
                    //{
                    //    driver.FindElement(By.Id("ctl03_TemplateBody_ctl00_PageLayout_ctl00_Placeholder3_ctl00_pageContent_ctl00_PlaceHolder2_ctl00_searchByParcel")).Click();
                    //    Thread.Sleep(5000);
                    //    gc.CreatePdf(orderNumber, outparcelno, "Parcel Search Result2", driver, "NM", "Bernalillo");
                    //}
                    //catch { }
                    //try
                    //{
                    //    driver.FindElement(By.Id("ctl03_TemplateBody_ctl00_PageLayout_ctl00_Placeholder3_ctl00_pageContent_ctl00_PlaceHolder2_ctl00_searchByParcel")).SendKeys(Keys.Enter);
                    //    Thread.Sleep(5000);
                    //    gc.CreatePdf(orderNumber, outparcelno, "Parcel Search Result3", driver, "NM", "Bernalillo");
                    //}
                    //catch { }

                    driver.Navigate().GoToUrl("https://wwp.bernco.gov/property-tax-search/");
                    try
                    {
                        IWebElement LinkSearch = driver.FindElement(By.Id("ctl03_TemplateBody_ctl00_PageLayout_ctl00_Placeholder3_ctl00_pageContent_ctl00_PlaceHolder2_ctl00_searchByParcel"));
                        IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                        js.ExecuteScript("arguments[0].click();", LinkSearch);
                        Thread.Sleep(5000);
                        gc.CreatePdf(orderNumber, outparcelno, "Parcel Search Result1", driver, "NM", "Bernalillo");
                    }
                    catch { }

                    try
                    {
                        driver.FindElement(By.Id("ctl03_TemplateBody_ctl00_PageLayout_ctl00_Placeholder3_ctl00_pageContent_ctl00_Placeholder5_ctl00_ctl00_parcelId")).Click();
                        Thread.Sleep(5000);
                        driver.FindElement(By.Id("ctl03_TemplateBody_ctl00_PageLayout_ctl00_Placeholder3_ctl00_pageContent_ctl00_Placeholder5_ctl00_ctl00_parcelId")).SendKeys(outparcelno);
                        gc.CreatePdf(orderNumber, outparcelno, "Parcel Click Result", driver, "NM", "Bernalillo");
                        driver.FindElement(By.Id("ctl03_TemplateBody_ctl00_PageLayout_ctl00_Placeholder3_ctl00_pageContent_ctl00_Placeholder5_ctl00_ctl00_submit")).Click();
                        Thread.Sleep(7000);
                    }
                    catch { }
                    IWebElement ButtonLinkSearch = driver.FindElement(By.Id("ctl03_TemplateBody_ctl00_PageLayout_ctl00_Placeholder3_ctl00_pageContent_ctl00_Placeholder5_ctl00_placeHolder_ctl00_resultList_ctl00_ctl04_parcel"));
                    IJavaScriptExecutor js3 = driver as IJavaScriptExecutor;
                    js3.ExecuteScript("arguments[0].click();", ButtonLinkSearch);
                    Thread.Sleep(9000);
                    string Ownershiptable = driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[6]/div[5]/div[1]/div[1]/div[3]/div[2]/div/table/tbody/tr[2]/td/table/tbody")).Text;
                    if (Ownershiptable.Contains("OWNER 2:"))
                    {
                        Owner1 = driver.FindElement(By.Id("ctl03_TemplateBody_ctl00_PageLayout_ctl00_Placeholder3_ctl00_pageContent_ctl00_Placeholder5_ctl00_placeHolder_ctl00_owner2Row")).Text.Replace("OWNER 2:", "");
                    }
                    else
                    {
                        Owner1 = driver.FindElement(By.Id("ctl03_TemplateBody_ctl00_PageLayout_ctl00_Placeholder3_ctl00_pageContent_ctl00_Placeholder5_ctl00_placeHolder_ctl00_owner1Row")).Text.Replace("OWNER 1:", "");
                    }
                    Location_Address = driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[6]/div[5]/div[1]/div[1]/div[3]/div[2]/div/table/tbody/tr[3]/td/table/tbody/tr/td")).Text;
                    LegalDescription = driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[6]/div[5]/div[1]/div[1]/div[3]/div[2]/div/table/tbody/tr[4]/td/ul/li")).Text;

                    //Notice Of Values  
                    IWebElement IValue = driver.FindElement(By.Id("ctl03_TemplateBody_ctl00_PageLayout_ctl00_Placeholder3_ctl00_pageContent_ctl00_PlaceHolder2_ctl00_noticeOfValues"));
                    //js1.ExecuteScript("arguments[0].click();", IValue);
                    IValue.Click();
                    Thread.Sleep(9000);
                    gc.CreatePdf(orderNumber, outparcelno, "Notice of Values Search", driver, "NM", "Bernalillo");

                    //Screenshot

                    //Tax Bill Download
                    IWebElement ITaxbill = driver.FindElement(By.Id("ctl03_TemplateBody_ctl00_PageLayout_ctl00_Placeholder3_ctl00_pageContent_ctl00_PlaceHolder2_ctl00_taxBill"));
                    //js1.ExecuteScript("arguments[0].click();", ITaxbill);
                    ITaxbill.Click();
                    Thread.Sleep(9000);
                    gc.CreatePdf(orderNumber, outparcelno, "Tax Bill Search", driver, "NM", "Bernalillo");
                    //Screenshot
                    try
                    {
                        //Take tax bills amount and store into table 
                        IWebElement tbTaxbill = driver.FindElement(By.XPath("//*[@id='ctl03_TemplateBody_ctl00_PageLayout_ctl00_Placeholder3_ctl00_pageContent_ctl00_Placeholder5_ctl00_placeHolder_ctl00_panel']/div/table[1]/tbody/tr/td[2]/table/tbody"));
                        IList<IWebElement> trTaxbill = tbTaxbill.FindElements(By.TagName("tr"));
                        IList<IWebElement> tdTaxbill;
                        foreach (IWebElement row in trTaxbill)
                        {
                            tdTaxbill = row.FindElements(By.TagName("td"));
                            if (tdTaxbill.Count != 0 && tdTaxbill[0].Text != " ")
                            {

                                Agencies = GlobalClass.After(tdTaxbill[0].Text, ":");
                                TaxRate = GlobalClass.After(tdTaxbill[1].Text, ":");
                                NetTaxableValue = GlobalClass.After(tdTaxbill[2].Text, ":");
                                AmountDue = GlobalClass.After(tdTaxbill[3].Text, ":");

                                if (Agencies == "")
                                {
                                    Agencies = tdTaxbill[0].Text;
                                    TaxRate = tdTaxbill[1].Text;
                                    NetTaxableValue = tdTaxbill[2].Text;
                                    AmountDue = tdTaxbill[3].Text;
                                }


                                strRowData = Agencies + "~" + TaxRate + "~" + NetTaxableValue + "~" + AmountDue;
                                gc.insert_date(orderNumber, outparcelno, 115, strRowData, 1, DateTime.Now);
                            }
                        }
                    }
                    catch
                    { }
                    //OtherTaxDue 
                    try
                    {
                        IWebElement Tbtx = driver.FindElement(By.XPath("//*[@id='ctl03_TemplateBody_ctl00_PageLayout_ctl00_Placeholder3_ctl00_pageContent_ctl00_Placeholder5_ctl00_placeHolder_ctl00_panel']/div/table[2]/tbody"));
                        IList<IWebElement> TRtx = Tbtx.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDtx;

                        foreach (IWebElement row5 in TRtx)
                        {
                            if (!row5.Text.Contains("YEAR"))
                            {
                                TDtx = row5.FindElements(By.TagName("td"));
                                OtherTaxDueYear = driver.FindElement(By.XPath("/html/body/form/div[6]/div[5]/div[1]/div[1]/div[3]/div[2]/div/div/div/div/table[2]/tbody/tr[2]/td[1]")).Text;
                                OtherTaxDueTax = driver.FindElement(By.XPath("/html/body/form/div[6]/div[5]/div[1]/div[1]/div[3]/div[2]/div/div/div/div/table[2]/tbody/tr[2]/td[2]")).Text;
                                OtherTaxDueInterest = driver.FindElement(By.XPath("/html/body/form/div[6]/div[5]/div[1]/div[1]/div[3]/div[2]/div/div/div/div/table[2]/tbody/tr[2]/td[3]")).Text;
                                OtherTaxDuePanalty = driver.FindElement(By.XPath("/html/body/form/div[6]/div[5]/div[1]/div[1]/div[3]/div[2]/div/div/div/div/table[2]/tbody/tr[2]/td[4]")).Text;
                                OtherTaxDueFees = driver.FindElement(By.XPath("/html/body/form/div[6]/div[5]/div[1]/div[1]/div[3]/div[2]/div/div/div/div/table[2]/tbody/tr[2]/td[5]")).Text;
                                OtherTaxDueAmountDue = driver.FindElement(By.XPath("/html/body/form/div[6]/div[5]/div[1]/div[1]/div[3]/div[2]/div/div/div/div/table[2]/tbody/tr[2]/td[6]")).Text;
                                string thertax = OtherTaxDueYear + "~" + OtherTaxDueTax + "~" + OtherTaxDueInterest + "~" + OtherTaxDuePanalty + "~" + OtherTaxDueFees + "~" + OtherTaxDueAmountDue;
                                gc.insert_date(orderNumber, outparcelno, 118, thertax, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    //Tax And Payment History
                    IWebElement ITaxpay = driver.FindElement(By.Id("ctl03_TemplateBody_ctl00_PageLayout_ctl00_Placeholder3_ctl00_pageContent_ctl00_PlaceHolder2_ctl00_taxPaymentHistory"));
                    //js1.ExecuteScript("arguments[0].click();", ITaxpay);
                    ITaxpay.Click();
                    Thread.Sleep(9000);
                    gc.CreatePdf(orderNumber, outparcelno, "Parcel Search", driver, "NM", "Bernalillo");

                    IWebElement Tableinstallment = driver.FindElement(By.XPath("//*[@id='ctl03_TemplateBody_ctl00_PageLayout_ctl00_Placeholder3_ctl00_pageContent_ctl00_Placeholder5_ctl00_placeHolder_ctl00_historyList_ctl00']/tbody"));
                    IList<IWebElement> TRinstallment = Tableinstallment.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDinstallment;

                    try
                    {
                        foreach (IWebElement row in TRinstallment)
                        {
                            if (!row.Text.Contains("Summary of Taxes Due"))
                            {
                                TDinstallment = row.FindElements(By.TagName("td"));
                                if (TDinstallment.Count != 0)
                                {
                                    try
                                    {

                                        taxYear = GlobalClass.After(TDinstallment[0].Text, "\r\n");
                                        netTaxable = GlobalClass.After(TDinstallment[1].Text, "\r\n");
                                        tax = GlobalClass.After(TDinstallment[2].Text, "\r\n");
                                        Interest = GlobalClass.After(TDinstallment[3].Text, "\r\n");
                                        Penalty = GlobalClass.After(TDinstallment[4].Text, "\r\n");
                                        Fees = GlobalClass.After(TDinstallment[5].Text, "\r\n");
                                        Paid = GlobalClass.After(TDinstallment[6].Text, "\r\n");
                                        AmountDue = GlobalClass.After(TDinstallment[7].Text, "\r\n");
                                        if (taxYear == "" && netTaxable == "")
                                        {

                                            taxYear = TDinstallment[0].Text;
                                            netTaxable = TDinstallment[1].Text;
                                            tax = TDinstallment[2].Text;
                                            Interest = TDinstallment[3].Text;
                                            Penalty = TDinstallment[4].Text;
                                            Fees = TDinstallment[5].Text;
                                            Paid = TDinstallment[6].Text;
                                            AmountDue = TDinstallment[7].Text;

                                        }

                                        if (taxYear == "")
                                        {
                                            FirstHalftax = driver.FindElement(By.XPath("/html/body/form/div[6]/div[5]/div[1]/div[1]/div[3]/div[2]/div[1]/div/div[1]/table/tbody/tr[12]/td[3]")).Text;
                                            FirstHalfInterest = driver.FindElement(By.XPath("/html/body/form/div[6]/div[5]/div[1]/div[1]/div[3]/div[2]/div[1]/div/div[1]/table/tbody/tr[12]/td[4]")).Text;
                                            FirstHalfPenalty = driver.FindElement(By.XPath("/html/body/form/div[6]/div[5]/div[1]/div[1]/div[3]/div[2]/div[1]/div/div[1]/table/tbody/tr[12]/td[5]")).Text;
                                            FirstHalfFees = driver.FindElement(By.XPath("/html/body/form/div[6]/div[5]/div[1]/div[1]/div[3]/div[2]/div[1]/div/div[1]/table/tbody/tr[12]/td[6]")).Text;
                                            FirstHalfPaid = driver.FindElement(By.XPath("/html/body/form/div[6]/div[5]/div[1]/div[1]/div[3]/div[2]/div[1]/div/div[1]/table/tbody/tr[12]/td[7]")).Text;
                                            FirstHalfAmountDue = driver.FindElement(By.XPath("/html/body/form/div[6]/div[5]/div[1]/div[1]/div[3]/div[2]/div[1]/div/div[1]/table/tbody/tr[12]/td[8]")).Text;

                                            SecondHalftax = driver.FindElement(By.XPath("/html/body/form/div[6]/div[5]/div[1]/div[1]/div[3]/div[2]/div[1]/div/div[1]/table/tbody/tr[13]/td[3]")).Text;
                                            SecondHalfInterest = driver.FindElement(By.XPath("/html/body/form/div[6]/div[5]/div[1]/div[1]/div[3]/div[2]/div[1]/div/div[1]/table/tbody/tr[13]/td[4]")).Text;
                                            SecondHalfPenalty = driver.FindElement(By.XPath("/html/body/form/div[6]/div[5]/div[1]/div[1]/div[3]/div[2]/div[1]/div/div[1]/table/tbody/tr[13]/td[5]")).Text;
                                            SecondHalfFees = driver.FindElement(By.XPath("/html/body/form/div[6]/div[5]/div[1]/div[1]/div[3]/div[2]/div[1]/div/div[1]/table/tbody/tr[13]/td[6]")).Text;
                                            SecondHalfPaid = driver.FindElement(By.XPath("/html/body/form/div[6]/div[5]/div[1]/div[1]/div[3]/div[2]/div[1]/div/div[1]/table/tbody/tr[13]/td[7]")).Text;
                                            SecondHalfAmountDue = driver.FindElement(By.XPath("/html/body/form/div[6]/div[5]/div[1]/div[1]/div[3]/div[2]/div[1]/div/div[1]/table/tbody/tr[13]/td[8]")).Text;

                                            TotalHalftax = driver.FindElement(By.XPath("/html/body/form/div[6]/div[5]/div[1]/div[1]/div[3]/div[2]/div[1]/div/div[1]/table/tbody/tr[14]/td[3]")).Text;
                                            TotalHalfInterest = driver.FindElement(By.XPath("/html/body/form/div[6]/div[5]/div[1]/div[1]/div[3]/div[2]/div[1]/div/div[1]/table/tbody/tr[14]/td[4]")).Text;
                                            TotalHalfPenalty = driver.FindElement(By.XPath("/html/body/form/div[6]/div[5]/div[1]/div[1]/div[3]/div[2]/div[1]/div/div[1]/table/tbody/tr[14]/td[5]")).Text;
                                            TotalHalfFees = driver.FindElement(By.XPath("/html/body/form/div[6]/div[5]/div[1]/div[1]/div[3]/div[2]/div[1]/div/div[1]/table/tbody/tr[14]/td[6]")).Text;
                                            TotalHalfPaid = driver.FindElement(By.XPath("/html/body/form/div[6]/div[5]/div[1]/div[1]/div[3]/div[2]/div[1]/div/div[1]/table/tbody/tr[14]/td[7]")).Text;
                                            TotalHalfAmountDue = driver.FindElement(By.XPath("/html/body/form/div[6]/div[5]/div[1]/div[1]/div[3]/div[2]/div[1]/div/div[1]/table/tbody/tr[14]/td[8]")).Text;

                                        }
                                        if (taxYear == "" && netTaxable == "")
                                        {

                                            string taxinf = Owner1 + "~" + Location_Address + "~" + LegalDescription + "~" + "2017" + "~" + netTaxable + "~" + tax + "~" + Interest + "~" + Penalty + "~" + Fees + "~" + Paid + "~" + AmountDue + "~" + FirstHalftax + "~" + FirstHalfInterest + "~" + FirstHalfPenalty + "~" + FirstHalfFees + "~" + FirstHalfPaid + "~" + FirstHalfAmountDue + "~" + SecondHalftax + "~" + SecondHalfInterest + "~" + SecondHalfPenalty + "~" + SecondHalfFees + "~" + SecondHalfPaid + "~" + SecondHalfAmountDue + "~" + TotalHalftax + "~" + TotalHalfInterest + "~" + TotalHalfPenalty + "~" + TotalHalfFees + "~" + TotalHalfPaid + "~" + TotalHalfAmountDue + "~" + Tax_Authority + "~";
                                            gc.insert_date(orderNumber, outparcelno, 114, taxinf, 1, DateTime.Now);

                                        }
                                        else
                                        {
                                            string taxinf = Owner1 + "~" + Location_Address + "~" + LegalDescription + "~" + taxYear + "~" + netTaxable + "~" + tax + "~" + Interest + "~" + Penalty + "~" + Fees + "~" + Paid + "~" + AmountDue + "~" + FirstHalftax + "~" + FirstHalfInterest + "~" + FirstHalfPenalty + "~" + FirstHalfFees + "~" + FirstHalfPaid + "~" + FirstHalfAmountDue + "~" + SecondHalftax + "~" + SecondHalfInterest + "~" + SecondHalfPenalty + "~" + SecondHalfFees + "~" + SecondHalfPaid + "~" + SecondHalfAmountDue + "~" + TotalHalftax + "~" + TotalHalfInterest + "~" + TotalHalfPenalty + "~" + TotalHalfFees + "~" + TotalHalfPaid + "~" + TotalHalfAmountDue + "~" + Tax_Authority + "~";
                                            gc.insert_date(orderNumber, outparcelno, 114, taxinf, 1, DateTime.Now);
                                        }
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                    catch { }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "NM", "Bernalillo", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "NM", "Bernalillo");
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
