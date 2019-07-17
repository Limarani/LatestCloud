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
    public class Webdriver_COOKIL
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();
        IWebElement IAmg;
        public string FTP_CookIL(string Address, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string Parcel_number = "", Tax_Authority = "", As_of = "", Total_Due = "";
            //request.UseDefaultCredentials = true;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    if (searchType == "titleflex")
                    {
                        //string address = houseno + " " + direction + " " + streetname + " " + streettype;
                        gc.TitleFlexSearch(orderNumber, "", ownername, Address.Trim(), "IL", "Cook");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_CookIL"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString().Replace("-", "");
                        searchType = "parcel";
                    }
                    driver.Navigate().GoToUrl("https://www.cookcountytreasurer.com/setsearchparameters.aspx");
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.Id("ContentPlaceHolder1_ASPxPanel1_SearchByPIN1_txtPIN1")).SendKeys(parcelNumber.Substring(0, 2));
                        driver.FindElement(By.Id("ContentPlaceHolder1_ASPxPanel1_SearchByPIN1_txtPIN2")).SendKeys(parcelNumber.Substring(2, 2));
                        driver.FindElement(By.Id("ContentPlaceHolder1_ASPxPanel1_SearchByPIN1_txtPIN3")).SendKeys(parcelNumber.Substring(4, 3));
                        driver.FindElement(By.Id("ContentPlaceHolder1_ASPxPanel1_SearchByPIN1_txtPIN4")).SendKeys(parcelNumber.Substring(7, 3));
                        driver.FindElement(By.Id("ContentPlaceHolder1_ASPxPanel1_SearchByPIN1_txtPIN5")).SendKeys(parcelNumber.Substring(10, 4));
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax After click", driver, "IL", "Cook");
                        driver.FindElement(By.Id("ContentPlaceHolder1_ASPxPanel1_SearchByPIN1_cmdContinue")).Click();
                        Thread.Sleep(2000);
                        try
                        {
                            IWebElement Inodata = driver.FindElement(By.Id("ContentPlaceHolder1_ASPxPanel1_SearchByPIN1_panNoPropertyFoundMessage"));
                            if(Inodata.Text.Contains("No property was found under this Property Index Number (PIN)."))
                            {
                                HttpContext.Current.Session["Nodata_CookIL"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_DataViewNavigationDesktop1_ASPxMenu1_DXI0_T']")).Click();
                    Thread.Sleep(2000);
                    Parcel_number = driver.FindElement(By.Id("ContentPlaceHolder1_DataViewYourPropertyTaxOverviewResults1_DataViewHeader1_lblPIN")).Text;
                    string propertlocation1 = driver.FindElement(By.Id("ContentPlaceHolder1_DataViewYourPropertyTaxOverviewResults1_DataViewPropertyAndMailingAddress1_lblPropertyAddress")).Text;
                    string propertlocation2 = driver.FindElement(By.Id("ContentPlaceHolder1_DataViewYourPropertyTaxOverviewResults1_DataViewPropertyAndMailingAddress1_lblPropertyCityStateZipCode")).Text;
                    string propertlocation = propertlocation1 + propertlocation2;
                    string Mailingaddress1 = driver.FindElement(By.Id("ContentPlaceHolder1_DataViewYourPropertyTaxOverviewResults1_DataViewPropertyAndMailingAddress1_lblTaxpayerName1")).Text;
                    string Mailingaddress2 = driver.FindElement(By.Id("ContentPlaceHolder1_DataViewYourPropertyTaxOverviewResults1_DataViewPropertyAndMailingAddress1_lblMailingAddress1")).Text;
                    string Mailingaddress3 = driver.FindElement(By.Id("ContentPlaceHolder1_DataViewYourPropertyTaxOverviewResults1_DataViewPropertyAndMailingAddress1_lblMailingCityStateZipCode1")).Text;
                    string Mailingaddress = Mailingaddress1 + Mailingaddress2 + Mailingaddress3;
                    string taxyear = driver.FindElement(By.Id("ContentPlaceHolder1_DataViewYourPropertyTaxOverviewResults1_DataViewYourPropertyTaxOverviewPayments1_lblCurrentTaxYear")).Text;
                    string totalamtbill = driver.FindElement(By.Id("ContentPlaceHolder1_DataViewYourPropertyTaxOverviewResults1_DataViewYourPropertyTaxOverviewPayments1_lblCurrentTotalAmountBilled")).Text;
                    string totalamtdue = driver.FindElement(By.Id("ContentPlaceHolder1_DataViewYourPropertyTaxOverviewResults1_DataViewYourPropertyTaxOverviewPayments1_lblCurrentTotalAmountDue")).Text.Trim();
                    Tax_Authority = driver.FindElement(By.XPath("//*[@id='PageFooter']/div[2]/div/div/div[1]/div/div[1]")).Text.Trim();
                    gc.CreatePdf(orderNumber, Parcel_number, "Tax Bill detail", driver, "IL", "Cook");
                    for (int i = 1; i < 3; i++)
                    {
                        IWebElement instalmenttable = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_DataViewYourPropertyTaxOverviewResults1_DataViewYourPropertyTaxOverviewPayments1_panOnlineCurrent']/div[2]/div[1]/div/div[" + i + "]"));
                        string instalment = GlobalClass.Before(instalmenttable.Text, "Original Billed Amount:");
                        string Original_Billed = gc.Between(instalmenttable.Text, "Original Billed Amount:", "Due Date:").Trim();
                        string DueDate = gc.Between(instalmenttable.Text, "Due Date:", "Tax:").Trim();
                        string tax = gc.Between(instalmenttable.Text, "Tax:", "Interest:").Trim();
                        string Interest = gc.Between(instalmenttable.Text, "Interest:", "Last Payment Received:");
                        string LastPaymentReceived = gc.Between(instalmenttable.Text, "Last Payment Received:", "Date Received:");
                        string DateReceived = gc.Between(instalmenttable.Text, "Date Received:", "Current Amount Due:").Trim();
                        string CurrentAmountDue = GlobalClass.After(instalmenttable.Text, "Current Amount Due:").Trim();
                        string tax_bill_result = propertlocation + "~" + Mailingaddress + "~" + taxyear + "~" + totalamtbill + "~" + instalment + "~" + Original_Billed + "~" + DueDate + "~" + tax + "~" + Interest + "~" + LastPaymentReceived + "~" + DateReceived + "~" + CurrentAmountDue + "~" + totalamtdue + "~" + Tax_Authority;
                        gc.insert_date(orderNumber, Parcel_number, 1035, tax_bill_result, 1, DateTime.Now);
                    }
                    //Tax Exemptions
                    driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_DataViewNavigationDesktop1_ASPxMenu1_DXI3_T']")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Tax Exemptions", driver, "IL", "Cook");
                    IWebElement exceptionelement = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_DataViewExemptionHistoryResults1_panViewDataSet']/div[1]/p[1]"));
                    string exceptiontaxyear = gc.Between(exceptionelement.Text, "for", ".").Trim();
                    IWebElement exceptiontaxyeartable = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_DataViewExemptionHistoryResults1_panViewDataSet']/div[3]/div[1]/table/tbody"));
                    IList<IWebElement> exceptiontaxyearrow = exceptiontaxyeartable.FindElements(By.TagName("tr"));
                    IList<IWebElement> exceptiontaxyearid;
                    foreach (IWebElement exception in exceptiontaxyearrow)
                    {
                        exceptiontaxyearid = exception.FindElements(By.TagName("td"));
                        if (exceptiontaxyearid[0].Text.Trim() == "")
                        {
                            DBconnection dbconn = new DBconnection();
                            string execptionyear = exceptiontaxyear + "~" + exceptiontaxyearid[1].Text + "~" + exceptiontaxyearid[2].Text + "~" + exceptiontaxyearid[3].Text + "~" + exceptiontaxyearid[4].Text;
                            dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + execptionyear + "' where Id = '" + 1036 + "'");
                        }
                        if (exceptiontaxyearid.Count != 0 && exceptiontaxyearid[0].Text.Trim() != "")
                        {
                            string exceptionyearresult = exceptiontaxyearid[0].Text + "~" + exceptiontaxyearid[1].Text + "~" + exceptiontaxyearid[2].Text + "~" + exceptiontaxyearid[3].Text + "~" + exceptiontaxyearid[4].Text;
                            gc.insert_date(orderNumber, Parcel_number, 1036, exceptionyearresult, 1, DateTime.Now);
                        }

                    }
                    //Delinquent Taxes
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_DataViewNavigationDesktop1_ASPxMenu1_DXI4_T']")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, Parcel_number, "Delinquent Taxes1", driver, "IL", "Cook");
                        string tax_year = driver.FindElement(By.Id("ContentPlaceHolder1_DataViewDelinquentPropertyTaxResults1_DataList1_lblTaxYear_0")).Text;
                        string tax_type = driver.FindElement(By.Id("ContentPlaceHolder1_DataViewDelinquentPropertyTaxResults1_DataList1_lblTaxType_0")).Text;
                        string Total_amt = driver.FindElement(By.Id("ContentPlaceHolder1_DataViewDelinquentPropertyTaxResults1_DataList1_lblTotalAmountDue_0")).Text;
                        string Name_tax = driver.FindElement(By.Id("ContentPlaceHolder1_DataViewDelinquentPropertyTaxResults1_DataList1_lblTaxpayerName_0")).Text;
                        string delquenttaxfirstresult = tax_year + "~" + tax_type + "~" + Total_amt + "~" + Name_tax + "~" + propertlocation;
                        gc.insert_date(orderNumber, Parcel_number, 1044, delquenttaxfirstresult, 1, DateTime.Now);
                    }
                    catch { }
                    //Overview - Payments
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_DataViewNavigationDesktop1_ASPxMenu1_DXI0_T']")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, Parcel_number, "Delinquent Taxes", driver, "IL", "Cook");
                        string urgent_delequent = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_DataViewYourPropertyTaxOverviewResults1_DataViewYourPropertyTaxOverviewPayments1_panTaxesSoldTaxNotice']/div/div[2]/div[4]")).Text.Trim();
                        gc.insert_date(orderNumber, Parcel_number, 1056, urgent_delequent, 1, DateTime.Now);
                    }
                    catch { }
                    //Download Your Tax Bill
                    driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_DataViewNavigationDesktop1_ASPxMenu1_DXI1_T']")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_number, "Delinquent Taxes", driver, "IL", "Cook");
                    IWebElement downloadtax = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_DataViewRequestADuplicateTaxBillResults1_DataViewYourPropertyTaxOverviewTaxBills1_panCurrentTaxBill']/div/a[2]"));
                    string downloadhref = downloadtax.GetAttribute("href");
                    gc.downloadfile(downloadhref, orderNumber, Parcel_number, "ViewTaxBill.pdf", "IL", "Cook");
                    Thread.Sleep(2000);
                    //Taxing Districts' Financials
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_DataViewNavigationDesktop1_ASPxMenu1_DXI9_T']")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, Parcel_number, "Taxing Districts Financials second", driver, "IL", "Cook");
                        driver.FindElement(By.Id("ContentPlaceHolder1_DataViewTaxingDistrictsResults1_ASPxPageControl1_T0")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, Parcel_number, "Taxing Districts Financials First", driver, "IL", "Cook");

                    }
                    catch { }
                    //Delinquent Property Tax Search
                    try
                    {
                        driver.Navigate().GoToUrl("https://taxdelinquent.cookcountyclerk.com/");
                        driver.FindElement(By.Id("Pin")).SendKeys(Parcel_number);
                        gc.CreatePdf(orderNumber, Parcel_number, "Delinquent Property Tax Search", driver, "IL", "Cook");
                        driver.FindElement(By.XPath("//*[@id='collapseOne']/div[2]/form/div[3]/button")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, Parcel_number, "Delinquent Property Tax Sold", driver, "IL", "Cook");
                        try
                        {
                            string nodeliquent = driver.FindElement(By.XPath("//*[@id='collapseOne']/div[2]/form/div[1]/div")).Text.Trim();
                            if (nodeliquent.Trim() != "")
                            {
                                gc.insert_date(orderNumber, Parcel_number, 1120, nodeliquent, 1, DateTime.Now);
                            }

                        }
                        catch { }
                        try
                        {
                            IWebElement dateoftable = driver.FindElement(By.XPath("/html/body/div/div/div[1]/div[3]/div/h6"));
                            string dateof = GlobalClass.After(dateoftable.Text, "Data as of").Trim();
                            IWebElement solddelinquenttaxtable = driver.FindElement(By.XPath("//*[@id='collapseTwo']/div[1]/table/tbody"));
                            IList<IWebElement> solddelequentrow = solddelinquenttaxtable.FindElements(By.TagName("tr"));
                            IList<IWebElement> soldid;
                            foreach (IWebElement sold in solddelequentrow)
                            {
                                soldid = sold.FindElements(By.TagName("td"));
                                if (soldid.Count != 0)
                                {
                                    string soldresult = dateof + "~" + soldid[0].Text + "~" + soldid[1].Text + "~" + soldid[2].Text + "~" + soldid[3].Text + "~" + soldid[4].Text + "~" + soldid[5].Text;
                                    gc.insert_date(orderNumber, Parcel_number, 1057, soldresult, 1, DateTime.Now);
                                    dateof = "";
                                }
                            }
                        }
                        catch { }
                    }
                    catch { }
                    driver.Quit();
                    gc.mergpdf(orderNumber, "IL", "Cook");
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