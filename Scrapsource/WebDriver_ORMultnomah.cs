//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.IO;
//using System.Drawing.Imaging;
//using OpenQA.Selenium.Chrome;
//using OpenQA.Selenium;
//using System.Threading;
//using OpenQA.Selenium.Support.UI;
//using OpenQA.Selenium.Interactions;
//using MySql.Data.MySqlClient;
//using System.Configuration;
//using System.Drawing;
//using System.Data;
//using HtmlAgilityPack;
//using iTextSharp.text;
//using System.Text.RegularExpressions;
//using OpenQA.Selenium.PhantomJS;
//using OpenQA.Selenium.Support.Extensions;
//using System.Net;

//namespace ScrapMaricopa.Scrapsource
//{
//    public class WebDriver_ORMultnomah
//    {
//        string outputPath = "";
//        IWebDriver driver;
//        DBconnection db = new DBconnection();
//        GlobalClass gc = new GlobalClass();
//        MySqlParameter[] mParam;
//        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
//        public string FTP_ORMultnomah(string address, string parcelNumber, string searchType, string orderNumber, string directParcel)
//        {
//            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "", AssessTakenTime = "", TaxTakentime = "", CityTaxtakentime = "";
//            string TotaltakenTime = "";
//            GlobalClass.global_orderNo = orderNumber;
//            HttpContext.Current.Session["orderNo"] = orderNumber;
//            GlobalClass.global_parcelNo = parcelNumber;
//            var driverService = PhantomJSDriverService.CreateDefaultService();
//            driverService.HideCommandPromptWindow = true;
//            driver = new PhantomJSDriver();

//            string Oname = "-", propID = "-", propAddress = "-", alteraccno = "-", map_tax_lot = "-", levy_code_area = "-", tax_roll_desc = "-", acc_status = "-", prop_use = "-", year_built = "-";

//            try
//            {
//                StartTime = DateTime.Now.ToString("HH:mm:ss");
//                if (searchType == "titleflex")
//                {
//                    gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "OR", "Multnomah");
//                    if (HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes")
//                    {
//                        return "MultiParcel";
//                    }
//                    parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
//                    searchType = "parcel";
//                }
//                if (searchType == "address")
//                {
//                    driver.Navigate().GoToUrl("https://multcoproptax.com/Property-Search?");
//                    driver.FindElement(By.XPath("/html/body/form/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[8]/td/input[2]")).SendKeys(Keys.Enter);
//                    Thread.Sleep(2000);
//                    driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[1]/td/table/tbody/tr[6]/td[2]/input")).SendKeys(address);
//                    gc.CreatePdf_WOP(orderNumber, "InputPassed_AddressSearch",driver, "OR", "Multnomah");
//                    driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[1]/td/table/tbody/tr[6]/td[3]/input[1]")).SendKeys(Keys.Enter);
//                    Thread.Sleep(3000);
//                    gc.CreatePdf_WOP(orderNumber, "ResultGrid",driver, "OR", "Multnomah");
//                    string count = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[6]/td/form/table/tbody/tr/td/table/tbody/tr[1]/td")).Text.Trim();
//                    if (count != "1 Records Found")
//                    {
//                        GlobalClass.multiparcel_ORMulttomah = "Yes";
//                        //Owner_Name~Alternate_Account_Number~Situs_Address~egal_Description
//                        IWebElement multitableElement = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td/table"));
//                        IList<IWebElement> multitableRow = multitableElement.FindElements(By.TagName("tr"));
//                        IList<IWebElement> multirowTD;
//                        int maxCheck = 1;
//                        foreach (IWebElement row in multitableRow)
//                        {
//                            if (maxCheck <= 25)
//                            {
//                                multirowTD = row.FindElements(By.TagName("td"));
//                                if (multirowTD.Count != 0 && !row.Text.Contains("Search Results") && !row.Text.Contains("Property ID"))
//                                {
//                                    string multi = multirowTD[1].Text.Trim() + "~" + multirowTD[2].Text.Trim() + "~" + multirowTD[3].Text.Trim() + "~" + multirowTD[4].Text.Trim();
//                                    if (multi.Contains("\r\n"))
//                                    {
//                                        multi = multi.Replace("\r\n", "");
//                                        gc.insert_date(orderNumber, multirowTD[0].Text.Trim(), 19, multi, 1, DateTime.Now);
//                                        //db.ExecuteQuery("insert into data_value_master (Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table) values ('" + orderNumber + "','" + multirowTD[0].Text.Trim() + "',19 ,'" + multi + "',1)");

