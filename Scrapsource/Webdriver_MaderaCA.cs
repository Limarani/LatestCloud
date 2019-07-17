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
using System.ComponentModel;
using System.Text;
using HtmlAgilityPack;
using iTextSharp.text;
using System.Text.RegularExpressions;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Support.Extensions;
using System.Net;
using OpenQA.Selenium.Firefox;
using System.Diagnostics;
using OpenQA.Selenium.Remote;
using Org.BouncyCastle.Utilities;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
namespace ScrapMaricopa.Scrapsource
{
    public class Webdriver_MaderaCA
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();
        Placer pltitle = new Placer();

        public string FTP_MaderaCA(string Address, string ownernm, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            GlobalClass.sname = "CA";
            GlobalClass.cname = "Madera";
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string Tax_Authority = "", YearBuilt = "", Taxresult1 = "", taxresult2 = "", taxresult3 = "", Firsthalf = "", Secondhalf = "", fullhalf = "", prepayresult1 = "", prepayresult2 = "", prepayresult3 = "", paiedamt = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new ChromeDriver();   
            // driver = new PhantomJSDriver()
            List<string> hreftax = new List<string>();
            string addresscs = "", rollcast = "", rollcast1 = "";
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    if (searchType == "titleflex")
                    {
                        //string address = houseno + " " + direction + " " + streetname + " " + streettype;
                        gc.TitleFlexSearch(orderNumber, parcelNumber, ownernm, Address, "CA", "Madera");
                        if (HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes")
                        {
                            driver.Quit();
                            return "MultiParcel";

                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Zero_Madera"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString().Replace("-", "");
                        searchType = "parcel";
                    }
                    int lat = 0;
                    int q = 0;
                    driver.Navigate().GoToUrl("https://common3.mptsweb.com/mbc/madera/tax/search");
                    IWebElement Taxauthoritytable = driver.FindElement(By.XPath("//*[@id='footer']/div[1]/div/div[1]/div[2]/div/ul"));
                    Tax_Authority = GlobalClass.After(Taxauthoritytable.Text, "Tax Collector").Trim();
                    driver.FindElement(By.Id("SearchValue")).SendKeys(parcelNumber);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Information Parcel", driver, "CA", "Madera");
                    for (int p = 0; p < 2; p++)
                    {
                        if (p == 1)
                        {

                            IWebElement PropertyHistry = driver.FindElement(By.Id("SelTaxYear"));
                            SelectElement PropertySelect = new SelectElement(PropertyHistry);
                            PropertySelect.SelectByIndex(p);
                            Thread.Sleep(1000);

                            gc.CreatePdf(orderNumber, parcelNumber, "Peior", driver, "CA", "Madera");
                        }
                        driver.FindElement(By.Id("SearchSubmit")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Information click" + p, driver, "CA", "Madera");
                        IWebElement taxviewclick = driver.FindElement(By.Id("ResultDiv"));
                        IList<IWebElement> Aherftax = taxviewclick.FindElements(By.TagName("a"));

                        foreach (IWebElement taxviewdetail in Aherftax)
                        {
                            string Taxid = GlobalClass.After(taxviewdetail.GetAttribute("href"), "tax/main/");
                            if (p == 0)
                            {

                                if (taxviewdetail.Text.Contains("View Details"))
                                {
                                    lat++;
                                    hreftax.Add(taxviewdetail.GetAttribute("href"));

                                }
                            }
                            if (p == 1)
                            {
                                if (taxviewdetail.Text.Contains("View Details"))
                                {
                                    hreftax.Add(taxviewdetail.GetAttribute("href"));

                                }
                            }
                        }
                    }
                    int i = 1; string Balancetax = "";
                    int lat1 = 0;
                    string pyear = "";
                    foreach (string firstview in hreftax)
                    {

                        driver.Navigate().GoToUrl(firstview);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Information Detail" + i, driver, "CA", "Madera");
                        IWebElement assmenttaxtable = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/ul/li[2]/a"));
                        string assmenttaxhrdf = assmenttaxtable.GetAttribute("href");
                        assmenttaxtable.Click();
                        // driver.Navigate().GoToUrl(assmenttaxhrdf);
                        Thread.Sleep(2000);
                        IWebElement rollcasttable = driver.FindElement(By.XPath("//*[@id='h2tab2']"));
                        rollcast = gc.Between(rollcasttable.Text, "Roll Category", "Doc Num").Trim();
                        string rollcasttype = GlobalClass.Before(rollcast, "-");
                        addresscs = GlobalClass.After(rollcasttable.Text, "Address");
                        gc.CreatePdf(orderNumber, parcelNumber, "Assment Information Detail" + i, driver, "CA", "Madera");
                        driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/ul/li[1]/a")).Click();
                        Thread.Sleep(2000);
                        IWebElement taxinfotable = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/div/div[1]"));
                        string Assessment = gc.Between(taxinfotable.Text, "ASMT", "PARCEL");
                        string taxyear = "";
                        if (taxinfotable.Text.Contains("VIEW TAX BILL"))
                        {
                            taxyear = gc.Between(taxinfotable.Text, "YEAR", "VIEW TAX BILL").Trim();
                        }
                        else
                        {
                            taxyear = GlobalClass.After(taxinfotable.Text, "YEAR").Trim();
                        }

                        if (rollcasttype.Contains("CS"))
                        {
                            pyear = taxyear;
                        }
                        IList<IWebElement> viwetaxbill = taxinfotable.FindElements(By.TagName("a"));
                        foreach (IWebElement taxyearelement in viwetaxbill)
                        {
                            if (taxyearelement.Text.Contains("VIEW TAX BILL"))
                            {
                                string viewhref = taxyearelement.GetAttribute("href");
                                gc.downloadfile(viewhref, orderNumber, parcelNumber, "ViewTaxBill.pdf" + i, "CA", "Madera");
                                i++;
                            }
                        }
                        //IWebElement compain = driver.FindElement(By.XPath("//*[@id='h2tab1']/div[2]"));
                        //string compain1 = GlobalClass.Before(compain.Text, "Total Due");
                        //string TotalDuecompain = gc.Between(compain.Text, "Total Due", "Total Paid").Trim();
                        //string TotalPaidcompain = gc.Between(compain.Text, "Total Paid", "Total Balance").Trim();
                        //string TotalBalance = GlobalClass.After(compain.Text, "Total Balance");

                        string FilePath = gc.filePath(orderNumber, parcelNumber) + "ViewTaxBill.pdf" + (i - 1) + ".pdf";
                        PdfReader reader;
                        string pdfData;
                        string pdftext = "";
                        try
                        {
                            reader = new PdfReader(FilePath);
                            String textFromPage = PdfTextExtractor.GetTextFromPage(reader, 1);
                            System.Diagnostics.Debug.WriteLine("" + textFromPage);
                            pdftext = textFromPage;


                            //PDF reading for assessment details
                            if (pdftext.Contains("SECURED TAX ROLL"))
                            {
                                string tableassess = gc.Between(pdftext, " PHONE # DESCRIPTION PRIOR CURRENT      BILLED ", "VALUES  X  TAX RATE").Trim();
                                string[] tableArray = tableassess.Split('\n');
                                string total = "", taxratearea = "";

                                total = GlobalClass.After(tableassess, "NET TAXABLE VALUE").Trim();
                                pltitle.Land = tableArray[0].Split(' ').Last();
                                pltitle.Improvements = tableArray[3].Split(' ').Last();
                                pltitle.ExemptionHomeowners = tableArray[6].Split(' ').Last().Replace("-", "");
                                taxratearea = gc.Between(pdftext, "TAX RATE AREA:", "ACRES:").Trim();
                                if (taxratearea.Contains("\n"))
                                {
                                    string strtaxratearea = gc.Between(pdftext, "TAX RATE AREA:", "ACRES:").Trim();
                                    taxratearea = GlobalClass.Before(strtaxratearea, "\n").Replace("\r", "").Trim();
                                }
                                if (taxratearea.Contains("Cortac Number"))
                                {
                                    taxratearea = GlobalClass.Before(taxratearea, "Cortac Number").Trim();
                                    pltitle.TaxIDNumberFurtherDescribed = taxratearea;
                                }
                                if (taxratearea.Contains("Ownership change"))
                                {
                                    taxratearea = GlobalClass.Before(taxratearea, "Ownership change").Trim();
                                    pltitle.TaxIDNumberFurtherDescribed = taxratearea;
                                }
                                if (taxratearea.Contains("New owner bill"))
                                {
                                    taxratearea = GlobalClass.Before(taxratearea, "New owner bill").Trim();
                                    pltitle.TaxIDNumberFurtherDescribed = taxratearea;
                                }
                                pltitle.TaxIDNumberFurtherDescribed = taxratearea;

                            }

                        }
                        catch { }
                        //pdf reading end
                        for (int j = 1; j < 3; j++)
                        {
                            string PaidStatus1 = "", Due_PaidDate = "";
                            IWebElement firstinstalment = driver.FindElement(By.XPath("//*[@id='h2tab1']/div[1]/div[" + j + "]"));
                            string instalfirst = GlobalClass.Before(firstinstalment.Text, "Paid Status");
                            //FirstInstallment     
                            if(firstinstalment.Text.Contains("Delinq. Date"))
                            {
                                PaidStatus1 = gc.Between(firstinstalment.Text, "Paid Status", "Delinq. Date").Trim();

                                Due_PaidDate = gc.Between(firstinstalment.Text, "Delinq. Date", "Total Due").Trim();
                            }
                            else
                            {
                                PaidStatus1 = gc.Between(firstinstalment.Text, "Paid Status", "Paid Date").Trim();

                                Due_PaidDate = gc.Between(firstinstalment.Text, "Paid Date", "Total Due").Trim();
                            }
                            string totaldue = gc.Between(firstinstalment.Text, "Total Due", "Total Paid").Trim();
                            string TotalPaid = gc.Between(firstinstalment.Text, "Total Paid", "Balance").Trim();
                            if (firstinstalment.Text.Contains("\r\nADD"))
                            {
                                Balancetax = gc.Between(firstinstalment.Text, "Balance", "\r\nADD").Trim();
                            }
                            if (!firstinstalment.Text.Contains("\r\nADD"))
                            {
                                Balancetax = GlobalClass.After(firstinstalment.Text, "Balance").Trim();
                            }
                            if (lat1 < lat)
                            {
                                pltitle.assyear = taxyear.Trim();

                                pltitle.Year = Convert.ToInt32(pltitle.assyear);

                                if (j == 1)
                                {
                                    pltitle.FirstInstallment = totaldue.Replace(",", "").Replace("$", "").Trim();
                                    pltitle.FirstDueDate = Due_PaidDate;
                                    pltitle.FirstPaid = TotalPaid;
                                    if (TotalPaid == "$0.00")
                                    {
                                        pltitle.FirstPaid = "False";
                                    }
                                    else
                                    {
                                        pltitle.FirstPaid = "True";
                                    }
                                    pltitle.FirstDue = Balancetax;
                                    if (Balancetax == "$0.00")
                                    {
                                        pltitle.FirstDue = "False";
                                    }
                                    else
                                    {
                                        pltitle.FirstDue = "True";
                                    }
                                    //Paid Status
                                    if (PaidStatus1 == "PAID")
                                    {
                                        pltitle.FirstTaxesOutDate = Due_PaidDate;
                                        pltitle.FirstDueDate = null;
                                    }

                                    else if ((PaidStatus1 == "LATE") || (PaidStatus1 == "DUE"))
                                    {
                                        pltitle.FirstDueDate = Due_PaidDate;
                                        pltitle.FirstTaxesOutDate = null;
                                    }
                                }
                                if (j == 2)
                                {
                                    pltitle.SecondInstallment = totaldue.Replace(",", "").Replace("$", "").Trim();
                                    // pltitle.SecondDueDate = Due_PaidDate;
                                    // pltitle.SecondPaid = TotalPaid;
                                    if (TotalPaid == "$0.00")
                                    {
                                        pltitle.SecondPaid = "False";
                                    }
                                    else
                                    {
                                        pltitle.SecondPaid = "True";
                                    }
                                    pltitle.SecondDue = Balancetax;
                                    if (Balancetax == "$0.00")
                                    {
                                        pltitle.SecondDue = "False";
                                    }
                                    else
                                    {
                                        pltitle.SecondDue = "True";
                                    }
                                    if (PaidStatus1 == "PAID")
                                    {

                                        pltitle.SecondTaxesOutDate = Due_PaidDate;
                                        pltitle.SecondDueDate = null;

                                    }
                                    else if (PaidStatus1 == "LATE" || PaidStatus1 == "DUE")
                                    {
                                        pltitle.SecondDueDate = Due_PaidDate;
                                        pltitle.SecondTaxesOutDate = null;
                                    }
                                }


                                // q++;
                            }
                            string taxresult = addresscs + "~" + rollcast + "~" + Assessment + "~" + taxyear + "~" + instalfirst + "~" + PaidStatus1 + "~" + Due_PaidDate + "~" + totaldue + "~" + TotalPaid + "~" + Balancetax + "~" + Tax_Authority;
                            gc.insert_date(orderNumber, parcelNumber, 1240, taxresult, 1, DateTime.Now);
                        }
                        AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                        try
                        {
                            driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/ul/li[3]/a")).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Tax Code Detail" + i, driver, "CA", "Madera");
                            for (int k = 1; k < 15; k++)
                            {
                                try
                                {
                                    IWebElement Taxcodeinfotable = driver.FindElement(By.XPath("//*[@id='h2tab3']/div[" + k + "]/div/div/dl"));
                                    string tax_code = gc.Between(Taxcodeinfotable.Text, "Tax Code", "Description").Trim();
                                    string Description = gc.Between(Taxcodeinfotable.Text, "Description", "Rate").Trim();
                                    string Rate = gc.Between(Taxcodeinfotable.Text, "Rate", "1st").Trim();
                                    string Firstinstalment = gc.Between(Taxcodeinfotable.Text, "1st Installment", "2nd ").Trim();
                                    string secondinstalment = gc.Between(Taxcodeinfotable.Text, "2nd Installment", "Total").Trim();
                                    string Total = gc.Between(Taxcodeinfotable.Text, "Total", "Phone").Trim();
                                    string Phone = GlobalClass.After(Taxcodeinfotable.Text, "Phone").Trim();
                                    string Taxcoderesult = taxyear + "~" + tax_code + "~" + Description + "~" + Rate + "~" + Firstinstalment + "~" + secondinstalment + "~" + Total + "~" + Phone;
                                    //pltitle.assyear = taxyear;
                                    gc.insert_date(orderNumber, parcelNumber, 1241, Taxcoderesult, 1, DateTime.Now);
                                }
                                catch { }
                            }
                        }
                        catch { }
                        //Default tax
                        try
                        {
                            driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/ul/li[4]/a")).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Default tax" + i, driver, "CA", "Madera");
                            IWebElement defaulttaxtable = driver.FindElement(By.XPath("//*[@id='h2tab4']/div[2]"));
                            string DefaultNumber = gc.Between(defaulttaxtable.Text, "Default Number", "Pay Plan in Effect");
                            string PayEffect = gc.Between(defaulttaxtable.Text, "in Effect", "Annual Payment");
                            string AnnualPayment = gc.Between(defaulttaxtable.Text, "Annual Payment", "\r\nBalance");
                            string Balance = GlobalClass.After(defaulttaxtable.Text, "Balance\r\n");
                            string Commends = "For Defaulted Tax due, you must call the Collector's Office";
                            string defaulttaxresult = taxyear + "~" + DefaultNumber + "~" + PayEffect + "~" + AnnualPayment + "~" + Balance + "~" + Commends;
                            // pltitle.assyear = taxyear;
                            gc.insert_date(orderNumber, parcelNumber, 1242, defaulttaxresult, 1, DateTime.Now);
                        }
                        catch (Exception ex) { }

                        if (lat1 < lat)
                        {

                            pltitle.TaxIDNumber = parcelNumber;
                            if (rollcasttype.Contains("CS"))
                            {
                                gc.InsertSearchTax(orderNumber, pltitle.Land, pltitle.Improvements, pltitle.ExemptionHomeowners, "0", pltitle.FirstInstallment, pltitle.FirstDueDate, pltitle.FirstTaxesOutDate, pltitle.FirstPaid, pltitle.FirstDue, pltitle.SecondInstallment, pltitle.SecondDueDate, pltitle.SecondTaxesOutDate, pltitle.SecondPaid, pltitle.SecondDue, pltitle.assyear, 1, pltitle.Year, "Madera County Treasurer-Tax Collector", parcelNumber, "County", pltitle.TaxIDNumberFurtherDescribed);
                            }
                            if (rollcasttype.Contains("SS"))
                            {
                                if (pyear == taxyear)
                                {
                                    gc.InsertSearchTax(orderNumber, pltitle.Land, pltitle.Improvements, pltitle.ExemptionHomeowners, "0", pltitle.FirstInstallment, pltitle.FirstDueDate, pltitle.FirstTaxesOutDate, pltitle.FirstPaid, pltitle.FirstDue, pltitle.SecondInstallment, pltitle.SecondDueDate, pltitle.SecondTaxesOutDate, pltitle.SecondPaid, pltitle.SecondDue, pltitle.assyear, 100, pltitle.Year, "Madera County Treasurer-Tax Collector", parcelNumber, "Supplemental", pltitle.TaxIDNumberFurtherDescribed);
                                }
                            }
                            lat1++;
                        }

                    }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "CA", "Madera", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    driver.Quit();
                    //PlacerTitle

                    gc.mergpdf(orderNumber, "CA", "Madera");
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
    }

}


