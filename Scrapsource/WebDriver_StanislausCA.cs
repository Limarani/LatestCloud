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

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_StanislausCA
    {


        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();
        IWebElement multiparceladd;

        public string FTP_StanislausCA(string Address, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string Parcel_number = "", Tax_Authority = "", YearBuilt = "", Taxresult1 = "", taxresult2 = "", taxresult3 = "", Firsthalf = "", Secondhalf = "", fullhalf = "", prepayresult1 = "", prepayresult2 = "", prepayresult3 = "", paiedamt = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new ChromeDriver();
            //PhantomJSDriver
            driver = new PhantomJSDriver();
            try
            {
                StartTime = DateTime.Now.ToString("HH:mm:ss");
                driver.Navigate().GoToUrl("http://qa.co.stanislaus.ca.us/AssessorWeb/public/");
                if (searchType == "titleflex")
                {
                    //string address = houseno + " " + direction + " " + streetname + " " + streettype;
                    gc.TitleFlexSearch(orderNumber, "", ownername, Address, "CA", "Stanislaus");
                    if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                    {
                        driver.Quit();
                        return "MultiParcel";
                    }
                    else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                    {
                        HttpContext.Current.Session["Zero_Stanislaus"] = "Zero";
                        driver.Quit();
                        return "No Data Found";
                    }
                    parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                    searchType = "parcel";
                }
                if (searchType == "parcel")
                {
                    driver.FindElement(By.Id("feeParcelValue")).SendKeys(parcelNumber);
                    gc.CreatePdf_WOP(orderNumber, "Parcel search", driver, "CA", "Stanislaus");
                    IWebElement ISpan12 = driver.FindElement(By.XPath("//*[@id='pageForm']/table/tbody/tr[3]/td[4]/input[1]"));
                    IJavaScriptExecutor js12 = driver as IJavaScriptExecutor;
                    js12.ExecuteScript("arguments[0].click();", ISpan12);
                    Thread.Sleep(4000);
                    try
                    {
                        gc.CreatePdf_WOP(orderNumber, "Parcel search Result", driver, "CA", "Stanislaus");
                        int count = 0;
                        string Multiparcelnumber = "", Singlerowclick = "";
                        IWebElement Multyaddresstable1 = driver.FindElement(By.XPath("/html/body/center/iframe"));
                        driver.SwitchTo().Frame(Multyaddresstable1);
                        IWebElement Multyaddresstable = driver.FindElement(By.XPath("/html/body/table"));
                        IList<IWebElement> multiaddressrow = Multyaddresstable.FindElements(By.TagName("tr"));
                        IList<IWebElement> multiaddressid;
                        foreach (
                            IWebElement Multiaddress in multiaddressrow)
                        {
                            multiaddressid = Multiaddress.FindElements(By.TagName("td"));
                            if (multiaddressid.Count != 0 && Multiaddress.Text.Trim() != "" && !Multiaddress.Text.Contains("Name") && Multiparcelnumber != multiaddressid[1].Text.Trim())
                            {
                                Multiparcelnumber = multiaddressid[1].Text;
                                IWebElement Singleclick = multiaddressid[1].FindElement(By.TagName("a"));
                                Singlerowclick = Singleclick.GetAttribute("href");
                                string Ownername = multiaddressid[0].Text;
                                string multiaddressresult = Multiparcelnumber + "~" + Ownername;
                                gc.insert_date(orderNumber, Multiparcelnumber, 1266, multiaddressresult, 1, DateTime.Now);
                                count++;
                            }
                        }

                        if (count == 1)
                        {
                            driver.Navigate().GoToUrl("http://qa.co.stanislaus.ca.us/AssessorWeb/public/PublicView.jsp?asmt=" + Multiparcelnumber.Replace("-", ""));
                        }
                        if (count > 1 && count < 26)
                        {
                            HttpContext.Current.Session["multiparcel_Stanislaus"] = "Yes";
                            driver.Quit();
                            return "MultiParcel";
                        }
                        if (count > 25)
                        {
                            HttpContext.Current.Session["multiparcel_Stanislaus_Multicount"] = "Maximum";
                            driver.Quit();
                            return "Maximum";
                        }
                        if (count == 0)
                        {
                            HttpContext.Current.Session["Zero_Stanislaus"] = "Zero";
                            driver.Quit();
                            return "Zero";
                        }
                    }
                    catch { }

                }

                //Assessment Details
                //insert 1267
                gc.CreatePdf_WOP(orderNumber, "Assessment Details", driver, "CA", "Stanislaus");
                YearBuilt = driver.FindElement(By.XPath("/html/body/center[2]/center[2]/table/tbody/tr[8]/td")).Text;
                string Assessmentdetails1 = "", assessmentdescrip = "", Assessment = "", ParcelNumber = "", TaxRateArea = "", Taxability = "", LandUse = "", Assessee = "", Land = "", Structure = "", Fixtures = "", Growingimprovement = "", TotalLandImp = "", PersonalProperty = "", PersonalPropertyMH = "", Homeownerexpm = "", Otherexmp = "", NetAssessment = "", Ownership = "";

                IWebElement Bigdata1 = driver.FindElement(By.XPath("/html/body/center[2]/table/tbody[1]"));
                IList<IWebElement> TRBigdata1 = Bigdata1.FindElements(By.TagName("tr"));
                IList<IWebElement> TDBigdata1;
                foreach (IWebElement row in TRBigdata1)
                {
                    TDBigdata1 = row.FindElements(By.TagName("td"));

                    if (TDBigdata1.Count != 0 && TDBigdata1.Count == 2 && !row.Text.Contains("Current Document") && !row.Text.Contains("Land Sq Ft") && !row.Text.Contains("No assessment found"))
                    {
                        Assessment = TDBigdata1[0].Text;
                        ParcelNumber = TDBigdata1[1].Text;

                        //gc.insert_date(orderNumber, ParcelNumber, 1267, Assessmentdetails1, 1, DateTime.Now);
                    }
                    if (TDBigdata1.Count != 0 && TDBigdata1.Count == 2 && !row.Text.Contains("Assessment") && !row.Text.Contains("Current Document") && !row.Text.Contains("No assessment found"))
                    {

                        TaxRateArea = TDBigdata1[1].Text;

                        //gc.insert_date(orderNumber, ParcelNumber, 1267, Assessmentdetails1, 1, DateTime.Now);
                    }
                    if (TDBigdata1.Count != 0 && TDBigdata1.Count == 1 && !row.Text.Contains("Land Use") && !row.Text.Contains("Assessee") && !row.Text.Contains("Assessment Description") && !row.Text.Contains("No assessment found"))
                    {

                        Taxability = TDBigdata1[0].Text;

                        //gc.insert_date(orderNumber, ParcelNumber, 1267, Assessmentdetails1, 1, DateTime.Now);
                    }
                    if (TDBigdata1.Count != 0 && TDBigdata1.Count == 1 && !row.Text.Contains("Taxability") && !row.Text.Contains("Assessee") && !row.Text.Contains("Assessment Description") && !row.Text.Contains("No assessment found"))
                    {

                        LandUse = TDBigdata1[0].Text;

                        //gc.insert_date(orderNumber, ParcelNumber, 1267, Assessmentdetails1, 1, DateTime.Now);
                    }
                    if (TDBigdata1.Count != 0 && TDBigdata1.Count == 1 && !row.Text.Contains("Taxability") && !row.Text.Contains("Land Use") && !row.Text.Contains("Assessment Description") && !row.Text.Contains("No assessment found"))
                    {

                        Assessee = TDBigdata1[0].Text;

                        //gc.insert_date(orderNumber, ParcelNumber, 1267, Assessmentdetails1, 1, DateTime.Now);
                    }
                    if (TDBigdata1.Count != 0 && TDBigdata1.Count == 1 && row.Text.Contains("Assessment Description") && !row.Text.Contains("No assessment found"))
                    {

                        assessmentdescrip = TDBigdata1[0].Text;

                        //gc.insert_date(orderNumber, ParcelNumber, 1267, Assessmentdetails1, 1, DateTime.Now);
                    }

                }
                string Rollvaluesasof = "";
                try
                {
                    IWebElement Rollvalues = driver.FindElement(By.XPath("/html/body/center[2]/table/thead[2]/tr/th"));
                    Rollvaluesasof = GlobalClass.After(Rollvalues.Text, "Roll Values as of:").Trim();

                    IWebElement Bigdata2 = driver.FindElement(By.XPath("/html/body/center[2]/table/tbody[2]"));
                    IList<IWebElement> TRBigdata2 = Bigdata2.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDBigdata2;
                    foreach (IWebElement row2 in TRBigdata2)
                    {
                        TDBigdata2 = row2.FindElements(By.TagName("td"));

                        if (TDBigdata2.Count != 0 && TDBigdata2.Count == 2 && row2.Text.Contains("Land") && !row2.Text.Contains("Net Assessment"))
                        {
                            Land = TDBigdata2[0].Text;
                            PersonalProperty = TDBigdata2[1].Text;
                        }
                        if (TDBigdata2.Count != 0 && TDBigdata2.Count == 2 && row2.Text.Contains("Structure(s)"))
                        {
                            Structure = TDBigdata2[0].Text;
                            PersonalPropertyMH = TDBigdata2[1].Text;
                        }
                        if (TDBigdata2.Count != 0 && TDBigdata2.Count == 2 && row2.Text.Contains("Fixtures"))
                        {
                            Fixtures = TDBigdata2[0].Text;
                            Homeownerexpm = TDBigdata2[1].Text;
                        }
                        if (TDBigdata2.Count != 0 && TDBigdata2.Count == 2 && row2.Text.Contains("Growing Improvements"))
                        {
                            Growingimprovement = TDBigdata2[0].Text;
                            Otherexmp = TDBigdata2[1].Text;
                        }
                        if (TDBigdata2.Count != 0 && TDBigdata2.Count == 2 && row2.Text.Contains("Total Land & Improvements"))
                        {
                            TotalLandImp = TDBigdata2[0].Text;
                            NetAssessment = TDBigdata2[1].Text;
                        }

                    }
                }
                catch { }

                try
                {
                    string OwnerName1 = "", OwnPercentage = "", Pri = "";

                    IWebElement Bigdata3 = driver.FindElement(By.XPath("/html/body/center[2]/center[1]/table"));
                    IList<IWebElement> TRBigdata3 = Bigdata3.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDBigdata3;
                    foreach (IWebElement row3 in TRBigdata3)
                    {
                        TDBigdata3 = row3.FindElements(By.TagName("td"));

                        if (TDBigdata3.Count != 0 && TDBigdata3.Count == 6 && !row3.Text.Contains("Owner Name") /*&& !row3.Text.Contains("Net Assessment")*/)
                        {
                            OwnerName1 += TDBigdata3[0].Text + ", ";
                            OwnPercentage += TDBigdata3[1].Text + ", ";
                            Pri = TDBigdata3[2].Text + ", ";
                        }


                    }

                    string Assessesult = OwnerName1.Replace(",", "").Trim() + "~" + OwnPercentage.Replace(",", "").Trim() + "~" + Pri.Replace(",", "").Trim() + "~" + TaxRateArea.Trim() + "~" + Taxability.Trim() + "~" + LandUse.Trim() + "~" + Assessee.Trim() + "~" + assessmentdescrip.Trim() + "~" + Rollvaluesasof.Trim() + "~" + Land.Trim() + "~" + Structure.Trim() + "~" + Fixtures.Trim() + "~" + Growingimprovement.Trim() + "~" + TotalLandImp.Trim() + "~" + PersonalProperty.Trim() + "~" + PersonalPropertyMH.Trim() + "~" + Homeownerexpm.Trim() + "~" + Otherexmp.Trim() + "~" + NetAssessment.Trim() + "~" + YearBuilt.Trim();
                    gc.insert_date(orderNumber, parcelNumber, 1267, Assessesult, 1, DateTime.Now);
                }
                catch { }
                gc.CreatePdf(orderNumber, ParcelNumber, "Assessor Public Inquiry", driver, "CA", "Stanislaus");
                AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                //tax Information
                List<string> hreftax = new List<string>();
                string Balance = "";
                string addresscs = "", rollcast = "", rollcast1 = "";

                driver.Navigate().GoToUrl("https://common3.mptsweb.com/MBC/stanislaus/tax/search");
                IWebElement Taxauthoritytable = driver.FindElement(By.XPath("//*[@id='footer']/div[1]/div/div[1]/div[2]/div/ul"));
                Tax_Authority = GlobalClass.After(Taxauthoritytable.Text, "Tax Collector").Trim();
                driver.FindElement(By.Id("SearchValue")).Clear();
                IWebElement text = driver.FindElement(By.Id("SearchValue"));
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("document.getElementById('SearchValue').value='" + parcelNumber + "'");
                //driver.FindElement(By.XPath("//*[@id='compwa']/form/div/input[2]")).SendKeys(Keys.Enter);

                //driver.FindElement(By.Id("SearchValue")).SendKeys(parcelNumber);
                gc.CreatePdf(orderNumber, parcelNumber, "Tax Information Parcel", driver, "CA", "Stanislaus");
                driver.FindElement(By.Id("SearchSubmit")).Click();
                Thread.Sleep(2000);
                for (int p = 0; p < 2; p++)
                {
                    if (p == 1)
                    {
                        IWebElement PropertyHistry = driver.FindElement(By.Id("SelTaxYear"));
                        SelectElement PropertySelect = new SelectElement(PropertyHistry);
                        PropertySelect.SelectByIndex(p);
                        Thread.Sleep(1000);
                        driver.FindElement(By.Id("SearchSubmit")).Click();
                    }
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Information click" + p, driver, "CA", "Stanislaus");

                    IWebElement taxviewclick = driver.FindElement(By.Id("ResultDiv"));
                    IList<IWebElement> Aherftax = taxviewclick.FindElements(By.TagName("a"));

                    foreach (IWebElement taxviewdetail in Aherftax)
                    {
                        if (taxviewdetail.Text.Contains("View Details"))
                        {
                            hreftax.Add(taxviewdetail.GetAttribute("href"));
                        }
                    }
                }
                int i = 1;
                int q = 0;
                foreach (string firstview in hreftax)
                {
                    driver.Navigate().GoToUrl(firstview);
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Information Detail" + q, driver, "CA", "Stanislaus");
                    IWebElement assmenttaxtable = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/ul/li[2]/a"));
                    string assmenttaxhrdf = assmenttaxtable.GetAttribute("href");
                    assmenttaxtable.Click();
                    // driver.Navigate().GoToUrl(assmenttaxhrdf);
                    Thread.Sleep(5000);
                    IWebElement rollcasttable = driver.FindElement(By.XPath("//*[@id='h2tab2']"));
                    rollcast = gc.Between(rollcasttable.Text, "Roll Category", "Doc Num").Trim();
                    addresscs = GlobalClass.After(rollcasttable.Text, "Address");
                    string asstaxyear = gc.Between(rollcasttable.Text, "Taxyear", "Parcel Number").Trim();
                    string rollcategory = gc.Between(rollcasttable.Text, "Roll Category", "Doc Num").Trim();

                    gc.CreatePdf(orderNumber, parcelNumber, "Assment Information Detail" + q, driver, "CA", "Stanislaus");
                    driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/ul/li[1]/a")).Click();
                    Thread.Sleep(5000);
                    IWebElement taxinfotable = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/div/div[1]"));
                    string Assessment1 = gc.Between(taxinfotable.Text, "ASMT", "PARCEL");
                    string taxyear = GlobalClass.After(taxinfotable.Text, "YEAR").Trim();
                    IList<IWebElement> viwetaxbill = taxinfotable.FindElements(By.TagName("a"));
                    foreach (IWebElement taxyearelement in viwetaxbill)
                    {
                        if (taxyearelement.Text.Contains("VIEW TAX BILL"))
                        {
                            string viewhref = taxyearelement.GetAttribute("href");
                            gc.downloadfile(viewhref, orderNumber, parcelNumber, "ViewTaxBill.pdf" + i, "CA", "Stanislaus");
                            i++;
                        }

                    }
                    IWebElement compain = driver.FindElement(By.XPath("//*[@id='h2tab1']/div[2]"));
                    string compain1 = GlobalClass.Before(compain.Text, "Total Due");
                    string TotalDuecompain = gc.Between(compain.Text, "Total Due", "Total Paid").Trim();
                    string TotalPaidcompain = gc.Between(compain.Text, "Total Paid", "Total Balance").Trim();
                    string TotalBalance = GlobalClass.After(compain.Text, "Total Balance");
                    for (int j = 1; j < 3; j++)
                    {
                        IWebElement firstinstalment = driver.FindElement(By.XPath("//*[@id='h2tab1']/div[1]/div[" + j + "]"));
                        string PaidStatus1 = "", Due_PaidDate = "";
                        string instalfirst = GlobalClass.Before(firstinstalment.Text, "Paid Status");
                        if (firstinstalment.Text.Contains("Delinq. Date"))
                        {
                            PaidStatus1 = gc.Between(firstinstalment.Text, "Paid Status", "Delinq. Date").Trim();
                            Due_PaidDate = gc.Between(firstinstalment.Text, "Delinq. Date", "Total Due").Trim();
                        }
                        else
                        {
                            PaidStatus1 = gc.Between(firstinstalment.Text, "Paid Status", "Paid Date").Trim();
                            Due_PaidDate = gc.Between(firstinstalment.Text, "Paid Date", "Total Due").Trim();
                        }

                        // string PaidStatus1 = gc.Between(firstinstalment.Text, "Paid Status", "Delinq. Date").Trim();
                        // string Due_PaidDate = gc.Between(firstinstalment.Text, "Delinq. Date", "Total Due").Trim();
                        string totaldue = gc.Between(firstinstalment.Text, "Total Due", "Total Paid").Trim();
                        string TotalPaid = gc.Between(firstinstalment.Text, "Total Paid", "Balance").Trim();
                        if (firstinstalment.Text.Contains("ADD"))
                        {
                            Balance = gc.Between(firstinstalment.Text, "Balance", "\r\nADD").Trim();
                        }
                        if (!firstinstalment.Text.Contains("ADD"))
                        {
                            Balance = GlobalClass.After(firstinstalment.Text, "Balance").Trim();
                        }
                        string taxresult = addresscs + "~" + rollcast + "~" + Assessment1 + "~" + taxyear + "~" + instalfirst + "~" + PaidStatus1 + "~" + Due_PaidDate + "~" + totaldue + "~" + TotalPaid + "~" + Balance + "~" + compain1 + "~" + TotalDuecompain + "~" + TotalPaidcompain + "~" + TotalBalance + "~" + Tax_Authority;
                        gc.insert_date(orderNumber, parcelNumber, 1268, taxresult, 1, DateTime.Now);
                    }
                    //Tax Code Details
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/ul/li[3]/a")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Code Detail" + q, driver, "CA", "Stanislaus");
                        for (int k = 1; k < 11; k++)
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
                                string Taxcoderesult = asstaxyear + "~" + rollcategory + "~" + tax_code + "~" + Description + "~" + Rate + "~" + Firstinstalment + "~" + secondinstalment + "~" + Total + "~" + Phone;
                                gc.insert_date(orderNumber, parcelNumber, 1269, Taxcoderesult, 1, DateTime.Now);
                            }
                            catch { }
                        }
                    }
                    catch { }
                    //Default tax
                    string defaulltyear = "", assessnumber = "";
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/ul/li[4]/a")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Default Tax Detail" + q, driver, "CA", "Stanislaus");
                        //2018
                        IWebElement taxinfotablede = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/div/div[1]"));
                        defaulltyear = GlobalClass.After(taxinfotable.Text, "YEAR\r\n");
                        assessnumber = gc.Between(taxinfotable.Text, "ASMT", "PARCEL");
                        IWebElement defaulttaxtable = driver.FindElement(By.XPath("//*[@id='h2tab4']/div[2]"));
                        string DefaultNumber = gc.Between(defaulttaxtable.Text, "Default Number", "Pay Plan in Effect").Trim();
                        string PayEffect = gc.Between(defaulttaxtable.Text, "in Effect", "Annual Payment").Trim();
                        string AnnualPayment = gc.Between(defaulttaxtable.Text, "Annual Payment", "\r\nBalance").Trim();
                        string Balance1 = GlobalClass.After(defaulttaxtable.Text, "Balance\r\n").Trim();
                        string defaulttaxresult = defaulltyear.Trim() + "~" + assessnumber.Trim() + "~" + DefaultNumber.Trim() + "~" + PayEffect.Trim() + "~" + AnnualPayment.Trim() + "~" + Balance1.Trim();
                        gc.insert_date(orderNumber, parcelNumber, 1270, defaulttaxresult, 1, DateTime.Now);
                    }
                    catch { }
                    q++;
                }

                TaxTime = DateTime.Now.ToString("HH:mm:ss");

                LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                gc.insert_TakenTime(orderNumber, "CA", "Stanislaus", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                driver.Quit();
                gc.mergpdf(orderNumber, "CA", "Stanislaus");
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
