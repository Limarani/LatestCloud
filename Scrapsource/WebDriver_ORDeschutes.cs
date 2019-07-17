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
using iTextSharp.text.pdf;
using iTextSharp;
using iTextSharp.text.pdf.parser;
using System.Text;

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_ORDeschutes
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        string OwnerName = "-", Property_Address = "-", City = "-", Subdivision = "-", Property_Type = "-", Multidata = "-";
        string Mailing_Name = "-", Map_Tax = "-", Account_Number = "-", Situs_Address = "-", Tax_Status = "-", Assessor_Property_Description = "-", Assessor_Acres = "-", Property_Class = "-", Year_Built = "-";
        string Special_Description = "-", Special_Amount = "-", Special_Year = "-";
        string Assemnt_Details1 = "-", Assemnt_Details2 = "-", Assemnt_Details3 = "-", Assemnt_Details4 = "-", Assemnt_Details5 = "-", Special_details = "-", Property_Deatail = "-";
        string Tax_Code = "-", TaxYear_2018 = "-", TaxYear_2014 = "-", TaxYear_2015 = "-", TaxYear_2016 = "-", TaxYear_2017 = "-", Tax_Authority = "-", Tax_details = "-";
        string Payment_Year = "", Date_Due = "", Transaction_Type = "", Transaction_Date = "", AsOf_Date = "", Amount_Received = "", Tax_Due = "", Discount_Amount = "", Interest_Charged = "", Refund_Interest = "", Payment_Details = "";
        string CurrentTaxBalance = "", FilePath = "", Acct_sttus = "", Rol_Typ = "", Situs = "", Id_situs = "", Ints_Id = "", TaxSummary_details = "", tableassess = "", newrow = "", newrow1 = "", pdftext = "";
        string newrow3 = "", newrow4 = "", a3 = "", tableassess2 = "", pdftext1 = "", FilePath1 = "", a1 = "", Tax_Statemet1 = "", Tax_Statemet2 = "";
        string year = "", tableassess3 = "", line1 = "", line2 = "", line3 = "", filename = "", newrow10 = "", newrow11 = "";

        public string FTP_ORDeschutes(string address, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            GlobalClass.sname = "OR";
            GlobalClass.cname = "Deschutes";
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //  driver = new ChromeDriver();;

            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "OR", "Deschutes");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_ORDeschutes"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("http://dial.deschutes.org/");
                        Thread.Sleep(2000);

                        driver.FindElement(By.XPath("/html/body/div[1]/div[1]/ul/li[6]/a")).Click();
                        Thread.Sleep(2000);

                        driver.FindElement(By.Id("value")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "OR", "Deschutes");

                        driver.FindElement(By.XPath("/html/body/div[1]/div[2]/form/input[1]")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Address search Result", driver, "OR", "Deschutes");

                        //MultiParcel
                        try
                        {
                            IWebElement MultiTable = driver.FindElement(By.XPath("/html/body/div[1]/div[2]/div[2]/div[4]/div/table/tbody"));
                            IList<IWebElement> MultiTR = MultiTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiTD;
                            gc.CreatePdf_WOP(orderNumber, "MultiAddresssearch", driver, "OR", "Deschutes");
                            foreach (IWebElement Multi in MultiTR)
                            {
                                MultiTD = Multi.FindElements(By.TagName("td"));
                                if (MultiTD.Count != 0 && !Multi.Text.Contains("Personal"))
                                {
                                    parcelNumber = MultiTD[2].Text;
                                    OwnerName = MultiTD[3].Text;
                                    Property_Address = MultiTD[4].Text;
                                    City = MultiTD[5].Text;
                                    Subdivision = MultiTD[6].Text;
                                    Property_Type = MultiTD[7].Text;
                                    Multidata = OwnerName + "~" + Property_Address + "~" + City + "~" + Subdivision + "~" + Property_Type;
                                    gc.insert_date(orderNumber, parcelNumber, 272, Multidata, 1, DateTime.Now);
                                }
                            }
                            driver.Quit();
                            HttpContext.Current.Session["multiParcel_ORDeschutes"] = "Yes";

                            if (MultiTR.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_ORDeschutes_Multicount"] = "Maximum";
                            }
                            return "MultiParcel";
                        }
                        catch
                        { }
                    }

                    else if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("http://dial.deschutes.org/");
                        Thread.Sleep(2000);

                        driver.FindElement(By.XPath("/html/body/div[1]/div[1]/ul/li[4]/a")).Click();
                        Thread.Sleep(2000);

                        driver.FindElement(By.Id("value")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearch", driver, "OR", "Deschutes");
                        driver.FindElement(By.XPath("/html/body/div[1]/div[2]/form/input[1]")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                    }

                    else if (searchType == "ownername")
                    {
                        driver.Navigate().GoToUrl("http://dial.deschutes.org/");
                        Thread.Sleep(2000);

                        driver.FindElement(By.XPath("/html/body/div[1]/div[1]/ul/li[3]/a")).Click();
                        Thread.Sleep(2000);
                        driver.FindElement(By.Id("value")).SendKeys(ownername);
                        gc.CreatePdf(orderNumber, parcelNumber, "OwnerSearch", driver, "OR", "Deschutes");
                        driver.FindElement(By.XPath("/html/body/div[1]/div[2]/form/input[1]")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        //MultiParcel
                        try
                        {
                            IWebElement MultiTable = driver.FindElement(By.XPath("/html/body/div[1]/div[2]/div[2]/div[4]/div/table/tbody"));
                            IList<IWebElement> MultiTR = MultiTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiTD;
                            gc.CreatePdf(orderNumber, parcelNumber, "MultiOwnersearch", driver, "OR", "Deschutes");
                            foreach (IWebElement Multi in MultiTR)
                            {
                                MultiTD = Multi.FindElements(By.TagName("td"));
                                if (MultiTD.Count != 0 && !Multi.Text.Contains("Personal"))
                                {
                                    parcelNumber = MultiTD[2].Text;
                                    OwnerName = MultiTD[3].Text;
                                    Property_Address = MultiTD[4].Text;
                                    City = MultiTD[5].Text;
                                    Subdivision = MultiTD[6].Text;
                                    Property_Type = MultiTD[7].Text;
                                    Multidata = OwnerName + "~" + Property_Address + "~" + City + "~" + Subdivision + "~" + Property_Type;
                                    gc.insert_date(orderNumber, parcelNumber, 272, Multidata, 1, DateTime.Now);
                                }
                            }
                            HttpContext.Current.Session["multiParcel_ORDeschutes"] = "Yes";
                            if (MultiTR.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_ORDeschutes_Multicount"] = "Maximum";
                            }
                            driver.Quit();
                            return "MultiParcel";
                        }
                        catch
                        { }
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.XPath("//*[@id='content-app']/div[2]"));
                        if(INodata.Text.Contains("returned no matches"))
                        {
                            HttpContext.Current.Session["Nodata_ORDeschutes"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    //Property_Details
                    try
                    {
                        Mailing_Name = driver.FindElement(By.XPath("/html/body/div[1]/div[3]/div[2]/div[2]/div[1]/p[2]")).Text;
                        Mailing_Name = gc.Between(Mailing_Name, "Mailing Name:", "Map and Taxlot:").Trim();

                        Map_Tax = driver.FindElement(By.Id("uxMapTaxlot")).Text;

                        Account_Number = driver.FindElement(By.XPath("/html/body/div[1]/div[3]/div[2]/div[2]/div[1]/p[2]")).Text;
                        Account_Number = gc.Between(Account_Number, "Account:", "Situs Address:").Trim();

                        Situs_Address = driver.FindElement(By.Id("uxSitusAddress")).Text;

                        Tax_Status = driver.FindElement(By.XPath("/html/body/div[1]/div[3]/div[2]/div[2]/div[1]/p[2]")).Text;
                        Tax_Status = WebDriverTest.After(Tax_Status, "Tax Status:").Trim();

                        Assessor_Property_Description = driver.FindElement(By.XPath("/html/body/div[1]/div[3]/div[2]/div[2]/div[2]/p[4]/strong[2]/a")).Text;

                        Assessor_Acres = driver.FindElement(By.XPath("/html/body/div[1]/div[3]/div[2]/div[2]/div[2]/p[5]")).Text;
                        Assessor_Acres = gc.Between(Assessor_Acres, "Assessor Acres:", "Property Class:").Trim();

                        Property_Class = driver.FindElement(By.XPath("/html/body/div[1]/div[3]/div[2]/div[2]/div[2]/p[5]")).Text;
                        Property_Class = WebDriverTest.After(Property_Class, "Property Class:").Trim();

                        gc.CreatePdf(orderNumber, Account_Number, "Property Details", driver, "OR", "Deschutes");

                        IWebElement ValueTable = driver.FindElement(By.XPath("/html/body/div[1]/div[3]/div[2]/div[2]/div[3]/table[1]/tbody"));
                        Thread.Sleep(2000);
                        IList<IWebElement> ValueTR = ValueTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> ValueTD;
                        List<string> Land = new List<string>();
                        List<string> Structures = new List<string>();
                        List<string> Total = new List<string>();

                        int i = 0;
                        foreach (IWebElement Value in ValueTR)
                        {
                            ValueTD = Value.FindElements(By.TagName("td"));
                            if (i == 0)
                            {
                                Land.Add(ValueTD[1].Text);
                            }
                            else if (i == 1)
                            {
                                Structures.Add(ValueTD[1].Text);
                            }
                            else if (i == 2)
                            {
                                Total.Add(ValueTD[1].Text);
                            }
                            i++;
                        }

                        IWebElement AssessedTable = driver.FindElement(By.XPath("/html/body/div[1]/div[3]/div[2]/div[2]/div[3]/table[2]/tbody"));
                        IList<IWebElement> AssessedTR = AssessedTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> AssessedTD;

                        List<string> Maximum_Assessed = new List<string>();
                        List<string> Assessed_Value = new List<string>();
                        List<string> Veterans_Exemption = new List<string>();

                        int j = 0;
                        foreach (IWebElement Assessed in AssessedTR)
                        {
                            AssessedTD = Assessed.FindElements(By.TagName("td"));
                            if (j == 0)
                            {
                                Maximum_Assessed.Add(AssessedTD[1].Text);
                            }
                            else if (j == 1)
                            {
                                Assessed_Value.Add(AssessedTD[1].Text);
                            }
                            else if (j == 2)
                            {
                                Veterans_Exemption.Add(AssessedTD[1].Text);
                            }
                            j++;
                        }


                        driver.FindElement(By.XPath("/html/body/div[1]/div[2]/ul/li[2]/ul/li[5]/a")).Click();
                        Thread.Sleep(2000);
                        Year_Built = driver.FindElement(By.XPath("/html/body/div[1]/div[3]/div[2]/div[2]/table[1]/tbody/tr/td[3]")).Text;

                        Property_Deatail = Mailing_Name + "~" + Map_Tax + "~" + Account_Number + "~" + Situs_Address + "~" + Tax_Status + "~" + Assessor_Property_Description + "~" + Assessor_Acres + "~" + Property_Class + "~" + Land[0] + "~" + Structures[0] + "~" + Total[0] + "~" + Maximum_Assessed[0] + "~" + Assessed_Value[0] + "~" + Veterans_Exemption[0] + "~" + Year_Built;
                        gc.insert_date(orderNumber, Account_Number, 273, Property_Deatail, 1, DateTime.Now);
                    }
                    catch
                    { }

                    //Assement_Details
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div[2]/ul/li[2]/ul/li[2]/a")).Click();
                        Thread.Sleep(2000);

                        IWebElement AssementTable = driver.FindElement(By.XPath("/html/body/div[1]/div[3]/div[2]/div[2]/table/tbody"));
                        Thread.Sleep(4000);
                        IList<IWebElement> AssementTR = AssementTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> AssementTH;
                        IList<IWebElement> AssementTD;
                        gc.CreatePdf(orderNumber, Account_Number, "Assement Details", driver, "OR", "Deschutes");
                        List<string> Value_History = new List<string>();
                        List<string> RealMarketValue_Land = new List<string>();
                        List<string> RealMarketValue_Structures = new List<string>();
                        List<string> TotalRealMarket_Value = new List<string>();
                        List<string> MaximumAssessed_Value = new List<string>();
                        List<string> TotalAssessed_Value = new List<string>();
                        List<string> VeteransExemption_Value = new List<string>();

                        int k = 0;
                        foreach (IWebElement Assement in AssementTR)
                        {

                            AssementTD = Assement.FindElements(By.TagName("td"));

                            if (k == 0)
                            {
                                AssementTH = Assement.FindElements(By.TagName("th"));
                                Value_History.Add(AssementTH[1].Text);
                                Value_History.Add(AssementTH[2].Text);
                                Value_History.Add(AssementTH[3].Text);
                                Value_History.Add(AssementTH[4].Text);
                                Value_History.Add(AssementTH[5].Text);
                            }
                            else if (k == 1)
                            {
                                RealMarketValue_Land.Add(AssementTD[1].Text);
                                RealMarketValue_Land.Add(AssementTD[2].Text);
                                RealMarketValue_Land.Add(AssementTD[3].Text);
                                RealMarketValue_Land.Add(AssementTD[4].Text);
                                RealMarketValue_Land.Add(AssementTD[5].Text);
                            }
                            else if (k == 2)
                            {
                                RealMarketValue_Structures.Add(AssementTD[1].Text);
                                RealMarketValue_Structures.Add(AssementTD[2].Text);
                                RealMarketValue_Structures.Add(AssementTD[3].Text);
                                RealMarketValue_Structures.Add(AssementTD[4].Text);
                                RealMarketValue_Structures.Add(AssementTD[5].Text);
                            }
                            else if (k == 3)
                            {
                                TotalRealMarket_Value.Add(AssementTD[1].Text);
                                TotalRealMarket_Value.Add(AssementTD[2].Text);
                                TotalRealMarket_Value.Add(AssementTD[3].Text);
                                TotalRealMarket_Value.Add(AssementTD[4].Text);
                                TotalRealMarket_Value.Add(AssementTD[5].Text);
                            }
                            else if (k == 4)
                            {
                                MaximumAssessed_Value.Add(AssementTD[1].Text);
                                MaximumAssessed_Value.Add(AssementTD[2].Text);
                                MaximumAssessed_Value.Add(AssementTD[3].Text);
                                MaximumAssessed_Value.Add(AssementTD[4].Text);
                                MaximumAssessed_Value.Add(AssementTD[5].Text);
                            }
                            else if (k == 5)
                            {
                                TotalAssessed_Value.Add(AssementTD[1].Text);
                                TotalAssessed_Value.Add(AssementTD[2].Text);
                                TotalAssessed_Value.Add(AssementTD[3].Text);
                                TotalAssessed_Value.Add(AssementTD[4].Text);
                                TotalAssessed_Value.Add(AssementTD[5].Text);
                            }
                            else if (k == 6)
                            {
                                VeteransExemption_Value.Add(AssementTD[1].Text);
                                VeteransExemption_Value.Add(AssementTD[2].Text);
                                VeteransExemption_Value.Add(AssementTD[3].Text);
                                VeteransExemption_Value.Add(AssementTD[4].Text);
                                VeteransExemption_Value.Add(AssementTD[5].Text);
                            }
                            k++;
                        }
                        Assemnt_Details1 = Value_History[0] + "~" + RealMarketValue_Land[0] + "~" + RealMarketValue_Structures[0] + "~" + TotalRealMarket_Value[0] + "~" + MaximumAssessed_Value[0] + "~" + TotalAssessed_Value[0] + "~" + VeteransExemption_Value[0];
                        Assemnt_Details2 = Value_History[1] + "~" + RealMarketValue_Land[1] + "~" + RealMarketValue_Structures[1] + "~" + TotalRealMarket_Value[1] + "~" + MaximumAssessed_Value[1] + "~" + TotalAssessed_Value[1] + "~" + VeteransExemption_Value[1];
                        Assemnt_Details3 = Value_History[2] + "~" + RealMarketValue_Land[2] + "~" + RealMarketValue_Structures[2] + "~" + TotalRealMarket_Value[2] + "~" + MaximumAssessed_Value[2] + "~" + TotalAssessed_Value[2] + "~" + VeteransExemption_Value[2];
                        Assemnt_Details4 = Value_History[3] + "~" + RealMarketValue_Land[3] + "~" + RealMarketValue_Structures[3] + "~" + TotalRealMarket_Value[3] + "~" + MaximumAssessed_Value[3] + "~" + TotalAssessed_Value[3] + "~" + VeteransExemption_Value[3];
                        Assemnt_Details5 = Value_History[4] + "~" + RealMarketValue_Land[4] + "~" + RealMarketValue_Structures[4] + "~" + TotalRealMarket_Value[4] + "~" + MaximumAssessed_Value[4] + "~" + TotalAssessed_Value[4] + "~" + VeteransExemption_Value[4];
                        gc.insert_date(orderNumber, Account_Number, 282, Assemnt_Details1, 1, DateTime.Now);
                        gc.insert_date(orderNumber, Account_Number, 282, Assemnt_Details2, 1, DateTime.Now);
                        gc.insert_date(orderNumber, Account_Number, 282, Assemnt_Details3, 1, DateTime.Now);
                        gc.insert_date(orderNumber, Account_Number, 282, Assemnt_Details4, 1, DateTime.Now);
                        gc.insert_date(orderNumber, Account_Number, 282, Assemnt_Details5, 1, DateTime.Now);
                    }
                    catch
                    { }
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    //Tax Information
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div[2]/ul/li[2]/ul/li[3]/a")).Click();
                        Thread.Sleep(2000);
                        int ye = 0;
                        Tax_Code = driver.FindElement(By.XPath("/html/body/div[1]/div[3]/div[2]/div[2]/div[2]/div[1]/p[3]")).Text;
                        Tax_Code = WebDriverTest.After(Tax_Code, "Tax Code Area: ").Trim();

                        IWebElement taxyeartable = driver.FindElement(By.XPath("//*[@id='results-data']/div[4]/table[1]/tbody"));
                        IList<IWebElement> Taxyearrow = taxyeartable.FindElements(By.TagName("tr"));
                        IList<IWebElement> Taxyeartd;
                        foreach (IWebElement Taxyear in Taxyearrow)
                        {
                            Taxyeartd = Taxyear.FindElements(By.TagName("td"));
                            if (Taxyeartd.Count != 0 && ye == 0)
                            {
                                TaxYear_2016 = Taxyeartd[0].Text;
                                TaxYear_2017 = Taxyeartd[1].Text;
                                TaxYear_2018 = Taxyeartd[1].Text;
                                ye++;
                            }

                        }
                        gc.CreatePdf(orderNumber, Account_Number, "Tax Details", driver, "OR", "Deschutes");

                        IWebElement TBPayment = driver.FindElement(By.XPath("/html/body/div[1]/div[3]/div[2]/div[2]/div[4]/table[2]/tbody"));
                        Thread.Sleep(2000);
                        IList<IWebElement> TRPayment = TBPayment.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDPayment;

                        foreach (IWebElement PaymentTax in TRPayment)
                        {
                            TDPayment = PaymentTax.FindElements(By.TagName("td"));
                            if (TDPayment.Count != 0 && !PaymentTax.Text.Contains("Year") && !PaymentTax.Text.Contains("Total:"))
                            {
                                Payment_Year = TDPayment[0].Text;
                                Date_Due = TDPayment[1].Text;
                                Transaction_Type = TDPayment[2].Text;
                                Transaction_Date = TDPayment[3].Text;
                                AsOf_Date = TDPayment[4].Text;
                                Amount_Received = TDPayment[5].Text;
                                Tax_Due = TDPayment[6].Text;
                                Discount_Amount = TDPayment[7].Text;
                                Interest_Charged = TDPayment[8].Text;
                                Refund_Interest = TDPayment[9].Text;

                                Payment_Details = Payment_Year + "~" + Date_Due + "~" + Transaction_Type + "~" + Transaction_Date + "~" + AsOf_Date + "~" + Amount_Received + "~" + Tax_Due + "~" + Discount_Amount + "~" + Interest_Charged + "~" + Refund_Interest;
                                gc.insert_date(orderNumber, Account_Number, 292, Payment_Details, 1, DateTime.Now);
                            }
                            if (TDPayment.Count == 3)
                            {
                                string Total1 = TDPayment[0].Text;
                                string Total1_Value = TDPayment[1].Text;

                                string Payment_Details1 = "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + Total1 + "~" + Total1_Value + "~" + "" + "~" + "" + "~" + "";
                                gc.insert_date(orderNumber, Account_Number, 292, Payment_Details1, 1, DateTime.Now);
                            }
                        }

                        try
                        {
                            //Download Pdf files
                            IWebElement CurrentBalance = driver.FindElement(By.XPath("/html/body/div[1]/div[3]/div[2]/div[2]/div[2]/div[1]/p[2]/a"));
                            CurrentTaxBalance = CurrentBalance.GetAttribute("href");
                            gc.downloadfile(CurrentTaxBalance, orderNumber, Account_Number, "Current_Balance", "OR", "Deschutes");

                            FilePath = gc.filePath(orderNumber, Account_Number) + "Current_Balance.pdf";
                            PdfReader reader;
                            reader = new PdfReader(FilePath);
                            String textFromPage = PdfTextExtractor.GetTextFromPage(reader, 1);
                            System.Diagnostics.Debug.WriteLine("" + textFromPage);

                            pdftext = textFromPage;

                            try
                            {
                                Acct_sttus = gc.Between(pdftext, "Account Status ", " Loan Number");
                                Rol_Typ = gc.Between(pdftext, "Roll Type ", " Property");
                                Situs = gc.Between(pdftext, "Address ", " Interest");
                                Id_situs = gc.Between(pdftext, "ID ", "Situs");
                                Ints_Id = gc.Between(pdftext, "To ", "Tax Summary");

                                TaxSummary_details = Acct_sttus + "~" + Rol_Typ + "~" + Situs + "~" + Id_situs + "~" + Ints_Id;
                                gc.insert_date(orderNumber, Account_Number, 898, TaxSummary_details, 1, DateTime.Now);
                            }
                            catch
                            { }

                            tableassess = GlobalClass.After(pdftext, "Due Date").Trim();
                            string[] tableArray = tableassess.Split('\n');
                            List<string> rowarray1 = new List<string>();
                            int i = 0, j = 0, k = 0, y = 0, w = 0;
                            int count1 = tableArray.Length;
                            for (i = 0; i < count1; i++)
                            {
                                a1 = tableArray[i].Replace(" ", "~");
                                string[] rowarray = a1.Split('~');
                                rowarray1.AddRange(rowarray);
                                if (rowarray1.Count != 5 & rowarray1.Count != 3 && rowarray1.Count != 2)
                                {
                                    newrow = rowarray1[j] + "~" + rowarray1[j + 1] + "~" + rowarray1[j + 2] + "~" + rowarray1[j + 3] + "~" + rowarray1[j + 4] + "~" + rowarray1[j + 5] + "~" + rowarray1[j + 6] + "~" + rowarray1[j + 7] + " " + rowarray1[j + 8] + " " + rowarray1[j + 9];
                                    gc.insert_date(orderNumber, Account_Number, 896, newrow, 1, DateTime.Now);
                                    rowarray1.Clear();
                                }
                                if (rowarray1.Count == 5)
                                {
                                    newrow1 = "" + "~" + rowarray1[k] + "~" + rowarray1[k + 1] + "~" + rowarray1[k + 2] + "~" + rowarray1[k + 3] + "~" + rowarray1[k + 4] + "~" + "" + "~" + "" + " " + "" + " " + "";
                                    gc.insert_date(orderNumber, Account_Number, 896, newrow1, 1, DateTime.Now);
                                }
                                if (rowarray1.Count == 3)
                                {
                                    newrow11 = " " + "~" + "Total" + "~" + rowarray1[y] + "~" + rowarray1[y + 1] + "~" + "$0.00" + "~" + rowarray1[y + 2] + "~" + "" + "~" + "" + " " + "" + " " + "";
                                    gc.insert_date(orderNumber, Account_Number, 896, newrow11, 1, DateTime.Now);
                                    rowarray1.Clear();
                                }
                            }

                            try
                            {
                                string FinalRow = newrow10.Replace("", "") + "~" + newrow11 + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + " " + "" + " " + "";
                                gc.insert_date(orderNumber, Account_Number, 896, FinalRow, 1, DateTime.Now);
                            }
                            catch
                            { }

                            //try
                            //{
                            //    IWebElement Tax_State1 = driver.FindElement(By.XPath("//*[@id='results-data']/div[4]/table[1]/tbody/tr[3]/td[3]/a"));
                            //    Tax_Statemet1 = Tax_State1.GetAttribute("href");
                            //    gc.downloadfile(Tax_Statemet1, orderNumber, Account_Number, "Tax Statemet 2015", "OR", "Deschutes");

                            //    IWebElement Tax_State2 = driver.FindElement(By.XPath("//*[@id='results-data']/div[4]/table[1]/tbody/tr[3]/td[4]/a"));
                            //    Tax_Statemet2 = Tax_State2.GetAttribute("href");
                            //    gc.downloadfile(Tax_Statemet2, orderNumber, Account_Number, "Tax Statemet 2016", "OR", "Deschutes");
                            //}
                            //catch
                            //{ }

                            List<string> urlListTaxBills = new List<string>();

                            IWebElement TaxStateTB = driver.FindElement(By.XPath("/html/body/div[1]/div[3]/div[2]/div[2]/div[4]/table[1]/tbody"));
                            IList<IWebElement> TaxStateTR = TaxStateTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> TaxStateA;

                            foreach (IWebElement TaxState in TaxStateTR)
                            {
                                TaxStateA = TaxState.FindElements(By.TagName("a"));
                                if (TaxStateA.Count != 0)
                                {
                                    urlListTaxBills.Add(TaxStateA[0].GetAttribute("href"));
                                    urlListTaxBills.Add(TaxStateA[1].GetAttribute("href"));
                                    urlListTaxBills.Add(TaxStateA[2].GetAttribute("href"));
                                }
                            }

                            try
                            {
                                int z = 0;
                                int bill = 0;
                                foreach (string sewer in urlListTaxBills)
                                {
                                    if (z == 0 || z == 1 || z == 2)
                                    {
                                        gc.downloadfile(sewer, orderNumber, Account_Number, "TaxYear_Statement" + bill, "OR", "Deschutes");
                                        filename = "TaxYear_Statement" + bill;
                                        FilePath1 = gc.filePath(orderNumber, Account_Number) + filename + ".pdf";
                                        PdfReader reader1;
                                        reader1 = new PdfReader(FilePath1);
                                        String textFromPage1 = PdfTextExtractor.GetTextFromPage(reader1, 1);
                                        System.Diagnostics.Debug.WriteLine("" + textFromPage1);

                                        pdftext1 = textFromPage1;

                                        tableassess2 = gc.Between(pdftext1, "PAYMENT OPTIONS", "TOTAL DUE (").Trim();
                                        string[] tableArray2 = tableassess2.Split('\n');
                                        List<string> rowarray4 = new List<string>();

                                        int n = 0, o = 0, p = 0, q = 0;
                                        int count3 = tableArray2.Length;

                                        for (n = 1; n < count3; n++)
                                        {
                                            a3 = tableArray2[n].Replace(" ", "~");
                                            string[] rowarray5 = a3.Split('~');
                                            rowarray4.AddRange(rowarray5);

                                            if (rowarray4.Count == 4)
                                            {
                                                newrow4 = rowarray4[0] + "~" + rowarray4[1] + "~" + rowarray4[2] + "~" + rowarray4[3];
                                                gc.insert_date(orderNumber, Account_Number, 906, newrow4, 1, DateTime.Now);
                                            }
                                            if (rowarray4.Count == 3)
                                            {
                                                if (q == 2)
                                                {
                                                    newrow4 = rowarray4[0] + "~" + "" + "~" + rowarray4[1] + "~" + rowarray4[2];
                                                    gc.insert_date(orderNumber, Account_Number, 906, newrow4, 1, DateTime.Now);
                                                }
                                                if (q == 3)
                                                {
                                                    newrow4 = "Total" + "~" + rowarray4[0] + "~" + rowarray4[1] + "~" + rowarray4[2];
                                                    gc.insert_date(orderNumber, Account_Number, 906, newrow4, 1, DateTime.Now);
                                                    q++;
                                                }
                                            }
                                            if (rowarray4.Count == 2)
                                            {
                                                newrow4 = rowarray4[0] + "~" + "" + "~" + "" + "~" + rowarray4[1];
                                                gc.insert_date(orderNumber, Account_Number, 906, newrow4, 1, DateTime.Now);
                                            }
                                            q++;
                                            rowarray4.Clear();
                                        }

                                        year = gc.Between(pdftext1, "DESCHUTES COUNTY REAL", "PROPERTY TAXES ACCOUNT NO.").Trim();

                                        tableassess3 = gc.Between(pdftext1, "Date Due Amount", "Mailing address change on back").Trim();
                                        try
                                        {
                                            tableassess3 = GlobalClass.After(tableassess3, "Date Due Amount");
                                        }
                                        catch { }
                                        line1 = gc.Between(tableassess3, "Full Payment Enclosed", "or 2/3 Payment Enclosed").Trim();
                                        line2 = gc.Between(tableassess3, "or 2/3 Payment Enclosed", "or 1/3 Payment Enclosed").Trim();
                                        line3 = GlobalClass.After(tableassess3, "or 1/3 Payment Enclosed").Trim();

                                        tableArray2 = line1.Split('\n');
                                        line1 = tableArray2[0];
                                        line2 = line2 + " " + tableArray2[1] + " " + tableArray2[2];
                                        line3 = line3.Replace("\n", " ");

                                        string[] tableline1 = line1.Split(' ');
                                        string[] tableline2 = line2.Split(' ');
                                        string[] tableline3 = line3.Split(' ');

                                        newrow3 = year + "~" + "Full Payment Enclosed" + "~" + tableline1[0] + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + tableline1[1] + "~" + tableline1[2];
                                        gc.insert_date(orderNumber, Account_Number, 899, newrow3, 1, DateTime.Now);
                                        newrow3 = year + "~" + "or 2/3 Payment Enclosed" + "~" + tableline2[0] + "~" + tableline2[1] + "~" + tableline2[3] + "~" + "" + "~" + "" + "~" + tableline2[4] + "~" + tableline2[2];
                                        gc.insert_date(orderNumber, Account_Number, 899, newrow3, 1, DateTime.Now);
                                        newrow3 = year + "~" + "or 1/3 Payment Enclosed" + "~" + tableline3[0] + "~" + tableline3[1] + "~" + tableline3[2] + "~" + tableline3[3] + "~" + tableline3[6] + "~" + tableline3[4] + "~" + tableline3[5];
                                        gc.insert_date(orderNumber, Account_Number, 899, newrow3, 1, DateTime.Now);
                                        bill++;
                                    }
                                }
                            }
                            catch
                            { }
                        }
                        catch
                        { }
                    }
                    catch
                    { }

                    //Sales_Information
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div[2]/ul/li[2]/ul/li[4]/a")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, Account_Number, "Sales Details", driver, "OR", "Deschutes");
                    }
                    catch
                    { }

                    //Land_Structure
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='navigation']/ul/li[2]/ul/li[5]/a")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, Account_Number, "Land Structure Details", driver, "OR", "Deschutes");
                    }
                    catch
                    { }

                    //SpecialAssements_Details
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div[2]/ul/li[2]/ul/li[6]/a")).Click();
                        Thread.Sleep(2000);

                        IWebElement SpecialTable = driver.FindElement(By.XPath("/html/body/div[1]/div[3]/div[2]/div[2]/table/tbody"));
                        Thread.Sleep(2000);
                        IList<IWebElement> SpecialTR = SpecialTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> SpecialTD;
                        gc.CreatePdf(orderNumber, Account_Number, "Special Details", driver, "OR", "Deschutes");
                        foreach (IWebElement Special in SpecialTR)
                        {
                            SpecialTD = Special.FindElements(By.TagName("td"));
                            if (SpecialTD.Count != 0)
                            {
                                Special_Description = SpecialTD[0].Text;
                                Special_Amount = SpecialTD[1].Text;
                                Special_Year = SpecialTD[2].Text;
                                Special_details = Special_Description + "~" + Special_Amount + "~" + Special_Year;
                                gc.insert_date(orderNumber, Account_Number, 290, Special_details, 1, DateTime.Now);
                            }
                        }
                    }
                    catch
                    { }

                    try
                    {
                        Tax_Authority = "Deschutes Services Building,1300 NW Wall Street, 2nd Floor Bend, OR 97701" + " " + "(541) 388 - 6540 Phone" + " " + "(541) 385 - 3248 Fax";
                    }
                    catch
                    { }
                    Tax_details = Tax_Code + "~" + TaxYear_2014 + "~" + TaxYear_2015 + "~" + TaxYear_2016 + "~" + TaxYear_2017 + "~" + TaxYear_2018 + "~" + Tax_Authority;
                    gc.insert_date(orderNumber, Account_Number, 291, Tax_details, 1, DateTime.Now);

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "OR", "Deschutes", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    //megrge pdf files
                    gc.mergpdf(orderNumber, "OR", "Deschutes");
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
