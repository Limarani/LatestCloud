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
    public class WebDriver_MahoningOH
    {

        List<string> AddressSearch = new List<string>();
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());


        public string FTP_Mahoning(string streetno, string streetname, string direction, string streetype, string unitno, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            List<string> multiparcel = new List<string>();
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            // driver = new ChromeDriver();
            using (driver = new ChromeDriver())
            {

                StartTime = DateTime.Now.ToString("HH:mm:ss");

                driver.Navigate().GoToUrl("http://oh-mahoning-auditor.publicaccessnow.com/");
                Thread.Sleep(2000);
                try
                {
                    if (searchType == "titleflex")
                    {
                        string address = streetno + " " + direction + " " + streetname + " " + streetype + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "OH", "Mahoning");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_MahoningOH"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    if (searchType == "address")
                    {
                        string taxurl = driver.FindElement(By.XPath("//*[@id='dnn_ctr465_HtmlModule_lblContent']/table/tbody/tr/td[3]/a")).GetAttribute("href");
                        driver.Navigate().GoToUrl(taxurl);
                        driver.FindElement(By.Id("HouseNoLow")).SendKeys(streetno);
                        driver.FindElement(By.Id("Street")).SendKeys(streetname);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "SearchAddressBefore", driver, "OH", "Mahoning");

                        IWebElement IAddressSearch = driver.FindElement(By.LinkText("Search"));
                        IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                        js.ExecuteScript("arguments[0].click();", IAddressSearch);
                        Thread.Sleep(9000);
                        gc.CreatePdf_WOP(orderNumber, "SearchAddressAfter", driver, "OH", "Mahoning");
                        //Multiparcel
                        try
                        {
                            int count = 0;
                            Thread.Sleep(5000);
                            string Owner = "", Address = "", MultiParcelNumber = "";
                            int Mcount = 0;
                            //IWebElement mul = driver.FindElement(By.Id("lxT438"));
                            //string exceptmul = GlobalClass.Before(mul.Text, "\r\n");
                            IWebElement multy = driver.FindElement(By.XPath("//*[@id='tabs-1']/table[2]/tbody"));
                            IList<IWebElement> muladdress = multy.FindElements(By.TagName("table"));
                            IList<IWebElement> mulid;
                            foreach (IWebElement addressrow in muladdress)
                            {
                                mulid = addressrow.FindElements(By.TagName("td"));
                                if (mulid.Count != 0 && !addressrow.Text.Contains("Parcel Number"))
                                {

                                    if (mulid.Count > 2)
                                    {
                                        if (Mcount <= 20)
                                        {
                                            MultiParcelNumber = mulid[1].Text;
                                            Address = mulid[2].Text;
                                            Owner = mulid[0].Text;
                                            string MultyInst = Address + "~" + Owner;
                                            gc.insert_date(orderNumber, MultiParcelNumber, 850, MultyInst, 1, DateTime.Now);
                                        }
                                    }
                                    Mcount++;
                                }
                            }
                            gc.CreatePdf_WOP(orderNumber, "Multiparcel Result", driver, "OH", "Mahoning");
                            if (Mcount > 1 && Mcount < 26)
                            {
                                HttpContext.Current.Session["multiparcel_Mahoning"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Mcount > 26)
                                HttpContext.Current.Session["multiParcel_Mahoning_Maximum"] = "Maximum";
                            driver.Quit();
                            return "Maximum";

                        }
                        catch
                        {
                        }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='tabs-1']/div/span")).Text;
                            if (nodata.Contains("No Results Found"))
                            {
                                HttpContext.Current.Session["Nodata_MahoningOH"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    if (searchType == "parcel")
                    {
                        string taxurl = driver.FindElement(By.XPath("//*[@id='dnn_ctr465_HtmlModule_lblContent']/table/tbody/tr/td[5]/a")).GetAttribute("href");
                        driver.Navigate().GoToUrl(taxurl);
                        parcelNumber = parcelNumber.Replace("-", "").Replace(" ", "").Replace(".", "").Trim();
                        driver.FindElement(By.Id("owner")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "SearchAddressBefore", driver, "OH", "Mahoning");

                        IWebElement IParcelSearch = driver.FindElement(By.XPath("//*[@id='tabs-1']/button"));
                        IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                        js.ExecuteScript("arguments[0].click();", IParcelSearch);
                        Thread.Sleep(9000);
                        gc.CreatePdf(orderNumber, parcelNumber, "SearchAddressAfter", driver, "OH", "Mahoning");
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='tabs-1']/div/span")).Text;
                            if (nodata.Contains("No Results Found"))
                            {
                                HttpContext.Current.Session["Nodata_MahoningOH"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    if (searchType == "ownername")
                    {
                        string taxurl = driver.FindElement(By.XPath("//*[@id='dnn_ctr465_HtmlModule_lblContent']/table/tbody/tr/td[4]/a")).GetAttribute("href");
                        driver.Navigate().GoToUrl(taxurl);
                        gc.CreatePdf(orderNumber, parcelNumber, "SearchAddressBefore", driver, "OH", "Mahoning");
                        driver.FindElement(By.Id("owner")).SendKeys(ownername);

                        IWebElement IOwnerSearch = driver.FindElement(By.XPath("//*[@id='tabs-1']/button"));
                        IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                        js.ExecuteScript("arguments[0].click();", IOwnerSearch);
                        Thread.Sleep(9000);
                        gc.CreatePdf(orderNumber, parcelNumber, "SearchAddressAfter", driver, "OH", "Mahoning");
                        //Multiparcel
                        try
                        {
                            int count = 0;
                            Thread.Sleep(5000);
                            string Owner = "", Address = "", MultiParcelNumber = "";
                            int Mcount = 0;
                            IWebElement mul = driver.FindElement(By.Id("lxT438"));
                            string exceptmul = GlobalClass.Before(mul.Text, "\r\n");
                            if (Convert.ToInt32(exceptmul) <= 20)
                            {
                                IWebElement multy = driver.FindElement(By.XPath("//*[@id='tabs-1']/table[2]/tbody"));
                                IList<IWebElement> muladdress = multy.FindElements(By.TagName("table"));
                                IList<IWebElement> mulid;
                                foreach (IWebElement addressrow in muladdress)
                                {
                                    mulid = addressrow.FindElements(By.TagName("td"));
                                    if (mulid.Count != 0 && !addressrow.Text.Contains("Parcel Number"))
                                    {
                                        MultiParcelNumber = mulid[1].Text;
                                        Address = mulid[2].Text;
                                        Owner = mulid[0].Text;
                                        string MultyInst = Address + "~" + Owner;
                                        gc.insert_date(orderNumber, MultiParcelNumber, 850, MultyInst, 1, DateTime.Now);
                                    }
                                }
                                gc.CreatePdf_WOP(orderNumber, "Multiparcel Result", driver, "OH", "Mahoning");

                                HttpContext.Current.Session["multiparcel_Mahoning"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiParcel_Mahoning_Maximum"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                        }
                        catch { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='tabs-1']/div/span")).Text;
                            if (nodata.Contains("No Results Found"))
                            {
                                HttpContext.Current.Session["Nodata_MahoningOH"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }


                    //Property Details
                    string ParcelID = "", PropertyAddress = "", Ownername = "", MailingAddress = "", TaxSet = "", SchoolDistrict = "", Neighborhood = "", UseCode = "", Acres = "", Description = "", YearBuilt = "";
                    try
                    {
                        //gc.CreatePdf_WOP(orderNumber, "Multiparcel Result", driver, "OH", "Mahoning");
                        string propbulktext = driver.FindElement(By.XPath("//*[@id='lxT427']/div/table/tbody/tr[2]/td/table/tbody/tr[1]/td[1]/table/tbody")).Text.Trim();
                        ParcelID = gc.Between(propbulktext, "Property Number ", "Owner Name").Trim();
                        gc.CreatePdf(orderNumber, ParcelID, "SearchAddressAfter", driver, "OH", "Mahoning");
                        Ownername = gc.Between(propbulktext, "Owner Name", "Owner Address").Trim();
                        MailingAddress = gc.Between(propbulktext, "Owner Address", "Tax Set").Trim();
                        TaxSet = gc.Between(propbulktext, "Tax Set", "School District").Trim();
                        SchoolDistrict = gc.Between(propbulktext, "School District", "Neighborhood").Trim();
                        Neighborhood = gc.Between(propbulktext, "Neighborhood", "Use Code").Trim();
                        UseCode = gc.Between(propbulktext, "Use Code", "Acres").Trim();
                        Acres = gc.Between(propbulktext, "Acres", "Description").Trim();
                        Description = GlobalClass.After(propbulktext, "Description").Trim();
                        string propbulktext1 = driver.FindElement(By.XPath("//*[@id='lxT427']/div/table/tbody/tr[2]/td/table/tbody/tr[1]/td[2]/table/tbody")).Text.Trim();
                        PropertyAddress = gc.Between(propbulktext1, "Property Address:", "Tax Payer Address:").Trim();
                        try
                        {
                            IWebElement IyearBuilt = driver.FindElement(By.XPath("//*[@id='lxT427']/div/table/tbody/tr[4]/td/table/tbody/tr/td/table/tbody"));
                            IList<IWebElement> IyearBuiltRow = IyearBuilt.FindElements(By.TagName("tr"));
                            IList<IWebElement> IyearBuiltTd;
                            foreach (IWebElement year in IyearBuiltRow)
                            {
                                IyearBuiltTd = year.FindElements(By.TagName("td"));
                                if (IyearBuiltTd.Count != 0 && year.Text.Contains("Year Built"))
                                {
                                    try
                                    {
                                        if (IyearBuiltTd[0].Text == "Year Built")
                                        {
                                            YearBuilt = IyearBuiltTd[1].Text;
                                        }
                                        else if (IyearBuiltTd[2].Text == "Year Built")
                                        {
                                            YearBuilt = IyearBuiltTd[3].Text;
                                        }
                                        else if (IyearBuiltTd[4].Text == "Year Built")
                                        {
                                            YearBuilt = IyearBuiltTd[5].Text;
                                        }
                                    }
                                    catch { }
                                }

                            }

                        }
                        catch
                        {

                        }


                        string propertydetails = ParcelID + "~" + PropertyAddress + "~" + Ownername + "~" + MailingAddress + "~" + TaxSet + "~" + SchoolDistrict + "~" + Neighborhood + "~" + UseCode + "~" + Acres + "~" + Description + "~" + YearBuilt;
                        gc.insert_date(orderNumber, ParcelID, 832, propertydetails, 1, DateTime.Now);
                    }
                    catch (Exception ex)
                    {

                        GlobalClass.LogError(ex, orderNumber);
                        throw ex;
                    }
                    //Assessment Details
                    string MarketLandValue = "", CAUV = "", MarketImprovementValue = "", Total = "", AssessedlandValue = "", AssessedImprovementsValue = "", AssessedTotalValue = "", BoardofRevision = "", HomesteadDisability = "", OwnerOccCredit = "", DividedProperty = "", NewConstruction = "", Foreclosure = "", OtherAssessments = "", FrontFt = "";
                    try
                    {
                        string assessbulktext = driver.FindElement(By.XPath("//*[@id='lxT427']/div/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table/tbody")).Text.Trim();
                        BoardofRevision = gc.Between(assessbulktext, "Board of Revision", "Mkt Land Value").Trim();
                        MarketLandValue = gc.Between(assessbulktext, "Mkt Land Value", "Valid Sale").Trim();
                        HomesteadDisability = gc.Between(assessbulktext, "Homestead/Disability", "CAUV").Trim();
                        CAUV = gc.Between(assessbulktext, "CAUV", "# Parcels").Trim();
                        OwnerOccCredit = gc.Between(assessbulktext, "Owner Occupied", "Mkt Impr Value").Trim();
                        MarketImprovementValue = gc.Between(assessbulktext, "Mkt Impr Value", "Deed Type").Trim();
                        DividedProperty = gc.Between(assessbulktext, "Divided Property", "Total").Trim();
                        Total = gc.Between(assessbulktext, "Total", "Amount").Trim();
                        NewConstruction = gc.Between(assessbulktext, "New Construction", "Current Tax").Trim();
                        Foreclosure = gc.Between(assessbulktext, "Foreclosure", "Annual Tax").Trim();
                        OtherAssessments = gc.Between(assessbulktext, "Other Assessments", "Paid").Trim();
                        FrontFt = gc.Between(assessbulktext, "Front Ft.", "Delq").Trim();
                    }
                    catch (Exception ex)
                    {
                        GlobalClass.LogError(ex, orderNumber);
                        throw ex;
                    }
                    try
                    {
                        string taxurl = driver.FindElement(By.LinkText("Tax")).GetAttribute("href");
                        driver.Navigate().GoToUrl(taxurl);
                        Thread.Sleep(5000);
                        gc.CreatePdf(orderNumber, ParcelID, "TaxPagePdf", driver, "OH", "Mahoning");

                        string bulkdataset = driver.FindElement(By.XPath("//*[@id='lxT428']/div/table/tbody/tr/td/table/tbody/tr[1]/td/table[2]/tbody/tr[4]/td/table/tbody")).Text.Trim();
                        AssessedlandValue = gc.Between(bulkdataset, "Land", "Effective Rate").Trim();
                        AssessedImprovementsValue = gc.Between(bulkdataset, "Improvements", "Certified Delq Year").Trim();
                        AssessedTotalValue = gc.Between(bulkdataset, "Total", "Payment Plan").Trim();

                        string assessmentdetails = MarketLandValue + "~" + CAUV + "~" + MarketImprovementValue + "~" + Total + "~" + AssessedlandValue + "~" + AssessedImprovementsValue + "~" + AssessedTotalValue + "~" + BoardofRevision + "~" + HomesteadDisability + "~" + OwnerOccCredit + "~" + DividedProperty + "~" + NewConstruction + "~" + Foreclosure + "~" + OtherAssessments + "~" + FrontFt;
                        gc.insert_date(orderNumber, ParcelID, 837, assessmentdetails, 1, DateTime.Now);
                        AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    }
                    catch (Exception ex)
                    {
                        //  driver.Quit();
                        GlobalClass.LogError(ex, orderNumber);
                        throw ex;
                    }
                    //Tax information Details1
                    string FullRate = "", ReductionFactor = "", EffectiveRate = "", CertifiedDelqYear = "", PaymentPlan = "", TaxDue = "", PaidtoDate = "", TaxingAuthority = "";
                    try
                    {
                        string taxbulkdata = driver.FindElement(By.XPath("//*[@id='lxT428']/div/table/tbody/tr/td/table/tbody/tr[1]/td/table[2]/tbody/tr[4]/td/table/tbody/tr/td/table/tbody")).Text.Trim();
                        FullRate = gc.Between(taxbulkdata, "Full Rate", "Assessed Value").Trim();
                        ReductionFactor = gc.Between(taxbulkdata, "Reduction Factor", "Land").Trim();
                        EffectiveRate = gc.Between(taxbulkdata, "Effective Rate", "Improvements").Trim();
                        CertifiedDelqYear = gc.Between(taxbulkdata, "Certified Delq Year", "Total").Trim();
                        PaymentPlan = gc.Between(taxbulkdata, "Payment Plan", "Tax Due").Trim();
                        string[] Taxduesplit = GlobalClass.After(taxbulkdata, "Date\r\n").Trim().Split(' ');
                        TaxDue = Taxduesplit[0];
                        PaidtoDate = Taxduesplit[1];
                    }
                    catch (Exception ex)
                    {

                        GlobalClass.LogError(ex, orderNumber);
                        throw ex;
                    }

                    //Tax information Details2
                    string Field = "", TaxType = "", StartYear = "", ExpirationYear = "", PriorChg = "", PriorAdj = "", FirstHalfChg = "", FirstHalfAdj = "", SecondHalfChg = "", SecondHalfAdj = "";
                    try
                    {
                        IWebElement taxinfo2 = driver.FindElement(By.XPath("//*[@id='lxT428']/div/table/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr[2]/td/table/tbody"));
                        IList<IWebElement> TRtaxinfo2 = taxinfo2.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDtaxinfo2;
                        foreach (IWebElement row in TRtaxinfo2)
                        {
                            TDtaxinfo2 = row.FindElements(By.TagName("td"));
                            if (TDtaxinfo2.Count != 0 && TDtaxinfo2.Count == 7 && !row.Text.Contains("Prior") && !row.Text.Contains("Chg"))
                            {
                                Field = TDtaxinfo2[0].Text;
                                PriorChg = TDtaxinfo2[1].Text;
                                PriorAdj = TDtaxinfo2[2].Text;
                                FirstHalfChg = TDtaxinfo2[3].Text;
                                FirstHalfAdj = TDtaxinfo2[4].Text;
                                SecondHalfChg = TDtaxinfo2[5].Text;
                                SecondHalfAdj = TDtaxinfo2[6].Text;

                                string Taxinfo2details1 = Field.Trim() + "~" + "Current Tax" + "~" + "" + "~" + "" + "~" + PriorChg.Trim() + "~" + PriorAdj.Trim() + "~" + FirstHalfChg.Trim() + "~" + FirstHalfAdj + "~" + SecondHalfChg + "~" + SecondHalfAdj;
                                gc.insert_date(orderNumber, ParcelID, 848, Taxinfo2details1, 1, DateTime.Now);
                            }
                        }
                    }
                    catch
                    {

                    }
                    int ITaxInfoCount = 0;
                    try
                    {
                        IWebElement ITaxInfo = driver.FindElement(By.XPath("//*[@id='lxT429']/div/table/tbody/tr/td/table/tbody/tr[2]/td/table/tbody"));
                        IList<IWebElement> ITaxInfoRow = ITaxInfo.FindElements(By.TagName("table"));
                        ITaxInfoCount = ITaxInfoRow.Count;
                    }
                    catch
                    {

                    }
                    for (int i = 1; i <= ITaxInfoCount; i++)
                    {
                        TaxType = ""; StartYear = ""; ExpirationYear = "";
                        string Field1 = "", PriorChg1 = "", PriorAdj1 = "", FirstHalfChg1 = "", FirstHalfAdj1 = "", SecondHalfChg1 = "", SecondHalfAdj1 = "";
                        try
                        {
                            IWebElement taxinfo3 = driver.FindElement(By.XPath("//*[@id='lxT429']/div/table/tbody/tr/td/table/tbody/tr[2]/td/table/tbody/tr[" + i + "]/td/table/tbody"));
                            IList<IWebElement> TRtaxinfo3 = taxinfo3.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDtaxinfo3;
                            foreach (IWebElement row in TRtaxinfo3)
                            {
                                TDtaxinfo3 = row.FindElements(By.TagName("td"));
                                if (TDtaxinfo3.Count == 1)
                                {
                                    TaxType = TDtaxinfo3[0].Text;
                                }
                                if (TDtaxinfo3.Count == 4 && !row.Text.Contains("Prior"))
                                {
                                    StartYear = TDtaxinfo3[1].Text;
                                    ExpirationYear = TDtaxinfo3[3].Text;
                                }

                                if (TDtaxinfo3.Count != 0 && TDtaxinfo3.Count == 7 && !row.Text.Contains("Prior") && !row.Text.Contains("Chg"))
                                {
                                    Field1 = TDtaxinfo3[0].Text;
                                    PriorChg1 = TDtaxinfo3[1].Text;
                                    PriorAdj1 = TDtaxinfo3[2].Text;
                                    FirstHalfChg1 = TDtaxinfo3[3].Text;
                                    FirstHalfAdj1 = TDtaxinfo3[4].Text;
                                    SecondHalfChg1 = TDtaxinfo3[5].Text;
                                    SecondHalfAdj1 = TDtaxinfo3[6].Text;

                                    string Taxinfo2details2 = Field1.Trim() + "~" + TaxType.Trim() + "~" + StartYear.Trim() + "~" + ExpirationYear.Trim() + "~" + PriorChg1.Trim() + "~" + PriorAdj1.Trim() + "~" + FirstHalfChg1.Trim() + "~" + FirstHalfAdj1 + "~" + SecondHalfChg1 + "~" + SecondHalfAdj1;
                                    gc.insert_date(orderNumber, ParcelID, 848, Taxinfo2details2, 1, DateTime.Now);
                                }
                            }
                        }
                        catch
                        {

                        }
                    }

                    //Payment History Details
                    string PaidDate = "", Installment = "", Prior = "", FirstHalf = "", SecondHalf = "", Receipt = "";
                    try
                    {

                        IWebElement payinfo2 = driver.FindElement(By.XPath(" //*[@id='lxT430']/div/table/tbody/tr[3]/td/table/tbody"));
                        IList<IWebElement> TRpayinfo2 = payinfo2.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDpayinfo2;
                        foreach (IWebElement row in TRpayinfo2)
                        {
                            TDpayinfo2 = row.FindElements(By.TagName("td"));
                            if (TDpayinfo2.Count != 0 && TDpayinfo2.Count == 6)
                            {
                                PaidDate = TDpayinfo2[0].Text;
                                Installment = TDpayinfo2[1].Text;
                                Prior = TDpayinfo2[2].Text;
                                FirstHalf = TDpayinfo2[3].Text;
                                SecondHalf = TDpayinfo2[4].Text;
                                Receipt = TDpayinfo2[5].Text;

                                string PaymentHisotryDetails = PaidDate.Trim() + "~" + Installment.Trim() + "~" + Prior + "~" + FirstHalf.Trim() + "~" + SecondHalf.Trim() + "~" + Receipt;
                                gc.insert_date(orderNumber, ParcelID, 849, PaymentHisotryDetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch
                    {

                    }
                    //Tax Authority
                    try
                    {
                        driver.Navigate().GoToUrl("https://www.mahoningcountyoh.gov/831/Property-Tax-Payment-Options");
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, ParcelID, "TaxingAuthority", driver, "OH", "Mahoning");
                        TaxingAuthority = driver.FindElement(By.XPath("//*[@id='divEditor359ec1ff-9a6e-44db-a060-07ad9467006c']/strong")).Text.Trim();
                    }
                    catch
                    {
                    }
                    string taxinformationdetails1 = FullRate + "~" + ReductionFactor + "~" + EffectiveRate + "~" + CertifiedDelqYear + "~" + PaymentPlan + "~" + TaxDue + "~" + PaidtoDate + "~" + TaxingAuthority;
                    gc.insert_date(orderNumber, ParcelID, 847, taxinformationdetails1, 1, DateTime.Now);

                    //End
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Quit();
                    gc.mergpdf(orderNumber, "OH", "Mahoning");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "OH", "Mahoning", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
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
