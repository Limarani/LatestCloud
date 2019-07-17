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
    public class WebDriver_CODenver
    {
        Amrock amck = new Amrock();
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        private object driverIE;
        private int multicount;
        private string strMultiAddress;
        private string b;
        string mul = "";
        public string FTP_Denver(string address, string ownername,  string parcelNumber, string searchType, string orderNumber, string directParcel)
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

                    driver.Navigate().GoToUrl("https://www.denvergov.org/property");
                    Thread.Sleep(4000);


                    if (searchType == "titleflex")
                    {

                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "CO", "Denver");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_CODenver"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {

                        driver.FindElement(By.XPath("//*[@id='search']")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "CO", "Denver");
                        driver.FindElement(By.Id("btnSearch")).Click();
                        Thread.Sleep(3000);
                        try
                        {
                            int iRowsCount = driver.FindElements(By.XPath("//*[@id='results_table']/tbody/tr")).Count;

                            gc.CreatePdf_WOP(orderNumber, "Address search result", driver, "CO", "Denver");

                            if (iRowsCount >= 3)
                            {
                                //multi parcel
                                IWebElement tbmulti2 = driver.FindElement(By.XPath("//*[@id='results_table']/tbody"));
                                IList<IWebElement> TRmulti2 = tbmulti2.FindElements(By.TagName("tr"));
                                IList<IWebElement> TDmulti2;
                                foreach (IWebElement row in TRmulti2)
                                {
                                    TDmulti2 = row.FindElements(By.TagName("td"));
                                    if (TDmulti2.Count != 0)
                                    {
                                        string multi1 = TDmulti2[0].Text + "~" + TDmulti2[2].Text;
                                        gc.insert_date(orderNumber, TDmulti2[1].Text, 316, multi1, 1, DateTime.Now);
                                        //  address~Owner
                                    }
                                }
                                HttpContext.Current.Session["multiParcel_Denver"] = "Yes";

                                driver.Quit();
                                return "MultiParcel";
                            }
                            else
                            {

                                driver.FindElement(By.XPath("//*[@id='results_table']/tbody/tr[2]/td[1]/a")).Click();
                                Thread.Sleep(3000);

                            }
                        }
                        catch { }
                    }
                    else if (searchType == "parcel")
                    {
                        if (parcelNumber.Contains("-"))
                        {
                            parcelNumber = parcelNumber.Replace("-", "");
                        }
                        driver.FindElement(By.XPath("//*[@id='search']")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search", driver, "CO", "Denver");
                        driver.FindElement(By.Id("btnSearch")).Click();
                        Thread.Sleep(3000);
                        try
                        {
                            int iRowsCount = driver.FindElements(By.XPath("//*[@id='results_table']/tbody/tr")).Count;


                            gc.CreatePdf(orderNumber, parcelNumber, "Parcel search result", driver, "CO", "Denver");
                            if (iRowsCount >= 3)
                            {
                                //multi parcel
                                IWebElement tbmulti1 = driver.FindElement(By.XPath("//*[@id='results_table']/tbody"));
                                IList<IWebElement> TRmulti1 = tbmulti1.FindElements(By.TagName("tr"));
                                IList<IWebElement> TDmulti1;
                                foreach (IWebElement row in TRmulti1)
                                {
                                    TDmulti1 = row.FindElements(By.TagName("td"));
                                    if (TDmulti1.Count != 0)
                                    {
                                        string multi1 = TDmulti1[0].Text + "~" + TDmulti1[2].Text;
                                        gc.insert_date(orderNumber, TDmulti1[1].Text, 316, multi1, 1, DateTime.Now);
                                    }
                                }
                                HttpContext.Current.Session["multiParcel_Denver"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            else
                            {

                                driver.FindElement(By.XPath("//*[@id='results_table']/tbody/tr[2]/td[1]/a")).Click();
                                Thread.Sleep(3000);

                            }
                        }
                        catch { }
                    }
                    try
                    {
                        IWebElement INodata = driver.FindElement(By.Id("no_results_div"));
                        if(INodata.Text.Contains("No properties found"))
                        {
                            HttpContext.Current.Session["Nodata_CODenver"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    //property details

                    string owner_Address = "", parcel_no = "", Legal_desc = "", Property_type = "", Tax_district = "", Year_built = "";
                    //*[@id="property-info-bar"]/tbody
                    Year_built = driver.FindElement(By.XPath("//*[@id='property_summary']/div[2]/div[2]/table/tbody/tr[3]/td[2]")).Text;
                    IWebElement tbmulti = driver.FindElement(By.XPath(" //*[@id='property-info-bar']/tbody"));
                    IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDmulti;
                    foreach (IWebElement row in TRmulti)
                    {
                        TDmulti = row.FindElements(By.TagName("td"));
                        if (TDmulti.Count != 0)
                        {
                            //*[@id="property-info-bar"]/tbody/tr[2]/td[3]/span
                            //   parcel_no = driver.FindElement(By.XPath("//*[@id='property-info-bar']/tbody/tr[2]/td[3]/span")).Text;
                            parcel_no = TDmulti[2].Text;
                            //if (parcel_no.Contains("-"))
                            //{
                            //    parcel_no = parcel_no.Replace("-", "");
                            //}
                            owner_Address = TDmulti[0].Text.Replace("\r\n", " ");
                            string property_details = owner_Address + "~" + TDmulti[3].Text + "~" + TDmulti[4].Text + "~" + TDmulti[5].Text + "~" + Year_built;
                            gc.insert_date(orderNumber, parcel_no, 313, property_details, 1, DateTime.Now);

                            //     owner_Address~Legal_Description~Property_Type~Tax_District~Year_built
                        }
                    }
                    amck.TaxId = parcel_no;
                    //assessment details
                    gc.CreatePdf(orderNumber, parcel_no, "Property", driver, "CO", "Denver");
                    driver.FindElement(By.XPath(" //*[@id='Assessment']/a")).Click();
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, parcel_no, "Assessment", driver, "CO", "Denver");
                    string land_actual = "", land_assessed = "", land_exempt = "", improvements_actual = "", improvements_assessed = "", improvements_exempt = "", total_actual = "", total_assessed = "", total_exempt = "";

                    string current_year = driver.FindElement(By.XPath("//*[@id='assessment_data']/div/div/div[1]/div[1]/h4")).Text;
                    land_actual = driver.FindElement(By.XPath(" //*[@id='assessment_data']/div/div/div[1]/div[2]/table/tbody/tr[1]/td[2]")).Text;
                    land_assessed = driver.FindElement(By.XPath(" //*[@id='assessment_data']/div/div/div[1]/div[2]/table/tbody/tr[1]/td[3]")).Text;
                    land_exempt = driver.FindElement(By.XPath(" //*[@id='assessment_data']/div/div/div[1]/div[2]/table/tbody/tr[1]/td[4]")).Text;

                    improvements_actual = driver.FindElement(By.XPath(" //*[@id='assessment_data']/div/div/div[1]/div[2]/table/tbody/tr[2]/td[2]")).Text;
                    improvements_assessed = driver.FindElement(By.XPath(" //*[@id='assessment_data']/div/div/div[1]/div[2]/table/tbody/tr[2]/td[3]")).Text;
                    improvements_exempt = driver.FindElement(By.XPath(" //*[@id='assessment_data']/div/div/div[1]/div[2]/table/tbody/tr[2]/td[4]")).Text;

                    total_actual = driver.FindElement(By.XPath(" //*[@id='assessment_data']/div/div/div[1]/div[2]/table/tbody/tr[3]/td[2]")).Text;
                    total_assessed = driver.FindElement(By.XPath(" //*[@id='assessment_data']/div/div/div[1]/div[2]/table/tbody/tr[3]/td[3]")).Text;
                    total_exempt = driver.FindElement(By.XPath(" //*[@id='assessment_data']/div/div/div[1]/div[2]/table/tbody/tr[3]/td[4]")).Text;

                    string assessment_details = current_year + "~" + land_actual + "~" + land_assessed + "~" + land_exempt + "~" + improvements_actual + "~" + improvements_assessed + "~" + improvements_exempt + "~" + total_actual + "~" + total_assessed + "~" + total_exempt;
                    gc.insert_date(orderNumber, parcel_no, 314, assessment_details, 1, DateTime.Now);


                    string prior_year = driver.FindElement(By.XPath("//*[@id='assessment_data']/div/div/div[2]/div[1]/h4")).Text;

                    land_actual = driver.FindElement(By.XPath(" //*[@id='assessment_data']/div/div/div[2]/div[2]/table/tbody/tr[1]/td[2]")).Text;
                    land_assessed = driver.FindElement(By.XPath(" //*[@id='assessment_data']/div/div/div[2]/div[2]/table/tbody/tr[1]/td[3]")).Text;
                    land_exempt = driver.FindElement(By.XPath(" //*[@id='assessment_data']/div/div/div[2]/div[2]/table/tbody/tr[1]/td[4]")).Text;

                    improvements_actual = driver.FindElement(By.XPath(" //*[@id='assessment_data']/div/div/div[2]/div[2]/table/tbody/tr[2]/td[2]")).Text;
                    improvements_assessed = driver.FindElement(By.XPath(" //*[@id='assessment_data']/div/div/div[2]/div[2]/table/tbody/tr[2]/td[3]")).Text;
                    improvements_exempt = driver.FindElement(By.XPath(" //*[@id='assessment_data']/div/div/div[2]/div[2]/table/tbody/tr[2]/td[4]")).Text;

                    total_actual = driver.FindElement(By.XPath(" //*[@id='assessment_data']/div/div/div[2]/div[2]/table/tbody/tr[3]/td[2]")).Text;
                    total_assessed = driver.FindElement(By.XPath(" //*[@id='assessment_data']/div/div/div[2]/div[2]/table/tbody/tr[3]/td[3]")).Text;
                    total_exempt = driver.FindElement(By.XPath(" //*[@id='assessment_data']/div/div/div[2]/div[2]/table/tbody/tr[3]/td[4]")).Text;

                    string assessment_details1 = prior_year + "~" + land_actual + "~" + land_assessed + "~" + land_exempt + "~" + improvements_actual + "~" + improvements_assessed + "~" + improvements_exempt + "~" + total_actual + "~" + total_assessed + "~" + total_exempt;
                    gc.insert_date(orderNumber, parcel_no, 314, assessment_details1, 1, DateTime.Now);

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");




                    //Actual_land~Assessed_Land~Exempt_Land~Actual_Improvements~Assessed_Improvements~Exempt_Improvements~Actual_total~Assessed_total~Exempt_total
                    //Tax info

                    driver.FindElement(By.XPath("//*[@id='Taxes']/a")).Click();
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, parcel_no, "Tax info", driver, "CO", "Denver");

                    IWebElement valuetableElement = driver.FindElement(By.XPath("//*[@id='TaxesTable']/tbody[2]"));

                    IList<IWebElement> valuetableRow = valuetableElement.FindElements(By.TagName("tr"));
                    IList<IWebElement> valuerowTD;
                    List<string> date_paid = new List<string>();
                    List<string> Original_Tax_Levy = new List<string>();
                    List<string> Liens_Fees = new List<string>();
                    List<string> Interest = new List<string>();
                    List<string> Paid = new List<string>();
                    List<string> Due = new List<string>();
                    string due1 = "", due2 = "",lienfees="",interest="",due="";
                    int i = 0;
                    foreach (IWebElement row in valuetableRow)
                    {
                        valuerowTD = row.FindElements(By.TagName("td"));
                        if (valuerowTD.Count != 0)
                        {
                            if (i == 0)
                            {

                                date_paid.Add(valuerowTD[1].Text);
                                date_paid.Add(valuerowTD[2].Text);
                                date_paid.Add(valuerowTD[3].Text);

                            }
                            else if (i == 1)
                            {
                                Original_Tax_Levy.Add(valuerowTD[1].Text);
                                amck.Instamount1 = valuerowTD[1].Text;
                                Original_Tax_Levy.Add(valuerowTD[2].Text);
                                amck.Instamount2 = valuerowTD[2].Text;
                                Original_Tax_Levy.Add(valuerowTD[3].Text);
                            }
                            else if (i == 2)
                            {
                                Liens_Fees.Add(valuerowTD[1].Text);
                                Liens_Fees.Add(valuerowTD[2].Text);
                                Liens_Fees.Add(valuerowTD[3].Text);
                                lienfees = valuerowTD[3].Text;
                                

                            }
                            else if (i == 3)
                            {
                                Interest.Add(valuerowTD[1].Text);
                                Interest.Add(valuerowTD[2].Text);
                                Interest.Add(valuerowTD[3].Text);
                                interest = valuerowTD[3].Text;
                                
                            }
                            else if (i == 4)
                            {
                                Paid.Add(valuerowTD[1].Text);
                                amck.Instamountpaid1 = valuerowTD[1].Text;
                                Paid.Add(valuerowTD[2].Text);
                                amck.Instamountpaid2 = valuerowTD[2].Text;
                                Paid.Add(valuerowTD[3].Text);
                            }
                            else if (i == 5)
                            {
                                Due.Add(valuerowTD[1].Text);
                                due1 = valuerowTD[1].Text;
                                if(due1=="$0.00")
                                {
                                    amck.InstPaidDue1 = "Paid";
                                }
                                else
                                {
                                    amck.InstPaidDue1 = "Due";
                                }
                                Due.Add(valuerowTD[2].Text);
                                due2 = valuerowTD[2].Text;
                                if (due2 == "$0.00")
                                {
                                    amck.InstPaidDue2 = "Paid";
                                }
                                else
                                {
                                    amck.InstPaidDue2 = "Due";
                                }
                                Due.Add(valuerowTD[3].Text);
                                due = valuerowTD[3].Text;
                            }

                        }
                        i++;
                    }
                    if (interest == "$0.00" && lienfees == "$0.00")
                    {
                        amck.IsDelinquent = "No";
                    }
                    else
                    {
                        if(due== "$0.00")
                        {
                            amck.IsDelinquent = "No";
                        }
                        else
                        {
                            amck.IsDelinquent = "Yes";

                        }
                       
                    }
                    if (amck.IsDelinquent != "Yes")
                    {
                        gc.InsertAmrockTax(orderNumber, amck.TaxId, amck.Instamount1, amck.Instamount2, amck.Instamount3, amck.Instamount4, amck.Instamountpaid1, amck.Instamountpaid2, amck.Instamountpaid3, amck.Instamountpaid4, amck.InstPaidDue1, amck.InstPaidDue2, amck.instPaidDue3, amck.instPaidDue4, amck.IsDelinquent);
                    }
                    else
                    {
                        gc.InsertAmrockTax(orderNumber, amck.TaxId, null, null, null, null, null, null, null, null, null, null, null, null, amck.IsDelinquent);

                    }
                    //*[@id="TaxesTable"]/tbody[1]/tr/th[2]
                    //*[@id="TaxesTable"]/tbody[1]/tr/th[3]
                    //*[@id="TaxesTable"]/tbody[1]/tr/th[4]
                    string instal1 = driver.FindElement(By.XPath("//*[@id='TaxesTable']/tbody[1]/tr/th[2] ")).Text;
                    string instal2 = driver.FindElement(By.XPath("//*[@id='TaxesTable']/tbody[1]/tr/th[3] ")).Text;
                    string instal3 = driver.FindElement(By.XPath("//*[@id='TaxesTable']/tbody[1]/tr/th[4] ")).Text;

                    string tax1 = instal1 + "~" + date_paid[0] + "~" + Original_Tax_Levy[0] + "~" + Liens_Fees[0] + "~" + Interest[0] + "~" + Paid[0] + "~" + Due[0];
                    string tax2 = instal2 + "~" + date_paid[1] + "~" + Original_Tax_Levy[1] + "~" + Liens_Fees[1] + "~" + Interest[1] + "~" + Paid[1] + "~" + Due[1];
                    string tax3 = instal3 + "~" + date_paid[2] + "~" + Original_Tax_Levy[2] + "~" + Liens_Fees[2] + "~" + Interest[2] + "~" + Paid[2] + "~" + Due[2];

                    gc.insert_date(orderNumber, parcel_no, 315, tax1, 1, DateTime.Now);
                    gc.insert_date(orderNumber, parcel_no, 315, tax2, 1, DateTime.Now);
                    gc.insert_date(orderNumber, parcel_no, 315, tax3, 1, DateTime.Now);

                    //  date_paid~Original_Tax_Levy~Liens_Fees~Interest~Paid~Due

                    //Additional information

                    driver.FindElement(By.XPath("//*[@id='accordion']/div[2]/div[1]/h4/a")).Click();
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, parcel_no, "Additional info", driver, "CO", "Denver");



                    string Additional_Assessment = "", Additional_Owner = "", Adjustments = "", Local_Improvement_Assessment = "", Maintenance_District = "", Pending_Local_Improvement = "", Prior_Year_Delinquency = "", Scheduled_to_be_Paid_by_Mortgage_Company = "", Sewer_Storm_Drainage_Liens = "", Tax_Lien_Sale = "", Treasurer_Deed = "";

                    Additional_Assessment = driver.FindElement(By.XPath("//*[@id='collapseTwo']/div/div[2]/table/tbody/tr[1]/td[2]")).Text;
                    Additional_Owner = driver.FindElement(By.XPath("//*[@id='collapseTwo']/div/div[2]/table/tbody/tr[2]/td[2]")).Text;
                    Adjustments = driver.FindElement(By.XPath("//*[@id='collapseTwo']/div/div[2]/table/tbody/tr[3]/td[2]")).Text;
                    Local_Improvement_Assessment = driver.FindElement(By.XPath("//*[@id='collapseTwo']/div/div[2]/table/tbody/tr[4]/td[2]")).Text;
                    Maintenance_District = driver.FindElement(By.XPath("//*[@id='collapseTwo']/div/div[2]/table/tbody/tr[5]/td[2]")).Text;
                    Pending_Local_Improvement = driver.FindElement(By.XPath("//*[@id='collapseTwo']/div/div[2]/table/tbody/tr[6]/td[2]")).Text;
                    Prior_Year_Delinquency = driver.FindElement(By.XPath("//*[@id='collapseTwo']/div/div[2]/table/tbody/tr[1]/td[4]")).Text;
                    Scheduled_to_be_Paid_by_Mortgage_Company = driver.FindElement(By.XPath("//*[@id='collapseTwo']/div/div[2]/table/tbody/tr[2]/td[4]")).Text;
                    Sewer_Storm_Drainage_Liens = driver.FindElement(By.XPath("//*[@id='collapseTwo']/div/div[2]/table/tbody/tr[3]/td[4]")).Text;
                    Tax_Lien_Sale = driver.FindElement(By.XPath("//*[@id='collapseTwo']/div/div[2]/table/tbody/tr[4]/td[4]")).Text;
                    Treasurer_Deed = driver.FindElement(By.XPath("//*[@id='collapseTwo']/div/div[2]/table/tbody/tr[5]/td[4]")).Text;

                    string tax = driver.FindElement(By.XPath("//*[@id='collapseTwo']/div/div[3]/div/strong")).Text;
                    //Additional_Assessment~Additional_Owner~Adjustments~Local_Improvement_Assessment~Maintenance_District~Pending_Local_Improvement~Prior_Year_Delinquency~Scheduled_to_be_Paid_by_Mortgage_Company~Sewer_Storm_Drainage_Liens~Tax_Lien_Sale~Treasurer_Deed
                    string additional_info = tax + "~" + Additional_Assessment + "~" + Additional_Owner + "~" + Adjustments + "~" + Local_Improvement_Assessment + "~" + Maintenance_District + "~" + Pending_Local_Improvement + "~" + Prior_Year_Delinquency + "~" + Scheduled_to_be_Paid_by_Mortgage_Company + "~" + Sewer_Storm_Drainage_Liens + "~" + Tax_Lien_Sale + "~" + Treasurer_Deed;
                    gc.insert_date(orderNumber, parcel_no, 317, additional_info, 1, DateTime.Now);

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "CO", "Denver", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "CO", "Denver");
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