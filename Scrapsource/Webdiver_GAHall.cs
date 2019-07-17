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
    public class Webdiver_GAHall
    {
        string Assemnt_Details1 = "", Assemnt_Details2 = "", Year1 = "", Year2 = "", yblt = "", Parcelno = "", Owner = "", Property_Address = "", MultiOwner_details = "", MultiAddress_details = "";
        string ParcelID = "", Loc_Addrs = "", Leg_Desp = "", Class = "", Tax_Dist = "", Milagerate = "", Acres = "", Neighberwood = "", Homstd_Exmp = "", Owner1 = "", Year_Built = "", property = "", Zoning = "", Mailing_Address = "";
        string Dist = "", Ac = "", AsVal = "", ProVal = "", AppVal = "", ApprVal = "", Dis = "", Acres1 = "", Assed_Val = "", Appr_Value = "", Description = "", Pro_Addr = "", Overall_OwnMailAdd = "";
        string Sta = "", Amtpaid = "", Latpay = "", Status = "", Last_Pay = "", Amt_Paid = "", Rcdtyp = "", tYear = "", Rep = "", Dudt = "", Duet = "", Record_Type = "", Taxy = "", Recep = "", Due_Date = "", Accnt_Num;
        string Base_Taxes = "", Interst = "", Other_Fee = "", penalty = "", Back_Tax = "", Total_Due = "", tldue = "", Base = "", Pan = "", Inst = "", Otherfee = "", Backtax = "";
        string Entity = "", AdjtFMV = "", NetAssnt = "", Exemptions = "", Taxbleval = "", Milagete = "", grosstax = "", Credits = "", Net_Tax = "", Breakdown_details = "";
        string Entity1 = "", AdjtFMV1 = "", NetAssnt1 = "", Exemptions1 = "", Taxbleval1 = "", Milagete1 = "", grosstax1 = "", Credits1 = "", Net_Tax1 = "", Breakdown_details1 = "", Del_Taxyear = "";
        string Owners = "", year = "", Bill = "", Desc = "", Type = "", Paid = "", Paid_date = "", Payment_details = "";
        string name = "", Taxyear = "", bill_no = "", amount = "", Del_details = "", bill_no1 = "", amount1 = "", Del_details1 = "", Taxing = "", Phone = "", Fax = "", Taxing_Authority = "", Taxauthority_Details = "", Tax_Deatils = "", Owner_Mailning = "", Mailing_Address1 = "", Mailing_Address2 = "", Tax_Sale = "", TaxSale_Comments = "";
        string City_Year = "", Bill_Yash = "", Deed_Name = "", Pro_Add = "", Map_Code = "", D_Date = "", Prior_Payment = "", Amt_Due = "", City_Paid = "", CityPayment_details = "";
        string Bill_No = "", due_Date = "", Cuur_Date = "", Pri_Pay = "", CityBack_Tax = "", tol_Due = "", OwnerMailing = "", City_Authority = "", Tax_Payer = "", CityMap_Code = "", City_desp = "", Location = "", CityTax_Deatils = "", payer = "", Map = "", Desk = "", Loc = "";
        string CityDel_details = "", CityBiiling_Date = "", CityDue_Date = "", Fair_mrkt = "", CityAcres = "", CityLand_Value = "", CityBuilding_Value = "";
        string Cu_Date = "", City_Penalty = "", City_Interest = "", City_Other = "", Pre_Payments = "", City_back = "", Citytl_Due = "", Citypaid = "", ps_dt = "", tl_Du = "", bck = "", Pre_Pay = "", Otr = "", inst = "", pent = "", Cuu = "";
        string CityEntity_details = "", CNet_Tax = "", Credit = "", Grs = "", Mil_rt = "", Txn_Val = "", Exemp = "", Net_Asse = "", Adj_FMV = "", City_Entity = "", Owner2 = "";
        int i = 0, j = 0;

        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_GAHall(string address, string assessment_id, string parcelNumber, string searchType, string orderNumber, string directParcel, string ownername)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

            string StartTime = "", AssessmentTime = "", TaxTime = "", CitytaxTime = "", LastEndTime = "";

            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            using (driver = new PhantomJSDriver())
            {
                //driver = new PhantomJSDriver();
                //driver = new ChromeDriver();

                try
                {
                    StartTime = DateTime.Now.ToString("HH:mm:ss");

                    if (searchType == "titleflex")
                    {
                        string titleaddress = address;
                        gc.TitleFlexSearch(orderNumber, "", "", titleaddress, "GA", "Hall");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_GAHall"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("https://qpublic.schneidercorp.com/Application.aspx?App=HallCountyGA&Layer=Parcels&PageType=Search");
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
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "GA", "Hall");
                        driver.FindElement(By.Id("ctlBodyPane_ctl01_ctl01_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        try
                        {
                            IWebElement MultiAddressTB = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_ctl01_gvwParcelResults']/tbody"));
                            IList<IWebElement> MultiAddressTR = MultiAddressTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiAddressTD;
                            gc.CreatePdf_WOP(orderNumber, "Multi Address search", driver, "GA", "Hall");
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
                                        gc.insert_date(orderNumber, Parcelno, 709, MultiAddress_details, 1, DateTime.Now);
                                    }
                                    AddressmaxCheck++;
                                }
                            }
                            if (MultiAddressTR.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_GAHall_Multicount"] = "Maximum";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiparcel_GAHall"] = "Yes";
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
                                HttpContext.Current.Session["Nodata_GAHall"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("https://qpublic.schneidercorp.com/Application.aspx?App=HallCountyGA&Layer=Parcels&PageType=Search");
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
                        gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearch", driver, "GA", "Hall");
                        driver.FindElement(By.Id("ctlBodyPane_ctl02_ctl01_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                    }

                    if (searchType == "ownername")
                    {
                        driver.Navigate().GoToUrl("https://qpublic.schneidercorp.com/Application.aspx?App=HallCountyGA&Layer=Parcels&PageType=Search");
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
                        gc.CreatePdf_WOP(orderNumber, "Owner search", driver, "GA", "Hall");
                        driver.FindElement(By.Id("ctlBodyPane_ctl00_ctl01_btnSearch")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        try
                        {
                            IWebElement MultiOwnerTB = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl00_ctl01_gvwParcelResults']/tbody"));
                            IList<IWebElement> MultiOwnerTR = MultiOwnerTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> MultiOwnerTD;
                            gc.CreatePdf_WOP(orderNumber, "Multi Owner search", driver, "GA", "Hall");
                            int AddressmaxCheck = 0;
                            foreach (IWebElement MultiOwner in MultiOwnerTR)
                            {
                                if (AddressmaxCheck <= 25)
                                {
                                    MultiOwnerTD = MultiOwner.FindElements(By.TagName("td"));
                                    if (MultiOwnerTD.Count != 0)
                                    {
                                        Parcelno = MultiOwnerTD[1].Text;
                                        Owner = MultiOwnerTD[3].Text;
                                        Property_Address = MultiOwnerTD[4].Text;

                                        MultiOwner_details = Owner + "~" + Property_Address;
                                        gc.insert_date(orderNumber, Parcelno, 709, MultiOwner_details, 1, DateTime.Now);
                                    }
                                    AddressmaxCheck++;
                                }
                            }
                            if (MultiOwnerTR.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_GAHall_Multicount"] = "Maximum";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiparcel_GAHall"] = "Yes";
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
                                HttpContext.Current.Session["Nodata_GAHall"] = "Yes";
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
                        Owner1 = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl01_ctl01_lnkOwnerName_lnkSearch']")).Text;
                        Owner2 = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl01_ctl01_lblAddress']")).Text;
                        Mailing_Address = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl01_ctl01_lblCityStateZip']")).Text;
                        Overall_OwnMailAdd = Owner1 + "&" + Owner2 + " " + Mailing_Address;
                    }
                    catch
                    { }

                    try
                    {
                        Owner1 = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl01_ctl01_lnkOwnerName_lblSearch']")).Text;
                        Mailing_Address1 = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl01_ctl01_lblAddress']")).Text;
                        Mailing_Address2 = driver.FindElement(By.XPath("//*[@id='ctlBodyPane_ctl01_ctl01_lblCityStateZip']")).Text;
                        Overall_OwnMailAdd = Owner1 + "&" + Mailing_Address1 + " " + Mailing_Address2;
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

                    property = Loc_Addrs + "~" + Leg_Desp + "~" + Class + "~" + Zoning + "~" + Tax_Dist + "~" + Milagerate + "~" + Acres + "~" + Neighberwood + "~" + Homstd_Exmp + "~" + Overall_OwnMailAdd + "~" + Year_Built;
                    gc.CreatePdf(orderNumber, ParcelID, "Property Details", driver, "GA", "Hall");
                    gc.insert_date(orderNumber, ParcelID, 710, property, 1, DateTime.Now);

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

                    Assemnt_Details1 = Year1 + "~" + Previous_Value[0] + "~" + Land_Value[0] + "~" + Improvement_Value[0] + "~" + Accessory_Value[0] + "~" + Current_Value[0];
                    Assemnt_Details2 = Year2 + "~" + Previous_Value[1] + "~" + Land_Value[1] + "~" + Improvement_Value[1] + "~" + Accessory_Value[1] + "~" + Current_Value[1];
                    gc.insert_date(orderNumber, ParcelID, 711, Assemnt_Details1, 1, DateTime.Now);
                    gc.insert_date(orderNumber, ParcelID, 711, Assemnt_Details2, 1, DateTime.Now);
                    AssessmentTime = DateTime.Now.ToString("HH:mm:ss");

                    //Tax Details
                    driver.Navigate().GoToUrl("https://hallcountytax.org/taxes.html#/WildfireSearch");
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
                    Thread.Sleep(5000);

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
                                Bill = TaxPaymentTD[2].Text;
                                Desc = TaxPaymentTD[3].Text;
                                Type = TaxPaymentTD[4].Text;
                                Paid = TaxPaymentTD[5].Text;
                                Paid_date = TaxPaymentTD[6].Text;

                                Payment_details = Owners + "~" + year + "~" + Bill + "~" + Desc + "~" + Type + "~" + Paid + "~" + Paid_date;
                                gc.CreatePdf(orderNumber, ParcelID, "Tax Bill Details", driver, "GA", "Hall");
                                gc.insert_date(orderNumber, ParcelID, 713, Payment_details, 1, DateTime.Now);
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
                                driver.FindElement(By.XPath("//*[@id='avalon']/div/div[4]/div[2]/table/tbody/tr[" + p + "]/td[9]/button")).Click();
                            }
                            catch { }
                            Thread.Sleep(6000);

                            try
                            {
                                Tax_Sale = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[1]/div[3]/div[3]")).Text;
                            }
                            catch
                            { }

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
                                        gc.CreatePdf(orderNumber, ParcelID, "Deliquent Tax Details" + Taxy, driver, "GA", "Hall");
                                        gc.insert_date(orderNumber, ParcelID, 717, Del_details, 1, DateTime.Now);
                                    }
                                }

                            }
                            catch
                            { }
                            try
                            {
                                if (p != 2)
                                {
                                    IWebElement DeliquentfootTB = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/table/tfoot"));
                                    IList<IWebElement> DeliquentfootTR = DeliquentfootTB.FindElements(By.TagName("tr"));
                                    IList<IWebElement> DeliquentfootTD;

                                    foreach (IWebElement Deliquentfoot in DeliquentfootTR)
                                    {
                                        DeliquentfootTD = Deliquentfoot.FindElements(By.TagName("th"));
                                        if (DeliquentfootTD.Count != 0)
                                        {
                                            bill_no1 = DeliquentfootTD[0].Text;
                                            amount1 = DeliquentfootTD[2].Text;

                                            Del_details1 = "" + "~" + "" + "~" + bill_no1 + "~" + amount1;
                                            gc.insert_date(orderNumber, ParcelID, 717, Del_details1, 1, DateTime.Now);
                                        }
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

                            Owner_Mailning = driver.FindElement(By.XPath("//*[@id='avalon']/div/div/div/div[1]/div[1]/div[1]")).Text.Replace("\r\n", "");
                            Owner_Mailning = WebDriverTest.After(Owner_Mailning, "Owner Information");

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

                            if (Tax_Sale == "Tax Sale")
                            {
                                TaxSale_Comments = "You must call the Collector's Office.";
                            }

                            gc.CreatePdf(orderNumber, ParcelID, "Tax Details" + Taxy, driver, "GA", "Hall");

                            Tax_Deatils = Taxy + "~" + Owner_Mailning + "~" + Dis + "~" + Acres1 + "~" + Description + "~" + Pro_Addr + "~" + Assed_Val + "~" + Appr_Value + "~" + Status + "~" + Last_Pay + "~" + Amt_Paid + "~" + Record_Type + "~" + Recep + "~" + Accnt_Num + "~" + Due_Date + "~" + Base_Taxes + "~" + penalty + "~" + Interst + "~" + Other_Fee + "~" + Del_Taxyear + "~" + Back_Tax + "~" + Total_Due + "~" + TaxSale_Comments;
                            gc.insert_date(orderNumber, ParcelID, 716, Tax_Deatils, 1, DateTime.Now);

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
                                        gc.insert_date(orderNumber, ParcelID, 715, Breakdown_details, 1, DateTime.Now);
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
                                gc.insert_date(orderNumber, ParcelID, 715, Breakdown_details1, 1, DateTime.Now);
                            }
                            catch
                            { }

                            //Tax Bill
                            try
                            {
                                driver.FindElement(By.XPath("//*[@id='avalon']/div/div/ul/li[2]/a")).Click();
                                Thread.Sleep(5000);

                                gc.CreatePdf(orderNumber, ParcelID, "Tax Bill Details" + Taxy, driver, "GA", "Hall");
                            }
                            catch
                            { }
                            try
                            {
                                driver.FindElement(By.XPath("//*[@id='avalon']/div/div/ul/li[3]/a")).Click();
                                Thread.Sleep(4000);

                                gc.CreatePdf(orderNumber, ParcelID, "Tax Receipt Details" + Taxy, driver, "GA", "Hall");
                            }
                            catch
                            { }

                            driver.Navigate().Back();
                            Thread.Sleep(3000);
                        }
                    }

                    //Tax Authority
                    driver.Navigate().GoToUrl("https://hallcountytax.org/#/contact");
                    Thread.Sleep(2000);

                    driver.FindElement(By.XPath("//*[@id='nav']/li[5]/a")).Click();
                    Thread.Sleep(2000);

                    try
                    {
                        Taxing = driver.FindElement(By.XPath("//*[@id='editor331']/table/tbody/tr[1]/td[3]/p")).Text.Replace("Mailing Address", " ");
                        Phone = "Office: 770-531-6950";
                        Fax = "Fax: 770-531-7106";
                        gc.CreatePdf(orderNumber, ParcelID, "Tax Authority Details", driver, "GA", "Hall");
                        Taxing_Authority = Taxing + " " + Phone + " " + Fax;

                        Taxauthority_Details = Taxing_Authority;
                        gc.insert_date(orderNumber, ParcelID, 718, Taxauthority_Details, 1, DateTime.Now);
                    }
                    catch
                    { }
                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    //city of Gainesville 
                    driver.Navigate().GoToUrl("https://gainesvillega.governmentwindow.com/tax.html");
                    Thread.Sleep(2000);

                    driver.FindElement(By.XPath("//*[@id='taxesSearchForm']/div[2]/div[2]/div/div/input[1]")).SendKeys(ParcelID);
                    gc.CreatePdf(orderNumber, ParcelID, "City Parcel Search", driver, "GA", "Hall");
                    driver.FindElement(By.XPath("//*[@id='taxesSearchForm']/div[2]/div[2]/div/div/input[2]")).SendKeys(Keys.Enter);
                    Thread.Sleep(4000);

                    try
                    {
                        //City Tax Payment History Details
                        IWebElement CityPaymentTB = driver.FindElement(By.XPath("//*[@id='tbl_tax_results']/tbody"));
                        IList<IWebElement> CityPaymentTR = CityPaymentTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> CityPaymentTD;

                        foreach (IWebElement CityPayment in CityPaymentTR)
                        {
                            CityPaymentTD = CityPayment.FindElements(By.TagName("td"));
                            if (CityPaymentTD.Count != 0)
                            {
                                City_Year = CityPaymentTD[0].Text;
                                Bill_Yash = CityPaymentTD[1].Text;
                                Deed_Name = CityPaymentTD[2].Text;
                                Pro_Add = CityPaymentTD[3].Text;
                                Map_Code = CityPaymentTD[4].Text;
                                D_Date = CityPaymentTD[5].Text;
                                Prior_Payment = CityPaymentTD[6].Text;
                                Amt_Due = CityPaymentTD[7].Text;
                                City_Paid = CityPaymentTD[8].Text;
                                City_Paid = WebDriverTest.After(City_Paid, "Paid ");

                                CityPayment_details = City_Year + "~" + Bill_Yash + "~" + Deed_Name + "~" + Pro_Add + "~" + Map_Code + "~" + D_Date + "~" + Prior_Payment + "~" + Amt_Due + "~" + City_Paid;
                                gc.CreatePdf(orderNumber, ParcelID, "City Multi Details", driver, "GA", "Hall");
                                gc.insert_date(orderNumber, ParcelID, 731, CityPayment_details, 1, DateTime.Now);
                            }
                        }

                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='tbl_tax_results']/tbody/tr/td[2]/a")).Click();
                            Thread.Sleep(2000);
                        }
                        catch
                        { }

                        //City Tax Info Details
                        IWebElement CityAsseTB = driver.FindElement(By.XPath("//*[@id='tbl_tax_bill_total']/tbody"));
                        IList<IWebElement> CityAsseTR = CityAsseTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> CityAsseTD;

                        foreach (IWebElement CityAsse in CityAsseTR)
                        {
                            CityAsseTD = CityAsse.FindElements(By.TagName("td"));
                            if (CityAsseTD.Count != 0)
                            {
                                Bill_No = CityAsseTD[0].Text;
                                due_Date = CityAsseTD[1].Text;
                                Cuur_Date = CityAsseTD[2].Text;
                                Pri_Pay = CityAsseTD[3].Text;
                                CityBack_Tax = CityAsseTD[4].Text;
                                tol_Due = CityAsseTD[5].Text;
                            }
                        }

                        try
                        {
                            OwnerMailing = driver.FindElement(By.XPath("//*[@id='tax_pay_bill']/div/div[2]/div[1]/div/div")).Text;
                            City_Authority = driver.FindElement(By.XPath("//*[@id='tax_pay_bill']/div/div[3]/div[1]/div")).Text;
                        }
                        catch
                        { }

                        IWebElement CityTaxTB = driver.FindElement(By.XPath("//*[@id='tax_pay_bill']/div/div[3]/div[3]/table/tbody"));
                        IList<IWebElement> CityTaxTR = CityTaxTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> CityTaxTD;

                        foreach (IWebElement CityTax in CityTaxTR)
                        {
                            CityTaxTD = CityTax.FindElements(By.TagName("td"));
                            if (CityTaxTD.Count != 0)
                            {
                                payer = CityTaxTD[0].Text;
                                if (payer.Contains("Tax Payer:"))
                                {
                                    Tax_Payer = CityTaxTD[1].Text;
                                }
                                Map = CityTaxTD[0].Text;
                                if (Map.Contains("Map Code:"))
                                {
                                    CityMap_Code = CityTaxTD[1].Text;
                                }
                                Desk = CityTaxTD[0].Text;
                                if (Desk.Contains("Description:"))
                                {
                                    City_desp = CityTaxTD[1].Text;
                                }
                                Loc = CityTaxTD[0].Text;
                                if (Loc.Contains("Location:"))
                                {
                                    Location = CityTaxTD[1].Text;
                                }
                            }
                        }

                        CityTax_Deatils = Tax_Payer + "~" + CityMap_Code + "~" + City_desp + "~" + Location + "~" + OwnerMailing + "~" + Bill_No + "~" + due_Date + "~" + Cuur_Date + "~" + Pri_Pay + "~" + CityBack_Tax + "~" + tol_Due + "~" + City_Authority;
                        gc.CreatePdf(orderNumber, ParcelID, "City Tax Details", driver, "GA", "Hall");
                        gc.insert_date(orderNumber, ParcelID, 733, CityTax_Deatils, 1, DateTime.Now);

                        //City Assement Details
                        IWebElement CityDeliquentTB = driver.FindElement(By.XPath("//*[@id='tbl_tax_bill_item']/tbody"));
                        IList<IWebElement> CityDeliquentTR = CityDeliquentTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> CityDeliquentTD;

                        foreach (IWebElement CityDeliquent in CityDeliquentTR)
                        {
                            CityDeliquentTD = CityDeliquent.FindElements(By.TagName("td"));
                            if (CityDeliquentTD.Count != 0)
                            {
                                CityBuilding_Value = CityDeliquentTD[0].Text;
                                CityLand_Value = CityDeliquentTD[1].Text;
                                CityAcres = CityDeliquentTD[2].Text;
                                Fair_mrkt = CityDeliquentTD[3].Text;
                                CityDue_Date = CityDeliquentTD[4].Text;
                                CityBiiling_Date = CityDeliquentTD[5].Text;
                            }
                        }

                        IWebElement CityAsserTB = driver.FindElement(By.XPath("//*[@id='tax_pay_bill']/div/div[5]/div[2]/table/tbody"));
                        IList<IWebElement> CityAsserTR = CityAsserTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> CityAsserTD;
                        foreach (IWebElement CityAsser in CityAsserTR)
                        {
                            CityAsserTD = CityAsser.FindElements(By.TagName("td"));
                            if (CityAsserTD.Count != 0)
                            {
                                Cuu = CityAsserTD[0].Text;
                                if (Cuu.Contains("Current Due"))
                                {
                                    Cu_Date = CityAsserTD[1].Text;
                                }
                                pent = CityAsserTD[0].Text;
                                if (pent.Contains("Penalty"))
                                {
                                    City_Penalty = CityAsserTD[1].Text;
                                }
                                inst = CityAsserTD[0].Text;
                                if (inst.Contains("Interest"))
                                {
                                    City_Interest = CityAsserTD[1].Text;
                                }
                                Otr = CityAsserTD[0].Text;
                                if (Otr.Contains("Other Fees"))
                                {
                                    City_Other = CityAsserTD[1].Text;
                                }
                                Pre_Pay = CityAsserTD[0].Text;
                                if (Pre_Pay.Contains("Previous Payments"))
                                {
                                    Pre_Payments = CityAsserTD[1].Text;
                                }
                                bck = CityAsserTD[0].Text;
                                if (bck.Contains("Back Taxes"))
                                {
                                    City_back = CityAsserTD[1].Text;
                                }
                                tl_Du = CityAsserTD[0].Text;
                                if (tl_Du.Contains("Total Due"))
                                {
                                    Citytl_Due = CityAsserTD[1].Text;
                                }
                                ps_dt = CityAsserTD[0].Text;
                                if (ps_dt.Contains("Paid Date"))
                                {
                                    Citypaid = CityAsserTD[1].Text;
                                }
                            }
                        }

                        CityDel_details = CityBuilding_Value + "~" + CityLand_Value + "~" + CityAcres + "~" + Fair_mrkt + "~" + CityDue_Date + "~" + CityBiiling_Date + "~" + Cu_Date + "~" + City_Penalty + "~" + City_Interest + "~" + City_Other + "~" + Pre_Payments + "~" + City_back + "~" + Citytl_Due + "~" + Citypaid;
                        gc.insert_date(orderNumber, ParcelID, 738, CityDel_details, 1, DateTime.Now);

                        //City Entity Details
                        IWebElement CityEntityTB = driver.FindElement(By.XPath("//*[@id='tbl_tax_positions']/tbody"));
                        IList<IWebElement> CityEntityTR = CityEntityTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> CityEntityTD;

                        foreach (IWebElement CityEntity in CityEntityTR)
                        {
                            CityEntityTD = CityEntity.FindElements(By.TagName("td"));
                            if (CityEntityTD.Count != 0 && !CityEntity.Text.Contains("TOTALS"))
                            {
                                City_Entity = CityEntityTD[0].Text;
                                Adj_FMV = CityEntityTD[1].Text;
                                Net_Asse = CityEntityTD[2].Text;
                                Exemp = CityEntityTD[3].Text;
                                Txn_Val = CityEntityTD[4].Text;
                                Mil_rt = CityEntityTD[5].Text;
                                Grs = CityEntityTD[6].Text;
                                Credit = CityEntityTD[7].Text;
                                CNet_Tax = CityEntityTD[8].Text;

                                CityEntity_details = City_Entity + "~" + Adj_FMV + "~" + Net_Asse + "~" + Exemp + "~" + Txn_Val + "~" + Mil_rt + "~" + Grs + "~" + Credit + "~" + CNet_Tax;
                                gc.insert_date(orderNumber, ParcelID, 739, CityEntity_details, 1, DateTime.Now);
                            }
                        }

                        IWebElement CityEntityTB1 = driver.FindElement(By.XPath("//*[@id='tbl_tax_positions']/tbody"));
                        IList<IWebElement> CityEntityTR1 = CityEntityTB1.FindElements(By.TagName("tr"));
                        IList<IWebElement> CityEntityTD1;

                        foreach (IWebElement CityEntity1 in CityEntityTR1)
                        {
                            CityEntityTD1 = CityEntity1.FindElements(By.TagName("td"));
                            if (CityEntityTD1.Count != 0 && CityEntity1.Text.Contains("TOTALS"))
                            {
                                Txn_Val = CityEntityTD1[0].Text;
                                Mil_rt = CityEntityTD1[1].Text;
                                Grs = CityEntityTD1[2].Text;
                                Credit = CityEntityTD1[3].Text;
                                CNet_Tax = CityEntityTD1[4].Text;

                                CityEntity_details = "" + "~" + "" + "~" + "" + "~" + "" + "~" + Txn_Val + "~" + Mil_rt + "~" + Grs + "~" + Credit + "~" + CNet_Tax;
                                gc.insert_date(orderNumber, ParcelID, 739, CityEntity_details, 1, DateTime.Now);
                            }
                        }
                    }
                    catch
                    { }

                    CitytaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "GA", "Hall", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "GA", "Hall");
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