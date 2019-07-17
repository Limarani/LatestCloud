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
//using iTextSharp.text;
using System.Text.RegularExpressions;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Support.Extensions;
using System.Net;
using OpenQA.Selenium.Firefox;
using System.Diagnostics;
using OpenQA.Selenium.Remote;

namespace ScrapMaricopa.Scrapsource
{
    public class Webdriver_wapierce
    {
        string outputPath = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_pierce(string houseno, string sname, string sttype, string blockno, string parcelNumber, string searchType, string orderNumber, string directParcel, string direction)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            List<string> mcheck = new List<string>();

            string outparcelno = "", siteaddr = "-", taxpayer = "-", tax_desc = "-", year_built = "-", tax_authority = "-", propertyType = "-", occupancy = "-", property_type = "-", usecode = "-";
            string valued_year = "-", tax_year = "-", assess_land = "-", ass_improve = "-", ass_total = "-", tax_value = "-", exem = "-";
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string maddress = "";
            if (direction == "")
            {
                maddress = houseno + " " + sname;
            }
            else
            {
                maddress = houseno + " " + direction + " " + sname;
            }

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            // using (driver = new PhantomJSDriver())
            using (driver = new PhantomJSDriver())
            {

                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("https://epip.co.pierce.wa.us/cfapps/atr/epip/search.cfm");
                        driver.FindElement(By.XPath("//*[@id='customContent']/table/tbody/tr[1]/td/table[3]/tbody/tr[3]/td/table/tbody/tr/td[5]/table/tbody/tr/td/form/input[1]")).SendKeys(houseno);
                        driver.FindElement(By.XPath("//*[@id='customContent']/table/tbody/tr[1]/td/table[3]/tbody/tr[3]/td/table/tbody/tr/td[5]/table/tbody/tr/td/form/input[2]")).SendKeys(sname);
                        gc.CreatePdf_WOP(orderNumber, "AddressSearch_InputPassed", driver, "WA", "Pierce");
                        var wait = new WebDriverWait(driver, TimeSpan.FromMilliseconds(3000));
                        wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@id='customContent']/table/tbody/tr[1]/td/table[3]/tbody/tr[3]/td/table/tbody/tr/td[5]/table/tbody/tr/td/form/input[4]")));
                        driver.FindElement(By.XPath("//*[@id='customContent']/table/tbody/tr[1]/td/table[3]/tbody/tr[3]/td/table/tbody/tr/td[5]/table/tbody/tr/td/form/input[4]")).Click();
                        IWebElement Iresult = driver.FindElement(By.XPath("//*[@id='customContent']/table/tbody/tr[1]/td/table[2]/tbody/tr/td"));
                        string result = Iresult.Text;

