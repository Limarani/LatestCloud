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
using OpenQA.Selenium.Firefox;
using System.Diagnostics;
using OpenQA.Selenium.Remote;
namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_StLouis
    {
        string Outparcelno = "", outputPath = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        private TimeSpan timeOutInSeconds;
        MySqlParameter[] mParam;

        public string FTP_STLouis(string houseno, string sname, string sttype, string parcelNumber, string searchType, string orderNumber, string ownername, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            IWebElement iframeElement1;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
                string locator_no = "-", owner_name = "-", taxing_address = "-", city_name = "-", subdivison_name = "-", legal_description = "-", year_built = "-";
                string locator_num = "-", prop_location = "-", tax_authority = "St. Louis County Government,41 South Central, Clayton, Missouri 63105", dele_status = "-";


                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    if (searchType == "titleflex")
                    {
                        string address = houseno + " " + sname;
                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "SC", "Charleston");
                        if (HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes")
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_WilliamsonTX"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("http://revenue.stlouisco.com/IAS/");

                        //find ifram using xpath

                        IWebElement iframeElement = driver.FindElement(By.XPath("/html/frameset/frameset/frame[1]"));
                        //now use the switch command
                        driver.SwitchTo().Frame(iframeElement);
                        Thread.Sleep(3000);
                        driver.FindElement(By.XPath("/html/body/form/table/tbody/tr[2]/td[1]/div[3]/span/label")).Click();
                        Thread.Sleep(3000);
                        driver.FindElement(By.XPath("//*[@id='tboxAddrNum']")).SendKeys(houseno);
                        driver.FindElement(By.XPath("//*[@id='tboxStreet']")).SendKeys(sname);
                        driver.FindElement(By.XPath("//*[@id='butFind']")).SendKeys(Keys.Enter);

                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "MO", "Saint Louis");

                        //multi parecl
                        driver.SwitchTo().DefaultContent();
                        iframeElement1 = driver.FindElement(By.XPath("/html/frameset/frameset/frame[2]"));
                        driver.SwitchTo().Frame(iframeElement1);
                        Thread.Sleep(3000);
                        string count = driver.FindElement(By.XPath("//*[@id='labelTotalRows']")).Text.Trim();
                        if (count != "1 Record Found")
                        {
                            Multi_Parcel_Search(orderNumber);
                            driver.Quit();
                            return "MultiParcel";
                        }

                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='panelData']")).Text;
                            if (nodata.Contains("No records were found"))
                            {
                                HttpContext.Current.Session["Nodata_StLouis"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    else if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("http://revenue.stlouisco.com/IAS/");
                        //find ifram using xpath
                        IWebElement iframeElement = driver.FindElement(By.XPath("/html/frameset/frameset/frame[1]"));
                        //now use the switch command
                        driver.SwitchTo().Frame(iframeElement);
                        Thread.Sleep(3000);
                        driver.FindElement(By.XPath("//*[@id='tboxLocatorNum']")).SendKeys(parcelNumber);
                        driver.FindElement(By.XPath("//*[@id='butFind']")).SendKeys(Keys.Enter);

                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search", driver, "MO", "Saint Louis");

                        driver.SwitchTo().DefaultContent();
                        iframeElement1 = driver.FindElement(By.XPath("/html/frameset/frameset/frame[2]"));
                        driver.SwitchTo().Frame(iframeElement1);
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='panelData']")).Text;
                            if (nodata.Contains("No records were found"))
                            {
                                HttpContext.Current.Session["Nodata_StLouis"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }

                    }

                    else if (searchType == "ownername")
                    {
                        driver.Navigate().GoToUrl("http://revenue.stlouisco.com/IAS/");
                        //find ifram using xpath
                        IWebElement iframeElement = driver.FindElement(By.Name("SearchInput"));
                        //now use the switch command
                        driver.SwitchTo().Frame(iframeElement);
                        Thread.Sleep(3000);
                        driver.FindElement(By.Id("rbutName")).Click();
                        Thread.Sleep(3000);
                        string firstname = "", lastname = "";
                        if (ownername.Contains(" "))
                        {
                            string[] name = ownername.Split(' ');
                            firstname = name[0];
                            lastname = name[1];
                            driver.FindElement(By.XPath("//*[@id='tboxLastName']")).SendKeys(firstname);
                            driver.FindElement(By.XPath("//*[@id='tboxFirstName']")).SendKeys(lastname);
                            driver.FindElement(By.XPath("//*[@id='butFind']")).SendKeys(Keys.Enter);
                        }
                        else
                        {

                            driver.FindElement(By.XPath("//*[@id='tboxLastName']")).SendKeys(ownername);
                            driver.FindElement(By.XPath("//*[@id='butFind']")).SendKeys(Keys.Enter);
                        }
                        gc.CreatePdf_WOP(orderNumber, "Owner Search", driver, "MO", "Saint Louis");

                        driver.SwitchTo().DefaultContent();
                        iframeElement1 = driver.FindElement(By.XPath("/html/frameset/frameset/frame[2]"));
                        driver.SwitchTo().Frame(iframeElement1);
                        Thread.Sleep(3000);
                        try
                        {
                            string count = driver.FindElement(By.XPath("//*[@id='labelTotalRows']")).Text.Trim();
                            string getcount = WebDriverTest.Before(count, "Record Found").Trim();
                            int igetcount = Convert.ToInt16(getcount);
                            if (igetcount <= 10)
                            {

                                if (count != "1 Record Found")
                                {
                                    Multi_Parcel_Search(orderNumber);
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                            }
                            else
                            {
                                HttpContext.Current.Session["multiparcel_StLouis_count"] = "Maximum";
                                return "Maximum";
                            }
                        }
                        catch
                        { }

                        try
                        {
                            string count = driver.FindElement(By.XPath("//*[@id='labelTotalRows']")).Text.Trim();
                            string getcount = WebDriverTest.Before(count, "Records Found").Trim();
                            int igetcount = Convert.ToInt16(getcount);
                            if (igetcount <= 10)
                            {

                                if (count != "1 Records Found")
                                {
                                    Multi_Parcel_Search(orderNumber);
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                            }
                            else
                            {
                                HttpContext.Current.Session["multiparcel_StLouis_count"] = "Maximum";
                                return "Maximum";
                            }
                        }
                        catch
                        { }


                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='panelData']")).Text;
                            if (nodata.Contains("No records were found"))
                            {
                                HttpContext.Current.Session["Nodata_StLouis"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    //Thread.Sleep(3000);
                    //driver.SwitchTo().DefaultContent();
                    //iframeElement1 = driver.FindElement(By.XPath("/html/frameset/frameset/frame[2]"));
                    //driver.SwitchTo().Frame(iframeElement1);
                    driver.FindElement(By.XPath("/html/body/form/div[3]/center/table/tbody/tr[3]/td/table/tbody/tr[2]/td[3]/img")).Click();
                    driver.SwitchTo().DefaultContent();
                    Thread.Sleep(1000);

                    //owner ship and legal information
                    // Property Details Table.
                    IWebElement iframeElement2 = driver.FindElement(By.XPath("/html/frameset/frame"));
                    driver.SwitchTo().Frame(iframeElement2);

                    Thread.Sleep(3000);
                    //assessment details
                    locator_no = driver.FindElement(By.XPath("//*[@id='ctl00_MainContent_OwnLeg_labLocatorNum']")).Text;
                    int k = 0, j = 0;
                    IWebElement TBAssessment = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/form/table/tbody/tr/td[2]/div[3]/div[4]/div[2]/div[1]/div[2]/table/tbody"));
                    IList<IWebElement> TRAssessment = TBAssessment.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDAssessment;
                    foreach (IWebElement row1 in TRAssessment)
                    {
                        TDAssessment = row1.FindElements(By.TagName("td"));
                        if (TDAssessment.Count != 0 && !row1.Text.Contains("Appraised Values") && !row1.Text.Contains("Year"))
                        {
                            try
                            {
                                //click expnad the year
                                IList<IWebElement> expand = TBAssessment.FindElements(By.XPath("//tr[contains(@id,'trAsmt')]/td[1]/img"));

                                foreach (IWebElement exe in expand)
                                {
                                    if (j > 0 && j < 3)
                                    {
                                        IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                                        js.ExecuteScript("arguments[0].click();", exe);
                                        Thread.Sleep(1000);

                                    }
                                    j++;
                                }
                            }
                            catch { }

                            //store the values
                            IList<IWebElement> residential_values = TBAssessment.FindElements(By.XPath("//tr[contains(@id,'trAsmt')]"));
                            foreach (IWebElement rv in residential_values)
                            {
                                try
                                {
                                    if (k > 0 && k < 4)
                                    {
                                        //insert details
                                        TDAssessment = rv.FindElements(By.TagName("td"));
                                        string assessment_details = TDAssessment[1].Text.Trim() + "~" + TDAssessment[3].Text.Trim() + "~" + TDAssessment[4].Text.Trim() + "~" + TDAssessment[5].Text.Trim() + "~" + TDAssessment[7].Text.Trim() + "~" + TDAssessment[8].Text.Trim() + "~" + TDAssessment[9].Text.Trim();
                                        gc.insert_date(orderNumber, locator_no, 23, assessment_details, 1, DateTime.Now);

                                    }
                                    k++;
                                }
                                catch (Exception e)
                                { }
                            }

                            gc.CreatePdf(orderNumber, locator_no, "Assessment Details", driver, "MO", "Saint Louis");

                        }
                    }



                    // Property Details Table.

                    owner_name = driver.FindElement(By.XPath("//*[@id='ctl00_MainContent_OwnLeg_labOwnerName']")).Text;
                    taxing_address = driver.FindElement(By.XPath("//*[@id='ctl00_MainContent_OwnLeg_labTaxAddr']")).Text;
                    city_name = driver.FindElement(By.XPath("//*[@id='ctl00_MainContent_OwnLeg_labCityName']")).Text;
                    subdivison_name = driver.FindElement(By.XPath("//*[@id='ctl00_MainContent_OwnLeg_labSubdivisionName']")).Text;
                    legal_description = driver.FindElement(By.XPath("//*[@id='ctl00_MainContent_OwnLeg_labLegalDesc']")).Text;

                    driver.FindElement(By.ClassName("RevDeptContentLink")).SendKeys(Keys.Enter);
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, locator_no, "PropertyDetails", driver, "MO", "Saint Louis");
                    Thread.Sleep(2000);
                    try
                    {
                        year_built = driver.FindElement(By.XPath("//*[@id='ctl00_MainContent_DwellingDataRes_labYearBuilt']")).Text;
                    }
                    catch { }

                    string property_details = owner_name + "~" + taxing_address + "~" + city_name + "~" + subdivison_name + "~" + legal_description + "~" + year_built;
                    if (property_details.Contains("\r\n"))
                    {
                        property_details = property_details.Replace("\r\n", "");
                        gc.insert_date(orderNumber, locator_no, 22, property_details, 1, DateTime.Now);
                        //db.ExecuteQuery("insert into data_value_master (Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table) values ('" + orderNumber + "','" + multirowTD[0].Text.Trim() + "',19 ,'" + multi + "',1)");

                    }
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");


                    Thread.Sleep(2000);
                    //Tax amount due
                    driver.FindElement(By.XPath("//*[@id='ctl00_LeftMargin_MarginLinks_aTaxDue']")).SendKeys(Keys.Enter);
                    Thread.Sleep(3000);
                    //need to add tax authority
                    try
                    {
                        IWebElement TBTaxDue = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[3]/td/form/table/tbody/tr/td[2]/div[5]/div[2]/div[1]/table/tbody/tr[2]/td/table/tbody"));
                        IList<IWebElement> TRTaxDue = TBTaxDue.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDTaxDue;
                        int i = 0;
                        int count = TRTaxDue.Count;


                        //if condition used for display two tables

                        foreach (IWebElement row1 in TRTaxDue)
                        {
                            if (!row1.Text.Contains("Locator Number") && !row1.Text.Contains("Tax Year"))
                            {
                                TDTaxDue = row1.FindElements(By.TagName("td"));
                                int TDcount = TDTaxDue.Count();
                                if (TDcount == 8)
                                {

                                    //insert details
                                    string Tax_details = TDTaxDue[0].Text.Trim() + "~" + TDTaxDue[1].Text.Trim() + "~" + TDTaxDue[2].Text.Trim() + "~" + TDTaxDue[3].Text.Trim() + "~" + TDTaxDue[4].Text.Trim() + "~" + TDTaxDue[5].Text.Trim() + "~" + TDTaxDue[6].Text.Trim() + "~" + TDTaxDue[7].Text.Trim() + "~" + "-" + "~" + "-";
                                    gc.insert_date(orderNumber, locator_no, 24, Tax_details, 1, DateTime.Now);
                                }

                                if (TDcount == 6 || TDcount == 2)
                                {
                                    if (i < count - 3)
                                    {

                                        //insert details
                                        string Tax_details = TDTaxDue[0].Text.Trim() + "~" + TDTaxDue[1].Text.Trim() + "~" + TDTaxDue[2].Text.Trim() + "~" + TDTaxDue[3].Text.Trim() + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + TDTaxDue[4].Text.Trim() + "~" + TDTaxDue[5].Text.Trim();
                                        gc.insert_date(orderNumber, locator_no, 24, Tax_details, 1, DateTime.Now);
                                    }
                                    if (i == count - 3)
                                    {
                                        //insert details
                                        string Tax_details = TDTaxDue[0].Text.Trim() + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + TDTaxDue[1].Text.Trim();
                                        gc.insert_date(orderNumber, locator_no, 24, Tax_details, 1, DateTime.Now);
                                    }

                                    i++;
                                }

                            }

                        }

                        gc.CreatePdf(orderNumber, locator_no, "tax Details", driver, "MO", "Saint Louis");
                    }
                    catch (Exception e) { }

                    driver.Navigate().Back();
                    Thread.Sleep(3000);
                    //Tax History
                    driver.SwitchTo().DefaultContent();
                    Thread.Sleep(3000);
                    IWebElement iframeElement21 = driver.FindElement(By.XPath("/html/frameset/frame"));
                    driver.SwitchTo().Frame(iframeElement21);
                    Thread.Sleep(3000);
                    driver.FindElement(By.Id("ctl00_LeftMargin_MarginLinks_aTaxHistory")).SendKeys(Keys.Enter);
                    Thread.Sleep(3000);
                    locator_num = driver.FindElement(By.XPath("//*[@id='ctl00_MainContent_RealEstateHistoryData1_labelLocatorNum']")).Text;
                    owner_name = driver.FindElement(By.XPath("//*[@id='ctl00_MainContent_RealEstateHistoryData1_labelOwner']")).Text;
                    prop_location = driver.FindElement(By.XPath("//*[@id='ctl00_MainContent_RealEstateHistoryData1_labelLocation']")).Text;

                    IWebElement TBTax_History = driver.FindElement(By.XPath("//*[@id='ctl00_MainContent_RealEstateHistoryData1_tableTaxHistory']/tbody"));
                    IList<IWebElement> TRTax_History = TBTax_History.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDTax_History;
                    int a = 0;
                    foreach (IWebElement row1 in TRTax_History)
                    {
                        if (!row1.Text.Contains("Tax Year"))
                        {
                            if (a < 3)
                            {
                                TDTax_History = row1.FindElements(By.TagName("td"));
                                //insert details
                                string TaxHistory_details = owner_name + "~" + prop_location + "~" + TDTax_History[0].Text.Trim() + "~" + TDTax_History[2].Text.Trim() + "~" + TDTax_History[3].Text.Trim() + "~" + TDTax_History[4].Text.Trim() + "~" + TDTax_History[5].Text.Trim() + "~" + TDTax_History[6].Text.Trim() + "~" + TDTax_History[7].Text.Trim() + "~" + tax_authority + "~" + "-";
                                gc.insert_date(orderNumber, locator_num, 25, TaxHistory_details, 1, DateTime.Now);

                            }

                            a++;
                        }

                    }

                    gc.CreatePdf(orderNumber, locator_no, "Tax_History", driver, "MO", "Saint Louis");

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");

                    gc.insert_TakenTime(orderNumber, "MO", "Saint Louis", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);


                    driver.Quit();
                    gc.mergpdf(orderNumber, "MO", "Saint Louis");
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

        //Multi Parcel
        public void Multi_Parcel_Search(string orderno)
        {

            IWebElement multitableElement = driver.FindElement(By.XPath("/html/body/form/div[3]/center/table/tbody/tr[3]/td/table/tbody"));
            IList<IWebElement> multitableRow = multitableElement.FindElements(By.TagName("tr"));
            IList<IWebElement> multirowTD;
            foreach (IWebElement row in multitableRow)
            {
                multirowTD = row.FindElements(By.TagName("td"));
                if (multirowTD.Count != 0 && !row.Text.Contains("Map"))
                {
                    string multi = multirowTD[3].Text.Trim() + "~" + multirowTD[4].Text.Trim();
                    gc.insert_date(orderno, multirowTD[2].Text.Trim(), 38, multi, 1, DateTime.Now);
                }
            }

            HttpContext.Current.Session["multiparcel_StLouis"] = "Yes";
        }     

    }
}