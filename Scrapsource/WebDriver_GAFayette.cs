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
    public class WebDriver_GAFayette
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_GAFayette(string address, string assessment_id, string parcelNumber, string searchType, string orderNumber, string directParcel, string ownername)
        {
            string Parcelno = "", Owner = "", Property_Address = "", Legal_Desp = "", MultiOwner_details = "", MultiAddress_details = "";
            string Parcel_ID = "", Location = "", Legal = "", Property_Class = "", Nighbrhod = "", Tax_Dis = "", Zoing = "", Acres = "", Homestd = "", Exemption = "", Overall_OwnMailAdd = "", Year_Built = "";
            string property = "", yblt = "", Mailing_Address2 = "", Mailing_Address1 = "", Owner1 = "", Owner2 = "", Exemp = "", Home = "", Acrs = "", Zone = "", tx_Dis = "", Nhg = "", Pro_Cls = "", Lg_Des = "", Loc = "", Par = "";
            string Year1 = "", Year2 = "", Year3 = "", Year4 = "", Assemnt_Details1 = "", Assemnt_Details2 = "", Assemnt_Details3 = "", Assemnt_Details4 = "", total = "", Total_Payments = "", Convey = "", Convey_Fee = "", Paid = "", Total_Paid = "", TaxPayment_details1 = "", TaxPayment_details = "", Balnce_Due = "", Tax_Bill = "", Tax_year = "";
            string Taxy = "", Total_DUE = "", Last_PAYDATE = "", Tax_P = "", Tax_Payer = "", Map = "", Map_Code = "", Desk = "", Desk_Pro = "", Locy = "", Tax_Location = "", Bl_No = "", Tax_Billno = "", Disti = "", Tax_Dist = "", Build_Val = "", Land_Val = "", Tax_Acres = "", FrMkt_Val = "", Tax_DueDate = "", Tax_Exmpions = "", TaxInfo_Details = "";
            string Cu_Du = "", Penalt = "", In_Tx = "", Oth_Fe = "", Pre_Pay = "", Bc_Tx = "", Tl_Du = "", Cur_Due = "", Tax_Penalty = "", Int_Rest = "", Other_Fees = "", Previous_Payments = "", Back_Taxes = "", TaxToal_Due = "", DueTaxInfo_Details = "";
            string Entity = "", FMV = "", Net_Assmnt = "", Exmpl = "", Taxble_Value = "", Millage_Rate = "", Grs_Tax = "", Cretit = "", NetTax = "", CityPayment_details = "", Taxble_Value1 = "", Millage_Rate1 = "", Grs_Tax1 = "", Cretit1 = "", NetTax1 = "";
            string Tax_Auth1 = "", Tax_Auth2 = "", Tax_Authotiry = "", TaxAuthority_Details = "";

            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver()) 
            {

                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    if (searchType == "titleflex")
                    {
                        string titleaddress = address;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "GA", "Fayette");

                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_GAFayette"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }

                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("https://qpublic.schneidercorp.com/Application.aspx?AppID=942&LayerID=18406&PageTypeID=2&PageID=8204");
                        Thread.Sleep(2000);
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='appBody']/div[4]/div/div/div[3]/a[1]")).Click();
                            Thread.Sleep(2000);
                        }
                        catch { }

                        driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_txtAddress")).SendKeys(address);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "GA", "Fayette");
                        driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        try
                        {
                            IWebElement MultiAddressTB = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_ctl01_gvwParcelResults']/tbody"));
                            IList<IWebElement> MultiAddressTR = MultiAddressTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiAddressTD;
                            gc.CreatePdf_WOP(orderNumber, "Multi Address search", driver, "GA", "Fayette");
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
                                        gc.insert_date(orderNumber, Parcelno, 784, MultiAddress_details, 1, DateTime.Now);
                                    }
                                    AddressmaxCheck++;
                                }
                            }
                            if (MultiAddressTR.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_GAFayette_Multicount"] = "Maximum";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiparcel_GAFayette"] = "Yes";
                            }
                            driver.Quit();
                            return "MultiParcel";
                        }
                        catch
                        { }
                    }

                    if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("https://qpublic.schneidercorp.com/Application.aspx?AppID=942&LayerID=18406&PageTypeID=2&PageID=8204");
                        Thread.Sleep(2000);
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='appBody']/div[4]/div/div/div[3]/a[1]")).Click();
                            Thread.Sleep(2000);
                        }
                        catch { }

                        if (parcelNumber.Contains(" ") || parcelNumber.Contains("-"))
                        {
                            parcelNumber = parcelNumber.Replace(" ", "").Replace("-", "");
                        }

                        string CommonParcel = parcelNumber;

                        if (CommonParcel.Length == 9)
                        {
                            driver.FindElement(By.Id("ctlBodyPane_ctl02_ctl01_txtParcelID")).SendKeys(parcelNumber);
                            gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearch", driver, "GA", "Fayette");
                            driver.FindElement(By.Id("ctlBodyPane_ctl02_ctl01_btnSearch")).SendKeys(Keys.Enter);
                            Thread.Sleep(2000);
                        }

                        if (CommonParcel.Length == 7)
                        {
                            driver.FindElement(By.Id("ctlBodyPane_ctl02_ctl01_txtParcelID")).SendKeys(parcelNumber);
                            gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearch", driver, "GA", "Fayette");
                            driver.FindElement(By.Id("ctlBodyPane_ctl02_ctl01_btnSearch")).SendKeys(Keys.Enter);
                            Thread.Sleep(2000);
                        }
                    }

                    if (searchType == "ownername")
                    {
                        driver.Navigate().GoToUrl("https://qpublic.schneidercorp.com/Application.aspx?AppID=942&LayerID=18406&PageTypeID=2&PageID=8204");
                        Thread.Sleep(2000);
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='appBody']/div[4]/div/div/div[3]/a[1]")).Click();
                            Thread.Sleep(2000);
                        }
                        catch { }

                        driver.FindElement(By.Id("ctlBodyPane_ctl00_ctl01_txtName")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "Owner search", driver, "GA", "Fayette");
                        driver.FindElement(By.Id("ctlBodyPane_ctl00_ctl01_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        try
                        {
                            IWebElement MultiOwnerTB = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_ctl01_gvwParcelResults']/tbody"));
                            IList<IWebElement> MultiOwnerTR = MultiOwnerTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiOwnerTD;
                            gc.CreatePdf_WOP(orderNumber, "Multi Owner search", driver, "GA", "Fayette");
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
                                        gc.insert_date(orderNumber, Parcelno, 784, MultiOwner_details, 1, DateTime.Now);
                                    }
                                    AddressmaxCheck++;
                                }
                            }
                            if (MultiOwnerTR.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_GAFayette_Multicount"] = "Maximum";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiparcel_GAFayette"] = "Yes";
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
                        if(Inodata.Text.Contains("No results match your search criteria"))
                        {
                            HttpContext.Current.Session["Nodata_GAFayette"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                    }
                    catch { }
                    //Property Details
                    IWebElement PropertyTB = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_mSection']/div/table[1]/tbody"));
                    IList<IWebElement> PropertyTR = PropertyTB.FindElements(By.TagName("tr"));
                    IList<IWebElement> PropertyTD;
                    foreach (IWebElement Property in PropertyTR)
                    {
                        PropertyTD = Property.FindElements(By.TagName("td"));
                        if (PropertyTD.Count != 0)
                        {
                            Par = PropertyTD[0].Text;
                            if (Par.Contains("Parcel Number"))
                            {
                                Parcel_ID = PropertyTD[1].Text;
                            }

                            Loc = PropertyTD[0].Text;
                            if (Loc.Contains("Location Address"))
                            {
                                Location = PropertyTD[1].Text;
                            }

                            Lg_Des = PropertyTD[0].Text;
                            if (Lg_Des.Contains("Legal Description"))
                            {
                                Legal = PropertyTD[1].Text;
                            }

                            Pro_Cls = PropertyTD[0].Text;
                            if (Pro_Cls.Contains("Property Class"))
                            {
                                Property_Class = PropertyTD[1].Text;
                            }

                            Nhg = PropertyTD[0].Text;
                            if (Nhg.Contains("Neighborhood"))
                            {
                                Nighbrhod = PropertyTD[1].Text;
                            }

                            tx_Dis = PropertyTD[0].Text;
                            if (tx_Dis.Contains("Tax District"))
                            {
                                Tax_Dis = PropertyTD[1].Text;
                            }

                            Zone = PropertyTD[0].Text;
                            if (Zone.Contains("Zoning"))
                            {
                                Zoing = PropertyTD[1].Text;
                            }

                            Acrs = PropertyTD[0].Text;
                            if (Acrs.Contains("Acres"))
                            {
                                Acres = PropertyTD[1].Text;
                            }

                            Home = PropertyTD[0].Text;
                            if (Home.Contains("Homestead"))
                            {
                                Homestd = PropertyTD[1].Text;
                            }

                            Exemp = PropertyTD[0].Text;
                            if (Exemp.Contains("Exemptions"))
                            {
                                Exemption = PropertyTD[1].Text;
                            }
                        }
                    }

                    try
                    {
                        Owner1 = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl01_ctl01_lnkOwnerName1_lblSearch']")).Text;
                        Owner2 = driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_lblOwnerName2")).Text;
                        Mailing_Address1 = driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_lblAddress1")).Text;
                        Mailing_Address2 = driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_lblCityStZip")).Text;
                        Overall_OwnMailAdd = Owner1 + " " + Owner2 + " & " + Mailing_Address1 + " " + Mailing_Address2;
                    }
                    catch
                    { }

                    try
                    {
                        Owner1 = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl01_ctl01_lnkOwnerName1_lblSearch']")).Text;
                        Mailing_Address1 = driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_lblAddress1")).Text;
                        Mailing_Address2 = driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_lblCityStZip")).Text;
                        Overall_OwnMailAdd = Owner1 + " & " + Mailing_Address1 + " " + Mailing_Address2;
                    }
                    catch
                    { }

                    try
                    {
                        Owner1 = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl01_ctl01_lnkOwnerName1_lnkSearch']")).Text;
                        Mailing_Address1 = driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_lblAddress1")).Text;
                        Mailing_Address2 = driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_lblCityStZip")).Text;
                        Overall_OwnMailAdd = Owner1 + " & " + Mailing_Address1 + " " + Mailing_Address2;
                    }
                    catch
                    { }

                    try
                    {
                        IWebElement Yeartb = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl04_mSection']/div/div/div[1]/table/tbody"));
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

                    property = Location + "~" + Legal + "~" + Property_Class + "~" + Nighbrhod + "~" + Tax_Dis + "~" + Zoing + "~" + Acres + "~" + Homestd + "~" + Exemption + "~" + Overall_OwnMailAdd + "~" + Year_Built;
                    gc.CreatePdf(orderNumber, Parcel_ID, "Property Details", driver, "GA", "Fayette");
                    gc.insert_date(orderNumber, Parcel_ID, 785, property, 1, DateTime.Now);

                    //Assessment Details
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

                    List<string> LUC = new List<string>();
                    List<string> Class = new List<string>();
                    List<string> Land_Value = new List<string>();
                    List<string> Build_Value = new List<string>();
                    List<string> Total_Value = new List<string>();
                    List<string> Assed_Value = new List<string>();

                    foreach (IWebElement Assmrow in AssmTr)
                    {
                        AssmTd = Assmrow.FindElements(By.TagName("td"));

                        if (AssmTd.Count != 0)
                        {

                            if (j == 0)
                            {
                                LUC.Add(AssmTd[2].Text);
                                LUC.Add(AssmTd[3].Text);
                                LUC.Add(AssmTd[4].Text);
                                LUC.Add(AssmTd[5].Text);
                            }
                            else if (j == 1)
                            {
                                Class.Add(AssmTd[2].Text);
                                Class.Add(AssmTd[3].Text);
                                Class.Add(AssmTd[4].Text);
                                Class.Add(AssmTd[5].Text);
                            }
                            else if (j == 2)
                            {
                                Land_Value.Add(AssmTd[2].Text);
                                Land_Value.Add(AssmTd[3].Text);
                                Land_Value.Add(AssmTd[4].Text);
                                Land_Value.Add(AssmTd[5].Text);
                            }
                            else if (j == 3)
                            {
                                Build_Value.Add(AssmTd[2].Text);
                                Build_Value.Add(AssmTd[3].Text);
                                Build_Value.Add(AssmTd[4].Text);
                                Build_Value.Add(AssmTd[5].Text);
                            }
                            else if (j == 4)
                            {
                                Total_Value.Add(AssmTd[2].Text);
                                Total_Value.Add(AssmTd[3].Text);
                                Total_Value.Add(AssmTd[4].Text);
                                Total_Value.Add(AssmTd[5].Text);
                            }
                            else if (j == 6)
                            {
                                Assed_Value.Add(AssmTd[2].Text);
                                Assed_Value.Add(AssmTd[3].Text);
                                Assed_Value.Add(AssmTd[4].Text);
                                Assed_Value.Add(AssmTd[5].Text);
                            }

                            j++;
                        }
                    }

                    Assemnt_Details1 = Year1 + "~" + LUC[0] + "~" + Class[0] + "~" + Land_Value[0] + "~" + Build_Value[0] + "~" + Total_Value[0] + "~" + Assed_Value[0];
                    Assemnt_Details2 = Year2 + "~" + LUC[1] + "~" + Class[1] + "~" + Land_Value[1] + "~" + Build_Value[1] + "~" + Total_Value[1] + "~" + Assed_Value[1];
                    Assemnt_Details3 = Year3 + "~" + LUC[2] + "~" + Class[2] + "~" + Land_Value[2] + "~" + Build_Value[2] + "~" + Total_Value[2] + "~" + Assed_Value[2];
                    Assemnt_Details4 = Year4 + "~" + LUC[3] + "~" + Class[3] + "~" + Land_Value[3] + "~" + Build_Value[3] + "~" + Total_Value[3] + "~" + Assed_Value[3];
                    gc.insert_date(orderNumber, Parcel_ID, 791, Assemnt_Details1, 1, DateTime.Now);
                    gc.insert_date(orderNumber, Parcel_ID, 791, Assemnt_Details2, 1, DateTime.Now);
                    gc.insert_date(orderNumber, Parcel_ID, 791, Assemnt_Details3, 1, DateTime.Now);
                    gc.insert_date(orderNumber, Parcel_ID, 791, Assemnt_Details4, 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    try
                    {
                        //Tax Details
                        driver.Navigate().GoToUrl("http://www.fayettecountytaxcomm.com/taxbillsearch.aspx");

                        driver.FindElement(By.Id("Body_MapParcelID1")).SendKeys(Parcel_ID);
                        Thread.Sleep(2000);

                        gc.CreatePdf(orderNumber, Parcel_ID, "Tax Parcel Search", driver, "GA", "Fayette");
                        driver.FindElement(By.Id("Body_MapParcelIDGo")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        gc.CreatePdf(orderNumber, Parcel_ID, "Tax Site", driver, "GA", "Fayette");
                        driver.FindElement(By.XPath("//*[@id='Body_GridView1']/tbody/tr[2]/td[1]/a")).Click();
                        Thread.Sleep(2000);

                        driver.SwitchTo().Window(driver.WindowHandles.Last());
                        Thread.Sleep(3000);

                        //Tax Payment Details
                        try
                        {
                            IWebElement TaxPaymentTB = driver.FindElement(By.XPath("//*[@id='Body_GridView1']/tbody"));
                            IList<IWebElement> TaxPaymentTR = TaxPaymentTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> TaxPaymentTD;

                            foreach (IWebElement TaxPayment in TaxPaymentTR)
                            {
                                TaxPaymentTD = TaxPayment.FindElements(By.TagName("td"));
                                if (TaxPaymentTD.Count != 0 && !TaxPayment.Text.Contains("Year"))
                                {
                                    Tax_year = TaxPaymentTD[0].Text;
                                    Tax_Bill = TaxPaymentTD[1].Text;
                                    Balnce_Due = TaxPaymentTD[2].Text;

                                    TaxPayment_details = Tax_year + "~" + Tax_Bill + "~" + Balnce_Due;
                                    gc.CreatePdf(orderNumber, Parcel_ID, "Tax Payment Details", driver, "GA", "Fayette");
                                    gc.insert_date(orderNumber, Parcel_ID, 792, TaxPayment_details, 1, DateTime.Now);
                                }
                            }
                        }
                        catch
                        { }

                        try
                        {
                            IWebElement Tax2TB = driver.FindElement(By.XPath("//*[@id='form1']/div[3]/div/table/tbody"));
                            IList<IWebElement> Tax2TR = Tax2TB.FindElements(By.TagName("tr"));
                            IList<IWebElement> Tax2TD;
                            foreach (IWebElement Tax2 in Tax2TR)
                            {
                                Tax2TD = Tax2.FindElements(By.TagName("td"));
                                if (Tax2TD.Count != 0)
                                {
                                    total = Tax2TD[0].Text;
                                    if (total.Contains("Total Payments Pending:"))
                                    {
                                        Total_Payments = Tax2TD[1].Text;
                                    }
                                    Convey = Tax2TD[0].Text;
                                    if (Convey.Contains("Convenience Fee:"))
                                    {
                                        Convey_Fee = Tax2TD[1].Text;
                                    }
                                    Paid = Tax2TD[0].Text;
                                    if (Paid.Contains("Total Amount to be paid:"))
                                    {
                                        Total_Paid = Tax2TD[1].Text;
                                    }
                                }
                            }

                            TaxPayment_details1 = "" + "~" + "" + "~" + "" + "~" + Total_Payments + "~" + Convey_Fee + "~" + Total_Paid;
                            gc.insert_date(orderNumber, Parcel_ID, 792, TaxPayment_details1, 1, DateTime.Now);
                        }
                        catch
                        { }
                        //Tax Info Details

                        List<string> TaxInfoSearch = new List<string>();

                        IWebElement TaxInfoTB = driver.FindElement(By.XPath("//*[@id='Body_GridView1']/tbody"));
                        IList<IWebElement> TaxInfoTR = TaxInfoTB.FindElements(By.TagName("tr"));
                        TaxInfoTR.Reverse();

                        int rows_count = TaxInfoTR.Count;

                        for (int row = 0; row < rows_count; row++)
                        {
                            if (row == rows_count - 1 || row == rows_count - 2 || row == rows_count - 3)
                            {
                                IList<IWebElement> Columns_row = TaxInfoTR[row].FindElements(By.TagName("td"));

                                int columns_count = Columns_row.Count;

                                for (int column = 0; column < columns_count; column++)
                                {
                                    if (column == columns_count - 2)
                                    {
                                        IWebElement ParcelBill_link = Columns_row[1].FindElement(By.TagName("a"));
                                        string Parcelurl = ParcelBill_link.GetAttribute("href");
                                        TaxInfoSearch.Add(Parcelurl);
                                    }
                                }
                            }
                        }

                        foreach (string TaxInfobill in TaxInfoSearch)
                        {
                            driver.Navigate().GoToUrl(TaxInfobill);
                            Thread.Sleep(3000);
                            driver.SwitchTo().Window(driver.WindowHandles.Last());
                            Thread.Sleep(2000);
                            try
                            {
                                Taxy = driver.FindElement(By.Id("PropertyTaxLabel")).Text;
                                Taxy = WebDriverTest.Before(Taxy, " Property Tax Statement");
                                Total_DUE = driver.FindElement(By.Id("TotalDueTopText")).Text;
                                Last_PAYDATE = driver.FindElement(By.Id("LastPaymentTopText")).Text;
                                Last_PAYDATE = WebDriverTest.After(Last_PAYDATE, "Last payment made on: ");
                            }
                            catch
                            { }

                            try
                            {
                                IWebElement IntrestTB = driver.FindElement(By.XPath("//*[@id='TaxPayerInfo']/tbody"));
                                IList<IWebElement> IntrestTR = IntrestTB.FindElements(By.TagName("tr"));
                                IList<IWebElement> IntrestTD;
                                foreach (IWebElement Intrest in IntrestTR)
                                {
                                    IntrestTD = Intrest.FindElements(By.TagName("td"));
                                    if (IntrestTD.Count != 0)
                                    {
                                        Tax_P = IntrestTD[0].Text;
                                        if (Tax_P.Contains("Tax Payer:"))
                                        {
                                            Tax_Payer = IntrestTD[1].Text;
                                        }
                                        Map = IntrestTD[0].Text;
                                        if (Map.Contains("Map Code:"))
                                        {
                                            Map_Code = IntrestTD[1].Text;
                                        }
                                        Desk = IntrestTD[0].Text;
                                        if (Desk.Contains("Description:"))
                                        {
                                            Desk_Pro = IntrestTD[1].Text;
                                        }
                                        Locy = IntrestTD[0].Text;
                                        if (Locy.Contains("Location:"))
                                        {
                                            Tax_Location = IntrestTD[1].Text;
                                        }
                                        Bl_No = IntrestTD[0].Text;
                                        if (Bl_No.Contains("Bill No:"))
                                        {
                                            Tax_Billno = IntrestTD[1].Text;
                                        }
                                        Disti = IntrestTD[0].Text;
                                        if (Disti.Contains("District:"))
                                        {
                                            Tax_Dist = IntrestTD[1].Text;
                                        }
                                    }
                                }
                            }
                            catch
                            { }

                            try
                            {
                                IWebElement CinfoTB = driver.FindElement(By.XPath("//*[@id='BottomTable']/tbody"));
                                IList<IWebElement> CinfoTR = CinfoTB.FindElements(By.TagName("tr"));
                                IList<IWebElement> CinfoTD;
                                foreach (IWebElement Cinfo in CinfoTR)
                                {
                                    CinfoTD = Cinfo.FindElements(By.TagName("td"));
                                    if (CinfoTD.Count != 0 && !Cinfo.Text.Contains("Building Value") && Cinfo.Text != "")
                                    {
                                        Build_Val = CinfoTD[0].Text;
                                        Land_Val = CinfoTD[1].Text;
                                        Tax_Acres = CinfoTD[2].Text;
                                        FrMkt_Val = CinfoTD[3].Text;
                                        Tax_DueDate = CinfoTD[4].Text;
                                        Tax_Exmpions = CinfoTD[7].Text;
                                    }
                                }
                            }
                            catch
                            { }

                            gc.CreatePdf(orderNumber, Parcel_ID, "Tax Info Details" + " " + Taxy, driver, "GA", "Fayette");

                            TaxInfo_Details = Taxy + "~" + Tax_Payer + "~" + Map_Code + "~" + Desk_Pro + "~" + Tax_Location + "~" + Tax_Billno + "~" + Tax_Dist + "~" + Last_PAYDATE + "~" + Build_Val + "~" + Land_Val + "~" + Tax_Acres + "~" + FrMkt_Val + "~" + Tax_Exmpions + "~" + Tax_DueDate + "~" + Total_DUE;
                            gc.insert_date(orderNumber, Parcel_ID, 793, TaxInfo_Details, 1, DateTime.Now);
                            Tax_Payer = ""; Map_Code = ""; Desk_Pro = ""; Tax_Location = ""; Tax_Billno = ""; Tax_Dist = ""; Last_PAYDATE = ""; Build_Val = ""; Land_Val = ""; Tax_Acres = ""; FrMkt_Val = ""; Tax_Exmpions = ""; Tax_DueDate = ""; Total_DUE = "";

                            try
                            {
                                IWebElement DueTaxTB = driver.FindElement(By.XPath("//*[@id='CurrentDueTable']/tbody"));
                                IList<IWebElement> DueTaxTR = DueTaxTB.FindElements(By.TagName("tr"));
                                IList<IWebElement> DueTaxTD;

                                foreach (IWebElement DueTax in DueTaxTR)
                                {

                                    DueTaxTD = DueTax.FindElements(By.TagName("td"));
                                    if (DueTaxTD.Count != 0 && DueTaxTD.Count != 1 && DueTaxTD.Count != 8 && DueTaxTD.Count != 9)
                                    {
                                        Cu_Du = DueTaxTD[1].Text;
                                        if (Cu_Du.Contains("Current Due"))
                                        {
                                            Cur_Due = DueTaxTD[2].Text;
                                        }
                                        Penalt = DueTaxTD[1].Text;
                                        if (Penalt.Contains("Penalty"))
                                        {
                                            Tax_Penalty = DueTaxTD[2].Text;
                                        }
                                        In_Tx = DueTaxTD[1].Text;
                                        if (In_Tx.Contains("Interest"))
                                        {
                                            Int_Rest = DueTaxTD[2].Text;
                                        }
                                        Oth_Fe = DueTaxTD[1].Text;
                                        if (Oth_Fe.Contains("Other Fees"))
                                        {
                                            Other_Fees = DueTaxTD[2].Text;
                                        }
                                        Pre_Pay = DueTaxTD[1].Text;
                                        if (Pre_Pay.Contains("Previous Payments"))
                                        {
                                            Previous_Payments = DueTaxTD[2].Text;
                                        }
                                        Bc_Tx = DueTaxTD[1].Text;
                                        if (Bc_Tx.Contains("Back Taxes"))
                                        {
                                            Back_Taxes = DueTaxTD[2].Text;
                                        }
                                        Tl_Du = DueTaxTD[1].Text;
                                        if (Tl_Du.Contains("Total Due"))
                                        {
                                            TaxToal_Due = DueTaxTD[2].Text;
                                        }
                                    }
                                }

                                DueTaxInfo_Details = Taxy + "~" + Cur_Due + "~" + Tax_Penalty + "~" + Int_Rest + "~" + Other_Fees + "~" + Previous_Payments + "~" + Back_Taxes + "~" + TaxToal_Due;
                                gc.insert_date(orderNumber, Parcel_ID, 802, DueTaxInfo_Details, 1, DateTime.Now);
                            }
                            catch
                            { }

                            try
                            {
                                IWebElement CityTaxPaymentTB = driver.FindElement(By.XPath("//*[@id='EntityTable']/tbody"));
                                IList<IWebElement> CityTaxPaymentTR = CityTaxPaymentTB.FindElements(By.TagName("tr"));
                                IList<IWebElement> CityTaxPaymentTD;

                                foreach (IWebElement CityTaxPayment in CityTaxPaymentTR)
                                {
                                    CityTaxPaymentTD = CityTaxPayment.FindElements(By.TagName("td"));
                                    if (CityTaxPaymentTD.Count != 0 && !CityTaxPayment.Text.Contains("Entity") && !CityTaxPayment.Text.Contains("Totals: "))
                                    {
                                        Entity = CityTaxPaymentTD[0].Text;
                                        FMV = CityTaxPaymentTD[1].Text;
                                        Net_Assmnt = CityTaxPaymentTD[2].Text;
                                        Exmpl = CityTaxPaymentTD[3].Text;
                                        Taxble_Value = CityTaxPaymentTD[4].Text;
                                        Millage_Rate = CityTaxPaymentTD[5].Text;
                                        Grs_Tax = CityTaxPaymentTD[6].Text;
                                        Cretit = CityTaxPaymentTD[7].Text;
                                        NetTax = CityTaxPaymentTD[8].Text;

                                        CityPayment_details = Entity + "~" + FMV + "~" + Net_Assmnt + "~" + Exmpl + "~" + Taxble_Value + "~" + Millage_Rate + "~" + Grs_Tax + "~" + Cretit + "~" + NetTax;
                                        gc.insert_date(orderNumber, Parcel_ID, 803, CityPayment_details, 1, DateTime.Now);
                                    }
                                }
                            }
                            catch
                            { }

                            try
                            {
                                IWebElement CityTaxPaymentTB1 = driver.FindElement(By.XPath("//*[@id='EntityTable']/tbody"));
                                IList<IWebElement> CityTaxPaymentTR1 = CityTaxPaymentTB1.FindElements(By.TagName("tr"));
                                IList<IWebElement> CityTaxPaymentTD1;

                                foreach (IWebElement CityTaxPayment1 in CityTaxPaymentTR1)
                                {
                                    CityTaxPaymentTD1 = CityTaxPayment1.FindElements(By.TagName("td"));
                                    if (CityTaxPaymentTD1.Count != 0 && CityTaxPayment1.Text.Contains("Totals: "))
                                    {
                                        Taxble_Value1 = CityTaxPaymentTD1[0].Text;
                                        Millage_Rate1 = CityTaxPaymentTD1[1].Text;
                                        Grs_Tax1 = CityTaxPaymentTD1[2].Text;
                                        Cretit1 = CityTaxPaymentTD1[3].Text;
                                        NetTax1 = CityTaxPaymentTD1[4].Text;

                                        CityPayment_details = "" + "~" + "" + "~" + "" + "~" + "" + "~" + Taxble_Value1 + "~" + Millage_Rate1 + "~" + Grs_Tax1 + "~" + Cretit1 + "~" + NetTax1;
                                        gc.insert_date(orderNumber, Parcel_ID, 803, CityPayment_details, 1, DateTime.Now);
                                    }
                                }
                            }
                            catch
                            { }
                        }

                    }
                    catch
                    { }

                    //Tax Avthority
                    driver.Navigate().GoToUrl("http://www.fayettecountytaxcomm.com/");
                    Thread.Sleep(2000);

                    Tax_Auth1 = driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[3]/td[3]/p[19]")).Text.Replace("Suite 109 (Motor Vehicles)", "");
                    Tax_Auth2 = driver.FindElement(By.XPath("/html/body/div/table/tbody/tr[3]/td[3]/p[15]")).Text;
                    Tax_Authotiry = Tax_Auth1 + " & " + Tax_Auth2;

                    TaxAuthority_Details = Tax_Authotiry;
                    gc.CreatePdf(orderNumber, Parcel_ID, "Tax Authority Details", driver, "GA", "Fayette");
                    gc.insert_date(orderNumber, Parcel_ID, 804, TaxAuthority_Details, 1, DateTime.Now);

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "GA", "Fayette", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "GA", "Fayette");
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