//                                    }
//                                }
//                                maxCheck++;
//                            }
//                        }
//                        if (multitableRow.Count > 25)
//                        {
//                            HttpContext.Current.Session["multiParcel_ORMulttomah_Multicount"] = "Maximum";
//                        }
//                        else
//                        {
//                            HttpContext.Current.Session["multiparcel_ORMulttomah"] = "Yes";
//                        }
//                        driver.Quit();
//                        return "MultiParcel";
//                    }
//                }
//                else if (searchType == "parcel")
//                {
//                    driver.Navigate().GoToUrl("https://multcoproptax.com/Property-Search?");
//                    driver.FindElement(By.XPath("/html/body/form/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[8]/td/input[2]")).SendKeys(Keys.Enter);
//                    Thread.Sleep(2000);
//                    var category = driver.FindElement(By.Name("selecttype"));
//                    var selectElement = new SelectElement(category);
//                    selectElement.SelectByValue("6");
//                    driver.FindElement(By.Name("search")).SendKeys(parcelNumber);
//                    gc.CreatePdf(orderNumber, parcelNumber, "InputPassed_ParcelSearch",driver, "OR", "Multnomah");
//                    driver.FindElement(By.Name("Submit")).SendKeys(Keys.Enter);
//                    Thread.Sleep(3000);
//                    gc.CreatePdf(orderNumber, parcelNumber, "ResultGrid",driver, "OR", "Multnomah");
//                }
//                //property_details
//                //Alternate_parcelNumber~Map_Tax_Lot~Property_Address~Owner_Name~Legal_Description~Levy_Code_Area~Property_Use~Year_Built~Account_Status
//                driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td/table/tbody/tr[3]/td[1]/a")).SendKeys(Keys.Enter);
//                Thread.Sleep(3000);
//                Oname = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr/td/table/tbody/tr[2]/td[2]/table/tbody/tr[1]/td/table/tbody/tr[3]/td[1]")).Text.Trim();
//                propID = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr/td/table/tbody/tr[2]/td[2]/table/tbody/tr[1]/td/table/tbody/tr[3]/td[2]")).Text.Trim();
//                propAddress = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr/td/table/tbody/tr[2]/td[2]/table/tbody/tr[1]/td/table/tbody/tr[5]/td[2]")).Text.Trim().Replace("\r\n", "");
//                alteraccno = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr/td/table/tbody/tr[2]/td[2]/table/tbody/tr[1]/td/table/tbody/tr[7]/td[1]")).Text.Trim();
//                map_tax_lot = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr/td/table/tbody/tr[2]/td[2]/table/tbody/tr[1]/td/table/tbody/tr[9]/td[1]")).Text.Trim();
//                levy_code_area = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr/td/table/tbody/tr[2]/td[2]/table/tbody/tr[1]/td/table/tbody/tr[9]/td[2]")).Text.Trim();
//                tax_roll_desc = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr/td/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td/table/tbody/tr[4]/td[1]")).Text.Trim();
//                acc_status = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr/td/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td/table/tbody/tr[6]/td[2]")).Text.Trim();
//                prop_use = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr/td/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td/table/tbody/tr[8]/td[1]")).Text.Trim();
//                year_built = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr/td/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td/table/tbody/tr[8]/td[2]")).Text.Trim();
//                gc.CreatePdf(orderNumber, propID, "PropertDetails",driver, "OR", "Multnomah");
//                string property_details = alteraccno + "~" + map_tax_lot + "~" + propAddress + "~" + Oname + "~" + tax_roll_desc + "~" + levy_code_area + "~" + prop_use + "~" + year_built + "~" + acc_status;
//                gc.insert_date(orderNumber, propID, 14, property_details, 1, DateTime.Now);
//                //db.ExecuteQuery("insert into data_value_master (Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table) values ('" + orderNumber + "','" + propID + "',14 ,'" + property_details + "',1)");

//                //assessment details 
//                //Assessed_Year~Land_Value~Building_Value~Market_Value~Exemptions~SpecialMkt_Use~Total_Assessed_Value
//                driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr/td/table/tbody/tr/td[3]/a")).SendKeys(Keys.Enter);
//                Thread.Sleep(2000);
//                gc.CreatePdf(orderNumber, propID, "Assessment_Details",driver, "OR", "Multnomah");
//                string Assessed_Year = "-", Land_Value = "-", Building_Value = "-", Market_Value = "-", Exemptions = "-", SpecialMkt_Use = "-", Total_Assessed_Value = "-";
//                Assessed_Year = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td/table/tbody/tr[3]/td[1]")).Text.Trim();
//                Land_Value = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td/table/tbody/tr[3]/td[3]")).Text.Trim();
//                Building_Value = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td/table/tbody/tr[3]/td[2]")).Text.Trim();
//                Market_Value = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td/table/tbody/tr[3]/td[5]")).Text.Trim();
//                Exemptions = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td/table/tbody/tr[3]/td[6]")).Text.Trim();
//                if (Exemptions == "")
//                {
//                    Exemptions = "-";
//                }
//                SpecialMkt_Use = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td/table/tbody/tr[3]/td[4]")).Text.Trim();
//                Total_Assessed_Value = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td/table/tbody/tr[3]/td[7]")).Text.Trim();
//                string assessment_details = Assessed_Year + "~" + Land_Value + "~" + Building_Value + "~" + Market_Value + "~" + Exemptions + "~" + SpecialMkt_Use + "~" + Total_Assessed_Value;
//                gc.insert_date(orderNumber, propID, 15, assessment_details, 1, DateTime.Now);
//                //db.ExecuteQuery("insert into data_value_master (Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table) values ('" + orderNumber + "','" + propID + "',15 ,'" + assessment_details + "',1)");

//                AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
//                //Tax Details
//                string curr_year_tax_owed = "-", total_tax_PO_amout = "-", gtd = "-";
//                //Tax Summary Tab 
//                //Year~Total_Tax~Ad_Valorem~Special_Assessments~Principal~Interest_Due~Paid_Date~Total_Owed
//                driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/table/tbody/tr/td/table/tbody/tr/td[2]/a")).SendKeys(Keys.Enter);
//                Thread.Sleep(2000);
//                gc.CreatePdf(orderNumber, propID, "Tax_Summary",driver, "OR", "Multnomah");

//                // IWebElement url = driver.FindElement(By.LinkText("Click to View Tax Bill"));
//                gc.CreatePdf(orderNumber, propID, "Assessment_Details",driver, "OR", "Multnomah");
//                string URL = "http://vance.co.multnomah.or.us/cgi-bin/tax-page?" + propID;
//                gc.downloadfile(URL, orderNumber, propID, "Tax Bill", "OR", "Multnomah");
//                //string billpdf = outputPath + "Tax_Bill.pdf";
//                //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
//                //WebClient downloadpdf = new WebClient();
//                //downloadpdf.DownloadFile(URL, billpdf);

