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
using System.Collections.ObjectModel;


namespace ScrapMaricopa.Scrapsource
{
    public class WebDriver_BayFL
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        GlobalClass gc = new GlobalClass();

        public string FTP_BayFL(string streetno, string direction, string streetname, string streettype, string unitno, string ownername, string parcelNumber, string searchType, string orderNumber)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            List<string> multiparcel = new List<string>();
            string Parcel_number = "", Tax_Authority = "", Year = "", Adderess = "", YearBuilt = "", taxresult3 = "", Firsthalf = "", Secondhalf = "", fullhalf = "", prepayresult1 = "", prepayresult2 = "", prepayresult3 = "", paiedamt = "";
            string outparcelno = "", taxparcel = "", mailingaddress = "", tax_district = "", siteaddr = "", legal_desc = "", year_built = "", propuse = "", par1 = "", par2 = "";
            string Building_Value = "", LandValue = "", JustValue = "", cctaxyear = "", Cap = "", AssessedValue = "", Exemption = "", TaxableValue = "";
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string strBill = "-", strBalance = "-", strBillDate = "-", strBillPaid = "-";
            List<string> strissuecertificate = new List<string>();
            List<string> taxhistorylink = new List<string>();
            List<string> downloadlink = new List<string>();
            List<string> taxhistorylinkinst = new List<string>();
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())//
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Navigate().GoToUrl("http://qpublic6.qpublic.net/qp6/fl_search_dw.php?county=fl_bay");
                    Thread.Sleep(2000);
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/ul/li[1]/b/a")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                    }
                    catch { }
                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='appBody']/div[4]/div/div/div[3]/a[1]")).Click();
                        Thread.Sleep(2000);
                    }
                    catch { }
                    string address = "";
                    if (direction != "")
                    {
                        address = streetno + " " + direction + " " + streetname + " " + streettype + "" + unitno;
                    }
                    else
                    {
                        address = streetno + " " + streetname + " " + streettype + "" + unitno;
                    }
                    if (searchType == "titleflex")
                    {

                        gc.TitleFlexSearch(orderNumber, "", "", address.Trim(), "FL", "Bay");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_BayFL"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_txtAddress")).SendKeys(address.Trim());
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "FL", "Bay");
                        driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        try
                        {
                            string Parcelno = "", Owner = "", Property_Address = "", Legal_Desp = "", MultiAddress_details = "";
                            IWebElement MultiAddressTB = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_ctl01_gvwParcelResults']/tbody"));
                            IList<IWebElement> MultiAddressTR = MultiAddressTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiAddressTD;
                            gc.CreatePdf_WOP(orderNumber, "Multi Address search", driver, "FL", "Bay");
                            int AddressmaxCheck = 0;
                            foreach (IWebElement MultiAddress in MultiAddressTR)
                            {
                                if (AddressmaxCheck <= 25)
                                {
                                    MultiAddressTD = MultiAddress.FindElements(By.TagName("td"));
                                    if (MultiAddressTD.Count != 0)
                                    {
                                        Parcelno = MultiAddressTD[1].Text;
                                        Owner = MultiAddressTD[2].Text;
                                        Property_Address = MultiAddressTD[3].Text;
                                        Legal_Desp = MultiAddressTD[4].Text;

                                        MultiAddress_details = Owner + "~" + Property_Address + "~" + Legal_Desp;
                                        gc.insert_date(orderNumber, Parcelno, 1495, MultiAddress_details, 1, DateTime.Now);
                                    }
                                    AddressmaxCheck++;
                                }
                            }
                            if (MultiAddressTR.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_BayFL_Maximum"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiparcel_BayFL"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                        }
                        catch
                        { }
                    }

                    if (searchType == "parcel")
                    {
                        if (parcelNumber.Contains(" ") || parcelNumber.Contains("-"))
                        {
                            parcelNumber = parcelNumber.Replace(" ", "").Replace("-", "");
                        }

                        string CommonParcel = parcelNumber;

                        if (CommonParcel.Length == 9)
                        {
                            driver.FindElement(By.Id("ctlBodyPane_ctl02_ctl01_txtParcelID")).SendKeys(parcelNumber);
                            gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearch", driver, "FL", "Bay");
                            driver.FindElement(By.Id("ctlBodyPane_ctl02_ctl01_btnSearch")).SendKeys(Keys.Enter);
                            Thread.Sleep(2000);
                        }

                        if (CommonParcel.Length == 7)
                        {
                            driver.FindElement(By.Id("ctlBodyPane_ctl02_ctl01_txtParcelID")).SendKeys(parcelNumber);
                            gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearch", driver, "FL", "Bay");
                            driver.FindElement(By.Id("ctlBodyPane_ctl02_ctl01_btnSearch")).SendKeys(Keys.Enter);
                            Thread.Sleep(2000);
                        }
                    }

                    if (searchType == "ownername")
                    {
                        driver.FindElement(By.Id("ctlBodyPane_ctl00_ctl01_txtName")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "Owner search", driver, "FL", "Bay");
                        driver.FindElement(By.Id("ctlBodyPane_ctl00_ctl01_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        try
                        {
                            string Parcelno = "", Owner = "", Property_Address = "", Legal_Desp = "", MultiOwner_details = "";
                            IWebElement MultiOwnerTB = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_ctl01_gvwParcelResults']/tbody"));
                            IList<IWebElement> MultiOwnerTR = MultiOwnerTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiOwnerTD;
                            gc.CreatePdf_WOP(orderNumber, "Multi Owner search", driver, "FL", "Bay");
                            int AddressmaxCheck = 0;
                            foreach (IWebElement MultiOwner in MultiOwnerTR)
                            {
                                if (AddressmaxCheck <= 25)
                                {
                                    MultiOwnerTD = MultiOwner.FindElements(By.TagName("td"));
                                    if (MultiOwnerTD.Count != 0)
                                    {
                                        Parcelno = MultiOwnerTD[1].Text;
                                        Owner = MultiOwnerTD[2].Text;
                                        Property_Address = MultiOwnerTD[3].Text;
                                        Legal_Desp = MultiOwnerTD[4].Text;

                                        MultiOwner_details = Owner + "~" + Property_Address + "~" + Legal_Desp;
                                        gc.insert_date(orderNumber, Parcelno, 1495, MultiOwner_details, 1, DateTime.Now);
                                    }
                                    AddressmaxCheck++;
                                }
                            }
                            if (MultiOwnerTR.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_BayFL_Maximum"] = "Maximum";
                                driver.Quit();
                                return "Maximum";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiparcel_BayFL"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                        }
                        catch
                        { }
                    }

                    try
                    {
                        IWebElement Inodata = driver.FindElement(By.Id("ctlBodyPane_noDataList_pnlNoResults"));
                        if (Inodata.Text.Contains("No results match your search criteria"))
                        {
                            HttpContext.Current.Session["Nodata_BayFL"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }

                    string ParcelID = "", OwnerName = "", MailingAddress = "", Address1 = "", TaxDescription = "", PropertyAddress = "", PropertyUseCode = "", Finalmillagerate = "", Acreage = "", Homestead = "", SectionTown = "", SubDivision = "", TaxDistrict = "", TaxRate = "", Propertyusage = "", ActualYear = "", EffectiveYear = ""; ;
                    IWebElement tbmulti11 = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_mSection']/div/table/tbody"));
                    IList<IWebElement> TRmulti11 = tbmulti11.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDmulti11;
                    foreach (IWebElement row in TRmulti11)
                    {
                        TDmulti11 = row.FindElements(By.TagName("td"));
                        if (TDmulti11.Count != 0)
                        {
                            if (row.Text.Contains("Owner Name"))
                            {
                                OwnerName = TDmulti11[1].Text;
                            }
                            if (row.Text.Contains("Parcel ID"))
                            {
                                ParcelID = TDmulti11[1].Text.Trim();
                            }
                            if (row.Text.Contains("Brief Tax Description"))
                            {
                                TaxDescription = TDmulti11[1].Text;
                            }
                            if (row.Text.Contains("Tax District"))
                            {
                                TaxDistrict = TDmulti11[1].Text;
                            }
                            if (row.Text.Contains("Location Address"))
                            {
                                PropertyAddress = TDmulti11[1].Text;
                            }
                            if (row.Text.Contains("Final Millage Rate"))
                            {
                                Finalmillagerate = TDmulti11[1].Text;
                            }
                            if (row.Text.Contains("Property Use Code"))
                            {
                                PropertyUseCode = TDmulti11[1].Text;
                            }
                            if (row.Text.Contains("Acreage"))
                            {
                                Acreage = TDmulti11[1].Text;
                            }
                            if (row.Text.Contains("Homestead"))
                            {
                                Homestead = TDmulti11[1].Text;
                            }
                            if (row.Text.Contains("Sec/Twp/Rng"))
                            {
                                SectionTown = TDmulti11[1].Text;
                            }
                        }
                    }

                    try
                    {
                        IWebElement Yeartb = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl03_mSection']/div/table/tbody"));
                        IList<IWebElement> YearTR = Yeartb.FindElements(By.TagName("tr"));
                        IList<IWebElement> YearTD;
                        foreach (IWebElement Tax4 in YearTR)
                        {
                            YearTD = Tax4.FindElements(By.TagName("td"));
                            if (YearTD.Count != 0)
                            {
                                if (Tax4.Text.Contains("Year Built") || Tax4.Text.Contains("Actual Year Built"))
                                {
                                    ActualYear = YearTD[1].Text;
                                }
                                if (Tax4.Text.Contains("Effective Year Built"))
                                {
                                    EffectiveYear = YearTD[1].Text;
                                }
                            }
                        }
                    }
                    catch
                    { }

                    gc.CreatePdf(orderNumber, ParcelID, "Property Result", driver, "FL", "Bay");

                    string FirstYear = "", SecondYear = "", Assessment = "", FirstAssessment = "", SecondAssessment = "";
                    //Assessment Details
                    IWebElement AssmThTb = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl02_ctl01_grdValuation']/thead"));
                    IList<IWebElement> AssmThTr = AssmThTb.FindElements(By.TagName("tr"));
                    IList<IWebElement> AssmTh;
                    foreach (IWebElement Assm in AssmThTr)
                    {
                        AssmTh = Assm.FindElements(By.TagName("th"));
                        if (AssmTh.Count != 0)
                        {
                            FirstYear = AssmTh[0].Text;
                            SecondYear = AssmTh[1].Text;
                        }
                    }

                    IWebElement AssmTb = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl02_ctl01_grdValuation']/tbody"));
                    IList<IWebElement> AssmTr = AssmTb.FindElements(By.TagName("tr"));
                    IList<IWebElement> AssmTd;
                    foreach (IWebElement Assmrow in AssmTr)
                    {
                        AssmTd = Assmrow.FindElements(By.TagName("td"));
                        if (AssmTd.Count != 0)
                        {
                            Assessment += AssmTd[0].Text + "~";
                            FirstAssessment += AssmTd[1].Text + "~";
                            SecondAssessment += AssmTd[2].Text + "~";
                        }
                    }
                    db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + "Type~" + Assessment.Remove(Assessment.Length - 1, 1) + "' where Id = '" + 1497 + "'");
                    gc.insert_date(orderNumber, ParcelID, 1497, FirstYear + "~" + FirstAssessment.Remove(FirstAssessment.Length - 1, 1), 1, DateTime.Now);
                    gc.insert_date(orderNumber, ParcelID, 1497, SecondYear + "~" + SecondAssessment.Remove(SecondAssessment.Length - 1, 1), 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    try
                    {
                        IWebElement IOwner = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl01_mSection']/div"));
                        string[] OwnerSplit = IOwner.Text.Split('\n');
                        OwnerName = OwnerSplit[1].Replace("\r", "").Trim();
                    }
                    catch { }

                    //Tax Authority
                    string Taxauthority = "", Taxauthority1 = "";
                    try
                    {
                        driver.Navigate().GoToUrl("https://www.baytaxcollector.com/contacts/");
                        Taxauthority = driver.FindElement(By.XPath("/html/body/div[1]/div/div[3]/div/div/article/section/div[3]/div/div/div/section[2]/div/div/div/header/h4")).Text.Trim();
                    }
                    catch { }
                    //Property Details Insert
                    string propertydetails = PropertyAddress + "~" + OwnerName + "~" + TaxDescription + "~" + PropertyUseCode + "~" + SectionTown + "~" + TaxDistrict + "~" + Finalmillagerate + "~" + Acreage + "~" + Homestead + "~" + ActualYear + "~" + EffectiveYear + "~" + Taxauthority;
                    gc.insert_date(orderNumber, ParcelID, 1496, propertydetails, 1, DateTime.Now);
                    //Property Address~Owner~Legal Description~Property Use Code~Sec/Twp/Rng~Tax District~Final Millage Rate~Acreage~Homestead~Actual Year Built~Effective Year Built~Tax Authority

                    string taxowner = "", tax_addr = "", accno = "", alterkey = "", millagecode = "", milagerate = "", ctax_year = "", cpaiddate = "", strbillyear = "", cpaidamount = "", creceipt = "", combinedtaxamount = "", ceffdate = "", grosstax = "", ifpaidby = "", pleasepay = "";
                    //Tax Information Details
                    driver.Navigate().GoToUrl("https://bay.county-taxes.com/public");
                    driver.FindElement(By.Name("search_query")).SendKeys(ParcelID);
                    gc.CreatePdf(orderNumber, ParcelID, "Tax Search parcel", driver, "FL", "Bay");
                    driver.FindElement(By.XPath("//*[@id='search-controls']/div/form[1]/div[1]/div/span/button")).Click();
                    Thread.Sleep(2000);
                    IWebElement ITaxSearch = driver.FindElement(By.LinkText("Full bill history"));
                    string strITaxSearch = ITaxSearch.GetAttribute("href");
                    driver.Navigate().GoToUrl(strITaxSearch);
                    Thread.Sleep(3000);
                    gc.CreatePdf(orderNumber, ParcelID, "Full Bill History", driver, "FL", "Bay");
                    IWebElement IBillHistorytable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/table"));
                    IList<IWebElement> IBillHistoryRow = IBillHistorytable.FindElements(By.TagName("tr"));
                    IList<IWebElement> IBillHistoryTD;
                    IList<IWebElement> IBillHistoryTH;
                    int i = 0; int m = 0; int j = 0; int k = 0;
                    foreach (IWebElement bill in IBillHistoryRow)
                    {
                        string billyear = "", inst = "", paidamount = "", receipt = "", effectivedate = "";
                        IBillHistoryTD = bill.FindElements(By.TagName("td"));
                        IBillHistoryTH = bill.FindElements(By.TagName("th"));
                        if (IBillHistoryTD.Count != 0)
                        {
                            try
                            {
                                if (IBillHistoryTD[0].Text.Contains("Redeemed certificate") || IBillHistoryTD[0].Text.Contains("Issued certificate") || IBillHistoryTD[0].Text.Contains("Tax Deed Application") || IBillHistoryTD[0].Text.Contains("Expired certificate"))
                                {
                                    if (IBillHistoryTD.Count == 5)
                                    {
                                        //billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                        billyear = IBillHistoryTD[0].Text;
                                        strBalance = IBillHistoryTD[1].Text.Replace("\r\n", " ");
                                        strBillDate = IBillHistoryTD[2].Text;
                                        paidamount = IBillHistoryTD[3].Text;
                                        string strTaxHistory2 = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                        gc.insert_date(orderNumber, ParcelID, 1498, strTaxHistory2, 1, DateTime.Now);
                                    }
                                }

                                if (IBillHistoryTD.Count == 2)
                                {
                                    strBillDate = IBillHistoryTD[0].Text;
                                    paidamount = IBillHistoryTD[1].Text;
                                    if (strBillDate.Contains("Effective"))
                                    {
                                        strBillDate = strBillDate.Replace("Effective ", " ").Trim();
                                        var splitdate = strBillDate.Split(' ');
                                        strBillDate = splitdate[0].Trim();
                                        effectivedate = splitdate[1];
                                    }
                                    if (paidamount.Contains("Paid"))
                                    {
                                        strBillPaid = IBillHistoryTD[1].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                        paidamount = gc.Between(strBillPaid, "Paid ", "Receipt ");
                                        receipt = GlobalClass.After(strBillPaid, "Receipt #");
                                    }
                                    string strTaxHistory2 = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                    gc.insert_date(orderNumber, ParcelID, 1498, strTaxHistory2, 1, DateTime.Now);
                                }
                                if (IBillHistoryTD[2].Text.Contains("Paid"))
                                {
                                    inst = "Total"; paidamount = IBillHistoryTD[2].Text.Replace("Paid", ""); strBalance = "";
                                    string strTaxHistory = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                    gc.insert_date(orderNumber, ParcelID, 1498, strTaxHistory, 1, DateTime.Now);
                                }

                                if (IBillHistoryTD[0].Text.Contains("Issued certificate"))
                                {
                                    IWebElement ITaxBillCount = IBillHistoryTD[0].FindElement(By.TagName("a"));
                                    string issuelink = ITaxBillCount.GetAttribute("href");
                                    strissuecertificate.Add(issuelink);
                                }


                                if (bill.Text.Contains("Annual Bill") || bill.Text.Contains("Pay this bill") || bill.Text.Contains("Installment Bill"))
                                {
                                    if (m < 12)
                                    {
                                        IWebElement ITaxBillCount = IBillHistoryTD[0].FindElement(By.TagName("a"));
                                        strbillyear = GlobalClass.Before(ITaxBillCount.Text, " Annual Bill");
                                        string taxlink = ITaxBillCount.GetAttribute("href");

                                        if (bill.Text.Contains("Annual Bill"))
                                        {
                                            if (j < 3)
                                            {
                                                //download
                                                try
                                                {
                                                    IWebElement ITaxBill = IBillHistoryTD[3].FindElement(By.TagName("a"));
                                                    string BillTax = ITaxBill.GetAttribute("href");
                                                    downloadlink.Add(BillTax);
                                                    string dbill = gc.Between(BillTax, "parcels/", "/bills");
                                                    strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();

                                                    BillDownload(orderNumber, ParcelID, BillTax, dbill, strbillyear);
                                                }
                                                catch { }
                                                try
                                                {
                                                    IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                                    string BillTax = ITaxBill.GetAttribute("href");
                                                    downloadlink.Add(BillTax);
                                                    string dbill = gc.Between(BillTax, "parcels/", "/bills");
                                                    strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                                    string fname = "Tax Bill" + " " + ParcelID + " " + strBill;
                                                    // gc.downloadfile(BillTax, orderNumber, ParcelID, fname, "FL", "Bay");

                                                    BillDownload(orderNumber, ParcelID, BillTax, dbill, strbillyear);
                                                }
                                                catch (Exception ex)
                                                {
                                                }
                                                taxhistorylink.Add(taxlink);

                                            }

                                            if (taxhistorylinkinst.Count != 0 && taxhistorylinkinst.Count < 12 && j < 8 && taxhistorylink.Count < 2)
                                            {
                                                //download 
                                                try
                                                {
                                                    IWebElement ITaxBill = IBillHistoryTD[3].FindElement(By.TagName("a"));
                                                    string BillTax = ITaxBill.GetAttribute("href");
                                                    downloadlink.Add(BillTax);

                                                    string dbill = gc.Between(BillTax, "parcels/", "/bills");
                                                    strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();

                                                    BillDownload(orderNumber, ParcelID, BillTax, dbill, strbillyear);

                                                }
                                                catch { }
                                                try
                                                {
                                                    IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                                    string BillTax = ITaxBill.GetAttribute("href");
                                                    downloadlink.Add(BillTax);
                                                    //string dbill = gc.Between(BillTax, "bills/", "/print");
                                                    string dbill = gc.Between(BillTax, "parcels/", "/bills");
                                                    strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();

                                                    BillDownload(orderNumber, ParcelID, BillTax, dbill, strbillyear);

                                                }
                                                catch (Exception ex)
                                                {
                                                }
                                                //taxhistorylink.Add(taxlink);
                                            }
                                            j++;
                                            k++;
                                            strbillyear = "";
                                        }
                                        else if (bill.Text.Contains("Installment Bill"))
                                        {
                                            if (taxhistorylink.Count == 3)
                                            {
                                                //
                                            }
                                            else if (taxhistorylink.Count == 2)
                                            {
                                                if (j < 4)
                                                {
                                                    //download
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[3].FindElement(By.TagName("a"));
                                                        strbillyear = GlobalClass.Before(ITaxBillCount.Text, " Installment Bill");
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        string dbill = gc.Between(BillTax, "parcels/", "/bills");
                                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();

                                                        BillDownload(orderNumber, ParcelID, BillTax, dbill, strbillyear);


                                                    }
                                                    catch { }
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                                        strbillyear = GlobalClass.Before(ITaxBillCount.Text, " Installment Bill");
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        downloadlink.Add(BillTax);
                                                        string dbill = gc.Between(BillTax, "parcels/", "/bills");
                                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();

                                                        BillDownload(orderNumber, ParcelID, BillTax, dbill, strbillyear);
                                                    }
                                                    catch
                                                    {
                                                    }
                                                    IWebElement ITaxBillCount1 = IBillHistoryTD[0].FindElement(By.TagName("a"));
                                                    string taxlink1 = ITaxBillCount1.GetAttribute("href");
                                                    taxhistorylinkinst.Add(taxlink1);
                                                    j++;
                                                }
                                            }
                                            else if (taxhistorylink.Count == 1)
                                            {
                                                if (j < 7)
                                                {
                                                    //download
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[3].FindElement(By.TagName("a"));
                                                        strbillyear = GlobalClass.Before(ITaxBillCount.Text, " Installment Bill");
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        downloadlink.Add(BillTax);
                                                        string dbill = gc.Between(BillTax, "parcels/", "/bills");
                                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();

                                                        BillDownload(orderNumber, ParcelID, BillTax, dbill, strbillyear);


                                                    }
                                                    catch { }
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                                        strbillyear = GlobalClass.Before(ITaxBillCount.Text, " Installment Bill");
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        downloadlink.Add(BillTax);
                                                        string dbill = gc.Between(BillTax, "parcels/", "/bills");
                                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();

                                                        BillDownload(orderNumber, ParcelID, BillTax, dbill, strbillyear);


                                                    }
                                                    catch
                                                    {
                                                    }


                                                    IWebElement ITaxBillCount1 = IBillHistoryTD[0].FindElement(By.TagName("a"));
                                                    string taxlink1 = ITaxBillCount1.GetAttribute("href");
                                                    taxhistorylinkinst.Add(taxlink1);
                                                    j++;
                                                }
                                            }
                                            else if (taxhistorylink.Count == 0)
                                            {
                                                if (j < 12)
                                                {
                                                    //download
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[3].FindElement(By.TagName("a"));
                                                        strbillyear = GlobalClass.Before(ITaxBillCount.Text, " Installment Bill");
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        downloadlink.Add(BillTax);
                                                        string dbill = gc.Between(BillTax, "parcels/", "/bills");
                                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();

                                                        BillDownload(orderNumber, ParcelID, BillTax, dbill, strbillyear);

                                                    }
                                                    catch { }
                                                    try
                                                    {
                                                        IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                                        strbillyear = GlobalClass.Before(ITaxBillCount.Text, " Installment Bill");
                                                        string BillTax = ITaxBill.GetAttribute("href");
                                                        downloadlink.Add(BillTax);
                                                        string dbill = gc.Between(BillTax, "parcels/", "/bills");
                                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();

                                                        BillDownload(orderNumber, ParcelID, BillTax, dbill, strbillyear);

                                                    }
                                                    catch
                                                    {
                                                    }

                                                    IWebElement ITaxBillCount1 = IBillHistoryTD[0].FindElement(By.TagName("a"));
                                                    string taxlink1 = ITaxBillCount1.GetAttribute("href");
                                                    taxhistorylinkinst.Add(taxlink1);
                                                    j++;
                                                }
                                            }
                                            strbillyear = "";
                                        }
                                        m++;
                                    }
                                    if (bill.Text.Contains("Pay this bill"))
                                    {
                                        //download
                                        try
                                        {
                                            IWebElement ITaxBill = IBillHistoryTD[3].FindElement(By.TagName("a"));
                                            try
                                            {
                                                strbillyear = GlobalClass.Before(ITaxBill.Text, " Annual Bill");
                                            }
                                            catch { }
                                            try
                                            {
                                                strbillyear = GlobalClass.Before(ITaxBill.Text, " Installment Bill");
                                            }
                                            catch { }
                                            string BillTax = ITaxBill.GetAttribute("href");
                                            strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                            downloadlink.Add(BillTax);
                                            string dbill = gc.Between(BillTax, "parcels/", "/bills");
                                            strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();

                                            BillDownload(orderNumber, ParcelID, BillTax, dbill, strbillyear);

                                        }
                                        catch { }
                                        try
                                        {
                                            IWebElement ITaxBill = IBillHistoryTD[4].FindElement(By.TagName("a"));
                                            string BillTax = ITaxBill.GetAttribute("href");
                                            downloadlink.Add(BillTax);
                                            string dbill = gc.Between(BillTax, "parcels/", "/bills");
                                            strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd();

                                            BillDownload(orderNumber, ParcelID, BillTax, dbill, strbillyear);
                                        }
                                        catch
                                        {
                                        }


                                        IWebElement ITaxBillCount = IBillHistoryTD[0].FindElement(By.TagName("a"));
                                        string taxlink = ITaxBillCount.GetAttribute("href");
                                        //taxhistorylink.Add(taxlink);
                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd().Replace("Installment Bill #", "").Replace("Bill", "");
                                        var Splitbill = strBill.Split(' ');
                                        billyear = Splitbill[0]; inst = Splitbill[1];
                                        strBalance = IBillHistoryTD[1].Text.Replace("\r\n", "").TrimStart().TrimEnd();
                                        string strTaxHistory = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                        gc.insert_date(orderNumber, ParcelID, 1498, strTaxHistory, 1, DateTime.Now);

                                        strbillyear = "";
                                    }
                                    else
                                    {

                                        strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd().Replace("Installment Bill #", "").Replace("Bill", "");
                                        var Splitbill1 = strBill.Split(' ');
                                        billyear = Splitbill1[0]; inst = Splitbill1[1];
                                        strBalance = IBillHistoryTD[1].Text.Replace("\r\n", "").TrimStart().TrimEnd();
                                        strBillDate = IBillHistoryTD[2].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                        if (strBillDate.Contains("Effective"))
                                        {
                                            strBillDate = strBillDate.Replace("Effective ", "");
                                            var splitdate = strBillDate.Split(' ');
                                            strBillDate = splitdate[0];
                                            effectivedate = splitdate[1];
                                        }
                                        strBillPaid = IBillHistoryTD[3].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                        paidamount = gc.Between(strBillPaid, "Paid ", "Receipt ");
                                        receipt = GlobalClass.After(strBillPaid, "Receipt #");
                                        string strTaxHistory = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                        gc.insert_date(orderNumber, ParcelID, 1498, strTaxHistory, 1, DateTime.Now);

                                    }


                                    // m++;
                                    strbillyear = "";
                                }

                                else
                                {
                                    strBill = IBillHistoryTD[0].Text.Replace("\r\n", " ").TrimStart().TrimEnd().Replace("Installment Bill #", "").Replace("Bill", "");
                                    var Splitbill = strBill.Split(' ');
                                    billyear = Splitbill[0]; inst = Splitbill[1];
                                    strBalance = IBillHistoryTD[1].Text.Replace("\r\n", "").TrimStart().TrimEnd();
                                    strBillDate = IBillHistoryTD[2].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                    if (strBillDate.Contains("Effective"))
                                    {
                                        strBillDate = strBillDate.Replace("Effective ", "");
                                        var splitdate = strBillDate.Split(' ');
                                        strBillDate = splitdate[0];
                                        effectivedate = splitdate[1];
                                    }
                                    strBillPaid = IBillHistoryTD[3].Text.Replace("\r\n", " ").TrimStart().TrimEnd();
                                    paidamount = gc.Between(strBillPaid, "Paid ", "Receipt ");
                                    receipt = GlobalClass.After(strBillPaid, "Receipt #");
                                    //Tax Year~Installment~Total Due~Paid Date~Effective Date~Paid Amount~Receipt Number
                                    string strTaxHistory = billyear + "~" + inst + "~" + strBalance + "~" + strBillDate + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                    gc.insert_date(orderNumber, ParcelID, 1498, strTaxHistory, 1, DateTime.Now);
                                }

                            }
                            catch
                            {

                            }


                        }
                        if (IBillHistoryTH.Count != 0)
                        {
                            if (IBillHistoryTH[0].Text.Contains("Total Balance"))
                            {
                                inst = "Total Balance"; strBalance = IBillHistoryTH[1].Text.Replace("\r\n", "").TrimStart().TrimEnd();
                                string strTaxHistory = billyear + "~" + inst + "~" + strBalance + "~" + "" + "~" + effectivedate + "~" + paidamount + "~" + receipt;
                                gc.insert_date(orderNumber, ParcelID, 1498, strTaxHistory, 1, DateTime.Now);
                            }
                        }

                        i++;
                    }

                    int q = 0;
                    foreach (string URL in taxhistorylink)
                    {

                        try
                        {
                            accno = ""; milagerate = ""; alterkey = ""; taxowner = ""; tax_addr = ""; millagecode = ""; ctax_year = ""; cctaxyear = ""; combinedtaxamount = ""; grosstax = ""; cpaidamount = ""; cpaiddate = ""; ceffdate = ""; creceipt = "";
                            driver.Navigate().GoToUrl(URL);
                            Thread.Sleep(3000);
                            //*[@id="content"]/div[1]/div[8]/div/div[1]                        
                            cctaxyear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[1]/div[2]")).Text.Trim().Replace("Real Estate", "").Replace("Print this bill (PDF)", "").Replace("\r\n", "");
                            gc.CreatePdf(orderNumber, ParcelID, "Tax Details" + cctaxyear, driver, "FL", "Bay");

                            if (q == 0)
                            {

                                IWebElement valoremtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[2]"));
                                IList<IWebElement> valoremtableRow = valoremtable.FindElements(By.TagName("tr"));
                                int valoremtablerowcount = valoremtableRow.Count;
                                IList<IWebElement> valoremtablerowTD;
                                int d = 0;
                                foreach (IWebElement rowid in valoremtableRow)
                                {
                                    valoremtablerowTD = rowid.FindElements(By.TagName("td"));
                                    if (valoremtablerowTD.Count != 0 && !rowid.Text.Contains("Taxing authority"))
                                    {
                                        string valoremtax = "";
                                        if (d < valoremtablerowcount - 1)
                                        {
                                            if (valoremtablerowTD.Count == 6)
                                            {
                                                //Taxing Authority~Tax Type~Millage~Assessed~Exemption~Taxable~Taxes
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[1].Text + "~" + valoremtablerowTD[2].Text + "~" + valoremtablerowTD[3].Text + "~" + valoremtablerowTD[4].Text + "~" + valoremtablerowTD[5].Text;
                                                gc.insert_date(orderNumber, ParcelID, 1501, valoremtax, 1, DateTime.Now);
                                            }

                                            else if (valoremtablerowTD.Count == 2)
                                            {
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                                gc.insert_date(orderNumber, ParcelID, 1501, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        else if (d == valoremtablerowcount - 1)
                                        {
                                            milagerate = valoremtablerowTD[0].Text;
                                            valoremtax = "Total" + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + valoremtablerowTD[1].Text;
                                            gc.insert_date(orderNumber, ParcelID, 1501, valoremtax, 1, DateTime.Now);
                                        }
                                    }
                                    d++;
                                }

                                //Non Ad-Valorems
                                try
                                {
                                    //*[@id="content"]/div[1]/div[8]/div/table[3]
                                    IWebElement nonvaloremtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[3]"));
                                    IList<IWebElement> nonvaloremtableRow = nonvaloremtable.FindElements(By.TagName("tr"));
                                    int nonvaloremtablerowcount = nonvaloremtableRow.Count;
                                    IList<IWebElement> nonvaloremtablerowTD;
                                    int e = 0;
                                    foreach (IWebElement rowid in nonvaloremtableRow)
                                    {
                                        nonvaloremtablerowTD = rowid.FindElements(By.TagName("td"));
                                        if (nonvaloremtablerowTD.Count != 0 && !rowid.Text.Contains("Levying authority"))
                                        {
                                            string valoremtax = "";
                                            if (e < nonvaloremtablerowcount - 1)
                                            {
                                                //Taxing Authority~Tax Type~Millage~Assessed~Exemption~Taxable~Taxes
                                                valoremtax = nonvaloremtablerowTD[0].Text + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[1].Text + "~" + nonvaloremtablerowTD[2].Text;
                                                gc.insert_date(orderNumber, ParcelID, 1501, valoremtax, 1, DateTime.Now);
                                            }
                                            else if (e == nonvaloremtablerowcount - 1)
                                            {
                                                valoremtax = "Total" + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[0].Text;
                                                gc.insert_date(orderNumber, ParcelID, 1501, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        e++;
                                    }
                                }
                                catch { }
                            }

                            string currenttaxbulktext = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/p")).Text;
                            combinedtaxamount = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/p")).Text.Trim().Replace("Combined taxes and assessments: ", "");
                            try
                            {
                                string bulkgrosstax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]/tbody/tr/td[1]")).Text.Trim();
                                if (bulkgrosstax.Contains("Gross"))
                                {

                                    grosstax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]/tbody/tr/td[1]")).Text.Trim().Replace("Gross", "");
                                    //if paid by                 
                                    string ifpaidbulk = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]/tbody/tr/td[3]")).Text;
                                    string[] stringSeparators1 = new string[] { "\r\n" };
                                    string[] lines1 = ifpaidbulk.Split(stringSeparators1, StringSplitOptions.None);
                                    ifpaidby = lines1[0]; pleasepay = lines1[1];
                                    string single = cctaxyear + "~" + ifpaidby + "~" + pleasepay;
                                    gc.insert_date(orderNumber, ParcelID, 1502, single, 1, DateTime.Now);
                                }

                                else
                                {
                                    string IfPaidBy = "", PlesePay = "", DueDate = "", itaxyear = "";
                                    try
                                    {
                                        IWebElement multitableElement26 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]"));
                                        IList<IWebElement> multitableRow26 = multitableElement26.FindElements(By.TagName("tr"));
                                        IList<IWebElement> multirowTD26;
                                        foreach (IWebElement row in multitableRow26)
                                        {
                                            multirowTD26 = row.FindElements(By.TagName("td"));
                                            int iRowsCount = multirowTD26.Count;
                                            for (int n = 0; n < iRowsCount; n++)
                                            {
                                                if ((!multirowTD26[n].Text.Contains("Face Amt")) && (!multirowTD26[n].Text.Contains("Certificate")))
                                                {
                                                    IfPaidBy = multirowTD26[n].Text.Replace("\r\n", "~");
                                                    var IfpaySplit = IfPaidBy.Split('~');
                                                    IfPaidBy = IfpaySplit[0];
                                                    PlesePay = IfpaySplit[1];
                                                    DueDate = cctaxyear + "~" + IfPaidBy + "~" + PlesePay;
                                                    gc.insert_date(orderNumber, ParcelID, 1502, DueDate, 1, DateTime.Now);

                                                }
                                            }
                                        }

                                    }
                                    catch { }
                                }
                            }
                            catch { }
                            try
                            {

                                string bulktext1 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[5]")).Text.Replace("\r\n", ",");
                                taxowner = gc.Between(bulktext1, "Owner,", "Situs address").Replace(",,", "");
                                tax_addr = gc.Between(bulktext1, "Situs address", "Legal description").Trim().Replace(",", "");
                                //*[@id='content']/div[1]/div[7]/div/table[1]/tbody
                                IWebElement currrtaxtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[1]/tbody"));
                                IList<IWebElement> currrtaxtableRow = currrtaxtable.FindElements(By.TagName("tr"));
                                int currrtaxtabletableRowcount = currrtaxtableRow.Count;
                                IList<IWebElement> currrtaxtableRowTD;
                                int e = 0;
                                foreach (IWebElement rowid in currrtaxtableRow)
                                {
                                    currrtaxtableRowTD = rowid.FindElements(By.TagName("td"));
                                    if (currrtaxtableRowTD.Count != 0 && !rowid.Text.Contains("Account number"))
                                    {
                                        accno = currrtaxtableRowTD[0].Text; alterkey = currrtaxtableRowTD[1].Text; millagecode = currrtaxtableRowTD[3].Text;
                                    }
                                }

                            }
                            catch { }
                            try
                            {

                                string paidbulk = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[3]/div[1]/div")).Text.Trim();
                                cpaiddate = gc.Between(paidbulk, "PAID ", " $").Trim();
                                cpaidamount = gc.Between(paidbulk, "$", "Receipt ").Trim();
                                cpaidamount = "$" + cpaidamount;
                                if (cpaidamount.Contains("Effective"))
                                {
                                    ceffdate = GlobalClass.After(cpaidamount, "Effective").Trim();
                                    cpaidamount = GlobalClass.Before(cpaidamount, "Effective").Trim();
                                }
                                creceipt = GlobalClass.After(paidbulk, "Receipt #");
                            }
                            catch { }

                            ////Parcel Details
                            IWebElement ITaxSearchparcell = driver.FindElement(By.LinkText("Parcel details"));
                            string strITaxSearchparcell = ITaxSearchparcell.GetAttribute("href");
                            driver.Navigate().GoToUrl(strITaxSearchparcell);
                            Thread.Sleep(3000);
                            gc.CreatePdf(orderNumber, ParcelID, "Parcel Details" + cctaxyear, driver, "FL", "Bay");
                            //Account Number~Alternate Key~Owner Name & Mailing Address~Property Address~Millage Code~Millage rate~Tax Year~Tax Amount~Gross Tax~Paid Amount~Paid Date~Effective Date~Receipt Number
                            if (milagerate.Trim() == "")
                            {
                                milagerate = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[3]/div[2]/dl[2]/dd[4]")).Text;
                            }
                            string curtax = alterkey + "~" + taxowner + "~" + tax_addr + "~" + millagecode + "~" + milagerate + "~" + cctaxyear + "~" + combinedtaxamount + "~" + grosstax + "~" + cpaidamount + "~" + cpaiddate + "~" + ceffdate + "~" + creceipt + "~" + Tax_Authority;
                            gc.insert_date(orderNumber, ParcelID, 1710, curtax, 1, DateTime.Now);
                            q++;

                        }

                        catch
                        {

                        }

                        try
                        {
                            accno = ""; alterkey = ""; taxowner = ""; tax_addr = ""; millagecode = ""; ctax_year = ""; cctaxyear = ""; combinedtaxamount = ""; grosstax = ""; cpaidamount = ""; cpaiddate = ""; ceffdate = ""; creceipt = "";
                            driver.Navigate().GoToUrl(URL);
                            Thread.Sleep(3000);

                            cctaxyear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[1]/div[2]")).Text.Trim().Replace("Real Estate", "").Replace("Print this bill (PDF)", "").Replace("\r\n", "");
                            gc.CreatePdf(orderNumber, ParcelID, "Tax Details" + cctaxyear, driver, "FL", "Bay");

                            if (q == 0)
                            {

                                IWebElement valoremtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[2]"));
                                IList<IWebElement> valoremtableRow = valoremtable.FindElements(By.TagName("tr"));
                                int valoremtablerowcount = valoremtableRow.Count;
                                IList<IWebElement> valoremtablerowTD;
                                int d = 0;
                                foreach (IWebElement rowid in valoremtableRow)
                                {
                                    valoremtablerowTD = rowid.FindElements(By.TagName("td"));
                                    if (valoremtablerowTD.Count != 0 && !rowid.Text.Contains("Taxing authority"))
                                    {
                                        string valoremtax = "";
                                        if (d < valoremtablerowcount - 1)
                                        {
                                            if (valoremtablerowTD.Count == 6)
                                            {
                                                //Taxing Authority~Tax Type~Millage~Assessed~Exemption~Taxable~Taxes
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[1].Text + "~" + valoremtablerowTD[2].Text + "~" + valoremtablerowTD[3].Text + "~" + valoremtablerowTD[4].Text + "~" + valoremtablerowTD[5].Text;
                                                gc.insert_date(orderNumber, ParcelID, 1501, valoremtax, 1, DateTime.Now);
                                            }

                                            else if (valoremtablerowTD.Count == 2)
                                            {
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                                gc.insert_date(orderNumber, ParcelID, 1501, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        else if (d == valoremtablerowcount - 1)
                                        {
                                            milagerate = valoremtablerowTD[0].Text;
                                            valoremtax = "Total" + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + valoremtablerowTD[1].Text;
                                            gc.insert_date(orderNumber, ParcelID, 1501, valoremtax, 1, DateTime.Now);
                                        }
                                    }
                                    d++;
                                }

                                //Non Ad-Valorems
                                try
                                {
                                    //*[@id="content"]/div[1]/div[8]/div/table[3]
                                    IWebElement nonvaloremtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[3]"));
                                    IList<IWebElement> nonvaloremtableRow = nonvaloremtable.FindElements(By.TagName("tr"));
                                    int nonvaloremtablerowcount = nonvaloremtableRow.Count;
                                    IList<IWebElement> nonvaloremtablerowTD;
                                    int e = 0;
                                    foreach (IWebElement rowid in nonvaloremtableRow)
                                    {
                                        nonvaloremtablerowTD = rowid.FindElements(By.TagName("td"));
                                        if (nonvaloremtablerowTD.Count != 0 && !rowid.Text.Contains("Levying authority"))
                                        {
                                            string valoremtax = "";
                                            if (e < nonvaloremtablerowcount - 1)
                                            {
                                                //Taxing Authority~Tax Type~Millage~Assessed~Exemption~Taxable~Taxes
                                                valoremtax = nonvaloremtablerowTD[0].Text + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[1].Text + "~" + nonvaloremtablerowTD[2].Text;
                                                gc.insert_date(orderNumber, ParcelID, 1501, valoremtax, 1, DateTime.Now);
                                            }
                                            else if (e == nonvaloremtablerowcount - 1)
                                            {
                                                valoremtax = "Total" + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[0].Text;
                                                gc.insert_date(orderNumber, ParcelID, 1501, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        e++;
                                    }
                                }
                                catch { }
                            }

                            string currenttaxbulktext = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/p")).Text;
                            combinedtaxamount = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/p")).Text.Trim().Replace("Combined taxes and assessments: ", "");
                            try
                            {
                                string bulkgrosstax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]/tbody/tr/td[1]")).Text.Trim();
                                if (bulkgrosstax.Contains("Gross"))
                                {

                                    grosstax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]/tbody/tr/td[1]")).Text.Trim().Replace("Gross", "");
                                    //if paid by                 
                                    string ifpaidbulk = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]/tbody/tr/td[3]")).Text;
                                    string[] stringSeparators1 = new string[] { "\r\n" };
                                    string[] lines1 = ifpaidbulk.Split(stringSeparators1, StringSplitOptions.None);
                                    ifpaidby = lines1[0]; pleasepay = lines1[1];
                                    string single = cctaxyear + "~" + ifpaidby + "~" + pleasepay;
                                    gc.insert_date(orderNumber, ParcelID, 1502, single, 1, DateTime.Now);
                                }

                                else
                                {
                                    string IfPaidBy = "", PlesePay = "", DueDate = "", itaxyear = "";
                                    try
                                    {
                                        IWebElement multitableElement26 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]"));
                                        IList<IWebElement> multitableRow26 = multitableElement26.FindElements(By.TagName("tr"));
                                        IList<IWebElement> multirowTD26;
                                        foreach (IWebElement row in multitableRow26)
                                        {
                                            multirowTD26 = row.FindElements(By.TagName("td"));
                                            int iRowsCount = multirowTD26.Count;
                                            for (int n = 0; n < iRowsCount; n++)
                                            {
                                                if ((!multirowTD26[n].Text.Contains("Face Amt")) && (!multirowTD26[n].Text.Contains("Certificate")))
                                                {
                                                    IfPaidBy = multirowTD26[n].Text.Replace("\r\n", "~");
                                                    var IfpaySplit = IfPaidBy.Split('~');
                                                    IfPaidBy = IfpaySplit[0];
                                                    PlesePay = IfpaySplit[1];
                                                    DueDate = cctaxyear + "~" + IfPaidBy + "~" + PlesePay;
                                                    gc.insert_date(orderNumber, ParcelID, 1502, DueDate, 1, DateTime.Now);

                                                }
                                            }
                                        }

                                    }
                                    catch { }
                                }
                            }
                            catch { }
                            try
                            {

                                string bulktext1 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[5]")).Text.Replace("\r\n", ",");
                                taxowner = gc.Between(bulktext1, "Owner,", "Situs address").Replace(".", "");
                                tax_addr = gc.Between(bulktext1, "Situs address", "Legal description").Trim().Replace(",", "");
                                //*[@id='content']/div[1]/div[7]/div/table[1]/tbody
                                IWebElement currrtaxtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[1]/tbody"));
                                IList<IWebElement> currrtaxtableRow = currrtaxtable.FindElements(By.TagName("tr"));
                                int currrtaxtabletableRowcount = currrtaxtableRow.Count;
                                IList<IWebElement> currrtaxtableRowTD;
                                int e = 0;
                                foreach (IWebElement rowid in currrtaxtableRow)
                                {
                                    currrtaxtableRowTD = rowid.FindElements(By.TagName("td"));
                                    if (currrtaxtableRowTD.Count != 0 && !rowid.Text.Contains("Account number"))
                                    {
                                        accno = currrtaxtableRowTD[0].Text; alterkey = currrtaxtableRowTD[1].Text; millagecode = currrtaxtableRowTD[3].Text;
                                    }
                                }

                            }
                            catch { }
                            try
                            {

                                string paidbulk = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[3]/div[1]/div")).Text.Trim();
                                cpaiddate = gc.Between(paidbulk, "PAID ", " $").Trim();
                                cpaidamount = gc.Between(paidbulk, "$", "Receipt ").Trim();
                                cpaidamount = "$" + cpaidamount;
                                creceipt = GlobalClass.After(paidbulk, "Receipt #");
                            }
                            catch { }

                            ////Parcel Details
                            IWebElement ITaxSearchparcell = driver.FindElement(By.LinkText("Parcel details"));
                            string strITaxSearchparcell = ITaxSearchparcell.GetAttribute("href");
                            driver.Navigate().GoToUrl(strITaxSearchparcell);
                            Thread.Sleep(3000);
                            gc.CreatePdf(orderNumber, ParcelID, "Parcel Details" + cctaxyear, driver, "FL", "Bay");


                            //Account Number~Alternate Key~Owner Name & Mailing Address~Property Address~Millage Code~Millage rate~Tax Year~Tax Amount~Gross Tax~Paid Amount~Paid Date~Effective Date~Receipt Number
                            string curtax = alterkey + "~" + taxowner + "~" + tax_addr + "~" + millagecode + "~" + milagerate + "~" + cctaxyear + "~" + combinedtaxamount + "~" + grosstax + "~" + cpaidamount + "~" + cpaiddate + "~" + ceffdate + "~" + creceipt + "~" + Tax_Authority;
                            gc.insert_date(orderNumber, ParcelID, 1710, curtax, 1, DateTime.Now);
                            q++;

                        }
                        catch
                        { }
                    }


                    //q = 0;
                    foreach (string URL in taxhistorylinkinst)
                    {
                        try
                        {
                            accno = ""; alterkey = ""; taxowner = ""; tax_addr = ""; millagecode = ""; ctax_year = ""; cctaxyear = ""; combinedtaxamount = ""; grosstax = ""; cpaidamount = ""; cpaiddate = ""; ceffdate = ""; creceipt = "";
                            driver.Navigate().GoToUrl(URL);
                            Thread.Sleep(3000);
                            //*[@id="content"]/div[1]/div[8]/div/div[1]                        
                            cctaxyear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[1]/div[2]")).Text.Trim().Replace("Real Estate", "").Replace("Print this bill (PDF)", "").Replace("\r\n", "");
                            gc.CreatePdf(orderNumber, ParcelID, "Tax Details" + cctaxyear, driver, "FL", "Bay");

                            if (q == 0)
                            {

                                IWebElement valoremtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[2]"));
                                IList<IWebElement> valoremtableRow = valoremtable.FindElements(By.TagName("tr"));
                                int valoremtablerowcount = valoremtableRow.Count;
                                IList<IWebElement> valoremtablerowTD;
                                int d = 0;
                                foreach (IWebElement rowid in valoremtableRow)
                                {
                                    valoremtablerowTD = rowid.FindElements(By.TagName("td"));
                                    if (valoremtablerowTD.Count != 0 && !rowid.Text.Contains("Taxing authority"))
                                    {
                                        string valoremtax = "";
                                        if (d < valoremtablerowcount - 1)
                                        {
                                            if (valoremtablerowTD.Count == 6)
                                            {
                                                //Taxing Authority~Tax Type~Millage~Assessed~Exemption~Taxable~Taxes
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[1].Text + "~" + valoremtablerowTD[2].Text + "~" + valoremtablerowTD[3].Text + "~" + valoremtablerowTD[4].Text + "~" + valoremtablerowTD[5].Text;
                                                gc.insert_date(orderNumber, ParcelID, 1501, valoremtax, 1, DateTime.Now);
                                            }

                                            else if (valoremtablerowTD.Count == 2)
                                            {
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                                gc.insert_date(orderNumber, ParcelID, 1501, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        else if (d == valoremtablerowcount - 1)
                                        {
                                            milagerate = valoremtablerowTD[0].Text;
                                            valoremtax = "Total" + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + valoremtablerowTD[1].Text;
                                            gc.insert_date(orderNumber, ParcelID, 1501, valoremtax, 1, DateTime.Now);
                                        }
                                    }
                                    d++;
                                }

                                //Non Ad-Valorems
                                try
                                {
                                    //*[@id="content"]/div[1]/div[8]/div/table[3]
                                    IWebElement nonvaloremtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[3]"));
                                    IList<IWebElement> nonvaloremtableRow = nonvaloremtable.FindElements(By.TagName("tr"));
                                    int nonvaloremtablerowcount = nonvaloremtableRow.Count;
                                    IList<IWebElement> nonvaloremtablerowTD;
                                    int e = 0;
                                    foreach (IWebElement rowid in nonvaloremtableRow)
                                    {
                                        nonvaloremtablerowTD = rowid.FindElements(By.TagName("td"));
                                        if (nonvaloremtablerowTD.Count != 0 && !rowid.Text.Contains("Levying authority"))
                                        {
                                            string valoremtax = "";
                                            if (e < nonvaloremtablerowcount - 1)
                                            {
                                                //Taxing Authority~Tax Type~Millage~Assessed~Exemption~Taxable~Taxes
                                                valoremtax = nonvaloremtablerowTD[0].Text + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[1].Text + "~" + nonvaloremtablerowTD[2].Text;
                                                gc.insert_date(orderNumber, ParcelID, 1501, valoremtax, 1, DateTime.Now);
                                            }
                                            else if (e == nonvaloremtablerowcount - 1)
                                            {
                                                valoremtax = "Total" + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[0].Text;
                                                gc.insert_date(orderNumber, ParcelID, 1501, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        e++;
                                    }
                                }
                                catch { }
                            }

                            string currenttaxbulktext = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/p")).Text;
                            combinedtaxamount = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/p")).Text.Trim().Replace("Combined taxes and assessments: ", "");
                            try
                            {
                                string bulkgrosstax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]/tbody/tr/td[1]")).Text.Trim();
                                if (bulkgrosstax.Contains("Gross"))
                                {

                                    grosstax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]/tbody/tr/td[1]")).Text.Trim().Replace("Gross", "");
                                    //if paid by                 
                                    string ifpaidbulk = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]/tbody/tr/td[3]")).Text;
                                    string[] stringSeparators1 = new string[] { "\r\n" };
                                    string[] lines1 = ifpaidbulk.Split(stringSeparators1, StringSplitOptions.None);
                                    ifpaidby = lines1[0]; pleasepay = lines1[1];
                                    string single = cctaxyear + "~" + ifpaidby + "~" + pleasepay;
                                    gc.insert_date(orderNumber, ParcelID, 1502, single, 1, DateTime.Now);
                                }

                                else
                                {
                                    string IfPaidBy = "", PlesePay = "", DueDate = "", itaxyear = "";
                                    try
                                    {
                                        IWebElement multitableElement26 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[4]"));
                                        IList<IWebElement> multitableRow26 = multitableElement26.FindElements(By.TagName("tr"));
                                        IList<IWebElement> multirowTD26;
                                        foreach (IWebElement row in multitableRow26)
                                        {
                                            multirowTD26 = row.FindElements(By.TagName("td"));
                                            int iRowsCount = multirowTD26.Count;
                                            for (int n = 0; n < iRowsCount; n++)
                                            {
                                                if ((!multirowTD26[n].Text.Contains("Face Amt")) && (!multirowTD26[n].Text.Contains("Certificate")))
                                                {
                                                    IfPaidBy = multirowTD26[n].Text.Replace("\r\n", "~");
                                                    var IfpaySplit = IfPaidBy.Split('~');
                                                    IfPaidBy = IfpaySplit[0];
                                                    PlesePay = IfpaySplit[1];
                                                    DueDate = cctaxyear + "~" + IfPaidBy + "~" + PlesePay;
                                                    gc.insert_date(orderNumber, ParcelID, 1502, DueDate, 1, DateTime.Now);

                                                }
                                            }
                                        }

                                    }
                                    catch { }
                                }
                            }
                            catch { }
                            try
                            {
                                string bulktext1 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[5]")).Text.Replace("\r\n", ",");
                                taxowner = gc.Between(bulktext1, "Owner,", "Situs address").Replace(",,", "");
                                tax_addr = gc.Between(bulktext1, "Situs address", "Legal description").Trim().Replace(",", "");
                                //*[@id='content']/div[1]/div[7]/div/table[1]/tbody
                                IWebElement currrtaxtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/table[1]/tbody"));
                                IList<IWebElement> currrtaxtableRow = currrtaxtable.FindElements(By.TagName("tr"));
                                int currrtaxtabletableRowcount = currrtaxtableRow.Count;
                                IList<IWebElement> currrtaxtableRowTD;
                                int e = 0;
                                foreach (IWebElement rowid in currrtaxtableRow)
                                {
                                    currrtaxtableRowTD = rowid.FindElements(By.TagName("td"));
                                    if (currrtaxtableRowTD.Count != 0 && !rowid.Text.Contains("Account number"))
                                    {
                                        accno = currrtaxtableRowTD[0].Text; alterkey = currrtaxtableRowTD[1].Text; millagecode = currrtaxtableRowTD[3].Text;
                                    }
                                }

                            }
                            catch { }
                            try
                            {

                                string paidbulk = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[8]/div/div[3]/div[1]/div")).Text.Trim();
                                cpaiddate = gc.Between(paidbulk, "PAID ", " $").Trim();
                                cpaidamount = gc.Between(paidbulk, "$", "Receipt ").Trim();
                                cpaidamount = "$" + cpaidamount;
                                if (cpaidamount.Contains("Effective"))
                                {
                                    ceffdate = GlobalClass.After(cpaidamount, "Effective").Trim();
                                    cpaidamount = GlobalClass.Before(cpaidamount, "Effective").Trim();
                                }
                                creceipt = GlobalClass.After(paidbulk, "Receipt #");
                            }
                            catch { }

                            //Parcel Details
                            IWebElement ITaxSearchparcell = driver.FindElement(By.LinkText("Parcel details"));
                            string strITaxSearchparcell = ITaxSearchparcell.GetAttribute("href");
                            driver.Navigate().GoToUrl(strITaxSearchparcell);
                            Thread.Sleep(3000);
                            gc.CreatePdf(orderNumber, ParcelID, "Parcel Details" + cctaxyear, driver, "FL", "Bay");


                            //Account Number~Alternate Key~Owner Name & Mailing Address~Property Address~Millage Code~Millage rate~Tax Year~Tax Amount~Gross Tax~Paid Amount~Paid Date~Effective Date~Receipt Number
                            string curtax = alterkey + "~" + taxowner + "~" + tax_addr + "~" + millagecode + "~" + milagerate + "~" + cctaxyear + "~" + combinedtaxamount + "~" + grosstax + "~" + cpaidamount + "~" + cpaiddate + "~" + ceffdate + "~" + creceipt + "~" + Tax_Authority;
                            gc.insert_date(orderNumber, ParcelID, 1710, curtax, 1, DateTime.Now);
                            q++;
                        }
                        catch
                        {

                        }
                        try
                        {
                            accno = ""; alterkey = ""; taxowner = ""; tax_addr = ""; millagecode = ""; ctax_year = ""; cctaxyear = ""; combinedtaxamount = ""; grosstax = ""; cpaidamount = ""; cpaiddate = ""; ceffdate = ""; creceipt = "";
                            driver.Navigate().GoToUrl(URL);
                            Thread.Sleep(3000);

                            cctaxyear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[1]/div[2]")).Text.Trim().Replace("Real Estate", "").Replace("Print this bill (PDF)", "").Replace("\r\n", "");
                            gc.CreatePdf(orderNumber, ParcelID, "Tax Details" + cctaxyear, driver, "FL", "Bay");

                            if (q == 0)
                            {

                                IWebElement valoremtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[2]"));
                                IList<IWebElement> valoremtableRow = valoremtable.FindElements(By.TagName("tr"));
                                int valoremtablerowcount = valoremtableRow.Count;
                                IList<IWebElement> valoremtablerowTD;
                                int d = 0;
                                foreach (IWebElement rowid in valoremtableRow)
                                {
                                    valoremtablerowTD = rowid.FindElements(By.TagName("td"));
                                    if (valoremtablerowTD.Count != 0 && !rowid.Text.Contains("Taxing authority"))
                                    {
                                        string valoremtax = "";
                                        if (d < valoremtablerowcount - 1)
                                        {
                                            if (valoremtablerowTD.Count == 6)
                                            {
                                                //Taxing Authority~Tax Type~Millage~Assessed~Exemption~Taxable~Taxes
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[1].Text + "~" + valoremtablerowTD[2].Text + "~" + valoremtablerowTD[3].Text + "~" + valoremtablerowTD[4].Text + "~" + valoremtablerowTD[5].Text;
                                                gc.insert_date(orderNumber, ParcelID, 1501, valoremtax, 1, DateTime.Now);
                                            }

                                            else if (valoremtablerowTD.Count == 2)
                                            {
                                                valoremtax = valoremtablerowTD[0].Text + "~" + "Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                                gc.insert_date(orderNumber, ParcelID, 1501, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        else if (d == valoremtablerowcount - 1)
                                        {
                                            milagerate = valoremtablerowTD[0].Text;
                                            valoremtax = "Total" + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[0].Text + "~" + "" + "~" + "" + "~" + "" + "~" + valoremtablerowTD[1].Text;
                                            gc.insert_date(orderNumber, ParcelID, 1501, valoremtax, 1, DateTime.Now);
                                        }
                                    }
                                    d++;
                                }

                                //Non Ad-Valorems
                                try
                                {
                                    //*[@id="content"]/div[1]/div[8]/div/table[3]
                                    IWebElement nonvaloremtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[3]"));
                                    IList<IWebElement> nonvaloremtableRow = nonvaloremtable.FindElements(By.TagName("tr"));
                                    int nonvaloremtablerowcount = nonvaloremtableRow.Count;
                                    IList<IWebElement> nonvaloremtablerowTD;
                                    int e = 0;
                                    foreach (IWebElement rowid in nonvaloremtableRow)
                                    {
                                        nonvaloremtablerowTD = rowid.FindElements(By.TagName("td"));
                                        if (nonvaloremtablerowTD.Count != 0 && !rowid.Text.Contains("Levying authority"))
                                        {
                                            string valoremtax = "";
                                            if (e < nonvaloremtablerowcount - 1)
                                            {
                                                //Taxing Authority~Tax Type~Millage~Assessed~Exemption~Taxable~Taxes
                                                valoremtax = nonvaloremtablerowTD[0].Text + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[1].Text + "~" + nonvaloremtablerowTD[2].Text;
                                                gc.insert_date(orderNumber, ParcelID, 1501, valoremtax, 1, DateTime.Now);
                                            }
                                            else if (e == nonvaloremtablerowcount - 1)
                                            {
                                                valoremtax = "Total" + "~" + "Non Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[0].Text;
                                                gc.insert_date(orderNumber, ParcelID, 1501, valoremtax, 1, DateTime.Now);
                                            }
                                        }
                                        e++;
                                    }
                                }
                                catch { }
                            }

                            string currenttaxbulktext = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/p")).Text;
                            combinedtaxamount = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/p")).Text.Trim().Replace("Combined taxes and assessments: ", "");
                            try
                            {
                                string bulkgrosstax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]/tbody/tr/td[1]")).Text.Trim();
                                if (bulkgrosstax.Contains("Gross"))
                                {

                                    grosstax = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]/tbody/tr/td[1]")).Text.Trim().Replace("Gross", "");
                                    //if paid by                 
                                    string ifpaidbulk = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]/tbody/tr/td[3]")).Text;
                                    string[] stringSeparators1 = new string[] { "\r\n" };
                                    string[] lines1 = ifpaidbulk.Split(stringSeparators1, StringSplitOptions.None);
                                    ifpaidby = lines1[0]; pleasepay = lines1[1];
                                    string single = cctaxyear + "~" + ifpaidby + "~" + pleasepay;
                                    gc.insert_date(orderNumber, ParcelID, 1502, single, 1, DateTime.Now);
                                }

                                else
                                {
                                    string IfPaidBy = "", PlesePay = "", DueDate = "", itaxyear = "";
                                    try
                                    {
                                        IWebElement multitableElement26 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[4]"));
                                        IList<IWebElement> multitableRow26 = multitableElement26.FindElements(By.TagName("tr"));
                                        IList<IWebElement> multirowTD26;
                                        foreach (IWebElement row in multitableRow26)
                                        {
                                            multirowTD26 = row.FindElements(By.TagName("td"));
                                            int iRowsCount = multirowTD26.Count;
                                            for (int n = 0; n < iRowsCount; n++)
                                            {
                                                if ((!multirowTD26[n].Text.Contains("Face Amt")) && (!multirowTD26[n].Text.Contains("Certificate")))
                                                {
                                                    IfPaidBy = multirowTD26[n].Text.Replace("\r\n", "~");
                                                    var IfpaySplit = IfPaidBy.Split('~');
                                                    IfPaidBy = IfpaySplit[0];
                                                    PlesePay = IfpaySplit[1];
                                                    DueDate = cctaxyear + "~" + IfPaidBy + "~" + PlesePay;
                                                    gc.insert_date(orderNumber, ParcelID, 1502, DueDate, 1, DateTime.Now);

                                                }
                                            }
                                        }

                                    }
                                    catch { }
                                }
                            }
                            catch { }
                            try
                            {

                                string bulktext1 = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[5]")).Text.Replace("\r\n", ",");
                                taxowner = gc.Between(bulktext1, "Owner,", "Situs address").Replace(".", "");
                                tax_addr = gc.Between(bulktext1, "Situs address", "Legal description").Trim().Replace(",", "");
                                //*[@id='content']/div[1]/div[7]/div/table[1]/tbody
                                IWebElement currrtaxtable = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/table[1]/tbody"));
                                IList<IWebElement> currrtaxtableRow = currrtaxtable.FindElements(By.TagName("tr"));
                                int currrtaxtabletableRowcount = currrtaxtableRow.Count;
                                IList<IWebElement> currrtaxtableRowTD;
                                int e = 0;
                                foreach (IWebElement rowid in currrtaxtableRow)
                                {
                                    currrtaxtableRowTD = rowid.FindElements(By.TagName("td"));
                                    if (currrtaxtableRowTD.Count != 0 && !rowid.Text.Contains("Account number"))
                                    {
                                        accno = currrtaxtableRowTD[0].Text; alterkey = currrtaxtableRowTD[1].Text; millagecode = currrtaxtableRowTD[3].Text;
                                    }
                                }
                            }
                            catch { }
                            try
                            {

                                string paidbulk = driver.FindElement(By.XPath("//*[@id='content']/div[1]/div[7]/div/div[3]/div[1]/div")).Text.Trim();
                                cpaiddate = gc.Between(paidbulk, "PAID ", " $").Trim();
                                cpaidamount = gc.Between(paidbulk, "$", "Receipt ").Trim();
                                cpaidamount = "$" + cpaidamount;
                                creceipt = GlobalClass.After(paidbulk, "Receipt #");
                            }
                            catch { }

                            ////Parcel Details
                            IWebElement ITaxSearchparcell = driver.FindElement(By.LinkText("Parcel details"));
                            string strITaxSearchparcell = ITaxSearchparcell.GetAttribute("href");
                            driver.Navigate().GoToUrl(strITaxSearchparcell);
                            Thread.Sleep(3000);
                            gc.CreatePdf(orderNumber, ParcelID, "Parcel Details" + cctaxyear, driver, "FL", "Bay");


                            //Account Number~Alternate Key~Owner Name & Mailing Address~Property Address~Millage Code~Millage rate~Tax Year~Tax Amount~Gross Tax~Paid Amount~Paid Date~Effective Date~Receipt Number
                            string curtax = alterkey + "~" + taxowner + "~" + tax_addr + "~" + millagecode + "~" + milagerate + "~" + cctaxyear + "~" + combinedtaxamount + "~" + grosstax + "~" + cpaidamount + "~" + cpaiddate + "~" + ceffdate + "~" + creceipt + "~" + Tax_Authority;
                            gc.insert_date(orderNumber, ParcelID, 1710, curtax, 1, DateTime.Now);
                            q++;
                        }
                        catch
                        { }
                    }

                    //issue certificate
                    string adno = "", faceamount = "", issuedate = "", ex_date = "", buyer = "", intrate = "", cer_no = "";
                    foreach (string URL1 in strissuecertificate)
                    {
                        driver.Navigate().GoToUrl(URL1);
                        Thread.Sleep(3000);
                        try
                        {
                            string issueyear = driver.FindElement(By.XPath("//*[@id='content']/div[1]/p")).Text.Trim().Replace("This parcel has an issued certificate for ", "").Replace(".", "");
                            gc.CreatePdf(orderNumber, ParcelID, "Issue Certificate Details" + issueyear, driver, "FL", "Bay");
                            string issuecertificatebulktext = driver.FindElement(By.XPath("//*[@id='content']/div[1]/dl")).Text.Trim();
                            cer_no = driver.FindElement(By.XPath("//*[@id='certificate']")).Text.Replace("Certificate #", "");
                            adno = gc.Between(issuecertificatebulktext, "Advertised number", "Face amount").Trim();
                            faceamount = gc.Between(issuecertificatebulktext, "Face amount", "Issued date").Trim();
                            issuedate = gc.Between(issuecertificatebulktext, "Issued date", "Expiration date").Trim();
                            ex_date = gc.Between(issuecertificatebulktext, "Expiration date", "Buyer").Trim();
                            buyer = gc.Between(issuecertificatebulktext, "Buyer", "Interest rate").Trim().Replace("\r\n", ",");
                            intrate = GlobalClass.After(issuecertificatebulktext, "Interest rate");
                            //Tax Year~Certificate Number~Advertised Number~Face Amount~Issued Date~Expiration Date~Buyer~Interest Rate
                            string isscer = issueyear + "~" + cer_no + "~" + adno + "~" + faceamount + "~" + issuedate + "~" + ex_date + "~" + buyer + "~" + intrate;
                            gc.insert_date(orderNumber, ParcelID, 1711, isscer, 1, DateTime.Now);
                        }
                        catch { }
                    }


                    TaxTime = DateTime.Now.ToString("HH:mm:ss");
                    driver.Quit();
                    gc.mergpdf(orderNumber, "FL", "Bay");
                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "FL", "Bay", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);
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
        public void BillDownload(string orderNumber, string Parcel_number, string BillTax, string dbill, string strbillyear)
        {
            string fileName = "";
            var chromeOptions = new ChromeOptions();
            var downloadDirectory = ConfigurationManager.AppSettings["AutoPdf"];
            chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
            chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
            chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
            chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
            var chDriver = new ChromeDriver(chromeOptions);
            Array.ForEach(Directory.GetFiles(@downloadDirectory), File.Delete); //Delete all the files in auto pdf
            try
            {
                chDriver.Navigate().GoToUrl(BillTax);
                Thread.Sleep(5000);
                try
                {

                    // fileName = "Escambia-County--Real-Estate-" + dbill + "-" + strbillyear + "-Annual-bill.pdf";
                    fileName = latestfilename();
                    Thread.Sleep(3000);
                    gc.AutoDownloadFile(orderNumber, Parcel_number, "Bay", "FL", fileName);

                }
                catch { }

                try
                {
                    // fileName = "Escambia-County--Real-Estate-" + dbill + "-" + strbillyear + "-Installment-bill-1.pdf";
                    fileName = latestfilename();
                    Thread.Sleep(3000);
                    gc.AutoDownloadFile(orderNumber, Parcel_number, "Bay", "FL", fileName);

                }
                catch (Exception ex)
                {

                }
                try
                {
                    //fileName = "Escambia-County--Real-Estate-" + dbill + "-" + strbillyear + "-Installment-bill-2.pdf";
                    fileName = latestfilename();
                    Thread.Sleep(3000);
                    gc.AutoDownloadFile(orderNumber, Parcel_number, "Bay", "FL", fileName);

                }
                catch { }
                try
                {
                    // fileName = "Escambia-County--Real-Estate-" + dbill + "-" + strbillyear + "-Installment-bill-3.pdf";
                    fileName = latestfilename();
                    Thread.Sleep(3000);
                    gc.AutoDownloadFile(orderNumber, Parcel_number, "Bay", "FL", fileName);

                }
                catch { }

                try
                {
                    //fileName = "Escambia-County--Real-Estate-" + dbill + "-" + strbillyear + "-Installment-bill-4.pdf";
                    fileName = latestfilename();
                    Thread.Sleep(3000);
                    gc.AutoDownloadFile(orderNumber, Parcel_number, "Bay", "FL", fileName);

                }
                catch { }

                chDriver.Quit();
            }
            catch (Exception ex)
            {
                chDriver.Quit();
            }
        }
        public string latestfilename()
        {
            var downloadDirectory1 = ConfigurationManager.AppSettings["AutoPdf"];
            var files = new DirectoryInfo(downloadDirectory1).GetFiles("*.*");
            string latestfile = "";
            DateTime lastupdated = DateTime.MinValue;
            foreach (FileInfo file in files)
            {
                if (file.LastWriteTime > lastupdated)
                {
                    lastupdated = file.LastWriteTime;
                    latestfile = file.Name;
                }

            }
            return latestfile;
        }
    }
}


//        public string FTP_BayFL(string streetno, string direction, string streetname, string streettype, string unitno, string ownername, string parcelNumber, string searchType, string orderNumber)
//{
//    GlobalClass.global_orderNo = orderNumber;
//    HttpContext.Current.Session["orderNo"] = orderNumber;
//    GlobalClass.global_parcelNo = parcelNumber;
//    List<string> multiparcel = new List<string>();
//    string Parcel_number = "", Tax_Authority = "", Year = "", Adderess = "", YearBuilt = "", taxresult3 = "", Firsthalf = "", Secondhalf = "", fullhalf = "", prepayresult1 = "", prepayresult2 = "", prepayresult3 = "", paiedamt = "";
//    string outparcelno = "", taxparcel = "", mailingaddress = "", tax_district = "", siteaddr = "", legal_desc = "", year_built = "", propuse = "", par1 = "", par2 = "";
//    string Building_Value = "", LandValue = "", JustValue = "", cctaxyear = "", Cap = "", AssessedValue = "", Exemption = "", TaxableValue = "";
//    string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
//    string strBill = "-", strBalance = "-", strBillDate = "-", strBillPaid = "-";
//    // string YearBuilt = "";
//    string taxowner = "", tax_addr = "", accno = "", alterkey = "", millagecode = "", milagerate = "", ctax_year = "", cpaiddate = "", strbillyear = "", cpaidamount = "", creceipt = "", combinedtaxamount = "", ceffdate = "", grosstax = "", ifpaidby = "", pleasepay = "";
//    List<string> strissuecertificate = new List<string>();
//    List<string> taxhistorylink = new List<string>();
//    List<string> downloadlink = new List<string>();
//    List<string> taxhistorylinkinst = new List<string>();
//    var driverService = PhantomJSDriverService.CreateDefaultService();
//    driverService.HideCommandPromptWindow = true;
//    //driver = new PhantomJSDriver();
//    //driver = new ChromeDriver();
//    using (driver = new PhantomJSDriver()) //ChromeDriver
//    {
//        StartTime = DateTime.Now.ToString("HH:mm:ss");
//        driver.Navigate().GoToUrl("http://qpublic6.qpublic.net/qp6/fl_search_dw.php?county=fl_bay");
//        Thread.Sleep(2000);
//        try
//        {
//            driver.FindElement(By.XPath("/html/body/ul/li[1]/b/a")).SendKeys(Keys.Enter);
//            Thread.Sleep(2000);
//        }
//        catch { }
//        try
//        {
//            driver.FindElement(By.XPath("//*[@id='appBody']/div[4]/div/div/div[3]/a[1]")).Click();
//            Thread.Sleep(2000);
//        }
//        catch { }
//        string address = "";
//        if (direction != "")
//        {
//            address = streetno + " " + direction + " " + streetname + " " + streettype + "" + unitno;
//        }
//        else
//        {
//            address = streetno + " " + streetname + " " + streettype + "" + unitno;
//        }
//        if (searchType == "titleflex")
//        {

//            gc.TitleFlexSearch(orderNumber, "", "", address.Trim(), "FL", "Bay");
//            if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
//            {
//                driver.Quit();
//                return "MultiParcel";
//            }
//            else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
//            {
//                HttpContext.Current.Session["Nodata_BayFL"] = "Yes";
//                driver.Quit();
//                return "No Data Found";
//            }
//            parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
//            searchType = "parcel";
//        }


//                    if (searchType == "address")
//                    {
//                        ///html/body/form/table/tbody/tr/td[2]/table/tbody/tr[1]/td[2]/input
//                        driver.FindElement(By.XPath("/html/body/form/table/tbody/tr/td[2]/table/tbody/tr[1]/td[2]/input")).SendKeys(streetno);
//driver.FindElement(By.XPath("//*[@id='streetName']")).SendKeys(streetname);
//gc.CreatePdf_WOP(orderNumber, "SearchAddressBefore", driver, "FL", "Bay");
//                        driver.FindElement(By.XPath("/html/body/form/table/tbody/tr/td[2]/table/tbody/tr[4]/td[2]/input")).SendKeys(Keys.Enter);
//gc.CreatePdf_WOP(orderNumber, "SearchAddressAfter", driver, "FL", "Bay");
//                        Thread.Sleep(2000);
//                        try
//                        {
//                            gc.CreatePdf_WOP(orderNumber, "Multiparcel Result", driver, "FL", "Bay");
//                            Thread.Sleep(2000);
//                            string Owner = "", Address = "", MultiParcelNumber = "";
//int Mcount = 0;
//IWebElement multy = driver.FindElement(By.XPath("/html/body/table/tbody"));
//IList<IWebElement> muladdress = multy.FindElements(By.TagName("tr"));
//IList<IWebElement> mulid;
//                            foreach (IWebElement addressrow in muladdress)
//                            {
//                                mulid = addressrow.FindElements(By.TagName("td"));
//                                if (mulid.Count != 0 && muladdress.Count != 7 && !addressrow.Text.Contains("Return to Main Search") && !addressrow.Text.Contains("Search Criteria:") && !addressrow.Text.Contains("Search produced") && !addressrow.Text.Contains("Parcel Number") && !addressrow.Text.Contains("To create") && !addressrow.Text.Contains("No warranties") && !addressrow.Text.Contains("Print Mailing Labels") && !addressrow.Text.Contains("The Bay") && !addressrow.Text.Contains(" Return to Main Search "))
//                                {
//                                    if (Mcount <= 29)
//                                    {
//                                        IWebElement Iparcel = mulid[0].FindElement(By.TagName("a"));
//MultiParcelNumber = Iparcel.Text;
//                                        Owner = mulid[1].Text;
//                                        Address = mulid[2].Text;
//                                        string MultyInst = Owner + "~" + Address;
//gc.insert_date(orderNumber, MultiParcelNumber, 1495, MultyInst, 1, DateTime.Now);
//                                    }
//                                    Mcount++;
//                                }
//                                if (muladdress.Count == 7)
//                                {
//                                    driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[1]/a")).Click();
//Thread.Sleep(2000);
//                                }
//                            }
//                            if (muladdress.Count > 32)
//                            {
//                                HttpContext.Current.Session["multiParcel_BayFL_Maximum"] = "Maximum";
//                                return "Maximum";
//                            }
//                            else
//                            {
//                                HttpContext.Current.Session["multiparcel_BayFL"] = "Yes";
//                            }
//                            driver.Quit();
//                            return "MultiParcel";
//                        }
//                        catch
//                        {

//                        }
//                        gc.CreatePdf_WOP(orderNumber, "SearchAddressAfter", driver, "FL", "Bay");
//                        Thread.Sleep(2000);
//                        try
//                        {
//                            //No Data Found
//                            string nodata = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td")).Text;
//                            if (nodata.Contains("No Records Found. Please check your search terms and try again."))
//                            {
//                                HttpContext.Current.Session["Nodata_BayFL"] = "Yes";
//                                driver.Quit();
//                                return "No Data Found";
//                            }
//                        }
//                        catch { }
//                    }
//                    //Owername search
//                    if (searchType == "ownername")
//                    {
//                        driver.FindElement(By.XPath("/html/body/form/table/tbody/tr/td/input[2]")).SendKeys(ownername);
//gc.CreatePdf_WOP(orderNumber, "SearchAddressBefore", driver, "FL", "Bay");
//                        driver.FindElement(By.XPath("/html/body/form/table/tbody/tr/td/input[5]")).SendKeys(Keys.Enter);
//gc.CreatePdf_WOP(orderNumber, "SearchAddressAfter", driver, "FL", "Bay");
//                        Thread.Sleep(2000);
//                        try
//                        {
//                            gc.CreatePdf_WOP(orderNumber, "Multiparcel Result", driver, "FL", "Bay");
//                            Thread.Sleep(2000);
//                            string Owner = "", Address = "", MultiParcelNumber = "";
//int Mcount = 0;
//IWebElement multy = driver.FindElement(By.XPath("/html/body/table/tbody"));
//IList<IWebElement> muladdress = multy.FindElements(By.TagName("tr"));
//IList<IWebElement> mulid;
//                            foreach (IWebElement addressrow in muladdress)
//                            {
//                                mulid = addressrow.FindElements(By.TagName("td"));
//                                if (mulid.Count != 0 && muladdress.Count != 7 && !addressrow.Text.Contains("Return to Main Search") && !addressrow.Text.Contains("Search Criteria:") && !addressrow.Text.Contains("Search produced") && !addressrow.Text.Contains("Parcel Number") && !addressrow.Text.Contains("To create") && !addressrow.Text.Contains("No warranties") && !addressrow.Text.Contains("Print Mailing Labels") && !addressrow.Text.Contains("The Bay") && !addressrow.Text.Contains(" Return to Main Search "))
//                                {
//                                    if (Mcount <= 29)
//                                    {
//                                        IWebElement Iparcel = mulid[0].FindElement(By.TagName("a"));
//MultiParcelNumber = Iparcel.Text;
//                                        Owner = mulid[1].Text;
//                                        Address = mulid[2].Text;
//                                        string MultyInst = Owner + "~" + Address;
//gc.insert_date(orderNumber, MultiParcelNumber, 1495, MultyInst, 1, DateTime.Now);
//                                    }
//                                    Mcount++;
//                                }
//                                if (muladdress.Count == 7)
//                                {
//                                    driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[1]/a")).Click();
//Thread.Sleep(2000);
//                                }
//                            }
//                            if (muladdress.Count > 32)
//                            {
//                                HttpContext.Current.Session["multiParcel_BayFL_Maximum"] = "Maximum";
//                                return "Maximum";
//                            }
//                            else
//                            {
//                                HttpContext.Current.Session["multiparcel_BayFL"] = "Yes";
//                            }
//                            driver.Quit();
//                            return "MultiParcel";
//                        }
//                        catch
//                        {

//                        }
//                        gc.CreatePdf_WOP(orderNumber, "SearchAddressAfter", driver, "FL", "Bay");
//                        Thread.Sleep(2000);
//                        try
//                        {
//                            //No Data Found
//                            string nodata = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td")).Text;
//                            if (nodata.Contains("No Records Found. Please check your search terms and try again."))
//                            {
//                                HttpContext.Current.Session["Nodata_BayFL"] = "Yes";
//                                driver.Quit();
//                                return "No Data Found";
//                            }
//                        }
//                        catch { }
//                    }
//                    if (searchType == "parcel")
//                    {
//                        driver.FindElement(By.XPath("html/body/form/table/tbody/tr/td/input[2]")).SendKeys(parcelNumber);
//gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search Before", driver, "FL", "Bay");
//                        driver.FindElement(By.XPath("/html/body/form/table/tbody/tr/td/input[5]")).SendKeys(Keys.Enter);
//gc.CreatePdf(orderNumber, parcelNumber, "Parcel Search After", driver, "FL", "Bay");
//                        Thread.Sleep(2000);
//                        try
//                        {
//                            //No Data Found
//                            string nodata = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td/font")).Text;
//                            if (nodata.Contains("0 of 0 Results Returned"))
//                            {
//                                HttpContext.Current.Session["Nodata_BayFL"] = "Yes";
//                                driver.Quit();
//                                return "No Data Found";
//                            }
//                        }
//                        catch { }
//                    }
//                    //Property Details
//                    try
//                    {
//                        driver.FindElement(By.XPath("/html/body/table/tbody/tr[4]/td[1]/a")).Click();
//Thread.Sleep(2000);
//                    }
//                    catch { }

//                    string ParcelID = "", OwnerName = "", MailingAddress = "", Address1 = "", PropertyAddress = "", Finalmillagerate = "", Acreage = "", Homestead = "", MapBlock = "", Lot = "", SubDivision = "", TaxDistrict = "", TaxRate = "", Propertyusage = "";

//IWebElement tbmulti11 = driver.FindElement(By.XPath("/html/body/center[2]/table[2]/tbody"));
//IList<IWebElement> TRmulti11 = tbmulti11.FindElements(By.TagName("tr"));
//IList<IWebElement> TDmulti11;
//                    foreach (IWebElement row in TRmulti11)
//                    {
//                        TDmulti11 = row.FindElements(By.TagName("td"));
//                        if (TDmulti11.Count != 0)
//                        {
//                            if (row.Text.Contains("Owner Name"))
//                            {
//                                OwnerName = TDmulti11[1].Text;
//                            }
//                            if (row.Text.Contains("Parcel Number"))
//                            {
//                                ParcelID = TDmulti11[3].Text.Trim();
//                            }
//                            if (row.Text.Contains("Mailing Address"))
//                            {
//                                MailingAddress = TDmulti11[1].Text;
//                            }
//                            if (row.Text.Contains("Tax District"))
//                            {
//                                TaxDistrict = TDmulti11[3].Text;
//                                Address1 = TDmulti11[1].Text;
//                            }
//                            if (row.Text.Contains("Location Address"))
//                            {
//                                PropertyAddress = TDmulti11[1].Text;
//                            }
//                            if (row.Text.Contains("Final Millage Rates"))
//                            {
//                                Finalmillagerate = TDmulti11[3].Text;
//                            }
//                            if (row.Text.Contains("Acreage"))
//                            {
//                                Acreage = TDmulti11[3].Text;
//                            }
//                            if (row.Text.Contains("Homestead"))
//                            {
//                                Homestead = TDmulti11[3].Text;
//                            }
//                            if (row.Text.Contains("Map Block"))
//                            {
//                                MapBlock = TDmulti11[1].Text;
//                            }
//                            if (row.Text.Contains("Lot"))
//                            {
//                                Lot = TDmulti11[1].Text;
//                            }
//                            if (row.Text.Contains("Subdivision"))
//                            {
//                                SubDivision = TDmulti11[1].Text;
//                            }
//                            if (row.Text.Contains("Tax Rate"))
//                            {
//                                TaxRate = TDmulti11[3].Text;
//                            }
//                            if (row.Text.Contains("Usage"))
//                            {
//                                Propertyusage = TDmulti11[1].Text;
//                            }
//                        }
//                    }

//                    try
//                    {
//                        YearBuilt = driver.FindElement(By.XPath("/html/body/center[2]/table[5]/tbody/tr[5]/td[6]")).Text;
//                    }
//                    catch { }

//                    gc.CreatePdf(orderNumber, ParcelID, "Property Details", driver, "FL", "Bay");
//                    ByVisibleElement(driver.FindElement(By.XPath("/html/body/center[2]/center/table[2]/tbody/tr/td")));
//                    gc.CreatePdf(orderNumber, ParcelID, "Property Details1", driver, "FL", "Bay");

//                    //********Assessment Details*******
//                    string assesstitle = "", assessvalue = "", assessvalue1 = "", Taxyear1 = "", Taxyear2 = "";
//IWebElement tbmulti12 = driver.FindElement(By.XPath("/html/body/center[2]/table[4]/tbody/tr[2]/td[1]/table[1]/tbody"));
//IList<IWebElement> TRmulti12 = tbmulti12.FindElements(By.TagName("tr"));
//IList<IWebElement> TDmulti12;

//                    foreach (IWebElement row in TRmulti12)
//                    {
//                        TDmulti12 = row.FindElements(By.TagName("td"));

//                        if (TDmulti12.Count != 0 && row.Text.Contains("Certified Values") && !row.Text.Contains("Maximum Save Our Homes Portability") && !row.Text.Contains("AGL Amount"))
//                        {
//                            Taxyear1 = TDmulti12[1].Text.Replace("Certified Values", "").Trim();
//Taxyear2 = TDmulti12[2].Text.Replace("Certified Values", "").Trim();

//                        }
//                        if (TDmulti12.Count != 0 && !row.Text.Contains("Certified Values") && !row.Text.Contains("Maximum Save Our Homes Portability") && !row.Text.Contains("AGL Amount"))
//                        {
//                            assesstitle += TDmulti12[0].Text + "~";
//                            assessvalue1 += TDmulti12[1].Text + "~";
//                            assessvalue += TDmulti12[2].Text + "~";
//                        }
//                    }
//                    db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + "Tax Year" + "~" + assesstitle.Remove(assesstitle.Length - 1, 1) + "' where Id = '" + 1497 + "'");
//                    gc.insert_date(orderNumber, ParcelID, 1497, Taxyear1 + "~" + assessvalue1.Remove(assessvalue1.Length - 1, 1), 1, DateTime.Now);
//                    gc.insert_date(orderNumber, ParcelID, 1497, Taxyear2 + "~" + assessvalue.Remove(assessvalue.Length - 1, 1), 1, DateTime.Now);
//                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
//                    //Tax Authority
//                    string Taxauthority = "", Taxauthority1 = "";
//                    try
//                    {
//                        driver.Navigate().GoToUrl("https://www.baytaxcollector.com/contacts/");
//Taxauthority = driver.FindElement(By.XPath("/html/body/div[1]/div/div[3]/div/div/article/section/div[3]/div/div/div/section[2]/div/div/div/header/h4")).Text.Trim();
//                    }
//                    catch { }
//                    //Property Details Insert
//                    string propertydetails = OwnerName + "~" + MailingAddress + " " + Address1 + "~" + PropertyAddress + "~" + Finalmillagerate + "~" + Acreage + "~" + Homestead + "~" + TaxDistrict + "~" + Propertyusage + "~" + YearBuilt + "~" + Taxauthority;
//gc.insert_date(orderNumber, ParcelID, 1496, propertydetails, 1, DateTime.Now);