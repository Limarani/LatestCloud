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
    public class WebDriver_INMarion
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        string outputpath = "-";
        string Tax_Year = "-", Account_Number = "-", Owner_Name = "-", Property_Address = "-", Due_Date = "-", Amount = "-", Invoice_Number = "-", Tax_Details = "-";
        public string FTP_INMarion(string address, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                try
                {

                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, "", ownername, address, "IN", "Marion");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_INMarion"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }

                    if (searchType == "parcel")
                    {
                        if (GlobalClass.titleparcel != "")
                        {
                            parcelNumber = GlobalClass.titleparcel;
                        }

                        driver.Navigate().GoToUrl("http://maps.indy.gov/AssessorPropertyCards/");
                        Thread.Sleep(4000);

                        driver.FindElement(By.Id("dojox_mobile_Button_0")).Click();
                        Thread.Sleep(4000);

                        driver.FindElement(By.Id("ParcelNumberListItem")).Click();
                        Thread.Sleep(4000);
                        IList<IWebElement> inputParcel = driver.FindElements(By.TagName("input"));

                        foreach (IWebElement Parcel in inputParcel)
                        {

                            string id = Parcel.GetAttribute("id");
                            if (id == "parcelNumberTextBox")
                            {
                                Parcel.SendKeys(parcelNumber);
                                break;
                            }

                        }

                        gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearch", driver, "IN", "Marion");
                        Thread.Sleep(4000);
                        driver.FindElement(By.Id("parcelNumberButton")).Click();
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearch11", driver, "IN", "Marion");
                        try
                        {
                            IWebElement INodata = driver.FindElement(By.Id("dojox_mobile_Pane_0"));
                            if (INodata.Text.Contains("No Results"))
                            {
                                HttpContext.Current.Session["Nodata_INMarion"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }

                        //Tax Information

                        driver.Navigate().GoToUrl("https://www.invoicecloud.com/portal/(S(gpe11qcgppiihpmzc5g2kcx5))/2/customerlocator.aspx?iti=8&bg=3a912998-0175-4640-a401-47fabc0fc06b&vsii=1");
                        Thread.Sleep(4000);
                        driver.FindElement(By.Id("ctl00_ctl00_cphBody_cphBodyLeft_rptInputs_ctl01_txtValue")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "TaxInfoSearch", driver, "IN", "Marion");
                        driver.FindElement(By.XPath("//*[@id='ctl00_ctl00_cphBody_cphBodyLeft_btnSearch']")).Click();
                        Thread.Sleep(4000);

                        try
                        {
                            IWebElement TaxTable = driver.FindElement(By.XPath("/html/body/div[2]/form/table/tbody/tr/td[1]/div[6]/div/div/table/tbody"));
                            IList<IWebElement> TaxTR = TaxTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> TaxTD;
                            foreach (IWebElement Tax in TaxTR)
                            {
                                TaxTD = Tax.FindElements(By.TagName("td"));
                                if (TaxTD.Count != 0 && TaxTD.Count != 1)
                                {
                                    Tax_Year = TaxTD[2].GetAttribute("textContent");
                                    Account_Number = TaxTD[3].GetAttribute("textContent");
                                    Owner_Name = TaxTD[4].GetAttribute("textContent");
                                    Property_Address = TaxTD[5].GetAttribute("textContent");
                                    Due_Date = TaxTD[6].GetAttribute("textContent");
                                    Amount = TaxTD[7].GetAttribute("textContent");
                                    Invoice_Number = TaxTD[8].GetAttribute("textContent");

                                    Tax_Details = Tax_Year + "~" + Account_Number + "~" + Owner_Name + "~" + Property_Address + "~" + Due_Date + "~" + Amount + "~" + Invoice_Number;
                                    gc.insert_date(orderNumber, parcelNumber, 299, Tax_Details, 1, DateTime.Now);
                                }
                            }
                            //View Invoice Download
                            IWebElement ViewInvoice = driver.FindElement(By.XPath("/html/body/div[2]/form/table/tbody/tr/td[1]/div[6]/div/div/table/tbody/tr[1]/td[10]/a[1]"));
                            string ViewTaxInvoice = ViewInvoice.GetAttribute("href");
                            gc.downloadfile(ViewTaxInvoice, orderNumber, parcelNumber, "View_Invoice.pdf", "IN", "Marion");
                        }
                        catch
                        { }
                    }


                    driver.Quit();
                    //megrge pdf files
                    gc.mergpdf(orderNumber, "IN", "Marion");
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