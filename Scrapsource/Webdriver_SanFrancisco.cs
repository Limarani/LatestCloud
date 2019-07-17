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
using System.Text;
using OpenQA.Selenium.Firefox;

namespace ScrapMaricopa.Scrapsource
{
    public class Webdriver_SanFrancisco
    {
        string outputPath = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();

        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_SanFrancisco(string address, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            string parceno = "-", property_address = "-", year_built = "-";
            string Land_Value = "=", Building_Value = "-", Fixtures = "-", Personal_Property = "-";
            IWebDriver chdriver;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new ChromeDriver();
            //driver = new PhantomJSDriver();
            using (driver = new PhantomJSDriver())
            {
                var option = new ChromeOptions();
                option.AddArgument("No-Sandbox");
                chdriver = new ChromeDriver(option);

                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    if (searchType == "titleflex")
                    {
                        string titleaddress = address;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress.Trim(), "CA", "San Francisco");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_SanFrascisco"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    chdriver.Navigate().GoToUrl("http://sfplanninggis.org/pim/");
                    if (searchType == "address")
                    {

                        //IWebElement iframeElement = driver.FindElement(By.XPath("/html/frameset/frame[1]"));
                        ////now use the switch command
                        //driver.SwitchTo().Frame(iframeElement);
                        //Thread.Sleep(3000);
                        chdriver.FindElement(By.Id("addressInput")).SendKeys(address.ToUpper());
                        Thread.Sleep(10000);
                        IWebElement Iviewtax = chdriver.FindElement(By.XPath("//*[@id='Search-icon']"));
                        IJavaScriptExecutor js = chdriver as IJavaScriptExecutor;
                        js.ExecuteScript("arguments[0].click();", Iviewtax);

                        Thread.Sleep(10000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search", chdriver, "CA", "San Francisco");
                        //   gc.CreatePdf_WOP(orderNumber, "Address Search result", driver, "CA", "San Francisco");
                       
                    }
                    if (searchType == "parcel")
                    {
                        if (HttpContext.Current.Session["titleparcel"] != null)
                        {
                            parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        }
                        if (HttpContext.Current.Session["titleparcel"] != null && (HttpContext.Current.Session["titleparcel"].ToString().Contains(".") || HttpContext.Current.Session["titleparcel"].ToString().Contains("-")))
                        {
                            parcelNumber = HttpContext.Current.Session["titleparcel"].ToString().Replace(".", "");
                        }

                        //1 driver.Navigate().GoToUrl("http://propertymap.sfplanning.org/");
                        //driver.Navigate().GoToUrl("http://sfplanninggis.org/pim/");
                        // IWebElement iframeElement = driver.FindElement(By.XPath("/html/frameset/frame"));
                        //now use the switch command
                        //driver.SwitchTo().Frame(iframeElement);
                        chdriver.FindElement(By.Id("addressInput")).SendKeys(parcelNumber);
                        Thread.Sleep(2000);
                        chdriver.FindElement(By.Id("Search-icon")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search", chdriver, "CA", "San Francisco");
                        try
                        {
                            IWebElement INodata = chdriver.FindElement(By.XPath("/html/body/div[3]"));
                            if (INodata.Text.Contains("does not appear to be a valid"))
                            {
                                HttpContext.Current.Session["Nodata_SanFrascisco"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    try
                    {
                        IWebElement INodata = chdriver.FindElement(By.XPath("//*[@id='Report_DynamicContent_Property']/div[1]/div[3]"));
                        if (INodata.Text.Contains("no parcels at this location"))
                        {
                            HttpContext.Current.Session["Nodata_SanFrascisco"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    //Thread.Sleep(6000);
                    //driver.FindElement(By.Id("propertyButton")).SendKeys(Keys.Enter);
                    //Thread.Sleep(6000);
                    //property details
                    //chdriver.FindElement(By.LinkText("Assessor Summary")).Click();
                    //Thread.Sleep(2000);
                    try
                    {////*[@id="Report_DynamicContent_Property"]/div[1]/div[2]/div/div[2]/div[4]/a[1]
                        chdriver.FindElement(By.XPath("//*[@id='Report_DynamicContent_Property']/div[1]/div[2]/div/div[3]/div[4]/a[1]")).Click();
                        Thread.Sleep(2000);
                    }
                    catch { }
                    chdriver.SwitchTo().Window(chdriver.WindowHandles.Last());
                  
                    //chdriver.FindElement(By.LinkText("Assessor Summary")).Click();
                    try
                    {
                        parceno = chdriver.FindElement(By.XPath("//*[@id='modalContent']/div[1]/div/div[1]/div[2]")).Text.Trim();
                        property_address = chdriver.FindElement(By.XPath("//*[@id='modalContent']/div[1]/div/div[2]/div[2]")).Text;
                        year_built = chdriver.FindElement(By.XPath("//*[@id='modalContent']/div[4]/div/div[8]/div[2]")).Text;
                        string Usetype = chdriver.FindElement(By.XPath("//*[@id='modalContent']/div[4]/div/div[2]/div[4]")).Text;
                        string prop_details = property_address + "~ " + year_built + "~" + Usetype;
                        gc.insert_date(orderNumber, parceno, 72, prop_details, 1, DateTime.Now);
                    }
                    catch { }
                    try
                    {
                        //Assessment Details
                        Land_Value = chdriver.FindElement(By.XPath("//*[@id='modalContent']/div[4]/div/div[2]/div[2]")).Text;
                        Building_Value = chdriver.FindElement(By.XPath("//*[@id='modalContent']/div[4]/div/div[3]/div[2]")).Text;
                        Fixtures = chdriver.FindElement(By.XPath("//*[@id='modalContent']/div[4]/div/div[4]/div[2]")).Text;
                        Personal_Property = chdriver.FindElement(By.XPath("//*[@id='modalContent']/div[4]/div/div[5]/div[2]")).Text;

                        string ass_details = Land_Value + "~ " + Building_Value + "~ " + Fixtures + "~ " + Personal_Property;
                        gc.insert_date(orderNumber, parceno, 73, ass_details, 1, DateTime.Now);
                        gc.CreatePdf_WOP(orderNumber, "Property details", chdriver, "CA", "San Francisco");
                        AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    }
                    catch { }
                    chdriver.Quit();
                    //download bill
                    //driver.FindElement(By.ClassName("NoPrint")).SendKeys(Keys.Enter);
                    //driver.SwitchTo().Window(driver.WindowHandles.Last());
                    //Thread.Sleep(5000);
                    //IWebElement element = driver.FindElement(By.XPath("//*[@id='mapPrintOptions']/tbody/tr[1]/td[2]/button"));
                    //Actions actions = new Actions(driver);
                    //actions.MoveToElement(element).Click().Perform();
                    //Thread.Sleep(2000);
                    //string billurl = driver.Url;


                    //  CreatePdf_WOP(orderNumber, "Bill pdf");

                    //Tax Details
                    driver.Navigate().GoToUrl("https://ttxonlineportal.sfgov.org/content/San-Francisco-Forms-Portal/Residents/propertyTaxAndLicense.html");
                    // driver.Navigate().GoToUrl("c");
                    string block_num = "";
                    string lot_num = "";
                    if (parceno.Count() == 8)
                    {
                        block_num = parceno.Substring(0, 4);
                        lot_num = parceno.Substring(4, 3);
                    }
                    else if (parceno.Count() == 8)
                    {
                        block_num = parceno.Substring(0, 4);
                        lot_num = parceno.Substring(4, 3);
                    }
                    else if (parceno.Count() == 7)
                    {
                        block_num = parceno.Substring(0, 4);
                        lot_num = parceno.Substring(4, 3);
                    }

                    if (block_num.Trim() == "" && lot_num.Trim() == "")
                    {
                        string[] splitno = parceno.Split('-');
                        block_num = splitno[0];
                        lot_num = splitno[1];
                        parceno = parcelNumber;
                    }

                    driver.FindElement(By.Id("addressBlockNumber")).SendKeys(block_num);
                    driver.FindElement(By.Id("addressLotNumber")).SendKeys(lot_num);
                    gc.CreatePdf_WOP(orderNumber, "Tax details", driver, "CA", "San Francisco");
                    driver.FindElement(By.Id("submit")).SendKeys(Keys.Enter);
                    gc.CreatePdf_WOP(orderNumber, "Tax Details result", driver, "CA", "San Francisco");
                    Thread.Sleep(2000);
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='block-system-main']/div/div/article/div/div/div/div[3]/div/div/div[1]/ul/li[2]/a")).Click();
                        Thread.Sleep(3000);
                    }
                    catch { }
                    //string paymenthistory
                    //Bill_Type~Tax_Year~Installment~Bill~Total_Paid~Paid_Date
                    IWebElement TBpayment_history = driver.FindElement(By.XPath("//*[@id='priorYearTaxTable']/tbody"));
                    IList<IWebElement> TRpayment_history = TBpayment_history.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDpayment_history;

                    foreach (IWebElement row2 in TRpayment_history)
                    {
                        if (!row2.Text.Contains("There are no properties that match your search criteria"))
                        {
                            TDpayment_history = row2.FindElements(By.TagName("td"));
                            string paymenthistory = TDpayment_history[0].GetAttribute("innerText") + "~ " + TDpayment_history[1].GetAttribute("innerText") + "~ " + TDpayment_history[2].GetAttribute("innerText") + "~ " + TDpayment_history[4].GetAttribute("innerText") + "~ " + TDpayment_history[5].GetAttribute("innerText") + "~ " + TDpayment_history[6].GetAttribute("innerText");
                            gc.insert_date(orderNumber, parceno, 76, paymenthistory, 1, DateTime.Now);
                        }
                    }
                    //   gc.CreatePdf_WOP(orderNumber, "Payment History Details", driver, "CA", "San Francisco");

                    //Current property tax statements
                    string text = "";
                    string Property_Address = "-", Bill_Type = "-", Bill_Number = "-", Installment = "-", Due_Date = "-", Amount_Due = "-", Total_Due = "-";
                    driver.FindElement(By.XPath("//*[@id='block-system-main']/div/div/article/div/div/div/div[3]/div/div/div[1]/ul/li[1]/a")).SendKeys(Keys.Enter);
                    Thread.Sleep(3000);
                    IList<IWebElement> trc = driver.FindElements(By.XPath("//*[@id='taxTable']/tbody/tr"));
                    int trcount = trc.Count();
                    if (trcount == 1)
                    {

                        try
                        {
                            IWebElement tr1 = driver.FindElement(By.XPath(" //*[@id='taxTable']/tbody/tr/td"));
                            text = driver.FindElement(By.XPath("//*[@id='taxTable']/tbody/tr/td")).Text;
                        }

                        catch { }
                        if (!text.Contains("There are no current property tax statements due at this time."))
                        {
                            try
                            {
                                Property_Address = driver.FindElement(By.XPath("//*[@id='taxTable']/tbody/tr/td[1]/div/div/h5[1]")).Text;
                                Bill_Type = driver.FindElement(By.XPath("//*[@id='taxTable']/tbody/tr/td[1]/div/div/h4")).Text;
                                Bill_Number = driver.FindElement(By.XPath("//*[@id='taxTable']/tbody/tr/td[2]/strong")).Text;
                                Installment = driver.FindElement(By.XPath("//*[@id='taxTable']/tbody/tr/td[3]/strong")).Text;
                                Due_Date = driver.FindElement(By.XPath("//*[@id='taxTable']/tbody/tr/td[4]/strong")).Text;
                                Amount_Due = driver.FindElement(By.XPath("//*[@id='taxTable']/tbody/tr/td[5]/strong")).Text;
                                Total_Due = driver.FindElement(By.XPath("//*[@id='taxTable']/tbody/tr/td[6]/strong")).Text;

                                string current_tax = Property_Address + "~ " + Bill_Type + "~ " + Bill_Number + "~ " + Installment + "~ " + Due_Date + "~ " + Amount_Due + "~ " + Total_Due;
                                gc.insert_date(orderNumber, parceno, 78, current_tax, 1, DateTime.Now);

                            }
                            catch { }
                            String Parent_Window = driver.CurrentWindowHandle;

                            IWebElement view = driver.FindElement(By.XPath("//*[@id='taxTable']/tbody/tr/td[1]/div/div/a/button"));
                            view.SendKeys(Keys.Enter);

                            try
                            {
                                driver.SwitchTo().Window(driver.WindowHandles.Last());
                                string url = driver.Url;
                                string billpdf = outputPath + Bill_Number + "currenttax_bill.pdf";
                                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                                WebClient downloadpdf = new WebClient();
                                downloadpdf.DownloadFile(url, billpdf);
                            }
                            catch { }

                            //parent window
                            driver.SwitchTo().Window(Parent_Window);

                            string paybutton = driver.FindElement(By.XPath("//*[@id='taxTable']/tbody/tr/td[7]/button")).Text.Trim();
                            paybutton += paybutton + " ";

                            if (paybutton.Trim().Contains("Special Handling"))
                            {
                                HttpContext.Current.Session["SpecialHandling_SanFrancisco"] = "Yes";
                            }

                        }

                        gc.CreatePdf_WOP(orderNumber, "Current Property Tax Statements", driver, "CA", "San Francisco");
                    }

                    else
                    {
                        for (int i = 1; i <= trcount; i++)
                        {
                            try
                            {
                                Property_Address = driver.FindElement(By.XPath("//*[@id='taxTable']/tbody/tr[" + i + "]/td[1]/div/div/h5[1]")).Text;
                                Bill_Type = driver.FindElement(By.XPath("//*[@id='taxTable']/tbody/tr[" + i + "]/td[1]/div/div/h4")).Text;
                                Bill_Number = driver.FindElement(By.XPath("//*[@id='taxTable']/tbody/tr[" + i + "]/td[2]/strong")).Text;
                                Installment = driver.FindElement(By.XPath("//*[@id='taxTable']/tbody/tr[" + i + "]/td[3]/strong")).Text;
                                Due_Date = driver.FindElement(By.XPath("//*[@id='taxTable']/tbody/tr[" + i + "]/td[4]/strong")).Text;
                                Amount_Due = driver.FindElement(By.XPath("//*[@id='taxTable']/tbody/tr[" + i + "]/td[5]/strong")).Text;
                                Total_Due = driver.FindElement(By.XPath("//*[@id='taxTable']/tbody/tr[" + i + "]/td[6]/strong")).Text;

                                string current_tax = Property_Address + "~ " + Bill_Type + "~ " + Bill_Number + "~ " + Installment + "~ " + Due_Date + "~ " + Amount_Due + "~ " + Total_Due;
                                gc.insert_date(orderNumber, parceno, 78, current_tax, 1, DateTime.Now);
                            }
                            catch { }
                            String Parent_Window = driver.CurrentWindowHandle;

                            IWebElement view = driver.FindElement(By.XPath("//*[@id='taxTable']/tbody/tr[" + i + "]/td[1]/div/div/a/button"));

                            view.SendKeys(Keys.Enter);
                            try
                            {
                                driver.SwitchTo().Window(driver.WindowHandles.Last());
                                string url = driver.Url;
                                string billpdf = outputPath + Bill_Number + "Propertycurrenttax_bill.pdf";
                                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                                WebClient downloadpdf = new WebClient();
                                downloadpdf.DownloadFile(url, billpdf);

                            }
                            catch (Exception e) { }
                            //parent window
                            driver.SwitchTo().Window(Parent_Window);

                            string paybutton = driver.FindElement(By.XPath("//*[@id='taxTable']/tbody/tr[" + i + "]/td[7]/button")).Text.Trim();
                            paybutton += paybutton + " ";

                            if (paybutton.Trim().Contains("Special Handling"))
                            {
                                HttpContext.Current.Session["SpecialHandling_SanFrancisco"] = "Yes";
                            }
                        }
                    }
                    gc.CreatePdf_WOP(orderNumber, "Current Property Tax Statements", driver, "CA", "San Francisco");
                    driver.FindElement(By.XPath("//*[@id='block-system-main']/div/div/article/div/div/div/div[3]/div/div/div[1]/ul/li[2]/a")).Click();
                    Thread.Sleep(3000);
                    gc.CreatePdf_WOP(orderNumber, "Payment History Details", driver, "CA", "San Francisco");

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "CA", "San Francisco", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);


                    driver.Quit();
                    HttpContext.Current.Session["titleparcel"] = null;
                    gc.mergpdf(orderNumber, "CA", "San Francisco");
                    return "Data Inserted Successfully";
                }
                catch (Exception ex)
                {
                    driver.Quit();
                    chdriver.Quit();
                    GlobalClass.LogError(ex, orderNumber);
                    throw ex;
                }

            }
        }

        public void CreatePdf(string orderno, string parcelno, string pdfName)
        {
            string outputPath = "";
            MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString);
            string query = "SELECT Storage_Path FROM state_county_master where State_Name = '" + GlobalClass.sname + "' and County_Name='" + GlobalClass.cname + "'";
            MySqlCommand cmd = new MySqlCommand(query, con);
            con.Open();
            MySqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                outputPath = dr["Storage_Path"].ToString();
            }
            dr.Close();
            con.Close();
            outputPath = outputPath + orderno + "\\" + parcelno + "\\";
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            string img = outputPath + pdfName + ".png";
            string pdf = outputPath + pdfName + ".pdf";

            //driver.Manage().Window.Maximize();
            // driver.TakeScreenshot().SaveAsFile(img, ScreenshotImageFormat.Png);
            ITakesScreenshot screenshotDriver = driver as ITakesScreenshot;
            Screenshot screenshot = screenshotDriver.GetScreenshot();
            screenshot.SaveAsFile(img, ScreenshotImageFormat.Png);

            WebDriverTest.ConvertImageToPdf(img, pdf);
            if (File.Exists(img))
            {
                File.Delete(img);
            }

        }

        public void CreatePdf_WOP(string orderno, string pdfName)
        {
            string outputPath = "";
            MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString);
            string query = "SELECT Storage_Path FROM state_county_master where State_Name = '" + GlobalClass.sname + "' and County_Name='" + GlobalClass.cname + "'";
            MySqlCommand cmd = new MySqlCommand(query, con);
            con.Open();
            MySqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                outputPath = dr["Storage_Path"].ToString();
            }
            dr.Close();
            con.Close();
            outputPath = outputPath + orderno + "\\";
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            string img = outputPath + pdfName + ".png";
            string pdf = outputPath + pdfName + ".pdf";

            //  driver.Manage().Window.Maximize();
            //driver.TakeScreenshot().SaveAsFile(img, ScreenshotImageFormat.Png);
            ITakesScreenshot screenshotDriver = driver as ITakesScreenshot;
            Screenshot screenshot = screenshotDriver.GetScreenshot();
            screenshot.SaveAsFile(img, ScreenshotImageFormat.Png);

            WebDriverTest.ConvertImageToPdf(img, pdf);
            if (File.Exists(img))
            {
                File.Delete(img);
            }

        }

    }
}