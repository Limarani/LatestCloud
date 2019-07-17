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
namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_CAContraCosta
    {
        string outputPath = "", outparcelno = "";
        string Situs_Address = "-";
        string Assessment_Year = "-", Land = "-";
        string Bill_Type = "-", Bill_SaleId = "-", Install_No = "-", Date_Due = "-", Amount = "-", Pay = "-", Currenttax_Details = "-", TotalAmountSelected_ToPay = "-", Totaltax_Details = "-";
        string TaxBill_Type = "-", Tax_Year = "-", Instal_No = "-", AdValorem_tax = "-", Special_Assessments = "-", Delinquent_PenaltyCost = "-", TaxAmount = "-", Bill_Status, TaxBillAmount_Details = "-";
        string parcel = "-", parcel1 = "-", parcel2 = "-", parcel3 = "-", ParcelId = "-", strUrl = "-", Property_Deatail = "-", address = "-", Property_Address = "-", Multiparcel = "-";
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_CAContracosta(string houseno, string sname, string stype, string city, string parcelNumber, string searchType, string orderNumber, string ownername, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            // driver = new ChromeDriver();
            //driver = new PhantomJSDriver()
            using (driver = new PhantomJSDriver())
            {

                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    string taddress = "";
                    if (searchType == "titleflex")
                    {
                        if (directParcel != null)
                        {
                            taddress = houseno + " " + directParcel + " " + sname  ;
                        }
                        else
                        {
                            taddress = houseno + " " + directParcel + " " + sname ;
                        }
                        gc.TitleFlexSearch(orderNumber, parcelNumber, ownername, taddress, "CA", "Contra Costa");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_CAContraCosta"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        searchType = "parcel";
                        parcelNumber = GlobalClass.global_parcelNo.Replace("-", "");
                        parcelNumber = parcelNumber.Substring(0, 9);
                    }

                    if (searchType == "address")
                    {

                        driver.Navigate().GoToUrl("https://taxcolp.cccounty.us/taxpaymentrev3/lookup/");
                        Thread.Sleep(4000);

                        var SelectAddress = driver.FindElement(By.Id("searchtypeselect"));
                        Thread.Sleep(4000);
                        var SelectAddressTax = new SelectElement(SelectAddress);
                        SelectAddressTax.SelectByText("Property Address");
                        if (directParcel == null || directParcel == "")
                        {
                            address = houseno + " " + sname;
                        }

                        else
                        {
                            if (directParcel != null)
                            {
                                address = houseno + " " + directParcel + " " + sname;
                            }
                            else
                            {
                                address = houseno + " " + directParcel + " " + sname;
                            }
                        }


                        driver.FindElement(By.Id("searchfield")).SendKeys(address);

                        //var SelectAddressSuffix = driver.FindElement(By.Id("streetsuffixselect"));
                        //var SelectAddressSuffixTax = new SelectElement(SelectAddressSuffix);
                        //SelectAddressSuffixTax.SelectByText(stype);

                        var SelectAddressCity = driver.FindElement(By.Id("cityselect"));
                        var SelectAddressCityTax = new SelectElement(SelectAddressCity);
                        SelectAddressCityTax.SelectByText(city.ToUpper().ToString().Trim());

                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "CA", "Contra Costa");
                        driver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/form/div/div/div[6]/input")).SendKeys(Keys.Enter);
                        Thread.Sleep(8000);

                        //MultiParcel
                        try
                        {
                            IWebElement MultiTable = driver.FindElement(By.XPath("//*[@id='address-picker']/table/tbody"));
                            IList<IWebElement> MultiTR = MultiTable.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiTD;

                            int maxCheck = 0;
                            gc.CreatePdf_WOP(orderNumber, "MultiAddress", driver, "CA", "Contra Costa");
                            foreach (IWebElement Multi in MultiTR)
                            {
                                if (maxCheck <= 25)
                                {
                                    MultiTD = Multi.FindElements(By.TagName("td"));
                                    if (MultiTD.Count != 0)
                                    {
                                        Property_Address = MultiTD[0].Text;
                                        parcelNumber = MultiTD[1].Text;

                                        Multiparcel = Property_Address.Replace("\r\n", "");
                                        gc.insert_date(orderNumber, parcelNumber, 411, Multiparcel, 1, DateTime.Now);
                                    }
                                    maxCheck++;
                                }
                            }
                            HttpContext.Current.Session["multiParcel_CAContraCosta"] = "Yes";

                            if (MultiTR.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_CAContraCosta_Multicount"] = "Maximum";
                            }
                            driver.Quit();
                            return "MultiParcel";
                        }
                        catch { }
                    }

                    if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("https://taxcolp.cccounty.us/taxpaymentrev3/lookup/");
                        Thread.Sleep(4000);

                        var SelectParcel = driver.FindElement(By.Id("searchtypeselect"));
                        var SelectParcelTax = new SelectElement(SelectParcel);
                        SelectParcelTax.SelectByText("Parcel Number");

                        driver.FindElement(By.Id("searchfield")).Click();
                        parcel = parcelNumber.Replace("-", "");
                        parcel1 = parcel.Substring(0, 3);
                        parcel2 = parcel.Substring(3, 3);
                        parcel3 = parcel.Substring(6, 3);
                        ParcelId = parcel1 + parcel2 + parcel3;
                        Thread.Sleep(8000);
                        driver.FindElement(By.Id("searchfield")).Clear();
                        driver.FindElement(By.Id("searchfield")).Click();
                        driver.FindElement(By.Id("searchfield")).SendKeys(ParcelId);
                        gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearch", driver, "CA", "Contra Costa");
                        driver.FindElement(By.XPath("/html/body/div/div[3]/div[2]/form/div/div/div[6]/input")).SendKeys(Keys.Enter);
                        Thread.Sleep(8000);
                    }

                    try
                    {
                        IWebElement Inodata = driver.FindElement(By.Id("error"));
                        if(Inodata.Text.Contains("Address Not Found") || Inodata.Text.Contains("APN not found"))
                        {
                            HttpContext.Current.Session["Nodata_CAContraCosta"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    //Property Details
                    driver.FindElement(By.XPath("//*[@id='results']/div[1]/div[1]/a")).SendKeys(Keys.Enter);
                    Thread.Sleep(6000);

                    Situs_Address = driver.FindElement(By.XPath("//*[@id='results']/div[1]/h3")).Text;
                    Situs_Address = WebDriverTest.After(Situs_Address, "Address (Situs): ").Trim();

                    outparcelno = driver.FindElement(By.XPath("//*[@id='hide-details']/div[1]/h3")).Text;
                    outparcelno = GlobalClass.After(outparcelno, "Parcel Number (APN): ").Trim();
                    // outparcelno = gc.Between(outparcelno, "Parcel Number (APN): ", "SIGN UP HERE FOR SECURED").Trim();

                    gc.CreatePdf(orderNumber, outparcelno, "Property Details", driver, "CA", "Contra Costa");
                    Property_Deatail = Situs_Address;
                    gc.insert_date(orderNumber, outparcelno, 320, Property_Deatail, 1, DateTime.Now);

                    //Assement details
                    driver.FindElement(By.LinkText("ASSESSMENT INFORMATION")).Click();
                    Thread.Sleep(4000);
                    gc.CreatePdf(orderNumber, outparcelno, "Assement Details", driver, "CA", "Contra Costa");
                    IWebElement AssementTB = driver.FindElement(By.XPath("//*[@id='current-assessment-information']/table/tbody"));
                    IList<IWebElement> AssementTR = AssementTB.FindElements(By.TagName("tr"));
                    IList<IWebElement> AssementTD;
                    foreach (IWebElement Assement in AssementTR)
                    {
                        AssementTD = Assement.FindElements(By.TagName("td"));
                        if (AssementTD.Count != 0)
                        {
                            if (Assement.Text.Contains("Assessment Year"))
                            {
                                Assessment_Year = AssementTD[1].Text;
                            }
                            else
                            {
                                Land = AssementTD[2].Text + "~" + Land;
                            }
                        }
                    }
                    Land = Assessment_Year + "~" + Land.Replace("~-", "");
                    gc.insert_date(orderNumber, outparcelno, 365, Land, 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    //CurrentTax details

                    driver.FindElement(By.LinkText("CURRENT TAXES")).Click();
                    Thread.Sleep(4000);
                    gc.CreatePdf(orderNumber, outparcelno, "CurretntTax Details", driver, "CA", "Contra Costa");

                    List<string> urlListSecured = new List<string>();
                    List<string> urlListSupplemental = new List<string>();

                    IWebElement CurrentTaxTB = driver.FindElement(By.XPath("//*[@id='results']/div[2]/table/tbody"));
                    IList<IWebElement> CurrentTaxTR = CurrentTaxTB.FindElements(By.TagName("tr"));
                    IList<IWebElement> CurrentTaxTD;
                    IList<IWebElement> CurrentTaxA;
                    foreach (IWebElement CurrentTax in CurrentTaxTR)
                    {
                        CurrentTaxTD = CurrentTax.FindElements(By.TagName("td"));
                        CurrentTaxA = CurrentTax.FindElements(By.TagName("a"));
                        if (CurrentTaxTD.Count != 0)
                        {
                            Bill_Type = CurrentTaxTD[0].Text;
                            Bill_SaleId = CurrentTaxTD[1].Text.Replace(" - View Bill", "").Trim();
                            Install_No = CurrentTaxTD[2].Text;
                            Date_Due = CurrentTaxTD[3].Text;
                            Amount = CurrentTaxTD[4].Text;
                            try
                            {
                                Pay = CurrentTaxTD[5].Text;

                            }
                            catch { Pay = "PAY"; }

                            Currenttax_Details = Bill_Type + "~" + Bill_SaleId + "~" + Install_No + "~" + Date_Due + "~" + Amount + "~" + Pay + "~" + "-";
                            gc.insert_date(orderNumber, outparcelno, 366, Currenttax_Details, 1, DateTime.Now);
                        }
                        if (CurrentTaxA.Count != 0)
                        {
                            if (Bill_Type == "SECURED")
                            {
                                urlListSecured.Add(CurrentTaxA[0].GetAttribute("href"));
                            }
                            else if (Bill_Type == "SUPPLEMENTAL")
                            {
                                urlListSupplemental.Add(CurrentTaxA[0].GetAttribute("href"));

                            }
                        }
                    }
                    try
                    {
                        TotalAmountSelected_ToPay = driver.FindElement(By.XPath("//*[@id='summary']/div[2]/div[3]/h3")).Text;

                        Totaltax_Details = "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + TotalAmountSelected_ToPay;
                        gc.insert_date(orderNumber, outparcelno, 366, Totaltax_Details, 1, DateTime.Now);
                    }
                    catch
                    { }

                    //Download Tax Bills
                    try
                    {
                        int i = 0;
                        String Parent_Window1 = driver.CurrentWindowHandle;
                        foreach (string item in urlListSecured)
                        {
                            gc.downloadfileHeader(item, orderNumber, outparcelno, "Secured_Bill" + i, "CA", "Contra Costa", driver);
                            i++;
                        }

                        int k = 0;
                        foreach (string item in urlListSupplemental)
                        {
                            gc.downloadfileHeader(item, orderNumber, outparcelno, "Supplemental_Bill" + k, "CA", "Contra Costa", driver);
                            k++;
                        }
                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        driver.SwitchTo().Window(driver.WindowHandles.Last());
                        Thread.Sleep(4000);

                        driver.SwitchTo().Window(Parent_Window1);
                        Thread.Sleep(2000);
                    }
                    catch
                    { }
                    //TaxHistory Details
                    driver.FindElement(By.LinkText("TAX HISTORY")).Click();
                    Thread.Sleep(4000);
                    gc.CreatePdf(orderNumber, outparcelno, "TaxHistory Details", driver, "CA", "Contra Costa");

                    IWebElement TaxHistoryTB = driver.FindElement(By.XPath("//*[@id='prioryear-results']/div[2]/table/tbody"));
                    IList<IWebElement> TaxHistoryTR = TaxHistoryTB.FindElements(By.TagName("tr"));
                    IList<IWebElement> TaxHistoryTD;

                    foreach (IWebElement TaxHistory in TaxHistoryTR)
                    {
                        TaxHistoryTD = TaxHistory.FindElements(By.TagName("td"));
                        if (TaxHistoryTD.Count != 0)
                        {
                            TaxBill_Type = TaxHistoryTD[0].Text;
                            Tax_Year = TaxHistoryTD[1].Text.Replace("View Bill", "\r\n").Trim();
                            Instal_No = TaxHistoryTD[2].Text;
                            AdValorem_tax = TaxHistoryTD[3].Text;
                            Special_Assessments = TaxHistoryTD[4].Text;
                            Delinquent_PenaltyCost = TaxHistoryTD[5].Text;
                            TaxAmount = TaxHistoryTD[6].Text;
                            Bill_Status = TaxHistoryTD[7].Text;

                            TaxBillAmount_Details = TaxBill_Type + "~" + Tax_Year + "~" + Instal_No + "~" + AdValorem_tax + "~" + Special_Assessments + "~" + Delinquent_PenaltyCost + "~" + TaxAmount + "~" + Bill_Status;
                            gc.insert_date(orderNumber, outparcelno, 367, TaxBillAmount_Details, 1, DateTime.Now);
                        }
                    }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "CA", "Contra Costa", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    //megrge pdf files
                    gc.mergpdf(orderNumber, "CA", "Contra Costa");
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
