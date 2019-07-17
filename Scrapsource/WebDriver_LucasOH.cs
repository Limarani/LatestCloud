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
    public class WebDriver_LucasOH
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlParameter[] mParam;

        public string FTP_LucasOH(string houseno, string Direction, string sname, string stype, string unitno, string parcelNumber, string ownername, string searchType, string orderNumber)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string multi = "", TaxAuthority = "";
            string Parcelno = "", Ownername = "", parcellocation = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new PhantomJSDriver();
            // driver = new ChromeDriver();
            using (driver = new PhantomJSDriver()) //ChromeDriver
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("http://icare.co.lucas.oh.us/LucasCare/search/commonsearch.aspx?mode=address");
                    Thread.Sleep(1000);

                    if (searchType == "titleflex")
                    {
                        string address = "";
                        if (Direction != "")
                        {
                            address = houseno + " " + Direction + " " + sname + " " + stype + " " + unitno;
                        }
                        if (Direction == "")
                        {
                            address = houseno + " " + sname + " " + stype + " " + unitno;
                        }

                        gc.TitleFlexSearch(orderNumber, "", "", address.Trim(), "OH", "Lucas");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_LucasOH"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                        parcelNumber = parcelNumber.Replace("-", " ");
                    }
                    if (searchType == "address")
                    {
                        try
                        {
                            driver.FindElement(By.LinkText("Address")).SendKeys(Keys.Enter);
                            Thread.Sleep(1000);
                        }
                        catch { }

                        driver.FindElement(By.Id("inpNumber")).SendKeys(houseno);
                        driver.FindElement(By.Id("inpStreet")).SendKeys(sname.Trim());
                        gc.CreatePdf_WOP(orderNumber, "Address Search", driver, "OH", "Lucas");
                        driver.FindElement(By.Id("btSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(1000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search Result", driver, "OH", "Lucas");
                        //Multiparcel
                        try
                        {
                            string Assess = "", Landused = "", Totalval = "";
                            IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='searchResults']/tbody"));
                            IList<IWebElement> TRmultiaddress = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> THmultiaddress = multiaddress.FindElements(By.TagName("th"));
                            IList<IWebElement> TDmultiaddress;
                            if (TRmultiaddress.Count > 28)
                            {
                                HttpContext.Current.Session["multiParcel_Lucas_Maximum"] = "Maimum";
                                driver.Quit();
                                return "Maximum";
                            }
                            if (TRmultiaddress.Count > 3)
                            {
                                foreach (IWebElement row in TRmultiaddress)
                                {
                                    TDmultiaddress = row.FindElements(By.TagName("td"));
                                    if (!row.Text.Contains("Parcel ID") && row.Text.Trim() != "" && TDmultiaddress.Count != 2)
                                    {
                                        try
                                        {
                                            Parcelno = TDmultiaddress[1].Text;
                                            Assess = TDmultiaddress[2].Text;
                                            Ownername = TDmultiaddress[3].Text;
                                            parcellocation = TDmultiaddress[4].Text;
                                            Landused = TDmultiaddress[5].Text;
                                            Totalval = TDmultiaddress[6].Text;
                                            //Assessor~Owner Name~Address~Land Use~Total Value
                                            string Multi = Assess + "~" + Ownername + "~" + parcellocation + "~" + Landused + "~" + Totalval;
                                            gc.insert_date(orderNumber, Parcelno, 1709, Multi, 1, DateTime.Now);
                                        }
                                        catch { }
                                    }
                                }
                                HttpContext.Current.Session["multiParcel_Lucas"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (TRmultiaddress.Count <= 3)
                            {
                                //TDmultiaddress[0].Click();
                                driver.FindElement(By.XPath("//*[@id='searchResults']/tbody/tr[3]/td[1]/table/tbody/tr/td[2]/font")).Click();
                                Thread.Sleep(1000);
                            }
                        }
                        catch { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='frmMain']/table/tbody/tr/td/div/div/table[2]/tbody/tr/td/table/tbody/tr[3]/td/center/table[1]/tbody/tr[1]/td/div/p")).Text;
                            if (nodata.Contains("Your search did not find any records."))
                            {
                                HttpContext.Current.Session["Nodata_LucasOH"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    if (searchType == "parcel")
                    {
                        try
                        {
                            driver.FindElement(By.LinkText("Parcel Number")).SendKeys(Keys.Enter);
                            Thread.Sleep(1000);
                        }
                        catch { }
                        driver.FindElement(By.Id("inpParid")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search", driver, "OH", "Lucas");
                        driver.FindElement(By.Id("btSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(1000);
                        gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Result", driver, "OH", "Lucas");
                        try
                        {
                            IWebElement Iclick = driver.FindElement(By.XPath("//*[@id='searchResults']/tbody/tr[3]/td[1]/table/tbody"));
                            Iclick.Click();
                            Thread.Sleep(2000);
                        }
                        catch { }

                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='frmMain']/table/tbody/tr/td/div/div/table[2]/tbody/tr/td/table/tbody/tr[3]/td/center/table[1]/tbody/tr[1]/td/div/p")).Text;
                            if (nodata.Contains("Your search did not find any records."))
                            {
                                HttpContext.Current.Session["Nodata_LucasOH"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }
                    if (searchType == "ownername")
                    {
                        try
                        {
                            driver.FindElement(By.LinkText("Owner")).SendKeys(Keys.Enter);
                            Thread.Sleep(1000);
                        }
                        catch { }
                        driver.FindElement(By.Id("inpOwner")).SendKeys(ownername.Trim());
                        gc.CreatePdf_WOP(orderNumber, "Ownername Search", driver, "OH", "Lucas");
                        driver.FindElement(By.Id("btSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(1000);
                        gc.CreatePdf_WOP(orderNumber, "Ownername Search Result", driver, "OH", "Lucas");
                        //Multiparcel
                        try
                        {
                            string Assess = "", Landused = "", Totalval = "";
                            IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='searchResults']/tbody"));
                            IList<IWebElement> TRmultiaddress = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> THmultiaddress = multiaddress.FindElements(By.TagName("th"));
                            IList<IWebElement> TDmultiaddress;
                            if (TRmultiaddress.Count > 28)
                            {
                                HttpContext.Current.Session["multiParcel_Lucas_Maximum"] = "Maimum";
                                driver.Quit();
                                return "Maximum";
                            }
                            if (TRmultiaddress.Count > 3)
                            {
                                foreach (IWebElement row in TRmultiaddress)
                                {
                                    TDmultiaddress = row.FindElements(By.TagName("td"));
                                    if (!row.Text.Contains("Parcel ID") && row.Text.Trim() != "" && TDmultiaddress.Count != 2)
                                    {
                                        try
                                        {
                                            Parcelno = TDmultiaddress[1].Text;
                                            Assess = TDmultiaddress[2].Text;
                                            Ownername = TDmultiaddress[3].Text;
                                            parcellocation = TDmultiaddress[4].Text;
                                            Landused = TDmultiaddress[5].Text;
                                            Totalval = TDmultiaddress[6].Text;
                                            //Assessor~Owner Name~Address~Land Use~Total Value
                                            string Multi = Assess + "~" + Ownername + "~" + parcellocation + "~" + Landused + "~" + Totalval;
                                            gc.insert_date(orderNumber, Parcelno, 1709, Multi, 1, DateTime.Now);
                                        }
                                        catch { }
                                    }
                                }
                                HttpContext.Current.Session["multiParcel_Lucas"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (TRmultiaddress.Count <= 3)
                            {
                                //TDmultiaddress[0].Click();
                                driver.FindElement(By.XPath("//*[@id='searchResults']/tbody/tr[3]/td[1]/table/tbody/tr/td[2]/font")).Click();
                                Thread.Sleep(1000);
                            }
                        }
                        catch { }

                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='frmMain']/table/tbody/tr/td/div/div/table[2]/tbody/tr/td/table/tbody/tr[3]/td/center/table[1]/tbody/tr[1]/td/div/p")).Text;
                            if (nodata.Contains("Your search did not find any records."))
                            {
                                HttpContext.Current.Session["Nodata_LucasOH"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    // Property Details
                    string Parcelid = "", Assessor = "", Owner = "", Propertyadd = "", Taxdistrict = "", Class = "", Landuse = "", Marketarea = "", Zoningcode = "", Zoningdes = "", Legaldes = "", Censustract = "", Yearbuilt = "", Acres = "";

                    string propertydata = driver.FindElement(By.XPath("//*[@id='datalet_header_row']/td/table/tbody")).Text;

                    Parcelid = gc.Between(propertydata, "PARCEL ID:", "ASSESSOR#:").Trim();
                    Assessor = gc.Between(propertydata, "ASSESSOR#:", "MARKET AREA:").Trim();

                    string Bulkdata = driver.FindElement(By.Id("Summary - General")).Text;

                    Owner = gc.Between(Bulkdata, "Owner", "Property Address").Trim();
                    Propertyadd = gc.Between(Bulkdata, "Property Address", "Mailing Address").Trim();
                    Taxdistrict = gc.Between(Bulkdata, "Tax District", "Class").Trim();
                    Class = gc.Between(Bulkdata, "Class", "Land Use").Trim();
                    Landuse = gc.Between(Bulkdata, "Land Use", "Market Area").Trim();
                    Marketarea = gc.Between(Bulkdata, "Market Area", "Zoning Code").Replace("- Click here to view map", " ").Trim();
                    Zoningcode = gc.Between(Bulkdata, "Zoning Code", "Zoning Description").Replace("- Click here for zoning details", " ").Trim();
                    Zoningdes = gc.Between(Bulkdata, "Zoning Description", "Water and Sewer").Trim();
                    Legaldes = gc.Between(Bulkdata, "Legal Desc.", "Certified Delinquent Year").Trim();
                    Censustract = GlobalClass.After(Bulkdata, "Census Tract").Trim();
                    //gc.CreatePdf(orderNumber, Parcelid, "Summary", driver, "OH", "Lucas");

                    //Acres Click  Land click
                    driver.FindElement(By.XPath("//*[@id='sidemenu']/li[8]/a")).Click();
                    Thread.Sleep(2000);
                    Acres = driver.FindElement(By.XPath("//*[@id='Land Line Details']/tbody/tr[5]/td[2]")).Text.Trim();
                    gc.CreatePdf(orderNumber, Parcelid, "Acres", driver, "OH", "Lucas");

                    //Yearbuilt Click
                    driver.FindElement(By.XPath("//*[@id='sidemenu']/li[6]/a")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcelid, "Yearbuilt", driver, "OH", "Lucas");
                    try
                    {
                        Yearbuilt = driver.FindElement(By.XPath("//*[@id='Residential Building Information']/tbody/tr[9]/td[2]")).Text.Trim();
                    }
                    catch { }

                    string propertydetails = Assessor + "~" + Owner + "~" + Propertyadd + "~" + Taxdistrict + "~" + Class + "~" + Landuse + "~" + Marketarea + "~" + Zoningcode + "~" + Zoningdes + "~" + Legaldes + "~" + Censustract + "~" + Acres + "~" + Yearbuilt;
                    gc.insert_date(orderNumber, Parcelid, 1694, propertydetails, 1, DateTime.Now);

                    //Summary - Values Details Table:
                    driver.FindElement(By.XPath("//*[@id='sidemenu']/li[1]/a")).Click();
                    Thread.Sleep(2000);

                    IWebElement Summaryvalue = driver.FindElement(By.Id("Summary - Values"));
                    IList<IWebElement> TRSummaryvalue = Summaryvalue.FindElements(By.TagName("tr"));
                    IList<IWebElement> THSummaryvalue = Summaryvalue.FindElements(By.TagName("th"));
                    IList<IWebElement> TDSummaryvalue;
                    foreach (IWebElement row in TRSummaryvalue)
                    {
                        TDSummaryvalue = row.FindElements(By.TagName("td"));

                        if (TDSummaryvalue.Count != 0 && !row.Text.Contains("35% Values") && row.Text.Trim() != "")
                        {
                            string Summaryvaluedetails = TDSummaryvalue[0].Text + "~" + TDSummaryvalue[1].Text + "~" + TDSummaryvalue[2].Text + "~" + TDSummaryvalue[3].Text + "~" + TDSummaryvalue[4].Text;
                            gc.insert_date(orderNumber, Parcelid, 1695, Summaryvaluedetails, 1, DateTime.Now);
                        }
                    }
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    //Tax Credits Details
                    IWebElement TaxCredit = driver.FindElement(By.Id("Tax Credits"));
                    IList<IWebElement> TRTaxCredit = TaxCredit.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDTaxCredit;
                    foreach (IWebElement row1 in TRTaxCredit)
                    {
                        TDTaxCredit = row1.FindElements(By.TagName("td"));

                        if (TDTaxCredit.Count != 0 && row1.Text.Trim() != "")
                        {
                            string TaxCreditvaluedetails = TDTaxCredit[0].Text + "~" + TDTaxCredit[1].Text;
                            gc.insert_date(orderNumber, Parcelid, 1696, TaxCreditvaluedetails, 1, DateTime.Now);
                        }
                    }

                    //Tranfers and Values Pdf
                    //By Fund and By Fund & Levy Pdf

                    driver.FindElement(By.XPath("//*[@id='sidemenu']/li[4]/a")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcelid, "Transfers", driver, "OH", "Lucas");


                    //Value Change History Details Table
                    driver.FindElement(By.XPath("//*[@id='sidemenu']/li[5]/a")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcelid, "Values", driver, "OH", "Lucas");

                    //*******************************//
                    //Value Change History (35%) - To 2014
                    string valuehistitle = driver.FindElement(By.XPath("//*[@id='datalet_div_5']/table[1]/tbody/tr/td/font")).Text.Trim();

                    IWebElement Valuechange1 = driver.FindElement(By.Id("Value Change History (35%) - To 2014"));
                    IList<IWebElement> TRValuechange1 = Valuechange1.FindElements(By.TagName("tr"));
                    IList<IWebElement> THValuechange1 = Valuechange1.FindElements(By.TagName("th"));
                    IList<IWebElement> TDValuechange1;
                    foreach (IWebElement rowValuechange1 in TRValuechange1)
                    {
                        TDValuechange1 = rowValuechange1.FindElements(By.TagName("td"));

                        if (TDValuechange1.Count == 7 && TDValuechange1.Count != 0 && !rowValuechange1.Text.Contains("Land") && Valuechange1.Text.Trim() != "")
                        {
                            string valuedetails1 = valuehistitle + "~" + TDValuechange1[0].Text + "~" + TDValuechange1[1].Text + "~" + TDValuechange1[2].Text + "~" + TDValuechange1[3].Text + "~" + TDValuechange1[4].Text + "~" + TDValuechange1[5].Text + "~" + TDValuechange1[6].Text;
                            gc.insert_date(orderNumber, Parcelid, 1536, valuedetails1, 1, DateTime.Now);
                        }
                    }
                    //Value Change History (35%) - Prior to 2014
                    string valuehistitle1 = driver.FindElement(By.XPath("//*[@id='datalet_div_7']/table[1]/tbody/tr/td/font")).Text.Trim();


                    IWebElement Valuechange2 = driver.FindElement(By.Id("Value Change History (35%) - Prior to 2014"));
                    IList<IWebElement> TRValuechange2 = Valuechange2.FindElements(By.TagName("tr"));
                    IList<IWebElement> THValuechange2 = Valuechange2.FindElements(By.TagName("th"));
                    IList<IWebElement> TDValuechange2;
                    foreach (IWebElement rowValuechange2 in TRValuechange2)
                    {
                        TDValuechange2 = rowValuechange2.FindElements(By.TagName("td"));

                        if (TDValuechange2.Count == 7 && TDValuechange2.Count != 0 && !rowValuechange2.Text.Contains("Land") && Valuechange2.Text.Trim() != "")
                        {
                            string valuedetails2 = valuehistitle1 + "~" + TDValuechange2[0].Text + "~" + TDValuechange2[1].Text + "~" + TDValuechange2[2].Text + "~" + TDValuechange2[3].Text + "~" + TDValuechange2[4].Text + "~" + TDValuechange2[5].Text + "~" + TDValuechange2[6].Text;
                            gc.insert_date(orderNumber, Parcelid, 1536, valuedetails2, 1, DateTime.Now);
                        }
                    }

                    //CAUV / Forest / Recoupment Details
                    driver.FindElement(By.XPath("//*[@id='sidemenu']/li[11]/a")).Click();
                    Thread.Sleep(2000);

                    IWebElement TaxCurrent1 = driver.FindElement(By.Id("CAUV / Forest / Recoupment"));
                    IList<IWebElement> TRTaxCurrent1 = TaxCurrent1.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDTaxCurrent1;
                    foreach (IWebElement row2 in TRTaxCurrent1)
                    {
                        TDTaxCurrent1 = row2.FindElements(By.TagName("td"));

                        if (TDTaxCurrent1.Count != 0 && row2.Text.Trim() != "")
                        {
                            string CAUVvaluedetails = TDTaxCurrent1[0].Text + "~" + TDTaxCurrent1[1].Text;
                            gc.insert_date(orderNumber, Parcelid, 1697, CAUVvaluedetails, 1, DateTime.Now);
                        }
                    }
                    gc.CreatePdf(orderNumber, Parcelid, "CurrentTax1", driver, "OH", "Lucas");
                    //Current Taxes Details Table
                    IWebElement TaxCurrent2 = driver.FindElement(By.Id("Current Taxes"));
                    IList<IWebElement> TRTaxCurrent2 = TaxCurrent2.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDTaxCurrent2;
                    foreach (IWebElement row3 in TRTaxCurrent2)
                    {
                        TDTaxCurrent2 = row3.FindElements(By.TagName("td"));

                        if (TDTaxCurrent2.Count != 0 && row3.Text.Trim() != "" && !row3.Text.Contains("1st Half 2nd Half") && !row3.Text.Contains("Tax Year"))
                        {
                            string Currenttaxvaluedetails = TDTaxCurrent2[0].Text.Replace(":", "").Trim().Replace("(see note)", "").Trim() + "~" + TDTaxCurrent2[1].Text + "~" + TDTaxCurrent2[2].Text + "~" + TDTaxCurrent2[3].Text;
                            gc.insert_date(orderNumber, Parcelid, 1698, Currenttaxvaluedetails, 1, DateTime.Now);
                        }
                    }
                    //  gc.CreatePdf(orderNumber, parcelNumber, "CurrentTax2", driver, "OH", "Lucas");

                    //Tax Distribution Details
                    driver.FindElement(By.XPath("//*[@id='sidemenu']/li[12]/a")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcelid, "Tax Distribution", driver, "OH", "Lucas");

                    IWebElement TaxDistribution = driver.FindElement(By.Id("Distribution by Authority"));
                    IList<IWebElement> TRTaxDistribution = TaxDistribution.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDTaxDistribution;
                    foreach (IWebElement Distribution in TRTaxDistribution)
                    {
                        TDTaxDistribution = Distribution.FindElements(By.TagName("td"));

                        if (TDTaxDistribution.Count != 0 && Distribution.Text.Trim() != "" && !Distribution.Text.Contains("Authorities"))
                        {
                            string Distributionvaluedetails = TDTaxDistribution[0].Text.Replace(":", "").Trim() + "~" + TDTaxDistribution[1].Text + "~" + TDTaxDistribution[2].Text + "~" + TDTaxDistribution[3].Text;
                            gc.insert_date(orderNumber, Parcelid, 1704, Distributionvaluedetails, 1, DateTime.Now);
                        }
                    }

                    IWebElement Taxspecialassess = driver.FindElement(By.Id("Special Assessments"));
                    IList<IWebElement> TRTaxspecialassess = Taxspecialassess.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDTaxspecialassess;
                    foreach (IWebElement specialassess in TRTaxspecialassess)
                    {
                        TDTaxspecialassess = specialassess.FindElements(By.TagName("td"));

                        if (TDTaxspecialassess.Count != 0 && specialassess.Text.Trim() != "" && !specialassess.Text.Contains("Authority"))
                        {
                            string specialassessvaluedetails = TDTaxspecialassess[0].Text.Replace(":", "").Trim() + "~" + "" + "~" + TDTaxspecialassess[1].Text + "~" + TDTaxspecialassess[2].Text;
                            gc.insert_date(orderNumber, Parcelid, 1704, specialassessvaluedetails, 1, DateTime.Now);
                        }
                    }

                    IWebElement Taxspecialassess1 = driver.FindElement(By.XPath("//*[@id='datalet_div_6']/table[2]/tbody"));
                    IList<IWebElement> TRTaxspecialassess1 = Taxspecialassess1.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDTaxspecialassess1;
                    foreach (IWebElement specialassess1 in TRTaxspecialassess1)
                    {
                        TDTaxspecialassess1 = specialassess1.FindElements(By.TagName("td"));

                        if (TDTaxspecialassess1.Count != 0 && specialassess1.Text.Trim() != "" && !specialassess1.Text.Contains("Authority"))
                        {
                            string specialassessvaluedetails1 = TDTaxspecialassess1[0].Text.Replace(":", "").Trim() + "~" + "" + "~" + "" + "~" + TDTaxspecialassess1[1].Text;
                            gc.insert_date(orderNumber, Parcelid, 1704, specialassessvaluedetails1, 1, DateTime.Now);
                        }
                    }

                    //By Fund and By Fund and Levy Pdf
                    driver.FindElement(By.XPath("//*[@id='sidemenu']/li[13]/a")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcelid, "By Fund", driver, "OH", "Lucas");

                    driver.FindElement(By.XPath("//*[@id='sidemenu']/li[14]/a")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcelid, "By Fund and Levy", driver, "OH", "Lucas");

                    //Prior Year Taxes Details
                    string TaxYear = "";

                    driver.FindElement(By.XPath("//*[@id='sidemenu']/li[15]/a")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcelid, "Prior Taxes", driver, "OH", "Lucas");
                    int Count = 0;
                    IWebElement TaxPrior = driver.FindElement(By.XPath("//*[@id='frmMain']/div[3]/div/div/table/tbody/tr/td/table/tbody"));
                    IList<IWebElement> TRTaxPrior = TaxPrior.FindElements(By.TagName("table"));
                    IList<IWebElement> TDTaxPrior;

                    foreach (IWebElement Prior in TRTaxPrior)
                    {
                        IWebElement TaxPrioryear2 = Prior;
                        IList<IWebElement> TRTaxPrioryear2 = TaxPrioryear2.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDTaxPrioryear2;

                        foreach (IWebElement Prioryear in TRTaxPrioryear2)
                        {
                            if (Count < 4)
                            {
                                TDTaxPrioryear2 = Prioryear.FindElements(By.TagName("td"));
                                if (TDTaxPrioryear2.Count != 0 && Prioryear.Text.Trim() != "" && Prioryear.Text.Contains("Tax Year") && !Prioryear.Text.Contains("1st Half 2nd Half"))
                                {
                                    TaxYear = TDTaxPrioryear2[0].Text.Replace("Tax Year", "").Replace(":", "").Trim();
                                    if (TDTaxPrioryear2[0].Text.Contains("Tax Year"))
                                    {
                                        Count++;
                                    }
                                }
                                if (!Prioryear.Text.Contains("PARCEL ID:"))
                                {
                                    if (TDTaxPrioryear2.Count != 0 && TDTaxPrioryear2.Count == 5 && Prioryear.Text.Trim() != "" && !Prioryear.Text.Contains("1st Half 2nd Half") && !Prioryear.Text.Contains("Tax Year"))
                                    {
                                        string Priorvaluedetails1 = TaxYear.Trim() + "~" + TDTaxPrioryear2[0].Text.Replace(":", "").Trim() + "~" + TDTaxPrioryear2[1].Text + "~" + TDTaxPrioryear2[2].Text + "~" + TDTaxPrioryear2[3].Text;
                                        gc.insert_date(orderNumber, Parcelid, 1708, Priorvaluedetails1, 1, DateTime.Now);
                                    }
                                }
                            }
                        }
                    }

                    //Special Assessment Details
                    driver.FindElement(By.XPath("//*[@id='sidemenu']/li[16]/a")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Special Assessment", driver, "OH", "Lucas");
                    IWebElement TaxSplass = driver.FindElement(By.XPath("//*[@id='datalet_div_2']/table[2]/tbody"));
                    IList<IWebElement> TRTaxSplass = TaxSplass.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDTaxSplass;
                    foreach (IWebElement Splass in TRTaxSplass)
                    {
                        TDTaxSplass = Splass.FindElements(By.TagName("td"));
                        if (TDTaxSplass.Count != 0 && Splass.Text.Trim() != "" && Splass.Text.Contains("Tax Year"))
                        {
                            TaxYear = TDTaxSplass[0].Text.Replace("Tax Year", "").Trim();
                        }
                        if (TDTaxSplass.Count != 0 && Splass.Text.Trim() != "" && !Splass.Text.Contains("Authority") && !Splass.Text.Contains("Tax Year"))
                        {
                            string Priorvaluedetails1 = TaxYear.Trim() + "~" + TDTaxSplass[0].Text.Replace(":", "").Trim() + "~" + TDTaxSplass[1].Text + "~" + TDTaxSplass[2].Text + "~" + TDTaxSplass[3].Text + "~" + TDTaxSplass[4].Text + "~" + TDTaxSplass[5].Text + "~" + TDTaxSplass[6].Text;
                            gc.insert_date(orderNumber, Parcelid, 1712, Priorvaluedetails1, 1, DateTime.Now);
                        }
                    }
                    //Prior Year Special Assessment Details
                    driver.FindElement(By.XPath("//*[@id='sidemenu']/li[19]/a")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcelid, "Prior Special Assessment", driver, "OH", "Lucas");
                    int count = 0;

                    IWebElement TaxPriorSplass = driver.FindElement(By.XPath("//*[@id='frmMain']/div[3]/div/div/table/tbody/tr/td/table/tbody"));
                    IList<IWebElement> TRTaxPriorSplass1 = TaxPriorSplass.FindElements(By.TagName("table"));
                    IList<IWebElement> TDTaxPriorSplass;
                    foreach (IWebElement PriorSplass1 in TRTaxPriorSplass1)
                    {
                        IWebElement TaxPriorSplass2 = PriorSplass1;
                        IList<IWebElement> TRTaxPriorSplass2 = TaxPriorSplass2.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDTaxPriorSplass2;
                        foreach (IWebElement PriorSplass in TRTaxPriorSplass2)
                        {
                            if (count < 3)
                            {
                                TDTaxPriorSplass2 = PriorSplass.FindElements(By.TagName("td"));
                                if (TDTaxPriorSplass2.Count != 0 && PriorSplass1.Text.Trim() != "" && !PriorSplass.Text.Contains("Authority") && PriorSplass.Text.Contains("Tax Year"))
                                {
                                    TaxYear = TDTaxPriorSplass2[0].Text.Replace("Tax Year", "").Trim();
                                    if (TDTaxPriorSplass2[0].Text.Contains("Tax Year"))
                                    {
                                        count++;
                                    }
                                }
                                if (!PriorSplass.Text.Contains("PARCEL ID:"))
                                {
                                    if (TDTaxPriorSplass2.Count != 0 && TDTaxPriorSplass2.Count == 6 && PriorSplass.Text.Trim() != "" && !PriorSplass.Text.Contains("Authority") && !PriorSplass.Text.Contains("Tax Year"))
                                    {
                                        string Priorvaluedetails1 = TaxYear.Trim() + "~" + TDTaxPriorSplass2[0].Text.Replace(":", "").Trim() + "~" + "" + "~" + TDTaxPriorSplass2[1].Text + "~" + TDTaxPriorSplass2[2].Text + "~" + TDTaxPriorSplass2[3].Text + "~" + TDTaxPriorSplass2[4].Text + "~" + TDTaxPriorSplass2[5].Text;
                                        gc.insert_date(orderNumber, Parcelid, 1712, Priorvaluedetails1, 1, DateTime.Now);
                                    }
                                }
                            }
                        }
                    }

                    //Payment Details Table
                    driver.FindElement(By.XPath("//*[@id='sidemenu']/li[17]/a")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Payment Details", driver, "OH", "Lucas");
                    IWebElement TaxPayment = driver.FindElement(By.Id("Payment Details"));
                    IList<IWebElement> TRTaxPayment = TaxPayment.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDTaxPayment;
                    foreach (IWebElement Payment in TRTaxPayment)
                    {
                        TDTaxPayment = Payment.FindElements(By.TagName("td"));

                        if (TDTaxPayment.Count != 0 && Payment.Text.Trim() != "" && !Payment.Text.Contains("Year - Half"))
                        {
                            string Paymentdetails1 = TDTaxPayment[0].Text.Trim() + "~" + TDTaxPayment[1].Text + "~" + TDTaxPayment[2].Text + "~" + TDTaxPayment[3].Text + "~" + TDTaxPayment[4].Text;
                            gc.insert_date(orderNumber, Parcelid, 1713, Paymentdetails1, 1, DateTime.Now);
                        }
                    }








                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "OH", "Lucas", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "OH", "Lucas");
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
