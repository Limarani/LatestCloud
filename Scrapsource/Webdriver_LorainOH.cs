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
    public class Webdriver_LorainOH
    {

        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();

        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_Lorain(string address, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
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

                    driver.Navigate().GoToUrl("http://www.loraincountyauditor.com/gis/");
                    Thread.Sleep(7000);

                    if (searchType == "titleflex")
                    {
                        string titleaddress = address;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "OH", "Lorain");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_LorainOH"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";

                    }

                    if (searchType == "address")
                    {
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='touring']/div[1]/div[3]/div/button[2]")).SendKeys(Keys.Enter);
                            Thread.Sleep(4000);
                        }
                        catch { }
                        driver.FindElement(By.XPath("//*[@id='bha_FullTextSearchBox_3']/input")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "OH", "Lorain");

                        driver.FindElement(By.XPath("//*[@id='restconfigsearches_1']/table/tfoot/tr/td[1]/button")).Click();
                        Thread.Sleep(5000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search result", driver, "OH", "Lorain");
                        try
                        {
                            driver.FindElement(By.Id("open-my-report")).Click();
                            Thread.Sleep(4000);
                        }
                        catch { }
                        try
                        {
                            IWebElement Iparcelnum = driver.FindElement(By.XPath("//*[@id='widget-parcel-report']/div[2]/div/div[2]/div/table[1]/tbody/tr[1]/td[2]/a"));
                            parcelNumber = Iparcelnum.Text.Trim();
                            driver.Navigate().GoToUrl("http://www.loraincountyauditor.com/gis/report/Report.aspx?pin=" + parcelNumber + "");
                            Thread.Sleep(4000);
                        }
                        catch { }
                        try
                        {
                            IWebElement multiaddCheck = driver.FindElement(By.XPath("//*[@id='widget-query-results']/div[2]/ul"));
                            if (multiaddCheck.Text != "")
                            {
                                try
                                {
                                    string strowner = "", strAddress = "";
                                    IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='widget-query-results']/div[2]/ul"));
                                    IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                                    IList<IWebElement> multiTD;
                                    foreach (IWebElement multi in multiRow)
                                    {
                                        multiTD = multi.FindElements(By.TagName("td"));

                                        if (multiTD.Count != 0 && multiRow.Count > 25)
                                        {
                                            HttpContext.Current.Session["multiparcel_LorainOH_Maximum"] = "Maximum";
                                            driver.Quit();
                                            return "Maximum";
                                        }
                                        if (multiTD.Count != 0 && multiRow.Count > 2 && multiRow.Count <= 25 && multi.Text != "" && !multi.Text.Contains("Mobile Home") && !multi.Text.Contains("Sale Date") && !multi.Text.Contains("Sale Price"))
                                        {
                                            if (multi.Text.Contains("Parcel Number"))
                                            {
                                                parcelNumber = multiTD[2].Text;
                                            }
                                            if (multi.Text.Contains("Owner Name"))
                                            {
                                                strowner = multiTD[1].Text;
                                            }
                                            if (multi.Text.Contains("Address"))
                                            {
                                                strAddress = multiTD[1].Text;
                                            }
                                            if (multi.Text.Contains("Address"))
                                            {
                                                string multidetails = strowner + "~" + strAddress;
                                                gc.insert_date(orderNumber, parcelNumber, 1229, multidetails, 1, DateTime.Now);
                                            }
                                        }
                                    }
                                    if (multiRow.Count > 2 && multiRow.Count <= 25)
                                    {
                                        HttpContext.Current.Session["multiparcel_LorainOH"] = "Yes";
                                        driver.Quit();
                                        return "MultiParcel";
                                    }
                                }
                                catch { }
                            }
                        }
                        catch { }
                    }
                    else if (searchType == "parcel")
                    {
                        parcelNumber = parcelNumber.Trim();
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='touring']/div[1]/div[3]/div/button[2]")).SendKeys(Keys.Enter);
                            Thread.Sleep(4000);
                        }
                        catch { }
                        driver.FindElement(By.XPath("//*[@id='bha_FullTextSearchBox_0']/input")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "parcel search", driver, "OH", "Lorain");
                        driver.FindElement(By.XPath("//*[@id='restconfigsearches_1']/table/tfoot/tr/td[1]/button")).Click();
                        Thread.Sleep(5000);
                        gc.CreatePdf(orderNumber, parcelNumber, "parcel search Result", driver, "OH", "Lorain");
                        try
                        {
                            driver.FindElement(By.Id("open-my-report")).Click();
                            Thread.Sleep(4000);
                        }
                        catch { }

                        try
                        {
                            IWebElement Iparcelnum = driver.FindElement(By.XPath("//*[@id='widget-parcel-report']/div[2]/div/div[2]/div/table[1]/tbody/tr[1]/td[2]/a"));
                            parcelNumber = Iparcelnum.Text.Trim();
                            driver.Navigate().GoToUrl("http://www.loraincountyauditor.com/gis/report/Report.aspx?pin=" + parcelNumber + "");
                            Thread.Sleep(5000);
                        }
                        catch { }
                        try
                        {
                            IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='widget-query-results']/div[2]/ul"));
                            IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> multiTD;
                            foreach (IWebElement multi in multiRow)
                            {
                                multiTD = multi.FindElements(By.TagName("td"));

                                if (multiTD.Count != 0 && multiTD[2].Text == parcelNumber)
                                {
                                    parcelNumber = multiTD[2].Text;
                                    multiTD[2].Click();
                                    Thread.Sleep(2000);
                                    break;
                                }
                            }


                            try
                            {
                                driver.FindElement(By.Id("open-my-report")).Click();
                                Thread.Sleep(4000);
                            }
                            catch { }

                            try
                            {
                                IWebElement Iparcelnum = driver.FindElement(By.XPath("//*[@id='widget-parcel-report']/div[2]/div/div[2]/div/table[1]/tbody/tr[1]/td[2]/a"));
                                parcelNumber = Iparcelnum.Text.Trim();
                                driver.Navigate().GoToUrl("http://www.loraincountyauditor.com/gis/report/Report.aspx?pin=" + parcelNumber + "");
                                Thread.Sleep(4000);
                            }
                            catch { }
                        }
                        catch { }
                    }
                    else if (searchType == "ownername")
                    {
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='touring']/div[1]/div[3]/div/button[2]")).SendKeys(Keys.Enter);
                            Thread.Sleep(4000);
                        }
                        catch { }
                        driver.FindElement(By.XPath("//*[@id='bha_FullTextSearchBox_2']/input")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "OwnerName Search", driver, "OH", "Lorain");
                        driver.FindElement(By.XPath("//*[@id='restconfigsearches_1']/table/tfoot/tr/td[1]/button")).Click();
                        Thread.Sleep(5000);
                        gc.CreatePdf_WOP(orderNumber, "OwnerName Search Results", driver, "OH", "Lorain");
                        try
                        {
                            IWebElement multiaddCheck = driver.FindElement(By.XPath("//*[@id='widget-query-results']/div[2]/ul"));
                            if (multiaddCheck.Text != "")
                            {
                                try
                                {
                                    string strowner = "", strAddress = "";
                                    IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='widget-query-results']/div[2]/ul"));
                                    IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                                    IList<IWebElement> multiTD;
                                    foreach (IWebElement multi in multiRow)
                                    {
                                        multiTD = multi.FindElements(By.TagName("td"));

                                        if (multiTD.Count != 0 && multiRow.Count > 150)
                                        {
                                            HttpContext.Current.Session["multiparcel_LorainOH_Maximum"] = "Maximum";
                                            driver.Quit();
                                            return "Maximum";
                                        }
                                        if (multiTD.Count != 0 && multiRow.Count > 2 && multiRow.Count <= 150 && multi.Text != "" && !multi.Text.Contains("Mobile Home") && !multi.Text.Contains("Sale Date") && !multi.Text.Contains("Sale Price"))
                                        {
                                            if (multi.Text.Contains("Parcel Number"))
                                            {
                                                parcelNumber = multiTD[2].Text;
                                            }
                                            if (multi.Text.Contains("Owner Name"))
                                            {
                                                strowner = multiTD[1].Text;
                                            }
                                            if (multi.Text.Contains("Address"))
                                            {
                                                strAddress = multiTD[1].Text;
                                            }
                                            if (multi.Text.Contains("Address"))
                                            {
                                                string multidetails = strowner + "~" + strAddress;
                                                gc.insert_date(orderNumber, parcelNumber, 1229, multidetails, 1, DateTime.Now);
                                            }
                                        }
                                    }
                                    if (multiRow.Count > 2 && multiRow.Count <= 150)
                                    {
                                        HttpContext.Current.Session["multiparcel_LorainOH"] = "Yes";
                                        driver.Quit();
                                        return "MultiParcel";
                                    }
                                }
                                catch { }
                            }
                        }
                        catch { }
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.XPath("//*[@id='widget-query-results']/div[2]/div"));
                        if(INodata.Text.Contains("no matches"))
                        {
                            HttpContext.Current.Session["Nodata_LorainOH"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    //property details
                    string bulkdata = "";
                    string owner_name = "", LocationAddress = "", Taxbill = "", PropertyDesc = "", Taxdistrict = "", Landuse = "", Neighbourhood = "", Acres = "", SchoolDistrict = "", DelinRealEstate = "", yearbuilt = "";
                    try
                    {
                        bulkdata = driver.FindElement(By.XPath("//*[@id='form1']/div[6]/table/tbody")).Text;
                    }
                    catch { }
                    try
                    {
                        bulkdata = driver.FindElement(By.XPath("//*[@id='form1']/div[7]/table/tbody")).Text;
                    }
                    catch { }

                    parcelNumber = gc.Between(bulkdata, "Parcel Number", "Land Use").Trim();
                    owner_name = gc.Between(bulkdata, "Owner", "Neighborhood").Trim();
                    LocationAddress = gc.Between(bulkdata, "Location Address", "Acres").Trim();
                    Taxbill = gc.Between(bulkdata, "Tax Bill Mailed To", "School District").Trim();
                    PropertyDesc = gc.Between(bulkdata, "Property Description", "Instrument Number").Trim();
                    Taxdistrict = gc.Between(bulkdata, "Tax District", "Delinquent Real Estate").Trim();
                    Landuse = gc.Between(bulkdata, "Land Use", "Owner").Trim();
                    Neighbourhood = gc.Between(bulkdata, "Neighborhood", "Location Address").Trim();
                    Acres = gc.Between(bulkdata, "Acres", "Tax Bill Mailed To").Trim();
                    SchoolDistrict = gc.Between(bulkdata, "School District", "Property Description").Trim();
                    DelinRealEstate = GlobalClass.After(bulkdata, "Delinquent Real Estate").Trim();
                    try
                    {
                        IWebElement Iyearbuilt = driver.FindElement(By.XPath("//*[@id='form1']/div[11]/table/tbody/tr[2]/td[2]"));
                        yearbuilt = Iyearbuilt.Text;
                    }
                    catch { }

                    gc.CreatePdf(orderNumber, parcelNumber, "Property Details", driver, "OH", "Lorain");

                    string propertydetails = owner_name + "~" + LocationAddress + "~" + Taxbill + "~" + PropertyDesc + "~" + Taxdistrict + "~" + Landuse + "~" + Neighbourhood + "~" + Acres + "~" + SchoolDistrict + "~" + DelinRealEstate + "~" + yearbuilt;
                    gc.insert_date(orderNumber, parcelNumber, 1216, propertydetails, 1, DateTime.Now);

                    // Assessment Value Details

                    string Title = "", Value = "";
                    IWebElement Assessmentdetails = driver.FindElement(By.XPath("//*[@id='form1']/div[8]/table/tbody"));
                    IList<IWebElement> TRAssessmentdetails = Assessmentdetails.FindElements(By.TagName("tr"));
                    IList<IWebElement> THAssessmentdetails = Assessmentdetails.FindElements(By.TagName("td"));
                    IList<IWebElement> TDAssessmentdetails;
                    foreach (IWebElement row in TRAssessmentdetails)
                    {
                        TDAssessmentdetails = row.FindElements(By.TagName("td"));
                        if (TDAssessmentdetails.Count != 0 && row.Text.Trim() != "")
                        {
                            Title += TDAssessmentdetails[0].Text + "~";
                            Value += TDAssessmentdetails[1].Text + "~";

                        }
                    }


                    db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + Title.Remove(Title.Length - 1, 1) + "' where Id = '" + 1217 + "'");
                    gc.insert_date(orderNumber, parcelNumber, 1217, Value.Remove(Value.Length - 1, 1), 1, DateTime.Now);

                    // Current Taxes Details

                    string currenttax = driver.FindElement(By.XPath("//*[@id='form1']/div[9]/table/tbody")).Text;
                    string Grossfulltax = "", Statecredit = "", Subtotal = "", non_business_credit = "", owner_occu_Credit = "", Homestead_Credit = "", NetTax = "", SpecialAssess = "";
                    string Delin_spl_Assess = "", UnpaidTaxes = "", TotalTaxes_owed = "", Total_taxes_paid = "", SplAssessment = "", TaxAuth = "";

                    Grossfulltax = gc.Between(currenttax, "Gross Full Year Tax", "Special Assessment").Trim();
                    Statecredit = gc.Between(currenttax, "State Credit", "Delinquent Special Assessment").Trim();
                    Subtotal = gc.Between(currenttax, "Subtotal", "Unpaid Taxes").Trim();
                    non_business_credit = gc.Between(currenttax, "Non-Business Credit", "Total Taxes Owed").Trim();
                    owner_occu_Credit = gc.Between(currenttax, "Owner Occupancy Credit", "Total Taxes Paid").Trim();
                    Homestead_Credit = gc.Between(currenttax, "Homestead Credit", "Special Assessments").Trim();
                    NetTax = GlobalClass.After(currenttax, "Net Tax").Trim();
                    SpecialAssess = gc.Between(currenttax, "Special Assessment", "State Credit").Trim();
                    Delin_spl_Assess = gc.Between(currenttax, "Delinquent Special Assessment", "Subtotal").Trim();
                    UnpaidTaxes = gc.Between(currenttax, "Unpaid Taxes", "Non-Business Credit").Trim();
                    TotalTaxes_owed = gc.Between(currenttax, "Total Taxes Owed", "Owner Occupancy Credit").Trim();
                    Total_taxes_paid = gc.Between(currenttax, "Total Taxes Paid", "Homestead Credit").Trim();
                    SplAssessment = gc.Between(currenttax, "Special Assessments", "Net Tax").Trim();
                    TaxAuth = "Lorain County Auditor 226 Middle Ave., 2nd Floor Elyria, OH 44035 (440) 329-5787";

                    string currenttaxdetails = Grossfulltax + "~" + Statecredit + "~" + Subtotal + "~" + non_business_credit + "~" + owner_occu_Credit + "~" + Homestead_Credit + "~" + NetTax + "~" + SpecialAssess + "~" + Delin_spl_Assess + "~" + UnpaidTaxes + "~" + TotalTaxes_owed + "~" + Total_taxes_paid + "~" + SplAssessment + "~" + TaxAuth;
                    gc.insert_date(orderNumber, parcelNumber, 1218, currenttaxdetails, 1, DateTime.Now);

                    // Alert Comments
                    try
                    {
                        if (SplAssessment.Contains("Yes"))
                        {
                            string alertmessage = "Yes, contact the Auditor’s Office at 440-329-5212 for details.";
                            gc.insert_date(orderNumber, parcelNumber, 1232, alertmessage, 1, DateTime.Now);
                        }

                    }
                    catch { }

                    // Special Assessments Tax Details
                    try
                    {
                        IWebElement Itaxyear = driver.FindElement(By.XPath("//*[@id='form1']/div[17]/table/tbody/tr[1]/td"));
                        string Taxyear = "", Taxyear1 = "", AssessTitle = "", AssessValue = "";
                        Taxyear = Itaxyear.Text.Trim();
                        string[] Taxyear2 = Taxyear.Split();
                        Taxyear1 = Taxyear2[0];
                        IWebElement SPlAssessmentdetails = driver.FindElement(By.XPath("//*[@id='form1']/div[17]/table/tbody"));
                        IList<IWebElement> TRSPLAssessmentdetails = SPlAssessmentdetails.FindElements(By.TagName("tr"));
                        IList<IWebElement> THSPLAssessmentdetails = SPlAssessmentdetails.FindElements(By.TagName("td"));
                        IList<IWebElement> TDSPLAssessmentdetails;
                        foreach (IWebElement row in TRSPLAssessmentdetails)
                        {
                            TDSPLAssessmentdetails = row.FindElements(By.TagName("td"));
                            if (TDSPLAssessmentdetails.Count == 1 && row.Text.Trim() != "" && row.Text.Contains("Year"))
                            {
                                AssessTitle += Taxyear1 + "~";
                                AssessValue += TDSPLAssessmentdetails[0].Text.Replace("Year", "").Trim() + "~";
                            }
                            if (TDSPLAssessmentdetails.Count > 1 && row.Text.Trim() != "")
                            {
                                AssessTitle += TDSPLAssessmentdetails[0].Text + "~";
                                AssessValue += TDSPLAssessmentdetails[1].Text + "~";

                            }
                            if (row.Text.Contains("Total Charge"))
                            {
                                db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + AssessTitle.Remove(AssessTitle.Length - 1, 1) + "' where Id = '" + 1219 + "'");
                                gc.insert_date(orderNumber, parcelNumber, 1219, AssessValue.Remove(AssessValue.Length - 1, 1), 1, DateTime.Now);
                                AssessTitle = "";
                                AssessValue = "";
                            }
                        }
                    }
                    catch { }

                    // Tax History Details Table

                    string TaxHistory = driver.FindElement(By.XPath("//*[@id='form1']/div[15]/table/tbody")).Text;
                    int i1 = 1;
                    string Tax_Year = "", strtaxyear = "", strtaxyear1 = "", Grossfulltax1 = "", Statecredit1 = "", Subtotal1 = "", non_business_credit1 = "", owner_occu_Credit1 = "", Homestead_Credit1 = "", NetTax1 = "", SpecialAssess1 = "";
                    string Delin_spl_Assess1 = "", UnpaidTaxes1 = "", TotalTaxes_owed1 = "", Total_taxes_paid1 = "";

                    IWebElement tbmulti2;

                    tbmulti2 = driver.FindElement(By.XPath("//*[@id='form1']/div[15]/table/tbody"));
                    IList<IWebElement> TRmulti2 = tbmulti2.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDmulti2;
                    foreach (IWebElement row in TRmulti2)
                    {
                        TDmulti2 = row.FindElements(By.TagName("td"));

                        if (TDmulti2.Count != 0 && row.Text.Trim() != "")
                        {

                            if (i1 == 1 || i1 == 8 || row.Text.Contains("Tax Year"))
                            {
                                Tax_Year = TDmulti2[0].Text;
                                string[] taxyear = Tax_Year.Split();
                                strtaxyear = taxyear[0] + " " + taxyear[1];
                                strtaxyear1 = taxyear[2];

                            }
                            if (i1 == 2 || i1 == 9 || row.Text.Contains("Gross Full Year Tax"))
                            {
                                Grossfulltax1 = TDmulti2[1].Text;
                                NetTax1 = TDmulti2[3].Text;
                            }
                            if (i1 == 3 || row.Text.Contains("State Credit"))
                            {
                                Statecredit1 = TDmulti2[1].Text;
                                SpecialAssess1 = TDmulti2[3].Text;
                            }
                            if (i1 == 4 || row.Text.Contains("Subtotal"))
                            {
                                Subtotal1 = TDmulti2[1].Text;
                                Delin_spl_Assess1 = TDmulti2[3].Text;
                            }
                            if (i1 == 5 || row.Text.Contains("Non-Business Credit"))
                            {
                                non_business_credit1 = TDmulti2[1].Text;
                                UnpaidTaxes1 = TDmulti2[3].Text;
                            }
                            if (i1 == 6 || row.Text.Contains("Owner Occupancy Credit"))
                            {
                                owner_occu_Credit1 = TDmulti2[1].Text;
                                TotalTaxes_owed1 = TDmulti2[3].Text;
                            }
                            if (i1 == 7 || row.Text.Contains("Homestead Credit"))
                            {
                                Homestead_Credit1 = TDmulti2[1].Text;
                                Total_taxes_paid1 = TDmulti2[3].Text;
                            }
                            if (row.Text.Contains("Total Taxes Paid") && Grossfulltax1 != "" && Total_taxes_paid1 != "" && non_business_credit1 != "" && TotalTaxes_owed1 != "")
                            {
                                string TaxHistorydetails = strtaxyear1 + "~" + Grossfulltax1 + "~" + Statecredit1 + "~" + Subtotal1 + "~" + non_business_credit1 + "~" + owner_occu_Credit1 + "~" + Homestead_Credit1 + "~" + NetTax1 + "~" + SpecialAssess1 + "~" + Delin_spl_Assess1 + "~" + UnpaidTaxes1 + "~" + TotalTaxes_owed1 + "~" + Total_taxes_paid1;
                                gc.insert_date(orderNumber, parcelNumber, 1223, TaxHistorydetails, 1, DateTime.Now);
                            }
                            i1++;
                        }

                    }

                    // Tax Payments



                    IWebElement TaxInfo = driver.FindElement(By.XPath("//*[@id='form1']/div[16]/table"));
                    IList<IWebElement> TRTaxInfo = TaxInfo.FindElements(By.TagName("tr"));
                    IList<IWebElement> THTaxInfo = TaxInfo.FindElements(By.TagName("th"));
                    IList<IWebElement> TDTaxInfo;
                    foreach (IWebElement row in TRTaxInfo)
                    {
                        TDTaxInfo = row.FindElements(By.TagName("td"));
                        if (TRTaxInfo.Count > 1 && TDTaxInfo.Count != 0 && row.Text.Trim() != "")
                        {
                            string TaxPayDetails = TDTaxInfo[0].Text + "~" + TDTaxInfo[1].Text + "~" + TDTaxInfo[2].Text + "~" + TDTaxInfo[3].Text + "~" + TDTaxInfo[4].Text + "~" + TDTaxInfo[5].Text;

                            gc.insert_date(orderNumber, parcelNumber, 1228, TaxPayDetails, 1, DateTime.Now);
                        }

                    }










                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "OH", "Lorain", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "OH", "Lorain");
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