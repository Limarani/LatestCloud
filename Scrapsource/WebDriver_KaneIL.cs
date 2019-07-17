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
using OpenQA.Selenium.Firefox;
using System.Diagnostics;
using OpenQA.Selenium.Remote;

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_KaneIL
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_KaneIL(string houseno, string direction, string sname, string sttype, string unitno, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            GlobalClass.sname = "IL";
            GlobalClass.cname = "Kane";
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "", filename = "";
            List<string> URL = new List<string>();
            List<string> multi = new List<string>();
            string outputparcel = "", strURL = "", Taxsold = "", TA = "", address = "", outputparcelwoh = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            if (direction != "")
            {
                address = houseno + " " + direction + " " + sname + " " + sttype;
            }
            else
            {
                address = houseno + " " + sname + " " + sttype;
            }

            using (driver = new PhantomJSDriver())
            {
                StartTime = DateTime.Now.ToString("HH:mm:ss");
                try
                {
                    if (searchType == "titleflex")
                    {
                        string titleaddress = houseno + " " + direction + " " + sname + " " + sttype + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "IL", "Kane");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_kaneIL"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    //tax authority
                    driver.Navigate().GoToUrl("http://www.kanecountytreasurer.org/");
                    gc.CreatePdf_WOP(orderNumber, "Tax Authority", driver, "IL", "Kane");
                    string TAaddress = driver.FindElement(By.XPath("/html/body/div/div/table/tbody/tr[1]/td[3]/p[4]")).Text.Replace("\r\n", ",");
                    string taphone = driver.FindElement(By.XPath("/html/body/div/div/table/tbody/tr[1]/td[3]/p[5]")).Text.Replace("\r\n", ",");
                    TA = TAaddress + "," + taphone;

                    driver.Navigate().GoToUrl("http://kaneil.devnetwedge.com/");
                    if (searchType == "address")
                    {
                        driver.FindElement(By.Id("house-number")).SendKeys(houseno);
                        driver.FindElement(By.Id("street-name")).SendKeys(sname);
                        driver.FindElement(By.Id("street-suffix")).SendKeys(sttype);
                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "IL", "Kane");
                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.Id("property-key")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "IL", "Kane");
                    }

                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("owner-name")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "Ownername Search", driver, "IL", "Kane");
                    }
                    driver.FindElement(By.XPath("/html/body/div[2]/div[4]/div/form/div[2]/button[1]")).Click();
                    Thread.Sleep(3000);
                    string SiteAddress = "", OwnerNamemail = "", legdesc = "", ptaxyear = "", propeclass = "", taxcode = "", taxstatus = "", nettaxvalue = "", taxrate = "", totaltax = "";
                    gc.CreatePdf_WOP(orderNumber, "Search Results", driver, "IL", "Kane");

                    //No Data

                    try
                    {
                        string nodata = driver.FindElement(By.Id("search-results")).Text;
                        if (nodata.Contains("No data available in table"))
                        {
                            HttpContext.Current.Session["Nodata_kaneIL"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    //multiparcel
                    try
                    {
                        string entrycount = driver.FindElement(By.Id("search-results_info")).Text;
                        //Showing 1 to 2 of 2 entries
                        entrycount = gc.Between(entrycount, "1 to ", " of ");
                        int count = Int32.Parse(entrycount);
                        if (count > 1 && count <= 25)
                        {
                            HttpContext.Current.Session["multiParcel_kaneIL"] = "Yes";
                            //Select address from list....
                            IWebElement multiaddrtableElement = driver.FindElement(By.XPath("//*[@id='search-results']/tbody"));
                            IList<IWebElement> multiaddrtableRow = multiaddrtableElement.FindElements(By.TagName("tr"));
                            IList<IWebElement> multiaddrrowTD;
                            int j = 0;
                            foreach (IWebElement row in multiaddrtableRow)
                            {
                                multiaddrrowTD = row.FindElements(By.TagName("td"));
                                if (multiaddrrowTD.Count != 0 && row.Text.Contains(address.ToUpper().TrimEnd()))
                                {
                                    if (j < 26)
                                    {
                                        try
                                        {
                                            //Year~Owner~Address~Type
                                            string multiowner = multiaddrrowTD[0].Text + "~" + multiaddrrowTD[2].Text + "~" + multiaddrrowTD[3].Text + "~" + multiaddrrowTD[4].Text;
                                            gc.insert_date(orderNumber, multiaddrrowTD[1].Text.Trim(), 1749, multiowner, 1, DateTime.Now);

                                        }
                                        catch
                                        {
                                        }
                                        j++;
                                    }

                                }
                            }

                        }

                        else
                        {
                            HttpContext.Current.Session["multiParcel_kaneIL_Multicount"] = "Maximum";
                            return "Maximum";
                        }
                        driver.Quit();
                        return "MutliParcel";
                    }
                    catch { }

                    int curryear = DateTime.Now.Year;
                    int s = 0;
                    for (int year = curryear; year >= curryear - 7; year--)
                    {
                        //check billing table 
                        string uparcel = driver.FindElement(By.XPath("//*[@id='parcel-views']/div[2]/table/tbody/tr[1]/td[1]/div[2]")).Text.Replace("-", "").Trim();
                        string taxurl = "http://kaneil.devnetwedge.com/parcel/view/" + uparcel + "/" + year;
                        driver.Navigate().GoToUrl(taxurl);
                        string nobill = driver.FindElement(By.XPath("/html/body")).Text;
                        string profulltext = "";
                        outputparcelwoh = uparcel.Replace("-", "").Trim();
                        if (!nobill.Contains("No Billing Information"))
                        {
                            if (s < 3)
                            {
                                profulltext = driver.FindElement(By.XPath("//*[@id='parcel-views']/div[2]/table/tbody")).Text.Replace("\r\n", " ");
                                outputparcel = gc.Between(profulltext, "Parcel Number", "Site Address");
                                gc.CreatePdf(orderNumber, outputparcelwoh, "Tax History Details" + " " + year, driver, "IL", "Kane");
                            }

                            if (s == 0)
                            {

                                SiteAddress = gc.Between(profulltext, "Site Address", "Owner Name & Address").Trim();
                                OwnerNamemail = gc.Between(profulltext, "Owner Name & Address", "Tax Year").Trim();
                                ptaxyear = gc.Between(profulltext, "Tax Year", "Toggle Dropdown").Trim();
                                propeclass = gc.Between(profulltext, "Property Class", "Tax Code").Trim();
                                taxcode = gc.Between(profulltext, "Tax Code", "Tax Status").Trim();
                                taxstatus = gc.Between(profulltext, "Tax Status", "Net Taxable Value").Trim();
                                nettaxvalue = gc.Between(profulltext, "Net Taxable Value", "Tax Rate").Trim();
                                taxrate = gc.Between(profulltext, "Tax Rate", "Print Tax Bill").Trim();
                                totaltax = gc.Between(profulltext, "Total Tax", "Township").Trim();
                                legdesc = GlobalClass.After(profulltext, "Legal Description").Trim();
                                //Parcel Number~Site Address~Owner Name & Address~Legal Description~Tax Year~Property Class~Tax Code~Tax Status~Net Taxable Value~Tax Rate~Total Tax
                                string prodet = outputparcel + "~" + SiteAddress + "~" + OwnerNamemail + "~" + legdesc + "~" + ptaxyear + "~" + propeclass + "~" + taxcode + "~" + taxstatus + "~" + nettaxvalue + "~" + taxrate + "~" + totaltax;
                                gc.insert_date(orderNumber, outputparcel, 1732, prodet, 1, DateTime.Now);

                                //assessment details

                                IWebElement assTable = driver.FindElement(By.XPath("//*[@id='parcel-views']/div[6]/table/tbody"));
                                IList<IWebElement> assTableRow = assTable.FindElements(By.TagName("tr"));
                                IList<IWebElement> assTableRowTD;
                                foreach (IWebElement Role in assTableRow)
                                {
                                    assTableRowTD = Role.FindElements(By.TagName("td"));
                                    if (assTableRowTD.Count != 0)
                                    {
                                        //Level~Homesite~Dwelling~Farm Land~Farm Building~Mineral~Total
                                        string assdet = assTableRowTD[0].Text + "~" + assTableRowTD[1].Text + "~" + assTableRowTD[2].Text + "~" + assTableRowTD[3].Text + "~" + assTableRowTD[4].Text + "~" + assTableRowTD[5].Text + "~" + assTableRowTD[6].Text;
                                        gc.insert_date(orderNumber, outputparcel, 1733, assdet, 1, DateTime.Now);
                                    }
                                }

                                //exemption details
                                try
                                {
                                    IWebElement exemTable = driver.FindElement(By.XPath("//*[@id='parcel-views']/div[7]/table/tbody"));
                                    IList<IWebElement> exemTableRow = exemTable.FindElements(By.TagName("tr"));
                                    IList<IWebElement> exemTableRowTD;
                                    foreach (IWebElement Role in exemTableRow)
                                    {
                                        exemTableRowTD = Role.FindElements(By.TagName("td"));
                                        if (exemTableRowTD.Count != 0)
                                        {
                                            //Exemption Type~Requested Date~Granted Date~Renewal Date~Prorate Date~Requested Amount~Granted Amount
                                            string exemdet = exemTableRowTD[0].Text + "~" + exemTableRowTD[1].Text + "~" + exemTableRowTD[2].Text + "~" + exemTableRowTD[3].Text + "~" + exemTableRowTD[4].Text + "~" + exemTableRowTD[5].Text + "~" + exemTableRowTD[6].Text;
                                            gc.insert_date(orderNumber, outputparcel, 1734, exemdet, 1, DateTime.Now);
                                        }
                                    }
                                }
                                catch { }
                                AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                                //Taxing Bodies
                                IWebElement tbodTable = driver.FindElement(By.XPath("//*[@id='taxing-bodies-table']/tbody"));
                                IList<IWebElement> tbodTableRow = tbodTable.FindElements(By.TagName("tr"));
                                IList<IWebElement> tbodTableRowTD;
                                foreach (IWebElement Role in tbodTableRow)
                                {
                                    tbodTableRowTD = Role.FindElements(By.TagName("td"));
                                    if (tbodTableRowTD.Count != 0)
                                    {
                                        //District~Tax Rate~Extension
                                        string boddet = tbodTableRowTD[0].Text + "~" + tbodTableRowTD[1].Text + "~" + tbodTableRowTD[2].Text;
                                        gc.insert_date(orderNumber, outputparcel, 1736, boddet, 1, DateTime.Now);
                                    }
                                }

                                //Tax History
                                string hxpath = "";
                                try
                                {
                                    string TTaxsold = driver.FindElement(By.XPath("//*[@id='parcel-views']/div[5]/div[2]")).Text;
                                    if (TTaxsold.Contains("CONTACT"))
                                    {
                                        Taxsold = TTaxsold;
                                    }
                                }
                                catch { }
                                try
                                {
                                    driver.FindElement(By.XPath("//*[@id='parcel-views']/div[5]/div[3]/table/tbody[3]/tr[2]/td/button")).Click();
                                    hxpath = "//*[@id='parcel-views']/div[5]/div[3]/table";
                                }
                                catch { }
                                try
                                {
                                    driver.FindElement(By.XPath("//*[@id='parcel-views']/div[5]/div[2]/table/tbody[3]/tr[2]/td/button")).Click();
                                    hxpath = "//*[@id='parcel-views']/div[5]/div[2]/table";
                                }
                                catch { }

                                IWebElement hisTable = driver.FindElement(By.XPath(hxpath));
                                IList<IWebElement> hisTableRow = hisTable.FindElements(By.TagName("tr"));
                                IList<IWebElement> hisTableRowTD;
                                string tax = "";
                                foreach (IWebElement Role in hisTableRow)
                                {
                                    hisTableRowTD = Role.FindElements(By.TagName("td"));
                                    if (hisTableRowTD.Count != 0)
                                    {
                                        if (hisTableRowTD.Count == 3)
                                        {
                                            tax = hisTableRowTD[0].Text + "~" + hisTableRowTD[1].Text + "~" + hisTableRowTD[2].Text + "~" + "" + "~" + Taxsold;
                                            gc.insert_date(orderNumber, outputparcel, 1737, tax, 1, DateTime.Now);
                                        }
                                        if (hisTableRowTD.Count == 4)
                                        {
                                            //Tax Year~Total Due~Total Paid~Amount Unpaid
                                            tax = hisTableRowTD[0].Text + "~" + hisTableRowTD[1].Text + "~" + hisTableRowTD[2].Text + "~" + hisTableRowTD[3].Text + "~" + "";
                                            gc.insert_date(orderNumber, outputparcel, 1737, tax, 1, DateTime.Now);
                                        }
                                    }
                                }

                                //Redemptions 
                                try
                                {
                                    IWebElement redeTable = driver.FindElement(By.XPath("//*[@id='parcel-views']/div[9]/table/tbody"));
                                    IList<IWebElement> redeTableRow = redeTable.FindElements(By.TagName("tr"));
                                    IList<IWebElement> redeTableRowTD;
                                    foreach (IWebElement Role in redeTableRow)
                                    {
                                        redeTableRowTD = Role.FindElements(By.TagName("td"));
                                        if (redeTableRowTD.Count != 0)
                                        {
                                            //Year~Certificate~Type~Date Sold~Sale Status~Status Date~Penalty Date
                                            string rededet = redeTableRowTD[0].Text + "~" + redeTableRowTD[1].Text + "~" + redeTableRowTD[2].Text + "~" + redeTableRowTD[3].Text + "~" + redeTableRowTD[4].Text + "~" + redeTableRowTD[5].Text + "~" + redeTableRowTD[6].Text;
                                            gc.insert_date(orderNumber, outputparcel, 1741, rededet, 1, DateTime.Now);
                                        }

                                    }
                                }
                                catch
                                {

                                }
                                //Get bill URL
                                IWebElement Iurl = driver.FindElement(By.XPath("//*[@id='parcel-views']/div[2]/table/tbody/tr[5]/td[3]/div[1]/a"));
                                strURL = Iurl.GetAttribute("href");
                                // URL.Add(strURL);


                                //bill download
                                filename = "TaxBill";
                                var chromeOptions = new ChromeOptions();
                                var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];
                                chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                                chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                                chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                                chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                                var chDriver = new ChromeDriver(chromeOptions);
                                try
                                {
                                    chDriver.Navigate().GoToUrl(strURL);
                                    Thread.Sleep(5000);
                                    // Array.ForEach(Directory.GetFiles(@downloadDirectory), File.Delete);
                                    gc.AutoDownloadFile(orderNumber, outputparcelwoh, "Kane", "IL", filename + ".pdf");
                                    // gc.AutoDownloadFileSpokane(orderNumber, outputparcel, "Kane", "IL", filename+".pdf");
                                    chDriver.Quit();
                                    strURL = "";
                                }
                                catch (Exception e)
                                {
                                    chDriver.Quit();
                                }

                            }
                            s++;

                            if (s < 4)
                            {
                                //billing table
                                IWebElement billTable = driver.FindElement(By.XPath("//*[@id='parcel-views']/div[4]/table/tbody"));
                                IList<IWebElement> billTableRow = billTable.FindElements(By.TagName("tr"));
                                IList<IWebElement> billTableRowTD;
                                foreach (IWebElement Role in billTableRow)
                                {
                                    billTableRowTD = Role.FindElements(By.TagName("td"));
                                    if (billTableRowTD.Count != 0)
                                    {
                                        string billdet = billTableRowTD[0].Text + "~" + billTableRowTD[1].Text + "~" + billTableRowTD[2].Text + "~" + billTableRowTD[3].Text + "~" + billTableRowTD[4].Text + "~" + billTableRowTD[5].Text + "~" + billTableRowTD[6].Text + "~" + billTableRowTD[7].Text + "~" + billTableRowTD[8].Text + "~" + billTableRowTD[9].Text + "~" + TA;
                                        gc.insert_date(orderNumber, outputparcel, 1735, billdet, 1, DateTime.Now);
                                    }

                                }

                            }
                        }
                    }


                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "IL", "Kane", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    gc.mergpdf(orderNumber, "IL", "Kane");
                    return "Data Inserted Successfully";
                }
                catch (Exception ex1)
                {
                    driver.Quit();

                    throw ex1;
                }
            }
        }
    }
}