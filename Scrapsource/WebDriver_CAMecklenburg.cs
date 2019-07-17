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

namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_CAMecklenburg
    {
        string outputPath = "";
        IWebDriver driver;
        DBconnection db = new DBconnection();
        IWebElement good_date;
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_Mecklenburg(string address, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "", AssessTakenTime = "", TaxTakentime = "", CityTaxtakentime = "";
            string TotaltakenTime = "";
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string Parcel_No = "-", Account_No = "-", Location_Address = "-", Owner_Name = "-", Mailing_Address = "-", Land_Use_Code = "-", Land_Use_Desc = "-", Legal_Description = "-", Year_Built = "-";
            string Land_Value = "-", Building_Value = "-", Extra_Features = "-", Total_Appraised_Value = "-", Exemption_Deferment = "-";
            string owner_name = "-", Propertytax_Bill = "-", Bill_Status = "-", Bill_Flag = "-", Due_Date = "-", Interest_Begins = "-", Total_Billed = "-", Interest = "-", Paid_Date = "-", Type = "-", Paid_By = "-", Receipt_No = "-", Paid_Amount = "-", current_due = "-", Good_through_date = "-";
            string strAddress = "", strparcel = "", strOwner = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new PhantomJSDriver();
            // driver = new ChromeDriver();
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    try
                    {
                        StartTime = DateTime.Now.ToString("HH:mm:ss");

                        if (searchType == "titleflex")
                        {

                            gc.TitleFlexSearch(orderNumber, "", "", address, "NC", "Mecklenburg");
                            if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                            {
                                driver.Quit();
                                return "MultiParcel";
                            }
                            else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                            {
                                HttpContext.Current.Session["Nodata_CAMecklenburg"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                            searchType = "parcel";
                            parcelNumber = HttpContext.Current.Session["titleparcel"].ToString().Replace(".", "").Replace("-", "");
                        }

                        if (searchType == "address")
                        {
                            driver.Navigate().GoToUrl("https://property.spatialest.com/nc/mecklenburg/#/");
                            Thread.Sleep(2000);
                            driver.FindElement(By.Id("primary_search")).SendKeys(address);
                            gc.CreatePdf_WOP(orderNumber, "Address search", driver, "NC", "Mecklenburg");
                            driver.FindElement(By.Id("primary_search")).SendKeys(Keys.Enter);
                            Thread.Sleep(7000);

                            gc.CreatePdf_WOP(orderNumber, "Address search Result", driver, "NC", "Mecklenburg");
                            Thread.Sleep(3000);

                            //multi parcel
                            int Max = 0;
                            try
                            {
                                IWebElement Assessment = driver.FindElement(By.XPath("//*[@id='main-app']/div/div[1]/div[2]/div/div/div/div[2]"));
                                IList<IWebElement> TRAssessment = Assessment.FindElements(By.TagName("div"));

                                IList<IWebElement> TDAssessment;
                                foreach (IWebElement row in TRAssessment)
                                {
                                    TDAssessment = row.FindElements(By.TagName("p"));
                                    string strmulti = row.GetAttribute("class");
                                    if (TDAssessment.Count != 0 && !row.Text.Contains("Assessed Total Value") && strmulti == "tile")
                                    {
                                        strAddress = TDAssessment[1].Text;
                                        strparcel = TDAssessment[4].Text.Replace("Parcel:", "").Trim();
                                        strOwner = TDAssessment[5].Text.Replace("Owners:", "").Trim();
                                        string multidetails = strOwner + "~" + strAddress;
                                        gc.insert_date(orderNumber, strparcel, 57, multidetails, 1, DateTime.Now);
                                        Max++;

                                    }

                                    if (TDAssessment.Count != 0 && TDAssessment.Count > 25)
                                    {
                                        HttpContext.Current.Session["multiParcel_mecklenberg_count"] = "Maximum";
                                        driver.Quit();
                                        return "Maximum";
                                    }

                                }
                                if (Max > 1 && Max < 26)
                                {
                                    HttpContext.Current.Session["multiParcel_mecklenberg"] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                                if (Max == 0)
                                {
                                    HttpContext.Current.Session["Nodata_CAMecklenburg"] = "Zero";
                                    driver.Quit();
                                    return "No Data Found";
                                }

                            }
                            catch { }
                        }




                        if (searchType == "parcel")
                        {
                            if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                            {
                                parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                            }
                            driver.Navigate().GoToUrl("https://property.spatialest.com/nc/mecklenburg/#/");
                            Thread.Sleep(2000);
                            driver.FindElement(By.Id("primary_search")).SendKeys(parcelNumber);

                            gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "NC", "Mecklenburg");
                            Thread.Sleep(5000);

                            driver.FindElement(By.Id("primary_search")).SendKeys(Keys.Enter);
                            Thread.Sleep(5000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Result", driver, "NC", "Mecklenburg");
                        }

                        if (searchType == "ownername")
                        {
                            driver.Navigate().GoToUrl("https://property.spatialest.com/nc/mecklenburg/#/");
                            Thread.Sleep(2000);
                            driver.FindElement(By.Id("primary_search")).SendKeys(ownername);
                            gc.CreatePdf_WOP(orderNumber, "Owner search", driver, "NC", "Mecklenburg");
                            driver.FindElement(By.Id("primary_search")).SendKeys(Keys.Enter);
                            Thread.Sleep(5000);
                            gc.CreatePdf_WOP(orderNumber, "Owner search Result", driver, "NC", "Mecklenburg");

                            //multi parcel
                            int Max = 0;
                            try
                            {
                                IWebElement Assessment = driver.FindElement(By.XPath("//*[@id='main-app']/div/div[1]/div[2]/div/div/div/div[2]"));
                                IList<IWebElement> TRAssessment = Assessment.FindElements(By.TagName("div"));

                                IList<IWebElement> TDAssessment;
                                foreach (IWebElement row in TRAssessment)
                                {
                                    TDAssessment = row.FindElements(By.TagName("p"));
                                    string strmulti = row.GetAttribute("class");
                                    if (TDAssessment.Count != 0 && !row.Text.Contains("Assessed Total Value") && strmulti == "tile")
                                    {
                                        strAddress = TDAssessment[1].Text;
                                        strparcel = TDAssessment[4].Text.Replace("Parcel:", "").Trim();
                                        strOwner = TDAssessment[5].Text.Replace("Owners:", "").Trim();
                                        string multidetails = strOwner + "~" + strAddress;
                                        gc.insert_date(orderNumber, strparcel, 57, multidetails, 1, DateTime.Now);
                                        Max++;

                                    }

                                    if (TDAssessment.Count != 0 && TDAssessment.Count > 25)
                                    {
                                        HttpContext.Current.Session["multiParcel_mecklenberg_count"] = "Maximum";
                                        driver.Quit();
                                        return "Maximum";
                                    }

                                }
                                if (Max > 1 && Max < 26)
                                {
                                    HttpContext.Current.Session["multiParcel_mecklenberg"] = "Yes";
                                    driver.Quit();
                                    return "MultiParcel";
                                }
                                if (Max == 0)
                                {
                                    HttpContext.Current.Session["Nodata_CAMecklenburg"] = "Zero";
                                    driver.Quit();
                                    return "No Data Found";
                                }

                            }

                            catch (NoSuchElementException ex1)
                            {
                                driver.Quit();
                                throw ex1;
                            }
                        }
                    }
                    catch (NoSuchElementException ex1)
                    {
                        driver.Quit();
                        throw ex1;
                    }


                    //Property details
                    Parcel_No = driver.FindElement(By.XPath("//*[@id='prccontent']/div/section/div/div[1]/div[2]/header/div/div/div[1]/div[1]/span")).Text.Replace("PARCEL ID:", "").Trim();
                    Location_Address = driver.FindElement(By.XPath("//*[@id='prccontent']/div/section/div/div[1]/div[2]/header/div/div/div[1]/div[2]/span")).Text;
                    try
                    {
                        Owner_Name = driver.FindElement(By.XPath("//*[@id='prccontent']/div/section/div/div[1]/div[2]/header/div/div/div[2]/div/div")).Text;
                        Owner_Name = GlobalClass.Before(Owner_Name, "\r\n");
                    }
                    catch { }
                    Land_Use_Code = driver.FindElement(By.XPath("//*[@id='keyinformation']/div[1]/div[2]/div/ul/li[1]/p[1]/span[2]")).Text;
                    Land_Use_Desc = driver.FindElement(By.XPath("//*[@id='keyinformation']/div[1]/div[2]/div/ul/li[2]/p[1]/span[2]")).Text;
                    Legal_Description = driver.FindElement(By.XPath("//*[@id='keyinformation']/div[1]/div[2]/div/ul/li[6]/p/span[2]")).Text;
                    try
                    {
                        Year_Built = driver.FindElement(By.XPath("//*[@id='BuildingSection_residential_0']/div/div[1]/div/div/ul/li[2]/p/span[2]")).Text;
                    }
                    catch { }

                    string prop_details = Owner_Name + "~" + Location_Address + "~" + Legal_Description + "~" + Land_Use_Code + "~" + Land_Use_Desc + "~" + Year_Built;

                    prop_details = prop_details.Replace("\r\n", ",");
                    gc.insert_date(orderNumber, Parcel_No, 58, prop_details, 1, DateTime.Now);


                    //Assessment details
                    Land_Value = driver.FindElement(By.XPath("//*[@id='keyinformation']/div[2]/div/div/ul/li[2]/p/span[2]")).Text;
                    Building_Value = driver.FindElement(By.XPath("//*[@id='keyinformation']/div[2]/div/div/ul/li[3]/p/span[2]")).Text;
                    Extra_Features = driver.FindElement(By.XPath("//*[@id='keyinformation']/div[2]/div/div/ul/li[4]/p/span[2]")).Text;
                    Total_Appraised_Value = driver.FindElement(By.XPath("//*[@id='keyinformation']/div[2]/div/div/ul/li[5]/p/span[2]")).Text;

                    string ass_details = Land_Value + "~" + Building_Value + "~" + Extra_Features + "~" + Total_Appraised_Value;
                    gc.insert_date(orderNumber, Parcel_No, 59, ass_details, 1, DateTime.Now);

                    gc.CreatePdf(orderNumber, Parcel_No, "Assessment and property details", driver, "NC", "Mecklenburg");
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    //Tax Details
                    driver.Navigate().GoToUrl("https://taxbill.co.mecklenburg.nc.us/publicwebaccess/");
                    //select parcel number from drop down
                    var SerachBy = driver.FindElement(By.Id("lookupCriterion"));
                    var selectElement = new SelectElement(SerachBy);
                    selectElement.SelectByText("Parcel Number");

                    //select tax type from drop down
                    var SerachTax = driver.FindElement(By.Id("taxYear"));
                    var selectElement1 = new SelectElement(SerachTax);
                    selectElement1.SelectByText("ALL");

                    driver.FindElement(By.Id("txtSearchString")).SendKeys(Parcel_No);
                    gc.CreatePdf(orderNumber, Parcel_No, "Tax Details Result", driver, "NC", "Mecklenburg");
                    driver.FindElement(By.Id("btnGo")).SendKeys(Keys.Enter);
                    gc.CreatePdf(orderNumber, Parcel_No, "Tax information", driver, "NC", "Mecklenburg");
                    //Tax History
                    string Bill = "-", Old_Bill = "-", Parcel = "-", Name = "-", location = "-", Bill_Flags = "-", Current_Due = "-", Taxyear = "";
                    IWebElement TBTax = driver.FindElement(By.XPath("//*[@id='G_dgResults']/tbody"));
                    IList<IWebElement> TRTax = TBTax.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDTax;
                    int count = TRTax.Count() - 1;
                    int j = 1;
                    foreach (IWebElement row1 in TRTax)
                    {

                        if (j <= count)
                        {
                            TDTax = row1.FindElements(By.TagName("td"));
                            Bill = TDTax[0].Text;
                            Old_Bill = TDTax[1].Text;
                            if (Old_Bill == " ")
                            {
                                Old_Bill = "-";
                            }
                            Parcel = TDTax[2].Text;
                            Name = TDTax[3].Text;
                            location = TDTax[4].Text;
                            Bill_Flags = TDTax[5].Text;
                            if (Bill_Flags == " ")
                            {
                                Bill_Flags = "-";
                            }
                            Current_Due = TDTax[6].Text;
                            string Tax_History = Bill + "~ " + Old_Bill + "~" + Name + "~" + location + "~" + Bill_Flags + "~" + Current_Due;
                            gc.insert_date(orderNumber, Parcel_No, 60, Tax_History, 1, DateTime.Now);
                        }

                        if (j > count)
                        {

                            string amount = WebDriverTest.After(row1.Text, "Total:");
                            string Tax_History = "-" + "~" + "-" + "~" + "-" + "~" + "-" + "~" + "Total:" + "~" + amount;
                            gc.insert_date(orderNumber, Parcel_No, 60, Tax_History, 1, DateTime.Now);
                        }
                        j++;
                    }

                    //Tax iformation table
                    int k = 0;
                    List<string> Taxlist = new List<string>();
                    string taxbill = "";
                    for (int l = 0; l < 5; l++)
                    {


                        try
                        {
                            IWebElement TaxInfo = driver.FindElement(By.Id("G_dgResults"));
                            IList<IWebElement> TRTaxInfo = TaxInfo.FindElements(By.TagName("tr"));
                            IList<IWebElement> THTaxInfo = TaxInfo.FindElements(By.TagName("th"));
                            IList<IWebElement> TDTaxInfo;
                            foreach (IWebElement row in TRTaxInfo)
                            {
                                TDTaxInfo = row.FindElements(By.TagName("td"));
                                if (TDTaxInfo.Count != 0 && !row.Text.Contains("Bill Flags") && !Taxlist.Contains(TDTaxInfo[0].Text.Trim()))
                                {
                                    taxbill = TDTaxInfo[0].Text.Trim();
                                    Taxlist.Add(taxbill);
                                    IWebElement Iclick = TDTaxInfo[0].FindElement(By.TagName("a"));
                                    Iclick.Click();
                                    Thread.Sleep(4000);
                                    k++;
                                    break;
                                }
                                try
                                {
                                    if (TDTaxInfo.Count != 0 && !row.Text.Contains("Bill Flags") && Taxlist.Contains(TDTaxInfo[0].Text.Trim()) && k != 1 && k != 2)
                                    {
                                        taxbill = TDTaxInfo[0].Text.Trim();
                                        Taxlist.Add(taxbill);
                                        IWebElement Iclick = TDTaxInfo[0].FindElement(By.TagName("a"));
                                        Iclick.Click();
                                        Thread.Sleep(4000);
                                        // break;

                                    }
                                }
                                catch { }
                            }
                        }
                        catch { }


                        //driver.FindElement(By.XPath("//*[@id='dgResults_r_0']/td[1]/a")).SendKeys(Keys.Enter);
                        gc.CreatePdf(orderNumber, Parcel_No, "Tax iformation Result" + l, driver, "NC", "Mecklenburg");
                        try
                        {
                            owner_name = driver.FindElement(By.Id("txtName")).Text;
                        }
                        catch { }
                        try
                        {
                            owner_name = driver.FindElement(By.Id("lblPriOwner")).Text;
                        }
                        catch { }
                        try
                        {
                            Propertytax_Bill = driver.FindElement(By.XPath("//*[@id='lblBill']")).Text;
                        }
                        catch { }
                        try
                        {
                            Propertytax_Bill = driver.FindElement(By.Id("lblNewAccount")).Text;
                        }
                        catch { }
                        try
                        {
                            Bill_Status = driver.FindElement(By.XPath("//*[@id='lblBillStatus']")).Text;
                        }
                        catch { }
                        Bill_Flag = driver.FindElement(By.XPath("//*[@id='lblBillFlag']")).Text;
                        if (Bill_Flag == "")
                        {
                            Bill_Flag = "-";
                        }
                        try
                        {
                            Due_Date = driver.FindElement(By.XPath("//*[@id='lblDueDate']")).Text;
                        }
                        catch { }
                        try
                        {
                            Interest_Begins = driver.FindElement(By.XPath("//*[@id='lblInterest']")).Text;
                        }
                        catch { }
                        try
                        {
                            Interest_Begins = driver.FindElement(By.Id("lblIntBegins")).Text;
                        }
                        catch { }
                        string paidbydate = "";
                        try
                        {
                            paidbydate = driver.FindElement(By.Id("interestCalDate_input")).Text;
                        }
                        catch { }

                        if (Bill_Flag == "DELINQUENT")
                        {
                            good_date = driver.FindElement(By.XPath("//*[@id='interestCalDate_input']"));
                            Good_through_date = good_date.GetAttribute("value");
                            if (Good_through_date.Contains("Select A Date"))
                            {
                                Good_through_date = "-";
                                Total_Billed = driver.FindElement(By.XPath("//*[@id='lblTotalAmountDue']")).Text;
                                Interest = driver.FindElement(By.XPath("//*[@id='lblInterestAmt']")).Text;
                                current_due = driver.FindElement(By.XPath("//*[@id='lblCurrentDue']")).Text;
                            }
                        }
                        else
                        {

                            if (Bill_Flag == "DELINQUENT")
                            {

                                DateTime G_Date = Convert.ToDateTime(Good_through_date);
                                string dateChecking = DateTime.Now.ToString("MM") + "/15/" + DateTime.Now.ToString("yyyy");

                                if (G_Date < Convert.ToDateTime(dateChecking))
                                {
                                    //end of the month
                                    Good_through_date = new DateTime(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(Convert.ToInt16(DateTime.Now.ToString("MM"))), DateTime.DaysInMonth(Convert.ToInt16(DateTime.Now.ToString("yyyy")), Convert.ToInt16(DateTime.Now.ToString("MM")))).ToString("MM/dd/yyyy");

                                }

                                else if (G_Date > Convert.ToDateTime(dateChecking))
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

                                //recalculate interest
                                good_date.Clear();
                                good_date.SendKeys(Good_through_date);
                                driver.FindElement(By.Id("btnRecalInterest")).SendKeys(Keys.Enter);
                                gc.CreatePdf(orderNumber, Parcel_No, "After Good Through Date Calculate" + l, driver, "NC", "Mecklenburg");
                                Total_Billed = driver.FindElement(By.XPath("//*[@id='lblTotalAmountDue']")).Text;
                                Interest = driver.FindElement(By.XPath("//*[@id='lblInterestAmt']")).Text;
                                current_due = driver.FindElement(By.XPath("//*[@id='lblCurrentDue']")).Text;

                            }
                        }


                        //transcation history
                        string strtaxyear1 = "", strtaxyear2 = "", strtaxyear3 = "", Pdate = "";
                        try
                        {
                            Pdate = driver.FindElement(By.Id("lblDueDate")).Text;
                            string[] Tax_year = Pdate.Split('/');
                            strtaxyear1 = Tax_year[0];
                            strtaxyear2 = Tax_year[1];
                            strtaxyear3 = Tax_year[2];
                        }
                        catch { }
                        IWebElement tb_trans = driver.FindElement(By.XPath("//*[@id='dgShowResultHistory']/tbody"));
                        IList<IWebElement> TR_trans = tb_trans.FindElements(By.TagName("tr"));
                        IList<IWebElement> TD_trans;

                        foreach (IWebElement row2 in TR_trans)
                        {
                            if (TR_trans.Count == 1)
                            {

                                string tax = strtaxyear3 + "~ " + owner_name + "~ " + Propertytax_Bill + "~ " + Bill_Status + "~ " + Bill_Flag + "~ " + Due_Date + "~ " + Interest_Begins + "~ " + Total_Billed + "~ " + Interest + "~ " + current_due + "~ " + Good_through_date + "~ " + Paid_Date + "~ " + Type + "~ " + Paid_By + "~ " + Receipt_No + "~ " + Paid_Amount;
                                // tax = tax.Replace("\r\n", "");
                                gc.insert_date(orderNumber, Parcel_No, 61, tax, 1, DateTime.Now);


                            }
                            if (!row2.Text.Contains("Date"))
                            {
                                TD_trans = row2.FindElements(By.TagName("td"));
                                try
                                {
                                    Type = TD_trans[1].Text;
                                    Paid_By = TD_trans[2].Text;
                                    Receipt_No = TD_trans[3].Text;
                                    Paid_Amount = TD_trans[4].Text;

                                }
                                catch { }
                                string tax = strtaxyear3 + "~ " + owner_name + "~ " + Propertytax_Bill + "~ " + Bill_Status + "~ " + Bill_Flag + "~ " + Due_Date + "~ " + Interest_Begins + "~ " + Total_Billed + "~ " + Interest + "~ " + current_due + "~ " + Good_through_date + "~ " + Paid_Date + "~ " + Type + "~ " + Paid_By + "~ " + Receipt_No + "~ " + Paid_Amount;
                                if (tax.Contains("\r\n"))
                                {
                                    tax = tax.Replace("\r\n", "");
                                    gc.insert_date(orderNumber, Parcel_No, 61, tax, 1, DateTime.Now);

                                }
                                else
                                {
                                    gc.insert_date(orderNumber, Parcel_No, 61, tax, 1, DateTime.Now);
                                }
                            }

                        }


                        //Tax/Fee Distribution Table: 
                        string Rate = "-", Tax_Districts = "-", Description = "-", Amount = "-";
                        try
                        {
                            IWebElement TBTaxFee = driver.FindElement(By.XPath("//*[@id='dgShowResultRate']/tbody"));
                            IList<IWebElement> TRTaxFee = TBTaxFee.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDTaxFee;

                            foreach (IWebElement row2 in TRTaxFee)
                            {
                                if (!row2.Text.Contains("Rate"))
                                {
                                    TDTaxFee = row2.FindElements(By.TagName("td"));
                                    Rate = TDTaxFee[0].Text;
                                    if (Rate == "")
                                    {
                                        Rate = "-";
                                    }
                                    Tax_Districts = TDTaxFee[1].Text;
                                    Description = TDTaxFee[2].Text;
                                    Amount = TDTaxFee[3].Text;
                                    string taxfee = strtaxyear3 + "~ " + Rate + "~ " + Tax_Districts + "~ " + Description + "~ " + Amount;
                                    gc.insert_date(orderNumber, Parcel_No, 64, taxfee, 1, DateTime.Now);
                                }
                            }
                        }
                        catch { }
                        driver.Navigate().Back();
                        Thread.Sleep(4000);
                        l++;

                    }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "NC", "Mecklenburg", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "NC", "Mecklenburg");
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

        public void MultiParcel(string ordernumber)
        {
            int i = 1;
            string m_parcelnumber = "-", m_owner_name = "-", m_address = "-", m_description = "-";

            HttpContext.Current.Session["multiParcel_mecklenberg"] = "Yes";
            IList<IWebElement> multiresult = driver.FindElements(By.XPath("//div[@class='results-list']/div"));
            int count = multiresult.Count();
            foreach (IWebElement div in multiresult)
            {
                if (i <= 25)
                {
                    m_parcelnumber = driver.FindElement(By.XPath("/html/body/main/div/div[2]/div/div[2]/div/div[2]/div/div[2]/div[" + i + "]/div/div[2]/ul/li[1]/a")).Text;
                    var splitted = m_parcelnumber.Split(new[] { ' ' }, 2);
                    m_parcelnumber = splitted[0];
                    m_owner_name = splitted[1];
                    m_address = driver.FindElement(By.XPath("/html/body/main/div/div[2]/div/div[2]/div/div[2]/div/div[2]/div[" + i + "]/div/div[2]/ul/li[2]/span[2]")).Text;
                    m_description = driver.FindElement(By.XPath("/html/body/main/div/div[2]/div/div[2]/div/div[2]/div/div[2]/div[" + i + "]/div/div[3]/ul/li[3]/span[2]")).Text;

                    string multiparcedata = m_owner_name + "~" + m_address + "~" + m_description;
                    gc.insert_date(ordernumber, m_parcelnumber, 57, multiparcedata, 1, DateTime.Now);

                }
                i++;
            }
        }


    }
}