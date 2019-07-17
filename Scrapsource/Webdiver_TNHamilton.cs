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
    public class Webdiver_TNHamilton
    {
        string Parcelno = "", Location = "", Owner = "", MultiAddress_details = "", MultiOwner_details = "";
        string Property_Address = "", Account_No = "", Parcel_ID = "", Pro_Type = "", Land_Use = "", Dist = "", Owner1 = "", Owner2 = "", Owners = "", Mailing1 = "", Mailing2 = "", Mailing3 = "", Mailing4 = "", Mailing_Address = "", Year_Built = "", Legal_Desp = "", property = "";
        string Building_Value = "", Xtra_Fetures_Value = "", Land_Value = "", Total_Value = "", Assessed_Value = "", Build = "", Xtra = "", Land = "", tol = "", Assed = "", Assessment = "";
        string Map = "", Group = "", Parcel = "", County_Taxyear = "", Bill = "", Bill_Type = "", CPro_Type = "", Own_name = "", Tol_Due = "", Status = "", CountyPayment_details = "";
        string Flags = "", Tax_Distrct = "", Bil_type = "", TaxBil_type = "", Bil_Status = "", Bill_Hash = "", Tax_Assessment = "", Billing_Date = "", Trns_type = "", Fee_Type = "", Bill_Amount = "", PayIfo_Date = "", PayIfoTrns_type = "", PayIfoFee_Type = "", PayIfo_Amount = "", t_du = "", Total_taxDue = "", Tax_Authority = "", Tax_Deatils = "";
        string City_map = "", City_Taxyear = "", City_Bill = "", CityBill_Type = "", CityPro_Type = "", CityOwn_name = "", CityTol_Due = "", CityStatus = "", CityPayment_details = "";
        string City_Flags = "", City_BillHash = "", City_Bil_type = "", City_TaxBil_Year = "", City_Bil_Status = "", City_Assessment = "", City_PaidDate = "", City_PaidAmount = "", City_TotalDue = "", CityTax_Authority = "", CityTax_Deatils = "", Cinfo_Taxyear = "", Cinfo_Transtype = "", Cinfo_Feetype = "", Cinfo_Amount = "", Cinfo_details = "";
        string ints = "", Deliq_Interest = "", Court = "", Deliq_CourtCost = "", Att = "", Deliq_Attornys = "", Clerk = "", Deliq_Clerk = "", TR = "", Deliq_TR = "", t_Inst = "", Tax_Instr = "", t_Water = "", Tax_Water = "", tauth_Inst = "", tauth_Inst1 = "", CityTax_Authority1 = "";

        IWebDriver driver;
        DBconnection db = new DBconnection();
        GlobalClass gc = new GlobalClass();
        MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ToString());

        public string FTP_TNHamilton(string houseno, string sname, string sttype, string accno, string parcelNumber, string searchType, string orderNumber, string ownername, string directParcel, string unitno)
        {
            GlobalClass.global_orderNo = orderNumber;
            HttpContext.Current.Session["orderNo"] = orderNumber;
            GlobalClass.global_parcelNo = parcelNumber;

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
                        string address = houseno + " " + sname + " " + unitno;
                        gc.TitleFlexSearch(orderNumber, parcelNumber, "", address, "TN", "Hamilton");
                        if ((HttpContext.Current.Session["TitleFlex_Search"] != null && HttpContext.Current.Session["TitleFlex_Search"].ToString() == "Yes"))
                        {
                            driver.Quit();
                            return "MultiParcel";
                        }
                        else if (HttpContext.Current.Session["titleparcel"].ToString() == "")
                        {
                            HttpContext.Current.Session["Nodata_TNHamilton"] = "Yes";
                            driver.Quit();
                            return "No Data Found";
                        }
                        parcelNumber = HttpContext.Current.Session["titleparcel"].ToString();
                        searchType = "parcel";
                    }

                    if (searchType == "address")
                    {
                        driver.Navigate().GoToUrl("http://assessor.hamiltontn.gov/search.aspx");
                        Thread.Sleep(2000);

                        driver.FindElement(By.Id("SearchStreetName")).SendKeys(sname);
                        driver.FindElement(By.Id("SearchStreetNumber")).SendKeys(houseno);

                        driver.FindElement(By.Id("cmdGo")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Address search", driver, "TN", "Hamilton");

                        IWebElement MultiAddressTB = driver.FindElement(By.XPath("//*[@id='T1']/tbody"));
                        IList<IWebElement> MultiAddressTR = MultiAddressTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> MultiAddressTD;

                        if (MultiAddressTR.Count == 1)
                        {
                            driver.FindElement(By.XPath("//*[@id='T1']/tbody/tr/td[1]/a")).Click();
                            Thread.Sleep(2000);
                        }

                        else
                        {
                            int AddressmaxCheck = 0;
                            foreach (IWebElement MultiAddress in MultiAddressTR)
                            {
                                if (AddressmaxCheck <= 25)
                                {
                                    MultiAddressTD = MultiAddress.FindElements(By.TagName("td"));
                                    if (MultiAddressTD.Count != 0)
                                    {
                                        Parcelno = MultiAddressTD[0].Text;
                                        Location = MultiAddressTD[1].Text;
                                        Owner = MultiAddressTD[2].Text;

                                        MultiAddress_details = Location + "~" + Owner;
                                        gc.insert_date(orderNumber, Parcelno, 764, MultiAddress_details, 1, DateTime.Now);
                                    }
                                    AddressmaxCheck++;
                                }
                            }
                            if (MultiAddressTR.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_TNHamilton_Multicount"] = "Maximum";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiparcel_TNHamilton"] = "Yes";
                            }
                            driver.Quit();

                            return "MultiParcel";
                        }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='body']/div[3]")).Text;
                            if (nodata.Contains("No results found!"))
                            {
                                HttpContext.Current.Session["Nodata_TNHamilton"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    if (searchType == "parcel")
                    {
                        driver.Navigate().GoToUrl("http://assessor.hamiltontn.gov/search.aspx");
                        Thread.Sleep(2000);

                        if (parcelNumber.Contains("-") || parcelNumber.Contains(" ") || parcelNumber.Contains("."))
                        {
                            parcelNumber = parcelNumber.Replace("-", "").Replace(" ", "").Replace(".", "");
                        }

                        string CommonParcel = parcelNumber;

                        if (CommonParcel.Length == 8)
                        {
                            string a = parcelNumber.Substring(0, 4);
                            string b = parcelNumber.Substring(4, 1);
                            string c = parcelNumber.Substring(5, 3);

                            parcelNumber = a + " " + b + " " + c;
                        }

                        if (CommonParcel.Length == 6)
                        {
                            string g = parcelNumber.Substring(0, 3);
                            string h = parcelNumber.Substring(3, 3);

                            parcelNumber = g + " " + h;
                        }

                        if (CommonParcel.Length == 13)
                        {
                            string d = parcelNumber.Substring(0, 4);
                            string e = parcelNumber.Substring(5, 1);
                            string f = parcelNumber.Substring(6, 3);

                            parcelNumber = d + " " + e + " " + f;
                        }

                        driver.FindElement(By.Id("SearchParcel")).SendKeys(parcelNumber);

                        driver.FindElement(By.Id("cmdGo")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf(orderNumber, parcelNumber, "ParcelSearch", driver, "TN", "Hamilton");

                        driver.FindElement(By.XPath("//*[@id='T1']/tbody/tr/td[1]/a")).Click();
                        Thread.Sleep(2000);
                    }

                    if (searchType == "ownername")
                    {
                        driver.Navigate().GoToUrl("http://assessor.hamiltontn.gov/search.aspx");
                        Thread.Sleep(2000);

                        driver.FindElement(By.Id("SearchOwner")).SendKeys(ownername);

                        driver.FindElement(By.Id("cmdGo")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);
                        gc.CreatePdf_WOP(orderNumber, "Owner search", driver, "TN", "Hamilton");

                        IWebElement MultiOwnerTB = driver.FindElement(By.XPath("//*[@id='T1']/tbody"));
                        IList<IWebElement> MultiOwnerTR = MultiOwnerTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> MultiOwnerTD;

                        if (MultiOwnerTR.Count == 1)
                        {
                            driver.FindElement(By.XPath("//*[@id='T1']/tbody/tr/td[1]/a")).Click();
                            Thread.Sleep(2000);
                        }

                        else
                        {
                            int AddressmaxCheck = 0;
                            foreach (IWebElement MultiOwner in MultiOwnerTR)
                            {
                                if (AddressmaxCheck <= 25)
                                {
                                    MultiOwnerTD = MultiOwner.FindElements(By.TagName("td"));
                                    if (MultiOwnerTD.Count != 0)
                                    {
                                        Parcelno = MultiOwnerTD[0].Text;
                                        Location = MultiOwnerTD[1].Text;
                                        Owner = MultiOwnerTD[2].Text;

                                        MultiOwner_details = Location + "~" + Owner;
                                        gc.insert_date(orderNumber, Parcelno, 764, MultiOwner_details, 1, DateTime.Now);
                                    }
                                    AddressmaxCheck++;
                                }
                            }
                            if (MultiOwnerTR.Count > 25)
                            {
                                HttpContext.Current.Session["multiParcel_TNHamilton_Multicount"] = "Maximum";
                            }
                            else
                            {
                                HttpContext.Current.Session["multiparcel_TNHamilton"] = "Yes";
                            }
                            driver.Quit();
                            return "MultiParcel";
                        }
                        try
                        {
                            //No Data Found
                            string nodata = driver.FindElement(By.XPath("//*[@id='body']/div[3]")).Text;
                            if (nodata.Contains("No results found!"))
                            {
                                HttpContext.Current.Session["Nodata_TNHamilton"] = "Yes";
                                driver.Quit();
                                return "No Data Found";
                            }
                        }
                        catch { }
                    }

                    //Property Details
                    try
                    {
                        Property_Address = driver.FindElement(By.XPath("//*[@id='body']/form/div[2]/table[1]/tbody/tr[1]/td[1]/div/div")).Text;
                        Account_No = driver.FindElement(By.XPath("//*[@id='body']/form/div[2]/table[1]/tbody/tr[1]/td[2]/div/div")).Text;
                        Parcel_ID = driver.FindElement(By.XPath("//*[@id='body']/form/div[2]/table[1]/tbody/tr[1]/td[3]/div/div")).Text;
                        Pro_Type = driver.FindElement(By.XPath("//*[@id='body']/form/div[2]/table[1]/tbody/tr[2]/td[1]/div/div")).Text;
                        Land_Use = driver.FindElement(By.XPath("//*[@id='body']/form/div[2]/table[1]/tbody/tr[2]/td[2]/div/div")).Text;
                        Dist = driver.FindElement(By.XPath("//*[@id='body']/form/div[2]/table[1]/tbody/tr[2]/td[3]/div/div")).Text;
                        Owner1 = driver.FindElement(By.XPath("//*[@id='body']/form/div[2]/table[2]/tbody/tr[2]/td/div/table/tbody/tr/td/table/tbody/tr[1]/td[2]/div")).Text;
                        Owner2 = driver.FindElement(By.XPath("//*[@id='body']/form/div[2]/table[2]/tbody/tr[2]/td/div/table/tbody/tr/td/table/tbody/tr[2]/td[2]/div")).Text;
                        Owners = Owner1 + " &" + Owner2;
                        Mailing1 = driver.FindElement(By.XPath("//*[@id='body']/form/div[2]/table[2]/tbody/tr[2]/td/div/table/tbody/tr/td/table/tbody/tr[3]/td[2]/div")).Text;
                        Mailing2 = driver.FindElement(By.XPath("//*[@id='body']/form/div[2]/table[2]/tbody/tr[2]/td/div/table/tbody/tr/td/table/tbody/tr[1]/td[4]/div")).Text;
                        Mailing3 = driver.FindElement(By.XPath("//*[@id='body']/form/div[2]/table[2]/tbody/tr[2]/td/div/table/tbody/tr/td/table/tbody/tr[2]/td[4]/div")).Text;
                        Mailing4 = driver.FindElement(By.XPath("//*[@id='body']/form/div[2]/table[2]/tbody/tr[2]/td/div/table/tbody/tr/td/table/tbody/tr[3]/td[4]/div")).Text;
                        Mailing_Address = Mailing1 + "," + Mailing2 + "," + Mailing3 + " " + Mailing4;
                        Year_Built = driver.FindElement(By.XPath("//*[@id='body']/form/div[2]/table[2]/tbody/tr[4]/td/div[2]/center/table/tbody/tr[2]/td/div/table/tbody/tr/td/div")).Text;
                        Year_Built = WebDriverTest.Between(Year_Built, "about ", " with");
                        Legal_Desp = driver.FindElement(By.XPath("//*[@id='body']/form/div[2]/table[2]/tbody/tr[4]/td/div[2]/center/table/tbody/tr[2]/td/font[2]/div[1]/center/table[2]/tbody/tr[2]/td/div/table/tbody/tr/td/div")).Text;

                        property = Account_No + "~" + Property_Address + "~" + Pro_Type + "~" + Land_Use + "~" + Dist + "~" + Owners + "~" + Mailing_Address + "~" + Year_Built + "~" + Legal_Desp;
                        gc.CreatePdf(orderNumber, Parcel_ID, "Property Details", driver, "TN", "Hamilton");
                        gc.insert_date(orderNumber, Parcel_ID, 766, property, 1, DateTime.Now);
                    }
                    catch
                    { }

                    //Assessment Deatils
                    try
                    {
                        IWebElement tbmulti11 = driver.FindElement(By.XPath("//*[@id='body']/form/div[2]/table[2]/tbody/tr[4]/td/div[1]/center/table/tbody/tr[2]/td/div/table/tbody/tr/td/table/tbody"));
                        IList<IWebElement> TRmulti11 = tbmulti11.FindElements(By.TagName("tr"));
                        IList<IWebElement> TDmulti11;

                        foreach (IWebElement row in TRmulti11)
                        {
                            TDmulti11 = row.FindElements(By.TagName("td"));
                            if (TDmulti11.Count != 0)
                            {
                                Build = TDmulti11[0].Text;
                                if (Build.Contains("Building Value"))
                                {
                                    Building_Value = TDmulti11[1].Text;
                                }

                                Xtra = TDmulti11[0].Text;
                                if (Xtra.Contains("Xtra Features Value"))
                                {
                                    Xtra_Fetures_Value = TDmulti11[1].Text;
                                }

                                Land = TDmulti11[0].Text;
                                if (Land.Contains("Land Value"))
                                {
                                    Land_Value = TDmulti11[1].Text;
                                }

                                tol = TDmulti11[0].Text;
                                if (tol.Contains("Total Value"))
                                {
                                    Total_Value = TDmulti11[1].Text;
                                }

                                Assed = TDmulti11[0].Text;
                                if (Assed.Contains("Assessed Value"))
                                {
                                    Assessed_Value = TDmulti11[1].Text;
                                }
                            }
                        }

                        Assessment = Building_Value + "~" + Xtra_Fetures_Value + "~" + Land_Value + "~" + Total_Value + "~" + Assessed_Value;
                        gc.insert_date(orderNumber, Parcel_ID, 767, Assessment, 1, DateTime.Now);
                    }
                    catch
                    { }
           

                    //Tax Details
                    driver.Navigate().GoToUrl("http://tpti.hamiltontn.gov/AppFolder/Trustee_PropertySearch.aspx");
                    Thread.Sleep(2000);

                    driver.FindElement(By.Id("ctl00_MainContent_btnMGP")).Click();
                    Thread.Sleep(2000);


                    Parcel_ID = Parcel_ID.Replace("_", "");

                    if (Parcel_ID.Contains("."))
                    {
                        Parcel_ID = GlobalClass.Before(Parcel_ID, ".").Trim();
                    }
                    string com_Parcel = Parcel_ID;
                    if (com_Parcel.Length == 7)
                    {
                        Map = Parcel_ID.Substring(0, 4);
                        Parcel = Parcel_ID.Substring(4, 3);
                    }
                    if (com_Parcel.Length == 8)
                    {
                        Map = Parcel_ID.Substring(0, 4);
                        Group = Parcel_ID.Substring(4, 1);
                        Parcel = Parcel_ID.Substring(5, 3);
                    }
                    if (com_Parcel.Length == 9)
                    {
                        Map = Parcel_ID.Substring(0, 5);
                        Group = Parcel_ID.Substring(5, 1);
                        Parcel = Parcel_ID.Substring(6, 3);
                    }

                    driver.FindElement(By.Id("ctl00_MainContent_txtMap")).SendKeys(Map);
                    driver.FindElement(By.Id("ctl00_MainContent_txtGroup")).SendKeys(Group);
                    driver.FindElement(By.Id("ctl00_MainContent_txtParcel")).SendKeys(Parcel);

                    gc.CreatePdf(orderNumber, Parcel_ID, "Tax Parcel search", driver, "TN", "Hamilton");
                    driver.FindElement(By.Id("ctl00_MainContent_cmdMGP_Search")).SendKeys(Keys.Enter);

                    //County Tax History Details
                    try
                    {
                        IWebElement CountyTaxPaymentTB = driver.FindElement(By.XPath("//*[@id='ctl00_MainContent_dgrResults']/tbody"));
                        IList<IWebElement> CountyTaxPaymentTR = CountyTaxPaymentTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> CountyTaxPaymentTD;

                        foreach (IWebElement CountyTaxPayment in CountyTaxPaymentTR)
                        {
                            CountyTaxPaymentTD = CountyTaxPayment.FindElements(By.TagName("td"));
                            if (CountyTaxPaymentTD.Count != 0 && !CountyTaxPayment.Text.Contains("Year"))
                            {
                                County_Taxyear = CountyTaxPaymentTD[1].Text;
                                Bill = CountyTaxPaymentTD[2].Text;
                                Bill_Type = CountyTaxPaymentTD[4].Text;
                                CPro_Type = CountyTaxPaymentTD[5].Text;
                                Own_name = CountyTaxPaymentTD[6].Text;
                                Tol_Due = CountyTaxPaymentTD[7].Text;
                                Status = CountyTaxPaymentTD[8].Text;

                                CountyPayment_details = County_Taxyear + "~" + Bill + "~" + Bill_Type + "~" + CPro_Type + "~" + Own_name + "~" + Tol_Due + "~" + Status;
                                gc.CreatePdf(orderNumber, Parcel_ID, "County Tax History Details", driver, "TN", "Hamilton");
                                gc.insert_date(orderNumber, Parcel_ID, 768, CountyPayment_details, 1, DateTime.Now);
                            }
                        }
                    }
                    catch
                    { }

                    //County Tax Info Details
                    List<string> CountyInfoSearch = new List<string>();

                    try
                    {
                        IWebElement CityInfoTB = driver.FindElement(By.XPath("//*[@id='ctl00_MainContent_dgrResults']/tbody"));
                        IList<IWebElement> CityInfoTR = CityInfoTB.FindElements(By.TagName("tr"));
                        IList<IWebElement> CityInfoTD;

                        int i = 0;
                        foreach (IWebElement CityInfo in CityInfoTR)
                        {
                            if (!CityInfo.Text.Contains("Year"))
                            {
                                if (i == 1 || i == 2 || i == 3)
                                {
                                    CityInfoTD = CityInfo.FindElements(By.TagName("td"));
                                    IWebElement CityInfo_link = CityInfoTD[0].FindElement(By.TagName("a"));
                                    string CityInfourl = CityInfo_link.GetAttribute("href");
                                    CountyInfoSearch.Add(CityInfourl);
                                }
                            }
                            i++;
                        }

                        foreach (string CityInfobill in CountyInfoSearch)
                        {
                            driver.Navigate().GoToUrl(CityInfobill);
                            Thread.Sleep(5000);

                            driver.SwitchTo().Window(driver.WindowHandles.Last());
                            Thread.Sleep(3000);

                            //Tax Info
                            Flags = driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[3]/table/tbody/tr[2]/td[2]/table/tbody/tr[5]/td/table/tbody/tr[1]/td[4]")).Text;
                            Tax_Distrct = driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[3]/table/tbody/tr[2]/td[2]/table/tbody/tr[5]/td/table/tbody/tr[2]/td[2]")).Text;
                            Bil_type = driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[3]/table/tbody/tr[2]/td[2]/table/tbody/tr[7]/td/table/tbody/tr[1]/td[2]")).Text;
                            TaxBil_type = driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[3]/table/tbody/tr[2]/td[2]/table/tbody/tr[7]/td/table/tbody/tr[1]/td[4]")).Text;
                            Bil_Status = driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[3]/table/tbody/tr[2]/td[2]/table/tbody/tr[7]/td/table/tbody/tr[2]/td[2]")).Text;
                            Bill_Hash = driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[3]/table/tbody/tr[2]/td[2]/table/tbody/tr[7]/td/table/tbody/tr[2]/td[4]")).Text;
                            Tax_Assessment = driver.FindElement(By.XPath("//*[@id='aspnetForm']/div[3]/table/tbody/tr[2]/td[2]/table/tbody/tr[7]/td/table/tbody/tr[5]/td[2]")).Text;

                            IWebElement Tax3TB = driver.FindElement(By.XPath("//*[@id='ctl00_MainContent_dgrTrans']/tbody"));
                            IList<IWebElement> Tax3TR = Tax3TB.FindElements(By.TagName("tr"));
                            IList<IWebElement> Tax3TD;
                            foreach (IWebElement Tax3 in Tax3TR)
                            {
                                Tax3TD = Tax3.FindElements(By.TagName("td"));
                                if (Tax3TD.Count != 0 && !Tax3.Text.Contains("Date"))
                                {
                                    Billing_Date = Tax3TD[0].Text;
                                    Trns_type = Tax3TD[1].Text;
                                    Fee_Type = Tax3TD[2].Text;
                                    Bill_Amount = Tax3TD[4].Text;
                                }
                            }

                            try
                            {
                                IWebElement Tax4TB = driver.FindElement(By.XPath("//*[@id='ctl00_MainContent_dgrPayments']/tbody"));
                                IList<IWebElement> Tax4TR = Tax4TB.FindElements(By.TagName("tr"));
                                IList<IWebElement> Tax4TD;
                                foreach (IWebElement Tax4 in Tax4TR)
                                {
                                    Tax4TD = Tax4.FindElements(By.TagName("td"));
                                    if (Tax4TD.Count != 0 && !Tax4.Text.Contains("Date Paid"))
                                    {
                                        PayIfo_Date = Tax4TD[0].Text;
                                        PayIfoTrns_type = Tax4TD[1].Text;
                                        PayIfoFee_Type = Tax4TD[2].Text;
                                        PayIfo_Amount = Tax4TD[3].Text;
                                    }
                                }
                            }
                            catch
                            { }

                            //Deliquent Details
                            try
                            {
                                IWebElement IntrestTB = driver.FindElement(By.XPath("//*[@id='ctl00_MainContent_lblIntAndFees']/table/tbody"));
                                IList<IWebElement> IntrestTR = IntrestTB.FindElements(By.TagName("tr"));
                                IList<IWebElement> IntrestTD;
                                foreach (IWebElement Intrest in IntrestTR)
                                {
                                    IntrestTD = Intrest.FindElements(By.TagName("td"));
                                    if (IntrestTD.Count != 0)
                                    {
                                        ints = IntrestTD[0].Text;
                                        if (ints.Contains("Interest:"))
                                        {
                                            Deliq_Interest = IntrestTD[1].Text;
                                        }

                                        Court = IntrestTD[0].Text;
                                        if (Court.Contains("Court Cost:"))
                                        {
                                            Deliq_CourtCost = IntrestTD[1].Text;
                                        }

                                        Att = IntrestTD[0].Text;
                                        if (Att.Contains("Attorney's Fee:"))
                                        {
                                            Deliq_Attornys = IntrestTD[1].Text;
                                        }

                                        Clerk = IntrestTD[0].Text;
                                        if (Clerk.Contains("Clerk Commission:"))
                                        {
                                            Deliq_Clerk = IntrestTD[1].Text;
                                        }

                                        TR = IntrestTD[0].Text;
                                        if (TR.Contains("TR Costs:"))
                                        {
                                            Deliq_TR = IntrestTD[1].Text;
                                        }
                                    }
                                }
                            }
                            catch
                            { }

                            IWebElement Tax2TB = driver.FindElement(By.XPath("//*[@id='ctl00_MainContent_pnlTotalDue']/table[1]/tbody/tr[1]/td/table/tbody"));
                            IList<IWebElement> Tax2TR = Tax2TB.FindElements(By.TagName("tr"));
                            IList<IWebElement> Tax2TD;

                            foreach (IWebElement Tax2 in Tax2TR)
                            {
                                Tax2TD = Tax2.FindElements(By.TagName("td"));
                                if (Tax2TD.Count != 0)
                                {
                                    t_du = Tax2TD[0].Text;
                                    if (t_du.Contains(" Total Due  "))
                                    {
                                        Total_taxDue = Tax2TD[1].Text;
                                    }
                                }
                            }
                            gc.CreatePdf(orderNumber, Parcel_ID, "Tax Details" + TaxBil_type, driver, "TN", "Hamilton");

                            try
                            {
                                Tax_Authority = driver.FindElement(By.XPath("//*[@id='ctl00_MainContent_pnlOurAddr']/table/tbody/tr/td/b")).Text;
                            }
                            catch
                            { }

                            try
                            {
                                Tax_Authority = driver.FindElement(By.XPath("//*[@id='ctl00_MainContent_pnlDelQ']/table/tbody/tr/td/b")).Text;
                            }
                            catch
                            { }

                            Tax_Deatils = Tax_Distrct + "~" + TaxBil_type + "~" + Flags + "~" + Bil_type + "~" + Bil_Status + "~" + Bill_Hash + "~" + Tax_Assessment + "~" + Billing_Date + "~" + Trns_type + "~" + Fee_Type + "~" + Bill_Amount + "~" + PayIfo_Date + "~" + PayIfoTrns_type + "~" + PayIfoFee_Type + "~" + PayIfo_Amount + "~" + Deliq_Interest + "~" + Deliq_CourtCost + "~" + Deliq_Attornys + "~" + Deliq_Clerk + "~" + Deliq_TR + "~" + Total_taxDue + "~" + Tax_Authority;
                            gc.insert_date(orderNumber, Parcel_ID, 769, Tax_Deatils, 1, DateTime.Now);
                            PayIfo_Date = ""; PayIfoTrns_type = ""; PayIfoFee_Type = ""; PayIfo_Amount = ""; Deliq_Interest = ""; Deliq_CourtCost = ""; Deliq_Attornys = ""; Deliq_Clerk = ""; Deliq_TR = ""; Total_taxDue = ""; Tax_Authority = "";
                        }
                    }
                    catch { }

                    TaxTime = DateTime.Now.ToString("HH:mm:ss");

                    if (Tax_Distrct == "Chattanooga (1)")
                    {
                        driver.Navigate().GoToUrl("http://propertytax.chattanooga.gov/component/ptaxweb/searchpage?field=mgp");
                        Thread.Sleep(2000);

                        City_map = Map.Replace(" ", "");

                        driver.FindElement(By.Id("filter_search_map")).SendKeys(City_map);
                        driver.FindElement(By.Id("filter_search_group")).SendKeys(Group);
                        driver.FindElement(By.Id("filter_search_parcel")).SendKeys(Parcel);

                        gc.CreatePdf(orderNumber, Parcel_ID, "City Parcel search", driver, "TN", "Hamilton");
                        driver.FindElement(By.XPath("//*[@id='content']/div[1]/form/div/div[2]/button")).SendKeys(Keys.Enter);
                        Thread.Sleep(2000);

                        driver.FindElement(By.XPath("//*[@id=\'content']/div/div[1]/form/table/tbody/tr/td[1]/a")).Click();
                        Thread.Sleep(2000);

                        //City Tax History Details
                        try
                        {
                            IWebElement CityTaxPaymentTB = driver.FindElement(By.XPath("//*[@id='content']/div/div[1]/form/table[2]/tbody"));
                            IList<IWebElement> CityTaxPaymentTR = CityTaxPaymentTB.FindElements(By.TagName("tr"));
                            IList<IWebElement> CityTaxPaymentTD;

                            foreach (IWebElement CityTaxPayment in CityTaxPaymentTR)
                            {
                                CityTaxPaymentTD = CityTaxPayment.FindElements(By.TagName("td"));
                                if (CityTaxPaymentTD.Count != 0 && !CityTaxPayment.Text.Contains("Year"))
                                {
                                    City_Taxyear = CityTaxPaymentTD[1].Text;
                                    City_Bill = CityTaxPaymentTD[2].Text;
                                    CityBill_Type = CityTaxPaymentTD[3].Text;
                                    CityPro_Type = CityTaxPaymentTD[4].Text;
                                    CityOwn_name = CityTaxPaymentTD[5].Text;
                                    CityTol_Due = CityTaxPaymentTD[6].Text;
                                    CityStatus = CityTaxPaymentTD[7].Text;

                                    CityPayment_details = City_Taxyear + "~" + City_Bill + "~" + CityBill_Type + "~" + CityPro_Type + "~" + CityOwn_name + "~" + CityTol_Due + "~" + CityStatus;
                                    gc.CreatePdf(orderNumber, Parcel_ID, "CIty Tax History Details", driver, "TN", "Hamilton");
                                    gc.insert_date(orderNumber, Parcel_ID, 770, CityPayment_details, 1, DateTime.Now);
                                }
                            }
                        }
                        catch
                        { }

                        //City Info Details
                        List<string> CityInfoSearch = new List<string>();

                        try
                        {
                            IWebElement CityInfo1TB = driver.FindElement(By.XPath("//*[@id='content']/div/div[1]/form/table[2]/tbody"));
                            IList<IWebElement> CityInfo1TR = CityInfo1TB.FindElements(By.TagName("tr"));
                            IList<IWebElement> CityInfo1TD;

                            int i = 0;
                            foreach (IWebElement CityInfo1 in CityInfo1TR)
                            {
                                if (i == 0 || i == 1 || i == 2)
                                {
                                    CityInfo1TD = CityInfo1.FindElements(By.TagName("td"));
                                    IWebElement CityInfo1_link = CityInfo1TD[0].FindElement(By.TagName("a"));
                                    string CityInfo1url = CityInfo1_link.GetAttribute("href");
                                    CityInfoSearch.Add(CityInfo1url);
                                }
                                i++;
                            }

                            foreach (string CityInfobill1 in CityInfoSearch)
                            {
                                driver.Navigate().GoToUrl(CityInfobill1);
                                Thread.Sleep(5000);

                                driver.SwitchTo().Window(driver.WindowHandles.Last());
                                Thread.Sleep(3000);

                                City_Flags = driver.FindElement(By.XPath("//*[@id='ptaxweb-content']/table[1]/tbody/tr[1]/td[4]")).Text;
                                City_BillHash = driver.FindElement(By.XPath("//*[@id='ptaxweb-content']/table[2]/tbody/tr/td[2]")).Text;
                                City_Bil_type = driver.FindElement(By.XPath("//*[@id='ptaxweb-content']/table[3]/tbody/tr[1]/td[2]")).Text;
                                City_TaxBil_Year = driver.FindElement(By.XPath("//*[@id='ptaxweb-content']/table[3]/tbody/tr[1]/td[4]")).Text;
                                City_Bil_Status = driver.FindElement(By.XPath("//*[@id='ptaxweb-content']/table[3]/tbody/tr[2]/td[2]")).Text;
                                City_Assessment = driver.FindElement(By.XPath("//*[@id='ptaxweb-content']/table[3]/tbody/tr[4]/td[4]")).Text;

                                try
                                {
                                    IWebElement Tax211TB = driver.FindElement(By.XPath("//*[@id='ptaxweb-content']/table[5]/tbody[2]"));
                                    IList<IWebElement> Tax211TR = Tax211TB.FindElements(By.TagName("tr"));
                                    IList<IWebElement> Tax211TD;
                                    foreach (IWebElement Tax211 in Tax211TR)
                                    {
                                        Tax211TD = Tax211.FindElements(By.TagName("td"));
                                        if (Tax211TD.Count != 0)
                                        {
                                            t_Inst = Tax211TD[0].Text;
                                            if (t_Inst.Contains("Taxes & Interest"))
                                            {
                                                Tax_Instr = Tax211TD[1].Text;
                                            }
                                            t_Water = Tax211TD[0].Text;
                                            if (t_Water.Contains("Water Quality Fee & Interest"))
                                            {
                                                Tax_Water = Tax211TD[1].Text;
                                            }
                                        }
                                    }
                                }
                                catch
                                { }

                                try
                                {
                                    IWebElement Tax212TB = driver.FindElement(By.XPath("//*[@id='ptaxweb-content']/table[5]/tbody[2]"));
                                    IList<IWebElement> Tax212TR = Tax212TB.FindElements(By.TagName("tr"));
                                    IList<IWebElement> Tax212TD;
                                    foreach (IWebElement Tax212 in Tax212TR)
                                    {
                                        Tax212TD = Tax212.FindElements(By.TagName("td"));
                                        if (Tax212TD.Count != 0)
                                        {
                                            City_PaidDate = Tax212TD[0].Text;
                                            City_PaidAmount = Tax212TD[1].Text;
                                        }
                                    }
                                }
                                catch
                                { }

                                try
                                {
                                    IWebElement Tax21TB = driver.FindElement(By.XPath("//*[@id='ptaxweb-content']/table[6]/tbody[2]"));
                                    IList<IWebElement> Tax21TR = Tax21TB.FindElements(By.TagName("tr"));
                                    IList<IWebElement> Tax21TD;
                                    foreach (IWebElement Tax21 in Tax21TR)
                                    {
                                        Tax21TD = Tax21.FindElements(By.TagName("td"));
                                        if (Tax21TD.Count != 0)
                                        {
                                            City_PaidDate = Tax21TD[0].Text;
                                            City_PaidAmount = Tax21TD[1].Text;
                                        }
                                    }
                                }
                                catch
                                { }

                                try
                                {
                                    IWebElement Tax311TB = driver.FindElement(By.XPath("//*[@id='ptaxweb-content']/table[7]/tbody"));
                                    IList<IWebElement> Tax311TR = Tax311TB.FindElements(By.TagName("tr"));
                                    IList<IWebElement> Tax311TD;
                                    foreach (IWebElement Tax311 in Tax311TR)
                                    {
                                        Tax311TD = Tax311.FindElements(By.TagName("td"));
                                        if (Tax311TD.Count != 0)
                                        {
                                            tauth_Inst1 = Tax311TD[0].Text;
                                            if (tauth_Inst.Contains("Total Due"))
                                            {
                                                City_TotalDue = Tax311TD[1].Text;
                                            }
                                        }
                                    }
                                }
                                catch
                                { }

                                try
                                {
                                    IWebElement Tax311TB = driver.FindElement(By.XPath("//*[@id='ptaxweb-content']/table[8]/tbody"));
                                    IList<IWebElement> Tax311TR = Tax311TB.FindElements(By.TagName("tr"));
                                    IList<IWebElement> Tax311TD;
                                    foreach (IWebElement Tax311 in Tax311TR)
                                    {
                                        Tax311TD = Tax311.FindElements(By.TagName("td"));
                                        if (Tax311TD.Count != 0)
                                        {
                                            tauth_Inst = Tax311TD[0].Text;
                                            if (tauth_Inst.Contains("Total Due"))
                                            {
                                                City_TotalDue = Tax311TD[1].Text;
                                            }
                                        }
                                    }
                                }
                                catch
                                { }

                                try
                                {
                                    CityTax_Authority = driver.FindElement(By.XPath("//*[@id='ptaxweb-content']/table[6]/tbody/tr/td")).Text;
                                    CityTax_Authority = WebDriverTest.After(CityTax_Authority, "MAKE CHECKS PAYABLE AND MAIL TO:");
                                }
                                catch
                                { }

                                try
                                {
                                    CityTax_Authority1 = driver.FindElement(By.XPath("//*[@id='ptaxweb-content']/table[7]/tbody/tr/td")).Text;
                                    CityTax_Authority1 = WebDriverTest.After(CityTax_Authority1, "MAKE CHECKS PAYABLE AND MAIL TO:");
                                }
                                catch
                                { }

                                CityTax_Deatils = City_Flags + "~" + City_BillHash + "~" + City_Bil_type + "~" + City_TaxBil_Year + "~" + City_Bil_Status + "~" + City_Assessment + "~" + City_PaidDate + "~" + City_PaidAmount + "~" + Tax_Instr + "~" + Tax_Water + "~" + City_TotalDue + "~" + CityTax_Authority + "~" + CityTax_Authority1;
                                gc.CreatePdf(orderNumber, Parcel_ID, "City Tax Details" + City_TaxBil_Year, driver, "TN", "Hamilton");
                                gc.insert_date(orderNumber, Parcel_ID, 771, CityTax_Deatils, 1, DateTime.Now);
                                City_PaidDate = ""; City_PaidAmount = ""; Tax_Instr = ""; Tax_Water = ""; City_TotalDue = "";

                                try
                                {
                                    IWebElement CinfoTB = driver.FindElement(By.XPath("//*[@id='ptaxweb-content']/table[4]/tbody[2]"));
                                    IList<IWebElement> CinfoTR = CinfoTB.FindElements(By.TagName("tr"));
                                    IList<IWebElement> CinfoTD;
                                    foreach (IWebElement Cinfo in CinfoTR)
                                    {
                                        CinfoTD = Cinfo.FindElements(By.TagName("td"));
                                        if (CinfoTD.Count != 0)
                                        {
                                            Cinfo_Taxyear = CinfoTD[0].Text;
                                            Cinfo_Transtype = CinfoTD[1].Text;
                                            Cinfo_Feetype = CinfoTD[2].Text;
                                            Cinfo_Amount = CinfoTD[3].Text;

                                            Cinfo_details = Cinfo_Taxyear + "~" + Cinfo_Transtype + "~" + Cinfo_Feetype + "~" + Cinfo_Amount;
                                            gc.insert_date(orderNumber, Parcel_ID, 772, Cinfo_details, 1, DateTime.Now);
                                        }
                                    }
                                }
                                catch
                                { }
                            }
                        }
                        catch { }
                    }
                    CitytaxTime = DateTime.Now.ToString("HH:mm:ss");

                    LastEndTime = DateTime.Now.ToString("HH:mm:ss");
                    gc.insert_TakenTime(orderNumber, "TN", "Hamilton", StartTime, AssessmentTime, TaxTime, CitytaxTime, LastEndTime);

                    driver.Quit();
                    gc.mergpdf(orderNumber, "TN", "Hamilton");
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