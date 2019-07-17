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
    public class WebDriver_LimestoneAL
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();

        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_Limestone(string address, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string Taxing_Authority = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            // driver = new PhantomJSDriver();
            //   driver = new ChromeDriver();
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    try
                    {
                        driver.Navigate().GoToUrl("https://www.limestonerevenue.net/CAPortal_MainPage.aspx");
                        IWebElement tax_auth1 = driver.FindElement(By.XPath("//*[@id='thisForm']/table[2]/tbody/tr[4]/td"));
                        IWebElement tax_auth2 = driver.FindElement(By.XPath("//*[@id='thisForm']/table[2]/tbody/tr[5]/td"));
                        IWebElement tax_auth3 = driver.FindElement(By.XPath("//*[@id='thisForm']/table[2]/tbody/tr[6]/td"));
                        Taxing_Authority = tax_auth1.Text + " " + tax_auth2.Text + " " + tax_auth3.Text;
                        gc.CreatePdf_WOP(orderNumber, "Tax Authority", driver, "AL", "Limestone");
                    }
                    catch { }
                    driver.Navigate().GoToUrl("https://www.limestonerevenue.net/CA_PropertyTaxSearch.aspx");

                    if (searchType == "titleflex")
                    {
                        string titleaddress = address;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "AL", "Limestone");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_LimestoneAL"] = "Yes";
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
                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "AL", "Limestone");
                        driver.FindElement(By.Id("Search")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search result", driver, "AL", "Limestone");

                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.Id("SearchByParcel")).Click();
                        Thread.Sleep(1000);
                        driver.FindElement(By.Id("SearchText")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "parcel search", driver, "AL", "Limestone");
                        driver.FindElement(By.Id("Search")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "parcel search Result", driver, "AL", "Limestone");

                    }
                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("SearchByName")).SendKeys(Keys.Enter);
                        Thread.Sleep(1000);
                        driver.FindElement(By.Id("SearchText")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "OwnerName Search", driver, "AL", "Limestone");
                        driver.FindElement(By.XPath("//*[@id='Search']")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "OwnerName Search Results", driver, "AL", "Limestone");
                        try
                        {
                            string mul = ""; int multicount = 0;
                            IWebElement multiadd = driver.FindElement(By.XPath("//*[@id='TotalRecFound']/b"));
                            mul = GlobalClass.Before(multiadd.Text, "Records Found").Trim();

                            if (Convert.ToInt32(mul) > 25)
                            {
                                HttpContext.Current.Session["multiParcel_LimestoneAL_Multicount"] = "Maximum";
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
                                    strAddress = gc.Between(multiaddress.Text, "ADDRESS", "LAND VALUE").Trim().Replace(": ", "");
                                    strowner = gc.Between(multiaddress.Text, "OWNER NAME", "RECEIPT NO").Trim().Replace(":", "");

                                    string multidetails = strowner + "~" + strAddress;
                                    gc.insert_date(orderNumber, strProperty, 1288, multidetails, 1, DateTime.Now);


                                }
                            }

                            if (Convert.ToInt32(mul) <= 25)
                            {
                                HttpContext.Current.Session["multiparcel_LimestoneAL"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }


                        }
                        catch { }
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.Id("TotalRecFound"));
                        if(INodata.Text.Contains("No Records Found"))
                        {
                            HttpContext.Current.Session["Nodata_LimestoneAL"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    // Property Details

                    IWebElement taxyear = driver.FindElement(By.XPath("//*[@id='TaxYear']"));
                    string Tax_Year = "", Land_Value = "", Improvement_Value = "", Tax_Year1 = "", Total_Value = "", Legal1 = "", Legal2 = "";
                    Tax_Year = taxyear.Text;
                    Tax_Year1 = GlobalClass.Before(Tax_Year, "\r\n");

                    IWebElement IProperty = driver.FindElement(By.XPath("//*[@id='BodyTable']/tbody/tr/td/fieldset/legend/span[1]/b/u"));
                    IProperty.Click();
                    Thread.Sleep(3000);
                    // gc.CreatePdf_WOP(orderNumber, "Property Details", driver, "AL", "Limestone");
                    IWebElement IAssessment = driver.FindElement(By.Id("Iframe2"));
                    driver.SwitchTo().Frame(IAssessment);

                    string Bigdata = driver.FindElement(By.XPath("//*[@id='MainTable']/tbody/tr[1]/td[1]/fieldset/table/tbody")).Text;
                    string Parcel_Id = "", Owner_Name = "", Mailing_Address = "", Year_Built = "", PropertyAddress = "";
                    Parcel_Id = gc.Between(Bigdata, "PARCEL", "OWNER").Replace(":", "").Replace("#", "").Trim();
                    Owner_Name = gc.Between(Bigdata, "OWNER", "ADDRESS").Replace(":", "");
                    Mailing_Address = gc.Between(Bigdata, "ADDRESS", "LOCATION").Replace(":", "").Trim();
                    PropertyAddress = GlobalClass.After(Bigdata, "LOCATION").Replace(":", "").Trim();
                    string Big_Data = driver.FindElement(By.XPath("//*[@id='MainTable']/tbody/tr[1]/td[2]/fieldset/table/tbody")).Text;
                    string LegalDesc = "", Code = "", Acres = "";
                    Acres = gc.Between(Big_Data, "Acres:", "Sales Info:").Replace(":", "").Trim();
                    Total_Value = gc.Between(Big_Data, "Total:", "Acres:").Replace(":", "").Trim();
                    Land_Value = gc.Between(Big_Data, "Land:", "Imp:").Replace(":", "").Trim();
                    Improvement_Value = gc.Between(Big_Data, "Imp:", "Total:").Replace(":", "").Trim();

                    // Land Info
                    driver.FindElement(By.Id("Land")).SendKeys(Keys.Enter);
                    Thread.Sleep(4000);
                    driver.SwitchTo().DefaultContent();
                    IWebElement IAssessment2 = driver.FindElement(By.Id("Iframe1"));
                    driver.SwitchTo().Frame(IAssessment2);
                    try
                    {
                        IWebElement ICode = driver.FindElement(By.XPath("//*[@id='TABLE1']/tbody/tr[2]/td[3]"));
                        IWebElement Ilegal1 = driver.FindElement(By.XPath("//*[@id='TABLE3']/tbody/tr[1]/td[1]"));
                        Legal1 = Ilegal1.Text.Replace("SUB DIVISON1:", "").Trim();
                        IWebElement Ilegal2 = driver.FindElement(By.XPath("//*[@id='TABLE3']/tbody/tr[2]/td[1]"));
                        Legal2 = Ilegal2.Text.Replace("SUB DIVISON2:", "").Trim();
                        LegalDesc = Legal1 + Legal2;
                        Code = ICode.Text.Trim();
                    }
                    catch { }
                    gc.CreatePdf(orderNumber, Parcel_Id, "Land Info", driver, "AL", "Limestone");
                    driver.SwitchTo().DefaultContent();
                    IWebElement ISale = driver.FindElement(By.Id("Iframe2"));
                    driver.SwitchTo().Frame(ISale);
                    // Sales Info
                    driver.FindElement(By.Id("Sales")).SendKeys(Keys.Enter);
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_Id, "Sales Info", driver, "AL", "Limestone");

                    driver.FindElement(By.Id("Buildings")).SendKeys(Keys.Enter);
                    Thread.Sleep(2000);
                    driver.SwitchTo().DefaultContent();
                    IWebElement Ibulild = driver.FindElement(By.Id("Iframe1"));
                    driver.SwitchTo().Frame(Ibulild);
                    try
                    {
                        gc.CreatePdf(orderNumber, Parcel_Id, "Buildings Info", driver, "AL", "Limestone");
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
                    IWebElement ISummary = driver.FindElement(By.Id("Iframe2"));
                    driver.SwitchTo().Frame(ISummary);
                    driver.FindElement(By.Id("Summary")).SendKeys(Keys.Enter);
                    Thread.Sleep(2000);
                    //gc.CreatePdf(orderNumber, Parcel_Id, "Summary Info", driver, "AL", "Limestone");
                    string Property_Details = Owner_Name + "~" + Mailing_Address + "~" + Year_Built + "~" + PropertyAddress + "~" + LegalDesc + "~" + Code;
                    gc.insert_date(orderNumber, Parcel_Id, 1283, Property_Details, 1, DateTime.Now);



                    driver.SwitchTo().DefaultContent();
                    Thread.Sleep(1000);
                    IWebElement IAssessment1 = driver.FindElement(By.Id("Iframe1"));
                    driver.SwitchTo().Frame(IAssessment1);
                    Thread.Sleep(2000);
                    IWebElement Ibulk = driver.FindElement(By.XPath("//*[@id='AssmtFS']/table/tbody"));

                    string bulkinfo = driver.FindElement(By.XPath("//*[@id='AssmtFS']/table/tbody")).Text;
                    string Property_Class = "", Exempt_Code = "", Mun_Code = "", School_District = "", Over_Assd_Value = "", Class_Use = "";
                    string Over_65_Code = "", Disability = "", Homestead_Year = "", Exemption_Override_Amount = "", Tax_Sale = "", BOE_Value = "";
                    string ForestAcres = "", Prevyearvalue = "", Key = "";
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
                    ForestAcres = gc.Between(bulkinfo, "FOREST ACRES:", "TAX SALE:").Trim();
                    Prevyearvalue = gc.Between(bulkinfo, "PREV YEAR VALUE:", "BOE VALUE:").Trim();
                    Key = GlobalClass.After(bulkinfo, "KEY #:").Trim();
                    string Assessment_Details = Tax_Year1 + "~" + Land_Value + "~" + Improvement_Value + "~" + Total_Value + "~" + Property_Class + "~" + Exempt_Code + "~" + Mun_Code + "~" + School_District + "~" + Over_Assd_Value + "~" + Class_Use + "~" + Over_65_Code + "~" + Disability + "~" + Homestead_Year + "~" + Exemption_Override_Amount + "~" + Tax_Sale + "~" + BOE_Value + "~" + Acres + "~" + ForestAcres + "~" + Prevyearvalue + "~" + Key;
                    gc.insert_date(orderNumber, Parcel_Id, 1284, Assessment_Details, 1, DateTime.Now);

                    // Tax Information

                    string Tax_Information = "";
                    try
                    {
                        IWebElement Taxinformation = driver.FindElement(By.XPath("//*[@id='thisForm']/table/tbody/tr/td/fieldset/table/tbody/tr[4]/td/fieldset/font/b"));
                        Tax_Information = Taxinformation.Text;
                        string Field_Name = "Comments";
                        string TaxinformationDetails = Field_Name + "~" + Tax_Information;
                        gc.insert_date(orderNumber, Parcel_Id, 1287, TaxinformationDetails, 1, DateTime.Now);
                        gc.CreatePdf(orderNumber, Parcel_Id, "Tax sale Info", driver, "AL", "Limestone");
                    }
                    catch { }
                    driver.SwitchTo().DefaultContent();
                    Thread.Sleep(1000);
                    string[] stryear = { "" };
                    // Tax Information
                    try
                    {
                        string strTaxYear = "";
                        for (int i = 0; i < 3; i++)
                        {

                            driver.SwitchTo().DefaultContent();
                            Thread.Sleep(4000);
                            IWebElement ISumma = driver.FindElement(By.Id("Iframe2"));
                            driver.SwitchTo().Frame(ISumma);
                            Thread.Sleep(4000);
                            IWebElement SerachCategory = driver.FindElement(By.Id("YearDDL"));
                            var selectElement1 = new SelectElement(SerachCategory);
                            selectElement1.SelectByIndex(i);
                            Thread.Sleep(4000);
                            if (i == 0)
                            {
                                try
                                {
                                    string TaxYear = SerachCategory.Text;
                                    stryear = TaxYear.Split('\r');
                                    strTaxYear = stryear[i].Replace("\n", "").Trim();
                                }
                                catch { }
                            }
                            else
                            {
                                strTaxYear = stryear[i].Replace("\n", "").Trim();
                            }
                            Thread.Sleep(5000);
                            //try
                            //{
                            //    driver.FindElement(By.Id("Summary")).SendKeys(Keys.Enter);
                            //    Thread.Sleep(3000);

                            //}
                            //catch { }
                            try
                            {
                                gc.CreatePdf(orderNumber, Parcel_Id, "Tax Info" + i, driver, "AL", "Limestone");

                                driver.SwitchTo().DefaultContent();
                                Thread.Sleep(5000);
                                IWebElement IAsses = driver.FindElement(By.Id("Iframe1"));
                                driver.SwitchTo().Frame(IAsses);
                                Thread.Sleep(5000);
                                IWebElement TaxInfo = driver.FindElement(By.XPath("//*[@id='thisForm']/table/tbody/tr/td/fieldset/table/tbody/tr[4]/td/fieldset/table"));
                                IList<IWebElement> TRTaxInfo = TaxInfo.FindElements(By.TagName("tr"));
                                IList<IWebElement> THTaxInfo = TaxInfo.FindElements(By.TagName("th"));
                                IList<IWebElement> TDTaxInfo;
                                foreach (IWebElement row in TRTaxInfo)
                                {
                                    TDTaxInfo = row.FindElements(By.TagName("td"));
                                    if (TRTaxInfo.Count > 1 && TDTaxInfo.Count != 0 && !row.Text.Contains("MUNCODE") && !row.Text.Contains("GRAND TOTAL") && !row.Text.Contains("TOTAL FEE & INTEREST") && row.Text.Trim() != "")
                                    {
                                        string TaxDetails = TDTaxInfo[0].Text + "~" + strTaxYear + "~" + TDTaxInfo[1].Text + "~" + TDTaxInfo[2].Text + "~" + TDTaxInfo[3].Text + "~" + TDTaxInfo[4].Text + "~" + TDTaxInfo[5].Text + "~" + TDTaxInfo[6].Text + "~" + TDTaxInfo[7].Text + "~" + Taxing_Authority;

                                        gc.insert_date(orderNumber, Parcel_Id, 1285, TaxDetails, 1, DateTime.Now);
                                    }
                                    else if (TRTaxInfo.Count > 1 && TDTaxInfo.Count != 0 && !row.Text.Contains("MUNCODE") && row.Text.Contains("GRAND TOTAL") && !row.Text.Contains("TOTAL FEE & INTEREST") && row.Text.Trim() != "")
                                    {
                                        string total = "TOTAL";
                                        string TaxDetails1 = total + "~" + strTaxYear + "~" + "" + "~" + "" + "~" + TDTaxInfo[0].Text.Replace("ASSD. VALUE:", "") + "~" + TDTaxInfo[1].Text + "~" + "" + "~" + "" + "~" + TDTaxInfo[2].Text.Replace("GRAND TOTAL:", "") + "~" + Taxing_Authority;

                                        gc.insert_date(orderNumber, Parcel_Id, 1285, TaxDetails1, 1, DateTime.Now);
                                    }
                                    if (TRTaxInfo.Count > 1 && TDTaxInfo.Count != 0 && !row.Text.Contains("MUNCODE") && !row.Text.Contains("GRAND TOTAL") && row.Text.Contains("TOTAL FEE & INTEREST") && row.Text.Trim() != "")
                                    {
                                        string TaxDetails2 = "" + "~" + strTaxYear + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + TDTaxInfo[2].Text + "~" + TDTaxInfo[3].Text + "~" + Taxing_Authority;

                                        gc.insert_date(orderNumber, Parcel_Id, 1285, TaxDetails2, 1, DateTime.Now);
                                    }
                                }
                            }
                            catch { }


                            // Tax Payment

                            try
                            {
                                if (i == 0)
                                {
                                    //gc.CreatePdf(orderNumber, Parcel_Id, "Tax Payment", driver, "AL", "Limestone");
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

                                            gc.insert_date(orderNumber, Parcel_Id, 1286, TaxPayDetails, 1, DateTime.Now);
                                        }
                                    }
                                }

                            }
                            catch { }

                        }
                    }
                    catch { }



                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "AL", "Limestone", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "AL", "Limestone");
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