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
using System.Web.UI;

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_CASan_Diego
    {
        string Parcel_No = "",titleaddress = "", Tax_authority = "", TaxAuthotity_details = "", Tax_Year = "", Current_Owner = "", Tax_Type = "", Secured_Details = "", Installment = "", Installment_Amount = "", Deliquent_After = "", Status = "", Amount_Due = "", Total_Due = "", Secured_Details1 = "", Installment1 = "", Installment_Amount1 = "", Deliquent_After1 = "", Status1 = "", Amount_Due1 = "", Total_Due1 = "", Secured_Details2 = "";
        string Tax_Type1 = "", Supplemnt_details = "", Value_Exceptions = "", Land = "", Improvemnts = "", Total = "", Supplemnt_details2 = "", Value_Exceptions1 = "", Land1 = "", Supplemnt_details1 = "";
        string Assessment_details = "", Value_Exemp = "", Land_Desrip = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_CASanDiego(string address, string assessment_id, string parcelNumber, string searchType, string orderNumber, string directParcel, string ownername)
        {
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
                        titleaddress = address;
                        gc.TitleFlexSearch(orderNumber, parcelNumber, ownername, titleaddress, "CA", "San Diego");

                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_CASanDiego"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "parcel")
                    {
                        ParcelSearch(orderNumber, parcelNumber);

                        try
                        {
                            IWebElement Securedtable = driver.FindElement(By.XPath("//*[@id='PaymentApplicationContent_gvSecured']/tbody"));
                            IList<IWebElement> SecuredRow = Securedtable.FindElements(By.TagName("tr"));
                            IList<IWebElement> SecuredTD;
                            int Securedcount = SecuredRow.Count;
                            List<string> strSecured = new List<string>();

                            foreach (IWebElement Secured in SecuredRow)
                            {
                                SecuredTD = Secured.FindElements(By.TagName("td"));
                                if (SecuredTD.Count != 0 && SecuredTD.Count == 13 && Secured.Text.Contains("View Bill"))
                                {
                                    strSecured.Add("https://iwr.sdtreastax.com/SanDiegoTTCPaymentApplication/SecuredDetails.aspx?parcelNumber=" + SecuredTD[1].Text.Trim().Replace("-", ""));
                                }
                            }
                            foreach (string SecuredURL in strSecured)
                            {
                                driver.Navigate().GoToUrl(SecuredURL);
                                securedDetais(orderNumber, parcelNumber);
                                try
                                {
                                    IWebElement AssessmentTB = driver.FindElement(By.XPath("//*[@id='bp7']/div[2]/table/tbody"));
                                    IList<IWebElement> AssessmentTR = AssessmentTB.FindElements(By.TagName("tr"));
                                    IList<IWebElement> AssessmentTD;

                                    foreach (IWebElement Assessment in AssessmentTR)
                                    {
                                        AssessmentTD = Assessment.FindElements(By.TagName("td"));
                                        if (AssessmentTD.Count != 0)
                                        {
                                            Land_Desrip = AssessmentTD[0].Text;
                                            Value_Exemp = AssessmentTD[2].Text;

                                            Assessment_details = Land_Desrip + "~" + Value_Exemp;
                                            gc.insert_date(orderNumber, Parcel_No, 958, Assessment_details, 1, DateTime.Now);
                                        }
                                    }
                                }
                                catch
                                { }
                                try
                                {
                                    Tax_authority = driver.FindElement(By.XPath("//*[@id='PaymentApplicationContent_footer']/div[2]")).Text;
                                    Tax_authority = WebDriverTest.After(Tax_authority, "payment to:");
                                    TaxAuthotity_details = Tax_authority;
                                    gc.insert_date(orderNumber, Parcel_No, 955, TaxAuthotity_details, 1, DateTime.Now);

                                }
                                catch
                                { }
                                driver.FindElement(By.Id("PaymentApplicationContent_btnSearchResults")).SendKeys(Keys.Enter);
                                Thread.Sleep(2000);
                            }
                        }
                        catch
                        { }

                        AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                        try
                        {
                            IWebElement Supplementaltable = driver.FindElement(By.XPath("//*[@id='PaymentApplicationContent_gvSupplemental']/tbody"));
                            IList<IWebElement> SupplementalRow = Supplementaltable.FindElements(By.TagName("tr"));
                            IList<IWebElement> SupplementalTD;
                            int rowcount = SupplementalRow.Count;
                            List<string> strSupplemental = new List<string>();

                            foreach (IWebElement Supplemental in SupplementalRow)
                            {
                                SupplementalTD = Supplemental.FindElements(By.TagName("td"));
                                if (SupplementalTD.Count != 0 && SupplementalTD.Count == 13 && Supplemental.Text.Contains("View Bill"))
                                {
                                    strSupplemental.Add("https://iwr.sdtreastax.com/SanDiegoTTCPaymentApplication/SupplementalDetails.aspx?parcelNumber=" + SupplementalTD[1].Text.Trim().Replace("-", ""));
                                }
                            }
                            foreach (string SupplementalURL in strSupplemental)
                            {
                                driver.Navigate().GoToUrl(SupplementalURL);
                                securedDetais(orderNumber, parcelNumber);
                                ExcemptionDetais(orderNumber, parcelNumber);
                            }
                        }
                        catch
                        { }
                    }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "CA", "San Diego", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "CA", "San Diego");
                    return "Data Inserted Successfully";
                }
                catch (Exception ex)
                {
                    driver.Quit();
                    GlobalClass.LogError(ex, orderNumber);
                    throw;
                }
            }
        }

        public void securedDetais(string orderNumber, string parcelNumber)
        {
            try
            {
                Tax_Year = driver.FindElement(By.Id("PaymentApplicationContent_divTaxBillYear")).Text;
                Current_Owner = driver.FindElement(By.Id("CurrentOwner")).Text;
                Current_Owner = WebDriverTest.After(Current_Owner, "OWNER");
                Tax_Type = driver.FindElement(By.Id("display-section-header")).Text;

                Secured_Details = Current_Owner + "~" + Tax_Year + "~" + Tax_Type + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                gc.insert_date(orderNumber, parcelNumber, 952, Secured_Details, 1, DateTime.Now);

                IWebElement TaxHistoryTB = driver.FindElement(By.XPath("//*[@id='PaymentApplicationContent_dataTableSecured']/tbody"));
                IList<IWebElement> TaxHistoryTR = TaxHistoryTB.FindElements(By.TagName("tr"));
                IList<IWebElement> TaxHistoryTD;

                foreach (IWebElement TaxHistory in TaxHistoryTR)
                {
                    TaxHistoryTD = TaxHistory.FindElements(By.TagName("td"));
                    if (TaxHistoryTD.Count != 0 && !TaxHistory.Text.Contains("Parcel Number") && TaxHistoryTD.Count != 6)
                    {
                        Parcel_No = TaxHistoryTD[0].Text;
                        Installment = TaxHistoryTD[1].Text;
                        Installment_Amount = TaxHistoryTD[2].Text;
                        Deliquent_After = TaxHistoryTD[3].Text;
                        Status = TaxHistoryTD[4].Text;
                        Amount_Due = TaxHistoryTD[5].Text;
                        Total_Due = TaxHistoryTD[6].Text;

                        Secured_Details1 = "" + "~" + "" + "~" + "" + "~" + Installment + "~" + Installment_Amount + "~" + Deliquent_After + "~" + Status + "~" + Amount_Due + "~" + Total_Due;
                        gc.insert_date(orderNumber, Parcel_No, 952, Secured_Details1, 1, DateTime.Now);
                    }
                    if (TaxHistoryTD.Count == 6)
                    {
                        Installment1 = TaxHistoryTD[0].Text;
                        Installment_Amount1 = TaxHistoryTD[1].Text;
                        Deliquent_After1 = TaxHistoryTD[2].Text;
                        Status1 = TaxHistoryTD[3].Text;
                        Amount_Due1 = TaxHistoryTD[4].Text;
                        Total_Due1 = TaxHistoryTD[5].Text;

                        Secured_Details2 = "" + "~" + "" + "~" + "" + "~" + Installment1 + "~" + Installment_Amount1 + "~" + Deliquent_After1 + "~" + Status1 + "~" + Amount_Due1 + "~" + Total_Due1;
                        gc.CreatePdf(orderNumber, Parcel_No, "Secured Details" + Tax_Year, driver, "CA", "San Diego");
                        gc.insert_date(orderNumber, Parcel_No, 952, Secured_Details2, 1, DateTime.Now);
                    }
                }
            }
            catch
            { }
        }
        public void ExcemptionDetais(string orderNumber, string parcelNumber)
        {
            try
            {
                string Tax_Type1 = driver.FindElement(By.Id("display-section-header")).Text;
                string Supplemnt_details = Tax_Type1 + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                gc.insert_date(orderNumber, Parcel_No, 954, Supplemnt_details, 1, DateTime.Now);
                IWebElement SupplemntTB = driver.FindElement(By.XPath("//*[@id='su_bp9']/div[2]/table/tbody"));
                IList<IWebElement> SupplemntTR = SupplemntTB.FindElements(By.TagName("tr"));
                IList<IWebElement> SupplemntTD;

                foreach (IWebElement Supplemnt in SupplemntTR)
                {
                    SupplemntTD = Supplemnt.FindElements(By.TagName("td"));
                    if (SupplemntTD.Count != 0 && SupplemntTD.Count != 2 && !Supplemnt.Text.Contains("LAND"))
                    {
                        Value_Exceptions = SupplemntTD[0].Text;
                        Land = SupplemntTD[1].Text;
                        Improvemnts = SupplemntTD[2].Text;
                        Total = SupplemntTD[3].Text;

                        Supplemnt_details2 = "" + "~" + Value_Exceptions + "~" + Land + "~" + Improvemnts + "~" + Total;
                        gc.insert_date(orderNumber, Parcel_No, 954, Supplemnt_details2, 1, DateTime.Now);
                    }
                    if (SupplemntTD.Count == 2)
                    {
                        Value_Exceptions1 = SupplemntTD[0].Text;
                        Land1 = SupplemntTD[1].Text;

                        Supplemnt_details1 = "" + "~" + Value_Exceptions1 + "~" + "" + "~" + "" + "~" + Land1;
                        gc.insert_date(orderNumber, Parcel_No, 954, Supplemnt_details1, 1, DateTime.Now);
                    }
                }

            }
            catch
            { }
        }
        public void ParcelSearch(string orderNumber, string parcelNumber)
        {
            driver.Navigate().GoToUrl("https://iwr.sdtreastax.com/SanDiegoTTCPaymentApplication/Search.aspx");
            Thread.Sleep(2000);

            driver.FindElement(By.XPath("//*[@id='accordion']/div[1]/div[1]/h1/a")).Click();
            Thread.Sleep(2000);

            Parcel_No = parcelNumber.Replace("-", "");
            driver.FindElement(By.Id("PaymentApplicationContent_tbParcelNumber")).SendKeys(Parcel_No);

            gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearch", driver, "CA", "San Diego");
            driver.FindElement(By.Id("PaymentApplicationContent_btnSubmitOption2")).SendKeys(Keys.Enter);
            Thread.Sleep(2000);
        }
    }
}