//                IWebElement taxsumtableElement = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[7]/td/table"));
//                IList<IWebElement> taxsumtableRow = taxsumtableElement.FindElements(By.TagName("tr"));
//                IList<IWebElement> taxsumrowTD;
//                foreach (IWebElement row in taxsumtableRow)
//                {
//                    taxsumrowTD = row.FindElements(By.TagName("td"));
//                    if (taxsumrowTD.Count != 0 && !row.Text.Contains("Tax Summary") && !row.Text.Contains("Year"))
//                    {
//                        int i = 0;
//                        if (i == 0)
//                        {
//                            string due = taxsumrowTD[5].Text.Trim();
//                            if (due != "0.00")
//                            {
//                                string currDate = DateTime.Now.ToString("MM/dd/yyyy");
//                                string dateChecking = DateTime.Now.ToString("MM") + "/15/" + DateTime.Now.ToString("yyyy");

//                                if (Convert.ToDateTime(currDate) > Convert.ToDateTime(dateChecking))
//                                {
//                                    string nextEndOfMonth = "";
//                                    if ((Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))) < 12))
//                                    {
//                                        gtd = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM")) + 1), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")) + 1)).ToString("MM/dd/yyyy");
//                                        driver.FindElement(By.XPath("//*[@id='taxdate']")).Clear();
//                                        driver.FindElement(By.XPath("//*[@id='taxdate']")).SendKeys(gtd);
//                                        driver.FindElement(By.XPath("//*[@id='dateform']/input[2]")).SendKeys(Keys.Enter);

//                                    }
//                                    else
//                                    {
//                                        int nxtYr = Convert.ToInt16(DateTime.Now.ToString("yyyy")) + 1;
//                                        gtd = new DateTime(nxtYr, 1, DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), 1)).ToString("MM/dd/yyyy");
//                                        driver.FindElement(By.XPath("//*[@id='taxdate']")).Clear();
//                                        driver.FindElement(By.XPath("//*[@id='taxdate']")).SendKeys(gtd);
//                                        driver.FindElement(By.XPath("//*[@id='dateform']/input[2]")).SendKeys(Keys.Enter);

//                                    }
//                                }


//                            }

//                            //string taxsum = taxsumrowTD[0].Text.Trim() + "~" + taxsumrowTD[1].Text.Trim() + "~" + taxsumrowTD[2].Text.Trim() + "~" + taxsumrowTD[3].Text.Trim() + "~" + taxsumrowTD[4].Text.Trim() + "~" + taxsumrowTD[5].Text.Trim() + "~" + taxsumrowTD[6].Text.Trim() + "~" + taxsumrowTD[7].Text.Trim();
//                            //if (taxsum.Contains("\r\n"))
//                            //{
//                            //    taxsum = taxsum.Replace("\r\n", ",");
//                            //}
//                            //gc.insert_date(orderNumber,DateTime.Now , propID, 17, taxsum, 1);

//                        }
//                        break;
//                    }
//                }

//                IWebElement taxsumtableElement1 = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[7]/td/table"));
//                IList<IWebElement> taxsumtableRow1 = taxsumtableElement1.FindElements(By.TagName("tr"));
//                IList<IWebElement> taxsumrowTD1;
//                foreach (IWebElement row1 in taxsumtableRow1)
//                {
//                    taxsumrowTD1 = row1.FindElements(By.TagName("td"));
//                    if (taxsumrowTD1.Count != 0 && !row1.Text.Contains("Tax Summary") && !row1.Text.Contains("Year"))
//                    {
//                        string taxsum = taxsumrowTD1[0].Text.Trim() + "~" + taxsumrowTD1[1].Text.Trim() + "~" + taxsumrowTD1[2].Text.Trim() + "~" + taxsumrowTD1[3].Text.Trim() + "~" + taxsumrowTD1[4].Text.Trim() + "~" + taxsumrowTD1[5].Text.Trim() + "~" + taxsumrowTD1[6].Text.Trim() + "~" + taxsumrowTD1[7].Text.Trim();
//                        if (taxsum.Contains("\r\n"))
//                        {
//                            taxsum = taxsum.Replace("\r\n", ",");
//                        }

//                        gc.insert_date(orderNumber, propID, 17, taxsum, 1, DateTime.Now);
//                    }
//                }



//                //Installment~Total_Tax~Amount_Paid~Taxes_Paid~Interest_Paid~Discount_Paid~Paid_Date~Current_Year_Tax_Owed~Total_Tax_Payoff_Amount~Good_through_date

//                driver.FindElement(By.LinkText("Current Property Tax")).SendKeys(Keys.Enter);
//                Thread.Sleep(2000);
//                gc.CreatePdf(orderNumber, parcelNumber, "Current_Property_Tax",driver, "OR", "Multnomah");
//                curr_year_tax_owed = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[4]/td/table/tbody/tr[2]/td[1]")).Text.Trim();
//                total_tax_PO_amout = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[4]/td/table/tbody/tr[2]/td[2]")).Text.Trim();

//                IWebElement insttableElement = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr[2]/td[2]/table/tbody/tr[5]/td/table"));
//                IList<IWebElement> insttableRow = insttableElement.FindElements(By.TagName("tr"));
//                IList<IWebElement> instrowTD;
//                foreach (IWebElement row in insttableRow)
//                {
//                    instrowTD = row.FindElements(By.TagName("td"));
//                    if (instrowTD.Count != 0 && !row.Text.Contains("Current Property Tax") && !row.Text.Contains("Third"))
//                    {
//                        string inst = instrowTD[0].Text.Trim() + "~" + instrowTD[1].Text.Trim() + "~" + instrowTD[2].Text.Trim() + "~" + instrowTD[3].Text.Trim() + "~" + instrowTD[4].Text.Trim() + "~" + instrowTD[5].Text.Trim() + "~" + instrowTD[6].Text.Trim();
//                        inst = inst + "~" + curr_year_tax_owed + "~" + total_tax_PO_amout + "~" + gtd;
//                        gc.insert_date(orderNumber, propID, 16, inst, 1, DateTime.Now);
//                        //db.ExecuteQuery("insert into data_value_master (Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table) values ('" + orderNumber + "','" + propID + "',16 ,'" + inst + "',1)");
//                    }
//                }


