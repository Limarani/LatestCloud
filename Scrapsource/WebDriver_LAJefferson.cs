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
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_LAJefferson
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        private object driverIE;
        private int multicount;
        private string strMultiAddress;
        private string b;
        string mul = "";
        public string FTP_Jefferson(string address, string ownername, string assessment_id, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass gc = new GlobalClass();

            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "LA", "Jefferson");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_LAJefferson"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }


                    driver.Navigate().GoToUrl("http://www.jpassessor.com/property-search");
                    Thread.Sleep(4000);
                    if (searchType == "address")
                    {
                        driver.FindElement(By.XPath("//*[@id='radio-address']")).Click();
                        Thread.Sleep(3000);
                        driver.FindElement(By.XPath("//*[@id='address']")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "LA", "Jefferson");
                        driver.FindElement(By.Id("btn-search-address")).Click();
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Address search result", driver, "LA", "Jefferson");
                        mul = driver.FindElement(By.XPath("//*[@id='panel-results']/div[2]/div[3]/div/span")).Text.Trim();
                        mul = WebDriverTest.Before(mul, " of");
                        List<string> searchlist = new List<string>();

                        IWebElement checksingle = driver.FindElement(By.XPath("//*[@id='panel-results']/div[1]/table/tbody"));
                        IList<IWebElement> TRchecksingle = checksingle.FindElements(By.TagName("tr"));
                        int j = 1;
                        IList<IWebElement> TDchecksingle;
                        foreach (IWebElement row in TRchecksingle)
                        {
                            if (j <= 25)
                            {
                                TDchecksingle = row.FindElements(By.TagName("td"));
                                if (TDchecksingle.Count != 0 && row.Text.Contains(address.ToUpper()))
                                {
                                    if (TDchecksingle[2].Text == address.ToUpper())
                                    {
                                        driver.FindElement(By.XPath("//*[@id='panel-results']/div[1]/table/tbody/tr[" + j + "]/td[1]/a")).Click();
                                        Thread.Sleep(3000);
                                        driver.SwitchTo().Window(driver.WindowHandles.Last());
                                        Thread.Sleep(2000);
                                        break;
                                    }
                                }
                                j++;
                            }

                        }
                        try
                        {
                            if (mul != "1 - 1")
                            {
                                //multi parcel
                                IWebElement tbmulti = driver.FindElement(By.XPath("//*[@id='panel-results']/div[1]/table/tbody"));
                                IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));
                                int k = 0;
                                IList<IWebElement> TDmulti;
                                foreach (IWebElement row in TRmulti)
                                {
                                    if (k <= 25)
                                    {
                                        TDmulti = row.FindElements(By.TagName("td"));
                                        if (TDmulti.Count != 0 && row.Text.Contains(address.ToUpper()))
                                        {
                                            if (TDmulti[2].Text == address.ToUpper())
                                            {
                                                driver.FindElement(By.XPath("//*[@id='panel-results']/div[1]/table/tbody/tr[" + k + "]/td[1]/a")).Click();
                                                break;
                                            }
                                            else
                                            {
                                                string multi1 = TDmulti[1].Text + "~" + TDmulti[2].Text + "~" + TDmulti[3].Text;
                                                gc.insert_date(orderNumber, TDmulti[0].Text, 245, multi1, 1, DateTime.Now);
                                            }
                                            searchlist.Add(TDmulti[0].Text);


                                        }
                                        k++;
                                    }

                                }

                                if (TRmulti.Count > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_Jefferson_Multicount"] = "Maximum";
                                }
                                if (searchlist.Count > 1)
                                {
                                    HttpContext.Current.Session["multiParcel_Jefferson"] = "Yes";
                                }
                                driver.Quit();
                                return "MultiParcel";
                            }
                            else
                            {
                                try
                                {
                                    driver.FindElement(By.XPath("//*[@id='panel-results']/div[1]/table/tbody/tr/td[1]/a")).Click();
                                    Thread.Sleep(3000);
                                    driver.SwitchTo().Window(driver.WindowHandles.Last());
                                    Thread.Sleep(2000);
                                }
                                catch { }
                            }

                        }
                        catch
                        {

                        }
                    }
                    else if (searchType == "parcel")
                    {
                        driver.FindElement(By.XPath("//*[@id='radio-parcel']")).Click();
                        Thread.Sleep(3000);

                        if (parcelNumber.Contains("-"))
                        {
                            parcelNumber = parcelNumber.Replace("-", "");
                        }
                        driver.FindElement(By.XPath("//*[@id='parcel']")).SendKeys(parcelNumber);

                        driver.FindElement(By.XPath("//*[@id='btn-search-parcel']")).Click();
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search result", driver, "LA", "Jefferson");

                        mul = driver.FindElement(By.XPath("//*[@id='panel-results']/div[2]/div[3]/div/span")).Text.Trim();

                        mul = WebDriverTest.Before(mul, " of");
                        if (mul != "1 - 1")
                        {
                            //multi parcel
                            IWebElement tbmulti = driver.FindElement(By.XPath("//*[@id='panel-results']/div[1]/table/tbody"));
                            IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));
                            int l = 0;
                            IList<IWebElement> TDmulti;
                            foreach (IWebElement row in TRmulti)
                            {
                                if (l <= 25)
                                {
                                    TDmulti = row.FindElements(By.TagName("td"));
                                    if (TDmulti.Count != 0)
                                    {
                                        string multi1 = TDmulti[1].Text + "~" + TDmulti[2].Text + "~" + TDmulti[3].Text;
                                        gc.insert_date(orderNumber, TDmulti[0].Text, 245, multi1, 1, DateTime.Now);
                                    }
                                    l++;
                                }
                            }
                            if (TRmulti.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Jefferson_Multicount"] = "Maximum";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiParcel_Jefferson"] = "Yes";
                            }
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//*[@id='panel-results']/div[1]/table/tbody/tr/td[1]/a")).Click();
                                Thread.Sleep(3000);
                                driver.SwitchTo().Window(driver.WindowHandles.Last());
                                Thread.Sleep(2000);
                            }
                            catch { }
                        }
                    }
                    else if (searchType == "ownername")
                    {
                        driver.FindElement(By.XPath("//*[@id='radio-owner']")).Click();
                        Thread.Sleep(3000);

                        driver.FindElement(By.XPath("//*[@id='owner']")).SendKeys(ownername);

                        driver.FindElement(By.XPath("//*[@id='btn-search-owner']")).Click();
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Owner search result", driver, "LA", "Jefferson");

                        mul = driver.FindElement(By.XPath("//*[@id='panel-results']/div[2]/div[3]/div/span")).Text.Trim();

                        mul = WebDriverTest.Before(mul, " of");
                        if (mul != "1 - 1")
                        {
                            //multi parcel
                            IWebElement tbmulti = driver.FindElement(By.XPath("//*[@id='panel-results']/div[1]/table/tbody"));
                            IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));
                            int l = 0;
                            IList<IWebElement> TDmulti;
                            foreach (IWebElement row in TRmulti)
                            {
                                if (l <= 25)
                                {
                                    TDmulti = row.FindElements(By.TagName("td"));
                                    if (TDmulti.Count != 0)
                                    {
                                        string multi1 = TDmulti[1].Text + "~" + TDmulti[2].Text + "~" + TDmulti[3].Text;
                                        gc.insert_date(orderNumber, TDmulti[0].Text, 245, multi1, 1, DateTime.Now);
                                    }
                                    l++;
                                }
                            }

                            if (TRmulti.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Jefferson_Multicount"] = "Maximum";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiParcel_Jefferson"] = "Yes";
                            }
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//*[@id='panel-results']/div[1]/table/tbody/tr/td[1]/a")).Click();
                                Thread.Sleep(3000);
                                driver.SwitchTo().Window(driver.WindowHandles.Last());
                                Thread.Sleep(2000);
                            }
                            catch { }
                        }
                    }

                    try
                    {
                        string nodata = driver.FindElement(By.XPath("//*[@id='panel-results']")).Text;
                        if (nodata.Contains("0 - 0 of 0 items"))
                        {
                            HttpContext.Current.Session["Nodata_LAJefferson"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    //property details
                    string Parcel_no = "", Ward_no = "", Owner_Name = "", Improvement_Address = "", Homestead_Exemption_Status = "", Subdivision = "", Legal_Description = "", Land_Assessment = "", Improvement_Assessment = "", Total_Assessment = "";
                    //gc.CreatePdf_WOP(orderNumber, "propert search result", driver);
                    Parcel_no = driver.FindElement(By.XPath("/html/body/div[2]/div/div/div/table/tbody/tr[2]/td/table/tbody/tr[1]/td[2]/span")).Text.Trim();
                    gc.CreatePdf(orderNumber, Parcel_no, "property search result", driver, "LA", "Jefferson");
                    Ward_no = driver.FindElement(By.XPath(" /html/body/div[2]/div/div/div/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]")).Text.Trim();
                    Owner_Name = driver.FindElement(By.XPath("/ html/body/div[2]/div/div/div/table/tbody/tr[2]/td/table/tbody/tr[3]/td[2]")).Text.Trim();
                    Improvement_Address = driver.FindElement(By.XPath("/ html/body/div[2]/div/div/div/table/tbody/tr[2]/td/table/tbody/tr[5]/td[2]")).Text.Trim();
                    Homestead_Exemption_Status = driver.FindElement(By.XPath("/ html/body/div[2]/div/div/div/table/tbody/tr[2]/td/table/tbody/tr[3]/td[4]")).Text.Trim();
                    Subdivision = driver.FindElement(By.XPath(" / html/body/div[2]/div/div/div/table/tbody/tr[2]/td/table/tbody/tr[4]/td[4]")).Text.Trim();
                    Legal_Description = driver.FindElement(By.XPath("/ html/body/div[2]/div/div/div/table/tbody/tr[2]/td/table/tbody/tr[5]/td[4]")).Text.Trim();

                    string assessment_details = Ward_no + "~" + Owner_Name + "~" + Improvement_Address + "~" + Homestead_Exemption_Status + "~" + Subdivision + "~" + Legal_Description;
                    gc.insert_date(orderNumber, Parcel_no, 246, assessment_details, 1, DateTime.Now);

                    //Assessment details

                    Land_Assessment = driver.FindElement(By.XPath("/ html/body/div[2]/div/div/div/table/tbody/tr[3]/td/table/tbody/tr[2]/td[1]")).Text.Trim();
                    Improvement_Assessment = driver.FindElement(By.XPath(" / html/body/div[2]/div/div/div/table/tbody/tr[3]/td/table/tbody/tr[2]/td[2]")).Text.Trim();
                    Total_Assessment = driver.FindElement(By.XPath("/ html/body/div[2]/div/div/div/table/tbody/tr[3]/td/table/tbody/tr[2]/td[3]")).Text.Trim();
                    string property_details = Land_Assessment + "~" + Improvement_Assessment + "~" + Total_Assessment;
                    gc.insert_date(orderNumber, Parcel_no, 247, property_details, 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");


                    string add1 = "200 Derbigny St.Suite 1200 Gretna, LA 70053", add2 = "504-363-5637", add3 = "504-363-5644";

                    string taxauthority_address = add1 + "  PH:" + add2 + "  FAX: " + add3;

                    driver.Navigate().GoToUrl("https://propertytax.jpso.com/PropertyTax/propsrch.aspx#result");
                    Thread.Sleep(3000);
                    var Select = driver.FindElement(By.Id("ContentPlaceHolder1_body_cboSearchBy"));
                    var selectElement1 = new SelectElement(Select);
                    selectElement1.SelectByText("Parcel #");
                    driver.FindElement(By.Id("ContentPlaceHolder1_body_txtParcel_In")).SendKeys(Parcel_no);
                    gc.CreatePdf(orderNumber, Parcel_no, "property tax search ", driver, "LA", "Jefferson");
                    driver.FindElement(By.Id("ContentPlaceHolder1_body_btSearch")).Click();
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, Parcel_no, "property tax search result", driver, "LA", "Jefferson");
                    string Hex = "", Parcel_no1 = "", Notice_no = "", Assessment = "", Tax_Due = "", Tax_Year = "";
                    Hex = driver.FindElement(By.Id("ContentPlaceHolder1_body_lblHEX")).Text.Trim();
                    Parcel_no1 = driver.FindElement(By.Id("ContentPlaceHolder1_body_lblParcel")).Text.Trim();
                    Notice_no = driver.FindElement(By.Id("ContentPlaceHolder1_body_lblBillNum")).Text.Trim();
                    Assessment = driver.FindElement(By.Id("ContentPlaceHolder1_body_lblAssessAmt")).Text.Trim();
                    Tax_Due = driver.FindElement(By.Id("ContentPlaceHolder1_body_lblTaxAmt")).Text.Trim();
                    Tax_Year = driver.FindElement(By.Id("ContentPlaceHolder1_body_lblTaxYear")).Text.Trim();

                    string taxinfo_details = Hex + "~" + Assessment + "~" + Tax_Due + "~" + Tax_Year + "~" + taxauthority_address;
                    gc.insert_date(orderNumber, Parcel_no, 240, taxinfo_details, 1, DateTime.Now);
                    //    Tax Notices and Payments Details Table:
                    driver.FindElement(By.Id("ContentPlaceHolder1_body_btViewHistory")).Click();
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, Parcel_no, "View history", driver, "LA", "Jefferson");
                    IWebElement multitableElement1 = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_body_dgHistory']/tbody"));
                    IList<IWebElement> multitableRow1 = multitableElement1.FindElements(By.TagName("tr"));
                    IList<IWebElement> multirowTD1;
                    foreach (IWebElement row in multitableRow1)
                    {
                        if (!row.Text.Contains("Tax Year"))
                        {
                            multirowTD1 = row.FindElements(By.TagName("td"));

                            string tax_payment = multirowTD1[0].Text.Trim() + "~" + multirowTD1[1].Text.Trim() + "~" + multirowTD1[2].Text.Trim() + "~" + multirowTD1[3].Text.Trim() + "~" + multirowTD1[4].Text.Trim() + "~" + multirowTD1[5].Text.Trim() + "~" + multirowTD1[6].Text.Trim();
                            gc.insert_date(orderNumber, Parcel_no, 248, tax_payment, 1, DateTime.Now);
                        }
                    }
                    driver.FindElement(By.XPath("//*[@id='MAIN_OUTLINE_TABLE']/div[1]/div/div[2]/a")).Click();
                    Thread.Sleep(3000);

                    driver.FindElement(By.Id("ContentPlaceHolder1_body_btViewMilage")).Click();
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, Parcel_no, "View millage", driver, "LA", "Jefferson");
                    string Bill_year = "", Bill_no = "", Tax_Due1 = "";

                    Bill_year = driver.FindElement(By.Id("ContentPlaceHolder1_body_lblYear")).Text.Trim();
                    Bill_no = driver.FindElement(By.Id("ContentPlaceHolder1_body_lblBillnumber")).Text.Trim();
                    Tax_Due1 = driver.FindElement(By.Id("ContentPlaceHolder1_body_LblTaxDue")).Text.Trim();
                    string tax_Milege1 = Bill_year + "~" + Bill_no + "~" + Tax_Due1 + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-";

                    gc.insert_date(orderNumber, Parcel_no, 257, tax_Milege1, 1, DateTime.Now);
                    try
                    {
                        IWebElement multitableElement11 = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_body_dgMileage']/tbody"));
                        IList<IWebElement> multitableRow11 = multitableElement11.FindElements(By.TagName("tr"));
                        IList<IWebElement> multirowTD11;
                        foreach (IWebElement row in multitableRow11)
                        {
                            if (!row.Text.Contains("Category"))
                            {
                                multirowTD11 = row.FindElements(By.TagName("td"));

                                string tax_Milege = "-" + "~" + "-" + "~" + "-" + "~" + multirowTD11[0].Text.Trim() + "~" + multirowTD11[1].Text.Trim() + "~" + multirowTD11[2].Text.Trim() + "~" + multirowTD11[3].Text.Trim();
                                gc.insert_date(orderNumber, Parcel_no, 257, tax_Milege, 1, DateTime.Now);

                            }
                        }
                    }
                    catch { }

                    //  State Tax Commission information can be obtained through the link.

                    driver.Navigate().GoToUrl("http://www.latax.state.la.us/Menu_ParishTaxRolls/TaxRolls.aspx");
                    Thread.Sleep(3000);
                    //address search
                    var Select1 = driver.FindElement(By.Id("ctl00_ContentPlaceHolderMain_ddParish"));
                    var selectElement11 = new SelectElement(Select1);
                    selectElement11.SelectByText("JEFFERSON PARISH");
                    Thread.Sleep(1000);
                    var Select11 = driver.FindElement(By.Id("ctl00_ContentPlaceHolderMain_ddYear"));
                    var selectElement12 = new SelectElement(Select11);
                    selectElement12.SelectByIndex(2);
                    //  selectElement12.SelectByText("2017");
                    Thread.Sleep(1000);
                    driver.FindElement(By.Id("ctl00_ContentPlaceHolderMain_rbSearchField_0")).Click();
                    Thread.Sleep(1000);
                    driver.FindElement(By.Id("ctl00_ContentPlaceHolderMain_txtSearch")).SendKeys(Improvement_Address);
                    driver.FindElement(By.Id("ctl00_ContentPlaceHolderMain_btnSubmitSearch")).Click();
                    Thread.Sleep(3000);

                    driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolderMain_gvResults']/tbody/tr[2]/td[5]/a")).Click();
                    Thread.Sleep(3000);

                    //DOWNLOAD REPORT

                    IWebElement Itaxbill = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolderMain_lblReport']/a[1]"));
                    string URL1 = Itaxbill.GetAttribute("href");
                    gc.downloadfile(URL1, orderNumber, Parcel_no, "REPORT.pdf", "LA", "Jefferson");

                    driver.FindElement(By.Id("ctl00_ContentPlaceHolderMain_lbReset")).Click();
                    Thread.Sleep(3000);
                    var Select13 = driver.FindElement(By.Id("ctl00_ContentPlaceHolderMain_ddParish"));
                    var selectElement13 = new SelectElement(Select13);
                    selectElement13.SelectByText("JEFFERSON PARISH");
                    Thread.Sleep(1000);
                    var Select14 = driver.FindElement(By.Id("ctl00_ContentPlaceHolderMain_ddYear"));
                    var selectElement14 = new SelectElement(Select14);
                    selectElement14.SelectByIndex(2);
                    //selectElement14.SelectByText("2017");
                    Thread.Sleep(1000);
                    driver.FindElement(By.Id("ctl00_ContentPlaceHolderMain_rbSearchField_1")).Click();
                    Thread.Sleep(1000);
                    driver.FindElement(By.Id("ctl00_ContentPlaceHolderMain_txtSearch")).SendKeys(Bill_no);
                    driver.FindElement(By.Id("ctl00_ContentPlaceHolderMain_btnSubmitSearch")).Click();
                    Thread.Sleep(3000);
                    driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolderMain_gvResults']/tbody/tr[2]/td[5]/a")).Click();
                    Thread.Sleep(3000);
                    //DOWNLOAD REPORT
                    IWebElement Itaxbill1 = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolderMain_lblReport']/a[1] "));
                    string URL11 = Itaxbill1.GetAttribute("href");
                    gc.downloadfile(URL11, orderNumber, Parcel_no, "Assessment_REPORT.pdf", "LA", "Jefferson");

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "LA", "Jefferson", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "LA", "Jefferson");
                    return "Data Inserted Successfully";
                }
                catch (Exception ex)
                {
                    driver.Quit();
                    throw ex;
                }
            }
        }

    }
}