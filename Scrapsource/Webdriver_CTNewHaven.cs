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
using System.Web.UI;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace ScrapMaricopa.Scrapsource
{
    public class Webdriver_CTNewHaven
    {
        string Parcelhref = "";
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        string msg;
        IWebElement addclick;
        IWebElement multitableElement1;
        string multiparceldata = "";
        string countyname = "";
        string uniqueidMap = "";
        string streetno1 = "", streetname1 = "", Acres = "";
        string urlAssess = "", urlTax = "", countAssess = "", countTax = "", taxCollectorlink = "";
        int countmulti;

        public string FTP_CTNewHaven(string streetno, string streetname, string streetdir, string streettype, string assessment_id, string parcelNumber, string searchType, string orderNumber, string directParcel, string ownername, string countynameCT, string statecountyid, string township, string townshipcode)
        {
            IWebDriver driver;
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            int duecount = 0;
            string hrefCardlink = "", hrefparcellink = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            // driver = new ChromeDriver();
            //driver = new PhantomJSDriver()
            DBconnection dbconn = new DBconnection();
            var chromeOptions1 = new ChromeOptions();
            var chromedriver = new ChromeDriver(chromeOptions1);
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    // var townshipcode1 = new List<string> {"02","03","05","08","09","11","12","14","15","16","18","19","20","23","26","27","29","31","32"};

                    CT_Link linkct = new Scrapsource.CT_Link();
                    string[] urllink = linkct.link(townshipcode, township, countynameCT);

                    urlAssess = urllink[0];
                    urlTax = urllink[1];
                    countAssess = urllink[2];
                    countTax = urllink[3];
                    taxCollectorlink = urllink[4];
                    HttpContext.Current.Session["linkNoAssess"] = countAssess;
                    HttpContext.Current.Session["linkNoTax"] = countTax;
                    if (countAssess == "No Tax")
                    {
                        HttpContext.Current.Session["NoTax_CT" + countynameCT] = "No_Tax";
                        driver.Quit();
                        return "Taxes Not Available";
                    }
                    driver.Navigate().GoToUrl(urlAssess);
                    string address = "";
                    if (streetdir != "")
                    {
                        address = streetno + " " + streetdir + " " + streetname + " " + streettype + " " + assessment_id;
                    }
                    else
                    {
                        address = streetno + " " + streetname + " " + streettype + " " + assessment_id;
                    }
                    Thread.Sleep(2000);

                    #region address search
                    if (searchType == "address")
                    {
                        if (countAssess == "titleflex")
                        {
                            searchType = "titleflex";
                        }
                        if (countAssess == "0")//Bridgeport
                        {
                            IWebElement IAddressSelect = driver.FindElement(By.Id("MainContent_ddlSearchSource"));
                            SelectElement sAddressSelect = new SelectElement(IAddressSelect);
                            sAddressSelect.SelectByText("Address");
                            driver.FindElement(By.Id("MainContent_txtSearchAddress")).SendKeys(address.Trim());
                            gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "CT", countynameCT);
                            IWebElement parceldata1 = driver.FindElement(By.XPath("//*[@id='SearchAll']/span[7]"));
                            IJavaScriptExecutor js11 = driver as IJavaScriptExecutor;
                            js11.ExecuteScript("arguments[0].click();", parceldata1);
                            Thread.Sleep(5000);
                            try
                            {
                                string nodata = driver.FindElement(By.XPath("//*[@id='MainContent_grdSearchResults']")).Text;
                                if (nodata.Contains("No Data for Current Search"))
                                {
                                    HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }
                            try
                            {
                                IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='MainContent_grdSearchResults']/tbody"));
                                IList<IWebElement> TRmultiaddress = multiaddress.FindElements(By.TagName("tr"));
                                IList<IWebElement> TDmultiaddress;
                                if (TRmultiaddress.Count <= 2)
                                {
                                    IWebElement parceldata = driver.FindElement(By.XPath("//*[@id='MainContent_grdSearchResults']/tbody/tr[2]/td[1]/a"));
                                    IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                                    js1.ExecuteScript("arguments[0].click();", parceldata);
                                    Thread.Sleep(5000);
                                }
                                if (TRmultiaddress.Count > 28)
                                {
                                    HttpContext.Current.Session["multiParcel_Multicount_CT" + countynameCT + township] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }
                                if (TRmultiaddress.Count > 2 && TRmultiaddress.Count < 28)
                                {

                                    foreach (IWebElement row in TRmultiaddress)
                                    {
                                        TDmultiaddress = row.FindElements(By.TagName("td"));
                                        if (TDmultiaddress.Count == 7 && !row.Text.Contains("Address") && !row.Text.Contains("Results"))
                                        {
                                            //Address~Owner~Account ID~PID
                                            multiparceldata = "Address~Owner~Account ID~MBLU";
                                            string Multi = TDmultiaddress[0].Text + "~" + TDmultiaddress[1].Text + "~" + TDmultiaddress[2].Text + "~" + TDmultiaddress[3].Text + "-" + TDmultiaddress[4].Text + "-" + TDmultiaddress[5].Text;
                                            gc.insert_date(orderNumber, TDmultiaddress[6].Text, 2185, Multi, 1, DateTime.Now);
                                        }
                                        if (TDmultiaddress.Count == 8 && !row.Text.Contains("Address") && !row.Text.Contains("Results"))
                                        {
                                            //Address~Owner~Account ID~PID
                                            multiparceldata = "Address~Owner~Account ID~MBLU";
                                            string Multi = TDmultiaddress[0].Text + "~" + TDmultiaddress[1].Text + "~" + TDmultiaddress[2].Text + "~" + TDmultiaddress[3].Text + "-" + TDmultiaddress[4].Text + "-" + TDmultiaddress[5].Text + "-" + TDmultiaddress[6].Text;
                                            gc.insert_date(orderNumber, TDmultiaddress[7].Text, 2185, Multi, 1, DateTime.Now);
                                        }
                                        if (TDmultiaddress.Count == 10 && !row.Text.Contains("Address") && !row.Text.Contains("Results"))
                                        {
                                            //Address~Owner~Account ID~PID
                                            multiparceldata = "Address~Owner~Account ID~MBLU";
                                            string Multi = TDmultiaddress[0].Text + "~" + TDmultiaddress[1].Text + "~" + TDmultiaddress[2].Text + "~" + TDmultiaddress[3].Text + "-" + TDmultiaddress[4].Text + "-" + TDmultiaddress[5].Text + "-" + TDmultiaddress[6].Text + TDmultiaddress[7].Text + "-" + TDmultiaddress[8].Text;
                                            gc.insert_date(orderNumber, TDmultiaddress[9].Text, 2185, Multi, 1, DateTime.Now);
                                        }


                                        if (TDmultiaddress.Count == 11 && !row.Text.Contains("Address") && !row.Text.Contains("Results"))
                                        {
                                            //Address~Owner~Account ID~PID
                                            multiparceldata = "Address~Owner~Account ID~MBLU";
                                            string Multi = TDmultiaddress[0].Text + "~" + TDmultiaddress[1].Text + "~" + TDmultiaddress[2].Text + "~" + TDmultiaddress[3].Text + "-" + TDmultiaddress[4].Text + "-" + TDmultiaddress[5].Text + "-" + TDmultiaddress[6].Text + TDmultiaddress[7].Text + "-" + TDmultiaddress[8].Text + "-" + TDmultiaddress[9].Text;
                                            gc.insert_date(orderNumber, TDmultiaddress[10].Text, 2185, Multi, 1, DateTime.Now);
                                        }
                                        if (TDmultiaddress.Count == 12 && !row.Text.Contains("Address") && !row.Text.Contains("Results"))
                                        {
                                            //Address~Owner~Account ID~PID
                                            multiparceldata = "Address~Owner~Account ID~MBLU";
                                            string Multi = TDmultiaddress[0].Text + "~" + TDmultiaddress[1].Text + "~" + TDmultiaddress[2].Text + "~" + TDmultiaddress[3].Text + "-" + TDmultiaddress[4].Text + "-" + TDmultiaddress[5].Text + "-" + TDmultiaddress[6].Text + TDmultiaddress[7].Text + "-" + TDmultiaddress[8].Text + "-" + TDmultiaddress[9].Text + "-" + TDmultiaddress[10].Text;
                                            gc.insert_date(orderNumber, TDmultiaddress[11].Text, 2185, Multi, 1, DateTime.Now);
                                        }
                                        if (TDmultiaddress.Count == 9 && !row.Text.Contains("Address") && !row.Text.Contains("Results"))
                                        {
                                            //Address~Owner~Account ID~PID
                                            multiparceldata = "Address~Owner~Account ID~MBLU";
                                            string Multi = TDmultiaddress[0].Text + "~" + TDmultiaddress[1].Text + "~" + TDmultiaddress[2].Text + "~" + TDmultiaddress[3].Text + "-" + TDmultiaddress[4].Text + "-" + TDmultiaddress[5].Text + "-" + TDmultiaddress[6].Text + "-" + TDmultiaddress[7].Text;
                                            gc.insert_date(orderNumber, TDmultiaddress[8].Text, 2185, Multi, 1, DateTime.Now);
                                        }
                                    }
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2185'");

                                    gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "CT", countynameCT);
                                    HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                            }
                            catch { }

                        }
                        if (countAssess == "1")//Easton
                        {
                            driver.FindElement(By.Id("MainContent_tbPropertySearchStreetNumber")).SendKeys(streetno);
                            string street = streetname.ToUpper() + " " + streettype.ToUpper();
                            IWebElement ISearch = driver.FindElement(By.XPath("//*[@id='MainContent_cbPropertySearchStreetName_chzn']/a"));
                            Actions action = new Actions(driver);
                            action.MoveToElement(ISearch).Perform(); // move to the button
                            ISearch.Click();
                            IWebElement IStreetClick = driver.FindElement(By.XPath("//*[@id='MainContent_cbPropertySearchStreetName_chzn']/div"));
                            IList<IWebElement> IStreetClickRow = IStreetClick.FindElements(By.TagName("li"));
                            foreach (IWebElement streetClick in IStreetClickRow)
                            {
                                if (streetClick.Text.Trim() == street.Trim())
                                {
                                    streetClick.Click();
                                    break;
                                }
                            }
                            Thread.Sleep(3000);
                            try
                            {
                                driver.FindElement(By.Id("MainContent_tbPropertySearchStreetUnit")).SendKeys(assessment_id);
                            }
                            catch { }
                            gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "CT", countynameCT);
                            driver.FindElement(By.Id("MainContent_btnPropertySearch")).SendKeys(Keys.Enter);
                            Thread.Sleep(3000);
                            try
                            {
                                string nodata = driver.FindElement(By.XPath("//*[@id='dt_a']")).Text;
                                if (nodata.Contains("No data available"))
                                {
                                    HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }
                            try
                            {
                                IWebElement IPageSelect = driver.FindElement(By.XPath("//*[@id='dt_a_length']/label/select"));
                                SelectElement sPageSelect = new SelectElement(IPageSelect);
                                sPageSelect.SelectByText("25");
                                Thread.Sleep(2000);
                            }
                            catch { }

                            try
                            {
                                string strmulti = gc.Between(driver.FindElement(By.Id("dt_a_info")).Text, "of ", " entries").Trim();
                                IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='dt_a']/tbody"));
                                IList<IWebElement> TRmultiaddress = multiaddress.FindElements(By.TagName("tr"));
                                IList<IWebElement> TDmultiaddress;
                                if (TRmultiaddress.Count < 2 && Convert.ToInt32(strmulti) < 2)
                                {
                                    IWebElement parceldata = driver.FindElement(By.XPath("//*[@id='dt_a']/tbody/tr[1]/td[1]/a"));
                                    parceldata.Click();
                                    Thread.Sleep(1000);
                                }
                                if (TRmultiaddress.Count > 25 && Convert.ToInt32(strmulti) > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_Multicount_CT" + countynameCT + township] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }
                                if ((TRmultiaddress.Count > 1 && TRmultiaddress.Count < 28) && (Convert.ToInt32(strmulti) > 1 && Convert.ToInt32(strmulti) <= 25))
                                {
                                    foreach (IWebElement row in TRmultiaddress)
                                    {
                                        TDmultiaddress = row.FindElements(By.TagName("td"));
                                        if (TDmultiaddress.Count != 0 && !row.Text.Contains("Address") && !row.Text.Contains("Results"))
                                        {
                                            //Address~Owner~MBLU~Property Use
                                            string Multi = TDmultiaddress[1].Text + " " + TDmultiaddress[0].Text + " " + TDmultiaddress[2].Text + "~" + TDmultiaddress[3].Text + "~" + TDmultiaddress[5].Text + "~" + TDmultiaddress[6].Text;
                                            gc.insert_date(orderNumber, TDmultiaddress[4].Text, 2185, Multi, 1, DateTime.Now);
                                        }
                                    }
                                    multiparceldata = "Address~Owner~MBLU~Property Use";
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2185'");

                                    gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "CT", countynameCT);
                                    HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                            }
                            catch { }


                        }


                        if (countAssess == "5")//Burlington
                        {
                            if (townshipcode != "25")
                            {
                                try
                                {
                                    driver.ExecuteJavaScript("document.getElementById('houseno').setAttribute('value','" + streetno + "')");
                                    gc.CreatePdf_WOP(orderNumber, "Address Number", driver, "CT", countynameCT);
                                    string Addresshrf = "", mergetype = "", AddressCombain = "";
                                    if (streettype != "")
                                    {
                                        mergetype = streetname.Trim().ToUpper() + " " + streettype.Trim().ToUpper();
                                    }
                                    else
                                    {
                                        mergetype = streetname.Trim().ToUpper();
                                    }

                                    IWebElement select = driver.FindElement(By.Id("street"));
                                    ((IJavaScriptExecutor)driver).ExecuteScript("var select = arguments[0]; for(var i = 0; i < select.options.length; i++){ if(select.options[i].text == arguments[1]){ select.options[i].selected = true; } }", select, mergetype);
                                    gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "CT", countynameCT);

                                    IWebElement Iviewpay = driver.FindElement(By.Name("go"));
                                    IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                                    js1.ExecuteScript("arguments[0].click();", Iviewpay);
                                    Thread.Sleep(5000);

                                    // IWebElement iframeElement1 = driver.FindElement(By.XPath("//*[@id='body']"));
                                    driver.SwitchTo().Frame(0);

                                    try
                                    {
                                        string nodata = driver.FindElement(By.XPath("/html/body/div[2]")).Text;
                                        gc.CreatePdf_WOP(orderNumber, "No data", driver, "CT", countynameCT);
                                        if (nodata.Contains("No matching"))
                                        {
                                            HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                            driver.Quit();
                                            return "No Data Found";
                                        }
                                    }
                                    catch { }


                                    if (streetdir.Trim() == "")
                                    {
                                        AddressCombain = streetno.Trim() + " " + mergetype;
                                    }
                                    else
                                    {
                                        AddressCombain = streetno.Trim() + " " + streetdir.Trim() + " " + mergetype;
                                    }
                                    int Max = 0;
                                    string GisID = "", UniqueID = "", Ownername = "", Address = "";
                                    IWebElement Addresstable = driver.FindElement(By.XPath("/html/body/div[2]/table/tbody"));
                                    IList<IWebElement> Addresrow = Addresstable.FindElements(By.TagName("tr"));
                                    IList<IWebElement> AddressTD;
                                    gc.CreatePdf_WOP(orderNumber, "Address After", driver, "CT", countynameCT);
                                    foreach (IWebElement AddressT in Addresrow)
                                    {
                                        AddressTD = AddressT.FindElements(By.TagName("td"));
                                        if (AddressTD.Count > 1 && AddressTD[1].Text.Contains(AddressCombain.ToUpper()))
                                        {
                                            string[] Arrayaddress = AddressTD[1].Text.Split('\r');
                                            if (townshipcode == "01" || townshipcode == "06")
                                            {

                                                GisID = Arrayaddress[0];
                                                UniqueID = Arrayaddress[1].Replace("\n", "").Trim();
                                                Ownername = Arrayaddress[2].Replace("\n", "").Trim();
                                                Address = Arrayaddress[3].Replace("\n", "").Trim();
                                            }
                                            if (townshipcode == "03" || townshipcode == "14" || townshipcode == "25")
                                            {

                                                GisID = Arrayaddress[0];
                                                UniqueID = "";
                                                Ownername = Arrayaddress[1].Replace("\n", "").Trim();
                                                Address = Arrayaddress[2].Replace("\n", "").Trim();
                                            }
                                            IWebElement Parcellink = AddressTD[2].FindElement(By.TagName("a"));
                                            hrefCardlink = Parcellink.GetAttribute("href");
                                            if (townshipcode == "03")
                                            {
                                                IWebElement Parcellinkw = AddressTD[2].FindElement(By.LinkText("eQuality Card"));
                                                hrefparcellink = Parcellinkw.GetAttribute("href");
                                            }
                                            else if (townshipcode == "14" || townshipcode == "25")
                                            {
                                                IWebElement Parcellinkw = AddressTD[2].FindElement(By.LinkText("Property Card"));
                                                hrefparcellink = Parcellinkw.GetAttribute("href");
                                            }
                                            else
                                            {
                                                IWebElement Parcellinkw = AddressTD[2].FindElement(By.LinkText("Summary Card"));
                                                hrefparcellink = Parcellinkw.GetAttribute("href");

                                            }
                                            string Multiresult = Address + "~" + Ownername + "~" + UniqueID;
                                            gc.insert_date(orderNumber, GisID, 2185, Multiresult, 1, DateTime.Now);
                                            Max++;
                                            gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "CT", countynameCT);
                                        }

                                    }
                                    multiparceldata = "Address~Owner~Account Number";
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2185'");

                                    if (Max == 1)
                                    {
                                        driver.Navigate().GoToUrl(hrefCardlink);
                                        Thread.Sleep(5000);
                                    }
                                    if (Max > 1 && Max < 26)
                                    {
                                        HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                        driver.Quit();
                                        return "MultiParcel";
                                    }
                                    if (Max > 25)
                                    {
                                        HttpContext.Current.Session["multiParcel_Multicount_CT" + countynameCT + township] = "Maximum";
                                        driver.Quit();
                                        return "Maximum";
                                    }
                                    if (Max == 0)
                                    {
                                        HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                        driver.Quit();
                                        return "No Data Found";
                                    }
                                }
                                catch (Exception e)
                                { }
                            }
                            else
                            {
                                try
                                {

                                    chromedriver.Navigate().GoToUrl(urlAssess);
                                    chromedriver.ExecuteJavaScript("document.getElementById('houseno').setAttribute('value','" + streetno + "')");
                                    gc.CreatePdf_WOP(orderNumber, "Address Number", chromedriver, "CT", countynameCT);
                                    string Addresshrf = "", mergetype = "", AddressCombain = "";
                                    if (streettype != "")
                                    {
                                        mergetype = streetname.Trim().ToUpper() + " " + streettype.Trim().ToUpper();
                                    }
                                    else
                                    {
                                        mergetype = streetname.Trim().ToUpper();
                                    }

                                    IWebElement select = chromedriver.FindElement(By.Id("street"));
                                    ((IJavaScriptExecutor)chromedriver).ExecuteScript("var select = arguments[0]; for(var i = 0; i < select.options.length; i++){ if(select.options[i].text == arguments[1]){ select.options[i].selected = true; } }", select, mergetype);
                                    gc.CreatePdf_WOP(orderNumber, "Address Search", chromedriver, "CT", countynameCT);

                                    IWebElement Iviewpay = chromedriver.FindElement(By.Name("go"));
                                    IJavaScriptExecutor js1 = chromedriver as IJavaScriptExecutor;
                                    js1.ExecuteScript("arguments[0].click();", Iviewpay);
                                    Thread.Sleep(5000);

                                    // IWebElement iframeElement1 = driver.FindElement(By.XPath("//*[@id='body']"));
                                    chromedriver.SwitchTo().Frame(0);

                                    try
                                    {
                                        string nodata = driver.FindElement(By.XPath("/html/body/div[2]")).Text;
                                        gc.CreatePdf_WOP(orderNumber, "No data", chromedriver, "CT", countynameCT);
                                        if (nodata.Contains("No matching"))
                                        {
                                            HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                            chromedriver.Quit();
                                            return "No Data Found";
                                        }
                                    }
                                    catch { }


                                    if (streetdir.Trim() == "")
                                    {
                                        AddressCombain = streetno.Trim() + " " + mergetype;
                                    }
                                    else
                                    {
                                        AddressCombain = streetno.Trim() + " " + streetdir.Trim() + " " + mergetype;
                                    }
                                    int Max = 0;
                                    string GisID = "", UniqueID = "", Ownername = "", Address = "";
                                    IWebElement Addresstable = chromedriver.FindElement(By.XPath("/html/body/div[2]/table/tbody"));
                                    IList<IWebElement> Addresrow = Addresstable.FindElements(By.TagName("tr"));
                                    IList<IWebElement> AddressTD;
                                    gc.CreatePdf_WOP(orderNumber, "Address After", driver, "CT", countynameCT);
                                    foreach (IWebElement AddressT in Addresrow)
                                    {
                                        AddressTD = AddressT.FindElements(By.TagName("td"));
                                        if (AddressTD.Count > 1 && AddressTD[1].Text.Contains(AddressCombain.ToUpper()))
                                        {
                                            string[] Arrayaddress = AddressTD[1].Text.Split('\r');
                                            if (townshipcode == "01" || townshipcode == "06")
                                            {

                                                GisID = Arrayaddress[0];
                                                UniqueID = Arrayaddress[1].Replace("\n", "").Trim();
                                                Ownername = Arrayaddress[2].Replace("\n", "").Trim();
                                                Address = Arrayaddress[3].Replace("\n", "").Trim();
                                            }
                                            if (townshipcode == "03" || townshipcode == "14" || townshipcode == "25")
                                            {

                                                GisID = Arrayaddress[0];
                                                UniqueID = "";
                                                Ownername = Arrayaddress[1].Replace("\n", "").Trim();
                                                Address = Arrayaddress[2].Replace("\n", "").Trim();
                                            }
                                            IWebElement Parcellink = AddressTD[2].FindElement(By.TagName("a"));
                                            hrefCardlink = Parcellink.GetAttribute("href");
                                            if (townshipcode == "03")
                                            {
                                                IWebElement Parcellinkw = AddressTD[2].FindElement(By.LinkText("eQuality Card"));
                                                hrefparcellink = Parcellinkw.GetAttribute("href");
                                            }
                                            else if (townshipcode == "14" || townshipcode == "25")
                                            {
                                                IWebElement Parcellinkw = AddressTD[2].FindElement(By.LinkText("Property Card"));
                                                hrefparcellink = Parcellinkw.GetAttribute("href");
                                            }
                                            else
                                            {
                                                IWebElement Parcellinkw = AddressTD[2].FindElement(By.LinkText("Summary Card"));
                                                hrefparcellink = Parcellinkw.GetAttribute("href");

                                            }
                                            string Multiresult = Address + "~" + Ownername + "~" + UniqueID;
                                            gc.insert_date(orderNumber, GisID, 2185, Multiresult, 1, DateTime.Now);
                                            Max++;
                                            gc.CreatePdf_WOP(orderNumber, "Address Search Result", chromedriver, "CT", countynameCT);
                                        }

                                    }
                                    multiparceldata = "Address~Owner~Account Number";
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2185'");

                                    if (Max == 1)
                                    {
                                        chromedriver.Navigate().GoToUrl(hrefCardlink);
                                        Thread.Sleep(5000);
                                    }
                                    if (Max > 1 && Max < 26)
                                    {
                                        HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                        chromedriver.Quit();
                                        return "MultiParcel";
                                    }
                                    if (Max > 25)
                                    {
                                        HttpContext.Current.Session["multiParcel_Multicount_CT" + countynameCT + township] = "Maximum";
                                        chromedriver.Quit();
                                        return "Maximum";
                                    }
                                    if (Max == 0)
                                    {
                                        HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                        chromedriver.Quit();
                                        return "No Data Found";
                                    }
                                }
                                catch (Exception e)
                                { }
                            }

                        }

                        if (countAssess == "7")//Address
                        {
                            // string addresshref = "";
                            Thread.Sleep(3000);
                            if (streetdir == "")
                            {
                                address = streetno + " " + streetname + " " + streettype;
                                address = address.Trim();
                            }
                            else
                            {
                                address = streetno + " " + streetname + " " + streetdir + " " + streettype;
                                address = address.Trim();
                            }
                            address = address.Trim();


                            driver.FindElement(By.Id("col2_filter")).SendKeys(address);
                            Thread.Sleep(5000);
                            gc.CreatePdf_WOP(orderNumber, "Address search", driver, "CT", countynameCT);
                            int a = 0;
                            try
                            {
                                string Nodatafound = driver.FindElement(By.XPath("//*[@id='example']/tbody/tr/td")).Text;
                                if (Nodatafound.Contains("No matching records"))
                                {
                                    HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }
                            IWebElement Addressmultitable = driver.FindElement(By.Id("example"));
                            IList<IWebElement> AddressmutiRow = Addressmultitable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Addressmutiid;
                            if (AddressmutiRow.Count() == 2)
                            {
                                driver.FindElement(By.XPath("//*[@id='example']/tbody/tr")).Click();
                                Thread.Sleep(2000);
                            }
                            else
                            {
                                foreach (IWebElement addressmulti in AddressmutiRow)
                                {
                                    Addressmutiid = addressmulti.FindElements(By.TagName("td"));
                                    if (Addressmutiid.Count != 0)
                                    {

                                        string proprtyad = Addressmutiid[0].Text;
                                        string ownerMulti = Addressmutiid[1].Text;
                                        string parcelid = Addressmutiid[2].Text;
                                        string Multiaddress = proprtyad + "~" + ownerMulti;
                                        gc.insert_date(orderNumber, parcelid, 2185, Multiaddress, 1, DateTime.Now);
                                        a++;
                                    }
                                }
                                multiparceldata = "Property Location~Owner Name";
                                dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2185'");
                                if (a == 1)
                                {
                                    driver.FindElement(By.XPath("//*[@id='example']/tbody/tr")).Click();
                                    Thread.Sleep(2000);
                                }
                                if (a > 1 && a < 26)
                                {
                                    HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel"; ;
                                }
                                if (a > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_Multicount_CT" + countynameCT + township] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }
                            }

                        }
                        if (countAssess == "8")//Address
                        {
                            // driver.Navigate().GoToUrl(urlAssess);
                            driver.FindElement(By.Name("houseno")).SendKeys(streetno);

                            IWebElement Streetselect = driver.FindElement(By.Name("street"));
                            SelectElement sStreetselect = new SelectElement(Streetselect);
                            sStreetselect.SelectByValue(streetname.Trim().ToUpper() + " " + streettype.Trim().ToUpper());
                            gc.CreatePdf_WOP(orderNumber, "Address search", driver, "CT", countynameCT);
                            driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td[2]/table/tbody/tr/td/form[1]/input[2]")).Click();
                            Thread.Sleep(4000);
                            gc.CreatePdf_WOP(orderNumber, "Address search Result", driver, "CT", countynameCT);
                            int a = 0;
                            IWebElement Addressmultitable = driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td[2]/div[1]/table/tbody"));
                            IList<IWebElement> AddressmutiRow = Addressmultitable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Addressmutiid;
                            foreach (IWebElement addressmulti in AddressmutiRow)
                            {
                                Addressmutiid = addressmulti.FindElements(By.TagName("td"));
                                if (Addressmutiid.Count > 1 && !addressmulti.Text.Contains("Parcel No"))
                                {
                                    if (Addressmutiid[2].Text.Trim() == streetno && Addressmutiid[3].Text.Trim().Contains(streetname.Trim().ToUpper() + " " + streettype.Trim().ToUpper()))
                                    {
                                        IWebElement ParcelNolink = Addressmutiid[0].FindElement(By.TagName("a"));
                                        Parcelhref = ParcelNolink.GetAttribute("href");
                                        string proprtyad = Addressmutiid[0].Text;
                                        string ownerMulti = Addressmutiid[1].Text;
                                        string parcelid = Addressmutiid[2].Text;
                                        string Multiaddress = proprtyad + "~" + ownerMulti;
                                        gc.insert_date(orderNumber, parcelid, 2185, Multiaddress, 1, DateTime.Now);
                                        a++;
                                    }
                                }
                            }
                            multiparceldata = "Property Location~Owner Name";
                            dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2185'");
                            if (a == 0)
                            {
                                HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                            if (a == 1)
                            {
                                driver.Navigate().GoToUrl(Parcelhref);
                                Thread.Sleep(2000);
                            }
                            if (a > 1 && a < 26)
                            {
                                HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                driver.Quit();
                                return "MultiParcel"; ;
                            }
                            if (a > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Multicount_CT" + countynameCT + township] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }

                        if (countAssess == "14")//Address
                        {
                            try
                            {
                                driver.FindElement(By.XPath("/html/body/div[2]/div[3]/div/button[1]")).Click();
                                Thread.Sleep(2000);
                            }
                            catch { }
                            driver.FindElement(By.Id("MainContent_txtHouseNumber")).SendKeys(streetno);
                            driver.FindElement(By.Id("MainContent_txtStreetName")).SendKeys(streetname.Trim().ToUpper() + " " + streettype.Trim().ToUpper());
                            gc.CreatePdf_WOP(orderNumber, "Address search", driver, "CT", countynameCT);
                            driver.FindElement(By.Id("MainContent_btnSearchAddress")).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf_WOP(orderNumber, "Address search After", driver, "CT", countynameCT);
                            int a = 0;
                            IWebElement Addressmultitable = driver.FindElement(By.Id("MainContent_GridView1"));
                            IList<IWebElement> AddressmutiRow = Addressmultitable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Addressmutiid;
                            foreach (IWebElement addressmulti in AddressmutiRow)
                            {
                                Addressmutiid = addressmulti.FindElements(By.TagName("td"));
                                if (Addressmutiid.Count > 1 && !addressmulti.Text.Contains("Property") && !addressmulti.Text.Contains("Nothing found"))
                                {
                                    IWebElement ParcelNolink = Addressmutiid[2].FindElement(By.TagName("a"));
                                    Parcelhref = ParcelNolink.GetAttribute("href");
                                    string proprtyad = Addressmutiid[0].Text;
                                    string ownerMulti = Addressmutiid[1].Text;
                                    //string parcelid = Addressmutiid[2].Text;
                                    string Multiaddress = proprtyad + "~" + ownerMulti;
                                    gc.insert_date(orderNumber, "", 2185, Multiaddress, 1, DateTime.Now);
                                    a++;
                                }
                            }
                            multiparceldata = "Property Location~Owner Name";
                            dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2185'");
                            if (a == 1)
                            {
                                driver.Navigate().GoToUrl(Parcelhref);
                                Thread.Sleep(2000);
                            }
                            if (a > 1 && a < 26)
                            {
                                HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                driver.Quit();
                                return "MultiParcel"; ;
                            }
                            if (a > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Multicount_CT" + countynameCT + township] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                            if (a == 0)
                            {
                                HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }

                        if (countAssess == "titleflex")
                        {
                            searchType = "titleflex";
                        }

                    }
                    #endregion
                    #region Owner search
                    if (searchType == "ownername")
                    {
                        if (countAssess == "titleflex")
                        {
                            searchType = "titleflex";
                        }
                        if (townshipcode == "")//Ridgefield and Monroe
                        {
                            HttpContext.Current.Session["Owner_CT" + countynameCT + township] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        if (countAssess == "0")//Bridgeport
                        {
                            IWebElement IOwnerSelect = driver.FindElement(By.Id("MainContent_ddlSearchSource"));
                            SelectElement sOwnerSelect = new SelectElement(IOwnerSelect);
                            sOwnerSelect.SelectByText("Owner");
                            driver.FindElement(By.Id("MainContent_txtSearchOwner")).SendKeys(ownername);
                            gc.CreatePdf_WOP(orderNumber, "Owner Search", driver, "CT", countynameCT);
                            IWebElement Iowner = driver.FindElement(By.XPath("//*[@id='SearchAll']/span[7]/i"));
                            IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                            js.ExecuteScript("arguments[0].click();", Iowner);
                            try
                            {
                                string nodata = driver.FindElement(By.XPath("//*[@id='MainContent_grdSearchResults']")).Text;
                                if (nodata.Contains("No Data for Current Search"))
                                {
                                    HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }
                            try
                            {
                                IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='MainContent_grdSearchResults']/tbody"));
                                IList<IWebElement> TRmultiaddress = multiaddress.FindElements(By.TagName("tr"));
                                IList<IWebElement> TDmultiaddress;
                                if (TRmultiaddress.Count <= 2)
                                {
                                    IWebElement parceldata = driver.FindElement(By.XPath("//*[@id='MainContent_grdSearchResults']/tbody/tr[2]/td[1]/a"));
                                    parceldata.Click();
                                    Thread.Sleep(1000);
                                }
                                if (TRmultiaddress.Count > 28)
                                {
                                    HttpContext.Current.Session["multiParcel_Multicount_CT" + countynameCT + township] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }
                                if (TRmultiaddress.Count > 2 && TRmultiaddress.Count < 28)
                                {

                                    foreach (IWebElement row in TRmultiaddress)
                                    {
                                        TDmultiaddress = row.FindElements(By.TagName("td"));
                                        if (TDmultiaddress.Count == 7 && !row.Text.Contains("Address") && !row.Text.Contains("Results"))
                                        {
                                            //Address~Owner~Account ID~PID
                                            multiparceldata = "Address~Owner~Account ID~MBLU";
                                            string Multi = TDmultiaddress[0].Text + "~" + TDmultiaddress[1].Text + "~" + TDmultiaddress[2].Text + "~" + TDmultiaddress[3].Text + "-" + TDmultiaddress[4].Text + "-" + TDmultiaddress[5].Text;
                                            gc.insert_date(orderNumber, TDmultiaddress[6].Text, 2185, Multi, 1, DateTime.Now);
                                        }
                                        if (TDmultiaddress.Count == 8 && !row.Text.Contains("Address") && !row.Text.Contains("Results"))
                                        {
                                            //Address~Owner~Account ID~PID
                                            multiparceldata = "Address~Owner~Account ID~MBLU";
                                            string Multi = TDmultiaddress[0].Text + "~" + TDmultiaddress[1].Text + "~" + TDmultiaddress[2].Text + "~" + TDmultiaddress[3].Text + "-" + TDmultiaddress[4].Text + "-" + TDmultiaddress[5].Text + "-" + TDmultiaddress[6].Text;
                                            gc.insert_date(orderNumber, TDmultiaddress[7].Text, 2185, Multi, 1, DateTime.Now);
                                        }
                                        if (TDmultiaddress.Count == 10 && !row.Text.Contains("Address") && !row.Text.Contains("Results"))
                                        {
                                            //Address~Owner~Account ID~PID
                                            multiparceldata = "Address~Owner~Account ID~MBLU";
                                            string Multi = TDmultiaddress[0].Text + "~" + TDmultiaddress[1].Text + "~" + TDmultiaddress[2].Text + "~" + TDmultiaddress[3].Text + "-" + TDmultiaddress[4].Text + "-" + TDmultiaddress[5].Text + "-" + TDmultiaddress[6].Text + TDmultiaddress[7].Text + "-" + TDmultiaddress[8].Text;
                                            gc.insert_date(orderNumber, TDmultiaddress[9].Text, 2185, Multi, 1, DateTime.Now);
                                        }


                                        if (TDmultiaddress.Count == 11 && !row.Text.Contains("Address") && !row.Text.Contains("Results"))
                                        {
                                            //Address~Owner~Account ID~PID
                                            multiparceldata = "Address~Owner~Account ID~MBLU";
                                            string Multi = TDmultiaddress[0].Text + "~" + TDmultiaddress[1].Text + "~" + TDmultiaddress[2].Text + "~" + TDmultiaddress[3].Text + "-" + TDmultiaddress[4].Text + "-" + TDmultiaddress[5].Text + "-" + TDmultiaddress[6].Text + TDmultiaddress[7].Text + "-" + TDmultiaddress[8].Text + "-" + TDmultiaddress[9].Text;
                                            gc.insert_date(orderNumber, TDmultiaddress[10].Text, 2185, Multi, 1, DateTime.Now);
                                        }
                                        if (TDmultiaddress.Count == 12 && !row.Text.Contains("Address") && !row.Text.Contains("Results"))
                                        {
                                            //Address~Owner~Account ID~PID
                                            multiparceldata = "Address~Owner~Account ID~MBLU";
                                            string Multi = TDmultiaddress[0].Text + "~" + TDmultiaddress[1].Text + "~" + TDmultiaddress[2].Text + "~" + TDmultiaddress[3].Text + "-" + TDmultiaddress[4].Text + "-" + TDmultiaddress[5].Text + "-" + TDmultiaddress[6].Text + TDmultiaddress[7].Text + "-" + TDmultiaddress[8].Text + "-" + TDmultiaddress[9].Text + "-" + TDmultiaddress[10].Text;
                                            gc.insert_date(orderNumber, TDmultiaddress[11].Text, 2185, Multi, 1, DateTime.Now);
                                        }
                                        if (TDmultiaddress.Count == 9 && !row.Text.Contains("Address") && !row.Text.Contains("Results"))
                                        {
                                            //Address~Owner~Account ID~PID
                                            multiparceldata = "Address~Owner~Account ID~MBLU";
                                            string Multi = TDmultiaddress[0].Text + "~" + TDmultiaddress[1].Text + "~" + TDmultiaddress[2].Text + "~" + TDmultiaddress[3].Text + "-" + TDmultiaddress[4].Text + "-" + TDmultiaddress[5].Text + "-" + TDmultiaddress[6].Text + "-" + TDmultiaddress[7].Text;
                                            gc.insert_date(orderNumber, TDmultiaddress[8].Text, 2185, Multi, 1, DateTime.Now);
                                        }
                                    }
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2185'");

                                    gc.CreatePdf_WOP(orderNumber, "Owner Search Result", driver, "CT", countynameCT);
                                    HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                            }
                            catch { }
                        }
                        if (countAssess == "1")//Easton
                        {
                            driver.FindElement(By.Id("MainContent_tbPropertySearchName")).SendKeys(ownername);
                            //string street = streetname.ToUpper() + " " + streettype.ToUpper();
                            //IWebElement ISearch = driver.FindElement(By.XPath("//*[@id='MainContent_cbPropertySearchStreetName_chzn']/a"));
                            //Actions action = new Actions(driver);
                            //action.MoveToElement(ISearch).Perform(); // move to the button
                            //ISearch.Click();

                            gc.CreatePdf_WOP(orderNumber, "Owner Search", driver, "CT", countynameCT);
                            driver.FindElement(By.Id("MainContent_btnPropertySearch")).SendKeys(Keys.Enter);
                            Thread.Sleep(3000);
                            try
                            {
                                string nodata = driver.FindElement(By.XPath("//*[@id='dt_a']")).Text;
                                if (nodata.Contains("No data available"))
                                {
                                    HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }
                            try
                            {
                                IWebElement IPageSelect = driver.FindElement(By.XPath("//*[@id='dt_a_length']/label/select"));
                                SelectElement sPageSelect = new SelectElement(IPageSelect);
                                sPageSelect.SelectByText("25");
                                Thread.Sleep(2000);
                            }
                            catch { }

                            try
                            {
                                string strmulti = gc.Between(driver.FindElement(By.Id("dt_a_info")).Text, "of ", " entries").Trim();
                                IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='dt_a']/tbody"));
                                IList<IWebElement> TRmultiaddress = multiaddress.FindElements(By.TagName("tr"));
                                IList<IWebElement> TDmultiaddress;
                                if (TRmultiaddress.Count < 2 && Convert.ToInt32(strmulti) < 2)
                                {
                                    IWebElement parceldata = driver.FindElement(By.XPath("//*[@id='dt_a']/tbody/tr[1]/td[1]/a"));
                                    parceldata.Click();
                                    Thread.Sleep(1000);
                                }
                                if (TRmultiaddress.Count > 25 && Convert.ToInt32(strmulti) > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_Multicount_CT" + countynameCT + township] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }
                                if ((TRmultiaddress.Count > 1 && TRmultiaddress.Count < 28) && (Convert.ToInt32(strmulti) > 1 && Convert.ToInt32(strmulti) <= 25))
                                {
                                    foreach (IWebElement row in TRmultiaddress)
                                    {
                                        TDmultiaddress = row.FindElements(By.TagName("td"));
                                        if (TDmultiaddress.Count != 0 && !row.Text.Contains("Address") && !row.Text.Contains("Results"))
                                        {
                                            //Address~Owner~MBLU~Property Use
                                            string Multi = TDmultiaddress[1].Text + " " + TDmultiaddress[0].Text + " " + TDmultiaddress[2].Text + "~" + TDmultiaddress[3].Text + "~" + TDmultiaddress[5].Text + "~" + TDmultiaddress[6].Text;
                                            gc.insert_date(orderNumber, TDmultiaddress[4].Text, 2185, Multi, 1, DateTime.Now);
                                        }
                                    }
                                    multiparceldata = "Address~Owner~MBLU~Property Use";
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2185'");

                                    gc.CreatePdf_WOP(orderNumber, "Owner Search Result", driver, "CT", countynameCT);
                                    HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                            }
                            catch { }
                        }


                        if (countAssess == "5")//Burlington
                        {
                            if (townshipcode != "25")
                            {
                                try
                                {
                                    driver.ExecuteJavaScript("document.getElementById('searchname').setAttribute('value','" + ownername + "')");
                                    gc.CreatePdf_WOP(orderNumber, "OwnerName Search", driver, "CT", countynameCT);
                                    IWebElement Iviewpay = driver.FindElement(By.Name("go"));
                                    IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                                    js1.ExecuteScript("arguments[0].click();", Iviewpay);
                                    Thread.Sleep(5000);
                                    gc.CreatePdf_WOP(orderNumber, "OwnerName Search After", driver, "CT", countynameCT);
                                    // IWebElement iframeElement1 = driver.FindElement(By.XPath("//*[@id='body']"));
                                    driver.SwitchTo().Frame(0);

                                    try
                                    {
                                        string nodata = driver.FindElement(By.XPath("/html/body/div[2]")).Text;
                                        if (nodata.Contains("No matching"))
                                        {
                                            gc.CreatePdf_WOP(orderNumber, "OwnerName Nodata", driver, "CT", countynameCT);
                                            HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                            driver.Quit();
                                            return "No Data Found";
                                        }
                                    }
                                    catch { }
                                    int Max = 0;


                                    string GisID = "", UniqueID = "", Ownername = "", Address = "";
                                    IWebElement Addresstable = driver.FindElement(By.XPath("/html/body/div[2]/table/tbody"));
                                    IList<IWebElement> Addresrow = Addresstable.FindElements(By.TagName("tr"));
                                    IList<IWebElement> AddressTD;
                                    gc.CreatePdf_WOP(orderNumber, "OwnerName Search After1", driver, "CT", countynameCT);
                                    foreach (IWebElement AddressT in Addresrow)
                                    {
                                        AddressTD = AddressT.FindElements(By.TagName("td"));
                                        if (AddressTD.Count > 1 && !AddressT.Text.Contains("Quick Links") && AddressT.Text.Trim() != "")
                                        {
                                            string[] Arrayaddress = AddressTD[1].Text.Split('\r');
                                            if (townshipcode == "01" || townshipcode == "06")
                                            {
                                                GisID = Arrayaddress[0];
                                                UniqueID = Arrayaddress[1].Replace("\n", "").Trim();
                                                Ownername = Arrayaddress[2].Replace("\n", "").Trim();
                                                Address = Arrayaddress[3].Replace("\n", "").Trim();
                                            }
                                            if (townshipcode == "03" || townshipcode == "14" || townshipcode == "25")
                                            {

                                                GisID = Arrayaddress[0];
                                                UniqueID = "";
                                                Ownername = Arrayaddress[1].Replace("\n", "").Trim();
                                                Address = Arrayaddress[2].Replace("\n", "").Trim();
                                            }
                                            IWebElement Parcellink = AddressTD[2].FindElement(By.TagName("a"));
                                            hrefCardlink = Parcellink.GetAttribute("href");
                                            if (townshipcode == "03")
                                            {
                                                IWebElement Parcellinkw = AddressTD[2].FindElement(By.LinkText("eQuality Card"));
                                                hrefparcellink = Parcellinkw.GetAttribute("href");
                                            }
                                            else if (townshipcode == "14" || townshipcode == "25")
                                            {
                                                IWebElement Parcellinkw = AddressTD[2].FindElement(By.LinkText("Property Card"));
                                                hrefparcellink = Parcellinkw.GetAttribute("href");
                                            }
                                            else
                                            {
                                                IWebElement Parcellinkw = AddressTD[2].FindElement(By.LinkText("Summary Card"));
                                                hrefparcellink = Parcellinkw.GetAttribute("href");
                                            }
                                            string Multiresult = Address + "~" + Ownername + "~" + UniqueID;
                                            gc.insert_date(orderNumber, GisID, 2185, Multiresult, 1, DateTime.Now);
                                            Max++;
                                            gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "CT", countynameCT);
                                        }

                                    }
                                    multiparceldata = "Address~Owner~Account Number";
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2185'");

                                    if (Max == 1)
                                    {
                                        driver.Navigate().GoToUrl(hrefCardlink);
                                        Thread.Sleep(5000);
                                    }
                                    if (Max > 1 && Max < 26)
                                    {
                                        HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                        driver.Quit();
                                        return "MultiParcel";
                                    }
                                    if (Max > 25)
                                    {
                                        HttpContext.Current.Session["multiParcel_Multicount_CT" + countynameCT + township] = "Maximum";
                                        driver.Quit();
                                        return "Maximum";
                                    }
                                    if (Max == 0)
                                    {
                                        HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                        driver.Quit();
                                        return "No Data Found";
                                    }
                                }
                                catch (Exception e)
                                { }
                            }
                            else
                            {
                                try
                                {
                                    chromedriver.Navigate().GoToUrl(urlAssess);
                                    chromedriver.ExecuteJavaScript("document.getElementById('searchname').setAttribute('value','" + ownername + "')");
                                    gc.CreatePdf_WOP(orderNumber, "OwnerName Search", chromedriver, "CT", countynameCT);
                                    IWebElement Iviewpay = chromedriver.FindElement(By.Name("go"));
                                    IJavaScriptExecutor js1 = chromedriver as IJavaScriptExecutor;
                                    js1.ExecuteScript("arguments[0].click();", Iviewpay);
                                    Thread.Sleep(5000);
                                    gc.CreatePdf_WOP(orderNumber, "OwnerName Search After", chromedriver, "CT", countynameCT);
                                    // IWebElement iframeElement1 = driver.FindElement(By.XPath("//*[@id='body']"));
                                    chromedriver.SwitchTo().Frame(0);

                                    try
                                    {
                                        string nodata = chromedriver.FindElement(By.XPath("/html/body/div[2]")).Text;
                                        if (nodata.Contains("No matching"))
                                        {
                                            gc.CreatePdf_WOP(orderNumber, "OwnerName Nodata", chromedriver, "CT", countynameCT);
                                            HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                            chromedriver.Quit();
                                            return "No Data Found";
                                        }
                                    }
                                    catch { }
                                    int Max = 0;


                                    string GisID = "", UniqueID = "", Ownername = "", Address = "";
                                    IWebElement Addresstable = chromedriver.FindElement(By.XPath("/html/body/div[2]/table/tbody"));
                                    IList<IWebElement> Addresrow = Addresstable.FindElements(By.TagName("tr"));
                                    IList<IWebElement> AddressTD;
                                    gc.CreatePdf_WOP(orderNumber, "OwnerName Search After1", chromedriver, "CT", countynameCT);
                                    foreach (IWebElement AddressT in Addresrow)
                                    {
                                        AddressTD = AddressT.FindElements(By.TagName("td"));
                                        if (AddressTD.Count > 1 && !AddressT.Text.Contains("Quick Links") && AddressT.Text.Trim() != "")
                                        {
                                            string[] Arrayaddress = AddressTD[1].Text.Split('\r');
                                            if (townshipcode == "01" || townshipcode == "06")
                                            {
                                                GisID = Arrayaddress[0];
                                                UniqueID = Arrayaddress[1].Replace("\n", "").Trim();
                                                Ownername = Arrayaddress[2].Replace("\n", "").Trim();
                                                Address = Arrayaddress[3].Replace("\n", "").Trim();
                                            }
                                            if (townshipcode == "03" || townshipcode == "14" || townshipcode == "25")
                                            {

                                                GisID = Arrayaddress[0];
                                                UniqueID = "";
                                                Ownername = Arrayaddress[1].Replace("\n", "").Trim();
                                                Address = Arrayaddress[2].Replace("\n", "").Trim();
                                            }
                                            IWebElement Parcellink = AddressTD[2].FindElement(By.TagName("a"));
                                            hrefCardlink = Parcellink.GetAttribute("href");
                                            if (townshipcode == "03")
                                            {
                                                IWebElement Parcellinkw = AddressTD[2].FindElement(By.LinkText("eQuality Card"));
                                                hrefparcellink = Parcellinkw.GetAttribute("href");
                                            }
                                            else if (townshipcode == "14" || townshipcode == "25")
                                            {
                                                IWebElement Parcellinkw = AddressTD[2].FindElement(By.LinkText("Property Card"));
                                                hrefparcellink = Parcellinkw.GetAttribute("href");
                                            }
                                            else
                                            {
                                                IWebElement Parcellinkw = AddressTD[2].FindElement(By.LinkText("Summary Card"));
                                                hrefparcellink = Parcellinkw.GetAttribute("href");
                                            }
                                            string Multiresult = Address + "~" + Ownername + "~" + UniqueID;
                                            gc.insert_date(orderNumber, GisID, 2185, Multiresult, 1, DateTime.Now);
                                            Max++;
                                            gc.CreatePdf_WOP(orderNumber, "Address Search Result", chromedriver, "CT", countynameCT);
                                        }

                                    }
                                    multiparceldata = "Address~Owner~Account Number";
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2185'");

                                    if (Max == 1)
                                    {
                                        chromedriver.Navigate().GoToUrl(hrefCardlink);
                                        Thread.Sleep(5000);
                                    }
                                    if (Max > 1 && Max < 26)
                                    {
                                        HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                        chromedriver.Quit();
                                        return "MultiParcel";
                                    }
                                    if (Max > 25)
                                    {
                                        HttpContext.Current.Session["multiParcel_Multicount_CT" + countynameCT + township] = "Maximum";
                                        chromedriver.Quit();
                                        return "Maximum";
                                    }
                                    if (Max == 0)
                                    {
                                        HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                        chromedriver.Quit();
                                        return "No Data Found";
                                    }
                                }
                                catch (Exception e)
                                { }
                            }

                        }
                        if (countAssess == "8")//ownername
                        {
                            // ByVisibleElement(driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td[2]/table/tbody/tr/td/p[5]")));
                            driver.FindElement(By.Name("owner")).SendKeys(ownername);
                            driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td[2]/table/tbody/tr/td/form[2]/div/input[2]")).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf_WOP(orderNumber, "Address search Result", driver, "CT", countynameCT);
                            int a = 0;
                            IWebElement Addressmultitable = driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td[2]/div[1]/table/tbody"));
                            IList<IWebElement> AddressmutiRow = Addressmultitable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Addressmutiid;
                            foreach (IWebElement addressmulti in AddressmutiRow)
                            {
                                Addressmutiid = addressmulti.FindElements(By.TagName("td"));
                                if (Addressmutiid.Count > 1 && !addressmulti.Text.Contains("Parcel No"))
                                {
                                    IWebElement ParcelNolink = Addressmutiid[0].FindElement(By.TagName("a"));
                                    Parcelhref = ParcelNolink.GetAttribute("href");
                                    string proprtyad = Addressmutiid[0].Text;
                                    string ownerMulti = Addressmutiid[1].Text;
                                    string parcelid = Addressmutiid[2].Text;
                                    string Multiaddress = proprtyad + "~" + ownerMulti;
                                    gc.insert_date(orderNumber, parcelid, 2185, Multiaddress, 1, DateTime.Now);
                                    a++;
                                }
                            }
                            multiparceldata = "Property Location~Owner Name";
                            dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2185'");
                            if (a == 1)
                            {
                                driver.Navigate().GoToUrl(Parcelhref);
                                Thread.Sleep(2000);
                            }
                            if (a > 1 && a < 26)
                            {
                                HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                driver.Quit();
                                return "MultiParcel"; ;
                            }
                            if (a > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Multicount_CT" + countynameCT + township] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        if (countAssess == "14")
                        {
                            HttpContext.Current.Session["Owner_CT" + countynameCT + township] = "Yes";
                            driver.Quit();
                            return "No Owner Search";
                        }
                        if (countAssess == "titleflex")
                        {
                            searchType = "titleflex";
                        }

                    }
                    #endregion

                    #region titleflex search
                    if (searchType == "titleflex")
                    {
                        try
                        {


                            string addresstitle = streetno + " " + streetname + " " + streettype;
                            gc.TitleFlexSearch(orderNumber, parcelNumber, "", addresstitle, "CT", countynameCT);
                            if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                            {
                                driver.Quit();
                                return "MultiParcel";
                            }
                            else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                            {
                                HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                            parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                            searchType = "parcel";


                        }
                        catch (Exception e)
                        { }
                    }
                    #endregion
                    #region parcel search
                    if (searchType == "parcel")
                    {
                        if (countAssess == "0")//Bridgeport
                        {
                            IWebElement IParcelSelect = driver.FindElement(By.Id("MainContent_ddlSearchSource"));
                            SelectElement sParcelSelect = new SelectElement(IParcelSelect);
                            sParcelSelect.SelectByText("PID");
                            driver.FindElement(By.Id("MainContent_txtSearchPid")).SendKeys(parcelNumber);
                            gc.CreatePdf_WOP(orderNumber, "Parcel Search", driver, "CT", countynameCT);
                            IWebElement Iowner = driver.FindElement(By.XPath("//*[@id='SearchAll']/span[7]/i"));
                            IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                            js.ExecuteScript("arguments[0].click();", Iowner);
                            // driver.FindElement(By.XPath("//*[@id='SearchAll']/span[7]/i")).SendKeys(Keys.Enter);
                            Thread.Sleep(1000);
                            try
                            {
                                string nodata = driver.FindElement(By.XPath("//*[@id='MainContent_grdSearchResults']")).Text;
                                if (nodata.Contains("No Data for Current Search"))
                                {
                                    HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }

                            IWebElement parceldata = driver.FindElement(By.XPath("//*[@id='MainContent_grdSearchResults']/tbody/tr[2]/td[1]/a"));
                            parceldata.Click();
                            Thread.Sleep(1000);
                        }
                        if (countAssess == "1")//Easton
                        {
                            driver.FindElement(By.Id("MainContent_tbPropertySearchUniqueId")).SendKeys(parcelNumber);
                            gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "CT", countynameCT);
                            driver.FindElement(By.Id("MainContent_btnPropertySearch")).SendKeys(Keys.Enter);
                            Thread.Sleep(3000);

                            try
                            {
                                string nodata = driver.FindElement(By.XPath("/html/body")).Text;
                                if (nodata.Contains("No matching results"))
                                {
                                    HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }

                            IWebElement parceldata = driver.FindElement(By.XPath("//*[@id='dt_a']/tbody/tr/td[1]/a"));
                            parceldata.Click();
                            Thread.Sleep(1000);
                        }
                        if (countAssess == "2")
                        {
                            driver.FindElement(By.Id("MainContent_tbPropertySearchUniqueId")).SendKeys(parcelNumber);
                            gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "CT", countynameCT);
                            driver.FindElement(By.Id("MainContent_btnPropertySearch")).SendKeys(Keys.Enter);
                            Thread.Sleep(3000);

                            try
                            {
                                string nodata = driver.FindElement(By.XPath("/html/body")).Text;
                                if (nodata.Contains("No matching results"))
                                {
                                    HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }

                            IWebElement parceldata = driver.FindElement(By.XPath("//*[@id='dt_a']/tbody/tr/td[1]/a"));
                            parceldata.Click();
                            Thread.Sleep(1000);
                        }

                        if (countAssess == "5")
                        {
                            if (townshipcode != "25")
                            {
                                try
                                {
                                    driver.ExecuteJavaScript("document.getElementById('mbl').setAttribute('value','" + parcelNumber + "')");
                                    gc.CreatePdf_WOP(orderNumber, "Parcel Search ", driver, "CT", countynameCT);
                                    IWebElement Iviewpay = driver.FindElement(By.Name("go"));
                                    IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                                    js1.ExecuteScript("arguments[0].click();", Iviewpay);
                                    Thread.Sleep(5000);
                                    gc.CreatePdf_WOP(orderNumber, "Parcel Search After", driver, "CT", countynameCT);
                                    // IWebElement iframeElement1 = driver.FindElement(By.XPath("//*[@id='body']"));
                                    driver.SwitchTo().Frame(0);

                                    try
                                    {
                                        string nodata = driver.FindElement(By.XPath("/html/body/div[2]")).Text;
                                        if (nodata.Contains("No matching"))
                                        {
                                            gc.CreatePdf_WOP(orderNumber, "Parcel Nodata", driver, "CT", countynameCT);
                                            HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                            driver.Quit();
                                            return "No Data Found";
                                        }
                                    }
                                    catch { }
                                    int Max = 0;


                                    string GisID = "", UniqueID = "", Ownername = "", Address = "";
                                    IWebElement Addresstable = driver.FindElement(By.XPath("/html/body/div[2]/table/tbody"));
                                    IList<IWebElement> Addresrow = Addresstable.FindElements(By.TagName("tr"));
                                    IList<IWebElement> AddressTD;
                                    gc.CreatePdf_WOP(orderNumber, "Parcel Search After1", driver, "CT", countynameCT);
                                    foreach (IWebElement AddressT in Addresrow)
                                    {
                                        AddressTD = AddressT.FindElements(By.TagName("td"));
                                        if (AddressTD.Count > 2 && !AddressT.Text.Contains("Quick Links") && AddressT.Text.Trim() != "")
                                        {
                                            string[] Arrayaddress = AddressTD[1].Text.Split('\r');
                                            if (townshipcode == "01" || townshipcode == "06")
                                            {

                                                GisID = Arrayaddress[0];
                                                UniqueID = Arrayaddress[1].Replace("\n", "").Trim();
                                                Ownername = Arrayaddress[2].Replace("\n", "").Trim();
                                                Address = Arrayaddress[3].Replace("\n", "").Trim();
                                            }
                                            if (townshipcode == "03" || townshipcode == "14" || townshipcode == "25")
                                            {

                                                GisID = Arrayaddress[0];
                                                UniqueID = "";
                                                Ownername = Arrayaddress[1].Replace("\n", "").Trim();
                                                Address = Arrayaddress[2].Replace("\n", "").Trim();
                                            }
                                            IWebElement Parcellink = AddressTD[2].FindElement(By.TagName("a"));
                                            hrefCardlink = Parcellink.GetAttribute("href");
                                            if (townshipcode == "03")
                                            {
                                                IWebElement Parcellinkw = AddressTD[2].FindElement(By.LinkText("eQuality Card"));
                                                hrefparcellink = Parcellinkw.GetAttribute("href");
                                            }
                                            else if (townshipcode == "14" || townshipcode == "25")
                                            {
                                                IWebElement Parcellinkw = AddressTD[2].FindElement(By.LinkText("Property Card"));
                                                hrefparcellink = Parcellinkw.GetAttribute("href");
                                            }
                                            else
                                            {
                                                IWebElement Parcellinkw = AddressTD[2].FindElement(By.LinkText("Summary Card"));
                                                hrefparcellink = Parcellinkw.GetAttribute("href");
                                            }
                                            string Multiresult = Address + "~" + Ownername + "~" + UniqueID;
                                            gc.insert_date(orderNumber, GisID, 2185, Multiresult, 1, DateTime.Now);
                                            Max++;
                                            gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "CT", countynameCT);
                                        }

                                    }
                                    multiparceldata = "Address~Owner~Account Number";
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2185'");

                                    if (Max == 1)
                                    {
                                        driver.Navigate().GoToUrl(hrefCardlink);
                                        Thread.Sleep(5000);
                                    }
                                    if (Max > 1 && Max < 26)
                                    {
                                        HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                        driver.Quit();
                                        return "MultiParcel";
                                    }
                                    if (Max > 25)
                                    {
                                        HttpContext.Current.Session["multiParcel_Multicount_CT" + countynameCT + township] = "Maximum";
                                        driver.Quit();
                                        return "Maximum";
                                    }
                                    if (Max == 0)
                                    {
                                        HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                        driver.Quit();
                                        return "No Data Found";
                                    }
                                }
                                catch (Exception e)
                                { }
                            }
                            else
                            {
                                try
                                {
                                    chromedriver.Navigate().GoToUrl(urlAssess);
                                    chromedriver.ExecuteJavaScript("document.getElementById('mbl').setAttribute('value','" + parcelNumber + "')");
                                    gc.CreatePdf_WOP(orderNumber, "Parcel Search ", chromedriver, "CT", countynameCT);
                                    IWebElement Iviewpay = chromedriver.FindElement(By.Name("go"));
                                    IJavaScriptExecutor js1 = chromedriver as IJavaScriptExecutor;
                                    js1.ExecuteScript("arguments[0].click();", Iviewpay);
                                    Thread.Sleep(5000);
                                    gc.CreatePdf_WOP(orderNumber, "Parcel Search After", chromedriver, "CT", countynameCT);
                                    // IWebElement iframeElement1 = driver.FindElement(By.XPath("//*[@id='body']"));
                                    chromedriver.SwitchTo().Frame(0);

                                    try
                                    {
                                        string nodata = chromedriver.FindElement(By.XPath("/html/body/div[2]")).Text;
                                        if (nodata.Contains("No matching"))
                                        {
                                            gc.CreatePdf_WOP(orderNumber, "Parcel Nodata", chromedriver, "CT", countynameCT);
                                            HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                            chromedriver.Quit();
                                            return "No Data Found";
                                        }
                                    }
                                    catch { }
                                    int Max = 0;


                                    string GisID = "", UniqueID = "", Ownername = "", Address = "";
                                    IWebElement Addresstable = chromedriver.FindElement(By.XPath("/html/body/div[2]/table/tbody"));
                                    IList<IWebElement> Addresrow = Addresstable.FindElements(By.TagName("tr"));
                                    IList<IWebElement> AddressTD;
                                    gc.CreatePdf_WOP(orderNumber, "Parcel Search After1", driver, "CT", countynameCT);
                                    foreach (IWebElement AddressT in Addresrow)
                                    {
                                        AddressTD = AddressT.FindElements(By.TagName("td"));
                                        if (AddressTD.Count > 2 && !AddressT.Text.Contains("Quick Links") && AddressT.Text.Trim() != "")
                                        {
                                            string[] Arrayaddress = AddressTD[1].Text.Split('\r');
                                            if (townshipcode == "01" || townshipcode == "06")
                                            {

                                                GisID = Arrayaddress[0];
                                                UniqueID = Arrayaddress[1].Replace("\n", "").Trim();
                                                Ownername = Arrayaddress[2].Replace("\n", "").Trim();
                                                Address = Arrayaddress[3].Replace("\n", "").Trim();
                                            }
                                            if (townshipcode == "03" || townshipcode == "14" || townshipcode == "25")
                                            {

                                                GisID = Arrayaddress[0];
                                                UniqueID = "";
                                                Ownername = Arrayaddress[1].Replace("\n", "").Trim();
                                                Address = Arrayaddress[2].Replace("\n", "").Trim();
                                            }
                                            IWebElement Parcellink = AddressTD[2].FindElement(By.TagName("a"));
                                            hrefCardlink = Parcellink.GetAttribute("href");
                                            if (townshipcode == "03")
                                            {
                                                IWebElement Parcellinkw = AddressTD[2].FindElement(By.LinkText("eQuality Card"));
                                                hrefparcellink = Parcellinkw.GetAttribute("href");
                                            }
                                            else if (townshipcode == "14" || townshipcode == "25")
                                            {
                                                IWebElement Parcellinkw = AddressTD[2].FindElement(By.LinkText("Property Card"));
                                                hrefparcellink = Parcellinkw.GetAttribute("href");
                                            }
                                            else
                                            {
                                                IWebElement Parcellinkw = AddressTD[2].FindElement(By.LinkText("Summary Card"));
                                                hrefparcellink = Parcellinkw.GetAttribute("href");
                                            }
                                            string Multiresult = Address + "~" + Ownername + "~" + UniqueID;
                                            gc.insert_date(orderNumber, GisID, 2185, Multiresult, 1, DateTime.Now);
                                            Max++;
                                            gc.CreatePdf_WOP(orderNumber, "Address Search Result", chromedriver, "CT", countynameCT);
                                        }

                                    }
                                    multiparceldata = "Address~Owner~Account Number";
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2185'");

                                    if (Max == 1)
                                    {
                                        chromedriver.Navigate().GoToUrl(hrefCardlink);
                                        Thread.Sleep(5000);
                                    }
                                    if (Max > 1 && Max < 26)
                                    {
                                        HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                        chromedriver.Quit();
                                        return "MultiParcel";
                                    }
                                    if (Max > 25)
                                    {
                                        HttpContext.Current.Session["multiParcel_Multicount_CT" + countynameCT + township] = "Maximum";
                                        chromedriver.Quit();
                                        return "Maximum";
                                    }
                                    if (Max == 0)
                                    {
                                        HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                        chromedriver.Quit();
                                        return "No Data Found";
                                    }
                                }
                                catch (Exception e)
                                { }
                            }

                        }


                        if (countAssess == "7")//parcel
                        {


                            try
                            {
                                IWebElement parceldata = driver.FindElement(By.Id("option-3"));
                                IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                                js1.ExecuteScript("arguments[0].click();", parceldata);
                            }
                            catch { }
                            Thread.Sleep(5000);
                            Thread.Sleep(2000);
                            driver.FindElement(By.Id("col2_filter")).SendKeys(parcelNumber);
                            Thread.Sleep(5000);
                            gc.CreatePdf_WOP(orderNumber, "Parcel search", driver, "CT", countynameCT);
                            try
                            {
                                string Nodatafound = driver.FindElement(By.XPath("//*[@id='example']/tbody/tr/td")).Text;
                                if (Nodatafound.Contains("No matching records"))
                                {
                                    HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }
                            driver.FindElement(By.XPath("//*[@id='example']/tbody/tr")).Click();
                            Thread.Sleep(5000);

                        }
                        if (countAssess == "8")//parcel
                        {
                            //driver.Navigate().GoToUrl(urlAssess);
                            driver.FindElement(By.Name("mbl")).SendKeys(parcelNumber);
                            gc.CreatePdf_WOP(orderNumber, "Parcel search", driver, "CT", countynameCT);
                            driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td[2]/table/tbody/tr/td/form[3]/div/input[2]")).Click();
                            Thread.Sleep(4000);
                            gc.CreatePdf_WOP(orderNumber, "Parcel search Result", driver, "CT", countynameCT);
                            IWebElement parcellink = driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td[2]/div[1]/p/table/tbody/tr[2]/td[1]")).FindElement(By.TagName("a"));
                            string parcelhref = parcellink.GetAttribute("href");
                            driver.Navigate().GoToUrl(parcelhref);
                        }

                        if (countAssess == "14")//parcel
                        {
                            HttpContext.Current.Session["Parcel_CT" + countynameCT + township] = "Yes";
                            driver.Quit();
                            return "No Parcel Search";
                        }
                    }
                    #endregion

                    #region Account Number search
                    if (searchType == "accountno")
                    {
                        if (countAssess == "0")//Bridgeport
                        {
                            IWebElement IAccountSelect = driver.FindElement(By.Id("MainContent_ddlSearchSource"));
                            SelectElement sAccountSelect = new SelectElement(IAccountSelect);
                            sAccountSelect.SelectByText("Acct#");
                            driver.FindElement(By.Id("MainContent_txtSearchAcctNum")).SendKeys(assessment_id);
                            gc.CreatePdf_WOP(orderNumber, "Account Numver Search", driver, "CT", countynameCT);
                            IWebElement Iowner = driver.FindElement(By.XPath("//*[@id='SearchAll']/span[7]/i"));
                            IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                            js.ExecuteScript("arguments[0].click();", Iowner);
                            // driver.FindElement(By.XPath("//*[@id='SearchAll']/span[7]/i")).SendKeys(Keys.Enter);
                            Thread.Sleep(1000);
                            try
                            {
                                string nodata = driver.FindElement(By.XPath("//*[@id='MainContent_grdSearchResults']")).Text;
                                if (nodata.Contains("No Data for Current Search"))
                                {
                                    HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }

                            IWebElement parceldata = driver.FindElement(By.XPath("//*[@id='MainContent_grdSearchResults']/tbody/tr[2]/td[1]/a"));
                            parceldata.Click();
                            Thread.Sleep(1000);
                        }
                        if (countAssess == "5")
                        {
                            if (townshipcode != "25")
                            {
                                try
                                {
                                    driver.ExecuteJavaScript("document.getElementById('mbl').setAttribute('value','" + assessment_id + "')");
                                    gc.CreatePdf_WOP(orderNumber, "Parcel Search ", driver, "CT", countynameCT);
                                    IWebElement Iviewpay = driver.FindElement(By.Name("go"));
                                    IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                                    js1.ExecuteScript("arguments[0].click();", Iviewpay);
                                    Thread.Sleep(5000);
                                    gc.CreatePdf_WOP(orderNumber, "Parcel Search After", driver, "CT", countynameCT);
                                    // IWebElement iframeElement1 = driver.FindElement(By.XPath("//*[@id='body']"));
                                    driver.SwitchTo().Frame(0);

                                    try
                                    {
                                        string nodata = driver.FindElement(By.XPath("/html/body/div[2]")).Text;
                                        if (nodata.Contains("No matching"))
                                        {
                                            gc.CreatePdf_WOP(orderNumber, "Parcel Nodata", driver, "CT", countynameCT);
                                            HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                            driver.Quit();
                                            return "No Data Found";
                                        }
                                    }
                                    catch { }
                                    int Max = 0;


                                    string GisID = "", UniqueID = "", Ownername = "", Address = "";
                                    IWebElement Addresstable = driver.FindElement(By.XPath("/html/body/div[2]/table/tbody"));
                                    IList<IWebElement> Addresrow = Addresstable.FindElements(By.TagName("tr"));
                                    IList<IWebElement> AddressTD;
                                    gc.CreatePdf_WOP(orderNumber, "Parcel Search After1", driver, "CT", countynameCT);
                                    foreach (IWebElement AddressT in Addresrow)
                                    {
                                        AddressTD = AddressT.FindElements(By.TagName("td"));
                                        if (AddressTD.Count > 2 && !AddressT.Text.Contains("Quick Links") && AddressT.Text.Trim() != "")
                                        {
                                            string[] Arrayaddress = AddressTD[1].Text.Split('\r');
                                            if (townshipcode == "01" || townshipcode == "06")
                                            {

                                                GisID = Arrayaddress[0];
                                                UniqueID = Arrayaddress[1].Replace("\n", "").Trim();
                                                Ownername = Arrayaddress[2].Replace("\n", "").Trim();
                                                Address = Arrayaddress[3].Replace("\n", "").Trim();
                                            }
                                            if (townshipcode == "03" || townshipcode == "14" || townshipcode == "25")
                                            {

                                                GisID = Arrayaddress[0];
                                                UniqueID = "";
                                                Ownername = Arrayaddress[1].Replace("\n", "").Trim();
                                                Address = Arrayaddress[2].Replace("\n", "").Trim();
                                            }
                                            IWebElement Parcellink = AddressTD[2].FindElement(By.TagName("a"));
                                            hrefCardlink = Parcellink.GetAttribute("href");
                                            if (townshipcode == "03")
                                            {
                                                IWebElement Parcellinkw = AddressTD[2].FindElement(By.LinkText("eQuality Card"));
                                                hrefparcellink = Parcellinkw.GetAttribute("href");
                                            }
                                            else if (townshipcode == "14" || townshipcode == "25")
                                            {
                                                IWebElement Parcellinkw = AddressTD[2].FindElement(By.LinkText("Property Card"));
                                                hrefparcellink = Parcellinkw.GetAttribute("href");
                                            }
                                            else
                                            {
                                                IWebElement Parcellinkw = AddressTD[2].FindElement(By.LinkText("Summary Card"));
                                                hrefparcellink = Parcellinkw.GetAttribute("href");
                                            }
                                            string Multiresult = Address + "~" + Ownername + "~" + UniqueID;
                                            gc.insert_date(orderNumber, GisID, 2185, Multiresult, 1, DateTime.Now);
                                            Max++;
                                            gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "CT", countynameCT);
                                        }

                                    }
                                    multiparceldata = "Address~Owner~Account Number";
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2185'");

                                    if (Max == 1)
                                    {
                                        driver.Navigate().GoToUrl(hrefCardlink);
                                        Thread.Sleep(5000);
                                    }
                                    if (Max > 1 && Max < 26)
                                    {
                                        HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                        driver.Quit();
                                        return "MultiParcel";
                                    }
                                    if (Max > 25)
                                    {
                                        HttpContext.Current.Session["multiParcel_Multicount_CT" + countynameCT + township] = "Maximum";
                                        driver.Quit();
                                        return "Maximum";
                                    }
                                    if (Max == 0)
                                    {
                                        HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                        driver.Quit();
                                        return "No Data Found";
                                    }
                                }
                                catch (Exception e)
                                { }
                            }

                            else
                            {
                                try
                                {
                                    chromedriver.Navigate().GoToUrl(urlAssess);
                                    chromedriver.ExecuteJavaScript("document.getElementById('mbl').setAttribute('value','" + assessment_id + "')");
                                    gc.CreatePdf_WOP(orderNumber, "Parcel Search ", chromedriver, "CT", countynameCT);
                                    IWebElement Iviewpay = chromedriver.FindElement(By.Name("go"));
                                    IJavaScriptExecutor js1 = chromedriver as IJavaScriptExecutor;
                                    js1.ExecuteScript("arguments[0].click();", Iviewpay);
                                    Thread.Sleep(5000);
                                    gc.CreatePdf_WOP(orderNumber, "Parcel Search After", chromedriver, "CT", countynameCT);
                                    // IWebElement iframeElement1 = driver.FindElement(By.XPath("//*[@id='body']"));
                                    chromedriver.SwitchTo().Frame(0);

                                    try
                                    {
                                        string nodata = chromedriver.FindElement(By.XPath("/html/body/div[2]")).Text;
                                        if (nodata.Contains("No matching"))
                                        {
                                            gc.CreatePdf_WOP(orderNumber, "Parcel Nodata", chromedriver, "CT", countynameCT);
                                            HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                            chromedriver.Quit();
                                            return "No Data Found";
                                        }
                                    }
                                    catch { }
                                    int Max = 0;


                                    string GisID = "", UniqueID = "", Ownername = "", Address = "";
                                    IWebElement Addresstable = chromedriver.FindElement(By.XPath("/html/body/div[2]/table/tbody"));
                                    IList<IWebElement> Addresrow = Addresstable.FindElements(By.TagName("tr"));
                                    IList<IWebElement> AddressTD;
                                    gc.CreatePdf_WOP(orderNumber, "Parcel Search After1", chromedriver, "CT", countynameCT);
                                    foreach (IWebElement AddressT in Addresrow)
                                    {
                                        AddressTD = AddressT.FindElements(By.TagName("td"));
                                        if (AddressTD.Count > 2 && !AddressT.Text.Contains("Quick Links") && AddressT.Text.Trim() != "")
                                        {
                                            string[] Arrayaddress = AddressTD[1].Text.Split('\r');
                                            if (townshipcode == "01" || townshipcode == "06")
                                            {

                                                GisID = Arrayaddress[0];
                                                UniqueID = Arrayaddress[1].Replace("\n", "").Trim();
                                                Ownername = Arrayaddress[2].Replace("\n", "").Trim();
                                                Address = Arrayaddress[3].Replace("\n", "").Trim();
                                            }
                                            if (townshipcode == "03" || townshipcode == "14" || townshipcode == "25")
                                            {

                                                GisID = Arrayaddress[0];
                                                UniqueID = "";
                                                Ownername = Arrayaddress[1].Replace("\n", "").Trim();
                                                Address = Arrayaddress[2].Replace("\n", "").Trim();
                                            }
                                            IWebElement Parcellink = AddressTD[2].FindElement(By.TagName("a"));
                                            hrefCardlink = Parcellink.GetAttribute("href");
                                            if (townshipcode == "03")
                                            {
                                                IWebElement Parcellinkw = AddressTD[2].FindElement(By.LinkText("eQuality Card"));
                                                hrefparcellink = Parcellinkw.GetAttribute("href");
                                            }
                                            else if (townshipcode == "14" || townshipcode == "25")
                                            {
                                                IWebElement Parcellinkw = AddressTD[2].FindElement(By.LinkText("Property Card"));
                                                hrefparcellink = Parcellinkw.GetAttribute("href");
                                            }
                                            else
                                            {
                                                IWebElement Parcellinkw = AddressTD[2].FindElement(By.LinkText("Summary Card"));
                                                hrefparcellink = Parcellinkw.GetAttribute("href");
                                            }
                                            string Multiresult = Address + "~" + Ownername + "~" + UniqueID;
                                            gc.insert_date(orderNumber, GisID, 2185, Multiresult, 1, DateTime.Now);
                                            Max++;
                                            gc.CreatePdf_WOP(orderNumber, "Address Search Result", chromedriver, "CT", countynameCT);
                                        }

                                    }
                                    multiparceldata = "Address~Owner~Account Number";
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2185'");

                                    if (Max == 1)
                                    {
                                        chromedriver.Navigate().GoToUrl(hrefCardlink);
                                        Thread.Sleep(5000);
                                    }
                                    if (Max > 1 && Max < 26)
                                    {
                                        HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                        chromedriver.Quit();
                                        return "MultiParcel";
                                    }
                                    if (Max > 25)
                                    {
                                        HttpContext.Current.Session["multiParcel_Multicount_CT" + countynameCT + township] = "Maximum";
                                        chromedriver.Quit();
                                        return "Maximum";
                                    }
                                    if (Max == 0)
                                    {
                                        HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                        chromedriver.Quit();
                                        return "No Data Found";
                                    }
                                }
                                catch (Exception e)
                                { }
                            }
                        }
                    }
                    #endregion

                    //Property details
                    #region Zero Assessment Link
                    if (countAssess == "0")////Bridgeport
                    {
                        //Property Details
                        string PropertyAddress = "", MapLot = "", Owner = "", Assessment = "", Appraisal = "", ParcelID = "", BuildingCount = "";
                        assessment_id = "";

                        IWebElement IBasicDetails = driver.FindElement(By.XPath("//*[@id='tabs-1']"));
                        IList<IWebElement> IBasicDetailsRow = IBasicDetails.FindElements(By.TagName("div"));
                        IList<IWebElement> IBasicDetailsTD;
                        foreach (IWebElement row in IBasicDetailsRow)
                        {
                            IBasicDetailsTD = row.FindElements(By.TagName("dl"));
                            if (IBasicDetailsTD.Count != 0 && row.Text.Contains("Location"))
                            {
                                PropertyAddress = IBasicDetailsTD[0].Text.Replace("\r\n", "").Replace("Location", "").Trim();
                            }
                            if (IBasicDetailsTD.Count != 0 && row.Text.Contains("Mblu"))
                            {

                                MapLot = IBasicDetailsTD[1].Text.Replace("Mblu\r\n", "").Trim();
                                parcelNumber = MapLot;
                            }
                            if (IBasicDetailsTD.Count != 0 && row.Text.Contains("Acct#"))
                            {
                                assessment_id = IBasicDetailsTD[2].Text.Replace("Acct#\r\n", "").Trim();
                            }
                            if (IBasicDetailsTD.Count != 0 && row.Text.Contains("Owner"))
                            {
                                //string ownertext = IBasicDetailsTD[3].Text;
                                if (IBasicDetailsTD[3].Text.Contains("Owner"))
                                {
                                    Owner = IBasicDetailsTD[3].Text.Replace("Owner\r\n", "").Trim();
                                }
                                else if (IBasicDetailsTD[2].Text.Contains("Owner"))
                                {
                                    Owner = IBasicDetailsTD[2].Text.Replace("Owner\r\n", "").Trim();
                                }
                            }
                            if (IBasicDetailsTD.Count != 0 && row.Text.Contains("Assessment"))
                            {
                                if (IBasicDetailsTD[4].Text.Contains("Assessment"))
                                {
                                    Assessment = IBasicDetailsTD[4].Text.Replace("Assessment\r\n", "").Trim();

                                }
                                else if (IBasicDetailsTD[3].Text.Contains("Assessment"))
                                {
                                    Assessment = IBasicDetailsTD[3].Text.Replace("Assessment\r\n", "").Trim();

                                }
                            }
                            if (IBasicDetailsTD.Count != 0 && row.Text.Contains("Appraisal"))
                            {
                                if (IBasicDetailsTD[5].Text.Contains("Appraisal"))
                                {
                                    Appraisal = IBasicDetailsTD[5].Text.Replace("Appraisal\r\n", "").Trim();
                                }
                                else if (IBasicDetailsTD[4].Text.Contains("Appraisal"))
                                {
                                    Appraisal = IBasicDetailsTD[4].Text.Replace("Appraisal\r\n", "").Trim();
                                }
                            }
                            if (IBasicDetailsTD.Count != 0 && row.Text.Contains("PID"))
                            {
                                if (IBasicDetailsTD[6].Text.Contains("PID"))
                                {
                                    ParcelID = IBasicDetailsTD[6].Text.Replace("PID\r\n", "").Trim();
                                }
                                else if (IBasicDetailsTD[5].Text.Contains("PID"))
                                {
                                    ParcelID = IBasicDetailsTD[5].Text.Replace("PID\r\n", "").Trim();
                                }
                            }
                            if (IBasicDetailsTD.Count != 0 && row.Text.Contains("Building Count"))
                            {
                                try
                                {
                                    if (IBasicDetailsTD[7].Text.Contains("Building Count"))
                                    {
                                        BuildingCount = IBasicDetailsTD[7].Text.Replace("Building Count\r\n", "").Trim();
                                    }
                                }
                                catch
                                { }
                                try
                                {
                                    if (IBasicDetailsTD[6].Text.Contains("Building Count"))
                                    {
                                        BuildingCount = IBasicDetailsTD[6].Text.Replace("Building Count\r\n", "").Trim();
                                    }
                                }
                                catch { }

                            }
                        }

                        if (assessment_id == "")
                        {
                            assessment_id = ParcelID;
                        }

                        string[] splitAddress = PropertyAddress.Split(' ');
                        streetno1 = splitAddress[0];
                        if (splitAddress.Count() == 2)
                        {
                            streetname1 = splitAddress[1];
                            streetname1 = streetname1.Trim();
                        }

                        if (splitAddress.Count() == 3)
                        {
                            streetname1 = splitAddress[1] + " " + splitAddress[2];
                            streetname1 = streetname1.Trim();
                        }

                        if (splitAddress.Count() == 4)
                        {
                            streetname1 = splitAddress[1] + " " + splitAddress[2] + " " + splitAddress[3];
                            streetname1 = streetname1.Trim();
                        }

                        if (splitAddress.Count() == 5)
                        {
                            streetname1 = splitAddress[1] + " " + splitAddress[2] + " " + splitAddress[3] + " " + splitAddress[4];
                            streetname1 = streetname1.Trim();
                        }

                        if (townshipcode == "04")
                        {
                            uniqueidMap = assessment_id;
                            uniqueidMap = uniqueidMap.TrimStart('0');
                        }
                        if (townshipcode == "09")
                        {
                            uniqueidMap = MapLot.Replace("/", "").Trim();
                            uniqueidMap = string.Concat(uniqueidMap.Where(c => !char.IsWhiteSpace(c)));
                            assessment_id = uniqueidMap;
                            parcelNumber = uniqueidMap;
                        }
                        if (townshipcode == "18")
                        {
                            assessment_id = assessment_id.PadLeft(uniqueidMap.Length + 8, '0');
                            uniqueidMap = assessment_id;
                        }
                        if (townshipcode == "16" || townshipcode == "13")
                        {

                            uniqueidMap = assessment_id;
                            parcelNumber = assessment_id;
                        }
                        if (townshipcode == "26" || townshipcode == "19" || townshipcode == "22" || townshipcode == "12" || townshipcode == "15" || townshipcode == "17" || townshipcode == "10")
                        {
                            uniqueidMap = assessment_id;
                        }
                        string BasicDetails = PropertyAddress + "~" + MapLot + "~" + assessment_id + "~" + Owner + "~" + Assessment + "~" + Appraisal + "~" + ParcelID + "~" + BuildingCount;
                        string property1 = "Property Address~Mblu~Account Number~Owner Name~Assessment~Appraisal~PID~Building Count";

                        dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property1 + "' where Id = '2174'");
                        gc.insert_date(orderNumber, assessment_id, 2174, BasicDetails, 1, DateTime.Now);
                        //Property Address~Mblu~Account Number~Owner Name~Assessment~Appraisal~PID~Building Count
                        assessment_id = assessment_id.Replace("-", "");
                        gc.CreatePdf(orderNumber, assessment_id, "Search Result", driver, "CT", countynameCT);

                        //Current Appraisal Valuation
                        IWebElement ICurrentValueAppDetails = driver.FindElement(By.XPath("//*[@id='MainContent_grdCurrentValueAppr']/tbody"));
                        IList<IWebElement> ICurrentValueAppDetailsRow = ICurrentValueAppDetails.FindElements(By.TagName("tr"));
                        IList<IWebElement> IICurrentValueAppDetailsTD;
                        foreach (IWebElement row in ICurrentValueAppDetailsRow)
                        {
                            IICurrentValueAppDetailsTD = row.FindElements(By.TagName("td"));
                            if (IICurrentValueAppDetailsTD.Count != 0 && IICurrentValueAppDetailsTD.Count == 4 && !row.Text.Contains("Valuation Year"))
                            {
                                string ValueAppraisal = "Appraisal" + "~" + IICurrentValueAppDetailsTD[0].Text + "~" + IICurrentValueAppDetailsTD[1].Text + "~" + IICurrentValueAppDetailsTD[2].Text + "~" + IICurrentValueAppDetailsTD[3].Text;
                                gc.insert_date(orderNumber, assessment_id, 2176, ValueAppraisal, 1, DateTime.Now);
                                //Type~Valuation Year~Improvements~Land~Total
                            }
                            if (IICurrentValueAppDetailsTD.Count != 0 && IICurrentValueAppDetailsTD.Count == 2 && !row.Text.Contains("Valuation Year"))
                            {
                                string ValueAppraisal = "Appraisal" + "~" + IICurrentValueAppDetailsTD[0].Text + "~" + "" + "~" + "" + "~" + IICurrentValueAppDetailsTD[1].Text;
                                gc.insert_date(orderNumber, assessment_id, 2176, ValueAppraisal, 1, DateTime.Now);
                                //Type~Valuation Year~Improvements~Land~Total
                            }
                        }
                        string property2 = "Type~Valuation Year~Improvements~Land~Total";
                        dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property2 + "' where Id = '2176'");
                        //Current Assessment Valuation
                        IWebElement ICurrentValueAssDetails = driver.FindElement(By.XPath("//*[@id='MainContent_grdCurrentValueAsmt']/tbody"));
                        IList<IWebElement> ICurrentValueAssDetailsRow = ICurrentValueAssDetails.FindElements(By.TagName("tr"));
                        IList<IWebElement> IICurrentValueAssDetailsTD;
                        foreach (IWebElement assessment in ICurrentValueAssDetailsRow)
                        {
                            IICurrentValueAssDetailsTD = assessment.FindElements(By.TagName("td"));
                            if (IICurrentValueAssDetailsTD.Count != 0 && IICurrentValueAssDetailsTD.Count == 4 && !assessment.Text.Contains("Valuation Year"))
                            {
                                string ValueAssessment = "Assessment" + "~" + IICurrentValueAssDetailsTD[0].Text + "~" + IICurrentValueAssDetailsTD[1].Text + "~" + IICurrentValueAssDetailsTD[2].Text + "~" + IICurrentValueAssDetailsTD[3].Text;
                                gc.insert_date(orderNumber, assessment_id, 2176, ValueAssessment, 1, DateTime.Now);
                                //Type~Valuation Year~Improvements~Land~Total
                            }
                            if (IICurrentValueAssDetailsTD.Count != 0 && IICurrentValueAssDetailsTD.Count == 2 && !assessment.Text.Contains("Valuation Year"))
                            {
                                string ValueAssessment = "Assessment" + "~" + IICurrentValueAssDetailsTD[0].Text + "~" + "" + "~" + "" + "~" + IICurrentValueAssDetailsTD[1].Text;
                                gc.insert_date(orderNumber, assessment_id, 2176, ValueAssessment, 1, DateTime.Now);
                                //Type~Valuation Year~Improvements~Land~Total
                            }
                        }

                        //Owner of Record
                        string ownerOnly = "", ownerAddress = "";

                        string Propertyhead = "";
                        string Propertyresult = "";

                        IWebElement multitableElement1 = driver.FindElement(By.XPath("//*[@id='MainContent_grdSales']/tbody"));
                        IList<IWebElement> multitableRow1 = multitableElement1.FindElements(By.TagName("tr"));
                        IList<IWebElement> multirowTD1;
                        IList<IWebElement> multirowTH1;
                        foreach (IWebElement row in multitableRow1)
                        {
                            multirowTH1 = row.FindElements(By.TagName("tH"));
                            multirowTD1 = row.FindElements(By.TagName("td"));
                            if (multirowTD1.Count != 0 && multirowTD1[0].Text != " ")
                            {
                                for (int i = 0; i < multirowTD1.Count; i++)
                                {
                                    Propertyresult += multirowTD1[i].Text + "~";
                                }
                                Propertyresult = Propertyresult.TrimEnd('~');
                                gc.insert_date(orderNumber, assessment_id, 2177, Propertyresult, 1, DateTime.Now);
                                Propertyresult = "";
                            }
                            if (multirowTH1.Count != 0 && multirowTH1[0].Text != " ")
                            {
                                for (int i = 0; i < multirowTH1.Count; i++)
                                {

                                    Propertyhead += multirowTH1[i].Text + "~";
                                }
                                Propertyhead = Propertyhead.TrimEnd('~');
                                dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + Propertyhead + "' where Id = '2177'");

                            }


                        }

                        // string property3 = "Address~Owner~Sale Price~Certificate~Book Page~Instrument~Sale Date";

                        //Building Information
                        string YearBuilt = "", LivingArea = "", ReplacementCost = "", BuildingPercent = "", LessDepreciation = "";
                        IWebElement IYearBuilt = driver.FindElement(By.XPath("//*[@id='MainContent_ctl01_tblBldg']/tbody"));
                        IList<IWebElement> IYearBuiltRow = IYearBuilt.FindElements(By.TagName("tr"));
                        IList<IWebElement> IYearBuiltTD;
                        foreach (IWebElement built in IYearBuiltRow)
                        {
                            IYearBuiltTD = built.FindElements(By.TagName("td"));
                            if (IYearBuiltTD.Count != 0 && built.Text.Contains("Year Built"))
                            {
                                YearBuilt = IYearBuiltTD[1].Text;
                            }
                            if (IYearBuiltTD.Count != 0 && built.Text.Contains("Living Area"))
                            {
                                LivingArea = IYearBuiltTD[1].Text;
                            }
                            if (IYearBuiltTD.Count != 0 && built.Text.Contains("Replacement Cost:"))
                            {
                                ReplacementCost = IYearBuiltTD[1].Text;
                            }
                            if (IYearBuiltTD.Count != 0 && built.Text.Contains("Building Percent Good"))
                            {
                                BuildingPercent = IYearBuiltTD[1].Text;
                            }
                            if (IYearBuiltTD.Count != 0 && built.Text.Contains("Less Depreciation"))
                            {
                                LessDepreciation = IYearBuiltTD[1].Text;
                            }
                        }

                        string YearBuiltDetails = YearBuilt + "~" + LivingArea + "~" + ReplacementCost + "~" + BuildingPercent + "~" + LessDepreciation;
                        gc.insert_date(orderNumber, assessment_id, 2178, YearBuiltDetails, 1, DateTime.Now);
                        string property4 = "Year Built~Living Area~Replacement Cost~Building Percent Good~Replacement Cost Less Depreciation";
                        dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property4 + "' where Id = '2178'");

                        //Year Built~Living Area~Replacement Cost~Building Percent Good~Replacement Cost Less Depreciation

                        //Land 
                        string UseCode = "", Description = "", Zone = "", Neighborhood = "", LandAppr = "", Category = "", Size = "", Frontage = "", Depth = "", AssessedValue = "", AppraisedValue = "";
                        IWebElement ILandUseDetails = driver.FindElement(By.XPath("//*[@id='MainContent_tblLandUse']/tbody"));
                        IList<IWebElement> ILandUseDetailsRow = ILandUseDetails.FindElements(By.TagName("tr"));
                        IList<IWebElement> ILandUseDetailsTD;
                        foreach (IWebElement use in ILandUseDetailsRow)
                        {
                            ILandUseDetailsTD = use.FindElements(By.TagName("td"));
                            if (ILandUseDetailsTD.Count != 0 && use.Text.Contains("Use Code"))
                            {
                                UseCode = ILandUseDetailsTD[1].Text;
                            }
                            if (ILandUseDetailsTD.Count != 0 && use.Text.Contains("Description"))
                            {
                                Description = ILandUseDetailsTD[1].Text;
                            }
                            if (ILandUseDetailsTD.Count != 0 && use.Text.Contains("Zone"))
                            {
                                Zone = ILandUseDetailsTD[1].Text;
                            }
                            if (ILandUseDetailsTD.Count != 0 && use.Text.Contains("Neighborhood"))
                            {
                                Neighborhood = ILandUseDetailsTD[1].Text;
                            }
                            if (ILandUseDetailsTD.Count != 0 && use.Text.Contains("Alt Land Appr"))
                            {
                                LandAppr = ILandUseDetailsTD[1].Text;
                            }
                            if (ILandUseDetailsTD.Count != 0 && use.Text.Contains("Category"))
                            {
                                Category = ILandUseDetailsTD[1].Text;
                            }
                        }

                        IWebElement ILandLineDetails = driver.FindElement(By.XPath("//*[@id='MainContent_tblLand']/tbody"));
                        IList<IWebElement> ILandLineDetailsRow = ILandLineDetails.FindElements(By.TagName("tr"));
                        IList<IWebElement> ILandLineDetailsTD;
                        foreach (IWebElement line in ILandLineDetailsRow)
                        {
                            ILandLineDetailsTD = line.FindElements(By.TagName("td"));
                            if (ILandLineDetailsTD.Count != 0 && line.Text.Contains("Size (Acres)"))
                            {
                                Size = ILandLineDetailsTD[1].Text;
                            }
                            if (ILandLineDetailsTD.Count != 0 && line.Text.Contains("Frontage"))
                            {
                                Frontage = ILandLineDetailsTD[1].Text;
                            }
                            if (ILandLineDetailsTD.Count != 0 && line.Text.Contains("Depth"))
                            {
                                Depth = ILandLineDetailsTD[1].Text;
                            }
                            if (ILandLineDetailsTD.Count != 0 && line.Text.Contains("Assessed Value"))
                            {
                                AssessedValue = ILandLineDetailsTD[1].Text;
                            }
                            if (ILandLineDetailsTD.Count != 0 && line.Text.Contains("Appraised Value"))
                            {
                                AppraisedValue = ILandLineDetailsTD[1].Text;
                            }
                        }

                        string LandDetails = UseCode + "~" + Description + "~" + Zone + "~" + Neighborhood + "~" + LandAppr + "~" + Category + "~" + Size + "~" + Frontage + "~" + Depth + "~" + AssessedValue + "~" + AppraisedValue;
                        gc.insert_date(orderNumber, assessment_id, 2183, LandDetails, 1, DateTime.Now);
                        //Use Code~Description~Zone~Neighborhood~Alt Land Appr~Category~Size(Acres)~Frontage~Depth~Assessed Value~Appraised Value

                        string property5 = "Use Code~Description~Zone~Neighborhood~Alt Land Appr~Category~Size(Acres)~Frontage~Depth~Assessed Value~Appraised Value";
                        dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property5 + "' where Id = '2183'");

                        try
                        {
                            //Appraisal Valuation History
                            IWebElement IValueAppDetails = driver.FindElement(By.XPath("//*[@id='MainContent_grdHistoryValuesAppr']/tbody"));
                            IList<IWebElement> IValueAppDetailsRow = IValueAppDetails.FindElements(By.TagName("tr"));
                            IList<IWebElement> IValueAppDetailsTD;
                            foreach (IWebElement row in IValueAppDetailsRow)
                            {
                                IValueAppDetailsTD = row.FindElements(By.TagName("td"));
                                if (IValueAppDetailsTD.Count != 0 && IValueAppDetailsTD.Count == 4 && !row.Text.Contains("Valuation Year"))
                                {
                                    string ValueAppraisal = "Appraisal" + "~" + IValueAppDetailsTD[0].Text + "~" + IValueAppDetailsTD[1].Text + "~" + IValueAppDetailsTD[2].Text + "~" + IValueAppDetailsTD[3].Text;
                                    gc.insert_date(orderNumber, assessment_id, 2184, ValueAppraisal, 1, DateTime.Now);
                                    //Type~Valuation Year~Improvements~Land~Total
                                }
                                if (IValueAppDetailsTD.Count != 0 && IValueAppDetailsTD.Count == 2 && !row.Text.Contains("Valuation Year"))
                                {
                                    string ValueAppraisal = "Appraisal" + "~" + IValueAppDetailsTD[0].Text + "~" + "" + "~" + "" + "~" + IValueAppDetailsTD[1].Text;
                                    gc.insert_date(orderNumber, assessment_id, 2184, ValueAppraisal, 1, DateTime.Now);
                                    //Type~Valuation Year~Improvements~Land~Total
                                }
                            }
                        }
                        catch { }
                        //Assessment Valuation History
                        try
                        {
                            IWebElement IValueAssDetails = driver.FindElement(By.XPath("//*[@id='MainContent_grdHistoryValuesAsmt']/tbody"));
                            IList<IWebElement> IValueAssDetailsRow = IValueAssDetails.FindElements(By.TagName("tr"));
                            IList<IWebElement> IValueAssDetailsTD;
                            foreach (IWebElement assessment in IValueAssDetailsRow)
                            {
                                IValueAssDetailsTD = assessment.FindElements(By.TagName("td"));
                                if (IValueAssDetailsTD.Count != 0 && IValueAssDetailsTD.Count == 4 && !assessment.Text.Contains("Valuation Year"))
                                {
                                    string ValueAssessment = "Assessment" + "~" + IValueAssDetailsTD[0].Text + "~" + IValueAssDetailsTD[1].Text + "~" + IValueAssDetailsTD[2].Text + "~" + IValueAssDetailsTD[3].Text;
                                    gc.insert_date(orderNumber, assessment_id, 2184, ValueAssessment, 1, DateTime.Now);
                                    //Type~Valuation Year~Improvements~Land~Total
                                }
                                if (IValueAssDetailsTD.Count != 0 && IValueAssDetailsTD.Count == 2 && !assessment.Text.Contains("Valuation Year"))
                                {
                                    string ValueAssessment = "Assessment" + "~" + IValueAssDetailsTD[0].Text + "~" + "" + "~" + "" + "~" + IValueAssDetailsTD[1].Text;
                                    gc.insert_date(orderNumber, assessment_id, 2184, ValueAssessment, 1, DateTime.Now);
                                    //Type~Valuation Year~Improvements~Land~Total
                                }
                            }
                        }
                        catch
                        { }

                        string property6 = "Type~Valuation Year~Improvements~Land~Total";
                        dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property6 + "' where Id = '2184'");
                        string urlpdf = "http://images.vgsi.com/cards/WestportCTCards//" + ParcelID + ".pdf";
                        try
                        {
                            gc.downloadfile(urlpdf, orderNumber, assessment_id, "Propertypdf", "CT", countynameCT);
                            Thread.Sleep(3000);
                        }
                        catch { }
                        //if (townshipcode == "31")
                        //{
                        //    try
                        //    {
                        //        string FilePath = gc.filePath(orderNumber, assessment_id) + "Propertypdf.pdf";
                        //        PdfReader reader;
                        //        string pdfData;
                        //        string pdftext = "";
                        //        try
                        //        {
                        //            reader = new PdfReader(FilePath);
                        //            String textFromPage = PdfTextExtractor.GetTextFromPage(reader, 1);
                        //            System.Diagnostics.Debug.WriteLine("" + textFromPage);

                        //            pdftext = textFromPage;
                        //        }
                        //        catch { }


                        //        string tableassess = gc.Between(pdftext, "Account #", "Bldg #:").Trim();
                        //        if (tableassess.Length == 4)
                        //        {
                        //            tableassess = "0" + tableassess;
                        //        }
                        //        if (tableassess.Length == 3)
                        //        {
                        //            tableassess = "00" + tableassess;
                        //        }
                        //        uniqueidMap = tableassess.Trim();
                        //        assessment_id = uniqueidMap;
                        //    }
                        //    catch { }
                        //}


                    }
                    #endregion
                    #region one Assessment Link
                    if (countAssess == "1")//Easton
                    {
                        try
                        {
                            driver.FindElement(By.LinkText("Parcel Data And Values")).Click();
                        }
                        catch { }
                        string ParcelValues = "", ParcelValuesHeader = "";

                        string col1 = "-", col3 = "-", col5 = "-";
                        gc.CreatePdf(orderNumber, parcelNumber, "Property Summary Info", driver, "CT", countynameCT);
                        IWebElement Parcel = driver.FindElement(By.XPath("//*[@id='tabParcel']/div[1]/div/table/tbody"));
                        IList<IWebElement> TRParcel = Parcel.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDParcel;
                        foreach (IWebElement row in TRParcel)
                        {
                            TDParcel = row.FindElements(By.TagName("td"));
                            if (TDParcel.Count != 0 && TDParcel.Count == 6 && (row.Text.Contains("Location") || row.Text.Contains("Unique ID") || row.Text.Contains("Zone") || row.Text.Contains("Census")))
                            {

                                ParcelValuesHeader += TDParcel[0].Text + "~" + TDParcel[2].Text + "~" + TDParcel[4].Text + "~";
                                col1 = TDParcel[1].Text;
                                col3 = TDParcel[3].Text;
                                col5 = TDParcel[5].Text;
                                if (TDParcel[4].Text == "")
                                {
                                    col5 = "-";
                                }
                                ParcelValues += col1 + "~" + col3 + "~" + col5 + "~";

                            }
                            if (TDParcel.Count != 0 && TDParcel.Count == 6 && (row.Text.Contains("Unique ID")))
                            {
                                if (TDParcel[0].Text == "Unique ID:")
                                {
                                    parcelNumber = TDParcel[1].Text;
                                }
                                if (TDParcel[2].Text == "Unique ID:")
                                {
                                    parcelNumber = TDParcel[3].Text;
                                }
                                if (TDParcel[4].Text == "Unique ID:")
                                {
                                    parcelNumber = TDParcel[5].Text;
                                }
                            }
                        }

                        string valuetitle = "", valueAppvalues = "", valueAssvalues = "";
                        IWebElement Value = driver.FindElement(By.XPath("//*[@id='tabParcel']/div[2]/div[1]/table/tbody"));
                        IList<IWebElement> TRValue = Value.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDValue;
                        foreach (IWebElement ValueInfo in TRValue)
                        {
                            TDValue = ValueInfo.FindElements(By.TagName("td"));
                            if (TDValue.Count != 0 && !ValueInfo.Text.Contains("Appraised Value"))
                            {
                                valuetitle += TDValue[0].Text + "~";
                                valueAppvalues += TDValue[1].Text + "~";
                                valueAssvalues += TDValue[2].Text + "~";
                            }
                        }
                        //Type~Land~Buildings~Detached Outbuildings~Total
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + "Type~" + valuetitle.Remove(valuetitle.Length - 1, 1) + "' where Id = '" + 2176 + "'");
                        gc.insert_date(orderNumber, parcelNumber, 2176, "Appraised Value~" + valueAppvalues.Remove(valueAppvalues.Length - 1, 1), 1, DateTime.Now);
                        gc.insert_date(orderNumber, parcelNumber, 2176, "Assessed Value~" + valueAssvalues.Remove(valueAssvalues.Length - 1, 1), 1, DateTime.Now);
                        string YearBuild = "";
                        try
                        {

                            driver.FindElement(By.Id("MainContent_showBuildingTab")).Click();
                            Thread.Sleep(1000);
                            IWebElement BuildingClick = driver.FindElement(By.XPath("//*[@id='MainContent_showBuildingTab']/ul"));
                            IList<IWebElement> TRBuildingClick = BuildingClick.FindElements(By.TagName("li"));
                            foreach (IWebElement Build in TRBuildingClick)
                            {
                                string strBuilding = Build.GetAttribute("innerText");
                                if (strBuilding.Contains("Building 1"))
                                {
                                    IWebElement IBuilding1 = Build.FindElement(By.TagName("a"));
                                    IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                                    js1.ExecuteScript("arguments[0].click();", IBuilding1);
                                    break;
                                }
                            }
                            gc.CreatePdf(orderNumber, parcelNumber, "Building Search Result", driver, "CT", countynameCT);

                            IWebElement Building = driver.FindElement(By.XPath("//*[@id='tabBuilding1']/div/div/div/div[2]/table/tbody"));
                            IList<IWebElement> TRBuilding = Building.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDBuilding;
                            foreach (IWebElement Build in TRBuilding)
                            {
                                TDBuilding = Build.FindElements(By.TagName("td"));
                                try
                                {
                                    if (TDBuilding.Count != 0 && TDBuilding[0].Text.Contains("Year Built"))
                                    {
                                        YearBuild = TDBuilding[1].Text;
                                        break;
                                    }
                                }
                                catch { }
                                try
                                {
                                    if (TDBuilding.Count != 0 && TDBuilding[2].Text.Contains("Year Built"))
                                    {
                                        YearBuild = TDBuilding[3].Text;
                                        break;
                                    }
                                }
                                catch { }
                                try
                                {
                                    if (TDBuilding.Count != 0 && TDBuilding[4].Text.Contains("Year Built"))
                                    {
                                        YearBuild = TDBuilding[5].Text;
                                        break;
                                    }
                                }
                                catch { }
                            }
                        }
                        catch
                        {
                            YearBuild = "";
                        }
                        string property2 = ParcelValues.TrimEnd('~') + "~" + YearBuild;
                        gc.insert_date(orderNumber, parcelNumber, 2174, property2, 1, DateTime.Now);
                        //Property Adddress~Property Use~Primary Use~Unique ID~Map Block Lot~Acres~490 Acres~Zone~Volume/Page~Developers Map/Lot~Census~Year Built
                        string property1 = ParcelValuesHeader + "Year Built";
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property1 + "' where Id = '" + 2174 + "'");
                        try
                        {
                            driver.FindElement(By.LinkText("Sales")).Click();
                            gc.CreatePdf(orderNumber, parcelNumber, "Sales Search Result", driver, "CT", countynameCT);
                            IWebElement Sales = driver.FindElement(By.XPath("//*[@id='tabSales']/table/tbody"));
                            IList<IWebElement> TRSales = Sales.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDSales;
                            foreach (IWebElement SaleInfo in TRSales)
                            {
                                TDSales = SaleInfo.FindElements(By.TagName("td"));
                                if (TDSales.Count != 0)
                                {
                                    string salesdetails = TDSales[0].Text + "~" + TDSales[1].Text + "~" + TDSales[2].Text + "~" + TDSales[3].Text + "~" + TDSales[4].Text + "~" + TDSales[5].Text + "~" + TDSales[6].Text;
                                    gc.insert_date(orderNumber, parcelNumber, 2177, salesdetails, 1, DateTime.Now);
                                    //Owner Name~Volume~Page~Sale Date~Deed Type~Valid Sale~Sale Price
                                }
                            }
                            //
                            string property9 = "Owner Name~Volume~Page~Sale Date~Deed Type~Valid Sale~Sale Price";
                            db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property9 + "' where Id = '" + 2177 + "'");
                        }
                        catch { }
                        try
                        {
                            IWebElement IAddressSearch3 = driver.FindElement(By.LinkText("Outbuildings"));
                            IJavaScriptExecutor js3 = driver as IJavaScriptExecutor;
                            js3.ExecuteScript("arguments[0].click();", IAddressSearch3);
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Out Building Result", driver, "CT", countynameCT);
                        }
                        catch { }
                        try
                        {
                            IWebElement IAddressSearch4 = driver.FindElement(By.LinkText("Permits"));
                            IJavaScriptExecutor js4 = driver as IJavaScriptExecutor;
                            js4.ExecuteScript("arguments[0].click();", IAddressSearch4);
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Permits Result", driver, "CT", countynameCT);
                        }
                        catch { }


                        if (townshipcode == "05" || townshipcode == "07" || townshipcode == "08" || townshipcode == "24" || townshipcode == "20")
                        {
                            uniqueidMap = parcelNumber;

                        }
                        assessment_id = uniqueidMap;
                    }
                    #endregion

                    #region five Assessment Link
                    if (countAssess == "5") //
                    {
                        if (townshipcode != "25")
                        {
                            gc.CreatePdf_WOP(orderNumber, "Site Load", driver, "CT", countynameCT);
                            string Gis = "", accountno = "", parcelid = "", owner = "", location = "", mailingaddress = "", uniqueId = "";
                            string Parcel_ID = "", AssessedResult = "", AppraisedValue = "", AssessedValue = "", salesresult = "", salesinformation = "";
                            IWebElement propertyDet = driver.FindElement(By.XPath("/html/body/table[1]/tbody"));
                            IList<IWebElement> propertyRow = propertyDet.FindElements(By.TagName("tr"));
                            IList<IWebElement> propertyTD;
                            foreach (IWebElement line in propertyRow)
                            {
                                propertyTD = line.FindElements(By.TagName("td"));
                                if (propertyTD.Count != 0 && line.Text.Contains("GIS ID"))
                                {
                                    Gis = propertyTD[0].Text.Replace("GIS ID\r\n", "");
                                }
                                if (propertyTD.Count != 0 && line.Text.Contains("Parcel ID"))
                                {
                                    parcelid = propertyTD[0].Text.Replace("Parcel ID\r\n", "");
                                    parcelNumber = parcelid;
                                }
                                if (propertyTD.Count != 0 && line.Text.Contains("Mblu"))
                                {
                                    parcelid = propertyTD[0].Text.Replace("Mblu\r\n", "");
                                    parcelNumber = parcelid;
                                }
                                if (propertyTD.Count != 0 && line.Text.Contains("Unique ID"))
                                {
                                    uniqueId = propertyTD[0].Text.Replace("Unique ID\r\n", "");
                                    parcelNumber = uniqueId;

                                }
                                if (propertyTD.Count != 0 && line.Text.Contains("Account Number"))
                                {
                                    accountno = propertyTD[0].Text.Replace("Account Number\r\n", "");
                                }
                                if (propertyTD.Count != 0 && line.Text.Contains("Acct#"))
                                {
                                    accountno = propertyTD[0].Text.Replace("Acct#\r\n", "");
                                }
                                if (propertyTD.Count != 0 && line.Text.Contains("Owner"))
                                {
                                    owner = propertyTD[0].Text.Replace("Owner\r\n", "");
                                    ownername = owner;
                                }
                                if (propertyTD.Count != 0 && line.Text.Contains("Location"))
                                {
                                    location = propertyTD[0].Text.Replace("Location\r\n", "");
                                }
                                if (propertyTD.Count != 0 && line.Text.Contains("MAILING ADDRESS"))
                                {
                                    mailingaddress = propertyTD[0].Text.Replace("MAILING ADDRESS\r\n", "").Replace("\r\n", " ");
                                }

                            }
                            string[] splitAddress = location.Split(' ');
                            streetno1 = splitAddress[0];
                            if (splitAddress.Count() == 2)
                            {
                                streetname1 = splitAddress[1];
                                streetname1 = streetname1.Trim();
                            }

                            if (splitAddress.Count() == 3)
                            {
                                streetname1 = splitAddress[1] + " " + splitAddress[2];
                                streetname1 = streetname1.Trim();
                            }

                            if (splitAddress.Count() == 4)
                            {
                                streetname1 = splitAddress[1] + " " + splitAddress[2] + " " + splitAddress[3];
                                streetname1 = streetname1.Trim();
                            }

                            if (splitAddress.Count() == 5)
                            {
                                streetname1 = splitAddress[1] + " " + splitAddress[2] + " " + splitAddress[3] + " " + splitAddress[4];
                                streetname1 = streetname1.Trim();
                            }
                            address = streetno1 + " " + streetname1;
                            IWebElement Parcelvaluationtable = driver.FindElement(By.XPath("/html/body/table[3]/tbody"));
                            IList<IWebElement> Parcelvaluationrow = Parcelvaluationtable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Parcelvaluationid;
                            foreach (IWebElement Parcelvaluation in Parcelvaluationrow)
                            {
                                Parcelvaluationid = Parcelvaluation.FindElements(By.TagName("td"));
                                if (Parcelvaluationid.Count != 0)
                                {
                                    AssessedResult += Parcelvaluationid[0].Text + "~";
                                    AppraisedValue += Parcelvaluationid[1].Text + "~";
                                    AssessedValue += Parcelvaluationid[2].Text + "~";
                                }
                                //Buildings Appraised Value~Buildings Assessed Value
                            }
                            AssessedResult = "Assessment Info" + "~" + AssessedResult;
                            AppraisedValue = "Appraised Value" + "~" + AppraisedValue;
                            AssessedValue = "Assessed Value" + "~" + AssessedValue;

                            db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + AssessedResult.Remove(AssessedResult.Length - 1, 1) + "' where Id = '" + 2176 + "'");
                            gc.insert_date(orderNumber, parcelNumber, 2176, AppraisedValue.Remove(AppraisedValue.Length - 1, 1), 1, DateTime.Now);
                            gc.insert_date(orderNumber, parcelNumber, 2176, AssessedValue.Remove(AssessedValue.Length - 1, 1), 1, DateTime.Now);

                            string property = "", information = "";
                            IWebElement Propertyinfotable = driver.FindElement(By.XPath("/html/body/table[5]/tbody"));
                            IList<IWebElement> Propertyinforow = Propertyinfotable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Propertyinfoid;
                            foreach (IWebElement Propertyinfo in Propertyinforow)
                            {
                                Propertyinfoid = Propertyinfo.FindElements(By.TagName("td"));
                                if (Propertyinfoid.Count != 0)
                                {
                                    property += Propertyinfoid[0].Text + "~";
                                    information += Propertyinfoid[1].Text + "~";
                                }
                            }
                            //Total Acres~Land Use
                            db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property.Remove(property.Length - 1, 1) + "' where Id = '" + 2177 + "'");
                            gc.insert_date(orderNumber, parcelNumber.Replace("Mblu", ""), 2177, information.Remove(information.Length - 1, 1), 1, DateTime.Now);


                            IWebElement Saleinfotable = driver.FindElement(By.XPath("/html/body/table[7]/tbody"));
                            IList<IWebElement> Saleinforow = Saleinfotable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Saleinfoid;
                            foreach (IWebElement Saleinfo in Saleinforow)
                            {
                                Saleinfoid = Saleinfo.FindElements(By.TagName("td"));
                                if (Saleinfoid.Count != 0)
                                {
                                    salesresult += Saleinfoid[0].Text + "~";
                                    salesinformation += Saleinfoid[1].Text + "~";
                                }
                                //Sale Date~Sale Price
                            }
                            db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + salesresult.Remove(salesresult.Length - 1, 1) + "' where Id = '" + 2178 + "'");
                            gc.insert_date(orderNumber, parcelNumber, 2178, salesinformation.Remove(salesinformation.Length - 1, 1), 1, DateTime.Now);
                            //gc.CreatePdf(orderNumber, parcelNumber, "property Details", driver, "CT", countynameCT);
                            //   hrefparcellink
                            // gc.downloadfile(hrefparcellink, orderNumber, parcelNumber, "aas", "CT", countynameCT);
                            if (townshipcode == "03")
                            {
                                uniqueidMap = uniqueId;
                                assessment_id = uniqueId;
                            }
                            if (townshipcode == "14")
                            {
                                uniqueidMap = accountno;
                                assessment_id = accountno;
                            }
                            if (townshipcode == "25")
                            {
                                uniqueidMap = accountno.TrimStart('0'); ;
                                assessment_id = accountno.TrimStart('0'); ;
                            }
                            try
                            {
                                IWebElement Equalitylink = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr[7]/td[2]/strong[2]")).FindElement(By.TagName("a"));
                                string Equlityhref = Equalitylink.GetAttribute("href");
                                driver.Navigate().GoToUrl(Equlityhref);
                                IWebElement Equalitytable = driver.FindElement(By.XPath("//*[@id='tabParcel']/div[1]/div/table/tbody"));
                                IList<IWebElement> Equalityrow = Equalitytable.FindElements(By.TagName("tr"));
                                IList<IWebElement> Equalitytd;
                                foreach (IWebElement Equality in Equalityrow)
                                {
                                    Equalitytd = Equality.FindElements(By.TagName("td"));
                                    if (Equalitytd.Count() != 0)
                                    {
                                        if (Equalitytd[0].Text.Trim() == "Acres:")
                                        {
                                            Acres = Equalitytd[1].Text.Trim();
                                        }
                                        if (Equalitytd[2].Text.Trim() == "Acres:")
                                        {
                                            Acres = Equalitytd[3].Text.Trim();
                                        }
                                        if (Equalitytd[4].Text.Trim() == "Acres:")
                                        {
                                            Acres = Equalitytd[5].Text.Trim();
                                        }
                                    }

                                }
                            }
                            catch { }
                            string Propetyresult = Gis + "~" + parcelid + "~" + accountno + "~" + owner + "~" + location + "~" + mailingaddress + "~" + Acres;
                            string property9 = "GIS ID~Parcel ID~Account Number~Owner~Property Address~Mailing Address~Acres";
                            db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property9 + "' where Id = '" + 2174 + "'");
                            gc.insert_date(orderNumber, parcelNumber, 2174, Propetyresult, 1, DateTime.Now);
                            string filename = "";

                            var chromeOptions = new ChromeOptions();
                            var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];
                            chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                            chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                            chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                            chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                            var driver1 = new ChromeDriver(chromeOptions);
                            Array.ForEach(Directory.GetFiles(@downloadDirectory), File.Delete);
                            try
                            {

                                driver1.Navigate().GoToUrl(hrefparcellink);
                                Thread.Sleep(6000);
                                try
                                {
                                    gc.CreatePdf(orderNumber, parcelNumber, "property card", driver1, "CT", countynameCT);
                                }
                                catch { }
                                filename = latestfilename();
                                gc.AutoDownloadFile(orderNumber, parcelNumber, countynameCT, "CT", filename);
                                Thread.Sleep(2000);
                                driver1.Quit();
                            }
                            catch
                            {
                                driver1.Quit();
                            }

                            if (townshipcode == "01" || townshipcode == "06")
                            {
                                try
                                {
                                    string FilePath = gc.filePath(orderNumber, parcelNumber) + filename;
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
                                    string tableassess = gc.Between(pdftext, "Property Listing Report", "Property Information").Trim();
                                    string[] propid = tableassess.Split(' ');
                                    int arrayLength = propid.Length;
                                    uniqueidMap = propid[arrayLength - 1];
                                    assessment_id = uniqueidMap;

                                    uniqueidMap = uniqueidMap.PadLeft(uniqueidMap.Length + 8, '0');
                                    assessment_id = uniqueidMap;
                                }
                                catch { }
                            }
                        }
                        else
                        {
                            gc.CreatePdf_WOP(orderNumber, "Site Load", chromedriver, "CT", countynameCT);
                            string Gis = "", accountno = "", parcelid = "", owner = "", location = "", mailingaddress = "", uniqueId = "";
                            string Parcel_ID = "", AssessedResult = "", AppraisedValue = "", AssessedValue = "", salesresult = "", salesinformation = "";
                            IWebElement propertyDet = chromedriver.FindElement(By.XPath("/html/body/table[1]/tbody"));
                            IList<IWebElement> propertyRow = propertyDet.FindElements(By.TagName("tr"));
                            IList<IWebElement> propertyTD;
                            foreach (IWebElement line in propertyRow)
                            {
                                propertyTD = line.FindElements(By.TagName("td"));
                                if (propertyTD.Count != 0 && line.Text.Contains("GIS ID"))
                                {
                                    Gis = propertyTD[0].Text.Replace("GIS ID\r\n", "");
                                }
                                if (propertyTD.Count != 0 && line.Text.Contains("Parcel ID"))
                                {
                                    parcelid = propertyTD[0].Text.Replace("Parcel ID\r\n", "");
                                    parcelNumber = parcelid;
                                }
                                if (propertyTD.Count != 0 && line.Text.Contains("Mblu"))
                                {
                                    parcelid = propertyTD[0].Text.Replace("Mblu\r\n", "");
                                    parcelNumber = parcelid;
                                }
                                if (propertyTD.Count != 0 && line.Text.Contains("Unique ID"))
                                {
                                    uniqueId = propertyTD[0].Text.Replace("Unique ID\r\n", "");
                                    parcelNumber = uniqueId;

                                }
                                if (propertyTD.Count != 0 && line.Text.Contains("Account Number"))
                                {
                                    accountno = propertyTD[0].Text.Replace("Account Number\r\n", "");
                                }
                                if (propertyTD.Count != 0 && line.Text.Contains("Acct#"))
                                {
                                    accountno = propertyTD[0].Text.Replace("Acct#\r\n", "");
                                }
                                if (propertyTD.Count != 0 && line.Text.Contains("Owner"))
                                {
                                    owner = propertyTD[0].Text.Replace("Owner\r\n", "");
                                    ownername = owner;
                                }
                                if (propertyTD.Count != 0 && line.Text.Contains("Location"))
                                {
                                    location = propertyTD[0].Text.Replace("Location\r\n", "");
                                }
                                if (propertyTD.Count != 0 && line.Text.Contains("MAILING ADDRESS"))
                                {
                                    mailingaddress = propertyTD[0].Text.Replace("MAILING ADDRESS\r\n", "").Replace("\r\n", " ");
                                }

                            }
                            string[] splitAddress = location.Split(' ');
                            streetno1 = splitAddress[0];
                            if (splitAddress.Count() == 2)
                            {
                                streetname1 = splitAddress[1];
                                streetname1 = streetname1.Trim();
                            }

                            if (splitAddress.Count() == 3)
                            {
                                streetname1 = splitAddress[1] + " " + splitAddress[2];
                                streetname1 = streetname1.Trim();
                            }

                            if (splitAddress.Count() == 4)
                            {
                                streetname1 = splitAddress[1] + " " + splitAddress[2] + " " + splitAddress[3];
                                streetname1 = streetname1.Trim();
                            }

                            if (splitAddress.Count() == 5)
                            {
                                streetname1 = splitAddress[1] + " " + splitAddress[2] + " " + splitAddress[3] + " " + splitAddress[4];
                                streetname1 = streetname1.Trim();
                            }
                            address = streetno1 + " " + streetname1;
                            IWebElement Parcelvaluationtable = chromedriver.FindElement(By.XPath("/html/body/table[3]/tbody"));
                            IList<IWebElement> Parcelvaluationrow = Parcelvaluationtable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Parcelvaluationid;
                            foreach (IWebElement Parcelvaluation in Parcelvaluationrow)
                            {
                                Parcelvaluationid = Parcelvaluation.FindElements(By.TagName("td"));
                                if (Parcelvaluationid.Count != 0)
                                {
                                    AssessedResult += Parcelvaluationid[0].Text + "~";
                                    AppraisedValue += Parcelvaluationid[1].Text + "~";
                                    AssessedValue += Parcelvaluationid[2].Text + "~";
                                }
                                //Buildings Appraised Value~Buildings Assessed Value
                            }
                            AssessedResult = "Assessment Info" + "~" + AssessedResult;
                            AppraisedValue = "Appraised Value" + "~" + AppraisedValue;
                            AssessedValue = "Assessed Value" + "~" + AssessedValue;

                            db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + AssessedResult.Remove(AssessedResult.Length - 1, 1) + "' where Id = '" + 2176 + "'");
                            gc.insert_date(orderNumber, parcelNumber, 2176, AppraisedValue.Remove(AppraisedValue.Length - 1, 1), 1, DateTime.Now);
                            gc.insert_date(orderNumber, parcelNumber, 2176, AssessedValue.Remove(AssessedValue.Length - 1, 1), 1, DateTime.Now);

                            string property = "", information = "";
                            IWebElement Propertyinfotable = chromedriver.FindElement(By.XPath("/html/body/table[5]/tbody"));
                            IList<IWebElement> Propertyinforow = Propertyinfotable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Propertyinfoid;
                            foreach (IWebElement Propertyinfo in Propertyinforow)
                            {
                                Propertyinfoid = Propertyinfo.FindElements(By.TagName("td"));
                                if (Propertyinfoid.Count != 0)
                                {
                                    property += Propertyinfoid[0].Text + "~";
                                    information += Propertyinfoid[1].Text + "~";
                                }
                            }
                            //Total Acres~Land Use
                            db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property.Remove(property.Length - 1, 1) + "' where Id = '" + 2177 + "'");
                            gc.insert_date(orderNumber, parcelNumber.Replace("Mblu", ""), 2177, information.Remove(information.Length - 1, 1), 1, DateTime.Now);


                            IWebElement Saleinfotable = chromedriver.FindElement(By.XPath("/html/body/table[7]/tbody"));
                            IList<IWebElement> Saleinforow = Saleinfotable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Saleinfoid;
                            foreach (IWebElement Saleinfo in Saleinforow)
                            {
                                Saleinfoid = Saleinfo.FindElements(By.TagName("td"));
                                if (Saleinfoid.Count != 0)
                                {
                                    salesresult += Saleinfoid[0].Text + "~";
                                    salesinformation += Saleinfoid[1].Text + "~";
                                }
                                //Sale Date~Sale Price
                            }
                            db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + salesresult.Remove(salesresult.Length - 1, 1) + "' where Id = '" + 2178 + "'");
                            gc.insert_date(orderNumber, parcelNumber, 2178, salesinformation.Remove(salesinformation.Length - 1, 1), 1, DateTime.Now);
                            //gc.CreatePdf(orderNumber, parcelNumber, "property Details", driver, "CT", countynameCT);
                            //   hrefparcellink
                            // gc.downloadfile(hrefparcellink, orderNumber, parcelNumber, "aas", "CT", countynameCT);
                            if (townshipcode == "03")
                            {
                                uniqueidMap = uniqueId;
                                assessment_id = uniqueId;
                            }
                            if (townshipcode == "14")
                            {
                                uniqueidMap = accountno;
                                assessment_id = accountno;
                            }
                            if (townshipcode == "25")
                            {
                                uniqueidMap = accountno.TrimStart('0'); ;
                                assessment_id = accountno.TrimStart('0'); ;
                            }
                            try
                            {
                                IWebElement Equalitylink = chromedriver.FindElement(By.XPath("/html/body/table[1]/tbody/tr[7]/td[2]/strong[2]")).FindElement(By.TagName("a"));
                                string Equlityhref = Equalitylink.GetAttribute("href");
                                chromedriver.Navigate().GoToUrl(Equlityhref);
                                IWebElement Equalitytable = chromedriver.FindElement(By.XPath("//*[@id='tabParcel']/div[1]/div/table/tbody"));
                                IList<IWebElement> Equalityrow = Equalitytable.FindElements(By.TagName("tr"));
                                IList<IWebElement> Equalitytd;
                                foreach (IWebElement Equality in Equalityrow)
                                {
                                    Equalitytd = Equality.FindElements(By.TagName("td"));
                                    if (Equalitytd.Count() != 0)
                                    {
                                        if (Equalitytd[0].Text.Trim() == "Acres:")
                                        {
                                            Acres = Equalitytd[1].Text.Trim();
                                        }
                                        if (Equalitytd[2].Text.Trim() == "Acres:")
                                        {
                                            Acres = Equalitytd[3].Text.Trim();
                                        }
                                        if (Equalitytd[4].Text.Trim() == "Acres:")
                                        {
                                            Acres = Equalitytd[5].Text.Trim();
                                        }
                                    }

                                }
                            }
                            catch { }
                            string Propetyresult = Gis + "~" + parcelid + "~" + accountno + "~" + owner + "~" + location + "~" + mailingaddress + "~" + Acres;
                            string property9 = "GIS ID~Parcel ID~Account Number~Owner~Property Address~Mailing Address~Acres";
                            db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property9 + "' where Id = '" + 2174 + "'");
                            gc.insert_date(orderNumber, parcelNumber, 2174, Propetyresult, 1, DateTime.Now);
                            string filename = "";

                            var chromeOptions = new ChromeOptions();
                            var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];
                            chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                            chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                            chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                            chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                            var driver1 = new ChromeDriver(chromeOptions);
                            Array.ForEach(Directory.GetFiles(@downloadDirectory), File.Delete);
                            try
                            {

                                driver1.Navigate().GoToUrl(hrefparcellink);
                                Thread.Sleep(6000);
                                try
                                {
                                    gc.CreatePdf(orderNumber, parcelNumber, "property card", driver, "CT", countynameCT);
                                }
                                catch { }
                                filename = latestfilename();
                                gc.AutoDownloadFile(orderNumber, parcelNumber, countynameCT, "CT", filename);
                                Thread.Sleep(2000);
                                driver1.Quit();
                            }
                            catch
                            {
                                driver1.Quit();
                            }

                            if (townshipcode == "01" || townshipcode == "06")
                            {
                                try
                                {
                                    string FilePath = gc.filePath(orderNumber, parcelNumber) + filename;
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
                                    string tableassess = gc.Between(pdftext, "Property Listing Report", "Property Information").Trim();
                                    string[] propid = tableassess.Split(' ');
                                    int arrayLength = propid.Length;
                                    uniqueidMap = propid[arrayLength - 1];
                                    assessment_id = uniqueidMap;
                                    uniqueidMap = uniqueidMap.PadLeft(uniqueidMap.Length + 8, '0');
                                    assessment_id = uniqueidMap;
                                }
                                catch { }
                            }
                        }

                    }
                    #endregion
                    #region seven Assessment Link
                    if (countAssess == "7")//Propertydata
                    {
                        string OwnerName = "", PropertyLocation = "", PropertyType = "", ZoneType = "", Acreage = "", parcelnumber = "", Marketvalue = "", Assessment = "";

                        driver.FindElement(By.XPath("//*[@id='example']/tbody/tr")).Click();
                        Thread.Sleep(5000);
                        gc.CreatePdf_WOP(orderNumber, "Address search Result", driver, "CT", countynameCT);

                        OwnerName = driver.FindElement(By.Id("OWNER")).Text;
                        PropertyLocation = driver.FindElement(By.Id("ADDRESS")).Text;
                        PropertyType = driver.FindElement(By.Id("PROPERTYTYPE")).Text;
                        ZoneType = driver.FindElement(By.Id("ZONING")).Text;
                        Acreage = driver.FindElement(By.Id("ACREAGE")).Text;
                        parcelnumber = driver.FindElement(By.Id("MBL")).Text;
                        Marketvalue = driver.FindElement(By.Id("MARKET_VALUE")).Text;
                        Assessment = driver.FindElement(By.Id("ASSESSMENT")).Text;
                        string propertydetails = OwnerName + "~" + PropertyLocation + "~" + PropertyType + "~" + ZoneType + "~" + Acreage + "~" + Marketvalue + "~" + Assessment;
                        string PropertyHeading = "Owner Name~ Property Location~Property Type~ Zone Type~Acreage~Market Value~Assessment";
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + PropertyHeading + "' where Id = '" + 2174 + "'");
                        gc.insert_date(orderNumber, parcelnumber, 2174, propertydetails, 1, DateTime.Now);

                        string filename = "";
                        // driver.SwitchTo().Window(driver.WindowHandles.Last());
                        string current = driver.Url;
                        Thread.Sleep(2000);
                        var chromeOptions = new ChromeOptions();
                        var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];
                        chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                        chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                        chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                        chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                        var driver1 = new ChromeDriver(chromeOptions);
                        Array.ForEach(Directory.GetFiles(@downloadDirectory), File.Delete);
                        try
                        {

                            driver1.Navigate().GoToUrl(current);
                            Thread.Sleep(2000);

                            try
                            {
                                IWebElement parceldata = driver1.FindElement(By.Id("option-3"));
                                IJavaScriptExecutor js1 = driver1 as IJavaScriptExecutor;
                                js1.ExecuteScript("arguments[0].click();", parceldata);
                            }
                            catch { }

                            driver1.FindElement(By.Id("col2_filter")).SendKeys(parcelnumber);
                            Thread.Sleep(5000);


                            try
                            {
                                IWebElement linedata = driver1.FindElement(By.XPath("//*[@id='example']/tbody/tr"));
                                IJavaScriptExecutor jsline = driver1 as IJavaScriptExecutor;
                                jsline.ExecuteScript("arguments[0].click();", linedata);
                            }
                            catch { }
                            try
                            {
                                IWebElement linedata = driver1.FindElement(By.XPath("//*[@id='example']/tbody/tr/td[1]"));
                                IJavaScriptExecutor jsline = driver1 as IJavaScriptExecutor;
                                jsline.ExecuteScript("arguments[0].click();", linedata);
                            }
                            catch { }
                            Thread.Sleep(6000);

                            try
                            {
                                IWebElement parceldata11 = driver1.FindElement(By.Id("btnCard"));
                                IJavaScriptExecutor js11 = driver1 as IJavaScriptExecutor;
                                js11.ExecuteScript("arguments[0].click();", parceldata11);
                            }
                            catch { }
                            Thread.Sleep(5000);
                            filename = latestfilename();
                            gc.AutoDownloadFile(orderNumber, parcelnumber, countynameCT, "CT", filename);
                            Thread.Sleep(1000);
                            driver1.Quit();
                            uniqueidMap = filename.Replace(".pdf", "").Trim();
                            assessment_id = uniqueidMap;

                        }
                        catch { }

                    }
                    #endregion
                    #region eight Assessment Link
                    if (countAssess == "8")//Assessment
                    {
                        string uniqueId = "", Account = "", Owner = "", Location = "", MailingAddress = "";

                        // Property Details
                        string propertydetail = driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td[2]/div/font/table/tbody")).Text;
                        parcelNumber = gc.Between(propertydetail, "Parcel No", "Unique ID");
                        uniqueId = gc.Between(propertydetail, "Unique ID", "Account");
                        Account = gc.Between(propertydetail, "Account", "Owner");
                        Owner = gc.Between(propertydetail, "Owner", "Location");
                        Location = gc.Between(propertydetail, "Location", "MAILING ADDRESS");
                        MailingAddress = GlobalClass.After(propertydetail, "MAILING ADDRESS");
                        gc.CreatePdf(orderNumber, parcelNumber, "Assessment_Info", driver, "CT", countynameCT);

                        string filename = "";
                        string Yearbuilt1 = "";
                        string current = driver.Url;
                        var chromeOptions = new ChromeOptions();
                        var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];
                        chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                        chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                        chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                        chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                        var driver1 = new ChromeDriver(chromeOptions);
                        Array.ForEach(Directory.GetFiles(@downloadDirectory), File.Delete);

                        try
                        {
                            driver1.Navigate().GoToUrl(current);
                            Thread.Sleep(2000);
                            driver1.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td[2]/div/div[2]/font/input[2]")).Click();
                            Thread.Sleep(4000);
                            driver1.SwitchTo().Window(driver1.WindowHandles.Last());


                            filename = latestfilename();
                            gc.AutoDownloadFile(orderNumber, parcelNumber, countynameCT, "CT", filename);
                            Thread.Sleep(1000);

                        }
                        catch { }

                        try
                        {
                            string FilePath = gc.filePath(orderNumber, parcelNumber) + filename;
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


                            string tableyear = gc.Between(pdftext, "Year Built", "Building Desc.").Trim();
                            string[] YearBuilt = tableyear.Split(' ');
                            Yearbuilt1 = YearBuilt[0];
                            driver1.Quit();
                        }
                        catch { driver1.Quit(); }

                        //  uniqueId~Account~Owner~Location~MailingAddress
                        string propertydetails = uniqueId + "~" + Account + "~" + Owner + "~" + Location + "~" + MailingAddress + "~" + Yearbuilt1;
                        string PropertyHeading = "Unique ID~Account~Owner~Location~Mailing Address~Year Built";
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + PropertyHeading + "' where Id = '" + 2174 + "'");
                        gc.insert_date(orderNumber, parcelNumber, 2174, propertydetails, 1, DateTime.Now);

                        // Assessment Details

                        string valuetype = "", AppraisedValue = "", AssessedValue = "";
                        IWebElement Assessmentdetails = driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td[2]/div/table[1]/tbody"));
                        IList<IWebElement> TRAssessmentdetails = Assessmentdetails.FindElements(By.TagName("tr"));
                        IList<IWebElement> THAssessmentdetails = Assessmentdetails.FindElements(By.TagName("th"));
                        IList<IWebElement> TDAssessmentdetails;
                        foreach (IWebElement row in TRAssessmentdetails)
                        {
                            TDAssessmentdetails = row.FindElements(By.TagName("td"));
                            if (TDAssessmentdetails.Count != 0 && !row.Text.Contains("Appraised Value") && row.Text.Trim() != "" && TDAssessmentdetails.Count == 3)
                            {
                                valuetype += TDAssessmentdetails[0].Text + "~";
                                AppraisedValue += TDAssessmentdetails[1].Text + "~";
                                AssessedValue += TDAssessmentdetails[2].Text + "~";

                            }
                        }


                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + valuetype.Remove(valuetype.Length - 1, 1) + "' where Id = '" + 2176 + "'");
                        gc.insert_date(orderNumber, parcelNumber, 2176, AppraisedValue.Remove(AppraisedValue.Length - 1, 1), 1, DateTime.Now);
                        gc.insert_date(orderNumber, parcelNumber, 2176, AssessedValue.Remove(AssessedValue.Length - 1, 1), 1, DateTime.Now);


                        // valuation History

                        string Bulkdata = "", Land_Acres = "", Land_Use = "", Land_Class = "", Zoning = "", Neighborhood = "", Lot_Description = "", Lot_Setting = "", Lot_Utilities = "", Street_Desc = "";

                        Bulkdata = driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td[2]/div/table[2]/tbody")).Text;
                        Land_Acres = gc.Between(Bulkdata, "Land Acres", "Land Use").Trim();
                        Land_Use = gc.Between(Bulkdata, "Land Use", "Land Class").Trim();
                        Land_Class = gc.Between(Bulkdata, "Land Class", "Zoning").Trim();
                        Zoning = gc.Between(Bulkdata, "Zoning", "Neighborhood").Trim();
                        Neighborhood = gc.Between(Bulkdata, "Neighborhood", "Lot Description").Trim();
                        Lot_Description = gc.Between(Bulkdata, "Lot Description", "Lot Setting").Trim();
                        Lot_Setting = gc.Between(Bulkdata, "Lot Setting", "Lot Utilities").Trim();
                        Lot_Utilities = gc.Between(Bulkdata, "Lot Utilities", "Street Description").Trim();
                        Street_Desc = GlobalClass.After(Bulkdata, "Street Description").Trim();

                        string Proinfo = "Land_Acres~Land_Use~Land_Class~Zoning~Neighborhood~Lot_Description~Lot_Setting~Lot_Utilities~Street_Desc";
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + Proinfo + "' where Id = '" + 2177 + "'");
                        string ValuationHistoryResult = Land_Acres + "~" + Land_Use + "~" + Land_Class + "~" + Zoning + "~" + Neighborhood + "~" + Lot_Description + "~" + Lot_Setting + "~" + Lot_Utilities + "~" + Street_Desc;
                        gc.insert_date(orderNumber, parcelNumber, 2177, ValuationHistoryResult, 1, DateTime.Now);

                        // Sales Information

                        string Sale_Date = "", Sale_Price = "", Book = "";
                        IWebElement SalesInformationTable = driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td[2]/div/table[3]/tbody"));
                        IList<IWebElement> TRSalesInformation = SalesInformationTable.FindElements(By.TagName("tr"));
                        // IList<IWebElement> THAssessmentdetails = Assessmentdetails.FindElements(By.TagName("th"));
                        IList<IWebElement> TDSalesInformation;
                        foreach (IWebElement SalesInformation in TRSalesInformation)
                        {
                            TDSalesInformation = SalesInformation.FindElements(By.TagName("td"));
                            if (TDSalesInformation.Count != 0 && !SalesInformation.Text.Contains("Appraised Value") && SalesInformation.Text.Trim() != "")
                            {
                                Sale_Date += TDSalesInformation[0].Text + "~";
                                Sale_Price += TDSalesInformation[1].Text + "~";
                            }
                        }
                        //  Sale_Date~Sale_Price~Book/Page
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + Sale_Date.Remove(Sale_Date.Length - 1) + "' where Id = '" + 2178 + "'");
                        string SaleInformation = Sale_Price;
                        gc.insert_date(orderNumber, parcelNumber, 2178, Sale_Price.Remove(Sale_Price.Length - 1), 1, DateTime.Now);
                        uniqueidMap = Account;
                        assessment_id = uniqueidMap;
                    }
                    #endregion
                    #region fourteen Assessment Link
                    if (countAssess == "14")//Assessment
                    {
                        //driver.SwitchTo().Window(driver.WindowHandles.Last());
                        string propertyaddress = "", maplot = "", Owner = "", Mailingaddress = "", YearBuilt = "";
                        string streetno11 = driver.FindElement(By.Id("FormView1_StreetNumberLabel")).Text.Replace("Address:", "").Trim();
                        string streetname11 = driver.FindElement(By.Id("FormView1_StreetNameLabel")).Text;
                        propertyaddress = streetno11 + " " + streetname11;
                        maplot = driver.FindElement(By.Id("FormView1_lblParcelID")).Text.Replace("Map/Lot:", "").Trim();
                        Owner = driver.FindElement(By.Id("frmOwnerInformation_CurrentOwnerLabel")).Text;
                        string Mailaddress1 = driver.FindElement(By.Id("frmOwnerInformation_Label5")).Text;
                        string Mailaddress2 = driver.FindElement(By.Id("frmOwnerInformation_Label9")).Text;
                        string Mailaddress3 = driver.FindElement(By.Id("frmOwnerInformation_Label11")).Text;
                        string Mailaddress4 = driver.FindElement(By.Id("frmOwnerInformation_Label13")).Text;
                        Mailingaddress = Mailaddress1 + " " + Mailaddress2 + " " + Mailaddress3 + " " + Mailaddress4;
                        try
                        {
                            YearBuilt = driver.FindElement(By.XPath("//*[@id='grdBuild1']/tbody/tr[2]/td[5]")).Text;
                        }
                        catch { }
                        string taxdistrict = driver.FindElement(By.Id("FormView5_DistrictNumLabel")).Text;
                        string DistrictName = driver.FindElement(By.Id("FormView5_DistrictNameLabel")).Text;
                        string Districtmill = driver.FindElement(By.Id("FormView5_MillRateLabel")).Text;
                        gc.CreatePdf(orderNumber, maplot, "Assessment_Info", driver, "CT", countynameCT);
                        string Propertyresult = propertyaddress + "~" + Owner + "~" + Mailingaddress + "~" + YearBuilt + "~" + taxdistrict + "~" + DistrictName + "~" + Districtmill;
                        string Propertyheading = "Property Location~Owner Name~Mailing Address~Year Built~Tax District~District Name~District Mill Rate";
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + Propertyheading + "' where Id = '" + 2174 + "'");
                        gc.insert_date(orderNumber, maplot, 2174, Propertyresult, 1, DateTime.Now);
                        //Appraisal Information

                        // string appraisalhead = "";
                        IWebElement AppraisalInformationTable = driver.FindElement(By.Id("grdCurrentValues"));
                        IList<IWebElement> TRAppraisalInformation = AppraisalInformationTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDAppraisalInformation;
                        IList<IWebElement> THAppraisalInformation;
                        foreach (IWebElement AppraisalInformation in TRAppraisalInformation)
                        {
                            THAppraisalInformation = AppraisalInformation.FindElements(By.TagName("th"));
                            TDAppraisalInformation = AppraisalInformation.FindElements(By.TagName("td"));
                            if (AppraisalInformation.Text.Contains("Card"))
                            {
                                string Headingapprisal = THAppraisalInformation[0].Text + "~" + THAppraisalInformation[1].Text + "~" + THAppraisalInformation[2].Text + "~" + THAppraisalInformation[3].Text + "~" + THAppraisalInformation[4].Text + "~" + THAppraisalInformation[5].Text;
                                db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + Headingapprisal + "' where Id = '" + 2176 + "'");
                            }
                            if (TDAppraisalInformation.Count() != 0)
                            {
                                string Resultapprisal = TDAppraisalInformation[0].Text + "~" + TDAppraisalInformation[1].Text + "~" + TDAppraisalInformation[2].Text + "~" + TDAppraisalInformation[3].Text + "~" + TDAppraisalInformation[4].Text + "~" + TDAppraisalInformation[5].Text;
                                gc.insert_date(orderNumber, maplot, 2176, Resultapprisal, 1, DateTime.Now);
                            }
                        }
                        //Totall
                        IWebElement AppraisalInformationTable1 = driver.FindElement(By.Id("grdTotalValue"));
                        IList<IWebElement> TRAppraisalInformation1 = AppraisalInformationTable1.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDAppraisalInformation1;
                        // IList<IWebElement> THAppraisalInformation1;
                        foreach (IWebElement AppraisalInformation1 in TRAppraisalInformation1)
                        {
                            // THAppraisalInformation1 = AppraisalInformation1.FindElements(By.TagName("th"));
                            TDAppraisalInformation1 = AppraisalInformation1.FindElements(By.TagName("td"));
                            if (TDAppraisalInformation1.Count() != 0)
                            {
                                string Resultapprisal = "Total Parcel" + "~" + TDAppraisalInformation1[1].Text + "~" + TDAppraisalInformation1[2].Text + "~" + TDAppraisalInformation1[3].Text + "~" + TDAppraisalInformation1[4].Text + "~" + TDAppraisalInformation1[5].Text;
                                gc.insert_date(orderNumber, maplot, 2176, Resultapprisal, 1, DateTime.Now);
                            }
                        }
                        //Land Information
                        IWebElement LandInformationTable1 = driver.FindElement(By.Id("GridView2"));
                        IList<IWebElement> TRLandInformation = LandInformationTable1.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDLandInformation;
                        IList<IWebElement> THLandInformation;
                        foreach (IWebElement LandInformation in TRLandInformation)
                        {
                            THLandInformation = LandInformation.FindElements(By.TagName("th"));
                            TDLandInformation = LandInformation.FindElements(By.TagName("td"));
                            if (LandInformation.Text.Contains("Lot Size"))
                            {
                                string HeadingLandInformation = THLandInformation[0].Text + "~" + THLandInformation[1].Text + "~" + THLandInformation[2].Text + "~" + THLandInformation[3].Text;
                                db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + HeadingLandInformation + "' where Id = '" + 2177 + "'");
                            }
                            if (TDLandInformation.Count() != 0)
                            {
                                string ResultLandInformation = TDLandInformation[0].Text + "~" + TDLandInformation[1].Text + "~" + TDLandInformation[2].Text + "~" + TDLandInformation[3].Text;
                                gc.insert_date(orderNumber, maplot, 2177, ResultLandInformation, 1, DateTime.Now);
                            }
                        }
                        // string lantotal = driver.FindElement(By.Id("FormView7_TotalLandArea")).Text;

                        string Sale_Date = "", Sale_Price = "", Book = "", sale_head = "";
                        IWebElement SalesInformationTable = driver.FindElement(By.Id("gvSalesInformation"));
                        IList<IWebElement> TRSalesInformation = SalesInformationTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDSalesInformation;
                        IList<IWebElement> THSalesInformation;
                        foreach (IWebElement SalesInformation in TRSalesInformation)
                        {
                            TDSalesInformation = SalesInformation.FindElements(By.TagName("td"));
                            THSalesInformation = SalesInformation.FindElements(By.TagName("th"));
                            if (SalesInformation.Text.Contains("Book"))
                            {
                                sale_head = THSalesInformation[0].Text + "~" + THSalesInformation[1].Text + "~" + THSalesInformation[2].Text + "~" + THSalesInformation[3].Text + "~" + THSalesInformation[4].Text + "~" + THSalesInformation[5].Text;
                                db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + sale_head + "' where Id = '" + 2178 + "'");
                            }
                            if (TDSalesInformation.Count != 0 && !SalesInformation.Text.Contains("Book") && SalesInformation.Text.Trim() != "")
                            {
                                Sale_Date = TDSalesInformation[0].Text + "~" + TDSalesInformation[1].Text + "~" + TDSalesInformation[2].Text + "~" + TDSalesInformation[3].Text + "~" + TDSalesInformation[4].Text + "~" + TDSalesInformation[5].Text;
                                gc.insert_date(orderNumber, maplot, 2178, Sale_Date, 1, DateTime.Now);
                            }
                        }
                        uniqueidMap = maplot.Replace("-", "");
                        assessment_id = uniqueidMap;
                        streetno1 = streetno11.Trim();
                        streetname1 = streetname11.Trim();
                    }

                    #endregion
                    //Tax details
                    //Tax details
                    driver.Navigate().GoToUrl(urlTax);
                    #region Zero Tax Link
                    if (countTax == "0")//Bridgeport
                    {

                        if (townshipcode == "05" || townshipcode == "02" || townshipcode == "21" || townshipcode == "25" || townshipcode == "14" || townshipcode == "20" || townshipcode == "24" || townshipcode == "07" || townshipcode == "08" || townshipcode == "26" || townshipcode == "19" || townshipcode == "22" || townshipcode == "18" || townshipcode == "17" || townshipcode == "15" || townshipcode == "03" || townshipcode == "04" || townshipcode == "09" || townshipcode == "10" || townshipcode == "12")
                        {
                            IWebElement ITaxSelect = driver.FindElement(By.Id("actionType"));
                            SelectElement sTaxSelect = new SelectElement(ITaxSelect);
                            sTaxSelect.SelectByText("Unique ID");
                            driver.FindElement(By.XPath("//*[@id='uniqueid']/input[1]")).SendKeys(uniqueidMap);
                            driver.FindElement(By.Id("searchbtn4")).SendKeys(Keys.Enter);
                            Thread.Sleep(3000);

                        }
                        if (townshipcode == "11" || townshipcode == "27" || townshipcode == "01")
                        {
                            string taxaddress = "";
                            IWebElement ITaxSelect = driver.FindElement(By.Id("actionType"));
                            SelectElement sTaxSelect = new SelectElement(ITaxSelect);
                            sTaxSelect.SelectByText("Property Location");
                            driver.FindElement(By.Name("propertyNumber")).SendKeys(streetno1);
                            driver.FindElement(By.Name("propertyName")).SendKeys(streetname1.Trim().ToUpper());
                            driver.FindElement(By.Id("searchbtn2")).SendKeys(Keys.Enter);
                            Thread.Sleep(3000);
                        }
                        string BillNumber = "";
                        List<string> InformURL = new List<string>();
                        List<string> HistoryURL = new List<string>();
                        List<string> DownloadURL = new List<string>();
                        List<string> SewerTaxinfo = new List<string>();

                        try
                        {                                                 //*[@id="notification"]/div/span
                            string Nodata = driver.FindElement(By.XPath("//*[@id='notification']/div/span")).Text;
                            if (Nodata.Contains("No record is found") || Nodata.Contains("Searching criteria has not"))
                            {
                                if (townshipcode == "24")
                                {
                                    uniqueidMap = Regex.Replace(uniqueidMap, @"\s+", " ");
                                    IWebElement ITaxSelect = driver.FindElement(By.Id("actionType"));
                                    SelectElement sTaxSelect = new SelectElement(ITaxSelect);
                                    sTaxSelect.SelectByText("Unique ID");
                                    driver.FindElement(By.XPath("//*[@id='uniqueid']/input[1]")).Clear();
                                    driver.FindElement(By.XPath("//*[@id='uniqueid']/input[1]")).SendKeys(uniqueidMap);
                                    driver.FindElement(By.Id("searchbtn4")).SendKeys(Keys.Enter);
                                    Thread.Sleep(3000);
                                }
                                else
                                {
                                    HttpContext.Current.Session["NoTax_CT" + countynameCT] = "No_Tax";
                                    driver.Quit();
                                    return "No Data in Tax Site";
                                }
                            }
                        }
                        catch { }
                        try
                        {
                            string currentyear = Convert.ToString(DateTime.Now.Year - 1);
                            IWebElement ITaxClick = driver.FindElement(By.XPath("//*[@id='content']/div/div/div/form[2]/div/table/tbody"));
                            IList<IWebElement> ITaxClickRow = ITaxClick.FindElements(By.TagName("tr"));
                            IList<IWebElement> ITaxClickTD;
                            for (int i = 0; i < ITaxClickRow.Count; i++)
                            {
                                if (ITaxClickRow.Count() != 0)
                                {
                                    IList<IWebElement> ITaxClickTag;
                                    ITaxClickTD = ITaxClickRow[i].FindElements(By.TagName("td"));
                                    if (ITaxClickTD.Count != 0)
                                    {
                                        BillNumber = GlobalClass.Before(ITaxClickTD[0].Text, "\r\n");
                                        string splitreal1 = ITaxClickTD[0].Text.Replace("\r", "");
                                        string splitreal2 = splitreal1.Replace("\n", "");
                                        string[] splitreal = splitreal2.Split(' ');
                                        string[] Yearsplit = BillNumber.Split('-');
                                        string Yeartax = Yearsplit[0];

                                        if (Yeartax.Trim() == currentyear.Trim() && splitreal2.Trim().Contains("(REAL"))
                                        {
                                            ITaxClickTag = ITaxClickRow[i].FindElements(By.TagName("a"));
                                            foreach (IWebElement click in ITaxClickTag)
                                            {
                                                if (ITaxClickRow.Count() != 0)
                                                {
                                                    string strLink = click.GetAttribute("title");
                                                    if (strLink.Contains("Information on this account"))
                                                    {
                                                        InformURL.Add(click.GetAttribute("href"));
                                                    }
                                                    if (strLink.Contains("Tax Payment History"))
                                                    {
                                                        HistoryURL.Add(click.GetAttribute("href"));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch { }


                        gc.CreatePdf(orderNumber, assessment_id, "Tax Search Result", driver, "CT", countynameCT);

                        //Tax Bill 
                        IWebElement IBillDetails = driver.FindElement(By.XPath("//*[@id='content']/div/div/div/form[2]/div/table/tbody"));
                        IList<IWebElement> IBillDetailsRow = IBillDetails.FindElements(By.TagName("tr"));
                        IList<IWebElement> IBillDetailsTD;
                        foreach (IWebElement bill in IBillDetailsRow)
                        {
                            IBillDetailsTD = bill.FindElements(By.TagName("td"));
                            if (IBillDetailsTD.Count != 0 && !bill.Text.Contains("BILL"))
                            {
                                try
                                {
                                    string BillDetails = IBillDetailsTD[0].Text + "~" + IBillDetailsTD[1].Text + "~" + IBillDetailsTD[2].Text + "~" + IBillDetailsTD[3].Text + "~" + IBillDetailsTD[4].Text + "~" + IBillDetailsTD[5].Text;
                                    gc.insert_date(orderNumber, assessment_id, 2186, BillDetails, 1, DateTime.Now);
                                    //Bill~Name/Address~Property/Vehicle~Total Tax~Paid~Outstanding
                                }
                                catch { }
                            }
                        }
                        string tax1 = "Bill~Name/Address~Property/Vehicle~Total Tax~Paid~Outstanding";
                        dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + tax1 + "' where Id = '2186'");
                        try
                        {
                            driver.FindElement(By.Id("sewerMenu")).Click();
                            Thread.Sleep(3000);
                            gc.CreatePdf(orderNumber, assessment_id, "sewer Menu click", driver, "CT", countynameCT);
                            if (townshipcode == "05" || townshipcode == "02" || townshipcode == "21" || townshipcode == "25" || townshipcode == "14" || townshipcode == "20" || townshipcode == "24" || townshipcode == "07" || townshipcode == "08" || townshipcode == "26" || townshipcode == "19" || townshipcode == "22" || townshipcode == "18" || townshipcode == "17" || townshipcode == "15" || townshipcode == "03" || townshipcode == "04" || townshipcode == "09" || townshipcode == "10" || townshipcode == "12")
                            {
                                IWebElement ITaxSelect = driver.FindElement(By.Id("actionType"));
                                SelectElement sTaxSelect = new SelectElement(ITaxSelect);
                                sTaxSelect.SelectByText("Unique ID");
                                driver.FindElement(By.XPath("//*[@id='uniqueid']/input[1]")).SendKeys(uniqueidMap);
                                driver.FindElement(By.Id("searchbtn4")).SendKeys(Keys.Enter);
                                Thread.Sleep(3000);

                            }
                            if (townshipcode == "11" || townshipcode == "27" || townshipcode == "01")
                            {
                                string taxaddress = "";
                                IWebElement ITaxSelect = driver.FindElement(By.Id("actionType"));
                                SelectElement sTaxSelect = new SelectElement(ITaxSelect);
                                sTaxSelect.SelectByText("Property Location");
                                driver.FindElement(By.Name("propertyNumber")).SendKeys(streetno1);
                                driver.FindElement(By.Name("propertyName")).SendKeys(streetname1.Trim().ToUpper());
                                driver.FindElement(By.Id("searchbtn2")).SendKeys(Keys.Enter);
                                Thread.Sleep(3000);
                            }

                            gc.CreatePdf(orderNumber, assessment_id, "sewer Detail", driver, "CT", countynameCT);
                            IWebElement Sewerdetailtable;
                            try
                            {
                                Sewerdetailtable = driver.FindElement(By.XPath("//*[@id='content']/div/div/div/form[2]/center/div/table/tbody"));
                            }
                            catch
                            {
                                Sewerdetailtable = driver.FindElement(By.XPath("//*[@id='content']/div/div/div/center/div/table/tbody"));
                            }

                            IList<IWebElement> Sewerdetailrow = Sewerdetailtable.FindElements(By.TagName("tr"));
                            IList<IWebElement> swerdetailTD;
                            foreach (IWebElement Sewerdetail in Sewerdetailrow)
                            {
                                swerdetailTD = Sewerdetail.FindElements(By.TagName("td"));
                                if (swerdetailTD.Count() != 0)
                                {
                                    string Sewerdetailresult = swerdetailTD[0].Text + "~" + swerdetailTD[1].Text + "~" + swerdetailTD[2].Text + "~" + swerdetailTD[3].Text + "~" + swerdetailTD[4].Text;
                                    gc.insert_date(orderNumber, assessment_id, 2255, Sewerdetailresult, 1, DateTime.Now);
                                }
                            }
                            try
                            {
                                string currentyear = Convert.ToString(DateTime.Now.Year - 1);
                                IWebElement ITaxClick;
                                try
                                {
                                    ITaxClick = driver.FindElement(By.XPath("//*[@id='content']/div/div/div/form[2]/center/div/table/tbody"));
                                }
                                catch
                                {
                                    ITaxClick = driver.FindElement(By.XPath("//*[@id='content']/div/div/div/center/div/table/tbody"));
                                }
                                IList<IWebElement> ITaxClickRow = ITaxClick.FindElements(By.TagName("tr"));
                                IList<IWebElement> ITaxClickTD;
                                for (int i = 0; i < ITaxClickRow.Count; i++)
                                {
                                    if (ITaxClickRow.Count() != 0)
                                    {
                                        IList<IWebElement> ITaxClickTag;
                                        ITaxClickTD = ITaxClickRow[i].FindElements(By.TagName("td"));
                                        if (ITaxClickRow.Count != 0)
                                        {
                                            BillNumber = GlobalClass.Before(ITaxClickTD[0].Text, "\r\n");
                                            string[] Yearsplit = BillNumber.Split('-');
                                            string Yeartax = Yearsplit[0];
                                            ITaxClickTag = ITaxClickRow[i].FindElements(By.TagName("a"));
                                            if (Yeartax.Trim() == currentyear.Trim())
                                            {
                                                foreach (IWebElement click in ITaxClickTag)
                                                {
                                                    if (ITaxClickRow.Count() != 0)
                                                    {
                                                        string strLink = click.GetAttribute("title");
                                                        if (strLink.Contains("Information on this account"))
                                                        {
                                                            SewerTaxinfo.Add(click.GetAttribute("href"));
                                                        }
                                                        //if (strLink.Contains("Tax Payment History"))
                                                        //{
                                                        //    HistoryURL.Add(click.GetAttribute("href"));
                                                        //}
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch { }
                        }
                        catch { }
                        foreach (string Sewerinfo in SewerTaxinfo)
                        {
                            driver.Navigate().GoToUrl(Sewerinfo);
                            Thread.Sleep(3000);
                            gc.CreatePdf(orderNumber, assessment_id, "Sewer Information Result", driver, "CT", countynameCT);
                        }
                        foreach (string information in InformURL)
                        {
                            driver.Navigate().GoToUrl(information);
                            try
                            {
                                gc.CreatePdf(orderNumber, assessment_id, "Tax Information Result", driver, "CT", countynameCT);
                                //Tax Information
                                string TaxBill = "", GrossAssessment = "", UniqueID = "", Exemptions = "", District = "", NetAssessment = "", Name = "", TownMillRate = "", CareOf = "", PropertyLocation = "", MBL = "", TownBenefit = "", VolumePage = "", ElderlyBenefit = "";
                                IWebElement ITaxInformDetails = driver.FindElement(By.XPath("//*[@id='content']/div/div/div/div[1]/table[1]/tbody"));
                                IList<IWebElement> ITaxInformDetailsRow = ITaxInformDetails.FindElements(By.TagName("tr"));
                                IList<IWebElement> ITaxInformDetailsTD;
                                foreach (IWebElement inform in ITaxInformDetailsRow)
                                {
                                    ITaxInformDetailsTD = inform.FindElements(By.TagName("td"));
                                    if (ITaxInformDetailsTD.Count != 0 && inform.Text.Contains("Bill #") && inform.Text.Contains("Gross Assessment"))
                                    {
                                        TaxBill = ITaxInformDetailsTD[1].Text;
                                        GrossAssessment = ITaxInformDetailsTD[3].Text;
                                    }
                                    if (ITaxInformDetailsTD.Count != 0 && inform.Text.Contains("Unique ID") && inform.Text.Contains("Exemptions"))
                                    {
                                        UniqueID = ITaxInformDetailsTD[1].Text;
                                        Exemptions = ITaxInformDetailsTD[3].Text;
                                    }
                                    if (ITaxInformDetailsTD.Count != 0 && inform.Text.Contains("District") && inform.Text.Contains("Net Assessment"))
                                    {
                                        District = ITaxInformDetailsTD[1].Text;
                                        NetAssessment = ITaxInformDetailsTD[3].Text;
                                    }
                                    if (ITaxInformDetailsTD.Count != 0 && inform.Text.Contains("Name") && inform.Text.Contains("Town Mill Rate"))
                                    {
                                        Name = ITaxInformDetailsTD[1].Text;
                                        TownMillRate = ITaxInformDetailsTD[3].Text;
                                    }
                                    if (ITaxInformDetailsTD.Count != 0 && inform.Text.Contains("Care Of"))
                                    {
                                        CareOf = ITaxInformDetailsTD[1].Text;
                                    }
                                    if (ITaxInformDetailsTD.Count != 0 && inform.Text.Contains("Property Location"))
                                    {
                                        PropertyLocation = ITaxInformDetailsTD[1].Text;
                                    }
                                    if (ITaxInformDetailsTD.Count != 0 && inform.Text.Contains("MBL") && inform.Text.Contains("Town Benefit"))
                                    {
                                        MBL = ITaxInformDetailsTD[1].Text;
                                        TownBenefit = ITaxInformDetailsTD[3].Text;
                                    }
                                    if (ITaxInformDetailsTD.Count != 0 && inform.Text.Contains("Elderly Benefit (C)") && inform.Text.Contains("Volume & Page"))
                                    {
                                        VolumePage = ITaxInformDetailsTD[1].Text;
                                        ElderlyBenefit = ITaxInformDetailsTD[3].Text;
                                    }
                                }
                                string TaxInformation = TaxBill + "~" + GrossAssessment + "~" + UniqueID + "~" + Exemptions + "~" + District + "~" + NetAssessment + "~" + Name + "~" + TownMillRate + "~" + CareOf + "~" + PropertyLocation + "~" + MBL + "~" + TownBenefit + "~" + VolumePage + "~" + ElderlyBenefit;
                                gc.insert_date(orderNumber, assessment_id, 2187, TaxInformation, 1, DateTime.Now);
                                //Bill~Gross Assessment~Unique ID~Exemptions~District~Net Assessment~Name~Town Mill Rate~Care Of~Property Location~MBL~Town Benefit~Volume & Page~Elderly Benefit (C)

                                string tax2 = "Bill~Gross Assessment~Unique ID~Exemptions~District~Net Assessment~Name~Town Mill Rate~Care Of~Property Location~MBL~Town Benefit~Volume & Page~Elderly Benefit (C)";
                                dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + tax2 + "' where Id = '2187'");


                                string Propertyhead = "";
                                string Propertyresult = "";
                                int counttaxbill = 0;
                                try
                                {
                                    try
                                    {
                                        multitableElement1 = driver.FindElement(By.XPath("//*[@id='content']/div/div/div/div[1]/table[2]/tbody/tr/td[1]/table/tbody"));
                                    }
                                    catch { }
                                    try
                                    {
                                        multitableElement1 = driver.FindElement(By.XPath(" //*[@id='content']/div/div/div/div[1]/table[2]/tbody/tr/td[2]/table/tbody[2]"));
                                    }
                                    catch { }
                                    IList<IWebElement> multitableRow1 = multitableElement1.FindElements(By.TagName("tr"));
                                    IList<IWebElement> multirowTD1;
                                    // IList<IWebElement> multirowTH1;
                                    foreach (IWebElement row in multitableRow1)
                                    {
                                        //multirowTH1 = row.FindElements(By.TagName("tH"));
                                        multirowTD1 = row.FindElements(By.TagName("td"));
                                        if (!row.Text.Contains("Total payments"))
                                        {
                                            if (row.Text.Contains("Installment"))
                                            {
                                                for (int i = 0; i < multirowTD1.Count; i++)
                                                {

                                                    Propertyhead += multirowTD1[i].Text + "~";
                                                }
                                                Propertyhead = Propertyhead.TrimEnd('~');
                                                dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + Propertyhead + "' where Id = '2188'");

                                            }
                                            else
                                            {
                                                for (int i = 0; i < multirowTD1.Count; i++)
                                                {
                                                    Propertyresult += multirowTD1[i].Text + "~";
                                                }
                                                Propertyresult = Propertyresult.TrimEnd('~');
                                                gc.insert_date(orderNumber, assessment_id, 2188, Propertyresult, 1, DateTime.Now);
                                                Propertyresult = "";
                                                counttaxbill = multirowTD1.Count;
                                            }
                                        }
                                        else
                                        {
                                            for (int i = 0; i < counttaxbill; i++)
                                            {
                                                Propertyresult += multirowTD1[i].Text + "~";
                                            }
                                            Propertyresult = Propertyresult.TrimEnd('~');
                                            gc.insert_date(orderNumber, assessment_id, 2188, Propertyresult, 1, DateTime.Now);
                                            Propertyresult = "";

                                        }
                                    }
                                }
                                catch { }
                                //Tax Payment Details     

                                IWebElement IPaymentDetails;
                                try
                                {
                                    try
                                    {
                                        //*[@id="content"]/div/div/div/div[2]/table/tbody
                                        IPaymentDetails = driver.FindElement(By.XPath("//*[@id='content']/div/div/div/div[2]/table/tbody"));
                                    }
                                    catch
                                    {
                                        IPaymentDetails = driver.FindElement(By.XPath("//*[@id='content']/div/div/div/form[2]/div/table/tbody"));
                                    }
                                    IList<IWebElement> IPaymentDetailsRow = IPaymentDetails.FindElements(By.TagName("tr"));
                                    IList<IWebElement> IPaymentDetailsTD;
                                    foreach (IWebElement bill in IPaymentDetailsRow)
                                    {
                                        IPaymentDetailsTD = bill.FindElements(By.TagName("td"));
                                        if (IPaymentDetailsTD.Count != 0 && !bill.Text.Contains("PAY DATE"))
                                        {
                                            string PaymentDetails = IPaymentDetailsTD[0].Text + "~" + IPaymentDetailsTD[1].Text + "~" + IPaymentDetailsTD[2].Text + "~" + IPaymentDetailsTD[3].Text + "~" + IPaymentDetailsTD[4].Text + "~" + IPaymentDetailsTD[5].Text + "~" + IPaymentDetailsTD[6].Text;
                                            gc.insert_date(orderNumber, assessment_id, 2189, PaymentDetails, 1, DateTime.Now);
                                            //Pay Date~Type~Tax/Principal~Interest~Lien~Fee~Total
                                        }
                                    }
                                    string tax3 = "Pay Date~Type~Tax/Principal~Interest~Lien~Fee~Total";
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + tax3 + "' where Id = '2189'");
                                }
                                catch { }
                                try
                                {
                                    string strTotalPayment = GlobalClass.After(driver.FindElement(By.XPath("//*[@id='content']/div/div/div/center/div[1]")).Text, ":").Trim();
                                    string PaymentDetails = "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + strTotalPayment;

                                    gc.insert_date(orderNumber, assessment_id, 2189, PaymentDetails, 1, DateTime.Now);
                                    //Pay Date~Type~Tax/Principal~Interest~Lien~Fee~Total
                                }
                                catch { }

                                //Tax Total Due 
                                string TaxPrincBint = "", InterestDue = "", LienDue = "", FeeDue = "", TotalDue = "";
                                IWebElement IDueDetails = driver.FindElement(By.XPath("//*[@id='content']/div/div/div/div[1]/table[2]/tbody/tr/td[2]/table/tbody[2]"));
                                IList<IWebElement> IDueDetailsRow = IDueDetails.FindElements(By.TagName("tr"));
                                IList<IWebElement> IDueDetailsTD;
                                foreach (IWebElement due in IDueDetailsRow)
                                {
                                    IDueDetailsTD = due.FindElements(By.TagName("td"));
                                    if (IDueDetailsTD.Count != 0 && !due.Text.Contains("Tax/Princ/Bint Due"))
                                    {
                                        TaxPrincBint = IDueDetailsTD[1].Text;
                                    }
                                    if (IDueDetailsTD.Count != 0 && !due.Text.Contains("Interest Due"))
                                    {
                                        InterestDue = IDueDetailsTD[1].Text;
                                    }
                                    if (IDueDetailsTD.Count != 0 && !due.Text.Contains("Lien Due"))
                                    {
                                        LienDue = IDueDetailsTD[1].Text;
                                    }
                                    if (IDueDetailsTD.Count != 0 && !due.Text.Contains("Fee Due"))
                                    {
                                        FeeDue = IDueDetailsTD[1].Text;
                                    }
                                    if (IDueDetailsTD.Count != 0 && !due.Text.Contains("Tax/Princ/Bint Due"))
                                    {
                                        TotalDue = IDueDetailsTD[1].Text;
                                    }
                                }
                                string DueDetails = TaxPrincBint + "~" + InterestDue + "~" + LienDue + "~" + FeeDue + "~" + TotalDue;
                                gc.insert_date(orderNumber, assessment_id, 2190, DueDetails, 1, DateTime.Now);
                                //Tax/Princ/Bint Due~Interest Due~Lien Due~Fee Due~Total Due
                            }
                            catch { }
                            string tax5 = "Tax/Princ/Bint Due~Interest Due~Lien Due~Fee Due~Total Due";
                            dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + tax5 + "' where Id = '2190'");
                        }
                        foreach (string strhistory in HistoryURL)
                        {
                            driver.Navigate().GoToUrl(strhistory);
                            Thread.Sleep(2000);
                            try
                            {
                                //Tax Bill History
                                gc.CreatePdf(orderNumber, assessment_id, "Tax Bill History Result", driver, "CT", countynameCT);
                                IWebElement IBillHistoryDetails = driver.FindElement(By.XPath("//*[@id='content']/div/div/div/div/table/tbody"));
                                IList<IWebElement> IBillHistoryDetailsRow = IBillHistoryDetails.FindElements(By.TagName("tr"));
                                IList<IWebElement> IBillHistoryDetailsTD;
                                foreach (IWebElement billHistory in IBillHistoryDetailsRow)
                                {
                                    IBillHistoryDetailsTD = billHistory.FindElements(By.TagName("td"));
                                    if (IBillHistoryDetailsTD.Count != 0 && !billHistory.Text.Contains("BILL #"))
                                    {
                                        string BillHistoryDetails = IBillHistoryDetailsTD[0].Text + "~" + IBillHistoryDetailsTD[1].Text + "~" + IBillHistoryDetailsTD[2].Text + "~" + IBillHistoryDetailsTD[3].Text + "~" + IBillHistoryDetailsTD[4].Text + "~" + IBillHistoryDetailsTD[5].Text + "~" + IBillHistoryDetailsTD[6].Text + "~" + IBillHistoryDetailsTD[7].Text;
                                        gc.insert_date(orderNumber, assessment_id, 2191, BillHistoryDetails, 1, DateTime.Now);
                                        //Bill~Type~Paid Date~Tax~Interest~Lien~Fee~Total
                                    }
                                }
                            }
                            catch { }
                            string tax6 = "Bill~Type~Paid Date~Tax~Interest~Lien~Fee~Total";
                            dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + tax6 + "' where Id = '2191'");

                        }
                        int count = 0;
                        string filename = "";

                        var chromeOptions = new ChromeOptions();
                        var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];
                        chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                        chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                        chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                        chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                        var driver1 = new ChromeDriver(chromeOptions);
                        Array.ForEach(Directory.GetFiles(@downloadDirectory), File.Delete);
                        try
                        {

                            driver1.Navigate().GoToUrl(urlTax);
                            Thread.Sleep(2000);

                            if (townshipcode == "11" || townshipcode == "27" || townshipcode == "01")
                            {
                                IWebElement ITaxSelect = driver1.FindElement(By.Id("actionType"));
                                SelectElement sTaxSelect = new SelectElement(ITaxSelect);
                                sTaxSelect.SelectByText("Property Location");
                                driver1.FindElement(By.Name("propertyNumber")).SendKeys(streetno1);
                                driver1.FindElement(By.Name("propertyName")).SendKeys(streetname1.Trim().ToUpper());
                                driver1.FindElement(By.Id("searchbtn2")).SendKeys(Keys.Enter);
                                Thread.Sleep(3000);
                            }
                            else
                            {
                                IWebElement ITaxDownSelect = driver1.FindElement(By.Id("actionType"));
                                SelectElement sTaxDownSelect = new SelectElement(ITaxDownSelect);
                                sTaxDownSelect.SelectByText("Unique ID");
                                driver1.FindElement(By.XPath("//*[@id='uniqueid']/input[1]")).SendKeys(uniqueidMap);
                                driver1.FindElement(By.Id("searchbtn4")).SendKeys(Keys.Enter);
                                Thread.Sleep(3000);
                            }
                            try
                            {
                                string currentyear = Convert.ToString(DateTime.Now.Year - 1);
                                IWebElement ITaxDownloadClick = driver1.FindElement(By.XPath("//*[@id='content']/div/div/div/form[2]/div/table/tbody"));
                                IList<IWebElement> ITaxDownloadClickRow = ITaxDownloadClick.FindElements(By.TagName("tr"));
                                IList<IWebElement> ITaxDownloadClickTD;
                                for (int i = 0; i < ITaxDownloadClickRow.Count; i++)
                                {
                                    if (ITaxDownloadClickRow.Count() != 0)
                                    {
                                        IList<IWebElement> ITaxClickTag;
                                        ITaxDownloadClickTD = ITaxDownloadClickRow[i].FindElements(By.TagName("td"));
                                        if (ITaxDownloadClickTD.Count != 0)
                                        {
                                            BillNumber = GlobalClass.Before(ITaxDownloadClickTD[0].Text, "\r\n");
                                            string[] Billsplit = BillNumber.Split('-');
                                            string splitreal1 = ITaxDownloadClickTD[0].Text.Replace("\r", "");
                                            string splitreal2 = splitreal1.Replace("\n", "");
                                            string[] splitreal = splitreal2.Split(' ');
                                            string yearsplit = Billsplit[0];

                                            if (yearsplit.Trim() == currentyear.Trim() && splitreal2.Trim().Contains("(REAL"))
                                            {
                                                ITaxClickTag = ITaxDownloadClickRow[i].FindElements(By.TagName("a"));
                                                foreach (IWebElement click in ITaxClickTag)
                                                {
                                                    if (ITaxDownloadClickRow.Count() != 0)
                                                    {
                                                        string strLink = click.GetAttribute("title");
                                                        if (strLink.Contains("Download PDF") || strLink.Contains("View original tax bill"))
                                                        {
                                                            string Href = click.GetAttribute("href");
                                                            try
                                                            {
                                                                click.Click();
                                                            }
                                                            catch { driver1.Navigate().GoToUrl(Href); }
                                                            Thread.Sleep(20000);
                                                            filename = latestfilename();
                                                            gc.AutoDownloadFile(orderNumber, assessment_id, countynameCT, "CT", filename);
                                                            Thread.Sleep(7000);

                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch { }
                            try
                            {
                                driver1.Navigate().GoToUrl(urlTax);
                                Thread.Sleep(2000);
                                driver1.FindElement(By.Id("sewerMenu")).Click();
                                Thread.Sleep(2000);
                                if (townshipcode == "05" || townshipcode == "02" || townshipcode == "21" || townshipcode == "25" || townshipcode == "14" || townshipcode == "20" || townshipcode == "24" || townshipcode == "07" || townshipcode == "08" || townshipcode == "26" || townshipcode == "19" || townshipcode == "22" || townshipcode == "18" || townshipcode == "17" || townshipcode == "15" || townshipcode == "03" || townshipcode == "04" || townshipcode == "09" || townshipcode == "10" || townshipcode == "12")
                                {
                                    IWebElement ITaxSelect = driver.FindElement(By.Id("actionType"));
                                    SelectElement sTaxSelect = new SelectElement(ITaxSelect);
                                    sTaxSelect.SelectByText("Unique ID");
                                    driver.FindElement(By.XPath("//*[@id='uniqueid']/input[1]")).SendKeys(uniqueidMap);
                                    driver.FindElement(By.Id("searchbtn4")).SendKeys(Keys.Enter);
                                    Thread.Sleep(3000);

                                }
                                if (townshipcode == "11" || townshipcode == "27" || townshipcode == "01")
                                {
                                    string taxaddress = "";
                                    IWebElement ITaxSelect = driver.FindElement(By.Id("actionType"));
                                    SelectElement sTaxSelect = new SelectElement(ITaxSelect);
                                    sTaxSelect.SelectByText("Property Location");
                                    driver.FindElement(By.Name("propertyNumber")).SendKeys(streetno1);
                                    driver.FindElement(By.Name("propertyName")).SendKeys(streetname1.Trim().ToUpper());
                                    driver.FindElement(By.Id("searchbtn2")).SendKeys(Keys.Enter);
                                    Thread.Sleep(3000);
                                }
                                IWebElement ITaxDownloadClick = driver1.FindElement(By.XPath("//*[@id='content']/div/div/div/center/div/table/tbody"));
                                IList<IWebElement> ITaxDownloadClickRow = ITaxDownloadClick.FindElements(By.TagName("tr"));
                                IList<IWebElement> ITaxDownloadClickTD;
                                for (int i = 0; i < ITaxDownloadClickRow.Count; i++)
                                {
                                    if (ITaxDownloadClickRow.Count() - 1 == i)
                                    {
                                        IList<IWebElement> ITaxClickTag;
                                        ITaxDownloadClickTD = ITaxDownloadClickRow[i].FindElements(By.TagName("td"));
                                        if (ITaxDownloadClickTD.Count != 0)
                                        {
                                            BillNumber = GlobalClass.Before(ITaxDownloadClickTD[0].Text, "\r\n");
                                        }
                                        ITaxClickTag = ITaxDownloadClickRow[i].FindElements(By.TagName("a"));
                                        foreach (IWebElement click in ITaxClickTag)
                                        {
                                            if (ITaxDownloadClickRow.Count() != 0)
                                            {
                                                string strLink = click.GetAttribute("title");
                                                if (strLink.Contains("Download PDF") || strLink.Contains("View original tax bill"))
                                                {
                                                    click.Click();
                                                    Thread.Sleep(20000);
                                                    filename = latestfilename();
                                                    gc.AutoDownloadFile(orderNumber, assessment_id, countynameCT, "CT", filename);
                                                    Thread.Sleep(5000);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch { }
                        }
                        catch { driver1.Quit(); }
                        driver1.Quit();
                    }
                    #endregion
                    #region One Tax Link
                    if (countTax == "1") //Darien
                    {

                        driver.Navigate().GoToUrl(urlTax);
                        driver.FindElement(By.Id("searchName")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Result", driver, "CT", countynameCT);
                        driver.FindElement(By.XPath("//*[@id='search_form']/p[2]/input[2]")).Click();
                        Thread.Sleep(1000);
                        driver.FindElement(By.XPath("//*[@id='search_form']/p[2]/input[3]")).Click();
                        Thread.Sleep(1000);
                        string currentyear = Convert.ToString(DateTime.Now.Year - 1);
                        IWebElement Multiaddresstable1add = driver.FindElement(By.Id("resultsTable"));
                        IList<IWebElement> multiaddressrows = Multiaddresstable1add.FindElements(By.TagName("tr"));
                        IList<IWebElement> Multiaddressid;
                        foreach (IWebElement Multiaddress in multiaddressrows)
                        {
                            Multiaddressid = Multiaddress.FindElements(By.TagName("td"));
                            if (Multiaddressid[4].Text.Trim() == currentyear && Multiaddressid.Count == 8 && !Multiaddress.Text.Contains("Add") && Multiaddressid[3].Text.Contains("REAL ESTATE"))
                            {
                                IWebElement Singleclick = Multiaddressid[7].FindElement(By.TagName("button"));
                                Singleclick.Click();
                                Thread.Sleep(2000);
                                break;
                            }
                        }
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Result", driver, "CT", countynameCT);
                        //Current Tax Bill Information  Details
                        string BillDate = "", List1 = "", Year = "", Description = "", Type = "", FirstDueDate = "", SecondDueDate = "", FirstDueAmoungt = "", SecondDueAmount = "", TotalDueAmount = "", TotalPaid = "";
                        try
                        {
                            BillDate = gc.Between(driver.FindElement(By.XPath("//*[@id='blockName']/h4")).Text, "as of ", ":");
                        }
                        catch { }
                        IWebElement currenttaxinfo1 = driver.FindElement(By.Id("blockName"));
                        IList<IWebElement> TRcurrenttaxinfo1value1 = currenttaxinfo1.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDcurrenttaxinfo1value1;
                        foreach (IWebElement currenttax in TRcurrenttaxinfo1value1)
                        {
                            TDcurrenttaxinfo1value1 = currenttax.FindElements(By.TagName("td"));

                            if (TDcurrenttaxinfo1value1.Count == 4 && TDcurrenttaxinfo1value1[0].Text.Trim() != "" && TDcurrenttaxinfo1value1.Count != 0 && currenttax.Text.Trim() != "" && !currenttax.Text.Contains("List#"))
                            {
                                List1 = TDcurrenttaxinfo1value1[0].Text.Trim();
                                Year = TDcurrenttaxinfo1value1[1].Text.Trim();
                                Description = TDcurrenttaxinfo1value1[2].Text.Trim();
                                Type = TDcurrenttaxinfo1value1[3].Text.Trim();
                            }
                        }
                        int count = 0;
                        IWebElement currenttaxinfo2 = driver.FindElement(By.XPath("//*[@id='CurrentBill_detail']/table/tbody"));
                        IList<IWebElement> TRcurrenttaxinfo1value2 = currenttaxinfo2.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDcurrenttaxinfo1value2;
                        foreach (IWebElement currenttax1 in TRcurrenttaxinfo1value2)
                        {
                            TDcurrenttaxinfo1value2 = currenttax1.FindElements(By.TagName("td"));
                            if (TDcurrenttaxinfo1value2.Count == 2 && TDcurrenttaxinfo1value2[0].Text.Trim() != "" && TDcurrenttaxinfo1value2.Count != 0 && currenttax1.Text.Trim() != "" && currenttax1.Text.Contains("/") && count == 0)
                            {
                                FirstDueDate = TDcurrenttaxinfo1value2[0].Text.Trim();
                                FirstDueAmoungt = TDcurrenttaxinfo1value2[1].Text.Trim();
                                count++;
                            }
                            if (TDcurrenttaxinfo1value2.Count == 2 && TDcurrenttaxinfo1value2[0].Text.Trim() != "" && TDcurrenttaxinfo1value2.Count != 0 && currenttax1.Text.Trim() != "" && currenttax1.Text.Contains("/") && count == 1)
                            {
                                SecondDueDate = TDcurrenttaxinfo1value2[0].Text.Trim();
                                SecondDueAmount = TDcurrenttaxinfo1value2[1].Text.Trim();
                            }
                            if (TDcurrenttaxinfo1value2.Count == 2 && currenttax1.Text.Contains("Installments Total"))
                            {
                                TotalDueAmount = TDcurrenttaxinfo1value2[1].Text.Trim();
                            }
                            if (TDcurrenttaxinfo1value2.Count == 2 && currenttax1.Text.Contains("Total Paid"))
                            {
                                TotalPaid = TDcurrenttaxinfo1value2[1].Text.Trim();
                            }
                        }

                        string BillHistoryDetails = BillDate + "~" + List1 + "~" + Year + "~" + Description + "~" + Type + "~" + FirstDueDate + "~" + FirstDueAmoungt + "~" + SecondDueDate + "~" + SecondDueAmount + "~" + TotalDueAmount + "~" + TotalPaid;
                        gc.insert_date(orderNumber, parcelNumber, 2186, BillHistoryDetails, 1, DateTime.Now);
                        //BillDate~List~Year~Description~Type~First Installment Due Date~First Installment Due Amount~Second Installment Due Date~Second Installment Due Amount~Total Installment Amount~Total Paid
                        string tax1 = "BillDate~List~Year~Description~Type~First Installment Due Date~First Installment Due Amount~Second Installment Due Date~Second Installment Due Amount~Total Installment Amount~Total Paid";
                        dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + tax1 + "' where Id = '2186'");

                        //Current Balance Due
                        string title2 = "", value2 = "";
                        IWebElement currenttaxinfo3 = driver.FindElement(By.Id("blockTotal"));
                        IList<IWebElement> TRcurrenttaxinfo1value3 = currenttaxinfo3.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDcurrenttaxinfo1value3;
                        foreach (IWebElement currenttax2 in TRcurrenttaxinfo1value3)
                        {
                            TDcurrenttaxinfo1value3 = currenttax2.FindElements(By.TagName("td"));

                            if (TDcurrenttaxinfo1value3.Count == 2 && TDcurrenttaxinfo1value3[0].Text.Trim() != "" && TDcurrenttaxinfo1value3.Count != 0 && currenttax2.Text.Trim() != "")
                            {
                                title2 += TDcurrenttaxinfo1value3[0].Text.Trim() + "~";
                                value2 += TDcurrenttaxinfo1value3[1].Text.Trim() + "~";
                                //string PaymentHistorydetails = List + "~" + Principal + "~" + Interest + "~" + Lien + "~" + Penalty + "~" + Total + "~" + DatePaid;

                            }
                        }
                        title2 = title2.TrimEnd('~');
                        value2 = value2.TrimEnd('~');
                        //Current Bill Total~Tax Due~Interest Due~Fee Due~Bond~Lien~Total Due
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + title2.Remove(title2.Length - 1, 1) + "' where Id = '" + 2187 + "'");
                        gc.insert_date(orderNumber, parcelNumber, 2187, value2.Remove(value2.Length - 1, 1), 1, DateTime.Now);

                        string List = "", Principal = "", Interest = "", Lien = "", Penalty = "", Total = "", DatePaid = "";
                        //Payment History Details
                        try
                        {
                            IWebElement Paymentdet1 = driver.FindElement(By.Id("resultsTable"));
                            IList<IWebElement> TRPaymentvalue1 = Paymentdet1.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDPaymentvalue1;
                            foreach (IWebElement Payment in TRPaymentvalue1)
                            {
                                TDPaymentvalue1 = Payment.FindElements(By.TagName("td"));

                                if (TDPaymentvalue1.Count == 7 && TDPaymentvalue1[0].Text.Trim() != "" && TDPaymentvalue1.Count != 0 && Payment.Text.Trim() != "" && !Payment.Text.Contains("List #"))
                                {
                                    List = TDPaymentvalue1[0].Text.Trim();
                                    Principal = TDPaymentvalue1[1].Text.Trim();
                                    Interest = TDPaymentvalue1[2].Text.Trim();
                                    Lien = TDPaymentvalue1[3].Text.Trim();
                                    Penalty = TDPaymentvalue1[4].Text.Trim();
                                    Total = TDPaymentvalue1[5].Text.Trim();
                                    DatePaid = TDPaymentvalue1[6].Text.Trim();

                                    string PaymentHistorydetails = List + "~" + Principal + "~" + Interest + "~" + Lien + "~" + Penalty + "~" + Total + "~" + DatePaid;
                                    gc.insert_date(orderNumber, parcelNumber, 2188, PaymentHistorydetails, 1, DateTime.Now);
                                    //List~Principal~Interest~Lien~Penalty~Total
                                    string tax23 = "List~Principal~Interest~Lien~Penalty~Total";
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + tax1 + "' where Id = '2188'");

                                }
                            }
                        }
                        catch { }
                    }
                    #endregion

                    #region Tax Three
                    if (countTax == "3")
                    {
                        //string address = "";
                        //if (streetdir != "")
                        //{
                        //    address = streetno + " " + streetdir + " " + streetname + " " + streettype;
                        //}
                        //else
                        //{
                        //    address = streetno + " " + streetname + " " + streettype;
                        //}
                        //Thread.Sleep(3000);
                        if (address.Trim() == "")
                        {
                            address = streetno1 + " " + streetname1;
                        }
                        if (townshipcode != "06")
                        {
                            driver.FindElement(By.Id("LISTNUM")).SendKeys(parcelNumber);
                            gc.CreatePdf(orderNumber, parcelNumber, "Tax info", driver, "CT", countynameCT);
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr/td/table[3]/tbody/tr/td/table/tbody/tr/td[1]/table/tbody/tr[11]/td[1]/input")).SendKeys(Keys.Enter);

                        }
                        if (townshipcode == "06")
                        {
                            driver.FindElement(By.Id("ADD1")).SendKeys(address.Trim());
                            gc.CreatePdf(orderNumber, parcelNumber, "Tax info", driver, "CT", countynameCT);
                            driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr/td/table[3]/tbody/tr/td/table/tbody/tr/td[1]/table/tbody/tr[7]/td[1]/input")).SendKeys(Keys.Enter);
                        }
                        // driver.FindElement(By.Id("LISTNUM")).SendKeys(address.Trim());
                        //  
                        //  driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr/td/table[3]/tbody/tr/td/table/tbody/tr/td[1]/table/tbody/tr[11]/td[1]/input")).SendKeys(Keys.Enter);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax info det", driver, "CT", countynameCT);
                        List<string> TaxPaymentLink = new List<string>();
                        IWebElement IpropertyDetails = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td/table/tbody/tr/td/table/tbody/tr/td/table[3]/tbody/tr/td/table/tbody/tr/td/table/tbody"));
                        IList<IWebElement> IpropertyDetailsRow = IpropertyDetails.FindElements(By.TagName("tr"));
                        IList<IWebElement> IpropertyDetailsTD;
                        foreach (IWebElement property in IpropertyDetailsRow)
                        {
                            IpropertyDetailsTD = property.FindElements(By.TagName("td"));
                            if (IpropertyDetailsTD.Count != 0 && !property.Text.Contains("Last Name / Company") && IpropertyDetailsTD[1].Text.ToUpper().Contains(ownername.ToUpper()) && IpropertyDetailsTD[3].Text.Contains("REAL"))
                            {
                                IWebElement AddressClick = IpropertyDetailsTD[0].FindElement(By.TagName("a"));
                                if (AddressClick.Text.Contains("show details"))
                                {
                                    string link = AddressClick.GetAttribute("href");
                                    TaxPaymentLink.Add(link);
                                }
                            }
                        }

                        foreach (string URL in TaxPaymentLink)
                        {
                            driver.Navigate().GoToUrl(URL);
                            gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search" + GlobalClass.After(URL, "=").Trim(), driver, "CT", "Hartford");
                            //Tax Payment History Details
                            IWebElement IPaymentDetails = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr/td/table[3]/tbody/tr/td/table[2]/tbody/tr/td/table/tbody"));
                            IList<IWebElement> IPaymentDetailsRow = IPaymentDetails.FindElements(By.TagName("tr"));
                            IList<IWebElement> IPaymentDetailsTD;
                            foreach (IWebElement payment in IPaymentDetailsRow)
                            {
                                IPaymentDetailsTD = payment.FindElements(By.TagName("td"));
                                if (IPaymentDetailsTD.Count != 0 && IPaymentDetailsTD.Count == 10 && !payment.Text.Contains("Description"))
                                {
                                    string paymentDetails = IPaymentDetailsTD[0].Text + "~" + IPaymentDetailsTD[1].Text + "~" + IPaymentDetailsTD[2].Text + "~" + IPaymentDetailsTD[3].Text + "~" + IPaymentDetailsTD[4].Text + "~" + IPaymentDetailsTD[5].Text + "~" + IPaymentDetailsTD[6].Text + "~" + IPaymentDetailsTD[7].Text + "~" + IPaymentDetailsTD[8].Text + "~" + IPaymentDetailsTD[9].Text;
                                    gc.insert_date(orderNumber, parcelNumber, 2186, paymentDetails, 1, DateTime.Now);
                                }
                            }
                        }
                        string paymentDet = "List #~Type~Grand List Year~Description~Principal Paid~Interest Paid~Lien Paid~Penalty Paid~Total Paid~Date Paid";
                        dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + paymentDet + "' where Id = '2186'");

                    }
                    #endregion
                    string taxauthority = "Tax Authority";
                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + taxauthority + "' where Id = '2192'");
                    gc.insert_date(orderNumber, assessment_id, 2192, taxCollectorlink, 1, DateTime.Now);


                    //
                    driver.Quit();
                    gc.mergpdf(orderNumber, "CT", countynameCT);
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
        public string latestfilename()
        {
            var downloadDirectory1 = ConfigurationManager.AppSettings["AutoPdf"];
            var files = new DirectoryInfo(downloadDirectory1).GetFiles("*.*");
            string latestfile = "";
            DateTime lastupdated = DateTime.MinValue;
            foreach (FileInfo file in files)
            {
                if (file.LastWriteTime > lastupdated)
                {
                    lastupdated = file.LastWriteTime;
                    latestfile = file.Name;
                }
            }
            return latestfile;
        }
    }
}