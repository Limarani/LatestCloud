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
using System.Web;


namespace ScrapMaricopa.Scrapsource
{

    public class WebDriver_MarinCA
    {

        IWebElement PaidCount;
        IWebDriver driver;
        DBconnection db = new DBconnection();
        List<string> link = new List<string>();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());


        public string FTP_MarinCA(string address, string parcelNumber, string searchType, string orderNumber, string ownername, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;

            GlobalClass.global_parcelNo = parcelNumber;
            List<string> taxinformation = new List<string>();
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                //driver = new ChromeDriver();
                StartTime = DateTime.Now.ToString("HH:mm:ss");
                string Taxauthority = "";
                try

                {


                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, "", ownername, address, "CA", "Marin");
                        if (HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes")
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_MarinCA"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }


                    //Tax Authority
                    try
                    {
                        string Taxauthority1 = "";
                        driver.Navigate().GoToUrl("https://www.marincounty.org/depts/df/property-tax-information");
                        Taxauthority1 = driver.FindElement(By.XPath("//*[@id='main-content']/div[4]/p[5]")).Text.Trim();
                        Taxauthority = gc.Between(Taxauthority1, "Mail:", "Office:").Trim();
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Authority PDF", driver, "CA", "Marin");
                    }
                    catch { }


                    driver.Navigate().GoToUrl("https://www.marincounty.org/depts/ar/divisions/assessor/search-assessor-records");
                    try
                    {

                        try
                        {

                            IWebElement IPropertySearch1 = driver.FindElement(By.XPath("//*[@id='main-content']/div[2]/div[10]/input[1]"));
                            IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                            js1.ExecuteScript("arguments[0].click();", IPropertySearch1);
                        }
                        catch { }
                        try
                        {

                            driver.FindElement(By.XPath("//*[@id='main-content']/div[2]/div[10]/input[1]")).Click();
                        }
                        catch { }


                        if (searchType == "parcel")
                        {
                            driver.FindElement(By.Id("PP")).Clear();
                            IWebElement text = driver.FindElement(By.Id("PP"));
                            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                            js.ExecuteScript("document.getElementById('PP').value='" + parcelNumber + "'");
                            driver.FindElement(By.XPath("//*[@id='P']/div[2]/div/div/input[1]")).SendKeys(Keys.Enter);
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Enter The Address After", driver, "CA", "Marin");

                            try
                            {
                                int count = 0;
                                string Multiparcelnumber = "", Singlerowclick = "";
                                IWebElement Multiaddresstable1add = driver.FindElement(By.XPath("//*[@id='app-ext']/div[1]/div[1]/table/tbody"));
                                IList<IWebElement> multiaddressrows = Multiaddresstable1add.FindElements(By.TagName("tr"));
                                IList<IWebElement> Multiaddressid;
                                foreach (IWebElement Multiaddress in multiaddressrows)
                                {
                                    Multiaddressid = Multiaddress.FindElements(By.TagName("td"));
                                    if (Multiaddressid.Count == 4 && !Multiaddress.Text.Contains("Parcel#") && Multiparcelnumber != Multiaddressid[0].Text)
                                    {
                                        Multiparcelnumber = Multiaddressid[0].Text;
                                        IWebElement Singleclick = Multiaddressid[0].FindElement(By.TagName("a"));
                                        Singlerowclick = Singleclick.GetAttribute("href");
                                        string Ownername = Multiaddressid[1].Text;
                                        string multiaddressresult = Multiparcelnumber + "~" + Ownername;
                                        gc.insert_date(orderNumber, Multiparcelnumber, 1201, multiaddressresult, 1, DateTime.Now);
                                        count++;
                                    }
                                }

                                if (count < 2)
                                {
                                    driver.Navigate().GoToUrl(Singlerowclick);
                                }
                                if (count > 1 && count < 26)
                                {
                                    HttpContext.Current.Session["multiparcel_Marin"] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                                if (count > 25)
                                {
                                    HttpContext.Current.Session["multiparcel_Marin_Maximum"] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }
                            }
                            catch { }
                            try
                            {
                                //No Data Found
                                string nodata = driver.FindElement(By.XPath("//*[@id='main-content']/div[2]/h3")).Text;
                                if (nodata.Contains("No records were found."))
                                {
                                    HttpContext.Current.Session["Nodata_MarinCA"] = "Yes";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                            }
                            catch { }

                        }

                    }
                    catch
                    {
                    }

                    //Property Details
                    string ParcelNumber = "", OwnerName = "", UseCode = "", UseCodeDefinition = "", ConstructionYear = "", TaxRateArea = "", AssessmentCity = "", TotalAssessedValueforTaxRollYear = "", TotalAssessedValueforTaxRollYear1 = "", Land = "", Improvements = "", TotalAssessedvalue = "", Homeowner = "", NetAssessedValueforTaxRollYear = "", NetAssessedValueforTaxRollYear1 = "", TotalAssessedvalue1 = "", LessTotalExemptions = "", NetAssessedvalue = "";
                    //IWebElement propertyinfo0 = driver.FindElement(By.XPath("//*[@id='app-ext']/div[2]/div[2]/div[2]/table/tbody"));
                    //IList<IWebElement> propertyinfo0TR = propertyinfo0.FindElements(By.TagName("tr"));
                    //IList<IWebElement> propertyinfo0TD;


                    IList<IWebElement> propertyinfo0TR0 = driver.FindElements(By.TagName("table"));// table list of Payment History Table

                    foreach (IWebElement Taxinfor00 in propertyinfo0TR0)
                    {
                        if (Taxinfor00.Text.Contains("Parcel Number")/*&& tab.Text.Contains("Land Value:")&& tab.Text.Contains("Levy Name")*/)
                        {
                            IList<IWebElement> ITaxRealRowQ0 = Taxinfor00.FindElements(By.TagName("tr"));
                            IList<IWebElement> ITaxRealTdQ0;
                            foreach (IWebElement ItaxReal0 in ITaxRealRowQ0)
                            {
                                ITaxRealTdQ0 = ItaxReal0.FindElements(By.TagName("td"));
                                if (ITaxRealTdQ0.Count == 3 && !ItaxReal0.Text.Contains("Parcel Number"))
                                {
                                    ParcelNumber = ITaxRealTdQ0[0].Text;

                                }
                            }
                        }

                        if (Taxinfor00.Text.Contains("Owner Name")/*&& tab.Text.Contains("Land Value:")&& tab.Text.Contains("Levy Name")*/)
                        {
                            IList<IWebElement> ITaxRealRowQ1 = Taxinfor00.FindElements(By.TagName("tr"));
                            IList<IWebElement> ITaxRealTdQ1;
                            foreach (IWebElement ItaxReal1 in ITaxRealRowQ1)
                            {
                                ITaxRealTdQ1 = ItaxReal1.FindElements(By.TagName("td"));
                                if (ITaxRealTdQ1.Count == 1 && !ItaxReal1.Text.Contains("Owner Name"))
                                {
                                    OwnerName += ITaxRealTdQ1[0].Text + " ";

                                }
                            }
                        }

                        if (Taxinfor00.Text.Contains("Land") && !Taxinfor00.Text.Contains("Land Sq. Ft.")/* && tab.Text.Contains("Levy Name")*/)
                        {
                            IList<IWebElement> ITaxRealRowQ2 = Taxinfor00.FindElements(By.TagName("tr"));
                            IList<IWebElement> ITaxRealTdQ2;
                            foreach (IWebElement ItaxReal2 in ITaxRealRowQ2)
                            {
                                ITaxRealTdQ2 = ItaxReal2.FindElements(By.TagName("td"));
                                if (ITaxRealTdQ2.Count == 2 && ItaxReal2.Text.Contains("Land"))
                                {
                                    Land = ITaxRealTdQ2[1].Text;
                                    //Improvements = ITaxRealTdQ2[0].Text;
                                    //TotalAssessedvalue = ITaxRealTdQ2[0].Text;
                                }
                                if (ITaxRealTdQ2.Count == 2 && ItaxReal2.Text.Contains("Improvements"))
                                {
                                    //Land = ITaxRealTdQ2[0].Text;
                                    Improvements = ITaxRealTdQ2[1].Text;
                                    //TotalAssessedvalue = ITaxRealTdQ2[0].Text;
                                }
                                if (ITaxRealTdQ2.Count == 2 && ItaxReal2.Text.Contains("Total Assessed Value"))
                                {
                                    //Land = ITaxRealTdQ2[0].Text;
                                    //Improvements = ITaxRealTdQ2[0].Text;
                                    TotalAssessedvalue = ITaxRealTdQ2[1].Text;
                                }
                            }
                        }
                        if (Taxinfor00.Text.Contains("Total Exemptions") && !Taxinfor00.Text.Contains("Less Total Exemptions")/*&& tab.Text.Contains("Land Value:")&& tab.Text.Contains("Levy Name")*/)
                        {
                            IList<IWebElement> ITaxRealRowQ3 = Taxinfor00.FindElements(By.TagName("tr"));
                            IList<IWebElement> ITaxRealTdQ3;
                            foreach (IWebElement ItaxReal3 in ITaxRealRowQ3)
                            {
                                ITaxRealTdQ3 = ItaxReal3.FindElements(By.TagName("td"));
                                if (ITaxRealTdQ3.Count == 2 && ItaxReal3.Text.Contains("Total Exemptions"))
                                {
                                    Homeowner = ITaxRealTdQ3[1].Text;

                                }
                            }
                        }
                        if (Taxinfor00.Text.Contains("Total Assessed Value") && !Taxinfor00.Text.Contains("Land")/*&& tab.Text.Contains("Levy Name")*/)
                        {
                            IList<IWebElement> ITaxRealRowQ4 = Taxinfor00.FindElements(By.TagName("tr"));
                            IList<IWebElement> ITaxRealTdQ4;
                            foreach (IWebElement ItaxReal4 in ITaxRealRowQ4)
                            {
                                ITaxRealTdQ4 = ItaxReal4.FindElements(By.TagName("td"));
                                if (ITaxRealTdQ4.Count == 2 && ItaxReal4.Text.Contains("Total Assessed Value"))
                                {
                                    TotalAssessedvalue1 = ITaxRealTdQ4[1].Text;
                                }
                                if (ITaxRealTdQ4.Count == 2 && ItaxReal4.Text.Contains("Less Total Exemptions"))
                                {
                                    LessTotalExemptions = ITaxRealTdQ4[1].Text;
                                }
                                if (ITaxRealTdQ4.Count == 2 && ItaxReal4.Text.Contains("Net Assessed Value"))
                                {
                                    NetAssessedvalue = ITaxRealTdQ4[1].Text;
                                }
                            }
                        }
                        if (Taxinfor00.Text.Contains("Use Code Definition")/* && tab.Text.Contains("Levy Name")*/)
                        {
                            IList<IWebElement> ITaxRealRowQ5 = Taxinfor00.FindElements(By.TagName("tr"));
                            IList<IWebElement> ITaxRealTdQ5;
                            foreach (IWebElement ItaxReal5 in ITaxRealRowQ5)
                            {
                                ITaxRealTdQ5 = ItaxReal5.FindElements(By.TagName("td"));
                                if (ITaxRealTdQ5.Count == 2 && ITaxRealRowQ5.Count > 6 && ItaxReal5.Text.Contains("Use Code") && !ItaxReal5.Text.Contains("Definition"))
                                {
                                    UseCode = ITaxRealTdQ5[1].Text;
                                }
                                if (ITaxRealTdQ5.Count == 2 && ITaxRealRowQ5.Count > 6 && ItaxReal5.Text.Contains("Use Code Definition"))
                                {
                                    UseCodeDefinition = ITaxRealTdQ5[1].Text;
                                }
                                if (ITaxRealTdQ5.Count == 2 && ITaxRealRowQ5.Count > 6 && ItaxReal5.Text.Contains("Construction Year"))
                                {
                                    ConstructionYear = ITaxRealTdQ5[1].Text;
                                }
                            }
                        }

                        if (Taxinfor00.Text.Contains("Tax Rate Area")/* && tab.Text.Contains("Levy Name")*/)
                        {
                            IList<IWebElement> ITaxRealRowQ6 = Taxinfor00.FindElements(By.TagName("tr"));
                            IList<IWebElement> ITaxRealTdQ6;
                            foreach (IWebElement ItaxReal6 in ITaxRealRowQ6)
                            {
                                ITaxRealTdQ6 = ItaxReal6.FindElements(By.TagName("td"));
                                if (ITaxRealTdQ6.Count == 2 && ItaxReal6.Text.Contains("Tax Rate Area"))
                                {
                                    TaxRateArea = ITaxRealTdQ6[1].Text;
                                }
                                if (ITaxRealTdQ6.Count == 2 && ItaxReal6.Text.Contains("Assessment City"))
                                {
                                    AssessmentCity = ITaxRealTdQ6[1].Text;
                                }
                            }
                        }


                    }
                    try
                    {
                        TotalAssessedValueforTaxRollYear1 = driver.FindElement(By.XPath("//*[@id='main-content']/div[2]/div[12]/div[1]/span")).Text.Trim();
                        TotalAssessedValueforTaxRollYear = GlobalClass.After(TotalAssessedValueforTaxRollYear1, "Total Assessed Value for Tax Roll Year:").Trim();

                    }
                    catch { }
                    try
                    {
                        NetAssessedValueforTaxRollYear1 = driver.FindElement(By.XPath("//*[@id='main-content']/div[2]/div[16]/div[1]/span")).Text.Trim();
                        NetAssessedValueforTaxRollYear = GlobalClass.After(NetAssessedValueforTaxRollYear1, "Net Assessed Value for Tax Roll Year:").Trim();
                    }
                    catch { }
                    //Property Details
                    string PropertyDetails = ParcelNumber.Trim() + "~" + OwnerName.Trim() + "~" + UseCode.Trim() + "~" + UseCodeDefinition.Trim() + "~" + ConstructionYear.Trim() + "~" + TaxRateArea.Trim() + "~" + AssessmentCity.Trim();
                    gc.insert_date(orderNumber, parcelNumber, 1213, PropertyDetails, 1, DateTime.Now);

                    //Assessment Details 
                    string AssessmentDetails = TotalAssessedValueforTaxRollYear.Trim() + "~" + Land.Trim() + "~" + Improvements.Trim() + "~" + TotalAssessedvalue.Trim() + "~" + Homeowner.Trim() + "~" + NetAssessedValueforTaxRollYear.Trim() + "~" + TotalAssessedvalue1.Trim() + "~" + LessTotalExemptions.Trim() + "~" + NetAssessedvalue.Trim() + "~" + Taxauthority.Trim();
                    gc.insert_date(orderNumber, parcelNumber, 1215, AssessmentDetails, 1, DateTime.Now);

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");


                    //Property Tax Bills Details
                    //Last Three Years All Tax Bills Details Information With Screeshot
                    //Tax Values Assessment Details Table
                    string TaxRollYear = "", LandValue = "", Improvements1 = "", BusinessProperty = "", PersonalProperty = "", TotalValue = "", HomeExemption = "", OtherExemption = "", NetValue = "", Taxassesmentvaluebind = "";
                    string Owner = "", ParcelNumber1 = "", BillNumber = "", BillDate = "", TaxRateArea1 = "", TaxRollYear1 = "", BillType = "";
                    string Overallassessmentdetails = "", Taxassesmentvaluebind1 = "", Taxsummaryassesmentvaluebind2 = "";
                    string Levy = "", Name = "", Ratefund = "", Install1 = "", Install2 = "", Total = "";
                    string DefaultYear = "", BillNumber1 = "", BillDate1 = "", BillType1 = "", TaxesandAssmnts = "", DelinqPenalty = "", DelinqCost = "", RedemptionPenalty = "";
                    string Taxyear = "", Billnumber = "", PaymentOwnername = "", Billtype = "", Installpay1 = "", Installpay2 = "";
                    try
                    {


                        driver.Navigate().GoToUrl("http://apps.marincounty.org/taxbillonline");

                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='compwa']/div[4]/input[1]")).Click();
                        }
                        catch { }
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='main-content']/div[2]/div[5]/input[1]")).Click();
                        }
                        catch { }
                        try
                        {
                            IWebElement IPropertySearch1 = driver.FindElement(By.XPath("//*[@id='compwa']/div[4]/input[1]"));
                            IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                            js1.ExecuteScript("arguments[0].click();", IPropertySearch1);
                            Thread.Sleep(5000);
                        }
                        catch { }
                        try
                        {
                            IWebElement IPropertySearch1 = driver.FindElement(By.XPath("//*[@id='main-content']/div[2]/div[5]/input[1]"));
                            IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                            js1.ExecuteScript("arguments[0].click();", IPropertySearch1);
                            Thread.Sleep(5000);
                        }
                        catch { }

                        driver.FindElement(By.Id("PropertyId")).Click();
                        driver.FindElement(By.Id("PropertyId")).Clear();
                        driver.Navigate().GoToUrl("http://apps.marincounty.org/TaxBillOnline/?" + ParcelNumber);
                        driver.FindElement(By.Id("PropertyId")).Clear();
                        IWebElement text = driver.FindElement(By.Id("PropertyId"));
                        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                        js.ExecuteScript("document.getElementById('PropertyId').value='" + ParcelNumber + "'");
                        driver.FindElement(By.XPath("//*[@id='main-content']/div[2]/form/div[2]/div[2]/div/input[1]")).SendKeys(Keys.Enter);

                        gc.CreatePdf(orderNumber, ParcelNumber, "Property Tax Bill Details PDF", driver, "CA", "Marin");
                        //Delinquent Details
                        string delinquenttax = "", InformationComments = "";
                        try
                        {
                            try
                            {
                                delinquenttax = driver.FindElement(By.XPath("//*[@id='compwa']/div[12]/div[1]/div[1]/span")).Text.Trim();
                            }
                            catch { }
                            if (delinquenttax.Contains("Delinquent Unpaid Tax Bills"))
                            {

                                InformationComments = "For tax amount due, you must call the Collector's Office.";
                                string alertmessage = InformationComments;
                                gc.insert_date(orderNumber, ParcelNumber, 1239, alertmessage, 1, DateTime.Now);
                            }
                        }
                        catch { }
                        IWebElement Taxbillclick = driver.FindElement(By.Id("content"));
                        IList<IWebElement> taxpay = Taxbillclick.FindElements(By.TagName("table"));
                        List<string> Singlerowclick = new List<string>();
                        foreach (IWebElement taxpament in taxpay)
                        {
                            if (taxpament.Text.Contains("Tax Year") && !taxpament.Text.Contains("To Pay Online") && !taxpament.Text.Contains("Default Nbr")/*&& tab.Text.Contains("Land Value:")&& tab.Text.Contains("Levy Name")*/)
                            {
                                IList<IWebElement> ITaxRealRowQ = taxpament.FindElements(By.TagName("tr"));
                                IList<IWebElement> ITaxRealTdQ;

                                foreach (IWebElement taxpamentHistory in ITaxRealRowQ)
                                {
                                    ITaxRealTdQ = taxpamentHistory.FindElements(By.TagName("td"));

                                    if (ITaxRealTdQ.Count != 0 && ITaxRealTdQ.Count != 3 && ITaxRealTdQ.Count == 6 && taxpamentHistory.Text.Trim() != "")
                                    {
                                        Taxyear = ITaxRealTdQ[0].Text;
                                        Billnumber = ITaxRealTdQ[1].Text;
                                        PaymentOwnername = ITaxRealTdQ[2].Text;
                                        Billtype = ITaxRealTdQ[3].Text;
                                        Installpay1 = ITaxRealTdQ[4].Text;
                                        Installpay2 = ITaxRealTdQ[5].Text;
                                        //Overall Secured Property Tax Charges
                                        string PaymentHistorydetails = Taxyear.Trim() + "~" + Billnumber.Trim() + "~" + PaymentOwnername.Trim() + "~" + Billtype.Trim() + "~" + Installpay1.Trim() + "~" + Installpay2.Trim();
                                        gc.insert_date(orderNumber, ParcelNumber, 1231, PaymentHistorydetails, 1, DateTime.Now);
                                    }
                                }
                                IList<IWebElement> Taxbillclickrows = taxpament.FindElements(By.TagName("a"));
                                foreach (IWebElement propertytaxclick in Taxbillclickrows)
                                {
                                    if (propertytaxclick.Text != "")
                                    {
                                        Singlerowclick.Add(propertytaxclick.GetAttribute("href"));
                                    }
                                }
                            }
                        }
                        int p = 0;
                        foreach (string Properclick in Singlerowclick)
                        {
                            driver.Navigate().GoToUrl(Properclick);
                            try
                            {
                                driver.FindElement(By.XPath("//*[@id='compwa']/div[4]/input[1]")).Click();
                            }
                            catch { }

                            string Billtypedval = "", TaxBillNumber = "";
                            IList<IWebElement> tableList = driver.FindElements(By.TagName("table"));// table list of Payment History Table

                            foreach (IWebElement tab in tableList)
                            {
                                if (tab.Text.Contains("Owner:")/*&& tab.Text.Contains("Land Value:")&& tab.Text.Contains("Levy Name")*/)
                                {
                                    IList<IWebElement> ITaxRealRowQ = tab.FindElements(By.TagName("tr"));
                                    IList<IWebElement> ITaxRealTdQ;
                                    foreach (IWebElement ItaxReal in ITaxRealRowQ)
                                    {
                                        ITaxRealTdQ = ItaxReal.FindElements(By.TagName("td"));
                                        if (ITaxRealTdQ.Count != 0 && ItaxReal.Text.Trim() != "" && ItaxReal.Text.Contains("Tax Roll Year:"))
                                        {
                                            TaxRollYear = ITaxRealTdQ[1].Text;
                                        }
                                        if (ITaxRealTdQ.Count != 0 && ItaxReal.Text.Trim() != "" && ItaxReal.Text.Contains("Bill Type:"))
                                        {
                                            Billtypedval = ITaxRealTdQ[1].Text;
                                        }
                                        if (ITaxRealTdQ.Count != 0 && ITaxRealTdQ.Count != 2 && ItaxReal.Text.Trim() != "" && ItaxReal.Text.Contains("Bill Number:"))
                                        {
                                            TaxBillNumber = ITaxRealTdQ[1].Text;
                                        }
                                        if (ITaxRealTdQ.Count != 0 && ItaxReal.Text.Trim() != "" && tab.Text.Contains("Owner:"))
                                        {
                                            Overallassessmentdetails += ITaxRealTdQ[1].Text + "~";
                                        }
                                    }
                                    //Overall Secured Property Tax Bill Details Table1
                                    string OverallTaxValuesAssessmentDetails = Overallassessmentdetails.TrimEnd('~').Trim();
                                    gc.insert_date(orderNumber, ParcelNumber, 1221, OverallTaxValuesAssessmentDetails, 1, DateTime.Now);
                                }

                            }
                            gc.CreatePdf(orderNumber, ParcelNumber, "Property Tax Bill Details Yearwise PDF" + p, driver, "CA", "Marin");



                            IList<IWebElement> tableList1 = driver.FindElements(By.TagName("table"));// table list of Payment History Table

                            foreach (IWebElement tab1 in tableList1)
                            {
                                if (tab1.Text.Contains("Land Value:")/*&& tab.Text.Contains("Land Value:")&& tab.Text.Contains("Levy Name")*/)
                                {
                                    IList<IWebElement> ITaxRealRowQ1 = tab1.FindElements(By.TagName("tr"));
                                    IList<IWebElement> ITaxRealTdQ1;
                                    foreach (IWebElement ItaxReal1 in ITaxRealRowQ1)
                                    {
                                        ITaxRealTdQ1 = ItaxReal1.FindElements(By.TagName("td"));

                                        if (ITaxRealTdQ1.Count != 0 && ItaxReal1.Text.Trim() != "" && tab1.Text.Contains("Land Value:"))
                                        {
                                            Taxassesmentvaluebind += ITaxRealTdQ1[1].Text + "~";
                                        }

                                    }
                                    //Tax Values Assessment Details Table:
                                    string TaxValuesAssessmentDetails = TaxRollYear.Trim() + "~" + Taxassesmentvaluebind.TrimEnd('~').Trim();
                                    gc.insert_date(orderNumber, ParcelNumber, 1220, TaxValuesAssessmentDetails, 1, DateTime.Now);
                                }
                            }
                            IList<IWebElement> tableList2 = driver.FindElements(By.TagName("table"));// table list of Payment History Table

                            foreach (IWebElement tab2 in tableList2)
                            {
                                if (tab2.Text.Contains("Levy Name Rate/Fund Install 1 Install 2 Total")/*&& tab.Text.Contains("Land Value:")&& tab.Text.Contains("Levy Name")*/)
                                {
                                    IList<IWebElement> ITaxRealRowQ2 = tab2.FindElements(By.TagName("tr"));
                                    IList<IWebElement> ITaxRealTdQ2;
                                    foreach (IWebElement ItaxReal2 in ITaxRealRowQ2)
                                    {
                                        ITaxRealTdQ2 = ItaxReal2.FindElements(By.TagName("td"));

                                        if (ITaxRealTdQ2.Count != 0 && ItaxReal2.Text.Trim() != "" && ITaxRealTdQ2.Count == 6)
                                        {
                                            Levy = ITaxRealTdQ2[0].Text;
                                            Name = ITaxRealTdQ2[1].Text;
                                            if (!Name.Contains("BASIC TAX") && !Name.Contains("Total Tax:") && !Name.Contains("Penalty:") && !Name.Contains("Additional Penalty:") && !Name.Contains("Amount Paid:") && !Name.Contains("Delinquent Date:") && !Name.Contains("Paid Date:") && !Name.Contains("Total Due:"))
                                            {
                                                Name = Name.Remove(0, 1);
                                            }
                                            Ratefund = ITaxRealTdQ2[2].Text;
                                            Install1 = ITaxRealTdQ2[3].Text;
                                            Install2 = ITaxRealTdQ2[4].Text;
                                            Total = ITaxRealTdQ2[5].Text;
                                            //Overall Secured Property Tax Charges
                                            string OverallTaxValuesAssessmentDetails2 = TaxRollYear + "~" + Billtypedval + "~" + Levy.Trim() + "~" + Name.Trim() + "~" + Ratefund.Trim() + "~" + Install1.Trim() + "~" + Install2.Trim() + "~" + Total.Trim();
                                            gc.insert_date(orderNumber, ParcelNumber, 1230, OverallTaxValuesAssessmentDetails2, 1, DateTime.Now);
                                        }
                                    }
                                }
                            }
                            //Tax Summary of Prior Years Details
                            IList<IWebElement> tableList3 = driver.FindElements(By.TagName("table"));// table list of Payment History Table

                            foreach (IWebElement tab3 in tableList3)
                            {
                                if (tab3.Text.Contains("Default Year")/*&& tab.Text.Contains("Land Value:")&& tab.Text.Contains("Levy Name")*/)
                                {
                                    IList<IWebElement> ITaxRealRowQ3 = tab3.FindElements(By.TagName("tr"));
                                    IList<IWebElement> ITaxRealTdQ3;
                                    foreach (IWebElement ItaxReal3 in ITaxRealRowQ3)
                                    {
                                        ITaxRealTdQ3 = ItaxReal3.FindElements(By.TagName("td"));

                                        if (ITaxRealTdQ3.Count != 0 && ItaxReal3.Text.Trim() != "" && ITaxRealTdQ3.Count == 8)
                                        {
                                            DefaultYear = ITaxRealTdQ3[0].Text;
                                            BillNumber1 = ITaxRealTdQ3[1].Text;
                                            BillDate1 = ITaxRealTdQ3[2].Text;
                                            BillType1 = ITaxRealTdQ3[3].Text;
                                            TaxesandAssmnts = ITaxRealTdQ3[4].Text;
                                            DelinqPenalty = ITaxRealTdQ3[5].Text;
                                            DelinqCost = ITaxRealTdQ3[6].Text;
                                            RedemptionPenalty = ITaxRealTdQ3[7].Text;

                                            //Tax Summary Prior Year Detaisl
                                            string Taxsummaryofprioryeardetails = DefaultYear.Trim() + "~" + BillNumber1.Trim() + "~" + BillDate1.Trim() + "~" + BillType1.Trim() + "~" + TaxesandAssmnts.Trim() + "~" + DelinqPenalty.Trim() + "~" + DelinqCost.Trim() + "~" + RedemptionPenalty.Trim();
                                            gc.insert_date(orderNumber, ParcelNumber, 1227, Taxsummaryofprioryeardetails, 1, DateTime.Now);
                                        }
                                    }
                                }
                            }

                            //Delinquent Tax Details
                            IList<IWebElement> tableList4 = driver.FindElements(By.TagName("table"));
                            foreach (IWebElement tab4 in tableList4)
                            {

                                if (tableList4.Count != 2 && tab4.Text.Contains("Owner:") && !tab4.Text.Contains("Delinquent Details") && !tab4.Text.Contains("Bill"))
                                {
                                    IList<IWebElement> ITaxRealRowQ4 = tab4.FindElements(By.TagName("tr"));
                                    IList<IWebElement> ITaxRealTdQ4;
                                    foreach (IWebElement ItaxReal4 in ITaxRealRowQ4)
                                    {
                                        ITaxRealTdQ4 = ItaxReal4.FindElements(By.TagName("td"));

                                        if (ITaxRealTdQ4.Count != 0 && ItaxReal4.Text.Trim() != "" && ITaxRealTdQ4.Count == 2 /*&& !ItaxReal4.Text.Contains("Default")*/)
                                        {
                                            string Totaldelinquent = ITaxRealTdQ4[0].Text + "~" + ITaxRealTdQ4[1].Text;

                                            gc.insert_date(orderNumber, ParcelNumber, 1350, Totaldelinquent, 1, DateTime.Now);
                                        }
                                    }
                                }
                            }
                            IList<IWebElement> tableList5 = driver.FindElements(By.TagName("table"));
                            foreach (IWebElement tab5 in tableList5)
                            {
                                if (!tab5.Text.Contains("Payment Due") && tab5.Text.Contains("Description"))
                                {
                                    IList<IWebElement> ITaxRealRowQ5 = tab5.FindElements(By.TagName("tr"));
                                    IList<IWebElement> ITaxRealTdQ5;
                                    foreach (IWebElement ItaxReal5 in ITaxRealRowQ5)
                                    {
                                        ITaxRealTdQ5 = ItaxReal5.FindElements(By.TagName("td"));

                                        if (ITaxRealTdQ5.Count != 0 && ItaxReal5.Text.Trim() != "" && ITaxRealTdQ5.Count == 2)
                                        {
                                            string Totaldelinquent = ITaxRealTdQ5[0].Text + "~" + ITaxRealTdQ5[1].Text;

                                            gc.insert_date(orderNumber, ParcelNumber, 1350, Totaldelinquent, 1, DateTime.Now);
                                        }
                                    }
                                }
                            }
                            TaxRollYear = ""; LandValue = ""; Improvements1 = ""; BusinessProperty = ""; PersonalProperty = ""; TotalValue = ""; HomeExemption = ""; OtherExemption = ""; NetValue = ""; Taxassesmentvaluebind = "";
                            Owner = ""; ParcelNumber1 = ""; BillNumber = ""; BillDate = ""; TaxRateArea1 = ""; TaxRollYear1 = ""; BillType = "";
                            Overallassessmentdetails = ""; Taxassesmentvaluebind1 = "";
                            Levy = ""; Name = ""; Ratefund = ""; Install1 = ""; Install2 = ""; Total = "";
                            DefaultYear = ""; BillNumber1 = ""; BillDate1 = ""; BillType1 = ""; TaxesandAssmnts = ""; DelinqPenalty = ""; DelinqCost = ""; RedemptionPenalty = "";

                            p++;

                        }
                    }
                    catch { }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "CA", "Marin", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    GlobalClass.titleparcel = "";
                    gc.mergpdf(orderNumber, "CA", "Marin");
                    return "Data Inserted Successfully";
                }
                catch (Exception ex)
                {
                    driver.Quit();
                    throw ex;
                }
            }
        }
    }
}