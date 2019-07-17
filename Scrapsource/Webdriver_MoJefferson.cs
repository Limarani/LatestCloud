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
    public class Webdriver_MoJefferson
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();
        string Mailing_Address, YearBuilt;
        IWebElement Addresstabledata;
        public string FTP_Jefferson(string streetno, string direction, string streetname, string city, string streettype, string unitnumber, string ownernm, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "", AppraisedValue="";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new ChromeDriver();
            driver = new PhantomJSDriver();
            try
            {
                StartTime = DateTime.Now.ToString("HH:mm:ss");
                driver.Navigate().GoToUrl("http://jeffersonmo.devnetwedge.com/");
                driver.FindElement(By.XPath("/html/body/div[2]/div[2]/div/div[1]/button[1]")).SendKeys(Keys.Enter);
                Thread.Sleep(2000);
                gc.CreatePdf(orderNumber, parcelNumber, "Site load", driver, "MO", "Jefferson");
                if (searchType == "titleflex")
                {
                    string address = streetno + " " + streettype + " " + streetname + "" + unitnumber;
                    gc.TitleFlexSearch(orderNumber, "", "", address, "MO", "Jefferson");
                    if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                    {
                        driver.Quit();
                        return "MultiParcel";
                    }
                    else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                    {
                        HttpContext.Current.Session["Nodata_MOJeferrson"] = "Yes";
                        driver.Quit();
                        return "No Data Found";
                    }
                    parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                    searchType = "parcel";
                }
                if (searchType == "address")
                {
                    driver.FindElement(By.Id("house-number-min")).SendKeys(streetno);
                    driver.FindElement(By.Id("house-number-max")).SendKeys(streetno);
                    driver.FindElement(By.Id("street-name")).SendKeys(streetname.Trim() + " " + streettype.Trim());
                    gc.CreatePdf_WOP(orderNumber, "SearchBefore", driver, "MO", "Jefferson");
                    Thread.Sleep(2000);

                    IWebElement Iparcel = driver.FindElement(By.Id("parcel"));
                    IList<IWebElement> IparcelRow = Iparcel.FindElements(By.TagName("button"));
                    foreach (IWebElement parcel in IparcelRow)
                    {
                        if (parcel.Text != "" && parcel.Text == "Search")
                        {
                            parcel.Click();
                            break;
                        }
                    }

                    Thread.Sleep(2000);
                    gc.CreatePdf_WOP(orderNumber, "SearchAfter", driver, "MO", "Jefferson");
                    try
                    {
                        string currentYear = DateTime.Now.Year.ToString();
                        string AddressMerge = "", strParcelNumber = "";
                        AddressMerge = streetno.ToUpper() + " " + streetname.ToUpper();
                        IWebElement EntriesTable = driver.FindElement(By.Id("search-results_info"));
                        string Entries = gc.Between(EntriesTable.Text, "of", "entries").Trim();
                        if (Convert.ToInt32(Entries) > 20)
                        {
                            HttpContext.Current.Session["multiParcel_Jefferson_Multicount"] = "Maximum";
                            gc.CreatePdf_WOP(orderNumber, "MultyAddressSearch", driver, "MO", "Jefferson");
                            gc.CreatePdf_WOP(orderNumber, "Multiple Parcel Maximum", driver, "MO", "Jefferson");
                            driver.Quit();
                            return "Maximum";
                        }
                        if (Convert.ToInt32(Entries) <= 20)
                        {
                            IWebElement Addressmergetable = driver.FindElement(By.XPath("//*[@id='search-results']/tbody"));
                            IList<IWebElement> Addressrow = Addressmergetable.FindElements(By.TagName("tr"));
                            IList<IWebElement> Addressid;
                            List<string> multiparcel = new List<string>();
                            foreach (IWebElement Addressc in Addressrow)
                            {
                                Addressid = Addressc.FindElements(By.TagName("td"));
                                if (Addressc.Text.Contains("No data available in table"))
                                {
                                    HttpContext.Current.Session["Nodata_MOJeferrson"] = "Yes";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                                if (Addressid.Count != 0)
                                {
                                    if (Addressc.Text.Contains(AddressMerge))
                                    {
                                        multiparcel.Add(Addressid[1].Text);
                                        Mailing_Address = Addressid[4].Text;
                                        strParcelNumber = Addressid[1].Text.Replace(".", "").Replace("-", "");
                                    }
                                }
                                if (Addressid.Count != 0)
                                {
                                    string parcelId = Addressid[1].Text;
                                    string owner = Addressid[2].Text;
                                    string address = Addressid[3].Text;
                                    string multiDetails = owner + "~" + address;
                                    gc.insert_date(orderNumber, parcelId, 666, multiDetails, 1, DateTime.Now);
                                }
                            }

                            if (multiparcel.Count == 1)
                            {
                                driver.Navigate().GoToUrl("http://jeffersonmo.devnetwedge.com/view/RE/" + strParcelNumber + "/" + currentYear + "");

                            }
                            if (multiparcel.Count > 1)
                            {
                                HttpContext.Current.Session["multiParcel_Jefferson"] = "Yes";
                                gc.CreatePdf_WOP(orderNumber, "Multiple Parcel", driver, "MO", "Jefferson");
                                driver.Quit();
                                return "MultiParcel";
                            }
                        }

                    }
                    catch { }
                }
                if (searchType == "parcel")
                {
                    driver.FindElement(By.Id("property-key")).SendKeys(parcelNumber);
                    IWebElement Iparcel = driver.FindElement(By.Id("parcel"));
                    IList<IWebElement> IparcelRow = Iparcel.FindElements(By.TagName("button"));
                    foreach (IWebElement parcel in IparcelRow)
                    {
                        if (parcel.Text != "" && parcel.Text == "Search")
                        {
                            parcel.Click();
                            break;
                        }
                    }
                    gc.CreatePdf_WOP(orderNumber, "Parcel Number", driver, "MO", "Jefferson");
                    try
                    {
                        IWebElement INodata = driver.FindElement(By.Id("search-results"));
                        if (INodata.Text.Contains("No data available in table"))
                        {
                            HttpContext.Current.Session["Nodata_MOJeferrson"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                }
                if (searchType == "ownername")
                {
                    driver.FindElement(By.Id("owner-name")).SendKeys(ownernm);
                    Thread.Sleep(2000);
                    gc.CreatePdf_WOP(orderNumber, "OwnerName", driver, "MO", "Jefferson");

                    IWebElement Iparcel = driver.FindElement(By.Id("parcel"));
                    IList<IWebElement> IparcelRow = Iparcel.FindElements(By.TagName("button"));
                    foreach (IWebElement parcel in IparcelRow)
                    {
                        if (parcel.Text != "" && parcel.Text == "Search")
                        {
                            parcel.Click();
                            break;
                        }
                    }

                    Thread.Sleep(2000);
                    gc.CreatePdf_WOP(orderNumber, "After OwnerName", driver, "MO", "Jefferson");
                    try
                    {
                        IWebElement EntriesTable = driver.FindElement(By.Id("search-results_info"));
                        string Entries = gc.Between(EntriesTable.Text, "of", "entries").Trim();
                        if (Convert.ToInt32(Entries) > 20)
                        {
                            HttpContext.Current.Session["multiParcel_Jefferson_Multicount"] = "Maximum";
                            gc.CreatePdf_WOP(orderNumber, "MultyAddressSearch", driver, "MO", "Jefferson");
                            gc.CreatePdf_WOP(orderNumber, "Multiple Parcel Maximum", driver, "MO", "Jefferson");
                            driver.Quit();
                            return "Maximum";
                        }
                        if (Convert.ToInt32(Entries) <= 20)
                        {
                            IWebElement Ownermulti = driver.FindElement(By.XPath("//*[@id='search-results']/tbody"));
                            IList<IWebElement> Ownermultirow = Ownermulti.FindElements(By.TagName("tr"));
                            IList<IWebElement> Ownenid;
                            foreach (IWebElement Owner in Ownermultirow)
                            {
                                Ownenid = Owner.FindElements(By.TagName("td"));
                                if (Owner.Text.Contains("No data available in table"))
                                {
                                    HttpContext.Current.Session["Nodata_MOJeferrson"] = "Yes";
                                    driver.Quit();
                                    return "No Data Found";
                                }
                                if (Ownenid.Count != 0)
                                {
                                    string parcelId = Ownenid[1].Text;
                                    string owner = Ownenid[2].Text;
                                    string address = Ownenid[3].Text;
                                    string multiDetails = owner + "~" + address;
                                    gc.insert_date(orderNumber, parcelId, 666, multiDetails, 1, DateTime.Now);
                                }
                            }
                            HttpContext.Current.Session["multiParcel_Jefferson"] = "Yes";
                            gc.CreatePdf_WOP(orderNumber, "Multiple Parcel", driver, "MO", "Jefferson");
                            driver.Quit();
                            return "MultiParcel";
                        }

                    }

                    catch
                    { }

                }

               

                IWebElement PropertydetailTable = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div[1]/div[2]/div[2]"));
                string parcel_Number = gc.Between(PropertydetailTable.Text, "Parcel Number", "Tax Year");
                string TaxYear1 = gc.Between(PropertydetailTable.Text, "Tax Year", "Class").Trim();
                string TaxYear = TaxYear1.Substring(0, 4);
                string TaxCode = gc.Between(PropertydetailTable.Text, "Tax Code", "Land Use");
                string Class = gc.Between(PropertydetailTable.Text, "Class", "Tax Code");
                string LandUse = gc.Between(PropertydetailTable.Text, "Land Use", "Site Address");
                string SiteAddress = gc.Between(PropertydetailTable.Text, "Site Address", "Mapped Acres");
                string MappedAcres = gc.Between(PropertydetailTable.Text, "Mapped Acres", "Assessed Value");
                string AssessedValue = gc.Between(PropertydetailTable.Text, "Assessed Value", "Tax Rate");
                string TaxRate = gc.Between(PropertydetailTable.Text, "Tax Rate", "Public Notes");
                string LegalDescription = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div[1]/div[4]/table/tbody/tr/td[1]")).Text;
                string SectionTownship_Range = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div[1]/div[4]/table/tbody/tr/td[2]")).Text;
                IWebElement Propertyownerdetail = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div[1]/div[7]/div[2]/div/div[1]"));
                string PropertyOwner = gc.Between(Propertyownerdetail.Text, "Property Owner", "Address");
                try { 
            AppraisedValue = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div[1]/div[9]/table/tbody/tr/td[3]")).Text;
                }
                catch { }
                try
                {
                    YearBuilt = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div[1]/div[10]/div[2]/table/tbody/tr/td[5]")).Text;
                }
                catch { }
                string Propertyresult = TaxYear + "~" + Class + "~" + TaxCode + "~" + LandUse + "~" + SiteAddress + "~" + MappedAcres + "~" + AssessedValue + "~" + TaxRate + "~" + LegalDescription + "~" + SectionTownship_Range + "~" + PropertyOwner + "~" + Mailing_Address + "~" + AppraisedValue + "~" + YearBuilt;
                gc.insert_date(orderNumber, parcel_Number, 646, Propertyresult, 1, DateTime.Now);
                //gc.CreatePdf(orderNumber, parcel_Number, "PropertyDetail", driver, "MO", "Jefferson");
                try
                {
                    IWebElement AssessmentTable = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div[1]/div[11]/table/tbody"));
                    IList<IWebElement> Assessmentrow = AssessmentTable.FindElements(By.TagName("tr"));
                    IList<IWebElement> Assessmentid;
                    foreach (IWebElement Assessment in Assessmentrow)
                    {
                        Assessmentid = Assessment.FindElements(By.TagName("td"));
                        if (Assessmentid.Count != 0)
                        {
                            string Assessmentresult = Assessmentid[0].Text + "~" + Assessmentid[1].Text + "~" + Assessmentid[2].Text + "~" + Assessmentid[3].Text + "~" + Assessmentid[4].Text + "~" + Assessmentid[5].Text + "~" + Assessmentid[6].Text;
                            gc.insert_date(orderNumber, parcel_Number, 648, Assessmentresult, 1, DateTime.Now);
                        }
                    }
                }
                catch { }
                // gc.CreatePdf(orderNumber, parcel_Number, "Assessment Detail", driver, "MO", "Jefferson");
                IWebElement BillDetailtable = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div[1]/div[5]/div[1]/div/table/tbody"));
                IList<IWebElement> Billdetailrow = BillDetailtable.FindElements(By.TagName("tr"));
                IList<IWebElement> Billdetailid;
                IList<IWebElement> BillDetailTH;
                foreach (IWebElement Billing in Billdetailrow)
                {
                    BillDetailTH = Billing.FindElements(By.TagName("th"));
                    Billdetailid = Billing.FindElements(By.TagName("td"));
                    if (Billdetailid.Count != 0 && !Billing.Text.Contains("Billing Details"))
                    {
                        string Billingresult = BillDetailTH[0].Text + "~" + Billdetailid[0].Text;
                        gc.insert_date(orderNumber, parcel_Number, 651, Billingresult, 1, DateTime.Now);
                    }
                }
                // gc.CreatePdf(orderNumber, parcel_Number, "Billing Detail", driver, "MO", "Jefferson");
                IWebElement Taxduetable = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div[1]/div[5]/div[2]/div/table/tbody"));
                IList<IWebElement> Taxduerow = Taxduetable.FindElements(By.TagName("tr"));
                IList<IWebElement> Taxdueid;
                foreach (IWebElement Taxdue in Taxduerow)
                {
                    Taxdueid = Taxdue.FindElements(By.TagName("td"));
                    if (Taxdueid.Count != 0)
                    {
                        string Taxdueresult = Taxdueid[0].Text + "~" + Taxdueid[1].Text;
                        gc.insert_date(orderNumber, parcel_Number, 654, Taxdueresult, 1, DateTime.Now);
                    }
                }
                gc.CreatePdf(orderNumber, parcel_Number, "Taxdue Detail", driver, "MO", "Jefferson");
                IWebElement PaymentHistorytable = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div[1]/div[6]/table/tbody"));
                IList<IWebElement> PaymentHistoryrow = PaymentHistorytable.FindElements(By.TagName("tr"));
                IList<IWebElement> PaymentHistoryid;
                foreach (IWebElement Payment in PaymentHistoryrow)
                {
                    PaymentHistoryid = Payment.FindElements(By.TagName("td"));
                    if (PaymentHistoryid.Count != 0)
                    {
                        string Paymentresult = PaymentHistoryid[0].Text + "~" + PaymentHistoryid[1].Text + "~" + PaymentHistoryid[2].Text + "~" + PaymentHistoryid[3].Text + "~" + PaymentHistoryid[4].Text;
                        gc.insert_date(orderNumber, parcel_Number, 656, Paymentresult, 1, DateTime.Now);
                    }
                }
                //  gc.CreatePdf(orderNumber, parcel_Number, "PaymentHistory Detail", driver, "MO", "Jefferson");
                try
                {
                    IWebElement Taxbodiestable = driver.FindElement(By.XPath("//*[@id='taxing-bodies-table']/tbody"));
                    IList<IWebElement> TaxbodiesRow = Taxbodiestable.FindElements(By.TagName("tr"));
                    IList<IWebElement> taxbodiesid;
                    foreach (IWebElement Taxbodies in TaxbodiesRow)
                    {
                        taxbodiesid = Taxbodies.FindElements(By.TagName("td"));
                        if (taxbodiesid.Count != 0)
                        {
                            string Taxbodiesresult = taxbodiesid[0].Text + "~" + taxbodiesid[1].Text + "~" + taxbodiesid[2].Text;
                            gc.insert_date(orderNumber, parcel_Number, 665, Taxbodiesresult, 1, DateTime.Now);
                        }
                    }
                    gc.CreatePdf(orderNumber, parcel_Number, "Tax Bodies Detail", driver, "MO", "Jefferson");
                }
                catch { }

                try
                {
                    IWebElement Linktaxreceipt = driver.FindElement(By.XPath("//*[@id='sidebar']/ul/li[11]/a"));
                    string linktaxreceipt = Linktaxreceipt.GetAttribute("href");
                    gc.downloadfile(linktaxreceipt, orderNumber, parcel_Number, "Tax Receipt PDF", "MO", "Jefferson");
                }
                catch
                { }

                TaxTime = DateTime.Now.ToString("HH:mm:ss");
                LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                gc.insert_TakenTime(orderNumber, "MO", "Jefferson", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
                gc.mergpdf(orderNumber, "MO", "Jefferson");
                driver.Quit();
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