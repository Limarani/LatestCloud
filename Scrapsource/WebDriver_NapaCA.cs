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

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_NapaCA
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        Placer pltitle = new Placer();
        public string FTP_NapaCA(string address, string assessment_id, string parcelNumber, string searchType, string orderNumber, string directParcel, string ownername)
        {
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "", AssessTakenTime = "", TaxTakentime = "", CityTaxtakentime = "";
            string TotaltakenTime = "";

            List<string> strTaxRealestate1 = new List<string>();
            string address1 = "", Assess = "", fee_parcel = "", Tra = "";
            string Roll_Category = "", Address = "", Tax_year = "", First_Installment_Paid_Status = "", First_Installment_Paid_Date = "", First_Installment_Total_Due = "", First_Installment_Total_Paid = "", First_Installment_Balance = "", Second_Installment_Paid_Status = "", Second_Installment_Paid_Date = "", Second_Installment_Total_Due = "", Second_Installment_Total_Paid = "", Second_Installment_Balance = "", FirstandSecondInstallment_Total_Due = "", FirstandSecondInstallment_Total_Paid = "", FirstandSecondInstallment_Total_Balance = "";
            string Assessor_ID_Number = "", Tax_Rate_Area = "";
            string Default_Number = "", Pay_Plan_in_Effect = "", Annual_Payment = "", Balance = "";
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            // new PhantomJSDriver()
            using (driver = new PhantomJSDriver())
            {
                //  driver = new ChromeDriver();

                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("https://www.countyofnapa.org/150/Assessor-Parcel-Data");
                    //Thread.Sleep(4000);
                    IWebElement iframeElement = driver.FindElement(By.XPath("//*[@id='ctl00_MasterContentPlaceHolder_ContentiFrame']"));
                    driver.SwitchTo().Frame(iframeElement);
                    Thread.Sleep(2000);
                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, "", ownername, address, "CA", "Napa");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        //var Select = driver.FindElement(By.Id("idSitus"));
                        //var selectElement1 = new SelectElement(Select);
                        //selectElement1.SelectByText("Begins with");
                        //IWebElement text = driver.FindElement(By.Name("idSitus"));                    
                        //IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                        //js.ExecuteScript("document.getElementById('idSitus').value='" + address + "'");
                        driver.FindElement(By.Name("idSitus")).SendKeys(address);
                        // driver.FindElement(By.XPath("/html/body/form/table/tbody/tr[5]/td[3]/input")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "CA", "Napa");
                        driver.FindElement(By.Name("Submit")).SendKeys(Keys.Enter);
                        gc.CreatePdf_WOP(orderNumber, "Address search result", driver, "CA", "Napa");
                        Thread.Sleep(6000);
                        try
                        {
                            IWebElement tbmulti = driver.FindElement(By.XPath("/html/body/form/table/tbody/tr[2]/td[1]/a"));
                            IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));
                            int maxCheck = 0;
                            if (TRmulti.Count > 6)
                            {
                                IList<IWebElement> TDmulti;
                                foreach (IWebElement row in TRmulti)
                                {
                                    if (maxCheck <= 25)
                                    {
                                        if (!row.Text.Contains("Assessment No."))
                                        {
                                            TDmulti = row.FindElements(By.TagName("td"));
                                            if (TDmulti.Count == 3 && TDmulti[0].Text.Trim() != "")
                                            {
                                                Assess = TDmulti[0].Text;
                                                fee_parcel = TDmulti[1].Text;
                                                Tra = TDmulti[2].Text;
                                            }
                                            if (TDmulti.Count == 1 && TDmulti[0].Text.Trim() != "")
                                            {
                                                address1 = TDmulti[0].Text;
                                                string multi1 = Assess + "~" + Tra + "~" + address1;
                                                gc.insert_date(orderNumber, fee_parcel, 1238, multi1, 1, DateTime.Now);
                                                //Assessment Id~Tra~Address
                                            }
                                        }
                                        maxCheck++;
                                    }
                                }

                                if (TRmulti.Count > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_CANapa_Multicount"] = "Maximum";
                                }
                                else
                                {
                                    HttpContext.Current.Session["multiparcel_CANapa"] = "Yes";
                                }
                                driver.Quit();
                                gc.mergpdf(orderNumber, "CA", "Napa");
                                return "MultiParcel";
                            }
                        }
                        catch { }
                    }
                    else if (searchType == "parcel")
                    {
                        var Select = driver.FindElement(By.Id("idfeeparcel"));
                        var selectElement1 = new SelectElement(Select);
                        selectElement1.SelectByText("Begins with");
                        if (parcelNumber.Contains("-"))
                        {
                            parcelNumber = parcelNumber.Replace("-", "");
                        }
                        string a = parcelNumber.Substring(0, 3);
                        string b = parcelNumber.Substring(3, 3);
                        string c = parcelNumber.Substring(6, 3);
                        string d = "";
                        if (parcelNumber.Length == 12)
                        {
                            d = parcelNumber.Substring(9, 3);
                        }
                        parcelNumber = a + "-" + b + "-" + c + "-" + d;
                        driver.FindElement(By.XPath("/html/body/form/table/tbody/tr[3]/td[3]/input")).SendKeys(parcelNumber.Trim());
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search", driver, "CA", "Napa");
                        driver.FindElement(By.XPath("/html/body/form/p/input[1]")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                    }
                    // /html/body/form/table/tbody/tr[2]/td[1]/a
                    Thread.Sleep(2000);
                    try
                    {
                        IWebElement runButton = driver.FindElement(By.XPath("/html/body/form/table/tbody/tr[2]/td[1]/a"));
                        runButton.Click();
                        Thread.Sleep(4000);
                    }
                    catch { }
                    //property details
                    string Parcel_id = "", owner_name = "", ImprovementValue = "", totalValue = "", Bussiness_property = "";
                    string fulltabletext = driver.FindElement(By.XPath("/html/body/form/table/tbody")).Text.Trim();
                    // Parcel_id = gc.Between(fulltabletext, "Assessor Parcel Number (APN)", "Assessment Number").Trim();

                    Assessor_ID_Number = gc.Between(fulltabletext, "Assessor ID Number", "Tax Rate Area (TRA)").Trim();
                    Tax_Rate_Area = gc.Between(fulltabletext, "Tax Rate Area (TRA)", "Last Recording Date").Trim();
                    pltitle.TaxIDNumberFurtherDescribed = Tax_Rate_Area;
                    //owner_name = gc.Between(fulltabletext, "Owner", "Lot Size").Trim();
                    gc.CreatePdf(orderNumber, Assessor_ID_Number, "Property details", driver, "CA", "Napa");
                    string prop = Assessor_ID_Number + "~" + Tax_Rate_Area + "~" + owner_name;
                    //  Assessment Number~Tax Rate Area~Owner Name
                    gc.insert_date(orderNumber, Assessor_ID_Number, 1233, prop, 1, DateTime.Now);

                    //Assessment details
                    string Land = "", Structural_Imprv = "", Fixtures_Real_Property = "", Growing_Imprv = "", TotalLandandImprovements = "", FixturesPersonalProperty = "", Personal_Property = "", ManufacturedHomes = "", Homeowners_Exemption = "", Other_Exemption = "", Acres = "", NetAssessedValue = "";
                    Land = gc.Between(fulltabletext, "Land", "Structural Imprv").Trim();
                    Structural_Imprv = gc.Between(fulltabletext, "Structural Imprv", "Fixtures Real Property").Trim();
                    Fixtures_Real_Property = gc.Between(fulltabletext, "Fixtures Real Property", "Growing Imprv").Trim();
                    Growing_Imprv = gc.Between(fulltabletext, "Growing Imprv", "Total Land and Improvements").Trim();
                    TotalLandandImprovements = gc.Between(fulltabletext, "Total Land and Improvements", "Fixtures Personal Property").Trim();
                    FixturesPersonalProperty = gc.Between(fulltabletext, "Fixtures Personal Property", "\r\nPersonal Property").Trim();
                    Personal_Property = gc.Between(fulltabletext, "\r\nPersonal Property", "Manufactured Homes").Trim();
                    ManufacturedHomes = gc.Between(fulltabletext, "Manufactured Homes", "Homeowners Exemption").Trim();

                    Homeowners_Exemption = gc.Between(fulltabletext, "Homeowners Exemption", "Other Exemption").Trim();
                    string he = Homeowners_Exemption.Replace(",", "").Replace("$", "").Trim();
                    if (he == "")
                    {
                        pltitle.ExemptionHomeowners = null;
                    }
                    else
                    {
                        pltitle.ExemptionHomeowners = he;
                    }

                    Other_Exemption = gc.Between(fulltabletext, "Other Exemption", "Net Assessed Value").Trim();
                    NetAssessedValue = gc.Between(fulltabletext, "Net Assessed Value", "Building Description(s)").Trim();
                    Acres = gc.Between(fulltabletext, "Acres", "Lot Size(SqFt)");
                    string assess = Land + "~" + Structural_Imprv + "~" + Fixtures_Real_Property + "~" + Growing_Imprv + "~" + TotalLandandImprovements + "~" + FixturesPersonalProperty + "~" + Personal_Property + "~" + ManufacturedHomes + "~" + Homeowners_Exemption + "~" + Other_Exemption + "~" + NetAssessedValue + "~" + Acres;
                    gc.insert_date(orderNumber, Assessor_ID_Number, 1234, assess, 1, DateTime.Now);

                    // pltitle.TaxRateArea = Tax_Rate_Area.Trim();
                    pltitle.Land = Land.Replace(",", "").Replace("$", "").Trim();
                    if (pltitle.Land =="")
                    {
                        pltitle.Land = null;
                    }
                    pltitle.Improvements = Structural_Imprv.Replace(",", "").Replace("$", "").Trim();
                    if (pltitle.Improvements == "")
                    {
                        pltitle.Improvements = null;
                    }

                    // pltitle.TotalValue = TotalLandandImprovements.Replace(",", "").Replace("$", "").Trim();

                    //Land~Structural Imprv~Fixtures Real Property~Growing Imprv~Total Land and Improvements~Fixtures Personal Property~Personal Property~Manufactured Homes~Homeowners Exemption~Other Exemption~Net Assessed Value
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    //Tax details
                    int lat = 0;
                    driver.Navigate().GoToUrl("https://common2.mptsweb.com/mbc/napa/tax/search/");
                    string Tax_Authority1 = driver.FindElement(By.XPath("//*[@id='footer']/div[1]/div/div[1]/div[2]/div/ul[1]")).Text;
                    string Tax_Authority = GlobalClass.After(Tax_Authority1, "Treasurer - Tax Collector");
                    for (int k = 1; k < 4; k++)
                    {
                        if (k == 1)
                        {
                            //select year
                            var year = driver.FindElement(By.Id("SelTaxYear"));
                            var selectElement1 = new SelectElement(year);
                            selectElement1.SelectByIndex(0);
                            //searchBy
                            var searchby = driver.FindElement(By.Id("SearchVal"));
                            var selectElement2 = new SelectElement(searchby);
                            selectElement2.SelectByText("FEE PARCEL");
                            driver.FindElement(By.XPath("//*[@id='SearchValue']")).SendKeys(Assessor_ID_Number);
                            driver.FindElement(By.XPath("//*[@id='SearchSubmit']")).SendKeys(Keys.Enter);
                            Thread.Sleep(3000);
                            gc.CreatePdf(orderNumber, Assessor_ID_Number, "tax1", driver, "CA", "Napa");
                        }

                        else if (k == 2)
                        {
                            //select year
                            var year = driver.FindElement(By.Id("SelTaxYear"));
                            var selectElement1 = new SelectElement(year);
                            selectElement1.SelectByIndex(1);
                            //searchBy
                            var searchby = driver.FindElement(By.Id("SearchVal"));
                            var selectElement2 = new SelectElement(searchby);
                            selectElement2.SelectByText("FEE PARCEL");
                            driver.FindElement(By.XPath("//*[@id='SearchSubmit']")).SendKeys(Keys.Enter);
                            Thread.Sleep(3000);
                            gc.CreatePdf(orderNumber, Assessor_ID_Number, "tax2", driver, "CA", "Napa");
                        }

                        else if (k == 3)
                        {
                            //select year
                            var year = driver.FindElement(By.Id("SelTaxYear"));
                            var selectElement1 = new SelectElement(year);
                            selectElement1.SelectByIndex(2);
                            //searchBy
                            var searchby = driver.FindElement(By.Id("SearchVal"));
                            var selectElement2 = new SelectElement(searchby);
                            selectElement2.SelectByText("FEE PARCEL");
                            driver.FindElement(By.XPath("//*[@id='SearchSubmit']")).SendKeys(Keys.Enter);
                            Thread.Sleep(3000);
                            gc.CreatePdf(orderNumber, Assessor_ID_Number, "tax3", driver, "CA", "Napa");
                        }

                        int j;
                        //Current Tax Details Table 
                        try
                        {
                            int divCount = driver.FindElements(By.XPath("//*[@id='ResultDiv']/div")).Count;
                            for (j = 1; j <= divCount; j++)
                            {
                                if (k != 1)
                                {
                                    IWebElement Itaxstmt = driver.FindElement(By.XPath("//*[@id='ResultDiv']/div[" + j + "]/div/div/div/a"));
                                    string stmt1 = Itaxstmt.GetAttribute("href");
                                    strTaxRealestate1.Add(stmt1);
                                }
                                if (k == 1)
                                {
                                    IWebElement Itaxstmt = driver.FindElement(By.XPath("//*[@id='ResultDiv']/div[" + j + "]/div/div/div/a"));
                                    string stmt1 = Itaxstmt.GetAttribute("href");
                                    strTaxRealestate1.Add(stmt1);
                                    lat++;
                                }

                            }
                        }
                        catch { }
                    }
                    int q = 0;
                    string pyear = "";
                    foreach (string real in strTaxRealestate1)
                    {
                        driver.Navigate().GoToUrl(real);
                        Thread.Sleep(4000);
                        string pdfassess = driver.FindElement(By.XPath(" /html/body/div[2]/section/div/div/div/div[6]/div/div[1]/div[1]/div[2]")).Text;
                        string pdfyear = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div/div/div[6]/div/div[1]/div[3]/div[2]")).Text;
                        string Installmenttype1 = driver.FindElement(By.XPath("//*[@id='h2tab1']/div[1]/div[1]/h4")).Text;
                        string pdf = pdfassess + " " + pdfyear;
                        string fulltabletextTax1 = driver.FindElement(By.XPath("//*[@id='h2tab1']/div[1]/div[1]/dl")).Text.Trim().Replace("\r\n", "");
                        if(fulltabletextTax1.Contains("Delinq. Date"))
                        {
                            First_Installment_Paid_Status = gc.Between(fulltabletextTax1, "Paid Status", "Delinq. Date").Trim();
                            First_Installment_Paid_Date = gc.Between(fulltabletextTax1, "Delinq. Date", "Total Due").Trim();
                        }
                        else
                        {
                            First_Installment_Paid_Status = gc.Between(fulltabletextTax1, "Paid Status", "Paid Date").Trim();
                            First_Installment_Paid_Date = gc.Between(fulltabletextTax1, "Paid Date", "Total Due").Trim();
                        }
                        First_Installment_Total_Due = gc.Between(fulltabletextTax1, "Total Due", "Total Paid").Trim();
                        First_Installment_Total_Paid = gc.Between(fulltabletextTax1, "Total Paid", "Balance").Trim();
                        First_Installment_Balance = WebDriverTest.After(fulltabletextTax1, "Balance");
                        string installmenttype2 = driver.FindElement(By.XPath("//*[@id='h2tab1']/div[1]/div[2]/h4")).Text;
                        string fulltabletextTax2 = driver.FindElement(By.XPath("//*[@id='h2tab1']/div[1]/div[2]/dl")).Text.Trim().Replace("\r\n", "");
                        if (fulltabletextTax2.Contains("Delinq. Date"))
                        {
                            Second_Installment_Paid_Status = gc.Between(fulltabletextTax2, "Paid Status", "Delinq. Date").Trim();
                            Second_Installment_Paid_Date = gc.Between(fulltabletextTax2, "Delinq. Date", "Total Due").Trim();
                        }
                        else
                        {
                            Second_Installment_Paid_Status = gc.Between(fulltabletextTax2, "Paid Status", "Paid Date").Trim();
                            Second_Installment_Paid_Date = gc.Between(fulltabletextTax2, "Paid Date", "Total Due").Trim();
                        }                        
                        Second_Installment_Total_Due = gc.Between(fulltabletextTax2, "Total Due", "Total Paid").Trim();
                        Second_Installment_Total_Paid = gc.Between(fulltabletextTax2, "Total Paid", "Balance").Trim();
                        Second_Installment_Balance = WebDriverTest.After(fulltabletextTax2, "Balance");

                        string fulltabletextTax3 = driver.FindElement(By.XPath("//*[@id='h2tab1']/div[2]/dl")).Text.Trim().Replace("\r\n", "");
                        FirstandSecondInstallment_Total_Due = gc.Between(fulltabletextTax3, "Total Due", "Total Paid").Trim();
                        FirstandSecondInstallment_Total_Paid = gc.Between(fulltabletextTax3, "Total Paid", "Total Balance").Trim();
                        FirstandSecondInstallment_Total_Balance = WebDriverTest.After(fulltabletextTax3, "Total Balance");

                        gc.CreatePdf(orderNumber, Assessor_ID_Number, "Taxes" + q, driver, "CA", "Napa");
                        driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/ul/li[2]/a")).Click();
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, Assessor_ID_Number, "Assess info" + q, driver, "CA", "Napa");
                        string fulltabletextTax4 = driver.FindElement(By.XPath("//*[@id='h2tab2']")).Text.Trim();
                        string assessment = gc.Between(fulltabletextTax4, "Assessment", "Taxyear").Trim().Replace("\r\n", " ");
                        Tax_year = gc.Between(fulltabletextTax4, "Taxyear", "Parcel Number").Trim().Replace("\r\n", " ");
                        Roll_Category = gc.Between(fulltabletextTax4, "Roll Category", "Doc Num").Trim().Replace("\r\n", " ");
                        string rollcasttype = GlobalClass.Before(Roll_Category, "-");
                        Address = WebDriverTest.After(fulltabletextTax4, "Address").Replace("\r\n", " ").Trim();
                        string tax = assessment + "~" + Roll_Category + "~" + Address + "~" + Tax_year + "~" + Installmenttype1 + "~" + First_Installment_Paid_Status + "~" + First_Installment_Paid_Date + "~" + First_Installment_Total_Due + "~" + First_Installment_Total_Paid + "~" + First_Installment_Balance + "~" + FirstandSecondInstallment_Total_Due + "~" + FirstandSecondInstallment_Total_Paid + "~" + FirstandSecondInstallment_Total_Balance + "~" + Tax_Authority;
                        gc.insert_date(orderNumber, Assessor_ID_Number, 1235, tax, 1, DateTime.Now);
                        string tax1 = assessment + "~" + Roll_Category + "~" + Address + "~" + Tax_year + "~" + installmenttype2 + "~" + Second_Installment_Paid_Status + "~" + Second_Installment_Paid_Date + "~" + Second_Installment_Total_Due + "~" + Second_Installment_Total_Paid + "~" + Second_Installment_Balance + "~" + FirstandSecondInstallment_Total_Due + "~" + FirstandSecondInstallment_Total_Paid + "~" + FirstandSecondInstallment_Total_Balance + "~" + Tax_Authority;
                        gc.insert_date(orderNumber, Assessor_ID_Number, 1235, tax1, 1, DateTime.Now);
                        if (Roll_Category.Contains("CS"))
                        {
                            pyear = Tax_year;
                        }

                        if (q < lat)
                        {
                            pltitle.TaxTypeID = 1;
                            pltitle.TaxingEntity = "TAX-IN32";
                            pltitle.TaxIDNumber = Assessor_ID_Number;
                            pltitle.Year = Convert.ToInt16(Tax_year);
                            pltitle.assyear = Tax_year;
                            pltitle.FirstTaxesOutDate = null;
                            pltitle.FirstDueDate = null;
                            pltitle.FirstInstallment = First_Installment_Total_Due.Replace(",", "").Replace("$", "").Trim();
                            pltitle.FirstPaid = First_Installment_Total_Paid.Replace(",", "").Replace("$", "").Trim();
                            pltitle.FirstDue = First_Installment_Balance.Replace(",", "").Replace("$", "").Trim();
                            pltitle.SecondTaxesOutDate = null;
                            pltitle.SecondDueDate = null;
                            pltitle.SecondInstallment = Second_Installment_Total_Due.Replace(",", "").Replace("$", "").Trim();
                            pltitle.SecondPaid = Second_Installment_Total_Paid.Replace(",", "").Replace("$", "").Trim();
                            pltitle.SecondDue = Second_Installment_Balance.Replace(",", "").Replace("$", "").Trim();

                            if (First_Installment_Total_Paid == "$0.00")
                            {
                                pltitle.FirstPaid = "False";
                            }
                            else
                            {
                                pltitle.FirstPaid = "True";
                            }

                            if (Second_Installment_Total_Paid == "$0.00")
                            {
                                pltitle.SecondPaid = "False";
                            }
                            else
                            {
                                pltitle.SecondPaid = "True";
                            }

                            if (First_Installment_Balance == "$0.00")
                            {
                                pltitle.FirstDue = "False";
                            }
                            else
                            {
                                pltitle.FirstDue = "True";
                            }

                            if (Second_Installment_Balance == "$0.00")
                            {
                                pltitle.SecondDue = "False";
                            }
                            else
                            {
                                pltitle.SecondDue = "True";
                            }


                            if (First_Installment_Paid_Status == "PAID")
                            {
                                if (First_Installment_Paid_Date == "")
                                {
                                    pltitle.FirstTaxesOutDate = null;
                                }
                                else
                                {
                                    pltitle.FirstTaxesOutDate = First_Installment_Paid_Date;
                                    pltitle.FirstDueDate = null;
                                }
                            }
                            else if ((First_Installment_Paid_Status == "LATE") || (First_Installment_Paid_Status == "DUE"))
                            {
                                if (First_Installment_Paid_Date == "")
                                {
                                    pltitle.FirstDueDate = null;
                                }
                                else
                                {
                                    pltitle.FirstDueDate = First_Installment_Paid_Date;
                                    pltitle.FirstTaxesOutDate = null;
                                }

                            }

                            if (Second_Installment_Paid_Status == "PAID")
                            {
                                if (Second_Installment_Paid_Date == "")
                                {
                                    pltitle.SecondTaxesOutDate = null;
                                }
                                else
                                {
                                    pltitle.SecondTaxesOutDate = Second_Installment_Paid_Date;
                                    pltitle.SecondDueDate = null;
                                }

                            }
                            else if (Second_Installment_Paid_Status == "LATE" || Second_Installment_Paid_Status == "DUE")
                            {
                                if (Second_Installment_Paid_Date == "")
                                {
                                    pltitle.SecondDueDate = null;
                                }
                                else
                                {
                                    pltitle.SecondDueDate = Second_Installment_Paid_Date;
                                    pltitle.SecondTaxesOutDate = null;
                                }

                            }
                        }

                        //Assessment~Roll Category~Address~Tax year~First Installment Paid Status~First Installment Paid Date~First Installment Total Due~First Installment Total Paid~First Installment Balance~Second Installment Paid Status~Second Installment Paid Date~Second Installment Total Due~Second Installment Total Paid~Second Installment Balance~First and Second Installment Total Due~First and Second Installment Total Paid~First and Second Installment Total Balance

                        //download taxbill
                        try
                        {
                            IWebElement Itaxbill = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/div/div[1]/div[4]/div/a"));
                            string URL1 = Itaxbill.GetAttribute("href");
                            gc.downloadfile(URL1, orderNumber, Assessor_ID_Number, "TaxBill" + q, "CA", "Napa");

                        }
                        catch { }
                        //Taxcode Info

                        driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/ul/li[3]/a")).Click();
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, Assessor_ID_Number, "Tax code" + q, driver, "CA", "Napa");

                        int count = driver.FindElements(By.XPath("//*[@id='h2tab3']/div")).Count;
                        int divCount2 = count + 1;
                        string[] TaxCode = new string[divCount2];
                        string[] description = new string[divCount2];
                        string[] rate = new string[divCount2];
                        string[] Istinstall = new string[divCount2];
                        string[] IIndinstall = new string[divCount2];
                        string[] total = new string[divCount2];
                        string[] phone = new string[divCount2];
                        for (int i = 1; i <= count; i++)
                        {
                            string Description1 = driver.FindElement(By.XPath(" //*[@id='h2tab3']/div[" + i + "]/div/div/dl")).Text.Trim().Replace("\r\n", "");
                            TaxCode[i] = gc.Between(Description1, "Tax Code", "Description").Trim();
                            description[i] = gc.Between(Description1, "Description", "Rate").Trim();
                            rate[i] = gc.Between(Description1, "Rate", "1st Installment").Trim();
                            Istinstall[i] = gc.Between(Description1, "1st Installment", "2nd Installment").Trim();
                            IIndinstall[i] = gc.Between(Description1, "2nd Installment", "Total").Trim();
                            total[i] = gc.Between(Description1, "Total", "Phone").Trim();
                            phone[i] = WebDriverTest.After(Description1, "Phone");
                            string taxcode = Tax_year + "~" + TaxCode[i] + "~" + description[i] + "~" + rate[i] + "~" + Istinstall[i] + "~" + IIndinstall[i] + "~" + total[i] + "~" + phone[i];
                            gc.insert_date(orderNumber, Assessor_ID_Number, 1236, taxcode, 1, DateTime.Now);
                        }
                        //Tax Year~Tax Code~Description~Rate~1st Installment~2nd Installment~Total~Phone

                        //Default Tax
                        try
                        {
                            driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/ul/li[4]/a")).Click();
                            Thread.Sleep(3000);
                            gc.CreatePdf(orderNumber, Assessor_ID_Number, "Default Tax" + q, driver, "CA", "Napa");

                            Default_Number = driver.FindElement(By.XPath("//*[@id='h2tab4']/div[2]/div[1]/div[2]")).Text;
                            Pay_Plan_in_Effect = driver.FindElement(By.XPath("//*[@id='h2tab4']/div[2]/div[2]/div[2]")).Text;
                            Annual_Payment = driver.FindElement(By.XPath("//*[@id='h2tab4']/div[2]/div[3]/div[2]")).Text;
                            Balance = driver.FindElement(By.XPath("//*[@id='h2tab4']/div[2]/div[4]/div[2]")).Text;

                            string default_tax = Tax_year + "~" + Default_Number + "~" + Pay_Plan_in_Effect + "~" + Annual_Payment + "~" + Balance;
                            gc.insert_date(orderNumber, Assessor_ID_Number, 1237, default_tax, 1, DateTime.Now);
                        }
                        //Tax Year~Default Number~Pay Plan in Effect~Annual Payment~Balance
                        catch { }

                        if (q < lat)
                        {
                            //  pltitle.TaxIDNumber = parcelNumber;
                            if (rollcasttype.Contains("CS"))
                            {
                                gc.InsertSearchTax(orderNumber, pltitle.Land, pltitle.Improvements, pltitle.ExemptionHomeowners, pltitle.ExemptionAdditional, pltitle.FirstInstallment, pltitle.FirstDueDate, pltitle.FirstTaxesOutDate, pltitle.FirstPaid, pltitle.FirstDue, pltitle.SecondInstallment, pltitle.SecondDueDate, pltitle.SecondTaxesOutDate, pltitle.SecondPaid, pltitle.SecondDue, pltitle.assyear, 1, pltitle.Year, "Napa County Treasurer-Tax Collector", pltitle.TaxIDNumber, "County", pltitle.TaxIDNumberFurtherDescribed);
                            }
                            if (rollcasttype.Contains("SS"))
                            {
                                if (pyear == Tax_year)
                                {
                                    gc.InsertSearchTax(orderNumber, pltitle.Land, pltitle.Improvements, pltitle.ExemptionHomeowners, pltitle.ExemptionAdditional, pltitle.FirstInstallment, pltitle.FirstDueDate, pltitle.FirstTaxesOutDate, pltitle.FirstPaid, pltitle.FirstDue, pltitle.SecondInstallment, pltitle.SecondDueDate, pltitle.SecondTaxesOutDate, pltitle.SecondPaid, pltitle.SecondDue, pltitle.assyear, 100, pltitle.Year, "Napa County Treasurer-Tax Collector", pltitle.TaxIDNumber, "Supplemental", pltitle.TaxIDNumberFurtherDescribed);
                                }
                            }
                            q++;
                        }

                    }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "CA", "Napa", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                    driver.Quit();
                    gc.mergpdf(orderNumber, "CA", "Napa");
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