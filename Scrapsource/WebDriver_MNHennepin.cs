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
using System.Text;
using OpenQA.Selenium.Firefox;
using System.Web.UI;

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_MNHennepin
    {
        string outputPath = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();

        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        string Owner_name = "-", parcel_no = "-";

        string address = "-", Muncipality = "-", School_District = "-", Construction_year = "-", Taxpayer_address = "-";
        string addition_name = "-", lot = "-", block = "-", parcel_size = "-", abstract_torrens = "-";
        string Land_market = "-", Building_market = "-", Machinery_market = "-", Total_market = "-", Qualifying_improvements = "-", Veterans_exclusion = "-", Homestead_market_value_exclusion = "-", Property_type = "-", Homestead_status = "-", Relative_homestead = "-", Agricultural = "-", Exempt_status = "-";
        string Property_ID_number = "-", Property_address = "-", Owner_name1 = "-", Taxpayer_name_address = "-";

        public Page Page { get; private set; }

        public string FTP_MNHennepin(string houseno, string housedir, string sname, string sttype, string parcelNumber, string searchType, string orderNumber, string ownername, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                //driver.Manage().Window.Size = new Size(1920, 1080);
                // driver = new ChromeDriver();
                //ChromeOptions chromeOptions = new ChromeOptions();
                //chromeOptions.AddArguments("--headless");
                //chromeOptions.AddArguments("--start-maximized");
                //IWebDriver driver = new ChromeDriver(chromeOptions);

                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "MN", "Hennepin");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_Hennepin"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    driver.Navigate().GoToUrl("https://www.hennepin.us/residents/property/property-information-search");

                    if (searchType == "address")
                    {
                        driver.FindElement(By.XPath(".//*[@id='mainform']/div[3]/section/div/header/div/ul[1]/li[1]/a")).Click();
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "MN", "Hennepin");
                        Thread.Sleep(8000);
                        driver.SwitchTo().Window(driver.WindowHandles.Last());
                        Thread.Sleep(8000);
                        driver.FindElement(By.XPath("//*[@id='INPUT1']")).SendKeys(houseno);
                        driver.FindElement(By.XPath("//*[@id='INPUT3']")).SendKeys(sname);
                        driver.FindElement(By.XPath("//*[@id='INPUT4']")).SendKeys(Keys.Enter);
                        Thread.Sleep(6000);
                        gc.CreatePdf_WOP(orderNumber, "Address search result", driver, "MN", "Hennepin");
                        //Multi Parcel
                        try
                        {
                            if (driver.FindElement(By.XPath("/ html / body / div / div[2] / table / tbody / tr[2] / td[2] / div[2] / table / tbody")).Displayed)
                            {
                                string mul = driver.FindElement(By.XPath("/ html / body / div / div[2] / table / tbody / tr[2] / td[2] / div[2] / table / tbody")).Text;
                                mul = WebDriverTest.After(mul, "of").Trim();
                                if ((mul != "1") && (mul != "0"))
                                {
                                    //multi address
                                    IWebElement tbmulti = driver.FindElement(By.XPath("//*[@id='TABLE1']/tbody/tr[2]/td[2]/div[1]/table/tbody"));
                                    IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));
                                    IList<IWebElement> TDmulti;
                                    int maxCheck = 0;
                                    foreach (IWebElement row in TRmulti)
                                    {
                                        if (maxCheck <= 25)
                                        {
                                            TDmulti = row.FindElements(By.TagName("td"));
                                            if (TDmulti.Count != 0)
                                            {
                                                string multi1 = TDmulti[1].Text + "~" + TDmulti[2].Text;
                                                gc.insert_date(orderNumber, TDmulti[0].Text, 173, multi1, 1, DateTime.Now);
                                                // gc.insert_date(orderNumber, Parcel_No, 58, prop_details, 1, DateTime.Now);
                                            }
                                            maxCheck++;
                                        }
                                    }

                                    if (TRmulti.Count > 25)
                                    {
                                        HttpContext.Current.Session["multiParcel_Hennepin_Multicount"] = "Maximum";
                                    }
                                    else
                                    {
                                        HttpContext.Current.Session["multiparcel_Hennepin"] = "Yes";
                                    }
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                            }
                        }

                        catch { }

                    }
                    else if (searchType == "parcel")
                    {
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search result", driver, "MN", "Hennepin");
                        // Property ID number(PID)

                        driver.FindElement(By.XPath("/html/body/form/div[3]/section/div/header/div/ul[1]/li[2]/a")).Click();
                        //driver.FindElement(By.LinkText("Property ID number(PID)")).Click();
                        Thread.Sleep(6000);
                        driver.SwitchTo().Window(driver.WindowHandles.Last());
                        //  Thread.Sleep(5000);
                        string strparcelNumber = parcelNumber.Replace(" ", "").Replace("-", "");
                        Thread.Sleep(1000);
                        driver.FindElement(By.XPath("/html/body/div/div[2]/div/table/tbody/tr[2]/td[2]/div[1]/form/table/tbody/tr/td/table/tbody/tr[1]/td[2]/input")).SendKeys(parcelNumber);
                        driver.FindElement(By.XPath("//*[@id='TD4']/div[1]/form/table/tbody/tr/td/table/tbody/tr[2]/td[1]/input")).SendKeys(Keys.Enter);
                        Thread.Sleep(6000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search result", driver, "MN", "Hennepin");

                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.XPath("//*[@id='TABLE1']"));
                        if(INodata.Text.Contains("No records found"))
                        {
                            HttpContext.Current.Session["Nodata_Hennepin"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    //property details
                    //*[@id="TABLE_DETAILS"]
                    //TABLE_DETAILS
                    string bulkpropertytext = driver.FindElement(By.XPath("//*[@id='TABLE_DETAILS']")).Text;
                    parcel_no = gc.Between(bulkpropertytext, "Property ID number:", "Address:").Trim();
                    //if (parcel_no.Contains("-"))
                    //{
                    //    parcel_no = parcel_no.Replace("-", "");
                    //}
                    address = gc.Between(bulkpropertytext, "Address:", "Municipality:").Trim();
                    Muncipality = gc.Between(bulkpropertytext, "Municipality:", "School district:").Trim();
                    School_District = gc.Between(bulkpropertytext, "School district:", "Watershed:").Trim();
                    Construction_year = gc.Between(bulkpropertytext, "Construction year:", "Owner name:").Trim();
                    Owner_name = gc.Between(bulkpropertytext, "Owner name:", "Taxpayer name & address:").Trim();
                    try
                    {
                        Taxpayer_address = gc.Between(bulkpropertytext, "Taxpayer name & address:", "ONLY THE TAXABLE MARKET").Trim();
                        Taxpayer_address = Taxpayer_address.Replace("\r\n", " ");
                    }
                    catch
                    {

                    }
                    try
                    {
                        Taxpayer_address = gc.Between(bulkpropertytext, "Taxpayer name & address:", "Sale information").Trim();
                        Taxpayer_address = Taxpayer_address.Replace("\r\n", " ");
                    }
                    catch
                    {

                    }
                    //*[@id="TABLE_DETAILS"]/tbody/tr[21]/td[2]/p
                    addition_name = gc.Between(bulkpropertytext, "Addition name:", "Lot:").Trim();
                    lot = gc.Between(bulkpropertytext, "Lot:", "Block:").Trim();
                    block = gc.Between(bulkpropertytext, "Block:", "Approximate parcel size:").Trim();
                    parcel_size = gc.Between(bulkpropertytext, "Approximate parcel size:", "Metes & Bounds:").Trim();
                    try
                    {
                        abstract_torrens = gc.Between(bulkpropertytext, "Abstract or Torrens:", "Value and tax summary").Trim();
                    }
                    catch { }
                    string property_details = address + "~" + Muncipality + "~" + School_District + "~" + Construction_year + "~" + Owner_name + "~" + Taxpayer_address + "~" + addition_name + "~" + lot + "~" + block + "~" + parcel_size + "~" + abstract_torrens;
                    gc.insert_date(orderNumber, parcel_no, 164, property_details, 1, DateTime.Now);

                    //assessment details
                    Land_market = driver.FindElement(By.XPath(" //*[@id='Table_Detail']/tbody/tr[4]/td[2]")).Text.Trim();
                    Building_market = driver.FindElement(By.XPath(" //*[@id='Table_Detail']/tbody/tr[5]/td[2]")).Text.Trim();
                    Machinery_market = driver.FindElement(By.XPath(" //*[@id='Table_Detail']/tbody/tr[6]/td[2]")).Text.Trim();
                    Total_market = driver.FindElement(By.XPath(" //*[@id='Table_Detail']/tbody/tr[7]/td[2]")).Text.Trim();
                    Qualifying_improvements = driver.FindElement(By.XPath(" //*[@id='Table_Detail']/tbody/tr[8]/td[2]")).Text.Trim();
                    Veterans_exclusion = driver.FindElement(By.XPath(" //*[@id='Table_Detail']/tbody/tr[9]/td[2]")).Text.Trim();
                    Homestead_market_value_exclusion = driver.FindElement(By.XPath(" //*[@id='Table_Detail']/tbody/tr[10]/td[2]")).Text.Trim();
                    Property_type = driver.FindElement(By.XPath(" //*[@id='Table_Detail']/tbody/tr[12]/td[2]")).Text.Trim();
                    Homestead_status = driver.FindElement(By.XPath(" //*[@id='Table_Detail']/tbody/tr[13]/td[2]")).Text.Trim();
                    Relative_homestead = driver.FindElement(By.XPath(" //*[@id='Table_Detail']/tbody/tr[14]/td[2]")).Text.Trim();
                    Agricultural = driver.FindElement(By.XPath(" //*[@id='Table_Detail']/tbody/tr[15]/td[2]")).Text.Trim();
                    Exempt_status = driver.FindElement(By.XPath(" //*[@id='Table_Detail']/tbody/tr[16]/td[2]")).Text.Trim();

                    string assessment_details = Land_market + "~" + Building_market + "~" + Machinery_market + "~" + Total_market + "~" + Qualifying_improvements + "~" + Veterans_exclusion + "~" + Homestead_market_value_exclusion + "~" + Property_type + "~" + Homestead_status + "~" + Relative_homestead + "~" + Agricultural + "~" + Exempt_status;
                    gc.insert_date(orderNumber, parcel_no, 169, assessment_details, 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    //2018 State Copy
                    try
                    {
                        driver.FindElement(By.PartialLinkText("state copy")).Click();

                        Thread.Sleep(4000);
                        gc.CreatePdf_WOP(orderNumber, "CurrentYear state copy", driver, "MN", "Hennepin");
                        driver.Navigate().Back();

                    }
                    catch (Exception e)
                    {

                    }
                    //current year tax due


                    string strURL = "https://www16.co.hennepin.mn.us/taxpayments/taxesdue.jsp?pid=" + parcel_no;

                    // IWebElement current = driver.FindElement(By.LinkText("Current year taxes due"));
                    // current.SendKeys(Keys.Enter);
                    //string URL1 = current.GetAttribute("href");
                    driver.Navigate().GoToUrl(strURL);
                    //driver.FindElement(By.LinkText("Current year taxes due")).SendKeys(Keys.Enter);
                    Thread.Sleep(6000);
                    gc.CreatePdf_WOP(orderNumber, "Current year taxes due", driver, "MN", "Hennepin");
                    //delinquent tax
                    try
                    {
                        if (driver.FindElement(By.XPath("/html/body/div[1]/section/div/div/div[4]/p")).Displayed)
                        {
                            string delinquenttax = driver.FindElement(By.XPath("/html/body/div[1]/section/div/div/div[4]/p")).Text.Trim();
                            HttpContext.Current.Session["delinquent_Hennepin"] = "Yes";

                        }
                    }
                    catch
                    {
                    }
                    //Property tax information Table
                    //*[@id="taxesdue"]/table/tbody/tr[1]/td[2]

                    try
                    {
                        Property_ID_number = driver.FindElement(By.XPath("//*[@id='taxesdue']/table/tbody/tr[1]/td[2]")).Text.Trim();
                        Property_address = driver.FindElement(By.XPath("//*[@id='taxesdue']/table/tbody/tr[2]/td[2]")).Text.Trim();
                        Owner_name1 = driver.FindElement(By.XPath("//*[@id='taxesdue']/table/tbody/tr[3]/td[2]")).Text.Trim();
                        Taxpayer_name_address = driver.FindElement(By.XPath("//*[@id='taxesdue']/table/tbody/tr[4]/td[2]")).Text.Trim();
                        Taxpayer_name_address = Taxpayer_name_address.Replace("\r\n", " ");

                        string tax_year = "-", tax_type = "-";
                        //*[@id="TH1"]                
                        tax_year = driver.FindElement(By.XPath("//*[@id='TH1']")).Text.Trim();
                        string property_tax1 = Property_ID_number + "~" + tax_year + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-";
                        gc.insert_date(orderNumber, parcel_no, 171, property_tax1, 1, DateTime.Now);



                        IWebElement multitableElement1 = driver.FindElement(By.XPath("//*[@id='TBODY4']"));
                        IList<IWebElement> multitableRow1 = multitableElement1.FindElements(By.TagName("tr"));
                        IList<IWebElement> multirowTD1;
                        foreach (IWebElement row in multitableRow1)
                        {
                            if (!row.Text.Contains("taxes"))
                            {
                                multirowTD1 = row.FindElements(By.TagName("td"));
                                if (multirowTD1.Count != 1 && row.Text.Trim() != "")
                                {
                                    string property_tax2 = Property_ID_number + "~" + "-" + "~" + multirowTD1[0].Text.Trim() + "~" + multirowTD1[1].Text.Trim() + "~" + multirowTD1[2].Text.Trim() + "~" + multirowTD1[3].Text.Trim();
                                    gc.insert_date(orderNumber, parcel_no, 171, property_tax2, 1, DateTime.Now);

                                }

                            }



                        }

                        string Ist_half = "-", Ist_half_amount = "-", IInd_half = "-", IInd_half_amount = "-", total_due = "-", total_due_amount = "-", msg = "-", msgadd = "-";
                        Ist_half = driver.FindElement(By.XPath("//*[@id='TABLE_SEL']/tbody/tr[1]/td[2]")).Text.Trim();
                        Ist_half_amount = driver.FindElement(By.XPath("//*[@id='TABLE_SEL']/tbody/tr[1]/td[4]")).Text.Trim();
                        string property_tax3 = Property_ID_number + "~" + "-" + "~" + Ist_half + "~" + "-" + "~" + "-" + "~" + Ist_half_amount;
                        gc.insert_date(orderNumber, parcel_no, 171, property_tax3, 1, DateTime.Now);


                        IInd_half = driver.FindElement(By.XPath("//*[@id='TABLE_SEL']/tbody/tr[2]/td[2]")).Text.Trim();
                        IInd_half_amount = driver.FindElement(By.XPath("//*[@id='TABLE_SEL']/tbody/tr[2]/td[4]")).Text.Trim();
                        string property_tax4 = Property_ID_number + "~" + "-" + "~" + IInd_half + "~" + "-" + "~" + "-" + "~" + IInd_half_amount;
                        gc.insert_date(orderNumber, parcel_no, 171, property_tax4, 1, DateTime.Now);

                        total_due = driver.FindElement(By.XPath("//*[@id='TABLE_SEL']/tbody/tr[4]/td[1]")).Text.Trim();
                        total_due_amount = driver.FindElement(By.XPath("//*[@id='TABLE_SEL']/tbody/tr[4]/td[3]")).Text.Trim();


                        msg = driver.FindElement(By.XPath("/html/body/div[1]/section/div/div/p")).Text.Trim();
                        msg = msg.Replace("\r\n", " ");
                        string property_tax6 = msg + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-";
                        gc.insert_date(orderNumber, parcel_no, 171, property_tax6, 1, DateTime.Now);

                        msgadd = driver.FindElement(By.XPath("//*[@id='taxesdue']/p/strong")).Text.Trim();
                        msgadd = msgadd.Replace("\r\n", " ");
                        string property_tax7 = "Tax Authority  :" + "~" + msgadd + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-";
                        gc.insert_date(orderNumber, parcel_no, 171, property_tax7, 1, DateTime.Now);
                        driver.Navigate().Back();
                        Thread.Sleep(4000);
                    }
                    catch
                    {
                        driver.Navigate().Back();
                    }
                    //Prior Year Taxes
                    // driver.Navigate().GoToUrl(strURL);
                    driver.FindElement(By.PartialLinkText("Prior year taxes")).Click();
                    Thread.Sleep(4000);
                    gc.CreatePdf_WOP(orderNumber, "Prior year taxes", driver, "MN", "Hennepin");
                    //Tax Summary
                    //   Date~Estimated_market_Value~Taxable_market_Value~Total_improvement_amount~Total_net_tax~Total_special_assessments~Solid_waste_fee~Total_Tax

                    string Estimated_market_Value = "-", Taxable_market_Value = "-", Total_improvement_amount = "-", Total_net_tax = "-", Total_special_assessments = "-", Solid_waste_fee = "-", Total_Tax = "-";
                    try
                    {

                        string fulltabletext = driver.FindElement(By.XPath("//*[@id='TABLE_DETAILS']")).Text;

                        Estimated_market_Value = gc.Between(fulltabletext, "Estimated market value:", "Taxable market value:").Trim();
                        Taxable_market_Value = gc.Between(fulltabletext, "Taxable market value:", "Total improvement amount:").Trim();
                        Total_improvement_amount = gc.Between(fulltabletext, "Total improvement amount:", "Total net tax:").Trim();
                        Total_net_tax = gc.Between(fulltabletext, "Total net tax:", "Total special assessments:").Trim();
                        Total_special_assessments = gc.Between(fulltabletext, "Total special assessments:", "Solid waste fee:").Trim();
                        Solid_waste_fee = gc.Between(fulltabletext, "Solid waste fee:", "Total Tax:").Trim();
                        Total_Tax = GlobalClass.After(fulltabletext, "Total Tax:").Trim().Replace("\r\n", "");
                        string tax_Summary = Estimated_market_Value + "~" + Taxable_market_Value + "~" + Total_improvement_amount + "~" + Total_net_tax + "~" + Total_special_assessments + "~" + Solid_waste_fee + "~" + Total_Tax;
                        gc.insert_date(orderNumber, parcel_no, 172, tax_Summary, 1, DateTime.Now);
                        driver.Navigate().Back();
                        //Current Year Value
                        driver.FindElement(By.LinkText("Current year values")).Click();
                        Thread.Sleep(4000);
                        gc.CreatePdf_WOP(orderNumber, "Current Year Value", driver, "MN", "Hennepin");
                        driver.Navigate().Back();
                    }
                    catch
                    { }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "MN", "Hennepin", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "MN", "Hennepin");
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