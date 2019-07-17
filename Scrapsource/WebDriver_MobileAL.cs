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

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_MobileAL
    {

        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();

        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_MobileAL(string address, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string Taxing_Authority = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                //  using (driver = new ChromeDriver())

                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    string taxAuth = "", taxauth1 = "", taxauth2 = "", taxauth3 = "", taxauth4 = "";
                    try
                    {
                        driver.Navigate().GoToUrl("https://www.mobilecopropertytax.com/contact-us/");
                        taxAuth = driver.FindElement(By.XPath("/html/body/div[2]/div/div[1]/div/div/div[1]/div[3]/a")).Text.Trim();

                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Authority", driver, "AL", "Mobile");
                    }
                    catch { }

                    driver.Navigate().GoToUrl("https://www.mobile-propertytaxapps.com/mobilecz/Caportal/caportal_mainpage.aspx?CapEntry=4");

                    driver.FindElement(By.Id("SearchBttn1")).Click();
                    Thread.Sleep(4000);
                    IWebElement IAssessment = driver.FindElement(By.Id("Iframe1"));
                    driver.SwitchTo().Frame(IAssessment);

                    if (searchType == "titleflex")
                    {
                        string titleaddress = address;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "AL", "Mobile");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_MobileAL"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                        parcelNumber = parcelNumber.Replace("-", "");

                    }

                    if (searchType == "address")
                    {
                        driver.FindElement(By.Id("SearchByPropAddr")).Click();
                        Thread.Sleep(1000);
                        driver.FindElement(By.Id("SearchText")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "AL", "Mobile");
                        driver.FindElement(By.Id("Search")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search result", driver, "AL", "Mobile");
                        try
                        {
                            string mul = ""; int multicount = 0;
                            IWebElement multiadd = driver.FindElement(By.XPath("//*[@id='TotalRecFound']/b"));
                            mul = GlobalClass.Before(multiadd.Text, "Records Found").Trim();

                            if (Convert.ToInt32(mul) > 25)
                            {
                                HttpContext.Current.Session["multiParcel_MobileAL_Multicount"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                            if (Convert.ToInt32(mul) > 1)
                            {
                                for (int i = 1; i <= Convert.ToInt32(mul); i++)
                                {
                                    string strowner = "", strAddress = "", strProperty = "";
                                    IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='BodyTable']/tbody/tr[" + i + "]/td/fieldset"));

                                    strProperty = GlobalClass.Before(multiaddress.Text, "OWNER NAME").Trim();
                                    strAddress = gc.Between(multiaddress.Text, "ADDRESS", "LAND VALUE").Replace(": ", "").Trim();
                                    strowner = gc.Between(multiaddress.Text, "OWNER NAME", "RECEIPT NO").Replace(":", "").Trim();

                                    string multidetails = strowner + "~" + strAddress;
                                    gc.insert_date(orderNumber, strProperty, 1651, multidetails, 1, DateTime.Now);


                                }
                            }

                            if (Convert.ToInt32(mul) <= 25)
                            {
                                HttpContext.Current.Session["multiparcel_MobileAL"] = "Yes";
                                driver.Quit();
                                return "Multiparcel";
                            }


                        }
                        catch { }

                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.Id("SearchByParcel")).Click();
                        Thread.Sleep(1000);
                        driver.FindElement(By.Id("SearchText")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "parcel search", driver, "AL", "Mobile");
                        driver.FindElement(By.Id("Search")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "parcel search Result", driver, "AL", "Mobile");

                    }
                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("SearchByName")).SendKeys(Keys.Enter);
                        Thread.Sleep(1000);
                        driver.FindElement(By.Id("SearchText")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "OwnerName Search", driver, "AL", "Mobile");
                        driver.FindElement(By.XPath("//*[@id='Search']")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "OwnerName Search Results", driver, "AL", "Mobile");
                        try
                        {
                            string mul = ""; int multicount = 0;
                            IWebElement multiadd = driver.FindElement(By.XPath("//*[@id='TotalRecFound']/b"));
                            mul = GlobalClass.Before(multiadd.Text, "Records Found").Trim();

                            if (Convert.ToInt32(mul) > 25)
                            {
                                HttpContext.Current.Session["multiParcel_MobileAL_Multicount"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                            if (Convert.ToInt32(mul) > 1)
                            {
                                for (int i = 1; i <= Convert.ToInt32(mul); i++)
                                {
                                    string strowner = "", strAddress = "", strProperty = "";
                                    IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='BodyTable']/tbody/tr[" + i + "]/td/fieldset"));

                                    strProperty = GlobalClass.Before(multiaddress.Text, "OWNER NAME").Trim();
                                    strAddress = gc.Between(multiaddress.Text, "ADDRESS", "LAND VALUE").Replace(": ", "").Trim();
                                    strowner = gc.Between(multiaddress.Text, "OWNER NAME", "RECEIPT NO").Replace(":", "").Trim();

                                    string multidetails = strowner + "~" + strAddress;
                                    gc.insert_date(orderNumber, strProperty, 1651, multidetails, 1, DateTime.Now);


                                }
                            }

                            if (Convert.ToInt32(mul) <= 25)
                            {
                                HttpContext.Current.Session["multiparcel_MobileAL"] = "Yes";
                                driver.Quit();
                                return "Multiparcel";
                            }


                        }
                        catch { }
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.XPath("//*[@id='TABLE2']"));
                        if (INodata.Text.Contains("No Records Found"))
                        {
                            HttpContext.Current.Session["Nodata_MobileAL"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    // Property Details

                    IWebElement taxyear = driver.FindElement(By.XPath("//*[@id='TaxYear']"));
                    string Tax_Year = "", Land_Value = "", Improvement_Value = "", Tax_Year1 = "", Total_Value = "";
                    Tax_Year = taxyear.Text;
                    Tax_Year1 = GlobalClass.Before(Tax_Year, "\r\n");

                    IWebElement IProperty = driver.FindElement(By.XPath("//*[@id='BodyTable']/tbody/tr/td/fieldset/legend/span[1]/b/u"));
                    IProperty.Click();
                    Thread.Sleep(3000);
                    // gc.CreatePdf_WOP(orderNumber, "Property Details", driver, "AL", "Mobile");
                    IWebElement IAssessment1 = driver.FindElement(By.Id("Iframe2"));
                    driver.SwitchTo().Frame(IAssessment1);
                    var SelectY = driver.FindElement(By.Id("YearDDL"));
                    var selectElementY = new SelectElement(SelectY);
                    selectElementY.SelectByIndex(1);
                    string Bigdata = driver.FindElement(By.XPath("//*[@id='MainTable']/tbody/tr[1]/td[1]/fieldset/table/tbody")).Text;
                    string Parcel_Id = "", Owner_Name = "", Mailing_Address = "", PropertyAddress = "", Year_Built = "", Legal_Desc = "";
                    Parcel_Id = gc.Between(Bigdata, "PARCEL", "OWNER").Replace(":", "").Replace("#", "").Replace(" ", "");
                    Owner_Name = gc.Between(Bigdata, "OWNER", "ADDRESS").Replace(":", "");
                    Mailing_Address = gc.Between(Bigdata, "ADDRESS", "LOCATION:").Replace(":", "").Trim();
                    PropertyAddress = GlobalClass.After(Bigdata, "LOCATION").Replace(":", "").Trim();


                    IWebElement totalvalue = driver.FindElement(By.XPath("//*[@id='MainTable']/tbody/tr[1]/td[2]/fieldset/table/tbody/tr[3]/td[3]/b"));
                    Total_Value = totalvalue.Text;
                    IWebElement landvalue = driver.FindElement(By.XPath("//*[@id='MainTable']/tbody/tr[1]/td[2]/fieldset/table/tbody/tr[3]/td[1]/b"));
                    Land_Value = landvalue.Text;
                    IWebElement improvementvalue = driver.FindElement(By.XPath("//*[@id='MainTable']/tbody/tr[1]/td[2]/fieldset/table/tbody/tr[3]/td[2]/b"));
                    Improvement_Value = improvementvalue.Text;


                    // Land Info
                    driver.FindElement(By.Id("Land")).SendKeys(Keys.Enter);
                    Thread.Sleep(3000);
                    driver.SwitchTo().DefaultContent();
                    IWebElement IAssessment21 = driver.FindElement(By.Id("Iframe1"));
                    driver.SwitchTo().Frame(IAssessment21);
                    try
                    {
                        IWebElement IAssessment22 = driver.FindElement(By.Id("Iframe1"));
                        driver.SwitchTo().Frame(IAssessment22);
                        Legal_Desc = driver.FindElement(By.XPath("//*[@id='TABLE3']/tbody/tr[9]/td")).Text.Replace("METES AND BOUNDS:", "").Trim();

                    }
                    catch { }

                    gc.CreatePdf(orderNumber, Parcel_Id, "Land Info", driver, "AL", "Mobile");


                    // Sales Info
                    try
                    {
                        driver.SwitchTo().DefaultContent();
                        IWebElement Isales = driver.FindElement(By.Id("Iframe1"));
                        driver.SwitchTo().Frame(Isales);
                        IWebElement Isales1 = driver.FindElement(By.Id("Iframe2"));
                        driver.SwitchTo().Frame(Isales1);

                        driver.FindElement(By.Id("Sales")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, Parcel_Id, "Sales Info", driver, "AL", "Mobile");
                    }
                    catch { }

                    try
                    {
                        driver.FindElement(By.Id("Buildings")).Click();
                        Thread.Sleep(2000);
                    }
                    catch { }
                    driver.SwitchTo().DefaultContent();
                    IWebElement IAssessment2 = driver.FindElement(By.Id("Iframe1"));
                    driver.SwitchTo().Frame(IAssessment2);
                    try
                    {
                        IWebElement IAssessment12 = driver.FindElement(By.Id("Iframe1"));
                        driver.SwitchTo().Frame(IAssessment12);
                    }
                    catch { }
                    try
                    {
                        gc.CreatePdf(orderNumber, Parcel_Id, "Buildings Info", driver, "AL", "Mobile");
                        IWebElement Iyearbuilt = driver.FindElement(By.XPath("//*[@id='GeneralFS']/table/tbody"));
                        IList<IWebElement> IyearRow = Iyearbuilt.FindElements(By.TagName("tr"));
                        IList<IWebElement> Iyeartd;
                        foreach (IWebElement year in IyearRow)
                        {
                            Iyeartd = year.FindElements(By.TagName("td"));
                            if (Iyeartd.Count != 0 && year.Text.Contains("Built"))
                            {
                                Year_Built = Iyeartd[1].Text;
                                break;
                            }
                        }
                    }
                    catch { }

                    driver.SwitchTo().DefaultContent();
                    IWebElement ISummary = driver.FindElement(By.Id("Iframe1"));
                    driver.SwitchTo().Frame(ISummary);
                    //driver.FindElement(By.Id("Summary")).SendKeys(Keys.Enter);
                    //Thread.Sleep(2000);
                    //gc.CreatePdf(orderNumber, Parcel_Id, "Summary Info", driver, "AL", "Mobile");
                    string Property_Details = Owner_Name + "~" + Mailing_Address + "~" + PropertyAddress + "~" + Year_Built + "~" + Legal_Desc;
                    gc.insert_date(orderNumber, Parcel_Id, 1639, Property_Details, 1, DateTime.Now);

                    driver.SwitchTo().DefaultContent();
                    IWebElement Iturn = driver.FindElement(By.Id("Iframe1"));
                    driver.SwitchTo().Frame(Iturn);
                    IWebElement Iturn1 = driver.FindElement(By.Id("Iframe2"));
                    driver.SwitchTo().Frame(Iturn1);
                    Thread.Sleep(1000);
                    driver.FindElement(By.Id("Summary")).Click();
                    Thread.Sleep(2000);
                    driver.SwitchTo().DefaultContent();
                    IWebElement IAssessment11 = driver.FindElement(By.Id("Iframe1"));
                    driver.SwitchTo().Frame(IAssessment11);
                    IWebElement IAssess1 = driver.FindElement(By.Id("Iframe1"));
                    driver.SwitchTo().Frame(IAssess1);
                    Thread.Sleep(2000);

                    IWebElement Ibulk = driver.FindElement(By.XPath("//*[@id='AssmtFS']/table/tbody"));

                    string bulkinfo = driver.FindElement(By.XPath("//*[@id='AssmtFS']/table/tbody")).Text;
                    string Property_Class = "", Exempt_Code = "", Mun_Code = "", School_District = "", Over_Assd_Value = "", Class_Use = "";
                    string Over_65_Code = "", Disability = "", Homestead_Year = "", Exemption_Override_Amount = "", Tax_Sale = "", BOE_Value = "", KeyNumber = "";
                    Property_Class = gc.Between(bulkinfo, "PROPERTY CLASS", "OVER 65 CODE").Replace(":", "");
                    Exempt_Code = gc.Between(bulkinfo, "EXEMPT CODE", "DISABILITY CODE").Replace(":", "");
                    Mun_Code = gc.Between(bulkinfo, "MUN CODE", "HS YEAR").Replace(":", "");
                    School_District = gc.Between(bulkinfo, "SCHOOL DIST", "EXM OVERRIDE AMT").Replace(":", "");
                    Over_Assd_Value = gc.Between(bulkinfo, "OVR ASD VALUE", "CLASS USE").Replace(":", "");
                    Class_Use = gc.Between(bulkinfo, "CLASS USE", "FOREST ACRES").Replace(":", "");
                    Over_65_Code = gc.Between(bulkinfo, "OVER 65 CODE", "EXEMPT CODE").Replace(":", "");
                    Disability = gc.Between(bulkinfo, "DISABILITY CODE", "MUN CODE").Replace(":", "");
                    Homestead_Year = gc.Between(bulkinfo, "HS YEAR", "SCHOOL DIST").Replace(":", "");
                    Exemption_Override_Amount = gc.Between(bulkinfo, "EXM OVERRIDE AMT", "OVR ASD VALUE").Replace(":", "");
                    Tax_Sale = gc.Between(bulkinfo, "TAX SALE", "PREV YEAR VALUE").Replace(":", "");
                    BOE_Value = gc.Between(bulkinfo, "BOE VALUE", "KEY #:").Replace(":", "").Replace("\r\n", "");
                    KeyNumber = driver.FindElement(By.XPath("//*[@id='AssmtFS']/table/tbody/tr[10]/td[2]")).Text;
                    string Assessment_Details = Land_Value + "~" + Improvement_Value + "~" + Total_Value + "~" + Property_Class + "~" + Exempt_Code + "~" + Mun_Code + "~" + School_District + "~" + Over_Assd_Value + "~" + Class_Use + "~" + KeyNumber + "~" + Over_65_Code + "~" + Disability + "~" + Homestead_Year + "~" + Exemption_Override_Amount + "~" + Tax_Sale + "~" + BOE_Value;
                    gc.insert_date(orderNumber, Parcel_Id, 1648, Assessment_Details, 1, DateTime.Now);

                    // Tax Information

                    int Iyear = 0; int Cyear = 0;
                    int Syear = DateTime.Now.Year;
                    int Smonth = DateTime.Now.Month;
                    if (Smonth >= 9)
                    {
                        Iyear = Syear;
                        Cyear = Syear;
                    }
                    else
                    {
                        Iyear = Syear - 1;
                        Cyear = Syear - 1;

                    }


                    string Tax_Information = "";
                    try
                    {
                        IWebElement Taxinformation = driver.FindElement(By.XPath("//*[@id='thisForm']/table/tbody/tr/td/fieldset/table/tbody/tr[4]/td/fieldset/font/b"));
                        Tax_Information = Taxinformation.Text;
                        string Field_Name = "Comments";
                        string TaxinformationDetails = Field_Name + "~" + Tax_Information;
                        gc.insert_date(orderNumber, Parcel_Id, 1665, TaxinformationDetails, 1, DateTime.Now);
                        gc.CreatePdf(orderNumber, Parcel_Id, "Tax sale Info", driver, "AL", "Mobile");
                    }
                    catch { }
                    string strTaxYear = "";
                    string[] stryear = { "" };
                    for (int i = 0; i < 3; i++)
                    {
                        driver.SwitchTo().DefaultContent();
                        Thread.Sleep(1000);
                        IWebElement IAssessment10 = driver.FindElement(By.Id("Iframe1"));
                        driver.SwitchTo().Frame(IAssessment10);
                        IWebElement IAssessment12 = driver.FindElement(By.Id("Iframe2"));
                        driver.SwitchTo().Frame(IAssessment12);
                        if (i == 0)
                        {
                            var SelectY1 = driver.FindElement(By.Id("YearDDL"));
                            var selectElementY1 = new SelectElement(SelectY1);
                            selectElementY1.SelectByIndex(1);
                        }
                        if (i == 1)
                        {
                            var SelectY1 = driver.FindElement(By.Id("YearDDL"));
                            var selectElementY1 = new SelectElement(SelectY1);
                            selectElementY1.SelectByIndex(2);
                        }
                        if (i == 2)
                        {
                            var SelectY1 = driver.FindElement(By.Id("YearDDL"));
                            var selectElementY1 = new SelectElement(SelectY1);
                            selectElementY1.SelectByIndex(3);
                        }
                        //if (i == 0)
                        //{
                        //    try
                        //    {
                        //        string TaxYear = Iyear.ToString();
                        //        stryear = TaxYear.Split('\r');
                        //        strTaxYear = stryear[i].Replace("\n", "").Trim();
                        //    }
                        //    catch { }
                        //}
                        //else
                        //{
                        //    strTaxYear = stryear[i].Replace("\n", "").Trim();
                        //}
                        Thread.Sleep(5000);

                        driver.SwitchTo().DefaultContent();
                        Thread.Sleep(1000);

                        // Tax Information
                        try
                        {



                            try
                            {
                                gc.CreatePdf(orderNumber, Parcel_Id, "Tax Info" + Iyear, driver, "AL", "Mobile");
                                string delinquentstatus = "";
                                driver.SwitchTo().DefaultContent();
                                Thread.Sleep(5000);
                                IWebElement IAsses = driver.FindElement(By.Id("Iframe1"));
                                driver.SwitchTo().Frame(IAsses);
                                IWebElement IAsses1 = driver.FindElement(By.Id("Iframe1"));
                                driver.SwitchTo().Frame(IAsses1);
                                Thread.Sleep(5000);
                                try
                                {
                                    delinquentstatus = driver.FindElement(By.XPath("//*[@id='thisForm']/table/tbody/tr/td/fieldset/table/tbody/tr[4]/td/fieldset/table/tbody/tr[9]/td[1]/font/b")).Text;
                                }
                                catch { }


                                IWebElement TaxInfo = driver.FindElement(By.XPath("//*[@id='thisForm']/table/tbody/tr/td/fieldset/table/tbody/tr[4]/td/fieldset/table"));
                                IList<IWebElement> TRTaxInfo = TaxInfo.FindElements(By.TagName("tr"));
                                IList<IWebElement> THTaxInfo = TaxInfo.FindElements(By.TagName("th"));
                                IList<IWebElement> TDTaxInfo;
                                foreach (IWebElement row in TRTaxInfo)
                                {
                                    TDTaxInfo = row.FindElements(By.TagName("td"));
                                    if (TRTaxInfo.Count > 1 && TDTaxInfo.Count != 0 && !row.Text.Contains("MUNCODE") && !row.Text.Contains("GRAND TOTAL") && row.Text.Contains("STATE ") && row.Text.Trim() != "" && TDTaxInfo.Count == 8)
                                    {
                                        string TaxDetails = Iyear + "~" + TDTaxInfo[0].Text + "~" + TDTaxInfo[1].Text + "~" + TDTaxInfo[2].Text + "~" + TDTaxInfo[3].Text + "~" + TDTaxInfo[4].Text + "~" + TDTaxInfo[5].Text + "~" + TDTaxInfo[6].Text + "~" + TDTaxInfo[7].Text + "~" + delinquentstatus + "~" + taxAuth;

                                        gc.insert_date(orderNumber, Parcel_Id, 1649, TaxDetails, 1, DateTime.Now);
                                    }
                                    else if (TRTaxInfo.Count > 1 && TDTaxInfo.Count != 0 && !row.Text.Contains("MUNCODE") && !row.Text.Contains("GRAND TOTAL") && !row.Text.Contains("STATE ") && row.Text.Trim() != "" && TDTaxInfo.Count == 8)
                                    {
                                        string TaxDetails = Iyear + "~" + TDTaxInfo[0].Text + "~" + TDTaxInfo[1].Text + "~" + TDTaxInfo[2].Text + "~" + TDTaxInfo[3].Text + "~" + TDTaxInfo[4].Text + "~" + TDTaxInfo[5].Text + "~" + TDTaxInfo[6].Text + "~" + TDTaxInfo[7].Text + "~" + "" + "~" + taxAuth;

                                        gc.insert_date(orderNumber, Parcel_Id, 1649, TaxDetails, 1, DateTime.Now);
                                    }
                                    else if (TRTaxInfo.Count > 1 && TDTaxInfo.Count != 0 && !row.Text.Contains("MUNCODE") && !row.Text.Contains("GRAND TOTAL") && row.Text.Trim() != "" && TDTaxInfo.Count == 4)
                                    {

                                        string TaxDetails1 = Iyear + "~" + TDTaxInfo[2].Text + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + TDTaxInfo[3].Text + "~" + "" + "~" + taxAuth;

                                        gc.insert_date(orderNumber, Parcel_Id, 1649, TaxDetails1, 1, DateTime.Now);
                                    }
                                    else if (TRTaxInfo.Count > 1 && TDTaxInfo.Count != 0 && !row.Text.Contains("MUNCODE") && row.Text.Contains("GRAND TOTAL") && row.Text.Trim() != "" && TDTaxInfo.Count == 3)
                                    {
                                        string total = "TOTAL";
                                        string TaxDetails1 = Iyear + "~" + total + "~" + "" + "~" + "" + "~" + TDTaxInfo[0].Text.Replace("ASSD. VALUE:", "") + "~" + TDTaxInfo[1].Text + "~" + "" + "~" + "" + "~" + TDTaxInfo[2].Text.Replace("GRAND TOTAL:", "") + "~" + "" + "~" + taxAuth;

                                        gc.insert_date(orderNumber, Parcel_Id, 1649, TaxDetails1, 1, DateTime.Now);
                                    }
                                }
                            }
                            catch { }


                            // Tax Payment

                            try
                            {
                                if (i == 0)
                                {

                                    IWebElement Taxpay = driver.FindElement(By.XPath("//*[@id='PaymentFS']/table/tbody"));
                                    IList<IWebElement> TRTaxpay = Taxpay.FindElements(By.TagName("tr"));
                                    IList<IWebElement> THTaxpay = Taxpay.FindElements(By.TagName("th"));
                                    IList<IWebElement> TDTaxpay;
                                    foreach (IWebElement row in TRTaxpay)
                                    {
                                        TDTaxpay = row.FindElements(By.TagName("td"));
                                        if (TRTaxpay.Count > 1 && TDTaxpay.Count != 0 && !row.Text.Contains("PAID BY") && row.Text.Trim() != "")
                                        {
                                            string TaxPayDetails = TDTaxpay[0].Text + "~" + TDTaxpay[1].Text + "~" + TDTaxpay[2].Text + "~" + TDTaxpay[3].Text;

                                            gc.insert_date(orderNumber, Parcel_Id, 1650, TaxPayDetails, 1, DateTime.Now);
                                        }
                                    }
                                }

                            }
                            catch { }

                            Iyear--;
                        }

                        catch { }
                    }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "AL", "Mobile", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "AL", "Mobile");
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