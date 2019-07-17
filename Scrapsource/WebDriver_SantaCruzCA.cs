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
using System.Linq;

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_SantaCruzCA
    {

        List<string> AddressSearch = new List<string>();
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        IWebElement ITaxbillhistorySearch;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());


        public string FTP_SantaCruz(string Address, string unitno, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            List<string> multiparcel = new List<string>();
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                string Parcel_Number = "", taxauthority1 = "", taxauthority = ""; ;
                List<string> strUrl = new List<string>();
                StartTime = DateTime.Now.ToString("HH:mm:ss");
                try
                {
                    driver.Navigate().GoToUrl(" http://www.co.santa-cruz.ca.us/Departments/TaxCollector.aspx");
                    Thread.Sleep(2000);
                    gc.CreatePdf_WOP(orderNumber, "Tax Authority Pdf", driver, "CA", "Santa Cruz");
                    taxauthority1 = driver.FindElement(By.XPath("//*[@id='dnn_ctr7661_HtmlModule_lblContent']/div/div[2]")).Text.Trim();
                    taxauthority = GlobalClass.Before(taxauthority1, "FAX:").Trim();
                }
                catch { }


                driver.Navigate().GoToUrl("http://sccounty01.co.santa-cruz.ca.us/ASR/");
                Thread.Sleep(2000);

                try
                {
                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, "", ownername, Address, "CA", "Santa Cruz");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_SantaCruzCA"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {

                        driver.FindElement(By.Id("txtSitus")).SendKeys(Address);
                        gc.CreatePdf_WOP(orderNumber, "Enter The Address Before", driver, "CA", "Santa Cruz");
                        driver.FindElement(By.Id("butSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Enter The Address After", driver, "CA", "Santa Cruz");


                        //Multiparcel
                        try
                        {
                            int count = 0;
                            IWebElement Multiaddresstable1add = driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/table/tbody/tr[6]/td/div/div/div[3]"));
                            IList<IWebElement> multiaddressrows = Multiaddresstable1add.FindElements(By.TagName("div"));
                            IList<IWebElement> Multiaddressid;
                            foreach (IWebElement Multiaddress in multiaddressrows)
                            {
                                Multiaddressid = Multiaddress.FindElements(By.TagName("div"));
                                if (Multiaddressid.Count == 3 && !Multiaddress.Text.Contains("Parcel Info") && !Multiaddress.Text.Contains("APN"))
                                {
                                    string Multiparcelnumber = Multiaddressid[0].Text;
                                    string OWnerclass = Multiaddressid[2].Text;
                                    string Address1 = Multiaddressid[1].Text;
                                    string multiaddressresult = OWnerclass + "~" + Address1;
                                    gc.insert_date(orderNumber, Multiparcelnumber, 1088, multiaddressresult, 1, DateTime.Now);
                                    count++;
                                }


                            }
                            if (count > 1 && count < 26)
                            {
                                HttpContext.Current.Session["multiparcel_Santa Cruz"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (count > 25)
                            {
                                HttpContext.Current.Session["multiparcel_Santa Cruz_Maximum"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='pnlContainer']/form/table/tbody/tr[1]/td/span")).Text;
                            if (nodata.Contains("No parcels found for the entered address."))
                            {
                                HttpContext.Current.Session["Nodata_SantaCruzCA"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {
                        parcelNumber = parcelNumber.Replace("-", "").Replace(" ", "").Replace("000", "").Trim();
                        driver.FindElement(By.Id("txtAPNNO")).SendKeys(parcelNumber);
                        gc.CreatePdf_WOP(orderNumber, "Enter The ParcelNumber Before", driver, "CA", "Santa Cruz");
                        driver.FindElement(By.Id("butSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Enter The Address After", driver, "CA", "Santa Cruz");
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='pnlContainer']/form/table/tbody/tr[1]/td/span")).Text;
                            if (nodata.Contains("No parcels found for the entered APN."))
                            {
                                HttpContext.Current.Session["Nodata_SantaCruzCA"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }

                    }
                    //Property Details
                    IWebElement IAddressSearch = driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/table/tbody/tr[6]/td/div/div/div[3]/div/div[1]/a"));
                    IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                    js.ExecuteScript("arguments[0].click();", IAddressSearch);
                    Thread.Sleep(5000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Property Info", driver, "CA", "Santa Cruz");

                    string ParcelNumber = driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/table/tbody/tr[6]/td/div[1]/div/div[3]/div/div[1]")).Text.Trim();
                    string PropertyAddress = driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/table/tbody/tr[6]/td/div[1]/div/div[3]/div/div[2]")).Text.Trim();
                    string PropertyClass = driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/table/tbody/tr[6]/td/div[1]/div/div[3]/div/div[3]")).Text.Trim();

                    string propertydetails = PropertyAddress + "~" + PropertyClass;
                    gc.insert_date(orderNumber, ParcelNumber, 1042, propertydetails, 1, DateTime.Now);

                    //Assessment Details
                    string yearwise = "", Assessment = "", yearwise1 = "";
                    IWebElement Multiaddresstable = driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/table/tbody/tr[6]/td/div[2]/div/div[3]/div[1]"));
                    IList<IWebElement> multiaddressrow = Multiaddresstable.FindElements(By.TagName("div"));
                    foreach (IWebElement Multiassess in multiaddressrow)
                    {
                        string Yearassess = Multiassess.GetAttribute("title");
                        if (Yearassess != "")
                        {
                            Assessment += Multiassess.GetAttribute("title") + "~";
                            yearwise += Multiassess.Text + "~";
                        }

                    }
                    Assessment = Assessment.TrimEnd('~');
                    yearwise = yearwise.TrimEnd('~');
                    DBconnection dbconn = new DBconnection();
                    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + Assessment + "' where Id = '" + 1053 + "'");
                    gc.insert_date(orderNumber, ParcelNumber, 1053, yearwise, 1, DateTime.Now);
                    IWebElement Multiaddresstable1 = driver.FindElement(By.XPath("/html/body/center/table/tbody/tr/td/table/tbody/tr[6]/td/div[2]/div/div[3]/div[2]"));
                    IList<IWebElement> multiaddressrow1 = Multiaddresstable1.FindElements(By.TagName("div"));
                    foreach (IWebElement Multiaddress1 in multiaddressrow1)
                    {
                        string Yearassess = Multiaddress1.GetAttribute("title");
                        if (Yearassess != "")
                        {
                            yearwise1 += Multiaddress1.Text + "~";
                        }
                    }
                    yearwise1 = yearwise1.TrimEnd('~');
                    gc.insert_date(orderNumber, ParcelNumber, 1053, yearwise1, 1, DateTime.Now);

                    //Tax Information Details

                    driver.Navigate().GoToUrl("http://ttc.co.santa-cruz.ca.us/taxbills/");
                    string AnnualandTaxtypehistory = "";
                    driver.FindElement(By.Id("Parcel")).SendKeys(ParcelNumber);
                    gc.CreatePdf(orderNumber, ParcelNumber, "Enter ParcelNumber Before", driver, "CA", "Santa Cruz");
                    IWebElement IAddressSearch1 = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr[2]/td[2]/table/tbody/tr[3]/td/table[1]/tbody/tr/td/table/tbody/tr[3]/td[2]/input"));
                    IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                    js.ExecuteScript("arguments[0].click();", IAddressSearch1);
                    Thread.Sleep(5000);
                    gc.CreatePdf(orderNumber, ParcelNumber, "Tax  Info", driver, "CA", "Santa Cruz");

                    //Annual Tax Bill Details
                    //Supplemental and Default Taxes
                    string alertmessage = "";
                    for (int q = 2; q < 20; q++)
                    {
                        try
                        {
                            string SupplementalComments = "";
                            try
                            {
                                string supplementaltax = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr[2]/td[2]/table/tbody/tr[3]/td/table[1]/tbody/tr[3]/td/div")).Text.Trim();
                                if (supplementaltax.Contains("Prior Year Taxes defaulted."))
                                {

                                    SupplementalComments = "Past Due or Defaulted";
                                    alertmessage = SupplementalComments;
                                }
                            }
                            catch { }
                            string AnnualandTaxtype = "", AnnualTaxyear = "", TaxType = "", AnnualTaxyear1 = "", AnnualTaxyear2 = "", AnnualTaxyear3 = "", TaxType1 = "", TaxType2 = "", TaxType3 = "";
                            IWebElement taxinfo = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr[2]/td[2]/table/tbody/tr[3]/td/table[" + q + "]/tbody"));
                            IList<IWebElement> taxinfoTR = taxinfo.FindElements(By.TagName("tr"));
                            IList<IWebElement> taxinfoTD;

                            foreach (IWebElement Taxinfor in taxinfoTR)
                            {
                                taxinfoTD = Taxinfor.FindElements(By.TagName("td"));
                                if (taxinfoTD.Count == 2 && taxinfoTD.Count != 4 && !Taxinfor.Text.Contains("Review/Pay Other Tax Bills"))
                                {
                                    AnnualandTaxtype = taxinfoTD[0].Text;
                                    string[] AnnualandTaxtypesplit = AnnualandTaxtype.Split();
                                    AnnualTaxyear1 = AnnualandTaxtypesplit[0].Trim();
                                    AnnualTaxyear2 = AnnualandTaxtypesplit[1].Trim();
                                    AnnualTaxyear3 = AnnualandTaxtypesplit[2].Trim();
                                    AnnualTaxyear = AnnualTaxyear1 + AnnualTaxyear2 + AnnualTaxyear3;
                                    TaxType1 = AnnualandTaxtypesplit[3].Trim() + "   ";
                                    TaxType2 = AnnualandTaxtypesplit[4].Trim() + "   ";
                                    TaxType3 = AnnualandTaxtypesplit[5].Trim();
                                    TaxType = TaxType1 + TaxType2 + TaxType3;

                                }
                                if (taxinfoTD.Count != 0 && taxinfoTD.Count == 4 && !Taxinfor.Text.Contains("Review/Pay Other Tax Bills"))
                                {

                                    string Installmenttype = taxinfoTD[0].Text;
                                    string Duedate = taxinfoTD[1].Text;
                                    string Paiddate = taxinfoTD[2].Text;
                                    string Totalamount = taxinfoTD[3].Text;

                                    string taxinfodetails = AnnualTaxyear + "~" + TaxType + "~" + Installmenttype + "~" + Duedate + "~" + Paiddate + "~" + Totalamount + "~" + alertmessage;
                                    gc.insert_date(orderNumber, ParcelNumber, 1054, taxinfodetails, 1, DateTime.Now);
                                }
                            }
                        }
                        catch { }
                    }

                    //"Show Tax Bill History"
                    string AnnualandTaxtypeHistory = "", AnnualTaxyearHistory = "", TaxTypeHistory = "";
                    List<string> seedetails = new List<string>();
                    try
                    {
                        ITaxbillhistorySearch = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr[2]/td[2]/table/tbody/tr[3]/td/table[3]/tbody/tr/td[2]/form/input"));

                    }
                    catch { }
                    try
                    {
                        ITaxbillhistorySearch = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr[2]/td[2]/table/tbody/tr[3]/td/table[6]/tbody/tr/td[2]/form/input"));

                    }
                    catch { }
                    try
                    {
                        ITaxbillhistorySearch = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr[2]/td[2]/table/tbody/tr[3]/td/table[4]/tbody/tr/td[2]/form/input"));

                    }
                    catch { }
                    IJavaScriptExecutor js2 = driver as IJavaScriptExecutor;
                    js.ExecuteScript("arguments[0].click();", ITaxbillhistorySearch);
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, ParcelNumber, "Show Tax Bill History", driver, "CA", "Santa Cruz");
                    for (int i = 2; i < 20; i++)
                    {
                        try
                        {
                            IWebElement taxinfohistory = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr[2]/td[2]/table/tbody/tr[3]/td/table[" + i + "]"));
                            IList<IWebElement> taxinfohistoryTR = taxinfohistory.FindElements(By.TagName("tr"));
                            IList<IWebElement> taxinfohistoryTD;

                            foreach (IWebElement Taxinformation in taxinfohistoryTR)
                            {
                                taxinfohistoryTD = Taxinformation.FindElements(By.TagName("td"));
                                if (taxinfohistoryTD.Count == 2 && taxinfohistoryTD.Count != 4 && !Taxinformation.Text.Contains("Review/Pay Other Tax Bills"))
                                {
                                    AnnualandTaxtypeHistory = taxinfohistoryTD[0].Text;
                                    string[] AnnualandTaxtypeHistorysplit = AnnualandTaxtypeHistory.Split();
                                    string AnnualTaxyearHistory1 = AnnualandTaxtypeHistorysplit[0].Trim();
                                    string AnnualTaxyearHistory2 = AnnualandTaxtypeHistorysplit[1].Trim();
                                    string AnnualTaxyearHistory3 = AnnualandTaxtypeHistorysplit[2].Trim();
                                    AnnualTaxyearHistory = AnnualTaxyearHistory1 + AnnualTaxyearHistory2 + AnnualTaxyearHistory3;
                                    string TaxTypeHistory1 = AnnualandTaxtypeHistorysplit[3].Trim() + "   ";
                                    string TaxTypeHistory2 = AnnualandTaxtypeHistorysplit[4].Trim() + "   ";
                                    string TaxTypeHistory3 = AnnualandTaxtypeHistorysplit[5].Trim();
                                    TaxTypeHistory = TaxTypeHistory1 + TaxTypeHistory2 + TaxTypeHistory3;
                                    if (i < 5)
                                    {
                                        try
                                        {
                                            IWebElement taxinfohis = taxinfohistoryTD[1].FindElement(By.TagName("a"));
                                            string seedata = taxinfohis.GetAttribute("href");
                                            seedetails.Add(seedata);
                                        }
                                        catch { }
                                    }
                                }
                                if (taxinfohistoryTD.Count != 0 && taxinfohistoryTD.Count == 4 && !Taxinformation.Text.Contains("Review/Pay Other Tax Bills"))
                                {
                                    string Installmenttypehistory = taxinfohistoryTD[0].Text;
                                    string Taxamounthistory = taxinfohistoryTD[1].Text;
                                    string Paiddatehistory = taxinfohistoryTD[2].Text;
                                    string Totalamounthistory = taxinfohistoryTD[3].Text;

                                    string taxinfohistorydetails = AnnualTaxyearHistory + "~" + TaxTypeHistory + "~" + Installmenttypehistory + "~" + Taxamounthistory + "~" + Paiddatehistory + "~" + Totalamounthistory;
                                    gc.insert_date(orderNumber, ParcelNumber, 1055, taxinfohistorydetails, 1, DateTime.Now);
                                }
                            }

                        }
                        catch { }
                    }
                    //Current Year Tax Details

                    try
                    {
                        IWebElement Icurrentyeartaxdetails = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr[2]/td[2]/table/tbody/tr[3]/td/table[2]/tbody/tr[1]/td[2]/a"));
                        IJavaScriptExecutor js3 = driver as IJavaScriptExecutor;
                        js.ExecuteScript("arguments[0].click();", Icurrentyeartaxdetails);
                        Thread.Sleep(2000);
                    }
                    catch { }
                    //Tax information Details2
                    string annualtaxyearvalue = "", annualtaxyeartitle = "";
                    try
                    {

                        string taxratearea1 = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr[2]/td[2]/table/tbody/tr[3]/td/table/tbody/tr[2]/td[2]")).Text.Trim();
                        string taxratearea = GlobalClass.After(taxratearea1, "Tax Rate Area").Trim();
                        string address1 = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr[2]/td[2]/table/tbody/tr[3]/td/table/tbody/tr[3]/td[2]")).Text.Trim();
                        string address = GlobalClass.After(address1, "Address").Trim();
                        string taxinfodetails2 = taxratearea + "~" + address + "~" + taxauthority;
                        gc.insert_date(orderNumber, ParcelNumber, 1062, taxinfodetails2, 1, DateTime.Now);
                    }
                    catch { }
                    int p = 0;
                    foreach (string seehistory in seedetails)
                    {
                        driver.Navigate().GoToUrl(seehistory);
                        gc.CreatePdf(orderNumber, ParcelNumber, "Icurrent Yeartaxdetails Pdf" + p, driver, "CA", "Santa Cruz");
                        //Tax year details
                        string annualhistoryyear = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr[2]/td[2]/table/tbody/tr[3]/td/table/tbody/tr[2]/td[1]")).Text.Trim();
                        string[] splitannualhistoryyear = annualhistoryyear.Split();
                        string annualtaxyear1 = splitannualhistoryyear[0].Trim();
                        string annualtaxyear2 = splitannualhistoryyear[1].Trim();
                        string annualtaxyear3 = splitannualhistoryyear[2].Trim();
                        annualtaxyearvalue = annualtaxyear1 + annualtaxyear2 + annualtaxyear3.Trim();
                        string annualtaxyeartitle1 = splitannualhistoryyear[3].Trim();
                        string annualtaxyeartitle2 = splitannualhistoryyear[4].Trim();
                        string annualtaxyeartitle3 = splitannualhistoryyear[5].Trim();
                        annualtaxyeartitle = annualtaxyeartitle1 + annualtaxyeartitle2 + annualtaxyeartitle3.Trim();

                        //Tax Distributon Details
                        IWebElement taxdistribution = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr[2]/td[2]/table/tbody/tr[3]/td/table/tbody/tr[4]/td/table/tbody/tr[4]/td/table"));
                        IList<IWebElement> taxdistributionTR = taxdistribution.FindElements(By.TagName("tr"));
                        IList<IWebElement> taxdistributionTD;

                        foreach (IWebElement taxdistributiondetails in taxdistributionTR)
                        {
                            taxdistributionTD = taxdistributiondetails.FindElements(By.TagName("td"));

                            if (taxdistributiondetails.Text.Contains("TOTAL") && taxdistributionTD.Count != 0)
                            {

                                string Taxingagency = taxdistributionTD[0].Text;
                                string Rate = taxdistributionTD[1].Text;
                                string Amount = taxdistributionTD[2].Text;
                                string taxdistributiondetailsdata = annualtaxyearvalue + "~" + annualtaxyeartitle + "~" + Taxingagency + "~" + Rate + "~" + Amount;
                                gc.insert_date(orderNumber, ParcelNumber, 1060, taxdistributiondetailsdata, 1, DateTime.Now);

                            }
                            if (taxdistributionTD.Count != 0 && taxdistributionTD.Count == 3 && !taxdistributiondetails.Text.Contains("BASIC PROPERTY TAXES") && !taxdistributiondetails.Text.Contains("TOTAL"))
                            {
                                string Taxingagency = taxdistributionTD[0].Text;
                                string Rate = taxdistributionTD[1].Text;
                                string Amount = taxdistributionTD[2].Text;
                                string taxdistributiondetailsdata = annualtaxyearvalue + "~" + annualtaxyeartitle + "~" + Taxingagency + "~" + Rate + "~" + Amount;
                                gc.insert_date(orderNumber, ParcelNumber, 1060, taxdistributiondetailsdata, 1, DateTime.Now);
                            }
                        }

                        //Tax Distribution Installments
                        IWebElement taxinstallments = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr[2]/td[2]/table/tbody/tr[3]/td/table/tbody/tr[4]/td/table/tbody/tr[6]/td/table"));
                        IList<IWebElement> taxinstallmentsTR = taxinstallments.FindElements(By.TagName("tr"));
                        IList<IWebElement> taxinstallmentsTD;

                        foreach (IWebElement taxinstallmentsdetails in taxinstallmentsTR)
                        {
                            taxinstallmentsTD = taxinstallmentsdetails.FindElements(By.TagName("td"));

                            if (taxinstallmentsTD.Count != 0 && taxinstallmentsTD.Count == 4 && !taxinstallmentsdetails.Text.Contains("FIRST INSTALLMENT"))
                            {
                                string Taxingagency = taxinstallmentsTD[0].Text;
                                string FirstRate = taxinstallmentsTD[1].Text;
                                string SecondRate = taxinstallmentsTD[2].Text;
                                string Amount = taxinstallmentsTD[3].Text;
                                string taxinstallmentsdetailsdata = annualtaxyearvalue + "~" + Taxingagency + "~" + FirstRate + "~" + SecondRate + "~" + Amount;
                                gc.insert_date(orderNumber, ParcelNumber, 1063, taxinstallmentsdetailsdata, 1, DateTime.Now);
                            }
                        }
                        p++;
                    }


                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Quit();
                    gc.mergpdf(orderNumber, "CA", "Santa Cruz");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "CA", "Santa Cruz", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
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

