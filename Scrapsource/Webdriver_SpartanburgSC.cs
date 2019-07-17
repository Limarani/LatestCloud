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
using System.Text.RegularExpressions;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Support.Extensions;
using System.Net;
using OpenQA.Selenium.Firefox;
using System.Diagnostics;
using OpenQA.Selenium.Remote;

namespace ScrapMaricopa.Scrapsource
{
    public class Webdriver_SpartanburgSC
    {
        string Parcelno = "", Owner = "", Property_Address = "", City = "", MultiAddress_details = "", Parcelhref = "";
        string ParcelID = "", Account = "", Land_Size = "", Location_Address = "", Legal_Desc = "", Neighberwood = "", Property_Usage = "", property = "", Owner1 = "", Owner2 = "", OwnerN = "", Year_Built = "";
        string Year1 = "", Year2 = "", Year3 = "", Taxing = "", Phone = "", Fax = "", Taxing_Authority = "", Dist = "", Ac = "", AsVal = "", AppVal = "";
        string Owners = "", year = "", receipt = "", Desc = "", Type = "", Paid = "", Paid_date = "", Payment_details = "", Assemnt_Details1 = "", Assemnt_Details2 = "", Assemnt_Details3 = "";
        string Dis = "", Acres = "", Assed_Val = "", Appr_Value = "", Description = "", Sta = "", Latpay = "", Pstmrk = "", Amtpaid = "";
        string Status = "", Last_Pay = "", Post_Mark = "", Amt_Paid = "", Base = "", Cre = "", Fe = "", Pan = "", Cos = "", ToDue = "";
        string Record_Type = "", Taxy = "", Recep = "", Due_Date = "", Penalty_Date = "", Penalty = "", Amount_due = "";
        string Base_Taxes = "", credit = "", Fees = "", penalty = "", Costs = "", Totl_Due = "", PenaltyDates_details = "", Tax_Deatils = "";
        int i = 0, k = 0, l = 0, j = 0, m = 0, n = 0;
        IWebDriver driver;
        IWebElement Parcelwe;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_spartanburg(string houseno, string sname, string sttype, string accno, string parcelNumber, string searchType, string orderNumber, string ownername, string directParcel, string unitno)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new ChromeDriver();
            //driver = new PhantomJSDriver();
            var option = new ChromeOptions();
            option.AddArgument("No-Sandbox");
            using (driver = new ChromeDriver(option))
            //using (driver = new ChromeDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    if (searchType == "titleflex")
                    {
                        string titleaddress = houseno + " " + sname + " " + sttype + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "SC", "Spartanburg");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Spartanburg_Zero"] = "Zero";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("https://qpublic.schneidercorp.com/Application.aspx?App=SpartanburgCountySC&Layer=Parcels&PageType=Search");
                        string address = houseno + " " + sname + " " + sttype;
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='appBody']/div[4]/div/div/div[3]/a[1]")).Click();
                            Thread.Sleep(2000);
                        }
                        catch { }
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='appBody']/div[5]/div/div/div[3]/a")).Click();
                            Thread.Sleep(2000);
                        }
                        catch { }

                        driver.FindElement(By.Id("ctlBodyPane_ctl02_ctl01_txtAddress")).SendKeys(address);

                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "SC", "Spartanburg");
                        driver.FindElement(By.Id("ctlBodyPane_ctl02_ctl01_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        try
                        {
                            IWebElement MultiAddressTB = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_ctl01_gvwParcelResults']/tbody"));
                            IList<IWebElement> MultiAddressTR = MultiAddressTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiAddressTD;
                            gc.CreatePdf_WOP(orderNumber, "Multi Address search", driver, "SC", "Spartanburg");
                            int AddressmaxCheck = 0;

                            foreach (IWebElement MultiAddress in MultiAddressTR)
                            {
                                if (AddressmaxCheck <= 25)
                                {
                                    MultiAddressTD = MultiAddress.FindElements(By.TagName("td"));
                                    if (MultiAddressTD.Count != 0 && MultiAddressTD[4].Text.Trim().Contains(address.Trim().ToUpper()))
                                    {
                                        Parcelwe = MultiAddressTD[1].FindElement(By.TagName("a"));
                                        Parcelhref = Parcelwe.GetAttribute("href");
                                        Parcelno = MultiAddressTD[1].Text;
                                        Owner = MultiAddressTD[3].Text;
                                        Property_Address = MultiAddressTD[4].Text;
                                        City = MultiAddressTD[5].Text;
                                        MultiAddress_details = Owner + "~" + Property_Address + "~" + City;
                                        gc.insert_date(orderNumber, Parcelno, 606, MultiAddress_details, 1, DateTime.Now);
                                        AddressmaxCheck++;
                                    }

                                }
                            }
                            if (MultiAddressTR.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Spartanburg_Multicount"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                            if (AddressmaxCheck > 1 && AddressmaxCheck < 26)
                            {
                                HttpContext.Current.Session["multiparcel_Spartanburg"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (AddressmaxCheck == 1)
                            {
                                driver.Navigate().GoToUrl(Parcelhref);
                                Thread.Sleep(2000);
                            }
                        }
                        catch
                        { }

                        try
                        {
                            string Nodata = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_noDataList_pnlNoResults']/h3")).Text;
                            if (Nodata.Contains("No results match your search criteria."))
                            {
                                HttpContext.Current.Session["Spartanburg_Zero"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("https://qpublic.schneidercorp.com/Application.aspx?App=SpartanburgCountySC&Layer=Parcels&PageType=Search");
                        Thread.Sleep(2000);

                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='appBody']/div[4]/div/div/div[3]/a[1]")).Click();
                            Thread.Sleep(2000);
                        }
                        catch { }
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='appBody']/div[5]/div/div/div[3]/a")).Click();
                            Thread.Sleep(2000);
                        }
                        catch { }

                        driver.FindElement(By.Id("ctlBodyPane_ctl03_ctl01_txtParcelID")).SendKeys(parcelNumber);

                        gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearch", driver, "SC", "Spartanburg");
                        driver.FindElement(By.Id("ctlBodyPane_ctl03_ctl01_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        try
                        {
                            string Nodata = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_noDataList_pnlNoResults']/h3")).Text;
                            if (Nodata.Contains("No results match your search criteria."))
                            {
                                HttpContext.Current.Session["Spartanburg_Zero"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    else if (searchType == "block")
                    {
                        driver.Navigate().GoToUrl("https://qpublic.schneidercorp.com/Application.aspx?App=SpartanburgCountySC&Layer=Parcels&PageType=Search");
                        Thread.Sleep(2000);

                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='appBody']/div[4]/div/div/div[3]/a[1]")).Click();
                            Thread.Sleep(2000);
                        }
                        catch { }
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='appBody']/div[5]/div/div/div[3]/a")).Click();
                            Thread.Sleep(4000);
                        }
                        catch { }

                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl02_mSection']/div/div")));
                        Thread.Sleep(2000);

                        driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl04_ctl01_txtAlternateID']")).SendKeys(accno);
                        gc.CreatePdf_WOP(orderNumber, "Accoount search", driver, "SC", "Spartanburg");
                        driver.FindElement(By.Id("ctlBodyPane_ctl04_ctl01_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        try
                        {
                            string Nodata = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_noDataList_pnlNoResults']/h3")).Text;
                            if (Nodata.Contains("No results match your search criteria."))
                            {
                                HttpContext.Current.Session["Spartanburg_Zero"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    if (searchType == "ownername")
                    {
                        driver.Navigate().GoToUrl("https://qpublic.schneidercorp.com/Application.aspx?App=SpartanburgCountySC&Layer=Parcels&PageType=Search");
                        Thread.Sleep(2000);

                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='appBody']/div[4]/div/div/div[3]/a[1]")).Click();
                            Thread.Sleep(2000);
                        }
                        catch { }
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='appBody']/div[5]/div/div/div[3]/a")).Click();
                            Thread.Sleep(2000);
                        }
                        catch { }

                        driver.FindElement(By.Id("ctlBodyPane_ctl00_ctl01_txtName")).SendKeys(ownername);

                        gc.CreatePdf_WOP(orderNumber, "Owner search", driver, "SC", "Spartanburg");
                        driver.FindElement(By.Id("ctlBodyPane_ctl00_ctl01_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        try
                        {
                            IWebElement MultiAddressTB = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_ctl01_gvwParcelResults']/tbody"));
                            IList<IWebElement> MultiAddressTR = MultiAddressTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiAddressTD;
                            gc.CreatePdf_WOP(orderNumber, "Multi Owner search", driver, "SC", "Spartanburg");
                            int AddressmaxCheck = 0;

                            foreach (IWebElement MultiAddress in MultiAddressTR)
                            {
                                if (AddressmaxCheck <= 25)
                                {
                                    MultiAddressTD = MultiAddress.FindElements(By.TagName("td"));
                                    if (MultiAddressTD.Count != 0)
                                    {
                                        Parcelno = MultiAddressTD[1].Text;
                                        Owner = MultiAddressTD[3].Text;
                                        Property_Address = MultiAddressTD[4].Text;
                                        City = MultiAddressTD[5].Text;

                                        MultiAddress_details = Owner + "~" + Property_Address + "~" + City;
                                        gc.insert_date(orderNumber, Parcelno, 606, MultiAddress_details, 1, DateTime.Now);
                                    }
                                    AddressmaxCheck++;
                                }
                            }
                            if (MultiAddressTR.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Spartanburg_Multicount"] = "Maximum";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiparcel_Spartanburg"] = "Yes";
                            }
                            driver.Quit();

                            return "MultiParcel";
                        }
                        catch
                        { }

                        try
                        {
                            string Nodata = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_noDataList_pnlNoResults']/h3")).Text;
                            if (Nodata.Contains("No results match your search criteria."))
                            {
                                HttpContext.Current.Session["Spartanburg_Zero"] = "Zero";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    //Property Details
                    IWebElement tbmulti11 = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_mSection']/div/table/tbody"));
                    IList<IWebElement> TRmulti11 = tbmulti11.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDmulti11;
                    foreach (IWebElement row in TRmulti11)
                    {

                        TDmulti11 = row.FindElements(By.TagName("td"));
                        if (TDmulti11.Count != 0)
                        {
                            if (i == 0)
                                ParcelID = TDmulti11[1].Text;
                            if (i == 1)
                                Account = TDmulti11[1].Text;
                            if (i == 4)
                                Land_Size = TDmulti11[1].Text;
                            if (i == 8)
                                Location_Address = TDmulti11[1].Text;
                            if (i == 9)
                                Legal_Desc = TDmulti11[1].Text;
                            if (i == 11)
                                Neighberwood = TDmulti11[1].Text;
                            if (i == 12)
                                Property_Usage = TDmulti11[1].Text;
                            i++;
                        }
                    }

                    try
                    {
                        Owner1 = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl01_mSection']/div/div/div[1]")).GetAttribute("innerText").ToString().Replace("\r\n", " ");
                        // Owner1 = WebDriverTest.Before(Owner1, " &");
                    }
                    catch
                    { }

                    try
                    {
                        Owner2 = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl01_ctl01_lstDeed_ctl01_lblDeedName_lblSearch']")).GetAttribute("innerText").ToString();
                    }
                    catch
                    { }
                    if (Owner2 == "")
                    {
                        OwnerN = Owner1;
                    }
                    else
                    {
                        OwnerN = Owner1 + " & " + Owner2;
                    }


                    try
                    {

                    }
                    catch
                    { }

                    try
                    {
                        IWebElement Yeartb = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl06_mSection']/div/table/tbody"));
                        IList<IWebElement> YearTR = Yeartb.FindElements(By.TagName("tr"));
                        IList<IWebElement> YearTD;

                        foreach (IWebElement Tax4 in YearTR)
                        {
                            YearTD = Tax4.FindElements(By.TagName("td"));
                            if (YearTD.Count != 0)
                            {
                                string yblt = YearTD[0].Text;
                                if (yblt.Contains("Year Built"))
                                {
                                    Year_Built = YearTD[1].Text;
                                }
                            }
                        }
                    }
                    catch
                    { }

                    property = Account + "~" + Land_Size + "~" + Location_Address + "~" + Legal_Desc + "~" + Neighberwood + "~" + Property_Usage + "~" + OwnerN + "~" + Year_Built;
                    gc.CreatePdf(orderNumber, ParcelID, "Property Details", driver, "SC", "Spartanburg");
                    gc.insert_date(orderNumber, ParcelID, 607, property, 1, DateTime.Now);


                    //Assessment Details
                    IWebElement AssmThTb = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl03_ctl01_grdValuation']/thead"));
                    IList<IWebElement> AssmThTr = AssmThTb.FindElements(By.TagName("tr"));
                    IList<IWebElement> AssmTh;

                    foreach (IWebElement Assm in AssmThTr)
                    {
                        AssmTh = Assm.FindElements(By.TagName("th"));
                        if (AssmTh.Count != 0)
                        {
                            Year1 = AssmTh[0].Text;
                            try
                            {
                                Year2 = AssmTh[1].Text;
                                Year3 = AssmTh[2].Text;
                            }
                            catch
                            { }
                        }
                    }


                    try
                    {
                        IWebElement AssmTb = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl03_ctl01_grdValuation']/tbody"));
                        IList<IWebElement> AssmTr = AssmTb.FindElements(By.TagName("tr"));
                        IList<IWebElement> AssmTd;

                        List<string> MarketLand_Value = new List<string>();
                        List<string> MarketImprovement_Value = new List<string>();
                        List<string> MarketMisc_Value = new List<string>();
                        List<string> TotalMarket_Value = new List<string>();
                        List<string> TaxableLand_Value = new List<string>();
                        List<string> TaxableImprovement_Value = new List<string>();
                        List<string> TaxableMisc_Value = new List<string>();
                        List<string> AgCredit_Value = new List<string>();
                        List<string> TotalTaxable_Value = new List<string>();
                        List<string> AssessedLand_Value = new List<string>();
                        List<string> AssessedImprovement_Value = new List<string>();
                        List<string> AssessedMisc_Value = new List<string>();
                        List<string> TotalAssessed_Value = new List<string>();

                        foreach (IWebElement Assmrow in AssmTr)
                        {
                            AssmTd = Assmrow.FindElements(By.TagName("td"));

                            if (AssmTd.Count != 0)
                            {

                                if (j == 0)
                                {
                                    MarketLand_Value.Add(AssmTd[2].Text);
                                    try
                                    {
                                        MarketLand_Value.Add(AssmTd[3].Text);
                                        MarketLand_Value.Add(AssmTd[4].Text);
                                    }
                                    catch
                                    { }
                                }
                                else if (j == 1)
                                {
                                    MarketImprovement_Value.Add(AssmTd[2].Text);
                                    try
                                    {
                                        MarketImprovement_Value.Add(AssmTd[3].Text);
                                        MarketImprovement_Value.Add(AssmTd[4].Text);
                                    }
                                    catch
                                    { }
                                }
                                else if (j == 2)
                                {
                                    MarketMisc_Value.Add(AssmTd[2].Text);
                                    try
                                    {
                                        MarketMisc_Value.Add(AssmTd[3].Text);
                                        MarketMisc_Value.Add(AssmTd[4].Text);
                                    }
                                    catch
                                    { }

                                }
                                else if (j == 3)
                                {
                                    TotalMarket_Value.Add(AssmTd[2].Text);
                                    try
                                    {
                                        TotalMarket_Value.Add(AssmTd[3].Text);
                                        TotalMarket_Value.Add(AssmTd[4].Text);
                                    }
                                    catch
                                    { }

                                }
                                else if (j == 4)
                                {
                                    TaxableLand_Value.Add(AssmTd[2].Text);
                                    try
                                    {
                                        TaxableLand_Value.Add(AssmTd[3].Text);
                                        TaxableLand_Value.Add(AssmTd[4].Text);
                                    }
                                    catch
                                    { }

                                }
                                else if (j == 5)
                                {
                                    TaxableImprovement_Value.Add(AssmTd[2].Text);
                                    try
                                    {
                                        TaxableImprovement_Value.Add(AssmTd[3].Text);
                                        TaxableImprovement_Value.Add(AssmTd[4].Text);
                                    }
                                    catch
                                    { }

                                }
                                else if (j == 6)
                                {
                                    TaxableMisc_Value.Add(AssmTd[2].Text);
                                    try
                                    {
                                        TaxableMisc_Value.Add(AssmTd[3].Text);
                                        TaxableMisc_Value.Add(AssmTd[4].Text);
                                    }
                                    catch
                                    { }
                                }
                                else if (j == 7)
                                {
                                    AgCredit_Value.Add(AssmTd[2].Text);
                                    try
                                    {
                                        AgCredit_Value.Add(AssmTd[3].Text);
                                        AgCredit_Value.Add(AssmTd[4].Text);
                                    }
                                    catch
                                    { }

                                }
                                else if (j == 8)
                                {
                                    TotalTaxable_Value.Add(AssmTd[2].Text);
                                    try
                                    {
                                        TotalTaxable_Value.Add(AssmTd[3].Text);
                                        TotalTaxable_Value.Add(AssmTd[4].Text);
                                    }
                                    catch
                                    { }

                                }
                                else if (j == 9)
                                {
                                    AssessedLand_Value.Add(AssmTd[2].Text);
                                    try
                                    {
                                        AssessedLand_Value.Add(AssmTd[3].Text);
                                        AssessedLand_Value.Add(AssmTd[4].Text);
                                    }
                                    catch
                                    { }

                                }
                                else if (j == 10)
                                {
                                    AssessedImprovement_Value.Add(AssmTd[2].Text);
                                    try
                                    {
                                        AssessedImprovement_Value.Add(AssmTd[3].Text);
                                        AssessedImprovement_Value.Add(AssmTd[4].Text);
                                    }
                                    catch
                                    { }


                                }
                                else if (j == 11)
                                {
                                    AssessedMisc_Value.Add(AssmTd[2].Text);
                                    try
                                    {
                                        AssessedMisc_Value.Add(AssmTd[3].Text);
                                        AssessedMisc_Value.Add(AssmTd[4].Text);
                                    }
                                    catch
                                    { }

                                }
                                else if (j == 12)
                                {
                                    TotalAssessed_Value.Add(AssmTd[2].Text);
                                    try
                                    {
                                        TotalAssessed_Value.Add(AssmTd[3].Text);
                                        TotalAssessed_Value.Add(AssmTd[4].Text);
                                    }
                                    catch
                                    { }
                                }
                                j++;
                            }
                        }

                        Assemnt_Details1 = Year1 + "~" + MarketLand_Value[0] + "~" + MarketImprovement_Value[0] + "~" + MarketMisc_Value[0] + "~" + TotalMarket_Value[0] + "~" + TaxableLand_Value[0] + "~" + TaxableImprovement_Value[0] + "~" + TaxableMisc_Value[0] + "~" + AgCredit_Value[0] + "~" + TotalTaxable_Value[0] + "~" + AssessedLand_Value[0] + "~" + AssessedImprovement_Value[0] + "~" + AssessedMisc_Value[0] + "~" + TotalAssessed_Value[0];
                        try
                        {
                            Assemnt_Details2 = Year2 + "~" + MarketLand_Value[1] + "~" + MarketImprovement_Value[1] + "~" + MarketMisc_Value[1] + "~" + TotalMarket_Value[1] + "~" + TaxableLand_Value[1] + "~" + TaxableImprovement_Value[1] + "~" + TaxableMisc_Value[1] + "~" + AgCredit_Value[1] + "~" + TotalTaxable_Value[1] + "~" + AssessedLand_Value[1] + "~" + AssessedImprovement_Value[1] + "~" + AssessedMisc_Value[1] + "~" + TotalAssessed_Value[1];
                            if (Year3 != "")
                            {
                                Assemnt_Details3 = Year3 + "~" + MarketLand_Value[2] + "~" + MarketImprovement_Value[2] + "~" + MarketMisc_Value[2] + "~" + TotalMarket_Value[2] + "~" + TaxableLand_Value[2] + "~" + TaxableImprovement_Value[2] + "~" + TaxableMisc_Value[2] + "~" + AgCredit_Value[2] + "~" + TotalTaxable_Value[2] + "~" + AssessedLand_Value[2] + "~" + AssessedImprovement_Value[2] + "~" + AssessedMisc_Value[2] + "~" + TotalAssessed_Value[2];
                            }
                        }
                        catch
                        { }

                        gc.insert_date(orderNumber, ParcelID, 608, Assemnt_Details1, 1, DateTime.Now);
                        try
                        {
                            gc.insert_date(orderNumber, ParcelID, 608, Assemnt_Details2, 1, DateTime.Now);
                            if (Year3 != "")
                            {
                                gc.insert_date(orderNumber, ParcelID, 608, Assemnt_Details3, 1, DateTime.Now);
                            }
                        }
                        catch
                        { }

                        try
                        {
                            ByVisibleElement(driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl03_ctl01_grdValuation']/tbody/tr[3]")));
                            gc.CreatePdf(orderNumber, ParcelID, "Assessment Details", driver, "SC", "Spartanburg");
                        }
                        catch (Exception)
                        { }

                        try
                        {
                            ByVisibleElement(driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl06_mSection']/div/table")));
                            gc.CreatePdf(orderNumber, ParcelID, "Year Built Details", driver, "SC", "Spartanburg");
                        }
                        catch (Exception)
                        { }
                        AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    }
                    catch
                    { }

                    //Tax Details
                    driver.Navigate().GoToUrl("https://spartanburgcountytax.com/#/WildfireSearch");
                    Thread.Sleep(8000);

                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[3]/button[1]")).Click();
                        Thread.Sleep(6000);
                    }
                    catch
                    { }
                    //ParcelID = WebDriverTest.Before(ParcelID, ".");
                    driver.FindElement(By.Id("searchBox")).SendKeys(ParcelID);
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='searchForm']/div[1]/div/span/button")).SendKeys(Keys.Enter);
                    }
                    catch { }
                    Thread.Sleep(4000);

                    //TaxPayment Receipt Details
                    try
                    {
                        IWebElement TaxPaymentTB = null;
                        try
                        {
                            TaxPaymentTB = driver.FindElement(By.XPath("//*[@id='avalon']/div/div[3]/div[2]/table/tbody"));
                        }
                        catch { }
                        try
                        {
                            if (TaxPaymentTB == null)
                            {
                                TaxPaymentTB = driver.FindElement(By.XPath("//*[@id='avalon']/div/div[4]/div[2]/table/tbody"));
                            }
                        }
                        catch { }
                        IList<IWebElement> TaxPaymentTR = TaxPaymentTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> TaxPaymentTD;

                        foreach (IWebElement TaxPayment in TaxPaymentTR)
                        {
                            TaxPaymentTD = TaxPayment.FindElements(By.TagName("td"));
                            if (TaxPaymentTD.Count != 0)
                            {
                                Owners = TaxPaymentTD[0].Text;
                                year = TaxPaymentTD[1].Text;
                                receipt = TaxPaymentTD[2].Text;
                                Desc = TaxPaymentTD[3].Text;
                                Type = TaxPaymentTD[4].Text;
                                Paid = TaxPaymentTD[5].Text;
                                Paid_date = TaxPaymentTD[6].Text;

                                Payment_details = Owners + "~" + year + "~" + receipt + "~" + Desc + "~" + Type + "~" + Paid + "~" + Paid_date;
                                gc.CreatePdf(orderNumber, ParcelID, "Tax Payment Details", driver, "SC", "Spartanburg");
                                gc.insert_date(orderNumber, ParcelID, 609, Payment_details, 1, DateTime.Now);
                            }
                        }
                    }
                    catch
                    { }

                    try
                    {
                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='avalon']/div/div[3]/div[2]/table/tbody")));
                        gc.CreatePdf(orderNumber, ParcelID, "Tax Payment Details1", driver, "SC", "Spartanburg");
                    }
                    catch (Exception)
                    { }
                    try
                    {
                        ByVisibleElement(driver.FindElement(By.XPath("//*[@id='avalon']/div/div[4]/div[2]/table/tbody")));
                        gc.CreatePdf(orderNumber, ParcelID, "Tax Payment Details1", driver, "SC", "Spartanburg");
                    }
                    catch (Exception)
                    { }
                    //Tax Info Details
                    IWebElement Receipttable = null;
                    try
                    {
                        Receipttable = driver.FindElement(By.XPath("//*[@id='avalon']/div/div[3]/div[2]/table/tbody"));
                    }
                    catch { }
                    try
                    {
                        if (Receipttable == null)
                        {
                            Receipttable = driver.FindElement(By.XPath("//*[@id='avalon']/div/div[4]/div[2]/table/tbody"));
                        }
                    }
                    catch { }
                    //IWebElement Receipttable = driver.FindElement(By.XPath("//*[@id='avalon']/div/div[3]/div[2]/table/tbody"));
                    IList<IWebElement> ReceipttableRow = Receipttable.FindElements(By.TagName("tr"));
                    int rowcount = ReceipttableRow.Count;

                    for (int p = 1; p <= rowcount; p++)
                    {
                        if (p < 4)
                        {
                            try
                            {
                                //driver.FindElement(By.XPath("//*[@id='avalon']/div/div[3]/div[2]/table/tbody/tr[" + p + "]/td[9]/button")).Click();
                                IWebElement IAddressSearch1 = driver.FindElement(By.XPath("//*[@id='avalon']/div/div[3]/div[2]/table/tbody/tr[" + p + "]/td[9]/button"));
                                IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                                js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                            }
                            catch { }
                            try
                            {
                                //driver.FindElement(By.XPath("//*[@id='avalon']/div/div[4]/div[2]/table/tbody/tr[" + p + "]/td[9]/button")).Click();
                                IWebElement IAddressSearch1 = driver.FindElement(By.XPath("//*[@id='avalon']/div/div[4]/div[2]/table/tbody/tr[" + p + "]/td[9]/button"));
                                IJavaScriptExecutor js1 = driver as IJavaScriptExecutor;
                                js1.ExecuteScript("arguments[0].click();", IAddressSearch1);
                            }
                            catch { }
                            Thread.Sleep(4000);

                            IWebElement TaxTB = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[2]/div[1]/table/tbody"));
                            IList<IWebElement> TaxTR = TaxTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> TaxTD;
                            foreach (IWebElement Tax in TaxTR)
                            {

                                TaxTD = Tax.FindElements(By.TagName("td"));
                                if (TaxTD.Count != 0 && !Tax.Text.Contains("Parcel Number") && TaxTD[1].Text != "")
                                {
                                    Dist = TaxTD[0].Text;
                                    if (Dist.Contains("District"))
                                    {
                                        Dis = TaxTD[1].Text;
                                    }
                                    Ac = TaxTD[0].Text;
                                    if (Ac.Contains("Acres"))
                                    {
                                        Acres = TaxTD[1].Text;
                                    }
                                    AsVal = TaxTD[0].Text;
                                    if (AsVal.Contains("Assessed Value"))
                                    {
                                        Assed_Val = TaxTD[1].Text;
                                    }
                                    AppVal = TaxTD[0].Text;
                                    if (AppVal.Contains("Appraised Value"))
                                    {
                                        Appr_Value = TaxTD[1].Text;
                                    }
                                }
                            }

                            try
                            {
                                Description = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[3]/div[1]")).Text;
                                Description = WebDriverTest.After(Description, "Description").Replace("\r\n", " ").Trim();
                            }
                            catch
                            { }

                            IWebElement Tax4TB = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[2]/div[3]/table/tbody"));
                            IList<IWebElement> Tax4TR = Tax4TB.FindElements(By.TagName("tr"));
                            IList<IWebElement> Tax4TD;
                            foreach (IWebElement Tax4 in Tax4TR)
                            {
                                Tax4TD = Tax4.FindElements(By.TagName("td"));
                                if (Tax4TD.Count != 0 && Tax4TD[1].Text != "")
                                {
                                    Base = Tax4TD[0].Text;
                                    if (Base.Contains("Base Taxes"))
                                    {
                                        Base_Taxes = Tax4TD[1].Text;
                                    }
                                    Cre = Tax4TD[0].Text;
                                    if (Cre.Contains("Credit"))
                                    {
                                        credit = Tax4TD[1].Text;
                                    }
                                    Fe = Tax4TD[0].Text;
                                    if (Fe.Contains("Fees"))
                                    {
                                        Fees = Tax4TD[1].Text;
                                    }
                                    Pan = Tax4TD[0].Text;
                                    if (Pan.Contains("Penalty"))
                                    {
                                        penalty = Tax4TD[1].Text;
                                    }
                                    Cos = Tax4TD[0].Text;
                                    if (Cos.Contains("Costs"))
                                    {
                                        Costs = Tax4TD[1].Text;
                                    }
                                    ToDue = Tax4TD[0].Text;
                                    if (ToDue.Contains("Total Due"))
                                    {
                                        Totl_Due = Tax4TD[1].Text;
                                    }
                                }
                            }

                            IWebElement Tax1TB = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[1]/div[2]/table/tbody"));
                            IList<IWebElement> Tax1TR = Tax1TB.FindElements(By.TagName("tr"));
                            IList<IWebElement> Tax1TD;
                            foreach (IWebElement Tax1 in Tax1TR)
                            {
                                Tax1TD = Tax1.FindElements(By.TagName("td"));
                                if (Tax1TD.Count != 0)
                                {
                                    Sta = Tax1TD[0].Text;
                                    if (Sta.Contains("Status"))
                                    {
                                        Status = Tax1TD[1].Text;
                                    }
                                    Latpay = Tax1TD[0].Text;
                                    if (Latpay.Contains("Last Payment Date"))
                                    {
                                        Last_Pay = Tax1TD[1].Text;
                                    }
                                    Pstmrk = Tax1TD[0].Text;
                                    if (Pstmrk.Contains("Postmark Date"))
                                    {
                                        Post_Mark = Tax1TD[1].Text;
                                    }
                                    Amtpaid = Tax1TD[0].Text;
                                    if (Amtpaid.Contains("Amount Paid"))
                                    {
                                        Amt_Paid = Tax1TD[1].Text;
                                    }
                                }
                            }

                            IWebElement Tax2TB = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[2]/div[2]/table/tbody"));
                            IList<IWebElement> Tax2TR = Tax2TB.FindElements(By.TagName("tr"));
                            IList<IWebElement> Tax2TD;
                            foreach (IWebElement Tax2 in Tax2TR)
                            {
                                Tax2TD = Tax2.FindElements(By.TagName("td"));
                                if (Tax2TD.Count != 0)
                                {
                                    string Rcdtyp = Tax2TD[0].Text;
                                    if (Rcdtyp.Contains("Record Type"))
                                    {
                                        Record_Type = Tax2TD[1].Text;
                                    }
                                    string tYear = Tax2TD[0].Text;
                                    if (tYear.Contains("Tax Year"))
                                    {
                                        Taxy = Tax2TD[1].Text;
                                    }
                                    string Rep = Tax2TD[0].Text;
                                    if (Rep.Contains("Receipt"))
                                    {
                                        Recep = Tax2TD[1].Text;
                                    }
                                    string Dudt = Tax2TD[0].Text;
                                    if (Dudt.Contains("Due Date"))
                                    {
                                        Due_Date = Tax2TD[1].Text;
                                    }
                                }
                            }

                            //Penalty Dates
                            IWebElement Tax3TB = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[3]/div[2]/table/tbody"));
                            IList<IWebElement> Tax3TR = Tax3TB.FindElements(By.TagName("tr"));
                            IList<IWebElement> Tax3TD;
                            foreach (IWebElement Tax3 in Tax3TR)
                            {
                                Tax3TD = Tax3.FindElements(By.TagName("td"));
                                if (Tax3TD.Count != 0)
                                {
                                    Penalty_Date = Tax3TD[0].Text;
                                    Penalty = Tax3TD[1].Text;
                                    Amount_due = Tax3TD[2].Text;

                                    PenaltyDates_details = Penalty_Date + "~" + Penalty + "~" + Amount_due;
                                    gc.insert_date(orderNumber, ParcelID, 614, PenaltyDates_details, 1, DateTime.Now);
                                }
                            }
                            gc.CreatePdf(orderNumber, ParcelID, "Tax Details" + Taxy, driver, "SC", "Spartanburg");

                            Tax_Deatils = Taxy + "~" + Dis + "~" + Acres + "~" + Assed_Val + "~" + Appr_Value + "~" + Description + "~" + Base_Taxes + "~" + credit + "~" + Fees + "~" + penalty + "~" + Costs + "~" + Totl_Due + "~" + Status + "~" + Last_Pay + "~" + Post_Mark + "~" + Amt_Paid + "~" + Record_Type + "~" + Recep + "~" + Due_Date;
                            gc.insert_date(orderNumber, ParcelID, 615, Tax_Deatils, 1, DateTime.Now);

                            try
                            {
                                ByVisibleElement(driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[2]/div[3]")));
                                gc.CreatePdf(orderNumber, ParcelID, "Tax Payment Details1" + Taxy, driver, "SC", "Spartanburg");
                            }
                            catch (Exception)
                            { }

                            //Tax Bill
                            driver.FindElement(By.XPath("//*[@id='avalon']/div/div/ul/li[2]/a")).Click();
                            Thread.Sleep(4000);

                            gc.CreatePdf(orderNumber, ParcelID, "Tax Bill Details" + Taxy, driver, "SC", "Spartanburg");

                            try
                            {
                                ByVisibleElement(driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[2]/form/div/div[1]/div[3]")));
                                gc.CreatePdf(orderNumber, ParcelID, "Tax Bill Details1" + Taxy, driver, "SC", "Spartanburg");
                            }
                            catch (Exception)
                            { }

                            driver.Navigate().Back();
                            Thread.Sleep(3000);
                        }

                    }

                    //Tax Authority
                    driver.Navigate().GoToUrl("https://www.spartanburgcounty.org/178/Tax-Collector");
                    Thread.Sleep(2000);

                    try
                    {
                        Taxing = driver.FindElement(By.XPath("//*[@id='ccd6923f64-2522-4bce-8783-61ad1b416b69']/div/div/div/div/div/ol/li[2]/div[1]")).Text;
                        Taxing = WebDriverTest.After(Taxing, "Physical Address View Map").Replace("\r\n", " ").Trim();
                        Phone = driver.FindElement(By.XPath("//*[@id='ccd6923f64-2522-4bce-8783-61ad1b416b69']/div/div/div/div/div/ol/li[2]/div[4]")).Text;
                        Fax = driver.FindElement(By.XPath("//*[@id='ccd6923f64-2522-4bce-8783-61ad1b416b69']/div/div/div/div/div/ol/li[2]/div[5]")).Text;
                        gc.CreatePdf(orderNumber, ParcelID, "Tax Authority Details", driver, "SC", "Spartanburg");

                        try
                        {
                            ByVisibleElement(driver.FindElement(By.XPath("//*[@id='ccd6923f64-2522-4bce-8783-61ad1b416b69']/div/div/div/div/div/ol/li[2]/div[1]")));
                            gc.CreatePdf(orderNumber, ParcelID, "Tax Authority Details", driver, "SC", "Spartanburg");
                        }
                        catch (Exception)
                        { }

                        Taxing_Authority = Taxing + " " + Phone + " " + Fax;

                        string Taxauthority_Details = Taxing_Authority;
                        gc.insert_date(orderNumber, ParcelID, 625, Taxauthority_Details, 1, DateTime.Now);
                    }
                    catch
                    { }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "SC", "Spartanburg", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "SC", "Spartanburg");
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
        public void ByVisibleElement(IWebElement Element)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].scrollIntoView();", Element);
        }

    }
}