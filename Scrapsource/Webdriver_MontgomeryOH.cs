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
    public class Webdriver_MontgomeryOH
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;
        public string FTP_MontgomeryOH(string houseno, string Direction, string sname, string stype, string account, string parcelNumber, string ownername, string searchType, string orderNumber)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string multi = "", TaxAuthority = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {

                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("http://www.mcrealestate.org/search/commonsearch.aspx?mode=address");
                    Thread.Sleep(1000);

                    if (searchType == "titleflex")
                    {
                        string address = "";
                        if (Direction != "")
                        {
                            address = houseno + " " + Direction + " " + sname + " " + stype + " " + account;
                        }
                        if (Direction == "")
                        {
                            address = houseno + " " + sname + " " + stype + " " + account;
                        }

                        gc.TitleFlexSearch(orderNumber, "", "", address.Trim(), "OH", "Montgomery");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_MontgomeryOH"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                        parcelNumber = parcelNumber.Replace("-", " ");

                    }
                    if (searchType == "address")
                    {
                        string Parcelno = "", Ownername = "", parcellocation = "";
                        try
                        {
                            driver.FindElement(By.LinkText("Address")).SendKeys(Keys.Enter);
                            Thread.Sleep(1000);
                        }
                        catch { }

                        driver.FindElement(By.Id("inpNumber")).SendKeys(houseno);
                        driver.FindElement(By.Id("inpStreet")).SendKeys(sname);
                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "OH", "Montgomery");
                        driver.FindElement(By.Id("btSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(1000);
                        //IWebElement multirecord = driver.FindElement(By.XPath("//*[@id='mMessage']"));

                        gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "OH", "Montgomery");
                        try
                        {
                            IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='searchResults']/tbody"));
                            IList<IWebElement> TRmultiaddress = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> THmultiaddress = multiaddress.FindElements(By.TagName("th"));
                            IList<IWebElement> TDmultiaddress;


                            if (TRmultiaddress.Count > 28)
                            {
                                HttpContext.Current.Session["multiParcel_Montgomery_Maximum"] = "Maimum";
                                driver.Quit();
                                return "Maximum";
                            }
                            if (TRmultiaddress.Count > 5)
                            {
                                foreach (IWebElement row in TRmultiaddress)
                                {
                                    TDmultiaddress = row.FindElements(By.TagName("td"));
                                    if (!row.Text.Contains("Parcel Location") && row.Text.Trim() != "" && TDmultiaddress.Count != 2)
                                    {
                                        try
                                        {
                                            Parcelno = TDmultiaddress[0].Text;
                                            Ownername = TDmultiaddress[3].Text;
                                            parcellocation = TDmultiaddress[4].Text;
                                            string Multi = Ownername + "~" + parcellocation;
                                            gc.insert_date(orderNumber, Parcelno, 1172, Multi, 1, DateTime.Now);
                                        }
                                        catch { }

                                    }



                                }
                                HttpContext.Current.Session["multiParcel_Montgomery"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (TRmultiaddress.Count <= 4)
                            {
                                //TDmultiaddress[0].Click();
                                driver.FindElement(By.XPath("//*[@id='searchResults']/tbody/tr[3]/td[1]/table/tbody/tr/td[2]/font")).Click();
                                Thread.Sleep(1000);
                            }
                        }
                        catch { }
                    }

                    if (searchType == "parcel")
                    {
                        try
                        {
                            driver.FindElement(By.LinkText("Parcel")).SendKeys(Keys.Enter);
                            Thread.Sleep(1000);
                        }
                        catch { }

                        driver.FindElement(By.Id("inpParid")).SendKeys(parcelNumber);
                        gc.CreatePdf_WOP(orderNumber, "Parcel Search", driver, "OH", "Montgomery");
                        driver.FindElement(By.Id("btSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(1000);
                        gc.CreatePdf_WOP(orderNumber, "Parcel Search Result", driver, "OH", "Montgomery");
                        try
                        {
                            IWebElement Iclick = driver.FindElement(By.XPath("//*[@id='searchResults']/tbody/tr[3]/td[1]/table/tbody"));
                            Iclick.Click();
                            Thread.Sleep(2000);
                        }
                        catch { }
                    }

                    if (searchType == "ownername")
                    {
                        try
                        {
                            driver.FindElement(By.LinkText("Owner Name")).SendKeys(Keys.Enter);
                            Thread.Sleep(1000);
                        }
                        catch { }

                        driver.FindElement(By.Id("inpOwner")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "Ownername Search", driver, "OH", "Montgomery");
                        driver.FindElement(By.Id("btSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(1000);
                        gc.CreatePdf_WOP(orderNumber, "Ownername Search Result", driver, "OH", "Montgomery");

                        string ParcelNum = "", Owner_Name = "", ParcelLocation = "";
                        try
                        {
                            IWebElement multiadd = driver.FindElement(By.XPath("//*[@id='searchResults']/tbody"));
                            IList<IWebElement> TRmultiadd = multiadd.FindElements(By.TagName("tr"));
                            IList<IWebElement> THmultiadd = multiadd.FindElements(By.TagName("th"));
                            IList<IWebElement> TDmultiadd;


                            if (TRmultiadd.Count > 28)
                            {
                                HttpContext.Current.Session["multiParcel_Montgomery_Maximum"] = "Maimum";
                                driver.Quit();
                                return "Maximum";
                            }
                            if (TRmultiadd.Count > 5)
                            {
                                foreach (IWebElement row in TRmultiadd)
                                {
                                    TDmultiadd = row.FindElements(By.TagName("td"));
                                    if (!row.Text.Contains("Parcel Location") && row.Text.Trim() != "" && TDmultiadd.Count != 2)
                                    {
                                        try
                                        {
                                            ParcelNum = TDmultiadd[0].Text;
                                            Owner_Name = TDmultiadd[3].Text;
                                            ParcelLocation = TDmultiadd[4].Text;
                                            string Multi1 = Owner_Name + "~" + ParcelLocation;
                                            gc.insert_date(orderNumber, ParcelNum, 1172, Multi1, 1, DateTime.Now);
                                        }
                                        catch { }

                                    }



                                }
                                HttpContext.Current.Session["multiParcel_Montgomery"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (TRmultiadd.Count <= 4)
                            {
                                driver.FindElement(By.XPath("//*[@id='searchResults']/tbody/tr[3]/td[1]/table/tbody/tr/td[2]/font")).Click();
                                Thread.Sleep(1000);
                            }
                        }
                        catch { }
                    }

                    try
                    {
                        IWebElement INodata = driver.FindElement(By.XPath("//*[@id='frmMain']/table[2]/tbody/tr/td/table"));
                        if(INodata.Text.Contains("search did not find any records"))
                        {
                            HttpContext.Current.Session["Nodata_MontgomeryOH"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    // Property Details

                    string propertydata = driver.FindElement(By.XPath("//*[@id='datalet_header_row']/td/table/tbody")).Text;
                    string Parcel_Location = "", Yearbuilt = "", Mail_Address1 = "", Mail_Address2 = "";
                    parcelNumber = gc.Between(propertydata, "PARID:", "PARCEL LOCATION:").Trim();
                    Parcel_Location = GlobalClass.After(propertydata, "PARCEL LOCATION:").Trim();

                    string Bulkdata = driver.FindElement(By.XPath("//*[@id='Mailing']/tbody")).Text;
                    string OwnerName = "", MailingAddress = "", LegalDesc = "", LandUseDesc = "", TaxDistrictName = "";
                    OwnerName = gc.Between(Bulkdata, "Name", "Mailing Address").Trim();
                    Mail_Address1 = gc.Between(Bulkdata, "Mailing Address", "City, State, Zip").Trim().Replace("\r\n", "");
                    Mail_Address2 = GlobalClass.After(Bulkdata, "City, State, Zip").Replace("\r\n", "").Trim();
                    MailingAddress = Mail_Address1 + " " + Mail_Address2;
                    string Bulkdata1 = driver.FindElement(By.XPath("//*[@id='Legal']/tbody")).Text;
                    LegalDesc = gc.Between(Bulkdata1, "Legal Description", "Land Use Description").Trim();
                    LandUseDesc = gc.Between(Bulkdata1, "Land Use Description", "Acres").Trim();
                    TaxDistrictName = GlobalClass.After(Bulkdata1, "Tax District Name").Trim();
                    try
                    {
                        IWebElement Iyearbuilt = driver.FindElement(By.XPath("//*[@id='Building']/tbody/tr[4]/td[2]/font"));
                        Yearbuilt = Iyearbuilt.Text;
                    }
                    catch { }

                    string propertydetails = OwnerName + "~" + Parcel_Location + "~" + MailingAddress + "~" + LegalDesc + "~" + LandUseDesc + "~" + TaxDistrictName + "~" + Yearbuilt;
                    gc.insert_date(orderNumber, parcelNumber, 1136, propertydetails, 1, DateTime.Now);


                    // Tax Payment History

                    try
                    {
                        driver.FindElement(By.LinkText("Payments List")).Click();
                        Thread.Sleep(1000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Payment List", driver, "OH", "Montgomery");
                    }
                    catch { }
                    try
                    {
                        IWebElement payhistory = driver.FindElement(By.XPath("//*[@id='Payments by Business Date']/tbody"));
                        IList<IWebElement> TRpayhistory = payhistory.FindElements(By.TagName("tr"));
                        IList<IWebElement> THpayhistory = payhistory.FindElements(By.TagName("th"));
                        IList<IWebElement> TDpayhistory;
                        foreach (IWebElement row in TRpayhistory)
                        {
                            TDpayhistory = row.FindElements(By.TagName("td"));

                            if (TDpayhistory.Count != 0 && !row.Text.Contains("Bus. Date") && row.Text.Trim() != "")
                            {
                                string payhistorydetails = TDpayhistory[0].Text + "~" + TDpayhistory[1].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1160, payhistorydetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }


                    // Distribution Details
                    try
                    {
                        driver.FindElement(By.LinkText("Levy Distribution")).SendKeys(Keys.Enter);
                        Thread.Sleep(1000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Levy Distribution", driver, "OH", "Montgomery");
                    }
                    catch { }
                    try
                    {//*[@id="datalet_div_2"]/table[2]/tbody
                        IWebElement Distribution = driver.FindElement(By.XPath("//*[@id='datalet_div_2']/table[2]/tbody"));
                        IList<IWebElement> TRDistribution = Distribution.FindElements(By.TagName("tr"));
                        IList<IWebElement> THDistribution = Distribution.FindElements(By.TagName("th"));
                        IList<IWebElement> TDDistribution;
                        foreach (IWebElement row in TRDistribution)
                        {
                            TDDistribution = row.FindElements(By.TagName("td"));

                            if (TDDistribution.Count != 0 && !row.Text.Contains("Year Levied") && !row.Text.Contains("TOTAL") && row.Text.Trim() != "")
                            {
                                string Distributiondetails = TDDistribution[0].Text + "~" + TDDistribution[1].Text + "~" + TDDistribution[2].Text + "~" + TDDistribution[3].Text + "~" + TDDistribution[4].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1161, Distributiondetails, 1, DateTime.Now);
                            }
                            if (TDDistribution.Count != 0 && !row.Text.Contains("Year Levied") && row.Text.Contains("TOTAL") && row.Text.Trim() != "")
                            {
                                string Distributiondetails = TDDistribution[0].Text + "~" + TDDistribution[1].Text + "~" + TDDistribution[2].Text + "~" + TDDistribution[3].Text + "~" + TDDistribution[4].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1161, Distributiondetails, 1, DateTime.Now);
                            }
                        }
                    }
                    catch { }


                    try
                    {
                        driver.FindElement(By.LinkText("Property Description")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Property Description", driver, "OH", "Montgomery");
                    }
                    catch { }

                    try
                    {
                        driver.FindElement(By.LinkText("Permits")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Permits", driver, "OH", "Montgomery");
                    }
                    catch { }

                    try
                    {
                        driver.FindElement(By.LinkText("Value History")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Value History", driver, "OH", "Montgomery");
                    }
                    catch { }

                    try
                    {
                        driver.FindElement(By.LinkText("Summary")).SendKeys(Keys.Enter);
                        Thread.Sleep(4000);
                    }
                    catch { }



                    // Assessment Details
                    int j = 0;
                    int Syear = DateTime.Now.Year;
                    int Smonth = DateTime.Now.Month;
                    int tax_year = 0;
                    if (Smonth >= 9)
                    {
                        tax_year = Syear;
                    }
                    else
                    {
                        tax_year = Syear - 1;
                    }
                    string[] stryear = { "" };
                    string percentage1 = "", percentage2 = "", land1 = "", land2 = "", Improvements1 = "", Improvements2 = "", CAUV1 = "", CAUV2 = "", Total1 = "", Total2 = "";
                    for (int i = 0; i < 3; i++)
                    {

                        IWebElement SerachCategory = driver.FindElement(By.Id("ddTaxYear"));
                        var selectElement1 = new SelectElement(SerachCategory);
                        selectElement1.SelectByText(tax_year.ToString());
                        Thread.Sleep(4000);
                        IWebElement Iassess = driver.FindElement(By.XPath("//*[@id='Values']/tbody"));
                        IList<IWebElement> TRIassess = Iassess.FindElements(By.TagName("tr"));
                        IList<IWebElement> THIassess = Iassess.FindElements(By.TagName("th"));
                        IList<IWebElement> TDIassess;
                        foreach (IWebElement row in TRIassess)
                        {
                            TDIassess = row.FindElements(By.TagName("td"));

                            if (TDIassess.Count != 0 && !row.Text.Contains("TENTATIVE") && row.Text.Trim() != "")
                            {
                                if (j == 1 || j == 14 || j == 27)
                                {
                                    percentage1 = TDIassess[2].Text;
                                    percentage2 = TDIassess[3].Text;
                                }
                                if (j == 3 || row.Text.Contains("Land"))
                                {
                                    land1 = TDIassess[2].Text;
                                    land2 = TDIassess[3].Text;
                                }
                                if (j == 5 || row.Text.Contains("Improvements"))
                                {
                                    Improvements1 = TDIassess[2].Text;
                                    Improvements2 = TDIassess[3].Text;
                                }
                                if (j == 7 || row.Text.Contains("CAUV"))
                                {
                                    CAUV1 = TDIassess[2].Text;
                                    CAUV2 = TDIassess[3].Text;
                                }
                                if (j == 9 || row.Text.Contains("Total"))
                                {
                                    Total1 = TDIassess[2].Text;
                                    Total2 = TDIassess[3].Text;
                                }
                            }
                            j++;
                        }

                        string assessmentdetails1 = tax_year + "~" + percentage1 + "~" + land1 + "~" + Improvements1 + "~" + CAUV1 + "~" + Total1;
                        gc.insert_date(orderNumber, parcelNumber, 1154, assessmentdetails1, 1, DateTime.Now);
                        string assessmentdetails2 = tax_year + "~" + percentage2 + "~" + land2 + "~" + Improvements2 + "~" + CAUV2 + "~" + Total2;
                        gc.insert_date(orderNumber, parcelNumber, 1154, assessmentdetails2, 1, DateTime.Now);





                        // Rollback Summary Table
                        string NonBusinessCredit = "", OwnerOccupancyCredit = "", Homestead = "", DaytonCredit = "", ReductionFactor = "";
                        try
                        {
                            gc.CreatePdf(orderNumber, parcelNumber, "Property Details" + tax_year, driver, "OH", "Montgomery");
                            IWebElement Rollback = driver.FindElement(By.XPath("//*[@id='Current Year Rollback Summary']/tbody"));
                            IList<IWebElement> TRRollback = Rollback.FindElements(By.TagName("tr"));
                            IList<IWebElement> THRollback = Rollback.FindElements(By.TagName("th"));
                            IList<IWebElement> TDRollback;
                            foreach (IWebElement row in TRRollback)
                            {
                                TDRollback = row.FindElements(By.TagName("td"));

                                if (TDRollback.Count != 0 && row.Text.Trim() != "")
                                {
                                    if (row.Text.Contains("Non Business Credit"))
                                    {
                                        NonBusinessCredit = TDRollback[1].Text;
                                    }
                                    if (row.Text.Contains("Owner Occupancy Credit"))
                                    {
                                        OwnerOccupancyCredit = TDRollback[1].Text;
                                    }
                                    if (row.Text.Contains("Homestead"))
                                    {
                                        Homestead = TDRollback[1].Text;
                                    }
                                    if (row.Text.Contains("City of Dayton Credit"))
                                    {
                                        DaytonCredit = TDRollback[1].Text;
                                    }
                                    if (row.Text.Contains("Reduction Factor"))
                                    {
                                        ReductionFactor = TDRollback[1].Text;
                                    }

                                }
                            }
                            string RollBackSummary = tax_year + "~" + NonBusinessCredit + "~" + OwnerOccupancyCredit + "~" + Homestead + "~" + DaytonCredit + "~" + ReductionFactor;
                            gc.insert_date(orderNumber, parcelNumber, 1155, RollBackSummary, 1, DateTime.Now);

                        }
                        catch { }

                        // Tax Summary Details
                        try
                        {
                            driver.FindElement(By.LinkText("Tax Summary")).SendKeys(Keys.Enter);
                            Thread.Sleep(4000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Tax Summary Details" + tax_year, driver, "OH", "Montgomery");
                        }
                        catch { }
                        // First Half
                        string Installment1 = "";
                        //*[@id="<center>First Half Taxes</center>"]/tbody
                        //*[@id='datalet_div_1']/table[1]/tbody/tr/td/font/center
                        try
                        {
                            IWebElement TaxInstallment = driver.FindElement(By.XPath("//*[@id='datalet_div_2']/table[1]/tbody/tr/td/font/center"));
                            Installment1 = TaxInstallment.Text.Replace("Taxes", "").Trim();
                            IWebElement Taxsummary = driver.FindElement(By.XPath("//*[@id='<center>First Half Taxes</center>']/tbody"));
                            IList<IWebElement> TRTaxsummary = Taxsummary.FindElements(By.TagName("tr"));
                            IList<IWebElement> THTaxsummary = Taxsummary.FindElements(By.TagName("th"));
                            IList<IWebElement> TDTaxsummary;
                            foreach (IWebElement row in TRTaxsummary)
                            {
                                TDTaxsummary = row.FindElements(By.TagName("td"));

                                if (TDTaxsummary.Count != 0 && !row.Text.Contains("Adjustments") && row.Text.Trim() != "" && row.Text.Contains("REAL"))
                                {
                                    string Taxsummarydetails = Installment1 + "~" + tax_year + "~" + TDTaxsummary[1].Text + "~" + TDTaxsummary[2].Text + "~" + TDTaxsummary[3].Text + "~" + TDTaxsummary[4].Text + "~" + TDTaxsummary[5].Text;
                                    gc.insert_date(orderNumber, parcelNumber, 1159, Taxsummarydetails, 1, DateTime.Now);

                                }
                                if (TDTaxsummary.Count != 0 && !row.Text.Contains("Adjustments") && row.Text.Trim() != "" && !row.Text.Contains("REAL") && !row.Text.Contains("Total"))
                                {
                                    string Taxsummarydetails1 = Installment1 + "~" + tax_year + "~" + TDTaxsummary[1].Text + "~" + TDTaxsummary[2].Text + "~" + TDTaxsummary[3].Text + "~" + TDTaxsummary[4].Text + "~" + TDTaxsummary[5].Text;
                                    gc.insert_date(orderNumber, parcelNumber, 1159, Taxsummarydetails1, 1, DateTime.Now);

                                }
                                if (TDTaxsummary.Count != 0 && !row.Text.Contains("Adjustments") && row.Text.Trim() != "" && !row.Text.Contains("REAL") && row.Text.Contains("Total"))
                                {
                                    string Taxsummarydetails11 = Installment1 + "~" + tax_year + "~" + TDTaxsummary[0].Text + "~" + TDTaxsummary[2].Text + "~" + TDTaxsummary[3].Text + "~" + TDTaxsummary[4].Text + "~" + TDTaxsummary[5].Text;
                                    gc.insert_date(orderNumber, parcelNumber, 1159, Taxsummarydetails11, 1, DateTime.Now);

                                }
                            }
                        }
                        catch { }
                        // Second Half
                        string Installment2 = "";
                        try
                        {
                            IWebElement TaxInstallment2 = driver.FindElement(By.XPath("//*[@id='datalet_div_3']/table[1]/tbody/tr/td/font/center"));
                            Installment2 = TaxInstallment2.Text.Replace("Taxes", "").Trim();
                            IWebElement Taxsummary2 = driver.FindElement(By.XPath("//*[@id='<center>Second Half Taxes</center>']/tbody"));
                            IList<IWebElement> TRTaxsummary2 = Taxsummary2.FindElements(By.TagName("tr"));
                            IList<IWebElement> THTaxsummary2 = Taxsummary2.FindElements(By.TagName("th"));
                            IList<IWebElement> TDTaxsummary2;
                            foreach (IWebElement row2 in TRTaxsummary2)
                            {
                                TDTaxsummary2 = row2.FindElements(By.TagName("td"));

                                if (TDTaxsummary2.Count != 0 && !row2.Text.Contains("Adjustments") && row2.Text.Trim() != "" && row2.Text.Contains("REAL"))
                                {
                                    string Taxsummarydetails2 = Installment2 + "~" + tax_year + "~" + TDTaxsummary2[1].Text + "~" + TDTaxsummary2[2].Text + "~" + TDTaxsummary2[3].Text + "~" + TDTaxsummary2[4].Text + "~" + TDTaxsummary2[5].Text;
                                    gc.insert_date(orderNumber, parcelNumber, 1159, Taxsummarydetails2, 1, DateTime.Now);

                                }
                                if (TDTaxsummary2.Count != 0 && !row2.Text.Contains("Adjustments") && row2.Text.Trim() != "" && !row2.Text.Contains("REAL") && !row2.Text.Contains("Total"))
                                {
                                    string Taxsummarydetails3 = Installment2 + "~" + tax_year + "~" + TDTaxsummary2[1].Text + "~" + TDTaxsummary2[2].Text + "~" + TDTaxsummary2[3].Text + "~" + TDTaxsummary2[4].Text + "~" + TDTaxsummary2[5].Text;
                                    gc.insert_date(orderNumber, parcelNumber, 1159, Taxsummarydetails3, 1, DateTime.Now);

                                }
                                if (TDTaxsummary2.Count != 0 && !row2.Text.Contains("Adjustments") && row2.Text.Trim() != "" && !row2.Text.Contains("REAL") && row2.Text.Contains("Total"))
                                {
                                    string Taxsummarydetails4 = Installment2 + "~" + tax_year + "~" + TDTaxsummary2[0].Text + "~" + TDTaxsummary2[2].Text + "~" + TDTaxsummary2[3].Text + "~" + TDTaxsummary2[4].Text + "~" + TDTaxsummary2[5].Text;
                                    gc.insert_date(orderNumber, parcelNumber, 1159, Taxsummarydetails4, 1, DateTime.Now);

                                }
                            }
                        }
                        catch { }
                        // Grand Total Information
                        try
                        {
                            string Installment3 = "";
                            IWebElement TaxInstallment3 = driver.FindElement(By.XPath("//*[@id='datalet_div_9']/table[1]/tbody/tr/td"));
                            Installment3 = TaxInstallment3.Text.Trim();
                            IWebElement Taxsummary3 = driver.FindElement(By.XPath("//*[@id='<center>Grand Totals</center>']/tbody"));
                            IList<IWebElement> TRTaxsummary3 = Taxsummary3.FindElements(By.TagName("tr"));
                            IList<IWebElement> THTaxsummary3 = Taxsummary3.FindElements(By.TagName("th"));
                            IList<IWebElement> TDTaxsummary3;
                            foreach (IWebElement row3 in TRTaxsummary3)
                            {
                                TDTaxsummary3 = row3.FindElements(By.TagName("td"));

                                if (TDTaxsummary3.Count != 0 && !row3.Text.Contains("Adjustments") && row3.Text.Trim() != "")
                                {
                                    string Taxsummarydetails5 = Installment3 + "~" + tax_year + "~" + TDTaxsummary3[0].Text + "~" + TDTaxsummary3[1].Text + "~" + TDTaxsummary3[2].Text + "~" + TDTaxsummary3[3].Text + "~" + TDTaxsummary3[4].Text;
                                    gc.insert_date(orderNumber, parcelNumber, 1159, Taxsummarydetails5, 1, DateTime.Now);

                                }

                            }

                        }
                        catch { }


                        // Taxing Authority

                        try
                        {
                            IWebElement Tax_Auth = driver.FindElement(By.XPath("//*[@id='datalet_div_10']/table[2]/tbody"));
                            TaxAuthority = Tax_Auth.Text.Replace("TAX PAYMENTS MAY BE MAILED TO", "").Trim();
                        }
                        catch { }

                        //   New Levies
                        try
                        {
                            driver.FindElement(By.LinkText("New Levies")).SendKeys(Keys.Enter);
                            Thread.Sleep(2000);
                            gc.CreatePdf(orderNumber, parcelNumber, "New Levies" + tax_year, driver, "OH", "Montgomery");
                        }
                        catch { }

                        //   Special Assessments
                        try
                        {
                            driver.FindElement(By.LinkText("Special Assessments")).SendKeys(Keys.Enter);
                            Thread.Sleep(4000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Special Assessments" + tax_year, driver, "OH", "Montgomery");
                        }
                        catch { }
                        try
                        {
                            IWebElement SplAssess = driver.FindElement(By.XPath("//*[@id='datalet_div_2']/table[2]/tbody"));
                            IList<IWebElement> TRSplAssess = SplAssess.FindElements(By.TagName("tr"));
                            IList<IWebElement> THSplAssess = SplAssess.FindElements(By.TagName("th"));
                            IList<IWebElement> TDSplAssess;
                            foreach (IWebElement row in TRSplAssess)
                            {
                                TDSplAssess = row.FindElements(By.TagName("td"));

                                if (TDSplAssess.Count != 0 && !row.Text.Contains("Total Charge") && row.Text.Trim() != "")
                                {
                                    string SplAssessdetails = TDSplAssess[0].Text + "~" + TDSplAssess[1].Text + "~" + TDSplAssess[2].Text + "~" + TDSplAssess[3].Text + "~" + TDSplAssess[4].Text + "~" + TDSplAssess[5].Text + "~" + TDSplAssess[6].Text;
                                    gc.insert_date(orderNumber, parcelNumber, 1162, SplAssessdetails, 1, DateTime.Now);
                                }
                            }
                        }
                        catch { }

                        //   Tax Without Payments 
                        try
                        {
                            driver.FindElement(By.LinkText("Tax Detail")).SendKeys(Keys.Enter);
                            Thread.Sleep(4000);
                            gc.CreatePdf(orderNumber, parcelNumber, "Tax Info Details" + tax_year, driver, "OH", "Montgomery");
                        }
                        catch { }
                        try
                        {
                            IWebElement TaxWPay = driver.FindElement(By.XPath("//*[@id='Taxes for Selected Year (Without Payments)']/tbody"));
                            IList<IWebElement> TRTaxWPay = TaxWPay.FindElements(By.TagName("tr"));
                            IList<IWebElement> THTaxWPay = TaxWPay.FindElements(By.TagName("th"));
                            IList<IWebElement> TDTaxWPay;
                            foreach (IWebElement row in TRTaxWPay)
                            {
                                TDTaxWPay = row.FindElements(By.TagName("td"));

                                if (TDTaxWPay.Count != 0 && !row.Text.Contains("Total") && row.Text.Trim() != "")
                                {
                                    string TaxWithoutPay = tax_year + "~" + TDTaxWPay[0].Text + "~" + TDTaxWPay[1].Text + "~" + TDTaxWPay[2].Text + "~" + TDTaxWPay[3].Text + "~" + TDTaxWPay[4].Text + "~" + TDTaxWPay[5].Text + "~" + TDTaxWPay[6].Text;
                                    gc.insert_date(orderNumber, parcelNumber, 1163, TaxWithoutPay, 1, DateTime.Now);
                                }
                            }
                        }
                        catch { }

                        //   Tax Information
                        string strCharge1 = "", strCharge2 = "", strCharge3 = "";
                        string strtaxamount1 = "", strtaxamount2 = "", strtaxamount3 = "";
                        string strpayment1 = "", strpayment2 = "", strpayment3 = "";
                        string strpenalty1 = "", strpenalty2 = "", strpenalty3 = "";
                        string strInterest1 = "", strInterest2 = "", strInterest3 = "";
                        string strTotalDue1 = "", strTotalDue2 = "", strTotalDue3 = "", InformationComments = "";
                        string[] InterestSplit;
                        string[] PenaltySplit;
                        try
                        {
                            IWebElement TaxInformation = driver.FindElement(By.XPath("//*[@id='Current Taxes Due']/tbody"));
                            IList<IWebElement> TRTaxInformation = TaxInformation.FindElements(By.TagName("tr"));
                            IList<IWebElement> THTaxInformation = TaxInformation.FindElements(By.TagName("th"));
                            IList<IWebElement> TDTaxInformation;
                            foreach (IWebElement row in TRTaxInformation)
                            {
                                TDTaxInformation = row.FindElements(By.TagName("td"));


                                if (TDTaxInformation.Count != 0 && !row.Text.Contains("Unpaid Balance") && row.Text.Trim() != "")
                                {
                                    string[] ChargesSplit = TDTaxInformation[0].Text.Split('\r');
                                    try
                                    {
                                        strCharge1 = ChargesSplit[0].Replace("\n", "");
                                        strCharge2 = ChargesSplit[1].Replace("\n", "");
                                        strCharge3 = ChargesSplit[2].Replace("\n", "");
                                    }
                                    catch { }

                                    string[] TaxAmountSplit = TDTaxInformation[1].Text.Split('\r');
                                    try
                                    {
                                        strtaxamount1 = TaxAmountSplit[0].Replace("\n", "");
                                        strtaxamount2 = TaxAmountSplit[1].Replace("\n", "");
                                        strtaxamount3 = TaxAmountSplit[2].Replace("\n", "");
                                    }
                                    catch { }

                                    string[] PaymentSplit = TDTaxInformation[2].Text.Split('\r');
                                    try
                                    {
                                        strpayment1 = PaymentSplit[0].Replace("\n", "");
                                        strpayment2 = PaymentSplit[1].Replace("\n", "");
                                        strpayment3 = PaymentSplit[2].Replace("\n", "");
                                    }
                                    catch { }

                                    PenaltySplit = TDTaxInformation[3].Text.Split('\r');
                                    try
                                    {
                                        strpenalty1 = PenaltySplit[0].Replace("\n", "");
                                        strpenalty2 = PenaltySplit[1].Replace("\n", "");
                                        strpenalty3 = PenaltySplit[2].Replace("\n", "");
                                    }
                                    catch { }

                                    InterestSplit = TDTaxInformation[4].Text.Split('\r');
                                    try
                                    {
                                        strInterest2 = InterestSplit[0].Replace("\n", "");
                                        strInterest3 = InterestSplit[1].Replace("\n", "");
                                        //strInterest3 = InterestSplit[2].Replace("\n", "");
                                    }
                                    catch { }

                                    string[] TotalDueSplit = TDTaxInformation[5].Text.Split('\r');
                                    try
                                    {
                                        strTotalDue1 = TotalDueSplit[0].Replace("\n", "");
                                        strTotalDue2 = TotalDueSplit[1].Replace("\n", "");
                                        strTotalDue3 = TotalDueSplit[2].Replace("\n", "");
                                    }
                                    catch { }


                                    string TaxInfodetails1 = tax_year + "~" + strCharge1 + "~" + strtaxamount1 + "~" + strpayment1 + "~" + strpenalty1 + "~" + strInterest1 + "~" + strTotalDue1 + "~" + TaxAuthority;
                                    gc.insert_date(orderNumber, parcelNumber, 1166, TaxInfodetails1, 1, DateTime.Now);

                                    string TaxInfodetails2 = tax_year + "~" + strCharge2 + "~" + strtaxamount2 + "~" + strpayment2 + "~" + strpenalty2 + "~" + strInterest2 + "~" + strTotalDue2 + "~" + TaxAuthority;
                                    gc.insert_date(orderNumber, parcelNumber, 1166, TaxInfodetails2, 1, DateTime.Now);

                                    string TaxInfodetails3 = tax_year + "~" + strCharge3 + "~" + strtaxamount3 + "~" + strpayment3 + "~" + strpenalty3 + "~" + strInterest3 + "~" + strTotalDue3 + "~" + TaxAuthority;
                                    gc.insert_date(orderNumber, parcelNumber, 1166, TaxInfodetails3, 1, DateTime.Now);

                                }
                            }
                            if (!strpenalty1.Contains("$0.00") || !strpenalty2.Contains("$0.00") || !strpenalty3.Contains("$0.00") || !strInterest2.Contains("$0.00") || !strInterest3.Contains("$0.00"))
                            {
                                InformationComments = "Taxes are Delinquent";
                                string alertmessage = tax_year + "~" + InformationComments;
                                gc.insert_date(orderNumber, parcelNumber, 1182, alertmessage, 1, DateTime.Now);
                            }


                        }
                        catch { }

                        tax_year--;

                        try
                        {
                            driver.FindElement(By.LinkText("Summary")).SendKeys(Keys.Enter);
                            Thread.Sleep(4000);
                        }
                        catch { }
                    }

                    // Delinquent Taxes
                    try
                    {
                        driver.FindElement(By.LinkText("Tax Summary")).SendKeys(Keys.Enter);
                        Thread.Sleep(4000);
                        //gc.CreatePdf(orderNumber, parcelNumber, "Tax Summary Details" + tax_year, driver, "OH", "Montgomery");
                    }
                    catch { }
                    try
                    {
                        string Installment4 = "";
                        IWebElement TaxInstallment4 = driver.FindElement(By.XPath("//*[@id='datalet_div_4']/table[1]/tbody"));
                        Installment4 = TaxInstallment4.Text.Replace("Taxes", "").Trim();
                        IWebElement Taxsummary4 = driver.FindElement(By.XPath("//*[@id='<center>Delinquent Taxes</center>']/tbody"));
                        IList<IWebElement> TRTaxsummary4 = Taxsummary4.FindElements(By.TagName("tr"));
                        IList<IWebElement> THTaxsummary4 = Taxsummary4.FindElements(By.TagName("th"));
                        IList<IWebElement> TDTaxsummary4;
                        foreach (IWebElement row4 in TRTaxsummary4)
                        {
                            TDTaxsummary4 = row4.FindElements(By.TagName("td"));

                            if (TDTaxsummary4.Count != 0 && !row4.Text.Contains("Adjustments") && !row4.Text.Contains("Total") && row4.Text.Trim() != "")
                            {
                                string Taxsummarydetails6 = Installment4 + "~" + TDTaxsummary4[0].Text + "~" + TDTaxsummary4[1].Text + "~" + TDTaxsummary4[2].Text + "~" + TDTaxsummary4[3].Text + "~" + TDTaxsummary4[4].Text + "~" + TDTaxsummary4[5].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1159, Taxsummarydetails6, 1, DateTime.Now);

                            }
                            if (TDTaxsummary4.Count != 0 && !row4.Text.Contains("Adjustments") && row4.Text.Contains("Total") && row4.Text.Trim() != "")
                            {
                                string Taxsummarydetails6 = Installment4 + "~" + "" + "~" + TDTaxsummary4[0].Text + "~" + TDTaxsummary4[1].Text + "~" + TDTaxsummary4[2].Text + "~" + TDTaxsummary4[3].Text + "~" + TDTaxsummary4[4].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1159, Taxsummarydetails6, 1, DateTime.Now);

                            }


                        }
                    }
                    catch { }




                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "OH", "Montgomery", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "OH", "Montgomery");
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