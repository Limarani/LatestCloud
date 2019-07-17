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
    public class WebDriver_WaltonGA
    {

        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();

        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_WaltonGA(string address, string parcelNumber, string searchType, string ownername, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //  driver = new PhantomJSDriver();
            //driver = new ChromeDriver();
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    driver.Navigate().GoToUrl("https://qpublic.schneidercorp.com/Application.aspx?App=WaltonCountyGA&Layer=Parcels&PageType=Search");
                    Thread.Sleep(4000);
                    try
                    {
                        driver.FindElement(By.LinkText("Agree")).Click();
                        Thread.Sleep(1000);
                    }
                    catch { }
                    if (searchType == "titleflex")
                    {
                        string titleaddress = address;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "GA", "Walton");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_WaltonGA"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {

                        driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_txtAddress")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "GA", "Walton");
                        driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(4000);
                        gc.CreatePdf_WOP(orderNumber, "Address Search result", driver, "GA", "Walton");
                        try
                        {
                            int Max = 0;
                            string strowner = "", strAddress = "", strCity = "";
                            IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_gvSearchResults']/tbody"));
                            IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                            IList<IWebElement> multiTD;
                            foreach (IWebElement multi in multiRow)
                            {
                                multiTD = multi.FindElements(By.TagName("td"));
                                if (multiTD.Count != 0 && multiRow.Count < 3)
                                {
                                    IWebElement IsearchClick = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_gvSearchResults']/tbody/tr[2]/td[1]/a"));
                                    IsearchClick.Click();
                                    Thread.Sleep(2000);
                                    Max++;
                                    break;

                                }
                                if (multiTD.Count != 0 && multiRow.Count > 25)
                                {
                                    HttpContext.Current.Session["multiparcel_WaltonGA_Maximum"] = "Maximum";
                                    driver.Quit();
                                    return "Maximum";
                                }
                                if (multiTD.Count != 0 && multiRow.Count > 2 && multiRow.Count <= 25)
                                {
                                    strowner = multiTD[1].Text;
                                    strAddress = multiTD[2].Text;

                                    string multidetails = strowner + "~" + strAddress;
                                    gc.insert_date(orderNumber, multiTD[0].Text, 1431, multidetails, 1, DateTime.Now);
                                    Max++;
                                }
                            }
                            if (Max > 1 && Max < 26)
                            {
                                HttpContext.Current.Session["multiparcel_WaltonGA"] = "Yes";
                                driver.Quit();
                                return "MultiParcel";
                            }
                            if (Max == 0)
                            {
                                HttpContext.Current.Session["Nodata_WaltonGA"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    //else if (searchType == "ownername")
                    //{
                    //    try
                    //    {
                    //        driver.FindElement(By.Id("ContentPlaceHolder1_btnDisclaimerAccept")).Click();
                    //        Thread.Sleep(1000);
                    //    }
                    //    catch { }
                    //    try
                    //    {
                    //        driver.FindElement(By.LinkText("Owner")).Click();
                    //        Thread.Sleep(1000);
                    //    }
                    //    catch { }
                    //    string Lastname = "", Firstname = "";
                    //    try
                    //    {
                    //        var ownersplit = ownername.Trim().Split(' ');
                    //        Lastname = ownersplit[0];
                    //        Firstname = ownersplit[1];
                    //        driver.FindElement(By.Id("ContentPlaceHolder1_Owner_tbOwnerLastName")).SendKeys(Lastname);
                    //        driver.FindElement(By.Id("ContentPlaceHolder1_Owner_tbOwnerFirstName")).SendKeys(Firstname);
                    //        ownername = Lastname + " " + Firstname;
                    //    }
                    //    catch { }

                    //    gc.CreatePdf_WOP(orderNumber, "OwnerName Search", driver, "GA", "Walton");
                    //    driver.FindElement(By.Id("ContentPlaceHolder1_Owner_btnSearchOwner")).SendKeys(Keys.Enter);
                    //    Thread.Sleep(3000);
                    //    gc.CreatePdf_WOP(orderNumber, "OwnerName Search Results", driver, "GA", "Walton");
                    //    try
                    //    {
                    //        int Max = 0;
                    //        string strowner = "", strAddress = "", strCity = "";
                    //        IWebElement multiaddress = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_gvSearchResults']/tbody"));
                    //        IList<IWebElement> multiRow = multiaddress.FindElements(By.TagName("tr"));
                    //        IList<IWebElement> multiTD;
                    //        foreach (IWebElement multi in multiRow)
                    //        {
                    //            multiTD = multi.FindElements(By.TagName("td"));
                    //            if (multiTD.Count != 0 && multiRow.Count < 2)
                    //            {
                    //                IWebElement IsearchClick = driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_gvSearchResults']/tbody/tr[2]/td[1]/a"));
                    //                IsearchClick.Click();
                    //                Thread.Sleep(2000);
                    //                break;
                    //            }
                    //            if (multiTD.Count != 0 && multiRow.Count > 25)
                    //            {
                    //                HttpContext.Current.Session["multiparcel_WayneOH_Maximum"] = "Maximum";
                    //                driver.Quit();
                    //                return "Maximum";
                    //            }
                    //            if (multiTD.Count != 0 && multiRow.Count > 2 && multiRow.Count <= 25)
                    //            {
                    //                strowner = multiTD[1].Text;
                    //                strAddress = multiTD[2].Text;

                    //                string multidetails = strowner + "~" + strAddress;
                    //                gc.insert_date(orderNumber, multiTD[0].Text, 1431, multidetails, 1, DateTime.Now);
                    //                Max++;
                    //            }
                    //        }
                    //        if (Max > 1 && Max < 26)
                    //        {
                    //            HttpContext.Current.Session["multiparcel_WayneOH"] = "Yes";
                    //            driver.Quit();
                    //            return "MultiParcel";
                    //        }
                    //        if (Max == 0)
                    //        {
                    //            HttpContext.Current.Session["Zero_Wayne"] = "Zero";
                    //            driver.Quit();
                    //            return "Zero";
                    //        }
                    //    }
                    //    catch { }
                    //}


                    else if (searchType == "parcel")
                    {

                        driver.FindElement(By.Id("ctlBodyPane_ctl02_ctl01_txtParcelID")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "parcel search", driver, "GA", "Walton");
                        driver.FindElement(By.Id("ctlBodyPane_ctl02_ctl01_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(3000);
                        // gc.CreatePdf(orderNumber, parcelNumber, "parcel search Result", driver, "GA", "Walton");
                        // driver.FindElement(By.XPath("//*[@id='ContentPlaceHolder1_gvSearchResults']/tbody/tr[2]/td[1]/a")).SendKeys(Keys.Enter);
                        // Thread.Sleep(3000);
                    }

                    try
                    {
                        IWebElement Inodata = driver.FindElement(By.Id("ctlBodyPane_noDataList_pnlNoResults"));
                        if (Inodata.Text.Contains("No results match"))
                        {
                            HttpContext.Current.Session["Nodata_WaltonGA"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    //property details
                    gc.CreatePdf(orderNumber, parcelNumber, "Property Details", driver, "GA", "Walton");
                    int i = 0, j = 0;
                    string Loc_Addrs = "", Leg_Desp = "", Class = "", Tax_Dist = "", Milagerate = "", Acres = "", Neighberwood = "";
                    string Homstd_Exmp = "", Zoning = "", Owner1 = "", Owner2 = "", Mailing_Address = "", Overall_OwnMailAdd = "";
                    string Mailing_Address1 = "", Mailing_Address2 = "", yblt = "", Year_Built = "", Year1 = "", Year2 = "";
                    IWebElement tbmulti11 = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_ctl01_dvNonPrebillMH']/table/tbody"));
                    IList<IWebElement> TRmulti11 = tbmulti11.FindElements(By.TagName("tr"));
                    IList<IWebElement> TDmulti11;
                    foreach (IWebElement row in TRmulti11)
                    {

                        TDmulti11 = row.FindElements(By.TagName("td"));
                        if (TDmulti11.Count != 0)
                        {
                            if (i == 0)
                                parcelNumber = TDmulti11[1].Text;
                            if (i == 1)
                                Loc_Addrs = TDmulti11[1].Text;
                            if (i == 3)
                                Leg_Desp = TDmulti11[1].Text;
                            if (i == 5)
                                Class = TDmulti11[1].Text;
                            if (i == 8)
                                Tax_Dist = TDmulti11[1].Text;
                            if (i == 9)
                                Milagerate = TDmulti11[1].Text;
                            if (i == 10)
                                Acres = TDmulti11[1].Text;
                            if (i == 11)
                                Neighberwood = TDmulti11[1].Text;
                            if (i == 12)
                                Homstd_Exmp = TDmulti11[1].Text;
                            i++;
                        }
                    }

                    try
                    {
                        Zoning = driver.FindElement(By.Id("ctlBodyPane_ctl00_ctl01_lblZoning")).Text;
                        Owner1 = driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_lnkOwnerName_lblSearch")).Text;
                        Owner2 = driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_lblAddress")).Text;
                        Mailing_Address = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl01_ctl01_lblCityStateZip']")).Text;
                        Overall_OwnMailAdd = Owner1 + " " + Owner2 + " " + Mailing_Address;
                    }
                    catch
                    { }

                    try
                    {
                        Owner1 = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl01_ctl01_lnkOwnerName_lblSearch']")).Text;
                        Mailing_Address1 = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl01_ctl01_lblAddress']")).Text;
                        Mailing_Address2 = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl01_ctl01_lblCityStateZip']")).Text;
                        Overall_OwnMailAdd = Owner1 + " " + Mailing_Address1 + " " + Mailing_Address2;
                    }
                    catch
                    { }

                    try
                    {
                        IWebElement Yeartb = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl04_mSection']/div/table/tbody"));
                        IList<IWebElement> YearTR = Yeartb.FindElements(By.TagName("tr"));
                        IList<IWebElement> YearTD;

                        foreach (IWebElement Tax4 in YearTR)
                        {
                            YearTD = Tax4.FindElements(By.TagName("td"));
                            if (YearTD.Count != 0)
                            {
                                yblt = YearTD[0].Text;
                                if (yblt.Contains("Year Built"))
                                {
                                    Year_Built = YearTD[1].Text;
                                }
                            }
                        }
                    }
                    catch
                    { }

                    string property = Loc_Addrs + "~" + Leg_Desp + "~" + Class + "~" + Zoning + "~" + Tax_Dist + "~" + Milagerate + "~" + Acres + "~" + Neighberwood + "~" + Homstd_Exmp + "~" + Overall_OwnMailAdd + "~" + Year_Built;
                    gc.CreatePdf(orderNumber, parcelNumber, "Property Details", driver, "GA", "Walton");
                    gc.insert_date(orderNumber, parcelNumber, 1452, property, 1, DateTime.Now);

                    //Assessment Details
                    IWebElement AssmThTb = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl12_ctl01_grdValuation']/thead"));
                    IList<IWebElement> AssmThTr = AssmThTb.FindElements(By.TagName("tr"));
                    IList<IWebElement> AssmTh;

                    foreach (IWebElement Assm in AssmThTr)
                    {
                        AssmTh = Assm.FindElements(By.TagName("th"));
                        if (AssmTh.Count != 0)
                        {
                            Year1 = AssmTh[2].Text;
                            Year2 = AssmTh[3].Text;
                        }
                    }

                    IWebElement AssmTb = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl12_ctl01_grdValuation']/tbody"));
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
                            }
                            else if (j == 1)
                            {
                                Land_Value.Add(AssmTd[2].Text);
                                Land_Value.Add(AssmTd[3].Text);
                            }
                            else if (j == 2)
                            {
                                Improvement_Value.Add(AssmTd[2].Text);
                                Improvement_Value.Add(AssmTd[3].Text);
                            }
                            else if (j == 3)
                            {
                                Accessory_Value.Add(AssmTd[2].Text);
                                Accessory_Value.Add(AssmTd[3].Text);
                            }
                            else if (j == 4)
                            {
                                Current_Value.Add(AssmTd[2].Text);
                                Current_Value.Add(AssmTd[3].Text);
                            }

                            j++;
                        }
                    }

                    string Assemnt_Details1 = Year1 + "~" + Previous_Value[0] + "~" + Land_Value[0] + "~" + Improvement_Value[0] + "~" + Accessory_Value[0] + "~" + Current_Value[0];
                    string Assemnt_Details2 = Year2 + "~" + Previous_Value[1] + "~" + Land_Value[1] + "~" + Improvement_Value[1] + "~" + Accessory_Value[1] + "~" + Current_Value[1];
                    gc.insert_date(orderNumber, parcelNumber, 1453, Assemnt_Details1, 1, DateTime.Now);
                    gc.insert_date(orderNumber, parcelNumber, 1453, Assemnt_Details2, 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");





                    // Tax Information
                    driver.Navigate().GoToUrl("https://waltoncountyga.governmentwindow.com/tax.html");
                    Thread.Sleep(4000);
                    string parcel1 = "", parcel2 = "";
                    parcel1 = parcelNumber.Substring(0, 5);

                    parcel2 = parcelNumber.Substring(5, 3);

                    parcelNumber = parcel1 + "-00000-" + parcel2 + "-000";

                    driver.FindElement(By.XPath("//*[@id='taxesSearchForm']/div[2]/div[2]/div/div/input[1]")).SendKeys(parcelNumber);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Search", driver, "GA", "Walton");
                    driver.FindElement(By.XPath("//*[@id='taxesSearchForm']/div[2]/div[2]/div/div/input[2]")).Click();
                    Thread.Sleep(1000);
                    gc.CreatePdf(orderNumber, parcelNumber, "Tax Search Result", driver, "GA", "Walton");

                    IWebElement taxinfo = driver.FindElement(By.XPath("//*[@id='tbl_tax_results']"));
                    IList<IWebElement> TRtaxinfo = taxinfo.FindElements(By.TagName("tr"));
                    IList<IWebElement> THtaxinfo = taxinfo.FindElements(By.TagName("th"));
                    IList<IWebElement> TDtaxinfo;
                    foreach (IWebElement row in TRtaxinfo)
                    {
                        TDtaxinfo = row.FindElements(By.TagName("td"));

                        if (TDtaxinfo.Count != 0 && !row.Text.Contains("Property Address"))
                        {
                            string Taxhistorydetails = TDtaxinfo[0].Text + "~" + TDtaxinfo[1].Text + "~" + TDtaxinfo[2].Text + "~" + TDtaxinfo[3].Text + "~" + TDtaxinfo[4].Text + "~" + TDtaxinfo[5].Text + "~" + TDtaxinfo[6].Text + "~" + TDtaxinfo[7].Text + "~" + TDtaxinfo[8].Text;
                            gc.insert_date(orderNumber, parcelNumber, 1463, Taxhistorydetails, 1, DateTime.Now);
                        }
                    }

                    int Tax_Year = DateTime.Now.Year;
                    List<string> bililist = new List<string>();
                    int I = 0;
                    IWebElement taxhistory = driver.FindElement(By.XPath("//*[@id='tbl_tax_results']"));
                    IList<IWebElement> TRtaxhistory = taxhistory.FindElements(By.TagName("tr"));
                    IList<IWebElement> THtaxhistory = taxhistory.FindElements(By.TagName("th"));
                    IList<IWebElement> TDtaxhistory;
                    foreach (IWebElement row in TRtaxhistory)
                    {
                        TDtaxhistory = row.FindElements(By.TagName("td"));

                        if (TDtaxhistory.Count != 0 && !row.Text.Contains("Property Address") && bililist.Count < 4)
                        {
                            IWebElement Billlink = TDtaxhistory[1].FindElement(By.TagName("a"));
                            string Billhref = Billlink.GetAttribute("href");
                            bililist.Add(Billhref);
                            Thread.Sleep(1000);
                            I++;
                        }
                        if (bililist.Count == 3)
                        {
                            break;
                        }
                    }
                    foreach (string Link in bililist)
                    {
                        // tax Bill Details
                        string taxyear = "";
                        driver.Navigate().GoToUrl(Link);
                        try
                        {
                            taxyear = driver.FindElement(By.XPath("//*[@id='tax_pay_bill']/div/div[2]/div[1]/div/h3/u")).Text;
                            taxyear = taxyear.Replace("Property Tax Statement", "").Trim();

                        }
                        catch { }
                        IWebElement taxbill = driver.FindElement(By.Id("tbl_tax_bill_total"));
                        IList<IWebElement> TRtaxbill = taxbill.FindElements(By.TagName("tr"));
                        IList<IWebElement> THtaxbill = taxbill.FindElements(By.TagName("th"));
                        IList<IWebElement> TDtaxbill;
                        foreach (IWebElement row in TRtaxbill)
                        {
                            TDtaxbill = row.FindElements(By.TagName("td"));

                            if (TDtaxbill.Count != 0 && !row.Text.Contains("Prior Payment"))
                            {
                                string TaxBilldetails = taxyear + "~" + TDtaxbill[0].Text + "~" + TDtaxbill[1].Text + "~" + TDtaxbill[2].Text + "~" + TDtaxbill[3].Text + "~" + TDtaxbill[4].Text + "~" + TDtaxbill[5].Text.Replace("Pay", "").Trim();
                                gc.insert_date(orderNumber, parcelNumber, 1461, TaxBilldetails, 1, DateTime.Now);
                            }
                        }

                        // Tax Breakdown Details Table: 

                        IWebElement taxbreakdown = driver.FindElement(By.XPath("//*[@id='tbl_tax_positions']"));
                        IList<IWebElement> TRtaxbreakdown = taxbreakdown.FindElements(By.TagName("tr"));
                        IList<IWebElement> THtaxbreakdown = taxbreakdown.FindElements(By.TagName("th"));
                        IList<IWebElement> TDtaxbreakdown;
                        foreach (IWebElement row in TRtaxbreakdown)
                        {
                            TDtaxbreakdown = row.FindElements(By.TagName("td"));

                            if (TDtaxbreakdown.Count != 0 && !row.Text.Contains("Adjusted FMV") && !row.Text.Contains("TOTALS"))
                            {
                                string TaxBreakDowndetails = taxyear + "~" + TDtaxbreakdown[0].Text + "~" + TDtaxbreakdown[1].Text + "~" + TDtaxbreakdown[2].Text + "~" + TDtaxbreakdown[3].Text + "~" + TDtaxbreakdown[4].Text + "~" + TDtaxbreakdown[5].Text + "~" + TDtaxbreakdown[6].Text + "~" + TDtaxbreakdown[7].Text + "~" + TDtaxbreakdown[8].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1462, TaxBreakDowndetails, 1, DateTime.Now);
                            }
                            if (TDtaxbreakdown.Count != 0 && !row.Text.Contains("Adjusted FMV") && row.Text.Contains("TOTALS"))
                            {
                                string TaxBreakDowndetails = taxyear + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + TDtaxbreakdown[0].Text + "~" + TDtaxbreakdown[1].Text + "~" + TDtaxbreakdown[2].Text + "~" + TDtaxbreakdown[3].Text + "~" + TDtaxbreakdown[4].Text;
                                gc.insert_date(orderNumber, parcelNumber, 1462, TaxBreakDowndetails, 1, DateTime.Now);
                            }
                        }

                        // Tax Information Details

                        string Taxstatus = driver.FindElement(By.XPath("//*[@id='tax_pay_bill']/div/div[3]/div[3]/table/tbody")).Text;
                        string Taxpayer = "", Mapcode = "", Description = "", Location = "", Billno = "", TaxAuth = "";
                        Taxpayer = gc.Between(Taxstatus, "Tax Payer:", "Map Code:");
                        Mapcode = gc.Between(Taxstatus, "Map Code:", "Description:").Trim();
                        Description = gc.Between(Taxstatus, "Description:", "Location:").Trim();
                        Location = gc.Between(Taxstatus, "Location:", "Bill No:").Trim();
                        Billno = GlobalClass.After(Taxstatus, "Bill No:").Replace("\r\n", "").Trim();
                        TaxAuth = driver.FindElement(By.XPath("//*[@id='tax_pay_bill']/div/div[3]/div[1]/div")).Text.Replace("Tax Commissioner", "").Replace("\r\n", " ").Trim();

                        IWebElement taxstatus = driver.FindElement(By.Id("tbl_tax_bill_item"));
                        IList<IWebElement> TRtaxstatus = taxstatus.FindElements(By.TagName("tr"));
                        IList<IWebElement> THtaxstatus = taxstatus.FindElements(By.TagName("th"));
                        IList<IWebElement> TDtaxstatus;
                        foreach (IWebElement row in TRtaxstatus)
                        {
                            TDtaxstatus = row.FindElements(By.TagName("td"));

                            if (TDtaxstatus.Count != 0 && !row.Text.Contains("Fair Market Value") && !row.Text.Contains("Billing Date"))
                            {
                                string Taxstatusdetails = taxyear + "~" + Taxpayer + "~" + Mapcode + "~" + Description + "~" + Location + "~" + Billno + "~" + TDtaxstatus[0].Text + "~" + TDtaxstatus[1].Text + "~" + TDtaxstatus[2].Text + "~" + TDtaxstatus[3].Text + "~" + TDtaxstatus[4].Text + "~" + TDtaxstatus[5].Text + "~" + TaxAuth;
                                gc.insert_date(orderNumber, parcelNumber, 1466, Taxstatusdetails, 1, DateTime.Now);
                            }
                        }

                        // Tax payment status Details
                        string valuetype = "", Amount = "";
                        valuetype += "Tax Year" + "~";
                        Amount += Tax_Year + "~";
                        IWebElement Assessmentdetails = driver.FindElement(By.XPath("//*[@id='tax_pay_bill']/div/div[5]/div[2]/table/tbody"));
                        IList<IWebElement> TRAssessmentdetails = Assessmentdetails.FindElements(By.TagName("tr"));
                        IList<IWebElement> THAssessmentdetails = Assessmentdetails.FindElements(By.TagName("th"));
                        IList<IWebElement> TDAssessmentdetails;
                        foreach (IWebElement row in TRAssessmentdetails)
                        {
                            TDAssessmentdetails = row.FindElements(By.TagName("td"));
                            if (row.Text.Trim() != "" && TDAssessmentdetails.Count == 2)
                            {
                                valuetype += TDAssessmentdetails[0].Text + "~";
                                Amount += TDAssessmentdetails[1].Text + "~";
                            }
                        }


                        db.ExecuteQuery("update data_field_master set Data_Fields_Text='" + valuetype.Remove(valuetype.Length - 1, 1) + "' where Id = '" + 1467 + "'");
                        gc.insert_date(orderNumber, parcelNumber, 1467, Amount.Remove(Amount.Length - 1, 1), 1, DateTime.Now);
                        //Tax_Year--;
                    }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "GA", "Walton", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "GA", "Walton");
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