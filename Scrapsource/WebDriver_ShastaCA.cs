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
    public class WebDriver_ShastaCA
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_CAShasta(string houseno, string sname, string direction, string type, string unitno, string parcelNumber, string searchType, string orderNumber, string directParcel, string ownername)
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
            // driver = new PhantomJSDriver();
            //  driver = new ChromeDriver();
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("https://www.co.shasta.ca.us/index/assessor_index/assessment_inquiry.aspx");
                    Thread.Sleep(4000);
                    if (searchType == "titleflex")
                    {
                        string address = "";
                        address = houseno + " " + sname + " " + direction + " " + type + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, parcelNumber.Trim(), ownername.Trim(), address.Trim(), "CA", "Shasta");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_ShataCA"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";


                    }
                    if (searchType == "block")
                    {
                        IWebElement iframeElement = driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[3]/div/div[2]/div/div[2]/div/div[2]/div/div[2]/iframe"));
                        driver.SwitchTo().Frame(iframeElement);
                        var Select = driver.FindElement(By.Id("idAsmt"));
                        var selectElement1 = new SelectElement(Select);
                        selectElement1.SelectByText("Begins with");
                        driver.FindElement(By.XPath("/html/body/form/table/tbody/tr[3]/td[3]/input")).SendKeys(unitno);
                        gc.CreatePdf(orderNumber, parcelNumber, "Assessment search", driver, "CA", "Shasta");
                        driver.FindElement(By.XPath("/html/body/form/p/input[1]")).SendKeys(Keys.Enter);
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Assessment search result", driver, "CA", "Shasta");

                    }
                    else if (searchType == "parcel")
                    {
                        IWebElement iframeElement = driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[3]/div/div[2]/div/div[2]/div/div[2]/div/div[2]/iframe"));
                        driver.SwitchTo().Frame(iframeElement);

                        var Select = driver.FindElement(By.Id("idfeeparcel"));
                        var selectElement1 = new SelectElement(Select);
                        selectElement1.SelectByText("Begins with");

                        driver.FindElement(By.XPath("/html/body/form/table/tbody/tr[4]/td[3]/input")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search", driver, "CA", "Shasta");
                        driver.FindElement(By.XPath("/html/body/form/p/input[1]")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search result", driver, "CA", "Shasta");



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

                                            string multi1 = fee_parcel + "~" + Tra;
                                            gc.insert_date(orderNumber, Assess, 663, multi1, 1, DateTime.Now);

                                        }
                                    }
                                    l++;
                                }
                            }

                            if (TRmulti.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_CAShasta_Multicount"] = "Maximum";
                                driver.Quit();
                                gc.mergpdf(orderNumber, "CA", "Shasta");
                                return "Maximum";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiparcel_CAShasta"] = "Yes";
                                driver.Quit();
                                gc.mergpdf(orderNumber, "CA", "Shasta");
                                return "MultiParcel";
                            }
                        }


                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.XPath("/html/body/form/table"));
                        if (INodata.Text.Contains("Assessor Inquiry: Please enter search criteria"))
                        {
                            HttpContext.Current.Session["Nodata_ShataCA"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    string parcel_id1 = "";
                    parcel_id1 = driver.FindElement(By.XPath("/html/body/form/table/tbody/tr[2]/td[2]")).Text.Trim();

                    IWebElement runButton = driver.FindElement(By.XPath("/html/body/form/table/tbody/tr[2]/td[1]/a"));
                    runButton.Click();
                    Thread.Sleep(4000);

                    //property details
                    string Parcel_id = "", owner_name = "", ImprovementValue = "", totalValue = "", Bussiness_property = "", PropertyType = "", Acres = "", AsmtDescription = "", AsmtStatus = "", YearBuilt = "";
                    string fulltabletext = driver.FindElement(By.XPath("/html/body/form/table/tbody")).Text.Trim().Replace("\r\n", "");
                    Parcel_id = gc.Between(fulltabletext, "Assessor ID Number", "Tax Rate Area (TRA)").Trim();
                    gc.CreatePdf(orderNumber, parcel_id1, "Property details", driver, "CA", "Shasta");

                    Tax_Rate_Area = gc.Between(fulltabletext, "Tax Rate Area (TRA)", "Last Recording Date").Trim();
                    PropertyType = gc.Between(fulltabletext, "Property Type", "Acres").Trim();
                    Acres = gc.Between(fulltabletext, "Acres", "Asmt Description").Trim();
                    AsmtDescription = gc.Between(fulltabletext, "Asmt Description", "Asmt Status").Trim();
                    AsmtStatus = gc.Between(fulltabletext, "Asmt Status", "Roll Values").Trim();
                    YearBuilt = gc.Between(fulltabletext, "Year Built", "Bedrooms").Trim();
                    string prop = Parcel_id + "~" + Tax_Rate_Area + "~" + PropertyType + "~" + Acres + "~" + AsmtDescription + "~" + AsmtStatus + "~" + YearBuilt;
                    //  Assessment Number~Tax Rate Area~Property Type~Acres~Asmt Description~Asmt Status~Year Built

                    gc.insert_date(orderNumber, parcel_id1, 658, prop, 1, DateTime.Now);

                    //Assessment details
                    string Land = "", Fixtures = "", Growing = "", Personal_Property = "", Homeowners_Exemption = "", Other_Exemption = "", Net_Assessment = "";
                    Land = gc.Between(fulltabletext, "Land", "Structure").Trim();
                    ImprovementValue = gc.Between(fulltabletext, "Structure", "Fixtures").Trim();
                    Fixtures = gc.Between(fulltabletext, "Fixtures", "Growing").Trim();
                    Growing = gc.Between(fulltabletext, "Growing", "Total Land and Improvements").Trim();
                    totalValue = gc.Between(fulltabletext, "Total Land and Improvements", "Personal Property").Trim();
                    Personal_Property = gc.Between(fulltabletext, "Personal Property", "Business Property").Trim();
                    Bussiness_property = gc.Between(fulltabletext, "Business Property", "Homeowners Exemption").Trim();
                    Homeowners_Exemption = gc.Between(fulltabletext, "Homeowners Exemption", "Other Exemption").Trim();
                    Other_Exemption = gc.Between(fulltabletext, "Other Exemption", "Net Assessment").Trim();
                    Net_Assessment = gc.Between(fulltabletext, "Net Assessment", "Ownership").Trim();
                    string assess = Land + "~" + ImprovementValue + "~" + Fixtures + "~" + Growing + "~" + totalValue + "~" + Personal_Property + "~" + Bussiness_property + "~" + Homeowners_Exemption + "~" + Other_Exemption + "~" + Net_Assessment;
                    gc.insert_date(orderNumber, parcel_id1, 659, assess, 1, DateTime.Now);
                    //Land Value~Improvement Value~Fixtures~Growing~Total Value~Personal Property~Business Property~Homeowners Exemption~Other Exemptions~Net Assessment
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    //Tax details
                    driver.Navigate().GoToUrl("https://common2.mptsweb.com/mbc/shasta/tax/search#");
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
                            driver.FindElement(By.XPath("//*[@id='SearchValue']")).SendKeys(parcel_id1);
                            driver.FindElement(By.XPath("//*[@id='SearchSubmit']")).SendKeys(Keys.Enter);
                            Thread.Sleep(3000);
                            gc.CreatePdf(orderNumber, parcel_id1, "tax1", driver, "CA", "Shasta");
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
                            gc.CreatePdf(orderNumber, parcel_id1, "tax2", driver, "CA", "Shasta");
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
                            gc.CreatePdf(orderNumber, parcel_id1, "tax3", driver, "CA", "Shasta");
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
                    int k1 = 1;
                    foreach (string real in strTaxRealestate1)
                    {
                        driver.Navigate().GoToUrl(real);
                        Thread.Sleep(4000);

                        string pdfassess = driver.FindElement(By.XPath(" /html/body/div[2]/section/div/div/div/div[6]/div/div[1]/div[1]/div[2]")).Text;
                        string pdfyear = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div/div/div[6]/div/div[1]/div[3]/div[2]")).Text;
                        int pdf = k1;
                        string fulltabletextTax1 = driver.FindElement(By.XPath("//*[@id='h2tab1']/div[1]/div[1]/dl")).Text.Trim().Replace("\r\n", "");
                        if (fulltabletextTax1.Contains("Delinq. Date"))
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

                        gc.CreatePdf(orderNumber, parcel_id1, "Taxes" + pdf, driver, "CA", "Shasta");
                        driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/ul/li[2]/a")).Click();
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcel_id1, "Assess info" + pdf, driver, "CA", "Shasta");
                        string fulltabletextTax4 = driver.FindElement(By.XPath("//*[@id='h2tab2']/dl")).Text.Trim().Replace("\r\n", "");
                        string assessment = gc.Between(fulltabletextTax4, "Assessment", "Taxyear").Trim();
                        Tax_year = gc.Between(fulltabletextTax4, "Taxyear", "Parcel Number").Trim();
                        Roll_Category = gc.Between(fulltabletextTax4, "Roll Category", "Doc Num").Trim();
                        string owner = WebDriverTest.After(fulltabletextTax4, "Owner");
                        string tax = assessment + "~" + Roll_Category + "~" + owner + "~" + Tax_year + "~" + First_Installment_Paid_Status + "~" + First_Installment_Paid_Date + "~" + First_Installment_Total_Due + "~" + First_Installment_Total_Paid + "~" + First_Installment_Balance + "~" + Second_Installment_Paid_Status + "~" + Second_Installment_Paid_Date + "~" + Second_Installment_Total_Due + "~" + Second_Installment_Total_Paid + "~" + Second_Installment_Balance + "~" + FirstandSecondInstallment_Total_Due + "~" + FirstandSecondInstallment_Total_Paid + "~" + FirstandSecondInstallment_Total_Balance;
                        gc.insert_date(orderNumber, parcel_id1, 660, tax, 1, DateTime.Now);

                        //      Assessment~Roll Category~Address~Tax year~First Installment Paid Status~First Installment Paid Date~First Installment Total Due~First Installment Total Paid~First Installment Balance~Second Installment Paid Status~Second Installment Paid Date~Second Installment Total Due~Second Installment Total Paid~Second Installment Balance~First and Second Installment Total Due~First and Second Installment Total Paid~First and Second Installment Total Balance

                        //download taxbill
                        try
                        {
                            IWebElement Itaxbill = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/div/div[1]/div[4]/div/a"));
                            string URL1 = Itaxbill.GetAttribute("href");
                            gc.downloadfile(URL1, orderNumber, parcel_id1, "TaxBill" + pdf, "CA", "Shasta");
                        }
                        catch { }

                        //Taxcode Info

                        driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/ul/li[3]/a")).Click();
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcel_id1, "Tax code" + pdf, driver, "CA", "Shasta");

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
                            gc.insert_date(orderNumber, parcel_id1, 661, taxcode, 1, DateTime.Now);
                        }
                        //Tax Year~Tax Code~Description~Rate~1st Installment~2nd Installment~Total~Phone

                        //Default Tax
                        try
                        {
                            driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/ul/li[4]/a")).Click();
                            Thread.Sleep(3000);
                            gc.CreatePdf(orderNumber, parcel_id1, "Default Tax" + pdf, driver, "CA", "Shasta");

                            Default_Number = driver.FindElement(By.XPath("//*[@id='h2tab4']/div[2]/div[1]/div[2]")).Text;
                            Pay_Plan_in_Effect = driver.FindElement(By.XPath("//*[@id='h2tab4']/div[2]/div[2]/div[2]")).Text;
                            Annual_Payment = driver.FindElement(By.XPath("//*[@id='h2tab4']/div[2]/div[3]/div[2]")).Text;
                            Balance = driver.FindElement(By.XPath("//*[@id='h2tab4']/div[2]/div[4]/div[2]")).Text;

                            string default_tax = Tax_year + "~" + Default_Number + "~" + Pay_Plan_in_Effect + "~" + Annual_Payment + "~" + Balance;
                            gc.insert_date(orderNumber, parcel_id1, 662, default_tax, 1, DateTime.Now);
                        }
                        //Tax Year~Default Number~Pay Plan in Effect~Annual Payment~Balance
                        catch { }
                        k1++;
                    }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "CA", "Shasta", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "CA", "Shasta");
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