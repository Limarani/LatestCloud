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

namespace ScrapMaricopa.Scrapsource
{
    public class Webdriver_CAAlameda
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        public string FTP_CAAlamenda(string houseno, string direction, string sname, string unitno, string parcelNumber, string searchType, string orderNumber, string ownername, string City, string sttype)
        {

            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string Address = houseno + " " + sname;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "", AssessTakenTime = "", TaxTakentime = "", CityTaxtakentime = "";
            string TotaltakenTime = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new ChromeDriver();
            // driver = new PhantomJSDriver();
            using (driver = new PhantomJSDriver()) //PhantomJSDriver
            {
                driver.Manage().Window.Size = new Size(1920, 1080);
                string[] stringSeparators1 = new string[] { "\r\n" };
                List<string> listurl = new List<string>();
                string Date = "";
                Date = DateTime.Now.ToString("M/d/yyyy");
                string Alternatekey = "", PropertyAddress = "", MailingAddress = "", PropertyType = "", YearBuilt = "", LegalDescription = "", FullParcelID = "";
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");


                    if (searchType == "titleflex")
                    {
                        string straddress = "";
                        if (direction != "")
                        {
                            straddress = houseno + " " + direction + " " + sname + " " + sttype + " " + unitno;
                        }
                        else
                        {
                            straddress = houseno + " " + sname + " " + sttype + " " + unitno;
                        }
                        try
                        {
                            var Paslpit = parcelNumber.Split('-');
                            string Par1 = Paslpit[0];
                            string Par2 = Paslpit[1];
                            string Par3 = Paslpit[2];
                            string Par4 = Paslpit[3];
                            parcelNumber = "0" + Par1 + "-" + "0" + Par2 + "-" + "0" + Par3 + "-" + "0" + Par4;
                        }
                        catch { }


                        gc.TitleFlexSearch(orderNumber, parcelNumber, ownername, straddress, "CA", "Alameda");

                        string pa1 = "";
                        string pa2 = "";
                        string pa3 = "";
                        string pa4 = "";
                        if (HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].Equals("Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else
                        {

                            searchType = "parcel";


                        }

                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        parcelNumber = parcelNumber.Replace("-", "");

                        if (parcelNumber.Count() != 8)
                        {
                            parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                            try
                            {
                                var Numsplit = parcelNumber.Split('-');


                                pa1 = Numsplit[0].Substring(0, 1);


                                if (pa1 == "0")
                                {
                                    pa1 = Numsplit[0].Substring(1, 2);

                                }
                                else { pa1 = Numsplit[0]; }
                                pa2 = Numsplit[1].Substring(0, 1);
                                if (pa2 == "0")
                                {
                                    pa2 = Numsplit[1].Substring(1, 3);

                                }
                                else { pa2 = Numsplit[1]; }
                                pa3 = Numsplit[2].Substring(0, 1);
                                if (pa3 == "0")
                                {
                                    pa3 = Numsplit[2].Substring(1, 2);

                                }
                                else { pa3 = Numsplit[2]; }
                                pa4 = Numsplit[3].Substring(0, 1);
                                if (pa4 == "0")
                                {
                                    pa4 = Numsplit[3].Substring(1, 1);

                                }
                                else
                                { pa4 = Numsplit[3]; }
                                parcelNumber = pa1 + "-" + pa2 + "-" + pa3 + "-" + pa4;
                            }
                            catch
                            {
                                parcelNumber = pa1 + "-" + pa2 + "-" + pa3.Replace("0", "");

                            }

                        }

                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Zero_Alameda"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("http://www.acgov.org/MS/prop/index.aspx");
                        Thread.Sleep(2000);
                        driver.FindElement(By.XPath("//*[@id='txtStreetNum']")).SendKeys(houseno.Trim());
                        // driver.FindElement(By.XPath("//*[@id='inpDir']")).SendKeys(Dir.Trim());
                        driver.FindElement(By.XPath("//*[@id='txtStreetName']")).SendKeys(sname.Trim());
                        if (unitno != null && unitno != "")
                        {
                            driver.FindElement(By.XPath("//*[@id='txtUnitNum']")).SendKeys(unitno.Trim());
                        }
                        Thread.Sleep(1000);
                        var SerachCategory = driver.FindElement(By.XPath("//*[@id='ddlbCity']"));
                        var selectElement1 = new SelectElement(SerachCategory);
                        selectElement1.SelectByText(City);
                        Thread.Sleep(1000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "CA", "Alameda");
                        driver.FindElement(By.Name("btnSubmit")).Click();
                        try
                        {
                            string Nodata = driver.FindElement(By.Id("lblError")).Text;
                            if (Nodata.Contains("No information found"))
                            {
                                HttpContext.Current.Session["Zero_Alameda"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "CA", "Alameda");
                    }

                    if (searchType == "parcel")
                    {

                        driver.Navigate().GoToUrl("http://www.acgov.org/MS/prop/index.aspx");
                        Thread.Sleep(2000);

                        driver.FindElement(By.XPath("//*[@id='txtParcelNum']")).SendKeys(parcelNumber.Trim());
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel  Search", driver, "CA", "Alameda");
                        Thread.Sleep(1000);


                        driver.FindElement(By.XPath("//*[@id='btnSubmit']")).Click();

                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Result", driver, "CA", "Alameda");
                    }


                    string UseCode = "", Land = "", Improvement = "", TotatalTaxableValue = "", HomeOwner = "", Other = "", TotalNetTaxablevalue = "";

                    parcelNumber = driver.FindElement(By.XPath("//*[@id='FormView1_print_parcel_label']")).Text;
                    UseCode = driver.FindElement(By.XPath("//*[@id='FormView1_Label2']")).Text;
                    Land = driver.FindElement(By.XPath("//*[@id='FormView1_roll_land_label']")).Text;
                    Improvement = driver.FindElement(By.XPath("//*[@id='FormView1_roll_imps_label']")).Text;
                    TotatalTaxableValue = driver.FindElement(By.XPath("//*[@id='FormView1_roll_tot_tax_label']")).Text;
                    HomeOwner = driver.FindElement(By.XPath("//*[@id='FormView1_roll_hoex_label']")).Text;
                    Other = driver.FindElement(By.XPath("//*[@id='FormView1_roll_otex_label']")).Text;
                    TotalNetTaxablevalue = driver.FindElement(By.XPath("//*[@id='FormView1_roll_net_tax_label']")).Text;
                    string Description = driver.FindElement(By.XPath("//*[@id='FormView1_use_name_label']")).Text;

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("https://www.acgov.org/ptax_pub_app/RealSearchInit.do?showSearchParmsFromLookup=true");
                    Thread.Sleep(5000);
                    driver.Manage().Cookies.DeleteAllCookies();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax 1", driver, "CA", "Alameda");
                    driver.FindElement(By.XPath("//*[@id='displayApn']")).SendKeys(parcelNumber);
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax 2", driver, "CA", "Alameda");

                    driver.FindElement(By.XPath("//*[@id='taxcontent']/form/table[5]/tbody/tr[1]/td[2]/input")).Click();
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax 2", driver, "CA", "Alameda");
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='taxcontent']/form/table[5]/tbody/tr[1]/td[5]/input")).Click();
                    }
                    catch
                    {

                    }
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='taxcontent']/form/table[5]/tbody/tr[1]/td[5]/input")).SendKeys(Keys.Enter);
                    }
                    catch { }
                    Thread.Sleep(5000);
                    gc.CreatePdf(orderNumber, parcelNumber, "TaxDetail", driver, "CA", "Alameda");
                    PropertyAddress = driver.FindElement(By.XPath("//*[@id='pplresultcontent3']/tbody/tr[3]/td[2]")).Text;

                    string PropertyDetail = PropertyAddress + "~" + UseCode + "~" + Description;
                    string Assesment = Improvement + "~" + TotatalTaxableValue + "~" + HomeOwner + "~" + Other + "~" + TotalNetTaxablevalue + "~" + Land;


                    gc.insert_date(orderNumber, parcelNumber, 301, PropertyDetail, 1, DateTime.Now);

                    gc.insert_date(orderNumber, parcelNumber, 329, Assesment, 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    string TaxType = "", Installment = "", BillYear = "", DueDate = "", Tracer = "", TotalAmount = "", InstallmentAmount = "", OptionStatusDate = "";
                    int I = 0;
                    IWebElement TaxDisTB = driver.FindElement(By.XPath("//*[@id='pplresultcontent4']/tbody"));
                    IList<IWebElement> TaxDisTR = TaxDisTB.FindElements(By.TagName("tr"));
                    IList<IWebElement> TaxDisTD;
                    IList<IWebElement> TaxDisTA;

                    foreach (IWebElement row1 in TaxDisTR)
                    {

                        TaxDisTD = row1.FindElements(By.TagName("td"));

                        if (TaxDisTD.Count != 0 && TaxDisTD.Count != 1 && TaxDisTD[1].Text != "Tax Type" && TaxDisTD[1].Text != "Installment" && I == 0)
                        {
                            TaxType = TaxDisTD[1].Text;

                            BillYear = TaxDisTD[2].Text;

                            Tracer = TaxDisTD[3].Text;
                            TotalAmount = TaxDisTD[4].Text;
                            I++;
                            if (TaxType == "Supplemental")
                            {
                                foreach (IWebElement row in TaxDisTD)
                                {
                                    TaxDisTA = row.FindElements(By.TagName("a"));
                                    if (TaxDisTA.Count != 0)
                                    {

                                        listurl.Add(TaxDisTA[0].GetAttribute("href"));
                                    }
                                }


                            }




                        }
                        else if (TaxDisTD.Count != 0 && TaxDisTD.Count != 1 && TaxDisTD[1].Text != "Tax Type" && TaxDisTD[1].Text != "Installment" && I == 1)
                        {
                            Installment = TaxDisTD[1].Text;

                            DueDate = TaxDisTD[2].Text;

                            InstallmentAmount = TaxDisTD[4].Text;
                            OptionStatusDate = TaxDisTD[5].Text;


                            string Currenttax = TaxType + "~" + Installment + "~" + BillYear + "~" + DueDate + "~" + Tracer + "~" + InstallmentAmount + "~" + TotalAmount + "~" + OptionStatusDate;
                            gc.insert_date(orderNumber, parcelNumber, 308, Currenttax, 1, DateTime.Now);
                            if (Installment == "2nd Installment")
                            { I = 0; }

                        }

                    }

                    try
                    {
                        IWebElement TaxHisTB = driver.FindElement(By.XPath("//*[@id='pplresultcontent5']/tbody"));
                        IList<IWebElement> TaxHisTR = TaxHisTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> TaxHisTD;
                        int K = 0;

                        foreach (IWebElement row1 in TaxHisTR)
                        {

                            TaxHisTD = row1.FindElements(By.TagName("td"));

                            if (TaxHisTD.Count != 0 && TaxHisTD.Count != 1 && TaxHisTD[1].Text != "Tax Type" && TaxHisTD[1].Text != "Installment" && K == 0)
                            {
                                TaxType = TaxHisTD[1].Text;
                                BillYear = TaxHisTD[2].Text;
                                Tracer = TaxHisTD[3].Text;
                                TotalAmount = TaxHisTD[4].Text;
                                K++;
                            }
                            else if (TaxHisTD.Count != 0 && TaxHisTD.Count != 1 && TaxHisTD[1].Text != "Tax Type" && TaxHisTD[1].Text != "Installment" && K == 1)
                            {
                                Installment = TaxHisTD[1].Text;
                                DueDate = TaxHisTD[2].Text;
                                InstallmentAmount = TaxHisTD[4].Text;
                                OptionStatusDate = TaxHisTD[5].Text;
                                string Currenttax = TaxType + "~" + Installment + "~" + BillYear + "~" + DueDate + "~" + Tracer + "~" + InstallmentAmount + "~" + TotalAmount + "~" + OptionStatusDate;
                                gc.insert_date(orderNumber, parcelNumber, 309, Currenttax, 1, DateTime.Now);
                                if (Installment == "2nd Installment")
                                { K = 0; }
                            }
                        }
                    }
                    catch { }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    string DownloadURL = driver.FindElement(By.XPath("//*[@id='pplresultcontent4']/tbody/tr[4]/td[6]/a")).GetAttribute("href");

                    //  gc.downloadfile(DownloadURL1, orderNumber, parcelNumber, "Current  tax bill"+ Tracer, "CA", "Alameda");
                    //load chrome driver...
                    //IWebDriver chDriver = new ChromeDriver();
                    List<string> strTaxRealestate = new List<string>();
                    var chromeOptions = new ChromeOptions();
                    var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];

                    chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                    chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                    chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                    chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                    var chDriver = new ChromeDriver(chromeOptions);

                    try
                    {
                        chDriver.Navigate().GoToUrl(DownloadURL);
                        chDriver.Manage().Cookies.DeleteAllCookies();
                        Thread.Sleep(2000);
                        chDriver.FindElement(By.Name("displayApn")).SendKeys(parcelNumber.Trim());
                        chDriver.FindElement(By.Name("searchBills")).Click();
                        Thread.Sleep(3000);
                        IWebElement bill1 = chDriver.FindElement(By.XPath("//*[@id='pplresultcontent4']/tbody/tr[4]/td[6]/a[1]"));
                        bill1.Click();
                        Thread.Sleep(4000);
                        gc.AutoDownloadFile(orderNumber, parcelNumber, "Alameda", "CA", "SecuredBill" + ".pdf");
                        IWebElement bill2 = chDriver.FindElement(By.XPath("//*[@id='pplresultcontent4']/tbody/tr[9]/td[6]/a[1]"));
                        bill2.Click();
                        Thread.Sleep(4000);
                        gc.AutoDownloadFile(orderNumber, parcelNumber, "Alameda", "CA", "SupplementalBill" + ".pdf");
                        Thread.Sleep(2000);
                        chDriver.Quit();

                    }
                    catch
                    {
                        chDriver.Quit();
                    }
                    //gc.downloadfile(DownloadURL, orderNumber, parcelNumber, "tax bill", "CA", "Alameda");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "CA", "Alameda", StartTime, AssessmentTime, TaxTime, CityTaxtakentime, LastEndTime);
                    driver.Quit();
                    gc.mergpdf(orderNumber, "CA", "Alameda");
                    //gc.MMREM_Template(orderNumber, parcelNumber, "", driver, "CA", "Alameda", "9", "");
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