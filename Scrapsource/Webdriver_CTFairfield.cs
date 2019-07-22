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

    public class Webdriver_CTFairfield
    {
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        string msg;
        IWebElement addclick;
        string multiparceldata = "";
        string countyname = "";
        string uniqueidMap = "";
        string urlAssess = "", urlTax = "", countAssess = "", countTax = "", taxCollectorlink = "";
        int countmulti;
        string streetno1 = "", streetname1 = "";
        public string FTP_CTFairfield(string streetno, string streetname, string streetdir, string streettype, string assessment_id, string parcelNumber, string searchType, string orderNumber, string directParcel, string ownername, string countynameCT, string statecountyid, string township, string townshipcode)
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
                            driver.FindElement(By.XPath("//*[@id='SearchAll']/span[7]")).Click();
                            Thread.Sleep(3000);
                           
                            gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "CT", countynameCT);

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
                                    //IWebElement parceldata = driver.FindElement(By.XPath("//*[@id='MainContent_grdSearchResults']/tbody/tr[2]/td[1]/a"));
                                    //parceldata.Click();
                                    //Thread.Sleep(1000);

                                    IWebElement Iaddress1 = driver.FindElement(By.XPath("//*[@id='MainContent_grdSearchResults']/tbody/tr[2]/td[1]/a"));
                                    IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                                    js.ExecuteScript("arguments[0].click();", Iaddress1);
                                    Thread.Sleep(3000);
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
                                            gc.insert_date(orderNumber, TDmultiaddress[6].Text, 2166, Multi, 1, DateTime.Now);
                                        }
                                        if (TDmultiaddress.Count == 8 && !row.Text.Contains("Address") && !row.Text.Contains("Results"))
                                        {
                                            //Address~Owner~Account ID~PID
                                            multiparceldata = "Address~Owner~Account ID~MBLU";
                                            string Multi = TDmultiaddress[0].Text + "~" + TDmultiaddress[1].Text + "~" + TDmultiaddress[2].Text + "~" + TDmultiaddress[3].Text + "-" + TDmultiaddress[4].Text + "-" + TDmultiaddress[5].Text + "-" + TDmultiaddress[6].Text;
                                            gc.insert_date(orderNumber, TDmultiaddress[7].Text, 2166, Multi, 1, DateTime.Now);
                                        }
                                        if (TDmultiaddress.Count == 10 && !row.Text.Contains("Address") && !row.Text.Contains("Results"))
                                        {
                                            //Address~Owner~Account ID~PID
                                            multiparceldata = "Address~Owner~Account ID~MBLU";
                                            string Multi = TDmultiaddress[0].Text + "~" + TDmultiaddress[1].Text + "~" + TDmultiaddress[2].Text + "~" + TDmultiaddress[3].Text + "-" + TDmultiaddress[4].Text + "-" + TDmultiaddress[5].Text + "-" + TDmultiaddress[6].Text + TDmultiaddress[7].Text + "-" + TDmultiaddress[8].Text;
                                            gc.insert_date(orderNumber, TDmultiaddress[9].Text, 2166, Multi, 1, DateTime.Now);
                                        }


                                        if (TDmultiaddress.Count == 11 && !row.Text.Contains("Address") && !row.Text.Contains("Results"))
                                        {
                                            //Address~Owner~Account ID~PID
                                            multiparceldata = "Address~Owner~Account ID~MBLU";
                                            string Multi = TDmultiaddress[0].Text + "~" + TDmultiaddress[1].Text + "~" + TDmultiaddress[2].Text + "~" + TDmultiaddress[3].Text + "-" + TDmultiaddress[4].Text + "-" + TDmultiaddress[5].Text + "-" + TDmultiaddress[6].Text + TDmultiaddress[7].Text + "-" + TDmultiaddress[8].Text + "-" + TDmultiaddress[9].Text;
                                            gc.insert_date(orderNumber, TDmultiaddress[10].Text, 2166, Multi, 1, DateTime.Now);
                                        }
                                        if (TDmultiaddress.Count == 12 && !row.Text.Contains("Address") && !row.Text.Contains("Results"))
                                        {
                                            //Address~Owner~Account ID~PID
                                            multiparceldata = "Address~Owner~Account ID~MBLU";
                                            string Multi = TDmultiaddress[0].Text + "~" + TDmultiaddress[1].Text + "~" + TDmultiaddress[2].Text + "~" + TDmultiaddress[3].Text + "-" + TDmultiaddress[4].Text + "-" + TDmultiaddress[5].Text + "-" + TDmultiaddress[6].Text + TDmultiaddress[7].Text + "-" + TDmultiaddress[8].Text + "-" + TDmultiaddress[9].Text + "-" + TDmultiaddress[10].Text;
                                            gc.insert_date(orderNumber, TDmultiaddress[11].Text, 2166, Multi, 1, DateTime.Now);
                                        }

                                    }
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2166'");

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
                                    Thread.Sleep(3000);
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
                                            gc.insert_date(orderNumber, TDmultiaddress[4].Text, 2166, Multi, 1, DateTime.Now);
                                        }
                                    }
                                    multiparceldata = "Address~Owner~MBLU~Property Use";
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2166'");

                                    gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "CT", countynameCT);
                                    HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                            }
                            catch { }


                        }
                        if (countAssess == "2")//Sherman
                        {
                            driver.FindElement(By.Id("MainContent_tbPropertySearchStreetNumber")).SendKeys(streetno);
                            string street = streetname.ToUpper().Trim() + " " + streettype.ToUpper().Trim();
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
                                foreach (IWebElement row in TRmultiaddress)
                                {
                                    TDmultiaddress = row.FindElements(By.TagName("td"));
                                    if (TRmultiaddress.Count < 2 && Convert.ToInt32(strmulti) < 2)
                                    {

                                        IWebElement parceldata = driver.FindElement(By.XPath("//*[@id='dt_a']/tbody/tr[1]/td[1]/a"));
                                        uniqueidMap = TDmultiaddress[1].Text;
                                        parceldata.Click();
                                        Thread.Sleep(1000);
                                    }
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
                                        if (TDmultiaddress.Count != 0 && !row.Text.Contains("Location") && row.Text.Trim() != "")
                                        {
                                            //Address~Owner~MBLU~Property Use
                                            string Multi = TDmultiaddress[1].Text + " " + TDmultiaddress[0].Text + " " + TDmultiaddress[2].Text + "~" + TDmultiaddress[3].Text + "~" + TDmultiaddress[5].Text + "~" + TDmultiaddress[6].Text;
                                            gc.insert_date(orderNumber, TDmultiaddress[4].Text, 2166, Multi, 1, DateTime.Now);
                                        }
                                    }
                                    multiparceldata = "Address~Owner~MBLU~Property Use";
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2166'");

                                    gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "CT", countynameCT);
                                    HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                            }
                            catch { }
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
                        }
                        if (countAssess == "3")//Bethel
                        {
                            try
                            {

                                driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table/tbody/tr/td/table/tbody/tr[2]/td[4]/table/tbody/tr/td/span/input")).SendKeys(streetno);
                                try
                                {
                                    IWebElement IAddressSelect = driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table/tbody/tr/td/table/tbody/tr[2]/td[5]/table/tbody/tr/td/span/select"));
                                    SelectElement sAddressSelect = new SelectElement(IAddressSelect);
                                    sAddressSelect.SelectByText(streetname.ToUpper() + " " + streettype.ToUpper());
                                }
                                catch { }
                                gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "CT", countynameCT);
                                driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr/td[1]/table/tbody/tr/td/span/input")).SendKeys(Keys.Enter);
                            }
                            catch { }
                            try
                            {
                                driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/table/tbody/tr/td/table/tbody/tr[2]/td[4]/table/tbody/tr/td/span/input")).SendKeys(streetno);
                                try
                                {
                                    IWebElement IAddressSelect = driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/table/tbody/tr/td/table/tbody/tr[2]/td[5]/table/tbody/tr/td/span/select"));
                                    SelectElement sAddressSelect = new SelectElement(IAddressSelect);
                                    sAddressSelect.SelectByText(streetname.ToUpper() + " " + streettype.ToUpper());
                                }
                                catch { }
                                gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "CT", countynameCT);
                                driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[3]/td/table/tbody/tr/td[1]/table/tbody/tr/td/span/input")).SendKeys(Keys.Enter);

                            }
                            catch { }
                            try
                            {
                                string nodata = driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td/table")).Text;
                                if (nodata.Contains("search did not return any results"))
                                {
                                    HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }

                            try
                            {
                                IWebElement multiaddress = driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/table/tbody"));
                                string multi = gc.Between(multiaddress.Text, "of", "Page").Trim();
                                if (Convert.ToInt32(multi) == 1)
                                {
                                    driver.FindElement(By.Id("/html/body/div/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]/span/a")).Click();
                                }
                                else if (Convert.ToInt32(multi) < 25 && Convert.ToInt32(multi) > 1)
                                {
                                    IWebElement multiaddress1 = driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table/tbody"));

                                    IList<IWebElement> TRmultiaddress = multiaddress1.FindElements(By.TagName("tr"));
                                    IList<IWebElement> TDmultiaddress;

                                    foreach (IWebElement row in TRmultiaddress)
                                    {
                                        TDmultiaddress = row.FindElements(By.TagName("td"));
                                        if (TDmultiaddress.Count != 0 && !row.Text.Contains("Street Number :"))
                                        {
                                            string address1 = TDmultiaddress[3].Text.Trim() + " " + TDmultiaddress[4].Text.Trim();
                                            string address2 = streetno.Trim() + " " + streetname.ToUpper().Trim() + " " + streettype.ToUpper().Trim();
                                            if (address1 == address2)
                                            {
                                                countmulti++;
                                                addclick = TDmultiaddress[1].FindElement(By.TagName("a"));
                                            }
                                            string Multi = TDmultiaddress[3].Text + " " + TDmultiaddress[4].Text + "~" + TDmultiaddress[5].Text + "~" + TDmultiaddress[6].Text + "~" + TDmultiaddress[7].Text;
                                            gc.insert_date(orderNumber, TDmultiaddress[2].Text, 2166, Multi, 1, DateTime.Now);
                                        }
                                    }
                                    multiparceldata = "Address~LUC~Class~Card";
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2166'");

                                    if (countmulti == 1)
                                    {
                                        addclick.Click();
                                    }
                                    else if (countmulti > 1)
                                    {

                                        gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "CT", countynameCT);
                                        HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                        driver.Quit();
                                        return "MultiParcel";
                                    }
                                }
                                else if (Convert.ToInt32(multi) > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_Multicount_CT" + countynameCT + township] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }
                            }
                            catch { }

                        }
                        if (countAssess == "4")//Darein
                        {
                            driver.FindElement(By.XPath("//*[@id='content']/div/div[2]/p[3]/a[3]")).Click();
                            driver.FindElement(By.Id("btAgree")).SendKeys(Keys.Enter);
                            Thread.Sleep(1000);
                            driver.FindElement(By.Id("inpNumber")).SendKeys(streetno);
                            driver.FindElement(By.Id("inpStreet")).SendKeys(streetname.Trim());
                            gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "CT", countynameCT);
                            driver.FindElement(By.Id("btSearch")).SendKeys(Keys.Enter);
                            Thread.Sleep(1000);
                            gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "CT", countynameCT);
                            try
                            {
                                string nodata = driver.FindElement(By.XPath("//*[@id='frmMain']/table/tbody/tr/td/div/div/table[2]")).Text;
                                if (nodata.Contains("search did not find any records"))
                                {
                                    HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }
                            try
                            {
                                IWebElement IAddressSelect = driver.FindElement(By.Id("selPageSize"));
                                SelectElement sAddressSelect = new SelectElement(IAddressSelect);
                                sAddressSelect.SelectByText("25");
                            }
                            catch { }

                            try
                            {
                                IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='searchResults']/tbody"));
                                IList<IWebElement> TRmultiaddress = multiaddress.FindElements(By.TagName("tr"));
                                IList<IWebElement> TDmultiaddress;
                                if (TRmultiaddress.Count > 27)
                                {
                                    HttpContext.Current.Session["multiParcel_Multicount_CT" + countynameCT + township] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }
                                if (TRmultiaddress.Count > 3 && TRmultiaddress.Count < 28)
                                {
                                    foreach (IWebElement row in TRmultiaddress)
                                    {
                                        TDmultiaddress = row.FindElements(By.TagName("td"));
                                        if (TDmultiaddress.Count != 0 && !row.Text.Contains("Address") && row.Text.Trim() != "")
                                        {
                                            //Address~Owner
                                            string Multi = TDmultiaddress[2].Text + "~" + TDmultiaddress[1].Text;
                                            gc.insert_date(orderNumber, TDmultiaddress[0].Text, 2166, Multi, 1, DateTime.Now);
                                        }
                                    }
                                    multiparceldata = "Address~Owner";
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2166'");

                                    gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "CT", countynameCT);
                                    HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                                else
                                {
                                    driver.FindElement(By.XPath("//*[@id='searchResults']/tbody/tr[3]/td[1]")).Click();
                                }

                            }
                            catch { }

                        }

                        if (countAssess == "5")//Burlington
                        {
                            try
                            {
                                driver.ExecuteJavaScript("document.getElementById('houseno').setAttribute('value','" + streetno + "')");

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
                                //gc.CreatePdf_WOP(orderNumber, "Address After", driver, "CO", "Adams");
                                foreach (IWebElement AddressT in Addresrow)
                                {
                                    AddressTD = AddressT.FindElements(By.TagName("td"));
                                    if (AddressTD.Count > 1 && !AddressT.Text.Contains("Quick Links") && AddressT.Text.Trim() != "")
                                    {
                                        string[] Arrayaddress = AddressTD[1].Text.Split('\r');
                                        if (townshipcode == "24")
                                        {

                                            GisID = Arrayaddress[0];
                                            UniqueID = Arrayaddress[1].Replace("\n", "").Trim();
                                            Ownername = Arrayaddress[2].Replace("\n", "").Trim();
                                            Address = Arrayaddress[3].Replace("\n", "").Trim();
                                        }
                                        if (townshipcode == "22")
                                        {

                                            GisID = Arrayaddress[0];
                                            UniqueID = "";
                                            Ownername = Arrayaddress[1].Replace("\n", "").Trim();
                                            Address = Arrayaddress[2].Replace("\n", "").Trim();
                                        }
                                        IWebElement Parcellink = AddressTD[2].FindElement(By.TagName("a"));
                                        hrefCardlink = Parcellink.GetAttribute("href");
                                        IWebElement Parcellinkw = AddressTD[2].FindElement(By.LinkText("Summary Card"));
                                        hrefparcellink = Parcellinkw.GetAttribute("href");

                                        string Multiresult = Address + "~" + Ownername + "~" + UniqueID;
                                        gc.insert_date(orderNumber, GisID, 2166, Multiresult, 1, DateTime.Now);
                                        Max++;
                                        gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "CT", countynameCT);
                                    }

                                }
                                multiparceldata = "Address~Owner~Account Number";
                                dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2166'");

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
                        if (countAssess == "6")//Straford
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//*[@id='appBody']/div[4]/div/div/div[3]/a[1]")).Click();
                                Thread.Sleep(2000);
                            }
                            catch { }

                            driver.FindElement(By.Id("ctlBodyPane_ctl03_ctl01_txtAddress")).SendKeys(address);
                            gc.CreatePdf_WOP(orderNumber, "Address search", driver, "CT", countynameCT);
                            driver.FindElement(By.Id("ctlBodyPane_ctl03_ctl01_btnSearch")).SendKeys(Keys.Enter);
                            Thread.Sleep(2000);

                            try
                            {
                                string Parcelno = "", Owner = "", Property_Address = "", Legal_Desp = "", MultiAddress_details = "";

                                IWebElement MultiAddressTB = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_ctl01_gvwParcelResults']/tbody"));
                                IList<IWebElement> MultiAddressTR = MultiAddressTB.FindElements(By.TagName("tr"));
                                IList<IWebElement> MultiAddressTD;
                                gc.CreatePdf_WOP(orderNumber, "Multi Address search", driver, "CT", countynameCT);
                                int AddressmaxCheck = 0;
                                foreach (IWebElement MultiAddress in MultiAddressTR)
                                {
                                    if (AddressmaxCheck <= 25)
                                    {
                                        MultiAddressTD = MultiAddress.FindElements(By.TagName("td"));
                                        if (MultiAddressTD.Count != 0)
                                        {
                                            Parcelno = MultiAddressTD[1].Text;
                                            Owner = MultiAddressTD[2].Text;
                                            Property_Address = MultiAddressTD[3].Text;

                                            MultiAddress_details = Property_Address + "~" + Owner;
                                            gc.insert_date(orderNumber, Parcelno, 2166, MultiAddress_details, 1, DateTime.Now);
                                        }
                                        AddressmaxCheck++;
                                    }
                                    multiparceldata = "Property Address~Owner";
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2166'");

                                }

                                if (MultiAddressTR.Count > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_Multicount_CT" + countynameCT + township] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }
                                else
                                {
                                    HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                            }
                            catch
                            { }
                        }
                        if (countAssess == "titleflex")
                        {
                            searchType = "titleflex";
                        }

                        if (countAssess == "13")//New Canaan Address
                        {
                            driver.FindElement(By.Name("snumber")).SendKeys(streetno);
                            //IWebElement Streetname = driver.FindElement(By.Name("sname"));
                            //SelectElement sStreetname = new SelectElement(Streetname);
                            //IList<IWebElement> options = sStreetname.Options;
                            //sStreetname.SelectedOption.GetAttribute(streetname+ " "+streettype);
                            driver.FindElement(By.Name("sname")).SendKeys(streetname.ToUpper() + " " + streettype.ToUpper());
                            gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "CT", countynameCT);
                            driver.FindElement(By.XPath("//*[@id='maintablediv']/table/tbody/tr[4]/td[3]/input[2]")).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf_WOP(orderNumber, "Address Search After", driver, "CT", countynameCT);
                            try
                            {
                                string nodata = driver.FindElement(By.XPath("//*[@id='maintablediv']/table/tbody/tr[2]/td[2]/font[3]/i")).Text;

                                if (nodata.Contains("No Records Found"))
                                {
                                    gc.CreatePdf_WOP(orderNumber, "No Data Found", driver, "CT", countynameCT);
                                    HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }
                            try
                            {
                                int Max = 0;
                                IWebElement Addresstable = driver.FindElement(By.XPath("//*[@id='ctl00_ContentPlaceHolder_QuickSearchResultsDisplay']/tbody"));
                                IList<IWebElement> Addresrow = Addresstable.FindElements(By.TagName("tr"));
                                IList<IWebElement> AddressTD;
                                gc.CreatePdf_WOP(orderNumber, "Address After", driver, "CO", "Adams");
                                foreach (IWebElement AddressT in Addresrow)
                                {
                                    AddressTD = AddressT.FindElements(By.TagName("td"));
                                    if (AddressTD.Count != 0)
                                    {
                                        //IWebElement Parcellink = AddressTD[0].FindElement(By.TagName("a"));
                                        //Addresshrf = Parcellink.GetAttribute("href");
                                        string parcelno = AddressTD[0].Text;
                                        string OwnerName = AddressTD[1].Text;
                                        string Address = AddressTD[2].Text;
                                        string Unit = AddressTD[2].Text;
                                        string Multiresult = OwnerName + "~" + Address + "~" + Unit;
                                        gc.insert_date(orderNumber, parcelno, 2166, Multiresult, 1, DateTime.Now);
                                        Max++;
                                    }
                                }
                                multiparceldata = "Owner~Property Address~Unit";
                                dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2166'");
                                if (Max > 1 && Max < 26)
                                {
                                    HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                                if (Max > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_Multicount_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "Maximum";
                                }
                            }
                            catch { }

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
                        if (townshipcode == "21" || townshipcode == "12" || townshipcode == "29")//Ridgefield and Monroe
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
                            driver.FindElement(By.XPath("//*[@id='SearchAll']/span[7]")).Click();
                            Thread.Sleep(3000);
                           
                          
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
                                            gc.insert_date(orderNumber, TDmultiaddress[6].Text, 2166, Multi, 1, DateTime.Now);
                                        }
                                        if (TDmultiaddress.Count == 8 && !row.Text.Contains("Address") && !row.Text.Contains("Results"))
                                        {
                                            //Address~Owner~Account ID~PID
                                            multiparceldata = "Address~Owner~Account ID~MBLU";
                                            string Multi = TDmultiaddress[0].Text + "~" + TDmultiaddress[1].Text + "~" + TDmultiaddress[2].Text + "~" + TDmultiaddress[3].Text + "-" + TDmultiaddress[4].Text + "-" + TDmultiaddress[5].Text + "-" + TDmultiaddress[6].Text;
                                            gc.insert_date(orderNumber, TDmultiaddress[7].Text, 2166, Multi, 1, DateTime.Now);
                                        }
                                        if (TDmultiaddress.Count == 10 && !row.Text.Contains("Address") && !row.Text.Contains("Results"))
                                        {
                                            //Address~Owner~Account ID~PID
                                            multiparceldata = "Address~Owner~Account ID~MBLU";
                                            string Multi = TDmultiaddress[0].Text + "~" + TDmultiaddress[1].Text + "~" + TDmultiaddress[2].Text + "~" + TDmultiaddress[3].Text + "-" + TDmultiaddress[4].Text + "-" + TDmultiaddress[5].Text + "-" + TDmultiaddress[6].Text + TDmultiaddress[7].Text + "-" + TDmultiaddress[8].Text;
                                            gc.insert_date(orderNumber, TDmultiaddress[9].Text, 2166, Multi, 1, DateTime.Now);
                                        }


                                        if (TDmultiaddress.Count == 11 && !row.Text.Contains("Address") && !row.Text.Contains("Results"))
                                        {
                                            //Address~Owner~Account ID~PID
                                            multiparceldata = "Address~Owner~Account ID~MBLU";
                                            string Multi = TDmultiaddress[0].Text + "~" + TDmultiaddress[1].Text + "~" + TDmultiaddress[2].Text + "~" + TDmultiaddress[3].Text + "-" + TDmultiaddress[4].Text + "-" + TDmultiaddress[5].Text + "-" + TDmultiaddress[6].Text + TDmultiaddress[7].Text + "-" + TDmultiaddress[8].Text + "-" + TDmultiaddress[9].Text;
                                            gc.insert_date(orderNumber, TDmultiaddress[10].Text, 2166, Multi, 1, DateTime.Now);
                                        }
                                        if (TDmultiaddress.Count == 12 && !row.Text.Contains("Address") && !row.Text.Contains("Results"))
                                        {
                                            //Address~Owner~Account ID~PID
                                            multiparceldata = "Address~Owner~Account ID~MBLU";
                                            string Multi = TDmultiaddress[0].Text + "~" + TDmultiaddress[1].Text + "~" + TDmultiaddress[2].Text + "~" + TDmultiaddress[3].Text + "-" + TDmultiaddress[4].Text + "-" + TDmultiaddress[5].Text + "-" + TDmultiaddress[6].Text + TDmultiaddress[7].Text + "-" + TDmultiaddress[8].Text + "-" + TDmultiaddress[9].Text + "-" + TDmultiaddress[10].Text;
                                            gc.insert_date(orderNumber, TDmultiaddress[11].Text, 2166, Multi, 1, DateTime.Now);
                                        }
                                    }
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2166'");

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
                                            gc.insert_date(orderNumber, TDmultiaddress[4].Text, 2166, Multi, 1, DateTime.Now);
                                        }
                                    }
                                    multiparceldata = "Address~Owner~MBLU~Property Use";
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2166'");

                                    gc.CreatePdf_WOP(orderNumber, "Owner Search Result", driver, "CT", countynameCT);
                                    HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                            }
                            catch { }
                        }
                        if (countAssess == "2")//Sherman
                        {
                            HttpContext.Current.Session["Owner_CT" + countynameCT + township] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        //owner
                        if (countAssess == "3") //Bethel
                        {
                            try
                            {
                                driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table/tbody/tr/td/table/tbody/tr[2]/td[3]/table/tbody/tr/td/span/input")).SendKeys(ownername);
                                gc.CreatePdf_WOP(orderNumber, "Owner Search", driver, "CT", countynameCT);
                                driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr/td[1]/table/tbody/tr/td/span/input")).SendKeys(Keys.Enter);
                            }
                            catch { }

                            try
                            {
                                driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/table/tbody/tr/td/table/tbody/tr[2]/td[3]/table/tbody/tr/td/span/input")).SendKeys(ownername);
                                gc.CreatePdf_WOP(orderNumber, "Owner Search", driver, "CT", countynameCT);
                                driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[3]/td/table/tbody/tr/td[1]/table/tbody/tr/td/span/input")).SendKeys(Keys.Enter);

                            }
                            catch { }
                            try
                            {
                                string nodata = driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td/table")).Text;
                                if (nodata.Contains("search did not return any results"))
                                {
                                    HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }

                            try
                            {
                                IWebElement multiaddress = driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/table/tbody"));
                                string multi = gc.Between(multiaddress.Text, "of", "Page").Trim();
                                if (Convert.ToInt32(multi) == 1)
                                {
                                    driver.FindElement(By.Id("/html/body/div/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table/tbody/tr[2]/td[2]/span/a")).Click();
                                }
                                else if (Convert.ToInt32(multi) < 25 && Convert.ToInt32(multi) > 1)
                                {
                                    IWebElement multiaddress1 = driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table/tbody"));

                                    IList<IWebElement> TRmultiaddress = multiaddress1.FindElements(By.TagName("tr"));
                                    IList<IWebElement> TDmultiaddress;

                                    foreach (IWebElement row in TRmultiaddress)
                                    {
                                        TDmultiaddress = row.FindElements(By.TagName("td"));
                                        if (TDmultiaddress.Count != 0 && !row.Text.Contains("Street Number :"))
                                        {

                                            countmulti++;
                                            addclick = TDmultiaddress[1].FindElement(By.TagName("a"));

                                            string Multi = TDmultiaddress[3].Text + " " + TDmultiaddress[4].Text + "~" + TDmultiaddress[5].Text + "~" + TDmultiaddress[6].Text + "~" + TDmultiaddress[7].Text;
                                            gc.insert_date(orderNumber, TDmultiaddress[2].Text, 2166, Multi, 1, DateTime.Now);
                                        }
                                    }
                                    multiparceldata = "Address~LUC~Class~Card";
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2166'");

                                    if (countmulti == 1)
                                    {
                                        addclick.Click();
                                    }
                                    else if (countmulti > 1)
                                    {

                                        gc.CreatePdf_WOP(orderNumber, "Owner Search Result", driver, "CT", countynameCT);
                                        HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                        driver.Quit();
                                        return "MultiParcel";
                                    }
                                }
                                else if (Convert.ToInt32(multi) > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_Multicount_CT" + countynameCT + township] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }
                            }
                            catch { }
                        }
                        //owner
                        if (countAssess == "4")//Darient
                        {
                            driver.FindElement(By.XPath("//*[@id='content']/div/div[2]/p[3]/a[2]")).Click();
                            driver.FindElement(By.Id("btAgree")).SendKeys(Keys.Enter);
                            Thread.Sleep(1000);
                            driver.FindElement(By.Id("inpOwner")).SendKeys(ownername.Trim());
                            gc.CreatePdf_WOP(orderNumber, "Ownername Search", driver, "CT", countynameCT);
                            driver.FindElement(By.Id("btSearch")).SendKeys(Keys.Enter);
                            Thread.Sleep(1000);

                            try
                            {
                                string nodata = driver.FindElement(By.XPath("//*[@id='frmMain']/table/tbody/tr/td/div/div/table[2]")).Text;
                                if (nodata.Contains("search did not find any records"))
                                {
                                    HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }
                            try
                            {
                                IWebElement IAddressSelect = driver.FindElement(By.Id("selPageSize"));
                                SelectElement sAddressSelect = new SelectElement(IAddressSelect);
                                sAddressSelect.SelectByText("25");
                            }
                            catch { }

                            try
                            {
                                IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='searchResults']/tbody"));
                                IList<IWebElement> TRmultiaddress = multiaddress.FindElements(By.TagName("tr"));
                                IList<IWebElement> TDmultiaddress;
                                if (TRmultiaddress.Count > 27)
                                {
                                    HttpContext.Current.Session["multiParcel_Multicount_CT" + countynameCT + township] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }
                                if (TRmultiaddress.Count > 3 && TRmultiaddress.Count < 28)
                                {
                                    foreach (IWebElement row in TRmultiaddress)
                                    {
                                        TDmultiaddress = row.FindElements(By.TagName("td"));
                                        if (TDmultiaddress.Count != 0 && !row.Text.Contains("Address") && row.Text.Trim() != "")
                                        {
                                            //Address~Owner
                                            string Multi = TDmultiaddress[2].Text + "~" + TDmultiaddress[1].Text;
                                            gc.insert_date(orderNumber, TDmultiaddress[0].Text, 2166, Multi, 1, DateTime.Now);
                                        }
                                    }
                                    multiparceldata = "Address~Owner";
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2166'");

                                    gc.CreatePdf_WOP(orderNumber, "Owner Search Result", driver, "CT", countynameCT);
                                    HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                                else
                                {
                                    driver.FindElement(By.XPath("//*[@id='searchResults']/tbody/tr[3]/td[1]")).Click();
                                }

                            }
                            catch { }

                        }

                        if (countAssess == "5")//Burlington
                        {
                            try
                            {
                                driver.ExecuteJavaScript("document.getElementById('searchname').setAttribute('value','" + ownername + "')");
                                IWebElement Iviewpay = driver.FindElement(By.Name("go"));
                                IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                                js1.ExecuteScript("arguments[0].click();", Iviewpay);
                                Thread.Sleep(5000);

                                // IWebElement iframeElement1 = driver.FindElement(By.XPath("//*[@id='body']"));
                                driver.SwitchTo().Frame(0);

                                try
                                {
                                    string nodata = driver.FindElement(By.XPath("/html/body/div[2]")).Text;
                                    if (nodata.Contains("No matching"))
                                    {
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
                                //gc.CreatePdf_WOP(orderNumber, "Address After", driver, "CO", "Adams");
                                foreach (IWebElement AddressT in Addresrow)
                                {
                                    AddressTD = AddressT.FindElements(By.TagName("td"));
                                    if (AddressTD.Count > 1 && !AddressT.Text.Contains("Quick Links") && AddressT.Text.Trim() != "")
                                    {
                                        string[] Arrayaddress = AddressTD[1].Text.Split('\r');
                                        if (townshipcode == "24")
                                        {
                                            GisID = Arrayaddress[0];
                                            UniqueID = Arrayaddress[1].Replace("\n", "").Trim();
                                            Ownername = Arrayaddress[2].Replace("\n", "").Trim();
                                            Address = Arrayaddress[3].Replace("\n", "").Trim();
                                        }
                                        if (townshipcode == "22")
                                        {

                                            GisID = Arrayaddress[0];
                                            UniqueID = "";
                                            Ownername = Arrayaddress[1].Replace("\n", "").Trim();
                                            Address = Arrayaddress[2].Replace("\n", "").Trim();
                                        }
                                        IWebElement Parcellink = AddressTD[2].FindElement(By.TagName("a"));
                                        hrefCardlink = Parcellink.GetAttribute("href");
                                        IWebElement Parcellinkw = AddressTD[2].FindElement(By.LinkText("Summary Card"));
                                        hrefparcellink = Parcellinkw.GetAttribute("href");

                                        string Multiresult = Address + "~" + Ownername + "~" + UniqueID;
                                        gc.insert_date(orderNumber, GisID, 2166, Multiresult, 1, DateTime.Now);
                                        Max++;
                                        gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "CT", countynameCT);
                                    }

                                }
                                multiparceldata = "Address~Owner~Account Number";
                                dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2166'");

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
                        if (countAssess == "6")//Straford
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//*[@id='appBody']/div[4]/div/div/div[3]/a[1]")).Click();
                                Thread.Sleep(2000);
                            }
                            catch { }

                            driver.FindElement(By.Id("ctlBodyPane_ctl00_ctl01_txtName")).SendKeys(ownername);
                            gc.CreatePdf_WOP(orderNumber, "Owner search", driver, "CT", countynameCT);
                            driver.FindElement(By.Id("ctlBodyPane_ctl00_ctl01_btnSearch")).SendKeys(Keys.Enter);
                            Thread.Sleep(2000);

                            try
                            {
                                string Parcelno = "", Owner = "", Property_Address = "", Legal_Desp = "", MultiOwner_details = "";
                                IWebElement MultiOwnerTB = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_ctl01_gvwParcelResults']/tbody"));
                                IList<IWebElement> MultiOwnerTR = MultiOwnerTB.FindElements(By.TagName("tr"));
                                IList<IWebElement> MultiOwnerTD;
                                gc.CreatePdf_WOP(orderNumber, "Multi Owner search", driver, "CT", countynameCT);
                                int AddressmaxCheck = 0;
                                foreach (IWebElement MultiOwner in MultiOwnerTR)
                                {
                                    if (AddressmaxCheck <= 25)
                                    {
                                        MultiOwnerTD = MultiOwner.FindElements(By.TagName("td"));
                                        if (MultiOwnerTD.Count != 0)
                                        {
                                            Parcelno = MultiOwnerTD[1].Text;
                                            Owner = MultiOwnerTD[2].Text;
                                            Property_Address = MultiOwnerTD[3].Text;
                                            Legal_Desp = MultiOwnerTD[4].Text;

                                            MultiOwner_details = Owner + "~" + Property_Address + "~" + Legal_Desp;
                                            gc.insert_date(orderNumber, Parcelno, 784, MultiOwner_details, 1, DateTime.Now);
                                        }
                                        AddressmaxCheck++;
                                    }
                                }
                                if (MultiOwnerTR.Count > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_Multicount_CT" + countynameCT + township] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }
                                else
                                {
                                    HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                            }
                            catch
                            { }
                        }
                        if (countAssess == "13")//New Canaan Ownername
                        {
                            driver.FindElement(By.XPath("//*[@id='maintablediv']/table/tbody/tr[6]/td[3]/input[1]")).SendKeys(ownername);
                            driver.FindElement(By.XPath("//*[@id='maintablediv']/table/tbody/tr[4]/td[3]/input[2]")).Click();
                            Thread.Sleep(2000);
                            try
                            {
                                string nodata = driver.FindElement(By.XPath("//*[@id='maintablediv']/table/tbody/tr[2]/td[2]/font[3]/i")).Text;
                                if (nodata.Contains("No Records Found"))
                                {
                                    HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }
                            try
                            {
                                int Max = 0;
                                IWebElement Addresstable = driver.FindElement(By.XPath("/html/body/div/table[2]/tbody"));
                                IList<IWebElement> Addresrow = Addresstable.FindElements(By.TagName("tr"));
                                IList<IWebElement> AddressTD;
                                gc.CreatePdf_WOP(orderNumber, "Address After", driver, "CO", "Adams");
                                foreach (IWebElement AddressT in Addresrow)
                                {
                                    AddressTD = AddressT.FindElements(By.TagName("td"));
                                    if (AddressTD.Count != 0 && !AddressT.Text.Contains("Parcel ID"))
                                    {
                                        //IWebElement Parcellink = AddressTD[0].FindElement(By.TagName("a"));
                                        //Addresshrf = Parcellink.GetAttribute("href");
                                        string parcelno = AddressTD[0].Text;
                                        string OwnerName = AddressTD[1].Text;
                                        string Address = AddressTD[2].Text;
                                        string Unit = AddressTD[2].Text;
                                        string Multiresult = OwnerName + "~" + Address + "~" + Unit;
                                        gc.insert_date(orderNumber, parcelno, 2166, Multiresult, 1, DateTime.Now);
                                        Max++;
                                    }
                                }
                                multiparceldata = "Owner~Property Address~Unit";
                                dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2166'");
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
                            }
                            catch { }
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

                            if (townshipcode == "04" || townshipcode == "10" || townshipcode == "17")
                            {
                                string[] titleid = parcelNumber.Split('-');
                                if (titleid.Count() == 5)
                                {
                                    titleid[1] = titleid[1].TrimStart('0');
                                    titleid[3] = titleid[3].TrimStart('0');
                                    titleid[4] = titleid[4].TrimEnd('0');
                                    if (titleid[1].Length == 1)
                                    {
                                        titleid[1] = "0" + titleid[1];
                                    }
                                    uniqueidMap = titleid[1] + "-" + titleid[3] + "/" + titleid[4];
                                }
                                if (titleid.Count() == 4)
                                {
                                    titleid[1] = titleid[1].TrimStart('0');
                                    titleid[3] = titleid[3].TrimStart('0');
                                    if (titleid[1].Length == 1)
                                    {
                                        titleid[1] = "0" + titleid[1];
                                    }
                                    uniqueidMap = titleid[1] + "-" + titleid[3];
                                }
                                assessment_id = uniqueidMap;
                            }
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

                            driver.FindElement(By.XPath("//*[@id='SearchAll']/span[7]")).Click();
                            Thread.Sleep(3000);
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
                        if (countAssess == "3")
                        {
                            try
                            {
                                driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table/tbody/tr/td/table/tbody/tr[2]/td[1]/table/tbody/tr/td/span/input")).SendKeys(parcelNumber);
                                gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "CT", countynameCT);
                                driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr/td[1]/table/tbody/tr/td/span/input")).SendKeys(Keys.Enter);
                            }
                            catch { }
                            try
                            {
                                driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/table/tbody/tr/td/table/tbody/tr[2]/td[1]/table/tbody/tr/td/span/input")).SendKeys(parcelNumber);
                                gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "CT", countynameCT);
                                driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[3]/td/table/tbody/tr/td[1]/table/tbody/tr/td/span/input")).SendKeys(Keys.Enter);
                            }
                            catch { }
                            Thread.Sleep(1000);
                            try
                            {
                                string nodata = driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td/table")).Text;
                                if (nodata.Contains("search did not return any results"))
                                {
                                    HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }

                        }
                        if (countAssess == "4")//Darien
                        {
                            driver.FindElement(By.XPath("//*[@id='content']/div/div[2]/p[3]/a[4]")).Click();
                            driver.FindElement(By.Id("btAgree")).SendKeys(Keys.Enter);
                            Thread.Sleep(1000);
                            driver.FindElement(By.Id("inpParid")).SendKeys(parcelNumber);
                            gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "CT", countynameCT);
                            driver.FindElement(By.Id("btSearch")).SendKeys(Keys.Enter);
                            Thread.Sleep(1000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Result", driver, "CT", countynameCT);
                            try
                            {
                                string nodata = driver.FindElement(By.XPath("//*[@id='frmMain']/table/tbody/tr/td/div/div/table[2]/tbody/tr/td/table")).Text;
                                if (nodata.Contains("search did not find any records"))
                                {
                                    HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }

                            IWebElement parceldata = driver.FindElement(By.XPath("//*[@id='searchResults']/tbody/tr[3]/td[1]/div"));
                            parceldata.Click();
                            Thread.Sleep(1000);
                        }
                        if (countAssess == "5")
                        {
                            try
                            {
                                driver.ExecuteJavaScript("document.getElementById('mbl').setAttribute('value','" + parcelNumber + "')");
                                IWebElement Iviewpay = driver.FindElement(By.Name("go"));
                                IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                                js1.ExecuteScript("arguments[0].click();", Iviewpay);
                                Thread.Sleep(5000);

                                // IWebElement iframeElement1 = driver.FindElement(By.XPath("//*[@id='body']"));
                                driver.SwitchTo().Frame(0);

                                try
                                {
                                    string nodata = driver.FindElement(By.XPath("/html/body/div[2]")).Text;
                                    if (nodata.Contains("No matching"))
                                    {
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
                                //gc.CreatePdf_WOP(orderNumber, "Address After", driver, "CO", "Adams");
                                foreach (IWebElement AddressT in Addresrow)
                                {
                                    AddressTD = AddressT.FindElements(By.TagName("td"));
                                    if (AddressTD.Count > 1 && !AddressT.Text.Contains("Quick Links") && AddressT.Text.Trim() != "")
                                    {
                                        string[] Arrayaddress = AddressTD[1].Text.Split('\r');
                                        if (townshipcode == "24")
                                        {

                                            GisID = Arrayaddress[0];
                                            UniqueID = Arrayaddress[1].Replace("\n", "").Trim();
                                            Ownername = Arrayaddress[2].Replace("\n", "").Trim();
                                            Address = Arrayaddress[3].Replace("\n", "").Trim();
                                        }
                                        if (townshipcode == "22")
                                        {

                                            GisID = Arrayaddress[0];
                                            UniqueID = "";
                                            Ownername = Arrayaddress[1].Replace("\n", "").Trim();
                                            Address = Arrayaddress[2].Replace("\n", "").Trim();
                                        }
                                        IWebElement Parcellink = AddressTD[2].FindElement(By.TagName("a"));
                                        hrefCardlink = Parcellink.GetAttribute("href");
                                        IWebElement Parcellinkw = AddressTD[2].FindElement(By.LinkText("Summary Card"));
                                        hrefparcellink = Parcellinkw.GetAttribute("href");

                                        string Multiresult = Address + "~" + Ownername + "~" + UniqueID;
                                        gc.insert_date(orderNumber, GisID, 2166, Multiresult, 1, DateTime.Now);
                                        Max++;
                                        gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "CT", countynameCT);
                                    }

                                }
                                multiparceldata = "Address~Owner~Account Number";
                                dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2166'");

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
                        if (countAssess == "6")//Straford
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//*[@id='appBody']/div[4]/div/div/div[3]/a[1]")).Click();
                                Thread.Sleep(2000);
                            }
                            catch { }
                            driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_txtParcelID")).SendKeys(parcelNumber);
                            gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearch", driver, "CT", countynameCT);
                            driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_btnSearch")).SendKeys(Keys.Enter);
                            Thread.Sleep(2000);
                        }

                        if (countAssess == "13")//Parcel
                        {

                            driver.FindElement(By.Name("parcelid")).SendKeys(parcelNumber);
                            gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearch", driver, "CT", countynameCT);
                            driver.FindElement(By.XPath("//*[@id='maintablediv']/table/tbody/tr[4]/td[3]/input[2]")).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearch after", driver, "CT", countynameCT);

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
                            IWebElement IAccount = driver.FindElement(By.XPath("//*[@id='SearchAll']/span[7]"));
                            IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                            js.ExecuteScript("arguments[0].click();", IAccount);
                            // driver.FindElement(By.XPath("//*[@id='SearchAll']/span[7]/i")).SendKeys(Keys.Enter);
                            Thread.Sleep(3000);
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
                                assessment_id = IBasicDetailsTD[2].Text.Replace("Acct#", "").Trim();
                            }
                            if (IBasicDetailsTD.Count != 0 && row.Text.Contains("Owner"))
                            {
                                Owner = IBasicDetailsTD[3].Text.Replace("Owner\r\n", "").Trim();
                            }
                            if (IBasicDetailsTD.Count != 0 && row.Text.Contains("Assessment"))
                            {
                                Assessment = IBasicDetailsTD[4].Text.Replace("Assessment\r\n", "").Trim();
                            }
                            if (IBasicDetailsTD.Count != 0 && row.Text.Contains("Appraisal"))
                            {
                                Appraisal = IBasicDetailsTD[5].Text.Replace("Appraisal\r\n", "").Trim();
                            }
                            if (IBasicDetailsTD.Count != 0 && row.Text.Contains("PID"))
                            {
                                ParcelID = IBasicDetailsTD[6].Text.Replace("PID\r\n", "").Trim();
                            }
                            if (IBasicDetailsTD.Count != 0 && row.Text.Contains("Building Count"))
                            {
                                BuildingCount = IBasicDetailsTD[7].Text.Replace("Building Count\r\n", "").Trim();
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
                            streetname1 = splitAddress[1] + " " + splitAddress[2] + " " + splitAddress[3];
                            streetname1 = streetname1.Trim();
                        }

                        if (townshipcode == "02")
                        {

                            string[] uniqueidsplit = MapLot.Split(' ');
                            string block = "", lot = "", unit = "";
                            string block1 = "", block2 = "", lot1 = "", lot2 = "", unit1 = "", unit2 = "";
                            block = uniqueidsplit[1];
                            lot = uniqueidsplit[2];
                            unit = uniqueidsplit[3];
                            string[] blocksplit = block.Split('/');
                            string[] lotsplit = lot.Split('/');
                            string[] unitsplit = unit.Split('/');
                            if (blocksplit[0].Length == 3)
                            {
                                block1 = "0" + blocksplit[0];
                            }
                            else if (blocksplit[0].Length == 2)
                            {
                                block1 = "00" + blocksplit[0];
                            }
                            else if (blocksplit[0].Length == 1)
                            {
                                block1 = "000" + blocksplit[0];
                            }
                            else if (blocksplit[0].Length == 4)
                            {
                                block1 = blocksplit[0];
                            }
                            block2 = GlobalClass.After(block, "/");
                            if (block2 == "")
                            {
                                block2 = "-";
                            }
                            if (lotsplit[0].Length == 1)
                            {
                                lot1 = "0" + lotsplit[0];
                            }
                            if (lotsplit[0].Length == 2)
                            {
                                lot1 = lotsplit[0];
                            }
                            lot2 = GlobalClass.After(lot, "/");
                            if (lot2 == "")
                            {
                                lot2 = "-";
                            }

                            unit1 = GlobalClass.Before(unit, "/");
                            if (unit1 != "")
                            {
                                bool containsLetter = Regex.IsMatch(unit1, "[A-Z]");
                                if (containsLetter == true)
                                {
                                    unit = unit1;
                                }
                                else
                                {
                                    if (unit1.Length == 3)
                                    {
                                        unit = unit1;
                                    }
                                    if (unit1.Length == 1)
                                    {
                                        unit = "00" + unit1;
                                    }
                                    if (unit1.Length == 2)
                                    {
                                        unit = "0" + unit1;
                                    }
                                }
                            }
                            else
                            {
                                unit = unit1;
                            }
                            uniqueidMap = block1 + block2 + "-" + lot1 + lot2 + "-" + unit;
                        }
                        if (townshipcode == "03" || townshipcode == "08" || townshipcode == "09" || townshipcode == "11" || townshipcode == "12" || townshipcode == "14" || townshipcode == "15" || townshipcode == "18" || townshipcode == "19" || townshipcode == "20" || townshipcode == "23" || townshipcode == "26" || townshipcode == "27")
                        {
                            uniqueidMap = assessment_id;
                        }
                        if (townshipcode == "32")
                        {

                            uniqueidMap = assessment_id.TrimStart('0');
                        }
                        if (townshipcode == "05")
                        {

                            string[] uniqueidsplit = MapLot.Split(' ');
                            string map = "", block = "", lot = "", unit = "";
                            string block1 = "", block2 = "", lot1 = "", lot2 = "", unit1 = "", unit2 = "";
                            map = uniqueidsplit[0];
                            lot = uniqueidsplit[2];
                            unit = uniqueidsplit[3];

                            map = GlobalClass.Before(map, "/");
                            lot = GlobalClass.Before(lot, "/");
                            unit = GlobalClass.Before(unit, "/");

                            if (lot.Length == 1)
                            {
                                lot = "00" + lot;
                            }
                            else if (lot.Length == 2)
                            {
                                lot = "0" + lot;
                            }
                            else if (lot.Length == 0)
                            {
                                lot = "000";
                            }

                            unit = GlobalClass.Before(unit, "/");
                            if (unit.Trim() == "")
                            {
                                uniqueidMap = map + lot;
                            }
                            else
                            {
                                uniqueidMap = map + lot + "-" + unit;
                            }
                            assessment_id = uniqueidMap;
                        }
                        if (townshipcode == "16")
                        {

                            string[] uniqueidsplit = MapLot.Split(' ');
                            string map = "", block = "", lot = "", unit = "";
                            string block1 = "", block2 = "", lot1 = "", lot2 = "", unit1 = "", unit2 = "";
                            map = uniqueidsplit[0];
                            block = uniqueidsplit[1];
                            lot = uniqueidsplit[2];
                            unit = uniqueidsplit[3];

                            map = GlobalClass.Before(map, "/");
                            block = GlobalClass.Before(block, "/");
                            lot = GlobalClass.Before(lot, "/");
                            unit = GlobalClass.Before(unit, "/");
                            uniqueidMap = map + "-" + block + "-" + lot + "-" + unit;
                            //assessment_id = uniqueidMap;
                        }
                        string BasicDetails = PropertyAddress + "~" + MapLot + "~" + assessment_id + "~" + Owner + "~" + Assessment + "~" + Appraisal + "~" + ParcelID + "~" + BuildingCount;
                        string property1 = "Property Address~Mblu~Account Number~Owner Name~Assessment~Appraisal~PID~Building Count";

                        dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property1 + "' where Id = '2160'");
                        gc.insert_date(orderNumber, assessment_id, 2160, BasicDetails, 1, DateTime.Now);
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
                                gc.insert_date(orderNumber, assessment_id, 2161, ValueAppraisal, 1, DateTime.Now);
                                //Type~Valuation Year~Improvements~Land~Total
                            }
                            if (IICurrentValueAppDetailsTD.Count != 0 && IICurrentValueAppDetailsTD.Count == 2 && !row.Text.Contains("Valuation Year"))
                            {
                                string ValueAppraisal = "Appraisal" + "~" + IICurrentValueAppDetailsTD[0].Text + "~" + "" + "~" + "" + "~" + IICurrentValueAppDetailsTD[1].Text;
                                gc.insert_date(orderNumber, assessment_id, 2161, ValueAppraisal, 1, DateTime.Now);
                                //Type~Valuation Year~Improvements~Land~Total
                            }
                        }
                        string property2 = "Type~Valuation Year~Improvements~Land~Total";
                        dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property2 + "' where Id = '2161'");
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
                                gc.insert_date(orderNumber, assessment_id, 2161, ValueAssessment, 1, DateTime.Now);
                                //Type~Valuation Year~Improvements~Land~Total
                            }
                            if (IICurrentValueAssDetailsTD.Count != 0 && IICurrentValueAssDetailsTD.Count == 2 && !assessment.Text.Contains("Valuation Year"))
                            {
                                string ValueAssessment = "Assessment" + "~" + IICurrentValueAssDetailsTD[0].Text + "~" + "" + "~" + "" + "~" + IICurrentValueAssDetailsTD[1].Text;
                                gc.insert_date(orderNumber, assessment_id, 2161, ValueAssessment, 1, DateTime.Now);
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
                                gc.insert_date(orderNumber, assessment_id, 2162, Propertyresult, 1, DateTime.Now);
                                Propertyresult = "";
                            }
                            if (multirowTH1.Count != 0 && multirowTH1[0].Text != " ")
                            {
                                for (int i = 0; i < multirowTH1.Count; i++)
                                {

                                    Propertyhead += multirowTH1[i].Text + "~";
                                }
                                Propertyhead = Propertyhead.TrimEnd('~');
                                dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + Propertyhead + "' where Id = '2162'");

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
                            if (IYearBuiltTD.Count != 0 && built.Text.Contains("Replacement Cost"))
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
                        gc.insert_date(orderNumber, assessment_id, 2163, YearBuiltDetails, 1, DateTime.Now);
                        string property4 = "Year Built~Living Area~Replacement Cost~Building Percent Good~Replacement Cost Less Depreciation";
                        dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property4 + "' where Id = '2163'");

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
                        gc.insert_date(orderNumber, assessment_id, 2164, LandDetails, 1, DateTime.Now);
                        //Use Code~Description~Zone~Neighborhood~Alt Land Appr~Category~Size(Acres)~Frontage~Depth~Assessed Value~Appraised Value

                        string property5 = "Use Code~Description~Zone~Neighborhood~Alt Land Appr~Category~Size(Acres)~Frontage~Depth~Assessed Value~Appraised Value";
                        dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property5 + "' where Id = '2164'");


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
                                gc.insert_date(orderNumber, assessment_id, 2165, ValueAppraisal, 1, DateTime.Now);
                                //Type~Valuation Year~Improvements~Land~Total
                            }
                            if (IValueAppDetailsTD.Count != 0 && IValueAppDetailsTD.Count == 2 && !row.Text.Contains("Valuation Year"))
                            {
                                string ValueAppraisal = "Appraisal" + "~" + IValueAppDetailsTD[0].Text + "~" + "" + "~" + "" + "~" + IValueAppDetailsTD[1].Text;
                                gc.insert_date(orderNumber, assessment_id, 2165, ValueAppraisal, 1, DateTime.Now);
                                //Type~Valuation Year~Improvements~Land~Total
                            }
                        }

                        //Assessment Valuation History
                        IWebElement IValueAssDetails = driver.FindElement(By.XPath("//*[@id='MainContent_grdHistoryValuesAsmt']/tbody"));
                        IList<IWebElement> IValueAssDetailsRow = IValueAssDetails.FindElements(By.TagName("tr"));
                        IList<IWebElement> IValueAssDetailsTD;
                        foreach (IWebElement assessment in IValueAssDetailsRow)
                        {
                            IValueAssDetailsTD = assessment.FindElements(By.TagName("td"));
                            if (IValueAssDetailsTD.Count != 0 && IValueAssDetailsTD.Count == 4 && !assessment.Text.Contains("Valuation Year"))
                            {
                                string ValueAssessment = "Assessment" + "~" + IValueAssDetailsTD[0].Text + "~" + IValueAssDetailsTD[1].Text + "~" + IValueAssDetailsTD[2].Text + "~" + IValueAssDetailsTD[3].Text;
                                gc.insert_date(orderNumber, assessment_id, 2165, ValueAssessment, 1, DateTime.Now);
                                //Type~Valuation Year~Improvements~Land~Total
                            }
                            if (IValueAssDetailsTD.Count != 0 && IValueAssDetailsTD.Count == 2 && !assessment.Text.Contains("Valuation Year"))
                            {
                                string ValueAssessment = "Assessment" + "~" + IValueAssDetailsTD[0].Text + "~" + "" + "~" + "" + "~" + IValueAssDetailsTD[1].Text;
                                gc.insert_date(orderNumber, assessment_id, 2165, ValueAssessment, 1, DateTime.Now);
                                //Type~Valuation Year~Improvements~Land~Total
                            }
                        }

                        string property6 = "Type~Valuation Year~Improvements~Land~Total";
                        dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property6 + "' where Id = '2165'");
                        string urlpdf = "http://images.vgsi.com/cards/WestportCTCards//" + ParcelID + ".pdf";
                        try
                        {
                            gc.downloadfile(urlpdf, orderNumber, assessment_id, "Propertypdf", "CT", countynameCT);
                            Thread.Sleep(3000);
                        }
                        catch { }
                        if (townshipcode == "31")
                        {
                            try
                            {
                                string FilePath = gc.filePath(orderNumber, assessment_id) + "Propertypdf.pdf";
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


                                string tableassess = gc.Between(pdftext, "Account #", "Bldg #:").Trim();
                                if (tableassess.Length == 4)
                                {
                                    tableassess = "0" + tableassess;
                                }
                                if (tableassess.Length == 3)
                                {
                                    tableassess = "00" + tableassess;
                                }
                                uniqueidMap = tableassess.Trim();
                                assessment_id = uniqueidMap;
                            }
                            catch { }
                        }


                    }
                    #endregion
                    #region one Assessment Link
                    if (countAssess == "1")//Easton
                    {
                        try
                        {
                            driver.FindElement(By.LinkText("Parcel Data And Values")).Click();
                            Thread.Sleep(2000);

                        }
                        catch { }
                        string ParcelValues = "", ParcelValuesHeader = "";

                        string col1 = "-", col3 = "-", col5 = "-";

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
                                parcelNumber = TDParcel[1].Text;
                            }
                        }
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Data And Values1", driver, "CT", countynameCT);
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
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + "Type~" + valuetitle.Remove(valuetitle.Length - 1, 1) + "' where Id = '" + 2161 + "'");
                        gc.insert_date(orderNumber, parcelNumber, 2161, "Appraised Value~" + valueAppvalues.Remove(valueAppvalues.Length - 1, 1), 1, DateTime.Now);
                        gc.insert_date(orderNumber, parcelNumber, 2161, "Assessed Value~" + valueAssvalues.Remove(valueAssvalues.Length - 1, 1), 1, DateTime.Now);
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
                            //driver.FindElement(By.LinkText("Building 1")).Click();
                            Thread.Sleep(3000);
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
                        gc.insert_date(orderNumber, parcelNumber, 2160, property2, 1, DateTime.Now);
                        //Property Adddress~Property Use~Primary Use~Unique ID~Map Block Lot~Acres~490 Acres~Zone~Volume/Page~Developers Map/Lot~Census~Year Built
                        string property1 = ParcelValuesHeader + "Year Built";
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property1 + "' where Id = '" + 2160 + "'");
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
                                    gc.insert_date(orderNumber, parcelNumber, 2162, salesdetails, 1, DateTime.Now);
                                    //Owner Name~Volume~Page~Sale Date~Deed Type~Valid Sale~Sale Price
                                }
                            }
                            //
                            string property9 = "Owner Name~Volume~Page~Sale Date~Deed Type~Valid Sale~Sale Price";
                            db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property9 + "' where Id = '" + 2162 + "'");
                        }
                        catch { }

                        //try
                        //{
                        //    driver.FindElement(By.LinkText("Permits")).Click();
                        //    gc.CreatePdf(orderNumber, parcelNumber, "Permits Result", driver, "CT", countynameCT);
                        //}
                        //catch { }
                        try
                        {
                            IWebElement IAddressSearch3 = driver.FindElement(By.LinkText("Outbuildings"));
                            IJavaScriptExecutor js3 = driver as IJavaScriptExecutor;
                            js3.ExecuteScript("arguments[0].click();", IAddressSearch3);
                            Thread.Sleep(1000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Out Building Result", driver, "CT", countynameCT);
                        }
                        catch { }
                        try
                        {
                            IWebElement IAddressSearch4 = driver.FindElement(By.LinkText("Permits"));
                            IJavaScriptExecutor js4 = driver as IJavaScriptExecutor;
                            js4.ExecuteScript("arguments[0].click();", IAddressSearch4);
                            Thread.Sleep(1000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Permits Result", driver, "CT", countynameCT);
                        }
                        catch { }

                        if (townshipcode == "30" || townshipcode == "21" || townshipcode == "07")
                        {
                            uniqueidMap = parcelNumber;
                        }
                        assessment_id = uniqueidMap;
                    }
                    #endregion
                    #region two Assessment Link
                    if (countAssess == "2")//Sherman
                    {
                        if (townshipcode == "25")
                        {
                            assessment_id = uniqueidMap;

                        }

                        IWebElement IBasicDetails = driver.FindElement(By.XPath("//*[@id='dt_a']/tbody"));
                        IList<IWebElement> IBasicDetailsRow = IBasicDetails.FindElements(By.TagName("tr"));
                        IList<IWebElement> IBasicDetailsTD;
                        foreach (IWebElement row in IBasicDetailsRow)
                        {
                            IBasicDetailsTD = row.FindElements(By.TagName("td"));
                            if (IBasicDetailsTD.Count != 0 && !row.Text.Contains("Location"))
                            {
                                //Property Address~Account Number~Parcel Id~Property Use~Acres~Zone~Buildings
                                string Multi = IBasicDetailsTD[0].Text + "~" + IBasicDetailsTD[1].Text + "~" + IBasicDetailsTD[2].Text + "~" + IBasicDetailsTD[3].Text + IBasicDetailsTD[4].Text + "~" + IBasicDetailsTD[5].Text + "~" + IBasicDetailsTD[6].Text;
                                gc.insert_date(orderNumber, IBasicDetailsTD[1].Text, 2160, Multi, 1, DateTime.Now);
                                parcelNumber = IBasicDetailsTD[1].Text;
                                if (townshipcode == "25")
                                {
                                    assessment_id = IBasicDetailsTD[1].Text;
                                    uniqueidMap = IBasicDetailsTD[1].Text;
                                }
                            }
                        }
                        string property9 = "Property Address~Account Number~Parcel Id~Property Use~Acres~Zone~Buildings";
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property9 + "' where Id = '" + 2160 + "'");


                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "CT", countynameCT);
                        //driver.SwitchTo().Window(driver.WindowHandles.Last());
                        Thread.Sleep(5000);
                        try
                        {
                            IWebElement IDownload = driver.FindElement(By.XPath("//*[@id='dt_a']/tbody/tr/td[1]/a"));
                            string strdownload = IDownload.GetAttribute("href");
                            gc.downloadfile(strdownload, orderNumber, parcelNumber, "Assessment", "CT", countynameCT);
                        }
                        catch { }
                    }
                    #endregion
                    #region three Assessment Link
                    if (countAssess == "3")//Bethel
                    {
                        string AlternateID = "", FirstCard = "", SecondCard = "", StName = "", StNo = "", Zoining = "", LUC = "", Acres = "";
                        //Property Details
                        IWebElement IpropertyDetails = driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/table/tbody/tr/td/table/tbody"));
                        IList<IWebElement> IpropertyDetailsRow = IpropertyDetails.FindElements(By.TagName("tr"));
                        IList<IWebElement> IpropertyDetailsTD;
                        foreach (IWebElement property in IpropertyDetailsRow)
                        {
                            IpropertyDetailsTD = property.FindElements(By.TagName("td"));
                            if (IpropertyDetailsTD.Count != 0 && IpropertyDetailsTD.Count > 2 && !property.Text.Contains("Parcel ID"))
                            {
                                parcelNumber = IpropertyDetailsTD[0].Text;
                                AlternateID = IpropertyDetailsTD[2].Text;
                                FirstCard = IpropertyDetailsTD[4].Text;
                                SecondCard = IpropertyDetailsTD[6].Text;
                                StName = IpropertyDetailsTD[8].Text;
                                StNo = IpropertyDetailsTD[10].Text;
                                Zoining = IpropertyDetailsTD[12].Text;
                                LUC = IpropertyDetailsTD[14].Text;
                                Acres = IpropertyDetailsTD[16].Text;
                                uniqueidMap = AlternateID;
                                assessment_id = AlternateID;
                            }
                        }
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "CT", countynameCT);
                        //Dwelling Details
                        string FirstOwner = "", SecondOwner = "", StreetNo = "", Street = "", City = "", State = "", Zip = "", Volume = "", Page = "", DeedDate = "";
                        IWebElement IOwnerDetails = driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td/table/tbody/tr[4]/td/table/tbody/tr/td/table/tbody/tr/td[1]/table/tbody/tr/td/table/tbody/tr[1]/td/table/tbody/tr[2]/td/table/tbody/tr/td/table/tbody"));
                        IList<IWebElement> IOwnerDetailsRow = IOwnerDetails.FindElements(By.TagName("tr"));
                        IList<IWebElement> IOwnerDetailsTD;
                        foreach (IWebElement owner in IOwnerDetailsRow)
                        {
                            IOwnerDetailsTD = owner.FindElements(By.TagName("td"));
                            if (IOwnerDetailsTD.Count != 0 && owner.Text.Contains("Owner 1 Name"))
                            {
                                FirstOwner = IOwnerDetailsTD[1].Text;
                            }
                            if (IOwnerDetailsTD.Count != 0 && owner.Text.Contains("Owner 2 Name"))
                            {
                                SecondOwner = IOwnerDetailsTD[1].Text.Trim();
                                if (SecondOwner != "")
                                {
                                    FirstOwner = "&" + SecondOwner;
                                }
                            }
                            if (IOwnerDetailsTD.Count != 0 && owner.Text.Contains("Street 1"))
                            {
                                StreetNo = IOwnerDetailsTD[1].Text;
                            }
                            if (IOwnerDetailsTD.Count != 0 && owner.Text.Contains("Street 2"))
                            {
                                Street = IOwnerDetailsTD[1].Text.Trim();
                                if (Street != "")
                                {
                                    StreetNo = " " + Street;
                                }
                            }
                            if (IOwnerDetailsTD.Count != 0 && owner.Text.Contains("City"))
                            {
                                City = IOwnerDetailsTD[1].Text;
                            }
                            if (IOwnerDetailsTD.Count != 0 && owner.Text.Contains("State"))
                            {
                                State = IOwnerDetailsTD[1].Text;
                            }
                            if (IOwnerDetailsTD.Count != 0 && owner.Text.Contains("Zip"))
                            {
                                Zip = IOwnerDetailsTD[1].Text;
                            }
                            if (IOwnerDetailsTD.Count != 0 && owner.Text.Contains("Volume"))
                            {
                                Volume = IOwnerDetailsTD[1].Text;
                            }
                            if (IOwnerDetailsTD.Count != 0 && owner.Text.Contains("Page"))
                            {
                                Page = IOwnerDetailsTD[1].Text;
                            }
                            if (IOwnerDetailsTD.Count != 0 && owner.Text.Contains("Deed Date"))
                            {
                                DeedDate = IOwnerDetailsTD[1].Text;
                            }
                        }

                        //Dwelling Details
                        string YearBuilt = "";
                        IWebElement IdwellingDetails = driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td/table/tbody/tr[4]/td/table/tbody/tr/td/table/tbody/tr/td[1]/table/tbody/tr/td/table/tbody/tr[3]/td/table/tbody/tr[2]/td/table/tbody/tr/td/table/tbody"));
                        IList<IWebElement> IdwellingDetailsRow = IdwellingDetails.FindElements(By.TagName("tr"));
                        IList<IWebElement> IdwellingDetailsTD;
                        foreach (IWebElement dewelling in IdwellingDetailsRow)
                        {
                            IdwellingDetailsTD = dewelling.FindElements(By.TagName("td"));
                            if (IdwellingDetailsTD.Count != 0 && dewelling.Text.Contains("Year Built"))
                            {
                                YearBuilt = IdwellingDetailsTD[1].Text;
                            }
                        }
                        //Alternate ID/Map Block Lot~Card~Card~Street Name~Street Number~Zoning~LUC~Acres~Owner~Mailling Address~City~State~Zip~Volume~Page~Deed Date~Year Built 
                        string propertyDetails = AlternateID + "~" + FirstCard + "~" + SecondCard + "~" + StNo + "~" + StName + "~" + Zoining + "~" + LUC + "~" + Acres + "~" + FirstOwner + "~" + StreetNo + "~" + City + "~" + State + "~" + Zip + "~" + Volume + "~" + Page + "~" + DeedDate + "~" + YearBuilt;
                        gc.insert_date(orderNumber, parcelNumber, 2160, propertyDetails, 1, DateTime.Now);

                        string property9 = "Alternate ID/Map Block Lot~First Card~Second Card~Street Name~Street Number~Zoning~LUC~Acres~Owner~Mailling Address~City~State~Zip~Volume~Page~Deed Date~Year Built";
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property9 + "' where Id = '" + 2160 + "'");



                        //Valuations Details
                        string ValuationTitle = "", ValuationValue = "";
                        IWebElement IvaluationsDetails = driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td/table/tbody/tr[4]/td/table/tbody/tr/td/table/tbody/tr/td[1]/table/tbody/tr/td/table/tbody/tr[5]/td/table/tbody/tr[2]/td/table/tbody/tr/td/table/tbody"));
                        IList<IWebElement> IvaluationsDetailsRow = IvaluationsDetails.FindElements(By.TagName("tr"));
                        IList<IWebElement> IvaluationsDetailsTD;
                        foreach (IWebElement valuation in IvaluationsDetailsRow)
                        {
                            IvaluationsDetailsTD = valuation.FindElements(By.TagName("td"));
                            if (IvaluationsDetailsTD.Count == 3 && !valuation.Text.Contains("Valuation:"))
                            {
                                ValuationTitle += IvaluationsDetailsTD[0].Text + "~";
                                ValuationValue += IvaluationsDetailsTD[1].Text + "~";
                            }
                        }
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + ValuationTitle.Remove(ValuationTitle.Length - 1, 1) + "' where Id = '" + 2161 + "'");
                        gc.insert_date(orderNumber, parcelNumber, 2161, ValuationValue.Remove(ValuationValue.Length - 1, 1), 1, DateTime.Now);
                        //Appraised Land~Appraised Land PA490~Appraised Bldg~Appraised Total~Total Assessment 

                        try
                        {
                            //Sales History Details
                            IWebElement ISalesDetails = driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td/table/tbody/tr[5]/td/table/tbody/tr[2]/td/table/tbody/tr/td/table/tbody"));
                            IList<IWebElement> ISalesDetailsRow = ISalesDetails.FindElements(By.TagName("tr"));
                            IList<IWebElement> ISalesDetailsTD;
                            foreach (IWebElement sale in ISalesDetailsRow)
                            {
                                ISalesDetailsTD = sale.FindElements(By.TagName("td"));
                                if (ISalesDetailsTD.Count != 0 && !sale.Text.Contains("Book"))
                                {
                                    //Book~Page~Sale Date~Price~Validity~Sale Type
                                    string SaleDetails = ISalesDetailsTD[0].Text + "~" + ISalesDetailsTD[1].Text + "~" + ISalesDetailsTD[2].Text + "~" + ISalesDetailsTD[3].Text + "~" + ISalesDetailsTD[4].Text + "~" + ISalesDetailsTD[5].Text;
                                    gc.insert_date(orderNumber, parcelNumber, 2162, SaleDetails, 1, DateTime.Now);
                                }
                            }
                            string property94 = "Book~Page~Sale Date~Price~Validity~Sale Type";
                            db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property94 + "' where Id = '" + 2162 + "'");
                        }
                        catch { }
                    }
                    #endregion
                    #region Four Assessment Link
                    if (countAssess == "4")//Darien
                    {

                        Thread.Sleep(1000);
                        //Property Details
                        string Parid = "", Owner = "", Address = "", title = "", Value = "";

                        IWebElement Propertydet = driver.FindElement(By.XPath("//*[@id='datalet_header_row']/td/table/tbody"));
                        IList<IWebElement> TRPropertyvalue = Propertydet.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDPropertyvalue;
                        foreach (IWebElement row in TRPropertyvalue)
                        {
                            TDPropertyvalue = row.FindElements(By.TagName("td"));

                            if (TDPropertyvalue.Count == 2 && TDPropertyvalue.Count != 0 && row.Text.Trim() != "" && row.Text.Contains("PARID:"))
                            {
                                Parid = TDPropertyvalue[0].Text.Replace("PARID:", "").Trim();
                                parcelNumber = Parid;
                                uniqueidMap = Parid;
                            }
                            if (TDPropertyvalue.Count == 2 && TDPropertyvalue.Count != 0 && row.Text.Trim() != "" && !row.Text.Contains("PARID:"))
                            {
                                Owner = TDPropertyvalue[0].Text.Trim();
                                Address = TDPropertyvalue[1].Text.Trim();
                            }
                        }

                        IWebElement Propertydet1 = driver.FindElement(By.Id("Parcel"));
                        IList<IWebElement> TRPropertyvalue1 = Propertydet1.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDPropertyvalue1;
                        foreach (IWebElement row1 in TRPropertyvalue1)
                        {
                            TDPropertyvalue1 = row1.FindElements(By.TagName("td"));

                            if (TDPropertyvalue1.Count == 2 && TDPropertyvalue1[0].Text.Trim() != "" && TDPropertyvalue1.Count != 0 && row1.Text.Trim() != "" && !row1.Text.Contains("Address") && !row1.Text.Contains("Notes"))
                            {
                                title += TDPropertyvalue1[0].Text.Trim() + "~";
                                Value += TDPropertyvalue1[1].Text.Trim() + "~";
                            }
                        }

                        //Owner~Address~MapLot~Class~LandUse
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + "Owner~Property Address~" + title.Remove(title.Length - 1, 1) + "' where Id = '" + 2160 + "'");
                        gc.insert_date(orderNumber, Parid, 2160, Owner + "~" + Address + "~" + Value.Remove(Value.Length - 1).Trim(), 1, DateTime.Now);

                        //Assessment Details
                        //Screenshots
                        driver.FindElement(By.XPath("//*[@id='sidemenu']/li[2]/a")).Click();
                        Thread.Sleep(1000);
                        gc.CreatePdf(orderNumber, Parid, "Sales Details", driver, "CT", countynameCT);
                        driver.FindElement(By.XPath("//*[@id='sidemenu']/li[3]/a")).Click();
                        Thread.Sleep(1000);
                        gc.CreatePdf(orderNumber, Parid, "Residential Details", driver, "CT", countynameCT);
                        driver.FindElement(By.XPath("//*[@id='sidemenu']/li[7]/a")).Click();
                        Thread.Sleep(1000);
                        gc.CreatePdf(orderNumber, Parid, "Values Details", driver, "CT", countynameCT);

                        string DescriptionAppraised = "", Appraisedvalue = "", DescriptionAssessed = "", Assessedvalue = "";

                        //Appraised values and Assessed Values
                        IWebElement Appraiseddet1 = driver.FindElement(By.Id("datalet_div_0"));
                        IList<IWebElement> TRAppraisedvalue1 = Appraiseddet1.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDAppraisedvalue1;
                        foreach (IWebElement Appraised in TRAppraisedvalue1)
                        {
                            TDAppraisedvalue1 = Appraised.FindElements(By.TagName("td"));

                            if (TDAppraisedvalue1.Count == 2 && TDAppraisedvalue1[0].Text.Trim() != "" && TDAppraisedvalue1.Count != 0 && Appraised.Text.Trim() != "")
                            {
                                Appraisedvalue += TDAppraisedvalue1[1].Text.Trim() + "~";

                            }
                            if (TDAppraisedvalue1.Count == 1 && TDAppraisedvalue1[0].Text.Trim() != "" && TDAppraisedvalue1.Count != 0 && Appraised.Text.Trim() != "")
                            {
                                DescriptionAppraised = TDAppraisedvalue1[0].Text.Trim();
                            }
                        }
                        Appraisedvalue = Appraisedvalue.TrimEnd('~');
                        //Type~Land~Building~Total
                        gc.insert_date(orderNumber, Parid, 2161, DescriptionAppraised + "~" + Appraisedvalue, 1, DateTime.Now);

                        IWebElement Assesseddet1 = driver.FindElement(By.Id("datalet_div_1"));
                        IList<IWebElement> TRAssessedvalue1 = Assesseddet1.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDAssessedvalue1;
                        foreach (IWebElement Assessed in TRAssessedvalue1)
                        {
                            TDAssessedvalue1 = Assessed.FindElements(By.TagName("td"));

                            if (TDAssessedvalue1.Count == 2 && TDAssessedvalue1[0].Text.Trim() != "" && TDAssessedvalue1.Count != 0 && Assessed.Text.Trim() != "")
                            {
                                Assessedvalue += TDAssessedvalue1[1].Text.Trim() + "~";
                            }
                            if (TDAssessedvalue1.Count == 1 && TDAssessedvalue1[0].Text.Trim() != "" && TDAssessedvalue1.Count != 0 && Assessed.Text.Trim() != "")
                            {
                                DescriptionAssessed = TDAssessedvalue1[0].Text.Trim();
                            }
                        }
                        Assessedvalue = Assessedvalue.TrimEnd('~');
                        //Type~Land~Building~Total
                        gc.insert_date(orderNumber, Parid, 2161, DescriptionAssessed + "~" + Assessedvalue, 1, DateTime.Now);
                        string property9 = "Type~Land~Building~Total";
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property9 + "' where Id = '" + 2161 + "'");

                        driver.FindElement(By.XPath("//*[@id='sidemenu']/li[8]/a")).Click();
                        Thread.Sleep(1000);
                        gc.CreatePdf(orderNumber, Parid, "Permits Details", driver, "CT", countynameCT);
                    }
                    #endregion
                    #region five Assessment Link
                    if (countAssess == "5") //
                    {

                        string Gis = "", accountno = "", parcelid = "", owner = "", location = "", mailingaddress = "";
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
                            }
                            if (propertyTD.Count != 0 && line.Text.Contains("Account Number"))
                            {
                                accountno = propertyTD[0].Text.Replace("Account Number\r\n", "");
                            }
                            if (propertyTD.Count != 0 && line.Text.Contains("Owner"))
                            {
                                owner = propertyTD[0].Text.Replace("Owner\r\n", "");
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
                        if (townshipcode == "24")
                        {
                            string[] uniquearray = Gis.Split('-');
                            string part1 = "", part11 = "", part2 = "", part21 = "";
                            //     uniqueidMap
                            part1 = uniquearray[0];
                            part2 = uniquearray[1];
                            string[] part1array = part1.Split('.');
                            string[] part2array = part2.Split('.');
                            if (part1array[1] == "")
                            {
                                part1array[1] = " ";
                            }
                            try
                            {
                                uniqueidMap = part1array[0] + part1array[1] + " " + part2array[0] + part2array[1];
                            }
                            catch
                            {
                                uniqueidMap = part1array[0] + part1array[1] + " " + part2array[0];
                            }
                            parcelNumber = uniqueidMap;
                            assessment_id = uniqueidMap;
                        }
                        if (townshipcode == "22")
                        {
                            parcelNumber = parcelid;
                            assessment_id = parcelid;
                        }
                        string Propetyresult = Gis + "~" + parcelid + "~" + accountno + "~" + owner + "~" + location + "~" + mailingaddress;
                        string property9 = "GIS ID~Parcel ID~Account Number~Owner~Property Address~Mailing Address";
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property9 + "' where Id = '" + 2160 + "'");
                        gc.insert_date(orderNumber, parcelNumber, 2160, Propetyresult, 1, DateTime.Now);


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

                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + AssessedResult.Remove(AssessedResult.Length - 1, 1) + "' where Id = '" + 2161 + "'");
                        gc.insert_date(orderNumber, parcelNumber, 2161, AppraisedValue.Remove(AppraisedValue.Length - 1, 1), 1, DateTime.Now);
                        gc.insert_date(orderNumber, parcelNumber, 2161, AssessedValue.Remove(AssessedValue.Length - 1, 1), 1, DateTime.Now);

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
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property.Remove(property.Length - 1, 1) + "' where Id = '" + 2162 + "'");
                        gc.insert_date(orderNumber, parcelNumber, 2162, information.Remove(information.Length - 1, 1), 1, DateTime.Now);


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
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + salesresult.Remove(salesresult.Length - 1, 1) + "' where Id = '" + 2163 + "'");
                        gc.insert_date(orderNumber, parcelNumber, 2163, salesinformation.Remove(salesinformation.Length - 1, 1), 1, DateTime.Now);
                        gc.CreatePdf(orderNumber, parcelNumber, "property Details", driver, "CT", countynameCT);
                        //   hrefparcellink
                        // gc.downloadfile(hrefparcellink, orderNumber, parcelNumber, "aas", "CT", countynameCT);
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
                            filename = latestfilename();
                            gc.AutoDownloadFile(orderNumber, assessment_id, countynameCT, "CT", filename);
                            Thread.Sleep(2000);
                            driver1.Quit();
                        }
                        catch
                        {
                            driver1.Quit();
                        }
                        if (townshipcode == "22")
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
                            }
                            catch { }
                        }
                    }
                    #endregion
                    #region six Assessment Link
                    if (countAssess == "6")//Strafford
                    {
                        //Property Details
                        string Parcel_ID = "", AccountNumber = "", PropertyAddress = "", MapLot = "", UseClass = "", AssessNeighborhood = "", CensusTract = "", Acreage = "", Overall_OwnMailAdd = "", OwnerName = "", Mailing_Address1 = "", Mailing_Address2 = "", YearBuilt = "";
                        IWebElement PropertyTB = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_mSection']/div/table/tbody"));
                        IList<IWebElement> PropertyTR = PropertyTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> PropertyTD;
                        foreach (IWebElement Property in PropertyTR)
                        {
                            PropertyTD = Property.FindElements(By.TagName("td"));
                            if (PropertyTD.Count != 0)
                            {
                                if (Property.Text.Contains("ParcelId"))
                                {
                                    Parcel_ID = PropertyTD[1].Text;
                                    parcelNumber = Parcel_ID;
                                }

                                if (Property.Text.Contains("Account Number"))
                                {
                                    AccountNumber = PropertyTD[1].Text;

                                }

                                if (Property.Text.Contains("Location Address"))
                                {
                                    PropertyAddress = PropertyTD[1].Text;
                                }

                                if (Property.Text.Contains("Map-Block-Lot"))
                                {
                                    MapLot = gc.Between(PropertyTB.Text, "Map-Block-Lot", "Use Class/Description").Trim();
                                }

                                if (Property.Text.Contains("Use Class/Description"))
                                {
                                    UseClass = PropertyTD[1].Text;
                                }

                                if (Property.Text.Contains("Assessing Neighborhood"))
                                {
                                    AssessNeighborhood = PropertyTD[1].Text;
                                }

                                if (Property.Text.Contains("Acreage"))
                                {
                                    Acreage = PropertyTD[1].Text;
                                }

                                if (Property.Text.Contains("Census Tract"))
                                {
                                    CensusTract = PropertyTD[1].Text;
                                }
                            }
                        }

                        try
                        {
                            OwnerName = driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_lstOwners_ctl01_lblOwnerFirstName")).Text;
                            Mailing_Address1 = driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_lstOwners_ctl01_lblAddress1")).Text;
                            Mailing_Address2 = driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_lstOwners_ctl01_lblAddress2")).Text;
                            Overall_OwnMailAdd = OwnerName + Mailing_Address1 + " " + Mailing_Address2;
                        }
                        catch
                        { }

                        try
                        {
                            IWebElement Yeartb = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl05_mSection']/div/table/tbody"));
                            IList<IWebElement> YearTR = Yeartb.FindElements(By.TagName("tr"));
                            IList<IWebElement> YearTD;

                            foreach (IWebElement Built in YearTR)
                            {
                                YearTD = Built.FindElements(By.TagName("td"));
                                if (YearTD.Count != 0)
                                {
                                    if (Built.Text.Contains("Year Built") || Built.Text.Contains("Actual Year Built"))
                                    {
                                        YearBuilt = YearTD[1].Text;
                                    }
                                }
                            }
                        }
                        catch
                        { }

                        string PropertyDetails = AccountNumber + "~" + PropertyAddress + "~" + MapLot + "~" + UseClass + "~" + AssessNeighborhood + "~" + CensusTract + "~" + Acreage + "~" + Overall_OwnMailAdd + "~" + YearBuilt;
                        gc.CreatePdf(orderNumber, Parcel_ID, "Property Details", driver, "CT", countynameCT);
                        gc.insert_date(orderNumber, Parcel_ID, 2160, PropertyDetails, 1, DateTime.Now);
                        //Account Number~Property Address~Map-Block-Lot~Use Class/Description~Assessing Neighborhood~Census Tract~Acreage~Owner~Year Built

                        string property94 = "Account Number~Property Address~Map-Block-Lot~Use Class/Description~Assessing Neighborhood~Census Tract~Acreage~Owner~Year Built";
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property94 + "' where Id = '" + 2160 + "'");


                        //Appraised Details
                        string FirstYear = "", SecondYear = "", Appraised = "", FirstAppraised = "", SecondAppraised = "";
                        IWebElement AppmThTb = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl02_ctl01_grdValuation']/thead"));
                        IList<IWebElement> AppmThTr = AppmThTb.FindElements(By.TagName("tr"));
                        IList<IWebElement> AppmTh;

                        foreach (IWebElement Appm in AppmThTr)
                        {
                            AppmTh = Appm.FindElements(By.TagName("th"));
                            if (AppmTh.Count != 0)
                            {
                                FirstYear = AppmTh[0].Text;
                                SecondYear = AppmTh[1].Text;
                            }
                        }

                        IWebElement AppmTb = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl02_ctl01_grdValuation']/tbody"));
                        IList<IWebElement> AppmTr = AppmTb.FindElements(By.TagName("tr"));
                        IList<IWebElement> AppmTd;
                        foreach (IWebElement Appmrow in AppmTr)
                        {
                            AppmTd = Appmrow.FindElements(By.TagName("td"));

                            if (AppmTd.Count != 0)
                            {
                                Appraised += AppmTd[1].Text + "~";
                                FirstAppraised += AppmTd[2].Text + "~";
                                SecondAppraised += AppmTd[3].Text + "~";
                            }
                        }
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + "Year~Type~" + Appraised.Remove(Appraised.Length - 1, 1) + "' where Id = '" + 2161 + "'");
                        gc.insert_date(orderNumber, Parcel_ID, 2161, FirstYear + "~Appraised Value~" + FirstAppraised.Remove(FirstAppraised.Length - 1, 1), 1, DateTime.Now);
                        gc.insert_date(orderNumber, Parcel_ID, 2161, SecondYear + "~Appraised Value~" + SecondAppraised.Remove(SecondAppraised.Length - 1, 1), 1, DateTime.Now);
                        //Year~Type~First Year~Second Year

                        //Assessment Details
                        string AssFirstYear = "", AssSecondYear = "", Assessment = "", FirstAssessment = "", SecondAssessment = "";
                        IWebElement AssmThTb = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl02_ctl01_grdValuation']/thead"));
                        IList<IWebElement> AssmThTr = AssmThTb.FindElements(By.TagName("tr"));
                        IList<IWebElement> AssmTh;

                        foreach (IWebElement Assm in AssmThTr)
                        {
                            AssmTh = Assm.FindElements(By.TagName("th"));
                            if (AssmTh.Count != 0)
                            {
                                AssFirstYear = AssmTh[0].Text;
                                AssSecondYear = AssmTh[1].Text;
                            }
                        }

                        IWebElement AssmTb = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl02_ctl01_grdValuation']/tbody"));
                        IList<IWebElement> AssmTr = AssmTb.FindElements(By.TagName("tr"));
                        IList<IWebElement> AssmTd;
                        foreach (IWebElement Assmrow in AssmTr)
                        {
                            AssmTd = Assmrow.FindElements(By.TagName("td"));

                            if (AssmTd.Count != 0)
                            {
                                Assessment += AssmTd[1].Text + "~";
                                FirstAssessment += AssmTd[2].Text + "~";
                                SecondAssessment += AssmTd[3].Text + "~";
                            }
                        }
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + "Year~Type~" + Assessment.Remove(Assessment.Length - 1, 1) + "' where Id = '" + 2161 + "'");
                        gc.insert_date(orderNumber, Parcel_ID, 2161, AssFirstYear + "~Assessment History~" + FirstAssessment.Remove(FirstAssessment.Length - 1, 1), 1, DateTime.Now);
                        gc.insert_date(orderNumber, Parcel_ID, 2161, AssSecondYear + "~Assessment History~" + SecondAssessment.Remove(SecondAssessment.Length - 1, 1), 1, DateTime.Now);
                        //Year~Type~First Year~Second Year

                        //Land                   
                        IWebElement ILand = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl04_ctl01_grdLand']/tbody"));
                        IList<IWebElement> ILandRow = ILand.FindElements(By.TagName("tr"));
                        IList<IWebElement> ILandTD;
                        foreach (IWebElement land in ILandRow)
                        {
                            ILandTD = land.FindElements(By.TagName("td"));
                            if (ILandTD.Count != 0)
                            {
                                string LandDetails = ILandTD[0].Text + "~" + ILandTD[1].Text + "~" + ILandTD[2].Text + "~" + ILandTD[3].Text + "~" + ILandTD[4].Text;
                                gc.insert_date(orderNumber, Parcel_ID, 2162, LandDetails, 1, DateTime.Now);
                                //Use~Class~Zoning~Area~Value
                            }
                        }
                        string prop = "Use~Class~Zoning~Area~Value";
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + prop + "' where Id = '" + 2162 + "'");
                        try
                        {
                            //Sales History                    
                            IWebElement ISales = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl08_ctl01_grdSalesHist']/tbody"));
                            IList<IWebElement> ISalesRow = ISales.FindElements(By.TagName("tr"));
                            IList<IWebElement> ISalesTD;
                            foreach (IWebElement sale in ISalesRow)
                            {
                                ISalesTD = sale.FindElements(By.TagName("td"));
                                if (ISalesTD.Count != 0)
                                {
                                    string SalesDetails = ISalesTD[0].Text + "~" + ISalesTD[1].Text + "~" + ISalesTD[2].Text + "~" + ISalesTD[3].Text + "~" + ISalesTD[4].Text + "~" + ISalesTD[5].Text;
                                    gc.insert_date(orderNumber, Parcel_ID, 2163, SalesDetails, 1, DateTime.Now);
                                    //Sales~DateType of Document~Grantee~Vacant/Improved~Book/Page~Amount
                                }
                            }
                            string prop1 = "Sales~DateType of Document~Grantee~Vacant/Improved~Book/Page~Amount";
                            db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + prop1 + "' where Id = '" + 2163 + "'");
                        }
                        catch { }
                        try
                        {
                            //Permit Information               
                            IWebElement IPermit = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl10_ctl01_grdPermits']/tbody"));
                            IList<IWebElement> IPermitRow = IPermit.FindElements(By.TagName("tr"));
                            IList<IWebElement> IPermitTD;
                            foreach (IWebElement permit in IPermitRow)
                            {
                                IPermitTD = permit.FindElements(By.TagName("td"));
                                if (IPermitTD.Count != 0)
                                {
                                    string PermitDetails = IPermitTD[0].Text + "~" + IPermitTD[1].Text + "~" + IPermitTD[2].Text + "~" + IPermitTD[3].Text + "~" + IPermitTD[4].Text + "~" + IPermitTD[5].Text + "~" + IPermitTD[6].Text + "~" + IPermitTD[7].Text + "~" + IPermitTD[8].Text;
                                    gc.insert_date(orderNumber, Parcel_ID, 2164, PermitDetails, 1, DateTime.Now);
                                    //Permit ID~Issue Date~Type~Description~Amount~Inspection Date~Complete~Date Complete~Comments
                                }
                            }
                            string prop2 = "Permit ID~Issue Date~Type~Description~Amount~Inspection Date~Complete~Date Complete~Comments";
                            db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + prop2 + "' where Id = '" + 2164 + "'");
                        }
                        catch { }

                        if (townshipcode == "28")
                        {
                            uniqueidMap = AccountNumber;
                            assessment_id = AccountNumber;
                        }
                    }
                    #endregion

                    #region 13 AssessmentLink
                    if (countAssess == "13")//New Canaan Assessment
                    {
                        //string Propertydetail = driver.FindElement(By.XPath("//*[@id='maintablediv']/table/tbody")).Text;
                        //location
                        string Location = "", owner = "", Account = "", mblu = "", Ownerhistoryresult = "", Item = "", Appraised_Value = "", Assessed_Value = "", YearBuilt = "";
                        IWebElement Propertdetailtable = driver.FindElement(By.XPath("//*[@id='maintablediv']/table/tbody"));
                        IList<IWebElement> propertdetailrow = Propertdetailtable.FindElements(By.TagName("tr"));
                        IList<IWebElement> propertydetailTD;
                        foreach (IWebElement Propertydetail in propertdetailrow)
                        {
                            propertydetailTD = Propertydetail.FindElements(By.TagName("td"));
                            if (propertydetailTD.Count != 0 && !Propertydetail.Text.Contains("Location"))
                            {
                                Location = propertydetailTD[0].Text;
                                owner = propertydetailTD[1].Text;
                                Account = propertydetailTD[2].Text;
                                mblu = propertydetailTD[3].Text;
                            }
                        }
                        string Ownerrecored = driver.FindElement(By.Id("owner")).Text;
                        try
                        {
                            string Yearbuilttable = driver.FindElement(By.XPath("//*[@id='const']/table[1]/tbody")).Text;
                            YearBuilt = GlobalClass.After(Yearbuilttable, "Year Built");
                        }
                        catch { }
                        string property1 = "Property Address~Owner~MBLU~Owner of Record~Year Built";
                        string propertydetailresult = Location + "~" + owner + "~" + mblu + "~" + Ownerrecored + "~" + YearBuilt;
                        dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property1 + "' where Id = '2160'");
                        gc.insert_date(orderNumber, Account, 2160, propertydetailresult, 1, DateTime.Now);
                        gc.CreatePdf(orderNumber, Account, "ParcelSearch after", driver, "CT", countynameCT);
                        //Assessment Detail
                        IWebElement Assessmentdetailtable = driver.FindElement(By.Id("pvalue"));
                        IList<IWebElement> Assessmentdetailrow = Assessmentdetailtable.FindElements(By.TagName("tr"));
                        IList<IWebElement> AssessmentdetailTD;
                        foreach (IWebElement Assessmentdetail in Assessmentdetailrow)
                        {
                            AssessmentdetailTD = Assessmentdetail.FindElements(By.TagName("td"));
                            if (AssessmentdetailTD.Count != 0 && !Assessmentdetail.Text.Contains("Item"))
                            {
                                Item += AssessmentdetailTD[0].Text + "~";
                                Appraised_Value += AssessmentdetailTD[1].Text + "~";
                                Assessed_Value += AssessmentdetailTD[2].Text + "~";
                            }
                        }
                        dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + Item.Remove(Item.Length - 1) + "' where Id = '2161'");
                        gc.insert_date(orderNumber, Account, 2161, Appraised_Value.Remove(Appraised_Value.Length - 1), 1, DateTime.Now);
                        gc.insert_date(orderNumber, Account, 2161, Assessed_Value.Remove(Assessed_Value.Length - 1), 1, DateTime.Now);
                        //Ownerhistory
                        IWebElement ownerHistorytable = driver.FindElement(By.Id("ownerhistory"));
                        IList<IWebElement> ownerHistoryrow = ownerHistorytable.FindElements(By.TagName("tr"));
                        IList<IWebElement> ownerHistoryTD;
                        foreach (IWebElement ownerHistory in ownerHistoryrow)
                        {
                            ownerHistoryTD = ownerHistory.FindElements(By.TagName("td"));
                            if (ownerHistoryTD.Count != 0 && !ownerHistory.Text.Contains("Name"))
                            {
                                Ownerhistoryresult = ownerHistoryTD[0].Text + "~" + ownerHistoryTD[1].Text + "~" + ownerHistoryTD[2].Text + "~" + ownerHistoryTD[3].Text;
                                gc.insert_date(orderNumber, Account, 2162, Ownerhistoryresult, 1, DateTime.Now);
                            }
                        }
                        string OwnerhistoryHeading = "Name~Book/Page~Sale Date~Sale Price";
                        dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + OwnerhistoryHeading + "' where Id = '2162'");
                        //gc.insert_date(orderNumber, assessment_id, 2162, Ownerhistoryresult, 1, DateTime.Now);
                        //Assessment History
                        driver.FindElement(By.Id("view-more-history")).Click();
                        Thread.Sleep(2000);
                        IWebElement AssessmentHistorytable = driver.FindElement(By.Id("ashist"));
                        IList<IWebElement> AssessmentHistoryrow = AssessmentHistorytable.FindElements(By.TagName("tr"));
                        IList<IWebElement> AssessmentHistoryTD;
                        foreach (IWebElement AssessmentHistory in AssessmentHistoryrow)
                        {
                            AssessmentHistoryTD = AssessmentHistory.FindElements(By.TagName("td"));
                            if (AssessmentHistoryTD.Count != 0 && !AssessmentHistory.Text.Contains("Year"))
                            {
                                string AssessmentHistoryresult = AssessmentHistoryTD[0].Text + "~" + AssessmentHistoryTD[1].Text;
                                gc.insert_date(orderNumber, Account, 2163, AssessmentHistoryresult, 1, DateTime.Now);
                            }
                        }
                        string AssessmenthistoryHeading = "Year~Total Assessment";
                        dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + AssessmenthistoryHeading + "' where Id = '2163'");
                        //Building Permits 
                        IWebElement Buildingpermittable = driver.FindElement(By.Id("perm"));
                        IList<IWebElement> Buildingpermitrow = Buildingpermittable.FindElements(By.TagName("tr"));
                        IList<IWebElement> BuildingpermitTD;
                        foreach (IWebElement Buildingpermit in Buildingpermitrow)
                        {
                            BuildingpermitTD = Buildingpermit.FindElements(By.TagName("td"));
                            if (BuildingpermitTD.Count != 0 && !Buildingpermit.Text.Contains("Permit ID"))
                            {
                                string Buildingpermitresult = BuildingpermitTD[0].Text + "~" + BuildingpermitTD[1].Text + "~" + BuildingpermitTD[2].Text + "~" + BuildingpermitTD[3].Text;
                                gc.insert_date(orderNumber, Account, 2164, Buildingpermitresult, 1, DateTime.Now);
                            }
                        }
                        string BuildingPermitsHeading = "Permit Id~Issue Date~Amount~Description";
                        dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + BuildingPermitsHeading + "' where Id = '2164'");
                        //Land Line Valuation 
                        IWebElement LandLineValuationtable = driver.FindElement(By.Id("land"));
                        IList<IWebElement> LandLineValuationrow = LandLineValuationtable.FindElements(By.TagName("tr"));
                        IList<IWebElement> LandLineValuationTD;
                        foreach (IWebElement LandLineValuation in LandLineValuationrow)
                        {
                            LandLineValuationTD = LandLineValuation.FindElements(By.TagName("td"));
                            if (LandLineValuationTD.Count != 0 && !LandLineValuation.Text.Contains("Size"))
                            {
                                string LandLineValuationresult = LandLineValuationTD[0].Text + "~" + LandLineValuationTD[1].Text + "~" + LandLineValuationTD[2].Text + "~" + LandLineValuationTD[3].Text + "~" + LandLineValuationTD[4].Text;
                                gc.insert_date(orderNumber, Account, 2165, LandLineValuationresult, 1, DateTime.Now);
                            }
                        }
                        string LandLineValuationHeading = "Size~Zone~Dev Map~Appraised Value~Assessed Value";
                        dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + LandLineValuationHeading + "' where Id = '2165'");

                        string[] uniquearray = mblu.Split('/');
                        string part1 = "", part3 = "", part2 = "", part4 = "";
                        string part11 = "", part22 = "", part33 = "", part44 = "";
                        //     uniqueidMap
                        part1 = uniquearray[0].TrimStart('0');
                        part2 = uniquearray[1].Trim().TrimStart('0');
                        // string par2 = part2;
                        part3 = uniquearray[2].Trim().TrimStart('0');
                        if (uniquearray[3].Length != 0)
                        {
                            part4 = uniquearray[3].Trim().TrimStart('0');
                            part44 = " " + " " + " " + part4;

                        }
                        if (part1.Length == 1)
                        {
                            part11 = part1 + " " + " " + " ";
                        }
                        if (part1.Length == 2)
                        {
                            part11 = part1 + " " + " ";
                        }
                        if (part1.Length == 3)
                        {
                            part11 = part1 + " ";
                        }
                        //if (part3.Length == 1)
                        //{
                        //    part33 = " " + " " + " " + part3;
                        //}
                        //if (part3.Length == 2)
                        //{
                        //    part33 = " " + " " + part3;
                        //}
                        //if (part3.Length == 3)
                        //{
                        //    part33 = " " + part3;
                        //}
                        if (part2.Length == 1)
                        {
                            part22 = part2 + " " + " " + " ";
                        }
                        if (part2.Length == 2)
                        {
                            part22 = part2 + " " + " ";
                        }
                        if (part2.Length == 3)
                        {
                            part22 = part2 + " ";
                        }
                        if (part44.Length == 0)
                        {
                            uniqueidMap = part11 + part22 + part3;
                        }
                        if (part44.Length != 0)
                        {
                            uniqueidMap = part11 + part22 + part3 + part44;
                        }
                        parcelNumber = uniqueidMap;
                        assessment_id = uniqueidMap;
                        assessment_id = Regex.Replace(assessment_id, @"\s+", "");
                    }
                    #endregion

                    //Tax details
                    driver.Navigate().GoToUrl(urlTax);
                    Thread.Sleep(3000);
                    #region Zero Tax Link
                    if (countTax == "0")//Bridgeport
                    {

                        if (townshipcode == "04" || townshipcode == "10" || townshipcode == "17")
                        {
                            IWebElement ITaxSelect = driver.FindElement(By.Id("actionType"));
                            SelectElement sTaxSelect = new SelectElement(ITaxSelect);
                            sTaxSelect.SelectByText("Parcel Number");
                            driver.FindElement(By.Name("uniqueId")).SendKeys(uniqueidMap);
                            driver.FindElement(By.Id("searchbtn4")).SendKeys(Keys.Enter);
                            Thread.Sleep(3000);
                        }
                        if (townshipcode == "13" || townshipcode == "22" || townshipcode == "02" || townshipcode == "03" || townshipcode == "08" || townshipcode == "09" || townshipcode == "11" || townshipcode == "12" || townshipcode == "14" || townshipcode == "15" || townshipcode == "18" || townshipcode == "19" || townshipcode == "20" || townshipcode == "23" || townshipcode == "26" || townshipcode == "27" || townshipcode == "32" || townshipcode == "30" || townshipcode == "21" || townshipcode == "07" || townshipcode == "25" || townshipcode == "25" || townshipcode == "01" || townshipcode == "28" || townshipcode == "05" || townshipcode == "24" || townshipcode == "31")
                        {
                            IWebElement ITaxSelect = driver.FindElement(By.Id("actionType"));
                            SelectElement sTaxSelect = new SelectElement(ITaxSelect);
                            sTaxSelect.SelectByText("Unique ID");
                            driver.FindElement(By.XPath("//*[@id='uniqueid']/input[1]")).SendKeys(uniqueidMap);
                            driver.FindElement(By.Id("searchbtn4")).SendKeys(Keys.Enter);
                            Thread.Sleep(3000);
                        }
                        if (townshipcode == "29")
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

                        try
                        {
                            string Nodata = driver.FindElement(By.XPath("//*[@id='notification']/div/span")).Text;
                            if (Nodata.Contains("No record is found"))
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
                                    HttpContext.Current.Session["NoTax_NJ" + countynameCT + township] = "No_Tax";
                                    driver.Quit();
                                    return "No Data in Tax Site";
                                }
                            }
                        }
                        catch { }
                        string BillNumber = "";
                        List<string> InformURL = new List<string>();
                        List<string> HistoryURL = new List<string>();
                        List<string> DownloadURL = new List<string>();
                        List<string> SewerTaxinfo = new List<string>();
                        gc.CreatePdf(orderNumber, assessment_id, "Tax Search Result", driver, "CT", countynameCT);
                        try
                        {
                            string Nodata = driver.FindElement(By.XPath("//*[@id='notification']/div/span")).Text;
                            if (Nodata.Contains("No record is found"))
                            {
                                HttpContext.Current.Session["NoTax_NJ" + countynameCT + township] = "No_Tax";
                                driver.Quit();
                                return "No Data in Tax Site";
                            }
                        }
                        catch { }
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
                                    gc.insert_date(orderNumber, assessment_id, 2167, BillDetails, 1, DateTime.Now);
                                    //Bill~Name/Address~Property/Vehicle~Total Tax~Paid~Outstanding
                                }
                                catch { }
                            }
                        }
                        string tax1 = "Bill~Name/Address~Property/Vehicle~Total Tax~Paid~Outstanding";
                        dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + tax1 + "' where Id = '2167'");

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
                                        string[] Yearsplit = BillNumber.Split('-');
                                        string Yeartax = Yearsplit[0];

                                        if (Yeartax.Trim() == currentyear.Trim())
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
                        try
                        {
                            driver.FindElement(By.Id("sewerMenu")).Click();
                            Thread.Sleep(3000);
                            gc.CreatePdf(orderNumber, assessment_id, "sewer Menu click", driver, "CT", countynameCT);
                            if (townshipcode == "04" || townshipcode == "10" || townshipcode == "17")
                            {
                                IWebElement ITaxSelect = driver.FindElement(By.Id("actionType"));
                                SelectElement sTaxSelect = new SelectElement(ITaxSelect);
                                sTaxSelect.SelectByText("Parcel Number");
                                driver.FindElement(By.Name("uniqueId")).SendKeys(uniqueidMap);
                                driver.FindElement(By.Id("searchbtn4")).SendKeys(Keys.Enter);
                                Thread.Sleep(3000);
                            }
                            if (townshipcode == "13" || townshipcode == "22" || townshipcode == "02" || townshipcode == "03" || townshipcode == "08" || townshipcode == "09" || townshipcode == "11" || townshipcode == "12" || townshipcode == "14" || townshipcode == "15" || townshipcode == "18" || townshipcode == "19" || townshipcode == "20" || townshipcode == "23" || townshipcode == "26" || townshipcode == "27" || townshipcode == "32" || townshipcode == "30" || townshipcode == "21" || townshipcode == "07" || townshipcode == "25" || townshipcode == "25" || townshipcode == "01" || townshipcode == "28" || townshipcode == "05" || townshipcode == "24" || townshipcode == "31")
                            {
                                IWebElement ITaxSelect = driver.FindElement(By.Id("actionType"));
                                SelectElement sTaxSelect = new SelectElement(ITaxSelect);
                                sTaxSelect.SelectByText("Unique ID");
                                driver.FindElement(By.XPath("//*[@id='uniqueid']/input[1]")).SendKeys(uniqueidMap);
                                driver.FindElement(By.Id("searchbtn4")).SendKeys(Keys.Enter);
                                Thread.Sleep(3000);
                            }
                            if (townshipcode == "29")
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
                            gc.CreatePdf(orderNumber, assessment_id, "Unique ID sewer Result", driver, "CT", countynameCT);
                            IWebElement Sewerdetailtable = driver.FindElement(By.XPath("//*[@id='content']/div/div/div/center/div/table/tbody"));
                            IList<IWebElement> Sewerdetailrow = Sewerdetailtable.FindElements(By.TagName("tr"));
                            IList<IWebElement> swerdetailTD;
                            foreach (IWebElement Sewerdetail in Sewerdetailrow)
                            {
                                swerdetailTD = Sewerdetail.FindElements(By.TagName("td"));
                                if (swerdetailTD.Count() != 0)
                                {
                                    string Sewerdetailresult = swerdetailTD[0].Text + "~" + swerdetailTD[1].Text + "~" + swerdetailTD[2].Text + "~" + swerdetailTD[3].Text + "~" + swerdetailTD[4].Text;
                                    gc.insert_date(orderNumber, assessment_id, 2254, Sewerdetailresult, 1, DateTime.Now);
                                }
                            }

                            try
                            {
                                string currentyear = Convert.ToString(DateTime.Now.Year - 1);
                                IWebElement ITaxClick = driver.FindElement(By.XPath("//*[@id='content']/div/div/div/center/div/table/tbody"));
                                IList<IWebElement> ITaxClickRow = ITaxClick.FindElements(By.TagName("tr"));
                                IList<IWebElement> ITaxClickTD;
                                for (int i = 0; i < ITaxClickRow.Count; i++)
                                {
                                    if (ITaxClickRow.Count() != 0)
                                    {
                                        IList<IWebElement> ITaxClickTag;
                                        ITaxClickTD = ITaxClickRow[i].FindElements(By.TagName("td"));
                                        if (ITaxClickRow.Count - 1 == i)
                                        {
                                            BillNumber = GlobalClass.Before(ITaxClickTD[0].Text, "\r\n");
                                            string[] Yearsplit = BillNumber.Split('-');
                                            string Yeartax = Yearsplit[0];
                                            ITaxClickTag = ITaxClickRow[i].FindElements(By.TagName("a"));
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
                            catch { }
                        }
                        catch { }
                        foreach (string Sewerinfo in SewerTaxinfo)
                        {
                            driver.Navigate().GoToUrl(Sewerinfo);
                            Thread.Sleep(2000);
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
                                gc.insert_date(orderNumber, assessment_id, 2168, TaxInformation, 1, DateTime.Now);
                                //Bill~Gross Assessment~Unique ID~Exemptions~District~Net Assessment~Name~Town Mill Rate~Care Of~Property Location~MBL~Town Benefit~Volume & Page~Elderly Benefit (C)

                                string tax2 = "Bill~Gross Assessment~Unique ID~Exemptions~District~Net Assessment~Name~Town Mill Rate~Care Of~Property Location~MBL~Town Benefit~Volume & Page~Elderly Benefit (C)";
                                dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + tax2 + "' where Id = '2168'");


                                string Propertyhead = "";
                                string Propertyresult = "";
                                int counttaxbill = 0;
                                try
                                {
                                    IWebElement multitableElement1 = driver.FindElement(By.XPath("//*[@id='content']/div/div/div/div[1]/table[2]/tbody/tr/td[1]/table/tbody"));
                                    IList<IWebElement> multitableRow1 = multitableElement1.FindElements(By.TagName("tr"));
                                    IList<IWebElement> multirowTD1;
                                    // IList<IWebElement> multirowTH1;
                                    foreach (IWebElement row in multitableRow1)
                                    {
                                        //multirowTH1 = row.FindElements(By.TagName("tH"));
                                        multirowTD1 = row.FindElements(By.TagName("td"));
                                        if (!row.Text.Contains("Total payments") && row.Text.Trim() != "")
                                        {
                                            if (row.Text.Contains("Installment"))
                                            {
                                                for (int i = 0; i < multirowTD1.Count; i++)
                                                {

                                                    Propertyhead += multirowTD1[i].Text + "~";
                                                }
                                                Propertyhead = Propertyhead.TrimEnd('~');
                                                dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + Propertyhead + "' where Id = '2169'");

                                            }
                                            else
                                            {
                                                for (int i = 0; i < multirowTD1.Count; i++)
                                                {
                                                    Propertyresult += multirowTD1[i].Text + "~";
                                                }
                                                Propertyresult = Propertyresult.TrimEnd('~');
                                                gc.insert_date(orderNumber, assessment_id, 2169, Propertyresult, 1, DateTime.Now);
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
                                            gc.insert_date(orderNumber, assessment_id, 2169, Propertyresult, 1, DateTime.Now);
                                            Propertyresult = "";

                                        }
                                    }
                                }
                                catch { }
                                //Tax Payment Details     

                                IWebElement IPaymentDetails = null;
                                try
                                {
                                    IPaymentDetails = driver.FindElement(By.XPath("//*[@id='content']/div/div/div/div[2]/table/tbody"));
                                }
                                catch
                                { }
                                try
                                {
                                    if (IPaymentDetails == null)
                                    {
                                        IPaymentDetails = driver.FindElement(By.XPath("//*[@id='content']/div/div/div/form[2]/div/table/tbody"));
                                    }
                                }
                                catch { }
                                try
                                {
                                    IList<IWebElement> IPaymentDetailsRow = IPaymentDetails.FindElements(By.TagName("tr"));
                                    IList<IWebElement> IPaymentDetailsTD;
                                    foreach (IWebElement bill in IPaymentDetailsRow)
                                    {
                                        IPaymentDetailsTD = bill.FindElements(By.TagName("td"));
                                        if (IPaymentDetailsTD.Count != 0 && !bill.Text.Contains("PAY DATE"))
                                        {
                                            string PaymentDetails = IPaymentDetailsTD[0].Text + "~" + IPaymentDetailsTD[1].Text + "~" + IPaymentDetailsTD[2].Text + "~" + IPaymentDetailsTD[3].Text + "~" + IPaymentDetailsTD[4].Text + "~" + IPaymentDetailsTD[5].Text + "~" + IPaymentDetailsTD[6].Text;
                                            gc.insert_date(orderNumber, assessment_id, 2170, PaymentDetails, 1, DateTime.Now);
                                            //Pay Date~Type~Tax/Principal~Interest~Lien~Fee~Total
                                        }
                                    }
                                    string tax3 = "Pay Date~Type~Tax/Principal~Interest~Lien~Fee~Total";
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + tax3 + "' where Id = '2170'");
                                }
                                catch { }
                                try
                                {
                                    string strTotalPayment = GlobalClass.After(driver.FindElement(By.XPath("//*[@id='content']/div/div/div/center/div[1]")).Text, ":").Trim();
                                    string PaymentDetails = "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + strTotalPayment;

                                    gc.insert_date(orderNumber, assessment_id, 2170, PaymentDetails, 1, DateTime.Now);
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
                                    if (IDueDetailsTD.Count != 0 && due.Text.Contains("Tax/Princ/Bint Due"))
                                    {
                                        TaxPrincBint = IDueDetailsTD[1].Text;
                                    }
                                    if (IDueDetailsTD.Count != 0 && due.Text.Contains("Interest Due"))
                                    {
                                        InterestDue = IDueDetailsTD[1].Text;
                                    }
                                    if (IDueDetailsTD.Count != 0 && due.Text.Contains("Lien Due"))
                                    {
                                        LienDue = IDueDetailsTD[1].Text;
                                    }
                                    if (IDueDetailsTD.Count != 0 && due.Text.Contains("Fee Due"))
                                    {
                                        FeeDue = IDueDetailsTD[1].Text;
                                    }
                                    if (IDueDetailsTD.Count != 0 && due.Text.Contains("Total Due Now"))
                                    {
                                        TotalDue = IDueDetailsTD[1].Text;
                                    }
                                }
                                string DueDetails = TaxPrincBint + "~" + InterestDue + "~" + LienDue + "~" + FeeDue + "~" + TotalDue;
                                gc.insert_date(orderNumber, assessment_id, 2171, DueDetails, 1, DateTime.Now);
                                //Tax/Princ/Bint Due~Interest Due~Lien Due~Fee Due~Total Due
                            }
                            catch { }
                            string tax5 = "Tax/Princ/Bint Due~Interest Due~Lien Due~Fee Due~Total Due";
                            dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + tax5 + "' where Id = '2171'");

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
                                        gc.insert_date(orderNumber, assessment_id, 2172, BillHistoryDetails, 1, DateTime.Now);
                                        //Bill~Type~Paid Date~Tax~Interest~Lien~Fee~Total
                                    }
                                }
                            }
                            catch { }
                            string tax6 = "Bill~Type~Paid Date~Tax~Interest~Lien~Fee~Total";
                            dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + tax6 + "' where Id = '2172'");

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
                            if (townshipcode == "04" || townshipcode == "10" || townshipcode == "17")
                            {
                                IWebElement ITaxSelect = driver.FindElement(By.Id("actionType"));
                                SelectElement sTaxSelect = new SelectElement(ITaxSelect);
                                sTaxSelect.SelectByText("Parcel Number");
                                driver.FindElement(By.Name("uniqueId")).SendKeys(uniqueidMap);
                                driver.FindElement(By.Id("searchbtn4")).SendKeys(Keys.Enter);
                                Thread.Sleep(3000);
                            }
                            if (townshipcode == "13" || townshipcode == "22" || townshipcode == "02" || townshipcode == "03" || townshipcode == "08" || townshipcode == "09" || townshipcode == "11" || townshipcode == "12" || townshipcode == "14" || townshipcode == "15" || townshipcode == "18" || townshipcode == "19" || townshipcode == "20" || townshipcode == "23" || townshipcode == "26" || townshipcode == "27" || townshipcode == "32" || townshipcode == "30" || townshipcode == "21" || townshipcode == "07" || townshipcode == "25" || townshipcode == "25" || townshipcode == "01" || townshipcode == "28" || townshipcode == "05" || townshipcode == "24" || townshipcode == "31")
                            {
                                IWebElement ITaxSelect = driver.FindElement(By.Id("actionType"));
                                SelectElement sTaxSelect = new SelectElement(ITaxSelect);
                                sTaxSelect.SelectByText("Unique ID");
                                driver.FindElement(By.XPath("//*[@id='uniqueid']/input[1]")).SendKeys(uniqueidMap);
                                driver.FindElement(By.Id("searchbtn4")).SendKeys(Keys.Enter);
                                Thread.Sleep(3000);
                            }
                            if (townshipcode == "29")
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
                                            string yearsplit = Billsplit[0];
                                            if (yearsplit.Trim() == currentyear)
                                            {
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
                                                            Thread.Sleep(1000);

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
                                if (townshipcode == "04" || townshipcode == "10" || townshipcode == "17")
                                {
                                    IWebElement ITaxSelect = driver.FindElement(By.Id("actionType"));
                                    SelectElement sTaxSelect = new SelectElement(ITaxSelect);
                                    sTaxSelect.SelectByText("Parcel Number");
                                    driver.FindElement(By.Name("uniqueId")).SendKeys(uniqueidMap);
                                    driver.FindElement(By.Id("searchbtn4")).SendKeys(Keys.Enter);
                                    Thread.Sleep(3000);
                                }
                                if (townshipcode == "13" || townshipcode == "22" || townshipcode == "02" || townshipcode == "03" || townshipcode == "08" || townshipcode == "09" || townshipcode == "11" || townshipcode == "12" || townshipcode == "14" || townshipcode == "15" || townshipcode == "18" || townshipcode == "19" || townshipcode == "20" || townshipcode == "23" || townshipcode == "26" || townshipcode == "27" || townshipcode == "32" || townshipcode == "30" || townshipcode == "21" || townshipcode == "07" || townshipcode == "25" || townshipcode == "25" || townshipcode == "01" || townshipcode == "28" || townshipcode == "05" || townshipcode == "24" || townshipcode == "31")
                                {
                                    IWebElement ITaxSelect = driver.FindElement(By.Id("actionType"));
                                    SelectElement sTaxSelect = new SelectElement(ITaxSelect);
                                    sTaxSelect.SelectByText("Unique ID");
                                    driver.FindElement(By.XPath("//*[@id='uniqueid']/input[1]")).SendKeys(uniqueidMap);
                                    driver.FindElement(By.Id("searchbtn4")).SendKeys(Keys.Enter);
                                    Thread.Sleep(3000);
                                }
                                if (townshipcode == "29")
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
                                                    Thread.Sleep(1000);
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
                        parcelNumber = uniqueidMap;
                        assessment_id = uniqueidMap;
                        driver.Navigate().GoToUrl(urlTax);
                        driver.FindElement(By.Id("searchName")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Result", driver, "CT", countynameCT);
                        driver.FindElement(By.XPath("//*[@id='search_form']/p[2]/input[2]")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Result1", driver, "CT", countynameCT);
                        driver.FindElement(By.XPath("//*[@id='search_form']/p[2]/input[3]")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Result2", driver, "CT", countynameCT);

                        IWebElement Multiaddresstable1add = driver.FindElement(By.Id("resultsTable"));
                        IList<IWebElement> multiaddressrows = Multiaddresstable1add.FindElements(By.TagName("tr"));
                        IList<IWebElement> Multiaddressid;
                        foreach (IWebElement Multiaddress in multiaddressrows)
                        {
                            Multiaddressid = Multiaddress.FindElements(By.TagName("td"));
                            if (Multiaddressid.Count == 8 && !Multiaddress.Text.Contains("Add") && Multiaddressid[3].Text.Contains("REAL ESTATE"))
                            {
                                IWebElement Singleclick = Multiaddressid[7].FindElement(By.TagName("button"));
                                Singleclick.Click();
                                Thread.Sleep(2000);
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
                        gc.insert_date(orderNumber, parcelNumber, 2167, BillHistoryDetails, 1, DateTime.Now);
                        //BillDate~List~Year~Description~Type~First Installment Due Date~First Installment Due Amount~Second Installment Due Date~Second Installment Due Amount~Total Installment Amount~Total Paid
                        string tax1 = "BillDate~List~Year~Description~Type~First Installment Due Date~First Installment Due Amount~Second Installment Due Date~Second Installment Due Amount~Total Installment Amount~Total Paid";
                        dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + tax1 + "' where Id = '2167'");

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
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + title2.Remove(title2.Length - 1, 1) + "' where Id = '" + 2168 + "'");
                        gc.insert_date(orderNumber, parcelNumber, 2168, value2.Remove(value2.Length - 1, 1), 1, DateTime.Now);

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
                                    gc.insert_date(orderNumber, parcelNumber, 2169, PaymentHistorydetails, 1, DateTime.Now);
                                    //List~Principal~Interest~Lien~Penalty~Total
                                    string tax23 = "List~Principal~Interest~Lien~Penalty~Total";
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + tax1 + "' where Id = '2169'");

                                }
                            }
                        }
                        catch { }
                    }
                    #endregion
                    #region two Tax Link
                    if (countTax == "2")//Norwalk
                    {
                        //Tax details
                        string customerid = "", filename = "";

                        try
                        {

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
                                try
                                {
                                    IWebElement ITaxBillSelect = driver1.FindElement(By.Id("RBCategory_0"));
                                    ITaxBillSelect.Click();
                                }
                                catch { }
                                IWebElement IBillSelect = driver1.FindElement(By.Id("DDSearchBy"));
                                SelectElement sBillSelect = new SelectElement(IBillSelect);
                                sBillSelect.SelectByText("Property ID");
                                //sBillSelect.SelectByText("Address");
                                driver1.FindElement(By.Id("txtSearchBy")).SendKeys(uniqueidMap);
                                gc.CreatePdf(orderNumber, assessment_id, "Tax Bill Search", driver1, "CT", countynameCT);
                                driver1.FindElement(By.Id("btnSearch")).SendKeys(Keys.Enter);
                                Thread.Sleep(5000);
                                customerid = driver1.FindElement(By.XPath("//*[@id='GV_PropertyList']/tbody/tr[2]/td[4]")).Text;
                                gc.CreatePdf(orderNumber, assessment_id, "Tax Bill Search Result", driver1, "CT", countynameCT);
                                //driver1.FindElement(By.XPath("//*[@id='GV_PropertyList']/tbody/tr[2]/td[1]/a")).Click();
                                IWebElement IAssess = driver1.FindElement(By.XPath("//*[@id='GV_PropertyList']/tbody/tr[2]/td[1]/a"));
                                IJavaScriptExecutor js = driver1 as IJavaScriptExecutor;
                                js.ExecuteScript("arguments[0].click();", IAssess);
                                Thread.Sleep(3000);
                                gc.CreatePdf(orderNumber, assessment_id, "Tax Bill Result", driver1, "CT", countynameCT);
                                filename = latestfilename();
                                gc.AutoDownloadFile(orderNumber, assessment_id, countynameCT, "CT", filename);

                            }
                            catch { driver1.Quit(); }
                            driver1.Quit();
                        }
                        catch { }
                        driver.Navigate().GoToUrl(urlTax);
                        IWebElement ITaxSelect = driver.FindElement(By.Id("RBCategory_1"));
                        ITaxSelect.Click();
                        IWebElement ISelect = driver.FindElement(By.Id("DDSearchBy"));
                        SelectElement sSelect = new SelectElement(ISelect);
                        sSelect.SelectByText("Customer ID");
                        //sSelect.SelectByText("Address");
                        driver.FindElement(By.Id("txtSearchBy")).SendKeys(customerid);
                        gc.CreatePdf(orderNumber, assessment_id, "Tax Search", driver, "CT", countynameCT);
                        driver.FindElement(By.Id("btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(5000);
                        gc.CreatePdf(orderNumber, assessment_id, "Tax Search Result", driver, "CT", countynameCT);
                        //driver.FindElement(By.XPath("//*[@id='GV_PropertyList']/tbody/tr[2]/td[1]/a")).Click();
                        //gc.CreatePdf(orderNumber, assessment_id, "Tax Result", driver, "CT", countynameCT);
                        try
                        {
                            //driver.FindElement(By.XPath("//*[@id='GV_PropertyList']/tbody/tr[2]/td[1]/a")).Click();
                            IWebElement IAssess1 = driver.FindElement(By.XPath("//*[@id='GV_PropertyList']/tbody/tr[2]/td[1]/a"));
                            IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                            js1.ExecuteScript("arguments[0].click();", IAssess1);
                            Thread.Sleep(3000);
                            gc.CreatePdf(orderNumber, assessment_id, "Tax Result", driver, "CT", countynameCT);

                            //Tax Payment History
                            IWebElement IPaymentHistory = driver.FindElement(By.Id("GV_PaymentDetail"));
                            IList<IWebElement> IPaymentHistoryRow = IPaymentHistory.FindElements(By.TagName("tr"));
                            IList<IWebElement> IPaymentHistoryTD;
                            foreach (IWebElement pay in IPaymentHistoryRow)
                            {
                                IPaymentHistoryTD = pay.FindElements(By.TagName("td"));
                                if (IPaymentHistoryTD.Count != 0 && !pay.Text.Contains("Owner"))
                                {
                                    try
                                    {
                                        string BillDetails = IPaymentHistoryTD[0].Text + "~" + IPaymentHistoryTD[1].Text + "~" + IPaymentHistoryTD[2].Text + "~" + IPaymentHistoryTD[3].Text + "~" + IPaymentHistoryTD[4].Text + "~" + IPaymentHistoryTD[5].Text + "~" + IPaymentHistoryTD[6].Text + "~" + IPaymentHistoryTD[7].Text + "~" + IPaymentHistoryTD[8].Text;
                                        gc.insert_date(orderNumber, assessment_id, 2167, BillDetails, 1, DateTime.Now);
                                        //CustomerId~Owner Name~Description~GLYear~List~Date~Type~Principal~Interest
                                    }
                                    catch { }
                                }
                            }
                            string tax1 = "CustomerId~Owner Name~Description~GLYear~List~Date~Type~Principal~Interest";
                            dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + tax1 + "' where Id = '2167'");
                        }
                        catch { }
                    }
                    #endregion
                    string taxauthority = "Tax Authority";
                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + taxauthority + "' where Id = '2173'");
                    gc.insert_date(orderNumber, assessment_id, 2173, taxCollectorlink, 1, DateTime.Now);

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