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
    public class Webdriver_CTHartford
    {
        string Parcelhref = "";
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        string msg;
        IWebElement addclick;
        IWebElement Divspanrow;
        string multiparceldata = "";
        string countyname = "";
        string uniqueidMap = "";
        string urlAssess = "", urlTax = "", countAssess = "", countTax = "", taxCollectorlink = "";
        int countmulti;
        public string FTP_CTHartford(string streetno, string streetname, string streetdir, string streettype, string assessment_id, string parcelNumber, string searchType, string orderNumber, string directParcel, string ownername, string countynameCT, string statecountyid, string township, string townshipcode)
        {
            IWebDriver driver;
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            int duecount = 0; int RE = 0;
            IWebElement Tbody;
            string hrefCardlink = "", hrefparcellink = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            // driver = new ChromeDriver();
            //driver = new PhantomJSDriver()
            var chromeOptions1 = new ChromeOptions();
            var chromedriver = new ChromeDriver(chromeOptions1);
            DBconnection dbconn = new DBconnection();
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    // var townshipcode1 = new List<string> {"02","03","05","08","09","11","12","14","15","16","18","19","20","23","26","27","29","31","32"};

                    CT_Link linkct = new Scrapsource.CT_Link();
                    string[] urllink = linkct.link(townshipcode, township, countynameCT);
                    string asd11 = "";
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
                        if (countAssess == "0") //address
                        {
                            IWebElement IAddressSelect = driver.FindElement(By.Id("MainContent_ddlSearchSource"));
                            SelectElement sAddressSelect = new SelectElement(IAddressSelect);
                            sAddressSelect.SelectByText("Address");
                            driver.FindElement(By.Id("MainContent_txtSearchAddress")).SendKeys(address.Trim());
                            gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "CT", countynameCT);
                            driver.FindElement(By.XPath("//*[@id='SearchAll']/span[7]")).Click();
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
                                    if (TDmultiaddress.Count == 11 && !row.Text.Contains("Address") && !row.Text.Contains("Results"))
                                    {
                                        //Address~Owner~Account ID~PID
                                        multiparceldata = "Address~Owner~Account ID~MBLU";
                                        string Multi = TDmultiaddress[0].Text + "~" + TDmultiaddress[1].Text + "~" + TDmultiaddress[2].Text + "~" + TDmultiaddress[3].Text + "-" + TDmultiaddress[4].Text + "-" + TDmultiaddress[5].Text + "-" + TDmultiaddress[6].Text + "-" + TDmultiaddress[7].Text + "-" + TDmultiaddress[8].Text + "-" + TDmultiaddress[9].Text;
                                        gc.insert_date(orderNumber, TDmultiaddress[10].Text, 2241, Multi, 1, DateTime.Now);
                                    }
                                }
                                dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2241'");

                                gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "CT", countynameCT);
                                HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }

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
                                            gc.insert_date(orderNumber, TDmultiaddress[4].Text, 2241, Multi, 1, DateTime.Now);
                                        }
                                    }
                                    multiparceldata = "Address~Owner~MBLU~Property Use";
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2241'");

                                    gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "CT", countynameCT);
                                    HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                            }
                            catch { }


                        }
                        if (countAssess == "3")//Bethel
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
                                            gc.insert_date(orderNumber, TDmultiaddress[2].Text, 2241, Multi, 1, DateTime.Now);
                                        }
                                    }
                                    multiparceldata = "Address~LUC~Class~Card";
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2241'");

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
                                        gc.insert_date(orderNumber, GisID, 2241, Multiresult, 1, DateTime.Now);
                                        Max++;
                                        gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "CT", countynameCT);
                                    }

                                }
                                multiparceldata = "Address~Owner~Account Number";
                                dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2241'");

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
                        if (countAssess == "9")//address
                        {
                            driver.FindElement(By.Id("input_st_num")).SendKeys(streetno);
                            driver.FindElement(By.Id("input_st_name")).SendKeys(streetname + " " + streettype);
                            gc.CreatePdf_WOP(orderNumber, "Address search", driver, "CT", countynameCT);
                            //  driver.FindElement(By.XPath("//*[@id='clear_button']/a")).SendKeys(Keys.Enter);
                            Thread.Sleep(7000);
                            gc.CreatePdf_WOP(orderNumber, "Address search Result", driver, "CT", countynameCT);

                            try
                            {
                                string nodata = driver.FindElement(By.XPath("//*[@id='no_results_spaceholder']")).Text;
                                if (nodata.Contains("There are no results"))
                                {
                                    HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }

                            try
                            {
                                IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='paging_box']/span"));
                                string multi = GlobalClass.After(multiaddress.Text, "of").Trim();
                                if (Convert.ToInt32(multi) == 1)
                                {

                                }
                                else if (Convert.ToInt32(multi) < 25 && Convert.ToInt32(multi) > 1)
                                {
                                    IWebElement multiaddress1 = driver.FindElement(By.XPath("//*[@id='results_list']"));

                                    IList<IWebElement> TRmultiaddress = multiaddress1.FindElements(By.TagName("div"));
                                    IList<IWebElement> TDmultiaddress;

                                    foreach (IWebElement row in TRmultiaddress)
                                    {
                                        TDmultiaddress = row.FindElements(By.TagName("div"));
                                        if (TDmultiaddress.Count == 5)
                                        {
                                            string Multi = TDmultiaddress[0].Text + "~" + TDmultiaddress[2].Text;
                                            gc.insert_date(orderNumber, TDmultiaddress[1].Text, 2241, Multi, 1, DateTime.Now);
                                        }
                                    }
                                    multiparceldata = "Address~Owner Name";
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2241'");


                                    gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "CT", countynameCT);
                                    HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";

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
                        //Address Search

                        if (countAssess == "10") //Glastonbury 
                        {
                            chromedriver.Navigate().GoToUrl(urlAssess);
                            IJavaScriptExecutor js = chromedriver as IJavaScriptExecutor;
                            Thread.Sleep(9000);
                            try
                            {
                                chromedriver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/div[2]/div/div/div/div[2]/div[8]/div/div/button")).SendKeys(Keys.Enter);
                            }
                            catch { }

                            try
                            {
                                IWebElement Accept = chromedriver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/div[2]/div/div/div/div[2]/div[8]/div/div/button"));
                                js.ExecuteScript("arguments[0].click();", Accept);
                            }
                            catch { }
                            Thread.Sleep(7000);
                            IWebElement IAddressSearch = chromedriver.FindElement(By.XPath("/html/body/div/div[3]/div[1]/div/div[2]/div[3]/div/div/div[2]/div[1]/ul"));
                            IList<IWebElement> IAddressSearchRow = IAddressSearch.FindElements(By.TagName("li"));
                            IList<IWebElement> IAddressSearchTD;
                            foreach (IWebElement addresssearch in IAddressSearchRow)
                            {
                                IAddressSearchTD = addresssearch.FindElements(By.TagName("button"));
                                if (IAddressSearchTD.Count != 0 && IAddressSearchTD.Count == 2)
                                {
                                    string strAddress = IAddressSearchTD[1].GetAttribute("title");
                                    if (strAddress.Contains("House Number Search") || strAddress.Contains("Search by House Number"))
                                    {
                                        js.ExecuteScript("arguments[0].click();", IAddressSearchTD[1]);
                                        Thread.Sleep(3000);
                                        break;
                                    }
                                }
                            }

                            gc.CreatePdf_WOP_Chrome(orderNumber, "Address Search Agree", chromedriver, "CT", countynameCT);
                            Thread.Sleep(9000);
                            chromedriver.FindElement(By.XPath("/html/body/div/div[3]/div[1]/div/div[3]/div[1]/div[1]/div/div/div[2]/div[11]/div/div[3]/div/div/form/div[1]/div/div[3]/div/div/div/div[1]/input")).Click();
                            chromedriver.FindElement(By.XPath("/html/body/div/div[3]/div[1]/div/div[3]/div[1]/div[1]/div/div/div[2]/div[11]/div/div[3]/div/div/form/div[1]/div/div[3]/div/div/div/div[1]/input")).Clear();
                            chromedriver.FindElement(By.XPath("/html/body/div/div[3]/div[1]/div/div[3]/div[1]/div[1]/div/div/div[2]/div[11]/div/div[3]/div/div/form/div[1]/div/div[3]/div/div/div/div[1]/input")).SendKeys(streetname.Trim().ToUpper() + " " + streettype.Trim().ToUpper());
                            chromedriver.FindElement(By.XPath("/html/body/div/div[3]/div[1]/div/div[3]/div[1]/div[1]/div/div/div[2]/div[11]/div/div[3]/div/div/form/div[2]/button[1]")).SendKeys(Keys.Enter);
                            Thread.Sleep(5000);
                            gc.CreatePdf_WOP_Chrome(orderNumber, "Address Search", chromedriver, "CT", countynameCT);
                            chromedriver.FindElement(By.XPath("/html/body/div/div[3]/div[1]/div/div[3]/div[1]/div[1]/div/div/div[2]/div[11]/div/div[3]/div/div/form/div[1]/div/div[5]/div/div/div/div[1]/input")).Click();
                            chromedriver.FindElement(By.XPath("/html/body/div/div[3]/div[1]/div/div[3]/div[1]/div[1]/div/div/div[2]/div[11]/div/div[3]/div/div/form/div[1]/div/div[5]/div/div/div/div[1]/input")).Clear();
                            chromedriver.FindElement(By.XPath("/html/body/div/div[3]/div[1]/div/div[3]/div[1]/div[1]/div/div/div[2]/div[11]/div/div[3]/div/div/form/div[1]/div/div[5]/div/div/div/div[1]/input")).SendKeys(streetno.Trim().ToUpper());
                            chromedriver.FindElement(By.XPath("/html/body/div/div[3]/div[1]/div/div[3]/div[1]/div[1]/div/div/div[2]/div[11]/div/div[3]/div/div/form/div[2]/button[1]")).SendKeys(Keys.Enter);
                            Thread.Sleep(5000);
                            try
                            {
                                string strMulti = gc.Between(chromedriver.FindElement(By.XPath("/html/body/div/div[3]/div[1]/div/div[3]/div[1]/div[1]/div/div/div[2]/div[7]/div/div[2]/div[1]/div[2]/div[3]")).Text, "Total: ", ")").Trim();
                                if (Convert.ToInt32(strMulti) > 1 && Convert.ToInt32(strMulti) < 25)
                                {
                                    IWebElement singleowner = chromedriver.FindElement(By.XPath("/html/body/div/div[3]/div[1]/div/div[3]/div[1]/div[1]/div/div/div[2]/div[7]/div/div[2]/div[1]/div[2]/div[2]"));
                                    IList<IWebElement> ISOwnerRow = singleowner.FindElements(By.TagName("span"));
                                    foreach (IWebElement sowner in ISOwnerRow)
                                    {
                                        try
                                        {
                                            string strParcel = "", strOwner = "", strAddress = "";
                                            IList<IWebElement> ISParcel = sowner.FindElements(By.TagName("button"));
                                            if (ISParcel.Count != 0 && sowner.Text.Trim() != "" && sowner.Text.Contains("Parcel GIS ID"))
                                            {
                                                strParcel = ISParcel[0].Text.Replace("Parcel GIS ID", "").Trim();
                                            }
                                            IList<IWebElement> ISaddresslist = sowner.FindElements(By.TagName("span"));
                                            if (ISaddresslist.Count != 0 && sowner.Text.Trim() != "" && sowner.Text.Contains("Parcel GIS ID"))
                                            {
                                                IList<IWebElement> ISAddress = sowner.FindElements(By.TagName("p"));
                                                if (ISAddress.Count != 0 && sowner.Text.Trim() != "" && sowner.Text.Contains("Property Address"))
                                                {
                                                    strAddress = ISAddress[1].Text.Replace("Property Address :", "").Trim();
                                                }
                                                if (ISAddress.Count != 0 && sowner.Text.Trim() != "" && sowner.Text.Contains("Owner Name"))
                                                {
                                                    strOwner = ISAddress[0].Text.Replace("Owner Name:", "").Trim();
                                                }
                                            }

                                            if (strParcel != "" && strAddress != "" && strOwner != "")
                                            {
                                                string Multi = strOwner + "~" + strAddress;
                                                gc.insert_date(orderNumber, strParcel, 2241, Multi, 1, DateTime.Now);
                                            }
                                        }
                                        catch { }
                                    }

                                    gc.CreatePdf_WOP(orderNumber, "Address Search Multi", chromedriver, "CT", countynameCT);
                                    HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }

                                if (Convert.ToInt32(strMulti) == 1)
                                {
                                    chromedriver.FindElement(By.XPath("/html/body/div/div[3]/div[1]/div/div[3]/div[1]/div[1]/div/div/div[2]/div[7]/div/div[2]/div[1]/div[2]/div[2]/div/div/span/button")).SendKeys(Keys.Enter);
                                    Thread.Sleep(2000);
                                }
                                if (Convert.ToInt32(strMulti) > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_Multicount_CT" + countynameCT + township] = "Maximum";
                                    chromedriver.Quit();
                                    return "Maximum";
                                }
                            }
                            catch { }

                            try
                            {
                                IWebElement INodata = chromedriver.FindElement(By.XPath("/html/body/div/div[3]/div[1]/div/div[3]/div[1]/div[1]/div/div/div[2]/div[11]/div/div[3]"));
                                if (INodata.Text.Contains("No parcels were found"))
                                {
                                    HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                    chromedriver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }

                            gc.CreatePdf_WOP(orderNumber, "Address Search Result", chromedriver, "CT", countynameCT);
                            Thread.Sleep(4000);
                        }

                        if (countAssess == "11")
                        {

                            IWebElement iframeElement = driver.FindElement(By.XPath("/html/frameset/frame[2]"));
                            driver.SwitchTo().Frame(iframeElement);
                            Thread.Sleep(2000);
                            driver.FindElement(By.Id("SearchStreetName")).SendKeys(streetname);
                            driver.FindElement(By.Id("SearchStreetNumber")).SendKeys(streetno);
                            gc.CreatePdf_WOP(orderNumber, "Address search", driver, "CT", countynameCT);
                            driver.FindElement(By.Id("cmdGo")).SendKeys(Keys.Enter);
                            Thread.Sleep(4000);
                            gc.CreatePdf_WOP(orderNumber, "Address search Result", driver, "CT", countynameCT);

                            // Multi parcel

                            try
                            {
                                driver.SwitchTo().DefaultContent();
                                IWebElement iframe3 = driver.FindElement(By.XPath("/html/frameset/frame[3]"));
                                driver.SwitchTo().Frame(iframe3);
                                Thread.Sleep(2000);
                                string nodata = driver.FindElement(By.XPath("/html/body")).Text;
                                if (nodata.Contains("No matching records found"))
                                {
                                    HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }


                            try
                            {
                                //string strmulti = gc.Between(driver.FindElement(By.Id("dt_a_info")).Text, "of ", " entries").Trim();
                                IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='T1']/tbody"));
                                IList<IWebElement> TRmultiaddress = multiaddress.FindElements(By.TagName("tr"));
                                IList<IWebElement> TDmultiaddress;
                                if (TRmultiaddress.Count < 2)
                                {
                                    driver.FindElement(By.XPath("//*[@id='T1']/tbody/tr/td[1]/a")).SendKeys(Keys.Enter);
                                    Thread.Sleep(4000);
                                }
                                if (TRmultiaddress.Count > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_Multicount_CT" + countynameCT + township] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }
                                if ((TRmultiaddress.Count > 1 && TRmultiaddress.Count < 28))
                                {
                                    foreach (IWebElement row in TRmultiaddress)
                                    {
                                        TDmultiaddress = row.FindElements(By.TagName("td"));
                                        if (TDmultiaddress.Count != 0 && !row.Text.Contains("Address") && !row.Text.Contains("Results"))
                                        {
                                            //Address~Owner~MBLU~Property Use
                                            string Multi = TDmultiaddress[1].Text + "~" + TDmultiaddress[2].Text + "~" + TDmultiaddress[3].Text + "~" + TDmultiaddress[4].Text;
                                            gc.insert_date(orderNumber, TDmultiaddress[0].Text, 2241, Multi, 1, DateTime.Now);
                                        }
                                    }
                                    multiparceldata = "Location~Owner~Built Type~Total Value";
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2241'");

                                    gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "CT", countynameCT);
                                    HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                            }
                            catch { }

                        }
                        // Address Search

                        if (countAssess == "12")
                        {
                            // string address = "";
                            Thread.Sleep(6000);
                            driver.FindElement(By.XPath("//*[@id='divSplashScreenDialog']/table/tbody/tr[3]/td/div/div/table/tbody/tr/td")).Click();
                            Thread.Sleep(2000);
                            if (streetdir == "")
                            {
                                address = streetno + " " + streetname + " " + streettype;
                            }
                            else
                            {
                                address = streetno + " " + streetname + " " + streetdir + " " + streettype;
                            }
                            address = address.Trim();
                            driver.FindElement(By.Id("txtSearchText")).Clear();
                            driver.FindElement(By.Id("txtSearchText")).SendKeys(address);
                            gc.CreatePdf_WOP(orderNumber, "Address search", driver, "CT", countynameCT);
                            driver.FindElement(By.Id("imgLocate")).Click();
                            Thread.Sleep(4000);

                            // Multi parcel

                            try
                            {

                                string nodata = driver.FindElement(By.Id("divNoResultsLabel")).Text;
                                if (nodata.Contains("No results"))
                                {
                                    HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }

                            try
                            {
                                IWebElement multiaddress = driver.FindElement(By.Id("tblParcelSearchResults"));
                                IList<IWebElement> TRmultiaddress = multiaddress.FindElements(By.TagName("tr"));
                                IList<IWebElement> TDmultiaddress;
                                foreach (IWebElement row1 in TRmultiaddress)
                                {
                                    TDmultiaddress = row1.FindElements(By.TagName("td"));
                                    if (TRmultiaddress.Count < 2 && row1.Text.Contains(address.ToUpper()))
                                    {
                                        TDmultiaddress[1].Click();
                                        Thread.Sleep(4000);
                                    }
                                    if (TRmultiaddress.Count > 25)
                                    {
                                        HttpContext.Current.Session["multiParcel_Multicount_CT" + countynameCT + township] = "Maximum";
                                        driver.Quit();
                                        return "Maximum";
                                    }
                                    if ((TRmultiaddress.Count > 1 && TRmultiaddress.Count < 28))
                                    {
                                        foreach (IWebElement row in TRmultiaddress)
                                        {
                                            TDmultiaddress = row.FindElements(By.TagName("td"));
                                            if (TDmultiaddress.Count != 0 && !row.Text.Contains("Address") && !row.Text.Contains("Results"))
                                            {
                                                if (TDmultiaddress[1].Text == address.ToUpper())
                                                {
                                                    TDmultiaddress[1].Click();
                                                    Thread.Sleep(4000);
                                                }
                                                //Address~OwnerName
                                                string Multi = TDmultiaddress[1].Text + "~" + TDmultiaddress[2].Text;
                                                gc.insert_date(orderNumber, TDmultiaddress[0].Text, 2241, Multi, 1, DateTime.Now);
                                            }
                                        }
                                        multiparceldata = "Address~OwnerName";
                                        dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2241'");

                                        gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "CT", countynameCT);
                                        HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                        driver.Quit();
                                        return "MultiParcel";
                                    }
                                }
                            }
                            catch { }

                        }

                        if (countAssess == "15")//Address
                        {
                            string addressre = "", Showing1 = "", Ownersearch = "", Addres = "", parcelmulti = "";
                            IWebElement Tbodyclose = driver.FindElement(By.XPath("/html/body"));
                            IList<IWebElement> closetagrow = Tbodyclose.FindElements(By.TagName("button"));
                            foreach (IWebElement closetag in closetagrow)
                            {
                                string Buttonst1 = closetag.GetAttribute("innerHTML");
                                if (Buttonst1.Trim().Contains("Close"))
                                {
                                    IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                                    js.ExecuteScript("arguments[0].click();", closetag);
                                    Thread.Sleep(2000);
                                }
                            }
                            gc.CreatePdf_WOP(orderNumber, "Address Close", driver, "CT", countynameCT);
                            IWebElement footer = driver.FindElement(By.XPath("//*[@id='tippy-1']/div/div[2]/div/div/footer/ul/li[1]/a"));
                            IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                            js1.ExecuteScript("arguments[0].click();", footer);
                            Thread.Sleep(2000);
                            gc.CreatePdf_WOP(orderNumber, "Address Exit", driver, "CT", countynameCT);
                            //IWebElement TbodyExit = driver.FindElement(By.XPath("/html/body"));
                            //IList<IWebElement> Exittagrow = TbodyExit.FindElements(By.TagName("button"));
                            //foreach (IWebElement Exittag in Exittagrow)
                            //{
                            //    string ExitButtonst = Exittag.GetAttribute("innerHTML");
                            //    if (ExitButtonst.Trim().Contains("Exit"))
                            //    {
                            //        Exittag.Click();
                            //        Thread.Sleep(2000);
                            //    }
                            //}
                            if (streetdir == "")
                            {
                                address = streetno.ToUpper() + " " + streetname + " " + streettype;
                            }
                            else
                            {
                                address = streetno + " " + streetname + " " + streetdir + " " + streettype;
                            }
                            int Buttonn = 0;
                            driver.FindElement(By.Id("search-field-quicksearch-displayName")).SendKeys(address.ToUpper());
                            gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "CT", countynameCT);
                            Tbody = driver.FindElement(By.XPath("/html/body"));
                            IList<IWebElement> Tbodyrow = Tbody.FindElements(By.TagName("button"));
                            foreach (IWebElement Button in Tbodyrow)
                            {
                                string Buttonst = Button.GetAttribute("innerHTML");
                                if (Buttonst.Trim().Contains("Search"))
                                {
                                    if (Buttonn != 0)
                                    {
                                        Button.Click();
                                        Thread.Sleep(2000);
                                    }
                                    else
                                    {
                                        Buttonn++;
                                    }

                                }
                            }
                            gc.CreatePdf_WOP(orderNumber, "Address After", driver, "CT", countynameCT);
                            IList<IWebElement> Showingtr = Tbody.FindElements(By.TagName("div"));
                            foreach (IWebElement showing in Showingtr)
                            {
                                string showingdata = showing.GetAttribute("innerHTML");
                                if (showingdata.Contains("Showing 1-"))
                                {
                                    Showing1 = gc.Between(showingdata, "Showing 1-", "results. Scroll").Trim();
                                    break;
                                }
                            }
                            gc.CreatePdf_WOP(orderNumber, "Address Search After", driver, "CT", countynameCT);
                            if (Showing1 == "1")
                            {
                                IList<IWebElement> Tbodyrow1 = Tbody.FindElements(By.TagName("span"));
                                IList<IWebElement> TDspan;
                                foreach (IWebElement Button1 in Tbodyrow1)
                                {
                                    TDspan = Button1.FindElements(By.TagName("title"));
                                    string Buttonst = Button1.GetAttribute("innerHTML");
                                    try
                                    {
                                        addressre = Buttonst.Replace("Address:", "").Trim();
                                    }
                                    catch { }
                                    if (addressre.Contains(address.ToUpper()))
                                    {
                                        Button1.Click();
                                        Thread.Sleep(3000);
                                    }
                                }
                            }
                            else
                            {

                                int Max = 0;
                                int divcount = 0;
                                IList<IWebElement> Tbodyrow1 = Tbody.FindElements(By.TagName("span"));
                                //IList<IWebElement> Dividrow = Tbody.FindElements(By.TagName("div"));
                                IList<IWebElement> TDspan;
                                foreach (IWebElement Button1 in Tbodyrow1)
                                {
                                    string Title = Button1.GetAttribute("title");
                                    string Buttonst = Title.Replace("Address:", "").Trim();

                                    if (Buttonst.Contains(address.ToUpper()) || divcount != 0)
                                    {
                                        if (Title.Contains("Address:"))
                                        {
                                            Addres = Title.Replace("Address:", "").Trim();
                                            Max++;
                                            divcount++;
                                        }
                                        if (Title.Contains("Owner Name:"))
                                        {
                                            Ownersearch = Title.Replace("Owner Name:", "").Trim();
                                        }
                                        if (Title.Contains("Identifier:"))
                                        {
                                            Divspanrow = Button1;
                                            parcelmulti = Title.Replace("Identifier:", "").Trim();
                                            string Multiparcel = Addres + "~" + Ownersearch;
                                            gc.insert_date(orderNumber, parcelmulti, 2241, Multiparcel, 1, DateTime.Now);
                                            divcount = 0;
                                        }
                                    }
                                }
                                multiparceldata = "Address~Owner Name";
                                dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2241'");
                                // string Multiparcel = Addres + "~" + Ownersearch;
                                //gc.insert_date(orderNumber, parcelmulti, 2241, Addres.Remove(Addres.Length-1), 1, DateTime.Now);
                                if (Max == 1)
                                {
                                    Divspanrow.Click();
                                    Thread.Sleep(2000);
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

                        }



                    }
                    #endregion
                    #region Owner search
                    if (searchType == "ownername")
                    {
                        if (townshipcode == "15")
                        {

                            HttpContext.Current.Session["Owner_CT" + countynameCT + township] = "Yes";
                            driver.Quit();
                            return "No Owner Search";



                        }
                        if (countAssess == "titleflex")
                        {
                            searchType = "titleflex";
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
                                    if (TDmultiaddress.Count == 11 && !row.Text.Contains("Address") && !row.Text.Contains("Results"))
                                    {
                                        //Address~Owner~Account ID~PID
                                        multiparceldata = "Address~Owner~Account ID~MBLU";
                                        string Multi = TDmultiaddress[0].Text + "~" + TDmultiaddress[1].Text + "~" + TDmultiaddress[2].Text + "~" + TDmultiaddress[3].Text + "-" + TDmultiaddress[4].Text + "-" + TDmultiaddress[5].Text + "-" + TDmultiaddress[6].Text + "-" + TDmultiaddress[7].Text + "-" + TDmultiaddress[8].Text + "-" + TDmultiaddress[9].Text;
                                        gc.insert_date(orderNumber, TDmultiaddress[10].Text, 2241, Multi, 1, DateTime.Now);
                                    }
                                }
                                dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2241'");

                                gc.CreatePdf_WOP(orderNumber, "Owner Search Result", driver, "CT", countynameCT);
                                HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
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
                                            gc.insert_date(orderNumber, TDmultiaddress[4].Text, 2241, Multi, 1, DateTime.Now);
                                        }
                                    }
                                    multiparceldata = "Address~Owner~MBLU~Property Use";
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2241'");

                                    gc.CreatePdf_WOP(orderNumber, "Owner Search Result", driver, "CT", countynameCT);
                                    HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                            }
                            catch { }
                        }
                        //owner
                        if (countAssess == "3") //Bethel
                        {
                            driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table/tbody/tr/td/table/tbody/tr[2]/td[3]/table/tbody/tr/td/span/input")).SendKeys(ownername);
                            gc.CreatePdf_WOP(orderNumber, "Owner Search", driver, "CT", countynameCT);
                            driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr/td[1]/table/tbody/tr/td/span/input")).SendKeys(Keys.Enter);

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
                                            gc.insert_date(orderNumber, TDmultiaddress[2].Text, 2241, Multi, 1, DateTime.Now);
                                        }
                                    }
                                    multiparceldata = "Address~LUC~Class~Card";
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2241'");

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
                                    if (AddressTD.Count > 1 && !AddressT.Text.Contains("Quick Links"))
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
                                        gc.insert_date(orderNumber, GisID, 2241, Multiresult, 1, DateTime.Now);
                                        Max++;
                                        gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "CT", countynameCT);
                                    }

                                }
                                multiparceldata = "Address~Owner~Account Number";
                                dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2241'");

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

                        if (countAssess == "9")
                        {
                            driver.FindElement(By.Id("input_owner")).SendKeys(ownername);
                            gc.CreatePdf_WOP(orderNumber, "OwnerName search", driver, "CT", countynameCT);

                            try
                            {
                                string nodata = driver.FindElement(By.XPath("//*[@id='no_results_spaceholder']")).Text;
                                if (nodata.Contains("There are no results"))
                                {
                                    HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }

                            try
                            {
                                IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='paging_box']/span"));
                                string multi = GlobalClass.After(multiaddress.Text, "of").Trim();
                                if (Convert.ToInt32(multi) == 1)
                                {

                                }
                                else if (Convert.ToInt32(multi) < 25 && Convert.ToInt32(multi) > 1)
                                {
                                    IWebElement multiaddress1 = driver.FindElement(By.XPath("//*[@id='results_list']"));

                                    IList<IWebElement> TRmultiaddress = multiaddress1.FindElements(By.TagName("div"));
                                    IList<IWebElement> TDmultiaddress;

                                    foreach (IWebElement row in TRmultiaddress)
                                    {
                                        TDmultiaddress = row.FindElements(By.TagName("div"));
                                        if (TDmultiaddress.Count == 5)
                                        {
                                            string Multi = TDmultiaddress[0].Text + "~" + TDmultiaddress[2].Text;
                                            gc.insert_date(orderNumber, TDmultiaddress[1].Text, 2241, Multi, 1, DateTime.Now);
                                        }
                                    }
                                    multiparceldata = "Address~Owner Name";
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2241'");


                                    gc.CreatePdf_WOP(orderNumber, "Owner Search Result", driver, "CT", countynameCT);
                                    HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";

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


                        //Owner Search
                        if (countAssess == "10") //Glastonbury 
                        {
                            chromedriver.Navigate().GoToUrl(urlAssess);
                            IJavaScriptExecutor js = chromedriver as IJavaScriptExecutor;
                            Thread.Sleep(9000);
                            try
                            {
                                chromedriver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/div[2]/div/div/div/div[2]/div[8]/div/div/button")).SendKeys(Keys.Enter);
                            }
                            catch { }

                            try
                            {
                                IWebElement Accept = chromedriver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/div[2]/div/div/div/div[2]/div[8]/div/div/button"));
                                js.ExecuteScript("arguments[0].click();", Accept);
                            }
                            catch { }
                            Thread.Sleep(6000);
                            IWebElement IAddressSearch = chromedriver.FindElement(By.XPath("/html/body/div/div[3]/div[1]/div/div[2]/div[3]/div/div/div[2]/div[1]/ul"));
                            IList<IWebElement> IAddressSearchRow = IAddressSearch.FindElements(By.TagName("li"));
                            IList<IWebElement> IAddressSearchTD;
                            foreach (IWebElement addresssearch in IAddressSearchRow)
                            {
                                IAddressSearchTD = addresssearch.FindElements(By.TagName("button"));
                                if (IAddressSearchTD.Count != 0 && IAddressSearchTD.Count == 2)
                                {
                                    string strAddress = IAddressSearchTD[0].GetAttribute("title");
                                    if (strAddress.Contains("Owner Name Search") || strAddress.Contains("Search by Owner Name"))
                                    {
                                        js.ExecuteScript("arguments[0].click();", IAddressSearchTD[0]);
                                        break;
                                    }
                                }
                            }

                            Thread.Sleep(6000);
                            gc.CreatePdf_WOP_Chrome(orderNumber, "Owner Agree", chromedriver, "CT", countynameCT);
                            chromedriver.FindElement(By.XPath("/html/body/div/div[3]/div[1]/div/div[3]/div[1]/div[1]/div/div/div[2]/div[11]/div/div[3]/div/div/form/div[1]/div/div[3]/div/div/div/div[1]/input")).Click();
                            chromedriver.FindElement(By.XPath("/html/body/div/div[3]/div[1]/div/div[3]/div[1]/div[1]/div/div/div[2]/div[11]/div/div[3]/div/div/form/div[1]/div/div[3]/div/div/div/div[1]/input")).Clear();
                            chromedriver.FindElement(By.XPath("/html/body/div/div[3]/div[1]/div/div[3]/div[1]/div[1]/div/div/div[2]/div[11]/div/div[3]/div/div/form/div[1]/div/div[3]/div/div/div/div[1]/input")).SendKeys(ownername.Trim().ToUpper());
                            chromedriver.FindElement(By.XPath("/html/body/div/div[3]/div[1]/div/div[3]/div[1]/div[1]/div/div/div[2]/div[11]/div/div[3]/div/div/form/div[2]/button[1]")).SendKeys(Keys.Enter);
                            Thread.Sleep(3000);
                            gc.CreatePdf_WOP_Chrome(orderNumber, "Owner Search", chromedriver, "CT", countynameCT);
                            try
                            {
                                string strMulti = gc.Between(chromedriver.FindElement(By.XPath("/html/body/div/div[3]/div[1]/div/div[3]/div[1]/div[1]/div/div/div[2]/div[7]/div/div[2]/div[1]/div[2]/div[3]")).Text, "Total: ", ")").Trim();
                                if (Convert.ToInt32(strMulti) > 1 && Convert.ToInt32(strMulti) < 25)
                                {
                                    IWebElement singleowner = chromedriver.FindElement(By.XPath("/html/body/div/div[3]/div[1]/div/div[3]/div[1]/div[1]/div/div/div[2]/div[7]/div/div[2]/div[1]/div[2]/div[2]"));
                                    IList<IWebElement> ISOwnerRow = singleowner.FindElements(By.TagName("span"));
                                    foreach (IWebElement sowner in ISOwnerRow)
                                    {
                                        try
                                        {
                                            string strParcel = "", strOwner = "", strAddress = "";
                                            IList<IWebElement> ISParcel = sowner.FindElements(By.TagName("button"));
                                            if (ISParcel.Count != 0 && sowner.Text.Trim() != "" && sowner.Text.Contains("Parcel GIS ID"))
                                            {
                                                strParcel = ISParcel[0].Text.Replace("Parcel GIS ID", "").Trim();
                                            }
                                            IList<IWebElement> ISaddresslist = sowner.FindElements(By.TagName("span"));
                                            if (ISaddresslist.Count != 0 && sowner.Text.Trim() != "" && sowner.Text.Contains("Parcel GIS ID"))
                                            {
                                                IList<IWebElement> ISAddress = sowner.FindElements(By.TagName("p"));
                                                if (ISAddress.Count != 0 && sowner.Text.Trim() != "" && sowner.Text.Contains("Property Address"))
                                                {
                                                    strAddress = ISAddress[1].Text.Replace("Property Address :", "").Trim();
                                                }
                                                if (ISAddress.Count != 0 && sowner.Text.Trim() != "" && sowner.Text.Contains("Owner Name"))
                                                {
                                                    strOwner = ISAddress[0].Text.Replace("Owner Name:", "").Trim();
                                                }
                                            }

                                            if (strParcel != "" && strAddress != "" && strOwner != "")
                                            {
                                                string Multi = strOwner + "~" + strAddress;
                                                gc.insert_date(orderNumber, strParcel, 2241, Multi, 1, DateTime.Now);
                                            }
                                        }
                                        catch { }
                                    }

                                    gc.CreatePdf_WOP_Chrome(orderNumber, "Multi Owner Search", chromedriver, "CT", countynameCT);
                                    HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                    chromedriver.Quit();
                                    driver.Quit();
                                    return "MultiParcel";
                                }

                                if (Convert.ToInt32(strMulti) == 1)
                                {
                                    chromedriver.FindElement(By.XPath("/html/body/div/div[3]/div[1]/div/div[3]/div[1]/div[1]/div/div/div[2]/div[7]/div/div[2]/div[1]/div[2]/div[2]/div/div/span/button")).SendKeys(Keys.Enter);
                                    Thread.Sleep(2000);
                                }
                                if (Convert.ToInt32(strMulti) > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_Multicount_CT" + countynameCT + township] = "Maximum";
                                    chromedriver.Quit();
                                    driver.Quit();
                                    return "Maximum";
                                }
                            }
                            catch { }

                            try
                            {
                                IWebElement INodata = chromedriver.FindElement(By.XPath("/html/body/div/div[3]/div[1]/div/div[3]/div[1]/div[1]/div/div/div[2]/div[11]/div/div[3]"));
                                if (INodata.Text.Contains("No parcels were found"))
                                {
                                    HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                    chromedriver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }

                            gc.CreatePdf_WOP_Chrome(orderNumber, "Owner search Result", chromedriver, "CT", countynameCT);
                            Thread.Sleep(4000);
                        }
                        if (countAssess == "11")
                        {
                            IWebElement iframeElement = driver.FindElement(By.XPath("/html/frameset/frame[2]"));
                            driver.SwitchTo().Frame(iframeElement);
                            Thread.Sleep(2000);
                            driver.FindElement(By.Id("SearchOwner")).SendKeys(ownername);
                            gc.CreatePdf_WOP(orderNumber, "OwnerName search", driver, "CT", countynameCT);
                            driver.FindElement(By.Id("cmdGo")).SendKeys(Keys.Enter);
                            Thread.Sleep(4000);
                            gc.CreatePdf_WOP(orderNumber, "OwnerName search Result", driver, "CT", countynameCT);

                            // Multi parcel

                            try
                            {
                                driver.SwitchTo().DefaultContent();
                                IWebElement iframe3 = driver.FindElement(By.XPath("/html/frameset/frame[3]"));
                                driver.SwitchTo().Frame(iframe3);
                                Thread.Sleep(2000);
                                string nodata = driver.FindElement(By.XPath("/html/body")).Text;
                                if (nodata.Contains("No matching records found"))
                                {
                                    HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }


                            try
                            {
                                //string strmulti = gc.Between(driver.FindElement(By.Id("dt_a_info")).Text, "of ", " entries").Trim();
                                IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='T1']/tbody"));
                                IList<IWebElement> TRmultiaddress = multiaddress.FindElements(By.TagName("tr"));
                                IList<IWebElement> TDmultiaddress;
                                if (TRmultiaddress.Count < 2)
                                {
                                    driver.FindElement(By.XPath("//*[@id='T1']/tbody/tr/td[1]/a")).SendKeys(Keys.Enter);
                                    Thread.Sleep(4000);
                                }
                                if (TRmultiaddress.Count > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_Multicount_CT" + countynameCT + township] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }
                                if ((TRmultiaddress.Count > 1 && TRmultiaddress.Count < 28))
                                {
                                    foreach (IWebElement row in TRmultiaddress)
                                    {
                                        TDmultiaddress = row.FindElements(By.TagName("td"));
                                        if (TDmultiaddress.Count != 0 && !row.Text.Contains("Address") && !row.Text.Contains("Results"))
                                        {
                                            //Address~Owner~MBLU~Property Use
                                            string Multi = TDmultiaddress[1].Text + "~" + TDmultiaddress[2].Text + "~" + TDmultiaddress[3].Text + "~" + TDmultiaddress[4].Text;
                                            gc.insert_date(orderNumber, TDmultiaddress[0].Text, 2241, Multi, 1, DateTime.Now);
                                        }
                                    }
                                    multiparceldata = "Location~Owner~Built Type~Total Value";
                                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2241'");

                                    gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "CT", countynameCT);
                                    HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                            }
                            catch { }
                        }

                        // OwnerName Search

                        if (countAssess == "12")
                        {
                            driver.FindElement(By.XPath("//*[@id='divSplashScreenDialog']/table/tbody/tr[3]/td/div/div/table/tbody/tr/td")).Click();
                            Thread.Sleep(2000);
                            driver.FindElement(By.Id("txtSearchText")).Clear();
                            driver.FindElement(By.Id("txtSearchText")).SendKeys(ownername);
                            gc.CreatePdf_WOP(orderNumber, "OwnerName search", driver, "CT", countynameCT);
                            driver.FindElement(By.Id("imgLocate")).Click();
                            Thread.Sleep(4000);
                            gc.CreatePdf_WOP(orderNumber, "OwnerName search Result", driver, "CT", countynameCT);

                            // Multi parcel

                            try
                            {

                                string nodata = driver.FindElement(By.Id("divNoResultsLabel")).Text;
                                if (nodata.Contains("No results"))
                                {
                                    HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }

                            try
                            {
                                IWebElement multiaddress = driver.FindElement(By.Id("tblParcelSearchResults"));
                                IList<IWebElement> TRmultiaddress = multiaddress.FindElements(By.TagName("tr"));
                                IList<IWebElement> TDmultiaddress;
                                foreach (IWebElement row1 in TRmultiaddress)
                                {
                                    TDmultiaddress = row1.FindElements(By.TagName("td"));
                                    if (TRmultiaddress.Count < 2 && row1.Text.Contains(ownername.ToUpper()))
                                    {
                                        TDmultiaddress[1].Click();
                                        Thread.Sleep(4000);
                                    }
                                    if (TRmultiaddress.Count > 25)
                                    {
                                        HttpContext.Current.Session["multiParcel_Multicount_CT" + countynameCT + township] = "Maximum";
                                        driver.Quit();
                                        return "Maximum";
                                    }
                                    if ((TRmultiaddress.Count > 1 && TRmultiaddress.Count < 28))
                                    {
                                        foreach (IWebElement row in TRmultiaddress)
                                        {
                                            TDmultiaddress = row.FindElements(By.TagName("td"));
                                            if (TDmultiaddress.Count != 0 && !row.Text.Contains("Address") && row.Text.Trim() != "")
                                            {
                                                if (TDmultiaddress[2].Text == ownername.ToUpper())
                                                {
                                                    TDmultiaddress[1].Click();
                                                    Thread.Sleep(4000);
                                                }
                                                //Address~OwnerName
                                                string Multi = TDmultiaddress[1].Text + "~" + TDmultiaddress[2].Text;
                                                gc.insert_date(orderNumber, TDmultiaddress[0].Text, 2241, Multi, 1, DateTime.Now);
                                            }
                                        }
                                        multiparceldata = "Address~OwnerName";
                                        dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2241'");

                                        HttpContext.Current.Session["multiparcel_CT" + countynameCT + township] = "Yes";
                                        driver.Quit();
                                        return "MultiParcel";
                                    }
                                }
                            }
                            catch { }

                        }

                        if (countAssess == "15")//Ownername
                        {
                            int Buttonn = 0;
                            string Showing1 = "", Ownersearch = "", Addres = "", parcelmulti = "";
                            IWebElement Tbodyclose = driver.FindElement(By.XPath("/html/body"));
                            IList<IWebElement> closetagrow = Tbodyclose.FindElements(By.TagName("button"));
                            foreach (IWebElement closetag in closetagrow)
                            {
                                string Buttonst1 = closetag.GetAttribute("innerHTML");
                                if (Buttonst1.Trim().Contains("Close"))
                                {
                                    IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                                    js.ExecuteScript("arguments[0].click();", closetag);
                                    Thread.Sleep(2000);
                                }
                            }
                            gc.CreatePdf_WOP(orderNumber, "Owner Search Close", driver, "CT", countynameCT);
                            IWebElement footer = driver.FindElement(By.XPath("//*[@id='tippy-1']/div/div[2]/div/div/footer/ul/li[1]/a"));
                            IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                            js1.ExecuteScript("arguments[0].click();", footer);
                            Thread.Sleep(2000);
                            gc.CreatePdf_WOP(orderNumber, "Owner Search Exit", driver, "CT", countynameCT);
                            driver.FindElement(By.Id("search-field-quicksearch-ownerName")).SendKeys(ownername);
                            gc.CreatePdf_WOP(orderNumber, "Owner Search Before", driver, "CT", countynameCT);
                            Tbody = driver.FindElement(By.XPath("/html/body"));
                            IList<IWebElement> Tbodyrow = Tbody.FindElements(By.TagName("button"));
                            foreach (IWebElement Button in Tbodyrow)
                            {
                                string Buttonst = Button.GetAttribute("innerHTML");
                                if (Buttonst.Trim().Contains("Search"))
                                {
                                    if (Buttonn != 0)
                                    {
                                        Button.Click();
                                        Thread.Sleep(2000);
                                    }
                                    else
                                    {
                                        Buttonn++;
                                    }

                                }
                            }
                            gc.CreatePdf_WOP(orderNumber, "Owner Search After", driver, "CT", countynameCT);
                            IList<IWebElement> Showingtr = Tbody.FindElements(By.TagName("div"));
                            foreach (IWebElement showing in Showingtr)
                            {
                                string showingdata = showing.GetAttribute("innerHTML");
                                if (showingdata.Contains("Showing 1-"))
                                {
                                    Showing1 = gc.Between(showingdata, "Showing 1-", "results. Scroll").Trim();
                                    break;
                                }
                            }
                            if (Showing1 == "1")
                            {
                                IList<IWebElement> Tbodyrow1 = Tbody.FindElements(By.TagName("span"));
                                IList<IWebElement> TDspan;
                                foreach (IWebElement Button1 in Tbodyrow1)
                                {
                                    TDspan = Button1.FindElements(By.TagName("title"));
                                    string Buttonst = Button1.GetAttribute("innerHTML");
                                    if (Buttonst.Contains("Owner Name:"))
                                    {
                                        Ownersearch = Buttonst.Replace("Owner Name:", "").Trim();
                                        Button1.Click();
                                        Thread.Sleep(3000);
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                int Max = 0;
                                int divcount = 0;
                                IList<IWebElement> Tbodyrow1 = Tbody.FindElements(By.TagName("span"));
                                //IList<IWebElement> Dividrow = Tbody.FindElements(By.TagName("div"));
                                IList<IWebElement> TDspan;
                                foreach (IWebElement Button1 in Tbodyrow1)
                                {
                                    string Title = Button1.GetAttribute("title");
                                    string Buttonst = Title.Replace("Address:", "").Trim();

                                    if (Buttonst.Contains(address.ToUpper()) || divcount != 0)
                                    {
                                        if (Title.Contains("Address:"))
                                        {
                                            Addres = Title.Replace("Address:", "").Trim();
                                            Max++;
                                            divcount++;
                                        }
                                        if (Title.Contains("Owner Name:"))
                                        {
                                            Ownersearch = Title.Replace("Owner Name:", "").Trim();
                                        }
                                        if (Title.Contains("Identifier:"))
                                        {
                                            Divspanrow = Button1;
                                            parcelmulti = Title.Replace("Identifier:", "").Trim();
                                            string Multiparcel = Addres + "~" + Ownersearch;
                                            gc.insert_date(orderNumber, parcelmulti, 2241, Multiparcel, 1, DateTime.Now);
                                            divcount = 0;
                                        }
                                    }
                                }
                                multiparceldata = "Address~Owner Name";
                                dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2241'");
                                // string Multiparcel = Addres + "~" + Ownersearch;
                                //gc.insert_date(orderNumber, parcelmulti, 2241, Addres.Remove(Addres.Length-1), 1, DateTime.Now);
                                if (Max == 1)
                                {
                                    Divspanrow.Click();
                                    Thread.Sleep(2000);
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
                        if (townshipcode == "15")
                        {

                            HttpContext.Current.Session["Owner_CT" + countynameCT + township] = "Yes";
                            driver.Quit();
                            return "No Parcel Search";
                        }
                        if (countAssess == "0")//Bridgeport
                        {

                            IWebElement IParcelSelect = driver.FindElement(By.Id("MainContent_ddlSearchSource"));
                            SelectElement sParcelSelect = new SelectElement(IParcelSelect);
                            if (townshipcode == "26")
                            {
                                sParcelSelect.SelectByText("Vision Id #");
                            }
                            else
                            {
                                sParcelSelect.SelectByText("PID");
                            }
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
                            try
                            {
                                string nodata = driver.FindElement(By.XPath("//*[@id='dt_a']/tbody")).Text;
                                if (nodata.Contains("No data available"))
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

                            driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table/tbody/tr/td/table/tbody/tr[2]/td[1]/table/tbody/tr/td/span/input")).SendKeys(parcelNumber);
                            gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "CT", countynameCT);
                            driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table/tbody/tr[3]/td/table/tbody/tr/td[1]/table/tbody/tr/td/span/input")).SendKeys(Keys.Enter);
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
                                    if (AddressTD.Count > 2 && !AddressT.Text.Contains("Quick Links"))
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
                                        else if (townshipcode == "14")
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
                                        gc.insert_date(orderNumber, GisID, 2241, Multiresult, 1, DateTime.Now);
                                        Max++;
                                        gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "CT", countynameCT);
                                    }

                                }
                                multiparceldata = "Address~Owner~Account Number";
                                dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + multiparceldata + "' where Id = '2241'");

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

                        if (countAssess == "9")
                        {

                            driver.FindElement(By.Id("input_pid")).SendKeys(parcelNumber);
                            gc.CreatePdf_WOP(orderNumber, "Parcel search", driver, "CT", countynameCT);

                            try
                            {

                                string nodata = driver.FindElement(By.Id("no_results_spaceholder")).Text;
                                if (nodata.Contains("There are no results for the search you've entered"))
                                {
                                    HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }

                        }

                        if (townshipcode == "12")
                        {
                            HttpContext.Current.Session["Parcel_CT" + countynameCT + township] = "Yes";
                            driver.Quit();
                            return "No Parcel Search";
                        }

                        if (countAssess == "11")
                        {
                            IWebElement iframeElement = driver.FindElement(By.XPath("/html/frameset/frame[2]"));
                            driver.SwitchTo().Frame(iframeElement);
                            Thread.Sleep(2000);
                            driver.FindElement(By.Id("SearchParcel")).SendKeys(parcelNumber);
                            gc.CreatePdf_WOP(orderNumber, "Parcel search", driver, "CT", countynameCT);
                            driver.FindElement(By.Id("cmdGo")).SendKeys(Keys.Enter);
                            Thread.Sleep(4000);
                            gc.CreatePdf_WOP(orderNumber, "Parcel search Result", driver, "CT", countynameCT);

                            try
                            {
                                driver.SwitchTo().DefaultContent();
                                IWebElement iframe3 = driver.FindElement(By.XPath("/html/frameset/frame[3]"));
                                driver.SwitchTo().Frame(iframe3);
                                Thread.Sleep(2000);
                                string nodata = driver.FindElement(By.XPath("/html/body")).Text;
                                if (nodata.Contains("No matching records found"))
                                {
                                    HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }

                            driver.SwitchTo().DefaultContent();
                            IWebElement iframe2 = driver.FindElement(By.XPath("/html/frameset/frame[3]"));
                            driver.SwitchTo().Frame(iframe2);
                            Thread.Sleep(2000);
                            driver.FindElement(By.XPath("//*[@id='T1']/tbody/tr/td[1]/a")).SendKeys(Keys.Enter);
                            Thread.Sleep(4000);
                        }
                        // Parcel search


                        if (countAssess == "12")
                        {
                            driver.FindElement(By.XPath("//*[@id='divSplashScreenDialog']/table/tbody/tr[3]/td/div/div/table/tbody/tr/td")).Click();
                            Thread.Sleep(2000);

                            driver.FindElement(By.Id("txtSearchText")).Clear();
                            driver.FindElement(By.Id("txtSearchText")).SendKeys(parcelNumber);
                            gc.CreatePdf(orderNumber, parcelNumber, "Parcel search", driver, "CT", countynameCT);
                            driver.FindElement(By.Id("imgLocate")).Click();
                            Thread.Sleep(10000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Parcel search Result", driver, "CT", countynameCT);


                            try
                            {

                                string nodata = driver.FindElement(By.Id("divNoResultsLabel")).Text;
                                if (nodata.Contains("No results"))
                                {
                                    HttpContext.Current.Session["Zero_CT" + countynameCT + township] = "Yes";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }

                            try
                            {
                                IWebElement multiaddress = driver.FindElement(By.Id("tblParcelSearchResults"));
                                IList<IWebElement> TRmultiaddress = multiaddress.FindElements(By.TagName("tr"));
                                IList<IWebElement> TDmultiaddress;

                                foreach (IWebElement row in TRmultiaddress)
                                {
                                    TDmultiaddress = row.FindElements(By.TagName("td"));
                                    if (TDmultiaddress.Count != 0 && !row.Text.Contains("Address") && row.Text.Trim() != "")
                                    {
                                        try
                                        {
                                            if (TDmultiaddress[0].Text == parcelNumber)
                                            {
                                                try
                                                {
                                                    TDmultiaddress[0].Click();
                                                    Thread.Sleep(4000);
                                                }
                                                catch { }
                                            }
                                        }
                                        catch { }

                                    }
                                }


                            }

                            catch (Exception ex)
                            { }
                        }

                        if (countAssess == "15")//parcel
                        {
                            int Buttonn = 0;
                            string addressre = "", Showing1 = "";
                            IWebElement Tbodyclose = driver.FindElement(By.XPath("/html/body"));
                            IList<IWebElement> closetagrow = Tbodyclose.FindElements(By.TagName("button"));
                            foreach (IWebElement closetag in closetagrow)
                            {
                                string Buttonst1 = closetag.GetAttribute("innerHTML");
                                if (Buttonst1.Trim().Contains("Close"))
                                {
                                    IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                                    js.ExecuteScript("arguments[0].click();", closetag);
                                    Thread.Sleep(2000);
                                }
                            }
                            gc.CreatePdf_WOP(orderNumber, "Owner Search Close", driver, "CT", countynameCT);
                            IWebElement footer = driver.FindElement(By.XPath("//*[@id='tippy-1']/div/div[2]/div/div/footer/ul/li[1]/a"));
                            IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                            js1.ExecuteScript("arguments[0].click();", footer);
                            Thread.Sleep(2000);
                            driver.FindElement(By.Id("search-field-quicksearch-id")).SendKeys(parcelNumber);
                            Thread.Sleep(2000);
                            gc.CreatePdf_WOP(orderNumber, "Owner Search Before", driver, "CT", countynameCT);
                            Tbody = driver.FindElement(By.XPath("/html/body"));
                            IList<IWebElement> Tbodyrow = Tbody.FindElements(By.TagName("button"));
                            foreach (IWebElement Button in Tbodyrow)
                            {
                                string Buttonst = Button.GetAttribute("innerHTML");
                                if (Buttonst.Trim().Contains("Search"))
                                {
                                    if (Buttonn != 0)
                                    {
                                        Button.Click();
                                        Thread.Sleep(2000);
                                    }
                                    else
                                    {
                                        Buttonn++;
                                    }

                                }
                            }
                            gc.CreatePdf_WOP(orderNumber, "Owner Search After", driver, "CT", countynameCT);
                            string Parcelmerge = "";
                            IList<IWebElement> Tbodyrow1 = Tbody.FindElements(By.TagName("span"));
                            IList<IWebElement> TDspan;
                            foreach (IWebElement Button1 in Tbodyrow1)
                            {
                                TDspan = Button1.FindElements(By.TagName("title"));
                                string Buttonst = Button1.GetAttribute("innerHTML");
                                try
                                {
                                    Parcelmerge = Buttonst.Replace("Identifier:", "").Trim();
                                }
                                catch { }
                                if (Parcelmerge.Contains(parcelNumber.Trim()))
                                {
                                    Button1.Click();
                                    Thread.Sleep(3000);
                                }
                            }
                        }
                    }
                    #endregion



                    //Property details
                    #region Zero Assessment Link
                    if (countAssess == "0")//address
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
                            if (IBasicDetailsTD.Count != 0 && row.Text.Contains("Parcel ID"))
                            {
                                assessment_id = IBasicDetailsTD[2].Text.Replace("Parcel ID\r\n", "").Trim();
                            }
                            if (IBasicDetailsTD.Count != 0 && row.Text.Contains("UID"))
                            {
                                assessment_id = IBasicDetailsTD[2].Text.Replace("UID\r\n", "").Trim();
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
                            if (IBasicDetailsTD.Count != 0 && row.Text.Contains("Vision Id #"))
                            {
                                ParcelID = IBasicDetailsTD[6].Text.Replace("Vision Id #\r\n", "").Trim();
                            }
                            if (IBasicDetailsTD.Count != 0 && row.Text.Contains("Building Count"))
                            {
                                BuildingCount = IBasicDetailsTD[7].Text.Replace("Building Count\r\n", "").Trim();
                            }
                        }

                        if (townshipcode == "13" || townshipcode == "04" || townshipcode == "07" || townshipcode == "10" || townshipcode == "16" || townshipcode == "18" || townshipcode == "23" || townshipcode == "25" || townshipcode == "29")
                        {
                            uniqueidMap = assessment_id;

                        }
                        if (townshipcode == "24")
                        {
                            parcelNumber = assessment_id;
                        }
                        if (townshipcode == "25")
                        {
                            uniqueidMap = "R" + assessment_id;
                            assessment_id = uniqueidMap;
                        }
                        string BasicDetails = PropertyAddress + "~" + MapLot + "~" + assessment_id + "~" + Owner + "~" + Assessment + "~" + Appraisal + "~" + ParcelID + "~" + BuildingCount;
                        string property1 = "Property Address~Mblu~Account Number~Owner Name~Assessment~Appraisal~PID~Building Count";

                        dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property1 + "' where Id = '2235'");
                        gc.insert_date(orderNumber, assessment_id, 2235, BasicDetails, 1, DateTime.Now);
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
                            if (IICurrentValueAppDetailsTD.Count != 0 && !row.Text.Contains("Valuation Year"))
                            {
                                string ValueAppraisal = "Appraisal" + "~" + IICurrentValueAppDetailsTD[0].Text + "~" + IICurrentValueAppDetailsTD[1].Text + "~" + IICurrentValueAppDetailsTD[2].Text + "~" + IICurrentValueAppDetailsTD[3].Text;
                                gc.insert_date(orderNumber, assessment_id, 2236, ValueAppraisal, 1, DateTime.Now);
                                //Type~Valuation Year~Improvements~Land~Total
                            }
                        }
                        string property2 = "Type~Valuation Year~Improvements~Land~Total";
                        dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property2 + "' where Id = '2236'");
                        //Current Assessment Valuation
                        IWebElement ICurrentValueAssDetails = driver.FindElement(By.XPath("//*[@id='MainContent_grdCurrentValueAsmt']/tbody"));
                        IList<IWebElement> ICurrentValueAssDetailsRow = ICurrentValueAssDetails.FindElements(By.TagName("tr"));
                        IList<IWebElement> IICurrentValueAssDetailsTD;
                        foreach (IWebElement assessment in ICurrentValueAssDetailsRow)
                        {
                            IICurrentValueAssDetailsTD = assessment.FindElements(By.TagName("td"));
                            if (IICurrentValueAssDetailsTD.Count != 0 && !assessment.Text.Contains("Valuation Year"))
                            {
                                string ValueAssessment = "Assessment" + "~" + IICurrentValueAssDetailsTD[0].Text + "~" + IICurrentValueAssDetailsTD[1].Text + "~" + IICurrentValueAssDetailsTD[2].Text + "~" + IICurrentValueAssDetailsTD[3].Text;
                                gc.insert_date(orderNumber, assessment_id, 2236, ValueAssessment, 1, DateTime.Now);
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
                                gc.insert_date(orderNumber, assessment_id, 2237, Propertyresult, 1, DateTime.Now);
                                Propertyresult = "";
                            }
                            if (multirowTH1.Count != 0 && multirowTH1[0].Text != " ")
                            {
                                for (int i = 0; i < multirowTH1.Count; i++)
                                {

                                    Propertyhead += multirowTH1[i].Text + "~";
                                }
                                Propertyhead = Propertyhead.TrimEnd('~');
                                dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + Propertyhead + "' where Id = '2237'");

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
                        gc.insert_date(orderNumber, assessment_id, 2238, YearBuiltDetails, 1, DateTime.Now);
                        string property4 = "Year Built~Living Area~Replacement Cost~Building Percent Good~Replacement Cost Less Depreciation";
                        dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property4 + "' where Id = '2238'");

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
                            if (ILandLineDetailsTD.Count != 0 && line.Text.Contains("Size(Acres)"))
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
                        gc.insert_date(orderNumber, assessment_id, 2239, LandDetails, 1, DateTime.Now);
                        //Use Code~Description~Zone~Neighborhood~Alt Land Appr~Category~Size(Acres)~Frontage~Depth~Assessed Value~Appraised Value

                        string property5 = "Use Code~Description~Zone~Neighborhood~Alt Land Appr~Category~Size(Acres)~Frontage~Depth~Assessed Value~Appraised Value";
                        dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property5 + "' where Id = '2239'");

                        try
                        {
                            //Appraisal Valuation History
                            IWebElement IValueAppDetails = driver.FindElement(By.XPath("//*[@id='MainContent_grdHistoryValuesAppr']/tbody"));
                            IList<IWebElement> IValueAppDetailsRow = ICurrentValueAppDetails.FindElements(By.TagName("tr"));
                            IList<IWebElement> IValueAppDetailsTD;
                            foreach (IWebElement row in IValueAppDetailsRow)
                            {
                                IValueAppDetailsTD = row.FindElements(By.TagName("td"));
                                if (IValueAppDetailsTD.Count != 0 && !row.Text.Contains("Valuation Year"))
                                {
                                    string ValueAppraisal = "Appraisal" + "~" + IValueAppDetailsTD[0].Text + "~" + IValueAppDetailsTD[1].Text + "~" + IValueAppDetailsTD[2].Text + "~" + IValueAppDetailsTD[3].Text;
                                    gc.insert_date(orderNumber, assessment_id, 2240, ValueAppraisal, 1, DateTime.Now);
                                    //Type~Valuation Year~Improvements~Land~Total
                                }
                            }
                        }
                        catch { }
                        //Assessment Valuation History
                        try
                        {
                            IWebElement IValueAssDetails = driver.FindElement(By.XPath("//*[@id='MainContent_grdHistoryValuesAsmt']/tbody"));
                            IList<IWebElement> IValueAssDetailsRow = ICurrentValueAssDetails.FindElements(By.TagName("tr"));
                            IList<IWebElement> IValueAssDetailsTD;
                            foreach (IWebElement assessment in IValueAssDetailsRow)
                            {
                                IValueAssDetailsTD = assessment.FindElements(By.TagName("td"));
                                if (IValueAssDetailsTD.Count != 0 && !assessment.Text.Contains("Valuation Year"))
                                {
                                    string ValueAssessment = "Assessment" + "~" + IValueAssDetailsTD[0].Text + "~" + IValueAssDetailsTD[1].Text + "~" + IValueAssDetailsTD[2].Text + "~" + IValueAssDetailsTD[3].Text;
                                    gc.insert_date(orderNumber, assessment_id, 2240, ValueAssessment, 1, DateTime.Now);
                                    //Type~Valuation Year~Improvements~Land~Total
                                }
                            }
                        }
                        catch
                        { }
                        string property6 = "Type~Valuation Year~Improvements~Land~Total";
                        dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property6 + "' where Id = '2240'");
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
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + "Type~" + valuetitle.Remove(valuetitle.Length - 1, 1) + "' where Id = '" + 2236 + "'");
                        gc.insert_date(orderNumber, parcelNumber, 2236, "Appraised Value~" + valueAppvalues.Remove(valueAppvalues.Length - 1, 1), 1, DateTime.Now);
                        gc.insert_date(orderNumber, parcelNumber, 2236, "Assessed Value~" + valueAssvalues.Remove(valueAssvalues.Length - 1, 1), 1, DateTime.Now);
                        string YearBuild = "";
                        try
                        {

                            driver.FindElement(By.Id("MainContent_showBuildingTab")).Click();
                            driver.FindElement(By.LinkText("Building 1")).Click();
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
                        gc.insert_date(orderNumber, parcelNumber, 2235, property2, 1, DateTime.Now);
                        //Property Adddress~Property Use~Primary Use~Unique ID~Map Block Lot~Acres~490 Acres~Zone~Volume/Page~Developers Map/Lot~Census~Year Built
                        string property1 = ParcelValuesHeader + "Year Built";
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property1 + "' where Id = '" + 2235 + "'");
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
                                    gc.insert_date(orderNumber, parcelNumber, 2237, salesdetails, 1, DateTime.Now);
                                    //Owner Name~Volume~Page~Sale Date~Deed Type~Valid Sale~Sale Price
                                }
                            }
                            //
                            string property9 = "Owner Name~Volume~Page~Sale Date~Deed Type~Valid Sale~Sale Price";
                            db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property9 + "' where Id = '" + 2237 + "'");
                        }
                        catch { }

                        if (townshipcode == "06" || townshipcode == "09" || townshipcode == "11" || townshipcode == "19" || townshipcode == "22" || townshipcode == "27")

                        {
                            uniqueidMap = parcelNumber;

                        }
                        assessment_id = uniqueidMap;
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
                        if (townshipcode == "20")
                        {
                            uniqueidMap = AlternateID;
                            assessment_id = AlternateID;
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
                        gc.insert_date(orderNumber, parcelNumber, 2235, propertyDetails, 1, DateTime.Now);

                        string property9 = "Alternate ID/Map Block Lot~First Card~Second Card~Street Name~Street Number~Zoning~LUC~Acres~Owner~Mailling Address~City~State~Zip~Volume~Page~Deed Date~Year Built";
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property9 + "' where Id = '" + 2235 + "'");



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
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + ValuationTitle.Remove(ValuationTitle.Length - 1, 1) + "' where Id = '" + 2236 + "'");
                        gc.insert_date(orderNumber, parcelNumber, 2236, ValuationValue.Remove(ValuationValue.Length - 1, 1), 1, DateTime.Now);
                        //Appraised Land~Appraised Land PA490~Appraised Bldg~Appraised Total~Total Assessment 

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
                                gc.insert_date(orderNumber, parcelNumber, 2237, SaleDetails, 1, DateTime.Now);
                            }
                        }
                        string property94 = "Book~Page~Sale Date~Price~Validity~Sale Type";
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property94 + "' where Id = '" + 2237 + "'");

                    }
                    #endregion
                    #region five Assessment Link
                    if (countAssess == "5") //
                    {

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

                        string Propetyresult = Gis + "~" + parcelid + "~" + accountno + "~" + owner + "~" + location + "~" + mailingaddress;
                        string property9 = "GIS ID~Parcel ID~Account Number~Owner~Property Address~Mailing Address";
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property9 + "' where Id = '" + 2235 + "'");
                        gc.insert_date(orderNumber, parcelNumber, 2235, Propetyresult, 1, DateTime.Now);


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

                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + AssessedResult.Remove(AssessedResult.Length - 1, 1) + "' where Id = '" + 2236 + "'");
                        gc.insert_date(orderNumber, parcelNumber, 2236, AppraisedValue.Remove(AppraisedValue.Length - 1, 1), 1, DateTime.Now);
                        gc.insert_date(orderNumber, parcelNumber, 2236, AssessedValue.Remove(AssessedValue.Length - 1, 1), 1, DateTime.Now);

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
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property.Remove(property.Length - 1, 1) + "' where Id = '" + 2237 + "'");
                        gc.insert_date(orderNumber, parcelNumber, 2237, information.Remove(information.Length - 1, 1), 1, DateTime.Now);


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
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + salesresult.Remove(salesresult.Length - 1, 1) + "' where Id = '" + 2238 + "'");
                        gc.insert_date(orderNumber, parcelNumber, 2238, salesinformation.Remove(salesinformation.Length - 1, 1), 1, DateTime.Now);
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

                        if (townshipcode == "03" || townshipcode == "05")
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
                    #endregion
                    #region Nine Assessment Link
                    if (countAssess == "9")
                    {
                        List<string> listlink = new List<string>();

                        IWebElement IBillDetails = driver.FindElement(By.XPath("//*[@id='results_list']/div[1]/div[4]/div"));
                        IList<IWebElement> IBillDetailsRow = IBillDetails.FindElements(By.TagName("a"));
                        foreach (IWebElement bill in IBillDetailsRow)
                        {
                            listlink.Add(bill.GetAttribute("href"));

                        }

                        Thread.Sleep(3000);
                        if (townshipcode == "21")
                        {
                            driver.Navigate().GoToUrl(listlink[3]);
                        }
                        else
                        {
                            driver.Navigate().GoToUrl(listlink[2]);
                        }
                        Thread.Sleep(2000);

                        string parcelNumberU = "";

                        parcelNumber = driver.FindElement(By.XPath("//*[@id='divQueryResultsListMulti']/div/table/tbody/tr/td[1]")).Text;
                        gc.CreatePdf(orderNumber, parcelNumber, "property det", driver, "CT", countynameCT);
                        // Property Details
                        string propertyheader = "", propertyresult = "";

                        IWebElement Saleinfotable = driver.FindElement(By.XPath("//*[@id='divQueryResultsDetailIdentify']/div/table/tbody"));
                        IList<IWebElement> Saleinforow = Saleinfotable.FindElements(By.TagName("tr"));
                        IList<IWebElement> Saleinfoid;
                        foreach (IWebElement Saleinfo in Saleinforow)
                        {
                            Saleinfoid = Saleinfo.FindElements(By.TagName("td"));
                            if (Saleinfoid.Count != 0)
                            {
                                propertyheader += Saleinfoid[0].Text + "~";
                                propertyresult += Saleinfoid[1].Text + "~";
                            }
                            //Sale Date~Sale Price
                        }
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + propertyheader.Remove(propertyheader.Length - 1, 1) + "' where Id = '" + 2235 + "'");
                        gc.insert_date(orderNumber, parcelNumber, 2235, propertyresult.Remove(propertyresult.Length - 1, 1), 1, DateTime.Now);




                        if (townshipcode == "08")
                        {

                            uniqueidMap = parcelNumber.PadLeft(uniqueidMap.Length + 8, '0');
                            assessment_id = uniqueidMap;
                        }
                        string filename = "";
                        string filename1 = "";
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

                            driver1.Navigate().GoToUrl(listlink[1]);
                            Thread.Sleep(6000);
                            filename = latestfilename();
                            gc.AutoDownloadFile(orderNumber, parcelNumber, countynameCT, "CT", filename);
                            Thread.Sleep(2000);
                            if (townshipcode == "21")
                            {
                                Array.ForEach(Directory.GetFiles(@downloadDirectory), File.Delete);
                                driver1.Navigate().GoToUrl(listlink[2]);
                                Thread.Sleep(6000);
                                filename1 = latestfilename();
                                gc.AutoDownloadFile(orderNumber, parcelNumber, countynameCT, "CT", filename1);
                                Thread.Sleep(2000);
                            }
                            driver1.Quit();
                        }
                        catch
                        {
                            driver1.Quit();
                        }
                        if (townshipcode == "21")
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


                                string tableassess = gc.Between(pdftext, "ACCOUNT NUMBER:", "LOCATION:").Trim();


                                uniqueidMap = tableassess;
                                assessment_id = uniqueidMap;
                            }
                            catch { }
                        }

                    }
                    #endregion
                    #region ten Assessment Link
                    if (countAssess == "10")
                    {

                        //driver.FindElement(By.XPath("/html/body/div/div[3]/div[1]/div/div[3]/div[1]/div[1]/div/div/div[2]/div[7]/div/div[2]/div[2]/div[2]/div/div[2]/div[2]/div/div/div/p[4]/a")).SendKeys(Keys.Enter);
                        IWebElement IReport = chromedriver.FindElement(By.XPath("/html/body/div/div[3]/div[1]/div/div[3]/div[1]/div[1]/div/div/div[2]/div[7]/div/div[2]/div[2]/div[2]/div/div[2]/div[2]/div/div/div/p[4]/a"));
                        IJavaScriptExecutor js1 = chromedriver as IJavaScriptExecutor;
                        js1.ExecuteScript("arguments[0].click();", IReport);
                        Thread.Sleep(9000);
                        parcelNumber = chromedriver.FindElement(By.XPath("/html/body/div/div[3]/div[1]/div/div[3]/div[1]/div[1]/div/div/div[2]/div[7]/div/div[2]/div[2]/div[2]/div/div[2]/div[5]/div/div/div[1]/ul/li[2]/div/span[1]")).Text;
                        Thread.Sleep(6000);
                        chromedriver.SwitchTo().Window(chromedriver.WindowHandles.Last());
                        gc.CreatePdf_Chrome(orderNumber, parcelNumber, "Property Details", chromedriver, "CT", countynameCT);
                        try
                        {
                            gc.downloadfile(chromedriver.Url, orderNumber, parcelNumber, "Property", "CT", countynameCT);
                            Thread.Sleep(6000);
                        }
                        catch { }

                        try
                        {
                            string FilePath = gc.filePath(orderNumber, parcelNumber).Replace("\\\\", "\\") + "Property.pdf";
                            PdfReader reader;
                            string pdfData;
                            string pdftext = "", strYearBuilt = "", strAssessment = "", strValuation = "", strOwnerRecord = "";
                            try
                            {
                                reader = new PdfReader(FilePath);
                                String textFromPage = PdfTextExtractor.GetTextFromPage(reader, 1);
                                System.Diagnostics.Debug.WriteLine("" + textFromPage);

                                pdftext = textFromPage;

                                string[] YearSplit = null, strBuild = null, strLandSplit = null, AppurtenancesSplit = null, TotalSplit = null;
                                string YearBuilt = "", Buildings = "", Land = "", Appurtenances = "", Total = "";
                                string AccountNumber = gc.Between(pdftext, "Account Number", "\nOwner of Record\n").Replace(":", "").Replace("\n", "").Trim();
                                string PropertyAddress = gc.Between(pdftext, "Property Address:", "GIS ID:").Replace(":", "").Replace("\n", "").Trim();
                                string GISID = gc.Between(pdftext, "GIS ID:", "Owner:").Replace("\n", "").Replace(":", "").Trim();
                                uniqueidMap = GISID;
                                assessment_id = GISID;
                                string Owner = gc.Between(pdftext, "Owner:", "Co-Owner:").Replace("\n", "").Replace(":", "").Trim();
                                string CoOwner = gc.Between(pdftext, "Co-Owner:", "\nAddress:").Replace("\n", "").Replace(":", "").Trim();
                                string Mailingaddress = gc.Between(pdftext, "\nAddress:", "Parcel Information").Replace("\n", "").Replace(":", "").Replace("City, State ZIP", "").Trim();
                                try
                                {
                                    strYearBuilt = gc.Between(pdftext, "Number of Rooms :", "Number of Bedrooms :").Replace("\n", "").Replace(":", "").Trim();
                                    YearSplit = strYearBuilt.Split(' ');
                                    YearBuilt = YearSplit[0];
                                }
                                catch { }

                                string PropertyDetails = "Account Number~Property Address~GIS Id~Owner Name~Co Owner Name~Mailling Address~Year Built";
                                db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + PropertyDetails + "' where Id = '" + 2235 + "'");
                                string PropertyInformation = AccountNumber + "~" + PropertyAddress + "~" + GISID + "~" + Owner + "~" + CoOwner + "~" + Mailingaddress + "~" + YearBuilt;
                                gc.insert_date(orderNumber, parcelNumber, 2235, PropertyInformation, 1, DateTime.Now);

                                string Map = gc.Between(pdftext, "Map/Street/Lot", " Property ID:").Replace("\n", "").Replace(":", "").Trim();
                                string PropertyID = gc.Between(pdftext, " Property ID:", "Developer Lot ID:").Replace("\n", "").Replace(":", "").Trim();
                                string DeveloperLot = gc.Between(pdftext, "Developer Lot ID:", "Water:").Replace("\n", "").Replace(":", "").Trim();
                                string Water = gc.Between(pdftext, "Water:", "Parcel Acreage:").Replace("\n", "").Replace(":", "").Trim();
                                string Acerage = gc.Between(pdftext, "Parcel Acreage:", "Sewer:").Replace("\n", "").Replace(":", "").Trim();
                                string Sewer = gc.Between(pdftext, "Sewer:", "Zoning Code:").Replace("\n", "").Trim().Replace(":", "");
                                string Zoining = gc.Between(pdftext, "Zoning Code:", "Census:").Replace("\n", "").Replace(":", "").Trim();
                                string Census = gc.Between(pdftext, "Census:", "Valuation Summary").Replace("\n", "").Trim();

                                string AssessmentDetails = "Map/Street/Lot~Property Id~Developer Lot Id~Water~Parcel Acerage~Sewer~Zoinig Code~Census";
                                db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + AssessmentDetails + "' where Id = '" + 2236 + "'");
                                string ParcelInformation = Map + "~" + PropertyID + "~" + DeveloperLot + "~" + Water + "~" + Acerage + "~" + Sewer + "~" + Zoining + "~" + Census;
                                gc.insert_date(orderNumber, parcelNumber, 2236, ParcelInformation, 1, DateTime.Now);

                                try
                                {
                                    string Valuation = "Item~Appraised Value~Assessed Value";
                                    db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + Valuation + "' where Id = '" + 2237 + "'");
                                    string strBuildings = gc.Between(pdftext, "Appraised Value Assessed Value\nBuildings", "Land").Replace("\n", "").Trim();
                                    strBuild = strBuildings.Split(' ');
                                    Buildings = "Buildings" + "~" + strBuild[0] + "~" + strBuild[1];
                                    gc.insert_date(orderNumber, parcelNumber, 2237, Buildings, 1, DateTime.Now);
                                    //Item~Appraised Value~Assessed Value
                                }
                                catch { }
                                try
                                {
                                    string strLand = gc.Between(pdftext, "Land", "Appurtenances").Replace("\n", "").Trim();
                                    strLandSplit = strLand.Split(' ');
                                    Land = "Land" + "~" + strLandSplit[0] + "~" + strLandSplit[1];
                                    gc.insert_date(orderNumber, parcelNumber, 2237, Land, 1, DateTime.Now);
                                    //Item~Appraised Value~Assessed Value
                                }
                                catch { }
                                try
                                {
                                    string strAppurtenances = gc.Between(pdftext, "Appurtenances", "Total").Replace("\nProperty highlighted in blue", "").Replace("\n", "").Trim();
                                    AppurtenancesSplit = strAppurtenances.Split(' ');
                                    Appurtenances = "Appurtenances" + "~" + AppurtenancesSplit[0] + "~" + AppurtenancesSplit[1];
                                    gc.insert_date(orderNumber, parcelNumber, 2237, Appurtenances, 1, DateTime.Now);
                                    //Item~Appraised Value~Assessed Value
                                }
                                catch { }
                                try
                                {
                                    string strTotal = gc.Between(pdftext, "Total", "Owner of Record Deed / Page Sale Date Sale Price").Replace("\n", "").Replace("\nProperty highlighted in blue", "").Trim();
                                    TotalSplit = strTotal.Split(' ');
                                    Total = "Total" + "~" + TotalSplit[0] + "~" + TotalSplit[1];
                                    gc.insert_date(orderNumber, parcelNumber, 2237, Total, 1, DateTime.Now);
                                    //Item~Appraised Value~Assessed Value
                                }
                                catch { }
                            }
                            catch (Exception EX) { chromedriver.Quit(); }
                        }
                        catch { chromedriver.Quit(); }
                        chromedriver.Quit();                 

                    }

                    #endregion
                    #region eleven Assessment Link
                    if (countAssess == "11")
                    {

                        // Property Details
                        string propertyowner = "", mailingaddress = "", City = "", state = "", zip = "", zoning = "", OldparcelID = "";
                        driver.SwitchTo().DefaultContent();
                        IWebElement iframe3 = driver.FindElement(By.XPath("/html/frameset/frame[3]"));
                        driver.SwitchTo().Frame(iframe3);
                        Thread.Sleep(2000);

                        parcelNumber = driver.FindElement(By.XPath("/html/body/form/div/table[1]/tbody/tr/td[3]/b/font")).Text.Trim();
                        OldparcelID = driver.FindElement(By.XPath("/html/body/form/div/table[2]/tbody/tr[1]/td/b/font")).Text.Trim();
                        propertyowner = driver.FindElement(By.XPath("/html/body/form/div/table[2]/tbody/tr[3]/td/div/table/tbody/tr/td/table/tbody/tr[1]/td[2]/font/b/font")).Text.Trim();
                        mailingaddress = driver.FindElement(By.XPath("/html/body/form/div/table[2]/tbody/tr[3]/td/div/table/tbody/tr/td/table/tbody/tr[3]/td[2]/font/b/font")).Text.Trim();
                        City = driver.FindElement(By.XPath("/html/body/form/div/table[2]/tbody/tr[3]/td/div/table/tbody/tr/td/table/tbody/tr[1]/td[4]/font/b/font")).Text.Trim();
                        state = driver.FindElement(By.XPath("/html/body/form/div/table[2]/tbody/tr[3]/td/div/table/tbody/tr/td/table/tbody/tr[2]/td[4]/font/b/font")).Text.Trim();
                        zip = driver.FindElement(By.XPath("/html/body/form/div/table[2]/tbody/tr[3]/td/div/table/tbody/tr/td/table/tbody/tr[3]/td[4]/font/b/font")).Text.Trim();
                        zoning = driver.FindElement(By.XPath("/html/body/form/div/table[2]/tbody/tr[3]/td/div/table/tbody/tr/td/table/tbody/tr[4]/td[4]/font/b/font")).Text.Trim();
                        gc.CreatePdf(orderNumber, parcelNumber, "Property Details", driver, "CT", countynameCT);
                        uniqueidMap = parcelNumber.Replace("-", "");
                        assessment_id = parcelNumber;
                        // Old Parcel ID~Property Owner~Mailing Address~City~State~Zip~Zoning

                        string Propetyresult = OldparcelID + "~" + propertyowner + "~" + mailingaddress + "~" + City + "~" + state + "~" + zip + "~" + zoning;
                        string property9 = "Old Parcel ID~Property Owner~Mailing Address~City~State~Zip~Zoning";
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property9 + "' where Id = '" + 2235 + "'");
                        gc.insert_date(orderNumber, parcelNumber, 2235, Propetyresult, 1, DateTime.Now);

                        // Assessment Details

                        string AssessedBuildingvalue = "", AssessedXtraValue = "", AssessedLandValue = "", AssessedTotalValue = "";
                        AssessedBuildingvalue = driver.FindElement(By.XPath("/html/body/form/div/table[2]/tbody/tr[5]/td/div/center/table/tbody/tr[2]/td/div/table/tbody/tr/td/table/tbody/tr[2]/td[4]/font/b")).Text.Trim();
                        AssessedXtraValue = driver.FindElement(By.XPath("/html/body/form/div/table[2]/tbody/tr[5]/td/div/center/table/tbody/tr[2]/td/div/table/tbody/tr/td/table/tbody/tr[3]/td[4]/font/b")).Text.Trim();
                        AssessedLandValue = driver.FindElement(By.XPath("/html/body/form/div/table[2]/tbody/tr[5]/td/div/center/table/tbody/tr[2]/td/div/table/tbody/tr/td/table/tbody/tr[4]/td[4]/font/b")).Text.Trim();
                        AssessedTotalValue = driver.FindElement(By.XPath("/html/body/form/div/table[2]/tbody/tr[5]/td/div/center/table/tbody/tr[2]/td/div/table/tbody/tr/td/table/tbody/tr[5]/td[4]/font/b")).Text.Trim();

                        // Assessed Building value~Assessed Xtra Value~Assessed Land Value~Assessed Total Value
                        string assessmentdetails = AssessedBuildingvalue + "~" + AssessedXtraValue + "~" + AssessedLandValue + "~" + AssessedTotalValue;
                        string propertyAss = "Assessed Building value~Assessed Xtra Value~Assessed Land Value~Assessed Total Value";
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + propertyAss + "' where Id = '" + 2236 + "'");
                        gc.insert_date(orderNumber, parcelNumber, 2236, assessmentdetails, 1, DateTime.Now);

                        // sales Information
                        string SaleDate = "", SalePrice = "", LegalReference = "", Grantor = "";
                        SaleDate = driver.FindElement(By.XPath("/html/body/form/div/table[2]/tbody/tr[4]/td/div/center/table/tbody/tr[2]/td/div/table/tbody/tr/td/table/tbody/tr[1]/td[2]/font/b/font")).Text.Trim();
                        SalePrice = driver.FindElement(By.XPath("/html/body/form/div/table[2]/tbody/tr[4]/td/div/center/table/tbody/tr[2]/td/div/table/tbody/tr/td/table/tbody/tr[2]/td[2]/font/b/font")).Text.Trim();
                        LegalReference = driver.FindElement(By.XPath("/html/body/form/div/table[2]/tbody/tr[4]/td/div/center/table/tbody/tr[2]/td/div/table/tbody/tr/td/table/tbody/tr[1]/td[4]/font/b/font")).Text.Trim();
                        Grantor = driver.FindElement(By.XPath("/html/body/form/div/table[2]/tbody/tr[4]/td/div/center/table/tbody/tr[2]/td/div/table/tbody/tr/td/table/tbody/tr[2]/td[4]/font/b/font")).Text.Trim();

                        //  Sale Date~Sale Price~Legal Reference~Grantor
                        string SaleInfodetails = SaleDate + "~" + SalePrice + "~" + LegalReference + "~" + Grantor;
                        string propertysale = "Sale Date~Sale Price~Legal Reference~Grantor";
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + propertysale + "' where Id = '" + 2237 + "'");
                        gc.insert_date(orderNumber, parcelNumber, 2237, SaleInfodetails, 1, DateTime.Now);

                        //  Printable Record Card
                        string current = "";
                        current = driver.CurrentWindowHandle;
                        try
                        {
                            driver.SwitchTo().DefaultContent();
                            IWebElement iframe4 = driver.FindElement(By.XPath("/html/frameset/frame[2]"));
                            driver.SwitchTo().Frame(iframe4);
                            Thread.Sleep(2000);
                            driver.FindElement(By.LinkText("Printable Record Card")).Click();
                            Thread.Sleep(4000);
                            driver.SwitchTo().Window(driver.WindowHandles.Last());
                            gc.CreatePdf(orderNumber, parcelNumber, "Printable Record Card", driver, "CT", countynameCT);
                        }
                        catch { }

                        // Previous Assessment
                        try
                        {
                            driver.SwitchTo().Window(current);
                            Thread.Sleep(2000);
                            driver.SwitchTo().DefaultContent();
                            IWebElement iframe5 = driver.FindElement(By.XPath("/html/frameset/frame[2]"));
                            driver.SwitchTo().Frame(iframe5);
                            Thread.Sleep(2000);
                            driver.FindElement(By.LinkText("Previous Assessment")).Click();
                            Thread.Sleep(4000);
                            driver.SwitchTo().Window(driver.WindowHandles.Last());
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Previous Assessment", driver, "CT", countynameCT);
                        }
                        catch { }

                        try
                        {
                            IWebElement Assessment = driver.FindElement(By.XPath("/html/body/table"));
                            IList<IWebElement> TRAssessment = Assessment.FindElements(By.TagName("tr"));
                            IList<IWebElement> THAssessment = Assessment.FindElements(By.TagName("th"));
                            IList<IWebElement> TDAssessment;
                            foreach (IWebElement row in TRAssessment)
                            {
                                TDAssessment = row.FindElements(By.TagName("td"));
                                if (TDAssessment.Count != 0 && !row.Text.Contains("Building"))
                                {
                                    string Assessmentdetails = TDAssessment[0].Text + "~" + TDAssessment[1].Text + "~" + TDAssessment[2].Text + "~" + TDAssessment[3].Text + "~" + TDAssessment[4].Text + "~" + TDAssessment[5].Text + "~" + TDAssessment[6].Text;
                                    string propertyASD = "Year~Code~Building~Yard Items~Land Value~Category~Total";
                                    db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + propertyASD + "' where Id = '" + 2238 + "'");
                                    gc.insert_date(orderNumber, parcelNumber, 2238, Assessmentdetails, 1, DateTime.Now);
                                }
                            }
                        }
                        catch { }
                    }

                    #endregion
                    #region twelve Assessment Link
                    if (countAssess == "12")
                    {
                        
                            // Property Details
                            string Propertyowner = "", PropertyLoc = "", Co_Owner = "", OwnerAdd = "", CSZ = "", AccountNo = "";
                            string propertytxt = driver.FindElement(By.Id("tableParcelInfoWindowData")).Text;

                            parcelNumber = gc.Between(propertytxt, "PARCEL ID:", "OWNER NAME:").Trim();
                            Propertyowner = gc.Between(propertytxt, "OWNER NAME:", "PROPERTY LOCATION:").Trim();
                            PropertyLoc = gc.Between(propertytxt, "PROPERTY LOCATION:", "CO-OWNER:").Trim();
                            Co_Owner = gc.Between(propertytxt, "CO-OWNER:", "OWNER ADDRESS:").Trim();
                            OwnerAdd = gc.Between(propertytxt, "OWNER ADDRESS:", "CSZ:").Trim();
                            CSZ = gc.Between(propertytxt, "CSZ:", "ACCOUNT NUMBER:").Trim();
                            AccountNo = GlobalClass.After(propertytxt, "ACCOUNT NUMBER:").Trim();

                            if (parcelNumber.Length == 1)
                            {
                                uniqueidMap = parcelNumber.PadLeft(parcelNumber.Length + 4, '0');
                                assessment_id = uniqueidMap;
                            }
                            if (parcelNumber.Length == 2)
                            {
                                uniqueidMap = parcelNumber.PadLeft(parcelNumber.Length + 3, '0');
                                assessment_id = uniqueidMap;
                            }
                            if (parcelNumber.Length == 3)
                            {
                                uniqueidMap = parcelNumber.PadLeft(parcelNumber.Length + 2, '0');
                                assessment_id = uniqueidMap;
                            }
                            if (parcelNumber.Length == 4)
                            {
                                uniqueidMap = parcelNumber.PadLeft(parcelNumber.Length + 1, '0');
                                assessment_id = uniqueidMap;
                            }
                            if (parcelNumber.Length == 5)
                            {
                                uniqueidMap = parcelNumber;
                                assessment_id = uniqueidMap;
                            }

                            gc.CreatePdf(orderNumber, parcelNumber, "Property Details", driver, "CT", countynameCT);
                            // Propertyowner~PropertyLoc~Co_Owner~OwnerAdd~CSZ~AccountNo

                            string propertydetails = Propertyowner + "~" + PropertyLoc + "~" + Co_Owner + "~" + OwnerAdd + "~" + CSZ + "~" + AccountNo;
                            string property9 = "Propertyowner~PropertyLoc~Co_Owner~OwnerAdd~CSZ~AccountNo";
                            db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + property9 + "' where Id = '" + 2235 + "'");
                            gc.insert_date(orderNumber, parcelNumber, 2235, propertydetails, 1, DateTime.Now);

                            try
                            {
                                driver.FindElement(By.XPath("//*[@id='divAssessment']/div[2]")).Click();
                                Thread.Sleep(2000);
                            }
                            catch { }
                            // Assessment Details
                            string LandValue = "", BuildingValue = "", TotalValue = "", BuildingType = "", LandUseCode = "", TotalAcres = "", YearBuilt = "";

                            string AssessText = driver.FindElement(By.Id("tableParceltabInfoWindowData")).Text;

                            LandValue = gc.Between(AssessText, "LAND VALUE:", "BUILDING VALUE:").Trim();
                            BuildingValue = gc.Between(AssessText, "BUILDING VALUE:", "TOTAL VALUE:").Trim();
                            TotalValue = gc.Between(AssessText, "TOTAL VALUE:", "BUILDING TYPE:").Trim();
                            BuildingType = gc.Between(AssessText, "BUILDING TYPE:", "LAND USE CODE:").Trim();
                            LandUseCode = gc.Between(AssessText, "LAND USE CODE:", "TOTAL ACRES:").Trim();
                            TotalAcres = gc.Between(AssessText, "TOTAL ACRES:", "YEAR BUILT:").Trim();
                            YearBuilt = GlobalClass.After(AssessText, "YEAR BUILT:").Trim();

                            // LandValue~BuildingValue~TotalValue~BuildingType~LandUseCode~TotalAcres~YearBuilt
                            string assessmentdetails = LandValue + "~" + BuildingValue + "~" + TotalValue + "~" + BuildingType + "~" + LandUseCode + "~" + TotalAcres + "~" + YearBuilt;
                            string propertyAss = "LandValue~BuildingValue~TotalValue~BuildingType~LandUseCode~TotalAcres~YearBuilt";
                            db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + propertyAss + "' where Id = '" + 2236 + "'");
                            gc.insert_date(orderNumber, parcelNumber, 2236, assessmentdetails, 1, DateTime.Now);


                            try
                            {
                                driver.FindElement(By.XPath("//*[@id='divSales']/div[2]")).Click();
                                Thread.Sleep(2000);
                            }
                            catch { }

                            // Sale Information Details
                            string Book_or_Page = "", SalePrice = "", SaleDate = "", SaleQualified = "";
                            string SaleText = driver.FindElement(By.Id("tableParcelSalesInfoWindowData")).Text;

                            Book_or_Page = gc.Between(SaleText, "BOOK/PAGE:", "SALE PRICE:").Trim();
                            SalePrice = gc.Between(SaleText, "SALE PRICE:", "SALE DATE:").Trim();
                            SaleDate = gc.Between(SaleText, "SALE DATE:", "SALE QUALIFIED?:").Trim();
                            SaleQualified = GlobalClass.After(SaleText, "SALE QUALIFIED?:").Trim();

                            try
                            {

                                //  Book_or_Page~SalePrice~SaleDate~SaleQualified
                                string SaleInfodetails = Book_or_Page + "~" + SalePrice + "~" + SaleDate + "~" + SaleQualified;
                                string propertysale = "Book_or_Page~SalePrice~SaleDate~SaleQualified";
                                db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + propertysale + "' where Id = '" + 2237 + "'");
                                gc.insert_date(orderNumber, parcelNumber, 2237, SaleInfodetails, 1, DateTime.Now);

                            }
                            catch { }

                        }


                    
                    #endregion
                    #region fifteen Assessment Link

                    if (countAssess == "15")//Assessment
                    {
                        string PropertyHead = "", Propertyresuly = "", assessmentresult = "", reportcard = "", Parcelid = "";
                        int A = 0;
                        IWebElement IAssessment = driver.FindElement(By.XPath("/html/body"));
                        IList<IWebElement> IAssessmentRow = IAssessment.FindElements(By.TagName("table"));
                        foreach (IWebElement assess in IAssessmentRow)
                        {
                            IList<IWebElement> TRAssessment = assess.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDAssessment;
                            foreach (IWebElement row in TRAssessment)
                            {
                                TDAssessment = row.FindElements(By.TagName("td"));
                                if (row.Text.Trim() != "")
                                {
                                    if (row.Text.Contains("Property Record Card"))
                                    {
                                        IWebElement Propertycard = TDAssessment[0].FindElement(By.TagName("a"));
                                        reportcard = Propertycard.GetAttribute("href");
                                    }
                                    if (TDAssessment.Count == 2 && A > 3)
                                    {
                                        PropertyHead += TDAssessment[0].Text + "~";
                                        Propertyresuly += TDAssessment[1].Text + "~";
                                        A++;
                                    }
                                    if (TDAssessment.Count == 2 && A <= 3)
                                    {
                                        if (row.Text.Contains("ID"))
                                        {
                                            Parcelid = TDAssessment[1].Text.Trim();
                                        }
                                        //Assessmentheading = TDAssessment[0].Text + "~";
                                        assessmentresult += TDAssessment[1].Text + "~";
                                        A++;
                                    }
                                }
                            }
                        }
                        gc.CreatePdf(orderNumber, Parcelid, "Property Details", driver, "CT", countynameCT);
                        string Propertyheading1 = "Address~Id~Name~Mailing Address";
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + Propertyheading1 + "' where Id = '" + 2235 + "'");
                        gc.insert_date(orderNumber, Parcelid, 2235, assessmentresult.Remove(assessmentresult.Length - 1, 1), 1, DateTime.Now);
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + PropertyHead.Remove(PropertyHead.Length - 1) + "' where Id = '" + 2236 + "'");
                        gc.insert_date(orderNumber, Parcelid, 2236, Propertyresuly.Remove(Propertyresuly.Length - 1, 1), 1, DateTime.Now);
                        //IWebElement IPropertyreport = driver.FindElement(By.XPath("/html/body"));
                        //IList<IWebElement> IPropertyreportRow = IPropertyreport.FindElements(By.TagName("a"));
                        //foreach (IWebElement assess in IAssessmentRow)
                        //{
                        //    if (assess.Text.Trim().Contains("Property Record Card"))
                        //    {
                        //        reportcard = assess.GetAttribute("href");

                        //    }
                        //}
                        string filename = "";
                        var chromeOptions = new ChromeOptions();
                        var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];
                        chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                        chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                        chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                        chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                        var driver1 = new ChromeDriver(chromeOptions);
                        Array.ForEach(Directory.GetFiles(@downloadDirectory), File.Delete);
                        driver1.Navigate().GoToUrl(reportcard);
                        Thread.Sleep(2000);
                        filename = latestfilename();
                        gc.AutoDownloadFile(orderNumber, Parcelid, countynameCT, "CT", filename);
                        Thread.Sleep(1000);
                        try
                        {
                            string FilePath = gc.filePath(orderNumber, Parcelid) + filename;
                            PdfReader reader;
                            string pdfData;
                            string pdftext = "";

                            reader = new PdfReader(FilePath);
                            String textFromPage = PdfTextExtractor.GetTextFromPage(reader, 1);
                            System.Diagnostics.Debug.WriteLine("" + textFromPage);

                            pdftext = textFromPage;
                            string downloadparcel = gc.Between(pdftext, "Account #", "Bldg #:");
                            uniqueidMap = downloadparcel.Trim();
                            assessment_id = uniqueidMap;
                        }
                        catch { }
                        driver1.Quit();
                    }
                    #endregion
                    //Tax details
                    //Tax details
                    driver.Navigate().GoToUrl(urlTax);
                    #region Zero Tax Link
                    if (countTax == "0")//Bridgeport
                    {

                        if (townshipcode == "")
                        {
                            IWebElement ITaxSelect = driver.FindElement(By.Id("actionType"));
                            SelectElement sTaxSelect = new SelectElement(ITaxSelect);
                            sTaxSelect.SelectByText("Parcel Number");
                            driver.FindElement(By.Name("uniqueId")).SendKeys(uniqueidMap);
                            driver.FindElement(By.Id("searchbtn4")).SendKeys(Keys.Enter);
                            Thread.Sleep(3000);
                        }

                        if (townshipcode == "17" || townshipcode == "28"||townshipcode == "12" || townshipcode == "14" || townshipcode == "21" || townshipcode == "08" || townshipcode == "13" || townshipcode == "20" || townshipcode == "29" || townshipcode == "04" || townshipcode == "07" || townshipcode == "10" || townshipcode == "16" || townshipcode == "18" || townshipcode == "23" || townshipcode == "25" || townshipcode == "06" || townshipcode == "09" || townshipcode == "11" || townshipcode == "19" || townshipcode == "22" || townshipcode == "27")

                        {
                            IWebElement ITaxSelect = driver.FindElement(By.Id("actionType"));
                            SelectElement sTaxSelect = new SelectElement(ITaxSelect);
                            sTaxSelect.SelectByText("Unique ID");
                            driver.FindElement(By.XPath("//*[@id='uniqueid']/input[1]")).SendKeys(uniqueidMap);
                            driver.FindElement(By.Id("searchbtn4")).SendKeys(Keys.Enter);
                            Thread.Sleep(3000);
                        }
                        string BillNumber = "";
                        List<string> InformURL = new List<string>();
                        List<string> HistoryURL = new List<string>();
                        List<string> DownloadURL = new List<string>();

                        if (townshipcode == "17")
                        {

                            try
                            {
                                IWebElement ITaxClick1 = driver.FindElement(By.XPath("//*[@id='content']/div/div/div/form[2]/div/table/tbody"));
                                IList<IWebElement> ITaxClickRow1 = ITaxClick1.FindElements(By.TagName("tr"));
                                IList<IWebElement> ITaxClickTD1;
                                foreach (IWebElement Itaxclickcount in ITaxClickRow1)
                                {
                                    ITaxClickTD1 = Itaxclickcount.FindElements(By.TagName("td"));
                                    if (ITaxClickTD1.Count != 0)
                                    {
                                        string billnumber = GlobalClass.Before(ITaxClickTD1[0].Text, "\r\n");
                                        string[] billsplit = billnumber.Split('-');
                                        if (billsplit[1].Trim() == "01")
                                        {
                                            RE++;
                                        }
                                    }
                                }

                                IWebElement ITaxClick = driver.FindElement(By.XPath("//*[@id='content']/div/div/div/form[2]/div/table/tbody"));
                                IList<IWebElement> ITaxClickRow = ITaxClick.FindElements(By.TagName("tr"));
                                IList<IWebElement> ITaxClickTD;
                                for (int i = 0; i < ITaxClickRow.Count; i++)
                                {
                                    if (RE - 1 == i)
                                    {
                                        IList<IWebElement> ITaxClickTag;
                                        ITaxClickTD = ITaxClickRow[i].FindElements(By.TagName("td"));
                                        if (ITaxClickTD.Count != 0)
                                        {
                                            BillNumber = GlobalClass.Before(ITaxClickTD[0].Text, "\r\n");
                                        }
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
                            catch { }
                        }
                        if (townshipcode == "15" || townshipcode == "26" || townshipcode == "02")
                        {
                            string taxaddress = "";
                            IWebElement ITaxSelect = driver.FindElement(By.Id("actionType"));
                            SelectElement sTaxSelect = new SelectElement(ITaxSelect);
                            sTaxSelect.SelectByText("Property Location");
                            driver.FindElement(By.Name("propertyNumber")).SendKeys(streetno);
                            driver.FindElement(By.Name("propertyName")).SendKeys(streetname.Trim().ToUpper() + " " + streettype.Trim().ToUpper());
                            driver.FindElement(By.Id("searchbtn2")).SendKeys(Keys.Enter);
                            Thread.Sleep(3000);
                            taxaddress = streetno.Trim() + " " + streetname.Trim().ToUpper() + " " + streettype.Trim().ToUpper();
                            try
                            {
                                IWebElement ITaxClick = driver.FindElement(By.XPath("//*[@id='content']/div/div/div/form[2]/div/table/tbody"));
                                IList<IWebElement> ITaxClickRow = ITaxClick.FindElements(By.TagName("tr"));
                                IList<IWebElement> ITaxClickTD;
                                ITaxClickRow.Reverse();
                                for (int i = 0; i < ITaxClickRow.Count; i++)
                                {
                                    if (ITaxClickRow.Count() - 1 == i)
                                    {
                                        IList<IWebElement> ITaxClickTag;
                                        ITaxClickTD = ITaxClickRow[i].FindElements(By.TagName("td"));
                                        string Addressmerg = GlobalClass.Before(ITaxClickTD[2].Text, "\r\n").Trim();
                                        string Addressmerg1 = Regex.Replace(Addressmerg, @"\s+", " ");
                                        string taxaddress1 = Regex.Replace(taxaddress, @"\s+", " ");
                                        if (Addressmerg1.Trim().Contains(taxaddress1.Trim()))
                                        {
                                            if (ITaxClickTD.Count != 0)
                                            {
                                                BillNumber = GlobalClass.Before(ITaxClickTD[0].Text, "\r\n");
                                            }
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
                            catch { }
                        }
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
                                    gc.insert_date(orderNumber, assessment_id, 2242, BillDetails, 1, DateTime.Now);
                                    //Bill~Name/Address~Property/Vehicle~Total Tax~Paid~Outstanding
                                }
                                catch { }
                            }
                        }
                        string tax1 = "Bill~Name/Address~Property/Vehicle~Total Tax~Paid~Outstanding";
                        dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + tax1 + "' where Id = '2242'");
                        if (townshipcode != "15" || townshipcode != "26" || townshipcode != "17")
                        {
                            try
                            {
                                IWebElement ITaxClick = driver.FindElement(By.XPath("//*[@id='content']/div/div/div/form[2]/div/table/tbody"));
                                IList<IWebElement> ITaxClickRow = ITaxClick.FindElements(By.TagName("tr"));
                                IList<IWebElement> ITaxClickTD;
                                for (int i = 0; i < ITaxClickRow.Count; i++)
                                {
                                    if (ITaxClickRow.Count() - 1 == i)
                                    {
                                        IList<IWebElement> ITaxClickTag;
                                        ITaxClickTD = ITaxClickRow[i].FindElements(By.TagName("td"));
                                        if (ITaxClickTD.Count != 0)
                                        {
                                            BillNumber = GlobalClass.Before(ITaxClickTD[0].Text, "\r\n");
                                        }
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
                            catch { }
                        }
                        foreach (string information in InformURL)
                        {
                            driver.Navigate().GoToUrl(information);
                            try
                            {
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
                                if (townshipcode == "15")
                                {
                                    assessment_id = UniqueID;
                                }

                                gc.CreatePdf(orderNumber, assessment_id, "Tax Information Result", driver, "CT", countynameCT);

                                string TaxInformation = TaxBill + "~" + GrossAssessment + "~" + UniqueID + "~" + Exemptions + "~" + District + "~" + NetAssessment + "~" + Name + "~" + TownMillRate + "~" + CareOf + "~" + PropertyLocation + "~" + MBL + "~" + TownBenefit + "~" + VolumePage + "~" + ElderlyBenefit;
                                gc.insert_date(orderNumber, assessment_id, 2243, TaxInformation, 1, DateTime.Now);
                                //Bill~Gross Assessment~Unique ID~Exemptions~District~Net Assessment~Name~Town Mill Rate~Care Of~Property Location~MBL~Town Benefit~Volume & Page~Elderly Benefit (C)

                                string tax2 = "Bill~Gross Assessment~Unique ID~Exemptions~District~Net Assessment~Name~Town Mill Rate~Care Of~Property Location~MBL~Town Benefit~Volume & Page~Elderly Benefit (C)";
                                dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + tax2 + "' where Id = '2243'");


                                string Propertyhead = "";
                                string Propertyresult = "";
                                int counttaxbill = 0;
                                IWebElement multitableElement1 = driver.FindElement(By.XPath("//*[@id='content']/div/div/div/div[1]/table[2]/tbody/tr/td[1]/table/tbody"));
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
                                            dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + Propertyhead + "' where Id = '2244'");

                                        }
                                        else
                                        {
                                            for (int i = 0; i < multirowTD1.Count; i++)
                                            {
                                                Propertyresult += multirowTD1[i].Text + "~";
                                            }
                                            Propertyresult = Propertyresult.TrimEnd('~');
                                            gc.insert_date(orderNumber, assessment_id, 2244, Propertyresult, 1, DateTime.Now);
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
                                        gc.insert_date(orderNumber, assessment_id, 2244, Propertyresult, 1, DateTime.Now);
                                        Propertyresult = "";

                                    }
                                }
                                //Tax Payment Details     

                                IWebElement IPaymentDetails;
                                try
                                {
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
                                        string PaymentDetails = IPaymentDetailsTD[0].Text + "~" + IPaymentDetailsTD[1].Text + "~" + IPaymentDetailsTD[2].Text + "~" + IPaymentDetailsTD[3].Text + "~" + IPaymentDetailsTD[4].Text + "~" + IPaymentDetailsTD[5].Text + "~" + "";
                                        gc.insert_date(orderNumber, assessment_id, 2245, PaymentDetails, 1, DateTime.Now);
                                        //Pay Date~Type~Tax/Principal~Interest~Lien~Fee~Total
                                    }
                                }
                                string tax3 = "Pay Date~Type~Tax/Principal~Interest~Lien~Fee~Total";
                                dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + tax3 + "' where Id = '2245'");

                                try
                                {
                                    string strTotalPayment = GlobalClass.After(driver.FindElement(By.XPath("//*[@id='content']/div/div/div/center/div[1]")).Text, ":").Trim();
                                    string PaymentDetails = "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + strTotalPayment;

                                    gc.insert_date(orderNumber, assessment_id, 2245, PaymentDetails, 1, DateTime.Now);
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
                                gc.insert_date(orderNumber, assessment_id, 2246, DueDetails, 1, DateTime.Now);
                                //Tax/Princ/Bint Due~Interest Due~Lien Due~Fee Due~Total Due
                            }
                            catch { }
                            string tax5 = "Tax/Princ/Bint Due~Interest Due~Lien Due~Fee Due~Total Due";
                            dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + tax5 + "' where Id = '2246'");

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
                                        gc.insert_date(orderNumber, assessment_id, 2247, BillHistoryDetails, 1, DateTime.Now);
                                        //Bill~Type~Paid Date~Tax~Interest~Lien~Fee~Total
                                    }
                                }
                            }
                            catch { }
                            string tax6 = "Bill~Type~Paid Date~Tax~Interest~Lien~Fee~Total";
                            dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + tax6 + "' where Id = '2247'");

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
                            IWebElement ITaxDownSelect = driver1.FindElement(By.Id("actionType"));
                            SelectElement sTaxDownSelect = new SelectElement(ITaxDownSelect);
                            sTaxDownSelect.SelectByText("Unique ID");
                            driver1.FindElement(By.XPath("//*[@id='uniqueid']/input[1]")).SendKeys(uniqueidMap);
                            driver1.FindElement(By.Id("searchbtn4")).SendKeys(Keys.Enter);
                            Thread.Sleep(3000);
                            if (townshipcode == "17")
                            {
                                try
                                {
                                    IWebElement ITaxDownloadClick = driver1.FindElement(By.XPath("//*[@id='content']/div/div/div/form[2]/div/table/tbody"));
                                    IList<IWebElement> ITaxDownloadClickRow = ITaxDownloadClick.FindElements(By.TagName("tr"));
                                    IList<IWebElement> ITaxDownloadClickTD;
                                    for (int i = 0; i < ITaxDownloadClickRow.Count; i++)
                                    {
                                        if (RE - 1 == i)
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
                            if (townshipcode != "17")
                            {
                                try
                                {
                                    IWebElement ITaxDownloadClick = driver1.FindElement(By.XPath("//*[@id='content']/div/div/div/form[2]/div/table/tbody"));
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

                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='resultsTable']/tbody/tr[2]/td[8]")).Click();
                        }
                        catch { }
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='resultsTable']/tbody/tr[3]/td[8]")).Click();
                        }
                        catch { }
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Result", driver, "CT", countynameCT);
                        //Current Tax Bill Information  Details
                        string BillDate = "", List1 = "", Year = "", Description = "", Type = "", FirstDueDate = "", SecondDueDate = "", FirstDueAmoungt = "", SecondDueAmount = "", TotalDueAmount = "", TotalPaid = "";
                        try
                        {
                            BillDate = gc.Between(driver.FindElement(By.XPath("//*[@id='blockName']/h4")).Text, "as of ", ":");
                        }
                        catch { }
                        IWebElement currenttaxinfo1 = driver.FindElement(By.XPath("//*[@id='blockName']/table/tbody"));
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
                        gc.insert_date(orderNumber, parcelNumber, 2242, BillHistoryDetails, 1, DateTime.Now);
                        //BillDate~List~Year~Description~Type~First Installment Due Date~First Installment Due Amount~Second Installment Due Date~Second Installment Due Amount~Total Installment Amount~Total Paid
                        string tax1 = "BillDate~List~Year~Description~Type~First Installment Due Date~First Installment Due Amount~Second Installment Due Date~Second Installment Due Amount~Total Installment Amount~Total Paid";
                        dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + tax1 + "' where Id = '2242'");

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
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + title2.Remove(title2.Length - 1, 1) + "' where Id = '" + 2243 + "'");
                        gc.insert_date(orderNumber, parcelNumber, 2243, value2.Remove(value2.Length - 1, 1), 1, DateTime.Now);

                        string List = "", Principal = "", Interest = "", Lien = "", Penalty = "", Total = "", DatePaid = "";
                        //Payment History Details
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
                                gc.insert_date(orderNumber, parcelNumber, 2244, PaymentHistorydetails, 1, DateTime.Now);
                                //List~Principal~Interest~Lien~Penalty~Total
                                string tax23 = "List~Principal~Interest~Lien~Penalty~Total";
                                dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + tax1 + "' where Id = '2244'");

                            }
                        }
                    }
                    #endregion


                    string taxauthority = "Tax Authority";
                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + taxauthority + "' where Id = '2248'");
                    gc.insert_date(orderNumber, assessment_id, 2248, taxCollectorlink, 1, DateTime.Now);


                    chromedriver.Quit();
                    driver.Quit();
                    gc.mergpdf(orderNumber, "CT", countynameCT);
                    return "Data Inserted Successfully";
                }
                catch (Exception ex)
                {
                    driver.Quit();
                    chromedriver.Quit();
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