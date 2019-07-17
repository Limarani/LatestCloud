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
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_LakeIL
    {

        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();

        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_LakeIL(string houseno, string Direction, string sname, string stype, string unitNumber, string zipcode, string parcelNumber, string ownername, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string address = "", lastName = "", firstName = "", Pinnumber = "", PropertyAdd = "", Strownername = "", Pin = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            // using (driver = new ChromeDriver())
            {


                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");


                    if (searchType == "titleflex")
                    {
                        if (Direction != "")
                        {
                            address = houseno + " " + Direction + " " + sname + " " + stype + " " + unitNumber;
                            address = address.Trim();
                        }
                        if (Direction == "")
                        {
                            address = houseno + " " + sname + " " + stype + " " + unitNumber;
                            address = address.Trim();
                        }
                        string titleaddress = address;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "IL", "Lake");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Zero_LakeIL"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("https://apps03.lakecountyil.gov/comparables/PTAIAddress.aspx");
                        Thread.Sleep(4000);

                        driver.FindElement(By.XPath("//*[@id='tbStreetNum']")).SendKeys(houseno);
                        driver.FindElement(By.Id("tbStreetName")).SendKeys(sname);
                        try
                        {
                            IWebElement streettype = driver.FindElement(By.Id("ddlStreetType"));
                            SelectElement sttype = new SelectElement(streettype);
                            sttype.SelectByValue(stype);
                        }
                        catch { }
                        driver.FindElement(By.Id("tbZip")).SendKeys(zipcode);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "IL", "Lake");
                        driver.FindElement(By.Id("cmdSubmit")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        gc.CreatePdf_WOP(orderNumber, "Address search Result", driver, "IL", "Lake");

                        try
                        {
                            int Max = 0;
                            string strowner = "", strAddress = "", strCity = "";
                            string Record = "";
                            IWebElement multiaddress;

                            multiaddress = driver.FindElement(By.Id("Table2"));

                            IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> multiTD;
                            foreach (IWebElement multi in multiRow)
                            {
                                multiTD = multi.FindElements(By.TagName("td"));
                                if (multiTD.Count != 0 && multiRow.Count >= 1 && multiRow.Count <= 25 && multi.Text.Trim() != "")
                                {

                                    strAddress = multiTD[0].Text.Trim();
                                    parcelNumber = GlobalClass.After(strAddress, "=").Trim();
                                    PropertyAdd = GlobalClass.Before(strAddress, "=").Trim();

                                    string multidetails = PropertyAdd;
                                    gc.insert_date(orderNumber, parcelNumber, 1861, multidetails, 1, DateTime.Now);
                                    Max++;
                                }
                                if (multiTD.Count != 0 && multiRow.Count > 25)
                                {
                                    HttpContext.Current.Session["multiparcel_LakeIL_Maximum"] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }

                            }
                            if (Max > 1 && Max < 26)
                            {
                                HttpContext.Current.Session["multiparcel_LakeIL"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }

                            if (Max == 0)
                            {
                                HttpContext.Current.Session["Zero_LakeIL"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }

                        }
                        catch { }
                    }



                    else if (searchType == "parcel")
                    {

                        driver.Navigate().GoToUrl("https://apps03.lakecountyil.gov/comparables/PTAIPIN.aspx");
                        Thread.Sleep(4000);

                        driver.FindElement(By.Id("txtPIN")).SendKeys(parcelNumber);
                        Thread.Sleep(3000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel search", driver, "IL", "Lake");
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='Button2']")).SendKeys(Keys.Enter);
                            Thread.Sleep(3000);
                        }
                        catch { }

                        try
                        {
                            int Max = 0;
                            string strowner = "", strAddress = "", strCity = "";
                            string Record = "";
                            IWebElement multiaddress;

                            gc.CreatePdf(orderNumber, parcelNumber, "Parcel search Result", driver, "IL", "Lake");

                            multiaddress = driver.FindElement(By.Id("Table2"));

                            IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> multiTD;
                            foreach (IWebElement multi in multiRow)
                            {
                                multiTD = multi.FindElements(By.TagName("td"));
                                if (multiTD.Count != 0 && multiRow.Count >= 1 && multiRow.Count <= 25 && multi.Text.Trim() != "")
                                {


                                    strAddress = multiTD[0].Text.Trim();
                                    parcelNumber = GlobalClass.After(strAddress, "=").Trim();
                                    PropertyAdd = GlobalClass.Before(strAddress, "=").Trim();

                                    string multidetails = PropertyAdd;
                                    gc.insert_date(orderNumber, parcelNumber, 1861, multidetails, 1, DateTime.Now);
                                    Max++;
                                }
                                if (multiTD.Count != 0 && multiRow.Count > 25)
                                {
                                    HttpContext.Current.Session["multiparcel_LakeIL_Maximum"] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }

                            }
                            if (Max > 1 && Max < 26)
                            {
                                HttpContext.Current.Session["multiparcel_LakeIL"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }

                            if (Max == 0)
                            {
                                HttpContext.Current.Session["Zero_LakeIL"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }

                        }
                        catch { }
                    }



                    //property details

                    string streetAddress = "", City = "", Zipcode = "", LandAmount = "";
                    string BuildingAmount = "", TotalAmount = "", Township = "", AssessDate = "", ClassDesc = "", YearBuilt = "";

                    string bulkdata = driver.FindElement(By.XPath("//*[@id='PropertyCharacteristics1_tblPropertyAddress']/tbody")).Text;
                    parcelNumber = gc.Between(bulkdata, "Pin", "Street Address").Replace(":", "").Replace("-", "").Trim();

                    streetAddress = gc.Between(bulkdata, "Street Address", "City").Replace(":", "").Trim();
                    City = gc.Between(bulkdata, "City", "Zip Code").Replace(":", "").Trim();
                    Zipcode = gc.Between(bulkdata, "Zip Code", "Land Amount").Replace(":", "").Trim();
                    LandAmount = gc.Between(bulkdata, "Land Amount", "Building Amount").Replace(":", "").Trim();
                    BuildingAmount = gc.Between(bulkdata, "Building Amount", "Total Amount").Replace(":", "").Trim();
                    TotalAmount = gc.Between(bulkdata, "Total Amount", "Township").Replace(":", "").Trim();
                    Township = gc.Between(bulkdata, "Township", "Assessment Date").Replace(":", "").Trim();
                    AssessDate = GlobalClass.After(bulkdata, "Assessment Date").Replace(":", "").Trim();
                    ClassDesc = driver.FindElement(By.XPath("//*[@id='PropertyCharacteristics1_lblClassDescription']")).Text;
                    try
                    {

                        YearBuilt = driver.FindElement(By.Id("PropertyCharacteristics1_lblEffectiveAge")).Text;
                    }
                    catch { }

                    string propertydetails = streetAddress + "~" + City + "~" + Zipcode + "~" + LandAmount + "~" + BuildingAmount + "~" + TotalAmount + "~" + Township + "~" + AssessDate + "~" + ClassDesc + "~" + YearBuilt;
                    gc.insert_date(orderNumber, parcelNumber, 1793, propertydetails, 1, DateTime.Now);

                    gc.CreatePdf(orderNumber, parcelNumber, "Property Details", driver, "IL", "Lake");



                    // Tax Information Details
                    string taxAuth = "", taxauth1 = "", taxauth2 = "";
                    try
                    {
                        driver.Navigate().GoToUrl("http://www.lakecountyil.gov/508/Current-Payment-Status");
                        taxAuth = driver.FindElement(By.XPath("//*[@id='divInfoAdvae35de24-85e5-460b-b9ce-18eda922e6cb']/div[1]/div/div/ol/li/span")).Text.Replace("Contact Us", "").Replace("Lake County, IL", "").Replace("Parking and Directions", "").Trim();
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Authority", driver, "IL", "Lake");
                    }
                    catch { }
                    driver.Navigate().GoToUrl("https://apps03.lakecountyil.gov/treasurer/collbook/collbook2.asp");
                    Thread.Sleep(5000);

                    driver.FindElement(By.XPath("//*[@id='table53']/tbody/tr[3]/td/input")).SendKeys(parcelNumber);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Search", driver, "IL", "Lake");
                    driver.FindElement(By.XPath("//*[@id='table53']/tbody/tr[5]/td/input")).Click();
                    Thread.Sleep(4000);
                    try
                    {
                        driver.SwitchTo().Window(driver.WindowHandles.Last());
                        Thread.Sleep(2000);
                    }
                    catch { }
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Search Result", driver, "IL", "Lake");
                    int taxyear = 0;
                    string tax_year = "", tax_year1 = "";
                    tax_year = driver.FindElement(By.Id("DTLNavigator_ddTaxYear")).Text;
                    tax_year1 = GlobalClass.Before(tax_year, "\r\n").Trim();
                    int Taxmonth = DateTime.Now.Month;
                    if (Taxmonth >= 9)
                    {
                        taxyear = Convert.ToInt32(tax_year1);
                    }
                    else
                    {
                        taxyear = Convert.ToInt32(tax_year1) - 1;
                    }


                    string PropertyLocation = "", PropertyLocation2 = "", TaxYear = "", TaxCode = "", Acres = "", LegalDesc1 = "", LegalDesc2 = "", LegalDesc3 = "", LegalDesc = "";

                    PropertyLocation = driver.FindElement(By.XPath("//*[@id='datalet_header_row']/td/table/tbody/tr[4]/td[2]")).Text;
                    TaxYear = driver.FindElement(By.XPath("//*[@id='datalet_header_row']/td/table/tbody/tr[5]/td[1]")).Text;
                    TaxYear = gc.Between(TaxYear, "Tax Year", "Taxes").Replace(":", "").Replace("(", "").Trim();
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='sidemenu']/li[1]/a/span")).Click();
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Acre - Info", driver, "IL", "Lake");
                        Acres = driver.FindElement(By.XPath("//*[@id='Parcel']/tbody/tr[15]/td[2]")).Text;
                    }
                    catch { }

                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='sidemenu']/li[2]/a/span")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Legal Information", driver, "IL", "Lake");
                        TaxCode = driver.FindElement(By.XPath("//*[@id='Legal Description']/tbody/tr[9]/td[2]")).Text;
                        LegalDesc1 = driver.FindElement(By.XPath("//*[@id='Legal Description']/tbody/tr[4]/td[2]")).Text.Trim();
                        LegalDesc2 = driver.FindElement(By.XPath("//*[@id='Legal Description']/tbody/tr[5]/td[2]")).Text.Trim();
                        LegalDesc3 = driver.FindElement(By.XPath("//*[@id='Legal Description']/tbody/tr[6]/td[2]")).Text.Trim();
                        LegalDesc = LegalDesc1 + " " + LegalDesc2 + " " + LegalDesc3;
                        LegalDesc = LegalDesc.Trim();
                    }
                    catch { }


                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Bill" + TaxYear, driver, "IL", "Lake");

                    try
                    {
                        string TaxInfodetails = PropertyLocation + "~" + TaxYear + "~" + TaxCode + "~" + Acres + "~" + LegalDesc + "~" + taxAuth;
                        gc.insert_date(orderNumber, parcelNumber, 1800, TaxInfodetails, 1, DateTime.Now);

                    }
                    catch { }

                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='sidemenu']/li[5]/a/span")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Summary" + TaxYear, driver, "IL", "Lake");
                    }
                    catch { }
                    // Taxing Body Details
                    try
                    {
                        IWebElement TaxBody = driver.FindElement(By.XPath("//*[@id='Property Tax by Entity']/tbody"));
                        IList<IWebElement> TRTaxBody = TaxBody.FindElements(By.TagName("tr"));
                        IList<IWebElement> THTaxBody = TaxBody.FindElements(By.TagName("th"));
                        IList<IWebElement> TDTaxBody;
                        foreach (IWebElement row in TRTaxBody)
                        {
                            TDTaxBody = row.FindElements(By.TagName("td"));
                            if (TDTaxBody.Count != 0 && !row.Text.Contains("Entities") && row.Text.Trim() != "")
                            {
                                string TaxBodydetails = TDTaxBody[0].Text + "~" + TDTaxBody[1].Text + "~" + TDTaxBody[2].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1801, TaxBodydetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    // Tax Status

                    try
                    {
                        string value = "", taxyear1 = "", valuetype = "";
                        IWebElement Assessmentdetails = driver.FindElement(By.XPath("//*[@id='Tax Status']/tbody"));
                        IList<IWebElement> TRAssessmentdetails = Assessmentdetails.FindElements(By.TagName("tr"));
                        IList<IWebElement> THAssessmentdetails = Assessmentdetails.FindElements(By.TagName("th"));
                        IList<IWebElement> TDAssessmentdetails;
                        foreach (IWebElement row in TRAssessmentdetails)
                        {
                            TDAssessmentdetails = row.FindElements(By.TagName("td"));
                            if (THAssessmentdetails.Count != 0 && row.Text.Contains("Value Type"))
                            {
                                valuetype += THAssessmentdetails[0].Text + "~";
                                value += THAssessmentdetails[1].Text.Replace("Tax Year", "").Replace("\r\n", "") + "~";

                            }
                            if (TDAssessmentdetails.Count != 0 && !row.Text.Contains("Value Type") && row.Text.Trim() != "")
                            {
                                valuetype += TDAssessmentdetails[0].Text.Replace(":", "") + "~";
                                value += TDAssessmentdetails[1].Text + "~";

                            }
                        }


                        DBconnection dbconn = new DBconnection();
                        dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + valuetype.Remove(valuetype.Length-1,1) + "' where Id = '" + 1802 + "'");
                        gc.insert_date(orderNumber, parcelNumber, 1802, value.Remove(value.Length-1,1), 1, DateTime.Now);

                    }
                    catch (Exception ex) { }



                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='sidemenu']/li[7]/a/span")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Payment History", driver, "IL", "Lake");
                    }
                    catch { }

                    // Tax Payment Details
                    try
                    {
                        IWebElement Taxpayment = driver.FindElement(By.XPath("//*[@id='Payments']/tbody"));
                        IList<IWebElement> TRTaxpayment = Taxpayment.FindElements(By.TagName("tr"));
                        IList<IWebElement> THTaxpayment = Taxpayment.FindElements(By.TagName("th"));
                        IList<IWebElement> TDTaxpayment;
                        foreach (IWebElement row in TRTaxpayment)
                        {
                            TDTaxpayment = row.FindElements(By.TagName("td"));
                            if (TDTaxpayment.Count != 0 && !row.Text.Contains("Recept") && row.Text.Trim() != "")
                            {
                                string TaxpaymentHistory = TDTaxpayment[0].Text + "~" + TDTaxpayment[1].Text + "~" + TDTaxpayment[2].Text + "~" + TDTaxpayment[3].Text + "~" + TDTaxpayment[4].Text + "~" + TDTaxpayment[5].Text + "~" + TDTaxpayment[6].Text + "~" + TDTaxpayment[7].Text + "~" + TDTaxpayment[8].Text + "~" + TDTaxpayment[9].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1814, TaxpaymentHistory, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    // Tax Redemption 
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='sidemenu']/li[8]/a/span")).Click();
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax sale or Redemption", driver, "IL", "Lake");

                    }
                    catch { }
                    // Tax Sale/Redemption Receipts
                    try
                    {
                        IWebElement Taxsale = driver.FindElement(By.XPath("//*[@id='frmMain']/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/table/tbody"));
                        IList<IWebElement> TRTaxsale = Taxsale.FindElements(By.TagName("tr"));
                        IList<IWebElement> THTaxsale = Taxsale.FindElements(By.TagName("th"));
                        IList<IWebElement> TDTaxsale;
                        foreach (IWebElement row in TRTaxsale)
                        {
                            TDTaxsale = row.FindElements(By.TagName("td"));
                            if (TDTaxsale.Count != 0 && !row.Text.Contains("Sale or Redemption") && !row.Text.Contains("No Data") && row.Text.Trim() != "")
                            {
                                string TaxSaledetails = TDTaxsale[0].Text + "~" + TDTaxsale[1].Text + "~" + TDTaxsale[2].Text + "~" + TDTaxsale[3].Text + "~" + TDTaxsale[4].Text + "~" + TDTaxsale[5].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1848, TaxSaledetails, 1, DateTime.Now);
                            }
                            if (TDTaxsale.Count != 0 && !row.Text.Contains("Sale or Redemption") && row.Text.Contains("No Data") && row.Text.Trim() != "")
                            {
                                string TaxSaledetails = TDTaxsale[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                gc.insert_date(orderNumber, parcelNumber, 1848, TaxSaledetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }
                    // Tax Adjustment
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='sidemenu']/li[9]/a/span")).Click();
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Adjustment", driver, "IL", "Lake");

                    }
                    catch { }
                    // Tax Adjustment
                    try
                    {
                        IWebElement TaxAdjust = driver.FindElement(By.XPath("//*[@id='frmMain']/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/table/tbody"));
                        IList<IWebElement> TRTaxAdjust = TaxAdjust.FindElements(By.TagName("tr"));
                        IList<IWebElement> THTaxAdjust = TaxAdjust.FindElements(By.TagName("th"));
                        IList<IWebElement> TDTaxAdjust;
                        foreach (IWebElement row in TRTaxAdjust)
                        {
                            TDTaxAdjust = row.FindElements(By.TagName("td"));
                            if (TDTaxAdjust.Count != 0 && !row.Text.Contains("Sale or Redemption") && row.Text.Trim() != "" && !row.Text.Contains("No Data"))
                            {
                                string TaxAdjustmentdetails = TDTaxAdjust[0].Text + "~" + TDTaxAdjust[1].Text + "~" + TDTaxAdjust[2].Text + "~" + TDTaxAdjust[3].Text + "~" + TDTaxAdjust[4].Text + "~" + TDTaxAdjust[5].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1865, TaxAdjustmentdetails, 1, DateTime.Now);
                            }
                            if (TDTaxAdjust.Count != 0 && !row.Text.Contains("Sale or Redemption") && row.Text.Trim() != "" && row.Text.Contains("No Data"))
                            {
                                string TaxAdjustmentdetails = TDTaxAdjust[0].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1865, TaxAdjustmentdetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    // Tax Value History
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='sidemenu']/li[13]/a/span")).Click();
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Value History", driver, "IL", "Lake");

                    }
                    catch { }

                    try
                    {
                        IWebElement valuehistory = driver.FindElement(By.XPath("//*[@id='Values History']/tbody"));
                        IList<IWebElement> TRvaluehistory = valuehistory.FindElements(By.TagName("tr"));
                        IList<IWebElement> THvaluehistory = valuehistory.FindElements(By.TagName("th"));
                        IList<IWebElement> TDvaluehistory;
                        foreach (IWebElement row in TRvaluehistory)
                        {
                            TDvaluehistory = row.FindElements(By.TagName("td"));
                            if (TDvaluehistory.Count != 0 && !row.Text.Contains("Land MV") && row.Text.Trim() != "")
                            {
                                string ValueHistorydetails = TDvaluehistory[0].Text + "~" + TDvaluehistory[1].Text + "~" + TDvaluehistory[2].Text + "~" + TDvaluehistory[3].Text + "~" + TDvaluehistory[4].Text + "~" + TDvaluehistory[5].Text + "~" + TDvaluehistory[6].Text + "~" + TDvaluehistory[7].Text + "~" + TDvaluehistory[8].Text + "~" + TDvaluehistory[9].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1866, ValueHistorydetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }


                    // Tax Excemptions Current
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='sidemenu']/li[19]/a/span")).Click();
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Excemptions Current", driver, "IL", "Lake");

                    }
                    catch { }

                    string Excemptioncurrent = "", Asmtyear = "", Payyear = "", Excemptiontype = "", status = "", Receiptdate = "";
                    try
                    {
                        Excemptioncurrent = driver.FindElement(By.XPath("//*[@id='Exemptions']/tbody")).Text;
                        Asmtyear = gc.Between(Excemptioncurrent, "Asmt Year", "Pay Year").Replace(":", "").Trim();
                        Payyear = gc.Between(Excemptioncurrent, "Pay Year", "Exemption Type").Replace(":", "").Trim();
                        Excemptiontype = gc.Between(Excemptioncurrent, "Exemption Type", "Status").Replace(":", "").Trim();
                        status = gc.Between(Excemptioncurrent, "Status", "Application/Renewal Receipt Date").Replace(":", "").Trim();
                        Receiptdate = gc.Between(Excemptioncurrent, "Application/Renewal Receipt Date", "SmartFile Filing ID").Replace(":", "").Trim();

                        string TaxExcemptionCurrent = Asmtyear + "~" + Payyear + "~" + Excemptiontype + "~" + status + "~" + Receiptdate;
                        gc.insert_date(orderNumber, parcelNumber, 1867, TaxExcemptionCurrent, 1, DateTime.Now);
                    }
                    catch { }

                    // Tax Excemptions History
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='sidemenu']/li[20]/a/span")).Click();
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Excemptions History", driver, "IL", "Lake");

                    }
                    catch { }

                    try
                    {
                        IWebElement Excemptionhistory = driver.FindElement(By.XPath("//*[@id='Exemption History']/tbody"));
                        IList<IWebElement> TRExcemptionhistory = Excemptionhistory.FindElements(By.TagName("tr"));
                        IList<IWebElement> THExcemptionhistory = Excemptionhistory.FindElements(By.TagName("th"));
                        IList<IWebElement> TDExcemptionhistory;
                        foreach (IWebElement row in TRExcemptionhistory)
                        {
                            TDExcemptionhistory = row.FindElements(By.TagName("td"));
                            if (TDExcemptionhistory.Count != 0 && !row.Text.Contains("Tax Year") && row.Text.Trim() != "")
                            {
                                string ValueHistorydetails = TDExcemptionhistory[0].Text + "~" + TDExcemptionhistory[1].Text + "~" + TDExcemptionhistory[2].Text + "~" + TDExcemptionhistory[3].Text + "~" + TDExcemptionhistory[4].Text + "~" + TDExcemptionhistory[5].Text + "~" + TDExcemptionhistory[6].Text + "~" + TDExcemptionhistory[7].Text + "~" + TDExcemptionhistory[8].Text + "~" + TDExcemptionhistory[9].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1868, ValueHistorydetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    // Land Information
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='sidemenu']/li[22]/a/span")).Click();
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Land Information", driver, "IL", "Lake");

                    }
                    catch { }

                    try
                    {
                        IWebElement landsummary = driver.FindElement(By.XPath("//*[@id='Land Summary']/tbody"));
                        IList<IWebElement> TRlandsummary = landsummary.FindElements(By.TagName("tr"));
                        IList<IWebElement> THlandsummary = landsummary.FindElements(By.TagName("th"));
                        IList<IWebElement> TDlandsummary;
                        foreach (IWebElement row in TRlandsummary)
                        {
                            TDlandsummary = row.FindElements(By.TagName("td"));
                            if (TDlandsummary.Count != 0 && !row.Text.Contains("Land Description") && row.Text.Trim() != "")
                            {
                                string LandInfodetails = TDlandsummary[0].Text + "~" + TDlandsummary[1].Text + "~" + TDlandsummary[2].Text + "~" + TDlandsummary[3].Text + "~" + TDlandsummary[4].Text + "~" + TDlandsummary[5].Text + "~" + TDlandsummary[6].Text + "~" + TDlandsummary[7].Text + "~" + TDlandsummary[8].Text + "~" + TDlandsummary[9].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1869, LandInfodetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    //  Permits
                    string permittitle = "", permitvalue = "";
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='sidemenu']/li[24]/a/span")).Click();
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Permits", driver, "IL", "Lake");

                    }
                    catch { }
                    try
                    {
                        IWebElement Permitsdetails = driver.FindElement(By.XPath("//*[@id='Permit Details']/tbody"));
                        IList<IWebElement> TRPermitsdetails = Permitsdetails.FindElements(By.TagName("tr"));
                        IList<IWebElement> THPermitsdetails = Permitsdetails.FindElements(By.TagName("th"));
                        IList<IWebElement> TDPermitsdetails;
                        foreach (IWebElement row in TRPermitsdetails)
                        {
                            TDPermitsdetails = row.FindElements(By.TagName("td"));
                            if (THPermitsdetails.Count != 0 && row.Text.Trim() != "")
                            {
                                permittitle += THPermitsdetails[0].Text + "~";
                                permitvalue += THPermitsdetails[1].Text.Replace("Tax Year", "").Replace("\r\n", "") + "~";

                            }
                            if (TDPermitsdetails.Count != 0 && row.Text.Trim() != "")
                            {
                                permittitle += TDPermitsdetails[0].Text.Replace(":", "") + "~";
                                permitvalue += TDPermitsdetails[1].Text + "~";

                            }
                        }


                        DBconnection dbconn = new DBconnection();
                        dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + permittitle.Remove(permittitle.Length-1,1) + "' where Id = '" + 1870 + "'");
                        gc.insert_date(orderNumber, parcelNumber, 1870, permitvalue.Remove(permitvalue.Length-1,1), 1, DateTime.Now);
                    }
                    catch { }
                    try
                    {
                        IWebElement Permits = driver.FindElement(By.XPath("//*[@id='frmMain']/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[2]/td/div/table/tbody"));
                        IList<IWebElement> TRPermits = Permits.FindElements(By.TagName("tr"));
                        IList<IWebElement> THPermits = Permits.FindElements(By.TagName("th"));
                        IList<IWebElement> TDPermits;
                        foreach (IWebElement row in TRPermits)
                        {
                            TDPermits = row.FindElements(By.TagName("td"));
                            
                            if (TDPermits.Count != 0 && !row.Text.Contains("Land Description") && row.Text.Contains("No Data") && row.Text.Trim() != "")
                            {
                                string LandInfodetails = TDPermits[0].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1870, LandInfodetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    // Property Transfer History

                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='sidemenu']/li[27]/a/span")).Click();
                        Thread.Sleep(4000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Property Transfer History", driver, "IL", "Lake");
                    }
                    catch { }

                    try
                    {
                        IWebElement ProHistory = driver.FindElement(By.XPath("//*[@id='Sales']/tbody"));
                        IList<IWebElement> TRProHistory = ProHistory.FindElements(By.TagName("tr"));
                        IList<IWebElement> THProHistory = ProHistory.FindElements(By.TagName("th"));
                        IList<IWebElement> TDProHistory;
                        foreach (IWebElement row in TRProHistory)
                        {
                            TDProHistory = row.FindElements(By.TagName("td"));
                            if (TDProHistory.Count != 0 && !row.Text.Contains("Sale Date") && row.Text.Trim() != "")
                            {
                                string LandInfodetails = TDProHistory[0].Text + "~" + TDProHistory[1].Text + "~" + TDProHistory[2].Text + "~" + TDProHistory[3].Text + "~" + TDProHistory[4].Text + "~" + TDProHistory[5].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1871, LandInfodetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    for (int i = 0; i < 3; i++)
                    {

                        try
                        {
                            string Tyear = driver.FindElement(By.XPath("//*[@id='DTLNavigator_ddTaxYear']")).Text;
                            IWebElement Iyear = driver.FindElement(By.Id("DTLNavigator_ddTaxYear"));
                            SelectElement Iyer = new SelectElement(Iyear);
                            Iyer.SelectByValue(taxyear.ToString());
                            Thread.Sleep(4000);
                        }
                        catch { }
                        // Taxes Due Details
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='sidemenu']/li[6]/a/span")).Click();
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Tax Due" + taxyear, driver, "IL", "Lake");
                        }
                        catch { }


                        try
                        {
                            IWebElement TaxBill = driver.FindElement(By.XPath("//*[@id='Taxes Due Treasurer']/tbody"));
                            IList<IWebElement> TRTaxBill = TaxBill.FindElements(By.TagName("tr"));
                            IList<IWebElement> THTaxBill = TaxBill.FindElements(By.TagName("th"));
                            IList<IWebElement> TDTaxBill;
                            foreach (IWebElement row in TRTaxBill)
                            {
                                TDTaxBill = row.FindElements(By.TagName("td"));
                                if (TDTaxBill.Count != 0 && !row.Text.Contains("Tax Year") && row.Text.Trim() != "")
                                {
                                    string TaxDueDetails = TDTaxBill[0].Text + "~" + TDTaxBill[1].Text + "~" + TDTaxBill[2].Text + "~" + TDTaxBill[3].Text + "~" + TDTaxBill[4].Text + "~" + TDTaxBill[5].Text + "~" + TDTaxBill[6].Text + "~" + TDTaxBill[7].Text + "~" + TDTaxBill[8].Text;
                                    gc.insert_date(orderNumber, parcelNumber, 1873, TaxDueDetails, 1, DateTime.Now);
                                }
                            }
                        }
                        catch { }
                        taxyear--;
                    }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "IL", "Lake", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "IL", "Lake");
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