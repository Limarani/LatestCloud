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
namespace ScrapMaricopa.Scrapsource
{
    public class Webdriver_StaffordVA
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();

        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_Stafford(string address, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel, string taxmapnumber)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            GlobalClass.sname = "VA";
            GlobalClass.cname = "Stafford";
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {

                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    // Property Details
                    driver.Navigate().GoToUrl("http://va-stafford-assessor.publicaccessnow.com/PropertySearch.aspx");
                    Thread.Sleep(4000);

                    if (searchType == "titleflex")
                    {
                        if (ownername != "")
                        {
                            address = "";
                        }
                        string titleaddress = address;
                        gc.TitleFlexSearch(orderNumber, "", ownername, titleaddress, "VA", "Stafford");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_StaffordVA"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";

                    }

                    if (searchType == "address")
                    {
                        driver.FindElement(By.Id("fldSearchFor")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "VA", "Stafford");

                        driver.FindElement(By.XPath("//*[@id='QuickSearch']/div[1]/div[1]/table/tbody/tr/td[2]/button[1]")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        try
                        {
                            string mul = "";
                            IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='QuickSearch']/div[1]/div[2]"));
                            mul = gc.Between(multiaddress.Text, "Your search returned ", "records.").Trim();
                            int count = Convert.ToInt32(mul);
                            if ((mul != "1") && (mul != "0"))
                            {
                                for (int i = 1; i < count; i++)
                                {
                                    string Iparcelno = driver.FindElement(By.XPath("//*[@id='QuickSearch']/div[2]/div[" + i + "]/ul[2]/li[1]/a")).Text;
                                    string pro_add = driver.FindElement(By.XPath("//*[@id='QuickSearch']/div[2]/div[" + i + "]/ul[2]/li[3]")).Text;

                                    gc.insert_date(orderNumber, Iparcelno, 696, pro_add, 1, DateTime.Now);

                                }

                                gc.CreatePdf_WOP(orderNumber, "Multi Address search result", driver, "VA", "Stafford");
                                HttpContext.Current.Session["multiparcel_StaffordVA"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            else
                            {

                            }
                        }
                        catch { }

                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.XPath("//*[@id='fldSearchFor']")).SendKeys(parcelNumber.Replace("-", "").Replace(" ", ""));
                        gc.CreatePdf(orderNumber, parcelNumber.Replace("-", "").Replace(" ", ""), "parcel search", driver, "VA", "Stafford");
                        driver.FindElement(By.XPath("//*[@id='QuickSearch']/div[1]/div[1]/table/tbody/tr/td[2]/button[1]")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);

                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.Id("QuickSearch"));
                        if (INodata.Text.Contains("no records were found"))
                        {
                            HttpContext.Current.Session["Nodata_StaffordVA"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    string alternate_id = "", property_address = "";
                    string bulkaccesstext = driver.FindElement(By.XPath("//*[@id='QuickSearch']/div[2]")).Text.Trim().Replace("\r\n", "");

                    alternate_id = gc.Between(bulkaccesstext, "ALTERNATE ID/PIN", "PROPERTY ADDRESS").Trim().Replace(":", "");
                    property_address = gc.Between(bulkaccesstext, "PROPERTY ADDRESS", "MAILING ADDRESS").Trim().Replace(":", "");
                    gc.CreatePdf(orderNumber, parcelNumber.Replace("-", "").Replace(" ", ""), "Tax information search", driver, "VA", "Stafford");
                    driver.FindElement(By.XPath("//*[@id='QuickSearch']/div[2]/div[1]/ul[2]/li[1]/a")).SendKeys(Keys.Enter);
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, parcelNumber.Replace("-", "").Replace(" ", ""), "Tax information search result", driver, "VA", "Stafford");
                    string property_ID = "", owner_name = "", land_type = "", acres = "", property_class = "", neighborhood_code = "", legal_description = "";
                    string bulkgenralinfotext = driver.FindElement(By.XPath("//*[@id='dnn_wrapper']/div[5]/div[4]/table/tbody/tr/td[2]/table/tbody")).Text.Trim();
                    property_ID = gc.Between(bulkgenralinfotext, "Property ID", "Alternate ID/PIN").Trim();
                    parcelNumber = property_ID.Replace(" ", "").Replace("-", "");
                    owner_name = gc.Between(bulkgenralinfotext, "General Info", "Property ID").Trim();
                    property_class = gc.Between(bulkgenralinfotext, "Property Class", "Neighborhood").Trim();
                    acres = gc.Between(bulkgenralinfotext, "Deed Acres", "Value History").Trim();
                    string year_built = "";
                    if (bulkgenralinfotext.Contains("Year Built"))
                    {
                        year_built = gc.Between(bulkgenralinfotext, "Year Built", "Attributes").Trim();
                    }

                    neighborhood_code = gc.Between(bulkgenralinfotext, "Neighborhood", "Deed Acres").Trim();
                    land_type = gc.Between(bulkgenralinfotext, "Land Type Acres", "Legal Descriptions").Trim().Replace("0.29", "");
                    legal_description = GlobalClass.After(bulkgenralinfotext, "Description").Trim().Replace("\r\n", "");
                    string propertydetails = alternate_id + "~" + property_address + "~" + property_class + "~" + neighborhood_code + "~" + owner_name + "~" + year_built + "~" + land_type + "~" + acres + "~" + legal_description;
                    gc.insert_date(orderNumber, property_ID, 664, propertydetails, 1, DateTime.Now);

                    try
                    {
                        var chromeOptions = new ChromeOptions();

                        var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];

                        chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                        chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                        chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                        var driver1 = new ChromeDriver(chromeOptions);
                        driver1.Navigate().GoToUrl(driver.Url);
                        string fileName = "report.pdf";

                        IWebElement assessme = driver1.FindElement(By.XPath("//*[@id='lxT470']/p/a[3]"));
                        assessme.Click();
                        Thread.Sleep(3000);
                        //string ass = assessme.GetAttribute("href");
                        try
                        {
                            gc.AutoDownloadFile(orderNumber, parcelNumber, "Stafford", "VA", fileName);
                        }
                        catch { }

                        driver1.Quit();
                        string FilePath = gc.filePath(orderNumber, parcelNumber) + "report.pdf";
                        PdfReader reader;
                        string pdfData;
                        string pdftext = "";
                        try
                        {
                            reader = new PdfReader(FilePath);
                            String textFromPage = PdfTextExtractor.GetTextFromPage(reader, 1);
                            System.Diagnostics.Debug.WriteLine("" + textFromPage);

                            pdftext = textFromPage;
                        }
                        catch { }


                        string tableassess = gc.Between(pdftext, "Use", "Sales History").Trim();
                        string[] tableArray = tableassess.Split('\n');

                        int count1 = tableArray.Length;
                        for (int i = 0; i < count1; i++)
                        {
                            // 
                            string a1 = tableArray[i].Replace(" ", "~");
                            string[] rowarray = a1.Split('~');
                            int tdcount = rowarray.Length;
                            if (tdcount == 9)
                            {
                                gc.insert_date(orderNumber, property_ID, 745, a1, 1, DateTime.Now);
                            }
                            else if (tdcount == 8)
                            {
                                int j = 0;
                                string newrow = rowarray[j] + "~" + "" + "~" + rowarray[j + 1] + "~" + rowarray[j + 2] + "~" + rowarray[j + 3] + "~" + rowarray[j + 4] + "~" + rowarray[j + 5] + "~" + rowarray[j + 6] + "~" + rowarray[j + 7];
                                gc.insert_date(orderNumber, property_ID, 745, newrow, 1, DateTime.Now);
                            }
                            else if (tdcount == 10)
                            {
                                int k = 0;
                                string newrow = rowarray[k] + "~" + rowarray[k + 1] + " " + rowarray[k + 2] + "~" + rowarray[k + 3] + "~" + rowarray[k + 4] + "~" + rowarray[k + 5] + "~" + rowarray[k + 6] + "~" + rowarray[k + 7] + "~" + rowarray[k + 8] + "~" + rowarray[k + 9];
                                gc.insert_date(orderNumber, property_ID, 745, newrow, 1, DateTime.Now);
                            }
                        }
                    }
                    catch
                    {

                    }
                    //Year~Reason~Appraised Land~Appraised Improvements~Appraised Total~Assessed Land~Assessed Land Use~Assessed Improvements~Assessed Total

                    int a = 0;
                    // Assessment Details Table:
                    //IWebElement propertyreport = driver.FindElement(By.XPath("//*[@id='dnn_ContentPane']/div[1]"));
                    //IList<IWebElement> TRreport = propertyreport.FindElements(By.TagName("a"));
                    //foreach (IWebElement property in TRreport)
                    //{
                    //    if (property.Text != "" && property.Text == "Property Report")
                    //    {
                    //        string URL4 = property.GetAttribute("href");
                    //        gc.downloadfile(URL4, orderNumber, parcelNumber, "report", "VA", "Stafford");
                    //    }
                    //}

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    //string strparcelNumber = property_ID.Replace(" ", "-");
                    // string strParcelBook = strparcelNumber.Substring(0, 3);
                    //string strParcelMap = strparcelNumber.Substring(3, 3);

                    // Tax details Table:
                    driver.Navigate().GoToUrl("https://stafford.virginiainteractive.org/Public/REAccountPayment");
                    Thread.Sleep(4000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Details search", driver, "VA", "Stafford");
                    driver.FindElement(By.XPath("/html/body/div[5]/div[3]/div/button[1]")).SendKeys(Keys.Enter);
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Details search result", driver, "VA", "Stafford");
                    driver.FindElement(By.XPath("//*[@id='MapNumber']")).SendKeys(parcelNumber);
                    gc.CreatePdf_WOP(orderNumber, "Assesement details", driver, "VA", "Stafford");
                    driver.FindElement(By.XPath("//*[@id='main']/form/div/input[1]")).SendKeys(Keys.Enter);
                    Thread.Sleep(4000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax payment history search", driver, "VA", "Stafford");

                    string mailing_add = "", map_no = "", alt_pin = "", legal_Desc = "", total_taxpaid = "", total_penalty = "", total_feespaid = "", total_assessment = "", total_due = "", tax_authority = "";
                    string bulktext = driver.FindElement(By.XPath("/html/body")).Text.Trim();
                    mailing_add = gc.Between(bulktext, "Name / Mailing Address:", "Property Description").Trim();
                    map_no = gc.Between(bulktext, "Map", "Alt. ID/PIN").Trim().Replace("#:", "");
                    alt_pin = gc.Between(bulktext, "Alt. ID/PIN:", "Legal").Trim();
                    legal_Desc = gc.Between(bulktext, "Legal:", "Pay Total Due Today").Trim();
                    total_taxpaid = gc.Between(bulktext, "Total Tax Paid:", "Total Penalty").Trim();
                    total_penalty = gc.Between(bulktext, "Total Penalty/Int Paid:", "Total Fees Paid").Trim();
                    total_feespaid = gc.Between(bulktext, "Total Fees Paid:", "Total Other Assessments").Trim();
                    total_assessment = gc.Between(bulktext, "Total Other Assessments:", "Year Bill").Trim();
                    total_due = gc.Between(bulktext, "Total Due:", "Total Tax Paid").Trim();
                    tax_authority = "1300 Courthouse Road Room 227 Stafford, VA 22554 Phone: (540) 658-8700 Fax: (540) 658-8554";


                    string date = "";
                    DateTime G_Date = DateTime.Now;
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


                    string goodthroughdate = date;
                    string taxdetails = map_no + "~" + alt_pin + "~" + legal_Desc + "~" + mailing_add + "~" + total_taxpaid + "~" + total_penalty + "~" + total_feespaid + "~" + total_assessment + "~" + total_due + "~" + goodthroughdate + "~" + tax_authority;
                    gc.insert_date(orderNumber, property_ID, 667, taxdetails, 1, DateTime.Now);


                    // Tax Payment History Details Table:

                    IWebElement tbmulti = driver.FindElement(By.XPath("//*[@id='frmDetails']/div/div[4]/table/tbody"));
                    IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));

                    IList<IWebElement> TDmulti;
                    foreach (IWebElement row in TRmulti)
                    {

                        TDmulti = row.FindElements(By.TagName("td"));
                        if (TDmulti.Count == 11)
                        {
                            string multi1 = TDmulti[0].Text + "~" + TDmulti[1].Text + "~" + TDmulti[2].Text + "~" + TDmulti[3].Text + "~" + TDmulti[4].Text + "~" + TDmulti[5].Text + "~" + TDmulti[6].Text + "~" + TDmulti[7].Text + "~" + TDmulti[8].Text + "~" + TDmulti[9].Text + "~" + TDmulti[10].Text;
                            gc.insert_date(orderNumber, property_ID, 668, multi1, 1, DateTime.Now);
                        }
                    }

                    try
                    {
                        IWebElement DelinquentMsg = driver.FindElement(By.XPath("//*[@id='main']/fieldset/fieldset[1]/div"));
                        if (DelinquentMsg.Text.Contains("Special Conditions Apply"))
                        {
                            HttpContext.Current.Session["multiparcel_StaffordVA_Delinquent"] = "Yes";
                        }
                    }
                    catch { }


                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "VA", "Stafford", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "VA", "Stafford");
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