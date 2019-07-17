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
    public class WebDriver_GADekalp
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        string Outparcelno = "", outputPath = "", strtaxYear = "";
        int taxYear = 0;
        string strparcelNumber = "", tax_District = "", strOwnerName = "-", strTaxYear = "-", strReceipt = "-", strTaxInstallment = "-", strDueDate = "-", strTotalTax = "",
               strPaid = "-", strPaidDate = "-", strStatus = "-", strTotalDue = "-", strPropertyId = "-", strtaxstatus = "";
        IWebElement tableAss1;
        public string FTP_Dekalb(string address, string parcelNumber, string owner_name, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new PhantomJSDriver();
            //driver = new ChromeDriver();
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("http://taxcommissioner.dekalbcountyga.gov/PropertyAppraisal/realSearch.asp");
                    if (searchType == "address")
                    {
                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/form/table[1]/tbody/tr[4]/td[2]/input")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "InputPassed_Address Search", driver, "GA", "DeKalb");
                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/form/table[2]/tbody/tr/td/input")).SendKeys(Keys.Enter);
                        gc.CreatePdf_WOP(orderNumber, "ResultGrid_AddressSearch", driver, "GA", "DeKalb");
                        //multi parcel....
                        try
                        {
                            IWebElement tableElement = driver.FindElement(By.XPath("/html/body/table[2]"));
                            IList<IWebElement> tableRow = tableElement.FindElements(By.TagName("tr"));
                            int tablerowcount = tableRow.Count;
                            if (tablerowcount <= 10)
                            {
                                if (tablerowcount > 2)
                                {
                                    IList<IWebElement> rowTD;
                                    HttpContext.Current.Session["multipleParcel_deKalb"] = "Yes";
                                    foreach (IWebElement row in tableRow)
                                    {
                                        rowTD = row.FindElements(By.TagName("td"));
                                        if (rowTD.Count != 0)
                                        {
                                            string multiOwnerData = rowTD[0].Text.Trim() + "~" + rowTD[2].Text.Trim();
                                            gc.insert_date(orderNumber, rowTD[1].Text.Trim(), 21, multiOwnerData, 1, DateTime.Now);
                                        }
                                    }

                                    driver.Quit();
                                    return "MultiParcel";
                                }
                            }
                            else
                            {
                                HttpContext.Current.Session["multipleParcel_deKalb_count"] = "Maximum";
                                driver.Quit();
                                return "Maximum";

                            }
                        }
                        catch
                        {

                        }
                    }

                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, "", "", address, "GA", "Dekalb");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_GADekalb"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "parcel")
                    {
                        if (GlobalClass.titleparcel != "")
                        {
                            parcelNumber = GlobalClass.titleparcel;
                        }
                        if (parcelNumber.Contains("-"))
                        {
                            parcelNumber = parcelNumber.Replace("-", "");
                        }
                        driver.FindElement(By.Id("rePID")).SendKeys(parcelNumber);
                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/form/table[2]/tbody/tr/td/input")).SendKeys(Keys.Enter);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search", driver, "GA", "DeKalb");
                    }
                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/form/table[1]/tbody/tr[6]/td[2]/input")).SendKeys(owner_name);
                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/form/table[2]/tbody/tr/td/input")).SendKeys(Keys.Enter);
                        gc.CreatePdf_WOP(orderNumber, "Owner Search", driver, "GA", "DeKalb");
                        //multi parcel....
                        try
                        {
                            IWebElement tableElement = driver.FindElement(By.XPath("/html/body/table[2]"));
                            IList<IWebElement> tableRow = tableElement.FindElements(By.TagName("tr"));
                            int tablerowcount = tableRow.Count;
                            if (tablerowcount <= 10)
                            {
                                if (tablerowcount > 2)
                                {
                                    IList<IWebElement> rowTD;
                                    HttpContext.Current.Session["multipleParcel_deKalb"] = "Yes";
                                    foreach (IWebElement row in tableRow)
                                    {
                                        rowTD = row.FindElements(By.TagName("td"));
                                        if (rowTD.Count != 0)
                                        {
                                            string multiOwnerData = rowTD[0].Text.Trim() + "~" + rowTD[1].Text.Trim();
                                            gc.insert_date(orderNumber, rowTD[2].Text.Trim(), 21, multiOwnerData, 1, DateTime.Now);
                                        }
                                    }
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                            }
                            else
                            {
                                HttpContext.Current.Session["multipleParcel_deKalb_count"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch
                        {

                        }

                    }

                    Thread.Sleep(2000);
                    //Property details....
                    if (searchType != "parcel")
                    {
                        try
                        {
                            driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td[1]/a")).SendKeys(Keys.Enter);
                        }
                        catch { }
                    }
                    Thread.Sleep(2000);
                    try
                    {
                        IWebElement INodata = driver.FindElement(By.XPath("/html/body/table"));
                        if(INodata.Text.Contains("entered was not found"))
                        {
                            HttpContext.Current.Session["Nodata_GADekalb"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        } 
                    }
                    catch { }
                    gc.CreatePdf(orderNumber, parcelNumber, "Parcel search Result", driver, "GA", "DeKalb");
                    string parcel_number = "-", owner_Name = "-", property_address = "-", year_built = "-", taxing_district = "-", deed_acreage = "-", property_class = "-";

                    parcel_number = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table[1]/tbody/tr[1]/td[2]/a/font/b")).Text;
                    parcelNumber = parcel_number;
                    owner_Name = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table[2]/tbody/tr[3]/td[2]")).Text;
                    if (driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table[2]/tbody/tr[4]/td[2]")).Text.Trim() != "")
                    {
                        owner_Name = owner_Name + " &" + driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table[2]/tbody/tr[4]/td[2]")).Text;
                    }

                    property_address = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table[2]/tbody/tr[8]/td[2]/a")).Text.Trim();
                    year_built = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table[5]/tbody/tr[2]/td[2]")).Text.Trim();
                    if (year_built == "")
                    {
                        year_built = "-";
                    }
                    taxing_district = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table[2]/tbody/tr[2]/td[2]")).Text.Trim();
                    if (taxing_district == "")
                    {
                        taxing_district = "-";
                    }
                    deed_acreage = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table[2]/tbody/tr[6]/td[5]")).Text.Trim();
                    if (deed_acreage == "")
                    {
                        deed_acreage = "-";
                    }
                    property_class = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table[2]/tbody/tr[8]/td[5]")).Text.Trim();
                    if (property_class == "")
                    {
                        property_class = "-";
                    }

                    string insertProprty = owner_Name + "~" + property_address + "~" + year_built + "~" + taxing_district + "~" + deed_acreage + "~" + property_class;
                    gc.insert_date(orderNumber, parcel_number, 20, insertProprty, 1, DateTime.Now);

                    //Assessment details....
                    try
                    {
                        IWebElement tableAss = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table[6]/tbody"));
                        IList<IWebElement> tableRowAss = tableAss.FindElements(By.TagName("tr"));
                        IList<IWebElement> rowTDAss;
                        int idx = 0;
                        foreach (IWebElement row in tableRowAss)
                        {
                            rowTDAss = row.FindElements(By.TagName("td"));

                            if (rowTDAss.Count != 0)
                            {
                                if (idx != 0 && idx != 1 && idx == 2)
                                {
                                    string assessData = rowTDAss[0].Text.Trim() + "~" + rowTDAss[1].Text.Trim() + "~" + rowTDAss[2].Text.Trim() + "~" + rowTDAss[3].Text.Trim() + "~" + rowTDAss[4].Text.Trim() + "~" + rowTDAss[5].Text.Trim();
                                    gc.insert_date(orderNumber, parcel_number, 29, assessData, 1, DateTime.Now);
                                }

                            }

                            idx++;
                        }
                    }
                    catch
                    {
                    }

                    try
                    {
                        try
                        {
                            tableAss1 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table[5]/tbody"));
                            if (!tableAss1.Text.Contains("Current Appraised & Assessment Value"))
                            {
                                tableAss1 = null;
                            }
                        }
                        catch { }
                        try
                        {
                            tableAss1 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table[6]/tbody"));
                            if (!tableAss1.Text.Contains("Current Appraised & Assessment Value"))
                            {
                                tableAss1 = null;
                            }
                        }
                        catch { }

                        IList<IWebElement> tableRowAss1 = tableAss1.FindElements(By.TagName("tr"));
                        IList<IWebElement> rowTDAss1;
                        int idx1 = 0;
                        foreach (IWebElement row in tableRowAss1)
                        {

                            rowTDAss1 = row.FindElements(By.TagName("td"));

                            if (rowTDAss1.Count != 0)
                            {
                                if (idx1 != 0 && idx1 != 1 && idx1 == 2)
                                {
                                    string assessData = rowTDAss1[0].Text.Trim() + "~" + rowTDAss1[1].Text.Trim() + "~" + rowTDAss1[2].Text.Trim() + "~" + rowTDAss1[3].Text.Trim() + "~" + rowTDAss1[4].Text.Trim() + "~" + rowTDAss1[5].Text.Trim();
                                    gc.insert_date(orderNumber, parcel_number, 29, assessData, 1, DateTime.Now);
                                }
                            }
                            idx1++;
                        }
                    }
                    catch
                    {

                    }


                    gc.CreatePdf(orderNumber, parcelNumber, "Property Detail and assessment detail", driver, "GA", "DeKalb");
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    //Tax details......
                    driver.Navigate().GoToUrl("https://taxcommissioner.dekalbcountyga.gov/TaxCommissioner/TCSearch.asp");
                    driver.FindElement(By.Id("Parcel")).SendKeys(parcelNumber);
                    gc.CreatePdf(orderNumber, parcelNumber, "tax detail search", driver, "GA", "DeKalb");
                    driver.FindElement(By.Name("Submit")).Click();
                    //driver.FindElement(By.XPath("/html/body/form/table/tbody/tr/td/div/table[3]/tbody/tr/td[1]/input")).SendKeys(Keys.Enter);
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, parcelNumber, "tax detail search result", driver, "GA", "DeKalb");

                    IWebElement Taxinfotable = driver.FindElement(By.XPath("//*[@id='printable']/table/tbody/tr/td/table/tbody/tr/td[1]/table/tbody/tr[1]/td/table/tbody"));
                    string pin_Number = driver.FindElement(By.XPath("//*[@id='printable']/table/tbody/tr/td/table/tbody/tr/td[1]/table/tbody/tr[1]/td/table/tbody/tr[3]/td[2]")).Text.Trim().Replace("Pin Number", "");
                    string property_Type = gc.Between(Taxinfotable.Text, "Property Type", "Tax District").Trim();
                    string tax_District = GlobalClass.After(Taxinfotable.Text, "Tax District").Trim();
                    //IWebElement Taxinfotable1 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[1]/tbody"));

                    string tax_Year = driver.FindElement(By.XPath("//*[@id='printable']/table/tbody/tr/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[1]/tbody/tr[2]/td[2]")).Text.Replace("Taxable Year", "").Trim();
                    string milage_Rate = driver.FindElement(By.XPath("//*[@id='printable']/table/tbody/tr/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[1]/tbody/tr[3]/td[2]")).Text.Replace("Millage Rate", "").Trim();
                    string tax_Type = "DeKalb County";

                    string firstinstallmentamount = "", SecondInstallmentAmt = "", taxes_Billed = "", taxes_Paid = "", taxes_Due = ""; string total_Billed = "-", total_Paid = "-", total_Due = "-", firstPaymentDate = "-", firstPaymentAmount = "-", lastPaymentDate = "-", lastPaymentAmount = "-", cityTaxType = "-", cityTaxesBilled = "-", cityTaxesPaid = "-", cityTaxesDue = "-", cityFirstPaymentDate = "-", cityFirstPaymentAmount = "-", cityLastPaymentDate = "-", cityLastAmount = "-", taxingAuthority = "-", countyExemptionType = "", countyTaxExemptionAmount = "", cityExemptionType = "-", CityExemptionAmount = "-", taxExemptionType = "-", valueExemptionAmount = "-";

                    IWebElement tableTaxinfo = driver.FindElement(By.XPath("//*[@id='printable']/table/tbody/tr/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[1]/tbody"));
                    IList<IWebElement> TRTaxinfo = tableTaxinfo.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDTaxinfo;
                    foreach (IWebElement row1 in TRTaxinfo)
                    {
                        TDTaxinfo = row1.FindElements(By.TagName("td"));
                        if (TDTaxinfo.Count != 0)
                        {
                            if (row1.Text.Contains("1 st Installment Amount"))
                            {
                                firstinstallmentamount = TDTaxinfo[1].Text.Trim();
                            }
                            if (row1.Text.Contains("2 nd Installment Amount"))
                            {
                                SecondInstallmentAmt = TDTaxinfo[1].Text.Trim();
                            }
                            if (row1.Text.Contains("DeKalb County Taxes Billed"))
                            {
                                taxes_Billed = TDTaxinfo[1].Text.Trim();
                            }
                            if (row1.Text.Contains("DeKalb County Taxes Paid"))
                            {
                                taxes_Paid = TDTaxinfo[1].Text.Trim();
                            }
                            if (row1.Text.Contains("DeKalb County Taxes Due"))
                            {
                                taxes_Due = TDTaxinfo[1].Text.Trim();
                            }
                            if (row1.Text.Contains("Total Taxes Billed"))
                            {
                                total_Billed = TDTaxinfo[1].Text.Trim();
                            }
                            if (row1.Text.Contains("Total Taxes Paid"))
                            {
                                total_Paid = TDTaxinfo[1].Text.Trim();
                            }
                            if (row1.Text.Contains("Total Taxes Due"))
                            {
                                total_Due = TDTaxinfo[1].Text.Trim();
                            }
                            if (row1.Text.Contains("Atlanta Taxes Billed"))
                            {
                                cityTaxesBilled = TDTaxinfo[1].Text.Trim();
                            }
                            if (row1.Text.Contains("Atlanta Taxes Paid"))
                            {
                                cityTaxesPaid = TDTaxinfo[1].Text.Trim();
                            }
                            if (row1.Text.Contains("Atlanta Taxes Due"))
                            {
                                cityTaxesDue = TDTaxinfo[1].Text.Trim();
                            }

                        }
                    }

                    try
                    {

                        IWebElement citytableTaxinfo = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[1]/tbody/tr[16]/td/table"));
                        IList<IWebElement> cityTRTaxinfo = citytableTaxinfo.FindElements(By.TagName("tr"));
                        IList<IWebElement> cityTDTaxinfo;
                        int cityTRTaxinfocount = cityTRTaxinfo.Count;

                        if (cityTRTaxinfocount == 5)
                        {
                            firstPaymentDate = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[1]/tbody/tr[16]/td/table/tbody/tr[2]/td[2]")).Text.Trim();
                            firstPaymentAmount = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[1]/tbody/tr[16]/td/table/tbody/tr3]/td[2]")).Text.Trim();
                            lastPaymentDate = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[1]/tbody/tr[16]/td/table/tbody/tr[4]/td[2]")).Text.Trim();
                            lastPaymentAmount = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[1]/tbody/tr[16]/td/table/tbody/tr[5]/td[2]")).Text.Trim();
                        }
                        else if (cityTRTaxinfocount == 10)
                        {
                            firstPaymentDate = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[1]/tbody/tr[16]/td/table/tbody/tr[2]/td[2]")).Text.Trim();
                            firstPaymentAmount = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[1]/tbody/tr[16]/td/table/tbody/tr[3]/td[2]")).Text.Trim();
                            lastPaymentDate = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[1]/tbody/tr[16]/td/table/tbody/tr[4]/td[2]")).Text.Trim();
                            lastPaymentAmount = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[1]/tbody/tr[16]/td/table/tbody/tr[5]/td[2]")).Text.Trim();

                            cityFirstPaymentDate = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[1]/tbody/tr[16]/td/table/tbody/tr[7]/td[2]")).Text.Trim();
                            cityFirstPaymentAmount = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[1]/tbody/tr[16]/td/table/tbody/tr[8]/td[2]")).Text.Trim();
                            cityLastPaymentDate = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[1]/tbody/tr[16]/td/table/tbody/tr[9]/td[2]")).Text.Trim();
                            cityLastAmount = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[1]/tbody/tr[16]/td/table/tbody/tr[10]/td[2]")).Text.Trim();
                        }
                    }

                    catch
                    {

                    }

                    try
                    {

                        IWebElement citytableTaxinfo = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[1]/tbody/tr[13]/td/table"));
                        IList<IWebElement> cityTRTaxinfo = citytableTaxinfo.FindElements(By.TagName("tr"));
                        IList<IWebElement> cityTDTaxinfo;
                        int cityTRTaxinfocount = cityTRTaxinfo.Count;

                        if (cityTRTaxinfocount == 5)

                        {
                            firstPaymentDate = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[1]/tbody/tr[13]/td/table/tbody/tr[2]/td[2]")).Text.Trim();
                            firstPaymentAmount = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[1]/tbody/tr[13]/td/table/tbody/tr[3]/td[2]")).Text.Trim();
                            lastPaymentDate = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[1]/tbody/tr[13]/td/table/tbody/tr[4]/td[2]")).Text.Trim();
                            lastPaymentAmount = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[1]/tbody/tr[13]/td/table/tbody/tr[5]/td[2]")).Text.Trim();

                        }

                        else if (cityTRTaxinfocount == 10)
                        {
                            firstPaymentDate = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[1]/tbody/tr[13]/td/table/tbody/tr[2]/td[2]")).Text.Trim();
                            firstPaymentAmount = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[1]/tbody/tr[13]/td/table/tbody/tr[3]/td[2]")).Text.Trim();
                            lastPaymentDate = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[1]/tbody/tr[13]/td/table/tbody/tr[4]/td[2]")).Text.Trim();
                            lastPaymentAmount = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[1]/tbody/tr[13]/td/table/tbody/tr[5]/td[2]")).Text.Trim();

                            cityFirstPaymentDate = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[1]/tbody/tr[13]/td/table/tbody/tr[7]")).Text.Trim();
                            cityFirstPaymentAmount = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[1]/tbody/tr[13]/td/table/tbody/tr[8]")).Text.Trim();
                            cityLastPaymentDate = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[1]/tbody/tr[13]/td/table/tbody/tr[9]")).Text.Trim();
                            cityLastAmount = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[1]/tbody/tr[13]/td/table/tbody/tr[10]")).Text.Trim();

                        }


                    }
                    catch
                    {

                    }

                    taxingAuthority = "DeKalb County Tax Commissioner Collections Division PO Box 100004,Decatur, GA 30031-7004";

                    try
                    {
                        countyExemptionType = driver.FindElement(By.XPath("//*[@id='printable']/table/tbody/tr/td/table/tbody/tr/td[1]/table/tbody/tr[4]/td/table/tbody/tr[2]/td[2]")).Text;
                        countyTaxExemptionAmount = driver.FindElement(By.XPath("//*[@id='printable']/table/tbody/tr/td/table/tbody/tr/td[1]/table/tbody/tr[4]/td/table/tbody/tr[3]/td[2]")).Text;
                        cityExemptionType = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[2]/td/table/tbody/tr/td[1]/table/tbody/tr[3]/td/table/tbody/tr[4]/td[2]")).Text;
                        if (cityExemptionType == " ")
                        {
                            cityExemptionType = "-";
                        }
                        CityExemptionAmount = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[2]/td/table/tbody/tr/td[1]/table/tbody/tr[3]/td/table/tbody/tr[5]/td[2]")).Text;
                    }
                    catch
                    {

                    }
                    try
                    {

                        cityTaxType = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[1]/tbody/tr[16]/td/table/tbody/tr[6]/td")).Text.Trim();
                    }
                    catch { }

                    IWebElement Homesteadtable = driver.FindElement(By.XPath("//*[@id='printable']/table/tbody/tr/td/table/tbody/tr/td[1]/table/tbody/tr[3]/td/table/tbody"));
                    taxExemptionType = gc.Between(Homesteadtable.Text, "Exemption Type", "Tax Exempt Amount").Trim();
                    valueExemptionAmount = GlobalClass.After(Homesteadtable.Text, "Tax Exempt Amount").Trim();


                    //Delinquent Taxes
                    string Tax_Sale_File_Number = "-", FiFa_GED_Book_Page = "-", Levy_Date = "-", Sale_Date = "-", Delinquent_Amount_Due = "-";
                    try
                    {
                        Tax_Sale_File_Number = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]")).Text.Trim();
                        if (Tax_Sale_File_Number == "")
                        {
                            Tax_Sale_File_Number = "-";
                        }
                        FiFa_GED_Book_Page = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[3]/td[2]")).Text.Trim();
                        if (FiFa_GED_Book_Page == "")
                        {
                            FiFa_GED_Book_Page = "-";
                        }
                        Levy_Date = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[4]/td[2]")).Text.Trim();
                        if (Levy_Date == "")
                        {
                            Levy_Date = "-";
                        }
                        Sale_Date = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[5]/td[2]")).Text.Trim();
                        if (Sale_Date == "")
                        {
                            Sale_Date = "-";
                        }
                        Delinquent_Amount_Due = driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[2]/td/table/tbody/tr[6]/td[2]")).Text.Trim();
                        if (Delinquent_Amount_Due == "")
                        {
                            Delinquent_Amount_Due = "-";
                        }

                    }
                    catch { }

                    string tax_deliquent_details = pin_Number + "~" + property_Type + "~" + tax_District + "~" + tax_Year + "~" + milage_Rate + "~" + tax_Type + "~" + firstinstallmentamount + "~" + SecondInstallmentAmt + "~" + taxes_Billed + "~" + taxes_Paid + "~" + taxes_Due + "~" + total_Billed + "~" + total_Paid + "~" + total_Due + "~" + cityTaxesBilled + "~" + cityTaxesPaid + "~" + cityTaxesDue + "~" + firstPaymentDate + "~" + firstPaymentAmount + "~" + lastPaymentDate + "~" + lastPaymentAmount + "~" + cityFirstPaymentDate + "~" + cityFirstPaymentAmount + "~" + cityLastPaymentDate + "~" + cityLastAmount + "~" + taxingAuthority + "~" + countyExemptionType + "~" + countyTaxExemptionAmount + "~" + cityExemptionType + "~" + CityExemptionAmount + "~" + cityTaxType + "~" + taxExemptionType + "~" + valueExemptionAmount + "~" + Tax_Sale_File_Number + "~" + FiFa_GED_Book_Page + "~" + Levy_Date + "~" + Sale_Date + "~" + Delinquent_Amount_Due;
                    gc.insert_date(orderNumber, parcel_number, 55, tax_deliquent_details, 1, DateTime.Now);

                    int i = 1;
                    string strTaxType = "";
                    //Tax History
                    IWebElement tableTaxHistory = driver.FindElement(By.XPath("//*[@id='printable']/table/tbody/tr/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[3]/tbody"));
                    IList<IWebElement> TRTaxHistory = tableTaxHistory.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDTaxHistory;
                    foreach (IWebElement row1 in TRTaxHistory)
                    {

                        TDTaxHistory = row1.FindElements(By.TagName("td"));
                        if (TDTaxHistory.Count != 0 && TDTaxHistory.Count == 1 && !row1.Text.Contains("Commissioner") && !row1.Text.Contains("TaxYear") && !row1.Text.Contains("Prior Years Tax"))
                        {
                            if (TDTaxHistory[0].Text.Contains("DeKalb County Tax") || TDTaxHistory[0].Text.Contains("City Atlanta Tax"))
                            {
                                strTaxType = TDTaxHistory[0].Text;
                            }
                        }
                        if (i >= 7)
                        {


                            if (TDTaxHistory.Count > 1 && !row1.Text.Contains("City ") && !row1.Text.Contains("TaxYear"))
                            {
                                string TDTaxHistory4 = "";
                                if (TDTaxHistory[4].Text.Trim() == "")
                                {
                                    TDTaxHistory4 = "-";
                                }
                                string taxhistory_details = strTaxType + "~" + TDTaxHistory[0].Text.Trim() + "~" + TDTaxHistory[1].Text.Trim() + "~" + TDTaxHistory[2].Text.Trim() + "~" + TDTaxHistory[3].Text.Trim() + "~" + TDTaxHistory4;
                                gc.insert_date(orderNumber, parcel_number, 54, taxhistory_details, 1, DateTime.Now);
                            }

                        }
                        i++;
                    }

                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Details", driver, "GA", "DeKalb");
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/table[3]/tbody/tr[2]/td/table/tbody/tr/td[2]/table/tbody/tr[1]/td/table[2]/tbody/tr[1]/td[2]/form/input[3]")).SendKeys(Keys.Enter);
                        driver.SwitchTo().Window(driver.WindowHandles.Last());
                        IWebElement tableTaxBill = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table[3]"));
                        IList<IWebElement> tableRowTaxBil = tableTaxBill.FindElements(By.TagName("tr"));
                        IList<IWebElement> rowTDTaxBill;

                        List<string> urlList = new List<string>();
                        foreach (IWebElement row in tableRowTaxBil)
                        {
                            rowTDTaxBill = row.FindElements(By.TagName("td"));
                            if (rowTDTaxBill.Count != 0)
                            {
                                if (rowTDTaxBill[0].Text.Contains("2017") || rowTDTaxBill[0].Text.Contains("2018"))
                                {
                                    IWebElement link = rowTDTaxBill[2].FindElement(By.TagName("a"));
                                    string url = link.GetAttribute("onclick");
                                    url = WebDriverTest.Between(url, "('", "')");
                                    string pdfName = url;
                                    //string outputPath = ConfigurationManager.AppSettings["screenShotPath-GAde"];
                                    //outputPath = outputPath + orderNumber + "\\" + parcelNumber.Replace(" ", "") + "\\" + pdfName.Replace('/', '_');
                                    url = "https://taxcommissioner.dekalbcountyga.gov/TaxCommissioner/" + url;
                                    //WebClient downloadTaxBills = new WebClient();
                                    //downloadTaxBills.DownloadFile(url, outputPath);
                                    gc.downloadfile(url, orderNumber, parcelNumber, pdfName.Replace('/', '_'), "GA", "DeKalb");
                                }
                            }
                        }
                    }
                    catch { }

                    if (tax_District.Contains("DECATUR"))
                    {
                        try
                        {
                            DekalbTax_search1(orderNumber, parcelNumber);
                            DekalbTax_search2(orderNumber, parcelNumber);
                            // DekalbTax_Year_Installment1();
                            //DekalbTax_Year_Installment2();
                        }
                        catch (Exception e)
                        {
                        }
                    }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "GA", "DeKalb", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "GA", "DeKalb");
                    return "Data Inserted Successfully";
                }
                catch (Exception ex)
                {
                    driver.Quit();
                    throw ex;
                }
            }
        }
        public void DekalbTax_search1(string orderNumber, string parcelNumber)
        {
            strtaxYear = DateTime.Now.Year.ToString();
            for (int i = 0; i < 2; i++)
            {
                try
                {
                    driver.Navigate().GoToUrl("http://www.decaturgatax.com/taxes");
                    driver.FindElement(By.Id("ctl00_cphMainContent_SkeletonCtrl_3_btnAccept")).SendKeys(Keys.Enter);
                    IWebElement Itaxyearselect = driver.FindElement(By.Id("ctl00_cphMainContent_SkeletonCtrl_3_drpTaxYear"));
                    SelectElement Staxselect = new SelectElement(Itaxyearselect);
                    Staxselect.SelectByText(strtaxYear);
                    IWebElement Itaxinstall1 = driver.FindElement(By.Id("ctl00_cphMainContent_SkeletonCtrl_3_drpInstallment"));
                    SelectElement Staxinstall1 = new SelectElement(Itaxinstall1);
                    Staxinstall1.SelectByText("1st Installment");
                    IWebElement ItaxParcel = driver.FindElement(By.Id("ctl00_cphMainContent_SkeletonCtrl_3_drpSearchParam"));
                    SelectElement Staxparcel = new SelectElement(ItaxParcel);
                    Staxparcel.SelectByText("Property ID");
                    if (parcelNumber != " ")
                    {
                        string strparcelno = parcelNumber.Replace(" ", "");
                        string strpsplit1 = strparcelno.Substring(0, 2);
                        string strpsplit2 = strparcelno.Substring(2, 3);
                        string strpsplit3 = strparcelno.Substring(5, 2);
                        string strpsplit4 = strparcelno.Substring(7, 3);
                        strparcelNumber = strpsplit1 + " " + strpsplit2 + " " + strpsplit3 + " " + strpsplit4;
                    }
                    driver.FindElement(By.Id("ctl00_cphMainContent_SkeletonCtrl_3_txtSearchParam")).Clear();
                    driver.FindElement(By.Id("ctl00_cphMainContent_SkeletonCtrl_3_txtSearchParam")).SendKeys(strparcelNumber);
                    gc.CreatePdf(orderNumber, strparcelNumber, "DecaturCityTaxSearchFirstInstall", driver, "GA", "DeKalb");
                    driver.FindElement(By.Id("ctl00_cphMainContent_SkeletonCtrl_3_btnSearch")).SendKeys(Keys.Enter);
                    gc.CreatePdf(orderNumber, strparcelNumber, "DecaturCityTaxSearch_ResultsFirstInstall", driver, "GA", "DeKalb");
                    // WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                    //wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("ctl00_cphMainContent_SkeletonCtrl_3_gvRecords_ctl02_btnSelectRecord")));
                    IWebElement Ilink = driver.FindElement(By.Id("ctl00_cphMainContent_SkeletonCtrl_3_gvRecords_ctl02_btnSelectRecord"));
                    IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
                    executor.ExecuteScript("arguments[0].click();", Ilink);
                    Thread.Sleep(3000);
                    DekalbTax_Details(orderNumber, parcelNumber);
                }
                catch { }

                taxYear = Convert.ToInt32(strtaxYear);
                taxYear--;
                strtaxYear = Convert.ToString(taxYear);
            }

        }
        public void DekalbTax_search2(string orderNumber, string parcelNumber)
        {
            strtaxYear = DateTime.Now.Year.ToString();

            for (int i = 0; i < 2; i++)
            {
                try
                {

                    driver.Navigate().GoToUrl("http://www.decaturgatax.com/taxes");
                    driver.FindElement(By.Id("ctl00_cphMainContent_SkeletonCtrl_3_btnAccept")).SendKeys(Keys.Enter);
                    IWebElement Itaxyearselect = driver.FindElement(By.Id("ctl00_cphMainContent_SkeletonCtrl_3_drpTaxYear"));
                    SelectElement Staxselect = new SelectElement(Itaxyearselect);
                    Staxselect.SelectByText(strtaxYear);
                    IWebElement Itaxinstall2 = driver.FindElement(By.Id("ctl00_cphMainContent_SkeletonCtrl_3_drpInstallment"));
                    SelectElement Staxinstall2 = new SelectElement(Itaxinstall2);
                    Staxinstall2.SelectByText("2nd Installment");
                    IWebElement ItaxParcel = driver.FindElement(By.Id("ctl00_cphMainContent_SkeletonCtrl_3_drpSearchParam"));
                    SelectElement Staxparcel = new SelectElement(ItaxParcel);
                    Staxparcel.SelectByText("Property ID");
                    if (parcelNumber != " ")
                    {
                        strparcelNumber = "";
                        string strparcelno = parcelNumber.Replace(" ", "");
                        string strpsplit1 = strparcelno.Substring(0, 2);
                        string strpsplit2 = strparcelno.Substring(2, 3);
                        string strpsplit3 = strparcelno.Substring(5, 2);
                        string strpsplit4 = strparcelno.Substring(7, 3);
                        strparcelNumber = strpsplit1 + " " + strpsplit2 + " " + strpsplit3 + " " + strpsplit4;
                    }
                    driver.FindElement(By.Id("ctl00_cphMainContent_SkeletonCtrl_3_txtSearchParam")).Clear();
                    driver.FindElement(By.Id("ctl00_cphMainContent_SkeletonCtrl_3_txtSearchParam")).SendKeys(strparcelNumber);
                    gc.CreatePdf(orderNumber, strparcelNumber, "DecaturCityTaxSearchSecondInstall", driver, "GA", "DeKalb");
                    driver.FindElement(By.Id("ctl00_cphMainContent_SkeletonCtrl_3_btnSearch")).SendKeys(Keys.Enter);
                    gc.CreatePdf(orderNumber, strparcelNumber, "DecaturCityTaxSearch_ResultsSecondInstall", driver, "GA", "DeKalb");

                    //IWebElement Itaxtable = driver.FindElement(By.XPath("//*[@id='ctl00_cphMainContent_SkeletonCtrl_3_gvRecords']"));
                    //IList<IWebElement> Itaxrow = Itaxtable.FindElements(By.TagName("tr"));
                    //IList<IWebElement> ItaxTd;
                    //CreatePdf(orderNumber, strparcelNumber, "DecaturCityTaxHistory");
                    // WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                    // wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("ctl00_cphMainContent_SkeletonCtrl_3_gvRecords_ctl03_btnSelectRecord")));
                    IWebElement Ilink = driver.FindElement(By.Id("ctl00_cphMainContent_SkeletonCtrl_3_gvRecords_ctl02_btnSelectRecord"));
                    IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
                    executor.ExecuteScript("arguments[0].click();", Ilink);
                    Thread.Sleep(3000);
                    DekalbTax_Details(orderNumber, parcelNumber);
                }
                catch { }

                taxYear = Convert.ToInt32(strtaxYear);
                taxYear--;
                strtaxYear = Convert.ToString(taxYear);
            }
        }

        public void DekalbTax_Details(string orderNumber, string parcelNumber)
        {
            Thread.Sleep(2000);
            strOwnerName = driver.FindElement(By.Id("ctl00_cphMainContent_SkeletonCtrl_3_lblOwnerName")).Text;
            strTaxYear = driver.FindElement(By.Id("ctl00_cphMainContent_SkeletonCtrl_3_lblTaxYear")).Text;
            strReceipt = driver.FindElement(By.Id("ctl00_cphMainContent_SkeletonCtrl_3_lblReceipt")).Text;
            strTaxInstallment = driver.FindElement(By.Id("ctl00_cphMainContent_SkeletonCtrl_3_lblInstallment")).Text;
            strDueDate = driver.FindElement(By.Id("ctl00_cphMainContent_SkeletonCtrl_3_lblDueDate")).Text;
            strTotalTax = driver.FindElement(By.Id("ctl00_cphMainContent_SkeletonCtrl_3_lblBaseAmtDue")).Text;
            strPaid = driver.FindElement(By.Id("ctl00_cphMainContent_SkeletonCtrl_3_lblPaidAmount")).Text;
            strPaidDate = driver.FindElement(By.Id("ctl00_cphMainContent_SkeletonCtrl_3_lblPaidDate")).Text;
            strStatus = driver.FindElement(By.Id("ctl00_cphMainContent_SkeletonCtrl_3_lblPaymentStatus")).Text;
            strTotalDue = driver.FindElement(By.Id("ctl00_cphMainContent_SkeletonCtrl_3_lblBalanceDue")).Text;
            gc.CreatePdf(orderNumber, strparcelNumber, "DecaturCityTaxDetails" + strTaxYear + "_" + strTaxInstallment + "Installment", driver, "GA", "DeKalb");

            //Tax Installment
            string strTaxinformation = strOwnerName + "~" + strTaxYear + "~" + strReceipt + "~" + strTaxInstallment + "~" + strDueDate + "~" + strTotalTax + "~" + strPaid + "~" + strPaidDate + "~" + strStatus + "~" + strTotalDue + "~" + "-";
            gc.insert_date(orderNumber, parcelNumber, 56, strTaxinformation, 1, DateTime.Now);

            strPropertyId = driver.FindElement(By.Id("ctl00_cphMainContent_SkeletonCtrl_3_lblParcel")).Text;
            //Tax History
            //Owner_Name~Tax_Year~Receipt~Installment~Due_Date~Total_Tax~Paid_Amount~Paid_Date~Status~Total_Due~Property_ID
            string strTaxHistory = strOwnerName + "~" + strTaxYear + "~" + strReceipt + "~" + strTaxInstallment + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + strStatus + "~" + "-" + "~" + strPropertyId;
            gc.insert_date(orderNumber, parcelNumber, 56, strTaxHistory, 1, DateTime.Now);
            try
            {
                IWebElement IPrint = driver.FindElement(By.XPath("//*[@id='ctl00_cphMainContent_SkeletonCtrl_3_tbbill']/a"));
                IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
                executor.ExecuteScript("arguments[0].click();", IPrint);
                Thread.Sleep(2000);
                //pdf ViewPrintBill
                IWebElement printurl = driver.FindElement(By.Id("ctl00_cphMainContent_SkeletonCtrl_3_linkSaveBill"));
                string printURL = printurl.GetAttribute("href");
                //outputPath = ConfigurationManager.AppSettings["screenShotPath-GADe"];
                string printpdf = outputPath + orderNumber + "\\" + strparcelNumber.Replace(" ", "") + "\\" + strTaxYear + strTaxInstallment + "View_Print_Bill.pdf";
                //WebClient downloadprintpdf = new WebClient();
                //downloadprintpdf.DownloadFile(printURL, printpdf);
                gc.downloadfile(printURL, orderNumber, parcelNumber, strTaxYear + strTaxInstallment + "View_Print_Bill", "GA", "DeKalb");
            }
            catch
            { }
            try
            {
                IWebElement IReceipt = driver.FindElement(By.XPath("//*[@id='ctl00_cphMainContent_SkeletonCtrl_3_tbreceipt']/a"));
                IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
                executor.ExecuteScript("arguments[0].click();", IReceipt);
                Thread.Sleep(2000);
                //pdf ViewReceiptBill
                IWebElement receipturl = driver.FindElement(By.Id("ctl00_cphMainContent_SkeletonCtrl_3_linkSaveReceipt"));
                string receiptURL = receipturl.GetAttribute("href");
                //outputPath = ConfigurationManager.AppSettings["screenShotPath-GADe"];
                //string receiptpdf = outputPath + orderNumber + "\\" + strparcelNumber.Replace(" ", "") + "\\" + strTaxYear + strTaxInstallment + "View_Receipt_Bill.pdf";
                //WebClient downloadreceiptpdf = new WebClient();
                //downloadreceiptpdf.DownloadFile(receiptURL, receiptpdf);
                gc.downloadfile(receiptURL, orderNumber, parcelNumber, strTaxYear + strTaxInstallment + "View_Receipt_Bill", "GA", "DeKalb");
            }
            catch
            { }
        }
    }
}