                        if (!result.Contains("1 record(s)"))
                        {
                            if (result.Contains("0 record(s)"))
                            {
                                HttpContext.Current.Session["Nodata_PierceWA"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                            else
                            {
                                string xpath = "";
                                gc.CreatePdf_WOP(orderNumber, "MultipleAddress", driver, "WA", "Pierce");
                                //Select address from list....
                                IWebElement multiaddrtableElement = driver.FindElement(By.XPath("//*[@id='customContent']/table/tbody/tr[1]/td/table[3]/tbody/tr/td/table/tbody"));
                                IList<IWebElement> multiaddrtableRow = multiaddrtableElement.FindElements(By.TagName("tr"));
                                IList<IWebElement> multiaddrrowTD;
                                int m = 1;
                                foreach (IWebElement row in multiaddrtableRow)
                                {
                                    multiaddrrowTD = row.FindElements(By.TagName("td"));
                                    if (multiaddrrowTD.Count != 0 && row.Text.Contains(maddress.ToUpper()))
                                    {
                                        //Type~Status~TaxpayerName~SiteAddress
                                        string multi = multiaddrrowTD[1].Text.Trim() + "~" + multiaddrrowTD[2].Text.Trim() + "~" + multiaddrrowTD[3].Text.Trim() + "~" + multiaddrrowTD[4].Text.Trim();
                                        gc.insert_date(orderNumber, multiaddrrowTD[0].Text.Trim(), 36, multi, 1, DateTime.Now);
                                        //db.ExecuteQuery("insert into la_multiowner(parcel_no,type,status,taxpayer_name,situs_address,order_no) values('" + multiaddrrowTD[0].Text + "','" + multiaddrrowTD[1].Text + "','" + multiaddrrowTD[2].Text + "','" + multiaddrrowTD[3].Text + "','" + multiaddrrowTD[4].Text + "','" + orderNumber + "') ");
                                        xpath = "//*[@id='customContent']/table/tbody/tr[1]/td/table[3]/tbody/tr/td/table/tbody/tr[" + m + "]/td[1]/a";
                                        mcheck.Add(multiaddrrowTD[0].Text.Trim());
                                    }
                                    m++;
                                }
                                if (mcheck.Count == 1)
                                {
                                    driver.FindElement(By.XPath(xpath)).Click();
                                }
                                else
                                {
                                    if (mcheck.Count <= 25)
                                    {
                                        driver.Quit();
                                        HttpContext.Current.Session["multiparcel_WAPierce"] = "Yes";
                                        return "MultiParcel";
                                    }

                                    else
                                    {
                                        driver.Quit();
                                        HttpContext.Current.Session["Maximum_WAPierce"] = "Yes";
                                        return "Maximum";
                                    }
                                }
                            }
                        }
                        else
                        {
                            driver.FindElement(By.XPath("//*[@id='customContent']/table/tbody/tr[1]/td/table[3]/tbody/tr/td/table/tbody/tr[2]/td[1]/a")).Click();
                        }

                    }

                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, "", directParcel, "", "WA", "Pierce");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_PierceWA"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "parcel")
                    {
                        if (HttpContext.Current.Session["titleparcel"] != null)
                        {
                            parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        }
                        if (parcelNumber.Contains("-"))
                        {
                            parcelNumber = parcelNumber.Replace("-", "");
                        }
                        driver.Navigate().GoToUrl("https://epip.co.pierce.wa.us/cfapps/atr/epip/search.cfm");
                        driver.FindElement(By.XPath("//*[@id='customContent']/table/tbody/tr[1]/td/table[2]/tbody/tr[3]/td/table/tbody/tr/td[1]/table/tbody/tr/td/form/input[1]")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearch", driver, "WA", "Pierce");

                        var wait = new WebDriverWait(driver, TimeSpan.FromMilliseconds(3000));
                        wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@id='customContent']/table/tbody/tr[1]/td/table[2]/tbody/tr[3]/td/table/tbody/tr/td[1]/table/tbody/tr/td/form/input[3]")));
                        driver.FindElement(By.XPath("//*[@id='customContent']/table/tbody/tr[1]/td/table[2]/tbody/tr[3]/td/table/tbody/tr/td[1]/table/tbody/tr/td/form/input[3]")).Click();
                        try
                        {
                            Thread.Sleep(3000);
                            IWebElement Iresult = driver.FindElement(By.XPath("//*[@id='customContent']/table/tbody/tr[1]/td/table[2]/tbody/tr/td"));
                            string result = Iresult.Text;
                            if (result.Contains("0 record(s)"))
                            {
                                HttpContext.Current.Session["Nodata_PierceWA"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                        driver.FindElement(By.LinkText(parcelNumber)).Click();
                        Thread.Sleep(3000);
                    }

                    if (searchType == "block")
                    {
                        driver.Navigate().GoToUrl("https://epip.co.pierce.wa.us/cfapps/atr/epip/search.cfm");
                        driver.FindElement(By.XPath("//*[@id='customContent']/table/tbody/tr[1]/td/table[4]/tbody/tr[3]/td/table/tbody/tr/td[1]/table/tbody/tr/td/form/input[1]")).SendKeys(blockno);
                        driver.FindElement(By.XPath("//*[@id='customContent']/table/tbody/tr[1]/td/table[4]/tbody/tr[3]/td/table/tbody/tr/td[1]/table/tbody/tr/td/form/input[2]")).SendKeys(sname);
                        gc.CreatePdf_WOP(orderNumber, "BlockSearch", driver, "WA", "Pierce");

                        var wait = new WebDriverWait(driver, TimeSpan.FromMilliseconds(3000));
                        wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@id='customContent']/table/tbody/tr[1]/td/table[4]/tbody/tr[3]/td/table/tbody/tr/td[1]/table/tbody/tr/td/form/input[4]")));
                        driver.FindElement(By.XPath("//*[@id='customContent']/table/tbody/tr[1]/td/table[4]/tbody/tr[3]/td/table/tbody/tr/td[1]/table/tbody/tr/td/form/input[4]")).Click();
                        IWebElement Iresult = driver.FindElement(By.XPath("//*[@id='customContent']/table/tbody/tr[1]/td/table[2]/tbody/tr/td"));
                        string result = Iresult.Text;
                        if (!result.Contains("1 record(s)"))
                        {
                            if (result.Contains("0 record(s)"))
                            {
                                HttpContext.Current.Session["Nodata_PierceWA"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                            gc.CreatePdf_WOP(orderNumber, "MultipleAddress", driver, "WA", "Pierce");
                            HttpContext.Current.Session["multiparcel_WAPierce"] = "Yes";
                            //Select address from list....
                            IWebElement multiaddrtableElement = driver.FindElement(By.XPath("//*[@id='customContent']/table/tbody/tr[1]/td/table[3]/tbody/tr/td/table"));
                            IList<IWebElement> multiaddrtableRow = multiaddrtableElement.FindElements(By.TagName("tr"));
                            IList<IWebElement> multiaddrrowTD;

                            foreach (IWebElement row in multiaddrtableRow)
                            {
                                multiaddrrowTD = row.FindElements(By.TagName("td"));
                                if (multiaddrrowTD.Count != 0)
                                {
                                    string multi = multiaddrrowTD[1].Text.Trim() + "~" + multiaddrrowTD[2].Text.Trim() + "~" + multiaddrrowTD[3].Text.Trim() + "~" + multiaddrrowTD[4].Text.Trim();
                                    gc.insert_date(orderNumber, multiaddrrowTD[0].Text.Trim(), 36, multi, 1, DateTime.Now);
                                    // db.ExecuteQuery("insert into la_multiowner(parcel_no,type,status,taxpayer_name,situs_address,order_no) values('" + multiaddrrowTD[0].Text + "','" + multiaddrrowTD[1].Text + "','" + multiaddrrowTD[2].Text + "','" + multiaddrrowTD[3].Text + "','" + multiaddrrowTD[4].Text + "','" + orderNumber + "') ");
                                }
                            }
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else
                        {

                            //*[@id="customContent"]/table/tbody/tr[1]/td/table[3]/tbody/tr/td/table/tbody/tr[2]/td[1]/a
                            driver.FindElement(By.XPath("//*[@id='customContent']/table/tbody/tr[1]/td/table[3]/tbody/tr/td/table/tbody/tr[2]/td[1]/a")).Click();

                        }

                    }


                    IWebElement Ioutparcelno = driver.FindElement(By.XPath(".//*[@id='customContent']/table/tbody/tr[1]/td/table[2]/tbody/tr/td[1]/table/tbody/tr[2]/td[2]"));
                    outparcelno = Ioutparcelno.Text;
                    IWebElement Isiteaddr = driver.FindElement(By.XPath(".//*[@id='customContent']/table/tbody/tr[1]/td/table[2]/tbody/tr/td[1]/table/tbody/tr[3]/td[2]"));
                    siteaddr = Isiteaddr.Text;
                    IWebElement Itaxpayer = driver.FindElement(By.XPath(".//*[@id='customContent']/table/tbody/tr[1]/td/table[2]/tbody/tr/td[2]/table/tbody/tr[2]/td[2]"));
                    taxpayer = Itaxpayer.Text;
                    IWebElement Itax_desc = driver.FindElement(By.XPath(".//*[@id='customContent']/table/tbody/tr[1]/td/table[5]/tbody/tr/td/table/tbody/tr[2]/td"));
                    tax_desc = Itax_desc.Text;
                    property_type = driver.FindElement(By.XPath("//*[@id='customContent']/table/tbody/tr[1]/td/table[2]/tbody/tr/td[1]/table/tbody/tr[4]/td[2]")).Text;
                    usecode = driver.FindElement(By.XPath(".//*[@id='customContent']/table/tbody/tr[1]/td/table[2]/tbody/tr/td[1]/table/tbody/tr[6]/td[2]")).Text;

                    //Summary Tab
                    gc.CreatePdf(orderNumber, outparcelno, "SummaryTab_Details", driver, "WA", "Pierce");
                    try
                    {
                        driver.FindElement(By.LinkText("Buildings")).SendKeys(Keys.Enter);
                        gc.CreatePdf(orderNumber, outparcelno, "BuildingTab", driver, "WA", "Pierce");

                        //buildings Tab

                        IWebElement Iyearbuilt = driver.FindElement(By.XPath(".//*[@id='customContent']/table/tbody/tr[1]/td/table[4]/tbody/tr/td/table/tbody/tr[2]/td/table[2]/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]"));
                        year_built = Iyearbuilt.Text;

                        propertyType = driver.FindElement(By.XPath("//*[@id='customContent']/table/tbody/tr[1]/td/table[4]/tbody/tr/td/table/tbody/tr[2]/td/table[1]/tbody/tr[1]/td[2]")).Text;
                        occupancy = driver.FindElement(By.XPath("//*[@id='customContent']/table/tbody/tr[1]/td/table[4]/tbody/tr/td/table/tbody/tr[2]/td/table[1]/tbody/tr[5]/td[2]")).Text;
                    }
                    catch
                    {

                    }

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    //Taxes/Values Tab
                    driver.FindElement(By.LinkText("Taxes/Values")).SendKeys(Keys.Enter);

                    gc.CreatePdf(orderNumber, outparcelno, "TaxValues", driver, "WA", "Pierce");

                    IWebElement Iexem = driver.FindElement(By.XPath(".//*[@id='customContent']/table/tbody/tr[1]/td/table[4]/tbody/tr/td[2]/table[1]/tbody/tr[2]/td/table/tbody/tr/td"));
                    exem = Iexem.Text;

                    IWebElement tableElement = driver.FindElement(By.XPath(".//*[@id='customContent']/table/tbody/tr[1]/td/table[3]/tbody/tr/td/table/tbody/tr[2]/td/table"));
                    IList<IWebElement> tableRow = tableElement.FindElements(By.TagName("tr"));
                    IList<IWebElement> rowTD;
                    int i = 0;
                    foreach (IWebElement row in tableRow)
                    {

                        rowTD = row.FindElements(By.TagName("td"));
                        if (rowTD.Count != 0)
                        {
                            if (i < 3)
                            {
                                valued_year = rowTD[0].Text;
                                tax_year = rowTD[1].Text;
                                tax_value = rowTD[2].Text;
                                ass_total = rowTD[3].Text;
                                assess_land = rowTD[4].Text;
                                ass_improve = rowTD[5].Text;
                                //Valued_Year~Tax_Year~Assessed_Land~Assessed_Improvements~Assessed_Total~Taxable_Value~Valued_Year~exemption_type
                                string ass = valued_year + "~" + tax_year + "~" + assess_land + "~" + ass_improve + "~" + ass_total + "~" + tax_value + "~" + exem;
                                gc.insert_date(orderNumber, outparcelno, 35, ass, 1, DateTime.Now);
                                //db.ExecuteQuery("insert into la_assessor (parcel_no,value_year,year,land,improvements,total,tax_value, order_no,exemption_type) values ('" + outparcelno + "','" + valued_year + "', '" + tax_year + "', '" + assess_land + "' , '" + ass_improve + "','" + ass_total + "' ,'" + tax_value + "' , '" + orderNumber + "','" + exem + "') ");
                            }
                        }
                        i++;

                    }


                    //Treasurer Details

                    // receipt Details
                    try
                    {
                        IWebElement receiptstableElement = driver.FindElement(By.XPath(".//*[@id='customContent']/table/tbody/tr[1]/td/table[4]/tbody/tr/td[2]/table[3]/tbody/tr[2]/td/table"));
                        IList<IWebElement> receiptstableRow = receiptstableElement.FindElements(By.TagName("tr"));
                        IList<IWebElement> receiptsrowTD;
                        foreach (IWebElement row in receiptstableRow)
                        {
                            receiptsrowTD = row.FindElements(By.TagName("td"));
                            if (receiptsrowTD.Count != 0)
                            {
                                //Paid_date~Receipt_number~Paid_Amount
                                string bill = receiptsrowTD[0].Text.Trim() + "~" + receiptsrowTD[1].Text.Trim() + "~" + receiptsrowTD[2].Text.Trim();
                                gc.insert_date(orderNumber, outparcelno, 37, bill, 1, DateTime.Now);
                                //db.ExecuteQuery("insert into bill_details (order_no,parcel_no,paid_date,receipt_no,amount) values ('" + orderNumber + "', '" + outparcelno + "', '" + receiptsrowTD[0].Text + "', '" + receiptsrowTD[1].Text + "', '" + receiptsrowTD[2].Text + "')");

                            }
                        }

                    }
                    catch { }
                    //delinquent details
                    try
                    {
                        IWebElement delinqtableElement = driver.FindElement(By.XPath(".//*[@id='customContent']/table/tbody/tr[1]/td/table[4]/tbody/tr/td[1]/table[1]/tbody/tr[4]/td/table"));
                        IList<IWebElement> delinqtableRow = delinqtableElement.FindElements(By.TagName("tr"));
                        IList<IWebElement> delinqrowTD;
                        int j = 0;
                        foreach (IWebElement row in delinqtableRow)
                        {
                            delinqrowTD = row.FindElements(By.TagName("td"));
                            if (delinqrowTD.Count != 0 && delinqrowTD.Count > 3)
                            {

                                if (j > 2)
                                {

                                    //tax_year~Charge_Type~Amount_Charged~Minimum_Due~Balance_Due~Due_Date
                                    string taxyear = delinqrowTD[0].Text.Trim();
                                    if (taxyear == "")
                                    {
                                        taxyear = "-";
                                    }
                                    string tax = taxyear + "~" + delinqrowTD[1].Text + "~" + delinqrowTD[2].Text + "~" + delinqrowTD[3].Text.Trim() + "~" + delinqrowTD[4].Text.Trim() + "~" + delinqrowTD[5].Text.Trim();
                                    gc.insert_date(orderNumber, outparcelno, 39, tax, 1, DateTime.Now);
                                    //db.ExecuteQuery("insert into la_delinquent (order_no,parcel_no,default_year,charge_type,amount_paid,redemption_amt,pay_amount_due,pay_due_date) values ('" + orderNumber + "', '" + outparcelno + "', '" + delinqrowTD[0].Text + "', '" + delinqrowTD[1].Text + "', '" + delinqrowTD[2].Text + "', '" + delinqrowTD[3].Text + "','" + delinqrowTD[4].Text + "','" + delinqrowTD[5].Text + "')");
                                }

                            }
                            j++;
                        }
                    }
                    catch
                    {
                    }

                    IWebElement Itaxauthority = driver.FindElement(By.XPath("//*[@id='customContent']/table/tbody/tr[2]/td/p[1]"));
                    tax_authority = Itaxauthority.Text;
                    if (tax_authority.Contains("\r\n"))
                    {
                        tax_authority = tax_authority.Replace("\r\n", ",");
                    }
                    // db.ExecuteQuery("update la_multiowner set tax_authority = '" + tax_authority + "' where order_no ='" + orderNumber + "'");

                    //Taxpayer_Name~Site_Address~Legal_Description~Year_Built~tax_authority~property_type
                    string property_details = taxpayer + "~" + siteaddr + "~" + tax_desc + "~" + year_built + "~" + tax_authority + "~" + property_type + "~" + usecode;
                    gc.insert_date(orderNumber, outparcelno, 34, property_details, 1, DateTime.Now);
                    //db.ExecuteQuery("insert into la_multiowner ( parcel_no,situs_address, taxpayer_name,year_built, legal_description,order_no,property_type,occupancy) values ('" + outparcelno + "', '" + siteaddr + "','" + taxpayer + "', '" + year_built + "','" + tax_desc + "','" + orderNumber + "','" + propertyType + "','" + occupancy + "')");

                    //Treasurer Details

                    IWebElement taxtableElement = driver.FindElement(By.XPath(".//*[@id='customContent']/table/tbody/tr[1]/td/table[4]/tbody/tr/td[1]/table[2]/tbody/tr[3]/td/table"));
                    IList<IWebElement> taxtableRow = taxtableElement.FindElements(By.TagName("tr"));
                    IList<IWebElement> taxrowTD;
                    foreach (IWebElement row in taxtableRow)
                    {
                        taxrowTD = row.FindElements(By.TagName("td"));
                        if (taxrowTD.Count != 0)
                        {

                            string year = taxrowTD[0].Text.Trim();
                            if (year == "")
                            {
                                year = "-";
                            }
                            string charge_type = taxrowTD[1].Text;
                            string amountpaid = taxrowTD[2].Text;
                            //tax_payer_name~tax_year~charge_type~amount_paid
                            string breakdown = taxpayer + "~" + year + "~" + charge_type + "~" + amountpaid;
                            gc.insert_date(orderNumber, outparcelno, 40, breakdown, 1, DateTime.Now);
                            //db.ExecuteQuery("insert into paid_charges (order_no,parcel_no,tax_payer_name,tax_year,charge_type,amount_paid) values('" + orderNumber + "','" + outparcelno + "','" + taxpayer + "','" + year + "','" + charge_type + "','" + amountpaid + "')");
                        }
                    }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "WA", "Pierce", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);


                    driver.Quit();
                    //megrge pdf files
                    gc.mergpdf(orderNumber, "WA", "Pierce");
                    GlobalClass.titleparcel = "";
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