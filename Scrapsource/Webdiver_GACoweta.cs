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
using System.Web.UI;

namespace ScrapMaricopa.Scrapsource
{

    public class Webdiver_GACoweta
    {
        string Parcelno = "", Owner = "", Property_Address = "", MultiAddress_details = "";
        string ParcelID = "", Loc_Addrs = "", Leg_Desp = "", Class = "", Tax_Dist = "", Milagerate = "", Acres = "", Neighberwood = "", Homstd_Exmp = "", Owner1 = "", Year_Built = "", property = "";
        string Year1 = "", Year2 = "", Year3 = "", Assemnt_Details1 = "", Assemnt_Details2 = "", Assemnt_Details3 = "";
        string Owners = "", year = "", receipt = "", Desc = "", Type = "", Paid = "", Paid_date = "", Payment_details = "";
        string Record_Type = "", Taxy = "", Recep = "", Due_Date = "", Rcdtyp = "", tYear = "", Rep = "", Dudt = "", Duet = "";
        string Base_Taxes = "", Interst = "", Other_Fee = "", penalty = "", Back_Tax = "", Total_Due = "", tldue = "", Base = "", Pan = "", Inst = "", Otherfee = "", Backtax = "";
        string Dis = "", Acres1 = "", Assed_Val = "", Appr_Value = "", Description = "", Pro_Addr = "", Dist = "", Ac = "", AsVal = "", ProVal = "", AppVal = "", ApprVal = "", Accnt_Num = "";
        string Status = "", Last_Pay = "", Amt_Paid = "", Sta = "", Amtpaid = "", Latpay = "";
        string Entity = "", AdjtFMV = "", NetAssnt = "", Exemptions = "", Taxbleval = "", Milagete = "", grosstax = "", Credits = "", Net_Tax = "", Breakdown_details = "";
        string Entity1 = "", AdjtFMV1 = "", NetAssnt1 = "", Exemptions1 = "", Taxbleval1 = "", Milagete1 = "", grosstax1 = "", Credits1 = "", Net_Tax1 = "", Breakdown_details1 = "", Del_Taxyear = "";
        string Taxing = "", Tax_athuer = "", Phone = "", Fax = "", Taxing_Authority = "", Taxauthority_Details = "";
        string Del_details = "", name = "", Taxyear = "", bill_no = "", amount = "";
        string Entiteis = "CITY OF";
        int i = 0, j = 0;

        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_Coweta(string address, string assessment_id, string parcelNumber, string searchType, string orderNumber, string directParcel, string ownername)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())//
            {
                //driver = new ChromeDriver();

                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    if (searchType == "titleflex")
                    {
                        string titleaddress = address;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "GA", "Coweta");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_CowetaGA"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("https://qpublic.schneidercorp.com/Application.aspx?App=CowetaCountyGA&Layer=Parcels&PageType=Search");
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

                        driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_txtAddress")).SendKeys(address);

                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "GA", "Coweta");
                        driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        try
                        {
                            IWebElement MultiAddressTB = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_ctl01_gvwParcelResults']/tbody"));
                            IList<IWebElement> MultiAddressTR = MultiAddressTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiAddressTD;
                            gc.CreatePdf_WOP(orderNumber, "Multi Address search", driver, "GA", "Coweta");
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

                                        MultiAddress_details = Owner + "~" + Property_Address;
                                        gc.insert_date(orderNumber, Parcelno, 635, MultiAddress_details, 1, DateTime.Now);
                                    }
                                    AddressmaxCheck++;
                                }
                            }
                            if (MultiAddressTR.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Coweta_Multicount"] = "Maximum";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiparcel_Coweta"] = "Yes";
                            }
                            driver.Quit();

                            return "MultiParcel";
                        }
                        catch
                        { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("ctlBodyPane_noDataList_pnlNoResults")).Text;
                            if (nodata.Contains("No results match your search criteria."))
                            {
                                HttpContext.Current.Session["Nodata_CowetaGA"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("https://qpublic.schneidercorp.com/Application.aspx?App=CowetaCountyGA&Layer=Parcels&PageType=Search");
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

                        driver.FindElement(By.Id("ctlBodyPane_ctl02_ctl01_txtParcelID")).SendKeys(parcelNumber);

                        gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearch", driver, "GA", "Coweta");
                        driver.FindElement(By.Id("ctlBodyPane_ctl02_ctl01_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("ctlBodyPane_noDataList_pnlNoResults")).Text;
                            if (nodata.Contains("No results match your search criteria."))
                            {
                                HttpContext.Current.Session["Nodata_CowetaGA"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    if (searchType == "ownername")
                    {
                        driver.Navigate().GoToUrl("https://qpublic.schneidercorp.com/Application.aspx?App=CowetaCountyGA&Layer=Parcels&PageType=Search");
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

                        gc.CreatePdf_WOP(orderNumber, "Owner search", driver, "GA", "Coweta");
                        driver.FindElement(By.Id("ctlBodyPane_ctl00_ctl01_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        try
                        {
                            IWebElement MultiAddressTB = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_ctl01_gvwParcelResults']/tbody"));
                            IList<IWebElement> MultiAddressTR = MultiAddressTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiAddressTD;
                            gc.CreatePdf_WOP(orderNumber, "Multi Owner search", driver, "GA", "Coweta");
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

                                        MultiAddress_details = Owner + "~" + Property_Address;
                                        gc.insert_date(orderNumber, Parcelno, 635, MultiAddress_details, 1, DateTime.Now);
                                    }
                                    AddressmaxCheck++;
                                }
                            }
                            if (MultiAddressTR.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_Coweta_Multicount"] = "Maximum";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiparcel_Coweta"] = "Yes";
                            }
                            driver.Quit();

                            return "MultiParcel";
                        }
                        catch
                        { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("ctlBodyPane_noDataList_pnlNoResults")).Text;
                            if (nodata.Contains("No results match your search criteria."))
                            {
                                HttpContext.Current.Session["Nodata_CowetaGA"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    //Property Details
                    IWebElement tbmulti11 = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_ctl01_dvNonPrebillMH']/table/tbody"));
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
                                Loc_Addrs = TDmulti11[1].Text;
                            if (i == 3)
                                Leg_Desp = TDmulti11[1].Text;
                            if (i == 5)
                                Class = TDmulti11[1].Text;
                            if (i == 7)
                                Tax_Dist = TDmulti11[1].Text;
                            if (i == 8)
                                Milagerate = TDmulti11[1].Text;
                            if (i == 9)
                                Acres = TDmulti11[1].Text;
                            if (i == 10)
                                Neighberwood = TDmulti11[1].Text;
                            if (i == 11)
                                Homstd_Exmp = TDmulti11[1].Text;
                            i++;
                        }
                    }

                    try
                    {
                        Owner1 = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl01_mSection']/div")).Text.Replace("\r\n", "").Trim();
                    }
                    catch
                    { }
                    try
                    {
                        Year_Built = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl09_ctl01_gvwPrebillMH']/tbody/tr/td[4]")).Text;
                    }
                    catch
                    { }
                    try
                    {
                        IWebElement Yeartb = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl05_mSection']/div/table/tbody"));
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

                    property = Loc_Addrs + "~" + Leg_Desp + "~" + Class + "~" + Tax_Dist + "~" + Milagerate + "~" + Acres + "~" + Neighberwood + "~" + Homstd_Exmp + "~" + Owner1 + "~" + Year_Built;
                    gc.CreatePdf(orderNumber, ParcelID, "Property Details", driver, "GA", "Coweta");
                    gc.insert_date(orderNumber, ParcelID, 636, property, 1, DateTime.Now);

                    //Assessment Details
                    IWebElement AssmThTb = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl13_ctl01_grdValuation']/thead"));
                    IList<IWebElement> AssmThTr = AssmThTb.FindElements(By.TagName("tr"));
                    IList<IWebElement> AssmTh;

                    foreach (IWebElement Assm in AssmThTr)
                    {
                        AssmTh = Assm.FindElements(By.TagName("th"));
                        if (AssmTh.Count != 0)
                        {
                            Year1 = AssmTh[0].Text;
                            Year2 = AssmTh[1].Text;
                            Year3 = AssmTh[2].Text;
                        }
                    }

                    IWebElement AssmTb = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl13_ctl01_grdValuation']/tbody"));
                    IList<IWebElement> AssmTr = AssmTb.FindElements(By.TagName("tr"));
                    IList<IWebElement> AssmTd;

                    List<string> Previous_Value = new List<string>();
                    List<string> Land_Value = new List<string>();
                    List<string> Improvement_Value = new List<string>();
                    List<string> Accessory_Value = new List<string>();
                    List<string> Current_Value = new List<string>();

                    foreach (IWebElement Assmrow in AssmTr)
                    {
                        AssmTd = Assmrow.FindElements(By.TagName("td"));

                        if (AssmTd.Count != 0)
                        {

                            if (j == 0)
                            {
                                Previous_Value.Add(AssmTd[2].Text);
                                Previous_Value.Add(AssmTd[3].Text);
                                Previous_Value.Add(AssmTd[4].Text);
                            }
                            else if (j == 1)
                            {
                                Land_Value.Add(AssmTd[2].Text);
                                Land_Value.Add(AssmTd[3].Text);
                                Land_Value.Add(AssmTd[4].Text);
                            }
                            else if (j == 2)
                            {
                                Improvement_Value.Add(AssmTd[2].Text);
                                Improvement_Value.Add(AssmTd[3].Text);
                                Improvement_Value.Add(AssmTd[4].Text);
                            }
                            else if (j == 3)
                            {
                                Accessory_Value.Add(AssmTd[2].Text);
                                Accessory_Value.Add(AssmTd[3].Text);
                                Accessory_Value.Add(AssmTd[4].Text);
                            }
                            else if (j == 4)
                            {
                                Current_Value.Add(AssmTd[2].Text);
                                Current_Value.Add(AssmTd[3].Text);
                                Current_Value.Add(AssmTd[4].Text);
                            }

                            j++;
                        }
                    }

                    Assemnt_Details1 = Year1 + "~" + Previous_Value[0] + "~" + Land_Value[0] + "~" + Improvement_Value[0] + "~" + Accessory_Value[0] + "~" + Current_Value[0];
                    Assemnt_Details2 = Year2 + "~" + Previous_Value[1] + "~" + Land_Value[1] + "~" + Improvement_Value[1] + "~" + Accessory_Value[1] + "~" + Current_Value[1];
                    Assemnt_Details3 = Year3 + "~" + Previous_Value[2] + "~" + Land_Value[2] + "~" + Improvement_Value[2] + "~" + Accessory_Value[2] + "~" + Current_Value[2];
                    gc.insert_date(orderNumber, ParcelID, 638, Assemnt_Details1, 1, DateTime.Now);
                    gc.insert_date(orderNumber, ParcelID, 638, Assemnt_Details2, 1, DateTime.Now);
                    gc.insert_date(orderNumber, ParcelID, 638, Assemnt_Details3, 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    //Tax Details
                    driver.Navigate().GoToUrl("https://www.cowetataxcom.com/taxes.html#/WildfireSearch");
                    Thread.Sleep(2000);
                    //driver.FindElement(By.LinkText("SEARCH & PAY TAXES")).Click();
                    Thread.Sleep(2000);
                    try
                    {
                        driver.FindElement(By.Id("btnAccept")).Click();
                        Thread.Sleep(2000);
                    }
                    catch
                    { }

                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[3]/button[1]")).Click();
                        Thread.Sleep(2000);
                    }
                    catch
                    { }

                    driver.FindElement(By.Id("searchBox")).SendKeys(ParcelID);
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
                                gc.CreatePdf(orderNumber, ParcelID, "Tax Bill Details", driver, "GA", "Coweta");
                                gc.insert_date(orderNumber, ParcelID, 645, Payment_details, 1, DateTime.Now);
                            }
                        }
                    }
                    catch
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
                    IList<IWebElement> ReceipttableRow = Receipttable.FindElements(By.TagName("tr"));
                    int rowcount = ReceipttableRow.Count;

                    for (int p = 1; p <= rowcount; p++)
                    {
                        if (p < 4)
                        {

                            try
                            {
                                driver.FindElement(By.XPath("//*[@id='avalon']/div/div[3]/div[2]/table/tbody/tr[" + p + "]/td[9]/button")).Click();
                            }
                            catch { }

                            try
                            {
                                driver.FindElement(By.XPath("//*[@id='avalon']/div/div[4]/div[2]/table/tbody/tr[" + p + "]/td[9]/button/i")).Click();
                            }
                            catch { }

                            Thread.Sleep(6000);

                            try
                            {
                                IWebElement DeliquentTB = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/table/tbody"));
                                IList<IWebElement> DeliquentTR = DeliquentTB.FindElements(By.TagName("tr"));
                                IList<IWebElement> DeliquentTD;

                                foreach (IWebElement Deliquent in DeliquentTR)
                                {
                                    DeliquentTD = Deliquent.FindElements(By.TagName("td"));
                                    if (DeliquentTD.Count != 0 && DeliquentTR.Count != 2)
                                    {
                                        name = DeliquentTD[0].Text;
                                        Taxyear = DeliquentTD[1].Text;
                                        bill_no = DeliquentTD[2].Text;
                                        amount = DeliquentTD[6].Text;

                                        Del_details = name + "~" + Taxyear + "~" + bill_no + "~" + amount;
                                        gc.CreatePdf(orderNumber, ParcelID, "Deliquent Details" + Taxy, driver, "GA", "Coweta");
                                        gc.insert_date(orderNumber, ParcelID, 652, Del_details, 1, DateTime.Now);
                                        name = ""; Taxyear = ""; bill_no = ""; amount = "";
                                    }
                                }

                            }
                            catch
                            { }
                            try
                            {
                                IWebElement DeliquentfootTB = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/table/tfoot"));
                                IList<IWebElement> DeliquentfootTR = DeliquentfootTB.FindElements(By.TagName("tr"));
                                IList<IWebElement> DeliquentfootTD;

                                foreach (IWebElement Deliquentfoot in DeliquentfootTR)
                                {
                                    DeliquentfootTD = Deliquentfoot.FindElements(By.TagName("th"));
                                    if (DeliquentfootTD.Count != 0 && !Deliquentfoot.Text.Contains("$734.16"))
                                    {
                                        string bill_no1 = DeliquentfootTD[0].Text;
                                        string amount1 = DeliquentfootTD[2].Text;

                                        string Del_details1 = "" + "~" + "" + "~" + bill_no1 + "~" + amount1;
                                        gc.insert_date(orderNumber, ParcelID, 652, Del_details1, 1, DateTime.Now);
                                    }
                                }
                            }
                            catch
                            { }

                            try
                            {
                                driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[2]/button[2]")).Click();
                                Thread.Sleep(2000);
                            }
                            catch
                            { }

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
                                        Acres1 = TaxTD[1].Text;
                                    }
                                    AsVal = TaxTD[0].Text;
                                    if (AsVal.Contains("Description"))
                                    {
                                        Description = TaxTD[1].Text;
                                    }
                                    ProVal = TaxTD[0].Text;
                                    if (ProVal.Contains("Property Address"))
                                    {
                                        Pro_Addr = TaxTD[1].Text;
                                    }
                                    AppVal = TaxTD[0].Text;
                                    if (AppVal.Contains("Assessed Value"))
                                    {
                                        Assed_Val = TaxTD[1].Text;
                                    }
                                    ApprVal = TaxTD[0].Text;
                                    if (ApprVal.Contains("Appraised Value"))
                                    {
                                        Appr_Value = TaxTD[1].Text;
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
                                    Rcdtyp = Tax2TD[0].Text;
                                    if (Rcdtyp.Contains("Record Type"))
                                    {
                                        Record_Type = Tax2TD[1].Text;
                                    }
                                    tYear = Tax2TD[0].Text;
                                    if (tYear.Contains("Tax Year"))
                                    {
                                        Taxy = Tax2TD[1].Text;
                                    }
                                    Rep = Tax2TD[0].Text;
                                    if (Rep.Contains("Bill Number"))
                                    {
                                        Recep = Tax2TD[1].Text;
                                    }
                                    Dudt = Tax2TD[0].Text;
                                    if (Dudt.Contains("Account Number"))
                                    {
                                        Accnt_Num = Tax2TD[1].Text;
                                    }
                                    Duet = Tax2TD[0].Text;
                                    if (Duet.Contains("Due Date"))
                                    {
                                        Due_Date = Tax2TD[1].Text;
                                    }
                                }
                            }

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
                                    Pan = Tax4TD[0].Text;
                                    if (Pan.Contains("Penalty"))
                                    {
                                        penalty = Tax4TD[1].Text;
                                    }
                                    Inst = Tax4TD[0].Text;
                                    if (Inst.Contains("Interest"))
                                    {
                                        Interst = Tax4TD[1].Text;
                                    }
                                    Otherfee = Tax4TD[0].Text;
                                    if (Otherfee.Contains("Other Fees"))
                                    {
                                        Other_Fee = Tax4TD[1].Text;
                                    }
                                    string tytl_due = Tax4TD[0].Text;
                                    if (tytl_due.Contains(Taxy + "Total Due"))
                                    {
                                        string Del_Taxyear = Tax4TD[1].Text;
                                    }
                                    Backtax = Tax4TD[0].Text;
                                    if (Backtax.Contains("Back Taxes"))
                                    {
                                        Back_Tax = Tax4TD[1].Text;
                                    }
                                    tldue = Tax4TD[0].Text;
                                    if (tldue.Contains("Total Due"))
                                    {
                                        Total_Due = Tax4TD[1].Text;
                                    }
                                }
                            }

                            gc.CreatePdf(orderNumber, ParcelID, "Tax Details" + Taxy, driver, "GA", "Coweta");

                            string Tax_Deatils = Taxy + "~" + Dis + "~" + Acres1 + "~" + Description + "~" + Pro_Addr + "~" + Assed_Val + "~" + Appr_Value + "~" + Status + "~" + Last_Pay + "~" + Amt_Paid + "~" + Record_Type + "~" + Recep + "~" + Accnt_Num + "~" + Due_Date + "~" + Base_Taxes + "~" + penalty + "~" + Interst + "~" + Other_Fee + "~" + Del_Taxyear + "~" + Back_Tax + "~" + Total_Due;
                            gc.insert_date(orderNumber, ParcelID, 647, Tax_Deatils, 1, DateTime.Now);

                            //Tax Breakdown


                            try
                            {
                                IWebElement BreakdownTB = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[3]/div/table/tbody"));
                                IList<IWebElement> BreakdownTR = BreakdownTB.FindElements(By.TagName("tr"));
                                IList<IWebElement> BreakdownTD;

                                foreach (IWebElement Breakdown in BreakdownTR)
                                {
                                    BreakdownTD = Breakdown.FindElements(By.TagName("td"));
                                    if (BreakdownTD.Count != 0)
                                    {
                                        Entity = BreakdownTD[0].Text;
                                        AdjtFMV = BreakdownTD[1].Text;
                                        NetAssnt = BreakdownTD[2].Text;
                                        Exemptions = BreakdownTD[3].Text;
                                        Taxbleval = BreakdownTD[4].Text;
                                        Milagete = BreakdownTD[5].Text;
                                        grosstax = BreakdownTD[6].Text;
                                        Credits = BreakdownTD[7].Text;
                                        Net_Tax = BreakdownTD[8].Text;

                                        Breakdown_details = Entity + "~" + AdjtFMV + "~" + NetAssnt + "~" + Exemptions + "~" + Taxbleval + "~" + Milagete + "~" + grosstax + "~" + Credits + "~" + Net_Tax;
                                        gc.insert_date(orderNumber, ParcelID, 650, Breakdown_details, 1, DateTime.Now);
                                    }
                                }
                            }
                            catch
                            { }

                            try
                            {
                                IWebElement FooterTB = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[3]/div/table/tfoot"));
                                IList<IWebElement> FooterTR = FooterTB.FindElements(By.TagName("tr"));
                                IList<IWebElement> FooterTD;

                                foreach (IWebElement Footer in FooterTR)
                                {
                                    FooterTD = Footer.FindElements(By.TagName("th"));
                                    if (FooterTD.Count != 0)
                                    {
                                        Entity1 = FooterTD[0].Text;
                                        AdjtFMV1 = FooterTD[1].Text;
                                        NetAssnt1 = FooterTD[2].Text;
                                        Exemptions1 = FooterTD[3].Text;
                                        Taxbleval1 = FooterTD[4].Text;
                                        Milagete1 = FooterTD[5].Text;
                                        grosstax1 = FooterTD[6].Text;
                                        Credits1 = FooterTD[7].Text;
                                        Net_Tax1 = FooterTD[8].Text;
                                    }
                                }

                                Breakdown_details1 = Entity1 + "~" + AdjtFMV1 + "~" + NetAssnt1 + "~" + Exemptions1 + "~" + Taxbleval1 + "~" + Milagete1 + "~" + grosstax1 + "~" + Credits1 + "~" + Net_Tax1;
                                gc.insert_date(orderNumber, ParcelID, 650, Breakdown_details1, 1, DateTime.Now);
                            }
                            catch
                            { }

                            //Tax Bill
                            try
                            {
                                driver.FindElement(By.XPath("//*[@id='avalon']/div/div/ul/li[2]/a")).Click();
                                Thread.Sleep(5000);

                                gc.CreatePdf(orderNumber, ParcelID, "Tax Bill Details" + Taxy, driver, "GA", "Coweta");
                            }
                            catch
                            { }
                            try
                            {
                                driver.FindElement(By.XPath("//*[@id='avalon']/div/div/ul/li[3]/a")).Click();
                                Thread.Sleep(4000);

                                gc.CreatePdf(orderNumber, ParcelID, "Tax Receipt Details" + Taxy, driver, "GA", "Coweta");
                            }
                            catch
                            { }

                            driver.Navigate().Back();
                            Thread.Sleep(3000);
                        }
                    }

                    //Tax Authority
                    driver.Navigate().GoToUrl("https://www.cowetataxcom.com/#/contact");
                    Thread.Sleep(2000);
                    try
                    {
                        Taxing = driver.FindElement(By.XPath("//*[@id='editor1567']/p[1]")).Text;
                        Taxing = WebDriverTest.Between(Taxing, "Coweta County Tax Commissioner", "County Administration Building").Replace("\r\n", " ").Trim();
                        Tax_athuer = "Newnan, Georgia 30264";
                        Phone = "770-254-2670";
                        Fax = "770-683-2038";
                        gc.CreatePdf(orderNumber, ParcelID, "Tax Authority Details", driver, "GA", "Coweta");
                        Taxing_Authority = Taxing + " " + Tax_athuer + " " + Phone + " " + Fax;

                        Taxauthority_Details = Taxing_Authority;
                        gc.insert_date(orderNumber, ParcelID, 653, Taxauthority_Details, 1, DateTime.Now);
                    }
                    catch
                    { }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "GA", "Coweta", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "GA", "Coweta");
                    return "Data Inserted Successfully";
                }
                catch (Exception ex)
                {
                    driver.Quit();
                    GlobalClass.LogError(ex, orderNumber);
                    throw;
                }
            }
        }
    }
}