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
    public class WebDriver_JeffersonAL
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();

        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_JeffersonAL(string address, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
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

                try
                {
                    try
                    {
                        driver.Navigate().GoToUrl("http://eringcapture.jccal.org/caportal/CAPortal_MainPage.aspx");
                        Thread.Sleep(3000);
                        Taxing_Authority = driver.FindElement(By.XPath("//*[@id='thisForm']/table[2]/tbody")).Text.Trim().Replace("\r\n", " ");
                        gc.CreatePdf_WOP(orderNumber, "Tax Authority", driver, "AL", "Jefferson");
                    }
                    catch { }
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("http://eringcapture.jccal.org/caportal/CA_PropertyTaxSearch.aspx");
                    Thread.Sleep(3000);
                    if (searchType == "titleflex")
                    {
                        string titleaddress = address;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "AL", "Jefferson");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_JeffersonAL"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                        parcelNumber = parcelNumber.Replace("-", "");

                    }

                    if (searchType == "address")
                    {
                        driver.FindElement(By.Id("SearchByAddress")).Click();
                        Thread.Sleep(1000);
                        driver.FindElement(By.Id("SearchText")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "AL", "Jefferson");
                        driver.FindElement(By.Id("Search")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search result", driver, "AL", "Jefferson");

                        try
                        {
                            string mul = ""; int multicount = 0;
                            IWebElement multiadd = driver.FindElement(By.XPath("//*[@id='TotalRecFound']/b"));
                            mul = GlobalClass.Before(multiadd.Text, "Records Found").Trim();

                            if (Convert.ToInt32(mul) > 25)
                            {
                                HttpContext.Current.Session["multiParcel_JeffersonAL_Multicount"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                            if (Convert.ToInt32(mul) > 1)
                            {
                                for (int i = 1; i <= Convert.ToInt32(mul); i++)
                                {
                                    string strowner = "", strAddress = "", strProperty = "";
                                    IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='BodyTable']/tbody/tr[" + i + "]/td/fieldset"));

                                    strProperty = GlobalClass.Before(multiaddress.Text, "         \r\n").Trim();
                                    strAddress = gc.Between(multiaddress.Text, "ADDRESS", "LAND VALUE").Trim().Replace(": ", "");
                                    strowner = gc.Between(multiaddress.Text, "OWNER NAME", "RECEIPT NO").Trim().Replace(":", "");

                                    string multidetails = strowner + "~" + strAddress;
                                    gc.insert_date(orderNumber, strProperty, 1128, multidetails, 1, DateTime.Now);


                                }
                            }

                            if (Convert.ToInt32(mul) <= 25)
                            {
                                HttpContext.Current.Session["multiparcel_JeffersonAL"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }


                        }
                        catch { }


                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.Id("SearchByParcel")).Click();
                        Thread.Sleep(1000);
                        driver.FindElement(By.Id("SearchText")).SendKeys(parcelNumber.Trim().Replace(" ", ""));
                        gc.CreatePdf(orderNumber, parcelNumber, "parcel search", driver, "AL", "Jefferson");
                        driver.FindElement(By.Id("Search")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "parcel search Result", driver, "AL", "Jefferson");

                    }
                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("SearchByName")).SendKeys(Keys.Enter);
                        Thread.Sleep(1000);
                        driver.FindElement(By.Id("SearchText")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "OwnerName Search", driver, "AL", "Jefferson");
                        driver.FindElement(By.XPath("//*[@id='Search']")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "OwnerName Search Results", driver, "AL", "Jefferson");
                        try
                        {
                            string mul = ""; int multicount = 0;
                            IWebElement multiadd = driver.FindElement(By.XPath("//*[@id='TotalRecFound']/b"));
                            mul = GlobalClass.Before(multiadd.Text, "Records Found").Trim();

                            if (Convert.ToInt32(mul) > 25)
                            {
                                HttpContext.Current.Session["multiParcel_JeffersonAL_Multicount"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                            if (Convert.ToInt32(mul) > 1)
                            {
                                for (int i = 1; i <= Convert.ToInt32(mul); i++)
                                {
                                    string strowner = "", strAddress = "", strProperty = "";
                                    IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='BodyTable']/tbody/tr[" + i + "]/td/fieldset"));

                                    strProperty = GlobalClass.Before(multiaddress.Text, "         \r\n").Trim();
                                    strAddress = gc.Between(multiaddress.Text, "ADDRESS", "LAND VALUE").Trim().Replace(": ", "");
                                    strowner = gc.Between(multiaddress.Text, "OWNER NAME", "RECEIPT NO").Trim().Replace(":", "");

                                    string multidetails = strowner + "~" + strAddress;
                                    gc.insert_date(orderNumber, strProperty, 1128, multidetails, 1, DateTime.Now);


                                }
                            }

                            if (Convert.ToInt32(mul) <= 25)
                            {
                                HttpContext.Current.Session["multiparcel_JeffersonAL"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }


                        }
                        catch { }
                    }

                    try
                    {
                        IWebElement Inodata = driver.FindElement(By.Id("TotalRecFound"));
                        if (Inodata.Text.Contains("No Records Found"))
                        {
                            HttpContext.Current.Session["Nodata_JeffersonAL"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    IWebElement taxyear = driver.FindElement(By.XPath("//*[@id='TaxYear']"));
                    string Tax_Year = "", Land_Value = "", Improvement_Value = "", Tax_Year1 = "", Total_Value = "";
                    Tax_Year = taxyear.Text;
                    Tax_Year1 = GlobalClass.Before(Tax_Year, "\r\n");

                    IWebElement IProperty = driver.FindElement(By.XPath("//*[@id='BodyTable']/tbody/tr/td/fieldset/legend/span[1]/b/u"));
                    IProperty.Click();
                    Thread.Sleep(3000);
                    gc.CreatePdf_WOP(orderNumber, "Property Details", driver, "AL", "Jefferson");
                    IWebElement IAssessment = driver.FindElement(By.Id("Iframe2"));
                    driver.SwitchTo().Frame(IAssessment);

                    string Bigdata = driver.FindElement(By.XPath("//*[@id='MainTable']/tbody/tr[1]/td[1]/fieldset/table/tbody")).Text;
                    string Parcel_Id = "", Owner_Name = "", Mailing_Address = "", Property_address = "", Year_Built = "", Legal = ""; ;
                    Parcel_Id = gc.Between(Bigdata, "PARCEL", "OWNER").Replace(":", "").Replace("#", "").Replace(" ", "");
                    Owner_Name = gc.Between(Bigdata, "OWNER", "ADDRESS").Replace(":", "");
                    Mailing_Address = gc.Between(Bigdata, "ADDRESS", "LOCATION").Replace(":", "");
                    Property_address = GlobalClass.After(Bigdata, "LOCATION").Replace(":", "");
                    // Land Info
                    driver.FindElement(By.Id("Land")).SendKeys(Keys.Enter);
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, Parcel_Id, "Land Info", driver, "AL", "Jefferson");
                    // Sales Info
                    driver.FindElement(By.Id("Sales")).SendKeys(Keys.Enter);
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_Id, "Sales Info", driver, "AL", "Jefferson");

                    driver.FindElement(By.Id("Buildings")).SendKeys(Keys.Enter);
                    Thread.Sleep(2000);
                    driver.SwitchTo().DefaultContent();
                    Thread.Sleep(3000);
                    IWebElement IAssessment2 = driver.FindElement(By.XPath("//*[@id='Iframe1']"));
                    driver.SwitchTo().Frame(IAssessment2);
                    Thread.Sleep(3000);
                    try
                    {
                        gc.CreatePdf(orderNumber, Parcel_Id, "Buildings Info", driver, "AL", "Jefferson");

                        IWebElement Iyearbuilt = driver.FindElement(By.XPath("//*[@id='GeneralFS']/table/tbody"));
                        IList<IWebElement> IyearRow = Iyearbuilt.FindElements(By.TagName("tr"));
                        IList<IWebElement> Iyeartd;
                        foreach (IWebElement year in IyearRow)
                        {
                            Iyeartd = year.FindElements(By.TagName("td"));
                            if (Iyeartd.Count != 0 && year.Text.Contains("Built"))
                            {
                                Year_Built = Iyeartd[1].Text;
                            }
                        }
                    }
                    catch (Exception ex) { }

                    driver.SwitchTo().DefaultContent();
                    IWebElement ILand = driver.FindElement(By.XPath("//*[@id='Iframe2']"));
                    driver.SwitchTo().Frame(ILand);
                    driver.FindElement(By.Id("Land")).SendKeys(Keys.Enter);
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_Id, "Land", driver, "AL", "Jefferson");
                    driver.SwitchTo().DefaultContent();
                    Thread.Sleep(1000);
                    IWebElement ILegal1 = driver.FindElement(By.Id("Iframe1"));
                    driver.SwitchTo().Frame(ILegal1);
                    Thread.Sleep(2000);
                    IWebElement ILandDetails = driver.FindElement(By.XPath("//*[@id='TABLE3']"));
                    IList<IWebElement> ILandRow = ILandDetails.FindElements(By.TagName("tr"));
                    IList<IWebElement> ILandTD;
                    foreach (IWebElement land in ILandRow)
                    {
                        ILandTD = land.FindElements(By.TagName("td"));
                        if (ILandTD.Count != 0 && land.Text.Contains("METES AND BOUNDS:"))
                        {
                            Legal = ILandTD[0].Text.Replace("METES AND BOUNDS:", "");
                        }
                    }

                    driver.SwitchTo().DefaultContent();
                    IWebElement ISummary = driver.FindElement(By.XPath("//*[@id='Iframe2']"));
                    driver.SwitchTo().Frame(ISummary);
                    driver.FindElement(By.Id("Summary")).SendKeys(Keys.Enter);
                    Thread.Sleep(2000);
                    //gc.CreatePdf(orderNumber, Parcel_Id, "Summary Info", driver, "AL", "Jefferson");
                    string Property_Details = Owner_Name + "~" + Mailing_Address + "~" + Property_address + "~" + Legal + "~" + Year_Built;
                    gc.insert_date(orderNumber, Parcel_Id, 1123, Property_Details, 1, DateTime.Now);

                    driver.SwitchTo().DefaultContent();
                    Thread.Sleep(5000);
                    IWebElement ISale = driver.FindElement(By.Id("Iframe1"));
                    driver.SwitchTo().Frame(ISale);
                    Thread.Sleep(5000);
                    // Tax Information

                    string Tax_Information = "";
                    try
                    {
                        IWebElement Taxinformation = driver.FindElement(By.XPath("//*[@id='thisForm']/table/tbody/tr/td/fieldset/table/tbody/tr[4]/td/fieldset/font/b"));
                        Tax_Information = Taxinformation.Text;
                        string Field_Name = "Comments";
                        string TaxinformationDetails = "" + "~" + Field_Name + "~" + Tax_Information;
                        gc.insert_date(orderNumber, Parcel_Id, 1127, TaxinformationDetails, 1, DateTime.Now);
                        gc.CreatePdf(orderNumber, Parcel_Id, "Tax sale Info", driver, "AL", "Jefferson");
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
                                driver.SwitchTo().DefaultContent();
                                Thread.Sleep(4000);
                                IWebElement IAss = driver.FindElement(By.Id("Iframe2"));
                                driver.SwitchTo().Frame(IAss);
                                Thread.Sleep(4000);
                                IWebElement IAssessmentDetails = driver.FindElement(By.XPath("//*[@id='MainTable']/tbody/tr[1]/td[2]/fieldset/table"));
                                IList<IWebElement> IAssessmentRow = IAssessmentDetails.FindElements(By.TagName("tr"));
                                IList<IWebElement> IAssessmentTD;
                                foreach (IWebElement Assessment in IAssessmentRow)
                                {
                                    IAssessmentTD = Assessment.FindElements(By.TagName("td"));
                                    if (IAssessmentTD.Count != 0 && Assessment.Text.Contains("Land:") && Assessment.Text.Contains("Imp:") && Assessment.Text.Contains("Total:"))
                                    {
                                        Land_Value = IAssessmentTD[0].Text.Replace("Land:", "");
                                        Improvement_Value = IAssessmentTD[1].Text.Replace("Imp:", "");
                                        Total_Value = IAssessmentTD[2].Text.Replace("Total:", "");
                                    }
                                }
                                //IWebElement totalvalue = driver.FindElement(By.XPath("//*[@id='MainTable']/tbody/tr[1]/td[2]/fieldset/table/tbody/tr[2]/td[3]/b"));
                                //Total_Value = totalvalue.Text;
                                //IWebElement landvalue = driver.FindElement(By.XPath("//*[@id='MainTable']/tbody/tr[1]/td[2]/fieldset/table/tbody/tr[2]/td[1]/b"));
                                //Land_Value = landvalue.Text;
                                //IWebElement improvementvalue = driver.FindElement(By.XPath("//*[@id='MainTable']/tbody/tr[1]/td[2]/fieldset/table/tbody/tr[2]/td[2]/b"));
                                //Improvement_Value = improvementvalue.Text;

                                driver.SwitchTo().DefaultContent();
                                Thread.Sleep(1000);
                                IWebElement IAssessment1 = driver.FindElement(By.Id("Iframe1"));
                                driver.SwitchTo().Frame(IAssessment1);
                                Thread.Sleep(2000);
                                IWebElement Ibulk = driver.FindElement(By.XPath("//*[@id='AssmtFS']/table/tbody"));

                                string bulkinfo = driver.FindElement(By.XPath("//*[@id='AssmtFS']/table/tbody")).Text;
                                string Property_Class = "", Exempt_Code = "", Mun_Code = "", School_District = "", Over_Assd_Value = "", Total_Millege = "", Class_Use = "";
                                string Over_65_Code = "", Disability = "", Homestead_Year = "", Exemption_Override_Amount = "", Tax_Sale = "", BOE_Value = "";
                                Property_Class = gc.Between(bulkinfo, "PROPERTY CLASS", "OVER 65 CODE").Replace(":", "");
                                Exempt_Code = gc.Between(bulkinfo, "EXEMPT CODE", "DISABILITY CODE").Replace(":", "");
                                Mun_Code = gc.Between(bulkinfo, "MUN CODE", "HS YEAR").Replace(":", "");
                                School_District = gc.Between(bulkinfo, "SCHOOL DIST", "EXM OVERRIDE AMT").Replace(":", "");
                                if (bulkinfo.Contains("TOTAL MILLAGE:"))
                                {
                                    Over_Assd_Value = gc.Between(bulkinfo, "OVR ASD VALUE", "TOTAL MILLAGE:").Replace(":", "");
                                    Total_Millege = gc.Between(bulkinfo, "TOTAL MILLAGE:", "CLASS USE").Replace(":", "");
                                }
                                if (!bulkinfo.Contains("TOTAL MILLAGE:"))
                                {
                                    Over_Assd_Value = gc.Between(bulkinfo, "OVR ASD VALUE", "CLASS USE").Replace(":", "");
                                    Total_Millege = "";
                                }
                                Class_Use = gc.Between(bulkinfo, "CLASS USE", "FOREST ACRES").Replace(":", "");
                                Over_65_Code = gc.Between(bulkinfo, "OVER 65 CODE", "EXEMPT CODE").Replace(":", "");
                                Disability = gc.Between(bulkinfo, "DISABILITY CODE", "MUN CODE").Replace(":", "");
                                Homestead_Year = gc.Between(bulkinfo, "HS YEAR", "SCHOOL DIST").Replace(":", "");
                                Exemption_Override_Amount = gc.Between(bulkinfo, "EXM OVERRIDE AMT", "OVR ASD VALUE").Replace(":", "");
                                Tax_Sale = gc.Between(bulkinfo, "TAX SALE", "PREV YEAR VALUE").Replace(":", "");
                                BOE_Value = GlobalClass.After(bulkinfo, "BOE VALUE").Replace(":", "").Replace("\r\n", "");
                                string Assessment_Details = strTaxYear + "~" + Land_Value + "~" + Improvement_Value + "~" + Total_Value + "~" + Property_Class + "~" + Exempt_Code + "~" + Mun_Code + "~" + School_District + "~" + Over_Assd_Value + "~" + Total_Millege + "~" + Class_Use + "~" + Over_65_Code + "~" + Disability + "~" + Homestead_Year + "~" + Exemption_Override_Amount + "~" + Tax_Sale + "~" + BOE_Value;
                                gc.insert_date(orderNumber, Parcel_Id, 1124, Assessment_Details, 1, DateTime.Now);

                            }
                            catch { }

                            try
                            {
                                gc.CreatePdf(orderNumber, Parcel_Id, "Tax Info" + i, driver, "AL", "Jefferson");

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
                                    if (TRTaxInfo.Count > 1 && TDTaxInfo.Count != 0 && !row.Text.Contains("MUNCODE") && !row.Text.Contains("GRAND TOTAL") && row.Text.Trim() != "" && !row.Text.Contains("TOTAL"))
                                    {
                                        string TaxDetails = TDTaxInfo[0].Text + "~" + strTaxYear + "~" + TDTaxInfo[1].Text + "~" + TDTaxInfo[2].Text + "~" + TDTaxInfo[3].Text + "~" + TDTaxInfo[4].Text + "~" + TDTaxInfo[5].Text + "~" + TDTaxInfo[6].Text + "~" + TDTaxInfo[7].Text + "~" + Taxing_Authority;

                                        gc.insert_date(orderNumber, Parcel_Id, 1125, TaxDetails, 1, DateTime.Now);
                                    }
                                    if (TRTaxInfo.Count > 1 && TDTaxInfo.Count != 0 && !row.Text.Contains("MUNCODE") && (row.Text.Contains("GRAND TOTAL") || row.Text.Contains("TOTAL")) && row.Text.Trim() != "")
                                    {
                                        string TaxDetails1 = "";
                                        if (row.Text.Contains("TOTAL") && row.Text.Contains("FEE"))
                                        {
                                            TaxDetails1 = "~" + strTaxYear + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + TDTaxInfo[2].Text.Replace(":  (Detail)", "").Replace(":", "") + "~" + TDTaxInfo[3].Text + "~" + Taxing_Authority;
                                        }
                                        if (row.Text.Contains("GRAND TOTAL"))
                                        {
                                            string total = "TOTAL";
                                            TaxDetails1 = total + "~" + strTaxYear + "~" + "" + "~" + "" + "~" + TDTaxInfo[0].Text.Replace("ASSD. VALUE:", "") + "~" + TDTaxInfo[1].Text + "~" + "" + "~" + "" + "~" + TDTaxInfo[2].Text.Replace("GRAND TOTAL:", "") + "~" + Taxing_Authority;
                                        }

                                        gc.insert_date(orderNumber, Parcel_Id, 1125, TaxDetails1, 1, DateTime.Now);
                                    }
                                }
                            }
                            catch { }
                            string current = driver.CurrentWindowHandle;
                            try
                            {
                                IWebElement IFeeInfoDetail = driver.FindElement(By.XPath("//*[@id='thisForm']/table/tbody/tr/td/fieldset/table/tbody/tr[4]/td/fieldset/table"));
                                IList<IWebElement> IfeeClick = IFeeInfoDetail.FindElements(By.TagName("a"));
                                foreach (IWebElement feecick in IfeeClick)
                                {
                                    if (feecick.Text.Trim().Contains("(Detail)"))
                                    {
                                        feecick.Click();
                                    }
                                }
                                driver.SwitchTo().Window(driver.WindowHandles.Last());
                                Thread.Sleep(5000);
                                gc.CreatePdf(orderNumber, Parcel_Id, "Tax Fee" + i, driver, "AL", "Jefferson");
                                IWebElement IAsses = driver.FindElement(By.Id("TABLE3"));
                                IList<IWebElement> IAssesRow = IAsses.FindElements(By.TagName("tr"));
                                IList<IWebElement> IAssesTD;
                                foreach (IWebElement assess in IAssesRow)
                                {
                                    IAssesTD = assess.FindElements(By.TagName("td"));
                                    if (IAssesTD.Count != 0 && !assess.Text.Contains("Fee Type"))
                                    {
                                        string TaxDetails = strTaxYear + "~" + IAssesTD[0].Text + "~" + IAssesTD[1].Text;
                                        gc.insert_date(orderNumber, Parcel_Id, 1127, TaxDetails, 1, DateTime.Now);
                                    }
                                }
                                driver.SwitchTo().Window(current);
                            }
                            catch
                            {
                                driver.SwitchTo().Window(current);
                            }

                            IWebElement IAsses1 = driver.FindElement(By.Id("Iframe1"));
                            driver.SwitchTo().Frame(IAsses1);
                            Thread.Sleep(5000);
                            // Tax Payment

                            try
                            {
                                if (i == 0)
                                {
                                    //gc.CreatePdf(orderNumber, Parcel_Id, "Tax Payment", driver, "AL", "Jefferson");
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

                                            gc.insert_date(orderNumber, Parcel_Id, 1126, TaxPayDetails, 1, DateTime.Now);
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
                    gc.insert_TakenTime(orderNumber, "AL", "Jefferson", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "AL", "Jefferson");
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
