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

namespace ScrapMaricopa
{
    public class WebDriver_AdaID
    {

        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();

        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_Ada(string address, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "", latestfile = ""; ;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new PhantomJSDriver();            
            var option = new ChromeOptions();
            option.AddArgument("No-Sandbox");
            using (driver = new ChromeDriver(option))
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("http://www.adacountyassessor.org/propsys/AddressSearch.jsp");
                    Thread.Sleep(2000);
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='contents']/form/input[1]")).Click();
                        Thread.Sleep(1000);
                        driver.FindElement(By.XPath("//*[@id='contents']/form/input[3]")).SendKeys(Keys.Enter);
                        Thread.Sleep(1000);
                    }
                    catch { }

                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, "", ownername, address, "ID", "Ada");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_AdaID"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";

                    }


                    if (searchType == "address")
                    {//*[@id="landrecordsearch"]/ul/li[2]/a
                        try
                        {
                            IWebElement IAddressSearch1 = driver.FindElement(By.XPath("//*[@id='landrecordsearch']/ul/li[2]/a"));
                            IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                            js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                            Thread.Sleep(9000);
                        }
                        catch { }
                        driver.FindElement(By.Name("address")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "ID", "Ada");
                        driver.FindElement(By.XPath("//*[@id='contents']/form/table/tbody/tr[3]/td[2]/input")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "ID", "Ada");
                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='footer']")));
                        gc.CreatePdf_WOP(orderNumber, "Address Search Result1", driver, "ID", "Ada");
                        Thread.Sleep(2000);
                        int i = 0;
                        string searchclick = "";
                        try
                        {
                            string strParcel = "", strAddress = "", strYear = "";
                            IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='contents']/table/tbody"));
                            IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> multiTD;
                            foreach (IWebElement multi in multiRow)
                            {

                                multiTD = multi.FindElements(By.TagName("td"));

                                if (multiTD[2].Text.Contains(address.ToUpper()))
                                {
                                    strParcel = multiTD[0].Text;
                                    strYear = multiTD[1].Text;
                                    strAddress = multiTD[2].Text;
                                    IWebElement IsearchClick = multiTD[0].FindElement(By.TagName("a"));
                                    searchclick = IsearchClick.GetAttribute("href");
                                    string multidetails = strYear + "~" + strAddress;
                                    gc.insert_date(orderNumber, strParcel, 905, multidetails, 1, DateTime.Now);
                                    i++;
                                }
                            }

                        }
                        catch { }
                        if (i == 1)
                        {
                            driver.Navigate().GoToUrl(searchclick);
                            Thread.Sleep(5000);
                            gc.CreatePdf(orderNumber, parcelNumber, "address search Result", driver, "ID", "Ada");
                        }
                        if (i > 1 && i <= 25)
                        {
                            HttpContext.Current.Session["multiparcel_AdaID"] = "Yes";
                            driver.Quit();
                            return "MultiParcel";
                        }
                        if (i > 25)
                        {
                            HttpContext.Current.Session["multiparcel_AdaID_Maximum"] = "Maximum";
                            driver.Quit();
                            return "Maximum";
                        }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='contents']/span")).Text;
                            if (nodata.Contains("No Address Found"))
                            {
                                HttpContext.Current.Session["Nodata_AdaID"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.LinkText("Search by Parcel")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "parcel search", driver, "ID", "Ada");
                        driver.FindElement(By.XPath("//*[@id='contents']/form/table/tbody/tr[1]/td[2]/input")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "parcel search Input", driver, "ID", "Ada");
                        driver.FindElement(By.XPath("//*[@id='contents']/form/table/tbody/tr[3]/td[2]/input")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcelNumber, "parcel search Result", driver, "ID", "Ada");
                        // driver.FindElement(By.XPath("//*[@id='contents']/table/tbody/tr/td[1]/a")).SendKeys(Keys.Enter);
                        string info1 = "";
                        IWebElement info = driver.FindElement(By.XPath("//*[@id='contents']/table/tbody/tr/td[1]/a"));
                        info1 = info.GetAttribute("href");
                        driver.Navigate().GoToUrl(info1);
                        //Thread.Sleep(4000);
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='contents']/span")).Text;
                            if (nodata.Contains("No Parcel Found"))
                            {
                                HttpContext.Current.Session["Nodata_AdaID"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    // Property Details

                    Thread.Sleep(4000);
                    string bulkdata = "";

                    //try
                    //{
                    //    if (bulkdata == "")
                    //    {
                    //       IWebElement propdata = driver.FindElement(By.XPath("//*[@id='detailstab']"));
                    //        bulkdata = propdata.Text;
                    //    }
                    //}
                    //catch { }
                    //try
                    //{
                    //    if (bulkdata == "")
                    //    {
                    //        IWebElement propdata = driver.FindElement(By.XPath("//*[@id='detailstab']/table/tbody"));
                    //        bulkdata = propdata.Text;
                    //    }
                    //}
                    //catch (Exception ex)
                    //{
                    //    GlobalClass.LogError(ex, bulkdata + "TestingError0  " + orderNumber);
                    //}

                    string parcel_id = "", parcel_status = "", Owner_Name = "", Property_Address = "", TaxCodeArea = "", Legal_Desc = "";
                    string Sub_Division = "", Town = "", Year_built = "", Legal_Desc1 = "", Legal_Desc2 = "";

                    IWebElement IProperty = driver.FindElement(By.Id("detailstab"));
                    IList<IWebElement> IPropertyRow = IProperty.FindElements(By.TagName("tr"));
                    IList<IWebElement> IPropertyTD;
                    foreach (IWebElement prop in IPropertyRow)
                    {
                        IPropertyTD = prop.FindElements(By.TagName("td"));
                        if (IPropertyTD.Count != 0 && prop.Text.Contains("Parcel:"))
                        {
                            bulkdata = IPropertyTD[0].Text;
                        }
                    }
                    parcel_id = gc.Between(bulkdata, "Parcel", "Year").Replace(":", "").Trim();
                    parcelNumber = parcel_id;
                    parcel_status = gc.Between(bulkdata, "Parcel Status", "Primary Owner").Replace(":", "");
                    Owner_Name = gc.Between(bulkdata, "Primary Owner", "Zone Code").Replace(":", "");
                    gc.CreatePdf(orderNumber, parcelNumber, "property Details", driver, "ID", "Ada");

                    if (bulkdata.Contains("Instrument Number") && !bulkdata.Contains("Address"))
                    {
                        TaxCodeArea = gc.Between(bulkdata, "Tax Code Area", "Instrument Number").Replace(":", "");
                        Legal_Desc = GlobalClass.After(bulkdata, "Property Description").Replace(":", "").Replace("\r\n", "").Trim();
                    }
                    if (bulkdata.Contains("Property Description") && !bulkdata.Contains("Instrument Number") && !bulkdata.Contains("Address"))
                    {
                        TaxCodeArea = gc.Between(bulkdata, "Tax Code Area", "Property Description").Replace(":", "");
                        Legal_Desc = GlobalClass.After(bulkdata, "Property Description").Replace(":", "").Replace("\r\n", " ");
                    }
                    if (bulkdata.Contains("Instrument Number") && bulkdata.Contains("Address"))
                    {
                        TaxCodeArea = gc.Between(bulkdata, "Tax Code Area", "Instrument Number").Replace(":", "");
                        Legal_Desc = gc.Between(bulkdata, "Property Description", "Address").Replace(":", "").Replace("\r\n", "").Trim();
                    }
                    if (bulkdata.Contains("Property Description") && !bulkdata.Contains("Instrument Number") && bulkdata.Contains("Address"))
                    {
                        TaxCodeArea = gc.Between(bulkdata, "Tax Code Area", "Property Description").Replace(":", "");
                        Legal_Desc = gc.Between(bulkdata, "Property Description", "Address").Replace(":", "").Replace("\r\n", " ");
                    }

                    IWebElement Iaddress = driver.FindElement(By.XPath("//*[@id='parceldetails']"));

                    Property_Address = gc.Between(Iaddress.Text, "Address", "Subdivision").Replace(":", "");
                    Sub_Division = gc.Between(Iaddress.Text, "Subdivision", "Land Group Type").Replace(":", "");
                    Town = GlobalClass.After(Iaddress.Text, "Township/Range/Section").Replace(":", "");


                    IWebElement IValuation = driver.FindElement(By.Id("valuationdtab-tab"));
                    IList<IWebElement> IValuationRow = IValuation.FindElements(By.TagName("a"));
                    foreach (IWebElement valuation in IValuationRow)
                    {
                        if (valuation.Text != "" && valuation.Text.Contains("Valuation"))
                        {
                            valuation.Click();
                        }
                    }
                    // valuation 
                    int j = 0;
                    string AssessmentDetails = "";
                    //try
                    //{
                    //    driver.FindElement(By.LinkText("Valuation")).SendKeys(Keys.Enter);
                    //    Thread.Sleep(4000);
                    //}
                    //catch (Exception ex)
                    //{
                    //    GlobalClass.LogError(ex, orderNumber);

                    //}
                    //try
                    //{
                    //    IWebElement IValuation = driver.FindElement(By.LinkText("Valuation"));
                    //    IJavaScriptExecutor js5 = driver as IJavaScriptExecutor;
                    //    js5.ExecuteScript("arguments[0].click();", IValuation);
                    //    Thread.Sleep(4000);
                    //}
                    //catch { }
                    gc.CreatePdf(orderNumber, parcelNumber, "Assessment search ", driver, "ID", "Ada");
                    IWebElement Bigdata2 = driver.FindElement(By.XPath("//*[@id='valuationdtab']/table[1]/tbody"));
                    IList<IWebElement> TRBigdata2 = Bigdata2.FindElements(By.TagName("tr"));
                    IList<IWebElement> THBigdata2 = Bigdata2.FindElements(By.TagName("th"));
                    IList<IWebElement> TDBigdata2;
                    foreach (IWebElement row in TRBigdata2)
                    {
                        TDBigdata2 = row.FindElements(By.TagName("td"));
                        THBigdata2 = row.FindElements(By.TagName("th"));
                        if (TDBigdata2.Count != 0 && !row.Text.Contains("State Category Code"))
                        {
                            AssessmentDetails = TDBigdata2[0].Text + "~" + TDBigdata2[1].Text + "~" + TDBigdata2[2].Text + "~" + TDBigdata2[3].Text + "~" + TDBigdata2[4].Text + "~" + TDBigdata2[5].Text;
                            IWebElement Bigdata = driver.FindElement(By.XPath("//*[@id='valuationdtab']/table[2]/tbody/tr/td/table/tbody/tr/td/table/tbody"));
                            IList<IWebElement> TRBigdata = Bigdata.FindElements(By.TagName("tr"));
                            IList<IWebElement> THBigdata = Bigdata.FindElements(By.TagName("th"));
                            IList<IWebElement> TDBigdata;
                            foreach (IWebElement row1 in TRBigdata)
                            {
                                TDBigdata = row1.FindElements(By.TagName("td"));
                                THBigdata = row1.FindElements(By.TagName("th"));
                                if (j == 0 && TDBigdata.Count != 0 && !row1.Text.Contains("Assessed Value") && row1.Text.Contains("2018"))
                                {
                                    string Assessment = TDBigdata[0].Text + "~" + AssessmentDetails + "~" + TDBigdata[1].Text;

                                    gc.insert_date(orderNumber, parcelNumber, 901, Assessment, 1, DateTime.Now);
                                    j++;
                                }
                                else if (j >= 1 && TDBigdata.Count != 0 && !row1.Text.Contains("Assessed Value") && row1.Text.Contains("2018"))
                                {
                                    string Assessment1 = "" + "~" + AssessmentDetails + "~" + "";

                                    gc.insert_date(orderNumber, parcelNumber, 901, Assessment1, 1, DateTime.Now);
                                }
                            }
                        }
                    }

                    // Tax Search
                    //try
                    //{
                    //    driver.FindElement(By.LinkText("Tax Districts")).SendKeys(Keys.Enter);
                    //    Thread.Sleep(3000);
                    //}
                    //catch { }
                    //try
                    //{
                    //    IWebElement IDistricts = driver.FindElement(By.LinkText("Tax Districts"));
                    //    IJavaScriptExecutor js6 = driver as IJavaScriptExecutor;
                    //    js6.ExecuteScript("arguments[0].click();", IDistricts);
                    //    Thread.Sleep(4000);
                    //}
                    //catch { }
                    IWebElement ITax = driver.FindElement(By.Id("taxdistricts-tab"));
                    IList<IWebElement> ITaxRow = ITax.FindElements(By.TagName("a"));
                    foreach (IWebElement tax in ITaxRow)
                    {
                        if (tax.Text != "" && tax.Text.Contains("Tax Districts"))
                        {
                            tax.Click();
                        }
                    }


                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Districts", driver, "ID", "Ada");
                    //try
                    //{
                    //    driver.FindElement(By.LinkText("Taxes")).SendKeys(Keys.Enter);
                    //    Thread.Sleep(3000);
                    //}
                    //catch { }
                    //try
                    //{
                    //    IWebElement Itaxes = driver.FindElement(By.LinkText("Taxes"));
                    //    IJavaScriptExecutor js7 = driver as IJavaScriptExecutor;
                    //    js7.ExecuteScript("arguments[0].click();", Itaxes);
                    //    Thread.Sleep(4000);
                    //}
                    //catch { }

                    IWebElement ITaxes = driver.FindElement(By.Id("taxes-tab"));
                    IList<IWebElement> ITaxesRow = ITaxes.FindElements(By.TagName("a"));
                    foreach (IWebElement taxes in ITaxesRow)
                    {
                        if (taxes.Text != "" && taxes.Text.Contains("Taxes"))
                        {
                            taxes.Click();
                        }
                    }
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Search ", driver, "ID", "Ada");
                    IWebElement taxdata = driver.FindElement(By.XPath("//*[@id='taxes']/table/tbody"));
                    IList<IWebElement> TRtaxdata = taxdata.FindElements(By.TagName("tr"));
                    IList<IWebElement> THtaxdata = taxdata.FindElements(By.TagName("th"));
                    IList<IWebElement> TDtaxdata;
                    foreach (IWebElement row in TRtaxdata)
                    {
                        TDtaxdata = row.FindElements(By.TagName("td"));
                        THtaxdata = row.FindElements(By.TagName("th"));
                        if (TDtaxdata.Count != 0 && !row.Text.Contains("Taxes Paid"))
                        {
                            string taxdatadetails = TDtaxdata[0].Text + "~" + TDtaxdata[1].Text + "~" + TDtaxdata[2].Text + "~" + TDtaxdata[3].Text + "~" + TDtaxdata[4].Text + "~" + TDtaxdata[5].Text;

                            gc.insert_date(orderNumber, parcelNumber, 904, taxdatadetails, 1, DateTime.Now);
                        }
                        else { }
                    }



                    //Year Bulit
                    //try
                    //{
                    //    driver.FindElement(By.LinkText("Characteristics")).SendKeys(Keys.Enter);
                    //    Thread.Sleep(4000);
                    //}
                    //catch { }
                    //try
                    //{
                    //    IWebElement Iyearbuiltclick = driver.FindElement(By.LinkText("Characteristics"));
                    //    IJavaScriptExecutor js4 = driver as IJavaScriptExecutor;
                    //    js4.ExecuteScript("arguments[0].click();", Iyearbuiltclick);
                    //    Thread.Sleep(4000);
                    //}
                    //catch { }
                    IWebElement IYearBulit = driver.FindElement(By.Id("ctab-tab"));
                    IList<IWebElement> IYearBulitRow = IYearBulit.FindElements(By.TagName("a"));
                    foreach (IWebElement year in IYearBulitRow)
                    {
                        if (year.Text != "" && year.Text.Contains("Characteristics"))
                        {
                            year.Click();
                        }
                    }
                    gc.CreatePdf(orderNumber, parcelNumber, "Property Residential Result", driver, "ID", "Ada");
                    IWebElement ICharacter = driver.FindElement(By.XPath("//*[@id='ctab']/table/tbody"));
                    //try
                    //{

                    //    driver.FindElement(By.LinkText("Residential")).Click();
                    //    Thread.Sleep(1000);
                    //    gc.CreatePdf(orderNumber, parcelNumber, "Year Built", driver, "ID", "Ada");

                    //}
                    //catch { }
                    //try
                    //{
                    //    IWebElement Iresidential = driver.FindElement(By.LinkText("Residential"));
                    //    IJavaScriptExecutor js8 = driver as IJavaScriptExecutor;
                    //    js8.ExecuteScript("arguments[0].click();", Iresidential);
                    //    Thread.Sleep(4000);
                    //    gc.CreatePdf(orderNumber, parcelNumber, "Year Built", driver, "ID", "Ada");
                    //}
                    //catch { }
                    try
                    {
                        IWebElement Iresident = driver.FindElement(By.Id("ctab"));
                        IList<IWebElement> IresidentRow = Iresident.FindElements(By.TagName("a"));
                        foreach (IWebElement resident in IresidentRow)
                        {
                            if (resident.Text != "" && resident.Text.Contains("Residential"))
                            {
                                resident.Click();
                                gc.CreatePdf(orderNumber, parcelNumber, "Property Residential", driver, "ID", "Ada");
                            }
                        }
                    }
                    catch { }
                    //try
                    //{
                    //    driver.FindElement(By.LinkText("Commercial")).Click();
                    //    Thread.Sleep(1000);
                    //    gc.CreatePdf(orderNumber, parcelNumber, "Year Built", driver, "ID", "Ada");
                    //}
                    //catch { }
                    //try
                    //{
                    //    IWebElement Icommercial = driver.FindElement(By.LinkText("Commercial"));
                    //    IJavaScriptExecutor js9 = driver as IJavaScriptExecutor;
                    //    js9.ExecuteScript("arguments[0].click();", Icommercial);
                    //    Thread.Sleep(4000);
                    //    gc.CreatePdf(orderNumber, parcelNumber, "Year Built", driver, "ID", "Ada");
                    //}
                    //catch { }
                    try
                    {
                        IWebElement IComm = driver.FindElement(By.Id("ctab"));
                        IList<IWebElement> ICommRow = IComm.FindElements(By.TagName("a"));
                        foreach (IWebElement Comm in ICommRow)
                        {
                            if (Comm.Text != "" && Comm.Text.Contains("Commercial"))
                            {
                                Comm.Click();

                                gc.CreatePdf(orderNumber, parcelNumber, "Property Commercial Year Built", driver, "ID", "Ada");
                            }
                        }
                    }
                    catch { }
                    try
                    {
                        string bulkdata1 = driver.FindElement(By.XPath("//*[@id='contents']/table/tbody")).Text;
                        IWebElement Iyearbuilt = driver.FindElement(By.XPath("//*[@id='contents']/table/tbody"));
                        IList<IWebElement> IyearRow = Iyearbuilt.FindElements(By.TagName("tr"));
                        IList<IWebElement> Iyeartd;
                        foreach (IWebElement year in IyearRow)
                        {
                            Iyeartd = year.FindElements(By.TagName("td"));
                            if (Iyeartd.Count != 0 && year.Text.Contains("Year Built: "))
                            {
                                Year_built = Iyeartd[1].Text;
                            }
                        }

                    }
                    catch { }
                    string propertydetails = parcel_status + "~" + Owner_Name + "~" + Property_Address + "~" + TaxCodeArea + "~" + Legal_Desc + "~" + Sub_Division + "~" + Town + "~" + Year_built;
                    gc.insert_date(orderNumber, parcelNumber, 900, propertydetails, 1, DateTime.Now);
                    try
                    {
                        var chromeOptions = new ChromeOptions();

                        var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];

                        chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                        chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                        chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                        chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                        var driver1 = new ChromeDriver(chromeOptions);
                        driver1.Navigate().GoToUrl("http://www.adacountyassessor.org/propsys/AddressSearch.jsp");
                        Thread.Sleep(1000);
                        string fileName = DateTime.Now.Date.ToString() + ".pdf";
                        driver1.FindElement(By.XPath("//*[@id='contents']/form/input[1]")).Click();
                        Thread.Sleep(1000);
                        driver1.FindElement(By.XPath("//*[@id='contents']/form/input[3]")).SendKeys(Keys.Enter);
                        Thread.Sleep(1000);
                        driver1.FindElement(By.XPath("//*[@id='landrecordsearch']/ul/li[1]/a")).SendKeys(Keys.Enter);
                        Thread.Sleep(1000);
                        driver1.FindElement(By.XPath("//*[@id='contents']/form/table/tbody/tr[1]/td[2]/input")).SendKeys(parcel_id.Trim());
                        driver1.FindElement(By.XPath("//*[@id='contents']/form/table/tbody/tr[3]/td[2]/input")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        driver1.FindElement(By.XPath("//*[@id='contents']/table/tbody/tr/td[1]/a")).SendKeys(Keys.Enter);
                        // Assessment Notice Download
                        //IWebElement info11 = driver1.FindElement(By.XPath("//*[@id='contents']/table/tbody/tr/td[1]/a"));
                        //string stryrl = info11.GetAttribute("href");
                        //driver1.Navigate().GoToUrl(stryrl);
                        //driver1.FindElement(By.LinkText("Details")).SendKeys(Keys.Enter);
                        //Thread.Sleep(2000);
                        //string textdata = driver1.FindElement(By.XPath("//*[@id='contents']/table/tbody")).Text;
                        //if (textdata.Contains(parcel_id.Trim()))
                        //{
                        //    if (!textdata.Contains("Parcel"))
                        //    {
                        //        driver1.FindElement(By.XPath("//*[@id='contents']/table/tbody/tr/td[1]/a")).SendKeys(Keys.Enter);
                        //        Thread.Sleep(2000);
                        //    }
                        //    else
                        //    {
                        //        IWebElement IAssess = driver1.FindElement(By.XPath("//*[@id='detailstab']/input"));
                        //        IJavaScriptExecutor js12 = driver1 as IJavaScriptExecutor;
                        //        js12.ExecuteScript("arguments[0].click();", IAssess);
                        //        Thread.Sleep(3000);
                        //    }
                        //}
                        //else
                        //{
                        //    IWebElement IAssess = driver1.FindElement(By.XPath("//*[@id='detailstab']/input"));
                        //    IJavaScriptExecutor js12 = driver as IJavaScriptExecutor;
                        //    js12.ExecuteScript("arguments[0].click();", IAssess);
                        //    Thread.Sleep(3000);
                        //}
                        Array.ForEach(Directory.GetFiles(@downloadDirectory), File.Delete); //Delete all the files in auto pdf
                        IWebElement IAssess = driver1.FindElement(By.Id("detailstab"));
                        IList<IWebElement> IAssessRow = IAssess.FindElements(By.TagName("input"));
                        foreach (IWebElement prop in IAssessRow)
                        {
                            string strprop = prop.GetAttribute("value");
                            if (strprop != "" && strprop.Contains("Assessment Notice") && strprop.Contains("View"))
                            {
                                prop.Click();
                                Thread.Sleep(6000);
                            }
                        }

                        string downloadfile = "";
                        // latestfile = gc.getfiles(downloadfile);
                        try
                        {
                            gc.AutoDownloadFile(orderNumber, parcelNumber, "Ada", "ID", latestfile);
                        }
                        catch { }
                        driver1.Quit();
                    }
                    catch { }


                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "ID", "Ada", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "ID", "Ada");
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
        public void ByVisibleElement(IWebElement Element)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].scrollIntoView();", Element);
        }
        //public string getfiles()
        //{
        //    string latestfile1 = "";
        //    // string Path = ConfigurationManager.AppSettings["autopdf"];
        //    var Path = "D:\\AutoPdf\\";
        //    var files = new DirectoryInfo(Path).GetFiles("*.*");
        //    // var files = new DirectoryInfo("D:\\AutoPdf\\").GetFiles("*.*");

        //    DateTime lastupdated = DateTime.MinValue;
        //    foreach (FileInfo file in files)
        //    {
        //        if (file.LastWriteTime > lastupdated)
        //        {
        //            lastupdated = file.LastWriteTime;
        //            latestfile1 = file.Name;
        //        }

        //    }
        //    return latestfile1;
        //}
    }

}
