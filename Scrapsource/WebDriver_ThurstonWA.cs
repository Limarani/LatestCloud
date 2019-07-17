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
    public class WebDriver_ThurstonWA
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_ThurstonWA(string streetNo, string streetName, string direction, string streetType, string unitNumber, string parcelNumber, string ownerName, string searchType, string orderNumber)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string Taxing_Authority = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            // driver = new PhantomJSDriver();
            //driver = new ChromeDriver();
            string address = "";
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("http://tcproperty.co.thurston.wa.us/propsql/front.asp");
                    Thread.Sleep(3000);
                    driver.FindElement(By.XPath("/html/body/div[2]/form/input[2]")).Click();
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    if (direction != "")
                    {
                        address = streetNo + " " + direction + " " + streetName + " " + streetType + " " + unitNumber;
                    }
                    else
                    {
                        address = streetNo + " " + streetName + " " + streetType + " " + unitNumber;
                    }

                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, "", ownerName, address.Trim(), "WA", "Thurston");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_ThurstonWA"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        driver.FindElement(By.XPath("/html/body/div/form/table/tbody/tr/td/table[2]/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr[3]/td[2]/input")).SendKeys(streetNo);
                        driver.FindElement(By.XPath("/html/body/div/form/table/tbody/tr/td/table[2]/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr[4]/td[2]/input")).SendKeys(streetName);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search Input", driver, "WA", "Thurston");
                        driver.FindElement(By.XPath("/html/body/div/form/table/tbody/tr/td/table[3]/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr/td[1]/input")).SendKeys(Keys.Enter);
                        gc.CreatePdf_WOP(orderNumber, "Address Search result", driver, "WA", "Thurston");
                        //Multiparcel
                        try
                        {
                            int owner = 0;
                            IWebElement Imultitable = driver.FindElement(By.XPath("/html/body/div/table[2]/tbody"));
                            IList<IWebElement> ImutiRow = Imultitable.FindElements(By.TagName("tr"));
                            IList<IWebElement> ImultiTD;
                            foreach (IWebElement multi in ImutiRow)
                            {
                                ImultiTD = multi.FindElements(By.TagName("td"));
                                if (ImultiTD.Count != 0 && multi.Text.Contains("Owner"))
                                {
                                    string[] ownernamesplit = ImultiTD[4].Text.Split(' ');
                                    string parcelnumber = ownernamesplit[0].Trim();
                                    string strmultiDetails = ImultiTD[0].Text + "~" + ImultiTD[2].Text;
                                    gc.insert_date(orderNumber, parcelnumber, 1578, strmultiDetails, 1, DateTime.Now);
                                    owner++;
                                }

                            }
                            if (owner == 1)
                            {
                                driver.FindElement(By.XPath("/html/body/div/table[2]/tbody/tr[2]/td[5]/a[1]")).Click();
                                Thread.Sleep(2000);
                            }
                            if (owner > 2 && owner < 25)
                            {
                                HttpContext.Current.Session["multiParcel_ThurstonWA"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (owner > 25)
                            {
                                HttpContext.Current.Session["multiParcel_ThurstonWA_Maximum"] = "Maximum";
                                return "Maximum";
                            }
                        }
                        catch { }
                        gc.CreatePdf(orderNumber, parcelNumber, "Basic", driver, "WA", "Thurston");
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("ctlBodyPane_noDataList_pnlNoResults")).Text;
                            if (nodata.Contains("No results match your search criteria."))
                            {
                                HttpContext.Current.Session["Nodata_ThurstonWA"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.XPath("/html/body/div/form/table/tbody/tr/td/table[1]/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr/td[2]/input")).SendKeys(parcelNumber);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Input", driver, "WA", "Thurston");
                        driver.FindElement(By.XPath("/html/body/div/form/table/tbody/tr/td/table[3]/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr/td[1]/input")).SendKeys(Keys.Enter);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search result", driver, "WA", "Thurston");
                    }
                    string Lastname = "", Firstname = "";
                    if (searchType == "ownername")
                    {
                        string[] ownernamesplit = ownerName.Split(' ');
                        try
                        {
                            Lastname = ownernamesplit[0];
                            Firstname = ownernamesplit[1];
                        }
                        catch { }
                        driver.FindElement(By.XPath("/html/body/div/form/table/tbody/tr/td/table[2]/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr[1]/td[2]/input")).SendKeys(Lastname);
                        driver.FindElement(By.Id("ffname")).SendKeys(Firstname);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Ownername Search Input", driver, "WA", "Thurston");
                        driver.FindElement(By.XPath("/html/body/div/form/table/tbody/tr/td/table[3]/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr/td[1]/input")).SendKeys(Keys.Enter);
                        gc.CreatePdf_WOP(orderNumber, "Ownername Search result", driver, "WA", "Thurston");

                        //Multiparcel
                        try
                        {
                            int owner = 0;
                            IWebElement Imultitable = driver.FindElement(By.XPath("/html/body/div/table[2]/tbody"));
                            IList<IWebElement> ImutiRow = Imultitable.FindElements(By.TagName("tr"));
                            IList<IWebElement> ImultiTD;
                            foreach (IWebElement multi in ImutiRow)
                            {
                                ImultiTD = multi.FindElements(By.TagName("td"));
                                if (ImultiTD.Count != 0 && multi.Text.Contains("Owner"))
                                {
                                    string[] ownernamesplit1 = ImultiTD[4].Text.Split(' ');
                                    string parcelnumber = ownernamesplit1[0].Trim();
                                    string strmultiDetails = ImultiTD[0].Text + "~" + ImultiTD[2].Text;
                                    gc.insert_date(orderNumber, parcelnumber, 1578, strmultiDetails, 1, DateTime.Now);
                                    owner++;
                                }

                            }
                            if (owner == 1)
                            {
                                driver.FindElement(By.XPath("/html/body/div/table[2]/tbody/tr[2]/td[5]/a[1]")).Click();
                                Thread.Sleep(2000);
                            }

                            if (owner > 2 && owner < 25)
                            {

                                HttpContext.Current.Session["multiParcel_ThurstonWA"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (owner > 25)
                            {
                                HttpContext.Current.Session["multiParcel_ThurstonWA_Maximum"] = "Maximum";
                                return "Maximum";
                            }
                        }
                        catch { }
                        gc.CreatePdf(orderNumber, parcelNumber, "Basic", driver, "WA", "Thurston");
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("ctlBodyPane_noDataList_pnlNoResults")).Text;
                            if (nodata.Contains("No results match your search criteria."))
                            {
                                HttpContext.Current.Session["Nodata_ThurstonWA"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    //Property Details
                    //Basic Info
                    string TaxAuthority = "";
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div/table[2]/tbody/tr[2]/td[5]/a[1]")).Click();
                        Thread.Sleep(2000);
                    }
                    catch { }
                    int ownerinfo = 0;
                    string Parcelnumber = "", Role = "", Pct = "", Name = "", Name1 = "", Name2 = "", Street = "", City = "", State = "", Zip = "", SitusAddress = "", AbbreviatedLegal = "", SectTownRange = "", Size = "", UseCode = "", TCANumber = "", Taxable = "", Neighborhood = "", PropertyType = "", YearBuilt = "", LotAcreage = "";
                    Parcelnumber = driver.FindElement(By.XPath("/html/body/div[1]/table[1]/tbody/tr/td/table/tbody/tr/td/table/tbody/tr/td[2]/table/tbody/tr[2]/td/b")).Text.Replace("Property:", "");
                    //Structures
                    gc.CreatePdf(orderNumber, Parcelnumber, "BasicBasic", driver, "WA", "Thurston");
                    IWebElement IPropertyDetails = driver.FindElement(By.XPath("/html/body/div[2]/table/tbody/tr/td/table[1]/tbody/tr/td/table/tbody/tr[2]/td/table/tbody"));
                    IList<IWebElement> IPropertyRow = IPropertyDetails.FindElements(By.TagName("tr"));
                    IList<IWebElement> IPropertyTD;
                    foreach (IWebElement Propertydet in IPropertyRow)
                    {
                        IPropertyTD = Propertydet.FindElements(By.TagName("td"));
                        if (IPropertyTD.Count != 0 && IPropertyTD.Count == 3 && Propertydet.Text.Contains("Owner"))
                        {
                            Role = IPropertyTD[0].Text;
                            Pct = IPropertyTD[1].Text;
                            Name1 = IPropertyTD[2].Text;
                        }
                        if (IPropertyTD.Count != 0 && IPropertyTD.Count == 6)
                        {
                            Street = IPropertyTD[1].Text;
                            City = IPropertyTD[2].Text;
                            State = IPropertyTD[3].Text;
                            Zip = IPropertyTD[5].Text;
                            break;
                        }
                    }
                    //owername2
                    string taxpayee = driver.FindElement(By.XPath("/html/body/div[2]/table/tbody/tr/td/table[1]/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr[4]/td[1]")).Text.Trim();
                    try
                    {
                        if (!taxpayee.Contains("Taxpayer"))
                        {
                            Name2 = driver.FindElement(By.XPath("/html/body/div[2]/table/tbody/tr/td/table[1]/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr[4]/td[3]")).Text.Trim();
                        }
                    }
                    catch { }
                    Name = Name1 + "  " + Name2;
                    //Sitrus
                    //UseCode = "", TCANumber = "", Taxable = "", Neighborhood = "", PropertyType = "", YearBuilt = "", LotAcreage = "";
                    IWebElement IPropertyDetails1 = driver.FindElement(By.XPath("/html/body/div[2]/table/tbody/tr/td/table[2]/tbody/tr/td/table/tbody/tr[2]/td/table/tbody"));
                    IList<IWebElement> IPropertyRow1 = IPropertyDetails1.FindElements(By.TagName("tr"));
                    IList<IWebElement> IPropertyTD1;
                    foreach (IWebElement Propertydet1 in IPropertyRow1)
                    {
                        IPropertyTD1 = Propertydet1.FindElements(By.TagName("td"));
                        if (IPropertyTD1.Count != 0 && IPropertyTD1.Count == 2 && Propertydet1.Text.Contains("Situs Address:"))
                        {
                            SitusAddress = IPropertyTD1[1].Text;
                        }
                        if (IPropertyTD1.Count != 0 && IPropertyTD1.Count == 2 && Propertydet1.Text.Contains("Abbreviated Legal:"))
                        {
                            AbbreviatedLegal = IPropertyTD1[1].Text;
                        }
                        if (IPropertyTD1.Count != 0 && IPropertyTD1.Count == 2 && Propertydet1.Text.Contains("Sect/Town/Range:"))
                        {
                            SectTownRange = IPropertyTD1[1].Text;
                        }
                        if (IPropertyTD1.Count != 0 && IPropertyTD1.Count == 2 && Propertydet1.Text.Contains("Size:"))
                        {
                            Size = IPropertyTD1[1].Text;
                        }
                        if (IPropertyTD1.Count != 0 && IPropertyTD1.Count == 2 && Propertydet1.Text.Contains("Use Code:"))
                        {
                            UseCode = IPropertyTD1[1].Text;
                        }
                        if (IPropertyTD1.Count != 0 && IPropertyTD1.Count == 2 && Propertydet1.Text.Contains("TCA Number:"))
                        {
                            TCANumber = IPropertyTD1[1].Text;
                        }
                        if (IPropertyTD1.Count != 0 && IPropertyTD1.Count == 2 && Propertydet1.Text.Contains("Taxable:"))
                        {
                            Taxable = IPropertyTD1[1].Text;
                        }
                        if (IPropertyTD1.Count != 0 && IPropertyTD1.Count == 2 && Propertydet1.Text.Contains("Neighborhood:"))
                        {
                            Neighborhood = IPropertyTD1[1].Text;
                        }
                        if (IPropertyTD1.Count != 0 && IPropertyTD1.Count == 2 && Propertydet1.Text.Contains("Property Type:"))
                        {
                            PropertyType = IPropertyTD1[1].Text;
                        }
                    }
                    Taxing_Authority = driver.FindElement(By.XPath("/html/body/div[2]/div[2]/p/table/tbody")).Text.Trim();
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/table[2]/tbody/tr/td/table/tbody/tr/td/table[2]/tbody/tr/td/table[1]/tbody/tr/td[3]/a/img")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Structues Pdf", driver, "WA", "Thurston");

                    }
                    catch { }
                    try
                    {
                        YearBuilt = driver.FindElement(By.XPath("/html/body/div[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr[1]/td[2]")).Text.Trim();
                    }
                    catch { }
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/table[2]/tbody/tr/td/table/tbody/tr/td/table[2]/tbody/tr/td/table[1]/tbody/tr/td[4]/a/img")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Land Pdf", driver, "WA", "Thurston");

                    }
                    catch { }
                    LotAcreage = driver.FindElement(By.XPath("/html/body/div[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[2]/td[1]/table/tbody/tr[3]/td[3]")).Text.Trim();

                    string Property_Details = Role + "~" + Pct + "~" + Name + "~" + Street + "~" + City + "~" + State + "~" + Zip + "~" + SitusAddress + "~" + AbbreviatedLegal + "~" + SectTownRange + "~" + Size + "~" + UseCode + "~" + TCANumber + "~" + Taxable + "~" + Neighborhood + "~" + PropertyType + "~" + YearBuilt + "~" + LotAcreage + "~" + Taxing_Authority;
                    gc.insert_date(orderNumber, Parcelnumber, 1569, Property_Details, 1, DateTime.Now);

                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/table[2]/tbody/tr/td/table/tbody/tr/td/table[2]/tbody/tr/td/table[1]/tbody/tr/td[9]/a/img")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Owner History Pdf", driver, "WA", "Thurston");

                    }
                    catch { }
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/table[2]/tbody/tr/td/table/tbody/tr/td/table[2]/tbody/tr/td/table[2]/tbody/tr/td[1]/a/img")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Values Pdf", driver, "WA", "Thurston");
                    }
                    catch { }
                    //Assessment Details                    
                    string TaxyearTitle = "", AssessmentTitle = "", AssessmentValue = "", Activeexemption = "", Taxyear1 = "", Assessmentyear = "", Marketvalueland = "", Marketvaluebuildings = "", Marketvaluetotal = "";
                    IWebElement IAssessmentDetails = driver.FindElement(By.XPath("/html/body/div[2]/table/tbody/tr/td/table[1]/tbody/tr/td/table/tbody/tr[2]/td/table/tbody"));
                    IList<IWebElement> IAssessmentRow = IAssessmentDetails.FindElements(By.TagName("tr"));
                    IList<IWebElement> IAssessmentTD;
                    IList<IWebElement> IAssessmentTH;
                    foreach (IWebElement Assessment in IAssessmentRow)
                    {
                        IAssessmentTD = Assessment.FindElements(By.TagName("td"));
                        IAssessmentTH = Assessment.FindElements(By.TagName("th"));
                        if (IAssessmentTH.Count != 0 && (Assessment.Text.Contains("Tax Year") || Assessment.Text.Contains("Assessment Year")))
                        {
                            AssessmentTitle += IAssessmentTH[0].Text + "~";
                            TaxyearTitle += IAssessmentTH[1].Text + "~";
                            Assessmentyear += IAssessmentTH[2].Text + "~";
                            Marketvalueland += IAssessmentTH[3].Text + "~";
                        }
                        if (IAssessmentTD.Count != 0 && (Assessment.Text.Contains("Market Value Land") || Assessment.Text.Contains("Market Value Buildings") || Assessment.Text.Contains("Market Value Total")))
                        {
                            AssessmentTitle += IAssessmentTD[0].Text + "~";
                            TaxyearTitle += IAssessmentTD[1].Text + "~";
                            Assessmentyear += IAssessmentTD[2].Text + "~";
                            Marketvalueland += IAssessmentTD[3].Text + "~";

                        }
                    }
                    db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + AssessmentTitle.Remove(AssessmentTitle.Length - 1, 1) + "' where Id = '" + 1570 + "'");
                    gc.insert_date(orderNumber, Parcelnumber, 1570, TaxyearTitle.Remove(TaxyearTitle.Length - 1, 1), 1, DateTime.Now);
                    gc.insert_date(orderNumber, Parcelnumber, 1570, Assessmentyear.Remove(Assessmentyear.Length - 1, 1), 1, DateTime.Now);
                    gc.insert_date(orderNumber, Parcelnumber, 1570, Marketvalueland.Remove(Marketvalueland.Length - 1, 1), 1, DateTime.Now);
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/table[2]/tbody/tr/td/table/tbody/tr/td/table[2]/tbody/tr/td/table[2]/tbody/tr/td[4]/a/img")).Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Taxes Pdf", driver, "WA", "Thurston");
                    }
                    catch { }
                    driver.SwitchTo().Window(driver.WindowHandles.Last());
                    Thread.Sleep(5000);

                    //Tax Information Details
                    string propertyAddress = "", propertyDecription = "", propertyTaxCode = "", propertyUseCode = "", yearBuilt = "", strownerName = "", TaxableValue = "", ExemptionAmount = "", AssessedValue = "", TaxYear1 = "", TaxYear2 = "", TaxYear3 = "", ActiveExemption = "", MarketTotal = "", MarketLand = "", MarketImprovement = "", PersonalProperty = "";
                    //Property Values
                    IWebElement IValues = driver.FindElement(By.Id("mPropertyValues"));
                    IList<IWebElement> IValuesRow = IValues.FindElements(By.TagName("tr"));
                    IList<IWebElement> IValuesTD;
                    IList<IWebElement> IValuesTH;
                    foreach (IWebElement value in IValuesRow)
                    {
                        IValuesTD = value.FindElements(By.TagName("td"));
                        IValuesTH = value.FindElements(By.TagName("th"));
                        if (IValuesTH.Count != 0 && value.Text.Contains("Tax Year"))
                        {
                            TaxYear1 = IValuesTH[1].Text.Replace("Tax Year", "").Replace("\r\n", "").Trim() + "~";
                            TaxYear2 = IValuesTH[2].Text.Replace("Tax Year", "").Replace("\r\n", "").Trim() + "~";
                            TaxYear3 = IValuesTH[3].Text.Replace("Tax Year", "").Replace("\r\n", "").Trim() + "~";
                        }
                        if (IValuesTD.Count != 0 && value.Text.Contains("Taxable Value Regular"))
                        {
                            TaxYear1 += IValuesTD[1].Text + "~";
                            TaxYear2 += IValuesTD[2].Text + "~";
                            TaxYear3 += IValuesTD[3].Text + "~";
                        }
                        if (IValuesTD.Count != 0 && value.Text.Contains("Exemption Amount Regular"))
                        {
                            TaxYear1 += IValuesTD[1].Text + "~";
                            TaxYear2 += IValuesTD[2].Text + "~";
                            TaxYear3 += IValuesTD[3].Text + "~";
                        }
                        if (IValuesTD.Count != 0 && value.Text.Contains("Market Total"))
                        {
                            TaxYear1 += IValuesTD[1].Text + "~";
                            TaxYear2 += IValuesTD[2].Text + "~";
                            TaxYear3 += IValuesTD[3].Text + "~";
                        }
                        if (IValuesTD.Count != 0 && value.Text.Contains("Assessed Value"))
                        {
                            TaxYear1 += IValuesTD[1].Text + "~";
                            TaxYear2 += IValuesTD[2].Text + "~";
                            TaxYear3 += IValuesTD[3].Text + "~";
                        }
                        if (IValuesTD.Count != 0 && value.Text.Contains("Market Land"))
                        {
                            TaxYear1 += IValuesTD[1].Text + "~";
                            TaxYear2 += IValuesTD[2].Text + "~";
                            TaxYear3 += IValuesTD[3].Text + "~";
                        }
                        if (IValuesTD.Count != 0 && value.Text.Contains("Market Improvement"))
                        {
                            TaxYear1 += IValuesTD[1].Text + "~";
                            TaxYear2 += IValuesTD[2].Text + "~";
                            TaxYear3 += IValuesTD[3].Text + "~";
                        }
                        if (IValuesTD.Count != 0 && value.Text.Contains("Personal Property"))
                        {
                            TaxYear1 += IValuesTD[1].Text + "~";
                            TaxYear2 += IValuesTD[2].Text + "~";
                            TaxYear3 += IValuesTD[3].Text + "~";
                        }
                    }
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Details Pdf", driver, "WA", "Thurston");
                    IWebElement IExempet = driver.FindElement(By.Id("mActiveExemptions"));
                    IList<IWebElement> IExempetRow = IExempet.FindElements(By.TagName("tr"));
                    IList<IWebElement> IExempetTD;
                    foreach (IWebElement value in IExempetRow)
                    {
                        IExempetTD = value.FindElements(By.TagName("td"));
                        if (IExempetTD.Count != 0)
                        {
                            ActiveExemption = IExempetTD[0].Text;
                        }
                    }
                    gc.insert_date(orderNumber, Parcelnumber, 1571, TaxYear1 + ActiveExemption, 1, DateTime.Now);
                    gc.insert_date(orderNumber, Parcelnumber, 1571, TaxYear2, 1, DateTime.Now);
                    gc.insert_date(orderNumber, Parcelnumber, 1571, TaxYear3, 1, DateTime.Now);

                    //Tax Installments Payable Details Table:
                    //Tax Detailed Statement Table:
                    IWebElement IDistribution = driver.FindElement(By.Id("mCurrentTaxesDistribution"));
                    IList<IWebElement> IDistributionRow = IDistribution.FindElements(By.TagName("tr"));
                    IList<IWebElement> IDistributionTD;
                    foreach (IWebElement distribution in IDistributionRow)
                    {
                        IDistributionTD = distribution.FindElements(By.TagName("td"));
                        if (IDistributionTD.Count != 0)
                        {
                            string DistributionDetails = IDistributionTD[0].Text + "~" + IDistributionTD[1].Text + "~" + IDistributionTD[2].Text + "~" + IDistributionTD[3].Text + "~" + IDistributionTD[4].Text;
                            gc.insert_date(orderNumber, Parcelnumber, 1572, DistributionDetails, 1, DateTime.Now);
                        }
                    }
                    //Levy Rate History Table
                    IWebElement ILevyrate = driver.FindElement(By.Id("mLevyRateHistory"));
                    IList<IWebElement> ILevyrateRow = ILevyrate.FindElements(By.TagName("tr"));
                    IList<IWebElement> ILevyrateTD;
                    foreach (IWebElement Levyrate in ILevyrateRow)
                    {
                        ILevyrateTD = Levyrate.FindElements(By.TagName("td"));
                        if (ILevyrateTD.Count != 0)
                        {
                            string LevyrateDetails = ILevyrateTD[0].Text + "~" + ILevyrateTD[1].Text;
                            gc.insert_date(orderNumber, Parcelnumber, 1581, LevyrateDetails, 1, DateTime.Now);
                        }
                    }
                    //Tax Payment Details
                    IWebElement IReceipt = driver.FindElement(By.Id("mReceipts"));
                    IList<IWebElement> IReceiptRow = IReceipt.FindElements(By.TagName("tr"));
                    IList<IWebElement> IReceiptTD;
                    foreach (IWebElement receipt in IReceiptRow)
                    {
                        IReceiptTD = receipt.FindElements(By.TagName("td"));
                        if (IReceiptTD.Count != 0)
                        {
                            string ReceiptDetails = IReceiptTD[0].Text + "~" + IReceiptTD[1].Text + "~" + IReceiptTD[2].Text + "~" + IReceiptTD[3].Text + "~" + IReceiptTD[4].Text + "~" + IReceiptTD[5].Text;
                            gc.insert_date(orderNumber, Parcelnumber, 1573, ReceiptDetails, 1, DateTime.Now);
                        }
                    }
                    ////Installments and Charges Details
                    try
                    {
                        IWebElement IInstallment = driver.FindElement(By.Id("mTaxChargesBalancePayment"));
                        IList<IWebElement> IInstallmentRow = IInstallment.FindElements(By.TagName("tr"));
                        IList<IWebElement> IInstallmentTD;
                        foreach (IWebElement install in IInstallmentRow)
                        {
                            IInstallmentTD = install.FindElements(By.TagName("td"));
                            if (IInstallmentTD.Count != 0)
                            {
                                string InstallmentDetails = IInstallmentTD[0].Text + "~" + IInstallmentTD[1].Text + "~" + IInstallmentTD[2].Text + "~" + IInstallmentTD[3].Text + "~" + IInstallmentTD[4].Text + "~" + IInstallmentTD[5].Text + "~" + IInstallmentTD[6].Text;
                                gc.insert_date(orderNumber, Parcelnumber, 1577, InstallmentDetails, 1, DateTime.Now);
                            }
                            //Tax Year~Installment~Earliest Due Date~Principal~Interest Penalties and Costs~Total Due~Cumulative Due
                        }
                    }
                    catch { }
                    //Good Through Details
                    try
                    {
                        List<string> billinfo = new List<string>();
                        string Bill_Flag = driver.FindElement(By.XPath("//*[@id='mTaxChargesBalancePayment']/tbody/tr[2]/td[5]")).Text;
                        string Good_through_date = "";
                        if (Bill_Flag != "$0.00")
                        {

                            IWebElement href = driver.FindElement(By.Id("mFuturePayoff"));
                            string addview = href.GetAttribute("href");
                            driver.Navigate().GoToUrl(addview);
                            gc.CreatePdf(orderNumber, parcelNumber, "Calculate Future Payoff", driver, "WA", "Thurston");

                            IWebElement good_date = driver.FindElement(By.Id("mDate"));
                            Good_through_date = good_date.GetAttribute("value");

                            DateTime G_Date = Convert.ToDateTime(Good_through_date);
                            string dateChecking = DateTime.Now.ToString("MM") + "/15/" + DateTime.Now.ToString("yyyy");

                            if (G_Date < Convert.ToDateTime(dateChecking))
                            {
                                //end of the month
                                Good_through_date = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")))).ToString("MM/dd/yyyy");

                            }

                            else if (G_Date >= Convert.ToDateTime(dateChecking))
                            {
                                // nextEndOfMonth 
                                if ((Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))) < 12))
                                {
                                    Good_through_date = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM")) + 1), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")) + 1)).ToString("MM/dd/yyyy");
                                }
                                else
                                {
                                    int nxtYr = Convert.ToInt16(DateTime.Now.ToString("yyyy")) + 1;
                                    Good_through_date = new DateTime(nxtYr, 1, DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), 1)).ToString("MM/dd/yyyy");

                                }
                            }
                            driver.FindElement(By.Id("mDate")).Clear();
                            driver.FindElement(By.XPath("//*[@id='mDate']")).SendKeys(Good_through_date);
                            driver.FindElement(By.Id("mCalculate")).SendKeys(Keys.Enter);
                            gc.CreatePdf(orderNumber, Parcelnumber, "Calculate Future Payoff1", driver, "WA", "Thurston");

                            string asofdate = "", Principal = "", Interestandpenalties = "", Totaldue = "";

                            asofdate = driver.FindElement(By.Id("mDisplayDate")).Text;

                            IWebElement TaxInfoTB = driver.FindElement(By.Id("mGrid"));
                            IList<IWebElement> TaxInfoTR = TaxInfoTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> TaxInfoTD;

                            foreach (IWebElement TaxInfo in TaxInfoTR)
                            {
                                TaxInfoTD = TaxInfo.FindElements(By.TagName("td"));
                                if (TaxInfoTD.Count != 0 && TaxInfoTD.Count == 3 && !TaxInfo.Text.Contains("Principal"))
                                {
                                    Principal = TaxInfoTD[0].Text;
                                    Interestandpenalties = TaxInfoTD[1].Text;
                                    Totaldue = TaxInfoTD[2].Text;
                                    string TaxdelinqInfo_Details = asofdate.Trim() + "~" + Principal.Trim() + "~" + Interestandpenalties.Trim() + "~" + Totaldue.Trim();
                                    gc.insert_date(orderNumber, Parcelnumber, 1579, TaxdelinqInfo_Details, 1, DateTime.Now);
                                }
                            }
                        }
                    }
                    catch { }
                    try
                    {
                        IWebElement Backtoproperty = driver.FindElement(By.XPath("//*[@id='mMainHeader_SiteMapPath1']/span[5]/a"));
                        Backtoproperty.Click();
                        Thread.Sleep(2000);
                    }
                    catch { }
                    try
                    {
                        IWebElement viewdetailsclick = driver.FindElement(By.Id("mDetailedStatement"));
                        viewdetailsclick.Click();
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, Parcelnumber, "View Details", driver, "WA", "Thurston");
                        //driver.Navigate().Back();
                    }
                    catch { }

                    try
                    {////*[@id="mPageHeader_SiteMapPath1"]/span[1]/a
                        IWebElement Backtoproperty = driver.FindElement(By.XPath("//*[@id='mPageHeader_SiteMapPath1']/span[1]/a"));
                        Backtoproperty.Click();
                        Thread.Sleep(2000);
                    }
                    catch { }

                    //try
                    //{
                    //    IWebElement IUIDChrges = driver.FindElement(By.Id("mULID"));
                    //    IUIDChrges.Click();
                    //    gc.CreatePdf(orderNumber, Parcelnumber, "Tax Charges", driver, "WA", "Thurston");
                    //    TaxAuthority = GlobalClass.After(IUIDChrges.Text, "Send to ").Trim();
                    //    IWebElement ICharges = driver.FindElement(By.Id("mTaxChargesBalancePayment"));
                    //    IList<IWebElement> IChargesRow = ICharges.FindElements(By.TagName("tr"));
                    //    IList<IWebElement> IChargesTD;
                    //    foreach (IWebElement charges in IChargesRow)
                    //    {
                    //        IChargesTD = charges.FindElements(By.TagName("td"));
                    //        if (IChargesTD.Count != 0)
                    //        {
                    //            string ChargesDetails = IChargesTD[0].Text + "~" + IChargesTD[1].Text + "~" + IChargesTD[2].Text + "~" + IChargesTD[3].Text + "~" + IChargesTD[4].Text + "~" + IChargesTD[5].Text + "~" + IChargesTD[6].Text + "~" + TaxAuthority;
                    //            gc.insert_date(orderNumber, Parcelnumber, 1580, ChargesDetails, 1, DateTime.Now);
                    //        }
                    //    }
                    //}
                    //catch { }

                    //Tax Balance Details
                    try
                    {
                        // driver.FindElement(By.XPath("//*[@id='mErrorMessageLabel']/a")).Click();
                        // driver.SwitchTo().Window(driver.WindowHandles.Last());
                        driver.Navigate().GoToUrl("https://www.paydici.com/thurston-county-wa/search/new");
                        Thread.Sleep(2000);
                        driver.FindElement(By.Id("q")).SendKeys(Parcelnumber.Trim());
                        gc.CreatePdf(orderNumber, Parcelnumber, "Tax Balace Result1", driver, "WA", "Thurston");
                        driver.FindElement(By.XPath("//*[@id='main']/div[3]/div/div[5]/div/form/input[2]")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, Parcelnumber, "Tax Balace Result2", driver, "WA", "Thurston");
                        //driver.FindElement(By.XPath("//*[@id='main']/div[3]/div/div[2]/div[2]/div[1]/div/table/tbody")).Click();
                        //Thread.Sleep(5000);
                        // gc.CreatePdf(orderNumber, Parcelnumber, "Tax Balace Result3", driver, "WA", "Thurston");

                        try
                        {
                            IWebElement IAddressSearch1 = driver.FindElement(By.Id("bill_group_" + Parcelnumber.Trim()));
                            IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                            js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                            Thread.Sleep(9000);
                            gc.CreatePdf(orderNumber, Parcelnumber, "Tax Balace Result3", driver, "WA", "Thurston");
                        }
                        catch { }
                        string Taxyeees = "", Amountdue = "", Total = "";
                        IWebElement taxbal = driver.FindElement(By.XPath("//*[@id='main']/div[3]/div/div[2]/div[2]/div[1]/div/table/tbody"));
                        IList<IWebElement> taxbalRow = taxbal.FindElements(By.TagName("tr"));
                        IList<IWebElement> taxbalTDstrong;
                        IList<IWebElement> taxbalTDspan;//This bill is now past the due date, additional charges may apply.
                        foreach (IWebElement taxbal1 in taxbalRow)
                        {
                            taxbalTDstrong = taxbal1.FindElements(By.TagName("td"));
                            if (taxbalTDstrong.Count != 0 && taxbalTDstrong.Count == 1)
                            {
                                string[] Taxyearsplit = taxbalTDstrong[0].Text.Split('\r');
                                for (int i = 0; i < Taxyearsplit.Count(); i++)
                                {
                                    if (Taxyearsplit[i].Contains("Property Taxes"))
                                    {
                                        Taxyeees = Taxyearsplit[i].Replace("Property Taxes", "");
                                    }
                                    if (Taxyearsplit[i].Contains("1st Half"))
                                    {
                                        Amountdue = Taxyearsplit[i].Trim();
                                    }
                                    if (Taxyearsplit[i].Contains("Full Year"))
                                    {
                                        Total = Taxyearsplit[i].Trim();
                                    }
                                }
                                string Taxbalacedetails = Taxyeees + "~" + Amountdue + "~" + Total;
                                gc.insert_date(orderNumber, Parcelnumber, 1580, Taxbalacedetails, 1, DateTime.Now);
                                Taxyeees = ""; Amountdue = ""; Total = "";
                            }
                        }
                    }
                    catch { }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "WA", "Thurston", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "WA", "Thurston");
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