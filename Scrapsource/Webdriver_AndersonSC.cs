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
using System.ComponentModel;
using System.Text;
using HtmlAgilityPack;
using iTextSharp.text;
using System.Text.RegularExpressions;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Support.Extensions;
using System.Net;
using OpenQA.Selenium.Firefox;
using System.Diagnostics;
using OpenQA.Selenium.Remote;
using Org.BouncyCastle.Utilities;

namespace ScrapMaricopa.Scrapsource
{
    public class Webdriver_AndersonSC
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();
        IWebElement clickcheckbox;
        public string FTP_AndersonSC(string streetno, string unitno, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string Parcel_number = "", Tax_Authority = "", As_of = "", Total_Due = "", addresshref="";
            //request.UseDefaultCredentials = true;
            //var driverService = PhantomJSDriverService.CreateDefaultService();
            //driverService.HideCommandPromptWindow = true;
            //driver = new ChromeDriver();
            var option = new ChromeOptions();
            option.AddArgument("No-Sandbox");
            using (driver = new ChromeDriver(option))
            {
                //driver = new PhantomJSDriver();
                try
                {

                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("https://www.andersoncountysc.org/elected-officials/auditor/");
                    //tax Authority
                    Thread.Sleep(2000);
                    driver.FindElement(By.XPath("//*[@id='fl-accordion-5b294647a77d6-tab-0']/i")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf_WOP(orderNumber, "tax Authority", driver, "SC", "Anderson");
                    IWebElement tax_authoritytable = driver.FindElement(By.XPath("//*[@id='fl-accordion-5b294647a77d6-panel-0']/p[1]"));
                    string Tax_Authority1 = GlobalClass.Before(tax_authoritytable.Text, "Email").Trim();
                    IWebElement taxautho = driver.FindElement(By.XPath("//*[@id='fl-accordion-5b294647a77d6-panel-0']/p[2]"));
                    string Tax_Authority2 = GlobalClass.After(taxautho.Text, "p.m.").Trim();
                    Tax_Authority = Tax_Authority2 + Tax_Authority1;
                }
                catch { }
                try
                {
                    driver.Navigate().GoToUrl("https://acpass.andersoncountysc.org/real_prop_search.htm");
                    Thread.Sleep(2000);
                    gc.CreatePdf_WOP(orderNumber, "agree", driver, "SC", "Anderson");
                    driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[3]/td[2]/table/tbody/tr[8]/td/div/a/img")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf_WOP(orderNumber, "agree1", driver, "SC", "Anderson");
                    driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr[3]/td[1]/a")).SendKeys(Keys.Enter);
                    Thread.Sleep(2000);
                    gc.CreatePdf_WOP(orderNumber, "agree2", driver, "SC", "Anderson");
                    IWebElement agreeclick1 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[3]/map/area[1]"));
                    string agreeaclick1 = agreeclick1.GetAttribute("href");
                    agreeclick1.SendKeys(Keys.Enter);
                    Thread.Sleep(3000);
                    gc.CreatePdf_WOP(orderNumber, "agree3", driver, "SC", "Anderson");
                    if (searchType == "titleflex")
                    {
                        //string address = houseno + " " + direction + " " + streetname + " " + streettype;
                        gc.TitleFlexSearch(orderNumber, "", "", streetno.Trim(), "SC", "Anderson");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_AndersonSC"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString().Replace("-", "");
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {

                        driver.FindElement(By.XPath(" //*[@id='Search']/table/tbody/tr[5]/td[2]/div/font/font/input")).SendKeys(streetno);
                        driver.FindElement(By.Id("Sumbit")).Click();
                        Thread.Sleep(2000);

                        int a = 0;
                        gc.CreatePdf_WOP(orderNumber, "Address After click", driver, "SC", "Anderson");

                        IWebElement Addressmultitable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[3]/td[1]/table[2]/tbody/tr[1]/td/form/table/tbody"));
                        IList<IWebElement> AddressmutiRow = Addressmultitable.FindElements(By.TagName("tr"));
                        IList<IWebElement> Addressmutiid;
                        foreach (IWebElement addressmulti in AddressmutiRow)
                        {
                            Addressmutiid = addressmulti.FindElements(By.TagName("td"));
                            if (Addressmutiid.Count != 0 && !addressmulti.Text.Contains("NAME") && addressmulti.Text.Trim() != "")
                            {
                                if (Addressmutiid[4].Text.Contains(streetno.ToUpper()))
                                {
                                    IWebElement onerowaddress = Addressmutiid[1].FindElement(By.TagName("a"));
                                    addresshref = onerowaddress.GetAttribute("href");
                                    string Taxmnpnumber = Addressmutiid[2].Text;
                                    string Multiaddress = Addressmutiid[1].Text + "~" + Addressmutiid[4].Text;
                                    gc.insert_date(orderNumber, Taxmnpnumber, 909, Multiaddress, 1, DateTime.Now);
                                    a++;
                                }
                            }
                        }
                        if (a == 1)
                        {
                            driver.Navigate().GoToUrl(addresshref);
                            Thread.Sleep(2000);
                        }
                        if (a > 1 && a < 26)
                        {
                            HttpContext.Current.Session["multiparcel_Anderson"] = "Yes";
                            driver.Quit();
                            return "MultiParcel";
                        }
                        if (a > 25)
                        {
                            HttpContext.Current.Session["multiParcel_Anderson_Multicount"] = "Maximum";
                            driver.Quit();
                            return "Maximum";
                        }

                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.XPath("//*[@id='Search']/table/tbody/tr[3]/td[2]/div/font/font/input")).SendKeys(parcelNumber.Replace("-", "").Substring(0, 10));
                        gc.CreatePdf_WOP(orderNumber, "Address After clickP", driver, "SC", "Anderson");
                        driver.FindElement(By.Id("Sumbit")).Click();
                        Thread.Sleep(2000);
                    }
                    if (searchType == "ownername")
                    {
                        int Own = 0;
                        driver.FindElement(By.XPath("//*[@id='Search']/table/tbody/tr[1]/td[2]/div/font/font/input")).SendKeys(ownername);
                        driver.FindElement(By.Id("Sumbit")).Click();
                        Thread.Sleep(4000);
                        try
                        {
                            gc.CreatePdf_WOP(orderNumber, "Address After clickOWN", driver, "SC", "Anderson");
                            IWebElement Addressmultitable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[3]/td[1]/table[2]/tbody/tr[1]/td/form/table/tbody"));
                            IList<IWebElement> AddressmutiRow = Addressmultitable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Addressmutiid;
                            foreach (IWebElement addressmulti in AddressmutiRow)
                            {
                                Addressmutiid = addressmulti.FindElements(By.TagName("td"));
                                if (Addressmutiid.Count != 0 && !addressmulti.Text.Contains("NAME") && addressmulti.Text.Trim() != "")
                                {
                                    if (Addressmutiid[1].Text.Contains(ownername))
                                    {
                                        clickcheckbox = Addressmutiid[0];
                                        string Taxmnpnumber = Addressmutiid[2].Text;
                                        string Multiaddress = Addressmutiid[1].Text + "~" + Addressmutiid[3].Text;
                                        gc.insert_date(orderNumber, Taxmnpnumber, 909, Multiaddress, 1, DateTime.Now);
                                        Own++;
                                    }
                                }
                            }
                            if (Own == 1)
                            {
                                clickcheckbox.Click();
                                driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[3]/td[1]/table[2]/tbody/tr[1]/td/form/table/tbody/tr[27]/td/div/input[4]")).Click();
                                Thread.Sleep(2000);
                            }
                            if (Own > 1 && Own < 26)
                            {
                                HttpContext.Current.Session["multiparcel_Anderson"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Own > 25 && AddressmutiRow.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Anderson_Multicount"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[3]/td[2]/table"));
                        if(INodata.Text.Contains("Provided was not found"))
                        {
                            HttpContext.Current.Session["Nodata_AndersonSC"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        IWebElement INodata1 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[3]/td[1]/table[2]"));
                        IList<IWebElement> INodataRow = INodata1.FindElements(By.TagName("tr"));
                        if(INodataRow.Count <= 1)
                        {
                            HttpContext.Current.Session["Nodata_AndersonSC"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }


                    string Parcel_number1 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[3]/td[1]/table[2]/tbody/tr/td/table[1]/tbody/tr[5]/td[1]")).Text;
                    Parcel_number = Parcel_number1.Replace("-", "");
                    IWebElement propertdetailtable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[3]/td[1]/table[2]/tbody/tr/td/table[2]/tbody"));
                    string owner_name = gc.Between(propertdetailtable.Text, "Owner\r\nName", " Name ");
                    string cur_address = gc.Between(propertdetailtable.Text, "Address", " Address ");
                    string city_State = gc.Between(propertdetailtable.Text, "State", " City,");
                    string Zip = gc.Between(propertdetailtable.Text, "Zip", " Zip ");
                    IWebElement propertydetail1 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[3]/td[1]/table[2]/tbody/tr/td/table[4]/tbody"));
                    string Subdivision = gc.Between(propertydetail1.Text, "Subdivision", "Tax District");
                    string PhysicalAddress = gc.Between(propertydetail1.Text, "Physical Address", "Market Value");
                    string TaxDistrict = gc.Between(propertydetail1.Text, "Tax District", "Physical Address");
                    string MarketValue = gc.Between(propertydetail1.Text, "Market Value", "M/H");
                    string PriorValue = gc.Between(propertydetail1.Text, "Prior Value", "Tax Value");
                    string TaxValue = gc.Between(propertydetail1.Text, "Tax Value", "Exempt");
                    string Exempt = GlobalClass.After(propertydetail1.Text, "Exempt");
                    IWebElement legaltable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[3]/td[1]/table[2]/tbody/tr/td/table[5]/tbody"));
                    string legal1 = gc.Between(legaltable.Text, "Legal Desc 1", "Legal Desc 2");
                    string legal2 = gc.Between(legaltable.Text, "Legal Desc 2", "Legal Desc 3");
                    string legal3 = GlobalClass.After(legaltable.Text, "Legal Desc 3");
                    string Legaldescription = legal1 + legal2 + legal3;
                    gc.CreatePdf(orderNumber, Parcel_number, "Property Search Result", driver, "SC", "Anderson");
                    string Propertyresult = owner_name + "~" + cur_address + "~" + city_State + "~" + Zip + "~" + Subdivision + "~" + PhysicalAddress + "~" + TaxDistrict + "~" + MarketValue + "~" + PriorValue + "~" + TaxValue + "~" + Exempt + "~" + Legaldescription;
                    gc.insert_date(orderNumber, Parcel_number, 902, Propertyresult, 1, DateTime.Now);

                    //Assessment
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    IWebElement Assessmenttable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[3]/td[1]/table[2]/tbody/tr/td/table[6]/tbody"));
                    IList<IWebElement> assessmentrow = Assessmenttable.FindElements(By.TagName("tr"));
                    IList<IWebElement> assessmentid;
                    foreach (IWebElement assessment in assessmentrow)
                    {
                        assessmentid = assessment.FindElements(By.TagName("td"));
                        if (assessmentid.Count != 0 && !assessment.Text.Contains("YEAR") && !assessment.Text.Contains("Assessment Totals"))
                        {
                            string assessmentresult = assessmentid[0].Text + "~" + assessmentid[1].Text + "~" + assessmentid[2].Text + "~" + assessmentid[3].Text + "~" + assessmentid[4].Text + "~" + assessmentid[5].Text + "~" + assessmentid[6].Text + "~" + assessmentid[7].Text + "~" + assessmentid[8].Text;
                            gc.insert_date(orderNumber, Parcel_number, 903, assessmentresult, 1, DateTime.Now);
                        }

                    }

                    //Tax Information
                    driver.Navigate().GoToUrl("http://acpass.andersoncountysc.org/p_tax_search.htm");
                    Thread.Sleep(3000);
                    driver.FindElement(By.Id("QryMapno")).SendKeys(Parcel_number.Substring(0, 10));
                    gc.CreatePdf(orderNumber, Parcel_number, "Tax site", driver, "SC", "Anderson");
                    driver.FindElement(By.Id("Sumbit")).Click();
                    Thread.Sleep(2000);
                    List<string> ParcelSearch = new List<string>();
                    try
                    {
                        gc.CreatePdf(orderNumber, Parcel_number, "Tax Information click", driver, "SC", "Anderson");
                        IWebElement ParcelTB = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[3]/td[1]/table[2]/tbody/tr[1]/td/form/table/tbody"));
                        IList<IWebElement> ParcelTR = ParcelTB.FindElements(By.TagName("tr"));
                        ParcelTR.Reverse();
                        int rows_count = ParcelTR.Count - 1;

                        for (int row = 0; row < rows_count; row++)
                        {
                            if (row == rows_count - 3 || row == rows_count - 1 || row == rows_count - 2)
                            {
                                IList<IWebElement> Columns_row = ParcelTR[row].FindElements(By.TagName("td"));
                                int columns_count = Columns_row.Count;
                                if (columns_count != 0)
                                {
                                    IWebElement ParcelBill_link = Columns_row[1].FindElement(By.TagName("a"));
                                    string Parcelurl = ParcelBill_link.GetAttribute("href");
                                    ParcelSearch.Add(Parcelurl);
                                }
                            }
                        }
                    }
                    catch { }
                    foreach (string taxlink in ParcelSearch)
                    {
                        driver.Navigate().GoToUrl(taxlink);

                        IWebElement accountnumbertable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[3]/td[1]/table[2]/tbody/tr[3]/td/table[1]/tbody/tr[1]/td/table/tbody"));
                        string accountnumber = gc.Between(accountnumbertable.Text, "Account Number:", "TMS#:").Trim();
                        string accountyear = GlobalClass.Before(accountnumber, " ");
                        IWebElement taxdetailtabletable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[3]/td[1]/table[2]/tbody/tr[3]/td/table[1]/tbody"));
                        string citylevel = gc.Between(taxdetailtabletable.Text, "City:", "County:");
                        string countylevel = gc.Between(taxdetailtabletable.Text, "County:", "State");
                        string assessmenttax = GlobalClass.After(taxdetailtabletable.Text, "Assessment:");
                        string taxdetailresult = accountnumber + "~" + citylevel + "~" + countylevel + "~" + assessmenttax + "~" + Tax_Authority;
                        gc.insert_date(orderNumber, Parcel_number, 907, taxdetailresult, 1, DateTime.Now);
                        gc.CreatePdf(orderNumber, Parcel_number, "Tax Information" + accountyear, driver, "SC", "Anderson");

                        string paied = "", date = "";

                        IWebElement taxpaymenttable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[3]/td[1]/table[2]/tbody/tr[3]/td/table[3]/tbody"));
                        IList<IWebElement> taxpaymentrow = taxpaymenttable.FindElements(By.TagName("tr"));
                        IList<IWebElement> taxpaymentid;
                        foreach (IWebElement taxpayment in taxpaymentrow)
                        {
                            taxpaymentid = taxpayment.FindElements(By.TagName("td"));
                            if (taxpaymentid.Count != 0 && taxpayment.Text.Contains("Paid Date"))
                            {
                                paied = taxpaymentid[1].Text;
                                date = taxpaymentid[2].Text;
                            }
                            if (taxpaymentid.Count != 0 && taxpayment.Text.Contains("Payments"))
                            {
                                string taxpaymentresult1 = "Accountnumber" + "~" + "Paied date" + "~" + "Discription" + "~" + taxpaymentid[1].Text + "~" + taxpaymentid[2].Text + "~" + taxpaymentid[3].Text + "~" + "Data" + "~" + "Amount";
                                db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + taxpaymentresult1 + "' where Id = '" + 908 + "'");
                            }
                            if (taxpayment.Text.Contains("Tax") || taxpayment.Text.Contains("Pen:") || taxpayment.Text.Contains("Fee:") || taxpayment.Text.Contains("Total:"))
                            {
                                string taxpaymentresult = accountnumber + "~" + date + "~" + taxpaymentid[0].Text + "~" + taxpaymentid[1].Text.Trim() + "~" + taxpaymentid[2].Text + "~" + taxpaymentid[3].Text + "~" + taxpaymentid[4].Text + "~" + taxpaymentid[5].Text;
                                gc.insert_date(orderNumber, Parcel_number, 908, taxpaymentresult, 1, DateTime.Now);
                                date = "";
                            }
                            if (taxpaymentid.Count == 1 && taxpayment.Text.Contains("County"))
                            {
                                string taxpaymentresult = accountnumber + "~" + "~" + "Tax" + "~" + taxpaymentid[0].Text + "~" + "~" + "~" + "~";
                                gc.insert_date(orderNumber, Parcel_number, 908, taxpaymentresult, 1, DateTime.Now);
                            }
                        }
                    }
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "SC", "Anderson", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    driver.Quit();
                    gc.mergpdf(orderNumber, "SC", "Anderson");
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