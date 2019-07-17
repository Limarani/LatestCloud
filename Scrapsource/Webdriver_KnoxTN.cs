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
    public class Webdriver_KnoxTN
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();

        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_Knox(string address, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel, string taxmapnumber)
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

                    // Property Details
                    driver.Navigate().GoToUrl("http://tn-knox-assessor.publicaccessnow.com/PropertyLookup.aspx");

                    if (searchType == "titleflex")
                    {
                        string titleaddress = address;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "TN", "Knox");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_KnoxTN"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";

                    }
                    if (searchType == "address")
                    {
                        driver.FindElement(By.XPath("//*[@id='fldSearchFor']")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "TN", "Knox");

                        driver.FindElement(By.XPath("//*[@id='QuickSearch']/div/div/table/tbody/tr/td[2]/button[1]")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Address search result", driver, "TN", "Knox");

                        string mul = "";
                        try
                        {
                            IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='QuickSearch']/div[1]/div[2]"));
                            mul = gc.Between(multiaddress.Text, "Your search returned ", "records.").Trim();
                            int count = Convert.ToInt32(mul);

                            if (count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_KnoxTN_Multicount"] = "Maximum";
                                driver.Quit();
                                return "Maximum";

                            }
                            try
                            {
                                string Nodata = driver.FindElement(By.XPath("//*[@id='QuickSearch']/p")).Text;
                                if (Nodata.Contains("Sorry, no"))
                                {
                                    HttpContext.Current.Session["Nodata_KnoxTN"] = "Yes";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }

                            if ((mul != "1") && (mul != "0") && count <= 25)
                            {
                                for (int i = 1; i <= count; i++)
                                {

                                    string owner_name = driver.FindElement(By.XPath("//*[@id='QuickSearch']/div[2]/div[" + i + "]/ul[2]/li[1]/a")).Text;
                                    string pro_add1 = driver.FindElement(By.XPath("//*[@id='QuickSearch']/div[2]/div[" + i + "]/ul[2]/li[2]")).Text;
                                    string pro_add2 = driver.FindElement(By.XPath("//*[@id='QuickSearch']/div[2]/div[" + i + "]/ul[2]/li[3]")).Text;
                                    string pro_add = pro_add1 + " " + pro_add2;
                                    driver.FindElement(By.XPath("//*[@id='QuickSearch']/div[2]/div[" + i + "]/ul[2]/li[1]/a")).SendKeys(Keys.Enter);
                                    Thread.Sleep(3000);
                                    string parcel_num = "";
                                    string bulkdata = driver.FindElement(By.XPath("//*[@id='Body']")).Text.Trim();
                                    parcel_num = gc.Between(bulkdata, "Property ID", "Alternate ID").Trim();
                                    driver.FindElement(By.XPath("//*[@id='lxT491']/div/a[1]")).SendKeys(Keys.Enter);
                                    Thread.Sleep(3000);
                                    string multi1 = owner_name + "~" + pro_add;
                                    gc.insert_date(orderNumber, parcel_num, 707, multi1, 1, DateTime.Now);

                                }

                                HttpContext.Current.Session["multiparcel_KnoxTN"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }

                            else
                            {
                                try
                                {
                                    driver.FindElement(By.XPath("//*[@id='QuickSearch']/div[2]/div[1]/ul[2]/li[1]/a")).SendKeys(Keys.Enter);
                                    Thread.Sleep(3000);
                                    gc.CreatePdf_WOP(orderNumber, "Address single search", driver, "TN", "Knox");
                                }
                                catch { }
                            }
                        }
                        catch { }

                    }

                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.XPath("//*[@id='fldSearchFor']")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "Ownername search", driver, "TN", "Knox");
                        driver.FindElement(By.XPath("//*[@id='QuickSearch']/div/div/table/tbody/tr/td[2]/button[1]")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Ownername search result", driver, "TN", "Knox");
                        try
                        {
                            string mul = "";
                            IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='QuickSearch']/div[1]/div[2]"));
                            mul = gc.Between(multiaddress.Text, "Your search returned ", "records.").Trim();
                            int count = Convert.ToInt32(mul);
                            if ((mul != "1") && (mul != "0") && (Convert.ToInt32(mul) <= 25))
                            {
                                for (int i = 1; i <= count; i++)
                                {

                                    string owner_name = driver.FindElement(By.XPath("//*[@id='QuickSearch']/div[2]/div[" + i + "]/ul[2]/li[1]/a")).Text;
                                    string pro_add1 = driver.FindElement(By.XPath("//*[@id='QuickSearch']/div[2]/div[" + i + "]/ul[2]/li[2]")).Text;
                                    string pro_add2 = driver.FindElement(By.XPath("//*[@id='QuickSearch']/div[2]/div[" + i + "]/ul[2]/li[3]")).Text;
                                    string pro_add = pro_add1 + " " + pro_add2;
                                    driver.FindElement(By.XPath("//*[@id='QuickSearch']/div[2]/div[" + i + "]/ul[2]/li[1]/a")).SendKeys(Keys.Enter);
                                    Thread.Sleep(3000);
                                    string parcel_num = "";
                                    string bulkdata = driver.FindElement(By.XPath("//*[@id='Body']")).Text.Trim();
                                    parcel_num = gc.Between(bulkdata, "Property ID", "Alternate ID").Trim();
                                    driver.FindElement(By.XPath("//*[@id='lxT491']/div/a[1]")).SendKeys(Keys.Enter);
                                    Thread.Sleep(3000);
                                    string multi1 = owner_name + "~" + pro_add;
                                    gc.insert_date(orderNumber, parcel_num, 707, multi1, 1, DateTime.Now);

                                }
                                HttpContext.Current.Session["multiparcel_KnoxTN"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            else
                            {
                                try
                                {
                                    driver.FindElement(By.XPath("//*[@id='QuickSearch']/div[2]/div[1]/ul[2]/li[1]/a")).SendKeys(Keys.Enter);
                                    Thread.Sleep(3000);
                                    gc.CreatePdf_WOP(orderNumber, "Ownername single search ", driver, "TN", "Knox");
                                }
                                catch { }
                            }
                        }
                        catch { }

                    }
                    if (searchType == "parcel")
                    {
                        string strparcelNumber = parcelNumber.Replace("-", "");
                        driver.FindElement(By.XPath("//*[@id='fldSearchFor']")).SendKeys(strparcelNumber);

                        gc.CreatePdf(orderNumber, strparcelNumber, "parcel search", driver, "TN", "Knox");
                        driver.FindElement(By.XPath("//*[@id='QuickSearch']/div/div/table/tbody/tr/td[2]/button[1]")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, strparcelNumber, "parcel search Result", driver, "TN", "Knox");
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='QuickSearch']/div[2]/div[1]/ul[2]/li[1]/a")).SendKeys(Keys.Enter);
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, strparcelNumber, "parcel Result view", driver, "TN", "Knox");
                        }
                        catch { }
                    }


                    try
                    {
                        string Nodata = driver.FindElement(By.XPath("//*[@id='QuickSearch']/p")).Text;
                        if (Nodata.Contains("Sorry, no"))
                        {
                            HttpContext.Current.Session["Nodata_KnoxTN"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    string name_address = "", parcel_id = "", alternate_id = "", property_address = "", property_class = "", neighborhood = "", deed_acres = "", year_built = "", legal_desc1 = "", legal_desc2 = "";
                    string bulktext = driver.FindElement(By.XPath("//*[@id='Body']")).Text.Trim();
                    name_address = gc.Between(bulktext, "GENERAL INFORMATION", "Property ID").Trim();
                    parcel_id = gc.Between(bulktext, "Property ID", "Alternate ID").Trim();
                    alternate_id = gc.Between(bulktext, "Alternate ID", "Address").Trim();
                    property_address = gc.Between(bulktext, "Address", "Property Class").Trim();
                    property_class = gc.Between(bulktext, "Property Class", "Neighborhood").Trim();
                    neighborhood = gc.Between(bulktext, "Neighborhood", "Deed Acres").Trim();
                    deed_acres = gc.Between(bulktext, "Deed Acres", "VALUE HISTORY").Trim();
                    try
                    {
                        IWebElement IYearBuilt = driver.FindElement(By.XPath("//*[@id='462gallerywrap']/table[2]"));
                        year_built = gc.Between(IYearBuilt.Text, "Year Built ", "\r\nValue");
                    }
                    catch { }
                    //  year_built = gc.Between(bulktext, "Year Built", "Value").Trim();
                    try
                    {
                        legal_desc1 = driver.FindElement(By.XPath("//*[@id='lxT466']/table/tbody/tr[2]/td[2]")).Text.Trim();
                        legal_desc2 = driver.FindElement(By.XPath("//*[@id='lxT466']/table/tbody/tr[3]/td[2]")).Text.Trim();
                    }
                    catch { }
                    string propertydetails = alternate_id + "~" + property_address + "~" + name_address + "~" + property_class + "~" + neighborhood + "~" + deed_acres + "~" + year_built + "~" + legal_desc1 + legal_desc2;
                    gc.insert_date(orderNumber, parcel_id, 705, propertydetails, 1, DateTime.Now);

                    // Assessment Details table

                    IWebElement year = driver.FindElement(By.XPath("//*[@id='ValueHistory']/tbody/tr[1]/th[2]"));
                    IWebElement reason = driver.FindElement(By.XPath("//*[@id='ValueHistory']/tbody/tr[2]/td[1]"));
                    IWebElement land_value = driver.FindElement(By.XPath("//*[@id='ValueHistory']/tbody/tr[3]/td[1]"));
                    IWebElement improvement_value = driver.FindElement(By.XPath("//*[@id='ValueHistory']/tbody/tr[4]/td[1]"));
                    IWebElement total_appraised = driver.FindElement(By.XPath("//*[@id='ValueHistory']/tbody/tr[5]/td[1]"));
                    IWebElement land_market = driver.FindElement(By.XPath("//*[@id='ValueHistory']/tbody/tr[6]/td[1]"));
                    IWebElement land_use = driver.FindElement(By.XPath("//*[@id='ValueHistory']/tbody/tr[7]/td[1]"));
                    IWebElement improvement_market = driver.FindElement(By.XPath("//*[@id='ValueHistory']/tbody/tr[8]/td[1]"));
                    IWebElement total_market = driver.FindElement(By.XPath("//*[@id='ValueHistory']/tbody/tr[9]/td[1]"));
                    string assessmentdetails = year.Text + "~" + reason.Text + "~" + land_value.Text + "~" + improvement_value.Text + "~" + total_appraised.Text + "~" + land_market.Text + "~" + land_use.Text + "~" + improvement_market.Text + "~" + total_market.Text;
                    gc.insert_date(orderNumber, parcel_id, 708, assessmentdetails, 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");


                    // Tax information Table:
                    driver.Navigate().GoToUrl("https://www.msbpay.com/KnoxTrustee/#/Search");

                    gc.CreatePdf(orderNumber, parcel_id, "Tax Information", driver, "TN", "Knox");
                    parcel_id = parcel_id.Replace(" ", "");
                    driver.FindElement(By.XPath("//*[@id='PropertyId']")).SendKeys(parcel_id);
                    gc.CreatePdf(orderNumber, parcel_id, "Tax Information Input", driver, "TN", "Knox");
                    driver.FindElement(By.XPath("//*[@id='btnSearch']")).SendKeys(Keys.Enter);
                    Thread.Sleep(4000);
                    gc.CreatePdf(orderNumber, parcel_id, "Tax Information search", driver, "TN", "Knox");
                    driver.FindElement(By.XPath("//*[@id='tblSearchResult']/tbody/tr/td[4]/a[1]")).SendKeys(Keys.Enter);
                    Thread.Sleep(9000);
                    gc.CreatePdf(orderNumber, parcel_id, "Tax Information Result", driver, "TN", "Knox");
                    string owner_Name = "", mailing_Address = "", property_Add = "", appraisal = "", assessment = "", total_balance_due = "", tax_rate = "";
                    //*[@id="divPage"]/div[2]/div[2]/div/div[1]/div/table/tbody
                    string bulkdata1 = driver.FindElement(By.XPath("//*[@id='divPage']/div[2]/div[2]/div/div[1]/div/table/tbody")).Text.Trim();
                    owner_Name = gc.Between(bulkdata1, "Owner 1", "Address").Trim().Replace("Owner 2 N/AOwner 3 N/AOwner 4 N/A", "");
                    mailing_Address = gc.Between(bulkdata1, "Address", "Property Address").Trim().Replace("N/A", "").Replace("Address 1", " ").Replace("City", " ").Replace("Zip", " ").Replace("State", " ");
                    property_Add = gc.Between(bulkdata1, "Property Address", "Appraisal").Trim();
                    appraisal = gc.Between(bulkdata1, "Appraisal", "Assessment").Trim();
                    assessment = gc.Between(bulkdata1, "Assessment", "Total Balance Due").Trim();
                    total_balance_due = gc.Between(bulkdata1, "Total Balance Due", "Tax Rate").Trim();
                    tax_rate = GlobalClass.After(bulkdata1, "Tax Rate").Trim();
                    string Taxing_Authority = "Knox County Trustee P.O.Box 70 Knoxville, TN 37901";
                    string CITY = "";
                    if (!mailing_Address.Contains("KNOXVILLE"))
                    {

                        CITY = "Need to check City taxes";
                    }
                    string taxinformation = owner_Name + "~" + mailing_Address + "~" + property_Add + "~" + appraisal + "~" + assessment + "~" + total_balance_due + "~" + tax_rate + "~" + Taxing_Authority + "~" + CITY;
                    gc.insert_date(orderNumber, parcel_id, 712, taxinformation, 1, DateTime.Now);

                    // Tax History Table
                    IWebElement tbmulti = driver.FindElement(By.XPath("//*[@id='divPage']/div[2]/div[2]/div/div[2]/div/table/tbody"));
                    IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));

                    IList<IWebElement> TDmulti;
                    foreach (IWebElement row in TRmulti)
                    {

                        TDmulti = row.FindElements(By.TagName("td"));
                        if (TDmulti.Count == 7)
                        {
                            string multi1 = TDmulti[0].Text + "~" + TDmulti[1].Text + "~" + TDmulti[2].Text + "~" + TDmulti[3].Text + "~" + TDmulti[4].Text + "~" + TDmulti[5].Text + "~" + TDmulti[6].Text;
                            gc.insert_date(orderNumber, parcel_id, 719, multi1, 1, DateTime.Now);
                        }

                    }

                    string current = driver.CurrentWindowHandle;
                    try
                    {
                        //*[@id="print-tax-invoice"]
                        driver.FindElement(By.Id("print-tax-invoice")).SendKeys(Keys.Enter);
                        Thread.Sleep(5000);
                        driver.SwitchTo().Window(driver.WindowHandles.Last());
                        gc.CreatePdf(orderNumber, parcel_id, "Tax Print", driver, "TN", "Knox");
                    }
                    catch { }
                    driver.SwitchTo().Window(current);
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    // City Tax Information Table
                    try
                    {
                        driver.Navigate().GoToUrl("https://propertytax.knoxvilletn.gov/ptax_master_search_vw/ShowPtax_master_search_vwTable.aspx");
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, parcel_id, " City Tax Information", driver, "TN", "Knox");
                        driver.FindElement(By.XPath("//*[@id='ctl00_PageContent_property_idFilter']")).SendKeys(parcel_id);
                        driver.FindElement(By.XPath("//*[@id='ctl00_PageContent_Ptax_master_search_vwSearchButton1__Button']")).SendKeys(Keys.Enter);
                        Thread.Sleep(5000);
                        gc.CreatePdf(orderNumber, parcel_id, "City Tax Information Search", driver, "TN", "Knox");
                        driver.FindElement(By.XPath("//*[@id='ctl00_PageContent_Ptax_master_search_vwTableControlRepeater_ctl00_DETAIL__Button']")).SendKeys(Keys.Enter);
                        Thread.Sleep(5000);
                        gc.CreatePdf(orderNumber, parcel_id, "City Tax Information Result", driver, "TN", "Knox");
                        string bulkInfo = driver.FindElement(By.XPath("//*[@id='ctl00_PageContent_Ptax_master_show_vwRecordControlPanel']/table/tbody")).Text;
                        string Property_Add = "", Owner_Name = "", Mailing_Address = "", Sub_Division = "", Block_Lots = "", Classification = "", Tax_Year = "", Appraised_Value = "", Assessed_Value = "", Tax_Rate = "", Tax_Levy = "", Tax_Discount = "", Tax_Paid = "", Prior_Year_Taxes = "", Total_Balance_Due = "", Tax_Authority = "";
                        Property_Add = gc.Between(bulkInfo, "Property Address:", "Appraised Value:").Trim();
                        Owner_Name = gc.Between(bulkInfo, "Owner Names:", "Assessed Value:").Trim();
                        Mailing_Address = gc.Between(bulkInfo, "Owner Address:", "Tax Rate:").Trim();
                        Sub_Division = gc.Between(bulkInfo, "Subdivision:", "Tax Discount:").Trim();
                        Block_Lots = gc.Between(bulkInfo, "Block Lots:", "Tax for 2017 if paid in 08/2018").Trim();
                        Classification = gc.Between(bulkInfo, "Classification:", "Taxes and Fees for Prior Years:").Trim();
                        Tax_Year = gc.Between(bulkInfo, "Current Tax Year", "Map of Property").Trim();
                        Appraised_Value = gc.Between(bulkInfo, "Appraised Value:", "County Tax Lookup").Trim();
                        Assessed_Value = gc.Between(bulkInfo, "Assessed Value:", "Owner Address:").Trim();
                        Tax_Rate = gc.Between(bulkInfo, "Tax Rate:", "Property ID / Ward").Trim();
                        Tax_Levy = gc.Between(bulkInfo, "Tax Levy:", "Subdivision:").Trim();
                        Tax_Discount = gc.Between(bulkInfo, "Tax Discount:", "Block Lots:").Trim().Replace("**Use the Printer Icon at the top of the page to print tax report.**", "");
                        Tax_Paid = gc.Between(bulkInfo, "Tax for 2017 if paid in 08/2018", "Classification:").Trim();
                        Prior_Year_Taxes = gc.Between(bulkInfo, "Taxes and Fees for Prior Years:", "Calculation Date").Trim();
                        Total_Balance_Due = gc.Between(bulkInfo, "Total Balance Due:", "For payment in a later month:").Trim();


                        // City Tax History
                        string intereist_penalty1 = "", Total1 = "";
                        IWebElement tbmulti1 = driver.FindElement(By.XPath("//*[@id='ctl00_PageContent_Ptax_master_show_vwTabContainer_Ptax_detailTabPanel_Ptax_detailTableControlCollapsibleRegion']/table/tbody/tr[2]/td/table/tbody"));
                        IList<IWebElement> TRmulti1 = tbmulti1.FindElements(By.TagName("tr"));

                        IList<IWebElement> TDmulti1;
                        foreach (IWebElement row in TRmulti1)
                        {

                            TDmulti1 = row.FindElements(By.TagName("td"));
                            if (TDmulti1.Count == 12)
                            {
                                string multi1 = TDmulti1[1].Text + "~" + TDmulti1[2].Text + "~" + TDmulti1[3].Text + "~" + TDmulti1[4].Text + "~" + TDmulti1[5].Text + "~" + TDmulti1[6].Text + "~" + TDmulti1[7].Text + "~" + TDmulti1[8].Text + "~" + TDmulti1[9].Text + "~" + TDmulti1[10].Text + "~" + TDmulti1[11].Text;
                                string intereist_penalty = TDmulti1[7].Text;
                                string Total = TDmulti1[11].Text;
                                if (!intereist_penalty.Contains("$0.00") || !Total.Contains("$0.00"))
                                {
                                    intereist_penalty1 = "Deliquent";
                                    Total1 = "Deliquent";
                                }
                                gc.insert_date(orderNumber, parcel_id, 734, multi1, 1, DateTime.Now);
                            }

                        }

                        if (intereist_penalty1 == "Deliquent" || Total1 == "Deliquent")
                        {
                            IWebElement dt = driver.FindElement(By.XPath("//*[@id='ctl00_PageContent_calculation_date1']"));
                            string date = dt.GetAttribute("value");

                            DateTime G_Date = Convert.ToDateTime(date);
                            string dateChecking = DateTime.Now.ToString("MM") + "/15/" + DateTime.Now.ToString("yyyy");

                            if (G_Date < Convert.ToDateTime(dateChecking))
                            {
                                //end of the month
                                date = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")))).ToString("MM/dd/yyyy");

                            }

                            else if (G_Date > Convert.ToDateTime(dateChecking))
                            {
                                // nextEndOfMonth 
                                if ((Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))) < 12))
                                {
                                    date = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM")) + 1), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")) + 1)).ToString("MM/dd/yyyy");
                                }
                                else
                                {
                                    int nxtYr = Convert.ToInt16(DateTime.Now.ToString("yyyy")) + 1;
                                    date = new DateTime(nxtYr, 1, DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), 1)).ToString("MM/dd/yyyy");

                                }
                            }

                            Thread.Sleep(2000);
                            dt.Clear();
                            try
                            {

                                IAlert alert = driver.SwitchTo().Alert();
                                alert.Accept();
                            }
                            catch
                            {
                            }
                            driver.FindElement(By.XPath("//*[@id='ctl00_PageContent_calculation_date1']")).SendKeys(date);

                        }


                        IWebElement GTD1 = driver.FindElement(By.XPath("//*[@id='ctl00_PageContent_calculation_date1']"));
                        string Good_Through_Date1 = GTD1.GetAttribute("value");
                        Tax_Authority = GlobalClass.After(bulkInfo, "Please mail payments to").Trim().Replace("\r\n", " ");
                        string City_Tax_Info1 = Property_Add + "~" + Owner_Name + "~" + Mailing_Address + "~" + Sub_Division + "~" + Block_Lots + "~" + Classification + "~" + Tax_Year + "~" + Appraised_Value + "~" + Assessed_Value + "~" + Tax_Rate + "~" + Tax_Levy + "~" + Tax_Discount + "~" + Tax_Paid + "~" + Prior_Year_Taxes + "~" + Total_Balance_Due + "~" + Good_Through_Date1 + "~" + Tax_Authority;
                        gc.insert_date(orderNumber, parcel_id, 732, City_Tax_Info1, 1, DateTime.Now);



                    }
                    catch (Exception ex)
                    {
                    }

                    CitytaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "TN", "Knox", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "TN", "Knox");
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