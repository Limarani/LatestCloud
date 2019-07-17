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
    public class WebDriver_FLFlagler
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_FLFlagler(string address, string assessment_id, string parcelNumber, string searchType, string orderNumber, string directParcel, string ownername)
        {
            string Parcelno = "", Owner = "", Property_Address = "", City = "", MultiOwner_details = "", MultiAddress_details = "";
            string Par = "", Parcel_ID = "", Loc = "", Location = "", Lg_Des = "", Legal = "", Pro_Code = "", Property_Code = "", Distr = "", Property_District = "", Mi_Rate = "", Pro_Milagerate = "", Acres = "", Acreage = "", PRo_Home = "", Homestead = "", Pro_Owner = "", Pro_MilingAddreess = "", Pro_YearBuilt = "", property = "";
            string Assemnt_Details1 = "", Assemnt_Details2 = "", Assemnt_Details3 = "", Assemnt_Details4 = "", Year1 = "", Year2 = "", Year3 = "", Year4 = "", currenttax = "", curtax = "", prior_total = "", priortotlinsert = "", taxhis = "", Url = "", tax_authority = "", Tax_Sale = "", tax_sales = "", taxhistoryownername = "";
            string exempt = "", exempt_amount = "", EscrowCode = "", milage_code = "", taxhistoryPayid = "", tax_type = "", tax_years = "", valoremtotal = "", total_milage = "", total_Asstax = "", valoremtax = "", nonvaloremtax = "", total_ass = "", Total_taxes = "", valoremtotalass = "", valoremtotaltax = "", curtaxhis = "", date_paid = "", Transaction = "", Receipt = "", Amount_Paid = "", Item = "";

            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;
            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            //driver = new PhantomJSDriver();
            //driver = new ChromeDriver();
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    if (searchType == "titleflex")
                    {
                        string titleaddress = address;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "FL", "Flagler");

                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_FLFlagler"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("https://qpublic.schneidercorp.com/Application.aspx?App=FlaglerCountyFL&Layer=Parcels&PageType=Search");
                        Thread.Sleep(5000);
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='appBody']/div[4]/div/div/div[3]/a[1]")).Click();
                            Thread.Sleep(2000);
                        }
                        catch { }

                        driver.FindElement(By.Id("ctlBodyPane_ctl00_ctl01_txtAddress")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "FL", "Flagler");
                        driver.FindElement(By.Id("ctlBodyPane_ctl00_ctl01_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        try
                        {
                            IWebElement MultiAddressTB = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_ctl01_gvwParcelResults']/tbody"));
                            IList<IWebElement> MultiAddressTR = MultiAddressTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiAddressTD;
                            gc.CreatePdf_WOP(orderNumber, "Multi Address search", driver, "FL", "Flagler");
                            int AddressmaxCheck = 0;
                            foreach (IWebElement MultiAddress in MultiAddressTR)
                            {
                                if (AddressmaxCheck <= 25)
                                {
                                    MultiAddressTD = MultiAddress.FindElements(By.TagName("td"));
                                    if (MultiAddressTD.Count != 0)
                                    {
                                        Parcelno = MultiAddressTD[2].Text;
                                        Owner = MultiAddressTD[3].Text;
                                        Property_Address = MultiAddressTD[4].Text;
                                        City = MultiAddressTD[5].Text;

                                        MultiAddress_details = Owner + "~" + Property_Address + "~" + City;
                                        gc.insert_date(orderNumber, Parcelno, 1289, MultiAddress_details, 1, DateTime.Now);
                                    }
                                    AddressmaxCheck++;
                                }
                            }
                            if (MultiAddressTR.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_FLFlagler_Multicount"] = "Maximum";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiparcel_FLFlagler"] = "Yes";
                            }
                            driver.Quit();
                            return "MultiParcel";
                        }
                        catch
                        { }
                        try
                        {
                            string nodata = driver.FindElement(By.Id("ctlBodyPane_noDataList_pnlNoResults")).Text;
                            if (nodata.Contains("No results"))
                            {
                                HttpContext.Current.Session["Nodata_FLFlagler"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("https://qpublic.schneidercorp.com/Application.aspx?App=FlaglerCountyFL&Layer=Parcels&PageType=Search");
                        Thread.Sleep(2000);
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='appBody']/div[4]/div/div/div[3]/a[1]")).Click();
                            Thread.Sleep(2000);
                        }
                        catch { }

                        driver.FindElement(By.Id("ctlBodyPane_ctl02_ctl01_txtParcelID")).SendKeys(parcelNumber);
                        gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearch", driver, "FL", "Flagler");
                        driver.FindElement(By.Id("ctlBodyPane_ctl02_ctl01_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                    }

                    if (searchType == "ownername")
                    {
                        driver.Navigate().GoToUrl("https://qpublic.schneidercorp.com/Application.aspx?App=FlaglerCountyFL&Layer=Parcels&PageType=Search");
                        Thread.Sleep(2000);
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='appBody']/div[4]/div/div/div[3]/a[1]")).Click();
                            Thread.Sleep(2000);
                        }
                        catch { }

                        driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_txtName")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "Owner search", driver, "FL", "Flagler");
                        driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        try
                        {
                            IWebElement MultiOwnerTB = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_ctl01_gvwParcelResults']/tbody"));
                            IList<IWebElement> MultiOwnerTR = MultiOwnerTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiOwnerTD;
                            gc.CreatePdf_WOP(orderNumber, "Multi Owner search", driver, "FL", "Flagler");
                            int OwnermaxCheck = 0;
                            foreach (IWebElement MultiOwner in MultiOwnerTR)
                            {
                                if (OwnermaxCheck <= 25)
                                {
                                    MultiOwnerTD = MultiOwner.FindElements(By.TagName("td"));
                                    if (MultiOwnerTD.Count != 0)
                                    {
                                        Parcelno = MultiOwnerTD[2].Text;
                                        Owner = MultiOwnerTD[3].Text;
                                        Property_Address = MultiOwnerTD[4].Text;
                                        City = MultiOwnerTD[5].Text;

                                        MultiOwner_details = Owner + "~" + Property_Address + "~" + City;
                                        gc.insert_date(orderNumber, Parcelno, 1289, MultiOwner_details, 1, DateTime.Now);
                                    }
                                    OwnermaxCheck++;
                                }
                            }
                            if (MultiOwnerTR.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_FLFlagler_Multicount"] = "Maximum";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiparcel_FLFlagler"] = "Yes";
                            }
                            driver.Quit();
                            return "MultiParcel";
                        }
                        catch
                        { }
                    }
                    try
                    {
                        IWebElement Inodata = driver.FindElement(By.Id("ctlBodyPane_noDataList_pnlNoResults"));
                        if(Inodata.Text.Contains("No results match"))
                        {
                            HttpContext.Current.Session["Nodata_FLFlagler"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    //Property Details
                    IWebElement PropertyTB = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl01_mSection']/div/table/tbody"));
                    IList<IWebElement> PropertyTR = PropertyTB.FindElements(By.TagName("tr"));
                    IList<IWebElement> PropertyTD;
                    foreach (IWebElement Property in PropertyTR)
                    {
                        PropertyTD = Property.FindElements(By.TagName("td"));
                        if (PropertyTD.Count != 0)
                        {
                            Par = PropertyTD[0].Text;
                            if (Par.Contains("Parcel ID"))
                            {
                                Parcel_ID = PropertyTD[1].Text;
                            }

                            Loc = PropertyTD[0].Text;
                            if (Loc.Contains("Location Address"))
                            {
                                Location = PropertyTD[1].Text;
                            }

                            Lg_Des = PropertyTD[0].Text;
                            if (Lg_Des.Contains("Brief Tax Description*"))
                            {
                                Legal = PropertyTD[1].Text;
                            }

                            Pro_Code = PropertyTD[0].Text;
                            if (Pro_Code.Contains("Property Use Code"))
                            {
                                Property_Code = PropertyTD[1].Text;
                            }

                            Distr = PropertyTD[0].Text;
                            if (Distr.Contains("Tax District"))
                            {
                                Property_District = PropertyTD[1].Text;
                            }

                            Mi_Rate = PropertyTD[0].Text;
                            if (Mi_Rate.Contains("Millage Rate"))
                            {
                                Pro_Milagerate = PropertyTD[1].Text;
                            }

                            Acres = PropertyTD[0].Text;
                            if (Acres.Contains("Acreage"))
                            {
                                Acreage = PropertyTD[1].Text;
                            }

                            PRo_Home = PropertyTD[0].Text;
                            if (PRo_Home.Contains("Homestead"))
                            {
                                Homestead = PropertyTD[1].Text;
                            }
                        }
                    }

                    try
                    {
                        Pro_Owner = driver.FindElement(By.Id("ctlBodyPane_ctl00_ctl01_lstPrimaryOwner_ctl00_lblPrimaryOwnerName_lblSearch")).Text;
                        Pro_MilingAddreess = driver.FindElement(By.Id("ctlBodyPane_ctl00_ctl01_lstPrimaryOwner_ctl00_lblPrimaryOwnerAddress")).Text;
                        Pro_YearBuilt = driver.FindElement(By.Id("ctlBodyPane_ctl08_ctl01_lstBuildings_ctl00_Label1")).Text;
                    }
                    catch
                    { }

                    property = Location + "~" + Legal + "~" + Pro_Owner + "~" + Property_Code + "~" + Property_District + "~" + Pro_Milagerate + "~" + Acreage + "~" + Homestead + "~" + Pro_MilingAddreess + "~" + Pro_YearBuilt;
                    gc.CreatePdf(orderNumber, Parcel_ID, "Property Details", driver, "FL", "Flagler");
                    gc.insert_date(orderNumber, Parcel_ID, 1290, property, 1, DateTime.Now);

                    //Assessment Details
                    //*[@id="ctlBodyPane_ctl02_ctl01_grdValuation"]/thead
                    IWebElement AssmThTb = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl02_ctl01_grdValuation']/thead"));
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
                            Year4 = AssmTh[3].Text;
                        }
                    }

                    IWebElement AssmTb = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl02_ctl01_grdValuation']/tbody"));
                    IList<IWebElement> AssmTr = AssmTb.FindElements(By.TagName("tr"));
                    IList<IWebElement> AssmTd;
                    int j = 0;

                    List<string> Building_Value = new List<string>();
                    List<string> Extra_Feature_Value = new List<string>();
                    List<string> Land_Value = new List<string>();
                    List<string> Land_Agricultural_Value = new List<string>();
                    List<string> Agricultural_Value = new List<string>();
                    List<string> Just_Value = new List<string>();
                    List<string> Assessed_Value = new List<string>();
                    List<string> Exempt_Value = new List<string>();
                    List<string> Taxable_Value = new List<string>();
                    List<string> Protected_Value = new List<string>();

                    foreach (IWebElement Assmrow in AssmTr)
                    {
                        AssmTd = Assmrow.FindElements(By.TagName("td"));

                        if (AssmTd.Count != 0)
                        {
                            if (j == 0)
                            {
                                Building_Value.Add(AssmTd[1].Text);
                                Building_Value.Add(AssmTd[2].Text);
                                Building_Value.Add(AssmTd[3].Text);
                                Building_Value.Add(AssmTd[4].Text);
                            }

                            else if (j == 1)
                            {
                                Extra_Feature_Value.Add(AssmTd[1].Text);
                                Extra_Feature_Value.Add(AssmTd[2].Text);
                                Extra_Feature_Value.Add(AssmTd[3].Text);
                                Extra_Feature_Value.Add(AssmTd[4].Text);
                            }

                            else if (j == 2)
                            {
                                Land_Value.Add(AssmTd[1].Text);
                                Land_Value.Add(AssmTd[2].Text);
                                Land_Value.Add(AssmTd[3].Text);
                                Land_Value.Add(AssmTd[4].Text);
                            }

                            else if (j == 3)
                            {
                                Land_Agricultural_Value.Add(AssmTd[1].Text);
                                Land_Agricultural_Value.Add(AssmTd[2].Text);
                                Land_Agricultural_Value.Add(AssmTd[3].Text);
                                Land_Agricultural_Value.Add(AssmTd[4].Text);
                            }

                            else if (j == 4)
                            {
                                Agricultural_Value.Add(AssmTd[1].Text);
                                Agricultural_Value.Add(AssmTd[2].Text);
                                Agricultural_Value.Add(AssmTd[3].Text);
                                Agricultural_Value.Add(AssmTd[4].Text);
                            }

                            else if (j == 5)
                            {
                                Just_Value.Add(AssmTd[1].Text);
                                Just_Value.Add(AssmTd[2].Text);
                                Just_Value.Add(AssmTd[3].Text);
                                Just_Value.Add(AssmTd[4].Text);
                            }

                            else if (j == 6)
                            {
                                Assessed_Value.Add(AssmTd[1].Text);
                                Assessed_Value.Add(AssmTd[2].Text);
                                Assessed_Value.Add(AssmTd[3].Text);
                                Assessed_Value.Add(AssmTd[4].Text);
                            }

                            else if (j == 7)
                            {
                                Exempt_Value.Add(AssmTd[1].Text);
                                Exempt_Value.Add(AssmTd[2].Text);
                                Exempt_Value.Add(AssmTd[3].Text);
                                Exempt_Value.Add(AssmTd[4].Text);
                            }

                            else if (j == 8)
                            {
                                Taxable_Value.Add(AssmTd[1].Text);
                                Taxable_Value.Add(AssmTd[2].Text);
                                Taxable_Value.Add(AssmTd[3].Text);
                                Taxable_Value.Add(AssmTd[4].Text);
                            }

                            else if (j == 9)
                            {
                                Protected_Value.Add(AssmTd[1].Text);
                                Protected_Value.Add(AssmTd[2].Text);
                                Protected_Value.Add(AssmTd[3].Text);
                                Protected_Value.Add(AssmTd[4].Text);
                            }

                            j++;
                        }
                    }
                    Assemnt_Details1 = Year1 + "~" + Building_Value[0] + "~" + Extra_Feature_Value[0] + "~" + Land_Value[0] + "~" + Land_Agricultural_Value[0] + "~" + Agricultural_Value[0] + "~" + Just_Value[0] + "~" + Assessed_Value[0] + "~" + Exempt_Value[0] + "~" + Taxable_Value[0] + "~" + Protected_Value[0];
                    Assemnt_Details2 = Year2 + "~" + Building_Value[1] + "~" + Extra_Feature_Value[1] + "~" + Land_Value[1] + "~" + Land_Agricultural_Value[1] + "~" + Agricultural_Value[1] + "~" + Just_Value[1] + "~" + Assessed_Value[1] + "~" + Exempt_Value[1] + "~" + Taxable_Value[1] + "~" + Protected_Value[1];
                    Assemnt_Details3 = Year3 + "~" + Building_Value[2] + "~" + Extra_Feature_Value[2] + "~" + Land_Value[2] + "~" + Land_Agricultural_Value[2] + "~" + Agricultural_Value[2] + "~" + Just_Value[2] + "~" + Assessed_Value[2] + "~" + Exempt_Value[2] + "~" + Taxable_Value[2] + "~" + Protected_Value[2];
                    Assemnt_Details4 = Year4 + "~" + Building_Value[3] + "~" + Extra_Feature_Value[3] + "~" + Land_Value[3] + "~" + Land_Agricultural_Value[3] + "~" + Agricultural_Value[3] + "~" + Just_Value[3] + "~" + Assessed_Value[3] + "~" + Exempt_Value[3] + "~" + Taxable_Value[3] + "~" + Protected_Value[3];

                    gc.insert_date(orderNumber, Parcel_ID, 1291, Assemnt_Details1, 1, DateTime.Now);
                    gc.insert_date(orderNumber, Parcel_ID, 1291, Assemnt_Details2, 1, DateTime.Now);
                    gc.insert_date(orderNumber, Parcel_ID, 1291, Assemnt_Details3, 1, DateTime.Now);
                    gc.insert_date(orderNumber, Parcel_ID, 1291, Assemnt_Details4, 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    //Tax Details

                    driver.Navigate().GoToUrl("http://fl-flagler-taxcollector.governmax.com/collectmax/collect30.asp");
                    Thread.Sleep(5000);
                    IWebElement iframeElement = driver.FindElement(By.XPath("/html/frameset/frame"));
                    driver.SwitchTo().Frame(iframeElement);
                    IWebElement img = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[4]/td/center")).FindElement(By.TagName("a"));
                    string imghref = img.GetAttribute("href");
                    driver.Navigate().GoToUrl(imghref);
                   // img.SendKeys(Keys.Enter);
                    Thread.Sleep(5000);
                                               // /html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[1]/td/table/tbody/tr/td/table/tbody/tr[2]/td/font/input
                    //driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr[4]/td/center/a")).SendKeys(Keys.Enter);
                   // Thread.Sleep(5000);
                    driver.FindElement(By.Name("account")).SendKeys(Parcel_ID);
                    gc.CreatePdf(orderNumber, Parcel_ID, "TaxPage Input Passed", driver, "FL", "Flagler");
                    driver.FindElement(By.Name("go")).SendKeys(Keys.Enter);
                    Thread.Sleep(5000);
                    gc.CreatePdf(orderNumber, Parcel_ID, "Tax Deails", driver, "FL", "Flagler");
                    try
                    {
                        exempt = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[6]/td/table/tbody/tr/td/table/tbody/tr[2]/td[1]/font")).Text.Trim();
                    }
                    catch { }

                    try
                    {
                        IWebElement exemtable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[6]/td/table/tbody/tr/td/table/tbody/tr[2]/td[1]/table/tbody"));
                        IList<IWebElement> exemtableRow = exemtable.FindElements(By.TagName("tr"));
                        int exemtablecount = exemtableRow.Count;
                        IList<IWebElement> exemtablerowTD;
                        int a = 0;
                        foreach (IWebElement rowid in exemtableRow)
                        {
                            exemtablerowTD = rowid.FindElements(By.TagName("td"));
                            if (exemtablerowTD.Count != 0 && !rowid.Text.Contains("Taxing Authority"))
                            {
                                if (a == 0)
                                {
                                    exempt = exemtablerowTD[0].Text;
                                    exempt_amount = exemtablerowTD[1].Text;
                                }
                                else if (a == 1)
                                {
                                    exempt += ",";

                                    exempt += exemtablerowTD[0].Text;
                                    exempt_amount += ",";
                                    exempt_amount += exemtablerowTD[1].Text;
                                }
                                a++;
                            }
                        }
                    }
                    catch { }

                    try
                    {
                        milage_code = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[6]/td/table/tbody/tr/td/table/tbody/tr[2]/td[2]/font")).Text.Trim();
                        tax_type = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[2]/td[2]/font/b")).Text.Trim();
                        tax_years = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[2]/td[3]/font/b")).Text.Trim();
                        EscrowCode = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[6]/td/table/tbody/tr/td/table/tbody/tr[2]/td[3]/font")).Text;
                    }
                    catch
                    { }

                    //valorem Details
                    try
                    {
                        IWebElement valoremtable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[8]/td/table[1]/tbody"));
                        IList<IWebElement> valoremtableRow = valoremtable.FindElements(By.TagName("tr"));
                        int valoremtablerowcount = valoremtableRow.Count;
                        IList<IWebElement> valoremtablerowTD;
                        foreach (IWebElement rowid in valoremtableRow)
                        {
                            valoremtablerowTD = rowid.FindElements(By.TagName("td"));
                            if (valoremtablerowTD.Count != 0 && !rowid.Text.Contains("Taxing Authority"))
                            {

                                if (valoremtablerowTD.Count == 6)
                                {
                                    valoremtax = valoremtablerowTD[0].Text + "~" + "" + "~" + "Ad Valorem Taxes" + "~" + valoremtablerowTD[1].Text + "~" + valoremtablerowTD[2].Text + "~" + valoremtablerowTD[3].Text + "~" + valoremtablerowTD[4].Text + "~" + valoremtablerowTD[5].Text;
                                    gc.insert_date(orderNumber, Parcel_ID, 1297, valoremtax, 1, DateTime.Now);
                                }
                                else if (valoremtablerowTD.Count == 2)
                                {
                                    valoremtax = valoremtablerowTD[0].Text + "~" + "" + "~" + "Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + "";
                                    gc.insert_date(orderNumber, Parcel_ID, 1297, valoremtax, 1, DateTime.Now);
                                }
                            }
                        }

                        total_milage = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[8]/td/table[2]/tbody/tr/td[2]/font")).Text.Trim();
                        total_Asstax = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[8]/td/table[2]/tbody/tr/td[4]/font")).Text.Trim();
                        valoremtotal = "Total" + "~" + "" + "~" + "Ad Valorem Taxes" + "~" + total_milage + "~" + " " + "~" + " " + "~" + " " + "~" + total_Asstax;
                        gc.insert_date(orderNumber, Parcel_ID, 1297, valoremtotal, 1, DateTime.Now);

                    }
                    catch { }

                    try
                    {
                        IWebElement nonvaloremtable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[10]/td/table[1]/tbody"));
                        IList<IWebElement> nonvaloremtableRow = nonvaloremtable.FindElements(By.TagName("tr"));
                        int nonvaloremtablerowcount = nonvaloremtableRow.Count;
                        IList<IWebElement> nonvaloremtablerowTD;
                        foreach (IWebElement rowid in nonvaloremtableRow)
                        {
                            nonvaloremtablerowTD = rowid.FindElements(By.TagName("td"));
                            if (nonvaloremtablerowTD.Count != 0 && !rowid.Text.Contains("Levying Authority") && !rowid.Text.Trim().Contains(""))
                            {
                                nonvaloremtax = "";
                                try
                                {
                                    nonvaloremtax = nonvaloremtablerowTD[1].Text + "~" + nonvaloremtablerowTD[0].Text + "~" + "Non-Ad Valorem Taxes" + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + nonvaloremtablerowTD[2].Text;
                                    gc.insert_date(orderNumber, Parcel_ID, 1297, nonvaloremtax, 1, DateTime.Now);
                                }
                                catch { }
                            }
                        }

                        total_ass = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[10]/td/table[2]/tbody/tr/td[2]/font")).Text.Trim();
                        Total_taxes = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[3]/tbody/tr[11]/td/table/tbody/tr/td[4]/font")).Text.Trim();
                        valoremtotalass = "Total" + "~" + "" + "~" + "Total Assessments" + "~" + "" + "~" + " " + "~" + " " + "~" + " " + "~" + total_ass;
                        gc.insert_date(orderNumber, Parcel_ID, 1297, valoremtotalass, 1, DateTime.Now);
                        valoremtotaltax = "Total" + "~" + "" + "~" + "Taxes & Assessments" + "~" + "" + "~" + " " + "~" + " " + "~" + " " + "~" + Total_taxes;
                        gc.insert_date(orderNumber, Parcel_ID, 1297, valoremtotaltax, 1, DateTime.Now);
                    }
                    catch
                    { }

                    //Tax Due
                    try
                    {
                        IWebElement taxduetable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[4]/tbody/tr/td/table/tbody"));
                        IList<IWebElement> taxduetableRow = taxduetable.FindElements(By.TagName("tr"));
                        int taxduerowcount = taxduetableRow.Count;
                        IList<IWebElement> taxduetablerowTD;
                        foreach (IWebElement rowid in taxduetableRow)
                        {
                            taxduetablerowTD = rowid.FindElements(By.TagName("td"));
                            if (taxduetablerowTD.Count != 0 && !rowid.Text.Contains("If Paid By"))
                            {
                                try
                                {
                                    curtax = taxduetablerowTD[0].Text + "~" + taxduetablerowTD[1].Text;
                                    gc.insert_date(orderNumber, Parcel_ID, 1298, curtax, 1, DateTime.Now);
                                }
                                catch { }
                            }
                        }
                    }
                    catch { }

                    //curtaxhistory
                    try
                    {
                        IWebElement taxpaidtable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[5]"));
                        IList<IWebElement> taxpaidtableRow = taxpaidtable.FindElements(By.TagName("tr"));
                        int taxpaidrowcount = taxpaidtableRow.Count;
                        IList<IWebElement> taxduetablerowTD;
                        foreach (IWebElement rowid in taxpaidtableRow)
                        {
                            taxduetablerowTD = rowid.FindElements(By.TagName("td"));
                            if (taxduetablerowTD.Count != 0 && !rowid.Text.Contains("Date Paid"))
                            {
                                try
                                {
                                    date_paid = taxduetablerowTD[0].Text; Transaction = taxduetablerowTD[1].Text; Receipt = taxduetablerowTD[2].Text;
                                    Item = taxduetablerowTD[3].Text; Amount_Paid = taxduetablerowTD[4].Text;
                                    //currenttax = "" + "~" + "" + "~" + "" + "~" + "" + "~" + Item + "~" + "" + "~" + Amount_Paid + "~" + date_paid + "~" + "" + "~" + Receipt + "~" + "" + "~" + Transaction + "~" + "";
                                    //gc.insert_date(orderNumber, Parcel_ID, 1299, currenttax, 1, DateTime.Now);
                                }
                                catch { }
                            }
                        }
                    }
                    catch
                    { }

                    //prior year tax 
                    try
                    {
                        IWebElement priortaxtable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[6]/tbody/tr[2]/td/table[1]/tbody"));
                        IList<IWebElement> priortaxtableRow = priortaxtable.FindElements(By.TagName("tr"));
                        int priorrowcount = priortaxtableRow.Count;
                        IList<IWebElement> priortaxtablerowTD;
                        foreach (IWebElement rowid in priortaxtableRow)
                        {
                            priortaxtablerowTD = rowid.FindElements(By.TagName("td"));
                            if (priortaxtablerowTD.Count != 0 && !rowid.Text.Contains("Year"))
                            {
                                try
                                {
                                    curtax = priortaxtablerowTD[0].Text + "~" + priortaxtablerowTD[1].Text + "~" + priortaxtablerowTD[2].Text + "~" + priortaxtablerowTD[3].Text + "~" + priortaxtablerowTD[4].Text + "~" + priortaxtablerowTD[5].Text;
                                    gc.insert_date(orderNumber, Parcel_ID, 1300, curtax, 1, DateTime.Now);
                                }
                                catch { }
                            }
                        }
                    }
                    catch { }

                    try
                    {
                        prior_total = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[6]/tbody/tr[2]/td/table[3]/tbody/tr/td[2]")).Text.Trim();
                        priortotlinsert = "Prior Years Total " + "~" + "" + "~" + "" + "~" + "" + "~" + "" + "~" + prior_total;
                        gc.insert_date(orderNumber, Parcel_ID, 1300, priortotlinsert, 1, DateTime.Now);

                        IWebElement priortaxdettable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[7]/tbody"));
                        IList<IWebElement> priortaxdettableRow = priortaxdettable.FindElements(By.TagName("tr"));
                        int priortaxdettablecount = priortaxdettableRow.Count;
                        IList<IWebElement> priortaxdettablerowTD;
                        foreach (IWebElement rowid in priortaxdettableRow)
                        {
                            priortaxdettablerowTD = rowid.FindElements(By.TagName("td"));
                            if (priortaxdettablerowTD.Count != 0 && !rowid.Text.Contains("If Paid By"))
                            {
                                try
                                {
                                    curtax = priortaxdettablerowTD[0].Text + "~" + "Delinquent" + "~" + priortaxdettablerowTD[1].Text;
                                    gc.insert_date(orderNumber, Parcel_ID, 1298, curtax, 1, DateTime.Now);
                                }
                                catch { }
                            }
                        }
                    }
                    catch
                    { }

                    //Tax Sale
                    try
                    {
                        tax_sales = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[4]/tbody/tr/td")).Text;
                        tax_sales = WebDriverTest.Between(tax_sales, "Please contact the ", " at (386)");
                        if (tax_sales == "Tax Department")
                        {
                            Tax_Sale = "Due to the status code assigned to this account, the remaining detail is blocked from viewing. Please contact the Tax Department at (386) 313-4160 for further information regarding this account.";
                            gc.insert_date(orderNumber, Parcel_ID, 1372, Tax_Sale, 1, DateTime.Now);
                        }
                    }
                    catch
                    { }

                    //Tax History Details
                    try
                    {
                        try
                        {
                            driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[6]/tbody/tr/td/font/a")).SendKeys(Keys.Enter);
                            Thread.Sleep(2000);
                        }
                        catch { }
                        try
                        {
                            driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table[5]/tbody/tr/td/font/a")).SendKeys(Keys.Enter);
                        }
                        catch { }
                        Thread.Sleep(7000);
                        gc.CreatePdf(orderNumber, Parcel_ID, "Tax History Deails", driver, "FL", "Flagler");

                        IWebElement tablelist = driver.FindElement(By.XPath("html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td"));
                        IList<IWebElement> tablelists = tablelist.FindElements(By.TagName("table"));
                        int tablecount = tablelists.Count;
                        int k = 3;
                        for (k = 3; k < tablecount - 1; k++)
                        {
                            IWebElement taxhistorytable = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[2]/td[2]/table/tbody/tr/td/table/tbody/tr/td/table[" + k + "]/tbody"));
                            IList<IWebElement> taxhistorytableRow = taxhistorytable.FindElements(By.TagName("tr"));
                            int taxhistorytablecount = taxhistorytableRow.Count;
                            IList<IWebElement> taxhistorytablerowTD;
                            foreach (IWebElement rowid1 in taxhistorytableRow)
                            {
                                taxhistorytablerowTD = rowid1.FindElements(By.TagName("td"));
                                if (k == 3)
                                {
                                    if (taxhistorytablerowTD.Count != 0 && !rowid1.Text.Contains("Payment History") && !rowid1.Text.Contains("Year") && taxhistorytableRow.Count == 4)
                                    {
                                        if (taxhistorytablerowTD.Count == 6)
                                        {
                                            taxhis = taxhistorytablerowTD[0].Text + "~" + taxhistorytablerowTD[1].Text + "~" + taxhistorytablerowTD[2].Text + "~" + taxhistorytablerowTD[3].Text + "~" + taxhistorytablerowTD[4].Text + "~" + taxhistorytablerowTD[5].Text;

                                        }
                                        if (taxhistorytablerowTD.Count == 2)
                                        {
                                            if (taxhistorytablerowTD[0].Text.Trim().Contains("Owner Name"))
                                            {
                                                taxhistoryownername = taxhis + "~" + taxhistorytablerowTD[1].Text + "~" + "";
                                                gc.insert_date(orderNumber, Parcel_ID, 1304, taxhistoryownername, 1, DateTime.Now);
                                            }


                                        }
                                    }
                                    if (taxhistorytablerowTD.Count != 0 && !rowid1.Text.Contains("Payment History") && !rowid1.Text.Contains("Year") && taxhistorytableRow.Count == 5)
                                    {
                                        if (taxhistorytablerowTD.Count == 6)
                                        {
                                            taxhis = taxhistorytablerowTD[0].Text + "~" + taxhistorytablerowTD[1].Text + "~" + taxhistorytablerowTD[2].Text + "~" + taxhistorytablerowTD[3].Text + "~" + taxhistorytablerowTD[4].Text + "~" + taxhistorytablerowTD[5].Text;

                                        }
                                        if (taxhistorytablerowTD.Count == 2)
                                        {
                                            if (taxhistorytablerowTD[0].Text.Trim().Contains("Owner Name"))
                                            {
                                                taxhistoryownername = taxhistorytablerowTD[1].Text;
                                            }
                                            if (taxhistorytablerowTD[0].Text.Trim().Contains("Paid By"))
                                            {
                                                taxhistoryPayid = taxhis + "~" + taxhistoryownername + "~" + taxhistorytablerowTD[1].Text;
                                                gc.insert_date(orderNumber, Parcel_ID, 1304, taxhistoryPayid, 1, DateTime.Now);
                                            }

                                        }
                                    }
                                }
                                else
                                {
                                    if (taxhistorytablerowTD.Count != 0 && !rowid1.Text.Contains("Payment History") && !rowid1.Text.Contains("Year") && taxhistorytableRow.Count == 3)
                                    {
                                        if (taxhistorytablerowTD.Count == 6)
                                        {
                                            taxhis = taxhistorytablerowTD[0].Text + "~" + taxhistorytablerowTD[1].Text + "~" + taxhistorytablerowTD[2].Text + "~" + taxhistorytablerowTD[3].Text + "~" + taxhistorytablerowTD[4].Text + "~" + taxhistorytablerowTD[5].Text;

                                        }
                                        if (taxhistorytablerowTD.Count == 2)
                                        {
                                            if (taxhistorytablerowTD[0].Text.Trim().Contains("Owner Name"))
                                            {
                                                taxhistoryownername = taxhis + "~" + taxhistorytablerowTD[1].Text + "~" + "";
                                                gc.insert_date(orderNumber, Parcel_ID, 1304, taxhistoryownername, 1, DateTime.Now);
                                            }
                                        }
                                    }
                                    if (taxhistorytablerowTD.Count != 0 && !rowid1.Text.Contains("Payment History") && !rowid1.Text.Contains("Year") && taxhistorytableRow.Count == 4)
                                    {
                                        if (taxhistorytablerowTD.Count == 6)
                                        {
                                            taxhis = taxhistorytablerowTD[0].Text + "~" + taxhistorytablerowTD[1].Text + "~" + taxhistorytablerowTD[2].Text + "~" + taxhistorytablerowTD[3].Text + "~" + taxhistorytablerowTD[4].Text + "~" + taxhistorytablerowTD[5].Text;

                                        }
                                        if (taxhistorytablerowTD.Count == 2)
                                        {
                                            if (taxhistorytablerowTD[0].Text.Trim().Contains("Owner Name"))
                                            {
                                                taxhistoryownername = taxhistorytablerowTD[1].Text;
                                            }
                                            if (taxhistorytablerowTD[0].Text.Trim().Contains("Paid By"))
                                            {
                                                taxhistoryPayid = taxhis + "~" + taxhistoryownername + "~" + taxhistorytablerowTD[1].Text;
                                                gc.insert_date(orderNumber, Parcel_ID, 1304, taxhistoryPayid, 1, DateTime.Now);
                                            }

                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch { }

                    //Bill Screeshot
                    try
                    {
                        IWebElement Idownloadurl = driver.FindElement(By.XPath("/html/body/table[2]/tbody/tr[1]/td[1]/table/tbody/tr[2]/td/table/tbody/tr[8]/td/font/a"));
                        Url = Idownloadurl.GetAttribute("href");
                        driver.Navigate().GoToUrl(Url);
                        Thread.Sleep(5000);
                        gc.CreatePdf(orderNumber, Parcel_ID, "Tax Bill", driver, "FL", "Flagler");
                    }
                    catch
                    { }

                    //Tax Authority
                    try
                    {
                        tax_authority = "Flagler County Tax Collector 1769 E. Moody Blvd. Suite 102 Bunnell, FL 32110 Phone: (386) 313-4160   FAX: (386) 313-4161";
                    }
                    catch
                    { }

                    curtaxhis = exempt + "~" + exempt_amount + "~" + milage_code + "~" + EscrowCode + "~" + Item + "~" + tax_type + "~" + Total_taxes + "~" + Amount_Paid + "~" + date_paid + "~" + Receipt + "~" + Transaction + "~" + tax_authority;
                    gc.insert_date(orderNumber, Parcel_ID, 1299, curtaxhis, 1, DateTime.Now);
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "FL", "Flagler", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "FL", "Flagler");
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