//                //Property Tax History Summary 
//                //Tax_Year~Taxes_Levied~Total_Paid~Taxes_Paid~Interes_Paid~Date_Paid~Total_Owed
//                driver.FindElement(By.LinkText("Property Tax History Summary")).SendKeys(Keys.Enter);
//                Thread.Sleep(2000);
//                gc.CreatePdf(orderNumber, parcelNumber, "Property_Tax_History_Summary",driver, "OR", "Multnomah");
//                IWebElement proptaxtableElement = driver.FindElement(By.XPath("/html/body/table/tbody/tr[3]/td/table/tbody/tr[8]/td[2]/table"));
//                IList<IWebElement> proptaxtableRow = proptaxtableElement.FindElements(By.TagName("tr"));
//                IList<IWebElement> proptaxrowTD;
//                foreach (IWebElement row in proptaxtableRow)
//                {
//                    proptaxrowTD = row.FindElements(By.TagName("td"));

//                    if (proptaxrowTD.Count != 0 && !row.Text.Contains("Property Tax History Summary") && !row.Text.Contains("Tax Year"))
//                    {
//                        string taxsum = proptaxrowTD[0].Text.Trim() + "~" + proptaxrowTD[1].Text.Trim() + "~" + proptaxrowTD[2].Text.Trim() + "~" + proptaxrowTD[3].Text.Trim() + "~" + proptaxrowTD[4].Text.Trim() + "~" + proptaxrowTD[5].Text.Trim() + "~" + proptaxrowTD[6].Text.Trim();
//                        gc.insert_date(orderNumber, propID, 18, taxsum, 1, DateTime.Now);
//                        // db.ExecuteQuery("insert into data_value_master (Order_no,parcel_no,Data_Field_Text_Id,Data_Field_value,Is_Table) values ('" + orderNumber + "','" + propID + "',18 ,'" + taxsum + "',1)");
//                    }
//                }
//                TaxTime = DateTime.Now.ToString("HH:mm:ss");
//                LastEndTime = DateTime.Now.ToString("HH:mm:ss");
//                gc.insert_TakenTime(orderNumber, "OR", "Multnomah", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

//                driver.Quit();
//                gc.mergpdf(orderNumber, "OR", "Multnomah");
//                return "Data Inserted Successfully";
//            }
//            catch (Exception ex)
//            {
//                driver.Quit();
//                GlobalClass.LogError(ex, orderNumber);
//                throw ex;
//            }
//        }



