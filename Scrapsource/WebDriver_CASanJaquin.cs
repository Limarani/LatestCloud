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
    public class WebDriver_CASanJaquin
    {
        string Outparcelno = "", outputPath = "", strParcelNumber = "", strAddress = "", strTitleParcelNumber = "";
        string address1 = "", Assess = "", fee_parcel = "", Tra = "";
        string Assessment = "", Parcel_Number = "", Roll_Category = "", Address = "", Tax_year = "", First_Installment_Paid_Status = "", First_Installment_Paid_Date = "", First_Installment_Total_Due = "", First_Installment_Total_Paid = "", First_Installment_Balance = "", Second_Installment_Paid_Status = "", Second_Installment_Paid_Date = "", Second_Installment_Total_Due = "", Second_Installment_Total_Paid = "", Second_Installment_Balance = "", FirstandSecondInstallment_Total_Due = "", FirstandSecondInstallment_Total_Paid = "", FirstandSecondInstallment_Total_Balance = "";
        string roll_cat = "";
        string Assessor_ID_Number = "", Tax_Rate_Area = "", Property_type = "", Acres = "", Lot_Size = "", Asmt_Description = "", Year_Built = "";
        string ASMT = "", PARCEL = "", YEAR = "", Default_Number = "", Pay_Plan_in_Effect = "", Annual_Payment = "", Balance = "";
        string strPropertyAddress = "-", strPYearfirst = "", strPYearlast = "", strDetails = "", strDetails1 = "", strDetails2 = "", strDetails3 = "";
        int InstallCount = 0, YearCount, multicount;
        string strASMT = "-", strMultiAPN = "-", strMultiAddress = "-", strMulti = "-", strFeeParcelNo = "-", strYear = "-", strTaxYear = "-", strAssessYear = "-", strLand = "-", strStructure = "-", strTreesVanies = "-", strFEquip = "-", strFEquipPenalty = "-", strPProperty = "-",
               strPPPenalty = "-", strTAValue = "-", strExemption = "-", strNet = "-", strInstall1 = "-", strPaidInstall1 = "-", strPaiddateInstall1 = "-",
               strTotalDueInstall1 = "-", strTotalPaidInstall1 = "-", strBalanceInstall1 = "-", strInstall2 = "-", strPaidInstall2 = "-", strPaiddateInstall2 = "-",
              strTotalDueInstall2 = "-", strTotalPaidInstall2 = "-", strBalanceInstall2 = "-", strTotalDue = "-", strTaxType = "-", strTotalPaid = "-", strTotalBalance = "-", strtaxTRA = "-", strTRA = "-";
        string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
        IWebDriver driver;
        int Tax;
        IWebElement IFirstYear, ISecondYear;
        List<string> sRyear = new List<string>();
        List<string> listurl = new List<string>();
        List<string> listurldistinct = new List<string>();
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();

        public string FTP_CASanJoaquin(string address, string unitno, string parcelNumber, string ownerName, string searchType, string orderNumber, string directParcel)
        {

            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            // driver = new PhantomJSDriver();
            // driver = new ChromeDriver();
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("https://www.sjgov.org/department/assr/roll");
                    Thread.Sleep(4000);
                    driver.FindElement(By.Id("ctl00_Content_3_disclaimerbttn")).Click();
                    Thread.Sleep(4000);

                    if (searchType == "titleflex")
                    {
                        string titleaddress = address;
                        string titleowner = ownerName;
                        gc.TitleFlexSearch(orderNumber, "", titleowner, titleaddress, "CA", "San Joaquin");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_CASanJauin"] = "Zero";
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
                        driver.FindElement(By.XPath("/html/body/form/center/table/tbody/tr[6]/td[3]/input")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "CA", "San Joaquin");
                        IWebElement Iaddress = driver.FindElement(By.XPath("/html/body/form/center/p/input[1]"));
                        IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                        js.ExecuteScript("arguments[0].click();", Iaddress);
                        gc.CreatePdf_WOP(orderNumber, "Address search result", driver, "CA", "San Joaquin");
                        Thread.Sleep(6000);

                        try
                        {
                            IWebElement tbmulti = driver.FindElement(By.XPath("/html/body/form/center/table/tbody"));
                            IList<IWebElement> TRmulti = tbmulti.FindElements(By.TagName("tr"));
                            if (TRmulti.Count > 6)
                            {
                                IList<IWebElement> TDmulti;
                                foreach (IWebElement row in TRmulti)
                                {
                                    if (!row.Text.Contains("Asmt"))
                                    {
                                        if (row.Text.Contains("Assessor Inquiry: Please enter ONLY ONE search criteria"))
                                        {
                                            IWebElement runButton1 = driver.FindElement(By.XPath("/html/body/form/center/table/tbody/tr[2]/td[1]/a"));
                                            runButton1.Click();

                                        }
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
                                            gc.insert_date(orderNumber, fee_parcel, 218, multi1, 1, DateTime.Now);
                                        }
                                    }
                                }
                                HttpContext.Current.Session["multiparcel_CASanJoaquin"] = "Yes";
                                driver.Quit();
                                gc.mergpdf(orderNumber, "CA", "San Joaquin");
                                return "MultiParcel";
                            }
                        }
                        catch { }

                    }
                    if (searchType == "parcel")
                    {
                        var Select1 = driver.FindElement(By.XPath("//*[@id='idfeeparcel']"));
                        var selectElement1 = new SelectElement(Select1);
                        selectElement1.SelectByText("Begins with");
                        if (HttpContext.Current.Session["titleparcel"] != null)
                        {
                            strTitleParcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                            parcelNumber = strTitleParcelNumber;
                        }
                        if (strTitleParcelNumber.Contains(".") || strTitleParcelNumber.Contains("-"))
                        {
                            parcelNumber = strTitleParcelNumber.Replace(".", "").Replace("-", "");
                        }
                        if (parcelNumber.Contains("-"))
                        {
                            parcelNumber = parcelNumber.Replace("-", "");
                        }
                        string a = parcelNumber.Substring(0, 3);
                        string b = parcelNumber.Substring(3, 3);
                        string c = parcelNumber.Substring(6, 3);
                        string d = parcelNumber.Substring(9, 3);
                        parcelNumber = a + "-" + b + "-" + c + "-" + d;
                        driver.FindElement(By.XPath("/html/body/form/center/table/tbody/tr[3]/td[3]/input")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search", driver, "CA", "San Joaquin");
                        IWebElement IParcel = driver.FindElement(By.XPath("/html/body/form/center/p/input[1]"));
                        IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                        js.ExecuteScript("arguments[0].click();", IParcel);

                    }

                    try
                    {
                        IWebElement Inodata = driver.FindElement(By.XPath("/html/body/form/center/table"));
                        if (Inodata.Text.Contains("Field") && Inodata.Text.Contains("Search Type") && Inodata.Text.Contains("Parcel or Tail"))
                        {
                            HttpContext.Current.Session["Nodata_CASanJauin"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    Thread.Sleep(6000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Parcel search11", driver, "CA", "San Joaquin");
                    IWebElement runButton = driver.FindElement(By.XPath("/html/body/form/center/table/tbody/tr[2]/td[1]/a"));
                    runButton.Click();
                    Thread.Sleep(4000);


                    string date = DateTime.Today.ToString("dd-MM-yyyy");
                    //property details
                    string proptext = driver.FindElement(By.XPath("/html/body/form/center/table/tbody")).Text;
                    Assessor_ID_Number = gc.Between(proptext, "Assessor ID Number", "Tax Rate Area (TRA)").Trim();
                    parcelNumber = Assessor_ID_Number;
                    gc.CreatePdf(orderNumber, Assessor_ID_Number, "Assessment search result", driver, "CA", "San Joaquin");
                    Tax_Rate_Area = gc.Between(proptext, "Tax Rate Area (TRA)", "Last Recording Date").Trim();
                    Property_type = gc.Between(proptext, "Property Type", "Acres").Trim();
                    Acres = gc.Between(proptext, "Acres", "Lot Size(SqFt)").Trim();
                    Lot_Size = gc.Between(proptext, "Lot Size(SqFt)", "Asmt Description").Trim();
                    Asmt_Description = gc.Between(proptext, "Asmt Description", "Asmt Status").Trim();
                    try
                    { Year_Built = gc.Between(proptext, "Year Built", "Bedrooms").Trim(); }

                    catch
                    { }
                    string prop = Tax_Rate_Area + "~" + Property_type + "~" + Acres + "~" + Lot_Size + "~" + Asmt_Description + "~" + Year_Built;
                    gc.insert_date(orderNumber, Assessor_ID_Number, 83, prop, 1, DateTime.Now);

                    //Assessment details
                    string Land = "", Structure = "", Fixtures = "", Growing = "", TotalLand_and_Improvements = "", Manufactured_Home = "", Personal_Property = "", Homeowners_Exemption = "", Other_Exemption = "", Net_Assessment = "";
                    Land = gc.Between(proptext, "Land", "Structural Imprv").Trim();
                    Structure = gc.Between(proptext, "Structural Imprv", "Fixtures Real Property").Trim();
                    Fixtures = gc.Between(proptext, "Fixtures Real Property", "Growing Imprv").Trim();
                    Growing = gc.Between(proptext, "Growing Imprv", "Total Land and Improvements").Trim();
                    TotalLand_and_Improvements = gc.Between(proptext, "Total Land and Improvements", "Fixtures Personal Property").Trim();
                    Manufactured_Home = gc.Between(proptext, "Manufactured Homes", "Homeowners Exemption").Trim();
                    Personal_Property = gc.Between(proptext, "Personal Property", "Manufactured Homes").Trim();
                    if (Personal_Property.Contains("Personal Property"))
                    {

                        Personal_Property = GlobalClass.After(Personal_Property, "Personal Property");
                    }
                    Homeowners_Exemption = gc.Between(proptext, "Homeowners Exemption", "Other Exemption").Trim();
                    Other_Exemption = gc.Between(proptext, "Other Exemption", "Net Assessed Value").Trim();
                    Net_Assessment = gc.Between(proptext, "Net Assessed Value", "Building Description(s)").Trim();

                    string assess = Land + "~" + Structure + "~" + Fixtures + "~" + Growing + "~" + TotalLand_and_Improvements + "~" + Manufactured_Home + "~" + Personal_Property + "~" + Homeowners_Exemption + "~" + Other_Exemption + "~" + Net_Assessment;
                    gc.insert_date(orderNumber, Assessor_ID_Number, 84, assess, 1, DateTime.Now);

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    //tax details
                    driver.Navigate().GoToUrl("https://common3.mptsweb.com/MBC/sanjoaquin/tax/search");
                    //IWebElement ISearch = driver.FindElement(By.XPath("//*[@id='main']/section/div[2]/div/div[2]/a"));
                    //string strSearch = ISearch.GetAttribute("href");
                    //driver.Navigate().GoToUrl(strSearch);
                    taxclick(orderNumber, parcelNumber);

                    if (Tax > 0)
                    {
                        return "No Data Found";
                    }
                    //taxclick(orderNumber, parcelNumber, "2016 - PRIOR");


                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "CA", "San Joaquin", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);



                    driver.Quit();
                    HttpContext.Current.Session["titleparcel"] = "";
                    gc.mergpdf(orderNumber, "CA", "San Joaquin");
                    return "Data Inserted Successfully";
                }
                catch (Exception ex)
                {
                    gc.CreatePdf(orderNumber, parcelNumber, "CASanjaquinTaxSearch1233", driver, "CA", "San Joaquin");
                    driver.Quit();
                    GlobalClass.LogError(ex, orderNumber);
                    throw ex;
                }
            }
        }
        public void taxTRA()
        {
            //try
            //{
            //    string Nodata = driver.FindElement(By.XPath("//*[@id='ResultDiv']/div")).Text;
            //    if (Nodata.Contains("Sorry,")&& listurldistinct.Count==0)
            //    {
            //        HttpContext.Current.Session["CASanJoaquin_Zero"] = "Zero";
            //        driver.Quit();
            //        Tax++;

            //    }
            //}
            //catch { }
            try
            {
                strtaxTRA = driver.FindElement(By.XPath("//*[@id='ResultDiv']/div[1]/div/div/p")).Text;
                if (strtaxTRA.Contains("TRA : "))
                {
                    string strtaxdetails = driver.FindElement(By.XPath("//*[@id='ResultDiv']/div[1]/div/div/p")).Text;
                    strTRA = WebDriverTest.Between(strtaxdetails, "TRA : ", "\r\nRoll Cat.");
                }
            }
            catch { }
        }
        public void taxclick(string orderNumber, string parcelNumber)
        {
            IWebElement IRollyear = driver.FindElement(By.Id("SelTaxYear"));
            SelectElement sRollyear = new SelectElement(IRollyear);
            IList<IWebElement> strTaxRollYear = sRollyear.Options;
            IWebElement IFeeParcel = driver.FindElement(By.Id("SearchVal"));
            SelectElement sFeeParcel = new SelectElement(IFeeParcel);
            sFeeParcel.SelectByValue("feeparcel");
            driver.FindElement(By.Id("SearchValue")).SendKeys(parcelNumber);
            foreach (IWebElement roll in strTaxRollYear)
            {
                sRollyear.SelectByText(roll.Text);
                gc.CreatePdf(orderNumber, parcelNumber, "CASanjaquinTaxSearch" + roll.Text, driver, "CA", "San Joaquin");
                driver.FindElement(By.Id("SearchSubmit")).SendKeys(Keys.Enter);
                Thread.Sleep(2000);
                gc.CreatePdf(orderNumber, parcelNumber, "CASanjaquinTaxSearchResult" + roll.Text, driver, "CA", "San Joaquin");
                IWebElement Itax = driver.FindElement(By.Id("ResultDiv"));
                IList<IWebElement> ILink = driver.FindElements(By.TagName("a"));
                foreach (IWebElement link in ILink)
                {
                    if (link.Text.Contains("View Details"))
                    {
                        string linktext = link.GetAttribute("href");
                        string url = linktext;
                        listurl.Add(url);
                    }
                }
                listurldistinct = listurl.Distinct().ToList();
            }
            foreach (string URL in listurldistinct)
            {
                taxTRA();
                driver.Navigate().GoToUrl(URL);
                taxdetails(orderNumber, parcelNumber);
            }
        }

        public void taxdetails(string orderNumber, string parcelNumber)
        {
            for (int i = 1; i < 3; i++)
            {
                strASMT = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/div/div[1]/div[1]/div[2]")).Text;
                strFeeParcelNo = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/div/div[1]/div[2]/div[2]")).Text;
                strYear = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/div/div[1]/div[3]/div[2]")).Text;
                gc.CreatePdf(orderNumber, parcelNumber, "CASanjaquinTaxSearchResult" + strFeeParcelNo + " " + strYear + "", driver, "CA", "San Joaquin");
                strInstall1 = driver.FindElement(By.XPath("//*[@id='h2tab1']/div[1]/div[" + i + "]/h4")).Text;
                strPaidInstall1 = driver.FindElement(By.XPath("//*[@id='h2tab1']/div[1]/div[" + i + "]/dl/dd[1]")).Text;
                strPaiddateInstall1 = driver.FindElement(By.XPath("//*[@id='h2tab1']/div[1]/div[" + i + "]/dl/dd[2]")).Text;
                strTotalDueInstall1 = driver.FindElement(By.XPath("//*[@id='h2tab1']/div[1]/div[" + i + "]/dl/dd[3]")).Text;
                strTotalPaidInstall1 = driver.FindElement(By.XPath("//*[@id='h2tab1']/div[1]/div[" + i + "]/dl/dd[4]")).Text;
                strBalanceInstall1 = driver.FindElement(By.XPath("//*[@id='h2tab1']/div[1]/div[" + i + "]/dl/dd[5]")).Text;
                strTotalDue = driver.FindElement(By.XPath("//*[@id='h2tab1']/div[2]/dl/dd[1]")).Text;
                strTotalPaid = driver.FindElement(By.XPath("//*[@id='h2tab1']/div[2]/dl/dd[2]")).Text;
                strTotalBalance = driver.FindElement(By.XPath("//*[@id='h2tab1']/div[2]/dl/dd[3]")).Text;
                try
                {
                    IWebElement IAssessTax = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/ul/li[2]/a"));
                    IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                    js.ExecuteScript("arguments[0].click();", IAssessTax);
                    gc.CreatePdf(orderNumber, parcelNumber, "CASanjaquinTaxAssessmentResult" + strFeeParcelNo + " " + strYear + "", driver, "CA", "San Joaquin");
                    Thread.Sleep(4000);
                    strTaxType = driver.FindElement(By.XPath("//*[@id='h2tab2']/dl/dd[4]")).Text;
                    IWebElement ITax = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/ul/li[1]/a"));
                    js.ExecuteScript("arguments[0].click();", ITax);
                    Thread.Sleep(2000);
                }
                catch { }

                if (strTaxType == "")
                {
                    strTaxType = "-";
                }
                string strTaxDetails = strASMT + "~" + strFeeParcelNo + "~" + strTRA + "~" + strTaxType + "~" + strYear + "~" + strInstall1 + "~" + strPaidInstall1 + "~" + strPaiddateInstall1 + "~" + strTotalDueInstall1 + "~" + strTotalPaidInstall1 + "~" + strBalanceInstall1 + "~" +
                strTotalDue + "~" + strTotalPaid + "~" + strTotalBalance;
                gc.insert_date(orderNumber, parcelNumber, 85, strTaxDetails, 1, DateTime.Now);

                //download bill
                try
                {
                    //   IWebElement Itaxbill = driver.FindElement(By.XPath("/html/body/div[2]/section/div/div[1]/div/div[6]/div/div[1]/div[4]/div/a"));
                    IWebElement Itaxbill = driver.FindElement(By.LinkText("VIEW TAX BILL"));

                    string URL1 = Itaxbill.GetAttribute("href");
                    gc.downloadfile(URL1, orderNumber, Parcel_Number, strFeeParcelNo + strYear + "ViewTaxBill.pdf", "CA", "San Joaquin");
                }
                catch { }
            }
            driver.Navigate().Back();
        }


    }
}