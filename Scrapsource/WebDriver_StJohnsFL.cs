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
    public class WebDriver_StJohnsFL
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_StJohnsFL(string address, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
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

                StartTime = DateTime.Now.ToString("HH:mm:ss");

                driver.Navigate().GoToUrl("https://qpublic.schneidercorp.com/Application.aspx?App=StJohnsCountyFL&Layer=Parcels&PageType=Search");
                Thread.Sleep(2000);
                try
                {
                    driver.FindElement(By.XPath("//*[@id='appBody']/div[4]/div/div/div[3]/a[1]")).SendKeys(Keys.Enter);
                }
                catch { }
                try
                {
                    if (searchType == "titleflex")
                    {
                        gc.TitleFlexSearch(orderNumber, "", ownername, address, "FL", "St Johns");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_FLStJohns"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_txtAddress")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "SearchAddressBefore", driver, "FL", "St Johns");
                        driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        //Multiparcel
                        try
                        {
                            Thread.Sleep(2000);
                            string Owner = "", Address = "", MultiParcelNumber = "";
                            int Mcount = 0;
                            IWebElement multy = driver.FindElement(By.Id("ctlBodyPane_ctl00_ctl01_gvwParcelResults"));
                            IList<IWebElement> muladdress = multy.FindElements(By.TagName("tr"));
                            IList<IWebElement> mulid;
                            foreach (IWebElement addressrow in muladdress)
                            {
                                mulid = addressrow.FindElements(By.TagName("td"));
                                if (mulid.Count != 0 && !addressrow.Text.Contains("ParcelID"))
                                {
                                    if (Mcount <= 25)
                                    {
                                        MultiParcelNumber = mulid[1].Text;
                                        Owner = mulid[2].Text;
                                        Address = mulid[3].Text;
                                        string MultyInst = Owner + "~" + Address;
                                        gc.insert_date(orderNumber, MultiParcelNumber, 740, MultyInst, 1, DateTime.Now);
                                    }
                                    Mcount++;
                                }
                            }

                            gc.CreatePdf_WOP(orderNumber, "Multiparcel Result", driver, "FL", "St Johns");
                            if (Mcount > 25)
                            {
                                HttpContext.Current.Session["multiParcel_StJohns_Maximum"] = "Maximum";
                                return "Maximum";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiparcel_StJohns"] = "Yes";
                            }
                            driver.Quit();
                            return "MultiParcel";
                        }
                        catch
                        {

                        }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("ctlBodyPane_noDataList_pnlNoResults")).Text;
                            if (nodata.Contains("No results match your search criteria."))
                            {
                                HttpContext.Current.Session["Nodata_FLStJohns"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("ctlBodyPane_ctl00_ctl01_txtName")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "OwnernamesearchBefore", driver, "FL", "St Johns");
                        driver.FindElement(By.Id("ctlBodyPane_ctl00_ctl01_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(4000);
                        //MULTIPARCEL 
                        try
                        {
                            Thread.Sleep(2000);
                            string Owner = "", Address = "", MultiParcelNumber = "";
                            int Mcount = 0;
                            IWebElement multipar = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_ctl01_gvwParcelResults']/tbody"));
                            IList<IWebElement> multiowner = multipar.FindElements(By.TagName("tr"));
                            IList<IWebElement> mulid;
                            foreach (IWebElement addressrow in multiowner)
                            {
                                mulid = addressrow.FindElements(By.TagName("td"));
                                if (mulid.Count != 0)
                                {
                                    if (Mcount <= 25)
                                    {
                                        MultiParcelNumber = mulid[1].Text;
                                        Owner = mulid[2].Text;
                                        Address = mulid[3].Text;
                                        string MultyInst = Owner + "~" + Address;
                                        gc.insert_date(orderNumber, MultiParcelNumber, 740, MultyInst, 1, DateTime.Now);
                                    }
                                    Mcount++;
                                }
                            }

                            gc.CreatePdf_WOP(orderNumber, "Multiparcel Result", driver, "FL", "St Johns");
                            if (Mcount > 25)
                            {
                                HttpContext.Current.Session["multiParcel_StJohns"] = "Maximum";
                                return "Maximum";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiparcel_StJohns"] = "Yes";
                            }
                            driver.Quit();
                            return "MultiParcel";
                        }
                        catch
                        {

                        }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("ctlBodyPane_noDataList_pnlNoResults")).Text;
                            if (nodata.Contains("No results match your search criteria."))
                            {
                                HttpContext.Current.Session["Nodata_FLStJohns"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {
                        driver.FindElement(By.Id("ctlBodyPane_ctl02_ctl01_txtParcelID")).SendKeys(parcelNumber);
                        gc.CreatePdf_WOP(orderNumber, "ParcelsearchBefore", driver, "FL", "St Johns");
                        driver.FindElement(By.Id("ctlBodyPane_ctl02_ctl01_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("ctlBodyPane_noDataList_pnlNoResults")).Text;
                            if (nodata.Contains("No results match your search criteria."))
                            {
                                HttpContext.Current.Session["Nodata_FLStJohns"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    //Property Details
                    string ParcelID = "", OwnerName = "", PropertyAddress = "", MailingAddress = "", PropertyType = "", District = "", MillageRate = "", Acreage = "", Home1 = "", Homestead = "", YearBuilt = "", LegalDescription = "", Legal1 = "";

                    string bulktextdata = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl03_mSection']/div/table")).Text.Trim();
                    ParcelID = gc.Between(bulktextdata, "Parcel ID", "Location Address").Trim();
                    PropertyAddress = gc.Between(bulktextdata, "Location Address", "Neighborhood").Trim();
                    Legal1 = gc.Between(bulktextdata, "Tax Description*", "Property Use Code").Trim();
                    LegalDescription = GlobalClass.Before(Legal1, "*The Description").Replace("\r\n", " ").Trim();
                    PropertyType = gc.Between(bulktextdata, "Property Use Code", "Subdivision").Trim();
                    District = gc.Between(bulktextdata, "District", "Millage Rate").Trim();
                    MillageRate = gc.Between(bulktextdata, "Millage Rate", "Acreage").Trim();
                    Acreage = gc.Between(bulktextdata, "Acreage", "Homestead").Trim();
                    Home1 = GlobalClass.After(bulktextdata, "Homestead").Trim();
                    Homestead = GlobalClass.Before(Home1, "\r\n\r\nView Map").Trim();

                    string bulktextdata1 = driver.FindElement(By.Id("ctlBodyPane_ctl04_mSection")).Text.Trim();
                    OwnerName = gc.Between(bulktextdata1, "Owner Name", "Mailing Address").Trim();
                    MailingAddress = GlobalClass.After(bulktextdata1, "Mailing Address").Replace("\r\n", " ").Trim();
                    try
                    {
                        IWebElement tbyearbuilt = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl08_mSection']/div/div[1]/div[1]/table"));
                        IList<IWebElement> TRyearbuilt = tbyearbuilt.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDyearbuilt;
                        foreach (IWebElement row in TRyearbuilt)
                        {
                            TDyearbuilt = row.FindElements(By.TagName("td"));
                            if (TDyearbuilt.Count != 0 && row.Text.Contains("Actual Year Built"))
                            {
                                YearBuilt = TDyearbuilt[1].Text;
                                break;
                            }
                        }
                    }
                    catch { }

                    string propertydetails = OwnerName + "~" + PropertyAddress + "~" + MailingAddress + "~" + PropertyType + "~" + District + "~" + MillageRate + "~" + Acreage + "~" + Homestead + "~" + YearBuilt + "~" + LegalDescription;
                    gc.insert_date(orderNumber, ParcelID, 669, propertydetails, 1, DateTime.Now);


                    //Current Year Assessment Details
                    string Year1 = "";
                    Year1 = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl05_ctl01_grdValuation']/thead/tr")).Text.Trim();
                    gc.CreatePdf(orderNumber, ParcelID, "Assessment Details", driver, "FL", "St Johns");
                    try
                    {
                        string AssessmentTitle = "", AssessmentValue = "";
                        IWebElement tbcurasses12 = driver.FindElement(By.Id("ctlBodyPane_ctl05_ctl01_grdValuation"));
                        IList<IWebElement> TRcurasses2 = tbcurasses12.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDmulti12;
                        foreach (IWebElement row in TRcurasses2)
                        {
                            TDmulti12 = row.FindElements(By.TagName("td"));
                            if (TDmulti12.Count != 0 && !row.Text.Contains("Year"))
                            {
                                AssessmentTitle += TDmulti12[0].Text + "~";
                                AssessmentValue += TDmulti12[1].Text + "~";
                            }
                        }
                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + "Year" + "~" + AssessmentTitle.Remove(AssessmentTitle.Length - 1, 1) + "' where Id = '" + 703 + "'");

                        gc.insert_date(orderNumber, ParcelID, 703, Year1 + "~" + AssessmentValue.Remove(AssessmentValue.Length - 1, 1), 1, DateTime.Now);
                    }
                    catch { }
                    //Assessment Details               
                    try
                    {
                        string AssessmentYear = "", BuildingValue = "", ExtraFeaturesValue = "", TotalLandValue = "", AgMarketValue = "", AgAssessedValue = "", JustMarketValue = "", AssessedValue = "", TotalExemptions = "", TaxableValue = "";
                        IWebElement tbmulti12 = driver.FindElement(By.Id("ctlBodyPane_ctl06_ctl01_grdHistory"));
                        IList<IWebElement> TRmulti12 = tbmulti12.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDmulti12;
                        foreach (IWebElement row in TRmulti12)
                        {
                            TDmulti12 = row.FindElements(By.TagName("td"));
                            if (TDmulti12.Count != 0 && !row.Text.Contains("Year"))
                            {
                                AssessmentYear = TDmulti12[0].Text;
                                BuildingValue = TDmulti12[1].Text;
                                ExtraFeaturesValue = TDmulti12[2].Text;
                                TotalLandValue = TDmulti12[3].Text;
                                AgMarketValue = TDmulti12[4].Text;
                                AgAssessedValue = TDmulti12[5].Text;
                                JustMarketValue = TDmulti12[6].Text;
                                AssessedValue = TDmulti12[7].Text;
                                TotalExemptions = TDmulti12[8].Text;
                                TaxableValue = TDmulti12[9].Text;

                                string AssessmentDetails = AssessmentYear.Trim() + "~" + BuildingValue.Trim() + "~" + ExtraFeaturesValue.Trim() + "~" + TotalLandValue.Trim() + "~" + AgMarketValue.Trim() + "~" + AgAssessedValue.Trim() + "~" + JustMarketValue.Trim() + "~" + AssessedValue.Trim() + "~" + TotalExemptions.Trim() + "~" + TaxableValue.Trim();
                                gc.insert_date(orderNumber, ParcelID, 714, AssessmentDetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    //======Tax Information Details========
                    driver.Navigate().GoToUrl("http://stjohnstaxcollector.governmax.com/collectmax/homepage.asp?sid=5FAB060742144FB697559EEB012FC40A&agencyid=stjohnstaxcollector");
                    Thread.Sleep(2000);
                    IWebElement url = driver.FindElement(By.LinkText("Account or Parcel Number"));
                    string Tag = url.GetAttribute("href");
                    url.Click();

                    string TaxParcel = ParcelID.Substring(0, 6) + "-" + ParcelID.Substring(6, 4);
                    driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[1]/td/table/tbody/tr/td/table/tbody/tr[2]/td/font/input")).SendKeys(TaxParcel);
                    gc.CreatePdf(orderNumber, ParcelID, "Enter the Account Number Before", driver, "FL", "St Johns");
                    driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[4]/td/input")).SendKeys(Keys.Enter);
                    Thread.Sleep(30000);
                    gc.CreatePdf(orderNumber, ParcelID, "Enter the Account Number After", driver, "FL", "St Johns");
                    string AccountNumber = "", TaxType = "", TaxYear = "", TaxOwnerName = "", TaxPropertyAddress = "", ExemptionAmount = "", TaxableValue1 = "", ExemptionTypeAndAmount = "",
                       Exemption = "", ExemptionTypeFirst = "", ExemptionTypeSecond = "", ExemptionAmountFirst = "", ExemptionAmountSecond = "", MillageCode = "", PaidDate = "", TransactionType = "", ReceiptNumber = "", Item = "", PaidAmount = "", TaxingAuthority = "";

                    try
                    {
                        IWebElement tbmulti13 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody"));
                        IList<IWebElement> TRowmulti13 = tbmulti13.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDmulti13;
                        foreach (IWebElement row in TRowmulti13)
                        {
                            TDmulti13 = row.FindElements(By.TagName("td"));
                            if (TDmulti13.Count != 0 && !row.Text.Contains("Account or Parcel Number") && !row.Text.Contains("Mailing Address"))
                            {
                                AccountNumber = TDmulti13[0].Text;
                                TaxType = TDmulti13[1].Text;
                                TaxYear = TDmulti13[2].Text;
                                break;
                            }
                        }
                    }
                    catch { }
                    // TaxOwnerName = driver.FindElement(By.Id("ctlBodyPane_ctl04_mSection")).Text.Trim();
                    // string bulktext1 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[3]/td/table/tbody")).Text.Trim();
                    //  TaxPropertyAddress = GlobalClass.After(bulktext1, "Physical Address").Replace("\r\n", " ").Trim();
                    Thread.Sleep(5000);

                    try
                    {
                        IWebElement tbmulti14 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody"));
                        IList<IWebElement> TRowmulti14 = tbmulti14.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDmulti14;
                        foreach (IWebElement row in TRowmulti14)
                        {
                            TDmulti14 = row.FindElements(By.TagName("td"));
                            if (TDmulti14.Count != 0 && (Exemption == "Yes"))
                            {
                                ExemptionAmount = TDmulti14[0].Text;
                                TaxableValue1 = TDmulti14[1].Text;
                                break;
                            }
                            if (TDmulti14.Count != 0 && TDmulti14.Count == 2 && !(Exemption == "Yes") && (row.Text.Contains("Mailing Address")))
                            {
                                string strOwnerName = GlobalClass.After(TDmulti14[0].Text, "Mailing Address   \r\n");
                                strOwnerName = strOwnerName.Replace("\r\n", "~");
                                string[] Owner = strOwnerName.Split('~');
                                TaxOwnerName = Owner[0];
                                TaxPropertyAddress = GlobalClass.After(TDmulti14[1].Text, "Physical Address   \r\n");
                            }
                            if (row.Text.Contains("Exempt Amount") && row.Text.Contains("Taxable Value "))
                            {
                                Exemption = "Yes";
                            }
                        }
                    }
                    catch { }

                    try
                    {
                        IWebElement tbmulti15 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[6]/td/table/tbody/tr/td/table/tbody/tr[2]/td[1]/table/tbody"));
                        IList<IWebElement> TRowmulti15 = tbmulti15.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDmulti15;
                        foreach (IWebElement row in TRowmulti15)
                        {
                            TDmulti15 = row.FindElements(By.TagName("td"));
                            if (TDmulti15.Count != 0)
                            {
                                ExemptionTypeFirst += TDmulti15[0].Text + ",";
                                ExemptionAmountFirst += TDmulti15[1].Text + ",";
                            }
                        }
                        ExemptionTypeFirst = ExemptionTypeFirst.TrimEnd(',');
                        ExemptionAmountFirst = ExemptionAmountFirst.TrimEnd(',');
                    }
                    catch
                    {

                    }
                    ExemptionTypeAndAmount = ExemptionTypeFirst + "-" + ExemptionAmountFirst;

                    try
                    {
                        IWebElement tbmillage1 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[6]/td/table/tbody/tr/td/table/tbody"));
                        IList<IWebElement> TRowmillage1 = tbmillage1.FindElements(By.TagName("tr"));

                        IList<IWebElement> TDRowmillage1;
                        foreach (IWebElement row in TRowmillage1)
                        {
                            TDRowmillage1 = row.FindElements(By.TagName("td"));
                            if (!row.Text.Contains("Exemption Detail"))
                            {
                                if (row.Text.Contains("NO EXEMPTIONS"))
                                {
                                    ExemptionTypeAndAmount = TDRowmillage1[0].Text;
                                    MillageCode = TDRowmillage1[1].Text;
                                }
                            }

                            if (TDRowmillage1.Count == 7 && !row.Text.Contains("Exemption"))
                            {
                                MillageCode = TDRowmillage1[5].Text;
                            }
                            if (TDRowmillage1.Count == 5 && !row.Text.Contains("Exemption"))
                            {
                                MillageCode = TDRowmillage1[3].Text;
                            }

                        }
                    }

                    catch
                    {

                    }
                    try
                    {
                        IWebElement tbmulti16 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[5]/tbody"));
                        if (!tbmulti16.Text.Contains("Prior Years Due ") && !tbmulti16.Text.Contains("Prior Year Taxes Due "))
                        {
                            IList<IWebElement> TRowmulti16 = tbmulti16.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDmulti16;
                            foreach (IWebElement row in TRowmulti16)
                            {
                                TDmulti16 = row.FindElements(By.TagName("td"));
                                if (TDmulti16.Count != 0 && !row.Text.Contains("Date Paid") && !row.Text.Contains("If Paid By ") && !row.Text.Contains("Prior Years Due "))
                                {
                                    PaidDate = TDmulti16[0].Text;
                                    TransactionType = TDmulti16[1].Text;
                                    ReceiptNumber = TDmulti16[2].Text;
                                    Item = TDmulti16[3].Text;
                                    PaidAmount = TDmulti16[4].Text;
                                    break;
                                }
                            }
                        }
                    }
                    catch { }
                    string ifpaidby = "", amountdue = "";
                    try
                    {
                        IWebElement tbifpaid = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[4]/tbody/tr/td/table/tbody"));
                        if (!tbifpaid.Text.Contains("Year "))
                        {
                            IList<IWebElement> TRowifpaid = tbifpaid.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDifpaid;
                            foreach (IWebElement row in TRowifpaid)
                            {
                                TDifpaid = row.FindElements(By.TagName("td"));
                                if (TDifpaid.Count != 0 && !row.Text.Contains("If Paid By ") && !row.Text.Contains("Amount Due "))
                                {
                                    ifpaidby = TDifpaid[0].Text;
                                    amountdue = TDifpaid[1].Text;
                                    break;
                                }
                            }
                        }
                    }
                    catch { }

                    string bulktext = driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td[2]")).Text.Trim();
                    TaxingAuthority = GlobalClass.After(bulktext, "Hollingsworth").Replace("\r\n", " ").Trim();
                    string TaxInformationDetails = AccountNumber.Trim() + "~" + TaxType.Trim() + "~" + TaxYear.Trim() + "~" + TaxOwnerName.Trim() + "~" + TaxPropertyAddress.Trim() + "~" + ExemptionAmount.Trim() + "~" + TaxableValue1.Trim() + "~" + ExemptionTypeAndAmount.Trim() + "~" + MillageCode.Trim() + "~" + PaidDate.Trim() + "~" + TransactionType.Trim() + "~" + ReceiptNumber.Trim() + "~" + Item.Trim() + "~" + PaidAmount.Trim() + "~" + TaxingAuthority.Trim() + "~" + ifpaidby.Trim() + "~" + amountdue.Trim();
                    gc.insert_date(orderNumber, ParcelID, 777, TaxInformationDetails, 1, DateTime.Now);

                    //Tax Distribution Table:
                    string TAuthority = "", TRate = "", TDistributionAssessedValue = "", TDistributionExemptionAmount = "", TDistributionTaxableValue = "", TDistributionLevied = "";
                    try
                    {
                        IWebElement tbmulti17 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[8]/td/table[1]"));
                        IList<IWebElement> TRowmulti17 = tbmulti17.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDmulti17;
                        foreach (IWebElement row in TRowmulti17)
                        {
                            TDmulti17 = row.FindElements(By.TagName("td"));
                            if (TDmulti17.Count != 0 && TDmulti17.Count == 6 && !row.Text.Contains("Taxing Authority"))
                            {
                                TAuthority = TDmulti17[0].Text;
                                TRate = TDmulti17[1].Text;
                                TDistributionAssessedValue = TDmulti17[2].Text;
                                TDistributionExemptionAmount = TDmulti17[3].Text;
                                TDistributionTaxableValue = TDmulti17[4].Text;
                                TDistributionLevied = TDmulti17[5].Text;

                                string TaxDistributionTable1 = TAuthority.Trim() + "~" + "" + "~" + "Ad Valorem Taxes" + "~" + TRate.Trim() + "~" + TDistributionAssessedValue.Trim() + "~" + TDistributionExemptionAmount.Trim() + "~" + TDistributionTaxableValue.Trim() + "~" + TDistributionLevied.Trim();
                                gc.insert_date(orderNumber, ParcelID, 729, TaxDistributionTable1, 1, DateTime.Now);
                            }
                            if (TDmulti17.Count == 2)
                            {
                                TAuthority = ""; TRate = ""; TDistributionAssessedValue = ""; TDistributionExemptionAmount = ""; TDistributionTaxableValue = ""; TDistributionLevied = "";
                                TAuthority = TDmulti17[0].Text;
                                TRate = TDmulti17[1].Text;

                                string TaxDistributionDetails = TAuthority.Trim() + "~" + "" + "~" + "Ad Valorem Taxes" + "~" + TRate.Trim() + "~" + TDistributionAssessedValue.Trim() + "~" + TDistributionExemptionAmount.Trim() + "~" + TDistributionTaxableValue.Trim() + "~" + TDistributionLevied.Trim();
                                gc.insert_date(orderNumber, ParcelID, 729, TaxDistributionDetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }

                    string TotalMillage = "", TotalTaxes = "";
                    try
                    {
                        IWebElement tbmulti18 = driver.FindElement(By.XPath("html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[8]/td/table[2]/tbody"));
                        IList<IWebElement> TRowmulti18 = tbmulti18.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDmulti18;
                        foreach (IWebElement row in TRowmulti18)
                        {
                            TDmulti18 = row.FindElements(By.TagName("td"));
                            if (TDmulti18.Count != 0)
                            {
                                TotalMillage = TDmulti18[1].Text;
                                TotalTaxes = TDmulti18[3].Text;

                                string TaxDistributionTable1 = "TOTAL" + "~" + "" + "~" + "Ad Valorem Taxes" + "~" + TotalMillage.Trim() + "~" + "" + "~" + "" + "~" + "" + "~" + TotalTaxes.Trim();
                                gc.insert_date(orderNumber, ParcelID, 729, TaxDistributionTable1, 1, DateTime.Now);
                                break;
                            }
                        }
                    }
                    catch { }

                    string Code = "", LevyingAuthority = "", Amount = "";
                    try
                    {
                        IWebElement tbmulti19 = driver.FindElement(By.XPath("html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[10]/td/table[1]/tbody"));
                        IList<IWebElement> TRowmulti19 = tbmulti19.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDmulti19;
                        foreach (IWebElement row in TRowmulti19)
                        {
                            TDmulti19 = row.FindElements(By.TagName("td"));
                            if (TDmulti19.Count != 0 && TDmulti19.Count == 3 && !row.Text.Contains("Code"))
                            {
                                Code = TDmulti19[0].Text;
                                LevyingAuthority = TDmulti19[1].Text;
                                Amount = TDmulti19[2].Text;
                                string TaxDistributionTable3 = LevyingAuthority.Trim() + "~" + Code.Trim() + "~" + "Non-Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + Amount.Trim();
                                gc.insert_date(orderNumber, ParcelID, 729, TaxDistributionTable3, 1, DateTime.Now);
                            }
                        }
                    }
                    catch
                    {

                    }

                    string TotalAssessments = "";
                    try
                    {
                        IWebElement tbmulti20 = driver.FindElement(By.XPath("html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[10]/td/table[2]"));
                        IList<IWebElement> TRowmulti20 = tbmulti20.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDmulti20;
                        foreach (IWebElement row in TRowmulti20)
                        {
                            TDmulti20 = row.FindElements(By.TagName("td"));
                            if (TDmulti20.Count != 0)
                            {
                                TotalAssessments = TDmulti20[1].Text;
                                string TaxDistributionTable3 = "Total Assessments" + "~" + "" + "~" + "Non-Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + TotalAssessments.Trim();
                                gc.insert_date(orderNumber, ParcelID, 729, TaxDistributionTable3, 1, DateTime.Now);
                                break;
                            }
                        }
                    }
                    catch { }
                    string TaxesAndAssessments = "";
                    try
                    {
                        IWebElement tbmulti21 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[11]/td/table"));
                        IList<IWebElement> TRowmulti21 = tbmulti21.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDmulti21;
                        foreach (IWebElement row in TRowmulti21)
                        {
                            TDmulti21 = row.FindElements(By.TagName("td"));
                            if (TDmulti21.Count != 0)
                            {
                                TaxesAndAssessments = TDmulti21[3].Text;
                                string TaxDistributionTable4 = "Taxes And Assessments" + "~" + "" + "~" + "Non-Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + TaxesAndAssessments.Trim();
                                gc.insert_date(orderNumber, ParcelID, 729, TaxDistributionTable4, 1, DateTime.Now);
                                break;
                            }
                        }
                    }
                    catch { }
                    //Delinquent Details
                    string strTaxYear = "", strTaxFolio = "", strStatus = "", strTaxCertificateYear = "", strTaxCertificateNo = "", strTaxAmount = "";
                    try
                    {

                        IWebElement tbmulti23 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[4]/tbody/tr[2]/td/table[1]"));
                        IList<IWebElement> TRowmulti23 = tbmulti23.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDmulti23;
                        foreach (IWebElement row in TRowmulti23)
                        {
                            TDmulti23 = row.FindElements(By.TagName("td"));
                            if (TDmulti23.Count != 0 && !row.Text.Contains("Year ") && TDmulti23.Count == 7)
                            {
                                strTaxYear = TDmulti23[0].Text;
                                strTaxFolio = TDmulti23[1].Text;
                                strStatus = TDmulti23[2].Text;
                                strTaxCertificateNo = TDmulti23[3].Text;
                                strTaxCertificateYear = TDmulti23[4].Text;
                                strTaxAmount = TDmulti23[5].Text;


                                string UnpaidDetails = strTaxYear + "~" + strTaxFolio + "~" + strStatus + "~" + strTaxCertificateNo + "~" + strTaxCertificateYear + "~" + strTaxAmount;
                                gc.insert_date(orderNumber, ParcelID, 742, UnpaidDetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }
                    string strPriorYearTotal = "";
                    try
                    {
                        IWebElement tbmulti24 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[4]/tbody/tr[2]/td/table[3]/tbody"));
                        IList<IWebElement> TRowmulti24 = tbmulti24.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDmulti24;
                        foreach (IWebElement row in TRowmulti24)
                        {
                            TDmulti24 = row.FindElements(By.TagName("td"));
                            if (TDmulti24.Count != 0 && TDmulti24.Count == 2)
                            {
                                strTaxYear = TDmulti24[0].Text;
                                strPriorYearTotal = TDmulti24[1].Text;

                                string UnpaidDetails1 = "Prior Years Total" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + strPriorYearTotal;
                                gc.insert_date(orderNumber, ParcelID, 742, UnpaidDetails1, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }
                    string strIfpaidBy = "", PriorYearsDue = "";
                    try
                    {
                        IWebElement tbmulti25 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[5]/tbody"));
                        if (!tbmulti25.Text.Contains("Prior Year Taxes Due ") && !tbmulti25.Text.Contains("    NO DELINQUENT TAXES "))
                        {
                            IList<IWebElement> TRowmulti25 = tbmulti25.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDmulti25;
                            foreach (IWebElement row in TRowmulti25)
                            {
                                TDmulti25 = row.FindElements(By.TagName("td"));
                                if (TDmulti25.Count != 0 && TDmulti25.Count == 2 && !row.Text.Contains("If Paid By "))
                                {
                                    strIfpaidBy = TDmulti25[0].Text;
                                    PriorYearsDue = TDmulti25[1].Text;

                                    string UnpaidDetails2 = "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + strIfpaidBy + "~" + PriorYearsDue;
                                    gc.insert_date(orderNumber, ParcelID, 742, UnpaidDetails2, 1, DateTime.Now);
                                }
                            }
                        }
                    }
                    catch { }

                    //Tax History Details
                    driver.FindElement(By.LinkText("   Payment History")).SendKeys(Keys.Enter);
                    Thread.Sleep(5000);
                    gc.CreatePdf(orderNumber, ParcelID, "PaymentHistoryView", driver, "FL", "St Johns");
                    int YearCount = 0;
                    try
                    {
                        IWebElement IYear = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody"));
                        IList<IWebElement> IYearRow = IYear.FindElements(By.TagName("tr"));
                        IList<IWebElement> IYearTd;
                        IList<IWebElement> IYearWise;
                        foreach (IWebElement year in IYearRow)
                        {
                            IYearTd = year.FindElements(By.TagName("table"));
                            if (IYearTd.Count != 0)
                            {
                                YearCount = IYearTd.Count;
                            }
                        }
                    }
                    catch { }


                    for (int i = 3; i <= YearCount; i++)
                    {
                        try
                        {
                            string Year = "", Folio = "", THPaidDate = "", Receipt = "", BilledAmount = "", THPaidAmount = "", THOwnerName = "", PaidBy = "", Status = "";
                            IWebElement tbmulti22 = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table[" + i + "]/tbody"));
                            IList<IWebElement> TRowmulti22 = tbmulti22.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDmulti22;
                            foreach (IWebElement row in TRowmulti22)
                            {
                                TDmulti22 = row.FindElements(By.TagName("td"));
                                if (TDmulti22.Count != 0 && TDmulti22.Count == 6 && !row.Text.Contains("Payment History") && !row.Text.Contains("Year"))
                                {
                                    Year = TDmulti22[0].Text;
                                    Folio = TDmulti22[1].Text;
                                    THPaidDate = TDmulti22[2].Text;
                                    Receipt = TDmulti22[3].Text;
                                    BilledAmount = TDmulti22[4].Text;
                                    THPaidAmount = TDmulti22[5].Text;
                                }
                                if (TDmulti22.Count == 2 && row.Text.Contains("Owner Name"))
                                {
                                    THOwnerName = TDmulti22[1].Text;
                                }
                                if (TDmulti22.Count == 2 && TDmulti22.Count < 6 && row.Text.Contains("Paid By") && (THOwnerName != "" && Year != ""))
                                {
                                    PaidBy = TDmulti22[1].Text;
                                    string TaxHistorytable1 = Year.Trim() + "~" + Folio.Trim() + "~" + THPaidDate.Trim() + "~" + Receipt.Trim() + "~" + BilledAmount.Trim() + "~" + THPaidAmount.Trim() + "~" + THOwnerName.Trim() + "~" + PaidBy.Trim();
                                    gc.insert_date(orderNumber, ParcelID, 736, TaxHistorytable1, 1, DateTime.Now);
                                }
                                else if (TDmulti22.Count == 2 && TRowmulti22.Count == 3 && !row.Text.Contains("Paid By"))
                                {
                                    string TaxHistorytable1 = Year.Trim() + "~" + Folio.Trim() + "~" + THPaidDate.Trim() + "~" + Receipt.Trim() + "~" + BilledAmount.Trim() + "~" + THPaidAmount.Trim() + "~" + THOwnerName.Trim() + "~" + PaidBy.Trim();
                                    gc.insert_date(orderNumber, ParcelID, 736, TaxHistorytable1, 1, DateTime.Now);
                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                    try
                    {
                        string taxurl = driver.FindElement(By.XPath("html/body/table[2]/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[8]/td/font/a")).GetAttribute("href");
                        driver.Navigate().GoToUrl(taxurl);
                        Thread.Sleep(5000);
                        gc.CreatePdf(orderNumber, ParcelID, "TaxBillPdf", driver, "FL", "St Johns");
                    }
                    catch { }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Quit();
                    gc.mergpdf(orderNumber, "FL", "St Johns");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "FL", "St Johns", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
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


