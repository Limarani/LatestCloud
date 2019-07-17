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
    public class WebDriver_OHStark
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
        public string FTP_Stark(string houseno, string housedir, string sname, string unitno, string parcelNumber, string searchType, string orderNumber, string ownername, string directParcel)

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

                    driver.Navigate().GoToUrl("http://ddti.starkcountyohio.gov/disclaimer.aspx");
                    Thread.Sleep(4000);
                    driver.FindElement(By.Name("ctl00$ContentPlaceHolder1$btnDisclaimerAccept")).Click();
                    driver.FindElement(By.Name("ctl00$ContentPlaceHolder1$btnDisclaimerAccept")).Click();
                    Thread.Sleep(5000);
                    if (searchType == "titleflex")
                    {
                        string titleaddress = houseno + " " + sname + " " + housedir + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "OH", "Stark");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_OHStark"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {

                        //    Owner~Property_Address~Land_Use

                        driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_mnuSearchn2']/table/tbody/tr/td/a")).Click();
                        Thread.Sleep(3000);


                        driver.FindElement(By.Id("ContentPlaceHolder1_Address_tbAddressNumber")).SendKeys(houseno);
                        driver.FindElement(By.Id("ContentPlaceHolder1_Address_tbAddressDirection")).SendKeys(housedir);
                        driver.FindElement(By.Id("ContentPlaceHolder1_Address_tbAddressStreet")).SendKeys(sname);

                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "OH", "Stark");
                        driver.FindElement(By.Id("ContentPlaceHolder1_Address_btnSearchAddress")).SendKeys(Keys.Enter);
                        Thread.Sleep(4000);

                        gc.CreatePdf_WOP(orderNumber, "Address search result", driver, "OH", "Stark");
                        try
                        {
                            mul = driver.FindElement(By.Id("ContentPlaceHolder1_lblNumberOfResults")).Text.Trim();
                            mul = WebDriverTest.Before(mul, " Results");
                            if (mul != "1")
                            {
                                //multi parcel
                                IWebElement tbmulti = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_gvSearchResults']/tbody"));
                                IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));
                                IList<IWebElement> TDmulti;
                                foreach (IWebElement row in TRmulti)
                                {
                                    if (!row.Text.Contains("Parcel"))
                                    {
                                        TDmulti = row.FindElements(By.TagName("td"));
                                        if (TDmulti.Count != 0)
                                        {
                                            string multi1 = TDmulti[1].Text + "~" + TDmulti[2].Text + "~" + TDmulti[3].Text;
                                            gc.insert_date(orderNumber, TDmulti[0].Text, 302, multi1, 1, DateTime.Now);
                                        }
                                    }
                                }
                                HttpContext.Current.Session["multiparcel_Stark"] = "Yes";

                                driver.Quit();
                                return "MultiParcel";
                            }
                            else
                            {

                                driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_gvSearchResults']/tbody/tr[2]/td[1]/a")).Click();
                                gc.CreatePdf_WOP(orderNumber, "Address search result1", driver, "OH", "Stark");
                                Thread.Sleep(4000);
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
                        driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_mnuSearchn0']/table/tbody/tr/td/a")).Click();
                        Thread.Sleep(3000);

                        if (parcelNumber.Contains("-"))
                        {
                            parcelNumber = parcelNumber.Replace("-", "");
                        }
                        driver.FindElement(By.Id("ContentPlaceHolder1_Parcel_tbParcelNumber")).SendKeys(parcelNumber);
                        gc.CreatePdf_WOP(orderNumber, "Parcel search", driver, "OH", "Stark");
                        driver.FindElement(By.Id("ContentPlaceHolder1_Parcel_btnSearchParcel")).Click();
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search result", driver, "OH", "Stark");

                        try
                        {
                            mul = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_lblNumberOfResults']")).Text.Trim();

                            mul = WebDriverTest.Before(mul, " Results");
                            if (mul != "1")
                            {
                                //multi parcel
                                IWebElement tbmulti = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_gvSearchResults']/tbody"));
                                IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));

                                IList<IWebElement> TDmulti;
                                foreach (IWebElement row in TRmulti)
                                {
                                    if (!row.Text.Contains("Parcel"))
                                    {
                                        TDmulti = row.FindElements(By.TagName("td"));
                                        if (TDmulti.Count != 0)
                                        {
                                            string multi1 = TDmulti[1].Text + "~" + TDmulti[2].Text + "~" + TDmulti[3].Text;
                                            gc.insert_date(orderNumber, TDmulti[0].Text, 302, multi1, 1, DateTime.Now);
                                        }
                                    }
                                }
                                HttpContext.Current.Session["multiparcel_Stark"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            else
                            {
                                driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_gvSearchResults']/tbody/tr[2]/td[1]/a")).Click();
                                gc.CreatePdf(orderNumber, parcelNumber, "Parcel search result", driver, "OH", "Stark");
                                Thread.Sleep(3000);

                            }
                        }
                        catch { }
                    }
                    else if (searchType == "ownername")
                    {
                        string s = ownername;
                        string[] words = s.Split(' ');
                        string lastname = "", firstname = "";
                        try
                        {
                            lastname = words[0];
                            firstname = words[1];
                        }
                        catch { }


                        driver.FindElement(By.Id("ContentPlaceHolder1_Owner_tbOwnerLastName")).SendKeys(lastname);
                        driver.FindElement(By.Id("ContentPlaceHolder1_Owner_tbOwnerFirstName")).SendKeys(firstname);
                        gc.CreatePdf_WOP(orderNumber, "Owner search", driver, "OH", "Stark");
                        driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Owner_btnSearchOwner']")).Click();
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Owner search result", driver, "OH", "Stark");

                        try
                        {
                            mul = driver.FindElement(By.Id("ContentPlaceHolder1_lblNumberOfResults")).Text.Trim();
                            mul = WebDriverTest.Before(mul, " Results");
                            if (mul != "1")
                            {
                                //multi parcel
                                IWebElement tbmulti = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_gvSearchResults']/tbody"));
                                IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));

                                IList<IWebElement> TDmulti;
                                foreach (IWebElement row in TRmulti)
                                {
                                    if (!row.Text.Contains("Parcel"))
                                    {
                                        TDmulti = row.FindElements(By.TagName("td"));
                                        if (TDmulti.Count != 0)
                                        {
                                            string multi1 = TDmulti[1].Text + "~" + TDmulti[2].Text + "~" + TDmulti[3].Text;
                                            gc.insert_date(orderNumber, TDmulti[0].Text, 302, multi1, 1, DateTime.Now);
                                        }
                                    }
                                }
                                HttpContext.Current.Session["multiparcel_Stark"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            else
                            {
                                driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_gvSearchResults']/tbody/tr[2]/td[1]/a")).Click();
                                gc.CreatePdf_WOP(orderNumber, "Owner search result1", driver, "OH", "Stark");
                                Thread.Sleep(3000);
                            }
                        }
                        catch { }
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.Id("ContentPlaceHolder1_lblNumberOfResults"));
                        if(INodata.Text.Contains("No results"))
                        {
                            HttpContext.Current.Session["Nodata_OHStark"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    //property details

                    string Parcel_ID = "", Owner_Name = "", Property_Address = "", Property_Type = "", Year_Built = "", Legal_Description = "";
                    Parcel_ID = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataProfile_ParcelLabel")).Text.Trim();
                    Owner_Name = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataProfile_OwnerLabel")).Text.Trim();
                    Property_Address = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataProfile_AddressLabel")).Text.Trim();
                    Property_Type = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataLegal_PropertyClassLabel")).Text.Trim();
                    Legal_Description = driver.FindElement(By.Id("ContentPlaceHolder1_Base_fvDataLegal_LegalDescriptionLabel")).Text.Trim();

                    driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_mnuDatan5']/table/tbody/tr/td/a")).Click();
                    Thread.Sleep(4000);
                    gc.CreatePdf(orderNumber, Parcel_ID, "Building", driver, "OH", "Stark");
                    try
                    {
                        Year_Built = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Residential_gvDataBuildings']/tbody/tr[2]/td[5]")).Text.Trim();
                    }
                    catch { }
                    string property_details = Owner_Name + "~" + Property_Address + "~" + Property_Type + "~" + Year_Built + "~" + Legal_Description;
                    gc.insert_date(orderNumber, Parcel_ID, 303, property_details, 1, DateTime.Now);

                    //         Owner_Name~Property_Address~Property_Type~Year_Built~Legal_Description
                    //assessment details
                    driver.FindElement(By.XPath(" //*[@id='ContentPlaceHolder1_mnuDatan2']/table/tbody/tr/td/a")).Click();
                    gc.CreatePdf(orderNumber, Parcel_ID, "Assessment", driver, "OH", "Stark");
                    Thread.Sleep(4000);
                    IWebElement multitableElement11 = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Valuation_gvDataValuation']/tbody"));
                    IList<IWebElement> multitableRow11 = multitableElement11.FindElements(By.TagName("tr"));
                    int iRowsCount = driver.FindElements(By.XPath("//*[@id='ContentPlaceHolder1_Valuation_gvDataValuation']/tbody/tr")).Count;
                    IList<IWebElement> multirowTD11;
                    int i = 0;

                    foreach (IWebElement row in multitableRow11)
                    {

                        if (!row.Text.Contains("Year"))
                        {

                            multirowTD11 = row.FindElements(By.TagName("td"));
                            if (multirowTD11.Count != 1 && multirowTD11[0].Text.Trim() != "")
                            {

                                //Year~Appraised_Land_Value~Assessed_Land_Value~Appraised_Building_Value~Assessed_Building_Value~Appraised_Total_Value~Assessed_Total_Value~Override
                                string assessment_details = multirowTD11[0].Text.Trim() + "~" + multirowTD11[1].Text.Trim() + "~" + multirowTD11[2].Text.Trim() + "~" + multirowTD11[3].Text.Trim() + "~" + multirowTD11[4].Text.Trim() + "~" + multirowTD11[5].Text.Trim() + "~" + multirowTD11[6].Text.Trim() + "~" + multirowTD11[7].Text.Trim();
                                gc.insert_date(orderNumber, Parcel_ID, 304, assessment_details, 1, DateTime.Now);
                                i++;
                            }
                            if (i == 3)
                            { break; }
                        }



                    }
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");


                    //tax details

                    driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_mnuDatan4']/table/tbody/tr/td/a")).Click();
                    gc.CreatePdf(orderNumber, Parcel_ID, "Tax details", driver, "OH", "Stark");

                    Thread.Sleep(3000);

                    string taxdet = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Tax_fvDataTax']/tbody/tr/td/table/tbody")).Text.Trim();

                    string Bill_Number = gc.Between(taxdet, "Bill Number: ", "Installment Number:").Trim();
                    string Owner_Occupancy_Discount = driver.FindElement(By.XPath(" //*[@id='ContentPlaceHolder1_Tax_fvDataTax_ResidentialDiscountLabel']")).Text.Trim();

                    string CAUV_Recoupment = gc.Between(taxdet, "CAUV Recoupment:", "Recoupment Amount:").Trim();
                    string Homestead_Deduction = gc.Between(taxdet, "Homestead Deduction:", "Tax Abatement:").Trim();

                    string Homestead_Reduction = gc.Between(taxdet, "Homestead Reduction:", "CAUV Recoupment:").Trim();
                    string Recoupment_Amount = gc.Between(taxdet, "Recoupment Amount:", "Homestead Deduction:").Trim();
                    string Tax_Abatement = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Tax_fvDataTax_TaxAbatementLabel']")).Text.Trim();
                    string Property_Destruction = gc.Between(taxdet, "Property Destruction:", "Homestead Deduction Year:").Trim();
                    string Agricultural_Use_Value = gc.Between(taxdet, "Agricultural Use Value:", "Tax Year:").Trim();
                    string tax_year = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Tax_fvDataTax_TaxYearLabel']")).Text.Trim();

                    string tax_info_det = Bill_Number + "~" + Owner_Occupancy_Discount + "~" + CAUV_Recoupment + "~" + Homestead_Deduction + "~" + Homestead_Reduction + "~" + Recoupment_Amount + "~" + Tax_Abatement + "~" + Property_Destruction + "~" + Agricultural_Use_Value + "~" + tax_year;
                    gc.insert_date(orderNumber, Parcel_ID, 390, tax_info_det, 1, DateTime.Now);

                    //      Bill_Number~Owner_Occupancy_Discount~CAUV_Recoupment~Homestead_Deduction~Homestead_Reduction~Recoupment_Amount~Tax_Abatement~Property_Destruction~Agricultural_Use_Value~tax_year


                    IWebElement multitableElement2 = driver.FindElement(By.XPath(" //*[@id='ContentPlaceHolder1_Tax_gvDataTaxBilling']/tbody"));
                    IList<IWebElement> multitableRow2 = multitableElement2.FindElements(By.TagName("tr"));

                    IList<IWebElement> multirowTD2;
                    foreach (IWebElement row in multitableRow2)
                    {
                        if (!row.Text.Contains("Section"))
                        {
                            multirowTD2 = row.FindElements(By.TagName("td"));
                            if (multirowTD2.Count != 1 && multirowTD2[1].Text.Trim() != "")
                            {
                                string tax_info = multirowTD2[0].Text.Trim() + "~" + multirowTD2[1].Text.Trim() + "~" + multirowTD2[2].Text.Trim() + "~" + multirowTD2[3].Text.Trim() + "~" + multirowTD2[4].Text.Trim() + "~" + multirowTD2[5].Text.Trim();
                                gc.insert_date(orderNumber, Parcel_ID, 305, tax_info, 1, DateTime.Now);
                                //Section~Label~Tax_Amount~Paid_Amount~Due_Amount~Deliquent_Due
                            }
                        }

                    }



                    //tax payment


                    IWebElement multitableElement3 = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_Tax_gvDataTaxPayments']/tbody"));
                    IList<IWebElement> multitableRow3 = multitableElement3.FindElements(By.TagName("tr"));

                    IList<IWebElement> multirowTD3;
                    foreach (IWebElement row in multitableRow3)
                    {
                        if (!row.Text.Contains("Payment Date"))
                        {
                            multirowTD3 = row.FindElements(By.TagName("td"));
                            if (multirowTD3.Count != 1 && multirowTD3[0].Text.Trim() != "")
                            {
                                string tax_payment = multirowTD3[0].Text.Trim() + "~" + multirowTD3[1].Text.Trim() + "~" + multirowTD3[2].Text.Trim();
                                gc.insert_date(orderNumber, Parcel_ID, 306, tax_payment, 1, DateTime.Now);

                                //Payment_Date~Installment~Paid_Amount
                            }
                        }

                    }

                    //Special Assessments Table:
                    IWebElement multitableElement4 = driver.FindElement(By.XPath(" //*[@id='ContentPlaceHolder1_Tax_gvSpecials']/tbody"));
                    IList<IWebElement> multitableRow4 = multitableElement4.FindElements(By.TagName("tr"));

                    IList<IWebElement> multirowTD4;
                    foreach (IWebElement row in multitableRow4)
                    {
                        if (!row.Text.Contains("Agency"))
                        {
                            multirowTD4 = row.FindElements(By.TagName("td"));
                            if (multirowTD4.Count != 1)
                            {
                                string special_assessment = multirowTD4[0].Text.Trim() + "~" + multirowTD4[1].Text.Trim() + "~" + multirowTD4[2].Text.Trim() + "~" + multirowTD4[3].Text.Trim() + "~" + multirowTD4[4].Text.Trim() + "~" + multirowTD4[5].Text.Trim();
                                gc.insert_date(orderNumber, Parcel_ID, 307, special_assessment, 1, DateTime.Now);
                                //Agency~Code~Amount~Status~Type~Due
                            }
                        }

                    }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "OH", "Stark", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "OH", "Stark");
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