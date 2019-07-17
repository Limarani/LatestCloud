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
using System.Collections.ObjectModel;

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_FLStLucie
    {
        string Outparcelno = "", outputPath = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        //txtstreetno.Text.Trim(),txtdirection.Text.Trim(),txtstreetname.Text.Trim(), txtstreettype.Text.Trim(),txtunitnumber.Text.Trim(),txtparcelno.Text.Trim(), txtownername.Text.Trim(), searchType, txtorderno.Text.Trim(), dierctSearch
        public string FTP_FLSTLucie(string streetno, string stdirec, string sname, string sttype, string unitno, string parcelNumber, string ownername, string searchType, string orderNumber, string directparcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string outparcelno = "", accountno = "", siteaddr = "", legal_desc = "", year_built = "", propuse = "", par1 = "", par2 = "", multiAddOwn = "", currentHandle = "", strTaxbillPreviousYear = "", strTaxbillPresentYear = "";
            string Building_Value = "", LandValue = "", JustValue = "", AgriculturalCredit = "", Cap = "", AssessedValue = "", Exemption = "", TaxableValue = "";
            string strBill = "-", strBalance = "-", strBillDate = "-", strBillPaid = "-";
            string taxowner = "", tax_addr = "", accno = "", alterkey = "", millagecode = "", milagerate = "", ctax_year = "", cpaiddate = "", cpaidamount = "", creceipt = "", combinedtaxamount = "", ceffdate = "", grosstax = "", ifpaidby = "", pleasepay = "";
            List<string> strissuecertificate = new List<string>();
            List<string> taxhistorylink = new List<string>();
            List<string> taxhistorylinkinst = new List<string>();
            List<string> downloadlink = new List<string>();
            List<string> lmultiparcel = new List<string>();
            List<string> sameparcel = new List<string>();
            var driverService = PhantomJSDriverService.CreateDefaultService();
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {

                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    if (searchType == "titleflex")
                    {
                        string address = streetno + " " + sname + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "FL", "Saint Lucie");
                  
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_FLStLucie"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("https://www.paslc.org/property-search/real-estate/site-address");
                        Thread.Sleep(5000);
                        IWebElement add = driver.FindElement(By.XPath("/html/body/app-root/app-real-estate/app-site-address/div/div[2]/fieldset/form/div[1]"));
                        IList<IWebElement> MultiOwnerRow = add.FindElements(By.TagName("input"));
                        foreach (IWebElement row1 in MultiOwnerRow)
                        {
                            row1.SendKeys(streetno);
                        }
                        driver.FindElement(By.XPath("//*[@id='streetNameId']")).SendKeys(sname);
                        gc.CreatePdf_WOP(orderNumber, "InputPassed Address Search", driver, "FL", "Saint Lucie");
                        driver.FindElement(By.XPath("//*[@id='streetNameId']")).SendKeys(Keys.Tab);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search Results", driver, "FL", "Saint Lucie");
                        IWebElement multiaddrtableElement = driver.FindElement(By.XPath("/html/body/app-root/app-real-estate/app-site-address/div/app-grid-result/div[2]/kendo-grid/div/kendo-grid-list/div/div[1]/table/tbody"));
                        IList<IWebElement> multiaddrtableRow = multiaddrtableElement.FindElements(By.TagName("tr"));
                        int multiaddrtableRowcount = multiaddrtableRow.Count;
                        IList<IWebElement> multiaddrrowTD;
                        int o = 0;
                        if (multiaddrtableRowcount <= 2)
                        {
                            if (multiaddrtableRowcount == 1)
                            {
                                currentHandle = driver.CurrentWindowHandle;
                                IWebElement element = driver.FindElement(By.XPath("/html/body/app-root/app-real-estate/app-site-address/div/app-grid-result/div[2]/kendo-grid/div/kendo-grid-list/div/div[1]/table/tbody/tr[1]/td[1]/button[1]"));
                                PopupWindowFinder finder = new PopupWindowFinder(driver);
                                string popupWindowHandle = finder.Click(element);
                                Thread.Sleep(5000);
                                driver.SwitchTo().Window(popupWindowHandle);
                            }
                            else
                            {
                                foreach (IWebElement row in multiaddrtableRow)
                                {
                                    multiaddrrowTD = row.FindElements(By.TagName("td"));
                                    if (o == 0)
                                    {
                                        par1 = multiaddrrowTD[1].Text;
                                    }
                                    if (o == 1)
                                    {
                                        par2 = multiaddrrowTD[1].Text;
                                    }
                                    o++;
                                }

                                if (par1 == par2)
                                {
                                    // driver.FindElement(By.XPath("/html/body/app-root/app-real-estate/app-site-address/div/app-grid-result/div[2]/kendo-grid/div/kendo-grid-list/div/div[1]/table/tbody/tr[1]/td[1]/button[1]")).SendKeys(Keys.Enter);
                                    currentHandle = driver.CurrentWindowHandle;
                                    IWebElement element = driver.FindElement(By.XPath("/html/body/app-root/app-real-estate/app-site-address/div/app-grid-result/div[2]/kendo-grid/div/kendo-grid-list/div/div[1]/table/tbody/tr[1]/td[1]/button[1]"));
                                    PopupWindowFinder finder = new PopupWindowFinder(driver);
                                    string popupWindowHandle = finder.Click(element);
                                    Thread.Sleep(5000);
                                    driver.SwitchTo().Window(popupWindowHandle);
                                    HttpContext.Current.Session["Popup_Removed"] = "Yes";
                                }
                            }
                        }
                        else
                        {
                            // multiparcel(orderNumber);
                            var item = driver.FindElement(By.XPath("/html/body/app-root/app-real-estate/app-site-address/div/app-grid-result/div[2]/kendo-grid/kendo-pager/kendo-pager-page-sizes/select"));
                            var selectElement = new SelectElement(item);
                            selectElement.SelectByText("50");
                            IWebElement multiaddrtableElementa = driver.FindElement(By.XPath("/html/body/app-root/app-real-estate/app-site-address/div/app-grid-result/div[2]/kendo-grid/div/kendo-grid-list/div/div[1]/table/tbody"));
                            IList<IWebElement> multiaddrtableRowa = multiaddrtableElementa.FindElements(By.TagName("tr"));
                            int multiaddrtableRowcounta = multiaddrtableRowa.Count;
                            IList<IWebElement> multiaddrrowTDa;
                            int y = 0;
                            if (multiaddrtableRowcounta > 2)
                            {
                                foreach (IWebElement row in multiaddrtableRowa)
                                {
                                    if (y <= 25)
                                    {
                                        multiaddrrowTDa = row.FindElements(By.TagName("td"));
                                        string multi = multiaddrrowTDa[2].Text + "~" + multiaddrrowTDa[3].Text;
                                        gc.insert_date(orderNumber, multiaddrrowTDa[1].Text, 376, multi, 1, DateTime.Now);
                                    }

                                    y++;
                                }
                                driver.Quit();
                                HttpContext.Current.Session["multiparcel_FLStLoucie"] = "Yes";
                                return "MultiParcel";
                            }

                        }

                    }
                    if (searchType == "parcel")
                    {
                        if ((HttpContext.Current.Session["titleparcel"] != null && HttpContext.Current.Session["titleparcel"].ToString() != ""))
                        {
                            parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();

                        }
                        driver.Navigate().GoToUrl("https://www.paslc.org/property-search/real-estate/parcel");
                        parcelNumber = parcelNumber.Replace("-", "");
                        IWebElement par = driver.FindElement(By.XPath("//*[@id='parcelFormId']"));
                        IList<IWebElement> MultiOwnerRow = par.FindElements(By.TagName("input"));
                        foreach (IWebElement row1 in MultiOwnerRow)
                        {
                            row1.SendKeys(parcelNumber);
                        }
                        // driver.FindElement(By.XPath("//*[@id='parcelFormId']")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "InputPassed Parcel Search", driver, "FL", "Saint Lucie");
                        // driver.FindElement(By.XPath("//*[@id='parcelFormId']")).SendKeys(Keys.Tab);
                        Thread.Sleep(2000);

                        IWebElement multiaddrtableElement = driver.FindElement(By.XPath("/html/body/app-root/app-real-estate/app-parcel/div/app-grid-result/div[2]/kendo-grid/div/kendo-grid-list/div/div[1]/table/tbody"));
                        IList<IWebElement> multiaddrtableRow = multiaddrtableElement.FindElements(By.TagName("tr"));
                        int multiaddrtableRowcount = multiaddrtableRow.Count;
                        IList<IWebElement> multiaddrrowTD;
                        int o = 0;
                        if (multiaddrtableRowcount <= 2)
                        {
                            if (multiaddrtableRowcount == 1)
                            {
                                currentHandle = driver.CurrentWindowHandle;
                                IWebElement element = driver.FindElement(By.XPath("/html/body/app-root/app-real-estate/app-parcel/div/app-grid-result/div[2]/kendo-grid/div/kendo-grid-list/div/div[1]/table/tbody/tr[1]/td[1]/button[1]"));
                                PopupWindowFinder finder = new PopupWindowFinder(driver);
                                string popupWindowHandle = finder.Click(element);
                                Thread.Sleep(5000);
                                driver.SwitchTo().Window(popupWindowHandle);
                                HttpContext.Current.Session["Popup_Removed"] = "Yes";
                            }
                            else
                            {
                                foreach (IWebElement row in multiaddrtableRow)
                                {
                                    multiaddrrowTD = row.FindElements(By.TagName("td"));
                                    if (o == 0)
                                    {
                                        par1 = multiaddrrowTD[1].Text;
                                    }
                                    if (o == 1)
                                    {
                                        par2 = multiaddrrowTD[1].Text;
                                    }
                                    o++;
                                }

                                if (par1 == par2)
                                {
                                    //  driver.FindElement(By.XPath("/html/body/app-root/app-real-estate/app-site-address/div/app-grid-result/div[2]/kendo-grid/div/kendo-grid-list/div/div[1]/table/tbody/tr[1]/td[1]/button[1]")).SendKeys(Keys.Enter);
                                    currentHandle = driver.CurrentWindowHandle;
                                    IWebElement element = driver.FindElement(By.XPath("/html/body/app-root/app-real-estate/app-parcel/div/app-grid-result/div[2]/kendo-grid/div/kendo-grid-list/div/div[1]/table/tbody/tr[1]/td[1]/button[1]"));
                                    PopupWindowFinder finder = new PopupWindowFinder(driver);
                                    string popupWindowHandle = finder.Click(element);
                                    Thread.Sleep(5000);
                                    driver.SwitchTo().Window(popupWindowHandle);
                                    HttpContext.Current.Session["Popup_Removed"] = "Yes";
                                }
                            }
                        }
                        else
                        {
                            //multiparcel(orderNumber);
                            var item = driver.FindElement(By.XPath("/html/body/app-root/app-real-estate/app-parcel/div/app-grid-result/div[2]/kendo-grid/kendo-pager/kendo-pager-page-sizes/select"));
                            var selectElement = new SelectElement(item);
                            selectElement.SelectByText("50");
                            IWebElement multiaddrtableElementa = driver.FindElement(By.XPath("/html/body/app-root/app-real-estate/app-parcel/div/app-grid-result/div[2]/kendo-grid/div/kendo-grid-list/div/div[1]/table/tbody"));
                            IList<IWebElement> multiaddrtableRowa = multiaddrtableElementa.FindElements(By.TagName("tr"));
                            int multiaddrtableRowcounta = multiaddrtableRowa.Count;
                            IList<IWebElement> multiaddrrowTDa;
                            int y = 0;
                            if (multiaddrtableRowcounta > 2)
                            {
                                foreach (IWebElement row in multiaddrtableRowa)
                                {
                                    if (y <= 25)
                                    {
                                        multiaddrrowTDa = row.FindElements(By.TagName("td"));
                                        string multi = multiaddrrowTDa[2].Text + "~" + multiaddrrowTDa[3].Text;
                                        gc.insert_date(orderNumber, multiaddrrowTDa[1].Text, 376, multi, 1, DateTime.Now);
                                        sameparcel.Add(multiaddrrowTDa[1].Text);
                                    }
                                    y++;
                                }
                                List<string> same = new List<string>();
                                same = sameparcel.Distinct().ToList();
                                if (same.Count != 0 && same.Count == 1)
                                {
                                    //  driver.FindElement(By.XPath("/html/body/app-root/app-real-estate/app-site-address/div/app-grid-result/div[2]/kendo-grid/div/kendo-grid-list/div/div[1]/table/tbody/tr[1]/td[1]/button[1]")).SendKeys(Keys.Enter);
                                    currentHandle = driver.CurrentWindowHandle;
                                    IWebElement element = driver.FindElement(By.XPath("/html/body/app-root/app-real-estate/app-parcel/div/app-grid-result/div[2]/kendo-grid/div/kendo-grid-list/div/div[1]/table/tbody/tr[1]/td[1]/button[1]"));
                                    PopupWindowFinder finder = new PopupWindowFinder(driver);
                                    string popupWindowHandle = finder.Click(element);
                                    Thread.Sleep(5000);
                                    driver.SwitchTo().Window(popupWindowHandle);
                                    HttpContext.Current.Session["Popup_Removed"] = "Yes";
                                }
                            }
                            // driver.Quit();
                            // HttpContext.Current.Session["multiparcel_FLStLoucie"] = "Yes";
                            //return "MultiParcel";
                        }


                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Results", driver, "FL", "Saint Lucie");
                    }
                    if (searchType == "ownername")
                    {
                        driver.Navigate().GoToUrl("https://www.paslc.org/property-search/real-estate/owner");
                        string[] names = ownername.Split(' ');
                        int count = names.Count();
                        if (count == 1)
                        {
                            driver.FindElement(By.XPath("//*[@id='lastNameId']")).SendKeys(names[0]);
                        }
                        else if (count == 2)
                        {
                            driver.FindElement(By.XPath("//*[@id='lastNameId']")).SendKeys(names[0]);
                            driver.FindElement(By.XPath("//*[@id='firstNameId']")).SendKeys(names[1]);
                        }
                        gc.CreatePdf_WOP(orderNumber, "InputPassed Ownername Search", driver, "FL", "Saint Lucie");
                        driver.FindElement(By.XPath("//*[@id='firstNameId']")).SendKeys(Keys.Tab);
                        Thread.Sleep(5000);

                        IWebElement multiaddrtableElement = driver.FindElement(By.XPath("/html/body/app-root/app-real-estate/app-owner-name/div/app-grid-result/div[2]/kendo-grid/div/kendo-grid-list/div/div[1]/table/tbody"));
                        IList<IWebElement> multiaddrtableRow = multiaddrtableElement.FindElements(By.TagName("tr"));
                        int multiaddrtableRowcount = multiaddrtableRow.Count;
                        IList<IWebElement> multiaddrrowTD;
                        int o = 0;
                        if (multiaddrtableRowcount <= 2)
                        {
                            if (multiaddrtableRowcount == 1)
                            {
                                currentHandle = driver.CurrentWindowHandle;
                                IWebElement element = driver.FindElement(By.XPath("/html/body/app-root/app-real-estate/app-owner-name/div/app-grid-result/div[2]/kendo-grid/div/kendo-grid-list/div/div[1]/table/tbody/tr[1]/td[1]/button[1]"));
                                PopupWindowFinder finder = new PopupWindowFinder(driver);
                                string popupWindowHandle = finder.Click(element);
                                Thread.Sleep(5000);
                                driver.SwitchTo().Window(popupWindowHandle);
                                HttpContext.Current.Session["Popup_Removed"] = "Yes";
                            }
                            else
                            {
                                foreach (IWebElement row in multiaddrtableRow)
                                {
                                    multiaddrrowTD = row.FindElements(By.TagName("td"));
                                    if (o == 0)
                                    {
                                        par1 = multiaddrrowTD[1].Text;
                                    }
                                    if (o == 1)
                                    {
                                        par2 = multiaddrrowTD[1].Text;
                                    }
                                    o++;
                                }

                                if (par1 == par2)
                                {
                                    //driver.FindElement(By.XPath("/html/body/app-root/app-real-estate/app-site-address/div/app-grid-result/div[2]/kendo-grid/div/kendo-grid-list/div/div[1]/table/tbody/tr[1]/td[1]/button[1]")).SendKeys(Keys.Enter);
                                    currentHandle = driver.CurrentWindowHandle;
                                    IWebElement element = driver.FindElement(By.XPath("/html/body/app-root/app-real-estate/app-owner-name/div/app-grid-result/div[2]/kendo-grid/div/kendo-grid-list/div/div[1]/table/tbody/tr[1]/td[1]/button[1]"));
                                    PopupWindowFinder finder = new PopupWindowFinder(driver);
                                    string popupWindowHandle = finder.Click(element);
                                    Thread.Sleep(5000);
                                    driver.SwitchTo().Window(popupWindowHandle);
                                    HttpContext.Current.Session["Popup_Removed"] = "Yes";
                                }
                            }
                        }
                        else
                        {
                            // multiparcel(orderNumber);
                            IWebElement item = driver.FindElement(By.XPath("/html/body/app-root/app-real-estate/app-owner-name/div/app-grid-result/div[2]/kendo-grid/kendo-pager/kendo-pager-page-sizes/select"));
                            SelectElement selectElement = new SelectElement(item);
                            selectElement.SelectByText("50");
                            Thread.Sleep(3000);
                            IWebElement multiaddrtableElementa = driver.FindElement(By.XPath("/html/body/app-root/app-real-estate/app-owner-name/div/app-grid-result/div[2]/kendo-grid/div/kendo-grid-list/div/div[1]/table/tbody"));
                            IList<IWebElement> multiaddrtableRowa = multiaddrtableElementa.FindElements(By.TagName("tr"));
                            int multiaddrtableRowcounta = multiaddrtableRowa.Count;
                            IList<IWebElement> multiaddrrowTDa;
                            int y = 0;
                            if (multiaddrtableRowcounta > 2)
                            {
                                foreach (IWebElement row in multiaddrtableRowa)
                                {
                                    if (y <= 25)
                                    {
                                        multiaddrrowTDa = row.FindElements(By.TagName("td"));
                                        string multi = multiaddrrowTDa[2].Text + "~" + multiaddrrowTDa[3].Text;
                                        gc.insert_date(orderNumber, multiaddrrowTDa[1].Text, 376, multi, 1, DateTime.Now);
                                    }
                                    y++;
                                }
                                driver.Quit();
                                HttpContext.Current.Session["multiparcel_FLStLoucie"] = "Yes";
                                return "MultiParcel";
                            }

                        }


                    }

                    try
                    {
                        IWebElement Inodata = driver.FindElement(By.XPath("/html/body/app-root/app-real-estate/app-owner-name/div/app-grid-result/div[1]"));
                        if (Inodata.Text.Contains(" No records available"))
                        {
                            HttpContext.Current.Session["Nodata_FLStLucie"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    if (HttpContext.Current.Session["Popup_Removed"] != null && HttpContext.Current.Session["Popup_Removed"].ToString() == "Yes")
                    {
                        driver.FindElement(By.LinkText("Property Card")).SendKeys(Keys.Enter);
                        Thread.Sleep(10000);
                        gc.CreatePdf(orderNumber, outparcelno, "Test", driver, "FL", "Saint Lucie");
                        driver.SwitchTo().Window(driver.WindowHandles.Last());
                        Thread.Sleep(3000);
                    }

                    //string bulktext = driver.FindElement(By.XPath("/html/body/div/div/div[3]/div[2]/div/div[8]/div/div/div[3]/div/div[1]/div[3]")).Text.Trim();                
                    string bulktext = driver.FindElement(By.XPath("/html/body/div/div")).Text.Trim();
                    siteaddr = driver.FindElement(By.XPath("/html/body/div/div/div[3]/div[2]/div/div[1]/div/div/div[1]/div[3]/div[1]")).Text.Trim();
                    bulktext = driver.FindElement(By.XPath("/html/body/div/div/div[3]/div[2]/div/div[8]/div/div/div[3]/div/div[1]/div[3]")).Text.Trim();
                    siteaddr = gc.Between(bulktext, "Site Address: ", "Parcel ID: ").Trim();
                    outparcelno = gc.Between(bulktext, "Parcel ID:", "Sec/Town/Range: ").Trim();
                    accountno = gc.Between(bulktext, "Account #: ", "Map ID: ").Trim();
                    propuse = gc.Between(bulktext, "Use Type: ", "Zoning:").Trim();
                    ownername = driver.FindElement(By.XPath("/html/body/div/div/div[3]/div[2]/div/div[8]/div/div/div[3]/div/div[2]/div[2]")).Text.Trim().Replace("\r\n", ",");
                    legal_desc = driver.FindElement(By.XPath("/html/body/div/div/div[3]/div[2]/div/div[8]/div/div/div[3]/div/div[2]/div[4]/div")).Text.Trim();
                    string bulkyear_built = driver.FindElement(By.XPath("/html/body/div/div/div[3]/div[2]/div/div[8]/div/div/div[5]/div/div[4]")).Text.Trim();
                    year_built = gc.Between(bulkyear_built, "Year Built: ", "Frame:");
                    //Account Number~Owner Name & Mailing Address~Property Address~Property Type~Year Built~Legal Description
                    string property = accountno + "~" + ownername + "~" + siteaddr + "~" + propuse + "~" + year_built + "~" + legal_desc;
                    gc.insert_date(orderNumber, outparcelno, 374, property, 1, DateTime.Now);

                    //assessment details
                    string bulkassessment = driver.FindElement(By.XPath("/html/body/div/div/div[3]/div[2]/div/div[8]/div/div/div[7]/div/div[1]")).Text.Trim();
                    Building_Value = gc.Between(bulkassessment, "Building:", "Land:");
                    LandValue = gc.Between(bulkassessment, "Land:", "Just/Market:");
                    JustValue = gc.Between(bulkassessment, "Just/Market:", "Ag Credit:");
                    AgriculturalCredit = gc.Between(bulkassessment, "Ag Credit:", "Save Our Homes or 10% Cap");
                    Cap = gc.Between(bulkassessment, "Save Our Homes or 10% Cap:", "Assessed:");
                    AssessedValue = gc.Between(bulkassessment, "Assessed:", "Exemption(s):");
                    Exemption = gc.Between(bulkassessment, "Exemption(s):", "Taxable:");
                    TaxableValue = GlobalClass.After(bulkassessment, "Taxable:");
                    //Building Value~Land Value~Just Value~Agricultural Credit~10 % Cap~Assessed Value~Exemption~Taxable Value
                    string ass = Building_Value + "~" + LandValue + "~" + JustValue + "~" + AgriculturalCredit + "~" + Cap + "~" + AssessedValue + "~" + Exemption + "~" + TaxableValue;
                    gc.CreatePdf(orderNumber, outparcelno, "Assessment Details", driver, "FL", "Saint Lucie");
                    gc.insert_date(orderNumber, outparcelno, 375, ass, 1, DateTime.Now);

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    //tax details
                    // outparcelno = "3322-505-0106-000/1";
                    driver.Navigate().GoToUrl("https://stlucie.county-taxes.com/public");
                    driver.FindElement(By.Name("search_query")).SendKeys(outparcelno);
                    gc.CreatePdf(orderNumber, outparcelno, "Tax Search", driver, "FL", "Saint Lucie");
                    driver.FindElement(By.XPath("//*[@id='search-controls']/div/form[1]/div[1]/div/span/button")).SendKeys(Keys.Enter);
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, outparcelno, "Tax Search Results", driver, "FL", "Saint Lucie");

                    //full bill history
                    IWebElement ITaxSearch = driver.FindElement(By.LinkText("Full bill history"));
                    string strITaxSearch = ITaxSearch.GetAttribute("href");
                    driver.Navigate().GoToUrl(strITaxSearch);
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, outparcelno, "Full Bill History", driver, "FL", "Saint Lucie");
                    IWebElement IBillHistorytable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table"));
                    IList<IWebElement> IBillHistoryRow = IBillHistorytable.FindElements(By.TagName("tr"));
                    IList<IWebElement> IBillHistoryTD;
                    IList<IWebElement> IBillHistoryTH;
                    int i = 0; int m = 0; int j = 0; int k = 0; int p = 0;
                    foreach (IWebElement bill in IBillHistoryRow)
                    {
                        string billyear = "", inst = "", paidamount = "", receipt = "", effectivedate = ""; strBalance = ""; strBillDate = "";
                        IBillHistoryTD = bill.FindElements(By.TagName("td"));
                        IBillHistoryTH = bill.FindElements(By.TagName("th"));
                        if (IBillHistoryTD.Count != 0)
                        {
                            try
                            {
                                if (IBillHistoryTD[0].Text.Contains("Redeemed certificate") || IBillHistoryTD[0].Text.Contains("Issued certificate"))
                                {
                                    if (IBillHistoryTD.Count == 5)
                                    {
                                        //billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                        billyear = IBillHistoryTD[0].Text;
                                        strBalance = IBillHistoryTD[1].Text.Replace("\r\n", " ");
                                        strBillDate = IBillHistoryTD[2].Text;
                                        paidamount = IBillHistoryTD[3].Text;
                                        string strTaxHistory2 = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                        gc.insert_date(orderNumber, outparcelno, 377, strTaxHistory2, 1, DateTime.Now);
                                    }
                                }

                                if (IBillHistoryTD.Count == 2)
                                {
                                    strBillDate = IBillHistoryTD[0].Text;
                                    paidamount = IBillHistoryTD[1].Text;
                                    string strTaxHistory2 = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                    gc.insert_date(orderNumber, outparcelno, 377, strTaxHistory2, 1, DateTime.Now);
                                }
                                if (IBillHistoryTD[2].Text.Contains("Paid"))
                                {
                                    inst = "Total"; paidamount = IBillHistoryTD[2].Text.Replace("Paid", ""); strBalance = "";
                                    string strTaxHistory = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                    gc.insert_date(orderNumber, outparcelno, 377, strTaxHistory, 1, DateTime.Now);
                                }

                                if (IBillHistoryTD[0].Text.Contains("Issued certificate"))
                                {
                                    IWebElement ITaxBillCount = IBillHistoryTD[0].FindElement(By.TagName("a"));
                                    string issuelink = ITaxBillCount.GetAttribute("href");
                                    strissuecertificate.Add(issuelink);
                                }


                                if (bill.Text.Contains("Annual Bill") || bill.Text.Contains("Pay this bill") || bill.Text.Contains("Installment Bill"))
                                {
                                    IWebElement ITaxBillCount = IBillHistoryTD[0].FindElement(By.TagName("a"));
                                    string taxlink = ITaxBillCount.GetAttribute("href");
                                    if (bill.Text.Contains("Installment Bill"))
                                    {
                                        strTaxbillPresentYear = GlobalClass.Before(ITaxBillCount.Text, " Installment Bill ");
                                    }
                                    else if (bill.Text.Contains("Annual Bill"))
                                    {
                                        strTaxbillPresentYear = GlobalClass.Before(ITaxBillCount.Text, " Annual Bill");
                                    }
                                    if (strTaxbillPresentYear == strTaxbillPreviousYear)
                                    {
                                        p++;
                                        if (p == 4)
                                        {
                                            k++;
                                        }
                                    }
                                    if (strTaxbillPresentYear != strTaxbillPreviousYear)
                                    {
                                        m++;
                                        j++;
                                        strTaxbillPreviousYear = strTaxbillPresentYear;
                                        k = 0;
                                    }
                                    if ((m <= 4) && (k < 4))
                                    {
                                        if (bill.Text.Contains("Annual Bill"))
                                        {

                                            if (j < 4)
                                            {
                                                //download
                                                try
                                                {
                                                    IWebElement ITaxBill = IBillHistoryTD[3].FindElement(By.TagName("a"));
                                                    string BillTax = ITaxBill.GetAttribute("href");
                                                    downloadlink.Add(BillTax);
                                                    strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                                    gc.downloadfile(BillTax, orderNumber, outparcelno, "Tax Bill" + outparcelno + "-" + strBill, "FL", "Saint Lucie");
                                                }
                                                catch { }
                                                try
                                                {
                                                    IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                                    string BillTax = ITaxBill.GetAttribute("href");
                                                    downloadlink.Add(BillTax);
                                                    strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                                    gc.downloadfile(BillTax, orderNumber, outparcelno, "Tax Bill" + outparcelno + "-" + strBill, "FL", "Saint Lucie");

                                                }
                                                catch
                                                {
                                                }
                                                taxhistorylink.Add(taxlink);
                                            }

                                            //j++;
                                            //k++;

                                        }

                                        else if (bill.Text.Contains("Installment Bill"))
                                        {
                                            if (taxhistorylink.Count == 3)
                                            {
                                                //
                                            }
                                            else if (taxhistorylink.Count == 2)
                                            {
                                                if (j <= 4)
                                                {
                                                    //download
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[3].FindElement(By.TagName("a"));
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                                        downloadlink.Add(BillTax);
                                                        gc.downloadfile(BillTax, orderNumber, outparcelno, "Tax Bill" + outparcelno + "-" + strBill, "FL", "Saint Lucie");
                                                    }
                                                    catch { }
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        downloadlink.Add(BillTax);
                                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                                        gc.downloadfile(BillTax, orderNumber, outparcelno, "Tax Bill" + outparcelno + "-" + strBill, "FL", "Saint Lucie");
                                                    }
                                                    catch
                                                    {
                                                    }


                                                    IWebElement ITaxBillCount1 = IBillHistoryTD[0].FindElement(By.TagName("a"));
                                                    string taxlink1 = ITaxBillCount1.GetAttribute("href");
                                                    taxhistorylinkinst.Add(taxlink1);
                                                    // j++;
                                                }
                                            }
                                            else if (taxhistorylink.Count == 1)
                                            {
                                                if (j <= 4) //7
                                                {
                                                    //download
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[3].FindElement(By.TagName("a"));
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        downloadlink.Add(BillTax);
                                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                                        gc.downloadfile(BillTax, orderNumber, outparcelno, "Tax Bill" + outparcelno + "-" + strBill, "FL", "Saint Lucie");
                                                    }
                                                    catch { }
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        downloadlink.Add(BillTax);
                                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                                        gc.downloadfile(BillTax, orderNumber, outparcelno, "Tax Bill" + outparcelno + "-" + strBill, "FL", "Saint Lucie");
                                                    }
                                                    catch
                                                    {
                                                    }

                                                    IWebElement ITaxBillCount1 = IBillHistoryTD[0].FindElement(By.TagName("a"));
                                                    string taxlink1 = ITaxBillCount1.GetAttribute("href");
                                                    taxhistorylinkinst.Add(taxlink1);
                                                    // j++;
                                                }
                                            }
                                            else if (taxhistorylink.Count == 0)
                                            {
                                                if (j <= 4) //12
                                                {
                                                    //download
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[3].FindElement(By.TagName("a"));
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        downloadlink.Add(BillTax);
                                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                                        gc.downloadfile(BillTax, orderNumber, outparcelno, "Tax Bill" + outparcelno + "-" + strBill, "FL", "Saint Lucie");
                                                    }
                                                    catch { }
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        downloadlink.Add(BillTax);
                                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                                        gc.downloadfile(BillTax, orderNumber, outparcelno, "Tax Bill" + outparcelno + "-" + strBill, "FL", "Saint Lucie");
                                                    }
                                                    catch
                                                    {
                                                    }

                                                    IWebElement ITaxBillCount1 = IBillHistoryTD[0].FindElement(By.TagName("a"));
                                                    string taxlink1 = ITaxBillCount1.GetAttribute("href");
                                                    taxhistorylinkinst.Add(taxlink1);
                                                    // j++;
                                                }
                                            }
                                        }
                                    }


                                    if (bill.Text.Contains("Pay this bill"))
                                    {
                                        try
                                        {
                                            IWebElement ITaxBill = IBillHistoryTD[3].FindElement(By.TagName("a"));
                                            string BillTax = ITaxBill.GetAttribute("href");
                                            strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                            downloadlink.Add(BillTax);
                                            gc.downloadfile(BillTax, orderNumber, outparcelno, "Tax Bill" + outparcelno + "-" + strBill, "FL", "Saint Lucie");
                                        }
                                        catch { }

                                        try
                                        {
                                            IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                            string BillTax = ITaxBill.GetAttribute("href");
                                            strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                            downloadlink.Add(BillTax);
                                            gc.downloadfile(BillTax, orderNumber, outparcelno, "Tax Bill" + outparcelno + "-" + strBill, "FL", "Saint Lucie");
                                        }
                                        catch { }

                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd().Replace("Installment Bill #", "").Replace("Bill", "");
                                        var Splitbill = strBill.Split(' ');
                                        billyear = Splitbill[0]; inst = Splitbill[1];
                                        strBalance = IBillHistoryTD[1].Text.Replace("\r\n", "").TrimStart().TrimEnd();
                                        string strTaxHistory = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                        gc.insert_date(orderNumber, outparcelno, 377, strTaxHistory, 1, DateTime.Now);
                                    }
                                    else
                                    {


                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd().Replace("Installment Bill #", "").Replace("Bill", "");
                                        var Splitbill1 = strBill.Split(' ');
                                        billyear = Splitbill1[0]; inst = Splitbill1[1];
                                        strBalance = IBillHistoryTD[1].Text.Replace("\r\n", "").TrimStart().TrimEnd();
                                        strBillDate = IBillHistoryTD[2].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                        if (strBillDate.Contains("Effective"))
                                        {
                                            strBillDate = strBillDate.Replace("Effective ", "");
                                            var splitdate = strBillDate.Split(' ');
                                            strBillDate = splitdate[0];
                                            effectivedate = splitdate[1];
                                        }
                                        strBillPaid = IBillHistoryTD[3].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                        paidamount = gc.Between(strBillPaid, "Paid ", "Receipt ");
                                        receipt = GlobalClass.After(strBillPaid, "Receipt #");
                                        string strTaxHistory = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                        gc.insert_date(orderNumber, outparcelno, 377, strTaxHistory, 1, DateTime.Now);

                                    }

                                    //try
                                    //{
                                    //    IWebElement ITaxBill = IBillHistoryTD[3].FindElement(By.TagName("a"));
                                    //    string BillTax = ITaxBill.GetAttribute("href");
                                    //    strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                    //    gc.downloadfile(BillTax, orderNumber, outparcelno, "Tax Bill" + " " + strBill, "FL", "Saint Lucie");
                                    //}
                                    //catch { }
                                    //try
                                    //{
                                    //    IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                    //    string BillTax = ITaxBill.GetAttribute("href");
                                    //    gc.downloadfile(BillTax, orderNumber, outparcelno, "Tax Bill" + " " + strBill, "FL", "Saint Lucie");
                                    //}
                                    //catch
                                    //{
                                    //}


                                }

                                else
                                {
                                    strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd().Replace("Installment Bill #", "").Replace("Bill", "");
                                    var Splitbill = strBill.Split(' ');
                                    billyear = Splitbill[0]; inst = Splitbill[1];
                                    strBalance = IBillHistoryTD[1].Text.Replace("\r\n", "").TrimStart().TrimEnd();
                                    strBillDate = IBillHistoryTD[2].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                    if (strBillDate.Contains("Effective"))
                                    {
                                        strBillDate = strBillDate.Replace("Effective ", "");
                                        var splitdate = strBillDate.Split(' ');
                                        strBillDate = splitdate[0];
                                        effectivedate = splitdate[1];
                                    }
                                    strBillPaid = IBillHistoryTD[3].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                    paidamount = gc.Between(strBillPaid, "Paid ", "Receipt ");
                                    receipt = GlobalClass.After(strBillPaid, "Receipt #");
                                    //Tax Year~Installment~Total Due~Paid Date~Effective Date~Paid Amount~Receipt Number
                                    string strTaxHistory = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                    gc.insert_date(orderNumber, outparcelno, 377, strTaxHistory, 1, DateTime.Now);

                                }

                            }
                            catch
                            {

                            }


                        }
                        if (IBillHistoryTH.Count != 0)
                        {
                            if (IBillHistoryTH[0].Text.Contains("Total Balance"))
                            {
                                inst = "Total Balance"; strBalance = IBillHistoryTH[1].Text.Replace("\r\n", "").TrimStart().TrimEnd();
                                string strTaxHistory = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                gc.insert_date(orderNumber, outparcelno, 377, strTaxHistory, 1, DateTime.Now);
                            }
                        }

                        i++;
                    }



                    int q = 0;
                    foreach (string URL in taxhistorylink)
                    {
                        accno = ""; alterkey = ""; taxowner = ""; tax_addr = ""; millagecode = ""; ctax_year = ""; combinedtaxamount = ""; grosstax = ""; cpaidamount = ""; cpaiddate = ""; ceffdate = ""; creceipt = "";
                        driver.Navigate().GoToUrl(URL);
                        Thread.Sleep(3000);
                        //*[@id="content"]/div[1]/div[7]/div/div[1]/div[2]
                        string cctaxyear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[1]/div[2]")).Text.Trim().Replace("Real Estate", "").Replace("Print this bill (PDF)", "").Replace("\r\n", "");
                        gc.CreatePdf(orderNumber, outparcelno, "Tax Details" + cctaxyear, driver, "FL", "Saint Lucie");

                        if (q == 0)
                        {
                            //add valorem Tax
                            IWebElement valoremtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[2]"));
                            IList<IWebElement> valoremtableRow = valoremtable.FindElements(By.TagName("tr"));
                            int valoremtablerowcount = valoremtableRow.Count;
                            IList<IWebElement> valoremtablerowTD;
                            int d = 0;
                            foreach (IWebElement rowid in valoremtableRow)
                            {
                                valoremtablerowTD = rowid.FindElements(By.TagName("td"));
                                if (valoremtablerowTD.Count != 0 && !rowid.Text.Contains("Taxing authority"))
                                {
                                    string valoremtax = "";
                                    if (d < valoremtablerowcount - 1)
                                    {
                                        //Taxing Authority~Tax Type~Millage~Assessed~Exemption~Taxable~Taxes
                                        valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[1].Text + "~" + valoremtablerowTD[2].Text + "~" + valoremtablerowTD[3].Text + "~" + valoremtablerowTD[4].Text + "~" + valoremtablerowTD[5].Text;
                                        gc.insert_date(orderNumber, outparcelno, 378, valoremtax, 1, DateTime.Now);
                                    }
                                    else if (d < valoremtablerowcount)
                                    {
                                        milagerate = valoremtablerowTD[0].Text;
                                        valoremtax = "Total" + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + valoremtablerowTD[1].Text;
                                        gc.insert_date(orderNumber, outparcelno, 378, valoremtax, 1, DateTime.Now);
                                    }
                                }
                                d++;
                            }

                            //Non Ad-Valorems
                            try
                            {
                                IWebElement nonvaloremtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[3]"));
                                IList<IWebElement> nonvaloremtableRow = nonvaloremtable.FindElements(By.TagName("tr"));
                                int nonvaloremtablerowcount = nonvaloremtableRow.Count;
                                IList<IWebElement> nonvaloremtablerowTD;
                                int e = 0;
                                foreach (IWebElement rowid in nonvaloremtableRow)
                                {
                                    nonvaloremtablerowTD = rowid.FindElements(By.TagName("td"));
                                    if (nonvaloremtablerowTD.Count != 0 && !rowid.Text.Contains("Levying authority"))
                                    {
                                        string valoremtax = "";
                                        if (e < nonvaloremtablerowcount - 1)
                                        {
                                            //Taxing Authority~Tax Type~Millage~Assessed~Exemption~Taxable~Taxes
                                            valoremtax = nonvaloremtablerowTD[0].Text + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[1].Text + "~" + nonvaloremtablerowTD[2].Text;
                                            gc.insert_date(orderNumber, outparcelno, 378, valoremtax, 1, DateTime.Now);
                                        }
                                        else if (e < nonvaloremtablerowcount)
                                        {
                                            valoremtax = "Total" + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[0].Text;
                                            gc.insert_date(orderNumber, outparcelno, 378, valoremtax, 1, DateTime.Now);
                                        }
                                    }
                                    e++;
                                }
                            }
                            catch { }
                        }
                        //*[@id="content"]/div[1]/div[7]/div/p
                        //ctax_year = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[1]/div[2]/text()")).Text.Trim().Replace("Real Estate ", "");                    
                        string currenttaxbulktext = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/p")).Text;
                        combinedtaxamount = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/p")).Text.Trim().Replace("Combined taxes and assessments: ", "");
                        try
                        {
                            string bulkgrosstax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]/tbody/tr/td[1]")).Text.Trim();
                            if (bulkgrosstax.Contains("Gross"))
                            {

                                grosstax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]/tbody/tr/td[1]")).Text.Trim().Replace("Gross", "");
                                //if paid by                 
                                string ifpaidbulk = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]/tbody/tr/td[3]")).Text;
                                string[] stringSeparators1 = new string[] { "\r\n" };
                                string[] lines1 = ifpaidbulk.Split(stringSeparators1, StringSplitOptions.None);
                                ifpaidby = lines1[0]; pleasepay = lines1[1];
                                string single = cctaxyear + "~" + ifpaidby + "~" + pleasepay;
                                gc.insert_date(orderNumber, outparcelno, 401, single, 1, DateTime.Now);
                            }

                            else
                            {
                                string IfPaidBy = "", PlesePay = "", DueDate = "", itaxyear = "";
                                try
                                {
                                    IWebElement multitableElement26 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]"));
                                    IList<IWebElement> multitableRow26 = multitableElement26.FindElements(By.TagName("tr"));
                                    IList<IWebElement> multirowTD26;
                                    foreach (IWebElement row in multitableRow26)
                                    {
                                        multirowTD26 = row.FindElements(By.TagName("td"));
                                        int iRowsCount = multirowTD26.Count;
                                        for (int n = 0; n < iRowsCount; n++)
                                        {
                                            if ((!multirowTD26[n].Text.Contains("Face Amt")) && (!multirowTD26[n].Text.Contains("Certificate")))
                                            {
                                                IfPaidBy = multirowTD26[n].Text.Replace("\r\n", "~");
                                                var IfpaySplit = IfPaidBy.Split('~');
                                                IfPaidBy = IfpaySplit[0];
                                                PlesePay = IfpaySplit[1];
                                                DueDate = cctaxyear + "~" + IfPaidBy + "~" + PlesePay;
                                                gc.insert_date(orderNumber, outparcelno, 401, DueDate, 1, DateTime.Now);

                                            }
                                        }
                                    }

                                }
                                catch { }
                            }
                        }
                        catch { }
                        try
                        {

                            string bulktext1 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[5]")).Text.Replace("\r\n", ",");
                            taxowner = gc.Between(bulktext1, "Owner,", "Situs address").Replace(",,", "");
                            tax_addr = gc.Between(bulktext1, "Situs address", "Legal description").Trim().Replace(",", "");
                            //*[@id='content']/div[1]/div[7]/div/table[1]/tbody
                            IWebElement currrtaxtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[1]/tbody"));
                            IList<IWebElement> currrtaxtableRow = currrtaxtable.FindElements(By.TagName("tr"));
                            int currrtaxtabletableRowcount = currrtaxtableRow.Count;
                            IList<IWebElement> currrtaxtableRowTD;
                            int e = 0;
                            foreach (IWebElement rowid in currrtaxtableRow)
                            {
                                currrtaxtableRowTD = rowid.FindElements(By.TagName("td"));
                                if (currrtaxtableRowTD.Count != 0 && !rowid.Text.Contains("Account number"))
                                {
                                    accno = currrtaxtableRowTD[0].Text; alterkey = currrtaxtableRowTD[1].Text; millagecode = currrtaxtableRowTD[3].Text;
                                }
                            }

                        }
                        catch { }
                        try
                        {

                            string paidbulk = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[3]/div[1]/div")).Text.Trim();
                            cpaiddate = gc.Between(paidbulk, "PAID ", " $").Trim();
                            cpaidamount = gc.Between(paidbulk, "$", "Receipt ").Trim();
                            cpaidamount = "$" + cpaidamount;
                            creceipt = GlobalClass.After(paidbulk, "Receipt #");
                        }
                        catch { }

                        ////Parcel Details
                        //IWebElement ITaxSearchparcell = driver.FindElement(By.LinkText("Parcel details"));
                        //string strITaxSearchparcell = ITaxSearchparcell.GetAttribute("href");
                        //driver.Navigate().GoToUrl(strITaxSearchparcell);
                        //Thread.Sleep(3000);
                        //gc.CreatePdf(orderNumber, outparcelno, "Parcel Details" + cctaxyear, driver, "FL", "Saint Lucie");
                        //try
                        //{
                        //    string parcelbulktext = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[3]/div[2]")).Text.Replace("\r\n", ",");
                        //    taxowner = gc.Between(parcelbulktext, "Owner,", "Situs").Replace("and 1 other", "").Replace(",,", "");
                        //    tax_addr = gc.Between(parcelbulktext, "Situs", "Account number").Trim().Replace(",", "");
                        //    accno = gc.Between(parcelbulktext, "Account number", "Alternate Key").Trim().Replace("\r\n", "").Replace(",", "");
                        //    alterkey = gc.Between(parcelbulktext, "Alternate Key", "Millage code").Trim().Replace(",", "");
                        //    millagecode = gc.Between(parcelbulktext, "Millage code", "Millage rate").Trim().Replace(",", "");
                        //    try
                        //    {
                        //        milagerate = gc.Between(parcelbulktext, "Millage rate", "Assessed value").Trim().Replace(",", "");
                        //    }
                        //    catch { milagerate = gc.Between(parcelbulktext, "Millage rate", "Escrow company").Trim().Replace(",", ""); }
                        //    try
                        //    {
                        //        ctax_year = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[4]/div[1]/h3")).Text.Trim().Replace("View", "").Replace("\r\n", "");
                        //    }
                        //    catch { }
                        //}
                        //catch
                        //{ }

                        //Account Number~Alternate Key~Owner Name & Mailing Address~Property Address~Millage Code~Millage rate~Tax Year~Tax Amount~Gross Tax~Paid Amount~Paid Date~Effective Date~Receipt Number
                        string curtax = accno + "~" + alterkey + "~" + taxowner + "~" + tax_addr + "~" + millagecode + "~" + milagerate + "~" + cctaxyear + "~" + combinedtaxamount + "~" + grosstax + "~" + cpaidamount + "~" + cpaiddate + "~" + ceffdate + "~" + creceipt;
                        gc.insert_date(orderNumber, outparcelno, 379, curtax, 1, DateTime.Now);
                        q++;
                    }
                    q = 0;
                    foreach (string URL in taxhistorylinkinst)
                    {
                        accno = ""; alterkey = ""; taxowner = ""; tax_addr = ""; millagecode = ""; ctax_year = ""; combinedtaxamount = ""; grosstax = ""; cpaidamount = ""; cpaiddate = ""; ceffdate = ""; creceipt = "";
                        driver.Navigate().GoToUrl(URL);
                        Thread.Sleep(3000);
                        //*[@id="content"]/div[1]/div[7]/div/div[1]/div[2]
                        string cctaxyear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[1]/div[2]")).Text.Trim().Replace("Real Estate", "").Replace("Print this bill (PDF)", "").Replace("\r\n", "");
                        gc.CreatePdf(orderNumber, outparcelno, "Tax Details" + cctaxyear, driver, "FL", "Saint Lucie");

                        if (q == 0)
                        {
                            //add valorem Tax
                            IWebElement valoremtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[2]"));
                            IList<IWebElement> valoremtableRow = valoremtable.FindElements(By.TagName("tr"));
                            int valoremtablerowcount = valoremtableRow.Count;
                            IList<IWebElement> valoremtablerowTD;
                            int d = 0;
                            foreach (IWebElement rowid in valoremtableRow)
                            {
                                valoremtablerowTD = rowid.FindElements(By.TagName("td"));
                                if (valoremtablerowTD.Count != 0 && !rowid.Text.Contains("Taxing authority"))
                                {
                                    string valoremtax = "";
                                    if (d < valoremtablerowcount - 1)
                                    {
                                        //Taxing Authority~Tax Type~Millage~Assessed~Exemption~Taxable~Taxes
                                        valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[1].Text + "~" + valoremtablerowTD[2].Text + "~" + valoremtablerowTD[3].Text + "~" + valoremtablerowTD[4].Text + "~" + valoremtablerowTD[5].Text;
                                        gc.insert_date(orderNumber, outparcelno, 378, valoremtax, 1, DateTime.Now);
                                    }
                                    else if (d < valoremtablerowcount)
                                    {
                                        milagerate = valoremtablerowTD[0].Text;
                                        valoremtax = "Total" + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + valoremtablerowTD[1].Text;
                                        gc.insert_date(orderNumber, outparcelno, 378, valoremtax, 1, DateTime.Now);
                                    }
                                }
                                d++;
                            }

                            //Non Ad-Valorems
                            try
                            {
                                IWebElement nonvaloremtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[3]"));
                                IList<IWebElement> nonvaloremtableRow = nonvaloremtable.FindElements(By.TagName("tr"));
                                int nonvaloremtablerowcount = nonvaloremtableRow.Count;
                                IList<IWebElement> nonvaloremtablerowTD;
                                int e = 0;
                                foreach (IWebElement rowid in nonvaloremtableRow)
                                {
                                    nonvaloremtablerowTD = rowid.FindElements(By.TagName("td"));
                                    if (nonvaloremtablerowTD.Count != 0 && !rowid.Text.Contains("Levying authority"))
                                    {
                                        string valoremtax = "";
                                        if (e < nonvaloremtablerowcount - 1)
                                        {
                                            //Taxing Authority~Tax Type~Millage~Assessed~Exemption~Taxable~Taxes
                                            valoremtax = nonvaloremtablerowTD[0].Text + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[1].Text + "~" + nonvaloremtablerowTD[2].Text;
                                            gc.insert_date(orderNumber, outparcelno, 378, valoremtax, 1, DateTime.Now);
                                        }
                                        else if (e < nonvaloremtablerowcount)
                                        {
                                            valoremtax = "Total" + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[0].Text;
                                            gc.insert_date(orderNumber, outparcelno, 378, valoremtax, 1, DateTime.Now);
                                        }
                                    }
                                    e++;
                                }
                            }
                            catch { }
                        }
                        //*[@id="content"]/div[1]/div[7]/div/p
                        //ctax_year = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[1]/div[2]/text()")).Text.Trim().Replace("Real Estate ", "");                    
                        string currenttaxbulktext = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/p")).Text;
                        combinedtaxamount = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/p")).Text.Trim().Replace("Combined taxes and assessments: ", "");
                        try
                        {
                            string IfPaidBy = "", PlesePay = "", DueDate = "", itaxyear = "";
                            string bulkgrosstax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]/tbody/tr/td[1]")).Text.Trim();
                            if (bulkgrosstax.Contains("Gross"))
                            {

                                grosstax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]/tbody/tr/td[1]")).Text.Trim().Replace("Gross", "");
                                //if paid by                 
                                //string ifpaidbulk = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]/tbody/tr/td[3]")).Text;
                                //string[] stringSeparators1 = new string[] { "\r\n" };
                                //string[] lines1 = ifpaidbulk.Split(stringSeparators1, StringSplitOptions.None);
                                //ifpaidby = lines1[0]; pleasepay = lines1[1];
                                //string single = cctaxyear + "~" + ifpaidby + "~" + pleasepay;
                                //gc.insert_date(orderNumber, outparcelno, 401, single, 1, DateTime.Now);
                                try
                                {
                                    IWebElement multitableElement26 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]"));
                                    IList<IWebElement> multitableRow26 = multitableElement26.FindElements(By.TagName("tr"));
                                    IList<IWebElement> multirowTD26;
                                    foreach (IWebElement row in multitableRow26)
                                    {
                                        multirowTD26 = row.FindElements(By.TagName("td"));
                                        int iRowsCount = multirowTD26.Count;
                                        for (int n = 0; n < iRowsCount; n++)
                                        {
                                            if (multirowTD26[n].Text.Trim() != "" && (!multirowTD26[n].Text.Contains("Gross")) && (!multirowTD26[n].Text.Contains("Face Amt")) && (!multirowTD26[n].Text.Contains("Certificate")) && (!multirowTD26[n].Text.Contains("Discount")) && (!multirowTD26[n].Text.Contains("If paid by:\r\nPlease pay:")))
                                            {
                                                IfPaidBy = multirowTD26[n].Text.Replace("\r\n", "~");
                                                var IfpaySplit = IfPaidBy.Split('~');
                                                IfPaidBy = IfpaySplit[0];
                                                PlesePay = IfpaySplit[1];
                                                DueDate = cctaxyear + "~" + IfPaidBy + "~" + PlesePay;
                                                gc.insert_date(orderNumber, outparcelno, 401, DueDate, 1, DateTime.Now);

                                            }
                                        }
                                    }

                                }
                                catch { }
                            }

                            else
                            {
                                //string IfPaidBy = "", PlesePay = "", DueDate = "", itaxyear = "";
                                try
                                {
                                    IWebElement multitableElement26 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]"));
                                    IList<IWebElement> multitableRow26 = multitableElement26.FindElements(By.TagName("tr"));
                                    IList<IWebElement> multirowTD26;
                                    foreach (IWebElement row in multitableRow26)
                                    {
                                        multirowTD26 = row.FindElements(By.TagName("td"));
                                        int iRowsCount = multirowTD26.Count;
                                        for (int n = 0; n < iRowsCount; n++)
                                        {
                                            if ((!multirowTD26[n].Text.Contains("Face Amt")) && (!multirowTD26[n].Text.Contains("Certificate")) && (!multirowTD26[n].Text.Contains("If paid by:\r\nPlease pay:")))
                                            {
                                                IfPaidBy = multirowTD26[n].Text.Replace("\r\n", "~");
                                                var IfpaySplit = IfPaidBy.Split('~');
                                                IfPaidBy = IfpaySplit[0];
                                                PlesePay = IfpaySplit[1];
                                                DueDate = cctaxyear + "~" + IfPaidBy + "~" + PlesePay;
                                                gc.insert_date(orderNumber, outparcelno, 401, DueDate, 1, DateTime.Now);

                                            }
                                        }
                                    }

                                }
                                catch { }
                            }
                        }
                        catch { }
                        try
                        {

                            string bulktext1 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[5]")).Text.Replace("\r\n", ",");
                            taxowner = gc.Between(bulktext1, "Owner,", "Situs address").Replace(",,", "");
                            tax_addr = gc.Between(bulktext1, "Situs address", "Legal description").Trim().Replace(",", "");
                            //*[@id='content']/div[1]/div[7]/div/table[1]/tbody
                            IWebElement currrtaxtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[1]/tbody"));
                            IList<IWebElement> currrtaxtableRow = currrtaxtable.FindElements(By.TagName("tr"));
                            int currrtaxtabletableRowcount = currrtaxtableRow.Count;
                            IList<IWebElement> currrtaxtableRowTD;
                            int e = 0;
                            foreach (IWebElement rowid in currrtaxtableRow)
                            {
                                currrtaxtableRowTD = rowid.FindElements(By.TagName("td"));
                                if (currrtaxtableRowTD.Count != 0 && !rowid.Text.Contains("Account number"))
                                {
                                    accno = currrtaxtableRowTD[0].Text; alterkey = currrtaxtableRowTD[1].Text; millagecode = currrtaxtableRowTD[3].Text;
                                }
                            }

                        }
                        catch { }
                        try
                        {

                            string paidbulk = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[3]/div[1]/div")).Text.Trim();
                            cpaiddate = gc.Between(paidbulk, "PAID ", " $").Trim();
                            cpaidamount = gc.Between(paidbulk, "$", "Receipt ").Trim();
                            cpaidamount = "$" + cpaidamount;
                            creceipt = GlobalClass.After(paidbulk, "Receipt #");
                        }
                        catch { }

                        ////Parcel Details
                        //IWebElement ITaxSearchparcell = driver.FindElement(By.LinkText("Parcel details"));
                        //string strITaxSearchparcell = ITaxSearchparcell.GetAttribute("href");
                        //driver.Navigate().GoToUrl(strITaxSearchparcell);
                        //Thread.Sleep(3000);
                        //gc.CreatePdf(orderNumber, outparcelno, "Parcel Details" + cctaxyear, driver, "FL", "Saint Lucie");
                        //try
                        //{
                        //    string parcelbulktext = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[3]/div[2]")).Text.Replace("\r\n", ",");
                        //    taxowner = gc.Between(parcelbulktext, "Owner,", "Situs").Replace("and 1 other", "").Replace(",,", "");
                        //    tax_addr = gc.Between(parcelbulktext, "Situs", "Account number").Trim().Replace(",", "");
                        //    accno = gc.Between(parcelbulktext, "Account number", "Alternate Key").Trim().Replace("\r\n", "").Replace(",", "");
                        //    alterkey = gc.Between(parcelbulktext, "Alternate Key", "Millage code").Trim().Replace(",", "");
                        //    millagecode = gc.Between(parcelbulktext, "Millage code", "Millage rate").Trim().Replace(",", "");
                        //    try
                        //    {
                        //        milagerate = gc.Between(parcelbulktext, "Millage rate", "Assessed value").Trim().Replace(",", "");
                        //    }
                        //    catch { milagerate = gc.Between(parcelbulktext, "Millage rate", "Escrow company").Trim().Replace(",", ""); }
                        //    try
                        //    {
                        //        ctax_year = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[4]/div[1]/h3")).Text.Trim().Replace("View", "").Replace("\r\n", "");
                        //    }
                        //    catch { }
                        //}
                        //catch
                        //{ }

                        //Account Number~Alternate Key~Owner Name & Mailing Address~Property Address~Millage Code~Millage rate~Tax Year~Tax Amount~Gross Tax~Paid Amount~Paid Date~Effective Date~Receipt Number
                        string curtax = accno + "~" + alterkey + "~" + taxowner + "~" + tax_addr + "~" + millagecode + "~" + milagerate + "~" + cctaxyear + "~" + combinedtaxamount + "~" + grosstax + "~" + cpaidamount + "~" + cpaiddate + "~" + ceffdate + "~" + creceipt;
                        gc.insert_date(orderNumber, outparcelno, 379, curtax, 1, DateTime.Now);
                        q++;
                    }



                    //issue certificate
                    string adno = "", faceamount = "", issuedate = "", ex_date = "", buyer = "", intrate = "";
                    foreach (string URL in strissuecertificate)
                    {
                        driver.Navigate().GoToUrl(URL);
                        Thread.Sleep(3000);
                        string issueyear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/p")).Text.Trim().Replace("This parcel has an issued certificate for ", "").Replace(".", "");
                        gc.CreatePdf(orderNumber, outparcelno, "Issue Certificate Details" + issueyear, driver, "FL", "Saint Lucie");
                        string issuecertificatebulktext = driver.FindElement(By.XPath("//*[@id='content']/div[1]/dl[2]")).Text.Trim();
                        adno = gc.Between(issuecertificatebulktext, "Advertised number", "Face amount").Trim();
                        faceamount = gc.Between(issuecertificatebulktext, "Face amount", "Issued date").Trim();
                        issuedate = gc.Between(issuecertificatebulktext, "Issued date", "Expiration date").Trim();
                        ex_date = gc.Between(issuecertificatebulktext, "Expiration date", "Buyer").Trim();
                        buyer = gc.Between(issuecertificatebulktext, "Buyer", "Interest rate").Trim().Replace("\r\n", ",");
                        intrate = GlobalClass.After(issuecertificatebulktext, "Interest rate");
                        //Tax Year~Advertised Number~Face Amount~Issued Date~Expiration Date~Buyer~Interest Rate
                        string isscer = issueyear + "~" + adno + "~" + faceamount + "~" + issuedate + "~" + ex_date + "~" + buyer + "~" + intrate;
                        gc.insert_date(orderNumber, outparcelno, 380, isscer, 1, DateTime.Now);
                    }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Quit();
                    gc.mergpdf(orderNumber, "FL", "Saint Lucie");

                    gc.insert_TakenTime(orderNumber, "FL", "Saint Lucie", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
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

        public void multiparcel(string ordernumber)
        {
            var item = driver.FindElement(By.XPath("/html/body/app-root/app-real-estate/app-site-address/div/app-grid-result/div[2]/kendo-grid/kendo-pager/kendo-pager-page-sizes/select"));
            var selectElement = new SelectElement(item);
            selectElement.SelectByText("50");
            IWebElement multiaddrtableElement = driver.FindElement(By.XPath("/html/body/app-root/app-real-estate/app-site-address/div/app-grid-result/div[2]/kendo-grid/div/kendo-grid-list/div/div[1]/table/tbody"));
            IList<IWebElement> multiaddrtableRow = multiaddrtableElement.FindElements(By.TagName("tr"));
            int multiaddrtableRowcount = multiaddrtableRow.Count;
            IList<IWebElement> multiaddrrowTD;
            int o = 0;
            if (multiaddrtableRowcount == 2)
            {
                foreach (IWebElement row in multiaddrtableRow)
                {
                    if (0 <= 25)
                    {
                        multiaddrrowTD = row.FindElements(By.TagName("td"));
                        string multi = multiaddrrowTD[2].Text + "~" + multiaddrrowTD[3].Text;
                        gc.insert_date(ordernumber, multiaddrrowTD[2].Text, 376, multi, 1, DateTime.Now);
                    }

                    o++;
                }
            }
        }

    }
}