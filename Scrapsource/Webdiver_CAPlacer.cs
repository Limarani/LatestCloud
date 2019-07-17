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
    public class Webdiver_CAPlacer
    {
        string outputPath = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();

        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        private object driverIE;
        private int multicount;
        private string strMultiAddress;
        List<string> strTaxRealestate1 = new List<string>();
        public string FTP_placer(string houseno, string sname, string assessment_id, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            string address1 = "", Assess = "", fee_parcel = "", Tra = "";
            string Assessment = "", Parcel_Number = "", Roll_Category = "", Address = "", Tax_year = "", First_Installment_Paid_Status = "", First_Installment_Paid_Date = "", First_Installment_Total_Due = "", First_Installment_Total_Paid = "", First_Installment_Balance = "", Second_Installment_Paid_Status = "", Second_Installment_Paid_Date = "", Second_Installment_Total_Due = "", Second_Installment_Total_Paid = "", Second_Installment_Balance = "", FirstandSecondInstallment_Total_Due = "", FirstandSecondInstallment_Total_Paid = "", FirstandSecondInstallment_Total_Balance = "";
            string roll_cat = "";
            string Assessor_ID_Number = "", Tax_Rate_Area = "", Situs_Address = "", Acres = "", Lot_Size = "", Asmt_Description = "", Year_Built = "";
            string ASMT = "", PARCEL = "", YEAR = "", Default_Number = "", Pay_Plan_in_Effect = "", Annual_Payment = "", Balance = "";
            string tax_add1, tax_add2, tax_add3, tax_add;
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string address = houseno + " " + sname;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new PhantomJSDriver();
            //driver = new ChromeDriver();
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("https://www.placer.ca.gov/departments/assessor/assessment-inquiry");
                    Thread.Sleep(4000);
                    driver.FindElement(By.Id("agreeyes")).Click();
                    Thread.Sleep(4000);

                    IWebElement iframeElement = driver.FindElement(By.XPath("//*[@id='remoteform']/iframe"));
                    driver.SwitchTo().Frame(iframeElement);
                    Thread.Sleep(2000);


                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "CA", "Placer");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_CAPlacer"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }


                    if (searchType == "address")
                    {
                        var Select = driver.FindElement(By.Id("idSitus"));
                        var selectElement1 = new SelectElement(Select);
                        selectElement1.SelectByText("Begins with");
                        driver.FindElement(By.XPath("/html/body/form/table/tbody/tr[4]/td[3]/input")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "CA", "Placer");
                        driver.FindElement(By.XPath("/html/body/form/p/input[1]")).SendKeys(Keys.Enter);
                        gc.CreatePdf_WOP(orderNumber, "Address search result", driver, "CA", "Placer");
                        Thread.Sleep(6000);

                        IWebElement tbmulti = driver.FindElement(By.XPath("/html/body/form/table/tbody"));
                        IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));
                        int l = 0;
                        if (TRmulti.Count > 6)
                        {

                            IList<IWebElement> TDmulti;
                            foreach (IWebElement row in TRmulti)
                            {
                                if (l <= 25)
                                {
                                    if (!row.Text.Contains("Asmt"))
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
                                            string multi1 = Assess + "~" + fee_parcel + "~" + Tra + "~" + address1;
                                            gc.insert_date(orderNumber, fee_parcel, 180, multi1, 1, DateTime.Now);

                                        }
                                    }
                                    l++;
                                }
                            }

                            if (TRmulti.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Placer_Multicount"] = "Maximum";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiParcel_Placer"] = "Yes";
                            }
                            driver.Quit();
                            gc.mergpdf(orderNumber, "CA", "Placer");
                            return "MultiParcel";
                        }
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
                        string d = parcelNumber.Substring(9, 3);
                        parcelNumber = a + "-" + b + "-" + c + "-" + d;
                        driver.FindElement(By.XPath("/html/body/form/table/tbody/tr[2]/td[3]/input")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search", driver, "CA", "Placer");
                        driver.FindElement(By.XPath("/html/body/form/p/input[1]")).SendKeys(Keys.Enter);
                    }

                    else if (searchType == "block")
                    {
                        var Select = driver.FindElement(By.Id("idasmt"));
                        var selectElement1 = new SelectElement(Select);
                        selectElement1.SelectByText("Begins with");


                        if (assessment_id.Contains("-"))
                        {
                            assessment_id = assessment_id.Replace("-", "");
                        }

                        string a = assessment_id.Substring(0, 3);
                        string b = assessment_id.Substring(3, 3);
                        string c = assessment_id.Substring(6, 3);
                        string d = assessment_id.Substring(9, 3);
                        assessment_id = a + "-" + b + "-" + c + "-" + d;
                        driver.FindElement(By.XPath("/html/body/form/table/tbody/tr[3]/td[3]/input")).SendKeys(assessment_id);
                        gc.CreatePdf_WOP(orderNumber, "Assessment search result", driver, "CA", "Placer");
                        driver.FindElement(By.XPath("/html/body/form/p/input[1]")).SendKeys(Keys.Enter);
                    }

                    Thread.Sleep(6000);


                    IWebElement runButton = driver.FindElement(By.XPath("/html/body/form/table/tbody/tr[2]/td[1]/a"));
                    runButton.Click();
                    Thread.Sleep(4000);

                    //property details

                    string fulltabletext = driver.FindElement(By.XPath(" /html/body/form/table/tbody")).Text.Trim().Replace("\r\n", " ");
                    string Land = "", Structure = "", Fixtures = "", Growing = "", TotalLand_and_Improvements = "", Manufactured_Home = "", Personal_Property = "", Homeowners_Exemption = "", Other_Exemption = "", Net_Assessment = "";

                    Assessor_ID_Number = gc.Between(fulltabletext, "Assessor ID Number", "Tax Rate Area").Trim();
                    gc.CreatePdf(orderNumber, Assessor_ID_Number, "Assessment search result", driver, "CA", "Placer");
                    Tax_Rate_Area = gc.Between(fulltabletext, "Tax Rate Area (TRA)", "Last Recording Date").Trim();
                    Situs_Address = gc.Between(fulltabletext, "Situs Address", "Acres").Trim();

                    Acres = gc.Between(fulltabletext, "Acres", "Lot Size").Trim();
                    Lot_Size = gc.Between(fulltabletext, "Lot Size(SqFt)", "Asmt Description").Trim();
                    Asmt_Description = gc.Between(fulltabletext, "Asmt Description", "Asmt Status").Trim();
                    Year_Built = gc.Between(fulltabletext, "Year Built", "Bedrooms").Trim();


                    string prop = Tax_Rate_Area + "~" + Situs_Address + "~" + Acres + "~" + Lot_Size + "~" + Asmt_Description + "~" + Year_Built;

                    gc.insert_date(orderNumber, Assessor_ID_Number, 155, prop, 1, DateTime.Now);

                    //Assessment details
                    Land = gc.Between(fulltabletext, "Land", "Structure").Trim();

                    Structure = gc.Between(fulltabletext, "Structure", "Fixtures").Trim();
                    Fixtures = gc.Between(fulltabletext, "Fixtures", "Growing").Trim();
                    Growing = gc.Between(fulltabletext, "Growing", "Total Land and Improvements").Trim();
                    TotalLand_and_Improvements = gc.Between(fulltabletext, "Total Land and Improvements", "Manufactured Home").Trim();
                    Manufactured_Home = gc.Between(fulltabletext, "Manufactured Home", "Personal Property").Trim();
                    Personal_Property = gc.Between(fulltabletext, "Personal Property", "Homeowners Exemption").Trim();
                    Homeowners_Exemption = gc.Between(fulltabletext, "Homeowners Exemption", "Other Exemption").Trim();
                    Other_Exemption = gc.Between(fulltabletext, "Other Exemption", "Net Assessment").Trim();
                    Net_Assessment = gc.Between(fulltabletext, "Net Assessment", "Building Description(s)").Trim();

                    string assess = Land + "~" + Structure + "~" + Fixtures + "~" + Growing + "~" + TotalLand_and_Improvements + "~" + Manufactured_Home + "~" + Personal_Property + "~" + Homeowners_Exemption + "~" + Other_Exemption + "~" + Net_Assessment;
                    gc.insert_date(orderNumber, Assessor_ID_Number, 157, assess, 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    //Tax details
                    driver.Navigate().GoToUrl("https://common3.mptsweb.com/mbc/placer/tax/search");

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
                            gc.CreatePdf(orderNumber, Assessor_ID_Number, "Tax search", driver, "CA", "Placer");
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
                            gc.CreatePdf(orderNumber, Assessor_ID_Number, "Tax search1", driver, "CA", "Placer");
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
                            gc.CreatePdf(orderNumber, Assessor_ID_Number, "Tax search2", driver, "CA", "Placer");
                        }

                        int j;
                        //Current Tax Details Table 
                        try
                        {
                            int divCount = driver.FindElements(By.XPath("//*[@id='ResultDiv']/div")).Count;
                            for (j = 1; j <= divCount; j++)
                            {
                                IWebElement Itaxstmt = driver.FindElement(By.XPath("//*[@id='ResultDiv']/div[" + j + "]/div/div/div/a"));
                                string stmt1 = Itaxstmt.GetAttribute("href");
                                strTaxRealestate1.Add(stmt1);

                            }
                        }
                        catch { }
                    }
                    foreach (string real in strTaxRealestate1)
                    {
                        driver.Navigate().GoToUrl(real);
                        Thread.Sleep(4000);
                        tax_add1 = driver.FindElement(By.XPath(" //*[@id='footer']/div[1]/div/div[1]/div[2]/div/ul/li[2]")).Text;
                        tax_add2 = driver.FindElement(By.XPath("//*[@id='footer']/div[1]/div/div[1]/div[2]/div/ul/li[3]")).Text;
                        tax_add3 = driver.FindElement(By.XPath("//*[@id='footer']/div[1]/div/div[1]/div[2]/div/ul/li[4]")).Text;
                        tax_add = tax_add1 + " " + tax_add2 + " " + tax_add3;

                        string pdfassess = driver.FindElement(By.XPath(" /html/body/div[2]/section/div/div/div/div[6]/div/div[1]/div[1]/div[2]")).Text;
                        string pdfyear = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div/div/div[6]/div/div[1]/div[3]/div[2]")).Text;
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

                        gc.CreatePdf(orderNumber, Assessor_ID_Number, "Taxes" + pdf, driver, "CA", "Placer");
                        driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/ul/li[2]/a")).Click();
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, Assessor_ID_Number, "Assess info" + pdf, driver, "CA", "Placer");
                        string fulltabletextTax4 = driver.FindElement(By.XPath("//*[@id='h2tab2']/dl")).Text.Trim().Replace("\r\n", "");
                        string assessment = gc.Between(fulltabletextTax4, "Assessment", "Taxyear").Trim();
                        Tax_year = gc.Between(fulltabletextTax4, "Taxyear", "Parcel Number").Trim();
                        Roll_Category = gc.Between(fulltabletextTax4, "Roll Category", "Doc Num").Trim();
                        Address = WebDriverTest.After(fulltabletextTax4, "Address");

                        string tax = assessment + "~" + Roll_Category + "~" + Address + "~" + Tax_year + "~" + First_Installment_Paid_Status + "~" + First_Installment_Paid_Date + "~" + First_Installment_Total_Due + "~" + First_Installment_Total_Paid + "~" + First_Installment_Balance + "~" + Second_Installment_Paid_Status + "~" + Second_Installment_Paid_Date + "~" + Second_Installment_Total_Due + "~" + Second_Installment_Total_Paid + "~" + Second_Installment_Balance + "~" + FirstandSecondInstallment_Total_Due + "~" + FirstandSecondInstallment_Total_Paid + "~" + FirstandSecondInstallment_Total_Balance + "~" + tax_add;
                        gc.insert_date(orderNumber, Assessor_ID_Number, 165, tax, 1, DateTime.Now);

                        //      Assessment~Roll Category~Address~Tax year~First Installment Paid Status~First Installment Paid Date~First Installment Total Due~First Installment Total Paid~First Installment Balance~Second Installment Paid Status~Second Installment Paid Date~Second Installment Total Due~Second Installment Total Paid~Second Installment Balance~First and Second Installment Total Due~First and Second Installment Total Paid~First and Second Installment Total Balance

                        //download taxbill

                        IWebElement Itaxbill = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/div/div[1]/div[4]/div/a"));
                        string URL1 = Itaxbill.GetAttribute("href");
                        gc.downloadfile(URL1, orderNumber, Assessor_ID_Number, "TaxBill" + pdf, "CA", "Placer");


                        //Tax Year~Tax Code~Description~Rate~1st Installment~2nd Installment~Total~Phone

                        //Default Tax
                        try
                        {
                            driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/ul/li[4]/a")).Click();
                            Thread.Sleep(3000);
                            gc.CreatePdf(orderNumber, Assessor_ID_Number, "Default Tax" + pdf, driver, "CA", "Placer");

                            Default_Number = driver.FindElement(By.XPath("//*[@id='h2tab4']/div[2]/div[1]/div[2]")).Text;
                            Pay_Plan_in_Effect = driver.FindElement(By.XPath("//*[@id='h2tab4']/div[2]/div[2]/div[2]")).Text;
                            Annual_Payment = driver.FindElement(By.XPath("//*[@id='h2tab4']/div[2]/div[3]/div[2]")).Text;
                            Balance = driver.FindElement(By.XPath("//*[@id='h2tab4']/div[2]/div[4]/div[2]")).Text;
                            string default_tax = assessment + "~" + Tax_year + "~" + Default_Number + "~" + Pay_Plan_in_Effect + "~" + Annual_Payment + "~" + Balance;
                            //  string default_tax = Tax_year + "~" + Default_Number + "~" + Pay_Plan_in_Effect + "~" + Annual_Payment + "~" + Balance;
                            gc.insert_date(orderNumber, Assessor_ID_Number, 166, default_tax, 1, DateTime.Now);



                        }
                        //Tax Year~Default Number~Pay Plan in Effect~Annual Payment~Balance
                        catch { }

                    }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "CA", "Placer", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    driver.Dispose();
                    gc.mergpdf(orderNumber, "CA", "Placer");
                    return "Data Inserted Successfully";
                }

                catch (Exception ex)
                {
                    driver.Quit();
                    driver.Dispose();
                    throw ex;
                }
            }
        }
    }
}