//    }
//}



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
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_ORMultnomah
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();

        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_ORMultnomah(string address, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            GlobalClass.sname = "OR";
            GlobalClass.cname = "Multnomah";
            string TaxingAuthority = "";
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string Good_through_date = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                //   driver = new ChromeDriver();

                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("https://multcoproptax.com/Property-Search");
                    Thread.Sleep(4000);


                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, "", ownername, address, "OR", "Multnomah");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_ORMultnomah"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";

                    }
                    if (searchType == "address")
                    {
                        driver.FindElement(By.Id("dnn_ctr410_MultnomahGuestView_SearchTextBox")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "OR", "Multnomah");
                        driver.FindElement(By.XPath("//*[@id='SearchButtonDiv']/i")).Click();
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "OR", "Multnomah");
                        try
                        {
                            string strparcel = "";
                            int s = 0;
                            int Max = 0;
                            IWebElement add_search = driver.FindElement(By.XPath("//*[@id='grid']"));
                            IList<IWebElement> TRadd_search = add_search.FindElements(By.TagName("tr"));
                            IList<IWebElement> THadd_search = add_search.FindElements(By.TagName("th"));
                            IList<IWebElement> TDadd_search;
                            foreach (IWebElement row in TRadd_search)
                            {
                                TDadd_search = row.FindElements(By.TagName("td"));
                                if (TDadd_search.Count != 0 && !row.Text.Contains("Location Address"))
                                {
                                    parcelNumber = TDadd_search[0].Text;
                                    string Address = TDadd_search[4].Text;
                                    string ownername1 = TDadd_search[3].Text;
                                    string AddressDetails = ownername1 + "~" + Address;

                                    gc.insert_date(orderNumber, parcelNumber, 19, AddressDetails, 1, DateTime.Now);
                                    s++;
                                    strparcel = parcelNumber;
                                    Max++;

                                }

                            }
                            if (s == 1)
                            {
                                driver.FindElement(By.XPath("//*[@id='grid']/div[2]/table/tbody/tr/td[1]")).Click();
                                Thread.Sleep(5000);

                            }
                            if (s > 1 && s < 25)
                            {
                                gc.CreatePdf_WOP(orderNumber, "Multi Address search result", driver, "OR", "Multnomah");
                                HttpContext.Current.Session["multiparcel_ORMulttomah"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (s > 25)
                            {
                                HttpContext.Current.Session["multiParcel_MOJackson_Multicount"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                            if (Max == 0 && add_search.Text.Contains("No properties found"))
                            {
                                HttpContext.Current.Session["Nodata_ORMultnomah"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }


                        }
                        catch { }
                        try
                        {
                            IWebElement INodata = driver.FindElement(By.XPath("//*[@id='grid']"));
                            if (INodata.Text.Contains("No properties found"))
                            {
                                HttpContext.Current.Session["Nodata_ORMultnomah"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }


                    }
                    //else if (searchType == "ownername")
                    //{
                    //    if (!ownername.Contains('*'))
                    //    {
                    //        ownername = ownername + "*";
                    //    }

                    //    driver.FindElement(By.Id("mSearchControl_mName")).SendKeys(ownername);
                    //    gc.CreatePdf(orderNumber, parcelNumber, "ownername search", driver, "OR", "Multnomah");
                    //    driver.FindElement(By.Id("mSearchControl_mSubmit")).SendKeys(Keys.Enter);
                    //    Thread.Sleep(3000);
                    //    gc.CreatePdf(orderNumber, parcelNumber, "ownername search Result", driver, "OR", "Multnomah");
                    //    try
                    //    {
                    //        string strparcel = "";
                    //        int Max = 0;
                    //        IWebElement add_search = driver.FindElement(By.XPath("//*[@id='mResultscontrol_mGrid_RealDataGrid']/tbody"));
                    //        IList<IWebElement> TRadd_search = add_search.FindElements(By.TagName("tr"));
                    //        IList<IWebElement> THadd_search = add_search.FindElements(By.TagName("th"));
                    //        IList<IWebElement> TDadd_search;
                    //        foreach (IWebElement row in TRadd_search)
                    //        {
                    //            TDadd_search = row.FindElements(By.TagName("td"));
                    //            if (TRadd_search.Count > 3 && TDadd_search.Count != 0 && !row.Text.Contains("Location Address") && !row.Text.Contains("No Values Found"))
                    //            {
                    //                string parcel_id = TDadd_search[0].Text;
                    //                if (parcel_id.Trim().Replace("-", "").Count() == 17 && parcel_id.Trim().Replace("-", "").Count() != 9 && !row.Text.Contains("No Values Found"))
                    //                {

                    //                    string Address = TDadd_search[2].Text;
                    //                    string ownername1 = TDadd_search[1].Text;
                    //                    string AddressDetails = ownername1 + "~" + Address;

                    //                    gc.insert_date(orderNumber, parcel_id, 1420, AddressDetails, 1, DateTime.Now);
                    //                    strparcel = parcel_id;
                    //                    Max++;
                    //                }
                    //            }
                    //            if (TRadd_search.Count == 3)
                    //            {
                    //                driver.FindElement(By.XPath("//*[@id='mResultscontrol_mGrid_RealDataGrid']/tbody/tr[3]/td[1]/a")).SendKeys(Keys.Enter);
                    //                Thread.Sleep(5000);

                    //            }
                    //        }
                    //        if (TRadd_search.Count < 27 && TRadd_search.Count > 3)
                    //        {
                    //            gc.CreatePdf_WOP(orderNumber, "Multi Address search result", driver, "OR", "Multnomah");
                    //            HttpContext.Current.Session["multiparcel_ORMulttomah"] = "Yes";
                    //            driver.Quit();
                    //            return "MultiParcel";
                    //        }
                    //        if (TRadd_search.Count >= 27 && TRadd_search.Count > 3)
                    //        {
                    //            HttpContext.Current.Session["multiParcel_MOJackson_Multicount"] = "Maximum";
                    //            driver.Quit();
                    //            return "Maximum";
                    //        }
                    //        if (Max == 0)
                    //        {
                    //            HttpContext.Current.Session["Zero_Multnomah"] = "Zero";
                    //            driver.Quit();
                    //            return "Zero";
                    //        }

                    //    }
                    //    catch { }

                    //}

                    else if (searchType == "parcel")
                    {
                        driver.FindElement(By.Id("dnn_ctr410_MultnomahGuestView_SearchTextBox")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "parcel search", driver, "OR", "Multnomah");
                        driver.FindElement(By.Id("SearchButtonDiv")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "parcel search Result", driver, "OR", "Multnomah");
                        driver.FindElement(By.XPath("//*[@id='grid']/div[2]/table/tbody/tr/td[1]")).Click();
                        Thread.Sleep(2000);
                        try
                        {
                            string stradd_search = "";
                            int s = 0;
                            int Max = 0;
                            IWebElement Iadd_search = driver.FindElement(By.XPath("//*[@id='grid']/div[2]/table/tbody"));
                            stradd_search = Iadd_search.Text;


                            if (stradd_search.Contains("No properties found"))
                            {
                                HttpContext.Current.Session["Nodata_ORMultnomah"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }


                        }
                        catch { }
                    }

                    // Property Details
                    string OwnerName = "", Propertystatus = "", PropertyType = "", LegalDesc = "", AlternateAcNo = "", Neighborhood = "";
                    string MapNumber = "", PropertyUse = "", LevyCode = "", ProprtyAddress = "", YearBuilt = "";
                    string bulkdata = driver.FindElement(By.XPath("//*[@id='tblGeneralInformation']/tbody")).Text;
                    parcelNumber = driver.FindElement(By.Id("dnn_ctr380_View_tdPropertyID")).Text;
                    OwnerName = driver.FindElement(By.Id("dnn_ctr380_View_divOwnersLabel")).Text;
                    ProprtyAddress = driver.FindElement(By.Id("dnn_ctr380_View_tdPropertyAddress")).Text;
                    Propertystatus = gc.Between(bulkdata, "Property Status", "Property Type").Trim();
                    PropertyType = gc.Between(bulkdata, "Property Type", "Legal Description").Trim();
                    LegalDesc = gc.Between(bulkdata, "Legal Description", "Alternate Account Number").Trim();
                    AlternateAcNo = gc.Between(bulkdata, "Alternate Account Number", "Neighborhood").Trim();
                    Neighborhood = gc.Between(bulkdata, "Neighborhood", "Map Number").Trim();
                    MapNumber = gc.Between(bulkdata, "Map Number", "Property Use").Trim();
                    PropertyUse = gc.Between(bulkdata, "Property Use", "Levy Code Area").Trim();
                    LevyCode = GlobalClass.After(bulkdata, "Levy Code Area").Trim();


                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='dnn_ctr380_View_divCamaInfo']/ul/li/table/tbody/tr[1]/td[1]/i")).Click();
                        Thread.Sleep(2000);
                    }
                    catch { }
                    try
                    {
                        YearBuilt = driver.FindElement(By.XPath("//*[@id='dnn_ctr380_View_divCamaInfo']/ul/li/div/table/tbody/tr[2]/td[3]")).Text;
                    }
                    catch { }

                    string Property = OwnerName + "~" + ProprtyAddress + "~" + Propertystatus + "~" + PropertyType + "~" + LegalDesc + "~" + AlternateAcNo + "~" + Neighborhood + "~" + MapNumber + "~" + PropertyUse + "~" + LevyCode + "~" + YearBuilt;
                    gc.insert_date(orderNumber, parcelNumber, 14, Property, 1, DateTime.Now);
                    gc.CreatePdf(orderNumber, parcelNumber, "Property Details", driver, "OR", "Multnomah");


                    // Assessment Details 

                    IWebElement Assessmentdetails = driver.FindElement(By.XPath("//*[@id='dnn_ctr380_View_tblValueHistoryDataRP']/tbody"));
                    IList<IWebElement> TRAssessmentdetails = Assessmentdetails.FindElements(By.TagName("tr"));
                    IList<IWebElement> THAssessmentdetails = Assessmentdetails.FindElements(By.TagName("th"));
                    IList<IWebElement> TDAssessmentdetails;
                    foreach (IWebElement row1 in TRAssessmentdetails)
                    {
                        TDAssessmentdetails = row1.FindElements(By.TagName("td"));
                        if (TDAssessmentdetails.Count != 0 && !row1.Text.Contains("IMPROVEMENTS") && row1.Text.Trim() != "" && TDAssessmentdetails.Count == 8)
                        {
                            string Assessment = TDAssessmentdetails[0].Text + "~" + TDAssessmentdetails[1].Text + "~" + TDAssessmentdetails[2].Text + "~" + TDAssessmentdetails[3].Text + "~" + TDAssessmentdetails[4].Text + "~" + TDAssessmentdetails[5].Text + "~" + TDAssessmentdetails[6].Text + "~" + TDAssessmentdetails[7].Text;
                            gc.insert_date(orderNumber, parcelNumber, 15, Assessment, 1, DateTime.Now);
                        }

                    }

                    // Current Tax Details

                    driver.FindElement(By.Id("tabBills")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Current Tax Details", driver, "OR", "Multnomah");
                    string taxyear = "";
                    taxyear = driver.FindElement(By.Id("dnn_ctr380_View_thTotalAssessedValueTitle")).Text.Replace("Assessed Value", "").Trim();
                    string taxdata = driver.FindElement(By.XPath("//*[@id='dnn_ctr380_View_divPaymentModal']/table/tbody/tr[2]/td/table/tbody")).Text;
                    string CurrentYearDue = "", PastYearDue = "", TotalDue = "";
                    CurrentYearDue = gc.Between(taxdata, "Current Year Due", "Past Years Due");
                    PastYearDue = gc.Between(taxdata, "Past Years Due", "Total Due");
                    TotalDue = GlobalClass.After(taxdata, "Total Due");

                    string CurrentTax = taxyear + "~" + CurrentYearDue + "~" + PastYearDue + "~" + TotalDue;
                    gc.insert_date(orderNumber, parcelNumber, 16, CurrentTax, 1, DateTime.Now);

                    // Tax Payment

                    driver.FindElement(By.XPath("//*[@id='dnn_ctr380_View_divBillDetails']/div/table[1]/tbody/tr/td[2]/i")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Payment", driver, "OR", "Multnomah");
                    string Tax = "";
                    try
                    {
                        IWebElement Taxpay = driver.FindElement(By.XPath("//*[@id='dnn_ctr380_View_divBillDetails']/div/table[2]/tbody"));
                        IList<IWebElement> TRTaxpay = Taxpay.FindElements(By.TagName("tr"));
                        IList<IWebElement> THTaxpay = Taxpay.FindElements(By.TagName("th"));
                        IList<IWebElement> TDTaxpay;
                        foreach (IWebElement row1 in TRTaxpay)
                        {
                            TDTaxpay = row1.FindElements(By.TagName("td"));
                            if (TDTaxpay.Count != 0 && !row1.Text.Contains("TOTAL BILLED") && row1.Text.Trim() != "" && !TDTaxpay[5].Text.Contains("$0.00") && TDTaxpay.Count == 8)
                            {
                                Tax = "Delinquent";
                                break;
                            }
                            try
                            {
                                if (TDTaxpay.Count != 0 && !row1.Text.Contains("TOTAL BILLED") && row1.Text.Trim() != "" && !TDTaxpay[9].Text.Contains("$0.00") && TDTaxpay.Count == 12)
                                {
                                    Tax = "Delinquent";
                                    break;
                                }
                            }
                            catch { }

                        }
                    }
                    catch { }

                    if (Tax == "Delinquent")
                    {
                        try
                        {
                            //Good Through Details

                            IWebElement good_date = driver.FindElement(By.Id("effectiveDatePicker"));
                            Good_through_date = good_date.GetAttribute("value");
                            if (Good_through_date.Contains("Select A Date"))
                            {
                                Good_through_date = "-";
                            }
                            else
                            {

                                DateTime G_Date = Convert.ToDateTime(Good_through_date);
                                string dateChecking = DateTime.Now.ToString("MM") + "/15/" + DateTime.Now.ToString("yyyy");

                                if (G_Date < Convert.ToDateTime(dateChecking))
                                {
                                    //end of the month
                                    Good_through_date = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")))).ToString("MM/dd/yyyy");

                                }
                                else if (G_Date > Convert.ToDateTime(dateChecking))
                                {
                                    // nextEndOfMonth 
                                    if ((Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))) < 12))
                                    {
                                        Good_through_date = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM")) + 1), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")) + 1)).ToString("MM/dd/yyyy");
                                    }
                                    else
                                    {
                                        int nxtYr = Convert.ToInt16(DateTime.Now.ToString("yyyy")) + 1;
                                        Good_through_date = new DateTime(nxtYr, 1, DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), 1)).ToString("MM/dd/yyyy");

                                    }
                                }
                                driver.FindElement(By.Id("effectiveDatePicker")).Clear();
                                Thread.Sleep(1000);

                                string[] daysplit = Good_through_date.Split('/');
                                try
                                {
                                    driver.FindElement(By.XPath("//*[@id='effectiveDatePicker']")).SendKeys(Good_through_date);
                                    Thread.Sleep(2000);
                                }
                                catch { }

                                IWebElement Iday = driver.FindElement(By.XPath("//*[@id='ui-datepicker-div']/table/tbody"));
                                IList<IWebElement> IdayRow = Iday.FindElements(By.TagName("a"));
                                foreach (IWebElement day in IdayRow)
                                {
                                    if (day.Text != "" && day.Text == daysplit[1])
                                    {
                                        day.SendKeys(Keys.Enter);
                                        Thread.Sleep(2000);
                                        gc.CreatePdf(orderNumber, parcelNumber, "Good Through Date", driver, "OR", "Multnomah");
                                        driver.FindElement(By.XPath("//*[@id='dnn_ctr380_View_divBillDetails']/div/table[1]/tbody/tr/td[2]")).Click();
                                        Thread.Sleep(4000);

                                    }
                                }

                            }
                        }
                        catch
                        { }

                    }

                    IWebElement Taxpay1 = driver.FindElement(By.XPath("//*[@id='dnn_ctr380_View_divBillDetails']/div/table[2]/tbody"));
                    IList<IWebElement> TRTaxpay1 = Taxpay1.FindElements(By.TagName("tr"));
                    IList<IWebElement> THTaxpay1 = Taxpay1.FindElements(By.TagName("th"));
                    IList<IWebElement> TDTaxpay1;
                    foreach (IWebElement row2 in TRTaxpay1)
                    {
                        TDTaxpay1 = row2.FindElements(By.TagName("td"));
                        if (TDTaxpay1.Count != 0 && !row2.Text.Contains("TOTAL BILLED") && !row2.Text.Contains("Installment") && row2.Text.Trim() != "" && TDTaxpay1.Count == 12)
                        {
                            string Taxpayment = TDTaxpay1[0].Text + "~" + TDTaxpay1[1].Text + "~" + TDTaxpay1[2].Text + "~" + TDTaxpay1[3].Text + "~" + TDTaxpay1[8].Text + "~" + TDTaxpay1[9].Text + "~" + TDTaxpay1[10].Text + "~" + TDTaxpay1[11].Text + "~" + Good_through_date;
                            gc.insert_date(orderNumber, parcelNumber, 18, Taxpayment, 1, DateTime.Now);
                        }
                        if (TDTaxpay1.Count != 0 && !row2.Text.Contains("TOTAL BILLED") && !row2.Text.Contains("Installment") && row2.Text.Trim() != "" && TDTaxpay1.Count == 8)
                        {
                            string Taxpayment = TDTaxpay1[0].Text + "~" + TDTaxpay1[1].Text + "~" + TDTaxpay1[2].Text + "~" + TDTaxpay1[3].Text + "~" + TDTaxpay1[4].Text + "~" + TDTaxpay1[5].Text + "~" + TDTaxpay1[6].Text + "~" + TDTaxpay1[7].Text + "~" + Good_through_date;
                            gc.insert_date(orderNumber, parcelNumber, 18, Taxpayment, 1, DateTime.Now);
                        }
                        if (TDTaxpay1.Count != 0 && !row2.Text.Contains("TOTAL BILLED") && row2.Text.Contains("Installment") && row2.Text.Trim() != "" && TDTaxpay1.Count == 8)
                        {
                            string Taxpayment1 = TDTaxpay1[0].Text + "~" + TDTaxpay1[1].Text + "~" + TDTaxpay1[2].Text + "~" + TDTaxpay1[3].Text + "~" + TDTaxpay1[4].Text + "~" + TDTaxpay1[5].Text + "~" + TDTaxpay1[6].Text + "~" + TDTaxpay1[7].Text + "~" + Good_through_date;
                            gc.insert_date(orderNumber, parcelNumber, 18, Taxpayment1, 1, DateTime.Now);
                        }

                    }



                    // Tax Distribution Details

                    string tableassess = "";
                    try
                    {
                        var chromeOptions = new ChromeOptions();
                        var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];
                        chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                        chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                        chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                        chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                        Array.ForEach(Directory.GetFiles(@downloadDirectory), File.Delete); //Delete all the files in auto pdf
                        var driver1 = new ChromeDriver(chromeOptions);
                        driver1.Navigate().GoToUrl(driver.Url);
                        string fileName = "";
                        driver1.FindElement(By.Id("tabBills")).Click();
                        Thread.Sleep(2000);

                        IWebElement Receipttable = driver1.FindElement(By.XPath("//*[@id='dnn_ctr380_View_divBillDetails']/div/table[2]/tbody/tr[2]/td[1]/a"));
                        // string BillTax2 = Receipttable.GetAttribute("href");
                        // fileName = gc.Between(BillTax2, "taxreceipt/", "?id=").Replace("-", "_");
                        Receipttable.Click();
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Distribution Details", driver, "OR", "Multnomah");
                        // Array.ForEach(Directory.GetFiles(@downloadDirectory), File.Delete); //Delete all the files in auto pdf
                        var files = new DirectoryInfo(downloadDirectory).GetFiles("*.*");
                        string latestfile = "";
                        DateTime lastupdated = DateTime.MinValue;
                        foreach (FileInfo file in files)
                        {
                            if (file.LastWriteTime > lastupdated)
                            {
                                lastupdated = file.LastWriteTime;
                                fileName = file.Name;
                            }

                        }

                        gc.AutoDownloadFile(orderNumber, parcelNumber, "Multnomah", "OR", fileName);
                        string FilePath = gc.filePath(orderNumber, parcelNumber) + fileName;
                        PdfReader reader;
                        string pdfData;
                        reader = new PdfReader(FilePath);
                        String textFromPage = PdfTextExtractor.GetTextFromPage(reader, 1);
                        System.Diagnostics.Debug.WriteLine("" + textFromPage);
                        string pdftext = textFromPage;
                        try
                        {
                            tableassess = gc.Between(pdftext, "CURRENT TAX BY DISTRICT", "PROPERTY TAXES").Trim();
                            string[] tableArray = tableassess.Split('\n');
                            for (int i = 0; i < 1; i++)
                            {
                                int count1 = tableArray.Length;
                                string a1 = tableArray[i].Replace(" ", "~");
                                string[] rowarray = a1.Split('~');
                                int tdcount = rowarray.Length;
                                if (tdcount < 7)
                                {
                                    string datepdf = rowarray[0] + "~" + rowarray[2] + "" + rowarray[3] + " " + rowarray[4] + "~" + " " + "~" + rowarray[5];
                                    gc.insert_date(orderNumber, parcelNumber, 889, datepdf, 1, DateTime.Now);
                                }
                                if (tdcount > 6)
                                {
                                    string datepdf = rowarray[0] + "~" + rowarray[2] + "" + rowarray[3] + " " + rowarray[4] + "~" + rowarray[5] + " " + rowarray[6] + " " + rowarray[7] + "~" + rowarray[8];
                                    gc.insert_date(orderNumber, parcelNumber, 889, datepdf, 1, DateTime.Now);
                                }

                            }
                        }
                        catch { }
                        driver1.Quit();
                    }
                    catch (Exception ex) { driver.Quit(); }

                    // Tax PAyment History
                    try
                    {
                        IWebElement StrClick;
                        driver.FindElement(By.LinkText("Payment History")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Payment History Details", driver, "OR", "Multnomah");
                    }
                    catch { }
                    try
                    {
                        IWebElement Taxpayhist = driver.FindElement(By.XPath("//*[@id='dnn_ctr380_View_divPaymentHistory']/div/table/tbody"));
                        IList<IWebElement> TRTaxpayhist = Taxpayhist.FindElements(By.TagName("tr"));
                        IList<IWebElement> THTaxpayhist = Taxpayhist.FindElements(By.TagName("th"));
                        IList<IWebElement> TDTaxpayhist;
                        foreach (IWebElement row2 in TRTaxpayhist)
                        {
                            TDTaxpayhist = row2.FindElements(By.TagName("td"));
                            if (TDTaxpayhist.Count != 0 && !row2.Text.Contains("TRANSACTION DATE") && TDTaxpayhist.Count == 4 && row2.Text.Trim() != "")
                            {
                                string Taxpayment = TDTaxpayhist[0].Text + "~" + TDTaxpayhist[1].Text + "~" + TDTaxpayhist[2].Text + "~" + TDTaxpayhist[3].Text;
                                gc.insert_date(orderNumber, parcelNumber, 17, Taxpayment, 1, DateTime.Now);

                            }
                            if (TDTaxpayhist.Count != 0 && !row2.Text.Contains("TRANSACTION DATE") && TDTaxpayhist.Count == 6 && row2.Text.Trim() != "")
                            {
                                string Taxpayment = TDTaxpayhist[0].Text + "~" + TDTaxpayhist[3].Text + "~" + TDTaxpayhist[4].Text + "~" + TDTaxpayhist[5].Text;
                                gc.insert_date(orderNumber, parcelNumber, 17, Taxpayment, 1, DateTime.Now);

                            }

                        }
                    }
                    catch { }

                    // Tax Receipt Download
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
                    try
                    {
                        string current1 = driver.CurrentWindowHandle;

                        for (int i = 1; i < 4; i++)
                        {
                            try
                            {
                                IWebElement Taxpayhist1 = driver.FindElement(By.XPath("//*[@id='dnn_ctr380_View_divPaymentHistory']/div/table/tbody"));
                                IList<IWebElement> TRTaxpayhist1 = Taxpayhist1.FindElements(By.TagName("tr"));
                                IList<IWebElement> THTaxpayhist1 = Taxpayhist1.FindElements(By.TagName("th"));
                                IList<IWebElement> TDTaxpayhist1;
                                foreach (IWebElement row4 in TRTaxpayhist1)
                                {
                                    TDTaxpayhist1 = row4.FindElements(By.TagName("td"));
                                    if (TDTaxpayhist1.Count != 0 && !row4.Text.Contains("TRANSACTION DATE") && row4.Text.Trim() != "" && TDTaxpayhist1[0].Text == Convert.ToString(Iyear))
                                    {

                                        IWebElement IAddressSearch1 = TDTaxpayhist1[1].FindElement(By.TagName("a"));
                                        IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                                        js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                                        driver.SwitchTo().Window(driver.WindowHandles.Last());
                                        Thread.Sleep(5000);
                                        gc.downloadfile(driver.Url, orderNumber, parcelNumber, "Tax Bill" + Iyear, "OR", "Multnomah");
                                        Thread.Sleep(5000);
                                        driver.SwitchTo().DefaultContent();
                                        Iyear--;
                                    }

                                }
                            }
                            catch { }
                            driver.SwitchTo().Window(current1);


                        }
                    }
                    catch { }

                    CitytaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "OR", "Multnomah", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "OR", "Multnomah");
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