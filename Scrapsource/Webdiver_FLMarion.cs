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
    public class Webdiver_FLMarion
    {
        string Parcelno = "", Owner = "", Property_Address = "", MultiAddress_details = "", ParcelID = "", MultiOwner_details = "", assess1 = "", assess11 = "", Improve1 = "", assess = "", full1 = "", titleaddress = "", titleOwner = "";
        string Primary_Key = "", Owner_Mailing = "", Taxes_Assments = "", Map_Id = "", Improve = "", Total_Asst_Value = "", Excemptions = "", Total_Taxable = "", Assessment = "";
        string Year = "", Land_Just = "", Building = "", Misc_Value = "", Mkt_Just = "", Assed_Value = "", Exceptions = "", Taxabl_Val = "", History_details = "";
        string Year_Built = "", property = "", Land_Just_Value = "", Buildings = "", Miscellneous = "", Total_Just_Value = "", Millage = "", Acres = "", Situs = "", Proty_Desp = "";
        string Tax_Year = "", Roll = "", Acc_Num = "", Sta = "", Date_Paid = "", Amount_Paid = "", Balnc_Due = "", Payment_details = "";
        string Acc_No = "", Typ = "", Add = "", Taxy = "", Pdes1 = "", Pdes2 = "", Pdes3 = "", P_des = "", OInfo = "", M_Addr1 = "", M_Addr2 = "", M_Addr = "", Markt_Value = "", ASSESSMENT = "", TAXABLE = "", EXC = "", EXC1 = "", EXC2 = "", TAXES = "", SP_ASMT = "", INT = "", ADV = "", SALE = "";
        string Mkt = "", Asst = "", Txb = "", Ext = "", Ext1 = "", Ext2 = "", txes = "", sptxs = "", intt = "", Salet = "", Advt = "", Tax_Info = "";
        string Receipt = "", A = "", B = "", C = "", D = "", E = "", F = "", G = "", Receipt_Details = "", Deliquent = "", Pdes4 = "", OInfo1 = "", OInfo2 = "", M_Addr11 = "", M_Addr12 = "", Owner_Mail = "";
        string Count_Asst = "", County_ASMT = "", Count_txbl = "", County_TXBL = "", Tax_Asst = "", Tax_ASMT = "", Tax_txbl = "", Tax_TXBL = "";
        string Taxauthority_Details = "", Taxing_Authority = "", Deliquent_Comments = "", Advalorem_details = "", Advalorem_details1 = "", Receipt_Details9 = "", Receipt_Details4 = "", Receipt_Details3 = "", Receipt_Details2 = "", Receipt_Details1 = "", Receipt_Details5 = "", Commonreceipt = "", School_Taxable = "", CER = "", CER_Comments = "", Roll_Coment = "", full = "";

        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());


        public string FTP_FLMarion(string address, string assessment_id, string parcelNumber, string searchType, string orderNumber, string directParcel, string ownername)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            List<string> mcheck = new List<string>();

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                //driver = new ChromeDriver();

                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    if (searchType == "titleflex")
                    {
                        titleaddress = address;
                        titleOwner = ownername;
                        gc.TitleFlexSearch(orderNumber, "", ownername, titleaddress, "FL", "Marion");

                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_FLMarion"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("http://www.pa.marion.fl.us/PropertySearch.aspx");
                        Thread.Sleep(2000);

                        driver.FindElement(By.Id("MCPAMaster_MCPAContent_rblSearchBy_0")).Click();
                        Thread.Sleep(2000);

                        driver.FindElement(By.Id("MCPAMaster_MCPAContent_txtParm")).SendKeys(address);

                        driver.FindElement(By.Id("MCPAMaster_MCPAContent_btnWine")).SendKeys(Keys.Enter);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "FL", "Marion");
                        Thread.Sleep(2000);

                        try
                        {
                            IWebElement MultiAddressTB = driver.FindElement(By.XPath("//*[@id='srch']/table[1]/tbody"));
                            IList<IWebElement> MultiAddressTR = MultiAddressTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiAddressTD;
                            gc.CreatePdf_WOP(orderNumber, "Multi Address search", driver, "FL", "Marion");

                            if (MultiAddressTR.Count == 1)
                            {
                                driver.FindElement(By.XPath("//*[@id='srch']/table[1]/tbody/tr/td[1]/a")).Click();
                                Thread.Sleep(4000);
                            }

                            else
                            {
                                int AddressmaxCheck = 0;
                                string xpath = "";
                                int m = 1;
                                foreach (IWebElement MultiAddress in MultiAddressTR)
                                {
                                    if (AddressmaxCheck <= 25)
                                    {
                                        MultiAddressTD = MultiAddress.FindElements(By.TagName("td"));
                                        if (MultiAddressTD.Count != 0 && MultiAddress.Text.Contains(address.ToUpper()))
                                        {
                                            IWebElement Parcelno1 = MultiAddressTD[0].FindElement(By.TagName("a"));
                                            Parcelno = Parcelno1.Text;
                                            Owner = MultiAddressTD[1].Text;
                                            Property_Address = MultiAddressTD[2].Text;

                                            MultiAddress_details = Owner + "~" + Property_Address;
                                            gc.insert_date(orderNumber, Parcelno, 672, MultiAddress_details, 1, DateTime.Now);

                                            xpath = "//*[@id='srch']/table[1]/tbody/tr[" + m + "]/td[1]/a";
                                            mcheck.Add(MultiAddressTD[0].Text.Trim());
                                        }
                                        m++;
                                    }
                                    AddressmaxCheck++;
                                }
                                if (mcheck.Count == 1)
                                {
                                    driver.FindElement(By.XPath(xpath)).Click();
                                    Thread.Sleep(4000);
                                }
                                else
                                {
                                    if (MultiAddressTR.Count > 25)
                                    {
                                        HttpContext.Current.Session["multiParcel_FLMarion_Multicount"] = "Maximum";
                                        driver.Quit();
                                        return "Maximum";
                                    }
                                    else
                                    {
                                        HttpContext.Current.Session["multiparcel_FLMarion"] = "Yes";
                                        driver.Quit();
                                        return "MultiParcel";
                                    }
                                }
                                
                            }

                        }
                        catch
                        { }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("srch")).Text;
                            if (nodata.Contains("0 records found. End of search reached."))
                            {
                                HttpContext.Current.Session["Nodata_FLMarion"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("http://www.pa.marion.fl.us/PropertySearch.aspx");
                        Thread.Sleep(2000);

                        driver.FindElement(By.Id("MCPAMaster_MCPAContent_rblSearchBy_2")).Click();
                        Thread.Sleep(2000);

                        driver.FindElement(By.Id("MCPAMaster_MCPAContent_txtParm")).SendKeys(parcelNumber);

                        driver.FindElement(By.Id("MCPAMaster_MCPAContent_btnWine")).SendKeys(Keys.Enter);
                        gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearch", driver, "FL", "Marion");
                        Thread.Sleep(2000);

                        driver.FindElement(By.XPath("//*[@id='srch']/table[1]/tbody/tr/td[1]/a")).Click();
                        Thread.Sleep(2000);
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.Id("srch'")).Text;
                            if (nodata.Contains("0 records found. End of search reached."))
                            {
                                HttpContext.Current.Session["Nodata_FLMarion"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    if (searchType == "ownername")
                    {
                        var chromeOptions = new ChromeOptions();
                        var chDriver = new ChromeDriver(chromeOptions);

                        chDriver.Navigate().GoToUrl("http://www.pa.marion.fl.us/PropertySearch.aspx");
                        Thread.Sleep(2000);

                        chDriver.FindElement(By.Id("MCPAMaster_MCPAContent_rblSearchBy_1")).Click();
                        Thread.Sleep(2000);

                        chDriver.FindElement(By.Id("MCPAMaster_MCPAContent_txtParm")).SendKeys(ownername);

                        chDriver.FindElement(By.Id("MCPAMaster_MCPAContent_btnWine")).SendKeys(Keys.Enter);
                        gc.CreatePdf_WOP(orderNumber, "Owner search", chDriver, "FL", "Marion");
                        Thread.Sleep(2000);

                        try
                        {

                            IWebElement MultiOwnerTB = chDriver.FindElement(By.XPath("//*[@id='srch']/table[1]/tbody"));
                            IList<IWebElement> MultiOwnerTR = MultiOwnerTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiOwnerTD;
                            gc.CreatePdf_WOP(orderNumber, "Multi Owner search", chDriver, "FL", "Marion");

                            if (MultiOwnerTR.Count == 1)
                            {
                                chDriver.FindElement(By.XPath("//*[@id='srch']/table[1]/tbody/tr/td[1]/a")).Click();
                                Thread.Sleep(2000);
                            }

                            else
                            {
                                int OwnermaxCheck = 0;
                                foreach (IWebElement MultiOwner in MultiOwnerTR)
                                {
                                    if (OwnermaxCheck <= 25)
                                    {
                                        MultiOwnerTD = MultiOwner.FindElements(By.TagName("td"));
                                        if (MultiOwnerTD.Count != 0)
                                        {
                                            IWebElement Parcelno1 = MultiOwnerTD[0].FindElement(By.TagName("a"));
                                            Parcelno = Parcelno1.Text;
                                            Owner = MultiOwnerTD[1].Text;
                                            Property_Address = MultiOwnerTD[2].Text;

                                            MultiOwner_details = Owner + "~" + Property_Address;
                                            gc.insert_date(orderNumber, Parcelno, 672, MultiOwner_details, 1, DateTime.Now);
                                        }
                                        OwnermaxCheck++;
                                    }
                                }
                                if (MultiOwnerTR.Count > 25)
                                {
                                    HttpContext.Current.Session["multiParcel_FLMarion_Multicount"] = "Maximum";
                                }
                                else
                                {
                                    HttpContext.Current.Session["multiparcel_FLMarion"] = "Yes";
                                }
                                chDriver.Quit();

                                return "MultiParcel";
                            }
                        }
                        catch
                        { }

                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='srch']/text()")).Text;
                            if (nodata.Contains("0 records found. End of search reached."))
                            {
                                HttpContext.Current.Session["Nodata_FLMarion"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                        chDriver.Quit();

                    }

                    Thread.Sleep(3000);
                    driver.SwitchTo().Window(driver.WindowHandles.Last());
                    Thread.Sleep(4000);

                    //Property Details
                    //try
                    //{
                    //    driver.FindElement(By.XPath("//*[@id='prc']/div/a[1]")).Click();
                    //    Thread.Sleep(6000);
                    //}
                    //catch
                    //{ }


                    try
                    {
                        ParcelID = driver.FindElement(By.XPath("//*[@id='prc']/table/tbody/tr/td/h1")).Text;
                        Primary_Key = driver.FindElement(By.XPath("//*[@id='prc']/table/tbody/tr/td/table[1]/tbody/tr/td[1]")).Text;
                        Primary_Key = WebDriverTest.After(Primary_Key, "Prime Key: ");
                        Owner_Mailing = driver.FindElement(By.XPath("//*[@id='prc']/table/tbody/tr/td/table[2]/tbody/tr/td[1]")).Text;

                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='prc']/div/a[2]")).Click();
                            Thread.Sleep(2000);
                            full = driver.FindElement(By.XPath("//*[@id='prc']/table/tbody/tr/td/table[2]/tbody/tr/td[2]")).Text;
                            Taxes_Assments = gc.Between(full, "Taxes / Assessments:", "Map ID:");
                            try
                            {
                                assess1 = driver.FindElement(By.XPath("//*[@id='prc']/table/tbody/tr/td/table[3]/tbody/tr/td[1]")).Text.Replace("\r\n", "~");
                                assess11 = driver.FindElement(By.XPath("//*[@id='prc']/table/tbody/tr/td/table[3]/tbody/tr/td[2]")).Text.Replace("\r\n", "~");

                                DBconnection dbconn = new DBconnection();
                                dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + assess1 + "' where Id = '" + 692 + "'");
                                gc.CreatePdf(orderNumber, ParcelID, "Assessment1 Details", driver, "FL", "Marion");
                                gc.insert_date(orderNumber, ParcelID, 692, assess11, 1, DateTime.Now);
                            }
                            catch
                            { }

                            //History Accessd Values
                            try
                            {
                                IWebElement HistoryTB = driver.FindElement(By.XPath("//*[@id='prc']/table/tbody/tr/td/table[4]/tbody"));
                                IList<IWebElement> HistoryTR = HistoryTB.FindElements(By.TagName("tr"));
                                IList<IWebElement> HistoryTD;

                                foreach (IWebElement History in HistoryTR)
                                {
                                    HistoryTD = History.FindElements(By.TagName("td"));
                                    if (HistoryTD.Count != 0 && HistoryTR.Count != 1)
                                    {
                                        Year = HistoryTD[0].Text;
                                        Land_Just = HistoryTD[1].Text;
                                        Building = HistoryTD[2].Text;
                                        Misc_Value = HistoryTD[3].Text;
                                        Mkt_Just = HistoryTD[4].Text;
                                        Assed_Value = HistoryTD[5].Text;
                                        Exceptions = HistoryTD[6].Text;
                                        Taxabl_Val = HistoryTD[7].Text;

                                        History_details = Year + "~" + Land_Just + "~" + Building + "~" + Misc_Value + "~" + Mkt_Just + "~" + Assed_Value + "~" + Exceptions + "~" + Taxabl_Val;
                                        gc.insert_date(orderNumber, ParcelID, 693, History_details, 1, DateTime.Now);
                                    }
                                }
                            }
                            catch
                            { }
                            driver.Navigate().Back();
                            Thread.Sleep(5000);
                        }
                        catch
                        { }
                        Map_Id = gc.Between(full, "Map ID:", "Millage:");
                        Millage = GlobalClass.After(full, "Millage:");
                        full1 = driver.FindElement(By.XPath("//*[@id='prc']/table/tbody/tr/td/table[2]/tbody/tr/td[3]")).Text;
                        try
                        {
                            Acres = gc.Between(full1, "Acres: ", "Situs: ").Replace("More Situs", "");
                            Situs = WebDriverTest.After(full1, "Situs: ");
                        }
                        catch { }

                        Proty_Desp = driver.FindElement(By.XPath("//*[@id='prc']/table/tbody/tr/td")).Text;
                        Proty_Desp = gc.Between(Proty_Desp, "Property Description", "Land Data - Warning: Verify Zoning");
                        try
                        {
                            Improve1 = driver.FindElement(By.XPath("//*[@id='prc']/table/tbody/tr/td/table[6]/tbody/tr/td[1]/table/tbody/tr/td[2]")).Text.Replace("\r\n", "~");

                            string[] rowarrayname1 = Improve1.Split('~');

                            Improve = rowarrayname1[0];
                            Year_Built = driver.FindElement(By.XPath("//*[@id='prc']/table/tbody/tr/td/table[6]/tbody/tr/td[2]")).Text;
                            Year_Built = gc.Between(Year_Built, "Year Built", "Physical Deterioration");
                        }
                        catch
                        { }

                        property = Primary_Key + "~" + Owner_Mailing + "~" + Situs + "~" + Taxes_Assments + "~" + Map_Id + "~" + Millage + "~" + Acres + "~" + Proty_Desp + "~" + Improve + "~" + Year_Built;
                        gc.CreatePdf(orderNumber, ParcelID, "Assessment11 Details", driver, "FL", "Marion");
                        gc.insert_date(orderNumber, ParcelID, 691, property, 1, DateTime.Now);
                    }
                    catch
                    { }

                    //try
                    //{
                    //    assess = driver.FindElement(By.XPath("//*[@id='prc']/table/tbody/tr/td/table[3]/tbody/tr/td[1]")).Text.Replace("\r\n", "~");
                    //    assess1 = driver.FindElement(By.XPath("//*[@id='prc']/table/tbody/tr/td/table[3]/tbody/tr/td[2]")).Text.Replace("\r\n", "~");

                    //    DBconnection dbconn = new DBconnection();
                    //    dbconn.ExecuteQuery("update data_field_master set Data_Fields_Text='" + assess + "' where Id = '" + 692 + "'");
                    //    gc.insert_date(orderNumber, ParcelID, 692, assess1, 1, DateTime.Now);
                    //}
                    //catch
                    //{ }

                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    //Tax Details
                    driver.Navigate().GoToUrl("https://www.mariontax.com/itm/PropertySearchAccount.aspx");
                    Thread.Sleep(4000);

                    driver.FindElement(By.Id("btnAgree")).Click();
                    Thread.Sleep(4000);

                    driver.FindElement(By.Id("_ctl0_ContentPlaceHolder1_txtAccount")).SendKeys(ParcelID);

                    gc.CreatePdf(orderNumber, ParcelID, "Tax Details", driver, "FL", "Marion");

                    driver.FindElement(By.Id("_ctl0_ContentPlaceHolder1_btnSearch")).SendKeys(Keys.Enter);
                    Thread.Sleep(4000);

                    gc.CreatePdf(orderNumber, ParcelID, "Tax Parcels Details", driver, "FL", "Marion");

                    driver.FindElement(By.Id("_ctl0_ContentPlaceHolder1_rptSummary__ctl0_lnkDetails1")).Click();
                    Thread.Sleep(4000);


                    //Tax Payment History
                    try
                    {
                        IWebElement BreakdownTB = driver.FindElement(By.XPath("//*[@id='_ctl0_ContentPlaceHolder1_tblSummary']/tbody"));
                        IList<IWebElement> BreakdownTR = BreakdownTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> BreakdownTD;

                        foreach (IWebElement Breakdown in BreakdownTR)
                        {
                            BreakdownTD = Breakdown.FindElements(By.TagName("td"));
                            if (BreakdownTD.Count != 0 && !Breakdown.Text.Contains("Year"))
                            {
                                Tax_Year = BreakdownTD[0].Text;
                                Roll = BreakdownTD[1].Text;
                                Acc_Num = BreakdownTD[2].Text;
                                Sta = BreakdownTD[3].Text;
                                Date_Paid = BreakdownTD[4].Text;
                                Amount_Paid = BreakdownTD[5].Text;
                                Balnc_Due = BreakdownTD[6].Text;

                                Payment_details = Tax_Year + "~" + Roll + "~" + Acc_Num + "~" + Sta + "~" + Date_Paid + "~" + Amount_Paid + "~" + Balnc_Due;
                                gc.insert_date(orderNumber, ParcelID, 695, Payment_details, 1, DateTime.Now);
                            }
                        }
                    }
                    catch
                    { }

                    //Tax Information
                    Acc_No = driver.FindElement(By.XPath("//*[@id='tbl1']/tbody/tr[2]/td[2]")).Text.Replace("               ", "");
                    Typ = driver.FindElement(By.XPath("//*[@id='tbl1']/tbody/tr[2]/td[4]/table/tbody/tr/td[1]")).Text;
                    Add = driver.FindElement(By.XPath("//*[@id='tbl1']/tbody/tr[3]/td[2]")).Text;

                    try
                    {
                        Deliquent = driver.FindElement(By.XPath("//*[@id='tbl3']/tbody/tr[4]/td")).Text;
                    }
                    catch
                    { }

                    try
                    {
                        Roll_Coment = driver.FindElement(By.XPath("//*[@id='_ctl0_ContentPlaceHolder1_tblSummary']/tbody/tr[3]/td[2]")).Text;
                    }
                    catch
                    { }
                    List<string> ParcelSearch = new List<string>();

                    try
                    {
                        IWebElement ParcelTB = driver.FindElement(By.XPath("//*[@id='_ctl0_ContentPlaceHolder1_tblSummary']/tbody"));
                        IList<IWebElement> ParcelTR = ParcelTB.FindElements(By.TagName("tr"));
                        ParcelTR.Reverse();
                        int rows_count = ParcelTR.Count;

                        for (int row = 0; row < rows_count; row++)
                        {
                            if (row == rows_count - 1 || row == rows_count - 2 || row == rows_count - 3)
                            {
                                IList<IWebElement> Columns_row = ParcelTR[row].FindElements(By.TagName("td"));

                                int columns_count = Columns_row.Count;

                                for (int column = 0; column < columns_count; column++)
                                {
                                        if (column == columns_count - 2)
                                        {
                                            IWebElement ParcelBill_link = Columns_row[0].FindElement(By.TagName("a"));
                                            string Parcelurl = ParcelBill_link.GetAttribute("href");
                                            ParcelSearch.Add(Parcelurl);
                                        }
                                }
                            }
                        }
                    }
                    catch
                    { }

                    foreach (string Parcelbill in ParcelSearch)
                    {

                        driver.Navigate().GoToUrl(Parcelbill);
                        Thread.Sleep(3000);

                        Taxy = driver.FindElement(By.XPath("//*[@id='_ctl0_ContentPlaceHolder1_lblDetTaxYear']")).Text;

                        try
                        {
                            Pdes1 = driver.FindElement(By.XPath("//*[@id='Table2a']/tbody/tr[1]/td")).Text;
                            Pdes2 = driver.FindElement(By.XPath("//*[@id='Table2a']/tbody/tr[2]/td")).Text;
                            Pdes3 = driver.FindElement(By.XPath("//*[@id='Table2a']/tbody/tr[3]/td")).Text;
                            Pdes4 = driver.FindElement(By.XPath("//*[@id='Table2a']/tbody/tr[4]/td")).Text;

                            P_des = Pdes1 + " " + Pdes2 + " " + Pdes3 + " " + Pdes4;
                        }
                        catch
                        { }
                       

                        try
                        {
                            OInfo1 = driver.FindElement(By.XPath("//*[@id='Table2b']/tbody/tr[1]/td")).Text;
                            M_Addr1 = driver.FindElement(By.XPath("//*[@id='Table2b']/tbody/tr[2]/td")).Text;
                            M_Addr2 = driver.FindElement(By.XPath("//*[@id='Table2b']/tbody/tr[3]/td")).Text;
                            Owner_Mail = OInfo1 + " & " + M_Addr1 + " " + M_Addr2;
                        }
                        catch
                        { }

                        try
                        {
                            OInfo1 = driver.FindElement(By.XPath("//*[@id='Table2b']/tbody/tr[1]/td")).Text;
                            OInfo2 = driver.FindElement(By.XPath("//*[@id='Table2b']/tbody/tr[2]/td")).Text;
                            M_Addr1 = driver.FindElement(By.XPath("//*[@id='Table2b']/tbody/tr[3]/td")).Text;
                            M_Addr2 = driver.FindElement(By.XPath("//*[@id='Table2b']/tbody/tr[4]/td")).Text;
                            Owner_Mail = OInfo1 + " " + OInfo2 + " & " + M_Addr1 + " " + M_Addr2;
                        }
                        catch
                        { }

                        try
                        {
                            IWebElement tbmulti11 = driver.FindElement(By.XPath("//*[@id='Table2c']/tbody"));
                            IList<IWebElement> TRmulti11 = tbmulti11.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDmulti11;

                            foreach (IWebElement row in TRmulti11)
                            {
                                TDmulti11 = row.FindElements(By.TagName("td"));
                                if (TDmulti11.Count != 0)
                                {
                                    Mkt = TDmulti11[0].Text;
                                    if (Mkt.Contains("MARKET VALU"))
                                    {
                                        Markt_Value = TDmulti11[0].Text.Replace(" ", "");
                                        Markt_Value = WebDriverTest.After(Markt_Value, "MARKETVALU");
                                    }
                                    Asst = TDmulti11[0].Text;
                                    if (Asst.Contains("ASSESSMENT"))
                                    {
                                        ASSESSMENT = TDmulti11[0].Text.Replace(" ", "");
                                        ASSESSMENT = WebDriverTest.After(ASSESSMENT, "ASSESSMENT");
                                    }
                                    Txb = TDmulti11[0].Text;
                                    if (Txb.Contains("TAXABLE"))
                                    {
                                        TAXABLE = TDmulti11[0].Text.Replace(" ", "");
                                        TAXABLE = WebDriverTest.After(TAXABLE, "TAXABLE");
                                    }
                                    Ext = TDmulti11[0].Text;
                                    if (Ext.Contains("EXCD01"))
                                    {
                                        EXC = TDmulti11[0].Text.Replace(" ", "");
                                        EXC = WebDriverTest.After(EXC, "EXCD01");
                                    }
                                    Ext1 = TDmulti11[0].Text;
                                    if (Ext1.Contains("EXCD38"))
                                    {
                                        EXC1 = TDmulti11[0].Text.Replace(" ", "");
                                        EXC1 = WebDriverTest.After(EXC1, "EXCD38");
                                    }
                                    Ext2 = TDmulti11[0].Text;
                                    if (Ext2.Contains("EXCD02"))
                                    {
                                        EXC2 = TDmulti11[0].Text.Replace(" ", "");
                                        EXC2 = WebDriverTest.After(EXC2, "EXCD02");
                                    }
                                    Count_Asst = TDmulti11[0].Text;
                                    if (Count_Asst.Contains("COUNTY ASMT"))
                                    {
                                        County_ASMT = TDmulti11[0].Text.Replace(" ", "");
                                        County_ASMT = WebDriverTest.After(County_ASMT, "COUNTYASMT");
                                    }
                                    Count_txbl = TDmulti11[0].Text;
                                    if (Count_txbl.Contains("COUNTY TXBL"))
                                    {
                                        County_TXBL = TDmulti11[0].Text.Replace(" ", "");
                                        County_TXBL = WebDriverTest.After(County_TXBL, "COUNTYTXBL");
                                    }
                                    Tax_Asst = TDmulti11[0].Text;
                                    if (Tax_Asst.Contains("SCHOOL ASMT"))
                                    {
                                        Tax_ASMT = TDmulti11[0].Text.Replace(" ", "");
                                        Tax_ASMT = WebDriverTest.After(Tax_ASMT, "SCHOOLASMT");
                                    }
                                    Tax_txbl = TDmulti11[0].Text;
                                    if (Tax_txbl.Contains("SCHOOL TXBL"))
                                    {
                                        Tax_TXBL = TDmulti11[0].Text.Replace(" ", "");
                                        Tax_TXBL = WebDriverTest.After(Tax_TXBL, "SCHOOLTXBL");
                                    }
                                }
                            }

                            IWebElement tbmulti1 = driver.FindElement(By.XPath("//*[@id='Table2d']/tbody"));
                            IList<IWebElement> TRmulti1 = tbmulti1.FindElements(By.TagName("tr"));
                            IList<IWebElement> TDmulti1;

                            foreach (IWebElement row in TRmulti1)
                            {
                                TDmulti1 = row.FindElements(By.TagName("td"));
                                if (TDmulti1.Count != 0)
                                {
                                    txes = TDmulti1[0].Text;
                                    if (txes.Contains("TAXES"))
                                    {
                                        TAXES = TDmulti1[0].Text.Replace(" ", "");
                                        TAXES = WebDriverTest.After(TAXES, "TAXES");
                                    }

                                    sptxs = TDmulti1[0].Text;
                                    if (sptxs.Contains("SP. ASMT"))
                                    {
                                        SP_ASMT = TDmulti1[0].Text.Replace(" ", "");
                                        SP_ASMT = WebDriverTest.After(SP_ASMT, "SP.ASMT");
                                    }

                                    intt = TDmulti1[0].Text;
                                    if (intt.Contains("INT."))
                                    {
                                        INT = TDmulti1[0].Text.Replace(" ", "");
                                        INT = WebDriverTest.After(INT, "INT.");
                                    }

                                    Salet = TDmulti1[0].Text;
                                    if (Salet.Contains("SALE 5%"))
                                    {
                                        SALE = TDmulti1[0].Text.Replace(" ", "");
                                        SALE = WebDriverTest.After(SALE, "SALE5%");
                                    }

                                    Advt = TDmulti1[0].Text;
                                    if (Advt.Contains("ADV. FEE"))
                                    {
                                        ADV = TDmulti1[0].Text.Replace(" ", "");
                                        ADV = WebDriverTest.After(ADV, "ADV.FEE");
                                    }
                                }
                            }

                            Tax_Info = Acc_No + "~" + Typ + "~" + Add + "~" + Taxy + "~" + P_des + "~" + Owner_Mail + "~" + Markt_Value + "~" + ASSESSMENT + "~" + TAXABLE + "~" + EXC + "~" + EXC1 + "~" + EXC2 + "~" + TAXES + "~" + SP_ASMT + "~" + INT + "~" + SALE + "~" + ADV + "~" + County_ASMT + "~" + County_TXBL + "~" + Tax_ASMT + "~" + Tax_TXBL;
                            gc.insert_date(orderNumber, ParcelID, 697, Tax_Info, 1, DateTime.Now);
                            Markt_Value = ""; ASSESSMENT = ""; TAXABLE = ""; EXC = ""; EXC1 = ""; EXC2 = ""; TAXES = ""; SP_ASMT = ""; INT = ""; SALE = ""; ADV = ""; County_ASMT = ""; County_TXBL = ""; Tax_ASMT = ""; Tax_TXBL = "";
                        }
                        catch
                        { }
                        gc.CreatePdf(orderNumber, ParcelID, "Tax Payment Details" + Taxy, driver, "FL", "Marion");
                        Thread.Sleep(2000);

                        //Tax Receipt           
                        try
                        {
                            Receipt = driver.FindElement(By.XPath("//*[@id='tbl5']/tbody/tr[4]/td")).Text.Replace(" ", "");

                            Commonreceipt = Receipt;

                            if (Commonreceipt.Length == 58)
                            {
                                A = Receipt.Substring(0, 10);
                                B = Receipt.Substring(10, 19);
                                C = Receipt.Substring(29, 9);
                                D = Receipt.Substring(38, 7);
                                E = Receipt.Substring(45, 4);
                                F = Receipt.Substring(49, 9);

                                Receipt_Details5 = A + "~" + B + "~" + "~" + C + "~" + D + "~" + E + "~" + F;
                                gc.insert_date(orderNumber, ParcelID, 698, Receipt_Details5, 1, DateTime.Now);
                            }

                            if (Commonreceipt.Length == 42)
                            {
                                A = Receipt.Substring(0, 10);
                                B = Receipt.Substring(10, 19);
                                C = Receipt.Substring(29, 4);
                                D = Receipt.Substring(33, 9);

                                Receipt_Details1 = A + "~" + B + "~" + C + "~" + D + "~" + "" + "~" + "" + "~" + "";
                                gc.insert_date(orderNumber, ParcelID, 698, Receipt_Details1, 1, DateTime.Now);
                            }

                            if (Commonreceipt.Length == 62)
                            {
                                A = Receipt.Substring(0, 10);
                                B = Receipt.Substring(10, 19);
                                C = Receipt.Substring(29, 4);
                                D = Receipt.Substring(33, 9);
                                E = Receipt.Substring(42, 7);
                                F = Receipt.Substring(49, 4);
                                G = Receipt.Substring(53, 9);

                                Receipt_Details2 = A + "~" + B + "~" + C + "~" + D + "~" + E + "~" + F + "~" + G;
                                gc.insert_date(orderNumber, ParcelID, 698, Receipt_Details2, 1, DateTime.Now);
                            }

                            if (Commonreceipt.Length == 61)
                            {
                                A = Receipt.Substring(0, 10);
                                B = Receipt.Substring(10, 19);
                                C = Receipt.Substring(29, 4);
                                D = Receipt.Substring(33, 10);
                                E = Receipt.Substring(43, 6);
                                F = Receipt.Substring(49, 5);
                                G = Receipt.Substring(54, 7);

                                Receipt_Details3 = A + "~" + B + "~" + C + "~" + D + "~" + E + "~" + F + "~" + G;
                                gc.insert_date(orderNumber, ParcelID, 698, Receipt_Details3, 1, DateTime.Now);
                            }

                            if (Commonreceipt.Length == 60)
                            {
                                A = Receipt.Substring(0, 10);
                                B = Receipt.Substring(10, 19);
                                C = Receipt.Substring(29, 4);
                                D = Receipt.Substring(33, 9);
                                E = Receipt.Substring(42, 7);
                                F = Receipt.Substring(49, 4);
                                G = Receipt.Substring(53, 7);

                                Receipt_Details4 = A + "~" + B + "~" + C + "~" + D + "~" + E + "~" + F + "~" + G;
                                gc.insert_date(orderNumber, ParcelID, 698, Receipt_Details4, 1, DateTime.Now);
                            }

                            if (Commonreceipt.Length == 56)
                            {
                                A = Receipt.Substring(0, 10);
                                B = Receipt.Substring(10, 19);
                                C = Receipt.Substring(29, 9);
                                D = Receipt.Substring(38, 7);
                                E = Receipt.Substring(45, 4);
                                F = Receipt.Substring(49, 7);

                                Receipt_Details4 = A + "~" + B + "~" + "" + "~" + C + "~" + D + "~" + E + "~" + F;
                                gc.insert_date(orderNumber, ParcelID, 698, Receipt_Details4, 1, DateTime.Now);
                            }
                            if (Commonreceipt.Length == 64)
                            {
                                A = Receipt.Substring(0, 10);
                                B = Receipt.Substring(10, 19);
                                C = Receipt.Substring(29, 4);
                                D = Receipt.Substring(33, 9);
                                E = Receipt.Substring(42, 8);
                                F = Receipt.Substring(50, 4);
                                G = Receipt.Substring(54, 10);

                                Receipt_Details9 = A + "~" + B + "~" + C + "~" + D + "~" + E + "~" + F + "~" + G;
                                gc.insert_date(orderNumber, ParcelID, 698, Receipt_Details9, 1, DateTime.Now);
                            }
                        }
                        catch
                        { }

                        //driver.Navigate().Back();
                        //Thread.Sleep(2000);
                    }

                    driver.FindElement(By.Id("_ctl0_ContentPlaceHolder1_lnkAcctBill2")).Click();
                    Thread.Sleep(3000);

                    driver.SwitchTo().Window(driver.WindowHandles.Last());
                    Thread.Sleep(4000);

                    //Ad-Valorem & Non-Ad Valorem
                    IWebElement AdValoremTB = driver.FindElement(By.XPath("//*[@id='Table3']/tbody"));
                    IList<IWebElement> AdValoremTR = AdValoremTB.FindElements(By.TagName("tr"));
                    IList<IWebElement> AdValoremTD;

                    foreach (IWebElement AdValorem in AdValoremTR)
                    {

                        AdValoremTD = AdValorem.FindElements(By.TagName("td"));
                        if (AdValoremTD.Count != 0 && !AdValorem.Text.Contains("TAXING AUTHORITY") && AdValoremTD[0].Text != "" && !AdValorem.Text.Contains("*** PAID *** PAID *** PAID ***") && !AdValorem.Text.Contains("PERIOD"))
                        {
                            //if (AdValorem.Text.Contains("COMBINED TAXES & ASSESSMENTS TOTAL:"))
                            //{
                            //    break;
                            //}
                            string Output = AdValoremTD[0].Text;
                            Output = Regex.Replace(Output, @"\s+", " ").TrimStart();
                            Output = Output.Replace(" ", "~");
                            string[] rowarrayname1 = Output.Split('~');
                            List<string> listname = new List<string>();
                            listname.AddRange(rowarrayname1);
                            int count = listname.Count();
                            string name = "", millege = "", assessed = "", exemptions = "", taxable = "", taxes = "";
                            int k = 0;
                            if (count == 2 && listname[0].Any(Char.IsDigit))
                            {
                                Advalorem_details1 = "Total Millege" + "~" + listname[0] + "~" + "" + "~" + "" + "~" + "Total Ad Valorem Taxes" + "~" + listname[1];
                                gc.CreatePdf(orderNumber, ParcelID, "Ad-Valorem & Non-Ad Valorem Taxs Details" + Taxy, driver, "FL", "Marion");
                                gc.insert_date(orderNumber, ParcelID, 700, Advalorem_details1, 1, DateTime.Now);
                            }
                            else
                            {
                                for (int i = 0; i < count; i++)
                                {
                                    if (!listname[i].Any(Char.IsDigit))
                                    {
                                        name = name + " " + listname[i];
                                    }
                                    else
                                    {
                                        if (listname[i].Any(Char.IsDigit) && (k == 0))
                                        {
                                            millege = listname[i];

                                        }
                                        if (listname[i].Any(Char.IsDigit) && (k == 1))
                                        {
                                            assessed = listname[i];

                                        }
                                        if (listname[i].Any(Char.IsDigit) && (k == 2))
                                        {
                                            exemptions = listname[i];

                                        }
                                        if (listname[i].Any(Char.IsDigit) && (k == 3))
                                        {
                                            taxable = listname[i];

                                        }
                                        if (listname[i].Any(Char.IsDigit) && (k == 4))
                                        {
                                            taxes = listname[i];

                                        }
                                        k++;
                                    }
                                }
                            }
                            Advalorem_details = name + "~" + millege + "~" + assessed + "~" + exemptions + "~" + taxable + "~" + taxes;
                            gc.CreatePdf(orderNumber, ParcelID, "Ad-Valorem & Non-Ad Valorem Taxs Details" + Taxy, driver, "FL", "Marion");
                            gc.insert_date(orderNumber, ParcelID, 700, Advalorem_details, 1, DateTime.Now);
                        }
                    }

                    //Taxing Authority
                    driver.Navigate().GoToUrl("https://www.mariontax.com/itm.asp");
                    Thread.Sleep(2000);
                    Deliquent_Comments = "";
                    CER_Comments = "";
                    try
                    {
                        Taxing_Authority = driver.FindElement(By.XPath("//*[@id='ss-block-container-1']/div/address[2]")).Text.Replace("\r\n", " ");
                    }
                    catch { }

                    if (Deliquent == "DELINQUENT TAXES DUE          ")
                    {
                        Deliquent_Comments = "For prior tax amount due, you must call the tax Collector's Office.";
                    }

                    if (Roll_Coment == "CER")
                    {
                        CER_Comments = "Tax Sale in property. Need to contact Tax Collector";
                    }
                    Taxauthority_Details = Taxing_Authority + "~" + Deliquent_Comments + "~" + CER_Comments;
                    gc.CreatePdf(orderNumber, ParcelID, "Tax Authority", driver, "FL", "Marion");
                    gc.insert_date(orderNumber, ParcelID, 704, Taxauthority_Details, 1, DateTime.Now);

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "FL", "Marion", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "FL", "Marion");
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