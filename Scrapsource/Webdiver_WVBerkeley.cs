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
    public class Webdiver_WVBerkeley
    {
        string Tax_Authority = "", Account_Number = "", Pro_District = "", Owner_Name = "", Pro_Address = "", Legel_Desp = "", Pro_Map = "", Property_Details = "";
        string Home_Exp = "-", Back_Tax = "-", Exoneration = "-", Propr_del = "-";
        string Tax_Year = "", Ticket_Numbet = "", Tax_Class = "", Spl_Desp = "-", First_Half = "", Second_Half = "", Total_Due = "", Tax_Details = "";
        string Installment = "", FirstHlf = "", Scndhlf = "", Payment_details = "";
        string Assessment = "", Gross = "", Net = "", Assessment_details = "";
        string address = "";
        string Type = "", TaxPyrName = "", Address = "", Legel_Desp1 = "", Legel_Desp2 = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_WVBerkeley(string houseno, string housedir, string sname, string sttype, string unitno, string parcelNumber, string searchType, string orderno, string ownername, string directParcel)
        {
            GlobalClass.global_orderNo = orderno;
            HttpContext.Current.Session["orderNo"] = orderno;
            GlobalClass.global_parcelNo = parcelNumber;
            string District = "", Map = "", Pacel = "", SubParcel = "";

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                //driver = new ChromeDriver();

                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    if (searchType == "titleflex")
                    {
                        string titleaddress = houseno + " " + sname;
                        gc.TitleFlexSearch(orderno, "", "", titleaddress, "WV", "Berkeley");

                        if (HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes")
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_WVBerkeley"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "parcel")
                    {

                        driver.Navigate().GoToUrl("http://taxinq.berkeleywv.org/index.html");
                        Thread.Sleep(3000);

                        if (parcelNumber.Contains("-"))
                        {
                            parcelNumber = parcelNumber.Replace("-", "").Replace(" ", "").Replace(".", "");
                        }

                        string CommonParcel = parcelNumber;

                        if (CommonParcel.Length == 11)
                        {
                            District = parcelNumber.Substring(0, 2);
                            Map = parcelNumber.Substring(2, 1);
                            Pacel = parcelNumber.Substring(3, 4);
                            SubParcel = parcelNumber.Substring(7, 4);
                        }
                        if (CommonParcel.Length == 12)
                        {
                            District = parcelNumber.Substring(0, 2);
                            Map = parcelNumber.Substring(2, 2);
                            Pacel = parcelNumber.Substring(4, 4);
                            SubParcel = parcelNumber.Substring(8, 4);
                        }
                        if (CommonParcel.Length == 13)
                        {
                            District = parcelNumber.Substring(0, 2);
                            Map = parcelNumber.Substring(2, 3);
                            Pacel = parcelNumber.Substring(5, 4);
                            SubParcel = parcelNumber.Substring(9, 4);
                        }
                        if (CommonParcel.Length == 14)
                        {
                            District = parcelNumber.Substring(0, 2);
                            Map = parcelNumber.Substring(2, 4);
                            Pacel = parcelNumber.Substring(6, 4);
                            SubParcel = parcelNumber.Substring(10, 4);
                        }
                        if (CommonParcel.Length == 17)
                        {
                            District = parcelNumber.Substring(0, 2);
                            Map = parcelNumber.Substring(2, 3);
                            Pacel = parcelNumber.Substring(5, 4);
                            SubParcel = parcelNumber.Substring(9, 4);
                            try
                            {
                                string SubParcel1 = parcelNumber.Substring(13, 4);
                            }
                            catch
                            { }
                        }

                        var SelectDistrict = driver.FindElement(By.Name("DIST"));
                        var SelectDistrict1 = new SelectElement(SelectDistrict);
                        SelectDistrict1.SelectByValue(District);

                        var SelectMap = driver.FindElement(By.Name("MAP"));
                        var SelectMap1 = new SelectElement(SelectMap);
                        SelectMap1.SelectByText(Map);

                        var SelectParcel = driver.FindElement(By.Name("PARC"));
                        var SelectParcel1 = new SelectElement(SelectParcel);
                        SelectParcel1.SelectByText(Pacel);

                        var SelectSubParcel = driver.FindElement(By.Name("SPAR"));
                        var SelectSubParcel1 = new SelectElement(SelectSubParcel);
                        SelectSubParcel1.SelectByText(SubParcel);
                        gc.CreatePdf(orderno, parcelNumber, "Parcel search", driver, "WV", "Berkeley");

                        driver.FindElement(By.XPath("/html/body/center/table[2]/tbody/tr[4]/td[2]/form/input[4]")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderno, parcelNumber, "Multi Parcel search", driver, "WV", "Berkeley");

                        //Parcel Search
                        List<string> ParcelSearch = new List<string>();

                        try
                        {
                            IWebElement ParcelTB = driver.FindElement(By.XPath("//*[@id='inside']/div/div[1]/table[1]/tbody"));
                            IList<IWebElement> ParcelTR = ParcelTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> ParcelTD;
                            gc.CreatePdf_WOP(orderno, "Parcel MultiParcel search", driver, "WV", "Berkeley");

                            int i = 0;
                            foreach (IWebElement Parcel in ParcelTR)
                            {

                                if (i > 2)
                                {
                                    ParcelTD = Parcel.FindElements(By.TagName("td"));
                                    if (ParcelTD.Count != 0 && !Parcel.Text.Contains("PP"))
                                    {
                                        IWebElement ParcelBill_link = ParcelTD[0].FindElement(By.TagName("a"));
                                        string Parcelurl = ParcelBill_link.GetAttribute("href");
                                        ParcelSearch.Add(Parcelurl);
                                    }

                                }
                                i++;
                            }

                            foreach (string Parcelbill in ParcelSearch)
                            {

                                driver.Navigate().GoToUrl(Parcelbill);
                                Thread.Sleep(3000);

                                Thread.Sleep(2000);
                                driver.SwitchTo().Window(driver.WindowHandles.Last());
                                Thread.Sleep(3000);

                                //Tax Information
                                try
                                {
                                    Tax_Authority = driver.FindElement(By.XPath("/html/body/div[1]/center/div[1]/table/tbody/tr[4]/td/table/tbody/tr/td/font")).Text.Replace("\r\n", "");

                                    Tax_Year = driver.FindElement(By.XPath("//*[@id='body']/div[1]/div")).Text;
                                    Ticket_Numbet = driver.FindElement(By.XPath("//*[@id='body']/table[1]/tbody/tr[1]/td[2]")).Text;
                                    try
                                    {
                                        Tax_Class = driver.FindElement(By.XPath("//*[@id='body']/table[2]/tbody/tr[3]/td/table/tbody/tr/td[2]")).Text;
                                        Spl_Desp = driver.FindElement(By.XPath("//*[@id='body']/table[2]/tbody/tr[3]/td/table/tbody/tr/td[12]")).Text;

                                        if (Spl_Desp == "Delinquent")
                                        {
                                            Home_Exp = "Yes";
                                            Back_Tax = "Yes";
                                            Exoneration = "Yes";
                                            Propr_del = "Yes";
                                        }
                                        else
                                        {
                                            Home_Exp = driver.FindElement(By.XPath("//*[@id='body']/table[2]/tbody/tr[3]/td/table/tbody/tr/td[4]")).Text;
                                            Back_Tax = driver.FindElement(By.XPath("//*[@id='body']/table[2]/tbody/tr[3]/td/table/tbody/tr/td[6]")).Text;
                                            Exoneration = driver.FindElement(By.XPath("//*[@id='body']/table[2]/tbody/tr[3]/td/table/tbody/tr/td[8]")).Text;
                                            Propr_del = driver.FindElement(By.XPath("//*[@id='body']/table[2]/tbody/tr[3]/td/table/tbody/tr/td[10]")).Text;
                                        }
                                    }
                                    catch
                                    { }

                                    First_Half = driver.FindElement(By.XPath("//*[@id='body']/div[2]/table/tbody/tr/td[3]")).Text.Replace("\r\n", "");
                                    Second_Half = driver.FindElement(By.XPath("//*[@id='body']/div[2]/table/tbody/tr/td[5]")).Text.Replace("\r\n", "");
                                    Total_Due = driver.FindElement(By.XPath("//*[@id='body']/div[2]/table/tbody/tr/td[7]")).Text.Replace("\r\n", "");

                                    Tax_Details = Tax_Year + "~" + Ticket_Numbet + "~" + Tax_Class + "~" + Home_Exp + "~" + Back_Tax + "~" + Exoneration + "~" + Propr_del + "~" + Spl_Desp + "~" + First_Half + "~" + Second_Half + "~" + Total_Due + "~" + Tax_Authority;
                                    gc.insert_date(orderno, parcelNumber, 571, Tax_Details, 1, DateTime.Now);
                                }
                                catch
                                { }

                                //Payment Details
                                try
                                {
                                    IWebElement PaymentTB = driver.FindElement(By.XPath("//*[@id='body']/table[3]/tbody/tr/td[3]/table/tbody"));
                                    IList<IWebElement> PaymentTR = PaymentTB.FindElements(By.TagName("tr"));
                                    IList<IWebElement> PaymentTD;

                                    foreach (IWebElement Payment in PaymentTR)
                                    {
                                        PaymentTD = Payment.FindElements(By.TagName("td"));
                                        if (PaymentTD.Count != 0 && !Payment.Text.Contains("First Half"))
                                        {
                                            Installment = PaymentTD[0].Text;
                                            FirstHlf = PaymentTD[1].Text;
                                            Scndhlf = PaymentTD[2].Text;

                                            Payment_details = Installment + "~" + FirstHlf + "~" + Scndhlf;
                                            gc.insert_date(orderno, parcelNumber, 572, Payment_details, 1, DateTime.Now);
                                        }
                                    }
                                }
                                catch
                                { }
                            }

                        }
                        catch
                        { }

                    }

                    else if (searchType == "ownername")
                    {
                        driver.Navigate().GoToUrl("http://taxinq.berkeleywv.org/index.html");
                        Thread.Sleep(2000);

                        driver.FindElement(By.Name("TPNAME")).SendKeys(ownername);
                        driver.FindElement(By.XPath("/html/body/center/table[2]/tbody/tr[1]/td[2]/form/input")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        List<string> OwnerSearch = new List<string>();

                        try
                        {
                            IWebElement OwnerTB = driver.FindElement(By.XPath("//*[@id='inside']/div/div[1]/table[1]/tbody"));
                            IList<IWebElement> OwnerTR = OwnerTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> OwnerTD;
                            gc.CreatePdf_WOP(orderno, "Multi Owner search", driver, "WV", "Berkeley");

                            int maxCheck = 0;

                            foreach (IWebElement Owner in OwnerTR)
                            {
                                OwnerTD = Owner.FindElements(By.TagName("td"));
                                if (OwnerTD.Count != 0 && !Owner.Text.Contains("PP"))
                                {
                                    IWebElement Bill_link = OwnerTD[0].FindElement(By.TagName("a"));
                                    string url = Bill_link.GetAttribute("href");
                                    OwnerSearch.Add(url);
                                }
                            }

                            foreach (string bill in OwnerSearch)
                            {
                                if (maxCheck <= 25)
                                {
                                    driver.Navigate().GoToUrl(bill);
                                    Thread.Sleep(3000);

                                    Thread.Sleep(2000);
                                    driver.SwitchTo().Window(driver.WindowHandles.Last());
                                    Thread.Sleep(3000);

                                    string Dis = driver.FindElement(By.XPath("//*[@id='body']/table[1]/tbody/tr[2]/td[2]")).Text;
                                    Dis = WebDriverTest.Before(Dis, "-").Replace(" ", "");

                                    string mp = driver.FindElement(By.XPath("//*[@id='body']/table[2]/tbody/tr[2]/td/table/tbody/tr[1]/td[2]")).Text;

                                    string Pa = driver.FindElement(By.XPath("//*[@id='body']/table[2]/tbody/tr[2]/td/table/tbody/tr[1]/td[4]")).Text;

                                    string a = Pa.Substring(0, 4);
                                    string b = Pa.Substring(5, 5);
                                    string c = Pa.Substring(11, 5);

                                    Pa = a + b + c;
                                    parcelNumber = Dis + "-" + mp + "-" + Pa;

                                    TaxPyrName = driver.FindElement(By.XPath("//*[@id='body']/table[2]/tbody/tr[1]/td[1]/table/tbody/tr[1]/td[2]")).Text;
                                    Address = driver.FindElement(By.XPath("//*[@id='body']/table[2]/tbody/tr[1]/td[1]/table/tbody/tr[3]/td[2]")).Text;
                                    Type = "RE";

                                    string MultiOwner_details = Type + "~" + TaxPyrName + "~" + Address;
                                    gc.insert_date(orderno, parcelNumber, 573, MultiOwner_details, 1, DateTime.Now);
                                }
                                maxCheck++;
                            }
                            if (OwnerTR.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Berkeley_Multicount"] = "Maximum";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiparcel_Berkeley"] = "Yes";
                            }
                            driver.Quit();

                            return "MultiParcel";
                        }
                        catch { }
                    }

                    else if (searchType == "block")
                    {
                        driver.Navigate().GoToUrl("http://taxinq.berkeleywv.org/index.html");
                        Thread.Sleep(2000);

                        driver.FindElement(By.Name("TPACCT")).SendKeys(unitno);
                        driver.FindElement(By.XPath("/html/body/center/table[2]/tbody/tr[2]/td[2]/form/input")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        List<string> AccountSearch = new List<string>();

                        try
                        {
                            IWebElement AccountTB = driver.FindElement(By.XPath("//*[@id='inside']/div/div[1]/table[1]/tbody"));
                            IList<IWebElement> AccountTR = AccountTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> AccountTD;
                            gc.CreatePdf_WOP(orderno, "Account Owner search", driver, "WV", "Berkeley");

                            int maxCheck = 0;

                            foreach (IWebElement Account in AccountTR)
                            {
                                AccountTD = Account.FindElements(By.TagName("td"));
                                if (AccountTD.Count != 0 && !Account.Text.Contains("PP"))
                                {
                                    IWebElement AccountBill_link = AccountTD[0].FindElement(By.TagName("a"));
                                    string Accounturl = AccountBill_link.GetAttribute("href");
                                    AccountSearch.Add(Accounturl);
                                }
                            }

                            foreach (string Accountbill in AccountSearch)
                            {
                                if (maxCheck <= 25)
                                {
                                    driver.Navigate().GoToUrl(Accountbill);
                                    Thread.Sleep(3000);

                                    Thread.Sleep(2000);
                                    driver.SwitchTo().Window(driver.WindowHandles.Last());
                                    Thread.Sleep(3000);

                                    string Dis1 = driver.FindElement(By.XPath("//*[@id='body']/table[1]/tbody/tr[2]/td[2]")).Text;
                                    Dis1 = WebDriverTest.Before(Dis1, "-").Replace(" ", "");

                                    string mp1 = driver.FindElement(By.XPath("//*[@id='body']/table[2]/tbody/tr[2]/td/table/tbody/tr[1]/td[2]")).Text;

                                    string Pa1 = driver.FindElement(By.XPath("//*[@id='body']/table[2]/tbody/tr[2]/td/table/tbody/tr[1]/td[4]")).Text;

                                    string a1 = Pa1.Substring(0, 4);
                                    string b1 = Pa1.Substring(5, 5);
                                    string c1 = Pa1.Substring(11, 5);

                                    Pa1 = a1 + b1 + c1;
                                    parcelNumber = Dis1 + "-" + mp1 + "-" + Pa1;

                                    TaxPyrName = driver.FindElement(By.XPath("//*[@id='body']/table[2]/tbody/tr[1]/td[1]/table/tbody/tr[1]/td[2]")).Text;
                                    Address = driver.FindElement(By.XPath("//*[@id='body']/table[2]/tbody/tr[1]/td[1]/table/tbody/tr[3]/td[2]")).Text;
                                    Type = "RE";

                                    string MultiAccount_details = Type + "~" + TaxPyrName + "~" + Address;
                                    gc.insert_date(orderno, parcelNumber, 573, MultiAccount_details, 1, DateTime.Now);
                                }
                                maxCheck++;
                            }

                            if (AccountTR.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Berkeley_Multicount"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                            else if(AccountTR.Count <= 25 && AccountTR.Count > 1)
                            {
                                HttpContext.Current.Session["multiparcel_Berkeley"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                        }
                        catch { }
                    }

                    Thread.Sleep(2000);
                    driver.SwitchTo().Window(driver.WindowHandles.Last());
                    Thread.Sleep(3000);

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.XPath("//*[@id='inside']/div/div[1]/table[1]"));
                        IList<IWebElement> INodataRow = INodata.FindElements(By.TagName("td"));
                        foreach (IWebElement data in INodataRow)
                        {
                            if (data.Text.Contains("No matches found."))
                            {
                                HttpContext.Current.Session["Nodata_WVBerkeley"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        if (INodataRow.Count <= 1)
                        {
                            HttpContext.Current.Session["Nodata_WVBerkeley"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    //Property Details
                    try
                    {
                        Account_Number = driver.FindElement(By.XPath("//*[@id='body']/table[1]/tbody/tr[1]/td[4]")).Text;
                        Pro_District = driver.FindElement(By.XPath("//*[@id='body']/table[1]/tbody/tr[2]/td[2]")).Text;
                        Owner_Name = driver.FindElement(By.XPath("//*[@id='body']/table[2]/tbody/tr[1]/td[1]/table/tbody/tr[1]/td[2]")).Text;
                        Pro_Address = driver.FindElement(By.XPath("//*[@id='body']/table[2]/tbody/tr[1]/td[1]/table/tbody/tr[3]/td[2]")).Text;
                        Legel_Desp1 = driver.FindElement(By.XPath("//*[@id='body']/table[2]/tbody/tr[1]/td[2]/table/tbody/tr[1]/td[2]")).Text;
                        Legel_Desp2 = driver.FindElement(By.XPath("//*[@id='body']/table[2]/tbody/tr[1]/td[2]/table/tbody/tr[2]/td")).Text;
                        Legel_Desp = Legel_Desp1 + Legel_Desp2;
                        Pro_Map = driver.FindElement(By.XPath("//*[@id='body']/table[2]/tbody/tr[2]/td/table/tbody/tr[1]/td[2]")).Text;

                        Property_Details = Account_Number + "~" + Pro_District + "~" + Pro_Map + "~" + Owner_Name + "~" + Pro_Address + "~" + Legel_Desp;
                        gc.CreatePdf(orderno, parcelNumber, "Property Details", driver, "WV", "Berkeley");
                        gc.insert_date(orderno, parcelNumber, 569, Property_Details, 1, DateTime.Now);
                    }
                    catch
                    { }

                    //Assessment Details
                    try
                    {
                        IWebElement PropertyTB = driver.FindElement(By.XPath("//*[@id='body']/table[3]/tbody/tr/td[1]/table/tbody"));
                        IList<IWebElement> PropertyTR = PropertyTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> PropertyTD;

                        foreach (IWebElement Property in PropertyTR)
                        {
                            PropertyTD = Property.FindElements(By.TagName("td"));
                            if (PropertyTD.Count != 0 && !Property.Text.Contains("Assessment"))
                            {
                                Assessment = PropertyTD[0].Text;
                                Gross = PropertyTD[1].Text;
                                Net = PropertyTD[2].Text;

                                Assessment_details = Assessment + "~" + Gross + "~" + Net;
                                gc.insert_date(orderno, parcelNumber, 570, Assessment_details, 1, DateTime.Now);
                            }
                        }
                    }
                    catch
                    { }
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderno, "WV", "Berkeley", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderno, "WV", "Berkeley");
                    return "Data Inserted Successfully";
                }
                catch (Exception ex)
                {
                    driver.Quit();
                    GlobalClass.LogError(ex, orderno);
                    throw;
                }
            }
        }
    }
}