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
    public class WebDriver_MCHenryIL
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_MCHenryIL(string houseno, string direction, string sname, string sttype, string unitno, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            GlobalClass.sname = "IL";
            GlobalClass.cname = "McHenry";
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "", filename = "";
            List<string> URL = new List<string>();
            List<string> MainURL = new List<string>();
            List<string> multi = new List<string>();
            List<string> option = new List<string>();

            string outputparcel = "", strURL = "", Taxsold = "", TA = "", address = "";
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
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "IL", "McHenry");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_McHenryIL"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        searchType = "parcel";
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString().Replace("-","");
                    }
                    try
                    {
                        //tax authority                                                   
                        driver.Navigate().GoToUrl("https://www.mchenrycountyil.gov/county-government/departments-j-z/treasurer");
                        gc.CreatePdf_WOP(orderNumber, "Tax Authority", driver, "IL", "McHenry");
                        string TAaddress = driver.FindElement(By.XPath("//*[@id='widget_67_2175_1007']/table/tbody/tr/td[1]/h3/span")).Text.Replace("\r\n", ",");
                        string taphone = driver.FindElement(By.XPath("//*[@id='widget_67_2175_1007']/table/tbody/tr/td[1]/p[2]")).Text.Replace("\r\n", ",");
                        TA = TAaddress + "," + taphone;
                    }
                    catch { }

                    driver.Navigate().GoToUrl("http://mchenryil.devnetwedge.com/");
                    if (searchType == "address")
                    {
                        driver.FindElement(By.Id("house-number-min")).SendKeys(houseno);
                        driver.FindElement(By.Id("house-number-max")).SendKeys(houseno);
                        driver.FindElement(By.Id("street-name")).SendKeys(sname);
                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "IL", "McHenry");
                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.Id("property-key")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "IL", "McHenry");
                    }

                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("owner-name")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "Ownername Search", driver, "IL", "McHenry");
                    }
                    driver.FindElement(By.XPath("/html/body/div[2]/form/div[2]/button[1]")).Click();
                    Thread.Sleep(3000);
                    string SiteAddress = "", OwnerNamemail = "", legdesc = "", ptaxyear = "", propeclass = "", taxcode = "", taxstatus = "", nettaxvalue = "", taxrate = "", totaltax = "", acres = "";
                    gc.CreatePdf_WOP(orderNumber, "Search Results", driver, "IL", "McHenry");

                    //No Data

                    try
                    {
                        string nodata = driver.FindElement(By.Id("search-results")).Text;
                        if (nodata.Contains("No data available in table"))
                        {
                            HttpContext.Current.Session["Nodata_McHenryIL"] = "Yes";
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
                            HttpContext.Current.Session["multiParcel_McHenry"] = "Yes";
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
                                            gc.insert_date(orderNumber, multiaddrrowTD[1].Text.Trim(), 1791, multiowner, 1, DateTime.Now);

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
                            HttpContext.Current.Session["multiParcel_McHenryIL_Multicount"] = "Maximum";
                            return "Maximum";
                        }
                        driver.Quit();
                        return "MutliParcel";
                    }
                    catch { }

                    int curryear = DateTime.Now.Year;
                    int s = 0;

                    IWebElement SelectOption = driver.FindElement(By.XPath("//*[@id='parcel-views']/div[3]/table/tbody/tr[2]/td/div[2]/div[2]/ul"));
                    IList<IWebElement> Select = SelectOption.FindElements(By.TagName("a"));
                    int Check = 0;
                    string murl = "";
                    foreach (IWebElement a in Select)
                    {

                        if (Check <= 3)
                        {
                            murl = a.GetAttribute("href").ToString();
                            option.Add(murl);
                            //MainURL.Add(murl);

                        }
                        Check++;
                    }

                    foreach (string item in option)
                    {
                        //check billing table 
                        string profulltext = "";
                        driver.Navigate().GoToUrl(item);
                        string nobill = driver.FindElement(By.XPath("/html/body")).Text;
                        if (!nobill.Contains("No Billing Information"))
                        {
                            if (s < 3)
                            {
                                profulltext = driver.FindElement(By.XPath("//*[@id='parcel-views']/div[3]/table/tbody")).Text.Replace("\r\n", " ");
                                outputparcel = gc.Between(profulltext, "Parcel Number", "Site Address").Trim();
                                gc.CreatePdf(orderNumber, outputparcel, "Tax History Details" + " " + s, driver, "IL", "McHenry");
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
                                taxrate = gc.Between(profulltext, "Tax Rate", "Tax Bill").Trim();
                                totaltax = gc.Between(profulltext, "Total Tax", "Township").Trim();
                                legdesc = GlobalClass.After(profulltext, "Legal Description").Trim();
                                acres = gc.Between(profulltext, "Acres", "Mailing Address").Trim();
                                //Parcel Number~Site Address~Owner Name & Address~Legal Description~Tax Year~Property Class~Tax Code~Tax Status~Net Taxable Value~Tax Rate~Total Tax
                                string prodet = outputparcel + "~" + SiteAddress + "~" + OwnerNamemail + "~" + legdesc + "~" + ptaxyear + "~" + propeclass + "~" + taxcode + "~" + taxstatus + "~" + nettaxvalue + "~" + taxrate + "~" + totaltax + "~" + acres;
                                gc.insert_date(orderNumber, outputparcel, 1784, prodet, 1, DateTime.Now);

                                //assessment details

                                IWebElement assTable = driver.FindElement(By.XPath("//*[@id='parcel-views']/div[5]/table/tbody"));
                                IList<IWebElement> assTableRow = assTable.FindElements(By.TagName("tr"));
                                IList<IWebElement> assTableRowTD;
                                foreach (IWebElement Role in assTableRow)
                                {
                                    assTableRowTD = Role.FindElements(By.TagName("td"));
                                    if (assTableRowTD.Count != 0)
                                    {
                                        //Level~Homesite~Dwelling~Farm Land~Farm Building~Mineral~Total
                                        string assdet = assTableRowTD[0].Text + "~" + assTableRowTD[1].Text + "~" + assTableRowTD[2].Text + "~" + assTableRowTD[3].Text + "~" + assTableRowTD[4].Text + "~" + assTableRowTD[5].Text + "~" + assTableRowTD[6].Text;
                                        gc.insert_date(orderNumber, outputparcel, 1785, assdet, 1, DateTime.Now);
                                    }
                                }

                                //exemption details
                                try
                                {
                                    IList<IWebElement> tablee = driver.FindElements(By.XPath("//*[@id='parcel-views']/div/table"));
                                    int counte = tablee.Count;
                                    foreach (IWebElement tab in tablee)
                                    {
                                        if (tab.Text.Contains("Exemption Type"))
                                        {
                                            IList<IWebElement> exemTableRow = tab.FindElements(By.TagName("tr"));
                                            IList<IWebElement> exemTableRowTD;
                                            foreach (IWebElement Role in exemTableRow)
                                            {
                                                exemTableRowTD = Role.FindElements(By.TagName("td"));
                                                if (exemTableRowTD.Count != 0)
                                                {
                                                    //Exemption Type~Requested Date~Granted Date~Renewal Date~Prorate Date~Requested Amount~Granted Amount
                                                    string exemdet = exemTableRowTD[0].Text + "~" + exemTableRowTD[1].Text + "~" + exemTableRowTD[2].Text + "~" + exemTableRowTD[3].Text + "~" + exemTableRowTD[4].Text + "~" + exemTableRowTD[5].Text + "~" + exemTableRowTD[6].Text;
                                                    gc.insert_date(orderNumber, outputparcel, 1786, exemdet, 1, DateTime.Now);
                                                }
                                            }
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
                                        gc.insert_date(orderNumber, outputparcel, 1788, boddet, 1, DateTime.Now);
                                    }
                                }

                                //Tax History                                
                                IList<IWebElement> tables = driver.FindElements(By.XPath("//*[@id='parcel-views']/div/div/table"));
                                int count = tables.Count;
                                int j = 0; string tax = "";
                                foreach (IWebElement tab in tables)
                                {
                                    if (tab.Text.Contains("Amount Unpaid"))
                                    {
                                        IList<IWebElement> hisTableRow = tab.FindElements(By.XPath("tbody/tr"));
                                        IList<IWebElement> hisTableRowTD;
                                        foreach (IWebElement Role in hisTableRow)
                                        {
                                            hisTableRowTD = Role.FindElements(By.TagName("td"));
                                            if (hisTableRowTD.Count != 0)
                                            {
                                                if (hisTableRowTD.Count == 3)
                                                {
                                                    tax = hisTableRowTD[0].GetAttribute("innerText") + "~" + hisTableRowTD[1].GetAttribute("innerText") + "~" + hisTableRowTD[2].GetAttribute("innerText") + "~" + "" + "~" + Taxsold;
                                                    gc.insert_date(orderNumber, outputparcel, 1789, tax, 1, DateTime.Now);
                                                }
                                                if (hisTableRowTD.Count == 4)

                                                {
                                                    //Tax Year~Total Due~Total Paid~Amount Unpaid
                                                    tax = hisTableRowTD[0].GetAttribute("innerText") + "~" + hisTableRowTD[1].GetAttribute("innerText") + "~" + hisTableRowTD[2].GetAttribute("innerText") + "~" + hisTableRowTD[3].GetAttribute("innerText") + "~" + "";
                                                    gc.insert_date(orderNumber, outputparcel, 1789, tax, 1, DateTime.Now);
                                                }

                                            }
                                        }
                                    }
                                }



                                //Redemptions 
                                try
                                {                                   
                                    IList<IWebElement> tablesr = driver.FindElements(By.XPath("//*[@id='parcel-views']/div/table"));
                                    int countr = tablesr.Count;
                                    foreach (IWebElement tab in tablesr)
                                    {
                                        if (tab.Text.Contains("Certificate"))
                                        {                                            
                                            IList<IWebElement> redeTableRow = tab.FindElements(By.TagName("tbody/tr"));
                                            IList<IWebElement> redeTableRowTD;
                                            foreach (IWebElement Role in redeTableRow)
                                            {
                                                redeTableRowTD = Role.FindElements(By.TagName("td"));
                                                if (redeTableRowTD.Count != 0)
                                                {
                                                    //Year~Certificate~Type~Date Sold~Sale Status~Status Date~Penalty Date
                                                    string rededet = redeTableRowTD[0].GetAttribute("innerText") + "~" + redeTableRowTD[1].GetAttribute("innerText") + "~" + redeTableRowTD[2].GetAttribute("innerText") + "~" + redeTableRowTD[3].GetAttribute("innerText") + "~" + redeTableRowTD[4].GetAttribute("innerText") + "~" + redeTableRowTD[5].GetAttribute("innerText") + "~" + redeTableRowTD[6].GetAttribute("innerText");
                                                    gc.insert_date(orderNumber, outputparcel, 1790, rededet, 1, DateTime.Now);
                                                }

                                            }
                                        }
                                    }                                
                                          
                                }
                                catch
                                {

                                }

                                try
                                {
                                    IList<IWebElement> tabless = driver.FindElements(By.XPath("//*[@id='parcel-views']/div/table"));
                                    int counts = tabless.Count;
                                    foreach (IWebElement tab in tabless)
                                    {
                                        if (tab.Text.Contains("Document #"))
                                        {
                                            IList<IWebElement> saleTableRow = tab.FindElements(By.TagName("tr"));
                                            IList<IWebElement> saleTableRowTD;
                                            foreach (IWebElement Role in saleTableRow)
                                            {
                                                saleTableRowTD = Role.FindElements(By.TagName("td"));
                                                if (saleTableRowTD.Count != 0)
                                                {
                                                    //Year~Document #~Sale Type~Sale Date~Sold By~Sold To~Gross Price~Personal Property~Net Price
                                                    string rededet = saleTableRowTD[0].Text + "~" + saleTableRowTD[1].Text + "~" + saleTableRowTD[2].Text + "~" + saleTableRowTD[3].Text + "~" + saleTableRowTD[4].Text + "~" + saleTableRowTD[5].Text + "~" + saleTableRowTD[6].Text + "~" + saleTableRowTD[7].Text + "~" + saleTableRowTD[8].Text;
                                                    gc.insert_date(orderNumber, outputparcel, 1792, rededet, 1, DateTime.Now);
                                                }

                                            }
                                        }
                                    }
                                   
                                }
                                catch { }
                                //Get bill URL
                                IWebElement Iurl = driver.FindElement(By.XPath("//*[@id='parcel-views']/div[3]/table/tbody/tr[5]/td[3]/div[1]/a"));
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
                                    gc.AutoDownloadFile(orderNumber, outputparcel, "McHenry", "IL", filename + ".pdf");
                                    // gc.AutoDownloadFileSpokane(orderNumber, outputparcel, "McHenry", "IL", filename+".pdf");
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
                                IList<IWebElement> tableb = driver.FindElements(By.XPath("//*[@id='parcel-views']/div/table"));
                                int countb = tableb.Count;
                                foreach (IWebElement tab in tableb)
                                {
                                    if (tab.Text.Contains("Installment"))
                                    {
                                        IList<IWebElement> billTableRow = tab.FindElements(By.TagName("tr"));
                                        IList<IWebElement> billTableRowTD;
                                        foreach (IWebElement Role in billTableRow)
                                        {
                                            billTableRowTD = Role.FindElements(By.TagName("td"));
                                            if (billTableRowTD.Count != 0)
                                            {
                                                string billdet = billTableRowTD[0].Text + "~" + billTableRowTD[1].Text + "~" + billTableRowTD[2].Text + "~" + billTableRowTD[3].Text + "~" + billTableRowTD[4].Text + "~" + billTableRowTD[5].Text + "~" + billTableRowTD[6].Text + "~" + billTableRowTD[7].Text + "~" + billTableRowTD[8].Text + "~" + billTableRowTD[9].Text + "~" + TA;
                                                gc.insert_date(orderNumber, outputparcel, 1787, billdet, 1, DateTime.Now);
                                            }

                                        }
                                    }
                                }


                            }
                        }
                    }


                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "IL", "McHenry", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    gc.mergpdf(orderNumber, "IL", "McHenry");
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
