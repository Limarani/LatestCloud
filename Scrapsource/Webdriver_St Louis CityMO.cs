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
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Linq;

namespace ScrapMaricopa.Scrapsource
{
    public class Webdriver_St_Louis_CityMO
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();

        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_Stlouiscity(string address, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel, string taxmapnumber)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string TaxingAuthority = "";
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {

                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    try
                    {
                        //Taxing Authority
                        driver.Navigate().GoToUrl("https://www.stlouis-mo.gov/collector/real-estate-tax-home.cfm");
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Taxing Authority", driver, "MO", "St Louis City");
                        string bulkTaxauth = driver.FindElement(By.XPath("//*[@id='center-column']/p[5]")).Text.Replace("\r\n", " ");
                        TaxingAuthority = gc.Between(bulkTaxauth, "Collector of Revenue Office", "Email: propertytaxdept@stlouis-mo.gov").Trim();
                    }
                    catch { }
                    // Property Details
                    driver.Navigate().GoToUrl("https://www.stlouis-mo.gov/data/address-search/index.cfm");
                    Thread.Sleep(4000);
                    gc.CreatePdf_WOP(orderNumber, "Home page", driver, "MO", "St Louis City");

                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, "", ownername, address, "MO", "St Louis City");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_StLouisCityMO"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";

                    }
                    if (searchType == "address")
                    {
                        driver.FindElement(By.XPath("//*[@id='streetForm']/p[1]/input")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "MO", "St Louis City");
                        driver.FindElement(By.XPath("//*[@id='streetForm']/p[4]/input")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "MO", "St Louis City");
                        try
                        {
                            IWebElement add_search = driver.FindElement(By.XPath("//*[@id='CS_CCF_31321_31340']/div/div/table/tbody"));
                            IList<IWebElement> TRadd_search = add_search.FindElements(By.TagName("tr"));
                            IList<IWebElement> THadd_search = add_search.FindElements(By.TagName("th"));
                            IList<IWebElement> TDadd_search;
                            foreach (IWebElement row in TRadd_search)
                            {
                                TDadd_search = row.FindElements(By.TagName("td"));
                                if (TRadd_search.Count > 2 && TDadd_search.Count != 0 && THadd_search.Count == 3)
                                {
                                    string parcel_id = TDadd_search[0].Text;
                                    string Address = TDadd_search[1].Text;
                                    string ownername1 = TDadd_search[2].Text;
                                    string AddressDetails = Address + "~" + ownername1;

                                    gc.insert_date(orderNumber, parcel_id, 851, AddressDetails, 1, DateTime.Now);
                                }
                                if (TRadd_search.Count == 2)
                                {

                                }
                            }
                            if (TRadd_search.Count < 27 && TRadd_search.Count > 2 && THadd_search.Count == 3)
                            {
                                gc.CreatePdf_WOP(orderNumber, "Multi Address search result", driver, "MO", "St Louis City");
                                HttpContext.Current.Session["multiparcel_St_Louis_CityMO"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (TRadd_search.Count >= 27 && TRadd_search.Count > 2)
                            {
                                HttpContext.Current.Session["multiParcel_St_Louis_CityMO_Multicount"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }

                        }
                        catch { }



                    }

                    else if (searchType == "parcel")
                    {
                        driver.FindElement(By.XPath("//*[@id='streetForm']/p[1]/input")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "parcel search", driver, "MO", "St Louis City");
                        driver.FindElement(By.XPath("//*[@id='streetForm']/p[4]/input")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcelNumber, "parcel search Result", driver, "MO", "St Louis City");

                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.Id("content"));
                        if (INodata.Text.Contains("No record was found"))
                        {
                            HttpContext.Current.Session["Nodata_StLouisCityMO"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    string bulkdata = driver.FindElement(By.XPath("//*[@id='CS_CCF_31321_31340']/div[2]/div[1]/div/div/table/tbody")).Text;
                    string primary_address = "", owner_name = "", parcel_no = "", neighborhood = "", ward = "", land_use = "", property_desc = "";
                    primary_address = gc.Between(bulkdata, "Primary Address", "Owner name").Trim();
                    owner_name = gc.Between(bulkdata, "Owner name", "Parcel").Trim().Replace(":", "");
                    parcel_no = gc.Between(bulkdata, "Parcel", "Neighborhood").Trim();
                    neighborhood = gc.Between(bulkdata, "Neighborhood", "Ward").Trim();
                    ward = gc.Between(bulkdata, "Ward", "Land use").Trim().Replace(":", "");
                    land_use = gc.Between(bulkdata, "Land use", "Property description").Trim().Replace(":", "");
                    property_desc = GlobalClass.After(bulkdata, "Property description").Trim().Replace(":", "").Replace("Not meant for use in recorded legal documents", "").Replace("\r\n", "");

                    // address search
                    try
                    {
                        string bulktext2 = driver.FindElement(By.XPath("//*[@id='CS_CCF_31321_31340']/div[3]/div[1]/div/div/table/tbody")).Text;
                        string mailing_address2 = "", property_address2 = "", zip_code2 = "", year_built2 = "";
                        mailing_address2 = gc.Between(bulktext2, "Owner mailing address", "Property address").Trim().Replace(":", "");
                        property_address2 = gc.Between(bulktext2, "Property address", "Zip code").Trim().Replace(":", "");
                        zip_code2 = gc.Between(bulktext2, "Zip code", "Parcel number").Trim().Replace(":", "");
                        year_built2 = GlobalClass.After(bulktext2, "Year built").Trim();

                        string bulkinfo2 = driver.FindElement(By.XPath("//*[@id='CS_CCF_31321_31340']/div[4]/div[1]/div/div/table/tbody")).Text;
                        string condominium2 = "", numberofunits2 = "", frontage2 = "", land_area2 = "";
                        condominium2 = gc.Between(bulkinfo2, "Condominium", "Number of units").Trim().Replace(":", "");
                        numberofunits2 = gc.Between(bulkinfo2, "Number of units", "Frontage").Trim().Replace(":", "");
                        frontage2 = gc.Between(bulkinfo2, "Frontage", "Land area").Trim().Replace(":", "");
                        land_area2 = gc.Between(bulkinfo2, "Land area", "Property description").Trim().Replace(":", "");

                        string bulktxt2 = driver.FindElement(By.XPath("//*[@id='CS_CCF_31321_31340']/div[4]/div[2]/div/div/table/tbody")).Text;
                        string class_code2 = "", zoning2 = "", redevelopment_code2 = "", vacant_lot2 = "";
                        class_code2 = gc.Between(bulktxt2, "Class code", "Land use").Trim().Replace(":", "");
                        zoning2 = gc.Between(bulktxt2, "Zoning", "Redevelopment code").Trim().Replace(":", "");
                        redevelopment_code2 = gc.Between(bulktxt2, "Redevelopment code", "Vacant lot").Trim().Replace(":", "");
                        vacant_lot2 = GlobalClass.After(bulktxt2, "Vacant lot").Trim().Replace(":", "");
                        string propertydetails = primary_address + "~" + owner_name + "~" + mailing_address2 + "~" + neighborhood + "~" + ward + "~" + land_use + "~" + property_desc + "~" + property_address2 + "~" + zip_code2 + "~" + year_built2 + "~" + condominium2 + "~" + numberofunits2 + "~" + frontage2 + "~" + land_area2 + "~" + class_code2 + "~" + zoning2 + "~" + redevelopment_code2 + "~" + vacant_lot2;
                        gc.insert_date(orderNumber, parcel_no, 805, propertydetails, 1, DateTime.Now);
                    }
                    catch (Exception ex) { }

                    // Parcel search
                    try
                    {
                        string bulktext = driver.FindElement(By.XPath("//*[@id='CS_CCF_31321_31340']/div[3]/div[1]/div/div/table/tbody")).Text;
                        string mailing_address = "", property_address = "", zip_code = "", year_built = "";
                        mailing_address = gc.Between(bulktext, "Owner mailing address", "Property address").Trim().Replace(":", "");
                        property_address = gc.Between(bulktext, "Property address", "Zip code").Trim().Replace(":", "");
                        zip_code = gc.Between(bulktext, "Zip code", "Parcel number").Trim().Replace(":", "");
                        year_built = GlobalClass.After(bulktext, "Year built").Trim();

                        string bulkinfo = driver.FindElement(By.XPath("//*[@id='cs_control_31340']/div[2]/div[1]/div/div/table/tbody")).Text;
                        string condominium = "", numberofunits = "", frontage = "", land_area = "";
                        condominium = gc.Between(bulkinfo, "Condominium", "Number of units").Trim().Replace(":", "");
                        numberofunits = gc.Between(bulkinfo, "Number of units", "Frontage").Trim().Replace(":", "");
                        frontage = gc.Between(bulkinfo, "Frontage", "Land area").Trim().Replace(":", "");
                        land_area = gc.Between(bulkinfo, "Land area", "Property description").Trim().Replace(":", "");

                        string bulktxt = driver.FindElement(By.XPath("//*[@id='cs_control_31340']/div[2]/div[2]/div/div/table/tbody")).Text;
                        string class_code = "", zoning = "", redevelopment_code = "", vacant_lot = "";
                        class_code = gc.Between(bulktxt, "Class code", "Land use").Trim().Replace(":", "");
                        zoning = gc.Between(bulktxt, "Zoning", "Redevelopment code").Trim().Replace(":", "");
                        redevelopment_code = gc.Between(bulktxt, "Redevelopment code", "Vacant lot").Trim().Replace(":", "");
                        vacant_lot = GlobalClass.After(bulktxt, "Vacant lot").Trim().Replace(":", "");
                        string propertydetails = primary_address + "~" + owner_name + "~" + mailing_address + "~" + neighborhood + "~" + ward + "~" + land_use + "~" + property_desc + "~" + property_address + "~" + zip_code + "~" + year_built + "~" + condominium + "~" + numberofunits + "~" + frontage + "~" + land_area + "~" + class_code + "~" + zoning + "~" + redevelopment_code + "~" + vacant_lot;
                        gc.insert_date(orderNumber, parcel_no, 805, propertydetails, 1, DateTime.Now);
                    }
                    catch (Exception ex) { }


                    // Delinquent search
                    try
                    {
                        string bulktext1 = driver.FindElement(By.XPath("//*[@id='CS_CCF_31321_31340']/div[3]/div/table/tbody")).Text;
                        string mailing_address1 = "", property_address1 = "", zip_code1 = "", year_built1 = "";
                        mailing_address1 = gc.Between(bulktext1, "Owner mailing address", "Property address").Trim().Replace(":", "");
                        property_address1 = gc.Between(bulktext1, "Property address", "Zip code").Trim().Replace(":", "");
                        zip_code1 = gc.Between(bulktext1, "Zip code", "Parcel number").Trim().Replace(":", "");
                        year_built1 = GlobalClass.After(bulktext1, "Year built").Trim();

                        string bulkinfo1 = driver.FindElement(By.XPath("//*[@id='CS_CCF_31321_31340']/div[4]/div[1]/div/div/table/tbody")).Text;
                        string condominium1 = "", numberofunits1 = "", frontage1 = "", land_area1 = "";
                        condominium1 = gc.Between(bulkinfo1, "Condominium", "Number of units").Trim().Replace(":", "");
                        numberofunits1 = gc.Between(bulkinfo1, "Number of units", "Frontage").Trim().Replace(":", "");
                        frontage1 = gc.Between(bulkinfo1, "Frontage", "Land area").Trim().Replace(":", "");
                        land_area1 = gc.Between(bulkinfo1, "Land area", "Property description").Trim().Replace(":", "");

                        string bulktxt1 = driver.FindElement(By.XPath("//*[@id='CS_CCF_31321_31340']/div[4]/div[2]/div/div/table/tbody")).Text;
                        string class_code1 = "", zoning1 = "", redevelopment_code1 = "", vacant_lot1 = "";
                        class_code1 = gc.Between(bulktxt1, "Class code", "Land use").Trim().Replace(":", "");
                        zoning1 = gc.Between(bulktxt1, "Zoning", "Redevelopment code").Trim().Replace(":", "");
                        redevelopment_code1 = gc.Between(bulktxt1, "Redevelopment code", "Vacant lot").Trim().Replace(":", "");
                        vacant_lot1 = GlobalClass.After(bulktxt1, "Vacant lot").Trim().Replace(":", "");
                        string propertydetails = primary_address + "~" + owner_name + "~" + mailing_address1 + "~" + neighborhood + "~" + ward + "~" + land_use + "~" + property_desc + "~" + property_address1 + "~" + zip_code1 + "~" + year_built1 + "~" + condominium1 + "~" + numberofunits1 + "~" + frontage1 + "~" + land_area1 + "~" + class_code1 + "~" + zoning1 + "~" + redevelopment_code1 + "~" + vacant_lot1;
                        gc.insert_date(orderNumber, parcel_no, 805, propertydetails, 1, DateTime.Now);
                    }
                    catch (Exception ex) { }

                    // Assessment Details Address search
                    string Currentassessedvalue = "", AssessTitle = "", AssessValue = "", AssessType = "";
                    try
                    {
                        IWebElement Iassess = driver.FindElement(By.XPath("//*[@id='CS_CCF_31321_31340']/div[6]/div[1]/div/div/table"));
                        IList<IWebElement> TRassessCap = Iassess.FindElements(By.TagName("caption"));
                        foreach (IWebElement row in TRassessCap)
                        {
                            if (row.Text != "")
                            {
                                Currentassessedvalue = row.Text;
                            }
                        }
                        IList<IWebElement> TRassess = Iassess.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDassess;
                        IList<IWebElement> THassess;
                        foreach (IWebElement row in TRassess)
                        {
                            TDassess = row.FindElements(By.TagName("td"));
                            THassess = row.FindElements(By.TagName("th"));
                            if (TDassess.Count != 0 && TDassess.Count > 1)
                            {
                                AssessType = TDassess[0].Text;
                            }
                            if (TDassess.Count != 0 && TDassess.Count == 1)
                            {
                                AssessValue += TDassess[0].Text + "~";
                            }
                            if (THassess.Count != 0 && THassess.Count == 1)
                            {
                                AssessTitle += THassess[0].Text.Replace(":", "") + "~";
                            }
                        }
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + "Year Type" + "~" + "Assessment Type" + "~" + AssessTitle.Remove(AssessTitle.Length - 1, 1) + "' where Id = '" + 833 + "'");
                        gc.insert_date(orderNumber, parcel_no, 833, Currentassessedvalue + "~" + AssessType + "~" + AssessValue.Remove(AssessValue.Length - 1, 1), 1, DateTime.Now);
                    }
                    catch { }

                    string priorassessedvalue = "", AssessTitle1 = "", AssessValue1 = "", AssessType1 = "";
                    try
                    {
                        IWebElement Iassessment = driver.FindElement(By.XPath("//*[@id='CS_CCF_31321_31340']/div[6]/div[2]/div/div/table"));
                        IList<IWebElement> TRassessCap1 = Iassessment.FindElements(By.TagName("caption"));
                        foreach (IWebElement row in TRassessCap1)
                        {
                            if (row.Text != "")
                            {
                                priorassessedvalue = row.Text;
                            }
                        }
                        IList<IWebElement> TRassess1 = Iassessment.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDassess1;
                        IList<IWebElement> THassess1;
                        foreach (IWebElement row in TRassess1)
                        {
                            TDassess1 = row.FindElements(By.TagName("td"));
                            THassess1 = row.FindElements(By.TagName("th"));
                            if (TDassess1.Count != 0 && TDassess1.Count > 1)
                            {
                                AssessType1 = TDassess1[0].Text;
                            }
                            if (TDassess1.Count != 0 && TDassess1.Count == 1)
                            {
                                AssessValue1 += TDassess1[0].Text + "~";
                            }
                            if (THassess1.Count != 0 && THassess1.Count == 1)
                            {
                                AssessTitle1 += THassess1[0].Text.Replace(":", "") + "~";
                            }
                        }

                        gc.insert_date(orderNumber, parcel_no, 833, priorassessedvalue + "~" + AssessType1 + "~" + AssessValue1.Remove(AssessValue1.Length - 1, 1), 1, DateTime.Now);
                    }
                    catch { }

                    // Assessment Details Parcel search
                    string Currentassessedvalue1 = "", AssessTitle2 = "", AssessValue2 = "", AssessType2 = "";
                    try
                    {
                        IWebElement Iassess2 = driver.FindElement(By.XPath("//*[@id='cs_control_31340']/div[4]/div[1]/div/div/table"));
                        IList<IWebElement> TRassessCap2 = Iassess2.FindElements(By.TagName("caption"));
                        foreach (IWebElement row in TRassessCap2)
                        {
                            if (row.Text != "")
                            {
                                Currentassessedvalue1 = row.Text;
                            }
                        }
                        IList<IWebElement> TRassess2 = Iassess2.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDassess2;
                        IList<IWebElement> THassess2;
                        foreach (IWebElement row in TRassess2)
                        {
                            TDassess2 = row.FindElements(By.TagName("td"));
                            THassess2 = row.FindElements(By.TagName("th"));
                            if (TDassess2.Count != 0 && TDassess2.Count > 1)
                            {
                                AssessType2 = TDassess2[0].Text;
                            }
                            if (TDassess2.Count != 0 && TDassess2.Count == 1)
                            {
                                AssessValue2 += TDassess2[0].Text + "~";
                            }
                            if (THassess2.Count != 0 && THassess2.Count == 1)
                            {
                                AssessTitle2 += THassess2[0].Text.Replace(":", "") + "~";
                            }
                        }
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + "Year Type" + "~" + "Assessment Type" + "~" + AssessTitle2.Remove(AssessTitle2.Length - 1, 1) + "' where Id = '" + 833 + "'");
                        gc.insert_date(orderNumber, parcel_no, 833, Currentassessedvalue1 + "~" + AssessType2 + "~" + AssessValue2.Remove(AssessValue2.Length - 1, 1), 1, DateTime.Now);
                    }
                    catch { }

                    string priorassessedvalue1 = "", AssessTitle3 = "", AssessValue3 = "", AssessType3 = "";
                    try
                    {
                        IWebElement Iassessment1 = driver.FindElement(By.XPath("//*[@id='cs_control_31340']/div[4]/div[2]/div/div/table"));
                        IList<IWebElement> TRassessCap3 = Iassessment1.FindElements(By.TagName("caption"));
                        foreach (IWebElement row in TRassessCap3)
                        {
                            if (row.Text != "")
                            {
                                priorassessedvalue1 = row.Text;
                            }
                        }
                        IList<IWebElement> TRassess3 = Iassessment1.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDassess3;
                        IList<IWebElement> THassess3;
                        foreach (IWebElement row in TRassess3)
                        {
                            TDassess3 = row.FindElements(By.TagName("td"));
                            THassess3 = row.FindElements(By.TagName("th"));
                            if (TDassess3.Count != 0 && TDassess3.Count > 1)
                            {
                                AssessType3 = TDassess3[0].Text;
                            }
                            if (TDassess3.Count != 0 && TDassess3.Count == 1)
                            {
                                AssessValue3 += TDassess3[0].Text + "~";
                            }
                            if (THassess3.Count != 0 && THassess3.Count == 1)
                            {
                                AssessTitle3 += THassess3[0].Text.Replace(":", "") + "~";
                            }
                        }

                        gc.insert_date(orderNumber, parcel_no, 833, priorassessedvalue1 + "~" + AssessType3 + "~" + AssessValue3.Remove(AssessValue3.Length - 1, 1), 1, DateTime.Now);
                    }
                    catch { }

                    // Tax Bill Address search
                    string TaxBillDetails = "", TaxBillDetails2 = "";
                    try
                    {
                        IWebElement taxbill = driver.FindElement(By.XPath("//*[@id='CS_CCF_31321_31340']/div[9]/div[1]/div/table/tbody"));
                        IList<IWebElement> TRtaxbill = taxbill.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDtaxbill;
                        IList<IWebElement> THtaxbill;
                        foreach (IWebElement row in TRtaxbill)
                        {
                            TDtaxbill = row.FindElements(By.TagName("td"));
                            THtaxbill = row.FindElements(By.TagName("th"));
                            if (TDtaxbill.Count != 0 && TDtaxbill.Count > 1 && !row.Text.Contains("Interest") && TDtaxbill.Count > 8)
                            {
                                TaxBillDetails = TDtaxbill[0].Text + "~" + TDtaxbill[1].Text + "~" + TDtaxbill[2].Text + "~" + TDtaxbill[3].Text + "~" + TDtaxbill[4].Text + "~" + TDtaxbill[5].Text + "~" + TDtaxbill[6].Text + "~" + TDtaxbill[7].Text + "~" + TDtaxbill[8].Text + "~" + TDtaxbill[9].Text + "~" + TDtaxbill[10].Text;
                                gc.insert_date(orderNumber, parcel_no, 839, TaxBillDetails + "~" + TaxingAuthority, 1, DateTime.Now);
                            }
                            if (TDtaxbill.Count != 0 && TDtaxbill.Count < 8)
                            {
                                TaxBillDetails2 = TDtaxbill[0].Text + "~" + TDtaxbill[1].Text + "~" + TDtaxbill[2].Text + "~" + TDtaxbill[3].Text + "~" + TDtaxbill[4].Text + "~" + TDtaxbill[5].Text + "~" + TDtaxbill[6].Text + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                gc.insert_date(orderNumber, parcel_no, 839, TaxBillDetails2 + "~" + TaxingAuthority, 1, DateTime.Now);
                            }
                        }


                    }
                    catch { }

                    // Tax Bill Parcel Search
                    string TaxBillDetails1 = "";
                    try
                    {
                        IWebElement taxbill1 = driver.FindElement(By.XPath("//*[@id='cs_control_31340']/div[7]/div[1]/div/table/tbody"));
                        IList<IWebElement> TRtaxbill1 = taxbill1.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDtaxbill1;
                        IList<IWebElement> THtaxbill1;
                        foreach (IWebElement row in TRtaxbill1)
                        {
                            TDtaxbill1 = row.FindElements(By.TagName("td"));
                            THtaxbill1 = row.FindElements(By.TagName("th"));
                            if (TDtaxbill1.Count != 0 && TDtaxbill1.Count > 1)
                            {
                                TaxBillDetails1 = TDtaxbill1[0].Text + "~" + TDtaxbill1[1].Text + "~" + TDtaxbill1[2].Text + "~" + TDtaxbill1[3].Text + "~" + TDtaxbill1[4].Text + "~" + TDtaxbill1[5].Text + "~" + TDtaxbill1[6].Text + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                gc.insert_date(orderNumber, parcel_no, 839, TaxBillDetails1 + "~" + TaxingAuthority, 1, DateTime.Now);
                            }
                        }


                    }
                    catch { }

                    try
                    {
                        IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='CS_CCF_31321_31340']/p[4]"));
                        if (multiaddress.Text.Contains("please contact the Collector of Revenue Office") && !multiaddress.Text.Contains("$") && !multiaddress.Text.Contains("Total Amount Due For this Account:"))
                        {
                            HttpContext.Current.Session["Delinquent_St_Louis_CityMO"] = "Yes";
                        }
                    }
                    catch { }



                    CitytaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "MO", "St Louis City", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "MO", "St Louis City");
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