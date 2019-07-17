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
    public class Webdriver_ForsythGA
    {
        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());
        public string FTP_ForsythGA(string Address, string ownername, string parcelNumber, string searchType, string orderNumber, string directParcel)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";
            string Parcelno = "", Owner = "", Property_Address = "", Legal_Desp = "", MultiOwner_details = "", MultiAddress_details = "";
            string Parcel_ID = "", Location = "", Legal = "", Property_Class = "", Nighbrhod = "", Tax_Dis = "", Zoing = "", Acres = "", Homestd = "", Exemption = "", Overall_OwnMailAdd = "", Year_Built = "";
            string property = "", yblt = "", Mailing_Address2 = "", Mailing_Address1 = "", Owner1 = "", Owner2 = "", Exemp = "", Home = "", Acrs = "", Zone = "", tx_Dis = "", Nhg = "", Pro_Cls = "", Lg_Des = "", Loc = "", Par = "";
            string Year1 = "", Year5 = "", Year6 = "", Year2 = "", Year3 = "", Year4 = "", Assemnt_Details1 = "", Assemnt_Details2 = "", Assemnt_Details3 = "", Assemnt_Details4 = "", total = "", Total_Payments = "", Convey = "", Convey_Fee = "", Paid = "", Total_Paid = "", TaxPayment_details1 = "", TaxPayment_details = "", Balnce_Due = "", Tax_Bill = "", Tax_year = "";
            string Taxy = "", Total_DUE = "", Last_PAYDATE = "", Tax_P = "", Tax_Payer = "", Map = "", Map_Code = "", Desk = "", Desk_Pro = "", Locy = "", Tax_Location = "", Bl_No = "", Tax_Billno = "", Disti = "", Tax_Dist = "", Build_Val = "", Land_Val = "", Tax_Acres = "", FrMkt_Val = "", Tax_DueDate = "", Tax_Exmpions = "", TaxInfo_Details = "";
            string Cu_Du = "", Penalt = "", In_Tx = "", Oth_Fe = "", Pre_Pay = "", Bc_Tx = "", Tl_Du = "", Cur_Due = "", Tax_Penalty = "", Int_Rest = "", Other_Fees = "", Previous_Payments = "", Back_Taxes = "", TaxToal_Due = "", DueTaxInfo_Details = "";
            string Entity = "", FMV = "", Net_Assmnt = "", Exmpl = "", Taxble_Value = "", Millage_Rate = "", Grs_Tax = "", Cretit = "", NetTax = "", CityPayment_details = "", Taxble_Value1 = "", Millage_Rate1 = "", Grs_Tax1 = "", Cretit1 = "", NetTax1 = "";
            string Tax_Auth1 = "", Tax_Auth2 = "", Tax_Authotiry = "", TaxAuthority_Details = "";
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            // driver = new PhantomJSDriver();
            //driver = new ChromeDriver();
            //rdp
            using (driver = new PhantomJSDriver())
            {
                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    if (searchType == "titleflex")
                    {
                        string titleaddress = Address;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "GA", "Forsyth");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_ForsythGA"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }
                    driver.Navigate().GoToUrl("http://qpublic9.qpublic.net/ga_search_dw.php?county=ga_forsyth");
                    Thread.Sleep(2000);
                    if (searchType == "address")
                    {

                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='appBody']/div[4]/div/div/div[3]/a[1]")).Click();
                            Thread.Sleep(2000);
                        }
                        catch { }

                        driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_txtAddress")).SendKeys(Address);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "GA", "Forsyth");
                        driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        try
                        {
                            int AddressmaxCheck = 0;
                            IWebElement MultiAddressTB = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_ctl01_gvwParcelResults']/tbody"));
                            IList<IWebElement> MultiAddressTR = MultiAddressTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiAddressTD;
                            gc.CreatePdf_WOP(orderNumber, "Multi Address search", driver, "GA", "Forsyth");

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
                                        Legal_Desp = MultiAddressTD[5].Text;

                                        MultiAddress_details = Owner + "~" + Property_Address + "~" + Legal_Desp;
                                        gc.insert_date(orderNumber, Parcelno, 1364, MultiAddress_details, 1, DateTime.Now);
                                        AddressmaxCheck++;
                                    }

                                }
                            }
                            if (AddressmaxCheck > 25)
                            {
                                HttpContext.Current.Session["multiParcel_GAForsyth_Multicount"] = "Maximum";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiparcel_GAForsyth"] = "Yes";
                            }
                            driver.Quit();
                            return "MultiParcel";
                        }
                        catch
                        { }
                    }

                    if (searchType == "parcel")
                    {

                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='appBody']/div[4]/div/div/div[3]/a[1]")).Click();
                            Thread.Sleep(2000);
                        }
                        catch { }
                        driver.FindElement(By.Id("ctlBodyPane_ctl02_ctl01_txtParcelID")).SendKeys(parcelNumber);
                        driver.FindElement(By.Id("ctlBodyPane_ctl02_ctl01_btnSearch")).Click();
                        Thread.Sleep(2000);

                    }

                    if (searchType == "ownername")
                    {
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='appBody']/div[4]/div/div/div[3]/a[1]")).Click();
                            Thread.Sleep(2000);
                        }
                        catch { }

                        driver.FindElement(By.Id("ctlBodyPane_ctl00_ctl01_txtName")).SendKeys(ownername);
                        gc.CreatePdf_WOP(orderNumber, "Owner search", driver, "GA", "Forsyth");
                        driver.FindElement(By.Id("ctlBodyPane_ctl00_ctl01_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        try
                        {
                            int AddressmaxCheck = 0;
                            IWebElement MultiAddressTB = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_ctl01_gvwParcelResults']/tbody"));
                            IList<IWebElement> MultiAddressTR = MultiAddressTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiAddressTD;
                            gc.CreatePdf_WOP(orderNumber, "Multi Address search", driver, "GA", "Forsyth");

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
                                        Legal_Desp = MultiAddressTD[5].Text;
                                        MultiAddress_details = Owner + "~" + Property_Address + "~" + Legal_Desp;
                                        gc.insert_date(orderNumber, Parcelno, 1364, MultiAddress_details, 1, DateTime.Now);
                                        AddressmaxCheck++;
                                    }

                                }
                            }
                            if (AddressmaxCheck > 25)
                            {
                                HttpContext.Current.Session["multiParcel_GAForsyth_Multicount"] = "Maximum";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiparcel_GAForsyth"] = "Yes";
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
                            HttpContext.Current.Session["Nodata_ForsythGA"] = "Yes";
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
                    {                                        //  ctlBodyPane_ctl01_ctl01_lnkOwnerName1_lnkSearch
                                                             //ctlBodyPane_ctl01_ctl01_lnkOwnerName1_lblSearch
                        try
                        {
                            Owner1 = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl01_ctl01_lnkOwnerName1_lblSearch']")).Text;
                        }
                        catch
                        {
                            Owner1 = driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_lnkOwnerName1_lnkSearch")).Text;
                        }
                        Owner2 = driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_lblOwnerName2")).Text;
                        Mailing_Address1 = driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_lblAddress1")).Text;
                        Mailing_Address2 = driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_lblCityStZip")).Text;
                        Overall_OwnMailAdd = Owner1 + " " + Owner2 + " & " + Mailing_Address1 + " " + Mailing_Address2;
                    }
                    catch
                    { }

                    if (Owner2.Trim() == "")
                    {
                        try
                        {
                            Owner1 = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl01_ctl01_lnkOwnerName1_lblSearch']")).Text;
                        }
                        catch
                        {
                            Owner1 = driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_lnkOwnerName1_lnkSearch")).Text;
                        }
                        Mailing_Address1 = driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_lblAddress1")).Text;
                        Mailing_Address2 = driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_lblCityStZip")).Text;
                        Overall_OwnMailAdd = Owner1 + " & " + Mailing_Address1 + " " + Mailing_Address2;

                    }
                    try
                    {
                        IWebElement Yeartb = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl06_mSection']/div/div/div[1]/table/tbody"));
                        IList<IWebElement> YearTR = Yeartb.FindElements(By.TagName("tr"));
                        IList<IWebElement> YearTD;

                        foreach (IWebElement Tax4 in YearTR)
                        {
                            YearTD = Tax4.FindElements(By.TagName("td"));
                            if (YearTD.Count != 0 && Tax4.Text.Contains("Year Built"))
                            {

                                Year_Built = YearTD[1].Text;

                            }
                        }
                    }
                    catch
                    { }

                    property = Location + "~" + Legal + "~" + Property_Class + "~" + Nighbrhod + "~" + Tax_Dis + "~" + Zoing + "~" + Acres + "~" + Homestd + "~" + Exemption + "~" + Overall_OwnMailAdd + "~" + Year_Built;
                    gc.insert_date(orderNumber, Parcel_ID, 1243, property, 1, DateTime.Now);

                    //Assessment Details
                    IWebElement AssmThTb = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl02_ctl01_grdValuation']/thead"));
                    IList<IWebElement> AssmThTr = AssmThTb.FindElements(By.TagName("tr"));
                    IList<IWebElement> AssmTh;

                    foreach (IWebElement Assm in AssmThTr)
                    {
                        AssmTh = Assm.FindElements(By.TagName("th"));
                        if (AssmTh.Count != 0)
                        {
                            try
                            {
                                Year1 = AssmTh[0].Text;
                                Year2 = AssmTh[1].Text;
                                Year3 = AssmTh[2].Text;
                                Year4 = AssmTh[3].Text;
                                Year5 = AssmTh[4].Text;
                                Year6 = AssmTh[5].Text;
                            }
                            catch { }
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

                            }
                            else if (j == 1)
                            {
                                Class.Add(AssmTd[2].Text);
                                Class.Add(AssmTd[3].Text);

                            }
                            else if (j == 2)
                            {
                                Land_Value.Add(AssmTd[2].Text);
                                Land_Value.Add(AssmTd[3].Text);

                            }
                            else if (j == 3)
                            {
                                Build_Value.Add(AssmTd[2].Text);
                                Build_Value.Add(AssmTd[3].Text);

                            }
                            else if (j == 4)
                            {
                                Total_Value.Add(AssmTd[2].Text);
                                Total_Value.Add(AssmTd[3].Text);

                            }
                            else if (j == 6)
                            {
                                Assed_Value.Add(AssmTd[2].Text);
                                Assed_Value.Add(AssmTd[3].Text);

                            }

                            j++;
                        }
                    }

                    Assemnt_Details1 = Year1 + "~" + LUC[0] + "~" + Class[0] + "~" + Land_Value[0] + "~" + Build_Value[0] + "~" + Total_Value[0] + "~" + Assed_Value[0];
                    Assemnt_Details2 = Year2 + "~" + LUC[1] + "~" + Class[1] + "~" + Land_Value[1] + "~" + Build_Value[1] + "~" + Total_Value[1] + "~" + Assed_Value[1];
                    //Assemnt_Details3 = Year3 + "~" + LUC[2] + "~" + Class[2] + "~" + Land_Value[2] + "~" + Build_Value[2] + "~" + Total_Value[2] + "~" + Assed_Value[2];
                    //Assemnt_Details4 = Year4 + "~" + LUC[3] + "~" + Class[3] + "~" + Land_Value[3] + "~" + Build_Value[3] + "~" + Total_Value[3] + "~" + Assed_Value[3];
                    //string Assemnt_Details5 = Year5 + "~" + LUC[4] + "~" + Class[4] + "~" + Land_Value[4] + "~" + Build_Value[4] + "~" + Total_Value[4] + "~" + Assed_Value[4];
                    //string Assemnt_Details6 = Year6 + "~" + LUC[5] + "~" + Class[5] + "~" + Land_Value[5] + "~" + Build_Value[5] + "~" + Total_Value[5] + "~" + Assed_Value[5];
                    gc.insert_date(orderNumber, Parcel_ID, 1244, Assemnt_Details1, 1, DateTime.Now);
                    gc.insert_date(orderNumber, Parcel_ID, 1244, Assemnt_Details2, 1, DateTime.Now);
                    //gc.insert_date(orderNumber, Parcel_ID, 1244, Assemnt_Details3, 1, DateTime.Now);
                    //gc.insert_date(orderNumber, Parcel_ID, 1244, Assemnt_Details4, 1, DateTime.Now);
                    //gc.insert_date(orderNumber, Parcel_ID, 1244, Assemnt_Details5, 1, DateTime.Now);
                    //gc.insert_date(orderNumber, Parcel_ID, 1244, Assemnt_Details6, 1, DateTime.Now);
                    gc.CreatePdf(orderNumber, Parcel_ID, "Property Details", driver, "GA", "Forsyth");
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");
                    //tax site
                    driver.Navigate().GoToUrl("https://www1.forsythco.com/PayPropertyTax/TaxBillSearch.aspx");
                    string p_no = Parcel_ID.Trim();
                    string parcel = p_no.Split(' ')[0];
                    string parcel_num = p_no.Split(' ')[1];

                    driver.FindElement(By.Id("Body_MapParcelID1")).SendKeys(parcel);
                    driver.FindElement(By.Id("Body_MapParcelID2")).SendKeys(parcel_num);
                    gc.CreatePdf(orderNumber, Parcel_ID, "Tax Parcel", driver, "GA", "Forsyth");
                    driver.FindElement(By.Id("Body_MapParcelIDGo")).Click();
                    Thread.Sleep(2000);
                    gc.CreatePdf(orderNumber, Parcel_ID, "Tax Parcel click", driver, "GA", "Forsyth");
                    IWebElement Percelhref = driver.FindElement(By.XPath("//*[@id='Body_GridView1']/tbody/tr[2]/td[1]/a"));
                    string Hrefclick = Percelhref.GetAttribute("href");
                    driver.Navigate().GoToUrl(Hrefclick);
                    Thread.Sleep(1000);
                    List<string> ParcelSearch = new List<string>();
                    try
                    {
                        gc.CreatePdf(orderNumber, Parcel_ID, "Tax Information click", driver, "GA", "Forsyth");
                        IWebElement ParcelTB = driver.FindElement(By.XPath("//*[@id='Body_GridView1']/tbody"));
                        IList<IWebElement> ParcelTR = ParcelTB.FindElements(By.TagName("tr"));
                        ParcelTR.Reverse();
                        int rows_count = ParcelTR.Count;

                        for (int row = 0; row < rows_count; row++)
                        {
                            if (row == rows_count - 3 || row == rows_count - 1 || row == rows_count - 2)
                            {
                                IList<IWebElement> Columns_row = ParcelTR[row].FindElements(By.TagName("td"));
                                int columns_count = Columns_row.Count;
                                if (columns_count != 0)
                                {
                                    IWebElement ParcelBill_link = Columns_row[1].FindElement(By.TagName("a"));
                                    string Parcelurl = ParcelBill_link.GetAttribute("href");
                                    ParcelSearch.Add(Parcelurl);
                                }
                            }
                        }
                    }
                    catch { }
                    string bodyowner = driver.FindElement(By.Id("Body_Owners")).Text;
                    IWebElement taxbillnumbertable = driver.FindElement(By.XPath("//*[@id='Body_GridView1']/tbody"));
                    IList<IWebElement> taxbillnumberrow = taxbillnumbertable.FindElements(By.TagName("tr"));
                    IList<IWebElement> taxbillnumberid;
                    foreach (IWebElement Currenttax1 in taxbillnumberrow)
                    {
                        taxbillnumberid = Currenttax1.FindElements(By.TagName("td"));
                        if (taxbillnumberid.Count == 4 && taxbillnumberid[1].Text.Trim() != "")
                        {
                            string taxbillnumberresult = bodyowner + "~" + taxbillnumberid[0].Text + "~" + taxbillnumberid[1].Text + "~" + taxbillnumberid[2].Text;
                            gc.insert_date(orderNumber, Parcel_ID, 1250, taxbillnumberresult, 1, DateTime.Now);
                        }

                    }

                    gc.CreatePdf(orderNumber, Parcel_ID, "Tax Click after", driver, "GA", "Forsyth");
                    string TotalPaymenthead = driver.FindElement(By.Id("Body_TotalPaymentsPendingLabel")).Text;
                    string Toatalpaymentpending = driver.FindElement(By.Id("Body_TotalPaymentsPending")).Text;
                    gc.insert_date(orderNumber, Parcel_ID, 1250, "~" + "~" + TotalPaymenthead + "~" + Toatalpaymentpending, 1, DateTime.Now);
                    string ConvenFeehead = driver.FindElement(By.Id("Body_Label1")).Text;
                    string convenfeebody = driver.FindElement(By.Id("Body_ConvenienceFee")).Text;
                    gc.insert_date(orderNumber, Parcel_ID, 1250, "~" + "~" + ConvenFeehead + "~" + convenfeebody, 1, DateTime.Now);
                    string Totalbodyhead = driver.FindElement(By.Id("Body_Label2")).Text;
                    string Totalamtbody = driver.FindElement(By.Id("Body_TotalAmountToBePaid")).Text;
                    gc.insert_date(orderNumber, Parcel_ID, 1250, "~" + "~" + Totalbodyhead + "~" + Totalamtbody, 1, DateTime.Now);
                    int i = 0;
                    foreach (string taxlink in ParcelSearch)
                    {
                        driver.Navigate().GoToUrl(taxlink);
                        string Taxyear1 = driver.FindElement(By.Id("PropertyTaxLabel")).Text;
                        string Taxyear = GlobalClass.Before(Taxyear1, "Property Tax").Trim();
                        string Taxpayer = driver.FindElement(By.Id("TaxPayerText")).Text;
                        string Discription = driver.FindElement(By.Id("DescriptionText")).Text;
                        string LocationTax = driver.FindElement(By.Id("LocationText")).Text;
                        string Billnumber = driver.FindElement(By.Id("BillNoText")).Text;
                        string district = driver.FindElement(By.Id("DistrictText")).Text;
                        string Builiding = driver.FindElement(By.Id("BuildingValueText")).Text;
                        string Lanvalue = driver.FindElement(By.Id("LandValueText")).Text;
                        string Arces = driver.FindElement(By.Id("AcresText")).Text;
                        string FairMarket = driver.FindElement(By.Id("FairMarketValueText")).Text;
                        string Duedate = driver.FindElement(By.Id("DueDateText")).Text;
                        string Exemptions = driver.FindElement(By.Id("ExemptionsText")).Text;
                        string Topduedaate = driver.FindElement(By.Id("DueDateTopText")).Text;
                        string Totaldue = driver.FindElement(By.Id("TotalDueTopText")).Text;
                        string Lastpayerid = driver.FindElement(By.Id("LastPaymentTopText")).Text;
                        string Lastpayername = GlobalClass.After(Lastpayerid, "Last payment made on:");
                        Tax_Auth1 = driver.FindElement(By.Id("title")).Text;
                        Tax_Auth2 = driver.FindElement(By.Id("streetAddress")).Text;
                        Tax_Authotiry = driver.FindElement(By.Id("cityStateZip")).Text;
                        string Tax_Auth3 = driver.FindElement(By.XPath("//*[@id='commissionerData']/tbody/tr[8]/td")).Text;
                        TaxAuthority_Details = Tax_Auth1 + " " + Tax_Auth2 + " " + Tax_Authotiry + " " + Tax_Auth3;
                        string TaxDetailresult = Taxyear + "~" + Taxpayer + "~" + Discription + "~" + LocationTax + "~" + Billnumber + "~" + district + "~" + Builiding + "~" + Lanvalue + "~" + Arces + "~" + FairMarket + "~" + Duedate + "~" + Exemptions + "~" + Totaldue + "~" + Lastpayername + "~" + TaxAuthority_Details;
                        gc.insert_date(orderNumber, Parcel_ID, 1247, TaxDetailresult, 1, DateTime.Now);
                        IWebElement CurrenttaxTable = driver.FindElement(By.XPath("//*[@id='EntityTable']/tbody"));
                        IList<IWebElement> Currenttaxrow = CurrenttaxTable.FindElements(By.TagName("tr"));
                        IList<IWebElement> Currenttaxid;
                        foreach (IWebElement Currenttax in Currenttaxrow)
                        {
                            Currenttaxid = Currenttax.FindElements(By.TagName("td"));
                            if (Currenttaxid.Count == 9 && !Currenttax.Text.Contains("Entity"))
                            {
                                string Currentresult = Taxyear + "~" + Currenttaxid[0].Text + "~" + Currenttaxid[1].Text + "~" + Currenttaxid[2].Text + "~" + Currenttaxid[3].Text + "~" + Currenttaxid[4].Text + "~" + Currenttaxid[5].Text + "~" + Currenttaxid[6].Text + "~" + Currenttaxid[7].Text + "~" + Currenttaxid[8].Text;
                                gc.insert_date(orderNumber, Parcel_ID, 1248, Currentresult, 1, DateTime.Now);
                            }
                            if (Currenttaxid.Count == 5 && !Currenttax.Text.Contains("Entity"))
                            {
                                string Currentresult = Taxyear + "~" + " " + "~" + "~" + "~" + "~" + Currenttaxid[0].Text + "~" + Currenttaxid[1].Text + "~" + Currenttaxid[2].Text + "~" + Currenttaxid[3].Text + "~" + Currenttaxid[4].Text;
                                gc.insert_date(orderNumber, Parcel_ID, 1248, Currentresult, 1, DateTime.Now);
                            }
                        }
                        gc.CreatePdf(orderNumber, Parcel_ID, "Property Tax statment" + i, driver, "GA", "Forsyth");
                        IWebElement CurrenttaxTable1 = driver.FindElement(By.XPath("//*[@id='CurrentDueTable']/tbody"));
                        IList<IWebElement> Currenttaxrow1 = CurrenttaxTable1.FindElements(By.TagName("tr"));
                        IList<IWebElement> Currenttaxid1;
                        foreach (IWebElement Currenttax1 in Currenttaxrow1)
                        {
                            Currenttaxid1 = Currenttax1.FindElements(By.TagName("td"));
                            if (Currenttaxid1.Count == 3 && Currenttaxid1[1].Text.Trim() != "")
                            {
                                string Currentresult = Taxyear + "~" + Currenttaxid1[1].Text + "~" + Currenttaxid1[2].Text;
                                gc.insert_date(orderNumber, Parcel_ID, 1361, Currentresult, 1, DateTime.Now);
                            }

                        }
                        i++;
                    }
                    driver.Quit();
                    gc.mergpdf(orderNumber, "GA", "Forsyth");
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
    }
}