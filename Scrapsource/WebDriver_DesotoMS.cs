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
    public class WebDriver_DesotoMS
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_MSDesoto(string streetNo, string direction, string streetName, string streetType, string unitNumber, string parcelNumber, string ownerName, string searchType, string orderNumber)
        {
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new PhantomJSDriver();
            //driver = new ChromeDriver();
            using (driver = new PhantomJSDriver()) //ChromeDriver
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("http://cs.datasysmgt.com/tax?state=MS&county=17");
                    Thread.Sleep(2000);
                    if (searchType == "titleflex")
                    {
                        string Address = "";
                        if (direction != "")
                        {
                            Address = streetNo + " " + direction + " " + streetName + " " + streetType + " " + unitNumber;
                        }
                        else { Address = streetNo + " " + streetName + " " + streetType + " " + unitNumber; }
                        gc.TitleFlexSearch(orderNumber, "", "", Address.Trim(), "MS", "DeSoto");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["DesotoMS_NoRecord"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        driver.FindElement(By.Id("taxweb_grid-search-field-street-number-field")).SendKeys(streetNo);
                        string Add1 = streetName + " " + streetType;
                        driver.FindElement(By.Id("taxweb_grid-search-field-street-name-field")).SendKeys(Add1.Trim());
                        //gc.CreatePdf_WOP(orderNumber, "Address search", driver, "MS", "DeSoto");
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "MS", "DeSoto");
                        driver.FindElement(By.Id("ext-gen9")).SendKeys(Keys.Enter);
                        Thread.Sleep(9000);
                        gc.CreatePdf_WOP(orderNumber, "Address search1", driver, "MS", "DeSoto");
                        //gc.CreatePdf_WOP(orderNumber, "Address search1", driver, "MS", "DeSoto");
                        try
                        {
                            IWebElement INorecord = driver.FindElement(By.Id("ext-gen18"));
                            if (INorecord.Text.Contains("no records"))
                            {
                                HttpContext.Current.Session["DesotoMS_NoRecord"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                        // gc.CreatePdf_WOP(orderNumber, "Address search Result", driver, "MS", "DeSoto");
                        try
                        {
                            string multi = GlobalClass.After(driver.FindElement(By.Id("ext-comp-1008")).Text, "of").Trim();
                            string strAddress = "", strparcel = "", strOwner = "";
                            if (Convert.ToInt32(multi) <= 25)
                            {
                                IWebElement tbmulti = driver.FindElement(By.Id("ext-gen16"));
                                IList<IWebElement> TBmulti = tbmulti.FindElements(By.TagName("table"));
                                IList<IWebElement> TRmulti;
                                IList<IWebElement> TDmulti;
                                foreach (IWebElement row in TBmulti)
                                {
                                    TDmulti = row.FindElements(By.TagName("td"));
                                    if (TDmulti.Count != 0 && TDmulti[0].Text.Trim() != "")
                                    {
                                        strAddress = TDmulti[3].Text;
                                        strparcel = TDmulti[1].Text;
                                        strOwner = TDmulti[0].Text;

                                        string multiDetails = strAddress + "~" + strOwner;
                                        gc.insert_date(orderNumber, strparcel, 1351, multiDetails, 1, DateTime.Now);
                                    }
                                }
                                gc.CreatePdf_WOP(orderNumber, "Multi Address search Result", driver, "MS", "DeSoto");
                                HttpContext.Current.Session["multiparcel_DesotoMS"] = "Yes";
                                driver.Quit();
                                gc.mergpdf(orderNumber, "MS", "DeSoto");
                                return "MultiParcel";
                            }

                            if (Convert.ToInt32(multi) > 25)
                            {
                                HttpContext.Current.Session["multiparcel_DesotoMS_Multicount"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "parcel")
                    {
                        parcelNumber = parcelNumber.Replace(" ", "").Trim();
                        string Town = parcelNumber.Substring(0, 1);
                        string Rin = parcelNumber.Substring(1, 2);
                        string Area = parcelNumber.Substring(3, 1);
                        string Sct = parcelNumber.Substring(4, 2);
                        string Sub = parcelNumber.Substring(6, 2);
                        string Qtr = parcelNumber.Substring(8, 1);
                        string Lot = parcelNumber.Substring(9, 5);
                        string Split = parcelNumber.Substring(14, 2);
                        //gc.CreatePdf_WOP(orderNumber, "Parcel Before search Result1", driver, "MS", "DeSoto");
                        IWebElement element1 = driver.FindElement(By.Id("taxweb_grid-search-field-parcel-field1"));
                        element1.Clear();
                        driver.FindElement(By.Id("taxweb_grid-search-field-parcel-field1")).Click();
                        driver.FindElement(By.Id("taxweb_grid-search-field-parcel-field1")).SendKeys(Town);

                        IWebElement element2 = driver.FindElement(By.Id("taxweb_grid-search-field-parcel-field2"));
                        element2.Clear();
                        driver.FindElement(By.Id("taxweb_grid-search-field-parcel-field2")).Click();
                        driver.FindElement(By.Id("taxweb_grid-search-field-parcel-field2")).SendKeys(Rin);

                        IWebElement element3 = driver.FindElement(By.Id("taxweb_grid-search-field-parcel-field3"));
                        element3.Clear();
                        driver.FindElement(By.Id("taxweb_grid-search-field-parcel-field3")).Click();
                        driver.FindElement(By.Id("taxweb_grid-search-field-parcel-field3")).SendKeys(Area);

                        IWebElement element4 = driver.FindElement(By.Id("taxweb_grid-search-field-parcel-field4"));
                        element4.Clear();
                        driver.FindElement(By.Id("taxweb_grid-search-field-parcel-field4")).Click();
                        driver.FindElement(By.Id("taxweb_grid-search-field-parcel-field4")).SendKeys(Sct);

                        IWebElement element5 = driver.FindElement(By.Id("taxweb_grid-search-field-parcel-field5"));
                        element5.Clear();
                        driver.FindElement(By.Id("taxweb_grid-search-field-parcel-field5")).Click();
                        driver.FindElement(By.Id("taxweb_grid-search-field-parcel-field5")).SendKeys(Sub);

                        IWebElement element6 = driver.FindElement(By.Id("taxweb_grid-search-field-parcel-field6"));
                        element6.Clear();
                        driver.FindElement(By.Id("taxweb_grid-search-field-parcel-field6")).Click();
                        driver.FindElement(By.Id("taxweb_grid-search-field-parcel-field6")).SendKeys(Qtr);

                        IWebElement element7 = driver.FindElement(By.Id("taxweb_grid-search-field-parcel-field7"));
                        element7.Clear();
                        driver.FindElement(By.Id("taxweb_grid-search-field-parcel-field7")).Click();
                        driver.FindElement(By.Id("taxweb_grid-search-field-parcel-field7")).SendKeys(Lot);

                        IWebElement element8 = driver.FindElement(By.Id("taxweb_grid-search-field-parcel-field8"));
                        element8.Clear();
                        driver.FindElement(By.Id("taxweb_grid-search-field-parcel-field8")).Click();
                        driver.FindElement(By.Id("taxweb_grid-search-field-parcel-field8")).SendKeys(Split);

                        gc.CreatePdf_WOP(orderNumber, "Parcel search After Result1", driver, "MS", "DeSoto");
                        //driver.FindElement(By.XPath("//*[@id='ext-gen9']")).SendKeys(Keys.Enter);
                        // Thread.Sleep(9000);
                        try
                        {
                            IWebElement IAddressSearch1 = driver.FindElement(By.XPath("//*[@id='ext-gen9']"));
                            IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                            js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                            Thread.Sleep(9000);
                        }
                        catch { }
                        try
                        {
                            IWebElement INorecord = driver.FindElement(By.Id("ext-gen18"));
                            if (INorecord.Text.Contains("no records"))
                            {
                                HttpContext.Current.Session["DesotoMS_NoRecord"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("taxweb_grid-search-field-last-name-field")).SendKeys(ownerName);
                        gc.CreatePdf_WOP(orderNumber, "Owner search1", driver, "MS", "DeSoto");
                        driver.FindElement(By.Id("ext-gen9")).SendKeys(Keys.Enter);
                        Thread.Sleep(9000);
                        gc.CreatePdf_WOP(orderNumber, "Owner search Result1", driver, "MS", "DeSoto");
                        try
                        {
                            IWebElement INorecord = driver.FindElement(By.Id("ext-gen18"));
                            if (INorecord.Text.Contains("no records"))
                            {
                                HttpContext.Current.Session["DesotoMS_NoRecord"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                        try
                        {
                            string multi = GlobalClass.After(driver.FindElement(By.Id("ext-comp-1008")).Text, "of").Trim();
                            string strAddress = "", strparcel = "", strOwner = "";
                            if (Convert.ToInt32(multi) <= 25)
                            {
                                IWebElement tbmulti = driver.FindElement(By.Id("ext-gen16"));
                                IList<IWebElement> TBmulti = tbmulti.FindElements(By.TagName("table"));
                                IList<IWebElement> TRmulti;
                                IList<IWebElement> TDmulti;
                                foreach (IWebElement row in TBmulti)
                                {
                                    TDmulti = row.FindElements(By.TagName("td"));
                                    if (TDmulti.Count != 0 && TDmulti[0].Text.Trim() != "")
                                    {
                                        strAddress = TDmulti[3].Text;
                                        strparcel = TDmulti[1].Text;
                                        strOwner = TDmulti[0].Text;

                                        string multiDetails = strAddress + "~" + strOwner;
                                        gc.insert_date(orderNumber, strparcel, 1351, multiDetails, 1, DateTime.Now);
                                    }
                                }
                                gc.CreatePdf_WOP(orderNumber, "Multi Owner search Result", driver, "MS", "DeSoto");
                                HttpContext.Current.Session["multiparcel_DesotoMS"] = "Yes";
                                driver.Quit();
                                gc.mergpdf(orderNumber, "MS", "DeSoto");
                                return "MultiParcel";
                            }

                            if (Convert.ToInt32(multi) > 25)
                            {
                                HttpContext.Current.Session["multiparcel_DesotoMS_Multicount"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }
                    }

                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='ext-gen18']/div/table/tbody/tr/td[1]/div/a")).Click();
                        Thread.Sleep(2000);
                    }
                    catch { }

                    string strownerName = "", strPropertyAddress = "", County = "", TaxYear = "", Receipt = "", LegalDescription = "", YearBuilt = "";
                    IWebElement IProperty = driver.FindElement(By.XPath("//*[@id='ext-gen58']/table[1]/tbody"));
                    TaxYear = gc.Between(IProperty.Text, "Tax Year:", "County Name:").Trim();
                    County = gc.Between(IProperty.Text, "County Name:", "\r\nName:").Trim();
                    strPropertyAddress = gc.Between(IProperty.Text, "Address:", "City:").Trim();
                    IWebElement IParcel = driver.FindElement(By.XPath("//*[@id='ext-gen58']/table[2]"));
                    parcelNumber = gc.Between(IParcel.Text, "Twn", "Receipt #:").Replace("Rng", "").Replace("Area", "").Replace("Sct", "").Replace("Sub", "").Replace("Qtr", "").Replace("Lot#", "").Replace("Split", "").Trim();
                    Receipt = gc.Between(IParcel.Text, "Receipt #:", "Land Owner Name:").Trim();
                    strownerName = GlobalClass.After(IParcel.Text, "Land Owner Name:\r\n").Trim();
                    LegalDescription = driver.FindElement(By.XPath("//*[@id='ext-gen58']/table[3]/tbody/tr/td/p[1]")).Text;
                    try
                    {
                        YearBuilt = driver.FindElement(By.XPath("//*[@id='ext-gen58']/table[11]/tbody/tr[2]/td[4]")).Text;
                    }
                    catch { }
                    try
                    {
                        YearBuilt = driver.FindElement(By.XPath("//*[@id='ext-gen58']/table[13]/tbody/tr[2]/td[4]")).Text;
                    }
                    catch { }
                    string PropertyDetails = strPropertyAddress + "~" + strownerName + "~" + County + "~" + TaxYear + "~" + Receipt + "~" + LegalDescription + "~" + YearBuilt;
                    gc.insert_date(orderNumber, parcelNumber, 1355, PropertyDetails, 1, DateTime.Now);

                    string TaxDistrict = "", Assessment = "", Millage = "", GrossTax = "", HomeStead = "";
                    IWebElement IAssessment = driver.FindElement(By.XPath("//*[@id='ext-gen58']/table[4]/tbody/tr/td/table"));
                    IList<IWebElement> IAssessmentRow = IAssessment.FindElements(By.TagName("tr"));
                    IList<IWebElement> IAssessmentTD;
                    foreach (IWebElement Assess in IAssessmentRow)
                    {
                        IAssessmentTD = Assess.FindElements(By.TagName("td"));
                        if (IAssessmentTD.Count != 0 && !Assess.Text.Contains("District:") && (Assess.Text.Contains("True:") || Assess.Text.Contains("Assessed:")))
                        {
                            TaxDistrict += IAssessmentTD[0].Text + " ";
                            Assessment += IAssessmentTD[2].Text + "~" + IAssessmentTD[3].Text + "~" + IAssessmentTD[4].Text + "~";
                            if (Assess.Text.Contains("Assessed:"))
                            {
                                Millage = IAssessmentTD[5].Text;
                                GrossTax = IAssessmentTD[6].Text;
                            }
                        }
                        if (IAssessmentTD.Count != 0 && !Assess.Text.Contains("District:") && Assess.Text.Contains("Homestead Credit Amount:"))
                        {
                            HomeStead = IAssessmentTD[5].Text;
                        }
                    }

                    string AssessmentDetails = TaxDistrict.Trim() + "~" + Assessment.Trim() + Millage.Trim() + "~" + GrossTax.Trim() + "~" + HomeStead.Trim();
                    gc.insert_date(orderNumber, parcelNumber, 1356, AssessmentDetails, 1, DateTime.Now);
                    //Tax Information Details
                    string Entity = "", Mills = "", TaxAmount = "";
                    IWebElement ITaxEntity = driver.FindElement(By.XPath("//*[@id='ext-gen58']/table[5]/tbody/tr[1]/td[1]/table"));
                    IList<IWebElement> ITaxEntityRow = ITaxEntity.FindElements(By.TagName("tr"));
                    IList<IWebElement> ITaxEntityTD;
                    foreach (IWebElement TaxEntity in ITaxEntityRow)
                    {
                        ITaxEntityTD = TaxEntity.FindElements(By.TagName("td"));
                        if (ITaxEntityTD.Count != 0 && !TaxEntity.Text.Contains("Tax Entities:") && TaxEntity.Text.Trim() != "")
                        {
                            Entity = ITaxEntityTD[0].Text.Replace(":", "");
                            Mills = ITaxEntityTD[1].Text;
                            TaxAmount = ITaxEntityTD[3].Text;

                            string entityDetails = Entity + "~" + Mills + "~" + TaxAmount;
                            gc.insert_date(orderNumber, parcelNumber, 1357, entityDetails, 1, DateTime.Now);
                        }
                    }
                    string SpecialTaxes = "", SpecialAmount = "";
                    IWebElement ISpecialAssessment = driver.FindElement(By.XPath("//*[@id='ext-gen58']/table[5]/tbody/tr[1]/td[2]/table"));
                    IList<IWebElement> ISpecialAssessmentRow = ISpecialAssessment.FindElements(By.TagName("tr"));
                    IList<IWebElement> ISpecialAssessmentTD;
                    foreach (IWebElement SpecialAssessment in ISpecialAssessmentRow)
                    {
                        ISpecialAssessmentTD = SpecialAssessment.FindElements(By.TagName("td"));
                        if (ISpecialAssessmentTD.Count != 0 && SpecialAssessment.Text.Trim() != "" && !SpecialAssessment.Text.Contains("Tax Amount"))
                        {
                            SpecialTaxes = ISpecialAssessmentTD[0].Text.Replace(":", "");
                            SpecialAmount = ISpecialAssessmentTD[1].Text;

                            string specialDetails = SpecialTaxes + "~" + SpecialAmount;
                            gc.insert_date(orderNumber, parcelNumber, 1358, specialDetails, 1, DateTime.Now);
                        }
                    }

                    string TaxInterestHead = "", TaxInterestValue = "";
                    IWebElement ITaxInterest = driver.FindElement(By.XPath("//*[@id='ext-gen58']/table[5]/tbody/tr[2]/td/table"));
                    IList<IWebElement> ITaxInterestRow = ITaxInterest.FindElements(By.TagName("tr"));
                    IList<IWebElement> ITaxInterestTD;
                    foreach (IWebElement Interest in ITaxInterestRow)
                    {
                        ITaxInterestTD = Interest.FindElements(By.TagName("td"));
                        if (ITaxInterestTD.Count != 0)
                        {
                            TaxInterestHead += ITaxInterestTD[0].Text.Replace(":", "") + "~";
                            TaxInterestValue += ITaxInterestTD[1].Text + "~";
                        }
                    }

                    IWebElement ITaxDue = driver.FindElement(By.XPath("//*[@id='ext-gen58']/table[5]/tbody/tr[3]/td/table"));
                    IList<IWebElement> ITaxDueRow = ITaxDue.FindElements(By.TagName("tr"));
                    IList<IWebElement> ITaxDueTD;
                    foreach (IWebElement due in ITaxDueRow)
                    {
                        ITaxDueTD = due.FindElements(By.TagName("td"));
                        if (ITaxDueTD.Count != 0 && !due.Text.Contains("------------") && due.Text != "")
                        {
                            TaxInterestHead += ITaxDueTD[1].Text.Replace(":", "") + "~";
                            TaxInterestValue += ITaxDueTD[2].Text + "~";
                        }
                    }
                    IWebElement ITotalAmount = driver.FindElement(By.XPath("//*[@id='ext-gen58']/table[6]"));
                    string TotalAmount = GlobalClass.After(ITotalAmount.Text, "Total Amount:").Replace("CITY TAXES INCLUDED", "").Trim();
                    string delinquentdate = "";
                    try
                    {
                        IWebElement Idelinquentdate = driver.FindElement(By.XPath("//*[@id='ext-gen58']/table[9]/tbody/tr/td[1]"));
                        delinquentdate = GlobalClass.After(Idelinquentdate.Text, "delinquent on").Trim();
                    }
                    catch { }
                    try
                    {
                        IWebElement Idelinquentdate = driver.FindElement(By.XPath("//*[@id='ext-gen58']/table[11]/tbody/tr/td[1]/p"));
                        delinquentdate = GlobalClass.After(Idelinquentdate.Text, "delinquent on").Trim();
                    }
                    catch { }
                    string taxesdue = "";
                    try
                    {
                        IWebElement taxddue = driver.FindElement(By.XPath("//*[@id='ext-gen58']/table[8]/tbody/tr/td[1]"));
                        if (taxddue.Text.Contains("TAXES DUE:"))
                        {
                            taxesdue = driver.FindElement(By.XPath("//*[@id='ext-gen58']/table[8]/tbody/tr/td[2]")).Text;
                        }
                    }
                    catch { }
                    db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + TaxInterestHead + "Total Amount~Delinquent Date~Taxes Due" + "' where Id = '" + 1359 + "'");
                    gc.insert_date(orderNumber, parcelNumber, 1359, TaxInterestValue + TotalAmount + "~" + delinquentdate + "~" + taxesdue, 1, DateTime.Now);

                    //Tax Payments Details
                    try
                    {
                        string Paymentdate = "", Taxes = "", Special = "", Interestpay = "", Fees = "", Total = "", Paidby = "";
                        IWebElement ITotalDue = driver.FindElement(By.XPath("//*[@id='ext-gen58']/table[8]"));
                        string TotalDue = ITotalDue.Text.Replace("TAXES DUE:", "").Trim();
                        string[] totalduesplit = TotalDue.Split('\r');
                        Paymentdate = totalduesplit[0];
                        Taxes = totalduesplit[1];
                        Special = totalduesplit[2];
                        Interestpay = totalduesplit[3];
                        Fees = totalduesplit[4];
                        Total = totalduesplit[5];
                        Paidby = totalduesplit[6];
                        string TaxpaymentDetails = Paymentdate + "~" + Taxes + "~" + Special + "~" + Interestpay + "~" + Fees + "~" + Total + "~" + Paidby;
                        gc.insert_date(orderNumber, parcelNumber, 1587, TaxpaymentDetails, 1, DateTime.Now);

                    }
                    catch { }
                    try
                    {
                        var chDriver = new ChromeDriver();
                        chDriver.Navigate().GoToUrl(driver.Url);
                        Thread.Sleep(4000);
                        //gc.CreatePdf(orderNumber, parcelNumber, "Half pdf1", chDriver, "MS", "DeSoto");
                        //Parcel id work
                        parcelNumber = parcelNumber.Replace(" ", "").Trim();
                        string Town = parcelNumber.Substring(0, 1);
                        string Rin = parcelNumber.Substring(1, 2);
                        string Area = parcelNumber.Substring(3, 1);
                        string Sct = parcelNumber.Substring(4, 2);
                        string Sub = parcelNumber.Substring(6, 2);
                        string Qtr = parcelNumber.Substring(8, 1);
                        string Lot = parcelNumber.Substring(9, 5);
                        string Split = parcelNumber.Substring(14, 2);
                        IWebElement element1 = chDriver.FindElement(By.Id("taxweb_grid-search-field-parcel-field1"));
                        element1.Clear();
                        chDriver.FindElement(By.Id("taxweb_grid-search-field-parcel-field1")).Click();
                        chDriver.FindElement(By.Id("taxweb_grid-search-field-parcel-field1")).SendKeys(Town);

                        IWebElement element2 = chDriver.FindElement(By.Id("taxweb_grid-search-field-parcel-field2"));
                        element2.Clear();
                        chDriver.FindElement(By.Id("taxweb_grid-search-field-parcel-field2")).Click();
                        chDriver.FindElement(By.Id("taxweb_grid-search-field-parcel-field2")).SendKeys(Rin);

                        IWebElement element3 = chDriver.FindElement(By.Id("taxweb_grid-search-field-parcel-field3"));
                        element3.Clear();
                        chDriver.FindElement(By.Id("taxweb_grid-search-field-parcel-field3")).Click();
                        chDriver.FindElement(By.Id("taxweb_grid-search-field-parcel-field3")).SendKeys(Area);

                        IWebElement element4 = chDriver.FindElement(By.Id("taxweb_grid-search-field-parcel-field4"));
                        element4.Clear();
                        chDriver.FindElement(By.Id("taxweb_grid-search-field-parcel-field4")).Click();
                        chDriver.FindElement(By.Id("taxweb_grid-search-field-parcel-field4")).SendKeys(Sct);

                        IWebElement element5 = chDriver.FindElement(By.Id("taxweb_grid-search-field-parcel-field5"));
                        element5.Clear();
                        chDriver.FindElement(By.Id("taxweb_grid-search-field-parcel-field5")).Click();
                        chDriver.FindElement(By.Id("taxweb_grid-search-field-parcel-field5")).SendKeys(Sub);

                        IWebElement element6 = chDriver.FindElement(By.Id("taxweb_grid-search-field-parcel-field6"));
                        element6.Clear();
                        chDriver.FindElement(By.Id("taxweb_grid-search-field-parcel-field6")).Click();
                        chDriver.FindElement(By.Id("taxweb_grid-search-field-parcel-field6")).SendKeys(Qtr);

                        IWebElement element7 = chDriver.FindElement(By.Id("taxweb_grid-search-field-parcel-field7"));
                        element7.Clear();
                        chDriver.FindElement(By.Id("taxweb_grid-search-field-parcel-field7")).Click();
                        chDriver.FindElement(By.Id("taxweb_grid-search-field-parcel-field7")).SendKeys(Lot);

                        IWebElement element8 = chDriver.FindElement(By.Id("taxweb_grid-search-field-parcel-field8"));
                        element8.Clear();
                        chDriver.FindElement(By.Id("taxweb_grid-search-field-parcel-field8")).Click();
                        chDriver.FindElement(By.Id("taxweb_grid-search-field-parcel-field8")).SendKeys(Split);
                        gc.CreatePdf(orderNumber, parcelNumber, "Half data view pdf1", chDriver, "MS", "DeSoto");

                        chDriver.FindElement(By.XPath("//*[@id='ext-gen9']")).SendKeys(Keys.Enter);
                        Thread.Sleep(9000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Details Half data view pdf1", chDriver, "MS", "DeSoto");
                        ByVisibleElement(chDriver.FindElement(By.XPath("//*[@id='ext-gen58']/table[5]/tbody/tr[1]/td[2]/table/tbody/tr[1]/td[2]/p/u/b")), chDriver);
                        gc.CreatePdf(orderNumber, parcelNumber, "Tax Details search Result1", chDriver, "MS", "DeSoto");
                        try
                        {
                            ByVisibleElement(chDriver.FindElement(By.XPath("//*[@id='ext-gen58']/table[13]/tbody/tr[1]/td[4]/b")), chDriver);
                            gc.CreatePdf(orderNumber, parcelNumber, "Tax Details search Result2", chDriver, "MS", "DeSoto");
                        }
                        catch { }
                        chDriver.Quit();
                    }
                    catch { }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "MS", "DeSoto", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "MS", "DeSoto");
                    return "Data Inserted Successfully";
                }

                catch (Exception ex)
                {
                    driver.Quit();
                    throw ex;
                }
            }
        }
        public void ByVisibleElement(IWebElement Element, IWebDriver driver)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].scrollIntoView();", Element);
        }
    }
}

//Tax site and Assessement site as to same
//===========================================

//            try
//            {
//                StartTime = DateTime.Now.ToString("HH:mm:ss");
//                driver.Navigate().GoToUrl("http://173.225.129.53/tax?state=MS&county=17");
//                if (searchType == "titleflex")
//                {
//                    string Address = "";
//                    if (direction != "")
//                    {
//                        Address = streetNo + " " + direction + " " + streetName + " " + streetType + " " + unitNumber;
//                    }
//                    else { Address = streetNo + " " + streetName + " " + streetType + " " + unitNumber; }
//                    gc.TitleFlexSearch(orderNumber, "", "", Address.Trim(), "MS", "DeSoto");
//                    if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
//                    {
//                        driver.Quit();
//                        return "MultiParcel";
//                    }
//                    parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
//searchType = "parcel";
//                }
//                if (searchType == "address")
//                {
//                    driver.FindElement(By.Id("taxweb_grid-search-field-street-number-field")).SendKeys(streetNo);
//driver.FindElement(By.Id("taxweb_grid-search-field-street-name-field")).SendKeys(streetName + " " + streetType);
//gc.CreatePdf_WOP(orderNumber, "Address search", driver, "MS", "DeSoto");
//                    driver.FindElement(By.Id("ext-gen9")).SendKeys(Keys.Enter);
//Thread.Sleep(3000);
//                    try
//                    {
//                        IWebElement INorecord = driver.FindElement(By.Id("ext-gen9"));
//                        if (INorecord.Text.Contains("no records"))
//                        {
//                            HttpContext.Current.Session["multiparcel_DesotoMS_NoRecord"] = "Yes";
//                        }
//                    }
//                    catch { }
//                    gc.CreatePdf_WOP(orderNumber, "Address search Result", driver, "MS", "DeSoto");
//                    try
//                    {
//                        string multi = GlobalClass.After(driver.FindElement(By.Id("ext-comp-1008")).Text, "of").Trim();
//string strAddress = "", strparcel = "", strOwner = "";
//                        if (Convert.ToInt32(multi) <= 25)
//                        {
//                            IWebElement tbmulti = driver.FindElement(By.Id("ext-gen16"));
//IList<IWebElement> TBmulti = tbmulti.FindElements(By.TagName("table"));
//IList<IWebElement> TRmulti;
//IList<IWebElement> TDmulti;
//                            foreach (IWebElement row in TBmulti)
//                            {
//                                TDmulti = row.FindElements(By.TagName("td"));
//                                if (TDmulti.Count != 0 && TDmulti[0].Text.Trim() != "")
//                                {
//                                    strAddress = TDmulti[3].Text;
//                                    strparcel = TDmulti[1].Text;
//                                    strOwner = TDmulti[0].Text;

//                                    string multiDetails = strAddress + "~" + strOwner;
//gc.insert_date(orderNumber, strparcel, 1351, multiDetails, 1, DateTime.Now);
//                                }
//                            }
//                            gc.CreatePdf_WOP(orderNumber, "Multi Address search Result", driver, "MS", "DeSoto");
//                            HttpContext.Current.Session["multiparcel_DesotoMS"] = "Yes";
//                            driver.Quit();
//                            gc.mergpdf(orderNumber, "MS", "DeSoto");
//                            return "MultiParcel";
//                        }

//                        if (Convert.ToInt32(multi) > 25)
//                        {
//                            HttpContext.Current.Session["multiparcel_DesotoMS_Multicount"] = "Maximum";
//                            driver.Quit();
//                            return "Maximum";
//                        }
//                    }
//                    catch { }

//                }
//                if (searchType == "parcel")
//                {
//                    parcelNumber = parcelNumber.Replace(" ", "").Trim();
//string Town = parcelNumber.Substring(0, 1);
//string Rin = parcelNumber.Substring(1, 2);
//string Area = parcelNumber.Substring(3, 1);
//string Sct = parcelNumber.Substring(4, 2);
//string Sub = parcelNumber.Substring(6, 2);
//string Qtr = parcelNumber.Substring(8, 1);
//string Lot = parcelNumber.Substring(9, 5);
//string Split = parcelNumber.Substring(14, 2);
//driver.FindElement(By.Id("taxweb_grid-search-field-parcel-field1")).SendKeys(Town);
//driver.FindElement(By.Id("taxweb_grid-search-field-parcel-field2")).SendKeys(Rin);
//driver.FindElement(By.Id("taxweb_grid-search-field-parcel-field3")).SendKeys(Area);
//driver.FindElement(By.Id("taxweb_grid-search-field-parcel-field4")).SendKeys(Sct);
//driver.FindElement(By.Id("taxweb_grid-search-field-parcel-field5")).SendKeys(Sub);
//driver.FindElement(By.Id("taxweb_grid-search-field-parcel-field6")).SendKeys(Qtr);
//driver.FindElement(By.Id("taxweb_grid-search-field-parcel-field7")).SendKeys(Lot);
//driver.FindElement(By.Id("taxweb_grid-search-field-parcel-field8")).SendKeys(Split);
//gc.CreatePdf_WOP(orderNumber, "Parcel search", driver, "MS", "DeSoto");
//                    driver.FindElement(By.Id("ext-gen9")).SendKeys(Keys.Enter);
//gc.CreatePdf_WOP(orderNumber, "Parcel search Result", driver, "MS", "DeSoto");
//                    try
//                    {
//                        IWebElement INorecord = driver.FindElement(By.Id("ext-gen9"));
//                        if (INorecord.Text.Contains("no records"))
//                        {
//                            HttpContext.Current.Session["multiparcel_DesotoMS_NoRecord"] = "Yes";
//                        }
//                    }
//                    catch { }
//                }
//                if (searchType == "ownername")
//                {
//                    driver.FindElement(By.Id("taxweb_grid-search-field-last-name-field")).SendKeys(ownerName);
//gc.CreatePdf_WOP(orderNumber, "Owner search", driver, "MS", "DeSoto");
//                    driver.FindElement(By.Id("ext-gen9")).SendKeys(Keys.Enter);
//gc.CreatePdf_WOP(orderNumber, "Owner search Result", driver, "MS", "DeSoto");
//                    try
//                    {
//                        IWebElement INorecord = driver.FindElement(By.Id("ext-gen9"));
//                        if (INorecord.Text.Contains("no records"))
//                        {
//                            HttpContext.Current.Session["multiparcel_DesotoMS_NoRecord"] = "Yes";
//                        }
//                    }
//                    catch { }
//                    try
//                    {
//                        string multi = GlobalClass.After(driver.FindElement(By.Id("ext-comp-1008")).Text, "of").Trim();
//string strAddress = "", strparcel = "", strOwner = "";
//                        if (Convert.ToInt32(multi) <= 25)
//                        {
//                            IWebElement tbmulti = driver.FindElement(By.Id("ext-gen16"));
//IList<IWebElement> TBmulti = tbmulti.FindElements(By.TagName("table"));
//IList<IWebElement> TRmulti;
//IList<IWebElement> TDmulti;
//                            foreach (IWebElement row in TBmulti)
//                            {
//                                TDmulti = row.FindElements(By.TagName("td"));
//                                if (TDmulti.Count != 0 && TDmulti[0].Text.Trim() != "")
//                                {
//                                    strAddress = TDmulti[3].Text;
//                                    strparcel = TDmulti[1].Text;
//                                    strOwner = TDmulti[0].Text;

//                                    string multiDetails = strAddress + "~" + strOwner;
//gc.insert_date(orderNumber, strparcel, 1351, multiDetails, 1, DateTime.Now);
//                                }
//                            }
//                            gc.CreatePdf_WOP(orderNumber, "Multi Owner search Result", driver, "MS", "DeSoto");
//                            HttpContext.Current.Session["multiparcel_DesotoMS"] = "Yes";
//                            driver.Quit();
//                            gc.mergpdf(orderNumber, "MS", "DeSoto");
//                            return "MultiParcel";
//                        }

//                        if (Convert.ToInt32(multi) > 25)
//                        {
//                            HttpContext.Current.Session["multiparcel_DesotoMS_Multicount"] = "Maximum";
//                            driver.Quit();
//                            return "Maximum";
//                        }
//                    }
//                    catch { }
//                }

//                try
//                {
//                    driver.FindElement(By.XPath("//*[@id='ext-gen18']/div/table/tbody/tr/td[1]/div/a")).Click();
//                }
//                catch { }



//                string strownerName = "", strPropertyAddress = "", County = "", TaxYear = "", Receipt = "", LegalDescription = "";
//IWebElement IProperty = driver.FindElement(By.XPath("//*[@id='ext-gen58']/table[1]/tbody"));
//TaxYear = gc.Between(IProperty.Text, "Tax Year:", "County Name:").Trim();
//County = gc.Between(IProperty.Text, "County Name:", "\r\nName:").Trim();
//strPropertyAddress = gc.Between(IProperty.Text, "Address:", "City:").Trim();
//IWebElement IParcel = driver.FindElement(By.XPath("//*[@id='ext-gen58']/table[2]"));
//parcelNumber = gc.Between(IParcel.Text, "Twn", "Receipt #:").Replace("Rng", "").Replace("Area", "").Replace("Sct", "").Replace("Sub", "").Replace("Qtr", "").Replace("Lot#", "").Replace("Split", "").Trim();
//Receipt = gc.Between(IParcel.Text, "Receipt #:", "Land Owner Name:").Trim();
//strownerName = GlobalClass.After(IParcel.Text, "Land Owner Name:\r\n").Trim();
//LegalDescription = driver.FindElement(By.XPath("//*[@id='ext-gen58']/table[3]/tbody/tr/td/p[1]")).Text;
//                string PropertyDetails = strPropertyAddress + "~" + strownerName + "~" + County + "~" + TaxYear + "~" + Receipt + "~" + LegalDescription;
//gc.insert_date(orderNumber, parcelNumber, 1355, PropertyDetails, 1, DateTime.Now);

//                string TaxDistrict = "", Assessment = "", Millage = "", GrossTax = "", HomeStead = "";
//IWebElement IAssessment = driver.FindElement(By.XPath("//*[@id='ext-gen58']/table[4]/tbody/tr/td/table"));
//IList<IWebElement> IAssessmentRow = IAssessment.FindElements(By.TagName("tr"));
//IList<IWebElement> IAssessmentTD;
//                foreach (IWebElement Assess in IAssessmentRow)
//                {
//                    IAssessmentTD = Assess.FindElements(By.TagName("td"));
//                    if (IAssessmentTD.Count != 0 && !Assess.Text.Contains("District:") && (Assess.Text.Contains("True:") || Assess.Text.Contains("Assessed:")))
//                    {
//                        TaxDistrict += IAssessmentTD[0].Text + " ";
//                        Assessment += IAssessmentTD[2].Text + "~" + IAssessmentTD[3].Text + "~" + IAssessmentTD[4].Text + "~";
//                        if (Assess.Text.Contains("Assessed:"))
//                        {
//                            Millage = IAssessmentTD[5].Text;
//                            GrossTax = IAssessmentTD[6].Text;
//                        }
//                    }
//                    if (IAssessmentTD.Count != 0 && !Assess.Text.Contains("District:") && Assess.Text.Contains("Homestead Credit Amount:"))
//                    {
//                        HomeStead = IAssessmentTD[5].Text;
//                    }
//                }

//                string AssessmentDetails = TaxDistrict.Trim() + "~" + Assessment + Millage + "~" + GrossTax + "~" + HomeStead;
//gc.insert_date(orderNumber, parcelNumber, 1356, AssessmentDetails, 1, DateTime.Now);